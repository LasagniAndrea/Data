using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Xml.Serialization;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;

using EFS.Common;
using EFS.Common.Web;
using EFS.Common.MQueue;
using EFS.Permission;
using EFS.Restriction;
using EfsML;


namespace EFS.GridViewProcessor
{
    public class Export
    {
        #region Members
        private ContentPageBase currentPage;
        private string outFilename;
        private string cs;
        private DbSvrType dbSvrType;
        private bool isOracle;
        private string owner;
        #endregion
        #region Accessors
        public ContentPageBase CurrentPage
        {
            get { return this.currentPage; }
            set { this.currentPage = value; }
        }
        public string OutFilename
        {
            get { return this.outFilename; }
            set { this.outFilename = value; }
        }
        #endregion Members
        #region constructor
        public Export() { }
        #endregion constructor

        /// <summary>
        /// SQLExport
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pDatatable"></param>
        /// <returns></returns>
        public Cst.ErrLevel SQLExport(ContentPageBase pPage, Referential pReferential, DataTable pDatatable, string pFilename)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            owner = SQLCst.DBO.Trim();
            if (StrFunc.IsFilled(pPage.Request.QueryString["owner"]))
            {
                owner = pPage.Request.QueryString["owner"].ToString().Trim();
                if (false == owner.EndsWith("."))
                    owner += ".";
            }
            currentPage = pPage;
            outFilename = pFilename;
            cs = SessionTools.CS;
            dbSvrType = DataHelper.GetDbSvrType(cs);
            isOracle = (dbSvrType == DbSvrType.dbORA);

            StringBuilder sqlQuery = new StringBuilder();

            int maxStep = BoolFunc.IsTrue(SystemSettings.GetAppSettings("SQLExportAllowed", "false")) ? 2 : 1;
#if DEBUG
            maxStep = 2;
#endif

            List<string> lstOutputQuery = new List<string>();
            for (int i = 1; i <= maxStep; i++)
            {
                #region Tip: Génération en mode DEBUG des 2 syntaxes: Oracle et SQLServer

                if (i == 2)//Tip
                {
                    isOracle = (false == isOracle);
                    if (isOracle)
                        dbSvrType = (isOracle ? DbSvrType.dbORA : DbSvrType.dbSQL);
                }

                sqlQuery.Append(@"<div class=""panel panel-" + dbSvrType.ToString() + @""">");
                sqlQuery.Append(@"<div class=""panel-heading"">");

                if (maxStep == 2)
                {
                    if (isOracle)
                    {
                        sqlQuery.AppendLine(CommentOutput("Oracle® - PL/SQL"));
                    }
                    else
                    {
                        sqlQuery.AppendLine(CommentOutput("MS SQLserver® - Transact-SQL"));
                    }
                }
                #endregion

                #region Initialisation
                StringBuilder sqlInsertQuery = new StringBuilder();
                StringBuilder sqlDeleteQuery = new StringBuilder();
                StringBuilder sqlRestoreQuery = new StringBuilder();

                sqlQuery.Append("</div>");
                sqlQuery.Append(@"<div class=""panel-body"">");

                if (isOracle)
                {
                    sqlQuery.AppendLine("declare");
                    sqlQuery.AppendLine("  DtEnabled DATE;");
                    sqlQuery.AppendLine("  DtIns DATE;");
                    sqlQuery.AppendLine("  IdaIns NUMBER(5);");
                    sqlQuery.AppendLine("  Ida NUMBER(5);");
                    sqlQuery.AppendLine("begin");
                    sqlQuery.AppendLine("  DtEnabled := TO_DATE(TO_CHAR(SYSDATE, 'YYYY') || '0101', 'YYYYMMDD');");
                    sqlQuery.AppendLine("  DtIns := SYSDATE;");
                    sqlQuery.AppendLine("  select IDA into IdaIns from " + owner + "ACTOR where IDENTIFIER='SYSTEM';");
                }
                else
                {
                    sqlQuery.AppendLine("declare @DtEnabled UT_DATETIME, @DtIns UT_DATETIME, @IdaIns UT_ID, @Ida UT_ID");
                    sqlQuery.AppendLine("begin");
                    sqlQuery.AppendLine("  begin tran");
                    sqlQuery.AppendLine("  set @DtEnabled=convert(datetime, convert(varchar(4), datepart(year, getdate())) + '0101');");
                    sqlQuery.AppendLine("  set @DtIns=getdate();");
                    sqlQuery.AppendLine("  select @IdaIns=IDA from " + owner + "ACTOR where IDENTIFIER='SYSTEM';");
                }
                sqlQuery.AppendLine(" ");
                #endregion

                switch (pDatatable.TableName)
                {
                    case "ACCMODEL":
                        //WARNING: TBD for ORACLE
                        #region Reserved to Accounting model: recursive tables (Table: ACCMODEL)
                        // Optional input parameters from URL: http://..........List.apsx?..............&NewID=TOTO

                        // newId: Used in duplicate action. It's the new identifier of accounting model
                        string newId = pPage.Request.QueryString["NewID"];

                        // prefix and suffix: Add into the identifier of accounting schemas and rules.  
                        string prefix = pPage.Request.QueryString["Prefix"];
                        string suffix = pPage.Request.QueryString["Suffix"];

                        string idMenuParentTable = GetIdMenu();

                        foreach (DataRow row in pDatatable.Rows)
                        {
                            sqlInsertQuery.AppendLine(BuildAccModelInsertQuery(pReferential, newId, row));
                            ExploreChildTables(idMenuParentTable, pReferential, row, prefix, suffix, ref sqlInsertQuery);
                        }

                        sqlQuery.AppendLine(sqlInsertQuery.ToString());
                        break;
                        #endregion

                    case "IOTASK":
                        #region Reserved to I/O Task: recursive tables (Table: IOTASK)
                        ArrayList arParsing = new ArrayList();
                        ArrayList arParam = new ArrayList();

                        sqlInsertQuery.AppendLine(" ");
                        sqlInsertQuery.AppendLine(" ");
                        sqlInsertQuery.AppendLine(PrintOutput("Insert new Task...", '='));
                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                        sqlInsertQuery.AppendLine(CommentOutput("WARNING: In the following query replace <SYSTEM> by task owner", ' '));
                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                        if (isOracle)
                            sqlInsertQuery.AppendLine("  select IDA into Ida from " + owner + "ACTOR where IDENTIFIER='SYSTEM';");
                        else
                            sqlInsertQuery.AppendLine("  select @Ida = IDA from " + owner + "ACTOR where IDENTIFIER='SYSTEM';");
                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));


                        if (Convert.ToInt32(pDatatable.Rows.Count) != 1)
                        {
                            #region Error: Cette version ne gère l'exportation que d'une seule tâche
                            sqlInsertQuery.Remove(0, sqlInsertQuery.Length);
                            sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                            sqlInsertQuery.AppendLine("ERROR: Please, select only one task!");
                            sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                            #endregion
                        }
                        else
                        {
                            #region Export Task And Elements
                            //IOTASK
                            sqlInsertQuery.AppendLine(BuildInsertQuery(pReferential, pDatatable).ToString());

                            foreach (DataRow row in pDatatable.Rows) // for each IOTASK
                            {
                                int idIoTask = Convert.ToInt32(row["IDIOTASK"]);
                                string taskIdentifier = Convert.ToString(row["IDENTIFIER"]);
                                string parentTableName = pReferential.TableName;
                                string selectPkValue = "(select IDIOTASK from " + owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ")";

                                #region Spécifique au Consulting
                                if (false == isOracle)
                                {
                                    sqlRestoreQuery.AppendLine("/*");
                                    sqlRestoreQuery.AppendLine("  if exists (select * from sysobjects where name='UP_SIOTOLS_RESTORE_PARAMETERS' and xtype='P')");
                                    sqlRestoreQuery.AppendLine("    execute UP_SIOTOLS_RESTORE_PARAMETERS @pnTaskIdentifier=" + DataHelper.SQLString(taskIdentifier) + ",@bGetDefaultValue=1,@bIsHideOnGui=1");
                                    sqlRestoreQuery.AppendLine("  if exists (select * from sysobjects where name='UP_SIOTOLS_RESTORE_FILENAMES' and xtype='P')");
                                    sqlRestoreQuery.AppendLine("    execute UP_SIOTOLS_RESTORE_FILENAMES  @pnTaskIdentifier=" + DataHelper.SQLString(taskIdentifier) + ",@bGetOutDataname=1,@bGetOutXslMapping=1");
                                    sqlRestoreQuery.AppendLine("*/");
                                }
                                #endregion

                                sqlDeleteQuery.AppendLine(PrintOutput("Delete old data...", '='));
                                sqlDeleteQuery.AppendLine("  delete from " + owner + "IOTASK_PARAM where IDIOTASK in (select IDIOTASK from " + owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ");");
                                sqlDeleteQuery.AppendLine("  delete from " + owner + "IOTASKDET where IDIOTASK in (select IDIOTASK from " + owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ");");
                                sqlDeleteQuery.AppendLine("  delete from " + owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ";");

                                #region IOTASKDET
                                DataTable dtIOTaskDet = DataTableIOTaskDet(taskIdentifier);
                                DataRow[] rowsIOTaskDet = dtIOTaskDet.Select();

                                Referential opIoTaskDetReferential = new Referential();
                                RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOTASKDET"),
                                    Cst.ListType.Repository, "IOTASKDET", null, null, null, out opIoTaskDetReferential);

                                sqlInsertQuery.Append(BuildInsertQuery(opIoTaskDetReferential, dtIOTaskDet, selectPkValue).ToString());

                                string fkColumn = opIoTaskDetReferential.Column[opIoTaskDetReferential.IndexForeignKeyField].ColumnName;
                                string fkValue = row.ItemArray[pReferential.IndexColSQL_DataKeyField].ToString();
                                string parentIdentifierValue = row.ItemArray[pReferential.IndexColSQL_KeyField].ToString();

                                foreach (DataRow rowIoTaskDet in rowsIOTaskDet) // for each IOTASKDET
                                {
                                    string elementType = Convert.ToString(rowIoTaskDet["ELEMENTTYPE"]);
                                    string idIOElement = Convert.ToString(rowIoTaskDet["IDIOELEMENT"]);

                                    #region INPUT
                                    if (elementType == "INPUT")
                                    {
                                        DataTable dtIOInput = DataTableIOInput(idIOElement);
                                        Referential opIoInputReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOINPUT"),
                                            Cst.ListType.Repository, "IOINPUT", null, null, null, out opIoInputReferential);

                                        sqlInsertQuery.Append(BuildInsertQuery(opIoInputReferential, dtIOInput).ToString());

                                        #region IOINPUT_PARSING
                                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '-'));
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOINPUT_PARSING where IDIOINPUT=" + DataHelper.SQLString(idIOElement) + ";");
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOINPUT where IDIOINPUT=" + DataHelper.SQLString(idIOElement) + ";");

                                        DataTable dtIOInputParsing = DataTableIOInputParsing(idIOElement);
                                        DataRow[] rowsIOInputParsing = dtIOInputParsing.Select();
                                        foreach (DataRow rowIOInputParsing in rowsIOInputParsing)
                                        {
                                            if (!arParsing.Contains(rowIOInputParsing["IDIOPARSING"]))
                                            {
                                                arParsing.Add(rowIOInputParsing["IDIOPARSING"]);

                                                DataTable dtIOParsing = DataTableIOParsing(Convert.ToString(rowIOInputParsing["IDIOPARSING"]));
                                                // Write IOPARSING rows
                                                Referential opIoParsingReferential = new Referential();
                                                RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARSING"), Cst.ListType.Repository, "IOPARSING", null, null, null, out opIoParsingReferential);

                                                // Delete IOPARSING
                                                if (Convert.ToString(rowIOInputParsing["IDIOPARSING"]).Length > 0)
                                                {
                                                    sqlDeleteQuery.AppendLine("  delete from " + owner + "IOPARSING where IDIOPARSING=" + DataHelper.SQLString(Convert.ToString(rowIOInputParsing["IDIOPARSING"])) + ";");

                                                    // Insert IOPARSING
                                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParsingReferential, dtIOParsing).ToString());

                                                    // IOPARSINGDET
                                                    DataTable dtIOParsingDet = DataTableIOParsingDet(Convert.ToString(rowIOInputParsing["IDIOPARSING"]));
                                                    Referential opIoParsingDetReferential = new Referential();
                                                    RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARSINGDET"), Cst.ListType.Repository, "IOPARSINGDET", null, null, null, out opIoParsingDetReferential);
                                                    // Insert IOPARSINGDET
                                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParsingDetReferential, dtIOParsingDet).ToString());
                                                }
                                            }
                                        }
                                        Referential opIoInputParsingReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOINPUT_PARSING"), Cst.ListType.Repository, "IOINPUT_PARSING", null, null, null, out opIoInputParsingReferential);
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoInputParsingReferential, dtIOInputParsing).ToString());

                                        #endregion
                                    }
                                    #endregion
                                    #region OUTPUT
                                    //WARNING: TBD for ORACLE 
                                    if (elementType == "OUTPUT")
                                    {
                                        DataTable dtIOOutput = DataTableIOOutput(idIOElement);
                                        Referential opIoOutputReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOOUTPUT"), Cst.ListType.Repository, "IOOUTPUT",
                                            null, null, null, out opIoOutputReferential);

                                        // Delete
                                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '-'));
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOOUTPUT_PARSING where IDIOOUTPUT=" + DataHelper.SQLString(idIOElement) + ";");
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOOUTPUT where IDIOOUTPUT=" + DataHelper.SQLString(idIOElement) + ";");

                                        // Insert IOOUTPUT
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoOutputReferential, dtIOOutput).ToString());

                                        #region  IOOUTPUT_PARSING
                                        DataTable dtIoOutputParsing = DataTableIOOutputParsing(idIOElement);
                                        DataRow[] rowsIoOutputParsing = dtIoOutputParsing.Select();
                                        foreach (DataRow rowIoOutputParsing in rowsIoOutputParsing)
                                        {
                                            //Read IOPARSING
                                            DataTable dtIOParsing = DataTableIOParsing(Convert.ToString(rowIoOutputParsing["IDIOPARSING"]));
                                            //Write IOPARSING
                                            Referential opIoParsingReferential = new Referential();
                                            RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARSING"), Cst.ListType.Repository, "IOPARSING", null, null, null, out opIoParsingReferential);

                                            // Delete IOPARSING
                                            sqlDeleteQuery.AppendLine(" delete from " + owner + "IOPARSING where IDIOPARSING=" + DataHelper.SQLString(Convert.ToString(rowIoOutputParsing["IDIOPARSING"])) + ";");
                                            // Insert IOPARSING
                                            sqlInsertQuery.Append(BuildInsertQuery(opIoParsingReferential, dtIOParsing).ToString());

                                            // Write IOPARSINGDET
                                            DataTable dtIOParsingDet = DataTableIOParsingDet(Convert.ToString(rowIoOutputParsing["IDIOPARSING"]));
                                            Referential opIoParsingDetReferential = new Referential();
                                            RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARSINGDET"), Cst.ListType.Repository, "IOPARSINGDET", null, null, null, out opIoParsingDetReferential);
                                            sqlInsertQuery.Append(BuildInsertQuery(opIoParsingDetReferential, dtIOParsingDet).ToString());
                                        }
                                        #endregion

                                        // Write IOOUTPUT_PARSING
                                        Referential opIoOutputParsingReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOOUTPUT_PARSING"),
                                            Cst.ListType.Repository, "IOOUTPUT_PARSING", null, null, null, out opIoOutputParsingReferential);
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoOutputParsingReferential, dtIoOutputParsing).ToString());
                                    }
                                    #endregion
                                    #region SHELL
                                    if (elementType == "SHELL")
                                    {
                                        DataTable dtIOShell = DataTableIOShell(idIOElement);
                                        Referential opIoShellReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOSHELL"),
                                            Cst.ListType.Repository, "IOSHELL", null, null, null, out opIoShellReferential);

                                        // delete IOSHELL
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOSHELL where IDIOSHELL=" + DataHelper.SQLString(idIOElement) + ";");
                                        // write IDIOSHELL
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoShellReferential, dtIOShell).ToString());
                                    }
                                    #endregion
                                    #region COMPARE
                                    if (elementType == "COMPARE")
                                    {
                                        DataTable dtIOCompare = DataTableIOCompare(idIOElement);
                                        Referential opIoCompareReferential = new Referential();
                                        RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOCOMPARE"),
                                            Cst.ListType.Repository, "IOCOMPARE", null, null, null, out opIoCompareReferential);

                                        // delete IOCOMPARE
                                        sqlDeleteQuery.AppendLine("  delete from " + owner + "IOCOMPARE where IDIOCOMPARE=" + DataHelper.SQLString(idIOElement) + ";");
                                        // write IOCOMPARE
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoCompareReferential, dtIOCompare).ToString());
                                    }
                                    #endregion
                                } // end foreach (DataRow row in pDatatable.Rows) for each IOTASK
                                #endregion

                                #region IOTASK_PARAM
                                DataTable dtIOTaskParam = DataTableIOTaskParam(taskIdentifier);
                                DataRow[] rowsIOTaskParam = dtIOTaskParam.Select();
                                Referential opIoTaskParamReferential = new Referential();
                                RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOTASK_PARAM"), Cst.ListType.Repository, "IOTASK_PARAM", null, null, null, out opIoTaskParamReferential);

                                foreach (DataRow rowIOTaskParam in rowsIOTaskParam)
                                {
                                    DataTable dtIOParam = DataTableIOParam(Convert.ToString(rowIOTaskParam["IDIOPARAM"]));
                                    DataRow[] rowsIOParam = dtIOParam.Select();
                                    Referential opIoParamReferential = new Referential();
                                    RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARAM"), Cst.ListType.Repository, "IOPARAM", null, null, null, out opIoParamReferential);

                                    string idIOParam = Convert.ToString(rowIOTaskParam["IDIOPARAM"]);

                                    // delete IOPARAM
                                    sqlDeleteQuery.AppendLine("  delete from " + owner + "IOPARAM where IDIOPARAM=" + DataHelper.SQLString(idIOParam) + ";");

                                    // insert IOPARAM
                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParamReferential, dtIOParam).ToString());
                                    // insert IOTASK_PARAM
                                    sqlInsertQuery.Append(BuildInsertQuery(opIoTaskParamReferential, dtIOTaskParam, selectPkValue).ToString());

                                    // IOPARAMDET
                                    DataTable dtIOParamDet = DataTableIOParamDet(Convert.ToString(rowIOTaskParam["IDIOPARAM"]));
                                    DataRow[] rowsIOParamDet = dtIOParamDet.Select();
                                    Referential opIoParamDetReferential = new Referential();
                                    RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("IOPARAMDET"), Cst.ListType.Repository, "IOPARAMDET", null, null, null, out opIoParamDetReferential);
                                    // insert IOPARAMDET
                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParamDetReferential, dtIOParamDet).ToString());
                                }
                                #endregion
                            }
                            #endregion Export Task And Elements
                        }

                        sqlQuery.Append(sqlDeleteQuery);
                        sqlQuery.Append(sqlInsertQuery);
                        sqlQuery.Append(sqlRestoreQuery);
                        break;
                        #endregion

                    case "DERIVATIVECONTRACT":
                        /* BD : Export des Contrats Dérivés */
                        #region Export des Contrats Dérivés

                        sqlInsertQuery.AppendLine(PrintOutput("Insertion of new Derivatives Contracts...", '='));
                        sqlDeleteQuery.AppendLine(PrintOutput("Removing old datas...", '='));

                        if (Convert.ToInt32(pDatatable.Rows.Count) == 0)
                        {
                            // L'export des DC fonctionne pour 1 DC ou plus
                            sqlInsertQuery.Remove(0, sqlInsertQuery.Length);
                            sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                            sqlInsertQuery.AppendLine("ERROR: Please select at least one Derivative Contract.");
                            sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '*'));
                        }
                        else
                        {
                            // Trie des DC par CATEGORY ASC (de manière à avoir les Futures avant les Options)
                            DataView dv = pDatatable.DefaultView;
                            dv.Sort = "CATEGORY ASC";
                            DataTable dtDerivativeContract = dv.ToTable();
                            /* Pour chaque DC à exporter */
                            foreach (DataRow drDerivativeContract in dtDerivativeContract.Rows)
                            {
                                #region DERIVATIVECONTRACT
                                string dc_IDENTIFIER = Convert.ToString(drDerivativeContract["IDENTIFIER"]);
                                int dc_IDDC = Convert.ToInt32(drDerivativeContract["IDDC"]);
                                string dc_MARKET_SHORT_ACRONYM = Convert.ToString(drDerivativeContract["MI_SHORT_ACRONYM"]);

                                string sql_where_IDM = " and IDM = (select IDM from " + owner + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " where SHORT_ACRONYM = " + DataHelper.SQLString(dc_MARKET_SHORT_ACRONYM) + ")";

                                sqlInsertQuery.AppendLine(PrintOutput("Derivative Contract Identifier: " + dc_IDENTIFIER + " - Market: " + dc_MARKET_SHORT_ACRONYM, '.'));

                                #region INSTRUMENT
                                /* Nécessaire à la récupération de IDI */
                                DataTable dtInstrument = DataTableInstrument(dc_IDDC);

                                string select_IDI = "( " + SQLCst.SELECT + "IDI ";
                                select_IDI += "from " + owner + Cst.OTCml_TBL.INSTRUMENT;
                                select_IDI += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Convert.ToString(dtInstrument.Rows[0]["IDENTIFIER"])) + " )";
                                #endregion INSTRUMENT

                                #region ASSET_EQUITY ou ASSET_INDEX
                                string asset_IDENTIFIER = string.Empty;
                                string asset_DtName = string.Empty;
                                string select_IDASSET = string.Empty;
                                DataTable dtAsset = null;
                                Referential opAssetReferential = new Referential();

                                if (Convert.ToString(drDerivativeContract["IDASSET_UNL"]) != string.Empty) //Si IDASSET_UNL est renseigné, on cherche l'IDENTIFIER du sous-jacent...
                                {
                                    switch (Convert.ToString(drDerivativeContract["ASSETCATEGORY"]))
                                    {
                                        case "Index":
                                            dtAsset = DataTableAsset_Index(Convert.ToInt32(drDerivativeContract["IDASSET_UNL"]));
                                            RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("ASSET_INDEX"), Cst.ListType.Repository, "ASSET_INDEX", null, null, null, out opAssetReferential);
                                            asset_IDENTIFIER = Convert.ToString(dtAsset.Rows[0]["IDENTIFIER"]);
                                            asset_DtName = "ASSET_INDEX";
                                            break;
                                        case "EquityAsset":
                                            dtAsset = DataTableAsset_Equity(Convert.ToInt32(drDerivativeContract["IDASSET_UNL"]));
                                            RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("ASSET_EQUITY"), Cst.ListType.Repository, "ASSET_EQUITY", null, null, null, out opAssetReferential);
                                            asset_IDENTIFIER = Convert.ToString(dtAsset.Rows[0]["IDENTIFIER"]);
                                            asset_DtName = "ASSET_EQUITY";
                                            break;
                                        default:
                                            asset_IDENTIFIER = null;
                                            break;
                                    }
                                }

                                if (dtAsset != null)
                                {
                                    sqlInsertQuery.AppendLine(BuildInsertQuery(opAssetReferential, dtAsset, true).ToString());

                                    select_IDASSET = "( " + SQLCst.SELECT + "a.IDASSET ";
                                    select_IDASSET += "from " + owner + asset_DtName + " a";
                                    select_IDASSET += SQLCst.WHERE + " a.IDENTIFIER = " + DataHelper.SQLString(asset_IDENTIFIER) + " )";
                                }
                                #endregion ASSET_EQUITY ou ASSET_INDEX

                                #region MATURITYRULE
                                //PL 20130411 Debug
                                //DataTable dtMaturityRule = DataTableMaturityRule(dcIdentifier);
                                DataTable dtMaturityRule = DataTableMaturityRule(dc_IDDC);
                                Referential opMaturityRuleReferential = new Referential();
                                RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("MATURITYRULE"), Cst.ListType.Repository, "MATURITYRULE", null, null, null, out opMaturityRuleReferential);

                                string select_IDMATURITYRULE = "( " + SQLCst.SELECT + "IDMATURITYRULE ";
                                select_IDMATURITYRULE += "from " + owner + Cst.OTCml_TBL.MATURITYRULE;
                                select_IDMATURITYRULE += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Convert.ToString(dtMaturityRule.Rows[0]["IDENTIFIER"])) + " )";
                                sqlInsertQuery.AppendLine(BuildInsertQuery(opMaturityRuleReferential, dtMaturityRule, true).ToString());
                                #endregion MATURITYRULE

                                DataTable dt_temp = pDatatable.Clone();
                                dt_temp.Rows.Add(drDerivativeContract.ItemArray);
                                sqlInsertQuery.AppendLine(BuildInsertQuery(pReferential, dt_temp, select_IDI, select_IDMATURITYRULE, select_IDASSET).ToString());
                                #endregion DERIVATIVECONTRACT

                                #region MATURITY
                                DataTable dtMaturity = DataTableMaturity(Convert.ToInt32(dtMaturityRule.Rows[0]["IDMATURITYRULE"]));
                                Referential opMaturityReferential = new Referential();
                                RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("MATURITY"), Cst.ListType.Repository, "MATURITY", null, null, null, out opMaturityReferential);

                                sqlInsertQuery.AppendLine(BuildInsertQuery(opMaturityReferential, dtMaturity, true, select_IDMATURITYRULE).ToString());

                                foreach (DataRow drMaturity in dtMaturity.Rows)
                                {
                                    /* Pour chaque Maturity */
                                    #region DERIVATIVEATTRIB
                                    DataTable dtDerivativeAttrib = DataTableDerivativeAttrib(Convert.ToInt32(drDerivativeContract["IDDC"]), Convert.ToInt32(drMaturity["IDMATURITY"]));
                                    Referential opDerivativeAttribReferential = new Referential();
                                    RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("DERIVATIVEATTRIB"), Cst.ListType.Repository, "DERIVATIVEATTRIB", null, null, null, out opDerivativeAttribReferential);
                                    string select_IDMATURITY = "( " + SQLCst.SELECT + "IDMATURITY ";
                                    select_IDMATURITY += "from " + owner + Cst.OTCml_TBL.MATURITY;
                                    select_IDMATURITY += SQLCst.WHERE + "( IDMATURITYRULE = " + select_IDMATURITYRULE + " )";
                                    select_IDMATURITY += SQLCst.AND + "( MATURITYMONTHYEAR = " + DataHelper.SQLString(Convert.ToString(drMaturity["MATURITYMONTHYEAR"])) + " ) )";

                                    sqlInsertQuery.AppendLine(BuildInsertQuery(opDerivativeAttribReferential, dtDerivativeAttrib, select_IDMATURITY).ToString());
                                    #endregion DERIVATIVEATTRIB

                                    #region ASSET_ETD
                                    DataTable dtAsset_Etd = DataTableAsset_Etd(dc_IDENTIFIER, Convert.ToInt32(drMaturity["IDMATURITY"]));
                                    Referential opAsset_EtdReferential = new Referential();
                                    RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("ASSET_ETD"), Cst.ListType.Repository, "ASSET_ETD", null, null, null, out opAsset_EtdReferential);

                                    string select_IDDC = "( " + SQLCst.SELECT + "IDDC ";
                                    select_IDDC += "from " + owner + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                                    select_IDDC += " where IDENTIFIER = " + DataHelper.SQLString(dc_IDENTIFIER);
                                    select_IDDC += sql_where_IDM;
                                    select_IDDC += " )";

                                    string select_IDDERIVATIVEATTRIB = "( " + SQLCst.SELECT + "IDDERIVATIVEATTRIB ";
                                    select_IDDERIVATIVEATTRIB += "from " + owner + Cst.OTCml_TBL.DERIVATIVEATTRIB;
                                    select_IDDERIVATIVEATTRIB += SQLCst.WHERE + "( IDDC = " + select_IDDC + " )";
                                    select_IDDERIVATIVEATTRIB += SQLCst.AND + "( IDMATURITY = " + select_IDMATURITY + " ) )";

                                    sqlInsertQuery.AppendLine(BuildInsertQuery(opAsset_EtdReferential, dtAsset_Etd, select_IDDERIVATIVEATTRIB).ToString());
                                    #endregion ASSET_ETD
                                }
                                #endregion MATURITY

                                #region CONTRACTCASCADING //Pas encore utilisé dans Spheres
                                /* Table CONTRACTCASCADING */
                                //DataTable dtContractCascading = DataTableContractCascading(Convert.ToInt32(rowDC["IDDC"]));
                                //DataRow[] rowsContractCascading = dtContractCascading.Select();
                                //Referential opContractCascading = new Referential();
                                //RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, GetIdMenuFromTableName("CONTRACTCASCADING"), Cst.ListType.Referential, "CONTRACTCASCADING", null, null, out opContractCascading);
                                /* Insert CONTRACTCASCADING */
                                //sqlInsertQuery.AppendLine(BuildInsertQuery(opContractCascading, dtContractCascading).ToString());
                                #endregion CONTRACTCASCADING

                                #region Deletes
                                string sql_delete;
                                /* Delete QUOTE_ETD_H */
                                sql_delete = " delete from " + owner + Cst.OTCml_TBL.QUOTE_ETD_H + " where IDASSET in (";
                                sql_delete += "select IDASSET from " + owner + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " where CONTRACTIDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM;
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete ASSET_ETD */
                                sql_delete = " delete from " + owner + Cst.OTCml_TBL.ASSET_ETD + " where IDASSET in (";
                                sql_delete += "select IDASSET from " + owner + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " where CONTRACTIDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM;
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete DERIVATIVEATTRIB */
                                sql_delete = " delete from " + owner + Cst.OTCml_TBL.DERIVATIVEATTRIB + " where IDDC in (";
                                sql_delete += "select IDDC from " + owner + Cst.OTCml_TBL.DERIVATIVECONTRACT + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM;
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete CONTRACTCASCADING */
                                //sqlDeleteQuery.AppendLine(" delete from " + owner + "CONTRACTCASCADING where IDDC in (select IDDC from " + owner + "DERIVATIVECONTRACT where IDENTIFIER=" + DataHelper.SQLString(dcIdentifier) + ");");                                        
                                /* Delete DERIVATIVECONTRACT*/
                                sql_delete = " delete from " + owner + Cst.OTCml_TBL.DERIVATIVECONTRACT + " where IDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM;
                                sql_delete += ";";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                #endregion Deletes
                            }
                        }

                        sqlQuery.Append(sqlDeleteQuery);
                        sqlQuery.Append(" ");
                        sqlQuery.Append(sqlInsertQuery);
                        sqlQuery.Append(sqlRestoreQuery);
                        break;
                        #endregion Reserved to DC

                    default:
                        //WARNING: TBD for ORACLE
                        #region Display "insert query" for current consultation
                        bool isWhereNotExists = false;
                        if (pReferential.TableName == "MATURITYRULE")
                        {
                            isWhereNotExists = true;
                        }
                        sqlQuery.Append(BuildInsertQuery(pReferential, pDatatable, isWhereNotExists).ToString());
                        break;
                        #endregion
                }

                sqlQuery.AppendLine(CommentOutput(string.Empty, '*'));
                sqlQuery.AppendLine(CommentOutput("WARNING: Replace rollback by commit", ' '));
                sqlQuery.AppendLine(CommentOutput(string.Empty, '*'));
                sqlQuery.AppendLine("  rollback;");
                sqlQuery.AppendLine(CommentOutput(string.Empty, '*'));
                sqlQuery.AppendLine("end;");
                sqlQuery.Append("/");

                sqlQuery.Append("</div>");
                sqlQuery.AppendLine("</div>");

                lstOutputQuery.Add(sqlQuery.ToString().Replace(Cst.CrLf, Cst.HTMLBreakLine + Cst.CrLf));
                sqlQuery.Clear();
            }

            sqlQuery.Length = 0;
            sqlQuery.AppendLine(@"<!DOCTYPE html>");
            sqlQuery.AppendLine(@"<html>");
            sqlQuery.AppendLine(@"  <head>");
            sqlQuery.AppendLine(@"  <meta charset=""utf-16"" />");
            sqlQuery.AppendLine(@"  <title>" + "SQL Export : " + pPage.PageFullTitle + "</title>");

            sqlQuery.AppendLine(@"<style type=""text/css""> 

            h1 {
                font-family: monospace;
                color: #036AB5;
            }
            
            div[class^=""panel panel-db""] {
            font-family: monospace;
            border-radius: 4px;
            }

            .panel-dbSQL {
            border: 2pt solid #337ab7;
            }

            .panel-dbSQL > .panel-heading {
            background-color: #337ab7;
            color: #fff;
            font-size:20px;
            }

            .panel-dbORA {
            border: 2pt solid #d43f3a;
            }

            .panel-dbORA > .panel-heading {
            background-color: #d9534f;
            color: #fff;
            font-size:20px;
            }
            
            .panel-body {
            height: calc(50vh - 110px);
            padding: 5px;
            overflow-y: auto;
            }

            </style>");

            sqlQuery.AppendLine(@"  </head>");
            sqlQuery.AppendLine(@"  <body>");

            sqlQuery.Append(@"<h1>");
            sqlQuery.AppendLine(CommentOutput(pPage.PageFullTitle, ' ') + Cst.HTMLBreakLine);
            sqlQuery.Append(CommentOutput(@"Source : " + new CSManager(SessionTools.CS).GetCSWithoutPwd(), ' ') + Cst.HTMLBreakLine);
            sqlQuery.Append(@"</h1>");

            if (0 < lstOutputQuery.Count)
            {
                int i = 1;
                lstOutputQuery.ForEach(export_script =>
                {
                    sqlQuery.Append(export_script);
                    i++;
                });

            }

            sqlQuery.AppendLine(@"  </body>");
            sqlQuery.AppendLine(@"</html>");

            Display(sqlQuery);
            ret = Cst.ErrLevel.SUCCESS;
            return ret;
        }

        private string CommentOutput()
        {
            return CommentOutput(null, '~');
        }
        private string CommentOutput(string pMsg)
        {
            return CommentOutput(pMsg, ' ');
        }
        private string CommentOutput(string pMsg, char pChar)
        {
            string ret = string.Empty;
            if (pMsg == null)
                ret = "/* " + string.Empty.PadRight(100, pChar) + " */";
            else
                ret = "/* " + pMsg.TrimEnd().PadRight(100, pChar) + " */";

            return ret;
        }
        private string PrintOutput(string pMsg, char pChar)
        {
            string ret = string.Empty;


            if (pChar == '.')
            {
                if (isOracle)
                    ret = "dbms_output.put_line('" + pMsg.PadRight(81, pChar) + "');";
                else
                    ret = "print '" + pMsg.TrimEnd().PadRight(96, pChar) + "';";
            }
            else
            {
                if (isOracle)
                    ret = "dbms_output.put_line('" + string.Empty.PadRight(81, pChar) + "');" + Cst.CrLf
                        + "dbms_output.put_line('" + DataHelper.SQLString(pMsg) + "');" + Cst.CrLf
                        + "dbms_output.put_line('" + string.Empty.PadRight(81, pChar) + "');";
                else
                    ret = "print '" + string.Empty.PadRight(96, pChar) + "';" + Cst.CrLf
                        + "print '" + DataHelper.SQLString(pMsg) + "';" + Cst.CrLf
                        + "print '" + string.Empty.PadRight(96, pChar) + "';";
            }

            return ret;
        }


        private DataTable GetDataTable(string pSqlQuery, DataParameters pParameters)
        {
            QueryParameters qry = new QueryParameters(cs, pSqlQuery, pParameters);
            DataSet ds = DataHelper.ExecuteDataset(cs, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
            return ds.Tables[0];
        }

        private DataTable DataTableActor(int pIDA)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(cs, "IDA", DbType.Int32), pIDA);
                string sqlSelect = @"select *
                from dbo.ACTOR
                where IDA = @IDA" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        #region DataTables IO
        private DataTable DataTableIOInput(string pIdIOInput)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(cs, "IDIOINPUT", DbType.String), pIdIOInput);
                string sqlSelect = @"select *
                from dbo.IOINPUT
                where IDIOINPUT = @IDIOINPUT" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOOutput(string pIdIOOutput)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOOUTPUT", DbType.String), pIdIOOutput);
                string sqlSelect = @"select *
                from dbo.IOOUTPUT
                where IDIOOUTPUT = @IDIOOUTPUT" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOShell(string pIdIOShell)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOSHELL", DbType.String), pIdIOShell);
                string sqlSelect = @"select *
                from dbo.IOSHELL
                where IDIOSHELL = @IDIOSHELL" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOTaskDet(string pTaskIdentifier)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IOTASKIDENTIFIER", DbType.String), pTaskIdentifier);
                string sqlSelect = @"select iotd.*
                from dbo.IOTASKDET iotd
                inner join dbo.IOTASK iot on (iot.IDIOTASK = iotd.IDIOTASK)
                where iot.IDENTIFIER = @IOTASKIDENTIFIER
                order by iotd.SEQUENCENO asc" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOInputParsing(string pIdioInput)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOINPUT", DbType.String), pIdioInput);
                string sqlSelect = @"select *
                from dbo.IOINPUT_PARSING
                where IDIOINPUT = @IDIOINPUT" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOOutputParsing(string pIdioInput)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOOUTPUT", DbType.String), pIdioInput);
                string sqlSelect = @"select *
                from dbo.IOOUTPUT_PARSING
                where IDIOOUTPUT = @IDIOOUTPUT" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOParsing(string pIdioParsing)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOPARSING", DbType.String), pIdioParsing);
                string sqlSelect = @"select *
                from dbo.IOPARSING
                where IDIOPARSING = @IDIOPARSING" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOParsingDet(string pIdioParsing)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOPARSING", DbType.String), pIdioParsing);
                string sqlSelect = @"select *
                from dbo.IOPARSINGDET
                where IDIOPARSING = @IDIOPARSING
                order by SEQUENCENO asc" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOTaskParam(string pTaskIdentifier)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IOTASKIDENTIFIER", DbType.String), pTaskIdentifier);
                string sqlSelect = @"select iotp.*
                from dbo.IOTASK_PARAM iotp
                inner join dbo.IOTASK iot on (iot.IDIOTASK = iotp.IDIOTASK)
                where iot.IDENTIFIER = @IOTASKIDENTIFIER
                order by iotp.SEQUENCENO asc" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOParam(string pIdIOParam)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOPARAM", DbType.String), pIdIOParam);
                string sqlSelect = @"select *
                from dbo.IOPARAM
                where IDIOPARAM = @IDIOPARAM" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOParamDet(string pIdIOParam)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOPARAM", DbType.String), pIdIOParam);
                string sqlSelect = @"select *
                from dbo.IOPARAMDET
                where IDIOPARAM = @IDIOPARAM
                order by SEQUENCENO asc" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private DataTable DataTableIOCompare(string pIdIOCompare)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDIOCOMPARE", DbType.String), pIdIOCompare);
                string sqlSelect = @"select *
                from dbo.IOCOMPARE
                where IDIOCOMPARE = @IDIOCOMPARE" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion End DataTables IO

        #region DataTables DC
        /* ASSET_ETD */
        private DataTable DataTableAsset_Etd(string pDCIdentifier, int pIDMaturity)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "CONTRACTIDENTIFIER", DbType.String), pDCIdentifier);
                parameters.Add(new DataParameter(CurrentPage.CS, "IDMATURITY", DbType.Int32), pIDMaturity);
                string sqlSelect = @"select a.*
                from dbo.ASSET_ETD a
                inner join dbo.VW_ASSET_ETD_EXPANDED v on (v.IDASSET = a.IDASSET)
                inner join dbo.DERIVATIVEATTRIB da on (da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB)
                where (v.CONTRACTIDENTIFIER = @CONTRACTIDENTIFIER) and (da.IDMATURITY = @IDMATURITY)" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /* INSTRUMENT */
        private DataTable DataTableInstrument(int pIDDC)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDDC", DbType.Int32), pIDDC);
                string sqlSelect = @"select i.IDENTIFIER*
                from dbo.INSTRUMENT i
                inner join dbo.VW_ASSET_ETD_EXPANDED v on (v.IDASSET = a.IDASSET)
                inner join dbo.DERIVATIVECONTRACT dc on (dc.IDI = i.IDI) and 
                where (dc.IDDC = @IDDC)" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /* ASSET_EQUITY */
        private DataTable DataTableAsset_Equity(int pIDASSET_UNL)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDASSET", DbType.Int32), pIDASSET_UNL);
                string sqlSelect = @"select *
                from dbo.ASSET_EQUITY 
                where IDASSET = @IDASSET" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }


        /* ASSET_INDEX */
        private DataTable DataTableAsset_Index(int pIDASSET_UNL)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDASSET", DbType.Int32), pIDASSET_UNL);
                string sqlSelect = @"select *
                from dbo.ASSET_INDEX
                where IDASSET = @IDASSET" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /* DERIVATIVEATTRIB */
        private DataTable DataTableDerivativeAttrib(int pIDDC, int pIDMaturity)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDDC", DbType.Int32), pIDDC);
                parameters.Add(new DataParameter(CurrentPage.CS, "IDMATURITY", DbType.Int32), pIDMaturity);
                string sqlSelect = @"select *
                from dbo.DERIVATIVEATTRIB
                where (IDDC = @IDDC) and (IDMATURITY = @IDMATURITY)" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /* CONTRACTCASCADING */
        // Pas encore utilisé dans Spheres
        //private DataTable DataTableContractCascading(int pIdDC)
        //{
        //    try
        //    {
        //        DataParameters parameters = new DataParameters();
        //        parameters.Add(new DataParameter(CurrentPage.CS, "IDDC", DbType.Int32), pIdDC);
        //        string sqlSelect = @"select *
        //        from dbo.CONTRACTCASCADING
        //        where (IDDC = @IDDC)" + Cst.CrLf;
        //        return GetDataTable(sqlSelect, parameters);
        //    }
        //    catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        //}

        /* MATURITY */
        private DataTable DataTableMaturity(int pIdMaturityRule)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDMATURITYRULE", DbType.Int32), pIdMaturityRule);
                string sqlSelect = @"select *
                from dbo.MATURITY
                where (IDMATURITYRULE = @IDMATURITYRULE)" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /* MATURITYRULE */
        private DataTable DataTableMaturityRule(int pIdDC)
        {
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDDC", DbType.Int32), pIdDC);
                string sqlSelect = @"select *
                from dbo.MATURITYRULE
                where (IDMATURITYRULE =
                (select dc.IDMATURITYRULE from dbo.DERIVATIVECONTRACT dc where dc.IDDC = @IDDC))" + Cst.CrLf;
                return GetDataTable(sqlSelect, parameters);
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion End DataTables DC

        /// <summary>
        /// ExploreChildTables
        /// Find child tables and explore child tables datas
        /// </summary>
        /// <param name="pIdMenuParentTable"></param>
        /// <param name="pParentReferential"></param>
        /// <param name="pParentRow"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pSuffix"></param>
        /// <param name="pSQLScript"></param>
        /// FI 20160804 [Migration TFS] Modfify
        private void ExploreChildTables(string pIdMenuParentTable, Referential pParentReferential, DataRow pParentRow, string pPrefix, string pSuffix, ref StringBuilder pSQLScript)
        {
            try
            {
                // Retrieve child tables through Spheres menus
                List<String> lstIdMenuChildTables = RetreiveChildMenuList(pIdMenuParentTable);

                if (0 < lstIdMenuChildTables.Count)
                {
                    // For each child table
                    foreach (string item in lstIdMenuChildTables)
                    {
                        string childTableUrl = GetUrl(item);
                        string childTableName = GetTableName(childTableUrl);
                        string parentTableName = pParentReferential.TableName;

                        if (IsTable(childTableName) == true)
                        {
                            Referential opChildReferential = new Referential();
                            RepositoryTools.DeserializeXML_ForModeRW_25(CurrentPage.CS, item, Cst.ListType.Repository, childTableName, null, null, null, out opChildReferential);
                            // Find the column name which is the foreign key (column IDACCKEY of the table ACCKEYVALUE)
                            string fkColumn = opChildReferential.Column[opChildReferential.IndexForeignKeyField].ColumnName;
                            // Find the value of the fk column (here from the parent table)
                            string fkValue = pParentRow.ItemArray[pParentReferential.IndexColSQL_DataKeyField].ToString();
                            string parentIdentifierValue = pParentRow.ItemArray[pParentReferential.IndexColSQL_KeyField].ToString();

                            DataSet childDs = SQLReferentialData.RetrieveDataFromSQLTable(CurrentPage.CS, opChildReferential, fkColumn, fkValue, false, false, true);
                            DataTable childDt = childDs.Tables[0];

                            foreach (DataRow childRow in childDt.Rows)
                            {
                                string sqlInsertQueryChild = BuildComplexInsertQuery(pPrefix, pSuffix, opChildReferential, childRow, fkColumn, parentTableName, parentIdentifierValue);
                                pSQLScript.Append(sqlInsertQueryChild);
                                ExploreChildTables(item, opChildReferential, childRow, pPrefix, pSuffix, ref pSQLScript);
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        private bool IsColumnOk(Referential pReferential, ReferentialColumn pRc)
        {
            bool isOk = false;

            isOk = ((!pRc.IsIdentity.Value) || (pRc.IsIdentity.sourceSpecified))
                && (pRc.ColumnName != "ROWVERSION")
                && (!pRc.IsAdditionalData)
                && (!pRc.Ressource.StartsWith("NotePad"))
                && (!pRc.Ressource.StartsWith("AttachedDoc"))
                && (!pRc.IsVirtualColumn);

            if (isOk && pReferential.TableName.StartsWith("IO"))
                isOk = (pRc.ColumnName != "IDAUPD") && (pRc.ColumnName != "DTUPD");
            if (isOk && pRc.AliasTableNameSpecified && pRc.AliasTableName.EndsWith("_S"))
                isOk = false;

            return isOk;
        }

        /// <summary>
        /// BuildColumnList: returns list of columns from a table
        /// </summary>
        /// <param name="pReferential"></param>
        private string BuildColumnList(Referential pReferential)
        {
            try
            {
                string columnList = string.Empty;

                for (int i = 0; i < pReferential.Column.Length; i++)
                {
                    ReferentialColumn rc = pReferential.Column[i];
                    if (IsColumnOk(pReferential, rc))
                        columnList += ", " + rc.ColumnName;
                }
                columnList = SQLCst.X_INSERT + owner + pReferential.TableName + "(" + columnList.TrimStart(',') + ")" + Cst.CrLf;

                return columnList;
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /// <summary>
        /// returns query insert from table
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pReferential"></param>
        /// <param name="pDatatable"></param>
        private StringBuilder BuildInsertQuery(Referential pReferential, DataTable pDatatable)
        {
            return BuildInsertQuery(pReferential, pDatatable, false, null);
        }
        private StringBuilder BuildInsertQuery(Referential pReferential, DataTable pDatatable, params string[] pPkValue)
        {
            return BuildInsertQuery(pReferential, pDatatable, false, pPkValue);
        }
        private StringBuilder BuildInsertQuery(Referential pReferential, DataTable pDatatable, bool pIsWhereNotExists, params string[] pPkValue)
        {
            try
            {
                string parameterPrefix = string.Empty;
                if (false == isOracle)
                    parameterPrefix = "@";

                StringBuilder sqlInsertQuery = new StringBuilder();

                string columnList = BuildColumnList(pReferential);
                bool isFirstColum = false;

                foreach (DataRow row in pDatatable.Rows)
                {
                    if (pReferential.TableName == "IOTASK")
                    {
                        string msg = pReferential.TableName + ": " + row["IDENTIFIER"].ToString();
                        sqlInsertQuery.AppendLine(PrintOutput(msg, '.'));
                    }
                    else if (pReferential.TableName.StartsWith("IO") && !pReferential.TableName.EndsWith("DET"))
                    {
                        string msg = null;
                        if (pReferential.TableName.EndsWith("_PARSING"))
                            msg = pReferential.TableName + ": " + row["IDIOPARSING"].ToString();
                        else if (pReferential.TableName.EndsWith("_PARAM"))
                            msg = pReferential.TableName + ": " + row["IDIOPARAM"].ToString();
                        else
                            msg = pReferential.TableName + ": " + row["ID" + pReferential.TableName].ToString();
                        sqlInsertQuery.AppendLine(PrintOutput(msg, '.'));
                    }

                    isFirstColum = true;

                    sqlInsertQuery.Append(columnList);

                    if (pIsWhereNotExists)
                    {
                        //insert into T(A,B,C) select 'x','y','z' from dbo.DUAL where not exists (select 1 dbo.T where A='x');
                        sqlInsertQuery.Append("select ");
                    }
                    else
                    {
                        //insert into T(A,B,C) values ('x','y','z');
                        sqlInsertQuery.Append("values (");
                    }

                    for (int i = 0; i < pReferential.Column.Length; i++)
                    {
                        ReferentialColumn rc = pReferential.Column[i];
                        if (IsColumnOk(pReferential, rc))
                        {
                            if (isFirstColum)
                                isFirstColum = false;
                            else
                                sqlInsertQuery.Append(", ");

                            if ((row[rc.ColumnName] is DBNull) && (rc.ColumnName != "IDA") && (rc.ColumnName != "IDAINS") && (rc.ColumnName != "DTINS") && (rc.ColumnName != "DTENABLED") && (rc.ColumnName != "IDMATURITYRULE"))
                            {
                                sqlInsertQuery.Append(SQLCst.NULL);
                            }
                            else if (TypeData.IsTypeInt(rc.DataType.value))
                            {
                                switch (rc.ColumnName)
                                {
                                    case "IDIOTASK":
                                    case "IDMATURITY":
                                    case "IDI":
                                    case "IDDERIVATIVEATTRIB":
                                        sqlInsertQuery.Append(pPkValue[0]);
                                        break;
                                    case "IDMATURITYRULE":
                                        sqlInsertQuery.Append(Array.Find(pPkValue, s => s.Contains("IDMATURITYRULE")));
                                        break;
                                    case "IDASSET_UNL":
                                        sqlInsertQuery.Append(Array.Find(pPkValue, s => s.Contains("IDASSET")));
                                        break;
                                    case "IDI_UNL":
                                    case "IDBCMDT":
                                    case "IDDC":
                                    case "IDDC_CASC":
                                    case "IDDC_UNL":
                                    case "IDM":
                                    case "IDM_RELATED":
                                    case "IDC_PRICE":
                                    case "IDC_NOMINAL":
                                    case "IDACCCONDITION":
                                    case "IDFEE":
                                    case "IDFEESCHEDULE":
                                    case "IDINSTR":
                                    case "IDCONTRACT":
                                        string selectPkValue = SQLCst.NULL;
                                        if (row[rc.ColumnName] != Convert.DBNull)
                                        {
                                            string tableName = null;
                                            string columnName = rc.ColumnName;
                                            switch (rc.ColumnName)
                                            {
                                                case "IDI_UNL":
                                                    tableName = "INSTRUMENT";
                                                    columnName = "IDI";
                                                    break;
                                                case "IDBCMDT":
                                                    tableName = "BUSINESSCENTER";
                                                    columnName = "IDBC";
                                                    break;
                                                case "IDDC":
                                                case "IDDC_CASC":
                                                case "IDDC_UNL":
                                                    tableName = "DERIVATIVECONTRACT";
                                                    columnName = "IDDC";
                                                    break;
                                                case "IDM":
                                                case "IDM_RELATED":
                                                    tableName = "MARKET";
                                                    columnName = "IDM";
                                                    break;
                                                case "IDC_PRICE":
                                                case "IDC_NOMINAL":
                                                    tableName = "CURRENCY";
                                                    columnName = "IDC";
                                                    break;
                                                case "IDACCCONDITION":
                                                    tableName = "ACCCONDITION";
                                                    columnName = "IDACCCONDITION";
                                                    break;
                                                case "IDFEE":
                                                    tableName = "FEE";
                                                    columnName = "IDFEE";
                                                    break;
                                                case "IDFEESCHEDULE":
                                                    tableName = "FEESCHEDULE";
                                                    columnName = "IDFEESCHEDULE";
                                                    break;
                                                case "IDINSTR":
                                                    tableName = "INSTRUMENT";
                                                    columnName = "IDI";
                                                    if ((i > 0) && (pReferential.Column[i - 1].ColumnName == "TYPEINSTR"))
                                                    {
                                                        if (row["TYPEINSTR"].ToString() == "Product")
                                                        {
                                                            tableName = "PRODUCT";
                                                            columnName = "IDP";
                                                        }
                                                        else if (row["TYPEINSTR"].ToString() == "GrpInstr")
                                                        {
                                                            tableName = "GINSTR";
                                                            columnName = "IDGINSTR";
                                                        }
                                                    }
                                                    break;
                                                case "IDCONTRACT":
                                                    tableName = "DERIVATIVECONTRACT";
                                                    columnName = "IDDC";
                                                    if ((i > 0) && (pReferential.Column[i - 1].ColumnName == "TYPECONTRACT"))
                                                    {
                                                        if (row["TYPECONTRACT"].ToString() == "Market")
                                                        {
                                                            tableName = "MARKET";
                                                            columnName = "IDM";
                                                        }
                                                        else if (row["TYPECONTRACT"].ToString() == "GrpMarket")
                                                        {
                                                            tableName = "GMARKET";
                                                            columnName = "IDGMARKET";
                                                        }
                                                        else if (row["TYPECONTRACT"].ToString() == "GrpContract")
                                                        {
                                                            tableName = "GCONTRACT";
                                                            columnName = "IDGCONTRACT";
                                                        }
                                                    }
                                                    break;
                                            }
                                            string top1 = string.Empty;
                                            string addFilter = string.Empty;
                                            string dataIdentifier = null;
                                            string data2Identifier = null;
                                            string sqlSelectPkValue = "select IDENTIFIER from " + owner + tableName + " where " + columnName + "=@DataKey";
                                            DataParameters parameters = new DataParameters();
                                            // EG 20150920 [21314] Int (int32) to Long (Int64) 
                                            parameters.Add(new DataParameter(cs, "DataKey", DbType.Int64), row[rc.ColumnName]);
                                            IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                            if (dr.Read())
                                                dataIdentifier = dr.GetString(0);
                                            dr.Close();

                                            if (tableName == "DERIVATIVECONTRACT")
                                            {
                                                //Cas particulier des DC
                                                sqlSelectPkValue = "select v.SHORT_ACRONYM from " + owner + tableName + " t" + Cst.CrLf;
                                                sqlSelectPkValue += "inner join dbo.VW_MARKET_IDENTIFIER v on v.IDM=t.IDM" + Cst.CrLf;
                                                sqlSelectPkValue += "where t." + columnName + "=@DataKey";
                                                dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                                if (dr.Read())
                                                {
                                                    data2Identifier = dr.GetString(0);
                                                    if (!String.IsNullOrEmpty(data2Identifier))
                                                        addFilter = " and IDM=(select IDM from " + owner + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " where SHORT_ACRONYM=" + DataHelper.SQLString(data2Identifier) + ")";
                                                }
                                                dr.Close();
                                            }
                                            else if (tableName == "FEESCHEDULE")
                                            {
                                                top1 = "top 1 "; //TBD on Oracle

                                                sqlSelectPkValue = "select IDENTIFIER from " + owner + Cst.OTCml_TBL.FEE.ToString() + " t" + Cst.CrLf;
                                                sqlSelectPkValue += "where IDFEE=@DataKey";
                                                parameters["DataKey"].Value = row["IDFEE"];
                                                dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                                if (dr.Read())
                                                {
                                                    data2Identifier = dr.GetString(0);
                                                    if (!String.IsNullOrEmpty(data2Identifier))
                                                        addFilter = " and IDFEE=(select IDFEE from " + owner + Cst.OTCml_TBL.FEE.ToString() + " where IDENTIFIER=" + DataHelper.SQLString(data2Identifier) + ")";
                                                }
                                                dr.Close();
                                            }

                                            selectPkValue = "(select " + top1 + columnName + " from " + owner + "" + tableName + " where IDENTIFIER=" + DataHelper.SQLString(dataIdentifier) + addFilter + ")";
                                        }
                                        sqlInsertQuery.Append(selectPkValue);
                                        break;
                                    case "IDA":
                                        if (pDatatable.TableName == "MARKET")
                                        {
                                            //BD 20130514 Dans le cas de l'export de MARKET, on select IDA sur la table ACTOR
                                            string tableName = null;
                                            string columnName = rc.ColumnName;
                                            tableName = "ACTOR";
                                            columnName = "IDA";
                                            string instrumentIdentifier = null;
                                            string sqlSelectPkValue = "select IDENTIFIER from dbo." + tableName + " where " + columnName + "=@DataKey";
                                            DataParameters parameters = new DataParameters();
                                            parameters.Add(new DataParameter(cs, "DataKey", DbType.Int32), row[rc.ColumnName]);
                                            IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                            if (dr.Read())
                                                instrumentIdentifier = dr.GetString(0);
                                            dr.Close();
                                            selectPkValue = "(select " + columnName + " from " + owner + tableName + " where IDENTIFIER=" + DataHelper.SQLString(instrumentIdentifier) + ")";
                                            sqlInsertQuery.Append(selectPkValue);
                                        }
                                        else
                                            sqlInsertQuery.Append(parameterPrefix + "Ida");
                                        break;
                                    case "IDAINS":
                                        sqlInsertQuery.Append(parameterPrefix + "IdaIns");
                                        break;
                                    case "IDAUPD":
                                        sqlInsertQuery.Append(SQLCst.NULL);
                                        break;
                                    default:
                                        sqlInsertQuery.Append(row[rc.ColumnName].ToString());
                                        break;
                                }
                            }
                            else if (TypeData.IsTypeDec(rc.DataType.value))
                            {
                                decimal decValue = DecFunc.DecValue(row[rc.ColumnName].ToString());
                                string strVaue = StrFunc.FmtDecimalToInvariantCulture(decValue);
                                sqlInsertQuery.Append(strVaue);
                            }
                            else if (TypeData.IsTypeDate(rc.DataType.value))
                            {
                                switch (rc.ColumnName)
                                {
                                    case "DTINS":
                                        sqlInsertQuery.Append(parameterPrefix + "DtIns");
                                        break;
                                    case "DTENABLED":
                                        sqlInsertQuery.Append(parameterPrefix + "DtEnabled");
                                        break;
                                    case "DTDISABLED":
                                        sqlInsertQuery.Append("DtDisabled");
                                        break;
                                    case "DTUPD":
                                        sqlInsertQuery.Append(SQLCst.NULL);
                                        break;
                                    default:
                                        sqlInsertQuery.Append(DataHelper.SQLToDate(dbSvrType, Convert.ToDateTime(row[rc.ColumnName])));
                                        break;
                                }
                            }
                            else if (TypeData.IsTypeBool(rc.DataType.value))
                            {
                                sqlInsertQuery.Append(DataHelper.SQLBoolean(Convert.ToBoolean(row[rc.ColumnName])));
                            }
                            else
                            {
                                switch (rc.ColumnName)
                                {
                                    case "ACTION": //FEESCHEDULE & FEEMATRIX
                                        string dataIdAction = null;
                                        string sqlSelectAction = "select MNU_PERM_DESC from " + owner + "VW_ALL_VW_PERMIS_MENU where IDPERMISSION=@DataKey";
                                        DataParameters parameters2 = new DataParameters();
                                        parameters2.Add(new DataParameter(cs, "DataKey", DbType.String, 128), row[rc.ColumnName]);
                                        IDataReader drAction = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectAction, parameters2.GetArrayDbParameter());
                                        if (drAction.Read())
                                            dataIdAction = drAction.GetString(0);
                                        drAction.Close();

                                        string selectValue = "(select IDPERMISSION from " + owner + "VW_ALL_VW_PERMIS_MENU where MNU_PERM_DESC=" + DataHelper.SQLString(dataIdAction) + ")";
                                        sqlInsertQuery.Append(selectValue);
                                        break;
                                    default:
                                        sqlInsertQuery.Append(DataHelper.SQLString(row[rc.ColumnName].ToString()));
                                        break;
                                }
                            }
                        }
                    }

                    if (pIsWhereNotExists)
                    {
                        switch (pReferential.TableName)
                        {
                            case "MATURITYRULE":
                                sqlInsertQuery.Append(Cst.CrLf + "from " + owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + owner + "MATURITYRULE" + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(row["IDENTIFIER"].ToString()));
                                break;
                            case "MATURITY":
                                sqlInsertQuery.Append(Cst.CrLf + "from " + owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + owner + "MATURITY" + SQLCst.WHERE + "IDMATURITYRULE=" + Array.Find(pPkValue, s => s.Contains("IDMATURITYRULE")) + SQLCst.AND + "MATURITYMONTHYEAR=" + DataHelper.SQLString(row["MATURITYMONTHYEAR"].ToString()));
                                break;
                            case "ASSET_EQUITY":
                            case "ASSET_INDEX":
                                sqlInsertQuery.Append(Cst.CrLf + "from " + owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + owner + pReferential.TableName + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(row["IDENTIFIER"].ToString()));
                                break;
                        }
                    }

                    sqlInsertQuery.Append(");" + Cst.CrLf);

                }
                return sqlInsertQuery;
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }


        /// <summary>
        /// Builds ACCMODEL table specific insert query
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pNewId"></param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private string BuildAccModelInsertQuery(Referential pReferential, string pNewId, DataRow pRow)
        {
            try
            {
                string parameterPrefix = string.Empty;
                if (false == isOracle)
                    parameterPrefix = "@";

                string sqlInsertQuery = String.Empty;
                string columnList = BuildColumnList(pReferential);
                bool isOk = false;
                DataRow row = pRow;
                // pNewId is a parameters from URL
                // it will be valorize when we want to duplicate accountig model
                string newAccountingModel = pNewId;
                // when we want to extract an existed accounting model the newAccountingModel variable must be empty  
                if (newAccountingModel == null)
                    newAccountingModel = pRow["IDENTIFIER"].ToString();
                //
                sqlInsertQuery += columnList;
                sqlInsertQuery += "values (";

                for (int i = 0; i < pReferential.Column.Length; i++)
                {
                    ReferentialColumn rrc = pReferential.Column[i];
                    isOk = ((!rrc.IsIdentity.Value) || (rrc.IsIdentity.sourceSpecified))
                        && (rrc.ColumnName != "ROWVERSION")
                        && (!rrc.IsAdditionalData)
                        && (!rrc.Ressource.StartsWith("NotePad"))
                        && (!rrc.Ressource.StartsWith("AttachedDoc"))
                        && (!rrc.IsVirtualColumn);
                    if (isOk)
                    {
                        sqlInsertQuery += ", ";
                        if ((row[rrc.ColumnName] is DBNull) && (rrc.ColumnName != "IDAINS") && (rrc.ColumnName != "DTINS") && (rrc.ColumnName != "DTENABLED"))
                        {
                            sqlInsertQuery += SQLCst.NULL;
                        }
                        else if (TypeData.IsTypeNumeric(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                case "IDAINS":
                                    sqlInsertQuery += parameterPrefix + "IdaIns";
                                    break;
                                case "IDAUPD":
                                    sqlInsertQuery += SQLCst.NULL;
                                    break;
                                default:
                                    sqlInsertQuery += row[rrc.ColumnName].ToString();
                                    break;
                            }
                        }
                        else if (TypeData.IsTypeDate(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                case "DTINS":
                                    sqlInsertQuery += parameterPrefix + "DtIns";
                                    break;
                                case "DTENABLED":
                                    sqlInsertQuery += parameterPrefix + "DtEnabled";
                                    break;
                                case "DTDISABLED":
                                    // GS 20130117: enable disable date extraction
                                    sqlInsertQuery += parameterPrefix + "DtEnabled";
                                    //sqlInsertQuery += SQLCst.NULL;
                                    break;
                                case "DTUPD":
                                    sqlInsertQuery += SQLCst.NULL;
                                    break;
                                default:
                                    sqlInsertQuery += DataHelper.SQLToDate(CurrentPage.CS, Convert.ToDateTime(row[rrc.ColumnName]));
                                    break;
                            }
                        }
                        //
                        else if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            sqlInsertQuery += DataHelper.SQLBoolean(Convert.ToBoolean(row[rrc.ColumnName]));
                        }
                        else
                        {
                            if (rrc.ColumnName == "IDENTIFIER")
                                sqlInsertQuery += DataHelper.SQLString(newAccountingModel.ToString());
                            else if (rrc.ColumnName == "DISPLAYNAME")
                                sqlInsertQuery += DataHelper.SQLString(newAccountingModel.ToString());
                            else if (rrc.ColumnName == "DESCRIPTION")
                                sqlInsertQuery += DataHelper.SQLString("Schemi contabili (accounting rules) secondo le regole di ".ToString() + newAccountingModel);
                            else
                                sqlInsertQuery += DataHelper.SQLString(row[rrc.ColumnName].ToString());
                        }
                        //sqlInsertQuery += ",";
                    }
                }
                //sqlInsertQuery = sqlInsertQuery.TrimEnd(',');
                sqlInsertQuery = sqlInsertQuery.TrimStart(',');
                sqlInsertQuery += ");" + Cst.CrLf;

                return sqlInsertQuery;
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        }

        /// <summary>
        /// builds complex insert query 
        /// specific for data type, columns, tables
        /// </summary>
        /// <param name="pPrefix"></param>
        /// <param name="pSuffix"></param>
        /// <param name="pReferential"></param>
        /// <param name="pRow"></param>
        /// <param name="pFkParentColumn"></param>
        /// <param name="pParentTable"></param>
        /// <param name="pFkParentValue"></param>
        /// <returns></returns>
        private string BuildComplexInsertQuery(string pPrefix, string pSuffix, Referential pReferential, DataRow pRow, string pFkParentColumn, string pParentTable, string pFkParentValue)
        {
            string parameterPrefix = string.Empty;
            if (false == isOracle)
                parameterPrefix = "@";

            string sqlInsertQuery = string.Empty;
            string columnList = BuildColumnList(pReferential);
            bool isOk = false;
            string prefix = pPrefix;
            string suffix = pSuffix;

            DataRow row = pRow;
            sqlInsertQuery += columnList;
            sqlInsertQuery += "values (";

            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                ReferentialColumn rrc = pReferential.Column[i];

                isOk = ((!rrc.IsIdentity.Value) || (rrc.IsIdentity.sourceSpecified))
                    && (rrc.ColumnName != "ROWVERSION")
                    && (!rrc.IsAdditionalData)
                    && (!rrc.Ressource.StartsWith("NotePad"))
                    && (!rrc.Ressource.StartsWith("AttachedDoc"))
                    && (!rrc.IsVirtualColumn);
                if (isOk)
                {
                    sqlInsertQuery += ",";
                    if (rrc.IsForeignKeyField == true)
                    {
                        sqlInsertQuery += "(" + SQLCst.SELECT + "max( " + pFkParentColumn + " )" + SQLCst.X_FROM + owner + pParentTable + ")";
                    }
                    else
                    {
                        if ((row[rrc.ColumnName] is DBNull) && (rrc.ColumnName != "IDAINS") && (rrc.ColumnName != "DTINS") && (rrc.ColumnName != "DTENABLED"))
                        {
                            sqlInsertQuery += SQLCst.NULL;
                        }
                        else if (TypeData.IsTypeNumeric(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                // V1: add static condition for ACCKEYVALUE
                                case "IDACCKEY":
                                    if (pReferential.TableName == "ACCKEYVALUE")
                                    {
                                        Int32 currentIdAccKey = Convert.ToInt32(row[rrc.ColumnName].ToString());
                                        Int32 currentIdAccInstrEnvDet = GetCurrentIdAccInstrEnvDet(row);
                                        string currentIdAccEnum = GetIdAccKeyEnum(currentIdAccKey, currentIdAccInstrEnvDet);
                                        sqlInsertQuery +=
                                        "("
                                        + SQLCst.SELECT + " vw.IDACCKEY " + SQLCst.X_FROM + owner + " VW_ACCKEY vw "
                                        + SQLCst.X_INNER + owner + Cst.OTCml_TBL.ACCKEY + " ak " + SQLCst.ON + " (ak.IDACCKEY = vw.IDACCKEY) "
                                        + SQLCst.WHERE + " vw.IDACCKEYENUM = " + DataHelper.SQLString(currentIdAccEnum)
                                        + SQLCst.AND + " ak.IDACCINSTRENV =  "
                                        + "("
                                        + SQLCst.SELECT + " IDACCINSTRENV " + SQLCst.X_FROM + owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString()
                                        + SQLCst.WHERE + " IDACCINSTRENVDET = "
                                        + "("
                                        + SQLCst.SELECT + "max(IDACCINSTRENVDET)" + SQLCst.X_FROM + owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString()
                                        + ")"
                                        + ")"
                                        + ")";
                                    }
                                    break;
                                // GS 20130129 to create dynamic select add others pk column in this section
                                case "IDACCCONDITION":
                                    string selectPkValue = SQLCst.NULL;
                                    if (row[rrc.ColumnName] != Convert.DBNull)
                                    {
                                        string tableName = null;
                                        string columnName = rrc.ColumnName;
                                        switch (rrc.ColumnName)
                                        {
                                            case "IDACCCONDITION":
                                                tableName = "ACCCONDITION";
                                                columnName = "IDACCCONDITION";
                                                break;
                                            // GS 20130129 to create dynamic select add others pk column and table in this section
                                        }
                                        string instrumentIdentifier = null;
                                        string sqlSelectPkValue = "select IDENTIFIER from dbo." + tableName + " where " + columnName + "=@DataKey";
                                        DataParameters parameters = new DataParameters();
                                        parameters.Add(new DataParameter(cs, "DataKey", DbType.Int32), row[rrc.ColumnName]);
                                        IDataReader dr = DataHelper.ExecuteReader(cs, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                        if (dr.Read())
                                            instrumentIdentifier = dr.GetString(0);
                                        dr.Close();
                                        selectPkValue = "(select " + columnName + " from " + owner + "" + tableName + " where IDENTIFIER=" + DataHelper.SQLString(instrumentIdentifier) + ")";
                                    }
                                    sqlInsertQuery += selectPkValue;
                                    break;
                                case "IDAINS":
                                    sqlInsertQuery += parameterPrefix + "IdaIns";
                                    break;
                                case "IDAUPD":
                                    sqlInsertQuery += SQLCst.NULL;
                                    break;
                                default:
                                    sqlInsertQuery += row[rrc.ColumnName].ToString();
                                    break;
                            }
                        }
                        else if (TypeData.IsTypeDate(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                case "DTINS":
                                    sqlInsertQuery += parameterPrefix + "DtIns";
                                    break;
                                case "DTENABLED":
                                    sqlInsertQuery += parameterPrefix + "DtEnabled";
                                    break;
                                // GS 20130117: enable disable date extraction
                                case "DTDISABLED":
                                    sqlInsertQuery += parameterPrefix + "DtDisabled";
                                    //sqlInsertQuery += SQLCst.NULL;
                                    break;
                                case "DTUPD":
                                    sqlInsertQuery += SQLCst.NULL;
                                    break;
                                default:
                                    sqlInsertQuery += DataHelper.SQLToDate(CurrentPage.CS, Convert.ToDateTime(row[rrc.ColumnName]));
                                    break;
                            }
                        }
                        else if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            sqlInsertQuery += DataHelper.SQLBoolean(Convert.ToBoolean(row[rrc.ColumnName]));
                        }
                        else
                        {
                            // V1. special rules for ACCLABEL table 
                            // the field IDACCLABEL is key (PK_ACCLABEL index)
                            // the field DISPLAYNAME is key ( UX_ACCLABEL index)
                            if (pReferential.TableName == "ACCLABEL")
                            {
                                if (rrc.ColumnName == "IDACCLABEL" | rrc.ColumnName == "DISPLAYNAME")
                                    sqlInsertQuery += DataHelper.SQLString(prefix + row[rrc.ColumnName].ToString() + suffix);
                                else
                                    sqlInsertQuery += DataHelper.SQLString(row[rrc.ColumnName].ToString());
                            }
                            else
                            {
                                //  (rrc.IsKeyField == true && rrc.IsDataKeyField == false)
                                if (rrc.ColumnName == "IDENTIFIER" | rrc.ColumnName == "DISPLAYNAME")
                                    sqlInsertQuery += DataHelper.SQLString(prefix + row[rrc.ColumnName].ToString() + suffix);
                                else
                                    sqlInsertQuery += DataHelper.SQLString(row[rrc.ColumnName].ToString());
                            }
                        }
                    }
                    //sqlInsertQuery += ",";
                }
            }
            //sqlInsertQuery = sqlInsertQuery.TrimEnd(',');
            sqlInsertQuery = sqlInsertQuery.TrimStart(',');
            sqlInsertQuery += ");" + Cst.CrLf;

            return sqlInsertQuery;
        }

        /// <summary>
        /// Returns IDACCINSTRENVDET from datarow
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private static Int32 GetCurrentIdAccInstrEnvDet(DataRow pRow)
        {
            Int32 currentIdAccInstrEnvDetValue = Int32.MinValue;
            currentIdAccInstrEnvDetValue = Convert.ToInt32(pRow["IDACCINSTRENVDET"].ToString());
            return currentIdAccInstrEnvDetValue;
        }

        /// <summary>
        /// Returns IDACCKEYENUM value from VW_ACCKEY view 
        /// </summary>
        /// <param name="pCurrentIdAccKey"></param>
        /// <param name="pCurrentIdAccInstrEnvDet"></param>
        /// <returns></returns>
        private string GetIdAccKeyEnum(Int32 pCurrentIdAccKey, Int32 pCurrentIdAccInstrEnvDet)
        {
            IDataReader dr = null;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDACCKEY", DbType.Int32), pCurrentIdAccKey);
                parameters.Add(new DataParameter(CurrentPage.CS, "IDACCINSTRENVDET", DbType.Int32), pCurrentIdAccInstrEnvDet);
                StrBuilder sqlSelect = new StrBuilder();
                sqlSelect += SQLCst.SELECT + " vw.IDACCKEYENUM as IDACCKEYENUM " + Cst.CrLf;
                sqlSelect += SQLCst.X_FROM + owner + "VW_ACCKEY" + " vw " + Cst.CrLf;
                sqlSelect += SQLCst.X_INNER + owner + Cst.OTCml_TBL.ACCKEY + " ak " + SQLCst.ON + " (ak.IDACCKEY = vw.IDACCKEY) " + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "vw.IDACCKEY = @IDACCKEY " + Cst.CrLf;
                sqlSelect += SQLCst.AND + "ak.IDACCINSTRENV =  " + Cst.CrLf;
                sqlSelect += "(" + Cst.CrLf;
                sqlSelect += SQLCst.SELECT + "IDACCINSTRENV " + Cst.CrLf;
                sqlSelect += SQLCst.X_FROM + owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString() + Cst.CrLf;
                sqlSelect += SQLCst.WHERE + "IDACCINSTRENVDET = @IDACCINSTRENVDET" + Cst.CrLf;
                sqlSelect += ")" + Cst.CrLf;
                dr = DataHelper.ExecuteReader(CurrentPage.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
                if (dr.Read())
                    return Convert.ToString((dr["IDACCKEYENUM"]));
                else
                    return String.Empty;
            }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// returns 
        /// true if the child menu shows a table 
        /// false if the child menu shows a view
        /// </summary>
        /// <param name="childTable"></param>
        /// <returns></returns>
        private bool IsTable(string childTable)
        {
            IDataReader dr = null;
            string objectType = string.Empty;

            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "OBJECTNAME", DbType.String), childTable);
                string sqlSelect = @"select OBJECTTYPE as OBJECTTYPE
                from dbo.EFSOBJECT
                where OBJECTNAME = @OBJECTNAME" + Cst.CrLf;
                QueryParameters qry = new QueryParameters(cs, sqlSelect, parameters);
                dr = DataHelper.ExecuteReader(CurrentPage.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                if (dr.Read())
                    objectType = Convert.ToString(dr.GetValue(dr.GetOrdinal("OBJECTTYPE")));

                return (objectType == Convert.ToString("TABLE"));
            }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// return menu name for the table
        /// </summary>
        /// <returns></returns>
        private string GetIdMenu()
        {
            return currentPage.Request.QueryString["IdMenu"];
        }

        /// <summary>
        /// retreive a list contains all menu childrens
        /// </summary>
        /// <param name="pParentIdMenu"></param>
        /// <param name="pIdMenuList"></param>
        private List<string> RetreiveChildMenuList(string pParentIdMenu)
        {
            List<string> lstIdMenu = new List<string>();

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CurrentPage.CS, "IDMENU", DbType.String), pParentIdMenu);
            string sqlQuery = @"select mo.IDMENU as IDMENU
            from dbo.MENUOF mo
            inner join dbo.MENU m on (m.IDMENU = mo.IDMENU)
            where (mo.IDMENU_MENU = @IDMENU) and (m.URL like 'List%.aspx?Repository=%')
            order by mo.POSITION" + Cst.CrLf;
            DataTable dt = GetDataTable(sqlQuery, parameters);
            DataRow[] rows = dt.Select();

            foreach (DataRow row in rows)
            {
                lstIdMenu.Add(Convert.ToString(row["IDMENU"]));
            }
            return lstIdMenu;
        }

        /// <summary>
        /// returns URL from menu
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        private string GetUrl(string pIdMenu)
        {
            IDataReader dr = null;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "IDMENU", DbType.String), pIdMenu);
                string sqlSelect = @"select URL from dbo.MENU where IDMENU = @IDMENU" + Cst.CrLf;
                QueryParameters qry = new QueryParameters(cs, sqlSelect, parameters);
                dr = DataHelper.ExecuteReader(CurrentPage.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                if (dr.Read())
                    return Convert.ToString((dr["URL"]));
                else
                    return String.Empty;
            }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// returns menu from URL
        /// </summary>
        /// <param name="pUrl"></param>
        /// <returns></returns>
        private string GetIdMenuFromTableName(string pUrl)
        {

            string pUrl2 = "List%.aspx?Repository=" + pUrl + "%";

            IDataReader dr = null;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CurrentPage.CS, "URL", DbType.String), pUrl2);
                string sqlSelect = @"select IDMENU from dbo.MENU where URL like (@URL)" + Cst.CrLf;
                QueryParameters qry = new QueryParameters(cs, sqlSelect, parameters);
                dr = DataHelper.ExecuteReader(CurrentPage.CS, CommandType.Text, qry.query, qry.parameters.GetArrayDbParameter());
                if (dr.Read())
                    return Convert.ToString((dr["IDMENU"]));
                else
                    return String.Empty;
            }
            catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }
        }

        /// <summary>
        /// returns the name of the referential (table/view) using the url of the menu
        /// </summary>
        /// <param name="pUrl"></param>
        /// <returns></returns>
        private static string GetTableName(string pUrl)
        {
            string tableName = String.Empty;
            string url = pUrl;
            Regex substring = new Regex(@"=([A-Za-z0-9\-]+)&");
            Match match = substring.Match(url);

            if (match.Success)
            {
                tableName = match.Groups[1].Value;
            }
            return tableName;
        }

        /// <summary>
        /// NE PAS TOUCHER CETTE METHODE Display(), JE LA STANDARDISERAI PLUS TARD (PL)
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pData"></param>
        private void Display(StringBuilder pData)
        {
            bool isOnly_SQLFile = true;

            FileTools.WriteStringToFile(pData.ToString(), OutFilename);

            if (!isOnly_SQLFile)
            {
                #region
                string WindowID = FileTools.GetUniqueName("SQLExport", "SQLQuery");
                string write_File = SessionTools.TemporaryDirectory.MapPath("SQLExport") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "SQLExport" + @"/" + WindowID + ".xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("SQLExport, File: {0}", write_File);
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("SQLExport");
                xmlDoc.AppendChild(xmlRoot);
                //
                xmlRoot.AppendChild(xmlDoc.CreateCDataSection(pData.ToString()));
                //
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                XmlWriter xmlWritter = XmlTextWriter.Create(write_File, xmlWriterSettings);
                //
                xmlDoc.Save(xmlWritter);
                //   
                StringBuilder sbScript = new StringBuilder();
                sbScript.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='" + WindowID + "'>\n");
                sbScript.Append("window.open(\"" + open_File + "\",\"_blank\",'fullscreen=no,resizable=yes,top=0,left=0,height=350,width=700,location=no,menubar=yes,toolbar=yes,scrollbars=yes,status=yes');\n");
                sbScript.Append("</SCRIPT>\n");
                CurrentPage.ClientScript.RegisterClientScriptBlock(GetType(), WindowID, sbScript.ToString());
                #endregion
            }
        }

    }
}
