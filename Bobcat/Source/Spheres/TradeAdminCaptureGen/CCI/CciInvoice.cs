#region Using Directives
using EFS.ACommon;
using EFS.Book;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Web.UI;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoice.
    /// </summary>
    public class CciInvoice : CciTradeAdminBase, IContainerCci, IContainerCciPayerReceiver, IContainerCciQuoteBasis
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceDate")]
            invoiceDate,

            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustedDate")]
            paymentDate_adjustedDate,

            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            paymentDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            paymentDate_adjustableDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            paymentDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateRelativeDate.periodMultiplier")]
            paymentDate_relativeDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateRelativeDate.period")]
            paymentDate_relativeDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateRelativeDate.dayType")]
            paymentDate_relativeDate_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableOrRelativeDateRelativeDate.businessDayConvention")]
            paymentDate_relativeDate_bDC,

            [System.Xml.Serialization.XmlEnumAttribute("grossTurnOverAmount.amount")]
            grossTurnOverAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("grossTurnOverAmount.currency")]
            grossTurnOverAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("rebateIsInExcess")]
            rebate_isInExcess,
            [System.Xml.Serialization.XmlEnumAttribute("rebateAmount.amount")]
            rebateAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("rebateAmount.currency")]
            rebateAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("rebateConditions.totalRebateAmount.amount")]
            totalRebateAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("rebateConditions.totalRebateAmount.currency")]
            totalRebateAmount_currency,

            [System.Xml.Serialization.XmlEnumAttribute("tax.amount.amount")]
            taxAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("tax.amount.currency")]
            taxAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("tax.issueAmount.amount")]
            taxIssueAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("tax.issueAmount.currency")]
            taxIssueAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("tax.accountingAmount.amount")]
            taxAccountingAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("tax.accountingAmount.currency")]
            taxAccountingAmount_currency,

            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverAmount.amount")]
            netTurnOverAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverAmount.currency")]
            netTurnOverAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverIssueAmount.amount")]
            netTurnOverIssueAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverIssueAmount.currency")]
            netTurnOverIssueAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverAccountingAmount.amount")]
            netTurnOverAccountingAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverAccountingAmount.currency")]
            netTurnOverAccountingAmount_currency,

            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverAccountingRate")]
            netTurnOverAccountingRate_rate,
            netTurnOverIssueRate_quoteCurrencyPair,
            [System.Xml.Serialization.XmlEnumAttribute("netTurnOverIssueRate")]
            netTurnOverIssueRate_rate,

            [System.Xml.Serialization.XmlEnumAttribute("accountingRateRead")]
            netTurnOverAccountingRate_rateRead,
            [System.Xml.Serialization.XmlEnumAttribute("accountingRateIsReverse")]
            netTurnOverAccountingRate_reverse,
            [System.Xml.Serialization.XmlEnumAttribute("issueRateRead")]
            netTurnOverIssueRate_rateRead,
            [System.Xml.Serialization.XmlEnumAttribute("issueRateIsReverse")]
            netTurnOverIssueRate_reverse,

            [System.Xml.Serialization.XmlEnumAttribute("rateSource.assetFxRateId")]
            invoiceRateSource_assetFxRate,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.rateSource.rateSource")]
            invoiceRateSource_rateSource,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.rateSource.rateSourcePage")]
            invoiceRateSource_rateSourcePage,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.rateSource.rateSourcePageHeading")]
            invoiceRateSource_rateSourcePageHeading,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingTime.hourMinuteTime")]
            invoiceRateSource_fixingTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingTime.businessCenter")]
            invoiceRateSource_fixingTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingDate.dateRelativeTo")]
            invoiceRateSource_fixingDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingDate.period")]
            invoiceRateSource_fixingDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingDate.periodMultiplier")]
            invoiceRateSource_fixingDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingDate.businessDayConvention")]
            invoiceRateSource_fixingDate_businessDayConvention,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.fixingDate.dayType")]
            invoiceRateSource_fixingDate_dayType,
            [System.Xml.Serialization.XmlEnumAttribute("invoiceRateSource.adjustedFixingDate")]
            invoiceRateSource_fixingDate,

            quotedCurrencyPair_currency1,
            quotedCurrencyPair_currency2,
            quotedCurrencyPair_quoteBasis,

            [System.Xml.Serialization.XmlEnumAttribute("invoiceDetails")]
            invoiceDetails,

            feesDetail,
            rebatesDetail,

            [System.Xml.Serialization.XmlEnumAttribute("rebateConditions")]
            rebateConditionsSpecified,

            unknown,
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        private readonly IInvoice m_Invoice;
        private readonly string m_Prefix;
        private CciInvoiceRebate m_CciInvoiceRebate;
        private CciInvoiceTrade[] m_CciInvoiceTrade;
        #endregion Members
        //
        #region Accessors
        #region ExistCciAssetFxRate
        public bool ExistCciAssetFxRate
        {
            get { return Ccis.Contains(CciClientId(CciEnum.invoiceRateSource_assetFxRate)); }
        }
        #endregion ExistCciAssetFxRate
        #region ExistInvoiceRebate
        public bool ExistInvoiceRebate
        {
            get { return (null != m_CciInvoiceRebate); }
        }
        #endregion ExistInvoiceRebate
        #region Invoice
        public override IInvoice Invoice
        {
            get { return m_Invoice; }
        }
        #endregion Invoice
        #region InvoiceDetails
        public IInvoiceDetails InvoiceDetails
        {
            get { return m_Invoice.InvoiceDetails; }
        }
        #endregion InvoiceTrade
        #region InvoiceTrade
        /*
        public IInvoiceTrade[] InvoiceTrade
        {
            get { return InvoiceDetails.invoiceTrade; }
        }
        */
        #endregion InvoiceTrade
        #region InvoiceTradeLenght
        public int InvoiceTradeLenght
        {
            get { return ArrFunc.IsFilled(m_CciInvoiceTrade) ? m_CciInvoiceTrade.Length : 0; }
        }
        #endregion InvoiceTradeLenght
        #region IsModeConsult
        public bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(Ccis.CaptureMode); }
        }
        #endregion IsModeConsult
        #endregion Accessors
        //
        #region Constructors
        // EG 20120620 Suppression initialisation m_CciInvoiceRebate
        // EG 20180205 [23769] Del EFS_TradeLibray 
        public CciInvoice(TradeAdminCustomCaptureInfos pCcis)
            : base(pCcis)
        {
            m_Invoice = (IInvoice)CurrentTrade.Product;
            m_Prefix = TradeAdminCustomCaptureInfos.CCst.Prefix_Invoice + CustomObject.KEY_SEPARATOR;
            //new EFS_TradeLibrary(TradeCommonInput.FpMLDataDocReader);
            //m_CciInvoiceRebate = new CciInvoiceRebate(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateConditions, m_Invoice.rebateConditions);
        }
        #endregion Constructors
        //
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

            if (false == Ccis.Contains(CciClientId(CciEnum.invoiceRateSource_fixingDate_businessDayConvention)))
            {
                CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.invoiceRateSource_fixingDate_businessDayConvention), false, TypeData.TypeDataEnum.@string);
                Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate_businessDayConvention), BusinessDayConventionEnum.NONE.ToString());
            }

            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.quotedCurrencyPair_currency1), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.quotedCurrencyPair_currency2), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.CHK + CciClientId(CciEnum.rebateConditionsSpecified), true, TypeData.TypeDataEnum.@string);

            if (ExistInvoiceRebate)
                m_CciInvoiceRebate.AddCciSystem();

            for (int i = 0; i < InvoiceTradeLenght; i++)
                m_CciInvoiceTrade[i].AddCciSystem();


        }
        #endregion AddCciSystem
        #region Dump_ToDocument
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
                            m_Invoice.PayerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            m_Invoice.ReceiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion Receiver
                            break;
                        case CciEnum.invoiceDate:
                            #region InvoiceDate
                            m_Invoice.InvoiceDate.Value = data;
                            if (StrFunc.IsEmpty(m_Invoice.InvoiceDate.Id))
                                m_Invoice.InvoiceDate.Id = TradeAdminCustomCaptureInfos.CCst.INVOICEDATE_REFERENCE;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion InvoiceDate
                            break;
                        case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                            #region PaymentDate (AdjustableDate)
                            m_Invoice.PaymentDate.AdjustableDateSpecified = cci.IsFilledValue;
                            m_Invoice.PaymentDate.RelativeDateSpecified = (false == cci.IsFilledValue);
                            m_Invoice.PaymentDate.AdjustableDate.UnadjustedDate.Value = data;
                            if (StrFunc.IsEmpty(m_Invoice.PaymentDate.AdjustableDate.UnadjustedDate.Id))
                            {
                                m_Invoice.PaymentDate.AdjustableDate.UnadjustedDate.Id = TradeAdminCustomCaptureInfos.CCst.INVOICEPAYMENTDATE_REFERENCE;
                            }
                            #endregion PaymentDate (AdjustableDate)
                            break;
                        case CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC:
                            #region PaymentDate BDC (AdjustableDate)
                            if (m_Invoice.PaymentDate.AdjustableDateSpecified)
                            {
                                BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                m_Invoice.PaymentDate.AdjustableDate.DateAdjustments.BusinessDayConvention = bDCEnum;
                            }
                            #endregion PaymentDate BDC (AdjustableDate)
                            break;
                        case CciEnum.paymentDate_adjustedDate:
                            #region PaymentDate Adjusted date
                            m_Invoice.PaymentDate.AdjustedDateSpecified = cci.IsFilledValue;
                            m_Invoice.PaymentDate.AdjustedDate.Value = data;
                            #endregion PaymentDate Adjusted date
                            break;
                        case CciEnum.paymentDate_relativeDate_dateRelativeTo:
                            #region PaymentDate RelativeTo( RelativeDate)
                            m_Invoice.PaymentDate.RelativeDateSpecified = cci.IsFilledValue;
                            m_Invoice.PaymentDate.AdjustableDateSpecified = (false == cci.IsFilledValue);
                            m_Invoice.PaymentDate.RelativeDate.DateRelativeToValue = data;
                            #endregion PaymentDate RelativeTo( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_period:
                            #region PaymentDate Period ( RelativeDate)
                            if (m_Invoice.PaymentDate.RelativeDateSpecified)
                            {
                                PeriodEnum periodEnum = StringToEnum.Period(data);
                                m_Invoice.PaymentDate.RelativeDate.Period = periodEnum;
                            }
                            #endregion PaymentDate Period ( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_periodMultiplier:
                            #region PaymentDate Multiplier( RelativeDate)
                            if (m_Invoice.PaymentDate.RelativeDateSpecified)
                                m_Invoice.PaymentDate.RelativeDate.PeriodMultiplier.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low; // if period <> D => DayType = empty
                            #endregion PaymentDate Multiplier( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_bDC:
                            #region PaymentDate BDC( RelativeDate)
                            if (m_Invoice.PaymentDate.RelativeDateSpecified)
                            {
                                if (cci.IsFilledValue)
                                {
                                    BusinessDayConventionEnum bDCEnum = StringToEnum.BusinessDayConvention(data);
                                    m_Invoice.PaymentDate.RelativeDate.BusinessDayConvention = bDCEnum;
                                }
                            }
                            #endregion PaymentDate BDC( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_dayType:
                            #region PaymentDate DayType( RelativeDate)
                            if (m_Invoice.PaymentDate.RelativeDateSpecified)
                            {
                                m_Invoice.PaymentDate.RelativeDate.DayTypeSpecified = cci.IsFilledValue;
                                if (cci.IsFilledValue)
                                {
                                    DayTypeEnum dayTypeEnum = StringToEnum.DayType(data);
                                    m_Invoice.PaymentDate.RelativeDate.DayType = dayTypeEnum;
                                }
                            }
                            #endregion PaymentDate DayType( RelativeDate)
                            break;
                        case CciEnum.grossTurnOverAmount_amount:
                            #region GrossTurnOverAmount (Amount)
                            m_Invoice.GrossTurnOverAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion GrossTurnOverAmount (Amount)
                            break;
                        case CciEnum.grossTurnOverAmount_currency:
                            #region GrossTurnOverAmount (Currency)
                            m_Invoice.GrossTurnOverAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion GrossTurnOverAmount (Currency)
                            break;
                        case CciEnum.totalRebateAmount_amount:
                            #region TotalRebateAmount (Amount)
                            m_Invoice.RebateConditions.TotalRebateAmountSpecified = cci.IsFilledValue;
                            m_Invoice.RebateAmountSpecified = cci.IsFilledValue;
                            m_Invoice.RebateConditionsSpecified = m_Invoice.RebateConditions.TotalRebateAmountSpecified ||
                                                                  m_Invoice.RebateConditions.CapConditionsSpecified ||
                                                                  m_Invoice.RebateConditions.BracketConditionsSpecified;
                            if (m_Invoice.RebateConditionsSpecified &&
                                m_Invoice.RebateConditions.TotalRebateAmountSpecified)
                            {
                                m_Invoice.RebateConditions.TotalRebateAmount.Amount.Value = data;
                                m_Invoice.RebateConditions.TotalRebateAmount.Currency = m_Invoice.GrossTurnOverAmount.Currency;
                                m_Invoice.RebateAmount.Amount.Value = data;
                                m_Invoice.RebateAmount.Currency = m_Invoice.GrossTurnOverAmount.Currency;
                            }
                            else
                            {
                                m_Invoice.RebateConditions.TotalRebateAmount.Amount.DecValue = 0;
                                m_Invoice.RebateConditions.TotalRebateAmount.Currency = string.Empty;
                                m_Invoice.RebateAmount.Amount.DecValue = 0;
                                m_Invoice.RebateAmount.Currency = string.Empty;
                            }
                            CalculNetAmount();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion TotalRebateAmount (Amount)
                            break;
                        case CciEnum.taxAmount_amount:
                            #region TaxAmount (Amount)
                            m_Invoice.TaxSpecified = cci.IsFilledValue;
                            if (m_Invoice.TaxSpecified)
                                m_Invoice.Tax.Amount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion TaxAmount (Amount)
                            break;
                        case CciEnum.taxAmount_currency:
                            #region TaxAmount (Currency)
                            if (m_Invoice.TaxSpecified)
                                m_Invoice.Tax.Amount.Currency = data;
                            #endregion TaxAmount (Currency)
                            break;
                        case CciEnum.taxIssueAmount_amount:
                            #region TaxIssueAmount (Amount)
                            if (m_Invoice.TaxSpecified)
                                m_Invoice.Tax.IssueAmount.Amount.Value = data;
                            #endregion TaxIssueAmount (Amount)
                            break;
                        case CciEnum.taxIssueAmount_currency:
                            #region TaxIssueAmount (Currency)
                            if (m_Invoice.TaxSpecified)
                            {
                                m_Invoice.Tax.IssueAmountSpecified = true;
                                m_Invoice.Tax.IssueAmount.Currency = data;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion TaxIssueAmount (Currency)
                            break;
                        case CciEnum.taxAccountingAmount_amount:
                            #region TaxAccountingAmount (Amount)
                            if (m_Invoice.TaxSpecified)
                                m_Invoice.Tax.AccountingAmount.Amount.Value = data;
                            #endregion TaxAccountingAmount (Amount)
                            break;
                        case CciEnum.taxAccountingAmount_currency:
                            #region TaxAccountingAmount (Currency)
                            if (m_Invoice.TaxSpecified)
                            {
                                m_Invoice.Tax.AccountingAmountSpecified = cci.IsFilledValue;
                                if (m_Invoice.Tax.AccountingAmountSpecified)
                                    m_Invoice.Tax.AccountingAmount.Currency = data;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion TaxAccountingAmount (Currency)
                            break;
                        case CciEnum.invoiceRateSource_assetFxRate:
                            #region InvoiceRateSource (OTCmlId)
                            SQL_AssetFxRate sql_Asset = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            if (StrFunc.IsFilled(data))
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_Asset = new SQL_AssetFxRate(CS, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1);
                                    if (isLoaded)
                                        break;
                                }
                                m_Invoice.InvoiceRateSourceSpecified = isLoaded;
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_Asset.Identifier;
                                    cci.Sql_Table = sql_Asset;
                                }
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                            }
                            else
                                m_Invoice.InvoiceRateSourceSpecified = false;

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion InvoiceRateSource (OTCmlId)
                            break;
                        case CciEnum.netTurnOverAmount_amount:
                            #region NetTurnOverAmount (Amount)
                            m_Invoice.NetTurnOverAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.netTurnOverAmount_currency:
                            #region NetTurnOverAmount (Currency)
                            m_Invoice.NetTurnOverAmount.Currency = data;
                            #endregion NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.netTurnOverIssueAmount_amount:
                            #region NetTurnOverIssueAmount (Amount)
                            m_Invoice.NetTurnOverIssueAmount.Amount.Value = data;
                            #endregion NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.netTurnOverIssueAmount_currency:
                            #region NetTurnOverIssueAmount (Currency)
                            m_Invoice.NetTurnOverIssueAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.netTurnOverAccountingAmount_amount:
                            #region NetTurnOverAccountingAmount (Amount)
                            m_Invoice.NetTurnOverAccountingAmount.Amount.Value = data;
                            #endregion NetTurnOverAccountingAmount (Amount)
                            break;
                        case CciEnum.netTurnOverAccountingAmount_currency:
                            #region NetTurnOverAccountingAmount (Currency)
                            m_Invoice.NetTurnOverAccountingAmountSpecified = cci.IsFilledValue;
                            m_Invoice.NetTurnOverAccountingAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion NetTurnOverAccountingAmount (Currency)
                            break;
                        case CciEnum.invoiceRateSource_rateSource:
                            #region RateSource
                            m_Invoice.InvoiceRateSource.RateSource.RateSource.Value = data;
                            #endregion RateSource
                            break;
                        case CciEnum.invoiceRateSource_rateSourcePage:
                            #region RateSourcePage
                            m_Invoice.InvoiceRateSource.RateSource.RateSourcePageSpecified = cci.IsFilledValue;
                            m_Invoice.InvoiceRateSource.RateSource.RateSourcePage.Value = data;
                            #endregion RateSourcePage
                            break;
                        case CciEnum.invoiceRateSource_rateSourcePageHeading:
                            #region RateSourcePageHeading
                            m_Invoice.InvoiceRateSource.RateSource.RateSourcePageHeadingSpecified = cci.IsFilledValue;
                            m_Invoice.InvoiceRateSource.RateSource.RateSourcePageHeading = data;
                            #endregion RateSourcePageHeading
                            break;
                        case CciEnum.invoiceRateSource_fixingTime_hourMinuteTime:
                            #region fixingTime (HourMinuteTime)
                            m_Invoice.InvoiceRateSource.FixingTime.HourMinuteTime.Value = data;
                            #endregion fixingTime (HourMinuteTime)
                            break;
                        case CciEnum.invoiceRateSource_fixingTime_businessCenter:
                            #region fixingTime (BusinessCenter)
                            m_Invoice.InvoiceRateSource.FixingTime.BusinessCenter.Value = data;
                            #endregion fixingTime (BusinessCenter)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate:
                            #region FixingDate (Date)
                            m_Invoice.InvoiceRateSource.AdjustedFixingDate = ((IProduct)m_Invoice).ProductBase.CreateAdjustedDate(data);
                            CalculNetIssueAmount();
                            #endregion FixingDate (Date)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_dateRelativeTo:
                            #region FixingDate (DateRelativeTo)
                            m_Invoice.InvoiceRateSource.FixingDate.DateRelativeToValue = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion FixingDate (DateRelativeTo)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_period:
                            #region FixingDate (Period)
                            m_Invoice.InvoiceRateSource.FixingDate.Period = StringToEnum.Period(data);
                            #endregion FixingDate (Period)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_periodMultiplier:
                            #region FixingDate (PeriodMultiplier)
                            m_Invoice.InvoiceRateSource.FixingDate.PeriodMultiplier.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion FixingDate (PeriodMultiplier)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_dayType:
                            #region FixingDate (DayType)
                            m_Invoice.InvoiceRateSource.FixingDate.DayTypeSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                m_Invoice.InvoiceRateSource.FixingDate.DayType = StringToEnum.DayType(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion FixingDate (DayType)
                            break;
                        case CciEnum.rebateConditionsSpecified:
                            #region RebateConditionsSpecified
                            m_Invoice.RebateConditionsSpecified = cci.IsFilledValue;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            #endregion RebateConditionsSpecified
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

            for (int i = 0; i < InvoiceTradeLenght; i++)
                m_CciInvoiceTrade[i].Dump_ToDocument();

            if (ExistInvoiceRebate)
                m_CciInvoiceRebate.Dump_ToDocument();

            base.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region DumpSpecific_ToGUI
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            base.DumpSpecific_ToGUI(pPage);
            //
            cciTradeHeader.DumpSpecific_ToGUI(pPage);
            //
            bool isTaxSpecified = m_Invoice.TaxSpecified;
            CustomCaptureInfo cci = Cci(CciEnum.taxAmount_amount);
            Control ctrl = null;
            if (null != cci)
            {
                pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
                ctrl = pPage.FindControl("LBL" + cci.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            cci = Cci(CciEnum.taxAmount_currency);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            cci = Cci(CciEnum.taxIssueAmount_amount);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
                ctrl = pPage.FindControl("LBL" + cci.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            cci = Cci(CciEnum.taxIssueAmount_currency);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            cci = Cci(CciEnum.taxAccountingAmount_amount);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
                ctrl = pPage.FindControl("LBL" + cci.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            cci = Cci(CciEnum.taxAccountingAmount_currency);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isTaxSpecified;
            }
            //
            bool isRateReverse = false;
            cci = Cci(CciEnum.netTurnOverIssueRate_rateRead);
            if (null != cci)
            {
                isRateReverse = (cci.IsFilledValue);
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isRateReverse;
            }
            cci = Cci(CciEnum.netTurnOverIssueRate_reverse);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = false;

                ctrl = pPage.FindControl("LBL" + cci.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = (isRateReverse);
            }
            cci = Cci(CciEnum.netTurnOverIssueRate_rate);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = (false == isRateReverse);
            }
            //
            cci = Cci(CciEnum.netTurnOverAccountingRate_rateRead);
            if (null != cci)
            {
                isRateReverse = (cci.IsFilledValue);
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = isRateReverse;
            }
            cci = Cci(CciEnum.netTurnOverAccountingRate_reverse);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = false;

                ctrl = pPage.FindControl("LBL" + cci.ClientId_WithoutPrefix);
                if (null != ctrl)
                    ctrl.Visible = (isRateReverse);
            }
            cci = Cci(CciEnum.netTurnOverAccountingRate_rate);
            if (null != cci)
            {
                ctrl = pPage.FindControl(cci.ClientId);
                if (null != ctrl)
                    ctrl.Visible = (false == isRateReverse);
            }
        }
        #endregion DumpSpecific_ToGUI
        #region Initialize_FromCci
        // EG 20120620 Gestion écran Light ou Enhanced (avec détails factures : frais + Remises)
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_Invoice);
            if (Ccis.Contains(CciClientId(CciEnum.invoiceDetails)))
            {
                InitializeInvoiceRebateConditions_FromCci();
                InitializeInvoiceTrade_FromCci();
            }
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            //CustomCaptureInfosBase.ProcessQueueEnum processQueue;
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
                            data = m_Invoice.PayerPartyReference.HRef;
                            #endregion Payer
                            break;
                        case CciEnum.receiver:
                            #region Receiver
                            data = m_Invoice.ReceiverPartyReference.HRef;
                            #endregion Receiver
                            break;
                        case CciEnum.invoiceDate:
                            #region InvoiceDate
                            data = m_Invoice.InvoiceDate.Value;
                            #endregion InvoiceDate
                            break;
                        case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                            #region PaymentDate (AdjustableDate)
                            data = m_Invoice.PaymentDate.AdjustableDate.UnadjustedDate.Value;
                            #endregion PaymentDate (AdjustableDate)
                            break;
                        case CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC:
                            #region PaymentDate BusinessDayConvention (AdjustableDate)
                            data = m_Invoice.PaymentDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                            #endregion PaymentDate BusinessDayConvention (AdjustableDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_dateRelativeTo:
                            #region PaymentDate RelativeTo( RelativeDate)
                            data = m_Invoice.PaymentDate.RelativeDate.DateRelativeToValue;
                            #endregion PaymentDate RelativeTo( RelativeDate)
                            break;
                        case CciEnum.paymentDate_adjustedDate:
                            #region PaymentDate Adjusted date
                            data = m_Invoice.PaymentDate.AdjustedDate.Value;
                            #endregion PaymentDate Adjusted date
                            break;
                        case CciEnum.paymentDate_relativeDate_period:
                            #region PaymentDate Period (RelativeDate)
                            data = m_Invoice.PaymentDate.RelativeDate.Period.ToString();
                            #endregion PaymentDate Period (RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_periodMultiplier:
                            #region PaymentDate Multiplier( RelativeDate)
                            data = m_Invoice.PaymentDate.RelativeDate.PeriodMultiplier.Value;
                            #endregion PaymentDate Multiplier( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_bDC:
                            #region PaymentDate BDC( RelativeDate)
                            data = m_Invoice.PaymentDate.RelativeDate.BusinessDayConvention.ToString();
                            #endregion PaymentDate BDC( RelativeDate)
                            break;
                        case CciEnum.paymentDate_relativeDate_dayType:
                            #region PaymentDate DayType( RelativeDate)
                            data = m_Invoice.PaymentDate.RelativeDate.DayType.ToString();
                            #endregion PaymentDate DayType( RelativeDate)
                            break;
                        case CciEnum.grossTurnOverAmount_amount:
                            #region GrossTurnOverAmount (Amount)
                            data = m_Invoice.GrossTurnOverAmount.Amount.Value;
                            #endregion GrossTurnOverAmount (Amount)
                            break;
                        case CciEnum.grossTurnOverAmount_currency:
                            #region GrossTurnOverAmount (Currency)
                            data = m_Invoice.GrossTurnOverAmount.Currency;
                            #endregion GrossTurnOverAmount (Currency)
                            break;
                        case CciEnum.rebate_isInExcess:
                            #region RebateAmount (InExcess)
                            if (m_Invoice.RebateAmountSpecified)
                            {
                                if (m_Invoice.RebateIsInExcessSpecified && m_Invoice.RebateIsInExcess.BoolValue)
                                    data = Ressource.GetString(keyEnum.ToString());
                            }
                            #endregion RebateAmount (InExcess)
                            break;
                        case CciEnum.rebateAmount_amount:
                            #region RebateAmount (Amount)
                            if (m_Invoice.RebateAmountSpecified)
                                data = m_Invoice.RebateAmount.Amount.Value;
                            #endregion RebateAmount (Amount)
                            break;
                        case CciEnum.rebateAmount_currency:
                            #region RebateAmount (Currency)
                            if (m_Invoice.RebateAmountSpecified)
                                data = m_Invoice.RebateAmount.Currency;
                            Ccis.Set(CciClientId(CciEnum.rebateAmount_currency), "IsEnabled", false);
                            #endregion RebateAmount (Currency)
                            break;
                        case CciEnum.totalRebateAmount_amount:
                            #region TotalRebateAmount (Amount)
                            if (m_Invoice.RebateConditionsSpecified &&
                                m_Invoice.RebateConditions.TotalRebateAmountSpecified)
                                data = m_Invoice.RebateConditions.TotalRebateAmount.Amount.Value;
                            #endregion TotalRebateAmount (Amount)
                            break;
                        case CciEnum.totalRebateAmount_currency:
                            #region TotalRebateAmount (Currency)
                            if (m_Invoice.RebateConditionsSpecified &&
                                m_Invoice.RebateConditions.TotalRebateAmountSpecified)
                                data = m_Invoice.RebateConditions.TotalRebateAmount.Currency;
                            Ccis.Set(CciClientId(CciEnum.totalRebateAmount_currency), "IsEnabled", false);
                            #endregion TotalRebateAmount (Currency)
                            break;
                        case CciEnum.taxAmount_amount:
                            #region TaxAmount (Amount)
                            if (m_Invoice.TaxSpecified)
                            {
                                data = m_Invoice.Tax.Amount.Amount.Value;
                            }
                            #endregion TaxAmount (Amount)
                            break;
                        case CciEnum.taxAmount_currency:
                            #region TaxAmount (Currency)
                            if (m_Invoice.TaxSpecified)
                            {
                                data = m_Invoice.Tax.Amount.Currency;
                                Ccis.Set(CciClientId(CciEnum.taxAmount_currency), "IsEnabled", false);
                            }
                            #endregion TaxAmount (Currency)
                            break;
                        case CciEnum.taxIssueAmount_amount:
                            #region TaxIssueAmount (Amount)
                            if (m_Invoice.TaxSpecified)
                            {
                                data = m_Invoice.Tax.IssueAmount.Amount.Value;
                            }
                            #endregion TaxIssueAmount (Amount)
                            break;
                        case CciEnum.taxIssueAmount_currency:
                            #region TaxIssueAmount (Currency)
                            if (m_Invoice.TaxSpecified)
                            {
                                data = m_Invoice.Tax.IssueAmount.Currency;
                                isToValidate = StrFunc.IsEmpty(data);
                                Ccis.Set(CciClientId(CciEnum.taxIssueAmount_currency), "IsEnabled", false);
                            }
                            #endregion TaxIssueAmount (Currency)
                            break;
                        case CciEnum.taxAccountingAmount_amount:
                            #region TaxAccountingAmount (Amount)
                            if (m_Invoice.TaxSpecified && m_Invoice.NetTurnOverAccountingAmountSpecified)
                            {
                                data = m_Invoice.Tax.AccountingAmount.Amount.Value;
                            }
                            #endregion TaxAccountingAmount (Amount)
                            break;
                        case CciEnum.taxAccountingAmount_currency:
                            #region TaxAccountingAmount (Currency)
                            if (m_Invoice.TaxSpecified && m_Invoice.NetTurnOverAccountingAmountSpecified)
                            {
                                data = m_Invoice.Tax.AccountingAmount.Currency;
                                isToValidate = StrFunc.IsEmpty(data);
                                Ccis.Set(CciClientId(CciEnum.taxAccountingAmount_currency), "IsEnabled", false);
                            }
                            #endregion TaxAccountingAmount (Currency)
                            break;
                        case CciEnum.netTurnOverAmount_amount:
                            #region NetTurnOverAmount (Amount)
                            data = m_Invoice.NetTurnOverAmount.Amount.Value;
                            #endregion NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.netTurnOverAmount_currency:
                            #region NetTurnOverAmount (Currency)
                            data = m_Invoice.NetTurnOverAmount.Currency;
                            Ccis.Set(CciClientId(CciEnum.netTurnOverAmount_currency), "IsEnabled", false);
                            #endregion NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.netTurnOverIssueAmount_amount:
                            #region NetTurnOverIssueAmount (Amount)
                            data = m_Invoice.NetTurnOverIssueAmount.Amount.Value;
                            #endregion NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.netTurnOverIssueAmount_currency:
                            #region NetTurnOverIssueAmount (Currency)
                            data = m_Invoice.NetTurnOverIssueAmount.Currency;
                            isToValidate = StrFunc.IsEmpty(data);
                            Ccis.Set(CciClientId(CciEnum.netTurnOverIssueAmount_currency), "IsEnabled", false);
                            #endregion NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.netTurnOverAccountingAmount_amount:
                            #region NetTurnOverAccountingAmount (Amount)
                            if (m_Invoice.NetTurnOverAccountingAmountSpecified)
                            {
                                data = m_Invoice.NetTurnOverAccountingAmount.Amount.Value;
                            }
                            #endregion NetTurnOverAccountingAmount (Amount)
                            break;
                        case CciEnum.netTurnOverAccountingAmount_currency:
                            #region NetTurnOverAccountingAmount (Currency)
                            if (m_Invoice.NetTurnOverAccountingAmountSpecified)
                            {
                                data = m_Invoice.NetTurnOverAccountingAmount.Currency;
                                isToValidate = StrFunc.IsEmpty(data);
                                Ccis.Set(CciClientId(CciEnum.netTurnOverAccountingAmount_currency), "IsEnabled", false);
                            }
                            #endregion NetTurnOverAccountingAmount (Currency)
                            break;
                        case CciEnum.netTurnOverAccountingRate_rate:
                            #region NetTurnOverAccountingRate
                            if (m_Invoice.NetTurnOverAccountingRateSpecified)
                                data = m_Invoice.NetTurnOverAccountingRate.Value;
                            #endregion NetTurnOverAccountingRate
                            break;
                        case CciEnum.netTurnOverAccountingRate_rateRead:
                            #region NetTurnOverAccountingRateRead
                            if (m_Invoice.AccountingRateReadSpecified)
                                data = m_Invoice.AccountingRateRead.Value;
                            #endregion NetTurnOverAccountingRateRead
                            break;
                        case CciEnum.netTurnOverAccountingRate_reverse:
                            #region NetTurnOverAccountingRate Reverse
                            if (m_Invoice.AccountingRateIsReverseSpecified)
                                data = m_Invoice.AccountingRateIsReverse.Value;
                            #endregion NetTurnOverAccountingRate Reverse
                            break;
                        case CciEnum.netTurnOverIssueRate_rate:
                            #region NetTurnOverIssueRate
                            if (m_Invoice.NetTurnOverIssueRateSpecified)
                                data = m_Invoice.NetTurnOverIssueRate.Value;
                            #endregion NetTurnOverIssueRate
                            break;
                        case CciEnum.netTurnOverIssueRate_rateRead:
                            #region NetTurnOverIssueRateRead
                            if (m_Invoice.IssueRateReadSpecified)
                                data = m_Invoice.IssueRateRead.Value;
                            #endregion NetTurnOverIssueRateRead
                            break;
                        case CciEnum.netTurnOverIssueRate_reverse:
                            #region NetTurnOverIssueRate Reverse
                            if (m_Invoice.IssueRateIsReverseSpecified)
                                data = m_Invoice.IssueRateIsReverse.Value;
                            #endregion NetTurnOverIssueRate Reverse
                            break;
                        case CciEnum.invoiceRateSource_assetFxRate:
                            #region InvoiceRateSource (OTCmlId)
                            //bool isEnabled = false;
                            try
                            {
                                if (m_Invoice.InvoiceRateSourceSpecified)
                                {
                                    int idAsset = m_Invoice.InvoiceRateSource.RateSource.OTCmlId;
                                    if (idAsset > 0)
                                    {
                                        SQL_AssetFxRate sql_Asset = new SQL_AssetFxRate(CS, idAsset);
                                        //isEnabled = sql_Asset.IsLoaded;
                                        if (sql_Asset.IsLoaded)
                                        {
                                            cci.Sql_Table = sql_Asset;
                                            data = sql_Asset.Identifier;
                                            m_Invoice.InvoiceRateSource.RateSource.SetAssetFxRateId(idAsset, data);
                                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency1), sql_Asset.QCP_Cur1);
                                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency2), sql_Asset.QCP_Cur2);
                                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), sql_Asset.QCP_QuoteBasis);
                                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), sql_Asset.QCP_QuoteBasis);

                                        }
                                    }
                                }
                                //EnabledInvoiceRateSource(isEnabled);
                                //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            }
                            catch
                            {
                                cci.Sql_Table = null;
                                data = string.Empty;
                            }
                            #endregion InvoiceRateSource (OTCmlId)
                            break;
                        case CciEnum.invoiceRateSource_rateSource:
                            #region RateSource
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.RateSource.RateSource.Value;
                            #endregion RateSource
                            break;
                        case CciEnum.invoiceRateSource_rateSourcePage:
                            #region RateSourcePage
                            if (m_Invoice.InvoiceRateSourceSpecified &&
                                m_Invoice.InvoiceRateSource.RateSource.RateSourcePageSpecified)
                                data = m_Invoice.InvoiceRateSource.RateSource.RateSourcePage.Value;
                            #endregion RateSourcePage
                            break;
                        case CciEnum.invoiceRateSource_rateSourcePageHeading:
                            #region RateSourcePageHeading
                            if (m_Invoice.InvoiceRateSourceSpecified &&
                                m_Invoice.InvoiceRateSource.RateSource.RateSourcePageHeadingSpecified)
                                data = m_Invoice.InvoiceRateSource.RateSource.RateSourcePageHeading;
                            #endregion RateSourcePageHeading
                            break;
                        case CciEnum.invoiceRateSource_fixingTime_hourMinuteTime:
                            #region FixingTime (HourMinuteTime)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingTime.HourMinuteTime.Value;
                            #endregion FixingTime (HourMinuteTime)
                            break;
                        case CciEnum.invoiceRateSource_fixingTime_businessCenter:
                            #region FixingTime (BusinessCenter)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingTime.BusinessCenter.Value;
                            #endregion FixingTime (BusinessCenter)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate:
                            #region FixingDate (Date)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.AdjustedFixingDate.Value;
                            #endregion FixingDate (Date)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_dateRelativeTo:
                            #region FixingDate (DateRelativeTo)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingDate.DateRelativeToValue;
                            #endregion FixingDate (DateRelativeTo)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_period:
                            #region FixingDate (Period)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingDate.Period.ToString();
                            #endregion FixingDate (Period)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_periodMultiplier:
                            #region FixingDate (PeriodMultiplier)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingDate.PeriodMultiplier.Value;
                            #endregion FixingDate (PeriodMultiplier)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_businessDayConvention:
                            #region FixingDate BDC (BusinessDayConvention)
                            if (m_Invoice.InvoiceRateSourceSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingDate.BusinessDayConvention.ToString();
                            #endregion FixingDate (BusinessDayConvention)
                            break;
                        case CciEnum.invoiceRateSource_fixingDate_dayType:
                            #region FixingDate (DayType)
                            if (m_Invoice.InvoiceRateSourceSpecified && m_Invoice.InvoiceRateSource.FixingDate.DayTypeSpecified)
                                data = m_Invoice.InvoiceRateSource.FixingDate.DayType.ToString();
                            #endregion FixingDate (DayType)
                            break;
                        case CciEnum.rebateConditionsSpecified:
                            #region RebateConditionsSpecified
                            data = m_Invoice.RebateConditionsSpecified.ToString().ToLower();
                            #endregion RebateConditionsSpecified
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
            if (ExistInvoiceRebate)
                m_CciInvoiceRebate.Initialize_FromDocument();

            for (int i = 0; i < InvoiceTradeLenght; i++)
                m_CciInvoiceTrade[i].Initialize_FromDocument();

            base.Initialize_FromDocument();
        }
        #endregion
        #region ProcessInitialize
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
                    case CciEnum.receiver:
                        #region Payer / Receiver
                        base.InitializePartySide();
                        #endregion Payer / Receiver
                        break;
                    case CciEnum.rebateAmount_amount:
                    case CciEnum.grossTurnOverAmount_amount:
                    case CciEnum.totalRebateAmount_amount:
                        #region GrossTurnOverAmount / TotalRebateAmount
                        CalculNetAmount();
                        #endregion GrossTurnOverAmount / TotalRebateAmount
                        break;
                    case CciEnum.grossTurnOverAmount_currency:
                        #region GrossTurnOverAmount (Currency)
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverAmount_currency), m_Invoice.GrossTurnOverAmount.Currency);
                        Ccis.SetNewValue(CciClientId(CciEnum.totalRebateAmount_currency), m_Invoice.GrossTurnOverAmount.Currency);
                        #endregion GrossTurnOverAmount (Currency)
                        break;
                    case CciEnum.netTurnOverAmount_amount:
                    case CciEnum.netTurnOverIssueAmount_currency:
                        SetNetTurnOverIssueCurrency();
                        CalculNetIssueAmount();
                        break;
                    case CciEnum.invoiceRateSource_assetFxRate:
                        #region InvoiceRateSource (OTCmlId)
                        if (pCci.IsEmpty)
                        {
                            ClearInvoiceRateSource();
                            //EnabledInvoiceRateSource(false);
                        }
                        else if (null != pCci.Sql_Table)
                        {
                            SQL_AssetFxRate sql_Asset = (SQL_AssetFxRate)pCci.Sql_Table;
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSource), sql_Asset.PrimaryRateSrc);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePage), sql_Asset.PrimaryRateSrcPage);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePageHeading), sql_Asset.PrimaryRateSrcHead);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_hourMinuteTime), DtFunc.DateTimeToString(sql_Asset.TimeRateSrc, DtFunc.FmtISOTime));
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_businessCenter), sql_Asset.IdBC_RateSrc);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency1), sql_Asset.QCP_Cur1);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency2), sql_Asset.QCP_Cur2);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), sql_Asset.QCP_QuoteBasis);
                            SetNetTurnOverIssueCurrency();
                            //EnabledInvoiceRateSource(true);
                        }
                        else
                        {
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSource), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePage), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePageHeading), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_hourMinuteTime), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_businessCenter), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency1), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency2), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.quotedCurrencyPair_quoteBasis), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_amount), string.Empty);
                            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_currency), string.Empty);
                            //EnabledInvoiceRateSource(false);
                        }
                        #endregion InvoiceRateSource (OTCmlId)
                        break;
                    case CciEnum.invoiceRateSource_fixingDate:
                    case CciEnum.invoiceRateSource_fixingDate_period:
                    case CciEnum.invoiceRateSource_fixingDate_dayType:
                        CalculFixingDate();
                        break;
                    case CciEnum.invoiceRateSource_fixingDate_dateRelativeTo:
                        if (m_Invoice.InvoiceRateSourceSpecified && StrFunc.IsEmpty(m_Invoice.InvoiceRateSource.FixingDate.DateRelativeToValue))
                        {
                            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_currency), m_Invoice.NetTurnOverIssueAmount.Currency);
                        }
                        CalculFixingDate();
                        break;
                    case CciEnum.invoiceRateSource_fixingDate_periodMultiplier:
                        #region FixingDate (PeriodMultiplier)
                        Ccis.ProcessInitialize_DayType(CciClientId(CciEnum.invoiceRateSource_fixingDate_dayType), m_Invoice.InvoiceRateSource.FixingDate.GetOffset);
                        CalculFixingDate();
                        #endregion FixingDate (PeriodMultiplier)
                        break;
                    case CciEnum.rebateConditionsSpecified:
                        #region RebateConditionsSpecified
                        if (Cci(CciEnum.rebateConditionsSpecified.ToString()).IsFilledValue)
                            Ccis.SetNewValue(CciClientId(CciEnum.rebateConditionsSpecified), "false");
                        else
                            Ccis.SetNewValue(CciClientId(CciEnum.rebateConditionsSpecified), "true");
                        #endregion RebateConditionsSpecified
                        break;
                }
            }

            if (ExistInvoiceRebate)
                m_CciInvoiceRebate.ProcessInitialize(pCci);

            for (int i = 0; i < InvoiceTradeLenght; i++)
                m_CciInvoiceTrade[i].ProcessInitialize(pCci);

            base.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            EnabledInvoiceRateSource(m_Invoice.InvoiceRateSourceSpecified);
            if (ExistInvoiceRebate)
                m_CciInvoiceRebate.RefreshCciEnabled();
            base.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
            if (IsCci(CciEnum.invoiceRateSource_fixingDate_businessDayConvention, pCci))
            {
                if (m_Invoice.InvoiceRateSourceSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, m_Invoice.InvoiceRateSource.FixingDate.GetAdjustments);
            }
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
            for (int i = 0; i < InvoiceTradeLenght; i++)
            {
                isButtonMenu = m_CciInvoiceTrade[i].IsButtonMenu(pCci, ref pCo);
                if (isButtonMenu)
                    break;
            }
            return isButtonMenu;
        }
        #endregion isButtonMenu
        #region SetButtonReferential
        // EG 20120620 Gestion écran Light ou Enhanced (avec détails factures : frais + Remises)
        // EG 20141020 [20442] L'entité est Bé&néficiare ou payeur
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            if (IsCci(CciEnum.feesDetail, pCci))
            {
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Invoicing = "INVOICEFEESDETAIL";
                pCo.Fk = TradeCommonInput.IdT.ToString();
                /// EG 20141020 [20442]
                int idA_Entity = BookTools.GetEntityBook(CSCacheOn, cciParty[0].GetBookIdB());
                if (0 == idA_Entity)
                    idA_Entity = BookTools.GetEntityBook(CSCacheOn, cciParty[1].GetBookIdB());
                StringDynamicData idA = new StringDynamicData(TypeData.TypeDataEnum.integer.ToString(), "ENTITY", idA_Entity.ToString());
                StringDynamicData dt = new StringDynamicData(TypeData.TypeDataEnum.date.ToString(), "DATE1", m_Invoice.InvoiceDate.Value)
                {
                    dataformat = DtFunc.FmtISODate
                };
                pCo.DynamicArgument = new string[2] { idA.Serialize(), dt.Serialize() };
            }
            else
                base.SetButtonReferential(pCci, pCo);
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsSpecified, ref bool pIsEnabled)
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
        //
        #region Methods
        #region CalculNetAmount
        public void CalculNetAmount()
        {
            decimal totalRebate = 0;
            EFS_Decimal netTurnOverAmount = new EFS_Decimal(m_Invoice.GrossTurnOverAmount.Amount.DecValue);
            if (m_Invoice.RebateConditionsSpecified && m_Invoice.RebateAmountSpecified)
                totalRebate = m_Invoice.RebateAmount.Amount.DecValue;
            netTurnOverAmount.DecValue -= totalRebate;
            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverAmount_amount), netTurnOverAmount.Value);
        }
        #endregion CalculNetAmount
        #region CalculFixingDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CalculFixingDate()
        {
            if (m_Invoice.InvoiceRateSourceSpecified && (false == IsModeConsult))
            {
                m_Invoice.InvoiceRateSource.FixingDate.BusinessDayConvention = BusinessDayConventionEnum.NONE;
                Cst.ErrLevel ret = Tools.OffSetDateRelativeTo(CS, m_Invoice.InvoiceRateSource.FixingDate, out DateTime offsetDate, TradeCommonInput.DataDocument);
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    IAdjustedDate adjustedDate = ((IProduct)m_Invoice).ProductBase.CreateAdjustedDate(offsetDate);
                    Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate), adjustedDate.Value);
                }
            }
        }
        #endregion CalculFixingDate
        #region CalculNetIssueAmount
        // EG 20150706 [21021] Nullable<int> (idA_Pay|idB_Pay|idA_Rec|idB_Rec)
        public void CalculNetIssueAmount()
        {
            if (m_Invoice.InvoiceRateSourceSpecified)
            {
                if (false == IsModeConsult)
                {
                    //string netIssueCurrrency = string.Empty;
                    DateTime fixingDate = m_Invoice.InvoiceRateSource.AdjustedFixingDate.DateValue;
                    Nullable<int> idA_Pay = TradeCommonInput.DataDocument.GetOTCmlId_Party(m_Invoice.PayerPartyReference.HRef);
                    Nullable<int> idB_Pay = TradeCommonInput.DataDocument.GetOTCmlId_Book(m_Invoice.PayerPartyReference.HRef);
                    Nullable<int> idA_Rec = TradeCommonInput.DataDocument.GetOTCmlId_Party(m_Invoice.ReceiverPartyReference.HRef);
                    Nullable<int> idB_Rec = TradeCommonInput.DataDocument.GetOTCmlId_Book(m_Invoice.ReceiverPartyReference.HRef);
                    // EG 20150706 [21021]
                    KeyQuote keyQuote = new KeyQuote(CS, fixingDate, idA_Pay, idB_Pay, idA_Rec, idB_Rec, QuoteTimingEnum.Close);

                    KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
                    {
                        IdC1 = Cci(CciEnum.quotedCurrencyPair_currency1).NewValue,
                        IdC2 = Cci(CciEnum.quotedCurrencyPair_currency2).NewValue,
                        QuoteBasisSpecified = true,
                        QuoteBasis = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), Cci(CciEnum.quotedCurrencyPair_quoteBasis).NewValue, true)
                    };

                    SQL_Quote quote = new SQL_Quote(CS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, (IProductBase)m_Invoice, keyQuote, keyAssetFXRate);
                    if (quote.IsLoaded)
                    {
                        EFS_Cash cash = new EFS_Cash(CS, keyAssetFXRate.IdC1, keyAssetFXRate.IdC2, m_Invoice.NetTurnOverAmount.Amount.DecValue,
                            quote.QuoteValue, ((KeyAssetFxRate)quote.KeyAssetIN).QuoteBasis);
                        EFS_Decimal quoteValue = new EFS_Decimal(quote.QuoteValue);
                        EFS_Decimal netTurnOverIssueAmount = new EFS_Decimal(cash.ExchangeAmountRounded);
                        string quoteCurrencyPair;
                        if (QuoteBasisEnum.Currency1PerCurrency2 == keyAssetFXRate.QuoteBasis)
                            quoteCurrencyPair = keyAssetFXRate.IdC2 + "./" + keyAssetFXRate.IdC1;
                        else
                            quoteCurrencyPair = keyAssetFXRate.IdC1 + "./" + keyAssetFXRate.IdC2;
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_amount), netTurnOverIssueAmount.Value);
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_rate), quoteValue.Value);
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_quoteCurrencyPair), quoteCurrencyPair);
                    }
                    else
                    {
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_amount), string.Empty);
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_rate), string.Empty);
                        Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_quoteCurrencyPair), string.Empty);
                    }
                }
            }
            else
            {
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_amount), m_Invoice.NetTurnOverAmount.Amount.Value);
                Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_currency), m_Invoice.NetTurnOverAmount.Currency);
            }
        }
        #endregion CalculNetIssueAmount
        #region CalculRebateAmounts
        public void CalculRebateAmounts()
        {
            if (m_Invoice.RebateConditionsSpecified && ExistInvoiceRebate)
                m_CciInvoiceRebate.CalculRebateAmounts();
        }
        #endregion CalculRebateAmounts
        #region ClearInvoiceRateSource
        private void ClearInvoiceRateSource()
        {
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSource), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePage), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_rateSourcePageHeading), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_hourMinuteTime), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingTime_businessCenter), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate_dateRelativeTo), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate_period), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate_periodMultiplier), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.invoiceRateSource_fixingDate_dayType), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_rate), string.Empty);
            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueRate_quoteCurrencyPair), string.Empty);
        }
        #endregion ClearInvoiceRateSource
        #region DumpInvoicingScope_ToDocument
        /// <summary>
        /// Dump a InvoicingScope into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        // EG 20150706 [21021] Nullable<int> idA_Invoiced
        public override void DumpInvoicingScope_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            SQL_InvoicingRules sql_InvoicingRules = new SQL_InvoicingRules(CS, pData);
            if (sql_InvoicingRules.IsLoaded)
            {
                //int idA_Rec = this.DataDocument.GetOTCmlId_Party(Ccis.GetNewValue(CciClientIdReceiver), PartyInfoEnum.id);
                Nullable<int> idA_Invoiced = this.DataDocument.GetOTCmlId_Party(Ccis.GetNewValue(CciClientIdPayer), PartyInfoEnum.id);
                if ((sql_InvoicingRules.IdAInvoiced == idA_Invoiced) && idA_Invoiced.HasValue)
                {
                    pCci.Sql_Table = sql_InvoicingRules;
                    SetDisplay(pCci);
                }
            }

            if (StrFunc.IsFilled(pData))
            {
                //Check if InvoicingRules is valid for actors (ENTITY and PAYER)
                //A Terminer
            }
        }
        #endregion DumpInvoicingScope_ToDocument
        #region EnabledInvoiceRateSource
        private void EnabledInvoiceRateSource(bool pIsEnabled)
        {
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_rateSource), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_rateSourcePage), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_rateSourcePageHeading), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingDate), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingTime_hourMinuteTime), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingTime_businessCenter), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingDate_dateRelativeTo), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingDate_period), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingDate_periodMultiplier), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.invoiceRateSource_fixingDate_dayType), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.netTurnOverIssueRate_rate), "IsEnabled", pIsEnabled);
            Ccis.Set(CciClientId(CciEnum.netTurnOverAccountingRate_rate), "IsEnabled", m_Invoice.NetTurnOverAccountingRateSpecified);
        }
        #endregion EnabledInvoiceRateSource
        #region GetArrayElementDocumentCount
        public override int GetArrayElementDocumentCount(string pPrefix, string pParentClientId, int pParentOccurs)
        {
            int ret = base.GetArrayElementDocumentCount(pPrefix, pParentClientId, pParentOccurs);
            if (-1 == ret)
            {
                if (TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceTrade == pPrefix)
                {
                    if (null != m_Invoice.InvoiceDetails)
                        ret = ArrFunc.Count(m_Invoice.InvoiceDetails.InvoiceTrade);
                }
            }
            if (-1 == ret)
            {
                if (TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceFee == pPrefix)
                {
                    if ((null != m_Invoice.InvoiceDetails) && (null != m_Invoice.InvoiceDetails.InvoiceTrade))
                        ret = ArrFunc.Count(m_Invoice.InvoiceDetails.InvoiceTrade[pParentOccurs - 1].InvoiceFees.InvoiceFee);
                }
            }
            if (-1 == ret)
            {
                if (TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateBracketCalculation == pPrefix)
                {
                    if (m_Invoice.RebateConditionsSpecified &&
                        m_Invoice.RebateConditions.BracketConditionsSpecified)
                    {
                        IRebateBracketCalculation[] rebateBracketCalculations = m_Invoice.RebateConditions.BracketConditions.Result.Calculations.RebateBracketCalculation;
                        ret = ArrFunc.Count(rebateBracketCalculations);
                    }
                }
            }
            return ret;
        }
        #endregion GetArrayElementDocumentCount
        #region GetInvoiceTrade
        public IInvoiceTrade[] GetInvoiceTrade()
        {
            return InvoiceDetails.InvoiceTrade;
        }
        #endregion GetInvoiceTrade


        #region InitializeInvoiceRebateConditions_FromCci
        private void InitializeInvoiceRebateConditions_FromCci()
        {
            m_CciInvoiceRebate = new CciInvoiceRebate(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceRebateConditions, null)
            {
                RebateConditions = m_Invoice.RebateConditions
            };
            m_CciInvoiceRebate.Initialize_FromCci();
        }
        #endregion InitializeInvoiceRebateConditions_FromCci
        #region InitializeInvoiceTrade_FromCci
        private void InitializeInvoiceTrade_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciInvoiceTrade cciInvoiceTrade = new CciInvoiceTrade(this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceTrade, index + 1, null);
                //
                isOk = Ccis.Contains(cciInvoiceTrade.CciClientId(CciInvoiceTrade.CciEnum.trade_identifier));
                // EG 20110308 HPC Nb ligne de frais sur facture
                // EG 20120615
                isOk = isOk || ((null != GetInvoiceTrade()) && (index < TradeAdminInput.NbDisplayInvoiceTrade) && 
                    (TradeAdminInput.NbDisplayInvoiceTrade <= GetInvoiceTrade().Length) && (null != GetInvoiceTrade()[index]));
                if (isOk)
                {
                    // EG 20110308 HPC Nb ligne de frais sur facture
                    if (ArrFunc.IsEmpty(GetInvoiceTrade()) || (index == TradeAdminInput.NbDisplayInvoiceTrade))
                    {
                        ReflectionTools.AddItemInArray(InvoiceDetails, "invoiceTrade", index);
                        if (ArrFunc.IsFilled(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                        {
                            // TODO
                        }
                    }
                    //
                    if (StrFunc.IsEmpty(GetInvoiceTrade()[index].Id))
                        GetInvoiceTrade()[index].Id = m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceTrade + Convert.ToString(index + 1);
                    //					
                    cciInvoiceTrade.InvoiceTrade = InvoiceDetails.InvoiceTrade[index];
                    lst.Add(cciInvoiceTrade);
                }
            }
            m_CciInvoiceTrade = (CciInvoiceTrade[])lst.ToArray(typeof(CciInvoiceTrade));
            //
            for (int i = 0; i < InvoiceTradeLenght; i++)
                m_CciInvoiceTrade[i].Initialize_FromCci();
        }
        #endregion InitializeInvoiceTrade_FromCci
        #region SetNetTurnOverIssueCurrency
        private void SetNetTurnOverIssueCurrency()
        {
            string netIssueCurrrency = string.Empty;
            string netCurrency = Ccis.GetNewValue(CciClientId(CciEnum.netTurnOverAmount_currency));
            string currency1 = Ccis.GetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency1));
            string currency2 = Ccis.GetNewValue(CciClientId(CciEnum.quotedCurrencyPair_currency2));
            if (currency1 == netCurrency)
                netIssueCurrrency = currency2;
            else if (currency2 == netCurrency)
                netIssueCurrrency = currency1;
            Ccis.SetNewValue(CciClientId(CciEnum.netTurnOverIssueAmount_currency), netIssueCurrrency);
        }
        #endregion SetNetTurnOverIssueCurrency
        #endregion Methods
    }
}
