using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;
using System.Xml.Schema; 
using System.Xml.Serialization;


using EFS.ACommon;  

namespace EFS.Common
{
    /// <summary>
    /// Permet de gérer un Text comme étant un bloc CDATA.
    /// <para>Par défaut le text est considéré comme de type XML</para>
    /// <remarks>Il est possible de spécifier un Id</remarks>
    /// </summary> 
    public class CDATA : IXmlSerializable
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        private string _text;
        /// <summary>
        /// 
        /// </summary>
        public string _id;

        /// <summary>
        ///  Obtient ou définie un flag pour spécifier si le _text est un flux XML
        /// </summary>
        /// FI 20140519 [19923] add isXmltext
        public Boolean isXmltext = true;
        #endregion Members

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get { return _text; }
        }
        /// <summary>
        /// 
        /// </summary>
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool IdSpecified
        {
            get { return StrFunc.IsFilled(_id); }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CDATA()
        { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public CDATA(string text)
        {
            _text = text;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public CDATA(string text, Boolean pIsXmltext)
        {
            _text = text;
            isXmltext = pIsXmltext;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="id"></param>
        public CDATA(string text, string id)
            : this(text)
        {
            _id = id;
        }
        #endregion constructor

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            // RD 20100902 Il faudrait lire l'attribut en premier, avant de lire le texte
            _id = reader.GetAttribute("id");
            _text = reader.ReadString();
            //
            reader.Read();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            string data;
            /// FI 20140519 [19923] test sur isXmltext
            if (isXmltext)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    XmlWriterSettings xmlWritterSetting = new XmlWriterSettings
                    {
                        Indent = true
                    };
                    XmlWriter xmlWritter = XmlWriter.Create(sb, xmlWritterSetting);

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(_text);
                    doc.Save(xmlWritter);

                    data = sb.ToString();
                }
                catch (XmlException)
                {
                    // On rentre ds ce catch si la source n'est pas au format XML
                    data = _text;
                }
            }
            else
            {
                data = _text;
            }

            // RD 20100902 Il faudrait écrire l'attribut en premier, avant d'écrire le texte
            if (IdSpecified)
                writer.WriteAttributeString("id", _id);

            writer.WriteCData(data);
        }
        #endregion

    }
}
