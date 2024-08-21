#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EfsML.Enum;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.CorrelationSwaps;
using FpML.v44.Cd;
using FpML.v44.DividendSwaps;
using FpML.v44.Enum;
using FpML.v44.Eq.Shared;
using FpML.v44.Shared;
using FpML.v44.VarianceSwaps;
#endregion using directives


namespace FpML.v44.Option.Shared
{
    #region Asian
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Asian : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("averagingInOut", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "averaging", Width = 100)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingInOutEnum averagingInOut;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeFactorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strikeFactor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike factor")]
        public EFS_Decimal strikeFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodInSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodIn", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging In period")]
        public EquityAveragingPeriod averagingPeriodIn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Out period")]
        public EquityAveragingPeriod averagingPeriodOut;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool lookBackMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("lookBackMethod", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "LookBack method")]
		public Empty lookBackMethod;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:Asian";
        #endregion Members
    }
    #endregion Asian
    #region AveragingSchedule
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AveragingSchedule
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("startDate", Order = 1)]
        [ControlGUI(Name = "Start Date")]
        public EFS_Date startDate;
        [System.Xml.Serialization.XmlElementAttribute("endDate", Order = 2)]
        [ControlGUI(Name = "End Date")]
        public EFS_Date endDate;
        [System.Xml.Serialization.XmlElementAttribute("frequency", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency", IsVisible = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "value", Width = 75)]
        public EFS_Integer frequency;
        [System.Xml.Serialization.XmlElementAttribute("frequencyType", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency type", Width = 70)]
        //public FrequencyType frequencyType;
		public FrequencyTypeEnum frequencyType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weekNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("weekNumber", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Week number", Width = 75)]
        public EFS_Integer  weekNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayOfWeekSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayOfWeek", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Day of week", Width = 100)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public WeeklyRollConventionEnum dayOfWeek;
        #endregion Members
    }
    #endregion AveragingSchedule

    #region Barrier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Barrier : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierCapSpecified;
        [System.Xml.Serialization.XmlElementAttribute("barrierCap", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap : Trigger level approached from beneath")]
        public TriggerEvent barrierCap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierFloorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("barrierFloor", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor : Trigger level approached from above")]
        public TriggerEvent barrierFloor;
        #endregion Members
    }
    #endregion Barrier

    #region CalendarSpread
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CalendarSpread : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("expirationDateTwo", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date Two", IsVisible = true)]
        public AdjustableOrRelativeDate expirationDateTwo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date Two")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion CalendarSpread
    #region ClassifiedPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ClassifiedPayment : SimplePayment
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentType", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Payment types")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment type", IsClonable = true, IsChild = true)]
        public PaymentType[] paymentType;
        #endregion Members
    }
    #endregion ClassifiedPayment
    #region Composite
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Composite : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool determinationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Determination Method", Width = 500)]
        public EFS_MultiLineString determinationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Relative Date")]
        public RelativeDateOffset relativeDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxSpotRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxSpotRateSource", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Determination")]
        public FxSpotRateSource fxSpotRateSource;
        #endregion Members
    }
    #endregion Composite
    #region CreditEventNotice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CreditEventNotice : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("notifyingParty", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notifying party", IsVisible = false)]
        public NotifyingParty notifyingParty;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCenterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notifying party")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business center")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BusinessCenter businessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publiclyAvailableInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("publiclyAvailableInformation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publicly available information")]
        public PubliclyAvailableInformation publiclyAvailableInformation;
        #endregion Members
    }
    #endregion CreditEventNotice
    #region CreditEvents
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CreditEvents : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool bankruptcySpecified;
        [System.Xml.Serialization.XmlElementAttribute("bankruptcy", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Bankruptcy")]
        public Empty bankruptcy;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToPaySpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToPay", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Failure to pay")]
        public FailureToPay failureToPay;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToPayPrincipalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToPayPrincipal", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Failure to pay principal")]
        public Empty failureToPayPrincipal;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToPayInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToPayInterest", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Failure to pay interest")]
        public Empty failureToPayInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obligationDefaultSpecified;
        [System.Xml.Serialization.XmlElementAttribute("obligationDefault", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Obligation default")]
        public Empty obligationDefault;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obligationAccelerationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("obligationAcceleration", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Obligation acceleration")]
        public Empty obligationAcceleration;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repudiationMoratoriumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("repudiationMoratorium", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Repudiation moratorium")]
        public Empty repudiationMoratorium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool restructuringSpecified;
        [System.Xml.Serialization.XmlElementAttribute("restructuring", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Restructuring")]
        public Restructuring restructuring;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool distressedRatingsDowngradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("distressedRatingsDowngrade", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Distressed ratings downgrade")]
        public Empty distressedRatingsDowngrade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturityExtensionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturityExtension", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Maturity extension")]
        public Empty maturityExtension;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool writedownSpecified;
        [System.Xml.Serialization.XmlElementAttribute("writedown", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Write down")]
        public Empty writedown;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool defaultRequirementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("defaultRequirement", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Default requirement")]
        public Money defaultRequirement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditEventNoticeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEventNotice", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit event notice")]
        public CreditEventNotice creditEventNotice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion CreditEvents
    #region CreditEventsReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CreditEventsReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion CreditEventsReference

    #region EquityAveragingPeriod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class EquityAveragingPeriod : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("schedule", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging schedule")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public AveragingSchedule[] schedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingDateTimesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingDateTimes", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Dates")]
        public DateTimeList averagingDateTimes;
        [System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Market Disruption", Width = 200)]
        public MarketDisruption marketDisruption;
        #endregion Members
    }
    #endregion EquityAveragingPeriod

    #region FailureToPay
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FailureToPay : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool gracePeriodExtensionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("gracePeriodExtension", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Grace period extension")]
        public GracePeriodExtension gracePeriodExtension;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentRequirementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentRequirement", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment requirement")]
        public Money paymentRequirement;
        #endregion Members
    }
    #endregion FailureToPay
    #region FeaturePayment
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FeaturePayment : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment")]
		public EFS_RadioChoice pmt;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool pmtLevelPercentageSpecified;
		[System.Xml.Serialization.XmlElementAttribute("levelPercentage", typeof(EFS_Decimal), Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "value")]
		public EFS_Decimal pmtLevelPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool pmtAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_Decimal), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
		public EFS_Decimal pmtAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("time", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment time")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public TimeTypeEnum time;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 70)]
        public Currency currency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePaymentDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Date")]
        public AdjustableOrRelativeDate featurePaymentDate;
        #endregion Members

		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentScheduledlevelPercentageSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentScheduledlevelPercentage", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Scheduled level percentage")]
		public Schedule paymentScheduledlevelPercentage;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentAmountFormulaSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmountFormula", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount formula")]
		public FeatureFormulaEnum paymentAmountFormula;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountToCaptureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmountToCapture", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Amount to capture")]
        public Empty paymentAmountToCapture;

		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public string type = "efs:FeaturePayment";
		#endregion Members

    }
    #endregion FeaturePayment
    #region FrequencyType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FrequencyType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string frequencyTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "token")]
        public string Value;
        #endregion Members
    }
    #endregion FrequencyType
    #region FxFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class FxFeature : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("referenceCurrency", Order = 1)]
        [ControlGUI(Name = "Currency", Width = 75)]
        public IdentifiedCurrency referenceCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice fxFeature;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureQuantoSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quanto", typeof(Quanto),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Quanto", IsVisible = true)]
        public Quanto fxFeatureQuanto;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureCompositeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("composite", typeof(Composite),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Composite", IsVisible = true)]
        public Composite fxFeatureComposite;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureCrossCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("crossCurrency", typeof(Composite),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Composite", IsVisible = true)]
        public Composite fxFeatureCrossCurrency;
        #endregion Members
    }
    #endregion FxFeature

    #region GracePeriodExtension
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class GracePeriodExtension : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("gracePeriod", Order = 1)]
        public Offset gracePeriod;
        #endregion Members
    }
    #endregion GracePeriodExtension

    #region Knock
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Knock : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockInSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockIn",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock In")]
        public TriggerEvent knockIn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockOutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knockOut",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock Out")]
        public TriggerEvent knockOut;
        #endregion Members
    }
    #endregion Knock

    #region MarketDisruption
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class MarketDisruption : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string marketDisruptionScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion MarketDisruption

    #region NotifyingParty
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class NotifyingParty : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 1)]
        [ControlGUI(Name = "Buyer")]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seller")]
        public PartyReference sellerPartyReference;
        #endregion Members
    }
    #endregion NotifyingParty

    #region OptionBase
    // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapOption removed
    // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OptionBaseExtended))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BondOption.BondOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditDefaultSwapOption))]
    public abstract partial class OptionBase : Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference",Order=1)]
        [ControlGUI(Name = "Buyer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference buyerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order=2)]
        [ControlGUI(Name = "Seller")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrTradeSideReference sellerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Option type")]
        public OptionTypeEnum optionType;
        #endregion Members
    }
    #endregion OptionBase
    #region OptionBaseExtended
    // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
    // EG 20140702 New build FpML4.4 VarianceSwapOption removed
    // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
    // EG 20140702 New build FpML4.4 SettlementType is optional
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BondOption.BondOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditDefaultSwapOption))]
    public abstract partial class OptionBaseExtended : OptionBase
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("premium", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium")]
        public Premium premium;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice optionExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionExerciseAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("americanExercise", typeof(AmericanExercise),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American Exercise", IsVisible = true)]
        public AmericanExercise optionExerciseAmerican;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionExerciseBermudaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bermudaExercise", typeof(BermudaExercise),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Bermuda Exercise", IsVisible = true)]
        public BermudaExercise optionExerciseBermuda;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionExerciseEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("europeanExercise", typeof(EuropeanExercise),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European Exercise", IsVisible = true)]
        public EuropeanExercise optionExerciseEuropean;

        [System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise Procedure", IsVisible = true)]
        public ExerciseProcedure procedure;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("feature", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Exercise Procedure")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Feature")]
        public OptionFeature feature;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Notional")]
        public EFS_RadioChoice optionNotional;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionNotionalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalAmount", typeof(Money),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        public Money optionNotionalAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionNotionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", typeof(NotionalAmountReference),Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount reference", IsVisible = true)]
        public NotionalAmountReference optionNotionalReference;
		[System.Xml.Serialization.XmlElementAttribute("optionEntitlement", Order = 9)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option denomination components", IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nb of units of underlyer per option")]
        public EFS_Decimal optionEntitlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entitlementCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("entitlementCurrency", Order = 10)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency entitlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfOptionsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("numberOfOptions", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of options")]
        public EFS_Decimal numberOfOptions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementType", Order = 12)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option denomination components")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option settlement elements", IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Settlement type")]
        public SettlementTypeEnum settlementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementDate", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement date")]
        public AdjustableOrRelativeDate settlementDate;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Choice : ")]
        public EFS_RadioChoice optionSettlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionSettlementNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty optionSettlementNone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionSettlementAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementAmount", typeof(Money),Order=14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount", IsVisible = true)]
        public Money optionSettlementAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionSettlementCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementCurrency", typeof(Currency),Order=15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency", IsVisible = true)]
        public Currency optionSettlementCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Option settlement elements")]
        public bool FillBalise;
        #endregion Members
    }
    #endregion OptionBaseExtended
    #region OptionFeature
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class OptionFeature
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxFeature", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quanto or composite Fx feature")]
        public FxFeature fxFeature;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strategyFeature",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strategy feature")]
        public StrategyFeature strategyFeature;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool asianSpecified;
		[System.Xml.Serialization.XmlElementAttribute("asian", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asian feature")]
        public Asian asian;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("barrier", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier feature")]
        public Barrier barrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockSpecified;
		[System.Xml.Serialization.XmlElementAttribute("knock", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock feature")]
        public Knock knock;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool passThroughSpecified;
		[System.Xml.Serialization.XmlElementAttribute("passThrough", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pass through payment")]
        public PassThrough passThrough;
        #endregion Members
    }
    #endregion OptionFeature
    #region OptionNumericStrike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(OptionStrike))]
    public class OptionNumericStrike
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise")]
        public EFS_RadioChoice optionStrike;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionStrikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Price", IsVisible = true)]
        public EFS_Decimal optionStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionStrikePercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePercentage", typeof(EFS_Decimal),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Price", IsVisible = true)]
        public EFS_Decimal optionStrikePercentage;
        #endregion Members
        #region Constructors
        public OptionNumericStrike()
        {
            optionStrikePrice = new EFS_Decimal();
            optionStrikePercentage = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion OptionNumericStrike
    #region OptionStrike
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class OptionStrike : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike")]
        public EFS_RadioChoice typeStrike;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikePriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePrice", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal typeStrikePrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeStrikePercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikePercentage", typeof(EFS_Decimal),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal typeStrikePercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        #endregion Members
    }
    #endregion OptionStrike

    #region PassThrough
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PassThrough : ItemGUI 
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("passThroughItem", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pass through payment", IsClonable = true)]
        public PassThroughItem[] passThroughItem;
        #endregion Members
    }
    #endregion PassThrough
    #region PassThroughItem
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PassThroughItem : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("payerPartyReference",Order=1)]
        [ControlGUI(Name = "Payer")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference",Order=2)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
		public PartyOrAccountReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("underlyerReference", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer", IsVisible = true)]
        public AssetReference underlyerReference;
        [System.Xml.Serialization.XmlElementAttribute("passThroughPercentage", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        [ControlGUI(Name = "Percentage")]
        public EFS_Decimal passThroughPercentage;
        #endregion Members
    }
    #endregion PassThroughItem
    #region Premium
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Premium : SimplePayment
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premiumType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Forward start premium type")]
        public PremiumTypeEnum premiumType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pricePerOptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("pricePerOption", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amount expressed as function of the number of options")]
        public Money pricePerOption;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool percentageOfNotionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("percentageOfNotional", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "'The amount expressed as a percentage of the notional value of the transaction")]
        public EFS_Decimal percentageOfNotional;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("discountFactor", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "'Discount factor")]
        public EFS_Decimal discountFactor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool presentValueAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("presentValueAmount", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "'Present value of the forecast payment")]
        public Money presentValueAmount;
        #endregion Members
    }
    #endregion Premium

    #region PubliclyAvailableInformation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PubliclyAvailableInformation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool standardPublicSourcesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("standardPublicSources", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Standard public Sources")]
        public Empty standardPublicSources;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publicSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("publicSource",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Public source")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Public source", IsClonable = true, IsChild = false)]
        public EFS_StringArray[] publicSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specifiedNumberSpecified;
        [System.Xml.Serialization.XmlElementAttribute("specifiedNumber", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specified number")]
        public EFS_PosInteger specifiedNumber;
        #endregion Members
    }
    #endregion PubliclyAvailableInformation

    #region Quanto
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Quanto : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("fxRate",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Rate", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true)]
        public FxRate[] fxRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxSpotRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxSpotRateSource", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Determination")]
        public FxSpotRateSource fxSpotRateSource;
        #endregion Members
    }
    #endregion Quanto

    #region Restructuring
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Restructuring : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool restructuringTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("RestructuringType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public RestructuringType restructuringType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleHolderObligationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleHolderObligation", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Multiple holder obligation")]
        public Empty multipleHolderObligation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleCreditEventNoticesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleCreditEventNotices", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Multiple credit event notices")]
        public Empty multipleCreditEventNotices;
        #endregion Members
    }
    #endregion Restructuring
    #region RestructuringType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class RestructuringType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string restructuringScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
    }
    #endregion RestructuringType

    #region StrategyFeature
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StrategyFeature : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strategy features")]
        public EFS_RadioChoice strategyFeature;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureCalendarSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calendarSpread", typeof(CalendarSpread),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CalendarSpread strategyFeatureCalendarSpread;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strategyFeatureStrikeSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("strikeSpread", typeof(StrikeSpread),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public StrikeSpread strategyFeatureStrikeSpread;
        #endregion Members
    }
    #endregion StrategyFeature
    #region StrikeSpread
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class StrikeSpread : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("upperStrike", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Upper strike", IsVisible = false)]
        public OptionStrike upperStrike;
        [System.Xml.Serialization.XmlElementAttribute("upperStrikeNumberOfOptions", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Upper strike")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Number of options")]
        public EFS_Decimal upperStrikeNumberOfOptions;
        #endregion Members
    }
    #endregion StrikeSpread

    #region Trigger
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Trigger : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        public EFS_RadioChoice typeLevel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeLevelLevelSpecified;
        [System.Xml.Serialization.XmlElementAttribute("level", typeof(EFS_Decimal),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 100)]
        public EFS_Decimal typeLevelLevel;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeLevelLevelPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("levelPercentage", typeof(EFS_Decimal),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		[ControlGUI(Name = "value", Width = 100)]
        public EFS_Decimal typeLevelLevelPercentage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeLevelCreditEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEvents", typeof(CreditEvents),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CreditEvents typeLevelCreditEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeLevelCreditEventsReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEventsReference", typeof(CreditEventsReference),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CreditEventsReference typeLevelCreditEventsReference;
        #endregion Members
    }
    #endregion Trigger
    #region TriggerEvent
    public partial class TriggerEvent : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("schedule",Order=1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public AveragingSchedule[] schedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("triggerDates",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dates")]
        public DateList triggerDates;
		[System.Xml.Serialization.XmlElementAttribute("trigger",Order=3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePayment",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Feature payment")]
        public FeaturePayment featurePayment;
		#endregion Members
    }
    #endregion TriggerEvent
}
