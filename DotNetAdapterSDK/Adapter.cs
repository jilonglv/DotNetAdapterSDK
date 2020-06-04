namespace MTConnect
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;
    using System.Xml;

    public class Adapter
    {
        private CountdownEvent mActiveClients = new CountdownEvent(1);
        private bool mBegun = false;
        private ArrayList mClients = new ArrayList();
        private ArrayList mDataItems = new ArrayList();
        private ASCIIEncoding mEncoder = new ASCIIEncoding();
        private int mHeartbeat;
        private TcpListener mListener;
        private Thread mListenThread;
        private int mPort;
        private bool mRunning = false;
        private byte[] PONG;

        public Adapter(int aPort = 0x1ec6, bool verbose = false)
        {
            this.mPort = aPort;
            this.Heartbeat = 0x2710;
            this.Verbose = verbose;
        }

        public void AddAsset(Asset asset)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            StringBuilder output = new StringBuilder();
            output.Append(DateTime.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:ss.fffffffK"));
            output.Append("|@ASSET@|");
            output.Append(asset.AssetId);
            output.Append('|');
            output.Append(asset.GetMTCType());
            output.Append("|--multiline--ABCD\n");
            XmlWriterSettings settings = new XmlWriterSettings {
                OmitXmlDeclaration = true
            };
            XmlWriter writer = XmlWriter.Create(output, settings);
            asset.ToXml(writer);
            writer.Close();
            output.Append("\n--multiline--ABCD\n");
            this.SendToAll(output.ToString());
        }

        public void addClientStream(Stream aStream)
        {
            this.mClients.Add(aStream);
            this.SendAllTo(aStream);
        }

        public void AddDataItem(DataItem aDI)
        {
            this.mDataItems.Add(aDI);
        }

        public void Begin()
        {
            this.mBegun = true;
            foreach (DataItem item in this.mDataItems)
            {
                item.Begin();
            }
        }

        public void FlushAll()
        {
            foreach (Stream stream in this.mClients)
            {
                stream.Flush();
            }
        }

        private void HeartbeatClient(object client)
        {
            Exception exception;
            this.mActiveClients.AddCount();
            TcpClient client2 = (TcpClient) client;
            NetworkStream stream = client2.GetStream();
            this.mClients.Add(stream);
            ArrayList checkRead = new ArrayList();
            bool flag = false;
            byte[] buffer = new byte[0x1000];
            ASCIIEncoding encoding = new ASCIIEncoding();
            int offset = 0;
            try
            {
                while (this.mRunning && client2.Connected)
                {
                    int num2 = 0;
                    try
                    {
                        checkRead.Clear();
                        checkRead.Add(client2.Client);
                        if ((this.mHeartbeat > 0) && flag)
                        {
                            Socket.Select(checkRead, null, null, this.mHeartbeat * 0x7d0);
                        }
                        if ((checkRead.Count == 0) && flag)
                        {
                            Console.WriteLine("Heartbeat timed out, closing connection\n");
                            return;
                        }
                        num2 = stream.Read(buffer, offset, 0x1000 - offset);
                    }
                    catch (Exception exception1)
                    {
                        exception = exception1;
                        Console.WriteLine("Heartbeat read exception: " + exception.Message + "\n");
                        return;
                    }
                    if (num2 == 0)
                    {
                        Console.WriteLine("No bytes were read from heartbeat thread");
                        return;
                    }
                    int num3 = offset;
                    offset += num2;
                    int index = 0;
                    for (int i = num3; i < offset; i++)
                    {
                        if (buffer[i] == 10)
                        {
                            string aLine = encoding.GetString(buffer, index, i);
                            if (this.Receive(stream, aLine))
                            {
                                flag = true;
                            }
                            index = i + 1;
                        }
                    }
                    if (index > 0)
                    {
                        offset -= index;
                        if (offset > 0)
                        {
                            Array.Copy(buffer, index, buffer, 0, offset);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                exception = exception2;
                Console.WriteLine("Error during heartbeat: " + exception.Message);
            }
            finally
            {
                try
                {
                    this.mClients.Remove(stream);
                    client2.Close();
                }
                catch (Exception exception3)
                {
                    exception = exception3;
                    Console.WriteLine("Error during heartbeat cleanup: " + exception.Message);
                }
                this.mActiveClients.Signal();
            }
        }

        private void ListenForClients()
        {
            this.mRunning = true;
            try
            {
                while (this.mRunning)
                {
                    TcpClient parameter = this.mListener.AcceptTcpClient();
                    new Thread(new ParameterizedThreadStart(this.HeartbeatClient)).Start(parameter);
                    this.SendAllTo(parameter.GetStream());
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine("Execption occurred waiting for connection: " + exception.Message);
            }
            finally
            {
                this.mRunning = false;
                this.mListener.Stop();
            }
        }

        private bool Receive(Stream aClient, string aLine)
        {
            bool flag = false;
            if (aLine.StartsWith("* PING") && (this.mHeartbeat > 0))
            {
                flag = true;
                lock (aClient)
                {
                    this.WriteToClient(aClient, this.PONG);
                    aClient.Flush();
                }
            }
            return flag;
        }

        public void RemoveAllDataItems()
        {
            this.mDataItems.Clear();
        }

        public void RemoveDataItem(DataItem aItem)
        {
            int index = this.mDataItems.IndexOf(aItem);
            if (index >= 0)
            {
                this.mDataItems.RemoveAt(index);
            }
        }

        public void SendAllTo(Stream aClient)
        {
            lock (aClient)
            {
                List<DataItem> list = new List<DataItem>();
                List<DataItem> list2 = new List<DataItem>();
                foreach (DataItem item in this.mDataItems)
                {
                    List<DataItem> collection = item.ItemList(true);
                    if (item.NewLine)
                    {
                        list2.AddRange(collection);
                    }
                    else
                    {
                        list.AddRange(collection);
                    }
                }
                string str = DateTime.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:ss.fffffffK");
                string str2 = str;
                foreach (DataItem item in list)
                {
                    str2 = str2 + "|" + item.ToString();
                }
                str2 = str2 + "\n";
                byte[] bytes = this.mEncoder.GetBytes(str2.ToCharArray());
                aClient.Write(bytes, 0, bytes.Length);
                foreach (DataItem item in list2)
                {
                    str2 = str;
                    bytes = this.mEncoder.GetBytes((str2 + "|" + item.ToString() + "\n").ToCharArray());
                    this.WriteToClient(aClient, bytes);
                }
                aClient.Flush();
            }
        }

        public void SendChanged(string timestamp = null)
        {
            string str;
            if (this.mBegun)
            {
                foreach (DataItem item in this.mDataItems)
                {
                    item.Prepare();
                }
            }
            List<DataItem> list = new List<DataItem>();
            List<DataItem> list2 = new List<DataItem>();
            foreach (DataItem item in this.mDataItems)
            {
                List<DataItem> collection = item.ItemList(false);
                if (item.NewLine)
                {
                    list2.AddRange(collection);
                }
                else
                {
                    list.AddRange(collection);
                }
            }
            if (timestamp == null)
            {
                timestamp = DateTime.UtcNow.ToString(@"yyyy-MM-dd\THH:mm:ss.fffffffK");
            }
            if (list.Count > 0)
            {
                str = timestamp;
                foreach (DataItem item in list)
                {
                    str = str + "|" + item.ToString();
                }
                str = str + "\n";
                this.SendToAll(str);
            }
            if (list2.Count > 0)
            {
                foreach (DataItem item in list2)
                {
                    str = timestamp;
                    str = str + "|" + item.ToString() + "\n";
                    this.SendToAll(str);
                }
            }
            this.FlushAll();
            foreach (DataItem item in this.mDataItems)
            {
                item.Cleanup();
            }
            this.mBegun = false;
        }

        public void SendToAll(string line)
        {
            byte[] bytes = this.mEncoder.GetBytes(line.ToCharArray());
            if (this.Verbose)
            {
                Console.WriteLine("Sending: " + line);
            }
            foreach (Stream stream in this.mClients.ToArray())
            {
                lock (stream)
                {
                    this.WriteToClient(stream, bytes);
                }
            }
        }

        public void Start()
        {
            if (!this.mRunning)
            {
                this.mListener = new TcpListener(IPAddress.Any, this.mPort);
                this.mListener.Start();
                this.mListenThread = new Thread(new ThreadStart(this.ListenForClients));
                this.mListenThread.Start();
            }
        }

        public void Stop()
        {
            if (this.mRunning)
            {
                this.mRunning = false;
                this.mListener.Stop();
                foreach (object obj2 in this.mClients)
                {
                    ((Stream) obj2).Close();
                }
                this.mClients.Clear();
                this.mListenThread.Join(0x7d0);
                this.mActiveClients.Wait(0x7d0);
            }
        }

        public void Unavailable()
        {
            foreach (DataItem item in this.mDataItems)
            {
                item.Unavailable();
            }
        }

        private void WriteToClient(Stream aClient, byte[] aMessage)
        {
            try
            {
                aClient.Write(aMessage, 0, aMessage.Length);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error during write: " + exception.Message);
                try
                {
                    aClient.Close();
                }
                catch (Exception exception2)
                {
                    Console.WriteLine("Error during close: " + exception2.Message);
                }
                this.mClients.Remove(aClient);
            }
        }

        public int Heartbeat
        {
            get => 
                this.mHeartbeat;
            set
            {
                this.mHeartbeat = value;
                this.PONG = new ASCIIEncoding().GetBytes("* PONG " + this.mHeartbeat.ToString() + "\n");
            }
        }

        public int Port
        {
            get => 
                this.mPort;
            set
            {
                this.mPort = value;
            }
        }

        public bool Running =>
            this.mRunning;

        public int ServerPort =>
            ((IPEndPoint) this.mListener.LocalEndpoint).Port;

        public bool Verbose { get; set; }
    }
}

