#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Common;

using EfsML.Enum;
using FpML.Enum;
#endregion Using Directives

namespace EFS.Common
{
    #region CacheSerializer
    public class CacheSerializer
    {
        #region Members
        private static List<EFS_Serializer> m_Serializers;
        private static readonly object m_SerializersLock = new object();
        #endregion Members

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <returns></returns>
        public static object CloneDocument(EFS_SerializeInfoBase pSerializeInfo)
        {
            StringBuilder sb = Serialize(pSerializeInfo);
            pSerializeInfo.XMLDocument = sb.ToString();
            return Deserialize(pSerializeInfo);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <returns></returns>
        public static object Deserialize(EFS_SerializeInfoBase pSerializeInfo)
        {

            object result = null;
            EFS_Serializer serializer = GetSerializer(pSerializeInfo);
            if (null != serializer)
                result = serializer.Deserialize(pSerializeInfo.ReaderDocument);
            return result;

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <returns></returns>
        public static EFS_Serializer GetSerializer(EFS_SerializeInfoBase pSerializeInfo)
        {
            lock (m_SerializersLock)
            {
                if (null == m_Serializers)
                    m_Serializers = new List<EFS_Serializer>();
            }

            EFS_Serializer efs_serializer = null;
            lock (m_SerializersLock)
            {
                efs_serializer = m_Serializers.Find(item => item.SerializeType.Equals(pSerializeInfo.TypeDocument) && 
                    ((item.SerializeInfo.XmlAttributeOverrides != null && pSerializeInfo.XmlAttributeOverrides != null)
                            || (item.SerializeInfo.XmlAttributeOverrides == null && pSerializeInfo.XmlAttributeOverrides == null)));
                if (null != efs_serializer)
                {
                    efs_serializer.SerializeInfo = pSerializeInfo;
                }
                else
                {
                    efs_serializer = new EFS_Serializer(pSerializeInfo);
                    m_Serializers.Add(efs_serializer);
                }
            }
            return efs_serializer;
        }
        
        /// <summary>
        /// /
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <returns></returns>
        public static StringBuilder Serialize(EFS_SerializeInfoBase pSerializeInfo)
        {
            return Serialize(pSerializeInfo, new UnicodeEncoding());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <param name="pEncoding"></param>
        /// <returns></returns>
        public static StringBuilder Serialize(EFS_SerializeInfoBase pSerializeInfo, Encoding pEncoding)
        {
            
                StringBuilder result = null;
                EFS_Serializer serializer = GetSerializer(pSerializeInfo);
                if (null != serializer)
                    result = serializer.Serialize(pSerializeInfo, pEncoding);
                return result;
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <param name="pPath"></param>
        public static void Serialize(EFS_SerializeInfoBase pSerializeInfo, string pPath)
        {
            Serialize(pSerializeInfo, pPath, new UnicodeEncoding());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <param name="pPath"></param>
        /// <param name="pEncoding"></param>
        public static void Serialize(EFS_SerializeInfoBase pSerializeInfo, string pPath, Encoding pEncoding)
        {
            
                EFS_Serializer serializer = GetSerializer(pSerializeInfo);
                if (null != serializer)
                    serializer.Serialize(pPath, pSerializeInfo, pEncoding);
            
        }
        
        /// <summary>
        /// Serialize le document inclus dans pSerializeInfo et valide la conformité XSD
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <exception cref="XSDValidationException en cas de non conformité"></exception>
        public static void Conformity(EFS_SerializeInfoBase pSerializeInfo)
        {
            //Serialize
            StringBuilder sb = Serialize(pSerializeInfo);
            //Xsd Validation
            XSDValidation xsd = new XSDValidation(pSerializeInfo.Schemas);
            xsd.CheckConformity(sb.ToString());
        }
        
        /// <summary>
        /// 
        /// </summary>
        public static void Clear()
        {
            if (null != m_Serializers)
                m_Serializers.Clear(); 
        }
        #endregion Methods

    }
    #endregion CacheSerializer
    #region XmlTextWriterFormattedNoDeclaration
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class XmlTextWriterFormattedNoDeclaration : XmlTextWriter
    {
        public XmlTextWriterFormattedNoDeclaration(TextWriter pTw)
            : base(pTw)
        {
            Formatting = Formatting.Indented;
        }
        public override void WriteStartDocument() { }
    }
    #endregion XmlTextWriterFormattedNoDeclaration

    /// <summary>
    /// 
    /// </summary>
    public class EFS_Serializer
    {
        #region Members
        private EFS_SerializeInfoBase m_SerializeInfo;
        private bool m_IsRootOverride;
        //private bool m_SourceSpecified;
        private readonly XmlSerializer m_InstanceSerializer;
        protected bool m_IsDisposed;
        #region EFSClassEnum
        private enum EFSClassEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Shared.InformationSource")]
            assetFxRateId,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.CalculationPeriodDates")]
            efs_CalculationPeriodDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Shared.Product")]
            efs_Events,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Shared.Exercise")]
            efs_ExerciseDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.Fra")]
            efs_FraDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.FxLinkedNotionalSchedule")]
            efs_FxLinkedNotionalDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.MandatoryEarlyTermination")]
            efs_MandatoryEarlyTerminationDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Shared.Payment")]
            efs_Payment,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.PaymentDates")]
            efs_PaymentDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.ResetDates")]
            efs_ResetDates,
            [System.Xml.Serialization.XmlEnumAttribute("FpML.v44.Ird.Swaption")]
            efs_SwaptionDates,
        }
        #endregion EFSClassEnum
        #endregion Members
        #region Accessors
        #region SerializeInfo
        public EFS_SerializeInfoBase SerializeInfo
        {
            set { m_SerializeInfo = value; }
            get { return m_SerializeInfo; }
        }
        #endregion SerializeInfo
        #region DocumentAttributeOverride
        public XmlAttributeOverrides DocumentAttributeOverride
        {
            get
            {
                XmlAttributeOverrides attrOverrides = new XmlAttributeOverrides();
                XmlAttributes attrs;
                #region RootOverride
                if (IsRootOverride)
                {
                    XmlRootAttribute xRoot = new XmlRootAttribute
                    {
                        Namespace = m_SerializeInfo.NameSpace,
                        ElementName = m_SerializeInfo.Source
                    };
                    attrs = new XmlAttributes
                    {
                        XmlRoot = xRoot
                    };
                    attrOverrides.Add(m_SerializeInfo.TypeDocument, attrs);
                }
                #endregion RootOverride
                #region ClassOverride
                if (m_SerializeInfo.IsClassOverride)
                {
                    EFSClassEnum efsClassEnum = new EFSClassEnum();
                    FieldInfo[] flds = efsClassEnum.GetType().GetFields();
                    Assembly ass = null;
                    foreach (FieldInfo fld in flds)
                    {
                        object[] attributes = fld.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                        if (0 < attributes.Length)
                        {
                            XmlEnumAttribute enumAttribute = (XmlEnumAttribute)attributes[0];
                            Type tParentClass;
                            try { tParentClass = Type.GetType(enumAttribute.Name, true, true); }
                            catch (Exception ex)
                            {
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                                if (null == ass)
                                    ass = Assembly.Load(m_SerializeInfo.TypeDocument.Assembly.FullName);
                                tParentClass = ass.GetType(enumAttribute.Name, false, true);
                            }
                            if (null != tParentClass)
                            {
                                attrs = new XmlAttributes
                                {
                                    XmlIgnore = false
                                };
                                attrOverrides.Add(tParentClass, fld.Name, attrs);
                            }
                        }
                    }
                }
                #endregion ClassOverride
                return attrOverrides;
            }
        }
        #endregion DocumentAttributeOverride
        #region IsClassOverride
        public bool IsClassOverride
        {
            get { return m_SerializeInfo.IsClassOverride; }
            set { m_SerializeInfo.IsClassOverride = value; }
        }
        #endregion IsClassOverride
        #region IsRootOverride
        public bool IsRootOverride
        {
            get { return m_IsRootOverride; }
            set { m_IsRootOverride = value; }
        }
        #endregion IsRootOverride
        #region NameSpace
        public string NameSpace
        {
            get { return m_SerializeInfo.NameSpace; }
            set { m_SerializeInfo.NameSpace = value; }
        }
        #endregion NameSpace
        #region InstanceSerializer
        public XmlSerializer InstanceSerializer
        {
            get { return m_InstanceSerializer; }
        }
        #endregion SerializeType
        #region SerializeType
        public Type SerializeType
        {
            get { return m_SerializeInfo.TypeDocument; }
            set { m_SerializeInfo.TypeDocument = value; }
        }
        #endregion SerializeType
        #region Source
        public string Source
        {
            get { return m_SerializeInfo.Source; }
            set { m_SerializeInfo.Source = value; }
        }
        #endregion Source
        #region SourceSpecified
        public bool SourceSpecified
        {
            get { return StrFunc.IsFilled(m_SerializeInfo.Source); }
        }
        #endregion SourceSpecified
        #endregion Accessors
        #region Constructors
        public EFS_Serializer(EFS_SerializeInfoBase pSerializeInfo)
        {
            m_SerializeInfo = pSerializeInfo;

            m_IsRootOverride = SourceSpecified &&
                (Cst.FpML_Name == m_SerializeInfo.Source || Cst.FixML_Name == m_SerializeInfo.Source || Cst.EFSmL_Name == m_SerializeInfo.Source);

            if (m_SerializeInfo.IsClassOverride || m_IsRootOverride)
            {
                m_InstanceSerializer = new XmlSerializer(m_SerializeInfo.TypeDocument, (XmlAttributeOverrides)DocumentAttributeOverride);
            }
            else if (m_SerializeInfo.XmlAttributeOverrides != null)
            {
                /// FI 20130627 [18745] add gestion de m_SerializeInfo.xmlAttributeOverrides
                m_InstanceSerializer = new XmlSerializer(m_SerializeInfo.TypeDocument, m_SerializeInfo.XmlAttributeOverrides);
            }
            else
            {
                m_InstanceSerializer = new XmlSerializer(m_SerializeInfo.TypeDocument);
            }
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDocument"></param>
        /// <returns></returns>
        public object Deserialize(StringReader pDocument)
        {
            return m_InstanceSerializer.Deserialize(pDocument);
        }


        /// <summary>
        /// Serialize in StringBuilder 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <returns></returns>
        public StringBuilder Serialize(EFS_SerializeInfoBase pSerializeInfo)
        {
            return Serialize(pSerializeInfo, new System.Text.UnicodeEncoding());
        }

        /// <summary>
        /// Serialize in StringBuilder 
        /// </summary>
        /// <param name="pSerializeInfo"></param>
        /// <param name="pEncoding"></param>
        /// <returns></returns>
        public StringBuilder Serialize(EFS_SerializeInfoBase pSerializeInfo, Encoding pEncoding)
        {
            StringWriterWithEncoding writer = null;
            try
            {
                StringBuilder sb = new StringBuilder();
                writer = new StringWriterWithEncoding(sb, pEncoding);
                Serialize(writer, pSerializeInfo);
                return sb;
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
        }
        
        
        /// <summary>
        /// Serialize in File 
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pSerializeInfo"></param>
        public void Serialize(string pPath, EFS_SerializeInfoBase pSerializeInfo)
        {
            Serialize(pPath, pSerializeInfo, new UnicodeEncoding());
        }
       /// <summary>
        /// Serialize in File 
        /// </summary>
        /// <param name="pPath"></param>
        /// <param name="pSerializeInfo"></param>
        /// <param name="pEncoding"></param>
        public void Serialize(string pPath, EFS_SerializeInfoBase pSerializeInfo, Encoding pEncoding)
        {
            TextWriter writer = null;
            try
            {
                writer = new StreamWriter(pPath, false, pEncoding);
                Serialize(writer, pSerializeInfo);
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
        }
        /// <summary>
        /// Serialize
        /// </summary>
        /// <param name="pWriter"></param>
        /// <param name="pSerializeInfo"></param>
        public void Serialize(TextWriter pWriter, EFS_SerializeInfoBase pSerializeInfo)
        {
            XmlSerializerNamespaces ns;
            if (pSerializeInfo.IsXMLTrade)
            {
                ns = pSerializeInfo.AddNameSerializerNamespaces();
                if (null != ns)
                    m_InstanceSerializer.Serialize(pWriter, pSerializeInfo.Document, ns);
            }
            else if (pSerializeInfo.IsWithoutNamespaces)
            {
                ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                XmlWriter writer = new XmlTextWriterFormattedNoDeclaration(pWriter);
                m_InstanceSerializer.Serialize(writer, pSerializeInfo.Document, ns);
            }
            else
                m_InstanceSerializer.Serialize(pWriter, pSerializeInfo.Document);

        }
        
        #endregion Methods
    }

    /// <summary>
    /// 
    /// </summary>
    public class EFS_SerializeInfoBase
    {
        #region Members
        protected object m_Document;
        protected string m_XMLDocument;
        protected Type m_Type;
        protected string m_NameSpace;
        protected string m_Source;
        protected bool m_IsXMLTrade;
        protected bool m_IsClassOverride;
        protected bool m_IsWithoutNamespaces;
        protected string[] m_Schemas;
        protected EfsMLDocumentVersionEnum m_Version;
        
        #endregion Members

        #region Accessors
        public Type TypeDocument
        {
            set { m_Type = value; }
            get { return m_Type; }
        }
        public object Document { get { return m_Document; } }
        public string XMLDocument
        {
            set { m_XMLDocument = value; }
            get { return m_XMLDocument; }
        }
        public StringReader ReaderDocument
        {
            get { return new StringReader(m_XMLDocument); }
        }
        public string NameSpace
        {
            set { m_NameSpace = value; }
            get { return m_NameSpace; }
        }
        public string Source
        {
            set { m_Source = value; }
            get { return m_Source; }
        }
        public bool IsXMLTrade
        {
            set { m_IsXMLTrade = value; }
            get { return m_IsXMLTrade; }
        }
        public bool IsClassOverride
        {
            set { m_IsClassOverride = value; }
            get { return m_IsClassOverride; }
        }
        public bool IsWithoutNamespaces
        {
            set { m_IsWithoutNamespaces = value; }
            get { return m_IsWithoutNamespaces; }
        }
        public string[] Schemas
        {
            get { return m_Schemas; }
        }
        public EfsMLDocumentVersionEnum Version
        {
            set { m_Version = value; }
            get { return m_Version; }
        }
        #region AddNameSerializerNamespaces
        public XmlSerializerNamespaces AddNameSerializerNamespaces()
        {
            XmlSerializerNamespaces ret = null;
            if (IsXMLTrade)
            {
                ret = new XmlSerializerNamespaces();
                if (Cst.Track_Name == Source)
                    ret.Add("track", Cst.Track_Namespace);
                if (EfsMLDocumentVersionEnum.Version30 == Version)
                {
                    ret.Add("efs", Cst.EFSmL_Namespace_3_0);
                    ret.Add(string.Empty, Cst.FpML_Namespace_4_4);
                    ret.Add("fixml", Cst.FixML_Namespace_5_0_SP1);
                }
                else if (EfsMLDocumentVersionEnum.Version20 == Version)
                {
                    ret.Add("efs", Cst.EFSmL_Namespace_2_0);
                    ret.Add(string.Empty, Cst.FpML_Namespace_4_2);
                    ret.Add("fix", Cst.FixML_Namespace_4_4);
                }
                ret.Add("xsd", Cst.XMLSchema_Namespace);
                ret.Add("xsi", Cst.XMLSchemaInstance_Namespace);
            }
            return ret;
        }
        #endregion AddNameSerializerNamespaces

        /// <summary>
        /// Obtient ou définit les substitutions des attributs de serialization  
        /// </summary>
        /// FI 20130627 [18745] Add property
        public XmlAttributeOverrides XmlAttributeOverrides
        {
            set;
            get;
        }
        #endregion Accessors

        #region Constructors
        public EFS_SerializeInfoBase(object pDocument)
        {
            m_Document = pDocument;
            //m_IsXMLTrade	= true;
        }
        public EFS_SerializeInfoBase(Type pTypeDocument, object pDocument)
        {
            m_Type = pTypeDocument;
            m_Document = pDocument;
        }
        public EFS_SerializeInfoBase(Type pTypeDocument, string pXMLDocument)
        {
            m_Type = pTypeDocument;
            m_XMLDocument = pXMLDocument;
        }
        #endregion Constructors
        #region Members
        public void SetEfsMLTradeInfo(DocumentVersionEnum pDocumentVersion)
        {

            m_IsXMLTrade = true;
            m_Source = Cst.EFSmL_Name;
            if (DocumentVersionEnum.Version44 == pDocumentVersion)
            {
                m_Version = EfsMLDocumentVersionEnum.Version30;
                m_NameSpace = Cst.EFSmL_Namespace_3_0;
            }
            else if (DocumentVersionEnum.Version42 == pDocumentVersion)
            {
                m_Version = EfsMLDocumentVersionEnum.Version20;
                m_NameSpace = Cst.EFSmL_Namespace_2_0;
            }
        }
        public void SetPosRequestTradeInfo(EfsMLDocumentVersionEnum pEfsMLVersion)
        {

            m_IsXMLTrade = true;
            m_Source = string.Empty;
            m_Version = pEfsMLVersion;
            if (EfsMLDocumentVersionEnum.Version30 == pEfsMLVersion)
                m_NameSpace = Cst.EFSmL_Namespace_3_0;
            else if (EfsMLDocumentVersionEnum.Version20 == pEfsMLVersion)
                m_NameSpace = Cst.EFSmL_Namespace_2_0;
        }
        #endregion Members
    }
    
}
