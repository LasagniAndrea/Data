using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EfsML.Business;
using EfsML.Enum;
using FixML.Enum;
using FixML.v50SP1.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    /// <summary>
    /// Classe contenant les paramètres nécéssaires à la lecture des données
    /// </summary>
    internal class CBReadFlowParameters
    {
        #region Members
        internal string CS;
        internal int IdAEntity;
        internal DateTime DtBusiness;
        internal string CBActorTable;
        internal string CBActorDealerTable;
        internal string CBActorClearerTable;
        // RD 20200629 [25361] PERF : Add
        internal string CBTradeBusinessTable;
        internal bool IsWithClearer;
        internal IEnumerable<int> IdCssValid;
        internal int IdACss = 0;
        // EG 20180906 PERF New (Use With instruction and Temporary based to model table)
        // EG 20181010 PERF Upd 
        internal string CBTradeFungibleTable;
        internal string CBTradeNotFungibleTable;
        internal string CBTradeExecutedTable;
        // RD 20200618 [25361] PERF (Use Temporary table based on TRADE CashPayment)
        internal string CBTradeCashPaymentTable;
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        internal string CBEventClassTable;
        // EG 20181119 PERF Correction post RC (Step 2)
        //internal bool isOracle12OrHigher;
        internal bool isNoHints;
        // RD 20200622 [25361] Add 
        internal string TableId;
        #endregion Members
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pCBActorTable"></param>
        /// <param name="pCBActorDealerTable"></param>
        /// <param name="pCBActorClearerTable"></param>
        /// <param name="pIsWithClearer"></param>
        /// <param name="pIdCssValid"></param>
        public CBReadFlowParameters(string pCS, int pIdAEntity, DateTime pDtBusiness, string pCBActorTable, string pCBActorDealerTable, string pCBActorClearerTable,
            bool pIsWithClearer, IEnumerable<int> pIdCssValid)
            : this(pCS, pIdAEntity, pDtBusiness, pCBActorTable, pCBActorDealerTable, pCBActorClearerTable, pIsWithClearer, pIdCssValid, 0) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pCBActorTable"></param>
        /// <param name="pCBActorDealerTable"></param>
        /// <param name="pCBActorClearerTable"></param>
        /// <param name="pCBTradeBusinessTable"></param>
        /// <param name="pIsWithClearer"></param>
        /// <param name="pIdCssValid"></param>
        /// <param name="pTableId"></param>
        // RD 20200622 [25361] PERF : Add 
        public CBReadFlowParameters(string pCS, int pIdAEntity, DateTime pDtBusiness, string pCBActorTable, string pCBActorDealerTable, string pCBActorClearerTable, string pCBTradeBusinessTable,
            bool pIsWithClearer, IEnumerable<int> pIdCssValid, string pTableId)
            : this(pCS, pIdAEntity, pDtBusiness, pCBActorTable, pCBActorDealerTable, pCBActorClearerTable, pCBTradeBusinessTable, pIsWithClearer, pIdCssValid, 0, pTableId) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pCBActorTable"></param>
        /// <param name="pCBActorDealerTable"></param>
        /// <param name="pCBActorClearerTable"></param>
        /// <param name="pIsWithClearer"></param>
        /// <param name="pIdCssValid"></param>
        /// <param name="pIdACss"></param>
        // RD 20200622 [25361] PERF : Add 
        public CBReadFlowParameters(string pCS, int pIdAEntity, DateTime pDtBusiness, string pCBActorTable, string pCBActorDealerTable, string pCBActorClearerTable,
            bool pIsWithClearer, IEnumerable<int> pIdCssValid, int pIdACss)
            : this(pCS, pIdAEntity, pDtBusiness, pCBActorTable, pCBActorDealerTable, pCBActorClearerTable, string.Empty, pIsWithClearer, pIdCssValid, pIdACss, string.Empty) { }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pCBActorTable"></param>
        /// <param name="pCBActorDealerTable"></param>
        /// <param name="pCBActorClearerTable"></param>
        /// <param name="pCBTradeBusinessTable"></param>
        /// <param name="pIsWithClearer"></param>
        /// <param name="pIdCssValid"></param>
        /// <param name="pIdACss"></param>
        /// <param name="pTableId"></param>
        // RD 20200622 [25361] PERF : Add pTableId
        // RD 20200629 [25361] PERF : Add pCBTradeBusinessTable
        public CBReadFlowParameters(string pCS, int pIdAEntity, DateTime pDtBusiness, string pCBActorTable, string pCBActorDealerTable, string pCBActorClearerTable, string pCBTradeBusinessTable,
            bool pIsWithClearer, IEnumerable<int> pIdCssValid, int pIdACss, string pTableId)
        {
            CS = pCS;
            IdAEntity = pIdAEntity;
            DtBusiness = pDtBusiness;

            CBActorTable = pCBActorTable;
            CBActorDealerTable = pCBActorDealerTable;
            CBActorClearerTable = pCBActorClearerTable;
            CBTradeBusinessTable = pCBTradeBusinessTable;

            IsWithClearer = pIsWithClearer;
            IdCssValid = pIdCssValid;
            IdACss = pIdACss;
            TableId = pTableId;
        }
        #endregion Constructors
        #region Methods
        #region QueryTradeAllocFungible
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200629 [25361] PERF : Use CBTradeBusinessTable work table
        // EG 20230227 [XXXXX] PERF : Add DTOUT predicate
        private string QueryTradeAllocFungible()
        {
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(bc PK_BOOK) */";
            return String.Format(@"select {0}
            tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDASSET, tr.ASSETCATEGORY, tr.SIDE, tr.DTSETTLT, 
            bd.IDA_ENTITY as IDA_ENTITYDEALER, bd.IDA as IDA_OWNERDEALER, bc.IDA_ENTITY as IDA_ENTITYCLEARER, bc.IDA as IDA_OWNERCLEARER,
            pr.GPRODUCT, pr.FAMILY
            from dbo.{1} tb
            inner join dbo.TRADE tr on (tr.IDT=tb.IDT)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and (ns.FUNGIBILITYMODE != 'NONE')
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (bd.IDA_ENTITY = @IDA_ENTITY) and ((tr.DTOUT is null) or (tr.DTOUT > @DTBUSINESS)) and
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", hintsOracle, CBTradeBusinessTable);
        }
        #endregion QueryTradeAllocFungible
        #region QueryTradeAllocNotFungible
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200629 [25361] PERF : Use CBTradeBusinessTable work table
        // EG 20230227 [XXXXX] PERF : Add DTOUT predicate
        private string QueryTradeAllocNotFungible()
        {
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(bc PK_BOOK) */";
            return String.Format(@"select {0}
            tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_DEALER, tr.IDB_DEALER, tr.IDA_CLEARER, tr.IDB_CLEARER, tr.IDASSET, tr.ASSETCATEGORY, tr.SIDE, tr.DTSETTLT, 
            bd.IDA_ENTITY as IDA_ENTITYDEALER, bd.IDA as IDA_OWNERDEALER, 
            bc.IDA_ENTITY as IDA_ENTITYCLEARER, bc.IDA as IDA_OWNERCLEARER,
            pr.GPRODUCT, pr.FAMILY
            from dbo.{1} tb
            inner join dbo.TRADE tr on (tr.IDT=tb.IDT)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and (ns.FUNGIBILITYMODE = 'NONE')
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.GPRODUCT = 'COM')
            inner join dbo.BOOK bd on (bd.IDB = tr.IDB_DEALER)
            inner join dbo.BOOK bc on (bc.IDB = tr.IDB_CLEARER)
            where (bd.IDA_ENTITY = @IDA_ENTITY) and ((tr.DTOUT is null) or (tr.DTOUT > @DTBUSINESS)) and
            (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC')", hintsOracle, CBTradeBusinessTable);
        }
        #endregion QueryTradeAllocNotFungible
        #region QueryTradeExecution
        // EG 20180906 PERF New (Use With instruction and Temporary based to model table)
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200618 [25361] PERF (Use WITH)
        // RD 20200629 [25361] PERF : Use CBTradeBusinessTable work table
        private string QueryTradeExecution()
        {
            string hintsOracle = isNoHints ? string.Empty : "/*+ index(bk PK_BOOK) */";
            return String.Format(@"with trExec as 
                (
                    select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTBUSINESS, 
                    ti.IDA_CSSCUSTODIAN, ti.IDA_BUYER, ti.IDB_BUYER, ti.IDA_SELLER, ti.IDB_SELLER, ti.IDASSET, ti.ASSETCATEGORY, 
                    pr.IDENTIFIER as PRODUCTIDENTIFIER, pr.GPRODUCT, pr.FAMILY
                    from dbo.{1} tb
                    inner join dbo.TRADE tr on (tr.IDT=tb.IDT)
                    inner join dbo.VW_ALLTRADEINSTRUMENT ti on (ti.IDT = tr.IDT)
                    inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP)  and (pr.GPRODUCT in ('FX','OTC','SEC')) and (pr.FUNGIBILITYMODE = 'NONE')
                    where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS in ('EXECUTED','INTERMED'))
                )                
                {2}select {0}
                tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_BUYER, tr.IDB_BUYER, tr.IDA_SELLER, tr.IDB_SELLER, tr.IDASSET, tr.ASSETCATEGORY, 
                tr.PRODUCTIDENTIFIER, tr.GPRODUCT, tr.FAMILY
                from trExec tr
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER)
                where (bk.IDA_ENTITY = @IDA_ENTITY)
                union all
                select {0}
                tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_BUYER, tr.IDB_BUYER, tr.IDA_SELLER, tr.IDB_SELLER, tr.IDASSET, tr.ASSETCATEGORY, 
                tr.PRODUCTIDENTIFIER, tr.GPRODUCT, tr.FAMILY
                from trExec tr
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_SELLER)
                where (bk.IDA_ENTITY = @IDA_ENTITY)", hintsOracle, CBTradeBusinessTable, "{0}");
        }
        #endregion QueryTradeExecution
        #region QueryCBPayment
        /// <summary>
        /// Load CashPayment Trades for business date
        /// </summary>
        /// <returns></returns>
        // RD 20200618 [25361] Add
        private string QueryTradeCashPayment()
        {
            return String.Format(@"select tr.IDT, tr.IDENTIFIER, tr.DTTRADE, tr.DTBUSINESS, 
            null as IDA_CSSCUSTODIAN, null as IDA_BUYER, null as IDB_BUYER, null as IDA_SELLER, null as IDB_SELLER, null as IDASSET, null as ASSETCATEGORY, 
            pr.IDENTIFIER as PRODUCTIDENTIFIER, pr.GPRODUCT, pr.FAMILY
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT inst on (inst.IDI = tr.IDI) and {0}
            inner join dbo.PRODUCT pr on (pr.IDP = inst.IDP)  and (pr.IDENTIFIER = 'cashPayment')
            where (tr.DTTRADE = @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR')", OTCmlHelper.GetSQLDataDtEnabled(CS, "inst", DtBusiness));
        }
        #endregion QueryCBPayment
        #region QueryEventClass
        /// <summary>
        /// Load EventClass for business date
        /// </summary>
        /// <returns></returns>
        // RD 20200602 [25361] Add
        // RD 20200618 [25361] Add REC EventClass
        private string QueryEventClass()
        {
            //PL 20230913 [26333] Add Oracle Hint /*+ INDEX (ec, IX_EVENTCLASS1) */  
            return @"select /*+ INDEX (ec, IX_EVENTCLASS1) */
            ec.IDE, ec.EVENTCLASS, ec.DTEVENT, ec.DTEVENTFORCED, ec.ISPAYMENT
            from dbo.EVENTCLASS ec 
            where (ec.EVENTCLASS in ('FWR','G_K','VAL')) and (ec.DTEVENT >= @DTBUSINESS)
            union all
            select 
            ec.IDE, ec.EVENTCLASS, ec.DTEVENT, ec.DTEVENTFORCED, ec.ISPAYMENT
            from dbo.EVENTCLASS ec 
            where (ec.EVENTCLASS in ('REC','RMV')) and (ec.DTEVENT = @DTBUSINESS)";
        }
        #endregion QueryEventClass
        #region SetTableTrade
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefixTableModel"></param>
        /// <param name="pPrefixTableWork"></param>
        /// <param name="pSelect"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200622 [25361] Remove pDbConnection and pTableId
        // RD 20200706 [25361] Manage insert using clause with
        private string SetTableTrade(string pPrefixTableModel, string pPrefixTableWork, string pSelect)
        {
            string tableModel = String.Format("CBTRADE_{0}_MODEL", pPrefixTableModel);
            string tableWork = String.Format("CBTRADE_{0}_{1}_W", pPrefixTableWork, TableId);
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            bool isWithClauseFirst = (DbSvrType.dbSQL == serverType) && (pSelect.StartsWith("with"));
            string sqlQueryInsert;
            if (isWithClauseFirst)
                sqlQueryInsert = String.Format(pSelect, "insert into dbo.{0}" + Cst.CrLf);
            else
                sqlQueryInsert = String.Format("{1}" + pSelect, "", "insert into dbo.{0} ");

            bool isExistTable = DataHelper.IsExistTable(CS, tableWork);

            string sqlQuery;
            if (isExistTable)
            {
                if (DbSvrType.dbSQL == serverType)
                    sqlQuery = String.Format(@"truncate table dbo.{0}; " + sqlQueryInsert, tableWork);
                else if (DbSvrType.dbORA == serverType)
                    sqlQuery = String.Format(@"delete from dbo.{0};" + sqlQueryInsert, tableWork);
                else
                    throw new NotImplementedException("RDBMS not implemented");
            }
            else
            {
                DataHelper.CreateTableAsSelect(CS, tableModel, tableWork, out string command);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create table", command));
                sqlQuery = String.Format(sqlQueryInsert, tableWork);
            }

            DataParameters dataParameters = new DataParameters();

            if (pSelect.Contains("@" + DataParameter.ParameterEnum.IDA_ENTITY.ToString()))
                dataParameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA_ENTITY), IdAEntity);

            if (pSelect.Contains("@" + DataParameter.ParameterEnum.DTBUSINESS.ToString()))
                dataParameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, dataParameters);

            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", qryParameters.GetType().Name, qryParameters.GetQueryReplaceParameters()));
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == isExistTable)
            {

                if (DbSvrType.dbSQL == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create clustered index IX_{0} on dbo.{0} (IDT)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                }
                else if (DbSvrType.dbORA == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create index IX_{0} on dbo.{0} (IDT, GPRODUCT, FAMILY)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                    if (pPrefixTableWork == "AF")
                    {
                        qryParameters = new QueryParameters(CS, String.Format("create index IX_1_{0} on dbo.{0} (IDA_ENTITYDEALER, IDA_DEALER)", tableWork), null);
                        AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX_1", qryParameters.GetQueryReplaceParameters()));
                        DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                        if (this.IsWithClearer)
                        {
                            qryParameters = new QueryParameters(CS, String.Format("create index IX_2_{0} on dbo.{0} (IDA_ENTITYCLEARER, IDA_CLEARER)", tableWork), null);
                            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX_2", qryParameters.GetQueryReplaceParameters()));
                            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                        }
                    }
                }
                else
                    throw new NotImplementedException("RDBMS not implemented");
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, string.Format("update statistic on {0}", tableWork));
                DataHelper.UpdateStatTable(CS, tableWork);
            }

            return tableWork;
        }
        #endregion SetTableTrade
        #region SetTableEvent
        /// <summary>
        /// Create Event Working Table
        /// </summary>
        /// <param name="pTableModel"></param>
        /// <param name="pPrefixTableWork"></param>
        /// <param name="pSelect"></param>
        /// <returns></returns>
        // RD 20200602 [25361] Add
        // RD 20200622 [25361] Remove pDbConnection and pTableId
        private string SetTableEvent(string pTableModel, string pPrefixTableWork, string pSelect)
        {
            string tableModel = pTableModel;
            string tableWork = String.Format("CBEVENT_{0}_{1}_W", pPrefixTableWork, TableId);
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            bool isExistTable = DataHelper.IsExistTable(CS, tableWork);
            string sqlQuery;
            if (isExistTable)
            {
                if (DbSvrType.dbSQL == serverType)
                    sqlQuery = String.Format(@"truncate table dbo.{0}; insert into dbo.{0} {1}", tableWork, pSelect);
                else if (DbSvrType.dbORA == serverType)
                    sqlQuery = String.Format(@"delete from dbo.{0};insert into dbo.{0} {1};", tableWork, pSelect);
                else
                    throw new NotImplementedException("RDBMS not implemented");
            }
            else
            {
                DataHelper.CreateTableAsSelect(CS, tableModel, tableWork, out string command);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create table", command));
                sqlQuery = String.Format(@"insert into dbo.{0} {1}", tableWork, pSelect);
            }

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);
            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, dataParameters);

            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", qryParameters.GetType().Name, qryParameters.GetQueryReplaceParameters()));
            DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

            if (false == isExistTable)
            {
                if (DbSvrType.dbSQL == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create clustered index IX_{0} on dbo.{0} (IDE,EVENTCLASS,DTEVENT)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                }
                else if (DbSvrType.dbORA == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create index IX_{0} on dbo.{0} (IDE,EVENTCLASS,DTEVENT)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                }
                else
                    throw new NotImplementedException("RDBMS not implemented");
            }

            if (DbSvrType.dbORA == serverType)
            {
                AppInstance.TraceManager.TraceVerbose(null, string.Format("update statistic on {0}", tableWork));
                DataHelper.UpdateStatTable(CS, tableWork);
            }

            return tableWork;
        }
        #endregion SetTableEvent
        #region SetTableFlow
        /// <summary>
        /// Create Event Working Table
        /// </summary>
        /// <param name="pTableModel"></param>
        /// <param name="pPrefixTableWork"></param>
        /// <param name="pSelect"></param>
        /// <returns></returns>
        // RD 20200622 [25361] Add
        // PL 20230706 [26333] Use queryHint  TEST IN PROGRESS...
        public string SetTableFlow(string pTableModel, string pPrefixTableWork, string pSelect)
        {
            string tableModel = pTableModel;
            string tableWork = String.Format("CBFLOW_{0}_{1}_W", pPrefixTableWork, TableId);
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            bool isExistTable = DataHelper.IsExistTable(CS, tableWork);

            if (false == isExistTable)
            {
                DataHelper.CreateTableAsSelect(CS, tableModel, tableWork, out string command);
                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create table", command));
                string sqlQuery = String.Format(@"insert into dbo.{0} {1}", tableWork, pSelect);
                DataParameters dataParameters = new DataParameters();
                dataParameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.DTBUSINESS), DtBusiness);
                QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, dataParameters);

                AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", qryParameters.GetType().Name, qryParameters.GetQueryReplaceParameters()));
                //PL 20230706 [26333] Use queryHint (see also SubQueryEventOtherFlow())  TEST IN PROGRESS...
                //DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.QueryHint, qryParameters.GetArrayDbParameterHint());

                if (DbSvrType.dbSQL == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create clustered index IX_{0} on dbo.{0} (IDT,IDA_PAY,IDA_REC)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                }
                else if (DbSvrType.dbORA == serverType)
                {
                    qryParameters = new QueryParameters(CS, String.Format("create index IX_{0} on dbo.{0} (IDT,IDA_PAY,IDA_REC)", tableWork), null);
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", "create index IX", qryParameters.GetQueryReplaceParameters()));
                    DataHelper.ExecuteNonQuery(CS, CommandType.Text, qryParameters.Query);
                }
                else
                    throw new NotImplementedException("RDBMS not implemented");

                if (DbSvrType.dbORA == serverType)
                {
                    AppInstance.TraceManager.TraceVerbose(null, string.Format("update statistic on {0}", tableWork));
                    DataHelper.UpdateStatTable(CS, tableWork);
                }
            }

            return tableWork;
        }
        #endregion SetTableFlow

        #region InitializeQueryCBTrade
        /// <summary>
        /// Insérer les trades candidats pour la chargement des flux (SQLServer)
        /// </summary>
        /// <param name="pCS"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200618 [25361] PERF (Use Temporary table based on TRADE CashPayment)
        // RD 20200622 [25361] Remove pDbConnection and pTableId
        public void InitializeQueryCBTrade()
        {
            Common.AppInstance.TraceManager.TraceVerbose(this, "Initializing Trade Fungible Table");
            CBTradeFungibleTable = SetTableTrade("ALLOC", "AF", QueryTradeAllocFungible());

            Common.AppInstance.TraceManager.TraceVerbose(this, "Initializing Trade Not Fungible Table");
            CBTradeNotFungibleTable = SetTableTrade("ALLOC", "ANF", QueryTradeAllocNotFungible());

            Common.AppInstance.TraceManager.TraceVerbose(this, "Initializing Trade Executed Table");
            CBTradeExecutedTable = SetTableTrade("EXEC", "EXEC", QueryTradeExecution());

            // RD 20200618 [25361] Perf
            Common.AppInstance.TraceManager.TraceVerbose(this, "Initializing Trade CashPayment Table");
            CBTradeCashPaymentTable = SetTableTrade("EXEC", "CP", QueryTradeCashPayment());

            // RD 20200602 [25361] Perf
            Common.AppInstance.TraceManager.TraceVerbose(this, "Initializing EventClass Table");
            CBEventClassTable = SetTableEvent("CBEVENTCLASS_MODEL", "EC", QueryEventClass());
        }
        #endregion InitializeQueryCBOTrade        
        #endregion Methods
    }

    /// <summary>
    /// Interface indiquant qu'une classe possède la méthode GetQueryParameters
    /// </summary>
    internal interface ICBQueryReaderParameters
    {
        /// <summary>
        /// Construction de QueryParameters de selection des données.
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam);
    }

    /// <summary>
    /// Classe de base pour la lecture des Trade Id
    /// </summary>
    internal abstract class CBTradeIdReader : CBReader
    {
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookTrade)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookTrade ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                int idt = Convert.ToInt32(Reader["IDT"]);
                string identifier_t = (Convert.IsDBNull(Reader["T_IDENTIFIER"]) ? string.Empty : Reader["T_IDENTIFIER"].ToString());
                DateTime dtBusiness = (Convert.IsDBNull(Reader["DTBUSINESS"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTBUSINESS"]));
                //
                ret = new CBBookTrade(idb, ida, idt, identifier_t, FlowTypeEnum.CashFlows, status, dtBusiness);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Class statique des outils de lecture des données
    /// </summary>
    internal static class CBQueryReader
    {
        #region Methods
        /// <summary>
        /// Lecture des données d'un DataReader et les restitues sous la forme d'une liste d'objet de type T
        /// </summary>
        /// <typeparam name="T">Type des données à à lire</typeparam>
        /// <typeparam name="TReader">Type du Reader de convertion des données en objet</typeparam>
        /// <param name="pDataReader"></param>
        /// <returns></returns>
        public static List<T> CBReadDataList<T, TReader>(IDataReader pDataReader) where TReader : IReaderRow, new()
        {
            List<T> flow;
            if (pDataReader != default(IDataReader))
            {
                flow = (from row in pDataReader.DataReaderEnumerator<T, TReader>()
                        where row != null
                        select row).ToList();
                //
                pDataReader.Close();
                pDataReader.Dispose();
            }
            else
            {
                flow = new List<T>();
            }
            return flow;
        }

        /// <summary>
        /// Lecture en asynchrone des données d'un DataReader et les restitues sous la forme d'une liste d'objet de type T
        /// </summary>
        /// <typeparam name="T">Type des données à à lire</typeparam>
        /// <typeparam name="TReader">Type du Reader de convertion des données en objet et contenant la requête de selection des données</typeparam>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public static async Task<List<T>> ReadDataAsync<T, TReader>(CBReadFlowParameters pCBQueryParam) where TReader : IReaderRow, ICBQueryReaderParameters, new()
        {
            TReader queryReader = new TReader();

            AppInstance.TraceManager.TraceVerbose(null, string.Format("Initializing {0} Flow", queryReader.GetType().Name));
            QueryParameters queryParameters = queryReader.GetQueryParameters(pCBQueryParam);
            AppInstance.TraceManager.TraceVerbose(null, string.Format("Name:{0} - SQL:{1}", queryReader.GetType().Name, queryParameters.GetQueryReplaceParameters()));
            //
            Task<IDataReader> readDataTask;
            if (queryParameters.Parameters != default(DataParameters))
            {
                readDataTask = Task<IDataReader>.Run(() => DataHelper.ExecuteReader(pCBQueryParam.CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()));
            }
            else
            {
                readDataTask = Task<IDataReader>.Run(() => DataHelper.ExecuteReader(pCBQueryParam.CS, CommandType.Text, queryParameters.Query));
            }
            //
            return await readDataTask.ContinueWith(r => CBReadDataList<T, TReader>(r.Result));
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de base permettant la lecture des données à partir d'un IDataReader
    /// </summary>
    internal abstract class CBReader : IReaderRow, ICBQueryReaderParameters
    {
        #region Members
        /// <summary>
        /// Membres static pour être partagé par toutes les instances des classes héritant de CBReader
        /// </summary>
        protected static IEnumerable<int> _IdCssValid;
        protected static DateTime _DtBusiness;
        protected static string _CS;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des CSS pour lesquels les données sont valid
        /// </summary>
        protected static IEnumerable<int> IdCssValid
        { get { return _IdCssValid; } }
        /// <summary>
        /// Date Business traitée
        /// </summary>
        /// EG|PM 20190328 [MIGRATION VCL] DtBusiness devient virtuelle (override sur CBLastCashBalanceReader pour avoir une date différente : ici Précédent CB)
        protected virtual DateTime DtBusiness
        {
            get { return _DtBusiness; }
            set { _DtBusiness = value; }
        }
        #endregion Accessors

        #region Methods
        #region Tools Methods
        /// <summary>
        /// Création des 2 paramètres commun à toutes les requêtes
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdAEntity"></param>
        /// <param name="pDtBusiness"></param>
        /// <returns></returns>
        protected static DataParameters CommonParameters(string pCS, int pIdAEntity, DateTime pDtBusiness)
        {
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS), pDtBusiness);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY), pIdAEntity);
            return dataParameters;
        }

        /// <summary>
        /// Donne la status du CSS/Custodian en paramètre
        /// </summary>
        /// <param name="pIda"></param>
        /// <returns></returns>
        protected static StatusEnum CssCustodianStatus(int pIda)
        {
            return _IdCssValid.Contains(pIda) ? StatusEnum.Valid : StatusEnum.Unvalid;
        }

        /// <summary>
        /// Construction de QueryParameters de selection des données.
        /// Attention: Cela initialise également les données statiques nécessaires à l'interprétation des données
        /// </summary>
        /// <param name="pSqlQuery"></param>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        /// EG|PM 20190328 [MIGRATION VCL] Utilisation Acceseur DtBusiness 
        protected QueryParameters GetQueryParameters(string pSqlQuery, CBReadFlowParameters pCBQueryParam)
        {
            // Intialisation membre static
            _IdCssValid = pCBQueryParam.IdCssValid;
            DtBusiness = pCBQueryParam.DtBusiness;
            //
            DataParameters dataParameters = CommonParameters(pCBQueryParam.CS, pCBQueryParam.IdAEntity, pCBQueryParam.DtBusiness);
            QueryParameters queryParameters = new QueryParameters(pCBQueryParam.CS, pSqlQuery, dataParameters);
            //
            return queryParameters;
        }
        #endregion Tools Methods

        #region Methods Sub Query Events "With"
        /// <summary>
        /// Sous requête "WITH" de selection des événements de frais (tous trades)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        internal static string SubQueryEventFee()
        {
            //EG 20230728 [26333] Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //EG 20230828 [26333] Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            //PL 20230913 [26333] Upd Oracle Hint /*+ INDEX (ecrec, UX_EVENTCLASS) */  
            string sql = @"/* Spheres:Hint NOPARAMS */
                select 
                ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT,	
	            ev.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE, 
                ev.ISREMOVED
	            from 
	            (
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| Settled Fee |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+- */
                    select {1}
                    ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 
		            evfee.PAYMENTTYPE, evfee.IDTAX, evfee.IDTAXDET, evfee.TAXCOUNTRY, evfee.TAXTYPE, evfee.TAXRATE, 0 as ISREMOVED 
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (evFee.IDTAX is null)

		            union all

		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| REMOVED Settled Fee |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */ 
		            ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 
		            evfee.PAYMENTTYPE, evfee.IDTAX, evFee.IDTAXDET, evfee.TAXCOUNTRY, evfee.TAXTYPE, evfee.TAXRATE, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @DTBUSINESS) and
		            (evFee.IDTAX is null)

		            union all

		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| Unsettled Fee |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
	                -- select {1}                    
	                select /*+ INDEX (ecrec, UX_EVENTCLASS) */   
                    ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 
		            evfee.PAYMENTTYPE, evfee.IDTAX, evfee.IDTAXDET, evfee.TAXCOUNTRY, evfee.TAXTYPE, evfee.TAXRATE, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
	                inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @DTBUSINESS) and
		            (evFee.IDTAX is null)
	            ) ev";
            return sql;
        }
        /// <summary>
        /// Sous requête "WITH" de selection des événements de taxes (tous trades)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        internal static string SubQueryEventTax()
        {
            //EG 20230728 [26333] Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //EG 20230828 [26333] Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            //PL 20230913 [26333] Upd Oracle Hint /*+ INDEX (ecrec, UX_EVENTCLASS) */  
            string sql = @"/* Spheres:Hint NOPARAMS */
                select 
	            ev.IDT, ev.IDE, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT,	
	            evfeep.PAYMENTTYPE, ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
                ev.ISREMOVED
	            from 
	            (
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| Settled Tax |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+--+- */
		            select {2}
                    ev.IDT, ev.IDE, ev.IDE_EVENT, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT,	
		            evfee.IDTAX, evfee.IDTAXDET, evfee.TAXCOUNTRY, evfee.TAXTYPE, evfee.TAXRATE, 0 as ISREMOVED 
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{3} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (evFee.IDTAX is not null)

		            union all

		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| REMOVED Settled Tax |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
	                -- select {2}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
                    ev.IDT, ev.IDE, ev.IDE_EVENT, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT,	
		            evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{3} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'DEACTIV') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (ecrmv.EVENTCLASS = 'RMV') and (ecrmv.DTEVENT = @DTBUSINESS) and
		            (evFee.IDTAX is not null)

		            union all

		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| Unsettled Tax |-+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
	                -- select {2}                    
	                select /*+ INDEX (ecrec, UX_EVENTCLASS) */  
                    ev.IDT, ev.IDE, ev.IDE_EVENT, ev.EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT,	
		            evFee.IDTAX, evFee.IDTAXDET, evFee.TAXCOUNTRY, evFee.TAXTYPE, evFee.TAXRATE, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
	                inner join dbo.{3} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ecrec on (ecrec.IDE = ec.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ec.IDE)
		            inner join dbo.EVENTFEE evfee on (evfee.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('OPP', 'SKP')) and (ev.IDSTACTIVATION = 'REGULAR') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT > @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (ecrec.EVENTCLASS = 'REC') and (ecrec.DTEVENT <= @DTBUSINESS) and
		            (evFee.IDTAX is not null)
	            ) ev
                inner join dbo.EVENTFEE evfeep on (evfeep.IDE = ev.IDE_EVENT)";
            return sql;
        }

        /// <summary>
        /// Sous requête "WITH" de selection des événements CashFlows (alloc fungible)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        internal static string SubQueryEventCashFlow()
        {
            //EG 20230728 [26333] Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //EG 20230828 [26333] Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            string sql = @"/* Spheres:Hint NOPARAMS */
                select
	            ev.IDT, ev.IDE, null as EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT, 
                null as PAYMENTTYPE, null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE,
                ev.ISREMOVED
	            from 
	            (
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| Variation Margin/Cash Settlement |+-+-+-+-+- */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('LPC','LPP','TER')) and (ev.EVENTTYPE in ('VMG','SCU')) and (ev.IDSTACTIVATION = 'REGULAR') and
	                (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS)
	    
		            union all
	    
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| REMOVED Variation Margin/Cash Settlement |+-+-+-+-+- */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
	                where (ev.EVENTCODE in ('LPC','LPP','TER')) and (ev.EVENTTYPE in ('VMG','SCU')) and (ev.IDSTACTIVATION = 'DEACTIV') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT <  @DTBUSINESS) and
		            (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS)  

		            union all
		
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| Premium/Delivery Amount/Equalization Payment |+-+-+-+-+-+- */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
	                inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('LPC','LPP','INT','TER')) and (ev.EVENTTYPE in ('PRM','DVA','EQP')) and (ev.IDSTACTIVATION = 'REGULAR') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) 

		            union all

		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| REMOVED Premium/Delivery Amount/Equalization Payment |+-+-+-+-+ */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
		            inner join dbo.EVENTCLASS ecstl on (ecstl.IDE = ecrmv.IDE)
		            where (ev.EVENTCODE in ('LPC','LPP','INT','TER')) and (ev.EVENTTYPE in ('PRM','DVA','EQP')) and (ev.IDSTACTIVATION = 'DEACTIV') and
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT < @DTBUSINESS) and 
		            (ecstl.EVENTCLASS = 'STL') and (ecstl.ISPAYMENT = 1) and
		            (ecrmv.EVENTCLASS='RMV') and (ecrmv.DTEVENT = @DTBUSINESS)

	            ) ev";
            return sql;
        }

        /// <summary>
        /// Sous requête "WITH" de selection des Trades CashPayment
        /// </summary>
        /// <returns></returns>
        // RD 20200618 [25361] Add
        internal static string SubQueryEventPayment()
        {
            string sql = @"select
	            ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.IDB_PAY, ev.IDB_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT, ev.ISREMOVED
	            from 
	            (
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| Cash Payment                     |+-+-+-+-+- */
		            /* +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.IDB_PAY, ev.IDB_REC, ev.VALORISATION, ev.UNIT, 
                    ec.DTEVENT as DTVAL, 
                    0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'STA')
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
	            ) ev";
            return sql;
        }

        /// <summary>
        /// Retourne true si les optimisations hints doivent être appliquées sur les requêtes allocFungibleOtherFlow (Flows et Trade) 
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        /// FI 20190619 [24722] add Method
        protected static Boolean IsCBAllocFungibleOtherFlowNoHints(CBReadFlowParameters pCBQueryParam)
        {
            Boolean ret = pCBQueryParam.isNoHints;
            // FI 20190619 [24722] Add lecture du settings CBReaderAllocFungibleOtherFlowsMode
            string hintsOracleSettings = SystemSettings.GetAppSettings("CBReaderAllocFungibleOtherFlowsMode", "Default");
            switch (hintsOracleSettings)
            {
                case "Default":
                    break;
                case "NoHints":
                    ret = true;
                    break;
                case "Hints":
                    ret = false;
                    break;
                default:
                    throw new NotSupportedException(StrFunc.AppendFormat("hints {0} is not supported", hintsOracleSettings));
            }
            return ret;
        }


        /// <summary>
        /// Sous requête "WITH" de selection des événements OtherFlows (alloc fungible)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // PL 20190628 Add Flows TER/INT (for DSE) and Add Unsettled INT|TER/INT
        // PL 20190718 Remove Unsettled INT|TER/INT (VALBURY ne souhaitant pas voir les couposn avant leur règlement). 
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        // PL 20230706 [26333] Add Spheres:Hint NOPARAMS / Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */  TEST IN PROGRESS...
        internal static string SubQueryEventOtherFlow()
        {
            //PL 20230706 Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //PL 20230706 Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            string sql = @"/* Spheres:Hint NOPARAMS */
	            select
	            ev.IDT, ev.IDE, null as EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT, 
                null as PAYMENTTYPE, null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE,
                ev.ISREMOVED
	            from 
	            (
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                /* +-+-+-+-+-| Flows BWA,FDA,GAM,LOV,MGR |+-+-+-+-+- */
	                /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
                    select {1}
	                ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) 
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            where (ev.EVENTCODE in ('LPC','LPP')) and  (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION = 'REGULAR') and
	                (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS)
		            union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| REMOVED Flows BWA,FDA,GAM,LOV,MGR |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
	                ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
	                where (ev.EVENTCODE in ('LPC','LPP')) and  (ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','MGR')) and (ev.IDSTACTIVATION = 'DEACTIV') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT <  @DTBUSINESS) and
		            (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS)  
		            union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| Flows RMG (sauf pour equitySecurityTransaction) |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
                    select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ev.IDE)
		            inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and (pad.IDT_CLOSING = ev.IDT)
		            where ((tr.GPRODUCT != 'SEC') or (tr.FAMILY != 'ESE')) and 
                    (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = 'RMG') and (ev.IDSTACTIVATION = 'REGULAR') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS)
		            union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| REMOVED Flows RMG (sauf pour equitySecurityTransaction) |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
		            ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
		            inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ev.IDE)
		            inner join dbo.POSACTIONDET pad on (pad.IDPADET = epad.IDPADET) and (pad.IDT_CLOSING = ev.IDT)
		            where ((tr.GPRODUCT != 'SEC') or (tr.FAMILY != 'ESE')) and 
		            (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = 'RMG') and (ev.IDSTACTIVATION = 'DEACTIV') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT <  @DTBUSINESS) and 
	                (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS) 
	                union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-| Unsettled GAM et INT|TER/INT |+-+-+-+-+- */ 
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
                    select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
	                inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ec_rec on (ec_rec.IDE = ev.IDE)
                    where ((tr.GPRODUCT = 'SEC') or (tr.FAMILY = 'ESE')) and
		            ( (ev.EVENTCODE in ('LPC','LPP') and ev.EVENTTYPE = 'GAM') /* or (ev.EVENTCODE in ('INT','TER') and ev.EVENTTYPE = 'INT') */ ) and 
                    (ev.IDSTACTIVATION = 'REGULAR') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >  @DTBUSINESS) and 
		            (ec_rec.EVENTCLASS = 'REC') and (ec_rec.DTEVENT <=  @DTBUSINESS)
	                union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
		            /* +-+-+-+-+-|  Flows UMG et MKV  |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+ */
                    select {1}
		            ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
	                inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            where (ev.EVENTCODE = 'LPC') and (ev.IDSTACTIVATION = 'REGULAR') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS) and 
		            (
			            ((tr.DTSETTLT is null) and (ev.EVENTTYPE='UMG'))
			            or
			            (
				            (tr.DTSETTLT is not null) and
				            (
					            ((ev.EVENTTYPE = 'MKV') and (ec.DTEVENT >= tr.DTSETTLT))
					            or
					            ((ev.EVENTTYPE = 'UMG') and (ec.DTEVENT < tr.DTSETTLT))
				            )
			            )
		            )
		            union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| Flows INT|TER/INT (for DSE) |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
                    select {1}
		            ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            where (tr.GPRODUCT = 'SEC') and (tr.FAMILY = 'DSE') and 
		            (ev.EVENTCODE in ('INT','TER')) and (ev.EVENTTYPE = 'INT') and (ev.IDSTACTIVATION = 'REGULAR') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DTBUSINESS) 
		            union all
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| REMOVED Flows INT|TER/INT (for DSE) |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
		            ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
		            inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
		            where (tr.GPRODUCT = 'SEC') and (tr.FAMILY = 'DSE') and 
		            (ev.EVENTCODE = 'INT') and (ev.EVENTTYPE = 'INT') and (ev.IDSTACTIVATION = 'DEACTIV') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT <  @DTBUSINESS) and
		            (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS) 
	            ) ev";
            return sql;
        }

        /// <summary>
        /// Sous requête "WITH" de selection des événements des trades (execution)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        internal static string SubQueryEventExecutionTradeFlow()
        {
            //EG 20230728 [26333] Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //EG 20230828 [26333] Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            string sql = @"/* Spheres:Hint NOPARAMS */
                select
	            ev.IDT, ev.IDE, null as EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT,
                null as PAYMENTTYPE, null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE,
                ev.ISREMOVED
	            from 
	            (
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                /* +-+-+-+-+-| Flows MGR,BWA,FDA,GAM,LOV,RMG,VMG,UMG,PRM,CU1,CU2,SCU |+-+-+-+-+- */
	                /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
                    where (ev.IDSTACTIVATION = 'REGULAR') and (ec.DTEVENT >= @DTBUSINESS) and
                    (
                        (
                            (ev.EVENTCODE = 'LPC' and ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL')) or
                            (ev.EVENTCODE in ('LPC','LPP') and ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS = 'VAL')
                        )
                        or
                        (
                            (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL') and
                            ((ev.EVENTCODE in ('LPC','LPP') and ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')))
                        )
                    )
	    
		            union all
	    
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
                    /* +-+-+-+-+-| REMOVED Flows MGR,BWA,FDA,GAM,LOV,RMG,VMG,UMG,PRM,CU1,CU2,SCU |+-+-+-+-+- */
                    /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
	                where (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS) and (ev.IDSTACTIVATION = 'DEACTIV') and (ec.DTEVENT <  @DTBUSINESS) and
                    (
                        (
                            (ev.EVENTCODE = 'LPC' and ev.EVENTTYPE = 'MGR' and ec.EVENTCLASS in ('FWR','G_K','VAL')) or
                            (ev.EVENTCODE in ('LPC','LPP') and ev.EVENTTYPE in ('BWA','FDA','GAM','LOV','RMG','VMG','UMG') and ec.EVENTCLASS = 'VAL')
                        )
                        or
                        (
                            (ec.ISPAYMENT = 1) and (ec.EVENTCLASS='VAL') and
                            ((ev.EVENTCODE in ('LPC','LPP') and ev.EVENTTYPE = 'PRM') or (ev.EVENTCODE = 'TER' and ev.EVENTTYPE in ('CU1','CU2','SCU')))
                        )
                    )
	            ) ev";
            return sql;
        }

        /// <summary>
        /// Sous requête "WITH" de selection des événements des trades (alloc not fungible)
        /// </summary>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        internal static string SubQueryEventNotFungibleTradeFlow()
        {
            //EG 20230728 [26333] Add Hint NOPARAMS [NB: s'agissant de table de travail dont le nom change à chaque exécution, il est préférable de ne pas utiliser de variable pour permettre à l'optimisateur du RDBMS de calculer un plan plus performant]
            //EG 20230828 [26333] Add Oracle Hint /*+ INDEX (ec, UX_EVENTCLASS) */ 
            string sql = @"/* Spheres:Hint NOPARAMS */
                select
	            ev.IDT, ev.IDE, null as EVENTCODE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ev.DTVAL, ev.VALORISATION, ev.UNIT, 
                null as PAYMENTTYPE, null as IDTAX, null as IDTAXDET, null as TAXCOUNTRY, null as TAXTYPE, null as TAXRATE,
                ev.ISREMOVED
	            from 
	            (
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                /* +-+-+-+-+-| Flows LPP/GAM |+-+-+-+-+- */
	                /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                select {1}
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, ec.DTEVENT as DTVAL, ev.VALORISATION, ev.UNIT, 0 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ec on (ec.IDE = ev.IDE)
		            where (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and (ev.IDSTACTIVATION = 'REGULAR') and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT >= @DTBUSINESS)
	    
		            union all
	    
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
		            /* +-+-+-+-+-| REMOVED Flows LLP/GAM |+-+-+-+-+- */
		            /* +-+--+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+- */
	                -- select {1}                    
	                select /*+ INDEX (ec, UX_EVENTCLASS) */
                    ev.IDT, ev.IDE, ev.EVENTTYPE, ev.IDA_PAY, ev.IDA_REC, @DTBUSINESS as DTVAL, ev.VALORISATION, ev.UNIT, 1 as ISREMOVED
		            from dbo.{0} tr
		            inner join dbo.EVENT ev on (ev.IDT = tr.IDT)
		            inner join dbo.{2} ecrmv on (ecrmv.IDE = ev.IDE)
	                inner join dbo.EVENTCLASS ec on (ec.IDE = ecrmv.IDE)
	                where (ev.EVENTCODE = 'LPP') and (ev.EVENTTYPE = 'GAM') and (ev.IDSTACTIVATION = 'DEACTIV') and 
		            (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT <  @DTBUSINESS) and
		            (ecrmv.EVENTCLASS ='RMV') and (ecrmv.DTEVENT = @DTBUSINESS)  
	    
	            ) ev";
            return sql;
        }
        #endregion Methods Sub Query Events "With"
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// Construction de QueryParameters de selection des données.
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public abstract QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam);
        #endregion ICBQueryParameters

        #region IReaderRow
        /// <summary>
        /// Data Reader permettant de lire les enregistrements
        /// </summary>
        public IDataReader Reader { get; set; }
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'un objet
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public abstract object GetRowData();
        #endregion IReaderRow
    }

    #region Class Flows Reader
    #region CBAllocFungible
    /// <summary>
    /// Permet la lecture des frais des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleFeeFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFFEE", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDDC, amt.IDASSET, 
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                sum(case tr.IDA_DEALER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
                ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
	            sum(case tr.IDA_CLEARER when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
	            ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE" + Cst.CrLf;
            }
            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idDC = (Convert.IsDBNull(Reader["IDDC"]) ? 0 : Convert.ToInt32(Reader["IDDC"]));
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, 0, 0, default, default, 0, assetCategoryEnum, idDC, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des taxes des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleTaxFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeFungibleTable, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFTAX", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDDC, amt.IDASSET, 
            amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE,
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
                sum(case tr.IDA_DEALER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
                ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
	            sum(case tr.IDA_CLEARER when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
	            ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE" + Cst.CrLf;
            }
            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            // Intialisation membre static
            _CS = pCBQueryParam.CS;
            //
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                int idTax = (Convert.IsDBNull(Reader["IDTAX"]) ? 0 : Convert.ToInt32(Reader["IDTAX"]));
                int idTaxDet = (Convert.IsDBNull(Reader["IDTAXDET"]) ? 0 : Convert.ToInt32(Reader["IDTAXDET"]));
                string taxCountry = (Convert.IsDBNull(Reader["TAXCOUNTRY"]) ? string.Empty : Reader["TAXCOUNTRY"].ToString());
                string taxType = (Convert.IsDBNull(Reader["TAXTYPE"]) ? string.Empty : Reader["TAXTYPE"].ToString());
                decimal taxRate = (Convert.IsDBNull(Reader["TAXRATE"]) ? 0 : Convert.ToDecimal(Reader["TAXRATE"]));
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idDC = (Convert.IsDBNull(Reader["IDDC"]) ? 0 : Convert.ToInt32(Reader["IDDC"]));
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (idTax > 0)
                {
                    // Arrondir le Montant des taxes
                    EFS_Cash cash = new EFS_Cash(_CS, amount, idc);
                    amount = cash.AmountRounded;
                }
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, idDC, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des CashFlows des allocations fongibles
    /// </summary>
    // EG 20181119 PERF Correction post RC (Step 2)
    internal sealed class CBAllocFungibleCashFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des CashFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventCashFlow(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFCF", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDDC, amt.IDASSET,
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, 
                sum(case tr.IDA_DEALER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
                ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"	select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, asset.ASSETCATEGORY,
                case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
                case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, 
	            sum(case tr.IDA_CLEARER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
                left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, asset.ASSETCATEGORY,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
	            ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;
            }
            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des CashFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idDC = (Convert.IsDBNull(Reader["IDDC"]) ? 0 : Convert.ToInt32(Reader["IDDC"]));
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                // DVA identique à SCU (pas de distinction entre cashSettlement et delivery Amount)
                if (flowSubType == FlowSubTypeEnum.DVA)
                {
                    flowSubType = FlowSubTypeEnum.SCU;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, default, 0, 0, default, default, 0, assetCategoryEnum, idDC, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des OtherFlows des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleOtherFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des OtherFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            // FI 20190619 [24722] 
            //string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string hintsOracle = CBReader.IsCBAllocFungibleOtherFlowNoHints(pCBQueryParam) ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventOtherFlow(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFOF", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.CATEGORY, amt.FUTVALUATIONMETHOD, amt.IDDC, amt.IDASSET, amt.SIDE,
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, tr.SIDE,
	            asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
	            ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE,
	            sum(case tr.IDA_DEALER  when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
                                        when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
	            left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.SIDE,
	            asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
	            ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, tr.SIDE,
	            asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end as IDDC,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end as IDASSET,
	            ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE,
	            sum(case tr.IDA_CLEARER when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
				                        when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo.VW_ASSET asset on (asset.IDASSET = tr.IDASSET) and (asset.ASSETCATEGORY = tr.ASSETCATEGORY)
	            left outer join dbo.VW_ASSET_ETD_EXPANDED asset_ETD on (asset_ETD.IDASSET = tr.IDASSET) and ('ExchangeTradedContract' = tr.ASSETCATEGORY)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.SIDE,
	            asset.ASSETCATEGORY, asset_ETD.CATEGORY, asset_ETD.FUTVALUATIONMETHOD,
	            case when asset_ETD.IDDC is null then 0 else asset_ETD.IDDC end,
	            case when asset_ETD.IDDC is null then asset.IDASSET else 0 end,
	            ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;
            }

            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des OtherFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                string category = (Convert.IsDBNull(Reader["CATEGORY"]) ? string.Empty : Reader["CATEGORY"].ToString());
                Nullable<CfiCodeCategoryEnum> categoryEnum = ReflectionTools.ConvertStringToEnumOrNullable<CfiCodeCategoryEnum>(category);
                string futValuationMethod = (Convert.IsDBNull(Reader["FUTVALUATIONMETHOD"]) ? string.Empty : Reader["FUTVALUATIONMETHOD"].ToString());
                Nullable<FuturesValuationMethodEnum> futValuationMethodEnum = ReflectionTools.ConvertStringToEnumOrNullable<FuturesValuationMethodEnum>(futValuationMethod);
                int idDC = (Convert.IsDBNull(Reader["IDDC"]) ? 0 : Convert.ToInt32(Reader["IDDC"]));
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string side = (Convert.IsDBNull(Reader["SIDE"]) ? string.Empty : Reader["SIDE"].ToString());
                Nullable<SideEnum> sideEnum = ReflectionTools.ConvertStringToEnumOrNullable<SideEnum>(side);
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                CBDetFlow flow;
                if (flowSubType == FlowSubTypeEnum.MGR)
                {
                    // Margin Requierement sur les Trades (CFD)
                    flow = new CBDetDeposit(idb, ida, amount, idc, DtBusiness, 0, status);
                }
                else
                {
                    // Lorsque la date valeur est différente de la date business alors Unsettled
                    if (dtVal != DtBusiness)
                    {
                        // Unsettled
                        flowSubType = FlowSubTypeEnum.UST;
                    }
                    else if ((flowSubType == FlowSubTypeEnum.GAM) || (flowSubType == FlowSubTypeEnum.INT))
                    {
                        // Transformer les GrossAMount (GAM) et les INT/INT de la famille de product DSE en SCU
                        flowSubType = FlowSubTypeEnum.SCU;
                    }
                    flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, assetCategoryEnum, categoryEnum, futValuationMethodEnum, idDC, idAsset, sideEnum, status);
                }
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBAllocFungible

    #region CBExecutedTrade
    /// <summary>
    /// Permet la lecture des frais des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeFeeFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeExecutedTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXECFEE", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, 
            amt.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.ASSETCATEGORY, tr.IDASSET,
                bk.IDA, bk.IDB, bk.IDENTIFIER,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                sum(case bk.IDA when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							    when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							    end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
	            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect += @"group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, bk.IDENTIFIER, tr.ASSETCATEGORY, tr.IDASSET, ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE
            ) amt" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, 0, 0, default, default, 0, assetCategoryEnum, 0, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des taxes des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeTaxFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeExecutedTable, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXECTAX", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, 
            amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE,
            amt.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
                sum(case bk.IDA when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							    when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							    end * ev.VALORISATION) as AMOUNT,
                bk.IDENTIFIER
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
	            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect += @"group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, bk.IDENTIFIER, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE
            ) amt" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                int idTax = (Convert.IsDBNull(Reader["IDTAX"]) ? 0 : Convert.ToInt32(Reader["IDTAX"]));
                int idTaxDet = (Convert.IsDBNull(Reader["IDTAXDET"]) ? 0 : Convert.ToInt32(Reader["IDTAXDET"]));
                string taxCountry = (Convert.IsDBNull(Reader["TAXCOUNTRY"]) ? string.Empty : Reader["TAXCOUNTRY"].ToString());
                string taxType = (Convert.IsDBNull(Reader["TAXTYPE"]) ? string.Empty : Reader["TAXTYPE"].ToString());
                decimal taxRate = (Convert.IsDBNull(Reader["TAXRATE"]) ? 0 : Convert.ToDecimal(Reader["TAXRATE"]));
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, 0, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des flux des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventExecutionTradeFlow(), pCBQueryParam.CBTradeExecutedTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXEC", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, amt.PRODUCTIDENTIFIER, amt.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.ASSETCATEGORY, tr.IDASSET, tr.PRODUCTIDENTIFIER,
                bk.IDA, bk.IDB, bk.IDENTIFIER,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE, 
                sum(case bk.IDA when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							    when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							    end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
	            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect += @"group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, bk.IDENTIFIER, tr.PRODUCTIDENTIFIER, tr.ASSETCATEGORY, tr.IDASSET, ev.DTVAL, ev.UNIT, ev.EVENTTYPE
            ) amt" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                bool isFlowToAdd = true;
                //
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string productIdentifier = (Convert.IsDBNull(Reader["PRODUCTIDENTIFIER"]) ? string.Empty : Reader["PRODUCTIDENTIFIER"].ToString());
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                SideEnum? sideEnum = null;
                if (flowSubType == FlowSubTypeEnum.LOV)
                {
                    sideEnum = (amount < 0) ? SideEnum.Sell : SideEnum.Buy;
                }
                else if (flowSubType == FlowSubTypeEnum.UMG)
                {
                    // Ne pas prendre les UMG sur les Options
                    switch (productIdentifier.Trim())
                    {
                        case Cst.ProductFxSimpleOption:
                        case Cst.ProductFxDigitalOption:
                        case Cst.ProductFxBarrierOption:
                        case Cst.ProductFxAverageRateOption:
                            isFlowToAdd = false;
                            break;
                    }
                }
                else if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                else if ((flowSubType == FlowSubTypeEnum.CU1) || (flowSubType == FlowSubTypeEnum.CU2))
                {
                    // Cash Settlement
                    flowSubType = FlowSubTypeEnum.SCU;
                }
                else if (flowSubType == FlowSubTypeEnum.GAM)
                {
                    // Amount => Cash Settlement
                    flowSubType = FlowSubTypeEnum.SCU;
                }
                if (isFlowToAdd)
                {
                    CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, default, 0, 0, default, default, 0, assetCategoryEnum, default, default, 0, idAsset, sideEnum, status);
                    ret = new CBBookLeaf(ida, idb, identifier_b, flow);
                }
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBExecutedTrade

    #region CBAllocNotFungible
    /// <summary>
    /// Permet la lecture des frais des allocations non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleFeeFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeNotFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANFFEE", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, 
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                sum(case tr.IDA_DEALER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                sum(case tr.IDA_CLEARER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE" + Cst.CrLf;
            }
            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)
            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "b", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, 0, 0, default, default, 0, assetCategoryEnum, 0, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des taxes des allocations non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleTaxFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeNotFungibleTable, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANFTAX", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.PAYMENTTYPE, amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, 
            amt.IDTAX, amt.IDTAXDET, amt.TAXCOUNTRY, amt.TAXTYPE, amt.TAXRATE,
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
                sum(case tr.IDA_DEALER	when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
                ev.DTVAL, ev.UNIT as IDC, ev.EVENTCODE as AMOUNTSUBTYPE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE,
	            sum(case tr.IDA_CLEARER when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
							            when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT, ev.EVENTCODE, ev.PAYMENTTYPE,
                ev.IDTAX, ev.IDTAXDET, ev.TAXCOUNTRY, ev.TAXTYPE, ev.TAXRATE" + Cst.CrLf;
            }
            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)
            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "b", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string paymentType = (Convert.IsDBNull(Reader["PAYMENTTYPE"]) ? string.Empty : Reader["PAYMENTTYPE"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                int idTax = (Convert.IsDBNull(Reader["IDTAX"]) ? 0 : Convert.ToInt32(Reader["IDTAX"]));
                int idTaxDet = (Convert.IsDBNull(Reader["IDTAXDET"]) ? 0 : Convert.ToInt32(Reader["IDTAXDET"]));
                string taxCountry = (Convert.IsDBNull(Reader["TAXCOUNTRY"]) ? string.Empty : Reader["TAXCOUNTRY"].ToString());
                string taxType = (Convert.IsDBNull(Reader["TAXTYPE"]) ? string.Empty : Reader["TAXTYPE"].ToString());
                decimal taxRate = (Convert.IsDBNull(Reader["TAXRATE"]) ? 0 : Convert.ToDecimal(Reader["TAXRATE"]));
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                //
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, paymentType, idTax, idTaxDet, taxCountry, taxType, taxRate, assetCategoryEnum, 0, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }

    /// <summary>
    /// Permet la lecture des flux des allocations non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleFlowReader : CBReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventNotFungibleTradeFlow(), pCBQueryParam.CBTradeNotFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANF", sqlSubQuery);

            string sqlSelect = @"select amt.IDA_CSSCUSTODIAN, amt.IDA, amt.IDB, amt.DTVAL, amt.AMOUNT, amt.IDC, 
            amt.AMOUNTSUBTYPE, amt.ASSETCATEGORY, amt.IDASSET, 
            b.IDENTIFIER as B_IDENTIFIER
            from 
            (
	            select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE,
	            sum(case tr.IDA_DEALER  when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
                                        when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
	            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT as IDC, ev.EVENTTYPE as AMOUNTSUBTYPE,
	            sum(case tr.IDA_CLEARER when ev.IDA_PAY then case when (ISREMOVED=0) then -1 else  1 end
				                        when ev.IDA_REC then case when (ISREMOVED=0) then 1  else -1 end
							            end * ev.VALORISATION) as AMOUNT
	            from {0} ev
	            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
	            inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
	            where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
	            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.ASSETCATEGORY, tr.IDASSET,
	            ev.DTVAL, ev.UNIT, ev.EVENTTYPE" + Cst.CrLf;
            }

            sqlSelect += @") amt
            left outer join dbo.BOOK b on (b.IDB = amt.IDB)
            where (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "b", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idaCssCustodian = Convert.ToInt32(Reader["IDA_CSSCUSTODIAN"]);
                StatusEnum status = CssCustodianStatus(idaCssCustodian);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                DateTime dtVal = (Convert.IsDBNull(Reader["DTVAL"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTVAL"]));
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                Cst.UnderlyingAsset assetCategoryEnum = Cst.ConvertToUnderlyingAsset(assetCategory);
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                //
                if (dtVal != DtBusiness)
                {
                    // Unsettled
                    flowSubType = FlowSubTypeEnum.UST;
                }
                else if (flowSubType == FlowSubTypeEnum.GAM)
                {
                    // Amount => Cash Settlement
                    flowSubType = FlowSubTypeEnum.SCU;
                }
                CBDetCashFlows flow = new CBDetCashFlows(idb, ida, amount, idc, flowSubType, dtVal, default, 0, 0, default, default, 0, assetCategoryEnum, 0, idAsset, status);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBAllocNotFungible

    #region CBDepositReader
    /// <summary>
    /// Permet la lecture des deposits
    /// </summary>
    internal sealed class CBDepositReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.Deposit;
        #endregion Constants

        #region Methods
        /// <summary>
        /// Requête de selection des deposits
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ ordered index(tr IX_TRADE4) */";

            string dtEnabledInstr = OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "ns", "@DTBUSINESS");
            string sqlCss = String.Empty;
            if (pCBQueryParam.IdACss > 0)
                sqlCss = " and (tr.IDA_CSSCUSTODIAN = @IDA_CSS)";

            string sql = @"select amt.IDT, amt.T_IDENTIFIER, amt.DTSYS, amt.IDA_CSS, amt.IDA, amt.IDB, amt.B_IDENTIFIER, amt.IDC, amt.AMOUNT
            from (
                select {0}
                tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, tr.IDA_CSSCUSTODIAN as IDA_CSS, b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER, 
                ev.UNIT as IDC, (-1 * ev.VALORISATION) as AMOUNT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT ns on  (ns.IDI = tr.IDI) and ({1})
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'marginRequirement')
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = 'MGR') and (ev.EVENTCODE = 'LPC')
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
                inner join dbo.BOOK b on (b.IDB = ev.IDB_PAY)
                inner join dbo.{3} cba on (cba.IDA = b.IDA)
                inner join dbo.ACTORROLE ar on (ar.IDA = cba.IDA) and (ar.IDROLEACTOR = 'MARGINREQOFFICE')
                where (tr.DTBUSINESS = @DTBUSINESS)  and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR') 
                and (ev.IDA_REC = @IDA_ENTITY) {2}" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                #region union all pour le Clearer
                sql += @"   union all
                    select {0}
                    tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTSYS, tr.IDA_CSSCUSTODIAN as IDA_CSS, b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER, 
                    ev.UNIT as IDC, (1 * ev.VALORISATION) as AMOUNT
                    from dbo.TRADE tr
                    inner join dbo.INSTRUMENT ns on  (ns.IDI = tr.IDI) and ({1})
                    inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'marginRequirement')
                    inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE = 'MGR') and (ev.EVENTCODE = 'LPC')
                    inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
                    inner join dbo.BOOK b on (b.IDB = ev.IDB_REC)
                    inner join dbo.{4} cba on (cba.IDA = b.IDA)
                    inner join dbo.ACTORROLE ar on (ar.IDA = cba.IDA) and (ar.IDROLEACTOR = 'MARGINREQOFFICE')
                    where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTENVIRONMENT = 'REGULAR') and 
                    (ev.IDA_PAY = @IDA_ENTITY) {2}" + Cst.CrLf;
                #endregion union all pour le Clearer
            }

            sql += ") amt" + Cst.CrLf;
            return String.Format(sql, hintsOracle, dtEnabledInstr, sqlCss, pCBQueryParam.CBActorDealerTable, pCBQueryParam.CBActorClearerTable);
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des deposits
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            if (pCBQueryParam.IdACss > 0)
            {
                queryParameters.Parameters.Add(DataParameter.GetParameter(pCBQueryParam.CS, DataParameter.ParameterEnum.IDA_CSS), pCBQueryParam.IdACss);
            }
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (Tuple&ltCBBookLeaf,CBBookTrade&gt)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            Tuple<CBBookLeaf, CBBookTrade> ret = default;
            if (null != Reader)
            {
                int idt = Convert.ToInt32(Reader["IDT"]);
                string identifier_t = Reader["T_IDENTIFIER"].ToString();
                DateTime dtSysDeposit = Convert.ToDateTime(Reader["DTSYS"]);
                int ida_css = Convert.ToInt32(Reader["IDA_CSS"]);
                StatusEnum status = CssCustodianStatus(ida_css);
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                //
                CBDetDeposit flow = new CBDetDeposit(idb, ida, amount, idc, dtSysDeposit, ida_css, status);
                //
                CBBookLeaf leaf = new CBBookLeaf(ida, idb, identifier_b, flow);
                //
                CBBookTrade trade = new CBBookTrade(idb, ida, idt, identifier_t, FlowType, status, DtBusiness);
                //
                ret = new Tuple<CBBookLeaf, CBBookTrade>(leaf, trade);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBDepositReader

    #region CBCollateralReader
    /// <summary>
    /// Permet la lecture des collaterals
    /// </summary>
    internal sealed class CBCollateralReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.Collateral;
        #endregion Constants

        #region Methods
        /// <summary>
        /// Requête de selection des collaterals
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20181119 PERF Correction post RC (Step 2)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {

            // FI 20180109 [24237] Modification de la requête => Usage de la restriction
            // and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE')
            // pour éviter les doublons 

            string sql = StrFunc.AppendFormat(@"
select amt.IDA, amt.IDB, b.IDENTIFIER as B_IDENTIFIER, amt.IDA_CSS, amt.DTBUSINESS,
       amt.IDPOSCOLLATERAL, amt.HAIRCUTFORCED,
       amt.ASSETCATEGORY, amt.IDASSET,
       amt.IDPOSCOLLATERALVAL, amt.QTYVAL,
       amt.IDC, amt.AMOUNT
 from (
        select pc.IDA_PAY as IDA, pc.IDB_PAY as IDB, pc.IDA_CSS, pc.DTBUSINESS, 
             pc.IDPOSCOLLATERAL, pc.HAIRCUTFORCED, 
             pc.ASSETCATEGORY, pc.IDASSET,
             pcval_last.IDPOSCOLLATERALVAL, pcval_last.QTY as QTYVAL,
             pcval_last.IDC, (-1 * pcval_last.VALORISATION) as AMOUNT
        from dbo.VW_COLLATERALPOS pc
        inner join dbo.{0} cba on (cba.IDA = pc.IDA_PAY)
        left outer join dbo.POSCOLLATERALVAL pcval_last on (pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = @DTBUSINESS)
        where (pc.DTBUSINESS <= @DTBUSINESS)
            and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >= @DTBUSINESS))
            and (pc.IDA_REC = @IDA_ENTITY)
            and (pc.ASSETCATEGORY != 'Cash')
        and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE')))        
        union all 
        select pc.IDA_PAY as IDA, pc.IDB_PAY as IDB, pc.IDA_CSS, pc.DTBUSINESS, 
             pc.IDPOSCOLLATERAL, pc.HAIRCUTFORCED,
             pc.ASSETCATEGORY, pc.IDASSET,
             pcval_last.IDPOSCOLLATERALVAL, pcval_last.QTY as QTYVAL,
             pcval_last.IDC, (-1 * pcval_last.VALORISATION) as AMOUNT
        from dbo.VW_COLLATERALPOS pc
        inner join dbo.{0} cba on (cba.IDA = pc.IDA_PAY)
        left outer join (select pcval.IDPOSCOLLATERAL, max(pcval.DTBUSINESS) as LAST_DTBUSINESS
                           from dbo.POSCOLLATERALVAL pcval
                          where (pcval.IDSTACTIVATION = 'REGULAR')
                            and (pcval.DTBUSINESS <= @DTBUSINESS)
                          group by pcval.IDPOSCOLLATERAL
                        ) pcval_last1 on (pcval_last1.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)
        left outer join ( select pcval.DTBUSINESS, pcval.IDPOSCOLLATERAL, pcval.IDPOSCOLLATERALVAL, pcval.IDC, pcval.QTY, pcval.VALORISATION
                            from dbo.POSCOLLATERALVAL pcval
                           where (pcval.IDSTACTIVATION = 'REGULAR')
                             and (pcval.DTBUSINESS <= @DTBUSINESS)
                        ) pcval_last on (pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = pcval_last1.LAST_DTBUSINESS)
         where (pc.DTBUSINESS <= @DTBUSINESS)
         and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >= @DTBUSINESS))
         and (pc.IDA_REC = @IDA_ENTITY)
         and (pc.ASSETCATEGORY = 'Cash')
         and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE')))
         union all 
         select pc.IDA_REC as IDA, pc.IDB_REC as IDB, pc.IDA_CSS, pc.DTBUSINESS, 
             pc.IDPOSCOLLATERAL, pc.HAIRCUTFORCED,
             pc.ASSETCATEGORY, pc.IDASSET,
             pcval_last.IDPOSCOLLATERALVAL, pcval_last.QTY as QTYVAL,
             pcval_last.IDC, (pcval_last.VALORISATION) as AMOUNT
        from dbo.VW_COLLATERALPOS pc
        inner join dbo.{1} cba on (cba.IDA = pc.IDA_REC)
        left outer join dbo.POSCOLLATERALVAL pcval_last on (pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = @DTBUSINESS)
        where (pc.DTBUSINESS <= @DTBUSINESS)
         and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >= @DTBUSINESS))
         and (pc.IDA_PAY = @IDA_ENTITY)
         and (pc.ASSETCATEGORY != 'Cash')
         and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE')))
        union all 
        select pc.IDA_REC as IDA, pc.IDB_REC as IDB, pc.IDA_CSS, pc.DTBUSINESS, 
             pc.IDPOSCOLLATERAL,  pc.HAIRCUTFORCED,
             pc.ASSETCATEGORY, pc.IDASSET,
             pcval_last.IDPOSCOLLATERALVAL, pcval_last.QTY as QTYVAL,
             pcval_last.IDC, (pcval_last.VALORISATION) as AMOUNT
        from dbo.VW_COLLATERALPOS pc
        inner join dbo.{1} cba on (cba.IDA = pc.IDA_REC)
        left outer join ( select pcval.IDPOSCOLLATERAL, max(pcval.DTBUSINESS) as LAST_DTBUSINESS
                            from dbo.POSCOLLATERALVAL pcval
                            where (pcval.IDSTACTIVATION = 'REGULAR') and (pcval.DTBUSINESS <= @DTBUSINESS)
                            group by pcval.IDPOSCOLLATERAL
                        ) pcval_last1 on (pcval_last1.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL)
        left outer join ( select pcval.DTBUSINESS, pcval.IDPOSCOLLATERAL, pcval.IDPOSCOLLATERALVAL, pcval.IDC, pcval.QTY, pcval.VALORISATION
                                 from dbo.POSCOLLATERALVAL pcval
                                 where (pcval.IDSTACTIVATION = 'REGULAR')and (pcval.DTBUSINESS <= @DTBUSINESS)
                        ) pcval_last on (pcval_last.IDPOSCOLLATERAL = pc.IDPOSCOLLATERAL) and (pcval_last.DTBUSINESS = pcval_last1.LAST_DTBUSINESS)
         where (pc.DTBUSINESS <= @DTBUSINESS)
         and ((pc.DTTERMINATION is null) or (pc.DTTERMINATION >= @DTBUSINESS))
         and (pc.IDA_PAY = @IDA_ENTITY)
         and (pc.ASSETCATEGORY = 'Cash')
         and exists (select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE')))
       ) amt
        inner join dbo.BOOK b on (b.IDB = amt.IDB)", pCBQueryParam.CBActorDealerTable, pCBQueryParam.CBActorClearerTable);

            return sql;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des collaterals
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            if (pCBQueryParam.IdACss > 0)
            {
                queryParameters.Parameters.Add(DataParameter.GetParameter(pCBQueryParam.CS, DataParameter.ParameterEnum.IDA_CSS), pCBQueryParam.IdACss);
            }
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                int ida_css = (Convert.IsDBNull(Reader["IDA_CSS"]) ? 0 : Convert.ToInt32(Reader["IDA_CSS"]));
                DateTime dtBusinessCollateral = Convert.ToDateTime(Reader["DTBUSINESS"]);
                decimal? qty = (Convert.IsDBNull(Reader["QTYVAL"]) ? (decimal?)null : Convert.ToDecimal(Reader["QTYVAL"]));
                decimal? haircutForced = (Convert.IsDBNull(Reader["HAIRCUTFORCED"]) ? (decimal?)null : Convert.ToDecimal(Reader["HAIRCUTFORCED"]));
                string assetCategory = (Convert.IsDBNull(Reader["ASSETCATEGORY"]) ? string.Empty : Reader["ASSETCATEGORY"].ToString());
                int idAsset = (Convert.IsDBNull(Reader["IDASSET"]) ? 0 : Convert.ToInt32(Reader["IDASSET"]));
                int idPoscollateral = Convert.ToInt32(Reader["IDPOSCOLLATERAL"]);
                int idPoscollateralVal = (Convert.IsDBNull(Reader["IDPOSCOLLATERALVAL"]) ? 0 : Convert.ToInt32(Reader["IDPOSCOLLATERALVAL"]));
                bool isValorised = (false == Convert.IsDBNull(Reader["IDPOSCOLLATERALVAL"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                //
                CBDetCollateral flow = new CBDetCollateral(idb, ida, amount, idc, dtBusinessCollateral, ida_css, isValorised, qty, assetCategory, idAsset, haircutForced, StatusEnum.Valid)
                {
                    idPoscollateral = idPoscollateral,
                    idPoscollateralVal = idPoscollateralVal
                };
                //
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBCollateralReader
    #region CBPaymentReader
    /// <summary>
    /// Permet la lecture des payments
    /// </summary>
    internal sealed class CBPaymentReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.Payment;
        #endregion Constants

        #region Methods
        /// <summary>
        /// Requête de selection des payments
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // RD 20200618 [25361] PERF (Use Temporary table based on TRADE CashPayment)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventPayment(), pCBQueryParam.CBTradeCashPaymentTable, hintsOracle, pCBQueryParam.CBEventClassTable);

            //PLA 20181012 [24248] Mise en place d'un where exists pour corriger pb de doublons
            string sqlSelect = @"select amt.IDT, amt.T_IDENTIFIER,
                amt.IDA, amt.IDB, amt.B_IDENTIFIER,
                amt.IDC, amt.AMOUNT
                from (
                    select /*+ ordered */
                    tr.IDT, tr.IDENTIFIER as T_IDENTIFIER,
                    b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER,
                    ev.UNIT as IDC, (-1 * ev.VALORISATION) as AMOUNT
                    from {0} ev
                    inner join dbo.{1} tr on (tr.IDT = ev.IDT)
                    inner join dbo.BOOK b on (b.IDB = ev.IDB_PAY)
                    inner join dbo.{2} cba on (cba.IDA = b.IDA)        
                    where (ev.IDA_REC = @IDA_ENTITY)
                    and exists(select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE','MARGINREQOFFICE')))
                    union all 
                    select /*+ ordered */
                    tr.IDT, tr.IDENTIFIER as T_IDENTIFIER,
                    b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER,
                    ev.UNIT as IDC, (1 * ev.VALORISATION) as AMOUNT
                    from {0} ev
                    inner join dbo.{1} tr on (tr.IDT = ev.IDT)
                    inner join dbo.BOOK b on (b.IDB = ev.IDB_REC)
                    inner join dbo.{2} cba on (cba.IDA = b.IDA)         
                    where (ev.IDA_PAY = @IDA_ENTITY)
                    and exists(select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE','MARGINREQOFFICE')))
                ) amt";

            sqlSelect = StrFunc.AppendFormat(@"with {0} as (
                {3}
            )
            " + sqlSelect, "eventflow", pCBQueryParam.CBTradeCashPaymentTable, pCBQueryParam.CBActorTable, sqlSubQuery);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des payments
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        /// EG|PM 20190328 [MIGRATION VCL] Correction Affectation FlowType sur CBBOokTrade
        public override object GetRowData()
        {
            Tuple<CBBookLeaf, CBBookTrade> ret = default;
            if (null != Reader)
            {
                int idt = Convert.ToInt32(Reader["IDT"]);
                string identifier_t = Reader["T_IDENTIFIER"].ToString();
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                //
                CBDetFlow flow = new CBDetFlow(idb, ida, amount, idc, FlowType, FlowSubTypeEnum.None, DtBusiness, StatusEnum.Valid);
                //
                CBBookLeaf leaf = new CBBookLeaf(ida, idb, identifier_b, flow);
                //
                CBBookTrade trade = new CBBookTrade(idb, ida, idt, identifier_t, FlowType, StatusEnum.Valid, DtBusiness);
                //
                ret = new Tuple<CBBookLeaf, CBBookTrade>(leaf, trade);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBPaymentReader
    #region CBSettlmentReader
    /// <summary>
    /// Permet la lecture des settlments
    /// </summary>
    internal sealed class CBSettlmentReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.SettlementPayment;
        #endregion Constants

        #region Methods
        /// <summary>
        /// Requête de selection des settlments
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            //PLA 20181012 [24248] Mise en place d'un where exists pour corriger pb de doublons
            string sqlSelect = @"select amt.IDT, amt.T_IDENTIFIER, amt.IDA, amt.IDB, amt.B_IDENTIFIER, amt.DTEVENT, amt.IDC, amt.AMOUNT
            from (
                select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER,
                ec.DTEVENT, ev.UNIT as IDC, (-1 * ev.VALORISATION) as AMOUNT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT inst on (inst.IDI = tr.IDI) and ({0})
                inner join dbo.PRODUCT pr on (pr.IDP = inst.IDP) and (pr.IDENTIFIER = 'cashPayment')
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'STA')
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT >= @DTBUSINESS) and (ec.EVENTCLASS = 'STL')
                inner join dbo.BOOK b on (b.IDB = ev.IDB_PAY)
                inner join dbo.{1} cba on (cba.IDA = b.IDA)
                where (ev.IDA_REC = @IDA_ENTITY) and (tr.IDSTACTIVATION = 'REGULAR') and 
                exists(select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE','MARGINREQOFFICE')))
            
                union all
            
                select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, b.IDA, b.IDB, b.IDENTIFIER as B_IDENTIFIER,
                ec.DTEVENT, ev.UNIT as IDC, (1 * ev.VALORISATION) as AMOUNT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT inst on (inst.IDI = tr.IDI) and ({0})
                inner join dbo.PRODUCT pr on (pr.IDP = inst.IDP) and (pr.IDENTIFIER = 'cashPayment')
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'STA')
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT >= @DTBUSINESS) and (ec.EVENTCLASS = 'STL')
                inner join dbo.BOOK b on (b.IDB = ev.IDB_REC)
                inner join dbo.{1} cba on (cba.IDA = b.IDA)
                where (ev.IDA_PAY = @IDA_ENTITY) and (tr.IDSTACTIVATION = 'REGULAR') and 
                exists(select 1 from dbo.ACTORROLE ar where (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE','MARGINREQOFFICE')))
            ) amt" + Cst.CrLf;

            string sqlDataDtEnabled = OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "inst", pCBQueryParam.DtBusiness);
            return String.Format(sqlSelect, sqlDataDtEnabled, pCBQueryParam.CBActorTable);
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des settlments
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        /// EG|PM 20190328 [MIGRATION VCL] Correction Affectation FlowType sur CBBOokTrade
        public override object GetRowData()
        {
            Tuple<CBBookLeaf, CBBookTrade> ret = default;
            if (null != Reader)
            {
                int idt = Convert.ToInt32(Reader["IDT"]);
                string identifier_t = Reader["T_IDENTIFIER"].ToString();
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                DateTime dtEvent = (Convert.IsDBNull(Reader["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTEVENT"]));
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                bool isForward = (dtEvent != _DtBusiness);
                //
                CBDetStlPayment flow = new CBDetStlPayment(idb, ida, amount, idc, FlowSubTypeEnum.None, dtEvent, isForward, StatusEnum.Valid);
                //
                CBBookLeaf leaf = new CBBookLeaf(ida, idb, identifier_b, flow);
                //
                CBBookTrade trade = new CBBookTrade(idb, ida, idt, identifier_t, FlowType, StatusEnum.Valid, _DtBusiness);
                //
                ret = new Tuple<CBBookLeaf, CBBookTrade>(leaf, trade);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBSettlmentReader
    #region CBLastCashBalanceReader
    /// <summary>
    /// Permet la lecture des CashBalance précédant
    /// </summary>
    /// EG|PM 20190328 [MIGRATION VCL] Override DtBusiness (voir CBReader pour virtuel)
    internal sealed class CBLastCashBalanceReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.LastCashBalance;
        #endregion Constants

        static DateTime _DtBusinessPrev;

        protected override DateTime DtBusiness
        {
            get { return _DtBusinessPrev; }
            set { _DtBusinessPrev = value; }
        }

        #region Methods
        /// <summary>
        /// Requête de selection des CashBalance précédant
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string sqlSelect = @"select amt.IDA, amt.IDB, b.IDENTIFIER as B_IDENTIFIER, amt.IDT, amt.T_IDENTIFIER,
            amt.DTEVENT, amt.AMOUNTSUBTYPE, amt.IDC, amt.AMOUNT
            from (
                select distinct tr.IDT, tr.IDENTIFIER as T_IDENTIFIER,
                ec.DTEVENT, ev.EVENTTYPE as AMOUNTSUBTYPE,
                case when ev.IDA_PAY = @IDA_ENTITY then ev.IDA_REC else ev.IDA_PAY end as IDA,
                case when ev.IDA_PAY = @IDA_ENTITY then ev.IDB_REC else ev.IDB_PAY end as IDB,
                ev.UNIT as IDC, case when ev.IDA_PAY = @IDA_ENTITY then 1 else -1 end * ev.VALORISATION as AMOUNT
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT inst on  (inst.IDI = tr.IDI) and ({0})
                inner join dbo.PRODUCT pr on (pr.IDP = inst.IDP) and (pr.IDENTIFIER = 'cashBalance')
                inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTTYPE in ('CSB', 'CSA', 'CSU', 'CLA', 'CLU', 'UMR', 'MGR'))
                inner join dbo.EVENT ev_parent on (ev_parent.IDE = ev.IDE_EVENT) and (ev_parent.EVENTCODE = 'CBS')
                inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.DTEVENT = @DTBUSINESS) and (ec.EVENTCLASS = 'REC')
                inner join dbo.{1} cba on (((cba.IDA = ev.IDA_PAY) and (ev.IDA_REC = @IDA_ENTITY)) or  ((cba.IDA = ev.IDA_REC) and (ev.IDA_PAY = @IDA_ENTITY)))
                inner join dbo.ACTORROLE ar on (ar.IDA = cba.IDA) and (ar.IDROLEACTOR in ('CSHBALANCEOFFICE', 'MARGINREQOFFICE'))
                where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR')
            ) amt
            inner join dbo.BOOK b on (b.IDB = amt.IDB)" + Cst.CrLf;
            return String.Format(sqlSelect,
                OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "inst", pCBQueryParam.DtBusiness), pCBQueryParam.CBActorTable);
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des CashBalance précédant
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int idt = Convert.ToInt32(Reader["IDT"]);
                string identifier_t = Reader["T_IDENTIFIER"].ToString();
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                DateTime dtEvent = (Convert.IsDBNull(Reader["DTEVENT"]) ? DateTime.MinValue : Convert.ToDateTime(Reader["DTEVENT"]));
                string flowSubTypeStr = (Convert.IsDBNull(Reader["AMOUNTSUBTYPE"]) ? string.Empty : Reader["AMOUNTSUBTYPE"].ToString());
                FlowSubTypeEnum flowSubType = ReflectionTools.ConvertStringToEnumOrDefault<FlowSubTypeEnum>(flowSubTypeStr, FlowSubTypeEnum.None);
                string idc = (Convert.IsDBNull(Reader["IDC"]) ? string.Empty : Reader["IDC"].ToString());
                decimal amount = (Convert.IsDBNull(Reader["AMOUNT"]) ? 0 : Convert.ToDecimal(Reader["AMOUNT"]));
                //
                CBDetLastFlow flow = new CBDetLastFlow(idb, ida, amount, idc, flowSubType, dtEvent, idt, identifier_t, StatusEnum.Valid);
                //
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBLastCashBalanceReader

    #region CBCurrentCashBalanceReader
    /// <summary>
    /// Permet la lecture des CashBalance courant afin d'être certain de les mettre à jour, même s'ils n'ont plus aucun flux
    /// </summary>
    // PM 20190909 [24826][24915] Add CBCurrentCashBalanceReader
    internal sealed class CBCurrentCashBalanceReader : CBReader
    {
        #region Constants
        /// <summary>
        /// Type de flux
        /// </summary>
        internal const FlowTypeEnum FlowType = FlowTypeEnum.CurrentCashBalance;
        #endregion Constants

        #region Methods
        /// <summary>
        /// Requête de selection des CashBalance du jour
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private static string QueryFlows(CBReadFlowParameters pCBQueryParam)
        {
            string sql = @"select tr.IDA_RISK as IDA,
            b.IDB, b.IDENTIFIER as B_IDENTIFIER,
            tr.IDT, tr.IDENTIFIER as T_IDENTIFIER
            from dbo.TRADE tr
            inner join dbo.INSTRUMENT inst on (inst.IDI = tr.IDI) and (" + OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "inst", pCBQueryParam.DtBusiness) + @")
            inner join dbo.PRODUCT pr on (pr.IDP = inst.IDP) and (pr.IDENTIFIER = 'cashBalance')
            inner join dbo.BOOK b on (b.IDB = tr.IDB_RISK)
            where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDA_ENTITY = @IDA_ENTITY)";
            return sql;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des CashBalance précédant
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlows(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            //
            return queryParameters;
        }
        #endregion ICBQueryParameters
        #region IReaderRow
        /// <summary>
        /// Lit un enregistrement à partir du IDataReader et le restitue sous forme d'objet (CBBookLeaf)
        /// </summary>
        /// <returns>Un objet représentant l'enregistrement lu</returns>
        public override object GetRowData()
        {
            CBBookLeaf ret = default;
            if (null != Reader)
            {
                int ida = Convert.ToInt32(Reader["IDA"]);
                int idb = (Convert.IsDBNull(Reader["IDB"]) ? 0 : Convert.ToInt32(Reader["IDB"]));
                string identifier_b = (Convert.IsDBNull(Reader["B_IDENTIFIER"]) ? string.Empty : Reader["B_IDENTIFIER"].ToString());
                CBDetFlow flow = new CBDetFlow(idb, ida, 0, string.Empty, FlowTypeEnum.CurrentCashBalance, FlowSubTypeEnum.CSB, DtBusiness, StatusEnum.Valid);
                ret = new CBBookLeaf(ida, idb, identifier_b, flow);
            }
            return ret;
        }
        #endregion IReaderRow
    }
    #endregion CBLastCashBalanceReader
    #endregion Class Flows Reader

    #region Class Trades Id Reader
    #region CBAllocFungible
    /// <summary>
    /// Permet la lecture des Trade Id des frais des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleFeeTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180709 PERF (Unused view VW_TRADE_FUNGIBLE : pb new INDEX on TRADE)
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFFEE", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
                group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id des taxes des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleTaxTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180709 PERF (Unused view VW_TRADE_FUNGIBLE : pb new INDEX on TRADE)
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeFungibleTable + Cst.CrLf, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFTAX", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
                group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id des CashFlows des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleCashTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des CashFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180709 PERF (Unused view VW_TRADE_FUNGIBLE : pb new INDEX on TRADE)        
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventCashFlow(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFCF", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
                group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des CashFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id des OtherFlows des allocations fongibles
    /// </summary>
    internal sealed class CBAllocFungibleOtherTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des OtherFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180709 PERF (Unused view VW_TRADE_FUNGIBLE : pb new INDEX on TRADE)
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            // FI 20190619 [24722] 
            //string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string hintsOracle = CBReader.IsCBAllocFungibleOtherFlowNoHints(pCBQueryParam) ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventOtherFlow(), pCBQueryParam.CBTradeFungibleTable, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "AFOF", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC))
            group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER, tr.IDB_DEALER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC))
                group by tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER, tr.IDB_CLEARER, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des OtherFlows
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }
    #endregion CBAllocFungible

    #region CBExecutedTrade
    /// <summary>
    /// Permet la lecture des Trade Id pour les frais des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeFeeTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeExecutedTable + Cst.CrLf, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXECFEE", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
            group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id pour les taxes des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeTaxTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeExecutedTable + Cst.CrLf, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXECTAX", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
            group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id pour les flux des trades execution
    /// </summary>
    internal sealed class CBExecutedTradeTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventExecutionTradeFlow(), pCBQueryParam.CBTradeExecutedTable + Cst.CrLf, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "EXEC", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeExecutedTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_BUYER) or (bk.IDB = tr.IDB_SELLER)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = bk.IDA)
            group by tr.IDA_CSSCUSTODIAN, bk.IDA, bk.IDB, tr.IDT, tr.IDENTIFIER, tr.DTBUSINESS" + Cst.CrLf;

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }
    #endregion CBExecutedTrade

    #region CBAllocNotFungible
    /// <summary>
    /// Permet la lecture des Trade Id pour les frais des trades non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleFeeTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventFee(), pCBQueryParam.CBTradeNotFungibleTable + Cst.CrLf, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANFFEE", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC)) and (" + Cst.CrLf;
            sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC)) and (" + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id pour les taxes des trades non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleTaxTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle1 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(evfeep IX_EVENTFEE) */";
            string hintsOracle2 = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) index(evfee IX_EVENTFEE) */";
            string sqlSubQuery = String.Format(SubQueryEventTax(), pCBQueryParam.CBTradeNotFungibleTable + Cst.CrLf, hintsOracle1, hintsOracle2, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANFTAX", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC)) and (" + Cst.CrLf;
            sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC)) and (" + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des taxes
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }

    /// <summary>
    /// Permet la lecture des Trade Id pour les flux des trades non fongibles
    /// </summary>
    internal sealed class CBAllocNotFungibleFlowTradeIdReader : CBTradeIdReader
    {
        #region Methods
        /// <summary>
        /// Requête de selection des trade id des flux
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd Hints (Oracle version < 12)
        // EG 20181119 PERF Correction post RC (Step 2)
        // RD 20200602 [25361] PERF (Use Temporary table based on EVENTCLASS)
        // RD 20200622 [25361] PERF (Use Temporary table based on EVENT Flow)
        private static string QueryFlowsTrade(CBReadFlowParameters pCBQueryParam)
        {
            string hintsOracle = pCBQueryParam.isNoHints ? string.Empty : "/*+ index(ev PK_EVENT) */";
            string sqlSubQuery = String.Format(SubQueryEventNotFungibleTradeFlow(), pCBQueryParam.CBTradeNotFungibleTable + Cst.CrLf, hintsOracle, pCBQueryParam.CBEventClassTable);
            string sqlSubQueryTable = pCBQueryParam.SetTableFlow("CBFLOW_MODEL", "ANF", sqlSubQuery);

            string sqlSelect = @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
            tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERDEALER as IDA, tr.IDB_DEALER as IDB 
            from {0} ev
            inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
            inner join dbo." + pCBQueryParam.CBActorDealerTable + @" cb on (cb.IDA = tr.IDA_OWNERDEALER)
            inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
            where (tr.IDA_ENTITYDEALER = @IDA_ENTITY) and ((tr.IDA_DEALER = ev.IDA_PAY) or (tr.IDA_DEALER = ev.IDA_REC)) and (" + Cst.CrLf;
            sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;

            if (pCBQueryParam.IsWithClearer)
            {
                sqlSelect += "union all" + Cst.CrLf;

                sqlSelect += @"select tr.IDT, tr.IDENTIFIER as T_IDENTIFIER, tr.DTBUSINESS as DTBUSINESS, 
                tr.IDA_CSSCUSTODIAN, tr.IDA_OWNERCLEARER as IDA, tr.IDB_CLEARER as IDB 
                from {0} ev
                inner join dbo." + pCBQueryParam.CBTradeNotFungibleTable + @" tr on (tr.IDT = ev.IDT)
                inner join dbo." + pCBQueryParam.CBActorClearerTable + @" cb on (cb.IDA = tr.IDA_OWNERCLEARER)
                inner join dbo.BOOK bk on (bk.IDB = tr.IDB_DEALER)
                where (tr.IDA_ENTITYCLEARER = @IDA_ENTITY) and ((tr.IDA_CLEARER = ev.IDA_PAY) or (tr.IDA_CLEARER = ev.IDA_REC)) and (" + Cst.CrLf;
                sqlSelect += OTCmlHelper.GetSQLDataDtEnabled(pCBQueryParam.CS, "bk", pCBQueryParam.DtBusiness) + ")" + Cst.CrLf;
            }

            sqlSelect = String.Format(sqlSelect, "dbo." + sqlSubQueryTable);

            return sqlSelect;
        }
        #endregion Methods

        #region ICBQueryParameters
        /// <summary>
        /// QueryParameters de selection des trade id des frais
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        public override QueryParameters GetQueryParameters(CBReadFlowParameters pCBQueryParam)
        {
            string sqlQuery = QueryFlowsTrade(pCBQueryParam);
            QueryParameters queryParameters = GetQueryParameters(sqlQuery, pCBQueryParam);
            return queryParameters;
        }
        #endregion ICBQueryParameters
    }
    #endregion CBAllocNotFungible
    #endregion Class Trades Id Reader

    /// <summary>
    /// Complément de la classe CashBalanceProcess pour gérer les lectures de données en asynchrone
    /// </summary>
    public partial class CashBalanceProcess : RiskPerformanceProcessBase
    {
        /// <summary>
        /// Chargement de tous les flux necessaires au calcul du cashbalance et filtrage sur les acteurs MRO et CBO
        /// <para> 1- Charger les montants de cash-flows des trades pour tous les acteurs de la hiérarchie</para>
        /// <para> 2- Filtrer uniquement les acteurs avec les rôles MRO ou bien CBO</para>
        /// <para> 3- Calculer le montant des cash-flows sur chaque noed et par devise</para>
        /// <para> 4- Charger les déposits pour les acteurs MRO</para>
        /// <para> 5- Charger les versements (signe "-") et les retraits (signe "+") pour les acteurs CBO/MRO</para>
        /// <para> 6- Charger les montants des collaterals pour les acteurs CBO/MRO</para>
        /// <para> 7- Charger les montants des soldes précédents et défaut de déposit précédent pour les acteurs CBO/MRO</para>
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void LoadFinancialFlowsAndFilterMroCbo()
        {

            List<Task> loadingFlowTask = new List<Task>();

            // Construire l'ensemble des CSS pour lesquels le traitement EOD est valide
            IEnumerable<int> idCssValid = m_CBHierarchy.cssCustodianEODValid.Select(x => x.OTCmlId);

            // RD 20200618 [25361] PERF (Use Temporary table based on TRADE CashPayment)
            // Initialize ReadFlowParameters
            CBReadFlowParameters cbReadFlowParam = InitializeReadFlowParameters(idCssValid);

            // 1- Charger les montants de cash-flows des trades pour tous les acteurs de la hiérarchie
            LoadFlowsFromBusinessTrade(cbReadFlowParam);

            #region Dump...
            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                // Hiérarchie de tous les acteurs avec cash-flows sauvegardée.
                SerializationHelper.DumpObjectToFile<CBHierarchy>((CBHierarchy)m_CBHierarchy, new SysMsgCode(SysCodeEnum.LOG, 1061),
                    AppInstance.AppRootFolder, "CB_HierarchyBrut_CashFlows.xml", null, ProcessState.AddCriticalException);
            }
            #endregion Dump...

            // 2- Filtrer uniquement les acteurs avec les rôles MRO ou bien CBO
            Common.AppInstance.TraceManager.TraceInformation(this, "Start Filter CBO MRO Actor");
            m_CBHierarchy.FilterCBOMROActor();
            Common.AppInstance.TraceManager.TraceInformation(this, "End Filter CBO MRO Actor");

            // 3- Calculer le montant des cash-flows sur chaque noed et par devise
            Common.AppInstance.TraceManager.TraceInformation(this, "Start Cumul Child Flows: CashFlows");
            m_CBHierarchy.CumulChildFlows(FlowTypeEnum.CashFlows);
            Common.AppInstance.TraceManager.TraceInformation(this, "End Cumul Child Flows: CashFlows");

            // 4- Charger les déposits pour les acteurs MRO
            bool isMroActorExist = m_CBHierarchy.IsExistRole(RoleActor.MARGINREQOFFICE);
            if (false == isMroActorExist)
            {

                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4002), 2,
                    new LogParam(m_CBHierarchy.Identifier_Entity),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness))));
            }
            else
            {
                Task depositTask = LoadFlowsDepositAsync(idCssValid);
                loadingFlowTask.Add(depositTask);
            }
            //
            bool isClearerCboMroActorExist = m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer);
            //
            // 5- Charger les versements (signe "-") et les retraits (signe "+") pour les acteurs CBO/MRO
            //
            if (m_CBHierarchy.IsExistActorWithCBMethod(CashBalanceCalculationMethodEnum.CSBDEFAULT))
            {
                // Uniquement s'il existe des acteurs utilisant la méthode par defaut (méthode 1) de calcul du cash balance
                Task paymentTask = LoadFlowsTupleAsync<CBPaymentReader>(CBPaymentReader.FlowType, cbReadFlowParam);
                loadingFlowTask.Add(paymentTask);
            }
            //
            if (m_CBHierarchy.IsExistActorWithCBMethod(CashBalanceCalculationMethodEnum.CSBUK))
            {
                // Uniquement s'il existe des acteurs utilisant la méthode UK (méthode 2) de calcul du cash balance
                Task settlmentTask = LoadFlowsTupleAsync<CBSettlmentReader>(CBSettlmentReader.FlowType, cbReadFlowParam);
                loadingFlowTask.Add(settlmentTask);
            }

            // 6- Charger les montants des collaterals pour les acteurs CBO/MRO
            //
            Task collateralTask = LoadFlowsCBBookLeafAsync<CBCollateralReader>(CBCollateralReader.FlowType, m_CBHierarchy.DtBusiness, idCssValid);
            loadingFlowTask.Add(collateralTask);

            // 7- Charger les montants des soldes précédents et défaut de déposit précédent pour les acteurs CBO/MRO
            //
            Task lastCashBalanceTask = LoadFlowsCBBookLeafAsync<CBLastCashBalanceReader>(CBLastCashBalanceReader.FlowType, m_CBHierarchy.DtBusinessPrev, idCssValid);
            loadingFlowTask.Add(lastCashBalanceTask);

            // 8- Charger les CashBalance (Trade Risk Id, Acteur, Book, Identifier) du jour afin d'être certain de les mettre à jour, même s'ils n'ont plus aucun flux
            // PM 20190909 [24826][24915] Ajout lecteur cash balance jour
            Task currentCashBalanceTask = LoadFlowsCBBookLeafAsync<CBCurrentCashBalanceReader>(CBCurrentCashBalanceReader.FlowType, m_CBHierarchy.DtBusiness, idCssValid);
            loadingFlowTask.Add(currentCashBalanceTask);

            // Attendre la fin de l'execution des taches de lecture des flux
            try
            {
                Task.WaitAll(loadingFlowTask.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }

            #region Dump...
            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                // Hiérarchie des acteurs CBO/MRO avec tous les flux sauvegardée.
                SerializationHelper.DumpObjectToFile<CBHierarchy>((CBHierarchy)m_CBHierarchy, new SysMsgCode(SysCodeEnum.LOG, 1062),
                    AppInstance.AppRootFolder, "CB_Hierarchy.xml", null, this.ProcessState.AddCriticalException);
            }
            #endregion Dump...
        }

        /// <summary>
        /// Convert FlowType to String
        /// </summary>
        /// <param name="pFlowType"></param>
        /// <returns></returns>
        /// // PM 20190909 [24826][24915] Ajout FlowTypeEnum.CurrentCashBalance
        private string ShortFlowTypeToString(FlowTypeEnum pFlowType)
        {
            string message = pFlowType.ToString().ToLower();
            switch (pFlowType)
            {
                case FlowTypeEnum.LastCashBalance:
                    message = "previous cashbalance";
                    break;
                case FlowTypeEnum.Payment:
                    message = "deposit/withdrawal";
                    break;
                case FlowTypeEnum.SettlementPayment:
                    message = "unsettled deposit/withdrawal";
                    break;
                case FlowTypeEnum.CurrentCashBalance:
                    message = "current cashbalance";
                    break;
            }
            return message;
        }

        /// <summary>
        /// Initialize flows loading
        /// </summary>
        /// <param name="pIdCssValid"></param>
        /// <returns></returns>
        // RD 20200618 [25361] Add
        // RD 20200629 [25361] PERF : Use tblCBTRADE_BUSINESS_Work table
        private CBReadFlowParameters InitializeReadFlowParameters(IEnumerable<int> pIdCssValid)
        {
            string logMessage = "Initializing flows loading in progress...";


            AppInstance.AppTraceManager.TraceInformation(this, logMessage);

            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            CBReadFlowParameters cbQueryParam = new CBReadFlowParameters(Cs, m_CBHierarchy.Ida_Entity, m_CBHierarchy.DtBusiness,
                m_CBHierarchy.TblCBACTOR_Work.First, m_CBHierarchy.TblCBACTOR_NOTCLEARER_Work.First, m_CBHierarchy.TblCBACTOR_CLEARER_Work.First, m_CBHierarchy.TblCBTRADE_BUSINESS_Work,
                m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer), pIdCssValid, Session.BuildTableId())
            {
                isNoHints = DataHelper.GetSvrInfoConnection(Cs).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(Cs).IsNoHints
            };
            
            string hintsOracleSettings = SystemSettings.GetAppSettings("CBReaderMode", "Default");
            switch (hintsOracleSettings)
            {
                case "Default":
                    break;
                case "NoHints":
                    cbQueryParam.isNoHints = true;
                    break;
                case "Hints":
                    cbQueryParam.isNoHints = false;
                    break;
                default:
                    throw new NotSupportedException(StrFunc.AppendFormat("hints value {0} is not supported", hintsOracleSettings));
            }

            cbQueryParam.InitializeQueryCBTrade();

            logMessage = "Initializing flows loading done.";

            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            AppInstance.AppTraceManager.TraceInformation(this, logMessage);

            return cbQueryParam;
        }

        /// <summary>
        /// Chargement des flux et des trade Ids concernant les trades
        /// </summary>
        /// <param name="pCBQueryParam"></param>
        // EG 20180906 PERF New (Use Temporary table based to model table)
        // EG 20181010 PERF Upd isOracle12OrHigher
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        // RD 20200618 [25361] PERF (Use Temporary table based on TRADE CashPayment)
        private void LoadFlowsFromBusinessTrade(CBReadFlowParameters pCBQueryParam)
        {
            // FI 20190619 [24722] Message "Loading trade flows in progress..." généré après cbQueryParam.InitializeQueryCBTrade
            string logMessage = "Loading trade flows in progress...";
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            // Lancement de toutes les tâches de lecture des flux sur les trades
            Task<List<CBBookLeaf>> allocFungibleFeeFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocFungibleFeeFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocFungibleTaxFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocFungibleTaxFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocFungibleCashFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocFungibleCashFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocFungibleOtherFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocFungibleOtherFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> executedTradeFeeFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBExecutedTradeFeeFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> executedTradeTaxFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBExecutedTradeTaxFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> executedTradeFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBExecutedTradeFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocNotFungibleFeeFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocNotFungibleFeeFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocNotFungibleTaxFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocNotFungibleTaxFlowReader>(pCBQueryParam);
            Task<List<CBBookLeaf>> allocNotFungibleFlow = CBQueryReader.ReadDataAsync<CBBookLeaf, CBAllocNotFungibleFlowReader>(pCBQueryParam);

            List<Task<List<CBBookLeaf>>> readFlowTasks = new List<Task<List<CBBookLeaf>>>
            {
                allocFungibleFeeFlow,
                allocFungibleTaxFlow,
                allocFungibleCashFlow,
                allocFungibleOtherFlow,
                executedTradeFeeFlow,
                executedTradeTaxFlow,
                executedTradeFlow,
                allocNotFungibleFeeFlow,
                allocNotFungibleTaxFlow,
                allocNotFungibleFlow
            };

            // Lancement des tâches de lecture des Id des trades concernés par les flux lorsque la tâche de flux correpondante est terminée
            Task<List<CBBookTrade>> allocFungibleFeeTradeId = default;
            Task<List<CBBookTrade>> allocFungibleTaxTradeId = default;
            Task<List<CBBookTrade>> allocFungibleCashTradeId = default;
            Task<List<CBBookTrade>> allocFungibleOtherTradeId = default;
            Task<List<CBBookTrade>> executedTradeFeeTradeId = default;
            Task<List<CBBookTrade>> executedTradeTaxTradeId = default;
            Task<List<CBBookTrade>> executedTradeFlowTradeId = default;
            Task<List<CBBookTrade>> allocNotFungibleFeeTradeId = default;
            Task<List<CBBookTrade>> allocNotFungibleTaxTradeId = default;
            Task<List<CBBookTrade>> allocNotFungibleFlowTradeId = default;

            List<CBBookLeaf> allFlow = new List<CBBookLeaf>();
            List<Task<List<CBBookTrade>>> readTradeTasks = new List<Task<List<CBBookTrade>>>();
            bool firstDone = false;
            while (readFlowTasks.Count > 0)
            {
                try
                {
                    Task<Task<List<CBBookLeaf>>> oneDoneTask = Task.WhenAny(readFlowTasks);
                    // Accés à Result => attend la fin de la tâche oneDoneTask
                    Task<List<CBBookLeaf>> oneDone = oneDoneTask.Result;
                    List<CBBookLeaf> flow = oneDone.Result;
                    // FI 20190620 [XXXXX] Afin de connaitre le temps d'execution des requêtes, ecriture dans le log même si la requête retourne 0 Ligne 
                    if (oneDone == allocFungibleFeeFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocFungibleFeeFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocFungibleTaxFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocFungibleTaxFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocFungibleCashFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocFungibleCashFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocFungibleOtherFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocFungibleOtherFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == executedTradeFeeFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("executedTradeFeeFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == executedTradeTaxFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("executedTradeTaxFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == executedTradeFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("executedTradeFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocNotFungibleFeeFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocNotFungibleFeeFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocNotFungibleTaxFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocNotFungibleTaxFlow RowCount:{0}", flow.Count.ToString()));
                    else if (oneDone == allocNotFungibleFlow)
                        Common.AppInstance.TraceManager.TraceInformation(this, string.Format("allocNotFungibleFlow RowCount:{0}", flow.Count.ToString()));

                    if (flow.Count > 0)
                    {
                        allFlow.AddRange(flow);
                        if (oneDone == allocFungibleFeeFlow)
                        {
                            allocFungibleFeeTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocFungibleFeeTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocFungibleFeeTradeId);
                        }
                        else if (oneDone == allocFungibleTaxFlow)
                        {
                            allocFungibleTaxTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocFungibleTaxTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocFungibleTaxTradeId);
                        }
                        else if (oneDone == allocFungibleCashFlow)
                        {
                            allocFungibleCashTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocFungibleCashTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocFungibleCashTradeId);
                        }
                        else if (oneDone == allocFungibleOtherFlow)
                        {
                            allocFungibleOtherTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocFungibleOtherTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocFungibleOtherTradeId);
                        }
                        else if (oneDone == executedTradeFeeFlow)
                        {
                            executedTradeFeeTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBExecutedTradeFeeTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(executedTradeFeeTradeId);
                        }
                        else if (oneDone == executedTradeTaxFlow)
                        {
                            executedTradeTaxTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBExecutedTradeTaxTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(executedTradeTaxTradeId);
                        }
                        else if (oneDone == executedTradeFlow)
                        {
                            executedTradeFlowTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBExecutedTradeTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(executedTradeFlowTradeId);
                        }
                        else if (oneDone == allocNotFungibleFeeFlow)
                        {
                            allocNotFungibleFeeTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocNotFungibleFeeTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocNotFungibleFeeTradeId);
                        }
                        else if (oneDone == allocNotFungibleTaxFlow)
                        {
                            allocNotFungibleTaxTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocNotFungibleTaxTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocNotFungibleTaxTradeId);
                        }
                        else if (oneDone == allocNotFungibleFlow)
                        {
                            allocNotFungibleFlowTradeId = CBQueryReader.ReadDataAsync<CBBookTrade, CBAllocNotFungibleFlowTradeIdReader>(pCBQueryParam);
                            readTradeTasks.Add(allocNotFungibleFlowTradeId);
                        }
                    }

                    readFlowTasks.Remove(oneDone);
                }
                catch (AggregateException ae)
                {
                    throw ae.Flatten();
                }
                // Affichage d'un message lorsque la première tâche de lecture des trade Id démarre
                if ((false == firstDone) && (1 == readTradeTasks.Count))
                {
                    firstDone = true;
                    logMessage = "Loading trade references in progress...";

                    Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
                    Logger.Write();
                }
            }
            // Ajout des flux à la CBHierarchy
            IEnumerable<CBBookLeaf> allBookLeaf = allFlow.Where(l => l != default);
            //
            foreach (CBBookLeaf bookLeaf in allBookLeaf)
            {
                m_CBHierarchy.Add(bookLeaf);
            }
            //
            logMessage = "Loading trade flows done.";

            AppInstance.AppTraceManager.TraceInformation(this, string.Format("Trade Flow RowCount:{0}", allFlow.Count.ToString()));


            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            //
            // Attente tâche de lecture des trade Id
            try
            {
                Task.WaitAll(readTradeTasks.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
            //
            // Ajout des Id des trades à la CBHierarchy
            // RD 20190311 [24577][24600] Ajouter l'acteur et le book dans le test d'égalité de la méthode Distinct
            //List<CBBookTrade> allTradeId = readTradeTasks.Select(r => r.Result).Aggregate((a, b) => { a.AddRange(b); return a; }).Distinct((a, b) => a.IDT == b.IDT, c => c.IDT.GetHashCode()).ToList();
            // FI 20190619 [24722]  Message déplacé avant la boucle de chargement de m_CBHierarchy
            logMessage = "Loading trade references done.";
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            if (0 < readTradeTasks.Count)
            {
                List<CBBookTrade> allTradeId = readTradeTasks.Select(r => r.Result).Aggregate((a, b) => { a.AddRange(b); return a; }).Distinct((a, b) => (a.IDT == b.IDT) && (a.IDA == b.IDA) && (a.IDB == b.IDB), c => c.IDT.GetHashCode()).ToList();
                Common.AppInstance.TraceManager.TraceInformation(this, string.Format("Trade Id RowCount:{0}", allTradeId.Count.ToString()));
                foreach (CBBookTrade bookTrade in allTradeId)
                {
                    m_CBHierarchy.Add(bookTrade);
                }
            }

            // Positionner l'indicateur de flux chargé
            m_CBHierarchy.SetFlowLoaded(FlowTypeEnum.CashFlows);
        }

        /// <summary>
        /// Chargement des flux et des trade Ids concernant les deposits
        /// </summary>
        /// <param name="pIdCssValid"></param>
        /// <returns></returns>
        // EG 20181010 PERF Upd isOracle12OrHigher
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private async Task LoadFlowsDepositAsync(IEnumerable<int> pIdCssValid)
        {
            string logMessage = "Loading initial margin flows and references in progress...";


            AppInstance.AppTraceManager.TraceInformation(this, logMessage);


            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            //
            CBReadFlowParameters cbQueryParam;
            Task<List<Tuple<CBBookLeaf, CBBookTrade>>> depositFlow;
            List<Task<List<Tuple<CBBookLeaf, CBBookTrade>>>> allDepositTask = new List<Task<List<Tuple<CBBookLeaf, CBBookTrade>>>>();
            bool isClearerMroActorExist = m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer, RoleActor.MARGINREQOFFICE);
            //
            foreach (Pair<int, DateTime> rowCss in m_CBHierarchy.CssUsingPreviousDeposit)
            {
                // Chargement des Déposits:
                // - pour les chambres de compensation sur lesquelles la date de compensation du traitement est fériée
                // - pour la dernière date de compensation ouvrée, avant la date de compensation du traitement, sur la chambre en question
                //
                cbQueryParam = new CBReadFlowParameters(Cs, m_CBHierarchy.Ida_Entity, rowCss.Second,
                    m_CBHierarchy.TblCBACTOR_Work.First, m_CBHierarchy.TblCBACTOR_NOTCLEARER_Work.First,
                    m_CBHierarchy.TblCBACTOR_CLEARER_Work.First, m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer), pIdCssValid, rowCss.First)
                {
                    isNoHints = DataHelper.GetSvrInfoConnection(Cs).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(Cs).IsNoHints
                };
                //
                depositFlow = CBQueryReader.ReadDataAsync<Tuple<CBBookLeaf, CBBookTrade>, CBDepositReader>(cbQueryParam);
                allDepositTask.Add(depositFlow);
            }
            //
            // Chargement des Déposits:
            // - pour toutes les chambres de compensation
            // - pour la date de compensation du traitement
            cbQueryParam = new CBReadFlowParameters(Cs, m_CBHierarchy.Ida_Entity, m_CBHierarchy.DtBusiness,
                m_CBHierarchy.TblCBACTOR_Work.First, m_CBHierarchy.TblCBACTOR_NOTCLEARER_Work.First,
                m_CBHierarchy.TblCBACTOR_CLEARER_Work.First, m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer), pIdCssValid)
            {
                isNoHints = DataHelper.GetSvrInfoConnection(Cs).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(Cs).IsNoHints
            };
            //
            depositFlow = CBQueryReader.ReadDataAsync<Tuple<CBBookLeaf, CBBookTrade>, CBDepositReader>(cbQueryParam);
            allDepositTask.Add(depositFlow);
            //
            // Attente tâche de lecture des deposits
            List<Tuple<CBBookLeaf, CBBookTrade>>[] allTupleArray;
            try
            {
                allTupleArray = await Task.WhenAll(allDepositTask);
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }
            //
            if (allTupleArray != default(List<Tuple<CBBookLeaf, CBBookTrade>>[]))
            {
                // Aggregation de tous les Tuples
                List<Tuple<CBBookLeaf, CBBookTrade>> allTuple = allTupleArray.Aggregate((a, b) => { a.AddRange(b); return a; });
                //
                // PM 20190111 [24442] Ajout d'une section critique pour les accés à m_CBHierarchy
                lock (m_CBHierarchy)
                {
                    // Ajout des flux à la CBHierarchy
                    IEnumerable<CBBookLeaf> allBookLeaf = allTuple.Select(t => t.Item1);
                    //
                    foreach (CBBookLeaf bookLeaf in allBookLeaf)
                    {
                        m_CBHierarchy.Add(bookLeaf);
                    }
                    //
                    // Ajout des Id des trades à la CBHierarchy
                    IEnumerable<CBBookTrade> allTradeId = allTuple.Select(t => t.Item2);
                    //
                    foreach (CBBookTrade bookTrade in allTradeId)
                    {
                        m_CBHierarchy.Add(bookTrade);
                    }
                    // Positionner l'indicateur de flux chargé
                    m_CBHierarchy.SetFlowLoaded(FlowTypeEnum.Deposit);
                    //
                    // Calculer le montant des deposits sur chaque noeud et par devise
                    m_CBHierarchy.CumulChildFlows(FlowTypeEnum.Deposit);
                }
                Common.AppInstance.TraceManager.TraceInformation(this, string.Format("depositFlow RowCount:{0}", allTuple.Count.ToString()));
            }
            //
            logMessage = "Loading initial margin flows and references done.";
            
            AppInstance.AppTraceManager.TraceInformation(this, logMessage);


            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();
        }

        /// <summary>
        /// Chargement des flux en fonction du type de flux
        /// </summary>
        /// <param name="pFlowType"></param>
        /// <param name="pDtBusiness"></param>
        /// <param name="pIdCssValid"></param>
        /// <returns></returns>
        // EG 20181010 PERF Upd isOracle12OrHigher
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private async Task LoadFlowsCBBookLeafAsync<TReader>(FlowTypeEnum pFlowType, DateTime pDtBusiness, IEnumerable<int> pIdCssValid) where TReader : IReaderRow, ICBQueryReaderParameters, new()
        {
            string flowTypeToString = ShortFlowTypeToString(pFlowType);

            string logMessage = String.Format("Loading {0} flows in progress...", flowTypeToString);

            AppInstance.AppTraceManager.TraceInformation(this, logMessage);
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();


            CBReadFlowParameters cbQueryParam = new CBReadFlowParameters(Cs, m_CBHierarchy.Ida_Entity, pDtBusiness,
                m_CBHierarchy.TblCBACTOR_Work.First, m_CBHierarchy.TblCBACTOR_NOTCLEARER_Work.First,
                m_CBHierarchy.TblCBACTOR_CLEARER_Work.First, m_CBHierarchy.IsExistRole(CBHierarchy.RolesClearer), pIdCssValid)
            {
                isNoHints = DataHelper.GetSvrInfoConnection(Cs).IsOraDBVer12cR1OrHigher || DataHelper.GetSvrInfoConnection(Cs).IsNoHints
            };
            //
            List<CBBookLeaf> allBookLeaf = await CBQueryReader.ReadDataAsync<CBBookLeaf, TReader>(cbQueryParam);
            //
            if (allBookLeaf != default(List<CBBookLeaf>))
            {
                if (allBookLeaf.Count > 0)
                {
                    // PM 20190111 [24442] Ajout d'une section critique pour les accés à m_CBHierarchy
                    lock (m_CBHierarchy)
                    {
                        foreach (CBBookLeaf bookLeaf in allBookLeaf)
                        {
                            m_CBHierarchy.Add(bookLeaf);
                        }
                        // Positionner l'indicateur de flux chargé
                        m_CBHierarchy.SetFlowLoaded(pFlowType);
                        // Calculer les montants sur chaque noeud et par devise
                        m_CBHierarchy.CumulChildFlows(pFlowType);
                    }
                }
                AppInstance.AppTraceManager.TraceInformation(this, string.Format("{0} RowCount:{1}", flowTypeToString, allBookLeaf.Count.ToString()));
            }
            logMessage = String.Format("Loading {0} flows done.", flowTypeToString);

            AppInstance.AppTraceManager.TraceInformation(this, logMessage);
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();
        }

        /// <summary>
        /// Chargement des flux et des trade ids en fonction du type de flux
        /// </summary>
        /// <param name="pFlowType"></param>
        /// <param name="pCBQueryParam"></param>
        /// <returns></returns>
        // EG 20181010 PERF Upd isOracle12OrHigher
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private async Task LoadFlowsTupleAsync<TReader>(FlowTypeEnum pFlowType, CBReadFlowParameters pCBQueryParam) where TReader : IReaderRow, ICBQueryReaderParameters, new()
        {
            string flowTypeToString = ShortFlowTypeToString(pFlowType);
            string logMessage = String.Format("Loading {0} flows and references in progress...", flowTypeToString);

            AppInstance.AppTraceManager.TraceInformation(this, logMessage);
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();

            List<Tuple<CBBookLeaf, CBBookTrade>> allTuple = await CBQueryReader.ReadDataAsync<Tuple<CBBookLeaf, CBBookTrade>, TReader>(pCBQueryParam);

            if (allTuple != default(List<Tuple<CBBookLeaf, CBBookTrade>>))
            {
                if (allTuple.Count > 0)
                {
                    // PM 20190111 [24442] Ajout d'une section critique pour les accés à m_CBHierarchy
                    lock (m_CBHierarchy)
                    {
                        // Ajout des flux à la CBHierarchy
                        IEnumerable<CBBookLeaf> allBookLeaf = allTuple.Select(t => t.Item1);
                        //
                        foreach (CBBookLeaf bookLeaf in allBookLeaf)
                        {
                            m_CBHierarchy.Add(bookLeaf);
                        }
                        //
                        // Ajout des Id des trades à la CBHierarchy
                        IEnumerable<CBBookTrade> allTradeId = allTuple.Select(t => t.Item2);
                        //
                        foreach (CBBookTrade bookTrade in allTradeId)
                        {
                            m_CBHierarchy.Add(bookTrade);
                        }
                        // Positionner l'indicateur de flux chargé
                        m_CBHierarchy.SetFlowLoaded(pFlowType);
                        // Calculer les montants sur chaque noeud et par devise
                        m_CBHierarchy.CumulChildFlows(pFlowType);
                    }
                }
                AppInstance.AppTraceManager.TraceInformation(this, string.Format("{0} RowCount:{1}", flowTypeToString, allTuple.Count.ToString()));
            }
            logMessage = String.Format("Loading {0} flows and references done.", flowTypeToString);

            AppInstance.AppTraceManager.TraceInformation(this, logMessage);
            Logger.Log(new LoggerData(LogLevelEnum.None, logMessage, 2));
            Logger.Write();
        }
    }
}
