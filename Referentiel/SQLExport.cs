using System;
using System.Reflection;
using System.Configuration;
using System.Xml.Serialization;
using System.Data;
using System.Collections;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
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
// FI 20170908 [23409] Add
using EfsML.Enum;

namespace EFS.Referential
{
    public class Repository
    {
        #region Members
        private PageBase m_CurrentPage;
        private string m_OutFilename;
        private string m_CS;
        private DbSvrType m_dbSvrType;
        private bool m_isOracle;
        private string m_owner;
        #endregion
        #region Accessors
        public PageBase CurrentPage
        {
            get { return this.m_CurrentPage; }
            set { this.m_CurrentPage = value; }
        }
        public string OutFilename
        {
            get { return this.m_OutFilename; }
            set { this.m_OutFilename = value; }
        }
        #endregion
        #region constructor
        public Repository() { }
        #endregion

        /// <summary>
        /// SQLExport
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pReferential"></param>
        /// <param name="pDatatable"></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modfify (Repositoty instead of Referential)
        public Cst.ErrLevel SQLExport(PageBase pPage, ReferentialsReferential pReferential, DataTable pDatatable, string pFilename)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            m_owner = SQLCst.DBO.Trim();
            if (StrFunc.IsFilled(pPage.Request.QueryString["owner"]))
            {
                m_owner = pPage.Request.QueryString["owner"].ToString().Trim();
                if (!m_owner.EndsWith("."))
                    m_owner += ".";
            }
            CurrentPage = pPage;
            OutFilename = pFilename;
            m_CS = SessionTools.CS;
            m_dbSvrType = DataHelper.GetDbSvrType(m_CS);
            m_isOracle = (m_dbSvrType == DbSvrType.dbORA);

            StringBuilder sqlQuery = new StringBuilder();

            int maxStep = ((bool)SystemSettings.GetAppSettings("SQLExportAllowed", typeof(System.Boolean), false)) ? 2 : 1;
#if DEBUG
            maxStep = 2;
#endif

            for (int i = 1; i <= maxStep; i++)
            {
                #region Tip: Génération en mode DEBUG des 2 syntaxes: Oracle et SQLServer
                if (i == 2)//Tip
                {
                    m_isOracle = !m_isOracle;
                    if (m_isOracle)
                        m_dbSvrType = (m_isOracle ? DbSvrType.dbORA : DbSvrType.dbSQL);
                }
                if (maxStep == 2)
                {
                    if (m_isOracle)
                    {
                        sqlQuery.AppendLine(@"<center><span style=""color: red; font-size: 12pt; font-family:arial; font-weight: bold;"">");
                        sqlQuery.AppendLine(CommentOutput());
                        sqlQuery.AppendLine(CommentOutput("Oracle® - PL/SQL"));
                        sqlQuery.AppendLine(CommentOutput());
                        sqlQuery.AppendLine(@"</center>");
                    }
                    else
                    {
                        sqlQuery.AppendLine(@"<center><span style=""color: red; font-size: 12pt; font-family:arial; font-weight: bold;"">");
                        sqlQuery.AppendLine(" ");
                        sqlQuery.AppendLine(CommentOutput());
                        sqlQuery.AppendLine(CommentOutput("MS SQLserver® - Transact-SQL"));
                        sqlQuery.AppendLine(CommentOutput());
                        sqlQuery.AppendLine(@"</span></center>");
                    }
                }
                #endregion

                #region Initialisation
                StringBuilder sqlInsertQuery = new StringBuilder();
                StringBuilder sqlDeleteQuery = new StringBuilder();
                StringBuilder sqlRestoreQuery = new StringBuilder();

                if (m_isOracle)
                {
                    sqlQuery.AppendLine("declare");
                    sqlQuery.AppendLine("  DtEnabled DATE;");
                    sqlQuery.AppendLine("  DtIns DATE;");
                    sqlQuery.AppendLine("  IdaIns NUMBER(5);");
                    sqlQuery.AppendLine("  Ida NUMBER(5);");
                    sqlQuery.AppendLine("begin");
                    sqlQuery.AppendLine("  DtEnabled := TO_DATE(TO_CHAR(SYSDATE, 'YYYY') || '0101', 'YYYYMMDD');");
                    // FI 20200820 [25468] date systemes en UTC
                    sqlQuery.AppendLine("  DtIns := CAST(SYS_EXTRACT_UTC(SYSTIMESTAMP) as DATE);");
                    sqlQuery.AppendLine("  select IDA into IdaIns from " + this.m_owner + "ACTOR where IDENTIFIER='SYSTEM';");
                }
                else
                {
                    sqlQuery.AppendLine("declare @DtEnabled UT_DATETIME, @DtIns UT_DATETIME, @IdaIns UT_ID, @Ida UT_ID");
                    sqlQuery.AppendLine("begin");
                    sqlQuery.AppendLine("  begin tran");
                    sqlQuery.AppendLine("  set @DtEnabled=convert(datetime, convert(varchar(4), datepart(year, getdate())) + '0101');");
                    // FI 20200820 [25468] date systemes en UTC
                    sqlQuery.AppendLine("  set @DtIns=getutcdate();");
                    sqlQuery.AppendLine("  select @IdaIns=IDA from " + this.m_owner + "ACTOR where IDENTIFIER='SYSTEM';");
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
                        if (m_isOracle)
                            sqlInsertQuery.AppendLine("  select IDA into Ida from " + this.m_owner + "ACTOR where IDENTIFIER='SYSTEM';");
                        else
                            sqlInsertQuery.AppendLine("  select @Ida=IDA from " + this.m_owner + "ACTOR where IDENTIFIER='SYSTEM';");
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
                                string selectPkValue = "(select IDIOTASK from " + this.m_owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ")";

                                #region Spécifique au Consulting
                                if (!m_isOracle)
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
                                sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOTASK_PARAM where IDIOTASK in (select IDIOTASK from " + this.m_owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ");");
                                sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOTASKDET where IDIOTASK in (select IDIOTASK from " + this.m_owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ");");
                                sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOTASK where IDENTIFIER=" + DataHelper.SQLString(taskIdentifier) + ";");

                                #region IOTASKDET
                                DataTable dtIOTaskDet = DataTableIOTaskDet(taskIdentifier);
                                DataRow[] rowsIOTaskDet = dtIOTaskDet.Select();
                                
                                ReferentialsReferential opIoTaskDetReferential = new ReferentialsReferential();
                                ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, 
                                    Cst.ListType.Repository, "IOTASKDET", null, null, null, null,  out opIoTaskDetReferential);

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
                                        ReferentialsReferential opIoInputReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, 
                                            Cst.ListType.Repository, "IOINPUT", null, null, null, null, out opIoInputReferential);

                                        sqlInsertQuery.Append(BuildInsertQuery(opIoInputReferential, dtIOInput).ToString());

                                        #region IOINPUT_PARSING
                                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '-'));
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOINPUT_PARSING where IDIOINPUT=" + DataHelper.SQLString(idIOElement) + ";");
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOINPUT where IDIOINPUT=" + DataHelper.SQLString(idIOElement) + ";");

                                        DataTable dtIOInputParsing = DataTableIOInputParsing(idIOElement);
                                        DataRow[] rowsIOInputParsing = dtIOInputParsing.Select();
                                        foreach (DataRow rowIOInputParsing in rowsIOInputParsing)
                                        {
                                            if (!arParsing.Contains(rowIOInputParsing["IDIOPARSING"]))
                                            {
                                                arParsing.Add(rowIOInputParsing["IDIOPARSING"]);

                                                DataTable dtIOParsing = DataTableIOParsing(Convert.ToString(rowIOInputParsing["IDIOPARSING"]));
                                                // Write IOPARSING rows
                                                ReferentialsReferential opIoParsingReferential = new ReferentialsReferential();
                                                ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "IOPARSING", null, null, null, null, out opIoParsingReferential);

                                                // Delete IOPARSING
                                                if (Convert.ToString(rowIOInputParsing["IDIOPARSING"]).Length > 0)
                                                {
                                                    sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOPARSING where IDIOPARSING=" + DataHelper.SQLString(Convert.ToString(rowIOInputParsing["IDIOPARSING"])) + ";");

                                                    // Insert IOPARSING
                                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParsingReferential, dtIOParsing).ToString());

                                                    // IOPARSINGDET
                                                    DataTable dtIOParsingDet = DataTableIOParsingDet(Convert.ToString(rowIOInputParsing["IDIOPARSING"]));
                                                    ReferentialsReferential opIoParsingDetReferential = new ReferentialsReferential();
                                                    ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "IOPARSINGDET",  null,  null, null, null,  out opIoParsingDetReferential);
                                                    // Insert IOPARSINGDET
                                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParsingDetReferential, dtIOParsingDet).ToString());
                                                }
                                            }
                                        }
                                        ReferentialsReferential opIoInputParsingReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "IOINPUT_PARSING", null, null, null, null, out opIoInputParsingReferential);
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoInputParsingReferential, dtIOInputParsing).ToString());

                                        #endregion
                                    }
                                    #endregion
                                    #region OUTPUT
                                    //WARNING: TBD for ORACLE 
                                    if (elementType == "OUTPUT")
                                    {
                                        DataTable dtIOOutput = DataTableIOOutput(idIOElement);
                                        ReferentialsReferential opIoOutputReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "IOOUTPUT", 
                                            null, null, null, null, out opIoOutputReferential);

                                        // Delete
                                        sqlInsertQuery.AppendLine(CommentOutput(string.Empty, '-'));
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOOUTPUT_PARSING where IDIOOUTPUT=" + DataHelper.SQLString(idIOElement) + ";");
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOOUTPUT where IDIOOUTPUT=" + DataHelper.SQLString(idIOElement) + ";");

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
                                            ReferentialsReferential opIoParsingReferential = new ReferentialsReferential();
                                            ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "IOPARSING", null, null, null, null, out opIoParsingReferential);

                                            // Delete IOPARSING
                                            sqlDeleteQuery.AppendLine(" delete from " + this.m_owner + "IOPARSING where IDIOPARSING=" + DataHelper.SQLString(Convert.ToString(rowIoOutputParsing["IDIOPARSING"])) + ";");
                                            // Insert IOPARSING
                                            sqlInsertQuery.Append(BuildInsertQuery(opIoParsingReferential, dtIOParsing).ToString());

                                            // Write IOPARSINGDET
                                            DataTable dtIOParsingDet = DataTableIOParsingDet(Convert.ToString(rowIoOutputParsing["IDIOPARSING"]));
                                            ReferentialsReferential opIoParsingDetReferential = new ReferentialsReferential();
                                            ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "IOPARSINGDET", null, null, null, null, out opIoParsingDetReferential);
                                            sqlInsertQuery.Append(BuildInsertQuery(opIoParsingDetReferential, dtIOParsingDet).ToString());
                                        }
                                        #endregion

                                        // Write IOOUTPUT_PARSING
                                        ReferentialsReferential opIoOutputParsingReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, 
                                            Cst.ListType.Repository, "IOOUTPUT_PARSING", null, null, null, null, out opIoOutputParsingReferential);
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoOutputParsingReferential, dtIoOutputParsing).ToString());
                                    }
                                    #endregion
                                    #region SHELL
                                    if (elementType == "SHELL")
                                    {
                                        DataTable dtIOShell = DataTableIOShell(idIOElement);
                                        ReferentialsReferential opIoShellReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, 
                                            Cst.ListType.Repository, "IOSHELL", null, null, null, null,  out opIoShellReferential);

                                        // delete IOSHELL
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOSHELL where IDIOSHELL=" + DataHelper.SQLString(idIOElement) + ";");
                                        // write IDIOSHELL
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoShellReferential, dtIOShell).ToString());
                                    }
                                    #endregion
                                    #region COMPARE
                                    if (elementType == "COMPARE")
                                    {
                                        DataTable dtIOCompare = DataTableIOCompare(idIOElement);
                                        ReferentialsReferential opIoCompareReferential = new ReferentialsReferential();
                                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, 
                                            Cst.ListType.Repository, "IOCOMPARE", null, null, null, null, out opIoCompareReferential);

                                        // delete IOCOMPARE
                                        sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOCOMPARE where IDIOCOMPARE=" + DataHelper.SQLString(idIOElement) + ";");
                                        // write IOCOMPARE
                                        sqlInsertQuery.Append(BuildInsertQuery(opIoCompareReferential, dtIOCompare).ToString());
                                    }
                                    #endregion
                                } // end foreach (DataRow row in pDatatable.Rows) for each IOTASK
                                #endregion

                                #region IOTASK_PARAM
                                DataTable dtIOTaskParam = DataTableIOTaskParam(taskIdentifier);
                                DataRow[] rowsIOTaskParam = dtIOTaskParam.Select();
                                ReferentialsReferential opIoTaskParamReferential = new ReferentialsReferential();
                                ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "IOTASK_PARAM", null, null, null, null, out opIoTaskParamReferential);

                                foreach (DataRow rowIOTaskParam in rowsIOTaskParam)
                                {
                                    DataTable dtIOParam = DataTableIOParam(Convert.ToString(rowIOTaskParam["IDIOPARAM"]));
                                    DataRow[] rowsIOParam = dtIOParam.Select();
                                    ReferentialsReferential opIoParamReferential = new ReferentialsReferential();
                                    ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "IOPARAM", null, null, null, null, out opIoParamReferential);

                                    string idIOParam = Convert.ToString(rowIOTaskParam["IDIOPARAM"]);

                                    // delete IOPARAM
                                    sqlDeleteQuery.AppendLine("  delete from " + this.m_owner + "IOPARAM where IDIOPARAM=" + DataHelper.SQLString(idIOParam) + ";");

                                    // insert IOPARAM
                                    sqlInsertQuery.Append(BuildInsertQuery(opIoParamReferential, dtIOParam).ToString());
                                    // insert IOTASK_PARAM
                                    sqlInsertQuery.Append(BuildInsertQuery(opIoTaskParamReferential, dtIOTaskParam, selectPkValue).ToString());

                                    // IOPARAMDET
                                    DataTable dtIOParamDet = DataTableIOParamDet(Convert.ToString(rowIOTaskParam["IDIOPARAM"]));
                                    DataRow[] rowsIOParamDet = dtIOParamDet.Select();
                                    ReferentialsReferential opIoParamDetReferential = new ReferentialsReferential();
                                    ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,Cst.ListType.Repository, "IOPARAMDET", null, null, null, null, out opIoParamDetReferential);
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

                                string sql_where_IDM = " and IDM = (select IDM from " + this.m_owner + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " where SHORT_ACRONYM = " + DataHelper.SQLString(dc_MARKET_SHORT_ACRONYM) + ")";

                                sqlInsertQuery.AppendLine(PrintOutput("Derivative Contract Identifier: " + dc_IDENTIFIER + " - Market: " + dc_MARKET_SHORT_ACRONYM, '.'));

                                #region INSTRUMENT
                                /* Nécessaire à la récupération de IDI */
                                //PL 20130411 Debug
                                //DataTable dtInstrument = DataTableInstrument(dcIdentifier);
                                DataTable dtInstrument = DataTableInstrument(dc_IDDC);

                                string select_IDI = "( " + SQLCst.SELECT + "IDI ";
                                select_IDI += "from " + this.m_owner + Cst.OTCml_TBL.INSTRUMENT;
                                select_IDI += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Convert.ToString(dtInstrument.Rows[0]["IDENTIFIER"])) + " )";
                                #endregion INSTRUMENT

                                #region ASSET_EQUITY ou ASSET_INDEX
                                string asset_IDENTIFIER = string.Empty;
                                string asset_DtName = string.Empty;
                                string select_IDASSET = string.Empty;
                                DataTable dtAsset = null;
                                ReferentialsReferential opAssetReferential = new ReferentialsReferential();

                                if (Convert.ToString(drDerivativeContract["IDASSET_UNL"]) != string.Empty) //Si IDASSET_UNL est renseigné, on cherche l'IDENTIFIER du sous-jacent...
                                {
                                    switch (Convert.ToString(drDerivativeContract["ASSETCATEGORY"]))
                                    {
                                        case "Index":
                                            dtAsset = DataTableAsset_Index(Convert.ToInt32(drDerivativeContract["IDASSET_UNL"]));
                                            ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "ASSET_INDEX", null, null,null, null, out opAssetReferential);
                                            asset_IDENTIFIER = Convert.ToString(dtAsset.Rows[0]["IDENTIFIER"]);
                                            asset_DtName = "ASSET_INDEX";
                                            break;
                                        case "EquityAsset":
                                            dtAsset = DataTableAsset_Equity(Convert.ToInt32(drDerivativeContract["IDASSET_UNL"]));
                                            ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "ASSET_EQUITY", null, null, null, null, out opAssetReferential);
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
                                    select_IDASSET += "from " + this.m_owner + asset_DtName + " a";
                                    select_IDASSET += SQLCst.WHERE + " a.IDENTIFIER = " + DataHelper.SQLString(asset_IDENTIFIER) + " )";
                                }
                                #endregion ASSET_EQUITY ou ASSET_INDEX

                                #region MATURITYRULE
                                //PL 20130411 Debug
                                //DataTable dtMaturityRule = DataTableMaturityRule(dcIdentifier);
                                DataTable dtMaturityRule = DataTableMaturityRule(dc_IDDC);
                                ReferentialsReferential opMaturityRuleReferential = new ReferentialsReferential();
                                ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "MATURITYRULE", null, null, null, null, out opMaturityRuleReferential);

                                //PL 20131112 [TRIM 19164] TBD
                                string select_IDMATURITYRULE = "( " + SQLCst.SELECT + "IDMATURITYRULE ";
                                select_IDMATURITYRULE += "from " + this.m_owner + Cst.OTCml_TBL.MATURITYRULE;
                                select_IDMATURITYRULE += SQLCst.WHERE + "IDENTIFIER = " + DataHelper.SQLString(Convert.ToString(dtMaturityRule.Rows[0]["IDENTIFIER"])) + " )";
                                sqlInsertQuery.AppendLine(BuildInsertQuery(opMaturityRuleReferential, dtMaturityRule, true).ToString());
                                #endregion MATURITYRULE

                                //PL 20130411
                                //sqlInsertQuery.AppendLine(BuildInsertQuery(pReferential, pDatatable, selectInstrumentID, selectMRPkValue).ToString());
                                DataTable dt_temp = pDatatable.Clone();
                                dt_temp.Rows.Add(drDerivativeContract.ItemArray);
                                sqlInsertQuery.AppendLine(BuildInsertQuery(pReferential, dt_temp, select_IDI, select_IDMATURITYRULE, select_IDASSET).ToString());
                                #endregion DERIVATIVECONTRACT

                                #region MATURITY
                                DataTable dtMaturity = DataTableMaturity(Convert.ToInt32(dtMaturityRule.Rows[0]["IDMATURITYRULE"]));
                                ReferentialsReferential opMaturityReferential = new ReferentialsReferential();
                                ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, Cst.ListType.Repository, "MATURITY", null, null, null, null, out opMaturityReferential);

                                sqlInsertQuery.AppendLine(BuildInsertQuery(opMaturityReferential, dtMaturity, true, select_IDMATURITYRULE).ToString());

                                foreach (DataRow drMaturity in dtMaturity.Rows)
                                {
                                    /* Pour chaque Maturity */
                                    #region DERIVATIVEATTRIB
                                    DataTable dtDerivativeAttrib = DataTableDerivativeAttrib(Convert.ToInt32(drDerivativeContract["IDDC"]), Convert.ToInt32(drMaturity["IDMATURITY"]));
                                    ReferentialsReferential opDerivativeAttribReferential = new ReferentialsReferential();
                                    ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "DERIVATIVEATTRIB", null, null, null, null, out opDerivativeAttribReferential);
                                    string select_IDMATURITY = "( " + SQLCst.SELECT + "IDMATURITY ";
                                    select_IDMATURITY += "from " + this.m_owner + Cst.OTCml_TBL.MATURITY;
                                    select_IDMATURITY += SQLCst.WHERE + "( IDMATURITYRULE = " + select_IDMATURITYRULE + " )";
                                    select_IDMATURITY += SQLCst.AND + "( MATURITYMONTHYEAR = " + DataHelper.SQLString(Convert.ToString(drMaturity["MATURITYMONTHYEAR"])) + " ) )";

                                    sqlInsertQuery.AppendLine(BuildInsertQuery(opDerivativeAttribReferential, dtDerivativeAttrib, select_IDMATURITY).ToString());
                                    #endregion DERIVATIVEATTRIB

                                    #region ASSET_ETD
                                    DataTable dtAsset_Etd = DataTableAsset_Etd(dc_IDENTIFIER, Convert.ToInt32(drMaturity["IDMATURITY"]));
                                    ReferentialsReferential opAsset_EtdReferential = new ReferentialsReferential();
                                    ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, "ASSET_ETD", null, null,  null, null, out opAsset_EtdReferential);

                                    string select_IDDC = "( " + SQLCst.SELECT + "IDDC ";
                                    select_IDDC += "from " + this.m_owner + Cst.OTCml_TBL.DERIVATIVECONTRACT.ToString();
                                    select_IDDC += " where IDENTIFIER = " + DataHelper.SQLString(dc_IDENTIFIER);
                                    select_IDDC += sql_where_IDM; 
                                    select_IDDC += " )";
                                    
                                    string select_IDDERIVATIVEATTRIB = "( " + SQLCst.SELECT + "IDDERIVATIVEATTRIB ";
                                    select_IDDERIVATIVEATTRIB += "from " + this.m_owner + Cst.OTCml_TBL.DERIVATIVEATTRIB;
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
                                //ReferentialsReferential opContractCascading = new ReferentialsReferential();
                                //ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS, GetIdMenuFromTableName("CONTRACTCASCADING"), Cst.ListType.Referential, "CONTRACTCASCADING", null, null, out opContractCascading);
                                /* Insert CONTRACTCASCADING */
                                //sqlInsertQuery.AppendLine(BuildInsertQuery(opContractCascading, dtContractCascading).ToString());
                                #endregion CONTRACTCASCADING

                                #region Deletes
                                string sql_delete;
                                /* Delete QUOTE_ETD_H */
                                sql_delete = " delete from " + this.m_owner + Cst.OTCml_TBL.QUOTE_ETD_H + " where IDASSET in (";
                                sql_delete += "select IDASSET from " + this.m_owner + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " where CONTRACTIDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM;
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete ASSET_ETD */
                                sql_delete = " delete from " + this.m_owner + Cst.OTCml_TBL.ASSET_ETD + " where IDASSET in (";
                                sql_delete += "select IDASSET from " + this.m_owner + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " where CONTRACTIDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM; 
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete DERIVATIVEATTRIB */
                                sql_delete = " delete from " + this.m_owner + Cst.OTCml_TBL.DERIVATIVEATTRIB + " where IDDC in (";
                                sql_delete += "select IDDC from " + this.m_owner + Cst.OTCml_TBL.DERIVATIVECONTRACT + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
                                sql_delete += sql_where_IDM; 
                                sql_delete += ");";
                                sqlDeleteQuery.AppendLine(sql_delete);
                                /* Delete CONTRACTCASCADING */
                                //sqlDeleteQuery.AppendLine(" delete from " + this.m_owner + "CONTRACTCASCADING where IDDC in (select IDDC from " + this.m_owner + "DERIVATIVECONTRACT where IDENTIFIER=" + DataHelper.SQLString(dcIdentifier) + ");");                                        
                                /* Delete DERIVATIVECONTRACT*/
                                sql_delete = " delete from " + this.m_owner + Cst.OTCml_TBL.DERIVATIVECONTRACT + " where IDENTIFIER=" + DataHelper.SQLString(dc_IDENTIFIER);
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
                sqlQuery.AppendLine("/");
            }

            string outputQuery = sqlQuery.ToString().Replace(Cst.CrLf, Cst.HTMLBreakLine + Cst.CrLf);
            sqlQuery.Length = 0;
            sqlQuery.AppendLine(@"<!DOCTYPE html>");
            sqlQuery.AppendLine(@"<html>");
            sqlQuery.AppendLine(@"  <head>");
            sqlQuery.AppendLine(@"  <meta charset=""utf-16"" />");
            sqlQuery.AppendLine(@"  <title>" + pPage.Title + " - SQL Export" + "</title>");
            sqlQuery.AppendLine(@"  </head>");

            sqlQuery.AppendLine(@"  <body>");
            sqlQuery.AppendLine(@"    <div align=""center"" style=""color: blue; background-color: gray; font-weight:bold; font-family: arial; font-size: 12pt; font-weight: bold; border: 2px black solid"">");
            sqlQuery.AppendLine(CommentOutput(string.Empty, '=') + Cst.HTMLBreakLine);
            //sqlQuery.AppendLine(CommentOutput(Software.CopyrightFull, ' ') + Cst.HTMLBreakLine);
            //sqlQuery.AppendLine(CommentOutput(string.Empty, '-') + Cst.HTMLBreakLine);
            sqlQuery.AppendLine(CommentOutput(pPage.Title, ' ') + Cst.HTMLBreakLine);
            sqlQuery.AppendLine(CommentOutput(string.Empty, '-') + Cst.HTMLBreakLine);
            sqlQuery.AppendLine(CommentOutput(@"Source: " + new CSManager(SessionTools.CS).GetCSAnonymizePwd(), ' ') + Cst.HTMLBreakLine);
            sqlQuery.AppendLine(CommentOutput(string.Empty, '=') + Cst.HTMLBreakLine);
            sqlQuery.AppendLine(@"    </div>");
            sqlQuery.AppendLine(Cst.HTMLBreakLine);
            sqlQuery.AppendLine(@"    <div style=""color: black; background-color: silver; font-family: courier new; font-size: 10pt; border: 1px black solid"">");
            sqlQuery.Append(outputQuery);
            sqlQuery.AppendLine(@"    </div>");
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
            string ret;
            if (pMsg == null)
                ret = "/* " + string.Empty.PadRight(100, pChar) + " */";
            else
                ret = "/* " + pMsg.TrimEnd().PadRight(100, pChar) + " */";

            return ret;
        }
        private string PrintOutput(string pMsg, char pChar)
        {
            string ret;
            if (pChar == '.')
            {
                if (m_isOracle)
                    ret = "dbms_output.put_line('" + pMsg.PadRight(81, pChar) + "');";
                else
                    ret = "print '" + pMsg.TrimEnd().PadRight(96, pChar) + "';";
            }
            else
            {
                if (m_isOracle)
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

        #region DataTables IO
        private DataTable DataTableIOInput(string pIdIOInput)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOINPUT", DbType.String), pIdIOInput);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOINPUT.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOINPUT=@IDIOINPUT " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOOutput(string pIdIOOutput)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOOUTPUT", DbType.String), pIdIOOutput);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOOUTPUT.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOOUTPUT=@IDIOOUTPUT " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOShell(string pIdIOShell)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOSHELL", DbType.String), pIdIOShell);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOSHELL.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOSHELL=@IDIOSHELL " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOTaskDet(string pTaskIdentifier)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IOTASKIDENTIFIER", DbType.String), pTaskIdentifier);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "iotd.*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASKDET.ToString() + " iotd " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASK + " iot " + SQLCst.ON + " iot.IDIOTASK = iotd.IDIOTASK " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " iot.IDENTIFIER=@IOTASKIDENTIFIER " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "iotd.SEQUENCENO asc" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOInputParsing(string pIdioInput)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOINPUT", DbType.String), pIdioInput);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOINPUT_PARSING.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOINPUT=@IDIOINPUT " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "SEQUENCENO asc" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            return ds.Tables[0];
        }

        private DataTable DataTableIOOutputParsing(string pIdioInput)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOOUTPUT", DbType.String), pIdioInput);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOOUTPUT_PARSING.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOOUTPUT=@IDIOOUTPUT " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "SEQUENCENO asc" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            return ds.Tables[0];
        }


        private DataTable DataTableIOParsing(string pIdioParsing)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOPARSING", DbType.String), pIdioParsing);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOPARSING.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOPARSING=@IDIOPARSING " + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            return ds.Tables[0];
        }

        private DataTable DataTableIOParsingDet(string pIdioParsing)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOPARSING", DbType.String), pIdioParsing);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOPARSINGDET.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOPARSING=@IDIOPARSING " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "SEQUENCENO asc" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            return ds.Tables[0];
        }

        private DataTable DataTableIOTaskParam(string pTaskIdentifier)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IOTASKIDENTIFIER", DbType.String), pTaskIdentifier);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "iotp.*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK_PARAM.ToString() + " iotp " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASK + " iot " + SQLCst.ON + " iot.IDIOTASK = iotp.IDIOTASK " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " iot.IDENTIFIER=@IOTASKIDENTIFIER " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "iotp.SEQUENCENO asc" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOParam(string pIdIOParam)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOPARAM", DbType.String), pIdIOParam);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOPARAM.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOPARAM=@IDIOPARAM " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOParamDet(string pIdIOParam)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOPARAM", DbType.String), pIdIOParam);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOPARAMDET.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOPARAM=@IDIOPARAM " + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "SEQUENCENO asc" + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        private DataTable DataTableIOCompare(string pIdIOCompare)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDIOCOMPARE", DbType.String), pIdIOCompare);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOCOMPARE.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDIOCOMPARE=@IDIOCOMPARE " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }
        #endregion End DataTables IO

        #region DataTables DC
        /* ASSET_ETD */
        private DataTable DataTableAsset_Etd(string pDCIdentifier, int pIDMaturity)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "CONTRACTIDENTIFIER", DbType.String), pDCIdentifier);
            parameters.Add(new DataParameter(SessionTools.CS, "IDMATURITY", DbType.Int32), pIDMaturity);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "a.* " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_ETD.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_ASSET_ETD_EXPANDED + " v " + SQLCst.ON + " v.IDASSET = a.IDASSET " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB + " da " + SQLCst.ON + " da.IDDERIVATIVEATTRIB = a.IDDERIVATIVEATTRIB " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " v.CONTRACTIDENTIFIER=@CONTRACTIDENTIFIER " + Cst.CrLf;
            sqlSelect += SQLCst.AND + " da.IDMATURITY=@IDMATURITY " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        /* INSTRUMENT */
        //PL 20130411 Debug
        //private DataTable DataTableInstrument(string pDCIdentifier)
        private DataTable DataTableInstrument(int pIDDC)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDDC", DbType.Int32), pIDDC);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "i.IDENTIFIER" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i" + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.DERIVATIVECONTRACT + " dc" + SQLCst.ON + "dc.IDI=i.IDI and dc.IDDC=@IDDC";
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        /* ASSET_EQUITY */
        private DataTable DataTableAsset_Equity(int pIDASSET_UNL)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDASSET", DbType.Int32), pIDASSET_UNL);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "a.*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_EQUITY.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDASSET=@IDASSET";
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }


        /* ASSET_INDEX */
        private DataTable DataTableAsset_Index(int pIDASSET_UNL)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDASSET", DbType.Int32), pIDASSET_UNL);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "a.*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ASSET_INDEX.ToString() + " a" + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDASSET=@IDASSET";
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        /* DERIVATIVEATTRIB */
        private DataTable DataTableDerivativeAttrib(int pIDDC, int pIDMaturity)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDDC", DbType.Int32), pIDDC);
            parameters.Add(new DataParameter(SessionTools.CS, "IDMATURITY", DbType.Int32), pIDMaturity);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.DERIVATIVEATTRIB.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDDC=@IDDC";
            sqlSelect += SQLCst.AND + "IDMATURITY=@IDMATURITY";
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        /* CONTRACTCASCADING */
        // Pas encore utilisé dans Spheres
        //private DataTable DataTableContractCascading(int pIdDC)
        //{
        //    try
        //    {
        //        DataParameters parameters = new DataParameters();
        //        parameters.Add(new DataParameter(SessionTools.CS, "IDDC", DbType.Int32), pIdDC);
        //        StrBuilder sqlSelect = new StrBuilder();
        //        sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
        //        sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CONTRACTCASCADING.ToString() + Cst.CrLf;
        //        sqlSelect += SQLCst.WHERE + " IDDC=@IDDC " + Cst.CrLf;
        //        DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
        //        DataTable dt = ds.Tables[0];
        //        return dt;
        //    }
        //    catch (Exception ex) { throw new SpheresException(MethodInfo.GetCurrentMethod().Name, ex); }
        //}

        /* MATURITY */
        private DataTable DataTableMaturity(int pIdMaturityRule)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDMATURITYRULE", DbType.Int32), pIdMaturityRule);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITY.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDMATURITYRULE=@IDMATURITYRULE " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

        }

        /* MATURITYRULE */
        //PL 20130411 Debug
        //private DataTable DataTableMaturityRule(string pIdentifier)
        private DataTable DataTableMaturityRule(int pIdDC)
        {
            DataParameters parameters = new DataParameters();
            //parameters.Add(new DataParameter(SessionTools.CS, "IDENTIFIER", DbType.String), pIdentifier);
            parameters.Add(new DataParameter(SessionTools.CS, "IDDC", DbType.Int32), pIdDC);
            //PL 20131112 [TRIM 19164] TBD
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "*" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MATURITYRULE.ToString() + Cst.CrLf;
            //sqlSelect += SQLCst.WHERE + "IDMATURITYRULE = (select dc.IDMATURITYRULE from DERIVATIVECONTRACT dc where dc.IDENTIFIER = @IDENTIFIER) " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDMATURITYRULE=(select dc.IDMATURITYRULE from dbo.DERIVATIVECONTRACT dc where dc.IDDC=@IDDC)";
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            return dt;

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
        private void ExploreChildTables(string pIdMenuParentTable, ReferentialsReferential pParentReferential, DataRow pParentRow, string pPrefix, string pSuffix, ref StringBuilder pSQLScript)
        {
            List<String> idMenuChildTablesList = new List<string>();
            // Retrieve child tables through Spheres menus
            RetreiveChildMenuList(pIdMenuParentTable, idMenuChildTablesList);

            if (idMenuChildTablesList.Count != 0)
            {
                // For each child table
                foreach (string idMenuChildTable in idMenuChildTablesList)
                {
                    string childTableUrl = GetUrl(idMenuChildTable);
                    string childTableName = GetTableName(childTableUrl);
                    string parentTableName = pParentReferential.TableName;

                    if (IsTable(childTableName) == true)
                    {
                        ReferentialsReferential opChildReferential = new ReferentialsReferential();
                        // FI 20160804 [Migration TFS] Modfify (Repositoty instead of Referential)
                        ReferentialTools.DeserializeXML_ForModeRW_25(SessionTools.CS,  Cst.ListType.Repository, childTableName, null, null, null, null, out opChildReferential);
                        // Find the column name which is the foreign key (column IDACCKEY of the table ACCKEYVALUE)
                        string fkColumn = opChildReferential.Column[opChildReferential.IndexForeignKeyField].ColumnName;
                        // Find the value of the fk column (here from the parent table)
                        string fkValue = pParentRow.ItemArray[pParentReferential.IndexColSQL_DataKeyField].ToString();
                        string parentIdentifierValue = pParentRow.ItemArray[pParentReferential.IndexColSQL_KeyField].ToString();
                        //
                        DataSet childDs = SQLReferentialData.RetrieveDataFromSQLTable(SessionTools.CS, opChildReferential, fkColumn, fkValue, false, false);
                        DataTable childDt = childDs.Tables[0];
                        //
                        foreach (DataRow childRow in childDt.Rows)
                        {
                            string sqlInsertQueryChild = BuildComplexInsertQuery(pPrefix, pSuffix, opChildReferential, childRow, fkColumn, parentTableName);
                            pSQLScript.Append(sqlInsertQueryChild);
                            ExploreChildTables(idMenuChildTable, opChildReferential, childRow, pPrefix, pSuffix, ref pSQLScript);
                        }
                    }
                }
            }
        }

        private bool IsColumnOk(ReferentialsReferential pReferential, ReferentialsReferentialColumn pRrc)
        {

            // GS 20110531; external fields are discarded in V1 (!rrc.IsAdditionalData)
            bool isOk = (!pRrc.IsIdentity.Value || pRrc.IsIdentity.sourceSpecified)
                && pRrc.ColumnName != "ROWVERSION"
                && !pRrc.IsAdditionalData
                && !pRrc.Ressource.StartsWith("NotePad")
                && !pRrc.Ressource.StartsWith("AttachedDoc")
                && !pRrc.IsVirtualColumn;
            if (isOk && pReferential.TableName.StartsWith("IO"))
                isOk = (pRrc.ColumnName != "IDAUPD") && (pRrc.ColumnName != "DTUPD");
            if (isOk && pRrc.AliasTableNameSpecified && pRrc.AliasTableName.EndsWith("_S"))
                isOk = false;

            return isOk;
        }

        /// <summary>
        /// BuildColumnList: returns list of columns from a table
        /// </summary>
        /// <param name="pReferential"></param>
        private string BuildColumnList(ReferentialsReferential pReferential)
        {
            string columnList = string.Empty;

            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                ReferentialsReferentialColumn rrc = pReferential.Column[i];
                if (IsColumnOk(pReferential, rrc))
                    columnList += ", " + rrc.ColumnName;
            }
            columnList = SQLCst.X_INSERT + this.m_owner + pReferential.TableName + "(" + columnList.TrimStart(',') + ")" + Cst.CrLf;

            return columnList;

        }

        /// <summary>
        /// returns query insert from table
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pReferential"></param>
        /// <param name="pDatatable"></param>
        private StringBuilder BuildInsertQuery(ReferentialsReferential pReferential, DataTable pDatatable)
        {
            return BuildInsertQuery(pReferential, pDatatable, false, null);
        }
        private StringBuilder BuildInsertQuery(ReferentialsReferential pReferential, DataTable pDatatable, params string[] pPkValue)
        {
            return BuildInsertQuery(pReferential, pDatatable, false, pPkValue);
        }
        // FI 20170908 [23409] Modify 
        // EG 20210823 [XXXXX] [XXXXX] Plantage sur la requête d'insertion pour le référentiel ASSET_ETD (Colonne IDDERIVATIVEATTRIB) et
        // Dysfonctionnement sur référentiel BOOK (Colonne IDA) et tous les enfants du référentiel ACTOR
        private StringBuilder BuildInsertQuery(ReferentialsReferential pReferential, DataTable pDatatable, bool pIsWhereNotExists, params string[] pPkValue)
        {
            string parameterPrefix = string.Empty;
            if (!m_isOracle)
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
                    ReferentialsReferentialColumn rrc = pReferential.Column[i];
                    if (IsColumnOk(pReferential, rrc))
                    {
                        if (isFirstColum)
                            isFirstColum = false;
                        else
                            sqlInsertQuery.Append(", ");

                        if ((row[rrc.ColumnName] is DBNull) && (rrc.ColumnName != "IDA") && (rrc.ColumnName != "IDAINS") && (rrc.ColumnName != "DTINS") && (rrc.ColumnName != "DTENABLED") && (rrc.ColumnName != "IDMATURITYRULE"))
                        {
                            sqlInsertQuery.Append(SQLCst.NULL);
                        }
                        else if (TypeData.IsTypeInt(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                case "IDIOTASK":
                                    sqlInsertQuery.Append(pPkValue[0]);
                                    break;
                                case "IDMATURITY":
                                    // EG 20220926 [XXXXX] [WI436] Plantage sur la requête d'insertion pour le référentiel DERIVATIVEATTRIB
                                    if (ArrFunc.IsFilled(pPkValue))
                                        sqlInsertQuery.Append(Array.Find(pPkValue, s => s.Contains("IDMATURITY")));
                                    else
                                        sqlInsertQuery.Append(row[rrc.ColumnName].ToString());
                                    break;
                                case "IDMATURITYRULE":
                                    sqlInsertQuery.Append(Array.Find(pPkValue, s => s.Contains("IDMATURITYRULE")));
                                    break;
                                case "IDI":
                                    sqlInsertQuery.Append(pPkValue[0]);
                                    break;
                                case "IDDERIVATIVEATTRIB":
                                    // EG 20210823 [XXXXX] [XXXXX] Plantage sur la requête d'insertion pour le référentiel ASSET_ETD
                                    //sqlInsertQuery.Append(pPkValue[0]);
                                    if (ArrFunc.IsFilled(pPkValue))
                                        sqlInsertQuery.Append(Array.Find(pPkValue, s => s.Contains("IDDERIVATIVEATTRIB")));
                                    else
                                        sqlInsertQuery.Append(row[rrc.ColumnName].ToString());
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
                                    if (row[rrc.ColumnName] != Convert.DBNull)
                                    {
                                        string tableName = null;
                                        string columnName = rrc.ColumnName;
                                        switch (rrc.ColumnName)
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
                                                // FI 20170908 [23409] Utilisation de l'enum TypeContractEnum et gestion de COMMODITYCONTRACT
                                                tableName = "DERIVATIVECONTRACT";
                                                columnName = "IDDC";
                                                if ((i > 0) && (pReferential.Column[i - 1].ColumnName == "TYPECONTRACT"))
                                                {
                                                    if (row["TYPECONTRACT"].ToString() == TypeContractEnum.Market.ToString())
                                                    {
                                                        tableName = "MARKET";
                                                        columnName = "IDM";
                                                    }
                                                    else if (row["TYPECONTRACT"].ToString() == TypeContractEnum.GrpMarket.ToString())
                                                    {
                                                        tableName = "GMARKET";
                                                        columnName = "IDGMARKET";
                                                    }
                                                    else if (row["TYPECONTRACT"].ToString() == TypeContractEnum.GrpContract.ToString())
                                                    {
                                                        tableName = "GCONTRACT";
                                                        columnName = "IDGCONTRACT";
                                                    }
                                                    else if (row["TYPECONTRACT"].ToString() == TypeContractEnum.DerivativeContract.ToString())
                                                    {
                                                        tableName = "DERIVATIVECONTRACT";
                                                        columnName = "IDDC";
                                                    }
                                                    else if (row["TYPECONTRACT"].ToString() == TypeContractEnum.CommodityContract.ToString())
                                                    {
                                                        tableName = "COMMODITYCONTRACT";
                                                        columnName = "IDCC";
                                                    }
                                                }
                                                break;
                                        }
                                        string top1 = string.Empty;
                                        string addFilter = string.Empty;
                                        string dataIdentifier = null;
                                        string data2Identifier = null;
                                        string sqlSelectPkValue = "select IDENTIFIER from " + this.m_owner + tableName + " where " + columnName + "=@DataKey";
                                        DataParameters parameters = new DataParameters();
                                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                                        parameters.Add(new DataParameter(m_CS, "DataKey", DbType.Int64), row[rrc.ColumnName]);
                                        IDataReader dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                        if (dr.Read())
                                            dataIdentifier = dr.GetString(0);
                                        dr.Close();

                                        if (tableName == "DERIVATIVECONTRACT")
                                        {
                                            //Cas particulier des DC
                                            sqlSelectPkValue = "select v.SHORT_ACRONYM from " + this.m_owner + tableName + " t" + Cst.CrLf;
                                            sqlSelectPkValue += "inner join dbo.VW_MARKET_IDENTIFIER v on v.IDM=t.IDM" + Cst.CrLf;
                                            sqlSelectPkValue += "where t." + columnName + "=@DataKey";
                                            dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                            if (dr.Read())
                                            {
                                                data2Identifier = dr.GetString(0);
                                                if (!String.IsNullOrEmpty(data2Identifier))
                                                    addFilter = " and IDM=(select IDM from " + this.m_owner + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " where SHORT_ACRONYM=" + DataHelper.SQLString(data2Identifier) + ")";
                                            }
                                            dr.Close();
                                        }
                                        else if (tableName == "FEESCHEDULE")
                                        {
                                            top1 = "top 1 "; //TBD on Oracle

                                            sqlSelectPkValue = "select IDENTIFIER from " + this.m_owner + Cst.OTCml_TBL.FEE.ToString() + " t" + Cst.CrLf;
                                            sqlSelectPkValue += "where IDFEE=@DataKey";
                                            parameters["DataKey"].Value = row["IDFEE"];
                                            dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                            if (dr.Read())
                                            {
                                                data2Identifier = dr.GetString(0);
                                                if (!String.IsNullOrEmpty(data2Identifier))
                                                    addFilter = " and IDFEE=(select IDFEE from " + this.m_owner + Cst.OTCml_TBL.FEE.ToString() + " where IDENTIFIER=" + DataHelper.SQLString(data2Identifier) + ")";
                                            }
                                            dr.Close();
                                        }

                                        selectPkValue = "(select " + top1 + columnName + " from " + this.m_owner + "" + tableName + " where IDENTIFIER=" + DataHelper.SQLString(dataIdentifier) + addFilter + ")";
                                    }
                                    sqlInsertQuery.Append(selectPkValue);
                                    break;
                                case "IDA":
                                    // EG 20210823 [XXXXX] [XXXXX] Dysfonctionnement sur référentiel BOOK (Colonne IDA) et tous les enfants du référentiel ACTOR
                                    if (pDatatable.TableName == "MARKET")
                                    {
                                        //BD 20130514 Dans le cas de l'export de MARKET, on select IDA sur la table ACTOR
                                        string tableName = null;
                                        string columnName = rrc.ColumnName;
                                        tableName = "ACTOR";
                                        columnName = "IDA";
                                        string instrumentIdentifier = null;
                                        string sqlSelectPkValue = "select IDENTIFIER from dbo." + tableName + " where " + columnName + "=@DataKey";
                                        DataParameters parameters = new DataParameters();
                                        parameters.Add(new DataParameter(m_CS, "DataKey", DbType.Int32), row[rrc.ColumnName]);
                                        IDataReader dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                        if (dr.Read())
                                            instrumentIdentifier = dr.GetString(0);
                                        dr.Close();
                                        selectPkValue = "(select " + columnName + " from " + this.m_owner + tableName + " where IDENTIFIER=" + DataHelper.SQLString(instrumentIdentifier) + ")";
                                        sqlInsertQuery.Append(selectPkValue);
                                    }
                                    else if (String.IsNullOrEmpty(row[rrc.ColumnName].ToString()))
                                        sqlInsertQuery.Append(parameterPrefix + "Ida");
                                    else
                                        sqlInsertQuery.Append(row[rrc.ColumnName].ToString());
                                    break;
                                case "IDAINS":
                                    sqlInsertQuery.Append(parameterPrefix + "IdaIns");
                                    break;
                                case "IDAUPD":
                                    sqlInsertQuery.Append(SQLCst.NULL);
                                    break;
                                default:
                                    sqlInsertQuery.Append(row[rrc.ColumnName].ToString());
                                    break;
                            }
                        }
                        else if (TypeData.IsTypeDec(rrc.DataType.value))
                        {
                            decimal decValue = DecFunc.DecValue(row[rrc.ColumnName].ToString());
                            string strVaue = StrFunc.FmtDecimalToInvariantCulture(decValue);
                            sqlInsertQuery.Append(strVaue);
                        }
                        else if (TypeData.IsTypeDate(rrc.DataType.value))
                        {
                            switch (rrc.ColumnName)
                            {
                                case "DTINS":
                                    sqlInsertQuery.Append(parameterPrefix + "DtIns");
                                    break;
                                case "DTENABLED":
                                    sqlInsertQuery.Append(parameterPrefix + "DtEnabled");
                                    break;
                                case "DTDISABLED":
                                    // GS 20130117: enable disable date extraction
                                    // sqlInsertQuery.Append(SQLCst.NULL);
                                    sqlInsertQuery.Append("DtDisabled");
                                    break;
                                case "DTUPD":
                                    sqlInsertQuery.Append(SQLCst.NULL);
                                    break;
                                default:
                                    sqlInsertQuery.Append(DataHelper.SQLToDate(m_dbSvrType, Convert.ToDateTime(row[rrc.ColumnName])));
                                    break;
                            }
                        }
                        else if (TypeData.IsTypeBool(rrc.DataType.value))
                        {
                            sqlInsertQuery.Append(DataHelper.SQLBoolean(Convert.ToBoolean(row[rrc.ColumnName])));
                        }
                        else
                        {
                            switch (rrc.ColumnName)
                            {
                                case "ACTION": //FEESCHEDULE & FEEMATRIX
                                    string dataIdAction = null;
                                    string sqlSelectAction = "select MNU_PERM_DESC from " + this.m_owner + "VW_ALL_VW_PERMIS_MENU where IDPERMISSION=@DataKey";
                                    DataParameters parameters2 = new DataParameters();
                                    parameters2.Add(new DataParameter(m_CS, "DataKey", DbType.String, 128), row[rrc.ColumnName]);
                                    IDataReader drAction = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectAction, parameters2.GetArrayDbParameter());
                                    if (drAction.Read())
                                        dataIdAction = drAction.GetString(0);
                                    drAction.Close();

                                    string selectValue = "(select IDPERMISSION from " + this.m_owner + "VW_ALL_VW_PERMIS_MENU where MNU_PERM_DESC=" + DataHelper.SQLString(dataIdAction) + ")";
                                    sqlInsertQuery.Append(selectValue);
                                    break;
                                default:
                                    sqlInsertQuery.Append(DataHelper.SQLString(row[rrc.ColumnName].ToString()));
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
                            sqlInsertQuery.Append(Cst.CrLf + "from " + this.m_owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + this.m_owner + "MATURITYRULE" + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(row["IDENTIFIER"].ToString()));
                            break;
                        case "MATURITY":
                            sqlInsertQuery.Append(Cst.CrLf + "from " + this.m_owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + this.m_owner + "MATURITY" + SQLCst.WHERE + "IDMATURITYRULE=" + Array.Find(pPkValue, s => s.Contains("IDMATURITYRULE")) + SQLCst.AND + "MATURITYMONTHYEAR=" + DataHelper.SQLString(row["MATURITYMONTHYEAR"].ToString()));
                            break;
                        case "ASSET_EQUITY":
                        case "ASSET_INDEX":
                            sqlInsertQuery.Append(Cst.CrLf + "from " + this.m_owner + "DUAL" + Cst.CrLf + SQLCst.WHERE + SQLCst.NOT_EXISTS + "(" + SQLCst.SELECT + "1 from " + this.m_owner + pReferential.TableName + SQLCst.WHERE + "IDENTIFIER=" + DataHelper.SQLString(row["IDENTIFIER"].ToString()));
                            break;
                    }
                }

                sqlInsertQuery.Append(");" + Cst.CrLf);

            }
            return sqlInsertQuery;
        }


        /// <summary>
        /// Builds ACCMODEL table specific insert query
        /// </summary>
        /// <param name="pReferential"></param>
        /// <param name="pNewId"></param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private string BuildAccModelInsertQuery(ReferentialsReferential pReferential, string pNewId, DataRow pRow)
        {

            string parameterPrefix = string.Empty;
            if (!m_isOracle)
                parameterPrefix = "@";

            string sqlInsertQuery = String.Empty;
            string columnList = BuildColumnList(pReferential);
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
                ReferentialsReferentialColumn rrc = pReferential.Column[i];
                // GS 20110531; external fields are discarded in V1 (!rrc.IsAdditionalData)
                bool isOk = ((!rrc.IsIdentity.Value) || (rrc.IsIdentity.sourceSpecified))
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
                                sqlInsertQuery += DataHelper.SQLToDate(SessionTools.CS, Convert.ToDateTime(row[rrc.ColumnName]));
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
        private string BuildComplexInsertQuery(string pPrefix, string pSuffix, ReferentialsReferential pReferential, DataRow pRow, string pFkParentColumn, string pParentTable)
        {
            string parameterPrefix = string.Empty;
            if (!m_isOracle)
                parameterPrefix = "@";

            string sqlInsertQuery = string.Empty;
            string columnList = BuildColumnList(pReferential);
            string prefix = pPrefix;
            string suffix = pSuffix;

            DataRow row = pRow;
            sqlInsertQuery += columnList;
            sqlInsertQuery += "values (";

            for (int i = 0; i < pReferential.Column.Length; i++)
            {
                ReferentialsReferentialColumn rrc = pReferential.Column[i];

                bool isOk = ((!rrc.IsIdentity.Value) || (rrc.IsIdentity.sourceSpecified))
        && (rrc.ColumnName != "ROWVERSION")
        && (!rrc.IsAdditionalData)
        && (!rrc.Ressource.StartsWith("NotePad"))
        && (!rrc.Ressource.StartsWith("AttachedDoc"))
        && (!rrc.IsVirtualColumn);
                if (isOk)
                {
                    sqlInsertQuery += ", ";
                    if (rrc.IsForeignKeyField == true)
                    {
                        sqlInsertQuery += "(" + SQLCst.SELECT + "max( " + pFkParentColumn + " )" + SQLCst.X_FROM + this.m_owner + pParentTable + ")";
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
                                        + SQLCst.SELECT + " vw.IDACCKEY " + SQLCst.X_FROM + this.m_owner + " VW_ACCKEY vw "
                                        + SQLCst.X_INNER + this.m_owner + Cst.OTCml_TBL.ACCKEY + " ak " + SQLCst.ON + " (ak.IDACCKEY = vw.IDACCKEY) "
                                        + SQLCst.WHERE + " vw.IDACCKEYENUM = " + DataHelper.SQLString(currentIdAccEnum)
                                        + SQLCst.AND + " ak.IDACCINSTRENV =  "
                                        + "("
                                        + SQLCst.SELECT + " IDACCINSTRENV " + SQLCst.X_FROM + this.m_owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString()
                                        + SQLCst.WHERE + " IDACCINSTRENVDET = "
                                        + "("
                                        + SQLCst.SELECT + "max(IDACCINSTRENVDET)" + SQLCst.X_FROM + this.m_owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString()
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
                                        parameters.Add(new DataParameter(m_CS, "DataKey", DbType.Int32), row[rrc.ColumnName]);
                                        IDataReader dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, sqlSelectPkValue, parameters.GetArrayDbParameter());
                                        if (dr.Read())
                                            instrumentIdentifier = dr.GetString(0);
                                        dr.Close();
                                        selectPkValue = "(select " + columnName + " from " + this.m_owner + "" + tableName + " where IDENTIFIER=" + DataHelper.SQLString(instrumentIdentifier) + ")";
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
                                    sqlInsertQuery += DataHelper.SQLToDate(SessionTools.CS, Convert.ToDateTime(row[rrc.ColumnName]));
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
            return Convert.ToInt32(pRow["IDACCINSTRENVDET"].ToString());
        }

        /// <summary>
        /// Returns IDACCKEYENUM value from VW_ACCKEY view 
        /// </summary>
        /// <param name="pCurrentIdAccKey"></param>
        /// <param name="pCurrentIdAccInstrEnvDet"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetIdAccKeyEnum(Int32 pCurrentIdAccKey, Int32 pCurrentIdAccInstrEnvDet)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDACCKEY", DbType.Int32), pCurrentIdAccKey);
            parameters.Add(new DataParameter(SessionTools.CS, "IDACCINSTRENVDET", DbType.Int32), pCurrentIdAccInstrEnvDet);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + " vw.IDACCKEYENUM as IDACCKEYENUM " + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + this.m_owner + "VW_ACCKEY" + " vw " + Cst.CrLf;
            sqlSelect += SQLCst.X_INNER + this.m_owner + Cst.OTCml_TBL.ACCKEY + " ak " + SQLCst.ON + " (ak.IDACCKEY = vw.IDACCKEY) " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "vw.IDACCKEY = @IDACCKEY " + Cst.CrLf;
            sqlSelect += SQLCst.AND + "ak.IDACCINSTRENV =  " + Cst.CrLf;
            sqlSelect += "(" + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + "IDACCINSTRENV " + Cst.CrLf;
            sqlSelect += SQLCst.X_FROM + this.m_owner + Cst.OTCml_TBL.ACCINSTRENVDET.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "IDACCINSTRENVDET = @IDACCINSTRENVDET" + Cst.CrLf;
            sqlSelect += ")" + Cst.CrLf;
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    return Convert.ToString((dr["IDACCKEYENUM"]));
                else
                    return String.Empty;
            }
        }

        /// <summary>
        /// returns 
        /// true if the child menu shows a table 
        /// false if the child menu shows a view
        /// </summary>
        /// <param name="childTable"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private bool IsTable(string childTable)
        {
            bool isTable = false;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "OBJECTNAME", DbType.String), childTable);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "OBJECTTYPE as OBJECTTYPE" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSOBJECT.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " OBJECTNAME=@OBJECTNAME " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    isTable = ("TABLE" == Convert.ToString(dr["OBJECTTYPE"]));
            }
            return isTable;
        }

        /// <summary>
        /// return menu name for the table
        /// </summary>
        /// <returns></returns>
        private string GetIdMenu()
        {
            return CurrentPage.Request.QueryString["IdMenu"];
        }

        /// <summary>
        /// retreive a list contains all menu childrens
        /// </summary>
        /// <param name="pParentIdMenu"></param>
        /// <param name="pIdMenuList"></param>
        /// EG 20170519 Repository instead of Referential 
        private void RetreiveChildMenuList(string pParentIdMenu, List<string> pIdMenuList)
        {
            pIdMenuList.Clear();
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDMENU", DbType.String), pParentIdMenu);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "mo.IDMENU as IDMENU" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MENUOF.ToString() + " mo " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MENU + " m " + SQLCst.ON + " m.IDMENU=mo.IDMENU " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " mo.IDMENU_MENU=@IDMENU " + Cst.CrLf;
            sqlSelect += SQLCst.AND + "m.URL like 'List.aspx?Repository=%'" + Cst.CrLf;
            sqlSelect += SQLCst.ORDERBY + "POSITION" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];
            DataRow[] rows = dt.Select();

            //***********************************************************************************************
            //PL 20120823 Code mis en commentaire car résolu par la mise en place de POSITION dasn MENUOF
            //***********************************************************************************************
            //if (pParentIdMenu == "OTC_REF_ACC_INSTRENV")
            //{
            //    // OTC_REF_ACC_KEY lust be inserted before other siblings
            //    // before rows split: rows[1] is OTC_REF_ACC_KEY and rows[0] is OTC_REF_ACC_INSTRENVDET
            //    // after rows split: rows[1] is OTC_REF_ACC_INSTRENVDET and rows[0] is OTC_REF_ACC_KEY
            //    DataRow row_OTC_REF_ACC_KEY = rows[1];
            //    rows[1] = rows[0];
            //    rows[0] = row_OTC_REF_ACC_KEY;
            //}
            //***********************************************************************************************
            foreach (DataRow row in rows)
            {
                string childIdMenu = Convert.ToString(row["IDMENU"]);
                pIdMenuList.Add(childIdMenu);
            }
        }

        /// <summary>
        /// returns URL from menu
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        // EG 20180426 Analyse du code Correction [CA2202]
        private string GetUrl(string pIdMenu)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "IDMENU", DbType.String), pIdMenu);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "m.URL as URL" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.MENU.ToString() + " m " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " m.IDMENU=@IDMENU " + Cst.CrLf;

            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    return Convert.ToString((dr["URL"]));
                else
                    return String.Empty;
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

                XmlDocument xmlDoc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
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
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true
                };
                XmlWriter xmlWritter = XmlTextWriter.Create(write_File, xmlWriterSettings);
                xmlDoc.Save(xmlWritter);

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
