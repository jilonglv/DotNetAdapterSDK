namespace MTConnect
{
    using System;

    public class Message : DataItem
    {
        private string mCode;

        public Message(string name) : base(name)
        {
            base.mNewLine = true;
        }

        public override string ToString() => 
            string.Concat(new object[] { base.mName, "|", this.mCode, "|", base.mValue });

        public string Code
        {
            get => 
                this.mCode;
            set
            {
                if (this.mCode != value)
                {
                    base.mChanged = true;
                    this.mCode = value;
                }
            }
        }
    }
}

