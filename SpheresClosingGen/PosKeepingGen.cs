#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.Tuning;
#endregion Using Directives

namespace EFS.Process.PosKeeping
{
    public class PosKeepingGenProcess : ProcessTradeBase
    {
		#region Constructor
        public PosKeepingGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) :
            base(pMQueue, pAppInstance)
        {
        }
		#endregion Constructor
        #region Accessors
        #region dataTypeLock
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                TypeLockEnum typeLock = TypeLockEnum.NA;
                if (IsRequestUpdateEntry)
                    typeLock = TypeLockEnum.OTC_PROCESS_POSKEEPING_UPDATEENTRY;
                else if (IsRequestClearBulk)
                    typeLock = TypeLockEnum.OTC_PROCESS_POSKEEPING_CLEARINGBULK;
                else if (IsRequestUnclearing)
                    typeLock = TypeLockEnum.OTC_PROCESS_POSKEEPING_UNCLEARING;
                else if (IsRequestPositionTransfer)
                    typeLock = TypeLockEnum.OTC_PROCESS_POSKEEPING_POT;
                else if (IsRequestEndOfDay)
                    typeLock = TypeLockEnum.OTC_PROCESS_POSKEEPING_EOD;
                return typeLock;
            }
        }
        #endregion dataTypeLock
        #region dataTypeLock
        // EG 20130704
        public TypeLockEnum TypeLock
        {
            get {return DataTypeLock;}
        }
        #endregion TypeLock
        #region IsEntry
        /// <summary>
        /// Obtient true lorsque mQueue est de type PosKeepingEntryMQueue
        /// </summary>
        public bool IsEntry
        {
            get { return MQueue.GetType().Equals(typeof(PosKeepingEntryMQueue)); }
        }
        #endregion IsEntry
        #region IsRequest
        /// <summary>
        /// Obtient true lorsque mQueue est de type PosKeepingRequestMQueue
        /// </summary>
        public bool IsRequest
        {
            get { return MQueue.GetType().Equals(typeof(PosKeepingRequestMQueue)); }
        }
        #endregion IsRequest

        #region IsEntryOrRequestUpdateEntry
        public bool IsEntryOrRequestUpdateEntry
        {
            get { return IsEntry || IsRequestUpdateEntry; }
        }
        #endregion IsEntryOrRequestUpdateEntry

        #region IsRequestClearBulk
        public bool IsRequestClearBulk
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.ClearingBulk == RequestType); }
        }
        #endregion IsRequestClearBulk
        #region IsRequestClearSpec
        public bool IsRequestClearSpec
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.ClearingSpecific == RequestType); }
        }
        #endregion IsRequestClearSpec
        #region IsRequestClosingDay
        public bool IsRequestClosingDay
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.ClosingDay == RequestType); }
        }
        #endregion IsRequestClosingDay
        #region IsRequestDenouementOption
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est EXE,ASS,ABN,NEX ou NAS
        /// </summary>
        public bool IsRequestDenouementOption
        {
            get { return IsRequestOptionExercise || IsRequestOptionAbandon || IsRequestOptionAssignment; }
        }
        #endregion IsRequestDenouementOption
        #region IsRequestEndOfDay
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est EOD
        /// </summary>
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        public bool IsRequestEndOfDay
        {
            get { return IsRequest && ((Cst.PosRequestTypeEnum.EndOfDay == RequestType) || (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin == RequestType)); }
        }
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without margin Requirement(Cst.PosRequestTypeEnum.EndOfDayWithoutMR)
        public bool IsRequestEndOfDayWithMR
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.EndOfDay == RequestType); }
        }
        #endregion IsRequestEndOfDay
        #region IsRequestOptionAbandon
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est ABN,NEX ou NAS
        /// </summary>
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public bool IsRequestOptionAbandon
        {
            get { return IsRequest && ((Cst.PosRequestTypeEnum.OptionAbandon == RequestType) || 
                    (Cst.PosRequestTypeEnum.OptionNotExercised == RequestType) ||
                    (Cst.PosRequestTypeEnum.OptionNotAssigned == RequestType)); }
        }
        #endregion IsRequestOptionAbandon
        #region IsRequestOptionAssignment
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est ASS
        /// </summary>
        public bool IsRequestOptionAssignment
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.OptionAssignment == RequestType); }
        }
        #endregion IsRequestOptionAssignment
        #region IsRequestOptionExercise
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est EXE
        /// </summary>
        public bool IsRequestOptionExercise
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.OptionExercise == RequestType); }
        }
        #endregion IsRequestOptionExercise
        #region IsRequestPositionCancelation
        public bool IsRequestPositionCancelation
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.PositionCancelation == RequestType); }
        }
        #endregion IsRequestPositionCancelation
        #region IsRequestPositionTransfer
        public bool IsRequestPositionTransfer
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.PositionTransfer == RequestType); }
        }
        #endregion IsRequestPositionTransfer
        #region IsRequestRemoveAllocation
        public bool IsRequestRemoveAllocation
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.RemoveAllocation == RequestType); }
        }
        #endregion IsRequestRemoveAllocation
        #region IsRequestSplit
        public bool IsRequestSplit
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.TradeSplitting == RequestType); }
        }
        #endregion IsRequestSplit

        #region IsRequestUnclearing
        public bool IsRequestUnclearing
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.UnClearing == RequestType); }
        }
        #endregion IsRequestUnclearing
        #region IsRequestUnderlyerDelivery
        public bool IsRequestUnderlyerDelivery
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.UnderlyerDelivery == RequestType); }
        }
        #endregion IsRequestUnderlyerDelivery
        #region IsRequestUpdateEntry
        /// <summary>
        /// Obtient true si le requestType présent dans le Mqueue est UPDENTRY
        /// </summary>
        public bool IsRequestUpdateEntry
        {
            get { return IsRequest && (Cst.PosRequestTypeEnum.UpdateEntry == RequestType); }
        }
        #endregion IsRequestUpdateEntry
        
        #region IsMonoDataProcess
        protected override bool IsMonoDataProcess
        {
            get { return (null == MQueue) || IsEntry; }
        }
        #endregion IsMonoDataProcess
        #region RequestType
        /// <summary>
        /// Obtient le requestType présent dans le Mqueue
        /// </summary>
        /// EG 20130719 protected -> public
        /// EG 20151112 [21554] Add IsEntry Test
        public Cst.PosRequestTypeEnum RequestType
        {
            get 
            {
                if (IsEntry)
                    return Cst.PosRequestTypeEnum.Entry;
                else
                    return ((PosKeepingRequestMQueue)MQueue).requestType; 
            }
        }
        #endregion RequestType
        #endregion Accessors
        #region Methods
        #region ProcessInitialize
        /// <summary>
        /// Instancie le Log, Tracker Etc.
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();

            if (false == IsProcessObserver)
            {
                if (!ProcessTuningSpecified)
                {
                    ProcessTuning = new ProcessTuning(Cs, 0, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                }
                if (ProcessTuningSpecified)
                {
                    LogDetailEnum = ProcessTuning.LogDetailEnum;

                    Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessPreExecute
        // EG 20130704 Pas de Lock  ICI si POSREQUEST VIENT DE NORMMSGFACTORY ( avec IDPR à 0).
        protected override void ProcessPreExecute()
        {
            if (IsRequest)
            {
                ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;
                if (false == IsProcessObserver)
                {
                    // LockPOSKEEPING
                    if ((false == IsCreatedByNormalizedMessageFactory) && ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                        ProcessState.CodeReturn = LockCurrentObjectId();
                }
                // RD 20121031 ne pas vérifier la license pour les services pour des raisosn de performances         
                //CheckLicense();
            }
            else
            {
                base.ProcessPreExecute();
            }
        }
        #endregion ProcessPreExecute
        #region ProcessExecuteSpecific
        // EG 20150317 [POC] Add ProductGProduct_MTM
        // EG 20180502 Analyse du code Correction [CA2214]
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;

            if (MQueue.idInfoSpecified)
            {
                if (IsRequestClosingDay || IsRequestEndOfDay)
                {
                    PosKeepingGenMaker posKeepingGenMaker = new PosKeepingGenMaker(this);
                    //FI 20180830 [24152] Mise en commenatire de l'appel à InitParameters
                    //posKeepingGenMaker.InitParameters();
                    codeReturn = posKeepingGenMaker.Generate();
                }
                else
                {
                    PosKeepingGenProcessBase posKeepingGenProcess;
                    string gProduct = MQueue.GetStringValueIdInfoByKey("GPRODUCT");
                    // RD 20170216 [22852] Uncomment switch instruction
                    // EG 20170218 [22852] Add COM|SEC product gestion
                    switch (gProduct)
                    {
                        case Cst.ProductGProduct_FUT:
                            posKeepingGenProcess = new PosKeepingGen_ETD(this);
                            break;
                        case Cst.ProductGProduct_MTM:
                            posKeepingGenProcess = new PosKeepingGen_MTM(this);
                            break;
                        case Cst.ProductGProduct_COM:
                            posKeepingGenProcess = new PosKeepingGen_COM(this);
                            break;
                        case Cst.ProductGProduct_SEC:
                            posKeepingGenProcess = new PosKeepingGen_SEC(this);
                            break;
                        default:
                            posKeepingGenProcess = new PosKeepingGen_OTC(this);
                            break;
                    }

                    //FI 20180830 [24152] Mise en commenatire de l'appel à InitParameters
                    //posKeepingGenProcessBase.InitParameters();
                    codeReturn = posKeepingGenProcess.Generate();

                }
            }
            return codeReturn;
        }
        #endregion ProcessExecuteSpecific
        #region ProcessTerminateSpecific
        protected override void ProcessTerminateSpecific()
        {
            if (IsEntry)
                base.ProcessTerminateSpecific();
        }
        #endregion ProcessTerminateSpecific
          
        #endregion Methods
    }
}
