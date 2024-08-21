#region using directives
using System;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

using EFS.ACommon;

using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;

using EfsML.v30.Shared;

using FpML.Enum;

using FpML.v44.Assetdef;
using FpML.v44.Enum;
using FpML.v44.Cd;
using FpML.v44.Option.Shared;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.Shared;
#endregion using directives

namespace FpML.v44.Mktenv.ToDefine
{
    #region CompoundingFrequency
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class CompoundingFrequency
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/compounding-frequency-1-0")]
        public string compoundingFrequencyScheme = "http://www.fpml.org/coding-scheme/compounding-frequency-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion CompoundingFrequency
    #region CreditCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("creditCurve", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class CreditCurve : PricingStructure
    {
        public LegalEntity referenceEntity;
        public LegalEntityReference creditEntityReference;
        public CreditEvents creditEvents;
        public CreditSeniority seniority;
        public bool secured;
        [System.Xml.Serialization.XmlElementAttribute("currency")]
        public Currency currency1;
        public Obligations obligations;
        public DeliverableObligations deliverableObligations;
    }
    #endregion CreditCurve
    #region CreditCurveValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("creditCurveValuation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class CreditCurveValuation : PricingStructureValuation
    {
        public QuotedAssetSet inputs;
        public DefaultProbabilityCurve defaultProbabilityCurve;
        public System.Decimal recoveryRate;
        public TermCurve recoveryRateCurve;
    }
    #endregion CreditCurveValuation

    #region DefaultProbabilityCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DefaultProbabilityCurve : PricingStructureValuation
    {
        public PricingStructureReference baseYieldCurve;
        public TermCurve defaultProbabilities;
    }
    #endregion DefaultProbabilityCurve

    #region ForwardRateCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ForwardRateCurve
    {
        public AssetReference assetReference;
        public TermCurve rateCurve;
    }
    #endregion ForwardRateCurve
    #region FxCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("fxCurve", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class FxCurve : PricingStructure
    {
        public QuotedCurrencyPair quotedCurrencyPair;
    }
    #endregion FxCurve
    #region FxCurveValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("fxCurveValuation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class FxCurveValuation : PricingStructureValuation
    {
        public PricingStructureReference settlementCurrencyYieldCurve;
        public PricingStructureReference forecastCurrencyYieldCurve;
        public FxRateSet spotRate;
        public TermCurve fxForwardCurve;
        public TermCurve fxForwardPointsCurve;
    }
    #endregion FxCurveValuation
    #region FxRateSet
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FxRateSet : QuotedAssetSet { }
    #endregion FxRateSet

    #region MultiDimensionalPricingData
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MultiDimensionalPricingData
    {
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
        [System.Xml.Serialization.XmlElementAttribute("point")]
        public PricingStructurePoint[] point;
    }
    #endregion MultiDimensionalPricingData

    #region ParametricAdjustment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ParametricAdjustment
    {
        [System.Xml.Serialization.XmlElementAttribute(DataType = "normalizedString")]
        public string name;
        public PriceQuoteUnits inputUnits;
        [System.Xml.Serialization.XmlElementAttribute("datapoint")]
        public ParametricAdjustmentPoint[] datapoint;
    }
    #endregion ParametricAdjustment
    #region ParametricAdjustmentPoint
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ParametricAdjustmentPoint
    {
        public System.Decimal parameterValue;
        public System.Decimal adjustmentValue;
    }
    #endregion ParametricAdjustmentPoint
    #region PricingStructurePoint
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingStructurePoint
    {
        public PricingDataPointCoordinate coordinate;
        public PricingDataPointCoordinateReference coordinateReference;
        public object Item;
        public AssetReference underlyingAssetReference;
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
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion PricingStructurePoint

    #region TermCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TermCurve
    {
        public InterpolationMethod interpolationMethod;
        public bool extrapolationPermitted;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extrapolationPermittedSpecified;
        [System.Xml.Serialization.XmlElementAttribute("point")]
        public TermPoint[] point;
    }
    #endregion TermCurve
    #region TermPoint
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TermPoint
    {
        public TimeDimension term;
        public System.Decimal bid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool bidSpecified;
        public System.Decimal mid;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool midSpecified;
        public System.Decimal ask;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool askSpecified;
        public System.Decimal spreadValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool spreadValueSpecified;
        public AssetReference definition;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion TermPoint

    #region VolatilityMatrix
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("volatilityMatrixValuation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class VolatilityMatrix : PricingStructureValuation
    {
        public MultiDimensionalPricingData dataPoints;
        [System.Xml.Serialization.XmlElementAttribute("adjustment")]
        public ParametricAdjustment[] adjustment;
    }
    #endregion VolatilityMatrix
    #region VolatilityRepresentation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("volatilityRepresentation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class VolatilityRepresentation : PricingStructure
    {
        public AnyAssetReference asset;
    }
    #endregion VolatilityRepresentation

    #region YieldCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("yieldCurve", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class YieldCurve : PricingStructure
    {
        public string algorithm;
        public ForecastRateIndex forecastRateIndex;
    }
    #endregion YieldCurve
    #region YieldCurveValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("yieldCurveValuation", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class YieldCurveValuation : PricingStructureValuation
    {
        public QuotedAssetSet inputs;
        public ZeroRateCurve zeroCurve;
        [System.Xml.Serialization.XmlElementAttribute("forwardCurve")]
        public ForwardRateCurve[] forwardCurve;
        public TermCurve discountFactorCurve;
    }
    #endregion YieldCurveValuation

    #region ZeroRateCurve
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ZeroRateCurve
    {
        public CompoundingFrequency compoundingFrequency;
        public TermCurve rateCurve;
    }
    #endregion ZeroRateCurve
}


