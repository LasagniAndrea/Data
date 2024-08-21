#region Using Directives
using System;
using System.Reflection;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EFS.ACommon;
using EFS.Common;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Interface;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Shared;
#endregion Using Directives

namespace FpML.v42.Ird
{
    #region BulletPayment
    public partial class BulletPayment : IProduct,IBulletPayment,IDeclarativeProvision
    {
        #region Accessors
        #region Payment
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object Payment
        {
            get
            {
                return payment;
            }
        }
        #endregion Payment
                
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_EventDate PaymentDate
        {
            get
            {
                return payment.PaymentDate;
            }
        }

        #endregion PaymentDate
        #endregion Accessors
		#region Constructors
		public BulletPayment()
		{
			payment = new Payment();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IBulletPayment Members
        IPayment IBulletPayment.payment { get { return this.payment; }}
		#endregion IBulletPayment Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
    #endregion BulletPayment

	#region Calculation
	public partial class Calculation : ICalculation
	{
		#region Constructors
		public Calculation()
		{
			calculationNotional = new Notional();
			calculationFxLinkedNotional = new FxLinkedNotionalSchedule();
			rateFixedRate = new Schedule();
			rateFloatingRate = new FloatingRateCalculation();
		}
		#endregion Constructors

		#region ICalculation Members
		bool ICalculation.notionalSpecified 
		{
			set { this.calculationNotionalSpecified = value;}
			get { return this.calculationNotionalSpecified;}
		}
		INotional ICalculation.notional {get { return this.calculationNotional;} }
		bool ICalculation.fxLinkedNotionalSpecified 
        { 
            set { this.calculationFxLinkedNotionalSpecified = value; } 
            get { return this.calculationFxLinkedNotionalSpecified; } 
        }
		IFxLinkedNotionalSchedule ICalculation.fxLinkedNotional 
        { 
            set { this.calculationFxLinkedNotional = (FxLinkedNotionalSchedule)value; } 
            get { return this.calculationFxLinkedNotional; } 
        }
		bool ICalculation.rateFixedRateSpecified 
		{ 
			set { this.rateFixedRateSpecified = value; } 
			get { return this.rateFixedRateSpecified; } 
		}
		ISchedule ICalculation.rateFixedRate { get { return this.rateFixedRate; } }
		bool ICalculation.rateFloatingRateSpecified 
		{ 
			set { this.rateFloatingRateSpecified = value; } 
			get { return rateFloatingRateSpecified; } 
		}
		IFloatingRateCalculation ICalculation.rateFloatingRate { get { return rateFloatingRate; } }
        bool ICalculation.rateInflationRateSpecified
        {
            set { ; }
            get { return false; }
        }
        IInflationRateCalculation ICalculation.rateInflationRate
        { 
            get { return null; } 
        }
        bool ICalculation.discountingSpecified { get { return this.discountingSpecified; } }
		IDiscounting ICalculation.discounting { get { return this.discounting; } }
		bool ICalculation.compoundingMethodSpecified { get { return this.compoundingMethodSpecified; } }
		CompoundingMethodEnum ICalculation.compoundingMethod { get { return this.compoundingMethod; } }
		DayCountFractionEnum ICalculation.dayCountFraction 
		{
			set { this.dayCountFraction = value; } 
			get { return this.dayCountFraction; } 
		}
		#endregion ICalculation Members
	}
	#endregion Calculation
	#region CalculationPeriodAmount
	public partial class CalculationPeriodAmount : ICalculationPeriodAmount
	{
		#region Constructors
		public CalculationPeriodAmount()
		{
			calculationPeriodAmountCalculation = new Calculation();
			calculationPeriodAmountKnownAmountSchedule = new KnownAmountSchedule();
		}
		#endregion Constructors

		#region ICalculationPeriodAmount Members
		bool ICalculationPeriodAmount.calculationSpecified 
		{
			set { this.calculationPeriodAmountCalculationSpecified = value;}
			get { return this.calculationPeriodAmountCalculationSpecified;}
		}
		ICalculation ICalculationPeriodAmount.calculation {get { return this.calculationPeriodAmountCalculation;} }
		bool ICalculationPeriodAmount.knownAmountScheduleSpecified 
		{ 
			set { this.calculationPeriodAmountKnownAmountScheduleSpecified = value; } 
			get { return this.calculationPeriodAmountKnownAmountScheduleSpecified; } 
		}
		IKnownAmountSchedule ICalculationPeriodAmount.knownAmountSchedule { get { return this.calculationPeriodAmountKnownAmountSchedule; } }
		#endregion ICalculationPeriodAmount Members
	}
	#endregion CalculationPeriodAmount
	#region CalculationPeriodDates
	public partial class CalculationPeriodDates : ICalculationPeriodDates
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_CalculationPeriodDates efs_CalculationPeriodDates;
		#endregion Members
		#region Accessors
		#region AdjustedEffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedEffectiveDate
		{
			get
			{
				EFS_Date adjustedEffectiveDate = new EFS_Date();
				adjustedEffectiveDate.DateValue = EffectiveDateAdjustment.adjustedDate.DateValue;
				return adjustedEffectiveDate;
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
				EFS_Date adjustedTerminationDate = new EFS_Date();
				adjustedTerminationDate.DateValue = TerminationDateAdjustment.adjustedDate.DateValue;
				return adjustedTerminationDate;
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
			get {return efs_CalculationPeriodDates.effectiveDateAdjustment; }
		}
		#endregion EffectiveDateAdjustment
		#region UnadjustedEffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date UnadjustedEffectiveDate
		{
			get
			{
				EFS_Date unadjustedEffectiveDate = new EFS_Date();
				unadjustedEffectiveDate.DateValue = EffectiveDateAdjustment.adjustableDate.unadjustedDate.DateValue;
				return unadjustedEffectiveDate;
            }
		}
		#endregion UnadjustedEffectiveDate
		#region UnadjustedTerminationDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date UnadjustedTerminationDate
		{
			get
			{
				EFS_Date unadjustedTerminationDate = new EFS_Date();
				unadjustedTerminationDate.DateValue = TerminationDateAdjustment.adjustableDate.unadjustedDate.DateValue;
				return unadjustedTerminationDate;
            }
		}
		#endregion UnadjustedTerminationDate
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
			get {return efs_CalculationPeriodDates.terminationDateAdjustment;}
		}
		#endregion TerminationDateAdjustment
		#endregion Accessors
		#region Constructors
		public CalculationPeriodDates()
		{
			firstRegularPeriodStartDate = new EFS_Date();
			lastRegularPeriodEndDate = new EFS_Date();
		}
		#endregion Constructors

		#region ICalculationPeriodDates Members
		EFS_CalculationPeriodDates ICalculationPeriodDates.efs_CalculationPeriodDates 
		{
			set { this.efs_CalculationPeriodDates = value;}
			get { return this.efs_CalculationPeriodDates;}
		}
		ICalculationPeriodFrequency ICalculationPeriodDates.calculationPeriodFrequency {get { return this.calculationPeriodFrequency;}}
		IBusinessDayAdjustments ICalculationPeriodDates.calculationPeriodDatesAdjustments 
        {
            set { this.calculationPeriodDatesAdjustments = (BusinessDayAdjustments)value; } 
            get { return this.calculationPeriodDatesAdjustments;} 
        }
		bool ICalculationPeriodDates.firstPeriodStartDateSpecified 
		{ 
			set { this.firstPeriodStartDateSpecified = value; } 
			get { return this.firstPeriodStartDateSpecified; } 
		}
		IAdjustableDate ICalculationPeriodDates.firstPeriodStartDate { get { return this.firstPeriodStartDate; } }
		bool ICalculationPeriodDates.firstRegularPeriodStartDateSpecified 
		{ 
			set { this.firstRegularPeriodStartDateSpecified = value; } 
			get { return this.firstRegularPeriodStartDateSpecified; } 
		}
		EFS_Date ICalculationPeriodDates.firstRegularPeriodStartDate 
		{
			set { this.firstRegularPeriodStartDate = value; } 
            get { return this.firstRegularPeriodStartDate; } 
		}
		bool ICalculationPeriodDates.lastRegularPeriodEndDateSpecified 
		{ 
			set { this.lastRegularPeriodEndDateSpecified = value; } 
			get { return this.lastRegularPeriodEndDateSpecified; } 
		}
		EFS_Date ICalculationPeriodDates.lastRegularPeriodEndDate 
		{
            set { this.lastRegularPeriodEndDate = value; } 
            get { return this.lastRegularPeriodEndDate; } 
		}
		bool ICalculationPeriodDates.effectiveDateAdjustableSpecified 
        { 
            set { ; } 
            get { return true; } 
        }
        IAdjustableDate ICalculationPeriodDates.effectiveDateAdjustable
        {
            set { this.effectiveDate = (AdjustableDate)value; }
            get { return this.effectiveDate; }
        }
        bool ICalculationPeriodDates.effectiveDateRelativeSpecified
        {
            get { return false; }
        }
        IAdjustedRelativeDateOffset ICalculationPeriodDates.effectiveDateRelative
        {
            get { return null; }
        }
		bool ICalculationPeriodDates.terminationDateAdjustableSpecified
        {
            set { ; }
            get { return true; }
        }
        IAdjustableDate ICalculationPeriodDates.terminationDateAdjustable
        {
            get { return this.terminationDate; }
            set { this.terminationDate = (AdjustableDate)value; }
        }
		bool ICalculationPeriodDates.terminationDateRelativeSpecified { get { return false; } }
		IRelativeDateOffset ICalculationPeriodDates.terminationDateRelative { get { return null; } }
		string ICalculationPeriodDates.id 
		{ 
			set { this.id = value; } 
			get { return this.id; } 
		}
		IRequiredIdentifierDate ICalculationPeriodDates.CreateRequiredIdentifierDate(DateTime pDate)
		{
			RequiredIdentifierDate requiredIdentifierDate = new RequiredIdentifierDate();
			requiredIdentifierDate.DateValue = pDate;
			return requiredIdentifierDate;
		}
		#endregion ICalculationPeriodDates Members
	}
	#endregion CalculationPeriodDates
    #region CapFloor
    // 20081107 EG Ticket 16394
    public partial class CapFloor : IProduct,ICapFloor,IDeclarativeProvision
    {
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:CapFloor";
		#endregion Members
        #region Accessors
        #region MinEffectiveDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MinEffectiveDate
        {
            get {return ((IInterestRateStream)capFloorStream).EffectiveDate;}
        }
        #endregion MinEffectiveDate
        #region MaxTerminationDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MaxTerminationDate
        {
            get { return ((IInterestRateStream)capFloorStream).TerminationDate; }
        }
        #endregion MaxTerminationDate
        #region Stream
        public InterestRateStream Stream
        {
            get { return capFloorStream; }
        }
        #endregion Stream
        #endregion Accessors
		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ICapFloor Members
		IInterestRateStream ICapFloor.stream { get { return this.capFloorStream; } }
		IInterestRateStream[] ICapFloor.streamInArray 
		{ 
			get
			{
				InterestRateStream[] streams = new InterestRateStream[1] { this.capFloorStream };
				return streams;
			}
		}
		bool ICapFloor.premiumSpecified 
		{ 
			set { this.premiumSpecified = value;}
			get { return this.premiumSpecified;}
		}
		IPayment[] ICapFloor.premium { get { return this.premium; } }
		bool ICapFloor.additionalPaymentSpecified 
		{ 
			set { this.additionalPaymentSpecified = value; } 
			get { return this.additionalPaymentSpecified; } 
		}
		IPayment[] ICapFloor.additionalPayment { get { return this.additionalPayment; } }
		bool ICapFloor.earlyTerminationProvisionSpecified
		{
			get { return this.earlyTerminationProvisionSpecified; }
		}
		IEarlyTerminationProvision ICapFloor.earlyTerminationProvision
		{
			get { return this.earlyTerminationProvision; }
		}
		bool ICapFloor.implicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
		IImplicitProvision ICapFloor.implicitProvision { get { return this.implicitProvision; } }
		bool ICapFloor.implicitCancelableProvisionSpecified
		{
			get { return false; }
		}
		bool ICapFloor.implicitOptionalEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.optionalEarlyTerminationProvisionSpecified); }
		}
		bool ICapFloor.implicitExtendibleProvisionSpecified
		{
			get { return false; }
		}
		bool ICapFloor.implicitMandatoryEarlyTerminationProvisionSpecified
		{
			get { return false; }
		}
		#endregion ICapFloor Members
		#region IDeclarativeProvision Members
        bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
        ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
        bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
        IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
        bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
        IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return this.earlyTerminationProvision; } }
        bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
        IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
        bool IDeclarativeProvision.implicitProvisionSpecified { get { return implicitProvisionSpecified; } }
        IImplicitProvision IDeclarativeProvision.implicitProvision { get { return implicitProvision; } }
		#endregion IDeclarativeProvision Members
	}
    #endregion CapFloor
	#region CancelableProvision
	public partial class CancelableProvision : ICancelableProvision
	{
		#region Accessors
		#region EFS_Exercise
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object EFS_Exercise
		{
			get
			{
				if (cancelableExerciseAmericanSpecified)
					return cancelableExerciseAmerican;
				else if (cancelableExerciseBermudaSpecified)
					return cancelableExerciseBermuda;
				else if (cancelableExerciseEuropeanSpecified)
					return cancelableExerciseEuropean;
				else
					return null;
			}
		}
		#endregion EFS_Exercise
		#endregion Accessors
		#region Constructors
		public CancelableProvision()
		{
			cancelableExerciseAmerican = new AmericanExercise();
			cancelableExerciseBermuda = new BermudaExercise();
			cancelableExerciseEuropean = new EuropeanExercise();
		}
		#endregion Constructors

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle 
		{
			get 
			{
				if (this.cancelableExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.cancelableExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		ICashSettlement IProvision.cashSettlement 
		{
			set { ; } 
			get { return null; } 
		}
		#endregion IProvision Members
        #region IExerciseProvision Members
        bool IExerciseProvision.americanSpecified
        {
            get { return this.cancelableExerciseAmericanSpecified; }
            set { this.cancelableExerciseAmericanSpecified = value; }
        }
        IAmericanExercise IExerciseProvision.american
        {
            get { return this.cancelableExerciseAmerican; }
            set { this.cancelableExerciseAmerican = (AmericanExercise)value; }
        }
        bool IExerciseProvision.bermudaSpecified
        {
            get { return this.cancelableExerciseBermudaSpecified; }
            set { this.cancelableExerciseBermudaSpecified = value; }
        }
        IBermudaExercise IExerciseProvision.bermuda
        {
            get { return this.cancelableExerciseBermuda; }
            set { this.cancelableExerciseBermuda = (BermudaExercise)value; }
        }
        bool IExerciseProvision.europeanSpecified
        {
            get { return this.cancelableExerciseEuropeanSpecified; }
            set { this.cancelableExerciseEuropeanSpecified = value; }
        }
        IEuropeanExercise IExerciseProvision.european
        {
            get { return this.cancelableExerciseEuropean; }
            set { this.cancelableExerciseEuropean = (EuropeanExercise)value; }
        }
        bool IExerciseProvision.exerciseNoticeSpecified
        {
            get { return this.exerciseNoticeSpecified; }
            set { this.exerciseNoticeSpecified = value; }
        }
        IExerciseNotice[] IExerciseProvision.exerciseNotice
        {
            get
            {
                ExerciseNotice[] notice = new ExerciseNotice[1];
                notice[1] = this.exerciseNotice;
                return notice;
            }
            set
            {
                ExerciseNotice[] notice = (ExerciseNotice[])value;
                this.exerciseNotice = notice[1];
            }
        }
        bool IExerciseProvision.followUpConfirmation
        {
            set { this.followUpConfirmation = new EFS_Boolean(value); }
            get { return this.followUpConfirmation.BoolValue; }
        }
        IAmericanExercise IExerciseProvision.CreateAmerican
        {
            get { return new AmericanExercise(); }
        }
        #endregion IExerciseProvision Members
		#region ICancelableProvision Members
		object ICancelableProvision.EFS_Exercise { get { return this.EFS_Exercise; } }
		IReference ICancelableProvision.buyerPartyReference
		{
            set { this.buyerPartyReference = (PartyReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference ICancelableProvision.sellerPartyReference
		{
            set { this.sellerPartyReference = (PartyReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_ExerciseDates ICancelableProvision.efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion ICancelableProvision Members
	}
	#endregion CancelableProvision
    #region CashPriceMethod
    public partial class CashPriceMethod : ICashPriceMethod
    {
        #region Constructors
        public CashPriceMethod()
        {
            cashSettlementReferenceBanks = new CashSettlementReferenceBanks();
            //cashSettlementReferenceBanksSpecified = false;
            cashSettlementCurrency = new Currency();
        }
        #endregion Constructors

        #region ICashPriceMethod Members
        bool ICashPriceMethod.cashSettlementReferenceBanksSpecified
        {
            set { this.cashSettlementReferenceBanksSpecified = value; }
            get {return this.cashSettlementReferenceBanksSpecified; }
        }
        ICurrency ICashPriceMethod.cashSettlementCurrency
        {
            set { this.cashSettlementCurrency = (Currency)value; }
            get {return this.cashSettlementCurrency;}
        }
        QuotationRateTypeEnum ICashPriceMethod.quotationRateType
        {
            set { this.quotationRateType = value; }
            get { return this.quotationRateType; }
        }
        #endregion ICashPriceMethod Members
    }
    #endregion CashPriceMethod

	#region CashSettlement
	public partial class CashSettlement : ICashSettlement
	{
		#region Constructors
		public CashSettlement()
		{
			cashSettlementMethodcashPriceMethod = new CashPriceMethod();
			cashSettlementMethodcashPriceAlternateMethod = new CashPriceMethod();
			cashSettlementMethodparYieldCurveAdjustedMethod = new YieldCurveMethod();
			cashSettlementMethodparYieldCurveUnadjustedMethod = new YieldCurveMethod();
			cashSettlementMethodzeroCouponYieldAdjustedMethod = new YieldCurveMethod();
            cashSettlementMethodNone = new Empty();
            cashSettlementMethodNoneSpecified = true;
		}
		#endregion Constructors

		#region ICashSettlement Members
		IBusinessCenterTime ICashSettlement.valuationTime 
		{
			set { this.cashSettlementValuationTime = (BusinessCenterTime)value; } 
			get { return this.cashSettlementValuationTime; } 
		}
		IRelativeDateOffset ICashSettlement.valuationDate 
		{
			set {this.cashSettlementValuationDate = (RelativeDateOffset)value;}
			get {return this.cashSettlementValuationDate;}
		}
		bool ICashSettlement.paymentDateSpecified
		{
			set { this.cashSettlementPaymentDateSpecified = value;}
			get { return this.cashSettlementPaymentDateSpecified;}
		}
		ICashSettlementPaymentDate ICashSettlement.paymentDate
		{
			set { this.cashSettlementPaymentDate = (CashSettlementPaymentDate)value;} 
			get { return this.cashSettlementPaymentDate;} 
		}
		ICashSettlementPaymentDate ICashSettlement.CreatePaymentDate 
		{
			get { return new CashSettlementPaymentDate(); } 
		}
        ICashPriceMethod ICashSettlement.CreateCashPriceMethod(string pCurrency, QuotationRateTypeEnum pQuotationRateType)
        {
            CashPriceMethod cashPriceMethod = new CashPriceMethod();
            cashPriceMethod.cashSettlementReferenceBanksSpecified = false;
            cashPriceMethod.cashSettlementCurrency = new Currency(pCurrency);
            cashPriceMethod.quotationRateType = pQuotationRateType;
            return cashPriceMethod;
        }
        string ICashSettlement.id
        {
            set { this.id = value; }
            get { return this.id; }
        }
        bool ICashSettlement.cashPriceMethodSpecified
        {
            set { this.cashSettlementMethodcashPriceMethodSpecified = value; }
            get { return this.cashSettlementMethodcashPriceMethodSpecified; }
        }
        ICashPriceMethod ICashSettlement.cashPriceMethod
        {
            set { this.cashSettlementMethodcashPriceMethod = (CashPriceMethod)value; }
            get { return this.cashSettlementMethodcashPriceMethod; }
        }
        bool ICashSettlement.cashPriceAlternateMethodSpecified
        {
            set { this.cashSettlementMethodcashPriceAlternateMethodSpecified = value; }
            get { return this.cashSettlementMethodcashPriceAlternateMethodSpecified; }
        }
        ICashPriceMethod ICashSettlement.cashPriceAlternateMethod
        {
            set { this.cashSettlementMethodcashPriceAlternateMethod = (CashPriceMethod)value; }
            get { return this.cashSettlementMethodcashPriceAlternateMethod; }
        }
        bool ICashSettlement.parYieldCurveAdjustedMethodSpecified
        {
            set { this.cashSettlementMethodparYieldCurveAdjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodparYieldCurveAdjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.parYieldCurveAdjustedMethod
        {
            set { this.cashSettlementMethodparYieldCurveAdjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodparYieldCurveAdjustedMethod; }
        }
        bool ICashSettlement.zeroCouponYieldAdjustedMethodSpecified
        {
            set { this.cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodzeroCouponYieldAdjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.zeroCouponYieldAdjustedMethod
        {
            set { this.cashSettlementMethodzeroCouponYieldAdjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodzeroCouponYieldAdjustedMethod; }
        }
        bool ICashSettlement.parYieldCurveUnadjustedMethodSpecified
        {
            set { this.cashSettlementMethodparYieldCurveUnadjustedMethodSpecified = value; }
            get { return this.cashSettlementMethodparYieldCurveUnadjustedMethodSpecified; }
        }
        IYieldCurveMethod ICashSettlement.parYieldCurveUnadjustedMethod
        {
            set { this.cashSettlementMethodparYieldCurveUnadjustedMethod = (YieldCurveMethod)value; }
            get { return this.cashSettlementMethodparYieldCurveUnadjustedMethod; }
        }
        #endregion ICashSettlement Members
	}
	#endregion CashSettlement
	#region CashSettlementPaymentDate
	public partial class CashSettlementPaymentDate : ICashSettlementPaymentDate
	{
		#region Constructors
		public CashSettlementPaymentDate()
		{
			paymentDateAdjustables = new AdjustableDates();
			paymentDateRelative = new RelativeDateOffset();
			paymentDateBusinessDateRange = new BusinessDateRange();
		}
		#endregion Constructors

		#region ICashSettlementPaymentDate Members
		bool ICashSettlementPaymentDate.adjustableDatesSpecified 
		{
			set { this.paymentDateAdjustablesSpecified = value; } 
			get { return this.paymentDateAdjustablesSpecified; } 
		}
		IAdjustableDates ICashSettlementPaymentDate.adjustableDates 
		{
			set { this.paymentDateAdjustables = (AdjustableDates)value; } 
			get {return this.paymentDateAdjustables ; } 
		}
		bool ICashSettlementPaymentDate.businessDateRangeSpecified 
		{
			set { this.paymentDateBusinessDateRangeSpecified = value; } 
			get {return this.paymentDateBusinessDateRangeSpecified ; } 
		}
		IBusinessDateRange ICashSettlementPaymentDate.businessDateRange 
		{
			set { this.paymentDateBusinessDateRange = (BusinessDateRange)value; } 
			get { return this.paymentDateBusinessDateRange; } 
		}
		bool ICashSettlementPaymentDate.relativeDateSpecified 
		{
			set { this.paymentDateRelativeSpecified = value; } 
			get {return this.paymentDateRelativeSpecified; } 
		}
		IRelativeDateOffset ICashSettlementPaymentDate.relativeDate 
		{
			set {this.paymentDateRelative = (RelativeDateOffset)value; } 
			get {return this.paymentDateRelative; } 
		}
        string ICashSettlementPaymentDate.id
        {
            set { this.id = value; }
            get { return this.id; }
        }                                
		#endregion ICashSettlementPaymentDate Members
	}
	#endregion CashSettlementPaymentDate

	#region Discounting
	public partial class Discounting : IDiscounting
	{
		#region Constructors
		public Discounting()
		{
			discountRate = new EFS_Decimal();
		}
		#endregion Constructors

		#region IDiscounting Members
		DiscountingTypeEnum IDiscounting.discountingType {get { return this.discountingType;} }
		bool IDiscounting.rateSpecified	{get { return this.discountRateSpecified;}}
		decimal IDiscounting.rate {get { return this.discountRate.DecValue;}}
		bool IDiscounting.dayCountFractionSpecified {get { return discountRateDayCountFractionSpecified;}}
		DayCountFractionEnum IDiscounting.dayCountFraction {get { return this.discountRateDayCountFraction;}}
		#endregion IDiscounting Members
	}
	#endregion Discounting
	#region EarlyTerminationProvision
	public partial class EarlyTerminationProvision : IEarlyTerminationProvision
	{
		#region Accessors
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		public string id
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
		public EarlyTerminationProvision()
		{
			earlyTerminationMandatory = new MandatoryEarlyTermination();
			earlyTerminationOptional = new OptionalEarlyTermination();
		}
		#endregion Constructors

		#region IEarlyTerminationProvision Members
		bool IEarlyTerminationProvision.mandatorySpecified { get { return this.earlyTerminationMandatorySpecified; } }
		IMandatoryEarlyTermination IEarlyTerminationProvision.mandatory { get { return this.earlyTerminationMandatory; } }
		bool IEarlyTerminationProvision.optionalSpecified { get { return this.earlyTerminationOptionalSpecified; } }
		IOptionalEarlyTermination IEarlyTerminationProvision.optional { get { return this.earlyTerminationOptional; } }
		#endregion IEarlyTerminationProvision Members
	}
	#endregion EarlyTerminationProvision
	#region ExtendibleProvision
	public partial class ExtendibleProvision : IExtendibleProvision
	{
		#region Accessors
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object EFS_Exercise
		{
			get
			{
				if (extendibleExerciseAmericanSpecified)
					return extendibleExerciseAmerican;
				else if (extendibleExerciseBermudaSpecified)
					return extendibleExerciseBermuda;
				else if (extendibleExerciseEuropeanSpecified)
					return extendibleExerciseEuropean;
				else
					return null;
			}
		}
		#endregion Accessors
		#region Constructors
		public ExtendibleProvision()
		{
			extendibleExerciseAmerican = new AmericanExercise();
			extendibleExerciseBermuda = new BermudaExercise();
			extendibleExerciseEuropean = new EuropeanExercise();
		}
		#endregion Constructors

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle
		{
			get
			{
				if (this.extendibleExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.extendibleExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		ICashSettlement IProvision.cashSettlement 
		{
			set { ;}
			get { return null; } 
		}
		#endregion IProvision Members
        #region IExerciseProvision Members
        bool IExerciseProvision.americanSpecified
        {
            get { return this.extendibleExerciseAmericanSpecified; }
            set { this.extendibleExerciseAmericanSpecified = value; }
        }
        IAmericanExercise IExerciseProvision.american
        {
            get { return this.extendibleExerciseAmerican; }
            set { this.extendibleExerciseAmerican = (AmericanExercise)value; }
        }
        bool IExerciseProvision.bermudaSpecified
        {
            get { return this.extendibleExerciseBermudaSpecified; }
            set { this.extendibleExerciseBermudaSpecified = value; }
        }
        IBermudaExercise IExerciseProvision.bermuda
        {
            get { return this.extendibleExerciseBermuda; }
            set { this.extendibleExerciseBermuda = (BermudaExercise)value; }
        }
        bool IExerciseProvision.europeanSpecified
        {
            get { return this.extendibleExerciseEuropeanSpecified; }
            set { this.extendibleExerciseEuropeanSpecified = value; }
        }
        IEuropeanExercise IExerciseProvision.european
        {
            get { return this.extendibleExerciseEuropean; }
            set { this.extendibleExerciseEuropean = (EuropeanExercise)value; }
        }
        bool IExerciseProvision.exerciseNoticeSpecified
        {
            get { return this.exerciseNoticeSpecified; }
            set { this.exerciseNoticeSpecified = value; }
        }
        IExerciseNotice[] IExerciseProvision.exerciseNotice
        {
            get
            {
                ExerciseNotice[] notice = new ExerciseNotice[1];
                notice[1] = this.exerciseNotice;
                return notice;
            }
            set
            {
                ExerciseNotice[] notice = (ExerciseNotice[])value;
                this.exerciseNotice = notice[1];
            }
        }
        bool IExerciseProvision.followUpConfirmation
        {
            set { this.followUpConfirmation = new EFS_Boolean(value); }
            get { return this.followUpConfirmation.BoolValue; }
        }
        IAmericanExercise IExerciseProvision.CreateAmerican
        {
            get { return new AmericanExercise(); }
        }
        #endregion IExerciseProvision Members
		#region IExtendibleProvision Members
		object IExtendibleProvision.EFS_Exercise { get { return this.EFS_Exercise; } }
		IReference IExtendibleProvision.buyerPartyReference
		{
            set { this.buyerPartyReference = (PartyReference) value; }
			get { return this.buyerPartyReference; }
		}
		IReference IExtendibleProvision.sellerPartyReference
		{
            set { this.sellerPartyReference = (PartyReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_ExerciseDates IExtendibleProvision.efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion IExtendibleProvision Members

	}
	#endregion ExtendibleProvision

    #region Fra
    public partial class Fra : IProduct,IFra,IDeclarativeProvision
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_FraDates efs_FraDates;
        #endregion Members
        #region Accessors
        #region AdjustedEffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedEffectiveDate
        {
            get{return new EFS_Date(adjustedEffectiveDate.Value); }
        }
        #endregion AdjustedEffectiveDate
        #region AdjustedFixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedFixingDate
        {
            get
            {
				EFS_Date adjustedFixingDate = new EFS_Date();
				adjustedFixingDate.DateValue = efs_FraDates.fixingDateAdjustment.adjustedDate.DateValue;
				return adjustedFixingDate;
            }
        }
        #endregion AdjustedFixingDate
        #region AdjustedPaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedPaymentDate
        {
            get
            {
				EFS_Date adjustedPaymentDate = new EFS_Date();
				adjustedPaymentDate.DateValue = efs_FraDates.paymentDateAdjustment.adjustedDate.DateValue;
				return adjustedPaymentDate;
            }
        }

        #endregion AdjustedPaymentDate
        #region AdjustedPreSettlementDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedPreSettlementDate
        {
            get {return efs_FraDates.AdjustedPreSettlementDate;}
        }
        #endregion AdjustedPreSettlementDate
        #region AdjustedTerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedTerminationDate
        {
            get {return new EFS_Date(adjustedTerminationDate.Value); }
        }
        #endregion AdjustedTerminationDate
        #region BuyerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string BuyerPartyReference
        {
            get {return buyerPartyReference.href;}
        }
        #endregion BuyerPartyReference
        #region Currency
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string Currency
        {
            get {return this.notional.currency.Value;}
        }
        #endregion Currency
        #region DayCountFraction
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_DayCountFraction DayCountFraction
        {
            get
            {
                DateTime startDate = EffectiveDate.adjustedDate.DateValue;
                DateTime endDate = TerminationDate.adjustedDate.DateValue;
				Interval interval = new Interval(String.Empty, 0);
                return new EFS_DayCountFraction(startDate, endDate, dayCountFraction,(IInterval) interval);
            }
        }
        #endregion DayCountFraction
        #region EffectiveDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate EffectiveDate
        {
            get {return new EFS_EventDate(adjustedEffectiveDate.DateValue, adjustedEffectiveDate.DateValue);}
        }

        #endregion EffectiveDate
        #region FixingDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate FixingDate
        {
            get {return new EFS_EventDate(efs_FraDates.fixingDateAdjustment);}
        }
        #endregion FixingDate
        #region FloatingRate
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Asset AssetRateIndex
        {
            get
            {
                EFS_Asset asset = new EFS_Asset();
                asset.idAsset = floatingRateIndex.OTCmlId;
                asset.assetCategory = Cst.UnderlyingAsset.RateIndex;
                return asset;
            }
        }
        #endregion FloatingRate
        #region Notional
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Decimal Notional
        {
            get
            {
                if (null != this.notional)
                    return this.notional.amount;
                else
                    return null;
            }
        }

        #endregion Notional
        #region PaymentDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate PaymentDate
        {
            get{return new EFS_EventDate(efs_FraDates.paymentDateAdjustment);}
        }

        #endregion PaymentDate
        #region SellerPartyReference
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public string SellerPartyReference
        {
            get{return sellerPartyReference.href;}
        }
        #endregion SellerPartyReference
        #region TerminationDate
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate TerminationDate
        {
            get{return new EFS_EventDate(adjustedTerminationDate.DateValue, adjustedTerminationDate.DateValue);}
        }
        #endregion TerminationDate
        #endregion Accessors
        
        #region constructor
        public Fra()
        {
            indexTenor = (Interval[])((IProductBase)this).CreateIntervals();   
        }
        #endregion

        #region IProduct Members
        object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region IFra Members
		IReference IFra.buyerPartyReference {get { return buyerPartyReference; }}
		IReference IFra.sellerPartyReference { get { return sellerPartyReference; } }
		IRequiredIdentifierDate IFra.adjustedEffectiveDate
		{
			set { this.adjustedEffectiveDate = (RequiredIdentifierDate)value; }
			get { return this.adjustedEffectiveDate; }
		}
		EFS_Date IFra.adjustedTerminationDate
		{
			set { this.adjustedTerminationDate = value; }
			get { return this.adjustedTerminationDate; }
		}
		EFS_Decimal IFra.fixedRate
		{
			set { this.fixedRate = value; }
			get { return this.fixedRate; }
		}
		IFloatingRateIndex IFra.floatingRateIndex
		{
			set { this.floatingRateIndex = (FloatingRateIndex)value; }
			get { return this.floatingRateIndex; }
		}
		IMoney IFra.notional 
		{ 
			get { return this.notional; } 
		}
        bool IFra.indexTenorSpecified
        {
            get { return true; }
        }
        IInterval[] IFra.indexTenor
        {
            set { this.indexTenor = (Interval[])value; }
            get { return this.indexTenor; }
        }
        IInterval IFra.firstIndexTenor
        {
            set { this.indexTenor[0] = (Interval)value; }
            get { return this.indexTenor[0]; }
        }
		IAdjustableDate IFra.paymentDate 
		{
			get { return this.paymentDate; } 
		}
		IRelativeDateOffset IFra.fixingDateOffset 
		{ 
			get { return this.fixingDateOffset; } 
		}
		DayCountFractionEnum IFra.dayCountFraction 
		{ 
			set { this.dayCountFraction = value; } 
			get { return this.dayCountFraction; } 
		}
		EFS_PosInteger IFra.calculationPeriodNumberOfDays
		{
			set { this.calculationPeriodNumberOfDays = value; } 
			get { return this.calculationPeriodNumberOfDays; } 
		}
		FraDiscountingEnum IFra.fraDiscounting 
		{ 
			get { return this.fraDiscounting; } 
		}
		EFS_FraDates IFra.efs_FraDates
		{
			get { return this.efs_FraDates; }
			set { this.efs_FraDates = value; }
		}
		#endregion IFra Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return false; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return null; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return false; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return null; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return false; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return null; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return null; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
    #endregion Fra
	#region FxLinkedNotionalSchedule
	public partial class FxLinkedNotionalSchedule : IFxLinkedNotionalSchedule
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_FxLinkedNotionalDates efs_FxLinkedNotionalDates;
		#endregion Members
        #region constructors
        public FxLinkedNotionalSchedule()
        {
            this.constantNotionalScheduleReference = new NotionalReference();
        }
        #endregion constructors

        #region IFxLinkedNotionalSchedule Members
        EFS_FxLinkedNotionalDates IFxLinkedNotionalSchedule.efs_FxLinkedNotionalDates
		{
			get { return this.efs_FxLinkedNotionalDates; }
			set { this.efs_FxLinkedNotionalDates = value; }
		}
		string IFxLinkedNotionalSchedule.currency 
        {
            set { this.currency.Value = value; } 
            get { return this.currency.Value; } 
        }
		IRelativeDateOffset IFxLinkedNotionalSchedule.varyingNotionalInterimExchangePaymentDates 
        { 
            set { this.varyingNotionalInterimExchangePaymentDates = (RelativeDateOffset)value; } 
            get { return this.varyingNotionalInterimExchangePaymentDates; } 
        }
		IRelativeDateOffset IFxLinkedNotionalSchedule.varyingNotionalFixingDates 
        {
            set { this.varyingNotionalFixingDates = (RelativeDateOffset)value; }
            get { return this.varyingNotionalFixingDates; } 
        }
		IReference IFxLinkedNotionalSchedule.constantNotionalScheduleReference 
        {
            set {this.constantNotionalScheduleReference = (NotionalReference)value;}
            get { return this.constantNotionalScheduleReference; } 
        }
		bool IFxLinkedNotionalSchedule.initialValueSpecified 
        { 
            set { this.initialValueSpecified = value; } 
            get { return this.initialValueSpecified; } 
        }
		EFS_Decimal IFxLinkedNotionalSchedule.initialValue 
        { 
            set { this.initialValue = value; } 
            get { return this.initialValue; } 
        }
		IFxSpotRateSource IFxLinkedNotionalSchedule.fxSpotRateSource 
        { 
            set { this.fxSpotRateSource = (FxSpotRateSource)value; } 
            get { return this.fxSpotRateSource; } 
        }
        SQL_AssetFxRate IFxLinkedNotionalSchedule.GetSqlAssetFxRate(string pConnectionString)
        {
            SQL_AssetFxRate ret = null;
            if (null != fxSpotRateSource)
            {
                int idFxRate = fxSpotRateSource.primaryRateSource.OTCmlId;
                {
                    ret = new SQL_AssetFxRate(pConnectionString, idFxRate);
                    ret.LoadTable();
                }
            }
            return ret;
        }
		#endregion IFxLinkedNotionalSchedule Members
	}
	#endregion FxLinkedNotionalSchedule

	#region InterestRateStream
	public partial class InterestRateStream : IEFS_Array, IInterestRateStream
	{
		#region Members
        /*
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.None)]
        public bool streamExtensionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("streamExtension",Namespace="http://www.efs.org/2005/EFSmL-2-0")]
        [CreateControlGUI(Declare=MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level=MethodsGUI.LevelEnum.Intermediary,Name="Extensions...")]
        public StreamExtension streamExtension;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace="http://www.w3.org/2001/XMLSchema-instance",
			 Form=System.Xml.Schema.XmlSchemaForm.Qualified, DataType="normalizedString")]
		public string type="efs:InterestRateStream";
		*/
        #endregion Members
		#region Constructors
		public InterestRateStream()
		{
			payerPartyReference = new PartyReference();
			receiverPartyReference = new PartyReference();
			calculationPeriodDates = new CalculationPeriodDates();
			paymentDates = new PaymentDates();
			resetDates = new ResetDates();
			calculationPeriodAmount = new CalculationPeriodAmount();
			stubCalculationPeriodAmount = new StubCalculationPeriodAmount();
			principalExchanges = new PrincipalExchanges();
			//streamExtension             = new StreamExtension();
		}
		#endregion Constructors
        #region Methods
        #region EventType
        // 20090828 EG Add EventType
        public string EventType(string pProduct)
        {
            string eventType = "???";
            if ("Swap" == pProduct)
            {
                if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    Calculation calculation = this.calculationPeriodAmount.calculationPeriodAmountCalculation;
                    if (calculation.rateFixedRateSpecified)
                        eventType = EventTypeFunc.FixedRate;
                    else if (calculation.rateFloatingRateSpecified)
                        eventType = EventTypeFunc.FloatingRate;
                }
                else if (this.calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                    eventType = EventTypeFunc.KnownAmount;
            }
            else if ("CapFloor" == pProduct)
            {
                bool isCap = false;
                bool isFloor = false;
                //20090827 PL Add and Use countStrikeSchedule
                int countStrikeSchedule = 0;
                if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    Calculation calculation = this.calculationPeriodAmount.calculationPeriodAmountCalculation;
                    if (calculation.rateFloatingRateSpecified)
                    {
                        isCap = calculation.rateFloatingRate.capRateScheduleSpecified;
                        if (isCap)
                            countStrikeSchedule += calculation.rateFloatingRate.capRateSchedule.Length;
                        isFloor = calculation.rateFloatingRate.floorRateScheduleSpecified;
                        if (isFloor)
                            countStrikeSchedule += calculation.rateFloatingRate.floorRateSchedule.Length;
                    }
                }
                if ((isCap && isFloor) || (countStrikeSchedule > 1))
                    eventType = EventTypeFunc.CapFloorMultiStrikeSchedule;
                else if (isCap)
                    eventType = EventTypeFunc.Cap;
                else if (isFloor)
                    eventType = EventTypeFunc.Floor;
            }
            return eventType;
        }
        #endregion EventType
        #region GetCurrency
        public string GetCurrency()
        {
            string ret = string.Empty;
            if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
            {
                if (calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotionalSpecified)
                    ret = calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional.currency.Value;
                else if (calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotionalSpecified)
                    ret = calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional.notionalStepSchedule.currency.Value;
            }
            else if (calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                ret = calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.currency.Value;
            return ret;
        }
        #endregion GetCurrency
        #region Rate
        /// <revision>
        ///     <version>2.0.1</version><date>20080408</date><author>EG</author>
        ///     <comment>
        ///     Ticket : 156165 Initial & Final Stub : Wrong event type when one of rate not specified
        ///     </comment>
        /// </revision>
        // EG 20180423 Analyse du code Correction [CA2200]
        public object Rate(string pStub)
        {
            try
            {
                object rate = null;
                if ((StubEnum.None.ToString() == pStub) || (false == this.stubCalculationPeriodAmountSpecified))
                {
                    if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                    {
                        Calculation calculation = this.calculationPeriodAmount.calculationPeriodAmountCalculation;
                        if (calculation.rateFixedRateSpecified)
                            rate = calculation.rateFixedRate;
                        else if (calculation.rateFloatingRateSpecified)
                            rate = calculation.rateFloatingRate;
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
        #endregion Rate
        #endregion Methods

        #region IEFS_Array Members
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IInterestRateStream Members
		IReference IInterestRateStream.payerPartyReference 
        {
            set { this.payerPartyReference = (PartyReference)value; } 
            get { return this.payerPartyReference; } 
        }
        IReference IInterestRateStream.receiverPartyReference
        {
            set { this.receiverPartyReference = (PartyReference)value; } 
            get { return this.receiverPartyReference; }
        }
		ICalculationPeriodAmount IInterestRateStream.calculationPeriodAmount { get { return this.calculationPeriodAmount; } }
		bool IInterestRateStream.stubCalculationPeriodAmountSpecified 
		{
			set { this.stubCalculationPeriodAmountSpecified = value; } 
			get { return this.stubCalculationPeriodAmountSpecified; } 
		}
		IStubCalculationPeriodAmount IInterestRateStream.stubCalculationPeriodAmount 
		{ 
			set { this.stubCalculationPeriodAmount = (StubCalculationPeriodAmount)value ; } 
			get { return this.stubCalculationPeriodAmount; } 
		}
		bool IInterestRateStream.resetDatesSpecified
		{
			set { this.resetDatesSpecified = value; }
			get { return this.resetDatesSpecified; }
		}
		ICalculationPeriodDates IInterestRateStream.calculationPeriodDates {
			get { return this.calculationPeriodDates; }
			set {this.calculationPeriodDates = (CalculationPeriodDates) value;}
		}
		IPaymentDates IInterestRateStream.paymentDates
		{
			get { return this.paymentDates; }
			set { this.paymentDates = (PaymentDates)value; }
		}
		IResetDates IInterestRateStream.resetDates
		{
			get { return this.resetDates; }
			set { this.resetDates = (ResetDates)value; }
		}
		string IInterestRateStream.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
		bool IInterestRateStream.principalExchangesSpecified
		{
			set { this.principalExchangesSpecified = value; }
			get { return this.principalExchangesSpecified; }
		}
		IPrincipalExchanges IInterestRateStream.principalExchanges
		{
			get { return this.principalExchanges; }
		}
		IRelativeDateOffset IInterestRateStream.CreateRelativeDateOffset() {  return new RelativeDateOffset();  }
		IResetFrequency IInterestRateStream.CreateResetFrequency () {   return new ResetFrequency(); } 
		IResetDates IInterestRateStream.CreateResetDates() {  return new ResetDates();  }
		IInterval IInterestRateStream.CreateInterval() { return new Interval(); } 
		IOffset IInterestRateStream.CreateOffset() {  return new Offset();  }
		IStubCalculationPeriodAmount IInterestRateStream.CreateStubCalculationPeriodAmount() {  return new StubCalculationPeriodAmount(); }
		IBusinessDayAdjustments IInterestRateStream.CreateBusinessDayAdjustments(BusinessDayConventionEnum pBusinessDayConvention, string pIdBC)
		{
			BusinessDayAdjustments bda = new BusinessDayAdjustments();
			bda.businessDayConvention = pBusinessDayConvention;
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
                EFS_Date dt = new EFS_Date();
                dt.DateValue = ((IInterestRateStream)this).EffectiveDateAdjustment.adjustedDate.DateValue;
                return dt;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.AdjustedTerminationDate
        {
            get
            {
                EFS_Date dt = new EFS_Date();
                dt.DateValue = ((IInterestRateStream)this).TerminationDateAdjustment.adjustedDate.DateValue;
                return dt;
            }

        }
        // EG 20180423 Analyse du code Correction [CA2200]
        object IInterestRateStream.CalculationPeriodAmount
        {
            get
            {
                if (calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                {
                    if (calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotionalSpecified)
                        return calculationPeriodAmount.calculationPeriodAmountCalculation.calculationNotional;
                    else if (calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotionalSpecified)
                        return calculationPeriodAmount.calculationPeriodAmountCalculation.calculationFxLinkedNotional;
                }
                else if (calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified)
                    return calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule;
                return null;
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
            get{ return new EFS_EventDate(((IInterestRateStream)this).EffectiveDateAdjustment); }
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
                    Calculation calculation = calculationPeriodAmount.calculationPeriodAmountCalculation;
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
                    Calculation calculation = calculationPeriodAmount.calculationPeriodAmountCalculation;
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
        string IInterestRateStream.PayerPartyReference
        {
            get { return payerPartyReference.href; }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.ReceiverPartyReference
        {
            get { return receiverPartyReference.href; }
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
            get{return new EFS_EventDate(((IInterestRateStream)this).TerminationDateAdjustment); }
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
                EFS_Date dt = new EFS_Date();
                dt.DateValue = ((IInterestRateStream)this).EffectiveDateAdjustment.adjustableDate.unadjustedDate.DateValue;
                return dt;
            }
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        EFS_Date IInterestRateStream.UnadjustedTerminationDate
        {
            get
            {
                EFS_Date dt = new EFS_Date();
                dt.DateValue = ((IInterestRateStream)this).TerminationDateAdjustment.adjustableDate.unadjustedDate.DateValue;
                return dt;
            }
        }
        string IInterestRateStream.GetCurrency
        {
            get {return this.GetCurrency();}
        }
        string IInterestRateStream.EventType(string pProduct)
        {
            return ((IInterestRateStream)this).EventType2(pProduct, false);
        }
        // 20090828 EG Call method EventType
        string IInterestRateStream.EventType2(string pProduct, bool pIsDiscount)
        {
            string eventType = "???";
            if (pIsDiscount)
                eventType = EventTypeFunc.ZeroCoupon;
            else
                eventType = this.EventType(pProduct);
            return eventType;
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        object IInterestRateStream.Rate(string pStub)
        {
            return this.Rate(pStub);
        }
        #endregion IInterestRateStream Members
	}
	#endregion InterestRateStream

	#region MandatoryEarlyTermination
	public partial class MandatoryEarlyTermination : IProvision, IMandatoryEarlyTermination
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_MandatoryEarlyTerminationDates efs_MandatoryEarlyTerminationDates;
		#endregion Members
		#region Accessors
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
		public string id
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

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle {get {return ExerciseStyleEnum.European;}}
		ICashSettlement IProvision.cashSettlement 
		{
			set { this.cashSettlement = (CashSettlement)value; }
			get { return this.cashSettlement; }
		}
		#endregion
		#region IMandatoryEarlyTermination Members
		IAdjustableDate IMandatoryEarlyTermination.mandatoryEarlyTerminationDate {get { return this.mandatoryEarlyTerminationDate; }}
		ICashSettlement IMandatoryEarlyTermination.cashSettlement {get { return this.cashSettlement;}}
		bool IMandatoryEarlyTermination.adjustedDatesSpecified {get { return this.mandatoryEarlyTerminationAdjustedDatesSpecified;}}
		IMandatoryEarlyTerminationAdjustedDates IMandatoryEarlyTermination.adjustedDates {get { return this.mandatoryEarlyTerminationAdjustedDates;} }
		EFS_MandatoryEarlyTerminationDates IMandatoryEarlyTermination.efs_MandatoryEarlyTerminationDates
		{
			get { return this.efs_MandatoryEarlyTerminationDates; }
			set { this.efs_MandatoryEarlyTerminationDates = value; }
		}
		#endregion IMandatoryEarlyTermination Members
	}
	#endregion MandatoryEarlyTermination
	#region MandatoryEarlyTerminationAdjustedDates
	public partial class MandatoryEarlyTerminationAdjustedDates : IMandatoryEarlyTerminationAdjustedDates
	{
		#region IMandatoryEarlyTerminationAdjustedDates Members
		DateTime IMandatoryEarlyTerminationAdjustedDates.adjustedEarlyTerminationDate { get { return this.adjustedEarlyTerminationDate.DateValue; } }
		DateTime IMandatoryEarlyTerminationAdjustedDates.adjustedCashSettlementPaymentDate { get { return this.adjustedCashSettlementPaymentDate.DateValue; } }
		DateTime IMandatoryEarlyTerminationAdjustedDates.adjustedCashSettlementValuationDate { get { return this.adjustedCashSettlementValuationDate.DateValue; } }
		#endregion IMandatoryEarlyTerminationAdjustedDates Members
	}
	#endregion MandatoryEarlyTerminationAdjustedDates

	#region Notional
	public partial class Notional : INotional
	{

        #region constructor
        public Notional()
        {
            notionalStepSchedule = new AmountSchedule();
            notionalStepParameters = new NotionalStepRule();
        }
        #endregion

        #region INotional Members
		IAmountSchedule INotional.stepSchedule {get { return this.notionalStepSchedule;}}
		bool INotional.stepParametersSpecified {get {return this.notionalStepParametersSpecified;}}
		INotionalStepRule INotional.stepParameters { get { return this.notionalStepParameters; } }
		string INotional.id
		{
			get { return this.id; }
			set { this.id = value; }
        }
        #endregion INotional Members
    }
	#endregion Notional

	#region NotionalStepRule
	public partial class NotionalStepRule : INotionalStepRule
	{
		#region Members
		// 20071130 EG Ticket 15998
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Step efs_FirstStepDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Step efs_LastStepDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Step efs_VirtualLastStepDate;
		#endregion Members
		#region Accessors
		#region IsStepDateCalculated
		// 20071130 EG Ticket 15998
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public bool IsStepDateCalculated
		{
			get { return (null != efs_FirstStepDate) && (null != efs_LastStepDate) && (null != efs_VirtualLastStepDate); }
		}
		#endregion IsStepDateCalculated
		#endregion Accessors
		#region Methods
		#region CalcAdjustableStepDate
		// 20071130 EG Ticket 15998
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel CalcAdjustableStepDate(string pConnectionString, DateTime pTerminationDate,
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            decimal stepValue = 0;
            if (notionalStepAmountSpecified)
                stepValue = notionalStepAmount.DecValue;
            else if (notionalStepRateSpecified)
                stepValue = notionalStepRate.DecValue;
            efs_FirstStepDate = new EFS_Step(pConnectionString, firstNotionalStepDate.DateValue, stepValue, pBusinessDayAdjustments, pDataDocument);
            efs_LastStepDate = new EFS_Step(pConnectionString, lastNotionalStepDate.DateValue, stepValue, pBusinessDayAdjustments, pDataDocument);
            // To include the real LastStepDate
            DateTime virtualLastStepDate = Tools.ApplyInterval(lastNotionalStepDate.DateValue, pTerminationDate, stepFrequency);
            efs_VirtualLastStepDate = new EFS_Step(pConnectionString, virtualLastStepDate, stepValue, pBusinessDayAdjustments, pDataDocument);
            ret = Cst.ErrLevel.SUCCESS;

            return ret;

        }
		#endregion CalcAdjustableStepDate
		#endregion Methods

		#region INotionalStepRule Members
		bool INotionalStepRule.IsStepDateCalculated { get { return this.IsStepDateCalculated; } }
		EFS_Step INotionalStepRule.efs_FirstStepDate { get { return this.efs_FirstStepDate; } }
		EFS_Step INotionalStepRule.efs_LastStepDate { get { return this.efs_LastStepDate; } }
		EFS_Step INotionalStepRule.efs_VirtualLastStepDate { get { return this.efs_VirtualLastStepDate; } }
		IInterval INotionalStepRule.stepFrequency { get { return this.stepFrequency; } }
		bool INotionalStepRule.notionalStepAmountSpecified { get { return this.notionalStepAmountSpecified; } }
		decimal INotionalStepRule.notionalStepAmount { get { return this.notionalStepAmount.DecValue; } }
		bool INotionalStepRule.notionalStepRateSpecified { get { return this.notionalStepRateSpecified; } }
		bool INotionalStepRule.stepRelativeToSpecified { get { return this.stepRelativeToSpecified; } }
		StepRelativeToEnum INotionalStepRule.stepRelativeTo { get { return this.stepRelativeTo; } }
		decimal INotionalStepRule.notionalStepRate { get { return this.notionalStepRate.DecValue; } }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel INotionalStepRule.CalcAdjustableStepDate(string pCs, DateTime pTerminationDate, 
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
		{
			return this.CalcAdjustableStepDate(pCs, pTerminationDate, pBusinessDayAdjustments, pDataDocument);
		}
		#endregion INotionalStepRule Members

	}
	#endregion NotionalStepRule

	#region OptionalEarlyTermination
	public partial class OptionalEarlyTermination : IOptionalEarlyTermination
	{
		#region Accessors
		#region EFS_Exercise
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public object EFS_Exercise
		{
			get
			{
				if (optionalEarlyTerminationExerciseAmericanSpecified)
					return optionalEarlyTerminationExerciseAmerican;
				else if (optionalEarlyTerminationExerciseBermudaSpecified)
					return optionalEarlyTerminationExerciseBermuda;
				else if (optionalEarlyTerminationExerciseEuropeanSpecified)
					return optionalEarlyTerminationExerciseEuropean;
				else
					return null;
			}
		}
		#endregion EFS_Exercise
		#endregion Accessors
		#region Constructors
		public OptionalEarlyTermination()
		{
			optionalEarlyTerminationExerciseAmerican = new AmericanExercise();
			optionalEarlyTerminationExerciseBermuda = new BermudaExercise();
			optionalEarlyTerminationExerciseEuropean = new EuropeanExercise();
		}
		#endregion Constructors

		#region IProvision Members
		ExerciseStyleEnum IProvision.GetStyle
		{
			get
			{
				if (this.optionalEarlyTerminationExerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.optionalEarlyTerminationExerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		ICashSettlement IProvision.cashSettlement
		{
			set { this.cashSettlement = (CashSettlement)value; } 
			get { return this.cashSettlement; } 
		}
		#endregion IProvision Members
		#region IOptionalEarlyTermination Members
        ICalculationAgent IOptionalEarlyTermination.calculationAgent 
        { 
            set {this.calculationAgent = (CalculationAgent)value;}
            get { return this.calculationAgent; }
        }
		EFS_ExerciseDates IOptionalEarlyTermination.efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion IOptionalEarlyTermination Members
		#region IExerciseProvision Members
		bool IExerciseProvision.americanSpecified
		{
			get {return this.optionalEarlyTerminationExerciseAmericanSpecified;}
			set { this.optionalEarlyTerminationExerciseAmericanSpecified = value; }
		}
		IAmericanExercise IExerciseProvision.american
		{
			get { return this.optionalEarlyTerminationExerciseAmerican; }
			set { this.optionalEarlyTerminationExerciseAmerican = (AmericanExercise) value; }
		}
		bool IExerciseProvision.bermudaSpecified
		{
			get { return this.optionalEarlyTerminationExerciseBermudaSpecified; }
			set { this.optionalEarlyTerminationExerciseBermudaSpecified = value; }
		}
		IBermudaExercise IExerciseProvision.bermuda
		{
			get { return this.optionalEarlyTerminationExerciseBermuda; }
			set { this.optionalEarlyTerminationExerciseBermuda = (BermudaExercise)value; }
		}
		bool IExerciseProvision.europeanSpecified
		{
			get { return this.optionalEarlyTerminationExerciseEuropeanSpecified; }
			set { this.optionalEarlyTerminationExerciseEuropeanSpecified = value; }
		}
		IEuropeanExercise IExerciseProvision.european
		{
			get { return this.optionalEarlyTerminationExerciseEuropean; }
			set { this.optionalEarlyTerminationExerciseEuropean = (EuropeanExercise)value; }
		}
		bool IExerciseProvision.exerciseNoticeSpecified
		{
			get { return this.exerciseNoticeSpecified; }
			set { this.exerciseNoticeSpecified = value; }
		}
		IExerciseNotice[] IExerciseProvision.exerciseNotice
		{
			get { return this.exerciseNotice; }
			set { this.exerciseNotice = (ExerciseNotice[])value; }
		}
        bool IExerciseProvision.followUpConfirmation
        {
            set { this.followUpConfirmation = new EFS_Boolean(value); }
            get { return this.followUpConfirmation.BoolValue; }
        }
        IAmericanExercise IExerciseProvision.CreateAmerican 
        {
            get { return new AmericanExercise(); }
        }
		#endregion IExerciseProvision Members
	}
	#endregion OptionalEarlyTermination

	#region PaymentDates
    // EG 20140702 Upd Interface
	public partial class PaymentDates : IPaymentDates
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_PaymentDates efs_PaymentDates;
		#endregion Members
		#region Constructors
		public PaymentDates()
		{
			paymentDatesDateReferenceResetDatesReference = new DateReference();
			paymentDatesDateReferenceCalculationPeriodDatesReference = new DateReference();
			firstPaymentDate = new EFS_Date();
			lastRegularPaymentDate = new EFS_Date();
			paymentDatesAdjustments = new BusinessDayAdjustments();
		}
		#endregion Constructors

		#region IPaymentDates Members
		IInterval IPaymentDates.paymentFrequency 
		{ 
			set { this.paymentFrequency = (Interval)value ; } 
			get { return this.paymentFrequency; } 
		}
		PayRelativeToEnum IPaymentDates.payRelativeTo 
		{ 
			set { this.payRelativeTo = value; } 
			get { return this.payRelativeTo; } 
		}
		bool IPaymentDates.firstPaymentDateSpecified 
        {
            set { this.firstPaymentDateSpecified = value; } 
            get { return this.firstPaymentDateSpecified; } 
        }
		EFS_Date IPaymentDates.firstPaymentDate 
        {
            set { this.firstPaymentDate = value; } 
            get { return this.firstPaymentDate; } 
        }
		bool IPaymentDates.paymentDaysOffsetSpecified 
		{ 
			set { this.paymentDaysOffsetSpecified = value; } 
			get { return this.paymentDaysOffsetSpecified; } 
		}
		IOffset IPaymentDates.paymentDaysOffset 
        { 
            set { this.paymentDaysOffset = (Offset)value; } 
            get { return this.paymentDaysOffset; } 
        }
        IBusinessDayAdjustments IPaymentDates.paymentDatesAdjustments
        {
            set { paymentDatesAdjustments = (BusinessDayAdjustments)value; }
            get { return this.paymentDatesAdjustments; }
        }
        bool IPaymentDates.lastRegularPaymentDateSpecified
        {
            set { lastRegularPaymentDateSpecified = value; }
            get { return this.lastRegularPaymentDateSpecified; }
        }
        EFS_Date IPaymentDates.lastRegularPaymentDate
        {
            set { this.lastRegularPaymentDate = value; }
            get { return this.lastRegularPaymentDate; }
        }
		EFS_PaymentDates IPaymentDates.efs_PaymentDates 
		{ 
			set { this.efs_PaymentDates = value; } 
			get { return this.efs_PaymentDates; } 
		}
		bool IPaymentDates.calculationPeriodDatesReferenceSpecified 
		{ 
			set { paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified = value; } 
			get { return paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified; } 
		}
		IReference IPaymentDates.calculationPeriodDatesReference 
		{ 
			set { paymentDatesDateReferenceCalculationPeriodDatesReference = (DateReference)value; } 
			get { return paymentDatesDateReferenceCalculationPeriodDatesReference; } 
		}
		bool IPaymentDates.resetDatesReferenceSpecified 
		{
			set { this.paymentDatesDateReferenceResetDatesReferenceSpecified = value; } 
			get { return this.paymentDatesDateReferenceResetDatesReferenceSpecified; } 
		}
		IReference IPaymentDates.resetDatesReference 
		{ 
			set { this.paymentDatesDateReferenceResetDatesReference = (DateReference)value; } 
			get { return this.paymentDatesDateReferenceResetDatesReference; } 
		}
        bool IPaymentDates.valuationDatesReferenceSpecified
        {
            set { ; }
            get { return false; }
        }
        IReference IPaymentDates.valuationDatesReference
        {
            set { ; }
            get { return null; }
        }

		string IPaymentDates.id
		{
			get { return this.id; }
			set { this.id = value; }
		}
		#endregion IPaymentDates Members
	}
	#endregion PaymentDates

	#region ResetDates
	public partial class ResetDates : IResetDates
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_ResetDates efs_ResetDates;
		#endregion Members
		#region Constructors
		public ResetDates()
		{
			calculationPeriodDatesReference = new DateReference();
		}
		#endregion Constructors

		#region IResetDates Members
		IResetFrequency IResetDates.resetFrequency
		{
			set { this.resetFrequency = (ResetFrequency)value; }
			get { return this.resetFrequency; }
		}
		bool IResetDates.resetRelativeToSpecified 
		{
			set { this.resetRelativeToSpecified = value; }
			get { return this.resetRelativeToSpecified; }
		}
		ResetRelativeToEnum IResetDates.resetRelativeTo 
		{
			set {this.resetRelativeTo = value; }
			get {return this.resetRelativeTo; }
		}
		EFS_ResetDates IResetDates.efs_ResetDates 
		{ 
			set { this.efs_ResetDates = value; } 
			get { return this.efs_ResetDates; } 
		}
		IBusinessDayAdjustments IResetDates.resetDatesAdjustments
		{
			set { this.resetDatesAdjustments = (BusinessDayAdjustments)value; }
			get { return this.resetDatesAdjustments; }
		}
		bool IResetDates.initialFixingDateSpecified 
		{
			set { this.initialFixingDateSpecified = value; }
			get { return this.initialFixingDateSpecified; }
		}
		IRelativeDateOffset IResetDates.initialFixingDate {get { return this.initialFixingDate;}}
		IRelativeDateOffset IResetDates.fixingDates 
		{
			set { this.fixingDates = (RelativeDateOffset)value;} 
			get { return this.fixingDates;} 
		}
		bool IResetDates.rateCutOffDaysOffsetSpecified 
		{
			set { this.rateCutOffDaysOffsetSpecified = value; } 
			get { return this.rateCutOffDaysOffsetSpecified; } 
		}
		IOffset IResetDates.rateCutOffDaysOffset 
		{
			set { this.rateCutOffDaysOffset = (Offset)value; } 
			get { return this.rateCutOffDaysOffset; } 
		}
		IReference IResetDates.calculationPeriodDatesReference {get { return this.calculationPeriodDatesReference;}}
		string IResetDates.id
		{
			set { this.id = value; }
			get { return this.id; }
		}
		#endregion IResetDates Members
	}
	#endregion ResetDates

	#region Stub
	public partial class Stub : IStub
	{
		#region Constructors
		public Stub()
		{
			stubTypeFloatingRate = new FloatingRate[1] { new FloatingRate() };
			stubTypeFixedRate = new EFS_Decimal();
			stubTypeAmount = new Money();
		}
		#endregion Constructors

		#region IStub Members
		bool IStub.stubTypeFloatingRateSpecified 
		{ 
			set { this.stubTypeFloatingRateSpecified = value; } 
			get { return this.stubTypeFloatingRateSpecified; } 
		}
		IFloatingRate[] IStub.stubTypeFloatingRate 
		{ 
			set { this.stubTypeFloatingRate = (FloatingRate[])value; } 
			get { return this.stubTypeFloatingRate; } 
		}
		bool IStub.stubTypeFixedRateSpecified 
		{
			set { this.stubTypeFixedRateSpecified = value; } 
			get { return this.stubTypeFixedRateSpecified; } 
		}
		EFS_Decimal IStub.stubTypeFixedRate 
		{ 
			set { this.stubTypeFixedRate = value; } 
			get { return this.stubTypeFixedRate; } 
		}
		bool IStub.stubTypeAmountSpecified
		{
			set { this.stubTypeAmountSpecified = value; }
			get { return this.stubTypeAmountSpecified; }
		}
		IMoney IStub.stubTypeAmount 
		{ 
			set { this.stubTypeAmount = (Money)value; } 
			get { return this.stubTypeAmount; } 
		}
		bool IStub.stubStartDateSpecified { get { return this.stubStartDateSpecified; } }
		IAdjustableOrRelativeDate IStub.stubStartDate { get { return this.stubStartDate; } }
		bool IStub.stubEndDateSpecified { get { return this.stubEndDateSpecified; } }
		IAdjustableOrRelativeDate IStub.stubEndDate { get { return this.stubEndDate; } }
		IFloatingRate[] IStub.CreateFloatingRate {get { return new FloatingRate[2] {new FloatingRate(),new FloatingRate()};}}
		IMoney IStub.CreateMoney { get { return new Money(); } }
		#endregion IStub Members

	}
	#endregion Stub
	#region StubCalculationPeriodAmount
	public partial class StubCalculationPeriodAmount : IStubCalculationPeriodAmount
	{
		#region Constructors
		public StubCalculationPeriodAmount()
		{
			calculationPeriodDatesReference = new DateReference();
		}
		#endregion Constructors

		#region IStubCalculationPeriodAmount Members
		IReference IStubCalculationPeriodAmount.calculationPeriodDatesReference { get { return this.calculationPeriodDatesReference; } }
		bool IStubCalculationPeriodAmount.initialStubSpecified 
		{
            set { this.initialStubSpecified = value; }
            get { return this.initialStubSpecified; } 
		}
		IStub IStubCalculationPeriodAmount.initialStub 
		{
			set { this.initialStub = (Stub)value; } 
			get { return this.initialStub; } 
		}
		bool IStubCalculationPeriodAmount.finalStubSpecified 
		{ 
			set { this.finalStubSpecified = value; } 
			get { return this.finalStubSpecified; } 
		}
		IStub IStubCalculationPeriodAmount.finalStub 
		{
			set { this.finalStub = (Stub)value; }
			get { return this.finalStub; } 
		}
		IStub IStubCalculationPeriodAmount.CreateStub { get { return new Stub(); } }
		#endregion IStubCalculationPeriodAmount Members

	}
	#endregion StubCalculationPeriodAmount
	#region Swap
	public partial class Swap : IProduct, ISwap, IDeclarativeProvision
    {
        #region Members

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool isPaymentDatesSynchronous;
        
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Swap";
        
        #endregion Members
        #region Constructors
        public Swap()
        {
            additionalPayment = new Payment[1] { new Payment() };
        }
        #endregion Constructors
        #region Accessors
        #region MinEffectiveDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MinEffectiveDate
        {
            get
            {
                EFS_EventDate dtEffective = new EFS_EventDate();
                dtEffective.unadjustedDate = new EFS_Date();
                dtEffective.unadjustedDate.DateValue = DateTime.MinValue;
                dtEffective.adjustedDate = new EFS_Date();
                dtEffective.adjustedDate.DateValue = DateTime.MinValue;
                foreach (IInterestRateStream stream in swapStream)
                {
                    EFS_EventDate streamEffectiveDate = stream.EffectiveDate;
                    if ((DateTime.MinValue == dtEffective.unadjustedDate.DateValue) ||
                        (0 < dtEffective.unadjustedDate.DateValue.CompareTo(streamEffectiveDate.unadjustedDate.DateValue)))
                    {
                        dtEffective.unadjustedDate.DateValue = streamEffectiveDate.unadjustedDate.DateValue;
                        dtEffective.adjustedDate.DateValue = streamEffectiveDate.adjustedDate.DateValue;
                    }
                }
                return dtEffective;
            }
        }
        #endregion MinEffectiveDate
        #region MaxTerminationDate
        // 20071015 EG Ticket 15858
        public EFS_EventDate MaxTerminationDate
        {
            get
            {
                EFS_EventDate dtTermination = new EFS_EventDate();
                dtTermination.unadjustedDate = new EFS_Date();
                dtTermination.unadjustedDate.DateValue = DateTime.MinValue;
                dtTermination.adjustedDate = new EFS_Date();
                dtTermination.adjustedDate.DateValue = DateTime.MinValue;
                foreach (InterestRateStream stream in swapStream)
                {
                    EFS_EventDate streamTerminationDate = ((IInterestRateStream)stream).TerminationDate;
                    if (0 < streamTerminationDate.unadjustedDate.DateValue.CompareTo(dtTermination.unadjustedDate.DateValue))
                    {
                        dtTermination.unadjustedDate.DateValue = streamTerminationDate.unadjustedDate.DateValue;
                        dtTermination.adjustedDate.DateValue = streamTerminationDate.adjustedDate.DateValue;
                    }
                }
                return dtTermination;
            }
        }
        #endregion MaxTerminationDate
        #region Stream
        public InterestRateStream[] Stream
        {
            get { return swapStream; }
        }
        #endregion Stream
        #endregion Accessors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ISwap Members
		IInterestRateStream[] ISwap.stream { get { return this.swapStream; } }
		bool ISwap.cancelableProvisionSpecified {get { return this.cancelableProvisionSpecified; }}
		ICancelableProvision ISwap.cancelableProvision { get { return this.cancelableProvision; } }
		bool ISwap.extendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
		IExtendibleProvision ISwap.extendibleProvision { get { return this.extendibleProvision; } }
		bool ISwap.earlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
		IEarlyTerminationProvision ISwap.earlyTerminationProvision 
        { 
            set { this.earlyTerminationProvision = (EarlyTerminationProvision)value; } 
            get { return this.earlyTerminationProvision; } 
        }
		bool ISwap.stepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
		IStepUpProvision ISwap.stepUpProvision { get { return this.stepUpProvision; } }
		bool ISwap.additionalPaymentSpecified 
		{ 
			set { this.additionalPaymentSpecified = value; } 
			get { return this.additionalPaymentSpecified; } 
		}
		IPayment[] ISwap.additionalPayment { get { return this.additionalPayment; } }
		bool ISwap.implicitProvisionSpecified 
        {
            set { this.implicitProvisionSpecified = value; } 
            get { return this.implicitProvisionSpecified; } 
        }
		IImplicitProvision ISwap.implicitProvision 
        {
            set { this.implicitProvision = (ImplicitProvision) value; } 
            get { return this.implicitProvision; } 
        }
		bool ISwap.implicitCancelableProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.cancelableProvisionSpecified); }
		}
		bool ISwap.implicitOptionalEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.optionalEarlyTerminationProvisionSpecified); }
		}
		bool ISwap.implicitExtendibleProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.extendibleProvisionSpecified); }
		}
		bool ISwap.implicitMandatoryEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.mandatoryEarlyTerminationProvisionSpecified); }
		}
        bool ISwap.isPaymentDatesSynchronous
        {
            get { return this.isPaymentDatesSynchronous; }
            set { this.isPaymentDatesSynchronous = value; } 
        }        
        #endregion ISwap Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified { get { return this.cancelableProvisionSpecified; } }
		ICancelableProvision IDeclarativeProvision.cancelableProvision { get { return this.cancelableProvision; } }
		bool IDeclarativeProvision.extendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
		IExtendibleProvision IDeclarativeProvision.extendibleProvision { get { return this.extendibleProvision; } }
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision { get { return this.earlyTerminationProvision; } }
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision { get { return this.stepUpProvision; } }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return this.implicitProvision; } }
		#endregion IDeclarativeProvision Members	
	}
    #endregion Swap
    #region Swaption
	public partial class Swaption : IProduct, ISwaption,IDeclarativeProvision
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_SwaptionDates efs_SwaptionDates;
        #endregion Members
        #region Constructors
        public Swaption()
        {
            exerciseAmerican = new AmericanExercise();
            exerciseBermuda = new BermudaExercise();
            exerciseEuropean = new EuropeanExercise();
            swap = new Swap();
            exercise = new EFS_RadioChoice();
            swaptionStraddle = new EFS_Boolean(false);
        }
        #endregion Constructors
        #region Accessors
        #region EFS_Exercise
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object EFS_Exercise
        {
			set
			{
				if (exerciseAmericanSpecified)
					exerciseAmerican = (AmericanExercise)value;
				else if (exerciseBermudaSpecified)
					exerciseBermuda = (BermudaExercise)value;
				else if (exerciseEuropeanSpecified)
					exerciseEuropean = (EuropeanExercise)value;
			}
            get
            {
                if (exerciseAmericanSpecified)
                    return exerciseAmerican;
                else if (exerciseBermudaSpecified)
                    return exerciseBermuda;
                else if (exerciseEuropeanSpecified)
                    return exerciseEuropean;
                else
                    return null;
            }
        }
        #endregion EFS_Exercise
        #endregion Accessors

		#region IProduct Members
		object IProduct.product { get { return this; } }
		IProductBase IProduct.productBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ISwaption Members
		IReference ISwaption.buyerPartyReference
		{
			set { this.buyerPartyReference = (PartyReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference ISwaption.sellerPartyReference
		{
			set { this.sellerPartyReference = (PartyReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_SwaptionDates ISwaption.efs_SwaptionDates
		{
			get { return this.efs_SwaptionDates; }
			set { this.efs_SwaptionDates = value; }
		}
		bool ISwaption.exerciseAmericanSpecified 
		{
			set { this.exerciseAmericanSpecified = value;}
			get { return this.exerciseAmericanSpecified;}
		}
		IAmericanExercise ISwaption.exerciseAmerican 
		{
			set { this.exerciseAmerican = (AmericanExercise)value;}
			get { return this.exerciseAmerican;}
		}
		bool ISwaption.exerciseBermudaSpecified 
		{
			set {this.exerciseBermudaSpecified = value; }
			get {return this.exerciseBermudaSpecified; }
		}
		IBermudaExercise ISwaption.exerciseBermuda 
		{
			set { this.exerciseBermuda = (BermudaExercise)value; }
			get { return this.exerciseBermuda; }
		}
		bool ISwaption.exerciseEuropeanSpecified 
		{
			set { this.exerciseEuropeanSpecified = value; }
			get { return this.exerciseEuropeanSpecified; }
		}
		IEuropeanExercise ISwaption.exerciseEuropean 
		{
			set {this.exerciseEuropean = (EuropeanExercise)value; }
			get {return this.exerciseEuropean; }
		}
		ExerciseStyleEnum ISwaption.GetStyle
		{
			get
			{
				if (this.exerciseAmericanSpecified)
					return ExerciseStyleEnum.American;
				else if (this.exerciseBermudaSpecified)
					return ExerciseStyleEnum.Bermuda;

				return ExerciseStyleEnum.European;
			}
		}
		object ISwaption.EFS_Exercise 
		{ 
			set { this.EFS_Exercise = value; } 
			get { return this.EFS_Exercise; } 
		}
		bool ISwaption.premiumSpecified 
		{
			set { this.premiumSpecified = value; } 
			get { return this.premiumSpecified; } 
		}
		IPayment[] ISwaption.premium 
		{
			set { this.premium = (Payment[])value; } 
			get { return this.premium; } 
		}
        bool ISwaption.exerciseProcedureSpecified
        {
            set { }
            get { return true; }
        }
        IExerciseProcedure ISwaption.exerciseProcedure
		{
            set { this.procedure = (ExerciseProcedure)value; }
            get { return this.procedure; }
		}
        bool ISwaption.calculationAgentSpecified
        {
            set { ;}
            get { return true; }
        }
        ICalculationAgent ISwaption.calculationAgent
		{
			set { this.calculationAgent = (CalculationAgent)value; }
			get { return this.calculationAgent; }
		}
		bool ISwaption.cashSettlementSpecified
		{
			get { return this.cashSettlementSpecified; }
		}
		ICashSettlement ISwaption.cashSettlement
		{
			set { this.cashSettlement = (CashSettlement)value; }
			get { return this.cashSettlement; }
		}
		EFS_Boolean ISwaption.swaptionStraddle
		{
			set { this.swaptionStraddle = value; }
			get { return this.swaptionStraddle; }
		}
		bool ISwaption.swaptionAdjustedDatesSpecified
		{
			get { return this.swaptionAdjustedDatesSpecified; }
		}
		ISwap ISwaption.swap
		{
			set { this.swap = (Swap)value; }
			get { return this.swap; }
		}
		#endregion ISwaption Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.cancelableProvisionSpecified {get {return false;}}
		ICancelableProvision IDeclarativeProvision.cancelableProvision {get { return null;}}
		bool IDeclarativeProvision.extendibleProvisionSpecified {get { return false;}}
		IExtendibleProvision IDeclarativeProvision.extendibleProvision {get { return null;}}
		bool IDeclarativeProvision.earlyTerminationProvisionSpecified {get { return false;}}
		IEarlyTerminationProvision IDeclarativeProvision.earlyTerminationProvision {get { return null;}}
		bool IDeclarativeProvision.stepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.stepUpProvision {get { return null;} }
		bool IDeclarativeProvision.implicitProvisionSpecified { get { return false; } }
		IImplicitProvision IDeclarativeProvision.implicitProvision { get { return null; } }
		#endregion IDeclarativeProvision Members
	}
    #endregion Swaption

    #region YieldCurveMethod
    public partial class YieldCurveMethod : IYieldCurveMethod
    {
        #region Constructors
        public YieldCurveMethod()
        {
            settlementRateSource = new SettlementRateSource();
        }
        #endregion Constructors

        #region IYieldCurveMethod Members
        bool IYieldCurveMethod.settlementRateSourceSpecified
        {
            get { return this.settlementRateSourceSpecified; }
            set {this.settlementRateSourceSpecified = value; }
        }
        QuotationRateTypeEnum IYieldCurveMethod.quotationRateType
        {
            get { return this.quotationRateType; }
            set { this.quotationRateType = value; }
        }
        #endregion IYieldCurveMethod Members
    }
    #endregion YieldCurveMethod
}
