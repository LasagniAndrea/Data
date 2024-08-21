#region using directives
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using FpML.v44.Assetdef;
using FpML.v44.Mktenv.ToDefine;
using FpML.v44.Riskdef.ToDefine;
using FpML.v44.Shared;
using FpML.v44.ValuationResults.ToDefine;
using System.Reflection;
#endregion using directives

namespace FpML.v44.Riskdef
{
    #region Market
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlRootAttribute("market", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    public class Market : ItemGUI, IEFS_Array
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool nameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("name")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Name")]
        public string name;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool benchmarkQuotesSpecified;
        [System.Xml.Serialization.XmlElementAttribute("benchmarkQuotes")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Benchmark quotes")]
        public QuotedAssetSet benchmarkQuotes;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pricingStructureSpecified;
        [System.Xml.Serialization.XmlElementAttribute("pricingStructure")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Pricing structures")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Pricing structure", IsClonable = true, IsChild = true)]
        public PricingStructure[] pricingStructure;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool pricingStructureValuationSpecified;
        [System.Xml.Serialization.XmlElementAttribute("pricingStructureValuation")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Pricing structures valuation")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Pricing structure valuation", IsClonable = true, IsChild = true)]
        public PricingStructureValuation[] pricingStructureValuation;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool benchmarkPricingMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("benchmarkPricingMethod")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.First, Name = "Pricing methods")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Pricing method", IsClonable = true, IsChild = true)]
        public PricingMethod[] benchmarkPricingMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
        #endregion Members
        #region Accessors
        [System.Xml.Serialization.XmlAttributeAttribute("id", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "ID")]
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
        #region Methods
        #region DisplayArray
        public object DisplayArray(object pCurrent, FieldInfo pFldCurrent, object pParent, FieldInfo pFldParent, ControlGUI pControlGUI, object pGrandParent, FieldInfo pFldGrandParent, FullConstructor pFullCtor)
        {
            return (new OptionalItem(pCurrent, pFldCurrent, pControlGUI, pParent, pFldParent, pGrandParent, pFldGrandParent, pFullCtor));
        }
        #endregion DisplayArray
        #endregion Methods
    }
    #endregion Market
}

namespace FpML.v44.Riskdef.ToDefine
{
    #region AssetOrTermPointOrPricingStructureReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AssetOrTermPointOrPricingStructureReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion AssetOrTermPointOrPricingStructureReference

    #region BasicAssetValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BasicAssetValuation : Valuation
    {
        [System.Xml.Serialization.XmlElementAttribute("quote")]
        public BasicQuotation[] quote;
    }
    #endregion BasicAssetValuation

    #region DenominatorTerm
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DenominatorTerm
    {
        public WeightedPartialDerivative weightedPartial;
        [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
        public string power;
    }
    #endregion DenominatorTerm
    #region DerivativeCalculationMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DerivativeCalculationMethod
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/derivative-calculation-method-1-0")]
        public string derivativeCalculationMethodScheme = "http://www.fpml.org/coding-scheme/derivative-calculation-method-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion DerivativeCalculationMethod
    #region DerivativeCalculationProcedure
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DerivativeCalculationProcedure
    {
        public DerivativeCalculationMethod method;
        public System.Decimal perturbationAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool perturbationAmountSpecified;
        public bool averaged;
        public PerturbationType perturbationType;
        public string derivativeFormula;
        public PricingStructureReference replacementMarketInput;
    }
    #endregion DerivativeCalculationProcedure
    #region DerivativeFormula
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class DerivativeFormula
    {
        public FormulaTerm term;
        public DenominatorTerm denominatorTerm;
    }
    #endregion DerivativeFormula

    #region FormulaTerm
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FormulaTerm
    {
        public System.Decimal coefficient;
        [System.Xml.Serialization.XmlElementAttribute("partialDerivativeReference")]
        public PricingParameterDerivativeReference[] partialDerivativeReference;
    }
    #endregion FormulaTerm

    #region GenericDimension
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class GenericDimension
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "normalizedString")]
        public string name;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value;
    }
    #endregion GenericDimension

    #region InstrumentSet
    #endregion InstrumentSet

    #region MarketReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MarketReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion MarketReference

    #region PerturbationType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PerturbationType
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/perturbation-type-1-0")]
        public string perturbationTypeScheme = "http://www.fpml.org/coding-scheme/perturbation-type-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion PerturbationType
    #region PositionId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PositionId
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string positionIdScheme;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion PositionId

    #region PricingDataPointCoordinate
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingDataPointCoordinate
    {
        public TimeDimension term;
        public TimeDimension expiration;
        public System.Decimal strike;
        public GenericDimension generic;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion PricingDataPointCoordinate
    #region PricingDataPointCoordinateReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingDataPointCoordinateReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion PricingDataPointCoordinateReference
    #region PricingInputReplacement
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingInputReplacement
    {
        public PricingStructureReference originalInputReference;
        public PricingStructureReference replacementInputReference;
    }
    #endregion PricingInputReplacement
    #region PricingInputType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingInputType
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/pricing-input-type-1-0")]
        public string pricingInputTypeScheme = "http://www.fpml.org/coding-scheme/pricing-input-type-1-0";
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
    }
    #endregion PricingInputType
    #region PricingMethod
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingMethod
    {
        public AnyAssetReference assetReference;
        public PricingStructureReference pricingInputReference;
    }
    #endregion PricingMethod
    #region PricingParameterDerivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingParameterDerivative
    {
        public string description;
        [System.Xml.Serialization.XmlElementAttribute("inputDateReference", typeof(ValuationReference))]
        [System.Xml.Serialization.XmlElementAttribute("parameterReference", typeof(AssetOrTermPointOrPricingStructureReference))]
        public Reference[] Items;
        public DerivativeCalculationProcedure calculationProcedure;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion PricingParameterDerivative
    #region PricingParameterDerivativeReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingParameterDerivativeReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion PricingParameterDerivativeReference
    #region PricingParameterShift
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PricingParameterShift
    {
        public AssetOrTermPointOrPricingStructureReference parameterReference;
        public System.Decimal shift;
        public PriceQuoteUnits shiftUnits;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion PricingParameterShift
    #region PricingStructureValuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VolatilityMatrix))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(YieldCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DefaultProbabilityCurve))]
    public class PricingStructureValuation : Valuation
    {
        #region Members
        public IdentifiedDate baseDate;
        public IdentifiedDate spotDate;
        public IdentifiedDate inputDataDate;
        public IdentifiedDate endDate;
        public System.DateTime buildDateTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool buildDateTimeSpecified;
        #endregion Members
    }
    #endregion PricingStructureValuation

    #region QuotedAssetSet
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxRateSet))]
    public class QuotedAssetSet
    {
        [System.Xml.Serialization.XmlArrayItemAttribute("underlyingAsset", IsNullable = false)]
        public Asset[] instrumentSet;
        [System.Xml.Serialization.XmlElementAttribute("assetQuote")]
        public BasicAssetValuation[] assetQuote;
    }
    #endregion QuotedAssetSet

    #region SensitivityDefinition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SensitivityDefinition
    {
        public string name;
        public ValuationScenarioReference valuationScenarioReference;
        public object Item;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion SensitivityDefinition
    #region SensitivitySetDefinition
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class SensitivitySetDefinition
    {
        public string name;
        public QuotationCharacteristics sensitivityCharacteristics;
        public ValuationScenarioReference valuationScenarioReference;
        public PricingInputType pricingInputType;
        public PricingStructureReference pricingInputReference;
        public System.Decimal scale;
        [System.Xml.Serialization.XmlElementAttribute("sensitivityDefinition")]
        public SensitivityDefinition[] sensitivityDefinition;
        public DerivativeCalculationProcedure calculationProcedure;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion SensitivitySetDefinition

    #region TimeDimension
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class TimeDimension
    {
        [System.Xml.Serialization.XmlElementAttribute("tenor", typeof(Interval))]
        [System.Xml.Serialization.XmlElementAttribute("date", typeof(System.DateTime), DataType = "date")]
        public object Item;
    }
    #endregion TimeDimension

    #region Valuation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(AssetValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(PricingStructureValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(VolatilityMatrix))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(YieldCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CreditCurveValuation))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DefaultProbabilityCurve))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(BasicAssetValuation))]
    public class Valuation
    {
        public AnyAssetReference objectReference;
        public ValuationScenarioReference valuationScenarioReference;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string definitionRef;
    }
    #endregion Valuation
    #region ValuationReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion ValuationReference
    #region ValuationScenario
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationScenario
    {
        public string name;
        public IdentifiedDate valuationDate;
        public MarketReference marketReference;
        [System.Xml.Serialization.XmlElementAttribute("shift")]
        public PricingParameterShift[] shift;
        [System.Xml.Serialization.XmlElementAttribute("replacement")]
        public PricingInputReplacement[] replacement;
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
        public string id;
    }
    #endregion ValuationScenario
    #region ValuationScenarioReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ValuationScenarioReference : Reference
    {
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
    }
    #endregion ValuationScenarioReference

    #region WeightedPartialDerivative
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class WeightedPartialDerivative
    {
        public PricingStructureReference partialDerivativeReference;
        public System.Decimal weight;
    }
    #endregion WeightedPartialDerivative

}
