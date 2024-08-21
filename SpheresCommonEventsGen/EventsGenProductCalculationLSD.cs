#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Reflection;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Tuning;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FixML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationLSD : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationLSD(string pCS, TradeInfo pTradeInfo)
            : base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190613 [24683] Use slaveDbTransaction
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsExchangeTradedDerivative)
            {
                #region ExchangeTradedDerivative
                IExchangeTradedDerivative exchangeTradedDerivative = (IExchangeTradedDerivative)pProduct;
                exchangeTradedDerivative.Efs_ExchangeTradedDerivative = new EFS_ExchangeTradedDerivative(CS, pProcess.SlaveDbTransaction, exchangeTradedDerivative, m_tradeInfo.tradeLibrary.DataDocument,
                    m_tradeInfo.statusBusiness, m_tradeInfo.idT);
                ret = exchangeTradedDerivative.Efs_ExchangeTradedDerivative.ErrLevel;
                #endregion ExchangeTradedDerivative
            }
            else
            {
                pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 20), 0, new LogParam(pProduct.ProductBase.ProductName)));

                throw new InvalidOperationException();
            }
            return ret;
        }
        #endregion Methods
    }
}
