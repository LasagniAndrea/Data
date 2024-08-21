using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EfsML.Enum;
using FixML.Enum;
using FixML.v50SP1.Enum;
using System;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Data container pour les paramètres par Derivative Contract
    /// <remarks>Main Spheres reference table: IMCBOECONTRACT, IMCBOEMARKET </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<CboeContractParameter>.DATASETROWNAME,
        Namespace = DataHelper<CboeContractParameter>.DATASETNAMESPACE)]
    internal sealed class CboeContractParameter
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
        /// Pourcentage de la valeur de l'option
        /// </summary>
        [DataMember(Name = "PCTOPTVALUE", Order = 5)]
        public double PctOptionValueDouble
        { get; set; }
        public decimal PctOptionValue
        { get { return (decimal)PctOptionValueDouble; } }

        /// <summary>
        /// Pourcentage de la valeur du sous-jacent
        /// </summary>
        [DataMember(Name = "PCTUNLVALUE", Order = 6)]
        public double PctUnderlyingValueDouble
        { get; set; }
        public decimal PctUnderlyingValue
        { get { return (decimal)PctUnderlyingValueDouble; } }

        /// <summary>
        /// Pourcentage minimum de la valeur du sous-jacent
        /// </summary>
        [DataMember(Name = "PCTMINVALUE", Order = 7)]
        public double PctMinimumUnderlyingValueDouble
        { get; set; }
        public decimal PctMinimumUnderlyingValue
        { get { return (decimal)PctMinimumUnderlyingValueDouble; } }
    }

    /// <summary>
    /// Data container pour les informations sur chaque Asset 
    /// </summary>
    /// <remarks>Main Spheres reference table: ASSET_ETD, DERIVATIVEATTRIB, DERIVATIVECONTRACT, MATURITY</remarks>
    [DataContract(Name = DataHelper<CboeAssetExpandedParameter>.DATASETROWNAME,
        Namespace = DataHelper<CboeAssetExpandedParameter>.DATASETNAMESPACE)]
    internal sealed class CboeAssetExpandedParameter
    {
        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 1)]
        public int ContractId
        { get; set; }

        /// <summary>
        /// Category od the current derivative contract (IDDC), F for Futures, O for Options.
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 2)]
        public string Category
        { get; set; }

        public Nullable<CfiCodeCategoryEnum> CategoryEnum
        {
            get
            {
                Nullable<CfiCodeCategoryEnum> retCategeory = null;
                if (StrFunc.IsFilled(Category))
                    retCategeory = (CfiCodeCategoryEnum)ReflectionTools.EnumParse(new CfiCodeCategoryEnum(), Category);
                return retCategeory;
            }
        }

        /// <summary>
        /// Contract currency
        /// </summary>
        [DataMember(Name = "IDC_PRICE", Order = 3)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Valuation Method
        /// </summary>
        [DataMember(Name = "FUTVALUATIONMETHOD", Order = 4)]
        public string ValuationMethod
        { get; set; }

        public FuturesValuationMethodEnum ValuationMethodEnum
        {
            get
            {
                //object ret = StringToEnum.GetEnumValue(new FuturesValuationMethodEnum(), ValuationMethod);
                //return (ret != null ? (FuturesValuationMethodEnum)ret : FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
                return ReflectionTools.ConvertStringToEnumOrDefault<FuturesValuationMethodEnum>(ValuationMethod, FuturesValuationMethodEnum.FuturesStyleMarkToMarket);
            }
        }

        /// <summary>
        /// Contract price numerator
        /// </summary>
        [DataMember(Name = "INSTRUMENTNUM", Order = 5)]
        public double InstrumentNumDec
        { get; set; }
        public int InstrumentNum
        {
            get
            {
                return (int)InstrumentNumDec;
            }
        }

        /// <summary>
        /// Contract price denominator
        /// </summary>
        [DataMember(Name = "INSTRUMENTDEN", Order = 6)]
        public int InstrumentDen
        { get; set; }

        /// <summary>
        /// Underlying Asset Category
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 7)]
        public string UnderlyningAssetCategory
        { get; set; }

        public Cst.UnderlyingAsset UnderlyingAssetCategoryEnum
        {
            get
            {
                return Cst.ConvertToUnderlyingAsset(UnderlyningAssetCategory);
            }
        }

        /// <summary>
        /// Underlying Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC_UNL", Order = 8)]
        public int UnderlyningContractId
        { get; set; }

        /// <summary>
        /// Underlying Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 9)]
        public int UnderlyningAssetId
        { get; set; }

        /// <summary>
        /// Asset internal id
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 10)]
        public int AssetId
        { get; set; }

        /// <summary>
        /// Put / Call indicator
        /// </summary>
        [DataMember(Name = "PUTCALL", Order = 11)]
        public string PutCall
        { get; set; }

        public Nullable<PutOrCallEnum> PutOrCall
        {
            get
            {
                Nullable<PutOrCallEnum> retPutOrCall = null;
                if (StrFunc.IsFilled(PutCall))
                    retPutOrCall = (PutOrCallEnum)ReflectionTools.EnumParse(new PutOrCallEnum(), PutCall);
                return retPutOrCall;
            }
        }

        /// <summary>
        /// Strike Price
        /// </summary>
        [DataMember(Name = "STRIKEPRICE", Order = 12)]
        public double StrikePriceDouble
        { get; set; }
        public decimal StrikePrice
        { get { return (decimal)StrikePriceDouble; } }

        /// <summary>
        /// Maturity
        /// </summary>
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 13)]
        public string MaturityYearMonth
        { get; set; }

        /// <summary>
        /// Maturity Date
        /// </summary>
        // PM 20191025 [24983] Ajout MATURITYDATE
        [DataMember(Name = "MATURITYDATE", Order = 14)]
        public DateTime MaturityDate
        { get; set; }
        
        /// <summary>
        /// Contract multiplier
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 15)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } }

        /// <summary>
        /// Underlying Identifier
        /// </summary>
        [DataMember(Name = "IDENTIFIER_UNL", Order = 16)]
        public string UnderlyingIdentifier
        { get; set; }
    }
}

