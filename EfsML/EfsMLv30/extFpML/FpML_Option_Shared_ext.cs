#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EfsML.Enum;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Shared;
using System.Reflection;
#endregion using directives


namespace FpML.v44.Option.Shared
{
    #region Asian
    public partial class Asian : IAsian
	{
		#region Constructors
		public Asian()
		{
			strikeFactor = new EFS_Decimal();
		}
		#endregion Constructors

		#region IAsian Members
		AveragingInOutEnum IAsian.AveragingInOut
		{
			set { this.averagingInOut = value; }
			get { return this.averagingInOut; }
		}
		bool IAsian.StrikeFactorSpecified
		{
			set { this.strikeFactorSpecified = value; }
			get { return this.strikeFactorSpecified; }
		}
		EFS_Decimal IAsian.StrikeFactor
		{
			set { this.strikeFactor = value; }
			get { return this.strikeFactor; }
		}
		bool IAsian.AveragingPeriodInSpecified
		{
			set { this.averagingPeriodInSpecified = value; }
			get { return this.averagingPeriodInSpecified; }
		}
		IEquityAveragingPeriod IAsian.AveragingPeriodIn
		{
			set { this.averagingPeriodIn = (EquityAveragingPeriod)value; }
			get { return this.averagingPeriodIn; }
		}
		bool IAsian.AveragingPeriodOutSpecified
		{
			set { this.averagingPeriodOutSpecified = value; }
			get { return this.averagingPeriodOutSpecified; }
		}
		IEquityAveragingPeriod IAsian.AveragingPeriodOut
		{
			set { this.averagingPeriodOut = (EquityAveragingPeriod)value; }
			get { return this.averagingPeriodOut; }
		}
		bool IAsian.LookBackMethodSpecified
		{
			set { this.lookBackMethodSpecified = value; }
			get { return this.lookBackMethodSpecified; }
		}
		IEmpty IAsian.LookBackMethod
		{
			set { this.lookBackMethod = (Empty)value; }
			get { return this.lookBackMethod; }
		}
		#endregion IAsian Members
	}
	#endregion Asian
	#region AveragingPeriod
	public partial class EquityAveragingPeriod : IEquityAveragingPeriod
	{
		#region IEquityAveragingPeriod Members
		bool IEquityAveragingPeriod.ScheduleSpecified
		{
			set { this.scheduleSpecified = value; }
			get { return this.scheduleSpecified; }
		}
		IAveragingSchedule[] IEquityAveragingPeriod.Schedule
		{
			set { this.schedule = (AveragingSchedule[])value; }
			get { return this.schedule; }
		}
		bool IEquityAveragingPeriod.AveragingDateTimesSpecified
		{
			set { this.averagingDateTimesSpecified = value; }
			get { return this.averagingDateTimesSpecified; }
		}
		IDateTimeList IEquityAveragingPeriod.AveragingDateTimes
		{
			set { this.averagingDateTimes = (DateTimeList)value; }
			get { return this.averagingDateTimes; }
		}
		IScheme IEquityAveragingPeriod.MarketDisruption
		{
			set { this.marketDisruption = (MarketDisruption)value; }
			get { return this.marketDisruption; }
		}
		#endregion IEquityAveragingPeriod Members
	}
	#endregion AveragingPeriod
	#region AveragingSchedule
	public partial class AveragingSchedule : IEFS_Array, IAveragingSchedule
	{
		#region Constructors
		public AveragingSchedule()
		{
			weekNumber = new EFS_Integer();
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
		#region IAveragingSchedule Members
		EFS_Date IAveragingSchedule.StartDate
		{
			set { this.startDate = value; }
			get { return this.startDate; }
		}
		EFS_Date IAveragingSchedule.EndDate
		{
			set { this.endDate = value; }
			get { return this.endDate; }
		}
		EFS_Integer IAveragingSchedule.Frequency
		{
			set { this.frequency = value; }
			get { return this.frequency; }
		}
		/*
		IScheme IAveragingSchedule.frequencyType
		{
			set { this.frequencyType = (FrequencyType)value; }
			get { return this.frequencyType; }
		}
		*/
		FrequencyTypeEnum IAveragingSchedule.FrequencyType
		{
			set { this.frequencyType = value; }
			get { return this.frequencyType; }
		}
		bool IAveragingSchedule.WeekNumberSpecified
		{
			set { this.weekNumberSpecified = value; }
			get { return this.weekNumberSpecified; }
		}
		EFS_Integer IAveragingSchedule.WeekNumber
		{
			set { this.weekNumber = value; }
			get { return this.weekNumber; }
		}
		bool IAveragingSchedule.DayOfWeekSpecified
		{
			set { this.dayOfWeekSpecified = value; }
			get { return this.dayOfWeekSpecified; }
		}
		WeeklyRollConventionEnum IAveragingSchedule.DayOfWeek
		{
			set { this.dayOfWeek = value; }
			get { return this.dayOfWeek; }
		}
		IResetFrequency IAveragingSchedule.ResetFrequency
		{
			get
			{
                ResetFrequency resetFrequency = new ResetFrequency
                {
                    periodMultiplier = new EFS_Integer(frequency.IntValue),
                    period = PeriodEnum.D
                };
                if (dayOfWeekSpecified)
				{
					resetFrequency.period							= PeriodEnum.W;
					resetFrequency.weeklyRollConventionSpecified	= true;
					resetFrequency.weeklyRollConvention				= dayOfWeek;
				}
				if (weekNumberSpecified)
					resetFrequency.period = PeriodEnum.M;
				return resetFrequency;
			}
		}
		#endregion IAveragingSchedule Members
	}
	#endregion AveragingSchedule

	#region Barrier
	public partial class Barrier : IBarrier, IEFS_Array
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IBarrier Members
		bool IBarrier.BarrierCapSpecified
		{
			set { this.barrierCapSpecified = value; }
			get { return this.barrierCapSpecified; }
		}
		ITriggerEvent IBarrier.BarrierCap
		{
			set { this.barrierCap = (TriggerEvent)value; }
			get { return this.barrierCap; }
		}
		bool IBarrier.BarrierFloorSpecified
		{
			set { this.barrierFloorSpecified = value; }
			get { return this.barrierFloorSpecified; }
		}
		ITriggerEvent IBarrier.BarrierFloor
		{
			set { this.barrierFloor = (TriggerEvent)value; }
			get { return this.barrierFloor; }
		}
		#endregion IBarrier Members
	}
	#endregion Barrier

	#region CalendarSpread
	public partial class CalendarSpread : ICalendarSpread
	{
		#region ICalendarSpread Members
		IAdjustableOrRelativeDate ICalendarSpread.ExpirationDateTwo
		{
			set { this.expirationDateTwo = (AdjustableOrRelativeDate)value; }
			get { return this.expirationDateTwo; }
		}
		#endregion ICalendarSpread Members
	}
	#endregion CalendarSpread
    #region Composite
    public partial class Composite : IComposite
    {
		#region Constructors
        public Composite()
		{
		}
		#endregion Constructors

        #region IComposite Members
        IRelativeDateOffset IComposite.RelativeDate
        {
            set { this.relativeDate = (RelativeDateOffset)value; }
            get { return this.relativeDate; }
        }
        bool IComposite.RelativeDateSpecified
        {
            set { this.relativeDateSpecified = value; }
            get { return this.relativeDateSpecified; }
        }
        EFS_MultiLineString IComposite.DeterminationMethod
        {
            set { this.determinationMethod = value; }
            get { return this.determinationMethod; }
        }
        bool IComposite.DeterminationMethodSpecified
        {
            set { this.determinationMethodSpecified = value; }
            get { return this.determinationMethodSpecified; }
        }
        IFxSpotRateSource IComposite.FxSpotRateSource
        {
            set { fxSpotRateSource = (FxSpotRateSource) value; }
            get { return this.fxSpotRateSource; }
        }

        bool IComposite.FxSpotRateSourceSpecified
        {
            set { this.fxSpotRateSourceSpecified = value; }
            get { return this.fxSpotRateSourceSpecified; }
        }
        #endregion IComposite Members
    }
    #endregion Composite

	#region CreditEvents
	public partial class CreditEvents : ICreditEvents
	{
		#region Accessors
		[System.Xml.Serialization.XmlAttributeAttribute("id",Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
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

		#region ICreditEvents Members
		bool ICreditEvents.BankruptcySpecified
		{
			set { this.bankruptcySpecified = value; }
			get { return this.bankruptcySpecified; }
		}
		bool ICreditEvents.FailureToPaySpecified
		{
			set { this.failureToPaySpecified = value; }
			get { return this.failureToPaySpecified; }
		}
		IFailureToPay ICreditEvents.FailureToPay
		{
			set { this.failureToPay = (FailureToPay)value; }
			get { return this.failureToPay; }
		}
		bool ICreditEvents.FailureToPayPrincipalSpecified
		{
			set { this.failureToPayPrincipalSpecified = value; }
			get { return this.failureToPayPrincipalSpecified; }
		}
		bool ICreditEvents.FailureToPayInterestSpecified
		{
			set { this.failureToPayInterestSpecified = value; }
			get { return this.failureToPayInterestSpecified; }
		}
		bool ICreditEvents.ObligationDefaultSpecified
		{
			set { this.obligationDefaultSpecified = value; }
			get { return this.obligationDefaultSpecified; }
		}
		bool ICreditEvents.ObligationAccelerationSpecified
		{
			set { this.obligationAccelerationSpecified = value; }
			get { return this.obligationAccelerationSpecified; }
		}
		bool ICreditEvents.RepudiationMoratoriumSpecified
		{
			set { this.repudiationMoratoriumSpecified = value; }
			get { return this.repudiationMoratoriumSpecified; }
		}
		bool ICreditEvents.RestructuringSpecified
		{
			set { this.restructuringSpecified = value; }
			get { return this.restructuringSpecified; }
		}
		IRestructuring ICreditEvents.Restructuring
		{
			set { this.restructuring = (Restructuring)value; }
			get { return this.restructuring; }
		}
		bool ICreditEvents.DistressedRatingsDowngradeSpecified
		{
			set { this.distressedRatingsDowngradeSpecified = value; }
			get { return this.distressedRatingsDowngradeSpecified; }
		}
		bool ICreditEvents.MaturityExtensionSpecified
		{
			set { this.maturityExtensionSpecified = value; }
			get { return this.maturityExtensionSpecified; }
		}
		bool ICreditEvents.WritedownSpecified
		{
			set { this.writedownSpecified = value; }
			get { return this.writedownSpecified; }
		}
		bool ICreditEvents.DefaultRequirementSpecified
		{
			set { this.defaultRequirementSpecified = value; }
			get { return this.defaultRequirementSpecified; }
		}
		IMoney ICreditEvents.DefaultRequirement
		{
			set { this.defaultRequirement = (Money)value; }
			get { return this.defaultRequirement; }
		}
		bool ICreditEvents.CreditEventNoticeSpecified
		{
			set { this.creditEventNoticeSpecified = value; }
			get { return this.creditEventNoticeSpecified; }
		}
		ICreditEventNotice ICreditEvents.CreditEventNotice
		{
			set { this.creditEventNotice = (CreditEventNotice)value; }
			get { return this.creditEventNotice; }
		}
		#endregion ICreditEvents Members
	}
	#endregion CreditEvents
	#region CreditEventNotice
	public partial class CreditEventNotice : ICreditEventNotice
	{
		#region ICreditEventNotice Members
		INotifyingParty ICreditEventNotice.NotifyingParty
		{
			set { this.notifyingParty = (NotifyingParty)value; }
			get { return this.notifyingParty; }
		}
		bool ICreditEventNotice.BusinessCenterSpecified
		{
			set { this.businessCenterSpecified = value; }
			get { return this.businessCenterSpecified; }
		}
		IBusinessCenter ICreditEventNotice.BusinessCenter
		{
			set { this.businessCenter = (BusinessCenter)value; }
			get { return this.businessCenter; }
		}
		bool ICreditEventNotice.PubliclyAvailableInformationSpecified
		{
			set { this.publiclyAvailableInformationSpecified = value; }
			get { return this.publiclyAvailableInformationSpecified; }
		}
		IPubliclyAvailableInformation ICreditEventNotice.PubliclyAvailableInformation
		{
			set { this.publiclyAvailableInformation = (PubliclyAvailableInformation)value; }
			get { return this.publiclyAvailableInformation; }
		}
		#endregion ICreditEventNotice Members
	}
	#endregion CreditEventNotice
	#region CreditEventsReference
	public partial class CreditEventsReference : IReference
	{
		#region IReference Members
		string IReference.HRef
		{
			get { return this.href; }
			set { this.href = value; }
		}
		#endregion IReference Members
	}
	#endregion CreditEventsReference


	#region FailureToPay
	public partial class FailureToPay : IFailureToPay
	{
		#region IFailureToPay Members
		bool IFailureToPay.GracePeriodExtensionSpecified
		{
			set { this.gracePeriodExtensionSpecified = value; }
			get { return this.gracePeriodExtensionSpecified; }
		}
		IGracePeriodExtension IFailureToPay.GracePeriodExtension
		{
			set { this.gracePeriodExtension = (GracePeriodExtension)value; }
			get { return this.gracePeriodExtension; }
		}
		bool IFailureToPay.PaymentRequirementSpecified
		{
			set { this.paymentRequirementSpecified = value; }
			get { return this.paymentRequirementSpecified; }
		}
		IMoney IFailureToPay.PaymentRequirement
		{
			set { this.paymentRequirement = (Money)value; }
			get { return this.paymentRequirement; }
		}
		#endregion IFailureToPay Members
	}
	#endregion FailureToPay
	#region FeaturePayment
	public partial class FeaturePayment : IFeaturePayment
	{
		#region Constructors
		public FeaturePayment()
		{
			pmtLevelPercentage = new EFS_Decimal();
			pmtAmount = new EFS_Decimal();
		}
		#endregion Constructors

		#region IFeaturePayment Members
		IReference IFeaturePayment.PayerPartyReference
		{
			set { this.payerPartyReference = (PartyOrAccountReference)value; }
			get { return this.payerPartyReference; }
		}
		IReference IFeaturePayment.ReceiverPartyReference
		{
			set { this.receiverPartyReference = (PartyOrAccountReference)value; }
			get { return this.receiverPartyReference; }
		}
		bool IFeaturePayment.LevelPercentageSpecified
		{
			set { this.pmtLevelPercentageSpecified = value; }
			get { return this.pmtLevelPercentageSpecified; }
		}
		EFS_Decimal IFeaturePayment.LevelPercentage
		{
			set { this.pmtLevelPercentage = value; }
			get { return this.pmtLevelPercentage; }
		}
		bool IFeaturePayment.AmountSpecified
		{
			set { this.pmtAmountSpecified = value; }
			get { return this.pmtAmountSpecified; }
		}
		EFS_Decimal IFeaturePayment.Amount
		{
			set { this.pmtAmount = value; }
			get { return this.pmtAmount; }
		}
		bool IFeaturePayment.TimeSpecified
		{
			set { this.timeSpecified = value; }
			get { return this.timeSpecified; }
		}
		TimeTypeEnum IFeaturePayment.Time
		{
			set { this.time = value; }
			get { return this.time; }
		}
		bool IFeaturePayment.CurrencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}
		ICurrency IFeaturePayment.Currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		bool IFeaturePayment.FeaturePaymentDateSpecified
		{
			set { this.featurePaymentDateSpecified = value; }
			get { return this.featurePaymentDateSpecified; }
		}
		IAdjustableOrRelativeDate IFeaturePayment.FeaturePaymentDate
		{
			set { this.featurePaymentDate = (AdjustableOrRelativeDate)value; }
			get { return this.featurePaymentDate; }
		}
		#endregion IFeaturePayment Members
	}
	#endregion FeaturePayment
	#region FrequenceType
	public partial class FrequencyType : IScheme
	{
		#region Constructors
		public FrequencyType()
		{
			frequencyTypeScheme = "http://www.fpml.org/coding-scheme/market-disruption-1-0";
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.Scheme
		{
			set { this.frequencyTypeScheme = value; }
			get { return this.frequencyTypeScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion FrequencyType

	#region FxFeature
	public partial class FxFeature : IFxFeature
	{
		#region Constructors
		public FxFeature()
		{
			referenceCurrency = new IdentifiedCurrency();
			fxFeatureQuanto = new Quanto();
			fxFeatureComposite = new Composite();
			fxFeatureCrossCurrency = new Composite();
		}
		#endregion Constructors

		#region IFxFeature Members
		ISchemeId IFxFeature.ReferenceCurrency
		{
			set { this.referenceCurrency = (IdentifiedCurrency)value; }
			get { return this.referenceCurrency; }
		}
        bool IFxFeature.FxFeatureCompositeSpecified
        {
            set { this.fxFeatureCompositeSpecified = value; }
            get { return this.fxFeatureCompositeSpecified; }
        }
        IComposite IFxFeature.FxFeatureComposite
        {
            set { this.fxFeatureComposite = (Composite)value; }
            get { return this.fxFeatureComposite; }
        }
        bool IFxFeature.FxFeatureQuantoSpecified
        {
            set { this.fxFeatureQuantoSpecified = value; }
            get { return this.fxFeatureQuantoSpecified; }
        }
        IQuanto IFxFeature.FxFeatureQuanto
        {
            set { this.fxFeatureQuanto = (Quanto)value; }
            get { return this.fxFeatureQuanto; }
        }

        bool IFxFeature.FxFeatureCrossCurrencySpecified
		{
            set { this.fxFeatureCrossCurrencySpecified = value; }
            get { return this.fxFeatureCrossCurrencySpecified; }
		}
        IComposite IFxFeature.FxFeatureCrossCurrency
		{
            set { this.fxFeatureCrossCurrency = (Composite)value; }
            get { return this.fxFeatureCrossCurrency; }
		}
		#endregion IFxFeature Members
	}
	#endregion FxFeature

	#region GracePeriodExtension
	public partial class GracePeriodExtension : IGracePeriodExtension
	{
		#region IGracePeriodExtension Members
		IOffset IGracePeriodExtension.GracePeriod
		{
			set { this.gracePeriod = (Offset)value; }
			get { return this.gracePeriod; }
		}
		#endregion IGracePeriodExtension Members
	}
	#endregion GracePeriodExtension

	#region Knock
	public partial class Knock : IKnock
	{
		#region IKnock Members
		bool IKnock.KnockInSpecified
		{
			set { this.knockInSpecified = value; }
			get { return this.knockInSpecified; }
		}
		ITriggerEvent IKnock.KnockIn
		{
			set { this.knockIn = (TriggerEvent)value; }
			get { return this.knockIn; }
		}
		bool IKnock.KnockOutSpecified
		{
			set { this.knockOutSpecified = value; }
			get { return this.knockOutSpecified; }
		}
		ITriggerEvent IKnock.KnockOut
		{
			set { this.knockOut = (TriggerEvent)value; }
			get { return this.knockOut; }
		}
		#endregion IKnock Members
	}
	#endregion Knock

	#region MarketDisruption
	public partial class MarketDisruption : IScheme
	{
		#region Constructors
		public MarketDisruption()
		{
			marketDisruptionScheme = "http://www.fpml.org/coding-scheme/market-disruption-1-0";
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.Scheme
		{
			set { this.marketDisruptionScheme = value; }
			get { return this.marketDisruptionScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion MarketDisruption

	#region NotifyingParty
	public partial class NotifyingParty : INotifyingParty
	{
		#region INotifyingParty Members
		IReference INotifyingParty.BuyerPartyReference
		{
			set { this.buyerPartyReference = (PartyReference)value; }
			get { return this.buyerPartyReference; }
		}
		bool INotifyingParty.SellerPartyReferenceSpecified
		{
			set { this.sellerPartyReferenceSpecified = value; }
			get { return this.sellerPartyReferenceSpecified; }
		}
		IReference INotifyingParty.SellerPartyReference
		{
			set { this.sellerPartyReference = (PartyReference)value; }
			get { return this.sellerPartyReference; }
		}
		#endregion INotifyingParty Members
	}
	#endregion NotifyingParty

	#region OptionBase
    // EG 20150410 [20513] BANCAPERTA
	public abstract partial class OptionBase : IProduct, IOptionBase
	{
		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

        #region IOptionBase Membres
        // EG 20150410 [20513] BANCAPERTA
        IReference IOptionBase.BuyerPartyReference
        {
            set { this.buyerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.buyerPartyReference; }
        }

        IReference IOptionBase.SellerPartyReference
        {
            set { this.sellerPartyReference = (PartyOrTradeSideReference)value; }
            get { return this.sellerPartyReference; }
        }

        OptionTypeEnum IOptionBase.OptionType
        {
            set { this.optionType = value; }
            get { return this.optionType; }
        }
        #endregion IOptionBase Membres
	}
	#endregion OptionBase
	#region OptionBaseExtended
    /// EG 20150422 [20513] BANCAPERTA New INbOptionsAndNotionalBase
    public abstract partial class OptionBaseExtended : IProduct, IOptionBaseExtended, INbOptionsAndNotionalBase
    {
        #region Accessors
        #region EFS_Exercise
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public object EFS_Exercise
        {
            set
            {
                if (optionExerciseAmericanSpecified)
                    optionExerciseAmerican = (AmericanExercise)value;
                else if (optionExerciseBermudaSpecified)
                    optionExerciseBermuda = (BermudaExercise)value;
                else if (optionExerciseEuropeanSpecified)
                    optionExerciseEuropean = (EuropeanExercise)value;
            }
            get
            {
                if (optionExerciseAmericanSpecified)
                    return optionExerciseAmerican;
                else if (optionExerciseBermudaSpecified)
                    return optionExerciseBermuda;
                else if (optionExerciseEuropeanSpecified)
                    return optionExerciseEuropean;
                else
                    return null;
            }
        }
        #endregion EFS_Exercise
        #endregion Accessors
        #region Constructors
        public OptionBaseExtended()
		{
            numberOfOptions = new EFS_Decimal();
            optionEntitlement = new EFS_Decimal();
            premium = new Premium();
			optionExerciseAmerican = new AmericanExercise();
			optionExerciseBermuda = new BermudaExercise();
			optionExerciseEuropean = new EuropeanExercise();
			optionNotionalAmount = new Money();
			optionNotionalReference = new NotionalAmountReference();
			optionSettlementCurrency = new Currency();
			optionSettlementAmount = new Money();
            optionSettlementNone = new Empty();
            settlementDate = new AdjustableOrRelativeDate();
		}
		#endregion Constructors

		#region IProduct Members
		object IProduct.Product { get { return this; } }
		IProductBase IProduct.ProductBase { get { return this; } }
		//IProduct[] IProduct.ProductsStrategy { get { return null; } }
		#endregion IProduct Members

        #region IOptionBaseExtended Membres
        // EG 20150410 [20513] BANCAPERTA
        bool IOptionBaseExtended.PremiumSpecified
        {
            set { this.premiumSpecified = value; }
            get { return this.premiumSpecified; }
        }
        IPremium IOptionBaseExtended.Premium
        {
            set { this.premium = (Premium)value; }
            get { return this.premium; }
        }
        ExerciseStyleEnum IOptionBaseExtended.GetStyle
        {
            get
            {
                if (this.optionExerciseAmericanSpecified)
                    return ExerciseStyleEnum.American;
                else if (this.optionExerciseBermudaSpecified)
                    return ExerciseStyleEnum.Bermuda;
                return ExerciseStyleEnum.European;
            }
        }
        object IOptionBaseExtended.EFS_Exercise
        {
            set { this.EFS_Exercise = value; }
            get { return this.EFS_Exercise; }
        }
        bool IOptionBaseExtended.AmericanExerciseSpecified
        {
            set { this.optionExerciseAmericanSpecified = value; }
            get { return this.optionExerciseAmericanSpecified; }
        }
        IAmericanExercise IOptionBaseExtended.AmericanExercise
        {
            set { this.optionExerciseAmerican = (AmericanExercise)value; }
            get { return this.optionExerciseAmerican; }
        }
        bool IOptionBaseExtended.BermudaExerciseSpecified
        {
            set { this.optionExerciseBermudaSpecified = value; }
            get { return this.optionExerciseBermudaSpecified; }
        }
        IBermudaExercise IOptionBaseExtended.BermudaExercise
        {
            set { this.optionExerciseBermuda = (BermudaExercise)value; }
            get { return this.optionExerciseBermuda; }
        }
        bool IOptionBaseExtended.EuropeanExerciseSpecified
        {
            set { this.optionExerciseEuropeanSpecified = value; }
            get { return this.optionExerciseEuropeanSpecified; }
        }
        IEuropeanExercise IOptionBaseExtended.EuropeanExercise
        {
            set { this.optionExerciseEuropean = (EuropeanExercise)value; }
            get { return this.optionExerciseEuropean; }
        }
        IExerciseProcedure IOptionBaseExtended.ExerciseProcedure
        {
            set { this.procedure = (ExerciseProcedure)value; }
            get { return this.procedure; }
        }
        bool IOptionBaseExtended.FeatureSpecified
        {
            set { this.featureSpecified = value; }
            get { return this.featureSpecified; }
        }
        IOptionFeature IOptionBaseExtended.Feature
        {
            set { this.feature = (OptionFeature)value; }
            get { return this.feature; }
        }
        bool IOptionBaseExtended.NotionalAmountSpecified
        {
            set { this.optionNotionalAmountSpecified = value; }
            get { return this.optionNotionalAmountSpecified; }
        }
        IMoney IOptionBaseExtended.NotionalAmount
        {
            set { this.optionNotionalAmount = (Money)value; }
            get { return this.optionNotionalAmount; }
        }
        bool IOptionBaseExtended.NotionalAmountReferenceSpecified
        {
            set { this.optionNotionalReferenceSpecified = value; }
            get { return this.optionNotionalReferenceSpecified; }
        }
        IReference IOptionBaseExtended.NotionalAmountReference
        {
            set { this.optionNotionalReference = (NotionalAmountReference) value; }
            get { return (IReference)this.optionNotionalReference; }
        }
        EFS_Decimal IOptionBaseExtended.OptionEntitlement
        {
            set { this.optionEntitlement = value; }
            get { return this.optionEntitlement; }
        }
        bool IOptionBaseExtended.EntitlementCurrencySpecified
        {
            set { this.entitlementCurrencySpecified = value; }
            get { return this.entitlementCurrencySpecified; }
        }
        ICurrency IOptionBaseExtended.EntitlementCurrency
        {
            set { this.entitlementCurrency = (Currency)value; }
            get { return this.entitlementCurrency; }
        }
        bool IOptionBaseExtended.NumberOfOptionsSpecified
        {
            set { this.numberOfOptionsSpecified = value; }
            get { return this.numberOfOptionsSpecified; }
        }
        EFS_Decimal IOptionBaseExtended.NumberOfOptions
        {
            set { this.numberOfOptions = value; }
            get { return this.numberOfOptions; }
        }
        bool IOptionBaseExtended.SettlementTypeSpecified
        {
            set { this.settlementTypeSpecified = value; }
            get { return this.settlementTypeSpecified; }
        }
        SettlementTypeEnum IOptionBaseExtended.SettlementType
        {
            set { this.settlementType = value; }
            get { return this.settlementType; }
        }
        bool IOptionBaseExtended.SettlementDateSpecified
        {
            set { this.settlementDateSpecified = value; }
            get { return this.settlementDateSpecified; }
        }
        IAdjustableOrRelativeDate IOptionBaseExtended.SettlementDate
        {
            set { this.settlementDate = (AdjustableOrRelativeDate)value; }
            get { return this.settlementDate; }
        }
        bool IOptionBaseExtended.SettlementAmountSpecified
        {
            set { this.optionSettlementAmountSpecified = value; }
            get { return this.optionSettlementAmountSpecified; }
        }
        IMoney IOptionBaseExtended.SettlementAmount
        {
            set { this.optionSettlementAmount = (Money)value; }
            get { return this.optionSettlementAmount; }
        }
        bool IOptionBaseExtended.SettlementCurrencySpecified
        {
            set { this.optionSettlementCurrencySpecified = value; }
            get { return this.optionSettlementCurrencySpecified; }
        }
        ICurrency IOptionBaseExtended.SettlementCurrency
        {
            set { this.optionSettlementCurrency = (Currency)value; }
            get { return this.optionSettlementCurrency; }
        }
        IOptionFeature IOptionBaseExtended.CreateOptionFeature
        {
            get { return new OptionFeature(); }
        }
        #endregion IOptionBaseExtended Mebres

        #region INbOptionsAndNotionalBase Membres
        bool INbOptionsAndNotionalBase.NumberOfOptionsSpecified
        {
            get { return numberOfOptionsSpecified; }
        }
        EFS_Decimal INbOptionsAndNotionalBase.NumberOfOptions
        {
            get { return numberOfOptions; }
        }
        EFS_Decimal INbOptionsAndNotionalBase.OptionEntitlement
        {
            get { return optionEntitlement; }
        }
        bool INbOptionsAndNotionalBase.NotionalSpecified
        {
            get { return optionNotionalAmountSpecified; }
        }
        IMoney INbOptionsAndNotionalBase.Notional
        {
            get { return optionNotionalAmount; }
        }
        #endregion INbOptionsAndNotionalBase Membres
    }
	#endregion OptionBaseExtended
    #region OptionFeature
    // EG 20150410 [20513] BANCAPERTA
    public partial class OptionFeature : IOptionFeature
    {
        #region Constructors
        public OptionFeature()
		{
            asian = new Asian();
            barrier = new Barrier();
            fxFeature = new FxFeature();
            knock = new Knock();
            passThrough = new PassThrough();
            strategyFeature = new StrategyFeature();
		}
		#endregion Constructors

        #region IOptionFeature Membres
        bool IOptionFeature.FxFeatureSpecified
        {
            set { this.fxFeatureSpecified = value; }
            get { return this.fxFeatureSpecified; }
        }

        IFxFeature IOptionFeature.FxFeature
        {
            set { this.fxFeature = (FxFeature)value; }
            get { return this.fxFeature; }
        }
        bool IOptionFeature.StrategyFeatureSpecified
        {
            set { this.strategyFeatureSpecified = value; }
            get { return this.strategyFeatureSpecified; }
        }
        IStrategyFeature IOptionFeature.StrategyFeature
        {
            set { this.strategyFeature = (StrategyFeature)value; }
            get { return this.strategyFeature; }
        }
        bool IOptionFeature.AsianSpecified
        {
            set { this.asianSpecified = value; }
            get { return this.asianSpecified; }
        }
        IAsian IOptionFeature.Asian
        {
            set { this.asian = (Asian)value; }
            get { return this.asian; }
        }
        bool IOptionFeature.BarrierSpecified
        {
            set { this.barrierSpecified = value; }
            get { return this.barrierSpecified; }
        }
        IBarrier IOptionFeature.Barrier
        {
            set { this.barrier = (Barrier)value; }
            get { return this.barrier; }
        }
        bool IOptionFeature.KnockSpecified
        {
            set { this.knockSpecified = value; }
            get { return this.knockSpecified; }
        }
        IKnock IOptionFeature.Knock
        {
            set { this.knock = (Knock)value; }
            get { return this.knock; }
        }
        bool IOptionFeature.PassThroughSpecified
        {
            set { this.passThroughSpecified = value; }
            get { return this.passThroughSpecified; }
        }
        IPassThrough IOptionFeature.PassThrough
        {
            set { this.passThrough = (PassThrough)value; }
            get { return this.passThrough; }
        }
        #endregion IOptionFeature Membres
    }
    #endregion OptionFeature

	#region OptionStrike
    // EG 20150410 [20513] BANCAPERTA
	public partial class OptionStrike : IOptionStrike
	{
		#region Constructors
		public OptionStrike()
		{
			typeStrikePrice = new EFS_Decimal();
			typeStrikePercentage = new EFS_Decimal();
            currency = new Currency();
		}
		#endregion Constructors
		#region IOptionStrike Members
		bool IOptionStrike.PriceSpecified
		{
			set { this.typeStrikePriceSpecified = value; }
			get { return this.typeStrikePriceSpecified; }
		}
		EFS_Decimal IOptionStrike.Price
		{
			set { this.typeStrikePrice = value; }
			get { return this.typeStrikePrice; }
		}
		bool IOptionStrike.PercentageSpecified
		{
			set { this.typeStrikePercentageSpecified = value; }
			get { return this.typeStrikePercentageSpecified; }
		}
		EFS_Decimal IOptionStrike.Percentage
		{
			set { this.typeStrikePercentage = value; }
			get { return this.typeStrikePercentage; }
		}
		bool IOptionStrike.CurrencySpecified
		{
			set { this.currencySpecified = value; }
			get { return this.currencySpecified; }
		}
		ICurrency IOptionStrike.Currency
		{
			set { this.currency = (Currency)value; }
			get { return this.currency; }
		}
		#endregion IOptionStrike Members
	}
	#endregion OptionStrike


    public partial class PassThrough : IPassThrough 
    {

        #region IPassThrough Membres
        IPassThroughItem[] IPassThrough.PassThroughItem
        {
            get
            {
                return this.passThroughItem;  
            }
            set
            {
                passThroughItem = (PassThroughItem[])  value;
            }
        }
        #endregion
    }


    #region PassThroughItem
	public partial class PassThroughItem : IEFS_Array, IPassThroughItem
	{
		#region IEFS_Array Members
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
		{
			return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
		}
		#endregion DisplayArray
		#endregion IEFS_Array Members
		#region IPassThroughItem Members
		IReference IPassThroughItem.PayerPartyReference
		{
			set { this.payerPartyReference = (PartyOrAccountReference)value; }
			get { return this.payerPartyReference; }
		}
		IReference IPassThroughItem.ReceiverPartyReference
		{
			set { this.receiverPartyReference = (PartyOrAccountReference)value; }
			get { return this.receiverPartyReference; }
		}
		IReference IPassThroughItem.UnderlyerReference
		{
			set { this.underlyerReference = (AssetReference)value; }
			get { return this.underlyerReference; }
		}
		EFS_Decimal IPassThroughItem.PassThroughPercentage
		{
			set { this.passThroughPercentage = value; }
			get { return this.passThroughPercentage; }
		}
		#endregion IPassThroughItem Members
	}
	#endregion PassThroughItem

    #region Premium
    // EG 20150410 [20513] BANCAPERTA
    // EG 20150422 [20513] BANCAPERTA New IPremiumBase|valuationType
    public partial class Premium : IPremium, IPremiumBase
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        private PremiumAmountValuationTypeEnum valuationType;

        #region IPremium Membres
        PremiumAmountValuationTypeEnum IPremium.ValuationType
        {
            set { this.valuationType = (PremiumAmountValuationTypeEnum)value; }
            get
            {
                PremiumAmountValuationTypeEnum valuationType = this.valuationType;
                if (this.pricePerOptionSpecified)
                    valuationType = PremiumAmountValuationTypeEnum.PricePerOption;
                else if (this.percentageOfNotionalSpecified)
                    valuationType = PremiumAmountValuationTypeEnum.PercentageOfNotional;
                return valuationType;
            }
        }
        bool IPremium.PremiumTypeSpecified
        {
            set { this.premiumTypeSpecified = value; }
            get { return this.premiumTypeSpecified; }
        }
        PremiumTypeEnum IPremium.PremiumType
        {
            set { this.premiumType = value; }
            get { return this.premiumType; }
        }
        bool IPremium.PricePerOptionSpecified
        {
            set { this.pricePerOptionSpecified = value; }
            get { return this.pricePerOptionSpecified; }
        }
        IMoney IPremium.PricePerOption
        {
            set { this.pricePerOption = (Money)value; }
            get { return this.pricePerOption; }
        }
        bool IPremium.PercentageOfNotionalSpecified
        {
            set { this.percentageOfNotionalSpecified = value; }
            get { return this.percentageOfNotionalSpecified; }
        }
        EFS_Decimal IPremium.PercentageOfNotional
        {
            set { this.percentageOfNotional = value; }
            get { return this.percentageOfNotional; }
        }
        bool IPremium.DiscountFactorSpecified
        {
            set { this.discountFactorSpecified = value; }
            get { return this.discountFactorSpecified; }
        }
        EFS_Decimal IPremium.DiscountFactor
        {
            set { this.discountFactor = value; }
            get { return this.discountFactor; }
        }
        bool IPremium.PresentValueAmountSpecified
        {
            set { this.presentValueAmountSpecified = value; }
            get { return this.presentValueAmountSpecified; }
        }
        IMoney IPremium.PresentValueAmount
        {
            set { this.presentValueAmount = (Money)value; }
            get { return this.presentValueAmount; }
        }
        #endregion IPremium Membres
        #region ISimplePayment Membres
        IReference ISimplePayment.PayerPartyReference
        {
            set { this.payerPartyReference = (PartyOrAccountReference)value; }
            get { return this.payerPartyReference; }
        }
        IReference ISimplePayment.ReceiverPartyReference
        {
            set { this.receiverPartyReference = (PartyOrAccountReference)value; }
            get { return this.receiverPartyReference; }
        }
        IAdjustableOrRelativeAndAdjustedDate ISimplePayment.PaymentDate
        {
            set { this.paymentDate = (AdjustableOrRelativeAndAdjustedDate)value; }
            get { return this.paymentDate; }
        }
        IMoney ISimplePayment.PaymentAmount
        {
            set { this.paymentAmount = (Money)value; }
            get { return this.paymentAmount; }
        }
        #endregion ISimplePayment membres

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
    #endregion Premium
	#region PubliclyAvailableInformation
	public partial class PubliclyAvailableInformation : IPubliclyAvailableInformation
	{
		#region IPubliclyAvailableInformation Members
		bool IPubliclyAvailableInformation.StandardPublicSourcesSpecified
		{
			set { this.standardPublicSourcesSpecified = value; }
			get { return this.standardPublicSourcesSpecified; }
		}
		bool IPubliclyAvailableInformation.PublicSourceSpecified
		{
			set { this.publicSourceSpecified = value; }
			get { return this.publicSourceSpecified; }
		}
		EFS_StringArray[] IPubliclyAvailableInformation.PublicSource
		{
			set { this.publicSource = value; }
			get { return this.publicSource; }
		}
		bool IPubliclyAvailableInformation.SpecifiedNumberSpecified
		{
			set { this.specifiedNumberSpecified = value; }
			get { return this.specifiedNumberSpecified; }
		}
		EFS_PosInteger IPubliclyAvailableInformation.SpecifiedNumber
		{
			set { this.specifiedNumber = value; }
			get { return this.specifiedNumber; }
		}
		#endregion IPubliclyAvailableInformation Members
	}
	#endregion PubliclyAvailableInformation

	#region Quanto
	public partial class Quanto : IQuanto
	{
		#region IQuanto Members
		IFxRate[] IQuanto.CreateFxRate { get {return new FxRate[1] { new FxRate() }; } }
		IFxRate[] IQuanto.FxRate
		{
			set { this.fxRate = (FxRate[])value; }
			get { return this.fxRate; }
		}
		#endregion IQuanto Members
	}
	#endregion Quanto

	#region Restructuring
	public partial class Restructuring : IRestructuring
	{
		#region IRestructuring Members
		bool IRestructuring.RestructuringTypeSpecified
		{
			set { this.restructuringTypeSpecified = value; }
			get { return this.restructuringTypeSpecified; }
		}
		IScheme IRestructuring.RestructuringType
		{
			set { this.restructuringType = (RestructuringType)value; }
			get { return this.restructuringType; }
		}
		bool IRestructuring.MultipleHolderObligationSpecified
		{
			set { this.multipleHolderObligationSpecified = value; }
			get { return this.multipleHolderObligationSpecified; }
		}
		bool IRestructuring.MultipleCreditEventNoticesSpecified
		{
			set { this.multipleCreditEventNoticesSpecified = value; }
			get { return this.multipleCreditEventNoticesSpecified; }
		}
		#endregion IRestructuring Members
	}
	#endregion Restructuring
	#region RestructuringType
	public partial class RestructuringType : IScheme
	{
		#region Constructors
		public RestructuringType()
		{
			restructuringScheme = "http://www.fpml.org/coding-scheme/restructuring-1-0";
		}
		#endregion Constructors

		#region IScheme Members
		string IScheme.Scheme
		{
			set { this.restructuringScheme = value; }
			get { return this.restructuringScheme; }
		}
		string IScheme.Value
		{
			set { this.Value = value; }
			get { return this.Value; }
		}
		#endregion IScheme Members
	}
	#endregion RestructuringType


	#region StrategyFeature
	public partial class StrategyFeature : IStrategyFeature
	{
		#region Constructors
		public StrategyFeature()
		{
			strategyFeatureCalendarSpread = new CalendarSpread();
			strategyFeatureStrikeSpread = new StrikeSpread();
		}
		#endregion Constructors

		#region IStrategyFeature Members
		bool IStrategyFeature.CalendarSpreadSpecified
		{
			set { this.strategyFeatureCalendarSpreadSpecified = value; }
			get { return this.strategyFeatureCalendarSpreadSpecified; }
		}
		ICalendarSpread IStrategyFeature.CalendarSpread
		{
			set { this.strategyFeatureCalendarSpread = (CalendarSpread)value; }
			get { return this.strategyFeatureCalendarSpread; }
		}
		bool IStrategyFeature.StrikeSpreadSpecified
		{
			set { this.strategyFeatureStrikeSpreadSpecified = value; }
			get { return this.strategyFeatureStrikeSpreadSpecified; }
		}
		IStrikeSpread IStrategyFeature.StrikeSpread
		{
			set { this.strategyFeatureStrikeSpread = (StrikeSpread)value; }
			get { return this.strategyFeatureStrikeSpread; }
		}
		#endregion IStrategyFeature Members
	}
	#endregion StrategyFeature
	#region StrikeSpread
	public partial class StrikeSpread : IStrikeSpread
	{
		#region IStrikeSpread Members
		IOptionStrike IStrikeSpread.UpperStrike
		{
			set { this.upperStrike = (OptionStrike)value; }
			get { return this.upperStrike; }
		}
		EFS_Decimal IStrikeSpread.UpperStrikeNumberOfOptions
		{
			set { this.upperStrikeNumberOfOptions = value; }
			get { return this.upperStrikeNumberOfOptions; }
		}
		#endregion IStrikeSpread Members
	}
	#endregion StrikeSpread

	#region Trigger
	public partial class Trigger : ITrigger
	{
		#region Constructors
		public Trigger()
		{
			typeLevelLevel = new EFS_Decimal();
			typeLevelLevelPercentage = new EFS_Decimal();
			typeLevelCreditEvents = new CreditEvents();
			typeLevelCreditEventsReference = new CreditEventsReference();
		}
		#endregion Constructors

		#region ITrigger Members
		bool ITrigger.LevelSpecified
		{
			set { this.typeLevelLevelSpecified = value; }
			get { return this.typeLevelLevelSpecified; }
		}
		EFS_Decimal ITrigger.Level
		{
			set { this.typeLevelLevel = value; }
			get { return this.typeLevelLevel; }
		}
		bool ITrigger.LevelPercentageSpecified
		{
			set { this.typeLevelLevelPercentageSpecified = value; }
			get { return this.typeLevelLevelPercentageSpecified; }
		}
		EFS_Decimal ITrigger.LevelPercentage
		{
			set { this.typeLevelLevelPercentage = value; }
			get { return this.typeLevelLevelPercentage; }
		}
		bool ITrigger.CreditEventsSpecified
		{
			set { this.typeLevelCreditEventsSpecified = value; }
			get { return this.typeLevelCreditEventsSpecified; }
		}
		ICreditEvents ITrigger.CreditEvents
		{
			set { this.typeLevelCreditEvents = (CreditEvents)value; }
			get { return this.typeLevelCreditEvents; }
		}
		bool ITrigger.CreditEventsReferenceSpecified
		{
			set { this.typeLevelCreditEventsReferenceSpecified = value; }
			get { return this.typeLevelCreditEventsReferenceSpecified; }
		}
		IReference ITrigger.CreditEventsReference
		{
			set { this.typeLevelCreditEventsReference = (CreditEventsReference)value; }
			get { return this.typeLevelCreditEventsReference; }
		}
		#endregion ITrigger Members
	}
	#endregion Trigger
	#region TriggerEvent
	public partial class TriggerEvent : ITriggerEvent
	{
		#region ITriggerEvent Members
		bool ITriggerEvent.ScheduleSpecified
		{
			set { this.scheduleSpecified = value; }
			get { return this.scheduleSpecified; }
		}
		IAveragingSchedule[] ITriggerEvent.Schedule
		{
			set { this.schedule = (AveragingSchedule[])value; }
			get { return this.schedule; }
		}
		bool ITriggerEvent.TriggerDatesSpecified
		{
			set { this.triggerDatesSpecified = value; }
			get { return this.triggerDatesSpecified; }
		}
		IDateList ITriggerEvent.TriggerDates
		{
			set { this.triggerDates = (DateList)value; }
			get { return this.triggerDates; }
		}
		ITrigger ITriggerEvent.Trigger
		{
			set { this.trigger = (Trigger)value; }
			get { return this.trigger; }
		}
		bool ITriggerEvent.FeaturePaymentSpecified
		{
			set { this.featurePaymentSpecified = value; }
			get { return this.featurePaymentSpecified; }
		}
		IFeaturePayment ITriggerEvent.FeaturePayment
		{
			set { this.featurePayment = (FeaturePayment)value; }
			get { return this.featurePayment; }
		}
		#endregion ITriggerEvent Members
	}
	#endregion TriggerEvent
}
