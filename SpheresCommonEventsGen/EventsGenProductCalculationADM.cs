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
    public class EventsGenProductCalculationADM : EventsGenProductCalculationBase
    {
        #region Constructors
        public EventsGenProductCalculationADM(string pCS, TradeInfo pTradeInfo): base(pCS, pTradeInfo) { }
        #endregion Constructors
        #region Methods
        #region Calculation
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        // EG 20190114 Add detail to ProcessLog Refactoring
        public override Cst.ErrLevel Calculation(ProcessBase pProcess, IProduct pProduct)
        {
            Cst.ErrLevel ret;
            if (pProduct.ProductBase.IsInvoice)
            {
                #region Invoice
                IInvoice invoice = (IInvoice)pProduct;
                invoice.Efs_Invoice = new EFS_Invoice(m_ConnectionString, invoice, m_tradeInfo.tradeLibrary.DataDocument);
                ret = invoice.Efs_Invoice.ErrLevel;
                #endregion Invoice
            }
            else if (pProduct.ProductBase.IsAdditionalInvoice || pProduct.ProductBase.IsCreditNote)
            {
                #region AdditionalInvoice
                IInvoiceSupplement invoice = (IInvoiceSupplement)pProduct;
                invoice.Efs_Invoice = new EFS_Invoice(m_ConnectionString, invoice, m_tradeInfo.tradeLibrary.DataDocument);
                ret = invoice.Efs_Invoice.ErrLevel;
                #endregion AdditionalInvoice
            }
            else if (pProduct.ProductBase.IsInvoiceSettlement)
            {
                #region InvoiceSettlement
                IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)pProduct;
                invoiceSettlement.Efs_InvoiceSettlement = new EFS_InvoiceSettlement(m_ConnectionString, invoiceSettlement);
                ret = invoiceSettlement.Efs_InvoiceSettlement.ErrLevel;
                if (Cst.ErrLevel.SUCCESS == ret)
                    m_tradeInfo.tradeLibrary.DataDocument.AddParty(invoiceSettlement.Efs_InvoiceSettlement.bankActor);
                #endregion InvoiceSettlement
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
        #endregion Calculation
        #endregion Methods
    }
}
