#region Using Directives
using System;
using System.Reflection;

using EFS.GUI;
using EFS.GUI.Interface;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;

using EfsML.Enum;

using FpML.Enum;

using FpML.v42.Enum;
using FpML.v42.Main;
using FpML.v42.Doc;
using FpML.v42.Asset;
using FpML.v42.Eqs;
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
namespace FpML.v42.Cd
{
    #region AdditionalTerm
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: additionalTermScheme (xsd:anyURI)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• GeneralTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AdditionalTerm : IEFS_Array
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue additionalTerm;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 500)]
        public string additionalTermScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;

        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion AdditionalTerm
    #region AdjustedPaymentDates
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>adjustedPaymentDate (exactly one occurrence; of the type xsd:date) 
    /// The adjusted payment date. This date should already be adjusted for any applicable business day convention. 
    /// This component is not intended for use in trade confirmation but my be specified to allow the fee structure 
    /// to also serve as a cashflow type component (all dates the the Cashflows type are adjusted payment dates).</newpara>
    /// <newpara>paymentAmount (exactly one occurrence; of the type Money) 
    /// The currency amount of the payment.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• PeriodicPayment</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class AdjustedPaymentDates : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 1)]
		[ControlGUI(Name = "Date")]
        public EFS_Date adjustedPaymentDate;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 2)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money paymentAmount;
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
    #endregion AdjustedPaymentDates

    #region CashSettlementTerms
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type SettlementTerms)</newpara>
    /// <newpara>valuationDate (zero or one occurrence; of the type ValuationDate) 
    /// The number of business days after conditions to settlement have been satisfied when the calculation agent 
    /// obtains a price quotation on the Reference Obligation for purposes of cash settlement. 
    /// There may be one or more valuation dates. This is typically specified if the cash settlement amount 
    /// is not a fixed amount. ISDA 2003 Term: Valuation Date</newpara>
    /// <newpara>valuationTime (zero or one occurrence; of the type BusinessCenterTime) 
    /// The time of day in the specified business center when the calculation agent seeks quotations 
    /// for an amount of the reference obligation for purposes of cash settlement. ISDA 2003 Term: Valuation Time</newpara>
    /// <newpara>quotationMethod (zero or one occurrence; of the type QuotationRateTypeEnum) 
    /// The type of price quotations to be requested from dealers when determining the market value 
    /// of the reference obligation for purposes of cash settlement. 
    /// For example, Bid, Offer or Mid-market. ISDA 2003 Term: Quotation Method</newpara>
    /// <newpara>quotationAmount (zero or one occurrence; of the type Money) 
    /// In the determination of a cash settlement amount, if weighted average quotations are to be obtained, 
    /// the quotation amount specifies an upper limit to the outstanding principal balance of the reference 
    /// obligation for which the quote should be obtained. If not specified, the ISDA definitions provide for 
    /// a fallback amount equal to the floating rate payer calculation amount. ISDA 2003 Term: Quotation Amount</newpara>
    /// <newpara>minimumQuotationAmount (zero or one occurrence; of the type Money) 
    /// In the determination of a cash settlement amount, if weighted average quotations are to be obtained, 
    /// the minimum quotation amount specifies a minimum intended threshold amount of outstanding principal 
    /// balance of the reference obligation for which the quote should be obtained. 
    /// If not specified, the ISDA definitions provide for a fallback amount of the lower of either USD 1,000,000 
    /// (or its equivalent in the relevant obligation currency) or the quotation amount. ISDA 2003 Term: Minimum Quotation Amount</newpara>
    /// <newpara>dealer (zero or more occurrences; of the type xsd:string) 
    /// A dealer from whom quotations are obtained by the calculation agent on the reference obligation for purposes 
    /// of cash settlement. ISDA 2003 Term: Dealer</newpara>
    /// <newpara>cashSettlementBusinessDays (zero or one occurrence; of the type xsd:nonNegativeInteger) 
    /// The number of business days used in the determination of the cash settlement payment date. 
    /// If a cash settlement amount is specified, the cash settlement payment date will be this number of business 
    /// days following the calculation of the final price. If a cash settlement amount is not specified, 
    /// the cash settlement payment date will be this number of business days after all conditions to settlement 
    /// are satisfied. ISDA 2003 Term: Cash Settlement
    /// Date</newpara>
    /// <newpara>cashSettlementAmount (zero or one occurrence; of the type Money) 
    /// The amount paid by the seller to the buyer for cash settlement on the cash settlement date. 
    /// If not otherwise specified, would typically be calculated as 100 (or the Reference Price) minus the price 
    /// of the Reference Obligation (all expressed as a percentage) times Floating Rate Payer Calculation Amount. 
    /// ISDA 2003 Term: Cash Settlement Amount</newpara>
    /// <newpara>accruedInterest (zero or one occurrence; of the type xsd:boolean) 
    /// Indicates whether accrued interest is included (true) or not (false). 
    /// For cash settlement this specifies whether quotations should be obtained inclusive or not of accrued interest. 
    /// For physical settlement this specifies whether the buyer should deliver the obligation with an outstanding 
    /// principal balance that includes or excludes accrued interest. ISDA 2003 Term: Include/Exclude Accrued Interest</newpara>
    /// <newpara>valuationMethod (zero or one occurrence; of the type ValuationMethodEnum) 
    /// The ISDA defined methodology for determining the final price of the reference obligation for purposes of cash settlement. 
    /// (ISDA 2003 Term: Valuation Method). For example, Market, Highest etc.</newpara>
    /// </summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditDefaultSwap</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CashSettlementTerms : SettlementTerms
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation date")]
        public ValuationDate valuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationTimeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationTime", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation time")]
        public BusinessCenterTime valuationTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotationMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("quotationMethod", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quotation method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public QuotationRateTypeEnum quotationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotationAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("quotationAmount", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quotation amount")]
        public Money quotationAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool minimumquotationAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("minimumQuotationAmount", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Minimum quotation amount")]
        public Money minimumQuotationAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dealerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dealer",Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dealer")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dealer", IsClonable = true, IsChild = false)]
        public EFS_StringArray[] dealer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementBusinessDaysSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementBusinessDays", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash settlement business days", Width = 100)]
        public EFS_NonNegativeInteger cashSettlementBusinessDays;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashSettlementAmountSpecified;
		[System.Xml.Serialization.XmlElementAttribute("cashSettlementAmount", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash settlement amount")]
        public Money cashSettlementAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("accruedInterest", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest")]
        public EFS_Boolean accruedInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationMethodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("valuationMethod", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation method", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ValuationMethodEnum valuationMethod;
		#endregion Members
	}
    #endregion CashSettlementTerms
    #region CreditDefaultSwap
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type Product)</newpara>
    /// <newpara>• The base type which all FpML products extend.</newpara>
    /// <newpara>generalTerms (exactly one occurrence; of the type GeneralTerms) 
    /// This element contains all the data that appears in the section entitled "1. 
    /// General Terms" in the 2003 ISDA Credit Derivatives Confirmation.</newpara>
    /// <newpara>feeLeg (exactly one occurrence; of the type FeeLeg) 
    /// This element contains all the terms relevant to defining the fixed amounts/payments per the applicable ISDA definitions.</newpara>
    /// <newpara>protectionTerms (exactly one occurrence; of the type ProtectionTerms) 
    /// This element contains all the terms relevant to defining the applicable floating rate payer calculation amount, 
    /// credit events and associated conditions to settlement, and reference obligations.</newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either </newpara>
    /// <newpara>cashSettlementTerms (exactly one occurrence; of the type CashSettlementTerms) 
    /// This element contains all the ISDA terms relevant to cash settlement for when cash settlement is applicable. 
    /// ISDA 2003 Term: Cash Settlement</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>physicalSettlementTerms (exactly one occurrence; of the type PhysicalSettlementTerms) 
    /// This element contains all the ISDA terms relevant to physical settlement for when physical settlement is applicable. 
    /// ISDA 2003 Term: Physical Settlement</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlRootAttribute("creditDefaultSwap", Namespace = "http://www.fpml.org/2005/FpML-4-2", IsNullable = false)]
    public partial class CreditDefaultSwap : Product
    {
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("generalTerms", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "General Terms", IsVisible = false)]
        public GeneralTerms generalTerms;
		[System.Xml.Serialization.XmlElementAttribute("feeLeg", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "General Terms")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fee", IsVisible = false)]
        public FeeLeg feeLeg;
		[System.Xml.Serialization.XmlElementAttribute("protectionTerms", Order = 3)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Fee")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Protection Terms", IsVisible = false)]
        public ProtectionTerms protectionTerms;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Protection Terms")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = false, Name = "Settlement Terms")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemCashSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashSettlementTerms", typeof(CashSettlementTerms),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public CashSettlementTerms itemCash;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemPhysicalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("physicalSettlementTerms", typeof(PhysicalSettlementTerms),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public PhysicalSettlementTerms itemPhysical;
		#endregion Members
    }
    #endregion CreditDefaultSwap
    #region CreditEventNotice
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>notifyingParty (exactly one occurrence; of the type NotifyingParty) 
    /// Pointer style references to a party identifier defined elsewhere in the document. 
    /// The notifying party is the party that notifies the other party when a credit event has occurred by means of a credit event notice.
    /// If more than one party is referenced as being the notifying party then either party may notify the other of a credit event 
    /// occurring. ISDA 2003 Term: Notifying Party</newpara>
    /// <newpara>businessCenter (zero or one occurrence; of the type BusinessCenter) 
    /// Inclusion of this business center element implies that Greenwich Mean Time in Section 3.3 of the 2003 ISDA Credit Derivatives
    /// Definitions is replaced by the local time of the city indicated by the businessCenter element value.</newpara>
    /// <newpara>publiclyAvailableInformation (zero or one occurrence; of the type PubliclyAvailableInformation) 
    /// A specified condition to settlement. Publicly available information means information that reasonably confirms any of the
    /// facts relevant to determining that a credit event or potential repudiation/moratorium, as applicable, has occurred. 
    /// The ISDA defined list (2003) is the market standard and is considered comprehensive, and a minimum of two differing public 
    /// sources must have published the relevant information, to declare a Credit Event. 
    /// ISDA 2003 Term: Notice of Publicly Available Information Applicable</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditEvents</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CreditEventNotice : ItemGUI
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
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>bankruptcy (zero or one occurrence; of the type Empty) A credit event. 
    /// The reference entity has been dissolved or has become insolvent. 
    /// It also covers events that may be a precursor to insolvency such as instigation of bankruptcy or insolvency proceedings. 
    /// Sovereign trades are not subject to Bankruptcy as "technically" a Sovereign cannot become bankrupt. 
    /// ISDA 2003 Term: Bankruptcy</newpara>
    /// <newpara>failureToPay (zero or one occurrence; of the type FailureToPay) A credit event. 
    /// This credit event triggers, after the expiration of any applicable grace period, 
    /// if the reference entity fails to make due payments in an aggregrate amount of not less than the payment requirement 
    /// on one or more obligations (e.g. a missed coupon payment). ISDA 2003 Term: Failure to Pay</newpara>
    /// <newpara>obligationDefault (zero or one occurrence; of the type Empty) A credit event. 
    /// One or more of the obligations have become capable of being declared due and payable before they would otherwise 
    /// have been due and payable as a result of, or on the basis of, the occurrence of a default, event of default or 
    /// other similar condition or event other than failure to pay. ISDA 2003 Term: Obligation Default</newpara>
    /// <newpara>obligationAcceleration (zero or one occurrence; of the type Empty) A credit event. 
    /// One or more of the obligations have been declared due and payable before they would otherwise have been due and payable as
    /// a result of, or on the basis of, the occurrence of a default, event of default or other similar condition or event
    /// other than failure to pay (preferred by the market over Obligation Default, because more definitive and
    /// encompasses the definition of Obligation Default - this is more favorable to the Seller). 
    /// Subject to the default requirement amount. ISDA 2003 Term: Obligation Acceleration</newpara>
    /// <newpara>repudiationMoratorium (zero or one occurrence; of the type Empty) A credit event. 
    /// The reference entity, or a governmental authority, either refuses to recognise or challenges the validity of one 
    /// or more obligations of the reference entity, or imposes a moratorium thereby postponing payments on one or more 
    /// of the obligations of the reference entity. 
    /// Subject to the default requirement amount. ISDA 2003 Term: Repudiation/Moratorium</newpara>
    /// <newpara>restructuring (zero or one occurrence; of the type Restructuring) A credit event. 
    /// A restructuring is an event that materially impacts the reference entity's obligations, such as an interest 
    /// rate reduction, principal reduction, deferral of interest or principal, change in priority ranking, 
    /// or change in currency or composition of payment. ISDA 2003 Term: Restructuring</newpara>
    /// <newpara>defaultRequirement (zero or one occurrence; of the type Money) 
    /// In relation to certain credit events, serves as a threshold for Obligation Acceleration, Obligation Default, 
    /// Repudiation/Moratorium and Restructuring. Market standard is USD 10,000,000 (JPY 1,000,000,000 for all Japanese Yen trades). 
    /// This is applied on an aggregate or total basis across all Obligations of the Reference Entity. 
    /// Used to prevent technical/operational errors from triggering credit events. ISDA 2003 Term: Default Requirement</newpara>
    /// <newpara>creditEventNotice (zero or one occurrence; of the type CreditEventNotice) 
    /// A specified condition to settlement. An irrevocable written or verbal notice that describes a credit event that has occurred. 
    /// The notice is sent from the notifying party (either the buyer or the seller) to the counterparty. 
    /// It provides information relevant to determining that a credit event has occurred. 
    /// This is typically accompanied by Publicly Available Information. ISDA 2003 Term: Credit Event Notice</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ProtectionTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class CreditEvents : ItemGUI
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
        public bool obligationDefaultSpecified;
		[System.Xml.Serialization.XmlElementAttribute("obligationDefault", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Obligation default")]
        public Empty obligationDefault;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obligationAccelerationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("obligationAcceleration", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Obligation acceleration")]
        public Empty obligationAcceleration;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool repudiationMoratoriumSpecified;
		[System.Xml.Serialization.XmlElementAttribute("repudiationMoratorium", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Repudiation moratorium")]
        public Empty repudiationMoratorium;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool restructuringSpecified;
		[System.Xml.Serialization.XmlElementAttribute("restructuring", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Restructuring")]
        public Restructuring restructuring;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool defaultRequirementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("defaultRequirement", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Default requirement")]
        public Money defaultRequirement;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditEventNoticeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("creditEventNotice", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit event notice")]
        public CreditEventNotice creditEventNotice;
		#endregion Members
	}
    #endregion CreditEvents

    #region DeliverableObligations
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>accruedInterest (zero or one occurrence; of the type xsd:boolean) 
    /// Indicates whether accrued interest is included (true) or not (false). 
    /// For cash settlement this specifies whether quotations should be obtained inclusive or not of accrued interest. 
    /// For physical settlement this specifies whether the buyer should deliver the obligation with an outstanding principal 
    /// balance that includes or excludes accrued interest. ISDA 2003 Term: Include/Exclude Accrued Interest</newpara>
    /// <newpara>category (exactly one occurrence; of the type ObligationCategoryEnum) 
    /// Used in both obligations and deliverable obligations to represent a class or type of securities which apply. 
    /// ISDA 2003 Term: Obligation Category/Deliverable Obligation Category</newpara>
    /// <newpara>notSubordinated (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. An obligation that ranks at least equal with the most 
    /// senior Reference Obligation in priority of payment or, if no Reference Obligation is specified in the related Confirmation, 
    /// the obligations of the Reference Entity that are senior. ISDA 2003 Term: Not Subordinated</newpara>
    /// <newpara>specifiedCurrency (zero or one occurrence; of the type SpecifiedCurrency) 
    /// An obligation and deliverable obligation characteristic. The currency or currencies in which an obligation or deliverable 
    /// obligation must be payable. ISDA 2003 Term: Specified Currency</newpara>
    /// <newpara>notSovereignLender (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Any obligation that is not primarily (majority) owed to a Sovereign 
    /// or Supranational Organization. ISDA 2003 Term: Not Sovereign Lender</newpara>
    /// <newpara>notDomesticCurrency (zero or one occurrence; of the type NotDomesticCurrency) 
    /// An obligation and deliverable obligation characteristic. Any obligation that is payable in any currency other than 
    /// the domestic currency. Domestic currency is either the currency so specified or, if no currency is specified, 
    /// the currency of (a) the reference entity, if the reference entity is a sovereign, or (b) the jurisdiction in which 
    /// the relevant reference entity is organised, if the reference entity is not a sovereign. 
    /// ISDA 2003 Term: Not Domestic Currency</newpara>
    /// <newpara>notDomesticLaw (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. If the reference entity is a Sovereign, this means any obligation 
    /// that is not subject to the laws of the reference entity. If the reference entity is not a sovereign, this means any obligation 
    /// that is not subject to the laws of the jurisdiction of the reference entity. ISDA 2003 Term: Not Domestic Law</newpara>
    /// <newpara>listed (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Indicates whether or not the obligation is quoted, 
    /// listed or ordinarily purchased and sold on an exchange. ISDA 2003 Term: Listed</newpara>
    /// <newpara>notContingent (zero or one occurrence; of the type Empty) 
    /// A deliverable obligation characteristic. In essence Not Contingent means the repayment of principal cannot be dependant
    ///  on a formula/index, i.e. to prevent the risk of being delivered an instrument that may never pay any element of principal, 
    ///  and to ensure that the obligation is interest bearing (on a regular schedule). ISDA 2003 Term: Not Contingent</newpara>
    /// <newpara>notDomesticIssuance (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Any obligation other than an obligation that was intended to be 
    /// offered for sale primarily in the domestic market of the relevant Reference Entity. 
    /// This specifies that the obligation must be an internationally recognized bond. ISDA 2003 Term: Not Domestic Issuance</newpara>
    /// <newpara>assignableLoan (zero or one occurrence; of the type PCDeliverableObligationCharac) 
    /// A deliverable obligation characteristic. A loan that is freely assignable to a bank or financial institution without 
    /// the consent of the Reference Entity or the guarantor, if any, of the loan (or the consent of the applicable borrower if a
    /// Reference Entity is guaranteeing the loan) or any agent. ISDA 2003 Term: Assignable Loan</newpara>
    /// <newpara>consentRequiredLoan (zero or one occurrence; of the type PCDeliverableObligationCharac) 
    /// A deliverable obligation characteristic. A loan that is capable of being assigned with the consent of the Reference Entity or
    /// the guarantor, if any, of the loan or any agent. ISDA 2003 Term: Consent Required Loan</newpara>
    /// <newpara>directLoanParticipation (zero or one occurrence; of the type LoanParticipation) 
    /// A deliverable obligation characteristic. A loan with a participation agreement whereby the buyer is capable of creating, 
    /// or procuring the creation of, a contractual right in favour of the seller that provides the seller with recourse to the 
    /// participation seller for a specified share in any payments due under the relevant loan which are received by the
    /// participation seller. ISDA 2003 Term: Direct Loan Participation</newpara>
    /// <newpara>transferable (zero or one occurrence; of the type Empty) 
    /// A deliverable obligation characteristic. An obligation that is transferable to institutional investors without any contractual,
    /// statutory or regulatory restrictions. ISDA 2003 Term: Transferable</newpara>
    /// <newpara>maximumMaturity (zero or one occurrence; of the type Interval) 
    /// A deliverable obligation characteristic. An obligation that has a remaining maturity from the Physical Settlement Date 
    /// of not greater than the period specified. ISDA 2003 Term: Maximum Maturity</newpara>
    /// <newpara>acceleratedOrMatured (zero or one occurrence; of the type Empty) 
    /// A deliverable obligation characteristic. An obligation at time of default is due to mature and due to be repaid, 
    /// or as a result of downgrade/bankruptcy is due to be repaid as a result of an acceleration clause. 
    /// ISDA 2003 Term: Accelerated or Matured</newpara>
    /// <newpara>notBearer (zero or one occurrence; of the type Empty) 
    /// A deliverable obligation characteristic. Any obligation that is not a bearer instrument. 
    /// This applies to Bonds only and is meant to avoid tax, fraud and security/delivery provisions that can potentially be 
    /// associated with Bearer Bonds. ISDA 2003 Term: Not Bearer</newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either</newpara>
    /// <newpara>fullFaithAndCreditObLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: Full Faith and Credit Obligation Liability</newpara>
    /// <newpara>or</newpara>
    /// <newpara>generalFundObligationLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: General Fund Obligation Liability</newpara>
    /// <newpara>or</newpara>
    /// <newpara>revenueObligationLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: Revenue Obligation Liability</newpara>
    /// <newpara>indirectLoanParticipation (zero or one occurrence; of the type LoanParticipation) 
    /// ISDA 1999 Term: Indirect Loan Participation. NOTE: Only applicable as a deliverable obligation under ISDA Credit 1999.</newpara>
    /// <newpara>excluded (zero or one occurrence; of the type xsd:string) 
    /// A free format string to specify any excluded obligations or deliverable obligations, as the case may be, 
    /// of the reference entity or excluded types of obligations or deliverable obligations. 
    /// ISDA 2003 Term: Excluded Obligations/Excluded Deliverable Obligations</newpara>
    /// <newpara>othReferenceEntityObligations (zero or one occurrence; of the type xsd:string) 
    /// This element is used to specify any other obligations of a reference entity in both obligations and deliverable obligations. 
    /// The obligations can be specified free-form. ISDA 2003 Term: Other Obligations of a Reference Entity</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• PhysicalSettlementTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class DeliverableObligations : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestSpecified;
		[System.Xml.Serialization.XmlElementAttribute("accruedInterest", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest")]
        public EFS_Boolean accruedInterest;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool categorySpecified;
		[System.Xml.Serialization.XmlElementAttribute("category", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Category", Width = 250)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ObligationCategoryEnum category;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSubordinatedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notSubordinated", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Characteristics", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not subordinated")]
        public Empty notSubordinated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specifiedCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("specifiedCurrency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specified currency")]
        public SpecifiedCurrency specifiedCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSovereignLenderSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notSovereignLender", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not sovereign lender")]
        public Empty notSovereignLender;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticCurrency", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Not domestic currency")]
        public NotDomesticCurrency notDomesticCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticLawSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticLaw", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic law")]
        public Empty notDomesticLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool listedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("listed", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Listed")]
        public Empty listed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notContingentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notContingent", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not contingent")]
        public Empty notContingent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticIssuanceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticIssuance", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic issuance")]
        public Empty notDomesticIssuance;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool assignableLoanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("assignableLoan", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Assignable loan")]
        public PCDeliverableObligationCharac assignableLoan;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool consentRequiredLoanSpecified;
		[System.Xml.Serialization.XmlElementAttribute("consentRequiredLoan", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Consent required loan")]
        public PCDeliverableObligationCharac consentRequiredLoan;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool directLoanParticipationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("directLoanParticipation", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Direct loan participation")]
        public LoanParticipation directLoanParticipation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool transferableSpecified;
		[System.Xml.Serialization.XmlElementAttribute("transferable", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Transferable")]
        public Empty transferable;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maximumMaturitySpecified;
		[System.Xml.Serialization.XmlElementAttribute("maximumMaturity", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maximum maturity")]
        public Interval maximumMaturity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool acceleratedOrMaturedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("acceleratedOrMatured", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Accelerated or matured")]
        public Empty acceleratedOrMatured;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notBearerSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notBearer", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not bearer")]
        public Empty notBearer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = false, Name = "Obligation liability")]
        public EFS_RadioChoice liability;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityNone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityGeneralFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("generalFundObligationLiability", typeof(Empty),Order=18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityGeneralFund;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityFullFaithAndCreditSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullFaithAndCreditObLiability", typeof(Empty),Order=19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityFullFaithAndCredit;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityRevenueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("revenueObligationLiability", typeof(Empty),Order=20)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityRevenue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indirectLoanParticipationSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indirectLoanParticipation", Order=21)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Characteristics")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Indirect loan participation")]
        public LoanParticipation indirectLoanParticipation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("excluded", Order = 22)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Excluded")]
        public EFS_MultiLineString excluded;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool othReferenceEntityObligationsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("othReferenceEntityObligations", Order = 23)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Other reference entity Obligations")]
        public EFS_MultiLineString othReferenceEntityObligations;
		#endregion Members
		#region Constructors
		public DeliverableObligations()
        {
            liabilityNone = new Empty();
            liabilityFullFaithAndCredit = new Empty();
            liabilityGeneralFund = new Empty();
            liabilityRevenue = new Empty();
		}
		#endregion Constructors
	}
    #endregion DeliverableObligations

    #region EntityType
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: entityTypeScheme (xsd:anyURI)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ReferencePair</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class EntityType : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string entityTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion EntityName

    #region FailureToPay
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>gracePeriodExtension (zero or one occurrence; of the type GracePeriodExtension) 
    /// If this element is specified, indicates whether or not a grace period extension is applicable. 
    /// ISDA 2003 Term: Grace Period Extension Applicable</newpara>
    /// <newpara>paymentRequirement (zero or one occurrence; of the type Money) 
    /// Specifies a threshold for the failure to pay credit event. 
    /// Market standard is USD 1,000,000 (JPY 100,000,000 for Japanese Yen trades) or its equivalent in the relevant obligation currency.
    /// This is applied on an aggregate basis across all Obligations of the Reference Entity. Intended to prevent technical/operational
    /// errors from triggering credit events. ISDA 2003 Term: Payment Requirement</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditEvents</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FailureToPay : ItemGUI
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
    #region FeeLeg
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>initialPayment (zero or one occurrence; of the type InitialPayment) 
    /// Specifies a single fixed payment that is payable by the payer to the receiver on the initial payment date. 
    /// The fixed payment to be paid is specified in terms of a known currency amount. This element only should be used for 
    /// CDS Index trades. Upfront payments on Single Name CDS trades should use the singlePayment element</newpara>
    /// <newpara>singlePayment (zero or more occurrences; of the type SinglePayment) 
    /// Specifies a single fixed amount that is payable by the buyer to the seller on the fixed rate payer payment date. 
    /// The fixed amount to be paid is specified in terms of a known currency amount. The adjustable payment date. 
    /// ISDA 2003 Term:</newpara>
    /// <newpara>periodicPayment (zero or one occurrence; of the type PeriodicPayment) 
    /// Specifies a periodic schedule of fixed amounts that are payable by the buyer to the seller on the fixed rate payer payment dates.
    /// The fixed amount to be paid on each payment date can be specified in terms of a known currency amount or as an amount calculated 
    /// on a formula basis by reference to a per annum fixed rate. The applicable business day convention and business day for 
    /// adjusting any fixed rate payer payment date if it would otherwise fall on a day that is not a business day are those specified 
    /// in the dateAdjustments element within the generalTerms component. ISDA 2003 Term:</newpara>
    /// <newpara>marketFixedRate (zero or one occurrence; of the type xsd:decimal)
    /// An optional element that only has meaning in a credit index trade. This element contains the credit spread ("fair value") at 
    /// which the trade was executed. Unlike the fixedRate of an index, the marketFixedRate varies over the life of the index depending 
    /// on market conditions. The marketFixedRate is the price of the index as quoted by trading desks</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditDefaultSwap</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FeeLeg : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool initialPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("initialPayment", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial payment")]
        public InitialPayment initialPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool singlePaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singlePayment",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Single Payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Single Payment", IsClonable = true, IsChild = true)]
        public SinglePayment[] singlePayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool periodicPaymentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("periodicPayment", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Periodic Payment")]
        public PeriodicPayment periodicPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool marketFixedRateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("marketFixedRate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Market fixed rate")]
        public EFS_Decimal marketFixedRate;
		#endregion Members
	}
    #endregion FeeLeg
    #region FixedAmountCalculation
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>calculationAmount (zero or one occurrence; of the type Money) 
    /// The notional amount used in the calculation of fixed amounts where an amount is calculated on a formula basis, 
    /// i.e. fixed amount = fixed rate payer calculation amount x fixed rate x fixed rate day count fraction. 
    /// ISDA 2003 Term: Fixed Rate Payer Calculation Amount.</newpara>
    /// <newpara>fixedRate (exactly one occurrence; of the type xsd:decimal) 
    /// The calculation period fixed rate. A per annum rate, expressed as a decimal. 
    /// A fixed rate of 5% would be represented as 0.05.</newpara>
    /// <newpara>dayCountFraction (zero or one occurrence; of the type DayCountFractionEnum) 
    /// The day count fraction. ISDA 2003 Term: Fixed Rate Day Count Fraction.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• PeriodicPayment</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class FixedAmountCalculation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("calculationAmount", Order = 1)]
		[ControlGUI(Name = "Amount")]
        public Money calculationAmount;
		[System.Xml.Serialization.XmlElementAttribute("fixedRate", Order = 2)]
        [ControlGUI(Name = "Market fixed rate")]
        public EFS_Decimal fixedRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction",Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public DayCountFractionEnum dayCountFraction;
		#endregion Members
	}
    #endregion FixedAmountCalculation

    #region GeneralTerms
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>effectiveDate (exactly one occurrence; of the type AdjustableDate2) 
    /// The first day of the term of the trade. This day may be subject to adjustment in accordance with a business day convention. 
    /// ISDA 2003 Term: Effective Date.</newpara>
    /// <newpara>scheduledTerminationDate (exactly one occurrence; of the type ScheduledTerminationDate) 
    /// The scheduled date on which the credit protection will lapse. May be specified as an adjusting or non-adjusting date or
    /// alternatively as a period offset from the effective date. ISDA 2003 Term: Scheduled Termination Date</newpara>
    /// <newpara>sellerPartyReference (exactly one occurrence; of the type PartyReference) 
    /// The seller of the credit protection. ISDA 2003 Term: Floating Rate Payer.</newpara>
    /// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference) 
    /// The buyer of the credit protection. ISDA 2003 Term: Fixed Rate Payer.</newpara>
    /// <newpara>dateAdjustments (zero or one occurrence; of the type BusinessDayAdjustments) 
    /// ISDA 2003 Terms: Business Day and Business Day Convention.</newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>referenceInformation (exactly one occurrence; of the type ReferenceInformation) 
    /// This element contains all the terms relevant to defining the reference entity and reference obligation(s).</newpara>
    /// <newpara>or</newpara>
    /// <newpara>indexReferenceInformation (exactly one occurrence; of the type IndexReferenceInformation)
    /// This element contains all the terms relevant to defining the Credit DefaultSwap Index</newpara>
    /// <newpara>additionalTerm (zero or more occurrences; of the type AdditionalTerm) 
    /// This element is used for representing information contained in the Additional Terms field of the 2003 Master Credit Derivatives
    /// confirm.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditDefaultSwap</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class GeneralTerms : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Effective Date")]
        public AdjustableDate2 effectiveDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool scheduledTerminationDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("scheduledTerminationDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Scheduled Termination Date")]
        public ScheduledTerminationDate scheduledTerminationDate;
		[System.Xml.Serialization.XmlElementAttribute("sellerPartyReference", Order = 3)]
        [ControlGUI(Name = "Floating rate payer (the 'Seller')")]
        public PartyReference sellerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("buyerPartyReference", Order = 4)]
        [ControlGUI(Name = "Fixed rate payer (the 'Buyer')")]
        [ControlGUI(Name = "Buyer")]
        public PartyReference buyerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dateAdjustmentsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("dateAdjustments", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business Day Adjustments")]
        public BusinessDayAdjustments dateAdjustments;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information Terms", IsVisible = false)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemIndexReferenceInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexReferenceInformation", typeof(IndexReferenceInformation),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public IndexReferenceInformation itemIndexReferenceInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceInformationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceInformation", typeof(ReferenceInformation),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ReferenceInformation itemReferenceInformation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool additionalTermSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalTerm",Order=8)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Information Terms")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Additional Terms")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Additional Terms", IsClonable = true, IsChild = false)]
        public AdditionalTerm[] additionalTerm;
		#endregion Members
		#region Constructors
		public GeneralTerms()
        {
            itemIndexReferenceInformation = new IndexReferenceInformation();
            itemReferenceInformation = new ReferenceInformation();
		}
		#endregion Constructors
	}
    #endregion GeneralTerms
    #region GracePeriodExtension
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>gracePeriod (zero or one occurrence; of the type Offset) 
    /// The number of calendar or business days after any due date that the reference entity has to fulfil its obligations before a 
    /// failure to pay credit event is deemed to have occurred. ISDA 2003 Term: Grace Period</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FailureToPay</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class GracePeriodExtension : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("gracePeriod", Order = 1)]
        public Offset gracePeriod;
		#endregion Members
	}
    #endregion GracePeriodExtension

    #region IndexAnnexSource
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: indexAnnexSourceScheme (xsd:anyURI) </newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• IndexReferenceInformation</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndexAnnexSource : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string indexAnnexSourceScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;

        public IndexAnnexSource()
        {
            indexAnnexSourceScheme = "http://www.fpml.org/coding-scheme/cdx-index-annex-source-1-0";
        }
    }
    #endregion IndexAnnexSource
    #region IndexId
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: indexIdScheme (xsd:anyURI) </newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• IndexReferenceInformation</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndexId : ItemGUI, IEFS_Array
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue indexId;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string indexIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;

        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion IndexId
    #region IndexName
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    /// <newpara>Attribute: indexNameScheme (xsd:anyURI) </newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• IndexReferenceInformation</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndexName : ItemGUI
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(IsLabel = false, Name = null, LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_SchemeValue indexName;

        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.ValidatorOptional)]
        [ControlGUI(Name = "scheme", Width = 300)]
        public string indexNameScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(Name = "value", Width = 200)]
        public string Value;
    }
    #endregion IndexName
    #region IndexReferenceInformation
    /// <summary>
    /// <newpara><b>Description :</b>A type defining a Credit Default Swap Index</newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure;</newpara>
    /// <newpara>indexSeries (zero or one occurrence; of the type xsd:integer)
    /// A CDS index series identifier, e.g. 1, 2, 3 etc</newpara>
    /// <newpara>indexAnnexVersion (zero or one occurrence; of the type xsd:integer)
    /// A CDS index series version identifier, e.g. 1, 2, 3 etc.</newpara>
    /// <newpara>indexAnnexDate (zero or one occurrence; of the type xsd:date) A CDS index series annex date</newpara>
    /// <newpara>indexAnnexSource (zero or one occurrence; of the type IndexAnnexSource) A CDS index series annex source</newpara>
    /// <newpara>excludedReferenceEntity (zero or more occurrences; of the type LegalEntity) Excluded reference entity</newpara>
    /// <newpara>Attribute: id (xsd:ID)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• GeneralTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>

    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class IndexReferenceInformation : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexNameSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexName", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index name")]
        public IndexName indexName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indexId",Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Id")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Index Id", IsClonable = true)]
        public IndexId[] indexId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexSeriesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexSeries", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series identifier")]
        public EFS_Integer indexSeries;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexVersionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexAnnexVersion", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series version identifier")]
        public EFS_Integer indexAnnexVersion;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexAnnexDate", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series annex date")]
        public EFS_Date indexAnnexDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indexAnnexSourceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("indexAnnexSource", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CDS index series annex source", Width = 200)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public IndexAnnexSource indexAnnexSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedReferenceEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("excludedReferenceEntity",Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Excluded reference entity")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Excluded reference entity", IsClonable = true, IsChild = true)]
        public LegalEntity[] excludedReferenceEntity;

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
		#region Constructors
		public IndexReferenceInformation()
        {
            indexAnnexDate = new EFS_Date();
		}
		#endregion Constructors
	}
    #endregion IndexReferenceInformation
    #region InitialPayment
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>payerPartyReference (exactly one occurrence; of the type PartyReference)
    /// A reference to the party responsible for making the payments defined by this structure</newpara>
    /// <newpara>receiverPartyReference (exactly one occurrence; of the type PartyReference)
    /// A reference to the party that receives the payments corresponding to this structure</newpara>
    /// <newpara>adjustablePaymentDate (zero or one occurrence; of the type xsd:date)
    /// A fixed payment date that shall be subject to adjustment in accordance with the applicable business day convention if it would
    /// otherwise fall on a day that is not a business day. The applicable business day convention and business day are those specified
    /// in the dateAdjustments element within the generalTerms component</newpara>
    /// <newpara>adjustedPaymentDate (zero or one occurrence; of the type xsd:date)
    /// The adjusted payment date. This date should already be adjusted for any applicable business day convention. 
    /// This component is not intended for use in trade confirmation but may be specified to allow the fee structure to also serve 
    /// as a cashflow type component</newpara>
    /// <newpara>paymentAmount (exactly one occurrence; of the type Money)
    /// A fixed payment amount</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FeeLeg</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class InitialPayment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("payerPartyReference", Order = 1)]
		[ControlGUI(Name = "Payer")]
        public PartyReference payerPartyReference;
		[System.Xml.Serialization.XmlElementAttribute("receiverPartyReference", Order = 2)]
        [ControlGUI(Name = "Receiver")]
        public PartyReference receiverPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustablePaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjustable payment date")]
        public EFS_Date adjustablePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;
		[System.Xml.Serialization.XmlElementAttribute("paymentAmount", Order = 5)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment amount", IsVisible = false)]
        public Money paymentAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment amount")]
        public bool FillBalise;
		#endregion Members
		#region Constructors
		public InitialPayment()
        {
            adjustablePaymentDate = new EFS_Date();
            adjustedPaymentDate = new EFS_Date();
        }
        #endregion Constructors
    }
    #endregion InitialPayment

    #region LoanParticipation
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type PCDeliverableObligationCharac)</newpara>
    /// <newpara>qualifyingParticipationSeller (zero or one occurrence; of the type xsd:string) 
    /// If Direct Loan Participation is specified as a deliverable obligation characteristic, this specifies any requirements 
    /// for the Qualifying Participation Seller. 
    /// The requirements may be listed free-form. ISDA 2003 Term: Qualifying Participation Seller</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• DeliverableObligations</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class LoanParticipation : PCDeliverableObligationCharac
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool qualifyingParticipationSellerSpecified;
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[System.Xml.Serialization.XmlElementAttribute("qualifyingParticipationSeller", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Qualifying participation seller")]
        public EFS_MultiLineString qualifyingParticipationSeller;
		#endregion Members
	}
    #endregion LoanParticipation

    #region MultipleValuationDates
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type SingleValuationDate)</newpara>
    /// <newpara>businessDaysThereafter (zero or one occurrence; of the type xsd:positiveInteger) 
    /// The number of business days between successive valuation dates when multiple valuation dates are applicable for cash settlement.
    /// ISDA 2003 Term: Business Days thereafter</newpara>
    /// <newpara>numberValuationDates (zero or one occurrence; of the type xsd:positiveInteger) 
    /// Where multiple valuation dates are specified as being applicable for cash settlement, this element specifies (a) the number 
    /// of applicable valuation dates, and (b) the number of business days after satisfaction of all conditions to settlement when 
    /// the first such valuation date occurs, and (c) the number of business days thereafter of each successive valuation date. 
    /// ISDA 2003 Term: Multiple Valuation Dates</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ValuationDate</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class MultipleValuationDates : SingleValuationDate
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessDaysThereafterSpecified;
		[System.Xml.Serialization.XmlElementAttribute("businessDaysThereafter", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business days there after")]
        public EFS_PosInteger businessDaysThereafter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberValuationDatesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("numberValuationDates", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number valuation dates")]
        public EFS_PosInteger numberValuationDates;
		#endregion Members
	}
    #endregion MultipleValuationDates

    #region NotDomesticCurrency
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>currency (zero or one occurrence; of the type Currency) 
    /// An explicit specification of the domestic currency.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• DeliverableObligations</newpara>
    ///<newpara>• Obligations</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class NotDomesticCurrency : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
		#endregion Members
	}
    #endregion NotDomesticCurrency
    #region NotifyingParty
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>buyerPartyReference (exactly one occurrence; of the type PartyReference)</newpara>
    /// <newpara>sellerPartyReference (zero or one occurrence; of the type PartyReference)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditEventNotice</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class NotifyingParty
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

    #region Obligations
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>category (exactly one occurrence; of the type ObligationCategoryEnum) 
    /// Used in both obligations and deliverable obligations to represent a class or type of securities which apply. 
    /// ISDA 2003 Term: Obligation Category/Deliverable Obligation Category</newpara>
    /// <newpara>notSubordinated (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. An obligation that ranks at least equal with the most senior 
    /// Reference Obligation in priority of payment or, if no Reference Obligation is specified in the related Confirmation, 
    /// the obligations of the Reference Entity that are senior. ISDA 2003 Term: Not Subordinated</newpara>
    /// <newpara>specifiedCurrency (zero or one occurrence; of the type SpecifiedCurrency) 
    /// An obligation and deliverable obligation characteristic. The currency or currencies in which an obligation or 
    /// deliverable obligation must be payable. ISDA 2003 Term: Specified Currency</newpara>
    /// <newpara>notSovereignLender (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Any obligation that is not primarily (majority) owed 
    /// to a Sovereign or Supranational Organization. ISDA 2003 Term: Not Sovereign Lender</newpara>
    /// <newpara>notDomesticCurrency (zero or one occurrence; of the type NotDomesticCurrency) 
    /// An obligation and deliverable obligation characteristic. Any obligation that is payable in any currency other than 
    /// the domestic currency. Domestic currency is either the currency so specified or, if no currency is specified, 
    /// the currency of (a) the reference entity, if the reference entity is a sovereign, or (b) the jurisdiction in which 
    /// the relevant reference entity is organised, if the reference entity is not a sovereign. 
    /// ISDA 2003 Term: Not Domestic Currency</newpara>
    /// <newpara>notDomesticLaw (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. If the reference entity is a Sovereign, this means any obligation 
    /// that is not subject to the laws of the reference entity. If the reference entity is not a sovereign, this means any 
    /// obligation that is not subject to the laws of the jurisdiction of the reference entity. 
    /// ISDA 2003 Term: Not Domestic Law</newpara>
    /// <newpara>listed (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Indicates whether or not the obligation is quoted, 
    /// listed or ordinarily purchased and sold on an exchange. ISDA 2003 Term: Listed</newpara>
    /// <newpara>notDomesticIssuance (zero or one occurrence; of the type Empty) 
    /// An obligation and deliverable obligation characteristic. Any obligation other than an obligation that was intended 
    /// to be offered for sale primarily in the domestic market of the relevant Reference Entity. 
    /// This specifies that the obligation must be an internationally recognized bond. ISDA 2003 Term: Not Domestic Issuance</newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either </newpara>
    /// <newpara>fullFaithAndCreditObLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: Full Faith and Credit Obligation Liability </newpara>
    /// <newpara>or</newpara>
    /// <newpara>generalFundObligationLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: General Fund Obligation Liability </newpara>
    /// <newpara>or</newpara>
    /// <newpara>revenueObligationLiability (exactly one occurrence; of the type Empty)
    /// An obligation and deliverable obligation characteristic. Defined in the ISDA published additional provisions for U.S. 
    /// Municipal as Reference Entity. ISDA 2003 Term: Revenue Obligation Liability </newpara>
    /// <newpara>notContingent (zero or one occurrence; of the type Empty) 
    /// NOTE: Only allowed as an obligation charcteristic under ISDA Credit 1999. In essence Not Contingent means the repayment 
    /// of principal cannot be dependant on a formula/index, i.e. to prevent the risk of being delivered an instrument that may 
    /// never pay any element of principal, and to ensure that the obligation is interest bearing (on a regular schedule). 
    /// ISDA 2003 Term: Not Contingent</newpara>
    /// <newpara>excluded (zero or one occurrence; of the type xsd:string) 
    /// A free format string to specify any excluded obligations or deliverable obligations, as the case may be, 
    /// of the reference entity or excluded types of obligations or deliverable obligations. 
    /// ISDA 2003 Term: Excluded Obligations/Excluded Deliverable Obligations</newpara>
    /// <newpara>othReferenceEntityObligations (zero or one occurrence; of the type xsd:string) 
    /// This element is used to specify any other obligations of a reference entity in both obligations and deliverable obligations. 
    /// The obligations can be specified free-form. ISDA 2003 Term: Other Obligations of a Reference Entity</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ProtectionTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Obligations : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("category", Order = 1)]
		[ControlGUI(Name = "Category", Width = 200)]
        public ObligationCategoryEnum category;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSubordinatedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notSubordinated", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not subordinated")]
        public Empty notSubordinated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool specifiedCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("specifiedCurrency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Specified currency")]
        public SpecifiedCurrency specifiedCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notSovereignLenderSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notSovereignLender", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not sovereign lender")]
        public Empty notSovereignLender;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticCurrency", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Not domestic currency")]
        public NotDomesticCurrency notDomesticCurrency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticLawSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticLaw", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic law")]
        public Empty notDomesticLaw;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool listedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("listed", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Listed")]
        public Empty listed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notDomesticIssuanceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notDomesticIssuance", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not domestic issuance")]
        public Empty notDomesticIssuance;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = false, Name = "Obligation liability")]
        public EFS_RadioChoice liability;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityGeneralFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("generalFundObligationLiability", typeof(Empty),Order=9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityGeneralFund;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityFullFaithAndCreditSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullFaithAndCreditObLiability", typeof(Empty),Order=10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityFullFaithAndCredit;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool liabilityRevenueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("revenueObligationLiability", typeof(Empty),Order=11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty liabilityRevenue;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notContingentSpecified;
		[System.Xml.Serialization.XmlElementAttribute("notContingent", Order = 12)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Not contingent")]
        public Empty notContingent;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool excludedSpecified;
		[System.Xml.Serialization.XmlElementAttribute("excluded", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Excluded")]
        public EFS_MultiLineString excluded;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool othReferenceEntityObligationsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("othReferenceEntityObligations", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Other reference entity Obligations")]
        public EFS_MultiLineString othReferenceEntityObligations;
		#endregion Members

		#region Constructors
		public Obligations()
        {
            liabilityNone = new Empty();
            liabilityFullFaithAndCredit = new Empty();
            liabilityGeneralFund = new Empty();
            liabilityRevenue = new Empty();
        }
        #endregion Constructors
    }
    #endregion Obligations

    #region PCDeliverableObligationCharac
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>partialCashSettlement (zero or one occurrence; of the type Empty) 
    /// Specifies whether either 'Partial Cash Settlement of Assignable Loans', 'Partial Cash Settlement of Consent Required Loans' 
    /// or 'Partial Cash Settlement of Participations' is applicable. 
    /// If this element is specified and Assignable Loan is a Deliverable Obligation Chracteristic, any Assignable Loan that 
    /// is deliverable, but where a non-receipt of Consent by the Physical Settlement Date has occurred, the Loan can be cash 
    /// settled rather than physically delivered. If this element is specified and Consent Required Loan is a Deliverable Obligation 
    /// Characterisitc, any Consent Required Loan that is deliverable, but where a non-receipt of Consent by the Physical Settlement Date has
    /// occurred, the Loan can be cash settled rather than physically delivered. If this element is specified and Direct
    /// Loan Participation is a Deliverable Obligation Characterisitic, any Participation that is deliverable, but where this participation 
    /// has not been effected (has not come into effect) by the Physical Settlement Date, the participation can be cash settled rather than
    /// physically delivered.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• DeliverableObligations</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(LoanParticipation))]
    public class PCDeliverableObligationCharac : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool partialCashSettlementSpecified;
		[System.Xml.Serialization.XmlElementAttribute("partialCashSettlement", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, Name = "Partial cash settlement")]
        public Empty partialCashSettlement;
		#endregion Members
	}
    #endregion PCDeliverableObligationCharac
    #region PeriodicPayment
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>paymentFrequency (exactly one occurrence; of the type Interval)
    /// The time interval between regular fixed rate payer payment dates.</newpara>
    /// <newpara>firstPeriodStartDate (zero or one occurrence; of the type xsd:date)
    /// The start date of the initial calculation period if such date is not equal to the trade’s effective date. 
    /// It must only be specified if it is not equal to the effective date. The applicable business day convention and 
    /// business day are those specified in the dateAdjustments element within the generalTerms component 
    /// (or in a transaction supplement FpML representation defined within the referenced general terms confirmation agreement).</newpara>
    /// <newpara>firstPaymentDate (zero or one occurrence; of the type xsd:date) 
    /// The first unadjusted fixed rate payer payment date. The applicable business day convention and business day are 
    /// those specified in the dateAdjustments element within the generalTerms component (or in a transaction supplement FpML
    /// representation defined within the referenced general terms confirmation agreement). 
    /// ISDA 2003 Term: Fixed Rate Payer Payment Date</newpara>
    /// <newpara>lastRegularPaymentDate (zero or one occurrence; of the type xsd:date) 
    /// The last regular unadjusted fixed rate payer payment date. The applicable business day convention and business day are those
    /// specified in the dateAdjustments element within the generalTerms component (or in a transaction supplement FpML representation
    /// defined within the referenced general terms confirmation agreement). This element should only be included if there is a final 
    /// payment stub, i.e. where the last regular unadjusted fixed rate payer payment date is not equal to the scheduled termination date.
    /// ISDA 2003 Term: Fixed Rate Payer Payment Date</newpara>
    /// <newpara>rollConvention (zero or one occurrence; of the type RollConventionEnum)
    /// Used in conjunction with the effectiveDate, scheduledTerminationDate, firstPaymentDate, lastRegularPaymentDate and
    /// paymentFrequency to determine the regular fixed rate payer payment dates.</newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>fixedAmount (exactly one occurrence; of the type Money) 
    /// A fixed payment amount. ISDA 2003 Term: Fixed Amount</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>fixedAmountCalculation (exactly one occurrence; of the type FixedAmountCalculation) 
    /// This element contains all the terms relevant to calculating a fixed amount where the fixed amount is calculated by reference
    /// to a per annum fixed rate. There is no corresponding ISDA 2003 Term. The equivalent is Sec 5.1 "Calculation of Fixed Amount" 
    /// but this in itself is not a defined Term.</newpara>
    /// <newpara>adjustedPaymentDates (zero or more occurrences; of the type AdjustedPaymentDates) 
    /// An optional cashflow-like structure allowing the equivalent representation of the periodic fixed payments in terms of a
    /// series of adjusted payment dates and amounts. This is intended to support application integration within an organisation and 
    /// is not intended for use in inter-firm communication or confirmations. 
    /// ISDA 2003 Term: Fixed Rate Payer Payment Date</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FeeLeg</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PeriodicPayment : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPeriodStartDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("firstPeriodStartDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First period start date")]
        public EFS_Date firstPeriodStartDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool firstPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("firstPaymentDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "First payment date")]
        public EFS_Date firstPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lastRegularPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("lastRegularPaymentDate", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Last regular payment date")]
        public EFS_Date lastRegularPaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rollConventionSpecified;
		[System.Xml.Serialization.XmlElementAttribute("rollConvention", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Roll convention")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public RollConventionEnum rollConvention;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Fixed amount")]
        public EFS_RadioChoice fixedAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedAmountDefineSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedAmount", typeof(Money),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Money fixedAmountDefine;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fixedAmountCalculationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fixedAmountCalculation", typeof(FixedAmountCalculation),Order=7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public FixedAmountCalculation fixedAmountCalculation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDatesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDates",Order=8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment dates")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment dates", IsClonable = true, IsChild = false, MinItem = 0)]
        public AdjustedPaymentDates[] adjustedPaymentDates;
		#endregion Members
		#region Constructors
		public PeriodicPayment()
        {
            firstPeriodStartDate = new EFS_Date();
            firstPaymentDate = new EFS_Date();
            lastRegularPaymentDate = new EFS_Date();

            fixedAmountDefine = new Money();
            fixedAmountCalculation = new FixedAmountCalculation();
        }
        #endregion Constructors
    }
    #endregion PeriodicPayment
    #region PhysicalSettlementPeriod
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>businessDaysNotSpecified (exactly one occurrence; of the type Empty) 
    /// An explicit indication that a number of business days are not specified and therefore ISDA fallback provisions should apply.
    /// </newpara>
    /// <newpara>Or</newpara>
    /// <newpara>businessDays (exactly one occurrence; of the type xsd:nonNegativeInteger) 
    /// A number of business days. Its precise meaning is dependant on the context in which this element is used. 
    /// ISDA 2003 Term: Business Day</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>maximumBusinessDays (exactly one occurrence; of the type xsd:nonNegativeInteger) 
    /// A maximum number of business days. Its precise meaning is dependant on the context in which this element is used. 
    /// Intended to be used to limit a particular ISDA fallback provision.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• PhysicalSettlementTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PhysicalSettlementPeriod : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNotBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDaysNotSpecified", typeof(Empty),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNotBusinessDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessDays", typeof(EFS_NonNegativeInteger),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 70)]
        public EFS_NonNegativeInteger itemBusinessDays;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMaximumBusinessDaysSpecified;
        [System.Xml.Serialization.XmlElementAttribute("maximumBusinessDays", typeof(EFS_NonNegativeInteger),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Name = "value", Width = 70)]
        public EFS_NonNegativeInteger itemMaximumBusinessDays;
		#endregion Members
		#region Constructors
		public PhysicalSettlementPeriod()
        {
            itemNotBusinessDays = new Empty();
            itemBusinessDays = new EFS_NonNegativeInteger();
            itemMaximumBusinessDays = new EFS_NonNegativeInteger();
		}
		#endregion Constructors
	}
    #endregion PhysicalSettlementPeriod
    #region PhysicalSettlementTerms
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type SettlementTerms)</newpara>
    /// <newpara>physicalSettlementPeriod (zero or one occurrence; of the type PhysicalSettlementPeriod) 
    /// The number of business days used in the determination of the physical settlement date. 
    /// The physical settlement date is this number of business days after all applicable conditions to settlement are satisfied. 
    /// If a number of business days is not specified fallback provisions apply for determining the number of business days. 
    /// If Section 8.5/8.6 of the 1999/2003 ISDA Definitions are to apply the businessDaysNotSpecified element should be included. 
    /// If a specified number of business days are to apply these should be specified in the businessDays element. 
    /// If Section 8.5/8.6 of the 1999/2003 ISDA Definitions are to apply but capped at a maximum number of business days then the 
    /// maximum number should be specified in the maximumBusinessDays element. ISDA 2003 Term: Physical Settlement Period</newpara>
    /// <newpara>deliverableObligations (exactly one occurrence; of the type DeliverableObligations) 
    /// This element contains all the ISDA terms relevant to defining the deliverable obligations.</newpara>
    /// <newpara>escrow (zero or one occurrence; of the type xsd:boolean) 
    /// If this element is specified, indicates that physical settlement must take place through the use of an escrow agent. 
    /// (For Canadian counterparties this is always "Not Applicable". ISDA 2003 Term: Escrow</newpara>
    /// <newpara>sixtyBusinessDaySettlementCap (zero or one occurrence; of the type xsd:boolean) 
    /// If this element is specified, for a transaction documented under the 2003 ISDA Credit Derivatives Definitions, 
    /// has the effect of incorporating the language set forth below into the confirmation. The section references are to the 2003 ISDA
    /// Credit Derivatives Definitions. Notwithstanding Section 1.7 or any provisions of Sections 9.9 or 9.10 to the contrary, 
    /// but without prejudice to Section 9.3 and (where applicable) Sections 9.4, 9.5 and 9.6, if the Termination Date has not occurred 
    /// on or prior to the date that is 60 Business Days following the Physical Settlement Date, such 60th Business Day shall be deemed 
    /// to be the Termination Date with respect to this Transaction except in relation to any portion of the Transaction (an "Affected
    /// Portion") in respect of which: (1) a valid notice of Buy-in Price has been delivered that is effective fewer than three Business
    /// Days prior to such 60th Business Day, in which case the Termination Date for that Affected Portion shall be the third Business Day
    /// following the date on which such notice is effective; or (2) Buyer has purchased but not Delivered Deliverable Obligations validly
    /// specified by Seller pursuant to Section 9.10(b), in which case the Termination Date for that Affected Portion shall be the tenth
    /// Business Day following the date on which Seller validly specified such Deliverable Obligations to Buyer</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditDefaultSwap</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PhysicalSettlementTerms : SettlementTerms
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool physicalSettlementPeriodSpecified;
		[System.Xml.Serialization.XmlElementAttribute("physicalSettlementPeriod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Physical settlement period")]
        public PhysicalSettlementPeriod physicalSettlementPeriod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool deliverableObligationsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("deliverableObligations", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Deliverable obligations")]
        public DeliverableObligations deliverableObligations;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool escrowSpecified;
		[System.Xml.Serialization.XmlElementAttribute("escrow", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Escrow")]
        public EFS_Boolean escrow;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sixtyBusinessDaySettlementCapSpecified;
		[System.Xml.Serialization.XmlElementAttribute("sixtyBusinessDaySettlementCap", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Sixty business day settlement cap")]
        public EFS_Boolean sixtyBusinessDaySettlementCap;
		#endregion Members
	}
    #endregion PhysicalSettlementTerms
    #region ProtectionTerms
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>calculationAmount (exactly one occurrence; of the type Money) 
    /// The notional amount of protection coverage. ISDA 2003 Term: Floating Rate Payer Calculation Amount</newpara>
    /// <newpara>creditEvents (zero or one occurrence; of the type CreditEvents) 
    /// This element contains all the ISDA terms relating to credit events.</newpara>
    /// <newpara>obligations (zero or one occurrence; of the type Obligations) 
    /// The underlying obligations of the reference entity on which you are buying or selling protection. 
    /// The credit events Failure to Pay, Obligation Acceleration, Obligation Default, Restructuring, Repudiation/Moratorium 
    /// are defined with respect to these obligations. ISDA 2003 Term:</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditDefaultSwap</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ProtectionTerms : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("calculationAmount", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation amount", IsVisible = false)]
        public Money calculationAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditEventsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("creditEvents", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation amount")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit events")]
        public CreditEvents creditEvents;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool obligationsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("obligations", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Obligations")]
        public Obligations obligations;
		#endregion Members
	}
    #endregion ProtectionTerms
    #region PubliclyAvailableInformation
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>standardPublicSources (zero or one occurrence; of the type Empty) 
    /// If this element is specified, indicates that ISDA defined Standard Public Sources are applicable.</newpara>
    /// <newpara>publicSource (zero or more occurrences; of the type xsd:string) 
    /// A public information source, e.g. a particular newspaper or electronic news service, that may publish relevant 
    /// information used in the determination of whether or not a credit event has occurred. ISDA 2003 Term: Public Source</newpara>
    /// <newpara>specifiedNumber (zero or one occurrence; of the type xsd:positiveInteger) 
    /// The minimum number of the specified public information sources that must publish information that reasonably confirms 
    /// that a credit event has occurred. The market convention is two. ISDA 2003 Term: Specified Number</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditEventNotice</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class PubliclyAvailableInformation : ItemGUI
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

    #region ReferenceInformation
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>referenceEntity (exactly one occurrence; of the type LegalEntity)
    /// The corporate or sovereign entity on which you are buying or selling protection and any successor that assumes all 
    /// or substantially all of its contractual and other obligations. 
    /// It is vital to use the correct legal name of the entity and to be careful not to choose a subsidiary if you really want 
    /// to trade protection on a parent company. Please note, Reference Entities cannot be senior or subordinated. 
    /// It is the obligations of the Reference Entities that can be senior or subordinated. ISDA 2003 Term: Reference Entity</newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>referenceObligation (zero or more occurrences; of the type ReferenceObligation)
    /// The Reference Obligation is a financial instrument that is either issued or guaranteed by the reference entity. 
    /// It serves to clarify the precise reference entity protection is being offered upon, and its legal position with regard 
    /// to other related firms (parents/subsidiaries). Furthermore the Reference Obligation is ALWAYS deliverable and establishes 
    /// the Pari Passu ranking (as the deliverable bonds must rank equal to the reference obligation). 
    /// ISDA 2003 Term: Reference Obligation</newpara>
    /// <newpara>or</newpara>
    /// <newpara>noReferenceObligation (exactly one occurrence; of the type Empty)
    /// Used to indicate that there is no Reference Obligation associated with this Credit Default Swap and that there will 
    /// never be one</newpara>
    /// <newpara>or</newpara>
    /// <newpara>unknownReferenceObligation (exactly one occurrence; of the type Empty)
    /// Used to indicate that the Reference obligation associated with the Credit Default Swap is currently not known. 
    /// This is not valid for Legal Confirmation purposes, but is valid for earlier stages in the trade life cycle 
    /// (e.g. Broker Confirmation).</newpara>
    /// <newpara>allGuarantees (zero or one occurrence; of the type xsd:boolean) 
    /// Indicates whether an obligation of the Reference Entity, guaranteed by the Reference Entity on behalf of a non-Affiliate, 
    /// is to be considered an Obligation for the purpose of the transaction. 
    /// It will be considered an obligation if allGuarantees is applicable (true) and not if allGuarantees is inapplicable (false). 
    /// ISDA 2003 Term: All Guarantees</newpara>
    /// <newpara>referencePrice (zero or one occurrence; of the type xsd:decimal) 
    /// Used to determine (a) for physically settled trades, the Physical Settlement Amount, which equals the Floating Rate 
    /// Payer Calculation Amount times the Reference Price and (b) for cash settled trades, the Cash Settlement Amount, 
    /// which equals the greater of (i) the difference between the Reference Price and the Final Price and (ii) zero. 
    /// ISDA 2003 Term: Reference Price</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• GeneralTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferenceInformation
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("referenceEntity", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity", IsVisible = false)]
        public LegalEntity referenceEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Obligations")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("noReferenceObligation", typeof(Empty),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNoReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemUnknownReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unknownReferenceObligation", typeof(Empty),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemUnknownReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceObligation", typeof(ReferenceObligation),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Obligation", IsClonable = true, IsChild = true)]
        public ReferenceObligation[] itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool allGuaranteesSpecified;
		[System.Xml.Serialization.XmlElementAttribute("allGuarantees", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "All guarantees")]
        public EFS_Boolean allGuarantees;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool referencePriceSpecified;
		[System.Xml.Serialization.XmlElementAttribute("referencePrice", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference price")]
        public EFS_Decimal referencePrice;
		#endregion Members
		#region Constructors
		public ReferenceInformation()
        {
            itemNoReference = new Empty();
            itemUnknownReference = new Empty();
            itemReference = new ReferenceObligation[1] { new ReferenceObligation() };
		}
		#endregion Constructors
	}
    #endregion ReferenceInformation
    #region ReferenceObligation
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>bond (exactly one occurrence; with locally defined content)
    /// Defines the underlying asset when it is a bond.</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>convertibleBond (exactly one occurrence; with locally defined content)
    /// Defines the underlying asset when it is a convertible bond.</newpara>
    /// <newpara>There can be zero or one occurance of the following structure; Choice of either</newpara>
    /// <newpara>primaryObligor (exactly one occurrence; of the type LegalEntity) 
    /// The entity primarily responsible for repaying debt to a creditor as a result of borrowing or issuing bonds. 
    /// ISDA 2003 Term: Primary Obligor</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>primaryObligorReference (exactly one occurrence; of the type LegalEntityReference) 
    /// A pointer style reference to a reference entity defined elsewhere in the document. 
    /// Used when the reference entity is the primary obligor.</newpara>
    /// <newpara>Either</newpara>
    /// <newpara>guarantor (exactly one occurrence; of the type LegalEntity) 
    /// The party that guarantees by way of a contractual arrangement to pay the debts of an obligor if the obligor 
    /// is unable to make the required payments itself. ISDA 2003 Term: Guarantor</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>guarantorReference (exactly one occurrence; of the type LegalEntityReference) 
    /// A pointer style reference to a reference entity defined elsewhere in the document. 
    /// Used when the reference entity is the guarantor.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ReferenceInformation</newpara>
    ///<newpara>• ReferencePair</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferenceObligation : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Underlying asset")]
        public EFS_RadioChoice underlyingAsset;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Bond underlyingAssetBond;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyingAssetConvertibleBondSpecified;
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ConvertibleBond underlyingAssetConvertibleBond;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Primary obligor")]
        public EFS_RadioChoice primaryObligor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty primaryObligorNone;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorLegalEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("primaryObligor", typeof(LegalEntity),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity primaryObligorLegalEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool primaryObligorLegalEntityReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("primaryObligorReference", typeof(LegalEntityReference),Order=4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntityReference primaryObligorLegalEntityReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool guarantorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("guarantor", typeof(LegalEntity),Order=5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantors")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantor", IsClonable = true, IsChild = true)]
        public LegalEntity[] guarantor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool guarantorReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("guarantorReference", typeof(LegalEntityReference),Order=6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantors Reference")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantor", IsClonable = true, IsChild = false)]
        public LegalEntityReference[] guarantorReference;
		#endregion Members
		#region Constructors
		public ReferenceObligation()
        {
            underlyingAssetBond = new Bond();
            underlyingAssetConvertibleBond = new ConvertibleBond();

            primaryObligorNone = new Empty();
            primaryObligorLegalEntity = new LegalEntity();
            primaryObligorLegalEntityReference = new LegalEntityReference();
        }
        #endregion Constructors
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new ObjectArray(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion ReferenceObligation
    #region ReferencePair
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>referenceEntity (exactly one occurrence; of the type LegalEntity)
    /// The corporate or sovereign entity on which you are buying or selling protection and any successor that assumes all or 
    /// substantially all of its contractual and other obligations. It is vital to use the correct legal name of the entity and 
    /// to be careful not to choose a subsidiary if you really want to trade protection on a parent company. 
    /// Please note, Reference Entities cannot be senior or subordinated. 
    /// It is the obligations of the Reference Entities that can be senior or subordinated. ISDA 2003 Term: Reference Entity </newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either</newpara>
    /// <newpara>referenceObligation (exactly one occurrence; of the type ReferenceObligation)
    /// The Reference Obligation is a financial instrument that is either issued or guaranteed by the reference entity. 
    /// It serves to clarify the precise reference entity protection is being offered upon, and its legal position with regard to 
    /// other related firms (parents/subsidiaries). Furthermore the Reference Obligation is ALWAYS deliverable and establishes 
    /// the Pari Passu ranking (as the deliverable bonds must rank equal to the reference obligation). 
    /// ISDA 2003 Term: Reference Obligation </newpara>
    /// <newpara>or</newpara>
    /// <newpara>noReferenceObligation (exactly one occurrence; of the type Empty)
    /// Used to indicate that there is no Reference Obligation associated with this Credit Default Swap and that there will 
    /// never be one</newpara>
    /// <newpara>entityType (exactly one occurrence; of the type EntityType)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ReferencePoolItem</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferencePair : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("referenceEntity", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity", IsVisible = false)]
        public LegalEntity referenceEntity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference entity")]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Obligation")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceObligation", typeof(ReferenceObligation),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public ReferenceObligation itemReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemNoReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("noReferenceObligation", typeof(Empty),Order=3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty itemNoReference;

		[System.Xml.Serialization.XmlElementAttribute("entityType", Order = 4)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity type", IsVisible = false)]
        public EntityType entityType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity type")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion ReferencePair
    #region ReferencePoolItem
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>constituentWeight (exactly one occurrence; of the type ConstituentWeight)</newpara>
    /// <newpara>referencePair (exactly one occurrence; of the type ReferencePair)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ReferencePoolItem
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("constituentWeight", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent weight", IsVisible = false)]
        public ConstituentWeight constituentWeight;
		[System.Xml.Serialization.XmlElementAttribute("referencePair", Order = 4)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent weight")]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pair", IsVisible = false)]
        public ReferencePair referencePair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Reference pair")]
        public bool FillBalise;
		#endregion Members
	}
    #endregion ReferencePoolItem
    #region Restructuring
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>restructuringType (zero or one occurrence; of the type RestructuringType) 
    /// Specifies the type of restructuring that is applicable.</newpara>
    /// <newpara>multipleHolderObligation (zero or one occurrence; of the type Empty) 
    /// In relation to a restructuring credit event, unless multiple holder obligation is not specified restructurings are limited to
    /// multiple holder obligations. A multiple holder obligation means an obligation that is held by more than three holders that 
    /// are not affiliates of each other and where at least two thirds of the holders must agree to the event that constitutes the
    /// restructuring credit event. ISDA 2003 Term: Multiple Holder Obligation</newpara>
    /// <newpara>multipleCreditEventNotices (zero or one occurrence; of the type Empty) 
    /// Presence of this element indicates that Section 3.9 of the 2003 Credit Derivatives Definitions shall apply. 
    /// Absence of this element indicates that Section 3.9 shall not apply. NOTE: Not allowed under ISDA Credit 1999.</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CreditEvents</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class Restructuring : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool restructuringTypeSpecified;
		[System.Xml.Serialization.XmlElementAttribute("restructuringType", Order = 1)]
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
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>Inherited element(s): (This definition inherits the content defined by the type xsd:normalizedString)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• Complex type: Restructuring</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class RestructuringType : SchemeGUI
    {
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string restructuringScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;

        #region Constructors
        public RestructuringType()
        {
            restructuringScheme = "http://www.fpml.org/coding-scheme/restructuring-1-0";
        }
        #endregion Constructors
    }
    #endregion RestructuringType

    #region ScheduledTerminationDate
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>adjustableDate (exactly one occurrence; of the type AdjustableDate2)</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>relativeDate (exactly one occurrence; of the type Interval)</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• GeneralTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ScheduledTerminationDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Type")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemAdjustableDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustableDate", typeof(AdjustableDate2),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public AdjustableDate2 itemAdjustableDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemRelativeDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relativeDate", typeof(Interval),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Interval itemRelativeDate;
		#endregion Members
		#region Constructors
		public ScheduledTerminationDate()
        {
            itemAdjustableDate = new AdjustableDate2();
            itemRelativeDate = new Interval();
		}
		#endregion Constructors
	}

    #endregion ScheduledTerminationDate
    #region SettlementTerms
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>settlementCurrency (zero or one occurrence; of the type Currency) ISDA 2003 Term: Settlement Currency</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///<newpara>• CashSettlementTerms</newpara>
    ///<newpara>• PhysicalSettlementTerms</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PhysicalSettlementTerms))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CashSettlementTerms))]
    public class SettlementTerms : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementCurrencySpecified;
		[System.Xml.Serialization.XmlElementAttribute("relativeDate", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency settlementCurrency;
		#endregion Members
	}
    #endregion SettlementTerms
    #region SinglePayment
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>adjustablePaymentDate (exactly one occurrence; of the type xsd:date) 
    /// A fixed amount payment date that shall be subject to adjustment in accordance with the applicable business 
    /// day convention if it would otherwise fall on a day that is not a business day. 
    /// The applicable business day convention and business day are those specified in the dateAdjustments element 
    /// within the generalTerms component. ISDA 2003 Term: Fixed Rate Payer Payment Date</newpara>
    /// <newpara>adjustedPaymentDate (zero or one occurrence; of the type xsd:date) 
    /// The adjusted payment date. This date should already be adjusted for any applicable business day convention. 
    /// This component is not intended for use in trade confirmation but may be specified to allow the fee structure 
    /// to also serve as a cashflow type component.</newpara>
    /// <newpara>fixedAmount (exactly one occurrence; of the type Money) 
    /// A fixed payment amount. ISDA 2003 Term: Fixed Amount</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• FeeLeg</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class SinglePayment : ItemGUI, IEFS_Array
	{
		#region Members
		[System.Xml.Serialization.XmlElementAttribute("adjustablePaymentDate", Order = 1)]
		[ControlGUI(Name = "Adjustable payment date")]
        public EFS_Date adjustablePaymentDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool adjustedPaymentDateSpecified;
		[System.Xml.Serialization.XmlElementAttribute("adjustedPaymentDate", Order =2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Adjusted payment date")]
        public EFS_Date adjustedPaymentDate;
		[System.Xml.Serialization.XmlElementAttribute("fixedAmount", Order = 3)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed amount", IsVisible = false)]
        public Money fixedAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fixed amount")]
        public bool FillBalise;
		#endregion Members
		#region Constructors
		public SinglePayment()
        {
            adjustablePaymentDate = new EFS_Date();
            adjustedPaymentDate = new EFS_Date();

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
    #endregion SinglePayment
    #region SingleValuationDate
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>businessDays (zero or one occurrence; of the type xsd:nonNegativeInteger) 
    /// A number of business days. Its precise meaning is dependant on the context in which this element is used. 
    /// ISDA 2003 Term: Business Day</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• ValuationDate</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///<newpara>• MultipleValuationDates</newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MultipleValuationDates))]
    public class SingleValuationDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool businessDaysSpecified;
		[System.Xml.Serialization.XmlElementAttribute("businessDays", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Business days")]
        public EFS_NonNegativeInteger businessDays;
		#endregion Members
    }
    #endregion SingleValuationDate

    #region SpecifiedCurrency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class SpecifiedCurrency : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currencies")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currencies", IsClonable = true, IsChild = false)]
        public Currency[] currency;
		#endregion Members
	}
    #endregion SpecifiedCurrency
    #region ValuationDate
    /// <summary>
    /// <newpara><b>Description :</b> </newpara>
    /// <newpara><b>Contents :</b></newpara>
    /// <newpara>There can be one occurance of the following structure; Choice of either </newpara>
    /// <newpara>singleValuationDate (exactly one occurrence; of the type SingleValuationDate) 
    /// Where single valuation date is specified as being applicable for cash settlement, this element specifies the number 
    /// of business days after satisfaction of all conditions to settlement when such valuation date occurs. 
    /// ISDA 2003 Term: Single Valuation Date</newpara>
    /// <newpara>Or</newpara>
    /// <newpara>multipleValuationDates (exactly one occurrence; of the type MultipleValuationDates) 
    /// Where multiple valuation dates are specified as being applicable for cash settlement, this element specifies (a) the number 
    /// of applicable valuation dates, and (b) the number of business days after satisfaction of all conditions to settlement when 
    /// the first such valuation date occurs, and (c) the number of business days thereafter of each successive valuation date. 
    /// ISDA 2003 Term: Multiple Valuation Dates</newpara>
    ///</summary>
    ///<remarks>
    ///<newpara><b>Used by :</b></newpara>
    ///<newpara>• CashSettlementTerms</newpara>
    ///<newpara><b>Derived Types :</b></newpara>
    ///</remarks>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2005/FpML-4-2")]
    public class ValuationDate : ItemGUI
	{
		#region Members
		[System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Valuation date")]
        public EFS_RadioChoice item;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemSingleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singleValuationDate", typeof(SingleValuationDate),Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public SingleValuationDate itemSingle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool itemMultipleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multipleValuationDates", typeof(MultipleValuationDates),Order=2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public MultipleValuationDates itemMultiple;
		#endregion Members
		#region Constructors
		public ValuationDate()
        {
            itemSingle = new SingleValuationDate();
            itemMultiple = new MultipleValuationDates();
		}
		#endregion Constructors
	}
    #endregion ValuationDate
}
