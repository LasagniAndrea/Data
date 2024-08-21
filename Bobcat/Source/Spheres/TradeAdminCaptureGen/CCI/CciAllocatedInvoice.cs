#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Data;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciAllocatedInvoice.
    /// </summary>
    public class CciAllocatedInvoice : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {

        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("id")]
            id,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceDate")]
            date,
            [System.Xml.Serialization.XmlEnumAttribute("identifier")]
            identifier,
            [System.Xml.Serialization.XmlEnumAttribute("OTCmlId")]
            otcmlId,
            [System.Xml.Serialization.XmlEnumAttribute("amount.amount")]
            amount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("amount.currency")]
            amount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("issueAmount.amount")]
            issueAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("issueAmount.currency")]
            issueAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("accountingAmount.amount")]
            accountingAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("accountingAmount.currency")]
            accountingAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.amount.amount")]
            allocated_amount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.amount.currency")]
            allocated_amount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.issueAmount.amount")]
            allocated_issueAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.issueAmount.currency")]
            allocated_issueAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.accountingAmount.amount")]
            allocated_accountingAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("allocatedAmount.accountingAmount.currency")]
            allocated_accountingAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.amount.amount")]
            unallocated_amount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.amount.currency")]
            unallocated_amount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.issueAmount.amount")]
            unallocated_issueAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.issueAmount.currency")]
            unallocated_issueAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.accountingAmount.amount")]
            unallocated_accountingAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("unAllocatedAmount.accountingAmount.currency")]
            unallocated_accountingAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("fxGainOrLossAmount.amount")]
            fxGainOrLossAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("fxGainOrLossAmount.currency")]
            fxGainOrLossAmount_currency,

            allocated_enter_amount,
            allocated_enter_currency,
            selected,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly CciInvoiceSettlement m_CciInvoiceSettlement;
        private IAllocatedInvoice m_AllocatedInvoice;
        private readonly string m_Prefix;
        private readonly int m_Number;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private SQL_TradeCommon m_SQLTrade;
        #endregion Members
        #region Accessors
        #region AllocatedAmounts
        public INetInvoiceAmounts AllocatedAmounts
        {
            get
            {
                if (null != m_AllocatedInvoice)
                    return m_AllocatedInvoice.AllocatedAmounts;
                return null;
            }
        }
        #endregion AllocatedAmounts
        #region AllocatedInvoice
        public IAllocatedInvoice AllocatedInvoice
        {
            get { return m_AllocatedInvoice; }
            set
            {
                m_AllocatedInvoice = value;
                InitializeSQLTable();
            }
        }
        #endregion AllocatedInvoice
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region ExistSQLTrade
        public bool ExistSQLTrade
        {
            get { return ((null != m_SQLTrade) && m_SQLTrade.IsLoaded); }
        }
        #endregion ExistSQLTrade
        #region GetDefaultAllocatedEnterAmount
        // EG 20091202 
        public IMoney GetDefaultAllocatedEnterAmount
        {
            get
            {
                IMoney amount = null;
                if (null != m_AllocatedInvoice.AllocatedAmounts)
                {
                    string currency = m_CciInvoiceSettlement.InvoiceSettlement.SettlementAmount.Currency;
                    if (StrFunc.IsFilled(m_AllocatedInvoice.AllocatedEnterAmount.Currency))
                        currency = m_AllocatedInvoice.AllocatedEnterAmount.Currency;
                    if (currency == m_AllocatedInvoice.AllocatedAmounts.Amount.Currency)
                        amount = m_AllocatedInvoice.AllocatedAmounts.Amount;
                    else if (currency == m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Currency)
                        amount = m_AllocatedInvoice.AllocatedAmounts.IssueAmount;
                    else
                        amount = m_AllocatedInvoice.AllocatedAmounts.AccountingAmount;
                }
                return amount;
            }
        }
        #endregion GetDefaultAllocatedEnterAmount
        #region IsModeNewOrDuplicate
        public bool IsModeNewOrDuplicate
        {
            get { return Cst.Capture.IsModeNewOrDuplicate(Ccis.CaptureMode); }
        }
        #endregion IsModeNewOrDuplicate
        #region IsModeConsult
        public bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(Ccis.CaptureMode); }
        }
        #endregion IsModeConsult
        #region IsModeUpdateAllocatedInvoice
        public bool IsModeUpdateAllocatedInvoice
        {
            get { return Cst.Capture.IsModeUpdateAllocatedInvoice(Ccis.CaptureMode); }
        }
        #endregion IsModeUpdateAllocatedInvoice
        #region IsModeUpdate
        public bool IsModeUpdate
        {
            get { return Cst.Capture.IsModeUpdate(Ccis.CaptureMode); }
        }
        #endregion IsModeUpdate
        #region UnAllocatedAmounts
        public INetInvoiceAmounts UnAllocatedAmounts
        {
            get
            {
                if (null != m_AllocatedInvoice)
                    return m_AllocatedInvoice.UnAllocatedAmounts;
                return null;
            }
        }
        #endregion UnAllocatedAmounts
        #region Number
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < m_Number)
                    ret = m_Number.ToString();
                return ret;
            }
        }

        #endregion Number
        #region CciEnum
        #region AllocatedEnterAmountValue
        public decimal AllocatedEnterAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.allocated_enter_amount)); }
        }
        #endregion AllocatedEnterAmountValue
        #region AllocatedEnterCurrencyValue
        public string AllocatedEnterCurrencyValue
        {
            get { return Ccis.GetNewValue(CciClientId(CciEnum.allocated_enter_currency)); }
        }
        #endregion AllocatedEnterCurrencyValue
        #region AllocatedAmountValue
        public decimal AllocatedAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.allocated_amount_amount)); }
        }
        #endregion AllocatedAmountValue
        #region AllocatedAmountCurrencyValue
        public string AllocatedAmountCurrencyValue
        {
            get { return Ccis.GetNewValue(CciClientId(CciEnum.allocated_amount_currency)); }
        }
        #endregion AllocatedAmountCurrencyValue
        #region AllocatedIssueAmountValue
        public decimal AllocatedIssueAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.allocated_issueAmount_amount)); }
        }
        #endregion AllocatedIssueAmountValue
        #region AllocatedIssueAmountCurrencyValue
        public string AllocatedIssueAmountCurrencyValue
        {
            get { return Ccis.GetNewValue(CciClientId(CciEnum.allocated_issueAmount_currency)); }
        }
        #endregion AllocatedIssueAmountCurrencyValue
        #region AllocatedAccountingAmountValue
        public decimal AllocatedAccountingAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.allocated_accountingAmount_amount)); }
        }
        #endregion AllocatedAccountingAmountValue
        #region AllocatedAccountingAmountCurrencyValue
        public string AllocatedAccountingAmountCurrencyValue
        {
            get { return Ccis.GetNewValue(CciClientId(CciEnum.allocated_accountingAmount_currency)); }
        }
        #endregion AllocatedAccountingAmountCurrencyValue
        #region FxGainOrLossAmountValue
        public decimal FxGainOrLossAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.fxGainOrLossAmount_amount)); }
        }
        #endregion FxGainOrLossAmountValue

        #region UnallocatedAmountValue
        public decimal UnallocatedAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.unallocated_amount_amount)); }
        }
        #endregion UnallocatedAmountValue
        #region UnallocatedIssueAmountValue
        public decimal UnallocatedIssueAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.unallocated_issueAmount_amount)); }
        }
        #endregion UnallocatedIssueAmountValue
        #region UnallocatedAccountingAmountValue
        public decimal UnallocatedAccountingAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.unallocated_accountingAmount_amount)); }
        }
        #endregion UnallocatedAccountingAmountValue

        #endregion CciEnum
        #endregion Accessors
        #region Constructors
        public CciAllocatedInvoice(CciInvoiceSettlement pCciInvoiceSettlement, string pPrefix, int pAllocatedInvoiceNumber, IAllocatedInvoice pAllocatedInvoice)
        {
            m_CciInvoiceSettlement = pCciInvoiceSettlement;
            m_Ccis = pCciInvoiceSettlement.Ccis;
            m_Number = pAllocatedInvoiceNumber;
            m_Prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            m_AllocatedInvoice = pAllocatedInvoice;
            InitializeSQLTable();
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.id), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.fxGainOrLossAmount_amount), true, TypeData.TypeDataEnum.@decimal);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.fxGainOrLossAmount_currency), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.otcmlId), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.CHK + CciClientId(CciEnum.selected), true, TypeData.TypeDataEnum.@bool);
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_AllocatedInvoice);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.id:
                            #region Allocated Id
                            data = m_AllocatedInvoice.Id;
                            #endregion Allocated Id
                            break;
                        case CciEnum.otcmlId:
                            #region Invoice (Id)
                            data = m_AllocatedInvoice.OtcmlId;
                            #endregion Invoice (Id)
                            break;
                        case CciEnum.identifier:
                            #region Invoice (Identifier)
                            if (ExistSQLTrade)
                                data = m_SQLTrade.Identifier;
                            #endregion Invoice (Identifier)
                            break;
                        case CciEnum.date:
                            #region InvoiceDate
                            data = m_AllocatedInvoice.InvoiceDate.Value;
                            #endregion InvoiceDate
                            break;
                        case CciEnum.amount_amount:
                            #region NetTurnOverAmount (Amount)
                            data = m_AllocatedInvoice.Amount.Amount.Value;
                            #endregion NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.amount_currency:
                            #region NetTurnOverAmount (Currency)
                            data = m_AllocatedInvoice.Amount.Currency;
                            #endregion NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.issueAmount_amount:
                            #region NetTurnOverIssueAmount (Amount)
                            data = m_AllocatedInvoice.IssueAmount.Amount.Value;
                            #endregion NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.issueAmount_currency:
                            #region NetTurnOverIssueAmount (Currency)
                            data = m_AllocatedInvoice.IssueAmount.Currency;
                            #endregion NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.accountingAmount_amount:
                            #region NetTurnOverAccountingAmount (Amount)
                            data = m_AllocatedInvoice.AccountingAmount.Amount.Value;
                            #endregion NetTurnOverAccountingAmount (Amount)
                            break;
                        case CciEnum.accountingAmount_currency:
                            #region NetTurnOverAccountingAmount (Currency)
                            data = m_AllocatedInvoice.AccountingAmount.Currency;
                            #endregion NetTurnOverAccountingAmount (Currency)
                            break;
                        case CciEnum.allocated_amount_amount:
                            #region AllocatedAmount (Amount)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                                data = m_AllocatedInvoice.AllocatedAmounts.Amount.Amount.Value;
                            #endregion AllocatedAmount (Amount)
                            break;
                        case CciEnum.allocated_amount_currency:
                            #region AllocatedAmount (Currency)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                                data = m_AllocatedInvoice.AllocatedAmounts.Amount.Currency;
                            #endregion AllocatedAmount (Currency)
                            break;
                        case CciEnum.allocated_issueAmount_amount:
                            #region AllocatedIssueAmount (Amount)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                                data = m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Amount.Value;
                            #endregion AllocatedIssueAmount (Amount)
                            break;
                        case CciEnum.allocated_issueAmount_currency:
                            #region AllocatedIssueAmount (Currency)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                                data = m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Currency;
                            #endregion AllocatedIssueAmount (Currency)
                            break;
                        case CciEnum.allocated_accountingAmount_amount:
                            #region AllocatedAccountingAmount (Amount)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                            {
                                data = m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Amount.Value;
                                isToValidate = true; // m_CciInvoiceSettlement.IsModeConsult;
                            }
                            #endregion AllocatedAccountingAmount (Amount)
                            break;
                        case CciEnum.allocated_accountingAmount_currency:
                            #region AllocatedAccountingAmount (Currency)
                            if (null != m_AllocatedInvoice.AllocatedAmounts)
                                data = m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Currency;
                            #endregion AllocatedAccountingAmount (Currency)
                            break;
                        case CciEnum.allocated_enter_amount:
                            #region AllocatedEnterAmount (Amount)
                            if (null != m_AllocatedInvoice.AllocatedEnterAmount)
                                data = m_AllocatedInvoice.AllocatedEnterAmount.Amount.Value;
                            //data = GetDefaultAllocatedEnterAmount.amount.Value;
                            #endregion AllocatedEnterAmount (Amount)
                            break;
                        case CciEnum.allocated_enter_currency:
                            #region AllocatedEnterAmount (Currency)
                            if (null != m_AllocatedInvoice.AllocatedEnterAmount)
                                data = m_AllocatedInvoice.AllocatedEnterAmount.Currency;
                            //data = GetDefaultAllocatedEnterAmount.currency;
                            #endregion AllocatedEnterAmount (Currency)
                            break;
                        case CciEnum.unallocated_amount_amount:
                            #region UnallocatedAmount (Amount)
                            if (null != m_AllocatedInvoice.UnAllocatedAmounts)
                                data = m_AllocatedInvoice.UnAllocatedAmounts.Amount.Amount.Value;
                            #endregion UnallocatedAmount (Amount)
                            break;
                        case CciEnum.unallocated_amount_currency:
                            #region UnallocatedAmount (Currency)
                            data = m_AllocatedInvoice.Amount.Currency;
                            #endregion UnallocatedAmount (Currency)
                            break;
                        case CciEnum.unallocated_issueAmount_amount:
                            #region UnallocatedIssueAmount (Amount)
                            if (null != m_AllocatedInvoice.UnAllocatedAmounts)
                                data = m_AllocatedInvoice.UnAllocatedAmounts.IssueAmount.Amount.Value;
                            #endregion UnallocatedIssueAmount (Amount)
                            break;
                        case CciEnum.unallocated_issueAmount_currency:
                            #region UnallocatedIssueAmount (Currency)
                            data = m_AllocatedInvoice.IssueAmount.Currency;
                            #endregion UnallocatedIssueAmount (Currency)
                            break;
                        case CciEnum.unallocated_accountingAmount_amount:
                            #region UnallocatedAccountingAmount (Amount)
                            if (null != m_AllocatedInvoice.UnAllocatedAmounts)
                                data = m_AllocatedInvoice.UnAllocatedAmounts.AccountingAmount.Amount.Value;
                            #endregion UnallocatedAccountingAmount (Amount)
                            break;
                        case CciEnum.unallocated_accountingAmount_currency:
                            #region UnallocatedAccountingAmount (Currency)
                            data = m_AllocatedInvoice.AccountingAmount.Currency;
                            #endregion UnallocatedAccountingAmount (Currency)
                            break;
                        case CciEnum.fxGainOrLossAmount_amount:
                            #region FxGainOrLossAmount (Amount)
                            data = StrFunc.FmtDecimalToInvariantCulture(m_AllocatedInvoice.FxGainOrLossAmount.Amount.DecValue);
                            #endregion FxGainOrLossAmount (Amount)
                            break;
                        case CciEnum.fxGainOrLossAmount_currency:
                            #region FxGainOrLossAmount (Currency)
                            if (StrFunc.IsFilled(m_AllocatedInvoice.FxGainOrLossAmount.Currency))
                                data = m_AllocatedInvoice.FxGainOrLossAmount.Currency;
                            else
                            {
                                data = m_CciInvoiceSettlement.InvoiceSettlement.NetCashAmount.Currency;
                                isToValidate = true;
                            }
                            #endregion FxGainOrLossAmount (Currency)
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion
        #region Dump_ToDocument
        // EG 20091125 Add allocatedEnterAmount 
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.id:
                            #region Allocated (id)
                            m_AllocatedInvoice.Id = data;
                            #endregion Allocated (id)
                            break;
                        case CciEnum.allocated_enter_amount:
                            // EG 20091125 Add allocatedEnterAmount 
                            #region AllocatedEnterAmount (Amount)
                            if (null != m_AllocatedInvoice.AllocatedEnterAmount)
                                m_AllocatedInvoice.AllocatedEnterAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion AllocatedEnterAmount (Amount)
                            break;
                        case CciEnum.allocated_enter_currency:
                            // EG 20091125 Add allocatedEnterAmount 
                            #region AllocatedEnterAmount (Currency)
                            if (null != m_AllocatedInvoice.AllocatedEnterAmount)
                                m_AllocatedInvoice.AllocatedEnterAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion AllocatedEnterAmount (Currency)
                            break;
                        case CciEnum.allocated_amount_amount:
                            #region AllocatedAmount (Amount)
                            m_AllocatedInvoice.AllocatedAmounts.Amount.Amount.Value = data;
                            #endregion AllocatedAmount (Amount)
                            break;
                        case CciEnum.allocated_amount_currency:
                            #region AllocatedAmount (Currency)
                            m_AllocatedInvoice.AllocatedAmounts.Amount.Currency = data;
                            #endregion AllocatedAmount (Currency)
                            break;
                        case CciEnum.allocated_issueAmount_amount:
                            #region AllocatedIssueAmount (Amount)
                            m_AllocatedInvoice.AllocatedAmounts.IssueAmountSpecified = true;
                            m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Amount.Value = data;
                            #endregion AllocatedIssueAmount (Amount)
                            break;
                        case CciEnum.allocated_issueAmount_currency:
                            #region AllocatedIssueAmount (Currency)
                            m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Currency = data;
                            #endregion AllocatedIssueAmount (Currency)
                            break;
                        case CciEnum.allocated_accountingAmount_amount:
                            #region AllocatedAccountingAmount (Amount)
                            m_AllocatedInvoice.AllocatedAmounts.AccountingAmountSpecified = cci.IsFilledValue;
                            m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion AllocatedAccountingAmount (Amount)
                            break;
                        case CciEnum.allocated_accountingAmount_currency:
                            #region AllocatedAccountingAmount (Currency)
                            m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Currency = data;
                            #endregion AllocatedAccountingAmount (Currency)
                            break;
                        case CciEnum.unallocated_amount_amount:
                            #region UnallocatedAmount (Amount)
                            m_AllocatedInvoice.UnAllocatedAmounts.Amount.Amount.Value = data;
                            #endregion UnallocatedAmount (Amount)
                            break;
                        case CciEnum.unallocated_amount_currency:
                            #region UnallocatedAmount (Currency)
                            m_AllocatedInvoice.UnAllocatedAmounts.Amount.Currency = data;
                            #endregion UnallocatedAmount (Currency)
                            break;
                        case CciEnum.unallocated_issueAmount_amount:
                            #region UnallocatedIssueAmount (Amount)
                            m_AllocatedInvoice.UnAllocatedAmounts.IssueAmountSpecified = true;
                            m_AllocatedInvoice.UnAllocatedAmounts.IssueAmount.Amount.Value = data;
                            #endregion UnallocatedIssueAmount (Amount)
                            break;
                        case CciEnum.unallocated_issueAmount_currency:
                            #region UnallocatedIssueAmount (Currency)
                            m_AllocatedInvoice.UnAllocatedAmounts.IssueAmount.Currency = data;
                            #endregion UnallocatedIssueAmount (Currency)
                            break;
                        case CciEnum.unallocated_accountingAmount_amount:
                            #region UnallocatedAccountingAmount (Amount)
                            m_AllocatedInvoice.UnAllocatedAmounts.AccountingAmountSpecified = cci.IsFilledValue;
                            m_AllocatedInvoice.UnAllocatedAmounts.AccountingAmount.Amount.Value = data;
                            #endregion UnallocatedAccountingAmount (Amount)
                            break;
                        case CciEnum.unallocated_accountingAmount_currency:
                            #region UnallocatedAccountingAmount (Currency)
                            m_AllocatedInvoice.UnAllocatedAmounts.AccountingAmount.Currency = data;
                            #endregion UnallocatedAccountingAmount (Currency)
                            break;
                        case CciEnum.fxGainOrLossAmount_amount:
                            #region FxGainOrLoss (Amount)
                            m_AllocatedInvoice.FxGainOrLossAmountSpecified = cci.IsFilledValue;
                            m_AllocatedInvoice.FxGainOrLossAmount.Amount.Value = data;
                            #endregion FxGainOrLoss (Amount)
                            break;
                        case CciEnum.fxGainOrLossAmount_currency:
                            #region FxGainOrLoss (Currency)
                            m_AllocatedInvoice.FxGainOrLossAmount.Currency = data;
                            #endregion FxGainOrLoss (Currency)
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                switch (key)
                {
                    case CciEnum.allocated_enter_amount:
                    case CciEnum.allocated_enter_currency:
                        AllocatedAmountCalculation();
                        m_CciInvoiceSettlement.TotalFxGainOrLossCalculation();
                        break;
                    case CciEnum.allocated_accountingAmount_amount:
                        RefreshAllAllocatedInvoice();
                        break;
                }
            }
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            // EG 20120116 Add test IsModeUpdate (cas d'une entrée en modification après la saisie d'un règlement sans allocation initiale)
            bool isUnlocked = (IsModeNewOrDuplicate || IsModeUpdateAllocatedInvoice || IsModeUpdate) && AllocatedInvoice.Id.StartsWith("NEW_");
            Ccis.Set(CciClientId(CciEnum.selected), "IsEnabled", (0 < AllocatedInvoice.OTCmlId) && isUnlocked);
            bool isEnabled = m_CciInvoiceSettlement.AllocatedInvoiceSpecified && (0 < m_CciInvoiceSettlement.CashAmountValue);
            Ccis.Set(CciClientId(CciEnum.allocated_enter_amount), "IsEnabled", isEnabled && isUnlocked);
            Ccis.Set(CciClientId(CciEnum.allocated_enter_currency), "IsEnabled", isEnabled && isUnlocked);
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return m_Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return m_Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region ITradeGetInfoButton Members
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #region IsButtonMenu
        public bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            bool isButtonMenu = this.IsCci(CciEnum.identifier, pCci);
            if (isButtonMenu && (null != m_SQLTrade))
            {
                #region Trade
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin);
                pCo.PK = "IDT";
                pCo.PKV = m_SQLTrade.Id.ToString();
                #endregion Trade
            }
            return isButtonMenu;

        }
        #endregion IsButtonMenu
        #endregion ITradeGetInfoButton Members
        #endregion Interfaces
        #region Methods
        #region AllocatedAmountCalculation
        // EG 20091202 Test id start with NEW_
        // EG 20110205 Test 0 != settlementAmount
        public void AllocatedAmountCalculation()
        {
            if (AllocatedInvoice.Id.StartsWith("NEW_"))
            {
                decimal allocatedEnterAmount = AllocatedEnterAmountValue;
                string allocatedEnterCurrency = AllocatedEnterCurrencyValue;
                decimal settlementAmount = m_CciInvoiceSettlement.SettlementAmountValue;
                decimal cashAmount = m_CciInvoiceSettlement.CashAmountValue;
                string settlementCurrency = m_CciInvoiceSettlement.SettlementCurrencyValue;
                // 20090617 EG Ne pas prendre le NET mais le NON ALLOUE
                decimal NTO_Amount = m_AllocatedInvoice.Amount.Amount.DecValue;
                decimal NTI_Amount = m_AllocatedInvoice.IssueAmount.Amount.DecValue;
                decimal NTA_Amount = m_AllocatedInvoice.AccountingAmount.Amount.DecValue;
                decimal allocatedAmount = 0;
                decimal allocatedIssueAmount = 0;
                decimal allocatedAccountingAmount = 0;
                // AllocatedAmount
                if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.Amount.Currency)
                {
                    allocatedEnterAmount = Math.Min(allocatedEnterAmount, NTO_Amount);
                    // EG 20091125 Test NTO_Amount != 0
                    if (0 != NTO_Amount)
                    {
                        allocatedAmount = allocatedEnterAmount;
                        allocatedIssueAmount = NTI_Amount * (allocatedEnterAmount / NTO_Amount);
                        allocatedAccountingAmount = NTA_Amount * (allocatedEnterAmount / NTO_Amount);
                    }
                }
                else if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Currency)
                {
                    allocatedEnterAmount = Math.Min(allocatedEnterAmount, NTI_Amount);
                    // EG 20091125 Test NTI_Amount != 0
                    if (0 != NTI_Amount)
                    {
                        allocatedAmount = NTO_Amount * (allocatedEnterAmount / NTI_Amount);
                        allocatedIssueAmount = allocatedEnterAmount;
                        allocatedAccountingAmount = NTA_Amount * (allocatedEnterAmount / NTI_Amount);
                    }
                }
                else if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Currency)
                {
                    allocatedEnterAmount = Math.Min(allocatedEnterAmount, NTA_Amount);
                    // EG 20091125 Test NTA_Amount != 0
                    if (0 != NTA_Amount)
                    {
                        allocatedAmount = NTO_Amount * (allocatedEnterAmount / NTA_Amount);
                        allocatedIssueAmount = NTI_Amount * (allocatedEnterAmount / NTA_Amount);
                        allocatedAccountingAmount = allocatedEnterAmount;
                    }
                }
                else
                {
                    allocatedEnterAmount = 0;
                }
                decimal fxGainOrLossAmount;
                if (settlementCurrency == m_CciInvoiceSettlement.CashCurrencyValue)
                {
                    fxGainOrLossAmount = 0;
                }
                else
                {
                    // EG 20110205 Test 0 != settlementAmount
                    if (0 != settlementAmount)
                        fxGainOrLossAmount = allocatedEnterAmount * (cashAmount / settlementAmount);
                    else
                        fxGainOrLossAmount = 0;
                    if (allocatedEnterCurrency == settlementCurrency)
                        fxGainOrLossAmount -= allocatedAccountingAmount;
                    else if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.Amount.Currency)
                        fxGainOrLossAmount -= allocatedAmount;
                    else if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.IssueAmount.Currency)
                        fxGainOrLossAmount -= allocatedIssueAmount;
                    else if (allocatedEnterCurrency == m_AllocatedInvoice.AllocatedAmounts.AccountingAmount.Currency)
                        fxGainOrLossAmount -= allocatedAccountingAmount;
                }

                m_AllocatedInvoice.AllocatedEnterAmount.Amount.DecValue = allocatedEnterAmount;
                m_AllocatedInvoice.AllocatedEnterAmount.Currency = allocatedEnterCurrency;
                Ccis.SetNewValue(CciClientId(CciEnum.allocated_enter_amount), StrFunc.FmtDecimalToInvariantCulture(allocatedEnterAmount));
                Ccis.SetNewValue(CciClientId(CciEnum.allocated_enter_currency), allocatedEnterCurrency);
                SetAndDisplayAllocatedAmounts(allocatedAmount, allocatedIssueAmount, allocatedAccountingAmount);
                SetAndDisplayFxGainOrLossAmount(fxGainOrLossAmount);
                // EG 20091127 Replace SetAndDisplayUnallocatedAmounts()
                RefreshAllAllocatedInvoice();
                //SetAndDisplayUnallocatedAmounts();
            }
        }
        #endregion AllocatedAmountCalculation
        #region InitializeSQLTable
        private void InitializeSQLTable()
        {
            if (null != m_AllocatedInvoice)
            {
                if (0 < m_AllocatedInvoice.OTCmlId)
                    m_SQLTrade = new SQL_TradeCommon(m_CciInvoiceSettlement.CS, m_AllocatedInvoice.OTCmlId);
            }
        }
        #endregion InitializeSQLTable
        #region IsAllocatedCurrency
        private bool IsAllocatedCurrency(string pCurrency)
        {
            return (pCurrency == m_AllocatedInvoice.Amount.Currency) ||
                   (pCurrency == m_AllocatedInvoice.IssueAmount.Currency) ||
                   (pCurrency == m_AllocatedInvoice.AccountingAmount.Currency);
        }
        #endregion InitializeSQLTable
        #region LoadDDLCurrencyForAllocatedAmount
        public void LoadDDLCurrencyForAllocatedAmount(CciPageBase pPage)
        {
            CustomCaptureInfo cciCurrency = Cci(CciEnum.allocated_enter_currency);
            DropDownList ddl = (DropDownList)pPage.FindControl(cciCurrency.ClientId);
            if (null != ddl)
            {
                #region Load DDL currency for Allocated Enter amount
                foreach (ListItem item in ddl.Items)
                    item.Enabled = IsAllocatedCurrency(item.Value);
                #endregion Load DDL currency for Allocated Enter amount
            }
        }
        #endregion LoadDDLCurrencyForAllocatedAmount
        #region RefreshAllAllocatedInvoice
        // EG 20180425 Analyse du code Correction [CA2202]
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        public void RefreshAllAllocatedInvoice()
        {
            decimal allocatedAmounts = 0;
            decimal allocatedIssueAmounts = 0;
            decimal allocatedAccountingAmounts = 0;
            m_AllocatedInvoice.UnAllocatedAmounts = m_CciInvoiceSettlement.InvoiceSettlement.CreateAllocatedInvoice();
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(m_CciInvoiceSettlement.CS, "IDT", DbType.Int32), m_AllocatedInvoice.OTCmlId);
            string SQLQuery = SQLCst.SELECT;
            SQLQuery += "alloc.EVENTTYPE," + DataHelper.SQLIsNull(m_CciInvoiceSettlement.CS, "alloc.TURNOVER_AMOUNT", "0") + " as TURNOVER_AMOUNT, alloc.TURNOVER_CUR, " + Cst.CrLf;
            SQLQuery += "alloc.IDT, alloc.IDENTIFIER" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALLOCATEDINVOICESTL.ToString() + " alloc" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "(alloc.IDT = @IDT)";

            QueryParameters qryParameters = new QueryParameters(m_CciInvoiceSettlement.CS, SQLQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(m_CciInvoiceSettlement.CS), CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    //int idT = Convert.ToInt32(dr["IDT"]);
                    decimal ntoAmount = Convert.ToDecimal(dr["TURNOVER_AMOUNT"]);
                    string eventType = dr["EVENTTYPE"].ToString();
                    if (0 < ntoAmount)
                    {

                        if (EventTypeFunc.NetTurnOverAmount == eventType)
                            allocatedAmounts = ntoAmount;
                        else if (EventTypeFunc.NetTurnOverIssueAmount == eventType)
                            allocatedIssueAmounts = ntoAmount;
                        else if (EventTypeFunc.NetTurnOverAccountingAmount == eventType)
                            allocatedAccountingAmounts = ntoAmount;
                    }
                }
            }

            decimal unAllocatedAmount = m_AllocatedInvoice.Amount.Amount.DecValue - allocatedAmounts;
            decimal unAllocatedIssueAmount = m_AllocatedInvoice.IssueAmount.Amount.DecValue - allocatedIssueAmounts;
            decimal unAllocatedAccountingAmount = m_AllocatedInvoice.AccountingAmount.Amount.DecValue - allocatedAccountingAmounts;
            SetAndDisplayUnallocatedAmounts(unAllocatedAmount, unAllocatedIssueAmount, unAllocatedAccountingAmount);
            Ccis.IsToSynchronizeWithDocument = true;
        }
        #endregion RefreshAllAllocatedInvoice
        #region SetAndDisplayAllocatedAmounts
        // EG 20091126 Modification application Rounding
        private void SetAndDisplayAllocatedAmounts(decimal pAmount, decimal pIssueAmount, decimal pAccountingAmount)
        {
            Ccis.SetNewValue(CciClientId(CciEnum.allocated_amount_amount), StrFunc.FmtDecimalToInvariantCulture(SetRoundedAmount(pAmount, AllocatedInvoice.Amount.Currency)));
            Ccis.SetNewValue(CciClientId(CciEnum.allocated_issueAmount_amount), StrFunc.FmtDecimalToInvariantCulture(SetRoundedAmount(pIssueAmount, AllocatedInvoice.IssueAmount.Currency)));
            Ccis.SetNewValue(CciClientId(CciEnum.allocated_accountingAmount_amount), StrFunc.FmtDecimalToInvariantCulture(SetRoundedAmount(pAccountingAmount, AllocatedInvoice.AccountingAmount.Currency)));
        }
        #endregion SetAndDisplayAllocatedAmounts
        #region SetAndDisplayFxGainOrLossAmount
        private void SetAndDisplayFxGainOrLossAmount(decimal pFxGainOrLossAmount)
        {
            EFS_Cash cash = new EFS_Cash(m_CciInvoiceSettlement.CS, pFxGainOrLossAmount, m_CciInvoiceSettlement.CashCurrencyValue);
            Ccis.SetNewValue(CciClientId(CciEnum.fxGainOrLossAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
            Ccis.SetNewValue(CciClientId(CciEnum.fxGainOrLossAmount_currency), m_CciInvoiceSettlement.CashCurrencyValue);
        }
        #endregion SetAndDisplayFxGainOrLossAmount
        #region SetAndDisplayUnallocatedAmounts
        // EG 20091127
        private void SetAndDisplayUnallocatedAmounts(decimal pUnallocatedAmount, decimal pUnallocatedIssueAmount, decimal pUnallocatedAccountingAmount)
        {
            decimal unAllocatedAmount = pUnallocatedAmount;
            decimal unAllocatedIssueAmount = pUnallocatedIssueAmount;
            decimal unAllocatedAccountingAmount = pUnallocatedAccountingAmount;

            if ((false == IsModeConsult) && AllocatedInvoice.Id.StartsWith("NEW_"))
            {
                unAllocatedAmount -= AllocatedAmountValue;
                unAllocatedIssueAmount -= AllocatedIssueAmountValue;
                unAllocatedAccountingAmount -= AllocatedAccountingAmountValue;
            }
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_amount_amount), StrFunc.FmtDecimalToInvariantCulture(unAllocatedAmount));
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_issueAmount_amount), StrFunc.FmtDecimalToInvariantCulture(unAllocatedIssueAmount));
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_accountingAmount_amount), StrFunc.FmtDecimalToInvariantCulture(unAllocatedAccountingAmount));
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_amount_currency), m_AllocatedInvoice.Amount.Currency);
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_issueAmount_currency), m_AllocatedInvoice.IssueAmount.Currency);
            Ccis.SetNewValue(CciClientId(CciEnum.unallocated_accountingAmount_currency), m_AllocatedInvoice.AccountingAmount.Currency);
        }
        #endregion SetAndDisplayUnallocatedAmounts
        #region SetRoundedAmount
        // EG 20091126 
        private decimal SetRoundedAmount(decimal pAmount, string pCurrency)
        {
            EFS_Cash cash = new EFS_Cash(m_CciInvoiceSettlement.CS, pAmount, pCurrency);
            decimal amount = cash.AmountRounded;
            decimal diffRounded = amount - pAmount;
            if ((-1 < diffRounded) && (diffRounded < 1) && (0 == cash.AmountRoundPrec))
                amount = pAmount;
            return amount;
        }
        #endregion SetRoundedAmount
        #endregion Methods
    }
}
                                                                                    