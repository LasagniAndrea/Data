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
    public class EventsGenProductCalculationFX : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationFX(string pCS,  TradeInfo pTradeInfo)
            : base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20180514 [23812] Report
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsFxAverageRateOption)
            {
                #region FxAverageRateOption
                IFxAverageRateOption fxAverageRateOption = (IFxAverageRateOption)pProduct;
                fxAverageRateOption.Efs_FxAverageRateOption = new EFS_FxAverageRateOption(m_ConnectionString,m_tradeInfo.tradeLibrary.CurrentTrade.TradeHeader.TradeDate, fxAverageRateOption);
                foreach (IFxOptionPremium fxOptionPremium in fxAverageRateOption.FxOptionPremium)
                {
                    fxOptionPremium.Efs_FxOptionPremium = 
                        new EFS_FxOptionPremium(m_ConnectionString, fxAverageRateOption.ExpiryDateTime, fxOptionPremium,
                                                fxAverageRateOption.CallCurrencyAmount, fxAverageRateOption.PutCurrencyAmount, 
                                                m_tradeInfo.tradeLibrary.DataDocument);
                }
                #region Provisions
                ret = CalcProvisions(pProduct);
                #endregion Provisions
                #endregion FxAverageRateOption
            }
            else if (pProduct.ProductBase.IsFxBarrierOption)
            {
                #region FxBarrierOption
                IFxBarrierOption fxBarrierOption = (IFxBarrierOption)pProduct;
                fxBarrierOption.Efs_FxBarrierOption = new EFS_FxBarrierOption(m_ConnectionString, m_tradeInfo.tradeLibrary.CurrentTrade.TradeHeader.TradeDate, fxBarrierOption);
                foreach (IFxOptionPremium fxOptionPremium in fxBarrierOption.FxOptionPremium)
                {
                    fxOptionPremium.Efs_FxOptionPremium = 
                        new EFS_FxOptionPremium(m_ConnectionString, fxBarrierOption.ExpiryDateTime, fxOptionPremium,
                                                fxBarrierOption.CallCurrencyAmount, fxBarrierOption.PutCurrencyAmount, 
                                                m_tradeInfo.tradeLibrary.DataDocument);
                }
                #region Provisions
                ret = CalcProvisions(pProduct);
                #endregion Provisions
                #endregion FxBarrierOption
            }
            else if (pProduct.ProductBase.IsFxDigitalOption)
            {
                #region FxDigitalOption
                IFxDigitalOption fxDigitalOption = (IFxDigitalOption)pProduct;
                fxDigitalOption.Efs_FxDigitalOption = new EFS_FxDigitalOption(m_ConnectionString, m_tradeInfo.tradeLibrary.CurrentTrade.TradeHeader.TradeDate, fxDigitalOption);
                foreach (IFxOptionPremium fxOptionPremium in fxDigitalOption.FxOptionPremium)
                {
                    fxOptionPremium.Efs_FxOptionPremium =
                        new EFS_FxOptionPremium(m_ConnectionString, fxDigitalOption.ExpiryDateTime, fxOptionPremium, null, null, 
                            m_tradeInfo.tradeLibrary.DataDocument);
                }
                #region Provisions
                ret = CalcProvisions(pProduct);
                #endregion Provisions
                #endregion FxDigitalOption
            }
            else if (pProduct.ProductBase.IsFxOptionLeg)
            {
                #region FxOptionLeg
                IFxOptionLeg fxSimpleOption = (IFxOptionLeg)pProduct;
                foreach (IFxOptionPremium fxOptionPremium in fxSimpleOption.FxOptionPremium)
                {
                    fxOptionPremium.Efs_FxOptionPremium = 
                        new EFS_FxOptionPremium(m_ConnectionString, fxSimpleOption.ExpiryDateTime, fxOptionPremium,
                                                fxSimpleOption.CallCurrencyAmount, fxSimpleOption.PutCurrencyAmount, 
                                                m_tradeInfo.tradeLibrary.DataDocument);
                }
                // EG 20150403 (POC]
                fxSimpleOption.Efs_FxSimpleOption = new EFS_FxSimpleOption(m_ConnectionString, m_tradeInfo.tradeLibrary.CurrentTrade.TradeHeader.TradeDate, fxSimpleOption,
                    m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);


                #region Provisions
                ret = CalcProvisions(pProduct);
                #endregion Provisions
                #endregion FxOptionLeg
            }
            else if (pProduct.ProductBase.IsFxLeg)
            {
                #region FxLeg
                IFxLeg fxLeg = (IFxLeg)pProduct;
                // EG 20150403 (POC]
                fxLeg.Efs_FxLeg = new EFS_FxLeg(m_ConnectionString, fxLeg, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                ret = Cst.ErrLevel.SUCCESS;
                #endregion FxLeg
            }
            else if (pProduct.ProductBase.IsFxSwap)
            {
                #region FxSwap
                IFxSwap fxSwap = (IFxSwap)pProduct;
                // EG 20150403 (POC]
                foreach (IFxLeg fxLeg in fxSwap.FxSingleLeg)
                    fxLeg.Efs_FxLeg = new EFS_FxLeg(m_ConnectionString, fxLeg, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                ret = Cst.ErrLevel.SUCCESS;
                #endregion FxSwap
            }
            else if (pProduct.ProductBase.IsFxTermDeposit)
            {
                #region TermDeposit
                ITermDeposit termDeposit = (ITermDeposit)pProduct;
                termDeposit.Efs_TermDeposit = new EFS_TermDeposit(m_ConnectionString, termDeposit, m_tradeInfo.tradeLibrary.DataDocument);
                if (termDeposit.PaymentSpecified)
                    _ = PaymentTools.CalcPayments(CS, pProduct, termDeposit.Payment, m_tradeInfo.tradeLibrary.DataDocument);
                ret = Cst.ErrLevel.SUCCESS;
                #endregion TermDeposit
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
