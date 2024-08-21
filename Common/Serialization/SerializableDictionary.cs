using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using EFS.ACommon;

namespace EFS.Common
{
    /// <summary>
    /// Dictionary class extention adding serialization capability
    /// </summary>
    /// <typeparam name="TKey">key class type</typeparam>
    /// <typeparam name="TValue">value class type</typeparam>
    // EG 20180425 Analyse du code Correction [CA2237]
    // EG 20190308 Add ItemName
    [XmlRoot("Dictionary")]
    [Serializable]
    // EG 20190318 Add Namespace member
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
    {

        /// <summary>
        /// xml tag name of the dictionary item
        /// </summary>
        protected string ItemName
        { get; set; }

        /// <summary>
        /// xml tag name of the dictionary key
        /// </summary>
        protected string KeyName
        { get; set; }

        /// <summary>
        /// xml tag name of the dictionary value
        /// </summary>
        protected string ValueName
        { get; set; }

        /// <summary>
        /// DefaultNameSpace of the dictionary
        /// </summary>
        protected string Namespace
        { get; set; }

        /// <summary>
        /// Get an empty dictionary
        /// </summary>
        public SerializableDictionary()
        {
            ItemName = "Item";
            KeyName = "Key";
            ValueName = "Value";
        }

        /// <summary>
        /// Get an empty dictionary
        /// </summary>
        /// <param name="pItemName">node name of the serialized dictionary item</param> 
        /// <param name="pValueName">node name of the serialized dictionary value</param>
        /// <param name="pKeyName">node name of the serialized dictionary key</param>
        // EG 20190308 Add ItemName
        // EG 20190318 Add Namespace member
        public SerializableDictionary(string pKeyName, string pValueName)
            : this("Item", pKeyName, pValueName, null, null)
        { }
        // EG 20190318 Add Namespace member
        public SerializableDictionary(string pItemName, string pKeyName, string pValueName, string pNameSpace) :
            this(pItemName, pKeyName, pValueName, pNameSpace, null)
        { }

        /// <summary>
        /// Get an enmpty dictionary with a custom key comparer
        /// </summary>
        /// <param name="pValueName">node name of the serialized dictionary value</param>
        /// <param name="pKeyName">node name of the serialized dictionary key</param>
        /// <param name="pKeyComparer">user implemented key comparer</param>
        // EG 20190308 Add ItemName
        // EG 20190318 Add Namespace member
        public SerializableDictionary(string pKeyName, string pValueName, IEqualityComparer<TKey> pKeyComparer) :
            this("Item", pKeyName, pValueName, null, pKeyComparer)
        {
        }
        // EG 20190318 Add Namespace member
        public SerializableDictionary(string pItemName, string pKeyName, string pValueName, string pNameSpace, IEqualityComparer<TKey> pKeyComparer) :
            base(pKeyComparer)
        {
            ItemName = pItemName;
            KeyName = pKeyName;
            ValueName = pValueName;
            Namespace = pNameSpace;
        }

        /// <summary>
        /// Dictionary schema (no schema)
        /// </summary>
        /// <returns>Null</returns>
        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Read the dictionary content from the stream reader and add them to the collection.
        /// </summary>
        /// <param name="reader">Xml reader</param>
        // EG 20190308 Add ItemName
        // EG 20190318 Add Namespace member
        public void ReadXml(System.Xml.XmlReader reader)
        {
            this.ItemName = reader.GetAttribute("item");
            this.KeyName = reader.GetAttribute("key");
            this.ValueName = reader.GetAttribute("value");
            this.Namespace = reader.GetAttribute("namespace");

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey), this.Namespace);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), this.Namespace);

            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {
                reader.ReadStartElement(this.ItemName);

                reader.ReadStartElement(this.KeyName);
                TKey key = (TKey)keySerializer.Deserialize(reader);
                reader.ReadEndElement();

                reader.ReadStartElement(this.ValueName);
                TValue value = (TValue)valueSerializer.Deserialize(reader);
                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();
            }

            reader.ReadEndElement();
        }

        /// <summary>
        /// Write the dictionary content into the stream writer
        /// </summary>
        /// <param name="writer">the XMl writer</param>
        // EG 20190308 Add ItemName
        // EG 20190318 Add Namespace member
        public void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteAttributeString("item", this.ItemName);
            writer.WriteAttributeString("key", this.KeyName);
            writer.WriteAttributeString("value", this.ValueName);
            if (StrFunc.IsFilled(this.Namespace))
                writer.WriteAttributeString("xmlns", this.Namespace);

            XmlSerializer keySerializer = new XmlSerializer(typeof(TKey),this.Namespace);
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue), this.Namespace);

            foreach (TKey key in this.Keys)
            {
                writer.WriteStartElement(this.ItemName);

                writer.WriteStartElement(this.KeyName);
                keySerializer.Serialize(writer, key);
                writer.WriteEndElement();

                writer.WriteStartElement(this.ValueName);
                TValue value = this[key];
                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

                writer.WriteEndElement();
            }
        }

        // EG 20180425 Analyse du code Correction [CA2240]
        // EG 20190308 Add ItemName
        // EG 20190318 Add Namespace member
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException("info"); 
            info.AddValue("item", ItemName);
            info.AddValue("key", KeyName);
            info.AddValue("value", ValueName);
            if (StrFunc.IsFilled(Namespace))
                info.AddValue("xmlns", Namespace);
            base.GetObjectData(info, context);
        }

    }
}