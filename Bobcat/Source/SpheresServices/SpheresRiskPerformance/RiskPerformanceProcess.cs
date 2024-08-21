using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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
using EFS.Spheres.DataContracts;
using EFS.Spheres.Hierarchies;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.DataContracts;
using EFS.SpheresRiskPerformance.EntityMarket;
using EFS.SpheresRiskPerformance.Enum;
using EFS.SpheresRiskPerformance.EOD;
using EFS.SpheresRiskPerformance.External;
using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.SpheresRiskPerformance.Properties;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML.v30.MarginRequirement;
//
using FixML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// The main process executing the risk evaluation for the given entity
    /// </summary>
    public partial class RiskPerformanceProcess : RiskPerformanceProcessBase
    {

        /// <summary>
        /// Actor Hierarchy created starting from the entity (base pour identifier l'ensemble des acteurs MARGINREQOFFICE)
        /// </summary>
        ISpheresHierarchy<ActorNode, ActorRelationship> m_ActorRoleHierarchyDescendingFromEntity = new ActorRoleHierarchy();

        /// <summary>
        /// List of the markets treating allocation on derivative contracts connected to the entity
        /// </summary>
        readonly MarketsDictionary m_MarketsCollectionFromEntity = new MarketsDictionary();

        /// <summary>
        /// List containing all the actors role MARGINREQOFFICE, foreach list element will be performed a complete risk margin evaluation
        /// </summary>
        readonly List<ActorNodeWithSpecificRoles> m_ActorsRoleMarginReqOffice = new List<ActorNodeWithSpecificRoles>();

        /// <summary>
        /// Positions list
        /// </summary>
        /// PM 20170313 [22833] Remplacé par m_EvaluationRepository
        //PositionsRepository m_PositionsRepository = new PositionsRepository();

        /// <summary>
        /// Données d'évaluation du risque
        /// </summary>
        /// PM 20170313 [22833] new
        readonly RiskRepository m_EvaluationRepository = new RiskRepository();

        /// <summary>
        /// Hierarchy list descending from all the  Clearers existant into the positions collection (m_PositionsRepository).
        /// </summary>
        readonly List<ISpheresHierarchy<ActorNode, ActorRelationship>> m_ActorRoleHierarchiesDescendingFromClearer =
            new List<ISpheresHierarchy<ActorNode, ActorRelationship>>();

        /// <summary>
        /// Factory/repository for calculation methods
        /// </summary>
        RiskMethodFactory m_MethodFactory = new RiskMethodFactory();

        /// <summary>
        /// Results of the risk evaluation (including log details)
        /// </summary>
        readonly CalculationSheetRepository m_LogsRepository = new CalculationSheetRepository();

        /// <summary>
        /// Diagnostics helper class
        /// </summary>
        readonly IMRequestDiagnostics m_ImRequestDiagnostics = new IMRequestDiagnostics();

        // PM 20131217 [19365] Add m_AllPreviousTradeRisk
        /// <summary>
        /// Résultat du précédant calcul de déposit
        /// </summary>
        List<TradeRisk> m_AllPreviousTradeRisk = new List<TradeRisk>();

        #region Accessors
        /// <summary>
        /// List of the markets treating allocation on derivative contracts connected to the entity
        /// </summary>
        // pm 20180918 [XXXXX] Ajout accesseur MarketsCollectionFromEntity suite test Prisma Eurosys
        public MarketsDictionary MarketsCollectionFromEntity
        {
            get { return m_MarketsCollectionFromEntity; }
        }
        #endregion Accessors

        /// <summary>
        ///  Calcul du deposit
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pService"></param>
        public RiskPerformanceProcess(RiskPerformanceMQueue pQueue, AppInstanceService pService) : base(pQueue, pService)
        {

        }

        /// <summary>
        /// Reset the process collection
        /// </summary>
        // EG 20180803 PERF Truncate au lieu de Delete IMACTORPOS_{BuildTableId}_W
        public void Reset()
        {
            m_MarketsCollectionFromEntity.Clear();

            m_ActorsRoleMarginReqOffice.Clear();

            // PM 20170313 [22833] Remplacé par m_EvaluationRepository
            //m_PositionsRepository.DeleteImAssetEtd(this.Cs, this.appInstance.SessionId);
            //m_PositionsRepository.Reset();
            m_EvaluationRepository.ClearSessionData(Cs);
            m_EvaluationRepository.Clear();

            m_ActorRoleHierarchiesDescendingFromClearer.Clear();

            ((ActorRoleHierarchy)m_ActorRoleHierarchyDescendingFromEntity).TruncateImActor(this.Cs);

            // RD 20130419 [18575] 
            ActorRoleHierarchy.TruncateImActorPos(this.Cs);

            m_ActorRoleHierarchyDescendingFromEntity = new ActorRoleHierarchy();

            // PM 20160404 [22116]
            // m_MethodFactory.Methods.Clear();
            m_MethodFactory.Clear();

            m_MethodFactory = new RiskMethodFactory();

            m_LogsRepository.Reset();

            // PM 20131217 [19365] Add m_AllPreviousTradeRisk
            m_AllPreviousTradeRisk.Clear();
        }

        /// <summary>
        /// Perform a riskperformance processus
        /// </summary>
        /// <returns>the output status of the executed processus</returns>
        public Cst.ErrLevel ExternalCall()
        {
            this.ProcessInitialize();

            if (ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS)
            {
                ProcessPreExecute();
            }

            if (ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS)
            {
                ProcessExecuteSpecific();
            }
            return ProcessState.CodeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160307 [XXXXX] Modify
        /// FI 20161021 [22152] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void ProcessInitialize()
        {
            try
            {
                base.ProcessInitialize();

                if (false == IsProcessObserver)
                {
                    ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                    if (ProcessTuningSpecified)
                    {
                        LogDetailEnum = ProcessTuning.LogDetailEnum;


                        Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                    }
                }

                ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;

                //FI 20160307 [XXXXX] Appel ici puisque la méthode Reset fait appel à des requêtes présentes sous DataContractHelper
                //Avec cette initilisation il est possible de traiter sur Oracle alors que le précédent appel était sous SQLserver (et vis et versa)  
                if (false == IsProcessObserver)
                {
                    DataContractHelper.Init(this.Cs, Session.BuildTableId());
                    Reset();

                    ProcessState.CodeReturn = InitializeProcessInfo();

                    if (ProcessCall == ProcessCallEnum.Master)
                    {
                        if (((RiskPerformanceMQueue)MQueue).IsCashBalanceProcess)
                        {

                            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4000), 0,
                                new LogParam(CurrentId, MQueue.Identifier, "ACTOR"),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness))));
                        }
                        else if (((RiskPerformanceMQueue)MQueue).IsDepositProcess) // FI 20161021 [22152] add IsDepositProcess
                        {

                            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1000), 0,
                                new LogParam(CurrentId, MQueue.Identifier, "ACTOR"),
                                new LogParam(m_ProcessInfo.CssId, m_ProcessInfo.CssIdentifier, "ACTOR"),
                                new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                                new LogParam(m_ProcessInfo.Timing),
                                new LogParam(m_ProcessInfo.Mode),
                                new LogParam(m_ProcessInfo.Reset)));
                        }
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("process :{0} is not implemented", MQueue.ProcessType.ToString()));
                    }

                    m_Timers.CreateTimer("PROCESSINITIALISE");
                    if (m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1080), 1));
                    }
                    else
                    {
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1040), 1));
                    }

                    /////////////////////////////////////////////////////////////////////////////////////////////

                    InitializeProcessObjects();

                    //PM 20141216 [9700] Eurex Prisma for Eurosys Futures : Bypass
                    if (m_ProcessInfo.SoftwareRequester != Software.SOFTWARE_EurosysFutures)
                    {
                        if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal)
                        {
                            IMRequestHeaderPrepareElements();
                            IMRequestHeaderValidation(ProcessStateTools.StatusProgressEnum);
                        }
                    }
                }

                /////////////////////////////////////////////////////////////////////////////////////////////

#if DEBUGDEV
                Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 2, new LogParam("PROCESS INITIALIZATION"), new LogParam(m_Timers.GetElapsedTime("PROCESSINITIALISE", null))));
#endif
            }
            catch (Exception ex)
            {
                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);
                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(spheresEx);

                Logger.Log(new LoggerData(spheresEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 2));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20181119 PERF Correction post RC
        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override void ProcessPreExecute()
        {
            if (ProcessState.CodeReturn != Cst.ErrLevel.SUCCESS)
            {
                return;
            }
            if (false == IsProcessObserver)
            {
                m_Timers.CreateTimer("PROCESSPREEXECUTE");

                //PM 20141216 [9700] Eurex Prisma for Eurosys Futures
                if (this.m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
                {
                    // Pas de construction de hiérarchie des acteurs : seuls les acteurs avec une position seront considérés, mais sans hiérarchie
                    return;
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////

            try
            {
                base.ProcessPreExecute();

                if (false == IsProcessObserver)
                {
                    if (ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        // Licence initialized by ProcessPreExecute
                        m_LogsRepository.License = this.License;

                        // Build markets list including CSS 
                        ProcessState.CodeReturn = BuildMarketsCSSDictionary();
                    }

                    if (ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS)
                    {
                        
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1041), 1));

                        /* FI 20200120 [XXXXX] Mise commentaire et appel à LoadIMACTORPOS
                        //
                        // EG 20140221 [19575][19666] 
                        // EG 20181119 PERF Correction post RC
                        ActorRoleHierarchy.InsertImActorPos(Cs, m_ProcessInfo.DtBusiness, m_ProcessInfo.CssId, this.SvrInfoConnection);
                        */

                        // PM 20220930 [XXXXX] Ajout
                        // Mise à jour des référentiels référençant un Id de méthode de calcul de déposit 
                        UpdateReferentialWithIdMethod();

                        // Alimentation de IMACTORPOS
                        LoadIMACTORPOS();

                        // Build Entity hierarchy
                        BuildEntityHierarchy();

                        // Extract Role Margin Req Offices (from entity)
                        ExtractActorsRoleMarginReqOfficeEntity();
                    }
                }
            }
            catch (NotSupportedException ex)
            {
                // exceptions that block the process

                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);
                if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal)
                    IMRequestHeaderValidation(ProcessStateTools.StatusErrorEnum);
                throw spheresEx;
            }
            catch (Exception ex)
            {
                // exceptions that do not block the process
                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(spheresEx);

                
                Logger.Log(new LoggerData(spheresEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 1));

                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }

            if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal && ProcessState.CodeReturn != Cst.ErrLevel.SUCCESS)
            {
                IMRequestHeaderValidation(ProcessStateTools.StatusErrorEnum);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("PROCESS PRE-EXECUTION"), new LogParam(m_Timers.GetElapsedTime("PROCESSPREEXECUTE", null))));
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210120 [XXXXX] Add method
        private void InsertImActorPos()
        {
            /* Test DeadLock
            DeadLockGen.GenerateException();
             */
            ActorRoleHierarchy.InsertImActorPos(Cs, m_ProcessInfo.DtBusiness, m_ProcessInfo.CssId);
        }

        #region SetCurrentParallelSettings
        // EG 20180413 [23769] Gestion customParallelConfigSource
        // EG 20181127 PERF Post RC (Step 3)
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override void SetCurrentParallelSettings()
        {
            CurrentParallelSettings = null;
            if (ParallelTools.GetParallelSection("parallelMarginRequirement") is ParallelMarginRequirementSection parallelSection)
                CurrentParallelSettings = parallelSection.GetParallelSettings(this.m_ProcessInfo.EntityIdentifier, this.m_ProcessInfo.CssIdentifier);

            if (null != CurrentParallelSettings)
            {
                bool isParallelInitialMarginCalculation = IsParallelProcess(ParallelProcess.InitialMarginCalculation);
                bool isParallelInitialMarginWriting = IsParallelProcess(ParallelProcess.InitialMarginWriting);
                bool isSlaveCallEvents = IsSlaveCallEvents(ParallelProcess.InitialMarginWriting);
                string _eventGenMode = m_EventGenMode.ToString();
                if ((EventGenModeEnum.Internal == m_EventGenMode) && isSlaveCallEvents)
                    _eventGenMode += " (SlaveCall)";
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1090), 3,
                    new LogParam(isParallelInitialMarginCalculation ? "YES" : "NO"),
                    new LogParam(isParallelInitialMarginCalculation ? Convert.ToString(GetHeapSize(ParallelProcess.InitialMarginCalculation)) : "-"),
                    new LogParam(isParallelInitialMarginCalculation ? Convert.ToString(GetMaxThreshold(ParallelProcess.InitialMarginCalculation)) : "-"),
                    new LogParam(isParallelInitialMarginWriting ? "YES" : "NO"),
                    new LogParam(isParallelInitialMarginWriting ? Convert.ToString(GetHeapSize(ParallelProcess.InitialMarginWriting)) : "-"),
                    new LogParam(isParallelInitialMarginWriting ? Convert.ToString(GetMaxThreshold(ParallelProcess.InitialMarginWriting)) : "-"),
                    new LogParam(_eventGenMode),
                    new LogParam(isParallelInitialMarginWriting ? Convert.ToString(GetHeapSizeEvents(ParallelProcess.InitialMarginWriting)) : "-"),
                    new LogParam(isParallelInitialMarginWriting ? Convert.ToString(GetMaxThresholdEvents(ParallelProcess.InitialMarginWriting)) : "-")));
            }
        }
        #endregion SetCurrentParallelSettings
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20180413 [23769] Gestion customParallelConfigSource 
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            base.ProcessExecuteSpecific();
            Cst.ErrLevel ret = ProcessState.CodeReturn;
            /////////////////////////////////////////////////////////////////////////////////////////////
            try
            {

                #region Eurex Prisma for Eurosys Futures
                //PM 20141216 [9700] Eurex Prisma for Eurosys Futures
                if (this.m_ProcessInfo.SoftwareRequester == Software.SOFTWARE_EurosysFutures)
                {
                    string eurosysSchemaPrefix = Settings.Default.EurosysSchemaPrefix;
                    string eurosysReportPath = Settings.Default.EurosysReportPath;
                    //
                    RiskPerformanceExternal riskPerformanceEurosys = new RiskPerformanceExternal(this, eurosysSchemaPrefix);
                    //
                    // PM 20180918 [XXXXX] Suite test Prisma Eurosys : Ajout lecture des paramètres sur MARKET et CSS
                    riskPerformanceEurosys.BuildMarketsCSSExternal(m_MarketsCollectionFromEntity);
                    //
                    // Consitution de la position
                    // PM 20170313 [22833] PositionsRepository remplacé par RiskRepository
                    //riskPerformanceEurosys.BuildPositionsRepository(m_PositionsRepository);
                    // PM 20180926 [XXXXX] Prisma v8 : correction pour utilisation de IMASSET_ETD_{BuildTableId}_W
                    //riskPerformanceEurosys.BuildPositionsRepository(m_EvaluationRepository);
                    riskPerformanceEurosys.BuildPositionsRepository(m_EvaluationRepository, Session.BuildTableId());
                    //
                    // Initialisation de la méthode de calcul et des déposits à calculer
                    riskPerformanceEurosys.InitializeDeposits(m_MethodFactory, m_ImRequestDiagnostics);
                    //
                    // Calcul des déposits
                    ret = riskPerformanceEurosys.EvaluateDeposits();
                    //
                    // Ecriture des résultats
                    if ((Cst.ErrLevel.IRQ_EXECUTED != ret) && (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                    {
                        ret = riskPerformanceEurosys.CreateResults(m_LogsRepository, eurosysReportPath);
                    }
                    //
                    // Libération des ressources
                    Reset();
                    //
                    if (Cst.ErrLevel.IRQ_EXECUTED == ret)
                    {
                        ProcessState.CodeReturn = ret;
                    }
                    return ProcessState.CodeReturn;
                }
                #endregion Eurex Prisma for Eurosys Futures
                //
                m_Timers.CreateTimer("PROCESSEXECUTESPECIFIC");

                if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1042), 1));

                    // Constitute the positions (for the current actors hierarchy and Books managed by the current Entity) 
                    BuildPositionsRepository();

                    if (Cst.ErrLevel.IRQ_EXECUTED != ret)
                    {
                        // PM 20131217 [19365] Add m_AllPreviousTradeRisk
                        using (IDbConnection connection = DataHelper.OpenConnection(this.Cs))
                        {
                            // Lire tous les résultats précédant
                            // FI 20210222 [XXXXX] Usage de tryMultiple
                            TryMultiple tryMultiple = new TryMultiple(this.Cs, "LoadAllPreviousTradeRisks", "LoadAllPreviousTradeRisks")
                            {
                                SetErrorWarning = this.ProcessState.SetErrorWarning,
                                IsModeTransactional = false,
                                ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
                            };
                            m_AllPreviousTradeRisk = tryMultiple.Exec<IDbConnection, MarketsDictionary, RiskEvaluationMode, List<TradeRisk>>(delegate (IDbConnection arg1, MarketsDictionary arg2, RiskEvaluationMode arg3) { return LoadAllPreviousTradeRisks(arg1, arg2, arg3); }, connection, m_MarketsCollectionFromEntity, this.m_ProcessInfo.Mode);
                        }

                        if (m_EvaluationRepository.EvaluationData.ContainData() || ((m_AllPreviousTradeRisk != default(List<TradeRisk>)) && (m_AllPreviousTradeRisk.Where(t => t.IsClearer).Count() > 0)))
                        {
                            if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
                            {
                                // Build Clearer hierarchies
                                BuildClearerHierarchies();
                            }

                            if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                            {
                                // Extract Role Margin Req Offices (from clearers)
                                ExtractActorsRoleMarginReqOfficeClearer();
                            }
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            // Search Previous Evaluations
                            FindPreviousTradeRisks();
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            // Dispatch the constituted position for each actor
                            DispatchPositionsToMarginReqOfficeActors();
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            // build empty deposits
                            BuildRootElements();
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            // Assign the risk margin objects (net positions or deposits) along the hierarchy
                            AssignInheritedRiskMarginObjects();
                        }

                        if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                            (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                        {
                            // Intialize the deposits
                            ret = InitializeDeposits();
                        }

                        // IMRequest table initialization
                        if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal)
                        {
                            if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                            {
                                IMRequestPrepareElements();
                            }
                        }
                    }
                }

                // Evaluate the deposits
                // EG 20180205 [23769] New
                if ((Cst.ErrLevel.IRQ_EXECUTED != ret) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret)))
                {
                    SetCurrentParallelSettings();
                    DepositsGen();

                    if (Cst.ErrLevel.IRQ_EXECUTED != ProcessState.CodeReturn)
                    {
                        // Logs (including results) serialization into the Spheres DB
                        WriteCalculationSheets();

                        if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal)
                        {
                            // IMREquest validation
                            IMRequestValidation();
                        }

                        if (false == IsParallelProcess(ParallelProcess.InitialMarginWriting))
                        {
                            #region  EventGen
                            if (EventGenModeEnum.Internal == this.m_EventGenMode)
                                EventsGen();
                            else if (EventGenModeEnum.External == this.m_EventGenMode)
                                SendMessageQueueToEvent();
                            else
                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", m_EventGenMode.ToString()));
                            #endregion  EventGen
                        }
                    }
                }
                else
                {
                    ProcessState.CodeReturn = ret;
                }

                // process end
            }
            catch (Exception ex)
            {
                // FI 20220803 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(spheresEx);

                
                Logger.Log(new LoggerData(spheresEx));
                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.SYS, 1018), 1));

                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }

            if (false == IsProcessObserver)
            {
                DefineIMRequestHeaderGlobalStatus();
                // free ressources process
                Reset();
            }
            /////////////////////////////////////////////////////////////////////////////////////////////

#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("PROCESS EXECUTION"), new LogParam(m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", null))));
#endif
            return ProcessState.CodeReturn;

        }

        /// <summary>
        /// Initialize generic technical properties
        /// </summary>
        /// FI 20160307 [XXXXX] Modify
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void InitializeProcessObjects()
        {
            SerializationHelper.SerializationDirectory = this.Session.GetTemporaryDirectory(AppSession.AddFolderSessionId.True);

            this.m_MarketsCollectionFromEntity.InitDelegate(this.ProcessState.SetErrorWarning);

            // PM 20170313 [22833] m_PositionsRepository remplacé par m_EvaluationRepository
            m_EvaluationRepository.InitializeDeletage(LogDetailEnum,  this.ProcessState.AddCriticalException);

            //FI 20160307 [XXXXX] Mise en commentaire
            //DataContractHelper.Init(this.Cs);
                        
            this.m_LogsRepository.Cs = Cs;

            this.m_LogsRepository.ProcessInfo = this.m_ProcessInfo;
            
            this.m_LogsRepository.InitDelegate(LogTools.AddAttachedDoc, this.ProcessState.SetErrorWarning, this.ProcessState.AddCriticalException);
            
            this.m_LogsRepository.AppSession = this.Session; 
            this.m_LogsRepository.LogDetailEnum = this.LogDetailEnum;
            // PM 20170313 [22833] m_PositionsRepository remplacé par m_EvaluationRepository
            //this.m_LogsRepository.AssetETDCache = this.m_PositionsRepository.AssetETDCache;
            this.m_LogsRepository.AssetETDCache = this.m_EvaluationRepository.EvaluationData.AssetETDCache;
            
            this.m_ImRequestDiagnostics.AppSession = this.Session;
            this.m_ImRequestDiagnostics.InitDelegate(this.ProcessState.SetErrorWarning, this.ProcessState.AddCriticalException);

            this.m_ImRequestDiagnostics.Initialize(this.Cs, this.m_ProcessInfo.Entity, this.m_ProcessInfo.Timing, this.m_ProcessInfo.IdPr);
        }

        /// <summary>
        /// Alimentation de la table IMACTORPOS
        /// </summary>
        /// FI 20200120 [XXXXX] Add Method
        private void LoadIMACTORPOS()
        {

            // FI 20200120 [XXXXX] use TryMultiple afin d'appliquer n tentatives 
            TryMultiple tryMultiple = new TryMultiple(Cs, "InsertImActorPos", "Load Table IMACTORPOS")
            {
                SetErrorWarning = ProcessState.SetErrorWarning,
                Timeout = 240, //timeout (4 minutes)
                IsModeTransactional = false,
                ThreadSleep = 5 //blocage de 5 secondes entre chaque tentative
            };
            tryMultiple.Exec(delegate () { InsertImActorPos(); });

            if (DataHelper.GetSvrInfoConnection(Cs).IsOracle)
                DataHelper.UpdateStatTable(Cs, string.Format("IMACTORPOS_{0}_W", Session.BuildTableId()).ToUpper());
        }


        // EG 20181119 PERF Correction post RC (Step 2)
        private void BuildEntityHierarchy()
        {
            m_ActorRoleHierarchyDescendingFromEntity.InitDelegates(ProcessState.SetErrorWarning);
            m_ActorRoleHierarchyDescendingFromEntity.InitNodeFactoryDelegate(RiskHierarchyFactory.GetNodeInstance);
            m_ActorRoleHierarchyDescendingFromEntity.InitAttributeFactoryDelegate(RiskHierarchyFactory.GetAttributeInstance);

            m_ActorRoleHierarchyDescendingFromEntity.BuildHierarchy(this.Cs, m_ProcessInfo.Entity);

            // 20120712 MF Ticket 18004
            ((ActorRoleHierarchy)m_ActorRoleHierarchyDescendingFromEntity).InsertImActor(this.Cs,  Session.BuildTableId());

            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<ActorRoleHierarchy>(
                    (ActorRoleHierarchy)m_ActorRoleHierarchyDescendingFromEntity, new SysMsgCode(SysCodeEnum.LOG, 1069),
                    this.AppInstance.AppRootFolder, "ActorRoleHierarchyFromEntity.xml",
                    RiskHierarchyFactory.ExtraRolesTypes, this.ProcessState.AddCriticalException);
            }
        }

        private Cst.ErrLevel BuildMarketsCSSDictionary()
        {
            // TODO MF 20110520, V2 traitera plusieurs chambres par instance de processus 
            object[] cssBoxedId = new object[] { this.m_ProcessInfo.CssId };

            // the market/clearing house collection descend from the current entity
            m_MarketsCollectionFromEntity.BuildDictionary(this.Cs, m_ProcessInfo.Entity, cssBoxedId);

            // loaded business dates validation check
            Cst.ErrLevel errValidDate =
                m_MarketsCollectionFromEntity.ValidateDateValues(m_ProcessInfo.DtBusiness, m_ProcessInfo.Mode, m_ProcessInfo.Timing);

            Cst.ErrLevel errEOD = Cst.ErrLevel.SUCCESS;

            // EOD control validation check
            if (m_ProcessInfo.Timing == SettlSessIDEnum.EndOfDay)
            {
                errEOD = m_MarketsCollectionFromEntity.EndOfDayControl(this.Cs);
            }

            Cst.ErrLevel err = Cst.ErrLevel.SUCCESS;

            if (errValidDate != Cst.ErrLevel.SUCCESS || errEOD != Cst.ErrLevel.SUCCESS)
            {
                err = Cst.ErrLevel.FAILURE;
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<MarketsDictionary>(m_MarketsCollectionFromEntity,
                    new SysMsgCode(SysCodeEnum.LOG, 1066), this.AppInstance.AppRootFolder,
                    "MarketsList.xml", null,  this.ProcessState.AddCriticalException);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

            return err;
        }

        // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTORPOS_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void BuildClearerHierarchies()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1046), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            m_ActorRoleHierarchiesDescendingFromClearer.Clear();

            // we build one hierarchy for each clearer connected to the loaded positions set
            // PM 20170313 [22833] m_PositionsRepository remplacé par m_EvaluationRepository
            //int[] clearerIds =
            //    (from position
            //         in m_PositionsRepository.Positions
            //     select position.Key.idA_Clearer).Distinct().ToArray();
            int[] clearerIds = m_EvaluationRepository.EvaluationData.GetClearersIdA();

            // PM 20131217 [19365] Add tradeRiskClearerIds
            int[] tradeRiskClearerIds = (
                from tradeRisk in m_AllPreviousTradeRisk
                where (tradeRisk.IsClearer == true)
                select tradeRisk.ActorId).Distinct().ToArray();
            tradeRiskClearerIds = tradeRiskClearerIds.Except(clearerIds).ToArray();

            if (!ArrFunc.IsEmpty(clearerIds) || !ArrFunc.IsEmpty(tradeRiskClearerIds))
            {
                if (!ArrFunc.IsEmpty(clearerIds))
                {
                    foreach (int clearerId in clearerIds)
                    {
                        ISpheresHierarchy<ActorNode, ActorRelationship> actorRoleHierarchyDescendingFromClearer = new ActorRoleHierarchy();
                        actorRoleHierarchyDescendingFromClearer.InitDelegates(ProcessState.SetErrorWarning);
                        actorRoleHierarchyDescendingFromClearer.InitNodeFactoryDelegate(RiskHierarchyFactory.GetNodeInstance);
                        actorRoleHierarchyDescendingFromClearer.InitAttributeFactoryDelegate(RiskHierarchyFactory.GetAttributeInstance);
                        actorRoleHierarchyDescendingFromClearer.BuildHierarchy(this.Cs, clearerId);
                        m_ActorRoleHierarchiesDescendingFromClearer.Add(actorRoleHierarchyDescendingFromClearer);
                    }
                    // Si des clearers ne sont pas en position mais ont des trades risk
                    if (!ArrFunc.IsEmpty(tradeRiskClearerIds))
                    {
                        foreach (int clearerId in tradeRiskClearerIds)
                        {
                            // CC/PM 20131220 [] Ajout vérification qu'aucune hiérarchie de clearer ne comporte déjà le clearer du trade risk
                            bool alreadyExist = false;
                            foreach (ISpheresHierarchy<ActorNode, ActorRelationship> actorHierarchie in m_ActorRoleHierarchiesDescendingFromClearer)
                            {
                                // Vérifier que le clearer du trade risque n'est pas déjà racine d'une hiérarchie de clearers
                                if (actorHierarchie.Root.Id != clearerId)
                                {
                                    // Vérifier que le clearer du trade risque n'est pas déjà enfant d'une hiérarchie de clearers
                                    List<ActorNode> allChilds = actorHierarchie.Root.FindChilds(n => (n != default(ActorNode)) && (n.Built));
                                    if ((allChilds.Count > 0) && (true == allChilds.Any(n => n.Id == clearerId)))
                                    {
                                        alreadyExist = true;
                                    }
                                }
                            }
                            if (false == alreadyExist)
                            {
                                ISpheresHierarchy<ActorNode, ActorRelationship> actorRoleHierarchyDescendingFromClearer = new ActorRoleHierarchy();
                                actorRoleHierarchyDescendingFromClearer.InitDelegates(ProcessState.SetErrorWarning);
                                actorRoleHierarchyDescendingFromClearer.InitNodeFactoryDelegate(RiskHierarchyFactory.GetNodeInstance);
                                actorRoleHierarchyDescendingFromClearer.InitAttributeFactoryDelegate(RiskHierarchyFactory.GetAttributeInstance);
                                actorRoleHierarchyDescendingFromClearer.BuildHierarchy(Cs, clearerId);
                                m_ActorRoleHierarchiesDescendingFromClearer.Add(actorRoleHierarchyDescendingFromClearer);
                            }
                        }
                    }
                }
                else
                {
                    // Il n'y a pas de position :
                    // Ajouter les hiérarchies de clearer provenant des trades risk existant
                    foreach (int clearerId in tradeRiskClearerIds)
                    {
                        ISpheresHierarchy<ActorNode, ActorRelationship> actorRoleHierarchyDescendingFromClearer = new ActorRoleHierarchy();
                        actorRoleHierarchyDescendingFromClearer.InitDelegates( ProcessState.SetErrorWarning);
                        actorRoleHierarchyDescendingFromClearer.InitNodeFactoryDelegate(RiskHierarchyFactory.GetNodeInstance);
                        actorRoleHierarchyDescendingFromClearer.InitAttributeFactoryDelegate(RiskHierarchyFactory.GetAttributeInstance);
                        actorRoleHierarchyDescendingFromClearer.BuildHierarchy(this.Cs, clearerId);
                        m_ActorRoleHierarchiesDescendingFromClearer.Add(actorRoleHierarchyDescendingFromClearer);
                    }
                }

                ///////////////////////////////////////////////////////////////////////////////////////////// 

                // Dump...

                if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
                {
                    // casting object just for serialization purpose (otherwise the DumpObjectToFile will fail)
                    List<ActorRoleHierarchy> castedObjects = new List<ActorRoleHierarchy>();
                    foreach (ISpheresHierarchy<ActorNode, ActorRelationship> hierarchy in m_ActorRoleHierarchiesDescendingFromClearer)
                    {
                        castedObjects.Add((ActorRoleHierarchy)hierarchy);
                    }
                    
                    SerializationHelper.DumpObjectToFile<List<ActorRoleHierarchy>>(castedObjects, new SysMsgCode(SysCodeEnum.LOG, 1070),
                        this.AppInstance.AppRootFolder, "ActorRoleHierarchiesFromClearers.xml",
                        RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
                }

            }
            else
            {
                throw new NotSupportedException(Ressource.GetString("RiskPerformance_ERRNoClearingHouses"));
            }
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void ExtractActorsRoleMarginReqOfficeEntity()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1047), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            ActorNodeWithSpecificRoles[] vectorEntity = ExtractActorsRoleMarginReqOffice(m_ActorRoleHierarchyDescendingFromEntity);

            if (vectorEntity != null)
            {
                m_ActorsRoleMarginReqOffice.AddRange(vectorEntity);
            }
            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1071),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeFromEntity.xml",
                    RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
            }

        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void ExtractActorsRoleMarginReqOfficeClearer()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1047), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (ISpheresHierarchy<ActorNode, ActorRelationship> clearerHierarchy in m_ActorRoleHierarchiesDescendingFromClearer)
            {
                ActorNodeWithSpecificRoles[] vectorClearers = ExtractActorsRoleMarginReqOffice(clearerHierarchy);
                if (vectorClearers != null)
                {
                    m_ActorsRoleMarginReqOffice.AddRange(vectorClearers);
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1071),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeEntityAndClearers.xml",
                    RiskHierarchyFactory.ExtraRolesTypes, this.ProcessState.AddCriticalException);
            }
        }

        /// <summary>
        /// Extract actors MARGINREQOFFICE for the goven hierarchy
        /// </summary>
        /// <param name="pHierarchy">hierarchy where on search for the MARGINREQOFFICE actors</param>
        private ActorNodeWithSpecificRoles[] ExtractActorsRoleMarginReqOffice(ISpheresHierarchy<ActorNode, ActorRelationship> pHierarchy)
        {

            // Define a temporary list ActorNode (in order to use base class ActorNode)
            List<ActorNode> actorsRoleMarginReqOffice = new List<ActorNode>();
            // role check for the root, add it to the temporary list when it has role ROLEMARGINREQOFFICE
            if (pHierarchy.Root.RolesList.Find(
                relation => relation.Role == RolesCollection.ROLEMARGINREQOFFICE) != null)
            {
                actorsRoleMarginReqOffice.Add(pHierarchy.Root);
            }

            // role check for all the nodes of the root hierarchy, add it to the temporary list when they have role ROLEMARGINREQOFFICE
            actorsRoleMarginReqOffice.AddRange(pHierarchy.Root.FindChilds(
                node => node != null && node.Built == true));

            actorsRoleMarginReqOffice = actorsRoleMarginReqOffice.FindAll(
                node =>
                    (node.RolesList.Find(relation =>
                        relation.Role == RolesCollection.ROLEMARGINREQOFFICE) != null));

            // Copy ActorNodeWithSpecificRoles types nodes to the main list

            ActorNodeWithSpecificRoles[] vectorMRO = null;
            if (actorsRoleMarginReqOffice.Count > 0)
            {
                vectorMRO = new ActorNodeWithSpecificRoles[actorsRoleMarginReqOffice.Count];
                actorsRoleMarginReqOffice.CopyTo(vectorMRO);
            }
            return vectorMRO;

            /////////////////////////////////////////////////////////////////////////////////////////////

        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void FindPreviousTradeRisks()
        {
            m_Timers.CreateTimer("FINDPREVIOUSRISKRESULTS");
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1043), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            IEnumerable<RoleAttribute> attributes =
                from actorMarginReqOffice in m_ActorsRoleMarginReqOffice
                from role in actorMarginReqOffice.RoleSpecificAttributes
                where role is RoleMarginReqOfficeAttribute
                select role;

            // PM 20131217 [19365] Remplacé par m_AllPreviousTradeRisk
            //using (IDbConnection pConnection = DataHelper.OpenConnection(this.Cs))
            //{
            //    // PM 20131009 [19046] Added
            //    // Lire tous les résultats précédant
            //    List<TradeRisk> allPreviousTradeRisk = LoadAllPreviousTradeRisks(pConnection, this.m_MarketsCollectionFromEntity, this.m_ProcessInfo.Mode);

            foreach (RoleMarginReqOfficeAttribute attribute in attributes)
            {
                // PM 20131010 [19046] Replaced for optimisation
                //attribute.LoadPreviousTradeRisks(pConnection, this.m_MarketsCollectionFromEntity, this.m_ProcessInfo.Mode);
                // PM 20131217 [19365] Remplacé par m_AllPreviousTradeRisk
                //attribute.LoadPreviousTradeRisks(allPreviousTradeRisk);
                attribute.LoadPreviousTradeRisks(m_AllPreviousTradeRisk);

                foreach (TradeRisk prevresult in attribute.Results)
                {
                    if (this.m_ProcessInfo.Reset)
                    {
                        prevresult.ReEvaluation = RiskRevaluationMode.EvaluateWithUpdate;
                    }
                    else if (prevresult.Timing == SettlSessIDEnum.Intraday)
                    {
                        prevresult.ReEvaluation = RiskRevaluationMode.NewEvaluation;
                    }
                    else // EndOfDay and reset == false
                    {
                        prevresult.ReEvaluation = RiskRevaluationMode.DoNotEvaluate;
                    }
                }
            }
            //}

            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                
                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1072),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeIncludingPrevRisk.xml",
                    RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
            }

#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("LOADING PREVIOUS INITIAL MARGINS"), new LogParam(m_Timers.GetElapsedTime("FINDPREVIOUSRISKRESULTS", null))));
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pConnection"></param>
        /// <param name="pMarketsCollectionFromEntity"></param>
        /// <param name="pCurrentMode"></param>
        /// <returns></returns>
        // PM 20131009 [19046] Added
        private static List<TradeRisk> LoadAllPreviousTradeRisks(IDbConnection pConnection, MarketsDictionary pMarketsCollectionFromEntity, RiskEvaluationMode pCurrentMode)
        {
            List<TradeRisk> riskResults = new List<TradeRisk>();

            Cst.StatusEnvironment stEnv = CalculationSheetRepository.RiskEvaluationModeToStatusEnvironment(pCurrentMode);
            string sqlrequest = DataContractHelper.GetQuery(DataContractResultSets.RISKALLRESULTS);
            CommandType sqlrequesttype = DataContractHelper.GetType(DataContractResultSets.RISKALLRESULTS);

            // FI 20210118 [XXXXX] L'appel à XQueryTransform n'est pas nécessaire puisque la requête ne s'appuie sur aucun champs XML   
            //sqlrequest = DataHelper<MarginRequirementTrade>.XQueryTransform(pConnection, sqlrequesttype, sqlrequest);

            foreach (int cssId in pMarketsCollectionFromEntity.GetCSSInternalIDs())
            {
                DateTime dtBusiness = pMarketsCollectionFromEntity.GetBusinessDate(cssId);

                // RD 20170420 [23094] IDA_ENTITY Add parameter
                Dictionary<string, object> parameterValues = new Dictionary<string, object>
                {
                    { "DTBUSINESS", dtBusiness },
                    { "IDA_CSSCUSTODIAN", cssId },
                    { "IDSTENVIRONMENT", stEnv },
                    { "IDA_ENTITY", pMarketsCollectionFromEntity.EntityId }
                };

                List<MarginRequirementTrade> marginReqTrades =
                    DataHelper<MarginRequirementTrade>.ExecuteDataSet(pConnection, sqlrequesttype, sqlrequest,
                    DataContractHelper.GetDbDataParameters(DataContractResultSets.RISKALLRESULTS, parameterValues));

                // PM 20131217 [19365] Add IsClearer
                List<TradeRisk> tempRiskResults = (
                    from marginReq
                        in marginReqTrades
                    select
                        new TradeRisk(
                            marginReq.Trade,
                            marginReq.TradeId,
                            marginReq.TradeName,
                            dtBusiness,
                            cssId,
                            marginReq.ActorId,
                            marginReq.BookId,
                            marginReq.Timing,
                            marginReq.IsClearer)
                    ).ToList<TradeRisk>();

                riskResults.AddRange(tempRiskResults);
            }
            return riskResults;
        }

        // EG 20181119 PERF Correction post RC (Step 2)
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void BuildPositionsRepository()
        {
            m_Timers.CreateTimer("BUILDBOOKSPOSITIONSDICTIONARY");
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1045), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            // PM 20170313 [22833] Remplacement de m_PositionsRepository par m_EvaluationRepository
            //m_PositionsRepository.Reset();
            //m_PositionsRepository.SettlSessID = this.m_ProcessInfo.Timing;

            //// 1. building the Ids market collection (relative to the demanded clearing house)

            //m_PositionsRepository.Markets =
            //    this.m_ProcessInfo.Timing == SettlSessIDEnum.EndOfDay ?
            //        (from marketcss in this.m_MarketsCollectionFromEntity select (object)marketcss.Value.MarketId).ToArray<object>()
            //        :
            //        (from marketcss in this.m_MarketsCollectionFromEntity
            //         where marketcss.Value.MarketIsIntradayExeAssActivated
            //         select (object)marketcss.Value.MarketId).ToArray<object>();

            //m_PositionsRepository.MarketsWithoutIntraDayExeAssActivated =
            //    this.m_ProcessInfo.Timing == SettlSessIDEnum.Intraday ?
            //        (from marketcss in this.m_MarketsCollectionFromEntity
            //         where !marketcss.Value.MarketIsIntradayExeAssActivated
            //         select (object)marketcss.Value.MarketId).ToArray<object>()
            //        :
            //        null;

            m_EvaluationRepository.InitializeMarkets(m_ProcessInfo.Timing, m_MarketsCollectionFromEntity);

            // 2. building the IDs collection of all the actors descending by the entity (used only wit the original positions loading strategy)
            object[] actorsBoxedId = null;

            // 20120712 MF Ticket 18004
            if (Settings.Default.UseOriginalTradeLoading)
            {
                List<ActorNode> actors = new List<ActorNode>
                {
                    this.m_ActorRoleHierarchyDescendingFromEntity.Root
                };
                // Using Union instead AddRange to ge rid of elements duplicated
                actors = actors.Union(
                    this.m_ActorRoleHierarchyDescendingFromEntity.Root.FindChilds(actornode => actornode != null && actornode.Built))
                    .ToList();
                actorsBoxedId = (from actor in actors select (object)actor.Id).ToArray<object>();
            }

            // 3. finding the min business date in the market/entity couples collection descending by the entity
            // TODO MF 20110525 il faudra selectionner la business date d'une manière spécifique 
            //  par chambre, lorsque on traitera plusieurs chambres ?
            DateTime minDtBusiness = m_MarketsCollectionFromEntity.GetMinBusinessDate();

            // 4. get the method types
            // PM 20160404 [22116] Suppression de methodTypes qui n'est pas utilisé
            //IEnumerable<InitialMarginMethodEnum> methodTypes =
            //    (from elem in this.m_MarketsCollectionFromEntity select m_MethodFactory.ParseRiskMethodDescription(elem.Value.CssMarginMethod))
            //    .Distinct();

            // PM 20170808 [23371] Ajout dtBusinessNext
            DateTime dtBusinessNext = m_MarketsCollectionFromEntity.First(em => em.Value.DateBusiness == minDtBusiness).Value.DateBusinessNext;

            // 5. loading trades and trade-actions, then constituting the global positions set
            //PM 20140218 [19493] Ajout paramètre dtPositionTime
            if (this.m_ProcessInfo.Timing == SettlSessIDEnum.Intraday)
            {
                minDtBusiness += this.m_ProcessInfo.PositionTime;
            }
            // PM 20160404 [22116] Suppression du paramètre pMethodTypes qui n'est pas utilisé
            //m_PositionsRepository.BuildRepository(this.Cs,
            //    // 20120716 MF Ticket 18004 - pass SessionId
            //    this.m_ProcessInfo.Entity, minDtBusiness, methodTypes, this.appInstance, actorsBoxedId);
            // PM 20170313 [22833] m_PositionsRepository remplacé par m_EvaluationRepository (actorsBoxedId n'est plus utilisé)
            //m_PositionsRepository.BuildRepository(this.Cs, this.m_ProcessInfo.Entity, minDtBusiness, this.appInstance, actorsBoxedId);
            // PM 20170808 [23371] Ajout dtBusinessNext
            //m_EvaluationRepository.BuildRepository(appInstance, Cs, m_ProcessInfo.Entity, minDtBusiness);
            m_EvaluationRepository.BuildRepository( Session, Cs, m_ProcessInfo.Entity, minDtBusiness, dtBusinessNext);

            // PM 20170313 [22833] N'est plus utile, cela est réalisé directement dans BuildRepository
            //// 6. save the id asset for all the in position assets
            //m_PositionsRepository.InsertImAssetEtd(this.Cs, this.appInstance.SessionId);

            /////////////////////////////////////////////////////////////////////////////////////////////

            // Dump results...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<RiskRepository>(m_EvaluationRepository, new SysMsgCode(SysCodeEnum.LOG, 1067), AppInstance.AppRootFolder,
                    "TradesPositionsRepository.xml", null,  this.ProcessState.AddCriticalException);
            }

#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("LOADING POSITIONS"), new LogParam(m_Timers.GetElapsedTime("BUILDBOOKSPOSITIONSDICTIONARY", null))));
#endif
        }

        /// <summary>
        /// for each node with role MARGINREQOFFICE, we assign to him the positions which will affect its risk margin evaluation
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void DispatchPositionsToMarginReqOfficeActors()
        {

            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1048), 2));
            Logger.Write();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (ActorNodeWithSpecificRoles marginReqOffice in this.m_ActorsRoleMarginReqOffice)
            {
                // 1. get the positions for the current MRO actor
                // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
                //Dictionary<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> positionsSubTreeMarginReqOffice = GetMarginReqOfficePositions(marginReqOffice);
                Dictionary<int, RiskData> positionsSubTreeMarginReqOffice = GetMarginReqOfficePositions(marginReqOffice);

                // 2. copying the extracted positions from the temporary container to the container field of the current MARGINREQOFFICE actor
                // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
                //foreach (KeyValuePair<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> element
                //    in positionsSubTreeMarginReqOffice)
                foreach (KeyValuePair<int, RiskData> element in positionsSubTreeMarginReqOffice)
                {
                    List<RoleAttribute> attributesMarginReqOffice =
                        marginReqOffice.RoleSpecificAttributes.FindAll
                        (attribute => attribute is RoleMarginReqOfficeAttribute);

                    if (attributesMarginReqOffice.Count < 1)
                    {
                        throw new NullReferenceException(
                            String.Format(Ressource.GetString("RiskPerformance_ERRMissingAttributeRoleMarginReOffice"),
                            // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskRepository
                            //element.Value.Count,
                            element.Value.Count(),
                            marginReqOffice.Identifier, marginReqOffice.Id));
                    }

                    // 3. each position wil be assigned to all the MARGINREQOFFICE attributes defined over the current MARGINREQOFFICE actor 
                    foreach (RoleMarginReqOfficeAttribute attributeMarginReqOffice in attributesMarginReqOffice)
                    {
                        // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
                        //attributeMarginReqOffice.Positions.Add(element.Key, element.Value);
                        attributeMarginReqOffice.MRORiskData.Add(element.Key, element.Value);
                    }
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////
            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
# if DEBUGDEV
                SerializationHelper.DumpObjectToFile<ActorRoleHierarchy>((ActorRoleHierarchy)m_ActorRoleHierarchyDescendingFromEntity, new SysMsgCode(SysCodeEnum.LOG, 1068),
                this.AppInstance.AppRootFolder, "ActorRoleHierarchyFromEntityWithPositions.xml", RiskHierarchyFactory.ExtraRolesTypes,
                this.ProcessState.AddCriticalException);
#endif

                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1073),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeWithPositions.xml",
                    RiskHierarchyFactory.ExtraRolesTypes, this.ProcessState.AddCriticalException);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marginReqOffice"></param>
        /// <returns></returns>
        /// PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
        //private Dictionary<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>>GetMarginReqOfficePositions(ActorNodeWithSpecificRoles marginReqOffice)
        private Dictionary<int, RiskData> GetMarginReqOfficePositions(ActorNodeWithSpecificRoles marginReqOffice)
        {
            // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskDataActorBook
            //Dictionary<int, List<Pair<PosRiskMarginKey, RiskMarginPosition>>> positionsSubTreeMarginReqOffice = null;
            Dictionary<int, RiskData> positionsSubTreeMarginReqOffice = null;

            if (!marginReqOffice.IsDescendingFromClearer)
            {
                // 1. marginReqOffice.IsDescendingFromEntity

                //  1.1 Building a list including the actual actor MARGINREQOFFICE + his transversal childs (not MARGINREQOFFICE)
                ActorNode[] actorsSubTreeMarginReqOffice =
                    ((from childNode
                          // 1.1.1 recursive search of the childs depending from the current actor MARGINREQOFFICE
                          in marginReqOffice.FindChilds(node =>
                              // 1.1.2 "Role != DataContractHelper.ROLEMARGINREQOFFICE", the recursive search must stop 
                              //    when another actor node ROLEMARGINREQOFFICE is found. 
                              //    Each actor ROLEMARGINREQOFFICE must be considered separately.
                              (node.RolesList.Find(relation => relation.Role == RolesCollection.ROLEMARGINREQOFFICE)) == null)
                      select childNode).
                      Union((IEnumerable<ActorNode>)new ActorNode[] { marginReqOffice })).
                      ToArray();

                // 1.2 Extracting the positions of the current MARGINREQOFFICE actor from the global positions repository
                // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskRepository et m_PositionsRepository remplacé par m_EvaluationRepository
                //positionsSubTreeMarginReqOffice =
                //    (from actorNode
                //         in actorsSubTreeMarginReqOffice
                //     select new
                //     {
                //         actorId = actorNode.Id,
                //         positions = PositionsExtractor.GetActorPositionsFromRepository(actorNode.Id, m_PositionsRepository.Positions)
                //     }).ToDictionary(elem => elem.actorId, elem => elem.positions.ToList());

                // RD 20170420 [23092] Charger la position en utilisant la liste des Books sur le noeud Actor
                //positionsSubTreeMarginReqOffice = actorsSubTreeMarginReqOffice.ToDictionary(a => a.Id, a => m_EvaluationRepository.EvaluationData.GetActorRiskData(a.Id));
                List<Pair<int, IEnumerable<int>>> booksSubTreeMarginReqOffice =
                   (from actorNode
                        in actorsSubTreeMarginReqOffice
                    where actorNode is ActorNodeWithSpecificRoles
                    select new Pair<int, IEnumerable<int>>
                    (
                        actorNode.Id,
                        from book
                           in ((RoleMarginReqOfficeAttribute)((ActorNodeWithSpecificRoles)actorNode).RoleSpecificAttributes.Find(role => role is RoleMarginReqOfficeAttribute)).Books
                        select book.Id
                    )).
                    Union
                    (from actorNode
                        in actorsSubTreeMarginReqOffice
                     where !(actorNode is ActorNodeWithSpecificRoles)
                     select new Pair<int, IEnumerable<int>>
                     (
                         actorNode.Id,
                         from attribute in actorNode.Attributes
                         from book in attribute.Books
                         select book.Id
                     )).ToList();

                positionsSubTreeMarginReqOffice =
                    actorsSubTreeMarginReqOffice.ToDictionary(
                    a => a.Id,
                    a => m_EvaluationRepository.EvaluationData.GetBooksRiskData(booksSubTreeMarginReqOffice.Find(elem => elem.First == a.Id).Second));
            }
            else
            {
                // 2. marginReqOffice.IsDescendingFromClearer

                // 2.1  Find the books set of the actor

                RoleMarginReqOfficeAttribute attribute =
                    (RoleMarginReqOfficeAttribute)marginReqOffice.RoleSpecificAttributes.Find(role => role is RoleMarginReqOfficeAttribute);

                IEnumerable<int> booksId = from book in attribute.Books select book.Id;

                // 2.2  Extracting the positions of the current books set from the global positions repository

                // PM 20170313 [22833] Remplacement de la liste de position par la classe RiskRepository
                positionsSubTreeMarginReqOffice = new Dictionary<int, RiskData>
                {
                    { marginReqOffice.Id, m_EvaluationRepository.EvaluationData.BuildClearerRiskData(booksId, marginReqOffice.Id) }
                };
            }
            return positionsSubTreeMarginReqOffice;
        }

        /// <summary>
        /// for each node with role MARGINREQOFFICE, we build the needed empty deposit element
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void BuildRootElements()
        {
            m_Timers.CreateTimer("ROOTELEMENTS");

            
            Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 1050), 2));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (ActorNodeWithSpecificRoles marginReqOffice in this.m_ActorsRoleMarginReqOffice)
            {
                foreach (RoleMarginReqOfficeAttribute attribute in marginReqOffice.GetRolesTypeOf<RoleMarginReqOfficeAttribute>())
                {
                    attribute.BuildRootElements();
                }
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                
                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1074),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeWithPositionsAndEmptyDeposits.xml",
                    RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
            }

#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("INITIAL MARGINS INITIALIZATION"), new LogParam(m_Timers.GetElapsedTime("ROOTELEMENTS", null))));
#endif
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void IMRequestPrepareElements()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1049), 1));
            Logger.Write();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            this.m_ImRequestDiagnostics.Roots = this.GetHierarchyRoots();

            foreach (int cssId in m_MarketsCollectionFromEntity.GetCSSInternalIDs())
            {
                // PM 20160404 [22116] m_MethodFactory.Methods => m_MethodFactory.MethodSet
                //IEnumerable<Deposit> depositsInPosition =
                //    from deposit in m_MethodFactory.Method[cssId].Deposits.Values
                //    where !deposit.NotInPosition
                //    select deposit;
                IEnumerable<Deposit> depositsInPosition =
                    from deposit in m_MethodFactory.Deposits(cssId).Values
                    where (false == deposit.NotInPosition)
                    select deposit;

                this.m_ImRequestDiagnostics.PrepareElements
                    (cssId, m_MarketsCollectionFromEntity.GetBusinessDate(cssId), depositsInPosition, false);

                this.m_ImRequestDiagnostics.SetMainPropertiesNewImRequestElements(cssId, this.m_ActorsRoleMarginReqOffice);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AssignInheritedRiskMarginObjects()
        {
            m_Timers.CreateTimer("ASSIGNINHERITEDRISKMARGINOBJECTS");
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1051), 1));
            Logger.Write();
            //
            string[] marginReqOfficeIdentifiers = new string[this.m_ActorsRoleMarginReqOffice.Count];
            string[] marginReqOfficeIds = new string[this.m_ActorsRoleMarginReqOffice.Count];
            int idxMarginReqOfficeId = 0;

            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (ActorNodeWithSpecificRoles marginReqOffice in this.m_ActorsRoleMarginReqOffice)
            {
                /////////////////////////////////////////////////////////////////////////////////////////////

                marginReqOfficeIdentifiers[idxMarginReqOfficeId] = marginReqOffice.Identifier;
                marginReqOfficeIds[idxMarginReqOfficeId] = marginReqOffice.Id.ToString("000000");
                idxMarginReqOfficeId++;

                /////////////////////////////////////////////////////////////////////////////////////////////

                List<RiskElement> accumulator = new List<RiskElement>();
                AssignInheritedRiskMarginObjects(marginReqOffice, accumulator);

                /////////////////////////////////////////////////////////////////////////////////////////////
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

            string marginReqOfficeIdentifiers64Chars =
                marginReqOfficeIdentifiers.Length > 0 ?
                                marginReqOfficeIdentifiers.Aggregate((total, next) => String.Format("{0} {1}", total, next))
                                :
                                Cst.NotAvailable;

            string marginReqOfficeIds64Chars =
                marginReqOfficeIds.Length > 0 ?
                                marginReqOfficeIds.Aggregate((total, next) => String.Format("{0} {1}", total, next))
                                :
                                Cst.NotAvailable;

            marginReqOfficeIdentifiers64Chars =
                marginReqOfficeIdentifiers64Chars.Length > 64 ?
                String.Concat(marginReqOfficeIdentifiers64Chars.Substring(0, 61), "...")
                :
                marginReqOfficeIdentifiers64Chars;

            marginReqOfficeIds64Chars =
                marginReqOfficeIds64Chars.Length > 64 ?
                String.Concat(marginReqOfficeIds64Chars.Substring(0, 61), "...")
                :
                marginReqOfficeIds64Chars;

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1052), 2, new LogParam(marginReqOfficeIdentifiers64Chars), new LogParam(marginReqOfficeIds64Chars)));
            Logger.Write();
            //
            // Dump...

            if (this.LogDetailEnum >= LogLevelDetail.LEVEL3)
            {
                SerializationHelper.DumpObjectToFile<List<ActorNodeWithSpecificRoles>>(m_ActorsRoleMarginReqOffice, new SysMsgCode(SysCodeEnum.LOG, 1067),
                    this.AppInstance.AppRootFolder, "ActorsRoleMarginReqOfficeWithPositionsAndInheritedObjects.xml",
                    RiskHierarchyFactory.ExtraRolesTypes,  this.ProcessState.AddCriticalException);
            }

#if DEBUGDEV
                
                Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 2, new LogParam("OFFICES POSITION FORMATION"), new LogParam(m_Timers.GetElapsedTime("ASSIGNINHERITEDRISKMARGINOBJECTS", null))));
#endif
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void AssignInheritedRiskMarginObjects(ActorNodeWithSpecificRoles pMarginReqOffice, IEnumerable<RiskElement> pAccumulator)
        {
            // 1. Find all the attributes (type MARGINREQOFFICE ) of the current MARGINREQOFFICE actor
            RoleMarginReqOfficeAttribute[] currentMarginReqOfficeAttributes = pMarginReqOffice.GetRolesTypeOf<RoleMarginReqOfficeAttribute>();

            // 2. Find all the ancestors relative to the current actor pMarginReqOffice
            IEnumerable<ActorNode> roots = GetHierarchyRoots();
            // PM 20140114 [19489] First() => FirstOrDefault() + test du default

            ActorNode rootMarginReqOffice = (from root in roots where root.Id == pMarginReqOffice.RootId select root).FirstOrDefault();
            if (rootMarginReqOffice != default(ActorNode))
            {
                List<ActorNode> ancestors = pMarginReqOffice.FindAncestors(rootMarginReqOffice, obj => obj.Built);

                // 3. Select the ancestors having attribute MARGINREQOFFICE
                ActorNodeWithSpecificRoles[] marginReqOfficeAncestors = (
                    from ancestor in ancestors
                    where
                        ancestor is ActorNodeWithSpecificRoles roles &&
                        roles.GetRolesTypeOf<RoleMarginReqOfficeAttribute>().Length > 0
                    select (ActorNodeWithSpecificRoles)ancestor
                    ).ToArray();

                // 4. Duplicate elements suppression
                RemoveDuplicateRiskElements(currentMarginReqOfficeAttributes, pAccumulator, ProcessState.SetErrorWarning);

                // the risk objects inheritance process continue IFF the current MARGINREQOFFICE has any ancestors
                if (!ArrFunc.IsEmpty(marginReqOfficeAncestors))
                {

                    // 5. Find the first hierarchy level (the parents) of the current MARGINREQOFFICE actor
                    List<ActorNode> marginReqOfficeParents =
                        pMarginReqOffice.FindParents(rootMarginReqOffice,
                        parent => parent.Built
                            && parent is ActorNodeWithSpecificRoles roles
                            && roles.GetRolesTypeOf<RoleMarginReqOfficeAttribute>().Length > 0);

                    // 6. Find all the attributes (type MARGINREQOFFICE) of the parents
                    RoleMarginReqOfficeAttribute[] parentsAttributes = (
                        from parent in marginReqOfficeParents
                        from attribute in ((ActorNodeWithSpecificRoles)parent).RoleSpecificAttributes
                        where attribute is RoleMarginReqOfficeAttribute
                        select (RoleMarginReqOfficeAttribute)attribute
                        ).ToArray();

                    // 7. Any parent attribute must inherit the current MARGINREQOFFICE actor positions/deposits
                    foreach (RoleMarginReqOfficeAttribute parentAttribute in parentsAttributes)
                    {
                        RoleMarginReqOfficeAttribute attributeInRegardsWithParent =
                            GetAttributeInRegardsWithParent(currentMarginReqOfficeAttributes, parentAttribute);

                        // 7.1 the risk object inheritance may be done just when we have at least a valid relation 
                        //  between the current MARGINREQOFFICE actor and its own MARGINREQOFFICE parent
                        if (attributeInRegardsWithParent != null)
                        {
                            // 8. Set what kind of risk evaluation objects we have to pass at the previous hierarchy levels
                            //  - getInheritedGrossElements == true, then we pass sum of deposits
                            //  - getInheritedNetElements == true, then we pass sum of positions


                            InheritanceElementsFlags(marginReqOfficeAncestors, out bool getInheritedGrossElements, out bool getInheritedNetElements);

                            // 9. Build the risk elements of the current MARGINREQOFFICE actor 

                            IEnumerable<RiskElement> currentActorRiskElements =
                                attributeInRegardsWithParent.BuildRiskElements
                                (getInheritedGrossElements, getInheritedNetElements, RiskElementClass.ELEMENT);

                            // 9.1. Remove duplicates between the deposit collection and the accumulator risk elements collection
                            RoleMarginReqOfficeAttribute.RemoveDuplicateRiskElements(currentActorRiskElements, pAccumulator,  ProcessState.SetErrorWarning);

                            // 10. merging the accumulator with the new elements into a temporary list

                            IEnumerable<RiskElement> riskElements = currentActorRiskElements.Union(pAccumulator);

                            // 11. Select elements from the temporary list into the toInheritRiskElements list,
                            //  according with the specific parent attribute evaluation mode (IsGrossMargining)

                            RiskElementEvaluation function =
                                parentAttribute.IsGrossMargining ?
                                    RiskElementEvaluation.SumDeposit :
                                    RiskElementEvaluation.SumPosition;

                            IEnumerable<RiskElement> toInheritRiskElements =
                                //from riskElement in pAccumulator
                                from riskElement in riskElements
                                where
                                    riskElement.RiskElementEvaluation == function &&
                                    // the inherit procedure will be activated just in case an IMR book is set for the parent attribute
                                    parentAttribute.IMRBookId > 0
                                select riskElement;

                            // 12. Copy the element from the toInheritRiskElements collection to the parent attribute
                            foreach (RiskElement riskElement in toInheritRiskElements)
                            {
                                // 12.1 duplicate check to avoid to inherit risk elements multiple times, in case
                                //  the element to be inherited (riskElement.ActorId, riskElement.AffectedBookId) 
                                //  is accessible from another tree path

                                Pair<int, int> key = new Pair<int, int>(riskElement.ActorId, riskElement.AffectedBookId);

                                if (!parentAttribute.InheritedElements.ContainsKey(key))
                                {
                                    parentAttribute.InheritedElements.Add(key, riskElement);
                                }
                                else
                                {
                                    
                                    Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.SYS, 1027), 0,
                                        new LogParam(pMarginReqOffice.Identifier),
                                        new LogParam(Convert.ToString(pMarginReqOffice.Id)),
                                        new LogParam(Convert.ToString(riskElement.AffectedBookId)),
                                        new LogParam(Convert.ToString(parentAttribute.IdNode))));
                                }
                            }

                            // 13. Accumulate :
                            //  add the built elements to the accumulator. 
                            //  we pass all the element evaulated as SumPosition, 
                            //  the SumDeposit are passed IFF the parent actor has not a book IMR on himself.

                            pAccumulator =
                                pAccumulator
                                .Union(
                                from riskElement
                                    in currentActorRiskElements
                                where riskElement.RiskElementEvaluation == RiskElementEvaluation.SumPosition
                                || (riskElement.RiskElementEvaluation == RiskElementEvaluation.SumDeposit && parentAttribute.IMRBookId <= 0)
                                select riskElement
                                );

                        }
                        else
                        // No MARGINREQOFFICE attributes can be found for the current parent     
                        {
                            string message = String.Format(Ressource.GetString("RiskPerformance_WARNINGNoMarginReqOfficeAttributesForTheParentNode"),
                                        pMarginReqOffice.Identifier, pMarginReqOffice.Id, parentAttribute.IdNode);

                            // FI 20200623 [XXXXX] SetErrorWarning
                            ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                            
                            Logger.Log(new LoggerData(LogLevelEnum.Error, message));
                        }
                    }

                    // 14. Repeat the process for each MARGINREQOFFICE parent....
                    foreach (ActorNodeWithSpecificRoles marginReqOfficeParent in marginReqOfficeParents)
                    {
                        AssignInheritedRiskMarginObjects(marginReqOfficeParent, pAccumulator);
                    }
                }
            }
        }

        private IEnumerable<ActorNode> GetHierarchyRoots()
        {
            IEnumerable<ActorNode> roots =
                from hierarchy in m_ActorRoleHierarchiesDescendingFromClearer
                select hierarchy.Root;

            roots = roots.Union(new[] { m_ActorRoleHierarchyDescendingFromEntity.Root });
            return roots;
        }

        private static void InheritanceElementsFlags(
            ActorNodeWithSpecificRoles[] marginReqOfficeAncestors, out bool getInheritedGrossElements, out bool getInheritedNetElements)
        {
            RoleMarginReqOfficeAttribute[] ancestorAttributes = (
                                from ancestor in marginReqOfficeAncestors
                                from attribute in ancestor.RoleSpecificAttributes
                                where attribute is RoleMarginReqOfficeAttribute
                                select (RoleMarginReqOfficeAttribute)attribute
                                ).ToArray();

            bool isGrossMargining = ancestorAttributes[0].IsGrossMargining;

            bool grossAndNetEvaluations = false;

            for (int idx = 0; idx < ancestorAttributes.Length; idx++)
            {
                grossAndNetEvaluations = isGrossMargining ^ ancestorAttributes[idx].IsGrossMargining;

                if (grossAndNetEvaluations)
                    break;
            }

            getInheritedGrossElements = isGrossMargining || grossAndNetEvaluations;

            getInheritedNetElements = !isGrossMargining || grossAndNetEvaluations;
        }

        /// <summary>
        /// Remove duplicate between the deposit of the given attributes and the inherited risk elements control collection
        /// </summary>
        /// <param name="pMarginReqOfficeAttributes">affected lists</param>
        /// <param name="pControlRiskElements">control list, 
        /// any element existant either in the control collection either in the the attributes ones 
        /// has to be deleted from the attributes collection</param>
        /// <param name="pLog">log delegate, if null no log evenemtns will be generated</param>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private static void RemoveDuplicateRiskElements(RoleMarginReqOfficeAttribute[] pMarginReqOfficeAttributes, IEnumerable<RiskElement> pControlRiskElements, SetErrorWarning pSetErrorWarning)
        {

            foreach (RoleMarginReqOfficeAttribute attribute in pMarginReqOfficeAttributes)
            {
                if (attribute.InheritedElements.Count > 0)
                {
                    List<RiskElement> notAdditionalDeposits = (
                        from deposit
                            in attribute.RootElements
                        where deposit.RiskElementClass == RiskElementClass.DEPOSIT
                        select deposit
                        ).ToList();

                    RoleMarginReqOfficeAttribute.RemoveDuplicateRiskElements(notAdditionalDeposits, pControlRiskElements,  pSetErrorWarning);
                }
            }
        }

        private static RoleMarginReqOfficeAttribute GetAttributeInRegardsWithParent(
            RoleMarginReqOfficeAttribute[] childAttributes, RoleMarginReqOfficeAttribute parentAttribute)
        {
            // Find the MARGINREQOFFICE attribute of the current actor, relative to the current MARGINREQOFFICE parent 
            RoleMarginReqOfficeAttribute attributeInRegardsWithParent =
                childAttributes.FirstOrDefault(attribute => attribute.IdParentNode == parentAttribute.IdNode);

            // When no attributes relative to the current MARGINREQOFFICE parent are found, 
            //  then we search any "Joker" attribute: IdParentNode in (0, IdNode).
            if (attributeInRegardsWithParent == null)
            {
                attributeInRegardsWithParent = childAttributes.FirstOrDefault(
                    attribute => attribute.IdParentNode == 0 || attribute.IdParentNode == attribute.IdNode);
            }

            return attributeInRegardsWithParent;
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        // PM 20201028 [25570][25542] Ajout code de retour
        //private void InitializeDeposits()
        private Cst.ErrLevel InitializeDeposits()
        {
            Cst.ErrLevel ret = ProcessState.CodeReturn;

            m_Timers.CreateTimer("INITIALIZEDEPOSITS");

            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1054), 1));
            Logger.Write();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            // initialize the deposits for each clearing house

            foreach (int idCss in m_MarketsCollectionFromEntity.GetCSSInternalIDs())
            {
                DateTime dtBusiness = m_MarketsCollectionFromEntity.GetBusinessDate(idCss);

                //PM 20150506 [20575] Ajout dtMarket
                DateTime dtMarket = m_MarketsCollectionFromEntity.GetMarketDate(idCss);

                // Lecture des entity/market de la chambre
                IEnumerable<EntityMarketWithCSS> entityMarkets = m_MarketsCollectionFromEntity.GetEntityMarkets(idCss);

                // PM 20160404 [22116] New : Construction du jeu de méthodes de calcul en fonction des assets en position
                // PM 20170313 [22833] Passer en paramètre un RiskRepository à la place du dictionnaire de SQL_AssetETD
                // PM 20180219 [23824] Utilisation de RiskPerformanceProcessInfo en paramètre
                //RiskMethodSet methodSet = m_MethodFactory.BuildMethodSet(this.Cs, idCss, dtBusiness, dtMarket,
                //    m_ProcessInfo.Entity,
                //    m_ProcessInfo.Timing,
                //    m_ProcessInfo.RiskDataTime,
                //    appInstance.SessionId,
                //    //m_PositionsRepository.AssetETDCache,
                //    m_EvaluationRepository,
                //    m_ImRequestDiagnostics,
                //    entityMarkets,
                //    m_ActorsRoleMarginReqOffice);
                RiskMethodSet methodSet = m_MethodFactory.BuildMethodSet(ProcessInfo, idCss, dtBusiness, dtMarket,
                    m_EvaluationRepository,
                    m_ImRequestDiagnostics,
                    entityMarkets,
                    m_ActorsRoleMarginReqOffice);
                
                // PM 20231019 [XXXXX] Ajout log de la liste des méthodes utilisées
                if (methodSet.Methods.Count > 0)
                {
                    string methodAllIdentifier = string.Join(", ", methodSet.Methods.Keys.Select(m => $"{m.Identifier} ({m.IMMethodEnum})"));
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1085), 2, new LogParam(methodAllIdentifier)));
                }

                IEnumerable<ActorNodeWithSpecificRoles> enabledActorsRoleMarginReqOffice =
                    from actor in this.m_ActorsRoleMarginReqOffice
                    from attribute in actor.RoleSpecificAttributes
                    where attribute is RoleMarginReqOfficeAttribute attr
                    && DataContractHelper.GetDataContractElementEnabled(attr, dtBusiness)
                    select actor;

                //IEnumerable<Deposit> notInPositionDeposit =
                //    method.InitializeDeposits(enabledActorsRoleMarginReqOffice, this.m_ProcessInfo.Timing);
                IEnumerable<Deposit> notInPositionDeposit = methodSet.InitializeDeposits(enabledActorsRoleMarginReqOffice, this.m_ProcessInfo.Timing);

                //method.LoadParameters(this.Cs, m_PositionsRepository.AssetETDCache);

                /////////////////////////////////////////////////////////////////////////////////////////////

                IEnumerable<ActorNodeWithSpecificRoles> disabledActorsRoleMarginReqOffice =
                    this.m_ActorsRoleMarginReqOffice.Except(enabledActorsRoleMarginReqOffice);

                if (disabledActorsRoleMarginReqOffice.Count() > 0)
                {

                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1009), 0,
                            new LogParam((from actor in disabledActorsRoleMarginReqOffice select actor.Identifier).Aggregate((total, next) => String.Format("{0}.{1}", total, next))),
                            new LogParam(dtBusiness)));
                }

                if (notInPositionDeposit.Count() > 0)
                {
                    IEnumerable<string> notInPositionDepositWithPrevResults =
                       from deposit in notInPositionDeposit
                       where deposit.PrevResult != null
                       select deposit.PrevResult.Identifier;

                    // FI 20200623 [XXXXX] SetErrorWarning
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 1010), 0,
                            new LogParam(notInPositionDepositWithPrevResults.Count() > 0 ?
                                notInPositionDepositWithPrevResults.Aggregate((total, next) => String.Format("{0}.{1}", total, next))
                                : Cst.NotAvailable)));
                }

            }
#if DEBUGDEV
            
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 3, new LogParam("LOADING RISK PARAMETERS"), new LogParam(m_Timers.GetElapsedTime("INITIALIZEDEPOSITS", null))));
#endif
            return ret;
        }

        // EG 20180525 [23979] IRQ Processing
        private void DepositsGen()
        {
            EvaluateDepositsThreading();

            if (Cst.ErrLevel.IRQ_EXECUTED != ProcessState.CodeReturn)
                CreateResultsThreading();
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void WriteCalculationSheets()
        {
            m_Timers.CreateTimer("WRITECALCULATIONSHEETS");

            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1056), 1));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            CalculationSheetWriter logWriter = new CalculationSheetWriter();
            
            //logWriter.Init(
            //    this.Cs, this.processLog.header,
            //    m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", null));
            logWriter.Init(this.Cs, IdProcess, m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", null));
            logWriter.InitDelegate( this.ProcessState.SetErrorWarning, ProcessState.AddCriticalException);
            logWriter.AppSession = this.Session;

            bool writingok = logWriter.Write(this.m_LogsRepository, out int count, out int idMarginTrack);

            /////////////////////////////////////////////////////////////////////////////////////////////

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1059), 2,
                new LogParam(Convert.ToString(count)),
                new LogParam(Convert.ToString(writingok))));
            //
#if DEBUGDEV
            Logger.Log(new LoggerData(LogLevelEnum.Trace, new SysMsgCode(SysCodeEnum.LOG, 300), 2, new LogParam("ADDING CALCULATION AUDIT TO THE DATABASE"), new LogParam(m_Timers.GetElapsedTime("WRITECALCULATIONSHEETS", null))));
#endif

            /////////////////////////////////////////////////////////////////////////////////////////////

            if (!writingok)
            {
                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
            }
        }

        private void IMRequestHeaderPrepareElements()
        {
            this.m_ImRequestDiagnostics.PrepareElements
                    (this.m_ProcessInfo.CssId, this.m_ProcessInfo.DtBusiness, null, true);
        }

        private void IMRequestHeaderValidation(ProcessStateTools.StatusEnum pStatus)
        {
            // TODO 20110614 waiting for a parameter containing a css group
            //this.m_ImRequestDiagnostics.ValidateElements(this.m_ProcessInfo.CssId, this.m_ProcessInfo.DtBusiness, pStatus, true);
            // EG 20130529 si le statut est en ERROR alors IMREQUEST doit être marqué en ERROR
            ProcessState.SetProcessState(pStatus);
            this.m_ImRequestDiagnostics.ValidateElements(this.m_ProcessInfo.CssId, this.m_ProcessInfo.DtBusiness, ProcessState.Status, true);
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        private void IMRequestValidation()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1057), 1));
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (int cssId in m_MarketsCollectionFromEntity.GetCSSInternalIDs())
            {
                this.m_ImRequestDiagnostics.ValidateElements
                    (cssId, m_MarketsCollectionFromEntity.GetBusinessDate(cssId), ProcessStateTools.StatusSuccessEnum, false);
            }

            /////////////////////////////////////////////////////////////////////////////////////////////

        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void SendMessageQueueToEvent()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1058), 1));
            //
            foreach (MarginDetailsDocument doc in m_LogsRepository.CalculationSheets.Values)
            {
                int idT = doc.IdTechTrade;
                Boolean isDelEvent = false; // FI 20140415 DelEvent déjà effectuer dans ChekAndRecord
                EventsGenMQueue eventsGenMQueue = CaptureTools.GetMQueueForEventProcess(Cs, idT, isDelEvent, MQueue.header.requester);
                if (null != eventsGenMQueue)
                {
                    MQueueSendInfo sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.EVENTSGEN,AppInstance);
                    if (false == sendInfo.IsInfoValid)
                        throw new Exception("MOM information unavailable for Spheres® EventsGen service");
                    MQueueTools.Send(eventsGenMQueue, sendInfo);
                }
            }
        }

        // EG 20180525 [23979] IRQ Processing
        private void DefineIMRequestHeaderGlobalStatus()
        {

            if (this.m_ProcessInfo.Mode == RiskEvaluationMode.Normal)
            {
                // PM 20160404 [22116] Passage de Methods à MethodSet
                IEnumerable<Deposit> deposits =
                    from methodSet in m_MethodFactory.MethodSet
                    from deposit in methodSet.Value.Deposits
                    where deposit.Value.NotInPosition == false
                    select deposit.Value;

                ProcessStateTools.StatusEnum globalStatus = ProcessStateTools.StatusEnum.SUCCESS;

                if (ProcessStateTools.IsCodeReturnIRQExecuted(ProcessState.CodeReturn))
                    globalStatus = ProcessStateTools.StatusInterruptEnum;
                else if (false == ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                    globalStatus = ProcessStateTools.StatusEnum.ERROR;
                else if (deposits.Count() == 0)
                    globalStatus = ProcessStateTools.StatusEnum.NA;

                IMRequestHeaderValidation(globalStatus);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void EventsGen()
        {
            
            Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1058), 1));
            Logger.Write();
            //
            /////////////////////////////////////////////////////////////////////////////////////////////

            foreach (MarginDetailsDocument doc in m_LogsRepository.CalculationSheets.Values)
            {
                string tradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(Cs, doc.IdTechTrade);

                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = Cs,
                    id = doc.IdTechTrade,
                    identifier = tradeIdentifier,
                    idInfo = new IdInfo()
                    {
                        id = doc.IdTechTrade,
                        idInfos = new DictionaryEntry[]{
                                                    new DictionaryEntry("ident", "TRADE"),
                                                    new DictionaryEntry("identifier", tradeIdentifier),
                                                    new DictionaryEntry("GPRODUCT", Cst.ProductGProduct_RISK.ToString()  )}
                    },
                    requester = MQueue.header.requester
                };
                
                Logger.Log(new LoggerData(LogLevelEnum.Debug, new SysMsgCode(SysCodeEnum.LOG, 500), 2, new LogParam(LogTools.IdentifierAndId(tradeIdentifier, mQueueAttributes.idInfo.id))));
                
                _ = New_EventsGenAPI.ExecuteSlaveCall(new EventsGenMQueue(mQueueAttributes), null, this, false);
                
                Logger.Write();
            }

            /////////////////////////////////////////////////////////////////////////////////////////////


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <returns></returns>
        /// FI 20160613 [22256] Add
        /// PM 20170313 [22833] Ajout vérification que pSource n'est pas null
        private static UnderlyingStock ConvertToUnderlyingStock(StockCoverageDetailCommunicationObject pSource)
        {
            UnderlyingStock ret;
            if (pSource != default(StockCoverageDetailCommunicationObject))
            {
                FpML.v44.Assetdef.EquityAsset equityAsset = new FpML.v44.Assetdef.EquityAsset
                {
                    OTCmlId = pSource.AssetId,
                    description = new EFS.GUI.Interface.EFS_String(pSource.AssetIdentifier),
                    descriptionSpecified = true,
                    instrumentId = new FpML.v44.Shared.InstrumentId[1]
                    {
                        new FpML.v44.Shared.InstrumentId()
                        {
                            Value = pSource.AssetIdentifier,
                            instrumentIdScheme = Cst.OTCml_AssetIdScheme
                        }
                    }
                };
                /*
                EfsML.v30.Doc.ActorId actorId = new EfsML.v30.Doc.ActorId();
                actorId.actorNameSpecified = false;
                actorId.actorIdScheme = null; // pas de scheme (cet attribut est non obligatoire) => cela allège la flux et facilite la lecture du flux XML
                actorId.OTCmlId = pSource.ActorId;
                actorId.Value = pSource.ActorIdentifier;*/

                EfsML.v30.Doc.BookId bookId = new EfsML.v30.Doc.BookId
                {
                    bookNameSpecified = false,
                    bookIdScheme = null, // pas de scheme (cet attribut est non obligatoire) => cela allège la flux et facilite la lecture du flux XML
                    OTCmlId = pSource.BookId,
                    Value = pSource.BookIdentifier
                };

                //UnderlyingStock ret = new UnderlyingStock()
                ret = new UnderlyingStock()
                {
                    otcmlId = pSource.PosId.ToString(),
                    /*actorId = actorId, */
                    bookId = bookId,
                    equity = equityAsset,
                    qtyAvailable = pSource.QtyAvailable,
                    qtyUsedFut = pSource.QtyUsedFut,
                    qtyUsedOpt = pSource.QtyUsedOpt
                };
            }
            else
            {
                ret = default;
            }
            return ret;
        }

        /// <summary>
        /// Mise à jour de Id de méthode de calcul de déposit sur les référentiels CSS, MARKET, DERIVATIVECONTRACT, COMMODITYCONTRACT
        /// </summary>
        /// PM 20220930 [XXXXX] Nouvelle méthode
        private void UpdateReferentialWithIdMethod()
        {
            string sqlUpdateCssMethFut = @"
                  update CSS
                     set IDIMMETHOD =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = CSS.IDIMMETHOD)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = CSS.IDIMMETHOD)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateCssMethEtdOpt = @"
                  update CSS
                     set IDIMMETHODETDOPT =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = CSS.IDIMMETHODETDOPT)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = CSS.IDIMMETHODETDOPT)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateCssMethComSpot = @"
                  update CSS
                     set IDIMMETHODCOMSPOT =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = CSS.IDIMMETHODCOMSPOT)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHODCOMSPOT is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = CSS.IDIMMETHODCOMSPOT)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateMarketMethFut = @"
                  update MARKET
                     set IDIMMETHOD =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = MARKET.IDIMMETHOD)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = MARKET.IDIMMETHOD)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateMarketMethEtdOpt = @"
                  update MARKET
                     set IDIMMETHODETDOPT =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = MARKET.IDIMMETHODETDOPT)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = MARKET.IDIMMETHODETDOPT)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateMarketMethComSpot = @"
                  update MARKET
                     set IDIMMETHODCOMSPOT =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = MARKET.IDIMMETHODCOMSPOT)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHODCOMSPOT is not null)
                     and (IDA = @IDA_CSS)
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = MARKET.IDIMMETHODCOMSPOT)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateDrvContract = @"
                  update DERIVATIVECONTRACT
                     set IDIMMETHOD =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = DERIVATIVECONTRACT.IDIMMETHOD)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDM in (select IDM from MARKET where (IDA = @IDA_CSS)))
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = DERIVATIVECONTRACT.IDIMMETHOD)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            string sqlUpdateComContract = @"
                  update COMMODITYCONTRACT
                     set IDIMMETHOD =
                       (
                         select meth_e.IDIMMETHOD
                           from IMMETHOD meth_d
                          inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                          where (meth_d.IDIMMETHOD = COMMODITYCONTRACT.IDIMMETHOD)
                            and (meth_d.DTDISABLED is not null)
                            and (meth_d.DTDISABLED <= @DTBUSINESS)
                            and (meth_e.DTENABLED <= @DTBUSINESS)
                            and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                       )
                   where (IDIMMETHOD is not null)
                     and (IDM in (select IDM from MARKET where (IDA = @IDA_CSS)))
                     and exists ( select 1
                                   from IMMETHOD meth_d
                                  inner join IMMETHOD meth_e on (meth_e.IDENTIFIER = meth_d.IDENTIFIER)
                                  where (meth_d.IDIMMETHOD = COMMODITYCONTRACT.IDIMMETHOD)
                                    and (meth_d.DTDISABLED is not null)
                                    and (meth_d.DTDISABLED <= @DTBUSINESS)
                                    and (meth_e.DTENABLED <= @DTBUSINESS)
                                    and ((meth_e.DTDISABLED is null) or (meth_e.DTDISABLED > @DTBUSINESS))
                                  group by meth_e.IDENTIFIER having (count(*) = 1)
	                            )";

            DataParameters dataParameters = new DataParameters();
            QueryParameters qryUpdate;

            foreach ( var css in  this.m_MarketsCollectionFromEntity.BusinessDateByCss)
            {
                dataParameters.Add(DataParameter.GetParameter(this.Cs, DataParameter.ParameterEnum.DTBUSINESS), css.Value);
                dataParameters.Add(DataParameter.GetParameter(this.Cs, DataParameter.ParameterEnum.IDA_CSS), css.Key);

                // Mise à jour CSS future
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateCssMethFut, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour CSS option
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateCssMethEtdOpt, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour CSS commodity spot
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateCssMethComSpot, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour MARKET future
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateMarketMethFut, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour MARKET option
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateMarketMethEtdOpt, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour MARKET commodity spot
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateMarketMethComSpot, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour DERIVATIVECONTRACT
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateDrvContract, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());

                // Mise à jour COMMODITYCONTRACT
                qryUpdate = new QueryParameters(this.Cs, sqlUpdateComContract, dataParameters);
                DataHelper.ExecuteNonQuery(this.Cs, CommandType.Text, qryUpdate.Query, qryUpdate.Parameters.GetArrayDbParameter());
            }
        }
    }
}
