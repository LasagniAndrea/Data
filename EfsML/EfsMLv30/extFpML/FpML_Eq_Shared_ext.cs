#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.v30.Option.Shared;
using EfsML.v30.Shared;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Eq.Shared
{
    #region AdditionalPaymentAmount
    public partial class AdditionalPaymentAmount : IAdditionalPaymentAmount
	{
        //PL 20141003 Add constructor
		#region Constructors
        public AdditionalPaymentAmount()
		{
			this.paymentAmount = new Money();
		}
		#endregion Constructors

        #region IAdditionalPaymentAmount Members
		bool IAdditionalPaymentAmount.PaymentAmountSpecified
		{
			set {this.paymentAmountSpecified = value;}
			get {return this.paymentAmountSpecified;}
		}
		IMoney IAdditionalPaymentAmount.PaymentAmount
		{
			get {return this.paymentAmount; }
		}
		bool IAdditionalPaymentAmount.FormulaSpecified
		{
			get {return this.formulaSpecified; }
		}
		#endregion IAdditionalPaymentAmount Members
	}
	#endregion AdditionalPaymentAmount
	#region AdjustableDateOrRelativeDateSequence
    // EG 20140702 Update Interface
	public partial class AdjustableDateOrRelativeDateSequence : IAdjustableDateOrRelativeDateSequence
	{
		#region Constructors
		public AdjustableDateOrRelativeDateSequence()
		{
			adjustableOrRelativeDateSequenceAdjustableDate = new AdjustableDate();
			adjustableOrRelativeDateSequenceRelativeDateSequence = new RelativeDateSequence();
		}
		#endregion Constructors

		#region IAdjustableDateOrRelativeDateSequence Members
		bool IAdjustableDateOrRelativeDateSequence.AdjustableDateSpecified
		{
			set { this.adjustableOrRelativeDateSequenceAdjustableDateSpecified = value; }
			get { return this.adjustableOrRelativeDateSequenceAdjustableDateSpecified; }
		}
		IAdjustableDate IAdjustableDateOrRelativeDateSequence.AdjustableDate
		{
			get { return this.adjustableOrRelativeDateSequenceAdjustableDate; }
		}
		bool IAdjustableDateOrRelativeDateSequence.RelativeDateSequenceSpecified
		{
            set { this.adjustableOrRelativeDateSequenceRelativeDateSequenceSpecified = value; }
			get { return this.adjustableOrRelativeDateSequenceRelativeDateSequenceSpecified; }
		}
		IRelativeDateSequence IAdjustableDateOrRelativeDateSequence.RelativeDateSequence
		{
			get { return this.adjustableOrRelativeDateSequenceRelativeDateSequence; }
		}
		#endregion IAdjustableDateOrRelativeDateSequence Members
	}
	#endregion AdjustableDateOrRelativeDateSequence

    #region Compounding
    public partial class Compounding : ICompounding
    {
        #region ICompounding Members
        bool ICompounding.CompoundingMethodSpecified
        {
            set { this.compoundingMethodSpecified = value; }
            get { return this.compoundingMethodSpecified; }
        }
        CompoundingMethodEnum ICompounding.CompoundingMethod
        {
            set { this.compoundingMethod = value; }
            get { return this.compoundingMethod; }
        }
        ICompoundingRate ICompounding.CompoundingRate
        {
            set { this.compoundingRate = (CompoundingRate)value; }
            get { return this.compoundingRate; }
        }
        bool ICompounding.CompoundingSpreadSpecified
        {
            set { this.compoundingSpreadSpecified = value; }
            get { return this.compoundingSpreadSpecified; }
        }
        decimal ICompounding.CompoundingSpread
        {
            set { this.compoundingSpread = new EFS_Decimal(value); }
            get { return this.compoundingSpread.DecValue; }
        }

        #endregion
    }
    #endregion Compounding

    #region CompoundingRate
    public partial class CompoundingRate :ICompoundingRate
    {
        #region Constructors
        public CompoundingRate()
        {
            typeInterestLegRate = new InterestCalculationReference();
            typeSpecificRate = new InterestAccrualsMethod();
        }
        #endregion Constructors
        #region ICompounding Members
        bool ICompoundingRate.TypeInterestLegRateSpecified
        {
            set { this.typeInterestLegRateSpecified = value; }
            get { return this.typeInterestLegRateSpecified; }
        }
        IReference ICompoundingRate.TypeInterestLegRate
        {
            set { this.typeInterestLegRate = (InterestCalculationReference) value; }
            get { return this.typeInterestLegRate; }
        }
        bool ICompoundingRate.TypeSpecificRateSpecified
        {
            set { this.typeSpecificRateSpecified = value; }
            get { return this.typeSpecificRateSpecified; }
        }
        IInterestAccrualsMethod ICompoundingRate.TypeSpecificRate
        {
            set { this.typeSpecificRate = (InterestAccrualsMethod) value; }
            get { return this.typeSpecificRate; }
        }

        #endregion

    }
    #endregion CompoundingRate

	#region DividendAdjustment
	public partial class DividendAdjustment : IDividendAdjustment
	{
		#region IDividendAdjustment Members
		IDividendPeriodDividend[] IDividendAdjustment.DividendPeriodDividend
		{
			set { this.dividendPeriod = (DividendPeriodDividend[])value; }
			get { return this.dividendPeriod; }
		}

		#endregion
	}
	#endregion DividendAdjustment

    #region DividendPeriod
	public abstract partial class DividendPeriod : IDividendPeriod 
    {
		#region IDividendPeriod Members
		IAdjustedDate IDividendPeriod.UnadjustedStartDate
		{
			set { this.unadjustedStartDate = (IdentifiedDate)value; }
			get { return this.unadjustedStartDate; }
		}
		IAdjustedDate IDividendPeriod.UnadjustedEndDate
		{
			set { this.unadjustedEndDate = (IdentifiedDate)value; }
			get { return this.unadjustedEndDate; }
		}
		IBusinessDayAdjustments IDividendPeriod.DateAdjustments
		{
			set { this.dateAdjustments = (BusinessDayAdjustments)value; }
			get { return this.dateAdjustments; }
		}
		IReference IDividendPeriod.UnderlyerReference
		{
			set { this.underlyerReference = (AssetReference)value; }
			get { return this.underlyerReference; }
		}
		#endregion IDividendPeriod Members

	}
    #endregion DividendPeriod
	#region DividendPeriodDividend
	public partial class DividendPeriodDividend : IDividendPeriodDividend, IEFS_Array
	{
		#region IDividendPeriodDividend Members
		IMoney IDividendPeriodDividend.Dividend
		{
			set { this.dividend = (Money)value; }
			get { return this.dividend; }
		}
		EFS_Decimal IDividendPeriodDividend.Multiplier
		{
			set { this.multiplier = value; }
			get { return this.multiplier; }
		}
		#endregion IDividendPeriodDividend Members
		#region IDividendPeriod Members
		IAdjustedDate IDividendPeriod.UnadjustedStartDate
		{
			set { this.unadjustedStartDate = (IdentifiedDate)value; }
			get { return this.unadjustedStartDate; }
		}
		IAdjustedDate IDividendPeriod.UnadjustedEndDate
		{
			set { this.unadjustedEndDate = (IdentifiedDate)value; }
			get { return this.unadjustedEndDate; }
		}
		IBusinessDayAdjustments IDividendPeriod.DateAdjustments
		{
			set { this.dateAdjustments = (BusinessDayAdjustments)value; }
			get { return this.dateAdjustments; }
		}
		IReference IDividendPeriod.UnderlyerReference
		{
			set { this.underlyerReference = (AssetReference)value; }
			get { return this.underlyerReference; }
		}
		#endregion IDividendPeriod Members

		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
	}
	#endregion DividendPeriodDividend

	#region EquityPremium
    /// EG 20150422 [20513] BANCAPERTA New  IPremiumBase
    public partial class EquityPremium : IEquityPremium, IPremiumBase
	{
		#region Constructors
		public EquityPremium()
		{
			percentageOfNotional = new EFS_Decimal();
		}
		#endregion Constructors

		#region IEquityPremium Members
		IReference IEquityPremium.PayerPartyReference
		{
			set { this.payerPartyReference = (PartyOrAccountReference)value; }
			get { return this.payerPartyReference; }
		}
		IReference IEquityPremium.ReceiverPartyReference
		{
			set { this.receiverPartyReference = (PartyOrAccountReference)value; }
			get { return this.receiverPartyReference; }
		}
		bool IEquityPremium.PremiumTypeSpecified
		{
			set { this.premiumTypeSpecified = value; }
			get { return this.premiumTypeSpecified; }
		}
		PremiumTypeEnum IEquityPremium.PremiumType
		{
			set { this.premiumType = value; }
			get { return this.premiumType; }
		}
		bool IEquityPremium.PaymentAmountSpecified
		{
			set { this.paymentAmountSpecified = value; }
			get { return this.paymentAmountSpecified; }
		}
		IMoney IEquityPremium.PaymentAmount
		{
			set { this.paymentAmount = (Money)value; }
			get { return this.paymentAmount; }
		}
		bool IEquityPremium.PaymentDateSpecified
		{
			set { this.paymentDateSpecified = value; }
			get { return this.paymentDateSpecified; }
		}
		IAdjustableDate IEquityPremium.PaymentDate
		{
			set { this.paymentDate = (AdjustableDate)value; }
			get { return this.paymentDate; }
		}
		bool IEquityPremium.SwapPremiumSpecified
		{
			set { this.swapPremiumSpecified = value; }
			get { return this.swapPremiumSpecified; }
		}
		EFS_Boolean IEquityPremium.SwapPremium
		{
			set { this.swapPremium = value; }
			get { return this.swapPremium; }
		}
		bool IEquityPremium.PricePerOptionSpecified
		{
			set { this.pricePerOptionSpecified = value; }
			get { return this.pricePerOptionSpecified; }
		}
		IMoney IEquityPremium.PricePerOption
		{
			set { this.pricePerOption = (Money)value; }
			get { return this.pricePerOption; }
		}
		bool IEquityPremium.PercentageOfNotionalSpecified
		{
			set { this.percentageOfNotionalSpecified = value; }
			get { return this.percentageOfNotionalSpecified; }
		}
		EFS_Decimal IEquityPremium.PercentageOfNotional
		{
			set { this.percentageOfNotional = value; }
			get { return this.percentageOfNotional; }
		}
		#endregion IEquityPremium Members

        #region IPremiumBase Membres
        IReference IPremiumBase.PayerPartyReference
        {
            get { return this.payerPartyReference; }
        }
        IReference IPremiumBase.ReceiverPartyReference
        {
            get { return this.receiverPartyReference; }
        }

        bool IPremiumBase.PremiumTypeSpecified
        {
            get { return this.premiumTypeSpecified; }
        }

        PremiumTypeEnum IPremiumBase.PremiumType
        {
            get { return this.premiumType; }
        }

        bool IPremiumBase.PricePerOptionSpecified
        {
            get { return this.pricePerOptionSpecified; }
        }

        IMoney IPremiumBase.PricePerOption
        {
            get { return this.pricePerOption; }
        }

        bool IPremiumBase.PercentageOfNotionalSpecified
        {
            get { return this.percentageOfNotionalSpecified; }
        }

        EFS_Decimal IPremiumBase.PercentageOfNotional
        {
            get { return this.percentageOfNotional; }
        }

        bool IPremiumBase.PaymentAmountSpecified
        {
            get { return true; }
        }

        IMoney IPremiumBase.PaymentAmount
        {
            get { return this.paymentAmount; }
        }
        #endregion
	}
	#endregion EquityPremium
	#region EquityStrike
	public partial class EquityStrike : IEquityStrike
	{
		#region Constructors
		public EquityStrike()
		{
			typeStrikePrice = new EFS_Decimal();
			typeStrikePercentage = new EFS_Decimal();
		}
		#endregion Constructors

		#region IEquityStrike Members
		bool IEquityStrike.PriceSpecified
		{
			set { this.typeStrikePriceSpecified = value; }
			get { return this.typeStrikePriceSpecified; }
		}
		EFS_Decimal IEquityStrike.Price
		{
			set { this.typeStrikePrice = value; }
			get { return this.typeStrikePrice; }
		}
		bool IEquityStrike.PercentageSpecified
		{
			set { this.typeStrikePercentageSpecified = value; }
			get { return this.typeStrikePercentageSpecified; }
		}
		EFS_Decimal IEquityStrike.Percentage
		{
			set { this.typeStrikePercentage = value; }
			get { return this.typeStrikePercentage; }
		}
		bool IEquityStrike.StrikeDeterminationDateSpecified
		{
			set { this.strikeDeterminationDateSpecified = value; }
			get { return this.strikeDeterminationDateSpecified; }
		}
		IAdjustableOrRelativeDate IEquityStrike.StrikeDeterminationDate
		{
			set { this.strikeDeterminationDate = (AdjustableOrRelativeDate)value; }
			get { return this.strikeDeterminationDate; }
		}
		bool IEquityStrike.CurrencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}
		ICurrency IEquityStrike.Currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		#endregion IEquityStrike Members
	}
	#endregion EquityStrike
	#region EquityValuation
    // EG 20140702 Update Interface
    public partial class EquityValuation : IEquityValuation
	{
		#region IEquityValuation Members
		bool IEquityValuation.ValuationDateSpecified
		{
            set { this.valuationDateSpecified = value; }
			get { return this.valuationDateSpecified; }
		}
		IAdjustableDateOrRelativeDateSequence IEquityValuation.ValuationDate
		{
			get { return this.valuationDate; }
		}
		bool IEquityValuation.ValuationDatesSpecified
		{
			set { this.valuationDatesSpecified = value; }
			get { return this.valuationDatesSpecified; }
		}
		IAdjustableRelativeOrPeriodicDates IEquityValuation.ValuationDates
		{
			set { this.valuationDates = (AdjustableRelativeOrPeriodicDates)value; }
			get { return this.valuationDates; }
		}
		bool IEquityValuation.ValuationTimeTypeSpecified
		{
			get { return this.valuationTimeTypeSpecified; }
		}
		TimeTypeEnum IEquityValuation.ValuationTimeType
		{
			get { return this.valuationTimeType; }
		}
		bool IEquityValuation.ValuationTimeSpecified
		{
			get { return this.valuationTimeSpecified; }
		}
		IBusinessCenterTime IEquityValuation.ValuationTime
		{
			get { return this.valuationTime; }
		}
		bool IEquityValuation.FuturesPriceValuationSpecified
		{
			get { return this.futuresPriceValuationSpecified; }
		}
		EFS_Boolean IEquityValuation.FuturesPriceValuation
		{
			get { return this.futuresPriceValuation; }
		}
		bool IEquityValuation.OptionsPriceValuationSpecified
		{
			get { return this.optionsPriceValuationSpecified; }
		}
		EFS_Boolean IEquityValuation.OptionsPriceValuation
		{
			get { return this.optionsPriceValuation; }
		}
		#endregion IEquityValuation Members
	}
	#endregion EquityValuation
	#region ExtraordinaryEvents
	public partial class ExtraordinaryEvents : IExtraordinaryEvents
	{
		#region Constructors
		public ExtraordinaryEvents()
		{
			itemFailureToDeliver = new EFS_Boolean();
			itemAdditionalDisruptionEvents = new AdditionalDisruptionEvents();
		}
		#endregion Constructors

		#region IExtraordinaryEvents Members
		bool IExtraordinaryEvents.MergerEventsSpecified
		{
			get { return this.mergerEventsSpecified; }
		}
		#endregion IExtraordinaryEvents Members
	}
	#endregion ExtraordinaryEvents


	#region InterestCalculation
	public partial class InterestCalculation : IInterestCalculation
	{
		#region IInterestCalculation Members
		bool IInterestCalculation.CompoundingSpecified
		{
			get {return this.compoundingSpecified;}
		}
        ICompounding IInterestCalculation.Compounding
        {
            get { return this.compounding; }
        }
        DayCountFractionEnum IInterestCalculation.DayCountFraction
		{
			set { this.dayCountFraction = value; }
			get {return this.dayCountFraction;}
		}
		#endregion IInterestCalculation Members
	}
	#endregion InterestCalculation
    #region InterestCalculationReference
    public partial class InterestCalculationReference : IReference
    {
        #region IReference Members
        string IReference.HRef
        {
            set { this.href = value; }
            get { return this.href; }
        }
		#endregion IReference Members
	}
	#endregion InterestCalculationReference

	#region InterestLeg
	// EG 20140702 New build FpML4.4 Change Type of interestLegPaymentDates (AdjustableRelativeOrPeriodicDates2)
	// EG 20140702 Add Accessors | Upd Interface
	// EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis
	public partial class InterestLeg : IEFS_Array, IInterestLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_InterestLeg efs_InterestLeg;
		#endregion Members

        #region Accessors
        #region DayCountFraction
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public DayCountFractionEnum DayCountFraction
        {
            get { return this.interestCalculation.dayCountFraction;}
        }
        #endregion DayCountFraction
        #region Asset
        [System.Xml.Serialization.XmlIgnoreAttribute()]

        public EFS_Asset Asset
        {
            get {return efs_InterestLeg.Asset;}
        }
        #endregion Asset
        #region AdjustedEffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedEffectiveDate
        {
            get
            {
				EFS_Date dt = new EFS_Date
				{
					DateValue = EffectiveDateAdjustment.adjustedDate.DateValue
				};
				return dt;
            }
        }
        #endregion AdjustedEffectiveDate
        #region AdjustedTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedTerminationDate
        {
            get
            {
				EFS_Date dt = new EFS_Date
				{
					DateValue = TerminationDateAdjustment.adjustedDate.DateValue
				};
				return dt;
            }
        }
        #endregion AdjustedTerminationDate
        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate EffectiveDate
        {
            get {return new EFS_EventDate(EffectiveDateAdjustment); }
        }
        #endregion EffectiveDate
        #region EffectiveDateAdjustment
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate EffectiveDateAdjustment
        {
            get
            {
                try { return efs_InterestLeg.effectiveDateAdjustment; }
                catch (Exception) { throw; }
            }
        }
        #endregion EffectiveDateAdjustment        

        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate TerminationDate
        {
            get {return new EFS_EventDate(TerminationDateAdjustment);}
        }
        #endregion TerminationDate
        #region TerminationDateAdjustment
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate TerminationDateAdjustment
        {
            get {return efs_InterestLeg.terminationDateAdjustment; }
        }
        #endregion TerminationDateAdjustment

        #region PayerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
        {
            get {return payerPartyReference.href; }
        }
        #endregion PayerPartyReference
        #region ReceiverPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
        {
            get {return receiverPartyReference.href; }
        }
        #endregion ReceiverPartyReference
        #region LegEventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string LegEventCode
        {
            get
            {
                return EventCodeFunc.InterestLeg;
            }
        }
        #endregion LegEventCode
        #region LegEventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string LegEventType
        {
            get
            {
                string eventType = EventTypeFunc.FloatingRate;
                if (this.interestCalculation.rateFixedRateSpecified)
                    eventType = EventTypeFunc.FixedRate;
                return eventType;
            }
        }
        #endregion LegEventType

        #region IsPeriodRelativeToReturnLeg
        /// <summary>
        /// interestLegPaymentDates relativeTO interimValuationDate
        /// </summary>
        public bool IsPeriodRelativeToReturnLeg
        {
            get
            {
                return interestLegCalculationPeriodDates.interestLegPaymentDates.dtType_RelativeDatesSpecified &&
                       (interestLegCalculationPeriodDates.interestLegPaymentDates.dtType_RelativeDates.dateRelativeTo.href == "interimValuationDate");
            }
        }
		#endregion IsPeriodRelativeToReturnLeg
		// EG 20231127 [WI755] Implementation Return Swap : Add New Properties
		public bool IsFunding { get { return efs_InterestLeg.IsFunding; } }
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public string PaymentCurrency { get { return efs_InterestLeg.paymentCurrency; } }
		#endregion Accessors
		#region Methods
		#region Rate
		public object Rate(string pStub)
        {
            object rate = null;
            if ((StubEnum.None.ToString() == pStub) || (false == this.stubCalculationPeriodSpecified))
            {
                if (this.interestCalculation.rateFixedRateSpecified)
                    rate = this.interestCalculation.rateFixedRate;
                else if (this.interestCalculation.rateFloatingRateSpecified)
                    rate = this.interestCalculation.rateFloatingRate;
            }
            else if (this.stubCalculationPeriodSpecified)
            {
                IStub stub = null;
                if ((StubEnum.Initial.ToString() == pStub) && this.stubCalculationPeriod.initialStubSpecified)
                    stub = this.stubCalculationPeriod.initialStub;
                else if ((StubEnum.Final.ToString() == pStub) && this.stubCalculationPeriod.finalStubSpecified)
                    stub = this.stubCalculationPeriod.finalStub;

                if (null != stub)
                {
                    if (stub.StubTypeFixedRateSpecified)
                        rate = stub.StubTypeFixedRate;
                    else if (stub.StubTypeFloatingRateSpecified)
                        rate = stub.StubTypeFloatingRate;
                    else if (stub.StubTypeAmountSpecified)
                        rate = stub.StubTypeAmount;
                }
            }
            return rate;
        }
        #endregion Rate

        #endregion Methods

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IInterestLeg Members
		IReturnSwapLeg IInterestLeg.ReturnSwapLeg { get { return (IReturnSwapLeg)this; } }
		IInterestCalculation IInterestLeg.InterestCalculation 
		{
			set { this.interestCalculation = (InterestCalculation)value; }
			get { return this.interestCalculation; } 
		}
		IInterestLegCalculationPeriodDates IInterestLeg.CalculationPeriodDates 
		{
			set { this.interestLegCalculationPeriodDates = (InterestLegCalculationPeriodDates)value; }
			get { return this.interestLegCalculationPeriodDates; } 
		}
		bool IInterestLeg.StubCalculationPeriodSpecified { get { return this.stubCalculationPeriodSpecified; } }
		IStubCalculationPeriod IInterestLeg.StubCalculationPeriod { get { return this.stubCalculationPeriod; } }
		ILegAmount IInterestLeg.InterestAmount 
		{
			set { this.interestAmount = (LegAmount)value; }
			get { return this.interestAmount; } 
		}
		IAdjustableRelativeOrPeriodicDates2 IInterestLeg.PaymentDates { get { return this.interestLegCalculationPeriodDates.interestLegPaymentDates; } }
		IReturnSwapNotional IInterestLeg.Notional { get {return this.notional;}}
		IInterestCalculation IInterestLeg.CreateInterestCalculation { get { return new InterestCalculation(); } }
		IInterestLegCalculationPeriodDates IInterestLeg.CreateCalculationPeriodDates { get { return new InterestLegCalculationPeriodDates(); } }
		ILegAmount IInterestLeg.CreateLegAmount { get { return new LegAmount(); } }
        IInterestLegResetDates IInterestLeg.CreateResetDates { get { return new InterestLegResetDates(); } }
        IAdjustableRelativeOrPeriodicDates2 IInterestLeg.CreatePaymentDates 
        { 
            get 
            {
				AdjustableRelativeOrPeriodicDates2 _paymentDates = new AdjustableRelativeOrPeriodicDates2
				{
					dtType_AdjustableDates = new AdjustableDates(),
					dtType_RelativeDates = new RelativeDates(),
					dtType_PeriodicDates = new PeriodicDates()
				};
				return _paymentDates; 
            } 
        }
        IResetFrequency IInterestLeg.CreateResetFrequency { get { return new ResetFrequency(); } }
        IRelativeDateOffset IInterestLeg.CreateRelativeDateOffset { get { return new RelativeDateOffset(); } }
        EFS_InterestLeg IInterestLeg.Efs_InterestLeg
        {
            set { this.efs_InterestLeg = value; }
            get { return this.efs_InterestLeg; }
        }
        bool IInterestLeg.IsPeriodRelativeToReturnLeg
        {
            get { return this.IsPeriodRelativeToReturnLeg; }
        }
		#endregion IInterestLeg Members

        #region IReturnSwapLeg Members
        string IReturnSwapLeg.LegEventCode
        {
            get
            {
                return EventCodeFunc.InterestLeg;
            }
        }
        string IReturnSwapLeg.LegEventType
        {
            get
            {
                string eventType = EventTypeFunc.FloatingRate;
                if (this.interestCalculation.rateFixedRateSpecified)
                    eventType = EventTypeFunc.FixedRate;
                return eventType;
            }
        }
        #endregion IReturnSwapLeg Members

	}
	#endregion InterestLeg
	#region InterestLegCalculationPeriodDates
    // EG 20140526 New build FpML4.4 Change Type of interestLegPaymentDates (AdjustableRelativeOrPeriodicDates2)
	public partial class InterestLegCalculationPeriodDates : IInterestLegCalculationPeriodDates
	{
		#region IInterestLegCalculationPeriodDates Members
		IAdjustableOrRelativeDate IInterestLegCalculationPeriodDates.EffectiveDate { get { return this.effectiveDate; } }
		IAdjustableOrRelativeDate IInterestLegCalculationPeriodDates.TerminationDate { get { return this.terminationDate; } }
        IAdjustableRelativeOrPeriodicDates2 IInterestLegCalculationPeriodDates.PaymentDates
        {
            set { this.interestLegPaymentDates = (AdjustableRelativeOrPeriodicDates2)value; }
            get { return this.interestLegPaymentDates; }
        }
        IInterestLegResetDates IInterestLegCalculationPeriodDates.ResetDates
        {
            set { this.interestLegResetDates = (InterestLegResetDates)value; }
            get { return this.interestLegResetDates; }
        }
        string IInterestLegCalculationPeriodDates.Id
        {
            set { this.Id = value; }
            get { return this.Id; }
        }
		#endregion IInterestLegCalculationPeriodDates Members
	}
	#endregion InterestLegCalculationPeriodDates
	#region InterestLegResetDates
    // EG 20140702 Update Interface
	public partial class InterestLegResetDates : IInterestLegResetDates
	{
		#region Constructors
		public InterestLegResetDates()
		{
			resetDatesResetFrequency = new ResetFrequency();
            calculationPeriodDatesReference = new DateReference();
            fixingDates = new AdjustableDatesOrRelativeDateOffset();
		}
		#endregion Constructors

		#region IInterestLegResetDates Members
		IReference IInterestLegResetDates.CalculationPeriodDatesReference 
        { 
            set { this.calculationPeriodDatesReference = (DateReference)value; } 
            get { return this.calculationPeriodDatesReference; } 
        }
		bool IInterestLegResetDates.ResetRelativeToSpecified 
        { 
            set { this.resetDatesResetRelativeToSpecified = value; }
            get { return this.resetDatesResetRelativeToSpecified; }
        }
		ResetRelativeToEnum IInterestLegResetDates.ResetRelativeTo 
        {
            set { this.resetDatesResetRelativeTo = value; } 
            get { return this.resetDatesResetRelativeTo; } 
        }
		bool IInterestLegResetDates.ResetFrequencySpecified 
        {
            set { this.resetDatesResetFrequencySpecified = value; }
            get { return this.resetDatesResetFrequencySpecified; } 
        }
		IResetFrequency IInterestLegResetDates.ResetFrequency 
        { 
            set { this.resetDatesResetFrequency = (ResetFrequency) value; } 
            get { return this.resetDatesResetFrequency; } 
        }
        bool IInterestLegResetDates.InitialFixingDateSpecified
        {
            set { this.initialFixingDateSpecified = value; }
            get { return this.initialFixingDateSpecified; }
        }
        IRelativeDateOffset IInterestLegResetDates.InitialFixingDate
        {
            set { this.initialFixingDate = (RelativeDateOffset)value; }
            get { return this.initialFixingDate; }
        }
        bool IInterestLegResetDates.FixingDatesSpecified
        {
            set { this.fixingDatesSpecified = value; }
            get { return this.fixingDatesSpecified; }
        }
        IAdjustableDatesOrRelativeDateOffset IInterestLegResetDates.FixingDates
        {
            set { this.fixingDates = (AdjustableDatesOrRelativeDateOffset)value; }
            get { return this.fixingDates; }
        }
        #endregion IInterestLegResetDates Members

	}
	#endregion InterestLegResetDates

	#region LegAmount
	// EG 20140702 Update Constructors
	// EG 20231127 [WI755] Implementation Return Swap : Add New initialize in Constructor
	// EG 20231127 [WI755] Implementation Return Swap : Add New Interface Properties
	public partial class LegAmount : ILegAmount,ILegCurrency
	{
		#region Constructors
		public LegAmount()
		{
			legAmountFormula = new Formula();
			legAmountReferenceAmount = new ReferenceAmount();
			legCurrencyNone = new Empty();
			legCurrencyDeterminationMethod = new DeterminationMethod();
			legCurrencyCurrency = new Currency();
			legCurrencyCurrencyReference = new IdentifiedCurrencyReference();
			paymentCurrency = new PaymentCurrency();
		}
		#endregion Constructors

		#region ILegAmount Members
		bool ILegAmount.PaymentCurrencySpecified
		{
			get { return this.paymentCurrencySpecified; }
		}
		IPaymentCurrency ILegAmount.PaymentCurrency
		{
			get {return this.paymentCurrency;}
		}
		bool ILegAmount.ReferenceAmountSpecified
		{
			set { this.legAmountReferenceAmountSpecified = value;}
			get { return this.legAmountReferenceAmountSpecified;}
		}
		IScheme ILegAmount.ReferenceAmount
		{
			get { return this.legAmountReferenceAmount;}
		}
		bool ILegAmount.CalculationDatesSpecified
		{
			get {return this.calculationDatesSpecified;}
		}
		IAdjustableRelativeOrPeriodicDates ILegAmount.CalculationDates
		{
			get {return this.calculationDates;}
		}
		bool ILegAmount.FormulaSpecified
		{
			get { return this.legAmountFormulaSpecified; }
		}
		IFormula ILegAmount.Formula
		{
			get { return this.legAmountFormula; }
		}
		bool ILegAmount.CurrencyDeterminationMethodSpecified
		{
			get { return this.legCurrencyDeterminationMethodSpecified; }
		}
		IScheme ILegAmount.CurrencyDeterminationMethod
		{
			get { return this.legCurrencyDeterminationMethod; }
		}
		bool ILegAmount.CurrencySpecified
		{
			get { return this.legCurrencyCurrencySpecified; }
		}
		ICurrency ILegAmount.Currency
		{
			get { return this.legCurrencyCurrency; }
		}
		bool ILegAmount.CurrencyReferenceSpecified
		{
			get { return this.legCurrencyCurrencyReferenceSpecified; }
		}
		IReference ILegAmount.CurrencyReference
		{
			get { return this.legCurrencyCurrencyReference; }
		}
		public string MainLegAmountCurrency
		{
			get
			{
				string ret = string.Empty;
				if (this.paymentCurrencySpecified)
				{
					if (this.paymentCurrency.paymentCurrencyCurrencySpecified)
						ret = paymentCurrency.paymentCurrencyCurrency.Value;
				}
				else if (this.legCurrencyCurrencySpecified)
				{
					ret = this.legCurrencyCurrency.Value;
				}
				else if (this.legCurrencyCurrencyReferenceSpecified)
				{
					// EG 20231127 [WI749] Implementation Return Swap : TODO
				}
				return ret;
			}
		}
		#endregion ILegAmount Members

		#region ILegCurrency Members
		bool ILegCurrency.CurrencySpecified
		{
			get {return this.legCurrencyCurrencySpecified; }
		}
		ICurrency ILegCurrency.Currency
		{
			get {return this.legCurrencyCurrency; }
		}
		bool ILegCurrency.DeterminationMethodSpecified
		{
			get {return this.legCurrencyDeterminationMethodSpecified; }
		}
		IScheme ILegCurrency.DeterminationMethod
		{
			get {return this.legCurrencyDeterminationMethod;}
		}
		bool ILegCurrency.CurrencyReferenceSpecified
		{
			get {return this.legCurrencyCurrencyReferenceSpecified; }
		}
		IReference ILegCurrency.CurrencyReference
		{
			get {return this.legCurrencyCurrencyReference;}
		}
		#endregion ILegCurrency Members
	}
	#endregion LegAmount

	#region MakeWholeProvisions
	public partial class MakeWholeProvisions : IMakeWholeProvisions
	{
		#region IMakeWholeProvisions Members
		EFS_Date IMakeWholeProvisions.MakeWholeDate
		{
			set { this.makeWholeDate = value; }
			get { return this.makeWholeDate; }
		}
		EFS_Decimal IMakeWholeProvisions.RecallSpread
		{
			set { this.recallSpread = value; }
			get { return this.recallSpread; }
		}
		#endregion IMakeWholeProvisions Members
	}
	#endregion MakeWholeProvisions

	#region NettedSwapBase
	public abstract partial class NettedSwapBase : IProduct
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
	}
	#endregion NettedSwapBase

	#region OptionFeatures
	public partial class OptionFeatures : IOptionFeatures
	{
        #region Constructors
        public OptionFeatures()
        {
            asian = new Asian();
            barrier = new Barrier();
            knock = new Knock();
            passThrough = new PassThrough();
            dividendAdjustment = new DividendAdjustment();
            multipleBarrier = new ExtendedBarrier[1] { new ExtendedBarrier() };
        }
        #endregion Constructors
        #region IOptionFeatures Membres
		bool IOptionFeatures.AsianSpecified
		{
			set { this.asianSpecified = value; }
			get { return this.asianSpecified; }
		}
		IAsian IOptionFeatures.Asian
		{
			set { this.asian = (Asian)value; }
			get { return this.asian; }
		}
		bool IOptionFeatures.BarrierSpecified
		{
			set { this.barrierSpecified = value; }
			get { return this.barrierSpecified; }
		}
		IBarrier IOptionFeatures.Barrier
		{
			set { this.barrier = (Barrier)value; }
			get { return this.barrier; }
		}
		bool IOptionFeatures.KnockSpecified
		{
			set { this.knockSpecified = value; }
			get { return this.knockSpecified; }
		}
		IKnock IOptionFeatures.Knock
		{
			set { this.knock = (Knock)value; }
			get { return this.knock; }
		}
		bool IOptionFeatures.PassThroughSpecified
		{
			set { this.passThroughSpecified = value; }
			get { return this.passThroughSpecified; }
		}
		IPassThrough IOptionFeatures.PassThrough
		{
			set { this.passThrough = (PassThrough)value; }
			get { return this.passThrough; }
		}
		bool IOptionFeatures.DividendAdjustmentSpecified
		{
			set { this.dividendAdjustmentSpecified = value; }
			get { return this.dividendAdjustmentSpecified; }
		}
		IDividendAdjustment IOptionFeatures.DividendAdjustment
		{
			set { this.dividendAdjustment = (DividendAdjustment)value; }
			get { return this.dividendAdjustment; }
		}
        bool IOptionFeatures.MultipleBarrierSpecified
        {
            set { this.multipleBarrierSpecified = value; }
            get { return this.multipleBarrierSpecified; }
        }
        IExtendedBarrier[] IOptionFeatures.MultipleBarrier
        {
            set { this.multipleBarrier = (ExtendedBarrier[])value; }
            get { return this.multipleBarrier; }
        }

		#endregion IOptionFeatures Members
    }
	#endregion OptionFeatures


	#region PrincipalExchangeAmount
	public partial class PrincipalExchangeAmount : IPrincipalExchangeAmount
	{
		#region Constructors
		public PrincipalExchangeAmount()
		{
			exchangeAmountRelativeTo = new AmountReference();
			exchangeDeterminationMethod = new EFS_MultiLineString();
			exchangePrincipalAmount = new Money();
		}
		#endregion Constructors

		#region IPrincipalExchangeAmount Members
		bool IPrincipalExchangeAmount.RelativeToSpecified {get { return this.exchangeAmountRelativeToSpecified; }}
		IReference IPrincipalExchangeAmount.RelativeTo { get { return this.exchangeAmountRelativeTo; } }
		bool IPrincipalExchangeAmount.DeterminationMethodSpecified { get { return this.exchangeDeterminationMethodSpecified; } }
		EFS_MultiLineString IPrincipalExchangeAmount.DeterminationMethod { get { return this.exchangeDeterminationMethod; } }
		bool IPrincipalExchangeAmount.AmountSpecified { get { return this.exchangePrincipalAmountSpecified; } }
		IMoney IPrincipalExchangeAmount.Amount { get { return this.exchangePrincipalAmount; } }
		#endregion IPrincipalExchangeAmount Members
	}
	#endregion PrincipalExchangeAmount
	#region PrincipalExchangeDescriptions
	public partial class PrincipalExchangeDescriptions : IEFS_Array,IPrincipalExchangeDescriptions
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IPrincipalExchangeDescriptions Members
		IReference IPrincipalExchangeDescriptions.PayerPartyReference { get { return this.payerPartyReference; } }
		IReference IPrincipalExchangeDescriptions.ReceiverPartyReference { get { return this.receiverPartyReference; } }
		IPrincipalExchangeAmount IPrincipalExchangeDescriptions.PrincipalExchangeAmount { get { return this.principalExchangeAmount; } }
		IAdjustableOrRelativeDate IPrincipalExchangeDescriptions.PrincipalExchangeDate { get { return this.principalExchangeDate; } }
		#endregion IPrincipalExchangeDescriptions Members
	}
	#endregion PrincipalExchangeDescriptions
	#region PrincipalExchangeFeatures
	public partial class PrincipalExchangeFeatures : IPrincipalExchangeFeatures
	{
		#region IPrincipalExchangeFeatures Members
		bool IPrincipalExchangeFeatures.DescriptionsSpecified { get { return this.principalExchangeDescriptionsSpecified; } }
		IPrincipalExchanges IPrincipalExchangeFeatures.PrincipalExchanges { get { return this.principalExchanges; } }
		IPrincipalExchangeDescriptions[] IPrincipalExchangeFeatures.Descriptions { get { return this.principalExchangeDescriptions; } }
		#endregion IPrincipalExchangeFeatures Members
	}
	#endregion PrincipalExchangeFeatures

	#region Return
    // EG 20140702 Update Interface
	public partial class Return : IReturn
	{
		#region IReturn Members
		ReturnTypeEnum IReturn.ReturnType
		{
			set {this.returnType = value; }
			get {return this.returnType; }
		}
		bool IReturn.DividendConditionsSpecified
		{
			set { this.dividendConditionsSpecified = value; }
			get { return this.dividendConditionsSpecified; }
		}
		IDividendConditions IReturn.DividendConditions
		{
            set { this.dividendConditions = (DividendConditions)value; }
			get { return this.dividendConditions; }
		}
        IDividendConditions IReturn.CreateDividendConditions
		{
            get { return new DividendConditions();}
		}
		#endregion IReturn Members
	}
	#endregion Return
	#region ReturnLeg
	// EG 20140702 Update Accessors|Interface
	// EG 20231127 [WI755] Implementation Return Swap : Refactoring Code Analysis 
	public partial class ReturnLeg : IEFS_Array , IReturnLeg
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_ReturnLeg efs_ReturnLeg;
		#endregion Members
		#region Accessors
		#region AdjustedEffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedEffectiveDate
		{
			get
			{
				EFS_Date dt = new EFS_Date
				{
					DateValue = EffectiveDateAdjustment.adjustedDate.DateValue
				};
				return dt;
            }
		}
		#endregion AdjustedEffectiveDate
        #region AdjustedTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedTerminationDate
		{
			get
			{
				EFS_Date dt = new EFS_Date
				{
					DateValue = TerminationDateAdjustment.adjustedDate.DateValue
				};
				return dt;
            }
        }
        #endregion AdjustedTerminationDate
        #region Asset
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Asset Asset
        {
            get { return efs_ReturnLeg.Asset; }
        }
        #endregion Asset
        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate EffectiveDate
		{
			get {return new EFS_EventDate(EffectiveDateAdjustment); }
		}

		#endregion EffectiveDate
		#region EffectiveDateAdjustment
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate EffectiveDateAdjustment
		{
			get {return efs_ReturnLeg.effectiveDateAdjustment; }
		}
		#endregion EffectiveDateAdjustment
		#region PayerPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string PayerPartyReference
		{
			get {return payerPartyReference.href; }
		}
		#endregion PayerPartyReference
		#region ReceiverPartyReference
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string ReceiverPartyReference
		{
			get {return receiverPartyReference.href; }
		}
		#endregion ReceiverPartyReference

        #region IsOpenDailyPeriod
        /// <summary>
        /// Obtient true si l'élément rateOfReturn.valuationPriceInterim définie calculationPeriodFrequency=1D et si la terminationDate =  DateTime.MaxValue.Date
        /// </summary>
        public bool IsOpenDailyPeriod
		{
			get
			{
                bool _isOpenDailyPeriod = false;
                if (terminationDate.adjustableOrRelativeDateAdjustableDateSpecified)
                    _isOpenDailyPeriod =  (terminationDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate.DateValue == DateTime.MaxValue.Date) && IsDailyPeriod;
                return _isOpenDailyPeriod;
			}
        }
        #endregion IsOpenDailyPeriod
        #region IsDailyPeriod
        /// <summary>
        /// Obtient true si l'élément rateOfReturn.valuationPriceInterim définie calculationPeriodFrequency=1D 
        /// </summary>
        public bool IsDailyPeriod
        {
            get
            {
                bool _isDailyPeriod = false;
                if (rateOfReturn.valuationPriceInterimSpecified &&
                    rateOfReturn.valuationPriceInterim.valuationRulesSpecified &&
                    rateOfReturn.valuationPriceInterim.valuationRules.valuationDatesSpecified &&
                       rateOfReturn.valuationPriceInterim.valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDatesSpecified)
                {
                    PeriodicDates _valuationDates = rateOfReturn.valuationPriceInterim.valuationRules.valuationDates.adjustableRelativeOrPeriodicPeriodicDates;
                    _isDailyPeriod = (_valuationDates.calculationPeriodFrequency.period == PeriodEnum.D) &&
                                     (_valuationDates.calculationPeriodFrequency.periodMultiplier.IntValue == 1);
                }
                return _isDailyPeriod;
            }
        }
		#endregion IsDailyPeriod
		// EG 20231127 [WI755] Implementation Return Swap : New property
		public string PaymentCurrency
		{
			get { return efs_ReturnLeg.paymentCurrency; }
		}
		#region TerminationDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate TerminationDate
		{
			get {return new EFS_EventDate(TerminationDateAdjustment); }
		}
		#endregion TerminationDate
		#region TerminationDateAdjustment
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        private EFS_AdjustableDate TerminationDateAdjustment
		{
			get {return efs_ReturnLeg.terminationDateAdjustment; }
		}
		#endregion TerminationDateAdjustment

        #region ValuationEventCode
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ValuationEventCode
        {
            get { return efs_ReturnLeg.ValuationEventCode; }
        }
        #endregion ValuationEventCode
        #region ValuationEventType
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string ValuationEventType
        {
            get { return efs_ReturnLeg.ValuationEventType; }
        }
        #endregion ValuationEventType
        #endregion Accessors

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, 
            FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members

        #region IReturnSwapLeg Members
        string IReturnSwapLeg.LegEventCode
        {
            get
            {
                string eventCode = string.Empty;
                switch (@return.returnType)
                {
                    case ReturnTypeEnum.Total:
                        eventCode = EventCodeFunc.TotalReturnLeg;
                        break;
                    case ReturnTypeEnum.Price:
                        eventCode = EventCodeFunc.PriceReturnLeg;
                        break;
                    case ReturnTypeEnum.Dividend:
                        eventCode = EventCodeFunc.DividendReturnLeg;
                        break;
                }
                return eventCode;
            }
        }
        string IReturnSwapLeg.LegEventType
        {
            get
            {
                string eventType = EventTypeFunc.Term;
                if (efs_ReturnLeg.IsOpen)
                    eventType = EventTypeFunc.Open; ;
                return eventType;
            }
        }
        #endregion IReturnSwapLeg Members
        #region IReturnLeg Members
        IAdjustableOrRelativeDate IReturnLeg.EffectiveDate
        {
            set { this.effectiveDate = (AdjustableOrRelativeDate)value; }
            get { return this.effectiveDate; }
        }
        IAdjustableOrRelativeDate IReturnLeg.TerminationDate
        {
            set { this.terminationDate = (AdjustableOrRelativeDate)value; }
            get { return this.terminationDate; }
        }
        IReturnSwapAmount IReturnLeg.ReturnSwapAmount
        {
            set { this.amount = (ReturnSwapAmount)value; }
            get { return this.amount; }
        }
        IReturnLegValuation IReturnLeg.RateOfReturn 
        {
            set { this.rateOfReturn = (ReturnLegValuation)value; }
            get { return this.rateOfReturn; } 
        }
		IReturnSwapNotional IReturnLeg.Notional { get { return this.notional; } }
		IUnderlyer IReturnLeg.Underlyer { get { return this.underlyer; } }
		EFS_ReturnLeg IReturnLeg.Efs_ReturnLeg
		{
			set { this.efs_ReturnLeg = value; }
            get { return this.efs_ReturnLeg; }
		}
		bool IReturnLeg.FxFeatureSpecified
		{
			set { this.fxFeatureSpecified = value;}
			get { return this.fxFeatureSpecified;}
		}
		IFxFeature IReturnLeg.FxFeature 
		{ 
			set { this.fxFeature = (FxFeature)value; } 
			get { return this.fxFeature; } 
		}
		IReturn IReturnLeg.Return
		{
            set { this.@return = (Return)value; }
            get { return this.@return; }
		}
        IReturnLegValuation IReturnLeg.CreateRateOfReturn { get { return new ReturnLegValuation(); } }
        IFxFeature IReturnLeg.CreateFxFeature { get { return new FxFeature(); } }
        IReturnSwapAmount IReturnLeg.CreateReturnSwapAmount 
        { 
            get 
            {
                ReturnSwapAmount ret = new ReturnSwapAmount
                {
                    cashSettlement = new EFS_Boolean(false)
                };
                return ret;
            } 
        }
        IReturn IReturnLeg.CreateReturn 
        { 
            get 
            {
                Return ret = new Return
                {
                    returnType = ReturnTypeEnum.Price
                };
                return ret;
            } 
        }
        IMarginRatio IReturnLeg.CreateMarginRatio
        {
            get
            {
                return new MarginRatio();
            }
        }
        bool IReturnLeg.IsOpenDailyPeriod
        {
            get { return this.IsOpenDailyPeriod; }
        }
        bool IReturnLeg.IsDailyPeriod
        {
            get { return this.IsDailyPeriod; }
        }
        #endregion IReturnLeg Members
	}
	#endregion ReturnLeg
	#region ReturnLegValuation
    // EG 20140702 New build FpML4.4 notionalResetSpecified/notionalReset
	public partial class ReturnLegValuation : IReturnLegValuation
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_RateOfReturn efs_RateOfReturn;
        #endregion Members

        #region IReturnLegValuation Members
        bool IReturnLegValuation.NotionalResetSpecified
        {
            set { this.notionalResetSpecified = value; }
            get { return this.notionalResetSpecified; }
        }
        EFS_Boolean IReturnLegValuation.NotionalReset
        {
            set { this.notionalReset = value; }
            get { return this.notionalReset; }
        }
		IReturnLegValuationPrice IReturnLegValuation.InitialPrice 
		{
            set { this.initialPrice = (ReturnLegValuationPrice)value; }
            get { return this.initialPrice; } 
		}
		bool IReturnLegValuation.ValuationPriceInterimSpecified
		{
			set { this.valuationPriceInterimSpecified = value; }
			get { return this.valuationPriceInterimSpecified; }
		}
		IReturnLegValuationPrice IReturnLegValuation.ValuationPriceInterim
		{
            set { this.valuationPriceInterim = (ReturnLegValuationPrice)value; }
            get { return this.valuationPriceInterim; }
		}
		IReturnLegValuationPrice IReturnLegValuation.ValuationPriceFinal
		{
            set { this.valuationPriceFinal = (ReturnLegValuationPrice)value; }
            get { return this.valuationPriceFinal; }
		}
		IReturnLegValuationPrice IReturnLegValuation.CreateReturnLegValuationPrice {get {return new ReturnLegValuationPrice();}}
        IReturnSwapPaymentDates IReturnLegValuation.PaymentDates
        {
            set { this.paymentDates = (ReturnSwapPaymentDates) value; }
            get { return this.paymentDates; }
        }
        bool IReturnLegValuation.MarginRatioSpecified
        {
            set {this.marginRatioSpecified=value ; }
            get { return this.marginRatioSpecified; }
        }

        IMarginRatio IReturnLegValuation.MarginRatio
        {
            set { this.marginRatio = (MarginRatio)value; }
            get { return this.marginRatio; }
        }
        EFS_RateOfReturn IReturnLegValuation.Efs_RateOfReturn
        {
            set { this.efs_RateOfReturn = value; }
            get { return this.efs_RateOfReturn; }
        }
		#endregion IReturnLegValuation Members
	}
	#endregion ReturnLegValuation
	#region ReturnLegValuationPrice
	public partial class ReturnLegValuationPrice : IReturnLegValuationPrice
	{
		#region IReturnLegValuationPrice Members
        bool IReturnLegValuationPrice.ValuationRulesSpecified
        {
            set { this.valuationRulesSpecified = value; }
            get { return this.valuationRulesSpecified; }
        }
		IEquityValuation IReturnLegValuationPrice.ValuationRules
		{
			set {this.valuationRules = (EquityValuation) value;}
			get {return this.valuationRules;}
		}
		IEquityValuation IReturnLegValuationPrice.CreateValuationRules() { return new EquityValuation() ; }
		IAdjustableRelativeOrPeriodicDates IReturnLegValuationPrice.CreateAdjustableRelativeOrPeriodicDates() {return new AdjustableRelativeOrPeriodicDates();  }
		IPeriodicDates IReturnLegValuationPrice.CreatePeriodicDates() {  return new PeriodicDates();  }
		IActualPrice IReturnLegValuationPrice.CreatePrice() { return new ActualPrice(); }
		ICurrency IReturnLegValuationPrice.CreateCurrency() { return new Currency(); }
		#endregion IReturnLegValuationPrice Members
	}
	#endregion ReturnLegValuationPrice
	#region ReturnSwap
    // EG 20140702 Update 
    public partial class ReturnSwap : IProduct, IReturnSwap, IDeclarativeProvision
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_ReturnSwap efs_ReturnSwap;
        #endregion Members

        #region Accessors
        #region ClearingBusinessDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate ClearingBusinessDate
        {
            get { return efs_ReturnSwap.ClearingBusinessDate; }
        }
        #endregion ClearingBusinessDate
        #region InitialMarginPayerPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginPayerPartyReference
        {
            get
            {
                return efs_ReturnSwap.InitialMarginPayerPartyReference;
            }
        }
        #endregion InitialMarginPayerPartyReference
        #region InitialMarginReceiverPartyReference
        // EG 20141029 New
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public string InitialMarginReceiverPartyReference
        {
            get
            {
                return efs_ReturnSwap.InitialMarginReceiverPartyReference;
            }
        }
        #endregion InitialMarginReceiverPartyReference

        #region Stream
        public InterestLeg[] InterestLeg
        {
            get { return interestLeg; }
        }
		#endregion Stream
		// EG 20231127 [WI755] Implementation Return Swap : New Properties
		/// <summary>
		/// L'instrument est-il fongible
		/// </summary>
		public bool IsFungible
		{
			get { return efs_ReturnSwap.isFungible; }
		}
		/// <summary>
		/// L'instrument peut-il générer des événements de marge quotidiens (si Fongible=
		/// </summary>
		public bool IsMargining
		{
			get { return efs_ReturnSwap.isMargining; }
		}
		/// <summary>
		/// L'instrument peut-il générer des événement de financement quotidiens
		/// </summary>
		public bool IsFunding
		{
			get { return efs_ReturnSwap.isFunding; }
		}
		#endregion Accessors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

		#region IReturnSwap Members
        bool IReturnSwap.PrincipalExchangeFeaturesSpecified
        {
            set { this.principalExchangeFeaturesSpecified = value; }
            get { return this.principalExchangeFeaturesSpecified; }
        }
        IPrincipalExchangeFeatures IReturnSwap.PrincipalExchangeFeatures
        {
            set { this.principalExchangeFeatures = (PrincipalExchangeFeatures)value; }
            get { return this.principalExchangeFeatures; }
        }
		bool IReturnSwap.EarlyTerminationSpecified 
		{ 
			set { this.earlyTerminationSpecified = value; } 
			get { return this.earlyTerminationSpecified; } 
		}
		IReturnSwapEarlyTermination[] IReturnSwap.EarlyTermination 
		{
			set { this.earlyTermination = (ReturnSwapEarlyTermination[])value; } 
			get { return this.earlyTermination; } 
		}
		bool IReturnSwap.ExtraordinaryEventsSpecified 
		{ 
			set { this.extraordinaryEventsSpecified = value; } 
			get { return this.extraordinaryEventsSpecified; } 
		}
		bool IReturnSwap.AdditionalPaymentSpecified
		{
			set { this.additionalPaymentSpecified = value; }
			get { return this.additionalPaymentSpecified; }
		}
		IReturnSwapAdditionalPayment[] IReturnSwap.AdditionalPayment
		{
			set { this.additionalPayment = (ReturnSwapAdditionalPayment[])value; }
			get { return this.additionalPayment; }
		}
		IExtraordinaryEvents IReturnSwap.ExtraordinaryEvents 
		{ 
			set { this.extraordinaryEvents = (ExtraordinaryEvents)value; } 
			get { return this.extraordinaryEvents; } 
		}
		IExtraordinaryEvents IReturnSwap.CreateExtraordinaryEvents { get { return new ExtraordinaryEvents(); } }
		IReturnSwapEarlyTermination[] IReturnSwap.CreateEarlyTermination { get { return new ReturnSwapEarlyTermination[1] { new ReturnSwapEarlyTermination() }; } }
        EFS_ReturnSwap IReturnSwap.Efs_ReturnSwap
        {
            set { this.efs_ReturnSwap = value; }
            get { return this.efs_ReturnSwap; }
        }
		#endregion IReturnSwap Members

        #region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
    }
	#endregion ReturnSwap
	#region ReturnSwapAdditionalPayment
	public partial class ReturnSwapAdditionalPayment : IEFS_Array,IReturnSwapAdditionalPayment
	{
		#region Constructors
		public ReturnSwapAdditionalPayment()
		{
			additionalPaymentDate = new AdjustableOrRelativeDate();
			additionalPaymentAmount = new AdditionalPaymentAmount();
			payerPartyReference = new PartyOrAccountReference();
			receiverPartyReference = new PartyOrAccountReference();
			paymentType = new PaymentType();
		}
		#endregion Constructors
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IReturnSwapAdditionalPayment Members
		IReference IReturnSwapAdditionalPayment.PayerPartyReference
		{
			get { return this.payerPartyReference; }
		}
		IReference IReturnSwapAdditionalPayment.ReceiverPartyReference
		{
			get { return this.receiverPartyReference; }
		}
		IAdjustableOrRelativeDate IReturnSwapAdditionalPayment.AdditionalPaymentDate
		{
			get { return this.additionalPaymentDate; }
		}
		bool IReturnSwapAdditionalPayment.PaymentTypeSpecified
		{
			set {this.paymentTypeSpecified = value;}
			get {return this.paymentTypeSpecified;}
		}
		IScheme IReturnSwapAdditionalPayment.PaymentType
		{
			set { this.paymentType =(PaymentType) value;}
			get { return this.paymentType;}
		}
		IScheme IReturnSwapAdditionalPayment.CreatePaymentType
		{
			get { return new PaymentType(); }
		}
		IAdditionalPaymentAmount IReturnSwapAdditionalPayment.AdditionalPaymentAmount
		{
			get { return this.additionalPaymentAmount; }
		}
		#endregion IReturnSwapAdditionalPayment Members
	}
	#endregion ReturnSwapAdditionalPayment
	#region ReturnSwapAmount
	// EG 20231127 [WI755] Implementation Return Swap : Add new  interface properties
	public partial class ReturnSwapAmount : IReturnSwapAmount
	{
		#region IReturnSwapAmount Members
		EFS_Boolean IReturnSwapAmount.CashSettlement
		{
			get {return this.cashSettlement;}
		}
		bool IReturnSwapAmount.OptionsExchangeDividendsSpecified
		{
			get {return this.optionsExchangeDividendsSpecified; }
		}
		EFS_Boolean IReturnSwapAmount.OptionsExchangeDividends
		{
			get { return this.optionsExchangeDividends; }
		}
		bool IReturnSwapAmount.AdditionalDividendsSpecified
		{
			get {return this.additionalDividendsSpecified; }
		}
		EFS_Boolean IReturnSwapAmount.AdditionalDividends
		{
			get {return this.additionalDividends; }
		}
		#endregion IReturnSwapAmount Members
		#region ILegAmount Members
		bool ILegAmount.PaymentCurrencySpecified
		{
			get {return this.paymentCurrencySpecified; }
		}
		IPaymentCurrency ILegAmount.PaymentCurrency
		{
			get {return this.paymentCurrency; }
		}
		bool ILegAmount.ReferenceAmountSpecified
		{
			set {this.legAmountReferenceAmountSpecified = value; }
			get {return this.legAmountReferenceAmountSpecified; }
		}
		IScheme ILegAmount.ReferenceAmount
		{
			get {return this.legAmountReferenceAmount; }
		}
		bool ILegAmount.CalculationDatesSpecified
		{
			get {return this.calculationDatesSpecified; }
		}
		IAdjustableRelativeOrPeriodicDates ILegAmount.CalculationDates
		{
			get {return this.calculationDates; }
		}
		bool ILegAmount.FormulaSpecified
		{
			get { return this.legAmountFormulaSpecified; }
		}
		IFormula ILegAmount.Formula
		{
			get { return this.legAmountFormula; }
		}
		bool ILegAmount.CurrencyDeterminationMethodSpecified
		{
			get { return this.legCurrencyDeterminationMethodSpecified; }
		}
		IScheme ILegAmount.CurrencyDeterminationMethod
		{
			get { return this.legCurrencyDeterminationMethod; }
		}
		bool ILegAmount.CurrencySpecified
		{
			get { return this.legCurrencyCurrencySpecified; }
		}
		ICurrency ILegAmount.Currency
		{
			get { return this.legCurrencyCurrency; }
		}
		bool ILegAmount.CurrencyReferenceSpecified
		{
			get { return this.legCurrencyCurrencyReferenceSpecified; }
		}
		IReference ILegAmount.CurrencyReference
		{
			get { return this.legCurrencyCurrencyReference; }
		}
		#endregion ILegAmount Members
	}
	#endregion ReturnSwapAmount
	#region ReturnSwapBase
	// EG 20140702 Update Interface
	public partial class ReturnSwapBase : IProduct, IReturnSwapBase, IDeclarativeProvision 
	{
		#region Accessors
		#region BuyerPartyReference
		public string BuyerPartyReference
		{
			get
			{
				if (buyerPartyReferenceSpecified)
					return buyerPartyReference.href;
				else
					return null;
			}
		}
		#endregion BuyerPartyReference
		#region ReturnSwapType
		public string ReturnSwapType
		{
			get
			{
				string eventType = EventTypeFunc.Basket;
				foreach (ReturnLeg leg in returnLeg)
				{
					if (leg.underlyer.underlyerSingleSpecified)
					{
						Type tUnderlyer = leg.underlyer.GetType();
						if (tUnderlyer.Equals(typeof(Index)))
							eventType = EventTypeFunc.Index;
						else
							eventType = EventTypeFunc.Share;
						break;
					}
				}
				return eventType;
			}
		}
		#endregion ReturnSwapType
		#region SellerPartyReference
		public string SellerPartyReference
		{
			get
			{
				if (sellerPartyReferenceSpecified)
					return sellerPartyReference.href;
				else
					return null;
			}
		}
		#endregion SellerPartyReference
		#endregion Accessors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IReturnSwapBase Members
		bool IReturnSwapBase.BuyerPartyReferenceSpecified 
		{
			set { this.buyerPartyReferenceSpecified = value;}
			get { return this.buyerPartyReferenceSpecified;}
		}
		IReference IReturnSwapBase.BuyerPartyReference 
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference) value;}
			get { return this.buyerPartyReference;}
		}
		bool IReturnSwapBase.SellerPartyReferenceSpecified 
		{
			set { this.sellerPartyReferenceSpecified = value; }
			get { return this.sellerPartyReferenceSpecified;}
		}
		IReference IReturnSwapBase.SellerPartyReference 
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		bool IReturnSwapBase.ReturnLegSpecified 
        {
            set { this.returnLegSpecified = value; }
            get { return this.returnLegSpecified; } 
        }
		IReturnLeg[] IReturnSwapBase.ReturnLeg { get { return this.returnLeg; } }
		bool IReturnSwapBase.InterestLegSpecified
		{
			set { this.interestLegSpecified = value; }
			get { return this.interestLegSpecified; }
		}
		IInterestLeg[] IReturnSwapBase.InterestLeg
		{
			set { this.interestLeg = (InterestLeg[])value; }
			get { return this.interestLeg; }
		}
        IReference IReturnSwapBase.CreatePartyReference { get { return new PartyReference(); } }
        IReference IReturnSwapBase.CreatePartyOrTradeSideReference { get { return new PartyOrTradeSideReference(); } }
        IReturnLeg IReturnSwapBase.CreateReturnLeg { get { return new ReturnLeg(); } }
        /// <summary>
        /// Obtient true si returnLegSpecified ou interestLegSpecified
        /// </summary>
        bool IReturnSwapBase.ReturnSwapLegSpecified
        {
            get
            {
                return this.returnLegSpecified || this.interestLegSpecified;
            }
        }
        /// <summary>
        /// Obtient les returnLeg et les interestLeg dans un même array
        /// </summary>
        IReturnSwapLeg[] IReturnSwapBase.ReturnSwapLeg
        {
            get
            {
                ReturnSwapLeg[] _returnSwapLeg = null;
                ArrayList aReturnSwapLeg = new ArrayList();
                if (this.returnLegSpecified)
                    aReturnSwapLeg.AddRange(this.returnLeg);
                if (this.interestLegSpecified)
                    aReturnSwapLeg.AddRange(this.interestLeg);
                if (0 < aReturnSwapLeg.Count)
                    _returnSwapLeg = (ReturnSwapLeg[])aReturnSwapLeg.ToArray(typeof(ReturnSwapLeg));
                return _returnSwapLeg;
            }
        }
        // FI 20170116 [21916] RptSide (R majuscule)
        IFixTrdCapRptSideGrp[] IReturnSwapBase.RptSide
        {
            set { this.RptSide = (FixML.v50SP1.TrdCapRptSideGrp_Block[])value; }
            get { return this.RptSide; }
        }
		#endregion IReturnSwapBase Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return null; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return null; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return null; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion ReturnSwapBase
	#region ReturnSwapEarlyTermination
    // EG 20140702 Update Interface
	public partial class ReturnSwapEarlyTermination : IEFS_Array, IReturnSwapEarlyTermination
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent,
			ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IReturnSwapEarlyTermination Members
		IReference IReturnSwapEarlyTermination.PartyReference
		{
            set { this.partyReference = (PartyReference)value; }
			get { return this.partyReference; }
		}
		IStartingDate IReturnSwapEarlyTermination.StartingDate
		{
			set { this.startingDate = (StartingDate)value; }
			get { return this.startingDate; }
		}
		IStartingDate IReturnSwapEarlyTermination.CreateStartingDate { get { return new StartingDate(); } }
		#endregion IReturnSwapEarlyTermination Members
	}
	#endregion ReturnSwapEarlyTermination
	#region ReturnSwapLeg
    // EG 20140702 Update Abstract
	public abstract partial class ReturnSwapLeg : IReturnSwapLeg
	{
		#region IReturnSwapLeg Members
		IReference IReturnSwapLeg.PayerPartyReference 
        { 
            set { this.payerPartyReference = (PartyOrAccountReference)value; } 
            get { return this.payerPartyReference; } 
        }
		IReference IReturnSwapLeg.ReceiverPartyReference 
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; } 
        }
        IReference IReturnSwapLeg.CreateReference
        {
            get { return new PartyOrAccountReference(); }
        }
        IMoney IReturnSwapLeg.CreateMoney
        {
            get { return new Money(); }
        }
        string IReturnSwapLeg.LegEventCode
        {
            get { return EventCodeFunc.TotalReturnLeg; }
        }
        string IReturnSwapLeg.LegEventType
        {
            get { return EventTypeFunc.Term; }
        }
        #endregion IReturnSwapLeg Members
	}
	#endregion ReturnSwapLeg
	#region ReturnSwapNotional
    // EG 20140702 Update Accessors
	public partial class ReturnSwapNotional : IReturnSwapNotional
	{
		#region Accessors
        public string AmountRelativeToValue
        {
            get 
            {
                string hRef = string.Empty;
                if (this.returnSwapNotionalAmountRelativeToSpecified)
                    hRef = this.returnSwapNotionalAmountRelativeTo.href;
                return hRef;
            }
        }
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
		#endregion Accessors
		#region Constructors
		public ReturnSwapNotional()
		{
			returnSwapNotionalDeterminationMethod = new EFS_MultiLineString();
			returnSwapNotionalNotionalAmount = new Money();
			returnSwapNotionalAmountRelativeTo = new AmountReference();
		}
		#endregion Constructors

		#region IReturnSwapNotional Members
		bool IReturnSwapNotional.DeterminationMethodSpecified { get { return this.returnSwapNotionalDeterminationMethodSpecified; } }
		EFS_MultiLineString IReturnSwapNotional.DeterminationMethod { get { return this.returnSwapNotionalDeterminationMethod; } }
		bool IReturnSwapNotional.NotionalAmountSpecified { get { return this.returnSwapNotionalNotionalAmountSpecified; } }
		IMoney IReturnSwapNotional.NotionalAmount { get { return this.returnSwapNotionalNotionalAmount; } }
		bool IReturnSwapNotional.RelativeToSpecified { get { return this.returnSwapNotionalAmountRelativeToSpecified; } }
		IReference IReturnSwapNotional.RelativeTo { get { return this.returnSwapNotionalAmountRelativeTo; } }
		string IReturnSwapNotional.Id { get { return this.Id; } }
		#endregion IReturnSwapNotional Members

	}
	#endregion ReturnSwapNotional
	#region ReturnSwapPaymentDates
	public partial class ReturnSwapPaymentDates : IReturnSwapPaymentDates
	{
		#region IReturnSwapPaymentDates Members
		IAdjustableOrRelativeDate IReturnSwapPaymentDates.PaymentDateFinal {get {return this.paymentDateFinal ; }}
		bool IReturnSwapPaymentDates.PaymentDatesInterimSpecified { get { return this.paymentDatesInterimSpecified; } }
		IAdjustableOrRelativeDates IReturnSwapPaymentDates.PaymentDatesInterim { get { return this.paymentDatesInterim; } }
		#endregion IReturnSwapPaymentDates Members
	}
	#endregion ReturnSwapPaymentDates

	#region StartingDate
	public partial class StartingDate : IStartingDate
	{
		#region Constructors
		public StartingDate()
		{
			startingDateRelativeTo = new DateReference();
			startingDateAdjustableDate = new AdjustableDate();
		}
		#endregion Constructors

		#region IStartingDate Members
		bool IStartingDate.RelativeToSpecified
		{
			set {this.startingDateRelativeToSpecified = value;}
			get {return this.startingDateRelativeToSpecified;}
		}
		string IStartingDate.DateRelativeTo
		{
			set
			{
                this.startingDateRelativeTo = new DateReference
                {
                    href = value
                };
            }
			get { return this.startingDateRelativeTo.href; }
		}
		bool IStartingDate.AdjustableDateSpecified
		{
			get { return this.startingDateAdjustableDateSpecified; }
		}
		IAdjustableDate IStartingDate.AdjustableDate
		{
			get { return this.startingDateAdjustableDate; }
		}
		#endregion IStartingDate Members
	}
	#endregion StartingDate
	#region StubCalculationPeriod
	public partial class StubCalculationPeriod : IStubCalculationPeriod
	{
		#region IStubCalculationPeriod Members
		bool IStubCalculationPeriod.InitialStubSpecified { get { return this.initialStubSpecified; } }
		IStub IStubCalculationPeriod.InitialStub { get { return this.initialStub; } }
		bool IStubCalculationPeriod.FinalStubSpecified { get { return this.finalStubSpecified; } }
		IStub IStubCalculationPeriod.FinalStub { get { return this.finalStub; } }
		#endregion IStubCalculationPeriod Members
	}
	#endregion StubCalculationPeriod

}
