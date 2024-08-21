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
using EfsML.v30.Ird;
using EfsML.v30.Shared;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Ird
{
    #region BulletPayment
    // EG 20180608 Add ICashPayment
    public partial class BulletPayment : IProduct, IBulletPayment, IDeclarativeProvision, ICashPayment
	{
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:BulletPayment";
        #endregion Members
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
        // EG 20180608 Add entityPartyReference
		public BulletPayment():base()
		{
			payment = new Payment();
            entityPartyReference = new PartyReference();
		}
		#endregion Constructors

		#region IProduct Members
        object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
        #region IBulletPayment Members
        IPayment IBulletPayment.Payment { get { return this.payment; } }
        #endregion IBulletPayment Members
        // EG 20180608 Add entityPartyReference
        #region ICashPayment Members
        bool ICashPayment.EntityPartyReferenceSpecified
        {
            set { this.entityPartyReferenceSpecified = value; }
            get { return this.entityPartyReferenceSpecified; }
        }
        IReference ICashPayment.EntityPartyReference 
        {
            get { return (IReference)this.entityPartyReference; }
            set { entityPartyReference = (PartyReference)value; }
        }
        #endregion ICashPayment Members
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
            rateInflationRate = new InflationRateCalculation();
		}
		#endregion Constructors

		#region ICalculation Members
		bool ICalculation.NotionalSpecified
		{
			set { this.calculationNotionalSpecified = value; }
			get { return this.calculationNotionalSpecified; }
		}
		INotional ICalculation.Notional { get { return this.calculationNotional; } }
        bool ICalculation.FxLinkedNotionalSpecified
        {
            set { this.calculationFxLinkedNotionalSpecified = value; }
            get { return this.calculationFxLinkedNotionalSpecified; }
        }
        IFxLinkedNotionalSchedule ICalculation.FxLinkedNotional
        {
            set { this.calculationFxLinkedNotional = (FxLinkedNotionalSchedule)value; }
            get { return this.calculationFxLinkedNotional; }
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
            set {this.rateInflationRateSpecified=value ; }
            get { return this.rateInflationRateSpecified; }
        }
        IInflationRateCalculation ICalculation.RateInflationRate { get { return rateInflationRate; } }
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
                EFS_Date dt = new EFS_Date
                {
                    DateValue = EffectiveDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;
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
                EFS_Date dt = new EFS_Date
                {
                    DateValue = TerminationDateAdjustment.AdjustableDate.UnadjustedDate.DateValue
                };
                return dt;
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
			effectiveDateAdjustable = new AdjustableDate();
			effectiveDateRelative = new AdjustedRelativeDateOffset();
			terminationDateAdjustable = new AdjustableDate();
			terminationDateRelative = new RelativeDateOffset();
			firstRegularPeriodStartDate = new EFS_Date();
			lastRegularPeriodEndDate = new EFS_Date();
			firstCompoundingPeriodEndDate = new EFS_Date();
		}
		#endregion Constructors

		#region ICalculationPeriodDates Members
		EFS_CalculationPeriodDates ICalculationPeriodDates.Efs_CalculationPeriodDates
		{
			set { this.efs_CalculationPeriodDates = value; }
			get { return this.efs_CalculationPeriodDates; }
		}
		ICalculationPeriodFrequency ICalculationPeriodDates.CalculationPeriodFrequency { get { return this.calculationPeriodFrequency; } }
		IBusinessDayAdjustments ICalculationPeriodDates.CalculationPeriodDatesAdjustments 
        {
            set { this.calculationPeriodDatesAdjustments = (BusinessDayAdjustments) value; } 
            get { return this.calculationPeriodDatesAdjustments; } 
        }
		bool ICalculationPeriodDates.FirstPeriodStartDateSpecified
		{
			set { this.firstPeriodStartDateSpecified = value; }
			get { return this.firstPeriodStartDateSpecified; }
		}
        IAdjustableDate ICalculationPeriodDates.FirstPeriodStartDate
        {
            get { return this.firstPeriodStartDate; }
        }
		bool ICalculationPeriodDates.FirstRegularPeriodStartDateSpecified
		{
			set { this.firstRegularPeriodStartDateSpecified = value; }
			get { return this.firstRegularPeriodStartDateSpecified; }
		}
        EFS_Date ICalculationPeriodDates.FirstRegularPeriodStartDate
        {
            set { this.firstRegularPeriodStartDate = value; }
            get { return this.firstRegularPeriodStartDate; }
        }
		bool ICalculationPeriodDates.LastRegularPeriodEndDateSpecified
		{
			set { this.lastRegularPeriodEndDateSpecified = value; }
			get { return this.lastRegularPeriodEndDateSpecified; }
		}
		EFS_Date ICalculationPeriodDates.LastRegularPeriodEndDate
		{
            set { this.lastRegularPeriodEndDate = value; }
            get { return this.lastRegularPeriodEndDate; }
		}
		bool ICalculationPeriodDates.EffectiveDateAdjustableSpecified 
        { 
            set { this.effectiveDateAdjustableSpecified = value; } 
            get { return this.effectiveDateAdjustableSpecified; } 
        }
        IAdjustableDate ICalculationPeriodDates.EffectiveDateAdjustable
        {
            set { this.effectiveDateAdjustable = (AdjustableDate)value; }
            get { return this.effectiveDateAdjustable; }
        }
        bool ICalculationPeriodDates.EffectiveDateRelativeSpecified
        {
            get { return this.effectiveDateRelativeSpecified; }
        }
        IAdjustedRelativeDateOffset ICalculationPeriodDates.EffectiveDateRelative
        {
            get { return this.effectiveDateRelative; }
        }
		bool ICalculationPeriodDates.TerminationDateAdjustableSpecified
        {
            set { this.terminationDateAdjustableSpecified = value; } 
            get { return this.terminationDateAdjustableSpecified; } 
        }
        IAdjustableDate ICalculationPeriodDates.TerminationDateAdjustable
        {
            get { return this.terminationDateAdjustable; }
            set { this.terminationDateAdjustable = (AdjustableDate)value; }
        }
		bool ICalculationPeriodDates.TerminationDateRelativeSpecified { get { return this.terminationDateRelativeSpecified; } }
		IRelativeDateOffset ICalculationPeriodDates.TerminationDateRelative { get { return this.terminationDateRelative; } }
		string ICalculationPeriodDates.Id
		{
			set { this.Id = value; }
			get { return this.Id; }
		}
		IRequiredIdentifierDate ICalculationPeriodDates.CreateRequiredIdentifierDate(DateTime pDate)
		{
            RequiredIdentifierDate requiredIdentifierDate = new RequiredIdentifierDate
            {
                DateValue = pDate
            };
            return requiredIdentifierDate;
		}
		#endregion ICalculationPeriodDates Members
	}
	#endregion CalculationPeriodDates
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
		ICashSettlement IProvision.CashSettlement 
		{
			set { ;}
			get { return null; } 
		}
		#endregion IProvision Members
        #region IExerciseProvision Members
        bool IExerciseProvision.AmericanSpecified
        {
            get { return this.cancelableExerciseAmericanSpecified; }
            set { this.cancelableExerciseAmericanSpecified = value; }
        }
        IAmericanExercise IExerciseProvision.American
        {
            get { return this.cancelableExerciseAmerican; }
            set { this.cancelableExerciseAmerican = (AmericanExercise)value; }
        }
        bool IExerciseProvision.BermudaSpecified
        {
            get { return this.cancelableExerciseBermudaSpecified; }
            set { this.cancelableExerciseBermudaSpecified = value; }
        }
        IBermudaExercise IExerciseProvision.Bermuda
        {
            get { return this.cancelableExerciseBermuda; }
            set { this.cancelableExerciseBermuda = (BermudaExercise)value; }
        }
        bool IExerciseProvision.EuropeanSpecified
        {
            get { return this.cancelableExerciseEuropeanSpecified; }
            set { this.cancelableExerciseEuropeanSpecified = value; }
        }
        IEuropeanExercise IExerciseProvision.European
        {
            get { return this.cancelableExerciseEuropean; }
            set { this.cancelableExerciseEuropean = (EuropeanExercise)value; }
        }
        bool IExerciseProvision.ExerciseNoticeSpecified
        {
            get { return this.exerciseNoticeSpecified; }
            set { this.exerciseNoticeSpecified = value; }
        }
        IExerciseNotice[] IExerciseProvision.ExerciseNotice
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
        bool IExerciseProvision.FollowUpConfirmation
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
		IReference ICancelableProvision.BuyerPartyReference
		{
            set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference ICancelableProvision.SellerPartyReference
		{
            set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_ExerciseDates ICancelableProvision.Efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion ICancelableProvision Members
	}
	#endregion CancelableProvision
	#region CapFloor
	public partial class CapFloor : IProduct, ICapFloor,IDeclarativeProvision
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
			get
			{
				return ((IInterestRateStream) capFloorStream).EffectiveDate;
			}
		}
		#endregion MinEffectiveDate
		#region MaxTerminationDate
		// 20071015 EG Ticket 15858
		public EFS_EventDate MaxTerminationDate
		{
			get
			{
				return ((IInterestRateStream) capFloorStream).TerminationDate;
			}
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ICapFloor Members
		IInterestRateStream ICapFloor.Stream { get { return this.capFloorStream; } }
		IInterestRateStream[] ICapFloor.StreamInArray
		{
			get
			{
				InterestRateStream[] streams = new InterestRateStream[1] { this.capFloorStream };
				return streams;
			}
		}
		bool ICapFloor.PremiumSpecified
		{
			set { this.premiumSpecified = value; }
			get { return this.premiumSpecified; }
		}
		IPayment[] ICapFloor.Premium { get { return this.premium; } }
		bool ICapFloor.AdditionalPaymentSpecified
		{
			set { this.additionalPaymentSpecified = value; }
			get { return this.additionalPaymentSpecified; }
		}
		IPayment[] ICapFloor.AdditionalPayment { get { return this.additionalPayment; } }
		bool ICapFloor.EarlyTerminationProvisionSpecified
		{
			get { return this.earlyTerminationProvisionSpecified; }
		}
		IEarlyTerminationProvision ICapFloor.EarlyTerminationProvision
		{
			get { return this.earlyTerminationProvision; }
		}
		bool ICapFloor.ImplicitProvisionSpecified { get { return this.implicitProvisionSpecified; } }
		IImplicitProvision ICapFloor.ImplicitProvision { get { return this.implicitProvision; } }
		bool ICapFloor.ImplicitCancelableProvisionSpecified
		{
			get { return false; }
		}
		bool ICapFloor.ImplicitOptionalEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.optionalEarlyTerminationProvisionSpecified); }
		}
		bool ICapFloor.ImplicitExtendibleProvisionSpecified
		{
			get { return false; }
		}
		bool ICapFloor.ImplicitMandatoryEarlyTerminationProvisionSpecified
		{
			get { return false; }
		}
        EFS_EventDate ICapFloor.MaxTerminationDate { get { return MaxTerminationDate; } }
		#endregion ICapFloor Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified {get {return false;}}
		ICancelableProvision IDeclarativeProvision.CancelableProvision {get { return null;}}
		bool IDeclarativeProvision.ExtendibleProvisionSpecified {get { return false;}}
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision {get { return null;}}
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified {get { return this.earlyTerminationProvisionSpecified;}}
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision {get { return this.earlyTerminationProvision;}}
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return false; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision {get { return null;} }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return implicitProvisionSpecified; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return implicitProvision; } }
		#endregion IDeclarativeProvision Members
	}
	#endregion CapFloor

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
		DiscountingTypeEnum IDiscounting.DiscountingType { get { return this.discountingType; } }
		bool IDiscounting.RateSpecified { get { return this.discountRateSpecified; } }
		decimal IDiscounting.Rate { get { return this.discountRate.DecValue; } }
		bool IDiscounting.DayCountFractionSpecified { get { return discountRateDayCountFractionSpecified; } }
		DayCountFractionEnum IDiscounting.DayCountFraction { get { return this.discountRateDayCountFraction; } }
		#endregion IDiscounting Members
	}
	#endregion Discounting


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
		ICashSettlement IProvision.CashSettlement 
		{ 
			set { ; } 
			get { return null; } 
		}
		#endregion IProvision Members
        #region IExerciseProvision Members
        bool IExerciseProvision.AmericanSpecified
        {
            get { return this.extendibleExerciseAmericanSpecified; }
            set { this.extendibleExerciseAmericanSpecified = value; }
        }
        IAmericanExercise IExerciseProvision.American
        {
            get { return this.extendibleExerciseAmerican; }
            set { this.extendibleExerciseAmerican = (AmericanExercise)value; }
        }
        bool IExerciseProvision.BermudaSpecified
        {
            get { return this.extendibleExerciseBermudaSpecified; }
            set { this.extendibleExerciseBermudaSpecified = value; }
        }
        IBermudaExercise IExerciseProvision.Bermuda
        {
            get { return this.extendibleExerciseBermuda; }
            set { this.extendibleExerciseBermuda = (BermudaExercise)value; }
        }
        bool IExerciseProvision.EuropeanSpecified
        {
            get { return this.extendibleExerciseEuropeanSpecified; }
            set { this.extendibleExerciseEuropeanSpecified = value; }
        }
        IEuropeanExercise IExerciseProvision.European
        {
            get { return this.extendibleExerciseEuropean; }
            set { this.extendibleExerciseEuropean = (EuropeanExercise)value; }
        }
        bool IExerciseProvision.ExerciseNoticeSpecified
        {
            get { return this.exerciseNoticeSpecified; }
            set { this.exerciseNoticeSpecified = value; }
        }
        IExerciseNotice[] IExerciseProvision.ExerciseNotice
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
        bool IExerciseProvision.FollowUpConfirmation
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
		IReference IExtendibleProvision.BuyerPartyReference
		{
            set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference IExtendibleProvision.SellerPartyReference
		{
            set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_ExerciseDates IExtendibleProvision.Efs_ExerciseDates
		{
			get { return this.efs_ExerciseDates; }
			set { this.efs_ExerciseDates = value; }
		}
		#endregion IExtendibleProvision Members
	}
	#endregion ExtendibleProvision

	#region Fra
	public partial class Fra : IFra, IProduct,IDeclarativeProvision
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
                EFS_Date dt = new EFS_Date
                {
                    DateValue = efs_FraDates.fixingDateAdjustment.adjustedDate.DateValue
                };
                return dt;
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
                EFS_Date dt = new EFS_Date
                {
                    DateValue = efs_FraDates.paymentDateAdjustment.adjustedDate.DateValue
                };
                return dt;
			}
		}

		#endregion AdjustedPaymentDate
		#region AdjustedPreSettlementDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedPreSettlementDate
		{
			get {return efs_FraDates.AdjustedPreSettlementDate; }
		}
		#endregion AdjustedPreSettlementDate
		#region AdjustedTerminationDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_Date AdjustedTerminationDate
		{
			get{ return new EFS_Date(adjustedTerminationDate.Value); }
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
				return new EFS_DayCountFraction(startDate, endDate, dayCountFraction, (IInterval)interval);
			}
		}
		#endregion DayCountFraction
		#region EffectiveDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate EffectiveDate
		{
			get{return new EFS_EventDate(adjustedEffectiveDate.DateValue, adjustedEffectiveDate.DateValue);}
		}

		#endregion EffectiveDate
		#region FixingDate
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        // EG 20180423 Analyse du code Correction [CA2200]
        public EFS_EventDate FixingDate
		{
			get{return new EFS_EventDate(efs_FraDates.fixingDateAdjustment);}
		}
		#endregion FixingDate
		#region FloatingRate
        // EG 20140904 Add AssetCategory
		public EFS_Asset AssetRateIndex
		{
			get
			{
				try
				{
                    EFS_Asset asset = new EFS_Asset
                    {
                        idAsset = floatingRateIndex.OTCmlId,
                        assetCategory = Cst.UnderlyingAsset.RateIndex
                    };
                    return asset;
				}
				catch { return null; }
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
            indexTenor = (Interval[]) ((IProductBase)this).CreateIntervals();
        }
        #endregion

        #region IFra Members
        IReference IFra.BuyerPartyReference { get { return buyerPartyReference; } }
		IReference IFra.SellerPartyReference { get { return sellerPartyReference; } }
		IRequiredIdentifierDate IFra.AdjustedEffectiveDate
		{
			set { this.adjustedEffectiveDate = (RequiredIdentifierDate)value; }
			get { return this.adjustedEffectiveDate; }
		}
		EFS_Date IFra.AdjustedTerminationDate
		{
			set { this.adjustedTerminationDate = value; }
			get { return this.adjustedTerminationDate; }
		}
		EFS_Decimal IFra.FixedRate
		{
			set { this.fixedRate = value; }
			get { return this.fixedRate; }
		}
		IFloatingRateIndex IFra.FloatingRateIndex
		{
			set { this.floatingRateIndex = (FloatingRateIndex)value; }
			get { return this.floatingRateIndex; }
		}
		IMoney IFra.Notional
		{
			get { return this.notional; }
		}
		bool IFra.IndexTenorSpecified
		{
			get { return true; }
		}
		IInterval[] IFra.IndexTenor
		{
			set { this.indexTenor = (Interval[])value; }
			get { return this.indexTenor; }
		}
		IInterval IFra.FirstIndexTenor
		{
			set { this.indexTenor[0] = (Interval)value; }
			get { return this.indexTenor[0]; }
		}
		IAdjustableDate IFra.PaymentDate
		{
			get { return this.paymentDate; }
		}
		IRelativeDateOffset IFra.FixingDateOffset
		{
			get { return this.fixingDateOffset; }
		}
		DayCountFractionEnum IFra.DayCountFraction
		{
			set { this.dayCountFraction = value; }
			get { return this.dayCountFraction; }
		}
		EFS_PosInteger IFra.CalculationPeriodNumberOfDays
		{
			set { this.calculationPeriodNumberOfDays = value; }
			get { return this.calculationPeriodNumberOfDays; }
		}
		FraDiscountingEnum IFra.FraDiscounting
		{
			get { return this.fraDiscounting; }
		}
		EFS_FraDates IFra.Efs_FraDates
		{
			get { return this.efs_FraDates; }
			set { this.efs_FraDates = value; }
		}
		#endregion IFra Members
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
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
            this.constantNotionalScheduleReference = new ScheduleReference();
        }
        #endregion constructors

		#region IFxLinkedNotionalSchedule Members
		EFS_FxLinkedNotionalDates IFxLinkedNotionalSchedule.Efs_FxLinkedNotionalDates
		{
			get { return this.efs_FxLinkedNotionalDates; }
			set { this.efs_FxLinkedNotionalDates = value; }
		}
        string IFxLinkedNotionalSchedule.Currency
        {
            set { this.currency.Value = value; }
            get { return this.currency.Value; }
        }
        IRelativeDateOffset IFxLinkedNotionalSchedule.VaryingNotionalInterimExchangePaymentDates
        {
            set { this.varyingNotionalInterimExchangePaymentDates = (RelativeDateOffset)value; }
            get { return this.varyingNotionalInterimExchangePaymentDates; }
        }
        IRelativeDateOffset IFxLinkedNotionalSchedule.VaryingNotionalFixingDates
        {
            set { this.varyingNotionalFixingDates = (RelativeDateOffset)value; }
            get { return this.varyingNotionalFixingDates; }
        }
        IReference IFxLinkedNotionalSchedule.ConstantNotionalScheduleReference
        {
            set {this.constantNotionalScheduleReference = (ScheduleReference)value;}
            get { return this.constantNotionalScheduleReference; }
        }
        bool IFxLinkedNotionalSchedule.InitialValueSpecified
        {
            set { this.initialValueSpecified = value; }
            get { return this.initialValueSpecified; }
        }
        EFS_Decimal IFxLinkedNotionalSchedule.InitialValue
        {
            set { this.initialValue = value; }
            get { return this.initialValue; }
        }
        IFxSpotRateSource IFxLinkedNotionalSchedule.FxSpotRateSource
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
                    ret = new SQL_AssetFxRate(pConnectionString,idFxRate);
                    ret.LoadTable();
                }
            }
            return ret;
        }
        #endregion IFxLinkedNotionalSchedule Members
    }
	#endregion FxLinkedNotionalSchedule

    #region InflationRateCalculation
    public partial class InflationRateCalculation : IInflationRateCalculation
    {
		#region Constructors
        public InflationRateCalculation() { }
		#endregion Constructors

		#region IFloatingRateCalculation Members
		bool IFloatingRateCalculation.FinalRateRoundingSpecified { get { return this.finalRateRoundingSpecified; } }
		IRounding IFloatingRateCalculation.FinalRateRounding { get { return this.finalRateRounding; } }
		bool IFloatingRateCalculation.AveragingMethodSpecified { get { return this.averagingMethodSpecified; } }
		AveragingMethodEnum IFloatingRateCalculation.AveragingMethod { get { return this.averagingMethod; } }
		bool IFloatingRateCalculation.NegativeInterestRateTreatmentSpecified 
        {
            set { this.negativeInterestRateTreatmentSpecified = value; } 
            get { return this.negativeInterestRateTreatmentSpecified; } 
        }
		NegativeInterestRateTreatmentEnum IFloatingRateCalculation.NegativeInterestRateTreatment 
        {
            set { this.negativeInterestRateTreatment = value; } 
            get { return this.negativeInterestRateTreatment; } 
        }
		#endregion IFloatingRateCalculation Members
		#region IFloatingRate Members
		bool IFloatingRate.IndexTenorSpecified
		{
			set { this.indexTenorSpecified = value; }
			get { return this.indexTenorSpecified; }
		}
		IInterval IFloatingRate.IndexTenor
		{
			set { this.indexTenor = (Interval)value; }
			get { return this.indexTenor; }
		}
		bool IFloatingRate.CapRateScheduleSpecified 
		{ 
			set { this.capRateScheduleSpecified = value; } 
			get { return this.capRateScheduleSpecified; } 
		}
		IStrikeSchedule[] IFloatingRate.CapRateSchedule 
		{ 
			set { this.capRateSchedule = (StrikeSchedule[])value; } 
			get { return this.capRateSchedule; } 
		}
		bool IFloatingRate.FloorRateScheduleSpecified 
		{
            set { this.floorRateScheduleSpecified = value; } 
			get { return this.floorRateScheduleSpecified; } 
		}
		IStrikeSchedule[] IFloatingRate.FloorRateSchedule 
		{
			set { this.floorRateSchedule = (StrikeSchedule[])value; }
			get { return this.floorRateSchedule; } 
		}
		IFloatingRateIndex IFloatingRate.FloatingRateIndex { get { return this.floatingRateIndex; } }
		bool IFloatingRate.FloatingRateMultiplierScheduleSpecified 
		{ 
			set { this.floatingRateMultiplierScheduleSpecified = value; } 
			get { return this.floatingRateMultiplierScheduleSpecified; } 
		}
		ISchedule IFloatingRate.FloatingRateMultiplierSchedule { get { return this.floatingRateMultiplierSchedule; } }
		bool IFloatingRate.SpreadScheduleSpecified 
		{ 
			set { this.spreadScheduleSpecified = value; } 
			get { return this.spreadScheduleSpecified; } 
		}
		ISchedule IFloatingRate.SpreadSchedule
		{
            set 
            { 
                if ((null != this.spreadSchedule) && (0 < this.spreadSchedule.Length))
                    this.spreadSchedule[0] = (SpreadSchedule)value; 
            }
			get
			{
				if (this.spreadScheduleSpecified && (0 < this.spreadSchedule.Length))
					return this.spreadSchedule[0];
				else
					return null;
			}
		}
		bool IFloatingRate.RateTreatmentSpecified { get { return this.rateTreatmentSpecified; } }
		RateTreatmentEnum IFloatingRate.RateTreatment { get { return this.rateTreatment; } }
        IStrikeSchedule[] IFloatingRate.CreateStrikeSchedule(int pDim)
        {
            StrikeSchedule[] ret = new StrikeSchedule[pDim];
            for (int i = 0; i < pDim; i++)
                ret[i] = new StrikeSchedule();
            return ret;
        }
		#endregion IFloatingRate Members

    }
    #endregion InflationRateCalculation
	#region InterestRateStream
	public partial class InterestRateStream : IEFS_Array, IInterestRateStream
	{
		#region Members
		/*
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace="http://www.w3.org/2001/XMLSchema-instance",
			 Form=System.Xml.Schema.XmlSchemaForm.Qualified, DataType="normalizedString")]
		public string type="efs:InterestRateStream";
		*/
		#endregion Members
		#region Constructors
        public InterestRateStream()
        {
            payerPartyReference = new PartyOrAccountReference();
            receiverPartyReference = new PartyOrAccountReference();
            calculationPeriodDates = new CalculationPeriodDates();
            paymentDates = new PaymentDates();
            resetDates = new ResetDates();
            calculationPeriodAmount = new CalculationPeriodAmount();
            stubCalculationPeriodAmount = new StubCalculationPeriodAmount();
            principalExchanges = new PrincipalExchanges();
            cashflows = new Cashflows();
            settlementProvision = new SettlementProvision();
            formula = new Formula();
        }
		#endregion Constructors
		#region Methods
		#region EventType
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
                    else if (calculation.rateFloatingRateSpecified || calculation.rateInflationRateSpecified)
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
                    else if (calculation.rateInflationRateSpecified)
                    {
                        isCap = calculation.rateInflationRate.capRateScheduleSpecified;
                        if (isCap)
                            countStrikeSchedule += calculation.rateInflationRate.capRateSchedule.Length;
                        isFloor = calculation.rateInflationRate.floorRateScheduleSpecified;
                        if (isFloor)
                            countStrikeSchedule += calculation.rateInflationRate.floorRateSchedule.Length;
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
		#region Rate
		/// <revision>
		///     <version>2.0.1</version><date>20080408</date><author>EG</author>
		///     <comment>
		///     Ticket : 156165 Initial & Final Stub : Wrong event type when one of rate not specified
		///     </comment>
		/// </revision>
		public object Rate(string pStub)
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
                        else if (calculation.rateInflationRateSpecified)
                            rate = calculation.rateInflationRate;
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
		#endregion Rate
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
        IReference IInterestRateStream.PayerPartyReference
        {
            set { payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference IInterestRateStream.ReceiverPartyReference
        {
            set { receiverPartyReference = (PartyOrAccountReference)value; }
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
		IResetFrequency IInterestRateStream.CreateResetFrequency() {  return new ResetFrequency();  }
		IResetDates IInterestRateStream.CreateResetDates() {  return new ResetDates();  }
		IInterval IInterestRateStream.CreateInterval() {return new Interval();  }
		IOffset IInterestRateStream.CreateOffset() {  return new Offset(); } 
		IStubCalculationPeriodAmount IInterestRateStream.CreateStubCalculationPeriodAmount() { return new StubCalculationPeriodAmount(); }
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
        object IInterestRateStream.GetCalculationPeriodAmount
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
                {
                    return calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule;
                }

                return null;
            }
        }
        string IInterestRateStream.DayCountFraction
        {
            get
            {
                string ret = string.Empty;
                if (this.calculationPeriodAmount.calculationPeriodAmountCalculationSpecified)
                    ret = this.calculationPeriodAmount.calculationPeriodAmountCalculation.dayCountFraction.ToString();
                else if ((this.calculationPeriodAmount.calculationPeriodAmountKnownAmountScheduleSpecified) &&
                         (this.calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.dayCountFractionSpecified))
                    ret = this.calculationPeriodAmount.calculationPeriodAmountKnownAmountSchedule.dayCountFraction.ToString();
                return ret;

            }
        }
        EFS_EventDate IInterestRateStream.EffectiveDate
        {
            get
            {
                return new EFS_EventDate(((IInterestRateStream)this).EffectiveDateAdjustment);
            }
        }
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
                    else if (calculation.rateInflationRateSpecified)
                        ret = calculation.rateInflationRate.capRateScheduleSpecified;
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
                    else if (calculation.rateInflationRateSpecified)
                        ret = calculation.rateInflationRate.floorRateScheduleSpecified;
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
            get {return payerPartyReference.href;}
        }
        // EG 20180423 Analyse du code Correction [CA2200]
        string IInterestRateStream.GetReceiverPartyReference
        {
            get {return receiverPartyReference.href;}
        }
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
        EFS_EventDate IInterestRateStream.TerminationDate
        {
            get
            {
                return new EFS_EventDate(((IInterestRateStream)this).TerminationDateAdjustment);

            }
        }
        EFS_AdjustableDate IInterestRateStream.TerminationDateAdjustment
        {
            get
            {
                EFS_CalculationPeriodDates calculationPeriodDates = this.calculationPeriodDates.efs_CalculationPeriodDates;
                EFS_AdjustableDate adjustableDate = calculationPeriodDates.terminationDateAdjustment;
                return adjustableDate;

            }
        }
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
            get {return this.GetCurrency();}
        }
        string IInterestRateStream.EventType(string pProduct)
        {
            return ((IInterestRateStream)this).EventType2(pProduct, false);
        }
        // 20090828 EG Call method EventType
        string IInterestRateStream.EventType2(string pProduct, bool pIsDiscount)
        {
            string eventType;
            if (pIsDiscount)
                eventType = EventTypeFunc.ZeroCoupon;
            else
                eventType = this.EventType(pProduct);
            return eventType;
        }
        object IInterestRateStream.Rate(string pStub)
        {
            return this.Rate(pStub);
        }
        #endregion IInterestRateStream Members
    }
	#endregion InterestRateStream

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
        IAmountSchedule INotional.StepSchedule { get { return this.notionalStepSchedule; } }
		bool INotional.StepParametersSpecified { get { return this.notionalStepParametersSpecified; } }
		INotionalStepRule INotional.StepParameters { get { return this.notionalStepParameters; } }
		string INotional.Id
		{
			get { return this.Id; }
			set { this.Id = value; }
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
		// 20071130 EG Ticket 15998
		#region IsStepDateCalculated
		public bool IsStepDateCalculated
		{
			get { return (null != efs_FirstStepDate) && (null != efs_LastStepDate) && (null != efs_VirtualLastStepDate); }
		}
		#endregion IsStepDateCalculated
		#endregion Accessors
		#region Methods
		// 20071130 EG Ticket 15998
		#region CalcAdjustableStepDate
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        public Cst.ErrLevel CalcAdjustableStepDate(string pConnectionString, DateTime pTerminationDate, 
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
		{
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
			return Cst.ErrLevel.SUCCESS;
		}
		#endregion CalcAdjustableStepDate
		#endregion Methods

		#region INotionalStepRule Members
		bool INotionalStepRule.IsStepDateCalculated { get { return this.IsStepDateCalculated; } }
		EFS_Step INotionalStepRule.Efs_FirstStepDate { get { return this.efs_FirstStepDate; } }
		EFS_Step INotionalStepRule.Efs_LastStepDate { get { return this.efs_LastStepDate; } }
		EFS_Step INotionalStepRule.Efs_VirtualLastStepDate { get { return this.efs_VirtualLastStepDate; } }
		IInterval INotionalStepRule.StepFrequency { get { return this.stepFrequency; } }
		bool INotionalStepRule.NotionalStepAmountSpecified { get { return this.notionalStepAmountSpecified; } }
		decimal INotionalStepRule.NotionalStepAmount { get { return this.notionalStepAmount.DecValue; } }
		bool INotionalStepRule.NotionalStepRateSpecified { get { return this.notionalStepRateSpecified; } }
		bool INotionalStepRule.StepRelativeToSpecified { get { return this.stepRelativeToSpecified; } }
		StepRelativeToEnum INotionalStepRule.StepRelativeTo { get { return this.stepRelativeTo; } }
		decimal INotionalStepRule.NotionalStepRate { get { return this.notionalStepRate.DecValue; } }
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        Cst.ErrLevel INotionalStepRule.CalcAdjustableStepDate(string pCs, DateTime pTerminationDate,
            IBusinessDayAdjustments pBusinessDayAdjustments, DataDocumentContainer pDataDocument)
		{
			return this.CalcAdjustableStepDate(pCs, pTerminationDate, pBusinessDayAdjustments, pDataDocument);
		}
		#endregion INotionalStepRule Members
	}
	#endregion NotionalStepRule

	#region PaymentDates
    // EG 20140702 New build FpML4.4
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
            paymentDatesDateReferenceValuationDatesReference = new DateReference();
			firstPaymentDate = new EFS_Date();
			lastRegularPaymentDate = new EFS_Date();
			paymentDatesAdjustments = new BusinessDayAdjustments();
		}
		#endregion Constructors
		#region IPaymentDates Members
		IInterval IPaymentDates.PaymentFrequency
		{
			set { this.paymentFrequency = (Interval)value; }
			get { return this.paymentFrequency; }
		}
		PayRelativeToEnum IPaymentDates.PayRelativeTo
		{
			set { this.payRelativeTo = value; }
			get { return this.payRelativeTo; }
		}
		bool IPaymentDates.FirstPaymentDateSpecified 
        {
            set { this.firstPaymentDateSpecified = value; }
            get { return this.firstPaymentDateSpecified; } 
        }
		EFS_Date IPaymentDates.FirstPaymentDate 
        {
            set { this.firstPaymentDate = value; }
            get { return this.firstPaymentDate; } 
        }
		bool IPaymentDates.PaymentDaysOffsetSpecified
		{
			set { this.paymentDaysOffsetSpecified = value; }
			get { return this.paymentDaysOffsetSpecified; }
		}
		IOffset IPaymentDates.PaymentDaysOffset 
        {
            set { this.paymentDaysOffset = (Offset)value; } 
            get { return this.paymentDaysOffset; } 
        }
        IBusinessDayAdjustments IPaymentDates.PaymentDatesAdjustments
        {
            set { paymentDatesAdjustments = (BusinessDayAdjustments)value; }
            get { return this.paymentDatesAdjustments; }
        }
        bool IPaymentDates.LastRegularPaymentDateSpecified
        {
            set { this.lastRegularPaymentDateSpecified = value; }
            get { return this.lastRegularPaymentDateSpecified; }
        }
        EFS_Date IPaymentDates.LastRegularPaymentDate
        {
            set { this.lastRegularPaymentDate = value; }
            get { return this.lastRegularPaymentDate; }
        }
		EFS_PaymentDates IPaymentDates.Efs_PaymentDates
		{
			set { this.efs_PaymentDates = value; }
			get { return this.efs_PaymentDates; }
		}
		bool IPaymentDates.CalculationPeriodDatesReferenceSpecified
		{
			set { paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified = value; }
			get { return paymentDatesDateReferenceCalculationPeriodDatesReferenceSpecified; }
		}
		IReference IPaymentDates.CalculationPeriodDatesReference
		{
			set { paymentDatesDateReferenceCalculationPeriodDatesReference = (DateReference)value; }
			get { return paymentDatesDateReferenceCalculationPeriodDatesReference; }
		}
		bool IPaymentDates.ResetDatesReferenceSpecified
		{
			set { this.paymentDatesDateReferenceResetDatesReferenceSpecified = value; }
			get { return this.paymentDatesDateReferenceResetDatesReferenceSpecified; }
		}
		IReference IPaymentDates.ResetDatesReference
		{
			set { this.paymentDatesDateReferenceResetDatesReference = (DateReference)value; }
			get { return this.paymentDatesDateReferenceResetDatesReference; }
		}
        bool IPaymentDates.ValuationDatesReferenceSpecified
        {
            set { this.paymentDatesDateReferenceValuationDatesReferenceSpecified = value; }
            get { return this.paymentDatesDateReferenceValuationDatesReferenceSpecified; }
        }
        IReference IPaymentDates.ValuationDatesReference
        {
            set { this.paymentDatesDateReferenceValuationDatesReference = (DateReference)value; }
            get { return this.paymentDatesDateReferenceValuationDatesReference; }
        }
        string IPaymentDates.Id
		{
			get { return this.Id; }
			set { this.Id = value; }
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
		IResetFrequency IResetDates.ResetFrequency
		{
			set { this.resetFrequency = (ResetFrequency)value; }
			get { return this.resetFrequency; }
		}
		bool IResetDates.ResetRelativeToSpecified
		{
			set { this.resetRelativeToSpecified = value; }
			get { return this.resetRelativeToSpecified; }
		}
		ResetRelativeToEnum IResetDates.ResetRelativeTo
		{
			set { this.resetRelativeTo = value; }
			get { return this.resetRelativeTo; }
		}
		EFS_ResetDates IResetDates.Efs_ResetDates
		{
			set { this.efs_ResetDates = value; }
			get { return this.efs_ResetDates; }
		}
		IBusinessDayAdjustments IResetDates.ResetDatesAdjustments
		{
			set { this.resetDatesAdjustments = (BusinessDayAdjustments)value; }
			get { return this.resetDatesAdjustments; }
		}
		bool IResetDates.InitialFixingDateSpecified
		{
			set { this.initialFixingDateSpecified = value; }
			get { return this.initialFixingDateSpecified; }
		}
		IRelativeDateOffset IResetDates.InitialFixingDate { get { return this.initialFixingDate; } }
		IRelativeDateOffset IResetDates.FixingDates
		{
			set { this.fixingDates = (RelativeDateOffset)value; }
			get { return this.fixingDates; }
		}
		bool IResetDates.RateCutOffDaysOffsetSpecified
		{
			set { this.rateCutOffDaysOffsetSpecified = value; }
			get { return this.rateCutOffDaysOffsetSpecified; }
		}
		IOffset IResetDates.RateCutOffDaysOffset
		{
			set { this.rateCutOffDaysOffset = (Offset)value; }
			get { return this.rateCutOffDaysOffset; }
		}
		IReference IResetDates.CalculationPeriodDatesReference { get { return this.calculationPeriodDatesReference; } }
		string IResetDates.Id
		{
			set { this.Id = value; }
			get { return this.Id; }
		}
		#endregion IResetDates Members
	}
	#endregion ResetDates

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
		IReference IStubCalculationPeriodAmount.CalculationPeriodDatesReference { get { return this.calculationPeriodDatesReference; } }
		bool IStubCalculationPeriodAmount.InitialStubSpecified
		{
			set { this.initialStubSpecified = value; }
			get { return this.initialStubSpecified; }
		}
		IStub IStubCalculationPeriodAmount.InitialStub
		{
			set { this.initialStub = (Stub)value; }
			get { return this.initialStub; }
		}
		bool IStubCalculationPeriodAmount.FinalStubSpecified
		{
			set { this.finalStubSpecified = value; }
			get { return this.finalStubSpecified; }
		}
		IStub IStubCalculationPeriodAmount.FinalStub
		{
			set { this.finalStub = (Stub)value; }
			get { return this.finalStub; }
		}
		IStub IStubCalculationPeriodAmount.CreateStub { get { return new Stub(); } }
		#endregion IStubCalculationPeriodAmount Members
	}
	#endregion StubCalculationPeriodAmount
	#region Swap
	public partial class Swap : IProduct, ISwap,IDeclarativeProvision
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
                EFS_EventDate dtEffective = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
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
                EFS_EventDate dtTermination = new EFS_EventDate
                {
                    unadjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    },
                    adjustedDate = new EFS_Date
                    {
                        DateValue = DateTime.MinValue
                    }
                };
                foreach (IInterestRateStream stream in swapStream)
				{
                    EFS_EventDate streamTerminationDate = stream.TerminationDate;
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
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ISwap Members
		IInterestRateStream[] ISwap.Stream { get { return this.swapStream; } }
		bool ISwap.CancelableProvisionSpecified { get { return this.cancelableProvisionSpecified; } }
		ICancelableProvision ISwap.CancelableProvision { get { return this.cancelableProvision; } }
		bool ISwap.ExtendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
		IExtendibleProvision ISwap.ExtendibleProvision { get { return this.extendibleProvision; } }
		bool ISwap.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
		IEarlyTerminationProvision ISwap.EarlyTerminationProvision 
        { 
            set { this.earlyTerminationProvision = (EarlyTerminationProvision)value; } 
            get { return this.earlyTerminationProvision; } 
        }
		bool ISwap.StepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
		IStepUpProvision ISwap.StepUpProvision { get { return this.stepUpProvision; } }
		bool ISwap.ImplicitProvisionSpecified 
        {
            set { this.implicitProvisionSpecified = value; } 
            get { return this.implicitProvisionSpecified; } 
        }
		IImplicitProvision ISwap.ImplicitProvision 
        {
            set { this.implicitProvision = (ImplicitProvision)value; } 
            get { return this.implicitProvision; } 
        }
		bool ISwap.ImplicitCancelableProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.cancelableProvisionSpecified); }
		}
		bool ISwap.ImplicitOptionalEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.optionalEarlyTerminationProvisionSpecified); }
		}
		bool ISwap.ImplicitExtendibleProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.extendibleProvisionSpecified); }
		}
		bool ISwap.ImplicitMandatoryEarlyTerminationProvisionSpecified
		{
			get { return (this.implicitProvisionSpecified && this.implicitProvision.mandatoryEarlyTerminationProvisionSpecified); }
		}
		bool ISwap.AdditionalPaymentSpecified
		{
			set { this.additionalPaymentSpecified = value; }
			get { return this.additionalPaymentSpecified; }
		}
		IPayment[] ISwap.AdditionalPayment { get { return this.additionalPayment; } }
        EFS_EventDate ISwap.MaxTerminationDate { get { return MaxTerminationDate; } }

        bool ISwap.IsPaymentDatesSynchronous
        {
            set { this.isPaymentDatesSynchronous = value; }
            get { return this.isPaymentDatesSynchronous; }
        }



		#endregion ISwap Members
		#region IDeclarativeProvision Members
		bool IDeclarativeProvision.CancelableProvisionSpecified { get { return this.cancelableProvisionSpecified; } }
		ICancelableProvision IDeclarativeProvision.CancelableProvision { get { return this.cancelableProvision; } }
		bool IDeclarativeProvision.ExtendibleProvisionSpecified { get { return this.extendibleProvisionSpecified; } }
		IExtendibleProvision IDeclarativeProvision.ExtendibleProvision { get { return this.extendibleProvision; } }
		bool IDeclarativeProvision.EarlyTerminationProvisionSpecified { get { return this.earlyTerminationProvisionSpecified; } }
		IEarlyTerminationProvision IDeclarativeProvision.EarlyTerminationProvision { get { return this.earlyTerminationProvision; } }
		bool IDeclarativeProvision.StepUpProvisionSpecified { get { return this.stepUpProvisionSpecified; } }
		IStepUpProvision IDeclarativeProvision.StepUpProvision { get { return this.stepUpProvision; } }
		bool IDeclarativeProvision.ImplicitProvisionSpecified { get { return implicitProvisionSpecified; } }
		IImplicitProvision IDeclarativeProvision.ImplicitProvision { get { return implicitProvision; } }
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
        #region EventCode
        public string EventCode
        {
            get
            {
                string eventCode = string.Empty;
				if (exerciseAmericanSpecified)
					eventCode = EventCodeFunc.AmericanSwaption;
				else if (exerciseBermudaSpecified)
					eventCode = EventCodeFunc.BermudaSwaption;
				else if (exerciseEuropeanSpecified)
					eventCode = EventCodeFunc.EuropeanSwaption;
                return eventCode;
            }
        }
        #endregion EventCode
        #region EventType
        public string EventType
        {
            get
            {
                string eventType = EventTypeFunc.Regular;
                if (swaptionStraddle.BoolValue)
                    eventType = EventTypeFunc.Straddle;
                // EG 20160404 Migration vs2013
                //else if (false)
                //    eventType = EventTypeFunc.Strangle;
                return eventType;
            }
        }
        #endregion EventType
        #endregion Accessors

        #region IProduct Members
        object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members
		#region ISwaption Members
		IReference ISwaption.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.buyerPartyReference; }
		}
		IReference ISwaption.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
			get { return this.sellerPartyReference; }
		}
		EFS_SwaptionDates ISwaption.Efs_SwaptionDates
		{
			get { return this.efs_SwaptionDates; }
			set { this.efs_SwaptionDates = value; }
		}
		bool ISwaption.ExerciseAmericanSpecified
		{
			set { this.exerciseAmericanSpecified = value; }
			get { return this.exerciseAmericanSpecified; }
		}
		IAmericanExercise ISwaption.ExerciseAmerican
		{
			set { this.exerciseAmerican = (AmericanExercise)value; }
			get { return this.exerciseAmerican; }
		}
		bool ISwaption.ExerciseBermudaSpecified
		{
			set { this.exerciseBermudaSpecified = value; }
			get { return this.exerciseBermudaSpecified; }
		}
		IBermudaExercise ISwaption.ExerciseBermuda
		{
			set { this.exerciseBermuda = (BermudaExercise)value; }
			get { return this.exerciseBermuda; }
		}
		bool ISwaption.ExerciseEuropeanSpecified
		{
			set { this.exerciseEuropeanSpecified = value; }
			get { return this.exerciseEuropeanSpecified; }
		}
		IEuropeanExercise ISwaption.ExerciseEuropean
		{
			set { this.exerciseEuropean = (EuropeanExercise)value; }
			get { return this.exerciseEuropean; }
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
		bool ISwaption.PremiumSpecified
		{
			set { this.premiumSpecified = value; }
			get { return this.premiumSpecified; }
		}
		IPayment[] ISwaption.Premium
		{
			set { this.premium = (Payment[])value; }
			get { return this.premium; }
		}
        bool ISwaption.ExerciseProcedureSpecified
        {
            set { this.procedureSpecified = value; }
            get { return this.procedureSpecified; }
        }
        IExerciseProcedure ISwaption.ExerciseProcedure 
		{
			set { this.procedure = (ExerciseProcedure)value; }
            get { return this.procedure; }
		}
        bool ISwaption.CalculationAgentSpecified
        {
            set { this.calculationAgentSpecified = value; }
            get { return this.calculationAgentSpecified ; }
        }
        ICalculationAgent ISwaption.CalculationAgent
		{
			set { this.calculationAgent = (CalculationAgent)value; }
			get { return this.calculationAgent; }
		}
		bool ISwaption.CashSettlementSpecified
		{
			get { return this.cashSettlementSpecified; }
		}
		ICashSettlement ISwaption.CashSettlement
		{
			set { this.cashSettlement = (CashSettlement)value; }
			get { return this.cashSettlement; }
		}
		EFS_Boolean ISwaption.SwaptionStraddle
		{
			set { this.swaptionStraddle = value; }
			get { return this.swaptionStraddle; }
		}
		bool ISwaption.SwaptionAdjustedDatesSpecified
		{
			get { return this.swaptionAdjustedDatesSpecified; }
		}
		ISwap ISwaption.Swap
		{
			set { this.swap = (Swap)value; }
			get { return this.swap; }
		}
		#endregion ISwaption Members
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
	#endregion Swaption
}
