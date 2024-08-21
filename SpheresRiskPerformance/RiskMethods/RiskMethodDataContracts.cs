using System;
using System.Runtime.Serialization;
//
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.SpheresRiskPerformance.Enum;
//
using EfsML.Enum;
using EfsML.Enum.Tools;
//
using FpML.Enum;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    #region Common
    /// <summary>
    /// Class containing the parameters to evaluate the delivery deposit for a specific ETD asset
    /// </summary>
    [DataContract(
        Name = DataHelper<AssetDeliveryParameter>.DATASETROWNAME,
        Namespace = DataHelper<AssetDeliveryParameter>.DATASETNAMESPACE)]
    internal class AssetDeliveryParameter
    {
        /// <summary>
        /// Derivative contract Id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        internal int ContractId
        { get; set; }

        /// <summary>
        /// Asset Id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 2)]
        internal int AssetId
        { get; set; }

        /// <summary>
        ///  Expression type of the multiplier
        /// </summary>
        [DataMember(Name = "IMEXPRESSIONTYPE", Order = 3)]
        internal ExpressionType ExpressionType
        { get; set; }

        /// <summary>
        /// Currency 
        /// </summary>
        /// <remarks>
        /// (when expression type is Percentage, then the currency will be filled with the derivative contract price currency)
        /// </remarks>
        [DataMember(Name = "IDC", Order = 4)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Value for the quantity to be delivered
        /// </summary>
        [DataMember(Name = "IMDELIVERYAMOUNT", Order = 5)]
        public double DeliveryValueDouble
        { get; set; }
        public decimal DeliveryValue
        { get { return (decimal)DeliveryValueDouble; } }

        /// <summary>
        /// Contract size, it will be used by the parameters with expression type "percentage"
        /// </summary>
        [DataMember(Name = "FACTOR", Order = 6)]
        public double ContractSizeDouble
        { get; set; }
        public decimal ContractSize
        { get { return (decimal)ContractSizeDouble; } }

        /// <summary>
        /// Category of the underlying asset,
        /// it will be used by the parameters with expression type "percentage" to get the quote
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag167.html</remarks>
        [DataMember(Name = "ASSETCATEGORY", Order = 7)]
        public Cst.UnderlyingAsset UnlCategory
        { get; set; }

        /// <summary>
        /// Underlying asset Id, it will be used by the parameters with expression type "percentage" to get the quote
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 8)]
        internal int UnlAssetId
        { get; set; }

        /// <summary>
        /// Category of contract (F for Futures, O for Options),
        /// it will be used by the parameters with expression type "percentage" to get the quote
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 9)]
        public string ContractCategory
        { get; set; }

        /// <summary>
        /// Etape de livraison
        /// </summary>
        // PM 20130904 [17949] ajout DeliveryStep
        [DataMember(Name = "DELIVERYSTEP", Order = 10)]
        internal InitialMarginDeliveryStepEnum DeliveryStep
        { get; set; }

        /// <summary>
        /// Current connection string to the data base
        /// </summary>
        public string ConnectionString
        { get; set; }

    }

    /// <summary>
    /// Information sur les Commodity Contract
    /// </summary>
    /// <remarks>Main Spheres reference tables: COMMODITYCONTRACT, ASSET_COMMODITY, MARKET </remarks>
    /// PM 20170808 [23371] New
    [DataContract(Name = DataHelper<CommodityContractParameter>.DATASETROWNAME,
        Namespace = DataHelper<CommodityContractParameter>.DATASETNAMESPACE)]
    internal sealed class CommodityContractParameter
    {
        /// <summary>
        /// Id interne du Commodity Contract
        /// </summary>
        [DataMember(Name = "IDCC", Order = 1)]
        public int IdCC
        { get; set; }

        /// <summary>
        /// Identifiant du Commodity Contract
        /// </summary>
        [DataMember(Name = "CONTRACTIDENTIFIER", Order = 2)]
        public string Identifier
        { get; set; }

        /// <summary>
        /// Symbol du Commodity Contract
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 3)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Devise du Commodity Contract
        /// </summary>
        [DataMember(Name = "IDC", Order = 4)]
        public string Currency
        { get; set; }
    }

    #endregion Common

    #region Custom method

    /// <summary>
    /// Class representing an asset with regards to the custom method
    /// </summary>
    [DataContract(Name = DataHelper<AssetCustom>.DATASETROWNAME,
        Namespace = DataHelper<AssetCustom>.DATASETNAMESPACE)]
    internal sealed class AssetCustom
    {
        /// <summary>
        /// Derivative Contract identifier
        /// </summary>
        [DataMember(Name = "CONTRACT", Order = 1)]
        public string Contract
        { get; set; }

        /// <summary>
        /// Derivative contract internal id
        /// </summary>
        [DataMember(Name = "CONTRACTID", Order = 2)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Derivative contract category
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 3)]
        public string Category
        { get; set; }

        /// <summary>
        /// Currency identifier of the derivative contract price
        /// </summary>
        [DataMember(Name = "CURRENCY", Order = 4)]
        public string Currency
        { get; set; }

        /// <summary>
        /// PutCall info (null for dc futures)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag201.html</remarks>
        [DataMember(Name = "PUTCALL", Order = 5)]
        public string PutOrCall
        { get; set; }

        /// <summary>
        /// Id of the asset
        /// </summary>
        [DataMember(Name = "ASSETID", Order = 6)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Category of the underlying asset (for contracts option only)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag167.html</remarks>
        [DataMember(Name = "UNDERLYERCATEGORY", Order = 7)]
        public string CategoryUnderlyer
        { get; set; }

        /// <summary>
        /// id of the underlying asset (for contracts option only)
        /// </summary>
        [DataMember(Name = "UNDERLYERID", Order = 8)]
        public int IdUnderlyer
        { get; set; }

        /// <summary>
        /// id of the underlying asset (for contracts option only)
        /// </summary>
        [DataMember(Name = "UNDERLYERCONTRACTID", Order = 9)]
        public int ContractIdUnderlyer
        { get; set; }

        /// <summary>
        /// Contract multiplier for the asset quote, extracted from the DERIVATIVECONTRACT table. Used for expression type "percentage" only
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 10)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } }
    }

    /// <summary>
    /// Parameters containing a parameters set entry for the custom method
    /// </summary>
    [DataContract(Name = DataHelper<ParameterCustom>.DATASETROWNAME,
        Namespace = DataHelper<ParameterCustom>.DATASETNAMESPACE)]
    internal sealed class ParameterCustom : IDataContractEnabled
    {
        /// <summary>
        /// Derivative contract internal id
        /// </summary>
        [DataMember(Name = "CONTRACTID", Order = 1)]
        public int ContractId
        { get; set; }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Starting parameter validity date
        /// </summary>
        [DataMember(Name = "PARAM_DTEN", Order = 2)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Ending parameter validity date
        /// </summary>
        [DataMember(Name = "PARAM_DTDIS", Order = 3)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Expression type for the multipliers 
        /// </summary>
        [DataMember(Name = "EXPRESSIONTYPE", Order = 4)]
        public ExpressionType ExpressionType
        { get; set; }

        /// <summary>
        /// Multiplier for future straddles
        /// </summary>
        [DataMember(Name = "IMSTRADDLE", Order = 5)]
        public double FutureStraddleMultiplierDouble
        { get; set; }
        public decimal FutureStraddleMultiplier
        { get { return (decimal)FutureStraddleMultiplierDouble; } }

        /// <summary>
        /// Multiplier for futures or in long or in short position 
        /// </summary>
        [DataMember(Name = "IMNORMAL", Order = 6)]
        public double FutureNormalMultiplierDouble
        { get; set; }
        public decimal FutureNormalMultiplier
        { get { return (decimal)FutureNormalMultiplierDouble; } }

        /// <summary>
        /// Multiplier for call options in long position
        /// </summary>
        [DataMember(Name = "IMLONGCALL", Order = 7)]
        public double LongCallMultiplierDouble
        { get; set; }
        public decimal LongCallMultiplier
        { get { return (decimal)LongCallMultiplierDouble; } }

        /// <summary>
        /// Multiplier for put options in long position
        /// </summary>
        [DataMember(Name = "IMLONGPUT", Order = 8)]
        public double LongPutMultiplierDouble
        { get; set; }
        public decimal LongPutMultiplier
        { get { return (decimal)LongPutMultiplierDouble; } }

        /// <summary>
        /// Multiplier for call options in short position
        /// </summary>
        [DataMember(Name = "IMSHORTCALL", Order = 9)]
        public double ShortCallMultiplierDouble
        { get; set; }
        public decimal ShortCallMultiplier
        { get { return (decimal)ShortCallMultiplierDouble; } }

        /// <summary>
        /// Multiplier for put options in short position
        /// </summary>
        [DataMember(Name = "IMSHORTPUT", Order = 10)]
        public double ShortPutMultiplierDouble
        { get; set; }
        public decimal ShortPutMultiplier
        { get { return (decimal)ShortPutMultiplierDouble; } }

        /// <summary>
        /// currency
        /// </summary>
        [DataMember(Name = "IMCURRENCY", Order = 11)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Underlyer quote, it will be used by the parameters with expression type "percentage"
        /// </summary>
        public decimal Quote
        { get; set; }

        /// <summary>
        /// Flag indicating when the parameter does not exist
        /// </summary>
        public bool Missing
        { get; set; }
    }

    #endregion Custom method

    #region TIMS IDEM method

    /// <summary>
    /// Data container for the TIMS IDEM "class" parameters (including underlyings quotes)
    /// </summary>
    /// <remarks></remarks>
    [DataContract(Name = DataHelper<ClassParameterTimsIdem>.DATASETROWNAME,
        Namespace = DataHelper<ClassParameterTimsIdem>.DATASETNAMESPACE)]
    internal sealed class ClassParameterTimsIdem
    {
        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Underlying asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 2)]
        public int UnlAssetId
        { get; set; }

        /// <summary>
        /// Underlying asset quote
        /// </summary>
        [DataMember(Name = "ASSET_UNL_QUOTE", Order = 3)]
        public double AssetUnlQuoteDouble
        { get; set; }
        public decimal AssetUnlQuote
        { get { return (decimal)AssetUnlQuoteDouble; } set { AssetUnlQuoteDouble = (double)value; } }

        /// <summary>
        /// Underlying asset quote, as extracted from the "class" file
        /// </summary>
        [DataMember(Name = "CLASS_ASSET_UNL_QUOTE", Order = 4)]
        public double ClassFileAssetUnlQuoteDouble
        { get; set; }
        public decimal ClassFileAssetUnlQuote
        { get { return (decimal)ClassFileAssetUnlQuoteDouble; } set { ClassFileAssetUnlQuoteDouble = (double)value; } }

        /// <summary>
        /// Class symbol, identifying the second level of the IDEM product hierarchy
        /// </summary>
        [DataMember(Name = "CLASSGROUP", Order = 5)]
        public string ClassGroup
        { get; set; }

        /// <summary>
        /// Group symbol, identifying the first level of the IDEM product hierarchy
        /// </summary>
        [DataMember(Name = "PRODUCTGROUP", Order = 6)]
        public string ProductGroup
        { get; set; }

        /// <summary>
        /// Contract multiplier , extracted from the DERIVATIVECONTRACT table
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 7)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } set { ContractMultiplierDouble = (double)value; } }

        /// <summary>
        /// Contract multiplier ,  extracted from the "class" file
        /// </summary>
        [DataMember(Name = "CLASS_CONTRACTMULTIPLIER", Order = 8)]
        public double ClassFileContractMultiplierDouble
        { get; set; }
        public decimal ClassFileContractMultiplier
        { get { return (decimal)ClassFileContractMultiplierDouble; } set { ClassFileContractMultiplierDouble = (double)value; } }

        /// <summary>
        /// Market contract symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 9)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// ??
        /// </summary>
        [DataMember(Name = "DELIVERYRATE", Order = 10)]
        public double DeliveryRateDouble
        { get; set; }
        public decimal DeliveryRate
        { get { return (decimal)DeliveryRateDouble; } set { DeliveryRateDouble = (double)value; } }

        /// <summary>
        /// Risk offset of the current class in comparison with the other product classes (expressed as a percentage from 0 to 1)
        /// </summary>
        /// <value>0..1</value>
        [DataMember(Name = "OFFSET", Order = 11)]
        public double OffsetDouble
        { get; set; }
        public decimal Offset
        { get { return (decimal)OffsetDouble; } set { OffsetDouble = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "SPOTSPREADRATE", Order = 12)]
        public double SpotSpreadRateDouble
        { get; set; }
        public decimal SpotSpreadRate
        { get { return (decimal)SpotSpreadRateDouble; } set { SpotSpreadRateDouble = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "NONSPOTSPREADRATE", Order = 13)]
        public double NonSpotSpreadRateDouble
        { get; set; }
        public decimal NonSpotSpreadRate
        { get { return (decimal)NonSpotSpreadRateDouble; } set { NonSpotSpreadRateDouble = (double)value; } }

        /// <summary>
        /// Minimum margin for the current class
        /// </summary>
        [DataMember(Name = "MINIMUMMARGIN", Order = 14)]
        public double MinimumMarginDouble
        { get; set; }
        public decimal MinimumMargin
        { get { return (decimal)MinimumMarginDouble; } set { MinimumMarginDouble = (double)value; } }

        /// <summary>
        /// ??
        /// </summary>
        [DataMember(Name = "EXPIRYTIME", Order = 15)]
        public string ExpiryTime
        { get; set; }

        /// <summary>
        /// contract category 
        /// </summary>
        /// <value>F for futures, O for options</value>
        [DataMember(Name = "CATEGORY", Order = 16)]
        public string Category
        { get; set; }

        /// <summary>
        /// Contract currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 17)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Contract description (column display name)
        /// </summary>
        [DataMember(Name = "CONTRACTDESCRIPTION", Order = 18)]
        public string ContractDescription
        { get; set; }

        /// <summary>
        /// Maturity Rule Frequency
        /// </summary>
        [DataMember(Name = "MMYRULE", Order = 19)]
        public string MaturityRuleFrequency
        { get; set; }

        /// <summary>
        /// Underlying Contract Symbol
        /// </summary>
        // PM 20130422 [18592] Ajout UnderlyingContractSymbol
        [DataMember(Name = "UNDERLYINGCONTRACT", Order = 20)]
        public string UnderlyingContractSymbol
        { get; set; }

        /// <summary>
        /// Underlying Isin Code
        /// </summary>
        /// PM 20170222 [22881][22942] Ajout UnlIsinCode
        [DataMember(Name = "ISINCODE", Order = 21)]
        public string UnlIsinCode
        { get; set; }
        
        /// <summary>
        /// Indique si le DC est actif ou non
        /// </summary>
        /// PM 20170222 [22881][22942] Ajout IsActive
        [DataMember(Name = "ISACTIVE", Order = 22)]
        public bool IsActive
        { get; set; }

        public ClassParameterTimsIdem Clone
            (int pContractId, decimal pContractMultiplier, decimal pClassFileContractMultiplier, string pContractSymbol)
        {
            return
                new ClassParameterTimsIdem
                {
                    //
                    ContractId = pContractId,
                    ContractMultiplier = pContractMultiplier,
                    ClassFileContractMultiplier = pClassFileContractMultiplier,
                    ContractSymbol = pContractSymbol,
                    //

                    UnlAssetId = this.UnlAssetId,
                    AssetUnlQuote = this.AssetUnlQuote,
                    ClassFileAssetUnlQuote = this.ClassFileAssetUnlQuote,
                    ClassGroup = this.ClassGroup,
                    ProductGroup = this.ProductGroup,
                    DeliveryRate = this.DeliveryRate,
                    Offset = this.Offset,
                    SpotSpreadRate = this.SpotSpreadRate,
                    NonSpotSpreadRate = this.NonSpotSpreadRate,
                    MinimumMargin = this.MinimumMargin,
                    ExpiryTime = this.ExpiryTime,
                    Category = this.Category,
                    Currency = this.Currency,
                    ContractDescription = this.ContractDescription,
                    //MarketId = this.MarketId,

                    // PM 20130422 [18592] Ajout UnderlyingContractSymbol
                    UnderlyingContractSymbol = this.UnderlyingContractSymbol,
                    // PM 20170222 [22881][22942] Ajout UnlIsinCode
                    UnlIsinCode = this.UnlIsinCode,
                    // PM 20170222 [22881][22942] Ajout IsActive
                    IsActive = this.IsActive,
                };
        }
    }

    /// <summary>
    /// Data container for the TIMS IDEM "risk array" parameters (including F and O contracts quotes)
    /// </summary>
    [DataContract(Name = DataHelper<RiskArrayParameterTimsIdem>.DATASETROWNAME,
        Namespace = DataHelper<RiskArrayParameterTimsIdem>.DATASETNAMESPACE)]
    internal sealed class RiskArrayParameterTimsIdem
    {
        /// <summary>
        /// ETD Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Asset contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 2)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Asset quote on the current business day
        /// </summary>
        [DataMember(Name = "QUOTE", Order = 3)]
        public double QuoteDouble
        { get; set; }
        public decimal Quote
        { get { return (decimal)QuoteDouble; } set { QuoteDouble = (double)value; } }

        /// <summary>
        /// Asset quote as extracted from the "risk array" file
        /// </summary>
        [DataMember(Name = "RISKARRAY_QUOTE", Order = 4)]
        public double RiskArrayQuoteDouble
        { get; set; }
        public decimal RiskArrayQuote
        { get { return (decimal)RiskArrayQuoteDouble; } set { RiskArrayQuoteDouble = (double)value; } }

        /// <summary>
        /// Asset strike (to be compared with the official asset strike stocked in the ASSET_ETD table)
        /// </summary>
        [DataMember(Name = "RISKARRAY_STRIKEPRICE", Order = 5)]
        public double RiskArrayStrikePriceDouble
        { get; set; }
        public decimal RiskArrayStrikePrice
        { get { return (decimal)RiskArrayStrikePriceDouble; } set { RiskArrayStrikePriceDouble = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "DOWNSIDE5", Order = 6)]
        public double DownSide5Double
        { get; set; }
        public decimal DownSide5
        { get { return (decimal)DownSide5Double; } set { DownSide5Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "DOWNSIDE4", Order = 7)]
        public double DownSide4Double
        { get; set; }
        public decimal DownSide4
        { get { return (decimal)DownSide4Double; } set { DownSide4Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "DOWNSIDE3", Order = 8)]
        public double DownSide3Double
        { get; set; }
        public decimal DownSide3
        { get { return (decimal)DownSide3Double; } set { DownSide3Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "DOWNSIDE2", Order = 9)]
        public double DownSide2Double
        { get; set; }
        public decimal DownSide2
        { get { return (decimal)DownSide2Double; } set { DownSide2Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "DOWNSIDE1", Order = 10)]
        public double DownSide1Double
        { get; set; }
        public decimal DownSide1
        { get { return (decimal)DownSide1Double; } set { DownSide1Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "UPSIDE1", Order = 11)]
        public double UpSide1Double
        { get; set; }
        public decimal UpSide1
        { get { return (decimal)UpSide1Double; } set { UpSide1Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "UPSIDE2", Order = 12)]
        public double UpSide2Double
        { get; set; }
        public decimal UpSide2
        { get { return (decimal)UpSide2Double; } set { UpSide2Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "UPSIDE3", Order = 13)]
        public double UpSide3Double
        { get; set; }
        public decimal UpSide3
        { get { return (decimal)UpSide3Double; } set { UpSide3Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "UPSIDE4", Order = 14)]
        public double UpSide4Double
        { get; set; }
        public decimal UpSide4
        { get { return (decimal)UpSide4Double; } set { UpSide4Double = (double)value; } }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(Name = "UPSIDE5", Order = 15)]
        public double UpSide5Double
        { get; set; }
        public decimal UpSide5
        { get { return (decimal)UpSide5Double; } set { UpSide5Double = (double)value; } }

        /// <summary>
        /// Remplacement value for short positions in case of the UPSIDE5 parameter is out of limits
        /// </summary>
        [DataMember(Name = "SHORTADJ", Order = 16)]
        public double ShortAdjDouble
        { get; set; }
        public decimal ShortAdj
        { get { return (decimal)ShortAdjDouble; } set { ShortAdjDouble = (double)value; } }

        /// <summary>
        /// Asset maturity, 
        /// if NULL the current risk array parameter is valid for all the assets owned by the same derivative contract.
        /// </summary>
        /// <remarks>if NULL the current risk array parameter will be spread along to the complete assets set</remarks>
        [DataMember(Name = "MATURITYYEARMONTH", Order = 17)]
        public double MaturityYearMonthDouble
        { get; set; }
        public decimal MaturityYearMonth
        { get { return (decimal)MaturityYearMonthDouble; } set { MaturityYearMonthDouble = (double)value; } }

        // UNDONE 20111026 MF Remplacer le type string du membre Category avec une énum
        /// <summary>
        /// contract category 
        /// </summary>
        /// <value>F for futures, O for options</value>
        [DataMember(Name = "CATEGORY", Order = 18)]
        public string Category
        { get; set; }

        /// <summary>
        /// contract symbol
        /// </summary>
        /// <value>F for futures, O for options</value>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 19)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Underlying Isin Code
        /// </summary>
        /// PM 20170222 [22881][22942] Ajout UnlIsinCode
        [DataMember(Name = "ISINCODE", Order = 20)]
        public string UnlIsinCode
        { get; set; }

        /// <summary>
        /// Indique si le DC est actif ou non
        /// </summary>
        /// PM 20170222 [22881][22942] Ajout IsActive
        [DataMember(Name = "ISACTIVE", Order = 21)]
        public bool IsActive
        { get; set; }

        /// <summary>
        /// type for the specific parameter (depending either from category and quantity values)
        /// </summary>
        public RiskMethodQtyType Type
        { get; set; }

        /// <summary>
        /// Official asset strike 
        /// </summary>
        public decimal StrikePrice
        { get; set; }

        /// <summary>
        /// The cross margin is activated for the specific parameter, we consider this flag just for RiskMethodQtyType.PositionAction parameters
        /// </summary>
        public bool CrossMarginActivated
        { get; set; }

        /// <summary>
        /// Get a copy of the reference parameter for the input asset
        /// </summary>
        /// <param name="pAssetId">the asset that must share the same parameters of the referenced one</param>
        /// <param name="pContractID">contract of the asset</param>
        /// <returns>clone of the reference object</returns>
        public RiskArrayParameterTimsIdem Clone(int pAssetId, int pContractID)
        {
            return
                new RiskArrayParameterTimsIdem
                {

                    AssetId = pAssetId,

                    ContractId = pContractID,

                    Quote = this.Quote,
                    RiskArrayQuote = this.RiskArrayQuote,
                    RiskArrayStrikePrice = this.RiskArrayStrikePrice,
                    DownSide5 = this.DownSide5,
                    DownSide4 = this.DownSide4,
                    DownSide3 = this.DownSide3,
                    DownSide2 = this.DownSide2,
                    DownSide1 = this.DownSide1,
                    UpSide1 = this.UpSide1,
                    UpSide2 = this.UpSide2,
                    UpSide3 = this.UpSide3,
                    UpSide4 = this.UpSide4,
                    UpSide5 = this.UpSide5,
                    ShortAdj = this.ShortAdj,
                    MaturityYearMonth = this.MaturityYearMonth,
                    Category = this.Category,
                    ContractSymbol = this.ContractSymbol,
                    // PM 20170222 [22881][22942] Ajout UnlIsinCode et IsActive
                    UnlIsinCode = this.UnlIsinCode,
                    IsActive = this.IsActive,
                    //MarketId = this.MarketId,
                    Type = this.Type,
                    StrikePrice = this.StrikePrice,
                    CrossMarginActivated = this.CrossMarginActivated,
                };
        }

        public RiskArrayParameterTimsIdem Clone(int pAssetId, int pContractID, string pParamCategory)
        {
            RiskArrayParameterTimsIdem clonedParameter = this.Clone(pAssetId, pContractID);

            clonedParameter.Category = pParamCategory;

            return clonedParameter;
        }

        /// <summary>
        /// Clone un objet RiskArrayParameterTimsIdem en changeant les valeurs de AssetId, ContractID, ContractSymbol et Category
        /// </summary>
        /// <param name="pAssetId"></param>
        /// <param name="pContractID"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pCategory"></param>
        /// <returns></returns>
        /// PM 20170222 [22881][22942] Ajout méthode
        public RiskArrayParameterTimsIdem Clone(int pAssetId, int pContractID, string pContractSymbol, string pCategory)
        {
            RiskArrayParameterTimsIdem clonedParameter = this.Clone(pAssetId, pContractID, pCategory);

            clonedParameter.ContractSymbol = pContractSymbol;

            return clonedParameter;
        }
    }

    /// <summary>
    /// Data container pour les paramètres par Derivative Contract
    /// <remarks>Main Spheres reference table: IMTIMSIDEMCONTRACT </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<TimsIdemContractParameter>.DATASETROWNAME,
        Namespace = DataHelper<TimsIdemContractParameter>.DATASETNAMESPACE)]
    internal sealed class TimsIdemContractParameter
    {
        /// <summary>
        /// Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Contract identifier
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string ContractIdentifier
        { get; set; }

        /// <summary>
        /// Contract displayname
        /// </summary>
        [DataMember(Name = "DISPLAYNAME", Order = 3)]
        public string ContractDisplayname
        { get; set; }

        /// <summary>
        /// Contract symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 4)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Market internal id
        /// </summary>
        [DataMember(Name = "IDM", Order = 5)]
        public int MarketId
        { get; set; }

        /// <summary>
        /// Classe séparée par échéance
        /// </summary>
        [DataMember(Name = "ISCLASSBYMATURITY", Order = 6)]
        public bool IsClassByMaturity
        { get; set; }

        /// <summary>
        /// DC représentant la Class de livraison
        /// </summary>
        [DataMember(Name = "IDDC_DELIVERY", Order = 7)]
        public int DeliveryContractId
        { get; set; }

        /// <summary>
        /// DC représentant la Class de livraison non couverte
        /// </summary>
        [DataMember(Name = "IDDC_UNCOVEREDDLV", Order = 8)]
        public int UncoveredDeliveryContractId
        { get; set; }

        /// <summary>
        /// DC représentant la Class de position à livrer
        /// </summary>
        [DataMember(Name = "IDDC_MATCHEDDLV", Order = 9)]
        public int MatchedDeliveryContractId
        { get; set; }

        /// <summary>
        /// DC représentant la Class de position à recevoir
        /// </summary>
        [DataMember(Name = "IDDC_MATCHEDWITHDRAW", Order = 10)]
        public int MatchedWithdrawContractId
        { get; set; }

        /// <summary>
        /// Period Multiplier Delivery Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODMLTPDLVMGROFFSET", Order = 11)]
        public int DeliveryMarginOffsetMultiplier
        { get; set; }

        /// <summary>
        /// Period Delivery Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODDLVMGROFFSET", Order = 12)]
        public string DeliveryMarginOffset
        { get; set; }
        public PeriodEnum DeliveryMarginOffsetEnum
        {
            get
            {
                return StringToEnum.Period(DeliveryMarginOffset);
            }
        }

        /// <summary>
        /// Day Type Delivery Margin Offset
        /// </summary>
        [DataMember(Name = "DAYTYPEDLVMGROFFSET", Order = 13)]
        public string DeliveryMarginOffsetDayType
        { get; set; }
        public DayTypeEnum DeliveryMarginOffsetDayTypeEnum
        {
            get
            {
                return StringToEnum.DayType(DeliveryMarginOffsetDayType);
            }
        }

        /// <summary>
        /// Period Multiplier Increase Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODMLTPINCRMGROFFSET", Order = 14)]
        public int IncreaseMarginOffsetMultiplier
        { get; set; }

        /// <summary>
        /// Period Increase Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODINCRMGROFFSET", Order = 15)]
        public string IncreaseMarginOffset
        { get; set; }
        public PeriodEnum IncreaseMarginOffsetEnum
        {
            get
            {
                return StringToEnum.Period(IncreaseMarginOffset);
            }
        }

        /// <summary>
        /// Day Type Increase Margin Offset
        /// </summary>
        [DataMember(Name = "DAYTYPEINCRMGROFFSET", Order = 16)]
        public string IncreaseyMarginOffsetDayType
        { get; set; }
        public DayTypeEnum IncreaseyMarginOffsetDayTypeEnum
        {
            get
            {
                return StringToEnum.DayType(IncreaseyMarginOffsetDayType);
            }
        }

        /// <summary>
        /// Period Multiplier Gross Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODMLTPGROSSMGROFFSET", Order = 17)]
        public int GrossyMarginOffsetMultiplier
        { get; set; }

        /// <summary>
        /// Period Gross Margin Offset
        /// </summary>
        [DataMember(Name = "PERIODGROSSMGROFFSET", Order = 18)]
        public string GrossMarginOffset
        { get; set; }
        public PeriodEnum GrossMarginOffsetEnum
        {
            get
            {
                return StringToEnum.Period(GrossMarginOffset);
            }
        }

        /// <summary>
        /// Day Type Gross Margin Offset
        /// </summary>
        [DataMember(Name = "DAYTYPEGROSSMGROFFSET", Order = 19)]
        public string GrossMarginOffsetDayType
        { get; set; }
        public DayTypeEnum GrossMarginOffsetDayTypeEnum
        {
            get
            {
                return StringToEnum.DayType(GrossMarginOffsetDayType);
            }
        }
    }

    /// <summary>
    /// Data container pour les convertions d'assets en livraison
    /// </summary>
    [DataContract(Name = DataHelper<TimsIdemAssetDeliveryParameter>.DATASETROWNAME,
        Namespace = DataHelper<TimsIdemAssetDeliveryParameter>.DATASETNAMESPACE)]
    internal sealed class TimsIdemAssetDeliveryParameter
    {
        /// <summary>
        /// Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 2)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Asset internal id converted
        /// </summary>
        [DataMember(Name = "IDASSET_NEW", Order = 3)]
        public int NewAssetId
        { get; set; }

        /// <summary>
        /// Contract internal id converted
        /// </summary>
        [DataMember(Name = "IDDC_NEW", Order = 4)]
        public int NewContractId
        { get; set; }

        /// <summary>
        /// Date de début d'application de la convertion
        /// </summary>
        [DataMember(Name = "APPLYSTARTDATE", Order = 5)]
        public DateTime ApplyStartDate
        { get; set; }

        /// <summary>
        /// Date de fin d'application de la convertion
        /// </summary>
        [DataMember(Name = "APPLYENDDATE", Order = 6)]
        public DateTime ApplyEndDate
        { get; set; }

        /// <summary>
        /// Etape dans la pré-livraison donnant lieu à l'application de la convertion
        /// </summary>
        [DataMember(Name = "DELIVERYSTEP", Order = 7)]
        public string DeliveryStep
        { get; set; }
    }
    #endregion TIMS IDEM method

    #region TIMS EUREX method

    /// <summary>
    /// Data container for the TIMS EUREX "contract" parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: PARAMSEUREX_CONTRACT</remarks>
    [DataContract(Name = DataHelper<ContractParameterTimsEurex>.DATASETROWNAME,
        Namespace = DataHelper<ContractParameterTimsEurex>.DATASETNAMESPACE)]
    internal sealed class ContractParameterTimsEurex : IDataContractEnabled
    {
        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Market contract symbol. third and last level of the EUREX product hierarchy
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 2)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Margin class. Second level of the EUREX product hierarchy. More derivative contracts could be part of a margin class.
        /// </summary>
        [DataMember(Name = "MARGINCLASS", Order = 3)]
        public string MarginClass
        { get; set; }


        /// <summary>
        /// Margin class. Second level of the EUREX product hierarchy. More derivative contracts could be part of a margin class.
        /// </summary>
        [DataMember(Name = "MARGINGROUP", Order = 4)]
        public string MarginGroup
        { get; set; }

        /// <summary>
        /// Category od the current derivative contract (IDDC), F for Futures, O for Options.
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 5)]
        public string Category
        { get; set; }

        /// <summary>
        /// Contract multiplier of the DERIVATIVECONTRACT table
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 6)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } }

        /// <summary>
        /// Category of the underlying asset
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag167.html</remarks>
        [DataMember(Name = "ASSETCATEGORY", Order = 7)]
        public Cst.UnderlyingAsset UnlCategory
        { get; set; }

        /// <summary>
        /// Underlying asset internal id (null when UnlCategory = Future)
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 8)]
        public int UnlAssetId
        { get; set; }

        /// <summary>
        /// Underlying Future contract id (not null only when UnlCategory = Future)
        /// </summary>
        [DataMember(Name = "IDDC_UNL", Order = 8)]
        public int UnlContractId
        { get; set; }

        /// <summary>
        /// Risk offset of the current class in comparison with the other group classes (expressed as a percentage from 0 to 1)
        /// </summary>
        [DataMember(Name = "OFFSET", Order = 9)]
        public double OffsetDouble
        { get; set; }
        public decimal Offset
        { get { return (decimal)OffsetDouble; } }

        /// <summary>
        /// Maturity switch activation status. 
        /// When activated (value: Y) the maturity factor enters the risk evaluation process for the current derivative contract.
        /// </summary>
        /// <value>Y -> Yes, N -> Not</value>
        [DataMember(Name = "MATURITY_SWITCH", Order = 10)]
        public MaturitySwitch MaturitySwitch
        { get; set; }

        /// <summary>
        /// amounts to be paid for futures spread positions of the spot month for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        [DataMember(Name = "SPOTMONTH_SPREADRATE", Order = 11)]
        public double SpotMonthSpreadRateDouble
        { get; set; }
        public decimal SpotMonthSpreadRate
        { get { return (decimal)SpotMonthSpreadRateDouble; } }

        /// <summary>
        /// amounts to be paid for futures spread positions of the back months for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        [DataMember(Name = "BACKMONTH_SPREADRATE", Order = 12)]
        public double BackMonthSpreadRateDouble
        { get; set; }
        public decimal BackMonthSpreadRate
        { get { return (decimal)BackMonthSpreadRateDouble; } }

        /// <summary>
        /// Minimal value for out of the money positions
        /// </summary>
        [DataMember(Name = "OOM_MINIMUMRATE", Order = 13)]
        public double OutOfTheMoneyMinValueDouble
        { get; set; }
        public decimal OutOfTheMoneyMinValue
        { get { return (decimal)OutOfTheMoneyMinValueDouble; } }

        /// <summary>
        /// Multiplication factor for contract arriving at maturity at the current business date's month
        /// </summary>
        [DataMember(Name = "EXPIRYMONTH_FACTOR", Order = 14)]
        public double ExpiryMonthFactorDouble
        { get; set; }
        public decimal ExpiryMonthFactor
        { get { return (decimal)ExpiryMonthFactorDouble; } }

        /// <summary>
        /// Margin style. Related to the evaluation method of the derivative contract.
        /// </summary>
        [DataMember(Name = "MARGIN_STYLE", Order = 15)]
        public MarginStyle MarginStyle
        { get; set; }

        /// <summary>
        /// Contract tick size. 
        /// <list type="">
        /// <listheader>It has been used during the risk parameters integration to compute the following values:</listheader>
        /// <item>
        /// the minimum price increment unit of the contract, Spheres column:  DERIVATIVECONTRACT.MINPRICEINCR
        /// </item>
        /// <item>
        /// the contract multiplier, Spheres column:  DERIVATIVECONTRACT.CONTRACTMULTIPLIER
        /// (StructuresStockageParametresEUREX.docx)
        /// </item>
        /// <item>
        /// the asset multiplier, Spheres column:  ASSET_ETD.CONTRACTMULTIPLIER
        /// (StructuresStockageParametresEUREX.docx)
        /// </item>
        /// </list>
        /// </summary>
        /// <remarks>not used when the contract multiplier is not null</remarks>
        [DataMember(Name = "TICKSIZE", Order = 16)]
        public double TickSizeDouble
        { get; set; }
        public decimal TickSize
        { get { return (decimal)TickSizeDouble; } }

        /// <summary>
        /// Contract minimum price increment amount
        /// <list type="">
        /// <listheader>It has been used during the risk parameters integration to compute the following values:</listheader>
        /// <item>
        /// the contract multiplier, Spheres column:  DERIVATIVECONTRACT.CONTRACTMULTIPLIER
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// <item>
        /// the asset multiplier, Spheres column:  ASSET_ETD.CONTRACTMULTIPLIER
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// <item>
        /// the minimum price increment value of the contract, Spheres column:  DERIVATIVECONTRACT.MINPRICEINCRAMOUNT
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// </list>
        /// </summary>
        /// <remarks>not used when the contract multiplier is not null</remarks>
        [DataMember(Name = "TICKVALUE", Order = 17)]
        public double TickValueDouble
        { get; set; }
        public decimal TickValue
        { get { return (decimal)TickValueDouble; } }

        /// <summary>
        /// Contract currency
        /// </summary>
        [DataMember(Name = "IDC_PRICE", Order = 18)]
        public string Currency
        { get; set; }

        /// <summary>
        /// identifiant du marché (EUREX - XEUR, EEX - XEEX, REPO - ?, BOND - ?, etc..) relate to the current contract symbol (ContractSymbol)
        /// </summary>
        [DataMember(Name = "ISO10383_ALPHA4", Order = 19)]
        public string MarketAcronym
        { get; set; }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Activation date of the contract
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 20)]
        public DateTime ElementEnabledFrom
        {
            get;
            set;
        }

        /// <summary>
        /// Deactivation date of the contract
        /// </summary>
        [DataMember(Name = "DTDISABLED", Order = 21)]
        public DateTime ElementDisabledFrom
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Maturity Rule Frequency
        /// </summary>
        [DataMember(Name = "MMYRULE", Order = 22)]
        public string MaturityRuleFrequency
        { get; set; }
    }

    /// <summary>
    /// Data container for the TIMS EUREX "maturity" parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: PARAMSEUREX_MATURITY</remarks>
    [DataContract(Name = DataHelper<MaturityParameterTimsEurex>.DATASETROWNAME,
        Namespace = DataHelper<MaturityParameterTimsEurex>.DATASETNAMESPACE)]
    internal sealed class MaturityParameterTimsEurex
    {

        /// <summary>
        /// maturity internal id
        /// </summary>
        [DataMember(Name = "MATURITYID", Order = 1)]
        public int MaturityId
        { get; set; }


        /// <summary>
        /// Market contract symbol. third and last level of the EUREX product hierarchy
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 2)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// PutCall info (null for dc futures)
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag201.html</remarks>
        [DataMember(Name = "PUTCALL", Order = 3)]
        public string PutOrCall
        { get; set; }

        /// <summary>
        /// Maturity code
        /// </summary>
        [DataMember(Name = "MATURITYYEARMONTH", Order = 4)]
        public double MaturityYearMonthDouble
        { get; set; }
        public decimal MaturityYearMonth
        { get { return (decimal)MaturityYearMonthDouble; } }

        /// <summary>
        /// Maturity factor
        /// </summary>
        [DataMember(Name = "MATURITY_FACTOR", Order = 5)]
        public double MaturityFactorDouble
        { get; set; }
        public decimal MaturityFactor
        { get { return (decimal)MaturityFactorDouble; } }

        /// <summary>
        /// Theoretical Security free interest rate 
        /// </summary>
        [DataMember(Name = "THEORETICAL_INTEREST_RATE", Order = 6)]
        public double InterestRateDouble
        { get; set; }
        public decimal InterestRate
        { get { return (decimal)InterestRateDouble; } }

        /// <summary>
        /// Theoretical Yield rate 
        /// </summary>
        [DataMember(Name = "THEORETICAL_YIELD_RATE", Order = 7)]
        public double YieldRateDouble
        { get; set; }
        public decimal YieldRate
        { get { return (decimal)YieldRateDouble; } }
    }

    /// <summary>
    /// Data container for the TIMS EUREX "asset" parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: PARAMSEUREX_ASSETETD</remarks>
    [DataContract(Name = DataHelper<AssetParameterTimsEurex>.DATASETROWNAME,
        Namespace = DataHelper<AssetParameterTimsEurex>.DATASETNAMESPACE)]
    internal sealed class AssetParameterTimsEurex
    {

        /// <summary>
        /// asset id (element identifier)
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// maturity id (to identify maturity rule, put/call indicator of the ETD asset)
        /// </summary>
        [DataMember(Name = "MATURITYID", Order = 2)]
        public int MaturityId
        { get; set; }

        /// <summary>
        /// Trade unit, timed the tick value it is the contract minimum price increment amount
        /// <list type="">
        /// <listheader>It has been used during the risk parameters integration to compute the following values:</listheader>
        /// <item>
        /// the contract multiplier, Spheres column:  DERIVATIVECONTRACT.CONTRACTMULTIPLIER
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// <item>
        /// the asset multiplier, Spheres column:  ASSET_ETD.CONTRACTMULTIPLIER
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// <item>
        /// the minimum price increment amount of the contract, Spheres column:  DERIVATIVECONTRACT.MINPRICEINCRAMOUNT
        /// (using the formule at StructuresStockageParametresEUREX.docx)
        /// </item>
        /// </list>
        /// </summary>
        /// 
        [DataMember(Name = "TRADE_UNIT", Order = 3)]
        public double TradeUnitDouble
        { get; set; }
        public decimal TradeUnit
        { get { return (decimal)TradeUnitDouble; } }

        /// <summary>
        /// Closing quote parameter of the asset at the current business date.
        /// </summary>
        /// <remarks>same value as as the quote in the Spheres QUOTE_ETD table</remarks>
        [DataMember(Name = "VALUE_QUOTE_ASSETETD", Order = 4)]
        public double AssetQuoteParameterDouble
        { get; set; }
        public decimal AssetQuoteParameter
        { get { return (decimal)AssetQuoteParameterDouble; } }

        /// <summary>
        /// Closing quote parameter of the underlying asset at the current business date.
        /// </summary>
        /// <remarks>same value as as the quote in the Spheres QUOTE_XXX tables</remarks>
        [DataMember(Name = "VALUE_QUOTE_UNL", Order = 5)]
        public double UnlAssetQuoteParameterDouble
        { get; set; }
        public decimal UnlAssetQuoteParameter
        { get { return (decimal)UnlAssetQuoteParameterDouble; } }

        /// <summary>
        /// Exercice price pour un contrat Option (0 for Futures)
        /// </summary>
        /// <remarks></remarks>
        public decimal Strike
        { get; set; }

        /// <summary>
        /// Asset multiplier (to set externally, compliant data source: VW_ASSET_ETD_EXPANDED)
        /// </summary>
        /// <remarks></remarks>
        public decimal Multiplier
        { get; set; }

        /// <summary>
        /// Underlying asset internal id (null when UnlCategory = Future). (to set externally, compliant data source: VW_ASSET_ETD_EXPANDED)
        /// </summary>
        public int UnlAssetId
        { get; set; }

        /// <summary>
        /// Derivative Contract id (internal identifier)
        /// </summary>
        [DataMember(Name = "IDDC", Order = 7)]
        public int ContractId
        { get; set; }
    }

    /// <summary>
    /// Data container for the TIMS EUREX "volatility" parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: PARAMSEUREX_VOLATILITY</remarks>
    [DataContract(Name = DataHelper<VolatilityParameterTimsEurex>.DATASETROWNAME,
        Namespace = DataHelper<VolatilityParameterTimsEurex>.DATASETNAMESPACE)]
    internal sealed class VolatilityParameterTimsEurex
    {
        /// <summary>
        /// asset id 
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// maturity id 
        /// </summary>
        [DataMember(Name = "MATURITYID", Order = 2)]
        public int MaturityId
        { get; set; }

        /// <summary>
        /// Index of the array/scenario 
        /// </summary>
        /// <value>from 1 to max 29</value>
        [DataMember(Name = "RISKARRAY_INDEX", Order = 3)]
        public int RiskArrayIndex
        { get; set; }

        /// <summary>
        /// Indicator whether the projected underlying price is lesser, equals or greater than the closing price
        /// </summary>
        /// <value>D -> downside ; N -> neutral ; U -> Upside</value>
        [DataMember(Name = "QUOTE_ETD_UNL_COMPARE", Order = 4)]
        public string QuoteUnlVsQuote_Indicator
        { get; set; }

        /// <summary>
        /// volatility when the projected underlying price is greater than the closing price
        /// </summary>
        [DataMember(Name = "UPVOLATILITY", Order = 5)]
        public double UpVolatility
        { get; set; }

        /// <summary>
        /// Risk value/ Margin parameter when the projected underlying price is greater than the closing price
        /// </summary>
        [DataMember(Name = "UPTHEORETICAL_VALUE", Order = 6)]
        public double UpTheoreticalValue
        { get; set; }

        /// <summary>
        /// Short option adjustement when the projected underlying price is greater than the closing price
        /// </summary>
        [DataMember(Name = "UPSHORTOPTADJUSTMENT", Order = 7)]
        public double UpShortAdj
        { get; set; }

        /// <summary>
        /// volatility when the projected underlying price is equals than the closing price
        /// </summary>
        [DataMember(Name = "NTRLVOLATILITY", Order = 8)]
        public double NtrlVolatility
        { get; set; }

        /// <summary>
        /// Risk value/ Margin parameter when the projected underlying price is equals than the closing price
        /// </summary>
        [DataMember(Name = "NTRLTHEORETICAL_VALUE", Order = 9)]
        public double NtrlTheoreticalValue
        { get; set; }

        /// <summary>
        /// Short option adjustement when the projected underlying price is equals than the closing price
        /// </summary>
        [DataMember(Name = "NTRLSHORTOPTADJUSTMENT", Order = 10)]
        public double NtrlShortAdj
        { get; set; }

        /// <summary>
        /// volatility when the projected underlying price is lesser than the closing price
        /// </summary>
        [DataMember(Name = "DOWNVOLATILITY", Order = 11)]
        public double DownVolatility
        { get; set; }

        /// <summary>
        /// Risk value/ Margin parameter when the projected underlying price is lesser than the closing price
        /// </summary>
        [DataMember(Name = "DOWNTHEORETICAL_VALUE", Order = 12)]
        public double DownTheoreticalValue
        { get; set; }

        /// <summary>
        /// Short option adjustement when the projected underlying price is lesser than the closing price
        /// </summary>
        [DataMember(Name = "DOWNSHORTOPTADJUSTMENT", Order = 13)]
        public double DownShortAdj
        { get; set; }

        /// <summary>
        /// Indicator for the projected underlying price 
        /// </summary>
        /// <value>0 -> pure volatility bucket ; 1 -> zero price movement ; 2 ->  underlying price ; 3 ->  in-between strike </value>
        /// <remarks>this value is used just when the additional margin is evaluated, the scenarios 0/2 and 1/3 are different just because 
        /// we have to use for the 0/2 the short option adjustement value instead of the standard risk margin value according to some rules</remarks>
        [DataMember(Name = "QUOTE_UNL_INDICATOR", Order = 14)]
        public int QuoteUnlIndicator
        { get; set; }

        /// <summary>
        /// Delta Underlying price/Strike 
        /// </summary>
        /// <value>: « Quote underlying » - « asset strike price »</value>
        [DataMember(Name = "RISKVALUE_EXEASS", Order = 15)]
        public double RiskValueExeAssDouble
        { get; set; }
        public decimal RiskValueExeAss
        { get { return (decimal)RiskValueExeAssDouble; } }

        /// <summary>
        /// Price Up Risk value/ Margin parameter exists 
        /// </summary>
        /// <value>false when does not exist</value>
        [DataMember(Name = "UPTHEORETICAL_EXISTS", Order = 16)]
        public bool UpTheoreticalExists
        { get; set; }

        /// <summary>
        /// Price Neutral Risk value/ Margin parameter exists 
        /// </summary>
        /// <value>false when does not exist</value>
        [DataMember(Name = "NTRLTHEORETICAL_EXISTS", Order = 17)]
        public bool NtrlTheoreticalExists
        { get; set; }

        /// <summary>
        /// Price Down Risk value/ Margin parameter exists 
        /// </summary>
        /// <value>false when does not exist</value>
        [DataMember(Name = "DOWNTHEORETICAL_EXISTS", Order = 18)]
        public bool DownTheoreticalExists
        { get; set; }
    }


    /// <summary>
    /// Data container for the TIMS EUREX "fx rate" parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: ASSET_FXRATE / QUOTE_FXRATE_H</remarks>
    [DataContract(Name = DataHelper<FxRateTimsEurex>.DATASETROWNAME,
        Namespace = DataHelper<FxRateTimsEurex>.DATASETNAMESPACE)]
    internal sealed class FxRateTimsEurex : IDataContractEnabled
    {
        /// <summary>
        /// conversion currency 
        /// </summary>
        [DataMember(Name = "IDC_TO", Order = 1)]
        public string CurrencyTo
        { get; set; }

        /// <summary>
        /// starting currency of the deposit 
        /// </summary>
        [DataMember(Name = "IDC_FROM", Order = 2)]
        public string CurrencyFrom
        { get; set; }

        /// <summary>
        /// conversion rate
        /// </summary>
        [DataMember(Name = "VALUE", Order = 5)]
        public double RateValueDouble
        { get; set; }
        public decimal RateValue
        { get { return (decimal)RateValueDouble; } }

        /// <summary>
        /// conversion side (debit or credit)
        /// </summary>
        [DataMember(Name = "QUOTESIDE", Order = 6)]
        public ExchangeRateSide RateSide
        { get; set; }

        /// <summary>
        /// Market environement
        /// </summary>
        [DataMember(Name = "IDMARKETENV", Order = 7)]
        public string IdMarketEnv
        { get; set; }

        /// <summary>
        /// Evaluation scenario
        /// </summary>
        [DataMember(Name = "IDVALSCENARIO", Order = 8)]
        public string IdValScenario
        { get; set; }

        /// <summary>
        /// Activation indicator
        /// </summary>
        [DataMember(Name = "ISENABLED", Order = 9)]
        public bool IsEnabled
        { get; set; }

        #region IDataContractEnabled Membres

        /// <summary>
        /// Starting parameter validity date
        /// </summary>
        [DataMember(Name = "DTENABLED", Order = 3)]
        public DateTime ElementEnabledFrom
        { get; set; }

        [DataMember(Name = "DTDISABLED", Order = 4)]
        public DateTime ElementDisabledFrom
        { get; set; }

        #endregion
    }

    #endregion TIMS EUREX method
}