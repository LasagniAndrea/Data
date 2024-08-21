using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using FixML.Enum;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Data container for the MEFFCOM2 Asset parameters
    /// <remarks>Main Spheres reference table: IMMEFFCONTRACT_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<MeffContractAsset>.DATASETROWNAME,
        Namespace = DataHelper<MeffContractAsset>.DATASETNAMESPACE)]
    internal sealed class MeffContractAsset
    {
        /// <summary>
        /// Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset
        { get; set; }

        /// <summary>
        /// Asset Category
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 2)]
        public string AssetCategory
        { get; set; }

        /// <summary>
        /// Underlying Asset Category
        /// </summary>
        public Cst.UnderlyingAsset AssetCategoryEnum
        {
            get
            {
                Cst.UnderlyingAsset retCategory = Cst.UnderlyingAsset.Future;
                if (StrFunc.IsFilled(AssetCategory))
                {
                    retCategory = (Cst.UnderlyingAsset)ReflectionTools.EnumParse(new Cst.UnderlyingAsset(), AssetCategory);
                }
                return retCategory;
            }
        }

        /// <summary>
        /// Asset Code
        /// </summary>
        [DataMember(Name = "ASSETCODE", Order = 3)]
        public string AssetCode
        { get; set; }

        /// <summary>
        /// Valuation Array Code
        /// </summary>
        [DataMember(Name = "ARRAYCODE", Order = 4)]
        public string ArrayCode
        { get; set; }

        /// <summary>
        /// Sub Group Code
        /// </summary>
        [DataMember(Name = "SUBGROUPCODE", Order = 5)]
        public string SubGroupCode
        { get; set; }

        /// <summary>
        /// Margin underliyng asset code
        /// </summary>
        [DataMember(Name = "MGRUNLASSETCODE", Order = 6)]
        public string MgrUnlAssetCode
        { get; set; }

        /// <summary>
        /// Maturity Date
        /// </summary>
        [DataMember(Name = "MATURITYDATE", Order = 7)]
        public DateTime MaturityDate
        { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        [DataMember(Name = "PRICE", Order = 8)]
        public double PriceDouble
        { get; set; }
        public decimal Price
        { get { return (decimal)PriceDouble; } }
    }

    /// <summary>
    /// Data container for the MEFFCOM2 Valuation Array parameters
    /// <remarks>Main Spheres reference table: IMMEFFVALARRAY_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<MeffValuationArray>.DATASETROWNAME,
        Namespace = DataHelper<MeffValuationArray>.DATASETNAMESPACE)]
    internal sealed class MeffValuationArray
    {
        /// <summary>
        /// Valuation Array Code
        /// </summary>
        [DataMember(Name = "ARRAYCODE", Order = 1)]
        public string ArrayCode
        { get; set; }

        /// <summary>
        /// Expiry Span Type
        /// </summary>
        [DataMember(Name = "EXPIRYSPAN", Order = 2)]
        public string ExpirySpan
        { get; set; }

        /// <summary>
        /// Number of Values
        /// </summary>
        [DataMember(Name = "NBVALUE", Order = 3)]
        public int NbValue
        { get; set; }

        /// <summary>
        /// Price Fluctuation Type
        /// </summary>
        [DataMember(Name = "PRICEFLUCTTYPE", Order = 4)]
        public string PriceFluctuationType
        { get; set; }

        /// <summary>
        /// Price Increase Fluctuation
        /// </summary>
        [DataMember(Name = "PRICEINCFLUCT", Order = 5)]
        public double PriceIncreaseFluctuationDouble
        { get; set; }
        public decimal PriceIncreaseFluctuation
        { get { return (decimal)PriceIncreaseFluctuationDouble; } }

        /// <summary>
        /// Price Decrease Fluctuation
        /// </summary>
        [DataMember(Name = "PRICEDECFLUCT", Order = 6)]
        public double PriceDecreaseFluctuationDouble
        { get; set; }
        public decimal PriceDecreaseFluctuation
        { get { return (decimal)PriceDecreaseFluctuationDouble; } }

        /// <summary>
        /// Volatility Variation Type
        /// </summary>
        [DataMember(Name = "VOLATVARIATIONTYPE", Order = 7)]
        public string VolatilityVariationType
        { get; set; }

        /// <summary>
        /// Volatility Variation
        /// </summary>
        [DataMember(Name = "VOLATVARIATION", Order = 8)]
        public double VolatilityVariationDouble
        { get; set; }
        public decimal VolatilityVariation
        { get { return (decimal)VolatilityVariationDouble; } }

        /// <summary>
        /// Sub Group Code
        /// </summary>
        [DataMember(Name = "SUBGROUPCODE", Order = 9)]
        public string SubGroupCode
        { get; set; }

        /// <summary>
        /// Contract Type Code
        /// </summary>
        [DataMember(Name = "CONTRACTTYPECODE", Order = 10)]
        public double ContractTypeCodeDouble
        { get; set; }
        public decimal ContractTypeCode
        { get { return (decimal)ContractTypeCodeDouble; } }

        /// <summary>
        /// Large Position Threshold
        /// </summary>
        [DataMember(Name = "LARGEPOSTHRESHOLD", Order = 11)]
        public double LargePositionThresholdDouble
        { get; set; }
        public decimal LargePositionThreshold
        { get { return (decimal)LargePositionThresholdDouble; } }
    }

    /// <summary>
    /// Data container for the MEFFCOM2 Inter Spread parameters
    /// <remarks>Main Spheres reference table: IMMEFFINTERSPREAD </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<MeffValuationArray>.DATASETROWNAME,
        Namespace = DataHelper<MeffValuationArray>.DATASETNAMESPACE)]
    internal sealed class MeffInterSpread
    {
        /// <summary>
        /// Priority
        /// </summary>
        [DataMember(Name = "PRIORITY", Order = 1)]
        public string Priority
        { get; set; }
        
        /// <summary>
        /// Valuation Array Code 1
        /// </summary>
        [DataMember(Name = "ARRAYCODE1", Order = 2)]
        public string ArrayCode1
        { get; set; }

        /// <summary>
        /// Offset Discount 1
        /// </summary>
        [DataMember(Name = "OFFSETDISCOUNT1", Order = 3)]
        public double OffsetDiscount1Double
        { get; set; }
        public decimal OffsetDiscount1
        { get { return (decimal)OffsetDiscount1Double; } }

        /// <summary>
        /// Offset Multiplier 1
        /// </summary>
        [DataMember(Name = "OFFSETMULTIPLIER1", Order = 4)]
        public int OffsetMultiplier1
        { get; set; }

        /// <summary>
        /// Valuation Array Code 2
        /// </summary>
        [DataMember(Name = "ARRAYCODE2", Order = 5)]
        public string ArrayCode2
        { get; set; }

        /// <summary>
        /// Offset Discount 2
        /// </summary>
        [DataMember(Name = "OFFSETDISCOUNT2", Order = 6)]
        public double OffsetDiscount2Double
        { get; set; }
        public decimal OffsetDiscount2
        { get { return (decimal)OffsetDiscount2Double; } }

        /// <summary>
        /// Offset Multiplier 1
        /// </summary>
        [DataMember(Name = "OFFSETMULTIPLIER2", Order = 7)]
        public int OffsetMultiplier2
        { get; set; }

        /// <summary>
        /// Discount Type
        /// </summary>
        [DataMember(Name = "DISCOUNTTYPE", Order = 8)]
        public string DiscountType
        { get; set; }
    }

    /// <summary>
    /// Data container for the MEFFCOM2 Intra Spread parameters
    /// <remarks>Main Spheres reference table: IMMEFFINTRASPREAD </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<MeffValuationArray>.DATASETROWNAME,
        Namespace = DataHelper<MeffValuationArray>.DATASETNAMESPACE)]
    internal sealed class MeffIntraSpread
    {
        /// <summary>
        /// Valuation Array Code
        /// </summary>
        [DataMember(Name = "ARRAYCODE", Order = 1)]
        public string ArrayCode
        { get; set; }

        /// <summary>
        /// Factor
        /// </summary>
        [DataMember(Name = "FACTOR", Order = 2)]
        public double FactorDouble
        { get; set; }
        public decimal Factor
        { get { return (decimal)FactorDouble; } }

        /// <summary>
        /// Minimum Value
        /// </summary>
        [DataMember(Name = "MINIMUMVALUE", Order = 3)]
        public double MinimumValueDouble
        { get; set; }
        public decimal MinimumValue
        { get { return (decimal)MinimumValueDouble; } }

        /// <summary>
        /// Spread
        /// </summary>
        [DataMember(Name = "SPREAD", Order = 4)]
        public double SpreadDouble
        { get; set; }
        public decimal Spread
        { get { return (decimal)SpreadDouble; } }

        /// <summary>
        /// Day Calc
        /// </summary>
        [DataMember(Name = "DAYCALC", Order = 5)]
        public string DayCalc
        { get; set; }
    }

    /// <summary>
    /// Data container for the MEFFCOM2 Risk Array parameters
    /// <remarks>Main Spheres reference table: IMMEFFTHEORPRICE_H or IMMEFFDELTA_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<MeffRiskArray>.DATASETROWNAME,
        Namespace = DataHelper<MeffRiskArray>.DATASETNAMESPACE)]
    internal sealed class MeffRiskArray
    {
        /// <summary>
        /// Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset
        { get; set; }

        /// <summary>
        /// Side
        /// </summary>
        [DataMember(Name = "SIDE", Order = 2)]
        public string Side
        { get; set; }

        public Nullable<SideEnum> SideEnum
        {
            get
            {
                Nullable<SideEnum> retSide = null;
                if (StrFunc.IsFilled(Side))
                    retSide = (SideEnum)ReflectionTools.EnumParse(new SideEnum(), Side);
                return retSide;
            }
        }

        /// <summary>
        /// Number of Values
        /// </summary>
        [DataMember(Name = "NBVALUE", Order = 3)]
        public int NbValue
        { get; set; }

        /// <summary>
        /// Risk Value 1
        /// </summary>
        [DataMember(Name = "RISKVALUE1", Order = 4)]
        public double RiskValue1Double
        { get; set; }
        public decimal RiskValue1
        { get { return (decimal)RiskValue1Double; } }

        /// <summary>
        /// Risk Value 2
        /// </summary>
        [DataMember(Name = "RISKVALUE2", Order = 5)]
        public double RiskValue2Double
        { get; set; }
        public decimal RiskValue2
        { get { return (decimal)RiskValue2Double; } }

        /// <summary>
        /// Risk Value 3
        /// </summary>
        [DataMember(Name = "RISKVALUE3", Order = 6)]
        public double RiskValue3Double
        { get; set; }
        public decimal RiskValue3
        { get { return (decimal)RiskValue3Double; } }

        /// <summary>
        /// Risk Value 4
        /// </summary>
        [DataMember(Name = "RISKVALUE4", Order = 7)]
        public double RiskValue4Double
        { get; set; }
        public decimal RiskValue4
        { get { return (decimal)RiskValue4Double; } }

        /// <summary>
        /// Risk Value 5
        /// </summary>
        [DataMember(Name = "RISKVALUE5", Order = 8)]
        public double RiskValue5Double
        { get; set; }
        public decimal RiskValue5
        { get { return (decimal)RiskValue5Double; } }

        /// <summary>
        /// Risk Value 6
        /// </summary>
        [DataMember(Name = "RISKVALUE6", Order = 9)]
        public double RiskValue6Double
        { get; set; }
        public decimal RiskValue6
        { get { return (decimal)RiskValue6Double; } }

        /// <summary>
        /// Risk Value 7
        /// </summary>
        [DataMember(Name = "RISKVALUE7", Order = 10)]
        public double RiskValue7Double
        { get; set; }
        public decimal RiskValue7
        { get { return (decimal)RiskValue7Double; } }

        /// <summary>
        /// Risk Value 8
        /// </summary>
        [DataMember(Name = "RISKVALUE8", Order = 11)]
        public double RiskValue8Double
        { get; set; }
        public decimal RiskValue8
        { get { return (decimal)RiskValue8Double; } }

        /// <summary>
        /// Risk Value 9
        /// </summary>
        [DataMember(Name = "RISKVALUE9", Order = 12)]
        public double RiskValue9Double
        { get; set; }
        public decimal RiskValue9
        { get { return (decimal)RiskValue9Double; } }

        /// <summary>
        /// Risk Value 10
        /// </summary>
        [DataMember(Name = "RISKVALUE10", Order = 13)]
        public double RiskValue10Double
        { get; set; }
        public decimal RiskValue10
        { get { return (decimal)RiskValue10Double; } }

        /// <summary>
        /// Risk Value 11
        /// </summary>
        [DataMember(Name = "RISKVALUE11", Order = 14)]
        public double RiskValue11Double
        { get; set; }
        public decimal RiskValue11
        { get { return (decimal)RiskValue11Double; } }

        /// <summary>
        /// Risk Value 12
        /// </summary>
        [DataMember(Name = "RISKVALUE12", Order = 15)]
        public double RiskValue12Double
        { get; set; }
        public decimal RiskValue12
        { get { return (decimal)RiskValue12Double; } }

        /// <summary>
        /// Risk Value 13
        /// </summary>
        [DataMember(Name = "RISKVALUE13", Order = 16)]
        public double RiskValue13Double
        { get; set; }
        public decimal RiskValue13
        { get { return (decimal)RiskValue13Double; } }

        /// <summary>
        /// Risk Value 14
        /// </summary>
        [DataMember(Name = "RISKVALUE14", Order = 17)]
        public double RiskValue14Double
        { get; set; }
        public decimal RiskValue14
        { get { return (decimal)RiskValue14Double; } }

        /// <summary>
        /// Risk Value 15
        /// </summary>
        [DataMember(Name = "RISKVALUE15", Order = 18)]
        public double RiskValue15Double
        { get; set; }
        public decimal RiskValue15
        { get { return (decimal)RiskValue15Double; } }

        /// <summary>
        /// Permet d'obtenir les valeurs de risque sous la forme d'un tableau de decimal
        /// </summary>
        /// <returns>Un tableau de valeur decimal</returns>
        public decimal[] ToArray()
        {
            decimal[] decimalArray = new decimal[15];

            decimalArray[0] = RiskValue1;
            decimalArray[1] = RiskValue2;
            decimalArray[2] = RiskValue3;
            decimalArray[3] = RiskValue4;
            decimalArray[4] = RiskValue5;
            decimalArray[5] = RiskValue6;
            decimalArray[6] = RiskValue7;
            decimalArray[7] = RiskValue8;
            decimalArray[8] = RiskValue9;
            decimalArray[9] = RiskValue10;
            decimalArray[10] = RiskValue11;
            decimalArray[11] = RiskValue12;
            decimalArray[12] = RiskValue13;
            decimalArray[13] = RiskValue14;
            decimalArray[14] = RiskValue15;

            return decimalArray;
        }

        /// <summary>
        /// Permet d'obtenir les valeurs de risque sous la forme d'un dictionnaire
        /// </summary>
        /// <returns>Un dictionnaire de valeur de risque</returns>
        public Dictionary<int,decimal> ToDictionary()
        {
            Dictionary<int, decimal> riskValue = new Dictionary<int, decimal>
            {
                { 1, RiskValue1 },
                { 2, RiskValue2 },
                { 3, RiskValue3 },
                { 4, RiskValue4 },
                { 5, RiskValue5 },
                { 6, RiskValue6 },
                { 7, RiskValue7 },
                { 8, RiskValue8 },
                { 9, RiskValue9 },
                { 10, RiskValue10 },
                { 11, RiskValue11 },
                { 12, RiskValue12 },
                { 13, RiskValue13 },
                { 14, RiskValue14 },
                { 15, RiskValue15 }
            };

            return riskValue;
        }
    }
}
