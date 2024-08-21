#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using Tz = EFS.TimeZone;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceSettlement.
    /// </summary>
    public class CciInvoiceSettlement : CciTradeAdminBase, IContainerCci, IContainerCciPayerReceiver, IContainerCciQuoteBasis
    {
        #region Enums
        #region CciEnum
        // EG 20171122 [23509] Add orderEntered
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("receptionDate")]
            receptionDate,
            [System.Xml.Serialization.XmlEnumAttribute("settlementAmount.amount")]
            settlementAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("settlementAmount.currency")]
            settlementAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("unallocatedAmount.amount")]
            unallocatedAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("unallocatedAmount.currency")]
            unallocatedAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("cashAmount.amount")]
            cashAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("cashAmount.currency")]
            cashAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("bankchargesAmount.amount")]
            bankChargesAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("bankchargesAmount.currency")]
            bankChargesAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("vatBankchargesAmount.amount")]
            vatBankChargesAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("vatBankchargesAmount.currency")]
            vatBankChargesAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("netCashAmount.amount")]
            netCashAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("netCashAmount.currency")]
            netCashAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("fxGainOrLossAmount.amount")]
            fxGainOrLossAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("fxGainOrLossAmount.currency")]
            fxGainOrLossAmount_currency,

            deleteAllocatedInvoice,
            fxGainOrLossCalculation,
            refreshAvailableInvoice,
            selectAvailableInvoice,
            allocateAllNewInvoice,
            clearAllNewInvoice,
            availableCheckInvoice,
            allocatedCheckInvoice,
            totalRemainAllocatedAmount_amount,
            totalRemainAllocatedAmount_currency,

            orderEntered,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly IInvoiceSettlement m_InvoiceSettlement;
        private CciAccountNumber m_CciAccountNumber;
        private IAllocatedInvoice[] m_PreviousAllocatedInvoice;
        private CciAllocatedInvoice[] m_CciAllocatedInvoice;
        private CciAvailableInvoice[] m_CciAvailableInvoice;
        private string m_AccountingCurrency;
        private readonly string m_Prefix;
        #endregion Members
        #region Accessors
        #region AccountNumber
        public IAccountNumber AccountNumber
        {
            get { return InvoiceSettlement.AccountNumber; }
        }
        #endregion AccountNumber
        #region AllocatedInvoice
        /*
        public IAllocatedInvoice[] AllocatedInvoice
        {
            get { return InvoiceSettlement.allocatedInvoice; }
        }
        */
        #endregion AllocatedInvoice
        #region AllocatedInvoiceLenght
        public int AllocatedInvoiceLenght
        {
            get { return ArrFunc.IsFilled(m_CciAllocatedInvoice) ? m_CciAllocatedInvoice.Length : 0; }
        }
        #endregion AllocatedInvoiceLenght
        #region AllocatedInvoiceSpecified
        public bool AllocatedInvoiceSpecified
        {
            get
            {
                bool isSpecified = m_InvoiceSettlement.AllocatedInvoiceSpecified && (0 < AllocatedInvoiceLenght);
                isSpecified = isSpecified && (0 < GetAllocatedInvoice()[0].OTCmlId);
                return isSpecified;
            }
        }
        #endregion AllocatedInvoiceSpecified
        #region AvailableInvoice
        /*
        public IAvailableInvoice[] AvailableInvoice
        {
            get { return InvoiceSettlement.availableInvoice; }
        }
        */
        #endregion AvailableInvoice
        #region AvailableInvoiceLenght
        public int AvailableInvoiceLenght
        {
            get { return ArrFunc.IsFilled(m_CciAvailableInvoice) ? m_CciAvailableInvoice.Length : 0; }
        }
        #endregion AvailableInvoiceLenght
        #region AvailableInvoiceSpecified
        public bool AvailableInvoiceSpecified
        {
            get
            {
                bool isSpecified = m_InvoiceSettlement.AvailableInvoiceSpecified && (0 < AvailableInvoiceLenght);
                isSpecified = isSpecified && (0 < GetAvailableInvoice()[0].OTCmlId);
                return isSpecified;
            }
        }
        #endregion AvailableInvoiceSpecified
        #region BeneficiaryAndPayerSpecified
        public bool BeneficiaryAndPayerSpecified
        {
            get
            {
                bool isSpecified = StrFunc.IsFilled(m_InvoiceSettlement.PayerPartyReference.HRef) &&
                                   StrFunc.IsFilled(m_InvoiceSettlement.ReceiverPartyReference.HRef) &&
                                   DataDocument.PartyTradeIdentifier[0].BookIdSpecified &&
                                   (0 < DataDocument.PartyTradeIdentifier[0].BookId.OTCmlId);
                return isSpecified;
            }
        }
        #endregion BeneficiaryAndPayerSpecified
        #region ExistAllocatedSelected
        // EG 20091202
        public bool ExistAllocatedSelected
        {
            get
            {
                bool isExistAllocatedSelected = false;
                if ((null != GetAllocatedInvoice()) && (0 < AllocatedInvoiceLenght))
                {
                    for (int i = 0; i < m_CciAllocatedInvoice.Length; i++)
                    {
                        CciAllocatedInvoice cciAllocatedInvoice = m_CciAllocatedInvoice[i];
                        CustomCaptureInfo cci = Ccis[cciAllocatedInvoice.CciClientId(CciAllocatedInvoice.CciEnum.selected)];
                        if ((null != cci) && (Convert.ToBoolean(cci.NewValue)))
                        {
                            isExistAllocatedSelected = true;
                            break;
                        }
                    }
                }
                return isExistAllocatedSelected;
            }
        }
        #endregion ExistAllocatedSelected
        #region ExistAvailableSelected
        // EG 20091202
        public bool ExistAvailableSelected
        {
            get
            {
                bool isExistAvailableSelected = false;
                if ((null != GetAvailableInvoice()) && (0 < AvailableInvoiceLenght))
                {
                    for (int i = 0; i < m_CciAvailableInvoice.Length; i++)
                    {
                        CciAvailableInvoice cciAvailableInvoice = m_CciAvailableInvoice[i];
                        CustomCaptureInfo cci = Ccis[cciAvailableInvoice.CciClientId(CciAvailableInvoice.CciEnum.selected)];
                        if ((null != cci) && (Convert.ToBoolean(cci.NewValue)))
                        {
                            isExistAvailableSelected = true;
                            break;
                        }
                    }
                }
                return isExistAvailableSelected;
            }
        }
        #endregion ExistAvailableSelected
        #region InvoiceSettlement
        public override IInvoiceSettlement InvoiceSettlement
        {
            get { return m_InvoiceSettlement; }
        }
        #endregion InvoiceSettlement
        #region IsFxGainOrLoss
        public bool IsFxGainOrLoss
        {
            get
            {
                string cashCurrency = CashCurrencyValue;
                string settlementCurrency = SettlementCurrencyValue;
                return (m_AccountingCurrency == cashCurrency) && (m_AccountingCurrency != settlementCurrency);
            }
        }
        #endregion IsFxGainOrLoss
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
        #region IsModeUpdateOrUpdatePostEvts
        public bool IsModeUpdateOrUpdatePostEvts
        {
            get { return Cst.Capture.IsModeUpdateOrUpdatePostEvts(Ccis.CaptureMode); }
        }
        #endregion IsModeUpdateOrUpdatePostEvts
        #region IsModeUpdatePostEvts
        public bool IsModeUpdatePostEvts
        {
            get { return Cst.Capture.IsModeUpdatePostEvts(Ccis.CaptureMode); }
        }
        #endregion IsModeUpdatePostEvts
        #region IsModeUpdateGen
        public bool IsModeUpdateGen
        {
            get { return Cst.Capture.IsModeUpdateGen(Ccis.CaptureMode); }
        }
        #endregion IsModeUpdateGen

        #region IsCashAmountEqualToSettlementAmount
        public bool IsCashAmountEqualToSettlementAmount
        {
            get
            {
                string settlementCurrency = SettlementCurrencyValue;
                string cashCurrency = CashCurrencyValue;
                return (settlementCurrency == cashCurrency) && StrFunc.IsFilled(cashCurrency);
            }
        }
        #endregion IsCashAmountEqualToSettlementAmount
        #region NetBankChargesAmount
        public decimal NetBankChargesAmount
        {
            get
            {
                decimal netBankCharges = 0;
                if (m_InvoiceSettlement.BankChargesAmountSpecified)
                {
                    netBankCharges = m_InvoiceSettlement.BankChargesAmount.Amount.DecValue;
                    if (m_InvoiceSettlement.VatBankChargesAmountSpecified)
                        netBankCharges += m_InvoiceSettlement.VatBankChargesAmount.Amount.DecValue;
                }
                return netBankCharges;
            }
        }
        #endregion NetBankChargesAmount
        #region PrefixHeader
        public override string PrefixHeader
        {
            get { return TradeCommonCustomCaptureInfos.CCst.Prefix_invoiceSettlementHeader; }
        }
        #endregion PrefixHeader
        #region UnAllocatedAmountSpecified
        public bool UnAllocatedAmountSpecified
        {
            get { return (0 < m_InvoiceSettlement.UnallocatedAmount.Amount.DecValue); }
        }
        #endregion UnAllocatedAmountSpecified
        #region CciValue
        #region BankChargesAmountValue
        public decimal BankChargesAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.bankChargesAmount_amount)); }
        }
        #endregion BankChargesAmountValue
        #region CashCurrencyValue
        // EG 20150128 
        public string CashCurrencyValue
        {
            get 
            {
                string currency = string.Empty;
                if (null != m_CciAccountNumber)
                    currency = Ccis.GetNewValue(m_CciAccountNumber.CciClientId(CciAccountNumber.CciEnum.cashCurrency.ToString()));
                return currency;
            }
        }
        #endregion CashCurrencyValue
        #region CashAmountValue
        public decimal CashAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.cashAmount_amount)); }
        }
        #endregion CashAmountValue
        #region NetBankChargesAmountValue
        public decimal NetBankChargesAmountValue
        {
            get { return BankChargesAmountValue + VatBankChargesAmountValue; }
        }
        #endregion NetBankChargesAmountValue
        #region SettlementAmountValue
        public decimal SettlementAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.settlementAmount_amount)); }
        }
        #endregion SettlementAmountValue
        #region SettlementCurrencyValue
        public string SettlementCurrencyValue
        {
            get { return Ccis.GetNewValue(CciClientId(CciEnum.settlementAmount_currency)); }
        }
        #endregion SettlementCurrencyValue
        #region UnallocatedAmountValue
        public decimal UnallocatedAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.unallocatedAmount_amount)); }
        }
        #endregion UnallocatedAmountValue
        #region VatBankChargesAmountValue
        public decimal VatBankChargesAmountValue
        {
            get { return Ccis.GetDecimalNewValue(CciClientId(CciEnum.vatBankChargesAmount_amount)); }
        }
        #endregion VatBankChargesAmountValue
        #endregion CciValue

        /// <summary>
        /// Retourne le partyTradeInformation du bénéficiaire
        /// </summary>
        /// <returns></returns>
        // EG 20171122 [23509] New 
        private IPartyTradeInformation GetPartyTradeInformationBeneficiary()
        {
            IPartyTradeInformation partyTradeInformation = null;
            string partyId = m_InvoiceSettlement.ReceiverPartyReference.HRef;
            if (StrFunc.IsFilled(partyId))
            {
                partyTradeInformation = DataDocument.AddPartyTradeInformation(partyId);
            }
            return partyTradeInformation;
        }

        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] del tradeLibrary 
        public CciInvoiceSettlement(TradeAdminCustomCaptureInfos pCcis)
            : base(pCcis)
        {
            m_InvoiceSettlement = (IInvoiceSettlement)CurrentTrade.Product;
            SetPreviousAllocatedInvoice();
            m_Prefix = TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceSettlement + CustomObject.KEY_SEPARATOR;
            //EFS_TradeLibrary tradeLibray = new EFS_TradeLibrary(CS, null, TradeCommonInput.FpMLDataDocReader);
            //new EFS_TradeLibrary(TradeCommonInput.FpMLDataDocReader);
        }
        #endregion Constructors
        #region Interfaces
        #region ITradeCci Members
        #endregion ITradeCci Members
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            base.AddCciSystem();

            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.refreshAvailableInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.selectAvailableInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.deleteAllocatedInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.fxGainOrLossCalculation), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.allocateAllNewInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.clearAllNewInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.allocatedCheckInvoice), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.BUT + CciClientId(CciEnum.availableCheckInvoice), false, TypeData.TypeDataEnum.@string);

            if (null != m_CciAccountNumber)
                m_CciAccountNumber.AddCciSystem();

            for (int i = 0; i < AllocatedInvoiceLenght; i++)
                m_CciAllocatedInvoice[i].AddCciSystem();
            
            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public override void CleanUp()
        {
            base.CleanUp();
            if ((null != m_InvoiceSettlement.AvailableInvoice) && (0 < m_InvoiceSettlement.AvailableInvoice.Length))
            {
                for (int i = m_InvoiceSettlement.AvailableInvoice.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(m_InvoiceSettlement.AvailableInvoice[i].OtcmlId))
                        ReflectionTools.RemoveItemInArray(m_InvoiceSettlement, "availableInvoice", i);
                }
            }
            m_InvoiceSettlement.AvailableInvoiceSpecified = (ArrFunc.IsFilled(m_InvoiceSettlement.AvailableInvoice) && (0 < m_InvoiceSettlement.AvailableInvoice.Length));

            if ((null != m_InvoiceSettlement.AllocatedInvoice) && (0 < m_InvoiceSettlement.AllocatedInvoice.Length))
            {
                for (int i = m_InvoiceSettlement.AllocatedInvoice.Length - 1; -1 < i; i--)
                {
                    if (StrFunc.IsEmpty(m_InvoiceSettlement.AllocatedInvoice[i].OtcmlId))
                        ReflectionTools.RemoveItemInArray(m_InvoiceSettlement, "allocatedInvoice", i);
                }
            }
            m_InvoiceSettlement.AvailableInvoiceSpecified = (ArrFunc.IsFilled(m_InvoiceSettlement.AllocatedInvoice) && (0 < m_InvoiceSettlement.AllocatedInvoice.Length));
        }
        #endregion CleanUp
        #region Dump_ToDocument
        // EG 20171122 [23509] Add orderEntered
        public override void Dump_ToDocument()
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
                        case CciEnum.payer:
                            #region Payer
                            m_InvoiceSettlement.PayerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            m_InvoiceSettlement.ReceiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Receiver
                            break;
                        case CciEnum.orderEntered:
                            #region OrderEntered
                            DumpOrderEntered_ToDocument(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion OrderEntered
                            break;
                        case CciEnum.receptionDate:
                            #region ReceptionDate
                            m_InvoiceSettlement.ReceptionDate.Value = data;
                            if (StrFunc.IsEmpty(m_InvoiceSettlement.ReceptionDate.Id))
                                m_InvoiceSettlement.ReceptionDate.Id = TradeAdminCustomCaptureInfos.CCst.RECEPTIONDATE_REFERENCE;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion ReceptionDate
                            break;
                        case CciEnum.cashAmount_amount:
                            #region CashAmount (Amount)
                            m_InvoiceSettlement.CashAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion CashAmount (Amount)
                            break;
                        case CciEnum.cashAmount_currency:
                            #region CashAmount (Currency)
                            m_InvoiceSettlement.CashAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion CashAmount (Currency)
                            break;
                        case CciEnum.settlementAmount_amount:
                            #region SettlementAmount (Amount)
                            m_InvoiceSettlement.SettlementAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion SettlementAmount (Amount)
                            break;
                        case CciEnum.settlementAmount_currency:
                            #region SettlementAmount (Currency)
                            m_InvoiceSettlement.SettlementAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion SettlementAmount (Currency)
                            break;
                        case CciEnum.bankChargesAmount_amount:
                            #region BankChargesAmount (Amount)
                            m_InvoiceSettlement.BankChargesAmountSpecified = cci.IsFilledValue;
                            m_InvoiceSettlement.BankChargesAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion BankChargesAmount (Amount)
                            break;
                        case CciEnum.bankChargesAmount_currency:
                            #region BankChargesAmount (Currency)
                            m_InvoiceSettlement.BankChargesAmount.Currency = data;
                            #endregion BankChargesAmount (Currency)
                            break;
                        case CciEnum.vatBankChargesAmount_amount:
                            #region VATBankChargesAmount (Amount)
                            m_InvoiceSettlement.VatBankChargesAmountSpecified = cci.IsFilledValue;
                            m_InvoiceSettlement.VatBankChargesAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion VATBankChargesAmount (Amount)
                            break;
                        case CciEnum.vatBankChargesAmount_currency:
                            #region VATBankChargesAmount (Currency)
                            m_InvoiceSettlement.VatBankChargesAmount.Currency = data;
                            #endregion VATBankChargesAmount (Currency)
                            break;
                        case CciEnum.netCashAmount_amount:
                            #region NetCashAmount (Amount)
                            m_InvoiceSettlement.NetCashAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion NetCashAmount (Amount)
                            break;
                        case CciEnum.netCashAmount_currency:
                            #region NetCashAmount (Currency)
                            m_InvoiceSettlement.NetCashAmount.Currency = data;
                            #endregion NetCashAmount (Currency)
                            break;
                        case CciEnum.fxGainOrLossAmount_amount:
                            #region FxGainOrLossAmount (Amount)
                            m_InvoiceSettlement.FxGainOrLossAmountSpecified = cci.IsFilledValue;
                            m_InvoiceSettlement.FxGainOrLossAmount.Amount.Value = data;
                            #endregion FxGainOrLossAmount (Amount)
                            break;
                        case CciEnum.fxGainOrLossAmount_currency:
                            #region FxGainOrLossAmount (Currency)
                            m_InvoiceSettlement.FxGainOrLossAmount.Currency = data;
                            #endregion FxGainOrLossAmount (Currency)
                            break;
                        case CciEnum.unallocatedAmount_amount:
                            #region UnallocatedAmount (Amount)
                            m_InvoiceSettlement.UnallocatedAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion UnallocatedAmount (Amount)
                            break;
                        case CciEnum.unallocatedAmount_currency:
                            #region UnallocatedAmount (Currency)
                            m_InvoiceSettlement.UnallocatedAmount.Currency = data;
                            #endregion UnallocatedAmount (Currency)
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

            for (int i = 0; i < AllocatedInvoiceLenght; i++)
                m_CciAllocatedInvoice[i].Dump_ToDocument();

            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].Dump_ToDocument();

            if (null != m_CciAccountNumber)
                m_CciAccountNumber.Dump_ToDocument();
            base.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_InvoiceSettlement);
            InitializeAccountNumber_FromCci();
            InitializeAllocatedInvoice_FromCci();
            InitializeAvailableInvoice_FromCci();
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        // EG 20171122 [23509] Add orderEntered
        public override void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            //CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            //
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    //display = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.payer:
                            #region Payer
                            data = m_InvoiceSettlement.PayerPartyReference.HRef;
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            data = m_InvoiceSettlement.ReceiverPartyReference.HRef;
                            SynchronizeTimeZone();
                            #endregion Receiver
                            break;
                        case CciEnum.receptionDate:
                            #region ReceiveDate
                            data = m_InvoiceSettlement.ReceptionDate.Value;
                            #endregion ReceiveDate
                            break;
                        case CciEnum.orderEntered:
                            #region OrderEntered
                            IPartyTradeInformation partyTradeInformation = GetPartyTradeInformationBeneficiary();
                            if ((null != partyTradeInformation) && (partyTradeInformation.TimestampsSpecified && partyTradeInformation.Timestamps.OrderEnteredSpecified))
                            {
                                data = partyTradeInformation.Timestamps.OrderEntered;
                            }
                            else if (null != partyTradeInformation)
                            {
                                
                                data = Tz.Tools.ToString(OTCmlHelper.GetDateSysUTC(CS));
                            }
                            SynchronizeTimeZone();
                            #endregion OrderEntered
                            break;
                        case CciEnum.cashAmount_amount:
                            #region CashAmount (Amount)
                            data = m_InvoiceSettlement.CashAmount.Amount.Value;
                            #endregion CashAmount (Amount)
                            break;
                        case CciEnum.cashAmount_currency:
                            #region CashAmount (Currency)
                            data = m_InvoiceSettlement.CashAmount.Currency;
                            #endregion CashAmount (Currency)
                            break;
                        case CciEnum.settlementAmount_amount:
                            #region SettlementAmount (Amount)
                            data = m_InvoiceSettlement.SettlementAmount.Amount.Value;
                            #endregion SettlementAmount (Amount)
                            break;
                        case CciEnum.settlementAmount_currency:
                            #region SettlementAmount (Currency)
                            data = m_InvoiceSettlement.SettlementAmount.Currency;
                            #endregion SettlementAmount (Currency)
                            break;
                        case CciEnum.bankChargesAmount_amount:
                            #region BankChargesAmount (Amount)
                            if (m_InvoiceSettlement.BankChargesAmountSpecified)
                                data = m_InvoiceSettlement.BankChargesAmount.Amount.Value;
                            #endregion BankChargesAmount (Amount)
                            break;
                        case CciEnum.bankChargesAmount_currency:
                            #region BankChargesAmount (Currency)
                            data = m_InvoiceSettlement.BankChargesAmount.Currency;
                            #endregion BankChargesAmount (Currency)
                            break;
                        case CciEnum.vatBankChargesAmount_amount:
                            #region VatBankChargesAmount (Amount)
                            if (m_InvoiceSettlement.VatBankChargesAmountSpecified)
                                data = m_InvoiceSettlement.VatBankChargesAmount.Amount.Value;
                            #endregion VatBankChargesAmount (Amount)
                            break;
                        case CciEnum.vatBankChargesAmount_currency:
                            #region VatBankChargesAmount (Currency)
                            data = m_InvoiceSettlement.VatBankChargesAmount.Currency;
                            #endregion VatBankChargesAmount (Currency)
                            break;
                        case CciEnum.netCashAmount_amount:
                            #region NetCashAmount (Amount)
                            data = m_InvoiceSettlement.NetCashAmount.Amount.Value;
                            #endregion NetCashAmount (Amount)
                            break;
                        case CciEnum.netCashAmount_currency:
                            #region NetCashAmount (Currency)
                            data = m_InvoiceSettlement.NetCashAmount.Currency;
                            #endregion NetCashAmount (Currency)
                            break;
                        case CciEnum.unallocatedAmount_amount:
                            #region UnallocatedAmount (Amount)
                            data = m_InvoiceSettlement.UnallocatedAmount.Amount.Value;
                            isToValidate = (IsModeUpdateAllocatedInvoice && StrFunc.IsFilled(data));
                            #endregion UnallocatedAmount (Amount)
                            break;
                        case CciEnum.unallocatedAmount_currency:
                            #region UnallocatedAmount (Currency)
                            data = m_InvoiceSettlement.UnallocatedAmount.Currency;
                            #endregion UnallocatedAmount (Currency)
                            break;
                        case CciEnum.fxGainOrLossAmount_amount:
                            #region FxGainOrLossAmount (Amount)
                            data = m_InvoiceSettlement.FxGainOrLossAmount.Amount.Value;
                            #endregion FxGainOrLossAmount (Amount)
                            break;
                        case CciEnum.fxGainOrLossAmount_currency:
                            #region FxGainOrLossAmount (Currency)
                            data = m_InvoiceSettlement.FxGainOrLossAmount.Currency;
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
            if (null != m_CciAccountNumber)
                m_CciAccountNumber.Initialize_FromDocument();

            for (int i = 0; i < AllocatedInvoiceLenght; i++)
                m_CciAllocatedInvoice[i].Initialize_FromDocument();

            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].Initialize_FromDocument();
            base.Initialize_FromDocument();
        }
        #endregion
        #region ProcessInitialize
        // EG 20120420 Marque et Suppression du panier des nouvelles factures sélectionnées (Ticket 17754)
        // EG 20121003 Ticket: 18167
        // EG 20171122 [23509] Add orderEntered
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                //
                switch (key)
                {
                    case CciEnum.payer:
                        base.InitializePartySide();
                        break;
                    case CciEnum.receiver:
                        base.InitializePartySide();
                        // EG 20121003 Ticket: 18167
                        if ((pCci.HasChanged) && (null != m_CciAccountNumber))
                            Ccis.SetErrorMsg(m_CciAccountNumber.CciClientId(CciAccountNumber.CciEnum.correspondant), 
                                Ressource.GetString("ISMANDATORY"));
                        break;
                    case CciEnum.orderEntered:
                        if (pCci.HasChanged)
                        {
                            Ccis.SetNewValue(CciClientId(CciEnum.receptionDate), Tz.Tools.DateToStringISO(pCci.NewValue), true);
                            Ccis.IsToSynchronizeWithDocument = false;
                        }
                        break;
                    case CciEnum.refreshAvailableInvoice:

                        RefreshAvailableInvoice();
                        Ccis.IsToSynchronizeWithDocument = false;
                        break;
                    case CciEnum.settlementAmount_amount:
                        SetAndDisplayCashAmount();
                        TotalFxGainOrLossCalculation();
                        if (false == AvailableInvoiceSpecified)
                            RefreshAvailableInvoice();
                        Ccis.IsToSynchronizeWithDocument = false;
                        break;
                    case CciEnum.settlementAmount_currency:
                        SetAndDisplayCashAmount();
                        TotalFxGainOrLossCalculation();
                        if ((false == AvailableInvoiceSpecified) || pCci.HasChanged)
                            RefreshAvailableInvoice();
                        else
                            Ccis.IsToSynchronizeWithDocument = false;
                        break;
                    case CciEnum.cashAmount_amount:
                        SetAndDisplayNetCashAmount();
                        TotalFxGainOrLossCalculation();
                        if (false == AvailableInvoiceSpecified)
                            RefreshAvailableInvoice();
                        //Ccis.IsToSynchronizeWithDocument = false;
                        break;
                    case CciEnum.bankChargesAmount_amount:
                    case CciEnum.vatBankChargesAmount_amount:
                        SetAndDisplayNetCashAmount();
                        TotalFxGainOrLossCalculation();
                        Ccis.IsToSynchronizeWithDocument = false;
                        break;
                }
            }
            else
            {
                CustomCaptureInfo cci = Ccis[pCci.ClientId_WithoutPrefix];
                if ((null != cci) && cci.HasChanged)
                {
                    bool isActorReceiverChanged = cciParty[0].IsCci(CciTradeParty.CciEnum.actor, pCci);
                    bool isBookReceiverChanged = cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci);
                    bool isActorPayerChanged = cciParty[1].IsCci(CciTradeParty.CciEnum.actor, pCci);

                    if (isBookReceiverChanged)
                        GetAccountingCurrency();

                    // EG 20121003 Ticket: 18167
                    if (isActorReceiverChanged)
                    {
                        if (null != m_CciAccountNumber)
                        {
                            Ccis.SetErrorMsg(m_CciAccountNumber.CciClientId(CciAccountNumber.CciEnum.correspondant), Ressource.GetString("ISMANDATORY"));
                            m_CciAccountNumber.GetNostroAmount();
                        }
                    }

                    if (isActorReceiverChanged || isBookReceiverChanged || isActorPayerChanged)
                    {
                        //RefreshAvailableInvoice();
                        // EG 20120420 Marque et Suppression du panier des nouvelles factures sélectionnées (Ticket 17754)
                        AllocatedCheckInvoice(true);
                        DeleteBasket();
                    }
                }
            }

            if (null != m_CciAccountNumber)
                m_CciAccountNumber.ProcessInitialize(pCci);

            // EG 20091208 totalRemainAllocatedAmount
            decimal totalRemainAllocatedAmount = 0;
            string totalRemainAllocatedCurrency = SettlementCurrencyValue;
            for (int i = 0; i < AllocatedInvoiceLenght; i++)
            {
                m_CciAllocatedInvoice[i].ProcessInitialize(pCci);
                if (totalRemainAllocatedCurrency == CashCurrencyValue)
                    totalRemainAllocatedAmount += m_CciAllocatedInvoice[i].UnallocatedAccountingAmountValue;
                else
                    totalRemainAllocatedAmount += m_CciAllocatedInvoice[i].UnallocatedIssueAmountValue;
            }
            SetAndDisplayTotalRemainAllocatedAmount(totalRemainAllocatedAmount, totalRemainAllocatedCurrency);

            // Don't delete the next line
            if (IsModeUpdateAllocatedInvoice && (false == AvailableInvoiceSpecified) && (0 < UnallocatedAmountValue))
                RefreshAvailableInvoice();

            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].ProcessInitialize(pCci);

            base.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                //
                switch (key)
                {
                    case CciEnum.selectAvailableInvoice:
                        AddBasket();
                        break;
                    case CciEnum.deleteAllocatedInvoice:
                        DeleteBasket();
                        break;
                    case CciEnum.refreshAvailableInvoice:
                        RefreshAvailableInvoice();
                        break;
                    case CciEnum.fxGainOrLossCalculation:
                        TotalFxGainOrLossCalculation();
                        Ccis.IsToSynchronizeWithDocument = false;
                        break;
                    case CciEnum.allocateAllNewInvoice:
                        AllocatedAllNewInvoiceOnBasket();
                        break;
                    case CciEnum.clearAllNewInvoice:
                        ClearAllNewInvoiceOnBasket();
                        break;
                    case CciEnum.allocatedCheckInvoice:
                        AllocatedCheckInvoice(false == ExistAllocatedSelected);
                        break;
                    case CciEnum.availableCheckInvoice:
                        AvailableCheckInvoice(false == ExistAvailableSelected);
                        break;
                    default:
                        break;

                }
                base.ProcessExecute(pCci);
            }
            else
                m_CciAccountNumber.ProcessExecute(pCci);
        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New Function
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                //
                switch (key)
                {
                    case CciEnum.selectAvailableInvoice:
                    case CciEnum.deleteAllocatedInvoice:
                    case CciEnum.allocateAllNewInvoice:
                    case CciEnum.clearAllNewInvoice:
                        TotalFxGainOrLossCalculation();
                        break;
                    default:
                        break;

                }
                base.ProcessExecuteAfterSynchronize(pCci);
            }
        }
        #endregion ProcessExecuteAfterSynchronize
        #region RefreshCciEnabled
        public override void RefreshCciEnabled()
        {

            bool isEnabled = AllocatedInvoiceSpecified && (false == IsModeConsult);
            // EG 20091208
            Ccis.Set(CciClientId(CciEnum.settlementAmount_currency), "IsEnabled", (false == AllocatedInvoiceSpecified));
            Ccis.Set(CciClientId(CciEnum.deleteAllocatedInvoice), "IsEnabled", isEnabled);
            Ccis.Set(CciClientId(CciEnum.fxGainOrLossCalculation), "IsEnabled", isEnabled && (0 < CashAmountValue));
            Ccis.Set(CciClientId(CciEnum.refreshAvailableInvoice), "IsEnabled", BeneficiaryAndPayerSpecified);
            Ccis.Set(CciClientId(CciEnum.selectAvailableInvoice), "IsEnabled", AvailableInvoiceSpecified && (0 < UnallocatedAmountValue));
            Ccis.Set(CciClientId(CciEnum.allocateAllNewInvoice), "IsEnabled", isEnabled);
            Ccis.Set(CciClientId(CciEnum.clearAllNewInvoice), "IsEnabled", isEnabled);
            Ccis.Set(CciClientId(CciEnum.cashAmount_amount), "IsEnabled", (false == IsCashAmountEqualToSettlementAmount));
            // EG 20091208
            Ccis.Set(CciClientId(CciEnum.allocatedCheckInvoice), "IsEnabled", isEnabled);


            for (int i = 0; i < AllocatedInvoiceLenght; i++)
                m_CciAllocatedInvoice[i].RefreshCciEnabled();
            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].RefreshCciEnabled();

            base.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            m_CciAccountNumber.SetDisplay(pCci);
            base.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
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
        #region IContainerCciPayerReceiver Members
        #region CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer.ToString()); }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver.ToString()); }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion IContainerCciPayerReceiver Members
        #region ITradeGetInfoButton Members
        #region IsButtonMenu
        public override bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            bool isButtonMenu = false;
            for (int i = 0; i < AvailableInvoiceLenght; i++)
            {
                isButtonMenu = m_CciAvailableInvoice[i].IsButtonMenu(pCci, ref pCo);
                if (isButtonMenu)
                    break;
            }
            if (false == isButtonMenu)
            {
                for (int i = 0; i < AllocatedInvoiceLenght; i++)
                {
                    isButtonMenu = m_CciAllocatedInvoice[i].IsButtonMenu(pCci, ref pCo);
                    if (isButtonMenu)
                        break;
                }
            }
            return isButtonMenu;
        }
        #endregion isButtonMenu
        #region SetButtonReferential
        // 20090420 EG Affectation Condition sur Aide Référentiel
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            for (int i = 0; i < PartyLength; i++)
            {
                if (cciParty[i].IsCci(CciTradeParty.CciEnum.actor, pCci))
                {
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = "ACTOR";
                    pCo.Condition = (i == 0) ? RoleActor.COUNTERPARTY.ToString() : RoleActor.INVOICINGOFFICE.ToString();
                    break;
                }

                if (cciParty[i].IsCci(CciTradeParty.CciEnum.book, pCci))
                {
                    pCo.ClientId = pCci.ClientId_WithoutPrefix;
                    pCo.Referential = "BOOK_VIEWER";
                    // Add condition
                    pCo.Fk = null;
                    SQL_Table sql_Table = cciParty[i].Cci(CciTradeParty.CciEnum.actor).Sql_Table;
                    if (null != sql_Table)
                    {
                        SQL_Actor sql_Actor = (SQL_Actor)sql_Table;
                        pCo.Fk = sql_Actor.Id.ToString(); //=> uniquement les book de l'acteur
                    }
                    break;
                }
            }
            m_CciAccountNumber.SetButtonReferential(pCci, pCo);
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members
        #endregion Interfaces
        #region Methods
        #region AddBasket
        public void AddBasket()
        {
            if ((null != GetAvailableInvoice()) && (0 < AvailableInvoiceLenght))
            {
                ArrayList aAllocatedInvoice = new ArrayList();
                ArrayList aAvailableInvoice = new ArrayList();
                if (null != GetAllocatedInvoice())
                {
                    if ((1 < AllocatedInvoiceLenght) || (StrFunc.IsFilled(GetAllocatedInvoice()[0].OtcmlId)))
                        aAllocatedInvoice.AddRange(GetAllocatedInvoice());
                }
                for (int i = 0; i < m_CciAvailableInvoice.Length; i++)
                {
                    CciAvailableInvoice cciAvailableInvoice = m_CciAvailableInvoice[i];
                    CustomCaptureInfo cci = Ccis[cciAvailableInvoice.CciClientId(CciAvailableInvoice.CciEnum.selected)];
                    if (null != cci)
                    {
                        if (Convert.ToBoolean(cci.NewValue))
                        {
                            IAllocatedInvoice allocatedInvoice = m_InvoiceSettlement.CreateAllocatedInvoice();
                            IAvailableInvoice availableInvoice = cciAvailableInvoice.AvailableInvoice;
                            INetInvoiceAmounts availableAmounts = availableInvoice.AvailableAmounts;
                            allocatedInvoice.InvoiceDate.DateValue = availableInvoice.InvoiceDate.DateValue;
                            allocatedInvoice.Identifier.Value = availableInvoice.Identifier.Value;
                            allocatedInvoice.OtcmlId = availableInvoice.OtcmlId;
                            // Nets Origines de la facture
                            allocatedInvoice.Amount.Amount.DecValue = availableInvoice.Amount.Amount.DecValue;
                            allocatedInvoice.Amount.Currency = availableInvoice.Amount.Currency;
                            allocatedInvoice.IssueAmountSpecified = true;
                            allocatedInvoice.IssueAmount.Amount.DecValue = availableInvoice.IssueAmount.Amount.DecValue;
                            allocatedInvoice.IssueAmount.Currency = availableInvoice.IssueAmount.Currency;
                            allocatedInvoice.AccountingAmountSpecified = true;
                            allocatedInvoice.AccountingAmount.Amount.DecValue = availableInvoice.AccountingAmount.Amount.DecValue;
                            allocatedInvoice.AccountingAmount.Currency = availableInvoice.AccountingAmount.Currency;
                            // Nets disponibles de la facture
                            INetInvoiceAmounts unAllocatedAmounts = allocatedInvoice.UnAllocatedAmounts;
                            unAllocatedAmounts.Amount.Amount.DecValue = availableAmounts.Amount.Amount.DecValue;
                            unAllocatedAmounts.Amount.Currency = availableAmounts.Amount.Currency;
                            unAllocatedAmounts.IssueAmountSpecified = true;
                            unAllocatedAmounts.IssueAmount.Amount.DecValue = availableAmounts.IssueAmount.Amount.DecValue;
                            unAllocatedAmounts.IssueAmount.Currency = availableAmounts.IssueAmount.Currency;
                            unAllocatedAmounts.AccountingAmountSpecified = true;
                            unAllocatedAmounts.AccountingAmount.Amount.DecValue = availableAmounts.AccountingAmount.Amount.DecValue;
                            unAllocatedAmounts.AccountingAmount.Currency = availableAmounts.AccountingAmount.Currency;
                            // Règlement alloués 
                            INetInvoiceAmounts allocatedAmounts = allocatedInvoice.AllocatedAmounts;
                            allocatedAmounts.Amount.Amount.DecValue = 0;
                            allocatedAmounts.Amount.Currency = availableAmounts.Amount.Currency;
                            allocatedAmounts.IssueAmountSpecified = true;
                            allocatedAmounts.IssueAmount.Amount.DecValue = 0;
                            allocatedAmounts.IssueAmount.Currency = availableAmounts.IssueAmount.Currency;
                            allocatedAmounts.AccountingAmountSpecified = true;
                            allocatedAmounts.AccountingAmount.Amount.DecValue = 0;
                            allocatedAmounts.AccountingAmount.Currency = availableAmounts.AccountingAmount.Currency;
                            aAllocatedInvoice.Add(allocatedInvoice);
                            // EG 20091202
                            allocatedInvoice.AllocatedEnterAmount.Amount.DecValue = 0;
                            allocatedInvoice.AllocatedEnterAmount.Currency = availableAmounts.IssueAmount.Currency;

                        }
                        else
                            aAvailableInvoice.Add(m_InvoiceSettlement.AvailableInvoice[i]);
                    }
                }
                m_InvoiceSettlement.AllocatedInvoiceSpecified = (0 < aAllocatedInvoice.Count);
                if (m_InvoiceSettlement.AllocatedInvoiceSpecified)
                {
                    m_InvoiceSettlement.AllocatedInvoice = (IAllocatedInvoice[])aAllocatedInvoice.ToArray(m_InvoiceSettlement.TypeofAllocatedInvoice);
                    m_InvoiceSettlement.AvailableInvoiceSpecified = (0 < aAvailableInvoice.Count);
                    if (m_InvoiceSettlement.AvailableInvoiceSpecified)
                        m_InvoiceSettlement.AvailableInvoice = (IAvailableInvoice[])aAvailableInvoice.ToArray(m_InvoiceSettlement.TypeofAvailableInvoice);
                    else
                        m_InvoiceSettlement.AvailableInvoice = null;
                }
                else
                    m_InvoiceSettlement.AllocatedInvoice = null;
                Ccis.IsToSynchronizeWithDocument = true;
            }
        }
        #endregion AddBasket
        #region AllocatedAllNewInvoiceOnBasket
        // EG 20091127 New
        public void AllocatedAllNewInvoiceOnBasket()
        {
            AllocatedAllNewInvoiceOnBasket(false);
        }
        public void AllocatedAllNewInvoiceOnBasket(bool pIsClear)
        {
            if (AllocatedInvoiceSpecified)
            {
                decimal fxGainOrLossAmount = 0;
                decimal unAllocatedAmount = UnallocatedAmountValue;
                decimal previousAllocatedAmount = 0;
                //decimal remainAllocatedAccountingAmount = 0;
                for (int i = 0; i < AllocatedInvoiceLenght; i++)
                {
                    decimal amount = UnallocatedAmountValue;
                    string currency = SettlementCurrencyValue;

                    CciAllocatedInvoice cciAllocated = m_CciAllocatedInvoice[i];
                    CustomCaptureInfo cci = Ccis[cciAllocated.CciClientId(CciAllocatedInvoice.CciEnum.selected)];
                    bool complementaryCondition = true;
                    if (false == pIsClear)
                        complementaryCondition = (0 < cciAllocated.UnallocatedIssueAmountValue);
                    if ((cci.IsEnabled) && Convert.ToBoolean(cci.NewValue) && complementaryCondition)
                    {
                        IAllocatedInvoice allocatedInvoice = cciAllocated.AllocatedInvoice;
                        if (pIsClear)
                        {
                            amount = 0;
                            if (currency == cciAllocated.AllocatedAmountCurrencyValue)
                                unAllocatedAmount += cciAllocated.AllocatedAmountValue;
                            else if (currency == cciAllocated.AllocatedIssueAmountCurrencyValue)
                                unAllocatedAmount += cciAllocated.AllocatedIssueAmountValue;
                            else if (currency == cciAllocated.AllocatedAccountingAmountCurrencyValue)
                                unAllocatedAmount += cciAllocated.AllocatedAccountingAmountValue;
                        }
                        else
                        {
                            if (currency == allocatedInvoice.Amount.Currency)
                            {
                                amount = Math.Min(cciAllocated.UnallocatedAmountValue, amount);
                                previousAllocatedAmount = cciAllocated.AllocatedAmountValue;
                                amount += previousAllocatedAmount;

                            }
                            else if (currency == allocatedInvoice.IssueAmount.Currency)
                            {
                                amount = Math.Min(cciAllocated.UnallocatedIssueAmountValue, amount);
                                previousAllocatedAmount = cciAllocated.AllocatedIssueAmountValue;
                                amount += previousAllocatedAmount;
                            }
                            else if (currency == allocatedInvoice.AccountingAmount.Currency)
                            {
                                amount = Math.Min(cciAllocated.UnallocatedAccountingAmountValue, amount);
                                previousAllocatedAmount = cciAllocated.AllocatedAccountingAmountValue;
                                amount += previousAllocatedAmount;
                            }
                        }

                        Ccis.SetNewValue(cciAllocated.CciClientId(CciAllocatedInvoice.CciEnum.allocated_enter_amount), StrFunc.FmtDecimalToInvariantCulture(amount));
                        Ccis.SetNewValue(cciAllocated.CciClientId(CciAllocatedInvoice.CciEnum.allocated_enter_currency), currency);
                        cciAllocated.AllocatedAmountCalculation();
                        fxGainOrLossAmount += cciAllocated.FxGainOrLossAmountValue;

                        if (false == pIsClear)
                        {
                            if (currency == cciAllocated.AllocatedAmountCurrencyValue)
                            {
                                unAllocatedAmount -= cciAllocated.AllocatedAmountValue;
                                unAllocatedAmount += previousAllocatedAmount;
                            }
                            else if (currency == cciAllocated.AllocatedIssueAmountCurrencyValue)
                            {
                                unAllocatedAmount -= cciAllocated.AllocatedIssueAmountValue;
                                unAllocatedAmount += previousAllocatedAmount;
                            }
                            else if (currency == cciAllocated.AllocatedAccountingAmountCurrencyValue)
                            {
                                unAllocatedAmount -= cciAllocated.AllocatedAccountingAmountValue;
                                unAllocatedAmount += previousAllocatedAmount;
                            }
                        }
                        SetAndDisplayFxGainOrLoss(fxGainOrLossAmount, unAllocatedAmount);
                    }
                }
                m_InvoiceSettlement.UnallocatedAmount.Amount.DecValue = unAllocatedAmount;
                m_InvoiceSettlement.FxGainOrLossAmountSpecified = (0 != fxGainOrLossAmount);
                m_InvoiceSettlement.FxGainOrLossAmount.Amount.DecValue = fxGainOrLossAmount;
                Ccis.IsToSynchronizeWithDocument = false;
            }
        }
        #endregion AllocatedAllNewInvoiceOnBasket
        #region AllocatedCheckInvoice
        // EG 20091130 New
        public void AllocatedCheckInvoice(bool pValue)
        {
            if ((null != GetAllocatedInvoice()) && (0 < AllocatedInvoiceLenght))
            {
                for (int i = 0; i < m_CciAllocatedInvoice.Length; i++)
                {
                    CciAllocatedInvoice cciAllocatedInvoice = m_CciAllocatedInvoice[i];
                    CustomCaptureInfo cci = Ccis[cciAllocatedInvoice.CciClientId(CciAllocatedInvoice.CciEnum.selected)];
                    if ((null != cci) && (cci.IsEnabled))
                        cci.NewValue = (pValue ? Boolean.TrueString : Boolean.FalseString);
                }
                Ccis.IsToSynchronizeWithDocument = false;
            }
        }
        #endregion AllocatedCheckInvoice
        #region AvailableCheckInvoice
        // EG 20091130 New
        public void AvailableCheckInvoice(bool pValue)
        {
            if ((null != GetAvailableInvoice()) && (0 < AvailableInvoiceLenght))
            {
                for (int i = 0; i < m_CciAvailableInvoice.Length; i++)
                {
                    CciAvailableInvoice cciAvailableInvoice = m_CciAvailableInvoice[i];
                    CustomCaptureInfo cci = Ccis[cciAvailableInvoice.CciClientId(CciAvailableInvoice.CciEnum.selected)];
                    if (null != cci)
                        cci.NewValue = (pValue ? Boolean.TrueString : Boolean.FalseString);
                }
                Ccis.IsToSynchronizeWithDocument = false;
            }
        }
        #endregion AvailableCheckInvoice
        #region ClearAllNewInvoiceOnBasket
        // EG 20091127 New
        public void ClearAllNewInvoiceOnBasket()
        {
            AllocatedAllNewInvoiceOnBasket(true);
        }
        #endregion ClearAllNewInvoiceOnBasket
        #region DeleteBasket
        public void DeleteBasket()
        {
            if ((null != GetAllocatedInvoice()) && (0 < AllocatedInvoiceLenght))
            {
                ArrayList aAllocatedInvoice = new ArrayList();
                ArrayList aCciAllocatedInvoice = new ArrayList();
                for (int i = 0; i < m_CciAllocatedInvoice.Length; i++)
                {
                    CciAllocatedInvoice cciAllocatedInvoice = m_CciAllocatedInvoice[i];
                    CustomCaptureInfo cci = Ccis[cciAllocatedInvoice.CciClientId(CciAllocatedInvoice.CciEnum.selected)];
                    if ((null != cci) && (false == Convert.ToBoolean(cci.NewValue)))
                    {
                        aCciAllocatedInvoice.Add(cciAllocatedInvoice);
                        aAllocatedInvoice.Add(m_InvoiceSettlement.AllocatedInvoice[i]);
                    }
                }
                m_InvoiceSettlement.AllocatedInvoiceSpecified = (0 < aAllocatedInvoice.Count);
                if (m_InvoiceSettlement.AllocatedInvoiceSpecified)
                {
                    m_InvoiceSettlement.AllocatedInvoice = (IAllocatedInvoice[])aAllocatedInvoice.ToArray(m_InvoiceSettlement.TypeofAllocatedInvoice);
                    m_CciAllocatedInvoice = (CciAllocatedInvoice[])aCciAllocatedInvoice.ToArray(typeof(CciAllocatedInvoice));
                }
                else
                    m_InvoiceSettlement.AllocatedInvoice = null;

                TotalFxGainOrLossCalculation();
                Dump_ToDocument();
                RefreshAvailableInvoice();
                Ccis.IsToSynchronizeWithDocument = true;
            }
        }
        #endregion DeleteBasket
        #region DumpOrderEntered_ToDocument
        /// <summary>
        /// Dump a orderEntered into DataDocument
        /// </summary>
        /// <param name="pData"></param>
        // EG 20171122 [23509] New
        private void DumpOrderEntered_ToDocument(string pData)
        {
            IPartyTradeInformation partyTradeInformation = GetPartyTradeInformationBeneficiary();
            if (null != partyTradeInformation)
            {
                if (Tz.Tools.IsDateFilled(pData))
                {
                    partyTradeInformation.Timestamps.OrderEntered = pData;
                    partyTradeInformation.Timestamps.OrderEnteredSpecified = true;
                    Nullable<DateTimeOffset> dtOrderEnteredInTimeZone = Tz.Tools.FromTimeZone(partyTradeInformation.Timestamps.OrderEnteredDateTimeOffset.Value, GetTimeZoneValue());
                    DataDocument.TradeHeader.TradeDate.Value = DtFunc.DateTimeToStringDateISO(dtOrderEnteredInTimeZone.Value.Date);
                }
                else
                {
                    partyTradeInformation.Timestamps.OrderEntered = string.Empty;
                    partyTradeInformation.Timestamps.OrderEnteredSpecified = false;
                    DataDocument.TradeHeader.TradeDate.Value = string.Empty;
                }
                partyTradeInformation.TimestampsSpecified = partyTradeInformation.Timestamps.OrderEnteredSpecified;
            }
        }
        #endregion DumpOrderEntered_ToDocument

        #region DumpSpecific_ToGUI
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            bool isAllocated = AllocatedInvoiceSpecified;
            bool isAvailable = AvailableInvoiceSpecified;
            if (isAllocated && (false == IsModeConsult))
            {
                for (int i = 0; i < AllocatedInvoiceLenght; i++)
                    m_CciAllocatedInvoice[i].LoadDDLCurrencyForAllocatedAmount(pPage);
            }
            // On Masque la table des factures allouées s'il n'y a pas d'allocation
            Panel pnl = (Panel) pPage.FindControl("divtblAllocatedInvoice");
            if (null != pnl)
                pnl.Visible = isAllocated;
            Table tblAllocatedInvoice = (Table)pPage.FindControl("tblAllocatedInvoice");
            if (null != tblAllocatedInvoice)
                tblAllocatedInvoice.Visible = isAllocated;
            // On Masque la table des factures allouables si le règlement est totalement alloué
            pnl = (Panel)pPage.FindControl("divtblAvailableInvoice");
            if (null != pnl)
                pnl.Visible = isAvailable;
            Table tblAvailableInvoice = (Table)pPage.FindControl("tblAvailableInvoice");
            if (null != tblAvailableInvoice)
                tblAvailableInvoice.Visible = isAvailable;

            isAllocated &= (false == IsModeConsult);
            isAvailable &= (false == IsModeConsult);
            // On masque le bouton AllocatedCheck
            CustomCaptureInfo cciButton = Cci(CciEnum.allocatedCheckInvoice);
            Control ctrl;
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAllocated;
            }
            // On masque le bouton AddBasket
            cciButton = Cci(CciEnum.selectAvailableInvoice);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAvailable;
            }
            // On masque le bouton RefreshAvailableInvoice
            cciButton = Cci(CciEnum.refreshAvailableInvoice);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAvailable;
            }            // On masque le bouton DeleteBasket
            cciButton = Cci(CciEnum.deleteAllocatedInvoice);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAllocated;
            }            // On masque le bouton FxGainOrLossCalculation
            cciButton = Cci(CciEnum.fxGainOrLossCalculation);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAllocated;
            }            // On masque le bouton AllocateBasket
            cciButton = Cci(CciEnum.allocateAllNewInvoice);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAllocated;
                // On masque le bouton ClearBasket
            } 
            cciButton = Cci(CciEnum.clearAllNewInvoice);
            // EG 20150128 Test null
            if (null != cciButton)
            {
                ctrl = pPage.FindControl(cciButton.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isAllocated;
            }
        }
        #endregion DumpSpecific_ToGUI
        #region GetArrayElementDocumentCount
        public override int GetArrayElementDocumentCount(string pPrefix, string pParentClientId, int pParentOccurs)
        {
            int ret = base.GetArrayElementDocumentCount(pPrefix, pParentClientId, pParentOccurs);
            if (-1 == ret)
            {
                if (TradeAdminCustomCaptureInfos.CCst.Prefix_AvailableInvoice == pPrefix)
                {
                    if (null != m_InvoiceSettlement.AvailableInvoice)
                        ret = ArrFunc.Count(m_InvoiceSettlement.AvailableInvoice);
                }
            }
            if (-1 == ret)
            {
                if (TradeAdminCustomCaptureInfos.CCst.Prefix_AllocatedInvoice == pPrefix)
                {
                    if (null != m_InvoiceSettlement.AllocatedInvoice)
                        ret = ArrFunc.Count(m_InvoiceSettlement.AllocatedInvoice);
                }
            }
            return ret;
        }
        #endregion GetArrayElementDocumentCount
        #region GetAccountingCurrency
        private void GetAccountingCurrency()
        {
            CustomCaptureInfo cciBookReceiver = cciParty[0].Cci(CciTradeParty.CciEnum.book);
            if (null != cciBookReceiver)
            {
                SQL_Book bookReceiver = (SQL_Book)cciBookReceiver.Sql_Table;

                if ((null != bookReceiver) && (0 < bookReceiver.IdA_Entity))
                {
                    SQL_Entity entity = new SQL_Entity(CS, bookReceiver.IdA_Entity);
                    if (entity.IsLoaded)
                        m_AccountingCurrency = entity.IdCAccount;
                }
            }
        }
        #endregion GetAccountingCurrency
        #region IsPreviousAllocatedInvoice
        private bool IsPreviousAllocatedInvoice(int pOTCmlId)
        {
            bool isExist = false;
            if ((null != m_PreviousAllocatedInvoice) && (0 < m_PreviousAllocatedInvoice.Length))
            {
                foreach (IAllocatedInvoice allocated in m_PreviousAllocatedInvoice)
                {
                    if (allocated.OTCmlId == pOTCmlId)
                    {
                        isExist = true;
                        break;
                    }
                }
            }
            return isExist;
        }
        #endregion IsPreviousAllocatedInvoice
        #region GetAllocatedInvoice
        public IAllocatedInvoice[] GetAllocatedInvoice()
        {
            return InvoiceSettlement.AllocatedInvoice;
        }
        #endregion GetAllocatedInvoice
        #region GetAvailableInvoice
        public IAvailableInvoice[] GetAvailableInvoice()
        {
            return InvoiceSettlement.AvailableInvoice;
        }
        #endregion GetAvailableInvoice
        #region RefreshAvailableInvoice
        // EG 20091208 Filter on Settlement/Cashcurrency
        // EG 20100909 Problème sur gestion message d'erreur à la saisie payer (HPC)
        //EG 20120613 Add Parameters
        // EG 20180425 Analyse du code Correction [CA2202]
        public void RefreshAvailableInvoice()
        {
            // EG 20091209 Test sur Settlement & CashCurrency pour filtre facture available
            CustomCaptureInfo cciSettlementCurrency = Ccis[CciClientId(CciEnum.settlementAmount_currency)];
            CustomCaptureInfo cciCashCurrency = Ccis[CciClientId(CciEnum.cashAmount_currency)];
            string settlementCurrency = cciSettlementCurrency.NewValue;
            string cashCurrency = cciCashCurrency.NewValue;
            CustomCaptureInfo cciActorReceiver = cciParty[0].Cci(CciTradeParty.CciEnum.actor);
            CustomCaptureInfo cciBookReceiver = cciParty[0].Cci(CciTradeParty.CciEnum.book);
            CustomCaptureInfo cciActorPayer = cciParty[1].Cci(CciTradeParty.CciEnum.actor);
            SQL_Actor actorReceiver = (SQL_Actor)cciActorReceiver.Sql_Table;
            SQL_Book bookReceiver = (SQL_Book)cciBookReceiver.Sql_Table;
            SQL_Actor actorPayer = (SQL_Actor)cciActorPayer.Sql_Table;
            ArrayList aAvailableInvoice = new ArrayList();
            if ((null != actorReceiver) && (null != bookReceiver) && (null != actorPayer)) // && UnAllocatedAmountSpecified)
            {
                DataParameters dbParam = new DataParameters();
                dbParam.Add(new DataParameter(CS, "IDA_BENEFICIARY", DbType.Int32), actorReceiver.Id);
                dbParam.Add(new DataParameter(CS, "IDB_BENEFICIARY", DbType.Int32), bookReceiver.Id);
                dbParam.Add(new DataParameter(CS, "IDA_INVOICED", DbType.Int32), actorPayer.Id);
                dbParam.Add(new DataParameter(CS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.REGULAR.ToString());
                dbParam.Add(new DataParameter(CS, "IDSTENVIRONMENT", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusEnvironment.REGULAR.ToString());
                dbParam.Add(new DataParameter(CS, "PRODUCT_IDENTIFIER", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), "credit");

                dbParam.Add(new DataParameter(CS, "EVENTCODE_LPP", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventCodeFunc.LinkedProductPayment);
                dbParam.Add(new DataParameter(CS, "EVENTTYPE_NTO", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.NetTurnOverAmount);
                dbParam.Add(new DataParameter(CS, "EVENTTYPE_NTI", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.NetTurnOverIssueAmount);
                dbParam.Add(new DataParameter(CS, "EVENTTYPE_NTA", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventTypeFunc.NetTurnOverAccountingAmount);
                dbParam.Add(new DataParameter(CS, "EVENTCLASS_REC", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Recognition);
                dbParam.Add(new DataParameter(CS, "EVENTCLASS_STL", DbType.AnsiString, SQLCst.UT_EVENT_LEN), EventClassFunc.Settlement);

                if (settlementCurrency != cashCurrency)
                    dbParam.Add(new DataParameter(CS, "NTI_CUR", DbType.AnsiStringFixedLength, SQLCst.UT_CURR_LEN), settlementCurrency);

                string SQLQuery = SQLCst.SELECT;
                // 20090407 EG ajout INVOICEDATE
                SQLQuery += "avi.IDT, avi.IDENTIFIER, avi.DTTRADE, avi.INVOICEDATE, " + Cst.CrLf;
                /* Nets initiaux */
                SQLQuery += "avi.NTO_AMOUNT as INIT_NTO_AMOUNT, avi.NTO_CUR as INIT_NTO_CUR," + Cst.CrLf;
                SQLQuery += "avi.NTI_AMOUNT as INIT_NTI_AMOUNT, avi.NTI_CUR as INIT_NTI_CUR," + Cst.CrLf;
                SQLQuery += "avi.NTA_AMOUNT as INIT_NTA_AMOUNT, avi.NTA_CUR as INIT_NTA_CUR," + Cst.CrLf;
                /* Nets des avoirs éventuels */
                SQLQuery += DataHelper.SQLIsNull(CS, "avc_nto.TURNOVER_AMOUNT", "0") + " as CREDIT_NTO_AMOUNT, " + Cst.CrLf;
                SQLQuery += DataHelper.SQLIsNull(CS, "avc_nti.TURNOVER_AMOUNT", "0") + " as CREDIT_NTI_AMOUNT, " + Cst.CrLf;
                SQLQuery += DataHelper.SQLIsNull(CS, "avc_nta.TURNOVER_AMOUNT", "0") + " as CREDIT_NTA_AMOUNT, " + Cst.CrLf;
                /* Nets alloués éventuels */
                SQLQuery += DataHelper.SQLIsNull(CS, "alloc_nto.TURNOVER_AMOUNT", "0") + " as ALLOC_NTO_AMOUNT, " + Cst.CrLf;
                SQLQuery += DataHelper.SQLIsNull(CS, "alloc_nti.TURNOVER_AMOUNT", "0") + " as ALLOC_NTI_AMOUNT, " + Cst.CrLf;
                SQLQuery += DataHelper.SQLIsNull(CS, "alloc_nta.TURNOVER_AMOUNT", "0") + " as ALLOC_NTA_AMOUNT, " + Cst.CrLf;
                /* Restant dus éventuels */
                SQLQuery += "avi.NTO_AMOUNT - " + DataHelper.SQLIsNull(CS, "avc_nto.TURNOVER_AMOUNT", "0");
                SQLQuery += " - " + DataHelper.SQLIsNull(CS, "alloc_nto.TURNOVER_AMOUNT", "0") + " as REMAIN_NTO_AMOUNT, " + Cst.CrLf;
                SQLQuery += "avi.NTI_AMOUNT - " + DataHelper.SQLIsNull(CS, "avc_nti.TURNOVER_AMOUNT", "0");
                SQLQuery += " - " + DataHelper.SQLIsNull(CS, "alloc_nti.TURNOVER_AMOUNT", "0") + " as REMAIN_NTI_AMOUNT, " + Cst.CrLf;
                SQLQuery += "avi.NTA_AMOUNT - " + DataHelper.SQLIsNull(CS, "avc_nta.TURNOVER_AMOUNT", "0");
                SQLQuery += " - " + DataHelper.SQLIsNull(CS, "alloc_nta.TURNOVER_AMOUNT", "0") + " as REMAIN_NTA_AMOUNT" + Cst.CrLf;

                SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_AVAILABLEINVOICESTL.ToString() + " avi" + Cst.CrLf;

                // EG 20120528 New Query version without View on left join
                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAvailableCreditREC(EventTypeFunc.NetTurnOverAmount) + ") avc_nto";
                SQLQuery += SQLCst.ON + "(avc_nto.IDT = avi.IDT)" + Cst.CrLf;
                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAvailableCreditREC(EventTypeFunc.NetTurnOverIssueAmount) + ") avc_nti";
                SQLQuery += SQLCst.ON + "(avc_nti.IDT = avi.IDT)" + Cst.CrLf;
                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAvailableCreditREC(EventTypeFunc.NetTurnOverAccountingAmount) + ") avc_nta";
                SQLQuery += SQLCst.ON + "(avc_nta.IDT = avi.IDT)" + Cst.CrLf;

                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAllocatedInvoiceSTL(EventTypeFunc.NetTurnOverAmount) + ") alloc_nto";
                SQLQuery += SQLCst.ON + "(alloc_nto.IDT = avi.IDT)" + Cst.CrLf;
                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAllocatedInvoiceSTL(EventTypeFunc.NetTurnOverIssueAmount) + ") alloc_nti";
                SQLQuery += SQLCst.ON + "(alloc_nti.IDT = avi.IDT)" + Cst.CrLf;
                SQLQuery += SQLCst.X_LEFT + "(" + GetQueryAllocatedInvoiceSTL(EventTypeFunc.NetTurnOverAccountingAmount) + ") alloc_nta";
                SQLQuery += SQLCst.ON + "(alloc_nta.IDT = avi.IDT)" + Cst.CrLf;


                SQLQuery += SQLCst.WHERE + "(avi.IDA_BENEFICIARY = @IDA_BENEFICIARY)";
                SQLQuery += SQLCst.AND + "(avi.IDB_BENEFICIARY = @IDB_BENEFICIARY)";
                SQLQuery += SQLCst.AND + "(avi.IDA_INVOICED = @IDA_INVOICED)";
                // EG 20100225 Add Where ROWATTRIBUT
                SQLQuery += SQLCst.AND + "(" + DataHelper.SQLIsNull(CS, "avi.ROWATTRIBUT", DataHelper.SQLString("X")) + "<>" + DataHelper.SQLString("C") + ")";
                if (settlementCurrency != cashCurrency)
                    SQLQuery += SQLCst.AND + "(avi.NTI_CUR = @NTI_CUR)";
                SQLQuery += SQLCst.ORDERBY + "avi.IDT";
                //
                QueryParameters qry = new QueryParameters(CS, SQLQuery, dbParam);
                //
                qry.Cs = CSTools.SetTimeOut(qry.Cs, 180);
                using (IDataReader dr = DataHelper.ExecuteReader(CSTools.SetCacheOn(qry.Cs), CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                {
                    while (dr.Read())
                    {
                        int idT = Convert.ToInt32(dr["IDT"]);
                        if (m_InvoiceSettlement.IsInvoiceNotSelected(idT))
                        {
                            if (0 < Convert.ToDecimal(dr["REMAIN_NTO_AMOUNT"]))
                            {
                                IAvailableInvoice availableInvoice = m_InvoiceSettlement.CreateAvailableInvoice();
                                availableInvoice.Identifier = new EFS_String(dr["IDENTIFIER"].ToString());
                                availableInvoice.OTCmlId = idT;
                                // 20090407 EG replace DTTRADE par INVOICEDATE
                                availableInvoice.InvoiceDate.DateValue = Convert.ToDateTime(dr["INVOICEDATE"]);
                                /* INITIAL NETTURNOVER */
                                availableInvoice.Amount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["INIT_NTO_AMOUNT"]));
                                availableInvoice.Amount.Currency = dr["INIT_NTO_CUR"].ToString();
                                availableInvoice.IssueAmount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["INIT_NTI_AMOUNT"]));
                                availableInvoice.IssueAmount.Currency = dr["INIT_NTI_CUR"].ToString();
                                if (false == Convert.IsDBNull(dr["INIT_NTA_AMOUNT"]))
                                    availableInvoice.AccountingAmount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["INIT_NTA_AMOUNT"]));
                                availableInvoice.AccountingAmount.Currency = dr["INIT_NTA_CUR"].ToString();
                                // 20090617 EG Retrancher les net avoirs sur les nets d'origine 
                                if (false == Convert.IsDBNull(dr["CREDIT_NTO_AMOUNT"]))
                                    availableInvoice.Amount.Amount.DecValue -= Convert.ToDecimal(dr["CREDIT_NTO_AMOUNT"]);
                                if (false == Convert.IsDBNull(dr["CREDIT_NTI_AMOUNT"]))
                                    availableInvoice.IssueAmount.Amount.DecValue -= Convert.ToDecimal(dr["CREDIT_NTI_AMOUNT"]);
                                if ((false == Convert.IsDBNull(dr["CREDIT_NTA_AMOUNT"])) && (false == Convert.IsDBNull(dr["INIT_NTA_AMOUNT"])))
                                    availableInvoice.AccountingAmount.Amount.DecValue -= Convert.ToDecimal(dr["CREDIT_NTA_AMOUNT"]);

                                /* REMAIN NETTURNOVER */
                                availableInvoice.AvailableAmounts.Amount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["REMAIN_NTO_AMOUNT"]));
                                availableInvoice.AvailableAmounts.Amount.Currency = dr["INIT_NTO_CUR"].ToString();
                                availableInvoice.AvailableAmounts.IssueAmount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["REMAIN_NTI_AMOUNT"]));
                                availableInvoice.AvailableAmounts.IssueAmount.Currency = dr["INIT_NTI_CUR"].ToString();
                                if (false == Convert.IsDBNull(dr["REMAIN_NTA_AMOUNT"]))
                                    availableInvoice.AvailableAmounts.AccountingAmount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["REMAIN_NTA_AMOUNT"]));
                                availableInvoice.AvailableAmounts.AccountingAmount.Currency = dr["INIT_NTA_CUR"].ToString();
                                /* CURRENT ALLOCATED NETTURNOVER */
                                availableInvoice.AllocatedAccountingAmount.Amount = new EFS_Decimal(Convert.ToDecimal(dr["ALLOC_NTA_AMOUNT"]));
                                availableInvoice.AllocatedAccountingAmount.Currency = dr["INIT_NTA_CUR"].ToString();
                                availableInvoice.AllocatedAccountingAmountSpecified = (0 < availableInvoice.AllocatedAccountingAmount.Amount.DecValue);

                                aAvailableInvoice.Add(availableInvoice);
                            }
                        }
                    }
                }
            }
            m_InvoiceSettlement.AvailableInvoiceSpecified = (0 < aAvailableInvoice.Count);
            if (m_InvoiceSettlement.AvailableInvoiceSpecified)
                m_InvoiceSettlement.AvailableInvoice = (IAvailableInvoice[])aAvailableInvoice.ToArray(m_InvoiceSettlement.TypeofAvailableInvoice);
            else
                m_InvoiceSettlement.AvailableInvoice = null;
            // 20100909 EG Synchronisation si et seulement si les Ccis sont sans Erreur.
            Ccis.IsToSynchronizeWithDocument = StrFunc.IsEmpty(Ccis.GetErrorMessage());
            //Ccis.IsToSynchronizeWithDocument = true;
        }
        #endregion RefreshAvailableInvoice
        #region GetQueryAvailableCreditREC
        //EG 20120613 Add Parameters
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200914 [XXXXX] Correction Alias (trsys_credit => tr_credit)
        private string GetQueryAvailableCreditREC(string pEventType)
        {
            string sqlQuery = @"select tr.IDT, sum(ev.VALORISATION) as TURNOVER_AMOUNT
            from dbo.TRADE tr
            inner join dbo.TRADELINK tl on (tl.IDT_B = tr.IDT)
            inner join dbo.TRADE tr_credit on (tr_credit.IDT = tl.IDT_A)
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr_credit.IDI)
            inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and (pr.IDENTIFIER = @PRODUCT_IDENTIFIER)
            inner join dbo.EVENT ev on (ev.IDT = tr_credit.IDT) and (ev.EVENTCODE = @EVENTCODE_LPP) and (ev.EVENTTYPE = @EVENTTYPE_{0})
            inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS = @EVENTCLASS_REC)
            where (tr.IDSTENVIRONMENT = @IDSTENVIRONMENT) and (tr.IDSTACTIVATION = @IDSTACTIVATION) and 
            (tr_credit.IDSTENVIRONMENT = @IDSTENVIRONMENT) and (tr_credit.IDSTACTIVATION = @IDSTACTIVATION)
            group by tr.IDT" + Cst.CrLf;
            return String.Format(sqlQuery, pEventType);
        }
        #endregion GetQueryAvailableCreditREC
        #region GetQueryAllocatedInvoiceSTL
        //EG 20120613 Add Parameters
        private string GetQueryAllocatedInvoiceSTL(string pEventType)
        {
            string SQLQuery = SQLCst.SELECT + "ev.IDT, sum(ev.VALORISATION) as TURNOVER_AMOUNT" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " ev" + Cst.CrLf;
            SQLQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + SQLCst.ON + "(ec.IDE = ev.IDE)" + SQLCst.AND;
            SQLQuery += "(ec.EVENTCLASS = @EVENTCLASS_STL)" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE;
            SQLQuery += "(ev.EVENTCODE = @EVENTCODE_LPP)" + SQLCst.AND + "(ev.EVENTTYPE = @EVENTTYPE_" + pEventType + ")" + SQLCst.AND;
            SQLQuery += "(ev.IDSTACTIVATION = @IDSTACTIVATION)" + Cst.CrLf;
            SQLQuery += SQLCst.GROUPBY + "ev.IDT" + Cst.CrLf;
            return SQLQuery;
        }
        #endregion GetQueryAllocatedInvoiceSTL
        #region SetPreviousAllocatedInvoice
        public void SetPreviousAllocatedInvoice()
        {
            if (m_InvoiceSettlement.AllocatedInvoiceSpecified && (0 < m_InvoiceSettlement.AllocatedInvoice.Length))
            {
                ArrayList aAllocatedInvoice = new ArrayList();
                aAllocatedInvoice.AddRange(m_InvoiceSettlement.AllocatedInvoice);
                m_PreviousAllocatedInvoice = (IAllocatedInvoice[])aAllocatedInvoice.ToArray(m_InvoiceSettlement.TypeofAllocatedInvoice);
            }
        }
        #endregion SetPreviousAllocatedInvoice
        #region TotalFxGainOrLossCalculation
        // EG 20091208 remainAllocatedAccountingAmount
        public void TotalFxGainOrLossCalculation()
        {
            decimal fxGainOrLossAmount = 0;
            decimal unAllocatedAmount = SettlementAmountValue;
            string settlementCurrency = SettlementCurrencyValue;
            decimal remainAllocatedAccountingAmount = 0;
            if (AllocatedInvoiceSpecified)
            {
                for (int i = 0; i < AllocatedInvoiceLenght; i++)
                {
                    CciAllocatedInvoice cciAllocated = m_CciAllocatedInvoice[i];
                    cciAllocated.AllocatedAmountCalculation();
                    fxGainOrLossAmount += cciAllocated.FxGainOrLossAmountValue;

                    if (settlementCurrency == cciAllocated.AllocatedAmountCurrencyValue)
                        unAllocatedAmount -= cciAllocated.AllocatedAmountValue;
                    else if (settlementCurrency == cciAllocated.AllocatedIssueAmountCurrencyValue)
                        unAllocatedAmount -= cciAllocated.AllocatedIssueAmountValue;
                    else if (settlementCurrency == cciAllocated.AllocatedAccountingAmountCurrencyValue)
                        unAllocatedAmount -= cciAllocated.AllocatedAccountingAmountValue;

                    if (settlementCurrency == CashCurrencyValue)
                        remainAllocatedAccountingAmount += cciAllocated.UnallocatedAccountingAmountValue;
                    else
                        remainAllocatedAccountingAmount += cciAllocated.UnallocatedIssueAmountValue;
                }
            }
            SetAndDisplayFxGainOrLoss(fxGainOrLossAmount, unAllocatedAmount);
            SetAndDisplayTotalRemainAllocatedAmount(remainAllocatedAccountingAmount, settlementCurrency);
        }
        #endregion TotalFxGainOrLossCalculation
        #region InitializeAccountNumber_FromCci
        private void InitializeAccountNumber_FromCci()
        {
            m_CciAccountNumber = new CciAccountNumber(this, TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceSettlementHeader, null)
            {
                AccountNumber = m_InvoiceSettlement.AccountNumber
            };
            if (Ccis.Contains(m_CciAccountNumber.CciClientId(CciAccountNumber.CciEnum.nostroAccount)))
                m_CciAccountNumber.Initialize_FromCci();
        }
        #endregion InitializeAccountNumber_FromCci
        #region InitializeAllocatedInvoice_FromCci
        private void InitializeAllocatedInvoice_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciAllocatedInvoice cciAllocatedInvoice = new CciAllocatedInvoice(this, TradeAdminCustomCaptureInfos.CCst.Prefix_AllocatedInvoice, index + 1, null);
                isOk = Ccis.Contains(cciAllocatedInvoice.CciClientId(CciAllocatedInvoice.CciEnum.identifier));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(GetAllocatedInvoice()) || (index == GetAllocatedInvoice().Length))
                        ReflectionTools.AddItemInArray(m_InvoiceSettlement, "allocatedInvoice", index);

                    string id = m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_AllocatedInvoice + Convert.ToString(index + 1);
                    if (false == IsPreviousAllocatedInvoice(GetAllocatedInvoice()[index].OTCmlId))
                        id = "NEW_" + id;

                    GetAllocatedInvoice()[index].Id = id;

                    cciAllocatedInvoice.AllocatedInvoice = GetAllocatedInvoice()[index];
                    if (null == cciAllocatedInvoice.AllocatedInvoice.UnAllocatedAmounts)
                        cciAllocatedInvoice.AllocatedInvoice.UnAllocatedAmounts = cciAllocatedInvoice.AllocatedInvoice.CreateNetInvoiceAmounts();
                    lst.Add(cciAllocatedInvoice);
                }
            }
            m_CciAllocatedInvoice = (CciAllocatedInvoice[])lst.ToArray(typeof(CciAllocatedInvoice));
            for (int i = 0; i < AllocatedInvoiceLenght; i++)
                m_CciAllocatedInvoice[i].Initialize_FromCci();
        }
        #endregion InitializeAllocatedInvoice_FromCci
        #region InitializeAvailableInvoice_FromCci
        private void InitializeAvailableInvoice_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciAvailableInvoice cciAvailableInvoice = new CciAvailableInvoice(this, TradeAdminCustomCaptureInfos.CCst.Prefix_AvailableInvoice, index + 1, null);
                isOk = Ccis.Contains(cciAvailableInvoice.CciClientId(CciAvailableInvoice.CciEnum.identifier));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(GetAvailableInvoice()) || (index == GetAvailableInvoice().Length))
                    {
                        ReflectionTools.AddItemInArray(m_InvoiceSettlement, "availableInvoice", index);
                    }
                    if (StrFunc.IsEmpty(GetAvailableInvoice()[index].Id))
                        GetAvailableInvoice()[index].Id = m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_AvailableInvoice + Convert.ToString(index + 1);

                    cciAvailableInvoice.AvailableInvoice = m_InvoiceSettlement.AvailableInvoice[index];
                    lst.Add(cciAvailableInvoice);
                }
            }
            m_CciAvailableInvoice = (CciAvailableInvoice[])lst.ToArray(typeof(CciAvailableInvoice));
            for (int i = 0; i < AvailableInvoiceLenght; i++)
                m_CciAvailableInvoice[i].Initialize_FromCci();
        }
        #endregion InitializeAvailableInvoice_FromCci
        #region SetAndDisplayCashAmount
        private void SetAndDisplayCashAmount()
        {
            if (IsCashAmountEqualToSettlementAmount)
                Ccis.SetNewValue(CciClientId(CciEnum.cashAmount_amount), StrFunc.FmtDecimalToInvariantCulture(SettlementAmountValue));
        }
        #endregion SetAndDisplayCashAmount
        #region SetAndDisplayFxGainOrLoss
        private void SetAndDisplayFxGainOrLoss(decimal pFxGainOrLossAmount, decimal pUnAllocatedAmount)
        {
            EFS_Cash cash = new EFS_Cash(CS, pFxGainOrLossAmount, CashCurrencyValue);
            Ccis.SetNewValue(CciClientId(CciEnum.fxGainOrLossAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
            Ccis.SetNewValue(CciClientId(CciEnum.fxGainOrLossAmount_currency), CashCurrencyValue);
            cash = new EFS_Cash(CS, pUnAllocatedAmount, SettlementCurrencyValue);
            Ccis.SetNewValue(CciClientId(CciEnum.unallocatedAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
            Ccis.SetNewValue(CciClientId(CciEnum.unallocatedAmount_currency), SettlementCurrencyValue);
        }
        #endregion SetAndDisplayFxGainOrLoss
        #region SetAndDisplayNetCashAmount
        private void SetAndDisplayNetCashAmount()
        {
            decimal netCashAmount = CashAmountValue - NetBankChargesAmountValue;
            Ccis.SetNewValue(CciClientId(CciEnum.netCashAmount_amount), StrFunc.FmtDecimalToInvariantCulture(netCashAmount));
            Ccis.SetNewValue(CciClientId(CciEnum.netCashAmount_currency), CashCurrencyValue);
        }
        #endregion SetAndDisplayNetCashAmount
        #region SetAndDisplayTotalRemainAllocatedAmount
        // EG 20091208 New
        private void SetAndDisplayTotalRemainAllocatedAmount(decimal pTotalRemainAllocatedAmount, string pTotalRemainAllocatedCurrency)
        {
            EFS_Cash cash = new EFS_Cash(CS, pTotalRemainAllocatedAmount, pTotalRemainAllocatedCurrency);
            Ccis.SetNewValue(CciClientId(CciEnum.totalRemainAllocatedAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
            Ccis.SetNewValue(CciClientId(CciEnum.totalRemainAllocatedAmount_currency), pTotalRemainAllocatedCurrency);
        }
        #endregion SetAndDisplayTotalRemainAllocatedAmount

        #region GetTimeZoneValue
        /// <summary>
        /// Retourne le timezone.
        /// </summary>
        /// <returns></returns>
        // EG 20171122 [23509] New
        private string GetTimeZoneValue()
        {
            string timezone = string.Empty;
            CustomCaptureInfo cciOrderEntered = Cci(CciEnum.orderEntered);

            // 1. Recherche du Timezone associé à la date d'exécution
            if (null != cciOrderEntered)
            {
                CustomCaptureInfo cciZone = Ccis[cciOrderEntered.ClientIdZone, false];
                if ((null != cciZone) && StrFunc.IsFilled(cciZone.NewValue))
                    timezone = cciZone.NewValue;
            }
            // 2. Recherche du Timezone associé à la plateforme puis à l'entité et Universal timezone si non trouvé
            if (StrFunc.IsEmpty(timezone))
            {
                timezone = DataDocument.GetTradeTimeZone(CSCacheOn, Ccis.User.Entity_IdA, Tz.Tools.UniversalTimeZone);
            }
            return timezone;
        }
        #endregion GetTimeZoneValue
        #region SynchronizeTimeZone
        private void SynchronizeTimeZone()
        {
            string timeZone = DataDocument.GetTradeTimeZone(CSCacheOn, Ccis.User.Entity_IdA);
            string clientIdZone = Cci(CciEnum.orderEntered).ClientId.Replace(Cst.TMS, Cst.TMZ);
            CustomCaptureInfo cciZone = Ccis[clientIdZone, false];
            if (cciZone.NewValue != timeZone)
                Ccis.SetNewValue(cciZone.ClientId, false, timeZone, false);
        }
        #endregion SynchronizeTimeZone
        
        #endregion Methods
    }
}
