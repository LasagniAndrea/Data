#region Using Directives
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.LoggerClient;
using EFS.Tuning;
using System;
//
//
#endregion Using Directives

namespace EFS.Process
{
    #region InvoicingGenProcess
    public partial class InvoicingGenProcess : ProcessTradeBase
	{
		#region Members
        public EventProcess m_eventProcess;
		#endregion Members
        
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        protected override string DataIdent
        {
            get
            {
                //FI 20120105
                //sur le traitement InvoicingGenMQueue, si le paramètre entity est présent alors il est aussi stocké Mqueue.Id
                //Spheres® y stocke l'idA de l'entité 
                //La dataIdent est donc ds ce cas un ACTOR
                string ret = Cst.OTCml_TBL.TRADEADMIN.ToString();
                if (MQueue.GetType().Equals(typeof(InvoicingGenMQueue)))
                {
                    if (CurrentId >0)
                        ret = Cst.OTCml_TBL.ACTOR.ToString();  
                }
                return ret;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override TypeLockEnum DataTypeLock
        {
            get
            {
                return TypeLockEnum.OTC_INV_BROFEE_PROCESS_GENERATION;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsMonoDataProcess
        {
            get { return (null == MQueue) || (false == MQueue.GetType().Equals(typeof(InvoicingGenMQueue))); }
        }
        #endregion Accessors

        #region Constructors
        public InvoicingGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance) { }
		#endregion Constructors
		#region Methods


        #region ProcessInitialize
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessInitialize()
        {
            base.ProcessInitialize();
            if ((false == IsProcessObserver) && (false == IsMonoDataProcess))
            {
                SQL_Instrument sqlInstrument = new SQL_Instrument(Cs, "invoice");
                if (sqlInstrument.LoadTable(new string[] { "IDI" }))
                {
                    ProcessTuning = new ProcessTuning(Cs, sqlInstrument.IdI, MQueue.ProcessType, AppInstance.ServiceName, AppInstance.HostName);
                    if (ProcessTuningSpecified && (ProcessCall == ProcessCallEnum.Master))
                    {
                        LogDetailEnum = ProcessTuning.LogDetailEnum;

                        // PM 20200102 [XXXXX] New Log
                        Logger.CurrentScope.SetLogLevel(LoggerConversionTools.DetailEnumToLogLevelEnum(LogDetailEnum));
                    }
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessExecuteSpecific
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            
            Type tQueue = MQueue.GetType();
            InvoicingGenProcessBase invoicingGenProcessBase = null;
            if (tQueue.Equals(typeof(InvoicingGenMQueue)))
                invoicingGenProcessBase = new InvoicingGen_Invoice(this);
            else if (tQueue.Equals(typeof(InvoicingCorrectionGenMQueue)))
                invoicingGenProcessBase = new InvoicingGen_Correction(this);
            else if (tQueue.Equals(typeof(InvoicingAllocationGenMQueue)))
                invoicingGenProcessBase = new InvoicingSettlement_AllocatedInvoice(this);
            else if (tQueue.Equals(typeof(InvoicingCancelationSimulationGenMQueue)))
                invoicingGenProcessBase = new InvoicingGen_ActionOnSimulation(this);
            else if (tQueue.Equals(typeof(InvoicingValidationSimulationGenMQueue)))
                invoicingGenProcessBase = new InvoicingGen_ActionOnSimulation(this);

            if (null != invoicingGenProcessBase)
                codeReturn = invoicingGenProcessBase.Generate();
            return codeReturn;
        }
        #endregion ProcessExecuteSpecific
        /// <summary>
        /// 
        /// </summary>
        // 20090909 FI [Add Asset on InvoiceTrade] Traitement particulier si InvoicingGenMQueue
        protected override void ProcessPreExecute()
        {
            Type tQueue = MQueue.GetType();
            if (tQueue.Equals(typeof(InvoicingGenMQueue)))
            {
                //l'Id du message InvoicingGenMQueue n'est pas un trade
                ProcessState.CodeReturn = Cst.ErrLevel.SUCCESS;
                if (false == IsProcessObserver)
                {
                    #region LockINVOICE
                    if (ProcessStateTools.IsCodeReturnSuccess(ProcessState.CodeReturn))
                        ProcessState.CodeReturn = LockCurrentObjectId();
                    #endregion LockINVOICE
                }
                // RD 20121031 ne pas vérifier la license pour les services pour des raisosn de performances         
                //CheckLicense();
            }
            else
            {
                base.ProcessPreExecute();
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void ProcessTerminateSpecific()
        {
            Type tQueue = MQueue.GetType();
            if (false == tQueue.Equals(typeof(InvoicingGenMQueue)))
                base.ProcessTerminateSpecific();
        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void SelectDatas()
        {
        }
        #endregion Methods
    }
    #endregion InvoicingGenProcess
}