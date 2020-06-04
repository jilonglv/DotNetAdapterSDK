namespace MTConnect
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    public class Condition : DataItem
    {
        private List<Active> mActiveList;
        private bool mBegun;
        private bool mPrepared;
        private bool mSimple;

        public Condition(string name, bool simple = false) : base(name)
        {
            this.mBegun = false;
            this.mPrepared = false;
            this.mActiveList = new List<Active>();
            base.mNewLine = true;
            this.mSimple = simple;
            this.Add(new Active(base.mName, Level.UNAVAILABLE, "", "", "", ""));
        }

        private void Add(Active active)
        {
            this.mActiveList.Add(active);
        }

        public bool Add(Level level, string text = "", string code = "", string qualifier = "", string severity = "")
        {
            Predicate<Active> match = null;
            bool flag = false;
            Active active = null;
            if (this.mActiveList.Count > 0)
            {
                active = this.mActiveList.First<Active>();
            }
            if (((level == Level.NORMAL) || (level == Level.UNAVAILABLE)) && (code.Length == 0))
            {
                if ((this.mActiveList.Count == 1) && (active.mLevel == level))
                {
                    active.mMarked = true;
                    return flag;
                }
                this.mActiveList.Clear();
                this.Add(new Active(base.mName, level, "", "", "", ""));
                return (base.mChanged = true);
            }
            if ((this.mActiveList.Count<Active>() == 1) && active.mPlaceholder)
            {
                this.mActiveList.Clear();
            }
            if (match == null)
            {
                match = ak => ak.mNativeCode == code;
            }
            Active active2 = this.mActiveList.Find(match);
            if (active2 != null)
            {
                flag = active2.Set(level, text, qualifier, severity);
                base.mChanged = base.mChanged || flag;
                return flag;
            }
            this.Add(new Active(base.mName, level, text, code, qualifier, severity));
            return (base.mChanged = true);
        }

        public override void Begin()
        {
            if (!this.mSimple)
            {
                foreach (Active active in this.mActiveList)
                {
                    active.Clear();
                }
                this.mBegun = true;
            }
            this.mPrepared = base.mChanged = false;
        }

        public override void Cleanup()
        {
            base.Cleanup();
            this.mBegun = this.mPrepared = false;
            foreach (Active active in this.mActiveList.ToList<Active>())
            {
                if (!(active.mPlaceholder || active.mMarked))
                {
                    this.mActiveList.Remove(active);
                }
                active.Cleanup();
            }
        }

        public bool Clear(string code)
        {
            Active active = this.mActiveList.Find(ak => ak.mNativeCode == code);
            if (active != null)
            {
                if (this.mActiveList.Count<Active>() == 1)
                {
                    this.Add(Level.NORMAL, "", "", "", "");
                }
                else
                {
                    active.Set(Level.NORMAL, "", "", "");
                    active.Clear();
                }
                base.mChanged = true;
                return true;
            }
            return false;
        }

        public override List<DataItem> ItemList(bool all = false)
        {
            List<DataItem> list = new List<DataItem>();
            if (all)
            {
                foreach (Active active in this.mActiveList)
                {
                    list.Add(active);
                }
                return list;
            }
            if (this.mSimple)
            {
                foreach (Active active in this.mActiveList)
                {
                    if (active.Changed)
                    {
                        list.Add(active);
                    }
                }
                return list;
            }
            if (this.mBegun && this.mPrepared)
            {
                if (!base.mChanged)
                {
                    return list;
                }
                foreach (Active active in this.mActiveList)
                {
                    if (active.Changed)
                    {
                        list.Add(active);
                    }
                }
            }
            return list;
        }

        public bool Normal() => 
            this.Add(Level.NORMAL, "", "", "", "");

        public override void Prepare()
        {
            if (this.mBegun)
            {
                bool flag = false;
                foreach (Active active in this.mActiveList)
                {
                    if (active.mPlaceholder || active.mMarked)
                    {
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    this.Normal();
                }
                foreach (Active active in this.mActiveList)
                {
                    if (!(active.mPlaceholder || active.mMarked))
                    {
                        active.Set(Level.NORMAL, "", "", "");
                        active.mMarked = false;
                    }
                    if (active.Changed)
                    {
                        base.mChanged = true;
                    }
                }
                this.mPrepared = true;
            }
        }

        public override void Unavailable()
        {
            this.Add(Level.UNAVAILABLE, "", "", "", "");
        }

        public class Active : DataItem
        {
            public Condition.Level mLevel;
            public bool mMarked;
            public string mNativeCode;
            public string mNativeSeverity;
            public bool mPlaceholder;
            public string mQualifier;
            public string mText;

            public Active(string name, Condition.Level level, string text = "", string code = "", string qualifier = "", string severity = "") : base(name)
            {
                this.mMarked = true;
                this.mPlaceholder = false;
                this.mLevel = level;
                this.mText = text;
                this.mNativeCode = code;
                this.mQualifier = qualifier;
                this.mNativeSeverity = severity;
                base.mNewLine = true;
                if ((this.mNativeCode.Length == 0) && ((this.mLevel == Condition.Level.NORMAL) || (this.mLevel == Condition.Level.UNAVAILABLE)))
                {
                    this.mPlaceholder = true;
                }
            }

            public void Clear()
            {
                this.mMarked = false;
            }

            public bool Set(Condition.Level level, string text = "", string qualifier = "", string severity = "")
            {
                base.mChanged = (((level != this.mLevel) || (text != this.mText)) || (qualifier != this.mQualifier)) || (severity != this.mNativeSeverity);
                if (base.mChanged)
                {
                    this.mLevel = level;
                    this.mQualifier = qualifier;
                    this.mText = text;
                    this.mNativeSeverity = severity;
                }
                this.mMarked = true;
                return base.mChanged;
            }

            public override string ToString() => 
                (base.mName + "|" + Enum.GetName(this.mLevel.GetType(), this.mLevel) + "|" + this.mNativeCode + "|" + this.mNativeSeverity + "|" + this.mQualifier + "|" + this.mText);
        }

        public enum Level
        {
            UNAVAILABLE,
            NORMAL,
            WARNING,
            FAULT
        }
    }
}

