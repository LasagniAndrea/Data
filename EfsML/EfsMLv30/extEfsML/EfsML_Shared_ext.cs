#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.AssetDef;
using EfsML.v30.Ird;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Shared;
using System;
using System.Reflection;
using Tz = EFS.TimeZone;
#endregion using directives

namespace EfsML.v30.Shared
{
    #region AbstractTransaction
    public abstract partial class AbstractTransaction : IAbstractTransaction,IProduct
	{
		#region IAbstractTransaction Members
		IReference IAbstractTransaction.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IAbstractTransaction.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		#endregion IAbstractTransaction Members
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members	
	}
	#endregion AbstractUnitTransaction
	#region AbstractUnitTransaction
	public abstract partial class AbstractUnitTransaction : IAbstractUnitTransaction
	{
		#region IAbstractUnitTransaction Members
		EFS_Decimal IAbstractUnitTransaction.NumberOfUnits
		{
			set { this.numberOfUnits = value; }
			get { return this.numberOfUnits; }
		}
		IMoney IAbstractUnitTransaction.UnitPrice
		{
			set { this.unitPrice = (Money)value; }
			get { return this.unitPrice; }
		}
		#endregion IAbstractUnitTransaction Members
		#region IAbstractTransaction Members
		IReference IAbstractTransaction.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IAbstractTransaction.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		#endregion IAbstractTransaction Members
	}
	#endregion AbstractUnitTransaction
    #region AccountNumber
    public partial class AccountNumber : IAccountNumber
    {
        #region Constructors
        public AccountNumber()
        {
            accountName = new EFS_String();
            accountNumber = new EFS_String();
            correspondant = new SpheresId();
            currency = new Currency();
            nostroAccountNumber = new EFS_String();
        }
        #endregion Constructors
        #region IAccountNumber Members
        ISpheresIdSchemeId IAccountNumber.Correspondant
        {
            set { this.correspondant = (SpheresId)value; }
            get { return this.correspondant; }
        }
        ICurrency IAccountNumber.Currency
        {
            set { this.currency = (Currency)value; }
            get { return this.currency; }
        }
        EFS_String IAccountNumber.NostroAccountNumber
        {
            set { this.nostroAccountNumber = value; }
            get { return this.nostroAccountNumber; }
        }
        EFS_String IAccountNumber.AccountNumber
        {
            set { this.accountNumber = value; }
            get { return this.accountNumber; }
        }
        EFS_String IAccountNumber.AccountName
        {
            set { this.accountName = value; }
            get { return this.accountName; }
        }
        EFS_String IAccountNumber.JournalCode
        {
            set { this.journalCode = value; }
            get { return this.journalCode; }
        }
        #endregion IAccountNumber Members
    }
    #endregion AccountNumber
    #region AssetFxRateId
    public partial class AssetFxRateId : ICloneable , IAssetFxRateId
	{
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public int OTCmlId
		{
			get { return Convert.ToInt32(otcmlId); }
			set { otcmlId = value.ToString(); }
		}
		#endregion Accessors
		#region Constructors
		public AssetFxRateId() { }
		#endregion Constructors

		#region ICloneable Members
		#region Clone
		public object Clone()
		{
            AssetFxRateId clone = new AssetFxRateId
            {
                Value = this.Value,
                otcmlId = this.otcmlId
            };
            return clone;
		}
		#endregion Clone
		#endregion ICloneable Members
		#region IAssetFxRateId Members
		string IAssetFxRateId.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
        }
        #endregion IAssetFxRateId Members
        #region ISpheresId
        string ISpheresId.OtcmlId
		{
			set { this.otcmlId = value; }
			get { return this.otcmlId; }
		}
        int ISpheresId.OTCmlId
		{
			set { this.OTCmlId = value; }
			get { return this.OTCmlId; }
        }
        #endregion
    }
	#endregion AssetFxRateId

    #region CalculationBase
    public partial class CalculationBase : ICalculation
    {
        #region Constructors
        public CalculationBase()
        {
            calculationNotional = new Notional();
            rateFixedRate = new Schedule();
            rateFloatingRate = new FloatingRateCalculation();
        }
        #endregion Constructors
        #region ICalculation Members
        bool ICalculation.NotionalSpecified
        {
            set { ; }
            get { return true; }
        }
        INotional ICalculation.Notional { get { return this.calculationNotional; } }
        bool ICalculation.FxLinkedNotionalSpecified
        {
            set { ;}
            get { return false; }
        }
        IFxLinkedNotionalSchedule ICalculation.FxLinkedNotional
        {
            set { ;}
            get { return null; }
        }
        bool ICalculation.RateFixedRateSpecified
        {
            set { this.rateFixedRateSpecified = value; }
            get { return this.rateFixedRateSpecified; }
        }
        ISchedule ICalculation.RateFixedRate { get { return this.rateFixedRate; } }
        bool ICalculation.RateFloatingRateSpecified
        {
            set { rateFloatingRateSpecified = value; }
            get { return rateFloatingRateSpecified; }
        }
        IFloatingRateCalculation ICalculation.RateFloatingRate { get { return rateFloatingRate; } }
        bool ICalculation.RateInflationRateSpecified
        {
            set { ; }
            get { return false; }
        }
        IInflationRateCalculation ICalculation.RateInflationRate { get { return null; } }
        bool ICalculation.DiscountingSpecified { get { return this.discountingSpecified; } }
        IDiscounting ICalculation.Discounting { get { return this.discounting; } }
        bool ICalculation.CompoundingMethodSpecified { get { return this.compoundingMethodSpecified; } }
        CompoundingMethodEnum ICalculation.CompoundingMethod { get { return this.compoundingMethod; } }
        DayCountFractionEnum ICalculation.DayCountFraction
        {
            set { this.dayCountFraction = value; }
            get { return this.dayCountFraction; }
        }
        #endregion ICalculation Members
    }
    #endregion CalculationBase
    #region CalculationPeriodAmountBase
    public partial class CalculationPeriodAmountBase : ICalculationPeriodAmount
    {
        #region Constructors
        public CalculationPeriodAmountBase()
        {
            calculationPeriodAmountCalculation = new CalculationBase();
            calculationPeriodAmountKnownAmountSchedule = new KnownAmountSchedule();
        }
        #endregion Constructors

        #region ICalculationPeriodAmount Members
        bool ICalculationPeriodAmount.CalculationSpecified
        {
            set { this.calculationPeriodAmountCalculationSpecified = value; }
            get { return this.calculationPeriodAmountCalculationSpecified; }
        }
        ICalculation ICalculationPeriodAmount.Calculation { get { return this.calculationPeriodAmountCalculation; } }
        bool ICalculationPeriodAmount.KnownAmountScheduleSpecified
        {
            set { this.calculationPeriodAmountKnownAmountScheduleSpecified = value; }
            get { return this.calculationPeriodAmountKnownAmountScheduleSpecified; }
        }
        IKnownAmountSchedule ICalculationPeriodAmount.KnownAmountSchedule { get { return this.calculationPeriodAmountKnownAmountSchedule; } }
        #endregion ICalculationPeriodAmount Members
    }
    #endregion CalculationPeriodAmountBase

    /// <summary>
    /// 
    /// </summary>
    public partial class CashPosition
    {
        /// <summary>
        /// 
        /// </summary>
        private EFS_CashPosition _efsCashPosition;

        #region constructor
        public CashPosition()
        {
            dateDefine = new IdentifiedDate();
            dateReference = new DateReference();
            date = new EFS_RadioChoice();

            amount = new Money();
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
        }
        #endregion

        #region CashPosition
        /// <summary>
        /// Alimente l'élément efs_cashAvailable du CashAvailable
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataDoc">Représente le datadocument dans lequel est présent le MarginRequirement</param>
        public void SetEfsCashPosition(DataDocumentContainer pDataDoc)
        {
            _efsCashPosition = new EFS_CashPosition((CashPosition)this, pDataDoc);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EFS_Decimal GetAmount()
        {
            return amount.Amount;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetCurrency()
        {
            return amount.Currency;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public EFS_EventDate GetEventDate()
        {
            if (null == _efsCashPosition)
                throw new NullReferenceException("Field is null, call Method CashPosition.SetEfsCashPosition");
            return _efsCashPosition.GetEventDate();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetPayerReceiver(string pPayerReceiver)
        {
            if (false == System.Enum.IsDefined(typeof(PayerReceiverEnum), pPayerReceiver))
                throw new ArgumentException(StrFunc.AppendFormat("value {0} is not Defined in PayerReceiverEnum"), pPayerReceiver);
            //
            PayerReceiverEnum payerReceiver = (PayerReceiverEnum)System.Enum.Parse(typeof(PayerReceiverEnum), pPayerReceiver);
            //
            string ret = string.Empty;
            if (payerReceiver == PayerReceiverEnum.Payer)
                ret = payerPartyReference.href;
            else if (payerReceiver == PayerReceiverEnum.Receiver)
                ret = receiverPartyReference.href;
            return ret;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	public partial class CustomerSettlementPayment : ICustomerSettlementPayment
	{
		#region Constructors
		public CustomerSettlementPayment()
		{
			rate = new ExchangeRate();
			currency = new Currency();
		}
		#endregion Constructors

		#region ICustomerSettlementPayment Members
		IExchangeRate ICustomerSettlementPayment.Rate {get {return this.rate;}}
		bool ICustomerSettlementPayment.AmountSpecified 
		{
			set { amountSpecified = value;}
			get { return amountSpecified;}
		}
        IMoney ICustomerSettlementPayment.GetMoney()
        {
            return new Money(amount.DecValue, currency.Value);
        }
        EFS_Decimal ICustomerSettlementPayment.Amount
        {
            set { amount = value; }
            get { return amount; }
        }
        string ICustomerSettlementPayment.Currency
        {
            set { currency.Value = value; }
            get { return currency.Value; }
        }
		#endregion ICustomerSettlementPayment Members
	}

    /// <summary>
    /// 
    /// </summary>
    public partial class EventType : IScheme
    {
        #region Constructors
        public EventType()
        {
            eventTypeScheme = "http://www.euro-finance-systems.fr/otcml/eventType";
            Value = string.Empty;
        }
        #endregion Constructors

        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.eventTypeScheme = value; }
            get { return this.eventTypeScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    
    /// <summary>
    /// 
    /// </summary>
    public partial class ExchangeCashPosition
    {
        #region constructor
        public ExchangeCashPosition()
            : base()
        {
            this.exchangeAmountSpecified = false;
            this.exchangeAmount = new Money();

            this.exchangeFxRateReferenceSpecified = false;
            this.exchangeFxRateReference = new FxRateReference[] { };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosiiton"></param>
        public ExchangeCashPosition(CashPosition pCashPosition)
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
        #endregion constructor
    }

	#region ImplicitProvision
	public partial class ImplicitProvision : IImplicitProvision
	{
		#region Constructors
		public ImplicitProvision()
		{
			cancelableProvision = new Empty();
			mandatoryEarlyTerminationProvision = new Empty();
			optionalEarlyTerminationProvision = new Empty();
			extendibleProvision = new Empty();
			stepUpProvision = new Empty();
		}
		#endregion Constructors

		#region IImplicitProvision Membres
		bool IImplicitProvision.CancelableProvisionSpecified
		{
			set { this.cancelableProvisionSpecified = value; }
			get { return this.cancelableProvisionSpecified; }
		}
		IEmpty IImplicitProvision.CancelableProvision
		{
			set { this.cancelableProvision = (Empty)value; }
			get { return this.cancelableProvision; }
		}
		bool IImplicitProvision.MandatoryEarlyTerminationProvisionSpecified
		{
			set { this.mandatoryEarlyTerminationProvisionSpecified = value; }
			get { return this.mandatoryEarlyTerminationProvisionSpecified; }
		}
		IEmpty IImplicitProvision.MandatoryEarlyTerminationProvision
		{
			set { this.mandatoryEarlyTerminationProvision = (Empty)value; }
			get { return this.mandatoryEarlyTerminationProvision; }
		}
		bool IImplicitProvision.OptionalEarlyTerminationProvisionSpecified
		{
			set { this.optionalEarlyTerminationProvisionSpecified = value; }
			get { return this.optionalEarlyTerminationProvisionSpecified; }
		}
		IEmpty IImplicitProvision.OptionalEarlyTerminationProvision
		{
			set { this.optionalEarlyTerminationProvision = (Empty)value; }
			get { return this.optionalEarlyTerminationProvision; }
		}
		bool IImplicitProvision.ExtendibleProvisionSpecified
		{
			set { this.extendibleProvisionSpecified = value; }
			get { return this.extendibleProvisionSpecified; }
		}
		IEmpty IImplicitProvision.ExtendibleProvision
		{
			set { this.extendibleProvision = (Empty)value; }
			get { return this.extendibleProvision; }
		}
		bool IImplicitProvision.StepUpProvisionSpecified
		{
			set { this.stepUpProvisionSpecified = value; }
			get { return this.stepUpProvisionSpecified; }
		}
		IEmpty IImplicitProvision.StepUpProvision
		{
			set { this.stepUpProvision = (Empty)value; }
			get { return this.stepUpProvision; }
		}
		#endregion IImplicitProvision Members
	}
	#endregion ImplicitProvision
    #region Identification
    public partial class Identification : IScheme
    {
        #region Constructors
        public Identification() { }
        #endregion Constructors


        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.identificationScheme = value ; }
            get { return this.identificationScheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion Identification

    #region InterestRateStreamBase
    public abstract partial class InterestRateStreamBase : IEFS_Array, IInterestRateStream
    {
        #region Constructors
        public InterestRateStreamBase()
        {
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
            calculationPeriodDates = new CalculationPeriodDates();
            paymentDates = new PaymentDates();
            resetDates = new ResetDates();
            calculationPeriodAmount = new CalculationPeriodAmountBase();
            stubCalculationPeriodAmount = new StubCalculationPeriodAmount();
            settlementProvision = new SettlementProvision();
            formula = new Formula();
            principalExchanges = new PrincipalExchanges(); 
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region IInterestRateStream Members
        IReference IInterestRateStream.PayerPartyReference 
        { 
            set {  this.payerPartyReference = (PartyOrAccountReference) value; } 
            get { return this.payerPartyReference; } 
        }
        IReference IInterestRateStream.ReceiverPartyReference 
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; } 
            get { return this.receiverPartyReference; } 
        }
        ICalculationPeriodAmount IInterestRateStream.CalculationPeriodAmount { get { return this.calculationPeriodAmount; } }
        bool IInterestRateStream.StubCalculationPeriodAmountSpecified
        {
            set { this.stubCalculationPeriodAmountSpecified = value; }
            get { return this.stubCalculationPeriodAmountSpecified; }
        }
        IStubCalculationPeriodAmount IInterestRateStream.StubCalculationPeriodAmount
        {
            set { this.stubCalculationPeriodAmount = (StubCalculationPeriodAmount)value; }
            get { return this.stubCalculationPeriodAmount; }
        }
        bool IInterestRateStream.ResetDatesSpecified
        {
            set { this.resetDatesSpecified = value; }
            get { return this.resetDatesSpecified; }
        }
        ICalculationPeriodDates IInterestRateStream.CalculationPeriodDates
        {
            get { return this.calculationPeriodDates; }
            set { this.calculationPeriodDates = (CalculationPeriodDates)value; }
        }
        IPaymentDates IInterestRateStream.PaymentDates
        {
            get { return this.paymentDates; }
            set { this.paymentDates = (PaymentDates)value; }
        }
        IResetDates IInterestRateStream.ResetDates
        {
            get { return this.resetDates; }
            set { this.resetDates = (ResetDates)value; }
        }
        string IInterestRateStream.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        bool IInterestRateStream.PrincipalExchangesSpecified
        {
            set { this.principalExchangesSpecified = value; }
            get { return this.principalExchangesSpecified; }
        }
        IPrincipalExchanges IInterestRateStream.PrincipalExchanges
        {
            get { return this.principalExchanges; }
        }
        IRelativeDateOffset IInterestRateStream.CreateRelativeDateOffset() {  return new RelativeDateOffset();  }
        IResetFrequency IInterestRateStream.CreateResetFrequency() { return new ResetFrequency(); } 
        IResetDates IInterestRateStream.CreateResetDates() {  return new ResetDates(); } 
        IInterval IInterestRateStream.CreateInterval() {  return new Interval(); } 
        IOffset IInterestRateStream.CreateOffset() { return new Offset();  }
        IStubCalculationPeriodAmount IInterestRateStream.CreateStubCalculationPeriodAmount() { return new StubCalculationPeriodAmount();  }
        IBusinessDayAdjustments IInterestRateStream.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, string pIdBC)
        {
            BusinessDayAdjustments bda = new BusinessDayAdjustments
            {
                businessDayConvention = pBusinessDayConvention
            };
            if (StrFunc.IsFilled(pIdBC))
            {
                BusinessCenters bcs = new BusinessCenters();
                bcs.Add(new BusinessCenter(pIdBC));
                if (bcs.businessCenter.Length > 0)
                {
                    bda.businessCentersDefineSpecified = true;
                    bda.businessCentersDefine = bcs;
                }
            }
            return bda;
        }
        IBusinessDayAdjustments IInterestRateStream.CreateBusinessDayAdjustments() { return new BusinessDayAdjustments(); }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.AdjustedEffectiveDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = ((IInterestRateStream)this).EffectiveDateAdjustment.adjustedDate.DateValue
                };
                return dt;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.AdjustedTerminationDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = ((IInterestRateStream)this).TerminationDateAdjustment.adjustedDate.DateValue
                };
                return dt;
            }

        }
        /// <summary>
        /// Obtient calculationPeriodAmountCalculation ou calculationPeriodAmountKnownAmountSchedule
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        object IInterestRateStream.GetCalculationPeriodAmount
        {
            get
            {
                object ret = null;
                //
                if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                    ret = calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional;
                else if (calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                    ret = calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule;
                //
                return ret;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.DayCountFraction
        {
            get
            {
                string dayCountFraction = string.Empty;
                if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                    dayCountFraction = this.calculationPeriodAmount.calculationPeriodAmountCalculation.dayCountFraction.ToString();
                else if ((this.calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified) &&
                            (this.calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.dayCountFractionSpecified))
                    dayCountFraction = this.calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.dayCountFraction.ToString();
                return dayCountFraction;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_EventDate IInterestRateStream.EffectiveDate
        {
            get {return new EFS_EventDate(((IInterestRateStream)this).EffectiveDateAdjustment); }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_AdjustableDate IInterestRateStream.EffectiveDateAdjustment
        {
            get
            {
                EFS_CalculationPeriodDates calculationPeriodDates = this.calculationPeriodDates.efs_CalculationPeriodDates;
                EFS_AdjustableDate adjustableDate = calculationPeriodDates.effectiveDateAdjustment;
                return adjustableDate;
            }
        }
        string IInterestRateStream.EventGroupName
        {
            get { return "Stream-"; }
        }
        bool IInterestRateStream.IsCapped
        {
            get
            {
                bool ret = false;
                if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    CalculationBase calculation = calculationPeriodAmount.calculationPeriodAmountCalculation;
                    if (calculation.rateFloatingRateSpecified)
                        ret = calculation.rateFloatingRate.capRateScheduleSpecified;
                }
                return ret;
            }
        }
        bool IInterestRateStream.IsFloored
        {
            get
            {
                bool ret = false;
                if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    CalculationBase calculation = calculationPeriodAmount.calculationPeriodAmountCalculation;
                    if (calculation.rateFloatingRateSpecified)
                        ret = calculation.rateFloatingRate.floorRateScheduleSpecified;
                }
                return ret;
            }
        }
        bool IInterestRateStream.IsCapFloored
        {
            get
            {
                return (((IInterestRateStream)this).IsCapped || ((IInterestRateStream)this).IsFloored);
            }
        }
        bool IInterestRateStream.IsInitialExchange
        {
            get
            {
                bool isInitialExchange = false;
                if (principalExchangesSpecified)
                    isInitialExchange = principalExchanges.initialExchange.BoolValue;
                return (isInitialExchange);
            }
        }
        bool IInterestRateStream.IsIntermediateExchange
        {
            get
            {
                bool isIntermediateExchange = false;
                if (principalExchangesSpecified)
                    isIntermediateExchange = principalExchanges.intermediateExchange.BoolValue;
                return (isIntermediateExchange);
            }
        }
        bool IInterestRateStream.IsFinalExchange
        {
            get
            {
                bool isFinalExchange = false;
                if (principalExchangesSpecified)
                    isFinalExchange = principalExchanges.finalExchange.BoolValue;
                return (isFinalExchange);
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.GetPayerPartyReference
        {
            get {return payerPartyReference.href; }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.GetReceiverPartyReference
        {
            get {return receiverPartyReference.href; }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.StreamCurrency
        {
            get
            {
                object calculationPeriodAmount = ((IInterestRateStream)this).CalculationPeriodAmount;
                Type tCalculationPeriodAmount = calculationPeriodAmount.GetType();
                if (tCalculationPeriodAmount.Equals(typeof(Notional)))
                    return ((Notional)calculationPeriodAmount).notionalStepSchedule.currency.Value;
                else if (tCalculationPeriodAmount.Equals(typeof(FxLinkedNotionalSchedule)))
                    return ((FxLinkedNotionalSchedule)calculationPeriodAmount).currency.Value;
                else if (tCalculationPeriodAmount.Equals(typeof(KnownAmountSchedule)))
                    return ((KnownAmountSchedule)calculationPeriodAmount).currency.Value;
                else if (tCalculationPeriodAmount.Equals(typeof(AmountSchedule)))
                    return ((AmountSchedule)calculationPeriodAmount).currency.Value;
                else
                    return null;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_EventDate IInterestRateStream.TerminationDate
        {
            get {return new EFS_EventDate(((IInterestRateStream)this).TerminationDateAdjustment); }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_AdjustableDate IInterestRateStream.TerminationDateAdjustment
        {
            get
            {
                EFS_CalculationPeriodDates calculationPeriodDates = this.calculationPeriodDates.efs_CalculationPeriodDates;
                EFS_AdjustableDate adjustableDate = calculationPeriodDates.terminationDateAdjustment;
                return adjustableDate;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.UnadjustedEffectiveDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = ((IInterestRateStream)this).EffectiveDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.UnadjustedTerminationDate
        {
            get
            {
                EFS_Date dt = new EFS_Date
                {
                    DateValue = ((IInterestRateStream)this).TerminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;
            }
        }
        string IInterestRateStream.GetCurrency
        {
            get
            {
                string ret = string.Empty;
                if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    if (calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                        ret = calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.currency.Value;
                    else
                        ret = calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.currency.Value;
                }
                return ret;
            }
        }
        string IInterestRateStream.EventType(string pProduct)
        {
            return ((IInterestRateStream)this).EventType2(pProduct, false);
        }
        string IInterestRateStream.EventType2(string pProduct, bool pIsDiscount)
        {
            string eventType = "???";
            if (pIsDiscount)
                eventType = EventTypeFunc.ZeroCoupon;
            else if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
            {
                ICalculation calculation = this.calculationPeriodAmount.calculationPeriodAmountCalculation;
                if (calculation.RateFixedRateSpecified)
                    eventType = EventTypeFunc.FixedRate;
                else if (calculation.RateFloatingRateSpecified)
                    eventType = EventTypeFunc.FloatingRate;
            }
            else if (this.calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                eventType = EventTypeFunc.KnownAmount;
            return eventType;
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        object IInterestRateStream.Rate(string pStub)
        {
            try
            {
                object rate = null;
                if ((StubEnum.None.ToString() == pStub) || (false == this.stubCalculationPeriodAmountSpecified))
                {
                    if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                    {
                        ICalculation calculation = this.calculationPeriodAmount.calculationPeriodAmountCalculation;
                        if (calculation.RateFixedRateSpecified)
                            rate = calculation.RateFixedRate;
                        else if (calculation.RateFloatingRateSpecified)
                            rate = calculation.RateFloatingRate;
                    }
                }
                else if (this.stubCalculationPeriodAmountSpecified)
                {
                    Stub stub = null;
                    if ((StubEnum.Initial.ToString() == pStub) && this.stubCalculationPeriodAmount.initialStubSpecified)
                        stub = this.stubCalculationPeriodAmount.initialStub;
                    else if ((StubEnum.Final.ToString() == pStub) && this.stubCalculationPeriodAmount.finalStubSpecified)
                        stub = this.stubCalculationPeriodAmount.finalStub;

                    // 20080408 EG Ticket : 16165
                    if (null != stub)
                    {
                        if (stub.stubTypeFixedRateSpecified)
                            rate = stub.stubTypeFixedRate;
                        else if (stub.stubTypeFloatingRateSpecified)
                            rate = stub.stubTypeFloatingRate;
                        else if (stub.stubTypeAmountSpecified)
                            rate = stub.stubTypeAmount;
                    }
                }
                return rate;
            }
            catch (Exception) { throw; }
        }
        #endregion IInterestRateStream Members
    }
    #endregion InterestRateStreamBase

    #region MarginRatio
    public partial class MarginRatio : IMarginRatio 
    {
        #region Constructors
        public MarginRatio()
        {
            spreadSchedule = new SpreadSchedule();
            crossMarginRatio = new ActualPrice();
        }
        #endregion Constructors

        #region IMarginRatio Membres
        bool IMarginRatio.SpreadScheduleSpecified
        {
            set { this.spreadScheduleSpecified = value; }
            get { return this.spreadScheduleSpecified; }
        }
        ISpreadSchedule IMarginRatio.SpreadSchedule
        {
            set { this.spreadSchedule = (SpreadSchedule)value; }
            get { return this.spreadSchedule; }
        }
        void IMarginRatio.CreateSpreadMarginRatio(Nullable<decimal> pValue)
        {
            this.spreadScheduleSpecified = pValue.HasValue;
            if (this.spreadScheduleSpecified)
            {
                this.spreadSchedule = new SpreadSchedule
                {
                    initialValue = new EFS_Decimal(pValue.Value)
                };
            }
        }
        bool IMarginRatio.CrossMarginRatioSpecified
        {
            set { this.crossMarginRatioSpecified = value; }
            get { return this.crossMarginRatioSpecified; }
        }
        IActualPrice IMarginRatio.CrossMarginRatio
        {
            set { this.crossMarginRatio = (ActualPrice)value; }
            get { return this.crossMarginRatio; }
        }
        #endregion

        #region IActualPrice Membres
        PriceExpressionEnum IActualPrice.PriceExpression
        {
            set { this.priceExpression = value; }
            get { return this.priceExpression; }
        }
        EFS_Decimal IActualPrice.Amount
        {
            set { this.amount = value; }
            get { return this.amount; }
        }
        bool IActualPrice.CurrencySpecified
        {
            set { this.currencySpecified = value; }
            get { return this.currencySpecified; }
        }
        ICurrency IActualPrice.Currency
        {
            set { this.currency = (Currency)value; }
            get { return this.currency; }
        }
        #endregion IActualPrice Membres
        #region ISpreadSchedule Membres

        ISpreadScheduleType ISpreadSchedule.Type
        {
            get { return this.spreadSchedule.type; }
        }
        void ISpreadSchedule.CreateSpreadScheduleType(string pValue)
        {
            this.spreadSchedule.type = new SpreadScheduleType(pValue); 
        }
        #endregion
        #region ISchedule Membres
        EFS_Decimal ISchedule.InitialValue
        {
            set { this.spreadSchedule.initialValue = value; }
            get { return this.spreadSchedule.initialValue; }
        }
        bool ISchedule.StepSpecified
        {
            get { return this.spreadSchedule.stepSpecified; }
        }
        IStep[] ISchedule.Step
        {
            get { return this.spreadSchedule.step; }
        }
        EFS_Step[] ISchedule.Efs_Steps
        {
            get { return this.spreadSchedule.efs_Steps; }
        }
        bool ISchedule.IsStepCalculated
        {
            get { return this.spreadSchedule.IsStepCalculated; }
        }
        DateTime[] ISchedule.GetStepDatesValue
        {
            get { return this.spreadSchedule.GetStepDatesValue(); }
        }
        string ISchedule.Id
        {
            set { this.spreadSchedule.Id = value; }
            get { return this.spreadSchedule.Id; }
        }

        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel ISchedule.CalcAdjustableSteps(string pCs, IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            return this.spreadSchedule.CalcAdjustableSteps(pCs, pBusinessDayAdjustments, pDataDocument);
        }

        #endregion
    }
    #endregion MarginRatio
    #region PartyPayerReceiverReference
    public partial class PartyPayerReceiverReference : IPartyPayerReceiverReference,IEFS_Array
    {
        #region Constructors
        public PartyPayerReceiverReference()
        {
            partyReferencePayer = new PartyReference();
            partyReferenceReceiver = new PartyReference();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods

        #region IPartyPayerReceiverReference Members
        bool IPartyPayerReceiverReference.PayerPartyReferenceSpecified
        {
            get { return this.partyReferencePayerSpecified; }
            set { this.partyReferencePayerSpecified = value; }
        }

        IReference IPartyPayerReceiverReference.PayerPartyReference
        {
            get {return this.partyReferencePayer; }
            set {this.partyReferencePayer = (PartyReference) value; }
        }

        bool IPartyPayerReceiverReference.ReceiverPartyReferenceSpecified
        {
            get { return this.partyReferenceReceiverSpecified; }
            set { this.partyReferenceReceiverSpecified = value; }
        }

        IReference IPartyPayerReceiverReference.ReceiverPartyReference
        {
            get { return this.partyReferenceReceiver; }
            set { this.partyReferenceReceiver = (PartyReference)value; }
        }

        #endregion
    }
    #endregion PartyPayerReceiverReference

	#region PaymentQuote
	public partial class PaymentQuote : IPaymentQuote
	{
		#region Constructors
		public PaymentQuote() { }
		#endregion Constructors
		#region Methods
		#region InitializePercentageRateFromPercentageRateFraction
		public void InitializePercentageRateFromPercentageRateFraction()
		{
			percentageRate.Value = "0";
			if (percentageRateFractionSpecified)
			{
				Fraction fraction = new Fraction(percentageRateFraction.Value, true);
				percentageRate.Value = StrFunc.FmtDecimalToInvariantCulture(fraction.DecValue());
			}
		}
		#endregion InitializPercentageRateFromPercentageRateFraction
		#endregion Methods

		#region IPaymentQuote Members
		bool IPaymentQuote.PercentageRateFractionSpecified
		{
			set {this.percentageRateFractionSpecified = value;}
			get { return this.percentageRateFractionSpecified; }
		}
		string IPaymentQuote.PercentageRateFraction
		{
			set	{this.percentageRateFraction = new EFS_String(value);}
			get 
			{ 
				if (this.percentageRateFractionSpecified)
					return this.percentageRateFraction.Value;
				else
					return null;
			}
		}

		EFS_Decimal IPaymentQuote.PercentageRate
		{
			set { this.percentageRate = value; }
			get { return this.percentageRate; }
		}

		IReference IPaymentQuote.PaymentRelativeTo
		{
			set { this.paymentRelativeTo = (AmountReference)value; }
			get { return this.paymentRelativeTo; }
		}
		void IPaymentQuote.InitializePercentageRateFromPercentageRateFraction()
		{
			this.InitializePercentageRateFromPercentageRateFraction();
		}
		#endregion IPaymentQuote Members
	}
	#endregion PaymentQuote

	#region PriceReference
	public partial class PriceReference : IReference
	{
		#region IReference Members
		string IReference.HRef
		{
			set { this.href = value; }
			get { return this.href; }
		}
		#endregion IReference Members
	}
	#endregion ProductReference

    #region RoutingCreateElement
    public partial class RoutingCreateElement : IRoutingCreateElement
    {
        #region IRoutingCreateElement Membres
        IRouting IRoutingCreateElement.CreateRouting()
        {
            return new Routing();
        }
        IRoutingIds[] IRoutingCreateElement.CreateRoutingIds(IRoutingId[] pRoutingId)
        {
            RoutingIds[] routingIds = new RoutingIds[1] { new RoutingIds() };
            RoutingId[] routingId = new RoutingId[pRoutingId.Length];
            for (int i = 0; i < pRoutingId.Length; i++)
            {
                routingId[i] = (RoutingId)pRoutingId[i];
                routingId[i] = new RoutingId
                {
                    routingIdCodeScheme = pRoutingId[i].RoutingIdCodeScheme,
                    Value = pRoutingId[i].Value
                };
            }
            routingIds[0].routingId = routingId;
            return routingIds;
        }
        IRoutingIdsAndExplicitDetails IRoutingCreateElement.CreateRoutingIdsAndExplicitDetails()
        {
            return new RoutingIdsAndExplicitDetails();
        }
        IRoutingId IRoutingCreateElement.CreateRoutingId()
        {
            return new RoutingId();
        }
        IAddress IRoutingCreateElement.CreateAddress()
        {
            return new Address();
        }
        IStreetAddress IRoutingCreateElement.CreateStreetAddress()
        {
            return new StreetAddress();
        }
        IScheme IRoutingCreateElement.CreateCountry()
        {
            return new Country();
        }
        #endregion
    }
    #endregion 
    #region RoutingPartyReference
    public partial class RoutingPartyReference : IRoutingPartyReference
    {
        #region IRoutingPartyReference Membres
        bool IRoutingPartyReference.HRefSpecified
        {
            get { return this.hrefSpecified; }
            set { this.hrefSpecified = value; }
        }
        string IRoutingPartyReference.HRef
        {
            get { return this.href; }
            set { this.href = value; }
        }
        #endregion
    }
    #endregion

    #region SoftApplication
    public partial class SoftApplication : ISoftApplication
    {
        #region constructor
        public SoftApplication()
        {
            this.name = new EFS_String();
            this.version = new EFS_String();
        }
        #endregion

        #region ISoftApplication Membres
        EFS_String ISoftApplication.Name
        {
            get { return this.name; }
            set { name = value; }
        }
        EFS_String ISoftApplication.Version
        {
            get { return this.version; }
            set { version = value; }
        }
        #endregion
    }
    #endregion

    #region SpheresId
    /// EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
    public partial class SpheresId : ISpheresIdSchemeId, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string Id
        {
            set { efs_id = new EFS_Id(value); }
            get
            {
                if (efs_id == null)
                    return null;
                else
                    return efs_id.Value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public SpheresId()
        {
            scheme = string.Empty;
            Value = string.Empty;
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
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
        #endregion IBookId Members
        #region ISchemeId Members
        string ISchemeId.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
        // EG 20220902 [XXXXX][WI415] UTI/PUTI Enhancement (declaration source sur ISchemeId)
        string ISchemeId.Source
        {
            set;
            get;
        }
        #endregion ISchemeId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.scheme = value; }
            get { return this.scheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion SpheresId
    #region TradeExtend
    public partial class TradeExtend : ITradeExtend, IEFS_Array
    {
        #region Accessors
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Accessors
        #region Constructors
        public TradeExtend()
        {
            scheme = string.Empty;
            Value = string.Empty;
            //hRefSpecified = false;
        }
        #endregion Constructors
        #region ITradeExtend Members
        bool ITradeExtend.HRefSpecified
        {
            get { return this.hRefSpecified; }
            set { this.hRefSpecified = value; }
        }
        string ITradeExtend.HRef
        {
            set { this.hRef = value; }
            get { return this.hRef; }
        }
        #endregion ITradeExtend Members
        
        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
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
        #endregion IBookId Members
        #region IScheme Members
        string IScheme.Scheme
        {
            set { this.scheme = value; }
            get { return this.scheme; }
        }
        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }
        #endregion IScheme Members
    }
    #endregion SpheresId
    #region SpheresSource
    public partial class SpheresSource : ISpheresSource
    {
        #region Constructors
        public SpheresSource()
        {
        }
        #endregion Constructors

        #region ISpheresSource Members
        bool ISpheresSource.StatusSpecified
        {
            get { return this.statusSpecified; }
            set {this.statusSpecified=value;}
        }
        SpheresSourceStatusEnum ISpheresSource.Status
        {
            get { return this.status; }
            set { this.status = value; }
        }
        ISpheresIdSchemeId[] ISpheresSource.SpheresId
        {
            get {return this.spheresId;}
            set {this.spheresId = (SpheresId[])value;}
        }
        ISpheresIdSchemeId ISpheresSource.GetSpheresIdFromScheme(string pScheme)
        {
            return (ISpheresIdSchemeId)Tools.GetScheme(this.spheresId, pScheme);
        }
        ISpheresIdSchemeId[] ISpheresSource.GetSpheresIdLikeScheme(string pScheme)
        {
            return (ISpheresIdSchemeId[])Tools.GetLikeScheme(this.spheresId, pScheme);
        }
        ISpheresIdSchemeId ISpheresSource.GetSpheresIdFromSchemeId(string pSchemeId)
        {
            return (ISpheresIdSchemeId)Tools.GetSchemeById(this.spheresId, pSchemeId);
        }
        #endregion ISpheresSource Members        
    }
    #endregion SpheresSource
    #region Tax
    public partial class Tax : ITax,IEFS_Array
    {
        #region Constructors
        public Tax()
        {
            taxSource = new SpheresSource();
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region ITax Members
        ISpheresSource ITax.TaxSource
        {
            set { this.taxSource = (SpheresSource)value; }
            get { return this.taxSource; }
        }
        ITaxSchedule[] ITax.TaxDetail
        {
            set { this.taxDetail = (TaxSchedule[])value; }
            get { return this.taxDetail; }
        }
        #endregion ITax Members
    }
    #endregion TaxSchedule
    #region TaxSchedule
    public partial class TaxSchedule : ITaxSchedule, IEFS_Array
    {
        #region Constructors
        public TaxSchedule()
        {
        }
        #endregion Constructors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion IEFS_Array Members
        #region ITaxSchedule Members
        bool ITaxSchedule.TaxAmountSpecified
        {
            set { this.taxAmountSpecified = value; }
            get { return this.taxAmountSpecified; }
        }
        ITripleInvoiceAmounts ITaxSchedule.TaxAmount
        {
            set { this.taxAmount = (TripleInvoiceAmounts)value; }
            get { return this.taxAmount; }
        }
        ISpheresSource ITaxSchedule.TaxSource
        {
            set { this.taxSource = (SpheresSource)value; }
            get { return this.taxSource; }
        }
        ITripleInvoiceAmounts ITaxSchedule.CreateTripleInvoiceAmounts 
        { 
            get { return new TripleInvoiceAmounts(); }
            
        }
        IMoney ITaxSchedule.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        object ITaxSchedule.Clone()
        {
            int nbSource = taxSource.spheresId.Length;
            TaxSchedule clone = new TaxSchedule
            {
                taxAmountSpecified = taxAmountSpecified,
                taxSource = new SpheresSource()
                {
                    statusSpecified = taxSource.statusSpecified,
                    spheresId = new SpheresId[nbSource]
                },
                taxAmount = new TripleInvoiceAmounts()
                { 
                    amount = new Money(taxAmount.amount.amount.DecValue, taxAmount.amount.Currency),
                    issueAmountSpecified = taxAmount.issueAmountSpecified,
                    accountingAmountSpecified = taxAmount.accountingAmountSpecified,
                },
            };
            if (clone.taxAmount.issueAmountSpecified)
                clone.taxAmount.issueAmount = new Money(taxAmount.issueAmount.amount.DecValue, taxAmount.issueAmount.Currency);
            if (clone.taxAmount.accountingAmountSpecified)
                clone.taxAmount.accountingAmount = new Money(taxAmount.accountingAmount.amount.DecValue, taxAmount.accountingAmount.Currency);
            if (clone.taxSource.statusSpecified)
                clone.taxSource.status = taxSource.status;
            for (int i = 0; i < nbSource; i++)
            {
                clone.taxSource.spheresId[i] = new SpheresId
                {
                    otcmlId = taxSource.spheresId[i].otcmlId,
                    scheme = taxSource.spheresId[i].scheme,
                    Value = taxSource.spheresId[i].Value
                };
            }
            return clone;
        }
        string ITaxSchedule.GetEventType()
        {
            string eventType = string.Empty;
            ISpheresSource source = (ISpheresSource)taxSource;
            ISpheresIdSchemeId spheresIdScheme = source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme);
            if (null != spheresIdScheme)
                eventType = source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailEventTypeScheme).Value;
            return eventType;
        }
        decimal ITaxSchedule.GetRate()
        {
            decimal rate = 0;
            ISpheresSource source = (ISpheresSource)taxSource;
            ISpheresIdSchemeId spheresIdScheme = source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme);
            if (null != spheresIdScheme)
                rate = DecFunc.DecValueFromInvariantCulture(source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme).Value);
            return rate;
        }
        decimal ITaxSchedule.GetTaxAmount(string pCs,decimal pBaseAmount,string pCurrency)
        {
            decimal rate = 0;
            ISpheresSource source = (ISpheresSource)taxSource;
            ISpheresIdSchemeId spheresIdScheme = source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme);
            if (null != spheresIdScheme)
                rate = DecFunc.DecValueFromInvariantCulture(source.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme).Value);

            EFS_Cash cash = new EFS_Cash(pCs, pBaseAmount * rate, pCurrency);
            return cash.AmountRounded;
        }
        #endregion ITaxSchedule Members
    }
    #endregion TaxSchedule

    #region TradeExtends
    public partial class TradeExtends : ITradeExtends
    {
        #region Constructors
        public TradeExtends()
        {
        }
        #endregion Constructors

        #region ITradeExtends Members
        ITradeExtend[] ITradeExtends.TradeExtend
        {
            get { return this.tradeExtend; }
            set { this.tradeExtend = (TradeExtend[])value; }
        }
        ITradeExtend ITradeExtends.GetSpheresIdFromScheme(int pOTCmlId)
        {
            return (ITradeExtend)Tools.GetSchemeByOTCmlId(this.tradeExtend, pOTCmlId);
        }
        ITradeExtend ITradeExtends.GetSpheresIdFromScheme2(string pScheme)
        {
            return (ITradeExtend)Tools.GetScheme(this.tradeExtend, pScheme);
        }
        #endregion ITradeExtends Members
    }
    #endregion TradeExtends

    #region TripleInvoiceAmounts
    public partial class TripleInvoiceAmounts : ITripleInvoiceAmounts
    {
        #region Constructors
        public TripleInvoiceAmounts()
        {
            amount = new Money();
            issueAmount = new Money();
            accountingAmount = new Money();
        }
        #endregion Constructors

        #region ITripleInvoiceAmounts Members
        IMoney ITripleInvoiceAmounts.Amount
        {
            set { amount = (Money)value; }
            get { return amount; }
        }
        bool ITripleInvoiceAmounts.IssueAmountSpecified
        {
            set { issueAmountSpecified = value; }
            get { return issueAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.IssueAmount
        {
            set { issueAmount = (Money)value; }
            get { return issueAmount; }
        }
        bool ITripleInvoiceAmounts.AccountingAmountSpecified
        {
            set { accountingAmountSpecified = value; }
            get { return accountingAmountSpecified; }
        }
        IMoney ITripleInvoiceAmounts.AccountingAmount
        {
            set { accountingAmount = (Money)value; }
            get { return accountingAmount; }
        }
        IMoney ITripleInvoiceAmounts.CreateMoney(decimal pAmount, string pCurrency) { return new Money(pAmount, pCurrency); }
        #endregion ITripleInvoiceAmounts Members
    }
    #endregion TripleInvoiceAmounts

    #region ZonedDateTime
    /// EG 20171004 [23452] new
    // EG 20171031 [23509] Upd
    public partial class ZonedDateTime : IZonedDateTime
    {
        #region constructor
        public ZonedDateTime()
        {
            this._date = new EFS_DateTimeOffset();
            this.zonedDateTimeScheme = "http://www.iana.org/time-zones";
            this.efs_id = new EFS_Id();
        }
        #endregion

        #region IZonedDateTime Members
        string IZonedDateTime.Efs_id
        {
            set { this.efs_id.Value = value; }
            get { return this.efs_id.Value; }
        }

        string IScheme.Value
        {
            set { this.Value = value; }
            get { return this.Value; }
        }

        string IScheme.Scheme
        {
            set { this.zonedDateTimeScheme = value; }
            get { return this.zonedDateTimeScheme; }
        }
        Nullable<DateTimeOffset> IZonedDateTime.DateTimeOffsetValue
        {
            get { return Tz.Tools.ToDateTimeOffset(this.Value); }
        }
        string IZonedDateTime.Tzdbid
        {
            set { this.tzdbid = value; }
            get { return this.tzdbid; }
        }
        #endregion IZonedDateTime Members
    }
    #endregion ZonedDateTime
    
}
