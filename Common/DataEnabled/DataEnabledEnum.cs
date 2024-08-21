using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;

namespace EFS.Common
{
    /// <summary>
    /// Représente, en mémoire, les enums 
    /// </summary>
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.ENUM)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.ENUMS)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.STMATCH)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.STCHECK)]
    [Cst.DependsOnTableAttribute(Table = Cst.OTCml_TBL.EVENTENUM)]
    public class DataEnabledEnum : DataEnabledBase
    {

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static string GetKey()
        {
            return new DataEnabledEnum().GetType().Name;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string Key => GetKey();


        /// <summary>
        /// 
        /// </summary>
        public DataEnabledEnum() : base()
        {

        }

        /// <summary>
        /// Représente, en mémoire, les enums
        /// </summary>
        /// <param name="cs"></param>
        public DataEnabledEnum(string cs) : base(cs, null, DateTime.MinValue)
        {

        }


        /// <summary>
        /// Chargement des enums
        /// </summary>
        protected override void LoadData()
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

            DataSet dsEnums = DataHelper.ExecuteDataset(this.CS, CommandType.Text, SQLQuery.ToString());
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


            StringBuilder sb = new StringBuilder();
            using (TextWriter writer = new StringWriter(sb))
            {
                dsEnums.WriteXml(writer);
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ExtendEnums), sb.ToString());
                ExtendEnums m_ListEnumsSchemes = (ExtendEnums)CacheSerializer.Deserialize(serializeInfo);
                SetData<ExtendEnum[]>(m_ListEnumsSchemes.Items);
            }

        }

        /// <summary>
        ///  Retourne les enums
        /// </summary>
        /// <returns></returns>
        public ExtendEnum[] GetData()
        {
            return base.GetData<ExtendEnum[]>();
        }


        /// <summary>
        /// Suppresssion des Enum du cache en relation avec la base de donnée <paramref name="cs"/> 
        /// </summary>
        /// <param name="cs"></param>
        public static new int ClearCache(string cs)
        {
            return ClearCache(cs, GetKey());
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public static class DataEnabledEnumHelper
    {

        /// <summary>
        ///  Retourne l'Enum avec le code <paramref name="code"/>. Retouren null si l'enum n'existe pas
        /// </summary>
        /// <param name="cs"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public static ExtendEnum GetDataEnum(string cs, string code)
        {
            ExtendEnum[] dataEnum = new DataEnabledEnum(cs).GetData();
            return Array.Find<ExtendEnum>(dataEnum, (x => x.Code.ToUpper() == code.ToUpper()));
        }
    }
}
