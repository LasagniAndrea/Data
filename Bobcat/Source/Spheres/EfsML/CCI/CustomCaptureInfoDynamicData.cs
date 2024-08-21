#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;  
using System.Globalization;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Reflection;
using System.Xml.Serialization;
using System.Drawing;
using System.Data; 

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data; 

using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.Enum.Tools;
using EfsML.Enum;
using EfsML.DynamicData;
using FpML.Enum;
#endregion Using Directives

namespace EFS.GUI.CCI
{
    /// <summary>
    ///  <para>- Possède un membre StringDynamicData (newValueDynamicData) de manière à alimenter newValue avec des fonctions SQL, SpheresLib, etc...</para>
    ///  <para>- Possède un membre Liste de StringDynamicData (dynamicAttribs) de manière à alimenter les différents attributs avec des fonctions SQL, SpheresLib, etc...</para>
    /// </summary>
    public class CustomCaptureInfoDynamicData : CustomCaptureInfo
    {
        #region Members
        private StringDynamicData _newValueDynamicData;
        /// <summary>
        /// 
        /// </summary>
        // RD 20121207 [18240] Ajout et gestion d'un memebre List<StringDynamicData> _dynamicAttribs
        private List<StringDynamicData> _dynamicAttribs;
        #endregion

        #region accessor
        [System.Xml.Serialization.XmlElementAttribute("dynamicValue")]
        public StringDynamicData NewValueDynamicData
        {
            set { _newValueDynamicData = value; }
            get { return _newValueDynamicData; }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public bool DynamicAttribsSpecified;

        [System.Xml.Serialization.XmlArray(ElementName = "dynamicAttribs")]
        [System.Xml.Serialization.XmlArrayItemAttribute("dynamicAttrib")]
        public List<StringDynamicData> DynamicAttribs
        {
            set { _dynamicAttribs = value; }
            get { return _dynamicAttribs; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// FI 21070116 [21916] Modify
        /// FI 20170404 [23039] Modify
        public CustomCaptureInfoDynamicData(CustomCaptureInfo pCci)
        {
            ClientId = pCci.ClientId;
            DataType = pCci.DataType;
            Regex = pCci.Regex;

            IsMandatory = pCci.IsMandatory;
            IsEnabled = pCci.IsEnabled;
            IsSystem = pCci.IsSystem;  // FI 21070116 [21916] Add
            IsAutoCreate = pCci.IsAutoCreate; // FI 20170404 [23039] Add
            
            QuickClientId = pCci.QuickClientId;
            QuickDataPosition = pCci.QuickDataPosition;
            QuickSeparator = pCci.QuickSeparator;
            QuickFormat = pCci.QuickFormat;

            LastValue = pCci.LastValue;
            NewValue = pCci.NewValue;
            Sql_Table = pCci.Sql_Table;
            LastSql_Table = pCci.LastSql_Table;

            IsInputByUser = pCci.IsInputByUser;
            IsLastInputByUser = pCci.IsLastInputByUser;

            ErrorMsg = pCci.ErrorMsg;
            Display = pCci.Display;
        }
        public CustomCaptureInfoDynamicData()
            : base()
        {
        }
        #endregion constructor

        /// <summary>
        /// Alimente newValue à partir de newValueDynamicData
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        public void SetNewValue(string pCS, IDbTransaction pDbTransaction)
        {
            if (null != NewValueDynamicData)
            {
                NewValue = NewValueDynamicData.GetDataValue(pCS, pDbTransaction);
                NewValueDynamicData = null;
            }
        }

        /// <summary>
        /// Alimente les différents attributs à partir de la liste dynamicAttribs
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
        public void SetAttribute(string pCS, IDbTransaction pDbTransaction)
        {
            if (DynamicAttribsSpecified)
            {
                StringDynamicData attrib = null;
                string attribValue = string.Empty;

                attrib = DynamicAttribs.Find(item => item.name == "clientId");
                if (null != attrib)
                {
                    attribValue = attrib.GetDataValue(pCS, pDbTransaction);
                    if (StrFunc.IsFilled(attribValue))
                        ClientId = attribValue;
                }

                attrib = DynamicAttribs.Find(item => item.name == "mandatory");
                if (null != attrib)
                {
                    attribValue = attrib.GetDataValue(pCS, pDbTransaction);
                    if (StrFunc.IsFilled(attribValue))
                        IsMandatory = BoolFunc.IsTrue(attribValue);
                }

                attrib = DynamicAttribs.Find(item => item.name == "dataType");
                if (null != attrib)
                {
                    attribValue = attrib.GetDataValue(pCS, pDbTransaction);
                    if (StrFunc.IsFilled(attribValue))
                        DataType = TypeData.GetTypeDataEnum(attribValue, true);
                }

                attrib = DynamicAttribs.Find(item => item.name == "regex");
                if (null != attrib)
                {
                    attribValue = attrib.GetDataValue(pCS, pDbTransaction);
                    if (StrFunc.IsFilled(attribValue))
                    {
                        EFSRegex.TypeRegex typeRegex = EFSRegex.TypeRegex.None;
                        if (System.Enum.IsDefined(typeof(EFSRegex.TypeRegex), attribValue))
                            typeRegex = (EFSRegex.TypeRegex)System.Enum.Parse(typeof(EFSRegex.TypeRegex), attribValue);
                        Regex = typeRegex;
                    }
                }

                if (StrFunc.IsEmpty(NewValue))
                {
                    attrib = DynamicAttribs.Find(item => item.name == "defaultValue");
                    if (null != attrib)
                    {
                        attribValue = attrib.GetDataValue(pCS, pDbTransaction);
                        if (StrFunc.IsFilled(attribValue))
                            NewValue = attribValue;
                    }
                }
            }

            DynamicAttribs = null;
            // EG 20240213 [WI756] Correctif Majuscule sur [xxx]Specified
            DynamicAttribsSpecified = false;
        }
    }
}
