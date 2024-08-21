using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Process;
using EFS.Process.EventsGen;
using EFS.SpheresRiskPerformance;
using EFS.SpheresRiskPerformance.CalculationSheet;
using EFS.SpheresRiskPerformance.Properties;
using EFS.TradeInformation;
using EFS.Tuning;
//
using EfsML.Business;
using EfsML.Enum;
using EfsML.v30.CashBalance;
using EfsML.v30.Fix;
//
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CashBalance
{

    /// <summary>
    /// The main process executing the cash balance evaluation for the given entity
    /// </summary>
    public partial class CashBalanceProcess : RiskPerformanceProcessBase
    {
        #region members
        private bool m_IsCreateMaterializedView;
        private bool m_IsDataReaderMode;    //DataReader vs DataSet
        private bool m_IsDataValueMode;     //Data values vs DataParameters
        /// <summary>
        /// 
        /// </summary>
        private CBRequestDiagnostics2 m_CBRequest;

        /// <summary>
        /// La hiérarchie des acteurs (CBO et tous les descendants), avec des flux pour la journée de bourse concernée
        /// </summary>
        private CBHierarchy m_CBHierarchy;

        /// <summary>
        ///  Etats marchés/cssCustodian pour l'ensemble des CBO
        ///  <para>Seuls sont énumérés les marchés/cssCustodian en rapport avec la liste des marchés dits "en activité"(table CBOMARKET)</para>
        ///  <para>La clé du dictionnaire est constitué du couple {IDA_CBO, IDB_CBO}</para>
        /// </summary>
        /// FI 20161027 [22151] 
        public Dictionary<Pair<int, int>, EndOfDayStatus> cboEODstatus = new Dictionary<Pair<int, int>, EndOfDayStatus>(new PairComparer<Int32, Int32>());

        /// <summary>
        /// Nombre total de calculs d''App./Rest. de Déposit Soldes
        /// </summary>
        private int m_Trade_CountTotal;
        /// <summary>
        /// Nombre de calculs d''App./Rest. de Déposit Soldes non effectués
        /// </summary>
        private int m_Trade_CountNotProcessed;
        /// <summary>
        /// Nombre de calculs d''App./Rest. de Déposit Soldes effectués avec succès
        /// </summary>
        private int m_Trade_CountProcessedSuccess;
        /// <summary>
        /// Nombre de calculs d''App./Rest. de Déposit Soldes effectués avec erreurs
        /// </summary>
        private int m_Trade_CountProcessedError;
        /// <summary>
        /// Nombre de calculs d''App./Rest. de Déposit Soldes effectués avec succès mais identique à un précédent calcul
        /// </summary>
        // PM 20190701 [24761] Ajout m_Trade_CountProcessedUnchanged
        private int m_Trade_CountProcessedUnchanged;
        /// <summary>
        /// Gestion des données en cache
        /// </summary>
        private readonly CBCache m_CBCache;
        // EG 20180409 [23769] New
        private readonly static object m_CBCacheLock = new object();

        /// <summary>
        /// Pour la deserialisation des CB précédants
        /// </summary>
        // PM 20190701 [24761] New
        protected EFS_TradeLibrary m_TradeLibrary;
        #endregion
        #region constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pService"></param>
        public CashBalanceProcess(RiskPerformanceMQueue pQueue, AppInstanceService pService)
            : base(pQueue, pService)
        {
            // EG 20131216 [19342] Change To External treatment
            bool isExternal = pQueue.GetBoolValueParameterById("ISEXTERNALGENEVENTS");
            if (isExternal)
                m_EventGenMode = EventGenModeEnum.External;

            // EG 20131216 [19342] Change To External treatment (UNUSED)
            //m_IsCashFlowsLinked = pQueue.GetBoolValueParameterById("ISCASHFLOWSLINKED");
            m_IsCashFlowsLinked = true;

            // PM 20140910 [20066][20185] Add m_CBCache
            m_CBCache = new CBCache();
        }
        #endregion
        #region Methods
        // EG 20180413 [23769] Gestion customParallelConfigSource
        // EG 20181127 PERF Post RC (Step 3) - LOG
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override void SetCurrentParallelSettings()
        {
            CurrentParallelSettings = null;
            if (ParallelTools.GetParallelSection("parallelCashBalance") is ParallelCashBalanceSection parallelSection)
                CurrentParallelSettings = parallelSection.GetParallelSettings(base.MQueue.identifier);

            if (null != CurrentParallelSettings)
            {
                bool isParallelLoadFlows = IsParallelProcess(ParallelProcess.LoadFlows);
                bool isParallelCashBalance = IsParallelProcess(ParallelProcess.CashBalance);
                bool isSlaveCallEvents = IsSlaveCallEvents(ParallelProcess.CashBalance);
                string _eventGenMode = m_EventGenMode.ToString();
                if ((EventGenModeEnum.Internal == m_EventGenMode) && isSlaveCallEvents)
                    _eventGenMode += " (SlaveCall)";
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4055), 0,
                    new LogParam((isParallelLoadFlows ? "YES" : "NO")),
                    new LogParam((isParallelCashBalance ? "YES" : "NO")),
                    new LogParam((isParallelCashBalance ? Convert.ToString(GetHeapSize(ParallelProcess.CashBalance)) : "-")),
                    new LogParam((isParallelCashBalance ? Convert.ToString(GetMaxThreshold(ParallelProcess.CashBalance)) : "-")),
                    new LogParam(_eventGenMode),
                    new LogParam((isParallelCashBalance ? Convert.ToString(GetHeapSizeEvents(ParallelProcess.CashBalance)) : "-")),
                    new LogParam((isParallelCashBalance ? Convert.ToString(GetMaxThresholdEvents(ParallelProcess.CashBalance)) : "-"))));
            }
        }
        /// <summary>
        /// 
        /// </summary>
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

                    ProcessState.CodeReturn = InitializeProcessInfo();
                    //CB_Process
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4000), 0,
                        new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness))));

                    SerializationHelper.SerializationDirectory = this.Session.GetTemporaryDirectory(Common.AppSession.AddFolderSessionId.True);
                }
            }
            catch (Exception ex)
            {
                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;

                //CB_Process_Err
                SpheresException2 spheresEx = SpheresExceptionParser.GetSpheresException(ex.GetType().ToString(), ex);

                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(spheresEx); 
                
                Logger.Log(new LoggerData(spheresEx));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4060), 1,
                    new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness))));
            }
        }

        /// <summary>
        ///  Traitement du CashBalance 
        ///  <para>Le traitement est exécuté si les contrôles avant Lancement sont Ok</para>
        /// </summary>
        /// <returns></returns>
        // EG 20180413 [23769] Gestion customParallelConfigSource
        // EG 20180525 [23979] IRQ Processing
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            SetCurrentParallelSettings();

            Cst.ErrLevel retIRQ = ProcessState.CodeReturn;
            if (IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref retIRQ))
                ProcessState.CodeReturn = retIRQ;

            if (Cst.ErrLevel.IRQ_EXECUTED != ProcessState.CodeReturn)
            {
                ProcessState.CodeReturn = CashBalanceControlEOD();
                // Traitement de Calcul des Soldes
                if (ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS)
                    CashBalanceCalculation();
            }
            return ProcessState.CodeReturn;
        }

        /// <summary>
        /// Exécution du traitement de CashBalance 
        /// </summary>
        /// FI 20141126 [20526] Add Method 
        /// FI 20161027 [22151] Modify
        /// FI 20170421 [XXXXX] Modify
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private void CashBalanceCalculation()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            m_Timers.CreateTimer("PROCESSEXECUTESPECIFIC");

            try
            {
                //PL 20160524 
                m_IsDataReaderMode = EFS.SpheresRiskPerformance.Properties.Settings.Default.DataReaderMode;
                if (m_IsDataReaderMode)
                    m_IsCreateMaterializedView = EFS.SpheresRiskPerformance.Properties.Settings.Default.CreateMaterializedView;
                m_IsDataValueMode = EFS.SpheresRiskPerformance.Properties.Settings.Default.DataValueMode;

                //    ----------------------------
                // 1. Initialisation du traitement 
                //    ----------------------------
                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4016), 1));
                Logger.Write();

                m_CBRequest = new CBRequestDiagnostics2(Cs, Session.IdA, m_ProcessInfo.Entity, base.MQueue.identifier,
                    m_ProcessInfo.DtBusiness, m_ProcessInfo.Timing, m_ProcessInfo.IdPr);

                // - Préparer la table CBREQUEST
                m_CBRequest.StartCBRequest(Cs, null);
                

                if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn))
                {

                    //Chargement Hierarchy
                    m_CBHierarchy = new CBHierarchy(m_CBRequest.Ida_Entity, m_CBRequest.Identifier_Entity, m_CBRequest.DtBusiness, DateTime.MinValue, m_CBRequest.Timing);
                    m_CBHierarchy.InitDelegate(this.ProcessState.SetErrorWarning);

                    if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                        (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                    {
                        //    -------------------------------------------------------------------
                        // 2. Contrôle avant Calcul des Soldes
                        //    -------------------------------------------------------------------
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4017), 1));
                        Logger.Write();

                        codeReturn = CashBalanceCalculationControl(this.Cs);
                    }

                    if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                        (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                    {
                        //    ------------------------------------
                        // 3. Construire la hiérarchie des Acteurs
                        //    ------------------------------------
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4018), 1));
                        Logger.Write();

                        m_CBHierarchy.Dispose();

                        codeReturn = m_CBHierarchy.BuildBrutHierarchy(Cs, this);
                    }

                    if ((null != m_CBHierarchy) && (this.LogDetailEnum >= LogLevelDetail.LEVEL3))
                    {
                        SerializationHelper.DumpObjectToFile<CBHierarchy>((CBHierarchy)m_CBHierarchy, new SysMsgCode(SysCodeEnum.LOG, 1060),
                            this.AppInstance.AppRootFolder, "CB_HierarchyBrut.xml", null, this.ProcessState.AddCriticalException);
                    }
                }

                if (codeReturn != Cst.ErrLevel.SUCCESS)
                {
                    base.ProcessState.CodeReturn = codeReturn;
                    //
                    // Arrêter le traitement s'il y a un problème dans la construction de la hiérarchie:
                    // - Paramétrage absent
                    // - Paramétrage incohérent
                    // - ...
                    return;
                }

                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                {
                    //    ---------------------------------------------
                    // 4. Charger tous les flux sur les acteurs CBO/MRO
                    //    ---------------------------------------------
                    // RD 20200702 Add Log
                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4019), 1));
                    Logger.Write();

                    // PM 20180115 [CHEETAH] Lecture des flux en asynchrone
                    LoadFinancialFlowsAndFilterMroCbo();

                    if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                        (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                    {
                        //    ----------------------------------
                        // 5. Génération des Trades Cash-Balance
                        //    ----------------------------------
                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 4020), 1));
                        Logger.Write();

                        // - Génération des couples (CBO/MRO,Book)
                        List<CBPartyTradeInfo> partyTradeInfos = GenerateActorBookCouple();
                        // - Mettre à jour la table CBREQUEST pour les couples (CBO/MRO,Book)
                        m_CBRequest.UpdateCBRequestPartyTradeInfo(Cs, null, partyTradeInfos);
                        // - Génération des Trades Cash-Balance pour les couples (CBO/MRO,Book)
                        // EG 20180205 [23769] New
                        codeReturn = CashBalanceThreading(partyTradeInfos);
                    }
                }

                if (codeReturn != Cst.ErrLevel.SUCCESS)
                    ProcessState.CodeReturn = codeReturn;
            }
            catch (Exception ex)
            {
                // FI 20220803 [XXXXX] Trace déjà alimentée par le logger
                if (false == LoggerManager.IsEnabled)
                {
                    // FI 20200918 [XXXXX] Ecriture dans la trace du message Complet (avec pile des appels)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                }

                ProcessState.CodeReturn = Cst.ErrLevel.FAILURE;
               
                // FI 20200623 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex);

                

                
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
            }
            finally
            {
                // - Purger les tables de travail
                if (null != m_CBHierarchy)
                    m_CBHierarchy.Dispose();

                if (ProcessState.CodeReturn == Cst.ErrLevel.IRQ_EXECUTED)
                {
                    m_CBRequest.EndCBRequest(Cs, null, ProcessStateTools.StatusInterruptEnum);
                }
                else
                {
                    if ((ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS) || (ProcessState.CodeReturn == Cst.ErrLevel.NOTHINGTODO))
                    {
                        m_CBRequest.EndCBRequest(Cs, null);
                    }
                    else // Erreur
                    {
                        m_CBRequest.EndCBRequest(Cs, null, ProcessStateTools.StatusErrorEnum);

                        Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4060), 1,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness))));
                    }

                    if (m_Trade_CountTotal == 0)
                    {
                        //No Process
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4098), 1,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                            new LogParam(m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", "hms"))));
                    }
                    else if (m_Trade_CountTotal == m_Trade_CountProcessedSuccess)
                    {
                        // All Success
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4097), 1,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                            new LogParam(m_Trade_CountProcessedSuccess),
                            new LogParam(m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", "hms"))));
                    }
                    else if ((m_Trade_CountNotProcessed == 0) && (m_Trade_CountProcessedUnchanged == 0))
                    {
                        // All Processed
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4096), 1,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                            new LogParam(m_Trade_CountTotal),
                            new LogParam(m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", "hms")),
                            new LogParam(m_Trade_CountProcessedError),
                            new LogParam(m_Trade_CountProcessedSuccess)));
                    }
                    else
                    {
                        //Not All Processed
                        Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4099), 1,
                            new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId)),
                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                            new LogParam(m_Trade_CountTotal.ToString()),
                            new LogParam(m_Timers.GetElapsedTime("PROCESSEXECUTESPECIFIC", "hms")),
                            new LogParam(m_Trade_CountProcessedError),
                            new LogParam(m_Trade_CountProcessedSuccess),
                            new LogParam(m_Trade_CountProcessedUnchanged),
                            new LogParam(m_Trade_CountNotProcessed)));
                    }
                }
            }
        }

        /// <summary>
        /// Vérifications internes au traitement de CashBalance
        /// <para>- Vérifier si existence d'activité (ENTITYMARKET reseigné)</para>
        /// <para>- Vérifier si la date de compensation sélectionnée est valide</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20120429 [XXXXX] Modification => Contrôles que les traitements EOD sont terminées en SUCCES
        /// FI 20120510 [XXXXX] la méthode retourne Cst.ErrLevel
        /// FI 20161021 [22152] Modify
        /// FI 20170207 [22151] Modify
        /// FI 20170329 [23022] Modify
        /// FI 20170331 [23031] Modify
        /// FI 20170622 [23031] Modify
        // EG 20180525 [23979] IRQ Processing
        // EG 20190114 Add detail to ProcessLog Refactoring
        private Cst.ErrLevel CashBalanceCalculationControl(string pCS)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            /////////////////////////////////////////////////////////////////////////////////////////////
            // PRINCIPE: 
            // --------
            // ON CONSIDERE QUE TOUS LES MARCHES D'UNE MEME CHAMBRE DE COMPENSATION ONT DES DATES ALIGNEES:
            // - ILS ONT LES MEMES DATES DANS ENTITYMARKET: DTMARKETPREV, DTMARKET ET DTMARKETNEXT
            // - LES MEMES JOURS FERIES BUSINESS
            /////////////////////////////////////////////////////////////////////////////////////////////

            //FI 20120429 [] call method LoadEntityMarket
            DataRow[] drEntityMarket = MarketTools.LoadEntityMarketActivity(pCS, m_CBHierarchy.Ida_Entity, null);

            /////////////////////////////////////////////////////////////////////////////////////////////
            // Vérifier si existence d'activité (ENTITYMARKET reseigné)
            /////////////////////////////////////////////////////////////////////////////////////////////
            if (drEntityMarket.Length == 0)
            {
                //SYS-04020
                //<b>Calcul des App./Rest. de Déposit & Soldes non effectué. </b>  
                //<b>Cause:</b> Il n'existe aucun Dépouillement/Allocation pour l'Entité sélectionnée.  
                //<b>Détails:</b>  - Entité: <b>{1}</b>  - Date de compensation: <b>{2}</b>
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04020",
                    new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATANOTFOUND),
                        m_CBHierarchy.Identifier_Entity, DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness));
            }
            /////////////////////////////////////////////////////////////////////////////////////////////


            /////////////////////////////////////////////////////////////////////////////////////////////
            // - Vérifier si la date de compensation sélectionnée est valide
            /////////////////////////////////////////////////////////////////////////////////////////////
            // PM 20150520 [20575] Gestion DtEntity
            //IEnumerable<DataRow> cssWithDtMarketEqualDtBusiness =
            //    (from rowEntityMarket in drEntityMarket.ToList()
            //     where (Convert.ToDateTime(rowEntityMarket["DTMARKET"]) == m_CBHierarchy.DtBusiness)
            //     select rowEntityMarket);

            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
            {

                IEnumerable<DataRow> cssWithDtMarketEqualDtBusiness =
                    (from rowEntityMarket in drEntityMarket.ToList()
                     where (Convert.ToDateTime(rowEntityMarket["DTENTITY"]) == m_CBHierarchy.DtBusiness)
                     select rowEntityMarket);
                if (cssWithDtMarketEqualDtBusiness.Count() == 0)
                {
                    // FI 20170331 [23031]  
                    // Il est désormais possible de lancer un traitement de cashBalance en date DTENTITYPREV s'il n'existe pas en DTENTITY des CashBalances
                    Boolean isOk = false;
                    IEnumerable<DataRow> cssWithDtMarketEqualDtPrev =
                                (from rowEntityMarket in drEntityMarket.ToList()
                                 where (Convert.ToDateTime(rowEntityMarket["DTENTITYPREV"]) == m_CBHierarchy.DtBusiness)
                                 select rowEntityMarket);

                    if ((cssWithDtMarketEqualDtPrev.Count() > 0))
                    {
                        DateTime[] dtEntity = (from item in cssWithDtMarketEqualDtPrev
                                               select Convert.ToDateTime(item["DTENTITY"])).Distinct().ToArray();
                        isOk = (false == TradeRDBMSTools.IsExistCashBalance(pCS, dtEntity));
                    }

                    if (false == isOk)
                    {
                        //SYS-04030
                        //Calcul des App./Rest. de Déposit & Soldes non effectué.  
                        //<b>Cause:</b> Il n'existe aucune donnée à traiter à la date sélectionnée.   
                        //<b>Action:</b> Au besoin, veuillez saisir une autre date et relancer le traitement des App./Rest. de Déposit & Soldes.  
                        //<b>Détails:</b>  - Entité: <b>{1}</b>  - Date de compensation: <b>{2}</b>

                        // La date de compensation sélectionnée n'est pas à traiter, 
                        // car elle ne correspond à aucune date de compensation, sur au moins une chambre de compensation
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04030",
                            new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATAREJECTED),
                                m_CBHierarchy.Identifier_Entity, DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness));
                    }
                }
            }
            /////////////////////////////////////////////////////////////////////////////////////////////

            /////////////////////////////////////////////////////////////////////////////////////////////
            // - Charger la date de compensation précédente par rapport à la date de compensation sélectionnée
            //   elle correspond à la date de compensation précédente max, sur tous les marchés avec DTMARKET = la date de compensation sélectionnée
            /////////////////////////////////////////////////////////////////////////////////////////////
            // PM 20150507 [20575] Gestion DtEntity
            //m_CBHierarchy.DtBusinessPrev =
            //    cssWithDtMarketEqualDtBusiness.Max(rowEntityMarket => Convert.ToDateTime(rowEntityMarket["DTMARKETPREV"]));
            // PM 20170106 DtBusinessPrev doit être inférieure à DtBusiness
            //m_CBHierarchy.DtBusinessPrev = drEntityMarket.Max(x => Convert.ToDateTime(x["DTENTITYPREV"]));

            // FI 20170622 [23031] Lecture de ENTITYMARKETTRAIL la précédente dateBusiness 
            // si précédente dateBusiness est présente dans ENTITYMARKET (cas où plusieurs CSS seraient totalement non synchro) alors lecture de ENTITYMARKET par soucis d'économie de requête SQL
            //m_CBHierarchy.DtBusinessPrev = drEntityMarket.Select(x => Convert.ToDateTime(x["DTENTITYPREV"])).Where(dt => dt < m_CBHierarchy.DtBusiness).Max();

            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
            {
                IEnumerable<DateTime> dtPrevLst = drEntityMarket.Select(x => Convert.ToDateTime(x["DTENTITYPREV"])).Where(dt => dt < m_CBHierarchy.DtBusiness);
                if (dtPrevLst.Count() > 0)
                {
                    m_CBHierarchy.DtBusinessPrev = dtPrevLst.Max();
                }
                else
                {
                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTBUSINESS), m_CBHierarchy.DtBusiness);
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDA), m_CBHierarchy.Ida_Entity);

                    string queryDtPrev = @"
select MAX(DTENTITYCLOSED) 
from dbo.ENTITYMARKETTRAIL  emt
inner join dbo.ENTITYMARKET em on em.IDEM=emt.IDEM and em.IDA=@IDA 
where DTENTITYOPEN=@DTBUSINESS";

                    QueryParameters queryParameters = new QueryParameters(Cs, queryDtPrev, dp);

                    object dtPrev = DataHelper.ExecuteScalar(Cs, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                    if (null != dtPrev)
                    {
                        m_CBHierarchy.DtBusinessPrev = Convert.ToDateTime(dtPrev);
                    }
                    else
                    {
                        throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, 2, "SYS-04033",
                            new ProcessState(ProcessStateTools.StatusErrorEnum, Cst.ErrLevel.DATAREJECTED),
                                m_CBHierarchy.Identifier_Entity, DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness));
                    }
                }
            }

            // - Vérifier s'il existe des marché dont la date de compensation courante < la date de compensation sélectionnée
            // PM 20150507 [20575] Gestion DtEntity
            //IEnumerable<DataRow> cssWithDtMarketLowerDtBusiness =
            //    (from rowEntityMarket in drEntityMarket.ToList()
            //     where (Convert.ToDateTime(rowEntityMarket["DTMARKET"]) < m_CBHierarchy.DtBusiness)
            //     select rowEntityMarket);

            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
            {

                IEnumerable<DataRow> cssWithDtMarketLowerDtBusiness =
                    (from rowEntityMarket in drEntityMarket.ToList()
                     where (Convert.ToDateTime(rowEntityMarket["DTENTITY"]) < m_CBHierarchy.DtBusiness)
                     select rowEntityMarket);
                //
                if (cssWithDtMarketLowerDtBusiness.Count() > 0)
                {
                    //LOG-04001
                    //<b>Attention</b>, une partie de l'activité de l'entité porte sur des marchés où la date de compensation est inférieure au <b>{1}</b>.   
                    //L'activité portant sur ces marchés sera donc ignorée dans le présent traitement. 
                    //Lorsque l'activité sur ces marchés aura été traitée en date du {1}, un nouveau traitement devra impérativement être lancé à cette date afin de prendre en considération la totalité de l'activité de l'entité !

                    // Il existe des marchés sur d'autres chambres de compensation, 
                    // avec date de compensation (DTMARKET) < La date de compensation sélectionnée
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4001), 2,
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness))));
                }
            }

            // FI 20161021 [22152] Détermination de l'état des traitements de fin de journée par css/chambre
            // FI 20170329 [23022] variable drEntityMarketBusiness n'est plus utilisée 
            // Lecture de toutes les lignes présentes dans ENTITYMARKET => Il ne faut exlure les chambres pour lesquelles il y a eu un chgt de journée 
            //DataRow[] drEntityMarketBusiness = (drEntityMarket.Where(x => Convert.ToDateTime(x["DTENTITY"]) == m_CBHierarchy.DtBusiness)).ToArray();

            if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
            {
                List<SpheresIdentification> cssCustodian = new List<SpheresIdentification>();
                // FI 20170329 [23022] Lecture de toutes les lignes présentes dans ENTITYMARKET => Il ne faut exlure les chambres pour lesquelles il y a eu un chgt de journée 
                //Hashtable ht = GetDistinctCSSCustodian(drEntityMarketBusiness);
                Hashtable ht = GetDistinctCSSCustodian(drEntityMarket);
                foreach (int item in ht.Keys)
                    cssCustodian.Add(new SpheresIdentification(item, ht[item].ToString()));

                //Chargement des status EOD en date de traitement 
                List<Pair<SpheresIdentification, ProcessStatus>> eodStatus = LoadStatusEOD(cssCustodian, false);


                // CC/PM [RATP] Ajout vérification existence d'éléments dans eodStatus
                // FI 20170207 [22151] Modify eodStatus ne peut jamais être null (suppression du test (null != eodStatus))
                //if (null != eodStatus)
                //{
                m_CBHierarchy.cssCustodianEODUnValid = eodStatus.Where(item => ((null == item.Second) ||
                                                              (item.Second.status != ProcessStateTools.StatusEnum.SUCCESS &&
                                                               item.Second.status != ProcessStateTools.StatusEnum.WARNING))).Select(x => x.First).ToList();

                // CC/PM [RATP] Ne pas considérer les status à null
                //m_CBHierarchy.cssCustodianEODValid = eodStatus.Where(item => (
                //                    (item.Second.status == ProcessStateTools.StatusEnum.SUCCESS) ||
                //                    (item.Second.status == ProcessStateTools.StatusEnum.WARNING))).Select(x => x.First).ToList();
                m_CBHierarchy.cssCustodianEODValid = eodStatus.Where(item => ((null != item.Second) &&
                                    ((item.Second.status == ProcessStateTools.StatusEnum.SUCCESS) ||
                                    (item.Second.status == ProcessStateTools.StatusEnum.WARNING)))).Select(x => x.First).ToList();
                //}
                //else
                //{
                //    m_CBHierarchy.cssCustodianEODNotValid = new List<SpheresIdentification>();
                //    m_CBHierarchy.cssCustodianEODValid = new List<SpheresIdentification>();
                //}

                // FI 20161021 [22152] 
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                // - Charger la liste des chambres, sur lesquelles le traitement de fin de journée est en erreur ou non effectué
                //
                // - Remarque : Cette liste contient notamment les CSS pour lesquels il existe une nouvelle activité (cad date business > date de traitement). 
                //              Cela est sans indidence puisqu'il n'existe aucun flux à la date de traitement
                /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                int[] idAcssUnValid = (from item in m_CBHierarchy.cssCustodianEODUnValid
                                       select item.OTCmlId).ToArray();

                // FI 20170329 [23022] Lecture de toutes les lignes présentes dans ENTITYMARKET
                // Il ne faut pas exclure les chambres pour lesquelles il y a eu un chgt de journée 
                //IEnumerable<DataRow> drCssNotValid = drEntityMarketBusiness.Where(x => ArrFunc.ExistInArray(idAcssNotValid.Cast<IComparable>().ToArray(), (IComparable)Convert.ToInt32(x["IDA_CSSCUSTODIAN"])));
                IEnumerable<DataRow> drCssUnValid = drEntityMarket.Where(x => ArrFunc.ExistInArray(idAcssUnValid.Cast<IComparable>().ToArray(), (IComparable)Convert.ToInt32(x["IDA_CSSCUSTODIAN"])));

                // FI 20161021 [22152] Add msg d'avertissment s'il existe des traitements de fin de journée non traités ou en erreur
                // FI 20170329 [23022] Restriction aux seuls tels que DTENTITY == m_CBHierarchy.DtBusiness
                // => Cela évite d'afficher un message d'erreur s'il existe une nouvelle activité en date > DTENTITY sur une nouvelle chambre (cette dernière est dans la liste drCssUnValid)
                IEnumerable<DataRow> drDtBusinessCssUnValid = drCssUnValid.Where(x => Convert.ToDateTime(x["DTENTITY"]) == m_CBHierarchy.DtBusiness);
                if (drDtBusinessCssUnValid.Count() > 0)
                {
                    //LOG-04001
                    //<b>Attention</b>, une partie de l'activité de l'entité porte sur des marchés pour lesquels les traitements de fin de journée sont non effectué ou en erreur. 
                    //L'activité portant sur ces marchés sera donc ignorée dans le présent traitement. Lorsque l'activité sur ces marchés aura été traitée en date du {1}, un nouveau traitement devra impérativement être lancé à cette date afin de prendre en considération la totalité de l'activité de l'entité !
                    Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 4009), 2,
                        new LogParam(DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness))));
                }

                //////////////////////////////////////////////////////////////////////////////////////////////////
                // - Charger la liste des chambres, sur lesquelles (la date de compensation sélectionnée est fériée) et (DTENTITY != la date de compensation sélectionnée (cas rencontré lorsque Spheres® n'utilise pas le businessCentre de l'entité)) 
                /////////////////////////////////////////////////////////////////////////////////////////////////////

                if ((Cst.ErrLevel.IRQ_EXECUTED != codeReturn) &&
                    (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref codeReturn)))
                {

                    IEnumerable<DataRow> drCssInHoliday = drEntityMarket.Where(
                        x => Convert.ToDateTime(x["DTENTITY"]) != m_CBHierarchy.DtBusiness &&
                        new EFS_BusinessCenters(CSTools.SetCacheOn(pCS),
                            new BusinessCenters(new BusinessCenter[] { new BusinessCenter(Convert.ToString(x["IDBCENTITY"])) }), null).IsHoliday(m_CBHierarchy.DtBusiness)
                        );

                    //////////////////////////////////////////////////////////////////////////////////////////////////
                    // Chargement des chambres pour lesquelles le traitement de cashBalance considère le deposit de la veille  
                    /////////////////////////////////////////////////////////////////////////////////////////////////////
                    // PM 20150511 [20575] Gestion DtEntity
                    IEnumerable<Pair<int, string>> cssUsingPreviousDeposit =
                            from rowCss in
                                (from rowEntityMarket in drCssUnValid.Union(drCssInHoliday)
                                 select rowEntityMarket
                                ).GroupBy(row => Convert.ToInt32(row["IDA_CSSCUSTODIAN"]))
                            select new Pair<int, string>(rowCss.Key, (from row in rowCss select Convert.ToString(row["IDBCENTITY"])).First());

                    foreach (var item in cssUsingPreviousDeposit)
                    {
                        // Chercher la date du dernier jour ouvré par rapport la date de compensation sélectionnée 
                        // Cette date sera utilisée pour charger le Déposit sur cette chambre
                        int idA_CSS = item.First;
                        string businessCenter = item.Second;
                        //
                        IProductBase productBase = Tools.GetNewProductBase();
                        BusinessCenters bcs = new BusinessCenters(new BusinessCenter[] { new BusinessCenter(businessCenter) });
                        IOffset offset = productBase.CreateOffset(PeriodEnum.D, -1, DayTypeEnum.ExchangeBusiness);
                        IBusinessDayAdjustments bda = productBase.CreateBusinessDayAdjustments(BusinessDayConventionEnum.PRECEDING, bcs.businessCenter[0].Value);
                        DateTime dtBusinessPrev = Tools.ApplyOffset(CSTools.SetCacheOn(pCS), m_CBHierarchy.DtBusiness, offset, bda, null);
                        //
                        m_CBHierarchy.CssUsingPreviousDeposit.Add(new Pair<int, DateTime>(idA_CSS, dtBusinessPrev));
                    }
                }

            }
            return codeReturn;
        }

        /// <summary>
        /// Vérifie s'il existe un Trade cash-Balance du jour:
        /// <para>Pour le couple ({pIda},{pIdb})</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIda"></param>
        /// <param name="pIdb"></param>
        /// <param name="pIdt"></param>
        /// EG 20140225 REFACTORING (Use TRADEINSTRUMENT : IDA_RISK/IDB_RISK/IDA_ENTITY)
        /// FI 20161021 [22152] Modify
        // EG 20180205 [23769] Add dbTransaction  
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        private void CheckTradeExist(string pCS, IDbTransaction pDbTransaction, int pIda, int pIdb, out int pIdt)
        {
            pIdt = 0;
            //
            try
            {
                // FI 20161021 [22152] suppression du parameter @PRODUCT
                DataParameter dataParameter = null;
                DataParameters dataParameters = new DataParameters();
                //
                dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
                dataParameters.Add(dataParameter, pIda);
                //
                dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA_ENTITY);
                dataParameters.Add(dataParameter, m_CBHierarchy.Ida_Entity);
                //
                dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDB);
                dataParameters.Add(dataParameter, pIdb);
                //
                dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTBUSINESS); // FI 20201006 [XXXXX] DbType.Date
                dataParameters.Add(dataParameter, m_CBHierarchy.DtBusiness);

                dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.STATUS);
                dataParameters.Add(dataParameter, Cst.StatusActivation.REGULAR);

                string sqlQuery = String.Format(@"select tr.IDT, tr.IDENTIFIER
                from dbo.TRADE tr
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) and ({0})
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = 'cashBalance')
                where (tr.DTBUSINESS = @DTBUSINESS) and (tr.IDSTACTIVATION = @STATUS) and (tr.IDA_ENTITY = @IDA_ENTITY) and (tr.IDA_RISK = @IDA) and (tr.IDB_RISK = @IDB)",
                OTCmlHelper.GetSQLDataDtEnabled(pCS, "ns"));


                object result = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), dataParameters.GetArrayDbParameter());
                if (null != result)
                    pIdt = Convert.ToInt32(result);
            }
            catch (Exception ex)
            {
                // FI 20200603 [XXXXX] AddCriticalException
                ProcessState.AddCriticalException(ex); 
                
                // Une erreur s'est produite lors de la recherche d'un calcul précédent
                Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 4064), 3,
                    new LogParam(m_CBHierarchy.Identifier_Entity),
                    new LogParam(DtFunc.DateTimeToStringDateISO(m_CBHierarchy.DtBusiness))));

                throw;
            }
        }

        /// <summary>
        /// Recherche si le trade dont l'Id est passé en paramètre possède au moins un événement
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// /// <param name="pIdT"></param>
        /// <returns></returns>
        /// PM 20201215 [XXXXX] New
        private bool IsEventExist(string pCS, IDbTransaction pDbTransaction, int pIdT)
        {
            bool hasEvent = false;
            if (pIdT > 0)
            {
                try
                {
                    DataParameters dataParameters = new DataParameters();
                    DataParameter dataParameter = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT);
                    dataParameters.Add(dataParameter, pIdT);

                    string sqlQuery = @"select count(*)
                    from dbo.EVENT e
                    where e.IDT = @IDT";

                    object result = DataHelper.ExecuteScalar(pCS, pDbTransaction, CommandType.Text, sqlQuery.ToString(), dataParameters.GetArrayDbParameter());
                    if (null != result)
                    {
                        int count = Convert.ToInt32(result);
                        hasEvent = (count > 0);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                }
            }
            return hasEvent;
        }

        /// <summary>
        /// Générer la liste des couples (CBO/MRO, Book), en partant des acteurs CBO à la racine de la structure
        /// </summary>
        private List<CBPartyTradeInfo> GenerateActorBookCouple()
        {
            List<CBActorNode> actorCBOList = m_CBHierarchy.FindActor(RoleActor.CSHBALANCEOFFICE);
            List<CBPartyTradeInfo> ret = new List<CBPartyTradeInfo>();
            List<CBActorNode> firstMROChilds = null;
            List<int> depositBooks = null;
            List<int> cboBooks = null;
            //
            foreach (CBActorNode actorCBO in actorCBOList)
            {
                firstMROChilds = CBTools.FindFirst(actorCBO.ChildActors, RoleActor.MARGINREQOFFICE);
                //
                // ************************************************************************************************
                // L'algorithme s'appuit sur deux principes:
                // 1- S'il y'a un calcul Elementary (soit sur un CBO soit sur un MRO), 
                //      alors traiter tous les Books dans la même boucle, y compris le Book global
                // 2- Faire en sorte de ne pas traiter le Book Global deux fois (soit sur un CBO soit sur un MRO).
                // ************************************************************************************************
                //
                // Le CBO est avec calcul Global, Considérer le CBO comme acteur et Considérer le Book spécifié sur le CBO:
                // * Uniquement si :
                //  - Le CBO n'est pas avec calcul Elementary (dans le cas contraire, le Book global sera pris dans la même boucle du calcul elementary)
                //  - ou bien le CBO n'est pas lui même MRO (dans le cas contraire, le calcul Elementary se fera au niveau des MRO enfants du CBO)
                //  - ou bien, le CBO est lui même MRO, avec:
                //      . le MRO n'est pas avec calcul Elementary
                //      . ET si le MRO est avec calcul Global, alors le Book global du MRO doit être différent de celui spécifié sur le CBO
                //
                if (actorCBO.IsCBOGlobalScopWithBook &&
                    (actorCBO.IsCBONotElementaryScope || actorCBO.IsNotMRO ||
                    (actorCBO.IsMRONotElementaryScope && (actorCBO.IsMRONotGlobalScopOrWithoutBook || (actorCBO.BusinessAttribute.IdB_CBO != actorCBO.BusinessAttribute.IdB_MRO)))))
                {
                    ret.Add(
                        new CBPartyTradeInfo(actorCBO.Ida, actorCBO.Identifier, actorCBO.Roles,
                            actorCBO.BusinessAttribute.IdB_CBO, actorCBO.BusinessAttribute.IdentifierB_CBO,
                            actorCBO, firstMROChilds));
                }
                //
                // Le CBO est avec calcul Elementary:
                // - Tous ces Books, s'il est lui même MRO 
                // - Sinon, tous les books des MRO enfants
                if (actorCBO.IsCBOElementaryScope)
                {
                    // Le CBO est lui même MRO
                    if (actorCBO.IsMRO)
                    {
                        //  * Le MRO est "avec book", alors prendre le Book spécifié sur le MRO
                        //  Uniquement si :
                        //  - le MRO est SANS calcul du déposit sur tous les books
                        if (actorCBO.IsMROGlobalScopWithBook && actorCBO.IsMRONotElementaryScope)
                        {
                            bool isBookOfCBO = (actorCBO.IsCBOGlobalScopWithBook && (actorCBO.BusinessAttribute.IdB_CBO == actorCBO.BusinessAttribute.IdB_MRO));
                            //
                            ret.Add(
                                new CBPartyTradeInfo(actorCBO.Ida, actorCBO.Identifier, actorCBO.Roles,
                                    actorCBO.BusinessAttribute.IdB_MRO, actorCBO.BusinessAttribute.IdentifierB_MRO,
                                    actorCBO, isBookOfCBO, firstMROChilds, actorCBO, true));
                        }
                        //
                        //  * Le MRO est avec calcul du déposit sur tous les books, alors prendre tous les Books sur le MRO
                        if (actorCBO.IsMROElementaryScope)
                        {
                            depositBooks = (from flow in actorCBO.Flows select flow.IDB).Distinct().ToList();
                            //
                            foreach (int depositBook in depositBooks)
                            {
                                string depositBookIdentifier = (from book in actorCBO.Books where book.Idb == depositBook select book.Identifier).FirstOrDefault();
                                bool isBookOfCBO = (actorCBO.IsCBOGlobalScopWithBook && (actorCBO.BusinessAttribute.IdB_CBO == depositBook));
                                bool isBookOfMRO = (actorCBO.IsMROGlobalScopWithBook && (actorCBO.BusinessAttribute.IdB_MRO == depositBook));
                                //
                                ret.Add(
                                    new CBPartyTradeInfo(actorCBO.Ida, actorCBO.Identifier, actorCBO.Roles,
                                        depositBook, depositBookIdentifier,
                                        actorCBO, isBookOfCBO, firstMROChilds, actorCBO, isBookOfMRO));
                            }
                        }
                    }
                    else
                    {
                        // Sinon considérer tous les MRO enfants
                        foreach (CBActorNode childMRO in firstMROChilds)
                        {
                            //  * Le MRO est "avec book", alors prendre le Book spécifié sur le MRO
                            //  Uniquement si :
                            //  - le MRO est SANS calcul du déposit sur tous les books
                            if (childMRO.IsMROGlobalScopWithBook && childMRO.IsMRONotElementaryScope)
                            {
                                ret.Add(
                                    new CBPartyTradeInfo(childMRO.Ida, childMRO.Identifier, childMRO.Roles,
                                        childMRO.BusinessAttribute.IdB_MRO, childMRO.BusinessAttribute.IdentifierB_MRO,
                                        actorCBO, childMRO));
                            }
                            //
                            //  * Le MRO est avec calcul du déposit sur tous les books, alors prendre tous les Books sur le MRO
                            if (childMRO.IsMROElementaryScope)
                            {
                                depositBooks = (from flow in childMRO.Flows select flow.IDB).Distinct().ToList();
                                //
                                foreach (int depositBook in depositBooks)
                                {
                                    string depositBookIdentifier = (from book in childMRO.Books where book.Idb == depositBook select book.Identifier).FirstOrDefault();
                                    bool isBookOfMRO = (childMRO.IsMROGlobalScopWithBook && (childMRO.BusinessAttribute.IdB_MRO == depositBook));
                                    //
                                    ret.Add(
                                    new CBPartyTradeInfo(childMRO.Ida, childMRO.Identifier, childMRO.Roles,
                                        depositBook, depositBookIdentifier,
                                        actorCBO, childMRO, isBookOfMRO));
                                }
                            }
                        }
                        //PM 20140904 Prendre également les books du CBO
                        cboBooks = (from flow in actorCBO.Flows select flow.IDB).Distinct().ToList();
                        //
                        foreach (int cboBook in cboBooks)
                        {
                            string cboBookIdentifier = (from book in actorCBO.Books where book.Idb == cboBook select book.Identifier).FirstOrDefault();
                            // Pas Glop : l'acteur CBO doit également être affecté en tant qu'acteur MRO !
                            ret.Add(
                                new CBPartyTradeInfo(actorCBO.Ida, actorCBO.Identifier, actorCBO.Roles,
                                    cboBook, cboBookIdentifier,
                                    actorCBO, false, null, actorCBO, false));
                        }
                    }
                }
            }

            //
            return ret;
        }

        /// <summary>
        /// Send message queue for EventGen Process
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// FI 20140415 isDelEvent = false
        private void SendMessageQueueToEvent(string pCS, int pIdT)
        {
            Boolean isDelEvent = false; // FI 20140415 DelEvent déjà effectuer dans ChekAndRecord
            EventsGenMQueue eventsGenMQueue = CaptureTools.GetMQueueForEventProcess(pCS, pIdT, isDelEvent, MQueue.header.requester);
            if (null != eventsGenMQueue)
            {
                MQueueSendInfo sendInfo = EFS.SpheresService.ServiceTools.GetMqueueSendInfo(Cst.ProcessTypeEnum.EVENTSGEN, this.AppInstance);
                if (false == sendInfo.IsInfoValid)
                    throw new Exception("MOM information unavailable for Spheres® EventsGen service");

                // EG/RD 201402 [19615/19629] Mise à jour du compteur déplacée avant postage du message à EVENTSGEN
                Tracker.AddPostedSubMsg(1, Session);
                MQueueTools.Send(eventsGenMQueue, sendInfo);
            }
        }

        /// <summary>
        /// Retourne les chambres/Custudian présents dans les enregistrements {pEntityMarket} 
        /// </summary>
        /// <param name="pEntityMarket"></param>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method (Méthode anciennement présente dans NormMsgFactory_RISKPERFORMANCE)
        private static Hashtable GetDistinctCSSCustodian(DataRow[] pEntityMarket)
        {
            Hashtable ht = new Hashtable();
            foreach (DataRow row in pEntityMarket)
            {
                int idACss = Convert.ToInt32(row["IDA_CSSCUSTODIAN"]);
                string cssIdentifier = Convert.ToString(row["IDENTIFIER_CSSCUSTODIAN"]);

                if (false == ht.ContainsKey(idACss))
                    ht.Add(idACss, cssIdentifier);
            }
            return ht;
        }

        /// <summary>
        /// Contrôle l'execution du traitement CashBalance en fonction du paramètre CTRL_EOD_MODE
        /// <para>Retourne Cst.ErrLevel.SUCCESS si le traitement doit être exécuté</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private Cst.ErrLevel CashBalanceControlEOD()
        {
            //Si ret == Cst.ErrLevel.SUCCESS => Cela signifie que le traitement autorise l'exécution du traitement
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            ControlEODMode control = EndOfDayControlMode(this.MQueue as RiskPerformanceMQueue);
            if (control != ControlEODMode.NONE)
            {
                switch (control)
                {
                    case ControlEODMode.MODE1:
                    case ControlEODMode.MODE2:
                    case ControlEODMode.MODE3:

                        List<SpheresIdentification> cssCustodianContext = GetCssCustodianContextControl();

                        if (cssCustodianContext.Count > 0) //si idACssCustodianContext ==0 cela signifie qu'il n'y a aucun enregistrement dans ENTITYMARKET => aucun contrôle ici
                        {
                            //Vérification de la condition1
                            //Les derniers traitements EOD de chaque chambre du périmètre sont-ils en succès ou warning ?
                            List<Pair<SpheresIdentification, ProcessStatus>> eodStatus = LoadStatusEOD(cssCustodianContext, false);
                            List<SpheresIdentification> cssCustodianEODNoValid = eodStatus.Where(item => ((null == item.Second) ||
                                                          (item.Second.status != ProcessStateTools.StatusEnum.SUCCESS &&
                                                           item.Second.status != ProcessStateTools.StatusEnum.WARNING))).Select(item => item.First).ToList();

                            if (cssCustodianEODNoValid.Count() > 0)
                            {
                                ControlEODLogStatus status = EndOfDayControlLogLevel(this.MQueue as RiskPerformanceMQueue);

                                foreach (SpheresIdentification item in cssCustodianEODNoValid)
                                {
                                    switch (status)
                                    {
                                        case ControlEODLogStatus.INFO:
                                        case ControlEODLogStatus.ERROR:
                                        case ControlEODLogStatus.WARNING:
                                            // FI 20200603 [XXXXX] SetErrorWarning
                                            ProcessState.SetErrorWarning(ProcessStateTools.ParseStatus(status.ToString()));

                                            Logger.Log(new LoggerData(LoggerTools.StatusToLogLevelEnum(status), new SysMsgCode(SysCodeEnum.LOG, 4005), 1,
                                                new LogParam(item.Identifier),
                                                new LogParam(m_ProcessInfo.EntityIdentifier),
                                                new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness))));

                                            if ((status == ControlEODLogStatus.INFO) || (status == ControlEODLogStatus.WARNING))
                                                ret = Cst.ErrLevel.NOTHINGTODO;
                                            else if (status == ControlEODLogStatus.ERROR)
                                                ret = Cst.ErrLevel.FAILURE;
                                            else
                                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", status.ToString()));
                                            break;

                                        default:
                                            throw new Exception(StrFunc.AppendFormat("{0} is not implemented", status.ToString()));
                                    }
                                }
                            }
                            else
                            {
                                // Vérification de la condition2
                                // Le dernier traitement CashBalance exécuté est-il valide vis-à-vis d'une des chambre du périmètre ?
                                List<Pair<SpheresIdentification, ProcessStatus>> eodStatusInLastCB = LoadStatusEOD(cssCustodianContext, true);
                                var eodNotValidInLastCB = eodStatusInLastCB.Where(item => ((null == item.Second) ||
                                                              (item.Second.status != ProcessStateTools.StatusEnum.SUCCESS &&
                                                               item.Second.status != ProcessStateTools.StatusEnum.WARNING)));

                                if (eodNotValidInLastCB.Count() == 0)
                                {
                                    string cssCustodianNewEODExpected = string.Empty;
                                    int numberInfo = 0;
                                    switch (control)
                                    {
                                        case ControlEODMode.MODE1:
                                            numberInfo = 4008;
                                            ret = Cst.ErrLevel.NOTHINGTODO;
                                            break;

                                        case ControlEODMode.MODE2:
                                        case ControlEODMode.MODE3:
                                            //newCssCustodianEOD => liste des css/custodian dot les EOD sont postérieurs au dernier traitement CashBalance
                                            var newCssCustodianEOD =
                                               from item in eodStatus
                                               join itemcb in eodStatusInLastCB on item.First.OTCmlId equals itemcb.First.OTCmlId
                                               where item.Second.dtEnd > itemcb.Second.dtEnd
                                               select item.First;

                                            if (control == ControlEODMode.MODE2)
                                            {
                                                if (newCssCustodianEOD.Count() == 0)
                                                {
                                                    numberInfo = 4006;
                                                    ret = Cst.ErrLevel.NOTHINGTODO;
                                                }
                                            }
                                            else if (control == ControlEODMode.MODE3)
                                            {
                                                if (false == (newCssCustodianEOD.Count() == cssCustodianContext.Count))
                                                {
                                                    numberInfo = 4007;
                                                    ret = Cst.ErrLevel.NOTHINGTODO;

                                                    string[] lstCssCustodianNewEODExpected =
                                                    (from item in eodStatus
                                                     join itemcb in eodStatusInLastCB on item.First.OTCmlId equals itemcb.First.OTCmlId
                                                     where item.Second.dtEnd <= itemcb.Second.dtEnd
                                                     select item.First.Identifier).ToArray();

                                                    cssCustodianNewEODExpected = StrFunc.StringArrayList.StringArrayToStringList(lstCssCustodianNewEODExpected);
                                                }
                                            }


                                            break;
                                        default:
                                            throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", control.ToString()));
                                    }


                                    if (ret == Cst.ErrLevel.NOTHINGTODO)
                                    {
                                        Nullable<DateTime> dtEndLastCB = GetDtEndLastCashBalance();
                                        if (false == dtEndLastCB.HasValue)
                                            throw new InvalidProgramException("Last CashBalance : dtEnd is unknown");
                                        
                                        List<LogParam> logParam = new List<LogParam>
                                        {
                                            new LogParam(m_ProcessInfo.EntityIdentifier),
                                            new LogParam(DtFunc.DateTimeToStringDateISO(m_ProcessInfo.DtBusiness)),
                                            new LogParam(DtFunc.DateTimeToStringISO(dtEndLastCB.Value))
                                        };
                                        if (StrFunc.IsFilled(cssCustodianNewEODExpected))
                                        {
                                            logParam.Add(new LogParam(cssCustodianNewEODExpected));
                                        }
                                        Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, numberInfo), 1, logParam));
                                    }
                                }
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", control.ToString()));
                }
            }
            return ret;
        }

        /// <summary>
        /// Liste des chambres\Custodian où le control EOD est appliqué avant exécution du traitement CashBalance
        /// <para>En l'absence du CTRL_EOD_CSSCUSTODIANLIST cette méthode retourne toutes les chambres présentes dans ENTITYMARKET en rapport avec (entity,dtbusiness)</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private List<SpheresIdentification> GetCssCustodianContextControl()
        {
            List<SpheresIdentification> ret = new List<SpheresIdentification>();
            RiskPerformanceMQueue riskQueue = this.MQueue as RiskPerformanceMQueue;

            string list = riskQueue.GetStringValueParameterById(RiskPerformanceMQueue.PARAM_CTRL_EOD_CSSCUSTODIANLIST);
            if (StrFunc.IsEmpty(list))
                list = "ALL";

            DataRow[] dr = MarketTools.LoadEntityMarketActivity(Cs, m_ProcessInfo.Entity, m_ProcessInfo.DtBusiness);
            Hashtable ht = GetDistinctCSSCustodian(dr);

            List<int> lst = new List<int>();
            if (list == "ALL")
            {
                // Spheres® selectionne toutes les chambres/custodians en vigueur dans ENTITYMARKET
                lst = ht.Keys.Cast<int>().ToList();
            }
            else
            {
                List<SQL_Actor> actor = LoadActorFromList(Cs, list, new string[] { "IDA", "IDENTIFIER" });
                if (actor.Count == 0)
                    throw new ArgumentException(StrFunc.AppendFormat("Parameter:{0} doesn't contains valid actor ({1})", RiskPerformanceMQueue.PARAM_CTRL_EOD_CSSCUSTODIANLIST, list));

                List<int> lstIdA = actor.Select(item => item.Id).Cast<int>().ToList();

                lst =
                (from int idA in ht
                 where lstIdA.Contains(idA)
                 select idA).ToList();
            }

            foreach (int item in lst)
                ret.Add(new SpheresIdentification(item, ht[item].ToString()));

            return ret;
        }

        /// <summary>
        /// Récupère le paramètre PARAM_CTRL_EOD_MODE
        /// <para>Ce paramètre pilote l'exécution du traitement CashBalance en fonction des process EOD</para>
        /// </summary>
        /// <param name="riskQueue"></param>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private static ControlEODMode EndOfDayControlMode(RiskPerformanceMQueue riskQueue)
        {
            ControlEODMode ret = ControlEODMode.NONE;
            string mode = riskQueue.GetStringValueParameterById(RiskPerformanceMQueue.PARAM_CTRL_EOD_MODE);

            if (StrFunc.IsFilled(mode))
            {
                if (false == System.Enum.IsDefined(typeof(ControlEODMode), mode))
                    throw new ArgumentException(StrFunc.AppendFormat("value:{0} is not a valid value for parameter:{1}", mode, RiskPerformanceMQueue.PARAM_CTRL_EOD_MODE));

                ret = (ControlEODMode)System.Enum.Parse(typeof(ControlEODMode), mode);
            }
            return ret;
        }

        /// <summary>
        /// Récupère le paramètre PARAM_CTRL_EOD_LOGSTATUS
        /// <para>Ce paramètre pilote le statut final du traitement lorsque le process n'est pas écécuté à cause du contrôle PARAM_CTRL_EOD</para>
        /// <param name="riskQueue"></param>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private static ControlEODLogStatus EndOfDayControlLogLevel(RiskPerformanceMQueue riskQueue)
        {
            ControlEODLogStatus ret = ControlEODLogStatus.WARNING;
            string status = riskQueue.GetStringValueParameterById(RiskPerformanceMQueue.PARAM_CTRL_EOD_LOGSTATUS);

            if (StrFunc.IsFilled(status))
            {
                if (false == System.Enum.IsDefined(typeof(ControlEODLogStatus), status))
                    throw new ArgumentException(StrFunc.AppendFormat("value:{0} is not a valid value for parameter:{1}", status, RiskPerformanceMQueue.PARAM_CTRL_EOD_LOGSTATUS));

                ret = (ControlEODLogStatus)System.Enum.Parse(typeof(ControlEODLogStatus), status);
            }

            return ret;
        }

        /// <summary>
        /// Retourne les Css/Custodian déclarés dans {pList}
        /// <para>Retourne une liste vide si aucun acteur est connu</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pList">Liste des acteurs séparé par des ;</param>
        /// <param name="pColumn">Liste des colonnes à charger</param>
        /// <exception cref="ArgumentException si pList est non renseignée"></exception>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private static List<SQL_Actor> LoadActorFromList(string pCS, string pList, string[] pColumn)
        {
            if (StrFunc.IsEmpty(pList))
                throw new ArgumentException("pList Argument is not valid. Empty value is not allowed");

            List<SQL_Actor> ret = new List<SQL_Actor>();

            string[] list = pList.Split(";".ToCharArray());
            if (ArrFunc.IsFilled(list))
            {
                for (int i = 0; i < ArrFunc.Count(list); i++)
                {
                    if (StrFunc.IsFilled(list[i]))
                    {
                        SQL_Actor sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), list[i], SQL_Table.RestrictEnum.No,
                            SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);

                        sqlActor.LoadTable(pColumn);

                        if (sqlActor.IsLoaded)
                        {
                            ret.Add(sqlActor);
                        }
                        else if (IntFunc.IsPositiveInteger(list[i]))
                        {
                            sqlActor = new SQL_Actor(CSTools.SetCacheOn(pCS), SQL_TableWithID.IDType.Id, list[i], SQL_Table.RestrictEnum.No,
                            SQL_Table.ScanDataDtEnabledEnum.No, null, string.Empty);
                            sqlActor.LoadTable(pColumn);
                            if (sqlActor.IsLoaded)
                                ret.Add(sqlActor);
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne les status des derniers traitements EOD exécutés ou Retourne les status des traitements EOD lorsque le tout dernier traitement CashBalance a été exécuté
        /// <para>Le 1er élément de la pair représente un css/custodian</para>
        /// <para>Le 2ème élément de la pair représente le satut du traitement EOD. La valeur null est retournée lorsqu'il n'y a pas eu de traitement EOD</para>
        /// </summary>
        /// <param name="pCssCustodian">Liste des chambres/custodian</param>
        /// <param name="pIsFromLastCashBalance">True: Retourne les statuts EOD en vigueur lors du dernier traitement CashBalance, sinon les status EOD des derniers traitements EOD</param>
        /// FI 20141126 [20526] Add method
        private List<Pair<SpheresIdentification, ProcessStatus>> LoadStatusEOD(List<SpheresIdentification> pCssCustodian, Boolean pIsFromLastCashBalance)
        {
            if (null == pCssCustodian)
                throw new NullReferenceException("paramter pIdACss is null");

            List<Pair<SpheresIdentification, ProcessStatus>> ret = new List<Pair<SpheresIdentification, ProcessStatus>>();

            // Initialisation de ProcessStatus à null 
            foreach (SpheresIdentification item in pCssCustodian)
                ret.Add(new Pair<SpheresIdentification, ProcessStatus>(item, null));

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDA_ENTITY), this.m_ProcessInfo.Entity);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTBUSINESS), this.m_ProcessInfo.DtBusiness);


            string[] status = ProcessStateTools.GetStatusByReadyState(ProcessStateTools.ReadyStateEnum.TERMINATED);
            string sqlStatusIn = DataHelper.SQLColumnIn(Cs, "pr.STATUS", status, TypeData.TypeDataEnum.@string);

            string select;
            if (pIsFromLastCashBalance)
            {
                dp.Add(new DataParameter(Cs, "CBTIMING", DbType.AnsiString, 64), this.m_ProcessInfo.Timing);
                // Donne les status des traitements EOD lors du dernier CB si dernier était en succès
                select = StrFunc.AppendFormat(@"select pr.IDA_CSSCUSTODIAN, pr.STATUS, pr.DTUPD as DTEND 
                from dbo.POSREQUEST pr 
                inner join dbo.CBREQUEST cb on cb.IDA_CBO is null and cb.IDB_CBO is null and 
                cb.DTBUSINESS=pr.DTBUSINESS and cb.IDA_ENTITY= pr.IDA_ENTITY and cb.CBTIMING=@CBTIMING and cb.STATUS='SUCCESS'
                where pr.REQUESTTYPE='EOD' and pr.DTBUSINESS=@DTBUSINESS and pr.IDA_ENTITY=@IDA_ENTITY and pr.DTUPD < cb.DTEND 
                and {0}
                order by pr.IDA_CSSCUSTODIAN, pr.DTUPD desc", sqlStatusIn);
            }
            else
            {

                select = StrFunc.AppendFormat(@"select pr.IDA_CSSCUSTODIAN, pr.STATUS, pr.DTUPD as DTEND
                from dbo.POSREQUEST pr 
                where pr.REQUESTTYPE='EOD' and pr.DTBUSINESS=@DTBUSINESS and pr.IDA_ENTITY=@IDA_ENTITY 
                and {0}
                order by pr.IDA_CSSCUSTODIAN, pr.DTUPD desc", sqlStatusIn);
            }

            QueryParameters qry = new QueryParameters(Cs, select, dp);
            DataTable dt = DataHelper.ExecuteDataTable(Cs, qry.Query, qry.Parameters.GetArrayDbParameter());

            foreach (Pair<SpheresIdentification, ProcessStatus> item in ret)
            {
                DataRow dr = dt.Select("IDA_CSSCUSTODIAN=" + item.First.OTCmlId).FirstOrDefault();
                if (null != dr)
                {
                    item.Second = new ProcessStatus()
                    {
                        status = ((ProcessStateTools.StatusEnum)System.Enum.Parse(typeof(ProcessStateTools.StatusEnum), dr["STATUS"].ToString())),
                        // FI 20200820 [25468] Dates systemes en UTC
                        dtEnd = DateTime.SpecifyKind(Convert.ToDateTime(dr["DTEND"]), DateTimeKind.Utc)
                    };
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne la date du dernier traitement CashBalance
        /// <para>Retourne null si le traitement CashBAlance n'a jamais été exécuté poule le couple {Entité,Date}</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20141126 [20526] Add method
        private Nullable<DateTime> GetDtEndLastCashBalance()
        {
            Nullable<DateTime> ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDA_ENTITY), this.m_ProcessInfo.Entity);
            dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTBUSINESS), this.m_ProcessInfo.DtBusiness);
            dp.Add(new DataParameter(Cs, "CBTIMING", DbType.AnsiString, 64), this.m_ProcessInfo.Timing);

            string select = @"
select cb.DTEND 
from dbo.CBREQUEST cb
where 
cb.IDA_CBO is null and cb.IDB_CBO is null and  
cb.DTBUSINESS=@DTBUSINESS and cb.IDA_ENTITY= @IDA_ENTITY and cb.CBTIMING=@CBTIMING";

            QueryParameters qry = new QueryParameters(Cs, select, dp);

            object obj = DataHelper.ExecuteScalar(Cs, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (null != obj)
                ret = Convert.ToDateTime(obj);

            return ret;
        }


        /// <summary>
        /// Chargement des status des traitements de fin de journée des marchés/cssCustodian 
        /// </summary>
        /// <param name="pCBTradeInfo"></param>
        /// FI 20170207 [22151] Appel à SetEndOfDayStatus
        /// FI 20170208 [22151][22152] Modify
        /// FI 20170316 [22950] Modify
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20200226 Refactoring suite à à TRADEINSTRUMENT (INSTRUMENTNO=1) dans TRADE
        // EG 20230227 [22151][22152] Alimentation de la table de travail CBTradeWorkTable via SqlDataAdapter (+ UpdateBatchSize)
        // EG 20240205 [XXXXX] Ajout dbo. sur table de travail
        private void SetEndOfDayStatus(string pCS, CBTradeInfo pCBTradeInfo)
        {
            string tableTmp = string.Empty;
            DataSet ds = new DataSet();

            try
            {
                // FI 20170208 [22151][22152] Utilisation de TradesCashFlowSourceAllStatus pour avoir l'ensemble de l'activité du jour 
                // FI 20170316 [22950] Utilisation d'une table temporaire
                string query = string.Empty;

                using (IDbConnection dbConnection = DataHelper.OpenConnection(pCS))
                {

                    if (ArrFunc.IsFilled(pCBTradeInfo.TradesCashFlowSourceAllStatus))
                    {
                        /* Liste des trades impliqués dans le cashBalance courant */
                        tableTmp = CreateCBTradeWorkTable(Cs, this.Session.BuildTableId());
                        DataTable dt = DataHelper.ExecuteDataTable(pCS, $"select IDT from dbo.{tableTmp}");
                        dt.PrimaryKey = new DataColumn[] { dt.Columns["IDT"] };
                        pCBTradeInfo.TradesCashFlowSourceAllStatus.ForEach(item => dt.Rows.Add(dt.NewRow()["IDT"] = item.First));
                        Common.AppInstance.TraceManager.TraceVerbose(this, $"Count TradesCashFlowSourceAllStatus: {pCBTradeInfo.TradesCashFlowSourceAllStatus.Count}");
                        DataHelper.ExecuteDataAdapter(Cs, $"select IDT from dbo.{tableTmp}", dt, 250);

                        /*1er partie de la requête => activité du fait des trades jours */
                        /*2nd partie de la requête => activité habituelle (Référentiel "Activité sur les marchés") */
                        // FI 20170316 [22950] Suppression de la jointure sur trade puisque TradesCashFlowSourceAllStatus contient uniquement des trades jours
                        // RD 20171024 [23511] Use CBOMARKET.IDA_CSSCUSTODIAN
                        query = @"select distinct @IDA as IDA_CBO, @IDB as IDB_CBO, m.IDM, m.FIXML_SECURITYEXCHANGE, a.IDA, {0}
                        from {1} tmp 
                        inner join dbo.TRADE tr on (tr.IDT = tmp.IDT) 
                        inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = tr.IDM)
                        inner join dbo.ACTOR a on (a.IDA = isnull(tr.IDA_CSSCUSTODIAN, m.IDA))
                        union 
                        select cboMk.IDA_CBO, cboMk.IDB_CBO, m.IDM, m.FIXML_SECURITYEXCHANGE, a.IDA, {0}
                        from dbo.CBOMARKET cboMk
                        inner join dbo.BOOK b on (b.IDB = cboMk.IDB_CBO) and (b.IDA_ENTITY = @IDA_ENTITY)
                        inner join dbo.VW_ENTITYMARKET_ACTIVITY em on (em.IDA = @IDA_ENTITY) and (em.DTENTITY = @DTBUSINESS) and (em.IDM = cboMk.IDM) and (em.IDA_CSSCUSTODIAN = isnull(cboMk.IDA_CSSCUSTODIAN, em.IDA_CSSCUSTODIAN))
                        inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = em.IDM)
                        inner join dbo.ACTOR a on (a.IDA = em.IDA_CSSCUSTODIAN)
                        where (cboMk.IDA_CBO = @IDA) and (cboMk.IDB_CBO = @IDB) and {2}";

                        query = StrFunc.AppendFormat(query,
                                OTCmlHelper.GetACTORXMLId(Cs, "a", "XMLID_CSSCUSTODIAN"),
                                tableTmp,
                                OTCmlHelper.GetSQLDataDtEnabled(Cs, "cboMk", this.m_ProcessInfo.DtBusiness));

                    }
                    else
                    {
                        /*2nd partie de la requête => activité habituelle (Référentiel "Activité sur les marchés") */
                        // RD 20171024 [23511] Use CBOMARKET.IDA_CSSCUSTODIAN
                        query = @"select cboMk.IDA_CBO, cboMk.IDB_CBO, m.IDM, m.FIXML_SECURITYEXCHANGE, a.IDA, {0}
                        from dbo.CBOMARKET cboMk
                        inner join dbo.BOOK b on (b.IDB = cboMk.IDB_CBO) and (b.IDA_ENTITY = @IDA_ENTITY)
                        inner join dbo.VW_ENTITYMARKET_ACTIVITY em on (em.IDA = @IDA_ENTITY) and (em.DTENTITY = @DTBUSINESS) and (em.IDM = cboMk.IDM) and (em.IDA_CSSCUSTODIAN = isnull(cboMk.IDA_CSSCUSTODIAN, em.IDA_CSSCUSTODIAN))
                        inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = em.IDM)
                        inner join dbo.ACTOR a on (a.IDA = em.IDA_CSSCUSTODIAN)
                        where (cboMk.IDA_CBO = @IDA) and (cboMk.IDB_CBO = @IDB) and {1}";

                        query = StrFunc.AppendFormat(query,
                                OTCmlHelper.GetACTORXMLId(Cs, "a", "XMLID_CSSCUSTODIAN"),
                                OTCmlHelper.GetSQLDataDtEnabled(Cs, "cboMk", this.m_ProcessInfo.DtBusiness));
                    }

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.DTBUSINESS), this.m_ProcessInfo.DtBusiness);
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDA_ENTITY), this.m_ProcessInfo.Entity);
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDA), pCBTradeInfo.Ida);
                    dp.Add(DataParameter.GetParameter(Cs, DataParameter.ParameterEnum.IDB), pCBTradeInfo.Idb);

                    QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

                    ds = DataHelper.ExecuteDataset(dbConnection, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                }
            }
            catch
            {
                throw;
            }
            if (ds.Tables[0].Rows.Count > 0)
            {
                Int32[] idacssCustodianEODValid = (from item in m_CBHierarchy.cssCustodianEODValid
                                                   select item.OTCmlId).ToArray();

                DataTable dt = ds.Tables[0];
                IEnumerable<DataRow> dr = dt.Rows.Cast<DataRow>();

                CssCustodianStatus[] cssCustodianStatus = (from grpItem in dr.GroupBy
                                                           (x => new { idA = Convert.ToInt32(x["IDA"]), xmlId = Convert.ToString(x["XMLID_CSSCUSTODIAN"]) })
                                                           select new CssCustodianStatus()
                                                           {
                                                               idACssCustodian = grpItem.Key.idA,
                                                               cssCustodianHref = grpItem.Key.xmlId,
                                                               status = ArrFunc.ExistInArray(idacssCustodianEODValid.Cast<IComparable>().ToArray(), (IComparable)(grpItem.Key.idA)) ?
                                                                          PerformedSatusEnum.Performed : PerformedSatusEnum.Unperformed
                                                           }).ToArray();

                foreach (CssCustodianStatus itemMaster in cssCustodianStatus)
                {
                    IEnumerable<ExchangeStatus> exchangeStatus = from item in
                                                                     dr.Where(x => Convert.ToInt32(x["IDA"]) == itemMaster.idACssCustodian)
                                                                 select new ExchangeStatus()
                                                                 {
                                                                     OTCmlId = Convert.ToInt32(item["IDM"]),
                                                                     Exch = Convert.ToString(item["FIXML_SECURITYEXCHANGE"]),
                                                                     status = itemMaster.status
                                                                 };


                    itemMaster.exchStatus = exchangeStatus.ToArray();
                };

                if (ArrFunc.IsFilled(cssCustodianStatus))
                {
                    pCBTradeInfo.endOfDayStatus = new EndOfDayStatus
                    {
                        cssCustodianStatus = cssCustodianStatus
                    };
                }
            }
        }

        /// <summary>
        ///  Creation d'une table Temporaire
        ///  <para>Retourne le nom de table</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTableId"></param>
        /// <returns></returns>
        /// FI 20170316 [22950] Add
        /// FI 20170421 [XXXXX] Modify
        /// EG 20230227 [22151][22152] Refactoring La table est créée (+ création PK) si elle n'existe pas ou purgée dans le cas contraire
        private string CreateCBTradeWorkTable(string pCS, string pTableId)
        {
            string tblCBTRADE_Work = "CBTRADE" + "_" + pTableId + "_W";
            if (DataHelper.IsExistTable(pCS, tblCBTRADE_Work))
            {
                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, $"truncate table dbo.{tblCBTRADE_Work}", null);
            }
            else
            {
                DataHelper.CreateTableAsSelect(pCS, Cst.OTCml_TBL.CBTRADE_MODEL.ToString(), tblCBTRADE_Work);
                DataHelper.ExecuteNonQuery(Cs, CommandType.Text, $@"alter table dbo.{tblCBTRADE_Work} add constraint PK_{tblCBTRADE_Work} primary key(""IDT"")");
            }
            return tblCBTRADE_Work;
        }


        /// <summary>
        /// Indique si le CashBalance calculé a changé par rapport à celui présent dans la base de données
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdT"></param>
        /// <param name="pNewCBDataDoc"></param>
        /// <returns></returns>
        // PM 20190701 [24761] New
        private bool IsCashBalanceChanged(string pCS, IDbTransaction pDbTransaction, int pIdT, DataDocumentContainer pNewCBDataDoc)
        {
            m_TradeLibrary = new EFS_TradeLibrary(pCS, pDbTransaction, pIdT);
            bool isChanged = false == CBDataDocument.IsSameDataDocument(m_TradeLibrary.DataDocument, pNewCBDataDoc);
            return isChanged;
        }
        #endregion
    }

    /// <summary>
    ///  couple {ProcessStateTools.StatusEnum, DtEnd} 
    /// </summary>
    /// FI 20141126 [20526] Add class
    internal class ProcessStatus
    {
        /// <summary>
        ///  Statut d'un traitement
        /// </summary>
        public ProcessStateTools.StatusEnum status;

        /// <summary>
        ///  Date Fin d'un traitement
        /// </summary>
        public DateTime dtEnd;
    }
}
