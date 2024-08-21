#region Using Directives
using System;
using System.Reflection;
using System.Xml.Serialization;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Enum;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Asset;
using FpML.v42.Shared;
#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision
namespace FpML.v42.EqShared
{
    #region AdditionalDisruptionEvents
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining ISDA 2002 Equity Derivative Additional Disruption Events.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>changeInLaw (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>failureToDeliver (zero or one occurrence; of the type xsd:boolean)
    /// Where the underlying is shares and the transaction is physically settled, then, if true, a failure to deliver 
    /// the shares on the settlement date will not be an event of default for the purposes of the master agreement</newpara>
    /// <newpara>insolvencyFiling (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>hedgingDisruption (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>lossOfStockBorrow (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>increasedCostOfStockBorrow (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>increasedCostOfHedging (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>determiningPartyReference (exactly one occurrence; of the type PartyReference)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ExtraordinaryEvents</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AdditionalDisruptionEvents : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("changeInLaw", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "changeInLaw")]
        public EFS_Boolean changeInLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool failureToDeliverSpecified;
		[System.Xml.Serialization.XmlElementAttribute("failureToDeliver", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "failureToDeliver")]
        public EFS_Boolean failureToDeliver;
		[System.Xml.Serialization.XmlElementAttribute("insolvencyFiling", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others flags", IsVisible = false, IsGroup = true)]
        [ControlGUI(Name = "insolvencyFiling", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean insolvencyFiling;
		[System.Xml.Serialization.XmlElementAttribute("hedgingDisruption", Order = 4)]
        [ControlGUI(Name = "hedgingDisruption", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean hedgingDisruption;
		[System.Xml.Serialization.XmlElementAttribute("lossOfStockBorrow", Order = 5)]
        [ControlGUI(Name = "lossOfStockBorrow", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean lossOfStockBorrow;
		[System.Xml.Serialization.XmlElementAttribute("increasedCostOfStockBorrow", Order = 6)]
        [ControlGUI(Name = "increasedCostOfStockBorrow", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean increasedCostOfStockBorrow;
		[System.Xml.Serialization.XmlElementAttribute("increasedCostOfHedging", Order = 7)]
        [ControlGUI(Name = "increasedCostOfHedging", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean increasedCostOfHedging;
		[System.Xml.Serialization.XmlElementAttribute("determiningPartyReference", Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others flags")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Determining Party")]
        public PartyReference determiningPartyReference;
		#endregion Members
	}
    #endregion AdditionalDisruptionEvents
    #region Asian
    /// <summary>
    /// <newpara><b>Description :</b> As per ISDA 2002 Definitions</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>averagingInOut (exactly one occurrence; of the type AveragingInOutEnum)</newpara>
    /// <newpara>strikeFactor (zero or one occurrence; of the type xsd:decimal) The factor of strike.</newpara>
    /// <newpara>averagingPeriodIn (zero or one occurrence; of the type EquityAveragingPeriod) The averaging in period.</newpara>
    /// <newpara>averagingPeriodOut (zero or one occurrence; of the type EquityAveragingPeriod) The averaging out period.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityFeatures</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Asian : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("averagingInOut", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "averaging", Width = 100)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingInOutEnum averagingInOut;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool strikeFactorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("strikeFactor", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Strike factor")]
        public EFS_Decimal strikeFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodInSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodIn", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging In period")]
        public EquityAveragingPeriod averagingPeriodIn;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingPeriodOutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingPeriodOut", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Out period")]
        public EquityAveragingPeriod averagingPeriodOut;
		#endregion Members
		#region Constructors
		public Asian()
        {
            strikeFactor = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion Asian

    #region Barrier
    /// <summary>
    /// <newpara><b>Description :</b> As per ISDA 2002 Definitions.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>barrierCap (zero or one occurrence; of the type TriggerEvent) A trigger level approached from beneath.</newpara>
    /// <newpara>barrierFloor (zero or one occurrence; of the type TriggerEvent) A trigger level approached from above.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityFeatures</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Barrier : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierCapSpecified;
		[System.Xml.Serialization.XmlElementAttribute("barrierCap", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger level approached from beneath")]
        public TriggerEvent barrierCap;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierFloorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("barrierFloor", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger level approached from above")]
        public TriggerEvent barrierFloor;
		#endregion Members
    }
    #endregion Barrier

    #region Composite
    /// <summary>
    /// <newpara><b>Description :</b> Specifies the conditions to be applied for converting into a reference currency 
    /// when the actual currency rate is not determined upfront.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>determinationMethod (zero or one occurrence; of the type xsd:normalizedString)
    /// Specifies the method according to which an amount or a date is determined.</newpara>
    /// <newpara>relativeDate (zero or one occurrence; of the type RelativeDateOffset)
    /// A date specified as some offset to another date (the anchor date).</newpara>
    /// <newpara>fxSpotRateSource (zero or one occurrence; of the type FxSpotRateSource)
    /// Specifies the methodology (reference source and, optionally, fixing time) to be used for determining 
    /// a currency conversion rate.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FxFeature</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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

    #region Equity
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining an equity underlier.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>description (exactly one occurrence; of the type xsd:string) The long name of a security.</newpara>
    /// <newpara>instrumentId (one or more occurrences; with locally defined content)</newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) The currency in which an amount is denominated.</newpara>
    /// <newpara>exchangeId (zero or more occurrences; of the type ExchangeId)</newpara>
    /// <newpara>relatedExchangeId (zero or more occurrences; of the type ExchangeId)</newpara>
    /// <newpara>clearanceSystem (zero or one occurrence; of the type ClearanceSystem)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Equity : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("description", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description", Width = 600)]
        public EFS_String description;
        [System.Xml.Serialization.XmlElementAttribute("instrumentId",Order=2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Id", IsClonable = true, IsMaster = true)]
        public InstrumentId[] instrumentId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeId",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Id", IsClonable = true)]
        public ExchangeId[] exchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id", IsClonable = true)]
        public ExchangeId[] relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clearanceSystemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("clearanceSystem", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearance System")]
        public ClearanceSystem clearanceSystem;
		#endregion Members
	}
    #endregion Equity
    #region EquityAveragingPeriod
    /// <summary>
    /// <newpara><b>Description :</b> Period over which an average value is taken</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>schedule (zero or more occurrences; of the type EquitySchedule) A Equity Derivative schedule.</newpara>
    /// <newpara>averagingDateTimes (zero or one occurrence; of the type DateTimeList) Averaging DateTimes</newpara>
    /// <newpara>marketDisruption (exactly one occurrence; with locally defined content) 
    /// The market disruption event as defined by ISDA 2002 Definitions</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Asian</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityAveragingPeriod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("schedule",Order=1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Equity derivatives schedule")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public EquitySchedule[] schedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingDateTimesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averagingDateTimes",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Averaging Dates")]
        public DateTimeList averagingDateTimes;
		[System.Xml.Serialization.XmlElementAttribute("marketDisruption", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Market Disruption", Width = 200)]
        public MarketDisruption marketDisruption;
		#endregion Members
	}
    #endregion EquityAveragingPeriod
    #region EquityCorporateEvents
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining the merger events and their treatment</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>shareForShare (exactly one occurrence; of the type ShareExtraordinaryEventEnum) 
    /// The consideration paid for the original shares following the Merger Event consists wholly of new shares</newpara>
    /// <newpara>shareForOther (exactly one occurrence; of the type ShareExtraordinaryEventEnum)
    /// The consideration paid for the original shares following the Merger Event consists wholly of 
    /// cash/securities other than new shares</newpara>
    /// <newpara>shareForCombined (exactly one occurrence; of the type ShareExtraordinaryEventEnum) 
    /// The consideration paid for the original shares following the Merger Event consists of both cash/securities and new shares</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ExtraordinaryEvents</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityCorporateEvents : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("shareForShare", Order = 1)]
		[ControlGUI(Name = "Share for share", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForShare;
		[System.Xml.Serialization.XmlElementAttribute("shareForOther", Order = 2)]
        [ControlGUI(Name = "Share for other", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForOther;
		[System.Xml.Serialization.XmlElementAttribute("shareForCombined", Order = 3)]
        [ControlGUI(Name = "Share for combined", LblWidth = 150, Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ShareExtraordinaryEventEnum shareForCombined;
		#endregion
	}
    #endregion EquityCorporateEvents
    #region EquityFeatures
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining option features.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>asian (zero or one occurrence; of the type Asian) An option where and average price is taken on valuation.</newpara>
    /// <newpara>barrier (zero or one occurrence; of the type Barrier) An option with a barrier feature.</newpara>
    /// <newpara>knock (zero or one occurrence; of the type Knock) A knock feature.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityDerivativeLongFormBase</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquityFeatures : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool asianSpecified;
        [System.Xml.Serialization.XmlElementAttribute("asian",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asian")]
        public Asian asian;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool barrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("barrier",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier")]
        public Barrier barrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool knockSpecified;
        [System.Xml.Serialization.XmlElementAttribute("knock",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Knock")]
        public Knock knock;
		#endregion Members
	}
    #endregion EquityFeatures
    #region EquitySchedule
    /// <summary>
    /// <newpara><b>Description :</b> Method of generating a series of dates.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>startDate (exactly one occurrence; of the type xsd:date) The averaging period start date.</newpara>
    /// <newpara>endDate (exactly one occurrence; of the type xsd:date) The averaging period end date.</newpara>
    /// <newpara>frequency (exactly one occurrence; of the type xsd:decimal) The schedule frequency.</newpara>
    /// <newpara>frequencyType (exactly one occurrence; of the type FrequencyTypeEnum) The schedule frequency type</newpara>
    /// <newpara>weekNumber (zero or one occurrence; of the type xsd:decimal) The schedule week number.</newpara>
    /// <newpara>dayOfWeek (zero or one occurrence; of the type WeeklyRollConventionEnum)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: EquityAveragingPeriod</newpara>
    ///<newpara>• Complex type: TriggerEvent</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EquitySchedule : IEFS_Array
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
        public EFS_Decimal frequency;
		[System.Xml.Serialization.XmlElementAttribute("frequencyType", Order = 4)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency type", Width = 70)]
        public FrequencyTypeEnum frequencyType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weekNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("weekNumber", Order = 5)]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Frequency")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Week number", Width = 75)]
        public EFS_Decimal weekNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayOfWeekSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayOfWeek", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Day of week", Width = 100)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public WeeklyRollConventionEnum dayOfWeek;
		#endregion Members
		#region Constructors
		public EquitySchedule()
        {
            weekNumber = new EFS_Decimal();
        }
        #endregion Constructors

        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion EquitySchedule
    #region EquityValuation
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining how and when an equity option is to be valued.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either </newpara>
    /// <newpara>valuationDate (exactly one occurrence; of the type EquityValuationDate)
    /// The term "Valuation Date" is assumed to have the meaning as defined in the ISDA 2002 Equity Derivatives Definitions</newpara>
    /// <newpara>or</newpara>
    /// <newpara>valuationDates (exactly one occurrence; of the type AdjustableRelativeOrPeriodicDates)
    /// Specifies the interim equity valuation dates of the swap</newpara>
    /// <newpara>valuationTimeType (zero or one occurrence; of the type TimeTypeEnum)
    /// The time of day at which the calculation agent values the underlying, for example the official closing time of the exchange</newpara>
    /// <newpara>valuationTime (zero or one occurrence; of the type BusinessCenterTime) 
    /// The specific time of day at which the calculation agent values the underlying.</newpara>
    /// <newpara>futuresPriceValuation (zero or one occurrence; of the type xsd:boolean)
    /// The official settlement price as announced by the related exchange is applicable, 
    /// in accordance with the ISDA 2002 definitions.</newpara>
    /// <newpara>optionsPriceValuation (zero or one occurrence; of the type xsd:boolean)
    /// The official settlement price as announced by the related exchange is applicable, 
    /// in accordance with the ISDA 2002 definitions.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityExercise</newpara>
    ///<newpara>• EquitySwapValuationPrice</newpara>
    ///<newpara>• VarianceLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquityValuation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Date")]
        public EquityValuationDate valuationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationDates", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation Dates")]
        public AdjustableRelativeOrPeriodicDates valuationDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationTimeType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public TimeTypeEnum valuationTimeType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationTime", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time")]
        public BusinessCenterTime valuationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futuresPriceValuationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("futuresPriceValuation", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "futuresPriceValuation")]
        public EFS_Boolean futuresPriceValuation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsPriceValuationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionsPriceValuation", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "optionsPriceValuation")]
        public EFS_Boolean optionsPriceValuation;
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Others")]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion EquityValuation
    #region EquityValuationDate
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either </newpara>
    /// <newpara>adjustableDate (exactly one occurrence; of the type AdjustableDate)
    /// A date that shall be subject to adjustment if it would otherwise fall on a day that is not 
    /// a business day in the specified business centers, together with the convention for adjusting the date</newpara>
    /// <newpara>or</newpara>
    /// <newpara>relativeDateSequence (exactly one occurrence; of the type RelativeDateSequence) 
    /// A date specified in relation to some other date defined in the document (the anchor date), 
    /// where there is the opportunity to specify a combination of offset rules. 
    /// This component will typically be used for defining the valuation date in relation to the payment date, 
    /// as both the currency and the exchange holiday calendars need to be considered</newpara>
    /// <newpara>Attribute: id (xsd:ID)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityValuation</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class EquityValuationDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice equityValuationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityValuationDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate equityValuationDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool equityValuationDateRelativeDateSequenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDateSequence", typeof(RelativeDateSequence),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date Sequence", IsVisible = true)]
        public RelativeDateSequence equityValuationDateRelativeDateSequence;

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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion EquityValuationDate
    #region ExtraordinaryEvents
    ///<summary>
    /// <newpara><b>Description :</b> Where the underlying is shares, defines market events affecting the issuer of those shares
    /// that may require the terms of the transaction to be adjusted.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>mergerEvents (zero or one occurrence; of the type EquityCorporateEvents)
    /// Occurs when the underlying ceases to exist following a merger between the Issuer and another company.</newpara>
    /// <newpara>tenderOffer (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>tenderOfferEvents (zero or one occurrence; of the type EquityCorporateEvents)</newpara>
    /// <newpara>compositionOfCombinedConsideration (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>indexAdjustmentEvents (zero or one occurrence; of the type IndexAdjustmentEvents)</newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either </newpara>
    /// <newpara>additionalDisruptionEvents (exactly one occurrence; of the type AdditionalDisruptionEvents)</newpara>
    /// <newpara>or</newpara>
    /// <newpara>failureToDeliver (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>representations (zero or one occurrence; of the type Representations)
    /// ISDA 2002 Equity Derivative Representations</newpara>
    /// <newpara>nationalisationOrInsolvency (zero or one occurrence; of the type NationalisationOrInsolvencyOrDelistingEventEnum)
    /// The terms "Nationalisation" and "Insolvency" have the meaning as defined in the ISDA 2002 Equity Derivatives Definitions</newpara>
    /// <newpara>delisting (zero or one occurrence; of the type NationalisationOrInsolvencyOrDelistingEventEnum)
    /// The term "Delisting" has the meaning defined in the ISDA 2002 Equity Derivatives Definitions</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityDerivativeLongFormBase</newpara>
    ///<newpara>• EquitySwap</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ExtraordinaryEvents : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool mergerEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("mergerEvents",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Merger Events")]
        public EquityCorporateEvents mergerEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tenderOfferSpecified;
		[System.Xml.Serialization.XmlElementAttribute("tenderOffer", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "tenderOffer")]
        public EFS_Boolean tenderOffer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool tenderOfferEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tenderOfferEvents",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tender Offer Events")]
        public EquityCorporateEvents tenderOfferEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compositionOfCombinedConsiderationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("compositionOfCombinedConsideration", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "compositionOfCombinedConsideration")]
        public EFS_Boolean compositionOfCombinedConsideration;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAdjustmentEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexAdjustmentEvents",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Adjustment Events")]
        public IndexAdjustmentEvents indexAdjustmentEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemFailureToDeliverSpecified;
        [System.Xml.Serialization.XmlElementAttribute("failureToDeliver", typeof(EFS_Boolean),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "checked if yes")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "failureToDeliver", IsVisible = true)]
        public EFS_Boolean itemFailureToDeliver;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemAdditionalDisruptionEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalDisruptionEvents", typeof(AdditionalDisruptionEvents),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Additional Disruption Events", IsVisible = true)]
        public AdditionalDisruptionEvents itemAdditionalDisruptionEvents;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool representationsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("representations",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Representations")]
        public Representations representations;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nationalisationOrInsolvencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("nationalisationOrInsolvency",Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nationalisation Or Insolvency", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NationalisationOrInsolvencyOrDelistingEventEnum nationalisationOrInsolvency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool delistingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("delisting",Order=10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Delisting", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NationalisationOrInsolvencyOrDelistingEventEnum delisting;
		#endregion Members
	}
    #endregion ExtraordinaryEvents

    #region FeaturePayment
    /// <summary>
    /// <newpara><b>Description :</b> Payment made following trigger occurence.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>payerPartyReference (exactly one occurrence; of the type PartyReference) 
    /// A reference to the party responsible for making the payments defined by this structure.</newpara>
    /// <newpara>receiverPartyReference (exactly one occurrence; of the type PartyReference) 
    /// A reference to the party that receives the payments corresponding to this structure.</newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>levelPercentage (exactly one occurrence; of the type xsd:decimal) The trigger level percentage.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>amount (exactly one occurrence; of the type xsd:decimal) The monetary quantity in currency units.</newpara>
    /// <newpara></newpara>
    /// <newpara>time (zero or one occurrence; of the type TimeTypeEnum) The feature payment time.</newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) The currency in which an amount is denominated.</newpara>
    /// <newpara>featurePaymentDate (zero or one occurrence; of the type AdjustableOrRelativeDate) The feature payment date.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: TriggerEvent</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FeaturePayment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference",Order=1)]
        [ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference",Order=2)]
        [ControlGUI(Name = "Receiver", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference receiverPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment")]
        public EFS_RadioChoice payment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentLevelPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("levelPercentage", typeof(EFS_Decimal),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal paymentLevelPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amount", typeof(EFS_Decimal),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal paymentAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("time", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment time")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public TimeTypeEnum time;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 70)]
        public Currency currency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePaymentDate", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Date")]
        public AdjustableOrRelativeDate featurePaymentDate;
		#endregion Members
		#region Constructors
		public FeaturePayment()
        {
            paymentLevelPercentage = new EFS_Decimal();
            paymentAmount = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion FeaturePayment
    #region FxFeature
    /// <summary>
    /// <newpara><b>Description :</b> A type for defining FX features.</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>referenceCurrency (exactly one occurrence; of the type IdentifiedCurrency)
    /// Specifies the reference currency of the trade</newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>composite (exactly one occurrence; of the type Composite).</newpara>
    /// <newpara>or</newpara>
    /// <newpara>quanto (exactly one occurrence; of the type Quanto)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityDerivativeBase</newpara>
    ///<newpara>• EquityLeg</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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
		#endregion Members
	}
    #endregion FxFeature

    #region IndexAdjustmentEvents
    /// <summary>
    /// <newpara><b>Description :</b></newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>indexModification (exactly one occurrence; of the type IndexEventConsequenceEnum)</newpara>
    /// <newpara>indexCancellation (exactly one occurrence; of the type IndexEventConsequenceEnum)</newpara>
    /// <newpara>indexDisruption (exactly one occurrence; of the type IndexEventConsequenceEnum)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ExtraordinaryEvents</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndexAdjustmentEvents : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("indexModification", Order = 1)]
		[ControlGUI(Name = "Modification", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexModification;
		[System.Xml.Serialization.XmlElementAttribute("indexCancellation", Order = 2)]
		[ControlGUI(Name = "Cancellation", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexCancellation;
		[System.Xml.Serialization.XmlElementAttribute("indexDisruption", Order = 3)]
		[ControlGUI(Name = "Disruption", LblWidth = 100, Width = 220, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public IndexEventConsequenceEnum indexDisruption;
		#endregion Members
	}
    #endregion IndexAdjustmentEvents

    #region Knock
    /// <summary>
    /// <newpara><b>Description :</b> Knock In means option to exercise comes into existence. 
    /// Knock Out means option to exercise goes out of existence</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>knockIn (zero or one occurrence; of the type TriggerEvent) The knock in.</newpara>
    /// <newpara>knockOut (zero or one occurrence; of the type TriggerEvent) The knock out.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityFeatures</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Knock : ItemGUI
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
    /// <summary>
    /// <newpara><b>Description :</b></newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: marketDisruptionScheme (xsd:anyURI)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• EquityAveragingPeriod</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MarketDisruption : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string marketDisruptionScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
		#region Constructors
		public MarketDisruption()
        {
            marketDisruptionScheme = "http://www.fpml.org/spec/2003/marketDisruption-1-0";
		}
		#endregion Constructors
	}
    #endregion MarketDisruption

    #region Quanto
    /// <summary>
    /// <newpara><b>Description :</b>Determines the currency rate that the seller of the equity amounts will 
    /// apply at each valuation date for converting the respective amounts into a currency that is different 
    /// from the currency denomination of the underlyer</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>fxRate (one or more occurrences; of the type FxRate) 
    /// Specifies a currency conversion rate.</newpara>
    /// <newpara>fxSpotRateSource (zero or one occurrence; of the type FxSpotRateSource)
    /// Specifies the methodology (reference source and, optionally, fixing time) to be used for determining 
    /// a currency conversion rate.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FxFeature</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
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

    #region Representations
    /// <summary>
    /// <newpara><b>Description :</b>A type for defining ISDA 2002 Equity Derivative Representations</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>nonReliance (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>agreementsRegardingHedging (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>indexDisclaimer (zero or one occurrence; of the type xsd:boolean)</newpara>
    /// <newpara>additionalAcknowledgements (exactly one occurrence; of the type xsd:boolean)</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ExtraordinaryEvents</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Representations : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("nonReliance", Order = 1)]
		[ControlGUI(Name = "non Reliance", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean nonReliance;
		[System.Xml.Serialization.XmlElementAttribute("agreementsRegardingHedging", Order = 2)]
        [ControlGUI(Name = "agreements Regarding Hedging", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean agreementsRegardingHedging;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexDisclaimerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexDisclaimer", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "index Disclaimer")]
        public EFS_Boolean indexDisclaimer;
		[System.Xml.Serialization.XmlElementAttribute("additionalAcknowledgements", Order = 4)]
        [ControlGUI(Name = "additional Acknowledgements", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Boolean additionalAcknowledgements;
		#endregion Members
	}
    #endregion Representations

    #region Trigger
    /// <summary>
    /// <newpara><b>Description :</b> Trigger point at which feature is effective</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>level (exactly one occurrence; of the type xsd:decimal) The trigger level.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>levelPercentage (exactly one occurrence; of the type xsd:decimal) The trigger level percentage.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• TriggerEvent</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Trigger : ItemGUI
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
		#endregion Members
		#region Constructors
		public Trigger()
        {
            typeLevelLevel = new EFS_Decimal();
            typeLevelLevelPercentage = new EFS_Decimal();
		}
		#endregion Constructors
	}
    #endregion Trigger
    #region TriggerEvent
    /// <summary>
    /// <newpara><b>Description :</b> Observation point for trigger</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>schedule (zero or more occurrences; of the type EquitySchedule) A Equity Derivative schedule.</newpara>
    /// <newpara>triggerDates (zero or one occurrence; of the type DateList) The trigger Dates</newpara>
    /// <newpara>trigger (exactly one occurrence; of the type Trigger) The trigger level.</newpara>
    /// <newpara>featurePayment (zero or one occurrence; of the type FeaturePayment) The feature payment.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Barrier</newpara>
    ///<newpara>• Knock</newpara>
    ///<newpara><b>Derived by :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TriggerEvent : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("schedule",Order=1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Schedule", IsClonable = true, IsChild = true)]
        public EquitySchedule[] schedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerDates",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dates")]
        public DateList triggerDates;
		[System.Xml.Serialization.XmlElementAttribute("trigger", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger", IsVisible = false)]
        public Trigger trigger;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool featurePaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("featurePayment", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Trigger")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Feature payment")]
        public FeaturePayment featurePayment;
		#endregion Members
	}
    #endregion TriggerEvent
}