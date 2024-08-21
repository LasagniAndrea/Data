#region using directives
using System;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;

using EFS.EFSTools;

using EfsML.Enum;
using EfsML.DynamicData;
using EfsML.Interface;
using EfsML.Settlement;

using EfsML.v30.Shared;
using EfsML.v30.Ird;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Ird;
using FpML.v44.Shared;
#endregion using directives
namespace EfsML.v30.DebtSecurity
{
    #region AccruedInterestCalculationRules
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class AccruedInterestCalculationRules : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationMethod",Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public AccruedInterestCalculationMethodEnum calculationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool roundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rounding",Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding")]
        public Rounding rounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool prorataDayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("prorataDayCountFraction", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Prorata day count fraction")]
        public DayCountFractionEnum prorataDayCountFraction;
        #endregion Members
    }
    #endregion AccruedInterestCalculationRules

    #region Classification
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Classification : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool debtSecurityClassSpecified;
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityClass", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "DebtSecurity Class")]
        public Identification debtSecurityClass;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cfiCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cfiCode", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "CFI code")]
        public CFIIdentifier cfiCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool productTypeCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("productTypeCode", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "ProductType code")]
        public ProductTypeCodeEnum productTypeCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool financialInstrumentProductTypeCodeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("financialInstrumentProductTypeCode", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Financial instrument productType code")]
        public FinancialInstrumentProductTypeCodeEnum financialInstrumentProductTypeCode;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symbolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("symbol", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol")]
        public EFS_String symbol;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool symbolSfxSpecified;
        [System.Xml.Serialization.XmlElementAttribute("symbolSfx", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Symbol suffix")]
        public EFS_String symbolSfx;
        #endregion Members
    }
    #endregion Classification
    #region CFIIdentifier
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CFIIdentifier : StringGUI
    {
        #region Members
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
        #endregion Members
    }
    #endregion CFIIdentifier
    #region CommercialPaper
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class CommercialPaper : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool programSpecified;
        [System.Xml.Serialization.XmlElementAttribute("program", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Program")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CPProgramEnum program;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool regTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("regType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Regtype")]
        public EFS_Integer regType;
        #endregion Members
    }
    #endregion CommercialPaper

    #region DebtSecurity
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("debtSecurity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public partial class DebtSecurity : Product
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("security", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security")]
        public Security security;
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityStream", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Security")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Debt security Stream", IsClonable = true, IsMaster = true, IsMasterVisible = true, IsChild = true, IsCopyPasteItem = true)]
        [BookMarkGUI(Name = "S", IsVisible = true)]
        public DebtSecurityStream[] debtSecurityStream;
        #endregion Members
    }
    #endregion DebtSecurity
    #region DebtSecurityStream
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class DebtSecurityStream : InterestRateStreamBase 
    {
    }


    #endregion DebtSecurityStream

    #region FullCouponCalculationRules
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class FullCouponCalculationRules : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationMethod", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation method")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public FullCouponCalculationMethodEnum calculationMethod;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool unitCouponRoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("unitCouponRounding", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Unit coupon rounding")]
        public Rounding unitCouponRounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool roundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("rounding", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rounding")]
        public Rounding rounding;
        #endregion Members
    }
    #endregion FullCouponCalculationRules

    #region Localization
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Localization : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool countryOfIssueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("countryOfIssue", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Country of issue")]
        public Country countryOfIssue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool stateOrProvinceOfIssueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stateOrProvinceOfIssue", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "State or province of issue")]
        public Identification stateOrProvinceOfIssue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localeOfIssueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localeOfIssue", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Local of issue")]
        public Identification localeOfIssue;
        #endregion Members
    }
    #endregion Localization

    #region PriceUnits
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class PriceUnits : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("priceQuoteUnits", Order = 1)]
        public PriceQuoteUnits priceQuoteUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool forcedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("forced", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mandatory usage")]
        public EFS_Boolean forced;
        #endregion Members
    }
    #endregion PriceUnits

    #region Security
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Security : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool classificationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("classification", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Classification")]
        public Classification classification;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon type")]
        public CouponType couponType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceRateTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceRateType", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price rate type")]
        public PriceRateTypeCodeEnum priceRateType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool localizationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("localization", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Localization")]
        public Localization localization;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Instruction registry")]
        public EFS_RadioChoice instructionRegistry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool instructionRegistryCountrySpecified;
        [System.Xml.Serialization.XmlElementAttribute("instructionRegistryCountry", typeof(Country), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Country", IsVisible = true)]
        public Country instructionRegistryCountry;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool instructionRegistryReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("instructionRegistryReference", typeof(PartyReference), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Reference to", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference instructionRegistryReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool instructionRegistryNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty instructionRegistryNone;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool guarantorPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("guarantorPartyReference", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Guarantor")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference guarantorPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool managerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("managerPartyReference", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Manager")]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Party)]
        public PartyReference managerPartyReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool purposeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("purpose", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Purpose")]
        public EFS_String purpose;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool senioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("seniority", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seniority")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public CreditSeniority seniority;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool numberOfIssuedSecuritiesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("numberOfIssuedSecurities", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Number of issued securities")]
        public EFS_Integer numberOfIssuedSecurities;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool faceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("faceAmount", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Face amount")]
        public Money faceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("price", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price")]
        public SecurityPrice price;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commercialPaperSpecified;
        [System.Xml.Serialization.XmlElementAttribute("commercialPaper", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commercial paper")]
        public CommercialPaper commercialPaper;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool calculationRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("calculationRules", Order = 15)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation rules")]
        public SecurityCalculationRules calculationRules;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool orderRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("orderRules", Order = 16)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Order rules")]
        public SecurityOrderRules orderRules;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteRules", Order = 17)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quote rules")]
        public SecurityQuoteRules quoteRules;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool indicatorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("indicator", Order = 18)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Indicator")]
        public SecurityIndicator indicator;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yieldSpecified;
        [System.Xml.Serialization.XmlElementAttribute("yield", Order = 19)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield")]
        public SecurityYield yield;
        #endregion Members
    }
    #endregion Security
    #region SecurityCalculationRules
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityCalculationRules : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fullCouponCalculationRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fullCouponCalculationRules", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Full coupon calculation rules")]
        public FullCouponCalculationRules fullCouponCalculationRules;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestCalculationRulesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestCalculationRules", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest calculation rules")]
        public AccruedInterestCalculationRules accruedInterestCalculationRules;
        #endregion Members
    }
    #endregion SecurityCalculationRules
    #region SecurityIndicator
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityIndicator : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool certificatedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("certificatedIndicator", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Certificated")]
        public EFS_Boolean certificated;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dematerialisedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dematerialisedIndicator", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dematerialised")]
        public EFS_Boolean dematerialised;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fungibleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fungibleIndicator", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fungible")]
        public EFS_Boolean fungible;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool immobilisedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("immobilisedIndicator", Order = 4)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Immobilised")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        public EFS_Boolean immobilised;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool amortisedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amortisedIndicator", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Amortised")]
        public EFS_Boolean amortised;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool callProtectionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("callProtectionIndicator", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Call protection")]
        public EFS_Boolean callProtection;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool callableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("callableIndicator", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Callable")]
        public EFS_Boolean callable;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool putableSpecified;
        [System.Xml.Serialization.XmlElementAttribute("putableIndicator", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Putable")]
        public EFS_Boolean putable;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool convertibleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("convertibleIndicator", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Convertible")]
        public EFS_Boolean convertible;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool escrowedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("escrowedIndicator", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Escrowed")]
        public EFS_Boolean escrowed;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool prefundedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("prefundedIndicator", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Prefunded")]
        public EFS_Boolean prefunded;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentDirectionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentDirectionIndicator", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment direction")]
        public EFS_Boolean paymentDirection;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotedIndicator", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quoted indicator")]
        public EFS_Boolean quoted;
        #endregion Members
    }
    #endregion SecurityIndicator
    #region SecurityOrderRules
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityOrderRules : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceUnits", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price units")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public PriceUnits priceUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestIndicatorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestIndicator", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest indicator")]
        public EFS_Boolean accruedInterestIndicator;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceInPercentageRoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceInPercentageRounding", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price in percentage rounding")]
        public Rounding priceInPercentageRounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceInRateRoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("priceInRateRounding", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price in rate rounding")]
        public Rounding priceInRateRounding;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quantityTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quantityType", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quantity type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public OrderQuantityTypeEnum quantityType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool settlementDaysOffsetSpecified;
        [System.Xml.Serialization.XmlElementAttribute("settlementDaysOffset", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Settlement days offset")]
        public Offset settlementDaysOffset;
        #endregion Members
    }
    #endregion SecurityOrderRules
    #region SecurityPrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityPrice : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuePricePercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuePricePercentage", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issue price percentage")]
        public EFS_Decimal issuePricePercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Redemption price")]
        public EFS_RadioChoice redemptionPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redemptionPricePercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("redemptionPricePercentage", typeof(EFS_Decimal), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Value", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal redemptionPricePercentage;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redemptionPriceFormulaSpecified;
        [System.Xml.Serialization.XmlElementAttribute("redemptionPriceFormula", typeof(Formula), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Formula", IsVisible = true)]
        public Formula redemptionPriceFormula;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redemptionPriceNoneSpecified;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public Empty redemptionPriceNone;
        #endregion Members
    }
    #endregion SecurityPrice
    #region SecurityQuoteRules
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityQuoteRules : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteUnits", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quote units")]
        public PriceQuoteUnits quoteUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestIndicatorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestIndicator", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest indicator")]
        public EFS_Boolean accruedInterestIndicator;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteRoundingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteRounding", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quote rounding")]
        public Rounding quoteRounding;
        #endregion Members
    }
    #endregion SecurityQuoteRules
    #region SecurityYield
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class SecurityYield : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yieldTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("yieldType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield type")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public YieldTypeEnum yieldType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yieldSpecified;
        [System.Xml.Serialization.XmlElementAttribute("yield", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Yield")]
        public EFS_Decimal yield;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool yieldCalculationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("yieldCalculationDate", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Calculation date")]
        public EFS_Date yieldCalculationDate;
        #endregion Members
    }
    #endregion SecurityYield
}
