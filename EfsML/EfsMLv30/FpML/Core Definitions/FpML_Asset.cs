#region using directives
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EfsML.v30.AssetDef;
using EfsML.v30.CommodityDerivative;
using EfsML.v30.Security.Shared;
using FpML.Enum;
using FpML.v44.Eq.Shared;
using FpML.v44.Shared;
using System;
#endregion using directives


namespace FpML.v44.Assetdef
{
    #region ActualPrice
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class ActualPrice : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = "Amount", Width = 200)]
        public EFS_Decimal amount;
        [System.Xml.Serialization.XmlElementAttribute("priceExpression", Order = 3)]
        [ControlGUI(Name = "Expression", Width = 200)]
        public PriceExpressionEnum priceExpression;
        #endregion Members
    }
    #endregion ActualPrice
    #region AnyAssetReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AnyAssetReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion AnyAssetReference
    #region AssetPool
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AssetPool : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool versionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("version", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version")]
        public EFS_NonNegativeInteger version;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool effectiveDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("effectiveDate", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Version effective Date")]
        public IdentifiedDate effectiveDate;
        [System.Xml.Serialization.XmlElementAttribute("initialFactor", Order = 3)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Fixed, IsDisplay = true, Name = "Initial factor")]
        public EFS_Decimal initialFactor;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currentFactorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("currentFactor", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Current factor")]
        public EFS_Decimal currentFactor;
        #endregion Members
    }
    #endregion AssetPool

    #region Asset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Bond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Cash))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommodityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConvertibleBond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Deposit))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTraded))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedContract))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxRateAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Future))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Index))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Loan))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Mortgage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MutualFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RateIndex))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleCreditDefaultSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleFra))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ShortAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleIRSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(UnderlyingAsset))]
    /// EG 20140826 Add IncludeAttribute ShortAsset
    public abstract class Asset : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("instrumentId", Order = 1)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Id", IsClonable = true, IsMaster = true)]
        public InstrumentId[] instrumentId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description", Width = 600)]
        public EFS_String description;
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
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.DateRelativeTo)]
        public EFS_Id efs_id;
        #endregion Members
    }
    #endregion Asset
    #region AssetMeasureType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class AssetMeasureType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string assetMeasureScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
        #region Constructors
        public AssetMeasureType()
        {
            assetMeasureScheme = "http://www.fpml.org/coding-scheme/asset-measure-5-0";
        }
        #endregion Constructors
    }
    #endregion AssetMeasureType
    #region AssetReference
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class AssetReference : HrefGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "IDREF")]
        public string href;
        #endregion Members
    }
    #endregion AssetReference

    #region BasicQuotation
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BasicQuotation : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool ValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("value", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Value of the quotation")]
        public EFS_Decimal Value;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool measureTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("measureType", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public AssetMeasureType measureType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteUnits", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Optional units")]
        public PriceQuoteUnits quoteUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("side", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        public QuotationSideEnum side;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("timing", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quote timing")]
        public QuoteTiming timing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Quote location")]
        public EFS_RadioChoice quoteLocation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteLocationBusinessCenterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", typeof(BusinessCenter), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "BusinessCenter", IsVisible = true)]
        public BusinessCenter quoteLocationBusinessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteLocationExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeId", typeof(ExchangeId), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Exchange Id", IsVisible = true)]
        public ExchangeId quoteLocationExchangeId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("informationSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 9)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information sources")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source")]
        public InformationSource[] informationSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("time", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time when quote was observed or derived")]
        public EFS_DateTime time;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation date")]
        public EFS_Date valuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expiryTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expiryTime", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiry time")]
        public EFS_DateTime expiryTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashFlowTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashFlowType", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash flows type")]
        public CashflowType cashFlowType;
        #endregion Members
        #region Constructors
        public BasicQuotation()
        {
            quoteLocationBusinessCenter = new BusinessCenter();
            quoteLocationExchangeId = new ExchangeId();
        }
        #endregion Constructors
    }
    #endregion BasicQuotation
    #region Basket
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class Basket : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openUnitsSpecified;
		[System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Open Units", Width = 100)]
        public EFS_Decimal openUnits;
        [System.Xml.Serialization.XmlElementAttribute("basketConstituent", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Constituent", IsMaster = true, IsChild = true)]
        public BasketConstituent[] basketConstituent;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketDivisorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketDivisor", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = false, Name = "Divisor", Width = 100)]
        public EFS_Decimal basketDivisor;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketName", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket name")]
        public BasketName basketName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Basket Id")]
        public BasketId[] basketId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool basketCurrencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketCurrency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency basketCurrency;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedFlooredPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cappedFlooredPrice", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capped/Floored price")]
		public UnderlyerCappedFlooredPrice cappedFlooredPrice;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:Basket";
        #endregion Members
    }
    #endregion Basket
    #region BasketConstituent
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    public partial class BasketConstituent
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("deposit", typeof(Deposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedFund", typeof(ExchangeTradedFund), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("future", typeof(Future), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRateAsset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(Index), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("loan", typeof(Loan), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("mortgage", typeof(Mortgage), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("mutualFund", typeof(MutualFund), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("rateIndex", typeof(RateIndex), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleCreditDefaultSwap", typeof(SimpleCreditDefaultSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleFra", typeof(SimpleFra), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleIrSwap", typeof(SimpleIRSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Asset", IsVisible = false)]
        public UnderlyingAsset underlyingAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightSpecified;
        [System.Xml.Serialization.XmlElementAttribute("constituentWeight", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Asset")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent Weight")]
        public ConstituentWeight constituentWeight;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayout", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Payout")]
        public DividendPayout dividendPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("underlyerPrice", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer Price")]
        public Price underlyerPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerNotionalSpecified;
        [System.Xml.Serialization.XmlElementAttribute("underlyerNotional", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer Notional")]
        public Money underlyerNotional;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerSpreadSpecified;
        [System.Xml.Serialization.XmlElementAttribute("underlyerSpread", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Spread schedule reference")]
        public SpreadScheduleReference underlyerSpread;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon payment")]
        public PendingPayment couponPayment;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public EFS_Id efs_id;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
		public bool cappedFlooredPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cappedFlooredPrice", Order = 8)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End,Name = "Capped/Floored price")]
		public UnderlyerCappedFlooredPrice cappedFlooredPrice;
		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:BasketConstituent";
        #endregion Members
     }
    #endregion BasketConstituent
    #region BasketId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BasketId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string basketIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
    }
    #endregion BasketId
    #region BasketName
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class BasketName : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string basketNameScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
    }
    #endregion BasketName
    #region Bond
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("bond", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConvertibleBond))]
    // EG 20140702 Upd Interface (remove OTCmlId)
    /// EG 20150422 [20513] BANCAPERTA  
    public partial class Bond : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", IsCopyPaste = true)]
        public EFS_RadioChoice issuer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerName", typeof(EFS_String), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Name", IsVisible = true)]
        public EFS_String issuerName;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerPartyReference", typeof(PartyReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party reference", IsVisible = true)]
        public PartyReference issuerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool senioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("seniority", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repayment precedence of a debt instrument")]
        public CreditSeniority seniority;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponType", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Type")]
        public CouponType couponType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponRate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        public EFS_Decimal couponRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        public EFS_Date maturity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool parValueSpecified;
        [System.Xml.Serialization.XmlElementAttribute("parValue", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Nominal amount")]
        public EFS_Decimal parValue;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool faceAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("faceAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Total amount of issue")]
        public EFS_Decimal faceAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Bond";
        #endregion Members
    }
    #endregion Bond

    #region Cash
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("cash", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    public class Cash : Asset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        public Currency currency;
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
        public string type = "efs:Cash";
        #endregion Members
    }
    #endregion Cash
    #region Commission
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Commission : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("commissionDenomination", Order = 1)]
        [ControlGUI(Name = "denomination")]
        public CommissionDenominationEnum commissionDenomination;
        [System.Xml.Serialization.XmlElementAttribute("commissionAmount", Order = 2)]
        [ControlGUI(Name = "amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public EFS_Decimal commissionAmount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commissionPerTradeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("commissionPerTrade", typeof(EFS_Decimal), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Commission Per Trade")]
        public EFS_Decimal commissionPerTrade;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRate), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Rate", IsClonable = true, IsChild = true)]
        public FxRate[] fxRate;
        #endregion Members
    }
    #endregion Commission
    #region ConstituentWeight
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class ConstituentWeight : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Type")]
        public EFS_RadioChoice constituentWeight;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightBasketPercentageSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketPercentage", typeof(EFS_Decimal), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Basket Percentage", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal constituentWeightBasketPercentage;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightBasketAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basketAmount", typeof(Money), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Basket Amount", IsVisible = true)]
        public Money constituentWeightBasketAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentWeightOpenUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("openUnits", typeof(EFS_Decimal), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Open Units", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public EFS_Decimal constituentWeightOpenUnits;
        #endregion Members
        #region Constructors
        public ConstituentWeight()
        {
            constituentWeightBasketPercentage = new EFS_Decimal();
            constituentWeightBasketAmount = new Money();
            constituentWeightOpenUnits = new EFS_Decimal();
        }
        #endregion Constructors
    }
    #endregion ConstituentWeight
    #region ConvertibleBond
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("convertibleBond", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public partial class ConvertibleBond : Bond
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("underlyingEquity", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Equity", IsVisible = true)]
        public UnderlyingAsset underlyingEquity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlying Equity")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool redemptionDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("redemptionDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Redemption date")]
        public EFS_Date redemptionDate;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public new string type = "efs:ConvertibleBond";
        #endregion Members
    }
    #endregion ConvertibleBond
    #region CouponType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class CouponType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string couponTypeScheme;

        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
    }
    #endregion CouponType

    #region Deposit
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("deposit", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class Deposit : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("term", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Deposit";
        #endregion Members
    }
    #endregion Deposit
    #region DividendPayout
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class DividendPayout : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Dividend Payout")]
        public EFS_RadioChoice dividendPayout;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutRatioSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayoutRatio", typeof(EFS_Decimal), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Payout Ratio", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public EFS_Decimal dividendPayoutRatio;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutConditionsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayoutConditions", typeof(EFS_MultiLineString), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Dividend Payout Conditions", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 400)]
        public EFS_MultiLineString dividendPayoutConditions;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayment", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend payment")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend payment", IsChild = true)]
        public PendingPayment[] dividendPayment;
        #endregion Members
    }
    #endregion DividendPayout

    #region EquityAsset
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("equity", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
	public partial class EquityAsset : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:EquityAsset";
        #endregion Members
    }
    #endregion EquityAsset
    #region ExchangeTraded
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Bond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommodityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConvertibleBond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedContract))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Future))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Index))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(DebtSecurity))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ShortAsset))]
    /// EG 20140826 Add ShortAsset
    public abstract partial class ExchangeTraded : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool relatedExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("relatedExchangeId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ExchangeTraded", IsVisible = false, IsGroup = true)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Related Exchange Id")]
        public ExchangeId[] relatedExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool optionsExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("optionsExchangeId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Options Exchange Id")]
        public ExchangeId[] optionsExchangeId;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ExchangeTraded")]
		public bool FillBaliseExchangeTraded;
        #endregion Members
    }
    #endregion ExchangeTraded
    #region ExchangeTradedCalculatedPrice
    /// <summary>
    /// Abstract base class for all exchange traded financial products with a price which is calculated 
    /// from exchange traded constituents.
    /// </summary>
    // EG 20140526 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public abstract partial class ExchangeTradedCalculatedPrice : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool constituentExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("constituentExchangeId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ExchangeTradedCalculatedPrice", IsVisible = false, IsGroup = true)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent Exchange Ids")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Constituent Exchange Id")]
        public ExchangeId[] constituentExchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "ExchangeTradedCalculatedPrice")]
        public bool FillBaliseExchangeTradedCalculatedPrice;
        #endregion Members
    }
    #endregion ExchangeTradedCalculatedPrice
    #region ExchangeTradedContract
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class ExchangeTradedContract : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool contractReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("contractReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Reference")]
        public EFS_String contractReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expirationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expirationDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date")]
        public AdjustableOrRelativeDate expirationDate;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeTradedContract";
        #endregion Members
    }
    #endregion ExchangeTradedContract
    #region ExchangeTradedFund
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("exchangeTradedFund", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 New build FpML4.4 : ExchangeTradedCalculatedPrice replace ExchangeTraded
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class ExchangeTradedFund : ExchangeTradedCalculatedPrice
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fundManagerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fundManager", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Fund Manager")]
        public EFS_String fundManager;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:ExchangeTradedFund";
        #endregion Members
    }
    #endregion ExchangeTradedFund

    #region FacilityType
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FacilityType : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string facilityTypeScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "token")]
        public string Value;
        #endregion Members
        #region Constructors
        public FacilityType()
        {
            facilityTypeScheme = "http://www.fpml.org/coding-scheme/facility-type-1-0";
        }
        #endregion Constructors
    }
    #endregion FacilityType
    #region Future
    /// <summary>
    /// An exchange traded future contract
    /// </summary>
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("future", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public partial class Future : ExchangeTraded
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool multiplierSpecified;
        [System.Xml.Serialization.XmlElementAttribute("multiplier", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Multiplier", LblWidth = 70, Width = 32)]
        public EFS_Integer multiplier;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futureContractReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futureContractReference", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Contract Reference")]
        public EFS_String futureContractReference;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiration Date")]
        public EFS_Date maturity;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Future";
        #endregion Members
    }
    #endregion Future
    #region FutureId
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FutureId : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string futureIdScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Methods
        #region Clone
        public object Clone()
        {
            FutureId clone = new FutureId
            {
                futureIdScheme = this.futureIdScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion FutureId
    #region FxConversion
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class FxConversion : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice fxConversion;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(AmountReference), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        [ControlGUI(Name = "value")]
        public AmountReference fxConversionAmountRelativeTo;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionFxRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRate), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Conversion Fx rate")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Rate", IsClonable = true, IsChild = true)]
        public FxRate[] fxConversionFxRate;
        #endregion Members
        #region Constructors
        public FxConversion()
        {
            fxConversionAmountRelativeTo = new AmountReference();
            fxConversionFxRate           = new FxRate[1] { new FxRate() };
        }
        #endregion Constructors
    }
    #endregion FxConversion
    #region FxRateAsset
	//[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	//[System.Xml.Serialization.XmlRootAttribute("fxRate", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("fxRate", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    /// EG 20150302 change Namespace on quotedCurrencyPair (CFD Forex)
    public partial class FxRateAsset : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        //[System.Xml.Serialization.XmlElementAttribute("quotedCurrencyPair", Order = 1)]
        public QuotedCurrencyPair quotedCurrencyPair;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool rateSourceSpecified;
        //[System.Xml.Serialization.XmlElementAttribute("rateSource", typeof(FxSpotRateSource), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [System.Xml.Serialization.XmlElementAttribute("rateSource", typeof(FxSpotRateSource), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Rate source")]
        public FxSpotRateSource rateSource;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:FxRateAsset";

        #endregion Members
    }
    #endregion FxRateAsset

    #region Index
    /// <summary>
    /// A published index whose price depends on exchange traded constituents.
    /// </summary>
    // EG 20140526 New build FpML4.4
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("index", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public partial class Index : ExchangeTradedCalculatedPrice
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool futureIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("futureId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Future Id")]
        public FutureId futureId;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Index";
        #endregion Members
    }
    #endregion Index

    #region Lien
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class Lien : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string lienScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null, Width = 75)]
        public string Value;
        #endregion Members
        #region Constructors
        public Lien()
        {
            lienScheme = "http://www.fpml.org/coding-scheme/designated-priority-1-0";
        }
        #endregion Constructors
        #region Methods
        #region Clone
        public object Clone()
        {
            Lien clone = new Lien
            {
                lienScheme = this.lienScheme,
                Value = this.Value
            };
            return clone;
        }
        #endregion Clone
        #endregion Methods
    }
    #endregion Lien
    #region Loan
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("loan", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class Loan : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity")]
        public EFS_RadioChoice entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityBorrowerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("borrower", typeof(LegalEntity), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity entityBorrower;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityBorrowerReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("borrowerReference", typeof(LegalEntityReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntityReference entityBorrowerReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool lienSpecified;
        [System.Xml.Serialization.XmlElementAttribute("lien", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Seniority level of the lien")]
        public Lien lien;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool facilityTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("facilityType", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Facility type")]
        public FacilityType facilityType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity date")]
        public EFS_Date maturity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool creditAgreementDateSpecified;

        [System.Xml.Serialization.XmlElementAttribute("creditAgreementDate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Credit agreement date")]
        public EFS_Date creditAgreementDate;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trancheSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tranche", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Tranche")]
        public UnderlyingAssetTranche tranche;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Loan";
        #endregion Members
        #region Constructors
        public Loan()
        {
            entityBorrower = new LegalEntity();
            entityBorrowerReference = new LegalEntityReference();
        }
        #endregion Constructors
    }
    #endregion Loan

    #region Mortgage
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	[System.Xml.Serialization.XmlRootAttribute("mortgage", Namespace = "http://www.fpml.org/2007/FpML-4-4", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class Mortgage : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity")]
        public EFS_RadioChoice entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityInsurerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("insurer", typeof(LegalEntity), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity entityInsurer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityInsurerReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("insurerReference", typeof(LegalEntityReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntityReference entityInsurerReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Issuer", IsCopyPaste = true)]
        public EFS_RadioChoice issuer;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerNameSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerName", typeof(EFS_String), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Name", IsVisible = true)]
        public EFS_String issuerName;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool issuerPartyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("issuerPartyReference", typeof(PartyReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Party reference", IsVisible = true)]
        public PartyReference issuerPartyReference;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool senioritySpecified;
        [System.Xml.Serialization.XmlElementAttribute("seniority", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Repayment precedence of a debt instrument")]
        public CreditSeniority seniority;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponType", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Type")]
        public CouponType couponType;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponRateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponRate", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon Rate")]
        public EFS_Decimal couponRate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool maturitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("maturity", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Maturity")]
        public EFS_Date maturity;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool originalPrincipalAmountSpecified;
        [System.Xml.Serialization.XmlElementAttribute("originalPrincipalAmount", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Initial issued amount")]
        public EFS_Decimal originalPrincipalAmount;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool poolSpecified;
        [System.Xml.Serialization.XmlElementAttribute("pool", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mortgage pool")]
        public AssetPool pool;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sectorSpecified;
        [System.Xml.Serialization.XmlElementAttribute("sector", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 13)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mortgage sector")]
        public MortgageSector sector;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool trancheSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tranche", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 14)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Mortgage tranche")]
        public EFS_String tranche;

        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:Mortgage";
        #endregion Members
        #region Constructors
        public Mortgage()
        {
            entityInsurer = new LegalEntity();
            entityInsurerReference = new LegalEntityReference();
            issuerName = new EFS_String();
            issuerPartyReference = new PartyReference();
        }
        #endregion Constructors
    }
    #endregion Mortgage
    #region MortgageSector
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class MortgageSector : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        [System.ComponentModel.DefaultValueAttribute("http://www.fpml.org/coding-scheme/mortgage-sector-1-0")]
        public string mortgageSectorScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        public string Value;
        #endregion Members
        #region Constructors
        public MortgageSector()
        {
            mortgageSectorScheme = "http://www.fpml.org/coding-scheme/mortgage-sector-1-0";
        }
        #endregion Constructors
    }
    #endregion MortgageSector
    #region MutualFund
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("mutualFund", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class MutualFund : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openEndedFundSpecified;
        [System.Xml.Serialization.XmlElementAttribute("openEndedFund", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.Intermediary, Name = "Open Ended")]
        public EFS_Boolean openEndedFund;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fundManagerSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fundManager", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Fund Manager")]
        public EFS_String fundManager;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:MutualFund";
        #endregion Members
    }
    #endregion MutualFund

    #region PendingPayment
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class PendingPayment : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("paymentDate", Order = 1)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment date")]
        public EFS_Date paymentDate;
        [System.Xml.Serialization.XmlElementAttribute("amount", Order = 2)]
        [ControlGUI(Name = "Amount", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Money amount;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterest", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued interest on the dividend or coupon payment")]
        public Money accruedInterest;
        #endregion Members
    }
    #endregion PendingPayment
    #region Price
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ReturnLegValuationPrice))]
    public partial class Price : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool commissionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("commission", Order = 1)]
        //[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Price", IsVisible = false)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Commission")]
        public Commission commission;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool grossPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("grossPrice", typeof(ActualPrice), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "grossPrice")]
        public ActualPrice grossPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsLabel = false, Name = "Type")]
        public EFS_RadioChoice price;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceDeterminationMethodSpecified;
        [System.Xml.Serialization.XmlElementAttribute("determinationMethod", typeof(EFS_MultiLineString), Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Determination Method", IsVisible = true)]
        [ControlGUI(IsPrimary = false, Name = "value", Width = 500)]
        public EFS_MultiLineString priceDeterminationMethod;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceAmountRelativeToSpecified;
        [System.Xml.Serialization.XmlElementAttribute("amountRelativeTo", typeof(AmountReference), Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Amount Relative To", IsVisible = true)]
        [ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.Notional)]
        public AmountReference priceAmountRelativeTo;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool priceNetPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("netPrice", typeof(ActualPrice), Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Net Price", IsVisible = true)]
        public ActualPrice priceNetPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool accruedInterestPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("accruedInterestPrice", typeof(EFS_Decimal), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Accrued Interest Price")]
        public EFS_Decimal accruedInterestPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool fxConversionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("fxConversion", typeof(FxConversion), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Fx Conversion")]
        public FxConversion fxConversion;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Net Price")]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cleanNetPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cleanNetPrice", typeof(EFS_Decimal), Order = 8)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clean Net Price")]
        public EFS_Decimal cleanNetPrice;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quotationCharacteristicsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quotationCharacteristics", typeof(QuotationCharacteristics), Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quotation Characteristics")]
        public QuotationCharacteristics quotationCharacteristics;
        #endregion Members
    }
    #endregion Price
    #region PriceQuoteUnits
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class PriceQuoteUnits : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string priceQuoteUnitsScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
        #region Constructors
        public PriceQuoteUnits()
        {
            priceQuoteUnitsScheme = "http://www.fpml.org/coding-scheme/price-quote-units-1-1";
        }
        #endregion Constructors
    }
    #endregion PriceQuoteUnits

    #region QuotationCharacteristics
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QuotationCharacteristics : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool measureTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("measureType", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Type")]
        public AssetMeasureType measureType;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("quoteUnits", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Optional units")]
        public PriceQuoteUnits quoteUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool sideSpecified;
        [System.Xml.Serialization.XmlElementAttribute("side", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Side")]
        public QuotationSideEnum side;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("timing", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Quote timing")]
        public QuoteTiming timing;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Quote location")]
        public EFS_RadioChoice quoteLocation;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteLocationBusinessCenterSpecified;
        [System.Xml.Serialization.XmlElementAttribute("businessCenter", typeof(BusinessCenter), Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "BusinessCenter", IsVisible = true)]
        public BusinessCenter quoteLocationBusinessCenter;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool quoteLocationExchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeId", typeof(ExchangeId), Order = 7)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Exchange Id", IsVisible = true)]
        public ExchangeId quoteLocationExchangeId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool informationSourceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("informationSource", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 8)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information sources")]
        [ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Information source")]
        public InformationSource[] informationSource;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool timeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("time", Order = 9)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Time when quote was observed or derived")]
        public EFS_DateTime time;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool valuationDateSpecified;
        [System.Xml.Serialization.XmlElementAttribute("valuationDate", Order = 10)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Valuation date")]
        public EFS_Date valuationDate;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool expiryTimeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("expiryTime", Order = 11)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Expiry time")]
        public EFS_DateTime expiryTime;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cashFlowTypeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cashFlowType", Order = 12)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Cash flows type")]
        public CashflowType cashFlowType;
        #endregion Members
        #region Constructors
        public QuotationCharacteristics()
        {
            quoteLocationBusinessCenter	= new BusinessCenter();
            quoteLocationExchangeId		= new ExchangeId();
			time						= new EFS_DateTime();
			valuationDate				= new EFS_Date();
			expiryTime					= new EFS_DateTime();
			cashFlowType				= new CashflowType();
        }
        #endregion Constructors
    }
    #endregion QuotationCharacteristics
    #region QuoteTiming
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class QuoteTiming : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified, DataType = "anyURI")]
        public string quoteTimingScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "normalizedString")]
        [ControlGUI(IsLabel = false, Name = null)]
        public string Value;
        #endregion Members
        #region Constructors
        public QuoteTiming()
        {
            quoteTimingScheme = "http://www.fpml.org/coding-scheme/quote-timing-1-0";
        }
        #endregion Constructors
    }
    #endregion QuoteTiming

    #region RateIndex
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("rateIndex", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class RateIndex : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("floatingRateIndex", Namespace = "http://www.fpml.org/2007/FpML-4-4",Order=1)]
        [ControlGUI(Name = "Index", LineFeed = MethodsGUI.LineFeedEnum.After, Width = 295)]
        public FloatingRateIndex floatingRateIndex;
        [System.Xml.Serialization.XmlElementAttribute("term", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:RateIndex";
        #endregion Members
    }
    #endregion RateIndex

    #region SimpleCreditDefaultSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("simpleCreditDefaultSwap", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class SimpleCreditDefaultSwap : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Entity")]
        public EFS_RadioChoice entity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityReferenceEntitySpecified;
        [System.Xml.Serialization.XmlElementAttribute("referenceEntity", typeof(LegalEntity), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntity entityReferenceEntity;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool entityCreditEntityReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("creditEntityReference", typeof(LegalEntityReference), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public LegalEntityReference entityCreditEntityReference;

        [System.Xml.Serialization.XmlElementAttribute("term", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Name = "Payment Frequency", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:SimpleCreditDefaultSwap";
        #endregion Members
        #region Constructors
        public SimpleCreditDefaultSwap()
        {
            entityReferenceEntity       = new LegalEntity();
            entityCreditEntityReference = new LegalEntityReference();
        }
        #endregion Constructors
    }
    #endregion SimpleCreditDefaultSwap
    #region SimpleFra
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("simpleFra", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class SimpleFra : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("startTerm", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [ControlGUI(Name = "Start Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval startTerm;
        [System.Xml.Serialization.XmlElementAttribute("endTerm", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [ControlGUI(Name = "End Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval endTerm;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:SimpleFra";
        #endregion Members
    }
    #endregion SimpleFra
    #region SimpleIRSwap
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("simpleIrSwap", Namespace = "http://www.efs.org/2007/EFSmL-3-0", IsNullable = false)]
    // EG 20140702 Upd Interface (remove OTCmlId)
    public class SimpleIRSwap : UnderlyingAsset
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("term", Order = 1)]
        [ControlGUI(Name = "Term", LineFeed = MethodsGUI.LineFeedEnum.After)]
        public Interval term;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool paymentFrequencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("paymentFrequency", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Payment Frequency")]
        public Interval paymentFrequency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dayCountFractionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dayCountFraction", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "dayCountFraction")]
        public DayCountFractionEnum dayCountFraction;
        [System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
             Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
        public string type = "efs:SimpleIRSwap";
        #endregion Members
    }
    #endregion SimpleIRSwap
    #region SingleUnderlyer
    //[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
	[System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    /// EG 20150302 Add  notionalBase|notionalBaseSpecified (CFD Forex)
    public partial class SingleUnderlyer : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlElementAttribute("bond", typeof(Bond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("convertibleBond", typeof(ConvertibleBond), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("deposit", typeof(Deposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equity", typeof(EquityAsset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedFund", typeof(ExchangeTradedFund), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("future", typeof(Future), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxRate", typeof(FxRateAsset), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("index", typeof(Index), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("loan", typeof(Loan), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("mortgage", typeof(Mortgage), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("mutualFund", typeof(MutualFund), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("rateIndex", typeof(RateIndex), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleCreditDefaultSwap", typeof(SimpleCreditDefaultSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleFra", typeof(SimpleFra), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("simpleIrSwap", typeof(SimpleIRSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public UnderlyingAsset underlyingAsset;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool openUnitsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("openUnits", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Open Units", Width = 150)]
        public EFS_Decimal openUnits;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool dividendPayoutSpecified;
        [System.Xml.Serialization.XmlElementAttribute("dividendPayout", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Dividend Payout")]
        public DividendPayout dividendPayout;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool couponPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("couponPayment", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Coupon payment")]
        public PendingPayment couponPayment;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool cappedFlooredPriceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("cappedFlooredPrice", Order = 5)]
		[CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
		[ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Capped/Floored price")]
		public UnderlyerCappedFlooredPrice cappedFlooredPrice;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool notionalBaseSpecified;
        [System.Xml.Serialization.XmlElementAttribute("notionalBase", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, IsDisplay = true, Name = "Notional base", Width = 150)]
        public Money notionalBase;

		[System.Xml.Serialization.XmlAttributeAttribute(Namespace = "http://www.w3.org/2001/XMLSchema-instance",
			 Form = System.Xml.Schema.XmlSchemaForm.Qualified, DataType = "normalizedString")]
		public string type = "efs:SingleUnderlyer";
        #endregion Members
    }
    #endregion SingleUnderlyer

    #region Underlyer
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public partial class Underlyer : ItemGUI
    {
        #region Members
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Underlyer")]
        public EFS_RadioChoice underlyer;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerSingleSpecified;
        [System.Xml.Serialization.XmlElementAttribute("singleUnderlyer", typeof(SingleUnderlyer), Order = 1)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Single Underlyer", IsVisible = true)]
        public SingleUnderlyer underlyerSingle;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool underlyerBasketSpecified;
        [System.Xml.Serialization.XmlElementAttribute("basket", typeof(Basket), Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        [OpenDivGUI(Level = MethodsGUI.LevelEnum.HiddenKey, Name = "Underlyer Basket", IsVisible = true)]
        public Basket underlyerBasket;
        #endregion Members
    }
    #endregion Underlyer
    #region UnderlyingAsset
    /// <summary>
    /// Abstract base class for all underlying assets.
    /// </summary>
    // EG 20140702 New build FpML4.4
    // EG 20140702 Upd Interface (Add OTCmlId)
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Basket))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Bond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(CommodityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ConvertibleBond))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Deposit))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(EquityAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTraded))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedContract))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ExchangeTradedFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Future))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(FxRateAsset))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Index))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Loan))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(Mortgage))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(MutualFund))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(RateIndex))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleCreditDefaultSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(SimpleIRSwap))]
    [System.Xml.Serialization.XmlIncludeAttribute(typeof(ShortAsset))]
    /// EG 20140826 Add ShortAsset
	public partial class UnderlyingAsset : ItemGUI
    {
        #region Members
        #region IdentifiedAsset / Asset
        [System.Xml.Serialization.XmlElementAttribute("instrumentId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
		[OpenDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyingAsset", IsVisible = false, IsGroup = true)]
		[ArrayDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "Instrument Id", IsClonable = true, IsMaster = true)]
        public InstrumentId[] instrumentId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool descriptionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("description", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 2)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Description", Width = 600)]
        public EFS_String description;
        #endregion IdentifiedAsset / Asset

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool currencySpecified;
        [System.Xml.Serialization.XmlElementAttribute("currency", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 3)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Currency", Width = 75)]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public Currency currency;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool exchangeIdSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeId", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 4)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Exchange Id")]
        public ExchangeId exchangeId;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool clearanceSystemSpecified;
        [System.Xml.Serialization.XmlElementAttribute("clearanceSystem", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 5)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Clearance System")]
        [ControlGUI(IsPrimary = false, Name = "value")]
        public ClearanceSystem clearanceSystem;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.None)]
        public bool definitionSpecified;
        [System.Xml.Serialization.XmlElementAttribute("definition", Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 6)]
        [CreateControlGUI(Declare = MethodsGUI.CreateControlEnum.Optional)]
        [ControlGUI(Level = MethodsGUI.LevelEnum.End, Name = "Definition System")]
        public ProductReference definition;
		[System.Xml.Serialization.XmlIgnoreAttribute()]
		[CloseDivGUI(Level = MethodsGUI.LevelEnum.End, Name = "UnderlyingAsset")]
		[ReferenceGUI(Reference = MethodsGUI.ReferenceEnum.AssetReference)]
		public EFS_Id efs_id;

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
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion Members
    }
    #endregion UnderlyingAsset
    #region UnderlyingAssetTranche
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.fpml.org/2007/FpML-4-4")]
    public class UnderlyingAssetTranche : SchemeGUI
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute(DataType = "anyURI")]
        public string loanTrancheScheme;
        [System.Xml.Serialization.XmlTextAttribute(DataType = "token")]
        public string Value;
        #endregion Members
        #region Constructors
        public UnderlyingAssetTranche()
        {
            loanTrancheScheme = "http://www.fpml.org/coding-scheme/underlying-asset-tranche";
        }
        #endregion Constructors
    }
    #endregion UnderlyingAssetTranche
}