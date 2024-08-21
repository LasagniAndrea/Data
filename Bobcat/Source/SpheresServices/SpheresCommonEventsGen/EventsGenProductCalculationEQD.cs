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
//
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationEQD : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationEQD(string pCS, TradeInfo pTradeInfo): base(pCS, pTradeInfo) { }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20231127 [WI755] Implementation Return Swap : Set Ismargining & IsFunding
        // EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            if (pProduct.ProductBase.IsReturnSwap)
            {
                #region ReturnSwap
                IReturnSwap returnSwap = (IReturnSwap)pProduct;
                returnSwap.Efs_ReturnSwap = new EFS_ReturnSwap(m_ConnectionString, returnSwap, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                _ = new ReturnSwapContainer(returnSwap, m_tradeInfo.tradeLibrary.DataDocument);
                if (returnSwap.ReturnLegSpecified)
                {
                    #region ReturnLeg
                    foreach (IReturnLeg leg in returnSwap.ReturnLeg)
                    {
                        Pair<IReturnLeg, IReturnLegMainUnderlyer> _legInfo = ReturnSwapContainer.GetReturnLegInfo(m_ConnectionString, null, leg);
                        leg.Efs_ReturnLeg = new EFS_ReturnLeg(m_ConnectionString, _legInfo, m_tradeInfo.tradeLibrary.DataDocument)
                        {
                            IsMargining = returnSwap.Efs_ReturnSwap.isMargining
                        };
                    }
                    #endregion ReturnLeg
                }
                if (returnSwap.InterestLegSpecified)
                {
                    #region InterestLeg
                    foreach (IInterestLeg leg in returnSwap.InterestLeg)
                    {
                        Pair<IInterestLeg, IInterestCalculation> _legInfo = ReturnSwapContainer.GetInterestLegInfo(m_ConnectionString, null, leg);
                        leg.Efs_InterestLeg = new EFS_InterestLeg(m_ConnectionString, _legInfo, m_tradeInfo.tradeLibrary.DataDocument)
                        {
                            IsFunding = returnSwap.Efs_ReturnSwap.isFunding
                        };
                    }
                    #endregion InterestLeg
                }
                #endregion ReturnSwap
            }
            else if (pProduct.ProductBase.IsEquityOption)
            {
                #region EquityOption
                IEquityOption equityOption = (IEquityOption)pProduct;
                equityOption.Efs_EquityOption = new EFS_EquityOption(m_ConnectionString, equityOption, m_tradeInfo.tradeLibrary.DataDocument);
                #region Premium
                equityOption.Efs_EquityOptionPremium = new EFS_EquityOptionPremium(m_ConnectionString, equityOption, m_tradeInfo.tradeLibrary.DataDocument);
                #endregion Premium
                #endregion EquityOption
            }
            else
            {
                // FI 20200623 [XXXXX] SetErrorWarning
                pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);

                Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 20), 0, new LogParam(pProduct.ProductBase.ProductName)));

                throw new NotImplementedException();
            }
            return ret;
        }
        #endregion Methods
    }
    
}
