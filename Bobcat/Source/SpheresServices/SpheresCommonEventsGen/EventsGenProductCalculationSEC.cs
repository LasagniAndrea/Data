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
using FpML.Enum;
//
using FpML.Interface;
#endregion Using Directives

namespace EFS.Process.EventsGen
{
    /// <summary>
    /// 
    /// </summary>
    public class EventsGenProductCalculationSEC : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationSEC(string pCS, TradeInfo pTradeInfo)
            : base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        #region Calculation
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        // EG 20190730 Upd (Gestion HGA|GAM)
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsDebtSecurity)
            {
                #region DebtSecurity
                IDebtSecurity debtSecurity = (IDebtSecurity)pProduct;
                ret = CalcInterestRateStreams(pProcess, debtSecurity.Stream);

                // EG 20150907 New Calcul recordDate|ExDate fo each payment
                if ( (Cst.ErrLevel.SUCCESS == ret) && 
                    debtSecurity.Security.CalculationRulesSpecified &&
                    debtSecurity.Security.CalculationRules.FullCouponCalculationRulesSpecified)
                {
                    if (debtSecurity.Security.CalculationRules.FullCouponCalculationRules.RecordDateSpecified)
                        SetRecordAndExDates(DividendDateReferenceEnum.RecordDate, debtSecurity, debtSecurity.Security.CalculationRules.FullCouponCalculationRules.RecordDate);
                    if (debtSecurity.Security.CalculationRules.FullCouponCalculationRules.ExDateSpecified)
                        SetRecordAndExDates(DividendDateReferenceEnum.ExDate, debtSecurity, debtSecurity.Security.CalculationRules.FullCouponCalculationRules.ExDate);
                }
                #endregion DebtSecurity
            }
            else
            {
                #region Check Event exist on Security
                ProductContainer productContainer = m_tradeInfo.tradeLibrary.DataDocument.CurrentProduct.GetProduct(pProduct.ProductBase.Id);
                ISecurityAsset[] asset = productContainer.GetSecurityAsset();
                bool isErrorEvent = false;
                if (ArrFunc.IsFilled(asset))
                {
                    for (int i = 0; i < ArrFunc.Count(asset); i++)
                    {
                        if (false == TradeRDBMSTools.IsEventExist(CS, asset[i].OTCmlId))
                        {
                            isErrorEvent = true;
                            string assetIdentifier = Cst.NotAvailable;
                            if (asset[i].SecurityNameSpecified)
                                assetIdentifier = asset[i].SecurityName.Value;

                            // FI 20200623 [XXXXX] SetErrorWarning
                            pProcess.ProcessState.SetErrorWarning(ProcessStateTools.StatusErrorEnum);


                            Logger.Log(new LoggerData(LogLevelEnum.Error, new SysMsgCode(SysCodeEnum.SYS, 544), 0,
                                new LogParam(LogTools.IdentifierAndId(pProcess.MQueue.Identifier, pProcess.CurrentId)),
                                new LogParam(LogTools.IdentifierAndId(pProduct.GetType().Name, productContainer.IdI)),
                                new LogParam(LogTools.IdentifierAndId(assetIdentifier, asset[i].OTCmlId))));
                        }
                    }
                    if (isErrorEvent)
                        throw new Exception("An error occured on events calculation.");
                }
                #endregion

                if (pProduct.ProductBase.IsDebtSecurityTransaction)
                {
                    #region DebtSecurityTransaction
                    IDebtSecurityTransaction debtSecTransaction = (IDebtSecurityTransaction)pProduct;
                    debtSecTransaction.Efs_DebtSecurityTransactionAmounts = new EFS_DebtSecurityTransactionAmounts(CS, debtSecTransaction, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness)
                    {
                        StatusBusiness = m_tradeInfo.statusBusiness
                    };

                    debtSecTransaction.Efs_DebtSecurityTransactionStream = new EFS_DebtSecurityTransactionStream(CS, debtSecTransaction, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                    ret = debtSecTransaction.Efs_DebtSecurityTransactionAmounts.ErrLevel;
                    #endregion DebtSecurityTransaction
                }
                else if (pProduct.ProductBase.IsSaleAndRepurchaseAgreement)
                {
                    #region SaleAndRepurchaseAgreement
                    ISaleAndRepurchaseAgreement sarAgreement = (ISaleAndRepurchaseAgreement)pProduct;
                    #region CashStreams
                    _ = CalcInterestRateStreams(pProcess, sarAgreement.CashStream);
                    #endregion CashStreams
                    sarAgreement.Efs_SaleAndRepurchaseAgreement = new EFS_SaleAndRepurchaseAgreement(CS, sarAgreement, m_tradeInfo.tradeLibrary.DataDocument, m_tradeInfo.statusBusiness);
                    ret = sarAgreement.Efs_SaleAndRepurchaseAgreement.ErrLevel;
                    #endregion SaleAndRepurchaseAgreement
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
            return ret;
        }
        #endregion Calculation

        #region SetRecordAndExDates
        // EG 20150907 New
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void SetRecordAndExDates(DividendDateReferenceEnum pTypeDate, IDebtSecurity pDebtSecurity, IRelativeDateOffset pRelativeDateOffset)
        {
            foreach (IInterestRateStream stream in pDebtSecurity.Stream)
            {
                // Calcul des recordDates|exDates
                Tools.OffSetDateRelativeTo(CS, pRelativeDateOffset, out DateTime[] offsetDates, m_tradeInfo.tradeLibrary.DataDocument);
                if (ArrFunc.IsFilled(offsetDates) && (offsetDates.Length == stream.PaymentDates.Efs_PaymentDates.paymentDates.Length))
                {
                    int i = 0;
                    foreach (EFS_PaymentDate paymentDate in stream.PaymentDates.Efs_PaymentDates.paymentDates)
                    {
                        switch (pTypeDate)
                        {
                            case DividendDateReferenceEnum.ExDate:
                                paymentDate.exDateAdjustmentSpecified = true;
                                paymentDate.exDateAdjustment = new EFS_AdjustableDate(CS, offsetDates[i], pRelativeDateOffset.GetAdjustments, m_tradeInfo.tradeLibrary.DataDocument);
                                break;
                            case DividendDateReferenceEnum.RecordDate:
                                paymentDate.recordDateAdjustmentSpecified = true;
                                paymentDate.recordDateAdjustment = new EFS_AdjustableDate(CS, offsetDates[i], pRelativeDateOffset.GetAdjustments, m_tradeInfo.tradeLibrary.DataDocument);
                                break;
                            default:
                                break;
                        }
                        i++;
                    }
                }
            }
        }
        #endregion SetRecordAndExDates
        #endregion Methods
    }

}
