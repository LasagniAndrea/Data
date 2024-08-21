#region Using Directives
using System;
using System.Data;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Process.Provision;
using EfsML;
using EfsML.Business;
#endregion Using Directives

namespace EFS.Process
{
    public class TradeActionGenProcess : ProcessTradeBase
    {

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pMQueue"></param>
        /// <param name="pAppInstance"></param>
        public TradeActionGenProcess(MQueueBase pMQueue, AppInstanceService pAppInstance) : base(pMQueue, pAppInstance) { }

        #region Methods
        /// <summary>
        /// Execution du process
        /// </summary>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        protected override Cst.ErrLevel ProcessExecuteSpecific()
        {
            Type tQueue = MQueue.GetType();

            Cst.ErrLevel codeReturn;
            if (tQueue.Equals(typeof(TradeActionGenMQueue)))
            {
                DataSetTrade dsTrade = new DataSetTrade(Cs, CurrentId);
                EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(Cs, null, CurrentId);

                if (IsTradeAdmin)
                    codeReturn = TradeAdminActionProcess(dsTrade, tradeLibrary);
                else
                    codeReturn = TradeActionProcess(dsTrade, tradeLibrary);

                // EG 20120529 Dispose
                if ((null != dsTrade) && (null != dsTrade.DsTrade))
                    dsTrade.DsTrade.Dispose();
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("Type: {0} is not impeletd", typeof(TradeActionGenMQueue).ToString()));
            }

            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDsTrade"></param>
        /// <param name="pTradeLibrary"></param>
        /// <returns></returns>
        /// FI 20160907 [21831] Modify
        /// FI 20170306 [22225] Modify
        // EG 20180502 Analyse du code Correction [CA2214]
        // EG 20240123 [WI818] Trade input: Modification of periodic fees uninvoiced on a trade - Processing
        protected Cst.ErrLevel TradeActionProcess(DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary)
        {
            TradeActionGenMQueue tradeActionGenMQueue = (TradeActionGenMQueue)MQueue;
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            foreach (TradeActionMQueue item in tradeActionGenMQueue.item)
            {
                TradeActionGenProcessBase tradeActionGenProcessBase = null;
                Cst.ErrLevel codeReturnAction = Cst.ErrLevel.SUCCESS;
                switch (item.tradeActionCode)
                {
                    case TradeActionCode.TradeActionCodeEnum.Barrier:
                    case TradeActionCode.TradeActionCodeEnum.Trigger:
                        tradeActionGenProcessBase = new TradeActionGenProcessTrigger(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.Payout:
                    case TradeActionCode.TradeActionCodeEnum.Rebate:
                    case TradeActionCode.TradeActionCodeEnum.CashSettlement:
                        tradeActionGenProcessBase = new TradeActionGenProcessPayout(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.Abandon:
                    case TradeActionCode.TradeActionCodeEnum.Exercise:
                        tradeActionGenProcessBase = new TradeActionGenProcessAbandonExercise(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.RemoveTrade:
                        tradeActionGenProcessBase = new TradeActionGenProcessRemoveTrade(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.CustomerSettlementRate:
                        tradeActionGenProcessBase = new TradeActionGenProcessFxCustomerSettlement(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.CancelableProvision:
                    case TradeActionCode.TradeActionCodeEnum.ExtendibleProvision:
                    case TradeActionCode.TradeActionCodeEnum.MandatoryEarlyTerminationProvision:
                    case TradeActionCode.TradeActionCodeEnum.OptionalEarlyTerminationProvision:
                    case TradeActionCode.TradeActionCodeEnum.StepUpProvision:
                        tradeActionGenProcessBase = new TradeActionGenProcessProvision(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.FeesEventGen:// FI 20160907 [21831] Add
                        tradeActionGenProcessBase = new TradeActionGenFeesEventGen(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.FeesEventGenUninvoiced:// FI 20160907 [21831] Add
                        tradeActionGenProcessBase = new TradeActionGenFeesUninvoicedEventGen(this, pDsTrade, pTradeLibrary, item);
                        break;
                    case TradeActionCode.TradeActionCodeEnum.FeesCalculation:// FI 20170306 [22225] Add
                        tradeActionGenProcessBase = new TradeActionGenFeesCalculation(this, pDsTrade, pTradeLibrary, item);
                        break;

                    default:
                        break;
                }
                if (null != tradeActionGenProcessBase)
                {
                    tradeActionGenProcessBase.InitializeDataSetEvent();
                    codeReturnAction = tradeActionGenProcessBase.Valorize();
                    if (false == ProcessStateTools.IsCodeReturnSuccess(codeReturnAction) && (Cst.ErrLevel.SUCCESS == codeReturn))
                        codeReturn = codeReturnAction;
                }
            }
            return codeReturn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDsTrade"></param>
        /// <param name="pTradeLibrary"></param>
        /// <returns></returns>
        // EG 20200914 [XXXXX] Correction Initialisation et appel à la méthode Valorize
        protected Cst.ErrLevel TradeAdminActionProcess(DataSetTrade pDsTrade, EFS_TradeLibrary pTradeLibrary)
        {
            Cst.ErrLevel codeReturn = Cst.ErrLevel.SUCCESS;
            TradeActionGenMQueue tradeActionGenMQueue = (TradeActionGenMQueue)MQueue;
            foreach (TradeAdminActionMQueue item in tradeActionGenMQueue.item)
            {
                TradeActionGenProcessBase tradeActionGenProcessBase = null;
                Cst.ErrLevel codeReturnAction = Cst.ErrLevel.SUCCESS;
                switch (item.tradeActionCode)
                {
                    case TradeActionCode.TradeActionCodeEnum.RemoveTrade:
                        tradeActionGenProcessBase = new TradeAdminActionGenRemoveTrade(this, pDsTrade, pTradeLibrary, item);
                        break;
                    default:
                        break;
                }
                if (null != tradeActionGenProcessBase)
                {
                    tradeActionGenProcessBase.InitializeDataSetEvent();
                    codeReturnAction = tradeActionGenProcessBase.Valorize();
                    if (false == ProcessStateTools.IsCodeReturnSuccess(codeReturnAction) && (Cst.ErrLevel.SUCCESS == codeReturn))
                        codeReturn = codeReturnAction;
                }
            }
            return codeReturn;
        }
        #endregion Methods
    }

    public static class New_TradeActionGenAPI
    {
        /// <summary>
        /// Execution of a TradeAction process from calling Process
        /// </summary>
        /// <param name="pQueue">Execution parameters</param>
        /// <param name="ProcessBase">Reference at calling Process</param>
        /// <returns></returns>
        // EG 20190613 [24683] Upd Set Tracker To Slave
        public static ProcessState ExecuteSlaveCall(IDbTransaction pDbTransaction, TradeActionGenMQueue pQueue, ProcessBase pCallProcess, bool pIsNoLockcurrentId, bool pIsSendMessage_PostProcess)
        {
            //
            TradeActionGenProcess process = new TradeActionGenProcess(pQueue, pCallProcess.AppInstance)
            {
                
                ProcessCall = ProcessBase.ProcessCallEnum.Slave,
                SlaveDbTransaction = pDbTransaction,
                IsSendMessage_PostProcess = pIsSendMessage_PostProcess,
                
                IdProcess = pCallProcess.IdProcess,

                LogDetailEnum = pCallProcess.LogDetailEnum,
                NoLockCurrentId = pIsNoLockcurrentId,
                Tracker = pCallProcess.Tracker
            };
            process.ProcessStart();
            return process.ProcessState;
        }
    }

}
