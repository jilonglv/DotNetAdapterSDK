namespace MTConnect
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class CuttingTool : Asset
    {
        public const double CT_NULL = double.NaN;
        protected ArrayList mItems;
        protected HashSet<Measurement> mMeasurements;
        protected HashSet<Property> mProperties;

        public CuttingTool(string assetId, string toolId, string serialNumber) : base(assetId)
        {
            this.mProperties = new HashSet<Property>();
            this.mMeasurements = new HashSet<Measurement>();
            this.mItems = new ArrayList();
            this.ToolId = toolId;
            this.SerialNumber = serialNumber;
        }

        public void AddItem(CuttingItem item)
        {
            this.mItems.Add(item);
        }

        public Property AddLife(LifeType type, Direction direction, string value = null, string initial = null, string limit = null, string warning = null)
        {
            Property item = new Property("ToolLife", (string[])null, null)
            {
                Value = value
            };
            item.AddAttribute(new Property.Attribute("type", type.ToString()));
            item.AddAttribute(new Property.Attribute("countDirection", direction.ToString()));
            if (initial != null)
            {
                item.AddAttribute(new Property.Attribute("initial", initial));
            }
            if (limit != null)
            {
                item.AddAttribute(new Property.Attribute("limit", limit));
            }
            if (warning != null)
            {
                item.AddAttribute(new Property.Attribute("warning", warning));
            }
            this.mProperties.Add(item);
            return item;
        }

        public Measurement AddMeasurement(string name, string code, double value = (double)1.0 / (double)0.0, double nominal = (double)1.0 / (double)0.0, double min = (double)1.0 / (double)0.0, double max = (double)1.0 / (double)0.0, string native = null, string units = null)
        {
            Measurement item = new Measurement(name, code, value, nominal, min, max, native, units);
            this.mMeasurements.Add(item);
            return item;
        }

        public void AddProperty(Property property)
        {
            this.mProperties.Add(property);
        }

        public Property AddProperty(string name, string[] arguments, string value = null)
        {
            Property item = new Property(name, arguments, value);
            this.mProperties.Add(item);
            return item;
        }

        public CutterStatus AddStatus(string[] status)
        {
            CutterStatus item = new CutterStatus(status);
            this.mProperties.Add(item);
            return item;
        }

        public static Property.Attribute CreateAttribute(string name, string value) =>
            new Property.Attribute(name, value);

        public override string GetMTCType() =>
            "CuttingTool";

        public override XmlWriter ToXml(XmlWriter writer)
        {
            writer.WriteStartElement("CuttingTool");
            writer.WriteAttributeString("toolId", this.ToolId);
            writer.WriteAttributeString("serialNumber", this.SerialNumber);
            if (this.Manufacturers != null)
            {
                writer.WriteAttributeString("manufactures", this.Manufacturers);
            }
            base.ToXml(writer);
            writer.WriteElementString("Description", this.Description);
            writer.WriteStartElement("CuttingToolLifeCycle");
            foreach (Property property in this.mProperties)
            {
                property.ToXml(writer);
            }
            if (this.mMeasurements.Count > 0)
            {
                writer.WriteStartElement("Measurements");
                foreach (Measurement measurement in this.mMeasurements)
                {
                    measurement.ToXml(writer);
                }
                writer.WriteEndElement();
            }
            if (this.mItems.Count > 0)
            {
                writer.WriteStartElement("CuttingItems");
                foreach (CuttingItem item in this.mItems)
                {
                    item.ToXml(writer);
                }
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
            return writer;
        }

        public string Description { get; set; }

        public string Manufacturers { get; set; }

        public string SerialNumber { get; set; }

        public string ToolId { get; set; }

        public class CutterStatus : CuttingTool.Property
        {
            public HashSet<string> mStatus;

            public CutterStatus(string[] status) : base("CutterStatus", (CuttingTool.Property.Attribute[])null, null)
            {
                this.mStatus = new HashSet<string>();
                foreach (string str in status)
                {
                    this.mStatus.Add(str);
                }
            }

            public void Add(string s)
            {
                this.mStatus.Add(s);
            }

            public void Remove(string s)
            {
                this.mStatus.Remove(s);
            }

            public override XmlWriter ToXml(XmlWriter writer)
            {
                writer.WriteStartElement(base.Name);
                foreach (string str in this.mStatus)
                {
                    writer.WriteElementString("Status", str);
                }
                writer.WriteEndElement();
                return writer;
            }
        }

        public class CuttingItem
        {
            protected HashSet<CuttingTool.Measurement> mMeasurements = new HashSet<CuttingTool.Measurement>();
            protected HashSet<CuttingTool.Property> mProperties = new HashSet<CuttingTool.Property>();

            public CuttingItem(string indices, string id = null, string grade = null, string manufacturers = null)
            {
                this.Indices = indices;
                this.ItemId = id;
                this.Grade = grade;
                this.Manufacturers = manufacturers;
            }

            public CuttingTool.Property AddLife(CuttingTool.LifeType type, CuttingTool.Direction direction, string value = null, string initial = null, string limit = null, string warning = null)
            {
                CuttingTool.Property item = new CuttingTool.Property("ItemLife", (string[])null, null)
                {
                    Value = value
                };
                item.AddAttribute(new CuttingTool.Property.Attribute("type", type.ToString()));
                item.AddAttribute(new CuttingTool.Property.Attribute("countDirection", direction.ToString()));
                if (initial != null)
                {
                    item.AddAttribute(new CuttingTool.Property.Attribute("initial", initial));
                }
                if (limit != null)
                {
                    item.AddAttribute(new CuttingTool.Property.Attribute("limit", limit));
                }
                if (warning != null)
                {
                    item.AddAttribute(new CuttingTool.Property.Attribute("warning", warning));
                }
                this.mProperties.Add(item);
                return item;
            }

            public CuttingTool.Measurement AddMeasurement(string name, string code, double value = (double)1.0 / (double)0.0, double nominal = (double)1.0 / (double)0.0, double min = (double)1.0 / (double)0.0, double max = (double)1.0 / (double)0.0, string native = null, string units = null)
            {
                CuttingTool.Measurement item = new CuttingTool.Measurement(name, code, value, nominal, min, max, native, units);
                this.mMeasurements.Add(item);
                return item;
            }

            public CuttingTool.Property AddProperty(string name, string value)
            {
                CuttingTool.Property item = new CuttingTool.Property(name, value);
                this.mProperties.Add(item);
                return item;
            }

            public CuttingTool.Property AddProperty(string name, string[] arguments, string value = null)
            {
                CuttingTool.Property item = new CuttingTool.Property(name, arguments, value);
                this.mProperties.Add(item);
                return item;
            }

            public XmlWriter ToXml(XmlWriter writer)
            {
                writer.WriteStartElement("CuttingItem");
                writer.WriteAttributeString("indices", this.Indices);
                if (this.ItemId != null)
                {
                    writer.WriteAttributeString("itemId", this.ItemId);
                }
                if (this.Grade != null)
                {
                    writer.WriteAttributeString("grade", this.Grade);
                }
                if (this.Manufacturers != null)
                {
                    writer.WriteAttributeString("manufacturers", this.Manufacturers);
                }
                if (this.Description != null)
                {
                    writer.WriteElementString("Description", this.Description);
                }
                foreach (CuttingTool.Property property in this.mProperties)
                {
                    property.ToXml(writer);
                }
                if (this.mMeasurements.Count > 0)
                {
                    writer.WriteStartElement("Measurements");
                    foreach (CuttingTool.Measurement measurement in this.mMeasurements)
                    {
                        measurement.ToXml(writer);
                    }
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                return writer;
            }

            public string Description { get; set; }

            public string Grade { get; set; }

            public string Indices { get; set; }

            public string ItemId { get; set; }

            public string Manufacturers { get; set; }
        }

        public enum Direction
        {
            UP,
            DOWN
        }

        public enum LifeType
        {
            MINUTES,
            PART_COUNT,
            WEAR
        }

        public class Measurement : CuttingTool.Property
        {
            public Measurement(string name, string code, double value = (double)1.0 / (double)0.0, double nominal = (double)1.0 / (double)0.0, double min = (double)1.0 / (double)0.0, double max = (double)1.0 / (double)0.0, string native = null, string units = null) : base(name, (CuttingTool.Property.Attribute[])null, null)
            {
                base.AddAttribute(new CuttingTool.Property.Attribute("code", code));
                if (!double.IsNaN(value))
                {
                    base.Value = value.ToString();
                }
                if (!double.IsNaN(nominal))
                {
                    base.AddAttribute(new CuttingTool.Property.Attribute("nominal", nominal.ToString()));
                }
                if (!double.IsNaN(min))
                {
                    base.AddAttribute(new CuttingTool.Property.Attribute("minimum", min.ToString()));
                }
                if (!double.IsNaN(max))
                {
                    base.AddAttribute(new CuttingTool.Property.Attribute("maximum", max.ToString()));
                }
                if (native != null)
                {
                    base.AddAttribute(new CuttingTool.Property.Attribute("nativeUnits", native));
                }
                if (units != null)
                {
                    base.AddAttribute(new CuttingTool.Property.Attribute("units", units));
                }
            }
        }

        public class Property
        {
            public ArrayList mAttributes;

            public Property(string name, string value)
            {
                this.Name = name;
                this.Value = value;
            }

            public Property(string name, Attribute[] arguments = null, string value = null)
            {
                this.Name = name;
                this.Value = value;
                if (arguments != null)
                {
                    this.mAttributes = new ArrayList();
                    this.mAttributes = new ArrayList(arguments);
                }
            }

            public Property(string name, string[] arguments, string value = "")
            {
                this.Name = name;
                if (arguments != null)
                {
                    this.mAttributes = new ArrayList();
                    for (int i = 0; i < arguments.Length; i += 2)
                    {
                        this.mAttributes.Add(new Attribute(arguments[i], arguments[i + 1]));
                    }
                }
                this.Value = value;
            }

            public void AddAttribute(Attribute argument)
            {
                if (this.mAttributes == null)
                {
                    this.mAttributes = new ArrayList();
                }
                this.mAttributes.Add(argument);
            }

            public override bool Equals(object obj)
            {
                if (obj is CuttingTool.Property)
                {
                    return this.Name.Equals(((CuttingTool.Property)obj).Name);
                }
                return this.Name.Equals(obj);
            }

            public override int GetHashCode() =>
                this.Name.GetHashCode();

            public virtual XmlWriter ToXml(XmlWriter writer)
            {
                writer.WriteStartElement(this.Name);
                if (this.mAttributes != null)
                {
                    foreach (Attribute attribute in this.mAttributes)
                    {
                        writer.WriteAttributeString(attribute.Name, attribute.Value);
                    }
                }
                if (this.Value != null)
                {
                    writer.WriteValue(this.Value);
                }
                writer.WriteEndElement();
                return writer;
            }

            public string Name { get; set; }

            public string Value { get; set; }

            public class Attribute
            {
                public Attribute(string name, string value)
                {
                    this.Name = name;
                    this.Value = value;
                }

                public string Name { get; set; }

                public string Value { get; set; }
            }
        }
    }
}

