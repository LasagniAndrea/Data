using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Process.EventsGen;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.SpheresRiskPerformance.Properties;
using EFS.TradeInformation;

namespace EFS.SpheresRiskPerformance.CashBalance
{
    public partial class CBTradeInfo : RiskTradeInfo
    {
        #region Methods (Used in Asynchrone mode)
        /// <summary>
        /// Alimentation du log dans la création|mise à jour des trades Cash-Balance
        /// </summary>
        /// <param name="pTaskId"></param>
        /// <param name="pCounter"></param>
        /// <param name="pTotal"></param>
        // EG 20180205 [23769] New
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void SetLog(int pTaskId, int pCounter, int pTotal)
        {
            // FI 20200731 [XXXXX]  Modification du log en fin de traitement d'un couple Actor/Book (4032 à la place de 4031)
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4032), 2,
                new LogParam(LogTools.IdentifierAndId(PartyTradeInfo.Identifier_a, PartyTradeInfo.Ida)),
                new LogParam(LogTools.IdentifierAndId(PartyTradeInfo.Identifier_b, PartyTradeInfo.Idb)),
                new LogParam(String.Format("Task id : {0} - ", pTaskId) + pCounter.ToString()),
                new LogParam(pTotal)));

            if (Cst.ErrLevel.SUCCESS == status)
            {
                
                //string msg = (tradeResult.captureMode == Cst.Capture.ModeEnum.New) ? "LOG-04050" : "LOG-04051";
                SysMsgCode msg = (tradeResult.captureMode == Cst.Capture.ModeEnum.New) ? new SysMsgCode(SysCodeEnum.LOG, 4050) : new SysMsgCode(SysCodeEnum.LOG, 4051);
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, msg, 3, new LogParam(LogTools.IdentifierAndId(tradeResult.identifier, tradeResult.idT.ToString()))));
            }
            else if (Cst.ErrLevel.NOTHINGTODO == status)
            {
                // PM 20190701 [24761] Ajout message pour le cas où le CB n'a pas été modifié
                if (tradeResult != default(TradeResult))
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4053), 3, new LogParam(LogTools.IdentifierAndId(tradeResult.identifier, tradeResult.idT.ToString()))));
                }
                else
                {
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4040), 3));
                }

            }
        }
        #endregion Methods (Used in Asynchrone mode)
    }

    public partial class CashBalanceProcess
    {
        /// <summary>
        /// Méthode principale d'appel à la création ou modification des trades CB
        /// Découpage du traitement de génération des trade CB par tronçon (ChunckSize)
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pLstPartyTradeInfo">Liste des couples (Actor,Book) vis à vis desquels les trades Cash-Blance seront générés</param>
        /// <returns></returns>
        // EG 20180205 [23769] New
        // EG 20180409 [23769] Upd
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20201016 [XXXXX] Gestion Code retour de l'appel CreateResultAsync et mise à jour du Statut
        private Cst.ErrLevel CashBalanceThreading(List<CBPartyTradeInfo> pLstPartyTradeInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            m_Timers.CreateTimer("GenerateActorBookTrade");

            m_Trade_CountTotal = pLstPartyTradeInfo.Count;
            m_Trade_CountNotProcessed = 0;
            m_Trade_CountProcessedSuccess = 0;
            m_Trade_CountProcessedError = 0;
            m_Trade_CountProcessedUnchanged = 0;

            if (m_Trade_CountTotal > 0)
            {
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4030), 2, new LogParam(m_Trade_CountTotal.ToString())));
                Logger.Write();

                // Découpage du traitement de génération des trade CB par tronçon de n
                AppInstance.AppTraceManager.TraceInformation(this, "START CreateResultsAsync CB");

                CancellationTokenSource cts = new CancellationTokenSource();
                RiskTemplateInfo riskTemplateInfo = new RiskTemplateInfo(IdMenu.Menu.InputTradeRisk_CashBalance, Cst.ProductCashBalance, this);

                int heapSize = GetHeapSize(ParallelProcess.CashBalance);
                List<List<CBPartyTradeInfo>> subLstPartyTradeInfos = ListExtensionsTools.ChunkBy(pLstPartyTradeInfo.ToList(), heapSize);

                int counter = 1;
                int totSubList = subLstPartyTradeInfos.Count();
                int prevSubList = 0;
                subLstPartyTradeInfos.ForEach(lstPartyTradeInfos =>
                {
                    if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                        (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                    {

                        InitializeMaxThreshold(ParallelProcess.CashBalance);
                        Task<ProcessState> task = CreateResultsAsync(Cs, lstPartyTradeInfos, riskTemplateInfo, prevSubList, cts);
                        task.Wait();
                        ProcessState.SetErrorWarning(task.Result.Status, task.Result.CodeReturn);

                        
                        prevSubList += lstPartyTradeInfos.Count();

                        Common.AppInstance.TraceManager.TraceTimeSummary("CashBalanceTradeGenAsync", StrFunc.AppendFormat("{0}/{1}", counter, totSubList));
                        counter++;
                    }
                });
                Common.AppInstance.TraceManager.TraceInformation(this, "END CreateResultsAsync CB");
            }
            return ret;
        }

        /// <summary>
        /// Génération des trades Cash-Balance
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pLstPartyTradeInfo">Liste des couples (Actor,Book) vis à vis desquels les trades Cash-Blance seront générés</param>
        /// <param name="pRiskTemplateInfo">Informations de base communes à tous les trades (Template, Session, User, etc.)</param>
        /// <param name="pPreviousCounter">Compteur de génération (lié au tronçon "pLstPartyTradeInfo" précédement traité</param>
        /// <param name="pCts">Source de la notification d'annulation des tâches (Unused for the moment)</param>
        /// <returns></returns>
        // EG 20180205 [23769] New 
        // EG 20180525 [23979] IRQ Processing
        // EG 20201016 [XXXXX] Changement retour de l'appel et le traityement continue tout de même si une tâche est en erreur
        private async Task<ProcessState> CreateResultsAsync(string pCS, List<CBPartyTradeInfo> pLstPartyTradeInfo,
            RiskTemplateInfo pRiskTemplateInfo, int pPreviousCounter, CancellationTokenSource pCts)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ProcessState processState = new ProcessState(ProcessStateTools.StatusEnum.SUCCESS, ret);
            try
            {
                Common.AppInstance.TraceManager.TraceInformation(this, "START List of CBTradeInfo and SetEndOfDayStatus");
                List<CBTradeInfo> lstCBTradeInfo = SetListCBTradeInfo(pCS, pLstPartyTradeInfo);
                Common.AppInstance.TraceManager.TraceInformation(this, "STOP List of CBTradeInfo and SetEndOfDayStatus");

                #region Génération des trades Cash-Balance
                List<Task<CBTradeInfo>> getReturnTasks = null;

                IEnumerable<Task<CBTradeInfo>> getReturnTasksQuery =
                    from tradeInfo in lstCBTradeInfo select CreateResultAsync(pCS, tradeInfo, pRiskTemplateInfo, pCts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();
                int counter = 0;
                while (0 < getReturnTasks.Count)
                {
                    // On s'arrête sur la première tâche complétée.  
                    Task<CBTradeInfo> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    // On supprime la tâche de la liste des tâches pour éviter que le process ne la traite une nouvelle fois.  
                    getReturnTasks.Remove(firstFinishedTask);
                    counter++;

                    // On rentre ici exceptionnellement (si exception non gérée) ou interruption de traitement
                    // Dans la méthode CreateResult quasi toute exception rencontrée lors de la création du trade est trappée pour produire un log 
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CreateResultsAsync", processState);


                    CBTradeInfo tradeInfo = firstFinishedTask.Result;
                    tradeInfo.SetLog(firstFinishedTask.Id, counter + pPreviousCounter, m_Trade_CountTotal);
                    
                    Logger.Write();

                    if (IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref processState))
                        pCts.Cancel(true);
                }
                #endregion Génération des trades Cash-Balance

                if ((Cst.ErrLevel.IRQ_EXECUTED != processState.CodeReturn) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref processState)))
                {
                    #region Génération des événements
                    // On génére les EVTS en ASYNCHRONE
                    if (EventGenModeEnum.Internal == this.m_EventGenMode)
                    {
                        // FI 20200908 [XXXXX] Call CreateInternalEventsAsync
                        CreateInternalEventsAsync(ParallelProcess.CashBalance, lstCBTradeInfo.Cast<RiskTradeInfo>());
                    }
                    else if (EventGenModeEnum.External == this.m_EventGenMode)
                    {
                        // PM 20190701 [24761] Ajout code de retour
                        //CreateExternalEventsAsync(pCS, lstCBTradeInfo);
                        if (CreateExternalEventsAsync(pCS, lstCBTradeInfo) == Cst.ErrLevel.SUCCESS)
                        {
                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4061), 3));
                            Logger.Write();
                        }
                    }
                    #endregion Génération des événements
                }
            }
            catch (AggregateException)
            {
                if (processState.CodeReturn != Cst.ErrLevel.IRQ_EXECUTED)
                {
                    pCts.Cancel(true);
                    throw;
                }

            }
            catch (Exception)
            {
                pCts.Cancel(true);
                throw;
            }
            finally
            {
                Common.AppInstance.TraceManager.TraceInformation(this, "STOP CreateResultsAsync CB");

                Logger.Write();

                m_CBRequest.SynchronizeDatabase(Cs, null);
            }
            return processState;
        }

        /// <summary>
        /// Alimentation des caches pour la génération des trades Cash-Balance
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pLstPartyTradeInfo">Liste des couples (Actor,Book) vis à vis desquels les trades Cash-Blance seront générés</param>
        /// <returns></returns>
        // EG 20180205 [23769] New
        private List<CBTradeInfo> SetListCBTradeInfo(string pCS, List<CBPartyTradeInfo> pLstPartyTradeInfo)
        {
            List<CBTradeInfo> lstCBTradeInfo = new List<CBTradeInfo>();
            pLstPartyTradeInfo.ForEach(partyTradeInfo =>
            {
                CBTradeInfo tradeInfo = new CBTradeInfo(m_CBHierarchy.Ida_Entity, m_CBHierarchy.Identifier_Entity, m_CBHierarchy.DtBusiness,
                    m_CBHierarchy.DtBusinessPrev, m_CBHierarchy.Timing, partyTradeInfo);

                tradeInfo.InitDelegate(ProcessState.SetErrorWarning);
                tradeInfo.InitializeAmounts(this.m_ProcessInfo.DtBusiness);

                // Chargement des status des traitements de fin de journée des marchés/cssCustodian 
                SetEndOfDayStatus(pCS, tradeInfo);

                // Alimentation du cache de tous les DC/Asset d'un ensemble de flux
                m_CBCache.AddAssetInfoFromCashFlows(pCS, null, tradeInfo.CashFlowAll_FlowCur.CashFlows);

                // Alimentation du cache avec les assets présents dans les dépôts de garantie
                m_CBCache.AddAssetInfoFromCollateral(pCS, null, tradeInfo.FlowCollateral);

                // Alimentation du cache avec les informations sur les devises
                m_CBCache.AddCurCashInfoFromCashFlows(pCS, null, tradeInfo.CashFlowAll_FlowCur.CashFlows);

                lstCBTradeInfo.Add(tradeInfo);
            });

            return lstCBTradeInfo;
        }

        /// <summary>
        /// Mise en attente d'une création de trade Cash-Balance
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeInfo">Représente un couple (CBO/MRO, Book) et tous les montants, qui seront injectés dans le Trade Cash-Balance final</param>
        /// <param name="pRiskTemplateInfo">Informations de base communes à tous les trades (Template, Session, User, etc.)</param>
        /// <param name="pCt">Propagation de la notification en cas d'annulation de tâches (Unused for the moment)</param>
        /// <param name="pCt"></param>
        /// <returns></returns>
        // EG 20180205 [23769] New
        private async Task<CBTradeInfo> CreateResultAsync(string pCS, CBTradeInfo pTradeInfo, RiskTemplateInfo pRiskTemplateInfo, CancellationToken pCt)
        {
            string key = String.Format("(Actor: {0} Book: {1})", 
                LogTools.IdentifierAndId(pTradeInfo.PartyTradeInfo.Identifier_a, pTradeInfo.PartyTradeInfo.Ida),
                LogTools.IdentifierAndId(pTradeInfo.PartyTradeInfo.Identifier_b, pTradeInfo.PartyTradeInfo.Idb));

            string wait = "START CreateResultAsync Wait   : {0} " + key;
            string release = "STOP  CreateResultAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != SemaphoreAsync);
            await Task.Run(() =>
            {
                try
                {
                    Common.AppInstance.TraceManager.TraceTimeBegin("CashBalanceTradeGenAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Wait();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(wait, SemaphoreAsync.CurrentCount));
                    }
                    CreateResult(pCS, pTradeInfo, pRiskTemplateInfo);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Release();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(release, SemaphoreAsync.CurrentCount));
                    }
                    Common.AppInstance.TraceManager.TraceTimeEnd("CashBalanceTradeGenAsync", key); 
                }
            } , pCt);

            return pTradeInfo;
        }
        /// <summary>
        /// Génération d'un trade CB : Calculs
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pTradeInfo">Représente un couple (CBO/MRO, Book) et tous les montants, qui seront injectés dans le Trade Cash-Balance final</param>
        /// <param name="pRiskTemplateInfo">Informations de base communes à tous les trades (Template, Session, User, etc.)</param>
        /// <returns></returns>
        /// EG 20180205 [23769] New
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20200722 [XXXXX] Refactoring
        /// FI 20200723 [XXXXX] Refactoring
        private void CreateResult(string pCS, CBTradeInfo pTradeInfo, RiskTemplateInfo pRiskTemplateInfo)
        {
            bool isException = false;
            try
            {
               
                if (null == pTradeInfo)
                    throw new ArgumentNullException($"{nameof(pTradeInfo)} argument is null");

                m_CBRequest.UpdateCBRequestPartyTradeInfoRow(pTradeInfo.PartyTradeInfo, ProcessStateTools.StatusProgressEnum, 0);

                // FI 20231123 [WI748] transaction à null => A l'avenir, il ne faut plus ouvrir de transaction puisque qu'un tryMultiple est en vigueur dans TradeCommonCaptureGen.ErrorLevel RecordTradeRisk
                IDbTransaction dbTransaction = null;
                try
                {
                    // FI 20200722 [XXXXX] call SetSqlActorBook puisque pTradeInfo.sqlActor est utilisé si Exception
                    pTradeInfo.SetSqlActorBook(CSTools.SetCacheOn(pCS), dbTransaction);

                    // Arrondi des flux lus
                    lock (m_CBCacheLock)
                    {
                        pTradeInfo.RoundDetailledAmounts(pCS, dbTransaction, m_CBCache);
                        pTradeInfo.status = pTradeInfo.ProcessAmounts(pCS, dbTransaction);
                    }

                    if (pTradeInfo.status != Cst.ErrLevel.SUCCESS)
                    {
                        // Erreur de calcul de l'Appel de marge ( Exemple: Collateral non valorisé,...)
                        throw new SpheresException2("CreateResult", String.Format("Status on ERROR : {0}", pTradeInfo.status.ToString()));
                    }
                    else
                    {
                        if (this.LogDetailEnum == LogLevelDetail.LEVEL4)
                            m_CBHierarchy.Add(pTradeInfo);

                        // S'il existe au moins un montant différent de zéro, alors Création ou Mise à jour du Trade avec les différents montants
                        // Sinon 
                        //      Si le Trade existe alors Mise à jour du Trade existant avec les montants à zéro
                        //      Sinon Ne rien faire
                        CreateTradeCashBalance(pCS, dbTransaction, pTradeInfo, pRiskTemplateInfo);
                        if (null != dbTransaction)
                            DataHelper.CommitTran(dbTransaction);
                    }
                }
                catch (Exception ex)
                {
                    isException = true;
                    OnCreateResultException(ex, pTradeInfo);
                    if ((null != dbTransaction) && DataHelper.IsTransactionValid(dbTransaction))
                        DataHelper.RollbackTran(dbTransaction);
                    
                    // Remarque pas de throw => Le traitement continue
                }
            }
            catch (Exception ex)
            {
                isException = true;
                OnCreateResultException(ex, pTradeInfo);
                throw; // throw => interruption du traitement
            }
            finally
            {
                if (isException)
                {
                    m_Trade_CountProcessedError++;
                    m_CBRequest.UpdateCBRequestPartyTradeInfoRow(pTradeInfo.PartyTradeInfo, ProcessStateTools.StatusErrorEnum, 0);
                    pTradeInfo.status = Cst.ErrLevel.ABORTED;
                }
            }
        }

        /// <summary>
        ///  Traitement en cas d'exption sur la méthode CreateResult
        ///  <para>Alimentation du statut final du traitement et alimentation des logs  </para>
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="pTradeInfo"></param>
        /// FI 20200723 [XXXXX] Add method
        private void OnCreateResultException(Exception ex, CBTradeInfo pTradeInfo)
        {

            ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;

            // FI 20200623 [XXXXX] AddCriticalException 
            ProcessState.AddCriticalException(ex);

            // FI 20200623 [XXXXX] SetErrorWarning
            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

            string logActor = LogTools.IdentifierAndId(pTradeInfo.PartyTradeInfo.Identifier_a, pTradeInfo.PartyTradeInfo.Ida);
            string logBook = LogTools.IdentifierAndId(pTradeInfo.PartyTradeInfo.Identifier_b, pTradeInfo.PartyTradeInfo.Idb);
            string logEntity = LogTools.IdentifierAndId(m_CBHierarchy.Identifier_Entity, m_CBHierarchy.Ida_Entity);
            Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4065), 0,
                new LogParam(logActor), new LogParam(logBook), new LogParam(logEntity),
                new LogParam(DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness))));
        }


        #region CreateTradeCashBalance
        /// <summary>
        /// Génération d'un trade CB : Création|Mise à jour
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pTradeInfo">Représente un couple (CBO/MRO, Book) et tous les montants, qui seront injectés dans le Trade Cash-Balance final</param>
        /// <param name="pRiskTemplateInfo">Informations de base communes à tous les trades (Template, Session, User, etc.)</param>
        // EG 20180205 [23769] New 
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private void CreateTradeCashBalance(string pCS, IDbTransaction pDbTransaction, CBTradeInfo pTradeInfo, RiskTemplateInfo pRiskTemplateInfo)
        {
            bool saveCB = true;

            CheckTradeExist(pCS, pDbTransaction, pTradeInfo.PartyTradeInfo.Ida, pTradeInfo.PartyTradeInfo.Idb, out int opIdT);
            if (pTradeInfo.IsAmountFilled || (0 < opIdT))
            {
                if (pTradeInfo.SqlEntity.Id != pTradeInfo.SqlBook.IdA_Entity)
                {
                    #region The processing entity is different from the Book accounting entity
                    SQL_Actor sqlBookEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), pTradeInfo.SqlBook.IdA_Entity)
                    {
                        DbTransaction = pDbTransaction
                    };
                    sqlBookEntity.LoadTable();

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat(@"<b>Calculation of Margin Call and Cash-Balance not made.</b>
<b>Cause:</b> The processing entity is different from the Book accounting entity.
<b>Action:</b> Please specify the correct accounting entity on the Book and re-launch the process.
<b>Details:</b>
- Office: <b>{0}</b>
- Book: <b>{1}</b>
- Book accounting entity: <b>{2}</b>
- Processing entity: <b>{3}</b>
- Clearing date: <b>{4}</b>",
                     LogTools.IdentifierAndId(pTradeInfo.SqlActor.Identifier, pTradeInfo.SqlActor.Id),
                     LogTools.IdentifierAndId(pTradeInfo.SqlBook.Identifier, pTradeInfo.SqlBook.Id),
                     LogTools.IdentifierAndId(sqlBookEntity.Identifier, sqlBookEntity.Id),
                     LogTools.IdentifierAndId(pTradeInfo.SqlEntity.Identifier, pTradeInfo.SqlEntity.Id),
                     DtFunc.DateTimeToStringDateISO(pTradeInfo.DtBusiness)));
                    #endregion The processing entity is different from the Book accounting entity
                }

                #region Create new trade ou update existing trade

                int idTSource = 0;
                string screenName = string.Empty;
                string csLoad = pCS;

                TradeRiskCaptureGen captureGen = new TradeRiskCaptureGen();
                captureGen.TradeCommonInput.TradeLink = new List<TradeLink.TradeLink>();

                Nullable<Cst.Capture.ModeEnum> captureMode = null;

                if (0 < opIdT)
                {
                    captureMode = Cst.Capture.ModeEnum.Update;
                    idTSource = opIdT;
                }
                else
                {
                    captureMode = Cst.Capture.ModeEnum.New;
                    screenName = pRiskTemplateInfo.ScreenTemplate;
                    idTSource = pRiskTemplateInfo.IdTTemplate;
                    csLoad = CSTools.SetCacheOn(csLoad);
                }

                bool isFound = captureGen.Load(csLoad, pDbTransaction, idTSource, (Cst.Capture.ModeEnum)captureMode, pRiskTemplateInfo.InputUser.User, pRiskTemplateInfo.SessionInfo.session.SessionId, false);
                if (false == isFound)
                {
                    throw new InvalidOperationException(StrFunc.AppendFormat("<b>trade [idT:{0}] not found</b>", idTSource));
                }

                captureGen.InitBeforeCaptureMode(pCS, pDbTransaction, pRiskTemplateInfo.InputUser, pRiskTemplateInfo.SessionInfo);

                CBDataDocument.SetDataDocument(pCS, pDbTransaction, captureGen.TradeCommonInput.DataDocument, pTradeInfo, m_CBCache);

                // PM 20190701 [24761] Ajout vérification de changement dans le Cash Balance en mode Update
                if ((captureMode == Cst.Capture.ModeEnum.Update) && Settings.Default.VerifyUnchangedCashBalance)
                {
                    saveCB = IsCashBalanceChanged(pCS, pDbTransaction, opIdT, captureGen.TradeCommonInput.DataDocument);
                    if (false == saveCB)
                    {
                        // PM 20201215[XXXXX] Forcer l'écriture du CashBalance, car il n'a pas d'événement de généré
                        saveCB = (true != IsEventExist(pCS, pDbTransaction, opIdT));
                    }

                }
                if (saveCB)
                {
                    captureGen.TradeCommonInput.SetTradeNotification(true);
                    captureGen.TradeCommonInput.TradeStatus.stEnvironment.NewSt =
                        CalculationSheetRepository.RiskEvaluationModeToStatusEnvironment(this.m_ProcessInfo.Mode).ToString();

                    if (captureMode == Cst.Capture.ModeEnum.Update)
                    {
                        screenName = captureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;
                    }

                    CreateTradeLinkCashBalance(captureGen, pTradeInfo);

                    TradeRecordSettings recordSettings = new TradeRecordSettings()
                    {
                        displayName = string.Empty, // Sera identique à identifier
                        description = StrFunc.AppendFormat("Cash-Balance [{0}/{1}]", DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness), pTradeInfo.SqlActor.Identifier),
                        extLink = string.Empty,
                        idScreen = screenName,
                        isGetNewIdForIdentifier = true,
                        isCheckValidationRules = false,
                        isCheckValidationXSD = false,
                        isCheckLicense = false,
                        isCopyAttachedDoc = false,
                        isCopyNotePad = false,
                        isUpdateTradeXMLWithTradeLink = false,
                        isSaveUnderlyingInParticularTransaction = false
                    };

                    
                    //TradeCommonCaptureGen.ErrorLevel errLevel =
                    //RiskPerformanceTools.RecordTradeRisk(pCS, pDbTransaction, captureGen, recordSettings, pRiskTemplateInfo.sessionInfo,
                    //(Cst.Capture.ModeEnum)captureMode, pRiskTemplateInfo.inputUser.IdMenu,
                    //this.logDetailEnum, pTradeInfo.Log, this.processLog.AddAttachedDoc, this.tracker.idTRK_L, this);
                    TradeCommonCaptureGen.ErrorLevel errLevel = RiskPerformanceTools.RecordTradeRisk(pCS, pDbTransaction, captureGen, recordSettings, pRiskTemplateInfo.SessionInfo,
                    (Cst.Capture.ModeEnum)captureMode, pRiskTemplateInfo.InputUser.IdMenu,
                     ProcessState.SetErrorWarning, ProcessState.AddCriticalException,
                     LogDetailEnum,  LogTools.AddAttachedDoc);

                    if (errLevel != TradeCommonCaptureGen.ErrorLevel.SUCCESS)
                    {
                        // FI 20200723 [XXXXX] vu qu'il y a génération d'une exception l'opération m_Trade_CountProcessedError++ est effectué dans la méthode appelante 
                        //m_Trade_CountProcessedError++;
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 3, "SYS-04065",
                            new ProcessState(ProcessStateTools.StatusWarningEnum, ProcessStateTools.ParseCodeReturn(errLevel.ToString())),
                            LogTools.IdentifierAndId(pTradeInfo.SqlActor.Identifier, pTradeInfo.SqlActor.Id),
                            LogTools.IdentifierAndId(pTradeInfo.SqlBook.Identifier, pTradeInfo.SqlBook.Id),
                            LogTools.IdentifierAndId(m_CBHierarchy.Identifier_Entity, m_CBHierarchy.Ida_Entity),
                            DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness));
                    }
                    else
                    {
                        m_Trade_CountProcessedSuccess++;
                        pTradeInfo.SetResult(captureMode.Value,
                            captureGen.TradeCommonInput.DataDocument.CurrentTrade,
                            captureGen.TradeCommonInput.Identification.OTCmlId,
                            captureGen.TradeCommonInput.Identification.Identifier);

                        // FI 20200729 [XXXXX] plus de transaction 
                        m_CBRequest.UpdateCBRequestPartyTradeInfoRow(pTradeInfo.PartyTradeInfo, ProcessStateTools.StatusSuccessEnum,
                            pTradeInfo.tradeResult.idT);

                    }
                }
                else
                {
                    // PM 20190701 [24761] Cash balance non modifié car identique au précédent
                    m_Trade_CountProcessedUnchanged += 1;
                    pTradeInfo.SetResult(captureMode.Value,
                            captureGen.TradeCommonInput.DataDocument.CurrentTrade,
                            captureGen.TradeCommonInput.Identification.OTCmlId,
                            captureGen.TradeCommonInput.Identification.Identifier);
                    pTradeInfo.SetResult(Cst.ErrLevel.NOTHINGTODO);

                    // FI 20200729 [XXXXX] plus de transaction
                    m_CBRequest.UpdateCBRequestPartyTradeInfoRow(pTradeInfo.PartyTradeInfo, ProcessStateTools.StatusSuccessEnum, pTradeInfo.tradeResult.idT);

                }
                #endregion Create new trade ou update existing trade
            }
            else
            {
                m_Trade_CountNotProcessed++;
                pTradeInfo.SetResult(Cst.ErrLevel.NOTHINGTODO);
                // FI 20200729 [XXXXX] plus de transaction
                m_CBRequest.UpdateCBRequestPartyTradeInfoRow(pTradeInfo.PartyTradeInfo, ProcessStateTools.StatusNoneEnum, 0);
            }
        }
        #endregion CreateTradeCashBalance
        #region CreateTradeLinkCashBalance
        /// <summary>
        /// Alimentation des trades liés
        /// - Cash-balance précédent
        /// - Cash-Flows 
        /// - Deposit
        /// - Cash payment
        /// </summary>
        /// <param name="pCaptureGen">Classe chargée de sauvegarder un trade RISK</param>
        /// <param name="pTradeInfo">Représente un couple (CBO/MRO, Book) et tous les montants, qui seront injectés dans le Trade Cash-Balance final</param>
        // EG 20180205 [23769] New 
        private void CreateTradeLinkCashBalance(TradeRiskCaptureGen pCaptureGen, CBTradeInfo pTradeInfo)
        {
            if (pTradeInfo.PrevCashBalanceIdt > 0)
            {
                pCaptureGen.TradeCommonInput.TradeLink.Add(
                    new TradeLink.TradeLink(0, pTradeInfo.PrevCashBalanceIdt, TradeLink.TradeLinkType.PrevCashBalance, null, null,
                        new string[1] { pTradeInfo.PrevCashBalanceIdentifier },
                        new string[1] { TradeLink.TradeLinkDataIdentification.PrevCashBalanceIdentifier.ToString() }));
            }

            if (ArrFunc.IsFilled(pTradeInfo.TradesCashFlowSource)
                || ArrFunc.IsFilled(pTradeInfo.TradesDepositSource)
                || ArrFunc.IsFilled(pTradeInfo.TradesCashPaymentSource))
            {
                if (this.m_IsCashFlowsLinked) // UNUSED (toujours à true)
                {
                    foreach (Pair<int, string> trade in pTradeInfo.TradesCashFlowSource.OrderBy(match => match.Second))
                    {
                        pCaptureGen.TradeCommonInput.TradeLink.Add(
                            new TradeLink.TradeLink(0, trade.First, TradeLink.TradeLinkType.ExchangeTradedDerivativeInCashBalance, null, null,
                                new string[1] { trade.Second },
                                new string[1] { TradeLink.TradeLinkDataIdentification.ExchangeTradedDerivativeIdentifier.ToString() }));
                    }
                }
                foreach (Pair<int, string> trade in pTradeInfo.TradesCashPaymentSource.OrderBy(match => match.Second))
                {
                    pCaptureGen.TradeCommonInput.TradeLink.Add(
                        new TradeLink.TradeLink(0, trade.First, TradeLink.TradeLinkType.CashPaymentInCashBalance, null, null,
                            new string[1] { trade.Second },
                            new string[1] { TradeLink.TradeLinkDataIdentification.CashPaymentIdentifier.ToString() }));
                }
                foreach (Pair<int, string> trade in pTradeInfo.TradesDepositSource.OrderBy(match => match.Second))
                {
                    pCaptureGen.TradeCommonInput.TradeLink.Add(
                        new TradeLink.TradeLink(0, trade.First, TradeLink.TradeLinkType.MarginRequirementInCashBalance, null, null,
                            new string[1] { trade.Second },
                            new string[1] { TradeLink.TradeLinkDataIdentification.MarginRequirementIdentifier.ToString() }));
                }
            }
        }
        #endregion CreateTradeLinkCashBalance

        /// <summary>
        /// Mise en attente d'un message posté au service SpheresEventsGen
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeInfo"></param>
        /// <returns></returns>
        // EG 20180205 [23769] New 
        private async Task SendMessageQueueToEventAsync(string pCS, CBTradeInfo pTradeInfo)
        {
            string key = String.Format("(Trade: {0}({1}))", pTradeInfo.tradeResult.identifier, pTradeInfo.tradeResult.idT);
            string wait = "START CreateEventAsync Wait   : {0} " + key;
            string release = "STOP  CreateEventAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    Common.AppInstance.TraceManager.TraceTimeBegin("RiskTradeEventsGen", key);
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Wait();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(wait, SemaphoreAsync.CurrentCount));
                    }
                    SendMessageQueueToEvent(pCS, pTradeInfo.tradeResult.idT);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Release();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(release, SemaphoreAsync.CurrentCount));
                    }
                    Common.AppInstance.TraceManager.TraceTimeEnd("RiskTradeEventsGen", key);

                }
            });

        }
        /// <summary>
        /// Génération des événements des trades Cash-Balance par postage de message à SpheresEventsGen
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pLstCBTradeInfo">Liste des trades Cash-Balance générés</param>
        // EG 20180205 [23769] New  
        // EG 20181127 PERF Post RC (Step 3)
        // PM 20190701 [24761] Ajout code de retour
        // EG 20190719 Correction Envoi en double des messages de génération des événements (CB)
        //private void CreateExternalEventsAsync(string pCS, List<CBTradeInfo> pLstCBTradeInfo)
        private Cst.ErrLevel CreateExternalEventsAsync(string pCS, List<CBTradeInfo> pLstCBTradeInfo)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            int count = 0;

            // FI 20200908 [XXXXX] Appel à InitializeMaxThresholdEvents
            InitializeMaxThresholdEvents(ParallelProcess.CashBalance);

            int heapSizeEvents = GetHeapSizeEvents(ParallelProcess.CashBalance);
            List<List<CBTradeInfo>> subLstTradeInfos = ListExtensionsTools.ChunkBy(pLstCBTradeInfo.ToList(), heapSizeEvents);

            subLstTradeInfos.ForEach(lstTradeInfos =>
            {
                // PM 20190701 [24761] Ajout vérification status = SUCCESS
                IEnumerable<Task> getReturnTasksQuery =
                    from tradeInfo in lstTradeInfos
                    where (null != tradeInfo.tradeResult) && (Cst.ErrLevel.SUCCESS == tradeInfo.status)
                    select SendMessageQueueToEventAsync(pCS, tradeInfo);

                //count += getReturnTasksQuery.Count();

                //List<Task> getReturnTasks = getReturnTasksQuery.ToList();
                Task[] getReturnTasks = getReturnTasksQuery.ToArray();
                count += getReturnTasks.Count();

                try
                {
                    Task.WaitAll(getReturnTasks);
                }
                catch (Exception)
                {
                    throw;
                }

            });

            if (0 == count)
            {
                ret = Cst.ErrLevel.NOTHINGTODO;
            }

            return ret;
        }
    }
}
