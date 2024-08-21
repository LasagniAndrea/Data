#region Using Directives
using System;
using System.Collections;
using System.Reflection;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.EventMatrix;

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
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

namespace FpML.v42.Fx
{
    #region CutName
    /// <summary>
    /// <para><b>Description :</b> Allows for an expiryDateTime cut to be described by name.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type xsd:string)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ExpiryDateTime</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CutName : SchemeGUI
	{
		#region Members
		[System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string cutNameScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
		#endregion Members
		#region Constructors
		public CutName()
        {
            cutNameScheme = "http://www.fpml.org/coding-scheme/cut-name-1-0";
        }
        #endregion Constructors

    }
    #endregion CutName

    #region ExchangeRate
    /// <summary>
    /// <para><b>Description :</b> A type that is used for describing the exchange rate for a particular transaction.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type FxRate)</para>
    /// <para>• A type describing the rate of a currency conversion: pair of currency, quotation mode and
    /// exchange rate.</para>
    /// <para>spotRate (zero or one occurrence; of the type xsd:decimal) An optional element used for FX forwards and
    /// certain types of FX OTC options. For deals consumated in the FX Forwards Market, this represents the current
    /// market rate for a particular currency pair. For barrier and digital/binary options, it can be useful to include the
    /// spot rate at the time the option was executed to make it easier to know whether the option needs to move "up"
    /// or "down" to be triggered.</para>
    /// <para>forwardPoints (zero or one occurrence; of the type xsd:decimal) An optional element used for deals
    /// consumated in the FX Forwards market. Forward points represent the interest rate differential between the two
    /// currencies traded and are quoted as a preminum or a discount. Forward points are added to, or subtracted
    /// from, the spot rate to create the rate of the forward trade.</para>
    /// <para>sideRates (zero or one occurrence; of the type SideRates) An optional element that allow for definition of
    /// rates against base currency for non-base currency FX contracts.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class ExchangeRate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        public QuotedCurrencyPair quotedCurrencyPair;

        [System.Xml.Serialization.XmlElementAttribute("rate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [ControlGUI(Name = "rate", LblWidth = 75, Width = 75)]
        public EFS_Decimal rate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "spotRate", LblWidth = 75, Width = 75)]
        public EFS_Decimal spotRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardPointsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forwardPoints", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "forwardPoints", LblWidth = 75, Width = 75)]
        public EFS_Decimal forwardPoints;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideRatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sideRates", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side Rates")]
        public SideRates sideRates;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxFixingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxFixing",Order=6 )]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixing")]
        public FxFixing fxFixing;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeRate";
		#endregion Members
    }
    #endregion ExchangeRate
    #region ExpiryDateTime
    /// <summary>
    /// <para><b>Description :</b> A type that describes the date and time in a location of the option expiry. 
    /// In the case of American options this is the latest possible expiry date and time.</para>
    /// <para><b>Contents :</b></para>
    /// <para>expiryDate (exactly one occurrence; of the type xsd:date) Represents a standard expiry date as defined for an
    /// FX OTC option.</para>
    /// <para>expiryTime (exactly one occurrence; of the type BusinessCenterTime)</para>
    /// <para>cutName (zero or one occurrence; of the type CutName)</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para>• Complex type: FXDigitalOption</para>
    ///<para>• Complex type: FXOptionLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ExpiryDateTime : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("expiryDate", Order = 1)]
		[ControlGUI(IsLabel = true, Name = "Date")]
        public EFS_Date expiryDate;
		[System.Xml.Serialization.XmlElementAttribute("expiryTime", Order = 2)]
        [ControlGUI(Name = " ")]
        public BusinessCenterTime expiryTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cutNameSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cutName", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cut name")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CutName cutName;
		#endregion Members
    }
    #endregion ExpiryDateTime

    #region FxAmericanTrigger
    /// <summary>
    /// <para><b>Description :</b> A type that defines a particular type of payout in an FX OTC exotic option. 
    /// An American trigger occurs if the trigger criteria are met at any time from the initiation to the maturity of the option.</para>
    /// <para><b>Contents :</b></para>
    /// <para>touchCondition (exactly one occurrence; of the type TouchConditionEnum) The binary condition that applies
    /// to an American-style trigger. There can only be two domain values for this element: "touch" or "no touch".</para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>triggerRate (exactly one occurrence; of the type xsd:decimal) The market rate is observed relative to the
    /// trigger rate, and if it is found to be on the predefined side of (above or below) the trigger rate, a trigger event is
    /// deemed to have occurred.</para>
    /// <para>informationSource (one or more occurrences; of the type InformationSource) The information source where a
    /// published or displayed market rate will be obtained, e.g. Telerate Page 3750.</para>
    /// <para>observationStartDate (zero or one occurrence; of the type xsd:date) The start of the period over which
    /// observations are made to determine whether a trigger has occurred.</para>
    /// <para>observationEndDate (zero or one occurrence; of the type xsd:date) The end of the period over which
    /// observations are made to determine whether a trigger event has occurred.</para> 
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxDigitalOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxAmericanTrigger
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("touchCondition", Order = 1)]
		[ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TouchConditionEnum touchCondition;
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
		[System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource",Order= 4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation start date")]
        public EFS_Date observationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation end date")]
        public EFS_Date observationEndDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate; // use to determine trigger type (UP/DOWN)
		#endregion Members
    }
    #endregion FxAmericanTrigger
    #region FxAverageRateObservationDate
    /// <summary>
    /// <para><b>Description :</b> A type that, for average rate options, is used to describe each specific observation date, 
    /// as opposed to a parametric frequency of rate observations.</para>
    /// <para><b>Contents :</b></para>
    /// <para>observationDate (exactly one occurrence; of the type xsd:date) A specific date for which an observation
    /// against a particular rate will be made and will be used for subsequent computations.</para>
    /// <para>averageRateWeightingFactor (exactly one occurrence; of the type xsd:decimal) An optional factor that can
    /// be used for weighting certain observation dates. Typically, firms will weight each date with a factor of 1 if there
    /// are standard, unweighted adjustments.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxAverageRateOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxAverageRateObservationDate : ItemGUI
    {
        #region Members
		[System.Xml.Serialization.XmlElementAttribute("observationDate", Order = 1)]
        [ControlGUI(Name = "value")]
        public EFS_Date observationDate;
		[System.Xml.Serialization.XmlElementAttribute("averageRateWeightingFactor", Order = 2)]
        [ControlGUI(IsLabel = true, Name = "Weighting Factor", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 50)]
        public EFS_Decimal averageRateWeightingFactor;
        #endregion Members
    }
    #endregion FxAverageRateObservationDate
    #region FxAverageRateObservationSchedule
    /// <summary>
    /// <para><b>Description :</b> A type that describes average rate options rate observations. 
    /// This is used to describe a parametric frequency of rate observations against a particular rate. 
    /// Typical frequencies might include daily, every Friday, etc.</para>
    /// <para><b>Contents :</b></para>
    /// <para>observationStartDate (exactly one occurrence; of the type xsd:date) The start of the period over which
    /// observations are made to determine whether a trigger has occurred.</para>
    /// <para>observationEndDate (exactly one occurrence; of the type xsd:date) The end of the period over which
    /// observations are made to determine whether a trigger event has occurred.</para>
    /// <para>calculationPeriodFrequency (exactly one occurrence; of the type CalculationPeriodFrequency) The
    /// frequency at which calculation period end dates occur with the regular part of the calculation period schedule
    /// and their roll date convention.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxAverageRateOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxAverageRateObservationSchedule : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 1)]
		[ControlGUI(Name = "Start Date")]
        public EFS_Date observationStartDate;
		[System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 2)]
        [ControlGUI(Name = "End Date", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Date observationEndDate;
		[System.Xml.Serialization.XmlElementAttribute("calculationPeriodFrequency", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency", IsVisible = false)]
        public CalculationPeriodFrequency calculationPeriodFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation Period Frequency")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion FxAverageRateObservationSchedule
    #region FxAverageRateOption
    /// <summary>
    /// <para><b>Description :</b> A type that is used for an option whose payout is based on the average of the price 
    /// of the underlying over a specific period of time. The payout is the difference between the predetermined, fixed strike 
    /// price and the average of spot rates observed and is used for hedging against prevailing spot rates 
    /// over a given time period.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
    /// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
    /// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</para>
    /// <para>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
    /// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
    /// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</para>
    /// <para>expiryDateTime (exactly one occurrence; of the type ExpiryDateTime) The date and time in a location of the
    /// option expiry. In the case of american options this is the latest possible expiry date and time.</para>
    /// <para>exerciseStyle (exactly one occurrence; of the type ExerciseStyleEnum) The manner in which the option can
    /// be exercised.</para> 
    /// <para>fxOptionPremium (zero or more occurrences; of the type FXOptionPremium) Premium amount or premium
    /// installment amount for an option.</para>
    /// <para>valueDate (exactly one occurrence; of the type xsd:date) The date on which both currencies traded will settle.</para>
    /// <para>putCurrencyAmount (exactly one occurrence; of the type Money) The currency amount that the option gives
    /// the right to sell.</para>
    /// <para>callCurrencyAmount (exactly one occurrence; of the type Money) The currency amount that the option gives
    /// the right to buy.</para>
    /// <para>fxStrikePrice (exactly one occurrence; of the type FXStrikePrice) TBA</para>
    /// <para>spotRate (zero or one occurrence; of the type xsd:decimal) An optional element used for FX forwards and
    /// certain types of FX OTC options. For deals consumated in the FX Forwards Market, this represents the current
    /// market rate for a particular currency pair. For barrier and digital/binary options, it can be useful to include the
    /// spot rate at the time the option was executed to make it easier to know whether the option needs to move "up"
    /// or "down" to be triggered.</para>
    /// <para>payoutCurrency (exactly one occurrence; of the type Currency) The ISO code of the currency in which a
    /// payout (if any) is to be made when a trigger is hit on a digital or barrier option.</para>
    /// <para>averageRateQuoteBasis (exactly one occurrence; of the type StrikeQuoteBasisEnum) The method by which
    /// the average rate that is being observed is quoted.</para>
    /// <para>precision (zero or one occurrence; of the type xsd:nonNegativeInteger) Specifies the rounding precision in
    /// terms of a number of decimal places. Note how a percentage rate rounding of 5 decimal places is expressed
    /// as a rounding precision of 7 in the FpML document since the percentage is expressed as a decimal, e.g.
    /// 9.876543% (or 0.09876543) being rounded to the nearest 5 decimal places is 9.87654% (or 0.0987654).</para>
    /// <para>payoutFormula (zero or one occurrence; of the type xsd:string) The description of the mathematical
    /// computation for how the payout is computed.</para> 
    /// <para>primaryRateSource (exactly one occurrence; of the type InformationSource) The primary source for where
    /// the rate observation will occur. Will typically be either a page or a reference bank published rate.</para>
    /// <para>secondaryRateSource (zero or one occurrence; of the type InformationSource) An alternative, or secondary,
    /// source for where the rate observation will occur. Will typically be either a page or a reference bank published
    /// rate.</para>
    /// <para>fixingTime (exactly one occurrence; of the type BusinessCenterTime) The time at which the spot currency
    /// exchange rate will be observed. It is specified as a time in a specific business center, e.g. 11:00am London
    /// time.</para>
    /// <para>Either</para>
    /// <para>averageRateObservationSchedule (exactly one occurrence; of the type FXAverageRateObservationSchedule) 
    /// Parametric schedule of rate observations.</para>
    /// <para>Or</para>
    /// <para>averageRateObservationDate (one or more occurrences; of the type FXAverageRateObservationDate) One
    /// of more specific rate observation dates.</para>
    /// <para>observedRates (zero or more occurrences; of the type ObservedRates) Describes prior rate observations
    /// within average rate options. Periodically, an average rate option agreement will be struck whereby some rates
    /// have already been observed in the past but will become part of computation of the average rate of the option.
    /// This structure provides for these previously observed rates to be included in the description of the trade.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: fxAverageRateOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxAverageRateOption", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
	public partial class FxAverageRateOption : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference buyerPartyReference;
        [ControlGUI(Name = "Seller")]
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 2)]
        public PartyReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Value Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 7)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "spot Rate", Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlElementAttribute("payoutCurrency", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 11)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Currency", IsVisible = false)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 75)]
        public Currency payoutCurrency;
        [System.Xml.Serialization.XmlElementAttribute("averageRateQuoteBasis", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 12)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Currency")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "averageRateQuoteBasis", Width = 220)]
        public StrikeQuoteBasisEnum averageRateQuoteBasis;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool precisionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("precision", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Precision")]
        public EFS_PosInteger precision;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool payoutFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payoutFormula", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payout Formula")]
        public EFS_MultiLineString payoutFormula;
        [System.Xml.Serialization.XmlElementAttribute("primaryRateSource", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 15)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Source", IsVisible = false)]
        public InformationSource primaryRateSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool secondaryRateSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("secondaryRateSource", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Secondary Rate Source")]
        public InformationSource secondaryRateSource;
        [System.Xml.Serialization.XmlElementAttribute("fixingTime", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 17)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Source")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time", IsVisible = false)]
        public BusinessCenterTime fixingTime;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fixing Time")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate Observation")]
        public EFS_RadioChoice rateObservation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateObservationScheduleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averageRateObservationSchedule", typeof(FxAverageRateObservationSchedule)
             , Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Rate Observation Schedule", IsVisible = true)]
        public FxAverageRateObservationSchedule rateObservationSchedule;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateObservationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("averageRateObservationDate", typeof(FxAverageRateObservationDate)
             , Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Observation Dates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate Observation Date", IsClonable = true, IsMaster = true)]
        public FxAverageRateObservationDate[] rateObservationDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observedRatesSpecified;

        [System.Xml.Serialization.XmlElementAttribute("observedRates", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Observed Rates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Observed Rates", IsClonable = true, IsChild = true)]
        public ObservedRates[] observedRates;
        #endregion Members

		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 21)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order=22)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates")]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order=23)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool averageStrikeOptionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("averageStrikeOption", typeof(AverageStrikeOption),Order = 24)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Average strike option", Width = 75)]
		public AverageStrikeOption averageStrikeOption;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool geometricAverageSpecified;
		[System.Xml.Serialization.XmlElementAttribute("geometricAverage", Order = 25)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Geometric average")]
		public Empty geometricAverage;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
		public bool FillBalise;
		#endregion Members

    }
    #endregion FxAverageRateOption
    #region FxBarrier
    /// <summary>
    /// <para><b>Description :</b> A type that is used within the FX barrier option definition to define one or more barrier 
    /// levels that determine whether the option will be knocked-in or knocked-out.</para>
    /// <para><b>Contents :</b></para>
    /// <para>fxBarrierType (zero or one occurrence; of the type FxBarrierTypeEnum) This specifies whether the option
    /// becomes effective ("knock-in") or is annulled ("knock-out") when the respective trigger event occurs.</para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>triggerRate (exactly one occurrence; of the type xsd:decimal) The market rate is observed relative to the
    /// trigger rate, and if it is found to be on the predefined side of (above or below) the trigger rate, a trigger event is
    /// deemed to have occurred.</para>
    /// <para>informationSource (one or more occurrences; of the type InformationSource) The information source where a
    /// published or displayed market rate will be obtained, e.g. Telerate Page 3750.</para>
    /// <para>observationStartDate (zero or one occurrence; of the type xsd:date) The start of the period over which
    /// observations are made to determine whether a trigger has occurred.</para>
    /// <para>observationEndDate (zero or one occurrence; of the type xsd:date) The end of the period over which
    /// observations are made to determine whether a trigger event has occurred.</para> 
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXBarrierOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxBarrier : ItemGUI
    {
		#region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxBarrierTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public FxBarrierTypeEnum fxBarrierType;
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "QuotedCurrencyPair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
		[System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "QuotedCurrencyPair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Trigger rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource",Order=4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observationStartDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation Start Date")]
        public EFS_Date observationStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool observationEndDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("observationEndDate", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Observation End Date")]
        public EFS_Date observationEndDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate; // use to determine barrier type (UP/DOWN)
		#endregion Members
    }
    #endregion FxBarrier
    #region FxBarrierOption
    /// <summary>
    /// <para><b>Description :</b> A type that describes an option with a put/call component, but also one or more 
    /// associated barrier rates. If the market rate moves to reach a barrier rate a trigger event occurs. 
    /// The trigger event may for example be necessary to enable the option, or may annul the option contract. 
    /// [Since the barriers reduce the probability of exercise, the premium for an option with barriers is 
    /// likely to be cheaper than one without].</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type FXOptionLeg)</para>
    /// <para>• A type that is used for describing a standard FX OTC option (European or American) which may
    /// be a complete trade in its own right or part of a trade strategy.</para>
    /// <para>spotRate (zero or one occurrence; of the type xsd:decimal) An optional element used for FX forwards and
    /// certain types of FX OTC options. For deals consumated in the FX Forwards Market, this represents the current
    /// market rate for a particular currency pair. For barrier and digital/binary options, it can be useful to include the
    /// spot rate at the time the option was executed to make it easier to know whether the option needs to move "up"
    /// or "down" to be triggered.</para>
    /// <para>fxBarrier (one or more occurrences; of the type FXBarrier) Information about a barrier rate in a Barrier Option
    /// - specifying the exact criteria for a trigger event to occur.</para>
    /// <para>triggerPayout (zero or one occurrence; of the type FXOptionPayout) The amount of currency which becomes
    /// payable if and when a trigger event occurs.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: fxBarrierOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxBarrierOption", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class FxBarrierOption : FxOptionLegBarrier
    {
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Spot rate", Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlElementAttribute("fxBarrier", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 2)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Barrier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier", IsClonable = true, IsMaster = true, IsChild = true)]
        public FxBarrier[] fxBarrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool triggerPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("triggerPayout", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger Payout")]
        public FxOptionPayout triggerPayout;
		#endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 4)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates")]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure",Order = 6)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedCallOrFlooredPutSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cappedCallOrFlooredPut", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capped call or floored put")]
		public CappedCallOrFlooredPut cappedCallOrFlooredPut;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool fxBarrierParisianNumberOfDaysSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxBarrierParisianNumberOfDays", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Parisian barrier number of days")]
		public EFS_Integer fxBarrierParisianNumberOfDays;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool fxRebateBarrierSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fxRebateBarrier", Order = 9)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rebate barrier")]
		public FxBarrier fxRebateBarrier;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
		public bool FillBalise;
		#endregion Members
    }
    #endregion FxBarrierOption
    #region FxDigitalOption
    /// <summary>
    /// <para><b>Description :</b> A type that describes an option without a put/call component (and so no associated exercise), 
    /// but with one or more trigger rates) Examples are "one-touch", "no-touch", and "double-no-touch" options. For a specified
    /// period the market rate is observed relative to the trigger rates, and on a trigger event a fixed payout may
    /// become due to the buyer of the option, or alternatively the option contract may be annulled.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
    /// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
    /// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</para>
    /// <para>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
    /// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
    /// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</para>
    /// <para>expiryDateTime (exactly one occurrence; of the type ExpiryDateTime) The date and time in a location of the
    /// option expiry. In the case of american options this is the latest possible expiry date and time.</para>
    /// <para>fxOptionPremium (zero or more occurrences; of the type FXOptionPremium) Premium amount or premium
    /// installment amount for an option.</para> 
    /// <para>valueDate (exactly one occurrence; of the type xsd:date) The date on which both currencies traded will settle.</para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>spotRate (zero or one occurrence; of the type xsd:decimal) An optional element used for FX forwards and
    /// certain types of FX OTC options. For deals consumated in the FX Forwards Market, this represents the current
    /// market rate for a particular currency pair. For barrier and digital/binary options, it can be useful to include the
    /// spot rate at the time the option was executed to make it easier to know whether the option needs to move "up"
    /// or "down" to be triggered.</para>
    /// <para>Either</para>
    /// <para>fxEuropeanTrigger (one or more occurrences; of the type FXEuropeanTrigger) A European trigger occurs if
    /// the trigger criteria are met, but these are valid (and an observation is made) only at the maturity of the option.</para> 
    /// <para>Or</para>
    /// <para>fxAmericanTrigger (one or more occurrences; of the type FXAmericanTrigger) An American trigger occurs if
    /// the trigger criteria are met at any time from the initiation to the maturity of the option.</para>
    /// <para>triggerPayout (exactly one occurrence; of the type FXOptionPayout) The amount of currency which becomes
    /// payable if and when a trigger event occurs.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: fxDigitalOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxDigitalOption", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class FxDigitalOption : Product
    {
		#region Members
        [System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2)]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry date", IsVisible = false)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Value Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted currency pair", IsVisible = true)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("spotRate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted Currency Pair")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = true, Name = "Spot rate", Width = 75)]
        public EFS_Decimal spotRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger")]
        public EFS_RadioChoice typeTrigger;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeTriggerEuropeanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxEuropeanTrigger", Namespace = "http://www.fpml.org/2005/FpML-4-2",
             Type = typeof(FxEuropeanTrigger), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "European trigger", IsVisible = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "European trigger", IsClonable = true, IsChild = true)]
        public FxEuropeanTrigger[] typeTriggerEuropean;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool typeTriggerAmericanSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxAmericanTrigger", Namespace = "http://www.fpml.org/2005/FpML-4-2",
             Type = typeof(FxAmericanTrigger), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "American trigger", IsVisible = false)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "American trigger", IsClonable = true, IsChild = true)]
        public FxAmericanTrigger[] typeTriggerAmerican;
        [System.Xml.Serialization.XmlElementAttribute("triggerPayout", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 10)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger payout", IsVisible = false)]
        public FxOptionPayout triggerPayout;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool limitSpecified;
        [System.Xml.Serialization.XmlElementAttribute("limit", typeof(Empty), Order = 11)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Trigger payout")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Limit")]
        public Empty limit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool boundarySpecified;
        [System.Xml.Serialization.XmlElementAttribute("boundary", typeof(Empty), Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Boundary")]
        public Empty boundary;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assetOrNothingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("assetOrNothing", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Asset or nothing")]
        public AssetOrNothing assetOrNothing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool resurrectingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("resurrecting", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Resurrecting")]
        public PayoutPeriod resurrecting;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool extinguishingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("extinguishing", typeof(PayoutPeriod), Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Extinguishing")]
        public PayoutPeriod extinguishing;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxBarrierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxBarrier", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Barrier", IsClonable = true, IsChild = true)]
        public FxBarrier[] fxBarrier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
        public bool  FillBalise;
        #endregion Members
    }
    #endregion FxDigitalOption
    #region FxEuropeanTrigger
    /// <summary>
    /// <para><b>Description :</b> A type that defines a particular type of payout in an FX OTC exotic option. 
    /// A European trigger occurs if the trigger criteria are met, but these are valid (and an observation is made) 
    /// only at the maturity of the option.</para>
    /// <para><b>Contents :</b></para>
    /// <para>triggerCondition (exactly one occurrence; of the type TriggerConditionEnum) The binary condition that
    /// applies to a European-style trigger, determining where the spot rate must be relative to the triggerRate for the
    /// option to be exercisable. There can only be two domain values for this element: "aboveTrigger" or
    /// "belowTrigger".</para>
    /// <para>quotedCurrencyPair (exactly one occurrence; of the type QuotedCurrencyPair) Defines the two currencies for
    /// an FX trade and the quotation relationship between the two currencies.</para>
    /// <para>triggerRate (exactly one occurrence; of the type xsd:decimal) The market rate is observed relative to the
    /// trigger rate, and if it is found to be on the predefined side of (above or below) the trigger rate, a trigger event is
    /// deemed to have occurred.</para>
    /// <para>informationSource (one or more occurrences; of the type InformationSource) The information source where a
    /// published or displayed market rate will be obtained, e.g. Telerate Page 3750.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxDigitalOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxEuropeanTrigger
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("triggerCondition", Order = 1)]
		[ControlGUI(Name = "Condition", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public TriggerConditionEnum triggerCondition;
		[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 2)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair", IsVisible = false)]
        public QuotedCurrencyPair quotedCurrencyPair;
		[System.Xml.Serialization.XmlElementAttribute("triggerRate", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted currency pair")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Rate", Width = 75)]
        public EFS_Decimal triggerRate;
        [System.Xml.Serialization.XmlElementAttribute("informationSource",Order=4)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source", IsClonable = true, IsMaster = true, IsChild = true)]
        public InformationSource[] informationSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public EFS_Decimal spotRate;
		#endregion Members
    }
    #endregion FxEuropeanTrigger
    #region FxLeg
    /// <summary>
    /// <para><b>Description :</b> A type that represents a single exchange of one currency for another. 
    /// This is used for representing FX spot,forward, and swap transactions.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>exchangedCurrency1 (exactly one occurrence; of the type Payment) This is the first of the two currency flows
    /// that define a single leg of a standard foreign exchange transaction.</para>
    /// <para>exchangedCurrency2 (exactly one occurrence; of the type Payment) This is the second of the two currency
    /// flows that define a single leg of a standard foreign exchange transaction.</para>
    /// <para>Either</para>
    /// <para>valueDate (exactly one occurrence; of the type xsd:date) 
    /// The date on which both currencies traded will settle.</para> 
    /// <para>exchangeRate (exactly one occurrence; of the type ExchangeRate) The rate of exchange between the two
    /// currencies.</para>
    /// <para>nonDeliverableForward (zero or one occurrence; of the type FXCashSettlement) Used to describe a
    /// particular type of FX forward transaction that is settled in a single currency.</para>
    /// <para>confirmationSenderPartyReference (zero or one occurrence; of the type PartyReference) The party that is
    /// sending the current document as a confirmation of the trade.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: fxSingleLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("fxSingleLeg", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class FxLeg : Product
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("exchangedCurrency1", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 1", IsVisible = false, IsCopyPaste = true)]
        public Payment exchangedCurrency1;
		[System.Xml.Serialization.XmlElementAttribute("exchangedCurrency2", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 1")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 2", IsVisible = false, IsCopyPaste = true)]
        public Payment exchangedCurrency2;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateValueDateSpecified;

        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Currency 2")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Date Settlement", IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value date")]
        public EFS_Date fxDateValueDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateCurrency1ValueDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency1ValueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency 1 value date")]
        public EFS_Date fxDateCurrency1ValueDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxDateCurrency2ValueDateSpecified;

        [System.Xml.Serialization.XmlElementAttribute("currency2ValueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency 2 value date")]
        public EFS_Date fxDateCurrency2ValueDate;

		[System.Xml.Serialization.XmlElementAttribute("exchangeRate", Order = 6)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Date Settlement")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate", IsVisible = false)]
        public ExchangeRate exchangeRate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nonDeliverableForwardSpecified;

		[System.Xml.Serialization.XmlElementAttribute("nonDeliverableForward", Order = 7)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Rate")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "non Deliverable Forward", IsCopyPaste = true)]
        public FxCashSettlement nonDeliverableForward;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool confirmationSenderPartyReferenceSpecified;

		[System.Xml.Serialization.XmlElementAttribute("confirmationSenderPartyReference", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "confirmation Sender")]
        public PartyReference confirmationSenderPartyReference;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150731 [XXXXX] add  (Ajouter permet à BA de consulter des trades en FpML.v42)
        /// FI 20170116 [21916] RptSide (R majuscule)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;

		#endregion Members
    }
    #endregion FxLeg
    #region FXOptionLeg
    /// <summary>
    /// <para><b>Description :</b> A type that is used for describing a standard FX OTC option (European or American) which may be a
    /// complete trade in its own right or part of a trade strategy.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>buyerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that buys
    /// this instrument, ie. pays for this instrument and receives the rights defined by it. See 2000 ISDA definitions
    /// Article 11.1 (b). In the case of FRAs this the fixed rate payer.</para>
    /// <para>sellerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that sells
    /// ("writes") this instrument, i.e. that grants the rights defined by this instrument and in return receives a payment
    /// for it. See 2000 ISDA definitions Article 11.1 (a). In the case of FRAs this is the floating rate payer.</para>
    /// <para>expiryDateTime (exactly one occurrence; of the type ExpiryDateTime) The date and time in a location of the
    /// option expiry. In the case of american options this is the latest possible expiry date and time.</para>
    /// <para>exerciseStyle (exactly one occurrence; of the type ExerciseStyleEnum) The manner in which the option can
    /// be exercised.</para> 
    /// <para>fxOptionPremium (zero or more occurrences; of the type FXOptionPremium) Premium amount or premium
    /// installment amount for an option.</para>
    /// <para>valueDate (exactly one occurrence; of the type xsd:date) The date on which both currencies traded will settle.</para>
    /// <para>cashSettlementTerms (zero or one occurrence; of the type FXCashSettlement) This optional element is only
    /// used if an option has been specified at execution time to be settled into a single cash payment. This would be
    /// used for a non-deliverable option.</para>
    /// <para>putCurrencyAmount (exactly one occurrence; of the type Money) The currency amount that the option gives
    /// the right to sell.</para>
    /// <para>callCurrencyAmount (exactly one occurrence; of the type Money) The currency amount that the option gives
    /// the right to buy.</para>
    /// <para>fxStrikePrice (exactly one occurrence; of the type FXStrikePrice) TBA</para>
    /// <para>quotedAs (zero or one occurrence; of the type QuotedAs) Describes how the option was quoted.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: fxSimpleOption</para>
    ///<para>• Complex type: FXBarrierOption</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: FXBarrierOption</para>
    ///</remarks>
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.fpml.org/2005/FpML-4-2")]
    //[System.Xml.Serialization.XmlRootAttribute("fxSimpleOption", Namespace="http://www.fpml.org/2005/FpML-4-2", IsNullable=false)]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxSimpleOption", Namespace = "http://www.efs.org/2005/EFSmL-2-0", IsNullable = false)]
    public partial class FxOptionLeg : Product
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order = 1)]
        [ControlGUI(Name = "Buyer", LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 2 )]
        [ControlGUI(Name = "Seller", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PartyReference sellerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("expiryDateTime", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date", IsVisible = false, IsCopyPaste = true)]
        public ExpiryDateTime expiryDateTime;
        [System.Xml.Serialization.XmlElementAttribute("exerciseStyle", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Expiry Date")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Exercise")]
        public ExerciseStyleEnum exerciseStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxOptionPremiumSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxOptionPremium", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 5)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Premium")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium", IsClonable = true, IsChild = true, IsCopyPasteItem = true)]
        public FxOptionPremium[] fxOptionPremium;
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 6)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Date")]
        public EFS_Date valueDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementTermsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Cash Settlement Terms", IsCopyPaste = true)]
        public FxCashSettlement cashSettlementTerms;
        [System.Xml.Serialization.XmlElementAttribute("putCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 8)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount", IsVisible = false, IsCopyPaste = true)]
        [ControlGUI(Name = "value")]
        public Money putCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("callCurrencyAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 9)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Put Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount", IsVisible = false, IsCopyPaste = true)]
        [ControlGUI(Name = "value")]
        public Money callCurrencyAmount;
        [System.Xml.Serialization.XmlElementAttribute("fxStrikePrice", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 10)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call Amount")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price", IsVisible = false, IsCopyPaste = true)]
        public FxStrikePrice fxStrikePrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotedAsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedAs", Namespace = "http://www.fpml.org/2005/FpML-4-2", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Strike Price")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Quoted As", IsCopyPaste = true)]
        public QuotedAs quotedAs;
        #endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool optionTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("optionType", Order = 12)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions", IsGroup = true)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Call/Put")]
		public OptionTypeEnum optionType;

		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool bermudanExerciseDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("bermudanExerciseDates",Order = 13)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Bermudan exercise dates", IsCopyPaste = true)]
		public DateList bermudanExerciseDates;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool procedureSpecified;
		[System.Xml.Serialization.XmlElementAttribute("exerciseProcedure", Order = 14)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exercise Procedure")]
		public ExerciseProcedure procedure;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extensions")]
		public bool FillBalise;


        /// <summary>
        /// 
        /// </summary>
        /// FI 20150731 [XXXXX](Recette BA) add  (Ajouter permet à BA de consulter des trades en FpML.v42)
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixML.v50SP1.TrdCapRptSideGrp_Block[] RptSide;


		#endregion Members

    }
    #endregion FxOptionLeg
    #region FxOptionPayout
    /// <summary>
    /// <para><b>Description :</b> A type that contains full details of a predefined fixed payout which may occur (or not) 
    /// in a Barrier Option or Digital Option when a trigger event occurs (or not).</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Money)</para>
    /// <para>• A type defining a currency amount.</para>
    /// <para>payoutStyle (exactly one occurrence; of the type PayoutEnum) The trigger event and payout may be
    /// asynchonous. A payout may become due on the trigger event, or the payout may (by agreeement at initiation)
    /// be deferred (for example) to the maturity date.</para>
    /// <para>settlementInformation (zero or one occurrence; of the type SettlementInformation) The information required
    /// to settle a currency payment that results from a trade.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxBarrierOption</para>
    ///<para>• Complex type: FxDigitalOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxOptionPayout : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
		[ControlGUI(Name = "Currency", Width = 75)]
        public Currency currency;
		[System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = "Amount")]
        public EFS_Decimal amount;
		[System.Xml.Serialization.XmlElementAttribute("payoutStyle", Order = 3)]
        [ControlGUI(Name = "Payout style", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public PayoutEnum payoutStyle;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("settlementInformation", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Settlement Information")]
        public SettlementInformation settlementInformation;
		#endregion Members
    }
    #endregion FxOptionPayout
    #region FxOptionPremium
    /// <summary>
    /// <para><b>Description :</b> A type that specifies the premium exchanged for a single option trade or option strategy.</para>
    /// <para><b>Contents :</b></para>
    /// <para>payerPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party
    /// responsible for making the payments defined by this structure.</para>
    /// <para>receiverPartyReference (exactly one occurrence; of the type PartyReference) A reference to the party that
    /// receives the payments corresponding to this structure.</para>
    /// <para>premiumAmount (exactly one occurrence; of the type Money) The specific currency and amount of the option
    /// premium.</para>
    /// <para>premiumSettlementDate (exactly one occurrence; of the type xsd:date) The agreed-upon date when the
    /// option premium will be settled.</para>
    /// <para>settlementInformation (zero or one occurrence; of the type SettlementInformation) The information required
    /// to settle a currency payment that results from a trade.</para>
    /// <para>premiumQuote (zero or one occurrence; of the type PremiumQuote) This is the option premium as quoted. It
    /// is expected to be consistent with the premiumAmount and is for information only.</para> 
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxAverageRateOption</para>
    ///<para>• Complex type: FxDigitalOption</para>
    ///<para>• Complex type: FxOptionLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///<para>• Complex type: </para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2005/EFSmL-2-0")]
    public partial class FxOptionPremium
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=1)]
        [ControlGUI(Name = "Payer", LblWidth = 107)]
        public PartyReference payerPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
        [System.Xml.Serialization.XmlElementAttribute("premiumAmount", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=3)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money premiumAmount;
        [System.Xml.Serialization.XmlElementAttribute("premiumSettlementDate", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=4)]
        [ControlGUI(Name = "Settlement Date", LblWidth = 107)]
        public EFS_Date premiumSettlementDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementInformation", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement Information")]
        public SettlementInformation settlementInformation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool premiumQuoteSpecified;
        [System.Xml.Serialization.XmlElementAttribute("premiumQuote", Namespace = "http://www.fpml.org/2005/FpML-4-2",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Premium Quote")]
        public PremiumQuote premiumQuote;
		#endregion Members
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool customerSettlementPremiumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("customerSettlementPremium", Order = 7)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension", IsVisible = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Customer settlement premium")]
		public CustomerSettlementPayment customerSettlementPremium;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Extension")]
		public bool FillBalise;
		#endregion Members
    }
    #endregion FxOptionPremium
    #region FxStrikePrice
    /// <summary>
    /// <para><b>Description :</b> A type that describes the rate of exchange at which the option has been struck.</para>
    /// <para><b>Contents :</b></para>
    /// <para>rate (exactly one occurrence; of the type xsd:decimal) The rate of exchange between the two currencies of the
    /// leg of a deal. Must be specified with a quote basis.</para>
    /// <para>strikeQuoteBasis (exactly one occurrence; of the type StrikeQuoteBasisEnum) The method by which the
    /// strike rate is quoted.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FxAverageRateOption</para>
    ///<para>• Complex type: FxOptionLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class FxStrikePrice : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("rate", Order = 1)]
		[ControlGUI(Name = "Rate", Width = 75)]
        public EFS_Decimal rate;
		[System.Xml.Serialization.XmlElementAttribute("strikeQuoteBasis", Order = 2)]
        [ControlGUI(Name = "Basis", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 220)]
        public StrikeQuoteBasisEnum strikeQuoteBasis;
		#endregion Members
    }
    #endregion FXStrikePrice
    #region FxSwap
    /// <summary>
    /// <para><b>Description :</b> A type that describes an FX swap. This is similar to FpML_FXLeg, but contains multiple legs 
    /// for a particular trade.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>fxSingleLeg (one or more occurrences; of the type FXLeg) A single-legged FX transaction definition (e.g.,
    /// spot or forward).</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: fxSwap</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("fxSwap", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
	public partial class FxSwap : Product
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("fxSingleLeg",Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.First, Name = "Leg", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, MinItem = 2)]
        public FxLeg[] fxSingleLeg;
		#endregion Members
    }
    #endregion FxSwap

    #region ObservedRates
    /// <summary>
    /// <para><b>Description :</b> A type that describes prior rate observations within average rate options. 
    /// Periodically, an average rate option agreement will be struck whereby some rates have already been observed 
    /// in the past but will become part of computation of the average rate of the option. 
    /// This structure provides for these previously observed rates to be included in the description of the trade.</para>
    /// <para><b>Contents :</b></para>
    /// <para>observationDate (exactly one occurrence; of the type xsd:date) A specific date for which an observation
    /// against a particular rate will be made and will be used for subsequent computations.</para>
    /// <para>observedRate (exactly one occurrence; of the type xsd:decimal) The actual observed rate before any required
    /// rate treatment is applied, e.g. before converting a rate quoted on a discount basis to an equivalent yield. An
    /// observed rate of 5% would be represented as 0.05.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXAverageRateOption</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class ObservedRates
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("observationDate", Order = 1)]
		[ControlGUI(Name = "observation Date")]
        public EFS_Date observationDate;
		[System.Xml.Serialization.XmlElementAttribute("observedRate", Order = 2)]
        [ControlGUI(Name = "rate", LblWidth = 45, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal observedRate;
		#endregion Members
    }
    #endregion ObservedRates

    #region PremiumQuote
    /// <summary>
    /// <para><b>Description :</b> A type that describes the option premium as quoted.</para>
    /// <para><b>Contents :</b></para>
    /// <para>premiumValue (exactly one occurrence; of the type xsd:decimal) The value of the premium quote. In general
    /// this will be either a percentage or an explicit amount.</para>
    /// <para>premiumQuoteBasis (exactly one occurrence; of the type PremiumQuoteBasisEnum) The method by which
    /// the option premium was quoted.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXOptionPremium</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class PremiumQuote : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("premiumValue", Order = 1)]
		[ControlGUI(Name = "Value", Width = 75)]
        public EFS_Decimal premiumValue;
		[System.Xml.Serialization.XmlElementAttribute("premiumQuoteBasis", Order = 2)]
        [ControlGUI(Name = "Basis", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 250)]
        public PremiumQuoteBasisEnum premiumQuoteBasis;
		#endregion Members
	}
    #endregion PremiumQuote

    #region QuotedAs
    /// <summary>
    /// <para><b>Description :</b> A type that describes how the option was quoted.</para>
    /// <para><b>Contents :</b></para>
    /// <para>optionOnCurrency (exactly one occurrence; of the type Currency) Either the callCurrencyAmount or the
    /// putCurrencyAmount defined elsewhere in the document. The currency reference denotes the option currency
    /// as the option was quoted (as opposed to the face currency).</para>
    /// <para>faceOnCurrency (exactly one occurrence; of the type Currency) Either the callCurrencyAmount or the
    /// putCurrencyAmount defined elsewhere in the document.The currency reference denotes the face currency as
    /// the option was quoted (as opposed to the option currency).</para>
    /// <para>quotedTenor (zero or one occurrence; of the type Interval) Code denoting the tenor of the option leg.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: FXOptionLeg</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class QuotedAs : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("optionOnCurrency", Order = 1)]
        [ControlGUI(Name = "OptionOnCurrency", Width = 75)]
        public Currency optionOnCurrency;
		[System.Xml.Serialization.XmlElementAttribute("faceOnCurrency", Order = 2)]
        [ControlGUI(Name = "FaceOnCurrency", Width = 75)]
        public Currency faceOnCurrency;
		[System.Xml.Serialization.XmlElementAttribute("quotedTenor", Order = 3)]
        [ControlGUI(Name = "QuotedTenor", LblWidth = 80)]
        public Interval quotedTenor;
		#endregion Members
	}
    #endregion QuotedAs

    #region SideRate
    /// <summary>
    /// <para><b>Description :</b> A type that is used for describing a particular rate against base currency. 
    /// Exists within SideRates.</para>
    /// <para><b>Contents :</b></para>
    /// <para>currency (exactly one occurrence; of the type Currency) The currency in which an amount is denominated.</para>
    /// <para>sideRateBasis (exactly one occurrence; of the type SideRateBasisEnum) The method by which the exchange
    /// rate against base currency is quoted.</para>
    /// <para>rate (exactly one occurrence; of the type xsd:decimal) The rate of exchange between the two currencies of the
    /// leg of a deal. Must be specified with a quote basis.</para>
    /// <para>spotRate (zero or one occurrence; of the type xsd:decimal) An optional element used for FX forwards and
    /// certain types of FX OTC options. For deals consumated in the FX Forwards Market, this represents the current
    /// market rate for a particular currency pair. For barrier and digital/binary options, it can be useful to include the
    /// spot rate at the time the option was executed to make it easier to know whether the option needs to move "up"
    /// or "down" to be triggered.</para>
    /// <para>forwardPoints (zero or one occurrence; of the type xsd:decimal) An optional element used for deals
    /// consumated in the FX Forwards market. Forward points represent the interest rate differential between the two
    /// currencies traded and are quoted as a preminum or a discount. Forward points are added to, or subtracted
    /// from, the spot rate to create the rate of the forward trade.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: SideRates</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SideRate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
		[ControlGUI(Name = "currency", LblWidth = 190, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public Currency currency;
		[System.Xml.Serialization.XmlElementAttribute("sideRateBasis", Order = 2)]
        [ControlGUI(Name = "rateBasis", LblWidth = 60, Width = 205)]
        public SideRateBasisEnum sideRateBasis;
		[System.Xml.Serialization.XmlElementAttribute("rate", Order = 3)]
        [ControlGUI(Name = "value", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal rate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool spotRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("spotRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "spotRate", LblWidth = 180, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal spotRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forwardPointsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("forwardPoints", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "forwardPoints", LblWidth = 180, LineFeed = MethodsGUI.LineFeedEnum.After, Width = 75)]
        public EFS_Decimal forwardPoints;
		#endregion Members
    }
    #endregion SideRate
    #region SideRates
    /// <summary>
    /// <para><b>Description :</b> A type that is used for including rates against base currency for non-base currency 
    /// FX contracts.</para>
    /// <para><b>Contents :</b></para>
    /// <para>baseCurrency (exactly one occurrence; of the type Currency) The currency that is used as the basis for the
    /// side rates when calculating a cross rate.</para>
    /// <para>currency1SideRate (zero or one occurrence; of the type SideRate) The exchange rate for the first currency of
    /// the trade against base currency.</para>
    /// <para>currency2SideRate (zero or one occurrence; of the type SideRate) The exchange rate for the second
    /// currency of the trade against base currency.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Complex type: ExchangeRate</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public partial class SideRates : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("baseCurrency", Order = 1)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "base currency", Width = 75)]
        public Currency baseCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currency1SideRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency1SideRate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "currency 1")]
        public SideRate currency1SideRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currency2SideRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency2SideRate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "currency 2")]
        public SideRate currency2SideRate;
		#endregion Members
    }
    #endregion SideRates

    #region TermDeposit
    /// <summary>
    /// <para><b>Description :</b> A class defining the content model for a term deposit product.</para>
    /// <para><b>Contents :</b></para>
    /// <para>Inherited element(s): (This definition inherits the content defined by the type Product)</para>
    /// <para>• The base type which all FpML products extend.</para>
    /// <para>initialPayerReference (exactly one occurrence; of the type PartyReference) A pointer style reference to a
    /// party identifier defined elsewhere in the document. The party referenced is the payer of the initial principal of
    /// the deposit on the start date.</para>
    /// <para>initialReceiverReference (exactly one occurrence; of the type PartyReference) A pointer style reference to a
    /// party identifier defined elsewhere in the document. The party is the receiver of the initial principal of the
    /// deposit on the start date.</para>
    /// <para>startDate (exactly one occurrence; of the type xsd:date) The averaging period start date.</para>
    /// <para>maturityDate (exactly one occurrence; of the type xsd:date) The end date of the calculation period. This date
    /// should already be adjusted for any applicable business day convention.</para> 
    /// <para>dayCountFraction (exactly one occurrence; of the type DayCountFractionEnum) The day count fraction.</para>
    /// <para>principal (exactly one occurrence; of the type Money) The principal amount of the trade.</para>
    /// <para>fixedRate (exactly one occurrence; of the type xsd:decimal) The calculation period fixed rate. A per annum
    /// rate, expressed as a decimal. A fixed rate of 5% would be represented as 0.05.</para>
    /// <para>interest (zero or one occurrence; of the type Money) The total interest of at maturity of the trade.</para>
    /// <para>payment (zero or more occurrences; of the type Payment) A known payment between two parties.</para>
    ///</summary>
    ///<remarks>
    ///<para><b>Used by :</b></para>
    ///<para>• Element: termDeposit</para>
    ///<para><b>Derived Types :</b></para>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("termDeposit", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class TermDeposit : Product
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("initialPayerReference", Order = 1)]
        [ControlGUI(Name = "Payer", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public PartyReference initialPayerReference;
		[System.Xml.Serialization.XmlElementAttribute("initialReceiverReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference initialReceiverReference;
		[System.Xml.Serialization.XmlElementAttribute("startDate", Order = 3)]
        [ControlGUI(Name = "Start date", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.Before)]
        public EFS_Date startDate;
		[System.Xml.Serialization.XmlElementAttribute("maturityDate", Order = 4)]
        [ControlGUI(Name = "Maturity date", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.BeforeAndAfter)]
        public EFS_Date maturityDate;
		[System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 5)]
        [ControlGUI(Name = "Day count fraction", LblWidth = 115, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public DayCountFractionEnum dayCountFraction;
		[System.Xml.Serialization.XmlElementAttribute("principal", Order = 6)]
        [ControlGUI(Name = "Amount", LblWidth = 115)]
        public Money principal;
        // FI 20140909 [20340] fixedRate est une donnée obligatoire (voir FpML) 
        //[System.Xml.Serialization.XmlIgnoreAttribute()]
        //[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        //public bool fixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("fixedRate", Order = 7)]
        [ControlGUI(Name = "Fixed rate", LblWidth = 77, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool interestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("interest", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Interest")]
        public Money interest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("payment", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment", IsClonable = true, IsChild = true)]
        [BookMarkGUI(IsVisible = false)]
        public Payment[] payment;
		#endregion Members
    }
    #endregion TermDeposit
}
