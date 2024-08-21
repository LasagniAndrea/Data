#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
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
using EfsML.v30.CashBalance;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationRISK : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationRISK(string pCS, TradeInfo  pTradeInfo)
            : base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Del DataDocumentContainer parameter
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            DataDocumentContainer dataDocument = m_tradeInfo.tradeLibrary.DataDocument;

            if (pProduct.ProductBase.IsMarginRequirement)
            {
                MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)pProduct);
                marginRequirement.SetEfsSimplePayment(m_ConnectionString, dataDocument);
            }
            else if (pProduct.ProductBase.IsCashBalance)
            {
                #region Cash Balance
                CashBalance cashBalance = (CashBalance)pProduct;
                cashBalance.efs_CashBalance = new EFS_CashBalance(cashBalance, dataDocument);
                #region Exchange Stream
                if (cashBalance.exchangeCashBalanceStreamSpecified)
                {
                    ExchangeCashBalanceStream exCashBalenceStream = cashBalance.exchangeCashBalanceStream;
                    //PM 20140917 [20066][20185] Add test sur tous les flux
                    if (exCashBalenceStream.marginRequirementSpecified)
                    {
                        exCashBalenceStream.marginRequirement.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.cashAvailableSpecified)
                    {
                        exCashBalenceStream.cashAvailable.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.cashUsedSpecified)
                    {
                        exCashBalenceStream.cashUsed.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.collateralAvailableSpecified)
                    {
                        exCashBalenceStream.collateralAvailable.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.collateralUsedSpecified)
                    {
                        exCashBalenceStream.collateralUsed.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.uncoveredMarginRequirementSpecified)
                    {
                        exCashBalenceStream.uncoveredMarginRequirement.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.marginCallSpecified)
                    {
                        exCashBalenceStream.marginCall.SetEfsSimplePayment(CS, dataDocument);
                    }
                    //PM 20140919 [20066][20185] Add nouveaux flux
                    if (exCashBalenceStream.cashAvailableSpecified)
                    {
                        SetEfsCashAvailable(exCashBalenceStream.cashAvailable);
                    }
                    if (exCashBalenceStream.cashBalanceSpecified)
                    {
                        exCashBalenceStream.cashBalance.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.liquidatingValueSpecified)
                    {
                        if (exCashBalenceStream.liquidatingValue.longOptionValueSpecified)
                        {
                            exCashBalenceStream.liquidatingValue.longOptionValue.SetEfsCashPosition(dataDocument);
                        }
                        if (exCashBalenceStream.liquidatingValue.shortOptionValueSpecified)
                        {
                            exCashBalenceStream.liquidatingValue.shortOptionValue.SetEfsCashPosition(dataDocument);
                        }
                    }
                    // PM 20150616 [21124] Add marketValue
                    if (exCashBalenceStream.marketValueSpecified)
                    {
                        exCashBalenceStream.marketValue.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.fundingSpecified)
                    {
                        exCashBalenceStream.funding.SetEfsCashPosition(dataDocument);
                    }
                    // PM 20150323 [POC] Add Borrowing
                    if (exCashBalenceStream.borrowingSpecified)
                    {
                        exCashBalenceStream.borrowing.SetEfsCashPosition(dataDocument);
                    }
                    // PM 20150330 Add unsettledCash
                    if (exCashBalenceStream.unsettledCashSpecified)
                    {
                        exCashBalenceStream.unsettledCash.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.forwardCashPaymentSpecified)
                    {
                        exCashBalenceStream.forwardCashPayment.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.equityBalanceSpecified)
                    {
                        exCashBalenceStream.equityBalance.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.equityBalanceWithForwardCashSpecified)
                    {
                        exCashBalenceStream.equityBalanceWithForwardCash.SetEfsCashPosition(dataDocument);
                    }
                    // PM 20150616 [21124] Add totalAccountValue
                    if (exCashBalenceStream.totalAccountValueSpecified)
                    {
                        exCashBalenceStream.totalAccountValue.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.excessDeficitSpecified)
                    {
                        exCashBalenceStream.excessDeficit.SetEfsCashPosition(dataDocument);
                    }
                    if (exCashBalenceStream.excessDeficitWithForwardCashSpecified)
                    {
                        exCashBalenceStream.excessDeficitWithForwardCash.SetEfsCashPosition(dataDocument);
                    }
                }
                #endregion
                for (int i = 0; i < ArrFunc.Count(cashBalance.cashBalanceStream); i++)
                {
                    CashBalanceStream cbs = cashBalance.cashBalanceStream[i];
                    //marginRequirement
                    cbs.marginRequirement.SetEfsCashPosition(dataDocument);
                    //PM 20140924 [20066][20185] Utilisation de SetEfsCashAvailable
                    ////cashAvailable
                    //cbs.cashAvailable.SetEfsCashPosition(dataDocument);
                    //cbs.cashAvailable.constituent.previousCashBalance.SetEfsCashPosition(dataDocument);
                    //cbs.cashAvailable.constituent.cashBalancePayment.SetEfsCashPosition(dataDocument);
                    ////CashCashFlows of cashAvailable
                    //CashFlowsConstituent csfconstituent = cbs.cashAvailable.constituent.cashFlows.constituent;
                    //csfconstituent.variationMargin.SetEfsSimplePayment(CS, dataDocument);
                    //csfconstituent.premium.SetEfsSimplePayment(CS, dataDocument);
                    //csfconstituent.cashSettlement.SetEfsSimplePayment(CS, dataDocument);
                    //EventGenCalculationTools.CalcPayments(CS, dataDocument.currentProduct, csfconstituent.fee);
                    SetEfsCashAvailable(cbs.cashAvailable);
                    //cashUsed
                    if (cbs.cashUsedSpecified)
                        cbs.cashUsed.SetEfsCashPosition(dataDocument);
                    //collateralAvailable
                    cbs.collateralAvailable.SetEfsCashPosition(dataDocument);
                    //collateralUsed
                    if (cbs.collateralUsedSpecified)
                        cbs.collateralUsed.SetEfsCashPosition(dataDocument);
                    //uncoveredMarginRequirement
                    if (cbs.uncoveredMarginRequirementSpecified)
                        cbs.uncoveredMarginRequirement.SetEfsCashPosition(dataDocument);
                    //MarginCall
                    if (cbs.marginCallSpecified)
                        cbs.marginCall.SetEfsSimplePayment(CS, dataDocument);
                    //CashBalance
                    cbs.cashBalance.SetEfsCashPosition(dataDocument);
                    //PM 20140922 [20066][20185] Add nouveaux flux
                    //Liquidating Value
                    if (cbs.liquidatingValueSpecified)
                    {
                        if (cbs.liquidatingValue.longOptionValueSpecified)
                        {
                            cbs.liquidatingValue.longOptionValue.SetEfsCashPosition(dataDocument);
                        }
                        if (cbs.liquidatingValue.shortOptionValueSpecified)
                        {
                            cbs.liquidatingValue.shortOptionValue.SetEfsCashPosition(dataDocument);
                        }
                    }
                    // PM 20150616 [21124] Add marketValue
                    if (cbs.marketValueSpecified)
                    {
                        cbs.marketValue.SetEfsCashPosition(dataDocument);
                    }
                    //Funding
                    if (cbs.fundingSpecified)
                    {
                        cbs.funding.SetEfsCashPosition(dataDocument);
                    }
                    //Borrowing
                    // PM 20150324 [POC] Add Borrowing
                    if (cbs.borrowingSpecified)
                    {
                        cbs.borrowing.SetEfsCashPosition(dataDocument);
                    }
                    // PM 20150330 Add unsettledCash
                    if (cbs.unsettledCashSpecified)
                    {
                        cbs.unsettledCash.SetEfsCashPosition(dataDocument);
                    }
                    //Forward Cash Payment
                    if (cbs.forwardCashPaymentSpecified)
                    {
                        cbs.forwardCashPayment.SetEfsCashPosition(dataDocument);
                    }
                    //Equity Balance
                    if (cbs.equityBalanceSpecified)
                    {
                        cbs.equityBalance.SetEfsCashPosition(dataDocument);
                    }
                    //Equity Balance With Forward Cash
                    if (cbs.equityBalanceWithForwardCashSpecified)
                    {
                        cbs.equityBalanceWithForwardCash.SetEfsCashPosition(dataDocument);
                    }
                    // PM 20150616 [21124] Add totalAccountValue
                    if (cbs.totalAccountValueSpecified)
                    {
                        cbs.totalAccountValue.SetEfsCashPosition(dataDocument);
                    }
                    //Excess/Deficit
                    if (cbs.excessDeficitSpecified)
                    {
                        cbs.excessDeficit.SetEfsCashPosition(dataDocument);
                    }
                    //Excess/Deficit With Forward Cash
                    if (cbs.excessDeficitWithForwardCashSpecified)
                    {
                        cbs.excessDeficitWithForwardCash.SetEfsCashPosition(dataDocument);
                    }
                }
                #endregion
            }
            else if (pProduct.ProductBase.IsCashBalanceInterest)
            {
                //PM 20120810 [18058]
                #region CashBalanceInterest
                ICashBalanceInterest cashBalanceInterest = (ICashBalanceInterest)pProduct;
                #region InterestRateStreams
                ret = CalcInterestRateStreams(pProcess, cashBalanceInterest.Stream);
                #endregion InterestRateStreams
                #endregion CashBalanceInterest
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashAvailable"></param>
        /// <param name="pDataDocument"></param>
        ///PM 20140924 [20066][20185] New
        // EG 20180205 [23769] Del DataDocumentContainer parameter  
        private void SetEfsCashAvailable(CashAvailable pCashAvailable)
        {
            DataDocumentContainer dataDocument = m_tradeInfo.tradeLibrary.DataDocument;
            if (null != pCashAvailable)
            {
                pCashAvailable.SetEfsCashPosition(dataDocument);
                if (pCashAvailable.constituentSpecified)
                {
                    pCashAvailable.constituent.previousCashBalance.SetEfsCashPosition(dataDocument);
                    pCashAvailable.constituent.cashBalancePayment.SetEfsCashPosition(dataDocument);
                    //CashCashFlows of cashAvailable
                    CashFlowsConstituent csfconstituent = pCashAvailable.constituent.cashFlows.constituent;
                    csfconstituent.variationMargin.SetEfsSimplePayment(CS, dataDocument);
                    csfconstituent.premium.SetEfsSimplePayment(CS, dataDocument);
                    csfconstituent.cashSettlement.SetEfsSimplePayment(CS, dataDocument);
                    PaymentTools.CalcPayments(CS, dataDocument.CurrentProduct, csfconstituent.fee, dataDocument);
                    // PM 20150709 [21103] Add safekeeping
                    if (csfconstituent.safekeepingSpecified)
                    {
                        PaymentTools.CalcPayments(CS, dataDocument.CurrentProduct, csfconstituent.safekeeping, dataDocument);
                    }
                    // PM 20170911 [23408] Ajout Equalisation Payment
                    if (csfconstituent.equalisationPaymentSpecified)
                    {
                        csfconstituent.equalisationPayment.SetEfsSimplePayment(CS, dataDocument);
                    }
                }
            }
        }
        #endregion Methods
    }
}
