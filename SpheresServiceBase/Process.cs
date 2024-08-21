#region Using
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Linq;
//
using EFS;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.ApplicationBlocks.Data;
using EFS.EFSTools;

using EFS.OTCmlStatus;
using EFS.SpheresService;
using EFS.Tuning;

using FpML.Enum;
using EfsML.Enum;
using FpML.Interface;
using EfsML.Business;
using EfsML.Interface;
#endregion Using

namespace EFS.Process
{
    #region ProcessBase
    /// <summary>
    /// 
    /// </summary>
    public abstract class ProcessBase
    {
        #region Enum Process (Call/Slave)
        /// <summary>
        /// 
        /// </summary>
        public enum ProcessCallEnum
        {
            /// <summary>
            /// Le process est maître (C'est le traitement principal)
            /// </summary>
            Master,
            /// <summary>
            /// Le process est esclave d'un process principal (C'est un sous traitement)
            /// </summary>
            Slave
        }
        /// <summary>
        /// 
        /// </summary>
        public enum ProcessSlaveEnum
        {
            None,
            Start,
            Execute,
            End,
        }
        #endregion Enum Process (Call/Slave)
        #region Members
#if DEBUG
        /// <summary>
        /// Outils de diagnostique en mode Debug uniquement
        /// </summary>
        protected DiagnosticDebug diagnosticDebug = new DiagnosticDebug(false);
#endif
        /// <summary>
        /// Message qui active le process
        /// </summary>
        private MQueueBase _mQueue;

        /// <summary>
        /// 
        /// </summary>
        private IDbConnection _connection;

        /// <summary>
        /// Représente l'instance qui a lance le process
        /// </summary>
        private AppInstanceService _appInstance;

        /// <summary>
        /// 
        /// </summary>
        private Boolean _isModeSimul;

        /// <summary>
        /// 
        /// </summary>
        private Boolean _isProcessObserver;
        /// <summary>
        /// 
        /// </summary>
        private Boolean _isCreatedByNormalizedMessageFactory;

        /// <summary>
        /// 
        /// </summary>
        protected DataSet _dsDatas;

        /// <summary>
        /// 
        /// </summary>
        private int _currentId;

        /// <summary>
        /// 
        /// </summary>
        //private Request _request;

        /// <summary>
        /// Permet l'écriture dans de Tracker
        /// </summary>
        private Tracker _tracker;

        /// <summary>
        /// Permet l'écriture dans les  Logs
        /// </summary>
        private ProcessLog _processLog;

        /// <summary>
        /// Représente le niveau de log
        /// </summary>
        private ErrorManager.DetailEnum _logDetailEnum;

        /// <summary>
        /// Liste des objets lockés par le process
        /// </summary>
        private ArrayList _lockObject;

        /// <summary>
        /// Représente les paamétrages effectués dans PROCESSTUNING pour le process
        /// </summary>
        private ProcessTuning _processTuning;

        /// <summary>
        /// Représente le code retour du process
        /// </summary>
        private ProcessState _processState;

        /// <summary>
        /// permet l'usage de SESSIONRESTICT pour réduire les jeux de résultat SQL
        /// <para></para>
        /// </summary>
        private SqlSessionRestrict _sqlSessionRestrict;

        /// <summary>
        /// Représente la license
        /// </summary>
        private License _license;

        /// <summary>
        /// Indique si le process est arrété
        /// </summary>
        private bool _isProcessEnded;

        /// <summary>
        /// Cache quotation
        /// </summary>
        /// PM 20130104 : Déplacé de ProcessTradeBase vers TradeBase
        protected ProcessCacheContainer m_ProcessCacheContainer;

        /// <summary>
        /// Active/désactive la mise à jour du statut lorsqu'une Spheres® alimente le journal des logs avec une erreur ou un warning
        /// </summary>
        /// FI 20131213 [19347] add property
        public Boolean isProcessStateStatutUpdOnErrWarning
        {
            get;
            set;
        }

        /// <summary>
        /// Active/désactive la mise à jour automatique du log dans la base de données
        /// </summary>
        /// FI 20131213 [19347] add property
        public Boolean isAutoWriteLogDetail
        {
            get;
            set;
        }

        #endregion Member
        #region Accessors
        /// <summary>
        /// Obtient le service qui lance le process
        /// </summary>
        public AppInstanceService appInstance
        {
            get { return _appInstance; }
            set { _appInstance = value; }
        }
        /// <summary>
        /// Obtient ou définit l'Id sur lequel s'applique le traitement
        /// </summary>
        public int currentId
        {
            get { return _currentId; }
            set { _currentId = value; }
        }
        /// <summary>
        /// Liste des lock posés pendant le process 
        /// </summary>
        public ArrayList lockObject
        {
            set { _lockObject = value; }
            get { return _lockObject; }
        }
        /// <summary>
        /// 
        /// </summary>
        public License License
        {
            get { return _license; }
        }
        /// <summary>
        /// Indique si le process est arrêté
        /// </summary>
        public bool IsProcessEnded
        {
            set { _isProcessEnded = value; }
            get { return _isProcessEnded; }
        }
        /// <summary>
        /// 
        /// </summary>
        public MQueueBase mQueue
        {
            set { _mQueue = value; }
            get { return _mQueue; }
        }
        /// <summary>
        /// 
        /// </summary>
        public IDbConnection Connection
        {
            get { return _connection; }
            set { _connection = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcessTuning processTuning
        {
            set { _processTuning = value; }
            get { return _processTuning; }
        }

        /// <summary>
        /// 
        /// </summary>
        public Tracker tracker
        {
            set { _tracker = value; }
            get { return _tracker; }
        }

        /// <summary>
        /// Permet l'écriture dans les tables de logs (PROCESS_L et PROCESSDET_L)
        /// </summary>
        public ProcessLog processLog
        {
            set { _processLog = value; }
            get { return _processLog; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ErrorManager.DetailEnum logDetailEnum
        {
            get { return _logDetailEnum; }
            set { _logDetailEnum = value; }
        }

        /// <summary>
        ///  Obtient true si traitement en simulation
        /// </summary>
        public bool isProcessSimul
        {
            get { return _isModeSimul; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsProcessObserver
        {
            get { return _isProcessObserver; }
        }
        #region IsCreatedByNormalizedMessageFactory
        /// <summary>
        /// Indique que le message provient de la fabrique de message normalisé
        /// </summary>
        public bool IsCreatedByNormalizedMessageFactory
        {
            get { return _isCreatedByNormalizedMessageFactory; }
        }
        #endregion IsCreatedByNormalizedMessageFactory

        /// <summary>
        /// 
        /// </summary>
        public bool processTuningSpecified
        {
            get { return (null != _processTuning) && _processTuning.DrSpecified; }
        }

        /// <summary>
        /// Obteint la cs issu de message queue
        /// </summary>
        public string cs
        {
            get { return _mQueue.ConnectionString; }
        }
        /// <summary>
        /// Obtient le gestionnaire dédié à l'alimentation de la table SESSIONRESTRICT
        /// </summary>
        public SqlSessionRestrict sqlSessionRestrict
        {
            get { return _sqlSessionRestrict; }
        }

        public ProcessState ProcessState
        {
            get { return _processState; }
            set { _processState = value; }
        }
        /// <summary>
        /// Obtient ou définit processState.CodeReturn
        /// </summary>
        public Cst.ErrLevel CodeReturn
        {
            set { _processState.CodeReturn = value; }
            get { return _processState.CodeReturn; }
        }
        
        /// <summary>
        /// Obtient processState.LastSpheresException
        /// </summary>
        public SpheresException SpheresException
        {
            set { _processState.LastSpheresException = value; }
        }

        /// <summary>
        /// <para>
        /// Obtient true si le traitement s'applique donnée par donnée ( Exemple : Trade par Trade pour la Génération des Events, ...)
        /// </para>
        /// <para>
        /// Obtient false si le traitement s'applique pour un ensemble des données en même temps ( Exemple: Génération des avis d'opéré) 
        /// </para>
        /// </summary>
        protected virtual bool IsMonoDataProcess
        {
            get { return true; }
        }

        /// <summary>
        /// Obtient le type d'identification associé au currentId
        /// <para>Exemple lorsque currentId est un trade alors cette propertie doit retourner TRADE</para>
        /// </summary>
        protected virtual string dataIdent
        {
            get { return "N/A"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual TypeLockEnum dataTypeLock
        {
            get { return TypeLockEnum.NA; }
        }

        /// <summary>
        /// Obtient true si le process post un message
        /// <para>L'envoi effectif du message sera fonction de processTuning</para>
        /// </summary>
        protected virtual bool isProcessSendMessage
        {
            get { return true; }
        }

        /// <summary>
        /// Transaction en cours sur le process appelant (en mode SLAVE)
        /// </summary>
        public IDbTransaction slaveDbTransaction
        { get; set; }

        /// <summary>
        /// permet d'établir si le process est enfant d'un process appelant ou si le process est un process maître
        /// <para>Lorsque le porcess est enfant d'an autre process, il n'effectue pas de mise à jour dans PROCESS_L</para>
        /// </summary>
        public ProcessCallEnum processCall
        { get; set; }

        /// <summary>
        /// permet d'établir si le mode du process enfant d'un process appelant (Start, Execute, End)
        /// </summary>
        public ProcessSlaveEnum processSlave
        { get; set; }

        /// <summary>
        /// permet de definir si l'envoi d'un message à un autre service est autorisé après traitement (ProcessTuning)
        /// Plus généralement utilisé à false pour les process enfants.
        /// </summary>
        public bool IsSendMessage_PostProcess
        { get; set; }

        /// <summary>
        /// Block the update of the tracker, when true any update command to the tracker will be rejected
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>To set externally,
        /// to restore to the default value when the tracker update must be reactivated </remarks>
        public bool BlockTrackerUpdate
        { get; set; }

        /// <summary>
        /// DataSetTrade Call mode (true = Trade table only; false = all table (default))
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>Used by API slave call</remarks>
        public bool IsDataSetTrade_AllTable
        { get; set; }


        /// <summary>
        /// Remplace IsDataSetTrade_AllTable (Enumerateur de table à charger)
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>Used by API slave call</remarks>
        public EfsML.TradeTableEnum TradeTable
        { get; set; }

        /// <summary>
        /// NOLock CurrentId (Can be equal TRUE when processCall = Slave)
        /// </summary>
        /// <value>false by default</value>
        /// <remarks>To set externally,
        /// to restore to the default value when the tracker update must be reactivated </remarks>
        public bool NoLockCurrentId
        { get; set; }

        /// <summary>
        /// Stockage en cache des différents éléments utilisables dans un process 
        /// </summary>
        public ProcessCacheContainer ProcessCacheContainer
        {
            get { return m_ProcessCacheContainer; }
            set { m_ProcessCacheContainer = value; }
        }

        /// <summary>
        /// Dictionnaire du cache des EntityMarket
        /// </summary>
        public Dictionary<int, IPosKeepingMarket> EntityMarketCache
        {
            get { return ProcessCacheContainer.EntityMarketCache; }
            set { ProcessCacheContainer.EntityMarketCache = value; }
        }
        /// <summary>
        /// Dictionnaire du cache des cotations
        /// </summary>
        /// EG 20130528 Change Key int to Pair(Cst.UnderlyingAsset, int)
        public Dictionary<Pair<Cst.UnderlyingAsset, int>, Dictionary<Pair<QuotationSideEnum, DateTime>, Quote>> QuoteCache
        {
            get { return ProcessCacheContainer.QuoteCache; }
            set { ProcessCacheContainer.QuoteCache = value; }
        }

        /// <summary>
        /// Dictionnaire du cache des jours ouvrés sur Date (PRECEEDING / NONE / FOLLOWING)
        /// </summary>
        public Dictionary<DateTime, Dictionary<Pair<BusinessDayConventionEnum, string>, DateTime>> BusinessDateCache
        {
            get { return ProcessCacheContainer.BusinessDateCache; }
            set { ProcessCacheContainer.BusinessDateCache = value; }
        }


        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public ProcessBase()
        {
            processCall = ProcessCallEnum.Master;
            IsSendMessage_PostProcess = true;
            // FI 20131213 [19347] => comportement par défaut
            isProcessStateStatutUpdOnErrWarning = true;
            isAutoWriteLogDetail = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public ProcessBase(MQueueBase pMQueue, AppInstanceService pAppInstance)
        {
#if DEBUG
            diagnosticDebug.Start("ElapsedTime");
#endif

            // FI 20131213 [19347] => comportement par défaut
            isProcessStateStatutUpdOnErrWarning = true;
            isAutoWriteLogDetail = true;
            
            // PL/FDA 20120202 Ligne suivante permettant pour des tests de forcer le cache OFF
            //_mQueue.ConnectionString = CSTools.SetCacheOff(_mQueue.ConnectionString);
            //
            processCall = ProcessCallEnum.Master;
            //
            _mQueue = pMQueue;
            //FI 20120125 
            //Purge du cache avant chaque traitement d'un message
            //=>Les référentiels ont potentiellement changés depuis le dernier traitement d'un message
            //Anciennement Le cache n'était jamais activé
            //Je trouve qu'il est dommage de s'en privé
            //=> Ds un même traitement on pourrait éventuellement chargé plusieurs fois la même donnée, 
            //il est intéressant de pouvoir profiter du cache
            //FI 20120703 [17979] La purge du cache n'est plus effectuée systématiquement à chaque reception d'un message Queue
            //Mise en commenatire de la ligne 
            //(voir désormais la méthode InitializeQueryCache) 
            //DataHelper.queryCache.Clear();
            //
            //FI 20120124 Chgt des enums dans un cache 
            //Les enums sont valides pendants 5 minutes, ce n'est pas rechargé à chaque traitement d'un message

            //FI 20120712 Les enums ne sont plus chargés ici mais après avoir initialiser le cache
            //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(CSTools.SetCacheOff(_mQueue.ConnectionString));
            //
            //Clone de appInstance pour affecter un nouvel SessionId
            _appInstance = (AppInstanceService)pAppInstance.Clone();
            _appInstance.SetNewSessionId();
            _sqlSessionRestrict = new SqlSessionRestrict(cs, appInstance);
            _processState = new ProcessState(ProcessStateTools.StatusUnknownEnum, ProcessStateTools.CodeReturnSuccessEnum);
            _logDetailEnum = ErrorManager.DetailEnum.DEFAULT;
            //
            if (_mQueue.parametersSpecified)
            {
                _isModeSimul = mQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISSIMUL);
                _isProcessObserver = mQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISOBSERVER);
                // EG 20121109 Demande via SCHEDULER
                _isCreatedByNormalizedMessageFactory = mQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISCREATEDBY_NORMMSGFACTORY);
                _isCreatedByNormalizedMessageFactory &= (false == mQueue.idSpecified) || (0 == mQueue.id);
            }
        }
        #endregion Constructors
        #region Methods
        #region SetTradeStatus
        /// <summary>
        /// Update status on the trade (TRADESTSYS, TRADESTCHECK, TRADESTMATCH)
        /// </summary>
        /// <param name="pStatusProcess"></param>
        /// <returns></returns>
        protected virtual bool SetTradeStatus(int pIdT, ProcessStateTools.StatusEnum pStatusProcess)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            if (processTuningSpecified)
            {
                //Write Trade Status from PROCESSTUNING for OutputTradeSuccess/OutputTradeError
                switch (pStatusProcess)
                {
                    case ProcessStateTools.StatusEnum.SUCCESS:
                        errLevel = processTuning.WriteStatus(cs, null, TuningOutputTypeEnum.OTS, pIdT, appInstance.IdA);
                        break;
                    case ProcessStateTools.StatusEnum.ERROR:
                        errLevel = processTuning.WriteStatus(cs, null, TuningOutputTypeEnum.OTE, pIdT, appInstance.IdA);
                        break;
                }
            }
            return (errLevel == Cst.ErrLevel.SUCCESS);

        }
        #endregion SetTradeStatus
        #region ScanCompatibility_Trade
        /// <summary>
        /// ScanCompatibility: Scan si le trade est compatible avec le référentiel PROCESSTUNING
        ///   Step 1: On vérifie s'il existe un paramétrage dans PROCESSTUNING
        ///   Step 2: Récupère les statuts en vigueur sur le Trade
        ///   Step 3: On vérifie si ceux-ci sont compatibles avec le paramétrage en vigueur dans PROCESSTUNING
        ///   Step 4: On vérifie (en DUR) s'il s'agit: d'une ALLOC (ETD) et du service SIGEN; afin d'ignorer la demande
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel ScanCompatibility_Trade(int pIdT)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //Step 1
            if (processTuningSpecified)
            {
                //Step 2
                ret = Cst.ErrLevel.TUNING_UNMATCH;
                TradeStatus tradeStatus = new TradeStatus();
                tradeStatus.Initialize(cs, pIdT);

                //Step 3
                string msgControl = string.Empty;
                ret = processTuning.ScanTradeCompatibility(cs, pIdT, tradeStatus, ref msgControl);
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return ret;
        }
        #endregion ScanCompatibility_Trade
        #region ScanCompatibility_Event
        /// <summary>
        /// ScanCompatibility: Scan si le trade est compatible avec le référentiel PROCESSTUNING
        /// <para>
        /// Step 1: On vérifie s'il existe un paramétrage dans PROCESSTUNING
        /// </para>  
        /// <para>
        ///   Step 2: Récupère les statuts en vigueur sur l'Event
        /// </para>  
        /// <para>
        ///   Step 3: On vérifie si ceux-ci sont compatibles avec le paramétrage en vigueur dans PROCESSTUNING
        /// </para>  
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel ScanCompatibility_Event(int pIde)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            string msgControl = string.Empty;
            if (processTuningSpecified)
            {
                EventStatus eventStatus = new EventStatus();
                if (eventStatus.Initialize(cs, pIde))
                    ret = processTuning.ScanEventCompatibility(cs, pIde, eventStatus, ref msgControl);
                else
                    //Ce cas n'est sensé jamais arrivé...
                    ret = Cst.ErrLevel.TUNING_UNMATCH;
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return ret;
        }
        #endregion ScanCompatibility_Event
        #region LockCurrentObjectId
        /// <summary>
        /// Pose un Lock exclusif
        /// </summary>
        /// <returns></returns>
        protected Cst.ErrLevel LockCurrentObjectId()
        {
            return LockCurrentObjectId(LockTools.Exclusive);
        }
        
        /// <summary>
        /// Pose un Lock
        /// </summary>
        /// <param name="pLockMode"></param>
        /// <returns></returns>
        protected Cst.ErrLevel LockCurrentObjectId(string pLockMode)
        {
            return LockObjectId(dataTypeLock, currentId, mQueue.identifier, mQueue.ProcessType.ToString(), pLockMode);
        }
        #endregion LockCurrentObjectId
        #region LockObjectId
        /// <summary>
        /// Pose un lock sur un objet
        /// <para>Alimente la liste lockObject en parallele</para>
        /// <para>Retourne Cst.ErrLevel.LOCKUNSUCCESSFUL si lock non posé</para>
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pId"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pAction"></param>
        /// <param name="pLockMode"></param>
        /// <returns></returns>
        /// FI 20141125 [20230] Modify
        // EG 20151123 ObjectId = string
        public Cst.ErrLevel LockObjectId(TypeLockEnum pTypeLock, int pId, string pIdentifier, string pAction, string pLockMode)
        {
            return LockObjectId(pTypeLock, pId.ToString(), pIdentifier, pAction, pLockMode);
        }
        public Cst.ErrLevel LockObjectId(TypeLockEnum pTypeLock, string pId, string pIdentifier, string pAction, string pLockMode)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (TypeLockEnum.NA != pTypeLock)
            {
                // EG 20151123 ObjectId = string
                LockObject lockObject = new LockObject(pTypeLock, pId, pIdentifier, pLockMode);
                Lock lckExisting = null;
                // RD 20120809 [18070] Optimisation de la compta / Utilisation de processLog.GetDate()
                Lock lck = new Lock(cs, lockObject, appInstance, pAction, processLog.GetDate());
                if (false == LockTools.LockMode2(lck, out lckExisting))
                {
                    if (null != lckExisting)
                    {
                        // EG 20130620 Génération du message avec DetailEnum = DEFAULT
                        // FI 20141125 [20230] Mise en place d'un message dans le log avec status WARNING, isProcessStateStatutUpdOnErrWarning est postionné à false;
                        // Avec le code précédent Spheres® terminait une erreur, et le message était reposté 
                        //    => Au final, une fois le lock levé, le traitement était en succès mais le tracker en erreur
                        //ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, lckExisting.ToString());
                        bool savBehavior = isProcessStateStatutUpdOnErrWarning;
                        isProcessStateStatutUpdOnErrWarning = false;
                        ProcessLogAddDetail(ProcessStateTools.StatusEnum.WARNING, ErrorManager.DetailEnum.DEFAULT, lckExisting.ToString());
                        isProcessStateStatutUpdOnErrWarning = savBehavior;
                    }
                    lockObject = null; // Use For Not Unlock
                    ret = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                }
                if (ProcessStateTools.IsCodeReturnSuccess(ret))
                {
                    if (ArrFunc.IsEmpty(_lockObject))
                        _lockObject = new ArrayList();
                    _lockObject.Add(lockObject);
                }
            }
            
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return ret;
        }
        #endregion LockObjectId
        #region UnLockCurrentObjectId
        /// <summary>
        /// 
        /// </summary>
        protected void UnLockCurrentObjectId()
        {
            UnLockObjectId(_currentId);
        }
        #endregion UnLockCurrentObjectId
        #region UnLockObjectId
        /// <summary>
        /// ?????? 
        /// </summary>
        /// <param name="pId"></param>
        public void UnLockObjectId(int pId)
        {
            if (ArrFunc.IsFilled(_lockObject))
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

                foreach (LockObject item in _lockObject)
                {
                    if ((null != item) && ((false == IsMonoDataProcess) || (item.ObjectId == pId.ToString() )))
                        LockTools.UnLock(cs, item, appInstance.SessionId);
                }
                _lockObject.Clear();
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
        }
        #endregion UnLockObjectId
        #region LockElement
        /// <summary>
        /// Lock Exclusif
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pId"></param>
        /// <param name="pIdentifier"></param>
        /// <returns></returns>
        public LockObject LockElement(TypeLockEnum pTypeLock, int pId, string pIdentifier)
        {
            return LockElement(pTypeLock, pId, pIdentifier, true, LockTools.Exclusive);
        }
        /// <summary>
        /// Lock 
        /// </summary>
        /// <param name="pTypeLock"></param>
        /// <param name="pId"></param>
        /// <param name="pIdentifier"></param>
        /// <param name="pIsControlSession"></param>
        /// <param name="pLockMode"></param>
        /// <returns></returns>
        // EG 20151102 [21465] Refactoring
        public LockObject LockElement(TypeLockEnum pTypeLock, int pId, string pIdentifier, bool pIsControlSession, string pLockMode)
        {
            bool isLockBySameProcess = false;
            // EG 20151123 ObjectId = string
            LockObject lockObject = new LockObject(pTypeLock, pId, pIdentifier, pLockMode);
            if (null != lockObject)
            {
                Lock lckExisting = null;
                // EG 20151123 RequestType instead of mQueue.ProcessType.ToString()
                String action = mQueue.ProcessType.ToString();
                if (mQueue is PosKeepingRequestMQueue)
                    action = (mQueue as PosKeepingRequestMQueue).requestType.ToString();
                Lock lck = new Lock(cs, lockObject, appInstance, action);
                if (false == LockTools.LockMode1(lck, out lckExisting))
                {

                    if (null != lckExisting)
                    {
                        if (pIsControlSession)
                            isLockBySameProcess = (lckExisting.AppInstance.SessionId == appInstance.SessionId);
                        if (false == isLockBySameProcess)
                            ProcessLogAddDetail(lckExisting.ToString());
                    }
                    if (false == isLockBySameProcess)
                        lockObject = null; // Use For Not Unlock
                }
            }
            return lockObject;
        }
        #endregion LockElement
        #region Dispose
        /// <summary>
        /// UnLockSession, UnSetRestriction
        /// </summary>
        /// FI 20120831 
        protected void Dispose()
        {
            
            LockTools.UnLockSession(cs, appInstance.SessionId);
            
            //FI 20120831 use  _sqlSessionRestrict.UnSetRestriction à la place de SqlSessionRestrict.Dispose
            _sqlSessionRestrict.UnSetRestriction();
            //SqlSessionRestrict.Dispose(cs, appInstance);
        }
        #endregion Dispose
        #region PostProcess_SendMessage
        //protected bool PostProcess_SendMessage()
        protected int PostProcess_SendMessage()
        {
            int _nbPostedSubMsg = 0;
            //bool isMessageSended = false;
            // RD 20091221 [16803]
            if (processTuningSpecified)
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

                Cst.ProcessTypeEnum[] postProcess_SendMessageTypeArray = new Cst.ProcessTypeEnum[3] { Cst.ProcessTypeEnum.NA, Cst.ProcessTypeEnum.NA, Cst.ProcessTypeEnum.NA };

                //Get SendMessage type from PROCESSTUNING for OutputTradeSuccess/OutputTradeError
                bool isExistSendMessage = false;
                switch (_processState.Status)
                {
                    case ProcessStateTools.StatusEnum.SUCCESS:
                        isExistSendMessage = processTuning.Get_SendMessage(TuningOutputTypeEnum.OTS, ref postProcess_SendMessageTypeArray);
                        break;
                    case ProcessStateTools.StatusEnum.ERROR:
                        isExistSendMessage = processTuning.Get_SendMessage(TuningOutputTypeEnum.OTE, ref postProcess_SendMessageTypeArray);
                        break;
                }

                if (isExistSendMessage) //PL 20120824 Add if() for tuning
                {
                    foreach (Cst.ProcessTypeEnum postProcess_SendMessageType in postProcess_SendMessageTypeArray)
                    {
                        bool isAvailable = (Cst.ProcessTypeEnum.NA != postProcess_SendMessageType);
                        // EG 20120203 Add 
                        isAvailable &= (ProcessCallEnum.Master == processCall) || IsSendMessage_PostProcess;

                        // EG 20111003 Pas d'envoi de message de type POSKEEPENTRY si GPRODUCT <> ETD / FUT plus tard SEC
                        if (Cst.Process.IsPosKeepingEntry(postProcess_SendMessageType))
                        {
                            string _product = mQueue.GetStringValueIdInfoByKey("GPRODUCT");
                            isAvailable &= ((!string.IsNullOrEmpty(_product)) && (Cst.ProductGProduct_FUT == _product));
                        }

                        if (isAvailable)
                        {
                            MQueueSendInfo sendInfo = GetMqueueSendInfo(postProcess_SendMessageType);

                            if (sendInfo.isInfoValid)
                            {
                                MQueueBase mQueueTarget = MQueueTools.GetMQueueByProcess(postProcess_SendMessageType);
                                //Add Parameters
                                MQueueparameters mqParameters = new MQueueparameters();
                                if (mQueue.parametersSpecified)
                                {
                                    for (int i = 0; i < mQueue.parameters.Count; i++)
                                    {
                                        if (mQueueTarget.IsExistParameter(mQueue.parameters[i].id))
                                            mqParameters.Add(mQueue.parameters[i]);
                                    }
                                }
                                //Transfert de l'Id vers le process suivant 
                                //uniquement s'il est un IDT (TRADE) 
                                //et que le process suivant s'attend à recevoir en Id un IDT (TRADE)
                                int mQueueId = 0;
                                MQueueIdInfo mQueueIdInfoTarget = new MQueueIdInfo(mQueueId);
                                if (mQueue.IsIdT)
                                {
                                    if (mQueueTarget.IsIdT)
                                    {
                                        mQueueIdInfoTarget.id = _currentId;
                                        if (mQueue.idInfoSpecified)
                                            mQueueIdInfoTarget.idInfos = mQueue.idInfo.idInfos;
                                    }
                                    else
                                    {
                                        if (mQueueTarget.IsExistParameter(MQueueBase.PARAM_IDT))
                                        {
                                            // IDT est transféré comme paramètre au service suivant
                                            // A ce dernier de le considérer ou non
                                            MQueueparameter mqp = new MQueueparameter(MQueueBase.PARAM_IDT, TypeData.TypeDataEnum.integer);
                                            mqp.Value = _currentId.ToString();
                                            mqParameters.Add(mqp);
                                        }
                                    }
                                }
                                //
                                if (Cst.Process.IsConfirmationMsgGenIO(postProcess_SendMessageType))
                                {
                                    MQueueparameter mqp = new MQueueparameter();
                                    mqp.id = MQueueBase.PARAM_ISWITHIO;
                                    mqp.dataType = TypeData.TypeDataEnum.@bool;
                                    mqp.SetValue(true);
                                    mqParameters.Add(mqp);
                                }

                                MQueueTools.Send(postProcess_SendMessageType, cs, mQueueIdInfoTarget, mqParameters, mQueue.header.requester, sendInfo);
                                ProcessLogAddDetail(ProcessStateTools.StatusNoneEnum, ErrorManager.DetailEnum.DEFAULT, 1, "LOG-00301",
                                    Ressource.GetString(postProcess_SendMessageType.ToString()), postProcess_SendMessageType.ToString());

                                _nbPostedSubMsg++;
                                //isMessageSended = true;
                            }
                        }
                    }
                }
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            }
            return _nbPostedSubMsg;
            //return isMessageSended;
        }
        #endregion PostProcess_SendMessage

        /// <summary>
        /// Renvoi le status du log à considérer en fonction du code retourné {pCodeReturn}
        /// </summary>
        /// <param name="pCodeReturn"></param>
        /// <param name="pLevel"></param>
        /// <param name="pStatus"></param>
        // EG 20150306 [POC-BERKELEY] : Add FAILUREWARNING
        public ProcessStateTools.StatusEnum GetProcessState(Cst.ErrLevel pCodeReturn)
        {
            ProcessStateTools.StatusEnum ret = ProcessStateTools.StatusErrorEnum;
            switch (pCodeReturn)
            {
                case Cst.ErrLevel.SUCCESS:
                    ret = ProcessStateTools.StatusSuccessEnum;
                    break;
                case Cst.ErrLevel.TUNING_IGNOREFORCED://Operation Ignorée en DUR => Mise in None (avant Success)
                case Cst.ErrLevel.TUNING_IGNORE:      //Operation Ignorée       => Mise in None (avant Success)
                case Cst.ErrLevel.NOTHINGTODO:
                case Cst.ErrLevel.ENTITYMARKET_UNMANAGED:
                    ret = ProcessStateTools.StatusNoneEnum;
                    break;
                case Cst.ErrLevel.LOCKUNSUCCESSFUL: //Operation non traitée   => Remise in Queue
                case Cst.ErrLevel.TUNING_UNMATCH:   //Operation non traitée   => Remise in Queue
                case Cst.ErrLevel.DEADLOCK:         //Deadlock                => Remise in Queue
                    ret = ProcessStateTools.StatusPendingEnum;
                    break;
                case Cst.ErrLevel.ACCESDENIED:      //Licence incorrect
                    ret = ProcessStateTools.StatusErrorEnum;
                    break;
                case Cst.ErrLevel.TIMEOUT:          //Timeout                => Remise in Queue 
                    // EG 20130724  On ne repostera pas le message dans le cas de SpheresIO avec processLog instancié
                    if (appInstance.AppName.ToLower().Contains("spheresio") && (null!=_processLog))
                        ret = ProcessStateTools.StatusErrorEnum;
                    else
                        ret = ProcessStateTools.StatusPendingEnum;
                    break;
                case Cst.ErrLevel.FAILUREWARNING:
                    // EG 20150305 New
                    ret = ProcessStateTools.StatusWarningEnum;
                    break;
                default:
                    ret = ProcessStateTools.StatusErrorEnum;
                    break;
            }
            return ret;
        }


        #region SetProcessState
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        protected void SetProcessState(ProcessStateTools.StatusEnum pStatus)
        {
            if (false == ProcessStateTools.IsStatusErrorWarning(_processState.Status))
                _processState.Status = pStatus;
            if (pStatus != ProcessStateTools.StatusEnum.PROGRESS)
                _processState.CurrentStatus = pStatus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pStatus"></param>
        /// <param name="pCodeReturn"></param>
        protected void SetProcessState(ProcessStateTools.StatusEnum pStatus, Cst.ErrLevel pCodeReturn)
        {
            if (false == ProcessStateTools.IsStatusErrorWarning(_processState.Status))
            {
                _processState.Status = pStatus;
                _processState.CodeReturn = pCodeReturn;
            }
            if (pStatus != ProcessStateTools.StatusEnum.PROGRESS)
                _processState.CurrentStatus = pStatus;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessState"></param>
        public void SetProcessState(ProcessState pProcessState)
        {
            SetProcessState(pProcessState.Status, pProcessState.CodeReturn);
        }
        #endregion SetProcessState
        #region SelectDatas
        /// <summary>
        /// 
        /// </summary>
        protected virtual void SelectDatas()
        {
            // Method use for Load _dsDatas
        }
        #endregion SelectDatas
        #region ProcessStart
        /// <summary>
        /// Démarre le process
        /// </summary>
        /// FI 20141125 [20230] Modify
        public virtual void ProcessStart()
        {
            // Multidata Generation de plusieurs messages (Aucun Log pour ce traitement)
            // EG 20121116 Add ProcessTerminate, ProcessFinalize if SelectDatas return 0

            // RD 20140123 [19526] 
            // If process don't manage message queue without Id (Example: I/O)
            // - Set new tracker after Process Ended 
            // - Add ProcessTerminate, ProcessFinalize 
            _isProcessEnded = false;
            bool isSetTrackerAfterProcessEnded = true;

            #region MQueue sans Id
            if ((false == mQueue.IsMessageObserver) && (false == mQueue.idSpecified))
            {
                SelectDatas();

                if (IsMonoDataProcess && (null != _dsDatas))
                {
                    DataRow[] rows = _dsDatas.Tables[0].Select(null, "IDDATA", DataViewRowState.OriginalRows);
                    bool isFound = (0 < ArrFunc.Count(rows));
                    if (isFound)
                    {
                        if (ArrFunc.Count(rows) == 1)
                        {
                            mQueue.idSpecified = true;
                            mQueue.id = Convert.ToInt32(rows[0]["IDDATA"].ToString());
                        }
                        else
                        {
                            // Insertion d'une ligne dans le tracker
                            InsertNewTracker(ArrFunc.Count(rows));
                            isSetTrackerAfterProcessEnded = false;

                            foreach (DataRow dr in rows)
                            {
                                currentId = Convert.ToInt32(dr["IDDATA"].ToString());
                                mQueue.idSpecified = true;
                                mQueue.id = currentId;
                                MQueueIdInfo mQueueIdInfo = new MQueueIdInfo(currentId);
                                //2009108 FI Test sur IsIdT => On ne cherche le GPRODUCT que si le traitement porte sur un message
                                //Il faut revoir ce traitement pour rechercher le GPRODUCT d'un IDT (voir le cas où  rows = 1)
                                if (mQueue.IsIdT)
                                {
                                    string gProduct = null;
                                    try
                                    {
                                        gProduct = dr["GPRODUCT"].ToString();
                                    }
                                    catch (ArgumentException) { gProduct = null; }
                                    if (null != gProduct)
                                    {
                                        mQueueIdInfo.idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", dr["GPRODUCT"].ToString()) };
                                        mQueue.idInfoSpecified = true;
                                    }
                                }
                                mQueue.idInfo = mQueueIdInfo;
                                MQueueSendInfo mqSendInfo = ServiceTools.GetQueueInfo(appInstance.serviceName);
                                //PL 20101125 Refactoring
                                Send_RetrieveOnError(mQueue, mqSendInfo);//Default timeout: 60 sec.
                            }

                            _isProcessEnded = true;
                        }
                    }
                    else
                    {
                        // RD 20140123 [19526] Mise en commentaire
                        //_processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);

                        //bool isNewTracker;
                        //SetTracker(out isNewTracker);
                        //InsertNewLog();

                        //ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, "SYS-00009",
                        //false, null, new string[] { this.mQueue.header.messageQueueName });
                        _isProcessEnded = true;
                    }
                }
            }
            #endregion MQueue sans Id
            bool isProcessException = false;
            try
            {
                if (_isProcessEnded)
                {
                    if (isSetTrackerAfterProcessEnded)
                    {
                        _processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);

                        bool isNewTracker;
                        SetTracker(out isNewTracker);
                        InsertNewLog();

                        ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, "SYS-00009",
                        false, null, new string[] { this.mQueue.header.messageQueueName });

                    }
                }
                else
                {
                    #region Traitement specific du process
                    currentId = mQueue.id;
                    ProcessInitializeMqueue();
                    ProcessInitialize();
                    ProcessPreExecute();

                    if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn) && (false == IsProcessObserver))
                    {
                        CodeReturn = ProcessExecuteSpecific();
                    }
                    else if (ProcessStateTools.IsCodeReturnIgnoreTuning(CodeReturn) && (processCall == ProcessCallEnum.Master))
                    {
                        ProcessLogAddDetail(ProcessStateTools.StatusWarningEnum, ErrorManager.DetailEnum.FULL, "SYS-00007",
                                            LogTools.IdentifierAndId(mQueue.LogTradeIdentifier, currentId));
                    }
                    else if (ProcessStateTools.IsCodeReturnLockUnsuccessful(CodeReturn))
                    {
                        //PL 20130705 Add this "else if" SYS-00050 [WindowsEvent Project]
                        //ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, lckExisting.ToString());
                        // FI 20141125 [20230] isProcessStateStatutUpdOnErrWarning à false pour que le traitement termine en pending et que le message soit reposté
                        bool savBehavior = isProcessStateStatutUpdOnErrWarning;
                        isProcessStateStatutUpdOnErrWarning = false;
                        ProcessLogAddDetail(ProcessStateTools.StatusWarningEnum, ErrorManager.DetailEnum.DEFAULT, "SYS-00050", null);
                        isProcessStateStatutUpdOnErrWarning = savBehavior;
                    }

                    // EG 21030722
                    if (Cst.ErrLevel.SUCCESS != CodeReturn)
                        CodeReturn = SetCodeReturnFromException();
                    
                    #endregion Traitement specific du process
                }
            }
            catch (Exception ex)
            {
                isProcessException = true;
                SpheresException oEx = SpheresExceptionParser.GetSpheresException(null, ex);
                // EG 20130724
                //SetProcessState(oEx.ProcessState.Status, Cst.ErrLevel.BREAK);
                ProcessState.LastSpheresException = oEx;
                CodeReturn = SetCodeReturnFromException();

                // RD 20130104 [18336] S'il existe un problème lors de l'initialisation des Objets PROCESS_L, TRACKER_L, ...
                // Alors renvoyer l'exception pour l'écrire dans l'EventViewer
                if ((null == processLog) || (0 == processLog.header.IdProcess))
                    throw;

                ProcessLogAddDetail(oEx);
            }
            finally
            {
                try
                {
                    ProcessTerminate();
                    ProcessFinalize();
                    Dispose();
                }
                catch (Exception ex)
                {
                    if (false == isProcessException)
                        throw ex;
                }
            }
#if DEBUG
            diagnosticDebug.End("ElapsedTime", "***********************************************************************");
#endif
        }
        #endregion ProcessStart
        #region ProcessSlave
        /// <summary>
        /// Démarre le process en mode Slave
        /// </summary>
        public void ProcessSlave()
        {
            // Traitement specific du process
            bool isProcessException = false;
            try
            {
                currentId = mQueue.id;
                if (ProcessSlaveEnum.Start == processSlave)
                {
                    ProcessInitialize();
                    ProcessPreExecute();
                }
                if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn) && (ProcessSlaveEnum.Execute == processSlave))
                {
                    CodeReturn = ProcessExecuteSpecific();
                }
            }
            catch (SpheresException ex)
            {
                isProcessException = true;
                ProcessLogAddDetail(ex);
                SetProcessState(ex.ProcessState.Status, (ex.ProcessState.CodeReturn== Cst.ErrLevel.SUCCESS?Cst.ErrLevel.BREAK:ex.ProcessState.CodeReturn));
                if (0 == processLog.header.IdProcess)
                    throw ex;
            }
            catch (Exception ex)
            {
                SpheresException oEx = SpheresExceptionParser.GetSpheresException(null, ex);
                isProcessException = true;
                ProcessLogAddDetail(oEx);
                SetProcessState(oEx.ProcessState.Status, Cst.ErrLevel.BREAK);
                if (0 == processLog.header.IdProcess)
                    throw oEx;
            }
            finally
            {
                if (isProcessException || (processSlave == ProcessSlaveEnum.End))
                {
                    try
                    {
                        ProcessTerminate();
                        ProcessFinalize();
                        Dispose();
                    }
                    catch (Exception ex)
                    {
                        if (false == isProcessException)
                            throw ex;
                    }
                }
                else if (processSlave == ProcessSlaveEnum.Execute)
                    UnLockCurrentObjectId();
            }
        }
        #endregion ProcessSlave
        #region ProcessInitialize
        /// <summary>
        /// Initialisation des variables à chaque changement de Id 
        /// Nouveau Log, nouveau Tracker, etc...
        /// </summary>
        protected virtual void ProcessInitialize()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " ------------------------");
            diagnosticDebug.Start("PrepareRequest");
#endif

            _sqlSessionRestrict = new SqlSessionRestrict(cs, appInstance);
            _processState = new ProcessState(ProcessStateTools.StatusProgressEnum, ProcessStateTools.CodeReturnUndefinedEnum);

            bool isNewTracker;
            SetTracker(out isNewTracker);

            //Attention, cette méthode doit être appelée avant l'insert dans PROCESS_L
            //Ne pas déplacer
            bool isCacheCleared = InitializeQueryCache();

#if DEBUG
            diagnosticDebug.Start("PrepareProcessLog / Read Tracker");
#endif

            // when a processlog instance is already available, this process log instancee is shared by all the built process
            if (processCall != ProcessCallEnum.Slave)
                ProcessLogInitialize(isNewTracker);


#if DEBUG
            diagnosticDebug.End("PrepareProcessLog / Read Tracker");
#endif

            //
            // FI 20120712 Les enums sont chargés dans le cache, 
            // Les enums restent dans le cache tant que Spheres® traite les message issus d'un même request
            // Avant les enums étaients chargé toutes les 5 minutes (cache natif à la classe  LoadFpMLEnumsAndSchemes)
            // Cela pénalisant les longs traitements (exemple EAR) 
            // RD 20120809 [18070] Optimisation de la compta
            if (isCacheCleared || (ExtendEnumsTools.ListEnumsSchemes == default(ExtendEnums)))
            {
#if DEBUG
                diagnosticDebug.Start("LoadFpMLEnumsAndSchemes");
#endif
                //PL 20120822 Add and use isCacheCleared
                //PM 20120824 Add test to check if ExtendEnumsTools.ListEnumsSchemes is initialized
                ExtendEnumsTools.LoadFpMLEnumsAndSchemes(CSTools.SetCacheOn(cs));
#if DEBUG
                diagnosticDebug.End("LoadFpMLEnumsAndSchemes");
#endif
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
        }
        #endregion ProcessInitialize
        #region ProcessFinalize
        /// <summary>
        ///  Fin du process
        ///  <para>Lorsque le process est maître, écriture dans le journal des logs de Spheres et mise à jour du tracker</para>
        /// </summary>
        /// FI 20160412 [XXXXX] Modify
        protected virtual void ProcessFinalize()
        {
            if (processCall == ProcessCallEnum.Master)
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " --------------------------");
#endif
                // FI 20160412 [XXXXX] Tentative de mise à jour du Tracker et envoi des accusé de reception dans le finally 
                // => de manière à exécuter les tâches finales associées au tracker même lorsque la mise à jour des Logs plante (PROCESS_L et PROCESSDET_L)
                try
                {
                    ProcessLogFinalize();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    TrackerFinalize(); 
                }
#if DEBUG
                diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
            }
        }
        #endregion ProcessFinalize
        #region ProcessPreExecute
        /// <summary>
        /// Verification ultime avant execution du traitement 
        /// Cette méthode doit être overridée, on doit y trouver : 
        /// <para>- Mise en place d'un lock</para>
        /// <para>- Verification du respect par rapport à processTuning</para>
        /// </summary>
        protected virtual void ProcessPreExecute()
        {
            CodeReturn = Cst.ErrLevel.SUCCESS;
            // RD 20121031 ne pas vérifier la license pour les services pour des raisons de performances         
            //CheckLicense();
        }
        #endregion ProcessPreExecute
        #region ProcessExecuteSpecific
        /// <summary>
        /// Detail Du Traitement
        /// </summary>
        /// <param name="pSpheresException"></param>
        /// <returns></returns>
        protected virtual Cst.ErrLevel ProcessExecuteSpecific()
        {
            return Cst.ErrLevel.FAILURE;
        }
        #endregion ProcessExecuteSpecific
        #region ProcessTerminateSpecific
        /// <summary>
        /// 
        /// </summary>
        protected virtual void ProcessTerminateSpecific()
        {
        }
        #endregion ProcessTerminateSpecific
        #region GetInfoIdE
        /// <summary>
        /// Retourne la description de l'évènement {pIdE}
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public virtual string GetInfoIdE(int pIdE)
        {
            string ret = string.Empty;
            SQL_Event sqlEvent = new SQL_Event(cs, pIdE);
            sqlEvent.LoadTable();
            if (sqlEvent.IsLoaded)
                ret = sqlEvent.ToString();
            return ret;
        }
        #endregion GetInfoIdE

        #region GetInfoEvent
        /// <summary>
        /// Retourne la description de l'évènement {pIdE}
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        /// <summary>
        /// Retourne la description de l'évènement {pIdE}
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public virtual Pair<int, Pair<string, string>> GetInfoEvent(int pIdE)
        {
            Pair<int, Pair<string, string>> ret = null;
            SQL_Event sqlEvent = new SQL_Event(cs, pIdE);
            sqlEvent.LoadTable(new string[] { "IDE", "EVENTCODE", "EVENTTYPE" });
            if (sqlEvent.IsLoaded)
                ret = new Pair<int, Pair<string, string>>(sqlEvent.Id, new Pair<string, string>(sqlEvent.EventCode, sqlEvent.EventType));
            return ret;
        }
        #endregion GetInfoEvent

        #region ProcessTerminate
        /// <summary>
        /// 
        /// </summary>
        protected void ProcessTerminate()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name, " -------------------------");
#endif

            bool isSendMessage = false;
            try
            {
                // RD 20120626 [17950] Task: Execution of the element under condition
                // Partager du code via processLog.GetProcessState(), avec Task.GetCurrentElementReturnCode()
                // EG 20130722 Add DeadLock
                ProcessStateTools.StatusEnum logStatus;
                switch (CodeReturn)
                {
                    case Cst.ErrLevel.LOCKUNSUCCESSFUL: //Operation non traitée   => Remise in Queue
                    case Cst.ErrLevel.TUNING_UNMATCH:   //Operation non traitée   => Remise in Queue                        
                    case Cst.ErrLevel.TUNING_IGNORE:    //Operation Ignorée       => Mise in Success
                    case Cst.ErrLevel.ACCESDENIED:      //Licence incorrect
                        // EG 20130724
                        //processLog.GetProcessState(CodeReturn, out logStatus);
                        logStatus = GetProcessState(CodeReturn);
                        SetProcessState(logStatus, CodeReturn);
                        break;
                    case Cst.ErrLevel.DEADLOCK:         //Deadlock                => Remise in Queue
                    case Cst.ErrLevel.TIMEOUT:          //Timeout                 => Remise in Queue
                        // EG 20130724
                        //processLog.GetProcessState(CodeReturn, out logStatus);
                        logStatus = GetProcessState(CodeReturn);
                        _processState.Status = logStatus;
                        _processState.CurrentStatus = logStatus;
                        break;
                    case Cst.ErrLevel.BREAK:             //=>  exception occured, process interrupted
                        break;
                    case Cst.ErrLevel.SUCCESS:
                    case Cst.ErrLevel.NOTHINGTODO:
                    case Cst.ErrLevel.TUNING_IGNOREFORCED://Operation Ignorée en DUR => Mise in Success
                    case Cst.ErrLevel.ENTITYMARKET_UNMANAGED: //Operation Ignorée en DUR => Mise in Success
                        logStatus = GetProcessState(CodeReturn);
                        SetProcessState(logStatus, CodeReturn);
                        ProcessTerminateSpecific();
                        isSendMessage = true;
                        break;
                    default:  // necessarily error occured
                        // EG 20130724
                        //processLog.GetProcessState(CodeReturn, out logStatus);
                        logStatus = GetProcessState(CodeReturn);
                        SetProcessState(logStatus, CodeReturn);
                        ProcessTerminateSpecific();
                        isSendMessage = true;
                        break;
                }
            }
            catch (SpheresException ex)
            {
                SetProcessState(ex.ProcessState.Status);
                ProcessLogAddDetail(ex);
            }
            catch (Exception ex)
            {
                SetProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum);
                ProcessLogAddDetail(new ProcessLogInfo(ProcessStateTools.StatusErrorEnum, 0, string.Empty, new string[] { ex.Message, ex.Source }));
            }
            finally
            {
                #region Send Message
                try
                {
                    if ((isSendMessage) && (this.isProcessSendMessage))
                    {
                        _processState.PostedSubMsg += PostProcess_SendMessage();
                        //if ((0 < _processState.PostedSubMsg) && ProcessStateTools.IsStatusSuccess(_processState.Status))
                        //    SetProcessState(ProcessStateTools.StatusProgressEnum);
                    }
                }
                catch (Exception ex)
                {
                    SetProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum);
                    ProcessLogAddDetail(ProcessStateTools.StatusErrorEnum, ErrorManager.DetailEnum.DEFAULT, 3, "SYS-00006",
                        "Send Message Error", ex.Message, ex.Source);
                }
                #endregion
                UnLockCurrentObjectId();
            }

#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name, "------------------------------------------");
#endif
        }
        #endregion ProcessTerminate


        #region AddLog
        #region AddLogDetail
        #region ProcessLogAddDetail with Msg
        /// <summary>
        /// Ecrit un message dans le log
        /// <para>Le message est ajouté uniquement si le niveau de log est >= EXPAND (détail) </para>
        /// <para>Le statut de la igne sera  en N/A</para>
        /// </summary>
        /// <param name="pMsg"></param>
        public void ProcessLogAddDetail(string pMsg)
        {
            ProcessLogAddDetail(ErrorManager.DetailEnum.EXPANDED, pMsg, false);
        }
        /// <summary>
        /// Ecrit un message dans le log
        /// <para>Le message est ajouté uniquement si le niveau de log est >= {pLogLevel}</para>
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <param name="pMsg"></param>
        public void ProcessLogAddDetail(ErrorManager.DetailEnum pLogLevel, string pMsg)
        {
            ProcessLogAddDetail(pLogLevel, pMsg, false);
        }
        /// <summary>
        /// Ecrit un message dans le log, et éventuellement dans le tracker
        /// <para>Le message est ajouté dans le log uniquement si le niveau de log est >= EXPAND (détail) </para>
        /// </summary>
        /// <param name="pMsg"></param>
        /// <param name="pIsUpdateTracker">si true alors mise à jour du tacker</param>
        public void ProcessLogAddDetail(string pMsg, bool pIsUpdateTracker)
        {
            ProcessLogAddDetail(ErrorManager.DetailEnum.EXPANDED, pMsg, pIsUpdateTracker);
        }
        /// <summary>
        /// Ecrit dans le log, et éventuellement dans le tracker, le message {pMsg} 
        /// <para>Le message est ajouté uniquement si le niveau de log est >= {pLogLevel}</para>
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <param name="pMsg"></param>
        /// <param name="pIsUpdateTracker">si true alors mise à jour du tacker</param>
        public void ProcessLogAddDetail(ErrorManager.DetailEnum pLogLevel, string pMsg, bool pIsUpdateTracker)
        {
            // when the blocking flag is set any update command to the tracker is rejected
            if (false == BlockTrackerUpdate)
            {
                ProcessLogInfo processLogInfo = new ProcessLogInfo(ProcessStateTools.StatusUnknownEnum, 0, string.Empty, new string[] { pMsg });
                ProcessLogAddDetail(pLogLevel, processLogInfo);
            }
        }
        #endregion ProcessLogAddDetail with Msg

        #region ProcessLogAddDetail with ProcessLogInfo
        /// <summary>
        /// Ecrit une information dans le log 
        /// <para>L'information est ajoutée uniquement si le niveau de log est >= DetailEnum.EXPANDED </para>
        /// </summary>
        /// <param name="pLogInfo"></param>
        public void ProcessLogAddDetail(ProcessLogInfo pLogInfo)
        {
            ProcessLogAddDetail(ErrorManager.DetailEnum.EXPANDED, pLogInfo);
        }
        /// <summary>
        /// Ecrit une information dans le log 
        /// <para>L'information est ajoutée uniquement si le niveau de log est >= {pLogLevel}</para>
        /// <para>Lorsque le niveau de log est NONE, l'information est ajoutée si son statut est différent de SUCCES ou N/A</para>
        /// </summary>
        /// FI 20131213 [19337] gestion des membres isProcessStateStatutUpdOnErrWarning et  isAutoWriteLogDetail
        public void ProcessLogAddDetail(ErrorManager.DetailEnum pLogLevel, ProcessLogInfo pLogInfo)
        {

            bool isAdd = IsLevelToLog(pLogLevel);
            //
            // En mode NONE, Affichage tout de même des status autres que SUCCES, NA
            // Cela permet de voir les messages d'erreurs même lorsque le log est en mode "sans détail"
            // RD 20100305 / Correction pour être conforme à l'algorithme ci-dessus
            if ((false == isAdd) && (ErrorManager.DetailEnum.NONE == logDetailEnum))
            {
                isAdd = (false == ProcessStateTools.IsStatusSuccess(pLogInfo.status));
                isAdd = isAdd && (false == ProcessStateTools.IsStatusUnknown(pLogInfo.status));
            }

            // FI 20131213 [19337] L'alimentation de _processState.status n'est plus systématique en cas d'erreur
            if (isProcessStateStatutUpdOnErrWarning)
            {
                // EG 20121107 Mise à jour éventuelle du statut (WARNING/ERROR)
                _processState.SetStatus(pLogInfo.status);
            }
            //
            if (isAdd)
            {
                processLog.AddDetail(pLogInfo);
                if (ArrFunc.IsFilled(processLog.detail))
                {
                    if (isAutoWriteLogDetail)
                    {
                        bool isWrite = (processLog.detail.Length >= 100);
                        if ((!isWrite) && (processLog.detail.Length >= 2))
                        {
                            isWrite = (DateTime.Compare(processLog.detail[0].Info.dtstatus.AddSeconds(30), pLogInfo.dtstatus) <= 0);
                        }
                        if (isWrite && (0 != processLog.header.IdProcess))
                        {
                            processLog.SQLWriteDetail();
                        }
                    }
                }
            }
        }
        #endregion ProcessLogAddDetail with ProcessLogInfo


        #region ProcessLogAddDetail with SpheresException
        /// <summary>
        /// Ecrit une exception dans le log
        /// <para>Lorsque le niveau de log est NONE, l'exception est ajoutée uniquement si son status == ERROR </para>
        /// </summary>
        /// <param name="pEx"></param>
        /// FI 20131213 [19337] gestion du membre isAutoWriteLogDetail
        public void ProcessLogAddDetail(SpheresException pEx)
        {
            //PL 20130710 Add and Use IsExistProcessLog 
            bool IsExistProcessLog = (processLog != null);
            bool IsStatusError = pEx.IsStatusError;
            bool isAdd = (ErrorManager.DetailEnum.NONE == logDetailEnum) ? IsStatusError : true;

            if (IsExistProcessLog)
            {
                if (isAdd)
                {
                    processLog.countError++;
                    processLog.AddDetail(pEx);
                    if (ArrFunc.IsFilled(processLog.detail))
                    {
                        if (isAutoWriteLogDetail)
                        {
                            bool isWrite = (processLog.detail.Length >= 100) || (processLog.countError >= 10);
                            if (isWrite && (0 != processLog.header.IdProcess))
                            {
                                processLog.SQLWriteDetail();
                            }
                        }
                    }
                }
                //PL 20130702 [WindowsEvent Project] Newness GLOP voir pour ajouter l'Exception dans ProcessBase
                if (IsStatusError)
                {
                    //WARNING: Analyse en dur de certains messages critiques
                    if (StrFunc.IsFilled(pEx.Message) && pEx.Message.Contains(Cst.SYSTEM_EXCEPTION))
                    {
                        this.SpheresException = pEx;
                    }
                    else if (SpheresExceptionParser.IsCSharpException(pEx) || SpheresExceptionParser.IsRDBMSException(pEx))
                    {
                        this.SpheresException = pEx;
                    }
                }
            }
            else
            {
                //Ajout de l'exception courante dans la collection des exceptions 
                this.SpheresException = pEx;
                //Ajout d'une nouvelle exception dans la collection des exceptions 
                this.SpheresException = new SpheresException("ProcessLogAddDetail", "ERROR! Process log not initialised." + Cst.CrLf + Cst.SYSTEM_EXCEPTION, pEx);
            }
        }
        /// <summary>
        /// Ecrit plusieurs exception dans le log
        /// <para>Lorsque le niveau de log est NONE, les exceptions sont ajoutées uniquement si le status de l'exception == ERROR </para>
        /// </summary>
        /// <param name="pEx"></param>
        public void ProcessLogAddDetail(SpheresException[] pEx)
        {
            if (ArrFunc.IsFilled(pEx))
            {
                for (int i = 0; i < pEx.Length; i++)
                    processLog.AddDetail(pEx[i]);
            }
        }
        #endregion

        #region ProcessLogAddDetail with SystemMSgInfo
        /// <summary>
        /// 
        /// </summary>
        public void ProcessLogAddDetail(SystemMSGInfo pMsg)
        {
            ProcessLogAddDetail(0, pMsg);
        }
        public void ProcessLogAddDetail(int pLevelOrder, SystemMSGInfo pMsg)
        {
            string[] processLogData = new string[ArrFunc.Count(pMsg.datas) + 1];
            processLogData[0] = pMsg.identifier;
            pMsg.datas.CopyTo(processLogData, 1);
            ProcessLogInfo logInfo = new ProcessLogInfo(pMsg.processState.Status, 0, string.Empty, processLogData);
            logInfo.levelOrder = pLevelOrder;
            ProcessLogAddDetail(logInfo);
        }
        #endregion ProcessLogAddDetail with SystemMSgInfo

        /// <summary>
        /// Log a single process event
        /// </summary>
        /// <param name="pStatus">Error status</param>
        /// <param name="pStartLogLevel">minimum log level of the running process in order to give write permission</param>
        /// <param name="pMessage">Log message</param>
        /// <param name="pData">Additional parameters (used also to format the message string)</param>
        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
            string pMessage, params string[] pData)
        {
            ProcessLogAddDetail(pStatus, pStartLogLevel, 0, pMessage, false, null, pData);
        }

        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel, int pLevelOrder,
            string pMessage, params string[] pData)
        {
            List<string> dateList = new List<string>();
            dateList.Add(pMessage);
            dateList.AddRange(pData);
            pData = dateList.ToArray();
            ProcessLogInfo infoToLog = new ProcessLogInfo(pStatus, pLevelOrder, pData);
            ProcessLogAddDetail(pStartLogLevel, infoToLog);
        }

        /// <summary>
        /// Log a single process event
        /// </summary>
        /// <param name="pStatus">Error status</param>
        /// <param name="pStartLogLevel">minimum log level of the running process in order to give write permission</param>
        /// <param name="pMessage">Log message</param>
        /// <param name="pUpdateTracker">flag to set if you want change the tracker label</param>
        /// <param name="pEx">Optional exception to log, can be null</param>
        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
           string pMessage, bool pUpdateTracker, Exception pEx)
        {
            ProcessLogAddDetail(pStatus, pStartLogLevel, 0, pMessage, pUpdateTracker, pEx, null);
        }
        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
           int pLevelOrder, string pMessage, bool pUpdateTracker, Exception pEx)
        {
            ProcessLogAddDetail(pStatus, pStartLogLevel, pLevelOrder, pMessage, pUpdateTracker, pEx, null);
        }

        /// <summary>
        /// Log a single process event
        /// </summary>
        /// <param name="pStartLogLevel">minimum log level of the running process in order to give write permission</param>
        /// <param name="pMessage">Log message</param>
        /// <param name="pStatus">Error status</param>
        /// <param name="pUpdateTracker">flag to set if you want change the tracker label</param>
        /// <param name="pEx">Optional exception to log, can be null</param>
        /// <param name="pData">Additional parameters (used also to format the message string)</param>
        /// 
        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
            string pMessage, bool pUpdateTracker, Exception pEx, params string[] pData)
        {
            ProcessLogAddDetail(pStatus, pStartLogLevel, 0, pMessage, pUpdateTracker, pEx, pData);
        }
        public void ProcessLogAddDetail(ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
            int pLevelOrder, string pMessage, bool pUpdateTracker, Exception pEx, params string[] pData)
        {
            if (pData == null)
            {
                pData = new string[] { pMessage };
            }
            else
            {
                // Le Union utilise un comparateur, et si deux valeurs sont identiques, une seule sera prise en compte dans le Union
                //pData = new string[] { pMessage }.Union(pData).ToArray();
                List<string> dateList = new List<string>();
                dateList.Add(pMessage);
                dateList.AddRange(pData);
                pData = dateList.ToArray();
            }

            ProcessLogInfo infoToLog = new ProcessLogInfo(pStatus, pLevelOrder, pData);
            ProcessLogAddDetail(pStartLogLevel, infoToLog);

            // When the previous logged event is caused by an execption, then we write another log line with the exception details
            if (pEx != null)
            {
                SpheresException exLog = null;

                if (pEx is SpheresException)
                {
                    exLog = (SpheresException)pEx;
                }
                else
                {
                    exLog = new SpheresException(pMessage, pEx);
                }
                ProcessLogAddDetail(exLog);
            }
        }

        /// <summary>
        /// Ajoute dans le log l'exception {pEx}, puis insère un message
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pStatus">statut du message</param>
        /// <param name="pStartLogLevel">Niveau de log à partir duquel le message est ajouté</param>
        /// <param name="pMessage">Message</param>
        /// <param name="pData">paramètres du message</param>
        public void ProcessLogAddDetail(Exception pEx, ProcessStateTools.StatusEnum pStatus, ErrorManager.DetailEnum pStartLogLevel,
            string pMessage, params string[] pData)
        {
            // Log pEx and all innerexceptions
            ProcessLogAddDetail(pEx);

            // Log Message
            string[] dataInfo = null;
            if (pData == null)
            {
                dataInfo = new string[] { pMessage };
            }
            else
            {
                dataInfo = new string[] { pMessage }.Union(pData).ToArray();
            }
            //
            ProcessLogInfo infoToLog = new ProcessLogInfo(pStatus, 0, String.Empty, dataInfo);
            //
            ProcessLogAddDetail(pStartLogLevel, infoToLog);
        }

        /// <summary>
        /// Add in Log all innerExceptions and add {pEx}
        /// </summary>
        /// <param name="pEx"></param>
        public void ProcessLogAddDetail(Exception pEx)
        {
            ProcessLogAddDetail(pEx, true);
        }

        /// <summary>
        ///  Add in Log all innerExceptions and add {pEx} (optionally)
        /// </summary>
        /// <param name="pEx"></param>
        /// <param name="pIsExToLog">true for logging {pEx}</param>
        public void ProcessLogAddDetail(Exception pEx, bool pIsExToLog)
        {
            if (pEx != null)
            {
                if (pEx.InnerException != null)
                    ProcessLogAddDetail(pEx.InnerException, (pEx.Message.Contains(pEx.InnerException.Message) == false));
                if (pIsExToLog)
                    ProcessLogAddDetail(SpheresExceptionParser.GetSpheresException(null, pEx));
            }
        }
        #endregion
        #region AddLogAttachedDoc
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLogLevel">Niveau à partir duquel le document est ajouté dans le log</param>
        /// <param name="pCS"></param>
        /// <param name="pData"></param>
        /// <param name="pName"></param>
        /// <param name="pDocType"></param>
        public void ProcessLogAddAttachedDoc(ErrorManager.DetailEnum pLogLevel, string pCS, byte[] pData, string pName, string pDocType)
        {
            bool isAdd = IsLevelToLog(pLogLevel);
            if (isAdd)
                processLog.AddAttachedDoc(pCS, pData, pName, pDocType);
        }
        #endregion
        #endregion AddLog

        /// <summary>
        /// 
        /// </summary>
        private void InsertNewLog()
        {
            ProcessLogInfo logInfo = new ProcessLogInfo(ProcessStateTools.StatusProgressEnum, mQueue.id, dataIdent, mQueue);
            processLog = new ProcessLog(cs, mQueue.ProcessType, appInstance, logInfo);

            


            processLog.SetHeaderQueueMessage(mQueue.header.messageQueueName);
            if (null != _tracker.idData)
                processLog.SetHeaderIdData(_tracker.idData);
            if (null != _tracker.data)
                processLog.SetHeaderData(_tracker.data);

            processLog.header.idTRK_L = _tracker.idTRK_L;
            processLog.SetHeaderStatus(_processState.Status);
            if (mQueue.header.requesterSpecified && mQueue.header.requester.idPROCESSSpecified)
            {
                processLog.header.IdProcess = mQueue.header.requester.idPROCESS;
            }
            else
            {
                processLog.SQLWriteHeader();
                mQueue.header.requester.idPROCESS = processLog.header.IdProcess;
                mQueue.header.requester.idPROCESSSpecified = (0 < processLog.header.IdProcess);
            }
        }

        /// <summary>
        ///  Mise à jour d'un enregistrement dans le tracker ou création d'un nouvel enregistremenst dans le tracker 
        /// <para>La création d'un nouvel enregistrement s'effectue lorsque la demande de traitement ne fait pas suite à une demande web (Exemple Message issu d'une gateway)</para>
        /// </summary>
        /// <param name="pisNewTracker">Retourne true si le process a généré un nouvel enregistrement dans le tracker</param>
        private void SetTracker(out bool pisNewTracker)
        {
            pisNewTracker = false;

            if (mQueue.header.requesterSpecified && mQueue.header.requester.idTRKSpecified)
            {
                // Une ligne dans le TRACKER existe : LECTURE et UPD
                _tracker = new Tracker(cs, mQueue.ProcessType, mQueue.GetStringValueIdInfoByKey("GPRODUCT"), mQueue.header.requester.idTRK);
                _tracker.Select(mQueue.ProcessType);
                _tracker.readyState = ProcessStateTools.ReadyStateActiveEnum;
                _tracker.status = _processState.Status;
                if (mQueue.header.requester.entitySpecified)
                    _tracker.ack.request.appInstance.IdA_Entity = Convert.ToInt32(mQueue.header.requester.entity.otcmlId);
                _tracker.Update();
            }
            else
            {
                pisNewTracker = true;
                // EG 20121105 
                // Aucune ligne dans le TRACKER : INSERTION
                InsertNewTracker();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void InsertNewTracker()
        {
            InsertNewTracker(1);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPostedMsg"></param>
        private void InsertNewTracker(int pPostedMsg)
        {
            string gProduct = mQueue.GetStringValueIdInfoByKey("GPRODUCT");
            _tracker = new Tracker(cs, mQueue.ProcessType, gProduct, ProcessStateTools.ReadyStateActiveEnum, _processState.Status);

            // FI 20120129 [18252]
            // Les traitements I/O rentre dans le group EXT du tracker
            if (mQueue.GetType().Equals(typeof(IOMQueue)))
            {
                if (((IOMQueue)mQueue).IsGatewayMqueue)
                    _tracker.group = Cst.GroupTrackerEnum.EXT;
            }

            _tracker.idData = new TrackerIdData(mQueue.id, mQueue.identifier, dataIdent);

            MQueueTaskInfoTracker taskInfoTracker = SetMQueueTaskInfoTracker();
            _tracker.data = new TrackerData(taskInfoTracker);
            _tracker.status = _processState.Status;
            DateTime dtRequest = DateTime.MinValue;
            if (false == mQueue.header.requesterSpecified)
            {
                mQueue.header.requester = new MQueueRequester();
                mQueue.header.requester.date = _tracker.dtStart;
                mQueue.header.requesterSpecified = true;
            }

            _tracker.ack = new TrackerAck(mQueue.header.requester.date, (AppInstance)appInstance, pPostedMsg, taskInfoTracker);
            _tracker.ack.SetMQueueIdInfo(mQueue.idInfo);
            _tracker.Insert();

            mQueue.header.requester.idTRKSpecified = (0 < _tracker.idTRK_L);
            mQueue.header.requester.idTRK = _tracker.idTRK_L;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private MQueueTaskInfoTracker SetMQueueTaskInfoTracker()
        {
            MQueueTaskInfoTracker taskInfoTracker = new MQueueTaskInfoTracker(mQueue.ProcessType);
            taskInfoTracker.gProduct = mQueue.GetStringValueIdInfoByKey("GPRODUCT");
            if (mQueue is PosKeepingRequestMQueue)
                taskInfoTracker.caller = ((PosKeepingRequestMQueue)mQueue).requestType.ToString();
            else if (mQueue is IOMQueue)
                taskInfoTracker.caller = mQueue.GetStringValueIdInfoByKey("IN_OUT");
            else if (mQueue is NormMsgFactoryMQueue)
            {
                NormMsgFactoryMQueue normMsgFactoryMQueue = (NormMsgFactoryMQueue)mQueue;
                if (normMsgFactoryMQueue.buildingInfo.posRequestTypeSpecified)
                    taskInfoTracker.caller = normMsgFactoryMQueue.buildingInfo.posRequestType.ToString();
                else
                    taskInfoTracker.caller = normMsgFactoryMQueue.buildingInfo.processType.ToString();

                if (null != normMsgFactoryMQueue.acknowledgment)
                    taskInfoTracker.acknowledgment = normMsgFactoryMQueue.acknowledgment;
                else
                {
                    AckSchedule ackWebSession = AddDefaultAckWebSession();
                    if (null != ackWebSession)
                    {
                        taskInfoTracker.acknowledgment = new AcknowledgmentInfo();
                        taskInfoTracker.acknowledgment.schedules = new AckSchedules();
                        taskInfoTracker.acknowledgment.schedules.Item = new AckSchedule[1] { ackWebSession };
                    }
                }

            }
            else if (mQueue is ConfirmationMsgGenMQueue)
                taskInfoTracker.caller = "TODO";
            else if (mQueue is InvoicingGenMQueue)
                taskInfoTracker.caller = "TODO";
            else if (mQueue is QuotationHandlingMQueue)
                taskInfoTracker.caller = "TODO";

            taskInfoTracker.info = MQueueTools.SetDataTracker(mQueue);
            return taskInfoTracker;

        }


        #region Send_RetrieveOnError
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pSendInfo"></param>
        public void Send_RetrieveOnError(MQueueBase pMQueue, MQueueSendInfo pSendInfo)
        {
            double timeout = 60; //60 sec.
            Send_RetrieveOnError(pMQueue, pSendInfo, timeout);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pSendInfo"></param>
        /// <param name="pTimeout"></param>
        public void Send_RetrieveOnError(MQueueBase pMQueue, MQueueSendInfo pSendInfo, double pTimeout)
        {

            int nbAttemps = 0;
            MQueueTools.Send(pMQueue, pSendInfo,  pTimeout, ref nbAttemps);
            if (nbAttemps > 1)
            {
                //Il a fallu plusieurs tentative pour réussir à ouvrir la queue.
                string path = pSendInfo.MOMSetting.MOMPath;
                ProcessLogInfo logInfo = null;
                string[] info = null;

                //Injection dans le log d'un message d'erreur (le traitement finira donc en warning)
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Path: {0}]", path);
                info[1] = StrFunc.AppendFormat(@"[Failed attempts to access: {0}]", (nbAttemps - 1).ToString());

                string errorMsg = "[Code Return:" + Cst.Space + Cst.ErrLevel.MOM_PATH_ERROR.ToString() + Cst.Space + "]" + Cst.CrLf2;
                errorMsg += @"MSMQ is unreachable" + Cst.CrLf;
                errorMsg += info[0] + Cst.CrLf;
                errorMsg += info[1] + Cst.CrLf;

                logInfo = new ProcessLogInfo();
                //STATUT N/A
                //car finalement Spheres® accède à la queue (après plusieurs tentatives certes)
                //Dans ce cas de figure, il existe des pbs réseaux => ce message sera récupéré par les services pour injecter dans le journal des évènements
                //Le traitement ne termine pas en WARNING
                logInfo.status = ProcessStateTools.StatusUnknown;
                logInfo.message = errorMsg;
                logInfo.data1 = "Method [ProcessBase.Send_RetrieveOnError]";
                logInfo.data2 = info[0];
                logInfo.data3 = info[1];
                ProcessLogAddDetail(logInfo);

                //Injection dans le log d'un message success puisque Spheres® a réussi à se connecter après plusieurs tentatives 
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Path: {0}]", path);
                info[1] = StrFunc.AppendFormat(@"[Attempts to access: {0}]", nbAttemps.ToString());
                //
                string infoMsg = @"MSMQ has opened successfully" + Cst.CrLf;
                infoMsg += info[0] + Cst.CrLf;
                infoMsg += info[1] + Cst.CrLf;
                //
                logInfo = new ProcessLogInfo();
                logInfo.status = ProcessStateTools.StatusUnknown;
                logInfo.message = infoMsg;
                logInfo.data1 = "Method [ProcessBase.Send_RetrieveOnError]";
                logInfo.data2 = info[0];
                logInfo.data3 = info[1];
                ProcessLogAddDetail(logInfo);

                processLog.SQLWriteDetail();
            }
        }
        #endregion Send_RetrieveOnError

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceEnum"></param>
        /// <returns></returns>
        /// FI 20120702 [] Add SetCacheOn sur le select de la table LICENSEE
        public bool IsServiceAuthorised(Cst.ServiceEnum pServiceEnum)
        {
            DataTable dtLicenses = null;
            bool isOk = false;

            string SQLSelect = SQLCst.SELECT + "IDEFSSOFTWARE,REGISTRATIONXML" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString() + Cst.CrLf;

            DataSet dsLicenses = DataHelper.ExecuteDataset(CSTools.SetCacheOn(cs), CommandType.Text, SQLSelect);
            dtLicenses = dsLicenses.Tables[0];

            if (null == dtLicenses || dtLicenses.Select().Length < 1)
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Missing licensee! Contact EFS to obtain a licensee and methods of registration.");

            //PM 20120201 Add SOFTWARE_Spheres
            isOk = IsServiceAuthorised(pServiceEnum, dtLicenses, Software.SOFTWARE_Spheres);
            if (!isOk)
            {
                //PL 20120201 A supprimer prochainement...
                isOk = IsServiceAuthorised(pServiceEnum, dtLicenses, Software.SOFTWARE_OTCml);
                if (!isOk)
                {
                    isOk = IsServiceAuthorised(pServiceEnum, dtLicenses, Software.SOFTWARE_Vision);
                    //PL 20111020 Add SOFTWARE_Portal
                    if (!isOk)
                        isOk = IsServiceAuthorised(pServiceEnum, dtLicenses, Software.SOFTWARE_Portal);
                }
            }
            return isOk;
        }
        public bool IsServiceAuthorised(Cst.ServiceEnum pServiceEnum, DataTable pDtLicenses, string pSoftwareName)
        {
            bool isOk = false;
            string registrationXML = string.Empty;
            DataRow[] drLicense = pDtLicenses.Select("IDEFSSOFTWARE = " + DataHelper.SQLString(pSoftwareName));
            //
            if (drLicense.Length > 1)
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Too many '" + pSoftwareName + "' licensees! Contact EFS to obtain a licensee and methods of registration.");
            //
            if (drLicense.Length == 1)
            {
                registrationXML = drLicense[0]["REGISTRATIONXML"].ToString();

                if (StrFunc.IsFilled(registrationXML))
                {
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(License), registrationXML.Trim());
                    _license = (License)CacheSerializer.Deserialize(serializeInfo);
                }

                if (null == _license)
                    throw new SpheresException(MethodInfo.GetCurrentMethod().Name, "Missing '" + pSoftwareName + "' licensee! Contact EFS to obtain a licensee and methods of registration.");

                isOk = _license.IsLicServiceAuthorised(pServiceEnum);
            }
            return isOk;
        }

        /// <summary>
        /// Retourne true si le niveau de log {pLogLevel} est suffisant pour être inséré dans le log 
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <returns></returns>
        public bool IsLevelToLog(ErrorManager.DetailEnum pLogLevel)
        {
            return (logDetailEnum >= pLogLevel) ||
                ((logDetailEnum == ErrorManager.DetailEnum.DEFAULT) &&
                    (ErrorManager.DetailEnum.EXPANDED >= pLogLevel));
        }

        /// <summary>
        /// Cette méthode doit être appelée lorsque le service s'arrête violemment alors qu'un traitement est en cours
        /// <para>Elle position le statut retour du traitement en erreur</para>
        /// <para>Elle alimente le log avec le message Msg_ServiceStopped</para>
        /// </summary>
        public void StopProcess()
        {

            SetProcessState(ProcessStateTools.StatusErrorEnum);
            if (null != processLog)
            {
                processLog.SetHeaderStatus(ProcessState.Status);
                processLog.header.Info.message = "[Code Return:" + Cst.Space + Cst.ErrLevel.SERVICE_STOPPED + Cst.Space + "]" + Cst.CrLf + processLog.header.Info.message;
                processLog.SQLUpdateHeader();
            }

            if (null != tracker)
            {
                tracker.status = ProcessState.Status;
                tracker.Update();
                if (0 < tracker.idTRK_L)
                {
                    tracker.SetCounter(ProcessState.Status, _processState.PostedSubMsg);
                    if (ProcessStateTools.ReadyStateEnum.TERMINATED == tracker.readyState)
                        SendResponseQueue();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceTrace"></param>
        public void TraceProcess(ServiceTrace pServiceTrace)
        {

            AutoResetEvent autoResetEvent = new AutoResetEvent(false);
            TaskProcess task = new TaskProcess();
            task.DtLast = DateTime.Now;
            task.DtHost = task.DtLast;
            task.CS = cs;
            task.ProcessType = mQueue.ProcessType;
            task.isServiceObserver = mQueue.IsMessageObserver;
            task.countQueueHigh = pServiceTrace.countQueueHigh;
            task.countQueueNormal = pServiceTrace.countQueueNormal;
            task.countQueueLow = pServiceTrace.countQueueLow;
            if (mQueue.header.requesterSpecified)
                task.Requester = mQueue.header.requester;
            task.ProcessState = ProcessState;
            if ((null != processLog) && (null != processLog.header))
                task.IdProcess_L = processLog.header.IdProcess;
            task.handle = ThreadPool.RegisterWaitForSingleObject(autoResetEvent,
                                            new WaitOrTimerCallback(pServiceTrace.Update), task, 1000, false);
            autoResetEvent.Set();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pServiceTrace"></param>
        public void UpdateTrackerWithIdService(ServiceTrace pServiceTrace)
        {
            if ((mQueue.IsMessageObserver) && (null != tracker) && (null != tracker.data))
            {
                while (0 == pServiceTrace.CurrentIdService(mQueue.ProcessType))
                    Thread.Sleep(0);
                tracker.Update(Cst.OTCml_TBL.SERVICE_L.ToString(), pServiceTrace.CurrentIdService(mQueue.ProcessType));
            }
        }

        /// <summary>
        /// Retourne les informations MOM type, MOM patch etc pour le paramètre pProcess
        /// </summary>
        /// <param name="pProcess"></param>
        /// <returns></returns>
        /// EG 20130619 Service/MOMPath 
        public MQueueSendInfo GetMqueueSendInfo(Cst.ProcessTypeEnum pProcess)
        {
            return ServiceTools.GetMqueueSendInfo(pProcess, appInstance);
            //Cst.ServiceEnum serviceTarget = Cst.Process.GetService(pProcess);
            //PL 20121227 Use serviceName_WithoutInstance instead of serviceName
            //string serviceTargetName = appInstance.serviceName_WithoutInstance.Replace(appInstance.serviceEnum.ToString(), serviceTarget.ToString());
            //serviceTargetName = serviceTargetName.Replace("DEBUG", string.Empty);
            /// EG 20130619 Service/MOMPath Add appInstance.MOMPath parameter
            //MQueueSendInfo sendInfo = ServiceTools.GetQueueInfo(serviceTargetName, appInstance.MOMPath);
            //return sendInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public AckSchedule AddDefaultAckWebSession()
        {
            MQueueSendInfo sendInfo = null;
            AckWebSessionSchedule ackWebSession = null;
            Cst.ServiceEnum serviceTarget = Cst.Process.GetService(Cst.ProcessTypeEnum.RESPONSE);
            string serviceTargetName = appInstance.serviceName.Replace(appInstance.serviceEnum.ToString(), serviceTarget.ToString());
            sendInfo = ServiceTools.GetQueueInfo(serviceTargetName);
            if (false == sendInfo.isInfoValid)
            {
                serviceTargetName = serviceTargetName.Replace("DEBUG", string.Empty);
                sendInfo = ServiceTools.GetQueueInfo(serviceTargetName);
            }
            if (sendInfo.isInfoValid)
            {
                ackWebSession = new AckWebSessionSchedule();
                ackWebSession.responseSpecified = true;
                ackWebSession.response = sendInfo.MOMSetting;
            }
            return ackWebSession;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="SpheresException si la licence est incorrecte"></exception>
        protected void CheckLicense()
        {
            if (false == IsServiceAuthorised(appInstance.serviceEnum))
            {
                ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnAccessDeniedEnum);
                throw new SpheresException(MethodInfo.GetCurrentMethod().Name,
                    "Your licensee don't allow you to run " + appInstance.serviceEnum.ToString() + " service in this environment",
                     processState);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRequestProcess"></param>
        /// <param name="pNbMessage"></param>
        /// <param name="pNbSuccess"></param>
        /// <param name="pNbError"></param>
        /// <param name="pStatus"></param>
        private void SendResponseQueue()
        {
            //MQueueAttributes mQueueAttributes = new MQueueAttributes(cs);
            //mQueueAttributes.id = mQueue.id;
            //mQueueAttributes.idInfo = mQueue.idInfo;
            //mQueueAttributes.requester = mQueue.header.requester;
            //ResponseRequestMQueue responseMQueue = new ResponseRequestMQueue(tracker, mQueueAttributes);
            //MQueueSendInfo sendInfo = new MQueueSendInfo();
            //if ((mQueue.header.requester != null) && (mQueue.header.requester.responseSpecified))
            //    sendInfo.MOMSetting = mQueue.header.requester.response;
            //else
            //    sendInfo = GetMqueueSendInfo(tracker.process);
            ////
            //Send_RetrieveOnError(responseMQueue, sendInfo);//Default timeout: 60 sec.
        }

        /// <summary>
        /// Contrôle l'existence du folder associé à un path de fichier
        /// <para>Si plusieurs tentatives ont été nécessaires pour accéder au Folder, Spheres® injecte des lignes dans le log pour le notifier</para>
        /// <para>Attention ServiceBase s'appuie sur l'écriture dans le log pour restituer des informations dans le journal des évènements de windows</para>
        /// </summary>
        /// <exception cref="SpheresException[FOLDERNOTFOUND] si folder non accessible, l'exception donne les informations suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pFilePath"></param>
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout)
        {
            CheckFolderFromFilePath(pPathFileName, pTimeout, 0);
        }
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout, int pLevelOrder)
        {

            int nbAttemps = 0;
            //
            FileTools.CheckFolderFromFilePath(pPathFileName, pTimeout, out nbAttemps, pLevelOrder);
            //
            if (nbAttemps >= 2) // Cela veut dire que le folder existe mais qu'il a fallu plusieurs tentatives pour que Spheres® y accèder
            {
                string fileName, folderPath;
                FileTools.GetFilenameAndFoldername(pPathFileName, out fileName, out folderPath);

                ProcessLogInfo logInfo = null;
                string[] info = null;

                //Injection dans le log d'un message d'erreur (le traitement finira donc en warning)
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Folder: {0}]", folderPath);
                info[1] = StrFunc.AppendFormat(@"[Failed attempts to access: {0}]", (nbAttemps - 1).ToString());

                string errorMsg = "[Code Return:" + Cst.Space + Cst.ErrLevel.FOLDERNOTFOUND + Cst.Space + "]" + Cst.CrLf2;
                errorMsg += @"Folder does not exist or is not accessible" + Cst.CrLf;
                errorMsg += info[0] + Cst.CrLf;
                errorMsg += info[1] + Cst.CrLf;
                logInfo = new ProcessLogInfo();
                //PL/FI=> STATUT N/A
                //car finalement Spheres® accède au répertoire (après plusieurs tentatives certes)
                //Dans ce cas de figure il existe des pbs réseaux => ce meesage sera récupéré par les services pour injecter dans le journal des évènements
                //Le traitement ne termine pas en WARNING
                logInfo.status = ProcessStateTools.StatusUnknown;
                logInfo.message = errorMsg;
                logInfo.data1 = MethodInfo.GetCurrentMethod().Name;
                logInfo.data2 = info[0];
                logInfo.data3 = info[1];
                logInfo.levelOrder = pLevelOrder;
                ProcessLogAddDetail(logInfo);

                //Injection dans le log d'un message success puisque Spheres® a réussi à se connecter après plusieurs tentatives 
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Folder: {0}]", folderPath);
                info[1] = StrFunc.AppendFormat(@"[Attempts to access: {0}]", nbAttemps.ToString());
                //
                string infoMsg = @"Process has successfully restablished connection with folder" + Cst.CrLf;
                infoMsg += info[0] + Cst.CrLf;
                infoMsg += info[1] + Cst.CrLf;
                //
                logInfo = new ProcessLogInfo();
                logInfo.status = ProcessStateTools.StatusUnknown;
                logInfo.message = infoMsg;
                logInfo.data1 = MethodInfo.GetCurrentMethod().Name;
                logInfo.data2 = info[0];
                logInfo.data3 = info[1];
                logInfo.levelOrder = pLevelOrder;
                ProcessLogAddDetail(logInfo);
            }
        }

        /// <summary>
        /// Purge le cache SQL à chaque nouveau Request
        /// <para>La cache SQL est conservé lorsque spheres® traite les messages issus d'une même demande</para>
        /// </summary>
        /// FI 20120701 [17979] Add InitializeQueryCache
        private bool InitializeQueryCache()
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

            bool clearCache = true;
            if (0 < tracker.idTRK_L)
            {
                if (LogTools.ExistLogForTracker(CSTools.SetCacheOn(cs, 1, null), tracker.idTRK_L))
                    clearCache = false;
            }

            if (clearCache)
            {
                DataHelper.queryCache.Clear(cs);
            }
#if DEBUG
            diagnosticDebug.End(MethodInfo.GetCurrentMethod().Name);
#endif
            return clearCache;
        }

        /// <summary>
        /// Alimente, Enrichie le message queue lorsque ce dernier est pauvre
        /// </summary>
        protected virtual void ProcessInitializeMqueue()
        {
        }

        /// <summary>
        /// Initialisation du Log du traitement
        /// <para>Généralement Spheres® génère un nouveau log (nouvel enregistrement dans PROCESS_L)</para>
        /// <para>Spheres® récupère le log d'un précédent traitement terminé en PENDING </para>
        /// </summary>
        /// <param name="pIsNewTracker">vaut true lorsque le traitement n'a pas été demandé manuellement depuis l'application web</param>
        /// FI 20120613 Initialisation du membre processLog
        private void ProcessLogInitialize(bool pIsNewTracker)
        {
            ProcessLogInfo logInfo = new ProcessLogInfo(ProcessStateTools.StatusProgressEnum, mQueue.id, dataIdent, mQueue);
            processLog = new ProcessLog(cs, mQueue.ProcessType, appInstance, logInfo);

            processLog.SetHeaderQueueMessage(mQueue.header.messageQueueName);
            if (null != _tracker.data)
                processLog.SetHeaderData(_tracker.data);
            if (null != _tracker.idData && StrFunc.IsFilled(_tracker.idData.idDataIdent))
                processLog.SetHeaderIdData(_tracker.idData);

            processLog.header.idTRK_L = _tracker.idTRK_L;
            processLog.SetHeaderStatus(_processState.Status);

            if (mQueue.header.requesterSpecified && mQueue.header.requester.idPROCESSSpecified)
            {
                processLog.header.IdProcess = mQueue.header.requester.idPROCESS;
            }
            else
            {
                if (dataTypeLock != TypeLockEnum.NA && mQueue.header.creationTimestampSpecified)
                {
                    DateTime dt1 = mQueue.header.creationTimestamp;
                    DateTime dt2 = processLog.GetDate();
                    if (pIsNewTracker)
                    {
                        processLog.SetIdProcessFromPendingProcess(dt1, dt2, 0);
                    }
                    else
                    {
                        //Recherche d'un traitement exécutée et mise en attente (Pending) précédement par le service et issu de la demande idTRK_L
                        processLog.SetIdProcessFromPendingProcess(dt1, dt2, processLog.header.idTRK_L);
                    }
                }

                bool isNewLog = (processLog.header.IdProcess == 0);
                if (isNewLog)
                {
                    processLog.SQLWriteHeader();
                }
                else
                {
                    processLog.SQLUpdateHeader();
                    processLog.SQLDeleteDetail(null);
                }

                mQueue.header.requester.idPROCESS = processLog.header.IdProcess;
                mQueue.header.requester.idPROCESSSpecified = (0 < processLog.header.IdProcess);
            }
        }

        /// <summary>
        /// EG 20130722 Mise à jour du CodeReturn si RDBMSException (Timeout / DeadLock)
        /// </summary>
        /// EG 20140220 timeout/deadlock avec espace
        /// FI 20140819 [20291] Modify
        public Cst.ErrLevel SetCodeReturnFromException()
        {
            Cst.ErrLevel ret = CodeReturn;
            if (null != ProcessState.LastSpheresException)
            {
                SpheresException ex = ProcessState.LastSpheresException;
                ret = ex.ProcessState.CodeReturn;
                if (SpheresExceptionParser.IsExistRDBMSException(ex))
                {
                    string message = ex.Message;
                    if (StrFunc.IsFilled(ex.StackTrace))
                        message += Cst.CrLf + ex.StackTrace;
                    if (ex.IsInnerException && StrFunc.IsFilled(ex.InnerException.StackTrace))
                        message += Cst.CrLf + ex.InnerException.StackTrace;

                    if (message.ToLower().Contains(" timeout "))
                        ret = Cst.ErrLevel.TIMEOUT;
                    // EG 20151123 Add Test en français
                    else if (message.ToLower().Contains(" deadlock ") || ( message.ToLower().Contains(" a été bloquée ") && message.ToLower().Contains(" choisie comme victime ")))
                        ret = Cst.ErrLevel.DEADLOCK;
                }
                else if (ex.InnerException != null && ex.InnerException.GetType() == typeof(TimeoutException))
                {
                    // FI 20140819 [20291] add test sur TimeoutException pour être en phase avec DataAccessBase.AnalyseSQLException      
                    // Car Spheres® reposte le message si une exception TimeoutException est détectée (voir méthode SpheresServiceBase.ActiveProcess)
                    ret = Cst.ErrLevel.TIMEOUT;
                }

            }
            return ret;
        }
        
        /// <summary>
        /// <para>
        ///  - Mise à jour de PROCESS_L avec le statut final du traitement  
        /// </para>
        /// <para>
        ///  - écriture des éventuels lignes de détails non encore insérées dans PROCESSDET_L
        /// </para>
        /// </summary>
        /// FI 20160412 [XXXXX] Add
        private void ProcessLogFinalize()
        {
            if (processLog != null)
            {
                if (ProcessStateTools.IsStatusPending(_processState.Status))
                    ProcessLogAddDetail(_processState.Status, ErrorManager.DetailEnum.DEFAULT, "The message is placed again in the queue", null);

                processLog.SetHeaderStatus(_processState.Status);
                processLog.header.Info.message = "[Code Return:" + Cst.Space + CodeReturn + Cst.Space + "]" + Cst.CrLf + processLog.header.Info.message;

                if (0 != processLog.header.IdProcess)
                {
                    // Il est important de conserver la mise à jour de processLogHeader avec l'écriture du détail
                    // Comme cela PROCESS_L sera mis à jour même si une exception se produit pendant SQLWriteDetail 
                    processLog.SQLUpdateHeader();
                    processLog.SQLWriteDetail();
                }
            }
        }
        
        /// <summary>
        /// <para>
        /// - Mise à jour du statut du tracker
        /// </para>
        /// <para>
        /// - Génération des accusés de deception 
        /// </para>
        /// </summary>
        /// FI 20160412 [XXXXX] Add
        private void TrackerFinalize()
        {
            if ((tracker != null) && (tracker.idTRK_L > 0) && (ProcessStateTools.IsStatusTerminated(_processState.Status)))
            {
                tracker.SetCounter(_processState.Status, _processState.PostedSubMsg);
                if (ProcessStateTools.IsReadyStateTerminated(tracker.readyState))
                {
                    // EG 20151102 [21465] New
                    if (null != processLog)
                        _tracker.idProcess = processLog.header.IdProcess;
                    _tracker.AckGenerate();
                }

                if (0 < _processState.PostedSubMsg)
                {
                    SetProcessState(ProcessStateTools.StatusProgressEnum);
                }
            }
        }


        //FI 20140819 [XXXXX] Mise en commentaire de cette fonction puisque non utilisée
        //public bool IsExceptionTimoutAndDeadlock(Exception pException)
        //{
        //    SpheresException ex = SpheresExceptionParser.GetSpheresException(null, pException);
        //    ProcessState.LastSpheresException = ex;
        //    Cst.ErrLevel retRDBMS = SetCodeReturnForRDBMSException();
        //    return (retRDBMS == Cst.ErrLevel.TIMEOUT || retRDBMS == Cst.ErrLevel.DEADLOCK);
        //}

        // EG 20130722 DEADLOCK
        //// FI 20131213 Mise en commentaire => Il faut utiliser la classe EFS.ApplicationBlocks.Data.DeadLockGen
        //public void TEST_DEADLOCKMASTER()
        //{

        //    IDbTransaction dbTransaction = null;
        //    try
        //    {
        //        System.Threading.ThreadStart threadStart = new System.Threading.ThreadStart(TEST_DEADLOCK);
        //        System.Threading.Thread thread = new System.Threading.Thread(threadStart);

        //        dbTransaction = DataHelper.BeginTran(mQueue.ConnectionString);
        //        thread.Start();
        //        System.Threading.Thread.Sleep(1000);
        //        int id;
        //        Cst.ErrLevel errLevelb = SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_B");
        //        System.Threading.Thread.Sleep(2000);
        //        Cst.ErrLevel errLevela = SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_A");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        DataHelper.RollbackTran(dbTransaction);
        //    }
        //}
        //public void TEST_DEADLOCK()
        //{
        //    IDbTransaction dbTransaction = null;
        //    try
        //    {
        //        dbTransaction = DataHelper.BeginTran(mQueue.ConnectionString);

        //        int id;
        //        Cst.ErrLevel errLevela = SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_A");
        //        Thread.Sleep(2000);
        //        Cst.ErrLevel errLevelb = SQLUP.GetId(out id, dbTransaction, "TESTDEADLOCK_B");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //    finally
        //    {
        //        DataHelper.RollbackTran(dbTransaction);
        //    }
        //}

        #endregion Methods
    }
    #endregion ProcessBase

    #region ProcessTradeBase
    /// <summary>
    /// class de base pour traitement sur un trade
    /// </summary>
    public abstract class ProcessTradeBase : ProcessBase
    {
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAdmin
        {
            get { return mQueue.IsTradeAdmin; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeAsset
        {
            get { return mQueue.IsTradeAsset; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsTradeRisk
        {
            get { return mQueue.IsTradeRisk; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsMonoDataProcess
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected DataSet dsTrades
        {
            get { return _dsDatas; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string dataIdent
        {
            get
            {
                string ret = "N/A";
                if (IsTradeAdmin)
                    ret = Cst.OTCml_TBL.TRADEADMIN.ToString();
                else if (IsTradeAsset)
                    ret = Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
                else if (IsTradeRisk)
                    ret = Cst.OTCml_TBL.TRADERISK.ToString();
                else if (mQueue.GetType().Equals(typeof(PosKeepingRequestMQueue)))
                    ret = Cst.OTCml_TBL.POSREQUEST.ToString();
                else
                    ret = Cst.OTCml_TBL.TRADE.ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum dataTypeLock
        {
            get
            {
                return TypeLockEnum.TRADE;
            }
        }

        

        #endregion Accessors
        #region constructor
        public ProcessTradeBase(MQueueBase pMQueue, AppInstanceService pAppInstance): base(pMQueue, pAppInstance)
        { }
        #endregion
        #region Methods
        /// <summary>
        /// Charge les ids qui doivent être traités
        /// <para>Ici ce sont nécessairement des trades</para>
        /// </summary>
        protected override void SelectDatas()
        {
            SelectTrades();
        }

        /// <summary>
        /// Charge les trades qui doivent être traités 
        /// <para>Cette méthode est exécutée lorsque le message ne contient pas d'Id</para>
        /// </summary>
        protected virtual void SelectTrades()
        {
            DataParameters parameters = new DataParameters();
            if (mQueue.idSpecified)
                parameters.Add(new DataParameter(cs, "IDT", DbType.Int32), mQueue.id);
            else if (mQueue.identifierSpecified)
                parameters.Add(DataParameter.GetParameter(cs, DataParameter.ParameterEnum.IDENTIFIER), mQueue.identifier);
            //
            if (mQueue.parametersSpecified)
            {
                if (null != mQueue.GetObjectValueParameterById(MQueueBase.PARAM_ENTITY))
                {
                    if (-1 != mQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY))
                    {
                        DataParameter paramEntity = new DataParameter(cs, "ENTITY", DbType.Int32);
                        paramEntity.Value = mQueue.GetIntValueParameterById(MQueueBase.PARAM_ENTITY);
                        parameters.Add(paramEntity);
                    }
                }
            }

            StrBuilder sqlSelect = new StrBuilder(SQLCst.SELECT + @"t.IDT As IDDATA, t.GPRODUCT As GPRODUCT" + Cst.CrLf);
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_TRADE + " t" + Cst.CrLf;

            SQLWhere sqlWhere = new SQLWhere();
            if (mQueue.idSpecified)
                sqlWhere.Append(@"(t.IDT=@IDT)");
            else if (mQueue.identifierSpecified)
                sqlWhere.Append(@"(t.IDENTIFIER=@IDENTIFIER)");
            if (parameters.Contains("ENTITY"))
                sqlWhere.Append(@"((t.IDA_ENTITYBUYER=@ENTITY) or (t.IDA_ENTITYSELLER=@ENTITY))");

            sqlSelect += sqlWhere.ToString();
            string sqlSelectTrade = sqlSelect.ToString();
            _dsDatas = DataHelper.ExecuteDataset(cs, CommandType.Text, sqlSelectTrade, parameters.GetArrayDbParameter());
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                #region ProcessTuning => Initialisation from Trade
                // RD 20120809 [18070] Optimisation de la compta
                if (IsMonoDataProcess /*&&
                    (mQueue.ProcessType != Cst.ProcessTypeEnum.EARGEN && mQueue.ProcessType != Cst.ProcessTypeEnum.ACCOUNTGEN)*/)
                {
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(CSTools.SetCacheOn(cs), currentId);
                    if (sqlTrade.LoadTable(new string[] { "IDT", "IDI" }))
                    {
                        processTuning = new ProcessTuning(CSTools.SetCacheOn(cs), sqlTrade.IdI, mQueue.ProcessType, appInstance.serviceName, appInstance.HostName);
                        if (processTuningSpecified && (processCall != ProcessCallEnum.Slave))
                            logDetailEnum = processTuning.logDetailEnum;
                    }
                }
                #endregion
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessPreExecute()
        {
            base.ProcessPreExecute();
            if (false == IsProcessObserver)
            {
                if (false == TradeRDBMSTools.IsTradeExist(this.cs, currentId))
                {
                    ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnDataNotFoundEnum);
                    throw new SpheresException(MethodInfo.GetCurrentMethod().Name, 
                        StrFunc.AppendFormat("Trade n°{0} not found", currentId.ToString()), processState);
                }
                if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn) && (false == NoLockCurrentId))
                    CodeReturn = LockCurrentObjectId();
                if (ProcessStateTools.IsCodeReturnSuccess(CodeReturn))
                    CodeReturn = ScanCompatibility_Trade(currentId);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            SetTradeStatus(currentId, ProcessState.Status);
        }

        /// <summary>
        /// Execute le traitement pour le produit main du trade (ce produit peut être une strategie)
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        protected Cst.ErrLevel ProcessExecuteProduct(IProduct pProduct)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            Cst.ErrLevel masterCodeReturn = Cst.ErrLevel.SUCCESS;

            if (pProduct.productBase.IsStrategy)
            {
                foreach (IProduct product in ((IStrategy)pProduct).subProduct)
                {
                    codeReturn = ProcessExecuteProduct(product);
                    if (Cst.ErrLevel.SUCCESS != codeReturn)
                        masterCodeReturn = codeReturn;
                }
            }
            else if (pProduct.productBase.IsSwaption)
            {
                codeReturn = ProcessExecuteSimpleProduct(pProduct);
                if (Cst.ErrLevel.SUCCESS == codeReturn)
                    codeReturn = ProcessExecuteSimpleProduct((IProduct)((ISwaption)pProduct).swap);
                if (Cst.ErrLevel.SUCCESS != codeReturn)
                    masterCodeReturn = codeReturn;
            }
            else
            {
                masterCodeReturn = ProcessExecuteSimpleProduct(pProduct);
            }

            return masterCodeReturn;
        }

        /// <summary>
        /// Execute le traitement pour un produit simple (différent de strategie)
        /// <para>Le product peut être un subProduct d'une strategie</para>
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        protected virtual Cst.ErrLevel ProcessExecuteSimpleProduct(IProduct pProduct)
        {
            return Cst.ErrLevel.UNDEFINED;
        }
        #endregion Methods
    }
    #endregion ProcessTradeBase

    #region DiagnosticDebug
    /// <summary>
    /// Outils de diagnostique
    /// </summary>
    public class DiagnosticDebug
    {
        /// <summary>
        /// Activer la trace des différentes étapes du traitement
        /// </summary>
        public bool IsTraceSteps = true;
        private static TimerCollection m_Timers;

        public DiagnosticDebug(bool pIsTraceSteps)
        {
            IsTraceSteps = pIsTraceSteps;
            m_Timers = new TimerCollection();
        }

        /// <summary>
        /// Début de la trace, avec création d'un timer
        /// </summary>
        /// <param name="pName"></param>
        public void Start(string pName)
        {
            Start(pName, string.Empty);
        }
        /// <summary>
        /// Début de la trace, avec création d'un timer et écriture d'un Header avec le nom de l'étape
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pHeader"></param>
        public void Start(string pName, string pHeader)
        {
            if (IsTraceSteps)
            {
                m_Timers.CreateTimer(pName, true);

                if (StrFunc.IsFilled(pHeader))
                    Write(pName + pHeader);
            }
        }
        /// <summary>
        /// Fin de la trace, avec écriture du temps écoulé
        /// </summary>
        /// <param name="pName"></param>
        public void End(string pName)
        {
            End(pName, string.Empty);
        }
        /// <summary>
        /// Fin de la trace, avec écriture du temps écoulé et écriture d'un footer
        /// </summary>
        /// <param name="pName"></param>
        /// <param name="pFooter"></param>
        public void End(string pName, string pFooter)
        {
            if (IsTraceSteps)
            {
                System.Diagnostics.Debug.WriteLine(m_Timers.GetElapsedTime(pName, "sec", false));

                if (StrFunc.IsFilled(pFooter))
                    Write(pFooter);
            }
        }
        /// <summary>
        /// Ecriture d'un commentaire
        /// </summary>
        /// <param name="pComment"></param>
        public void Write(string pComment)
        {
            if (IsTraceSteps)
                System.Diagnostics.Debug.WriteLine(pComment);
        }
    }
    #endregion DiagnosticDebug
}
