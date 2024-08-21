#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.AssetDef;
using EfsML.v30.Shared;
using FixML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Reflection;
#endregion using directives

namespace EfsML.v30.CashBalance
{

    /// <summary>
    /// 
    /// </summary>
    public partial class CashFlows
    {
        #region constructor
        public CashFlows()
        {
            this.constituent = new CashFlowsConstituent();
        }
        public CashFlows(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);

            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }

            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }


        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashFlowsConstituent
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// PM 20150709 [21103] Add Constructor
        public CashFlowsConstituent()
        {
            safekeepingSpecified = false;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalance : IProduct
    {
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members

        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashBalance efs_CashBalance;
        #endregion Members

        #region Constructor
        public CashBalance()
        {
            this.cashBalanceOfficePartyReference = new PartyReference();
            this.entityPartyReference = new PartyReference();
        }
        #endregion
        #region methods
        /// <summary>
        /// Retourne timing
        /// <para>Utilisé par les évènements</para>
        /// </summary>
        /// <returns></returns>
        public SettlSessIDEnum GetTiming()
        {
            return timing;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceStream : IEFS_Array
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashBalanceStream efs_CashBalanceStream;
        #endregion Members

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrencyValue()
        {
            return currency.Value;
        }
        #endregion
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }

    /// <summary>
    /// 
    /// </summary>
    ///PM 20140910 [20066][20185] Add constructor
    public partial class ExchangeCashBalanceStream
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_CashBalanceStream efs_ExchangeCashBalanceStream;
        #endregion Members
        #region constructors
        public ExchangeCashBalanceStream()
        {
            marginRequirementSpecified = false;
            cashAvailableSpecified = false;
            cashUsedSpecified = false;
            collateralAvailableSpecified = false;
            collateralUsedSpecified = false;
            uncoveredMarginRequirementSpecified = false;
            marginCallSpecified = false;
            cashBalanceSpecified = false;
            previousMarginConstituentSpecified = false;
            realizedMarginSpecified = false;
            unrealizedMarginSpecified = false;
            liquidatingValueSpecified = false;
            marketValueSpecified = false;
            fundingSpecified = false;
            borrowingSpecified = false;
            forwardCashPaymentSpecified = false;
            equityBalanceSpecified = false;
            equityBalanceWithForwardCashSpecified = false;
            totalAccountValueSpecified = false;
            excessDeficitSpecified = false;
            excessDeficitWithForwardCashSpecified = false;
        }
        #endregion
        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrencyValue()
        {
            return currency.Value;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashAvailable
    {
        #region Members
        #endregion

        #region constructor
        public CashAvailable()
        {
            constituent = new CashAvailableConstituent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public CashAvailable(CashPosition pCashPosition)
            : base(pCashPosition)
        {

        }



        #endregion
    }



    /// <summary>
    /// 
    /// </summary>
    public partial class CollateralAvailable
    {
        #region Members
        #endregion

        #region constructor
        public CollateralAvailable()
        {
            constituent = new CollateralAvailableConstituent();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public CollateralAvailable(CashPosition pCashPosition)
            : base(pCashPosition)
        {

        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceScope
    {
        #region Constructors
        public CashBalanceScope()
        {

        }
        #endregion Constructors
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceSettings
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CashBalanceSettings()
        {
            this.cashBalanceOfficePartyReference = new PartyReference();
            this.exchangeCurrency = new Currency();
            this.useAvailableCash = new EFS_Boolean();
            this.managementBalance = new EFS_Boolean();
            this.cashBalanceMethod = CashBalanceCalculationMethodEnum.CSBDEFAULT;
            this.cashBalanceMethodSpecified = true;
            this.cashBalanceCurrency = new Currency();
            this.cashBalanceCurrencySpecified = false;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class AmountSide
    {
        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public AmountSide()
        {
            this.amt = new EFS_Decimal();
            this.AmtSide = default;
            this.AmtSideSpecified = false;
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CssAmount
    {
        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public CssAmount()
            : base()
        {
            //PM 20150402 [POC] cssHref devient optionel
            cssHrefSpecified = false;
            cssHref = string.Empty;
        }
        #endregion
    }



    /// <summary>
    /// 
    /// </summary>
    public partial class ContractAmount
    {
        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId

        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ContractAmount()
            : base()
        {
            m_Sym = new EFS_String();
            m_Exch = new EFS_String();
        }
        #endregion constructor
    }



    /// <summary>
    /// 
    /// </summary>
    public partial class ContractAmountAndTax
    {
        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public ContractAmountAndTax()
            : base()
        {
            tax = new AmountSide();
            taxSpecified = false;
        }
        #endregion constructor
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class CssExchangeCashPosition
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public CssExchangeCashPosition()
            : base()
        {
            detail = new CssAmount[] { new CssAmount() };
            detailSpecified = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExchCashPos"></param>
        public CssExchangeCashPosition(ExchangeCashPosition pExchCashPos)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pExchCashPos.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pExchCashPos.receiverPartyReference.href);

            //date
            this.dateDefineSpecified = pExchCashPos.dateDefineSpecified;
            if (dateDefineSpecified)
                this.dateDefine = (IdentifiedDate)pExchCashPos.dateDefine.Clone();
            this.dateReferenceSpecified = pExchCashPos.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pExchCashPos.dateReference.href;
            }

            //Montant
            this.amount = (Money)pExchCashPos.amount.Clone();

            //Exchange Montant
            this.exchangeAmountSpecified = pExchCashPos.exchangeAmountSpecified;
            if (this.exchangeAmountSpecified)
                this.exchangeAmount = (Money)pExchCashPos.exchangeAmount.Clone();

            //FxRate
            this.exchangeFxRateReferenceSpecified = pExchCashPos.exchangeFxRateReferenceSpecified;
            if (this.exchangeFxRateReferenceSpecified)
            {
                this.exchangeFxRateReference = new FxRateReference[pExchCashPos.exchangeFxRateReference.Length];
                for (int i = 0; i < pExchCashPos.exchangeFxRateReference.Length; i++)
                    this.exchangeFxRateReference[i].href = pExchCashPos.exchangeFxRateReference[i].href;
            }
        }
        #endregion constructor
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ContractSimplePayment
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ContractSimplePayment()
            : base()
        {
            this.detailSpecified = false;
            this.detail = new ContractAmount[] { new ContractAmount() };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSimplePayment"></param>
        public ContractSimplePayment(SimplePayment pSimplePayment)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pSimplePayment.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pSimplePayment.receiverPartyReference.href);

            //date
            this.paymentDate = new AdjustableOrRelativeAndAdjustedDate();
            this.paymentDate.adjustedDateSpecified = pSimplePayment.paymentDate.adjustedDateSpecified;
            if (this.paymentDate.adjustedDateSpecified)
            {
                this.paymentDate.adjustedDate = new IdentifiedDate();
                this.paymentDate.adjustedDate.Value = pSimplePayment.paymentDate.adjustedDate.Value;
            }
            //
            this.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified = pSimplePayment.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified;
            if (this.paymentDate.adjustableOrRelativeDateAdjustableDateSpecified)
                this.paymentDate.adjustableOrRelativeDateAdjustableDate = (AdjustableDate)pSimplePayment.paymentDate.adjustableOrRelativeDateAdjustableDate.Clone();

            //
            this.paymentDate.adjustableOrRelativeDateRelativeDateSpecified = pSimplePayment.paymentDate.adjustableOrRelativeDateRelativeDateSpecified;
            if (this.paymentDate.adjustableOrRelativeDateRelativeDateSpecified)
                this.paymentDate.adjustableOrRelativeDateRelativeDate = (RelativeDateOffset)pSimplePayment.paymentDate.adjustableOrRelativeDateRelativeDate.Clone();

            //Amount
            this.paymentAmount = (Money)pSimplePayment.paymentAmount.Clone();

        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class DetailedContractPayment
    {
        /// <summary>
        /// 
        /// </summary>
        public DetailedContractPayment()
            : base()
        {
            ///PM 20140829 [20066][20185] Rename DerivativeContractPayment to DetailedContractPayment
            //type = "efs:DerivativeContractPayment";
            type = "efs:DetailedContractPayment";
            detailSpecified = false;
            detail = new ContractAmountAndTax[] { new ContractAmountAndTax() };
        }
    }

    /// <summary>
    /// Constituant d'un montant portant sur des options
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class OptionMarginConstituent
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public OptionMarginConstituent()
            : base()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public OptionMarginConstituent(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// Constituant d'un montant portant sur des contrats OTC
    /// </summary>
    /// PM 20150616 [21124] New
    public partial class AssetMarginConstituent
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public AssetMarginConstituent()
            : base()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public AssetMarginConstituent(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// Détail d'un montant de valeur liquidative options
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class OptionLiquidatingValue
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public OptionLiquidatingValue()
            : base()
        {
            longOptionValueSpecified = false;
            longOptionValue = new CashPosition();
            shortOptionValueSpecified = false;
            shortOptionValue = new CashPosition();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public OptionLiquidatingValue(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// Détail d'un montant de cash payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class CashBalancePayment
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public CashBalancePayment()
            : base()
        {
            cashDepositSpecified = false;
            cashDeposit = new DetailedCashPayment();
            cashWithdrawalSpecified = false;
            cashWithdrawal = new DetailedCashPayment();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public CashBalancePayment(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// Détail d'un montant de cash payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class DetailedCashPayment : CashPosition
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public DetailedCashPayment()
            : base()
        {
            detailSpecified = false;
            detail = new CashPaymentDetail[] { new CashPaymentDetail() };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public DetailedCashPayment(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// Montant pour un type de payment
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class CashPaymentDetail
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public CashPaymentDetail()
            : base()
        {
            paymentTypeSpecified = false;
            paymentType = new PaymentType();
        }
    }

    /// <summary>
    /// Montant détaillé par contrat
    /// </summary>
    ///PM 20140829 [20066][20185] New
    public partial class DetailedCashPosition
    {
        /// <summary>
        /// Constructeur
        /// </summary>
        public DetailedCashPosition()
            : base()
        {
            this.dateDetailSpecified = false;
            this.dateDetail = new DetailedDateAmount[] { new DetailedDateAmount() };
            this.detailSpecified = false;
            this.detail = new ContractAmount[] { new ContractAmount() };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        public DetailedCashPosition(CashPosition pCashPosition)
            : this()
        {
            //Payer/Receiver
            this.payerPartyReference = new PartyOrAccountReference(pCashPosition.payerPartyReference.href);
            this.receiverPartyReference = new PartyOrAccountReference(pCashPosition.receiverPartyReference.href);
            //date
            this.dateDefineSpecified = pCashPosition.dateDefineSpecified;
            if (dateDefineSpecified)
            {
                this.dateDefine = (IdentifiedDate)pCashPosition.dateDefine.Clone();
            }
            this.dateReferenceSpecified = pCashPosition.dateReferenceSpecified;
            if (dateReferenceSpecified)
            {
                this.dateReference = new DateReference();
                this.dateReference.href = pCashPosition.dateReference.href;
            }
            //Montant
            this.amount = (Money)pCashPosition.amount.Clone();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    //PM 20150319 [POC] Add ContractSimplePaymentConstituant
    public partial class ContractSimplePaymentConstituent : ContractSimplePayment
    {
        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public ContractSimplePaymentConstituent()
            : base()
        {
            this.optionSpecified = false;
            this.otherSpecified = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSimplePayment"></param>
        public ContractSimplePaymentConstituent(SimplePayment pSimplePayment)
            : base(pSimplePayment)
        {
            this.optionSpecified = false;
            this.otherSpecified = false;
        }
        #endregion
    }

    /// <summary>
    /// Montant détaillé par date
    /// </summary>
    /// PM 20150616 [21124] New
    public partial class DetailedDateAmount
    {
        #region constructor
        /// <summary>
        /// Constructor
        /// </summary>
        public DetailedDateAmount()
            : base()
        {
            this.detailSpecified = false;
            this.detail = new ContractAmount[] { new ContractAmount() };
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20160530 [21885] Add
    public partial class PosCollateral : ISpheresId, IEFS_Array
    {
        #region OTCmlId
        /// <summary>
        /// IDPOSCOLLATERAL
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId

        #region ISpheresId Members
        string ISpheresId.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
    }

    /// <summary>
    /// 
    /// </summary>
    /// FI 20160530 [21885] Add
    public partial class PosCollateralValuation : ISpheresId
    {
        #region OTCmlId
        /// <summary>
        /// IDPOSCOLLATERALVAL
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId

        #region ISpheresId Members
        string ISpheresId.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        int ISpheresId.OTCmlId
        {
            set { this.OTCmlId = value; }
            get { return this.OTCmlId; }
        }
        #endregion
    }



    /// <summary>
    /// 
    /// </summary>
    /// FI 20160530 [21885] Add
    public partial class CssValue : IEFS_Array
    {

        #region IEFS_Array Members
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion IEFS_Array Members
    }

    /// <summary>
    /// Représente un Asset ou un DerivativeContract
    /// <para>Similitude avec ContractAmount pour que les assets soient réprésentés de la même manière dans un flux CB</para>
    /// </summary>
    public partial class ContractAsset
    {
        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId

        #region constructor
        /// <summary>
        /// Constructor par défaut
        /// </summary>
        public ContractAsset()
            : base()
        {
            m_Sym = new EFS_String();
            m_Exch = new EFS_String();
        }
        #endregion constructor
    }

}
