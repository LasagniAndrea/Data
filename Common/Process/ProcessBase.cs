using EFS.ACommon;
//
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using EFS.SpheresService;
using EFS.Status;
using EFS.Tuning;
//
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace EFS.Process
{


    /// <summary>
    /// Flags de gestion du Dispose dans ProcessBase
    /// </summary>
    // EG 20180503 New
    public sealed class ProcessBaseDisposeFlags
    {
        public bool isUnlockSession;
        public bool isUnSetRestriction;
        public bool isDropWorkTable;
        public bool isDeleteTradeList;
        public bool isDeleteEventList;

        public ProcessBaseDisposeFlags()
        {
            isUnlockSession = false;
            isUnSetRestriction = false;
            isDropWorkTable = false;
            isDeleteTradeList = false;
            isDeleteEventList = false;
        }

        public ProcessBaseDisposeFlags(bool pIsUnlockSession, bool pIsUnSetRestriction, bool pIsDropWorkTable, bool pIsDeleteTradeList, bool pIsDeleteEventList)
        {
            isUnlockSession = pIsUnlockSession;
            isUnSetRestriction = pIsUnSetRestriction;
            isDropWorkTable = pIsDropWorkTable;
            isDeleteTradeList = pIsDeleteTradeList;
            isDeleteEventList = pIsDeleteEventList;
        }
    }

    /// <summary>
    ///  Classe de base pour les Process Spheres®
    ///  <Para>Process activé suite à l'arrivé d'un message Queue</Para>
    /// </summary>
    /// FI 20160920 [XXXXX] Classe déplacée ds la project common et allégée (de manière à être utilisée par les solutions gateways)
    /// Dans les solutions gateways il n'est plus nécessaire de partager les projets ESFML,EFMLGUI,CommonWeb,skmMenu, Ajax, etc)
    /// EG 20190613 [24683] Upd
    /// FI 20200623 [XXXXX] Partial 
    public abstract partial class ProcessBase
    {
        #region Enum
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
        public enum ProcessSlaveTypeEnum
        {
            None,
            Start,
            Execute,
            End,
        }
        #endregion Enum

        #region Members
        /// <summary>
        /// 
        /// </summary>
        /// EG 20190613 [24683] New
        public virtual int IdEParent_ForOffsettingPosition { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public SemaphoreSlim SemaphoreAsync { set; get; }
        /// <summary>
        /// 
        /// </summary>
        // EG 20190613 [24683] New
        public SemaphoreSlim SemaphoreAsync2 { set; get; }
        /// <summary>
        /// 
        /// </summary>
        public ParallelElementCollection CurrentParallelSettings { set; get; }


#if DEBUG
        /// <summary>
        /// Outils de diagnostique en mode Debug uniquement
        /// </summary>
        protected DiagnosticDebug diagnosticDebug = new DiagnosticDebug(false);
#endif


        /// <summary>
        /// Retourne le nom de la sémaphore d'interruption (IRQ-IDTRK_L)
        /// sans suffixe Global
        /// </summary>
        // EG 20190315 New version of Naming IRQ Semaphore IRQ{IDTRK_L}{SERVERNAME}{DATABASENAME}
        // EG 20190318 New version of Naming IRQ Semaphore IRQ{REQUESTTYPE.IDTRK_L}{SERVERNAME}{DATABASENAME}
        public string IRQNamedSystemSemaphore
        {
            get
            {
                if (StrFunc.IsFilled(Tracker.IrqProcess))
                    return Cst.ProcessTypeEnum.IRQ.ToString() + "{" + Tracker.IrqProcess + "." + Tracker.IdTRK_L + "}" + Tracker.IRQDatabase;
                else
                    return Cst.ProcessTypeEnum.IRQ.ToString() + "{" + Tracker.IdTRK_L + "}" + Tracker.IRQDatabase;
            }
        }

        // EG 20180503 New
        public ProcessBaseDisposeFlags DisposeFlags
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient la session qui lance le process
        /// </summary>
        public AppSession Session
        {
            get;
            private set;
        }


        /// <summary>
        /// Obtient le service qui lance le process
        /// </summary>
        public AppInstanceService AppInstance
        {
            get { return Session.AppInstance as AppInstanceService; }
        }
    

        /// <summary>
        /// Obtient ou définit l'Id sur lequel s'applique le traitement
        /// </summary>
        public int CurrentId
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public DataSet DsDatas
        {
            protected set;
            get;
        }

        /// <summary>
        /// Liste des lock posés pendant le process 
        /// </summary>
        public ArrayList LockObject
        {
            private set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public License License
        {
            private set;
            get;
        }
      
        /// 
        /// </summary>
        public MQueueBase MQueue
        {
            set;
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsModeSimul
        {
            get { return MQueue.GetBoolValueParameterById(MQueueBase.PARAM_ISSIMUL); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsProcessObserver
        {
            get => MQueue.IsMessageObserver;
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20121109 Demande via SCHEDULER
        public bool IsCreatedByNormalizedMessageFactory
        {
            get
            {
                Boolean ret = MQueue.IsMessageCreatedByNormMsgFactory;
                ret &= (false == MQueue.idSpecified) || (0 == MQueue.id);
                return ret;
            }
        }

        /// <summary>
        /// Type de process
        /// </summary>
        
        public Cst.ProcessTypeEnum ProcessType
        {
            get { return MQueue.ProcessType; }
        }

        /// <summary>
        /// Obteint la Cs issu de message queue
        /// </summary>
        public string Cs
        {
            get { return MQueue.ConnectionString; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProcessTuning ProcessTuning
        {
            protected set;
            get;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool ProcessTuningSpecified
        {
            get { return (null != ProcessTuning) && ProcessTuning.DrSpecified; }
        }


        /// <summary>
        /// Obtient le gestionnaire dédié à l'alimentation de la table SESSIONRESTRICT
        /// </summary>
        public SqlSessionRestrict SqlSessionRestrict
        {
            private set;
            get;
        }

        /// <summary>
        /// Représente l'état retour du process
        /// </summary>
        public ProcessState ProcessState
        {
            get;
            set;
        }

      
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
        protected virtual string DataIdent
        {
            get { return "N/A"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual TypeLockEnum DataTypeLock
        {
            get { return TypeLockEnum.NA; }
        }

        /// <summary>
        /// Obtient true si le process post un message
        /// <para>L'envoi effectif du message sera fonction de processTuning</para>
        /// </summary>
        protected virtual bool IsProcessSendMessage
        {
            get { return true; }
        }

        /// <summary>
        /// Transaction en cours sur le process appelant (en mode SLAVE)
        /// </summary>
        public IDbTransaction SlaveDbTransaction
        { get; set; }

        /// <summary>
        /// permet d'établir si le process est enfant d'un process appelant ou si le process est un process maître
        /// <para>Lorsque le porcess est enfant d'an autre process, il n'effectue pas de mise à jour dans PROCESS_L</para>
        /// </summary>
        public ProcessCallEnum ProcessCall
        { get; set; }

        /// <summary>
        /// permet d'établir si le mode du process enfant d'un process appelant (Start, Execute, End)
        /// </summary>
        public ProcessSlaveTypeEnum ProcessSlaveType
        { get; set; }

        /// <summary>
        /// permet de definir si l'envoi d'un message à un autre service est autorisé après traitement (ProcessTuning)
        /// Plus généralement utilisé à false pour les process enfants.
        /// </summary>
        public bool IsSendMessage_PostProcess
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
        /// Obtient l'identifiant non significatif de l'acteur requester
        /// <para>Lorsqu'il n'est pas précisé dans le message Queue, Spheres® retourne l'utisateur qui exécute appInstance</para>
        /// </summary>
        /// FI 20170323 [XXXXX] Add  (code qui existait avant dans la class ioTask)
        public int UserId
        {
            get
            {
                int ret = 0;
                if (MQueue.header.requester != null)
                {
                    if (MQueue.header.requester.idASpecified)
                    {
                        if (MQueue.header.requester.idA >= 0)
                            ret = MQueue.header.requester.idA;
                    }
                }

                if (0 == ret)
                    ret = Session.IdA;

                return ret;
            }
        }

        // EG 20180503 New
        public bool IsUnlockSession
        { get { return (null == DisposeFlags) || DisposeFlags.isUnlockSession; } }
        // EG 20180503 New
        public bool IsUnSetRestriction
        { get { return (null == DisposeFlags) || DisposeFlags.isUnSetRestriction; } }
        // EG 20180503 New
        public bool IsDropWorkTable
        { get { return (null == DisposeFlags) || DisposeFlags.isDropWorkTable; } }
        // EG 20180503 New
        public bool IsDeleteTradeList
        { get { return (null == DisposeFlags) || DisposeFlags.isDeleteTradeList; } }
        // EG 20180503 New
        public bool IsDeleteEventList
        { get { return (null == DisposeFlags) || DisposeFlags.isDeleteEventList; } }


        #endregion Accessors

        #region Asynchrone configuration
        public int GetHeapSize(ParallelProcess pName)
        {
            return CurrentParallelSettings[pName].HeapSize;
        }
        // EG 20181127 PERF Post RC (Step 3)
        public int GetHeapSizeEvents(ParallelProcess pName)
        {
            Nullable<int> heapSizeEvents = CurrentParallelSettings[pName].HeapSizeEvents;
            if (false == heapSizeEvents.HasValue)
                heapSizeEvents = CurrentParallelSettings[pName].HeapSize;
            return heapSizeEvents.Value;
        }
        public int GetMaxThreshold(ParallelProcess pName)
        {
            return CurrentParallelSettings[pName].MaxThreshold;
        }
        // EG 20181127 PERF Post RC (Step 3)
        public int GetMaxThresholdEvents(ParallelProcess pName)
        {
            Nullable<int> maxThresholdEvents = CurrentParallelSettings[pName].MaxThresholdEvents;
            if (false == maxThresholdEvents.HasValue)
                maxThresholdEvents = CurrentParallelSettings[pName].MaxThreshold;
            return maxThresholdEvents.Value;
        }
        public bool IsParallelProcess(ParallelProcess pName)
        {
            return (null != CurrentParallelSettings) && (null != CurrentParallelSettings[pName]);
        }

        public void InitializeMaxThreshold(ParallelProcess pName)
        {
            SemaphoreAsync = null;
            SemaphoreAsync = new SemaphoreSlim(CurrentParallelSettings[pName].MaxThreshold);
        }
        // EG 20190613 [24683] New
        public void InitializeMaxThreshold2(ParallelProcess pName)
        {
            SemaphoreAsync2 = null;
            SemaphoreAsync2 = new SemaphoreSlim(CurrentParallelSettings[pName].MaxThreshold);
        }
        // EG 20181127 PERF Post RC (Step 3)
        public void InitializeMaxThresholdEvents(ParallelProcess pName)
        {
            SemaphoreAsync = null;
            int maxThresholdEvents = GetMaxThresholdEvents(pName);
            SemaphoreAsync = new SemaphoreSlim(maxThresholdEvents);
        }
        public bool IsSlaveCallEvents(ParallelProcess pName)
        {
            return CurrentParallelSettings[pName].IsSlaveCallEvents;
        }

        #endregion Asynchrone Configuration

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        public ProcessBase()
        {
            ProcessCall = ProcessCallEnum.Master;
            IsSendMessage_PostProcess = true;
            
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


            // PL/FDA 20120202 Ligne suivante permettant pour des tests de forcer le cache OFF
            //_mQueue.ConnectionString = CSTools.SetCacheOff(_mQueue.ConnectionString);
            //
            ProcessCall = ProcessCallEnum.Master;
            //
            MQueue = pMQueue;
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

            this.Session = new AppSession(pAppInstance)
            {
                IdA = 1,
                SessionId = SystemTools.GetNewGUID()
            };


            SqlSessionRestrict = new SqlSessionRestrict(Cs, Session);
            ProcessState = new ProcessState(ProcessStateTools.StatusUnknownEnum, ProcessStateTools.CodeReturnSuccessEnum);
            LogDetailEnum = LogLevelDetail.LEVEL3;
        }

        #endregion Constructors

        #region Methods
    
        #region SetCurrentParallelSettings
        // EG 20180413 [23769] Gestion customParallelConfigSource
        public virtual void SetCurrentParallelSettings()
        {
            CurrentParallelSettings = null;
        }
        #endregion SetCurrentParallelSettings

        #region SetTradeStatus
        /// <summary>
        /// Update status on the trade (TRADE (ex ...STSYS), TRADESTCHECK, TRADESTMATCH)
        /// </summary>
        /// <param name="pStatusProcess"></param>
        /// <returns></returns>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE)
        protected virtual bool SetTradeStatus(int pIdT, ProcessStateTools.StatusEnum pStatusProcess)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
            if (ProcessTuningSpecified)
            {
                //Write Trade Status from PROCESSTUNING for OutputTradeSuccess/OutputTradeError
                switch (pStatusProcess)
                {
                    case ProcessStateTools.StatusEnum.SUCCESS:
                        errLevel = ProcessTuning.WriteStatus(Cs, null, TuningOutputTypeEnum.OTS, pIdT, Session.IdA);
                        break;
                    case ProcessStateTools.StatusEnum.ERROR:
                        errLevel = ProcessTuning.WriteStatus(Cs, null, TuningOutputTypeEnum.OTE, pIdT, Session.IdA);
                        break;
                }
            }
            return (errLevel == Cst.ErrLevel.SUCCESS);

        }
        #endregion SetTradeStatus
        #region ScanCompatibility_Trade
        /// <summary>
        /// ScanCompatibility: Scan si le trade est compatible avec le référentiel PROCESSTUNING
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Use slaveDbTransaction
        public Cst.ErrLevel ScanCompatibility_Trade(int pIdT)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (ProcessTuningSpecified)
            {
                TradeStatus tradeStatus = new TradeStatus();
                tradeStatus.Initialize(Cs, pIdT);
                //FI 20190524 [23912] Alimentation du log en mode FULL 
                ret = ProcessTuning.ScanTradeCompatibility(Cs, pIdT, tradeStatus, out string msgControl);
                if (StrFunc.IsFilled(msgControl))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, msgControl));
                }
            }
            return ret;
        }
        #endregion ScanCompatibility_Trade
        #region ScanCompatibility_Event
        /// <summary>
        /// ScanCompatibility: Scan si le EVENT est compatible avec le référentiel PROCESSTUNING
        /// </summary>
        /// <returns></returns>
        public Cst.ErrLevel ScanCompatibility_Event(int pIde)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (ProcessTuningSpecified)
            {
                EventStatus eventStatus = new EventStatus();
                eventStatus.Initialize(Cs, pIde);
                //FI 20190524 [23912] Alimentation du log en mode FULL 
                ret = ProcessTuning.ScanEventCompatibility(Cs, pIde, eventStatus, out string msgControl);
                if (StrFunc.IsFilled(msgControl))
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Debug, msgControl));
                }
            }
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
            return LockObjectId(DataTypeLock, CurrentId, MQueue.identifier, MQueue.ProcessType.ToString(), pLockMode);
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
        // EG 20181010 Add IRQ
        public Cst.ErrLevel LockObjectId(TypeLockEnum pTypeLock, int pId, string pIdentifier, string pAction, string pLockMode)
        {
            return LockObjectId(pTypeLock, pId.ToString(), pIdentifier, pAction, pLockMode);
        }

        // EG 20190114 Add detail to ProcessLog Refactoring
        public Cst.ErrLevel LockObjectId(TypeLockEnum pTypeLock, string pId, string pIdentifier, string pAction, string pLockMode)
        {
#if DEBUG
            diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            if (false == IRQTools.IsIRQRequested(this, IRQNamedSystemSemaphore, ref ret))
            {
                if (TypeLockEnum.NA != pTypeLock)
                {
                    // EG 20151123 ObjectId = string
                    LockObject lockObject = new LockObject(pTypeLock, pId, pIdentifier, pLockMode);
                    // PM 20200102 [XXXXX] New Log : utilisation de DtProfiler à la place de ProcessLog pour gérer le temps écoulé
                    //// RD 20120809 [18070] Optimisation de la compta / Utilisation de processLog.GetDate()
                    //Lock lck = new Lock(Cs, lockObject, appInstance, pAction, processLog.GetDate());
                    // FI 20200812 [XXXXX] Usage de OTCmlHelper.GetDateSys(Cs);   
                    //Lock lck = new Lock(Cs, lockObject, AppInstance, pAction, DtProfiler.GetDate());
                    Lock lck = new Lock(Cs, lockObject, Session, pAction);
                    if (false == LockTools.LockMode2(lck, out Lock lckExisting))
                    {
                        if (null != lckExisting)
                        {
                            // EG 20130620 Génération du message avec DetailEnum = DEFAULT
                            // FI 20141125 [20230] Mise en place d'un message dans le log avec status WARNING, isProcessStateStatutUpdOnErrWarning est postionné à false;
                            // Avec le code précédent Spheres® terminait une erreur, et le message était reposté 
                            //    => Au final, une fois le lock levé, le traitement était en succès mais le tracker en erreur
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Warning, lckExisting.ToString()));
                        }
                        lockObject = null; // Use For Not Unlock
                        ret = Cst.ErrLevel.LOCKUNSUCCESSFUL;
                    }
                    if (ProcessStateTools.IsCodeReturnSuccess(ret))
                    {
                        if (ArrFunc.IsEmpty(LockObject))
                            LockObject = new ArrayList();
                        LockObject.Add(lockObject);
                    }
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
            UnLockObjectId(CurrentId);
        }
        #endregion UnLockCurrentObjectId
        #region UnLockObjectId
        /// <summary>
        /// ?????? 
        /// </summary>
        /// <param name="pId"></param>
        public void UnLockObjectId(int pId)
        {
            if (ArrFunc.IsFilled(LockObject))
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

                foreach (LockObject item in LockObject)
                {
                    if ((null != item) && ((false == IsMonoDataProcess) || (item.ObjectId == pId.ToString())))
                        LockTools.UnLock(Cs, item, Session.SessionId);
                }
                LockObject.Clear();
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
                // EG 20151123 RequestType instead of mQueue.ProcessType.ToString()
                String action = MQueue.ProcessType.ToString();
                if (MQueue is PosKeepingRequestMQueue)
                    action = (MQueue as PosKeepingRequestMQueue).requestType.ToString();
                Lock lck = new Lock(Cs, lockObject, Session, action);
                if (false == LockTools.LockMode1(lck, out Lock lckExisting))
                {

                    if (null != lckExisting)
                    {
                        if (pIsControlSession)
                            isLockBySameProcess = (lckExisting.Session.SessionId == Session.SessionId);
                        if (false == isLockBySameProcess)
                        {
                            
                            Logger.Log(new LoggerData(LogLevelEnum.Info, lckExisting.ToString()));
                        }
                    }
                    if (false == isLockBySameProcess)
                        lockObject = null; // Use For Not Unlock
                }
            }
            return lockObject;
        }
        #endregion LockElement



        /// <summary>
        /// UnLockSession, UnSetRestriction
        /// </summary>
        /// FI 20120831 
        /// FI 20170420 [23075] Modify
        /// EG 20180503 New
        /// EG 20210913 [XXXXX] Call CleanTemporaryEmptySubDirectory
        protected void CleanUp()
        {

            if (IsUnlockSession)
                LockTools.UnLockSession(Cs, Session.SessionId);

            //FI 20120831 use  _sqlSessionRestrict.UnSetRestriction à la place de SqlSessionRestrict.Dispose
            if (IsUnSetRestriction)
                SqlSessionRestrict.UnSetRestriction();


            // FI 20170420 [23075] DropWorkTable 
            if (IsDropWorkTable)
                Session.DropWorkTable(Cs);

            // FI 20170420 [23075] call 
            if (IsDeleteEventList)
                EventRDBMSTools.DeleteEventList(Cs, Session.SessionId);

            if (IsDeleteTradeList)
                TradeRDBMSTools.DeleteTradeList(Cs, Session.SessionId);
            // EG 20231030 [WI732] Delete Temporary Files and subdirectories
            AppInstance.CleanTemporaryDirectory();
        }

        #region PostProcess_SendMessage
        //protected bool PostProcess_SendMessage()
        protected int PostProcess_SendMessage()
        {
            int _nbPostedSubMsg = 0;
            //bool isMessageSended = false;
            // RD 20091221 [16803]
            if (ProcessTuningSpecified)
            {
#if DEBUG
                diagnosticDebug.Start(MethodInfo.GetCurrentMethod().Name);
#endif

                Cst.ProcessTypeEnum[] postProcess_SendMessageTypeArray = new Cst.ProcessTypeEnum[3] { Cst.ProcessTypeEnum.NA, Cst.ProcessTypeEnum.NA, Cst.ProcessTypeEnum.NA };

                //Get SendMessage type from PROCESSTUNING for OutputTradeSuccess/OutputTradeError
                bool isExistSendMessage = false;
                switch (ProcessState.Status)
                {
                    case ProcessStateTools.StatusEnum.SUCCESS:
                        isExistSendMessage = ProcessTuning.Get_SendMessage(TuningOutputTypeEnum.OTS, ref postProcess_SendMessageTypeArray);
                        break;
                    case ProcessStateTools.StatusEnum.ERROR:
                        isExistSendMessage = ProcessTuning.Get_SendMessage(TuningOutputTypeEnum.OTE, ref postProcess_SendMessageTypeArray);
                        break;
                }

                if (isExistSendMessage) //PL 20120824 Add if() for tuning
                {
                    foreach (Cst.ProcessTypeEnum postProcess_SendMessageType in postProcess_SendMessageTypeArray)
                    {
                        bool isAvailable = (Cst.ProcessTypeEnum.NA != postProcess_SendMessageType);
                        // EG 20120203 Add 
                        isAvailable &= (ProcessCallEnum.Master == ProcessCall) || IsSendMessage_PostProcess;

                        // EG 20111003 Pas d'envoi de message de type POSKEEPENTRY si GPRODUCT <> ETD / FUT plus tard SEC
                        if (Cst.Process.IsPosKeepingEntry(postProcess_SendMessageType))
                        {
                            string _product = MQueue.GetStringValueIdInfoByKey("GPRODUCT");
                            isAvailable &= ((!string.IsNullOrEmpty(_product)) && (Cst.ProductGProduct_FUT == _product));
                        }

                        if (isAvailable)
                        {
                            MQueueSendInfo sendInfo = ServiceTools.GetMqueueSendInfo(postProcess_SendMessageType, AppInstance);

                            if (sendInfo.IsInfoValid)
                            {
                                MQueueBase mQueueTarget = MQueueTools.GetMQueueByProcess(postProcess_SendMessageType);
                                //Add Parameters
                                MQueueparameters mqParameters = new MQueueparameters();
                                if (MQueue.parametersSpecified)
                                {
                                    for (int i = 0; i < MQueue.parameters.Count; i++)
                                    {
                                        if (mQueueTarget.IsExistParameter(MQueue.parameters[i].id))
                                            mqParameters.Add(MQueue.parameters[i]);
                                    }
                                }
                                //Transfert de l'Id vers le process suivant 
                                //uniquement s'il est un IDT (TRADE) 
                                //et que le process suivant s'attend à recevoir en Id un IDT (TRADE)
                                int mQueueId = 0;
                                IdInfo mQueueIdInfoTarget = new IdInfo() {
                                    id = mQueueId
                                };
                                if (MQueue.IsIdT)
                                {
                                    if (mQueueTarget.IsIdT)
                                    {
                                        mQueueIdInfoTarget.id = CurrentId;
                                        if (MQueue.idInfoSpecified)
                                            mQueueIdInfoTarget.idInfos = MQueue.idInfo.idInfos;
                                    }
                                    else
                                    {
                                        if (mQueueTarget.IsExistParameter(MQueueBase.PARAM_IDT))
                                        {
                                            // IDT est transféré comme paramètre au service suivant
                                            // A ce dernier de le considérer ou non
                                            MQueueparameter mqp = new MQueueparameter(MQueueBase.PARAM_IDT, TypeData.TypeDataEnum.integer)
                                            {
                                                Value = CurrentId.ToString()
                                            };
                                            mqParameters.Add(mqp);
                                        }
                                    }
                                }
                                //
                                if (Cst.Process.IsConfirmationMsgGenIO(postProcess_SendMessageType))
                                {
                                    MQueueparameter mqp = new MQueueparameter
                                    {
                                        id = MQueueBase.PARAM_ISWITHIO,
                                        dataType = TypeData.TypeDataEnum.@bool
                                    };
                                    mqp.SetValue(true);
                                    mqParameters.Add(mqp);
                                }

                                MQueueTools.Send(postProcess_SendMessageType, Cs, mQueueIdInfoTarget, mqParameters, MQueue.header.requester, sendInfo);

                                Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 301), 1,
                                    new LogParam(Ressource.GetString(postProcess_SendMessageType.ToString())),
                                    new LogParam(postProcess_SendMessageType)));

                                // EG 20220519 [WI637] Gestion compteur PostedSubMsg sur traitement Slave
                                if (ProcessCall == ProcessCallEnum.Slave)
                                    Tracker.AddPostedSubMsg(1, Session);
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

        #region GetProcessState
        /// <summary>
        /// Renvoi le status du log à considérer en fonction du code retourné {pCodeReturn}
        /// </summary>
        /// <param name="pCodeReturn"></param>
        /// EG 20150306 [POC-BERKELEY] : Add FAILUREWARNING
        // EG 20180525 [23979] IRQ Processing
        public ProcessStateTools.StatusEnum GetProcessStatus(Cst.ErrLevel pCodeReturn)
        {
            ProcessStateTools.StatusEnum ret;
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
                // FI 20220202 [XXXXX] Add
                case Cst.ErrLevel.SERVICE_STOPPED:  //                        => Remise in Queue 
                    ret = ProcessStateTools.StatusPendingEnum;
                    break;
                case Cst.ErrLevel.ACCESDENIED:      //Licence incorrect
                    ret = ProcessStateTools.StatusErrorEnum;
                    break;
                case Cst.ErrLevel.TIMEOUT:          //Timeout                => Remise in Queue 
                    // EG 20130724  On ne repostera pas le message dans le cas de SpheresIO avec processLog instancié

                    // FI 20200623 [XXXXX] suppression du test sur (null != _processLog)
                    //if (appInstance.AppName.ToLower().Contains("spheresio") && (null != _processLog))
                    if (AppInstance.AppName.ToLower().Contains("spheresio"))
                        ret = ProcessStateTools.StatusErrorEnum;
                    else
                        ret = ProcessStateTools.StatusPendingEnum;
                    break;
                case Cst.ErrLevel.FAILUREWARNING:
                    // EG 20150305 New
                    ret = ProcessStateTools.StatusWarningEnum;
                    break;
                case Cst.ErrLevel.IRQ_EXECUTED:
                    ret = ProcessStateTools.StatusInterruptEnum;
                    break;
                default:
                    ret = ProcessStateTools.StatusErrorEnum;
                    break;
            }
            return ret;
        }
        #endregion GetProcessState



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
        /// EG 20180525 [23979] IRQ Processing
        /// EG 20190114 Add detail to ProcessLog Refactoring
        /// FI 20200916 [XXXXX] Reecriture 
        /// Dans cette nouvelle version, il n'existe plus la boucle qui poste n Messages si le message n'a pas de id specifié) 
        /// Dans un tel contexte il faut désormais obligatoirement faire appel à NormmsgFactory qui se chargera de générer le bon MessageQueue 
        /// D'aures part si exception, elle est systématiquement ajoutée dans le log 
        public void ProcessStart()
        {
            try
            {

                ProcessInitializeMqueue();
                CurrentId = MQueue.id;
                ProcessInitialize();
                ProcessPreExecute();

                if (IsProcessObserver)
                {
                    // FI 20200709 [25473] En mode Observer => Ajout dans AttachedDoc des assemblies et du fichier de configuration
                    AddAssembliesAttachedDoc();
                    AddConfigAttachedDoc();
                    AddTraceFileAttachedDoc();
                }
                else if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                {
                    ProcessState.CodeReturn = ProcessExecuteSpecific();
                }
                else if (ProcessStateTools.IsCodeReturnIgnoreTuning(ProcessState.CodeReturn) && (ProcessCall == ProcessCallEnum.Master))
                {
                    // FI 20200623 [XXXXX] call SetErrorWarning
                    ProcessState.SetErrorWarning(ProcessStateTools.StatusWarningEnum);


                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 7), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));
                }
                else if (ProcessStateTools.IsCodeReturnUnmatchTuning(ProcessState.CodeReturn) && (ProcessCall == ProcessCallEnum.Master))
                {


                    Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.SYS, 8), 0, new LogParam(LogTools.IdentifierAndId(MQueue.Identifier, CurrentId))));

                    //FI 20190524 [23912] Purge du cache
                    //Indispensable car le message est reposté et retraité => Permet de prendre en considération des potentiels évolutions dans le paramétrage de ProcessTuning
                    DataHelper.queryCache.Remove(Cst.OTCml_TBL.PROCESSTUNING.ToString(), Cs, true);
                }
                else if (ProcessStateTools.IsCodeReturnLockUnsuccessful(ProcessState.CodeReturn))
                {
                    Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 50), 0));
                }

                if ((Cst.ErrLevel.IRQ_EXECUTED == ProcessState.CodeReturn) && (ProcessCall == ProcessCallEnum.Master))
                    ProcessLogIRQ();

                // EG 21030722
                if (Cst.ErrLevel.SUCCESS != ProcessState.CodeReturn)
                    ProcessState.SetCodeReturnFromLastException2();

            }
            catch (Exception ex)
            {
                SpheresException2 oEx = SpheresExceptionParser.GetSpheresException(null, ex);
                ProcessState.AddException(oEx);
                ProcessState.SetCodeReturnFromLastException2();

                // RD 20130104 [18336] S'il existe un problème lors de l'initialisation des Objets PROCESS_L, TRACKER_L, ...
                // Alors renvoyer l'exception pour l'écrire dans l'EventViewer
                
                if (IsLogAvailable)
                {
                    Logger.Log(new LoggerData(oEx));
                }
                else
                {
                    // FI 20220719 [XXXXX] Alimentation de la trace (Remarque le logger alimente également la trace lorsqu'il est actif)
                    Common.AppInstance.TraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                    throw;
                }
            }
            finally
            {
                ProcessEnd();
            }
        }

        #endregion ProcessStart
        #region ProcessSlave
        /// <summary>
        /// Démarre le process en mode Slave
        /// </summary>
        /// FI 20170406 [23053] Modify
        public void ProcessSlave()
        {
            // Traitement specific du process
            bool isProcessException = false;
            try
            {
                CurrentId = MQueue.id;
                if (ProcessSlaveTypeEnum.Start == ProcessSlaveType)
                {
                    ProcessInitialize();
                    ProcessPreExecute();
                }
                if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn) && (ProcessSlaveTypeEnum.Execute == ProcessSlaveType))
                {
                    ProcessState.CodeReturn = ProcessExecuteSpecific();
                }
            }
            catch (Exception ex)
            {
                isProcessException = true;

                SpheresException2 sEX = SpheresExceptionParser.GetSpheresException(null, ex);
                // FI 20200623 [XXXXX] call AddCriticalException
                ProcessState.AddCriticalException(ex);

                // FI 20200916 [XXXXX] je ne sais pas pourquoi ce if particulier et pourquoi Spheres® utilise le code retour BREAK (je laisse en l'état)
                if (ex.GetType().Equals(typeof(SpheresException2)))
                    ProcessState.SetProcessState(sEX.ProcessState.Status, (sEX.ProcessState.CodeReturn == Cst.ErrLevel.SUCCESS ? Cst.ErrLevel.BREAK : sEX.ProcessState.CodeReturn));
                else
                    ProcessState.SetProcessState(sEX.ProcessState.Status, Cst.ErrLevel.BREAK);

                if (IsLogAvailable)
                {
                    Logger.Log(new LoggerData(sEX));
                }
                else
                {
                    // FI 20220719 [XXXXX] Alimentation de la trace (Remarque le logger alimente également la trace lorsqu'il est actif)
                    AppInstance.AppTraceManager.TraceError(this, ExceptionTools.GetMessageAndStackExtended(ex));
                    throw;
                }
            }
            finally
            {
                if (isProcessException || (ProcessSlaveType == ProcessSlaveTypeEnum.End))
                {
                    ProcessEnd();
                }
                else if (ProcessSlaveType == ProcessSlaveTypeEnum.Execute)
                    UnLockCurrentObjectId();
            }
        }
        #endregion ProcessSlave
        #region ProcessInitialize
        /// <summary>
        /// Initialisation des variables à chaque changement de Id 
        /// Nouveau Log, nouveau Tracker, etc...
        /// </summary>
        // EG 20181010 PERF GetSvrInfoConnection
        // EG 20190114 Add detail to ProcessLog Refactoring
        protected virtual void ProcessInitialize()
        {
            SqlSessionRestrict = new SqlSessionRestrict(Cs, Session);
            
            ProcessState = new ProcessState(ProcessStateTools.StatusProgressEnum, ProcessStateTools.CodeReturnUndefinedEnum);

            // FI 20190605 [XXXXX] Le tracker n'est plus initilisé en mode slave
            // Il doit être valorisé en amont
            if (ProcessCall == ProcessCallEnum.Master)
                SetTracker();

            //Attention, cette méthode doit être appelée avant l'insert dans PROCESS_L
            //Ne pas déplacer
            bool isCacheCleared = false;
            // FI 20190605 [XXXXX] Pas de purge de cache en mode slave
            if (ProcessCall == ProcessCallEnum.Master)
            {
                isCacheCleared = InitializeQueryCache();
            }

            /* FI 20240731 [XXXXX] Mise en commentaire
            // FI 20120712 Les enums sont chargés dans le cache, 
            // Les enums restent dans le cache tant que Spheres® traite les message issus d'un même request
            // Avant les enums étaients chargé toutes les 5 minutes (cache natif à la classe  LoadFpMLEnumsAndSchemes)
            // Cela pénalisant les longs traitements (exemple EAR) 
            // RD 20120809 [18070] Optimisation de la compta
            if (isCacheCleared || (ExtendEnumsTools.ListEnumsSchemes == default(ExtendEnums)))
            {
                //PL 20120822 Add and use isCacheCleared
                //PM 20120824 Add test to check if ExtendEnumsTools.ListEnumsSchemes is initialized
                ExtendEnumsTools.LoadFpMLEnumsAndSchemes(CSTools.SetCacheOn(Cs));
            }
            */
            //  FI 20240731 [XXXXX] call DataEnabledEnum.ClearCache
            if (isCacheCleared)
                DataEnabledEnum.ClearCache(Cs);

            // when a processlog instance is already available, this process log instancee is shared by all the built process
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // PM 20200102 [XXXXX] New Log : Ne pas utiliser ProcessLog si le Logger est actif
                // Initialisation de m_IdProcess pour compatibilité
                if (LoggerManager.IsEnabled)
                {
                    // PM 20200102 [XXXXX] Création d'un nouveau LogScope compatible avec PROCESS_L
                    NewLogScope();

                    // FI 20201106 [XXXXX] Alimentation de Tracker.processRequested et de idProcess
                    if (IsLogAvailable)
                    {
                        Tracker.ProcessRequested = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), Logger.CurrentScope.LogScope.InitialProcessType);
                        Tracker.IdProcess = IdProcess;
                    }
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessLogIRQ
        /// <summary>
        /// Ecriture dans le log de l'exécution de la demande d'interruption du traitement
        /// + Eventuelle suppression de la sémaphore dans la collection
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        private void ProcessLogIRQ()
        {
            IRQSemaphore irqSemaphore = IRQTools.GetCurrentIRQSemaphore(IRQNamedSystemSemaphore);

            // FI 20200819 [XXXXX] Mise en place des variables requestedBy et requestedAt
            string requestedBy = LogTools.IdentifierAndId(irqSemaphore.userRequester.Item2, irqSemaphore.userRequester.Item1);

            string requestedAt = DtFunc.DateTimeToStringISO(irqSemaphore.userRequester.Item3);
            if (irqSemaphore.userRequester.Item3.Kind == DateTimeKind.Utc)
                requestedAt += " Etc/UTC";

            
            Logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 9920), 0,
                new LogParam(irqSemaphore.requestId),
                new LogParam(requestedBy),
                new LogParam(requestedAt),
                new LogParam(irqSemaphore.applicationRequester.Item1),
                new LogParam(irqSemaphore.applicationRequester.Item2),
                new LogParam(irqSemaphore.applicationRequester.Item3)));

            irqSemaphore.semaphore.Close();
            if (ProcessCall == ProcessCallEnum.Master)
                AppInstance.LstIRQSemaphore.Remove(irqSemaphore);

        }
        #endregion ProcessLogIRQ

        #region ProcessFinalize
        /// <summary>
        ///  Fin du process
        ///   <para>- Ecriture dans le journal des logs (PROCESS_L)</para> 
        ///   <para>- Mise à jour du tracker (avec envoi d'accusé de recéception si la demande initiale est totalement traitée)</para>
        ///  Ces actions sont opérées lorsque le process est maître
        /// </summary>
        /// FI 20160412 [XXXXX] Modify
        /// FI 20170406 [23053] Modify (la méthode n'et plus virtuelle)
        protected void ProcessFinalize()
        {
            if (ProcessCall == ProcessCallEnum.Master)
            {
                // FI 20160412 [XXXXX] Tentative de mise à jour du Tracker et envoi des accusé de reception dans le finally 
                // => de manière à exécuter les tâches finales associées au tracker même lorsque la mise à jour des Logs plante (PROCESS_L et PROCESSDET_L)
                try
                {
                    LoggerFinalize();
                }
                finally
                {
                    TrackerFinalize();
                }
            }
        }
        #endregion ProcessFinalize
        #region ProcessPreExecute
        /// <summary>
        /// Verification ultime avant execution du traitement.
        /// Cette méthode doit être overridée pour appliquer : 
        /// <para>- Mise en place d'un lock</para>
        /// <para>- Verification du respect par rapport à processTuning</para>
        /// </summary>
        protected virtual void ProcessPreExecute()
        {
            ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;
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
        /// <summary>
        /// 
        /// </summary>
        /// FI 20220202 [XXXXX] Add Method
        public void ProcessEnd()
        {
            try
            {
                ProcessTerminate();
                ProcessFinalize();
            }
            finally
            {
                CleanUp();
            }
        }
        #region GetInfoIdE
        /// <summary>
        /// Retourne la description de l'évènement {pIdE}
        /// </summary>
        /// <param name="pIdE"></param>
        /// <returns></returns>
        public virtual string GetInfoIdE(int pIdE)
        {
            string ret = string.Empty;
            SQL_Event sqlEvent = new SQL_Event(Cs, pIdE);
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
            SQL_Event sqlEvent = new SQL_Event(Cs, pIdE);
            sqlEvent.LoadTable(new string[] { "IDE", "EVENTCODE", "EVENTTYPE" });
            if (sqlEvent.IsLoaded)
                ret = new Pair<int, Pair<string, string>>(sqlEvent.Id, new Pair<string, string>(sqlEvent.EventCode, sqlEvent.EventType));
            return ret;
        }
        #endregion GetInfoEvent

        #region ProcessTerminate
        /// <summary>
        /// Mise à jour du log, de _processState
        /// <para>Envoie MsgQueue au process suivant</para>
        /// </summary>
        // EG 20180525 [23979] IRQ Processing
        protected void ProcessTerminate()
        {

            bool isSendMessage = false;

            try
            {
                // RD 20120626 [17950] Task: Execution of the element under condition
                // Partager du code via processLog.GetProcessState(), avec Task.GetCurrentElementReturnCode()
                // EG 20130722 Add DeadLock

                switch (ProcessState.CodeReturn)
                {
                    case Cst.ErrLevel.LOCKUNSUCCESSFUL: //Operation non traitée   => Remise in Queue
                    case Cst.ErrLevel.TUNING_UNMATCH:   //Operation non traitée   => Remise in Queue                        
                    case Cst.ErrLevel.SERVICE_STOPPED:  //Operation non traitée   => Remise in Queue                         
                    case Cst.ErrLevel.TUNING_IGNORE:    //Operation Ignorée       => Mise in Success
                    case Cst.ErrLevel.ACCESDENIED:      //Licence incorrect
                        ProcessState.SetProcessState(GetProcessStatus(ProcessState.CodeReturn), ProcessState.CodeReturn);
                        break;
                    case Cst.ErrLevel.DEADLOCK:         //Deadlock                => Remise in Queue
                    case Cst.ErrLevel.TIMEOUT:          //Timeout                 => Remise in Queue
                        // EG 20130724
                        //processLog.GetProcessState(CodeReturn, out logStatus);
                        ProcessStateTools.StatusEnum status = GetProcessStatus(ProcessState.CodeReturn);
                        ProcessState.Status = status;
                        ProcessState.CurrentStatus = status;
                        break;
                    case Cst.ErrLevel.BREAK:             //=>  exception occured, process interrupted
                        break;
                    case Cst.ErrLevel.SUCCESS:
                    case Cst.ErrLevel.NOTHINGTODO:
                    case Cst.ErrLevel.TUNING_IGNOREFORCED://Operation Ignorée en DUR => Mise in Success
                    case Cst.ErrLevel.ENTITYMARKET_UNMANAGED: //Operation Ignorée en DUR => Mise in Success
                        ProcessState.SetProcessState(GetProcessStatus(ProcessState.CodeReturn), ProcessState.CodeReturn);
                        ProcessTerminateSpecific();
                        //FI 20190524 [23912] Pas d'envoi de message au processs suivant si NOTHINGTODO
                        isSendMessage = (ProcessState.CodeReturn != Cst.ErrLevel.NOTHINGTODO);
                        break;
                    case Cst.ErrLevel.IRQ_EXECUTED:
                        ProcessState.SetProcessState(ProcessStateTools.StatusInterruptEnum, ProcessState.CodeReturn);
                        ProcessTerminateSpecific();
                        break;
                    default:  // necessarily error occured
                        // EG 20130724
                        //processLog.GetProcessState(CodeReturn, out logStatus);
                        ProcessState.SetProcessState(GetProcessStatus(ProcessState.CodeReturn), ProcessState.CodeReturn);
                        ProcessTerminateSpecific();
                        isSendMessage = true;
                        break;
                }
            }

            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] call AddCriticalException
                ProcessState.AddCriticalException(ex);
                ProcessState.SetProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum);

                if (IsLogAvailable)
                {
                    Logger.Log(new LoggerData(SpheresExceptionParser.GetSpheresException(null, ex)));
                }
                else
                    throw;
            }

            // Send Message
            try
            {
                if ((isSendMessage) && (this.IsProcessSendMessage))
                {
                    ProcessState.PostedSubMsg += PostProcess_SendMessage();
                    
                }
            }
            catch (Exception ex)
            {
                // FI 20200623 [XXXXX] call AddCriticalException
                ProcessState.AddCriticalException(ex);

                ProcessState.SetProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnFailureEnum);

                if (IsLogAvailable)
                {
                    
                    Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 6), 3,
                        new LogParam("Send Message Error"),
                        new LogParam(ex.Message),
                        new LogParam(ex.Source)));
                }
            }


            UnLockCurrentObjectId();
        }
        #endregion ProcessTerminate



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
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void Send_RetrieveOnError(MQueueBase pMQueue, MQueueSendInfo pSendInfo, double pTimeout)
        {

            int nbAttemps = 0;
            MQueueTools.Send(pMQueue, pSendInfo, pTimeout, ref nbAttemps);
            if (nbAttemps > 1)
            {
                //Il a fallu plusieurs tentative pour réussir à ouvrir la queue.
                string path = pSendInfo.MOMSetting.MOMPath;

                //ProcessLogInfo logInfo = null;
                //Injection dans le log d'un message d'erreur (le traitement finira donc en warning)
                string[] info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Path: {0}]", path);
                info[1] = StrFunc.AppendFormat(@"[Failed attempts to access: {0}]", (nbAttemps - 1).ToString());

                string errorMsg = "[Code Return:" + Cst.Space + Cst.ErrLevel.MOM_PATH_ERROR.ToString() + Cst.Space + "]" + Cst.CrLf2;
                errorMsg += @"MSMQ is unreachable" + Cst.CrLf;
                errorMsg += info[0] + Cst.CrLf;
                errorMsg += info[1] + Cst.CrLf;

                //STATUT N/A
                //car finalement Spheres® accède à la queue (après plusieurs tentatives certes)
                //Dans ce cas de figure, il existe des pbs réseaux => ce message sera récupéré par les services pour injecter dans le journal des évènements
                //Le traitement ne termine pas en WARNING

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, errorMsg, 0,
                    new LogParam(MethodInfo.GetCurrentMethod().Name),
                    new LogParam(info[0]),
                    new LogParam(info[1])));

                //Injection dans le log d'un message success puisque Spheres® a réussi à se connecter après plusieurs tentatives 
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Path: {0}]", path);
                info[1] = StrFunc.AppendFormat(@"[Attempts to access: {0}]", nbAttemps.ToString());
                //
                string infoMsg = @"MSMQ has opened successfully" + Cst.CrLf;
                infoMsg += info[0] + Cst.CrLf;
                infoMsg += info[1] + Cst.CrLf;
                
                
                Logger.Log(new LoggerData(LogLevelEnum.Info, infoMsg, 0,
                    new LogParam(MethodInfo.GetCurrentMethod().Name),
                    new LogParam(info[0]),
                    new LogParam(info[1])));
                Logger.Write();
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

            bool isOk = false;

            string SQLSelect = SQLCst.SELECT + "IDEFSSOFTWARE,REGISTRATIONXML" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString() + Cst.CrLf;

            using (DataSet dsLicenses = DataHelper.ExecuteDataset(CSTools.SetCacheOn(Cs), CommandType.Text, SQLSelect))
            {
                DataTable dtLicenses = dsLicenses.Tables[0];

                if (null == dtLicenses || dtLicenses.Select().Length < 1)
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Missing licensee! Contact EFS to obtain a licensee and methods of registration.");

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
            }
            return isOk;
        }
        public bool IsServiceAuthorised(Cst.ServiceEnum pServiceEnum, DataTable pDtLicenses, string pSoftwareName)
        {
            bool isOk = false;
            DataRow[] drLicense = pDtLicenses.Select("IDEFSSOFTWARE = " + DataHelper.SQLString(pSoftwareName));

            if (drLicense.Length > 1)
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Too many '" + pSoftwareName + "' licensees! Contact EFS to obtain a licensee and methods of registration.");

            if (drLicense.Length == 1)
            {
                string registrationXML = drLicense[0]["REGISTRATIONXML"].ToString();
                if (StrFunc.IsFilled(registrationXML))
                {
                    EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(typeof(License), registrationXML.Trim());
                    License = (License)CacheSerializer.Deserialize(serializeInfo);
                }

                if (null == License)
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Missing '" + pSoftwareName + "' licensee! Contact EFS to obtain a licensee and methods of registration.");

                isOk = License.IsLicServiceAuthorised(pServiceEnum);
            }
            return isOk;
        }



        /// <summary>
        /// Cette méthode doit être appelée lorsque le service s'arrête violemment alors qu'un traitement est en cours
        /// <para>Elle position le statut retour du traitement en erreur</para>
        /// <para>Elle alimente le log avec le message Msg_ServiceStopped</para>
        /// </summary>
        public void StopProcess()
        {
            ProcessState.CodeReturn = Cst.ErrLevel.SERVICE_STOPPED;
            Logger.Log(new LoggerData(LogLevelEnum.Warning, new SysMsgCode(SysCodeEnum.SYS, 60), 0, new LogParam(AppInstance.ServiceName)));
            ProcessEnd();
        }





        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="SpheresException2 si la licence est incorrecte"></exception>
        protected void CheckLicense()
        {
            if (false == IsServiceAuthorised(AppInstance.ServiceEnum))
            {
                ProcessState processState = new ProcessState(ProcessStateTools.StatusErrorEnum, ProcessStateTools.CodeReturnAccessDeniedEnum);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                    "Your licensee don't allow you to run " + AppInstance.ServiceEnum.ToString() + " service in this environment",
                     processState);
            }
        }



        /// <summary>
        /// Contrôle l'existence du folder associé à un path de fichier
        /// <para>Si plusieurs tentatives ont été nécessaires pour accéder au Folder, Spheres® injecte des lignes dans le log pour le notifier</para>
        /// <para>Attention ServiceBase s'appuie sur l'écriture dans le log pour restituer des informations dans le journal des évènements de windows</para>
        /// </summary>
        /// <exception cref="SpheresException2[FOLDERNOTFOUND] si folder non accessible, l'exception donne les informations suivantes: folder,nbr de tentatives,timeout en secondes "/>
        /// <param name="pFilePath"></param>
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout)
        {
            CheckFolderFromFilePath(pPathFileName, pTimeout, 0);
        }
        // EG 20190114 Add detail to ProcessLog Refactoring
        public void CheckFolderFromFilePath(string pPathFileName, double pTimeout, int pLevelOrder)
        {
            FileTools.CheckFolderFromFilePath(pPathFileName, pTimeout, out int nbAttemps, pLevelOrder);

            if (nbAttemps >= 2) // Cela veut dire que le folder existe mais qu'il a fallu plusieurs tentatives pour que Spheres® y accèder
            {
                FileTools.GetFilenameAndFoldername(pPathFileName, out _, out string folderPath);

                //ProcessLogInfo logInfo = null;
                //Injection dans le log d'un message d'erreur (le traitement finira donc en warning)
                string[] info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Folder: {0}]", folderPath);
                info[1] = StrFunc.AppendFormat(@"[Failed attempts to access: {0}]", (nbAttemps - 1).ToString());

                string errorMsg = "[Code Return:" + Cst.Space + Cst.ErrLevel.FOLDERNOTFOUND + Cst.Space + "]" + Cst.CrLf2;
                errorMsg += @"Folder does not exist or is not accessible" + Cst.CrLf;
                errorMsg += info[0] + Cst.CrLf;
                errorMsg += info[1] + Cst.CrLf;

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, errorMsg, pLevelOrder,
                    new LogParam(MethodInfo.GetCurrentMethod().Name),
                    new LogParam(info[0]),
                    new LogParam(info[1])));

                //Injection dans le log d'un message success puisque Spheres® a réussi à se connecter après plusieurs tentatives 
                info = new string[2] { string.Empty, string.Empty };
                info[0] = StrFunc.AppendFormat(@"[Folder: {0}]", folderPath);
                info[1] = StrFunc.AppendFormat(@"[Attempts to access: {0}]", nbAttemps.ToString());

                string infoMsg = @"Process has successfully restablished connection with folder" + Cst.CrLf;
                infoMsg += info[0] + Cst.CrLf;
                infoMsg += info[1] + Cst.CrLf;

                
                Logger.Log(new LoggerData(LogLevelEnum.Info, infoMsg, pLevelOrder,
                    new LogParam(MethodInfo.GetCurrentMethod().Name),
                    new LogParam(info[0]),
                    new LogParam(info[1])));
            }
        }

        /// <summary>
        /// Purge le cache SQL à chaque nouveau Request
        /// <para>La cache SQL est conservé lorsque spheres® traite les messages issus d'une même demande</para>
        /// </summary>
        /// <returns>Retourne true si le cache SQL  a été purgé</returns>
        /// FI 20120701 [17979] Add InitializeQueryCache
        private bool InitializeQueryCache()
        {
            bool ret = true;
            if (0 < Tracker.IdTRK_L)
            {
                // FI 20190528 [23912] Prise en compte du type de process et appInstance puisque chaque instance possède son propore cache SQL 
                // Pour information 
                // => si le process est enfant (cas du STP notamment), il y a purge systématique du cache
                if (LogTools.ExistLogForTracker(CSTools.SetCacheOn(Cs, 1, null), Tracker.IdTRK_L, MQueue.ProcessType, AppInstance.AppNameInstance))
                    ret = false;
            }

            if (ret)
            {
                DataHelper.queryCache.Clear(Cs);
                // FI 20220613 [XXXXX] 
                DataEnabledHelper.ClearCache(Cs);
            }
            return ret;
        }

        /// <summary>
        /// Alimente, Enrichie le message queue lorsque ce dernier est pauvre
        /// </summary>
        protected virtual void ProcessInitializeMqueue()
        {
            // FI 20200616 [XXXXX] Add alimentation éventuelle de d'Id s'il est non spécifié
            // Exemple :  cas d'un message IO où l'identifier est spécifié et non l'Id
            // Petite facilité issu de l'ancien code de ProcessStart
            // FI 20201013 [XXXXX] Exclusion des Messages générés par NormMsgFactory
            if ((false == (IsProcessObserver || MQueue.IsMessageCreatedByNormMsgFactory)) && (false == MQueue.idSpecified))
            {
                SelectDatas();
                if (null != DsDatas)
                {
                    DataRow[] rows = DsDatas.Tables[0].Select(null, "IDDATA", DataViewRowState.OriginalRows);
                    if (ArrFunc.Count(rows) == 1)
                    {
                        MQueue.idSpecified = true;
                        MQueue.id = Convert.ToInt32(rows[0]["IDDATA"].ToString());
                    }
                }
            }
        }


        /// <summary>
        /// Ajoute dans AttachedDoc les assemblies Local et Domain
        /// </summary>
        private void AddAssembliesAttachedDoc()
        {
            AddAssembliesAttachedDoc(pAssemblies: AssemblyTools.GetAppInstanceAssemblies<string>(this.AppInstance), pIsDomain: false);
            AddAssembliesAttachedDoc(pAssemblies: AssemblyTools.GetDomainAssemblies<string>(), pIsDomain: true);
        }


        /// <summary>
        /// Ajoute dans AttachedDoc les assemblies 
        /// </summary>
        /// <param name="pAssemblies"></param>
        /// <param name="pIsDomain"></param>
        private void AddAssembliesAttachedDoc(string pAssemblies, Boolean pIsDomain)
        {
            string fileName = "Assemblies" + "_" + ((pIsDomain) ? "Domain" : "Local") + ".txt";
            string fileDest = Session.MapTemporaryPath(fileName, AppSession.AddFolderSessionId.True);
            try
            {
                FileTools.WriteStringToFile(pAssemblies, fileDest);
                byte[] data = FileTools.ReadFileToBytes(fileDest);

                LogTools.AddAttachedDoc(Cs, IdProcess, Session.IdA, data, fileName, Cst.TypeMIME.Text.Plain.ToString());
            }
            finally
            {
                if ((File.Exists(fileDest)))
                    File.Delete(fileDest);
            }
        }

        /// <summary>
        /// Ajoute dans AttachedDoc le fichier de configuration
        /// </summary>
        private void AddConfigAttachedDoc()
        {
            string fileConfig = Directory.GetFiles(AppInstance.AppRootFolder, AppInstance.AppName + "*" + "config").FirstOrDefault();
            if (StrFunc.IsEmpty(fileConfig))
                throw new InvalidProgramException($"{AppInstance.ServiceName}: File config not found in folder {AppInstance.AppVersion}");

            byte[] data = FileTools.ReadFileToBytes(fileConfig);
            LogTools.AddAttachedDoc(Cs, IdProcess, Session.IdA, data, "Config.xml", Cst.TypeMIME.Text.Xml.ToString());
        }

        /// <summary>
        /// Ajoute dans AttachedDoc le(s) fichier(s) de trace de l'instance 
        /// </summary>
        private void AddTraceFileAttachedDoc()
        {

            var directories = ((from item in Common.AppInstance.TraceManager.SpheresTrace.GetTraceFile().Select(x => new FileInfo(x)).Where(y => y.Exists)
                                select item.Directory.FullName).Distinct()).ToArray();

            if (ArrFunc.IsFilled(directories))
            {
                foreach (string directory in directories)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(directory);
                    Regex regex = new Regex(@"^\d{8}"); // for exclude historic Files trace
                    var fileTrace = from item in directoryInfo.GetFiles()
                                    .Where(x => x.Name.Contains(this.AppInstance.AppName) && !(regex.IsMatch(x.Name)))
                                    select item;

                    List<Task> lstTask = new List<Task>();
                    foreach (var item in fileTrace)
                    {
                        lstTask.Add(
                            Task.Run(() =>
                                {
                                    string fileDest = Session.MapTemporaryPath($"{directoryInfo.Name}_{item.Name}", AppSession.AddFolderSessionId.True);
                                    try
                                    {
                                        File.Copy(item.FullName, fileDest, true);

                                        byte[] data = FileTools.ReadFileToBytes(fileDest);
                                        LogTools.AddAttachedDoc(this.Cs, IdProcess, Session.IdA, data, item.Name, Cst.TypeMIME.Text.Plain);
                                    }
                                    finally
                                    {
                                        if ((File.Exists(fileDest)))
                                            File.Delete(fileDest);
                                    }
                                }));

                    }
                    Task.WaitAll(lstTask.ToArray());
                }
            }
        }

        #endregion Methods
    }
}
