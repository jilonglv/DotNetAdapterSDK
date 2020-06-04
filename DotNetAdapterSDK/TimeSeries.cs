namespace MTConnect
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class TimeSeries : DataItem
    {
        public double[] mValues;

        public TimeSeries(string name, double rate = 0.0) : base(name)
        {
            base.mNewLine = true;
            this.Rate = rate;
        }

        public override string ToString()
        {
            string str2;
            int num;
            string str = (this.Rate == 0.0) ? "" : this.Rate.ToString();
            if (this.mValues != null)
            {
                str2 = string.Join(" ", (from p in this.Values select p.ToString()).ToArray<string>());
                num = this.Values.Count<double>();
            }
            else
            {
                num = 0;
                str2 = "";
            }
            return (base.mName + "|" + this.Values.Count<double>().ToString() + "|" + str + "|" + str2);
        }

        public double Rate { get; set; }

        public double[] Values
        {
            get => 
                this.mValues;
            set
            {
                this.mValues = value;
                base.mChanged = true;
            }
        }
    }
}

