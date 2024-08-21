#region Using Directives
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeInformation;
//
using EfsML;
using EfsML.Business;
using EfsML.ClosingReopeningPositions;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.PosRequest;
//
using FixML.Enum;
using FixML.v50SP1.Enum;
//
using FpML.Enum;
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    // EG 20180221 [23769] New pour gestion asynchrone
    // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
    public abstract partial class PosKeepingGenProcessBase
    {
        #region New Members
        /// <summary>
        /// Jeton courant de type POSREQUEST
        /// </summary>
        protected int m_IdPRAsync = 0;
        protected static object m_IdPRAsyncLock = new object();

        protected ConcurrentDictionary<string, int> m_CDicPUTIOk = null;

        private readonly static object m_AssetCacheLock = new object();
        // EG 20210707 [XXXXX] Variable de Lock pour le delete (si besoin) de POSREQUEST de livraison de sous-jacent
        protected static object m_UnderlyerDeliveryLock = new object();
        private readonly static object m_TemplateDataDocumentTradeLock = new object();
        #endregion New Members

        #region New Accessors
        public IProductBase Product { get { return m_Product; } }
        public PosKeepingGenProcess PKGenProcess { get { return m_PKGenProcess; } }
        public IPosRequest MasterPosRequest { get { return m_MasterPosRequest; } }
        public IPosRequest MarketPosRequest { get { return m_MarketPosRequest; } }
        public EventQuery EventQry { get { return m_EventQuery; } }

        public Hashtable TemplateDataDocumentTrade { get { return m_TemplateDataDocumentTrade; } }
        public object TemplateDataDocumentTradeLock { get { return m_TemplateDataDocumentTradeLock; } }
        #endregion New Accessors

        #region CacheAssetAddLock
        public void CacheAssetAddLock(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, PosKeepingAsset pPosKeepingAsset)
        {
            lock (m_AssetCacheLock)
            {
                if (null == m_AssetCache)
                    m_AssetCache = new Dictionary<CacheAsset, PosKeepingAsset>(new CacheAssetComparer());
                CacheAsset _key = new CacheAsset(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote);
                if (false == m_AssetCache.ContainsKey(_key))
                    m_AssetCache.Add(_key, pPosKeepingAsset);
            }
        }
        #endregion CacheAssetAddLock
        #region CacheAssetFindLock
        public PosKeepingAsset CacheAssetFindLock(Nullable<Cst.UnderlyingAsset> pUnderlyingAsset, int pIdAsset, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote)
        {
            lock (m_AssetCacheLock)
            {
                return CacheAssetFind(pUnderlyingAsset, pIdAsset, pPosRequestAssetQuote);
            }
        }
        #endregion CacheAssetFindLock
        /// <summary>
        /// Demande {pNumberOfToken} jetons de type POSREQUEST (Appel à  SQLUP.GetId)
        /// <para>Initilisation du jeton courant avec le 1er jeton obtenu</para>
        /// </summary>
        /// <param name="pNumberOfToken"></param>
        /// <returns></returns>
        /// FI 20200923 [XXXXX] Add
        protected int Initialize_IdPRAsync(int pNumberOfToken)
        {
            lock (m_IdPRAsyncLock)
            {
                int newIDPR;
                using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
                {
                    try
                    {
                        SQLUP.GetId(out newIDPR, dbTransaction, SQLUP.IdGetId.POSREQUEST, SQLUP.PosRetGetId.First, pNumberOfToken);
                        DataHelper.CommitTran(dbTransaction);
                        m_IdPRAsync = newIDPR;
                    }
                    catch (Exception)
                    {
                        DataHelper.RollbackTran(dbTransaction);
                        throw;
                    }
                };
                return newIDPR;
            }
        }


        /// <summary>
        /// Retourne le prochain jeton POSREQUEST (il devient du jeton courant)
        /// </summary>
        /// <returns></returns>
        public int Next_IdPRAsync()
        {
            lock (m_IdPRAsyncLock)
            {
                m_IdPRAsync++;
                return m_IdPRAsync;
            }
        }
        

        #region SetCurrentParallelSettings
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public void SetCurrentParallelSettings(string pEntity, string pCssCustodian, string pMarket)
        {
            m_PKGenProcess.CurrentParallelSettings = null;
            if (ParallelTools.GetParallelSection("parallelEndOfDay") is ParallelEndOfDaySection parallelSection)
                m_PKGenProcess.CurrentParallelSettings = parallelSection.GetParallelSettings(pEntity, pCssCustodian, pMarket);
        }
        #endregion SetCurrentParallelSettings

        /*  ----------------------------------------------*/
        /*             CALCUL DES MARGES                  */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/

        #region GetListQueueForEODCashFlowsAsync
        /// <summary>
        /// Construction d'une liste de messages EventsValMQueue pour traitement des Cash-Flows (VMG|UMG|LOV ...) pour une liste de trades
        /// Mode ASYNCHRONE
        /// </summary>
        /// <param name="pRows"></param>
        /// <returns></returns>
        // EG 20190114 New 
        private async Task<IEnumerable<EventsValMQueue>> GetListQueueForEODCashFlowsAsync(List<DataRow> pRows)
        {
            List<EventsValMQueue> lstEventsValMQueue = new List<EventsValMQueue>();
            List<Task<EventsValMQueue>> getReturnTasks = new List<Task<EventsValMQueue>>();

            try
            {
                IEnumerable<Task<EventsValMQueue>> getReturnTasksQuery =
                    from trade in pRows.AsEnumerable()
                    where (false == Convert.IsDBNull(trade["IDT"]))
                    select GetQueueForEODCashFlowsAsync(trade);

                getReturnTasks = getReturnTasksQuery.ToList();

                while (0 < getReturnTasks.Count)
                {
                    Task<EventsValMQueue> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "GetListQueueForEODCashFlowsAsync", null, true);

                    if (null != firstFinishedTask.Result)
                        lstEventsValMQueue.Add(firstFinishedTask.Result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                
                Logger.Write();
            }
            return lstEventsValMQueue;

        }
        #endregion GetListQueueForEODCashFlowsAsync
        #region GetQueueForEODCashFlowsAsync
        /// <summary>
        /// Tache unitaire pour génération d'un message EventsValMQueue pour traitement des Cash-Flows (VMG|UMG|LOV ...) pour un trade
        /// Mode ASYNCHRONE 
        /// </summary>
        /// <param name="pDataRow">DataRow Trade</param>
        /// <returns>EventsValMQueue</returns>
        // EG 20180205 [23769] New
        private async Task<EventsValMQueue> GetQueueForEODCashFlowsAsync(DataRow pDataRow)
        {
            EventsValMQueue _eventsValMQueue = await Task.Run(() => GetQueueForEODCashFlows(pDataRow));
            return _eventsValMQueue;
        }
        #endregion GetQueueForEODCashFlowsAsync
        #region CashFlowsGenByTrade
        /// <summary>
        /// Calcul et génération des cash-flows pour un trade (VMG, UMG, LOV, etc.) en mode ASYNCHRONE via SLAVE CALL SpheresEventsVal service
        /// </summary>
        /// <param name="pCallProcess">Process appelant</param>
        /// <param name="pProcessCacheContainer">Cache du process appelant</param> 
        /// <param name="pEventsValMQueue">Queue</param>
        /// <param name="pRow">Trade</param>
        /// <returns></returns>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190613 [24683] Set tracker to Slave
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private ProcessState CashFlowsGenByTrade(Pair<EventsValMQueue, DataRow> pSource)
        {
            EventsValProcess eventsValProcess = null;
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                if (false == Convert.IsDBNull(pSource.Second["IDT"]))
                {
                    if (null != pSource.First)
                    {
                        eventsValProcess = new EventsValProcess(pSource.First, ProcessBase.AppInstance)
                        {
                            ProcessCacheContainer = m_PKGenProcess.ProcessCacheContainer,
                            
                            IsDataSetTrade_AllTable = false,
                            TradeTable = TradeTableEnum.NONE,
                            ProcessCall = ProcessBase.ProcessCallEnum.Slave,
                            ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.Start,
                            IsSendMessage_PostProcess = false,
                            
                            IdProcess = ProcessBase.IdProcess,

                            LogDetailEnum = ProcessBase.LogDetailEnum,
                            NoLockCurrentId = true,
                            // FI 20190605 [XXXXX] valorisation du tracker
                            Tracker = ProcessBase.Tracker
                        };
                        eventsValProcess.ProcessSlave();
                        eventsValProcess.MQueue = pSource.First;
                        eventsValProcess.ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.Execute;
                        eventsValProcess.ProcessState = new ProcessState(ProcessStateTools.StatusEnum.NONE, Cst.ErrLevel.SUCCESS);
                        eventsValProcess.DisposeFlags = new ProcessBaseDisposeFlags();
                        eventsValProcess.ProcessSlave();

                        if (eventsValProcess.ProcessState.CodeReturn != Cst.ErrLevel.SUCCESS)
                            processState.SetErrorWarning(eventsValProcess.ProcessState.Status, eventsValProcess.ProcessState.CodeReturn);
                        else if (pSource.Second.Table.Columns.Contains("QTY") && (0 < Convert.ToDecimal(pSource.Second["QTY"])))
                        {
                            if (pSource.First.quoteSpecified && (false == pSource.First.quote.valueSpecified))
                                processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.QUOTENOTFOUND);
                        }
                        eventsValProcess.EventsValProcessBase.SlaveCallDispose();
                    }
                    else
                        processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.QUOTENOTFOUND);
                }
                return processState;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (null != eventsValProcess)
                {
                    eventsValProcess.ProcessSlaveType = ProcessBase.ProcessSlaveTypeEnum.End;
                    eventsValProcess.ProcessSlave();
                    eventsValProcess.EventsValProcessBase.SlaveCallDispose();
                }
            }
        }
        #endregion CashFlowsGenByTrade


        /*  ----------------------------------------------*/
        /*         CALCUL DES FRAIS MANQUANTS             */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/

        #region FeesCalculationGenByTrade
        /// <summary>
        /// (RE)Calcul des frais manquants pour un trade donné candidat
        /// TRADE CANDIDAT = Il exist un book d'un de ses acteurs (TRADEACTOR) avec :
        /// 1) règle "Absence de frais" non autorisée  
        /// 2) sans frais sur ce book (pas de frais avec ce book payeur ou receveur)
        /// </summary>
        /// <param name="pTrade">Trade</param>
        /// <param name="pUser">Utilisateur (SYSADM)</param>
        // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        private ProcessState FeesCalculationGenByTrade(TradeFeesCalculation pTrade, User pUser)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                int nbTotPayment = 0;
                List<IPayment> lstAutoPayments = new List<IPayment>();
                List<IPayment> lstManualPayments = new List<IPayment>();
                List<IPayment> lstForcedPayments = new List<IPayment>();

                TradeInput tradeInput = new TradeInput();
                tradeInput.SearchAndDeserializeShortForm(CS, null, pTrade.idT.ToString(), SQL_TableWithID.IDType.Id, pUser, m_PKGenProcess.Session.SessionId);
                pTrade.Product = tradeInput.Product.Product;
                pTrade.DataDocument = tradeInput.DataDocument;

                #region SAUVEGARDE DES FRAIS MANUELS / FORCES
                // Les frais manuels et forcés sont toujours préservés
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                {
                    // RD 20160705 [22320] Valorisation de la variable nbTotPayment
                    nbTotPayment = tradeInput.CurrentTrade.OtherPartyPayment.Length;
                    foreach (IPayment item in (IPayment[])tradeInput.CurrentTrade.OtherPartyPayment)
                    {
                        // Ligne MANUELLE = (Ligne sans paymentSource ou sans status)
                        // EG 20130701 Frais forcés préservés au recalcul (ITD / EOD)
                        bool isManualPayment = (false == item.PaymentSourceSpecified) || (item.PaymentSourceSpecified && (false == item.PaymentSource.StatusSpecified));
                        bool isForcedPayment = item.PaymentSourceSpecified && item.PaymentSource.StatusSpecified && (item.PaymentSource.Status == SpheresSourceStatusEnum.Forced);

                        if (isManualPayment) lstManualPayments.Add(item);
                        if (isForcedPayment) lstForcedPayments.Add(item);
                    }
                }
                #endregion SAUVEGARDE DES FRAIS MANUELS / FORCES

                #region RECALCUL DES FRAIS
                tradeInput.CurrentTrade.OtherPartyPayment = null;
                tradeInput.CurrentTrade.OtherPartyPaymentSpecified = false;

                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 7001), 0,
                    new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT))));

                tradeInput.RecalculFeeAndTax(CS, null);
                if (tradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                    lstAutoPayments.AddRange(tradeInput.CurrentTrade.OtherPartyPayment);
                #endregion RECALCUL DES FRAIS

                #region CONTROLE DES FRAIS FORCES
                // Frais forcés préservés au recalcul (ITD / EOD)
                if (0 < lstForcedPayments.Count)
                {
                    // On recherche si les frais forcés ont leurs correspondances dans les frais recalculés
                    // Si oui on conserve les frais Forcés d'origine
                    // Si non on garde les frais recalculés
                    lstForcedPayments.ForEach(forcedPayment =>
                    {
                        IPayment autoPaymentFounded = DeleteMatchPayment(forcedPayment, lstAutoPayments);
                        if (null != autoPaymentFounded)
                        {
                            lstAutoPayments.Remove(autoPaymentFounded);
                            lstManualPayments.Add(forcedPayment);
                        }
                    });
                }
                #endregion CONTROLE DES FRAIS FORCES

                #region CONTROLE EXISTENCE FRAIS SUR BOOKS (VRFEE = WARNING/ERROR)
                /// On contrôle l'existence d'au moins une ligne de frais (MANUELLE/CALCULEES) pour chaque book concerné (VRFEE = WARNING/ERROR)
                List<IPayment> lstPayments = new List<IPayment>();
                if ((0 < lstManualPayments.Count) || (0 < lstAutoPayments.Count))
                {
                    if (0 < lstManualPayments.Count) lstPayments.AddRange(lstManualPayments);
                    if (0 < lstAutoPayments.Count) lstPayments.AddRange(lstAutoPayments);


                    var payments = (from payment in lstPayments
                                    select new
                                    {
                                        bookPayer = tradeInput.DataDocument.GetBookId(payment.PayerPartyReference.HRef),
                                        bookReceiver = tradeInput.DataDocument.GetBookId(payment.ReceiverPartyReference.HRef),
                                        payment
                                    }).ToList();

                    // Contrôle pour tous les books
                    foreach (KeyValuePair<int, ActorFeesCalculation> actor in pTrade.actorFee)
                    {
                        IPayment paymentFounded = (from payment in payments
                                                   where ((payment.bookPayer != null) && (payment.bookPayer.OTCmlId == actor.Key)) ||
                                                         ((payment.bookReceiver != null) && (payment.bookReceiver.OTCmlId == actor.Key))
                                                   select payment.payment).FirstOrDefault();
                        if (null != paymentFounded)
                        {
                            // SUCCESS : Frais trouvé pour le book
                            m_PKGenProcess.AddLogFeeInformation(tradeInput.DataDocument, paymentFounded);
                        }
                        else
                        {
                            // ERROR : Aucun frais trouvé pour le book
                            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.ERROR;
                            if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                                status = ProcessStateTools.StatusEnum.WARNING;

                            // FI 20200623 [XXXXX] call SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(status);

                            
                            
                            int errNum = (status == ProcessStateTools.StatusEnum.WARNING ? 5174 : 5175);
                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.SYS, errNum), 2,
                                new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT)),
                                new LogParam(LogTools.IdentifierAndId(actor.Value.actorIdentifier, actor.Value.idA)),
                                new LogParam(LogTools.IdentifierAndId(actor.Value.bookIdentifier, actor.Value.idB)),
                                new LogParam(actor.Value.idRoleActor),
                                new LogParam(actor.Value.vrFee)));

                            if (actor.Value.vrFee == Cst.CheckModeEnum.Error)//Si vrFee = Warning, on ne considèrera pas le traitement comme erroné.
                                pTrade.IsNotExistFeesOnBookWithVRError = true;
                            else if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                                pTrade.IsNotExistFeesOnBookWithVRWarning = true;
                        }
                    }
                }
                else
                {
                    // Aucun frais sur le trade 
                    foreach (KeyValuePair<int, ActorFeesCalculation> actor in pTrade.actorFee)
                    {
                        ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.ERROR;
                        if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                            status = ProcessStateTools.StatusEnum.WARNING;
                        
                        // FI 20200623 [XXXXX] call SetErrorWarning
                        m_PKGenProcess.ProcessState.SetErrorWarning(status);

                        
                        
                        int errNum = (status == ProcessStateTools.StatusEnum.WARNING ? 5174 : 5175);
                        Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.SYS, errNum), 2,
                            new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT)),
                            new LogParam(LogTools.IdentifierAndId(actor.Value.actorIdentifier, actor.Value.idA)),
                            new LogParam(LogTools.IdentifierAndId(actor.Value.bookIdentifier, actor.Value.idB)),
                            new LogParam(actor.Value.idRoleActor),
                            new LogParam(actor.Value.vrFee)));

                        if (actor.Value.vrFee == Cst.CheckModeEnum.Error)//Si vrFee = Warning, on ne considèrera pas le traitement comme erroné.
                            pTrade.IsNotExistFeesOnBookWithVRError = true;
                        else if (actor.Value.vrFee == Cst.CheckModeEnum.Warning)
                            pTrade.IsNotExistFeesOnBookWithVRWarning = true;
                    }
                }
                #endregion CONTROLE EXISTENCE FRAIS SUR BOOKS (VRFEE = WARNING/ERROR)

                lstPayments.Clear();
                pTrade.IsDeleted = (lstManualPayments.Count <= nbTotPayment) && (0 < nbTotPayment);
                if (0 < lstManualPayments.Count + lstAutoPayments.Count)
                {
                    lstPayments.AddRange(lstAutoPayments);
                    if (lstManualPayments.Count <= nbTotPayment)
                        lstPayments.AddRange(lstManualPayments);

                    pTrade.IsInserted = (0 < lstPayments.Count);
                    if (pTrade.IsInserted)
                    {
                        IPayment[] insertPayments = lstPayments.ToArray();
                        int nbEvents = 0;
                        pTrade.Payments = EventQuery.PrepareFeeEvents(CS, pTrade.Product, pTrade.DataDocument, pTrade.idT, insertPayments, ref nbEvents);
                        pTrade.NbEvents = nbEvents;
                        pTrade.DataDocument.CurrentTrade.OtherPartyPayment = insertPayments;
                        pTrade.DataDocument.CurrentTrade.OtherPartyPaymentSpecified = pTrade.IsInserted;
                    }
                }
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(pTrade.DataDocument.DataDocument);
                pTrade.TradeXML = CacheSerializer.Serialize(serializerInfo);
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] Call SetErrorWarning
                m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                // FI 20200623 [XXXXX] Call AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);
                
                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException("FeeCalculationGenByTrade", ex)));
            }
            return processState;
        }
        #endregion FeesCalculationGenByTrade
        #region FeesWritingGenByTrade
        /// <summary>
        /// Ecriture des frais calculés précédemment
        /// </summary>
        // EG 20190308 Upd [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        protected ProcessState FeesWritingGenByTrade(TradeFeesCalculation pTrade)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            IDbTransaction dbTransaction = null;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            bool isException = false;
            string tradeLogIdentifier = LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT);

            int newIdE;
            try
            {
                dbTransaction = DataHelper.BeginTran(CS);
                SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, pTrade.NbEvents);
                DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception) { if (null != dbTransaction) DataHelper.RollbackTran(dbTransaction); throw; }

            try
            {

                dbTransaction = DataHelper.BeginTran(CS);

                #region SUPPRESSION DES EVENEMENTS DE FRAIS PRECEDENTS
                if (pTrade.IsDeleted)
                {
                    try
                    {
                        // DELETE
                        EventQuery.DeleteFeeEvents(CS, dbTransaction, pTrade.idT);
                    }
                    catch (DbException ex)
                    {
                        AppInstance.TraceManager.TraceInformation(this, string.Format("Error={0}", ex.Message));
                        if (ex.Message.Contains(Cst.OTCml_Constraint.FK_EARDAY_EVENTCLASS))
                        {
                            // FI 20200623 [XXXXX] SetErrorWarning
                            m_PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                            
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5178), 0, new LogParam(tradeLogIdentifier)));
                        }
                        throw;
                    }
                }
                #endregion SUPPRESSION DES EVENEMENTS DE FRAIS PRECEDENTS

                #region INSERTION DES EVENEMENTS DES NOUVEAUX FRAIS
                if (pTrade.IsInserted)
                {
                    m_EventQuery.InsertFeeEvents(CS, dbTransaction, pTrade.DataDocument, pTrade.idT, DtBusiness, pTrade.idE_Event, pTrade.Payments, newIdE);
                }
                #endregion INSERTION DES EVENEMENTS DES NOUVEAUX FRAIS

                #region MISE A JOUR TRADEXML
                // ------------------------------------------------------------------ 
                // TRAITEMENT FINAL DES FRAIS
                // Mise à jour des caractéristiques des frais sur le trade XML
                // A FAIRE S'IL Y A EU SUPPRESSION et/ou INSERTION
                // ------------------------------------------------------------------ 
                if ((Cst.ErrLevel.SUCCESS == codeReturn) && (false == isException))
                {
                    
                    //TradeRDBMSTools.UpdateTradeXML(dbTransaction, pTrade.idT, pTrade.tradeXML, dtSys,
                    //m_PKGenProcess.UserId, m_PKGenProcess.appInstance, m_PKGenProcess.processLog.header.idTRK_L, m_PKGenProcess.processLog.header.IdProcess);
                    // FI 20200820 [25468] dates systemes en UTC
                    TradeRDBMSTools.UpdateTradeXML(dbTransaction, pTrade.idT, pTrade.TradeXML, OTCmlHelper.GetDateSysUTC(CS),
                    m_PKGenProcess.UserId, m_PKGenProcess.Session, m_PKGenProcess.Tracker.IdTRK_L, m_PKGenProcess.IdProcess);
                }
                #endregion MISE A JOUR TRADEXML

                DataHelper.CommitTran(dbTransaction);

            }
            catch (Exception ex)
            {
                AppInstance.TraceManager.TraceInformation(this, string.Format("FeesWritingGen={0};Error={1};Trade={2}", "Catch", ex.Message, tradeLogIdentifier));
                codeReturn = Cst.ErrLevel.FAILURE;
                isException = true;
                bool blockException = (ex is DbException) && ex.Message.Contains(Cst.OTCml_Constraint.FK_EARDAY_EVENTCLASS);
                if (false == blockException) { throw; }
            }
            finally
            {
                //Il existe au moins un book sans frais, disposant d'une vrFee = Error --> on considèrera le traitement comme erroné.
                if (pTrade.IsNotExistFeesOnBookWithVRError)
                    processState.SetErrorWarning(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                else if (pTrade.IsNotExistFeesOnBookWithVRWarning)
                    processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.DATANOTFOUND);

                if ((null != dbTransaction) && isException)
                    DataHelper.RollbackTran(dbTransaction);

                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
            return processState;
        }
        #endregion FeesWritingGenByTrade

        #region DeleteMatchPayment
        /// <summary>
        /// La ligne de frais Forcée matche t-elle avec une ligne de frais recalculée issu d'un barème ???
        /// Quels éléments sont des critères de matching
        /// </summary>
        /// <param name="pPayment"></param>
        private IPayment DeleteMatchPayment(IPayment pPayment, List<IPayment> pLstPayment)
        {
            bool isMatch = false;

            IPayment _matchPayment = pLstPayment.Find(match =>
                (pPayment.PayerPartyReference.HRef == match.PayerPartyReference.HRef) &&
                (pPayment.ReceiverPartyReference.HRef == match.ReceiverPartyReference.HRef) &&
                (pPayment.PaymentTypeSpecified == match.PaymentTypeSpecified) &&
                (pPayment.PaymentType.Value == match.PaymentType.Value) &&
                (pPayment.PaymentAmount.GetCurrency == match.PaymentAmount.GetCurrency) &&
                (pPayment.PaymentDateSpecified == match.PaymentDateSpecified) &&
                (pPayment.PaymentDate.UnadjustedDate.DateValue == match.PaymentDate.UnadjustedDate.DateValue) &&
                (pPayment.PaymentSourceSpecified)
                );

            if (null != _matchPayment)
            {
                #region IdFeeMatrix
                if (Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeMatrixScheme) &&
                    Tools.IsPaymentSourceScheme(_matchPayment, Cst.OTCml_RepositoryFeeMatrixScheme)
                    )
                {
                    isMatch &= (pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId ==
                               _matchPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId);
                }
                #endregion IdFeeMatrix
                #region IdFee
                if (Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeScheme) &&
                    Tools.IsPaymentSourceScheme(_matchPayment, Cst.OTCml_RepositoryFeeScheme)
                    )
                {
                    isMatch &= (pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId ==
                               _matchPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId);
                }
                #endregion IdFee
                #region IdFeeSchedule
                if (Tools.IsPaymentSourceScheme(pPayment, Cst.OTCml_RepositoryFeeScheme) &&
                    Tools.IsPaymentSourceScheme(_matchPayment, Cst.OTCml_RepositoryFeeScheme)
                    )
                {
                    isMatch &= (pPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId ==
                               _matchPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId);
                }
                #endregion IdFeeSchedule
                if (false == isMatch)
                {
                    _matchPayment = null;
                }
            }
            return _matchPayment;
        }
        #endregion DeleteMatchPayment

        /*  ----------------------------------------------*/
        /*         CALCUL DES FRAIS DE GARDE              */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/
        #region SafeKeepingCalculationGenByTrade
        /// <summary>
        /// Calcul des frais de garde pour un trade donné candidat
        /// </summary>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190716 [VCL : New FixedIncome] Use GetQuoteLockWithKeyQuote instead of GetQuoteLock
        // EG 20190716 [VCL : New FixedIncome] Set marketValueData
        protected ProcessState SafeKeepingCalculationGenByTrade(TradeSafeKeepingCalculation pTrade, User pUser)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            try
            {
                // Trade en position (DTSETTLT) donc calcul des frais de garde
                if ((0 < pTrade.qty) && (0 < pTrade.idE_Event))
                {
                    TradeInput tradeInput = new TradeInput();
                    tradeInput.SearchAndDeserializeShortForm(CS, null, pTrade.idT.ToString(), SQL_TableWithID.IDType.Id, pUser, m_PKGenProcess.Session.SessionId);

                    SystemMSGInfo errReadOfficialClose = null;
                    Quote quote = GetQuoteLockWithKeyQuote(pTrade.IdAsset, pTrade.DtQuote, pTrade.AssetIdentifier, pTrade.UnderlyingAsset, ref errReadOfficialClose) as Quote;

                    // ----------------------------------------------------------------------------------------------------------------------
                    // Alimentation de la class SafekeepingAction pour le calcul (Qty, DtBusiness, Prix de clôture, Début/Fin de périodes)
                    // ----------------------------------------------------------------------------------------------------------------------
                    tradeInput.safekeepingAction = new SafekeepingAction
                    {
                        quantity = new EFS_Decimal(pTrade.qty),
                        dtBusiness = new EFS_Date
                        {
                            DateValue = DtBusiness
                        },
                        dtStartPeriod = new EFS_Date
                        {
                            DateValue = DtBusiness
                        },
                        dtEndPeriod = new EFS_Date
                        {
                            DateValue = m_EntityMarketInfo.DtEntityNext
                        },
                        closingPrice = quote,
                        marketValueSpecified = (null != pTrade.MarketValue),
                        marketValue = pTrade.MarketValue,
                        marketValueQty = pTrade.MarketValueQty,
                        marketValueDataSpecified = (null != pTrade.MarketValueData),
                        marketValueData = pTrade.MarketValueData,
                        errReadOfficialCloseSpecified = (null != errReadOfficialClose),
                        errReadOfficialClose = errReadOfficialClose
                    };

                    // ----------------------------------------------------------------------------------------------------------------------
                    // Lecture des barèmes candidats et calcul des frais
                    // ----------------------------------------------------------------------------------------------------------------------
                    tradeInput.RecalculFeeAndTax(CS, null);
                    pTrade.IsInserted = tradeInput.safekeepingAction.paymentSpecified;
                    if (pTrade.IsInserted)
                    {
                        pTrade.identifier = tradeInput.Identifier;
                        pTrade.IsQuoteNotFound = tradeInput.safekeepingAction.IsErrorQuote;
                        if (pTrade.IsQuoteNotFound)
                        {
                            pTrade.errReadOfficialCloseSpecified = tradeInput.safekeepingAction.errReadOfficialCloseSpecified;
                            pTrade.errReadOfficialClose = tradeInput.safekeepingAction.errReadOfficialClose;
                        }

                        if (tradeInput.Product.IsDebtSecurityTransaction)
                        {
                            //if (false == pTrade.isQuoteNotFound)
                            //    pTrade.isQuoteNotFound = tradeInput.safekeepingAction.payment.ToList().Exists(item => item.paymentAmount.Amount.DecValue == 0);
                            // FI 20230628 [XXXXX] use DebtSecurityTransactionContainer for ResolveSecurityAsset
                            IDebtSecurityTransaction debtSecurityTransaction = new DebtSecurityTransactionContainer(tradeInput.Product.Product as IDebtSecurityTransaction, tradeInput.DataDocument).DebtSecurityTransaction;
                            if (null == debtSecurityTransaction.Efs_DebtSecurityTransactionStream)
                                debtSecurityTransaction.SetStreams(CS, tradeInput.DataDocument, Cst.StatusBusiness.ALLOC);
                        }

                        pTrade.Payments = tradeInput.safekeepingAction.payment.ToList();
                        pTrade.DataDocument = tradeInput.DataDocument;
                        pTrade.Product = tradeInput.Product.Product;

                        // Ecriture des frais calculés dans la table EVENT
                        foreach (IPayment payment in pTrade.Payments)
                        {
                            payment.Efs_Payment = new EFS_Payment(m_PKGenProcess.Cs, null, payment, tradeInput.Product.Product, tradeInput.DataDocument);
                            DateTime dtStartperiod = tradeInput.safekeepingAction.dtStartPeriod.DateValue;
                            DateTime dtEndperiod = tradeInput.safekeepingAction.dtEndPeriod.DateValue.AddDays(-1);
                            payment.Efs_Payment.SetEventDatePeriods(dtStartperiod, dtStartperiod, dtEndperiod, dtEndperiod);
                        }

                        if (pTrade.IsQuoteNotFound)
                            processState.SetErrorWarning(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.QUOTENOTFOUND);
                    }
                }
            }
            catch (Exception ex)
            {
                processState.SetErrorWarning(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);

                // FI 20200623 [XXXXX] AddCriticalException
                m_PKGenProcess.ProcessState.AddCriticalException(ex);

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException("SafeKeepingCalculationGenByTrade", ex)));

            }
            return processState;
        }
        #endregion SafeKeepingCalculationGenByTrade
        #region SafeKeepingWritingGenByTrade
        /// <summary>
        /// Ecriture des frais calculés précédemment
        /// </summary>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        protected ProcessState SafeKeepingWritingGenByTrade(TradeSafeKeepingCalculation pTrade)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            if (pTrade.IsInserted || pTrade.IsDeleted)
            {
                bool isException = false;
                string tradeLogIdentifier = LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT);

                IDbTransaction dbTransaction = null;
                int newIdE = 0;
                if (0 < pTrade.NbEvents)
                {
                    try
                    {
                        /// EG 20160208 [POC-MUREX] Token sur EVENT pour l'ensemble des frais de garde (avec Commit)
                        dbTransaction = DataHelper.BeginTran(CS);
                        SQLUP.GetId(out newIdE, dbTransaction, SQLUP.IdGetId.EVENT, SQLUP.PosRetGetId.First, pTrade.NbEvents);
                        DataHelper.CommitTran(dbTransaction);
                    }
                    catch (Exception) { if (null != dbTransaction) DataHelper.RollbackTran(dbTransaction); throw; }
                    finally { if (null != dbTransaction)dbTransaction.Dispose(); }
                }

                try
                {
                    dbTransaction = DataHelper.BeginTran(CS);
                    // SKP/ttt EXISTE DEJA on le supprime avant recréation (si besoin => QTY > 0)
                    if (pTrade.IsDeleted)
                        EventQuery.DeleteSafekeepingEvent(m_PKGenProcess.Cs, dbTransaction, pTrade.idT, DtBusiness);

                    if (pTrade.IsInserted)
                    {
                        if (pTrade.IsQuoteNotFound)
                        {
                            if (pTrade.errReadOfficialCloseSpecified)
                            {
                                
                                Logger.Log(new LoggerData(LogLevelEnum.Warning, pTrade.errReadOfficialClose.SysMsgCode, 0, pTrade.errReadOfficialClose.LogParamDatas));
                            }
                            
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 5166), 0,
                                new LogParam(LogTools.IdentifierAndId(pTrade.identifier, pTrade.idT))));

                            processState.SetErrorWarning(ProcessStateTools.StatusEnum.WARNING, Cst.ErrLevel.QUOTENOTFOUND);
                        }

                        pTrade.Payments.ForEach(payment =>
                        {
                            m_EventQuery.InsertPaymentEvents(dbTransaction, pTrade.DataDocument, pTrade.idT, payment,
                                EventCodeFunc.SafeKeepingPayment, DtBusiness, 1, 1, ref newIdE, pTrade.idE_Event);
                            newIdE++;
                        });
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception ex)
                {
                    AppInstance.TraceManager.TraceInformation(this, string.Format("SafeKeepingWritingGenByTrade={0};Error={1};Trade={2}", "Catch", ex.Message, tradeLogIdentifier));
                    processState.SetErrorWarning(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                    isException = true;
                    bool blockException = (ex is DbException) && ex.Message.Contains(Cst.OTCml_Constraint.FK_EARDAY_EVENTCLASS);
                    if (false == blockException) { throw; }
                }
                finally
                {
                    if ((null != dbTransaction) && isException)
                        DataHelper.RollbackTran(dbTransaction);

                    if (null != dbTransaction)
                        dbTransaction.Dispose();
                }

            }
            return processState;
        }
        #endregion SafeKeepingWritingGenByTrade

        /*  ----------------------------------------------*/
        /*  COMMON                                        */
        /*  CALCUL DES CASH-FLOWS                         */
        /*  CALCUL DES FRAIS MANQUANTS                    */
        /*  CALCUL FRAIS DE GARDE                         */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/

        #region CommonGenThreading
        /// <summary>
        /// Préparation intermédiaire sous forme de sous-listes de sources candidates (Trades ou autres) pour envoi (ASYNBCHRONE MODE)
        /// - Cash-Flows
        /// - Calcul|Ecriture des frais manquants
        /// - Calcul|Ecriture des frais de garde
        /// - Fermeture /Réouverture de positions
        /// </summary>
        /// <typeparam name="T">Cash-Flows|TradeFeesCalculation|TradeSafeKeepingCalculation|ClosingReopeningCalculation</typeparam>
        /// <param name="pParallelProcess">Parallel Process 
        ///     - CashFlows
        ///     - FeesCalculation|FeesWriting
        ///     - SafeKeepingCalculation|SafeKeepingWriting
        ///     - ClosingReopeningCalculation|ClosingReopeningWriting)
        /// </param>
        /// <param name="pLstSource">Liste complète des sources (Trades ou autres) candidates</param>
        /// <param name="pUser">Utilisateur (SYSADM)</param>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190318 Upd ClosingReopening position Step3
        // EG 20190613 [24683] Gestion Closing/Reopening (Cas Synthetic)
        protected ProcessState CommonGenThreading<T>(ParallelProcess pParallelProcess, List<T> pLstSource, User pUser)
        {
            ProcessState processStateMain = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);

            int heapSize = ProcessBase.GetHeapSize(pParallelProcess);
            ProcessBase.InitializeMaxThreshold(pParallelProcess);
            ProcessBase.SemaphoreAsync2 = null;
            ProcessBase.SemaphoreAsync2 = new SemaphoreSlim(1);

            List<List<T>> lstSource = ListExtensionsTools.ChunkBy(pLstSource, heapSize);

            int counter = 1;
            int totSubList = lstSource.Count();
            lstSource.ForEach(subLstSource =>
            {
                if (false == IRQTools.IsIRQRequested(PKGenProcess, PKGenProcess.IRQNamedSystemSemaphore, ref processStateMain))
                {
                    if ((1 < heapSize) || (1 == totSubList) || (pParallelProcess == ParallelProcess.ClosingReopeningWriting))
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel {2} {0}/{1}", counter, totSubList, pParallelProcess.ToString()), 2));
                        Logger.Write();
                    }
                    processState = CommonGenAsync(pParallelProcess, subLstSource, pUser).Result;
                    processStateMain.SetErrorWarning(processState.Status, processState.CodeReturn);
                    AppInstance.TraceManager.TraceTimeSummary(pParallelProcess.ToString(), StrFunc.AppendFormat("{0}/{1}", counter, totSubList));
                    counter++;
                }
            });
            return processStateMain;
        }
        #endregion CommonGenThreading
        #region CommonGenAsync
        /// <summary>
        /// Envoi du traitement (ASYNBCHRONE MODE)
        /// - Cash-Flows
        /// - Calcul|Ecriture des frais manquants
        /// - Calcul|Ecriture des frais de garde
        /// </summary>
        /// <typeparam name="T">CashFlows|TradeFeesCalculation|TradeSafeKeepingCalculation</typeparam>
        /// <param name="pParallelProcess">Parallel Process (Calculation|Writing)</param>
        /// <param name="pSource">Sous-liste des trades candidats</param>
        /// <param name="pUser">Utilisateur (SYSADM)</param>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        // EG 20190613 [24683] Gestion ClosingReopenin (Writing)
        protected async Task<ProcessState> CommonGenAsync<T>(ParallelProcess pParallelProcess, List<T> pSource, User pUser)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<ProcessState>> getReturnTasks = null;

            try
            {
                IEnumerable<Task<ProcessState>> getReturnTasksQuery = null;
                switch (pParallelProcess)
                {
                    case ParallelProcess.FeesCalculation:
                    case ParallelProcess.SafeKeepingCalculation:
                        getReturnTasksQuery =
                            from trade in pSource
                            select CommonGenByTradeAsync(pParallelProcess, (trade as TradeBaseCalculation).idT, trade, pUser, cts.Token);
                        break;
                    case ParallelProcess.FeesWriting:
                    case ParallelProcess.SafeKeepingWriting:
                        getReturnTasksQuery =
                            from trade in pSource
                            where (trade as TradeBaseCalculation).IsWriting
                            select CommonGenByTradeAsync(pParallelProcess, (trade as TradeBaseCalculation).idT, trade, pUser, cts.Token);
                        break;
                    case ParallelProcess.CashFlows:
                        getReturnTasksQuery =
                            from source in pSource as List<Pair<EventsValMQueue, DataRow>>
                            select CommonGenByTradeAsync(pParallelProcess, Convert.ToInt32(source.Second["IDT"]), source, pUser, cts.Token);
                        break;
                    case ParallelProcess.ClosingReopeningCalculation:
                        getReturnTasksQuery =
                            from source in pSource as List<KeyValuePair<ARQTools.TradeKey, List<ARQTools.TradeCandidate>>>
                            select ClosingReopeningGenByPositionAsync(pParallelProcess, source, pUser, cts.Token);
                        break;
                    case ParallelProcess.ClosingReopeningWriting:
                        getReturnTasksQuery =
                            from source in pSource as List<KeyValuePair<ARQTools.TradeKey, List<ARQTools.TradeCandidate>>>
                            where source.Key.statusCalculationSpecified &&
                            ((ProcessStateTools.StatusSuccessEnum == source.Key.statusCalculation.Status) || 
                             (ProcessStateTools.StatusWarningEnum == source.Key.statusCalculation.Status))
                            select ClosingReopeningGenByPositionAsync(pParallelProcess, source, pUser, cts.Token);
                        break;
                }

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle de traitement asynchrone
                while (0 < getReturnTasks.Count)
                {
                    Task<ProcessState> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);

                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CommonGenAsync", null, true);

                    if (IRQTools.IsIRQRequested(m_PKGenProcess, m_PKGenProcess.IRQNamedSystemSemaphore, ref processState))
                        cts.Cancel(true);
                    else
                        processState.SetErrorWarning(firstFinishedTask.Result.Status, firstFinishedTask.Result.CodeReturn);
                }
            }
            catch (AggregateException aex)
            {
                // FI 20200722 [XXXXX] TraceDataError et pas d'appel à Flatten() puisque throw firstFinishedTask.Exception.Flatten();
                // AppInstance.TraceManager.TraceDataInformation(this, aex.Flatten().Message);
                AppInstance.TraceManager.TraceError(this, aex.Message);
                // FI 20200722 [XXXXX] Alimentation de toutes les exceptions qui accompagnent aex
                foreach (Exception ex in aex.InnerExceptions)
                    AppInstance.TraceManager.TraceError(this, $"{ex.GetType()}:{ex.Message}");

                if (false == ProcessStateTools.IsStatusInterrupt(processState.Status))
                    throw;
            }
            catch (Exception)
            {
                processState.SetErrorWarning(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.FAILURE);
                throw;
            }
            
            return processState;
        }
        #endregion CommonGenAsync
        #region CommonGenByTradeAsync
        /// <summary>
        /// Mise en file d'attente d'une tâche (ASYNCHRONE MODE)
        /// - Calcul des Cash-Flows
        /// - Calcul|Ecriture des frais manquants
        /// - Calcul|Ecriture des frais de garde
        /// </summary>
        /// <typeparam name="T">Pair(EventsValMQueue, DataRow)TradeFeesCalculation|TradeSafeKeepingCalculation</typeparam>
        /// <param name="pParallelProcess">Parallel Process (CashFlows|FeesCalculation|FeesWriting|SafeKeepingCalculation|SafeKeepingWriting)</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pSource">Trade</param>
        /// <param name="pUser">Utilisateur (SYSADM)</param>
        /// <param name="pCt">Notification d'annulation</param>
        // EG 20190308 New [VCL_Migration] MultiThreading refactoring (+SafeKeeping multiThreading)
        protected async Task<ProcessState> CommonGenByTradeAsync<T>(ParallelProcess pParallelProcess, int pIdT, T pSource, User pUser, CancellationToken pCt)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            string key = String.Format("(IDT: {0} )", pIdT);
            string wait = "START CommonGenByTradeAsync Wait   : {0} ({1}) " + key;
            string release = "STOP  CommonGenByTradeAsync Release: {0} ({1}) " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    AppInstance.TraceManager.TraceTimeBegin(pParallelProcess.ToString() + "CommonGenByTradeAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Wait();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount, pParallelProcess.ToString()));
                    }
                    switch (pParallelProcess)
                    {
                        case ParallelProcess.CashFlows:
                            processState = CashFlowsGenByTrade(pSource as Pair<EventsValMQueue, DataRow>);
                            break;
                        case ParallelProcess.FeesCalculation:
                            processState = FeesCalculationGenByTrade(pSource as TradeFeesCalculation, pUser);
                            break;
                        case ParallelProcess.FeesWriting:
                            // FI 20221006 [XXXXX] Add TryMultiple
                            TryMultiple tryMultiple = new TryMultiple(CS, "FeesWritingGenByTrade", $"Fees WritingGen By Trade")
                            {
                                SetErrorWarning = ProcessBase.ProcessState.SetErrorWarning,
                                IsModeTransactional = false,
                                ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                            };
                            processState = tryMultiple.Exec<TradeFeesCalculation,ProcessState>(FeesWritingGenByTrade, pSource as TradeFeesCalculation);
                            break;
                        case ParallelProcess.SafeKeepingCalculation:
                            processState = SafeKeepingCalculationGenByTrade(pSource as TradeSafeKeepingCalculation, pUser);
                            break;
                        case ParallelProcess.SafeKeepingWriting:
                            processState = SafeKeepingWritingGenByTrade(pSource as TradeSafeKeepingCalculation);
                            break;
                    }
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Release();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, ProcessBase.SemaphoreAsync.CurrentCount, pParallelProcess.ToString()));
                    }
                    AppInstance.TraceManager.TraceTimeEnd(pParallelProcess.ToString() + "CommonGenByTradeAsync", key);
                }
            }, pCt);

            return processState;
        }
        #endregion CommonGenByTradeAsync

        /*  ----------------------------------------------*/
        /*               CALCUL DES UTI                   */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/

        #region CalcAndRecordUTIThreading
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevelMessage CalcAndRecordUTIThreading(DataTable pDataTable)
        {
            List<Pair<bool, string>> retGlobal = new List<Pair<bool, string>>();
            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);
            string successMessage = string.Empty;
            string errorMessage = string.Empty;

            //Dictionnaire des PUTI traité avec succès 
            m_CDicPUTIOk = new ConcurrentDictionary<string, int>();

            ProcessBase.InitializeMaxThreshold(ParallelProcess.UTICalculation);
            int heapSize = ProcessBase.GetHeapSize(ParallelProcess.UTICalculation);
            List<List<DataRow>> lstRows = ListExtensionsTools.ChunkBy(pDataTable.AsEnumerable().ToList(), heapSize);
            int counter = 1;
            int totSubList = lstRows.Count();
            if (0 < totSubList)
            {
                lstRows.ForEach(subLstRows =>
                {
                    if ((1 < heapSize) || (1 == totSubList))
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Info, String.Format("Parallel UTI calculation {0}/{1}", counter, totSubList), 2));
                        Logger.Write();
                    }
                    List<Pair<bool, string>> retChunk = CalcAndRecordUTIGenAsync(subLstRows).Result;
                    if (0 < retChunk.Count)
                        retGlobal.AddRange(retChunk);
                    
                    AppInstance.TraceManager.TraceTimeSummary("CalcAndRecordUTIAsync", StrFunc.AppendFormat("{0}/{1}", counter, totSubList)); 
                    
                    counter++;
                });

                retGlobal.ForEach(item => StrFunc.BuildStringListElement(ref errorMessage, item.Second, 2));
                
                if (StrFunc.IsFilled(errorMessage))
                {
                    ret.Message = Ressource.GetString("ERROR") + ": " + errorMessage + Cst.CrLf;
                    ret.ErrLevel = Cst.ErrLevel.FAILURE;
                }
            }
            else
            {
                ret.ErrLevel = Cst.ErrLevel.FAILURE;
                ret.Message = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
            }
            return ret;

        }
        #endregion CalcAndRecordUTIThreading
        #region CalcAndRecordUTIGenAsync
        protected async Task<List<Pair<bool, string>>> CalcAndRecordUTIGenAsync(List<DataRow> pLstRows)
        {
            List<Pair<bool, string>> ret = new List<Pair<bool, string>>();

            List<Task<Pair<bool, string>>> getReturnTasks = null;

            CancellationTokenSource cts = new CancellationTokenSource();

            try
            {
                // Création des tâches de traitement des dénouements pour chaque clé de position
                IEnumerable<Task<Pair<bool, string>>> getReturnTasksQuery = from row in pLstRows select CalcAndRecordUTIByRowAsync(row, cts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle de traitement asynchrone des calculs UTI
                while (0 < getReturnTasks.Count)
                {
                    Task<Pair<bool, string>> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);

                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CalcAndRecordUTIGenAsync", null, true);

                    if (firstFinishedTask.Result.First)
                        ret.Add(firstFinishedTask.Result);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return ret;
        }
        #endregion CalcAndRecordUTIGenAsync
        #region CalcAndRecordUTIByRowAsync
        public async Task<Pair<bool, string>> CalcAndRecordUTIByRowAsync(DataRow pRow, CancellationToken pCt)
        {
            Pair<bool, string> error = new Pair<bool, string>(false, null);

            string key = String.Format("(IDT: {0} )", pRow["IDT"].ToString());
            string wait = "START CalcAndRecordUTIByRow Wait   : {0} " + key;
            string release = "STOP  CalcAndRecordUTIByRow Release: {0} " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    AppInstance.TraceManager.TraceTimeBegin("CalcAndRecordUTIAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Wait();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount));
                    }
                    error = CalcAndRecordUTIByRow(pRow);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Release();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, ProcessBase.SemaphoreAsync.CurrentCount));
                    }
                    AppInstance.TraceManager.TraceTimeEnd("CalcAndRecordUTIAsync", key);
                }
            }, pCt);

            return error;

        }
        #endregion CalcAndRecordUTIByRowAsync
        #region CalcAndRecordUTIByRow
        public Pair<bool, string> CalcAndRecordUTIByRow(DataRow pRow)
        {

            IDbTransaction dbTransaction = null;
            bool isException = false;
            Pair<bool, string> error = new Pair<bool, string>(false, null);

            string keyTrc = string.Empty;
            try
            {
                
                dbTransaction = DataHelper.BeginTran(CS);

                UTIComponents utiComponents = UTITools.InitUTIComponentsFromDataRow(CS, dbTransaction, pRow, null);
                // FI 20180314 [XXXXX] TraceTime
                keyTrc = String.Format("(IDT: {0} )", utiComponents.Trade_id);    
                AppInstance.TraceManager.TraceTimeBegin("UTICalculation", keyTrc);


                UTIType utiType = UTIType.UTI;
                if ((utiComponents.IsRecalculUTI_Dealer) || (utiComponents.IsRecalculUTI_Clearer))
                    utiType = UTIType.UTI; //=> Calcul de l'UTI/PUTI  
                else if ((utiComponents.IsRecalculPUTI_Dealer) || (utiComponents.IsRecalculPUTI_Clearer))
                    utiType = UTIType.PUTI; //=> Calcul du PUTI uniquement

                #region ERR: Already exists
                if (false == error.First)
                {
                    if (
                        ((utiType == UTIType.UTI) && (false == utiComponents.IsRecalculUTI_Dealer) && (false == utiComponents.IsRecalculUTI_Clearer)) ||
                        ((utiType == UTIType.PUTI) && (false == utiComponents.IsRecalculPUTI_Dealer) && (false == utiComponents.IsRecalculPUTI_Clearer))
                        )
                    {
                        error.First = true;
                        error.Second = "Already exists on {0}";
                    }
                }
                #endregion ERR: Already exists

                #region ERR: Entity not Regulatory Office
                if (false == error.First)
                {
                    if (utiComponents.Entity_RegulatoryOffice_Id <= 0)
                    {
                        // Pas de calcul de l'UTI lorsqu'il n'existe aucun Regulatory Office relatif à l'Entité.
                        error.First = true;
                        error.Second = "Entity not Regulatory Office on {0}";
                    }
                }
                #endregion ERR: Entity not Regulatory Office

                #region UTIs computation
                string keyPUTI = utiComponents.PositionKey;

                if (m_CDicPUTIOk.ContainsKey(keyPUTI))
                {
                    utiComponents.PosUti_IdPosUti = m_CDicPUTIOk[keyPUTI];
                    utiComponents.StatusPUTI_Dealer = "OK";
                    utiComponents.StatusPUTI_Clearer = "OK";

                }
                if (false == error.First)
                {
                    //PL 20160427 [22107] - Refactoring apporté dans le cadre de ce chantier, mais sans rapport direct avec lui.
                    UTITools.StatusCalcUTI resultUTI = UTITools.StatusCalcUTI.NotAvailable;
                    UTITools.StatusCalcUTI resultPUTI = UTITools.StatusCalcUTI.NotAvailable;

                    if ((utiType == UTIType.UTI))
                    {
                        //--------------------------------------------------------------------
                        resultUTI = UTITools.CalcAndRecordUTI(CS, dbTransaction, UTIType.UTI, utiComponents, m_PKGenProcess.Session.IdA);
                        //--------------------------------------------------------------------
                        resultPUTI = UTITools.CalcAndRecordUTI(CS, dbTransaction, UTIType.PUTI, utiComponents, m_PKGenProcess.Session.IdA);
                        //--------------------------------------------------------------------
                    }
                    else if (utiType == UTIType.PUTI)
                    {
                        //--------------------------------------------------------------------
                        resultPUTI = UTITools.CalcAndRecordUTI(CS, dbTransaction, UTIType.PUTI, utiComponents, m_PKGenProcess.Session.IdA);
                        //--------------------------------------------------------------------
                    }

                    // Si la clé keyPUTI a déjà été traité avec succès, le statut StatusCalcUTI.AlreadyExists est remplacé par ValuedWithSuccess
                    // de manière à ne pas générer une erreur dans le message de sortie
                    if (resultPUTI == UTITools.StatusCalcUTI.AlreadyExists && m_CDicPUTIOk.ContainsKey(keyPUTI))
                    {
                        resultPUTI = UTITools.StatusCalcUTI.ValuedWithSuccess;
                    }

                    if (resultPUTI == UTITools.StatusCalcUTI.ValuedWithSuccess)
                    {
                        if (false == m_CDicPUTIOk.ContainsKey(keyPUTI))
                            m_CDicPUTIOk.AddOrUpdate(keyPUTI, utiComponents.PosUti_IdPosUti, (key, oldValue) => utiComponents.PosUti_IdPosUti);
                    }

                    #region ERR: Miscellaneous
                    if ((utiType == UTIType.UTI) && (resultUTI != UTITools.StatusCalcUTI.ValuedWithSuccess))
                    {

                        error.First = true;
                        switch (resultUTI)
                        {
                            case UTITools.StatusCalcUTI.ErrorOccured:
                                error.Second = "Trade {0} : UTI error occured";
                                break;
                            case UTITools.StatusCalcUTI.NotComputable:
                                error.Second = "Trade {0} : UTI not computable";
                                break;
                            case UTITools.StatusCalcUTI.AlreadyExists:
                                error.Second = "Trade {0} : UTI already exists";
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Result {0} is not implemented", resultUTI.ToString()));
                        }

                    }
                    if (resultPUTI != UTITools.StatusCalcUTI.ValuedWithSuccess)
                    {
                        string errorMsg = string.Empty;
                        if (error.First)
                        {
                            //NB: Une erreur a également été rencontré lors du calcul de l'UTI.
                            errorMsg = error.Second + ", ";
                        }
                        else
                        {
                            error.First = true;
                            errorMsg = "Trade {0} : ";
                        }
                        switch (resultPUTI)
                        {
                            case UTITools.StatusCalcUTI.ErrorOccured:
                                error.Second = errorMsg + "PUTI error occured";
                                break;
                            case UTITools.StatusCalcUTI.NotComputable:
                                error.Second = errorMsg + "PUTI not computable on {0}";
                                break;
                            case UTITools.StatusCalcUTI.AlreadyExists:
                                error.Second = errorMsg + "PUTI already exists on {0}";
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("Result {0} is not implemented.", resultPUTI.ToString()));
                        }
                    }
                    #endregion
                }
                #endregion UTIs computation

                if (error.First)
                    error.Second = StrFunc.AppendFormat(error.Second, utiComponents.Trade_Identifier);

                DataHelper.CommitTran(dbTransaction);

            }
            catch
            {
                isException = true;
                throw;

            }
            finally
            {
                // FI 20180314 [XXXXX] TraceTime
                AppInstance.TraceManager.TraceTimeEnd("UTICalculation", keyTrc);
                
                if (null != dbTransaction)
                {
                    if (isException)
                        DataHelper.RollbackTran(dbTransaction);

                    dbTransaction.Dispose();
                }
            }
            return error;
        }
        #endregion CalcAndRecordUTIByRow


        /*  ----------------------------------------------*/
        /*     FERMETURE/REOUVERTURE DE POSITIONS         */
        /*  ----------- ASYNCHRONE MODE ------------------*/
        /*  ----------------------------------------------*/

        #region ClosingReopeningGenByPositionAsync
        /// <summary>
        /// Mise en file d'attente d'une tâche (ASYNCHRONE MODE)
        /// - Fermeture/Réouverture de positions
        /// </summary>
        /// <typeparam name="T">Pair(ARQTools.TradeKey, List(ARQTools.TradeCandidate))</typeparam>
        /// <param name="pParallelProcess">Parallel Process (ClosingReopeningCalculation|ClosingReopeningWriting)</param>
        /// <param name="pIdT">Id du trade</param>
        /// <param name="pSource">Trades candidats à Fermeture/Réouverture par clé de position</param>
        /// <param name="pUser">Utilisateur (SYSADM)</param>
        /// <param name="pCt">Notification d'annulation</param>
        // EG 20190318 New ClosingReopening position Step3
        // EG 20190613 [24683] Gestion ClosingReopenin (Writing)
        protected async Task<ProcessState> ClosingReopeningGenByPositionAsync(ParallelProcess pParallelProcess, KeyValuePair<ARQTools.TradeKey, List<ARQTools.TradeCandidate>> pSource, User pUser, CancellationToken pCt)
        {
            ProcessState processState = new ProcessState(ProcessStateTools.StatusSuccessEnum, Cst.ErrLevel.SUCCESS);
            ARQTools.TradeKey tradeKey = pSource.Key;
            string contract = (tradeKey.IdCCSpecified?tradeKey.IdCC.ToString():(tradeKey.IdDCSpecified?tradeKey.IdDC.ToString():"-"));
            string key = String.Format("(IDARQ:{0} IDI:{1} CONTRACT:{2} IDASSET:{3}/{4} DEALER: {5}/{6} CLEARER: {7}/{8} )",
                tradeKey.actionReference.IdARQ, tradeKey.IdI, contract, tradeKey.IdAsset, tradeKey.AssetCategory.ToString(),
                tradeKey.IdA_Dealer, tradeKey.IdB_Dealer, tradeKey.IdA_Clearer, tradeKey.IdB_Clearer);
            string wait = "START ClosingReopeningGenByPositionAsync Wait   : {0} ({1}) " + key;
            string release = "STOP  ClosingReopeningGenByPositionAsync Release: {0} ({1}) " + key;

            bool isSemaphoreSpecified = (null != ProcessBase.SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    AppInstance.TraceManager.TraceTimeBegin(pParallelProcess.ToString() + "ClosingReopeningGenByPositionAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Wait();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(wait, ProcessBase.SemaphoreAsync.CurrentCount, pParallelProcess.ToString()));
                    }
                    switch (pParallelProcess)
                    {
                        case ParallelProcess.ClosingReopeningCalculation:
                            processState = ClosingReopeningCalculation(tradeKey, pSource.Value);
                            break;
                        case ParallelProcess.ClosingReopeningWriting:
                            if (tradeKey.statusCalculationSpecified && (tradeKey.statusCalculation.Status == ProcessStateTools.StatusEnum.SUCCESS))
                                processState = ClosingReopeningWriting(tradeKey, pSource.Value, pUser);
                            else
                                processState = new ProcessState(ProcessStateTools.StatusWarningEnum, Cst.ErrLevel.CLOSINGREOPENINGREJECTED);
                            break;
                    }
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        ProcessBase.SemaphoreAsync.Release();
                        AppInstance.TraceManager.TraceVerbose(this, String.Format(release, ProcessBase.SemaphoreAsync.CurrentCount, pParallelProcess.ToString()));
                    }
                    AppInstance.TraceManager.TraceTimeEnd(pParallelProcess.ToString() + "ClosingReopeningGenByPositionAsync", key);
                }
            }, pCt);

            return processState;
        }
        #endregion CommonGenByTradeAsync

    }

    #region TradeData
    public abstract class TradeData : PosKeepingTrade
    {
        #region Members
        public IPosRequest posRequest;
        protected EFS_TradeLibrary m_TradeLibrary;
        // EG 20240115 [WI808] Upd by RptSideContainer abstract class
        protected RptSideProductContainer m_RptSideProductContainer;
        protected PosKeepingGenProcessBase m_Process;
        protected string m_Cs;
        protected DataTable m_DtPosition;
        protected DataTable m_DtPosActionDet;
        protected DataTable m_DtPosActionDet_Working;
        protected IProductBase m_Product;

        protected IPosKeepingData m_PosKeepingData;
        protected IPosKeepingKey m_PosKeepingKey;
        #endregion Members

        #region Accessors
        protected EventQuery EventQry { get { return m_Process.EventQry; } }
        protected DateTime MasterDtBusiness{ get { return m_Process.MasterPosRequest.DtBusiness; } }

        #region IsQuoteOk
        public bool IsQuoteOk
        {
            get
            {
                bool isOk;
                if (m_PosKeepingData.Asset.isOfficialCloseMandatory)
                    isOk = m_PosKeepingData.Asset.quoteSpecified;
                else if (m_PosKeepingData.Asset.isOfficialSettlementMandatory)
                    isOk = m_PosKeepingData.Asset.quoteReferenceSpecified;
                else
                    isOk = m_PosKeepingData.Asset.quoteSpecified || m_PosKeepingData.Asset.quoteReferenceSpecified;
                return isOk;
            }
        }
        #endregion IsQuoteOk

        #endregion Accessors
        #region Constructors
        public TradeData(PosKeepingGenProcessBase pProcess, IPosRequest pPosRequest)
        {
            m_Process = pProcess;
            m_Cs = m_Process.ProcessBase.Cs;
            posRequest = pPosRequest;
            m_Product = Tools.GetNewProductBase();
        }
        /// <summary>
        /// Constructeur d'initialisation des paramètres utilisé par
        /// - correction de position
        /// - transfert de position
        /// - dénouement manuel d'options
        /// </summary>
        // EG 20240115 [WI808] New : Harmonisation et réunification des méthodes
        public TradeData(PosKeepingGenProcessBase pProcess, IPosRequest pPosRequest, IPosKeepingData pPosKeepingData,
            (EFS_TradeLibrary TradeLibrary, DataTable Position, DataTable PosActionDet) pData)
            :this(pProcess, pPosRequest)
        {
            m_PosKeepingData = pPosKeepingData;
            m_TradeLibrary = pData.TradeLibrary;
            m_DtPosition = pData.Position;
            m_DtPosActionDet = pData.PosActionDet;
        }
        public TradeData(PosKeepingGenProcessBase pProcess, IPosRequest pPosRequest, IPosKeepingData pPosKeepingData, IPosKeepingKey pPosKeepingKey)
            :this(pProcess, pPosRequest)
        {
            m_PosKeepingData = pPosKeepingData;
            m_PosKeepingKey = pPosKeepingKey;
        }
        #endregion Constructors
        #region Methods

        #region DeserializeTrade
        public virtual void DeserializeTrade()
        {
            DeserializeTrade(null);
        }
        public virtual void DeserializeTrade(IDbTransaction pDbTransaction)
        {
            m_TradeLibrary = new EFS_TradeLibrary(m_Cs, pDbTransaction, idT);
        }
        #endregion DeserializeTrade
        #region GetIdPR
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        protected Nullable<int> GetIdPR()
        {
            Nullable<int> idPR = null;
            if (null != posRequest)
            {
                switch (posRequest.RequestType)
                {
                    case Cst.PosRequestTypeEnum.OptionAbandon:
                    case Cst.PosRequestTypeEnum.OptionNotExercised:
                    case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    case Cst.PosRequestTypeEnum.OptionAssignment:
                    case Cst.PosRequestTypeEnum.OptionExercise:
                    case Cst.PosRequestTypeEnum.PositionCancelation:
                        idPR = posRequest.IdPR;
                        break;
                }
            }
            return idPR;
        }
        #endregion GetIdPR
        #region Initialize
        public Cst.ErrLevel Initialize(int pIdT, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            return Initialize(null, pIdT, pPosRequestAssetQuote, pIsQuoteCanBeReaded);
        }
        // EG 20180326 [23769] Use DataReaderMapper.MapDataReaderRow
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel Initialize(IDbTransaction pDbTransaction, int pIdT, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            MapDataReaderRow mapDr = null;

            using (IDataReader dr = m_Process.SearchPosKeepingDataByTrade(pDbTransaction, Cst.UnderlyingAsset.ExchangeTradedContract, pIdT, pPosRequestAssetQuote))
            {
                mapDr = DataReaderExtension.DataReaderMapToSingle(dr);
            }

            Cst.ErrLevel ret;
            if (null != mapDr)
            {
                Cst.StatusBusiness statusBusiness = (Cst.StatusBusiness)StringToEnum.Parse(mapDr["IDSTBUSINESS"].Value.ToString(), Cst.StatusBusiness.UNDEFINED);
                Cst.StatusActivation statusActivation = (Cst.StatusActivation)StringToEnum.Parse(mapDr["IDSTACTIVATION"].Value.ToString(), Cst.StatusActivation.REGULAR);

                bool isStatusOk = (Cst.StatusBusiness.ALLOC == statusBusiness);
                // RD 20161212 [22660] Add condition m_PKGenProcess.IsEntry
                if (m_Process.PKGenProcess.IsEntry)
                {
                    isStatusOk &= (Cst.StatusActivation.REGULAR == statusActivation);
                }
                else
                {
                    switch (m_Process.MasterPosRequest.RequestType)
                    {
                        case Cst.PosRequestTypeEnum.RemoveAllocation:
                        case Cst.PosRequestTypeEnum.TradeSplitting:
                            //Autorisation de traitement à partir d'un trade Verrouillé
                            isStatusOk &= ((Cst.StatusActivation.REGULAR == statusActivation)
                                           || (Cst.StatusActivation.LOCKED == statusActivation));
                            break;
                        default:
                            isStatusOk &= (Cst.StatusActivation.REGULAR == statusActivation);
                            break;
                    }
                }

                if (isStatusOk)
                {
                    bool isPosKeepingBook = true;
                    if (m_Process.PKGenProcess.IsEntry)
                    {
                        isPosKeepingBook = Convert.ToBoolean(mapDr["POSKEEPBOOK_DEALER"].Value);
                    }
                    if (isPosKeepingBook)
                    {
                        ret = SetPosKeepingData(pDbTransaction, mapDr, pPosRequestAssetQuote, pIsQuoteCanBeReaded);
                    }
                    else
                    {
                        ret = Cst.ErrLevel.DATAIGNORE;
                    }
                }
                else
                {
                    string identifier = mapDr["IDENTIFIER"].ToString();
                    
                    //string sysCode = string.Empty;
                    SysMsgCode sysCode;
                    if (Cst.StatusActivation.REGULAR != statusActivation)
                    {
                        //sysCode = "SYS-05105"; //DEACTIV-LOCKED
                        sysCode = new SysMsgCode(SysCodeEnum.LOG, 5105); //DEACTIV-LOCKED
                        if (Cst.StatusActivation.MISSING == statusActivation)
                        {
                            //sysCode = "SYS-05106";
                            sysCode = new SysMsgCode(SysCodeEnum.LOG, 5106);
                        }

                        m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                        
                        Logger.Log(new LoggerData(LogLevelEnum.Error, sysCode, 0,
                            new LogParam(LogTools.IdentifierAndId(identifier, pIdT)),
                            new LogParam(statusActivation)));
                    }
                    if (false == m_Process.PKGenProcess.IsEntryOrRequestUpdateEntry)
                    {
                        if (Cst.StatusBusiness.ALLOC != statusBusiness)
                        {
                            m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5107), 0,
                                new LogParam(LogTools.IdentifierAndId(identifier, pIdT)),
                                new LogParam(statusBusiness)));
                        }
                    }
                    ret = Cst.ErrLevel.DATAIGNORE;
                }
            }
            else
            {
                m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5104), 0, new LogParam(pIdT)));

                ret = Cst.ErrLevel.DATAIGNORE;
            }
            return ret;
        }
        #endregion Initialize

        #region InsertNominalQuantityEvent
        protected Cst.ErrLevel InsertNominalQuantityEvent(IDbTransaction pDbTransaction, int pIdT, ref int pIdE, int pIdE_Event,
            DateTime pDtBusiness, bool pIsDealerBuyer, decimal pQty, decimal pRemainQty, int pIdPADET)
        {

            #region EVENT NOM/QTY
            string eventCode = (pRemainQty == 0) ? EventCodeFunc.Termination : EventCodeFunc.Intermediary;
            string eventType = EventTypeFunc.Nominal;

            int idA_Payer;
            int idB_Payer;
            int idA_Receiver;
            int idB_Receiver;
            if (pIsDealerBuyer)
            {
                idA_Payer = m_PosKeepingData.IdA_Clearer;
                idB_Payer = m_PosKeepingData.IdB_Clearer;
                idA_Receiver = m_PosKeepingData.IdA_Dealer;
                idB_Receiver = m_PosKeepingData.IdB_Dealer;
            }
            else
            {
                idA_Payer = m_PosKeepingData.IdA_Dealer;
                idB_Payer = m_PosKeepingData.IdB_Dealer;
                idA_Receiver = m_PosKeepingData.IdA_Clearer;
                idB_Receiver = m_PosKeepingData.IdB_Clearer;
            }
            Nullable<decimal> nominalValue = m_PosKeepingData.NominalValue(pQty);
            // Sur l'évènement de Nominal Spheres® insère la devise du nominal  
            Cst.ErrLevel codeReturn = EventQry.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Payer, idB_Payer, idA_Receiver, idB_Receiver,
                eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness,
                nominalValue, m_PosKeepingData.Asset.nominalCurrency, UnitTypeEnum.Currency.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                _ = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            pIdE++;
            eventType = EventTypeFunc.Quantity;
            codeReturn = EventQry.InsertEvent(pDbTransaction, pIdT, pIdE, pIdE_Event, null, 1, 1, idA_Receiver, idB_Receiver, idA_Payer, idB_Payer,
                eventCode, eventType, pDtBusiness, pDtBusiness, pDtBusiness, pDtBusiness, pQty, null, UnitTypeEnum.Qty.ToString(), null, null);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQry.InsertEventClass(pDbTransaction, pIdE, EventClassFunc.Recognition, pDtBusiness, false);
            if (Cst.ErrLevel.SUCCESS == codeReturn)
                codeReturn = EventQuery.InsertEventPosActionDet(pDbTransaction, pIdPADET, pIdE);

            #endregion EVENT NOM/QTY

            return codeReturn;
        }
        #endregion InsertNominalQuantityEvent


        #region PosActionDetCalculation
        // EG 20180530 [23980] Upd change qty by posRequest.qty
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20240115 [WI808] Harmonisation et réunification des méthodes
        public Cst.ErrLevel PosActionDetCalculation(IDbTransaction pDbTransaction)
        {
            //IPosRequestDetOption detail = posRequest.DetailBase as IPosRequestDetOption;
            // decimal availableQty = detail.AvailableQty;

            (decimal qty, decimal availableQty, IPosRequestDetail detail) = posRequest.GetDetailForAction;
            DataRow rowTradeClosing = m_DtPosition.Rows.Find(posRequest.IdT);

            Cst.ErrLevel codeReturn;
            if (null != rowTradeClosing)
            {
                decimal closableQty = Convert.ToDecimal(rowTradeClosing["QTY"]);
                if ((qty <= closableQty) && (0 < qty))
                {
                    int idT_Closing = Convert.ToInt32(rowTradeClosing["IDT"]);
                    string sideClosing = rowTradeClosing["SIDE"].ToString();
                    decimal closingQty = Math.Min(posRequest.Qty, closableQty);

                    // Insertion de la clôture dans la dataTable m_DtPosActionDet_Working
                    DataRow newRow = m_DtPosActionDet.NewRow();
                    newRow.BeginEdit();
                    newRow["IDPA"] = 0;
                    newRow["IDT_CLOSING"] = idT_Closing;
                    newRow["SIDE_CLOSING"] = sideClosing;
                    if (m_Process.IsTradeBuyer(sideClosing))
                    {
                        newRow["IDT_BUY"] = idT_Closing;
                        newRow["IDT_SELL"] = Convert.DBNull;
                    }
                    else
                    {
                        newRow["IDT_SELL"] = idT_Closing;
                        newRow["IDT_BUY"] = Convert.DBNull;
                    }
                    newRow["QTY"] = closingQty;
                    newRow["ORIGINALQTY_CLOSING"] = closableQty;
                    newRow["ORIGINALQTY_CLOSED"] = closableQty;
                    newRow["POSITIONEFFECT"] = Convert.DBNull;
                    newRow.EndEdit();
                    rowTradeClosing.BeginEdit();
                    rowTradeClosing["QTY"] = closableQty - closingQty;
                    rowTradeClosing.EndEdit();
                    m_DtPosActionDet.Rows.Add(newRow);

                    SetPaymentFeesForEvent(pDbTransaction, idT_Closing, closingQty);

                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
                else if (0 < qty)
                {
                    // ds le message d'erreur contient closableQty (véritable quantité disponible) plutôt que pAvailableQty (quantité disponible au moment de l'action)
                    codeReturn = Cst.ErrLevel.FAILURE;
                    m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5150), 0,
                        new LogParam(m_Process.GetPosRequestLogValue(posRequest.RequestType)),
                        new LogParam(LogTools.IdentifierAndId(rowTradeClosing["IDENTIFIER"].ToString(), idT)),
                        new LogParam(qty),
                        new LogParam(closableQty),
                        new LogParam(DtFunc.DateTimeToStringDateISO(MasterDtBusiness))));
                }
                else
                {
                    codeReturn = Cst.ErrLevel.SUCCESS;
                }
            }
            else
            {
                codeReturn = Cst.ErrLevel.DATAIGNORE;
                m_Process.PKGenProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                
                
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 5163), 0,
                    new LogParam(m_Process.GetPosRequestLogValue(posRequest.RequestType)),
                    new LogParam(LogTools.IdentifierAndId(identifier, idT)),
                    new LogParam(qty),
                    new LogParam(availableQty),
                    new LogParam(DtFunc.DateTimeToStringDateISO(MasterDtBusiness))));
            }
            return codeReturn;
        }
        #endregion PosActionDetCalculation

        #region SetPosKeepingData
        // EG 20180326 [23769] Use MapDataReaderRow
        protected virtual Cst.ErrLevel SetPosKeepingData(IDbTransaction pDbTransaction, MapDataReaderRow pMapDr, Cst.PosRequestAssetQuoteEnum pPosRequestAssetQuote, bool pIsQuoteCanBeReaded)
        {
            return Cst.ErrLevel.FAILURE;
        }
        #endregion SetPosKeepingData
        #region SetPosKeepingMarket
        /// <summary>
        /// Alimentation des données EntityMarket 
        /// </summary>
        protected void SetPosKeepingMarket()
        {
            m_PosKeepingData.Market = m_Process.PKGenProcess.ProcessCacheContainer.GetEntityMarketLock(m_Process.MarketPosRequest.IdEM);
            m_PosKeepingData.MarketSpecified = (null != m_PosKeepingData.Market);
        }
        #endregion SetPosKeepingMarket
        #region SetPaymentFeesForEvent
        // EG 20240115 [WI808] Upd : Harmonisation et réunification des méthodes
        public virtual void SetPaymentFeesForEvent(IDbTransaction pDbTransaction, int pIdT, decimal pQty)
        {
            (decimal qty, decimal availableQty, IPosRequestDetail detail) = posRequest.GetDetailForAction;
            if (m_Process is PosKeepingGen_ETD)
            {
                IExchangeTradedDerivative etd = (IExchangeTradedDerivative)m_TradeLibrary.Product;
                m_RptSideProductContainer = new ExchangeTradedDerivativeContainer(m_Cs, pDbTransaction, etd);
                ExchangeTradedDerivativeContainer etdContainer = m_RptSideProductContainer as ExchangeTradedDerivativeContainer;
                etdContainer.TradeCaptureReport.LastQty = new EFS_Decimal(pQty);
                etdContainer.Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(m_Cs, pDbTransaction,
                etdContainer.ExchangeTradedDerivative, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC, pIdT);
            }
            else if (m_Process is PosKeepingGen_SEC)
            {
                if (m_TradeLibrary.Product.ProductBase.IsESE)
                {
                    m_RptSideProductContainer = new EquitySecurityTransactionContainer(m_Cs, pDbTransaction, (IEquitySecurityTransaction)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
                    EquitySecurityTransactionContainer estContainer = m_RptSideProductContainer as EquitySecurityTransactionContainer;
                    estContainer.TradeCaptureReport.LastQty = new EFS_Decimal(pQty);
                    estContainer.Efs_EquitySecurityTransaction = new EFS_EquitySecurityTransaction(m_Cs, estContainer.EquitySecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                }
                else if (m_TradeLibrary.Product.ProductBase.IsDebtSecurityTransaction)
                {
                    m_RptSideProductContainer = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)m_TradeLibrary.Product, m_TradeLibrary.DataDocument);
                    DebtSecurityTransactionContainer dstContainer = m_RptSideProductContainer as DebtSecurityTransactionContainer;
                    dstContainer.DebtSecurityTransaction.Efs_DebtSecurityTransactionAmounts = new EFS_DebtSecurityTransactionAmounts(m_Cs, dstContainer.DebtSecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                    dstContainer.DebtSecurityTransaction.Efs_DebtSecurityTransactionStream = new EFS_DebtSecurityTransactionStream(m_Cs, dstContainer.DebtSecurityTransaction, m_TradeLibrary.DataDocument, Cst.StatusBusiness.ALLOC);
                    dstContainer.SetOrderQuantityForAction(pQty);
                }
            }

            if (ArrFunc.IsFilled(detail.PaymentFees))
            {
                EventQuery.InitPaymentForEvent(m_Cs, pDbTransaction, detail.PaymentFees, m_TradeLibrary.DataDocument, out int nbEvent);
                detail.NbAdditionalEvents += nbEvent;
            }
        }
        #endregion SetPaymentFeesForEvent
        #region SetPosKeepingTrade
        // EG 20180326 [23769] Use MapDataReaderRow
        protected virtual Cst.ErrLevel SetPosKeepingTrade(MapDataReaderRow pMapDr)
        {
            int idT = Convert.ToInt32(pMapDr["IDT"].Value);
            string identifier = pMapDr["IDENTIFIER"].Value.ToString();
            int side = Convert.ToInt32(pMapDr["SIDE"].Value);
            decimal qty = Convert.ToDecimal(pMapDr["QTY"].Value);
            DateTime dtBusiness = Convert.ToDateTime(pMapDr["DTBUSINESS"].Value);
            DateTime dtExecution = Convert.ToDateTime(pMapDr[Convert.IsDBNull(pMapDr["DTEXECUTION"].Value) ? "DTTIMESTAMP" : "DTEXECUTION"].Value);
            string positionEffect = pMapDr["POSITIONEFFECT"].Value.ToString();
            DateTime dtSettlement = Convert.ToDateTime(pMapDr["DTSETTLT"].Value);
            base.Set(idT, identifier, side, qty, dtBusiness, positionEffect, dtExecution, dtSettlement);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion SetPosKeepingTrade

        #endregion Methods
    }
    #endregion TradeData
}
