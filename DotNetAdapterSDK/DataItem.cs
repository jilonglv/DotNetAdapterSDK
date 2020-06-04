namespace MTConnect
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class DataItem
    {
        public string DevicePrefix = null;
        protected bool mChanged = true;
        protected string mName;
        protected bool mNewLine = false;
        protected object mValue = "UNAVAILABLE";

        public DataItem(string name)
        {
            this.mName = name;
        }

        public virtual void Begin()
        {
        }

        public virtual void Cleanup()
        {
            this.mChanged = false;
        }

        public void ForceChanged()
        {
            this.mChanged = true;
        }

        public bool IsUnavailable() => 
            this.mValue.Equals("UNAVAILABLE");

        public virtual List<DataItem> ItemList(bool all = false)
        {
            List<DataItem> list = new List<DataItem>();
            if (all || this.mChanged)
            {
                list.Add(this);
            }
            return list;
        }

        public virtual void Prepare()
        {
        }

        public override string ToString()
        {
            if (this.DevicePrefix == null)
            {
                return (this.mName + "|" + this.mValue);
            }
            return string.Concat(new object[] { this.DevicePrefix, ":", this.mName, "|", this.mValue });
        }

        public virtual void Unavailable()
        {
            this.Value = "UNAVAILABLE";
        }

        public bool Changed =>
            this.mChanged;

        public bool NewLine =>
            this.mNewLine;

        public object Value
        {
            get => 
                this.mValue;
            set
            {
                if (!this.mValue.Equals(value))
                {
                    this.mValue = value;
                    this.mChanged = true;
                }
            }
        }
    }
}

