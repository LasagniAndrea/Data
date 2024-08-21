#region Using Directives
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.SpheresRiskPerformance.RiskMethods;
using EFS.TradeInformation;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.MarginRequirement;
//
using FpML.Interface;
using FpML.v44.Shared;
#endregion Using Directives

namespace EFS.SpheresRiskPerformance
{
    public class MRTradeInfo : RiskTradeInfo
    {
        public RiskActorBook[] actorsBooksInPosition;
        public int idCss;
        public RiskElement root;
        public bool isGrossMargining;
        public TradeRisk prevResult;
        public List<RiskElement> riskElements;
        public List<Money> amounts;
        public List<Money> weightingRatioAmounts;
        public decimal weightingRatio;
        public InitialMarginMethodEnum[] calculationMethodType;
        public IMarginCalculationMethodCommunicationObject[] marginCalculationMethods;
        public bool fromClearer;
        public IEnumerable<UnderlyingStock> underlyingStock;
        public bool notInPosition;
        public DateTime dtBusiness;
        public bool isNotAdded;
        public bool isNotProcessed;
        // PM 20220202 Ajout isIncomplete
        public bool isIncomplete;
    }

    public abstract class RiskTradeInfo
    {
        #region Members
        public TradeResult tradeResult;
        public Cst.ErrLevel status;
        #endregion Members
        #region Methods
        public void SetResult(Cst.ErrLevel pStatus)
        {
            status = pStatus;
        }
        public void SetResult(Cst.Capture.ModeEnum pCaptureMode, ITrade pTrade, int pIdT, string pIdentifier)
        {
            status = Cst.ErrLevel.SUCCESS;
            tradeResult = new TradeResult(pCaptureMode, pTrade, pIdT, pIdentifier);
        }
        #endregion Methods
    }
    /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
    public class TradeResult
    {
        #region Members
        public Cst.Capture.ModeEnum captureMode;
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        [XmlIgnoreAttribute()]
        public ITrade trade;
        public int idT;
        public string identifier;
        #endregion Members
        #region Methods
        /// EG 20201002 [XXXXX] Correction Erreur de sérialisation XML dans "DumpObjectToFile" par ajout ou correction d'attributs de sérialization
        public TradeResult(){}
        public TradeResult(Cst.Capture.ModeEnum pCaptureMode, ITrade pTrade, int pIdT, string pIdentifier)
        {
            captureMode = pCaptureMode;
            trade = pTrade;
            idT = pIdT;
            identifier = pIdentifier;
        }
        #endregion Methods
    }

    public partial class RiskPerformanceProcess 
    {
        #region CALCUL DES DEPOSITS EN MODE ASYNCHRONE
        #region EvaluateDepositsThreading
        /// <summary>
        /// Méthode principale pour le calcul des déposits pour chaque chambre
        /// ASYNCHRONE MODE
        /// </summary>
        // EG 20180205 [23769]
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void EvaluateDepositsThreading()
        {
            m_Timers.CreateTimer("EVALUATEDEPOSITS");
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1053), 1));
            Logger.Write();

            //
            InitializeMaxThreshold(ParallelProcess.InitialMarginCalculation);

            // Evaluate the deposits for each clearing house
            Common.AppInstance.TraceManager.TraceInformation(this, "START EvaluateDepositsThreading");
            foreach (int idCss in m_MarketsCollectionFromEntity.GetCSSInternalIDs())
            {
                if (ProcessState.CodeReturn == Cst.ErrLevel.IRQ_EXECUTED)
                    break;

                Common.AppInstance.TraceManager.TraceInformation(this, string.Format("m_MethodFactory.EvaluateDeposits(idCSS : {0})", idCss.ToString()));
                m_MethodFactory.EvaluateDeposits(this, m_LogsRepository, idCss);

                if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
                {
                    // Dump deposits...
                    SerializableDictionary<Pair<int, int>, Deposit> depositsByMethod = m_MethodFactory.Deposits(idCss);

                    SerializationHelper.DumpObjectToFile<SerializableDictionary<Pair<int, int>, Deposit>>(depositsByMethod, new SysMsgCode(SysCodeEnum.LOG, 1076),
                        this.AppInstance.AppRootFolder, String.Format("EvaluatedDeposits_Css{0}.xml", idCss),
                        RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
                }
            }
            Common.AppInstance.TraceManager.TraceInformation(this, "END EvaluateDepositsThreading");
        }
        #endregion EvaluateDepositsThreading
        #endregion CALCUL DES DEPOSITS EN MODE ASYNCHRONE

        #region GENERATION DES TRADES DEPOSITS EN MODE ASYNCHRONE
        #region CreateResultsThreading
        /// <summary>
        /// Méthode principale d'appel à la création ou modification des trades MR
        /// ASYNCHRONE MODE
        /// </summary>
        // EG 20180205 [23769] New
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20201016 [XXXXX] Gestion Code retour de l'appel CreateResultAsync et mise à jour du Statut
        private void CreateResultsThreading()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            m_Timers.CreateTimer("CREATERESULTS");

            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1055), 1));
            Logger.Write();

            //
            IEnumerable<MRTradeInfo> tradeInfos =
                from methodSet in m_MethodFactory.MethodSet.Values
                from deposit in methodSet.Deposits
                select new MRTradeInfo()
                {
                    actorsBooksInPosition = deposit.Value.PairsActorBookConstitutingPosition,
                    idCss = methodSet.IdCSS,
                    root = deposit.Value.Root,
                    isGrossMargining = deposit.Value.IsGrossMargining,
                    prevResult = deposit.Value.PrevResult,
                    riskElements = deposit.Value.Factors,
                    amounts = deposit.Value.Amounts,
                    weightingRatioAmounts = deposit.Value.WeightingRatioAmounts,
                    weightingRatio = deposit.Value.WeightingRatio,
                    calculationMethodType = methodSet.MethodsType,
                    marginCalculationMethods = deposit.Value.MarginCalculationMethods,
                    fromClearer = deposit.Value.HierarchyClass == DepositHierarchyClass.CLEARER,
                    underlyingStock = (null != deposit.Value.MarginCalculationMethods) ?
                                        from itemMethod in deposit.Value.MarginCalculationMethods
                                        where itemMethod != default
                                        from itemUnderlyingStock in itemMethod.UnderlyingStock
                                        select ConvertToUnderlyingStock(itemUnderlyingStock) : null,
                    notInPosition = deposit.Value.NotInPosition,
                    // PM 20220202 Ajout isIncomplete
                    isIncomplete = deposit.Value.IsIncomplete,
                };

            CancellationTokenSource cts = new CancellationTokenSource();
            
            User user = new User(m_LogsRepository.AppSession.IdA, null, RoleActor.SYSADMIN);
            //CaptureSessionInfo
            CaptureSessionInfo sessionInfo = new CaptureSessionInfo
            {
                user = user,
                session = Session,
                licence = License,
                idProcess_L = IdProcess,
                idTracker_L = Tracker.IdTRK_L
            };

            string idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_InitialMargin);
            InputUser inputUser = new InputUser(idMenu, user);
            
            //inputUser.InitializeFromMenu(CSTools.SetCacheOn(sessionInfo.processLog.Cs));
            inputUser.InitializeFromMenu(CSTools.SetCacheOn(Cs));

            // Ininitalisation de la sémaphore
            InitializeMaxThreshold(ParallelProcess.InitialMarginWriting);

            // Découpage du traitement de génération des trade MR par tronçon de n éléments (.config)
            int heapSize = GetHeapSize(ParallelProcess.InitialMarginWriting);
            List<List<MRTradeInfo>> subLstTradeInfos = ListExtensionsTools.ChunkBy(tradeInfos.ToList(), heapSize);

            int counter = 1;
            int totSubList = subLstTradeInfos.Count();
            subLstTradeInfos.ForEach(lstTradeInfo =>
            {
                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                {
                    if ((1 < heapSize) || (1 == totSubList))
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.Info, string.Format("Parallel Create Trades MR {0}/{1}", counter, totSubList), 2));
                        Logger.Write();
                    }

                    Task<ProcessState> task = CreateResultsAsync(this.Cs, lstTradeInfo, inputUser, sessionInfo, cts);
                    task.Wait();
                    ProcessState.SetErrorWarning(task.Result.Status, task.Result.CodeReturn);
                   
                    AppInstance.AppTraceManager.TraceTimeSummary("MarginRequirementTradeGenAsync", StrFunc.AppendFormat("{0}/{1}", counter, totSubList));

                    counter++;
                }
            });

            // Dump...
            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<CalculationSheetRepository>(this.m_LogsRepository, new SysMsgCode(SysCodeEnum.LOG, 1077),
                    this.AppInstance.AppRootFolder, "RiskEvaluationLogResults.xml", null, this.ProcessState.AddCriticalException);
            }

            if (Cst.ErrLevel.IRQ_EXECUTED == codeReturn)
                ProcessState.CodeReturn = codeReturn;
        }
        #endregion CreateResultsThreading

        #region CreateResultsAsync
        /// <summary>
        /// Méthode de constitution de la liste des tâches de mise à jour des trades MR
        /// pour chaque pair (CSS/ATTRIBUTE(ACTOR))
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pTradeInfos">Données du trade MR</param>
        /// <param name="pChunckSize">Taille du tronçon</param>
        /// <param name="pInputUser">Environnement utilisateur</param>
        /// <param name="pSessionInfo">Environnement de sauvegarde</param>
        /// <param name="pCts">Source de la notification d'annulation des tâches (Unused for the moment)</param>
        // EG 20180205 [23769] New  
        // EG 20180525 [23979] IRQ Processing
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20201016 [XXXXX] Changement retour de l'appel et le traitement continue tout de même si une tâche est en erreur
        private async Task<ProcessState> CreateResultsAsync(string pCS, List<MRTradeInfo> pTradeInfos, InputUser pInputUser,
            CaptureSessionInfo pSessionInfo, CancellationTokenSource pCts)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            ProcessState processState = new ProcessState(ProcessStateTools.StatusEnum.SUCCESS, ret);
            try
            {
                List<Task<MRTradeInfo>> getReturnTasks = null;

                #region Génération des trades Déposits
                

                IEnumerable<Task<MRTradeInfo>> getReturnTasksQuery =
                    from tradeInfo in pTradeInfos select CreateResultAsync(pCS, tradeInfo, pInputUser, pSessionInfo, pCts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();

                // Boucle sur l'ensemble des tâches du Pool
                // On s'arrête à la 1ère tâche terminée pour :
                // 1. La supprimer de la liste
                // 2. Récupérer sa valeur de retour
                while (0 < getReturnTasks.Count)
                {
                    // On s'arrête sur la première tâche complétée.  
                    Task<MRTradeInfo> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    // On supprime la tâche de la liste des tâches pour éviter que le process ne la traite une nouvelle fois.  
                    getReturnTasks.Remove(firstFinishedTask);

                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //{
                    //    //throw firstFinishedTask.Exception.Flatten();
                    //    processState.SetErrorWarning(ProcessStateTools.StatusEnum.ERROR, Cst.ErrLevel.FAILURE);
                    //}
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CreateResultsAsync", processState);

                    if (IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref processState))
                        pCts.Cancel(true);
                }
                #endregion Génération des trades Déposits

                if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                {
                    // Setting de la piste d'audit de calcul
                    pTradeInfos.ForEach(tradeInfo => SetCalculationSheet(tradeInfo));

                    // FI 20200908 [XXXXX] Call CreateInternalEventsAsync
                    CreateInternalEventsAsync(ParallelProcess.InitialMarginWriting, pTradeInfos.Cast<RiskTradeInfo>());
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
            return processState;
        }
        #endregion CreateResultsAsync
        #region CreateResultAsync
        /// <summary>
        /// Mise en attente d'une tâche de mise à jour d'un trade MR
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pTradeInfo">Données du trade MR</param>
        /// <param name="pInputUser">Environnement de l'utilisateur</param>
        /// <param name="pSessionInfo">Environnement de sauvegarde</param>
        /// <param name="pCt">Notification d'annulation</param>
        // EG 20180205 [23769] New  
        // EG 20210916 [XXXXX] Implémentation TryMultiple
        private async Task<MRTradeInfo> CreateResultAsync(string pCS, MRTradeInfo pTradeInfo, InputUser pInputUser, CaptureSessionInfo pSessionInfo, CancellationToken pCt)
        {
            string key = String.Format("(CSS: {0} ActorId: {1} BookId: {2})", pTradeInfo.idCss, pTradeInfo.root.ActorId, pTradeInfo.root.AffectedBookId);
            string wait = "START CreateResultAsync Wait   : {0} " + key;
            string release = "STOP  CreateResultAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != SemaphoreAsync);
            await Task.Run(() =>
            {
                try
                {
                    Common.AppInstance.TraceManager.TraceTimeBegin("MarginRequirementTradeGenAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Wait();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(wait, SemaphoreAsync.CurrentCount));
                    }
                    //CreateResult(pCS, pTradeInfo, pInputUser, pSessionInfo, key);

                    TryMultiple tryMultiple = new TryMultiple(pCS, "CreateResult", "CreateResult")
                    {
                        SetErrorWarning = this.ProcessState.SetErrorWarning,
                        IsModeTransactional = false,
                        ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                    };
                    tryMultiple.Exec<string, MRTradeInfo, InputUser, CaptureSessionInfo, string, MRTradeInfo>(
                        delegate (String arg1, MRTradeInfo arg2, InputUser arg3, CaptureSessionInfo arg4, String arg5)
                        { return CreateResult(arg1, arg2, arg3, arg4, arg5); }, pCS, pTradeInfo, pInputUser, pSessionInfo, key);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Release();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(release, SemaphoreAsync.CurrentCount));
                    }
                    Common.AppInstance.TraceManager.TraceTimeEnd("MarginRequirementTradeGenAsync", key);
                }
            }, pCt);
            return pTradeInfo;
        }
        #endregion CreateResultAsync
        #region CreateResult
        /// <summary>
        /// Création ou mise à jour d'un trade MR
        /// ASYNCHRONE MODE
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pTradeInfo">Données du trade MR</param>
        /// <param name="pInputUser">Environnement de l'utilisateur</param>
        /// <param name="pSessionInfo">Environnement de sauvegarde</param>
        // EG 20180205 [23769] New  
        private MRTradeInfo CreateResult(string pCS, MRTradeInfo pTradeInfo, InputUser pInputUser, CaptureSessionInfo pSessionInfo, string pKey)
        {
            pTradeInfo.dtBusiness = m_MarketsCollectionFromEntity.GetBusinessDate(pTradeInfo.idCss);

            IDbTransaction dbTransaction = null;

            try
            {
                // FI 20231123 [WI748] transaction à null => A l'avenir, il ne faut plus ouvrir de transaction puisque qu'un tryMultiple est en vigueur dans TradeCommonCaptureGen.ErrorLevel RecordTradeRisk
                //dbTransaction = DataHelper.BeginTran(pCS);
                CreateTradeMarginRequirement(pCS, dbTransaction, pInputUser, pSessionInfo, pTradeInfo);

                if (false == pTradeInfo.notInPosition)
                {
                    this.m_ImRequestDiagnostics.SetTrade
                        (pTradeInfo.idCss, pTradeInfo.root.ActorId, pTradeInfo.root.AffectedBookId, pTradeInfo.tradeResult.idT);
                }
                pTradeInfo.status = Cst.ErrLevel.SUCCESS;
                if (null != dbTransaction)
                    DataHelper.CommitTran(dbTransaction);
            }
            catch (Exception)
            {
                if ((null != dbTransaction) && DataHelper.IsTransactionValid(dbTransaction))
                    DataHelper.RollbackTran(dbTransaction);

                pTradeInfo.status = Cst.ErrLevel.FAILURE;
                Common.AppInstance.TraceManager.TraceError(this, $"ERROR CreateResultAsync : {pKey}");
                throw;
            }
            return pTradeInfo;
        }
        #endregion CreateResult

        #region CreateTradeMarginRequirement
        /// <summary>
        /// Génération d'un trade CB : Création|Mise à jour
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pDbTransaction">Transaction</param>
        /// <param name="pInputUser">Environnement de l'utilisateur</param>
        /// <param name="pSessionInfo">Environnement de sauvegarde</param>
        /// <param name="pTradeInfo">Données du trade MR</param>
        // EG 20180205 [23769] New 
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private void CreateTradeMarginRequirement(string pCS, IDbTransaction pDbTransaction,
            InputUser pInputUser, CaptureSessionInfo pSessionInfo, MRTradeInfo pTradeInfo)
        {
            SQL_Instrument sqlInstrument =
                new SQL_Instrument(CSTools.SetCacheOn(pCS), Cst.ProductMarginRequirement, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty)
                {
                    DbTransaction = pDbTransaction
                };

            bool findInstr = sqlInstrument.LoadTable(new string[] { "IDI,IDENTIFIER" });
            if (false == findInstr)
                throw new Exception(StrFunc.AppendFormat("Instrument {0} not found", Cst.ProductMarginRequirement));

            Nullable<Cst.Capture.ModeEnum> captureMode = Cst.Capture.ModeEnum.New;
            if (null != pTradeInfo.prevResult)
            {
                if (pTradeInfo.prevResult.ReEvaluation == RiskRevaluationMode.EvaluateWithUpdate)
                    captureMode = Cst.Capture.ModeEnum.Update;
                else if (pTradeInfo.prevResult.ReEvaluation == RiskRevaluationMode.NewEvaluation)
                    captureMode = Cst.Capture.ModeEnum.New;
                else if (pTradeInfo.prevResult.ReEvaluation == RiskRevaluationMode.DoNotEvaluate)
                    captureMode = null;
            }

            if ((captureMode == Cst.Capture.ModeEnum.New || captureMode == Cst.Capture.ModeEnum.Update))
            {
                string screenName = string.Empty;

                int idTSource;
                if (captureMode == Cst.Capture.ModeEnum.Update)
                {
                    idTSource = pTradeInfo.prevResult.Id;
                }
                else
                {
                    SearchInstrumentGUI searchInstrumentGUI = new SearchInstrumentGUI(sqlInstrument.Id);
                    StringData[] data = searchInstrumentGUI.GetDefault(CSTools.SetCacheOn(pCS), pDbTransaction, false);

                    if (ArrFunc.IsEmpty(data))
                        throw new Exception(StrFunc.AppendFormat("Screen or template not found for Instrument {0}", sqlInstrument.Identifier));

                    screenName = ((StringData)ArrFunc.GetFirstItem(data, "SCREENNAME")).value;
                    string templateIdentifier = ((StringData)ArrFunc.GetFirstItem(data, "TEMPLATENAME")).value;

                    idTSource = TradeRDBMSTools.GetTradeIdT(CSTools.SetCacheOn(pCS), pDbTransaction, templateIdentifier);
                }

                if (idTSource == 0)
                    throw new Exception("Trade Source not found");

                #region Create new trade ou update existing trade
                TradeRiskCaptureGen captureGen = new TradeRiskCaptureGen();

                string csLoad = pCS;
                if (captureMode == Cst.Capture.ModeEnum.New)
                    csLoad = CSTools.SetCacheOn(csLoad);
                bool isFound = captureGen.Load(csLoad, pDbTransaction, idTSource, (Cst.Capture.ModeEnum)captureMode, pInputUser.User, pSessionInfo.session.SessionId, false);

                if (false == isFound)
                    throw new InvalidOperationException(StrFunc.AppendFormat("<b>trade [idT:{0}] not found</b>", idTSource));

                captureGen.InitBeforeCaptureMode(pCS, pDbTransaction, pInputUser, pSessionInfo);

                SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), pTradeInfo.root.ActorId)
                {
                    DbTransaction = pDbTransaction
                };
                sqlActor.LoadTable();

                SQL_Book sqlBook = new SQL_Book(CSTools.SetCacheOn(pCS), pTradeInfo.root.AffectedBookId)
                {
                    DbTransaction = pDbTransaction
                };
                sqlBook.LoadTable();

                SQL_Actor sqlActorCSS = new SQL_Actor(CSTools.SetCacheOn(pCS), pTradeInfo.idCss)
                {
                    DbTransaction = pDbTransaction
                };
                sqlActorCSS.LoadTable();

                SQL_Actor sqlActorEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), this.m_ProcessInfo.Entity)
                {
                    DbTransaction = pDbTransaction
                };
                sqlActorEntity.LoadTable();

                // RD 20170421 [23094] 
                if (sqlActorEntity.Id != sqlBook.IdA_Entity)
                {
                    SQL_Actor sqlBookEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), sqlBook.IdA_Entity)
                    {
                        DbTransaction = pDbTransaction
                    };
                    sqlBookEntity.LoadTable();

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        StrFunc.AppendFormat(@"<b>Calculation of Initial Margin not made.</b>
                <b>Cause:</b> The processing entity is different from the Book accounting entity.
                <b>Action:</b> Please specify the correct accounting entity on the Book and re-launch the process.
                <b>Details:</b>
                - Office: <b>{0}</b>
                - Book: <b>{1}</b>
                - Book accounting entity: <b>{2}</b>
                - Processing entity: <b>{3}</b>
                - Clearing house / Custodian: <b>{4}</b>
                - Clearing date: <b>{5}</b>",
                     LogTools.IdentifierAndId(sqlActor.Identifier, sqlActor.Id),
                     LogTools.IdentifierAndId(sqlBook.Identifier, sqlBook.Id),
                     LogTools.IdentifierAndId(sqlBookEntity.Identifier, sqlBookEntity.Id),
                     LogTools.IdentifierAndId(sqlActorEntity.Identifier, sqlActorEntity.Id),
                     LogTools.IdentifierAndId(sqlActorCSS.Identifier, sqlActorCSS.Id),
                     DtFunc.DateTimeToStringDateISO(pTradeInfo.dtBusiness)));
                }

                SetDataDocument(captureGen.TradeCommonInput.DataDocument, sqlActor, sqlBook, sqlActorEntity, sqlActorCSS, pTradeInfo);

                if (captureMode == Cst.Capture.ModeEnum.Update)
                    screenName = captureGen.TradeCommonInput.SQLLastTradeLog.ScreenName;

                if (captureMode == Cst.Capture.ModeEnum.New)
                {
                    //En création: Spheres® ecrase systématiquement le StatusEnvironment issu du template par REGULAR
                    captureGen.TradeCommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                }

                //Alimentation des partyNotifications (pas de messagerie)
                captureGen.TradeCommonInput.TradeNotification.SetSetConfirmation(false);
                //Alimentation des partyNotifications (pas de messagerie)
                captureGen.TradeCommonInput.TradeStatus.stEnvironment.NewSt =
                    CalculationSheetRepository.RiskEvaluationModeToStatusEnvironment(this.m_ProcessInfo.Mode).ToString();

                // PM 20220202 Gestion deposit incomplet
                if (pTradeInfo.isIncomplete)
                {
                    captureGen.TradeCommonInput.TradeStatus.stActivation.NewSt = Cst.StatusActivation.MISSING.ToString();
                }
                else
                {
                    captureGen.TradeCommonInput.TradeStatus.stActivation.NewSt = Cst.StatusActivation.REGULAR.ToString();
                }

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    displayName = string.Empty,  //Sera identique à identifier
                    description = $"Deposit [{DtFunc.DateTimeToStringDateISO(pTradeInfo.dtBusiness)}/{sqlActor.Identifier}/{sqlActorCSS.Identifier}]",
                    extLink = string.Empty,
                    idScreen = screenName,

                    isGetNewIdForIdentifier = true,
                    isCheckValidationRules = false,
                    isCheckValidationXSD = false,
                    isCheckLicense = false,
                    isSaveUnderlyingInParticularTransaction = false
                };

                TradeCommonCaptureGen.ErrorLevel lRet = RiskPerformanceTools.RecordTradeRisk(pCS, pDbTransaction,
                    captureGen, recordSettings, pSessionInfo, (Cst.Capture.ModeEnum)captureMode, pInputUser.IdMenu,
                m_LogsRepository.SetErrorWarning, m_LogsRepository.AddException, LogDetailEnum, m_LogsRepository.Attach);

                if (lRet != TradeCommonCaptureGen.ErrorLevel.SUCCESS)
                {
                    string msg;
                    if (captureMode == Cst.Capture.ModeEnum.New)
                        msg =
                            StrFunc.AppendFormat("Error[{0}] when creating Margin Requirement trade for Margin Requirement Office {1} and clearing organization {2}",
                            lRet.ToString(), sqlActor.Identifier, sqlActorCSS.Identifier);
                    else
                        msg = StrFunc.AppendFormat("Error[{0}] when modifying Margin Requirement trade {1} of Margin Requirement Office {2} and clearing organization {3}",
                            lRet.ToString(), pTradeInfo.prevResult.Identifier, sqlActor.Identifier, sqlActorCSS.Identifier);

                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, msg);
                }

                pTradeInfo.tradeResult = new TradeResult(captureMode.Value,
                    captureGen.TradeCommonInput.DataDocument.CurrentTrade,
                    captureGen.TradeCommonInput.Identification.OTCmlId,
                    captureGen.TradeCommonInput.Identification.Identifier);

                #endregion
            }
        }
        #endregion CreateTradeMarginRequirement
        #region SetDataDocument
        /// <summary>
        /// Mise à jour du Data document 
        /// </summary>
        // EG 20180205 [23769] New 
        private void SetDataDocument(DataDocumentContainer pDataDocument, SQL_Actor pSqlActor, SQL_Book pSqlBook, SQL_Actor pSqlEntity, SQL_Actor pSqlCss, MRTradeInfo pTradeInfo)
        {
            m_LogsRepository.SetDataDocument(pDataDocument, pSqlActor, pSqlBook, pSqlEntity, pSqlCss, pTradeInfo.dtBusiness,
                (pTradeInfo.notInPosition ? pTradeInfo.amounts : pTradeInfo.weightingRatioAmounts),
                this.m_ProcessInfo.Timing, pTradeInfo.fromClearer, pTradeInfo.calculationMethodType,
                pTradeInfo.isGrossMargining, pTradeInfo.underlyingStock);
        }
        #endregion SetDataDocument

        #region AddCalculationSheet
        /// <summary>
        /// Ajoute une feuille de calcul vide au Log 
        /// </summary>
        // EG 20180205 [23769] New 
        // EG 2018021 [23769] Upd Test pTradeInfo.tradeResult not null
        private bool AddCalculationSheet(MRTradeInfo pTradeInfo)
        {
            bool isAdded = false;
            if (null != pTradeInfo.tradeResult)
            {
                isAdded = m_LogsRepository.AddCalculationSheet(this.Cs,
                         pTradeInfo.root.ActorId, pTradeInfo.root.AffectedBookId,
                         pTradeInfo.idCss, pTradeInfo.isGrossMargining,
                         pTradeInfo.tradeResult.idT, pTradeInfo.tradeResult.identifier, pTradeInfo.tradeResult.trade,
                         pTradeInfo.weightingRatioAmounts, pTradeInfo.weightingRatio);
            }
            pTradeInfo.isNotAdded = (false == isAdded);
            return pTradeInfo.isNotAdded;
        }
        #endregion AddCalculationSheet
        #region ProcessCalculationSheet
        /// <summary>
        /// Traitement de l'élément ajouté précédemment dans la feuille de calcul
        /// en ajoutant tous les détails utilisés par le processus d'évaluation des risques pour calculer le résultat du déposit.
        /// </summary>
        // EG 20180205 [23769] New 
        private bool ProcessCalculationSheet(MRTradeInfo pTradeInfo)
        {
            bool isProcessed = m_LogsRepository.ProcessCalculationSheet(this.Cs, pTradeInfo.root.ActorId, pTradeInfo.root.AffectedBookId, pTradeInfo.idCss,
                pTradeInfo.dtBusiness, this.m_ProcessInfo.Timing, pTradeInfo.actorsBooksInPosition, pTradeInfo.riskElements,
                pTradeInfo.amounts, pTradeInfo.marginCalculationMethods);

            pTradeInfo.isNotProcessed = (false == isProcessed);
            return pTradeInfo.isNotProcessed;
        }
        #endregion ProcessCalculationSheet

        #region SetCalculationSheet
        /// <summary>
        /// Mise à jour du journal des risque avec le nouveau trade MR
        /// </summary>
        /// <param name="pTradeInfo">Données du trade MR</param>
        // EG 20180205 [23769] New  
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SetCalculationSheet(MRTradeInfo pTradeInfo)
        {
            try
            {
                bool modifyExistant = (null != pTradeInfo.prevResult) && (RiskRevaluationMode.DoNotEvaluate != pTradeInfo.prevResult.ReEvaluation);
                AddCalculationSheet(pTradeInfo);
                ProcessCalculationSheet(pTradeInfo);
                if (pTradeInfo.isNotAdded)
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1006), 2,
                        new LogParam(m_ActorsRoleMarginReqOffice.Where(actor => actor.Id == pTradeInfo.root.ActorId).First().Identifier),
                        new LogParam(Convert.ToString(modifyExistant))));
                }
                if (pTradeInfo.isNotProcessed)
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 1028), 2,
                        new LogParam(m_ActorsRoleMarginReqOffice.Where(actor => actor.Id == pTradeInfo.root.ActorId).First().Identifier),
                        new LogParam(Convert.ToString(modifyExistant))));
                }
            }
            catch (Exception ex)
            {
                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);
                ProcessState.AddCriticalException(spheresEx);
                
                Logger.Log(new LoggerData(spheresEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 2));

                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }
        }
        #endregion SetCalculationSheet



        #endregion GENERATION DES TRADES DEPOSITS EN MODE ASYNCHRONE
    }
}