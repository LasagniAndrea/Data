#region Using Directives
using System;
using System.Reflection;
using System.Collections;

using EFS.ACommon;
using EFS.Actor;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;


using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;
using EfsML.Settlement;

using EfsML.v20;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Eqd;
using FpML.v42.Eqs;
using FpML.v42.Fx;
using FpML.v42.Ird;
using FpML.v42.Cd;
#endregion Using Directives
#region Revision
/// <revision>
///     <version>1.2.0</version><date>20071003</date><author>EG</author>
///     <comment>
///     Ticket 15800 : Add two field pGrandParent and pFldGrandParent for all method DisplayArray (used to determine REGEX type for derived classes
///     </comment>
/// </revision>
#endregion Revision

namespace FpML.v42.Shared
{
    #region Address
    /// <summary>
    /// <para><b>Description :</b> A type that represents a physical postal address.</para>
    /// <para><b>Contents :</b></para>
    /// <para>streetAddress (zero or one occurrence; of the type StreetAddress) The set of street and building number
    /// information that identifies a postal address within a city.</para>
    /// <para>city (zero or one occurrence; of the type xsd:string) The city component of a postal address.</para>
    /// <para>country (zero or one occurrence; of the type Country) The ISO 3166 standard code for the country within
    /// which the postal address is located.</para>
    /// <para>postalCode (zero or one occurrence; of the type xsd:string) The code, required for computerised mail sorting
    /// systems, that is allocated to a physical address by a national postal authority.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RoutingExplicitDetails</para>
    ///<para>• Complex type: RoutingIdsAndExplicitDetails</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Address : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("streetAddress", Order = 1)]
		[ControlGUI(Name = "Address", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
		public StreetAddress streetAddress;
		/*
		[System.Xml.Serialization.XmlArrayItemAttribute("streetLine", IsNullable = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Street line", IsClonable = true, IsMaster = true, IsChild = false)]
        public EFS_StringArray[] streetAddress;
		*/
		[System.Xml.Serialization.XmlElementAttribute("city", Order = 2)]
		[ControlGUI(Name = "City", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
        public EFS_String city;
		[System.Xml.Serialization.XmlElementAttribute("state", Order = 3)]
        [ControlGUI(Name = "State", LblWidth = 60, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 375)]
        public EFS_String state;
		[System.Xml.Serialization.XmlElementAttribute("country", Order = 4)]
        [ControlGUI(Name = "Country", LblWidth = 60, Width = 350)]
        public Country country;
		[System.Xml.Serialization.XmlElementAttribute("postalCode", Order = 5)]
        [ControlGUI(Name = "Postal code", Width = 100)]
        public EFS_String postalCode;
		#endregion Members
	}
    #endregion Address
    #region AdjustableDate
    /// <summary>
    /// <para><b>Description :</b> A type for defining a date that shall be subject to adjustment if it would otherwise fall 
    /// on a day that is not a business day in the specified business centers, together 
    /// with the convention for adjusting the date.</para>
    /// <para><b>Contents :</b></para>
    /// <para>unadjustedDate (exactly one occurrence; with locally defined content) A date subject to adjustment.</para>
    /// <para>dateAdjustments (exactly one occurrence; of the type BusinessDayAdjustments) The business day
    /// convention and financial business centers used for adjusting the date if it would otherwise fall on a day that is
    /// not a business dat in the specified business centers.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableOrRelativeDate</para>
    ///<para>• Complex type: CalculationPeriodDates</para>
    ///<para>• Complex type: DividendPaymentDate</para>
    ///<para>• Complex type: EquityPremium</para>
    ///<para>• Complex type: EquitySwapAdditionalPayment</para>
    ///<para>• Complex type: EquitySwapEarlyTerminationType</para>
    ///<para>• Complex type: EquitySwapValuation</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: MandatoryEarlyTermination</para>
    ///<para>• Complex type: Payment</para>
    ///<para>• Complex type: QuotablePayment</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdjustableDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedDate", Order = 1)]
		[ControlGUI(Name = "value")]
        public IdentifierDate unadjustedDate;
		[System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Date Adjustments", IsVisible = false, IsCopyPaste = true)]
        public BusinessDayAdjustments dateAdjustments;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion AdjustableDate
    #region AdjustableDate2
    /// <summary>
    /// <para><b>Description :</b> A type that is different from AdjustableDate in two regards. First, 
    /// date adjustments can be specified with either a dateAdjustments element or a reference to an 
    /// existing dateAdjustments element. Second, it does not require the specification of date adjustments.</para>
    /// <para><b>Contents :</b></para>
    /// <para>unadjustedDate (exactly one occurrence; with locally defined content) A date subject to adjustment.</para>
    /// <para>Either</para>
    /// <para>dateAdjustments (exactly one occurrence; of the type BusinessDayAdjustments) The business day
    /// convention and financial business centers used for adjusting the date if it would otherwise fall on a day that is
    /// not a business dat in the specified business centers.</para>
    /// <para>Or</para>
    /// <para>dateAdjustmentsReference (exactly one occurrence; of the type BusinessDayAdjustmentsReference)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: GeneralTerms</para>
    ///<para>• Complex type: ScheduledTerminationDate</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AdjustableDate2 : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedDate", Order = 1)]
        [ControlGUI(Name = "value")]
        public IdentifierDate unadjustedDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Date adjustment")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustments", typeof(BusinessDayAdjustments),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessDayAdjustments itemDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dateAdjustmentsReference", typeof(BusinessDayAdjustmentsReference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessDayAdjustments)]
        public BusinessDayAdjustmentsReference itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
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
        #region Constructors
        public AdjustableDate2()
        {
            unadjustedDate = new IdentifierDate();
            itemDefine = new BusinessDayAdjustments();
            itemReference = new BusinessDayAdjustmentsReference();
        }
        #endregion Constructors
    }
    #endregion AdjustableDate2
    #region AdjustableDates
    /// <summary>
    /// <para><b>Description :</b> A type for defining a series of dates that shall be subject to adjustment 
    /// if they would otherwise fall on a day that is not a business day in the specified business centers, 
    /// together with the convention for adjusting the dates.</para>
    /// <para><b>Contents :</b></para>
    /// <para>unadjustedDate (one or more occurrences; with locally defined content) A date subject to adjustment.</para>
    /// <para>dateAdjustments (exactly one occurrence; of the type BusinessDayAdjustments) The business day
    /// convention and financial business centers used for adjusting the date if it would otherwise fall on a day that is
    /// not a business dat in the specified business centers.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableOrRelativeDates</para>
    ///<para>• Complex type: AdjustableRelativeOrPeriodicDates</para>
    ///<para>• Complex type: CashSettlementPaymentDate</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdjustableDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedDate",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unadjusted Dates", IsMaster = true, IsMasterVisible = true)]
        public unadjustedDate[] unadjustedDate;
		[System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 2)]
        public BusinessDayAdjustments dateAdjustments;
		#endregion Members
	}
    #endregion AdjustableDates
    #region AdjustableOrRelativeDate
    /// <summary>
    /// <para><b>Description :</b> A type giving the choice between defining a date as an explicit date 
    /// together with applicable adjustments or as relative to some other (anchor) date.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>adjustableDate (exactly one occurrence; of the type AdjustableDate) A date that shall be subject to
    /// adjustment if it would otherwise fall on a day that is not a business day in the specified business centers,
    /// together with the convention for adjusting the date.</para>
    /// <para>Or</para>
    /// <para>relativeDate (exactly one occurrence; of the type RelativeDateOffset) A date specified as some offset to
    /// another date (the anchor date).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableRelativeOrPeriodicDates</para>
    ///<para>• Complex type: AmericanExercise</para>
    ///<para>• Complex type: EquityBermudanExercise</para>
    ///<para>• Complex type: EquityEuropeanExercise</para>
    ///<para>• Complex type: EquityExercise</para>
    ///<para>• Complex type: EquityLeg</para>
    ///<para>• Complex type: EquityPaymentDates</para>
    ///<para>• Complex type: EuropeanExercise</para>
    ///<para>• Complex type: FeaturePayment</para>
    ///<para>• Complex type: InterestLeg</para>
    ///<para>• Complex type: PrincipalExchangeFeatures</para>
    ///<para>• Complex type: SharedAmericanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdjustableOrRelativeDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate adjustableOrRelativeDateAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDateRelativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(RelativeDateOffset),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date", IsVisible = true, IsCopyPaste = true)]
        public RelativeDateOffset adjustableOrRelativeDateRelativeDate;

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
    #endregion AdjustableOrRelativeDate
    #region AdjustableOrRelativeDates
    /// <summary>
    /// <para><b>Description :</b> A type giving the choice between defining a series of dates as an explicit list 
    /// of dates together with applicable adjustments or as relative to some other series of (anchor) dates.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>adjustableDates (exactly one occurrence; of the type AdjustableDates) A series of dates that shall be subject
    /// 	/// to adjustment if they would otherwise fall on a day that is not a business day in the specified business centers,
    /// together with the convention for adjusting the date.</para>
    /// <para>Or</para>
    /// <para>relativeDates (exactly one occurrence; of the type RelativeDates) A series of dates specified as some offset
    /// to another series of dates (the anchor dates).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AmericanExercise</para>
    ///<para>• Complex type: BermudaExercise</para>
    ///<para>• Complex type: EquityPaymentDates</para>
    ///<para>• Complex type: EuropeanExercise</para>
    ///<para>• Complex type: InterestLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdjustableOrRelativeDates : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableOrRelativeDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDatesAdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates adjustableOrRelativeDatesAdjustableDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableOrRelativeDatesRelativeDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDates", typeof(RelativeDates),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Dates", IsVisible = true)]
        public RelativeDates adjustableOrRelativeDatesRelativeDates;

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
    #endregion AdjustableOrRelativeDates
    #region AdjustableRelativeOrPeriodicDates
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>adjustableDates (exactly one occurrence; of the type AdjustableDates) A series of dates that shall be subject
    /// to adjustment if they would otherwise fall on a day that is not a business day in the specified business centers,
    /// together with the convention for adjusting the date.</para>
    /// <para>Or</para>
    /// <para>relativeDateSequence (exactly one occurrence; of the type RelativeDateSequence) A series of dates 
    /// specified as some offset to other dates (the anchor dates) which can</para>
    /// <para>Or</para>
    /// <para>periodicDates (exactly one occurrence; of the type PeriodicDates) A type defining the exercise period for an 
    /// American style option together with any rules governing the notional amount of the underlying which can be exercised 
    /// on any given exercise date and any associated exercise fees.</para>
    ///</summary>
    ///<remarks>	
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: EquityValuation</para>
    ///<para>• Complex type: LegAmount</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AdjustableRelativeOrPeriodicDates : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice adjustableRelativeOrPeriodic;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicAdjustableDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDates", typeof(AdjustableDates),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Dates", IsVisible = true)]
        public AdjustableDates adjustableRelativeOrPeriodicAdjustableDates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicRelativeDateSequenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDateSequence", typeof(RelativeDateSequence),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Relative Date Sequence", IsVisible = true)]
        public RelativeDateSequence adjustableRelativeOrPeriodicRelativeDateSequence;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustableRelativeOrPeriodicPeriodicDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("periodicDates", typeof(PeriodicDates),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Periodic Dates", IsVisible = true)]
        public PeriodicDates adjustableRelativeOrPeriodicPeriodicDates;

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
    #endregion AdjustableRelativeOrPeriodicDates
    #region AmericanExercise
    /// <summary>
    /// <para><b>Description :</b> A type defining the exercise period for an American style option together with any rules 
    /// governing the notional amount of the underlying which can be exercised on any given exercise date and 
    /// any associated exercise fees.</para>
    /// <para><b>Contents :</b>Inherited element(s): (This definition inherits the content defined by the type Exercise)</para>
    /// <para><b>• The abstract base class for all types which define way in which options may be exercised.</b></para>
    /// <para>commencementDate (exactly one occurrence; of the type AdjustableOrRelativeDate) The first day of the
    /// exercise period for an American style option.</para>
    /// <para>expirationDate (exactly one occurrence; of the type AdjustableOrRelativeDate) The last day within an
    /// exercise period for an American style option. For a European style option it is the only day within the exercise
    /// period.</para>
    /// <para>relevantUnderlyingDate (zero or one occurrence; of the type AdjustableOrRelativeDates) The daye on the
    /// underlying set by the exercise of an option. What this date is depends on the option (e.g. in a swaption it is the
    /// effective date, in an extendible/cancelable provision it is the termination date).</para>
    /// <para>earliestExerciseTime (exactly one occurrence; of the type BusinessCenterTime) The earliest time at which
    /// notice of exercise can be given by the buyer to the seller (or seller's agent) i) on the expriation date, in the
    /// case of a European style option, (ii) on each bermuda option exercise date and the expiration date, in the case
    /// of a Bermudan style option the commencement date to, and including, the expiration date , in the case of an
    /// American option.</para>
    /// <para>latestExerciseTime (zero or one occurrence; of the type BusinessCenterTime) For a Bermudan or American
    /// style option, the latest time on an exercise business day (excluding the expiration date) within the exercise
    /// period that notice can be given by the buyer to the seller or seller's agent. Notice of exercise given after this
    /// time will be deemed to have been given on the next exercise business day.</para>
    /// <para>expirationTime (exactly one occurrence; of the type BusinessCenterTime) The latest time for exercise on
    /// expirationDate.</para>
    /// <para>multipleExercise (zero or one occurrence; of the type MultipleExercise) As defined in the 2000 ISDA
    /// Definitions, Section 12.4. Multiple Exercise, the buyer of the option has the right to exercise all or less than all
    /// the unexercised notional amount of the underlying swap on one or more days in the exercise period, but on
    /// any such day may not exercise less than the minimum notional amount or more that the maximum notional
    /// amount, and if an integral multiple amount is specified, the notional amount exercised must be equal to, or be
    /// an intergral multiple of, the integral multiple amount.</para>
    /// <para>exerciseFeeSchedule (zero or one occurrence; of the type ExerciseFeeSchedule) The fees associated with
    /// an exercise date. The fees are conditional on the exercise occuring. The fees can be specified as actual
    /// currency amounts or as percentages of the notional amount being exercised.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: americanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("americanExercise", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class AmericanExercise : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("commencementDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commencement Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate commencementDate;
		[System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Commencement Date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date")]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
		[System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Latest Exercise Time")]
        public BusinessCenterTime latestExerciseTime;
		[System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multipleExercise", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Multiple Exercise", IsCopyPaste = true)]
        public MultipleExercise multipleExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeScheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseFeeSchedule", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee Schedule", IsCopyPaste = true)]
        public ExerciseFeeSchedule exerciseFeeSchedule;
		#endregion Members
    }
    #endregion AmericanExercise
    #region AmountSchedule
    /// <summary>
    /// <para><b>Description :</b> A type defining a currency amount or a currency amount schedule.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Schedule)</para>
    /// <para>• A type defining a schedule of rates or amounts in terms of an initial value and then a series of
    /// step date and value pairs. On each step date the rate or amount changes to the new step value.
    /// The series of step date and value pairs are optional. If not specified, this implies that the initial
    /// value remains unchanged over time.</para>
    /// <para>currency (exactly one occurance; of the type Currency) The currency in which an amount is denominated.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("notionalStepSchedule", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class AmountSchedule : Schedule
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Currency", Width = 70, LineFeed = MethodsGUI.LineFeedEnum.BeforeAndAfter)]
        public Currency currency;
		#endregion Members
    }
    #endregion AmountSchedule
    #region ArrayPartyReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ArrayPartyReference
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public EFS_Href efs_href;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
		}
		#endregion Members
    }
    #endregion ArrayPartyReference
    #region AutomaticExercise
    /// <summary>
    /// <para><b>Description :</b> A type to define automatic exercise of a swaption. With automatic exercise the option is deemed to have
    /// exercised if it is in the money by more than the threshold amount on the exercise date.</para>
    /// <para><b>Contents :</b></para>
    /// <para>thresholdRate (exactly one occurance; of the type xsd:decimal) A threshold rate. The threshold of 0.10%
    /// would be represented as 0.001</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class AutomaticExercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("thresholdRate", Order = 1)]
		[ControlGUI(Name = "thresholdRate", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal thresholdRate;
		#endregion Members
	}
    #endregion AutomaticExercise

    #region BermudaExercise
    /// <summary>
    /// <para><b>Description :</b> A type defining the bermudan option exercise dates and the expiration date together with any rules
    /// govenerning the notional amount of the underlying which can be exercised on any given exercise date and any
    /// associated exercise fee.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Exercise)</para>
    /// <para>• The abstract base class for all types which define way in which options may be exercised.</para>
    /// <para>bermudaExerciseDates (exactly one occurance; of the type AdjustableOrRelativeDates) The dates the define
    /// the bermudan option exercise dates and the expiration date. The last specified date is assumed to be the
    /// expiration date. The dates can either be specified as a series of explicit dates and associated adjustments or
    /// as a series of dates defined relative to another schedule of dates, for example, the calculation period start
    /// dates. Where a relative series of dates are defined the first and last possible exercise dates can be separately
    /// specified.</para>
    /// <para>relevantUnderlyingDate (zero or one occurance; of the type AdjustableOrRelativeDates) The daye on the
    /// underlying set by the exercise of an option. What this date is depends on the option (e.g. in a swaption it is the
    /// effective date, in an extendible/cancelable provision it is the termination date).</para>
    /// <para>earliestExerciseTime (exactly one occurance; of the type BusinessCenterTime) The earliest time at which
    /// notice of exercise can be given by the buyer to the seller (or seller's agent) i) on the expriation date, in the
    /// case of a European style option, (ii) on each bermuda option exercise date and the expiration date, in the case
    /// of a Bermudan style option the commencement date to, and including, the expiration date , in the case of an
    /// American option.</para>
    /// <para>latestExerciseTime (zero or one occurance; of the type BusinessCenterTime) For a Bermudan or American
    /// style option, the latest time on an exercise business day (excluding the expiration date) within the exercise
    /// period that notice can be given by the buyer to the seller or seller's agent. Notice of exercise given after this
    /// time will be deemed to have been given on the next exercise business day.</para>
    /// <para>expirationTime (exactly one occurance; of the type BusinessCenterTime) The latest time for exercise 
    /// on expirationDate</para>
    /// <para>multipleExercise (zero or one occurance; of the type MultipleExercise) As defined in the 2000 ISDA
    /// Definitions, Section 12.4. Multiple Exercise, the buyer of the option has the right to exercise all or less than all
    /// the unexercised notional amount of the underlying swap on one or more days in the exercise period, but on
    /// any such day may not exercise less than the minimum notional amount or more that the maximum notional
    /// amount, and if an integral multiple amount is specified, the notional amount exercised must be equal to, or be
    /// an intergral multiple of, the integral multiple amount.</para>
    ///<para>exerciseFeeSchedule (zero or one occurance; of the type ExerciseFeeSchedule) The fees associated with an
    /// exercise date. The fees are conditional on the exercise occuring. The fees can be specified as actual currency
    /// amounts or as percentages of the notional amount being exercised.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• bermudaExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("bermudaExercise", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class BermudaExercise : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("bermudaExerciseDates", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Bermuda Exercise Dates", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDates bermudaExerciseDates;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Bermuda Exercise Dates")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date", IsCopyPaste = true)]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
		[System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Latest Exercise Time")]
        public BusinessCenterTime latestExerciseTime;
		[System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multipleExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("multipleExercise", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Multiple Exercise", IsCopyPaste = true)]
        public MultipleExercise multipleExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeScheduleSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseFeeSchedule", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee Schedule", IsCopyPaste = true)]
        public ExerciseFeeSchedule exerciseFeeSchedule;
		#endregion Members
    }
    #endregion BermudaExercise
    #region BrokerConfirmation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class BrokerConfirmation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("brokerConfirmationType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 350)]
        public BrokerConfirmationType brokerConfirmationType;
		#endregion Members
	}
    #endregion BrokerConfirmation
    #region BrokerConfirmationType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class BrokerConfirmationType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string brokerConfirmationTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
		#endregion Members
        #region Constructors
        public BrokerConfirmationType()
        {
            brokerConfirmationTypeScheme = "http://www.fpml.org/coding-scheme/broker-confirmation-type-2-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            BrokerConfirmationType clone = new BrokerConfirmationType();
            clone.brokerConfirmationTypeScheme = this.brokerConfirmationTypeScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion BrokerConfirmationType
    #region BusinessCenter
    /// <summary>
    /// <para><b>Description :</b></para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    /// <para>• A type defining a schedule of rates or amounts in terms of an initial value and then a series of
    /// step date and value pairs. On each step date the rate or amount changes to the new step value.
    /// The series of step date and value pairs are optional. If not specified, this implies that the initial
    /// value remains unchanged over time.</para>
    /// <para>currency (exactly one occurance; of the type Currency) The currency in which an amount is denominated.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• BusinessCenters</para>
    ///<para>• BusinessCenterTime</para>
    ///<para>• CalculationAgent</para>
    ///<para>• ExerciseNotice</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BusinessCenter : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string businessCenterScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 300)]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessCenter)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion BusinessCenter
    #region BusinessCenters
    /// <summary>
    /// <para><b>Description :</b> A type for defining financial business centers used in determining whether a day is 
    /// a business day or not.</para>
    /// <para><b>Contents :</b></para>
    /// <para>businessCenter (one or more occurances; with locally defined content) A code identifying a financial business
    /// center location. A list of business centers may be ordered in the document alphabetically based on business
    /// center code. An FpML document containing an unordered business center list is still regarded as a conformant
    /// document.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("businessCenters", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class BusinessCenters
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("businessCenter",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Center")]
        public BusinessCenter[] businessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessCenters)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion BusinessCenters
    #region BusinessCentersReference
    /// <summary>
    /// <para><b>Description :</b> A pointer style reference to a set of financial business centers defined elsewhere in the document.
    ///  This set of business centers is used to determine whether a particular day is a business day or not.</para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RelativeDateSequence</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BusinessCentersReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        [ControlGUI(IsLabel = false, Name = "value", Width = 200)]
        public string href;
		#endregion Members
    }
    #endregion BusinessCentersReference
    #region BusinessCenterTime
    /// <summary>
    /// <para><b>Description :</b> A type for defining a time with respect to a business center location. 
    /// For example, 11:00am London time.</para>
    /// <para><b>Contents :</b></para>
    /// <para>hourMinuteTime (exactly one occurance; with locally defined content) A time specified in hh:mm:ss format
    /// where the second component must be '00', e.g. 11am would be represented as 11:00:00.</para>
    /// <para>businessCenter (exactly one occurance; with locally defined content) A code identifying a financial business
    /// center location. A list of business centers may be ordered in the document alphabetically based on business
    /// center code. An FpML document containing an unordered business center list is still regarded as a conformant
    /// document.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BusinessCenterTime : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("hourMinuteTime", Order = 1)]
		[ControlGUI(Name = "Time", Width = 100, IsSpaceBefore = false, Regex = EFSRegex.TypeRegex.RegexLongTime)]
        public HourMinuteTime hourMinuteTime;
		[System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 2)]
        [ControlGUI(Name = "Business center", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public BusinessCenter businessCenter;
		#endregion Members
    }
    #endregion BusinessCenterTime
    #region BusinessDateRange
    /// <summary>
    /// <para><b>Description :</b> A type defining a range of contiguous business days by defining an unadjusted first date, 
    /// an unadjusted last date and a business day convention and business centers for adjusting the first and 
    /// last dates if they would otherwise fall on a non business day in the specified business centers. 
    /// The days between the first and last date must also be good business days in the specified centers 
    /// to be counted in the range.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type DateRange)</para>
    /// <para>• A type defining a contiguous series of calendar dates. The date range is defined as all the dates
    /// between and including the first and the last date. The first date must fall before the last date.</para>
    /// <para>businessDayConvention (exactly one occurance; of the type BusinessDayConventionEnum) The convention
    /// for adjusting a date if it would otherwise fall on a day that is not a business day.</para>
    /// <para>businessDayConvention (exactly one occurance; of the type BusinessDayConventionEnum) The convention
    /// for adjusting a date if it would otherwise fall on a day that is not a business day.</para>
    /// <para>Either</para>
    /// <para>businessCentersReference (exactly one occurance; with locally defined content) A pointer style reference to a
    /// set of financial business centers defined elsewhere in the document. This set of business centers is used to
    /// determine whether a particular day is a business day or not.</para>
    /// <para>Or</para>
    /// <para>businessCenters (exactly one occurance; of the type BusinessCenters) A container for a set of financial
    /// business centers. This set of business centers is used to determin whether a day is a business day or not.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BusinessDateRange : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedFirstDate", Order = 1)]
		[ControlGUI(Name = "firstDate")]
        public EFS_Date unadjustedFirstDate;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedLastDate", Order = 2)]
		[ControlGUI(Name = "lastDate")]
        public EFS_Date unadjustedLastDate;

		[System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 3)]
		[ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;
		#endregion Members
    }
    #endregion BusinessDateRange
    #region BusinessDayAdjustments
    /// <summary>
    /// <para><b>Description :</b> A type defining the business day convention and financial business centers used 
    /// for adjusting any relevant date if it would otherwise fall on a day that is not a business day in 
    /// the specified business centers.</para>
    /// <para><b>Contents :</b></para>
    /// <para>businessDayConvention (exactly one occurance; of the type BusinessDayConventionEnum) The convention
    /// for adjusting a date if it would otherwise fall on a day that is not a business day.</para>
    /// <para>Either</para>
    /// <para>businessCentersReference (exactly one occurance; with locally defined content) A pointer style reference to a
    /// set of financial business centers defined elsewhere in the document. This set of business centers is used to
    /// determine whether a particular day is a business day or not.</para>
    /// <para>Or</para>
    /// <para>businessCenters (exactly one occurance; of the type BusinessCenters) A container for a set of financial
    /// business centers. This set of business centers is used to determin whether a day is a business day or not.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class BusinessDayAdjustments : ItemGUI
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 1)]
        [ControlGUI(Name = "convention")]
        public BusinessDayConventionEnum businessDayConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers", IsCopyPaste = true)]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.BusinessDayAdjustments)]
        public EFS_Id efs_id;
		#endregion Members
    }

    #endregion BusinessDayAdjustments
    #region BusinessDayAdjustmentsReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableDate2</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class BusinessDayAdjustmentsReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        [ControlGUI(IsLabel = false, Name = "value", Width = 200)]
        public string href;
		#endregion Members
	}
    #endregion BusinessDayAdjustmentsReference

    #region CalculationAgent
    /// <summary>
    /// <para><b>Description :</b> A type defining the ISDA calcuation agent responsible for performing duties 
    /// as defined in the applicable product definitions.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>calculationAgentPartyReference (one or more occurances; with locally defined content) A pointer style
    /// reference to a party identifier defined elsewhere in the document. The party referenced is the ISDA Calculation
    /// Agent for the trade. If more than one party is referenced then the parties are assumed to be co-calculation
    /// agents, i.e. they have joint reponsibility.</para>
    /// <para>Or</para>
    /// <para>calculationAgentParty (exactly one occurance; of the type CalculationAgentPartyEnum) The ISDA Calculation
    /// Agent where the actual party responsible for performing the duties associated with an optional early
    /// termination provision will be determined at exercise. For example, the Calculation Agent may be defined as
    /// being the Non-exercising Party.</para>
    /// <para>businessCenter (zero or one occurance; with locally defined content) A code identifying a financial business
    /// center location. A list of business centers may be ordered in the document alphabetically based on business
    /// center code. An FpML document containing an unordered business center list is still regarded as a conformant
    /// document.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class CalculationAgent : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice calculationAgent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentPartyReference", typeof(ArrayPartyReference),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party Reference", IsVisible = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Party Reference")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public ArrayPartyReference[] calculationAgentPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentPartySpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentParty", typeof(CalculationAgentPartyEnum),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public CalculationAgentPartyEnum calculationAgentParty;
		#endregion Members
    }
    #endregion CalculationAgent
    #region CalculationPeriodFrequency
    /// <summary>
    /// <para><b>Description :</b> A type defining the frequency at which calculation period end dates occur within the regular 
    /// part of the calculation period schedule and thier roll date convention.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Interval)</para>
    /// <para>• A type defining a time interval or offset, e.g. one day, three months. Used for specifying
    /// frequencies at which events occur, the tenor of a floating rate or an offset relative to another
    /// date.</para>
    /// <para>rollConvention (exactly one occurance; of the type RollConventionEnum) Used in conjunction with a frequency
    /// and the regular period start date of a calculation period, determines each calculation period end date within the
    /// regular part of a calculation period schedule.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class CalculationPeriodFrequency : Interval
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("rollConvention", Order = 1)]
		[ControlGUI(Name = "Roll", Width = 190)]
        public RollConventionEnum rollConvention;
		#endregion Members
    }
    #endregion CalculationPeriodFrequency
    #region ClearanceSystem
    /// <summary>
    /// <para><b>Description :</b> Unless otherwise specified, the principal clearance system customarily used for settling 
    /// trades in the relevant underlying.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Equity</para>
    ///<para>• Complex type: UnderlyingAsset</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ClearanceSystem : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string clearanceSystemIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion ClearanceSystem
    #region ContractualDefinitions
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ContractualDefinitions : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue contractualDefinitions;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string contractualDefinitionsScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
		#endregion Members
		#region Constructors
		public ContractualDefinitions()
        {
            contractualDefinitionsScheme = "http://www.fpml.org/coding-scheme/contractual-definitions-3-0";
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
    #endregion ContractualDefinitions
    #region ContractualMatrix
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ContractualMatrix : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("matrixType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 300)]
        public MatrixType matrixType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool publicationDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("publicationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Publication Date")]
        public EFS_Date publicationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool matrixTermSpecified;
		[System.Xml.Serialization.XmlElementAttribute("matrixTerm", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Term", Width = 350)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public MatrixTerm matrixTerm;
		#endregion Members
		#region Constructors
		public ContractualMatrix()
        {
            publicationDate = new EFS_Date();
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
    #endregion ContractualMatrix
    #region ContractualSupplement
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ContractualSupplement : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue contractualSupplement;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string contractualSupplementScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
		#endregion Members
		#region Constructors
		public ContractualSupplement()
        {
            contractualSupplementScheme = "http://www.fpml.org/coding-scheme/contractual-supplement-3-0";
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
    #endregion ContractualSupplement
    #region Country
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Country : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string countryScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
		#endregion Members
    }
    #endregion Country
    #region CreditSeniority
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CreditSeniority : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string creditSeniorityScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public CreditSeniority()
        {
            creditSeniorityScheme = "http://www.fpml.org/coding-scheme/credit-seniority-1-0";
        }
        #endregion Constructors
    }
    #endregion CreditSeniority
    #region Currency
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Currency : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string currencyScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
		#endregion Members
    }
    #endregion Currency

    #region DateList
    public partial class DateList : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("date", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date", IsChild = false)]
        public EFS_DateArray[] date;
		#endregion Members
    }
    #endregion DateList
    #region DateOffset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class DateOffset
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
		[System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
		[ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
		[ControlGUI(Name = "dayType")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
		[System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 4)]
		[ControlGUI(Name = "convention", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public BusinessDayConventionEnum businessDayConvention;
		#endregion Members
    }
    #endregion DateOffset
    #region DateRange
    /// <summary>
    /// <para><b>Description :</b> A type defining a contiguous series of calendar dates. 
    /// The date range is defined as all the dates between and including the first and the last date. 
    /// The first date must fall before the last date.</para>
    /// <para><b>Contents :</b></para>
    /// <para>unadjustedFirstDate (exactly one occurance; of the type xsd:date) The first date of a date range.</para>
    /// <para>unadjustedLastDate (exactly one occurance; of the type xsd:date) The last date of a date range.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex Types: BusinessDateRange</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BusinessDateRange))]
    public class DateRange : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("unadjustedFirstDate", Order = 1)]
        [ControlGUI(Name = "firstDate")]
        public EFS_Date unadjustedFirstDate;
		[System.Xml.Serialization.XmlElementAttribute("unadjustedLastDate", Order = 2)]
		[ControlGUI(Name = "lastDate")]
        public EFS_Date unadjustedLastDate;
		#endregion Members
	}
    #endregion DateRange
    #region DateReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: NotionalStepRule</para>
    ///<para>• Complex type: PaymentDates</para>
    ///<para>• Complex type: ResetDates</para>
    ///<para>• Complex type: StubCalculationPeriodAmount</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class DateReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
	}
    #endregion DateReference
    #region DateRelativeTo
    /// <summary>
    /// <para><b>Description :</b> Specifies the anchor as an href attribute. The href attribute value is a pointer style 
    /// reference to the element or component elsewhere in the document where the anchor date is defined.</para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: EquitySwapEarlyTerminationType</para>
    ///<para>• Complex type: RelativeDateOffset</para>
    ///<para>• Complex type: RelativeDateSequence</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class DateRelativeTo : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }
    #endregion DateRelativeTo
    #region DateTimeList
    public class DateTimeList : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("dateTime", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date", IsChild = false)]
        public EFS_DateTimeArray[] dateTime;
		#endregion Members

		#region Methods
		#region DisplayArray
        public static object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            if (MethodsGUI.IsOptionalControl(pCurrent, pFldCurrent))
                return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
            else
                return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion DateList

    #region DayCountFraction
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class DayCountFraction : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string dayCountFractionScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
		#endregion Members
		#region Constructors
		public DayCountFraction()
        {
            dayCountFractionScheme = "http://www.fpml.org/coding-scheme/day-count-fraction-1-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            DayCountFraction clone = new DayCountFraction();
            clone.dayCountFractionScheme = this.dayCountFractionScheme;
            clone.Value = this.Value;
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion DayCountFraction
    #region DividendConditions
    /// <summary>
    /// <para><b>Description :</b> A type describing the conditions governing the payment of dividends to 
    /// the receiver of the equity return. With
    /// the exception of the dividend payout ratio, which is defined for each of the underlying components.</para>
    /// <para><b>Contents :</b></para>
    /// <para>dividendReinvestment (exactly one occurrence; of the type xsd:boolean) Boolean element that defines
    /// whether the dividend will be reinvested or not.</para>
    /// <para>dividendEntitlement (exactly one occurrence; of the type DividendEntitlementEnum) Defines the date on
    /// which the receiver on the equity return is entitled to the dividend.</para>
    /// <para>dividendPaymentDate (zero or one occurrence; of the type DividendPaymentDate) Specifies when the
    /// dividend will be paid to the receiver of the equity return. Has the meaning as defined in the ISDA 2002 Equity
    /// Derivatives Definitions. Is not applicable in the case of a dividend reinvestment election.</para>
    /// <para>dividendPeriodEffectiveDate (zero or one occurrence; with locally defined content) Dividend period has the
    /// meaning as defined in the ISDA 2002 Equity Derivatives Definitions. This element specifies the date on which
    /// the dividend period will commence.</para>
    /// <para>dividendPeriodEndDate (zero or one occurrence; with locally defined content) Dividend period has the
    /// meaning as defined in the ISDA 2002 Equity Derivatives Definitions. This element specifies the date on which
    /// the dividend period will end. It includes a boolean attribute for defining whether this end date is included or
    /// excluded from the dividend period.</para>
    /// <para>paymentCurrency (exactly one occurrence; with locally defined content) Currency in which the payment
    /// relating to the leg amount (equity amount or interest amount) or the dividend will be denominated.</para>
    /// <para>dividendFxTriggerDate (zero or one occurrence; of the type DividendPaymentDate) Specifies the date on
    /// which the FX rate will be considered in the case of a Composite FX swap.</para>
    /// <para>interestAccrualsMethod (zero or one occurrence; with locally defined content) Defines the way in which
    /// interests are accrued: the applicable rate (fixed or floating reference) and the compounding method.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Return</para>
    ///<para><b>Derived by :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class DividendConditions : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendReinvestmentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendReinvestment", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Reinvestment")]
        public EFS_Boolean dividendReinvestment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendEntitlementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendEntitlement", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Entitlement")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendEntitlementEnum dividendEntitlement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendAmount", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend amount")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendAmountTypeEnum dividendAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPaymentDate", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Payment Date")]
        public DividendPaymentDate dividendPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodEffectiveDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPeriodEffectiveDate", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Period Effective Date")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateReference dividendPeriodEffectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPeriodEndDate", Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Period End Date")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateReference dividendPeriodEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendPeriod", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Period")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendPeriodEnum dividendPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extraOrdinaryDividendsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("extraOrdinaryDividends", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "ExtraOrdinary Party reference")]
        public PartyReference extraOrdinaryDividends;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excessDividendAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("excessDividendAmount", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Gross Cash Dividend per Share")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DividendAmountTypeEnum excessDividendAmount;
		[System.Xml.Serialization.XmlElementAttribute("paymentCurrency", Order = 10)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Currency", IsVisible = true)]
        public PaymentCurrency paymentCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendFxTriggerDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dividendFxTriggerDate", Order = 11)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Currency")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Fx Trigger Date")]
        public DividendPaymentDate dividendFxTriggerDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestAccrualsMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("interestAccrualsMethod", Order = 12)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Interest Accruals Method")]
        public InterestAccrualsCompoundingMethod interestAccrualsMethod;
		#endregion Members
	}
    #endregion DividendConditions
    #region DividendPaymentDate
    /// <summary>
    /// <para><b>Description :</b> A type describing the date on which the dividend will be paid/received. 
    /// This type is also used to specify the
    /// date on which the FX rate will be determined, when applicable.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>dividendDateReference (exactly one occurrence; of the type DividendDateReferenceEnum) Reference to a
    /// dividend date, either the pay date, the ex date or the record date.</para>
    /// <para>Or</para>
    /// <para>adjustableDate (exactly one occurrence; of the type AdjustableDate) A date that shall be subject to
    /// adjustment if it would otherwise fall on a day that is not a business day in the specified business centers,
    /// together with the convention for adjusting the date.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: DividendConditions</para>
    ///<para><b>Derived by :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class DividendPaymentDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice paymentDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateDividendDateReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendDateReference", typeof(DividendDateReferenceEnum),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Date Reference", IsVisible = true)]
        [ControlGUI(Name = "value", Width = 200)]
        public DividendDateReferenceEnum paymentDateDividendDateReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Adjustable Date", IsVisible = true)]
        public AdjustableDate paymentDateAdjustableDate;
		#endregion Members
	}
    #endregion DividendPaymentDate

    #region Documentation
    /// <summary>
    /// <para><b>Description :</b> An entity for defining the definitions that govern the document and should include 
    /// the year and type of definitions referenced, along with any relevant documentation (such as master agreement) 
    /// and the date it was signed.</para>
    /// <para><b>Contents :</b></para>
    /// <para>masterAgreement (zero or one occurance; of the type MasterAgreement) The agreement executed between
    /// the parties and intended to govern all OTC derivatives transactions between those parties.</para>
    /// <para>masterConfirmation (zero or one occurance; of the type MasterConfirmation) The agreement executed
    /// between the parties and intended to govern all OTC derivatives transactions between those parties.</para>
    /// <para>contractualDefinitions (zero or more occurances; of the type ContractualDefinitions) The definitions (such as
    /// those published by ISDA) published by ISDA that will define the terms of the trade.</para>
    /// <para>contractualSupplement (zero or more occurances; of the type ContractualSupplement) A contractual
    /// supplement (such as those published by ISDA) that will apply to the trade.</para> 
    /// <para>creditSupportDocument (zero or one occurance; of the type xsd:string) The agreement executed between the
    /// parties and intended to govern collateral arrangement for all OTC derivatives transactions between those
    /// parties.</para> 
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Documentation : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterAgreementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("masterAgreement", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Master Agreement")]
        public MasterAgreement masterAgreement;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice documentation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty documentationNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationMasterConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterConfirmation", typeof(MasterConfirmation),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Master Confirmation", IsVisible = true)]
        public MasterConfirmation documentationMasterConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool documentationBrokerConfirmationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("brokerConfirmation", typeof(BrokerConfirmation),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Broker Confirmation", IsVisible = true)]
        public BrokerConfirmation documentationBrokerConfirmation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualDefinitionsSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual Definitions")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ContractualDefinitions", IsClonable = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractualDefinitions",Order=4)]
        public ContractualDefinitions[] contractualDefinitions;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualSupplementSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual Supplement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ContractualSupplement", IsClonable = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractualSupplement",Order=5)]
        public ContractualSupplement[] contractualSupplement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractualMatrixSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Contractual Matrix")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ContractualMatrix", IsClonable = true, IsChild = true)]
        [System.Xml.Serialization.XmlElementAttribute("contractualMatrix",Order=6)]
        public ContractualMatrix[] contractualMatrix;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditSupportDocumentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("creditSupportDocument", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Credit Support Document", Width = 600)]
        public EFS_String creditSupportDocument;
		#endregion Members
		#region Constructors
		public Documentation()
        {
            documentationNone = new Empty();
            documentationMasterConfirmation = new MasterConfirmation();
            documentationBrokerConfirmation = new BrokerConfirmation();
		}
		#endregion Constructors
	}
    #endregion Documentation

    #region Empty
    /// <summary>
    /// <para><b>Description :</b> A special type meant to be used for elements with no content and not attributes.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: CreditEvents</para>
    ///<para>• Complex type: DeliverableObligations</para>
    ///<para>• Complex type: Obligations</para>
    ///<para>• Complex type: PCDeliverableObligationCharac</para>
    ///<para>• Complex type: PhysicalSettlementPeriod</para>
    ///<para>• Complex type: PubliclyAvailableInformation</para>
    ///<para>• Complex type: Restructuring</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Empty : EmptyGUI
    {
    }
    #endregion Empty
    #region EntityId
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EntityId : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue entityId;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string entityIdScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
		#endregion Members
		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion EntityId
    #region EntityName
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EntityName : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue entityName;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string entityNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
		#endregion Members
		#region Constructors
		public EntityName()
        {
            entityNameScheme = "http://www.fpml.org/spec/2003/entity-name-RED-1-0";
		}
		#endregion Constructors
	}
    #endregion EntityName
    #region EuropeanExercise
    /// <summary>
    /// <para><b>Description :</b> A type defining the exercise period for a European style option together with any rules 
    /// governing the notional amount of the underlying which can be exercised on any given exercise date and any 
    /// associated exercise fees.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Exercise)</para>
    /// <para>• The abstract base class for all types which define way in which options may be exercised.</para>
    /// <para>expirationDate (exactly one occurance; of the type AdjustableOrRelativeDate) The last day within an exercise
    /// period for an American style option. For a European style option it is the only day within the exercise period.</para>
    /// <para>relevantUnderlyingDate (zero or one occurance; of the type AdjustableOrRelativeDates) The daye on the
    /// underlying set by the exercise of an option. What this date is depends on the option (e.g. in a swaption it is the
    /// effective date, in an extendible/cancelable provision it is the termination date).</para>
    /// <para>earliestExerciseTime (exactly one occurance; of the type BusinessCenterTime) The earliest time at which
    /// notice of exercise can be given by the buyer to the seller (or seller's agent) i) on the expriation date, in the
    /// case of a European style option, (ii) on each bermuda option exercise date and the expiration date, in the case
    /// of a Bermudan style option the commencement date to, and including, the expiration date , in the case of an
    /// American option.</para>
    /// <para>expirationTime (exactly one occurance; of the type BusinessCenterTime) The latest time for exercise on
    /// expirationDate.</para>
    /// <para>partialExercise (zero or one occurance; of the type PartialExercise) As defined in the 2000 ISDA Definitions,
    /// Section 12.3. Partial Exercise, the buyer of the option has the right to exercise all or less than all the notional
    /// amount of the underlying swap on the expiration date, but may not exercise less than the minimum notional
    /// amount, and if an integral multiple amount is specified, the notional amount exercised must be equal to, or be
    /// an integral multiple of, the integral multiple amount.</para>
    /// <para>exerciseFee (zero or one occurance; of the type ExerciseFee) A fee to be paid on exercise. This could be
    /// represented as an amount or a rate and notional reference on which to apply the rate.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: europeanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("europeanExercise", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class EuropeanExercise : Exercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date", IsVisible = false, IsCopyPaste = true)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relevantUnderlyingDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relevantUnderlyingDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Relevant Underlying Date", IsCopyPaste = true)]
        public AdjustableOrRelativeDates relevantUnderlyingDate;
		[System.Xml.Serialization.XmlElementAttribute("earliestExerciseTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time", IsVisible = false)]
        public BusinessCenterTime earliestExerciseTime;
		[System.Xml.Serialization.XmlElementAttribute("expirationTime", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Earliest Exercise Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time", IsVisible = false)]
        public BusinessCenterTime expirationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partialExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partialExercise", Order = 5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiration Time")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Partial Exercise", IsCopyPaste = true)]
        public PartialExercise partialExercise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseFeeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseFee", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Fee", IsCopyPaste = true)]
        public ExerciseFee exerciseFee;
		#endregion Members
    }
    #endregion EuropeanExercise
    #region ExchangeId
    /// <summary>
    /// <para><b>Description :</b> A short form unique identifier for an exchange. If the element is not present then 
    /// the exchange shall be the primary exchange on which the underlying is listed. 
    /// The term "Exchange" is assumed to have the meaning as defined in the ISDA 2002 Equity Derivatives Definitions.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: bond</para>
    ///<para>• Element: convertibleBond</para>
    ///<para>• Element: equity</para>
    ///<para>• Element: exchangeTradedFund</para>
    ///<para>• Element: future</para>
    ///<para>• Element: index</para>
    ///<para>• Complex type: Equity</para>
    ///<para>• Complex type: UnderlyingAsset</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class ExchangeId : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue exchangeId;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string exchangeIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 250)]
        public string Value;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
            Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeId";
        #endregion Members
    }
    #endregion ExchangeId
    #region Exercise
    /// <summary>
    /// <para><b>Description :</b> The abstract base class for all types which define way in which options may be exercised.</para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: exercise</para>
    ///<para>• Complex type: AmericanExercise</para>
    ///<para>• Complex type: BermudaExercise</para>
    ///<para>• Complex type: EquityAmericanExercise</para>
    ///<para>• Complex type: EuropeanExercise</para>
    ///<para>• Complex type: SharedAmericanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: AmericanExercise</para>
    ///<para>• Complex type: BermudaExercise</para>
    ///<para>• Complex type: EquityAmericanExercise</para>
    ///<para>• Complex type: EuropeanExercise</para>
    ///<para>• Complex type: SharedAmericanExercise</para>
    ///</remarks>
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityEuropeanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BermudaExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmericanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SharedAmericanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAmericanExercise))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EuropeanExercise))]
    public partial class Exercise : ItemGUI
	{
		#region Members
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
    #endregion Exercise
    #region ExerciseFee
    /// <summary>
    /// <para><b>Description :</b> A type defining the fee payable on exercise of an option. 
    /// This fee may be defined as an amount or a percentage of the notional exercised.</para>
    /// <para><b>Contents :</b></para>
    /// <para>payerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party 
    /// responsible for making the payments defined by this structure.</para>
    /// <para>receiverPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party 
    /// that receives the payments corresponding to this structure.</para>
    /// <para>notionalReference (exactly one occurrence; of the type NotionalReference)</para>
    /// <para>Either</para>
    /// <para>feeAmount (exactly one occurrence; of the type xsd:decimal) The amount of fee to be paid on exercise. 
    /// The fee currency is that of the referenced notional.</para>
    /// <para>Or</para>
    /// <para>feeRate (exactly one occurrence; of the type xsd:decimal) A fee represented as a percentage of some referenced notional. 
    /// A percentage of 5% would be represented as 0.05.</para>
    /// <para>feePaymentDate (exactly one occurrence; of the type RelativeDateOffset) The date on which exercise fee(s) will be paid. 
    /// It is specified as a relative date.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("exerciseFee", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class ExerciseFee : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer", LblWidth = 120)]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 3)]
        [ControlGUI(Name = "NotionalReference", LblWidth = 100, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 170)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public NotionalReference notionalReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee")]
        public EFS_RadioChoice typeFee;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeAmount", typeof(EFS_Decimal),Order=4)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal typeFeeAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeRate", typeof(EFS_Decimal),Order=5)]
        [ControlGUI(Name = "value", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75, Regex = EFSRegex.TypeRegex.RegexFixedRateExtend)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal typeFeeRate;

		[System.Xml.Serialization.XmlElementAttribute("feePaymentDate", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date", IsVisible = false, IsCopyPaste = true)]
        public RelativeDateOffset feePaymentDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
    }
    #endregion ExerciseFee
    #region ExerciseFeeSchedule
    /// <summary>
    /// <para><b>Description :</b> A type to define a fee or schedule of fees to be payable on the exercise of an option. 
    /// This fee may be defined as an amount or a percentage of the notional exercised.</para>
    /// <para><b>Contents :</b></para>
    /// <para>payerPartyReference (exactly one occurance; with locally defined content) A pointer style reference to a party
    /// identifier defined elsewhere in the document.</para>
    /// <para>receiverPartyReference (exactly one occurance; with locally defined content) A pointer style reference to a
    /// party identifier defined elsewhere in the document.</para>
    /// <para>notionalReference (exactly one occurance; with locally defined content) A pointer style reference to the
    /// associated notional schedule defined elsewhere in the document.</para>
    /// <para>Either</para>
    /// <para>feeAmountSchedule (exactly one occurance; of the type AmountSchedule) The exercise fee amount schedule.
    /// The fees are expressed as currency amounts. The currency of the fee is assumed to be that of the notional
    /// schedule referenced.</para>
    /// <para>Or</para>
    /// <para>feeRateSchedule (exactly one occurance; of the type Schedule) The exercise fee rate schedule. The fees are
    /// expressed as percentage rates of the notional being exercised. The currency of the fee is assumed to be that
    /// of the notional schedule referenced.</para>
    /// <para>feePaymentDate (exactly one occurance; of the type RelativeDateOffset) The date on which exercise fee(s)
    /// will be paid. It is specified as a relative date.</para>
    /// </summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ExerciseFeeSchedule : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer", LblWidth = 287)]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
		[ControlGUI(Name = "Receiver", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 3)]
		[ControlGUI(Name = "NotionalReference", LblWidth = 267, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 170)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public NotionalReference notionalReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee", IsCopyPaste = true)]
        public EFS_RadioChoice typeFee;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeAmountSchedule", typeof(AmountSchedule),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AmountSchedule typeFeeAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeFeeRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("feeRateSchedule", typeof(Schedule),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Schedule typeFeeRate;
		[System.Xml.Serialization.XmlElementAttribute("feePaymentDate", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date", IsVisible = true, IsCopyPaste = true)]
        [ControlGUI(IsPrimary = false, Name = "offset", LblWidth = 233)]
        public RelativeDateOffset feePaymentDate;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fee Payment Date")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
    }
    #endregion ExerciseFeeSchedule
    #region ExerciseNotice
    /// <summary>
    /// <para><b>Description :</b> A type defining to whom and where notice of execution should be given. 
    /// The partyReference refers to one of the principal parties of the trade. 
    /// If present the exerciseNoticePartyReference refers to a party, other than the principal party, 
    /// to whome notice should be given.</para>
    /// <para><b>Contents :</b></para>
    /// <para>partyReference (exactly one occurance; with locally defined content) A pointer style reference to a party
    /// identifier defined elsewhere in the document. The party referenced has allocated the trade identifier.</para>
    /// <para>exerciseNoticePartyReference (zero or one occurance; with locally defined content) A pointer style reference
    /// to a party identifier defined elsewhere in the document. The party referenced is the party to which notice of
    /// exercise should be given by the buyer.</para>
    /// <para>businessCenter (exactly one occurance; with locally defined content) A code identifying a financial business
    /// center location. A list of business centers may be ordered in the document alphabetically based on business
    /// center code. An FpML document containing an unordered business center list is still regarded as a conformant
    /// document.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ExerciseNotice : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("partyReference", Order = 1)]
		[ControlGUI(Name = "partyReference")]
		public PartyReference partyReference;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool exerciseNoticePartyReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseNoticePartyReference", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Notice Party Reference")]
		public PartyReference exerciseNoticePartyReference;
		[System.Xml.Serialization.XmlElementAttribute("businessCenter", Order = 3)]
		[ControlGUI(IsPrimary = false, Name = "business Center")]
		public BusinessCenter businessCenter;
		#endregion Members
    }
    #endregion ExerciseNotice
    #region ExerciseNotionalReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ExerciseNotionalReference
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
		}
		#endregion Members
	}
    #endregion ExerciseNotionalReference
    #region ExerciseProcedure
    /// <summary>
    /// <para><b>Description :</b> A type describing how notice of exercise should be given. 
    /// This can be either manual or automatic.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>manualExercise (exactly one occurance; of the type ManualExercise) Specifies that the notice of exercise
    /// must be given by the buyer to the seller or seller's agent.</para>
    /// <para>Or</para>
    /// <para>automaticExercise (exactly one occurance; of the type AutomaticExercise) If automatic is specified then the
    /// notional amount of the underlying swap, not previously exercised under the swaption will be automatically
    /// exercised at the expriration time on the expiration date if at such time the buyer is in-the-money, provided that
    /// the difference between the settlement rate and the fixed rate under the relevant underlying swap is not less
    /// than the specified threshold rate. The term in-the-money is assumed to have the meaning defining in the 2000
    /// ISDA Definitions, Section 17.4 In-the-money.</para>
    /// <para>followUpConfirmation (exactly one occurance; of the type xsd:boolean) A flag to indicate whether follow-up
    /// confirmation of exercise (written or electronic) is required following telephonic notice by the buyer to the seller
    /// or seller's agent.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    /// EG 20150422 [20513] BANCAPERTA New followUpConfirmationSpecified
    public partial class ExerciseProcedure : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice exerciseProcedure;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseProcedureAutomaticSpecified;
        [System.Xml.Serialization.XmlElementAttribute("automaticExercise", typeof(AutomaticExercise),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Automatic Exercise", IsVisible = true)]
        public AutomaticExercise exerciseProcedureAutomatic;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseProcedureManualSpecified;
        [System.Xml.Serialization.XmlElementAttribute("manualExercise", typeof(ManualExercise),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Manual Exercise", IsVisible = true)]
        public ManualExercise exerciseProcedureManual;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool followUpConfirmationSpecified;

		[System.Xml.Serialization.XmlElementAttribute("followUpConfirmation", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "followUpConfirmation")]
        public EFS_Boolean followUpConfirmation;
		#endregion Members
    }
    #endregion ExerciseProcedure

    #region FloatingRate
    /// <summary>
    /// <para><b>Description :</b> A type defining a floating rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>floatingRateIndex (exactly one occurrence; of the type FloatingRateIndex)</para>
    /// <para>indexTenor (zero or one occurrence; of the type Interval) The ISDA Designated Maturity, i.e. the tenor of the
    /// floating rate.</para>
    /// <para>floatingRateMultiplierSchedule (zero or one occurrence; of the type Schedule) A rate multiplier or multiplier
    /// schedule to apply to the floating rate. A multiplier schedule is expressed as explicit multipliers and dates. In the
    /// case of a schedule, the step dates may be subject to adjustment in accordance with any adjustments specified
    /// in the calculationPeriodDatesAdjustments. The multiplier can be a positive or negative decimal. This element
    /// should only be included if the multiplier is not equal to 1 (one) for the term of the stream.</para>
    /// <para>spreadSchedule (zero or one occurrence; of the type Schedule) The ISDA Spread or a Spread schedule
    /// expressed as explicit spreads and dates. In the case of a schedule, the step dates may be subject to
    /// adjustment in accordance with any adjustments specified in calculationPeriodDatesAdjustments. The spread is
    /// a per annum rate, expressed as a decimal. For purposes of determining a calculation period amount, if
    /// positive the spread will be added to the floating rate and if negative the spread will be subtracted from the
    /// floating rate. A positive 10 basis point (0.1%) spread would be represented as 0.001.</para>
    /// <para>rateTreatment (zero or one occurrence; of the type RateTreatmentEnum) The specification of any rate
    /// conversion which needs to be applied to the observed rate before being used in any calculations. The two
    /// common conversions are for securities quoted on a bank discount basis which will need to be converted to
    /// either a Money Market Yield or Bond Equivalent Yield. See the Annex to the 2000 ISDA Definitions, Section
    /// 7.3. Certain General Definitions Relating to Floating Rate Options, paragraphs (g) and (h) for definitions of
    /// these terms.</para>
    /// <para>capRateSchedule (zero or more occurrences; of the type StrikeSchedule) The cap rate or cap rate schedule,
    /// if any, which applies to the floating rate. The cap rate (strike) is only required where the floating rate on a swap
    /// stream is capped at a certain level. A cap rate schedule is expressed as explicit cap rates and dates and the
    /// step dates may be subject to adjustment in accordance with any adjustments specified in
    /// calculationPeriodDatesAdjustments. The cap rate is assumed to be exclusive of any spread and is a per
    /// annum rate, expressed as a decimal. A cap rate of 5% would be represented as 0.05.</para>
    /// <para>floorRateSchedule (zero or more occurrences; of the type StrikeSchedule) The floor rate or floor rate
    /// schedule, if any, which applies to the floating rate. The floor rate (strike) is only required where the floating rate
    /// on a swap stream is floored at a certain strike level. A floor rate schedule is expressed as explicit floor rates
    /// and dates and the step dates may be subject to adjustment in accordance with any adjustments specified in
    /// calculationPeriodDatesAdjustments. The floor rate is assumed to be exclusive of any spread and is a per
    /// annum rate, expressed as a decimal. A floor rate of 5% would be represented as 0.05.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRateCalculation</para>
    ///<para>• Complex type: Stub</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: FloatingRateCalculation</para>
    ///</remarks>
    ///
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FloatingRateCalculation))]
    public partial class FloatingRate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("floatingRateIndex",Order=1)]
        [ControlGUI(Name = "Rate", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexTenorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation rules", IsVisible = false, IsGroup = true)]
        public bool FillBalise;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floatingRateMultiplierScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateMultiplierSchedule", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "FloatingRate Multiplier Schedule", IsCopyPaste = true)]
        public Schedule floatingRateMultiplierSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spreadScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spreadSchedule", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread Schedule", IsCopyPaste = true)]
        public Schedule spreadSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateTreatmentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateTreatment", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Treatment")]
        public RateTreatmentEnum rateTreatment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool capRateScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("capRateSchedule",Order=6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap Rate Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cap Rate Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public StrikeSchedule[] capRateSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool floorRateScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floorRateSchedule",Order=7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor Rate Schedule", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Floor Rate Schedule", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public StrikeSchedule[] floorRateSchedule;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation rules")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.RateIndex)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion FloatingRate
    #region FloatingRateCalculation
    /// <summary>
    /// <para><b>Description :</b> A type defining the floating rate and definitions relating to the calculation of floating 
    /// rate amounts.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type FloatingRate)</para>
    /// <para>• A type defining a floating rate.</para>
    /// <para>initialRate (zero or one occurrence; of the type xsd:decimal) The initial floating rate reset agreed between the
    /// principal parties involved in the trade. This is assumed to be the first required reset rate for the first regular
    /// calculation period. It should only be included when the rate is not equal to the rate published on the source
    /// implied by the floating rate index. An initial rate of 5% would be represented as 0.05.</para>
    /// <para>finalRateRounding (zero or one occurrence; of the type Rounding) The rounding convention to apply to the
    /// final rate used in determination of a calculation period amount.</para>
    /// <para>averagingMethod (zero or one occurrence; of the type AveragingMethodEnum) If averaging is applicable, this
    /// component specifies whether a weighted or unweighted average method of calculation is to be used. The
    /// component must only be included when averaging applies.</para>
    /// <para>negativeInterestRateTreatment (zero or one occurrence; of the type NegativeInterestRateTreatmentEnum)
    /// The specification of any provisions for calculating payment obligations when a floating rate is negative (either
    /// due to a quoted negative floating rate or by operation of a spread that is subtracted from the floating rate).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Calculation</para>
    ///<para>• Complex type: InterestAccrualsMethod</para>
    ///<para>• Complex type: InterestLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FloatingRateCalculation : FloatingRate
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("initialRate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "initialRate", Regex = EFSRegex.TypeRegex.RegexFixedRate)]
        public EFS_Decimal initialRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool finalRateRoundingSpecified;
		[System.Xml.Serialization.XmlElementAttribute("finalRateRounding", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "finalRateRounding")]
        public Rounding finalRateRounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool averagingMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averagingMethod", Order = 3)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "averagingMethod")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AveragingMethodEnum averagingMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool negativeInterestRateTreatmentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("negativeInterestRateTreatment", Order = 4)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "negativeInterestRateTreatment")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public NegativeInterestRateTreatmentEnum negativeInterestRateTreatment;
		#endregion Members
    }
    #endregion FloatingRateCalculation
    #region FloatingRateIndex
    /// <summary>
    /// <para><b>Description :</b> The ISDA Floating Rate Option, i.e. the floating rate index.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRate</para>
    ///<para>• Complex type: Fra</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class FloatingRateIndex : SchemeBoxGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string floatingRateIndexScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FloatingRateIndex";
		#endregion Members
    }
    #endregion FloatingRateIndex
    #region ForecastRateIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ForecastRateIndex : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("floatingRateIndex",Order=1)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlElementAttribute("indexTenor", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "indexTenor", LblWidth = 180)]
        public Interval indexTenor;
		#endregion Members
	}
    #endregion ForecastRateIndex

    #region FxCashSettlement
    /// <summary>
    /// <para><b>Description :</b> A type that is used for describing cash settlement of an option / non deliverable forward. 
    /// It includes the currency to settle into together with the fixings required to calculate the currency amount.</para>
    /// <para><b>Contents :</b></para>
    /// <para>settlementCurrency (exactly one occurrence; of the type Currency) The currency in which a cash settlement
    /// for non-deliverable forward and non-deliverable options.</para>
    /// <para>fixing (one or more occurrences; of the type FXFixing) Specifies the source for and timing of a fixing of an
    /// exchange rate. This is used in the agreement of non-deliverable forward trades as well as various types of FX
    /// OTC options that require observations against a particular rate.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxLeg</para>
    ///<para>• Complex type: FxOptionLeg</para>
    ///<para>• Complex type: QuotableFXLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    //	[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class FxCashSettlement : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("settlementCurrency", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ControlGUI(Name = "Settlement Currency", Width = 75)]
        public Currency settlementCurrency;
        [System.Xml.Serialization.XmlElementAttribute("fixing", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing", IsClonable = true, IsChild = true)]
        public FxFixing[] fixing;
		#endregion Members
		#region Members
        //PL 20100628 customerSettlementRateSpecified à supprimer plus tard...
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool customerSettlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("customerSettlementRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "customer Settlement Rate")]
        public Empty customerSettlementRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationAgentSettlementRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationAgentSettlementRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "calculation Agent Settlement Rate")]
        public Empty calculationAgentSettlementRate;
        #endregion Members
	}
    #endregion FxCashSettlement
    #region FxFixing
    /// <summary>
    /// <para><b>Description :</b> A type that specifies the source for and timing of a fixing of an exchange rate. 
    /// This is used in the agreement of non-deliverable forward trades as well as various types of FX OTC options that 
    /// require observations against a particular rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type FxSpotRateSource)</para>
    /// <para>• A type defining the source and time for an fx rate.</para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>fixingDate (exactly one occurrence; of the type xsd:date) Describes the specific date when a non-deliverable
    /// forward or non-deliverable option will "fix" against a particular rate, which will be used to compute the ultimate
    /// cash settlement.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxCashSettlement</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxFixing : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted Currency Pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlElementAttribute("fixingDate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted Currency Pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Fixing Date")]
        public EFS_Date fixingDate;
		#endregion Members
    }
    #endregion FxFixing
    #region FxRate
    /// <summary>
    /// <para><b>Description :</b> A type describing the rate of a currency conversion: pair of currency, 
    /// quotation mode and exchange rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>rate (exactly one occurrence; of the type xsd:decimal) The rate of exchange between the two currencies of the
    /// leg of a deal. Must be specified with a quote basis.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ExchangeRate</para>
    ///<para>• Complex type: EquityLeg</para>
    ///<para>• Complex type: Price</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: ExchangeRate</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeRate))]
    public partial class FxRate
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 1)]
		public QuotedCurrencyPair quotedCurrencyPair;
		[System.Xml.Serialization.XmlElementAttribute("rate", Order = 2)]
        [ControlGUI(Name = "rate", Regex = EFSRegex.TypeRegex.RegexFxRateExtend)]
        public EFS_Decimal rate;
		#endregion Members
    }
    #endregion FxRate
    #region FxSpotRateSource
    /// <summary>
    /// <para><b>Description :</b> A type defining the source and time for an fx rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>primaryRateSource (exactly one occurrence; of the type InformationSource) The primary source for where
    /// the rate observation will occur. Will typically be either a page or a reference bank published rate.</para>
    /// <para>secondaryRateSource (zero or one occurrence; of the type InformationSource) An alternative, or secondary,
    /// source for where the rate observation will occur. Will typically be either a page or a reference bank published
    /// rate.</para>
    /// <para>fixingTime (exactly one occurance; of the type BusinessCenterTime) The time at which the spot currency
    /// exchange rate will be observed. It is specified as a time in a specific business center, e.g. 11:00am London
    /// time.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXFixing</para>
    ///<para>• Complex type: EquityLeg</para>
    ///<para>• Complex type: FXFeature</para>
    ///<para>• Complex type: FxLinkedNotionalSchedule</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: FXFixing</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxFixing))]
    public partial class FxSpotRateSource : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Primary Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Primary Rate Source")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
		[System.Xml.Serialization.XmlElementAttribute("fixingTime", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing Time")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
    }
    #endregion FxSpotRateSource

    #region GoverningLaw
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class GoverningLaw : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string governingLawScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion GoverningLaw

    #region HourMinuteTime
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class HourMinuteTime : StringGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
    }
    #endregion HourMinuteTime

    #region IdentifiedCurrency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class IdentifiedCurrency : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string currencyScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion IdentifiedCurrency
    #region IdentifierDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class IdentifierDate : DateCalendarGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsLabel = true, Name = "value")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion IdentifierDate
    #region IdentifiedPayerReceiver
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IdentifiedPayerReceiver
    {
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 85)]
        public PayerReceiverEnum Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PayerReceiver)]
        public EFS_Id efs_id;

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
    }
    #endregion IdentifiedPayerReceiver

    #region InformationProvider
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class InformationProvider : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string informationProviderScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion InformationProvider
    #region InformationSource
    /// <summary>
    /// <para><b>Description :</b> A type defining the source for a piece of information (e.g. a rate refix or an fx fixing).</para>
    /// <para><b>Contents :</b></para>
    /// <para>rateSource (exactly one occurance; of the type InformationProvider) An information source for obtaining a
    /// market rate. For example Bloomberg, Reuters, Telerate etc.</para>
    /// <para>rateSourcePage (zero or one occurance; with locally defined content) A specific page for the rate source for
    /// obtaining a market rate.</para>
    /// <para>rateSourcePageHeading (zero or one occurance; of the type xsd:string) The specific information source page
    /// for obtaining a market rate. For example, 3750 (Telerate), LIBO (Reuters) etc.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXAmericanTrigger</para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para>• Complex type: FXBarrier</para>
    ///<para>• Complex type: FXEuropeanTrigger</para>
    ///<para>• Complex type: FxSpotRateSource</para>
    ///<para>• Complex type: SettlementRateSource</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class InformationSource : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate ref.", Width = 300)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        public AssetFxRateId assetFxRateId;

        [System.Xml.Serialization.XmlElementAttribute("rateSource", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "rateSource", Width = 400)]
        public InformationProvider rateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourcePageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateSourcePage", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "rateSourcePage", Width = 600)]
        public RateSourcePage rateSourcePage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourcePageHeadingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rateSourcePageHeading", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "heading", Width = 600)]
        public EFS_String rateSourcePageHeading;
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:InformationSource";
		#endregion Members
    }
    #endregion InformationSource
    #region InstrumentId
    /// <summary>
    /// <para><b>Description :</b> A short form unique identifier for a security.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: UnderlyingAsset</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class InstrumentId
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue instrumentId;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 200)]
        public string instrumentIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 300)]
        public string Value;
		#endregion Members
    }
    #endregion InstrumentId
    #region InterestAccrualsMethod
    /// <summary>
    /// <para><b>Description :</b> A type describing the method for accruing interests on dividends. Can be either a 
    /// fixed rate reference or a floating rate reference.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>floatingRateCalculation (exactly one occurrence; of the type FloatingRateCalculation) The floating rate
    /// calculation definitions</para>
    /// <para>Or</para>
    /// <para>fixedRate (exactly one occurrence; of the type xsd:decimal) The calculation period fixed rate. A per annum
    /// rate, expressed as a decimal. A fixed rate of 5% would be represented as 0.05.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: DividendConditions</para>
    ///<para><b>Derived by :</b></para>
    ///<para>• Complex type: DividendConditions</para>
    ///</remarks>
    ///
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestAccrualsMethod))]
    public partial class InterestAccrualsMethod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Rate")]
        public EFS_RadioChoice interestAccrualsMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestAccrualsMethodFloatingRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("floatingRateCalculation", typeof(FloatingRateCalculation),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Floating Rate Calculation", IsVisible = false)]
        public FloatingRateCalculation interestAccrualsMethodFloatingRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestAccrualsMethodFixedRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedRate", typeof(EFS_Decimal),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Fixed Rate", IsVisible = false)]
        [ControlGUI(IsPrimary = false, Name = "value", Regex = EFSRegex.TypeRegex.RegexFixedRateExtend)]
        public EFS_Decimal interestAccrualsMethodFixedRate;
		#endregion Members
	}
    #endregion InterestAccrualsMethod
    #region InterestAccrualsCompoundingMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(InterestAccrualsCompoundingMethod))]
    public class InterestAccrualsCompoundingMethod : InterestAccrualsMethod
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool compoundingMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("compoundingMethod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Compounding Method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CompoundingMethodEnum compoundingMethod;
    }
    #endregion InterestAccrualsCompoundingMethod
    #region IntermediaryInformation
    /// <summary>
    /// <para><b>Description :</b> A type that describes the information to identify an intermediary through which payment 
    /// will be made by the correspondent bank to the ultimate beneficiary of the funds.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>routingIds (exactly one occurrence; of the type RoutingIds) A set of unique identifiers for a party, eachone
    /// identifying the party within a payment system. The assumption is that each party will not have more than one
    /// identifier within the same payment system.</para>
    /// <para>Or</para>
    /// <para>routingExplicitDetails (exactly one occurrence; of the type RoutingExplicitDetails) A set of details that is used
    /// to identify a party involved in the routing of a payment when the party does not have a code that identifies it
    /// within one of the recognized payment systems.</para>
    /// <para>Or</para>
    /// <para>routingIdsAndExplicitDetails (exactly one occurrence; of the type RoutingIdsAndExplicitDetails) A
    /// combination of coded payment system identifiers and details for physical addressing for a party involved in the
    /// routing of a payment.</para>
    /// <para>intermediarySequenceNumber (exactly one occurrence; of the type xsd:integer) A sequence number that
    /// gives the position of the current intermediary in the chain of payment intermediaries. The assumed domain
    /// value set is an ascending sequence of integers starting from 1.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SettlementInstruction</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class IntermediaryInformation
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Routing")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;

		[System.Xml.Serialization.XmlElementAttribute("intermediarySequenceNumber", Order = 4)]
        [ControlGUI(Name = "Intermediary Sequence Number", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_PosInteger intermediarySequenceNumber;
		#endregion Members
    }
    #endregion IntermediaryInformation
    #region Interval
    /// <summary>
    /// <para><b>Description :</b> A type defining a time interval or offset, e.g. one day, three months. 
    /// Used for specifying frequencies at which events occur, the tenor of a floating rate or an offset relative to another date.</para>
    /// <para><b>Contents :</b></para>
    /// <para>periodMultiplier (exactly one occurrence; of the type xsd:integer) A time period multiplier, e.g. 1, 2 or 3 etc. A
    /// negative value can be used when specifying an offset relative to another date, e.g. -2 days. If the period value
    /// is T (Term) then periodMultiplier must contain the value 1.</para>
    /// <para>period (exactly one occurrence; of the type PeriodEnum) A time period, e.g. a day, week, month, year or term
    /// of the stream. If the periodMultiplier value is 0 (zero) then period must contain the value D (day).</para>
    /// <para>currency (exactly one occurance; of the type Currency) The currency in which an amount is denominated.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: CalculationPeriodFrequency</para>
    ///<para>• Complex type: Offset</para>
    ///<para>• Complex type: ResetFrequency</para>
    ///<para>• Complex type: DeliverableObligations</para>
    ///<para>• Complex type: FloatingRate</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: NotionalStepRule</para>
    ///<para>• Complex type: PaymentDates</para>
    ///<para>• Complex type: PeriodicPayment</para>
    ///<para>• Complex type: QuotedAs</para>
    ///<para>• Complex type: ScheduledTerminationDate</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: CalculationPeriodFrequency</para>
    ///<para>• Complex type: Offset</para>
    ///<para>• Complex type: ResetFrequency</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CalculationPeriodFrequency))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Offset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ResetFrequency))]
    public partial class Interval : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("periodMultiplier",Order = 1)]
		[ControlGUI(Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
		[System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "Period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion Interval
    #region IntradocumentReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RateObservation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IntradocumentReference : HrefGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
    }
    #endregion IntradocumentReference

    #region LegalEntity
    /// <summary>
    /// <para><b>Description :</b> A type defining a legal entity.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>entityId (one or more occurrences; of the type EntityId) A legal entity identifier (e.g. RED entity code).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ReferenceInformation</para>
    ///<para>• Complex type: ReferenceObligation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class LegalEntity : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityName", typeof(EntityName),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Name")]
        public EntityName entityName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("entityId",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity Id", IsChild = false)]
        public EntityId[] entityId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Entity)]
        public EFS_Id efs_id;
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
    }
    #endregion LegalEntity
    #region LegalEntityReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ReferenceObligation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class LegalEntityReference : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "value", Width = 200, LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Entity)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
		}
		#endregion Members
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
    }
    #endregion LegalEntityReference

    #region ManualExercise
    /// <summary>
    /// <para><b>Description :</b> A type defining manual exercise, i.e. that the option buyer counterparty must give notice
    /// to the option seller of exercise.</para>
    /// <para><b>Contents :</b></para>
    /// <para>exerciseNotice (zero or one occurrence; of the type ExerciseNotice) Definition of the party to whom notice of
    /// exercise should be given.</para>
    /// <para>fallbackExercise (zero or one occurrence; of the type xsd:boolean) If fallback exercise is specified then the
    /// notional amount of the underlying swap, not previously exercised under the swaption, will be automatically
    /// exercised at the expiration time on the expiration date if at such time the buyer is in-the-money, provided that
    /// the difference between the settlement rate and the fixed rate under the relevant underlying swap is not less
    /// than one tenth of a percentage point (0.10% or 0.001). The term in-the-money is assumed to have the
    /// meaning defined in the 2000 ISDA Definitions, Section 17.4. In-the-money.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ExerciseProcedure</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ManualExercise
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exerciseNoticeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseNotice", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Exercise Notice")]
        public ExerciseNotice exerciseNotice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fallbackExerciseSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fallbackExercise", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "fallbackExercise")]
        public EFS_Boolean fallbackExercise;
		#endregion Members
	}
    #endregion ManualExercise
    #region MasterAgreement
    /// <summary>
    /// <para><b>Description :</b> An entity for defining the agreement executed between the parties and intended to 
    /// govern all OTC derivatives transactions between those parties.</para>
    /// <para><b>Contents :</b></para>
    /// <para>masterAgreementType (exactly one occurrence; of the type MasterAgreementType) The agreement
    /// executed between the parties and intended to govern product-specific derivatives transactions between those
    /// parties.</para>
    /// <para>masterAgreementDate (zero or one occurrence; of the type xsd:date) The date on which the master
    /// agreement was signed.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Documentation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class MasterAgreement : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementType", Order = 1)]
        [ControlGUI(Name = "Type", Width = 150)]
        public MasterAgreementType masterAgreementType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterAgreementDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("masterAgreementDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Master Agreement Date")]
        public EFS_Date masterAgreementDate;
        #endregion Members
        #region Constructors
        public MasterAgreement()
        {
            masterAgreementDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion MasterAgreement
    #region MasterAgreementType
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class MasterAgreementType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string masterAgreementTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public MasterAgreementType()
        {
            masterAgreementTypeScheme = "http://www.fpml.org/spec/2002/master-agreement-type-1-0";
        }
        #endregion Constructors
    }
    #endregion MasterAgreementType
    #region MasterConfirmation
    /// <summary>
    /// <para><b>Description :</b> An entity for defining the master confirmation agreement executed between the parties.</para>
    /// <para><b>Contents :</b></para>
    /// <para>masterConfirmationType (exactly one occurrence; of the type MasterConfirmationType) The type of master
    /// confirmation executed between the parties.</para>
    /// <para>masterConfirmationDate (exactly one occurrence; of the type xsd:date) The date of the confirmation
    /// executed between the parties and intended to govern all relevant transactions between those parties.</para>
    /// <para>masterConfirmationAnnexDate (zero or one occurrence; of the type xsd:date) The date that an annex to the
    /// master confirmation was executed between the parties.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Documentation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MasterConfirmation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("masterConfirmationType", Order = 1)]
		[ControlGUI(Name = "Type", Width = 350)]
        public MasterConfirmationType masterConfirmationType;
		[System.Xml.Serialization.XmlElementAttribute("masterConfirmationDate", Order = 2)]
        [ControlGUI(IsPrimary = false, Name = "Date")]
        public EFS_Date masterConfirmationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool masterConfirmationAnnexDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("masterConfirmationAnnexDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Master Confirmation Annex Date")]
        public EFS_Date masterConfirmationAnnexDate;
		#endregion Members
		#region Constructors
		public MasterConfirmation()
        {
            masterConfirmationAnnexDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion MasterConfirmation
    #region MasterConfirmationType
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: MasterConfirmation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    // EG 20140702 New build FpML4.4 scheme version
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MasterConfirmationType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string masterConfirmationTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public MasterConfirmationType()
        {
            masterConfirmationTypeScheme = "http://www.fpml.org/coding-scheme/master-confirmation-type-5-8";
        }
        #endregion Constructors
    }
    #endregion MasterConfirmationType
    #region MatrixTerm
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MatrixTerm : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string matrixTermScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public MatrixTerm()
        {
            matrixTermScheme = "http://www.fpml.org/coding-scheme/credit-matrix-transaction-type-1-0";
        }
        #endregion Constructors
    }
    #endregion MatrixTerm
    #region MatrixType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MatrixType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string matrixTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public MatrixType()
        {
            matrixTypeScheme = "http://www.fpml.org/coding-scheme/matrix-type-1-0";
        }
        #endregion Constructors
    }
    #endregion MatrixType
    #region Money
    /// <summary>
    /// <para><b>Description :</b> A type defining a currency amount.</para>
    /// <para><b>Contents :</b></para>
    /// <para>currency (exactly one occurrence; of the type Currency) The currency in which an amount is denominated.</para>
    /// <para>amount (exactly one occurrence; of the type xsd:decimal) The monetary quantity in currency units.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXOptionPayout</para>
    ///<para>• Complex type: AdjustedPaymentDates</para>
    ///<para>• Complex type: BasketConstituent</para>
    ///<para>• Complex type: CashSettlementTerms</para>
    ///<para>• Complex type: CreditEvents</para>
    ///<para>• Complex type: EquityLeg</para>
    ///<para>• Complex type: EquityOption</para>
    ///<para>• Complex type: EquityPremium</para>
    ///<para>• Complex type: EquitySwapAdditionalPayment</para>
    ///<para>• Complex type: FailureToPay</para>
    ///<para>• Complex type: FixedAmountCalculation</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para>• Complex type: FXOptionLeg</para>
    ///<para>• Complex type: FXOptionPremium</para>
    ///<para>• Complex type: InterestLeg</para>
    ///<para>• Complex type: Payment</para>
    ///<para>• Complex type: PeriodicPayment</para>
    ///<para>• Complex type: PrincipalExchangeFeatures</para>
    ///<para>• Complex type: ProtectionTerms</para>
    ///<para>• Complex type: QuotablePayment</para>
    ///<para>• Complex type: SinglePayment</para>
    ///<para>• Complex type: SplitSettlement</para>
    ///<para>• Complex type: Stub</para>
    ///<para>• Complex type: TermDeposit</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: FXOptionPayout</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxOptionPayout))]
    public partial class Money : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
		[ControlGUI(IsLabel = false, Name = null, Width = 60)]
        public Currency currency;
		[System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = " ", Width = 150)]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion Money
    #region MultipleExercise
    /// <summary>
    /// <para><b>Description :</b> A type definint multiple exercise. As defining in the 200 ISDA Definitions, 
    /// Section 12.4. Multiple Exercise, the buyer of the option has the right to exercise all or less than all the 
    /// unexercised notional amount of the underlying swap on one or more days in the exercise period, 
    /// but on any such day may not exercise less than the minimum notional amount or more than the maximum notional amount, 
    /// and if an integral multiple amount is specified, the notional exercised must be equal to or, be an integral multiple of, 
    /// the integral multiple amount.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type PartialExercise)</para>
    /// <para>• A type defining partial exercise. As defined in the 2000 ISDA Definitions, Section 12.3 Partial
    /// Exercise, the buyer of the option may exercise all or less than all the notional amount of the
    /// underlying swap but may not be less than the minimum notional amount (if specified) and must
    /// be an integral multiple of the integral multiple amount if specified.</para>
    /// <para>maximumNotionalAmount (zero or one occurrence; of the type xsd:decimal) The maximum notional amount
    /// that can be exercised on a given exercise date.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AmericanExercise</para>
    ///<para>• Complex type: BermudaExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class MultipleExercise : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalReferenceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notionalReference",Order=1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Reference", IsMaster = true)]
        public ExerciseNotionalReference[] notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool integralMultipleAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("integralMultipleAmount", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Integral Multiple Amount")]
        public EFS_Decimal integralMultipleAmount;
		[System.Xml.Serialization.XmlElementAttribute("minimumNotionalAmount", Order = 3)]
		[ControlGUI(Name = "minimumNotionalAmount")]
        public EFS_Decimal minimumNotionalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumNotionalAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("maximumNotionalAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum Notional Amount")]
        public EFS_Decimal maximumNotionalAmount;
		#endregion Members
	}
    #endregion MultipleExercise

    #region NotionalReference
    /// <summary>
    /// <para><b>Description :</b> A pointer style reference to the associated notional schedule defined elsewhere 
    /// in the document.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Schedule)</para>
    /// <para>• A type defining a schedule of rates or amounts in terms of an initial value and then a series of
    /// step date and value pairs. On each step date the rate or amount changes to the new step value.
    /// The series of step date and value pairs are optional. If not specified, this implies that the initial
    /// value remains unchanged over time.</para>
    /// <para>currency (exactly one occurance; of the type Currency) The currency in which an amount is denominated.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ExerciseFee</para>
    ///<para>• Complex type: ExerciseFeeSchedule</para>
    ///<para>• Complex type: FxLinkedNotionalSchedule</para>
    ///<para>• Complex type: PartialExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class NotionalReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
    }
    #endregion NotionalReference

    #region Offset
    /// <summary>
    /// <para><b>Description :</b> A type defining an offset used in calculating a new date relative to a reference date. 
    /// Currently, the only offsets defined are expected to be expressed as either calendar or business day offsets.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Interval)</para>
    /// <para>• A type defining a time interval or offset, e.g. one day, three months. Used for specifying
    /// frequencies at which events occur, the tenor of a floating rate or an offset relative to another
    /// date.</para>
    /// <para>dayType (zero or one occurrence; of the type DayTypeEnum) In the case of an offset specified as a number
    /// of days, this element defines whether consideration is given as to whether a day is a good business day or
    /// not. If a day type of business days is specified then non-business days are ignored when calculating the offset.
    /// The financial business centers to use for determination of business days are implied by the context in which
    /// this element is used. This element must only be included when the offset is specified as a number of days. If
    /// the offset is zero days then the dayType element should not be included.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RelativeDateOffset</para>
    ///<para>• Complex type: GracePeriodExtension</para>
    ///<para>• Complex type: PaymentDates</para>
    ///<para>• Complex type: ResetDates</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: RelativeDateOffset</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDateOffset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    public partial class Offset : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
		[ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
		[System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [ControlGUI(Name = "dayType")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
		#endregion Members
    }
    #endregion Offset

    #region PartialExercise
    /// <summary>
    /// <para><b>Description :</b> A type defining partial exercise. As defined in the 2000 ISDA Definitions, 
    /// Section 12.3 Partial Exercise, the buyer of the option may exercise all or less than all the notional amount 
    /// of the underlying swap but may not be less than the minimum notional amount (if specified) and 
    /// must be an integral multiple of the integral multiple amount if specified.</para>
    /// <para><b>Contents :</b></para>
    /// <para>notionalReference (one or more occurrences; of the type NotionalReference)</para>
    /// <para>integralMultipleAmount (zero or one occurrence; of the type xsd:decimal) A notional amount which restricts
    /// the amount of notional that can be exercised when partial exercise or multiple exercise is applicable. The
    /// integral multiple amount defines a lower limit of notional that can be exercised and also defines a unit multiple
    /// of notional that can be exercised, i.e. only integer multiples of this amount can be exercised.</para>
    /// <para>minimumNotionalAmount (exactly one occurrence; of the type xsd:decimal) The minimum notional amount
    /// that can be exercised on a given exercise date. See multipleExercise.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: MultipleExercise</para>
    ///<para>• Complex type: EuropeanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: MultipleExercise</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MultipleExercise))]
    public partial class PartialExercise : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalReference", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Notional Reference", IsMaster = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public ExerciseNotionalReference[] notionalReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool integralMultipleAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("integralMultipleAmount", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Integral Multiple Amount")]
        public EFS_Decimal integralMultipleAmount;
		[System.Xml.Serialization.XmlElementAttribute("minimumNotionalAmount", Order = 3)]
		[ControlGUI(Name = "minimumNotionalAmount")]
        public EFS_Decimal minimumNotionalAmount;
		#endregion Members
	}
    #endregion PartialExercise
    #region PartyReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: CalculationAgent</para>
    ///<para>• Complex type: EquitySwapEarlyTerminationType</para>
    ///<para>• Complex type: ExerciseNotice</para>
    ///<para>• Complex type: FXLeg</para>
    ///<para>• Complex type: GeneralTerms</para>
    ///<para>• Complex type: MandatoryEarlyTermination</para>
    ///<para>• Complex type: NotifyingParty</para>
    ///<para>• Complex type: PartyPortfolioName</para>
    ///<para>• Complex type: Swaption</para>
    ///<para>• Complex type: TermDeposit</para>
    ///<para>• Complex type: TradeIdentifier</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PartyReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
    }
    #endregion PartyReference
    #region PeriodicDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PeriodicDates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("calculationStartDate", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Start Date", IsVisible = false)]
        public AdjustableOrRelativeDate calculationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("calculationEndDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Start Date")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation End Date")]
        public AdjustableOrRelativeDate calculationEndDate;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 3)]
        public CalculationPeriodFrequency calculationPeriodFrequency;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodDatesAdjustments", Order = 4)]
        public BusinessDayAdjustments calculationPeriodDatesAdjustments;
		#endregion Members
    }
    #endregion PeriodicDates
    #region Payment
    /// <summary>
    /// <para><b>Description :</b> A type for defining payments</para>
    /// <para><b>Contents :</b></para>
    /// <para>payerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party
    /// responsible for making the payments defined by this structure.</para>
    /// <para>receiverPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that
    /// receives the payments corresponding to this structure.</para>
    /// <para>paymentAmount (exactly one occurrence; of the type Money) The currency amount of the payment.</para>
    /// <para>paymentDate (zero or one occurrence; of the type AdjustableDate) The payment date. This date is subject to
    /// adjustment in accordance with any applicable business day convention.</para>
    /// <para>adjustedPaymentDate (zero or one occurrence; of the type xsd:date) The adjusted payment date. This date
    /// should already be adjusted for any applicable business day convention. This component is not intended for
    /// use in trade confirmation but my be specified to allow the fee structure to also serve as a cashflow type
    /// component (all dates the the Cashflows type are adjusted payment dates).</para>	
    /// <para>paymentType (zero or one occurrence; with locally defined content) A classification of the type of fee or
    /// additional payment, e.g. brokerage, upfront fee etc. FpML does not define domain values for this element.</para>
    /// <para>settlementInformation (zero or one occurrence; of the type SettlementInformation) The information required
    /// to settle a currency payment that results from a trade.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: BulletPayment</para>
    ///<para>• Complex type: CapFloor</para>
    ///<para>• Complex type: FXLeg</para>
    ///<para>• Complex type: Swap</para>
    ///<para>• Complex type: Swaption</para>
    ///<para>• Complex type: TermDeposit</para>
    ///<para>• Complex type: Trade</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class Payment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After, IsCopyPaste = true)]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        public AdjustableDate paymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentType", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment type")]
        [ControlGUI(IsPrimary = false, IsLabel = false, Name = "value", Width = 200)]
        public PaymentType paymentType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementInformation", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement information")]
        public SettlementInformation settlementInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool discountFactorSpecified;
		[System.Xml.Serialization.XmlElementAttribute("discountFactor", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Discount factor")]
        public EFS_Decimal discountFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool presentValueAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("presentValueAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Present value", IsCopyPaste = true)]
        public Money presentValueAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool efs_hrefSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference", Width = 200)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Formula)]
        public EFS_Href efs_href;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set { efs_href = new EFS_Href(value); }
            get
            {
                if (efs_href == null)
                    return null;
                else
                    return efs_href.Value;
            }
        }
		#endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool paymentQuoteSpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentQuote", Order = 10)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension", IsVisible = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Percentage")]
		public PaymentQuote paymentQuote;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool customerSettlementPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("customerSettlementPayment", Order = 11)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer settlement payment")]
		public CustomerSettlementPayment customerSettlementPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentSource", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Source")]
        public SpheresSource paymentSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension")]
		public bool FillBalise;
		#endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		public EFS_Payment efs_Payment;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:Payment";
		#endregion Members
    }
    #endregion Payment
    #region PaymentCurrency
    public partial class PaymentCurrency : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice paymentCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyReferenceSpecified;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 300)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Href paymentCurrencyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", typeof(Currency),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 75)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Currency", IsVisible = true)]
        public Currency paymentCurrencyCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentCurrencyDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 300)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        public EFS_MultiLineString paymentCurrencyDeterminationMethod;

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

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href
        {
            set { paymentCurrencyReference = new EFS_Href(value); }
            get
            {
                if (paymentCurrencyReference == null)
                    return null;
                else
                    return paymentCurrencyReference.Value;
            }
        }

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Currency)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion PaymentCurrency
    #region PaymentType
    /// <summary>
    /// <para><b>Description :</b> Inherited element(s): (This definition restricts the content defined by the type xsd:string)</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: EquitySwapAdditionalPayment</para>
    ///<para>• Complex type: Payment</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PaymentType : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string paymentTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
	}
    #endregion PaymentType
    #region PrincipalExchanges
    /// <summary>
    /// <para><b>Description :</b> A type defining which principal exchanges occur for the stream.</para>
    /// <para><b>Contents :</b></para>
    /// <para>initialExchange (exactly one occurrence; of the type xsd:boolean) A true/false flag to indicate whether there is
    /// an initial exchange of principal on the effective date.</para>
    /// <para>finalExchange (exactly one occurrence; of the type xsd:boolean) A true/false flag to indicate whether there is
    /// a final exchange of principal on the termination date.</para>
    /// <para>intermediateExchange (exactly one occurrence; of the type xsd:boolean) A true/false flag to indicate whether
    /// there are intermediate or interim exchanges of principal during the term of the swap.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: InterestRateStream</para>
    ///<para>• Complex type: PrincipalExchangeFeatures</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PrincipalExchanges : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("initialExchange", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Initial Exchange")]
        public EFS_Boolean initialExchange;
		[System.Xml.Serialization.XmlElementAttribute("finalExchange", Order = 2)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Final Exchange")]
        public EFS_Boolean finalExchange;
		[System.Xml.Serialization.XmlElementAttribute("intermediateExchange", Order = 3)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Intermediate Exchange")]
        public EFS_Boolean intermediateExchange;
		#endregion Members
	}
    #endregion PrincipalExchanges
    #region Product
    /// <summary>
    /// <para><b>Description :</b> The base type which all FpML products extend.</para>
    /// <para><b>Contents :</b></para>
    /// <para>productType (zero or one occurrence; of the type ProductType) A classification of the type of product. FpML
    /// does not define domain values for this element.</para>
    /// <para>productId (zero or more occurrences; of the type ProductId) A product reference identifier allocated by a
    /// party. FpML does not define the domain values associated with this element. Note that the domain values for
    /// this element are not strictly an enumerated list.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: product</para>
    ///<para>• Complex type: BulletPayment</para>
    ///<para>• Complex type: CapFloor</para>
    ///<para>• Complex type: CreditDefaultSwap</para>
    ///<para>• Complex type: EquityOption</para>
    ///<para>• Complex type: EquitySwap</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para>• Complex type: FXDigitalOption</para>
    ///<para>• Complex type: FXLeg</para>
    ///<para>• Complex type: FXOptionLeg</para>
    ///<para>• Complex type: FXSwap</para>
    ///<para>• Complex type: Strategy</para>
    ///<para>• Complex type: Swap</para>
    ///<para>• Complex type: Swaption</para>
    ///<para>• Complex type: TermDeposit</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: BulletPayment</para>
    ///<para>• Complex type: CapFloor</para>
    ///<para>• Complex type: CreditDefaultSwap</para>
    ///<para>• Complex type: EquityOption</para>
    ///<para>• Complex type: EquitySwap</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para>• Complex type: FXDigitalOption</para>
    ///<para>• Complex type: FXLeg</para>
    ///<para>• Complex type: FXOptionLeg</para>
    ///<para>• Complex type: FXSwap</para>
    ///<para>• Complex type: Strategy</para>
    ///<para>• Complex type: Swap</para>
    ///<para>• Complex type: Swaption</para>
    ///<para>• Complex type: TermDeposit</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BrokerEquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BulletPayment))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CapFloor))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditDefaultSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeLongFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityDerivativeShortFormBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityForward))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityOptionTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapBase))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquitySwapTransactionSupplement))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Fra))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxAverageRateOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxBarrierOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxDigitalOption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxOptionLeg))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Strategy))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Swap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Swaption))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(TermDeposit))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EfsML.v20.FIXML))]
    public partial class Product
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("productType", Order = 1)]
        public ProductType productType;
        [System.Xml.Serialization.XmlElementAttribute("productId",Order=2)]
        public ProductId[] productId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLocked = true,LineFeed=MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Product)]
        public EFS_Id efs_id;
		
        #region id
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
		#endregion id
        #endregion Members
    }
    #endregion Product
    #region ProductId
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Product</para>
    ///<para>• Complex type: QuotableProduct</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("quotableFxSingleLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public class ProductId
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string productIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
		#endregion Members
	}
    #endregion ProductId
    #region ProductReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Strategy</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    ///
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ProductReference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
	}
    #endregion ProductReference
    #region ProductType
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Product</para>
    ///<para>• Complex type: QuotableProduct</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class ProductType
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string productTypeScheme;
        /// <summary>
        /// Non de l'instrument
        /// </summary>
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        /// <summary>
        /// idI
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;
		#endregion Members
    }
    #endregion ProductType

    #region QuotedCurrencyPair
    /// <summary>
    /// <para><b>Description :</b> A type that describes the composition of a rate that has been quoted or is to be quoted. 
    /// This includes the two currencies and the quotation relationship between the two currencies and is used as a building 
    /// block throughout the FX specification.</para>
    /// <para><b>Contents :</b></para>
    /// <para>currency1 (exactly one occurrence; of the type Currency) The first currency specified when a pair of
    /// currencies is to be evaluated.</para>
    /// <para>currency2 (exactly one occurrence; of the type Currency) The second currency specified when a pair of
    /// currencies is to be evaluated.</para>
    /// <para>quoteBasis (exactly one occurrence; of the type QuoteBasisEnum) The method by which the exchange rate
    /// is quoted.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxAmericanTrigger</para>
    ///<para>• Complex type: FxBarrier</para>
    ///<para>• Complex type: FxDigitalOption</para>
    ///<para>• Complex type: FxEuropeanTrigger</para>
    ///<para>• Complex type: FxFixing</para>
    ///<para>• Complex type: FxRate</para>
    ///<para>• Complex type: QuotableFxRate</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class QuotedCurrencyPair : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency1", Order = 1)]
		[ControlGUI(Name = "currency1", LblWidth = 80, Width = 75)]
        public Currency currency1;
		[System.Xml.Serialization.XmlElementAttribute("currency2", Order = 2)]
		[ControlGUI(Name = "currency2", LblWidth = 80, Width = 75)]
        public Currency currency2;
		[System.Xml.Serialization.XmlElementAttribute("quoteBasis", Order = 3)]
		[ControlGUI(Name = "quoteBasis", LblWidth = 80, Width = 190, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public QuoteBasisEnum quoteBasis;
		#endregion Members
    }
    #endregion QuotedCurrencyPair

    #region RateObservation
    /// <summary>
    /// <para><b>Description :</b> A type defining parameters associated with an individual observation or fixing. 
    /// This type forms part of the cashflow representation of a stream.</para>
    /// <para><b>Contents :</b></para>
    /// <para>observedRate (zero or one occurrence; of the type xsd:decimal) The actual observed rate before any required
    /// rate treatment is applied, e.g. before converting a rate quoted on a discount basis to an equivalent yield. An
    /// observed rate of 5% would be represented as 0.05.</para>
    /// <para>treatedRate (zero or one occurrence; of the type xsd:decimal) The observed rate after any required rate
    /// treatment is applied. A treated rate of 5% would be represented as 0.05.</para>
    /// <para>observationWeight (exactly one occurrence; of the type xsd:positiveInteger) The number of days weighting to
    /// be associated with the rate observation, i.e. the number of days such rate is in effect. This is applicable in the
    /// case of a weighted average method of calculation where more than one reset date is established for a single
    /// calculation period.</para>
    /// <para>rateReference (zero or one occurrence; of the type IntradocumentReference) A pointer style reference to a
    /// floating rate component defined as part of a stub calculation period amount component. It is only required
    /// when it is necessary to distinguish two rate observations for the same fixing date which could occur when
    /// linear interpolation of two different rates occurs for a stub calculation period.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRateDefinition</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    ///
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RateObservation : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool resetDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resetDate",Order=1)]
        public EFS_Date resetDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adjustedFixingDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedFixingDate", Order = 2)]
		public EFS_Date adjustedFixingDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool observedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observedRate", Order = 3)]
        public EFS_Decimal observedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool treatedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("treatedRate", Order = 4)]
		public EFS_Decimal treatedRate;
		[System.Xml.Serialization.XmlElementAttribute("observationWeight", Order = 5)]
        public EFS_PosInteger observationWeight;
		[System.Xml.Serialization.XmlElementAttribute("rateReference", Order = 6)]
		public IntradocumentReference rateReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forecastRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("forecastRate", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Forecast Rate")]
        public EFS_Decimal forecastRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool treatedForecastRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("treatedForecastRate", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Treated Forecast Rate")]
        public EFS_Decimal treatedForecastRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
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
    }
    #endregion RateObservation
    #region RateSourcePage
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition restricts the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: InformationSource</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RateSourcePage : SchemeTextGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string rateSourcePageScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion RateSourcePage
    #region Reference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Reference : HrefGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
	}
    #endregion Reference
    #region ReferenceAmount
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ReferenceAmount : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue referenceAmount;
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string referenceAmountScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(Name = "value", Width = 150)]
        public string Value;
		#endregion Members
	}
    #endregion ReferenceAmount
    #region ReferenceBank
    /// <summary>
    /// <para><b>Description :</b> A type to describe an institution (party) identified by means of a coding scheme and an optional name.</para>
    /// <para><b>Contents :</b></para>
    /// <para>referenceBankId (exactly one occurrence; with locally defined content) An institution (party) identifier, e.g. a
    /// bank identifier code (BIC).</para>
    /// <para>referenceBankName (zero or one occurrence; of the type xsd:string) The name of the institution (party). A
    /// free format string. FpML does not define usage rules for the element.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: CashSettlementReferenceBanks</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferenceBank : ReferenceBankGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("referenceBankId", Order = 1)]
		[ControlGUI(Name = "referenceBank", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public ReferenceBankId referenceBankId;
		[System.Xml.Serialization.XmlElementAttribute("referenceBankName", Order = 2)]
        public string referenceBankName;
		#endregion Members
		#region Constructors
		public ReferenceBank()
        {
            referenceBankId = new ReferenceBankId();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #region _referenceBankId
        public static object _referenceBankId(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ReferenceBankControl(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion _referenceBankId
        #endregion Methods
    }
    #endregion ReferenceBank
    #region ReferenceBankId
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition restricts the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ReferenceBank</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferenceBankId
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string referenceBankIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
	}
    #endregion ReferenceBankId
    #region RelativeDateOffset
    /// <summary>
    /// <para><b>Description :</b> A type defining a date (referred to as the derived date) as a relative offset 
    /// from another date (referred to as the anchor date). If the anchor date is itself an adjustable date then the 
    /// offset is assumed to be calculated from the adjusted anchor date. A number of different scenarios can be supported, 
    /// namely; 1) the derived date my simply be a number of calendar periods (days, weeks, months or years) 
    /// preceding or following the anchor date; 2) the unadjusted derived date may be a number of calendar 
    /// periods (days, weeks, months or years) preceding or following the anchor date with the resulting unadjusted derived 
    /// date subject to adjustment in accordance with a specified business day convention, i.e. the derived date must 
    /// fall on a good business day; 3) the derived date may be a number of business days preceding or following the anchor date. 
    /// Note that the businessDayConvention specifies any required adjustment to the unadjusted derived date. A negative or
    /// positive value in the periodMultiplier indicates whether the unadjusted derived precedes or follows the anchor
    /// date. The businessDayConvention should contain a value NONE if the day type element contains a value of
    /// Business (since specifying a negative or positive business days offset would already guarantee that the
    /// derived date would fall on a good business day in the specified business centers).</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Offset)</para>
    /// <para>• A type defining an offset used in calculating a new date relative to a reference date. Currently,
    /// the only offsets defined are expected to be expressed as either calendar or business day offsets.</para>
    /// <para>businessDayConvention (exactly one occurrence; of the type BusinessDayConventionEnum) The
    ///convention for adjusting a date if it would otherwise fall on a day that is not a business day.</para>
    /// <para>dateRelativeTo (exactly one occurrence; of the type DateRelativeTo)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RelativeDates</para>
    ///<para>• Complex type: AdjustableOrRelativeDate</para>
    ///<para>• Complex type: CashSettlement</para>
    ///<para>• Complex type: CashSettlementPaymentDate</para>
    ///<para>• Complex type: EquityLeg</para>
    ///<para>• Complex type: EquitySwapAdditionalPayment</para>
    ///<para>• Complex type: ExerciseFee</para>
    ///<para>• Complex type: ExerciseFeeSchedule</para>
    ///<para>• Complex type: Fra</para>
    ///<para>• Complex type: FxLinkedNotionalSchedule</para>
    ///<para>• Complex type: ResetDates</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: RelativeDates</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RelativeDates))]
    public partial class RelativeDateOffset : ItemGUI
	{
		#region Members
		// Interval
		[System.Xml.Serialization.XmlElementAttribute("periodMultiplier", Order = 1)]
        [ControlGUI(Name = "multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer periodMultiplier;
		[System.Xml.Serialization.XmlElementAttribute("period", Order = 2)]
        [ControlGUI(Name = "period", Width = 70)]
        public PeriodEnum period;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;

        // Offset
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dayType", Order = 3)]
        [ControlGUI(Name = "dayType", LineFeed = MethodsGUI.LineFeedEnum.After)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public DayTypeEnum dayType;
        // Convention
        [ControlGUI(Name = "convention")]
		[System.Xml.Serialization.XmlElementAttribute("businessDayConvention", Order = 4)]
        public BusinessDayConventionEnum businessDayConvention;
        // Business Centers Choice
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty businessCentersNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 200)]
        public BusinessCentersReference businessCentersReference;

		[System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", Order = 7)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "dateRelativeTo", Width = 210)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateRelativeTo dateRelativeTo;
		#endregion Members
    }
    #endregion RelativeDateOffset
    #region RelativeDates
    /// <summary>
    /// <para><b>Description :</b> A type describing a set of dates defined as relative to another set of dates.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type RelativeDateOffset)</para>
    /// <para>• A type defining a date (referred to as the derived date) as a relative offset from another date
    /// (referred to as the anchor date). If the anchor date is itself an adjustable date then the offset is
    /// assumed to be calculated from the adjusted anchor date. A number of different scenarios can be
    /// supported, namely; 1) the derived date my simply be a number of calendar periods (days,
    /// weeks, months or years) preceding or following the anchor date; 2) the unadjusted derived date
    /// may be a number of calendar periods(days, weeks, months or years) preceding or following the
    /// anchor date with the resulting unadjusted derived date subject to adjustment in accordance with
    /// a specified business day convention, i.e. the derived date must fall on a good business day; 3)
    /// the derived date may be a number of business days preceding or following the anchor date. Note
    /// that the businessDayConvention specifies any required adjustment to the unadjusted derived
    /// date. A negative or positive value in the periodMultiplier indicates whether the unadjusted
    /// derived precedes or follows the anchor date. The businessDayConvention should contain a
    /// value NONE if the day type element contains a value of Business (since specifying a negative or
    /// positive business days offset would already guarantee that the derived date would fall on a good
    /// business day in the specified business centers).</para>
    /// <para>periodSkip (zero or one occurrence; of the type xsd:positiveInteger) The number of periods in the referenced
    /// date schedule that are between each date in the relative date schedule. Thus a skip of 2 would mean that
    /// dates are relative to every second date in the referenced schedule. If present this should have a value greater
    /// than 1.</para>
    /// <para>scheduleBounds (zero or one occurrence; of the type DateRange) The first and last dates of a schedule. This
    /// can be used to restrict the range of values in a reference series of dates.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableOrRelativeDates</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RelativeDates : RelativeDateOffset
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodSkipSpecified;
		[System.Xml.Serialization.XmlElementAttribute("periodSkip", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "periodSkip", Width = 70)]
        public EFS_PosInteger periodSkip;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduleBoundsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("periodSkip", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "scheduleBounds")]
        public DateRange scheduleBounds;
		#endregion Members
	}
    #endregion RelativeDates
    #region RelativeDateSequence
    /// <summary>
    /// <para><b>Description :</b> A type describing a date when this date is defined in reference to another date through 
    /// one or several date
    /// offsets.</para>
    /// <para><b>Contents :</b></para>
    /// <para>dateRelativeTo (exactly one occurrence; of the type DateRelativeTo)</para>
    /// <para>dateOffset (one or more occurrences; with locally defined content)</para>
    /// <para>Either</para>
    /// <para>businessCentersReference (exactly one occurrence; of the type BusinessCentersReference)</para>
    /// <para>Or</para>
    /// <para>businessCenters (exactly one occurrence; of the type BusinessCenters)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AdjustableRelativeOrPeriodicDates</para>
    ///<para>• Complex type: EquitySwapValuation</para>
    ///<para><b>Derived by :</b></para>
    ///</remarks>
    // EG 20140702 Upd
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RelativeDateSequence : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("dateRelativeTo", Order = 1)]
		[ControlGUI(Name = "dateRelativeTo", Width = 210)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public DateRelativeTo dateRelativeTo;
        [System.Xml.Serialization.XmlElementAttribute("dateOffset",Order=2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Date Offset", IsClonable = true, IsMaster = true, IsMasterVisible = true)]
        public DateOffset[] dateOffset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Business Centers")]
        public EFS_RadioChoice businessCenters;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenters", typeof(BusinessCenters),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public BusinessCenters businessCentersDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessCentersReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCentersReference", typeof(BusinessCentersReference),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public BusinessCentersReference businessCentersReference;
		#endregion Members
	}
    #endregion RelativeDateSequence
    #region RequiredIdentifierDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RequiredIdentifierDate : DateCalendarGUI
	{
		#region Members
		[System.Xml.Serialization.XmlTextAttribute()]
        [ControlGUI(IsLabel = true, Name = "value")]
        public string Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion RequiredIdentifierDate
    #region ResetFrequency
    /// <summary>
    /// <para><b>Description :</b> A type defining the reset frequency. In the case of a weekly reset, 
    /// also specifies the day of the week that the reset occurs. If the reset frequency is greater than the calculation 
    /// period frequency the this implies that more or more reset dates is established for each calculation period 
    /// and some form of rate averaginhg is applicable.
    /// The specific averaging method of calculation is specified in FloatingRateCalculation.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Interval)</para>
    /// <para>• A type defining a time interval or offset, e.g. one day, three months. Used for specifying
    /// frequencies at which events occur, the tenor of a floating rate or an offset relative to another
    /// date.</para>
    /// <para>weeklyRollConvention (zero or one occurrence; of the type WeeklyRollConventionEnum) The day of the
    /// week on which a weekly reset date occurs. This element must be included if the reset frequency is defined as
    /// weekly and not otherwise.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: InterestLeg</para>
    ///<para>• Complex type: ResetDates</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ResetFrequency : Interval
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool weeklyRollConventionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("weeklyRollConvention", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "WeeklyRoll", Width = 150)]
        public WeeklyRollConventionEnum weeklyRollConvention;
		#endregion Members
    }
    #endregion ResetFrequency
    #region Rounding
    /// <summary>
    /// <para><b>Description :</b> A type defining a rounding direction and precision to be used in the rounding of a rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>roundingDirection (exactly one occurrence; of the type RoundingDirectionEnum) Specifies the rounding
    /// direction.</para>
    /// <para>precision (exactly one occurrence; of the type xsd:nonNegativeInteger) Specifies the rounding precision in
    /// terms of a number of decimal places. Note how a percentage rate rounding of 5 decimal places is expressed
    /// as a rounding precision of 7 in the FpML document since the percentage is expressed as a decimal, e.g.
    /// 9.876543% (or 0.09876543) being rounded to the nearest 5 decimal places is 9.87654% (or 0.0987654).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRateCalculation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Rounding : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("roundingDirection", Order = 1)]
		[ControlGUI(Name = "direction", Width = 80)]
        public RoundingDirectionEnum roundingDirection;
		[System.Xml.Serialization.XmlElementAttribute("precision", Order = 2)]
        [ControlGUI(Name = "precision", LblWidth = 62, Width = 25)]
        public EFS_PosInteger precision;
		#endregion Members
    }
    #endregion Rounding
    #region Routing
    /// <summary>
    /// <para><b>Description :</b> A type that provides three alternative ways of identifying a party involved 
    /// in the routing of a payment. The identification may use payment system identifiers only; actual name, 
    /// address and other reference information; or a combination of both.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>routingIds (exactly one occurrence; of the type RoutingIds) A set of unique identifiers for a party, eachone
    /// identifying the party within a payment system. The assumption is that each party will not have more than one
    /// identifier within the same payment system.</para>
    /// <para>Or</para>
    /// <para>routingExplicitDetails (exactly one occurrence; of the type RoutingExplicitDetails) A set of details that is used
    /// to identify a party involved in the routing of a payment when the party does not have a code that identifies it
    /// within one of the recognized payment systems.</para>
    /// <para>Or</para>
    /// <para>routingIdsAndExplicitDetails (exactly one occurrence; of the type RoutingIdsAndExplicitDetails) A
    /// combination of coded payment system identifiers and details for physical addressing for a party involved in the
    /// routing of a payment.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SettlementInstruction</para>
    ///<para>• Complex type: SplitSettlement</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class Routing : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Rounting")]
        public EFS_RadioChoice routing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Routing Ids", IsVisible = true)]
        public RoutingIds routingIds;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingExplicitDetails", typeof(RoutingExplicitDetails),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Explicit Details", IsVisible = true)]
        public RoutingExplicitDetails routingExplicitDetails;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingIdsAndExplicitDetailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingIdsAndExplicitDetails", typeof(RoutingIdsAndExplicitDetails),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Ids And Explicit Details", IsVisible = true)]
        public RoutingIdsAndExplicitDetails routingIdsAndExplicitDetails;
		#endregion Members
    }
    #endregion Routing
    #region RoutingExplicitDetails
    /// <summary>
    /// <para><b>Description :</b> A type that models name, address and supplementary textual information for the purposes 
    /// of identifying a party involved in the routing of a payment.</para>
    /// <para><b>Contents :</b></para>
    /// <para>routingName (exactly one occurrence; of the type xsd:string) A real name that is used to identify a party
    /// involved in the routing of a payment.</para>
    /// <para>routingAddress (zero or one occurrence; of the type Address) A physical postal address via which a payment
    /// can be routed.</para>
    /// <para>routingAccountNumber (zero or one occurrence; of the type xsd:string) An account number via which a
    /// payment can be routed.</para>
    /// <para>routingReferenceText (zero or more occurrences; of the type xsd:string) A piece of free-format text used to
    /// assist the identification of a party involved in the routing of a payment.</para>
    /// </summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: IntermediaryInformation</para>
    ///<para>• Complex type: Routing</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RoutingExplicitDetails
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("routingName", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Name")]
        public EFS_String routingName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAddressSpecified;
		[System.Xml.Serialization.XmlElementAttribute("routingAddress", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Address")]
        public Address routingAddress;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAccountNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("routingAccountNumber", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Number")]
        public EFS_String routingAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingReferenceTextSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingReferenceText",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text", IsMaster = true)]
        public EFS_StringArray[] routingReferenceText;
		#endregion Members
    }
    #endregion RoutingExplicitDetails
    #region RoutingId
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: RoutingIds</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RoutingId : IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string routingIdCodeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = "value", LblWidth = 50, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public string Value;
        #endregion Members
    }
    #endregion RoutingId
    #region RoutingIds
    /// <summary>
    /// <para><b>Description :</b> A type that provides for identifying a party involved in the routing of a payment 
    /// by means of one or more standard identification codes. For example, both a SWIFT BIC code and a national bank 
    /// identifier may be required.</para>
    /// <para><b>Contents :</b></para>
    /// <para>routingId (one or more occurrences; of the type RoutingId) A unique identifier for party that is a participant in
    /// a recognized payment system.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: IntermediaryInformation</para>
    ///<para>• Complex type: Routing</para>
    ///<para>• Complex type: RoutingIdsAndExplicitDetails</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RoutingIds : IEFS_Array
    {
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("routingId",Order=1)]
        [ControlGUI(Name = "Routing Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Routing Id", IsMaster = true, IsMasterVisible = true)]
        public RoutingId[] routingId;
		#endregion Members
    }
    #endregion RoutingIds
    #region RoutingIdsAndExplicitDetails
    /// <summary>
    /// <para><b>Description :</b> A type that provides a combination of payment system identification codes with physical postal address
    /// details, for the purposes of identifying a party involved in the routing of a payment.</para>
    /// <para><b>Contents :</b></para>
    /// <para>routingIds (one or more occurrences; of the type RoutingIds) A set of unique identifiers for a party, eachone
    /// identifying the party within a payment system. The assumption is that each party will not have more than one
    /// identifier within the same payment system.</para>
    /// <para>routingName (exactly one occurrence; of the type xsd:string) A real name that is used to identify a party
    /// involved in the routing of a payment.</para>
    /// <para>routingAddress (zero or one occurrence; of the type Address) A physical postal address via which a payment
    /// can be routed.</para>
    ///<para>routingAccountNumber (zero or one occurrence; of the type xsd:string) An account number via which a
    /// payment can be routed.</para>
    /// <para>routingReferenceText (zero or more occurrences; of the type xsd:string) A piece of free-format text used to
    /// assist the identification of a party involved in the routing of a payment.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: IntermediaryInformation</para>
    ///<para>• Complex type: Routing</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class RoutingIdsAndExplicitDetails
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("routingIds", typeof(RoutingIds),Order=1)]
        [ControlGUI(Name = "Routing Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Routing Ids", IsMaster = true)]
        public RoutingIds[] routingIds;
		[System.Xml.Serialization.XmlElementAttribute("routingName", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Explicit Details", IsVisible = false, IsGroup = true)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Name")]
        public EFS_String routingName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAddressSpecified;
		[System.Xml.Serialization.XmlElementAttribute("routingAddress", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Address")]
        public Address routingAddress;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingAccountNumberSpecified;
		[System.Xml.Serialization.XmlElementAttribute("routingAccountNumber", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Account Number")]
        public EFS_String routingAccountNumber;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool routingReferenceTextSpecified;
        [System.Xml.Serialization.XmlElementAttribute("routingReferenceText",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference Text", IsMaster = true)]
        public EFS_StringArray[] routingReferenceText;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Explicit Details")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
    }
    #endregion RoutingIdsAndExplicitDetails

    #region Schedule
    /// <summary>
    /// <para><b>Description :</b> A type defining a schedule of rates or amounts in terms of an initial value and then a 
    /// series of step date and value pairs. On each step date the rate or amount changes to the new step value. 
    /// The series of step date and value pairs are optional. If not specified, this implies that the initial value remains 
    /// unchanged over time.</para>
    /// <para><b>Contents :</b></para>
    /// <para>initialValue (exactly one occurrence; of the type xsd:decimal) The initial rate or amount, as the case may be.
    /// An initial rate of 5% would be represented as 0.05.</para>
    /// <para>step (zero or more occurrences; of the type Step) The schedule of step date and value pairs. On each step
    /// date the associated step value becomes effective A list of steps may be ordered in the document by ascending
    /// step date. An FpML document containing an unordered list of steps is still regarded as a conformant
    /// document.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: AmountSchedule</para>
    ///<para>• Complex type: StrikeSchedule</para>
    ///<para>• Complex type: Calculation</para>
    ///<para>• Complex type: ExerciseFeeSchedule</para>
    ///<para>• Complex type: FloatingRate</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: AmountSchedule</para>
    ///<para>• Complex type: StrikeSchedule</para>
    ///</remarks>
    /// <revision>
    ///     <version>1.2.0</version><date>20071029</date><author>EG</author>
    ///     <comment>Ticket 15889
    ///     Step dates: Unajusted versus Ajusted
    ///     Add public member : efs_Steps
    ///     </comment>
    /// </revision>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AmountSchedule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(StrikeSchedule))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PayOutAmountSchedule))]
    public partial class Schedule : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("initialValue", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Initial Value")]
        public EFS_Decimal initialValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stepSpecified;
        [System.Xml.Serialization.XmlElementAttribute("step",Order=2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Step", IsCopyPaste = true)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Step", IsClonable = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public Step[] step;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        [ControlGUI(IsLabel = false, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Id efs_id;
		#endregion Members
    }

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public class PayOutAmountSchedule : Schedule
    {
    }
    #endregion Schedule
    #region SettlementInformation
    /// <summary>
    /// <para><b>Description :</b> A type that represents the choice of methods for settling a potential currency 
    /// payment resulting from a trade:
    /// by means of a standard settlement instruction, by netting it out with other payments, or with an explicit
    /// settlement instruction.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Either</para>
    /// <para>standardSettlementStyle (exactly one occurrence; of the type StandardSettlementStyleEnum) An optional
    /// element used to describe how a trade will settle. This defines a scheme and is used for identifying trades that
    /// are identified as settling standard and/or flagged for settlement netting.</para>
    /// <para>Either</para>
    /// <para>settlementInstruction (exactly one occurrence; of the type SettlementInstruction) An explicit specification of
    /// how a currency payment is to be made, when the payment is not netted and the route is other than the
    /// recipient's standard settlement instruction.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXOptionPayout</para>
    ///<para>• Complex type: FXOptionPremium</para>
    ///<para>• Complex type: Payment</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SettlementInformation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public EFS_RadioChoice information;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationStandardSpecified;
        [System.Xml.Serialization.XmlElementAttribute("standardSettlementStyle", typeof(StandardSettlementStyleEnum),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "Style")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Standard", IsVisible = true)]
        public StandardSettlementStyleEnum informationStandard;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationInstructionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInstruction", typeof(SettlementInstruction),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Instruction", IsVisible = true)]
        public SettlementInstruction informationInstruction;
		#endregion Members
    }
    #endregion SettlementInformation
    #region SettlementInstruction
    /// <summary>
    /// <para><b>Description :</b> A type that models a complete instruction for settling a currency payment, including 
    /// the settlement method to be used, the correspondent bank, any intermediary banks and the ultimate beneficary.</para>
    /// <para><b>Contents :</b></para>
    /// <para>settlementMethod (zero or one occurrence; of the type SettlementMethod) The mechanism by which
    /// settlement is to be made. The scheme of domain values will include standard mechanisms such as CLS,
    /// Fedwire, Chips ABA, Chips UID, SWIFT, CHAPS and DDA.</para>
    /// <para>correspondentInformation (zero or one occurrence; of the type Routing) The information required to identify
    /// the correspondent bank that will make delivery of the funds on the paying bank's behalf in the country where
    /// the payment is to be made</para>
    /// <para>intermediaryInformation (zero or more occurrences; of the type IntermediaryInformation) Information to
    /// identify an intermediary through which payment will be made by the correspondent bank to the ultimate
    /// beneficiary of the funds.</para>
    /// <para>beneficiaryBank (zero or one occurrence; of the type Routing) The bank that acts for the ultimate beneficiary
    /// of the funds in receiving payments.</para>
    /// <para>beneficiary (exactly one occurrence; of the type Routing) The ultimate beneficiary of the funds. The
    /// beneficiary can be identified either by an account at the beneficiaryBank (qv) or by explicit routingInformation.
    /// This element provides for the latter.</para>
    /// <para>splitSettlement (zero or more occurrences; of the type SplitSettlement) The set of individual payments that
    /// are to be made when a currency payment settling a trade needs to be split between a number of ultimate
    /// beneficiaries. Each split payment may need to have its own routing information.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SettlementInformation</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SettlementInstruction
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementMethod",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public SettlementMethod settlementMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool correspondentInformationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("correspondentInformation", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Correspondent Information")]
        public Routing correspondentInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool intermediaryInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("intermediaryInformation",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Intermediary Information")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Intermediary Information", IsClonable = true, IsChild = true)]
        public IntermediaryInformation[] intermediaryInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiaryBankSpecified;
		[System.Xml.Serialization.XmlElementAttribute("beneficiaryBank", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary Bank")]
        public Routing beneficiaryBank;
		[System.Xml.Serialization.XmlElementAttribute("beneficiary", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary", IsVisible = false)]
        public Routing beneficiary;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool splitSettlementSpecified;
        [System.Xml.Serialization.XmlElementAttribute("splitSettlement",Order=6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Split Settlement")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Split Settlement", IsClonable = true, IsChild = true)]
        public SplitSettlement[] splitSettlement;
		#endregion Members
    }
    #endregion SettlementInstruction
    #region SettlementMethod
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SettlementInstruction</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SettlementMethod : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementMethodScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
    }
    #endregion SettlementMethod
    #region SettlementPriceSource
    /// <summary>
    /// <para><b>Description :</b> The source from which the settlement price is to be obtained, e.g. a Reuters page, 
    /// Prezzo di Riferimento, etc.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: EquityExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class SettlementPriceSource : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string settlementPriceSourceScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
		#endregion Members
		#region Constructors
		public SettlementPriceSource()
        {
            settlementPriceSourceScheme = "http://www.fpml.org/coding-scheme/settlement-price-source-1-0";
        }
        #endregion Constructors
    }
    #endregion SettlementPriceSource
    #region SharedAmericanExercise
    /// <summary>
    /// <para><b>Description :</b> TBA</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Exercise)</para>
    /// <para>• The abstract base class for all types which define way in which options may be exercised.</para>
    /// <para>commencementDate (exactly one occurrence; of the type AdjustableOrRelativeDate) The first day of the
    /// exercise period for an American style option.</para>
    /// <para>expirationDate (exactly one occurrence; of the type AdjustableOrRelativeDate) The last day within an
    /// exercise period for an American style option. For a European style option it is the only day within the exercise
    /// period.</para>
    /// <para>latestExerciseTime (zero or one occurrence; of the type BusinessCenterTime) For a Bermudan or American
    /// style option, the latest time on an exercise business day (excluding the expiration date) within the exercise
    /// period that notice can be given by the buyer to the seller or seller's agent. Notice of exercise given after this
    /// time will be deemed to have been given on the next exercise business day.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: EquityAmericanExercise</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: EquityAmericanExercise</para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAmericanExercise))]
    public class SharedAmericanExercise
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("commencementDate", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commencement date", IsVisible = false)]
        public AdjustableOrRelativeDate commencementDate;
		[System.Xml.Serialization.XmlElementAttribute("expirationDate", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commencement date")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date", IsVisible = false)]
        public AdjustableOrRelativeDate expirationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool latestExerciseTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("latestExerciseTime", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "latest Exercise Time")]
        [ControlGUI(Name = "value")]
        public BusinessCenterTime latestExerciseTime;

        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
        public string id
        {
            set { efs_id = new EFS_Id(value); }
            get { return efs_id.Value; }
        }
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
	}
    #endregion SharedAmericanExercise
    #region SplitSettlement
    /// <summary>
    /// <para><b>Description :</b> A type that supports the division of a gross settlement amount into a number of split settlements, each
    /// requiring its own settlement instruction.</para>
    /// <para><b>Contents :</b></para>
    /// <para>splitSettlementAmount (exactly one occurrence; of the type Money) One of the monetary amounts in a split
    /// settlement payment.</para>
    /// <para>beneficiaryBank (zero or one occurrence; of the type Routing) The bank that acts for the ultimate beneficiary
    /// of the funds in receiving payments.</para>
    /// <para>beneficiary (exactly one occurrence; of the type Routing) The ultimate beneficiary of the funds. The
    /// beneficiary can be identified either by an account at the beneficiaryBank (qv) or by explicit routingInformation.
    /// This element provides for the latter.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SettlementInstruction</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class SplitSettlement : IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("splitSettlementAmount", Order = 1)]
		[ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money splitSettlementAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool beneficiaryBankSpecified;
		[System.Xml.Serialization.XmlElementAttribute("beneficiaryBank", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary Bank")]
        public Routing beneficiaryBank;
		[System.Xml.Serialization.XmlElementAttribute("beneficiary", Order = 3)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary", IsVisible = false)]
        public Routing beneficiary;
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Beneficiary")]
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool FillBalise;
		#endregion Members
		#region Methods
		#region DisplayArray
		public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion SplitSettlement
    #region Step
    /// <summary>
    /// <para><b>Description :</b> A type defining a step date and step value pair. This step definitions are used 
    /// to define varying rate or amount schedules, e.g. a notional amortization or a step-up coupon schedule.</para>
    /// <para><b>Contents :</b></para>
    /// <para>stepDate (exactly one occurrence; of the type xsd:date) The date on which the associated stepValue
    /// becomes effective. This day may be subject to adjustment in accordance with a business day convention.</para>
    /// <para>stepValue (exactly one occurrence; of the type xsd:decimal) The rate or amount which becomes effective on
    /// the associated stepDate. A rate of 5% would be represented as 0.05.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: Schedule</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class Step
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("stepDate", Order = 1)]
		[ControlGUI(Name = "Step Date", LblWidth = 75)]
        public EFS_Date stepDate;
		[System.Xml.Serialization.XmlElementAttribute("stepValue", Order = 2)]
		[ControlGUI(Name = "Value", LblWidth = 45)]
        public EFS_Decimal stepValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.PaymentQuote)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion Step
	#region StreetAddress
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
	public partial class StreetAddress : ItemGUI
	{
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("streetLine", IsNullable = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Street line", IsClonable = true, IsMaster = true, IsChild = false)]
		public EFS_StringArray[] streetLine;
		#endregion Members
	}
	#endregion StreetAddress

	#region Strike
	/// <summary>
    /// <para><b>Description :</b> A type describing a single cap or floor rate.</para>
    /// <para><b>Contents :</b></para>
    /// <para>strikeRate (exactly one occurrence; of the type xsd:decimal) The rate for a cap or floor.</para>
    /// <para>buyer (zero or one occurrence; with locally defined content) The buyer of the option</para>
    /// <para>seller (zero or one occurrence; with locally defined content) The party that has sold.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRateDefinition</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Strike : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("strikeRate", Order = 1)]
		public EFS_Decimal strikeRate;
		[System.Xml.Serialization.XmlElementAttribute("buyer", Order = 2)]
		public IdentifiedPayerReceiver buyer;
		[System.Xml.Serialization.XmlElementAttribute("seller", Order = 3)]
		public IdentifiedPayerReceiver seller;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
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
    }
    #endregion Strike
    #region StrikeSchedule
    /// <summary>
    /// <para><b>Description :</b> A type describing a schedule of cap or floor rates.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Schedule))</para>
    /// <para>• A type defining a schedule of rates or amounts in terms of an initial value and then a series of
    /// step date and value pairs. On each step date the rate or amount changes to the new step value.
    /// The series of step date and value pairs are optional. If not specified, this implies that the initial
    /// value remains unchanged over time.</para>
    /// <para>buyer (zero or one occurrence; with locally defined content) The buyer of the option</para>
    /// <para>seller (zero or one occurrence; with locally defined content) The party that has sold.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FloatingRate</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class StrikeSchedule : Schedule
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool buyerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("buyer", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Buyer")]
        public IdentifiedPayerReceiver buyer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sellerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("seller", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seller")]
        public IdentifiedPayerReceiver seller;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Pay Out Schedule")]
        public EFS_RadioChoice payOut;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payOutNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty payOutNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payOutAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payOutAmountSchedule", typeof(PayOutAmountSchedule),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public PayOutAmountSchedule payOutAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payOutRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payOutRateSchedule", typeof(Schedule),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Schedule payOutRate;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:StrikeSchedule";
		#endregion Members
    }
    #endregion StrikeSchedule

    #region TradeReference
    /// <summary>
    /// <para><b>Description :</b> </para>
    /// <para><b>Contents :</b></para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class TradeReference
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "IDREF")]
        public string href;
		#endregion Members
	}
    #endregion TradeReference

    #region UnadjustedDate
    /// <revision>
    ///     <version>1.1.9.10</version><date>20071003</date><author>EG</author>
    ///     <comment>
    ///     Ticket 15800
    ///     Add field dtValue type EFS_Date (not serialized) to format the unadjusted Date 
    ///     </comment>
    /// </revision>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class unadjustedDate
	{
		#region Members
		// 20071003 EG : Ticket 15800
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Name = "value")]
        public EFS_Date dtValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(LineFeed = MethodsGUI.LineFeedEnum.After)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
		#endregion Members
    }
    #endregion UnadjustedDate
}