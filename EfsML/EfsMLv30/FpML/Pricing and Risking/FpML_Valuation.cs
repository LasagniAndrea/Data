#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.Doc;
using FpML.v44.Enum;
using FpML.v44.Reconciliation.ToDefine;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.ValuationResults.ToDefine
{
    #region AssetValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AssetValuation : Valuation
    {
        [System.Xml.Serialization.XmlElementAttribute("quote")]
        public Quotation[] quote;
        public FxRate fxRate;
    }
    #endregion AssetValuation

    #region DerivedValuationScenario
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DerivedValuationScenario
    {
        public string name;
        public ValuationScenarioReference baseValuationScenario;
        public IdentifiedDate valuationDate;
        public MarketReference marketReference;
        [System.Xml.Serialization.XmlElementAttribute("shift")]
        public PricingParameterShift[] shift;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion DerivedValuationScenario

    #region Position
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DefinePosition))]
    public class Position
    {
        public PositionId positionId;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string version;
        public ReportingRoles reportingRoles;
        public PositionConstituent constituent;
        [System.Xml.Serialization.XmlElementAttribute("scheduledDate")]
        public ScheduledDate[] scheduledDate;
        [System.Xml.Serialization.XmlElementAttribute("valuation")]
        public AssetValuation[] valuation;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion Position
    #region PositionConstituent
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionConstituent
    {
        [System.Xml.Serialization.XmlElementAttribute("trade", typeof(Trade))]
        [System.Xml.Serialization.XmlElementAttribute("positionVersionReference", typeof(string), DataType = "positiveInteger")]
        [System.Xml.Serialization.XmlElementAttribute("tradeReference", typeof(PartyTradeIdentifier[]))]
        public object Item;
    }
    #endregion PositionConstituent

    #region Quotation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Quotation
    {
        public System.Decimal value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueSpecified;
        public AssetMeasureType measureType;
        public PriceQuoteUnits quoteUnits;
        public QuotationSideEnum side;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool sideSpecified;
        public Currency currency;
        public QuoteTiming timing;
        public BusinessCenter businessCenter;
        public ExchangeId exchangeId;
        [System.Xml.Serialization.XmlElementAttribute("informationSource")]
        public InformationSource[] informationSource;
        public System.DateTime time;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool timeSpecified;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public System.DateTime valuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valuationDateSpecified;
        public System.DateTime expiryTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool expiryTimeSpecified;
        public CashflowType cashFlowType;
        [System.Xml.Serialization.XmlElementAttribute("sensitivitySet")]
        public SensitivitySet[] sensitivitySet;
    }
    #endregion Quotation

    #region ReportingRoles
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ReportingRoles
    {
        public PartyReference baseParty;
        public PartyReference activityProvider;
        public PartyReference positionProvider;
        public PartyReference valuationProvider;
    }
    #endregion ReportingRoles

    #region ScheduledDate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ScheduledDate
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public DateTime unadjustedDate;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "date")]
        public DateTime adjustedDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool adjustedDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("adjustedDate", DataType = "date")]
        public DateTime adjustedDate1;
        public ScheduledDateType type;
        public AnyAssetReference assetReference;
        public AssetValuation associatedValue;
        public ValuationReference associatedValueReference;
    }
    #endregion ScheduledDate
    #region ScheduledDateType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ScheduledDateType
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/scheduled-date-type-1-0")]
        public string scheduledDateTypeScheme = "http://www.fpml.org/coding-scheme/scheduled-date-type-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ScheduledDateType
    #region ScheduledDates
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ScheduledDates
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "scheduledDate")]
        public ScheduledDate[] scheduledDate;
    }
    #endregion ScheduledDates
    #region Sensitivity
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Sensitivity
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "normalizedString")]
        public string name;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string definitionRef;
        [System.Xml.Serialization.XmlTextAttribute()]
        public System.Decimal Value;
    }
    #endregion Sensitivity
    #region SensitivitySet
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SensitivitySet
    {
        public string name;
        public SensitivitySetReference definitionReference;
        [System.Xml.Serialization.XmlElementAttribute("sensitivity")]
        public Sensitivity[] sensitivity;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion SensitivitySet
    #region SensitivitySetReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SensitivitySetReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion SensitivitySetReference

    #region ValuationSet
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("valuationSet", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class ValuationSet
    {
        public string name;
        [System.Xml.Serialization.XmlElementAttribute("valuationScenario")]
        public ValuationScenario[] valuationScenario;
        [System.Xml.Serialization.XmlElementAttribute("valuationScenarioReference")]
        public ValuationScenarioReference[] valuationScenarioReference;
        public PartyReference baseParty;
        [System.Xml.Serialization.XmlElementAttribute("quotationCharacteristics")]
        public QuotationCharacteristics[] quotationCharacteristics;
        [System.Xml.Serialization.XmlElementAttribute("sensitivitySetDefinition")]
        public SensitivitySetDefinition[] sensitivitySetDefinition;
        public ValuationSetDetail detail;
        [System.Xml.Serialization.XmlElementAttribute("assetValuation")]
        public AssetValuation[] assetValuation;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion ValuationSet
    #region ValuationSetDetail
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationSetDetail
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string valuationSetDetailScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion ValuationSetDetail
    #region Valuations
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Valuations
    {
        public AssetValuation valuation;
        public ValuationReference valuationReference;
    }
    #endregion Valuations


}
