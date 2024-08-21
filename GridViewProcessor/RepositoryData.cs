#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EfsML.DynamicData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel.Syndication;
#endregion using directives

namespace EFS.GridViewProcessor
{
    #region EFSSyndicationFeed
    /// <summary>
    /// Classe d'utilisation des SyndicationFeed (RSS, Atom)
    /// </summary>
    public sealed class EFSSyndicationFeed
    {
        #region public enum SyndicationFeedFormatEnum
        public enum SyndicationFeedFormatEnum
        {
            ALL, RSS20, Atom10
        }
        #endregion
        #region public enum SyndicationFeedTypeEnum
        public enum SyndicationFeedTypeEnum
        {
            SALESNEWS,      //SALES, FpML/FIXML version, ...
            SOFTWARENEWS,   //Release, Patch, Bug...
            BUSINESSNEWS,   //Corporate actions, ...
        }
        #endregion

        #region public SyndicationFeed
        /// <summary>
        /// Get a SyndicationFeed from SYNDICATIONFEED/SYNDICATIONITEM tables.
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pSyndicationFeedType"></param>
        /// <returns></returns>
        public static SyndicationFeed GetSyndicationFeed(string pCs, SyndicationFeedTypeEnum pSyndicationFeedType, string pCulture)
        {
            SyndicationFeed feed = null;
            SyndicationItem item = null;
            List<SyndicationItem> items = new List<SyndicationItem>();
            IDataReader dr;
            int idSyndicationFeed;
            string syndicationFeedLinks;

            string sql = SQLCst.SELECT + "IDSYNDICATIONFEED,LINKS,COPYRIGHT,AUTHORS,GENERATOR,IMAGEURL";
            sql += "," + DataHelper.SQLIsNull(pCs, "TITLE_" + pCulture, "TITLE_EN", "TITLE");
            sql += "," + DataHelper.SQLIsNull(pCs, "DESCRIPTION_" + pCulture, "DESCRIPTION_EN", "DESCRIPTION");
            sql += SQLCst.FROM_DBO + "SYNDICATIONFEED" + Cst.CrLf;
            sql += SQLCst.WHERE + "FEEDTYPE=" + DataHelper.SQLString(pSyndicationFeedType.ToString());
            dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sql);
            if (dr.Read())
            {
                idSyndicationFeed = Convert.ToInt32(dr["IDSYNDICATIONFEED"]);
                //TBD Multi-Links (separator ";")
                syndicationFeedLinks = Convert.ToString(dr["LINKS"]);
                feed = new SyndicationFeed(Convert.ToString(dr["TITLE"]), Convert.ToString(dr["DESCRIPTION"]), new Uri(syndicationFeedLinks));
                feed.Authors.Add(new SyndicationPerson(Convert.ToString(dr["AUTHORS"])));
                feed.Description = new TextSyndicationContent(Convert.ToString(dr["DESCRIPTION"]));
                feed.Language = pCulture.ToLower();
                feed.Copyright = new TextSyndicationContent(Convert.ToString(dr["COPYRIGHT"]));
                feed.Generator = Convert.ToString(dr["GENERATOR"]);

                //TBD Image width, height...
                feed.ImageUrl = new Uri(Convert.ToString(dr["IMAGEURL"]));
                dr.Close();

                sql = SQLCst.SELECT + "IDSYNDICATIONITEM,LINKS,PUBLISHDATE,CATEGORIES" + Cst.CrLf;
                sql += "," + DataHelper.SQLIsNull(pCs, "TITLE_" + pCulture, "TITLE_EN", "TITLE");
                sql += "," + DataHelper.SQLIsNull(pCs, "SUMMARY_" + pCulture, "SUMMARY_EN", "SUMMARY");
                sql += ", ENCLOSURE_URI, ENCLOSURE_LENGTH, ENCLOSURE_MEDIATYPE, COMMENTS";
                sql += SQLCst.FROM_DBO + "SYNDICATIONITEM" + Cst.CrLf;
                sql += SQLCst.WHERE + "IDSYNDICATIONFEED=" + idSyndicationFeed.ToString();
                sql += SQLCst.AND + "ISENABLED=1" + Cst.CrLf;
                sql += SQLCst.ORDERBY + "PUBLISHDATE" + SQLCst.DESC;
                dr = DataHelper.ExecuteReader(pCs, CommandType.Text, sql);
                while (dr.Read())
                {
                    item = new SyndicationItem(
                       Convert.ToString(dr["TITLE"]),
                       Convert.ToString(dr["SUMMARY"]),
                       new Uri(StrFunc.IsFilled(Convert.ToString(dr["LINKS"])) ? Convert.ToString(dr["LINKS"]) : syndicationFeedLinks),
                       Convert.ToString(dr["IDSYNDICATIONITEM"]),
                       Convert.ToDateTime(dr["PUBLISHDATE"])
                       );

                    if (Convert.IsDBNull(dr["CATEGORIES"]) || StrFunc.IsEmpty(Convert.ToString(dr["CATEGORIES"])))
                    {
                        item.Categories.Add(new SyndicationCategory("N/A"));
                    }
                    else
                    {
                        string[] categories = Convert.ToString(dr["CATEGORIES"]).Split(new char[] { ';' });
                        foreach (string category in categories)
                        {
                            item.Categories.Add(new SyndicationCategory(category));
                        }
                    }
                    item.PublishDate = Convert.ToDateTime(dr["PUBLISHDATE"]);

                    if ((false == Convert.IsDBNull(dr["ENCLOSURE_URI"])) &&
                        (false == Convert.IsDBNull(dr["ENCLOSURE_LENGTH"])) &&
                        (false == Convert.IsDBNull(dr["ENCLOSURE_MEDIATYPE"])))
                    {
                        SyndicationLink enclosure = SyndicationLink.CreateMediaEnclosureLink(new Uri(Convert.ToString(dr["ENCLOSURE_URI"])),
                            Convert.ToString(dr["ENCLOSURE_MEDIATYPE"]), Convert.ToInt32(dr["ENCLOSURE_LENGTH"]));
                        item.Links.Add(enclosure);
                    }
                    if (false == Convert.IsDBNull(dr["COMMENTS"]))
                    {
                        item.ElementExtensions.Add("comments", null, Convert.ToString(dr["COMMENTS"]));
                    }
                    items.Add(item);
                }
                feed.Items = items;

                dr.Close();
                dr.Dispose();
            }
            return feed;
        }

        /// <summary>
        /// Create an XML file from a SyndicationFeed.
        /// </summary>
        /// <param name="pFeed"></param>
        /// <param name="pSyndicationFeedFormat"></param>
        /// <param name="pPath"></param>
        public static void CreateSyndicationFeedFile(SyndicationFeed pFeed, SyndicationFeedFormatEnum pSyndicationFeedFormat, string pPath)
        {
            string write_File = pPath; ;

            System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
            xmlWriterSettings.Indent = true;
            System.Xml.XmlWriter feedWriter;

            switch (pSyndicationFeedFormat)
            {
                case SyndicationFeedFormatEnum.ALL:
                case SyndicationFeedFormatEnum.Atom10:
                    // Use Atom 1.0        
                    Atom10FeedFormatter atomFormatter = new Atom10FeedFormatter(pFeed);
                    if (pSyndicationFeedFormat == SyndicationFeedFormatEnum.ALL)
                        write_File = pPath.Replace("{ALL}", "Atom10");
                    feedWriter = System.Xml.XmlWriter.Create(write_File, xmlWriterSettings);
                    atomFormatter.WriteTo(feedWriter);
                    feedWriter.Close();
                    break;
            }
            switch (pSyndicationFeedFormat)
            {
                case SyndicationFeedFormatEnum.ALL:
                case SyndicationFeedFormatEnum.RSS20:
                    // Emit RSS 2.0
                    Rss20FeedFormatter rssFormatter = new Rss20FeedFormatter(pFeed);
                    if (pSyndicationFeedFormat == SyndicationFeedFormatEnum.ALL)
                        write_File = pPath.Replace("{ALL}", "RSS20");
                    feedWriter = System.Xml.XmlWriter.Create(write_File, xmlWriterSettings);
                    rssFormatter.WriteTo(feedWriter);
                    feedWriter.Close();
                    break;
            }
        }
        #endregion
    }
    #endregion EFSSyndicationFeed

    #region SQLReferentialColumn
    /// <summary>
    /// Classe encapsulant les méthodes et propriétés d'une colonne du referentiel dans le Script SQL
    /// </summary>
    public class SQLReferentialColumn
    {
        #region Members
        private ReferentialColumn _rc;

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
            get { return SQLColumnNameOrSQLColumnSQLSelect + SQLCst.AS + sqlColumnAlias; }
        }
        #endregion

        /// <summary>
        /// Obtient une l'expression SQL de la colonne {alias}+{ColumName} 
        /// </summary>        
        public string sqlColumnName
        {
            get { return _sqlColumnName; }
        }
        /// <summary>
        /// Obtient l'alias de la colonne
        /// </summary>
        public string sqlColumnAlias
        {
            get { return _sqlColumnAlias; }
        }
        /// <summary>
        /// Obtient l'order by asscié à la colonne
        /// </summary>
        public string sqlColumnOrderBy
        {
            get { return _sqlColumnOrderBy; }
        }
        #endregion Accessors

        #region public ConstructSqlColumn
        public void ConstructSqlColumn(string pCS, ReferentialColumn pRc, bool pIsWithOrderBy, bool pIsUseColumnAlias, bool pIsUseColumnAliasInOrderExpression)
        {
            _rc = pRc;

            if (_rc.ColumnName == Cst.OTCml_COL.ROWVERSION.ToString())
            {
                // RD 20091102 / Utilisation de sqlColumn
                // RD 20091223 / 16802/ LSTCOLUMN.SQLSELECT / Correction
                SetSqlColumnInfo(DataHelper.GetROWVERSION(pCS, _rc.AliasTableName, string.Empty), _rc.DataField.ToUpper(), _rc.AliasTableName);
            }
            else if (_rc.ColumnName == Cst.OTCml_COL.DTHOLIDAYNEXTDATE.ToString())
            {
                //PL 20120116 Newness: DTHOLIDAYNEXTDATE, colonne fictive associée à la colonne DTHOLIDAYVALUE
                SetSqlColumnInfo(DataHelper.SQLToDate(pCS, SQLCst.NULL), _rc.ColumnName);
            }
            else
            {

                SetSqlColumnInfo(_rc.ColumnName, _rc.DataField.ToUpper(), _rc.AliasTableName);
                if (_rc.ColumnNameOrColumnSQLSelectSpecified)
                    _sqlColumnNameOrSQLColumnSQLSelect = _rc.ColumnNameOrColumnSQLSelect;
            }

            if (pIsWithOrderBy)
            {
                if (_rc.IsOrderBySpecified && _rc.IsOrderBy.orderSpecified)
                {
                    _sqlColumnOrderBy = _rc.IsOrderBy.order.Replace(Cst.DYNAMIC_ALIASTABLE, _rc.AliasTableName);
                    //
                    if (pIsUseColumnAliasInOrderExpression)
                    {
                        _sqlColumnOrderBy = _sqlColumnOrderBy.Replace(_sqlColumnName, _sqlColumnAlias);
                        // 20110308 EG Correction Test sur Alias defini (tag AliasTableName) !!!
                        //_sqlColumnOrderBy = _sqlColumnOrderBy.Replace(SQLCst.TBLMAIN + ".", string.Empty);
                        string aliasTableName = SQLCst.TBLMAIN;
                        if (_rc.AliasTableNameSpecified)
                            aliasTableName = _rc.AliasTableName;
                        _sqlColumnOrderBy = _sqlColumnOrderBy.Replace(aliasTableName + ".", string.Empty);
                    }
                }
                else
                {
                    SetSqlColumnOrderBy(pIsUseColumnAlias);
                }
            }

        }
        public void ConstructSqlColumn(ReferentialSQLOrderBy pRRSQLOrderBy)
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
                _sqlColumnOrderBy = sqlColumnAlias;
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
            _sqlColumnNameOrSQLColumnSQLSelect = sqlColumnName;
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

            _sqlColumnNameOrSQLColumnSQLSelect = sqlColumnName;
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
            ret += SQLCst.AS + sqlColumnAlias;
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
            if (null == _rc)
                throw new Exception("ReferentialColumn is not Specified");
            //
            GetSqlGroupBy(_rc.GroupBySpecified, _rc.GroupBy, ref pSql_SelectGBFirst, ref pSql_SelectGB, ref pSql_GroupBy);

        }
        public void GetSqlGroupBy(ReferentialColumnGroupBy pRc_GroupBy,
            ref string pSql_SelectGBFirst, ref string pSql_SelectGB, ref string pSql_GroupBy)
        {
            GetSqlGroupBy(true, pRc_GroupBy, ref pSql_SelectGBFirst, ref pSql_SelectGB, ref pSql_GroupBy);
        }
        private void GetSqlGroupBy(bool pIsRrc_GroupBySpecified, ReferentialColumnGroupBy pRc_GroupBy,
            ref string pSql_SelectGBFirst, ref string pSql_SelectGB, ref string pSql_GroupBy)
        {
            // RD 20091102 / Utilisation de sqlColumn
            bool isRrcGroupBy = pIsRrc_GroupBySpecified && pRc_GroupBy.IsGroupBy;
            bool isRrcSqlGroupBy = pIsRrc_GroupBySpecified && pRc_GroupBy.SqlGroupBySpecified;
            bool isRrcAggregate = pIsRrc_GroupBySpecified && pRc_GroupBy.AggregateSpecified;
            //
            if (isRrcSqlGroupBy && (!isRrcAggregate))
                pSql_SelectGBFirst += "(" + pRc_GroupBy.SqlGroupBy + ")" + SQLCst.AS;
            //
            pSql_SelectGBFirst += sqlColumnAlias + ",";
            //
            if (isRrcAggregate)
            {
                //PL 20100210 Add WAVG (NB: Si différent de SUM ou AVG, alors SqlGroupBy contient toute la syntaxe SQL)
                string aggregate = pRc_GroupBy.Aggregate;
                //PL 20110228
                bool isSUMorAVG = aggregate.ToUpper() == "SUM" || aggregate.ToUpper() == "AVG";
                bool isOldSyntaxeForSUMorAVG = false;
                if (isSUMorAVG)
                {
                    //NB: Avec l'ancienne syntaxe, la colonne SQLGROUPBY ne contient pas toute la syntaxe SQL
                    isOldSyntaxeForSUMorAVG = !pRc_GroupBy.SqlGroupBy.StartsWith(aggregate);
                }
                //
                if (isOldSyntaxeForSUMorAVG)
                    pSql_SelectGB += aggregate + "(";
                //
                if (isRrcSqlGroupBy)
                    pSql_SelectGB += pRc_GroupBy.SqlGroupBy;
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
            pSql_SelectGB += SQLCst.AS + sqlColumnAlias + ",";
        }
        public void GetSqlGroupBy(ref string pSql_SelectGBFirst, ref string pSql_SelectGB)
        {
            pSql_SelectGBFirst += sqlColumnAlias;
            pSql_SelectGB += SQLSelect + ", ";
        }
        public void GetSqlGroupBy(string pSqlSelect, ref string pSql_SelectGBFirst, ref string pSql_SelectGB)
        {
            pSql_SelectGBFirst += sqlColumnAlias;
            pSql_SelectGB += pSqlSelect + ", ";
        }
        #endregion
    }
    #endregion SQLReferentialColumn

    #region SQLReferentialData
    public class SQLReferentialData
    {
        #region Constant/Enum
        private const string sql_GroupByNumber = "GroupByNumber";
        private const string sql_GroupByCount = "GroupByCount";

        /// <summary>
        /// 
        /// </summary>
        public enum SelectedColumnsEnum
        {
            All,
            NoHideOnly,
            None
        }
        #endregion
        //
        #region Constructor(s)
        public SQLReferentialData() { }
        #endregion Constructor(s)
        //
        #region public ApplyChangesInSQLTable
        /// <summary>
        /// Update le dataset passé en arg ainsi que d'eventuelles tables enfants de referential
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pDataSet">dataset contenant les données</param>
        /// <param name="opRowsAffected">OUT nb de lignes affectées</param>
        /// <param name="opMessage">OUT message d'erreur (traduit)</param>
        /// <param name="opError">OUT erreur (brute, non traduite)</param>
        /// <param name="pIsSendMQueue">Postage d'un message dans le MOM</param>
        /// <returns>Cst.ErrLevel</returns>
        // 20091014 EG Gestion des SendMessages
        public static Cst.ErrLevel ApplyChangesInSQLTable(string pSource, Referential pReferential, DataSet pDataSet,
            out int opRowsAffected, out string opMessage, out string opError)
        {
            return ApplyChangesInSQLTable(pSource, null, pReferential, pDataSet, out opRowsAffected, out opMessage, out opError, true, null, null);
        }
        public static Cst.ErrLevel ApplyChangesInSQLTable(string pSource, IDbTransaction pDbTransaction, Referential pReferential, DataSet pDataSet,
            out int opRowsAffected, out string opMessage, out string opError, MQueueRequester pRequester, string pIdMenu)
        {
            return ApplyChangesInSQLTable(pSource, pDbTransaction, pReferential, pDataSet, out opRowsAffected, out opMessage, out opError, true, pRequester, pIdMenu);
        }
        public static Cst.ErrLevel ApplyChangesInSQLTable(string pSource, Referential pReferential, DataSet pDataSet,
            out int opRowsAffected, out string opMessage, out string opError, bool pIsSendMQueue)
        {
            return ApplyChangesInSQLTable(pSource, null, pReferential, pDataSet, out opRowsAffected, out opMessage, out opError, pIsSendMQueue, null, null);
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        public static Cst.ErrLevel ApplyChangesInSQLTable(string pSource, IDbTransaction pDbTransaction, Referential pReferential, DataSet pDataSet,
            out int opRowsAffected, out string opMessage, out string opError, bool pIsSendMQueue, MQueueRequester pRequester, string pIdMenu)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            opError = string.Empty;
            opMessage = string.Empty;
            opRowsAffected = 0;

            bool isNeedUpdate = false;
            // Checking if datas need to be updated
            foreach (DataTable dt in pDataSet.Tables)
            {
                DataTable dtChanges = dt.GetChanges();
                if (dtChanges != null)
                {
                    isNeedUpdate = true;
                    break;
                }
            }

            if (isNeedUpdate)
            {
                try
                {
                    SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(pSource, pReferential);
                    sqlSelectParameters.isForExecuteDataAdapter = true;

                    ArrayList alChildSQLSelect;
                    ArrayList alFiller;
                    bool isFiller;
                    QueryParameters query = GetSQLSelect(sqlSelectParameters, out alChildSQLSelect, out alFiller, out isFiller);
                    string SQLSelect = query.GetQueryReplaceParameters(false);

                    #region Specifique au referentiel virtuel: QUOTE_H_EOD
                    // --------------------------------------------------------------------------------------------------------------------------------------------------------
                    // PL/FI 20110707 Les mises à jour effectuées depuis le référentiel "virtuel" QUOTE_H_EOD 
                    //                sont ensuite déversées dans les tables spécifiques EX QUOTE_ETD_H, QUOTE_EQUITY_H...
                    // --------------------------------------------------------------------------------------------------------------------------------------------------------
                    string referentialTableName = pReferential.TableName;
                    if (referentialTableName == "QUOTE_H_EOD")
                    {
                        //WARNING: Codage en "dur" 
                        //Pour rappel, on est ici sur une vue qui expose virtuellement les cotations nécessaires à une date de compensation donnée.
                        //La donnée valueDataKeyField contient: convert(varchar,isnull(q.IDQUOTE_H,0))+';'+'ETD'+';'+convert(varchar,a.IDASSET)
                        //Par conséquent, quand valueDataKeyField commence par "0;" cela indique une nouvelle cotation (IDQUOTE_H = 0).
                        string[] keyElements = pDataSet.Tables[0].Rows[0].ItemArray[0].ToString().Split(new char[] { ';' });
                        string quoteType = keyElements[1];
                        referentialTableName = "QUOTE_" + quoteType + "_H"; //ex. QUOTE_ETD_H

                        //Rename de la table dans la Query destinée au DataAdapter pour la maj de la table
                        SQLSelect = SQLSelect.Replace(SQLCst.DBO + "QUOTE_H_EOD", SQLCst.DBO + referentialTableName);

                        //Suppression de la colonne fictive dans la Query destinée au DataAdapter pour la maj de la table
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".KEYVALUE" + SQLCst.AS + "KEYVALUE,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_IDENTIFIER" + SQLCst.AS + "ASSET_IDENTIFIER,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_TYPE" + SQLCst.AS + "ASSET_TYPE,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_IDM" + SQLCst.AS + "ASSET_IDM,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_IDDC" + SQLCst.AS + "ASSET_IDDC,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_IDMATURITY" + SQLCst.AS + "ASSET_IDMATURITY,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_CATEGORY" + SQLCst.AS + "ASSET_CATEGORY,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_STRIKEPRICE" + SQLCst.AS + "ASSET_STRIKEPRICE,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_TICKVALUE" + SQLCst.AS + "ASSET_TICKVALUE,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_TICKSIZE" + SQLCst.AS + "ASSET_TICKSIZE,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_MULTIPLIER" + SQLCst.AS + "ASSET_MULTIPLIER,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_DEN" + SQLCst.AS + "ASSET_DEN,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_MATFMT_PROFIL" + SQLCst.AS + "ASSET_MATFMT_PROFIL,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_DC_IDENT" + SQLCst.AS + "ASSET_DC_IDENT,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_DC_DSP" + SQLCst.AS + "ASSET_DC_DSP,", string.Empty);
                        SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".ASSET_PUTCALL" + SQLCst.AS + "ASSET_PUTCALL,", string.Empty);

                        //Suppression des colonnes spécifiques aux ETD 
                        if (quoteType != "ETD")
                        {
                            SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".TIMETOEXPIRATION" + SQLCst.AS + "TIMETOEXPIRATION,", string.Empty);
                            SQLSelect = SQLSelect.Replace(SQLCst.TBLMAIN + ".CONTRACTMULTIPLIER" + SQLCst.AS + "CONTRACTMULTIPLIER,", string.Empty);
                        }
                    }
                    #endregion Specifique au referentiel virtuel: QUOTE_H_EOD

                    //Initialisation d'un objet mQueueDataset dans le cas où les données du référentiel nécessitent le postage d'un message à un service business.
                    MQueueDataset mQueueDataset = new MQueueDataset(pSource, referentialTableName);
                    if (mQueueDataset.IsAvailable)
                        mQueueDataset.Prepare(pDataSet.Tables[0]);
                    else
                        mQueueDataset = null;

                    int nRows = 0;
                    if (pDbTransaction == null)
                    {
                        nRows = DataHelper.ExecuteDataAdapter(pSource, SQLSelect, pDataSet.Tables[0]);
                    }
                    else
                    {
                        nRows = DataHelper.ExecuteDataAdapter(pDbTransaction, SQLSelect, pDataSet.Tables[0]);
                    }
                    if (nRows < 0)
                    {
                        ret = Cst.ErrLevel.FAILURE;
                    }
                    else if (Cst.ErrLevel.SUCCESS == ret)
                    {
                        #region Externals datas
                        if (pReferential.HasMultiTable)
                        {
                            if (!pReferential.isNewRecord)
                            {
                                //if not new record, updating externals datas if exists
                                for (int i = 1; i < pDataSet.Tables.Count; i++)
                                {
                                    if (pDataSet.Tables[i].Rows.Count > 0)
                                    {
                                        SQLSelect = alChildSQLSelect[i - 1].ToString();
                                        if (pDbTransaction == null)
                                        {
                                            nRows += DataHelper.ExecuteDataAdapter(pSource, SQLSelect, pDataSet.Tables[i]);
                                        }
                                        else
                                        {
                                            nRows += DataHelper.ExecuteDataAdapter(pDbTransaction, SQLSelect, pDataSet.Tables[i]);
                                        }
                                    }
                                }
                            }
                            else if (pReferential.IsForm && (pReferential.IndexKeyField != -1))
                            {
                                //if new record and exists Keyfield: get the new created ID with keyField for the new externals datas
                                IDbDataParameter sqlParam = null;
                                string sqlSelect = string.Empty;
                                sqlSelect += SQLCst.SELECT + pReferential.Column[pReferential.IndexDataKeyField].ColumnName + " as OVALUE";
                                sqlSelect += SQLCst.FROM_DBO + referentialTableName;
                                sqlSelect += SQLCst.WHERE + pReferential.Column[pReferential.IndexKeyField].ColumnName + "=" + DataHelper.GetVarPrefix(pSource) + "PARAM";

                                //formating data with datatype of keyfield
                                if (TypeData.IsTypeInt(pReferential.Column[pReferential.IndexKeyField].DataType.value))
                                {
                                    // EG 20150920 [21314] Int (int32) to Long (Int64) 
                                    sqlParam = new EFSParameter(pSource, "PARAM", DbType.Int64).DataParameter;
                                }
                                else if (TypeData.IsTypeBool(pReferential.Column[pReferential.IndexKeyField].DataType.value))
                                    sqlParam = new EFSParameter(pSource, "PARAM", DbType.Boolean).DataParameter;
                                else if (TypeData.IsTypeDec(pReferential.Column[pReferential.IndexKeyField].DataType.value))
                                    sqlParam = new EFSParameter(pSource, "PARAM", DbType.Decimal).DataParameter;
                                else if (TypeData.IsTypeDateOrDateTime(pReferential.Column[pReferential.IndexKeyField].DataType.value))
                                    sqlParam = new EFSParameter(pSource, "PARAM", DbType.DateTime).DataParameter;
                                else
                                    sqlParam = new EFSParameter(pSource, "PARAM", DbType.AnsiString, 64).DataParameter;

                                sqlParam.Value = pReferential.dataRow[pReferential.IndexColSQL_KeyField];

                                object oValue = null;
                                if (pDbTransaction == null)
                                {
                                    oValue = DataHelper.ExecuteScalar(pSource, CommandType.Text, sqlSelect, sqlParam);
                                }
                                else
                                {
                                    oValue = DataHelper.ExecuteScalar(pDbTransaction, CommandType.Text, sqlSelect, sqlParam);
                                }
                                if (oValue != null)
                                {
                                    //affecting ID to each externals datas
                                    for (int i = 0; i < pReferential.drExternal.GetLength(0); i++)
                                    {
                                        if (pReferential.isNewDrExternal[i])
                                        {
                                            pReferential.drExternal[i].BeginEdit();
                                            pReferential.drExternal[i]["ID"] = oValue;
                                            pReferential.drExternal[i].EndEdit();
                                        }
                                    }
                                    //updating each externals datas
                                    for (int i = 0; i < alChildSQLSelect.Count; i++)
                                    {
                                        if (pDataSet.Tables[i + 1].Rows.Count > 0)
                                        {
                                            SQLSelect = alChildSQLSelect[i].ToString();
                                            if (pDbTransaction == null)
                                            {
                                                nRows += DataHelper.ExecuteDataAdapter(pSource, SQLSelect, pDataSet.Tables[i + 1]);
                                            }
                                            else
                                            {
                                                nRows += DataHelper.ExecuteDataAdapter(pDbTransaction, SQLSelect, pDataSet.Tables[i + 1]);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        #endregion Externals datas
                    }

                    #region MQueueDataset (Sending)
                    if ((Cst.ErrLevel.SUCCESS == ret) && (mQueueDataset != null))
                    {
                        //Utilisation de l'objet mQueueDataset créé ci-dessus pour le postage d'un message à un service business.
                        MQueueSendInfo mqSendInfo = new MQueueSendInfo();
                        mqSendInfo.LoadCurrentAppSettings();
                        mQueueDataset.Send(pDbTransaction, pRequester, SessionTools.NewAppInstance(), mqSendInfo, pIdMenu);
                        mQueueDataset = null;
                    }
                    #endregion MQueueDataset (Sending)

                    opRowsAffected = nRows;
                }
                catch (Exception e)
                {
                    // if an exception occurs; analyse it with AnalyseSQLException()
                    opRowsAffected = 0;
                    bool isSQLException = DataHelper.AnalyseSQLException(pSource, e, out opMessage, out opError);
                    if (isSQLException)
                        ret = Cst.ErrLevel.SQLDEFINED;//an SQL error occurs
                    else
                        throw;
                }
                finally
                {
                    //Suppression de la derniere ligne si error SQL
                    if (ret == Cst.ErrLevel.SQLDEFINED && (pReferential.isNewRecord))
                    {
                        pReferential.dataSet.Tables[0].Rows.RemoveAt(pReferential.dataSet.Tables[0].Rows.Count - 1);
                    }
                }
            }

            return ret;
        }
        #endregion ApplyChangesInSQLTable
        #region public DeleteDataInSQLTable
        /// <summary>
        /// Delete la ligne correspondant au DataKeyField passé en arg pour le referential
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pDataKeyValue">valeur de DataKeyField</param>
        /// <returns>Cst.ErrLevel</returns>
        public static Cst.ErrLevel DeleteDataInSQLTable(string pSource, Referential pReferential, string pDataKeyValue)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;
            //
            //On execute le delete sur la table main
            string SQLDelete = GetSQLDelete(pReferential, pDataKeyValue);
            int rowsaffected = DataHelper.ExecuteNonQuery(pSource, CommandType.Text, SQLDelete);
            //
            //Si des enregistrements sont présents dans EXTLID pour cet enregistrement, on les delete aussi
            SQLDelete = GetSQLDeleteExternals(pReferential, pDataKeyValue);
            rowsaffected += DataHelper.ExecuteNonQuery(pSource, CommandType.Text, SQLDelete);
            //
            if (rowsaffected > 0)
                ret = Cst.ErrLevel.SUCCESS;
            //
            return ret;

        }
        #endregion DeleteDataInSQLTable
        #region public RetrieveDataFromSQLTable
        /// <revision>
        ///     <version>2.2.0</version><date>20090109</date><author>PL</author>
        ///     <comment>
        ///     New parameter: pIsFirstTableOnly
        ///     </comment>
        /// </revision>	
        /// <summary>
        /// Génére un DataSet contenant:
        /// - une table principale (table[0]) 
        /// - 0 à N tables secondaires, utilisées pour ajouter/modifier des données se trouvant dans d'autres tables que la tables principale (ex. EXTLID)
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pColumnFK">colonne pour un where specifique</param>
        /// <param name="pValueFKFormated">valeur de la colonne pour un where specifique</param>
        /// <param name="pForeignKeyValue">
        /// additional value, filled with the foreign key value 
        /// when the pColumnValue does not contain the foreign key value, but the data key instead</param>
        public static DataSet RetrieveDataFromSQLTable(string pCS, Referential pReferential, string pColumn, string pColumnValue, bool pIsColumnDataKeyField,
                                                        bool pIsTableMainOnly, bool pIsSelectDistinct, string pForeignKeyValue)
        {
            ArrayList alChildSQLSelect;
            ArrayList alChildTablename;
            bool isQueryWithSubTotal;

            SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(pCS, SelectedColumnsEnum.All, pReferential);
            QueryParameters query = GetQuery_LoadReferential(sqlSelectParameters,
                                            pColumn, pColumnValue, pIsColumnDataKeyField, 
                                            pIsTableMainOnly,
                                            out alChildSQLSelect, out alChildTablename, out isQueryWithSubTotal);

            // 20120313 MF Evaluate Dynamic argument relative to foreign key %%FOREIGN_KEY%%
            query.query = EvaluateForeignKeyDynamicArgument(
                pCS, query.query, pReferential.IndexForeignKeyField,
                pReferential.IndexForeignKeyField > 0 ?
                    pReferential[pReferential.IndexForeignKeyField].DataType.value : null,
                pIsColumnDataKeyField, pColumnValue, pForeignKeyValue);

            //PL 20110303 TEST for SQLServer WITH (TBD)
            query.query = TransformRecursiveQuery(pCS, pReferential, pColumn, pColumnValue, pIsColumnDataKeyField, query.query);
            DataSet ret = DataHelper.ExecuteDataset(pCS, CommandType.Text, query.query, query.parameters.GetArrayDbParameter());

            //Le DataSet récupéré contient N tables (autant que de queries), on leur affecte leur TableName pour les identifier
            ret.Tables[0].TableName = pReferential.TableName;
            for (int i = 1; i < ret.Tables.Count; i++)
                ret.Tables[i].TableName = alChildTablename[i - 1].ToString();

            return ret;
        }

        public static DataSet RetrieveDataFromSQLTable(
            string pCS, Referential pReferential, string pColumn, string pColumnValue, bool pIsColumnDataKeyField,
            bool pIsTableMainOnly, bool pIsSelectDistinct)
        {
            return RetrieveDataFromSQLTable(pCS, pReferential, pColumn, pColumnValue, pIsColumnDataKeyField, pIsTableMainOnly, pIsSelectDistinct, String.Empty);
        }

        #endregion RetrieveDataFromSQLTable

        /// <summary>
        /// Replace the Foreign Key dynamic argument (%%FOREIGN_KEY%%) with the value passed inside the input argument pColumnValue
        /// </summary>
        /// <param name="pCS">current connection string</param>
        /// <param name="pQuery">query where we want to perform the replacement</param>
        /// <param name="pIndexForeignKey">index of the Referential field where the foreign key has been found. IsForeignKeyField:=true</param>
        /// <param name="pDataTypeForeignKeyField">data type of the Referential field where the foreign key has been found</param>
        /// <param name="pIsColumnValueRelativeToDataKey">Check for pColumnValue parameter, when true then the value of pColumnValue is
        /// NOT bound to the foreign key but to the data key</param>
        /// <param name="pColumnValue">replacement value for the dynamic argument</param>
        /// <param name="pForeignKeyValue">
        /// additional value, filled with the foreign key value 
        /// when the pColumnValue does not contain the foreign key value, but the data key instead</param>
        /// <returns>the input string with the evaluated dynamic argument or in the original state </returns>
        /// <exception cref="NotSupportedException">in any case we need to use the pForeignKeyValue and this one is not well initialized </exception>
        public static string EvaluateForeignKeyDynamicArgument(
            string pCS, string pQuery, int pIndexForeignKey, string pDataTypeForeignKeyField,
            bool pIsColumnValueRelativeToDataKey, string pColumnValue, string pForeignKeyValue)
        {
            string res = pQuery;

            TypeData.TypeDataEnum typeData = TypeData.TypeDataEnum.unknown;

            if (!String.IsNullOrEmpty(pDataTypeForeignKeyField))
            {
                typeData = TypeData.GetTypeDataEnum(pDataTypeForeignKeyField, false);
            }

            // 1. Check arguments, when the foreign key is not declared OR 
            //  the data type of the foreign key is missing, then the query is returned without any replacements
            if (pIndexForeignKey < 0 || typeData == TypeData.TypeDataEnum.unknown || !pQuery.Contains(Cst.FOREIGN_KEY))
            {
                return res;
            }

            // 1.1 In case the replacement may be performed and we need to use the pForeignKeyValue argument, but the argument is not
            //   well initialized an exception is raised
            if (pIsColumnValueRelativeToDataKey && String.IsNullOrEmpty(pForeignKeyValue))
            {
                throw new ArgumentException(@"The foreign key value is not well initialised 
                    the %%FOREIGN_KEY%% dynamic argument can not be evaluated.", "pForeignKeyValue");
            }

            // 2. check if columnValue is related to the foreign key, else replace that with the explicit foreign key value
            if (pIsColumnValueRelativeToDataKey)
            {
                pColumnValue = pForeignKeyValue;
            }

            // 3. optional value formatting, and special cases for  empty pColumnValue
            switch (typeData)
            {
                case TypeData.TypeDataEnum.@string:

                    if (String.IsNullOrEmpty(pColumnValue))
                    {
                        pColumnValue = Cst.NotAvailable;
                    }

                    pColumnValue = DataHelper.SQLString(pColumnValue);

                    break;

                default:

                    if (String.IsNullOrEmpty(pColumnValue))
                    {
                        pColumnValue = "-1";
                    }

                    break;
            }

            // 4. replacement
            res = pQuery.Replace(Cst.FOREIGN_KEY, pColumnValue);

            return res;
        }

        //
        #region private GetSQLDelete
        /// <summary>
        /// Renvoie la requete SQL pour le delete d'un ligne dont on passe la valeur du dataKeyField en arg
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pDataKeyValue">valeur du DataKeyfield a supprimer</param>
        /// <returns>requete DELETE</returns>
        private static string GetSQLDelete(Referential pReferential, string pDataKeyValue)
        {
            string SQLQuery = string.Empty;
            if (pReferential.IndexDataKeyField != -1)
            {
                SQLQuery = SQLCst.DELETE_DBO + pReferential.TableName + Cst.CrLf;
                SQLQuery += SQLCst.WHERE;
                ReferentialColumn rrc = pReferential.Column[pReferential.IndexDataKeyField];
                if (TypeData.IsTypeString(rrc.DataType.value))
                    pDataKeyValue = DataHelper.SQLString(pDataKeyValue);
                SQLQuery += pReferential.Column[pReferential.IndexDataKeyField].ColumnName + "=" + pDataKeyValue;
            }
            return SQLQuery;
        }
        #endregion GetSQLDelete
        #region private GetSQLDeleteExternals
        /// <summary>
        /// Renvoie la requete SQL pour le delete des lignes de type externes (EXTLID par exemple)
        /// dont on passe la valeur du dataKeyField en arg
        /// </summary>
        /// <param name="pReferential">classe referential</param>
        /// <param name="pDataKeyValue">valeur du DataKeyfield a supprimer</param>
        /// <returns>requete DELETE</returns>
        private static string GetSQLDeleteExternals(Referential pReferential, string pDataKeyValue)
        {
            string SQLQuery = SQLCst.DELETE_DBO + Cst.OTCml_TBL.EXTLID.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "ID=";
            if (pReferential.IsDataKeyField_String)
                SQLQuery += DataHelper.SQLString(pDataKeyValue);
            else
                SQLQuery += pDataKeyValue;
            SQLQuery += SQLCst.AND + "TABLENAME=" + DataHelper.SQLString(pReferential.TableName);
            //
            return SQLQuery;
        }
        #endregion GetSQLDeleteExternals
        //
        #region public GetSQLSelect
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReferential">Classe referential</param>
        /// <param name="pSQLWhere">Where à implementer si existant</param>
        /// <param name="pIsForExecuteDataAdapter">Requete destinée à être passée à un pIsForExecuteDataAdapter (donc requête uniquement sur la table ppale</param>
        /// <param name="opDataTablesSQLChild">OUT requetes SQL des datatables enfants</param>
        /// <param name="opDataTablesChild">OUT datatables des tables enfants</param>
        /// <param name="isQueryWithSubTotal">OUT si la requête contient un Group by</param>
        /// <returns>SQL Query</returns>
        public static QueryParameters GetSQLSelect(SQLSelectParameters pSQLSelectParameters, out ArrayList opAlChildSQLselect, out ArrayList opAlChildTableName, out bool isQueryWithSubTotal)
        {
            QueryParameters sqlQuery;
            string sqlWhere;
            string sqlOrderby;
            SQL_Criteria sql_Criteria;

            GetSQL(pSQLSelectParameters,
                    out opAlChildSQLselect, out opAlChildTableName,
                    out sqlQuery, out sqlWhere, out sql_Criteria, out sqlOrderby, out isQueryWithSubTotal);

            return sqlQuery;
        }
        public static QueryParameters GetSQLSelect(SQLSelectParameters pSQLSelectParameters)
        {
            QueryParameters sqlQuery;
            ArrayList alChildSQLselect, alChildTableName;
            string sqlWhere;
            string sqlOrderby;
            bool isQueryWithSubTotal;
            SQL_Criteria sql_Criteria;

            GetSQL(pSQLSelectParameters,
                    out alChildSQLselect, out alChildTableName,
                    out sqlQuery, out sqlWhere, out sql_Criteria, out sqlOrderby, out isQueryWithSubTotal);

            return sqlQuery;
        }
        #endregion
        #region public GetSQLCriteria
        public static SQL_Criteria GetSQLCriteria(string pSource, Referential pReferential)
        {
            QueryParameters sqlQuery;
            string sqlWhere;
            string sqlOrderby;
            ArrayList dataTablesSQLChild;
            ArrayList dataTablesChild;
            SQL_Criteria sql_Criteria;
            bool isQueryWithSubTotal;

            GetSQL(pSource, SelectedColumnsEnum.All, pReferential, null,
                    false, false, null,
                    out dataTablesSQLChild, out dataTablesChild,
                    out sqlQuery, out sqlWhere, out sql_Criteria, out sqlOrderby, out isQueryWithSubTotal);

            return sql_Criteria;
        }
        #endregion
        #region public GetSQLWhere
        public static string GetSQLWhere(string pSource, Referential pReferential, string pSQLWhere)
        {
            QueryParameters sqlQuery;
            string sqlWhere;
            string sqlOrderBy;
            ArrayList dataTablesSQLChild;
            ArrayList dataTablesChild;
            SQL_Criteria sql_Criteria;
            bool isQueryWithSubTotal;

            GetSQL(pSource, SelectedColumnsEnum.All, pReferential, pSQLWhere,
                    false, false, null,
                    out dataTablesSQLChild, out dataTablesChild,
                    out sqlQuery, out sqlWhere, out sql_Criteria, out sqlOrderBy, out isQueryWithSubTotal);

            return sqlWhere;
        }
        #endregion
        #region public GetSQLOrderBy
        public static string GetSQLOrderBy(string pSource, Referential pReferential)
        {
            return GetSQLOrderBy(pSource, pReferential, false, false);
        }
        public static string GetSQLOrderBy(string pSource, Referential pReferential, bool pIsUseColumnAlias, bool pIsUseColumnAliasInOrderExpression)
        {
            QueryParameters sqlQuery;
            string sqlWhere;
            string sqlOrderBy;
            ArrayList dataTablesSQLChild;
            ArrayList dataTablesChild;
            SQL_Criteria sql_Criteria;
            bool isQueryWithSubTotal;

            GetSQL(pSource, SelectedColumnsEnum.All, pReferential, string.Empty,
                false, false, null,
                out dataTablesSQLChild, out dataTablesChild,
                out sqlQuery, out sqlWhere, out sql_Criteria, out sqlOrderBy, pIsUseColumnAlias, pIsUseColumnAliasInOrderExpression,
                out isQueryWithSubTotal);

            return sqlOrderBy;
        }
        #endregion
        #region public GetQuery_LoadReferential
        public static QueryParameters GetQuery_LoadReferential(SQLSelectParameters pSQLSelectParameters,
                                            string pColumn, string pColumnValue, bool pIsColumnDataKeyField, 
                                            bool pIsTableMainOnly,
                                            out ArrayList alChildSQLSelect, out ArrayList alChildTablename, out bool isQueryWithSubTotal)
        {
            QueryParameters ret = null;

            string sqlWhere = BuildSqlWhere(pSQLSelectParameters.referential, pColumn, pColumnValue, pIsColumnDataKeyField);

            //On récupère la query de la table ppale, ainsi que d'événtuelles queries child
            //ret = GetSQLSelect(pCS, pSelectedColumns, pReferential, sqlWhere, false, pIsSelectDistinct, pSQLHints,
            //                        out alChildSQLSelect, out alChildTablename, out isQueryWithSubTotal);
            pSQLSelectParameters.sqlWhere = sqlWhere;
            ret = GetSQLSelect(pSQLSelectParameters, out alChildSQLSelect, out alChildTablename, out isQueryWithSubTotal);

            if ((!pIsTableMainOnly) && pSQLSelectParameters.referential.HasMultiTable)
            {
                //On ajoute les queries pour les tables secondaires 
                for (int i = 0; i < alChildSQLSelect.Count; i++)
                {
                    ret.query += SQLCst.SEPARATOR_MULTISELECT;
                    ret.query += alChildSQLSelect[i].ToString();
                    if (StrFunc.IsFilled(sqlWhere))
                    {
                        //20111227 PL Suite à pb de taille sous Oracle
                        //if (alChildTablename[i].ToString().StartsWith("ex_"))
                        if (alChildTablename[i].ToString().StartsWith("e_"))
                        {
                            //"e_" --> Table EXTLID (ou EXTLIDS), donc la colonne "key" est tjs "ID"
                            if (sqlWhere.IndexOf("'NewRecord'") > 0) //Tip
                                ret.query += SQLCst.AND + sqlWhere;
                            else
                                ret.query += SQLCst.AND + "ID=" + BuildValueFormated(pSQLSelectParameters.referential, pColumnValue, pIsColumnDataKeyField);
                        }
                        else
                        {
                            //Autres tables (ex. ACTORROLE)
                            // 20110308 EG
                            if (sqlWhere.IndexOf("'NewRecord'") > 0) //Tip
                                ret.query += SQLCst.AND + sqlWhere;
                            else
                                ret.query += SQLCst.AND + SetAliasToSQLWhere(alChildTablename[i].ToString(), sqlWhere);
                        }
                    }
                }
                ret.query += SQLCst.SEPARATOR_MULTISELECT;
            }

            return ret;
        }
        #endregion
        //
        #region public GetQueryCountReferential
        /// <summary>
        /// Retourne la query qui permet de déterminer le nbr de lignes d'un referentiel 
        /// <remarks>
        /// <para>
        /// Format d'une query Referential: select ...from ....  order by 
        /// </para>
        /// <para>
        /// Format d'une query Count Referential: select 1 from ....
        /// </para>
        /// </remarks> 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pReferential"></param>
        /// <param name="pColumnFK"></param>
        /// <param name="pValueFK"></param>
        /// <returns></returns>
        public static QueryParameters GetQueryCountReferential(SQLSelectParameters pSQLSelectParameters,
                                        string pColumn, string pColumnValue)
        {
            ArrayList alChildSQLSelect;
            ArrayList alChildTablename;
            bool isQueryWithSubTotal;

            bool isTableMainOnly_True = true;
            bool isColumnDataKeyField_False = false;
            QueryParameters queryLoad = GetQuery_LoadReferential(pSQLSelectParameters,
                                                                    pColumn, pColumnValue, isColumnDataKeyField_False,
                                                                    isTableMainOnly_True,
                                                                    out alChildSQLSelect, out alChildTablename, out isQueryWithSubTotal);

            if ((pSQLSelectParameters.selectedColumns == SelectedColumnsEnum.All)
                || (pSQLSelectParameters.referential.CmptLevelSpecified && pSQLSelectParameters.referential.CmptLevel == "2.5"))
            {
                //Compatibilité avec le principe en vigueur en v2.5, principe encore utilisé pour les consultations LST où il est impossible actuellement d'identifier les jointures remontant plus d'une ligne. 
                string queryOrder = GetSQLOrderBy(pSQLSelectParameters.source, pSQLSelectParameters.referential);
                if (StrFunc.IsFilled(queryOrder))
                    queryLoad.query = queryLoad.query.Replace(queryOrder, string.Empty);

                queryLoad.query = SQLCst.SELECT + SQLCst.COUNT_1 + Cst.CrLf + SQLCst.X_FROM + "(" + queryLoad.query + ") tblGetQueryCountReferential";
            }

            return queryLoad;
        }
        #endregion
        #region public GetColumnSortInGroupBy
        // RD 20091102 / Utilisation de sqlColumn            
        public static string GetColumnSortInGroupBy(SQLReferentialColumn pSqlColumn,
            ref string pSql_SelectSort, ref string pSql_SelectGBSort)
        {
            // RD 20091214 / 16802/ LSTCOLUMN.SQLSELECT
            return GetColumnSortInGroupBy(pSqlColumn.SQLColumnNameOrSQLColumnSQLSelect, pSqlColumn.sqlColumnAlias,
            ref pSql_SelectSort, ref pSql_SelectGBSort);
        }
        public static string GetColumnSortInGroupBy(string pSqlColumnName, string pSqlColumnAlias,
            ref string pSql_SelectSort, ref string pSql_SelectGBSort)
        {
            string sqlGroupBySort = string.Empty;
            //
            //PLl 20101102 "_GroupBySort" --> "_GS" (pour éviter les alias > 30 car. sous Oracle)
            //sqlGroupBySort = pSqlColumnAlias + "_GroupBySort";
            sqlGroupBySort = pSqlColumnAlias + "_GS";
            //
            string selectSort = SQLCst.CASE + SQLCst.CASE_WHEN + " {0} " + SQLCst.IS_NULL;
            selectSort += SQLCst.CASE_THEN + "1" + SQLCst.CASE_ELSE + "0" + SQLCst.CASE_END;
            selectSort += SQLCst.AS + sqlGroupBySort + "," + Cst.CrLf;
            //
            pSql_SelectSort += StrFunc.AppendFormat(selectSort, pSqlColumnName);
            pSql_SelectGBSort += StrFunc.AppendFormat(selectSort, pSqlColumnAlias);
            //
            sqlGroupBySort += ",";
            //
            return sqlGroupBySort;
        }
        #endregion GetColumnSortInGroupBy
        // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
        #region public GetColumnNameOrColumnSelect
        public static string GetColumnNameOrColumnSelect(Referential pReferential, ReferentialSQLWhere pSQLWhere)
        {
            string tempAlias = (pSQLWhere.AliasTableNameSpecified ? pSQLWhere.AliasTableName : string.Empty);
            //
            return GetColumnNameOrColumnSelect(pReferential, pSQLWhere.ColumnName, tempAlias, pSQLWhere.ColumnNameOrColumnSQLSelectSpecified, pSQLWhere.ColumnNameOrColumnSQLSelect);
        }
        public static string GetColumnNameOrColumnSelect(ReferentialSQLOrderBy pSQLOrderBy)
        {
            ReferentialColumn rrc = null;
            return GetColumnNameOrColumnSelect(rrc, pSQLOrderBy.ColumnName, pSQLOrderBy.Alias, pSQLOrderBy.ColumnNameOrColumnSQLSelectSpecified, pSQLOrderBy.ColumnNameOrColumnSQLSelect);
        }
        public static string GetColumnNameOrColumnSelect(Referential pReferential, ReferentialSQLOrderBy pSQLOrderBy)
        {
            return GetColumnNameOrColumnSelect(pReferential, pSQLOrderBy.ColumnName, pSQLOrderBy.Alias, pSQLOrderBy.ColumnNameOrColumnSQLSelectSpecified, pSQLOrderBy.ColumnNameOrColumnSQLSelect);
        }
        public static string GetColumnNameOrColumnSelect(Referential pReferential, string pColumnName, string pAliasTableName, bool pColumnNameOrColumnSQLSelectSpecified, string pColumnNameOrColumnSQLSelect)
        {
            string ret = string.Empty;
            //
            ReferentialColumn rrc = pReferential[pColumnName, pAliasTableName];
            //
            ret = GetColumnNameOrColumnSelect(rrc, pColumnName, pAliasTableName, pColumnNameOrColumnSQLSelectSpecified, pColumnNameOrColumnSQLSelect);
            //
            return ret;
        }
        public static string GetColumnNameOrColumnSelect(string pColumnName, string pAliasTableName, bool pColumnNameOrColumnSQLSelectSpecified, string pColumnNameOrColumnSQLSelect)
        {
            ReferentialColumn rrc = null;
            return GetColumnNameOrColumnSelect(rrc, pColumnName, pAliasTableName, pColumnNameOrColumnSQLSelectSpecified, pColumnNameOrColumnSQLSelect);
        }

        public static string GetColumnNameOrColumnSelect(ReferentialColumn pRc, string pColumnName, string pAliasTableName, bool pColumnNameOrColumnSQLSelectSpecified, string pColumnNameOrColumnSQLSelect)
        {
            string ret = string.Empty;
            //
            if (null != pRc && pRc.ColumnNameOrColumnSQLSelectSpecified)
                ret = pRc.ColumnNameOrColumnSQLSelect;
            else if (pColumnNameOrColumnSQLSelectSpecified)
                ret = pColumnNameOrColumnSQLSelect;
            //
            if (StrFunc.IsEmpty(ret))
                ret = GetColumnNameExpression(pColumnName, pAliasTableName);
            //
            return ret;
        }

        /// <summary>
        /// <para>Retourne "{pColumnName}" si {pColumnName} contient déjà "." </para>
        /// <para>Retourne "{pAliasTableName}.{pColumnName}" sinon </para>
        /// </summary>
        /// <param name="pColumnName"></param>
        /// <param name="pAliasTableName"></param>
        /// <returns></returns>
        public static string GetColumnNameExpression(string pColumnName, string pAliasTableName)
        {
            string ret = pColumnName;
            if (StrFunc.IsFilled(pAliasTableName) && pColumnName.IndexOf(".") == -1)
                ret = pAliasTableName + "." + ret;
            //
            return ret;
        }
        #endregion GetColumnNameOrColumnSelect
        //
        // RD 20091102 
        // SetGroupBy() renommée en GetSqlGroupBy() 
        // et déplacée dans la nouvelle classe EFS.Referentiel.SQLReferentialColumn
        //
        #region private GetSQL
        /// <revision>
        ///     <version>3.1.0</version><date>20130102</date><author>PL</author>
        ///     <comment>
        ///     New surcharge with parameter SQLSelectParameters 
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>2.3.0</version><date>20090901</date><author>RD</author>
        ///     <comment>
        ///     1- New surcharge with parameter pIsWithColumnAlias
        ///     2- Manage Totals and sub-totals
        ///     </comment>
        /// </revision>
        /// <revision>
        ///     <version>2.3.0</version><date>20091102</date><author>RD</author>
        ///     <comment>
        ///     Utilisation d'un Objet SQLReferentialColumn
        ///     </comment>
        /// </revision>
        private static void GetSQL(SQLSelectParameters pSQLSelectParameters,
            out ArrayList opAlChildSQLselect, out ArrayList opAlChildTableName,
            out QueryParameters opSQLQuery, out string opSQLWhere, out SQL_Criteria opSQL_Criteria, out string opSQLOrderBy, out bool isQueryWithSubTotal)
        {
            GetSQL(pSQLSelectParameters.source, pSQLSelectParameters.selectedColumns, pSQLSelectParameters.referential, pSQLSelectParameters.sqlWhere,
                    pSQLSelectParameters.isForExecuteDataAdapter, pSQLSelectParameters.isSelectDistinct, pSQLSelectParameters.sqlHints,
                    out opAlChildSQLselect, out opAlChildTableName, out opSQLQuery, out opSQLWhere, out opSQL_Criteria, out opSQLOrderBy, false, false, out isQueryWithSubTotal);
        }
        private static void GetSQL(string pSource, SelectedColumnsEnum pSelectedColumns, Referential pReferential, string pSQLWhere,
            bool pIsForExecuteDataAdapter, bool pIsSelectDistinct, string pSQLHints,
            out ArrayList opAlChildSQLselect, out ArrayList opAlChildTableName,
            out QueryParameters opSQLQuery, out string opSQLWhere, out SQL_Criteria opSQL_Criteria, out string opSQLOrderBy, out bool isQueryWithSubTotal)
        {
            GetSQL(pSource, pSelectedColumns, pReferential, pSQLWhere, pIsForExecuteDataAdapter, pIsSelectDistinct, pSQLHints,
                    out opAlChildSQLselect, out opAlChildTableName, out opSQLQuery, out opSQLWhere, out opSQL_Criteria, out opSQLOrderBy, false, false, out isQueryWithSubTotal);
        }
        // EG 20110906 Add SQLCheckSelectedDefaultValueSpecified/SQLCheckSelectedDefaultValue
        // EG 20141020 [20442] Add GContractRole for DC to Invoicing context
        private static void GetSQL(string pSource, SelectedColumnsEnum pSelectedColumns, Referential pReferential, string pSQLWhere,
            bool pIsForExecuteDataAdapter, bool pIsSelectDistinct, string pSQLHints,
            out ArrayList opAlChildSQLselect, out ArrayList opAlChildTableName,
            out QueryParameters opSQLQuery, out string opSQLWhere, out SQL_Criteria opSQL_Criteria,
            out string opSQLOrderBy, bool pIsWithColumnAlias, bool pIsWithColumnAliasInOrderExpression, out bool isQueryWithSubTotal)
        {
            if (pReferential.CmptLevelSpecified && pReferential.CmptLevel == "2.5")
                pSelectedColumns = SelectedColumnsEnum.All;

            #region Initialisation
            #region Variables
            SQLWhere sqlWhere = new SQLWhere();
            string sql_Select = SQLCst.SELECT;
            if (StrFunc.IsFilled(pSQLHints) && DataHelper.isDbOracle(pSource))
                sql_Select += @"/*+ " + pSQLHints.Trim() + @" */ ";
            if (pIsSelectDistinct)
                sql_Select += SQLCst.DISTINCT.TrimStart(' ');
            string sql_Where = string.Empty;

            string sql_OrderBy = string.Empty;
            string sql_From = string.Empty;
            string sql_Join = string.Empty;
            string sql_Head_Join = string.Empty;
            string sql_Head_WhereJoin = string.Empty;
            string sql_Query = string.Empty;

            // 20090901 RD
            // Le but est de constituer une requête du genre:
            //
            // select sql_select from sql_from
            // union all
            // select sql_SelectGBFirst from (sql_SelectGB1 from sql_from group by sql_GroupBy1)
            // union all
            // select sql_SelectGBFirst from (sql_SelectGB2 from sql_from group by sql_GroupBy2)
            // ...
            // order by sql_orderby

            string sql_SelectGBFirst = sql_Select;
            string sql_SelectGB = sql_Select;
            string sql_GroupBy = string.Empty;
            string sql_SelectGBSort = string.Empty;
            string sql_SelectSort = string.Empty;
            //
            string sql_SelectAdditional = string.Empty;
            string sql_SelectGBAdditional = string.Empty;
            //
            // RD 20091102 / Utilisation de sqlColumn
            SQLReferentialColumn sqlColumn;
            //
            SQL_Criteria sql_Criteria = null;
            Cst.OTCml_TBL tblExtlId = (pReferential.IsDataKeyField_String ? Cst.OTCml_TBL.EXTLIDS : Cst.OTCml_TBL.EXTLID);
            #endregion Variables
            //
            string tableName = pReferential.TableName;
            //CC/PL 20120703 Utilisation de la table à la place de la vue (Vue et ADO.Net incompatible en Oracle®)
            if (pIsForExecuteDataAdapter)
            {
                if (tableName.StartsWith("VW_"))
                    tableName = tableName.Remove(0, 3);
                else if (tableName.StartsWith("EVW_"))
                    tableName = tableName.Remove(0, 4);
            }
            string aliasTableName = SQLCst.TBLMAIN;
            //
            if (pReferential.AliasTableNameSpecified)
                aliasTableName = pReferential.AliasTableName;
            //
            //20070511 FI 15925 => Nouvelle gestion du SQLSelect
            //if (pReferential.SQLSelectSpecified)
            //PL 20110627 Pour la gestion des référentiels basés sur une query dans l'élément SQLSelect (ex. QUOTE_ETD_H_DAILY)
            if ((!pIsForExecuteDataAdapter) && pReferential.SQLSelectSpecified)
                sql_From = SQLCst.X_FROM + "(" + pReferential.SQLSelectCommand + ") " + aliasTableName;
            else
                sql_From = SQLCst.FROM_DBO + tableName + " " + aliasTableName;
            //
            int nbColumn = 0;
            //
            string sql_DefaultOrderBy = string.Empty;
            int nbColumn_DefaultOrderBy = 0;

            //ArrayList listJoinTable = new ArrayList();

            opAlChildTableName = new ArrayList();
            opAlChildSQLselect = new ArrayList();
            //
            bool isWithGroupBy = false;

            // MF 20120430 ruptures with groupingset
            Cst.GroupingSet groupingSet = Cst.GroupingSet.Unknown;
            //
            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                if (pReferential.Column[i].GroupBySpecified)
                {
                    if (pReferential.Column[i].GroupBy.IsGroupBy)
                        isWithGroupBy = true;

                    // MF 20120430 ruptures with groupingset
                    groupingSet = pReferential.Column[i].GroupBy.GroupingSet | groupingSet;

                    //
                    if (isWithGroupBy && Cst.IsWithSubTotal(groupingSet))
                        break;
                }
            }
            //
            if (pReferential.SQLOrderBySpecified && (false == isWithGroupBy || false == Cst.IsWithTotalOrSubTotal(groupingSet)))
            {
                for (int i = 0; i < pReferential.SQLOrderBy.Length; i++)
                {
                    if (pReferential.SQLOrderBy[i].ColumnNotInReferential && pReferential.SQLOrderBy[i].GroupBySpecified)
                    {
                        if (pReferential.SQLOrderBy[i].GroupBy.IsGroupBy)
                            isWithGroupBy = true;
                        //
                        // MF 20120430 ruptures with groupingset
                        groupingSet = pReferential.SQLOrderBy[i].GroupBy.GroupingSet | groupingSet;
                        //
                        if (isWithGroupBy && Cst.IsWithSubTotal(groupingSet))
                            break;
                    }
                }
            }
            #endregion Initialisation

            #region for (int i=0;i<pReferential.Column.Length;i++)
            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                ReferentialColumn rrc = pReferential.Column[i];
                // RD 20091102 / Utilisation de sqlColumn
                sqlColumn = new SQLReferentialColumn();
                //
                if (rrc.IsExternal)
                {
                    bool isColumnUsedOnWhere = false;
                    if (pSelectedColumns == SelectedColumnsEnum.None)
                    {
                        isColumnUsedOnWhere = IsColumnUsedOnWhere(pReferential, rrc.AliasTableName);
                    }
                    if ((pSelectedColumns != SelectedColumnsEnum.None) || isColumnUsedOnWhere)//PLTest2012
                    {
                        #region rrc.IsExternal
                        string sqlRestriction = null;
                        string tblAlias = rrc.AliasTableName;//ex.: eaAAA avec e:Externalid, a:ACTOR, AAA:identifiant dans DEFINEEXTLID

                        //Constitution de la query sur la table pour les éventuelles MAJ
                        StrBuilder SQLSelectChild = new StrBuilder(SQLCst.SELECT);
                        SQLSelectChild += tblAlias + ".VALUE,";
                        SQLSelectChild += tblAlias + ".TABLENAME," + tblAlias + ".IDENTIFIER," + tblAlias + ".ID,";
                        SQLSelectChild += tblAlias + ".IDAINS," + tblAlias + ".DTINS,";
                        SQLSelectChild += tblAlias + ".IDAUPD," + tblAlias + ".DTUPD";
                        SQLSelectChild += Cst.CrLf;
                        SQLSelectChild += SQLCst.FROM_DBO + tblExtlId.ToString() + " " + tblAlias + Cst.CrLf;
                        if (!pIsForExecuteDataAdapter)
                        {
                            sqlRestriction = tblAlias + ".TABLENAME=" + DataHelper.SQLString(rrc.ExternalTableName);
                            sqlRestriction += SQLCst.AND;
                            sqlRestriction += tblAlias + ".IDENTIFIER=" + DataHelper.SQLString(rrc.ExternalIdentifier);
                            //
                            SQLSelectChild += SQLCst.WHERE + sqlRestriction;
                        }
                        opAlChildTableName.Add(tblAlias);
                        opAlChildSQLselect.Add(SQLSelectChild.ToString());

                        //Left join pour la query principale
                        if (!pIsForExecuteDataAdapter)
                        {
                            string sql_TmpJoin = SQLCst.LEFTJOIN_DBO + tblExtlId.ToString() + " " + tblAlias;
                            sql_TmpJoin += SQLCst.ON + "(";
                            sql_TmpJoin += tblAlias + ".ID=" + SQLCst.TBLMAIN + "." + pReferential.Column[pReferential.IndexDataKeyField].ColumnName;
                            sql_TmpJoin += SQLCst.AND + sqlRestriction;
                            sql_TmpJoin += ")";
                            //
                            sql_Join += Cst.CrLf + sql_TmpJoin;
                            //
                            // RD 20091102 / Utilisation de sqlColumn
                            // RD 20091223 / 16802/ LSTCOLUMN.SQLSELECT / Correction
                            sqlColumn.SetSqlColumnInfo(tblAlias + ".VALUE", "Col" + rrc.ExternalIdentifier);
                            //
                            sql_Select += Cst.CrLf + sqlColumn.SQLSelect + ",";
                            if (isWithGroupBy)
                                sqlColumn.GetSqlGroupBy(ref sql_SelectGBFirst, ref sql_SelectGB, ref sql_GroupBy);
                        }
                        #endregion rrc.IsExternal
                    }
                }
                else if (rrc.IsRole)
                {
                    if (pSelectedColumns != SelectedColumnsEnum.None)//PLTest2012
                    {
                        #region rrc.IsRole
                        string sqlRestriction = null;
                        string tblAlias = rrc.AliasTableName;                       //ex.: raSYSADMIN avec r:Role, a:ACTORROLE, SYSADMIN:role SYSADMIN
                        string tbl = pReferential.RoleTableName.Value;              //ex.:  "ACTORROLE"
                        string tblRole = "ROLE" + tbl.Substring(0, tbl.Length - 4); //ex.:  "ROLEACTOR"
                        string idcol = "ID" + tblRole;                              //ex.:  "IDROLEACTOR"
                        string idcolPK = pReferential.Column[pReferential.IndexDataKeyField].ColumnName; //ex.:  "IDA"

                        if (!rrc.DataField.StartsWith("IDA"))//Glop en DUR
                        {
                            //Constitution de la query sur la table pour les éventuelles MAJ
                            string SQLSelectChild = SQLCst.SELECT;
                            SQLSelectChild += "{0}." + idcol + ",{0}." + idcolPK + " as ID,";
                            if (tbl == Cst.OTCml_TBL.ACTORROLE.ToString())
                                SQLSelectChild += "{0}.IDA_ACTOR,";//Car inclu dans la AK
                            else if ((tbl == Cst.OTCml_TBL.GINSTRROLE.ToString()) || (tbl == Cst.OTCml_TBL.GCONTRACTROLE.ToString()))
                                SQLSelectChild += "{0}.IDA,";//Car inclu dans la UK
                            SQLSelectChild += "{0}.DTENABLED,";//Car not null
                            SQLSelectChild += "{0}.IDAINS,{0}.DTINS,{0}.IDAUPD,{0}.DTUPD" + Cst.CrLf;
                            SQLSelectChild += SQLCst.FROM_DBO + tbl + " {0}" + Cst.CrLf;
                            if (!pIsForExecuteDataAdapter)
                            {
                                sqlRestriction = "{0}." + idcol + "=" + DataHelper.SQLString(tblAlias.Substring(2));
                                SQLSelectChild += SQLCst.WHERE + sqlRestriction;
                            }
                            opAlChildTableName.Add(tblAlias);
                            opAlChildSQLselect.Add(String.Format(SQLSelectChild, tblAlias));
                        }
                        //NB: Left join pour la query principale: Aucun car données absentes du Datagrid
                        #endregion rrc.IsRole
                    }
                }
                else if (rrc.IsItem)
                {
                    #region rrc.IsItem
                    //rrc.AliasTableName: Constitué de 3 données (cf. DeserializeXML_ForModeRW())
                    //ex.: ii99 avec "i" en dur pour "Item", premier caractère lower() de la table (ex. "i" pour INSTRUMENT), valeur de la donnée (ex. 99 pour un IDI)
                    string tblAlias = rrc.AliasTableName;
                    string dataValue = rrc.AliasTableName.Substring(2);
                    string idcolPK = pReferential.Column[pReferential.IndexDataKeyField].ColumnName; //ex.:  "IDGINSTR" pour le référentiel "GINSTR"

                    //Constitution de l'ordre SQL SELECT sur la table concernée
                    string SQLSelectChild = SQLCst.SELECT;
                    SQLSelectChild += "{0}." + pReferential.Items.columnname + ",{0}." + idcolPK + " as ID,{0}.IDAINS,{0}.DTINS,{0}.IDAUPD,{0}.DTUPD";
                    if (pReferential.Items.addcolumnsSpecified && !String.IsNullOrEmpty(pReferential.Items.addcolumns))
                    {
                        SQLSelectChild += ",{0}." + pReferential.Items.addcolumns.Replace(",", ",{0}.");
                    }
                    SQLSelectChild += Cst.CrLf;
                    SQLSelectChild += SQLCst.FROM_DBO + pReferential.Items.tablename + " {0}" + Cst.CrLf;
                    if (!pIsForExecuteDataAdapter)
                    {
                        // EG 20170327 Test DataType
                        //SQLSelectChild += SQLCst.WHERE + "{0}." + pReferential.Items.columnname + "=" + DataHelper.SQLString(dataValue);
                        SQLSelectChild += SQLCst.WHERE + "{0}." + pReferential.Items.columnname + "=";
                        if (TypeData.IsTypeString(rrc.DataType.value))
                            SQLSelectChild += DataHelper.SQLString(dataValue);
                        else if (TypeData.IsTypeNumeric(rrc.DataType.value))
                        {
                            SQLSelectChild += dataValue.Replace("_","-");
                        }
                    }
                    opAlChildTableName.Add(tblAlias);
                    opAlChildSQLselect.Add(String.Format(SQLSelectChild, tblAlias));

                    //NB: Left join pour la query principale: Aucun, car données non présentes dans le Datagrid.
                    #endregion rrc.IsItem
                }
                else
                {
                    #region !rrc.IsAdditionalData
                    #region Compteur de colonnes
                    if (nbColumn % 10 == 0)
                    {
                        sql_Select += Cst.CrLf;
                        //
                        if (isWithGroupBy)
                        {
                            sql_SelectGBFirst += Cst.CrLf;
                            sql_SelectGB += Cst.CrLf;
                        }
                    }
                    //
                    nbColumn++;
                    #endregion
                    //
                    if (TypeData.IsTypeImage(rrc.DataType.value))
                    {
                        #region Cas particulier des colonnes de type Image
                        //20100506 PL Add if (!pIsForExecuteDataAdapter), car le CaseWhenIsNull pose pb en Oracle où ODP.Net génère un parameter mais pas la colonne
                        //            NB: On pourr as epsoser la question d'ailleurs s'il est utile de conserver cette colonne dans le select de load du datagrid ?   
                        if (!pIsForExecuteDataAdapter)
                        {
                            // RD 20091102 / Utilisation de sqlColumn
                            sqlColumn.ConstructSqlColumn(pSource, rrc, false, false, false);
                            //
                            // RD 20091029 - PL 20091029
                            // Pas de chargement des donnée de Type Image pour des raisons de performances, 
                            // en plus une donnée de type Image n'est affichée ni sur le réferential ni sur le formulaire
                            //
                            // RD 20091102 / Le "case when" est demandé par FI, pour une exploitation dans le chargement des Confirm en Zip
                            // RD 20091214 / 16802/ LSTCOLUMN.SQLSELECT
                            string sqlTextColumn = sqlColumn.GetSqlColumnName_CaseWhenIsNull("Binary");
                            //
                            sql_Select += sqlTextColumn + ", ";
                            //
                            if (isWithGroupBy)
                                sqlColumn.GetSqlGroupBy(sqlTextColumn, ref sql_SelectGBFirst, ref sql_SelectGB); // La colonne de type IMAGE n'est pas incluse dans la clause Group by
                        }
                        #endregion
                    }
                    else if (TypeData.IsTypeText(rrc.DataType.value) &&
                            (pIsSelectDistinct ||
                            (pReferential.IsGrid && rrc.IsHideInDataGridSpecified && rrc.IsHideInDataGrid && rrc.LengthInDataGridSpecified && (rrc.LengthInDataGrid == -1))))
                    {
                        #region Cas particulier des colonnes de type Text
                        // RD 20100506 Idem que la correction de Pascal ci-dessus ( problème du CaseWhenIsNull en Oracle/ODP.Net)
                        if (!pIsForExecuteDataAdapter)
                        {
                            // RD 20091102 / Utilisation de sqlColumn
                            sqlColumn.ConstructSqlColumn(pSource, rrc, false, false, false);
                            //
                            //Warning: En mode "select" pas d'affichage des données Text car incompatibilté avec un "select distinct"
                            //
                            // 20091030 RD
                            // Pour des raisons de performances, Pas de chargement des données :
                            // 1- En mode DataGrid ( par contre bien les charger en mode formulaire)
                            // 2- ET De type Text 
                            // 3- ET avec IsHideInDataGrid = True
                            // 4- ET avec LengthInDataGrid = -1
                            //
                            // RD 20091102 / Le "case when" est demandé par FI, pour une exploitation dans le chargement des Confirm en Zip
                            // RD 20091214 / 16802/ LSTCOLUMN.SQLSELECT
                            string sqlTextColumn = sqlColumn.GetSqlColumnName_CaseWhenIsNull("Text");
                            //
                            sql_Select += sqlTextColumn + ", ";
                            //
                            if (isWithGroupBy)
                                sqlColumn.GetSqlGroupBy(sqlTextColumn, ref sql_SelectGBFirst, ref sql_SelectGB); // La colonne de type TEXT n'est pas incluse dans la clause Group by
                        }
                        #endregion
                    }
                    else
                    {
                        #region Select
                        // RD 20091102 / Utilisation de sqlColumn
                        sqlColumn.ConstructSqlColumn(pSource, rrc, true, pIsWithColumnAlias, pIsWithColumnAliasInOrderExpression);

                        bool isColumnToAddInSelect = false;

                        if (pIsForExecuteDataAdapter)
                        {
                            if (rrc.IsAliasEqualToMasterAliasTableName(pReferential.AliasTableName) && rrc.IsNotVirtualColumn)
                                isColumnToAddInSelect = true;
                        }
                        else
                        {
                            isColumnToAddInSelect = true;

                            // RD 20130319 [18508] --------------------------------------------------------------------------------------
                            // Ce problème apparait sur les référentiels avec des colonnes cachées sur le DataGrid (<IsHideInDataGrid>true</IsHideInDataGrid>)
                            // Car ce n'est pas les mêmes requêtes qui sont générées pour:
                            // - le Chargement du DataGrid 
                            // - et pour l'Enregistrement des modifications (ExecuteDataAdapter).
                            //
                            // Correction apportée en v2.x uniquement : Mettre en commentaire le code ci-dessous
                            // -----------------------------------------------------------------------------------------------------------
                            //if (pSelectedColumns != SelectedColumnsEnum.All)//PLTest2012
                            //{
                            //    bool isColumnUsedOnWhere = rrc.ExistsRelation && IsColumnUsedOnWhere(pReferential, rrc.Relation[0].AliasTableName);
                            //    if (!isColumnUsedOnWhere)
                            //    {
                            //        if (pSelectedColumns == SelectedColumnsEnum.None)
                            //            isColumnToAddInSelect = false;
                            //        //PL 20120831 Add test on !rrc.IsHide (si une colonne est IsHideInDataGrid et IsHide, c'est qu'elle est destinée à un usage particulier, docn on la load)
                            //        //else if ((rrc.IsHideInDataGrid) && (!rrc.IsDataKeyField) && (rrc.ColumnName != "ROWATTRIBUT") && (rrc.ColumnName != "ROWVERSION"))
                            //        else if ((rrc.IsHideInDataGrid && !rrc.IsHide) && (!rrc.IsDataKeyField) && (rrc.ColumnName != "ROWATTRIBUT") && (rrc.ColumnName != "ROWVERSION"))
                            //            isColumnToAddInSelect = false;

                            //        if (!isColumnToAddInSelect)
                            //        {
                            //            sql_Select += SQLCst.NULL + SQLCst.AS + sqlColumn.sqlColumnAlias + ",";
                            //            System.Diagnostics.Debug.WriteLine(sqlColumn.sqlColumnAlias);
                            //        }
                            //    }
                            //}
                        }

                        // 20090828 RD / Pour alléger le code
                        if (isColumnToAddInSelect)
                        {
                            // RD 20091102 / Utilisation de sqlColumn
                            sql_Select += sqlColumn.SQLSelect + ",";

                            if (isWithGroupBy)
                                sqlColumn.GetSqlGroupBy(ref sql_SelectGBFirst, ref sql_SelectGB, ref sql_GroupBy);
                        }
                        #endregion Select

                        #region ExistsRelation
                        if ((!pIsForExecuteDataAdapter) && (rrc.ExistsRelation))
                        {
                            // RD 20091102 / Utilisation de sqlColumn
                            string columnNameForRelation = sqlColumn.sqlColumnName;

                            #region Compteur de colonnes
                            if (nbColumn % 10 == 0)
                            {
                                sql_Select += Cst.CrLf;
                                //
                                if (isWithGroupBy)
                                {
                                    sql_SelectGBFirst += Cst.CrLf;
                                    sql_SelectGB += Cst.CrLf;
                                }
                            }
                            //
                            nbColumn++;
                            #endregion

                            string aliasTableJoin = rrc.Relation[0].AliasTableName;
                            // FI 20171025 [23533]  devient une property
                            //rrc.Relation[0].RelationColumnSQLName = string.Empty;

                            //// RD 20091223 / 16802/ LSTCOLUMN.SQLSELECT / Correction
                            //if (-1 == rrc.Relation[0].ColumnSelect[0].ColumnName.IndexOf("."))
                            //    rrc.Relation[0].RelationColumnSQLName = aliasTableJoin + "_" + rrc.Relation[0].ColumnSelect[0].ColumnName;
                            //else
                            //    rrc.Relation[0].RelationColumnSQLName = rrc.Relation[0].ColumnSelect[0].ColumnName;

                            //sqlColumn.SetSqlColumnInfo(rrc.Relation[0].ColumnSelect[0].ColumnName, rrc.Relation[0].RelationColumnSQLName.ToUpper(), aliasTableJoin);

                            if ((pSelectedColumns != SelectedColumnsEnum.All) && (!isColumnToAddInSelect)
                                //PL Cas spécifiques... (géré en dur)
                                && (rrc.Relation[0].AliasTableName != "vw_tiu2" /* FEESCHEDULE */)
                                && (rrc.Relation[0].AliasTableName != "vw_ti2"  /* FEESCHEDULE */)
                                && (rrc.Relation[0].AliasTableName != "vw_tc2"  /* FEESCHEDULE */)
                                && (rrc.Relation[0].AliasTableName != "vw_2"    /* ACCKEYVALUE, ... */)
                                && (rrc.Relation[0].AliasTableName != "vw_mc2"  /* CNFMESSAGE, TS_CNFMESSAGE */)
                                && (rrc.Relation[0].AliasTableName != "vw_tm2"  /* INVOICINGRULES */)
                                && (rrc.Relation[0].AliasTableName != "evenum"  /* TAXEVENT */)
                                && (rrc.Relation[0].TableName != "PRODUCT"      /* INSTRUMENT */)
                                )
                            {
                                //PLTest2012
                                sql_Select += SQLCst.NULL + SQLCst.AS + sqlColumn.sqlColumnAlias + ",";

                                System.Diagnostics.Debug.WriteLine("--> " + sqlColumn.sqlColumnAlias);
                            }
                            else
                            {
                                sql_Select += sqlColumn.SQLSelect + ",";

                                if (isWithGroupBy)
                                    sqlColumn.GetSqlGroupBy(ref sql_SelectGBFirst, ref sql_SelectGB, ref sql_GroupBy);

                                // RD 20111230 Pour ne pas remplacer rrc.IsOrderBy.order s'il est spécifié dans le XML
                                if ((rrc.IsOrderBySpecified == false) || (rrc.IsOrderBy.orderSpecified == false))
                                    sqlColumn.SetSqlColumnOrderBy(pIsWithColumnAlias);

                                sql_Join += Cst.CrLf;
                                //PL 20081209 Utilisation systématique de LEFTJOIN_DBO (Utile pour les colonnes IsMandatory parfois Disabled, ex.: GINSTRROLE.IDA)
                                //sql_Join += (rrc.IsMandatory ? SQLCst.INNERJOIN_DBO:SQLCst.LEFTJOIN_DBO) + tableJoin + " " + aliasTableJoin; 
                                //sql_Join += SQLCst.LEFTJOIN_DBO + rrc.Relation[0].TableName + (rrc.Relation[0].TableName == aliasTableJoin ? string.Empty : " " + aliasTableJoin);
                                //PL 20151127 WARNING! Réutilisation de IsMandatory pour définir INNER JOIN ou LEFTJOIN_DBO 
                                //                     L'usage du LEFT modifie dans certains cas le plan d'exécution, dégradant les performances (ex. Consulation des événements depuis un trade - EVENTASSET.xml)
                                //                     Par ailleurs, GINSTRROLE.IDA dispose aujourd'hui de IsMandatory=false
                                //                     A voir à l'usage si certains référentiels posent problème... 
                                sql_Join += (rrc.IsMandatory ? SQLCst.INNERJOIN_DBO : SQLCst.LEFTJOIN_DBO)
                                          + rrc.Relation[0].TableName + (rrc.Relation[0].TableName == aliasTableJoin ? string.Empty : " " + aliasTableJoin);
                                sql_Join += SQLCst.ON + "(" + aliasTableJoin + "." + rrc.Relation[0].ColumnRelation[0].ColumnName + "=" + columnNameForRelation + ")";
                                #region Apply SQL Condition on main query
                                //------------------------------------------------------------
                                //PL 20100913 Add
                                //------------------------------------------------------------
                                if (rrc.Relation[0].Condition != null)
                                {
                                    for (int nbCondition = 0; nbCondition < rrc.Relation[0].Condition.Length; nbCondition++)
                                    {
                                        if (rrc.Relation[0].Condition[nbCondition].applySpecified
                                            && rrc.Relation[0].Condition[nbCondition].apply == "ALL"
                                            && rrc.Relation[0].Condition[nbCondition].SQLWhereSpecified)
                                        {
                                            sql_Join += SQLCst.AND + "(";
                                            //PL 20110914
                                            if (!rrc.Relation[0].Condition[nbCondition].SQLWhere.StartsWith("("))
                                                sql_Join += aliasTableJoin + ".";//PL: Bidouille à revoir...
                                            sql_Join += rrc.Relation[0].Condition[nbCondition].SQLWhere + ")";
                                        }
                                    }
                                }
                                //------------------------------------------------------------
                                #endregion Apply SQL Condition on main query
                            }
                        }
                        #endregion ExistsRelation

                        if (!pIsForExecuteDataAdapter)
                        {
                            #region sql_OrderBy & sql_DefaultOrderBy
                            if (rrc.IsOrderBySpecified
                                &&
                                (
                                (BoolFunc.IsTrue(rrc.IsOrderBy.Value))
                                ||
                                (rrc.IsOrderBy.Value == SQLCst.ASC.Trim())
                                ||
                                (rrc.IsOrderBy.Value == SQLCst.DESC.Trim())
                                )
                               )
                            {
                                // RD 20091102 / Utilisation de sqlColumn
                                if (isWithGroupBy)
                                    sql_OrderBy += GetColumnSortInGroupBy(sqlColumn, ref sql_SelectSort, ref sql_SelectGBSort);
                                //
                                //if (rrc.IsOrderBy.orderSpecified)
                                //{
                                //    sql_OrderBy += sqlColumn.sqlColumnOrderBy + ",";
                                //}
                                //else
                                //{
                                //    sql_OrderBy += sqlColumn.sqlColumnOrderBy;// RD 20091102 / Utilisation de sqlColumn
                                //    if (rrc.IsOrderBy.Value.ToLower() == SQLCst.DESC.Trim())
                                //        sql_OrderBy += SQLCst.DESC;
                                //    sql_OrderBy += ",";
                                //}
                                //PL 20110708 Debug et Refactoring
                                sql_OrderBy += sqlColumn.sqlColumnOrderBy;
                                if (rrc.IsOrderBy.Value.ToLower() == SQLCst.DESC.Trim())
                                    sql_OrderBy += SQLCst.DESC;
                                sql_OrderBy += ",";
                            }
                            //20090429 PL Ajout test sur !pIsSelectDistinct pour éviter un bug SQL (Distinct / Order By)
                            //20090812 FI le bug is Les éléments ORDER BY doivent se retrouver dans la liste de sélection si SELECT DISTINCT est spécifié. 
                            else if ((!pIsSelectDistinct)
                                && (nbColumn_DefaultOrderBy < 1)
                                && (rrc.IsHideInDataGridSpecified && !rrc.IsHideInDataGrid))
                            {
                                if (nbColumn_DefaultOrderBy != 0)
                                    sql_DefaultOrderBy += ",";
                                //
                                // RD 20091102 / Utilisation de sqlColumn
                                if (isWithGroupBy)
                                    sql_DefaultOrderBy += GetColumnSortInGroupBy(sqlColumn, ref sql_SelectSort, ref sql_SelectGBSort);
                                //
                                //20101203 FI
                                sql_DefaultOrderBy += sqlColumn.sqlColumnOrderBy + SQLCst.ASC;
                                //
                                nbColumn_DefaultOrderBy++;
                            }
                            #endregion
                        }
                    }
                    #endregion !rrc.IsAdditionalData
                }
            }
            #endregion for (int i=0;i<pReferential.Column.Length;i++)

            if (StrFunc.IsFilled(pSQLWhere))
            {
                //20070529 PL Astuce pour ne pas charger le Dataset en mode "création" ( cf. InitializeReferentialForForm_2() )
                if (pSQLWhere.IndexOf("'NewRecord'") > 0)
                {
                    sqlWhere.Append(pSQLWhere);
                }
                else
                {
                    //20081110 PL Refactoring pour le "."
                    sqlWhere.Append(SetAliasToSQLWhere(pReferential, pSQLWhere));
                }
            }
            //
            if (!pIsForExecuteDataAdapter)
            {
                if (SessionTools.IsRequestTrackEnabled && pReferential.RequestTrackSpecified)
                    CheckReferentialRequestTrack(pReferential.RequestTrack);

                #region SQLJoinSpecified
                if (pReferential.SQLJoinSpecified)
                {
                    for (int i = 0; i < pReferential.SQLJoin.Length; i++)
                    {
                        if (pReferential.SQLJoin[i] != null)
                        {
                            if (pSelectedColumns == SelectedColumnsEnum.None) //PLTest2012
                            {
                                if (!pReferential.SQLJoin[i].Trim().StartsWith(SQLCst.X_LEFT.Trim()))
                                    sql_Head_Join += Cst.CrLf + pReferential.SQLJoin[i];
                            }
                            else
                            {
                                sql_Head_Join += Cst.CrLf + pReferential.SQLJoin[i];
                            }
                        }
                    }
                }
                #endregion SQLJoinSpecified
                #region SQLWhereSpecified
                if (pReferential.SQLWhereSpecified)
                {
                    string source = SessionTools.CS;
                    bool isExistCriteria = false;
                    int currentArg = 0;
                    string colDataType = string.Empty;
                    string tmpColumn = string.Empty;
                    string tempColumnNameOrColumnSQLSelect = string.Empty;
                    string tmpOperator = string.Empty;
                    string tmpValue = string.Empty;
                    string tmpColumnSqlWhere = string.Empty;
                    string[] tmpValues;
                    //
                    SQL_ColumnCriteria[] sql_ColumnCriteria = null;
                    if (pReferential.SQLWhere.Length > 0)
                        sql_ColumnCriteria = new SQL_ColumnCriteria[pReferential.SQLWhere.Length];
                    //
                    for (int i = 0; i < pReferential.SQLWhere.Length; i++)
                    {
                        ReferentialSQLWhere rrw = pReferential.SQLWhere[i];
                        if (null != rrw)
                        {
                            #region SQLJoinSpecified
                            if (rrw.SQLJoinSpecified)
                            {
                                for (int iJoin = 0; iJoin < rrw.SQLJoin.Length; iJoin++)
                                {
                                    if (rrw.SQLJoin[iJoin] != null)
                                        sql_Head_WhereJoin += Cst.CrLf + rrw.SQLJoin[iJoin];
                                }
                            }
                            #endregion SQLJoinSpecified
                            #region ColumnNameSpecified
                            if (rrw.ColumnNameSpecified)
                            {
                                isExistCriteria = true;
                                //
                                // 20091021 RD
                                ReferentialColumn rrc = pReferential[rrw.ColumnName, rrw.AliasTableName];
                                if (null != rrc)
                                    tmpColumnSqlWhere = (rrc.ColumnSqlWhereSpecified ? rrc.ColumnSqlWhere : (rrw.ColumnSQLWhereSpecified ? rrw.ColumnSQLWhere : string.Empty));
                                else
                                    tmpColumnSqlWhere = (rrw.ColumnSQLWhereSpecified ? rrw.ColumnSQLWhere : string.Empty);
                                //
                                tmpColumn = rrw.ColumnName;
                                if (rrw.AliasTableNameSpecified && rrw.AliasTableName.Length > 0)
                                {
                                    tmpColumn = rrw.AliasTableName + "." + tmpColumn;
                                    tmpColumnSqlWhere = tmpColumnSqlWhere.Replace(Cst.DYNAMIC_ALIASTABLE, rrw.AliasTableName);
                                }
                                //
                                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                                tempColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameOrColumnSelect(pReferential, rrw);
                                //
                                #region LstValue
                                if (StrFunc.IsFilled(rrw.LstValue))
                                {
                                    if (rrw.LstValue == Cst.DA_DEFAULT)
                                    {
                                        // FI 201003 [pReferential.dynamicArgs n'est constitué que de donnée au format XML]
                                        /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
                                        //EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), pReferential.dynamicArgs[currentArg]);
                                        //StringDynamicData sDa = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
                                        StringDynamicData sDa = RepositoryTools.DeserializeDA(pReferential.xmlDynamicArgs[currentArg]);
                                        tmpValue = sDa.value;
                                        currentArg++;
                                    }
                                    else
                                    {
                                        tmpValue = rrw.LstValue;
                                    }
                                    //tmpValues = tmpValue.Split(";".ToCharArray());
                                    tmpValues = StrFunc.StringArrayList.StringListToStringArray(tmpValue);
                                    //Gestion des éventuels "Dynamic Argument"
                                    for (int j = 0; j < tmpValues.Length; j++)
                                    {
                                        if (tmpValues[j].StartsWith(Cst.DA_START))
                                            tmpValues[j] = pReferential.ReplaceDynamicArgument(pSource, tmpValues[j]);
                                    }
                                    tmpValue = ArrFunc.GetStringList(tmpValues, ";");
                                }
                                else
                                {
                                    tmpValue = string.Empty;
                                }
                                #endregion LstValue
                                //

                                SQL_ColumnCriteriaDataType sqlDatatype = new SQL_ColumnCriteriaDataType();
                                SQL_ColumnCriteriaInput sqlDataInput = new SQL_ColumnCriteriaInput();
                                TypeData.TypeDataEnum datatype = TypeData.GetTypeDataEnum(rrw.DataType.value);
                                if (datatype == TypeData.TypeDataEnum.datetime && (rrw.DataType.datakindSpecified && rrw.DataType.datakind == Cst.DataKind.Timestamp)
                                    && rrw.DataType.tzdbidSpecified)
                                {
                                    sqlDatatype = new SQL_ColumnCriteriaDataType(datatype, rrw.DataType.tzdbid);
                                    sqlDataInput = new SQL_ColumnCriteriaInput(tmpValue, SessionTools.GetCriteriaTimeZone());
                                }
                                else
                                {
                                    sqlDatatype = new SQL_ColumnCriteriaDataType(datatype);
                                    sqlDataInput = new SQL_ColumnCriteriaInput(tmpValue);
                                }

                                sql_ColumnCriteria[i] = new SQL_ColumnCriteria(sqlDatatype, tempColumnNameOrColumnSQLSelect, tmpColumnSqlWhere, rrw.Operator, sqlDataInput);
                            }
                            #endregion ColumnNameSpecified
                            #region SQLWhereSpecified
                            if (rrw.SQLWhereSpecified)
                            {
                                // EG 20120503
                                if (rrw.SQLWhere.Contains(Cst.DA_DEFAULT))
                                {
                                    /// EG 201306026 Appel à la méthode de Désérialisation d'un StringDynamicData en chaine
                                    //EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(StringDynamicData), pReferential.dynamicArgs[currentArg]);
                                    //StringDynamicData sDa = (StringDynamicData)CacheSerializer.Deserialize(serializerInfo);
                                    StringDynamicData sDa = RepositoryTools.DeserializeDA(pReferential.xmlDynamicArgs[currentArg]);
                                    sqlWhere.Append(rrw.SQLWhere.Replace(Cst.DA_DEFAULT, sDa.value));
                                    currentArg++;
                                }
                                else
                                    sqlWhere.Append(rrw.SQLWhere);
                            }
                            #endregion SQLWhereSpecified
                        }
                    }//end for
                    //
                    if (isExistCriteria)
                    {
                        // MF 20120410 collation strategy using ICultureParameter
                        // ticket 17743 blocked activity to make Oracle case insensitive via globalization parameters
                        //bool applySimpleCollation = DataHelper.MayUseUpper(SessionTools.CS);

                        bool applySimpleCollation = SystemSettings.IsCollationCI();
                        sql_Criteria = new SQL_Criteria(sql_ColumnCriteria, applySimpleCollation);
                        sqlWhere.Append(sql_Criteria.GetSQLWhere2(pSource, SQLCst.TBLMAIN));
                    }
                }
                #endregion SQLWhereSpecified
                #region isValidDataOnly & isDailyUpdDataOnly
                // RD 20120131 
                // Afficher uniquement les données valides
                // EG 20120202 utilisation de aliasTableName à la place de tblMain
                if (pReferential.isValidDataOnly)
                    sqlWhere.Append(OTCmlHelper.GetSQLDataDtEnabled(pSource, aliasTableName, true));
                // Afficher uniquement les données mises à jour aujourd’hui (créés ou modifiés)
                if (pReferential.isDailyUpdDataOnly)
                    sqlWhere.Append(OTCmlHelper.GetSQLDataDtUpd(pSource, aliasTableName));
                #endregion
                #region SQLOrderBySpecified
                if (pReferential.SQLOrderBySpecified)
                {
                    //Il existe un tri défini par l'utilisateur (Table LSTORDERBY)
                    sql_OrderBy = string.Empty;
                    sql_SelectSort = string.Empty;
                    sql_SelectGBSort = string.Empty;
                    //
                    for (int i = 0; i < pReferential.SQLOrderBy.Length; i++)
                    {
                        if (pReferential.SQLOrderBy[i] != null)
                        {
                            if (i != 0)
                                sql_OrderBy += ",";
                            //
                            if (pIsWithColumnAlias)
                                sql_OrderBy += pReferential.SQLOrderBy[i].ValueWithAlias;
                            else
                                sql_OrderBy += pReferential.SQLOrderBy[i].Value;
                            //
                            if (StrFunc.IsFilled(pReferential.SQLOrderBy[i].GroupBySort))
                            {
                                sql_SelectSort += pReferential.SQLOrderBy[i].GroupBySort;
                                sql_SelectGBSort += pReferential.SQLOrderBy[i].GroupBySortWithAlias;
                            }
                            // 20090928 RD / Add column in select Clause if does not exist
                            if (pReferential.SQLOrderBy[i].ColumnNotInReferential &&
                                false == TypeData.IsTypeImage(pReferential.SQLOrderBy[i].DataType) &&
                                false == TypeData.IsTypeText(pReferential.SQLOrderBy[i].DataType))
                            {
                                // RD 20091102 / Utilisation de sqlColumn
                                sqlColumn = new SQLReferentialColumn();
                                //
                                // RD 20091216 / 16802/ LSTCOLUMN.SQLSELECT
                                sqlColumn.ConstructSqlColumn(pReferential.SQLOrderBy[i]);
                                //  
                                sql_Select += sqlColumn.SQLSelect + ", ";
                                //
                                if (isWithGroupBy)
                                    sqlColumn.GetSqlGroupBy(pReferential.SQLOrderBy[i].GroupBy, ref sql_SelectGBFirst, ref sql_SelectGB, ref sql_GroupBy);
                            }
                        }
                    }
                }
                if ((sql_OrderBy.Length == 0) && (nbColumn_DefaultOrderBy > 0))
                {
                    //None order --> Set default order
                    sql_OrderBy = sql_DefaultOrderBy;
                }
                //
                #endregion SQLOrderBySpecified
                #region UseStatistic
                //region UseStatistic
                if (pReferential.UseStatisticSpecified && pReferential.UseStatistic)
                {
                    //FI 20101202 Reecriture => si pIsWithColumnAlias, il faut faire un order by l'alias
                    //string tmpOrderBy = sql_OrderBy.Replace(SQLCst.ORDERBY, string.Empty);
                    ////
                    //sql_OrderBy = OTCmlHelper.GetSQLOrderBy_Statistic(pSource, tableName);
                    ////
                    //if (pIsWithColumnAlias)
                    //    sql_OrderBy = sql_OrderBy.Replace(tableName + "_S.", tableName + "_S_");
                    ////
                    //if (tmpOrderBy.Length > 0)
                    //    sql_OrderBy += "," + tmpOrderBy;

                    //PLTest2012 A étudier concernant les Statistic
                    sql_Join += Cst.CrLf + OTCmlHelper.GetSQLJoin_Statistic(tableName, aliasTableName);

                    if (pIsWithColumnAlias)
                        sql_OrderBy += tableName + "_S_USEFREQUENCY";
                    else
                        sql_OrderBy = OTCmlHelper.GetSQLOrderBy_Statistic(pSource, tableName, sql_OrderBy);
                }
                #endregion UseStatistic
                //
                // 20090805 RD 
                // Ajouter de nouvelles colonnes dynamiques :
                //  - ROWSTYLE   : Le style des lignes 
                //  - ROWSTATE   : Le contenu de la colonne caractérisant la ligne
                //  - ISSELECTED : Pour gérer la sélection des lignes une par une à travers une CheckBox sur chaque ligne
                //
                #region sql_SelectAdditional for SQLRowStyle, SQLRowState and sqlIsSelected



                //
                string sqlRowStyle = string.Empty;
                if (pReferential.SQLRowStyleSpecified && StrFunc.IsFilled(pReferential.SQLRowStyle.Value))
                {
                    sqlRowStyle = Cst.CrLf + pReferential.SQLRowStyle.Value + SQLCst.AS + "ROWSTYLE, ";
                }

                string sqlRowState = string.Empty;
                if (pReferential.SQLRowStateSpecified && StrFunc.IsFilled(pReferential.SQLRowState.Value))
                {
                    sqlRowState = pReferential.SQLRowState.Value;
                    sqlRowState = Cst.CrLf + "(" + sqlRowState + ")" + SQLCst.AS + "ROWSTATE, ";
                }


                // FI 20140519 [19923]  add colum RequestTrackData
                string sqlRequestTrackData = string.Empty;
                if (SessionTools.IsRequestTrackEnabled && pReferential.RequestTrackSpecified)
                {
                    for (int k = 0; k < ArrFunc.Count(pReferential.RequestTrack.RequestTrackData); k++)
                    {
                        ReferentialRequestTrackData rtd = pReferential.RequestTrack.RequestTrackData[k];
                        sqlRequestTrackData += Cst.CrLf + rtd.columnGrp.sqlCol + SQLCst.AS + rtd.columnGrp.alias + ", ";
                        if (rtd.columnIdASpecified)
                            sqlRequestTrackData += Cst.CrLf + rtd.columnIdA.sqlCol + SQLCst.AS + rtd.columnIdA.alias + ", ";
                        if (rtd.columnIdBSpecified)
                            sqlRequestTrackData += Cst.CrLf + rtd.columnIdB.sqlCol + SQLCst.AS + rtd.columnIdB.alias + ", ";
                    }
                }


                string sqlIsSelected = string.Empty;
                if (pReferential.SQLCheckSelectedDefaultValueSpecified)// EG 20110906
                    sqlIsSelected = Cst.CrLf + (BoolFunc.IsFalse(pReferential.SQLCheckSelectedDefaultValue) ? "0" : "1");
                else
                    sqlIsSelected = "1";
                sqlIsSelected += SQLCst.AS + "ISSELECTED, ";


                sql_SelectAdditional += sqlIsSelected + sqlRowStyle + sqlRowState + sqlRequestTrackData;
                // 20110912 PM
                // Générer les colonnes ROWSTYLE et ROWSTATE vide pour les requêtes avec les données agrégés (GroupBy)
                if (isWithGroupBy)
                {
                    string sqlRowStyleGB = string.Empty;
                    if (pReferential.SQLRowStyleSpecified && StrFunc.IsFilled(pReferential.SQLRowStyle.Value))
                    {
                        sqlRowStyleGB = Cst.CrLf + "''" + SQLCst.AS + "ROWSTYLE, ";
                    }

                    string sqlRowStateGB = string.Empty;
                    if (pReferential.SQLRowStateSpecified && StrFunc.IsFilled(pReferential.SQLRowState.Value))
                    {
                        sqlRowStateGB = Cst.CrLf + "''" + SQLCst.AS + "ROWSTATE, ";
                    }

                    // FI 20140519 [19923]  add colum RequestTrackData
                    string sqlRequestTrackDataGB = string.Empty;
                    if (SessionTools.IsRequestTrackEnabled && pReferential.RequestTrackSpecified)
                    {
                        for (int k = 0; k < ArrFunc.Count(pReferential.RequestTrack.RequestTrackData); k++)
                        {
                            ReferentialRequestTrackData rtd = pReferential.RequestTrack.RequestTrackData[k];
                            sqlRequestTrackDataGB += Cst.CrLf + "''" + SQLCst.AS + rtd.columnGrp.alias + ", ";
                            if (pReferential.RequestTrack.RequestTrackData[k].columnIdASpecified)
                                sqlRequestTrackDataGB += Cst.CrLf + "null" + SQLCst.AS + rtd.columnIdA.alias + ", ";
                            if (pReferential.RequestTrack.RequestTrackData[k].columnIdBSpecified)
                                sqlRequestTrackDataGB += Cst.CrLf + "null" + SQLCst.AS + rtd.columnIdB.alias + ", ";
                        }
                    }
                    string sqlIsSelectedGB = "1" + SQLCst.AS + "ISSELECTED, ";


                    sql_SelectGBAdditional += sqlIsSelectedGB + sqlRowStyleGB + sqlRowStateGB + sqlRequestTrackDataGB;
                }
                #endregion
            }
            //
            if (sql_OrderBy.Length > 0)
            {
                if (!sql_OrderBy.Trim().ToLower().StartsWith(SQLCst.ORDERBY.Trim().ToLower()))
                    sql_OrderBy = SQLCst.ORDERBY + sql_OrderBy;
            }

            #region Final
            char[] cTrim;
            if (sql_Select == null)
                sql_Select = "*";
            //
            sql_Select += sql_SelectAdditional;
            //
            if (isWithGroupBy)
            {
                sql_Select += Cst.CrLf + sql_SelectSort;
                sql_Select += "0" + SQLCst.AS + sql_GroupByCount + ",0" + SQLCst.AS + sql_GroupByNumber + ",";
                //
                // 20110912 PM
                // Générer les colonnes ROWSTYLE et ROWSTATE vide pour les requêtes avec les données agrégés (GroupBy)
                sql_SelectGBFirst += sql_SelectGBAdditional + Cst.CrLf;
                //
                sql_SelectGBFirst += sql_SelectGBSort;
                sql_SelectGBFirst += sql_GroupByCount + "," + sql_GroupByNumber + ",";
                //
                sql_SelectGB += Cst.CrLf + SQLCst.COUNT_ALL.Trim() + SQLCst.AS + sql_GroupByCount + ",";
                //
                if (sql_OrderBy.Length > 0)
                    sql_OrderBy += ", " + sql_GroupByNumber;
            }
            //
            cTrim = (Cst.CrLf).ToCharArray();
            sql_Select = sql_Select.Trim().TrimEnd(cTrim);
            //
            if (isWithGroupBy)
            {
                sql_SelectGBFirst = sql_SelectGBFirst.Trim().TrimEnd(cTrim);
                sql_SelectGB = sql_SelectGB.Trim().TrimEnd(cTrim);
                sql_GroupBy = sql_GroupBy.Trim().TrimEnd(cTrim);
            }
            //
            cTrim = (",").ToCharArray();
            sql_Select = sql_Select.Trim().TrimEnd(cTrim);
            //
            if (isWithGroupBy)
            {
                sql_SelectGBFirst = sql_SelectGBFirst.Trim().TrimEnd(cTrim);
                sql_SelectGB = sql_SelectGB.Trim().TrimEnd(cTrim);
                sql_GroupBy = sql_GroupBy.Trim().TrimEnd(cTrim);
            }

            if (pSelectedColumns == SelectedColumnsEnum.None)//PLTest2012
                sql_Select = SQLCst.SELECT + SQLCst.COUNT_1;

            sql_Query = sql_Select + Cst.CrLf + sql_From;

            if (StrFunc.IsFilled(sql_Head_Join))
                sql_Query += sql_Head_Join;
            if (StrFunc.IsFilled(sql_Join))
                sql_Query += sql_Join;
            if (StrFunc.IsFilled(sql_Head_WhereJoin))
                sql_Query += sql_Head_WhereJoin;
            //
            cTrim = (",").ToCharArray();
            sql_Where = sqlWhere.ToString();
            if (sql_Where.Length > 0)
                sql_Query += Cst.CrLf + sql_Where.TrimEnd(cTrim);
            //
            #region GroupBy
            // MF 20120430 ruptures with groupingset
            if (isWithGroupBy && StrFunc.IsFilled(sql_GroupBy) && Cst.IsWithTotalOrSubTotal(groupingSet))
            {
                isQueryWithSubTotal = true;

                // MF 20120430 ruptures with groupingset
                if (Cst.IsWithDetails(groupingSet))
                {
                    sql_Query += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                }
                else
                {
                    sql_Query = String.Empty;
                }

                sql_Query += sql_SelectGBFirst + Cst.CrLf;
                sql_Query += SQLCst.X_FROM + Cst.CrLf;
                //
                sql_Query += "(" + sql_SelectGB + ",1" + SQLCst.AS + sql_GroupByNumber + Cst.CrLf;
                sql_Query += sql_From;
                //
                if (StrFunc.IsFilled(sql_Head_Join))
                    sql_Query += sql_Head_Join;
                if (StrFunc.IsFilled(sql_Join))
                    sql_Query += sql_Join;
                if (StrFunc.IsFilled(sql_Head_WhereJoin))
                    sql_Query += sql_Head_WhereJoin;
                //
                if (sql_Where.Length > 0)
                    sql_Query += Cst.CrLf + sql_Where.TrimEnd(cTrim);
                //
                //sql_Query += Cst.CrLf + SQLCst.GROUPBY + sql_GroupBy + ") tblGroupBy" + Cst.CrLf;
                sql_Query += Cst.CrLf + SQLCst.GROUPBY + sql_GroupBy.GetNormalizedGroupByString() + ") tblGroupBy" + Cst.CrLf;
                //
                // MF 20120430 ruptures with groupingset
                if (Cst.IsWithSubTotal(groupingSet))
                {
                    //string[] allGroupByColumn = sql_GroupBy.Split(",".ToCharArray());
                    string[] allGroupByColumn = sql_GroupBy.GetGroupByColumn();
                    //
                    if (allGroupByColumn.Length > 1)
                    {
                        string newSql_GroupBy = sql_GroupBy;
                        string newSql_SelectGB = sql_SelectGB;
                        //
                        for (int i = allGroupByColumn.Length; i > 1; i--)
                        {
                            int groupByNumber = allGroupByColumn.Length - i + 2;
                            string groupByColumn = allGroupByColumn[i - 1];
                            //
                            newSql_SelectGB = newSql_SelectGB.Replace(groupByColumn.Trim(), SQLCst.NULL);
                            //
                            newSql_GroupBy = newSql_GroupBy.Replace(groupByColumn.Trim(), string.Empty);
                            newSql_GroupBy = newSql_GroupBy.Trim().TrimEnd(cTrim);
                            //
                            sql_Query += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                            sql_Query += sql_SelectGBFirst + Cst.CrLf;
                            sql_Query += SQLCst.X_FROM + Cst.CrLf;
                            //
                            sql_Query += "(" + newSql_SelectGB + "," + groupByNumber.ToString() + SQLCst.AS + sql_GroupByNumber + Cst.CrLf;
                            sql_Query += sql_From;
                            //
                            if (StrFunc.IsFilled(sql_Head_Join))
                                sql_Query += sql_Head_Join;
                            if (StrFunc.IsFilled(sql_Join))
                                sql_Query += sql_Join;
                            if (StrFunc.IsFilled(sql_Head_WhereJoin))
                                sql_Query += sql_Head_WhereJoin;
                            //
                            if (sql_Where.Length > 0)
                                sql_Query += Cst.CrLf + sql_Where.TrimEnd(cTrim);
                            //
                            //sql_Query += Cst.CrLf + SQLCst.GROUPBY + newSql_GroupBy + ") tblGroupBy_" + groupByNumber.ToString() + Cst.CrLf;
                            sql_Query += Cst.CrLf + SQLCst.GROUPBY + newSql_GroupBy.GetNormalizedGroupByString() + ") tblGroupBy_" + groupByNumber.ToString() + Cst.CrLf;
                        }
                    }
                }
            }
            else
                isQueryWithSubTotal = false;
            #endregion
            //
            // RD 20120524 
            // Ne pas mettre la clause OrderBy directement dans la query 
            //
            // La requête suivante ne passe pas: 
            //
            //      select col1 as value1 from tbl1
            //      union all  
            //      select col1 as value1 from tbl2
            //      order by case when value1 = 'xxx' then 'xxxX' else value1 end
            //    
            // Enrichir la requête plustard pour utiliser la syntaxe ISO: ROW_NUMBER() OVER

            //if (sql_OrderBy.Length > 0)
            //    sql_Query = DataHelper.TransformQueryForOrderBy(sql_Query, sql_OrderBy.TrimEnd(cTrim), true, isQueryWithSubTotal);
            //
            sql_Query = pReferential.ReplaceDynamicArgument(pSource, sql_Query);
            opSQLQuery = pReferential.ConvertQueryToQueryParameters(pSource, sql_Query);
            //        
            opSQLWhere = sql_Where.TrimEnd(cTrim);
            opSQL_Criteria = sql_Criteria;
            opSQLOrderBy = sql_OrderBy.TrimEnd(cTrim);
            //
            #endregion Final
        }
        /// <summary>
        /// Retourne TRUE si la colonne est utilisée au sein d'un critère (Where), sur la base de la pérsence de son alias de table. Sinon retorune FALSE.
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pColumn_AliasTableName"></param>
        /// <returns></returns>
        private static bool IsColumnUsedOnWhere(Referential pReferential, string pColumn_AliasTableName)
        {
            bool ret = false;
            if (pReferential.SQLWhereSpecified)
            {
                for (int ii = 0; ii < pReferential.SQLWhere.Length; ii++)
                {
                    ReferentialSQLWhere rrw = pReferential.SQLWhere[ii];
                    if (null != rrw)
                    {
                        if (rrw.ColumnNameSpecified && rrw.AliasTableName == pColumn_AliasTableName)
                        {
                            //L'alias de la table de la colonne correspond à l'alias de table du critère courant.
                            ret = true;
                            break;
                        }
                    }
                    rrw = null;
                }
            }
            return ret;
        }
        #endregion

        #region private BuildValueFormated
        /// <summary>
        ///  Formatage SQL de la donnée FK ou PK
        /// </summary>
        /// <param name="pReferential">Class Referential</param>
        /// <param name="pValue">Valeur de la donnée</param>
        /// <param name="pIsDataKeyField">Indicateur de donnée PK (True) ou FK (False)</param>
        /// <returns></returns>
        public static string BuildValueFormated(Referential pReferential, string pColumnValue, bool pIsDataKeyField)
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(pColumnValue))
            {
                int index = -1;
                //
                if (pIsDataKeyField)
                    index = pReferential.IndexDataKeyField;
                else
                    index = pReferential.IndexForeignKeyField;
                //                    
                if ((index > -1) &&
                    (TypeData.IsTypeString(pReferential.Column[index].DataType.value)))
                    ret = DataHelper.SQLString(pColumnValue);
                else
                    ret = pColumnValue;
            }
            return ret;

        }
        #endregion
        //
        #region private BuildSqlWhere
        /// <summary>
        /// Genere le SQLWhere à partir de la FK ou de la PK
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pColumn"></param>
        /// <param name="pColumnValue"></param>
        /// <param name="pIsColumnDataKeyField"></param>
        /// <returns></returns>
        private static string BuildSqlWhere(Referential pReferential, string pColumn, string pColumnValue, bool pIsColumnDataKeyField)
        {
            //Load des données de la DataBase et stockage du DataTable dans le cache
            string valueFormated = BuildValueFormated(pReferential, pColumnValue, pIsColumnDataKeyField);
            //
            string ret = string.Empty;
            if (StrFunc.IsFilled(pColumn) && StrFunc.IsFilled(valueFormated))
                ret = pColumn + "=" + valueFormated;
            //
            return ret;
        }
        #endregion
        //
        #region TransformRecursiveQuery
        //PL 20110303 TEST for SQLServer WITH (TBD)
        //EG 20111222 Test SQLServer/Oracle
        ///<summary>
        /// Traitement d'une requête possédant des instructions de récursivité
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► SQLServer</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► PREAMBULE : La sous-requête de récursivité doit être encadrée des tags /* SQL: WITH */ et /* SQL: ENDWITH */</para>
        /// <para>1. La fonction déplace cette sous-requête en tête de requête principale</para>
        /// <para>2. Si présence des tags /* SQL: WITHWHERE */ et /* SQL: ENDWITHWHERE */</para> 
        /// <para>   alors la clause WHERE de la requête principale (FK/PK) de type (column = value) est:</para>
        /// <para>   ● déplacée dans celle de récursivité</para>
        /// <para>   ● remplacée par (column = column)</para>
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>► Oracle</para> 
        /// <para>────────────────────────────────────────────────────────────────────────────</para>
        /// <para>1. Si présence des tags /* SQL: WITHWHERE */ et /* SQL: ENDWITHWHERE */</para> 
        /// <para>   alors la clause WHERE de la requête principale (FK/PK) de type (column = value) est:</para>
        /// <para>   ● déplacée dans celle de récursivité</para>
        /// <para>   ● remplacée par (column = column)</para>
        ///</summary>
        public static string TransformRecursiveQuery(string pCS, Referential pReferential, string pColumn, string pColumnValue, bool pIsColumnDataKeyField, string pQuery)
        {

            string query = pQuery;
            DbSvrType serverType = DataHelper.GetDbSvrType(pCS);
            if (DbSvrType.dbSQL == serverType)
            {
                int posWith = query.IndexOf(@"/* SQL: WITH */");
                if (posWith > 0)
                {
                    int posEndWith = query.IndexOf(@"/* SQL: ENDWITH */");
                    string querySource = query;
                    string queryWidth = querySource.Substring(posWith, posEndWith + @"/* SQL: ENDWITH */".Length - posWith + 1);

                    string sqlWhere = BuildSqlWhere(pReferential, pColumn, pColumnValue, pIsColumnDataKeyField);
                    if (StrFunc.IsFilled(sqlWhere))
                    {
                        // EG 20130729 Test IsGrid en commentaire
                        //if (pReferential.IsGrid)
                        //{
                        int posWithWhere = queryWidth.IndexOf(@"/* SQL: WITHWHERE */");
                        if (posWithWhere > 0)
                        {
                            // on inhile la partie du where de la query principale (FK/PK): (column = value) devient (column = column)
                            string sqlWhereSubstitute = sqlWhere.Replace(pColumnValue, SetAliasToSQLWhere(pReferential, pColumn));
                            sqlWhereSubstitute = sqlWhereSubstitute.Replace("'", string.Empty);
                            querySource = querySource.Replace(sqlWhere, sqlWhereSubstitute);
                            // pour ne la conserver que dans le CTE
                            int posEndWithWhere = queryWidth.IndexOf(@"/* SQL: ENDWITHWHERE */");
                            queryWidth = queryWidth.Substring(0, posWithWhere) + sqlWhere + queryWidth.Substring(posEndWithWhere + @"/* SQL: ENDWITHWHERE */".Length);
                            posEndWith = querySource.IndexOf(@"/* SQL: ENDWITH */");
                        }
                        //}
                    }
                    query = queryWidth + querySource.Substring(0, posWith)
                            + querySource.Substring(posEndWith + @"/* SQL: ENDWITH */".Length);
                }
            }
            else if (DbSvrType.dbORA == serverType)
            {
                string sqlWhere = BuildSqlWhere(pReferential, pColumn, pColumnValue, pIsColumnDataKeyField);
                if (StrFunc.IsFilled(sqlWhere))
                {
                    if (pReferential.IsGrid)
                    {
                        int posWithWhere = query.IndexOf(@"/* SQL: WITHWHERE */");
                        if (posWithWhere > 0)
                        {
                            // on inhile la partie du where de la query principale (FK/PK): (column = value) devient (column = column)
                            string sqlWhereSubstitute = sqlWhere.Replace(pColumnValue, SetAliasToSQLWhere(pReferential, pColumn));
                            sqlWhereSubstitute = sqlWhereSubstitute.Replace("'", string.Empty);
                            query = query.Replace(sqlWhere, sqlWhereSubstitute);
                            // pour ne la conserver que dans le CTE
                            int posEndWithWhere = query.IndexOf(@"/* SQL: ENDWITHWHERE */");
                            query = query.Substring(0, posWithWhere) + sqlWhere + query.Substring(posEndWithWhere + @"/* SQL: ENDWITHWHERE */".Length);
                        }
                    }
                }
            }
            return query;
        }
        #endregion TransformRecursiveQuery

        #region private SetAliasToSQLWhere
        private static string SetAliasToSQLWhere(string pAliasTableName, string pSQLWhere)
        {
            string sqlWhere = pSQLWhere;
            bool isexistAliasTable = false;
            int posDot = sqlWhere.IndexOf(".");
            if (posDot >= 0)
            {
                //Verrue temporaire, pour géréer un cas tel que : COLUMN= 'DATA1.0'
                int posQuote = sqlWhere.IndexOf("'");
                if (posQuote >= 0)
                    isexistAliasTable = (posDot < posQuote);
                else
                    isexistAliasTable = true;
            }
            if (false == isexistAliasTable)
                sqlWhere = pAliasTableName + "." + sqlWhere;
            return sqlWhere;
        }
        private static string SetAliasToSQLWhere(Referential pReferential, string pSQLWhere)
        {
            string aliasTableName = SQLCst.TBLMAIN;
            if (pReferential.AliasTableNameSpecified)
                aliasTableName = pReferential.AliasTableName;
            return SetAliasToSQLWhere(aliasTableName, pSQLWhere);
        }
        #endregion private SetAliasToSQLWhere

        #region class SQLSelectParameters
        public class SQLSelectParameters
        {
            #region Constructor(s)
            public SQLSelectParameters(string pSource, Referential pReferential) :
                this(pSource, SelectedColumnsEnum.All, pReferential) { }
            public SQLSelectParameters(string pSource, SelectedColumnsEnum pSelectedColumns, Referential pReferential)
            {
                source = pSource;
                selectedColumns = pSelectedColumns;
                referential = pReferential;

                isForExecuteDataAdapter = false;
                isSelectDistinct = false;
            }
            public SQLSelectParameters(string pSource, Referential pReferential, string pSQLWhere) :
                this(pSource, SelectedColumnsEnum.All, pReferential)
            {
                sqlWhere = pSQLWhere;
            }
            #endregion

            #region Member(s)
            public string source;
            public Referential referential;

            public string sqlWhere;
            public string sqlHints;
            public bool isForExecuteDataAdapter;
            public bool isSelectDistinct;
            public SQLReferentialData.SelectedColumnsEnum selectedColumns;
            #endregion
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRequestTrack"></param>
        private static void CheckReferentialRequestTrack(ReferentialRequestTrack pRequestTrack)
        {

            for (int k = 0; k < ArrFunc.Count(pRequestTrack.RequestTrackData); k++)
            {
                ReferentialRequestTrackData rtd = pRequestTrack.RequestTrackData[k];
                if (null == rtd.columnGrp)
                    throw new Exception("RequestTrackData: ColumnGrp doesn't exist");

                if (rtd.columnIdASpecified)
                {
                    if (StrFunc.IsEmpty(rtd.columnIdA.alias) || StrFunc.IsEmpty(rtd.columnIdA.sqlCol))
                    {
                        throw new Exception("RequestTrackData: columnIdA doesn't contains alias or sqlExpresion");
                    }
                }
                if (rtd.columnIdBSpecified)
                {
                    if (StrFunc.IsEmpty(rtd.columnIdB.alias) || StrFunc.IsEmpty(rtd.columnIdB.sqlCol))
                    {
                        throw new Exception("RequestTrackData: columnIdA doesn't contains alias or sqlExpresion");
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="rrw"></param>
        /// <param name="pReferential"></param>
        /// FI 20140616 [XXXX] add Method
        public static SQL_ColumnCriteria GetSQL_ColumnCriteria(string pCS, ReferentialSQLWhere rrw, Referential pReferential)
        {
            if (false == rrw.ColumnNameSpecified)
                throw new Exception("ColumnNameis not Specified for rrw (ReferentialsReferentialSQLWhere)");

            SQL_ColumnCriteria sql_ColumnCriteria = null;

            string tmpValue;
            string[] tmpValues;

            string tmpColumn = rrw.ColumnName;
            string tmpColumnSqlWhere = string.Empty;

            ReferentialColumn rrc = pReferential[rrw.ColumnName, rrw.AliasTableName];
            if (null != rrc)
                tmpColumnSqlWhere = (rrc.ColumnSqlWhereSpecified ? rrc.ColumnSqlWhere : (rrw.ColumnSQLWhereSpecified ? rrw.ColumnSQLWhere : string.Empty));
            else
                tmpColumnSqlWhere = (rrw.ColumnSQLWhereSpecified ? rrw.ColumnSQLWhere : string.Empty);


            if (rrw.AliasTableNameSpecified && rrw.AliasTableName.Length > 0)
            {
                tmpColumn = rrw.AliasTableName + "." + tmpColumn;
                tmpColumnSqlWhere = tmpColumnSqlWhere.Replace(Cst.DYNAMIC_ALIASTABLE, rrw.AliasTableName);
            }

            string tempColumnNameOrColumnSQLSelect = SQLReferentialData.GetColumnNameOrColumnSelect(pReferential, rrw);


            if (StrFunc.IsFilled(rrw.LstValue))
            {
                tmpValue = rrw.LstValue;
                tmpValues = StrFunc.StringArrayList.StringListToStringArray(tmpValue);
                for (int j = 0; j < tmpValues.Length; j++)
                {
                    if (tmpValues[j].StartsWith(Cst.DA_START))
                        tmpValues[j] = pReferential.ReplaceDynamicArgument(pCS, tmpValues[j]);
                }
                tmpValue = ArrFunc.GetStringList(tmpValues, ";");
            }
            else
            {
                tmpValue = string.Empty;
            }

            SQL_ColumnCriteriaDataType sqlDatatype = new SQL_ColumnCriteriaDataType();
            SQL_ColumnCriteriaInput sqlDataInput = new SQL_ColumnCriteriaInput();
            TypeData.TypeDataEnum datatype = TypeData.GetTypeDataEnum(rrw.DataType.value);
            if (datatype == TypeData.TypeDataEnum.datetime && (rrw.DataType.datakindSpecified && rrw.DataType.datakind == Cst.DataKind.Timestamp)
                && rrw.DataType.tzdbidSpecified)
            {
                sqlDatatype = new SQL_ColumnCriteriaDataType(datatype, rrw.DataType.tzdbid);
                sqlDataInput = new SQL_ColumnCriteriaInput(tmpValue, SessionTools.GetCriteriaTimeZone());
            }
            else
            {
                sqlDatatype = new SQL_ColumnCriteriaDataType(datatype);
                sqlDataInput = new SQL_ColumnCriteriaInput(tmpValue);
            }

            sql_ColumnCriteria = new SQL_ColumnCriteria(sqlDatatype, tempColumnNameOrColumnSQLSelect, tmpColumnSqlWhere, rrw.Operator, sqlDataInput);


            return sql_ColumnCriteria;
        }

    }
    #endregion SQLReferentialData
}

