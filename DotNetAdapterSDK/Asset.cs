namespace MTConnect
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Xml;

    public abstract class Asset
    {
        public Asset(string id)
        {
            this.AssetId = id;
        }

        public abstract string GetMTCType();
        public virtual XmlWriter ToXml(XmlWriter writer)
        {
            writer.WriteAttributeString("assetId", this.AssetId);
            return writer;
        }

        public string AssetId { get; set; }
    }
}

