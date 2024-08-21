#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    ///  Gestion d'1 <see cref="IFxOptionPremium"/> ou <see cref="IPayment"/>
    /// </summary>
    // EG 20171109 [23509] Upd Set CciTradeBase instead of CciTrade
    public class CciPayment : IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver, IContainerCciSpecified, IContainerCciQuoteBasis, ICciPresentation
    {
        #region Enums
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            payer,
            receiver,
            amount,
            currency,
            date,
            settlementInformation,
            unknown,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnumFxOptionPremium
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("premiumAmount.amount")]
            premiumAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("premiumAmount.currency")]
            premiumAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("premiumSettlementDate")]
            premiumSettlementDate,
            [System.Xml.Serialization.XmlEnumAttribute("settlementInformation")]
            settlementInformation,
            [System.Xml.Serialization.XmlEnumAttribute("premiumQuote.premiumValue")]
            premiumQuote_premiumValue,
            [System.Xml.Serialization.XmlEnumAttribute("premiumQuote.premiumQuoteBasis")]
            premiumQuote_premiumQuoteBasis,
            unknown,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnumPayment
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.amount")]
            paymentAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.currency")]
            paymentAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.unadjustedDate")]
            paymentDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.dateAdjustments.businessDayConvention")]
            paymentDate_dateAdjustments_bDC,
            [System.Xml.Serialization.XmlEnumAttribute("paymentType")]
            paymentType,
            [System.Xml.Serialization.XmlEnumAttribute("settlementInformation")]
            settlementInformation,
            //
            [System.Xml.Serialization.XmlEnumAttribute("paymentQuote.percentageRate")]
            paymentQuote_percentageRateOrpercentageRateFraction,
            [System.Xml.Serialization.XmlEnumAttribute("paymentQuote.percentageRateFraction")]
            paymentQuote_percentageRateFraction,
            [System.Xml.Serialization.XmlEnumAttribute("paymentQuote.percentageRate")]
            paymentQuote_percentageRate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentQuote.paymentRelativeTo")]
            paymentQuote_paymentRelativeTo,
            /// <summary>
            /// 
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute("paymentSource")]
            paymentSource_feeSchedule,
            /// <summary>
            /// 
            /// </summary>
            /// FI 20180502 [23926] Add
            [System.Xml.Serialization.XmlEnumAttribute("paymentSource")]
            paymentSource_feeMatrix,
            [System.Xml.Serialization.XmlEnumAttribute("paymentSource")]
            paymentSource_feeInvoicing,
            [System.Xml.Serialization.XmlEnumAttribute("tax")]
            tax,

            unknown,
        }

        /// <summary>
        /// 
        /// </summary>
        public enum CurrencyEnum
        {
            paymentCurrency,
            customerCurrency,
        }

        /// <summary>
        /// Type (efsML ou FpML) géré par CciPayment
        /// </summary>
        public enum PaymentTypeEnum
        {
            /// <summary>
            /// 
            /// </summary>
            Payment,
            /// <summary>
            /// 
            /// </summary>
            FxOptionPremium,
        }
        #endregion Enums

        #region Members
        private readonly CciTradeBase cciTrade;
        private IPayment payment;
        private IFxOptionPremium fxOptionPremium;
        private readonly int number;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly string prefix;
        private readonly string _defaultPaymentType;
        private string _clientIdDefaultReceiver;
        private readonly string _clientIdDefaultBDC;
        private readonly string _clientIdDefaultCurrency;
        private string _clientIdDefaultDate;
        private CciCustomerSettlementPayment cciCustomerSettlementPayment;
        private readonly PaymentTypeEnum paymentTypeEnum;

        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ClientIdDefaultReceiver
        {
            set { _clientIdDefaultReceiver = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20180301 [23814] Add
        public string ClientIdDefaultDate
        {
            set { _clientIdDefaultDate = value; }
            get { return _clientIdDefaultDate; }
        }


        /// <summary>
        /// 
        /// </summary>
        private ICustomerSettlementPayment CustomerSettlementPayment
        {
            get
            {
                ICustomerSettlementPayment ret;
                //
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        ret = payment.CustomerSettlementPayment;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        ret = fxOptionPremium.CustomerSettlementPremium;
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Payment Type {0} is not implemented", paymentTypeEnum.ToString()));
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool ExistNumber { get { return (0 < number); } }

        /// <summary>
        /// 
        /// </summary>
        public bool ExistPaymentQuote
        {
            get
            {
                bool ret = false;
                if (PaymentTypeEnum.Payment == paymentTypeEnum)
                {
                    ret = Ccis.Contains(CciClientId(CciPayment.CciEnumPayment.paymentQuote_percentageRate));
                    ret = ret || Ccis.Contains(CciClientId(CciPayment.CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction));
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient l'élément Money présent dans le payment 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public IMoney Money
        {
            get
            {
                IMoney ret;
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        ret = payment.PaymentAmount;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        ret = fxOptionPremium.PremiumAmount;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = number.ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public Object Payment
        {
            set
            {
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        payment = (IPayment)value;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        fxOptionPremium = (IFxOptionPremium)value;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
            }
            get
            {
                object ret;
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        ret = payment;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        ret = fxOptionPremium;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private bool IsCaptureCustomerFilled
        {
            get
            {
                bool ret = Cci(CciEnum.amount).IsFilledValue;
                ret = ret && cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount).IsFilledValue;
                ret &= cciCustomerSettlementPayment.CciExchangeRate.IsExchangeRateFilled;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public bool IsSettlementInfoSpecified
        {
            get
            {
                bool bRet;
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        bRet = payment.SettlementInformationSpecified;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        bRet = fxOptionPremium.SettlementInformationSpecified;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
                return bRet;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        public bool IsSettlementInstructionSpecified
        {
            get
            {
                bool bRet;
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        bRet = IsSettlementInfoSpecified && SetttlementInformation.InstructionSpecified;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        bRet = IsSettlementInfoSpecified && SetttlementInformation.InstructionSpecified;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
                return bRet;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA1065]
        private ISettlementInformation SetttlementInformation
        {
            get
            {
                ISettlementInformation ret;
                //
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        ret = payment.SettlementInformation;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        ret = fxOptionPremium.SettlementInformation;
                        break;
                    default:
                        throw new InvalidOperationException("Payment Type unknown");
                }
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public CustomCaptureInfosBase.ProcessQueueEnum ProcessQueueCciDate
        {
            get;
            set;
        }



        /// <summary>
        /// Si renseigné application d'un offsetting lorsque de l'initialisation de la date de payment
        /// <para>Ne s'applique pas lorsque le produit est ETD (Pour l'instant codage en dur sur ETD)</para>
        /// </summary>
        /// FI 20150316 [XXXXX] Add 
        public Pair<IOffset, IBusinessCenters> DefaultDateSettings
        {
            get;
            set;
        }

        /// <summary>
        /// Gets true if the payment is a Fee
        /// </summary>
        /// FI 20240531 [WI900] Add
        public Boolean IsOPP
        {
            get { return prefix.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment); }
        }

        /// <summary>
        /// Gets or sets if the fee is invoiced by default
        /// </summary>
        /// FI 20240531 [WI900] Add
        public Boolean DefaultISINVOICING
        {
            get;
            set; 
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPaymentNumber"></param>
        /// <param name="pPayment"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pPrefixPayment"></param>
        // EG 20171109 [23509] Upd Set CciTradeBase instead of CciTrade
        public CciPayment(CciTradeBase pTrade, int pPaymentNumber, IPayment pPayment, PaymentTypeEnum pPaymentType, string pPrefixPayment)
            :
        this(pTrade, pPaymentNumber, pPayment, pPaymentType, pPrefixPayment, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPaymentNumber"></param>
        /// <param name="pPayment"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pPrefixPayment"></param>
        /// <param name="pDefaultPaymentType"></param>
        // EG 20171109 [23509] Upd Set CciTradeBase instead of CciTrade
        public CciPayment(CciTradeBase pTrade, int pPaymentNumber, IPayment pPayment, PaymentTypeEnum pPaymentType, string pPrefixPayment,
            string pDefaultPaymentType)
            :
            this(pTrade, pPaymentNumber, pPayment, pPaymentType, pPrefixPayment, pDefaultPaymentType, string.Empty, string.Empty, string.Empty, string.Empty)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pPaymentNumber"></param>
        /// <param name="pPayment"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pPrefixPayment"></param>
        public CciPayment(CciTradeBase pTrade, int pPaymentNumber, IFxOptionPremium pPayment, PaymentTypeEnum pPaymentType,
            string pPrefixPayment)
            :
            this(pTrade, pPaymentNumber, pPayment, pPaymentType, pPrefixPayment, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pPaymentNumber"></param>
        /// <param name="pPayment"></param>
        /// <param name="pPaymentTypeEnum"></param>
        /// <param name="pPrefixPayment"></param>
        /// <param name="pDefaultPaymentType">PaymentType pour initialisation du cci PaymentType</param>
        /// <param name="pClientIdDefaultReceiver"></param>
        /// <param name="pClientIdDefaultBDC">Cci utilisé pour l'initialisation du cci BusinessDayConvention du paiement (si non renseigné: None sera appliqué)</param>
        /// <param name="pClientIdDefaultCurrency">Cci utilisé pour l'initialisation du cci devise du paiement (si non renseigné: Devise par défaut sera aplliqué (fichier Config)</param>
        /// <param name="pClientIdDefaultDate"></param>
        // EG 20171109 [23509] Upd Set CciTradeBase instead of CciTrade
        public CciPayment(CciTradeBase pCciTrade, int pPaymentNumber, object pPayment, PaymentTypeEnum pPaymentTypeEnum, string pPrefixPayment,
            string pDefaultPaymentType, string pClientIdDefaultReceiver, string pClientIdDefaultBDC, string pClientIdDefaultCurrency, string pClientIdDefaultDate)
        {
            cciTrade = pCciTrade;
            _ccis = pCciTrade.Ccis;
            number = pPaymentNumber;  // Use property Number
            prefix = pPrefixPayment + NumberPrefix + CustomObject.KEY_SEPARATOR;
            paymentTypeEnum = pPaymentTypeEnum;

            switch (paymentTypeEnum)
            {
                case PaymentTypeEnum.Payment:
                    payment = (IPayment)pPayment;
                    break;
                case PaymentTypeEnum.FxOptionPremium:
                    fxOptionPremium = (IFxOptionPremium)pPayment;
                    break;
                default:
                    throw new Exception("Payment Type unknown");
            }

            // FI 20240531 [WI900] DefaultISINVOICING from Spheres_TradeInputDefault_ISINVOICING setting
            if (IsOPP)
                DefaultISINVOICING = BoolFunc.IsTrue(CciTrade.GetDefaultISINVOICING());

            _defaultPaymentType = pDefaultPaymentType;
            _clientIdDefaultReceiver = pClientIdDefaultReceiver;
            _clientIdDefaultBDC = pClientIdDefaultBDC;
            _clientIdDefaultCurrency = pClientIdDefaultCurrency;
            _clientIdDefaultDate = pClientIdDefaultDate;

            ProcessQueueCciDate = CustomCaptureInfosBase.ProcessQueueEnum.None;
        }
        #endregion Constructors


        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            ArrayList PayersReceivers = new ArrayList
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };

            IEnumerator ListEnum = PayersReceivers.GetEnumerator();
            while (ListEnum.MoveNext())
            {
                string clientId_WithoutPrefix = ListEnum.Current.ToString();
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            Ccis[CciClientIdReceiver].IsMandatory = Ccis[CciClientIdPayer].IsMandatory;

            AddCciSystemPaymentDate();

            if (ExistPaymentQuote)
            {
                string clientId_WithoutPrefix = CciClientId(CciEnumPayment.paymentQuote_paymentRelativeTo);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }


            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.AddCciSystem();

            //Don't erase
            CreateInstance();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {

            switch (paymentTypeEnum)
            {
                case PaymentTypeEnum.Payment:
                    payment.PaymentQuoteSpecified = false;
                    if (null != payment.PaymentQuote)
                        payment.PaymentQuoteSpecified = CaptureTools.IsDocumentElementValid(payment.PaymentQuote.PercentageRate);
                    break;
                case PaymentTypeEnum.FxOptionPremium:
                    fxOptionPremium.PremiumQuoteSpecified = false;
                    if (null != fxOptionPremium.PremiumQuote)
                        fxOptionPremium.PremiumQuoteSpecified = CaptureTools.IsDocumentElementValid(fxOptionPremium.PremiumQuote.PremiumValue);
                    break;
                default:
                    throw new Exception("Unknown Payment Type");
            }
            //			
            #region CustomerSettlementPayment
            if (null != cciCustomerSettlementPayment)
            {
                cciCustomerSettlementPayment.CleanUp();
                //				
                switch (paymentTypeEnum)
                {
                    case PaymentTypeEnum.Payment:
                        payment.CustomerSettlementPaymentSpecified = cciCustomerSettlementPayment.IsSpecified;
                        break;
                    case PaymentTypeEnum.FxOptionPremium:
                        fxOptionPremium.CustomerSettlementPremiumSpecified = cciCustomerSettlementPayment.IsSpecified;
                        break;
                    default:
                        throw new Exception("Unknown Payment Type");
                }
            }
            #endregion CustomerSettlementPayment
        }


        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            //
            switch (paymentTypeEnum)
            {
                case PaymentTypeEnum.Payment:
                    Dump_ToDocumentPayment();
                    break;
                case PaymentTypeEnum.FxOptionPremium:
                    Dump_ToDocumentFxOptionPremium();
                    break;
                default:
                    throw new Exception("Payment Type unknown");
            }
            //
            if (null != cciCustomerSettlementPayment)
            {
                if (PaymentTypeEnum.Payment == paymentTypeEnum)
                {
                    payment.CustomerSettlementPaymentSpecified = cciCustomerSettlementPayment.IsSpecified;
                    if (payment.CustomerSettlementPaymentSpecified)
                    {
                        if ((cciCustomerSettlementPayment.CciExchangeRate.IsSpotFixing) && (false == cciCustomerSettlementPayment.CciExchangeRate.CciFixing.ExistCciAssetFxRate))
                        {
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyA), payment.PaymentAmount.Currency);
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyB), payment.CustomerSettlementPayment.Currency);
                        }
                        else
                        {
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency1), fxOptionPremium.PremiumAmount.Currency);
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency2), fxOptionPremium.CustomerSettlementPremium.Currency);
                        }
                    }
                }
                else if (PaymentTypeEnum.FxOptionPremium == paymentTypeEnum)
                {
                    fxOptionPremium.CustomerSettlementPremiumSpecified = cciCustomerSettlementPayment.IsSpecified;
                    if (fxOptionPremium.CustomerSettlementPremiumSpecified)
                    {
                        if ((cciCustomerSettlementPayment.CciExchangeRate.IsSpotFixing) && (false == cciCustomerSettlementPayment.CciExchangeRate.CciFixing.ExistCciAssetFxRate))
                        {
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyA), fxOptionPremium.PremiumAmount.Currency);
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_currencyB), fxOptionPremium.CustomerSettlementPremium.Currency);
                        }
                        else
                        {
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency1), fxOptionPremium.PremiumAmount.Currency);
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciClientId(CciExchangeRate.CciEnum.quotedCurrencyPair_currency2), fxOptionPremium.CustomerSettlementPremium.Currency);
                        }
                    }
                }
                //
                cciCustomerSettlementPayment.Dump_ToDocument();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CreateInstance();
            //
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
            {
                cciCustomerSettlementPayment = new CciCustomerSettlementPayment(cciTrade, null, prefix + "customerSettlementPayment");
                bool isOk = Ccis.Contains(cciCustomerSettlementPayment.CciClientId(CciCustomerSettlementPayment.CciEnum.currency));
                if (isOk)
                {
                    if (false == payment.CustomerSettlementPaymentSpecified)
                        payment.CustomerSettlementPayment = payment.CreateCustomerSettlementPayment;
                    cciCustomerSettlementPayment.CustomerSettlementPayment = payment.CustomerSettlementPayment;
                }
                else
                    cciCustomerSettlementPayment = null;
                //
                //if (null != Cci(CciEnumPayment.paymentSource_feeInvoicing))
                //{
                //    ISpheresIdSchemeId[] spheresId = cciTrade.DataDocument.product.productBase.CreateSpheresId(1);
                //    spheresId[0].scheme = Cst.OTCml_RepositoryFeeInvoicingScheme;
                //    payment.paymentSource.spheresId = (ISpheresIdSchemeId[])Tools.AddSchemeCode((IScheme[])payment.paymentSource.spheresId, spheresId[0]);
                //}
                // EG 20100503 Ticket:16978
                // RD 20110929 Paratage du code via Tools.InitializePaymentSource()
                if (null != Cci(CciEnumPayment.paymentType))
                    Tools.InitializePaymentSource(payment, cciTrade.DataDocument.CurrentProduct.ProductBase);
            }
            else if (PaymentTypeEnum.FxOptionPremium == paymentTypeEnum)
            {
                cciCustomerSettlementPayment = new CciCustomerSettlementPayment(cciTrade, null, prefix + "customerSettlementPremium");
                bool isOk = Ccis.Contains(cciCustomerSettlementPayment.CciClientId(CciCustomerSettlementPayment.CciEnum.currency));
                if (isOk)
                {
                    if (false == fxOptionPremium.CustomerSettlementPremiumSpecified)
                        fxOptionPremium.CustomerSettlementPremium = fxOptionPremium.CreateCustomerSettlementPayment;
                    cciCustomerSettlementPayment.CustomerSettlementPayment = fxOptionPremium.CustomerSettlementPremium;
                }
                else
                    cciCustomerSettlementPayment = null;
            }
            //
            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.Initialize_FromCci();

        }


        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// FI 20121114 [18224] Refactoding Spheres® balaie les enums plutôt que la collections des ccis
        /// L'ancien code est présent dans la region OldCode
        public void Initialize_FromDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    string data = string.Empty;
                    SQL_Table sql_Table = null;
                    bool isSetting = true;

                    switch (cciEnum)
                    {
                        #region Payer/receiver
                        case CciEnum.payer:
                        case CciEnum.receiver:
                            string field = cciEnum.ToString() + "PartyReference";
                            FieldInfo fld = this.Payment.GetType().GetField(field);
                            //
                            if (null != fld)
                                data = ((IReference)fld.GetValue(Payment)).HRef;
                            break;
                        #endregion Payer/receiver

                        #region payment SettlementType
                        case CciEnum.settlementInformation:
                            // 20090306 RD 16536
                            if (null == SetttlementInformation || (false == IsSettlementInfoSpecified))
                                data = Cst.SettlementTypeEnum.None.ToString();
                            else if (SetttlementInformation.StandardSpecified)
                                // 20090305 RD 16536
                                data = SetttlementInformation.Standard.ToString();
                            else if (SetttlementInformation.InstructionSpecified)
                                data = Cst.SettlementInformationType.Instruction.ToString();
                            else
                                data = Cst.SettlementTypeEnum.None.ToString();
                            break;
                        #endregion SettlementType du trade
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }

                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);

                }
            }

            switch (paymentTypeEnum)
            {
                case PaymentTypeEnum.Payment:
                    Initialize_FromDocumentPayment();
                    break;
                case PaymentTypeEnum.FxOptionPremium:
                    Initialize_FromDocumentFxOptionPremium();
                    break;
                default:
                    throw new Exception("Payment Type unknown");
            }


            //Look
            if (false == Cci(CciEnum.payer).IsMandatory)
                SetEnabled(Cci(CciEnum.payer).IsFilledValue);
            //
            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.Initialize_FromDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

            switch (paymentTypeEnum)
            {
                case PaymentTypeEnum.Payment:
                    ProcessInitializePayment(pCci);
                    break;
                case PaymentTypeEnum.FxOptionPremium:
                    ProcessInitializeFxOptionPremium(pCci);
                    break;
                default:
                    throw new Exception("Payment Type unknown");
            }
            //
            if (null != cciCustomerSettlementPayment)
            {
                cciCustomerSettlementPayment.ProcessInitialize(pCci);
                //
                #region customerSettlementPayment.rate.rate.quotedCurrencyPair.quoteBasis
                if (cciCustomerSettlementPayment.CciExchangeRate.IsCci(CciExchangeRate.CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                    CalculRateFromAmount();
                #endregion quotedCurrencyPair_quoteBasis
                //
                #region customerSettlementPayment.rate.rate
                if (cciCustomerSettlementPayment.CciExchangeRate.IsCci(CciExchangeRate.CciEnum.rate, pCci) && pCci.IsFilledValue)
                {
                    CciCompare[] ccic = null;
                    if (!IsCaptureCustomerFilled)
                    {
                        // Mise en comment pour revenir en arrière potentiellement
                        //	ccic = new CciCompare[2] { 
                        //							 new CciCompare("AmountCur2", cciPaymentCur2.Cci(CciPayment.CciEnumPayment.paymentAmount_amount),1), 
                        //							 new CciCompare("Rate" ,Cci(CciEnum.exchangeRate_rate),2)};
                        // Le calcul du rate entraîne le recalcul de la contre-valeur si CTRValue = 0
                        if (cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount).IsEmptyValue)
                            ccic = new CciCompare[] { new CciCompare("AmountCur2", cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount), 1) };
                    }
                    //
                    if (ArrFunc.IsFilled(ccic))
                    {
                        Array.Sort(ccic);
                        //
                        switch (ccic[0].key)
                        {
                            case "AmountCur2":
                                CalculExchange(CurrencyEnum.customerCurrency);
                                break;
                            case "Rate":
                                CalculRateFromAmount();
                                CustomerSettlementPayment.Rate.Rate.Value = cciCustomerSettlementPayment.CciExchangeRate.Cci(CciExchangeRate.CciEnum.rate).NewValue;
                                break;
                        }
                    }
                }
                #endregion rate
                //
                #region Cas Montant
                if (false == cciCustomerSettlementPayment.CciExchangeRate.IsSpotFixing &&
                    pCci.IsFilledValue)
                {
                    if (IsCci(CciEnum.amount, pCci)
                        || cciCustomerSettlementPayment.IsCci(CciCustomerSettlementPayment.CciEnum.amount, pCci))
                    {
                        CciCompare[] ccic = null;
                        bool isFromAmountCur1 = IsCci(CciEnum.amount, pCci);
                        string clientIdSource = pCci.ClientId_WithoutPrefix;

                        string clientIdTarget;
                        if (isFromAmountCur1)
                            clientIdTarget = cciCustomerSettlementPayment.CciClientId(CciCustomerSettlementPayment.CciEnum.amount);
                        else
                            clientIdTarget = CciClientId(CciEnum.amount);

                        if ((!IsCaptureCustomerFilled) || isFromAmountCur1)
                        {
                            if (isFromAmountCur1 && cciCustomerSettlementPayment.CciExchangeRate.Cci(CciExchangeRate.CciEnum.rate).IsFilledValue)
                                ccic = new CciCompare[] { new CciCompare("AmountTarget", Ccis[clientIdTarget], 1) };
                            else
                                ccic = new CciCompare[] { new CciCompare("AmountTarget",Ccis[clientIdTarget],1), 
														  new CciCompare("Rate" ,cciCustomerSettlementPayment.CciExchangeRate.Cci(CciExchangeRate.CciEnum.rate),2)};
                        }
                        //						
                        if (ArrFunc.IsFilled(ccic))
                        {
                            Array.Sort(ccic);
                            CurrencyEnum currencyEnum;
                            //
                            switch (ccic[0].key)
                            {
                                case "AmountSource":
                                    // Cas particulier on L'utilisateur modifie un champs Montant et c'est ce dernier qui est racalculer => Mise à jour 
                                    if (clientIdSource == cciCustomerSettlementPayment.CciClientId(CciCustomerSettlementPayment.CciEnum.amount))
                                        currencyEnum = CurrencyEnum.customerCurrency;
                                    else
                                        currencyEnum = CurrencyEnum.paymentCurrency;
                                    //
                                    CalculExchange(currencyEnum);
                                    //
                                    if (isFromAmountCur1)
                                        Money.Amount.Value = Cci(CciPayment.CciEnumPayment.paymentAmount_amount).NewValue;
                                    else
                                    {
                                        CustomerSettlementPayment.Amount.Value = cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount).NewValue;
                                        CustomerSettlementPayment.AmountSpecified = cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount).IsFilledValue;
                                    }
                                    break;
                                case "AmountTarget":
                                    if (clientIdTarget == cciCustomerSettlementPayment.CciClientId(CciCustomerSettlementPayment.CciEnum.amount))
                                        currencyEnum = CurrencyEnum.customerCurrency;
                                    else
                                        currencyEnum = CurrencyEnum.paymentCurrency;
                                    CalculExchange(currencyEnum);
                                    break;
                                case "Rate":
                                    CalculRateFromAmount();
                                    break;
                            }
                        }
                    }
                }
                #endregion Montant
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string pPrefix)
        {
            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.RemoveLastItemInArray(pPrefix);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
            {
                if (IsCci(CciEnumPayment.paymentDate_dateAdjustments_bDC, pCci))
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, payment.PaymentDate.DateAdjustments);
            }
            //
            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.SetDisplay(pCci);
            //

        }

        #endregion IContainerCciFactory Members

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {

            bool isAlways = IsSpecified;
            //Payer
            _ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, isAlways);
            //
            //Receiver
            //Pas de synchrosisation du receiver lorsqu'il est à blanc et que l'utilisateur remplace la counterpartie à blanc par une donnée
            isAlways = IsSpecified;
            isAlways = isAlways && (TradeCustomCaptureInfos.CCst.Prefix_otherPartyPayment != prefix);
            _ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, isAlways);

        }

        #endregion

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciPayment.CciEnumPayment pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }

        public bool IsCciClientId(CciPayment.CciEnumFxOptionPremium pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }

        public bool IsCciClientId(CciPayment.CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region CciClientId
        public string CciClientId(CciPayment.CciEnum pEnumValue)
        {
            bool isPayment = (PaymentTypeEnum.Payment == paymentTypeEnum);

            string ret = string.Empty;

            switch (pEnumValue)
            {
                case CciEnum.payer:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.payer);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.payer);
                    break;
                case CciEnum.receiver:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.receiver);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.receiver);
                    break;

                case CciEnum.amount:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_amount);
                    break;

                case CciEnum.currency:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_currency);
                    break;

                case CciEnum.date:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.paymentDate_unadjustedDate);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.premiumSettlementDate);
                    break;

                case CciEnum.settlementInformation:
                    if (isPayment)
                        ret = CciClientId(CciPayment.CciEnumPayment.settlementInformation);
                    else
                        ret = CciClientId(CciPayment.CciEnumFxOptionPremium.settlementInformation);
                    break;

            }
            return ret;
        }

        public string CciClientId(CciPayment.CciEnumPayment pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(CciPayment.CciEnumFxOptionPremium pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciPayment.CciEnum pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }

        public CustomCaptureInfo Cci(CciPayment.CciEnumPayment pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }

        public CustomCaptureInfo Cci(CciPayment.CciEnumFxOptionPremium pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue)];
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumPayment pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(CciEnumFxOptionPremium pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion ITradeCci

        #region Membres de IContainerCciSpecified
        /// <summary>
        /// Retourne true si le payer est renseigné
        /// </summary>
        public bool IsSpecified
        {
            get { return Cci(CciEnum.payer).IsFilled; }
        }
        #endregion IContainerCciSpecified

        #region Membres de ITradeCciQuoteBasis
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            if (null != cciCustomerSettlementPayment)
            {
                if (false == isOk)
                    isOk = cciCustomerSettlementPayment.CciExchangeRate.IsClientId_QuoteBasis(pCci);
            }
            //
            return isOk;
        }
        //
        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (null != cciCustomerSettlementPayment)
            {
                if (StrFunc.IsEmpty(ret))
                    ret = cciCustomerSettlementPayment.CciExchangeRate.GetCurrency1(pCci);
            }
            //
            return ret;
        }
        //
        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (null != cciCustomerSettlementPayment)
            {
                if (StrFunc.IsEmpty(ret))
                    ret = cciCustomerSettlementPayment.CciExchangeRate.GetCurrency2(pCci);
            }
            //
            return ret;
        }
        #region public GetBaseCurrency
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            return ret;
        }
        #endregion
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCallCurrencyAmount"></param>
        /// <param name="pPutCurrencyAmout"></param>
        public void SetfxPremiumAmoutFromPremiumQuote(IMoney pCallCurrencyAmount, IMoney pPutCurrencyAmout)
        {

            Decimal amount = Decimal.Zero;
            string cur = string.Empty;
            //
            if (fxOptionPremium.PremiumQuoteSpecified)
            {
                switch (fxOptionPremium.PremiumQuote.PremiumQuoteBasis)
                {
                    case PremiumQuoteBasisEnum.PercentageOfCallCurrencyAmount:
                        amount = fxOptionPremium.PremiumQuote.PremiumValue.DecValue * pCallCurrencyAmount.Amount.DecValue;
                        cur = pCallCurrencyAmount.Currency;
                        break;
                    case PremiumQuoteBasisEnum.PercentageOfPutCurrencyAmount:
                        amount = fxOptionPremium.PremiumQuote.PremiumValue.DecValue * pPutCurrencyAmout.Amount.DecValue;
                        cur = pPutCurrencyAmout.Currency;
                        break;
                    case PremiumQuoteBasisEnum.CallCurrencyPerPutCurrency:
                        amount = fxOptionPremium.PremiumQuote.PremiumValue.DecValue * pPutCurrencyAmout.Amount.DecValue;
                        cur = pCallCurrencyAmount.Currency;
                        break;
                    case PremiumQuoteBasisEnum.PutCurrencyPerCallCurrency:
                        amount = fxOptionPremium.PremiumQuote.PremiumValue.DecValue * pCallCurrencyAmount.Amount.DecValue;
                        cur = pPutCurrencyAmout.Currency;
                        break;
                    case PremiumQuoteBasisEnum.Explicit:
                        amount = fxOptionPremium.PremiumQuote.PremiumValue.DecValue;
                        break;

                }
                //
                if (amount != Decimal.Zero)
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_amount), StrFunc.FmtDecimalToInvariantCulture(amount));
                if (StrFunc.IsFilled(cur))
                {
                    EFS_Cash cash = new EFS_Cash(cciTrade.CSCacheOn, amount, cur);
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_currency), cur);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void SetPaymentAmountFromPaymentQuote()
        {
            decimal notionalAmount = Decimal.Zero;
            string cur = string.Empty;
            //
            if (payment.PaymentQuoteSpecified && StrFunc.IsFilled(payment.PaymentQuote.PaymentRelativeTo.HRef))
            {
                string id = payment.PaymentQuote.PaymentRelativeTo.HRef;
                //
                object notionalReference = ReflectionTools.GetObjectById(cciTrade.CurrentTrade, id);
                //
                IMoney money = payment.GetNotionalAmountReference(notionalReference);
                if (null != money)
                {
                    notionalAmount = money.Amount.DecValue;
                    cur = money.Currency;
                }
                decimal percentageRate = payment.PaymentQuote.PercentageRate.DecValue;

                decimal amount;
                //
                if (payment.PaymentTypeSpecified && payment.PaymentType.Value == "Brokerage")
                    amount = BrokerageTools.CalcBrokerage(cciTrade.DataDocument, notionalAmount, percentageRate);
                else
                    amount = percentageRate * notionalAmount;
                //
                if ((amount) != Decimal.Zero)
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount), StrFunc.FmtDecimalToInvariantCulture(amount));
                if (StrFunc.IsFilled(cur))
                {
                    EFS_Cash cash = new EFS_Cash(cciTrade.CSCacheOn, amount, cur);
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumPayment.paymentAmount_amount), StrFunc.FmtDecimalToInvariantCulture(cash.AmountRounded));
                    _ccis.SetNewValue(CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency), cur);
                }
            }

        }

        /// <summary>
        /// Alimente le CCi CciEnum.date 
        /// <para>Cette méthode ne peut être appelée que dans un ProcessInitialize</para>
        /// </summary>
        /// <param name="pIsForced">si true, alimente le cci qu'il soit renseigné ou pas. Si false, alimente le cci s'il est vide uniquement</param>
        /// FI 20150316 [XXXXX] Modify 
        /// EG 20171109 [23509] Upd
        public void PaymentDateInitialize(bool pIsForced)
        {
            CustomCaptureInfo cciDefaultDate = _ccis[_clientIdDefaultDate];
            string defaultDate;
            // FI 20240412 [WI897]  Mise en place Verrue pour HPC
            // Verrue HPC => Copier/coller du code présent plus bas
            if (IsOPP && DefaultISINVOICING)
            {
                defaultDate = string.Empty;

                Nullable<DateTimeOffset> dtOffset = cciTrade.DataDocument.GetExecutionDateTimeOffset();
                if (false == dtOffset.HasValue)
                    dtOffset = cciTrade.DataDocument.GetOrderEnteredDateTimeOffset();

                if (dtOffset.HasValue)
                {
                    // FI 20190625 [XXXXX] Refactoring 
                    // L'usage de Tz.Tools.DateToStringISO n'est pas adapté
                    // Il faut convertir la date Execution ou OrderEntered (UTC) dans le fuseau horaire TradeTimeZone
                    DateTime? dtDefaultDate = dtOffset.Value.DateTime;

                    string timeZone = cciTrade.DataDocument.GetTradeTimeZone(cciTrade.CSCacheOn);
                    if (StrFunc.IsFilled(timeZone))
                    {
                        TimeZoneInfo tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(timeZone);
                        dtDefaultDate = TimeZoneInfo.ConvertTimeFromUtc(dtOffset.Value.UtcDateTime, tzInfoTarget);
                    }
                    defaultDate = DtFunc.DateTimeToString(dtDefaultDate.Value, DtFunc.FmtISODate);
                }

                CustomCaptureInfo cciDate = Cci(CciEnum.date);
                if (null != cciDate)
                {
                    if (StrFunc.IsEmpty(cciDate.LastValue) || pIsForced)
                        cciDate.NewValue = defaultDate;
                }
                return;
            }
            
            if (null != cciDefaultDate)
            {
                defaultDate = cciDefaultDate.NewValue;

                if (StrFunc.IsFilled(defaultDate))
                {
                    DateTime dtDefaultDate = DateTime.MinValue;
                    // FI 20190625 [XXXXX] cas datetimeoffset
                    switch (cciDefaultDate.DataType)
                    {
                        case TypeData.TypeDataEnum.datetimeoffset:
                            // FI 20190924 [24952][24953] Lors de l'importation des trades il n'existe pas le cci TMZ => Ajout du test dtLocalDatetime.HasValue
                            Nullable<DateTime> dtLocalDatetime = _ccis.GetLocalDateNewValue(cciDefaultDate.ClientId_WithoutPrefix, string.Empty);
                            if (false == dtLocalDatetime.HasValue)
                            {
                                if (this.cciTrade.cciProduct.IsCciExecutionDateTime(cciDefaultDate))
                                {
                                    // On rentre ici en mode import sur les EquitySecurityTransaction et les debtsecurityTransaction pour initialisation de la date de rglt du grossAmount
                                    // cette initialisation est obligatoire parce que Spheres® autorise que cette donnée soit non renseignée lors de l'importation (voir champ STLD dans la doc de l'importation)
                                    string timezone = cciTrade.DataDocument.GetTradeTimeZone(cciTrade.CSCacheOn, Ccis.User.Entity_IdA, Tz.Tools.UniversalTimeZone);
                                    dtLocalDatetime = _ccis.GetLocalDateNewValue(cciDefaultDate.ClientId_WithoutPrefix, timezone);
                                }
                            }
                            if (null != dtDefaultDate)
                                dtDefaultDate = dtLocalDatetime.Value;
                            break;
                        case TypeData.TypeDataEnum.datetime:
                        case TypeData.TypeDataEnum.date:
                            dtDefaultDate = new DtFunc().StringToDateTime(defaultDate, DtFunc.FmtISODate);
                            break;
                        default:
                            throw new InvalidProgramException(StrFunc.AppendFormat("datatype {0} is not supported", cciDefaultDate.DataType.ToString()));
                    }
                    if (cciTrade.DataDocument.CurrentProduct.IsExchangeTradedDerivative ||
                        cciTrade.DataDocument.CurrentProduct.IsStrategy)
                    {
                        // Pour les ETD, et pour l'Exe/Ass: La date de paiement des frais est égale à la date Exe/Ass + 1JO sur le BC du Market du DC
                        dtDefaultDate = cciTrade.DataDocument.CurrentProduct.GetNextBusinessDate(cciTrade.CSCacheOn, dtDefaultDate);
                    }
                    else if (null != this.DefaultDateSettings)
                    {
                        //FI 20150316 [XXXXX] prise en compte de defaultDateSettings
                        dtDefaultDate = Tools.ApplyOffset(cciTrade.CSCacheOn, dtDefaultDate, DefaultDateSettings.First, DefaultDateSettings.Second);
                    }

                    if (DtFunc.IsDateTimeFilled(dtDefaultDate))
                        defaultDate = DtFunc.DateTimeToString(dtDefaultDate, DtFunc.FmtISODate);
                }
            }
            else
            {
                defaultDate = string.Empty;
                if (cciTrade.DataDocument.CurrentProduct.IsExchangeTradedDerivative ||
                    cciTrade.DataDocument.CurrentProduct.IsStrategy)
                {
                    // Pour les ETD: La date de paiement des frais est égale à la date de compensation + 1JO sur le BC du Market du DC
                    DateTime dtDefaultDate = cciTrade.DataDocument.CurrentProduct.GetNextBusinessDate(cciTrade.CSCacheOn);
                    if (DtFunc.IsDateTimeFilled(dtDefaultDate))
                        defaultDate = DtFunc.DateTimeToString(dtDefaultDate, DtFunc.FmtISODate);
                }
                else
                {
                    Nullable<DateTimeOffset> dtOffset = cciTrade.DataDocument.GetExecutionDateTimeOffset();
                    if (false == dtOffset.HasValue)
                        dtOffset = cciTrade.DataDocument.GetOrderEnteredDateTimeOffset();

                    if (dtOffset.HasValue)
                    {
                        // FI 20190625 [XXXXX] Refactoring 
                        // L'usage de Tz.Tools.DateToStringISO n'est pas adapté
                        // Il faut convertir la date Execution ou OrderEntered (UTC) dans le fuseau horaire TradeTimeZone
                        DateTime? dtDefaultDate = dtOffset.Value.DateTime;

                        string timeZone = cciTrade.DataDocument.GetTradeTimeZone(cciTrade.CSCacheOn);
                        if (StrFunc.IsFilled(timeZone))
                        {
                            TimeZoneInfo tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(timeZone);
                            dtDefaultDate = TimeZoneInfo.ConvertTimeFromUtc(dtOffset.Value.UtcDateTime, tzInfoTarget);
                        }
                        defaultDate = DtFunc.DateTimeToString(dtDefaultDate.Value, DtFunc.FmtISODate);
                    }
                }
            }

            CustomCaptureInfo cci = Cci(CciEnum.date);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue) || pIsForced)
                    cci.NewValue = defaultDate;
            }
        }

        /// <summary>
        ///  Initialisation des ccis avec les valeurs par défaut (Les initiatisations s'appliquent uniquement sur les ccis non renseignés
        /// </summary>
        /// FI 20170116 [21916] Modify (Refactoring pour amélioration des performances en réduisant au max les recherches ds la collection de ccis)
        public void PaymentInitialize()
        {

            string currentClientIdDefaultReceiver = string.Empty;
            // Si aucune initialisation particuliere de _clientIdDefaultReceiver 
            // Le receiver est l'autre contrepartie
            CustomCaptureInfo cci = Ccis[_clientIdDefaultReceiver];
            if (null == cci)
            {
                if (Cci(CciPayment.CciEnum.payer).IsFilledValue)
                {
                    CustomCaptureInfo cciCounterpartyVs = cciTrade.GetCciCounterpartyVs(Cci(CciPayment.CciEnum.payer).NewValue);
                    if (null != cciCounterpartyVs)
                        currentClientIdDefaultReceiver = cciCounterpartyVs.ClientId_WithoutPrefix;
                }
            }
            else
            {
                currentClientIdDefaultReceiver = _clientIdDefaultReceiver;
            }

            string defaultReceiver;
            if (StrFunc.IsFilled(currentClientIdDefaultReceiver))
            {
                defaultReceiver = string.Empty;

                CustomCaptureInfo cciDefaultReceiver = _ccis[currentClientIdDefaultReceiver];

                //FI 20120709 [18003] Spheres recupère le defaultReceiver uniquement si ce cci n'est pas en cours de modification
                if (false == cciDefaultReceiver.HasChanged)
                {

                    defaultReceiver = cciDefaultReceiver.NewValue; //=> CodeBIC
                    if (null != (SQL_Actor)cciDefaultReceiver.Sql_Table)
                        defaultReceiver = ((SQL_Actor)cciDefaultReceiver.Sql_Table).XmlId;
                    //
                    if (defaultReceiver == Cci(CciEnum.payer).NewValue) // Garde Fou pour ne pas instaurer de payer et receiver identique
                        defaultReceiver = string.Empty;
                }
            }
            else
            {
                currentClientIdDefaultReceiver = CciClientIdReceiver;
                defaultReceiver = _ccis[currentClientIdDefaultReceiver].NewValue; //=> CodeBIC
            }

            bool isOk = (_ccis.Contains(currentClientIdDefaultReceiver));
            if (isOk)
            {
                CustomCaptureInfo cciDefaultBDC = _ccis[_clientIdDefaultBDC];
                string defaultBDC;
                if (null != cciDefaultBDC)
                    defaultBDC = cciDefaultBDC.NewValue;
                else
                    defaultBDC = BusinessDayConventionEnum.NONE.ToString();

                CustomCaptureInfo cciDefaultCurrency = _ccis[_clientIdDefaultCurrency];
                string defaultCurrency;
                if (null != cciDefaultCurrency)
                    defaultCurrency = cciDefaultCurrency.NewValue;
                else
                    defaultCurrency = CciTradeCommonBase.GetDefaultCurrency();
                // FI 20161114 [RATP] 
                // Sur les product  change l'élément payment.paymentDate ne doit pas être spécifié
                // La date est portée par l'élément fxLeg.valueDate (The date on which both currencies traded will settle)
                if (false == (this.cciTrade.Product.ProductBase.IsFxLeg || this.cciTrade.Product.ProductBase.IsFxSwap))
                {
                    // RD 20110429 
                    // Utilisation d'une méthode partagée PaymentDateInitialize()
                    PaymentDateInitialize(false);
                }

                string defaultSettlementType = Cst.SettlementTypeEnum.None.ToString();
                SetPaymentData(defaultReceiver, defaultBDC, defaultCurrency, _defaultPaymentType, defaultSettlementType, DefaultISINVOICING.ToString());
            }
        }

        /* FI 20200421 [XXXXX] Mise en commentaire
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        private void Dump_ToDocumentPayment()
        {
            foreach (CciEnumPayment cciEnum in Enum.GetValues(typeof(CciEnumPayment)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string clientId = cci.ClientId;
                    string data = cci.NewValue;
                    bool isSetting = true;

                    switch (cciEnum)
                    {
                        #region PaymentPayer/Receiver
                        case CciEnumPayment.payer:
                        case CciEnumPayment.receiver:
                            if (CciEnumPayment.payer == cciEnum)
                                payment.payerPartyReference.hRef = data;
                            else
                                payment.receiverPartyReference.hRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        #endregion PaymentPayer/Receiver
                        #region PaymentAmount
                        case CciEnumPayment.paymentAmount_amount:
                            payment.paymentAmount.amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        case CciEnumPayment.paymentAmount_currency:
                            payment.paymentAmount.currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                            break;
                        #endregion PaymentAmount
                        #region PaymentDate
                        case CciEnumPayment.paymentDate_unadjustedDate:
                            payment.paymentDateSpecified = cci.IsFilledValue;
                            payment.paymentDate.unadjustedDate.Value = data;
                            if (this.processQueueCciDate != CustomCaptureInfosBase.ProcessQueueEnum.None)
                                processQueue = processQueueCciDate;
                            break;
                        case CciEnumPayment.paymentDate_dateAdjustments_bDC:
                            DumpPaymentBDA();
                            break;
                        #endregion PaymentDate
                        #region PaymentType
                        case CciEnumPayment.paymentType:
                            payment.paymentTypeSpecified = cci.IsFilledValue;
                            payment.paymentType.Value = data;
                            payment.paymentSourceSpecified = false;
                            //
                            // RD 20110929 Paratage du code via Tools.SetPaymentSource()
                            Tools.SetPaymentSource(cciTrade.CSCacheOn, payment);
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion PaymentType
                        #region paymentQuote
                        case CciEnumPayment.paymentQuote_paymentRelativeTo:
                            //payment.paymentQuoteSpecified = cci.IsFilledValue; 
                            payment.paymentQuote.paymentRelativeTo.hRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnumPayment.paymentQuote_percentageRate:
                            payment.paymentQuoteSpecified = cci.IsFilledValue;
                            payment.paymentQuote.percentageRate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction:
                            payment.paymentQuoteSpecified = cci.IsFilledValue;
                            payment.paymentQuote.percentageRateFractionSpecified = cci.IsFilledValue && (false == cci.IsDataValidForFixedRate(data));
                            if (payment.paymentQuote.percentageRateFractionSpecified)
                            {
                                payment.paymentQuote.percentageRateFraction = data;
                                payment.paymentQuote.InitializePercentageRateFromPercentageRateFraction();
                            }
                            else
                            {
                                payment.paymentQuote.percentageRate.Value = data;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        #endregion paymentQuote
                        #region SettlementType
                        case CciEnumPayment.settlementInformation:
                            bool isSettlementInformationSpecified = (Cst.SettlementTypeEnum.None.ToString() != data) &&
                                StrFunc.IsFilled(data);
                            payment.settlementInformationSpecified = isSettlementInformationSpecified;
                            if (isSettlementInformationSpecified)
                            {
                                payment.settlementInformation.standardSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() != data);
                                payment.settlementInformation.instructionSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() == data);
                                //
                                if (payment.settlementInformation.standardSpecified)
                                    payment.settlementInformation.standard =
                                        (StandardSettlementStyleEnum)System.Enum.Parse(typeof(StandardSettlementStyleEnum), data);
                            }
                            else
                            {
                                payment.settlementInformation.standardSpecified = false;
                                payment.settlementInformation.instructionSpecified = false;
                            }
                            break;
                        #endregion SettlementType
                        #region paymentSource
                        case CciEnumPayment.paymentSource_feeSchedule:
                        case CciEnumPayment.paymentSource_feeMatrix:
                            // FI 20180427 [XXXXX] le barème n'est pas saissisable.
                            // Il peut toutefois passer à blanc lorsque l'utilisateur supprime une ligne de frais (lorsque le payer est placé à blanc)
                            if (StrFunc.IsEmpty(data))
                                cci.Sql_Table = null;
                            break;
                        case CciEnumPayment.paymentSource_feeInvoicing:
                            payment.paymentSourceSpecified = true;
                            if (payment.paymentSourceSpecified)
                            {
                                ISpheresIdSchemeId spheresIdScheme = payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme);
                                spheresIdScheme.Value = BoolFunc.IsTrue(data).ToString().ToLower();
                            }
                            break;
                        #endregion paymentSource

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                    {
                        if (false == ccis.IsLoading)
                        {
                            // 20080604 RD dans le cas d'un payment déja calculé automatiquement
                            if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheme))
                                payment.paymentSource.status = EfsML.Enum.SpheresSourceStatusEnum.Forced;
                        }
                        //
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
            }
        }
        */


        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        private void Dump_ToDocumentFxOptionPremium()
        {
            foreach (string clientId in Ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnumFxOptionPremium), cliendId_Key))
                {
                    CustomCaptureInfo cci = Ccis[clientId];
                    CciEnumFxOptionPremium cciEnum = (CciEnumFxOptionPremium)System.Enum.Parse(typeof(CciEnumFxOptionPremium), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region PaymentPayer/Receiver
                        case CciEnumFxOptionPremium.payer:
                        case CciEnumFxOptionPremium.receiver:
                            if (CciEnumFxOptionPremium.payer == cciEnum)
                                fxOptionPremium.PayerPartyReference.HRef = data;
                            else
                                fxOptionPremium.ReceiverPartyReference.HRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion PaymentPayer/Receiver

                        #region premiumQuote_premiumValue
                        case CciEnumFxOptionPremium.premiumQuote_premiumValue:
                            fxOptionPremium.PremiumQuote.PremiumValue.Value = data;
                            fxOptionPremium.PremiumQuoteSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        #region premiumQuote_premiumQuoteBasis
                        case CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis:
                            fxOptionPremium.PremiumQuoteSpecified = StrFunc.IsFilled(data);
                            if (fxOptionPremium.PremiumQuoteSpecified)
                            {
                                PremiumQuoteBasisEnum QbEnum = (PremiumQuoteBasisEnum)System.Enum.Parse(typeof(PremiumQuoteBasisEnum), data, true);
                                fxOptionPremium.PremiumQuote.PremiumQuoteBasis = QbEnum;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            }
                            break;
                        #endregion

                        #region PaymentAmount
                        case CciEnumFxOptionPremium.premiumAmount_amount:
                            fxOptionPremium.PremiumAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;

                        case CciEnumFxOptionPremium.premiumAmount_currency:
                            fxOptionPremium.PremiumAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        #endregion PaymentAmount

                        #region premiumSettlementDate
                        case CciEnumFxOptionPremium.premiumSettlementDate:
                            fxOptionPremium.PremiumSettlementDate.Value = data;
                            break;
                        #endregion

                        #region SettlementType
                        case CciEnumFxOptionPremium.settlementInformation:
                            ISettlementInformation stl = fxOptionPremium.SettlementInformation;
                            bool isSettlementInformationSpecified = (Cst.SettlementTypeEnum.None.ToString() != data) &&
                                StrFunc.IsFilled(data);

                            fxOptionPremium.SettlementInformationSpecified = isSettlementInformationSpecified;

                            if (isSettlementInformationSpecified)
                            {
                                stl.StandardSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() != data);
                                stl.InstructionSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() == data);
                                //
                                if (stl.StandardSpecified)
                                    stl.Standard = (StandardSettlementStyleEnum)System.Enum.Parse(typeof(StandardSettlementStyleEnum), data);
                            }
                            else
                            {
                                stl.StandardSpecified = false;
                                stl.InstructionSpecified = false;
                            }
                            break;
                        #endregion SettlementType

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }

                    if (isSetting)
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }

        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        private void ProcessInitializePayment(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnumPayment key = CciEnumPayment.unknown;
                if (System.Enum.IsDefined(typeof(CciEnumPayment), clientId_Element))
                    key = (CciEnumPayment)System.Enum.Parse(typeof(CciEnumPayment), clientId_Element);
                //
                switch (key)
                {
                    #region Payer/Receiver: Calcul des BCs
                    case CciEnumPayment.payer:
                        _ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);

                        if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.payer).IsMandatory))
                        {
                            // FI 20180427 [XXXXX] lorsque l'acteur est supprimé on efface les données paymentSource
                            // Si l'utilisateur renseigne ensuite le payeur tout se passe comme s'il saisit un frais manuel
                            if (payment.PaymentSourceSpecified)
                            {
                                payment.PaymentSourceSpecified = false;
                                payment.PaymentSource = payment.CreateSpheresSource;
                                Tools.InitializePaymentSource(payment, cciTrade.DataDocument.CurrentProduct.ProductBase);
                            }
                            Clear();
                        }
                        else
                            PaymentInitialize();

                        if (false == Cci(CciEnum.payer).IsMandatory)
                        {
                            SetEnabled(pCci.IsFilledValue);
                            // FI 20201105 [25554] montant et devise obligatoires conformément à la modelisation IMoney
                            // On suppose que si le payer est obligatoire alors les montant et devise seront de fait obligatoires et il n'y a pas besoin d'appliquer les instructions qui suivent
                            Ccis.Set(CciClientId(CciPayment.CciEnum.amount), "IsMandatory", pCci.IsFilledValue);
                            Ccis.Set(CciClientId(CciPayment.CciEnum.currency), "IsMandatory", pCci.IsFilledValue);
                        }

                        DumpPaymentBDA();
                        DumpPaymentSource(pCci.IsEmpty);
                        break;
                    case CciEnumPayment.receiver:
                        _ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        DumpPaymentBDA();
                        break;
                    #endregion
                    #region PaymentDate
                    case CciEnumPayment.paymentDate_unadjustedDate:
                        if (null != cciCustomerSettlementPayment && cciCustomerSettlementPayment.IsSpecified && cciCustomerSettlementPayment.CciExchangeRate.IsSpotFixing)
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.fixingDate), pCci.NewValue);
                        //
                        // RD 20091218 [16800]
                        //
                        // On a besoin de recahrger le Cci à partir du document dans le cas où:
                        // - Présence d'une Zone 'Date' sans Zone 'Ajustement'
                        //      (La Date est saisissable sur l'écran ( présence du Cci))
                        //      (Le businessDayConvention n'est pas saisissable sur l'écran (c'est un Cci System))
                        // - Le businessDayConvention n'est pas renseigné sur le document
                        //      (La Date n'est Ni renseignée sur le Template, Ni Calculée comme c'est le cas quand on a un Titre avec Offset comme sousjacent d'un debtSecurityTransaction)
                        //
                        // Ainsi à la saisie de la date:
                        // - paymentDate devient "Specified", 
                        // - et on va récupérer la valeur de 'paymentDate.dateAdjustments.businessDayConvention' pour renseigner le Cci
                        //
                        if (payment.PaymentDateSpecified)
                            Ccis.SetNewValue(CciClientId(CciEnumPayment.paymentDate_dateAdjustments_bDC), payment.PaymentDate.DateAdjustments.BusinessDayConvention.ToString());
                        break;
                    #endregion
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnumPayment.paymentAmount_amount:
                        _ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnumPayment.paymentAmount_amount), payment.PaymentAmount, (CciEnumPayment.paymentAmount_amount == key));
                        break;
                    case CciEnumPayment.paymentAmount_currency:
                        _ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnumPayment.paymentAmount_amount), payment.PaymentAmount, (CciEnumPayment.paymentAmount_amount == key));
                        DumpPaymentBDA();
                        break;
                    #endregion
                    case CciEnumPayment.paymentType:
                    case CciEnumPayment.paymentQuote_paymentRelativeTo:
                    case CciEnumPayment.paymentQuote_percentageRate:
                    case CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction:
                        SetPaymentAmountFromPaymentQuote();
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        private void ProcessInitializeFxOptionPremium(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnumFxOptionPremium key = CciEnumFxOptionPremium.unknown;
                if (System.Enum.IsDefined(typeof(CciEnumFxOptionPremium), clientId_Element))
                    key = (CciEnumFxOptionPremium)System.Enum.Parse(typeof(CciEnumFxOptionPremium), clientId_Element);
                //
                switch (key)
                {
                    #region Payer/Receiver: Calcul des BCs
                    case CciEnumFxOptionPremium.payer:
                        _ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.payer).IsMandatory))
                            Clear();
                        else
                            PaymentInitialize();

                        if (false == Cci(CciEnum.payer).IsMandatory)
                        {
                            SetEnabled(pCci.IsFilledValue);
                            // FI 20201105 [25554] montant et devise obligatoires conformément à la modelisation IMoney
                            // On suppose que si le payer est obligatoire alors les montant et devise seront de fait obligatoires et il n'y a pas besoin d'appliquer les instructions qui suivent
                            Ccis.Set(CciClientId(CciEnumFxOptionPremium.premiumAmount_amount), "IsMandatory", pCci.IsFilledValue);
                            Ccis.Set(CciClientId(CciEnumFxOptionPremium.premiumAmount_currency), "IsMandatory", pCci.IsFilledValue);
                        }

                        break;
                    case CciEnumFxOptionPremium.receiver:
                        _ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    #endregion
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnumFxOptionPremium.premiumAmount_amount:
                    case CciEnumFxOptionPremium.premiumAmount_currency:
                        _ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnumFxOptionPremium.premiumAmount_amount), fxOptionPremium.PremiumAmount,
                            (CciEnumFxOptionPremium.premiumAmount_currency == key));

                        break;
                    #endregion
                    #region premiumQuote_premiumValue
                    case CciEnumFxOptionPremium.premiumQuote_premiumValue:
                        if (fxOptionPremium.PremiumQuoteSpecified)
                        {
                            if (Cci(CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis).IsEmptyValue)
                                Cci(CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis).NewValue = fxOptionPremium.PremiumQuote.PremiumQuoteBasis.ToString();
                        }
                        else
                            Cci(CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis).NewValue = string.Empty;
                        break;
                    #endregion
                    #region premiumQuote_premiumQuoteBasis
                    case CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis:
                        break;
                    #endregion
                    #region premiumSettlementDate
                    case CciEnumFxOptionPremium.premiumSettlementDate:
                        if (null != cciCustomerSettlementPayment && cciCustomerSettlementPayment.IsSpecified && cciCustomerSettlementPayment.CciExchangeRate.IsSpotFixing)
                            Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciFixing.CciClientId(CciFxFixing.CciEnum.fixingDate), pCci.NewValue);
                        break;
                    #endregion
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void DumpPaymentBDA()
        {
            string clientId = CciClientId(CciEnumPayment.paymentDate_dateAdjustments_bDC);
            CciBC cciBC = new CciBC(cciTrade)
            {
                { CciClientIdPayer, CciBC.TypeReferentialInfo.Actor },
                { CciClientIdReceiver, CciBC.TypeReferentialInfo.Actor },
                { CciClientId(CciEnumPayment.paymentAmount_currency), CciBC.TypeReferentialInfo.Currency }
            };

            _ccis.DumpBDC_ToDocument(payment.PaymentDate.DateAdjustments, clientId, CciClientId(TradeCustomCaptureInfos.CCst.PAYMENT_BUSINESS_CENTERS_REFERENCE), cciBC);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsPayerEmpty"></param>
        private void DumpPaymentSource(bool pIsPayerEmpty)
        {

            if (pIsPayerEmpty)
            {
                payment.PaymentSourceSpecified = false;
                payment.TaxSpecified = false;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
                CciTools.SetCciContainer(this, "CciEnumPayment", "NewValue", string.Empty);
            else if (PaymentTypeEnum.FxOptionPremium == paymentTypeEnum)
                CciTools.SetCciContainer(this, "CciEnumFxOptionPremium", "NewValue", string.Empty);
            // 
            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.Clear();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(Boolean pIsEnabled)
        {
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
                CciTools.SetCciContainer(this, "CciEnumPayment", "IsEnabled", pIsEnabled);
            else if (PaymentTypeEnum.FxOptionPremium == paymentTypeEnum)
                CciTools.SetCciContainer(this, "CciEnumFxOptionPremium", "IsEnabled", pIsEnabled);
            
            //Doit tjs être Enabled 
            Cci(CciEnum.payer).IsEnabled = true;

            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.SetEnabled(cciCustomerSettlementPayment.IsSpecified);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pReceiver"></param>
        /// <param name="pBusinessDayConvention"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pPaymentType"></param>
        /// <param name="pSettlementType"></param>
        /// <param name="pIsInvoicing"></param>
        /// FI 20161214 [21916] Modify (divers refactoring pour tuning)
        private void SetPaymentData(string pReceiver, string pBusinessDayConvention, string pCurrency, string pPaymentType, string pSettlementType, string pIsInvoicing)
        {

            CustomCaptureInfo cci;

            //Receiver		
            cci = Cci(CciEnum.receiver);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pReceiver;
            }

            // RD 20110429 [17439]
            // La date est initialisée dans la méthode partagée PaymentDateInitialize()
            //BDC
            cci = Cci(CciEnumPayment.paymentDate_dateAdjustments_bDC);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pBusinessDayConvention;
            }

            //PaymentType
            cci = Cci(CciEnumPayment.paymentType);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pPaymentType;
            }

            //PaymentAmount
            cci = Cci(CciEnum.amount);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                {
                    //200812016 FI pour que cci.HasChanged = true afin de mettre à jour le datadocument;
                    cci.LastValue = "-1";
                    cci.NewValue = "0";
                }
            }

            //PaymentCurrency
            cci = Cci(CciEnum.currency);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pCurrency;
            }

            //PaymentSettlementInfo
            cci = Cci(CciEnum.settlementInformation);
            if (null != cci)
            {
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pSettlementType;
            }

            //PaymentIsInvoicing
            cci = Cci(CciEnumPayment.paymentSource_feeInvoicing);
            if (null != cci)
            {
                // 20090605 RD Ticket : 16496
                // Mettre la valeur par défaut uniquement à la Première saisie du Payer			
                CustomCaptureInfo cciPayer = Cci(CciEnum.payer);
                if (null != cciPayer && StrFunc.IsEmpty(cciPayer.LastValue))
                {
                    cci.NewValue = BoolFunc.IsTrue(pIsInvoicing).ToString().ToLower();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrencyEnum"></param>
        private void CalculExchange(CurrencyEnum pCurrencyEnum)
        {
            try
            {
                IQuotedCurrencyPair qcp = CustomerSettlementPayment.Rate.QuotedCurrencyPair;
                string cur1 = CustomerSettlementPayment.Rate.QuotedCurrencyPair.Currency1;
                string cur2 = CustomerSettlementPayment.Rate.QuotedCurrencyPair.Currency2;
                QuoteBasisEnum QbEnum = qcp.QuoteBasis;
                decimal rate = CustomerSettlementPayment.Rate.Rate.DecValue;
                //
                CurrencyEnum currencySource = CurrencyEnum.paymentCurrency;
                if (pCurrencyEnum == CurrencyEnum.paymentCurrency)
                    currencySource = CurrencyEnum.customerCurrency;
                //
                decimal amount = Decimal.Zero;
                //
                if (CurrencyEnum.paymentCurrency == currencySource)
                    amount = Money.Amount.DecValue;
                else
                {
                    if (CustomerSettlementPayment.AmountSpecified)
                        amount = CustomerSettlementPayment.Amount.DecValue;
                }
                //
                if (CurrencyEnum.customerCurrency == pCurrencyEnum)
                {
                    EFS_Cash cc2 = new EFS_Cash(cciTrade.CSCacheOn, cur1, cur2, amount, rate, QbEnum);
                    cciCustomerSettlementPayment.Cci(CciCustomerSettlementPayment.CciEnum.amount).NewValue =
                        StrFunc.FmtDecimalToInvariantCulture(cc2.ExchangeAmountRounded);
                }
                else
                {
                    EFS_Cash cc1 = new EFS_Cash(cciTrade.CSCacheOn, cur2, cur1, amount, 1 / rate, QbEnum);
                    Cci(CciEnum.amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(cc1.ExchangeAmountRounded);
                }
            }
            catch (DivideByZeroException) {/*donn't suppress*/}
        }

        /// <summary>
        /// 
        /// </summary>
        private void CalculRateFromAmount()
        {
            try
            {
                if (CustomerSettlementPayment.AmountSpecified)
                {
                    decimal amount1 = Money.Amount.DecValue;
                    decimal amount2 = CustomerSettlementPayment.Amount.DecValue;
                    IQuotedCurrencyPair qcp = CustomerSettlementPayment.Rate.QuotedCurrencyPair;

                    decimal rate = Tools.GetExchangeRate(cciTrade.CSCacheOn, qcp, amount1, amount2);

                    Ccis.SetNewValue(cciCustomerSettlementPayment.CciExchangeRate.CciClientId(CciExchangeRate.CciEnum.rate), StrFunc.FmtDecimalToInvariantCulture(rate));
                }
            }
            catch (DivideByZeroException) {/*donn't suppress*/}
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateInstance()
        {
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
            {
                CciTools.CreateInstance(this, Payment, "CciEnumPayment");
                //
                if (null != Cci(CciPayment.CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction))
                {
                    if (null == payment.PaymentQuote)
                        payment.PaymentQuote = payment.CreatePaymentQuote;
                    if (null == payment.PaymentQuote.PercentageRate)
                        payment.PaymentQuote.PercentageRate = new EFS_Decimal();
                    if (null == payment.PaymentQuote.PercentageRateFraction)
                        payment.PaymentQuote.PercentageRateFraction = string.Empty;
                    if (null == payment.PaymentQuote.PaymentRelativeTo)
                        payment.PaymentQuote.PaymentRelativeTo = payment.CreateReference;

                }
            }
            else if (PaymentTypeEnum.FxOptionPremium == paymentTypeEnum)
            {
                CciTools.CreateInstance(this, Payment, "CciEnumFxOptionPremium");
            }
        }


        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20180427 [XXXXX] refactoring
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            // MF 20110310 FxOptionPremium not managed (as usual but with a secure cast)
            if (this.Payment is IPayment)
            {
                CustomCaptureInfo cciFeeMatrix = this.Cci(CciPayment.CciEnumPayment.paymentSource_feeMatrix);
                if (null != cciFeeMatrix) //=> ADP, OPP, SKP
                {
                    // FI 20180502 [23926] 
                    SetTitleFeeMatrix(pPage, cciFeeMatrix);
                }

                CustomCaptureInfo cciFeeSchedule = this.Cci(CciPayment.CciEnumPayment.paymentSource_feeSchedule);
                if (null != cciFeeSchedule) //=> ADP, OPP, SKP
                {
                    SetTitleFeeSchedule(pPage, cciFeeSchedule);
                    SetImageFee(pPage, cciFeeSchedule);
                    // FI 20180502 [23926] gestion du boutons 3pts (ouverture du referentiel FEESCHEDULE)
                    // Le bouton est visible s'il existe un barème (possibilité offerte de le substituer)
                    if (pPage.PlaceHolder.FindControl(Cst.BUT + cciFeeSchedule.ClientId_WithoutPrefix) is WCToolTipButton img)
                        img.Visible = img.Visible && IsSpecified && (null != cciFeeSchedule.Sql_Table);
                }
            }

            if (null != cciCustomerSettlementPayment)
                cciCustomerSettlementPayment.DumpSpecific_ToGUI(pPage);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121114 [18224] Tuning
        private void Initialize_FromDocumentPayment()
        {

            foreach (CciEnumPayment cciEnum in Enum.GetValues(typeof(CciEnumPayment)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    string data = string.Empty;
                    SQL_Table sql_Table = null;
                    bool isSetting = true;

                    switch (cciEnum)
                    {
                        case CciEnumPayment.paymentAmount_amount:
                            data = payment.PaymentAmount.Amount.Value;
                            break;

                        case CciEnumPayment.paymentAmount_currency:
                            data = payment.PaymentAmount.Currency;
                            break;


                        case CciEnumPayment.paymentDate_unadjustedDate:
                            if (payment.PaymentDateSpecified)
                                data = payment.PaymentDate.UnadjustedDate.Value;
                            break;
                        case CciEnumPayment.paymentDate_dateAdjustments_bDC:
                            if (payment.PaymentDateSpecified)
                                data = payment.PaymentDate.DateAdjustments.BusinessDayConvention.ToString();
                            break;


                        case CciEnumPayment.paymentType:
                            if (payment.PaymentTypeSpecified)
                            {
                                data = payment.PaymentType.Value;
                                ISpheresIdSchemeId spheresIdScheme = payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme);
                                if (null != spheresIdScheme)
                                {
                                    SQL_Fee sqlFee = new SQL_Fee(cciTrade.CSCacheOn, spheresIdScheme.OTCmlId);
                                    if (sqlFee.IsLoaded)
                                        sql_Table = (SQL_Table)sqlFee;
                                }
                            }
                            break;


                        case CciEnumPayment.paymentQuote_paymentRelativeTo:
                            if (payment.PaymentQuoteSpecified)
                                data = payment.PaymentQuote.PaymentRelativeTo.HRef;
                            break;
                        case CciEnumPayment.paymentQuote_percentageRate:
                            if (payment.PaymentQuoteSpecified)
                                data = payment.PaymentQuote.PercentageRate.Value;
                            break;

                        case CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction:
                            if (payment.PaymentQuoteSpecified)
                            {
                                if (payment.PaymentQuote.PercentageRateFractionSpecified)
                                    data = payment.PaymentQuote.PercentageRateFraction;
                                else
                                    data = payment.PaymentQuote.PercentageRate.Value;
                            }
                            break;

                        case CciEnumPayment.paymentSource_feeSchedule:
                            if (payment.PaymentSourceSpecified)
                            {
                                ISpheresIdSchemeId spheresIdScheme = payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme);
                                if (null != spheresIdScheme)
                                {
                                    SQL_FeeSchedule sqlFeeSchedule = new SQL_FeeSchedule(cciTrade.CSCacheOn, spheresIdScheme.OTCmlId);
                                    if (sqlFeeSchedule.IsLoaded)
                                    {
                                        // FI 20180502 [23926] Affichage étendu
                                        //data = sqlFeeSchedule.Identifier;
                                        data = sqlFeeSchedule.ComplexLabel(cciTrade.Product.GetMainCurrency(cciTrade.CSCacheOn));
                                        sql_Table = (SQL_Table)sqlFeeSchedule;
                                    }
                                }
                            }
                            break;

                        case CciEnumPayment.paymentSource_feeMatrix:
                            // FI 20180502 [23926] add
                            if (payment.PaymentSourceSpecified)
                            {
                                ISpheresIdSchemeId spheresIdScheme = payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme);
                                if (null != spheresIdScheme)
                                {
                                    SQL_FeeMatrix sqlFeeMatrix = new SQL_FeeMatrix(cciTrade.CSCacheOn, spheresIdScheme.OTCmlId);
                                    if (sqlFeeMatrix.IsLoaded)
                                    {
                                        data = sqlFeeMatrix.ComplexLabel();
                                        sql_Table = (SQL_Table)sqlFeeMatrix;
                                    }
                                }
                            }
                            break;

                        case CciEnumPayment.paymentSource_feeInvoicing:
                            if (payment.PaymentSourceSpecified)
                            {
                                ISpheresIdSchemeId spheresIdScheme = payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme);
                                if (null != spheresIdScheme)
                                    data = BoolFunc.IsTrue(spheresIdScheme.Value).ToString().ToLower();
                            }
                            break;

                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
                }
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20121114 [18224] Tuning
        private void Initialize_FromDocumentFxOptionPremium()
        {

            foreach (CciEnumFxOptionPremium cciEnum in Enum.GetValues(typeof(CciEnumFxOptionPremium)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    string data = string.Empty;
                    SQL_Table sql_Table = null;
                    bool isSetting = true;

                    switch (cciEnum)
                    {
                        case CciEnumFxOptionPremium.premiumAmount_amount:
                            data = fxOptionPremium.PremiumAmount.Amount.Value;
                            break;

                        case CciEnumFxOptionPremium.premiumAmount_currency:
                            data = fxOptionPremium.PremiumAmount.Currency;
                            break;

                        case CciEnumFxOptionPremium.premiumSettlementDate:
                            data = fxOptionPremium.PremiumSettlementDate.Value;
                            break;

                        case CciEnumFxOptionPremium.premiumQuote_premiumValue:
                            if (fxOptionPremium.PremiumQuoteSpecified)
                                data = fxOptionPremium.PremiumQuote.PremiumValue.Value;
                            break;

                        case CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis:
                            if (fxOptionPremium.PremiumQuoteSpecified)
                                data = fxOptionPremium.PremiumQuote.PremiumQuoteBasis.ToString();
                            break;

                        default:
                            isSetting = false;
                            break;

                    }

                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void AddCciSystemPaymentDate()
        {
            //
            // Payment.PaymentDate n'est pas forcement renseignée (Ex sur le Change)
            //


            //
            // RD 20110429 []
            // Pour les ETD la date de payement pourrait être cachée, ce cci devient un cci systeme
            // uniquement sur les payments
            //
            // Le businessDayConvention est lui aussi caché, dans ce cas il faut absolument alimenter le dataDocument avec la bonne valeur.
            // 
            // Pour les frais, avec la date de payement et businessDayConvention cachés, businessDayConvention est toujours égale à "NONE"
            // - dans le cas de frais calculés automatiquement, voir FeeResponse.Calc()
            // - dans le cas de frais saisis manuellement, voir CciPayment.paymentInitialize()
            // 
            string clientId_WithoutPrefix = CciClientId(CciEnumPayment.paymentDate_unadjustedDate);
            if (PaymentTypeEnum.Payment == paymentTypeEnum)
                CciTools.AddCciSystem(Ccis, Cst.TXT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.date);

            if (_ccis.Contains(clientId_WithoutPrefix))
            {
                //s'il existe la PaymentDate (sur le Change PaymentDate n'est pas forcement renseignée)
                //Nécessaire pour le recalcul des BCs 
                clientId_WithoutPrefix = CciClientId(CciEnumPayment.paymentDate_dateAdjustments_bDC);
                // FI 2121128 [18289]  ce champ est tjs créé optionnel lorsqu'il n'existe pas
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCci"></param>
        /// EG 20160127 Add
        /// FI 20180427 [XXXXX] Gestion où SQL_FeeSchedule is null (ds cas cas le ctrl.lblViewer est non visible)
        private void SetTitleFeeSchedule(CciPageBase pPage, CustomCaptureInfo pCci)
        {
            if (pPage.PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) is WCDropDownList2 ddlCtrl)
            {
                if (ddlCtrl.HasViewer)
                {
                    ddlCtrl.LblViewer.Visible = false;
                    if (pCci.Sql_Table is SQL_FeeSchedule sql_FeeSchedule)
                    {
                        ddlCtrl.LblViewer.Visible = true;
                        ddlCtrl.SelectedItem.Text = sql_FeeSchedule.ComplexLabel(cciTrade.Product.GetMainCurrency(cciTrade.CSCacheOn));
                        ddlCtrl.ToolTip = ddlCtrl.LblViewer.Text;
                        pPage.SetOpenFormReferential(pCci, Cst.OTCml_TBL.FEESCHEDULE);
                    }
                }
            }
            else
            {
                // FI 20180502 [23926] Gestion si le controle est un WCTextBox2 
                WCTextBox2 txtctrl = pPage.PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) as WCTextBox2;
                txtctrl.Visible = false;
                if (pCci.Sql_Table is SQL_FeeSchedule sql_FeeSchedule)
                {
                    txtctrl.Visible = true;
                    txtctrl.Text = sql_FeeSchedule.ComplexLabel(cciTrade.Product.GetMainCurrency(cciTrade.CSCacheOn));
                    txtctrl.ToolTip = txtctrl.Text;
                    if (sql_FeeSchedule.IdA.HasValue)
                        ControlsTools.SetStyleList(txtctrl.Style, StrFunc.AppendFormat("border-color:{0};", CstCSSColor.redMedium));
                    //txtctrl.ReadOnly = true;
                    txtctrl.TabIndex = -1;
                    pPage.SetOpenFormReferential(pCci, Cst.OTCml_TBL.FEESCHEDULE);
                }
                else if (IsSpecified)
                {
                    txtctrl.Visible = true;
                    txtctrl.Text = Ressource.GetString("FeeManualInput", true);
                    txtctrl.ToolTip = txtctrl.Text;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCci"></param>
        /// FI 20180502 [23926] add
        private void SetTitleFeeMatrix(CciPageBase pPage, CustomCaptureInfo pCci)
        {
            if (pPage.PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) is WCDropDownList2 ddlCtrl)
            {
                if (ddlCtrl.HasViewer)
                {
                    ddlCtrl.LblViewer.Visible = false;
                    if (pCci.Sql_Table is SQL_FeeMatrix sql_FeeMatrix)
                    {
                        ddlCtrl.LblViewer.Visible = true;
                        ddlCtrl.SelectedItem.Text = sql_FeeMatrix.ComplexLabel();
                        ddlCtrl.ToolTip = ddlCtrl.LblViewer.Text;
                        pPage.SetOpenFormReferential(pCci, Cst.OTCml_TBL.FEEMATRIX);
                    }
                }
            }
            else
            {
                // FI 20180502 [23926] Gestion si le controle est un WCTextBox2 
                WCTextBox2 txtctrl = pPage.PlaceHolder.FindControl(pCci.ClientId_Prefix + pCci.ClientId_WithoutPrefix) as WCTextBox2;
                txtctrl.Visible = false;
                if (pCci.Sql_Table is SQL_FeeMatrix sql_FeeMatrix)
                {
                    txtctrl.Visible = true;
                    txtctrl.Text = sql_FeeMatrix.ComplexLabel();
                    pPage.SetOpenFormReferential(pCci, Cst.OTCml_TBL.FEEMATRIX);
                }
            }
        }


        /// <summary>
        ///  Mise à jour de l'image frais 
        ///  <para>Permet distinguer les frais (frais manuels, forcés, corrigés, etc...) </para>
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pCci"></param>
        /// FI 20180427 [XXXXX] Add
        private void SetImageFee(CciPageBase pPage, CustomCaptureInfo pCci)
        {
            //Control img feeSchedule
            if (pPage.PlaceHolder.FindControl(Cst.PNL + pCci.ClientId_WithoutPrefix) is WCToolTipPanel pnl)
            {
                ControlsTools.RemoveStyleDisplay(pnl);
                pnl.CssClass = string.Empty;
                pnl.Pty.TooltipContent = String.Empty;

                if (IsSpecified)
                {
                    if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme))
                    {
                        switch (payment.PaymentSource.Status)
                        {
                            case SpheresSourceStatusEnum.Forced:
                                pnl.Pty.TooltipContent = Ressource.GetString("imgFeeForced");
                                pnl.CssClass = "fee schedule forced";
                                break;
                            case SpheresSourceStatusEnum.Corrected:
                                pnl.Pty.TooltipContent = Ressource.GetString("imgFeeCorrected");
                                pnl.CssClass = "fee schedule corrected";
                                break;
                            case SpheresSourceStatusEnum.ScheduleForced:
                                pnl.Pty.TooltipContent = Ressource.GetString("imgFeeScheduleForced");
                                pnl.CssClass = "fee schedule scheduleForced";
                                break;
                            case SpheresSourceStatusEnum.Default:
                            default:
                                pnl.Pty.TooltipContent = Ressource.GetString("imgFeeFromSchedule");
                                pnl.CssClass = "fee schedule default";
                                break;
                        }
                    }
                    else if (payment.PaymentSourceSpecified && payment.PaymentSource.StatusSpecified &&
                        (payment.PaymentSource.Status == SpheresSourceStatusEnum.Corrected))
                    {
                        pnl.Pty.TooltipContent = Ressource.GetString("imgFeeCorrected");
                        pnl.CssClass = "fee schedule corrected";
                    }
                    else
                    {
                        // Manual data input
                        pnl.Pty.TooltipContent = Ressource.GetString("imgFeeManualInput");
                        pnl.CssClass = "fee schedule manual";
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        private void Dump_ToDocumentPayment()
        {
            foreach (string clientId in Ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnumPayment), cliendId_Key))
                {
                    CustomCaptureInfo cci = Ccis[clientId];
                    CciEnumPayment cciEnum = (CciEnumPayment)System.Enum.Parse(typeof(CciEnumPayment), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region PaymentPayer/Receiver
                        case CciEnumPayment.payer:
                        case CciEnumPayment.receiver:
                            if (CciEnumPayment.payer == cciEnum)
                                payment.PayerPartyReference.HRef = data;
                            else
                                payment.ReceiverPartyReference.HRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs
                            break;
                        #endregion PaymentPayer/Receiver
                        #region PaymentAmount
                        case CciEnumPayment.paymentAmount_amount:
                            payment.PaymentAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        case CciEnumPayment.paymentAmount_currency:
                            payment.PaymentAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs et d'arrondir PaymentAmount
                            break;
                        #endregion PaymentAmount
                        #region PaymentDate
                        case CciEnumPayment.paymentDate_unadjustedDate:
                            payment.PaymentDateSpecified = cci.IsFilledValue;
                            payment.PaymentDate.UnadjustedDate.Value = data;
                            if (this.ProcessQueueCciDate != CustomCaptureInfosBase.ProcessQueueEnum.None)
                                processQueue = ProcessQueueCciDate;
                            break;
                        case CciEnumPayment.paymentDate_dateAdjustments_bDC:
                            DumpPaymentBDA();
                            break;
                        #endregion PaymentDate
                        #region PaymentType
                        case CciEnumPayment.paymentType:
                            payment.PaymentTypeSpecified = cci.IsFilledValue;
                            payment.PaymentType.Value = data;
                            payment.PaymentSourceSpecified = false;
                            //
                            // RD 20110929 Paratage du code via Tools.SetPaymentSource()
                            Tools.SetPaymentSource(cciTrade.CSCacheOn, payment);
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion PaymentType
                        #region paymentQuote
                        case CciEnumPayment.paymentQuote_paymentRelativeTo:
                            //payment.paymentQuoteSpecified = cci.IsFilledValue; 
                            payment.PaymentQuote.PaymentRelativeTo.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnumPayment.paymentQuote_percentageRate:
                            payment.PaymentQuoteSpecified = cci.IsFilledValue;
                            payment.PaymentQuote.PercentageRate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        case CciEnumPayment.paymentQuote_percentageRateOrpercentageRateFraction:
                            payment.PaymentQuoteSpecified = cci.IsFilledValue;
                            payment.PaymentQuote.PercentageRateFractionSpecified = cci.IsFilledValue && (false == cci.IsDataValidForFixedRate(data));
                            if (payment.PaymentQuote.PercentageRateFractionSpecified)
                            {
                                payment.PaymentQuote.PercentageRateFraction = data;
                                payment.PaymentQuote.InitializePercentageRateFromPercentageRateFraction();
                            }
                            else
                            {
                                payment.PaymentQuote.PercentageRate.Value = data;
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        #endregion paymentQuote
                        #region SettlementType
                        case CciEnumPayment.settlementInformation:
                            bool isSettlementInformationSpecified = (Cst.SettlementTypeEnum.None.ToString() != data) &&
                                StrFunc.IsFilled(data);
                            payment.SettlementInformationSpecified = isSettlementInformationSpecified;
                            if (isSettlementInformationSpecified)
                            {
                                payment.SettlementInformation.StandardSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() != data);
                                payment.SettlementInformation.InstructionSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() == data);
                                //
                                if (payment.SettlementInformation.StandardSpecified)
                                    payment.SettlementInformation.Standard =
                                        (StandardSettlementStyleEnum)System.Enum.Parse(typeof(StandardSettlementStyleEnum), data);
                            }
                            else
                            {
                                payment.SettlementInformation.StandardSpecified = false;
                                payment.SettlementInformation.InstructionSpecified = false;
                            }
                            break;
                        #endregion SettlementType
                        #region paymentSource
                        case CciEnumPayment.paymentSource_feeSchedule:
                        case CciEnumPayment.paymentSource_feeMatrix:
                            // FI 20180427 [XXXXX] le barème n'est pas saissisable.
                            // Il peut toutefois passer à blanc lorsque l'utilisateur supprime une ligne de frais (lorsque le payer est placé à blanc)
                            if (StrFunc.IsEmpty(data))
                                cci.Sql_Table = null;
                            break;
                        case CciEnumPayment.paymentSource_feeInvoicing:
                            payment.PaymentSourceSpecified = true;
                            if (payment.PaymentSourceSpecified)
                            {
                                ISpheresIdSchemeId spheresIdScheme = payment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeInvoicingScheme);
                                spheresIdScheme.Value = BoolFunc.IsTrue(data).ToString().ToLower();
                            }
                            break;
                        #endregion paymentSource

                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }

                    if (isSetting)
                    {
                        if (false == Ccis.IsLoading)
                        {
                            // 20080604 RD dans le cas d'un payment déja calculé automatiquement
                            if (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheme))
                                payment.PaymentSource.Status = EfsML.Enum.SpheresSourceStatusEnum.Forced;
                        }
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
            }
        }
    }
}