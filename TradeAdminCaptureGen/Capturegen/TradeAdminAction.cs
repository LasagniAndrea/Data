using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Status;


namespace EFS.TradeInformation
{
    public sealed partial class TradeAdminCaptureGen
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataTable"></param>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        public Cst.ErrLevelMessage InvoicingCancellation(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pData)
        {
            return InvoicingSimulationAction(Cst.ProcessTypeEnum.INVOICINGCANCELLATION, pCS, pDataTable, pParameters);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataTable"></param>
        /// <param name="pParameters"></param>
        /// <returns></returns>
        public Cst.ErrLevelMessage InvoicingValidation(string pCS, DataTable pDataTable, MQueueparameters pParameters, params string[] pData)
        {
            return InvoicingSimulationAction(Cst.ProcessTypeEnum.INVOICINGVALIDATION, pCS, pDataTable, pParameters);
        }

        /// <summary>
        /// Méthode commune de traitement de validation/suppression de factures simulation
        /// </summary>
        /// <param name="pProcessType">Cst.ProcessTypeEnum.INVOICINGCANCELLATION|Cst.ProcessTypeEnum.INVOICINGVALIDATION</param>
        /// <param name="pCS">Connexion</param>
        /// <param name="pDataTable">Datarow matérialisant les factures simulées candidates à traitement</param>
        /// <param name="pParameters">Valuer des customObjects de l'interface de traitement (DATE DE FACTURATION et ENTITE)</param>
        /// <returns></returns>
        /// EG 20140826 New
        /// EG 20150115 [20683]
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static Cst.ErrLevelMessage InvoicingSimulationAction(Cst.ProcessTypeEnum pProcessType, string pCS, DataTable pDataTable, MQueueparameters pParameters)
        {
            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            // FI 20200820 [XXXXXX] dates systèmes en UTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(pCS);

            #region Lock Process
            bool isLockSuccessfulProcess = false;
            Lock lckExistingProcess = null;
            TypeLockEnum? _typeLock;
            IdMenu.Menu? _menu;
            switch (pProcessType)
            {
                case Cst.ProcessTypeEnum.INVOICINGCANCELLATION:
                    _typeLock = TypeLockEnum.OTC_INV_BROFEE_PROCESS_CANCELLATION;
                    _menu = IdMenu.Menu.InvoicingCancellation;
                    break;
                case Cst.ProcessTypeEnum.INVOICINGVALIDATION:
                    _typeLock = TypeLockEnum.OTC_INV_BROFEE_PROCESS_VALIDATION;
                    _menu = IdMenu.Menu.InvoicingValidation;
                    break;
                default:
                    throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, "Process not managed", ErrorLevel.FAILURE);
            }
            LockObject lockProcess = new LockObject(_typeLock.Value);
            if (null != lockProcess)
            {
                Lock lck = new Lock(pCS, lockProcess, SessionTools.AppSession, IdMenu.GetIdMenu(_menu.Value));
                isLockSuccessfulProcess = LockTools.LockMode1(lck, out lckExistingProcess);
            }
            #endregion Lock Process

            if (isLockSuccessfulProcess)
            {
                Tuple<Tracker, ProcessLog> log = InitInvoicingTrackerAndLog(pCS, pProcessType, SessionTools.AppSession, _menu.Value, pParameters);
                
                CaptureSessionInfo sessionInfo = new CaptureSessionInfo
                {
                    user = SessionTools.User,
                    session = SessionTools.AppSession,
                    licence = SessionTools.License,
                    idTracker_L = log.Item1.IdTRK_L,
                    idProcess_L = log.Item2.header.IdProcess
                };

                try
                {
                    #region Boucle sur Factures
                    foreach (DataRow row in pDataTable.Rows)
                    {
                        LockObject lockTrade = null;
                        IDbTransaction dbTransaction = null;
                        bool isLockSuccessfulTrade = false;
                        int idT = Convert.ToInt32(row["IDT"]);
                        string identifier = row["IDENTIFIER"].ToString();
                        string newIdentifier = string.Empty;
                        try
                        {
                            #region Lock Trade
                            lockTrade = new LockObject(TypeLockEnum.TRADE, idT, identifier, LockTools.Exclusive);
                            Lock lck = new Lock(pCS, lockTrade, sessionInfo.session, Cst.Capture.GetLabel(Cst.Capture.ModeEnum.Update));
                            isLockSuccessfulTrade = LockTools.LockMode1(lck, out Lock lckExistingTrade);
                            if ((false == isLockSuccessfulTrade) && (null != lckExistingTrade))
                                throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, lckExistingTrade.ToString(), ErrorLevel.LOCK_ERROR);
                            #endregion Lock Trade

                            #region START Transaction (Begin Tran)
                            //Begin Tran  doit être la 1er instruction Car si Error un  rollback est fait de manière systematique
                            try { dbTransaction = DataHelper.BeginTran(pCS); }
                            catch (Exception ex) { throw new TradeCommonCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.BEGINTRANSACTION_ERROR); }
                            #endregion START Transaction (Begin Tran)

                            try
                            {
                                switch (pProcessType)
                                {
                                    case Cst.ProcessTypeEnum.INVOICINGCANCELLATION:
                                        #region DELETE
                                        // EG 20150115 [20683]
                                        // FI 20160524 [XXXXX] passage du paramètre  Cst.ProductGProduct_ADM
                                        // FI 20160816 [22146] passage des paramètres idA, pDateSys
                                        TradeRDBMSTools.DeleteEvent(pCS, dbTransaction, idT, Cst.ProductGProduct_ADM, sessionInfo.user.IdA, dtSys);
                                        DataParameters parameters = new DataParameters();
                                        parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), idT);
                                        string sqlDelete = SQLCst.SQL_ANSI + Cst.CrLf + "delete from dbo.TRADE where (IDT = @IDT);" + Cst.CrLf;
                                        QueryParameters qryParameters = new QueryParameters(pCS, sqlDelete, parameters);
                                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                        #endregion DELETE
                                        break;
                                    case Cst.ProcessTypeEnum.INVOICINGVALIDATION:
                                        int idI = Convert.ToInt32(row["IDI"]);
                                        int idA_Entity = Convert.ToInt32(row["IDA_ENTITY"]);
                                        DateTime dtTrade = Convert.ToDateTime(row["DTTRADE"]);
                                        string screenName = row["SCREENNAME"].ToString();

                                        #region TRADE STATUS ENVIRONMENT
                                        try { UpdateTradeAdminStEnvironment(pCS, dbTransaction, idT, sessionInfo, dtSys); }
                                        catch (Exception ex) { throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.EDIT_TRADESTATUS_ERROR); }
                                        #endregion TRADE STATUS ENVIRONMENT

                                        #region TRADE / TRADELINK
                                        try
                                        {
                                            TradeStatus tradeStatus = new TradeStatus
                                            {
                                                stEnvironmentSpecified = true,
                                                stPrioritySpecified = (false == Convert.IsDBNull(row["IDSTPRIORITY"])),
                                                stActivationSpecified = (false == Convert.IsDBNull(row["IDSTACTIVATION"]))
                                            };
                                            tradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                                            if (tradeStatus.stPrioritySpecified)
                                                tradeStatus.stPriority.CurrentSt = row["IDSTPRIORITY"].ToString();
                                            if (tradeStatus.stActivationSpecified)
                                                tradeStatus.stActivation.CurrentSt = row["IDSTACTIVATION"].ToString();
                                            newIdentifier = UpdateTradeIdentifier(pCS, dbTransaction, idT, idI, idA_Entity, dtTrade, tradeStatus);
                                            UpdateTradeLink(pCS, dbTransaction, idT, identifier, newIdentifier);
                                        }
                                        catch (Exception ex) { throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.UPD_TRADEIDENTIFIER_ERROR); }
                                        #endregion TRADE / TRADELINK

                                        #region TRADETRAIL
                                        try { SaveTradeTrail(pCS, dbTransaction, idT, sessionInfo, screenName, Cst.Capture.ModeEnum.Update, dtSys); }
                                        catch (Exception ex) { throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.ADD_TRADETRAIL); }
                                        #endregion TRADETRAIL

                                        break;
                                }

                            }
                            catch (Exception ex) { throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.FAILURE); }

                            #region END Transaction (Commit/Rollback)
                            try { DataHelper.CommitTran(dbTransaction); }
                            catch (Exception ex) { throw new TradeAdminCaptureGenException(MethodInfo.GetCurrentMethod().Name, ex, ErrorLevel.COMMIT_ERROR); }
                            #endregion END Transaction (Commit/Rollback)

                            switch (pProcessType)
                            {
                                case Cst.ProcessTypeEnum.INVOICINGCANCELLATION:
                                    if (log.Item2 != default(ProcessLog))
                                    {
                                        log.Item2.AddDetail(new string[] { "LOG-05246", LogTools.IdentifierAndId(identifier, idT) }, ProcessStateTools.StatusNoneEnum);
                                        log.Item2.SQLWriteDetail();
                                    }
                                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5246), 0,
                                        new LogParam(LogTools.IdentifierAndId(identifier, idT))));
                                    Logger.Write();

                                    break;
                                case Cst.ProcessTypeEnum.INVOICINGVALIDATION:
                                    if (log.Item2 != default(ProcessLog))
                                    {
                                        log.Item2.AddDetail(new string[] { "LOG-05245", LogTools.IdentifierAndId(identifier, idT),
                                        LogTools.IdentifierAndId(newIdentifier, idT) }, ProcessStateTools.StatusNoneEnum);
                                        log.Item2.SQLWriteDetail();
                                    }
                                    // PM 20210121 [XXXXX] Passage du message au niveau de log None
                                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5245), 0,
                                        new LogParam(LogTools.IdentifierAndId(identifier, idT)),
                                        new LogParam(LogTools.IdentifierAndId(newIdentifier, idT))));
                                    Logger.Write();

                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            ret.ErrLevel = Cst.ErrLevel.FAILURE;
                            ret.Message += Cst.CrLf + ex.Message;
                            if (log.Item2 != default(ProcessLog))
                            {
                                log.Item2.AddDetail(new string[] { "LOG-05246", LogTools.IdentifierAndId(identifier, idT) }, ProcessStateTools.StatusNoneEnum);
                                log.Item2.AddDetail(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex));
                                log.Item2.SQLWriteDetail();
                            }
                            // PM 20210121 [XXXXX] Passage du message au niveau de log None
                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 5246), 0,
                                new LogParam(LogTools.IdentifierAndId(identifier, idT))));
                            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                            Logger.Write();

                            try { DataHelper.RollbackTran(dbTransaction); }
                            catch { }
                        }
                        finally
                        {
                            #region UnLock Trade
                            try
                            {
                                if (isLockSuccessfulTrade && (null != lockTrade))
                                    LockTools.UnLock(pCS, lockTrade, sessionInfo.session.SessionId);
                            }
                            catch
                            {
                                //Si ça plante tant pis le lock sera supprimer en fin de session
                            }
                            #endregion UnLock Trade
                        }
                    }
                    #endregion Boucle sur Factures
                }
                finally
                {
                    if (isLockSuccessfulProcess)
                    {
                        #region UnLock Process
                        try
                        {
                            if (null != lockProcess)
                                LockTools.UnLock(pCS, lockProcess, sessionInfo.session.SessionId);
                        }
                        catch
                        {
                            //Si ça plante tant pis le lock sera supprimer en fin de session
                        }
                        #endregion UnLock Process
                    }

                    log.Item1.ReadyState = ProcessStateTools.ReadyStateEnum.TERMINATED;
                    log.Item1.SetCounter((ret.ErrLevel == Cst.ErrLevel.SUCCESS ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum), 0, SessionTools.AppSession);
                }

            }
            else
            {
                string msg = "LOCKUNSUCCESSFUL";
                if (null != lckExistingProcess)
                    msg += StrFunc.AppendFormat(",Existing Lock is {0}", lckExistingProcess.ToString());
                ret = new Cst.ErrLevelMessage(Cst.ErrLevel.LOCKUNSUCCESSFUL, msg);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pNewIdentifier"></param>
        /// 20091124 EG Mise à jour de DATA1 et DATA2 (Remplacement de l'identifier SIMUL par l'identifier INVOICE) SIMUL ->DATA3 
        /// EG 20150716
        private static void UpdateTradeLink(string pCS, IDbTransaction pDbTransaction, int pIdT, string pIdentifier, string pNewIdentifier)
        {

            #region TradeLink
            SQL_TradeLink sqlTradeLink = new SQL_TradeLink(pCS, pIdT)
            {
                DbTransaction = pDbTransaction
            };
            string sqlQuery = sqlTradeLink.GetQueryParameters(
                    new string[] { "VW_TRADELINK.IDT_L", "IDT_A", "IDT_B", "DATA1", "DATA2" }).QueryReplaceParameters;

            // EG 20150716 pDbTransaction replace CS
            DataSet ds = DataHelper.ExecuteDataset(pCS, pDbTransaction, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];
            foreach (DataRow row in dt.Rows)
            {
                string sqlUpdate = string.Empty;
                if (pIdT == Convert.ToInt32(row["IDT_A"]))
                {
                    if ((false == Convert.IsDBNull(row["DATA1"])) && (pIdentifier == row["DATA1"].ToString()))
                        sqlUpdate += SQLCst.SET + "DATA1=" + DataHelper.SQLString(pNewIdentifier) + Cst.CrLf;
                }
                if (pIdT == Convert.ToInt32(row["IDT_B"]))
                {
                    if ((false == Convert.IsDBNull(row["DATA2"])) && (pIdentifier == row["DATA2"].ToString()))
                    {
                        sqlUpdate += StrFunc.IsEmpty(sqlUpdate) ? SQLCst.SET : ",";
                        sqlUpdate += "DATA2=" + DataHelper.SQLString(pNewIdentifier) + Cst.CrLf;
                    }
                }
                if (StrFunc.IsFilled(sqlUpdate))
                {
                    string identification = EFS.TradeLink.TradeLinkDataIdentification.OldIdentifier.ToString();
                    sqlUpdate += ", DATA3IDENT =" + DataHelper.SQLString(identification) + ", DATA3 = " + DataHelper.SQLString(pIdentifier);
                    sqlUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.TRADELINK + Cst.CrLf + sqlUpdate;
                    sqlUpdate += SQLCst.WHERE + "(IDT_L=" + row["IDT_L"].ToString() + ")" + Cst.CrLf;
                    sqlUpdate += SQLCst.AND + "(IDT_A=" + row["IDT_A"].ToString() + ")" + Cst.CrLf;
                    sqlUpdate += SQLCst.AND + "(IDT_B=" + row["IDT_B"].ToString() + ")";
                    _ = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlUpdate);
                }
            }
            #endregion TradeLink
        }

        /// <summary>
        /// Initialisation d'une ligne de tracker pour les actions sur Factures simulées (CANCELLATION|VALIDATION)
        /// </summary>
        /// <param name="pProcessType">Cst.ProcessTypeEnum.INVOICINGCANCELLATION|Cst.ProcessTypeEnum.INVOICINGVALIDATION</param>
        /// <param name="pAppSession">Instance</param>
        /// <param name="pMenu">Menu pour détection message dans tracker</param>
        /// <param name="pParameters">Valuer des customObjects de l'interface de traitement (DATE DE FACTURATION et ENTITE)</param>
        /// <returns></returns>
        /// EG 20140826 New
        private static Tuple<Tracker, ProcessLog> InitInvoicingTrackerAndLog(string pCS, Cst.ProcessTypeEnum pProcessType, AppSession pAppSession, IdMenu.Menu pMenu, MQueueparameters pParameters)
        {

            TrackerAttributes TrackerAttrib = new TrackerAttributes()
            {
                process = Cst.ProcessTypeEnum.INVOICINGGEN,
                gProduct = Cst.ProductGProduct_ADM,
                caller = IdMenu.GetIdMenu(pMenu),
                info = TrackerAttributes.BuildInfo(pProcessType, pParameters)
            };

            Tracker tracker = new Tracker(pCS)
            {
                ProcessRequested = Cst.ProcessTypeEnum.INVOICINGGEN,
                Status = ProcessStateTools.StatusEnum.PROGRESS,
                ReadyState = ProcessStateTools.ReadyStateEnum.ACTIVE,
                Group = TrackerAttrib.BuildTrackerGroup(),
                IdData = TrackerAttrib.BuildTrackerIdData(),
                Data = TrackerAttrib.BuildTrackerData()
            };

            tracker.Insert(pAppSession, 1);

            ProcessLog ProcessLog = new ProcessLog(pCS, pProcessType, pAppSession);
            ProcessLog.header.Info.message = Ressource.GetString(pProcessType.ToString());
            ProcessLog.header.IdTRK_L = tracker.IdTRK_L;
            ProcessLog.SQLWriteHeader();

            return new Tuple<Tracker, ProcessLog>(tracker, ProcessLog);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSessionInfo"></param>
        /// <param name="pDtSysBusiness"></param>
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static void UpdateTradeAdminStEnvironment(string pCS, IDbTransaction pDbTransaction, int pIdT, CaptureSessionInfo pSessionInfo, DateTime pDtSysBusiness)
        {
            SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(pCS, pIdT);
            string sqlQuery = sqlTradeStSys.GetQueryParameters(
                    new string[] { "IDT", "IDSTENVIRONMENT", "DTSTENVIRONMENT", "IDASTENVIRONMENT", "ROWATTRIBUT" }).QueryReplaceParameters;

            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sqlQuery);
            DataTable dt = ds.Tables[0];

            DataRow dr = dt.Rows[0];
            dr.BeginEdit();
            dr["IDSTENVIRONMENT"] = Cst.StatusEnvironment.REGULAR;
            dr["DTSTENVIRONMENT"] = pDtSysBusiness;
            dr["IDASTENVIRONMENT"] = pSessionInfo.session.IdA;
            dr.EndEdit();

            DataHelper.ExecuteDataAdapter(pDbTransaction, sqlQuery, dt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pIdI"></param>
        /// <param name="pIdA_Entity"></param>
        /// <param name="pDtTrade"></param>
        /// <param name="pTradeStatus"></param>
        /// <returns></returns>
        private static string UpdateTradeIdentifier(string pCS, IDbTransaction pDbTransaction, int pIdT, int pIdI, int pIdA_Entity, DateTime pDtTrade, TradeStatus pTradeStatus)
        {
            string methodName = "TradeAdminCaptureGen.UpdateTradeIdentifier";
            try
            {
                string prefix = string.Empty;
                string suffix = string.Empty;
                string idGetId = SQLUP.IdGetId.TRADE.ToString() + ":" + pTradeStatus.stEnvironment.CurrentSt;
                SQL_Instrument sql_Instrument = new SQL_Instrument(pCS, pIdI);
                TradeRDBMSTools.BuildTradeIdentifier(pCS, pDbTransaction, sql_Instrument, pIdA_Entity, pTradeStatus, pDtTrade, pDtTrade, out string newIdentifier, out prefix, out suffix);
                newIdentifier = prefix + newIdentifier + suffix;

                SQL_Actor sql_Actor = new SQL_Actor(pCS, pIdA_Entity);

                string sqlUpdate = string.Empty;
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdT);
                parameters.Add(new DataParameter(pCS, "NEWIDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), newIdentifier);

                sqlUpdate = @"update dbo.TRADE set IDENTIFIER=@NEWIDENTIFIER where (IDT=@IDT);";
                int nRow = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlUpdate, parameters.GetArrayDbParameter());

                if (DataHelper.IsDbSqlServer(pCS))
                {
                    string xQuery = OTCmlHelper.GetXMLNamespace_3_0(pCS);
                    parameters.Add(new DataParameter(pCS, "XMLID", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), sql_Actor.XmlId);
                    xQuery += @"replace value of (efs:EfsML/trade/tradeHeader/partyTradeIdentifier";
                    xQuery += @"[partyReference[@href=sql:variable(""@XMLID"")]]/tradeId[1]/text())[1] with sql:variable(""@NEWIDENTIFIER"")";

                    sqlUpdate = @"update dbo.TRADEXML set TRADEXML.modify('" + xQuery + "') where (IDT=@IDT)";
                }
                else if (DataHelper.IsDbOracle(pCS))
                {
                    sqlUpdate = @"update dbo.TRADEXML set TRADEXML=" + Cst.CrLf;
                    sqlUpdate += "UPDATEXML(TRADEXML,'efs:EfsML/fpml:trade/fpml:tradeHeader/fpml:partyTradeIdentifier";
                    sqlUpdate += @"[fpml:partyReference[@href=""" + sql_Actor.XmlId + @"""]]/fpml:tradeId[1]/text()','" + newIdentifier + @"',";
                    sqlUpdate += @"'xmlns:efs=""http://www.efs.org/2007/EFSmL-3-0"", xmlns:fpml=""http://www.fpml.org/2007/FpML-4-4""')";
                    sqlUpdate += " where (IDT=@IDT)";
                }
                nRow = DataHelper.ExecuteNonQuery(pDbTransaction, CommandType.Text, sqlUpdate, parameters.GetArrayDbParameter());

                return newIdentifier;
            }
            catch (TradeAdminCaptureGenException) { throw; }
            catch (Exception ex) { throw new TradeAdminCaptureGenException(methodName, ex, ErrorLevel.UPD_TRADEIDENTIFIER_ERROR); }
        }
    }
}

