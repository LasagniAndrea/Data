using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Process.EventsGen;
using EFS.SpheresRiskPerformance.Enum;
//
using EfsML.Business;
using EfsML.EventMatrix;
//
using FixML.Enum;

namespace EFS.SpheresRiskPerformance
{

    /// <summary>
    /// The base process sharing code for process of  riskperformane service
    /// </summary>
    /// FI 20120801 add RiskCommonProcessBase
    public abstract partial class RiskCommonProcessBase : ProcessBase
    {
        /// <summary>
        /// 
        /// </summary>
        protected enum EventGenModeEnum
        {
            /// <summary>
            /// Génération des évènement par le service RisPerformance
            /// </summary>
            Internal,
            /// <summary>
            /// Génération des évènement par le service SpheresEventGen
            /// </summary>
            External,
        }

        /// <summary>
        /// Timers to evaluate time consuming of the operations
        /// </summary>
        protected TimerCollection m_Timers = new TimerCollection();

        /// <summary>
        /// Events generation mode
        /// </summary>
        protected EventGenModeEnum m_EventGenMode = EventGenModeEnum.Internal; //External;


        // EG 20140217 
        /// <summary>
        /// UNUSED
        /// Flag indiquant si écritures des trade ETD (CashFlows et OtherFlows) liés au CB dans TRADELINK
        /// </summary>
        protected bool m_IsCashFlowsLinked = true;

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.ACTOR;
            }
        }

        /// <summary>
        /// Obtient ACTOR (le currentId est un acteur, c'est une entité) 
        /// </summary>
        protected override string DataIdent
        {
            get
            {
                return Cst.OTCml_TBL.ACTOR.ToString();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pService"></param>
        public RiskCommonProcessBase(MQueueBase pQueue, AppInstanceService pService) :
            base(pQueue, pService)
        { }

        /// <summary>
        /// Verification ultime avant execution du traitement 
        /// <para>- Mise en place d'un lock</para>
        /// </summary>
        /// FI 20141125 [20230] Modify
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();

            if (false == IsProcessObserver)
            {
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    //PL 20140917 [20526]  Si IsCashBalanceProcess==true alors pose d'un lock EXCLUSIF, afin d'interdire l'exécution simultané de 2 traitements CB.
                    //Attention Il ne faut absolument pas changer ce comportement. 
                    //
                    string lockType = LockTools.Shared;
                    if ((MQueue is RiskPerformanceMQueue queue) && (queue.IsCashBalanceProcess))
                        lockType = LockTools.Exclusive;

                    ProcessState.CodeReturn = LockCurrentObjectId(lockType);
                }
            }
        }
        /// <summary>
        /// Alimente, Enrichie le message queue lorsque ce dernier est pauvre
        /// </summary>
        protected override void ProcessInitializeMqueue()
        {
            base.ProcessInitializeMqueue();

            if (false == IsProcessObserver)
            {
                if (false == MQueue.idInfoSpecified)
                {
                    IdInfo mqueueInfo = SQL_TableTools.GetRiskMQueueIdInfo(CSTools.SetCacheOn(Cs), MQueue.id);
                    MQueue.idInfoSpecified = (null != mqueueInfo);
                    if (MQueue.idInfoSpecified)
                        MQueue.idInfo = mqueueInfo;
                }
            }
        }
    }

    /// <summary>
    /// The base process sharing code for process of  riskperformane service
    /// </summary>
    /// FI 20120801 RiskPerformanceProcessBase hérite de RiskCommonProcessBase
    public abstract class RiskPerformanceProcessBase : RiskCommonProcessBase
    {

        /// <summary>
        /// Process information driving the current risk margin evaluation
        /// </summary>
        protected RiskPerformanceProcessInfo m_ProcessInfo;

        /// <summary>
        /// Accessor pour les informations sur le process pilotant l'evaluation courante du déposit
        /// </summary>
        public RiskPerformanceProcessInfo ProcessInfo
        {
            get { return m_ProcessInfo; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pService"></param>
        public RiskPerformanceProcessBase(RiskPerformanceMQueue pQueue, AppInstanceService pService)
            : base(pQueue, pService)
        {
        }

        /// <summary>
        /// Initialise the internal process info struct extracting the values from the mqueue request
        /// </summary>
        /// <returns>SUCCESS when all the mandatory parmaeters are found</returns>
        /// FI 20141104 [20466] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected Cst.ErrLevel InitializeProcessInfo()
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;

#if !DEBUG && !DEBUGDEV && !RELEASEDEV

            this.LogDetailEnum = LogLevelDetail.LEVEL2;

#endif

            RiskPerformanceMQueue riskQueue = (RiskPerformanceMQueue)base.MQueue;
            if (riskQueue.idSpecified)
            {
                this.m_ProcessInfo.Entity = riskQueue.id;
                // FI 20141104 [20466]  
                this.m_ProcessInfo.EntityIdentifier = riskQueue.identifier;
            }

            if (AllParametersAvailable(riskQueue))
            {
                try
                {
                    // Initialisation des paramètre Asynchrone pour le déposit
                    string timing = riskQueue.GetStringValueParameterById(RiskPerformanceMQueue.PARAM_TIMING);
                    this.m_ProcessInfo.Timing =
                        (SettlSessIDEnum)System.Enum.Parse(typeof(SettlSessIDEnum),
                        ReflectionTools.GetMemberNameByXmlEnumAttribute(typeof(SettlSessIDEnum), timing, true));

                    this.m_ProcessInfo.DtBusiness = riskQueue.GetDateTimeValueParameterById(RiskPerformanceMQueue.PARAM_DTBUSINESS);
                    //this.m_ProcessInfo.Date1 = this.m_ProcessInfo.DtBusiness;

                    if (this.m_ProcessInfo.Timing == SettlSessIDEnum.Intraday)
                    {
                        //this.m_ProcessInfo.Date1 += riskQueue.GetTimeValueFromParameter(RiskPerformanceMQueue.PARAM_TIME1);
                        this.m_ProcessInfo.TimeBusiness = riskQueue.GetTimeValueFromParameter(RiskPerformanceMQueue.PARAM_TIMEBUSINESS);

                        //PM 20140214 [19493] Lecture nouveaux paramètres
                        this.m_ProcessInfo.PositionTime = riskQueue.GetTimeValueFromParameter(RiskPerformanceMQueue.PARAM_POSITIONTIME);
                        this.m_ProcessInfo.RiskDataTime = riskQueue.GetTimeValueFromParameter(RiskPerformanceMQueue.PARAM_RISKDATATIME);
                        if (this.m_ProcessInfo.PositionTime.Ticks == 0)
                        {
                            // Pour rétrocompatibilité: si heure = 0 prendre toutes les positions de la journée
                            this.m_ProcessInfo.PositionTime = new TimeSpan(23, 59, 59);
                        }
                        if (this.m_ProcessInfo.RiskDataTime.Ticks == 0)
                        {
                            // Pour rétrocompatibilité: si heure = 0 prendre les derniers paramètres de calcul de la journée
                            this.m_ProcessInfo.RiskDataTime = new TimeSpan(23, 59, 59);
                        }
                    }

                    if (false == riskQueue.IsCashBalanceProcess)
                    {
                        bool reset = riskQueue.GetBoolValueParameterById(RiskPerformanceMQueue.PARAM_ISRESET);
                        this.m_ProcessInfo.Reset = reset;

                        this.m_ProcessInfo.CssId = riskQueue.GetIntValueParameterById(RiskPerformanceMQueue.PARAM_CSSCUSTODIAN);
                        this.m_ProcessInfo.CssIdentifier = riskQueue.GetExtendValueParameterById(RiskPerformanceMQueue.PARAM_CSSCUSTODIAN);

                        bool isSimul = riskQueue.GetBoolValueParameterById(RiskPerformanceMQueue.PARAM_ISSIMUL);

                        if (isSimul)
                        {
                            this.m_ProcessInfo.Mode = RiskEvaluationMode.Simulation;
                        }
                        else
                        {
                            this.m_ProcessInfo.Mode = RiskEvaluationMode.Normal;
                        }

                        //PM 20141216 [9700] Eurex Prisma for Eurosys Futures
                        this.m_ProcessInfo.SoftwareRequester = riskQueue.GetStringValueParameterById(RiskPerformanceMQueue.PARAM_SOFTWARE);
                        if (StrFunc.IsEmpty(this.m_ProcessInfo.SoftwareRequester))
                        {
                            this.m_ProcessInfo.SoftwareRequester = Software.SOFTWARE_Spheres;
                        }
                    }
                }
                catch (SystemException ex)
                {
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    ProcessState.AddCriticalException(ex);

                    Logger.Log(new LoggerData(new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex)));
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1014), 0));

                    errLevel = Cst.ErrLevel.MISSINGPARAMETER;
                }
            } // end if parametersSpecified
            else
            {
                
                Logger.Log(new LoggerData(LogLevelEnum.None, Ressource.GetString("RiskPerformance_WARNINGParametersNotExist")));

                errLevel = Cst.ErrLevel.MISSINGPARAMETER;
            }

            this.m_ProcessInfo.MarginReqOfficeChild = null;
            if (riskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_MRO))
            {
                int idAMarginReqOff = riskQueue.GetIntValueParameterById(RiskPerformanceMQueue.PARAM_MRO);
                if (idAMarginReqOff > 0)
                    this.m_ProcessInfo.MarginReqOfficeChild = idAMarginReqOff;
            }

            if (riskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_IDPR))
            {
                int idTemp = riskQueue.GetIntValueParameterById(RiskPerformanceMQueue.PARAM_IDPR);
                if (idTemp > 0)
                {
                    //m_EventGenMode = EventGenModeEnum.Internal;
                    this.m_ProcessInfo.IdPr = idTemp;
                }
            }
            // PM 20180219 [23824] Ajout Process
            m_ProcessInfo.Process = this;
            return errLevel;
        }

        /// <summary>
        /// Returns true if the required parameters are set
        /// </summary>
        /// <param name="pRiskQueue">input parameters</param>
        /// <returns>true when the clearing house, reset and timing parameters are specified</returns>
        protected bool AllParametersAvailable(RiskPerformanceMQueue pRiskQueue)
        {
            bool ret = false;
            if (pRiskQueue.parametersSpecified)
            {
                if (pRiskQueue.IsCashBalanceProcess)
                {
                    ret = pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS) &&
                          pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_TIMING);
                }
                else
                {
                    ret = pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_CLEARINGHOUSE) &&
                          pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS) &&
                          pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_ISRESET) &&
                          pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_TIMING) &&
                          pRiskQueue.IsExistParameter(RiskPerformanceMQueue.PARAM_ISSIMUL);
                }
            }
            return ret;
        }


        #region CreateRiskEventsAsync
        // EG 20180503 One EventMatrix construction for all trades MR|CB
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected async Task<Cst.ErrLevel> CreateRiskEventsAsync(IEnumerable<Pair<int, string>> pTrades)
        {
            Common.AppInstance.TraceManager.TraceInformation(this, "START CreateRiskEventsAsync");

            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4061), 2));
            Logger.Write();

            CancellationTokenSource cts = new CancellationTokenSource();
            List<Task<Cst.ErrLevel>> getReturnTasks = null;
            try
            {

                Pair<int, string> tradeForMatrix = pTrades.FirstOrDefault(item => item.First > 0);
                EFS_EventMatrix eventMatrix = null;
                if (null != tradeForMatrix)
                {
                    EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(Cs, null, tradeForMatrix.First);
                    string path = AppInstance.MapPath(@"EventsGen");
                    tradeLibrary.EventsMatrixConstruction(Cs, path, "RiskTrade");
                    eventMatrix = tradeLibrary.EventMatrix;
                }

                IEnumerable<Task<Cst.ErrLevel>> getReturnTasksQuery =
                    from trade in pTrades select CreateRiskEventsByTradeAsync(trade, eventMatrix, cts.Token);

                getReturnTasks = getReturnTasksQuery.ToList();
                while (0 < getReturnTasks.Count)
                {
                    Task<Cst.ErrLevel> firstFinishedTask = await Task.WhenAny(getReturnTasks);
                    getReturnTasks.Remove(firstFinishedTask);
                    // EG 20230927 [26506] Ajout du message d'erreur dans la trace
                    //if (firstFinishedTask.IsFaulted)
                    //    throw firstFinishedTask.Exception.Flatten();
                    ProcessTools.AddTraceExceptionAndProcessStateFailure(this, firstFinishedTask, "CreateRiskEventsAsync", null, true);
                }
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4060), 2));
                Logger.Write();
            }
            catch (Exception)
            {
                cts.Cancel();
                throw;
            }
            finally
            {
                Common.AppInstance.TraceManager.TraceInformation(this, "STOP CreateRiskEventsAsync");
                Common.AppInstance.TraceManager.TraceTimeSummary("RiskEventsGenAsync");
            }
            return ret;
        }
        #endregion CreateRiskEventsAsync

        #region CreateRiskEventsByTradeAsync
        // EG 20180503 EventMatrix parameter
        // EG 20180525 [23979] IRQ Processing
        private async Task<Cst.ErrLevel> CreateRiskEventsByTradeAsync(Pair<int, string> pTrade, EFS_EventMatrix pEventMatrix, CancellationToken pCt)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string key = String.Format("(Trade: {0}({1}))", pTrade.Second, pTrade.First);
            string wait = "START CreateRiskEventsByTradeAsync Wait   : {0} " + key;
            string release = "STOP  CreateRiskEventsByTradeAsync Release: {0} " + key;

            bool isSemaphoreSpecified = (null != SemaphoreAsync);

            await Task.Run(() =>
            {
                try
                {
                    Common.AppInstance.TraceManager.TraceTimeBegin("RiskEventsGenAsync", key);
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Wait();

                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(wait, SemaphoreAsync.CurrentCount));
                    }
                    ret = CreateRiskEvents(pTrade, pEventMatrix);
                }
                catch (Exception) { throw; }
                finally
                {
                    if (isSemaphoreSpecified)
                    {
                        SemaphoreAsync.Release();
                        Common.AppInstance.TraceManager.TraceVerbose(this, string.Format(release, SemaphoreAsync.CurrentCount));
                    }
                    Common.AppInstance.TraceManager.TraceTimeEnd("RiskEventsGenAsync", key);
                }
            }, pCt);
            return ret;
        }
        #endregion CreateRiskEventsByTradeAsync

        #region CreateRiskEvents
        // EG 20180503 pEventMatrix parameter
        private Cst.ErrLevel CreateRiskEvents(Pair<int, string> pTrade, EFS_EventMatrix pEventMatrix)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string gProduct = ReflectionTools.ConvertEnumToString<ProductTools.GroupProductEnum>(ProductTools.GroupProductEnum.Risk);

            TradeInfo tradeInfo = TradeInfo.LoadTradeInfo(Cs, null, pTrade.First, gProduct);

            #region Calculation (EFS_xxx)
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                EventsGenProductCalculationRISK eventsGenProductCalculation = new EventsGenProductCalculationRISK(Cs, tradeInfo);
                ret = eventsGenProductCalculation.Calculation(this, tradeInfo.tradeLibrary.Product);
            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                if (tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPaymentSpecified)
                    ret = PaymentTools.CalcPayments(Cs, tradeInfo.tradeLibrary.Product,
                        tradeInfo.tradeLibrary.CurrentTrade.OtherPartyPayment, tradeInfo.tradeLibrary.DataDocument);
            }
            #endregion

            #region Event GENERATION
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                tradeInfo.tradeLibrary.EventMatrix = pEventMatrix;

                EventsGenTrade eventsGenTrade = new EventsGenTrade(this)
                {
                    TradeInfo = tradeInfo,
                    IsDelEvents = false
                };

                ret = eventsGenTrade.Generation();
            }
            #endregion

            return ret;
        }
        #endregion CreateRiskEvents


        /// <summary>
        /// Génération des événements des trades Risk par appel au service SpheresEventsGen (SLAVE Call)
        ///<para>ASYNCHRONE MODE</para>
        /// </summary>
        /// <param name="parallelProcess"></param>
        /// <param name="pLstTradeInfo">Liste des trades Risk générés</param>
        /// FI 20200908 [XXXXX] Add Method 
        protected void CreateInternalEventsAsync(ParallelProcess parallelProcess, IEnumerable<RiskTradeInfo> pLstTradeInfo)
        {
            InitializeMaxThresholdEvents(parallelProcess);

            int heapSizeEvents = GetHeapSizeEvents(parallelProcess);
            List<List<RiskTradeInfo>> subLstTradeInfos = ListExtensionsTools.ChunkBy(pLstTradeInfo.ToList(), heapSizeEvents);

            subLstTradeInfos.ForEach(lstTradeInfos =>
            {
                IEnumerable<Pair<int, string>> trades =
                from tradeInfo in lstTradeInfos
                where (null != tradeInfo.tradeResult) && (Cst.ErrLevel.SUCCESS == tradeInfo.status)
                select new Pair<int, string>(tradeInfo.tradeResult.idT, tradeInfo.tradeResult.identifier);

                // PM 20190701 [24761] Ajout test pour vérifier qu'il y a bien des trades dont générer les événements
                if (trades.Count() > 0)
                {
                    if (IsSlaveCallEvents(parallelProcess))
                    {
                        New_EventsGenAPI.CreateEventsAsync(this, trades, ProductTools.GroupProductEnum.Risk).Wait();
                    }
                    else
                    {
                        CreateRiskEventsAsync(trades).Wait();
                    }
                }
            });
        }
    }
}
