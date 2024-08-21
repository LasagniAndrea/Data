#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data; 
using System.IO;
using System.Linq;  
using System.Text;


using EFS.ACommon;
using EFS.Common; 
using EFS.ApplicationBlocks.Data;
#endregion Using Directives

namespace EFS.Common
{
    /// <summary>
    /// Liste des enums
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("ExtendEnums", Namespace = "", IsNullable = false)]
    public class ExtendEnums
    {
        [System.Xml.Serialization.XmlElementAttribute("ExtendEnum")]
        public ExtendEnum[] Items;
        /// <summary>
        /// 
        /// </summary>
        public ExtendEnums() { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pItems"></param>
        public ExtendEnums(ExtendEnum[] pItems)
        {
            Items = pItems;
        }
        /// <summary>
        /// Obtient l'enum qui a pour code {pCode}
        /// </summary>
        /// <param name="pCode"></param>
        /// <returns></returns>
        public ExtendEnum this[string pCode]
        {
            get
            {
                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].Code.ToUpper() == pCode.ToUpper()) 
                        return Items[i];
                }
                return null;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCompare"></param>
        /// <returns></returns>
        public ExtendEnum[] Sort(string pCompare)
        {
            if (ArrFunc.IsFilled(Items))
                Array.Sort(Items, new CompareExtendEnum());
            return Items;
        }
    }

    /// <summary>
    ///  Représente un Enum 
    /// </summary>
    public class ExtendEnum
    {
        [System.Xml.Serialization.XmlElementAttribute("URI")]
        public string Uri;
        /// <summary>
        /// Définition de l'enum
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DEFINITION")]
        public string Definition;
        /// <summary>
        /// Documentation de l'enum
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DOCUMENTATION")]
        public string Documentation;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DEFINED")]
        public string Defined;
        /// <summary>
        /// Représente les valeurs de l'enum
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("ExtendEnumValue")]
        public ExtendEnumValue[] item;
        /// <summary>
        /// Code de l'enum
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("CODE")]
        public string Code;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("EXTCODE")]
        public string ExtCode;


        /// <summary>
        /// Obtient l'enum dont ExtValue vaut {pExtValue}
        /// </summary>
        /// <param name="pExtValue"></param>
        /// <returns></returns>
        public ExtendEnumValue this[string pExtValue]
        {
            get
            {
                for (int i = 0; i < item.Length; i++)
                {
                    if (item[i].ExtValue.ToUpper() == pExtValue.ToUpper())
                        return item[i];
                }
                return null;
            }
        }
        /// <summary>
        /// Retourne l'enum dont Value est égale à {pValue}
        /// <para>Retourne null si  {pValue} n'existe pas</para>
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public ExtendEnumValue GetExtendEnumValueByValue(string pValue)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i].Value == pValue)
                    return item[i];
            }
            return null;
        }

        /// <summary>
        /// Retourne l'enum dont ExtlLink est égale à {pValue}
        /// <para>Retourne null si  {pValue} n'existe pas</para>
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        /// FI 20120211 [] add this method
        public ExtendEnumValue GetExtendEnumValueByExtlLink(string pValue)
        {
            for (int i = 0; i < item.Length; i++)
            {
                if (item[i].ExtlLink == pValue)
                    return item[i];
            }
            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pIsWithTranslate"></param>
        /// <returns></returns>
        //PL 20111124 Newness
        public ExtendEnumValue GetExtendEnumValueStartsWithValue(string pValue, bool pIsWithTranslate)
        {
            for (int i = 0; i < item.Length; i++)
            {
                string tmpItem = item[i].Value;
                if (pIsWithTranslate)
                    tmpItem = Ressource.GetString(tmpItem, true);
                if (tmpItem.StartsWith(pValue, StringComparison.InvariantCultureIgnoreCase))
                    return item[i];
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <param name="pIsWithTranslate"></param>
        /// <returns></returns>
        public ExtendEnumValue GetExtendEnumValueIndexOfValue(string pValue, bool pIsWithTranslate)
        {
            for (int i = 0; i < item.Length; i++)
            {
                string tmpItem = item[i].Value;
                if (pIsWithTranslate)
                    tmpItem = Ressource.GetString(tmpItem, true);
                if (tmpItem.IndexOf(pValue, StringComparison.InvariantCultureIgnoreCase) >= 0)
                    return item[i];
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pValue"></param>
        /// <returns></returns>
        public ExtendEnumValue GetExtendEnumValueForCriteria(string pValue)
        {
            ExtendEnumValue ret = GetExtendEnumValueStartsWithValue(pValue, true);
            if (ret == null)
                ret = this.GetExtendEnumValueIndexOfValue(pValue, true);
            return ret;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCompare"></param>
        /// <returns></returns>
        public ExtendEnumValue[] Sort(string pCompare)
        {
            if (item != null)
            {
                switch (pCompare)
                {
                    case "Value":
                        Array.Sort(item, new CompareExtendEnumValue());
                        break;
                    case "ExtValue":
                        Array.Sort(item, new CompareExtendEnumValueExtValue());
                        break;
                }
            }
            return item;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CompareExtendEnum : IComparer
    {
        public int Compare(object pEnum1, object pEnum2)
        {
            ExtendEnum enum1 = (ExtendEnum)pEnum1;
            ExtendEnum enum2 = (ExtendEnum)pEnum2;
            return (enum1.Code.CompareTo(enum2.Code));
        }
    }

    /// <summary>
    /// Représente une valeur issue d'un enum
    /// </summary>
    public class ExtendEnumValue
    {
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("SOURCE")]
        public string Source;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("DOCUMENTATION")]
        public string Documentation;
        /// <summary>
        /// valeur de l'enum
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("VALUE")]
        public string Value;
        /// <summary>
        /// valeur étendue de l'enum 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("EXTVALUE")]
        public string ExtValue;

        /// <summary>
        /// valeur additionnelle customizable de l'enum 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("CUSTOMVALUE")]
        public string CustomValue;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("FORECOLOR")]
        public string ForeColor;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("BACKCOLOR")]
        public string BackColor;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("EXTLLINK")]
        public string ExtlLink;
    }

    /// <summary>
    /// 
    /// </summary>
    public class CompareExtendEnumValue : IComparer
    {
        public int Compare(object pEnumValue1, object pEnumValue2)
        {
            ExtendEnumValue enumValue1 = (ExtendEnumValue)pEnumValue1;
            ExtendEnumValue enumValue2 = (ExtendEnumValue)pEnumValue2;
            if (pEnumValue1 == null || pEnumValue2 == null)
                return 0;
            else
                return (enumValue1.Value.CompareTo(enumValue2.Value));
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class CompareExtendEnumValueExtValue : IComparer
    {
        public int Compare(object pEnumValue1, object pEnumValue2)
        {
            ExtendEnumValue enumValue1 = (ExtendEnumValue)pEnumValue1;
            ExtendEnumValue enumValue2 = (ExtendEnumValue)pEnumValue2;
            if (pEnumValue1 == null || pEnumValue2 == null)
                return 0;
            else
                return (enumValue1.ExtValue.CompareTo(enumValue2.ExtValue));
        }
    }

    /* FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
    /// <summary>
    /// 
    /// </summary>
    /// EG 20170929 [23450][22374] Suppression Timezone
    public sealed class ExtendEnumsTools
    {
        #region Members
        private static ExtendEnums m_ListEnumsSchemes;
        private static bool m_IsExtendEnumsLoaded = false;
        private static string m_cs = string.Empty;
        private static DateTime m_dtLastRead = new DateTime();
        #endregion

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public static ExtendEnums ListEnumsSchemes
        {
            get
            {
                return m_ListEnumsSchemes;
            }
        }
        #endregion

        #region Members
        /// <summary>
        /// Chargement des Enums/Schemes
        /// <para>Les données sont mise ds le cache SQL sauf si indication contraire (présence de l'extention SpheresCache=false ds {pCS})</para>
        /// <para>Si cache SQL est non actif, le chargement s'opère uniquement si le dernier chargement date de plus de 5mn (cache natif à classe)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20120211 [] add EXTLLINK sur le chgt des enums (Evite des select dans l'import des trades)
        public static void LoadFpMLEnumsAndSchemes(string pCS)
        {
            //Mise en cache, si (web.config) le permet
            string cs = CSTools.SetCacheOn(pCS, -1, -1);
            CSManager csManager = new CSManager(cs);

            if (BoolFunc.IsTrue(csManager.IsUseCache))
            {
                //=> Spheres® rentre ds le ExecuteDataset, 
                //le select ne sera exécuté uniquement si les données ne sont pas présentes dans le cache
                m_IsExtendEnumsLoaded = false;
            }

            if (m_IsExtendEnumsLoaded && (m_cs != csManager.Cs))
            {
                //Changement de source => Reload des enums
                m_IsExtendEnumsLoaded = false;
            }

            if (m_IsExtendEnumsLoaded)
            {
                DateTime dtCurrent = SystemTools.GetOSDateSys();
                TimeSpan tsTmp = dtCurrent.Subtract(m_dtLastRead);
                if (tsTmp > new TimeSpan(0, 5, 0))
                {
                    //Même source, mais dernière lecture datant de plus de 5mn => Reload des enums
                    m_IsExtendEnumsLoaded = false;
                }
            }

            if (!m_IsExtendEnumsLoaded)
            {
                #region Query
                // VW_ENUMS
                StrBuilder SQLQuery = new StrBuilder();
                SQLQuery += SQLCst.SELECT + "CODE,EXTCODE,URI,DEFINITION,DOCUMENTATION,DEFINED" + Cst.CrLf;
                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_ENUMS;
                SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                
                // VW_ENUM union STMATCH union STCHECK 
                SQLQuery += SQLCst.SELECT + "CODE, VALUE, EXTVALUE, CUSTOMVALUE, SOURCE, DOCUMENTATION, FORECOLOR, BACKCOLOR, EXTLLINK" + Cst.CrLf;
                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_VW_ENUM + Cst.CrLf;
                SQLQuery += SQLCst.UNIONALL + Cst.CrLf;
                SQLQuery += SQLCst.SELECT + "'StatusMatch' as CODE, IDSTMATCH as VALUE, DISPLAYNAME as EXTVALUE, CUSTOMVALUE, " + Cst.CrLf;
                SQLQuery += "'OTCml' as SOURCE, LONOTE as DOCUMENTATION, FORECOLOR, BACKCOLOR, EXTLLINK" + Cst.CrLf;
                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STMATCH + Cst.CrLf;
                SQLQuery += SQLCst.UNIONALL + Cst.CrLf;
                SQLQuery += SQLCst.SELECT + "'StatusCheck' as CODE, IDSTCHECK as VALUE, DISPLAYNAME as EXTVALUE, CUSTOMVALUE, " + Cst.CrLf;
                SQLQuery += "'OTCml' as SOURCE, LONOTE as DOCUMENTATION, FORECOLOR, BACKCOLOR, EXTLLINK" + Cst.CrLf;
                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.STCHECK;
                SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
                #endregion Query

                DataSet dsEnums = DataHelper.ExecuteDataset(cs, CommandType.Text, SQLQuery.ToString());
                dsEnums.DataSetName = "ExtendEnums";

                DataTable dtExtendEnum = dsEnums.Tables[0];
                dtExtendEnum.TableName = "ExtendEnum";

                DataTable dtExtendEnumValue = dsEnums.Tables[1];
                dtExtendEnumValue.TableName = "ExtendEnumValue";

                DataRow row = dtExtendEnum.NewRow();
                row["CODE"] = "StatusMatch";
                row["EXTCODE"] = "MatchStatus";
                row["DEFINITION"] = "MatchStatus";
                row["DOCUMENTATION"] = "A match status";
                row["DEFINED"] = "OTCml";
                dtExtendEnum.Rows.Add(row);

                row = dtExtendEnum.NewRow();
                row["CODE"] = "StatusCheck";
                row["EXTCODE"] = "CheckStatus";
                row["DEFINITION"] = "CheckStatus";
                row["DOCUMENTATION"] = "A check status";
                row["DEFINED"] = "OTCml";
                dtExtendEnum.Rows.Add(row);

                #region Mapping
                dtExtendEnum.Columns["CODE"].ColumnMapping = MappingType.Attribute;
                dtExtendEnum.Columns["EXTCODE"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["CODE"].ColumnMapping = MappingType.Hidden;
                dtExtendEnumValue.Columns["VALUE"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["EXTVALUE"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["CUSTOMVALUE"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["FORECOLOR"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["BACKCOLOR"].ColumnMapping = MappingType.Attribute;
                dtExtendEnumValue.Columns["EXTLLINK"].ColumnMapping = MappingType.Attribute;
                #endregion Mapping
                #region Relation
                if (ArrFunc.IsEmpty(dsEnums.Relations) || (null == dsEnums.Relations["Enums_Enum"]))
                {
                    DataRelation rel = new DataRelation("Enums_Enum", dtExtendEnum.Columns["CODE"], dtExtendEnumValue.Columns["CODE"], false)
                    {
                        Nested = true
                    };
                    dsEnums.Relations.Add(rel);
                }
                #endregion Relation

                #region Deserialize
                TextWriter writer = null;
                try
                {
                    StringBuilder sb = new StringBuilder();
                    writer = new StringWriter(sb);
                    dsEnums.WriteXml(writer);
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ExtendEnums), sb.ToString());
                    m_ListEnumsSchemes = (ExtendEnums)CacheSerializer.Deserialize(serializeInfo);

                }
                catch
                {
                    throw;
                }
                finally
                {
                    if (null != writer)
                        writer.Close();
                }
                #endregion Deserialize

                m_cs = csManager.Cs;
                m_dtLastRead = SystemTools.GetOSDateSys();
                m_IsExtendEnumsLoaded = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void ResetFpMLEnumsAndSchemes()
        {
            m_cs = string.Empty;
            m_dtLastRead = DateTime.MinValue;
            m_ListEnumsSchemes = null;
            m_IsExtendEnumsLoaded = false;
        }
        #endregion
    }
    */
}
