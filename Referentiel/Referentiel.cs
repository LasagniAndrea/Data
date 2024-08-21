#region using directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Restriction;
using EFS.SpheresIO;
using EFS.Syndication;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
#endregion using directives


namespace EFS.Referential
{

    /// <summary>
    /// Classe encapsulant les méthodes et propriétés d'une colonne du referentiel dans le Script SQL
    /// </summary>
    public class SQLReferentialColumn
    {
        #region Members
        private ReferentialsReferentialColumn _rrc;

        private string _sqlColumnName;
        private string _sqlColumnNameOrSQLColumnSQLSelect;
        private string _sqlColumnAlias;
        private string _sqlColumnOrderBy;

        #endregion

        #region Constructor(s)
        public SQLReferentialColumn()
        { }
        #endregion Constructor(s)

        #region Accessors
        #region SQLColumnNameOrSQLColumnSQLSelect
        /// <summary>
        /// Obtient l'expression SQL de la colonne ({alias}+{ColumName}) ou bien l'expression SQLSelect de la colonne telle qu'elle existe dans LSTCOLUNM.SQLSELECT
        /// </summary>     
        public string SQLColumnNameOrSQLColumnSQLSelect
        {
            get { return _sqlColumnNameOrSQLColumnSQLSelect; }
        }
        #endregion
        #region SQLSelect
        /// <summary>
        /// Obtient SQLColumnNameOrSQLColumnSQLSelect auquel Spheres® a joute l'alias
        /// </summary>        
        public string SQLSelect
        {
            // RD 20091214 / 16802/ LSTCOLUMN.SQLSELECT
            get { return SQLColumnNameOrSQLColumnSQLSelect + SQLCst.AS + SqlColumnAlias; }
        }
        #endregion

        /// <summary>
        /// Obtient une l'expression SQL de la colonne {alias}+{ColumName} 
        /// </summary>        
        public string SqlColumnName
        {
            get { return _sqlColumnName; }
        }
        /// <summary>
        /// Obtient l'alias de la colonne
        /// </summary>
        public string SqlColumnAlias
        {
            get { return _sqlColumnAlias; }
        }
        /// <summary>
        /// Obtient l'order by asscié à la colonne
        /// </summary>
        public string SqlColumnOrderBy
        {
            get { return _sqlColumnOrderBy; }
        }
        #endregion Accessors

        #region public ConstructSqlColumn
        public void ConstructSqlColumn(string pCS, ReferentialsReferentialColumn pRrc, bool pIsWithOrderBy, bool pIsUseColumnAlias, bool pIsUseColumnAliasInOrderExpression)
        {
            _rrc = pRrc;

            if (_rrc.ColumnName == Cst.OTCml_COL.ROWVERSION.ToString())
            {
                // RD 20091102 / Utilisation de sqlColumn
                // RD 20091223 / 16802/ LSTCOLUMN.SQLSELECT / Correction
                SetSqlColumnInfo(DataHelper.GetROWVERSION(pCS, _rrc.AliasTableName, string.Empty), _rrc.DataField.ToUpper(), _rrc.AliasTableName);
            }
            else if (_rrc.ColumnName == Cst.OTCml_COL.DTHOLIDAYNEXTDATE.ToString())
            {
                //PL 20120116 Newness: DTHOLIDAYNEXTDATE, colonne fictive associée à la colonne DTHOLIDAYVALUE
                SetSqlColumnInfo(DataHelper.SQLToDate(pCS, SQLCst.NULL), _rrc.ColumnName);
            }
            else
            {

                SetSqlColumnInfo(_rrc.ColumnName, _rrc.DataField.ToUpper(), _rrc.AliasTableName);
                if (_rrc.ColumnNameOrColumnSQLSelectSpecified)
                    _sqlColumnNameOrSQLColumnSQLSelect = _rrc.ColumnNameOrColumnSQLSelect;
            }

            if (pIsWithOrderBy)
            {
                if (_rrc.IsOrderBySpecified && _rrc.IsOrderBy.orderSpecified)
                {
                    _sqlColumnOrderBy = _rrc.IsOrderBy.order.Replace(Cst.DYNAMIC_ALIASTABLE, _rrc.AliasTableName);
                    //
                    if (pIsUseColumnAliasInOrderExpression)
                    {
                        _sqlColumnOrderBy = _sqlColumnOrderBy.Replace(_sqlColumnName, _sqlColumnAlias);
                        // 20110308 EG Correction Test sur Alias defini (tag AliasTableName) !!!
                        //_sqlColumnOrderBy = _sqlColumnOrderBy.Replace(SQLCst.TBLMAIN + ".", string.Empty);
                        string aliasTableName = SQLCst.TBLMAIN;
                        if (_rrc.AliasTableNameSpecified)
                            aliasTableName = _rrc.AliasTableName;
                        _sqlColumnOrderBy = _sqlColumnOrderBy.Replace(aliasTableName + ".", string.Empty);
                    }
                }
                else
                {
                    SetSqlColumnOrderBy(pIsUseColumnAlias);
                }
            }

        }
        public void ConstructSqlColumn(ReferentialsReferentialSQLOrderBy pRRSQLOrderBy)
        {
            SetSqlColumnInfo(pRRSQLOrderBy.ColumnName, string.Empty, pRRSQLOrderBy.Alias);
            _sqlColumnNameOrSQLColumnSQLSelect = SQLReferentialData.GetColumnNameOrColumnSelect(pRRSQLOrderBy);

            if (pRRSQLOrderBy.ColumnNameOrColumnSQLSelectSpecified)
                _sqlColumnNameOrSQLColumnSQLSelect = pRRSQLOrderBy.ColumnNameOrColumnSQLSelect;
        }
        #endregion ConstructSqlColumn

        #region public SetSqlColumnOrderBy
        /// <summary>
        /// Valorise sqlColumnOrderBy avec l'alias de la colonne ou l'expression de la colonne
        /// </summary>
        /// <param name="pIsWithColumnAlias">Si true, utilise l'alias de la colonne</param>        
        public void SetSqlColumnOrderBy(bool pIsWithColumnAlias)
        {
            if (pIsWithColumnAlias)
                _sqlColumnOrderBy = SqlColumnAlias;
            else
                _sqlColumnOrderBy = SQLColumnNameOrSQLColumnSQLSelect;

        }
        #endregion

        #region public SetSqlColumnInfo
        /// <summary>
        /// Valorise les propriétés sqlColumnName et sqlColumnAlias et sqlColumnNameOrSQLColumnSQLSelect
        /// </summary>
        /// <param name="pSqlColumnName">Nom de la colonne dans une table, à utiliser dans les différentes Clauses de la requête SQL</param>
        /// <param name="pSqlColumnAlias">Alias de la colonne dans une table, à utiliser dans les différentes Clauses de la requête SQL</param>
        public void SetSqlColumnInfo(string pSqlColumnName, string pSqlColumnAlias)
        {
            _sqlColumnName = pSqlColumnName;
            _sqlColumnAlias = pSqlColumnAlias;
            _sqlColumnNameOrSQLColumnSQLSelect = SqlColumnName;
        }
        //
        /// <summary>
        /// Valorise les propriétés sqlColumnName et sqlColumnAlias et sqlColumnNameOrSQLColumnSQLSelect
        /// </summary>
        public void SetSqlColumnInfo(string pColumnName, string pColumnAlias, string pAliasTableName)
        {
            //PL 20120116
            _sqlColumnName = SQLReferentialData.GetColumnNameExpression(pColumnName, pAliasTableName);

            if (StrFunc.IsFilled(pColumnAlias))
                _sqlColumnAlias = pColumnAlias;
            else
                _sqlColumnAlias = pAliasTableName + "_" + pColumnName;

            _sqlColumnNameOrSQLColumnSQLSelect = SqlColumnName;
        }
        #endregion

        #region public GetSqlColumnName_CaseWhenIsNull
        /// <summary>
        /// Ajouter un "case when" pour retourner une valeur en dur en cas ou la valeur de la colonne n'est pas Null
        /// </summary>
        /// <param name="pSqlSubstituteIfNotNull">La chaine de caractère de substitution dans le cas ou la valeur de la colonne n'est pas Null</param>
        public string GetSqlColumnName_CaseWhenIsNull(string pSqlSubstituteIfNotNull)
        {
            string ret = SQLCst.CASE + SQLCst.CASE_WHEN + SQLColumnNameOrSQLColumnSQLSelect + SQLCst.IS_NULL + SQLCst.CASE_THEN + SQLCst.NULL;
            ret += SQLCst.CASE_ELSE + DataHelper.SQLString(pSqlSubstituteIfNotNull) + SQLCst.CASE_END;
            ret += SQLCst.AS + SqlColumnAlias;
            //
            return ret;
        }
        #endregion

        #region public GetSqlGroupBy
        /// <summary>
        /// Retourne les Scripts SQL du Group By incluant la colonne.
        /// Et celà en gérant le cas des ruptures multiples, en respectant le schéma suivant :
        /// 
        /// select sql_select from sql_from
        /// union all
        /// select sql_SelectGBFirst from (sql_SelectGB1 from sql_from group by sql_GroupBy1)
        /// union all
        /// select sql_SelectGBFirst from (sql_SelectGB2 from sql_from group by sql_GroupBy2)
        /// ...
        /// order by sql_orderby
        /// </summary> 
        /// <param name="pSql_SelectGBFirst"></param>
        /// <param name="pSql_SelectGB"></param>
        /// <param name="pSql_GroupBy"></param>
        public void GetSqlGroupBy(ref string pSql_SelectGBFirst, ref string pSql_SelectGB, ref string pSql_GroupBy)
        {
            if (null == _rrc)
                throw new Exception("ReferentialsReferentialColumn is not Specified");
            //
            GetSqlGroupBy(_rrc.GroupBySpecified, _rrc.GroupBy, ref pSql_SelectGBFirst, ref pSql_SelectGB, ref pSql_GroupBy);

        }
        public void GetSqlGroupBy(ReferentialsReferentialColumnGroupBy pRrc_GroupBy,
            ref string pSql_SelectGBFirst, ref string pSql_SelectGB, ref string pSql_GroupBy)
        {
            GetSqlGroupBy(true, pRrc_GroupBy, ref pSql_SelectGBFirst, ref pSql_SelectGB, ref pSql_GroupBy);
        }
        private void GetSqlGroupBy(bool pIsRrc_GroupBySpecified, ReferentialsReferentialColumnGroupBy pRrc_GroupBy,
            ref string pSql_SelectGBFirst, ref string pSql_SelectGB, ref string pSql_GroupBy)
        {
            // RD 20091102 / Utilisation de sqlColumn
            bool isRrcGroupBy = pIsRrc_GroupBySpecified && pRrc_GroupBy.IsGroupBy;
            bool isRrcSqlGroupBy = pIsRrc_GroupBySpecified && pRrc_GroupBy.SqlGroupBySpecified;
            bool isRrcAggregate = pIsRrc_GroupBySpecified && pRrc_GroupBy.AggregateSpecified;
            //
            if (isRrcSqlGroupBy && (!isRrcAggregate))
                pSql_SelectGBFirst += "(" + pRrc_GroupBy.SqlGroupBy + ")" + SQLCst.AS;
            //
            pSql_SelectGBFirst += SqlColumnAlias + ",";
            //
            if (isRrcAggregate)
            {
                //PL 20100210 Add WAVG (NB: Si différent de SUM ou AVG, alors SqlGroupBy contient toute la syntaxe SQL)
                string aggregate = pRrc_GroupBy.Aggregate;
                //PL 20110228
                bool isSUMorAVG = aggregate.ToUpper() == "SUM" || aggregate.ToUpper() == "AVG";
                bool isOldSyntaxeForSUMorAVG = false;
                if (isSUMorAVG)
                {
                    //NB: Avec l'ancienne syntaxe, la colonne SQLGROUPBY ne contient pas toute la syntaxe SQL
                    isOldSyntaxeForSUMorAVG = !pRrc_GroupBy.SqlGroupBy.StartsWith(aggregate);
                }
                //
                if (isOldSyntaxeForSUMorAVG)
                    pSql_SelectGB += aggregate + "(";
                //
                if (isRrcSqlGroupBy)
                    pSql_SelectGB += pRrc_GroupBy.SqlGroupBy;
                else
                    pSql_SelectGB += SQLColumnNameOrSQLColumnSQLSelect;
                //
                if (isOldSyntaxeForSUMorAVG)
                    pSql_SelectGB += " )";
                //
                // RD 20110302 
                // Pour permettre d'utiliser des colonnes dans la clause Group by, même si celle ci possède une fonction d'agrégation
                if (isRrcGroupBy)
                    //pSql_GroupBy += SQLColumnNameOrSQLColumnSQLSelect + ",";
                    pSql_GroupBy += String.Format("{{{0}}},", SQLColumnNameOrSQLColumnSQLSelect);
            }
            else if (isRrcGroupBy)
            {
                //pSql_GroupBy += SQLColumnNameOrSQLColumnSQLSelect + ",";
                pSql_GroupBy += String.Format("{{{0}}},", SQLColumnNameOrSQLColumnSQLSelect);

                pSql_SelectGB += SQLColumnNameOrSQLColumnSQLSelect;
            }
            else
                pSql_SelectGB += SQLCst.NULL;
            //
            pSql_SelectGB += SQLCst.AS + SqlColumnAlias + ",";
        }
        public void GetSqlGroupBy(ref string pSql_SelectGBFirst, ref string pSql_SelectGB)
        {
            pSql_SelectGBFirst += SqlColumnAlias;
            pSql_SelectGB += SQLSelect + ", ";
        }
        public void GetSqlGroupBy(string pSqlSelect, ref string pSql_SelectGBFirst, ref string pSql_SelectGB)
        {
            pSql_SelectGBFirst += SqlColumnAlias;
            pSql_SelectGB += pSqlSelect + ", ";
        }
        #endregion
    }


    /// <summary>
    /// Classe regroupant les methodes statiques employées pour le referenciel (classe referential) (en mode form ou list)
    /// </summary>
    public class ReferentialTools
    {
        #region const
        public const string SuffixEdit = "EDIT";
        public const string SuffixAggregate = "AGGREGATE";
        #endregion

        #region Methods

        #region public DeserializeXML_ForModeRW

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pType"></param>
        /// <param name="pObjectName"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pValueFK">value ForeignKeyField</param>
        /// <param name="opReferentialsReferential"></param>
        /// PL 20120823 Use by SQLExport 
        /// FI 20141211 [20563] Modification Signature add pDynamicArgs
        /// FI 20200205 [XXXXX] pDynamicArgs est de type Dictionary<string, ReferentialsReferentialStringDynamicData>
        /// FI 20201215 [XXXXX] Add pValueFK
        public static void DeserializeXML_ForModeRW_25(string pCS,Cst.ListType pType, string pObjectName,
            string pCondApp, string[] pParam, Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, string pValueFK,
            out ReferentialsReferential opReferentialsReferential)
        {
            DeserializeXML_ForModeRW(pCS,  pType, pObjectName, pCondApp, pParam, pDynamicArgs, pValueFK, out opReferentialsReferential);

            //PL 20120823 Add CmptLevel = "2.5", afin de forcer le chargement des colonnes.
            opReferentialsReferential.CmptLevelSpecified = true;
            opReferentialsReferential.CmptLevel = "2.5";
        }
        /// <summary>
        /// Renseigne une classe referential à partir d'un fichier XML de type "Repository"
        /// </summary>
        /// <param name="pCS">chaine pour la connexion SGBD</param>
        /// <param name="pType">type de consultation</param>
        /// <param name="pObjectName">object xml utilisé pour la Deserialization</param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pValueFK">value ForeignKeyField</param>
        /// <param name="opReferentialsReferential">OUT classe referential à renseigner</param>
        /// FI 20141211 [20563] Modification Signature add pDynamicArgs
        /// FI 20200205 [XXXXX] pDynamicArgs est de type Dictionary<string, ReferentialsReferentialStringDynamicData>
        /// FI 20201215 [XXXXX] Add pValueFK
        public static void DeserializeXML_ForModeRW(string pCS, Cst.ListType pType, string pObjectName,
            string pCondApp, string[] pParam, Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, string pValueFK,
            out ReferentialsReferential opReferentialsReferential)
        {
            List<string> lstObject = new List<string>
            {
                pObjectName
            };
            // FI 20201215 [XXXXX] Alimentation du paramètre pValueFK
            DeserializeXML_ForModeRW(pCS, pType, lstObject, pCondApp, pParam, pDynamicArgs, pValueFK, out opReferentialsReferential);
        }


        /// <summary>
        /// Renseigne une classe referential à partir d'un fichier XML de type "Repository"
        /// </summary>
        /// <param name="pCS">chaine pour la connexion SGBD</param>
        /// <param name="pType">type de consultation</param>
        /// <param name="pObjectName">Liste des object xml utilisés pour la Deserialization, priorité est donné au 1er item pour lequel il existe un fichier xml </param>
        /// <param name="pCondApp">condition d'application si nécessaire</param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pValueFK">value ForeignKeyField</param>
        /// <param name="opReferentialsReferential">OUT classe referential à renseigner</param>
        /// <param name="pIsFormCalling">true si le référentiel est chargé pour afficher la page (et non pas le Grid)ForeignKeyField</param>        
        /// EG 20141020 [20442] Add GContractRole for DC to Invoicing context
        /// FI 20141211 [20563] Modofication de la signature add pDynamicArgs
        /// FI 20160804 [Migration TFS] Modify
        /// EG 20180423 Analyse du code Correction [CA2202]
        /// EG 20180426 Analyse du code Correction [CA2202]
        /// FI 20200205 [XXXXX] pDynamicArgs est de type Dictionary<string, ReferentialsReferentialStringDynamicData>
        /// FI 20201215 [XXXXX] Add pValueFK
        // EG 20210304 [XXXXX] Relooking Referentiels
        // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du startdisplay et containerOverflow sur Html_BLOCK
        // EG 20231114 [WI736] Ajout pIsFormCalling indiquant chargement du repository en mode Form ou en Mode Grid
        public static void DeserializeXML_ForModeRW(string pCS, Cst.ListType pType, List<string> pObjectName,
             string pCondApp, string[] pParam, Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, string pValueFK,
             out ReferentialsReferential opReferentialsReferential, bool pIsFormCalling = false)
        {
            string xmlFile = string.Empty;
            string objectName = pObjectName[0];

            //Identification du fichier XML contenant le descriptif du référentiel 
            for (int j = 0; j < ArrFunc.Count(pObjectName); j++)
            {
                //PL 20160725 Newness 
                string folder = pType.ToString();
                string filename = pObjectName[j];
                if (filename.IndexOf(":") > 0)
                {
                    //Gestion du cas particulier d'un fichier XML utilisé sur plusieurs consultation, donc où le Folder n'est pas équivalent au "Type".
                    //ex. List.aspx?Trade=Accounting:ACCDAYBOOK
                    folder = filename.Substring(0, filename.IndexOf(":"));
                    filename = filename.Substring(filename.IndexOf(":") + 1);
                }


                // FI 20160804 [Migration TFS] Call SessionTools.NewAppInstance().SearchFile
                //xmlFile = GetXMLFile(folder, filename);
                // FI 20191227 [XXXXX] Lecture de la propertie AppInstance (Nécessaire du fait que la méthode peut être appelée dans le cadre d'un webservice
                SessionTools.AppSession.AppInstance.SearchFile2(pCS, StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.XML", folder, filename), ref xmlFile);
                if (StrFunc.IsFilled(xmlFile))
                {
                    objectName = filename;
                    break;
                }
            }

            bool isTablePreviousImage = objectName.EndsWith("_P");
            if (isTablePreviousImage)
                objectName = GetTableNameForReferential(objectName);

            bool isFound = File.Exists(xmlFile);

            if ((false == isFound) && DataHelper.IsDbSqlServer(pCS))
            {
                #region MS SQLServer: Call Stored Procedure for auto generate the xml file
                xmlFile = HttpContext.Current.Server.MapPath(@"Temporary/" + objectName + ".xml");
                //
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "OBJECTNAME", DbType.AnsiString, 255), objectName);
                DataHelper.ExecuteNonQuery(pCS, CommandType.StoredProcedure, "dbo.SQLTOXML", parameters.GetArrayDbParameter());
                FileStream fileStream = new FileStream(xmlFile, FileMode.OpenOrCreate);
                using (StreamWriter streamWriter = new StreamWriter(fileStream))
                {
                    using (IDataReader drSQLTOXML =
                        DataHelper.ExecuteReader(pCS, CommandType.Text, "select SQLTOXML from dbo.TMP_SQLTOXML order by IDSQLTOXML"))
                    {
                        while (drSQLTOXML.Read())
                        {
                            isFound = true;
                            streamWriter.WriteLine(drSQLTOXML.GetValue(0).ToString());
                        }
                    }

                }
                #endregion
            }

            if (false == isFound)
                throw new FileNotFoundException($"XML file not found for object: {pObjectName[0]}", xmlFile);
            
            using (StreamReader streamReader = new StreamReader(xmlFile))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Referentials));
                Referentials referentials = (Referentials)xmlSerializer.Deserialize(streamReader);
                if (isTablePreviousImage)
                {
                    //S'il s'agit d'un référentiel "_P" (previous image), alors on utilise le fichier XML de la table principale 
                    //auquel on rajoutera dynamiquement les N colonnes d'audit (ACTION_, DTSYS_, xxx_, ...)
                    referentials[objectName].TableName = referentials[objectName].TableName + "_P";
                }

                // FI 20141211 [20563] paramètre pDynamicArgs
                // FI 20201215 [XXXXX] Alimentation du paramètre pValueFK
                // EG 20231114 [WI736] Mise à jour IsForm avec paramètre pIsFormCalling
                referentials[objectName].IsForm = pIsFormCalling;
                referentials[objectName].Initialize(false, pCondApp, pParam, pDynamicArgs, pValueFK);
                opReferentialsReferential = referentials[objectName];
            }

            string SQLSelect;
            int i = -1;

            #region Add Additionnal Column (EXTLID, ROLE)
            bool isExistExternalData = false;
            ArrayList aObjects = new ArrayList();

            #region Add Additionnal Column from DEFINEEXTLID
            //PL 20120822
            if (!opReferentialsReferential.HideExtlId)
            {
                SQLSelect = SQLCst.SELECT + @"TABLENAME,IDENTIFIER,DISPLAYNAME,ISMANDATORY,REGULAREXPRESSION,LISTRETRIEVAL,DATATYPE,DEFAULTVALUE,ISHIDEINDATAGRID,";
                //if (AspTools.IsDBRevisionEqualOrGreater(3))
                SQLSelect += @"HELPTYPE,HELPDISPLAY,HELPMESSAGE" + Cst.CrLf;
                //else
                //  SQLSelect += @"null as HELPTYPE,null as HELPDISPLAY,null as HELPMESSAGE" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.DEFINEEXTLID.ToString() + Cst.CrLf;
                //PL/MF 20100708 
                //SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(objectName) + Cst.CrLf;
                //PL 20111027 Cas particulier de VW_BOOK_VIEWER
                if (opReferentialsReferential.TableName == Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString())
                    SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(Cst.OTCml_TBL.BOOK.ToString()) + Cst.CrLf;
                // RD 20161121 [22619] Use ExtlIDTableName if defined
                else if (opReferentialsReferential.ExtlIDTableNameSpecified)
                    SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(opReferentialsReferential.ExtlIDTableName) + Cst.CrLf;
                else
                    SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(opReferentialsReferential.TableName) + Cst.CrLf;
                SQLSelect += SQLCst.ORDERBY + "SEQUENCENO" + SQLCst.ASC;
                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, SQLSelect))
                {
                    i = -1;
                    while (dr.Read())
                    {
                        i++;
                        #region While
                        if (!isExistExternalData)
                        {
                            isExistExternalData = true;
                            for (int index = 0; index < opReferentialsReferential.Column.Length; index++)
                            {
                                aObjects.Add(((System.Array)opReferentialsReferential.Column).GetValue(index));
                            }
                        }
                        //
                        ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn();
                        string tableNameCust = dr["TABLENAME"].ToString();
                        string identifier = dr["IDENTIFIER"].ToString();
                        string displayName = dr["DISPLAYNAME"].ToString();
                        string regularExpression = dr["REGULAREXPRESSION"].ToString();
                        string listRetrieval = dr["LISTRETRIEVAL"].ToString();
                        string dataType = dr["DATATYPE"].ToString();
                        string defaultValue = dr["DEFAULTVALUE"].ToString();
                        bool isHideInDatagrid = Convert.ToBoolean(dr["ISHIDEINDATAGRID"]);
                        bool isMandatory = Convert.ToBoolean(dr["ISMANDATORY"]);
                        string helpType = (dr["HELPTYPE"] is DBNull ? string.Empty : dr["HELPTYPE"].ToString());
                        string helpDisplay = (dr["HELPDISPLAY"] is DBNull ? string.Empty : dr["HELPDISPLAY"].ToString());
                        string helpMessage = (dr["HELPMESSAGE"] is DBNull ? string.Empty : dr["HELPMESSAGE"].ToString());

                        if (i == 0)
                        {
                            rrc.html_BLOCK = new ReferentialsReferentialColumnhtml_BLOCK[1];
                            rrc.html_BLOCK[0] = new ReferentialsReferentialColumnhtml_BLOCK
                            {
                                columnbyrowSpecified = true,
                                columnbyrow = 1,
                                title = Ressource.GetString("ExternalInfos", true),
                                InformationSpecified = false
                            };
                        }
                        rrc.IsExternal = true;
                        rrc.ExternalFieldID = i;
                        //PL 20100716 (see also PL/MF 20100708)
                        //rrc.ExternalTableName = objectName;
                        // RD 20161121 [22619] Use ExtlIDTableName if defined
                        //rrc.ExternalTableName = opReferentialsReferential.TableName;
                        if (opReferentialsReferential.ExtlIDTableNameSpecified)
                            rrc.ExternalTableName = opReferentialsReferential.ExtlIDTableName;
                        else
                            rrc.ExternalTableName = opReferentialsReferential.TableName;

                        rrc.ExternalIdentifier = identifier;
                        rrc.ColumnName = "VALUE";
                        rrc.AliasTableNameSpecified = true;
                        //20060925 PL Suite à pb de taille sous Oracle
                        //rrc.AliasTableName            = "EXTLID_" + tableNameCust + identifier;
                        //20111227 PL Toujours suite à pb de taille sous Oracle
                        //rrc.AliasTableName = "ex_" + tableNameCust.Substring(0, 1).ToLower() + identifier;
                        rrc.AliasTableName = "e_" + tableNameCust.Substring(0, 1).ToLower() + identifier;
                        rrc.DataField = rrc.ColumnName;
                        rrc.Ressource = displayName;
                        rrc.RessourceSpecified = true;
                        rrc.RegularExpression = (regularExpression.Length > 0 ? regularExpression : rrc.RegularExpression);
                        rrc.IsMandatorySpecified = true;
                        rrc.IsMandatory = isMandatory;
                        rrc.DataType.value = dataType;
                        //20090107 PL
                        rrc.LabelWidth = "10%";
                        rrc.InputWidth = "90%";
                        rrc.Length = 64;
                        //20120220 PL Align (pour des raisons d'esthetique)
                        rrc.AlignSpecified = true;
                        //rrc.Align = (TypeData.IsTypeBool(dataType) ? "right" : "left");
                        rrc.Align = "left";
                        rrc.Scale = (TypeData.IsTypeDec(dataType)) ? 10 : 0;
                        rrc.Default = new ReferentialsReferentialColumnDefault[1];
                        rrc.Default[0] = new ReferentialsReferentialColumnDefault
                        {
                            Value = defaultValue
                        };
                        rrc.IsHideSpecified = true;
                        rrc.IsHide = false;
                        rrc.IsHideInDataGridSpecified = true;
                        rrc.IsHideInDataGrid = isHideInDatagrid;
                        rrc.IsKeyFieldSpecified = true;
                        rrc.IsKeyField = false;
                        rrc.IsDataKeyFieldSpecified = true;
                        rrc.IsDataKeyField = false;
                        rrc.IsIdentitySpecified = true;
                        rrc.IsIdentity.Value = false;

                        rrc.IsUpdatable = new ReferentialsReferentialColumnIsUpdatable();
                        rrc.IsUpdatableSpecified = true;
                        rrc.IsUpdatable.Value = true;

                        rrc.Relation = new ReferentialsReferentialColumnRelation[1];
                        rrc.Relation[0] = new ReferentialsReferentialColumnRelation();

                        DispatchListRetrieval(ref rrc, listRetrieval);

                        //PL 20120618 New features
                        #region rrc.Information
                        if (StrFunc.IsFilled(helpMessage))
                        {
                            rrc.InformationSpecified |= GetInformationCustomization(helpType, helpDisplay, helpMessage, out rrc.Information);
                            //PL 20121009 Newness
                            if (!String.IsNullOrEmpty(listRetrieval))
                            {
                                rrc.InputWidth = string.Empty; // Suppression du "90%" afin de cadrer, à gauche de la DDL, l'image de l'aide en ligne
                            }
                        }
                        #endregion

                        aObjects.Add(rrc);
                        #endregion While
                    }

                }
            }
            #endregion

            #region Add Additionnal Column from ROLE
            if (opReferentialsReferential.RoleTableNameSpecified)
            {
                EFS.Common.Web.Menu.Menu menuRole = SessionTools.Menus.SelectByURL("=" + opReferentialsReferential.RoleTableName.Value + "&");
                bool isUserRightEnabledOnMenu = ((menuRole != null) && (menuRole.IsEnabled));
                //Le menu d'accès direct aux items est accessible par l'utilisateur connecté --> Affichage des items pour consultation/modification directement depuis le référentiel en cours. 
                if (isUserRightEnabledOnMenu)
                {
                    //Modification autorisée uniquement lorsque toutes les permissions sur le menu correspondant aux rôles sont autorisées (Create, Modify et Remove)
                    bool isUpdatable = ReferentialTools.IsAllPermissionEnabled(SessionTools.CS, menuRole.IdMenu, SessionTools.User);

                    if (aObjects.Count == 0)
                    {
                        for (int index = 0; index < opReferentialsReferential.Column.Length; index++)
                        {
                            aObjects.Add(((System.Array)opReferentialsReferential.Column).GetValue(index));
                        }
                    }

                    string tbl = opReferentialsReferential.RoleTableName.Value;       //ex.:  "ACTORROLE"
                    string tblRole = "ROLE" + tbl.Substring(0, tbl.Length - 4); //ex.:  "ROLEACTOR"
                    string idcol = "ID" + tblRole;                              //ex.:  "IDROLEACTOR"
                    string concat = DataHelper.GetConcatOperator(SessionTools.CS);

                    #region SQLSelect - Select des ROLES triés par ROLETYPE
                    SQLSelect = SQLCst.SELECT + idcol + ",";
                    SQLSelect += idcol + concat + " case when (" + idcol + "!=DESCRIPTION) then "
                        + DataHelper.SQLString(" - ") + concat + " DESCRIPTION else ' ' end as NAME,";
                    SQLSelect += "ROLETYPE" + Cst.CrLf;
                    SQLSelect += SQLCst.FROM_DBO + tblRole + Cst.CrLf;

                    //CC/PL 20170922 [23429] Tips ACTOR/ALGO restrict
                    if (opReferentialsReferential.TableName == Cst.OTCml_TBL.ACTOR.ToString())
                    {
                        if (opReferentialsReferential.Ressource == "ALGORITHM")
                        {
                            SQLSelect += SQLCst.WHERE + "ROLETYPE='ALGORITHM'" + Cst.CrLf;
                        }
                        else
                        {
                            SQLSelect += SQLCst.WHERE + "ROLETYPE!='ALGORITHM'" + Cst.CrLf;
                        }
                    }


                    SQLSelect += SQLCst.ORDERBY + "ROLETYPE," + idcol;
                    #endregion

                    int iStart = i;
                    string lastRoleType = string.Empty;
                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, SQLSelect))
                    {
                        while (dr.Read())
                        {
                            i++;
                            #region Column IDROLExxx
                            ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn();
                            string identifier = dr[idcol].ToString().Trim();
                            string displayName = dr["NAME"].ToString().Trim();
                            string roleType = dr["ROLETYPE"].ToString().Trim();
                            string dataType = TypeData.TypeDataEnum.@bool.ToString();
                            string defaultValue = string.Empty;
                            //
                            if (i == iStart + 1)
                            {
                                rrc.html_BLOCK = new ReferentialsReferentialColumnhtml_BLOCK[1];
                                rrc.html_BLOCK[0] = new ReferentialsReferentialColumnhtml_BLOCK
                                {
                                    columnbyrowSpecified = true,
                                    startdisplaySpecified = true,
                                    startdisplay = "collapse"
                                };
                                if (opReferentialsReferential.RoleTableName.columnbyrowSpecified)
                                    rrc.html_BLOCK[0].columnbyrow = opReferentialsReferential.RoleTableName.columnbyrow;
                                else
                                    rrc.html_BLOCK[0].columnbyrow = 2;
                                rrc.html_BLOCK[0].title = Ressource.GetString("Role", true);
                                rrc.html_BLOCK[0].InformationSpecified = true;
                                rrc.html_BLOCK[0].Information = new ReferentialInformation
                                {
                                    TypeSpecified = true,
                                    Type = Cst.TypeInformationMessage.Information,
                                    LabelMessageSpecified = true,
                                    LabelMessage = "Msg_ROLExForxROLE"
                                };
                            }
                            if (lastRoleType != roleType)
                            {
                                lastRoleType = roleType;
                                //
                                rrc.html_HR = new ReferentialsReferentialColumnhtml_HR[1];
                                rrc.html_HR[0] = new ReferentialsReferentialColumnhtml_HR
                                {
                                    columnbyrowSpecified = false,
                                    size = "1",
                                    title = Ressource.GetString(roleType, true)
                                };
                            }
                            rrc.IsRole = true;
                            rrc.ExternalFieldID = i;
                            rrc.ExternalTableName = tbl;
                            rrc.ExternalIdentifier = identifier;
                            rrc.ColumnName = idcol;
                            rrc.AliasTableNameSpecified = true;
                            rrc.AliasTableName = "r" + tbl.Substring(0, 1).ToLower() + identifier;
                            rrc.DataField = rrc.ColumnName;
                            rrc.Ressource = displayName;
                            rrc.RessourceSpecified = true;
                            rrc.IsMandatorySpecified = true;
                            rrc.IsMandatory = true;
                            rrc.DataType.value = dataType;
                            rrc.ColspanSpecified = true;
                            rrc.Colspan = 1;
                            //rrc.LabelWidth = "10%";
                            //rrc.InputWidth = "100px";
                            rrc.Length = 1;
                            rrc.Scale = 0;
                            rrc.Default = new ReferentialsReferentialColumnDefault[1];
                            rrc.Default[0] = new ReferentialsReferentialColumnDefault
                            {
                                Value = defaultValue
                            };
                            rrc.IsHideSpecified = true;
                            rrc.IsHide = false;
                            rrc.IsHideInDataGridSpecified = true;
                            rrc.IsHideInDataGrid = true;
                            rrc.IsKeyFieldSpecified = true;
                            rrc.IsKeyField = false;
                            rrc.IsDataKeyFieldSpecified = true;
                            rrc.IsDataKeyField = false;
                            rrc.IsIdentitySpecified = true;
                            rrc.IsIdentity.Value = false;
                            //
                            rrc.IsUpdatable = new ReferentialsReferentialColumnIsUpdatable();
                            rrc.IsUpdatableSpecified = true;
                            rrc.IsUpdatable.Value = isUpdatable;
                            //PL 20100622 Add test on SYSADMIN and SYSOPER
                            if (tblRole == Cst.OTCml_TBL.ROLEACTOR.ToString())
                            {
                                if (
                                    ((identifier == RoleActor.SYSADMIN.ToString()) && !SessionTools.IsSessionSysAdmin)
                                    ||
                                    ((identifier == RoleActor.SYSOPER.ToString()) && !SessionTools.IsSessionSysAdmin && !SessionTools.IsSessionSysOper)
                                )
                                {
                                    rrc.IsUpdatable.Value = false;
                                    rrc.IsUpdatable.isupdatableincreation = false;
                                    rrc.IsUpdatable.isupdatableincreationSpecified = true;
                                    //NB: En création il faudrait initialiser à "false"... (à finaliser)
                                    //rrc.Default[0].Value = false.ToString();
                                }
                            }
                            //
                            rrc.Relation = new ReferentialsReferentialColumnRelation[1];
                            rrc.Relation[0] = new ReferentialsReferentialColumnRelation();
                            #endregion Column IDROLExxx
                            //
                            #region Column IDAxxx
                            bool isGinstrRole = (opReferentialsReferential.RoleTableName.Value == "GINSTRROLE");
                            bool isActorRole = (opReferentialsReferential.RoleTableName.Value == "ACTORROLE");
                            // EG 20141021 [20442]
                            bool isGContractRole = (opReferentialsReferential.RoleTableName.Value == "GCONTRACTROLE");
                            rrc.IsAutoPostBackSpecified = true;
                            rrc.IsAutoPostBack = isActorRole;
                            aObjects.Add(rrc);
                            // EG 20141021 [20442]
                            if (isGinstrRole || isActorRole || isGContractRole)
                            {
                                rrc = new ReferentialsReferentialColumn();
                                //identifier = "IDA";
                                displayName = "Actor";
                                dataType = TypeData.TypeDataEnum.integer.ToString();
                                defaultValue = string.Empty;
                                //
                                rrc.IsRole = true;
                                rrc.ExternalFieldID = i;
                                rrc.ExternalTableName = tbl;
                                rrc.ExternalIdentifier = "IDA" + "_" + identifier;//identifier;
                                if (isActorRole)
                                    rrc.ColumnName = "IDA_ACTOR";
                                else
                                    rrc.ColumnName = "IDA";
                                rrc.AliasTableNameSpecified = true;
                                rrc.AliasTableName = "r" + tbl.Substring(0, 1).ToLower() + identifier;
                                rrc.DataField = rrc.ColumnName;
                                rrc.Ressource = displayName;
                                rrc.RessourceSpecified = true;
                                rrc.IsMandatorySpecified = true;
                                rrc.IsMandatory = false;//Glop
                                rrc.DataType.value = dataType;
                                rrc.ColspanSpecified = true;
                                rrc.Colspan = 0;
                                rrc.LabelNoWrap = true;
                                //rrc.LabelWidth = "50px";
                                //rrc.InputWidth = "100px";
                                rrc.Length = 10;
                                rrc.Scale = 0;
                                rrc.Default = new ReferentialsReferentialColumnDefault[1];
                                rrc.Default[0] = new ReferentialsReferentialColumnDefault
                                {
                                    Value = defaultValue
                                };
                                rrc.IsHideSpecified = true;
                                rrc.IsHide = false;
                                rrc.IsHideInDataGridSpecified = true;
                                rrc.IsHideInDataGrid = true;
                                rrc.IsKeyFieldSpecified = true;
                                rrc.IsKeyField = false;
                                rrc.IsDataKeyFieldSpecified = true;
                                rrc.IsDataKeyField = false;
                                rrc.IsIdentitySpecified = true;
                                rrc.IsIdentity.Value = false;
                                //
                                rrc.IsUpdatableSpecified = true;
                                rrc.IsUpdatable = new ReferentialsReferentialColumnIsUpdatable
                                {
                                    // EG 20141021 [20442]
                                    Value = (isUpdatable && ((!isGinstrRole && !isGContractRole) || (identifier == "INVOICING"))),
                                    isupdatableincreationSpecified = true
                                };
                                rrc.IsUpdatable.isupdatableincreation = rrc.IsUpdatable.Value;
                                //
                                rrc.Relation = new ReferentialsReferentialColumnRelation[1];
                                rrc.Relation[0] = new ReferentialsReferentialColumnRelation
                                {
                                    //rrc.Relation[0].DDLType = new ReferentialsReferentialColumnRelationDDLType();
                                    //rrc.Relation[0].DDLType.Value = (rrc.IsUpdatable.Value ? "actor" : "empty");
                                    TableName = Cst.OTCml_TBL.ACTOR.ToString(),
                                    ColumnRelation = new ReferentialsReferentialColumnRelationColumnRelation[1]
                                };
                                rrc.Relation[0].ColumnRelation[0] = new ReferentialsReferentialColumnRelationColumnRelation
                                {
                                    ColumnName = "IDA",
                                    DataType = TypeData.TypeDataEnum.integer.ToString()
                                };
                                rrc.Relation[0].ColumnSelect = new ReferentialsReferentialColumnRelationColumnSelect[1];
                                rrc.Relation[0].ColumnSelect[0] = new ReferentialsReferentialColumnRelationColumnSelect
                                {
                                    ColumnName = "IDENTIFIER",
                                    DataType = TypeData.TypeDataEnum.@string.ToString(),
                                    Ressource = "ForActor"
                                };
                                rrc.Relation[0].ColumnLabel = new ReferentialsReferentialColumnRelationColumnLabel[1];
                                rrc.Relation[0].ColumnLabel[0] = new ReferentialsReferentialColumnRelationColumnLabel
                                {
                                    ColumnName = "DISPLAYNAME"
                                };
                                rrc.Relation[0].ColumnLabel[0].ColumnName = "DISPLAYNAME";
                                rrc.Relation[0].ColumnLabel[0].DataType = TypeData.TypeDataEnum.@string.ToString();
                                rrc.Relation[0].ColumnLabel[0].Ressource = "DISPLAYNAME";
                                rrc.Relation[0].Condition = new ReferentialsReferentialColumnRelationCondition[1];
                                rrc.Relation[0].Condition[0] = new ReferentialsReferentialColumnRelationCondition
                                {
                                    SQLWhereSpecified = true
                                };
                                if (isActorRole)
                                {
                                    //PL 20090305 
                                    //rrc.Relation[0].Condition[0].SQLWhere = "(1 = %%COLUMN_VALUE%%" + rrc.AliasTableName + "." + idcol + "%%)";
                                    //PL 20111227 Refactoring (use Cst.COLUMN_VALUE)
                                    rrc.Relation[0].Condition[0].SQLWhere = "(1 = " + Cst.COLUMN_VALUE + rrc.AliasTableName + "." + idcol + "%%)";
                                }
                                else
                                {
                                    rrc.Relation[0].Condition[0].SQLWhere = ((identifier == "INVOICING") ? "1=1" : "1=2");
                                }
                                rrc.Relation[0].ListTypeSpecified = true;
                                rrc.Relation[0].ListType = Cst.ListType.Repository.ToString();
                                //
                                //if (i == iStart + 1)
                                //{
                                //    rrc.InformationSpecified = true;
                                //    rrc.Information = new ReferentialInformation();
                                //    rrc.Information.TypeSpecified = true;
                                //    rrc.Information.Type = Cst.TypeInformationMessage.Information;
                                //    rrc.Information.LabelMessageSpecified = true;
                                //    rrc.Information.LabelMessage = "Msg_GInstrRole";
                                //    rrc.Information.PopupMessageSpecified = true;
                                //    rrc.Information.PopupMessage = "Msg_GInstrRole";
                                //}
                                //

                                if (isActorRole)
                                {
                                    rrc.Relation[0].AutoComplete = new ReferentialAutoComplete();
                                    
                                }
                                aObjects.Add(rrc);
                            }
                            #endregion Column IDAxxx
                        }
                    }
                }
            }
            #endregion

            #region Add Additionnal Column from ITEM
            if (opReferentialsReferential.ItemsSpecified)
            {
                EFS.Common.Web.Menu.Menu menuItem = SessionTools.Menus.SelectByURL("=" + opReferentialsReferential.Items.tablename + "&");//rctablename="GINSTR" tablename="INSTRG" 

                bool isUserRightEnabledOnMenu = ((menuItem != null) && (menuItem.IsEnabled));
                //Menu d'accès direct aux items, autorisé pour l'utilisateur connecté --> Affichage des items pour consultation/modification directement depuis le référentiel en cours. 
                if (isUserRightEnabledOnMenu)
                {
                    //Modification autorisée uniquement 
                    //- lorsqu'auaucun mode n'est précisé (tous les modes sont des modes Read Only)
                    //- lorsque toutes les permissions sont autorisées sur le menu correspondant aux items (Create, Modify et Remove)
                    bool isUpdatable = (opReferentialsReferential.Items.mode == "RW") && ReferentialTools.IsAllPermissionEnabled(SessionTools.CS, menuItem.IdMenu, SessionTools.User);

                    //Alimentation d'un ArrayList avec toutes les colonnes, si cela n'a pas déjà été opérée dans une étape précédente.
                    if (aObjects.Count == 0)
                    {
                        for (int index = 0; index < opReferentialsReferential.Column.Length; index++)
                        {
                            aObjects.Add(((System.Array)opReferentialsReferential.Column).GetValue(index));
                        }
                    }

                    #region SQLSelect - Select des ITEMS triés (spécifiquement pour certains référentiels)
                    SQLSelect = SQLCst.SELECT + "tblmain." + opReferentialsReferential.Items.columnname + " as ID,"; //ex.: "IDI" pour le référentiel "GINSTR" 
                    SQLSelect += "tblmain.IDENTIFIER || case when (tblmain.IDENTIFIER!=tblmain.DISPLAYNAME) then ' - ' || tblmain.DISPLAYNAME else ' ' end as NAME,";
                    switch (opReferentialsReferential.Items.srctablename)//ex.: "INSTRUMENT" pour le référentiel "GINSTR"
                    {
                        case "INSTRUMENT":
                            #region
                            SQLSelect += "p.GPRODUCT || '|' || p.FAMILY || '|' || case when p.CLASS='STRATEGY' and p.CLASS!=p.FAMILY then p.CLASS else ' ' end as Title" + Cst.CrLf;
                            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " tblmain" + Cst.CrLf;
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.PRODUCT,
                                SQLJoinTypeEnum.Inner, "tblmain.IDP", "p", DataEnum.All);
                            SQLSelect += SQLCst.ORDERBY + "p.GPRODUCT,p.FAMILY,p.CLASS,tblmain.IDENTIFIER";
                            break;
                        #endregion
                        case "MENU":
                            #region
                            SQLSelect = SQLCst.SELECT + "tblmain." + opReferentialsReferential.Items.columnname + " as ID,";
                            SQLSelect += "tblmain.IDMENU as NAME,tblmain.IDMENU as Title" + Cst.CrLf;
                            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MENU.ToString() + " tblmain" + Cst.CrLf;
                            //PL 20111223 Add TBD... (See also: ReferentialsReferential.Initialize())
                            if (DataHelper.IsDbOracle(pCS))
                                SQLSelect += SQLCst.WHERE + DataHelper.SQLLength(pCS, "tblmain.IDMENU") + "<28" + Cst.CrLf;
                            SQLSelect += SQLCst.ORDERBY + "tblmain.IDMENU";
                            break;
                        #endregion
                        case "MARKET":
                            #region
                            SQLSelect += DataHelper.SQLIsNull(pCS, "c.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "tblmain.IDCOUNTRY", " "))
                                + " || '|' || " + DataHelper.SQLIsNull(pCS, "bc.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "bc.DISPLAYNAME", " ")) + " as Title" + Cst.CrLf;
                            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MARKET.ToString() + " tblmain" + Cst.CrLf;
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.BUSINESSCENTER, SQLJoinTypeEnum.Left, "tblmain.IDBC", "bc", DataEnum.All);
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.COUNTRY, SQLJoinTypeEnum.Left, "tblmain.IDCOUNTRY", "c", DataEnum.All);
                            SQLSelect += SQLCst.ORDERBY + "tblmain.IDCOUNTRY,bc.DESCRIPTION,tblmain.IDBC,tblmain.IDENTIFIER";
                            break;
                        #endregion
                        case "DERIVATIVECONTRACT":
                            #region
                            SQLSelect += DataHelper.SQLIsNull(pCS, "c.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "m.IDCOUNTRY", " "))
                                + " || '|' || "
                                + DataHelper.SQLIsNull(pCS, "bc.DESCRIPTION", DataHelper.SQLIsNullChar(pCS, "bc.DISPLAYNAME", " "))
                                + " || '|' || " + DataHelper.SQLIsNullChar(pCS, "m.DISPLAYNAME", " ") + " as Title" + Cst.CrLf;
                            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString() + " tblmain" + Cst.CrLf;
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.MARKET, SQLJoinTypeEnum.Left, "tblmain.IDM", "m", DataEnum.All);
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.BUSINESSCENTER, SQLJoinTypeEnum.Left, "m.IDBC", "bc", DataEnum.All);
                            SQLSelect += OTCmlHelper.GetSQLJoin(pCS, Cst.OTCml_TBL.COUNTRY, SQLJoinTypeEnum.Left, "m.IDCOUNTRY", "c", DataEnum.All);
                            SQLSelect += SQLCst.ORDERBY + "m.IDCOUNTRY,bc.DESCRIPTION,m.IDBC,m.IDENTIFIER,tblmain.IDENTIFIER";
                            break;
                        #endregion
                        default:
                            #region
                            SQLSelect += "null as Title" + Cst.CrLf;
                            SQLSelect += SQLCst.FROM_DBO + opReferentialsReferential.Items.srctablename + " tblmain" + Cst.CrLf;
                            SQLSelect += SQLCst.ORDERBY + "IDENTIFIER";
                            break;
                            #endregion
                    }
                    #endregion

                    int iStart = i;
                    string lastTitle = null;

                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, SQLSelect))
                    {
                        // Création d'une colonne (à l'image des colonnes ExtlId) pour chaque valeur disponible (ex. pour chaque "INSTRUMENT" pour le référentiel "GINSTR").
                        while (dr.Read())
                        {
                            i++;
                            #region Column IDxxx
                            ReferentialsReferentialColumn rrc = new ReferentialsReferentialColumn();
                            string identifier = dr["ID"].ToString().Trim();
                            string displayName = dr["NAME"].ToString().Trim();
                            string title = dr["Title"].ToString().Trim();
                            string dataType = TypeData.TypeDataEnum.@bool.ToString();

                            if (i == iStart + 1)
                            {
                                #region html_BLOCK - Création d'un nouveau BLOC sur la première colonne
                                rrc.html_BLOCK = new ReferentialsReferentialColumnhtml_BLOCK[1];
                                if (opReferentialsReferential.Items.html_BLOCKSpecified)
                                {
                                    rrc.html_BLOCK[0] = opReferentialsReferential.Items.html_BLOCK;
                                    if (String.IsNullOrEmpty(rrc.html_BLOCK[0].title))
                                    {
                                        //Ticket:16978 On prend la ressource du sous-menu
                                        rrc.html_BLOCK[0].title = Ressource.GetString(menuItem.IdMenu, true);
                                    }
                                    else
                                    {
                                        rrc.html_BLOCK[0].title = Ressource.GetString(rrc.html_BLOCK[0].title, true);
                                    }
                                    // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du startdisplay
                                    rrc.html_BLOCK[0].startdisplaySpecified = opReferentialsReferential.Items.html_BLOCK.startdisplaySpecified;
                                    rrc.html_BLOCK[0].startdisplay = opReferentialsReferential.Items.html_BLOCK.startdisplay;
                                }
                                else
                                {
                                    rrc.html_BLOCK[0] = new ReferentialsReferentialColumnhtml_BLOCK
                                    {
                                        columnbyrowSpecified = false,
                                        // EG 20100504 Ticket:16978 On prend la ressource du sous-menu
                                        title = Ressource.GetString(menuItem.IdMenu, true),
                                        InformationSpecified = true,
                                        Information = new ReferentialInformation()
                                    };
                                    rrc.html_BLOCK[0].Information.TypeSpecified = true;
                                    rrc.html_BLOCK[0].Information.Type = Cst.TypeInformationMessage.Information;
                                    rrc.html_BLOCK[0].Information.LabelMessageSpecified = true;
                                    rrc.html_BLOCK[0].Information.LabelMessage = "Msg_ITEMxForxITEM";
                                    // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du startdisplay
                                    rrc.html_BLOCK[0].startdisplaySpecified = true;
                                    rrc.html_BLOCK[0].startdisplay = "collapse";
                                }
                                // EG 20210304 [XXXXX] Relooking Referentiels - Gestion du containerOverflow
                                if (opReferentialsReferential.Items.ContainerOverflowSpecified)
                                {
                                    rrc.ContainerOverflow = new ReferentialsReferentialContainerOverflow() { 
                                        startheightSpecified = opReferentialsReferential.Items.ContainerOverflow.startheightSpecified,
                                        startheight = opReferentialsReferential.Items.ContainerOverflow.startheight,
                                        Value = opReferentialsReferential.Items.ContainerOverflow.Value
                                    };
                                }
                                #endregion
                            }

                            if (lastTitle != title)
                            {
                                #region html_HR - Création éventuelle d'un nouveau HR sur la base d'une rupure sur "Title"
                                //PL 2013 Mettre un exemple afin de faciliter la ocmpréhension
                                if (lastTitle == null)
                                {
                                    lastTitle = @"#" + title.Replace(@"|", @"|#"); //Tip
                                }
                                string[] aLastTitle = lastTitle.Split(new char[] { '|' });
                                string[] aTitle = title.Split(new char[] { '|' });
                                rrc.html_HR = new ReferentialsReferentialColumnhtml_HR[aTitle.Length];
                                string start = string.Empty;
                                for (int nbHR = 0; nbHR < aTitle.Length; nbHR++)
                                {
                                    if (aTitle[nbHR] != aLastTitle[nbHR])
                                    {
                                        if ((nbHR == 0) || (StrFunc.IsFilled(aTitle[nbHR])))
                                        {
                                            rrc.html_HR[nbHR] = new ReferentialsReferentialColumnhtml_HR
                                            {
                                                columnbyrowSpecified = false,
                                                size = (aTitle.Length - nbHR).ToString(),
                                                title = start + aTitle[nbHR]
                                            };
                                            rrc.html_HR[nbHR].title = @"{NoTranslate}" + rrc.html_HR[nbHR].title.Replace(@",", @"," + Cst.HTMLBreakLine + Cst.HTMLSpace2);
                                        }
                                    }
                                    start += Cst.HTMLSpace2 + Cst.HTMLSpace2;
                                }

                                lastTitle = title;
                                #endregion
                            }
                            rrc.IsItem = true;
                            rrc.ExternalFieldID = i;
                            rrc.ExternalTableName = opReferentialsReferential.Items.srctablename;
                            rrc.ExternalIdentifier = identifier;
                            rrc.ColumnName = opReferentialsReferential.Items.columnname;
                            rrc.AliasTableNameSpecified = true;
                            //Important *********************************************************************************************************************
                            //rrc.AliasTableName: Constitué de 3 données (see also GetSQL())
                            //ex.: ii99 avec "i" en dur pour "Item", premier caractère lower() de la table (ex. "i" pour INSTRUMENT), valeur de la donnée (ex. 99 pour un IDI)
                            rrc.AliasTableName = "i" + opReferentialsReferential.Items.srctablename.Substring(0, 1).ToLower() + identifier.Replace("-", "_");
                            //*******************************************************************************************************************************
                            rrc.DataField = rrc.ColumnName;
                            rrc.Ressource = displayName;
                            rrc.RessourceSpecified = true;
                            rrc.ColspanSpecified = false;
                            rrc.Colspan = 1;//NB: Bien que ColspanSpecified soit false, il est nécessaire de valoriser Colspan
                            rrc.IsMandatorySpecified = true;
                            rrc.IsMandatory = true;
                            rrc.DataType.value = dataType;
                            rrc.Length = 1;
                            rrc.IsHideInDataGridSpecified = true;
                            rrc.IsHideInDataGrid = true;

                            rrc.IsUpdatable = new ReferentialsReferentialColumnIsUpdatable();
                            rrc.IsUpdatableSpecified = true;
                            rrc.IsUpdatable.Value = isUpdatable;

                            rrc.Relation = new ReferentialsReferentialColumnRelation[1];
                            rrc.Relation[0] = new ReferentialsReferentialColumnRelation();

                            //Ajout de la colonne dans l'ArrayList.
                            aObjects.Add(rrc);
                            #endregion Column IDxxx
                        }
                    }
                }
            }
            #endregion

            //Recréation des colonnes à partir de l'ArrayList créé et enrichi précédemment.
            if (aObjects.Count > 0)
            {
                opReferentialsReferential.HasMultiTable = true;
                System.Type type = ((System.Array)opReferentialsReferential.Column).GetType().GetElementType();
                opReferentialsReferential.Column = (ReferentialsReferentialColumn[])aObjects.ToArray(type);
            }
            #endregion Add Additionnal Column (EXTLID, ROLE)
        }
        #endregion

        #region public DeserializeXML_ForModeRO
        /// <summary> 
        /// Renseigne une classe referential à partir d'un fichier XML
        /// </summary>
        public static void DeserializeXML_ForModeRO(string pFileName, out ReferentialsReferential opReferentialsReferential)
        {
            // FI 20201215 [XXXXX] Alimentation du paramètre pValueFK Avec null
            DeserializeXML_ForModeRO(pFileName, null, null, null, null, out opReferentialsReferential);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileName"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArgs"></param>
        /// <param name="pValueFK">value ForeignKeyField</param>
        /// <param name="opReferentialsReferential"></param>
        /// FI 20141211 [20563] Modification de signature add pDynamicArgs
        /// FI 20201215 [XXXXX] Add pValueFK
        public static void DeserializeXML_ForModeRO(string pFileName, string pCondApp, string[] pParam, Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicArgs, string pValueFK,
            out ReferentialsReferential opReferentialsReferential)
        {
            StreamReader streamReader = null;
            try
            {
                string XML_File = pFileName;
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Referentials));

                streamReader = new StreamReader(XML_File);
                Referentials referentials = (Referentials)xmlSerializer.Deserialize(streamReader);
                // FI 20201215 [XXXXX] Alimentation du paramètre pValueFK
                referentials.Items[0].Initialize(false, pCondApp, pParam, pDynamicArgs, pValueFK);
                opReferentialsReferential = referentials.Items[0];
            }
            finally
            {
                if (streamReader != null)
                    streamReader.Close();
            }
        }
        #endregion

        /// <summary>
        /// Initialise la classe ReferentialsReferential pour un usage de type Grid
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential</param>
        /// FI 20200220 [XXXXX] pReferential n'est plus de type ref
        public static void InitializeReferentialForGrid(ReferentialsReferential pReferential)
        {
            // FI 20201214 [XXXXX] Pas d'alimentation de valueForeignKeyField ici
            //pReferential.valueForeignKeyField = pValueForeignKeyField;
            pReferential.IsGrid = true;
            pReferential.IsForm = false;

            GetExtlLinkCustomization(pReferential);
        }

        /// <summary>
        /// Ecriture des nouveaux fichiers de Syndication du site EFS
        /// </summary>
        /// <param name="pReferential"></param>
        /// EG 20210630 [25500] Customer Portal / EFS WebSite : Refactoring de la gestion de la mise à jour des flux RSS (Syndication)
        public static void UpdateSyndicationFeed()
        {
            string culture;
            for (int passCulture = 1; passCulture <= 3; passCulture++)
            {
                culture = (passCulture == 1 ? "en" : (passCulture == 2 ? "fr" : "it"));
                SyndicationTools.SaveSyndicationFeedFile(EFSSyndicationFeed.GetSyndicationFeed(SessionTools.CS, EFSSyndicationFeed.SyndicationFeedTypeEnum.BUSINESSNEWS, culture), SyndicationTools.FeedFormatEnum.ALL, culture);
            }

        }

        /// <summary>
        /// Initialise la classe ReferentialsReferential pour un usage de type FORM 
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <param name="pIsNewRecord">nouvel enregistrement</param>
        /// <param name="pConsultationMode">mode selection</param>
        /// <param name="pDataKeyField">nom de colonne du dataKeyField</param>
        /// <param name="pValueDataKeyField">valeur du dataKeyField</param>
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// FI 20151215 [XXXXX] Suppression du paramètre  pValueForeignKeyField
        public static void InitializeReferentialForForm_2(PageBaseReferentialv2 pPage,
                ReferentialsReferential pReferential,
                bool pIsNewRecord, bool pIsDuplicateRecord, Cst.ConsultationMode pConsultationMode,
                string pDataKeyField, string pValueDataKeyField)
        {
            pReferential.IsGrid = false;
            pReferential.IsForm = true;
            // 
            pReferential.isNewRecord = pIsNewRecord && !pIsDuplicateRecord;
            pReferential.consultationMode = pConsultationMode;
            //
            // EG 20121029 PartialReadOnlyMode test
            pReferential.isLookReadOnly = ((pConsultationMode != Cst.ConsultationMode.Normal) && (pConsultationMode != Cst.ConsultationMode.PartialReadOnly))
                || (!pReferential.Create & pIsNewRecord) || (!pReferential.Modify & !pIsNewRecord);
            // FI 20201215 [XXXXX] pReferential.valueForeignKeyField est déjà alimenté
            //pReferential.valueForeignKeyField = pValueForeignKeyField;
            //
            if (!pPage.IsPostBack)
            {
                DataSet ds = GetDsDataForm(pPage, pReferential, pIsNewRecord, pConsultationMode, pDataKeyField, pValueDataKeyField);
                pPage.DataReferentiel = ds;
            }
            //Entre 2 post de page le ds est sauvegardé dans une variable session
            pReferential.dataSet = pPage.DataReferentiel;

            bool isOk = true;
            DataRow rowSource = null;
            if (!pIsNewRecord || pIsDuplicateRecord)
            {
                //Obtention du DataRow correspondand depuis le DataSet
                rowSource = pReferential.GetRow(0, null, pValueDataKeyField);

                #region DataRow non trouvé
                if (rowSource == null)
                {
                    isOk = false;

                    //La page "PageBaseReferentialv2" nécessite l'existence d'un "pReferential.dataRow"
                    pReferential.dataRow = pReferential.dataSet.Tables[0].NewRow();

                    //Message utilisateur de type SelfClose, afin de fermer le formulaire.
                    JavaScript.DialogStartUpImmediate(pPage, Ressource.GetString("Msg_DataRowNotFound"), true, ProcessStateTools.StatusWarningEnum);
                }
                #endregion
            }

            if (isOk)
            {
                if (pIsNewRecord || pIsDuplicateRecord)
                {
                    #region
                    //New ou Duplicate Record: Création d'un nouveau DataRow puis initialisation de celui-ci.
                    pReferential.dataRow = pReferential.dataSet.Tables[0].NewRow();

                    if (pReferential.HasMultiTable)
                    {
                        pReferential.drExternal = new DataRow[pReferential.dataSet.Tables.Count - 1];
                        pReferential.isNewDrExternal = new bool[pReferential.dataSet.Tables.Count - 1];
                        for (int i = 1; i < pReferential.dataSet.Tables.Count; i++)
                        {
                            pReferential.drExternal[i - 1] = pReferential.dataSet.Tables[i].NewRow();
                            //PL 20130125 En mode "pIsDuplicateRecord, initialisation de "isNewDrExternal" plus bas après CreateControls(), afin de ne pas considérer 
                            //            ces lignes comme des lignes nouvelles et ainsi permettre la duplication de leur données respectives.
                            if (!pIsDuplicateRecord)
                            {
                                pReferential.isNewDrExternal[i - 1] = true;
                            }
                        }
                    }

                    if (pIsDuplicateRecord)
                    {
                        //PL 20130422 New feature
                        string actorIdentifier = (pPage.Request.QueryString["NAV"]);//NAV:New Actor Value
                        pReferential.DuplicateRow(ref pReferential.dataRow, rowSource, actorIdentifier);

                        //PL 20130125 New features: Duplication des données externes...
                        for (int i = 1; i < pReferential.dataSet.Tables.Count; i++)
                        {
                            DataRow extlRowSource = pReferential.GetRow(i, "ID", pValueDataKeyField);//La colonne "clé" est ici en toujours "ID"
                            if (extlRowSource != null)
                            {
                                //Une ligne existe, uniquement lorsque la donnée est "cochée".
                                for (int index = 0; index < extlRowSource.ItemArray.Length; index++)
                                {
                                    try
                                    {
                                        if (extlRowSource.Table.Columns[index].ColumnName == "ID")
                                        {
                                            //Colonne clé, on réinitialise sa valeur afin de pas impacter le référentiel servant de base à la duplication
                                            pReferential.drExternal[i - 1][index] = Convert.DBNull;
                                        }
                                        else
                                        {
                                            pReferential.drExternal[i - 1][index] = extlRowSource[index];
                                        }
                                    }
                                    catch
                                    {
                                        pReferential.drExternal[i - 1][index] = Convert.DBNull;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        pReferential.InitializeNewRow(ref pReferential.dataRow);
                    }
                    #endregion
                }
                else
                {
                    #region
                    pReferential.dataRow = rowSource;
                    // EG 20121029 Brachement temporaire pour alimenter des colonnes par des valeurs par défaut en MODIFICATION
                    // ACTUELLEMENT: Tracker exclusivement.
                    if (pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly)
                    {
                        pReferential.InitializeUpdateRow(pReferential.dataRow);
                    }
                    try
                    {
                        if (pReferential.ExistsColumnIDENTITY && (!pReferential.Column[pReferential.IndexIDENTITY].IsIdentity.sourceSpecified))
                        {
                            pPage.AddInfoLeft("ID System (" + pReferential.Column[pReferential.IndexIDENTITY].ColumnName + "): " + pReferential.dataRow[pReferential.IndexColSQL_IDENTITY]);
                        }

                        if (pReferential.ExistsColumnROWATTRIBUT && (pReferential.dataRow[pReferential.IndexColSQL_ROWATTRIBUT].ToString() == Cst.RowAttribut_Deleted))
                        {
                            pPage.AddFooterLeft(Ressource.GetString("Msg_ErasedItem"));
                        }

                        //20071102 PL Add ExistsColumnsDateValidity et test sur DTENABLED
                        if (pReferential.ExistsColumnsDateValidity)
                        {
                            if ((Convert.ToDateTime(pReferential.dataRow["DTENABLED"]).CompareTo(DateTime.Today) > 0)
                                ||
                                (!(pReferential.dataRow["DTDISABLED"] is DBNull) && (Convert.ToDateTime(pReferential.dataRow["DTDISABLED"]).CompareTo(DateTime.Today) <= 0)))
                            {
                                pPage.AddFooterLeft(Ressource.GetString("Msg_DisabledItem"));
                            }
                        }

                        string infoRight = GetInfoRight(pReferential);
                        if (StrFunc.IsFilled(infoRight))
                            pPage.AddInfoRight(infoRight);
                    }
                    catch (Exception)
                    {
                        //FI 20120119 Ajout des msg en debug afin de déteter des pbs
#if DEBUG
                        throw;
#endif
                    }

                    if (pReferential.HasMultiTable)
                    {
                        string filter = "ID=";
                        if (pReferential.IsDataKeyField_String)
                            filter += DataHelper.SQLString(pValueDataKeyField);
                        else
                            filter += pValueDataKeyField;

                        pReferential.drExternal = new DataRow[pReferential.dataSet.Tables.Count - 1];
                        pReferential.isNewDrExternal = new bool[pReferential.dataSet.Tables.Count - 1];
                        for (int i = 1; i < pReferential.dataSet.Tables.Count; i++)
                        {
                            //20060524 PL Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
                            pReferential.dataSet.Tables[i].CaseSensitive = true;
                            DataRow[] modifyExternalRows = pReferential.dataSet.Tables[i].Select(filter);
                            if (modifyExternalRows.GetLength(0) > 0)
                            {
                                pReferential.drExternal[i - 1] = modifyExternalRows[0];
                                pReferential.isNewDrExternal[i - 1] = false;
                            }
                            else
                            {
                                pReferential.drExternal[i - 1] = pReferential.dataSet.Tables[i].NewRow();
                                pReferential.isNewDrExternal[i - 1] = true;
                            }
                        }
                    }
                    #endregion
                }
                // RD 20110712 [17514]
                // Substituer la valeur de la colonne DTHOLIDAYVALUE, par la prochaine date du jour férié correspondant                
                if ((!pPage.IsPostBack) && pReferential.ExistsColumnDTHOLIDAYVALUE)
                {
                    // FI 20190509 [24661] null dans le paramètre date de reference
                    ReferentialTools.ValuateDTHOLIDAYVALUE(pReferential.dataRow, null);
                }

                GetExtlLinkCustomization(pReferential);

                CreateControls(pPage, pReferential, "form", pIsNewRecord, pIsDuplicateRecord);

                //PL 20130125 En mode "pIsDuplicateRecord, initialisation ici de "isNewDrExternal" afin de considérer ces lignes comme des lignes nouvelles .
                if (pReferential.HasMultiTable && pIsDuplicateRecord)
                {
                    for (int i = 1; i < pReferential.dataSet.Tables.Count; i++)
                    {
                        pReferential.isNewDrExternal[i - 1] = true;
                    }
                }

                //Warning: on repositionne ici isNewRecord sans tenir compe de pIsDuplicateRecord
                pReferential.isNewRecord = pIsNewRecord;
            }
        }

        /// <summary>
        /// Retourne le nom de la table qui pilote l'automate en fonction du nom de la table
        /// </summary>
        /// <param name="pTableName">nom de la table</param>
        /// <returns>nom de la table pour le referentiel</returns>
        public static string GetTableNameForReferential(string pTableName)
        {
            string retTableName = pTableName;

            if (pTableName.ToUpper().EndsWith("_P"))
            {
                //Table Previous Image for repository
                retTableName = retTableName.Substring(0, retTableName.Length - 2);
            }
            return retTableName;
        }

        #region public GetNewAlias
        /// <summary>
        /// Renvoie un alias numéroté si plusieurs alias existants
        /// </summary>
        /// <param name="pTableName">alias</param>
        /// <param name="pTableName">Liste des alias</param>
        /// <returns>nouveau alias numéroté</returns>
        public static string GetNewAlias(string pAlias, ref ArrayList pListAlias)
        {
            int aliasNumber = 0;
            string newAlias = pAlias;
            //
            while (pListAlias.Contains(newAlias))
            {
                aliasNumber++;
                newAlias = pAlias + aliasNumber.ToString();
            }
            //
            pListAlias.Add(newAlias);
            //
            return newAlias;
        }

        #endregion GetNewAlias
        #region GetExtlLinkCustomization
        /// <summary>
        /// Ajoute les éventuelles customisations paramétrées pour les colonnes: EXTLLINK, EXTLLINK2 et EXTLATTRB
        /// </summary>
        /// <param name="pReferential">REF la classe referential à renseigner</param>
        // EG 20180426 Analyse du code Correction [CA2202]
        private static void GetExtlLinkCustomization(ReferentialsReferential pReferential)
        {
            if (pReferential.ExistsColumnEXTL)
            {
                string SQLSelect = SQLCst.SELECT + @"DISPLAYNAME,ISMANDATORY,REGULAREXPRESSION,LISTRETRIEVAL,DATATYPE,ISHIDEINDATAGRID,DEFAULTVALUE,EXTLTYPE,";
                //if (AspTools.IsDBRevisionEqualOrGreater(3))
                SQLSelect += @"HELPTYPE,HELPDISPLAY,HELPMESSAGE" + Cst.CrLf;
                //else
                //  SQLSelect += @"null as HELPTYPE,null as HELPDISPLAY,null as HELPMESSAGE" + Cst.CrLf;  
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.DEFINEEXTLLINK.ToString() + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + "TABLENAME=" + DataHelper.SQLString(pReferential.TableName);
                //SQLSelect += SQLCst.AND + "SEQUENCENO in (1,2)";

                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect))
                {
                    while (dr.Read())
                    {
                        //int sequenceNo = Convert.ToInt32(dr["SEQUENCENO"]);
                        string extlType = dr["EXTLTYPE"].ToString();
                        string helpType = (dr["HELPTYPE"] is DBNull ? string.Empty : dr["HELPTYPE"].ToString());
                        string helpDisplay = (dr["HELPDISPLAY"] is DBNull ? string.Empty : dr["HELPDISPLAY"].ToString());
                        string helpMessage = (dr["HELPMESSAGE"] is DBNull ? string.Empty : dr["HELPMESSAGE"].ToString());

                        ReferentialsReferentialColumn rrc = null;
                        //if (pReferential.ExistsColumnEXTLLINK && sequenceNo == 1)
                        if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLLINK.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLLINK];
                        //else if (pReferential.ExistsColumnEXTLLINK2 && sequenceNo == 2)
                        else if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLLINK2.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLLINK2];
                        else if (pReferential.ExistsColumnEXTLLINK && (extlType == Cst.ExtlType.EXTLATTRB.ToString()))
                            rrc = pReferential.Column[pReferential.IndexEXTLATTRB];

                        rrc.Ressource = dr["DISPLAYNAME"].ToString();
                        rrc.RessourceSpecified = true;
                        string regularExpression = dr["REGULAREXPRESSION"].ToString();
                        if (regularExpression.Length > 0)
                            rrc.RegularExpression = regularExpression;
                        rrc.IsMandatorySpecified = true;
                        rrc.IsMandatory = Convert.ToBoolean(dr["ISMANDATORY"]);
                        rrc.DataType.value = dr["DATATYPE"].ToString();
                        rrc.IsHideInDataGridSpecified = true;
                        rrc.IsHideInDataGrid = Convert.ToBoolean(dr["ISHIDEINDATAGRID"]);

                        if (rrc.Default == null)
                            rrc.Default = new ReferentialsReferentialColumnDefault[1];
                        if (rrc.Default[0] == null)
                            rrc.Default[0] = new ReferentialsReferentialColumnDefault();
                        rrc.Default[0].Value = dr["DEFAULTVALUE"].ToString();

                        DispatchListRetrieval(ref rrc, dr["LISTRETRIEVAL"].ToString());

                        //PL 20120618 New features
                        #region rrc.Information
                        if (StrFunc.IsFilled(helpMessage))
                            rrc.InformationSpecified |= GetInformationCustomization(helpType, helpDisplay, helpMessage, out rrc.Information);
                        #endregion
                    }

                }
            }
        }
        #endregion GetExtlLinkCustomization

        #region GetInformationCustomization
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferential"></param>
        private static bool GetInformationCustomization(string pHelpType, string pHelpDisplay, string pHelpMessage,
                                                        out ReferentialInformation pInformation)
        {
            bool ret = false;
            ReferentialInformation Information = null;

            if (StrFunc.IsFilled(pHelpMessage))
            {
                Information = new ReferentialInformation
                {
                    Type = StrFunc.IsEmpty(pHelpType) ? Cst.TypeInformationMessage.Information : pHelpType.ToLower(),
                    TypeSpecified = true
                };
                pHelpDisplay = pHelpDisplay.ToLower();
                if (pHelpDisplay.IndexOf("label") >= 0)
                {
                    ret = true;
                    Information.LabelMessageSpecified = true;
                    if (pHelpMessage.Length > 50)
                    {
                        Information.LabelMessage = pHelpMessage.Substring(0, 47) + "...";
                        pHelpDisplay += "tooltip";
                    }
                    else
                    {
                        Information.LabelMessage = pHelpMessage;
                    }
                }
                if (pHelpDisplay.IndexOf("popup") >= 0)
                {
                    ret = true;
                    Information.PopupMessageSpecified = true;
                    Information.PopupMessage = pHelpMessage;
                }
                if (pHelpDisplay.IndexOf("tooltip") >= 0)
                {
                    ret = true;
                    Information.ToolTipMessageSpecified = true;
                    Information.ToolTipMessage = new ReferentialInformationToolTipMessage
                    {
                        Value = pHelpMessage
                    };
                }
            }

            pInformation = Information;
            return ret;
        }
        #endregion

        /// <summary>
        /// Enregistre dans la database, les modifications apportées à la classe Referential
        /// </summary>
        /// <param name="pPage">Page source</param>
        /// <param name="pDbTransaction">DbTransaction</param>
        /// <param name="pIdMenu">ID du menu</param>
        /// <param name="pReferential">Classe referential</param>
        /// <param name="opRowsAffected">OUT nb de lignes affectées</param>
        /// <param name="opMessage">OUT message d'erreur (traduit)</param>
        /// <param name="opError">OUT erreur (brute, non traduite)</param>
        /// <param name="opDataKeyField">OUT nom de colonne du DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <param name="opDataKeyFieldNewValue">OUT nouvelle valeur de la colonne DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <param name="opDataKeyFieldOldValue">OUT ancienne valeur de la colonne DataKeyField (dans le cas d'un nouvel enregistrement)</param>
        /// <returns>(int)ErrLevel</returns>
        /// EG [XXXXX][WI417] DataParameter des colonnes d'audit datetime2/timestamp(7)
        public static Cst.ErrLevel SaveReferential(IDbTransaction pDbTransaction, string pIdMenu,
            ReferentialsReferential pReferential,
            out int opRowsAffected, out string opMessage, out string opError,
            out string opDataKeyField, out string opDataKeyFieldNewValue, out string opDataKeyFieldOldValue)
        {
            opDataKeyField = opDataKeyFieldNewValue = opDataKeyFieldOldValue = string.Empty;

            #region Add new row(s) in datable(s)
            if (pReferential.isNewRecord)
            {
                pReferential.dataSet.Tables[0].Rows.Add(pReferential.dataRow);
            }

            if ((pReferential.drExternal != null) && (pReferential.dataRow.RowState != DataRowState.Deleted))
            {
                for (int i = 1; i <= pReferential.drExternal.GetLength(0); i++)
                {
                    if (pReferential.isNewDrExternal[i - 1])
                    {
                        pReferential.dataSet.Tables[i].Rows.Add(pReferential.drExternal[i - 1]);
                    }
                }
            }
            #endregion

            Cst.ErrLevel ret = SQLReferentialData.ApplyChangesInSQLTable(SessionTools.CS, pDbTransaction, pReferential, pReferential.dataSet,
                out opRowsAffected, out opMessage, out opError, pIdMenu);

            //New record --> Récupération de la valeur de la colonne DataKeyField 
            if ((ret == Cst.ErrLevel.SUCCESS) && (pReferential.isNewRecord))
            {
                if (pReferential.ExistsColumnDataKeyField)
                {
                    DataRow newCreatedRow = pReferential.dataSet.Tables[0].Rows[pReferential.dataSet.Tables[0].Rows.Count - 1];
                    ReferentialsReferentialColumn rrcDataKeyField = pReferential.Column[pReferential.IndexDataKeyField];
                    //DataKeyField de type "IsIdentity"
                    //S'il existe un KeyField différent de DataKeyField --> on l'utilise pour identifier le record et ainsi récupérer la valeur de l'identity généré
                    if ((rrcDataKeyField.IsIdentitySpecified) && (rrcDataKeyField.IsIdentity.Value) && (!rrcDataKeyField.IsIdentity.sourceSpecified)
                        && (pReferential.IndexKeyField != -1) && (pReferential.IndexKeyField != pReferential.IndexDataKeyField))
                    {
                        #region DataKeyfield de type IDENTITY (Oracle SEQUENCE)
                        ReferentialsReferentialColumn rrcKeyField = pReferential.Column[pReferential.IndexKeyField];

                        if (newCreatedRow[rrcKeyField.ColumnName].ToString() != Cst.AUTOMATIC_COMPUTE)
                        {
                            DateTime dtIns = DateTime.SpecifyKind(Convert.ToDateTime(newCreatedRow["DTINS"]), DateTimeKind.Utc);

                            object dataKeyFieldValue = GetDataKeyFieldNewValue2(SessionTools.CS, pDbTransaction,
                                rrcDataKeyField.ColumnName, pReferential.TableName,
                                rrcKeyField.ColumnName, rrcKeyField.DataType.value, newCreatedRow[rrcKeyField.ColumnName],
                                Convert.ToInt32(newCreatedRow["IDAINS"]), dtIns, true);

                            newCreatedRow.BeginEdit();
                            opDataKeyFieldOldValue = newCreatedRow[rrcDataKeyField.ColumnName].ToString();
                            newCreatedRow[rrcDataKeyField.ColumnName] = dataKeyFieldValue;
                            newCreatedRow.EndEdit();

                            opDataKeyField = rrcDataKeyField.ColumnName;
                            opDataKeyFieldNewValue = dataKeyFieldValue.ToString();
                        }
                        #endregion
                    }
                    //DataKeyField saisisable
                    else if ((!rrcDataKeyField.IsIdentitySpecified) || (!rrcDataKeyField.IsIdentity.Value) || (rrcDataKeyField.IsIdentity.sourceSpecified))
                    {
                        #region DataKeyField saisi
                        opDataKeyField = rrcDataKeyField.ColumnName;
                        opDataKeyFieldNewValue = newCreatedRow[rrcDataKeyField.ColumnName].ToString();
                        opDataKeyFieldOldValue = null; //TBD
                        #endregion
                    }
                }
            }

            //FI 20150720 [20982] Purge du cache Spheres®  
            if (ret == Cst.ErrLevel.SUCCESS && opRowsAffected >= 1)
            {
                ReferentialTools.RemoveCache(pReferential.dataSet.Tables[0].TableName);
            }

            //FI 20150720 [20982] Maj de Sessionrestrict (Si TableName est une OTCml_TBL) 
            if ((ret == Cst.ErrLevel.SUCCESS) && (opRowsAffected >= 1))
            {
                if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pReferential.TableName))
                {
                    // FI 20150529 [20982] Mise en commentaire SessionTools.SetRestriction
                    // Ne sert à rien de recharger SESSIONRESTRICT au complet
                    // Il est juste nécessaire d'ajouter un élément dans SESSIONRESTRICT en cas de création
                    // SessionTools.SetRestriction((Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pReferential.TableName));

                    // FI 20150529 [20982] call AddSessionRestrict
                    ReferentialTools.AddItemSessionRestrict(pReferential);
                }
            }

            return ret;
        }


        /// <summary>
        /// Ajoute un enregistrement dans SESSIONRESTRICT lorsqu'il y a ajout d'un nouvel élément via le référentiel
        /// </summary>
        /// FI 20150529 [20982] Add method
        /// FI 20150720 [20982] Modify
        public static void AddItemSessionRestrict(ReferentialsReferential pReferential)
        {

            if (pReferential.isNewRecord && SessionTools.User.IsApplySessionRestrict())
            {
                if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pReferential.TableName))
                {
                    Cst.OTCml_TBL tbl = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pReferential.TableName);

                    RestrictionUserBase restricUser = null;
                    switch (tbl)
                    {
                        case Cst.OTCml_TBL.ACTOR:
                            restricUser = new RestrictionActor();
                            break;
                        case Cst.OTCml_TBL.INSTRUMENT:
                            restricUser = new RestrictionInstrument();
                            break;
                        case Cst.OTCml_TBL.MARKET:
                            restricUser = new RestrictionMarket();
                            break;
                        case Cst.OTCml_TBL.IOTASK:
                            restricUser = new RestrictionIOTask();
                            break;
                        default:
                            break;
                            //Nothing
                    }

                    if (null != restricUser)
                    {
                        DataRow row = pReferential.dataSet.Tables[0].Rows[(pReferential.dataSet.Tables[0].Rows.Count - 1)];

                        string columnId = OTCmlHelper.GetColunmID(pReferential.TableName);
                        // FI 20150720 [20982] Add Message d'erreur en présence de valeur null
                        if (row[columnId] == Convert.DBNull)
                            throw new NotSupportedException(StrFunc.AppendFormat("column ({0}): value is DBNull. This value is not Supported", columnId));
                        if (row["IDENTIFIER"] == Convert.DBNull)
                            throw new NotSupportedException(StrFunc.AppendFormat("column ({0}): value is DBNull. This value is not Supported", "IDENTIFIER"));

                        restricUser.SetItemRestriction(Convert.ToInt32(row[columnId]), Convert.ToString(row["IDENTIFIER"]), true);

                        SqlSessionRestrict sqlSessionRestrict = new SqlSessionRestrict(SessionTools.CS, SessionTools.AppSession);
                        sqlSessionRestrict.SetRestrictUseSelectUnion(restricUser);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pDataKeyFieldColumnName"></param>
        /// <param name="pTableName"></param>
        /// <param name="pKeyFieldColumnName"></param>
        /// <param name="pKeyFieldDataType"></param>
        /// <param name="pKeyfieldValue"></param>
        /// <param name="pIDAValue"></param>
        /// <param name="pDTValue"></param>
        /// <param name="pIsUseINS"></param>
        /// <returns></returns>
        /// FI 20201019 [XXXXX] Add
        /// EG [XXXXX][WI417] DataParameter des colonnes d'audit datetime2/timestamp(7)
        private static object GetDataKeyFieldNewValue2(string pCS, IDbTransaction pDbTransaction,
            string pDataKeyFieldColumnName, string pTableName, string pKeyFieldColumnName, string pKeyFieldDataType,
            object pKeyfieldValue, int pIDAValue, DateTime pDTValue, bool pIsUseINS)
        {
            DataParameters dp = new DataParameters();

            Boolean isKeyfieldValueNull = (pKeyfieldValue == Convert.DBNull);
            if (false == isKeyfieldValueNull)
            {
                if (TypeData.IsTypeInt(pKeyFieldDataType))
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.Int64), pKeyfieldValue);
                else if (TypeData.IsTypeBool(pKeyFieldDataType))
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.Boolean), pKeyfieldValue);
                else if (TypeData.IsTypeDec(pKeyFieldDataType))
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.Decimal), pKeyfieldValue);
                else if (TypeData.IsTypeDate(pKeyFieldDataType))
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.Date), pKeyfieldValue);
                else if (TypeData.IsTypeDateTime(pKeyFieldDataType))
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.DateTime), pKeyfieldValue);
                else
                    dp.Add(new DataParameter(pCS, "KEYFIELD", DbType.AnsiString), pKeyfieldValue);
            }

            dp.Add(DataParameter.GetParameter(pCS, pIsUseINS ? DataParameter.ParameterEnum.IDAINS : DataParameter.ParameterEnum.IDAUPD), pIDAValue);
            // Usage d'un OracleDbType.Date sous Oracle® et sqldbType.datetime sous SQL SERVER®
            dp.Add(DataParameter.GetParameter(pCS, pIsUseINS ? DataParameter.ParameterEnum.DTINSDATETIME2 : DataParameter.ParameterEnum.DTUPDDATETIME2), pDTValue);

            string SQLQuery = $"{SQLCst.SELECT} {pDataKeyFieldColumnName} as DATAKEYFIELDVALUE" + Cst.CrLf;
            SQLQuery += $"{SQLCst.FROM_DBO}{pTableName}" + Cst.CrLf;
            if (false == isKeyfieldValueNull)
                SQLQuery += $"{SQLCst.WHERE}({pKeyFieldColumnName}=@KEYFIELD)";
            else
                SQLQuery += $"{SQLCst.WHERE}({pKeyFieldColumnName} is null)";

            if (pIsUseINS)
            {
                SQLQuery += SQLCst.AND + "(IDAINS=@IDAINS)";
                SQLQuery += SQLCst.AND + "(DTINS=@DTINS)";
            }
            else
            {
                SQLQuery += SQLCst.AND + "(IDAUPD=@IDAUPD)";
                SQLQuery += SQLCst.AND + "(DTUPD=@DTUPD)";
            }

            QueryParameters qryParameters = new QueryParameters(pCS, SQLQuery, dp);

            object ret = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            return ret;
        }

        #region public UpdateDataRowFromControls
        /// <revision>
        ///     <version>1.0.4</version><date>20041202</date><author>OD</author>
        ///     <comment>
        ///     Utilisation de la methode GetStringDefaultForReferencialColumn() pour recuperer le defaultValue
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>        
        /// <summary>
        /// Mets à jour les données contenu dans le datarow de la classe Referential en fonction des valeurs dans les controls de la page appelante
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential à mettre à jour</param>
        // EG 20210211 [25660] L'algorithme de hachage MD5 est déprécié. Méthode de transformation douce lors de la connexion
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        public static void UpdateDataRowFromControls2(PageBase pPage, ReferentialsReferential pReferential, IDbTransaction pDbTransaction)
        {
            Control ctrRef;
            TextBox txtRef;
            bool isVisible, isEnabled;

            pReferential.dataRow.BeginEdit();

            //PL 20161124 - RATP 4Eyes - MakingChecking
            bool isMakingChecking_ActionChecking = false;
            DataRow currentRow;
            if (pReferential.ExistsMakingChecking)
            {
                if (!pReferential.isNewRecord)
                {
                    currentRow = pReferential.dataRow;
                    if (BoolFunc.IsTrue(currentRow["ISCHK"]))
                    {
                        //Record actuellement CHECKED --> Making
                        isMakingChecking_ActionChecking = false;
                        //pReferential.isNewRecord = true;
                    }
                    else
                    {
                        //Record actuellement UNCHECKED --> Making ou Checking
                        try
                        {
                            int idaLast = Convert.ToInt32(currentRow["IDAINS"]);
                            if (currentRow["IDAUPD"] != Convert.DBNull)
                                idaLast = Convert.ToInt32(currentRow["IDAUPD"]);
                            //if (Convert.ToInt32(currentRow[(referential.isNewRecord ? "IDAINS" : "IDAUPD")]) != SessionTools.Collaborator_IDA)
                            if (idaLast == SessionTools.Collaborator_IDA)
                            {
                                //Utilisateur courant égale au dernier utilisateur ayant modifié les données --> Making
                                isMakingChecking_ActionChecking = false;
                            }
                            else
                            {
                                //Utilisateur courant différent du dernier utilisateur ayant modifié les données --> Cheking
                                isMakingChecking_ActionChecking = true;
                            }
                        }
                        catch { }
                    }
                }
            }

            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                ReferentialsReferentialColumn rrc = pReferential.Column[i];
                bool isColumnMandatory = (rrc.IsMandatorySpecified && rrc.IsMandatory);

                #region External DR BeginEdit
                if (rrc.IsAdditionalData)
                {
                    currentRow = pReferential.drExternal[rrc.ExternalFieldID];
                    if (currentRow.RowState == DataRowState.Deleted)
                    {
                        //NB: Dans le cas des rôles, la ligne se trouve deleted lorsque le rôle et décoché.
                        //    On ne peut donc pas mettre à jour l'acteur associé, ...
                        continue;
                    }
                    else
                    {
                        currentRow.BeginEdit();
                    }
                }
                else
                {
                    currentRow = pReferential.dataRow;
                }
                #endregion External DR BeginEdit

                isEnabled = false;
                if (pReferential.isNewRecord)
                {
                    if (rrc.IsUpdatableSpecified && rrc.IsUpdatable.isupdatableincreationSpecified)
                        isEnabled = rrc.IsUpdatable.isupdatableincreation;
                    else
                        isEnabled = true;
                }
                else if (rrc.IsUpdatableSpecified && rrc.IsUpdatable.Value)
                {
                    isEnabled = true;
                }
                isVisible = !(rrc.IsIdentity.Value || rrc.IsHide);

                #region Cas particulier d'un Identity avec Source (eg. GETID) PL 20130403
                if (pReferential.ExistsColumnIDENTITY && rrc.IsIdentity.Value && rrc.IsIdentity.sourceSpecified)
                {
                    if (pReferential.isNewRecord)
                    {
                        //MAJ uniquement en cas d'insertion
                        if (rrc.IsIdentity.source.ToUpper().Contains("SRC=TRADEID") || rrc.IsIdentity.source.ToUpper().Contains("SRC=IDTRADE"))
                        {
                            #region TRADEID/IDTRADE
                            //NB: La source est de type Src=TRADEID;Instr=<InstrumentIdentifier>;Book=<BookColumn>;TrdDt=<TradeDateColumn>;[BizDt=<BusinessDateColumn>;]
                            //    ex. Src=TRADEID;Instr=ExchangeTradedFuture;Book=IDB_PAY;TrdDt=DTBUSINESS;

                            SQL_Instrument sqlInstrument = null;
                            int idAEntity = 0;
                            DateTime tradeDate = DateTime.MinValue;
                            DateTime businessDate = DateTime.MinValue;

                            string[] srcs = rrc.IsIdentity.source.Split(";".ToCharArray());
                            foreach (string src in srcs)
                            {
                                if (src.ToUpper().StartsWith("INSTR"))
                                {
                                    string instrument = src.Remove(0, "INSTR".Length + 1);
                                    sqlInstrument = new SQL_Instrument(SessionTools.CS, instrument);
                                }
                                else if (src.ToUpper().StartsWith("BOOK"))
                                {
                                    #region Entity
                                    string columnName = src.Remove(0, "BOOK".Length + 1);
                                    if (pReferential[columnName] != null)
                                    {
                                        ReferentialsReferentialColumn rrcBD = pReferential[columnName];
                                        string controlID = ControlID.GetID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.DDL);
                                        ctrRef = pPage.FindControl(controlID);

                                        DropDownList ddl = (DropDownList)ctrRef;
                                        string data = ddl.SelectedValue;
                                        if (IntFunc.IsPositiveInteger(data))
                                        {
                                            SQL_Book sqlBook = new SQL_Book(SessionTools.CS, Convert.ToInt32(data));
                                            if (sqlBook.IsLoaded)
                                            {
                                                idAEntity = sqlBook.IdA_Entity;
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                else if (src.ToUpper().StartsWith("TRDDT"))
                                {
                                    #region Trade date
                                    string columnName = src.Remove(0, "TRDDT".Length + 1);
                                    if (pReferential[columnName] != null)
                                    {
                                        ReferentialsReferentialColumn rrcBD = pReferential[columnName];
                                        string controlID = ControlID.GetID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.TXT);
                                        ctrRef = pPage.FindControl(controlID);

                                        txtRef = ctrRef as TextBox;
                                        string data = txtRef.Text.Trim();
                                        if (!String.IsNullOrEmpty(data))
                                        {
                                            tradeDate = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null).StringToDateTime(data);
                                        }
                                    }
                                    #endregion
                                }
                                else if (src.ToUpper().StartsWith("BIZDT"))
                                {
                                    #region Business date
                                    string columnName = src.Remove(0, "BIZDT".Length + 1);
                                    if (pReferential[columnName] != null)
                                    {
                                        ReferentialsReferentialColumn rrcBD = pReferential[columnName];
                                        string controlID = ControlID.GetID(rrcBD.ColumnName, rrcBD.DataType.value, Cst.TXT);
                                        ctrRef = pPage.FindControl(controlID);

                                        txtRef = ctrRef as TextBox;
                                        string data = txtRef.Text.Trim();
                                        if (!String.IsNullOrEmpty(data))
                                        {
                                            businessDate = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null).StringToDateTime(data);
                                        }
                                    }
                                    #endregion
                                }
                            }
                            if (DtFunc.IsDateTimeEmpty(businessDate))
                            {
                                businessDate = tradeDate;
                            }
                            else if (DtFunc.IsDateTimeEmpty(tradeDate))
                            {
                                tradeDate = businessDate;
                            }

                            TradeRDBMSTools.BuildTradeIdentifier(SessionTools.CS, pDbTransaction,
                                sqlInstrument, idAEntity, null, tradeDate, businessDate,
                                out string newIdentifier, out string prefix, out string suffix);

                            //Constitution du nouvel identifiant 
                            currentRow[rrc.DataField] = prefix + newIdentifier + suffix;
                            #endregion
                        }
                        else if (rrc.IsIdentity.source.ToUpper().Contains("SRC=GETID") || rrc.IsIdentity.source.ToUpper().Contains("SRC=ID"))
                        {
                            #region GETID/ID
                            currentRow[rrc.DataField] = Convert.DBNull;//TBD
                            #endregion
                        }
                    }
                }
                #endregion

                #region isVisible
                if (isVisible)
                {
                    if (isEnabled)
                    {
                        string columnName = rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty);
                        string controlID = ControlID.GetID(columnName, rrc.DataType.value,
                            // 20100607 MF Autocomplete - if the autocomplete has been enabled then the target control est l'hidden field 
                            (ReferentialTools.IsDataForDDL(rrc) ? Cst.DDL : (rrc.AutoCompleteRelationEnabled ? Cst.HID : null)));

                        ctrRef = pPage.FindControl(controlID);

                        if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            #region IsTypeBool
                            CheckBox chkCtrl = (CheckBox)ctrRef;
                            if (rrc.IsExternal2)
                                //le champ est de type string
                                currentRow[rrc.DataField] = (chkCtrl.Checked ? "TRUE" : "FALSE");
                            else
                                currentRow[rrc.DataField] = (chkCtrl.Checked ? 1 : 0);
                            #endregion IsTypeBool
                        }
                        else if (TypeData.IsTypeDateOrDateTime(rrc.DataType.value))
                        {
                            #region IsTypeDateOrDateTime
                            txtRef = ctrRef as TextBox;
                            string data = txtRef.Text.Trim();
                            //PL 20110921
                            //if (false == !isColumnMandatory)
                            if (StrFunc.IsEmpty(data))
                            {
                                if (isColumnMandatory)
                                    currentRow[rrc.DataField] = DateTime.MinValue;
                                else
                                    currentRow[rrc.DataField] = Convert.DBNull;
                            }
                            else
                            {
                                if (rrc.IsExternal2)
                                {
                                    //PL 20111227 Nouveau format de stockage: FmtISODate 
                                    //string FmtDate = TypeData.IsTypeDate(rrc.DataType) ? DtFunc.FmtDateyyyyMMdd : DtFunc.FmtISODateTime;
                                    string FmtDate = TypeData.IsTypeDate(rrc.DataType.value) ? DtFunc.FmtISODate : DtFunc.FmtISODateTime;
                                    currentRow[rrc.DataField] = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null).GetDateTimeString(data, FmtDate);
                                }
                                else
                                {
                                    DateTime dtResult = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null).StringToDateTime(data);
                                    if (DtFunc.IsDateTimeFilled(dtResult))
                                    {
                                        currentRow[rrc.DataField] = dtResult;   //GLOPDATE
                                    }
                                }
                            }
                            #endregion IsTypeDateOrDateTime
                        }
                        //20090909 PL Add IsTypeTime()
                        else if (TypeData.IsTypeTime(rrc.DataType.value))
                        {
                            #region IsTypeTime
                            txtRef = ctrRef as TextBox;
                            string data = txtRef.Text.Trim();
                            if (StrFunc.IsEmpty(data) && !isColumnMandatory)
                            {
                                currentRow[rrc.DataField] = Convert.DBNull;
                            }
                            else
                            {
                                DateTime dtMinDefaultValue = new DtFunc().StringyyyyMMddToDateTime("19000101");
                                currentRow[rrc.DataField] = DtFunc.AddTimeToDate(dtMinDefaultValue, new DtFunc().StringToDateTime(data, DtFunc.FmtISOShortTime));
                            }
                            #endregion IsTypeTime
                        }
                        else //String
                        {
                            #region IsDataForDDL
                            if (ReferentialTools.IsDataForDDL(rrc))
                            {
                                bool isFK = !((!rrc.IsForeignKeyField) || (StrFunc.IsEmpty(pReferential.ValueForeignKeyField)));
                                if (isFK)
                                {
                                    Label lbl = (Label)ctrRef;
                                    currentRow[rrc.DataField] = lbl.Attributes["DDLValue"].ToString();
                                }
                                else
                                {
                                    if (rrc.IsAutoPostBack && StrFunc.IsEmpty(pPage.Request.Form[controlID]))
                                    {
                                        //PL 20111003 Ajout du if (rrc.IsAutoPostBack...) 
                                        //            Rq. Sur une DDL AutoPostBack mise à jour et disabled via JS, ASP.NET ne récupère pas la nouvelle valeur initialisée par JS. 
                                        //                Utilisation ici, faute de mieux, de IsAutoPostBack et de Request.Form[], pour identifier cette DDL particulière...
                                        //            Ex.: DDL ASSETCATEGORY dans COLLATERALPRIORITY
                                        currentRow[rrc.DataField] = Convert.DBNull;
                                    }
                                    else
                                    {
                                        DropDownList ddl = (DropDownList)ctrRef;
                                        if (TypeData.IsTypeInt(rrc.DataType.value) && (!isColumnMandatory))
                                        {
                                            if (
                                                (ddl.Items.Count == 0)
                                                ||
                                                ((ddl.SelectedIndex == 0) && StrFunc.IsEmpty(ddl.SelectedValue))
                                                )
                                            {
                                                //Item null is selected
                                                currentRow[rrc.DataField] = Convert.DBNull;
                                            }
                                            else
                                            {
                                                // EG 20150920 [21314] Int (int32) to Long (Int64) 
                                                currentRow[rrc.DataField] = Convert.ToInt64(ddl.SelectedValue);
                                            }
                                        }
                                        else
                                        {
                                            if (
                                                (ddl.Items.Count == 0)
                                                ||
                                                (StrFunc.IsEmpty(ddl.SelectedValue) && !isColumnMandatory)
                                                )
                                            {
                                                currentRow[rrc.DataField] = Convert.DBNull;
                                            }
                                            else
                                            {
                                                currentRow[rrc.DataField] = ddl.SelectedValue;
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion IsDataForDDL
                            #region AutoComplete
                            else if (rrc.AutoCompleteRelationEnabled)
                            {
                                if (ctrRef is HiddenField)
                                {
                                    HiddenField hdnRef = ctrRef as HiddenField;

                                    string data = hdnRef.Value;

                                    if (StrFunc.IsEmpty(data.Trim()) && !isColumnMandatory)
                                        currentRow[rrc.DataField] = Convert.DBNull;
                                    else
                                    {
                                        // EG 20130520 AUTOCOMPLETE Le champ caché lié à l'autocomplete est encore valorisé !!! 
                                        // qu'en est-il du champ TEXT ?
                                        currentRow[rrc.DataField] = data;
                                        string idTxt = ControlID.GetID(rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty), null, "TMP");
                                        txtRef = pPage.FindControl(idTxt) as TextBox;
                                        // mais le champ Text associé est vierge (effacement de la zone optionnelle)
                                        if ((null != txtRef) && StrFunc.IsEmpty(txtRef.Text))
                                            currentRow[rrc.DataField] = Convert.DBNull;
                                    }
                                }
                                else
                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                       String.Format("No valid control: {0} - ID: {1}",
                                           ctrRef.GetType(), ctrRef.ID));
                            }
                            #endregion
                            #region TextBox
                            else
                            {
                                txtRef = ctrRef as TextBox;
                                string data = txtRef.Text;
                                //PL 20130607 AUTOMATIC_COMPUTE
                                string resAUTOMATIC_COMPUTE = Ressource.GetString(Cst.AUTOMATIC_COMPUTE.ToString());
                                //if (data == Ressource.GetString(Cst.AUTOMATIC_COMPUTE.ToString()))
                                if (data.StartsWith(resAUTOMATIC_COMPUTE))
                                {
                                    string defaultValue = rrc.GetStringDefaultValue(pReferential.TableName);
                                    if (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue)
                                    {
                                        //data = Cst.AUTOMATIC_COMPUTE.ToString();
                                        data = Cst.AUTOMATIC_COMPUTE.ToString() + data.Remove(0, resAUTOMATIC_COMPUTE.Length);
                                    }
                                }

                                if (String.IsNullOrEmpty(data) && (rrc.ColumnName == "DISPLAYNAME") && (pReferential["IDENTIFIER"] != null))
                                {
                                    //PL 20130403 New feature
                                    currentRow[rrc.DataField] = currentRow["IDENTIFIER"];
                                }
                                else if (StrFunc.IsEmpty(data.Trim()) && !isColumnMandatory)
                                {
                                    currentRow[rrc.DataField] = Convert.DBNull;
                                }
                                else
                                {
                                    bool setInDataRow = true;
                                    #region TextBoxMode.Password
                                    if (rrc.TextMode == TextBoxMode.Password.ToString())
                                    {
                                        if (data == Cst.StringForTextBoxModePassword)
                                        {
                                            //Data not modify --> no update in database
                                            setInDataRow = false;
                                        }
                                        else
                                        {
                                            //Applic hash
                                            string algorithm = rrc.Coding;
                                            if (StrFunc.IsEmpty(algorithm))
                                            {
                                                // EG 20210211 [25660] 
                                                algorithm = SystemSettings.GetAppSettings_HashAlgorithm.Item1;
                                            }
                                            data = StrFunc.HashData(data, algorithm);
                                        }
                                    }
                                    #endregion
                                    if (setInDataRow)
                                    {
                                        //FI 20100309 le "." est désormais autorisé
                                        if (TypeData.IsTypeDec(rrc.DataType.value))
                                            currentRow[rrc.DataField] = DecFunc.DecValue(data);
                                        else
                                            currentRow[rrc.DataField] = data;
                                    }
                                }
                            }
                            #endregion TextBox
                        }
                    }
                }
                #endregion isVisible

                #region !isVisible
                if (false == isVisible)
                {
                    //Update data may be not captured (data not visibled on the screen invisibles à l'écran)
                    if ((rrc.IsAdditionalData && pReferential.isNewDrExternal[rrc.ExternalFieldID]))
                    {
                        try
                        {
                            string defaultValue = rrc.GetStringDefaultValue(pReferential.TableName);
                            if (StrFunc.IsEmpty(defaultValue))
                                currentRow[rrc.DataField] = Convert.DBNull;
                            else
                                currentRow[rrc.DataField] = defaultValue;
                        }
                        catch
                        {
                            currentRow[rrc.DataField] = Convert.DBNull;
                        }
                    }
                }
                #endregion !isVisible

                #region External DR EndEdit
                if (rrc.IsAdditionalData)
                {
                    bool isNewDr = (pReferential.isNewDrExternal[rrc.ExternalFieldID]);
                    //bool isRemoveDr = false;
                    if (rrc.IsExternal)
                    {
                        #region rrc.IsExternal
                        currentRow["TABLENAME"] = rrc.ExternalTableName;
                        currentRow["IDENTIFIER"] = rrc.ExternalIdentifier;
                        currentRow["ID"] = pReferential.dataRow[pReferential.IndexColSQL_DataKeyField];
                        // FI 20200820 [25468] Date systèmes en UTC 
                        currentRow[(pReferential.isNewDrExternal[rrc.ExternalFieldID] ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                        currentRow[(pReferential.isNewDrExternal[rrc.ExternalFieldID] ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;
                        #endregion rrc.IsExternal
                    }
                    else if (rrc.IsRole || rrc.IsItem)
                    {

                        #region rrc.IsRole or rrc.IsItem
                        bool isSet = ((rrc.IsRole && rrc.DataField.StartsWith("IDROLE")) || (rrc.IsItem));
                        if (isSet)
                        {
                            if (currentRow[rrc.DataField].ToString() == "1")
                            {
                                //"1" --> Checked, donc rôle ou élément accordé (nouvellement ou déjà en place).
                                currentRow[rrc.DataField] = rrc.AliasTableName.Substring(2);
                                if (isNewDr)
                                {
                                    //Nouvelle ligne, donc le rôle ou l'élément vient d'être accordé (sinon RAS)
                                    if (currentRow.Table.Columns.IndexOf("DTENABLED") >= 0)
                                    {
                                        string controlID = ControlID.GetID("DTENABLED", TypeData.TypeDataEnum.date.ToString());
                                        ctrRef = pPage.FindControl(controlID);
                                        string data = ((TextBox)ctrRef).Text.Trim();
                                        currentRow["DTENABLED"] = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null).StringToDateTime(data);
                                    }
                                    currentRow["ID"] = pReferential.dataRow[pReferential.IndexColSQL_DataKeyField];
                                    // FI 20200820 [25468] Date systèmes en UTC 
                                    currentRow[(pReferential.isNewDrExternal[rrc.ExternalFieldID] ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                                    currentRow[(pReferential.isNewDrExternal[rrc.ExternalFieldID] ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;

                                    //PL 20170913 [23409] Newness
                                    if (rrc.IsItem && pReferential.Items.addcolumnkeySpecified) //ex. CONTRACTG --> CONTRACTCATEGORY
                                    {
                                        currentRow[pReferential.Items.addcolumnkey] = pReferential.Items.addcolumnkeyvalue; //ex. CONTRACTG --> "DerivativeContract", "CommodityContract"
                                    }
                                }
                            }
                            else
                            {
                                //"0" --> Unchecked, donc rôle ou l'élément non accordé (nouvellement ou jamais en place).
                                if (isNewDr)
                                    //Nouvelle ligne, donc le rôle ou l'élément n'était pas accordé
                                    pReferential.isNewDrExternal[rrc.ExternalFieldID] = false;
                                else
                                {
                                    //Non nouvelle ligne, donc le rôle ou l'élément vient d'être retiré
                                    currentRow.Delete();
                                }
                            }
                        }
                        #endregion rrc.IsRole or rrc.IsItem
                    }
                    currentRow.EndEdit();
                    //if (isRemoveDr)
                    //    pReferential.drExternal[rrc.ExternalFieldID].Delete();
                    //else
                    //    pReferential.drExternal[rrc.ExternalFieldID] = currentRow;
                    //
                    //20090109 PL Set comment currentRow = pReferential.dataRow;
                }
                //else
                //{
                //    pReferential.dataRow = currentRow;
                //}
                #endregion External DR EndEdit
            }

            currentRow = pReferential.dataRow;//20090109 PL Move here
            //Update DTINS, IDAINS and DTUPD, IDAUPD
            #region DTINS, IDAINS and DTUPD, IDAUPD, ROWATTRIBUT et xxCHK
            if ((pReferential.isNewRecord && (pReferential.ExistsColumnsINS)) || (!pReferential.isNewRecord && (pReferential.ExistsColumnsUPD)))
            {
                currentRow[(pReferential.isNewRecord ? "DTINS" : "DTUPD")] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                currentRow[(pReferential.isNewRecord ? "IDAINS" : "IDAUPD")] = SessionTools.Collaborator_IDA;
            }
            try
            {
                //PL 20161124 - RATP 4Eyes - MakingChecking
                if (pReferential.ExistsMakingChecking)
                {
                    if (isMakingChecking_ActionChecking)
                    {
                        //----------------------------
                        //CHECKING
                        //----------------------------
                        // FI 20200820 [25468] Date systèmes en UTC 
                        currentRow["DTCHK"] = OTCmlHelper.GetDateSysUTC(SessionTools.CS);
                        currentRow["IDACHK"] = SessionTools.Collaborator_IDA;
                        currentRow["ISCHK"] = true;
                    }
                    else
                    {
                        //----------------------------
                        //MAKING
                        //----------------------------
                        //if (pReferential.isNewRecord)
                        //if (BoolFunc.IsTrue(currentRow["ISCHK"]))
                        //{ 
                        //currentRow["IDA"] = Convert.DBNull;
                        //currentRow["ROWVERSION"] = Convert.DBNull;
                        //currentRow["IDCHK"] = Convert.ToInt32(currentRow["IDCHK"]) * (-1);
                        //}
                        currentRow["DTCHK"] = Convert.DBNull;
                        currentRow["IDACHK"] = Convert.DBNull;
                        currentRow["ISCHK"] = false;
                    }
                }

                if (pReferential.isNewRecord)
                {
                    //En cas de duplication (= création) réinitialisation des colonnes UPD, ...
                    if (pReferential.ExistsColumnsUPD)
                    {
                        currentRow["DTUPD"] = Convert.DBNull;
                        currentRow["IDAUPD"] = Convert.DBNull;
                    }
                    currentRow["ROWATTRIBUT"] = Convert.DBNull;
                }
            }
            catch { }
            #endregion

            pReferential.dataRow.EndEdit();
        }
        #endregion UpdateDataRowFromControls
        #region public ValuateDTHOLIDAYVALUE
        /// <summary>
        /// Pour toutes les lignes du DataTable {pDt}, Valorise la colonne DTHOLIDAYVALUE avec la date du prochain jour férié correspondant
        /// </summary>
        /// <param name="pDt"></param>
        public static void ValuateDTHOLIDAYVALUE(DataTable pDt, Nullable<DateTime> pDtRef)
        {
            int totalRows = pDt.Rows.Count;
            int currentRow = 0;
            while (currentRow < totalRows)
            {
                ValuateDTHOLIDAYVALUE(pDt.Rows[currentRow], pDtRef);
                currentRow++;
            }
        }

        /// <summary>
        /// Valorisation, à partir de DTHOLIDAYVALUE, de la colonne fictive DTHOLIDAYNEXTDATE avec la date du prochain jour férié.
        /// </summary>
        /// <param name="pRow"></param>
        /// <param name="pEfs_BusinessCenters"></param>
        public static void ValuateDTHOLIDAYVALUE(DataRow pRow, Nullable<DateTime> pDtRef)
        {

            string data = pRow[Cst.OTCml_COL.DTHOLIDAYVALUE.ToString()].ToString();

            if (StrFunc.IsFilled(data))
            {
                EFS_BusinessCenters bcs = new EFS_BusinessCenters(SessionTools.CS);
                DateTime holidayDate = bcs.GetDate(data, pDtRef);
                if (DtFunc.IsDateTimeFilled(holidayDate))
                {
                    //data = DtFunc.DateTimeToString(holidayDate, DtFunc.FmtShortDate);
                    pRow.BeginEdit();
                    pRow[Cst.OTCml_COL.DTHOLIDAYNEXTDATE.ToString()] = holidayDate;
                    pRow.EndEdit();
                }
                //else
                //    data = string.Empty;
            }
            //else
            //    data = string.Empty;

            //pRow.BeginEdit();
            ////pRow[Cst.OTCml_COL.DTHOLIDAYVALUE.ToString()] = data;
            //pRow.EndEdit();

        }
        #endregion ValuateDTHOLIDAYVALUE

        /// <summary>
        /// Initialise les champs IDxxx (IDForItemTemplate et IDForItemTemplateRelation) d'une classe referential
        /// IDForItemTemplate et IDForItemTemplateRelation: 
        ///     Champs utilisé pour l'affectation de l'ID du control dans le cas de templateColumn qui nécessite plusieurs ID 
        ///     car le control pour l'affichage et le control pour la modification des données sont deux controles distincts 
        ///     donc nécessité d'un ID supplémentaire
        /// Rappel: ItemTemplate est un element de TemplateColumn
        /// </summary>
        /// <param name="pReferential">REF la classe referential à initialiser</param>
        /// FI 20200220 [XXXXX] pReferential n'est plus de type Ref
        public static void InitializeID(ReferentialsReferential pReferential)
        {
            for (int index = 0; index < pReferential.Column.Length; index++)
            {
                ReferentialsReferentialColumn rrc = pReferential.Column[index];
                if (rrc.IsAdditionalData)
                    rrc.IDForItemTemplate = "Col" + rrc.ExternalIdentifier;
                else
                    rrc.IDForItemTemplate = rrc.DataField;

                if (rrc.ExistsRelation)
                {
                    //PL 20111020
                    //rrc.IDForItemTemplateRelation = (rrc.Relation[0].AliasTableName == null ? rrc.Relation[0].TableName : rrc.Relation[0].AliasTableName); 
                    //rrc.IDForItemTemplateRelation += "_" + rrc.Relation[0].ColumnSelect[0].ColumnName;

                    //PL 20111021 Refactoring Relation[0].AliasTableName 
                    rrc.IDForItemTemplateRelation = rrc.Relation[0].AliasTableName + "_" + rrc.Relation[0].ColumnSelect[0].ColumnName;
                }

                // RD 20130222 / Utilisation des totaux dans le Referentiel
                pReferential.HasAggregateColumns = pReferential.HasAggregateColumns ||
                    (pReferential.Column[index].GroupBySpecified && pReferential.Column[index].GroupBy.AggregateSpecified);
            }
        }

        /// <summary>
        ///  Retourne un array à partir des paramètres de la requête http
        ///  P1=> array[0], P2=> array[1], P3=> array[1]
        /// </summary>
        /// <param name="pPage"></param>
        /// <returns></returns>
        public static string[] GetQueryStringParam(Page pPage)
        {
            NameValueCollection nvcQueryString = pPage.Request.QueryString;
            string[] ret = null;
            int length = 0;
            //
            if (ArrFunc.IsFilled(nvcQueryString))
            {
                for (int i = 0; i < 32; i++) // 32 params Max
                {
                    if (null == nvcQueryString["P" + (i + 1).ToString()])
                    {
                        length = i;
                        break;
                    }
                }
                //
                if (length > 0)
                    ret = new string[length];
                //
                if (ArrFunc.IsFilled(ret))
                {
                    for (int i = 0; i < ret.Length; i++)
                    {
                        ret[i] = nvcQueryString["P" + (i + 1).ToString()];
                    }
                }
            }
            //
            return ret;
        }

        #region public GetCheckBoxValue
        /// <summary>
        /// Obtient la valeur à renseigner pour une colonne de type bool 
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pRrc">la colonne concernée</param>
        /// <returns>valur de la checkBox</returns>
        public static bool GetCheckBoxValue(ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc)
        {
            bool ret = false;
            //
            if ((pRrc.IsAdditionalData && pReferential.isNewDrExternal[pRrc.ExternalFieldID]))
            {
                if (pRrc.ExistsDefaultValue)//PL 20130122 Add this test
                    ret = BoolFunc.IsTrue(pRrc.Default[0].Value);
            }
            else
            {
                string data;
                //
                if (pRrc.IsAdditionalData)
                    data = (pReferential.drExternal[pRrc.ExternalFieldID])[pRrc.ColumnName].ToString(); // 20090826 RD /Dans la requête de drExternal c'est "ColumnName" qui est utilisée.
                else
                    data = pReferential.dataRow[pRrc.DataField].ToString();
                //
                ret = BoolFunc.IsTrue(data);
            }

            return ret;
        }
        #endregion GetCheckBoxValue

        #region public ConstructColumnOrderBy
        public static string ConstructColumnOrderBy(string pAliasTable, string pColumnName, string pSqlOrder, bool pIsWithColumnAlias)
        {
            // 20091021 RD DYNAMIC_ALIASTABLE déplacé dans la classe Cst pour pouvoir le partager
            string ret = pColumnName;
            //
            if (StrFunc.IsFilled(pSqlOrder))
            {
                if (pSqlOrder.IndexOf(Cst.DYNAMIC_ALIASTABLE) >= 0)
                    //ex.: replicate('' '', 64-len(<aliasTable>.IDENTIFIER)) + <aliasTable>.IDENTIFIER
                    ret = pSqlOrder.Replace(Cst.DYNAMIC_ALIASTABLE, pAliasTable);
                else
                    ret = pAliasTable + "." + pSqlOrder;
            }
            //
            if (pIsWithColumnAlias)
                ret = ret.Replace(pAliasTable + ".", pAliasTable + "_"); // 20090831 RD / Il peut y avoir un problème si la colonne contient un alias     
            //
            return ret;
        }
        #endregion ConstructColumnOrderBy

        #region private CreateControls
        /// <revision>
        ///     <version>1.0.4</version><date>20041202</date><author>OD</author>
        ///     <comment>
        ///     Gestion du Datetime en plus du type Date
        ///     Utilisation de GetStringDefaultForReferencialColumn() pour l'affectation de valeur par defaut
        ///     </comment>
        /// </revision>
        /// <summary>
        /// Créé les controls pour une classe referential et les stocke dans cette classe
        /// </summary>
        /// <param name="pPage">page appelante</param>
        /// <param name="pReferential">REF classe referential pour laquelle on créé les controls</param>
        /// <param name="pMode">mode : grid | form</param>
        /// FI 20200220 [XXXXX] Public Method
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20220607 [XXXXX] Gestion titre sur tooltip quand ressource du contrôle vierge (contrôle lié)
        // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
        public static void CreateControls(PageBase pPage, ReferentialsReferential pReferential, string pMode,
            bool pIsNewRecord, bool pIsDuplicateRecord)
        {
            string warningMsg = string.Empty;
            //
            bool isModeDataGrid = ("grid" == pMode);
            bool isModeForm = ("form" == pMode);

            //Initialise les champs IDxxx (IDForItemTemplate et IDForItemTemplateRelation)
            InitializeID(pReferential);

            //RegularExpression, MsgErr & MsgErrRequiredField
            pReferential.InitializeRegEx();
            //
            pReferential.firstVisibleAndEnabledControlID = string.Empty;
            //
            string firstControlLinkedColumnName = string.Empty;
            //On créé et affecte le(s) control(s) pour chaque colonne du referenciel
            for (int index = 0; index < pReferential.Column.Length; index++)
            {
                ReferentialsReferentialColumn rrc = pReferential.Column[index];

                rrc.IsFirstControlLinked = ReferentialTools.IsFirstControlLinked(index, pReferential);
                rrc.IsMiddleControlLinked = ReferentialTools.IsMiddleControlLinked(index, pReferential);
                rrc.IsLastControlLinked = ReferentialTools.IsLastControlLinked(index, pReferential);

                if (rrc.IsFirstControlLinked)
                {
                    firstControlLinkedColumnName = rrc.ColumnName;
                }

                if (rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked || rrc.IsLastControlLinked)
                {
                    rrc.FirstControlLinkedColumnName = firstControlLinkedColumnName;
                }
                else
                {
                    firstControlLinkedColumnName = string.Empty;
                }

                #region Gestion ToolTip
                string toolTip = string.Empty;
                string toolTipLabel = string.Empty;
                //Si un tooltip est renseigné
                if (rrc.ToolTipSpecified)
                {
                    toolTip = Ressource.GetString(rrc.ToolTip, true);
                    toolTipLabel = toolTip;
                }
                //Si pas de tooltip renseigné, on utilise les ressources
                else
                {
                    //Cas d'un control lié : ressource du control 'père' + ( ressource du 'fils')
                    if (rrc.IsFirstControlLinked || rrc.IsMiddleControlLinked || rrc.IsLastControlLinked)
                    {
                        //PLl 20110228 Add Test on firstControlLinkedColumnName
                        if (StrFunc.IsFilled(firstControlLinkedColumnName))
                        {
                            toolTip = Ressource.GetMulti((pReferential[firstControlLinkedColumnName]).Ressource, 2);
                            toolTipLabel = toolTip;
                            //PL 20160218 Add SPACE
                            toolTip += " (" + Ressource.GetString(rrc.ColumnName, true) + ")";
                        }
                    }
                    //sinon, on affecte simplement la ressource au tooltip
                    else
                    {
                        toolTip = Ressource.GetMulti(rrc.Ressource, 2);
                        toolTipLabel = toolTip;
                    }
                }
                #endregion Gestion ToolTip

                bool isIdentity = (rrc.IsIdentitySpecified && rrc.IsIdentity.Value);
                bool isFK = (rrc.IsForeignKeyField && StrFunc.IsFilled(pReferential.ValueForeignKeyField)); ;
                bool isEnabled = !isFK;
                if (isEnabled && rrc.IsUpdatableSpecified)
                {
                    //Enabled if IsUpdatable
                    if (pReferential.isNewRecord || pIsDuplicateRecord)
                    {
                        if (rrc.IsUpdatable.isupdatableincreationSpecified)
                        {
                            isEnabled = rrc.IsUpdatable.isupdatableincreation;
                        }
                    }
                    else
                    {
                        isEnabled = rrc.IsUpdatable.Value;
                    }
                }

                //bool isTextBoxModeMultiLine = (rrc.TextMode == TextBoxMode.MultiLine.ToString()) || (TypeData.IsTypeString(rrc.DataType) && (rrc.Length >= 1000));

                #region Label
                WCTooltipLabel lblRef = new WCTooltipLabel
                {
                    ID = ControlID.GetID(rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty), null, Cst.LBL),
                    Enabled = isEnabled || isFK || (pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly),
                    Text = rrc.Label
                };
                //PL 20100121 Add center
                if (lblRef.Text == @"/")
                {
                    lblRef.Attributes.Add("style", "text-align:center");
                }

                #region isDisplayColumnName
                // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
                lblRef.Text += Cst.HTMLBreakLine + $"<span class='refcolname hide'>[Column: <span>{rrc.ColumnName}";
                if (rrc.IsDataKeyFieldSpecified && rrc.IsDataKeyField)
                    lblRef.Text += @"<sub>(PK)</sub>";
                else if (rrc.IsKeyFieldSpecified && rrc.IsKeyField)
                    lblRef.Text += @"<sub>(UK)</sub>";
                if (rrc.IsForeignKeyFieldSpecified && rrc.IsForeignKeyField)
                    lblRef.Text += @"<sub>(FK)</sub>";

                //PL 20140416 newness
                if (rrc.IsFirstControlLinked)
                {
                    for (int index_linked = index + 1; index_linked < pReferential.Column.Length; index_linked++)
                    {
                        ReferentialsReferentialColumn rrc_linked = pReferential.Column[index_linked];

                        lblRef.Text += @" - " + rrc_linked.ColumnName;

                        if (ReferentialTools.IsLastControlLinked(index_linked, pReferential))
                            break;
                    }
                }

                lblRef.Text += @"</span>]</span>";
                #endregion

                if (false == toolTipLabel.Equals(rrc.Label))
                    lblRef.Pty.TooltipContent = toolTipLabel;
                lblRef.Width = Unit.Percentage(100);

                #region urlHelp
                //				#warning 20040000 PL Test à remplacer par la nouvelle gestion de l'aide
                //                if (rrc.ExistsRelation 
                //                    && (Cst.IsDDLTypeBusinessCenter(rrc.Relation[0].DDLType) || Cst.IsDDLTypeRoundDir(rrc.Relation[0].DDLType)))
                //                {
                //                    string url;
                //                    string keyword = string.Empty;
                //
                //                    if (Cst.IsDDLTypeBusinessCenter(rrc.Relation[0].DDLType))
                //                        keyword = @"Complex_BusinessCenters";
                //                    else if (Cst.IsDDLTypeRoundDir(rrc.Relation[0].DDLType))
                //                        keyword = @"Complex_Rounding";
                //                                
                //                    url = Cst.GetUrlForHelp("fpml-shared-" + ConfigurationManager.AppSettings["FpMLHelpVersion"]+ ".html#" + keyword);
                //                    url = JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle._help);
                //                    lblRef.Attributes.Add("onclick", url);
                //                    lblRef.Attributes.Add("style", "CURSOR: help;");
                //                }                
                #endregion urlHelp

                rrc.ControlLabel = lblRef;
                #endregion Label

                if (isIdentity)
                {
                    CreateControl_Identity(pPage, pReferential, rrc, isModeForm, isEnabled);
                }
                else if (TypeData.IsTypeBool(rrc.DataType.value))
                {
                    CreateControl_Boolean(pPage, pReferential, rrc, toolTip, isModeForm, isEnabled);
                }
                else if (ReferentialTools.IsDataForDDL(rrc))
                {
                    string tmp_warningMsg = CreateControl_DropDownList(pPage, pReferential, rrc, toolTip, isModeForm, isEnabled, pIsNewRecord);
                    if (StrFunc.IsFilled(tmp_warningMsg))
                    {
                        if (StrFunc.IsFilled(warningMsg))
                            warningMsg += Cst.CrLf2;
                        warningMsg += tmp_warningMsg;
                    }
                }
                else if (rrc.AutoCompleteRelationEnabled)
                {
                    CreateControl_AutoCompleteRelation(pPage, pReferential, rrc, toolTip, isModeForm, isEnabled, pIsNewRecord);
                }
                else
                {
                    CreateControl_TextBox(pPage, pReferential, rrc, toolTip, isModeForm, isModeDataGrid, isEnabled);
                }

                #region Création de Button Zoom XML
                // GLOP FI 20070206 A faire plus tard....
                if (rrc.IsDataXml)
                {
                    string label;
                    if (rrc.ExistsRelation)
                        label = rrc.Relation[0].ColumnSelect[0].Ressource;
                    else
                        label = rrc.Ressource;
                    _ = Ressource.GetMulti(label);
                    //
                    string controlImgID = Cst.IMG + rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty);

                    LinkButton imgbutXml = new LinkButton
                    {
                        CausesValidation = false,
                        ID = controlImgID,
                        CssClass = "fa-icon orange",
                        Text = "<i class='fas fa-external-link-alt'></i> XML"
                    };
                    rrc.OtherControls = new WebControl[1];
                    rrc.OtherControls[0] = imgbutXml;
                }
                #endregion

                #region InformationControls
                if (rrc.InformationSpecified)
                {
                    WCToolTipPanel btnMessage = new WCToolTipPanel();
                    Label lblMessage = null;

                    string tmpRessource = rrc.Ressource;
                    // EG 20220607 [XXXXX] Gestion titre sur tooltip quand ressource du contrôle vierge (contrôle lié)
                    if (((tmpRessource == @"/") || String.IsNullOrEmpty(tmpRessource)) && (index > 0))
                        tmpRessource = pReferential.Column[index - 1].Ressource;

                    //PL 20121009 Debug
                    //rrc.HasInformationControls = rrc.Information.GetWebCtrlInformation(pPage, rrc.ColumnName, tmpRessource, ref imgMessage, ref lblMessage);
                    string ctrlID = rrc.ColumnName + (rrc.IsAdditionalData ? rrc.ExternalFieldID.ToString() : string.Empty);
                    rrc.HasInformationControls = rrc.Information.GetWebCtrlInformation(ctrlID, tmpRessource, ref btnMessage, ref lblMessage);

                    if (rrc.HasInformationControls)
                    {
                        rrc.InformationControls = new WebControl[2];
                        rrc.InformationControls[0] = btnMessage;
                        rrc.InformationControls[1] = lblMessage;
                    }
                }
                #endregion InformationControls

                //On renseigne l'ID du premier control visible
                if (StrFunc.IsEmpty(pReferential.firstVisibleAndEnabledControlID) && isEnabled && rrc.ControlMainSpecified && ((rrc.IsHideSpecified && !rrc.IsHide) || !rrc.IsHideSpecified))
                {
                    pReferential.firstVisibleAndEnabledControlID = rrc.ControlMain.ID;
                }
            }
            if (StrFunc.IsFilled(warningMsg))
            {
                JavaScript.DialogStartUpImmediate(pPage, warningMsg, false, ProcessStateTools.StatusWarningEnum);
            }
        }
        #endregion CreateControls

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pToolTip"></param>
        /// <param name="pIsModeForm"></param>
        /// <param name="pIsEnabled"></param>
        private static void CreateControl_Boolean(PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc, string pToolTip,
            bool pIsModeForm, bool pIsEnabled)
        {

            #region CheckBox/RadioButton
            bool isRadioButton = pRrc.DataType.value.StartsWith("bool2");
            WebControl chkRef;
            if (isRadioButton)
                chkRef = new RadioButton();
            else
                chkRef = new CheckBox();
            chkRef.ID = ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), pRrc.DataType.value);
            chkRef.CssClass = "chkCapture";
            chkRef.EnableViewState = true;
            chkRef.ToolTip = pToolTip;
            if (false == pIsModeForm)
                chkRef.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");

            // EG 20121029 PartialReadOnlyMode test
            bool isColumnReadOnly = pReferential.isLookReadOnly ||
                                ((pRrc.IsUpdatableSpecified && (false == pRrc.IsUpdatable.Value) &&
                                (pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly)));
            //if (pReferential.isLookReadOnly)
            if (isColumnReadOnly)
                chkRef.Enabled = false;
            else
                chkRef.Enabled = pIsEnabled;
            //
            SetJavascriptToColumnControl(chkRef, pReferential, pRrc);
            //
            if (isRadioButton)
            {
                RadioButton rb = (RadioButton)chkRef;
                rb.AutoPostBack = pRrc.IsAutoPostBack;
                rb.Text = Ressource.GetMulti(pRrc.Ressource);

                // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
                rb.Text += Cst.HTMLBreakLine + $"<span class='refcolname hide'>[Column: <span>{pRrc.ColumnName}</span>]</span>";
                if (pIsModeForm && (false == pPage.IsPostBack))
                {
                    rb.Checked = ReferentialTools.GetCheckBoxValue(pReferential, pRrc);
                }
            }
            else
            {
                CheckBox cb = (CheckBox)chkRef;
                cb.AutoPostBack = pRrc.IsAutoPostBack;
                cb.Text = Ressource.GetMulti(pRrc.Ressource);
                //PL 20140416 Use pIsDisplayColumnName insted of pPage.isTrace
                //if (pPage.isTrace)
                // EG 20230921 [WI711] Referential Page: Display columns name enhancement via jQuery (without PostBack)
                cb.Text += Cst.HTMLBreakLine + $"<span class='refcolname hide'>[Column: <span>{pRrc.ColumnName}</span>]</span>";
                if (pIsModeForm && (false == pPage.IsPostBack))
                {
                    cb.Checked = ReferentialTools.GetCheckBoxValue(pReferential, pRrc);
                }
            }
            //
            pRrc.ControlMain = chkRef;
            #endregion CheckBox/RadioButton
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pToolTip"></param>
        /// <param name="pIsModeForm"></param>
        /// <param name="pIsEnabled"></param>
        /// <param name="pIsNewRecord"></param>
        /// <returns></returns>
        /// FI 20140911 [XXXXX] Modify
        /// FI 20161130 [RATP] Modify
        // EG 20210222 [XXXXX] Suppression inscription PostBack et OnCtrlChanged (présent dans PageBase.js)
        // EG 20210222 [XXXXX] Suppression inscription SetColorStartup et appel de SetColor (présent dans PageBase.js)
        private static string CreateControl_DropDownList(PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc,
            string pToolTip, bool pIsModeForm, bool pIsEnabled, bool pIsNewRecord)
        {
            //PL 20130109 Test in progress... (Use an OptionGroupDropDownList)
            WCDropDownList2 ddlRef;
            if (pRrc.ColumnName == "ACTION" || pRrc.ColumnName == "STRATEGYTYPE" ||
                (pReferential.TableName == "PARTYTEMPLATE" && pRrc.ColumnName == "IDA_TRADER") ||
                (pReferential.TableName == "PARTYTEMPLATEDET" && pRrc.ColumnName == "IDA")
                )
            {
                OptionGroupDropDownList optionGroupDDLRef = new OptionGroupDropDownList();
                ddlRef = (WCDropDownList2)optionGroupDDLRef;
            }
            else
            {
                ddlRef = new WCDropDownList2();
            }

            // FI 20161130 [RATP]  (Modification nécessaire pour le référentiel ACTORRATING)
            // La colonne AGENCY est AutoPostBack parce que les notations (colonnes SHORTTERMRATING, LONGTERMRATING) sont fonction de l'agence
            // La colonne AGENCY est IsUpdatable = false (Modification impossible en Mode Modification)
            // => Dans ce cas le control Web doit être Enabled pour que  lors du postBack (après click Sur Enregistrement) pPage.Request.Form["DLLAGENCY"].ToString() retourne la donnée présente
            //  GetDDLSQLSelect est la méthode à pPage.Request.Form["DLLAGENCY"].ToString() est effectué
            if ((false == pIsEnabled) && (pRrc.IsAutoPostBack))
            {
                pIsEnabled = true;
                ddlRef.HasViewer = true;
            }

            string controlID = ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), null, Cst.DDL);
            string argValueForeignKeyField = pReferential.ValueForeignKeyField;
            string argCurrentNonUpdatableValue = string.Empty;
            string currentValue = string.Empty;
            bool isIgnore_PL20130517 = false;

            //le fonctionnement des DDL veux que si le champs est non editable, la DDL ne contient que sa valeur.
            //dans la majorité des cas, le champ est disabled car il s'agit d'une FK; on utilise donc la FK pour fixer la valeur du DDL disabled.
            //cela ne fonctionne plus dans certains cas particuliers : 
            //          -> par ex BOOK : FK = IDA et la PK (IDB qui est identity invisible ) est basée sur IDA_ENTITY et IDA_MANAGEMENT ; 
            //              il en resulte qu'à l'affichage en modif, IDA est disabled (c'est la FK donc pas de problème pour le LoadDDL)
            //              par contre pour IDA_ENTITY et IDA_MANAGEMENT qui sont disabled (puisque PK 'virtuelles') et sont des DDLs, on a besoin de leur valeur pour LoadDDL.
            //on renseigne donc argCurrentNonUpdatableValue qui primera sur argValueForeignKeyField pour la conception de la DLL
            //
            //on retrouve par exemple le meme problème avec IDA_ACTOR qui est non updatable dans ACTORROLE
            if (pIsModeForm)
            {
                if (pRrc.IsAdditionalData)
                {
                    // 20090826 RD /Dans la requête de drExternal c'est "ColumnName" qui est utilisée.
                    currentValue = (pReferential.drExternal[pRrc.ExternalFieldID])[pRrc.ColumnName].ToString();
                }
                else
                    currentValue = pReferential.dataRow[pRrc.DataField].ToString();

                if (false == pIsEnabled)
                    argCurrentNonUpdatableValue = currentValue;

                #region LoadDDL
                //PL/EG 20130517 IDAINS/IDAUPD
                isIgnore_PL20130517 = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Ignore_PL20130517", "false"));
                if (((pRrc.ColumnName != "IDAINS") && (pRrc.ColumnName != "IDAUPD")) || (isIgnore_PL20130517) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                {
                    ddlRef.ID = controlID;  //PL 20140728 WARNING: Alimentation ici de l'ID nécessaire à WriteSelectOptGroupScripts() via LoadDDL().
                    //                     En espérant que cela ne va pas introduire des pbs ! (à surveiller)
                    ReferentialTools.LoadDDL(pPage, pReferential, pRrc, ddlRef, argValueForeignKeyField, argCurrentNonUpdatableValue, pIsNewRecord);
                }
                #endregion LoadDDL
            }
            bool isFoundValue = (false == pIsModeForm) || (pPage.IsPostBack);
            string selectedValue = currentValue;
            string warningMsg = string.Empty;
            string textSelectedValue = currentValue;
            // EG 20121029 PartialReadOnlyMode test
            bool isColumnReadOnly = pReferential.isLookReadOnly ||
                                ((pRrc.IsUpdatableSpecified && (false == pRrc.IsUpdatable.Value) &&
                                (pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly)));

            //
            bool isFK = (pRrc.IsForeignKeyField && StrFunc.IsFilled(pReferential.ValueForeignKeyField));
            // EG 20121029 PartialReadOnlyMode test
            //if (pReferential.isLookReadOnly || isFK)
            if (isColumnReadOnly || isFK)
            {
                #region LookReadOnly --> Display a Label
                //PL/EG 20130517 IDAINS/IDAUPD
                if (((pRrc.ColumnName != "IDAINS") && (pRrc.ColumnName != "IDAUPD")) || (isIgnore_PL20130517) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                    isFoundValue = FoundValue(false, ddlRef, null, selectedValue, pRrc, ref textSelectedValue, ref warningMsg);

                WCTooltipLabel lblRef2 = new WCTooltipLabel
                {
                    ID = controlID,
                    Width = Unit.Percentage(100),
                    Enabled = true,
                    CssClass = (pRrc.IsForeignKeyFieldSpecified && pRrc.IsForeignKeyField ? EFSCssClass.LabelDisplayForeignKey : EFSCssClass.LabelDisplay)
                };
                if (false == pIsModeForm)
                    lblRef2.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
                //PL/EG 20130517 IDAINS/IDAUPD
                if (((pRrc.ColumnName != "IDAINS") && (pRrc.ColumnName != "IDAUPD")) || (isIgnore_PL20130517) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                {
                    lblRef2.Text = (isFoundValue ? ddlRef.SelectedItem.Text : selectedValue);
                    lblRef2.Attributes.Add("DDLValue", isFoundValue ? ddlRef.SelectedItem.Value : selectedValue);
                }
                lblRef2.Pty.TooltipContent = pToolTip;

                //FI 20140911 [XXXXX] Add HyperLink on control
                if (StrFunc.IsFilled(selectedValue))
                {
                    // FI 20170728 [23341] Affichage en mode normal
                    SetControlHyperLinkColumn(pPage, lblRef2, pReferential, pRrc, selectedValue, Cst.ConsultationMode.Normal);
                }

                TableRow tr = new TableRow();
                TableCell td;

                td = new TableCell
                {
                    Wrap = false
                };
                td.Controls.Add(lblRef2);
                tr.Cells.Add(td);

                td = new TableCell
                {
                    Visible = false,
                    Text = selectedValue
                };
                tr.Cells.Add(td);

                Table table = new Table
                {
                    CellPadding = 0,
                    CellSpacing = 0,
                    BorderWidth = 0
                };
                table.Rows.Add(tr);
                pRrc.ControlMain = table;
                #endregion LookReadOnly
            }
            else
            {
                #region Init Property
                ddlRef.ID = controlID;
                ddlRef.Enabled = pIsEnabled;
                //PL 20141017 Add if()
                if (ddlRef.CssClass != EFSCssClass.DropDownListJQOptionGroup)
                    ddlRef.CssClass = EFSCssClass.DropDownListCapture;
                ddlRef.EnableViewState = true;
                ddlRef.AutoPostBack = pRrc.IsAutoPostBack;
                if (pRrc.IsAutoPostBack)
                {
                    ddlRef.AutoPostBack = false;
                    //JavaScript.PostBack(pPage);
                    //JavaScript.OnCtrlChanged(pPage);
                    // FI 20200305 [XXXXX] suppression du "return false;" puisque le control n'est pas autoPostBack
                    // ddlRef.Attributes.Add("onchange", "OnCtrlChanged('" + ddlRef.UniqueID + "','');return false;");
                    ddlRef.Attributes.Add("onchange", "OnCtrlChanged('" + ddlRef.UniqueID + "','');");
                }
                if (false == pIsModeForm)
                    ddlRef.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
                if (pReferential.consultationMode != Cst.ConsultationMode.Normal)
                    ddlRef.Enabled = false;
                #endregion Init Property
                #region Init Value
                if (pIsModeForm && (false == pPage.IsPostBack))
                {
                    if (pReferential.isNewRecord || (pRrc.IsAdditionalData && pReferential.isNewDrExternal[pRrc.ExternalFieldID]))
                    {
                        #region Default Value
                        if (pRrc.IsForeignKeyField)
                            selectedValue = pReferential.ValueForeignKeyField;
                        else
                        {
                            string defaultValue = string.Empty;
                            if (pRrc.ExistsDefaultValue)
                                defaultValue = SessionTools.ReplaceDynamicConstantsWithValues(pRrc.GetStringDefaultValue(pReferential.TableName));
                            else if (pRrc.ExistsDefaultColumnName)
                            {
                                try
                                {
                                    //Default Value renseignée dans la méthode InitializeNewRow()
                                    defaultValue = pReferential.dataRow[pRrc.DataField].ToString();
                                }
                                catch { }
                            }
                            if (StrFunc.IsEmpty(defaultValue) && pRrc.IsMandatory && pRrc.ExistsDDLType)
                            {
                                //PL 20101020 Debug --> Add test != null
                                if (pRrc.Relation[0].DDLType != null)
                                    defaultValue = ConfigurationManager.AppSettings["Spheres_ReferentialDefault_" + pRrc.Relation[0].DDLType.Value];
                            }
                            selectedValue = defaultValue;
                        }
                        #endregion Default Value
                    }
                    //PL/EG 20130517 IDAINS/IDAUPD
                    if (((pRrc.ColumnName != "IDAINS") && (pRrc.ColumnName != "IDAUPD")) || (isIgnore_PL20130517) || StrFunc.IsFilled(argCurrentNonUpdatableValue))
                    {
                        _ = FoundValue(false, ddlRef, null, selectedValue, pRrc, ref textSelectedValue, ref warningMsg);
                    }
                }
                #endregion Init Value
                #region Init JS
                ReferentialTools.SetJavascriptToColumnControl(ddlRef, pReferential, pRrc);
                #endregion Init JS
                #region Set rrc.ControlMain
                //si on est dans le cas d'un DDL de type style, on ajoute un champ de type Preview pour le rendu du style
                if (pRrc.ExistsRelationDDLType && (pRrc.Relation[0].DDLType != null) && Cst.IsDDLTypeStyleComponentColor(pRrc.Relation[0].DDLType.Value))
                {
                    #region Selection de couleur (with preview)
                    if (pIsModeForm)
                    {
                        string typeColor = pRrc.Relation[0].DDLType.Value;
                        string ctrlID = "preview" + pRrc.FirstControlLinkedColumnName;
                        ddlRef.Attributes.Add("onchange", "SetColor(this.value ,'" + ctrlID + "','" + typeColor + "');");
                        if (false == pPage.IsPostBack)
                            JavaScript.CallFunction(pPage, String.Format("SetColor('{0}','{1}','{2}')",
                                pReferential.dataRow[pRrc.DataField].ToString(), ctrlID, typeColor));
                    }
                    #endregion Selection de couleur (with preview)
                }
                pRrc.ControlMain = ddlRef;
                #endregion Set rrc.ControlMain
            }
            return warningMsg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pIsModeForm"></param>
        /// <param name="pIsEnabled"></param>
        private static void CreateControl_Identity(PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc, bool pIsModeForm, bool pIsEnabled)
        {
            //Pour un identity on affiche en consultation sa valeur dans un Label (si rrc.Hide = false)
            Label lblIdentity = new Label
            {
                ID = ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), null, Cst.TXT),
                Enabled = pIsEnabled
            };
            if (pIsModeForm && (false == pPage.IsPostBack))
            {
                if (pRrc.ExistsRelation)
                    lblIdentity.Text = pReferential.dataRow[pRrc.IndexColSQL + 1].ToString();
                else
                    lblIdentity.Text = pRrc.GetStringValue(pReferential.dataRow[pRrc.DataField], pReferential.dataRow);
            }
            else
                lblIdentity.Text = string.Empty;
            lblIdentity.CssClass = EFSCssClass.LabelDisplayForeignKey;
            lblIdentity.ToolTip = string.Empty;
            pRrc.ControlMain = lblIdentity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pToolTip"></param>
        /// <param name="pIsModeForm"></param>
        /// <param name="pIsModeDataGrid"></param>
        /// <param name="pIsEnabled"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210217 [XXXXX] Gestion .Enabled en fonction de pIsEnabled et isColumnReadOnly
        // EG 20210304 [XXXXX] Relooking Referentiels
        // EG 20210304 [XXXXX] Relooking Referentiels - Width = 99% si pas de InputWidth
        private static void CreateControl_TextBox(PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc,
            string pToolTip, bool pIsModeForm, bool pIsModeDataGrid, bool pIsEnabled)
        {
            // EG 20121029 PartialReadOnlyMode test
            bool isColumnReadOnly = pReferential.isLookReadOnly ||
                                ((pRrc.IsUpdatableSpecified && (false == pRrc.IsUpdatable.Value) &&
                                (pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly)));


            bool isTextBoxModeMultiLine = (pRrc.TextMode == TextBoxMode.MultiLine.ToString()) ||
                (TypeData.IsTypeString(pRrc.DataType.value) && (pRrc.Length >= 1000));

            string controlID = ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), null, Cst.TXT);
            WCTextBox txtRef;
            #region new WCTextBox()
            //Textbox with required and regularExpression 
            if ((false == pReferential.isNewRecord) &&
                ((false == pRrc.IsUpdatableSpecified) || (pRrc.IsUpdatableSpecified && (false == pRrc.IsUpdatable.Value))))
                txtRef = new WCTextBox(controlID, string.Empty, string.Empty, true, string.Empty);
            else if ((pRrc.IsMandatory) && ((pRrc.RegularExpression.Length > 0) || (pRrc.Length > 0)))
                txtRef = new WCTextBox(controlID, pRrc.MsgErrRequiredField, pRrc.RegularExpression, true, pRrc.MsgErr);
            //Textbox with required 
            else if (pRrc.IsMandatory)
                txtRef = new WCTextBox(controlID, pRrc.MsgErrRequiredField, string.Empty, true, pRrc.MsgErr);
            //Textbox with regularExpression 
            else if ((pRrc.RegularExpression.Length > 0) || (pRrc.Length > 0))
                txtRef = new WCTextBox(controlID, string.Empty, pRrc.RegularExpression, true, pRrc.MsgErr);
            //Textbox without Validator
            else
                txtRef = new WCTextBox();
            #endregion new WCTextBox()

            #region Data
            string data = string.Empty;
            if (pIsModeForm)
            {
                bool isReadData;
                if (false == pPage.IsPostBack)
                {
                    isReadData = true;
                }
                else
                {
                    isReadData = (pReferential.isLookReadOnly);
                    //Lors d'un postback, lorsque qu'un contrôle est readOnly alors ASP affecte .text avec la valeur existante dans le viewstate (ASP écrase notamment la valeur lu sur le client) 
                    //Sauf que le viewState ne contient rien puisque les propriétés du contrôles sont affectés avant même que le contrôle soit ajouté à la page
                    //=> donc il faut relire la donnée systématiquement
                    if (false == isReadData)
                    {
                        isReadData = (false == pIsEnabled);
                        //Lors d'un postback, lorsque qu'un contrôle est disabled alors ASP affecte .text avec  la valeur existante dans le viewstate, 
                        // ASP ne lit même pas la valeur existante sur le client 
                        //Sauf que le viewState ne contient rien puisque les propriétés du contrôles sont affectés avant même que le contrôle soit ajouté à la page
                        //=> donc il faut relire la donnée systématiquement
                    }

                    if (false == isReadData)
                    {
                        // RD 20130322 [18515] 
                        // Lors d'un postback, lorsque qu'un control est "caché en mode Light" alors le .text n'est pas valorisé:
                        // - Soit comme ci-dessus, le viewstate ne contient pas la valeur du contol !!!
                        // - Soit pour une autre raison que je n'arrive pas à expliquer
                        // => donc il faut relire la donnée systématiquement
                        isReadData = (pRrc.IsHideOnLightDisplaySpecified && pRrc.IsHideOnLightDisplay);
                    }
                }
                if (isReadData)
                {
                    if (pReferential.isNewRecord || (pRrc.IsAdditionalData && pReferential.isNewDrExternal[pRrc.ExternalFieldID]))
                    {
                        //Default Value
                        data = pRrc.GetStringDefaultValue(pReferential.TableName);
                    }
                    else
                    {
                        Object objData;
                        if (pRrc.IsAdditionalData)
                        {
                            objData = (pReferential.drExternal[pRrc.ExternalFieldID])[pRrc.ColumnName].ToString(); // 20090826 RD /Dans la requête de drExternal c'est "ColumnName" qui est utilisée.
                            data = pRrc.GetStringValue(objData, pReferential.drExternal[pRrc.ExternalFieldID]);
                        }
                        else
                        {
                            objData = pReferential.dataRow[pRrc.DataField];
                            data = pRrc.GetStringValue(objData, pReferential.dataRow);
                        }
                        //
                        // RD 20100309 / Automatic Compute identifier
                        if (StrFunc.IsEmpty(data))
                        {
                            string defaultValue = pRrc.GetStringDefaultValue(pReferential.TableName);
                            if (Cst.AUTOMATIC_COMPUTE.ToString() == defaultValue)
                                data = defaultValue;
                        }
                    }
                }
            }
            #endregion Data
            txtRef.EnableViewState = true;
            txtRef.AutoPostBack = pRrc.IsAutoPostBack;
            txtRef.ReadOnly = isColumnReadOnly;
            txtRef.ToolTip = pToolTip;

            // EG 20210217 [XXXXX] Utilisation pIsEnabled & isColumnReadOnly
            if ((pReferential.consultationMode == Cst.ConsultationMode.PartialReadOnly) && (!pRrc.IsDataXml))
            {
                //NB: si donnée XML et non saisissable, on laisse à "false" afin de ne pas engendrer d'erreur ValidateRequest 
                txtRef.Enabled = pIsEnabled;
            }
            else
            {
                txtRef.Enabled = (!isColumnReadOnly && pIsEnabled);
            }

            //PL 20210121 Disabled ValidateRequestMode on XML Data and IOINPUT/IOOUTPUT (DATANAME)
            // EG 20210217 [XXXXX] Test sur txtRef.Enabled
            // RD 20230130 [26234] Add test of SIARGUMENTS
            if ((pRrc.IsDataXml || (pRrc.ColumnName.IndexOf("DATANAME") >= 0) || (pRrc.ColumnName.IndexOf("SIARGUMENTS") >= 0)) && txtRef.Enabled)
                txtRef.ValidateRequestMode = ValidateRequestMode.Disabled;

            // EG 20121029 PartialReadOnlyMode test
            //txtRef.CssClass = EFSCssClass.GetCssClass(TypeData.IsTypeNumeric(pRrc.DataType), pRrc.IsMandatory, isTextBoxModeMultiLine, pReferential.isLookReadOnly);
            txtRef.CssClass = EFSCssClass.GetCssClass(TypeData.IsTypeNumeric(pRrc.DataType.value), pRrc.IsMandatory, isTextBoxModeMultiLine, isColumnReadOnly);
            // FI 20210208 [XXXXX] Add
            if  (pRrc.AutoCompleteEnabled)
                txtRef.CssClass = $"rc-autocomplete {txtRef.CssClass}"; //rc => ReferentielColumn

            if (pRrc.AlignSpecified)
                txtRef.Attributes.Add("style", "text-align:" + pRrc.Align);
            if (false == pIsModeForm)
                txtRef.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
            SetJavascriptToColumnControl(txtRef, pReferential, pRrc);
            #region Width
            // EG 20121029 PartialReadOnlyMode test
            //if (TypeData.IsTypeDate(pRrc.DataType) && (false == pReferential.isLookReadOnly))
            if (TypeData.IsTypeDate(pRrc.DataType.value) && (false == pReferential.isLookReadOnly) && (false == isColumnReadOnly))
                txtRef.CssClass = "DtPicker " + txtRef.CssClass;
            // EG 20121029 PartialReadOnlyMode test
            //else if (TypeData.IsTypeDateTime(pRrc.DataType) && (false == pReferential.isLookReadOnly))
            else if (TypeData.IsTypeDateTime(pRrc.DataType.value) && (false == pReferential.isLookReadOnly) && (false == isColumnReadOnly))
                txtRef.CssClass = "DtTimePicker " + txtRef.CssClass;
            // EG 20121029 PartialReadOnlyMode test
            //else if (TypeData.IsTypeTime(pRrc.DataType) && (false == pReferential.isLookReadOnly) && (false == isColumnReadOnly))
            else if (TypeData.IsTypeTime(pRrc.DataType.value) && (false == pReferential.isLookReadOnly))
                txtRef.CssClass = "TimePicker " + txtRef.CssClass;
            else if (false == StrFunc.IsFilled(pRrc.InputWidth))
                txtRef.Width = Unit.Percentage(99);
            if (pIsModeDataGrid)
            {
                if (TypeData.IsTypeDateOrDateTime(pRrc.DataType.value))
                    //On reduit la taille pour le grid (pour pouvoir mettre WCImgCalendar dans la cellule)
                    txtRef.Width = Unit.Percentage(80);
            }
            else if (isTextBoxModeMultiLine)
                //EG 20100428 On reduit la taille pour qu'il ne déborde pas de la table
                txtRef.Width = Unit.Percentage(98);
            #endregion Width
            #region Color
            try
            {
                if (pRrc.CellStyleSpecified && pRrc.CellStyle.modelSpecified)
                {
                    // RD 20110704 [17501]
                    // Utilisation d'une nouvelle méthode ReferentialTools.GetCssClassForModel()
                    EFSCssClass.CssClassEnum cssClass = ReferentialTools.GetCssClassForModel(pRrc.CellStyle.model, data);
                    if (false == EFSCssClass.IsUnknown(cssClass))
                        txtRef.CssClass += " " + cssClass.ToString();
                }
                else if (pRrc.colorSpecified)
                {
                    txtRef.ForeColor = Color.FromName(pRrc.color);
                }
            }
            catch { }
            #endregion
            #region Ressource
            // RD 20100309 / Automatic Compute identifier
            if ((pRrc.IsResourceSpecified && pRrc.IsResource.IsResource) || (Cst.AUTOMATIC_COMPUTE.ToString() == data))
            {
                //PL L20100927 Add pForced=true
                //data = Ressource.GetString(data);
                data = Ressource.GetString(data, true);
            }
            #endregion

            txtRef.Text = data;

            #region TextBoxMode.Password
            if (pRrc.TextMode == TextBoxMode.Password.ToString())
            {
                txtRef.TextMode = TextBoxMode.Password;
                data = Cst.StringForTextBoxModePassword;
                txtRef.Attributes["value"] = data;
            }
            #endregion TextBoxMode.Password
            #region TextBoxMode.MultiLine
            if (isTextBoxModeMultiLine)
            {
                txtRef.TextMode = TextBoxMode.MultiLine;
                int txtHeight = 50;
                try
                {
                    txtHeight = Convert.ToInt32(pRrc.InputHeight);
                }
                catch
                { }
                txtRef.Height = Unit.Point(txtHeight);
            }
            #endregion

            #region Html data
            bool isTRIM = (pRrc.IsTRIMSpecified && pRrc.IsTRIM);
            // EG 20121029 PartialReadOnlyMode test
            //bool isDataHtml = (isTRIM || (isTextBoxModeMultiLine && pReferential.isLookReadOnly)) && (false == pRrc.IsDataXml);
            bool isDataHtml = (isTRIM || (isTextBoxModeMultiLine && isColumnReadOnly)) && (false == pRrc.IsDataXml);
            if (isDataHtml)
            {
                #region Specific to TRIM or Html Data
                //------------------------------------------------------------------------------------------------------------------
                //Affichage de ces données dans une HtmlTable afin d'y restituer les éventuels mis een forme Html (ex.: <b>Bold</b>)
                //------------------------------------------------------------------------------------------------------------------
                TableCell tc;
                TableRow tr;
                Table t = new Table
                {
                    ID = controlID,
                    BorderStyle = BorderStyle.None,
                    CellPadding = 0,
                    CellSpacing = 0,
                    Width = Unit.Percentage(100)
                };
                t.Style.Add(HtmlTextWriterStyle.WhiteSpace, "normal");

                if (StrFunc.IsFilled(data))
                {
                    if (isTRIM)
                    {
                        #region Specif for TRIM
                        data = GetDataTRIM(data, out string opCulture);
                        //
                        System.Web.UI.WebControls.Image imgLang;
                        //
                        #region Culture trouvée != Culture courante --> Bandeau EN
                        if ((opCulture != "**") && (opCulture != SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper()))
                        {
                            tr = new TableRow
                            {
                                BackColor = Color.Lavender,
                                ForeColor = Color.DarkBlue
                            };

                            #region Lang
                            string lang;
                            string imgName;
                            switch (opCulture)
                            {
                                case "EN":
                                    lang = "English";
                                    imgName = "en-GB";
                                    break;
                                case "FR":
                                    lang = "Français";
                                    imgName = "fr-FR";
                                    break;
                                case "IT":
                                    lang = "Italiano";
                                    imgName = "it-IT";
                                    break;
                                case "ES":
                                    lang = "Español";
                                    imgName = "es-ES";
                                    break;
                                default:
                                    lang = "*****";
                                    imgName = "xx-XX";
                                    break;
                            }
                            #endregion
                            lang = Cst.HTMLSpace + HtmlTools.HTMLBold(lang) + Cst.HTMLSpace;
                            imgLang = new System.Web.UI.WebControls.Image();
                            imgLang.Style.Add(HtmlTextWriterStyle.VerticalAlign, "bottom");
                            imgLang.ImageUrl = pPage.Request.ApplicationPath + @"/Images/PNG/BannerFlag-" + imgName + @".png";
                            tc = new TableCell();
                            tc.Controls.Add(imgLang);
                            tr.Controls.Add(tc);
                            tc = new TableCell
                            {
                                Width = Unit.Percentage(100)
                            };
                            tc.Controls.Add(new LiteralControl(lang));
                            tr.Controls.Add(tc);
                            t.Controls.Add(tr);
                        }
                        #endregion
                        tr = new TableRow();
                        tc = new TableCell
                        {
                            ColumnSpan = 2
                        };
                        data = data.Replace("<", "&lt;");
                        data = data.Replace(">", "&gt;");
                        data = data.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                        data = data.Replace(Cst.Tab, Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace + Cst.HTMLSpace);

                        tc.Controls.Add(new LiteralControl(data));
                        tr.Controls.Add(tc);
                        t.Controls.Add(tr);
                        #endregion
                    }
                    else
                    {
                        tr = new TableRow();
                        tc = new TableCell
                        {
                            ColumnSpan = 2
                        };

                        data = FormatLongData(data);

                        tc.Controls.Add(new LiteralControl(data));
                        tr.Controls.Add(tc);
                        t.Controls.Add(tr);
                    }
                }
                pRrc.ControlMain = t;
                #endregion
            }
            else
            {
                pRrc.ControlMain = txtRef;
            }
            #endregion Html data
        }


        #region CreateControl_AutoComplete
        /// <summary>
        /// Create the server side controls used by the autocomplete engine
        /// </summary>
        /// <param name="pPage">Aspx page which hosts the autocomplete server side control</param>
        /// <param name="pReferential">the XML document containing the whole deserialized referential data set</param>
        /// <param name="pRrc">the XML document column containing the deserialized column information, including the autocomplete description</param>
        /// <param name="pToolTip"></param>
        /// <param name="pIsModeForm">true if an editing form is going to be created</param>
        /// <param name="pIsEnabled">false if the control is readonly</param>
        /// <param name="pIsNewRecord">true if we are inserting a new record in the database</param>
        /// FI 20140912 [XXXXX] Modify
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private static void CreateControl_AutoCompleteRelation(
            PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc,
            string pToolTip, bool pIsModeForm, bool pIsEnabled, bool pIsNewRecord)
        {

            #region Create Controls
            // Create the autocomplete server-controls set
            // control hosting the actual selected value
            HiddenField hidRef = new HiddenField
            {
                ID = ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), null, Cst.HID)
            };

            // control hosting the actual selected value description (and the autocomplete functionality)
            WCTextBox txtRef = new WCTextBox(
                ControlID.GetID(pRrc.ColumnName + (pRrc.IsAdditionalData ? pRrc.ExternalFieldID.ToString() : string.Empty), null, Cst.TXT), 
                (pRrc.IsMandatory) ? pRrc.MsgErrRequiredField : string.Empty, string.Empty, true, (pRrc.IsMandatory) ? pRrc.MsgErr : string.Empty);

            //PL 20130516 Test
            //txtRef.AutoPostBack = pRrc.IsAutoPostBack;
            if (pRrc.IsAutoPostBack)
            {
                // FI 20230929 [WI719] Remplacement d'un simple __doPostBack par un PostBack avec Options
                //string attrib_key = "onBlur";
                //string attrib_value = "__doPostBack(\'" + txtRef.ClientID + "\',\'\');";
                //txtRef.Attributes.Add(attrib_key, attrib_value);

                PostBackOptions myPostBackOptions = new PostBackOptions(txtRef)
                {
                    AutoPostBack = true,
                    TrackFocus = true
                };
                txtRef.Attributes.Add("onBlur", "javascript:" + pPage.ClientScript.GetPostBackEventReference(myPostBackOptions));
            }
            #endregion Create Controls

            #region Data
            string dataValue = String.Empty;
            if (pIsModeForm)
            {
                // Get the actual value (dataValue), if it exists
                bool isActorRole = (pReferential.RoleTableNameSpecified && pReferential.RoleTableName.Value == "ACTORROLE");
                if (isActorRole)
                    dataValue = pReferential.dataRow["IDA"].ToString();
                else
                    dataValue = pReferential.dataRow[pRrc.DataField].ToString();

                if (pRrc.IsAdditionalData)
                    // 20090826 RD /Dans la requête de drExternal c'est "ColumnName" qui est utilisée.
                    dataValue = (pReferential.drExternal[pRrc.ExternalFieldID])[pRrc.ColumnName].ToString();

                if (StrFunc.IsFilled(dataValue))
                {
                    // Build a temporary DDL to get the whole dataset, and to find the text label relative to the dataValue
                    using (WCDropDownList2 ddlRef = new WCDropDownList2())
                    {
                        ReferentialTools.LoadDDL(pPage, pReferential, pRrc, ddlRef, pReferential.ValueForeignKeyField, dataValue, pIsNewRecord);

                        string warningMsg = string.Empty;
                        string textValue = string.Empty;
                        FoundValue(false, ddlRef, null, dataValue, pRrc, ref textValue, ref warningMsg);

                        // Set the text value, using the selected item label from the temporary DDL
                        if (ddlRef.SelectedItem != null)
                        {
                            txtRef.Text = ddlRef.SelectedItem.Text;
                            if (StrFunc.IsFilled(warningMsg))
                                txtRef.ToolTip = warningMsg;

                            if (StrFunc.IsEmpty(dataValue))
                                dataValue = ddlRef.SelectedValue;
                        }
                    }
                }

                #region Mise en cache de la requête qui permet d'accéder aux valeurs possibles pour l'autocompte
                // FI 20210202 [XXXXX] Alimentation du cache
                // FI 20210216 [XXXXX] Add whereWithoutReplace
                string sql = GetDDLSQLSelect(
                                pPage,
                                pReferential,
                                pRrc,
                                "",
                                false,
                                pRrc.Relation[0].iscolor,
                                pIsNewRecord,
                                pReferential.ValueForeignKeyField,
                                out _, out string whereWithoutReplace);

                // Mise en cache de la requête pour exécution dans l'autocomplete
                DataCache.SetData($"{pPage.GUID}_{txtRef.ClientID}_Query", sql);
                // FI 20210216 [XXXXX] Add
                DataCache.SetData($"{pPage.GUID}_{txtRef.ClientID}_WhereWithoutReplace", whereWithoutReplace);
                #endregion
            } 
            hidRef.Value = dataValue;
            #endregion Data

            #region Design Controls
            txtRef.Width = Unit.Percentage(99);
            txtRef.EnableViewState = true;
            txtRef.ReadOnly = pReferential.isLookReadOnly;
            txtRef.ToolTip = pToolTip;
            // EG 20170217
            txtRef.Enabled = pReferential.isLookReadOnly || pIsEnabled;
            txtRef.CssClass = EFSCssClass.GetCssClass(
                TypeData.IsTypeNumeric(pRrc.DataType.value), pRrc.IsMandatory, false, pReferential.isLookReadOnly);
            
            // FI 20210202 [XXXXX] Add
            if (pRrc.AutoCompleteRelationEnabled)
                txtRef.CssClass = $"rcr-autocomplete {txtRef.CssClass}"; //rcr => ReferentielColumnRelation

            if (!pIsModeForm)
                txtRef.Style.Add(HtmlTextWriterStyle.FontWeight, "normal");
            
            if (pRrc.AlignSpecified)
                txtRef.Style.Add(HtmlTextWriterStyle.TextAlign, pRrc.Align);
            else
                // 20100614 MF - default align for autocomplete control: left (suggested by PL)
                txtRef.Style.Add(HtmlTextWriterStyle.TextAlign, "left");
            if (pRrc.colorSpecified)
            {
                try
                { txtRef.ForeColor = Color.FromName(pRrc.color); }
                catch
                { }
            }

            //FI 20140912 [XXXXX] Add HyperLink on control
            if (txtRef.ReadOnly)
            {
                if (StrFunc.IsFilled(dataValue))
                    SetControlHyperLinkColumn(pPage, txtRef, pReferential, pRrc, dataValue, Cst.ConsultationMode.ReadOnly);
            }

            #endregion

            #region Create HTML Structure
            Table table = new Table
            {
                CellPadding = 0,
                CellSpacing = 0,
                BorderWidth = 0,
                BorderStyle = BorderStyle.None,
                Width = Unit.Percentage(100)
            };

            TableCell tc = new TableCell
            {
                Wrap = false,
                BorderStyle = BorderStyle.None,
                Width = Unit.Percentage(100)
            };

            TableRow tr = new TableRow();
            tr.Cells.Add(tc);
            table.Rows.Add(tr);


            // EG 20130527 Ticket:18694
            if (pIsModeForm)
            {
                tc.Controls.Add(txtRef);
                tc.Controls.Add(hidRef);
            }
            else
            {
                tc.Controls.Add(txtRef);
            }

            pRrc.ControlMain = table;
            #endregion
        }
        #endregion CreateControl_AutoComplete

        #region GetDataTRIM
        public static string GetDataTRIM(string pData, out string opCulture)
        {
            const string TODOTOTRANSLATE = @"TODO/TOTRANSLATE";
            string Template = @"<Info lang=""EN"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""FR"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""IT"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            Template += @"<Info lang=""ES"">" + Cst.CrLf + TODOTOTRANSLATE + Cst.CrLf + Cst.CrLf;
            string culture = SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper();
            //
            bool isFindCulture = false;
            int start;
            string search;
            string ret = string.Empty;
            //
            if (pData == Template)
                pData = string.Empty;
            //
            if (StrFunc.IsFilled(pData))
            {
                int guard = 0;
                while (!isFindCulture && (culture != "**") && (guard < 5))
                {
                    guard++;
                    //
                    search = @"<Info lang=""" + culture + @""">";
                    start = pData.IndexOf(search);
                    if (start >= 0)
                    {
                        //Culture courante trouvée
                        ret = pData.Substring(start + search.Length + 1).TrimStart();
                        //Vérification que la culture trouvée ne contient pas TODOTOTRANSLATE
                        isFindCulture = (ret.IndexOf(TODOTOTRANSLATE) != 0);
                    }
                    #region Recherche d'une autre cuture
                    if (!isFindCulture)
                    {
                        switch (SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                        {
                            case "EN":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "IT";
                                else if (culture == "IT")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "FR":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "IT";
                                else if (culture == "IT")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "IT":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "ES";
                                else
                                    culture = "**";
                                break;
                            case "ES":
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else if (culture == "EN")
                                    culture = "FR";
                                else if (culture == "FR")
                                    culture = "IT";
                                else
                                    culture = "**";
                                break;
                            default:
                                if (culture == SessionTools.Collaborator_Culture_ISOCHAR2.ToUpper())
                                    culture = "EN";
                                else
                                    culture = "**";
                                break;
                        }
                    }
                    #endregion
                }
                if (isFindCulture)
                {
                    search = @"<Info lang=""";
                    start = ret.IndexOf(search);
                    if (start >= 0)
                    {
                        //Suppression des culture suivantes
                        ret = ret.Substring(0, start);
                    }
                }
                else
                {
                    //Aucune culture trouvée --> le message doit être en English...
                    ret = pData;
                    ret = ret.Replace(TODOTOTRANSLATE, "...");
                }
            }
            //
            opCulture = culture;
            return ret;
        }
        #endregion GetDataTRIM

        #region GetCssClassForXXXXValue
        public static EFSCssClass.CssClassEnum GetCssClassForTradeSideValue(string pData, bool pIsLight)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_BuyerSeller;

            switch (pData)
            {
                // ------------------------------------------------------------
                // BLUE
                // ------------------------------------------------------------
                case "A"://Achat
                case "B"://Buyer
                case "Buyer":
                case "Buy":
                case "1":
                case "Long":
                case "Ask":
                case "P":
                    if (pIsLight)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Buyer_Light;
                    else
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Buyer;
                    break;
                // ------------------------------------------------------------
                // RED
                // ------------------------------------------------------------
                case "V"://Vente
                case "S"://Seller
                case "Seller":
                case "Sell":
                case "2":
                case "Short":
                case "Bid":
                case "R":
                    if (pIsLight)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Seller_Light;
                    else
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_Seller;
                    break;
            }

            return cssClass;
        }

        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForQuoteSideValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;
            switch (pData)
            {
                case "Ask":
                    cssClass = EFSCssClass.CssClassEnum.dg_ask;
                    break;
                case "Bid":
                    cssClass = EFSCssClass.CssClassEnum.dg_bid;
                    break;
                case "Mid":
                    cssClass = EFSCssClass.CssClassEnum.dg_mid;
                    break;
                case "OfficialClose":
                    cssClass = EFSCssClass.CssClassEnum.dg_officialclose;
                    break;
                case "OfficialSettlement":
                    cssClass = EFSCssClass.CssClassEnum.dg_officialsettlement;
                    break;
            }
            return cssClass;
        }
        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForQuoteTimingValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;
            switch (pData)
            {
                case "Close":
                    cssClass = EFSCssClass.CssClassEnum.dg_quotetimingclose;
                    break;
                case "Open":
                    cssClass = EFSCssClass.CssClassEnum.dg_quotetimingopen;
                    break;
            }
            return cssClass;
        }

        // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
        // EG 20160304 New Gestion pour les consultations des LOG avec POSREQUESTTYPE
        // EG 20170412 [23081]
        // EG 20211028 [XXXXX] Correction Calcul de la classe CSS (Cas rencontré où pData comporte plus de 2 tirets ex : Identifier de CA)
        public static void SetCssClassForLogPosRequestTypeValue(string pPrefixCssClass, Control pControl, string pVersion, string pData)
        {
            // REQUESTYPE-STATUS
            if (StrFunc.IsFilled(pData))
            {
                string[] _datas = pData.Split('-');
                //string _data = _datas[0];
                //string _status = _datas[1];
                //string _level = _datas[2];
                int lenghtData = _datas.Length;
                string _data = String.Join("-", _datas, 0, lenghtData - 2);
                string _status = _datas[lenghtData - 2];
                string _level = _datas[lenghtData - 1];

                WCToolTipPanel pnl = SetCssClassForPosRequestTypeValue(pPrefixCssClass, pControl, pVersion, _data);
                if (null != pnl)
                    pnl.CssClass += "-" + _status + "-" + _level;
            }
        }
        // EG 20160304 Upd Gestion pour les consultations des LOG avec POSREQUESTTYPE
        // EG 20171113 Upd
        // EG 20211028 [XXXXX] Forcer l'afficher de pData si ressource non trouvée (cas des CA où pData = Identifier de la CA)
        public static WCToolTipPanel SetCssClassForPosRequestTypeValue(string pPrefixCssClass, Control pControl, string pVersion, string pData)
        {
            WCToolTipPanel pnl = null;

            string _data = pData;
            string _resData = Ressource.GetString(pData, true);
            //Nullable<Cst.PosRequestTypeEnum> posRequestType = (Nullable<Cst.PosRequestTypeEnum>)StringToEnum.GetEnumValue(new Cst.PosRequestTypeEnum(), _data);
            if (StrFunc.IsFilled(_data))
            {
                Nullable<Cst.PosRequestTypeEnum> posRequestType = ReflectionTools.ConvertStringToEnumOrNullable<Cst.PosRequestTypeEnum>(_data);
                if (false == posRequestType.HasValue)
                {
                    if (Enum.IsDefined(typeof(Cst.PosRequestTypeEnum), _data))
                        posRequestType = (Cst.PosRequestTypeEnum)Enum.Parse(typeof(Cst.PosRequestTypeEnum), _data);
                    else
                        posRequestType = StringToEnum.ConvertToPosRequestTypeEnum(_data);
                }
                _data = ReflectionTools.GetXmlEnumAttributName(typeof(Cst.PosRequestTypeEnum), posRequestType.ToString());

                if (posRequestType.HasValue || (pData == "RECLEARING"))
                {
                    if (pControl is Label)
                    {
                        pnl = new WCToolTipPanel();
                        pControl.Parent.Controls.Add(pnl);
                        pControl.Parent.Controls.Remove(pControl);
                        pnl.CssClass = pPrefixCssClass + "-" + (StrFunc.IsFilled(pVersion) ? pVersion : "h") + "-" + _data;
                        pnl.Attributes.Add("prt-data", _data);
                        pnl.Attributes.Add("prt-exdata", Ressource.GetString(posRequestType.HasValue ? posRequestType.Value.ToString() : "ReClearing", _resData));
                        if (pData == "RECLEARING")
                            pnl.Pty.TooltipContent = Ressource.GetString("Qtip_UNCLEARINGREMAIN");
                        else
                            pnl.Pty.TooltipContent = Ressource.GetString("Qtip_" + _data);
                    }
                }
            }
            return pnl;
        }

        // EG 20160216 New
        public static EFSCssClass.CssClassEnum GetCssClassForMoneyPositionEnumValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            switch (pData)
            {
                case "OTM":
                case "Out the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_otm;
                    break;
                case "ITM":
                case "In the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_itm;
                    break;
                case "ATM":
                case "At the money":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm;
                    break;
                // CC/PL 20170307 [22916] 
                case "ATM_ITM":
                case "At the money (ITM)":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm_itm;
                    break;
                case "ATM_OTM":
                case "At the money (OTM)":
                    cssClass = EFSCssClass.CssClassEnum.dg_atm_otm;
                    break;
                case "N/A":
                    cssClass = EFSCssClass.CssClassEnum.dg_na;
                    break;
            }
            return cssClass;
        }
        /// <summary>
        /// Donne La class qui colorise la cellule en rouge ou en vert en fonction de la valeur {pData} 
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// FI 20120605 place GetCssClassForSide  
        public static EFSCssClass.CssClassEnum GetCssClassForGreenRed(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_PayerReceiver;

            switch (pData)
            {
                // ------------------------------------------------------------
                // RED
                // ------------------------------------------------------------
                case "A"://Achat
                case "B"://Buyer
                case "Buyer":
                case "Buy":
                case "1":
                case "Long":
                case "Ask":
                case "P":
                case "Pay":
                case "Payer":
                case "Debit":
                case "DEBIT":
                case "ERROR":
                case "Out":
                case "Override":
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Payer;
                    break;
                // ------------------------------------------------------------
                // GREEN
                // ------------------------------------------------------------
                case "V"://Vente
                case "S"://Seller
                case "Seller":
                case "Sell":
                case "2":
                case "Short":
                case "Bid":
                case "R":
                case "Receive":
                case "Receiver":
                case "Credit":
                case "CREDIT":
                case "SUCCESS":
                case "In":
                case "Standard":
                case "Beneficiary":
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Receiver;
                    break;
                default:
                    //Warning: A REVOIR: Utilisation en DUR pour les écritures comptables (PL)
                    //PL DISPLAY_RUPTURE_TOTAL Code ajouté pour géré en dur une rupture avec sous-total pour les écritures comptables
                    if (pData.IndexOf("Unbalanced") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_ErrorBackColor;
                    else if (pData.IndexOf("Well-balanced") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_SuccessBackColor;
                    //Warning: A REVOIR: Utilisation en DUR pour les logs (CC)
                    else if (pData.IndexOf("INFO") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_InfoBackColor;
                    else if ((pData.IndexOf("NA") >= 0) || (pData.IndexOf("N/A") >= 0))
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_NaBackColor;
                    else if (pData.IndexOf("WARNING") >= 0)
                        cssClass = EFSCssClass.CssClassEnum.DataGrid_WarningBackColor;
                    break;
            }

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForQuantityValue(string pData, bool pIsLight)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_BuyerSeller;

            if (StrFunc.IsFilled(pData))
            {
                decimal decdata = Convert.ToDecimal(pData);
                if (decdata > 0)
                    cssClass = GetCssClassForTradeSideValue("1", pIsLight);
                else if (decdata < 0)
                    cssClass = GetCssClassForTradeSideValue("2", pIsLight);
            }
            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForAmountValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.DataGrid_PayerReceiver;

            if (StrFunc.IsFilled(pData))
            {
                decimal decdata = Convert.ToDecimal(pData);
                if (decdata > 0)
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Receiver;
                else if (decdata < 0)
                    cssClass = EFSCssClass.CssClassEnum.DataGrid_Payer;
            }
            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForReverseAmountValue(string pData)
        {
            if (StrFunc.IsFilled(pData))
                pData = (Convert.ToDecimal(pData) * (-1)).ToString();

            return GetCssClassForAmountValue(pData);
        }
        // EG 20180525 [23979] IRQ Processing
        public static Color GetColorForStatusValue(string pData)
        {
            //Warning: see aso GetCssClassForStatusValue()
            Color color = Color.Empty;

            if (StrFunc.IsFilled(pData))
            {
                pData = pData.Replace(Cst.HTMLBold, string.Empty);
                pData = pData.Replace(Cst.HTMLEndBold, string.Empty);

                if (ProcessStateTools.IsStatusSuccess(pData))
                    color = CstCSSColor.Convert(CstCSSColor.green);
                else if (ProcessStateTools.IsStatusError(pData))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (ProcessStateTools.IsStatusWarning(pData))
                    color = CstCSSColor.Convert(CstCSSColor.orange);
                else if (ProcessStateTools.IsStatusNone(pData))
                    color = CstCSSColor.Convert(CstCSSColor.blue);
                else if (ProcessStateTools.IsStatusInterrupt(pData))
                    color = CstCSSColor.Convert(CstCSSColor.black);
                else if (pData == LevelStatusTools.StatusEnum.INFO.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.blue);
                else if (pData == Cst.ErrLevel.FAILURE.ToString() || pData == Cst.ErrLevel.BREAK.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.TIMEOUT.ToString() || pData == Cst.ErrLevel.DEADLOCK.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.UNDEFINED.ToString() || pData == Cst.ErrLevel.ABORTED.ToString() || pData == Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData == Cst.ErrLevel.DATAUNMATCH.ToString() || pData == Cst.ErrLevel.DATADISABLED.ToString())
                    color = CstCSSColor.Convert(CstCSSColor.gray);
                else if (ProcessStateTools.IsStatusProgress(pData))
                    color = CstCSSColor.Convert(CstCSSColor.violet);
                else if (ProcessStateTools.IsStatusPending(pData))
                    color = CstCSSColor.Convert(CstCSSColor.marron);
                else if (pData.StartsWith("ERROR") || pData.StartsWith("ALERT"))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData.StartsWith("WARNING"))
                    color = CstCSSColor.Convert(CstCSSColor.orange);
                else if (pData.StartsWith("MATCH"))
                    color = CstCSSColor.Convert(CstCSSColor.green);
                else if (pData.StartsWith("UNMATCH"))
                    color = CstCSSColor.Convert(CstCSSColor.red);
                else if (pData.EndsWith("NOTFOUND"))
                    color = CstCSSColor.Convert(CstCSSColor.yellow);
                else if (ProcessStateTools.IsStatusUnknown(pData))
                    color = CstCSSColor.Convert(CstCSSColor.gray);
            }
            return color;
        }
        // EG 20180525 [23979] IRQ Processing
        public static EFSCssClass.CssClassEnum GetCssClassForStatusValue(string pData)
        {
            //Warning: see aso GetColorForStatusValue()
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (StrFunc.IsFilled(pData))
            {
                pData = pData.Replace(Cst.HTMLBold, string.Empty);
                pData = pData.Replace(Cst.HTMLEndBold, string.Empty);

                if (ProcessStateTools.IsStatusSuccess(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccess;
                else if (ProcessStateTools.IsStatusError(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (ProcessStateTools.IsStatusInterrupt(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusInterrupt;
                else if (ProcessStateTools.IsStatusWarning(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccessWarning;
                else if (ProcessStateTools.IsStatusNone(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusInfo;

                else if (pData == LevelStatusTools.StatusEnum.INFO.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusInfo;
                else if (pData == Cst.ErrLevel.FAILURE.ToString() || pData == Cst.ErrLevel.BREAK.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData == Cst.ErrLevel.TIMEOUT.ToString() || pData == Cst.ErrLevel.DEADLOCK.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData == Cst.ErrLevel.UNDEFINED.ToString() || pData == Cst.ErrLevel.ABORTED.ToString() || pData == Cst.ErrLevel.LOCKUNSUCCESSFUL.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusUnXxxx;
                else if (pData == Cst.ErrLevel.DATAUNMATCH.ToString() || pData == Cst.ErrLevel.DATADISABLED.ToString())
                    cssClass = EFSCssClass.CssClassEnum.StatusOthers;

                else if (ProcessStateTools.IsStatusUnknown(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusNA;
                else if (ProcessStateTools.IsStatusProgress(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusProgress;
                else if (ProcessStateTools.IsStatusPending(pData))
                    cssClass = EFSCssClass.CssClassEnum.StatusPending;

                else if (pData.StartsWith("ERROR"))
                    cssClass = EFSCssClass.CssClassEnum.StatusError;
                else if (pData.StartsWith("WARNING"))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccessWarning;
                else if (pData.StartsWith("MATCH"))
                    cssClass = EFSCssClass.CssClassEnum.StatusSuccess;
                else if (pData.StartsWith("UNMATCH"))
                    cssClass = EFSCssClass.CssClassEnum.StatusUnXxxx;

                else if (pData.EndsWith("NOTFOUND"))
                    //Cst.ErrLevel.DATANOTFOUND, Cst.ErrLevel.FILENOTFOUND, Cst.ErrLevel.FOLDERNOTFOUND
                    cssClass = EFSCssClass.CssClassEnum.StatusNotfound;
            }

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForLevelValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (LevelStatusTools.IsLevelAlert(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelAlertMsg;
            else if (LevelStatusTools.IsLevelInfo(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelInfoMsg;
            else if (LevelStatusTools.IsLevelWarning(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelWarningMsg;
            else if (LevelStatusTools.IsLevelUnknown(pData))
                cssClass = EFSCssClass.CssClassEnum.LevelOtherMsg;

            return cssClass;
        }
        public static EFSCssClass.CssClassEnum GetCssClassForReadyStateValue(string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if (ProcessStateTools.IsReadyStateActive(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateActiveMsg;
            else if (ProcessStateTools.IsReadyStateRequested(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateRequestedMsg;
            else if (ProcessStateTools.IsReadyStateTerminated(pData))
                cssClass = EFSCssClass.CssClassEnum.ReadyStateTerminatedMsg;
            return cssClass;
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="pModel"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        public static EFSCssClass.CssClassEnum GetCssClassForModel(string pModel, string pData)
        {
            EFSCssClass.CssClassEnum cssClass = EFSCssClass.CssClassEnum.UNKNOWN;

            if ("status" == pModel)
                cssClass = ReferentialTools.GetCssClassForStatusValue(pData);
            else if ("level" == pModel)
                cssClass = ReferentialTools.GetCssClassForLevelValue(pData);
            else if ("readystate" == pModel)
                cssClass = ReferentialTools.GetCssClassForReadyStateValue(pData);
            else if ("side" == pModel)
                cssClass = ReferentialTools.GetCssClassForGreenRed(pData);
            else if ("tradeside" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue(pData, false);
            else if ("tradesideLong" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue("1", false);
            else if ("tradesideShort" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue("2", false);
            else if ("tradeside_light" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue(pData, true);
            else if ("tradesideLong_light" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue("1", true);
            else if ("tradesideShort_light" == pModel)
                cssClass = ReferentialTools.GetCssClassForTradeSideValue("2", true);
            else if ("quantity" == pModel)
                cssClass = ReferentialTools.GetCssClassForQuantityValue(pData, false);
            else if ("quantity_light" == pModel)
                cssClass = ReferentialTools.GetCssClassForQuantityValue(pData, true);
            else if ("amount" == pModel)
                cssClass = ReferentialTools.GetCssClassForAmountValue(pData);
            else if ("reverseamount" == pModel)
                cssClass = ReferentialTools.GetCssClassForReverseAmountValue(pData);
            else if ("moneyposition" == pModel)
                cssClass = ReferentialTools.GetCssClassForMoneyPositionEnumValue(pData);
            else if ("quoteside" == pModel)
                cssClass = ReferentialTools.GetCssClassForQuoteSideValue(pData);
            else if ("quotetiming" == pModel)
                cssClass = ReferentialTools.GetCssClassForQuoteTimingValue(pData);
            else if ("greenRed" == pModel)
            {
                //FI 20120605 add greenRed
                cssClass = ReferentialTools.GetCssClassForGreenRed(pData);
            }
            return cssClass;
        }
        // EG 20160224 New Gestion CSS complex sur Cellule (REQUESTTYPE)
        public static void SetCssClassForComplexModel(string pComplexModel, Control pControl, string pVersion, string pData)
        {
            switch (pComplexModel)
            {
                case "posRequestType":
                    ReferentialTools.SetCssClassForPosRequestTypeValue("prt", pControl, pVersion, pData);
                    break;
                case "logPosRequestType":
                    ReferentialTools.SetCssClassForLogPosRequestTypeValue("log-prt", pControl, pVersion, pData);
                    break;
            }
        }
        public static string GetCssClassFromDataValue(ReferentialsReferentialColumnStyleBase pStyle, string pData)
        {
            string cssClass = string.Empty;
            try
            {
                bool isVerify = false;
                //
                if (pStyle.WhenSpecified)
                {
                    foreach (ReferentialsReferentialColumnStyleWhen when in pStyle.When)
                    {
                        if (StrFunc.IsFilled(when.test))
                        {
                            string[] test = when.test.Split(";".ToCharArray());
                            string comparator = test[0].Substring(0, 1);
                            //
                            for (int pass = 1; pass <= 2; pass++)
                            {
                                if (pass == 2)
                                    comparator = test[0].Substring(0, 2);
                                //
                                if ((comparator == "=") || (comparator == "!="))
                                {
                                    for (int i = 0; i < test.Length; i++)
                                    {
                                        string testItem = test[i];
                                        if (i == 0)
                                            testItem = test[i].Substring(comparator.Length);
                                        //
                                        isVerify = ((StrFunc.IsEmpty(pData) && StrFunc.IsEmpty(testItem)));
                                        //
                                        if (false == isVerify && StrFunc.IsFilled(pData))
                                            isVerify = (pData.CompareTo(testItem) == 0);
                                        //
                                        if (isVerify)
                                            break;
                                    }
                                    //
                                    if (comparator == "!=")
                                        isVerify = (!isVerify);
                                }
                            }
                            //
                            if (isVerify)
                            {
                                cssClass = when.Value;
                                break;
                            }
                        }
                    }
                }
                //
                if (false == isVerify && pStyle.OtherwiseSpecified)
                    cssClass = pStyle.Otherwise.Value;
            }
            catch { }
            return cssClass;
        }
        #endregion

        #region FormatLongData
        public static string FormatLongData(string pData)
        {
            return FormatLongData(pData, 0);
        }
        public static string FormatLongData(string pData, int pMaxLength)
        {
            pData = pData.Replace(Cst.CrLf, Cst.HTMLBreakLine);
            pData = pData.Replace(Cst.Lf, Cst.HTMLBreakLine);
            pData = pData.Replace(Cst.Tab, Cst.HTMLSpace4);

            if ((pMaxLength > 0) && (pData.Length > pMaxLength))
            {
                pData = pData.Substring(0, pMaxLength) + " ...";
            }

            if (pData.StartsWith(@"[Code Return:"))
            {
                int pos = pData.IndexOf(@"]");
                if (pos > 0)
                {
                    string code = pData.Substring(13, pos - 13).Trim();
                    EFSCssClass.CssClassEnum cssClass = GetCssClassForStatusValue(code);
                    if (!EFSCssClass.IsUnknown(cssClass))
                        pData = string.Format(@"[Code Return: <span class=""{0}""><b>{1}</b></span>]{2}",
                            cssClass.ToString(), code, pData.Substring(pos + 1));
                }
            }
            else if (pData.StartsWith(@"Result:"))
            {
                string code = pData.Substring(7).Trim();
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    code = pData.Substring(7, pos - 7).Trim();
                EFSCssClass.CssClassEnum cssClass = GetCssClassForStatusValue(code);
                if (!EFSCssClass.IsUnknown(cssClass))
                    pData = string.Format(@"Result: <span class=""{0}""><b>{1}</b></span>{2}",
                        cssClass.ToString(), code, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            else if (pData.StartsWith(@"<b>Warning"))
            {
                string data = pData;
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    data = pData.Substring(0, pos).Trim();
                pData = string.Format(@"<span class=""{0}"">{1}</span>{2}",
                    EFSCssClass.CssClassEnum.StatusSuccessWarning.ToString(), data, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            else if (pData.StartsWith(@"<b>Data imported successfully") || pData.StartsWith(@"<b>Data exported successfully") || pData.StartsWith(@"<b>Data matched successfully"))
            {
                string data = pData;
                int pos = pData.IndexOf(Cst.HTMLBreakLine);
                if (pos > 0)
                    data = pData.Substring(0, pos).Trim();
                pData = string.Format(@"<span class=""{0}"">{1}</span>{2}",
                    EFSCssClass.CssClassEnum.StatusSuccess.ToString(), data, (pos >= 0) ? pData.Substring(pos) : string.Empty);
            }
            return pData;
        }
        #endregion FormatLongData
        #region FoundValue
        private static bool FoundValue(bool pIsHTMLSelect, DropDownList pDdlRef, HtmlSelect pHtmlddlRef, string pSelectedValue,
            ReferentialsReferentialColumn pRrc, ref string opTextSelectedValue, ref string opWarningMsg)
        {
            bool isHideWarning = false;

            bool ret;
            if (pIsHTMLSelect)
                ret = ControlsTools.DDLSelectByValue(pHtmlddlRef, pSelectedValue);
            else
                ret = ControlsTools.DDLSelectByValue(pDdlRef, pSelectedValue);

            if ((!ret) && (StrFunc.IsFilled(pSelectedValue)))
            {
                opTextSelectedValue = pSelectedValue;
                if (StrFunc.IsEmpty(opWarningMsg))
                {
                    opWarningMsg = Ressource.GetString("Msg_DataUnavailableOrRemoved", "Warning: Data disabled or removed !") + Cst.CrLf;
                    if (pRrc.Relation[0].TableName == Cst.OTCml_TBL.ACTOR.ToString())
                    {
                        if (pSelectedValue == 0.ToString())
                        {
                            //PL 20110819 Newness
                            //On est ici sur le cas particulier d'un acteur non renseigné (ex. SYSTEM_L)
                            isHideWarning = true;
                        }
                        else
                        {
                            //On est ici sur un acteur "indisponible" dans la DDL
                            opWarningMsg = Ressource.GetString("Msg_ActorUnavailableOrRemoved", "Warning: Actor disabled or removed !") + Cst.CrLf;
                            //Recherche de l'identifier/displayname pour affichage
                            try
                            {
                                SQL_Actor sql_Actor = new SQL_Actor(SessionTools.CS, Convert.ToInt32(pSelectedValue));
                                if (sql_Actor.IsLoaded)
                                {
                                    opTextSelectedValue = sql_Actor.Identifier + " - " + sql_Actor.DisplayName;
                                    if (sql_Actor.IsEnabled)
                                        opWarningMsg = Ressource.GetString("Msg_ActorHasNotRole", "Warning: Actor has not the necessary role !") + Cst.CrLf;
                                }
                                else
                                {
                                    opWarningMsg = Ressource.GetString("Msg_ActorRemoved", "Warning: Actor removed !") + Cst.CrLf;
                                }
                                sql_Actor = null;
                            }
                            catch { }
                        }
                    }
                }
                if (isHideWarning)
                {
                    opWarningMsg = string.Empty;
                    opTextSelectedValue = Ressource.GetString("Msg_UnavailableOrRemoved", "[unavailable]");
                }
                else
                {
                    opWarningMsg += Cst.CrLf + pRrc.Label + ": " + opTextSelectedValue;
                    opTextSelectedValue += " " + Ressource.GetString("Msg_UnavailableOrRemoved", "[unavailable]");
                }

                ListItem liUnavailable = new ListItem(opTextSelectedValue, pSelectedValue);
                liUnavailable.Attributes.Add("style", "color:#FFFFFF;background-color:#AE0303");
                if (pIsHTMLSelect)
                {
                    pHtmlddlRef.Items.Add(liUnavailable);
                    ret = ControlsTools.DDLSelectByValue(pHtmlddlRef, pSelectedValue);
                }
                else
                {
                    pDdlRef.Items.Add(liUnavailable);
                    ret = ControlsTools.DDLSelectByValue(pDdlRef, pSelectedValue);
                }
            }
            //
            return ret;

        }
        #endregion FoundValue
        #region private DispatchListRetrieval
        private static void DispatchListRetrieval(ref ReferentialsReferentialColumn opRrc, string pListRetrieval)
        {
            if (ControlsTools.DecomposeListRetrieval(pListRetrieval, out Cst.ListRetrievalEnum listRetrievalType, out string listRetrievalData))
            {
                opRrc.ListRetrievalType = listRetrievalType.ToString().ToLower();
                //
                switch (listRetrievalType)
                {
                    case Cst.ListRetrievalEnum.PREDEF:
                        //PL 20101020 Debug --> Add new ReferentialsReferentialColumnRelationDDLType()
                        if (opRrc.Relation[0].DDLType == null)
                            opRrc.Relation[0].DDLType.Value = listRetrievalData;
                        break;
                    default:
                        opRrc.ListRetrievalData = listRetrievalData;
                        break;
                }
            }
        }
        #endregion
        #region private SetJavascriptToColumnControl
        /// <creation>
        ///     <version>1.0.?</version><date>2004????</date><author>OD</author>
        /// </creation>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>
        /// <summary>
        /// Ajoute du javascript au control d'une colonne en fonction de ce qui est paramétré dans l'element <Help> et/ou <JavaScript> de cette colonne
        /// </summary>
        /// <param name="pCtrlNew">control à implémenter</param>
        /// <param name="pReferential">classe referentiel</param>
        /// <param name="pRrc">colonne à traiter</param>
        private static void SetJavascriptToColumnControl(Control pCtrlNew, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc)
        {
            if (pRrc.ExistsHelp)
            {
                XMLJavaScript.SetHelp(pCtrlNew, pRrc.Help);
            }
            //
            if (pRrc.ExistsJavaScript)
            {
                JavaScript.JavaScriptScript jss;
                for (int i = 0; i <= pRrc.JavaScript.Script.GetLength(0) - 1; i++)
                {
                    jss = pRrc.JavaScript.Script[i];

                    XMLJavaScript.Interpret(jss);

                    //dans le cas de EnabledDisabled on obtient l'ID du(des) control(s) dans la page
                    if (JavaScript.IsScriptTypeEnabledDisabled(jss.name)
                        || JavaScript.IsScriptTypeEFS_Copy(jss.name)
                        || JavaScript.IsScriptTypeEFS_Reset(jss.name)
                        || JavaScript.IsScriptTypeEFS_Set(jss.name)
                        || JavaScript.IsScriptTypeEFS_ApplyOffset(jss.name)
                        )
                    {
                        for (int ni = 0; ni < jss.aControl.Length; ni++)
                        {
                            ReferentialsReferentialColumn rrccolID = pReferential[jss.aControl[ni]];
                            if (jss.aControl[ni] != "this" && rrccolID != null)
                            {
                                string prefix = ReferentialTools.IsDataForDDL(rrccolID) ? Cst.DDL : null;
                                string suffix = rrccolID.IsAdditionalData ? rrccolID.ExternalFieldID.ToString() : string.Empty;
                                jss.aControl[ni] = ControlID.GetID(rrccolID.ColumnName + suffix, rrccolID.DataType.value, prefix);
                            }
                        }
                        if (JavaScript.IsScriptTypeEnabledDisabled(jss.name))
                            XMLJavaScript.InterpretEnabledDisabled(jss);
                        else if (JavaScript.IsScriptTypeEFS_Copy(jss.name))
                            XMLJavaScript.InterpretCopy(jss);
                        else if (JavaScript.IsScriptTypeEFS_Reset(jss.name) || JavaScript.IsScriptTypeEFS_Set(jss.name))
                            XMLJavaScript.InterpretSet(jss);
                        else if (JavaScript.IsScriptTypeEFS_ApplyOffset(jss.name))
                            XMLJavaScript.InterpretApplyOffset(jss);
                    }
                    //on ajoute le script sur l'attribut
                    XMLJavaScript.AddScriptToCtrl(pCtrlNew, jss);

                }
            }
        }
        #endregion SetJavascriptToColumnControl

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est le premier d'une serie de controls liés (1 seul libellé)
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <returns>bool</returns>
        public static bool IsFirstControlLinked(int pIndex, ReferentialsReferential pReferential)
        {
            // CC|PL 20160603 Add Test on IsHide (see QUOTE_H_EOD.xml column ASSET_DSP)
            bool ret = false;
            if ((pIndex + 1 < pReferential.Column.Length) &&
                // (StrFunc.IsEmpty(pReferential.Column[pIndex + 1].Ressource) &&
                (StrFunc.IsEmpty(pReferential.Column[pIndex + 1].Ressource) &&
                (!pReferential.Column[pIndex + 1].IsHide)) &&
                (StrFunc.IsFilled(pReferential.Column[pIndex].Ressource)))
            {
                pReferential.NbControlLinked = 2;
                int index = pIndex + 1;
                while (index + 1 < pReferential.Column.Length && StrFunc.IsEmpty(pReferential.Column[index + 1].Ressource))
                {
                    pReferential.NbControlLinked++;
                    index++;
                }
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est dans une serie de controls liés (1 seul libellé) sans etre ni le premier ni le dernier de cette serie
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">REF classe referential</param>
        /// <returns>bool</returns>
        public static bool IsMiddleControlLinked(int pIndex, ReferentialsReferential pReferential)
        {
            if ((pIndex - 1 >= 0) && (pIndex + 1 < pReferential.Column.Length))
                return (IsFirstControlLinked(pIndex - 1, pReferential) &&
                    StrFunc.IsEmpty(pReferential.Column[pIndex + 1].Ressource) &&
                    StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource));
            else
                return false;
        }

        /// <summary>
        /// Verifie si la colonne de l'index passé en arg est la dernière d'une serie de controls liés (1 seul libellé)
        /// </summary>
        /// <param name="pIndex">index de la colonne à verifier</param>
        /// <param name="pReferential">classe referential</param>
        /// <returns>bool</returns>
        public static bool IsLastControlLinked(int pIndex, ReferentialsReferential pReferential)
        {
            if (pIndex + 1 < pReferential.Column.Length)
                return (StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource) && StrFunc.IsFilled(pReferential.Column[pIndex + 1].Ressource));
            else
                return (StrFunc.IsEmpty(pReferential.Column[pIndex].Ressource));
        }

        /// <summary>
        /// Renvoie true si la colonne spécifié est de type DDL
        /// </summary>
        /// <param name="pRrc">la colonne à verifier</param>
        /// <returns>bool</returns>
        public static bool IsDataForDDL(ReferentialsReferentialColumn pRrc)
        {

            bool isDDL = pRrc.ExistsRelation
                //// 20100607 MF AutoComplete
                && !pRrc.AutoCompleteRelationEnabled;
            if (!isDDL)
            {
                if (pRrc.ExistsDDLType)
                {
                    if (pRrc.Relation[0].DDLType == null)
                        isDDL = true;
                    else
                        isDDL = !Cst.IsDDLTypeSumAmount(pRrc.Relation[0].DDLType.Value)
                                && !Cst.IsDDLTypePeriodMulti(pRrc.Relation[0].DDLType.Value);
                }
            }
            if (!isDDL)
            {
                isDDL = IsDataForDDLParticular(pRrc);
            }
            //
            return isDDL;

        }

        /// <creation>
        ///     <version>1.0.?</version><date>????????</date><author>??</author>
        /// </creation>
        /// <revision>
        ///     <version>[X.X.X]</version><date>[YYYYMMDD]</date><author>[Initial]</author>
        ///     <comment>
        ///     [To insert here a description of the made modifications ]
        ///     </comment>
        /// </revision>        
        /// <summary>
        /// Renvoie true si la colonne spécifié est de type DDL cas particulier
        /// </summary>
        /// <param name="pRrc">la colonne à verifier</param>
        /// <returns>bool</returns>
        public static bool IsDataForDDLParticular(ReferentialsReferentialColumn pRrc)
        {
            bool isDDL = false;
            // EG 20160404 Migration vs2013
            // #warning 20030101 PL (Not Urgent) Codage en dur à revoir
            isDDL |= (pRrc.ColumnName == "MTMMETHOD");
            isDDL |= (pRrc.ColumnName == "ACCRUEDINTMETHOD");
            isDDL |= (pRrc.ColumnName == "ACCRUEDINTPERIOD");
            isDDL |= (pRrc.ColumnName == "LINEARDEPPERIOD");
            //isDDL |= (pRrc.ColumnName=="SOURCE");
            isDDL |= (pRrc.ColumnName == "GPRODUCT");
            isDDL |= (pRrc.ColumnName == "FAMILY");
            isDDL |= (pRrc.ColumnName == "CLASS");

            return isDDL;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <param name="pDDLRef"></param>
        /// <param name="pValueForeignKeyField"></param>
        /// <param name="pCurrentNonUpdatableValue"></param>
        /// <param name="pIsNewRecord"></param>
        /// EG 20170929 [22374][23450] Add ReplaceColumnValueArgument
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        /// EG [XXXXX][WI417] DataParameter des colonnes d'audit datetime2/timestamp(7)
        private static void LoadDDL(PageBase pPage, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc, DropDownList pDDLRef,
            string pValueForeignKeyField, string pCurrentNonUpdatableValue, bool pIsNewRecord)
        {
            string source = SessionTools.CS;
            //
            if ((pRrc.Relation[0].DDLType != null) && (pRrc.Relation[0].DDLType.Value != null))
            {
                bool isResource = (pRrc.IsResourceSpecified && pRrc.IsResource.IsResource);
                bool isWithEmpy = !pRrc.IsMandatory;
                bool isDynamicArguments = pRrc.Relation[0].DDLType.Value.Contains(Cst.DA_START);
                string misc = ((pRrc.MiscSpecified) ? pRrc.Misc : string.Empty);

                //
                if (StrFunc.IsFilled(pValueForeignKeyField))
                {
                    if (misc.Length > 0)
                        misc += ";";
                    misc += "FK:" + pValueForeignKeyField;
                }
                if (pRrc.Relation[0].DDLType.miscSpecified)
                {
                    if (misc.Length > 0)
                        misc += ";";
                    misc += pRrc.Relation[0].DDLType.misc;
                }
                //
                string objectsPath = string.Empty;
                string ddlType = pRrc.Relation[0].DDLType.Value;
                if (isDynamicArguments)
                    ddlType = pReferential.ReplaceDynamicArgument2(source, ddlType);

                // EG 20170929 [22374][23450]
                ddlType = ReplaceColumnValueArgument(pPage, ddlType, pReferential);
                //
                if (Cst.IsDDLTypeWebControlType(ddlType))
                    objectsPath = pPage.Server.MapPath(Cst.CustomTradePath + @"\Objects\");
                //

                ControlsTools.DDLLoad_FromListRetrieval(pPage, pDDLRef, source, Cst.ListRetrievalEnum.PREDEF, ddlType,
                    isWithEmpy, isResource, misc, objectsPath);
            }
            else if (StrFunc.IsFilled(pRrc.ListRetrievalType))
            {
                bool isResource = (pRrc.IsResourceSpecified && pRrc.IsResource.IsResource);
                bool isWithEmpy = !pRrc.IsMandatory;
                string misc = ((pRrc.MiscSpecified) ? pRrc.Misc : string.Empty);
                Cst.ListRetrievalEnum listRetrieval = (Cst.ListRetrievalEnum)Enum.Parse(typeof(Cst.ListRetrievalEnum), pRrc.ListRetrievalType, true);
                //
                ControlsTools.DDLLoad_FromListRetrieval(pDDLRef, source, listRetrieval, pRrc.ListRetrievalData,
                    isWithEmpy, isResource, misc);
            }
            //-----------------------------------------------------------------------
            //20081118 A priori inutile maintenat ...
            else if (pRrc.ColumnName.StartsWith("IDSTENVIRONMENT"))
                ControlsTools.DDLLoad_StatusEnvironment(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("IDSTACTIVATION"))
                ControlsTools.DDLLoad_StatusActivation(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("IDSTPRIORITY"))
                ControlsTools.DDLLoad_StatusPriority(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("IDSTCHECK"))
                ControlsTools.DDLLoad_StatusCheck(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("IDSTMATCH"))
                ControlsTools.DDLLoad_StatusMatch(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("IDSTPROCESS"))
                ControlsTools.DDLLoad_StatusProcess(SessionTools.CS, pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName.StartsWith("BUYER_SELLER"))
                ControlsTools.DDLLoad_BuyerSellerType(pDDLRef, !pRrc.IsMandatory);
            //else if (pRrc.ColumnName.EndsWith("POS"))
            //    ControlsTools.DDLLoad_PositionType(pDDLRef, !pRrc.IsMandatory);
            //-----------------------------------------------------------------------
            // EG 20160404 Migration vs2013
            // #warning 20030101 PL (Not Urgent) Codage en dur à revoir
            //-----------------------------------------------------------------------
            else if (pRrc.ColumnName == "MTMMETHOD")
                ControlsTools.DDLLoad_FxMTMMethod(pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName == "ACCRUEDINTMETHOD")
                ControlsTools.DDLLoad_AccruedInterest(pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName == "ACCRUEDINTPERIOD")
                ControlsTools.DDLLoad_AccruedInterestPeriod(pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName == "LINEARDEPPERIOD")
                ControlsTools.DDLLoad_LinearDepreciationPeriod(pDDLRef, !pRrc.IsMandatory);
            //else if (pRrc.ColumnName=="SOURCE" || pRrc.ColumnName=="DEFINED")
            else if (pRrc.ColumnName == "DEFINED")
                ControlsTools.DDLLoad_Source(pDDLRef);
            else if (pRrc.ColumnName == "GPRODUCT")
                ControlsTools.DDLLoad_ProductGProduct(pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName == "FAMILY")
                ControlsTools.DDLLoad_ProductFamily(pDDLRef, !pRrc.IsMandatory);
            else if (pRrc.ColumnName == "CLASS")
                ControlsTools.DDLLoad_ProductClass(pDDLRef, !pRrc.IsMandatory);
            //-----------------------------------------------------------------------

            else
            {
                #region Relation Loading
                string SQLWhere = string.Empty;
                DataParameters parameters = new DataParameters();

                if ((pRrc.IsForeignKeyField && StrFunc.IsFilled(pValueForeignKeyField)) || (StrFunc.IsFilled(pCurrentNonUpdatableValue)))
                {
                    //The zone is disabled or has FK defined, loading only data for display
                    SQLWhere = SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnRelation[0].ColumnName + "=" + DataHelper.GetVarPrefix(source) + "PARAM";

                    DataParameter parameter;
                    if (TypeData.IsTypeInt(pRrc.DataType.value))
                        parameter = new DataParameter(source, "PARAM", DbType.Int64);
                    else if (TypeData.IsTypeBool(pRrc.DataType.value))
                        parameter = new DataParameter(source, "PARAM", DbType.Boolean);
                    else if (TypeData.IsTypeDec(pRrc.DataType.value))
                        parameter = new DataParameter(source, "PARAM", DbType.Decimal);
                    else if (TypeData.IsTypeDate(pRrc.DataType.value)) // FI 20201006 [XXXXX] DbType.Date  sur une donnée de type Date
                        parameter = new DataParameter(source, "PARAM", DbType.Date);
                    else if (TypeData.IsTypeDateTime(pRrc.DataType.value))
                        parameter = new DataParameter(source, "PARAM", DbType.DateTime);
                    else if (TypeData.IsTypeDateTimeOffset(pRrc.DataType.value))
                        parameter = new DataParameter(source, "PARAM", DbType.DateTimeOffset);
                    else
                        parameter = new DataParameter(source, "PARAM", DbType.AnsiString, 64);

                    parameters = new DataParameters();
                    parameters.Add(parameter, StrFunc.IsEmpty(pCurrentNonUpdatableValue)?pValueForeignKeyField:pCurrentNonUpdatableValue);
                }

                bool isColor = pRrc.Relation[0].iscolor;
                string SQLQuery = GetDDLSQLSelect(pPage, pReferential, pRrc, SQLWhere, false, isColor, pIsNewRecord, pValueForeignKeyField,
                    out string aliasColumnSelect, out _);
                bool isResource = (pRrc.IsResourceSpecified && pRrc.IsResource.IsResource);
                bool isSort = true;
                //ControlsTools.DDLLoad(pPage, pDDLRef, aliasColumnSelect, pRrc.Relation[0].ColumnRelation[0].ColumnName, source, SQLQuery,
                //    !pRrc.IsMandatory, isSort, isResource, null, sqlParam);
                //PM 20161124 [RATP] Ajout "ou" pIsNewRecord pour le paramètre pWithEmpty afin de laisser la DDL vide sur les nouveaux records
                //ControlsTools.DDLLoad(pPage, pDDLRef, aliasColumnSelect, pRrc.Relation[0].ColumnRelation[0].ColumnName, source, SQLQuery,
                //    pIsNewRecord || !pRrc.IsMandatory, isSort, isResource, null, sqlParam);
                //CC/PL 20170227 Add test on ForeignKeyField
                bool isEmpty = !pRrc.IsMandatory;
                if (pIsNewRecord && !pRrc.IsForeignKeyField)
                {
                    //Afin de laisser la DDL vide sur les nouveaux records, excepté pour les FK
                    isEmpty = true;
                }

                QueryParameters qryParameters = new QueryParameters(source, SQLQuery, parameters);
                ControlsTools.DDLLoad(pPage, pDDLRef, aliasColumnSelect, pRrc.Relation[0].ColumnRelation[0].ColumnName, source, qryParameters.Query,
                                      isEmpty, isSort, isResource, null, qryParameters.Parameters.GetArrayDbParameter());
                #endregion
            }
        }

        /// <summary>
        /// Mise à jour des COLUMN_VALUE sur DDLType
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pData"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc"></param>
        /// <returns></returns>
        /// EG 20170929 [22374][23450] New
        private static string ReplaceColumnValueArgument(PageBase pPage, string pData, ReferentialsReferential pReferential)
        {
            string data = pData;
            bool isColumnValue = data.Contains(Cst.COLUMN_VALUE);
            while (isColumnValue)
            {
                string currentvalue = string.Empty;
                string aliasTableName = null;

                int startConstant = data.IndexOf(Cst.COLUMN_VALUE);
                int endConstant = startConstant + Cst.COLUMN_VALUE.Length;
                int posDblPercent = data.IndexOf("%%", endConstant);
                string clientID = data.Substring(endConstant, posDblPercent - endConstant);
                string constantID = Cst.COLUMN_VALUE + clientID + "%%";

                if (clientID.IndexOf(".") > 0)
                {
                    int posDot = clientID.IndexOf(".");
                    aliasTableName = clientID.Substring(0, posDot);
                    clientID = clientID.Substring(posDot + 1);
                }

                ReferentialsReferentialColumn rrc = null;
                try
                {
                    string suffix_ClientID = string.Empty;
                    rrc = pReferential[clientID, aliasTableName];
                    if (rrc.IsAdditionalData)
                        suffix_ClientID = rrc.ExternalFieldID.ToString();

                    bool isEnabled = ((!rrc.IsForeignKeyField) || (StrFunc.IsEmpty(pReferential.ValueForeignKeyField)));
                    bool isFK = !isEnabled;

                    if ((pReferential.isLookReadOnly && !TypeData.IsTypeBool(rrc.DataType.value)) || isFK)
                    {
                        bool isOk = false;
                        if (isOk == false)
                        {
                            if (rrc.ControlMain is Table table)
                            {
                                isOk = true;
                                TableRow tr = table.Rows[0];
                                if (rrc.AutoCompleteRelationEnabled)
                                {
                                    TableCell td = tr.Cells[0];
                                    HiddenField hdn = (HiddenField)td.Controls[1];
                                    currentvalue = hdn.Value;
                                }
                                else
                                {
                                    if (tr.Cells[1] is TableCell td)
                                        currentvalue = td.Text;
                                }
                            }
                        }

                        if (isOk == false)
                        {
                            if (rrc.ControlMain is TextBox txt)
                            {
                                isOk = true;
                                currentvalue = txt.Text;
                            }
                        }

                    }
                    else if (pPage.IsPostBack)
                    {
                        #region IsPostBack
                        //Get value from Page
                        if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            clientID = Cst.CHK + clientID + suffix_ClientID;
                        }
                        else if (ReferentialTools.IsDataForDDL(rrc))
                        {
                            clientID = Cst.DDL + clientID + suffix_ClientID;
                        }
                        else
                        {
                            //PL 20130516 Test
                            if (rrc.ExistsRelation && rrc.AutoCompleteRelationEnabled)
                            {
                                clientID = Cst.HDN + clientID + suffix_ClientID;
                            }
                            else
                            {
                                clientID = Cst.TXT + clientID + suffix_ClientID;
                            }
                        }
                        // EG 20150923 Test null
                        if (null != pPage.Request.Form[clientID])
                        {
                            currentvalue = pPage.Request.Form[clientID].ToString();
                            if (TypeData.IsTypeBool(rrc.DataType.value))
                                currentvalue = DataHelper.SQLBoolean(BoolFunc.IsTrue(currentvalue));
                        }
                        #endregion IsPostBack
                    }
                    else
                    {
                        //Get value from Referential
                        if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            currentvalue = DataHelper.SQLBoolean(((CheckBox)rrc.ControlMain).Checked);
                        }
                        else if (ReferentialTools.IsDataForDDL(rrc))
                        {
                            currentvalue = ((DropDownList)rrc.ControlMain).SelectedValue;
                        }
                        else
                        {
                            //PL 20130516 Test
                            if (rrc.ExistsRelation && rrc.AutoCompleteRelationEnabled)
                            {
                                Table tbl = (Table)rrc.ControlMain;
                                TableRow tr = tbl.Rows[0];
                                TableCell tc = tr.Cells[0];
                                HiddenField hdn = (HiddenField)tc.Controls[1];
                                currentvalue = hdn.Value;
                            }
                            else
                            {
                                currentvalue = ((TextBox)rrc.ControlMain).Text;
                            }
                        }
                    }
                }
                catch
                {
                    #region catch
                    try
                    {
                        if (pPage.IsPostBack)
                        {
                            if (TypeData.IsTypeBool(rrc.DataType.value))
                                currentvalue = DataHelper.SQLBoolean(((CheckBox)rrc.ControlMain).Checked);
                            else if (ReferentialTools.IsDataForDDL(rrc))
                                currentvalue = ((DropDownList)rrc.ControlMain).SelectedValue;
                            else
                                currentvalue = ((TextBox)rrc.ControlMain).Text;
                        }
                    }
                    catch
                    {
                        currentvalue = string.Empty;
                    }
                    #endregion catch
                }

                if ((currentvalue.Length == 0) && (TypeData.IsTypeNumeric(rrc.DataType.value) || TypeData.IsTypeBool(rrc.DataType.value)))
                    currentvalue = SQLCst.NULL;

                data = data.Replace(constantID, currentvalue);

                isColumnValue = data.IndexOf(Cst.COLUMN_VALUE) > 0;
            }
            return data;
        }

        /// <summary>
        /// Renvoie la requete SQL pour un DDL de type <Relation>
        /// </summary>
        /// <param name="pRrc">colonne concernée</param>
        /// <param name="pSQLWhere">condition additionnelle si necessaire</param>
        /// <param name="pIsScalar"></param>
        /// <param name="pValueForeignKeyField">valeur pour la foreignKey</param>
        /// <param name="opAliasColumnSelect">OUT nom d'alias de la colonne dans la requete SQL générée</param>
        /// <returns></returns>
        /// FI 20210216 [XXXXX] Add opSqlWhereWithoutReplace
        private static string GetDDLSQLSelect(PageBase pPage,
            ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc,
            string pSQLWhere, bool pIsScalar, bool pIsColor, bool pIsNewRecord,
            string pValueForeignKeyField, out string opAliasColumnSelect, out string opSqlWhereWithoutReplace)
        {
            //PL 20100211 Add tableNameForDDL
            string tableNameForDDL = pRrc.Relation[0].TableNameForDDLSpecified ? pRrc.Relation[0].TableNameForDDL : pRrc.Relation[0].TableName;
            string sqlQuery;
            SQLWhere sqlWhere = new SQLWhere(pSQLWhere);
            SQLWhere sqlWhereWithoutReplace = new SQLWhere(pSQLWhere);

            if (pIsScalar)
            {
                sqlQuery = SQLCst.SELECT + " 1" + Cst.CrLf;
                opAliasColumnSelect = string.Empty;
            }
            else
            {
                /* FI 20210224 [XXXXX] Mise en commentaire
                 opAliasColumnSelect = pRrc.Relation[0].ColumnSelect[0].ColumnName;

                string columnSelect = SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnSelect[0].ColumnName;
                sqlQuery = SQLCst.SELECT_DISTINCT + SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnRelation[0].ColumnName + ",";

                if (ArrFunc.IsFilled(pRrc.Relation[0].ColumnLabel))
                {
                    string concat = DataHelper.GetConcatOperator(SessionTools.CS);
                    string columnLabel = SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnLabel[0].ColumnName;

                    //CC/PL 20180209 Improvement +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                    //sqlQuery += columnSelect;
                    //sqlQuery += concat + "case when (not " + columnLabel + " is null) and (" + columnLabel + "!=" + columnSelect + ") then "
                    //    + DataHelper.SQLString(" - ") + concat + columnLabel
                    //    + " else ' ' end";
                    sqlQuery += Cst.CrLf;
                    sqlQuery += "case when (" + columnLabel + " is null) then " + columnSelect + Cst.CrLf;
                    sqlQuery += "     when (" + columnSelect + " like '%' || " + columnLabel + ") then " + columnSelect + Cst.CrLf;
                    sqlQuery += "     when (" + columnLabel + " like  " + columnSelect + " || ' %') then " + columnLabel + Cst.CrLf;
                    sqlQuery += "     else " + columnSelect + " || ' - ' || " + columnLabel + " end";
                    //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+-+-+-+-+-+-+-+--+-+-+-+-

                    if (opAliasColumnSelect == pRrc.Relation[0].ColumnSelect[0].ColumnName)
                        opAliasColumnSelect += "_";
                }
                else
                {
                    sqlQuery += columnSelect;
                }
                */
                // FI 20210224 [XXXXX] use SqlColExpression Method
                sqlQuery = SQLCst.SELECT_DISTINCT + SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnRelation[0].ColumnName + ",";
                sqlQuery += SqlColExpression(SQLCst.TBLMAIN, 
                                pRrc.Relation[0].ColumnSelect[0].ColumnName, 
                                ArrFunc.IsFilled(pRrc.Relation[0].ColumnLabel) ? pRrc.Relation[0].ColumnLabel[0].ColumnName : string.Empty, 
                                out opAliasColumnSelect);
                
                sqlQuery += " as " + opAliasColumnSelect + Cst.CrLf;

                if (pIsColor)
                    sqlQuery += "," + SQLCst.TBLMAIN + ".FORECOLOR," + SQLCst.TBLMAIN + ".BACKCOLOR";
            }
            sqlQuery += SQLCst.FROM_DBO + tableNameForDDL + " " + SQLCst.TBLMAIN + Cst.CrLf;

            #region Restriction
            string tmp_SQLJoin = string.Empty;
            //
            if (pReferential.IsSQLWhereWithSessionRestrict() && (false == SessionTools.IsSessionSysAdmin))
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, false);
                //
                if (tableNameForDDL == Cst.OTCml_TBL.GINSTR.ToString())
                    tmp_SQLJoin = srh.GetSQLGroupeInstr(string.Empty, SQLCst.TBLMAIN + ".IDGINSTR");
                //
                if (tableNameForDDL == Cst.OTCml_TBL.BOOK.ToString() ||
                    tableNameForDDL == Cst.OTCml_TBL.VW_BOOK_VIEWER.ToString())
                {
                    srh.GetSQLBook(string.Empty, SQLCst.TBLMAIN + ".IDB", out tmp_SQLJoin, out string where);
                    sqlWhere.Append(where);
                }
                //
                if (tableNameForDDL == Cst.OTCml_TBL.ACTOR.ToString())
                {
                    if (false == ((pRrc.ColumnName == "IDAINS") || (pRrc.ColumnName == "IDAUPD")))
                        tmp_SQLJoin = srh.GetSQLActor(string.Empty, SQLCst.TBLMAIN + ".IDA");
                }
                //
                if (tableNameForDDL == Cst.OTCml_TBL.PRODUCT.ToString())
                {
                    if (false == (pReferential.TableName == Cst.OTCml_TBL.INSTRUMENT.ToString()))
                        tmp_SQLJoin = srh.GetSQLProduct(String.Empty, SQLCst.TBLMAIN + ".IDP");
                }
                //
                if (tableNameForDDL == Cst.OTCml_TBL.INSTRUMENT.ToString())
                    tmp_SQLJoin = srh.GetSQLInstr(string.Empty, SQLCst.TBLMAIN + ".IDI");
                //
                //FI 20100728 [17103] Add restrict sur Market
                if (tableNameForDDL == Cst.OTCml_TBL.MARKET.ToString())
                    tmp_SQLJoin = srh.GetSQLMarket(string.Empty, SQLCst.TBLMAIN + ".IDM");
                //
                sqlQuery += tmp_SQLJoin;
            }
            #endregion

            if (pRrc.Relation[0].Condition != null)
            {
                string where = string.Empty;
                string whereWithOutReplace = string.Empty; 
                for (int nbCondition = 0; nbCondition < pRrc.Relation[0].Condition.Length; nbCondition++)
                {
                    //PL 20190513 New feature: apply = "GRID"
                    //where = pRrc.Relation[0].Condition[nbCondition].SQLWhere; 
                    if ((!pRrc.Relation[0].Condition[nbCondition].applySpecified) || (pRrc.Relation[0].Condition[nbCondition].apply != "GRID"))
                    {
                        where = pRrc.Relation[0].Condition[nbCondition].SQLWhere;
                        whereWithOutReplace = where;
                    }

                    //
                    if (nbCondition == 0)
                    {
                        #region Condition[nbCondition].TableName
                        if (pRrc.Relation[0].Condition[nbCondition].TableName != null)
                        {
                            string tableName = pRrc.Relation[0].Condition[nbCondition].TableName;
                            if (tableName.IndexOf("|") > 0)
                            {
                                //Il existe une table pour les Création et une pour les Modifications (eg: TBLINS|TBLUPD)
                                string[] aTableName = tableName.Split("|".ToCharArray());
                                tableName = (pIsNewRecord ? aTableName[0] : aTableName[1]);
                            }
                            string aliasTableName = tableName;
                            if ((pRrc.Relation[0].Condition[nbCondition].AliasTableNameSpecified) && (pRrc.Relation[0].Condition[nbCondition].AliasTableName != null))
                                aliasTableName = pRrc.Relation[0].Condition[nbCondition].AliasTableName;
                            //
                            sqlQuery += Cst.CrLf + SQLCst.INNERJOIN_DBO + tableName;
                            if (aliasTableName != tableName)
                                sqlQuery += " " + aliasTableName;
                            sqlQuery += " on (";
                            sqlQuery += aliasTableName + "." + pRrc.Relation[0].Condition[nbCondition].ColumnRelation[0].ColumnName;
                            sqlQuery += " = ";
                            sqlQuery += SQLCst.TBLMAIN + "." + pRrc.Relation[0].ColumnRelation[0].ColumnName;
                            sqlQuery += ")";
                        }
                        #endregion
                    }
                    if (pIsScalar)
                    {
                        //On est ici sur le test d'existence des référentiels enfants, on utilise donc la PK
                        where = string.Empty;
                    }
                    else
                    //On est ici sur le référentiel lui même , on utilise donc la FK
                    {
                        //20081008 PL Newness valueForeignKey
                        string valueForeignKey = (StrFunc.IsFilled(pValueForeignKeyField) ? pValueForeignKeyField : SQLCst.NULL);
                        where = where.Replace(Cst.FOREIGN_KEY, valueForeignKey);
                    }
                    //
                    where = where.Replace(Cst.MODE_NEWRECORD, (pIsNewRecord ? "1" : "0"));
                    //
                    #region Condition[nbCondition].ColumnName
                    //20060424 PL GLOPTEST
                    bool isColumnName = pRrc.Relation[0].Condition[nbCondition].ColumnNameSpecified;
                    bool isColumnValue = where.IndexOf(Cst.COLUMN_VALUE) > 0;
                    
                    while (isColumnName || isColumnValue)
                    {
                        string currentvalue = string.Empty;
                        string aliasTableName = null;
                        string constantID;
                        string clientID;
                        if (isColumnName)
                        {
                            constantID = Cst.COLUMN_VALUE;
                            clientID = pRrc.Relation[0].Condition[nbCondition].ColumnName;
                        }
                        else
                        {
                            int startConstant = where.IndexOf(Cst.COLUMN_VALUE);
                            int endConstant = startConstant + Cst.COLUMN_VALUE.Length;
                            int posDblPercent = where.IndexOf("%%", endConstant);
                            clientID = where.Substring(endConstant, posDblPercent - endConstant);
                            constantID = Cst.COLUMN_VALUE + clientID + "%%";
                            //20090305 PL Si un "." est présent on a alors: aliasTableName.columnName
                            if (clientID.IndexOf(".") > 0)
                            {
                                int posDot = clientID.IndexOf(".");
                                aliasTableName = clientID.Substring(0, posDot);
                                clientID = clientID.Substring(posDot + 1);
                            }
                        }
                        //
                        ReferentialsReferentialColumn rrc = null;
                        try
                        {
                            string suffix_ClientID = string.Empty;
                            rrc = pReferential[clientID, aliasTableName];
                            if (rrc.IsAdditionalData)
                                suffix_ClientID = rrc.ExternalFieldID.ToString();
                            //
                            bool isEnabled = ((!rrc.IsForeignKeyField) || (StrFunc.IsEmpty(pReferential.ValueForeignKeyField)));
                            bool isFK = !isEnabled;

                            //PL 20120117 Add test on !IsTypeBool
                            if ((pReferential.isLookReadOnly && !TypeData.IsTypeBool(rrc.DataType.value)) || isFK)
                            {
                                //FI 20120305 Add cas TextBox
                                //En mode ReadOnly ControlMain n'est pas nécessairement une Table 
                                //C'est une table uniquement sur les données chargé via DDL ds le formulaire
                                //
                                bool isOk = false;
                                if (isOk == false)
                                {
                                    if (rrc.ControlMain is Table table)
                                    {
                                        isOk = true;
                                        TableRow tr = table.Rows[0];
                                        // EG 20130520 Test AutoComplete
                                        //TableCell td = tr.Cells[1];
                                        //currentvalue = td.Text;
                                        TableCell td = null;
                                        if (rrc.AutoCompleteRelationEnabled)
                                        {
                                            td = tr.Cells[0];
                                            HiddenField hdn = (HiddenField)td.Controls[1];
                                            currentvalue = hdn.Value;
                                        }
                                        else
                                        {
                                            td = tr.Cells[1] as TableCell;
                                            if (null != td)
                                                currentvalue = td.Text;
                                        }
                                    }
                                }
                                //
                                if (isOk == false)
                                {
                                    if (rrc.ControlMain is TextBox txt)
                                    {
                                        isOk = true;
                                        currentvalue = txt.Text;
                                    }
                                }

                            }
                            else if (pPage.IsPostBack)
                            {
                                #region IsPostBack
                                //Get value from Page
                                if (TypeData.IsTypeBool(rrc.DataType.value))
                                {
                                    clientID = Cst.CHK + clientID + suffix_ClientID;
                                }
                                else if (ReferentialTools.IsDataForDDL(rrc))
                                {
                                    clientID = Cst.DDL + clientID + suffix_ClientID;
                                }
                                else
                                {
                                    //PL 20130516 Test
                                    if (rrc.ExistsRelation && rrc.AutoCompleteRelationEnabled)
                                    {
                                        clientID = Cst.HDN + clientID + suffix_ClientID;
                                    }
                                    else
                                    {
                                        clientID = Cst.TXT + clientID + suffix_ClientID;
                                    }
                                }
                                // EG 20150923 Test null
                                if (null != pPage.Request.Form[clientID])
                                {
                                    currentvalue = pPage.Request.Form[clientID].ToString();
                                    if (TypeData.IsTypeBool(rrc.DataType.value))
                                        currentvalue = DataHelper.SQLBoolean(BoolFunc.IsTrue(currentvalue));
                                }
                                #endregion IsPostBack
                            }
                            else
                            {
                                //Get value from Referential
                                if (TypeData.IsTypeBool(rrc.DataType.value))
                                {
                                    currentvalue = DataHelper.SQLBoolean(((CheckBox)rrc.ControlMain).Checked);
                                }
                                else if (ReferentialTools.IsDataForDDL(rrc))
                                {
                                    currentvalue = ((DropDownList)rrc.ControlMain).SelectedValue;
                                }
                                else
                                {
                                    //PL 20130516 Test
                                    if (rrc.ExistsRelation && rrc.AutoCompleteRelationEnabled)
                                    {
                                        Table tbl = (Table)rrc.ControlMain;
                                        TableRow tr = tbl.Rows[0];
                                        TableCell tc = tr.Cells[0];
                                        HiddenField hdn = (HiddenField)tc.Controls[1];
                                        currentvalue = hdn.Value;
                                    }
                                    else
                                    {
                                        currentvalue = ((TextBox)rrc.ControlMain).Text;
                                    }
                                }
                            }
                        }
                        catch
                        {
                            #region catch
                            try
                            {
                                //20080829 PL En mode "Duplication" on est en "IsPostBack" mais étrangement Request.Form est vide...
                                //            On recherche donc dans la calsse "rrc"
                                if (pPage.IsPostBack)
                                {
                                    //Get value from Referential
                                    if (TypeData.IsTypeBool(rrc.DataType.value))
                                        currentvalue = DataHelper.SQLBoolean(((CheckBox)rrc.ControlMain).Checked);
                                    else if (ReferentialTools.IsDataForDDL(rrc))
                                        currentvalue = ((DropDownList)rrc.ControlMain).SelectedValue;
                                    else
                                        currentvalue = ((TextBox)rrc.ControlMain).Text;
                                }
                            }
                            catch
                            {
                                currentvalue = string.Empty;
                            }
                            #endregion catch
                        }
                        //20080922 PL Add if()
                        if ((currentvalue.Length == 0) && (TypeData.IsTypeNumeric(rrc.DataType.value) || TypeData.IsTypeBool(rrc.DataType.value)))
                            currentvalue = SQLCst.NULL;
                        
                        where = where.Replace(constantID, currentvalue);
                        //
                        if (isColumnName)
                            isColumnName = false;
                        isColumnValue = where.IndexOf(Cst.COLUMN_VALUE) > 0;
                    }
                    #endregion
                    //
                    sqlWhere.Append(where);
                    sqlWhereWithoutReplace.Append(whereWithOutReplace);
                }
            }
            string sql_Select = sqlQuery + sqlWhere.ToString(true);
            opSqlWhereWithoutReplace = sqlWhereWithoutReplace.ToString();
            
            Debug.WriteLine(sql_Select);
            return sql_Select;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="column"></param>
        /// <param name="columnDisplay"></param>
        /// <param name="aliasColumn"></param>
        /// <returns></returns>
        /// FI 20210224 [XXXXX] Add
        public static string SqlColExpression(string alias, string column, string columnDisplay, out string aliasColumn)
        {
            string ret = string.Empty;

            string column2 = (StrFunc.IsFilled(alias) ? $"{alias}.{column}" : column);
            if (StrFunc.IsFilled(columnDisplay))
            {
                string columnDisplay2 = (StrFunc.IsFilled(alias) ? $"{alias}.{columnDisplay}" : columnDisplay);

                //CC/PL 20180209 Improvement +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //sqlQuery += columnSelect;
                //sqlQuery += concat + "case when (not " + columnLabel + " is null) and (" + columnLabel + "!=" + columnSelect + ") then "
                //    + DataHelper.SQLString(" - ") + concat + columnLabel
                //    + " else ' ' end";
                ret += Cst.CrLf;
                ret += $@"case when ({columnDisplay2} is null) then {column2}
                        when ({column2} like '%' || {columnDisplay2}) then {column2}
                        when ({columnDisplay2} like  {column2} || ' %') then {columnDisplay2}
                        else {column2} || ' - ' || {columnDisplay2} end";
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+-+-+-+-+-+-+-+-+--+-+-+-+-

                aliasColumn = $"{column}_";
            }
            else
            {
                ret += column2;
                aliasColumn = column;
            }
            return ret;
        }

        /// <summary>
        /// Execute la commande SQLPreSelect définie dans referential
        /// <remarks>Remplace %%DA:PK%% et %%DA:FK%% pouvant exister ds la commande</remarks>
        /// <remarks>Remplace @PK et @FK pouvant exister ds la commande</remarks>
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pValueDataKeyField"></param>
        /// FI 20191227 [XXXXX] Methode Public
        /// FI 20201215 [XXXXX] Suppression du paramètre pValueForeigKeyField
        public static void ExecutePreSelect(ReferentialsReferential pReferential, string pValueDataKeyField)
        {
            // FI 20191227 [XXXXX] Appel à la méthode PreparePreSelect
            QueryParameters[] queryParameters = PreparePreSelect(pReferential, pValueDataKeyField);

            string cs = ReferentialTools.AlterConnectionString(SessionTools.CS, pReferential);
            if (ArrFunc.IsFilled(queryParameters))
            {
                for (int i = 0; i < ArrFunc.Count(queryParameters); i++)
                {
                    DataHelper.ExecuteNonQuery(cs, CommandType.Text, queryParameters[i].Query, queryParameters[i].Parameters.GetArrayDbParameter());
                }
            }
        }





        /// <summary>
        /// Désérialisation d'une chaine en StringDynamicData avec encodage et décodage du "& commercial"
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 201300626 New
        public static StringDynamicData DeserializeDA(string pData)
        {
            string _data = pData;
            string etcom = HttpUtility.UrlEncode("&", System.Text.Encoding.Default);
            _data = _data.Replace("&", etcom);
            EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), _data);
            StringDynamicData _dd = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
            if (StrFunc.IsFilled(_dd.value))
                _dd.value = _dd.value.Replace(etcom, "&");
            return _dd;

        }
        /// <summary>
        /// Sérialisation d'un StringDynamicData en string avec encodage et décodage du "& commercial"
        /// </summary>
        /// <param name="pData"></param>
        /// <returns></returns>
        /// EG 201300626 New
        public static string SerializeDA(StringDynamicData pDD)
        {
            string etcom = HttpUtility.UrlEncode("&", System.Text.Encoding.Default);
            bool isValueSpecified = StrFunc.IsFilled(pDD.value);
            if (isValueSpecified)
                pDD.value = pDD.value.Replace("&", etcom);
            string _data = pDD.Serialize();
            if (isValueSpecified)
                pDD.value = pDD.value.Replace(etcom, "&");
            return _data;
        }

        /// <summary>
        /// Retourne les DynamicArgument formattés à partir d'un paramètre {pDA} issu d'une requête Http 
        /// <para>Chaque item du dictionnaire  est au format par serialization d'un StringDynamicData</para>
        /// <para>Retourne null si {pDA} est vide</para>
        /// </summary>
        /// <param name="pDA"></param>
        /// <returns></returns>
        /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
        /// FI 20200205 [XXXXX] Retourne désormais un Dictionary&lt;string, ReferentialsReferentialStringDynamicData&gt;
        public static Dictionary<string, ReferentialsReferentialStringDynamicData> CalcDynamicArgumentFromHttpParameter2(string pDA)
        {
            Dictionary<string, ReferentialsReferentialStringDynamicData> ret = null;

            if (StrFunc.IsFilled(pDA))
            {
                string[] arrayDA = StrFunc.StringArrayList.StringListToStringArray(pDA);
                if (ArrFunc.IsFilled(arrayDA))
                {
                    ret = new Dictionary<string, ReferentialsReferentialStringDynamicData>();
                    for (int i = 0; i < arrayDA.Length; i++)
                    {

                        try
                        {
                            StringDynamicData sDD = DeserializeDA(arrayDA[i]);

                            ret.Add(sDD.name,
                                new ReferentialsReferentialStringDynamicData(sDD.datatype, sDD.name, sDD.value)
                                {
                                    source = DynamicDataSourceEnum.URL
                                });

                        }
                        catch
                        {
                            string name = "DATA_" + i.ToString();
                            ret.Add(name,
                                new ReferentialsReferentialStringDynamicData("string", name, arrayDA[i])
                                {
                                    source = DynamicDataSourceEnum.URL
                                });
                        }
                    }
                }
            }
            return ret;
        }



        /// <summary>
        /// Retournne l'URL qui ouvre le formulaire de saisie/consultation d'un référentiel
        /// </summary>
        /// <param name="pType">Valeurs possibles (Referential,Cconsultation,Log,etc...)</param>
        /// <param name="pObject">Nom du référentiel</param>
        /// <param name="pMode"></param>
        /// <param name="pNew"></param>
        /// <param name="pCondApp"></param>
        /// <param name="pParam"></param>
        /// <param name="pDynamicArg"></param>
        /// <param name="pFormId">Représente le formulaire appelant</param>
        /// <param name="pDS">Représente l'identifiant du datasetCourant (permet la navigation vers les autres lignes à partir du formulaire)</param>
        /// <param name="pPK">Représente la colonne PK</param>
        /// <param name="pPKValue">Repésente la valeur de la PK</param>
        /// <param name="pFK">Représente la colonne FK (utilisé pour les référentiels enfants)</param>
        /// <param name="pFKValue">Représente la colonne FK (utilisé pour les référentiels enfants)</param>
        /// <param name="pIdMenu">Représente le menu associé au référentiel</param>
        /// <param name="pTitleMenu"></param>
        /// <returns></returns>
        public static string GetURLOpenReferential(string pType, string pObject, Cst.ConsultationMode pMode, bool pNew,
            string pCondApp, string[] pParam, string[] pDynamicArg,
            string pPK, string pPKValue, string pFK, string pFKValue,
            string pIdMenu, string pTitleMenu)
        {
            //
            OpenReferentialFormArguments arg = new OpenReferentialFormArguments
            {
                Type = pType,
                @Object = pObject,
                Mode = pMode,
                IsNewRecord = pNew,
                CondApp = pCondApp,
                Param = pParam,
                DynamicArg = pDynamicArg,
                PK = pPK,
                PKValue = pPKValue,
                FK = pFK,
                FKValue = pFKValue,
                IdMenu = pIdMenu,
                TitleMenu = pTitleMenu
            };
            return arg.GetURLOpenFormReferential();
        }

        /// <summary>
        /// Retourne les commandes SQL rattachées à l'identifiant {pName}
        /// <para>Ouverture de la ressource incorporée pour récupérer les ordres SQL</para>
        /// </summary>
        /// <param name="pName"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException si la ressource incorporée qui contient la commande est non chargée"></exception>
        public static ReferentialsReferentialSQLSelects LoadReferentialsReferentialSQLSelect(string pName)
        {
            //
            string streamName = Assembly.GetExecutingAssembly().GetName().Name + ".XML_Files." + "sqlCommand.xml";
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(streamName);

            //FI 20120516 En mode Debug lecture du fichier
#if DEBUG
            //En debug Spheres® cherche à lire le fichier xml sui se trouve Sous Referentiel\XML_Files
            //Cela permet de ne pas compiler à chaque modification du fichier sqlCommand
            string path = HttpContext.Current.Request.PhysicalApplicationPath;
            if (path.EndsWith(@"\"))
                path = path.Substring(0, path.Length - 1);
            try
            {
                DirectoryInfo dirInfo = Directory.GetParent(path);
                path = dirInfo.FullName + @"\Referentiel\XML_Files\sqlCommand.xml";
                stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch (DirectoryNotFoundException)
            {
                //Spheres® abandonne

            }
#endif

            if (null == stream)
                throw new NullReferenceException("SqlCommand.xml Ressource not found");

            StreamReader reader = new StreamReader(stream);
            string xml = reader.ReadToEnd();
            reader.Close();
            //
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);
            //
            string xPath = StrFunc.AppendFormat(@"sqlCommands/item[@name='{0}']", pName);
            //
            XmlNodeList list = xmlDoc.SelectNodes(xPath);
            if (false == (list.Count > 0))
                throw new Exception(StrFunc.AppendFormat("tag sqlCommands/item['{0}'] not Found", pName));
            //
            string xmlItem = StrFunc.AppendFormat(@"<sqlCommands>{0}</sqlCommands>", list.Item(0).InnerXml);
            //
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(ReferentialsReferentialSQLSelects), xmlItem);
            ReferentialsReferentialSQLSelects ret = (ReferentialsReferentialSQLSelects)CacheSerializer.Deserialize(serializeInfo);
            //
            return ret;
        }

        /// <summary>
        /// Retourne true lorsque l'utilisateur a les droits de créer, modifier et supprimer sur le référentiel associé au menu {pIdMenu}
        /// <para>Cette méthode ne tient pas compte des drois directement définis sur le référentiel</para>
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        /// FI 20120116 Un admin n'a pas nécessairement les droits Create,Modify,Delete etc.. (c'est le cas lorsque la Database est de type READONLY)
        public static bool IsAllPermissionEnabled(string pCS, string pIdMenu, User pUser)
        {
            bool ret = true;
            if (StrFunc.IsFilled(pIdMenu))
            {
                if ((pIdMenu == "ATTACHEDDOC") || (pIdMenu == "ATTACHEDDOCS"))
                {
                    //Note: 
                    //Il n'existe pas pour l'instant de gestion des permissions sur l'accès aux pièces jointes
                    //Cela peut être gênant, car un user qui n'aurait pas le droit de modifié/supprimé un item 
                    //pourra malgré tout ajouter/supprimer une pièce jointe (cf. TRIM n°3230)
                }
                else
                {
                    RestrictionPermission restrictPermission = new RestrictionPermission(pIdMenu, pUser);
                    restrictPermission.Initialize(pCS);
                    //
                    if (!restrictPermission.IsCreate)
                        ret = false;
                    if (!restrictPermission.IsModify)
                        ret = false;
                    if (!restrictPermission.IsRemove)
                        ret = false;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne un dataset alimenté avec l'enregistrement ouvert par le formulaire
        /// <para>Le dataset est vide en mode pIsNewRecord</para>
        /// </summary>
        /// <param name="pPage">page qui contient le formuliare</param>
        /// <param name="pReferential"></param>
        /// <param name="pIsNewRecord">true si ajout d'un enregistrement</param>
        /// <param name="pConsultationMode"></param>
        /// <param name="pDataKeyField">Colonne KeyField</param>
        /// <param name="pValueDataKeyField">valeur de la colonne KeyField</param>
        /// FI 20201215 [XXXXX] Suppression de pValueForeigKeyField 
        private static DataSet GetDsDataForm(PageBaseReferentialv2 pPage,
            ReferentialsReferential pReferential,
            bool pIsNewRecord, Cst.ConsultationMode pConsultationMode,
            string pDataKeyField, string pValueDataKeyField)
        {
            //FI 20110704  
            //Dans le cas d'un appel direct au formulaire (sans passer par l'étape du DataGrid), Spheres joue le script contenu dans la PreSelectCommand
            //Ce cas se produit lorsque l'utilisateur ouvre un acteur depuis le masque de saisie
            //FI 2011117 
            //En mode isOpenFromGrid, si le dataset du grid associé est non chargé Spheres® lance le SQLPreSelect. 
            //Cet dernier doit absolument être joué avant de charger le dataset  
            bool isPreCommand = true; // par défaut
            if ((pPage.IsOpenFromGrid) & (pPage.IsDatasetGridFilled))
                isPreCommand = false;
            //                    
            if (isPreCommand)
                ExecutePreSelect(pReferential, pValueDataKeyField);
            //
            //Dans le cas d'un appel direct au formulaire (sans passer par l'étape du DataGrid), le DataSet est null
            //Spheres® le recharge ici
            string column = pDataKeyField;
            string value = pValueDataKeyField;
            if (pIsNewRecord && StrFunc.IsEmpty(column))
            {
                //20070529 PL Astuce pour que l'alimentation du Dataset ne retourne aucune ligne en mode "création" (cf. GetSQLSelect() )
                column = "'TIP'";
                value = "'NewRecord'";
            }

            _ = (pConsultationMode == Cst.ConsultationMode.Select);
            //
            // FI 20201215 [XXXXX] Usage de pReferential.ValueForeignKeyField
            // FI 20220113 [XXXXX] SetCacheOn si pIsNewRecord
            DataSet ds = SQLReferentialData.RetrieveDataFromSQLTable(pIsNewRecord? CSTools.SetCacheOn(SessionTools.CS) : SessionTools.CS,
                                    pReferential, column, value, true, false, pReferential.ValueForeignKeyField);
            //
            //20060524 PL Add: CaseSensitive = true pour Oracle et la gestion du ROWBERSION (ROWID) 
            ds.Tables[0].CaseSensitive = true;
            //
            return ds;
        }

        /// <summary>
        /// Suppression des données présentes dans le cache dépendantes ds la table {pTable}
        /// </summary>
        /// <param name="pTable"></param>
        public static void RemoveCache(string pTable)
        {
            //Cache WEB
            if (Enum.IsDefined(typeof(Cst.OTCml_TBL), pTable))
            {
                Cst.OTCml_TBL tableEnum = (Cst.OTCml_TBL)Enum.Parse(typeof(Cst.OTCml_TBL), pTable);
                /* FI 20240731[XXXXX] Mise en commentaire => use DataEnabledEnum / DataEnabledEnumHelper
                switch (tableEnum)
                {
                    case Cst.OTCml_TBL.ENUM:
                    case Cst.OTCml_TBL.ENUMS:
                    case Cst.OTCml_TBL.EVENTENUMS:
                    case Cst.OTCml_TBL.EVENTENUM:
                    case Cst.OTCml_TBL.STCHECK:
                    case Cst.OTCml_TBL.STMATCH:
                        ExtendEnumsTools.ResetFpMLEnumsAndSchemes();
                        ExtendEnumsTools.LoadFpMLEnumsAndSchemes(SessionTools.CS);
                        break;
                }*/

                // FI 20220613 [XXXXX] add
                DataEnabledHelper.ClearCache(SessionTools.CS, tableEnum);

            }
            //Cache SQL
            DataHelper.queryCache.Remove(pTable, SessionTools.CS);
        }

        /// <summary>
        /// Retourne le chemin du fichier xml descriptif  pour afficher {pObjectName}
        /// </summary>
        /// <param name="pObjectType"></param>
        /// <param name="pObjectName"></param>
        /// FI 20160804 [Migration TFS] Modify
        public static string GetObjectXMLFile(Cst.ListType pObjectType, string pObjectName)
        {
            //FI 20160804 [Migration TFS] Call SessionTools.NewAppInstance().SearchFile (cette méthode effectue une lecture de FileConfig)
            //return GetXMLFile(pObjectType.ToString(), pObjectName);
            string searchFileName = string.Empty;
            SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.XML", pObjectType.ToString(), pObjectName), ref searchFileName);
            return searchFileName;
        }

        /// <summary>
        /// Retourne un titre titre specifique pour un menu en fonction de l'object
        /// </summary>
        /// <param name="pObjectName"></param>
        /// <returns></returns>
        public static string ParticularTitle(string pIdMenu, string pObjectName)
        {
            string ret = string.Empty;
            //
            Nullable<IdMenu.Menu> idMenu = IdMenu.ConvertToMenu(pIdMenu);
            if (idMenu.HasValue && idMenu.Value == IdMenu.Menu.IOTRACKCOMPARE)
            {
                if (StrFunc.ContainsIn(pObjectName, "IOTRACKCOMPARE_"))
                {
                    string shortName = pObjectName.Replace("IOTRACKCOMPARE_", string.Empty);
                    //
                    Nullable<EFS.SpheresIO.CompareOptions> compareOption = CompareOptionsAttribute.Parse(shortName, true);
                    if (false == compareOption.HasValue)
                        compareOption = (EFS.SpheresIO.CompareOptions)Enum.Parse(typeof(EFS.SpheresIO.CompareOptions), shortName, true);
                    //
                    ret = Ressource.GetString(StrFunc.AppendFormat("CompareOptions_{0}", compareOption.Value.ToString()), compareOption.Value.ToString());
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne la liste des objects pouvait être utilisé pour déserializer la consultation 3 pts
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <param name="pObjectName"></param>
        /// <returns></returns>
        public static List<string> GetObjectNameForDeserialize(string pIdMenu, string pObjectName)
        {
            List<String> lstObjectName = new List<String>
            {
                pObjectName
            };
            //
            Nullable<IdMenu.Menu> idMenu = IdMenu.ConvertToMenu(pIdMenu);
            if (idMenu.HasValue)
            {
                switch (idMenu.Value)
                {
                    case IdMenu.Menu.IOTRACKCOMPARE:
                        if (pObjectName.Contains("IOTRACKCOMPARE_"))
                        {
                            string compareType = pObjectName.Replace("IOTRACKCOMPARE_", string.Empty);
                            Nullable<EFS.SpheresIO.CompareOptions> compareOptions = EFS.SpheresIO.CompareOptionsAttribute.Parse(compareType, true);
                            if (compareOptions.HasValue)
                            {
                                switch (compareOptions.Value)
                                {
                                    case EFS.SpheresIO.CompareOptions.Trades:

                                        lstObjectName.Add("IOTRACKCOMPARE_TRADE");

                                        break;

                                    case EFS.SpheresIO.CompareOptions.TradesInPosition:
                                    case EFS.SpheresIO.CompareOptions.Positions:

                                        //MF 20120402 - ticket 9928 , résolution : interface IOTRACKCOMPARE_POS additionelle position pour Vision
                                        if (Software.IsSoftwareVision())
                                        {
                                            lstObjectName.Add("IOTRACKCOMPARE_POS");
                                        }
                                        else
                                        {
                                            lstObjectName.Add("IOTRACKCOMPARE_TRADE");
                                        }

                                        break;

                                    case EFS.SpheresIO.CompareOptions.CashFlows:

                                        lstObjectName.Add("IOTRACKCOMPARE_AMT");

                                        break;

                                    case EFS.SpheresIO.CompareOptions.CashFlowsInstr:

                                        lstObjectName.Add("IOTRACKCOMPARE_AMI");

                                        break;
                                }
                            }
                        }
                        lstObjectName.Add("IOTRACKCOMPARE");
                        break;
                    default:
                        break;
                }
            }
            //
            return lstObjectName;
        }


        /// <summary>
        /// Mise en place link sur le control {pControl} qui ouvre un formulaire qui permet de consulter la donnée en rapport avec {pRrc} 
        /// </summary>
        /// <param name="pPage">Page qui contient le control</param>
        /// <param name="pControl"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRrc">Caractéristiques de la colonne</param>
        /// <param name="pData">Donnée à consulter</param>
        /// <param name="pMode"></param>
        /// FI 20140911 [XXXXX] add Method
        /// EG 20170125 [Refactoring URL] Upd 
        /// FI 20180216 [XXXXX] Add hyperlink sur les fees
        /// EG 20210412 [XXXXX] Construction du lien hyperlink si value = true
        private static void SetControlHyperLinkColumn(PageBase pPage, WebControl pControl, ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc, string pData, Cst.ConsultationMode pMode)
        {
            if (IsHyperLinkAvailable(pRrc))
            {
                // EG 20170125 [Refactoring URL] New
                Nullable<IdMenu.Menu> idMenu = null;
                if (pPage.Form != null)
                    _ = pPage.Form.ID;
                if (pRrc.ExistsHyperLinkColumn && pRrc.IsHyperLink.Value)
                {
                    _ = pRrc.IsHyperLink.type;
                    string colNameId = pRrc.IsHyperLink.data;
                    if (IsHyperLinkColumn(colNameId))
                    {
                        int indexColId = pReferential.GetIndexColSQL(colNameId);
                        if (-1 == indexColId)
                            throw new Exception(StrFunc.AppendFormat("Column [name:{0}] doesn't exist", colNameId));

                        string value = pReferential.dataRow[indexColId].ToString();
                        // EG 20170125 [Refactoring URL] Upd
                        //url = SpheresURL.GetURLHyperLink(new Pair<string, Nullable<Cst.OTCml_TBL>>(colNameId, null), value, pMode, formId);
                        idMenu = SpheresURL.GetMenu_Repository(colNameId, value);
                    }
                }
                else if (pRrc.ExistsRelation)
                {
                    // FI 20180216 [XXXXX] call GetHyperLinkColumnNameFromRelation
                    string columName = GetHyperLinkColumnNameFromRelation(pReferential, pRrc, pReferential.dataRow);
                    if (StrFunc.IsFilled(columName) && IsHyperLinkColumn(columName))
                    {
                        // EG 20170125 [Refactoring URL] Upd
                        //Pair<string, Nullable<Cst.OTCml_TBL>> column = new Pair<string, Nullable<Cst.OTCml_TBL>>(columName, null);
                        //if ((columName == "IDASSET") && (pReferential.dataRow.Table.Columns.Contains("ASSETCATEGORY")))
                        //{
                        //    Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), pReferential.dataRow["ASSETCATEGORY"].ToString());
                        //    Cst.OTCml_TBL table = EfsML.AssetTools.ConvertUnderlyingAssetToTBL(assetCategory);
                        //    column.Second = table;
                        //}
                        //url = SpheresURL.GetURLHyperLink(column, pData, pMode, formId);

                        if ((columName == "IDASSET") && (pReferential.dataRow.Table.Columns.Contains("ASSETCATEGORY")))
                        {
                            Cst.UnderlyingAsset assetCategory = (Cst.UnderlyingAsset)Enum.Parse(typeof(Cst.UnderlyingAsset), pReferential.dataRow["ASSETCATEGORY"].ToString());
                            idMenu = SpheresURL.GetMenu_Asset(columName, null, assetCategory);
                        }
                        else
                        {
                            idMenu = SpheresURL.GetMenu_Repository(columName, pData);
                        }
                    }
                }
                else if (IsHyperLinkColumn(pRrc.ColumnName))
                {
                    // EG 20170125 [Refactoring URL] Upd
                    //url = SpheresURL.GetURLHyperLink(new Pair<string, Nullable<Cst.OTCml_TBL>>(pRrc.ColumnName, null), pData, pMode, formId);
                    idMenu = SpheresURL.GetMenu_Repository(pRrc.ColumnName, pData);
                }

                // EG 20170125 [Refactoring URL] Upd
                //if (StrFunc.IsFilled(url))
                //    ControlsTools.SetHyperLinkOpenFormReferential(pControl, url);
                if (idMenu.HasValue)
                {
                    string url = SpheresURL.GetURL(idMenu, pData, pMode);
                    if (StrFunc.IsFilled(url))
                        ControlsTools.SetHyperLinkOpenFormReferential(pControl, url);

                }
            }
        }


        /// <summary>
        /// Retourne true si Spheres® est en mesure d'ouvrir un lien hypertext vers le référentiel associé à la colonne {pColumnName}
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        /// FI 20170908 [23409] Modify
        /// FI 20170908 [23409] Modify
        public static bool IsHyperLinkColumn(string pColumnName)
        {
            bool ret =
                       (pColumnName == "IDA")
                    || (pColumnName == "IDGACTOR")
                    || (pColumnName == "IDB")
                    || (pColumnName == "IDGBOOK")
                    || (pColumnName == "IDI")
                    || (pColumnName == "IDFEE")
                    || (pColumnName == "IDGINSTR")
                    || (pColumnName == "IDP")
                    || (pColumnName == "IDBC")
                    || (pColumnName == "IDRX")
                    || (pColumnName == "IDM")
                    || (pColumnName == "IDDC")
                    || (pColumnName == "IDDERIVATIVEATTRIB") // FI 20170306 [22225] add
                    || (pColumnName == "IDASSET")
                    || (pColumnName == "IDASSET_UNL")
                    || (pColumnName == "IDASSET_ETD")
                    || (pColumnName == "IDASSET_EQUITY")
                    || (pColumnName == "IDASSET_FXRATE")
                    || (pColumnName == "IDASSET_COMMODITY")
                    || (pColumnName == "IDASSET_INDEX")
                    || (pColumnName == "IDASSET_RATEINDEX")
                    || (pColumnName == "IDASSET_EXTRDFUND")
                    || (pColumnName == "IDMATURITYRULE")
                    || (pColumnName == "IDCAISSUE")
                    || (pColumnName == "IDT")
                    || (pColumnName == "IDT_ADMIN")
                    || (pColumnName == "IDT_RISK")
                    || (pColumnName == "IDFEEMATRIX")
                    || (pColumnName == "IDFEESCHEDULE")
                    || (pColumnName == "IDGCONTRACT") // FI 20170908 [23409] 
                    || (pColumnName == "IDGMARKET")
                    || (pColumnName == "IDCNFMESSAGE")
                    || (pColumnName == "IDSTLMESSAGE")
                    || (pColumnName == "IDIOTASK")
                    || (pColumnName == "IDIOTASKDET")
                    || (pColumnName == "IDIOELEMENT")
                    || (pColumnName == "IDIOINPUT")
                    || (pColumnName == "IDIOOUTPUT")
                    || (pColumnName == "IDIOPARSING")
                    || (pColumnName == "IDIOPARAM")
                    || (pColumnName == "IDIOSHELL")
                    || (pColumnName == "IDCONTRACT")
                    || (pColumnName == "IDCC")
                    || (pColumnName == "IDXC") // FI 20170908 [23409] Add
                    || (pColumnName == "IDMODELACTOR")
                    || (pColumnName == "IDMODELINSTRUMENT")
                    || (pColumnName == "IDMODELMARKET")
                    || (pColumnName == "IDMODELMENU")
                    || (pColumnName == "IDMODELPERMISSION")
                    || (pColumnName == "IDDEFINEEXTEND")
                    || (pColumnName == "IDMODELSAFETY")
                    || (pColumnName == "ID_");
            return ret;
        }

        /// <summary>
        /// Retourne true si les caractéristiques de la colonne {pRrc} permettent à Spheres de produire un URL Hyperlink qui permet l'ouverture d'un formulaire de consultation
        /// </summary>
        /// <param name="pRrc"></param>
        /// <returns></returns>
        // EG 20210412 [XXXXX] Hyperlink available : Ajout test sur value = true
        public static Boolean IsHyperLinkAvailable(ReferentialsReferentialColumn pRrc)
        {
            Boolean ret = false;

            if ((pRrc.ExistsHyperLinkDocument) || (pRrc.ExistsHyperLinkColumn && pRrc.IsHyperLink.Value) || (pRrc.ExistsHyperLinkExternal))
            {
                ret = true;
            }
            else if (pRrc.ExistsRelation)
            {
                if (IsHyperLinkColumn(pRrc.Relation[0].ColumnRelation[0].ColumnName))
                    ret = true;
            }
            else if (pRrc.ColumnName == "URL")
            {
                ret = true;
            }

            return ret;
        }

        /// <summary>
        /// Retourne le nom de colonne utilisé pour piloter les hyperLinks
        /// </summary>
        /// <param name="pRRCR"></param>
        /// <returns></returns>
        /// FI 20140812 [XXXXX] Add
        private static string GetHyperLinkColumnName(ReferentialsReferentialColumnRelation pRRCR)
        {
            string ret = string.Empty;
            if (ArrFunc.IsFilled(pRRCR.ColumnRelation))
            {
                ret = pRRCR.ColumnRelation[0].ColumnName;
                if (ret == "IDASSET")
                {
                    switch (pRRCR.TableName)
                    {
                        case "ASSET_ETD":
                        case "ASSET_INDEX":
                        case "ASSET_RATEINDEX":
                        case "ASSET_COMMODITY":
                        case "ASSET_COMMODITYCONTRACT":
                        case "ASSET_EQUITY":
                        case "ASSET_EXTRDFUND":
                            ret = "ID" + pRRCR.TableName;
                            break;
                        default:
                            break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le nom de colonne utilisé pour piloter les hyperLinks lorsque la colonne relation est nommée "ID_"
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pRRC">colonne qui contient nécessairement une relation nommée "ID_"</param>
        /// <param name="pDataRow"></param>
        /// <returns></returns>
        /// FI 20180216 [XXXXX] Add
        private static string GetHyperLinkColumnNameID_(ReferentialsReferential pReferential, ReferentialsReferentialColumn pRRC, DataRow pDataRow)
        {
            string columnName = string.Empty;
            int indexCol;
            switch (pRRC.ColumnName)
            {
                case "IDCONTRACT":
                case "IDCONTRACTEXCEPT":
                    string colTypeContract = "TYPECONTRACT";
                    if (pRRC.ColumnName == "IDCONTRACTEXCEPT")
                        colTypeContract = "TYPECONTRACTEXCEPT";
                    indexCol = pReferential.GetIndexColSQL(colTypeContract);
                    if (indexCol > 0)
                    {
                        string value = pDataRow.ItemArray[indexCol].ToString();
                        switch (value)
                        {
                            case "Market":
                                columnName = "IDM";
                                break;
                            case "GrpMarket":
                                columnName = "IDGMARKET";
                                break;
                            case "DerivativeContract":
                                columnName = "IDDC";
                                break;
                            case "CommodityContract":
                                columnName = "IDCC";
                                break;
                            case "GrpContract":
                                columnName = "IDGCONTRACT";
                                break;
                        }
                    }
                    break;
                case "IDINSTR":
                case "IDINSTR_UNL":
                    string colTypeInstr = "TYPEINSTR";
                    if (pRRC.ColumnName == "IDINSTR_UNL")
                        colTypeInstr = "TYPEINSTR_UNL";

                    indexCol = pReferential.GetIndexColSQL(colTypeInstr);
                    if (indexCol > 0)
                    {
                        string value = pDataRow.ItemArray[indexCol].ToString();
                        switch (value)
                        {
                            case "Product":
                                columnName = "IDP";
                                break;
                            case "Instr":
                                columnName = "IDI";
                                break;
                            case "GrpInstr":
                                columnName = "IDGINSTR";
                                break;
                        }
                    }
                    break;
                case "IDPARTY":
                case "IDPARTYA":
                case "IDPARTYB":
                case "IDOTHERPARTY1":
                case "IDOTHERPARTY2":
                    string colTypeParty = "TYPEPARTY";
                    string colTypeParty2 = string.Empty;
                    if (pRRC.ColumnName == "IDPARTY")
                    {
                        colTypeParty = "TYPEPARTY";
                    }
                    else if (pRRC.ColumnName == "IDPARTYA")
                    {
                        colTypeParty = "TYPEPARTYA";
                        colTypeParty2 = "FEETYPEPARTYA";
                    }
                    else if (pRRC.ColumnName == "IDPARTYB")
                    {
                        colTypeParty = "TYPEPARTYB";
                        colTypeParty2 = "FEETYPEPARTYA";
                    }
                    else if (pRRC.ColumnName == "IDOTHERPARTY1")
                    {
                        colTypeParty = "TYPEOTHERPARTY1";
                        colTypeParty2 = "FEETYPEOTHERPARTY1";
                    }
                    else if (pRRC.ColumnName == "IDOTHERPARTY2")
                    {
                        colTypeParty = "TYPEOTHERPARTY2";
                        colTypeParty2 = "FEETYPEOTHERPARTY2";
                    }

                    indexCol = pReferential.GetIndexColSQL(colTypeParty);
                    if (indexCol == -1 && StrFunc.IsFilled(colTypeParty2))
                        indexCol = pReferential.GetIndexColSQL(colTypeParty2);

                    if (indexCol > 0)
                    {
                        string value = pDataRow.ItemArray[indexCol].ToString();
                        switch (value)
                        {
                            case "Actor":
                                columnName = "IDA";
                                break;
                            case "GrpActor":
                                columnName = "IDGACTOR";
                                break;
                            case "Book":
                                columnName = "IDB";
                                break;
                            case "GrpBook":
                                columnName = "IDGBOOK";
                                break;
                        }
                    }
                    break;
            }

            return columnName;
        }


        /// <summary>
        /// Retourne le nom de colonne utilisé pour piloter les hyperLinks lorsque la colonne contient une relation
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pRRC"></param>
        /// <param name="pDataRow"></param>
        /// <returns></returns>
        /// FI 20180216 [XXXXX] Add
        public static string GetHyperLinkColumnNameFromRelation(ReferentialsReferential pReferential, ReferentialsReferentialColumn pRRC, DataRow pDataRow)
        {
            if (ArrFunc.IsEmpty(pRRC.Relation))
                throw new NotSupportedException("ReferentialsReferentialColumn with Relation is expected");

            string ret;
            if (pRRC.Relation[0].ColumnRelation[0].ColumnName == "ID_")
            {
                ret = GetHyperLinkColumnNameID_(pReferential, pRRC, pDataRow);
            }
            else
            {
                ret = GetHyperLinkColumnName(pRRC.Relation[0]);
            }

            return ret;
        }

        /// <summary>
        ///  Modification de la connexion {pCs} en fonction de {pReferential} 
        ///  <para>par exemple prise en compte du timeout lorsqu'il est renseigné</para>
        /// </summary>
        /// <param name="pCs">ConectionString en entrée</param>
        /// <param name="pReferential"></param>
        /// <returns></returns>
        // FI 20170531 [23206] Add Method
        public static string AlterConnectionString(string pCs, ReferentialsReferential pReferential)
        {
            string ret = pCs;
            if (pReferential.TimeoutSpecified && (0 < pReferential.Timeout))
            {
                int timeout = CSTools.GetTimout(ret);
                timeout = Math.Max(timeout, pReferential.Timeout);
                ret = CSTools.SetTimeOut(ret, timeout);
            }
            return ret;
        }

        /// <summary>
        ///  Vérification que la donnée supprimée n'est pas utilisée
        ///  <para>Retourne true si la suppression peut être effectuée</para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pTableName"></param>
        /// <param name="pkValue"></param>
        /// <returns></returns>
        /// FI 20190718 [XXXXX] Add Method
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        public static Boolean CheckBeforDelete(PageBase pPage, string pTableName, Object pkValue)
        {
            Boolean ret = true;
            Nullable<Cst.OTCml_TBL> table = ReflectionTools.ConvertStringToEnumOrNullable<Cst.OTCml_TBL>(pTableName);

            if (null != table)
            {
                Boolean isAsset = ReflectionTools.GetEnumValues<Cst.OTCml_TBL, UnderlyingAssetAttribute>().Contains(table.Value);
                if (isAsset)
                {
                    UnderlyingAssetAttribute attrib = ReflectionTools.GetAttribute<UnderlyingAssetAttribute>(table);
                    Cst.UnderlyingAsset underlyingAsset = attrib.UnderlyingAsset;

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.ASSETCATEGORY), underlyingAsset);
                    dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDASSET), Convert.ToInt32(pkValue));

                    string qry = @"select t.IDENTIFIER
                    from dbo.TRADE t 
                    where t.ASSETCATEGORY = @ASSETCATEGORY and t.IDASSET = @IDASSET
                    order by t.IDT desc";
                    QueryParameters qryParameters = new QueryParameters(SessionTools.CS, qry, dp);


                    using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
                    {
                        if (dr.Read())
                        {
                            ret = false;
                            string tradeIdentifier = dr[0].ToString();
                            JavaScript.DialogStartUpImmediate(pPage, Ressource.GetString2("Msg_DelAssetErr", tradeIdentifier), false, ProcessStateTools.StatusErrorEnum);
                        }
                    }
                }
            }

            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pValueDataKeyField"></param>
        /// <returns></returns>
        /// FI 20191227 [XXXXX] Add
        /// FI 20201215 [XXXXX] Suppression du paramètre pValueForeigKeyField
        public static QueryParameters[] PreparePreSelect(ReferentialsReferential pReferential, string pValueDataKeyField)
        {
            QueryParameters[] ret = null;

            if (pReferential.SQLPreSelectSpecified)
            {
                ret = pReferential.GetSqlPreSelectCommand(SessionTools.CS);
                for (int i = 0; i < ArrFunc.Count(ret); i++)
                {
                    // FI 20201125 [XXXXX] call ConsultationCriteria.ReplaceConsultCriteriaKeyword
                    ret[i].Query = ConsultationCriteria.ReplaceConsultCriteriaKeyword(pReferential, ret[i].Query);

                    if (pReferential.IndexDataKeyField > -1)
                    {
                        //Replace du %%DA:PK%% pouvant exister ds la query
                        StringDynamicData sdPk = new StringDynamicData
                        {
                            name = "PK",
                            datatype = pReferential[pReferential.IndexDataKeyField].DataType.value
                        };
                        if (StrFunc.IsFilled(pValueDataKeyField))
                        {
                            sdPk.value = pValueDataKeyField;
                        }
                        else
                        {
                            if (TypeData.IsTypeString(sdPk.datatype))
                                sdPk.value = "N/A";
                            else if (TypeData.IsTypeInt(sdPk.datatype))
                                sdPk.value = "-1";
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdPk.datatype));
                        }
                        ret[i].Query = sdPk.ReplaceInString(SessionTools.CS, ret[i].Query);

                        if ((pReferential.IsUseSQLParameters) && ret[i].Query.Contains("@PK"))
                        {
                            DataParameter pkParameter = sdPk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            ret[i].Parameters.Add(pkParameter);
                        }

                    }
                    //    
                    //Replace du %%DA:FK%% pouvant exister ds la query
                    if (pReferential.IndexForeignKeyField > -1)
                    {
                        //Replace du %%DA:PK%% pouvant exister ds la query
                        StringDynamicData sdFk = new StringDynamicData
                        {
                            name = "FK",
                            datatype = pReferential[pReferential.IndexForeignKeyField].DataType.value
                        };
                        if (StrFunc.IsFilled(pReferential.ValueForeignKeyField)) // FI 20201215 [XXXXX] Usage de pReferential.ValueForeignKeyField
                        {
                            sdFk.value = pReferential.ValueForeignKeyField;
                        }
                        else
                        {
                            if (TypeData.IsTypeString(sdFk.datatype))
                                sdFk.value = "N/A";
                            else if (TypeData.IsTypeInt(sdFk.datatype))
                                sdFk.value = "-1";
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("dataType [{0}] is not implemented", sdFk.datatype));
                        }
                        ret[i].Query = sdFk.ReplaceInString(SessionTools.CS, ret[i].Query);

                        if ((pReferential.IsUseSQLParameters) && ret[i].Query.Contains("@FK"))
                        {
                            DataParameter fkParameter = sdFk.GetDataParameter(SessionTools.CS, null, CommandType.Text, 0, ParameterDirection.Input);
                            ret[i].Parameters.Add(fkParameter);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne un array  à partir de pDynamicDatas.Chaque item contient un StringDynamicData serializé
        /// </summary>
        /// <param name="pDynamicDatas"></param>
        /// <param name="pSource">Permet de slectionner les source des pDynamicDatas considérés</param>
        /// <returns></returns>
        /// FI 20200205 [XXXXX] Add Method
        public static string[] DynamicDatasToString(Dictionary<string, ReferentialsReferentialStringDynamicData> pDynamicDatas, DynamicDataSourceEnum pSource)
        {
            string[] ret = null;
            if ((null != pDynamicDatas) && (ArrFunc.Count(pDynamicDatas) > 0))
            {
                IEnumerable listDA = pDynamicDatas.Values.Where(x => (x.source | pSource) == pSource);

                string dA = string.Empty;
                foreach (ReferentialsReferentialStringDynamicData item in listDA)
                {
                    if (StrFunc.IsFilled(dA))
                        dA += StrFunc.StringArrayList.LIST_SEPARATOR;
                    // FI 20200205 [XXXXX] Remarque 
                    // =>Génération d'un StringDynamicData à partie d'un ReferentialsReferentialStringDynamicData
                    ReferentialsReferentialStringDynamicData _dd = (ReferentialsReferentialStringDynamicData)(item);
                    dA += ReferentialTools.SerializeDA(
                        new StringDynamicData(_dd.datatype, _dd.name, _dd.value)
                        );
                }
                if (StrFunc.IsFilled(dA))
                    ret = StrFunc.StringArrayList.StringListToStringArray(dA);
            }
            return ret;
        }

        /// <summary>
        ///  Returns a string representing an elapsed time interval
        /// </summary>
        /// <param name="pElapsedSec">Number of elapsed seconds</param>
        /// <returns>
        ///  If the elapsed time is greater then 1 hour, returns XXh YYmn, otherwise returns XXmn YYs
        /// </returns>
        /// AL 20240703 [WI605] Datakind Seconds for integer type
        public static string GetFormattedDuration(int pElapsedSec) {
            TimeSpan t = TimeSpan.FromSeconds(pElapsedSec);
            if (t.TotalHours >= 1)
                return String.Format("{0}h {1}mn", (t.Days * 24) + t.Hours, StrFunc.IntegerPadding(t.Minutes, 2));
            else
                return String.Format("{0}mn {1}s", t.Minutes, StrFunc.IntegerPadding(t.Seconds, 2));
        }
        /// <summary>
        ///  Retourne une donnée datetime formatée pour affichage dans le grid ou dans le formulaire
        /// </summary>
        ///<param name="collaborator">Utilisateur connecté</param>
        /// <param name="fmtDisplayCol">Format d'affichage du profil</param>
        /// <param name="rrcDataType">type de colonne date </param>
        /// <param name="pObject">Valeur de la colonne</param>
        /// <param name="pRow">Enregistrement</param>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        /// FI 20200224 [XXXXX] Method Static
        public static string GetFormatedDateTime(NameValueCollection fmtDisplayCol, Collaborator collaborator, ReferentialsReferentialColumnDataType rrcDataType, object pObject, DataRow pRow)
        {
            string dataType = rrcDataType.value;
            if (false == (TypeData.IsTypeDateTime(dataType) || TypeData.IsTypeDate(dataType) || TypeData.IsTypeTime(dataType)))
                throw new NotImplementedException(StrFunc.AppendFormat("type ({0}) is not implemented.", dataType));

            string ret;

            DateTime dt = Convert.ToDateTime(pObject); ;

            if (rrcDataType.datakindSpecified && rrcDataType.datakind == Cst.DataKind.Timestamp)
            {
                ret = GetFormatedTimestamp(rrcDataType, dt, fmtDisplayCol, collaborator, pRow);
            }
            else
            {
                if (TypeData.IsTypeDateTime(dataType))
                {
                    ret = dt.ToString(DtFunc.FmtDateLongTime);
                }
                else if (TypeData.IsTypeDate(dataType))
                {
                    ret = dt.ToString(DtFunc.FmtShortDate);
                }
                else if (TypeData.IsTypeTime(dataType))
                {
                    ret = dt.ToString(DtFunc.FmtLongTime);
                }
                else
                    throw new NotImplementedException(StrFunc.AppendFormat("type ({0}) is not implemented.", dataType));
            }

            return ret;
        }


        /// <summary>
        ///  Formate une donnée horodatage dans le format attentu par la session (fonction du profil utilisateur)
        /// <para>le résulat du Formatage affiche également le tzDbId</para>
        /// </summary>
        /// <param name="rrcDataType">type de colonne date </param>
        /// <param name="pDt"></param>
        /// <param name="pFmtDisplayCol">Format d'affichage du profil</param>
        /// <param name="pCollaborator">Représente l'utilisateur connecté</param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20200720 [XXXXX] Add
        public static string GetFormatedTimestamp(ReferentialsReferentialColumnDataType rrcDataType, DateTime pDt, NameValueCollection pFmtDisplayCol, Collaborator pCollaborator, DataRow pRow)
        {

            if (false == rrcDataType.displaySpecified)
                throw new NullReferenceException(StrFunc.AppendFormat("display attribut doesn't exist. Expected value in {0}",
                        ArrFunc.GetStringList(Enum.GetNames(typeof(Cst.DataTypeDisplayMode)))));

            string ret;

            DateTimeTz dtZonedSource = new DateTimeTz(pDt, rrcDataType.tzdbid);
            switch (rrcDataType.display)
            {
                case Cst.DataTypeDisplayMode.Audit:
                    ret = DtFuncExtended.DisplayTimestampAudit(dtZonedSource, new AuditTimestampInfo()
                    {
                        Collaborator = pCollaborator,
                        TimestampZone = (Cst.AuditTimestampZone)Enum.Parse(typeof(Cst.AuditTimestampZone), pFmtDisplayCol["AuditTimestampZone"]),
                        Precision = (Cst.AuditTimestampPrecision)Enum.Parse(typeof(Cst.AuditTimestampPrecision), pFmtDisplayCol["AuditTimestampPrecision"])
                    });
                    break;

                case Cst.DataTypeDisplayMode.Trading:
                    ret = DtFuncExtended.DisplayTimestampTrading(dtZonedSource, new TradingTimestampInfo()
                    {
                        Collaborator = pCollaborator,
                        TimestampZone = (Cst.TradingTimestampZone)Enum.Parse(typeof(Cst.TradingTimestampZone), pFmtDisplayCol["TradingTimestampZone"]),
                        Precision = (Cst.TradingTimestampPrecision)Enum.Parse(typeof(Cst.TradingTimestampPrecision), pFmtDisplayCol["TradingTimestampPrecision"]),
                        DataRow = pRow
                    });
                    break;

                case Cst.DataTypeDisplayMode.Delivery: // FI 20221207 [XXXXX] add 
                    ret = DtFuncExtended.DisplayTimestamp<DeliveryTimestampInfo>(dtZonedSource, new DeliveryTimestampInfo(pRow));

                    //ret = DtFuncExtended.DisplayTimestampDelivery(dtZonedSource, new DeliveryTimestampInfo()
                    //{
                    //    Collaborator = pCollaborator,
                    //    TimestampZone = (Cst.DeliveryTimestampZone)Enum.Parse(typeof(Cst.DeliveryTimestampZone), pFmtDisplayCol["TradingTimestampZone"]),
                    //    Precision = (Cst.DeliveryTimestampPrecision)Enum.Parse(typeof(Cst.DeliveryTimestampPrecision), pFmtDisplayCol["TradingTimestampPrecision"]),
                    //    DataRow = pRow
                    //});
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("display ({0}) is not implemented.", rrcDataType.value));
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferential"></param>
        /// <returns></returns>
        /// FI 20200820 [25468] Add Method
        private static string GetInfoRight(ReferentialsReferential pReferential)
        {
            string infoRight = string.Empty;
            if (pReferential.ExistsColumnsINS)
            {
                ReferentialsReferentialColumnDataType datatypeDt = pReferential.Column.First(x => x.ColumnName == "DTINS").DataType;
                if (datatypeDt.datakindSpecified && datatypeDt.datakind == Cst.DataKind.Timestamp && datatypeDt.display == Cst.DataTypeDisplayMode.Audit)
                {
                    DateTimeTz timeTz = new DateTimeTz(Convert.ToDateTime(pReferential.dataRow[pReferential.IndexColSQL_DTINS]), datatypeDt.tzdbidSpecified ? datatypeDt.tzdbid : "Etc/UTC");
                    string collaborator = pReferential.dataRow[pReferential.IndexColSQL_IDAINS + 1].ToString();
                    infoRight = RessourceExtended.GetString_CreatedBy(timeTz, collaborator, true);
                }
                else
                {
                    throw new InvalidProgramException(@"Column: DTINS. Invalid DataType (datakind: ""Timestamp"",display:""Audit"" expected");
                }
            }

            bool isWithUPD = (pReferential.ExistsColumnsUPD) && (!(pReferential.dataRow[pReferential.IndexColSQL_DTUPD] is DBNull));
            if (isWithUPD)
            {
                ReferentialsReferentialColumnDataType datatypeDt = pReferential.Column.First(x => x.ColumnName == "DTUPD").DataType;
                if (datatypeDt.datakindSpecified && datatypeDt.datakind == Cst.DataKind.Timestamp && datatypeDt.display == Cst.DataTypeDisplayMode.Audit)
                {
                    DateTimeTz timeTz = new DateTimeTz(Convert.ToDateTime(pReferential.dataRow[pReferential.IndexColSQL_DTUPD]), datatypeDt.tzdbidSpecified ? datatypeDt.tzdbid : "Etc/UTC");
                    string collaborator = pReferential.dataRow[pReferential.IndexColSQL_IDAUPD + 1].ToString();
                    infoRight += " - " +
                    RessourceExtended.GetString_LastModifyBy(timeTz, collaborator, true);
                }
                else
                {
                    throw new InvalidProgramException(@"Column: DTUPD. Invalid DataType (datakind: ""Timestamp"",display:""Audit"" expected");
                }
            }
            //PL 20161124 - RATP 4Eyes - MakingChecking
            if (pReferential.ExistsMakingChecking)
            {
                if (!(pReferential.dataRow[pReferential.IndexColSQL_DTCHK] is DBNull) && !(pReferential.dataRow[pReferential.IndexColSQL_IDACHK] is DBNull))
                {
                    ReferentialsReferentialColumnDataType datatypeDt = pReferential.Column.First(x => x.ColumnName == "DTCHK").DataType;
                    if (datatypeDt.datakindSpecified && datatypeDt.datakind == Cst.DataKind.Timestamp && datatypeDt.display == Cst.DataTypeDisplayMode.Audit)
                    {
                        DateTimeTz timeTz = new DateTimeTz(Convert.ToDateTime(pReferential.dataRow[pReferential.IndexColSQL_DTCHK]), datatypeDt.tzdbidSpecified ? datatypeDt.tzdbid : "Etc/UTC");
                        string collaborator = pReferential.dataRow[pReferential.IndexColSQL_IDACHK + 1].ToString();
                        infoRight += " - " +
                        RessourceExtended.GetString_CheckingBy(timeTz, collaborator, true);
                    }
                    else
                    {
                        throw new InvalidProgramException(@"Column: DTCHK. Invalid DataType (datakind: ""Timestamp"",display:""Audit"" expected");
                    }
                }
            }
            return infoRight;
        }

        /// <summary>
        /// Remplace dans {strData} des éventuelles expressions chooose en fonction des valeurs de pReferential.dynamicArgs
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pInput"></param>
        /// <returns></returns>
        /// FI 20201125 [XXXXX] Add
        public static string ReplaceDynamicArgsInChooseExpression(ReferentialsReferential pReferential, string pInput, bool pForceBuildDynamicDataObject = false)
        {
            string ret = pInput;

            if (pReferential.dynamicArgsSpecified && pInput.Contains(@"<choose>"))
            {
                if ((pForceBuildDynamicDataObject) || (null == pReferential.DynamicDataArgsInstance))
                    pReferential.BuildDynamicDataArgsInstance();

                ret = StrFuncExtended.ReplaceChooseExpression2(ret, pReferential.DynamicDataArgsInstance, true);
            }
            return ret;
        }


        #endregion  Methods
    }

}
