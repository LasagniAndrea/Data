using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.IO;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
//
using FixML.Enum;
using FixML.v50SP1.Enum;

namespace EFS.SpheresRiskPerformance
{
    #region Class de stockage des données issues des fichiers
    /// <summary>
    /// Class représentant les données d'un Liquidation Group
    /// </summary>
    public sealed class RiskDataPrismaLiquidationGroup
    {
        #region Members
        private readonly string m_LiquidationGroup;
        #region Risk Measure
        private PrismaCurrencyTypeFlagEnum m_CurrencyTypeFlag;
        #endregion Risk Measure
        #endregion Members
        #region Accessors
        /// <summary>
        /// Liquidation Group
        /// </summary>
        public string LiquidationGroup { get { return m_LiquidationGroup; } }
        #region Risk Measure
        /// <summary>
        /// Currency Type Flag
        /// </summary>
        public PrismaCurrencyTypeFlagEnum CurrencyTypeFlag { get { return m_CurrencyTypeFlag; } }
        #endregion Risk Measure
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group</param>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group</param>
        public RiskDataPrismaLiquidationGroup(string pLiquidationGroup, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            m_LiquidationGroup = pLiquidationGroup;
            SetData(pInputSourceDataStyle, pParsingRow);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Ajout de données au Liquidation Group
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group</param>
        public void SetData(Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            if (pInputSourceDataStyle == Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE)
            {
                m_CurrencyTypeFlag = ReflectionTools.ConvertStringToEnumOrDefault<PrismaCurrencyTypeFlagEnum>(RiskDataLoad.GetRowDataValue(pParsingRow, "Currency Type Flag"), PrismaCurrencyTypeFlagEnum.NA);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les données d'un Liquidation Group Split
    /// </summary>
    public sealed class RiskDataPrismaLiquidationGroupSplit
    {
        #region Members
        private readonly string m_LiquidationGroup;
        private readonly string m_LiquidationGroupSplit;
        #region Risk Measure Aggregation Configuration
        private string m_RiskMethod;
        private PrismaAggregationMethod m_AggregationMethod;
        #endregion Risk Measure Aggregation Configuration
        #region Risk Measure Configuration
        // PM 20180903 [24015] Prisma v8.0 : add m_IsCalculateTEA
        private bool m_IsCalculateTEA;
        #endregion Risk Measure Configuration
        #endregion Members
        #region Accessors
        /// <summary>
        /// Liquidation Group
        /// </summary>
        public string LiquidationGroup { get { return m_LiquidationGroup; } }
        /// <summary>
        /// Liquidation Group Split
        /// </summary>
        public string LiquidationGroupSplit { get { return m_LiquidationGroupSplit; } }
        #region Risk Measure Aggregation Configuration
        /// <summary>
        /// Risk Method
        /// </summary>
        public string RiskMethod { get { return m_RiskMethod; } }
        /// <summary>
        /// Aggregation Method
        /// </summary>
        public PrismaAggregationMethod AggregationMethod { get { return m_AggregationMethod; } }
        #endregion Risk Measure Aggregation Configuration
        #region Risk Measure Configuration
        // PM 20180903 [24015] Prisma v8.0 : add IsCalculateTEA
        /// <summary>
        /// Calculate TEA or not
        /// </summary>
        public bool IsCalculateTEA { get { return m_IsCalculateTEA; } }
        #endregion Risk Measure Configuration
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group Split</param>
        public RiskDataPrismaLiquidationGroupSplit(string pLiquidationGroup, string pLiquidationGroupSplit, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            m_LiquidationGroup = pLiquidationGroup;
            m_LiquidationGroupSplit = pLiquidationGroupSplit;
            SetData(pInputSourceDataStyle, pParsingRow);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Ajout de données au Liquidation Group Split
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group Split</param>
        public void SetData(Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            switch (pInputSourceDataStyle)
            {
                #region Risk Measure Aggregation Configuration
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    m_RiskMethod = RiskDataLoad.GetRowDataValue(pParsingRow, "Risk Method ID");
                    m_AggregationMethod = ReflectionTools.ConvertStringToEnumOrDefault<PrismaAggregationMethod>(RiskDataLoad.GetRowDataValue(pParsingRow, "Aggregation Method"), PrismaAggregationMethod.NA);
                    break;
                #endregion Risk Measure Aggregation Configuration
                #region Risk Measure Configuration
                // PM 20180903 [24015] Prisma v8.0 : affectation m_IsCalculateTEA 
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    m_IsCalculateTEA = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "Calculate TEA"));
                    break;
                #endregion Risk Measure Configuration
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les données d'un Time To Expiry Adjustment
    /// </summary>
    // PM 20180903 [24015] Prisma v8.0 : new class RiskDataPrismaTimeToExpiryAdjustment
    public sealed class RiskDataPrismaTimeToExpiryAdjustment
    {
        #region Members
        private readonly string m_LiquidationGroup;
        private readonly string m_LiquidationGroupSplit;
        #region Time to Expiry Adjustment
        private readonly Dictionary<int, Tuple<decimal, decimal>> m_Weight;
        #endregion Time to Expiry Adjustment
        #endregion Members
        #region Accessors
        /// <summary>
        /// Liquidation Group
        /// </summary>
        public string LiquidationGroup { get { return m_LiquidationGroup; } }
        /// <summary>
        /// Liquidation Group Split
        /// </summary>
        public string LiquidationGroupSplit { get { return m_LiquidationGroupSplit; } }
        #region Time to Expiry Adjustment
        /// <summary>
        /// key: TEA Bucket ID
        /// value: HedgingWeight,DirectionalWeight
        /// </summary>
        public Dictionary<int, Tuple<decimal, decimal>> Weight { get { return m_Weight; } }
        #endregion Time to Expiry Adjustment
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Time to Expiry Adjustment</param>
        public RiskDataPrismaTimeToExpiryAdjustment(string pLiquidationGroup, string pLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRow)
        {
            m_LiquidationGroup = pLiquidationGroup;
            m_LiquidationGroupSplit = pLiquidationGroupSplit;
            m_Weight = new Dictionary<int, Tuple<decimal, decimal>>();
            if (pParsingRow != default)
            {
                string tea = RiskDataLoad.GetRowDataValue(pParsingRow, "TEA_Weight");
                if (StrFunc.IsFilled(tea))
                {
                    string[] teaWeights = tea.Split(';');
                    if ((teaWeights != default) && (teaWeights.Count() > 0))
                    {
                        int nbValues = ArrFunc.Count(teaWeights);
                        int current = 0;
                        while ((current + 3) <= nbValues)
                        {
                            int idTEA = IntFunc.IntValue(teaWeights[current]);
                            if (false == m_Weight.ContainsKey(idTEA))
                            {
                                decimal hedgingWeight = DecFunc.DecValue(teaWeights[current + 1]);
                                decimal directionalWeight = DecFunc.DecValue(teaWeights[current + 2]);
                                m_Weight.Add(idTEA, new Tuple<decimal, decimal>(hedgingWeight, directionalWeight));
                            }
                            current += 3;
                        }
                    }
                }
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les données d'un Risk Measure Set
    /// </summary>
    public sealed class RiskDataPrismaRiskMeasureSet
    {
        #region Members
        private readonly string m_LiquidationGroup;
        private readonly string m_LiquidationGroupSplit;
        private readonly string m_RiskMeasureSet;
        #region Risk Measure
        private PrismaHistoricalStressedEnum m_HistoricalStressed;
        private string m_RiskMeasureType;
        private decimal m_ConfidenceLevel;
        private bool m_IsUseRobustness;
        private decimal m_ScalingFactor;
        private bool m_IsCorrelactionBreak;
        private int m_CorrelationBreakSubWindow;
        private decimal m_CorrelationBreakConfidenceLevel;
        private decimal m_CorrelationBreakMax;
        private decimal m_CorrelationBreakMin;
        private decimal m_CorrelationBreakMultiplier;
        private bool m_IsLiquidityComponent;
        private decimal m_AlphaConfidenceLevel;
        private decimal m_AlphaFloor;
        private int m_NumberOfWorstScenarios;
        #endregion Risk Measure
        #region Risk Measure Aggregation
        private PrismaAggregationMethod m_AggregationMethod;
        private decimal m_WeightingFactor;
        #endregion Risk Measure Aggregation
        #endregion Members
        #region Accessors
        /// <summary>
        /// Liquidation Group
        /// </summary>
        public string LiquidationGroup { get { return m_LiquidationGroup; } }
        /// <summary>
        /// Liquidation Group Split
        /// </summary>
        public string LiquidationGroupSplit { get { return m_LiquidationGroupSplit; } }
        /// <summary>
        /// Risk Measure Set
        /// </summary>
        public string RiskMeasureSet { get { return m_RiskMeasureSet; } }
        #region Risk Measure
        /// <summary>
        /// Historical / Stressed
        /// </summary>
        public PrismaHistoricalStressedEnum HistoricalStressed { get { return m_HistoricalStressed; } }
        /// <summary>
        /// Risk Measure
        /// </summary>
        public string RiskMeasureType { get { return m_RiskMeasureType; } }
        /// <summary>
        /// Anchor Confidence Level
        /// </summary>
        public decimal ConfidenceLevel { get { return m_ConfidenceLevel; } }
        /// <summary>
        /// Robustness
        /// </summary>
        public bool IsUseRobustness { get { return m_IsUseRobustness; } }
        /// <summary>
        /// Scaling Factor
        /// </summary>
        public decimal ScalingFactor { get { return m_ScalingFactor; } }
        /// <summary>
        /// Correlaction Break Flag
        /// </summary>
        public bool IsCorrelactionBreak { get { return m_IsCorrelactionBreak; } }
        /// <summary>
        /// Moving Sub-Window
        /// </summary>
        public int CorrelationBreakSubWindow { get { return m_CorrelationBreakSubWindow; } }
        /// <summary>
        /// Confidence Level Correlation Break
        /// </summary>
        public decimal CorrelationBreakConfidenceLevel { get { return m_CorrelationBreakConfidenceLevel; } }
        /// <summary>
        /// Cap
        /// </summary>
        public decimal CorrelationBreakMax { get { return m_CorrelationBreakMax; } }
        /// <summary>
        /// Floor
        /// </summary>
        public decimal CorrelationBreakMin { get { return m_CorrelationBreakMin; } }
        /// <summary>
        /// Multiplier
        /// </summary>
        public decimal CorrelationBreakMultiplier { get { return m_CorrelationBreakMultiplier; } }
        /// <summary>
        /// Liquidity Risk Adjustment
        /// </summary>
        public bool IsLiquidityComponent { get { return m_IsLiquidityComponent; } }
        /// <summary>
        /// Confidence Level Diversification Factor
        /// </summary>
        public decimal AlphaConfidenceLevel { get { return m_AlphaConfidenceLevel; } }
        /// <summary>
        /// Alpha Floor
        /// </summary>
        public decimal AlphaFloor { get { return m_AlphaFloor; } }
        /// <summary>
        /// Number Of Worst Scenarios
        /// </summary>
        public int NumberOfWorstScenarios { get { return m_NumberOfWorstScenarios; } }
        #endregion Risk Measure
        #region Risk Measure Aggregation
        /// <summary>
        /// Weighting Factor
        /// </summary>
        public decimal WeightingFactor { get { return m_WeightingFactor; } }
        /// <summary>
        /// Aggregation Method
        /// </summary>
        public PrismaAggregationMethod AggregationMethod { get { return m_AggregationMethod; } }
        #endregion Risk Measure Aggregation
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pRiskMeasureSet">Risk Measure Set</param>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement  Risk Measure Set</param>
        public RiskDataPrismaRiskMeasureSet(string pLiquidationGroup, string pLiquidationGroupSplit, string pRiskMeasureSet, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            m_LiquidationGroup = pLiquidationGroup;
            m_LiquidationGroupSplit = pLiquidationGroupSplit;
            m_RiskMeasureSet = pRiskMeasureSet;
            SetData(pInputSourceDataStyle, pParsingRow);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Ajout de données au Risk Measure Set
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement  Risk Measure Set</param>
        public void SetData(Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            switch (pInputSourceDataStyle)
            {
                #region Risk Measure
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                    m_HistoricalStressed = ReflectionTools.ConvertStringToEnumOrDefault<PrismaHistoricalStressedEnum>(RiskDataLoad.GetRowDataValue(pParsingRow, "Historical / Stressed"), PrismaHistoricalStressedEnum.NA);
                    m_RiskMeasureType = RiskDataLoad.GetRowDataValue(pParsingRow, "Risk Measure");
                    m_ConfidenceLevel = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Anchor Confidence Level"));
                    m_IsUseRobustness = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "Robustness"));
                    m_ScalingFactor = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Scaling Factor"));
                    m_IsCorrelactionBreak = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "Correlation Break Flag"));
                    m_CorrelationBreakSubWindow = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Moving Sub-Window"));
                    m_CorrelationBreakConfidenceLevel = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Confidence Level Correlation Break"));
                    m_CorrelationBreakMax = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Cap"));
                    m_CorrelationBreakMin = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Floor"));
                    m_CorrelationBreakMultiplier = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Multiplier"));
                    m_IsLiquidityComponent = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Risk Component"));
                    m_AlphaConfidenceLevel = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Confidence Level Diversification Factor"));
                    m_AlphaFloor = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Alpha Floor"));
                    m_NumberOfWorstScenarios = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "NoWorstScenarios"));
                    break;
                #endregion Risk Measure
                #region Risk Measure Aggregation
                case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                    m_WeightingFactor = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Weighting Factor"));
                    m_AggregationMethod = ReflectionTools.ConvertStringToEnumOrDefault<PrismaAggregationMethod>(RiskDataLoad.GetRowDataValue(pParsingRow, "Aggregation Method"),PrismaAggregationMethod.NA);
                    break;
                #endregion Risk Measure Aggregation
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les données d'un Product
    /// </summary>
    public sealed class RiskDataPrismaProduct
    {
        #region Members
        
        private readonly string m_ProductId;
        private readonly decimal m_TickSize;
        private readonly decimal m_TickValue;
        private readonly string m_Currency;
        private string m_LiquidityClass;
        private string m_LiquidationGroup;
        private readonly PrismaMarginStyleEnum m_MarginStyle;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Fichier de provenance du Product
        /// </summary>
        public Cst.InputSourceDataStyle InputSourceDataStyle { get; private set; }
        /// <summary>
        /// Product Id
        /// </summary>
        public string ProductId { get { return m_ProductId; } }
        /// <summary>
        /// Tick Size
        /// </summary>
        public decimal TickSize { get { return m_TickSize; } }
        /// <summary>
        /// Tick Value
        /// </summary>
        public decimal TickValue { get { return m_TickValue; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        /// <summary>
        /// Liquidity Class
        /// </summary>
        public string LiquidityClass { get { return m_LiquidityClass; } }
        /// <summary>
        /// Liquidation Group
        /// </summary>
        public string LiquidationGroup { get { return m_LiquidationGroup; } }
        /// <summary>
        /// Margin Style
        /// </summary>
        public PrismaMarginStyleEnum MarginStyle { get { return m_MarginStyle; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement contrat (Product)</param>
        public RiskDataPrismaProduct(Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            InputSourceDataStyle = pInputSourceDataStyle;
            
            m_ProductId = RiskDataLoad.GetRowDataValue(pParsingRow, "Product ID");
            m_TickSize = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Tick Size"));
            m_TickValue = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Tick Value"));
            m_Currency = RiskDataLoad.GetRowDataValue(pParsingRow, "Currency");
            m_MarginStyle = ReflectionTools.ConvertStringToEnumOrDefault<PrismaMarginStyleEnum>( RiskDataLoad.GetRowDataValue(pParsingRow, "Margin Style"), PrismaMarginStyleEnum.NA);
            
            SetData(pParsingRow);
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Mise à jour des données propre à chaque fichier
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement contrat (Product)</param>
        private void SetData(IOTaskDetInOutFileRow pParsingRow)
        {
            switch (InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    m_LiquidityClass = RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Class");
                    m_LiquidationGroup = RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidation Group");
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    break;
            }
        }
        /// <summary>
        /// Mise à jour des données provenant d'un autre fichier
        /// </summary>
        /// <param name="pProduct">Données du contrat (Product)</param>
        public void SetData(RiskDataPrismaProduct pProduct)
        {
            if (pProduct != default)
            {
                switch (pProduct.InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                        m_LiquidityClass = pProduct.m_LiquidityClass;
                        m_LiquidationGroup = pProduct.m_LiquidationGroup;
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                        break;
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les données d'une Expiration
    /// </summary>
    /// FI 20220321 [XXXXX] RiskDataPrismaExpiration herite de PrismaExpiryDateComponent
    public sealed class RiskDataPrismaExpiration : PrismaExpiryDateComponent
    {
        #region Members

        #region EUREXPRISMA_THEORETICALPRICEFILE
        private int m_DaysToExpiry;
        private int m_XMMaturityBucketID;
        // PM 20180903 [24015] Prisma v8.0 : add m_DaysToExpiryBusiness
        private int m_DaysToExpiryBusiness;
        #endregion

        #region EUREXPRISMA_STLPRICESFILE
        private decimal m_UnderlyingClosePrice;
        #endregion 

        
        #endregion Members
        #region Accessors
        /// <summary>
        /// Fichier de provenance de l'Expiration
        /// </summary>
        public Cst.InputSourceDataStyle InputSourceDataStyle { get; private set; }
        
        /// <summary>
        /// Product
        /// </summary>
        public RiskDataPrismaProduct Product { get; set; }
        
        /// <summary>
        /// Days To Expiry
        /// </summary>
        public int DaysToExpiry { get { return m_DaysToExpiry; } }
        /// <summary>
        /// XM Maturity Bucket ID
        /// </summary>
        public int XMMaturityBucketID { get { return m_XMMaturityBucketID; } }
        /// <summary>
        /// Days To Expiry Business
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add DaysToExpiryBusiness
        public int DaysToExpiryBusiness { get { return m_DaysToExpiryBusiness; } }
        /// <summary>
        /// Underlying Close Price
        /// </summary>
        public decimal UnderlyingClosePrice { get { return m_UnderlyingClosePrice; } }


        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pProduct">Données du contract (Product)</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement échéance (Expiration)</param>
        public RiskDataPrismaExpiration(Cst.InputSourceDataStyle pInputSourceDataStyle, RiskDataPrismaProduct pProduct, IOTaskDetInOutFileRow pParsingRow) : base()
        {
            InputSourceDataStyle = pInputSourceDataStyle;

            Product = pProduct;

            ContractYear = RiskDataLoad.GetRowDataValue(pParsingRow, "Contract Year");
            ContractMonth = RiskDataLoad.GetRowDataValue(pParsingRow, "Contract Month");
            ExpirationYear = RiskDataLoad.GetRowDataValue(pParsingRow, "Expiration Year");
            ExpirationMonth = RiskDataLoad.GetRowDataValue(pParsingRow, "Expiration Month");
            ExpirationDay = RiskDataLoad.GetRowDataValue(pParsingRow, "Expiration Day");
            ContractDate = RiskDataLoad.GetRowDataValue(pParsingRow, "Contract Date");

            SetData(pParsingRow);
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Mise à jour des données propre à chaque fichier
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement échéance (Expiration)</param>
        private void SetData(IOTaskDetInOutFileRow pParsingRow)
        {
            switch (InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    m_DaysToExpiry = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Days to Expiry"));
                    m_XMMaturityBucketID = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "XM Maturity bucket ID"));
                    // PM 20180903 [24015] Prisma v8.0 : affectation m_DaysToExpiryBusiness
                    m_DaysToExpiryBusiness = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Days to Expiry Business"));
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    m_UnderlyingClosePrice = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Underlying close Price"));
                    break;
            }
        }

        /// <summary>
        /// Mise à jour des données provenant d'un autre fichier
        /// </summary>
        /// <param name="pExpiration">Données de l'échéance</param>
        public void SetData(RiskDataPrismaExpiration pExpiration)
        {
            if (pExpiration != default)
            {
                switch (pExpiration.InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                        m_DaysToExpiry = pExpiration.m_DaysToExpiry;
                        m_XMMaturityBucketID = pExpiration.m_XMMaturityBucketID;
                        // PM 20180903 [24015] Prisma v8.0 : affectation m_DaysToExpiryBusiness
                        m_DaysToExpiryBusiness = pExpiration.m_DaysToExpiryBusiness;
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                        m_UnderlyingClosePrice = pExpiration.m_UnderlyingClosePrice;
                        break;
                }
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les données d'une Serie
    /// </summary>
    /// FI 20220321 [XXXXX] Herite de PrismaSerieMainComponent
    public sealed class RiskDataPrismaSerie : PrismaSerieMainComponent
    {
        #region Members
        private readonly string m_SerieStatus;
        private readonly decimal m_TradingUnit;

        #region EUREXPRISMA_THEORETICALPRICEFILE
        private string m_MoneynessBucketID;
        private string m_TimeToExpiryBucketID;
        private string m_RiskBucket;
        private decimal m_OptionVega;
        private decimal m_ImpliedVolatility;
        private decimal m_InterestRate;
        private decimal m_DV01;
        private decimal m_Delta;
        private bool m_IsCrossMargin;
        #endregion

        #region EUREXPRISMA_STLPRICESFILE
        private decimal m_SettlementPrice;
        private decimal m_PVReferencePrice;
        private decimal m_UnderlyingPriceOffset;
        #endregion 

        #endregion Members

        #region Accessors
        /// <summary>
        /// Fichier de provenance de la Serie
        /// </summary>
        public Cst.InputSourceDataStyle InputSourceDataStyle { get; private set; }


        /// <summary>
        /// Expiration
        /// </summary>
        public RiskDataPrismaExpiration Expiration { get; set; }

        /// <summary>
        /// Time-To-Expiry Bucket ID
        /// </summary>
        public string TimeToExpiryBucketID { get { return m_TimeToExpiryBucketID; } }
        /// <summary>
        /// Moneyness Bucket ID
        /// </summary>
        public string MoneynessBucketID { get { return m_MoneynessBucketID; } }
        /// <summary>
        /// RiskBucket
        /// </summary>
        public string RiskBucket { get { return m_RiskBucket; } }
        /// <summary>
        /// Serie Status
        /// </summary>
        public string SerieStatus { get { return m_SerieStatus; } }
        /// <summary>
        /// Trading Unit
        /// </summary>
        public decimal TradingUnit { get { return m_TradingUnit; } }
        /// <summary>
        /// Vega
        /// </summary>
        public decimal OptionVega { get { return m_OptionVega; } }
        /// <summary>
        /// Volatility
        /// </summary>
        public decimal ImpliedVolatility { get { return m_ImpliedVolatility; } }
        /// <summary>
        /// Interest Rate
        /// </summary>
        public decimal InterestRate { get { return m_InterestRate; } }
        /// <summary>
        /// DV01
        /// </summary>
        public decimal DV01 { get { return m_DV01; } }
        /// <summary>
        /// Delta
        /// </summary>
        public decimal Delta { get { return m_Delta; } }
        /// <summary>
        /// IsCrossMargin
        /// </summary>
        public bool IsCrossMargin { get { return m_IsCrossMargin; } }
        /// <summary>
        /// NeutralPrice
        /// </summary>
        public decimal NeutralPrice { get; set; }
        /// <summary>
        /// Settlement Price
        /// </summary>
        public decimal SettlementPrice { get { return m_SettlementPrice; } }
        /// <summary>
        /// PV Reference Price
        /// </summary>
        public decimal PVReferencePrice { get { return m_PVReferencePrice; } }
        /// <summary>
        /// Underlying Price Offset
        /// </summary>
        public decimal UnderlyingPriceOffset { get { return m_UnderlyingPriceOffset; } }
        /// <summary>
        /// IdAsset
        /// </summary>
        public int IdAsset { get; set; }
        /// <summary>
        /// Contract Symbol
        /// </summary>
        public string ContractSymbol { get { return IsFlex ? FlexProductID : Expiration.Product.ProductId; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pInputSourceDataStyle">Type de fichier</param>
        /// <param name="pExpiration">Données de l'échéance</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Serie</param>
        public RiskDataPrismaSerie(Cst.InputSourceDataStyle pInputSourceDataStyle, RiskDataPrismaExpiration pExpiration, IOTaskDetInOutFileRow pParsingRow)
        {
            InputSourceDataStyle = pInputSourceDataStyle;

            Expiration = pExpiration;

            CallPut = RiskDataLoad.GetRowDataValue(pParsingRow, "Call Put Flag");
            StrikePrice = RiskDataLoad.GetRowDataValue(pParsingRow, "Exercise Price");
            Version = RiskDataLoad.GetRowDataValue(pParsingRow, "Series Version Number");
            // PM 20180903 [24015] Prisma v8.0 : add SettlementType
            SettlementType = RiskDataLoad.GetRowDataValue(pParsingRow, "Settlement Type");
            ExerciseStyle = RiskDataLoad.GetRowDataValue(pParsingRow, "Series exercise style flag");
            FlexProductID = RiskDataLoad.GetRowDataValue(pParsingRow, "Flex Product ID");
            FlexFlag = RiskDataLoad.GetRowDataValue(pParsingRow, "Flex Series Flag");
            UniqueContractID = RiskDataLoad.GetRowDataValue(pParsingRow, "Unique Contract ID");
            ContractMnemonic = RiskDataLoad.GetRowDataValue(pParsingRow, "Contract Mnemonic");
            ContractFrequency = RiskDataLoad.GetRowDataValue(pParsingRow, "Contract Frequency");

            m_SerieStatus = RiskDataLoad.GetRowDataValue(pParsingRow, "Series Status");
            m_TradingUnit = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Trading Unit"));

            SetData(pParsingRow);

            //SetMaturityMonthYear();
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Mise à jour des données propre à chaque fichier
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement Serie</param>
        private void SetData(IOTaskDetInOutFileRow pParsingRow)
        {
            switch (InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    m_TimeToExpiryBucketID = RiskDataLoad.GetRowDataValue(pParsingRow, "Time-To-Expiry Bucket ID");
                    m_MoneynessBucketID = RiskDataLoad.GetRowDataValue(pParsingRow, "Moneyness Bucket ID");
                    m_RiskBucket = RiskDataLoad.GetRowDataValue(pParsingRow, "Risk Bucket");
                    m_OptionVega = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Option Vega"));
                    m_ImpliedVolatility = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Implied Volatility"));
                    m_InterestRate = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Interest Rate"));
                    m_DV01 = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "DV01"));
                    m_Delta = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Delta"));
                    m_IsCrossMargin = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "XM Eligibility Flag"));
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    m_SettlementPrice = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Settlement Price"));
                    m_PVReferencePrice = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "PV Reference Price"));
                    m_UnderlyingPriceOffset = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Underlying price offset"));
                    break;
            }
        }

        /// <summary>
        /// Mise à jour des données provenant d'un autre fichier
        /// </summary>
        /// <param name="pSerie">Données de risk de l'asset (Serie)</param>
        public void SetData(RiskDataPrismaSerie pSerie)
        {
            switch (pSerie.InputSourceDataStyle)
            {
                case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                    m_TimeToExpiryBucketID = pSerie.m_TimeToExpiryBucketID;
                    m_MoneynessBucketID = pSerie.m_MoneynessBucketID;
                    m_RiskBucket = pSerie.m_RiskBucket;
                    m_OptionVega = pSerie.m_OptionVega;
                    m_ImpliedVolatility = pSerie.m_ImpliedVolatility;
                    m_InterestRate = pSerie.m_InterestRate;
                    m_DV01 = pSerie.m_DV01;
                    m_Delta = pSerie.m_Delta;
                    m_IsCrossMargin = pSerie.m_IsCrossMargin;
                    break;
                case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                    m_SettlementPrice = pSerie.m_SettlementPrice;
                    m_PVReferencePrice = pSerie.m_PVReferencePrice;
                    m_UnderlyingPriceOffset = pSerie.m_UnderlyingPriceOffset;
                    break;
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Class représentant les Liquidation Group Split des assets
    /// </summary>
    public sealed class RiskDataPrismaAssetLGS
    {
        #region Members
        private readonly int m_IdAsset;
        private readonly string m_LiquidationGroupSplit;
        private readonly bool m_IsDefault;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } }
        /// <summary>
        /// Liquidation Group Split
        /// </summary>
        public string LiquidationGroupSplit { get { return m_LiquidationGroupSplit; } }
        /// <summary>
        /// Is Default LGS
        /// </summary>
        public bool IsDefault { get { return m_IsDefault; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset">Id interne de l'asset</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group Split</param>
        public RiskDataPrismaAssetLGS(int pIdAsset, string pLiquidationGroupSplit, IOTaskDetInOutFileRow pParsingRow)
        {
            m_IdAsset = pIdAsset;
            m_LiquidationGroupSplit = pLiquidationGroupSplit;
            m_IsDefault = BoolFunc.IsTrue(RiskDataLoad.GetRowDataValue(pParsingRow, "Default LGS Indicator"));
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les Risk Measure Set des Liquidation Group Split des assets
    /// </summary>
    public sealed class RiskDataPrismaAssetLGSRMS
    {
        #region Members
        // Gestion d'un Id interne pour chaque instance de la classe
        private static int m_IdAssetRmsLgsAccumulator = 0;
        private readonly int m_IdAssetRmsLgs;
        //
        private readonly int m_IdAsset;
        private readonly string m_LiquidationGroupSplit;
        private readonly string m_RiskMeasureSet;
        private readonly int m_LiquidationHorizon;
        private readonly string m_FXSet;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } }
        /// <summary>
        /// Liquidation Group Split
        /// </summary>
        public string LiquidationGroupSplit { get { return m_LiquidationGroupSplit; } }
        /// <summary>
        /// Ris kMeasure Set
        /// </summary>
        public string RiskMeasureSet { get { return m_RiskMeasureSet; } }
        /// <summary>
        /// Liquidation Horizon
        /// </summary>
        public int LiquidationHorizon { get { return m_LiquidationHorizon; } }
        /// <summary>
        /// FX Set
        /// </summary>
        public string FXSet { get { return m_FXSet; } }
        //
        /// <summary>
        /// Id interne pour faciliter l'exploitation
        /// </summary>
        public int IdAssetRmsLgs { get { return m_IdAssetRmsLgs; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset">Id interne de l'asset</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pRiskMeasureSet">Risk Measure Set</param>
        /// <param name="pLiquidationHorizon">Liquidation Horizon</param>
        /// <param name="pFXSet">Jeu de taux de change</param>
        public RiskDataPrismaAssetLGSRMS(int pIdAsset, string pLiquidationGroupSplit, string pRiskMeasureSet, int pLiquidationHorizon, string pFXSet)
        {
            m_IdAsset = pIdAsset;
            m_LiquidationGroupSplit = pLiquidationGroupSplit;
            m_RiskMeasureSet = pRiskMeasureSet;
            m_LiquidationHorizon = pLiquidationHorizon;
            m_FXSet = pFXSet;
            //
            // Gestion Id interne
            m_IdAssetRmsLgsAccumulator += 1;
            m_IdAssetRmsLgs = m_IdAssetRmsLgsAccumulator;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les Scenario Prices des assets par Risk Measure Set et Liquidation Group Split
    /// </summary>
    public sealed class RiskDataPrismaAssetSP
    {
        #region Members
        private readonly RiskDataPrismaAssetLGSRMS m_AssetLGSRMS;
        private readonly decimal[] m_Prices;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Asset + Liquidation Group Split + Risk Measure Set
        /// </summary>
        public RiskDataPrismaAssetLGSRMS AssetLGSRMS { get { return m_AssetLGSRMS; } }
        /// <summary>
        /// Scenarios Prices
        /// </summary>
        public decimal[] Prices { get { return m_Prices; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pAssetLGSRMS">Asset + Liquidation Group Split + Risk Measure Set</param>
        /// <param name="pNeutralPrice">Neutral price</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement scenarios prices</param>
        public RiskDataPrismaAssetSP(RiskDataPrismaAssetLGSRMS pAssetLGSRMS, decimal pNeutralPrice, IOTaskDetInOutFileRow pParsingRow)
        {
            m_AssetLGSRMS = pAssetLGSRMS;
            if (pAssetLGSRMS != default(RiskDataPrismaAssetLGSRMS))
            {
                string sp = RiskDataLoad.GetRowDataValue(pParsingRow, "Scenarios Prices");
                if (StrFunc.IsFilled(sp))
                {
                    string[] spValues = sp.Split(';');
                    if ((spValues != default) && (spValues.Count() > 0))
                    {
                        // Neutral Price pour les valeurs de SP qui ne sont pas renseignées
                        m_Prices = spValues.Select(v => StrFunc.IsEmpty(v) ? pNeutralPrice : DecFunc.DecValue(v)).ToArray();
                    }
                }
                else
                {
                    m_Prices = new decimal[0];
                }
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les Compression Error des assets par Risk Measure Set et Liquidation Group Split
    /// </summary>
    public sealed class RiskDataPrismaAssetCE
    {
        #region Members
        private readonly RiskDataPrismaAssetLGSRMS m_AssetLGSRMS;
        private readonly string m_Currency;
        private readonly decimal m_CE1;
        private readonly decimal m_CE2;
        private readonly decimal m_CE3;
        private readonly decimal m_CE4;
        private readonly decimal m_CE5;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Asset + Liquidation Group Split + Risk Measure Set
        /// </summary>
        public RiskDataPrismaAssetLGSRMS AssetLGSRMS { get { return m_AssetLGSRMS; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        /// <summary>
        /// Compression error 1
        /// </summary>
        public decimal CE1 { get { return m_CE1; } }
        /// <summary>
        /// Compression error 2
        /// </summary>
        public decimal CE2 { get { return m_CE2; } }
        /// <summary>
        /// Compression error 3
        /// </summary>
        public decimal CE3 { get { return m_CE3; } }
        /// <summary>
        /// Compression error 4
        /// </summary>
        public decimal CE4 { get { return m_CE4; } }
        /// <summary>
        /// Compression error 5
        /// </summary>
        public decimal CE5 { get { return m_CE5; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pAssetLGSRMS">Asset + Liquidation Group Split + Risk Measure Set</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Compression Error</param>
        public RiskDataPrismaAssetCE(RiskDataPrismaAssetLGSRMS pAssetLGSRMS, IOTaskDetInOutFileRow pParsingRow)
        {
            m_AssetLGSRMS = pAssetLGSRMS;
            if (pAssetLGSRMS != default(RiskDataPrismaAssetLGSRMS))
            {
                string ce = RiskDataLoad.GetRowDataValue(pParsingRow, "Compression Error/Currency");
                if (StrFunc.IsFilled(ce))
                { 
                    string[] ceValues = ce.Split(';');
                    if ((ceValues != default) && (ceValues.Count() > 0))
                    {
                        int nbValues = System.Math.Min(pAssetLGSRMS.LiquidationHorizon, ArrFunc.Count(ceValues) - 1);
                        //
                        m_Currency = ceValues[ArrFunc.Count(ceValues) - 1];
                        //
                        for (int i = 1; i <= nbValues; i += 1)
                        {
                            switch (i)
                            {
                                case 1:
                                    m_CE1 = DecFunc.DecValue(ceValues[0]);
                                    break;
                                case 2:
                                    m_CE2 = DecFunc.DecValue(ceValues[1]);
                                    break;
                                case 3:
                                    m_CE3 = DecFunc.DecValue(ceValues[2]);
                                    break;
                                case 4:
                                    m_CE4 = DecFunc.DecValue(ceValues[3]);
                                    break;
                                case 5:
                                    m_CE5 = DecFunc.DecValue(ceValues[4]);
                                    break;
                            }
                        }
                    }
                }
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les VaR des assets par Risk Measure Set et Liquidation Group Split
    /// </summary>
    public sealed class RiskDataPrismaAssetVaR
    {
        #region Members
        private readonly RiskDataPrismaAssetLGSRMS m_AssetLGSRMS;
        private readonly string m_VaRType;
        private readonly decimal m_VaR;
        private readonly string m_LongShortIndicator;
        private readonly string m_Currency;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Asset + Liquidation Group Split + Risk Measure Set
        /// </summary>
        public RiskDataPrismaAssetLGSRMS AssetLGSRMS { get { return m_AssetLGSRMS; } }
        /// <summary>
        /// VaR Type : IVAR / AIVAR
        /// </summary>
        public string VaRType { get { return m_VaRType; } }
        /// <summary>
        /// VaR
        /// </summary>
        public decimal VaR { get { return m_VaR; } }
        /// <summary>
        /// Long Short Indicator : S ou L : Indique si la VaR concerne une position long ou short
        /// </summary>
        public string LongShortIndicator { get { return m_LongShortIndicator; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pAssetLGSRMS">Asset + Liquidation Group Split + Risk Measure Set</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Value at Risk</param>
        public RiskDataPrismaAssetVaR(string pVaRType, RiskDataPrismaAssetLGSRMS pAssetLGSRMS, IOTaskDetInOutFileRow pParsingRow)
        {
            if (pAssetLGSRMS != default(RiskDataPrismaAssetLGSRMS))
            {
                m_AssetLGSRMS = pAssetLGSRMS;
                m_VaRType = pVaRType;
                if (pVaRType == "IVAR")
                {
                    m_VaR = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Instrument VaR"));
                }
                else if (pVaRType == "AIVAR")
                {
                    m_VaR = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Additional Instrument VaR"));
                }
                m_LongShortIndicator = RiskDataLoad.GetRowDataValue(pParsingRow, "Long Short Indicator");
                m_Currency = RiskDataLoad.GetRowDataValue(pParsingRow, "Currency");
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les données d'un Liquidity Factor
    /// </summary>
    public sealed class RiskDataPrismaLiquidityFactor
    {
        #region Members
        private readonly string m_LiquidityClass;
        private readonly decimal m_MinPercentThreshold;
        private readonly Nullable<decimal> m_MaxPercentThreshold;
        private readonly decimal m_LiquidityFactorMinThreshold;
        private readonly Nullable<decimal> m_LiquidityFactorMaxThreshold;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Liquidity Class
        /// </summary>
        public string LiquidityClass { get { return m_LiquidityClass; } }
        /// <summary>
        /// Min Percent Threshold
        /// </summary>
        public decimal MinPercentThreshold { get { return m_MinPercentThreshold; } }
        /// <summary>
        /// Max Percent Threshold
        /// </summary>
        public Nullable<decimal> MaxPercentThreshold { get { return m_MaxPercentThreshold; } }
        /// <summary>
        /// Liquidity Factor Min Threshold
        /// </summary>
        public decimal LiquidityFactorMinThreshold { get { return m_LiquidityFactorMinThreshold; } }
        /// <summary>
        /// Liquidity Factor Max Threshold
        /// </summary>
        public Nullable<decimal> LiquidityFactorMaxThreshold { get { return m_LiquidityFactorMaxThreshold; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidity Factor</param>
        public RiskDataPrismaLiquidityFactor(IOTaskDetInOutFileRow pParsingRow)
        {
            string minPercentThreshold = RiskDataLoad.GetRowDataValue(pParsingRow, "Minimum Percentage Threshold");
            string maxPercentThreshold = RiskDataLoad.GetRowDataValue(pParsingRow, "Maximum Percentage Threshold");
            string liquidityFactorMinThreshold = RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Factor Minimum Threshold");
            string liquidityFactorMaxThreshold = RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Factor Maximum Threshold");
            //
            m_LiquidityClass = RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Class");
            m_MinPercentThreshold = DecFunc.DecValue(minPercentThreshold);
            if (StrFunc.IsFilled(maxPercentThreshold))
            {
                m_MaxPercentThreshold = DecFunc.DecValue(maxPercentThreshold);
            }
            else
            {
                m_MaxPercentThreshold = null;
            }
            m_LiquidityFactorMinThreshold = DecFunc.DecValue(liquidityFactorMinThreshold);
            if (StrFunc.IsFilled(liquidityFactorMaxThreshold))
            {
                m_LiquidityFactorMaxThreshold = DecFunc.DecValue(liquidityFactorMaxThreshold);
            }
            else
            {
                m_LiquidityFactorMaxThreshold = null;
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les données d'un Market Capacity
    /// </summary>
    public sealed class RiskDataPrismaMarketCapacity
    {
        #region Members
        private readonly string m_ProductLine;
        private readonly string m_ProductId;
        private readonly string m_UnlIsin;
        private readonly string m_PutCallFlag;
        private readonly string m_TimeToExpiryBucketID;
        private readonly string m_MoneynessBucketID;
        private readonly decimal m_MarketCapacity;
        private readonly decimal m_LiquidityPremium;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Product Line
        /// </summary>
        public string ProductLine { get { return m_ProductLine; } }
        /// <summary>
        /// Product Id
        /// </summary>
        public string ProductId { get { return m_ProductId; } }
        /// <summary>
        /// Unl Isin
        /// </summary>
        public string UnlIsin { get { return m_UnlIsin; } }
        /// <summary>
        /// Put Call Flag
        /// </summary>
        public string PutCallFlag { get { return m_PutCallFlag; } }
        /// <summary>
        /// Time To Expiry Bucket ID
        /// </summary>
        public string TimeToExpiryBucketID { get { return m_TimeToExpiryBucketID; } }
        /// <summary>
        /// Moneyness Bucket ID
        /// </summary>
        public string MoneynessBucketID { get { return m_MoneynessBucketID; } }
        /// <summary>
        /// Market Capacity
        /// </summary>
        public decimal MarketCapacity { get { return m_MarketCapacity; } }
        /// <summary>
        /// Liquidity Premium
        /// </summary>
        public decimal LiquidityPremium { get { return m_LiquidityPremium; } }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pProductId">Id du product (Symbole du contrat)</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Market Capacity</param>
        public RiskDataPrismaMarketCapacity(string pProductId, IOTaskDetInOutFileRow pParsingRow)
        {
            m_ProductLine = RiskDataLoad.GetRowDataValue(pParsingRow, "Product Line");
            m_ProductId = pProductId;
            m_UnlIsin = RiskDataLoad.GetRowDataValue(pParsingRow, "U/L_ISIN");
            m_PutCallFlag = RiskDataLoad.GetRowDataValue(pParsingRow, "Put Call Flag");
            m_TimeToExpiryBucketID = RiskDataLoad.GetRowDataValue(pParsingRow, "Time-To-Expiry Bucket ID");
            m_MoneynessBucketID = RiskDataLoad.GetRowDataValue(pParsingRow, "Moneyness Bucket ID");
            m_MarketCapacity = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Market Capacity"));
            m_LiquidityPremium = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Liquidity Premium"));
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class représentant les données d'un Fx Rate
    /// </summary>
    public sealed class RiskDataPrismaFxRate
    {
        #region Members
        private readonly string m_FxSet;
        private readonly string m_CurrencyPair;
        private readonly decimal m_ExchangeRate;
        private readonly Dictionary<string, Dictionary<int, decimal>> m_RMSExchangeRate;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Fx Set
        /// </summary>
        public string FxSet { get { return m_FxSet; } }
        /// <summary>
        /// Currency Pair
        /// </summary>
        public string CurrencyPair { get { return m_CurrencyPair; } }
        /// <summary>
        /// Exchange Rate
        /// </summary>
        public decimal ExchangeRate { get { return m_ExchangeRate; } }
        /// <summary>
        /// RMS Exchange Rate
        /// (Key = Risk Measure Set)
        /// </summary>
        public Dictionary<string, Dictionary<int, decimal>> RMSExchangeRate { get { return m_RMSExchangeRate; } }   
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRowFxSet">Parsing d'un enregistrement Fx Set (Jeu de taux de change)</param>
        /// <param name="pParsingRowCurrencyPair">Parsing d'un enregistrement Currency Pair</param>
        /// <param name="pParsingRowCurrentExchangeRate">Parsing d'un enregistrement Current Exchange Rate</param>
        public RiskDataPrismaFxRate(IOTaskDetInOutFileRow pParsingRowFxSet, IOTaskDetInOutFileRow pParsingRowCurrencyPair, IOTaskDetInOutFileRow pParsingRowCurrentExchangeRate)
        {
            m_FxSet = RiskDataLoad.GetRowDataValue(pParsingRowFxSet, "FX Set");
            m_CurrencyPair = RiskDataLoad.GetRowDataValue(pParsingRowCurrencyPair, "Currency pair");
            m_ExchangeRate = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(pParsingRowCurrentExchangeRate, "Current Exchange Rate"));
            m_RMSExchangeRate = new Dictionary<string, Dictionary<int, decimal>>();
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Ajout des Exchange Rate des scenarios
        /// </summary>
        /// <param name="pParsingRowRiskMeasureSet">Parsing d'un enregistrement Risk Measure Set des Fx Rate</param>
        public void AddRMS(IOTaskDetInOutFileRow pParsingRowRiskMeasureSet)
        {
            string rms = RiskDataLoad.GetRowDataValue(pParsingRowRiskMeasureSet, "Risk Measure Set");
            if (false == m_RMSExchangeRate.ContainsKey(rms))
            {
                string[] values = RiskDataLoad.GetRowDataValue(pParsingRowRiskMeasureSet, "Exchange Rate for scenarios").Split(';');
                Dictionary<int, decimal> exchangeRate = Enumerable.Range(0, values.Length).ToDictionary(x => x + 1, x => DecFunc.DecValue(values[x]));
                m_RMSExchangeRate.Add(rms, exchangeRate);
            }
        }
        #endregion Methods
    }
    #endregion Class de stockage des données issues des fichiers

    /// <summary>
    /// Class de chargement des fichiers Risk Data Prisma
    /// </summary>
    public class RiskDataLoadPrisma : RiskDataLoad
    {
        #region Members
        #region Record Type
        private const string recordProduct = "P";
        private const string recordExpiry = "E";
        private const string recordSerie = "S";
        private const string recordNeutralScenario = "N";
        private const string recordLiquidationGroup = "LG";
        private const string recordLiquidationGroupSplit = "LGS";
        private const string recordRiskMeasure = "RM";
        private const string recordRiskMeasureSet = "RMS";
        private const string recordLiquidationHorizon = "LH";
        private const string recordFXSet = "FX";
        private const string recordScenarioPrices = "SP";
        private const string recordCompressionError = "CE";
        private const string recordInstrumentVaR = "IVAR";
        private const string recordAdditionalInstrumentVaR = "AIVAR";
        private const string recordCurrencyPair = "P";
        private const string recordCurrencyExchangeRate = "C";
        private const string recordEndOfFile = "*EOF*";
        // PM 20180903 [24015] Prisma v8.0 : add recordTimeToExpiryAdjustment
        private const string recordTimeToExpiryAdjustment = "TEA";
        #endregion Record Type
        //
        #region Semaphore
        /// <summary>
        /// Semaphore pour l'ajout des Series (assets)
        /// </summary>
        private readonly static SemaphoreSlim semaphoreSerie = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Semaphore pour l'ajout des Liquidation Group
        /// </summary>
        private readonly static SemaphoreSlim semaphoreLiquidationGroup = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Semaphore pour l'ajout des Liquidation Group Split
        /// </summary>
        private readonly static SemaphoreSlim semaphoreLiquidationGroupSplit = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Semaphore pour l'ajout des Risk Measure Set
        /// </summary>
        private readonly static SemaphoreSlim semaphoreRiskMeasureSet = new SemaphoreSlim(1, 1);
        #endregion Semaphore
        //
        #region Données importées
        private readonly Dictionary<string, RiskDataPrismaProduct> m_ProductData; // Dictionnaire des contrats importés. Key : ProductId
        private readonly Dictionary<Tuple<string, DateTime>, RiskDataPrismaExpiration> m_ExpirationData; // Dictionnaire de échéance importées. Key : ProductId, ExpirationDate
        private readonly Dictionary<int, RiskDataPrismaSerie> m_SerieData; // Dictionnaire des assets importés. Key : IdAsset
        private readonly List<RiskDataPrismaAssetLGS> m_AssetLGSData; // Liste des Liquidation Group Split des assets
        private readonly List<RiskDataPrismaAssetLGSRMS> m_AssetLGSRMSData; // Liste des Risk Measure Set des Liquidation Group Split des assets
        private readonly List<RiskDataPrismaAssetSP> m_AssetSPData; // Liste des Scenario Prices des Risk Measure Set des Liquidation Group Split des assets
        private readonly List<RiskDataPrismaAssetCE> m_AssetCEData; // Liste des Compression Error des Risk Measure Set des Liquidation Group Split des assets
        private readonly List<RiskDataPrismaAssetVaR> m_AssetVaRData; // Liste des Values at Risk des Risk Measure Set des Liquidation Group Split des assets
        //
        private readonly Dictionary<string, RiskDataPrismaLiquidationGroup> m_LiquidationGroupData; // Dictionnaire des Liquidation Group importés. Key : LiquidationGroup
        private readonly Dictionary<Tuple<string, string>, RiskDataPrismaLiquidationGroupSplit> m_LiquidationGroupSplitData;  // Dictionnaire des Liquidation Group Split importés. Key Tuple : LiquidationGroup, LiquidationGroupSplit
        // PM 20180903 [24015] Prisma v8.0 : add m_TimeToExpiryAdjustmentData
        private readonly List<RiskDataPrismaTimeToExpiryAdjustment> m_TimeToExpiryAdjustmentData;    // Liste des Time To Expiry Adjustment importés
        private readonly Dictionary<Tuple<string, string, string>, RiskDataPrismaRiskMeasureSet> m_RiskMeasureSetData;   // Dictionnaire des Risk Measure Set importés. Key Tuple : LiquidationGroup, LiquidationGroupSplit, RiskMeasureSet
        //
        private List<RiskDataPrismaLiquidityFactor> m_LiquidityFactorData; // Liste des Liquidity Factor importés.
        private List<RiskDataPrismaMarketCapacity> m_MarketCapacityData; // Liste des Market Capacity importés.
        private List<RiskDataPrismaFxRate> m_FxRateData; // Liste des taux de change importés.
        #endregion Données importées
        #endregion Members
        #region Accessors
        #region Données importées
        #region Données par asset
        /// <summary>
        /// Ensemble des assets
        /// </summary>
        public List<RiskDataPrismaSerie> SerieData { get { return m_SerieData.Values.ToList(); } }
        /// <summary>
        /// Ensemble des Liquidation Group Split par asset
        /// </summary>
        public List<RiskDataPrismaAssetLGS> AssetLGSData { get { return m_AssetLGSData; } }
        /// <summary>
        /// Ensemble des Risk Measure Set de chaque Liquidation Group Split par asset
        /// </summary>
        public List<RiskDataPrismaAssetLGSRMS> AssetLGSRMSData { get { return m_AssetLGSRMSData; } }
        /// <summary>
        /// Ensemble des Scenario Prices des Risk Measure Set de chaque Liquidation Group Split par asset
        /// </summary>
        public List<RiskDataPrismaAssetSP> AssetSPData { get { return m_AssetSPData; } }
        /// <summary>
        /// Ensemble des Compression Error des Risk Measure Set de chaque Liquidation Group Split par asset
        /// </summary>
        public List<RiskDataPrismaAssetCE> AssetCEData { get { return m_AssetCEData; } }
        /// <summary>
        /// Ensemble des Values at Risk des Risk Measure Set de chaque Liquidation Group Split par asset
        /// </summary>
        public List<RiskDataPrismaAssetVaR> AssetVaRData { get { return m_AssetVaRData; } }
        #endregion Données par asset
        /// <summary>
        /// Ensemble des Liquidation Group
        /// </summary>
        public List<RiskDataPrismaLiquidationGroup> LiquidationGroupData { get { return m_LiquidationGroupData.Values.ToList(); } }
        /// <summary>
        /// Ensemble des Liquidation Group Split
        /// </summary>
        public List<RiskDataPrismaLiquidationGroupSplit> LiquidationGroupSplitData { get { return m_LiquidationGroupSplitData.Values.ToList(); } }
        /// <summary>
        /// Ensemble des Time To Expiry Adjustment importés
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustmentData
        public List<RiskDataPrismaTimeToExpiryAdjustment> TimeToExpiryAdjustmentData { get { return m_TimeToExpiryAdjustmentData; } }
        /// <summary>
        /// Ensemble des Risk Measure Set
        /// </summary>
        public List<RiskDataPrismaRiskMeasureSet> RiskMeasureSetData { get { return m_RiskMeasureSetData.Values.ToList(); } }
        /// <summary>
        /// Ensemble des Liquidity Factor
        /// </summary>
        public List<RiskDataPrismaLiquidityFactor> LiquidityFactorData { get { return m_LiquidityFactorData; } }
        /// <summary>
        /// Ensemble des Market Capacity
        /// </summary>
        public List<RiskDataPrismaMarketCapacity> MarketCapacityData { get { return m_MarketCapacityData; } }
        /// <summary>
        /// Ensemble des taux de change
        /// </summary>
        public List<RiskDataPrismaFxRate> FxRateData { get { return m_FxRateData; } }
        #endregion Données importées
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMethodType">Nom de la méthode de calcul pour laquelle charger les données</param>
        /// <param name="pAssetETD"> Assets d'un  même marché pour lesquels charger les données</param>
        /// PM 20190222 [24326] Ajout m_MethodType
        ///public RiskDataLoadPrisma(RiskDataAsset pAsset): base(pAsset)
        /// FI 20220321 [XXXXX] pAsset de type IEnumerable<IAssetETDFields>
        public RiskDataLoadPrisma(InitialMarginMethodEnum pMethodType, IEnumerable<IAssetETDIdent> pAssetETD)
            : base(pMethodType, pAssetETD)
        {
            // Création des ensembles vide de données importées
            m_ProductData = new Dictionary<string,RiskDataPrismaProduct>();
            m_ExpirationData = new Dictionary<Tuple<string, DateTime>, RiskDataPrismaExpiration>();
            m_SerieData = new Dictionary<int, RiskDataPrismaSerie>();
            m_AssetLGSData = new List<RiskDataPrismaAssetLGS>();
            m_AssetLGSRMSData = new List<RiskDataPrismaAssetLGSRMS>();
            m_AssetSPData = new List<RiskDataPrismaAssetSP>();
            m_AssetCEData = new List<RiskDataPrismaAssetCE>();
            m_AssetVaRData = new List<RiskDataPrismaAssetVaR>();
            //
            m_LiquidationGroupData = new Dictionary<string, RiskDataPrismaLiquidationGroup>();
            m_LiquidationGroupSplitData = new Dictionary<Tuple<string, string>, RiskDataPrismaLiquidationGroupSplit>();
            // PM 20180903 [24015] Prisma v8.0 : add m_TimeToExpiryAdjustmentData
            m_TimeToExpiryAdjustmentData = new List<RiskDataPrismaTimeToExpiryAdjustment>();
            m_RiskMeasureSetData = new Dictionary<Tuple<string, string, string>, RiskDataPrismaRiskMeasureSet>();
            //
            m_LiquidityFactorData = new List<RiskDataPrismaLiquidityFactor>();
            m_MarketCapacityData = new List<RiskDataPrismaMarketCapacity>();
            m_FxRateData = new List<RiskDataPrismaFxRate>();
        }
        #endregion Constructors
        #region Methods
        /// <summary>
        /// Chargement des données via le RiskDataElementLoader
        /// </summary>
        /// <param name="pRiskDataLoader">Objet de chargement du flux de données d'un element d'importation</param>
        /// <returns></returns>
        public override Cst.ErrLevel LoadFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret;
            if (pRiskDataLoader != default(RiskDataElementLoader))
            {
                switch (pRiskDataLoader.TaskInput.InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE:
                        ret = LoadTheoreticalPriceFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE:
                        ret = LoadRiskMeasureFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE:
                        ret = LoadRiskMeasureAggregationFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_LIQUIDITYFACTORSFILE:
                        ret = LoadLiquidityFactorsFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_MARKETCAPACITIESFILE:
                        ret = LoadMarketCapacitiesFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_FXRATESFILE:
                        ret = LoadFxRatesFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE:
                        ret = LoadStlPricesFile(pRiskDataLoader);
                        break;
                    default:
                        ret = Cst.ErrLevel.NOTHINGTODO;
                        break;
                }
            }
            else
            {
                 ret = Cst.ErrLevel.INCORRECTPARAMETER;
            }
            return ret;
        }

        #region private Methods
        /// <summary>
        /// Chargement des données du fichier TheoreticalPrice
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadTheoreticalPriceFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            RiskDataPrismaProduct product = default;
            RiskDataPrismaExpiration expiration = default;
            RiskDataPrismaSerie serie = default;
            Boolean isAssetOk = false; // Indique s'il faut charger les données de l'asset courant
            //
            int currentIdAsset = 0; // Id interne de l'asset courant
            bool isFirstAssetOfProduct = true;
            bool isFirstAssetOfExpiration = true;
            decimal currentNeutralPrice = 0;
            string currentLiquidationGroupSplit = string.Empty;
            string currentRiskMeasureSet = string.Empty;
            int currentLiquidationHorizon = 0;
            RiskDataPrismaAssetLGSRMS currentAssetLGSRMS = default;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string recordType = GetRecordType(currentLine);
                    switch (recordType)
                    {
                        case recordProduct: // Product
                            #region Product
                            IOTaskDetInOutFileRow parsingRowProduct = pRiskDataLoader.ParseLine(currentLine);
                            product = new RiskDataPrismaProduct(Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE, parsingRowProduct);
                            isFirstAssetOfProduct = true;
                            break;
                            #endregion Product
                        case recordExpiry: // Expiry
                            #region Expiry
                            IOTaskDetInOutFileRow parsingRowExpiry = pRiskDataLoader.ParseLine(currentLine);
                            expiration = new RiskDataPrismaExpiration(Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE, product, parsingRowExpiry);
                            isFirstAssetOfExpiration = true;
                            break;
                            #endregion Expiry
                        case recordSerie: // Serie
                            #region Serie
                            IOTaskDetInOutFileRow parsingRowSerie = pRiskDataLoader.ParseLine(currentLine);
                            serie = new RiskDataPrismaSerie(Cst.InputSourceDataStyle.EUREXPRISMA_THEORETICALPRICEFILE, expiration, parsingRowSerie);
                            break;
                            #endregion Serie
                        case recordNeutralScenario: // Neutral Scenario
                            #region Neutral Scenario
                            currentIdAsset = GetIdAssetETD(serie);
                            if (currentIdAsset > 0)
                            {
                                IOTaskDetInOutFileRow parsingRowNeutralScenario = pRiskDataLoader.ParseLine(currentLine);
                                currentNeutralPrice = DecFunc.DecValue(RiskDataLoad.GetRowDataValue(parsingRowNeutralScenario, "Theorical price for neutral scenario"));
                                //
                                serie.IdAsset = currentIdAsset;
                                serie.NeutralPrice = currentNeutralPrice;
                                serie = AddSerie(serie, isFirstAssetOfProduct, isFirstAssetOfExpiration);
                                // Affecter de nouveau Product et/ou Expiration
                                if (isFirstAssetOfProduct)
                                {
                                    product = serie.Expiration.Product;
                                    isFirstAssetOfProduct = false;
                                }
                                if (isFirstAssetOfExpiration)
                                {
                                    expiration = serie.Expiration;
                                    isFirstAssetOfExpiration = false;
                                }
                                //
                                isAssetOk = true;
                            }
                            else
                            {
                                isAssetOk = false;
                            }
                            break;
                            #endregion Neutral Scenario
                        case recordLiquidationGroupSplit: // Liquidation Group Split
                            #region Liquidation Group Split
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                currentLiquidationGroupSplit = RiskDataLoad.GetRowDataValue(parsingRow, "Liquidation Group Split");
                                RiskDataPrismaAssetLGS assetLGS = new RiskDataPrismaAssetLGS(currentIdAsset, currentLiquidationGroupSplit, parsingRow);
                                m_AssetLGSData.Add(assetLGS);
                            }
                            break;
                            #endregion Liquidation Group Split
                        case recordRiskMeasureSet: // Risk Measure Set
                            #region Risk Measure Set
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                currentRiskMeasureSet = RiskDataLoad.GetRowDataValue(parsingRow, "Risk Measure Set");
                            }
                            break;
                            #endregion Risk Measure Set
                        case recordLiquidationHorizon: // Liquidation Horizon
                            #region Liquidation Horizon
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                currentLiquidationHorizon = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(parsingRow, "Liquidation Horizon"));
                            }
                            break;
                            #endregion Liquidation Horizon
                        case recordFXSet: // FX Set
                            #region FX Set
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                string currentFXSet = RiskDataLoad.GetRowDataValue(parsingRow, "FX Set");
                                currentAssetLGSRMS = new RiskDataPrismaAssetLGSRMS(currentIdAsset, currentLiquidationGroupSplit, currentRiskMeasureSet, currentLiquidationHorizon, currentFXSet);
                                m_AssetLGSRMSData.Add(currentAssetLGSRMS);
                            }
                            break;
                            #endregion FX Set
                        case recordScenarioPrices: // Scenario Prices
                            #region Scenario Prices
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                RiskDataPrismaAssetSP assetSP = new RiskDataPrismaAssetSP(currentAssetLGSRMS, currentNeutralPrice, parsingRow);
                                m_AssetSPData.Add(assetSP);
                            }
                            break;
                            #endregion Scenario Prices
                        case recordCompressionError: // Compression Error
                            #region Compression Error
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                RiskDataPrismaAssetCE assetCE = new RiskDataPrismaAssetCE(currentAssetLGSRMS, parsingRow);
                                m_AssetCEData.Add(assetCE);
                            }
                            break;
                            #endregion Compression Error
                        case recordInstrumentVaR: // Instrument VaR
                        case recordAdditionalInstrumentVaR: // Additional Instrument VaR
                            #region Instrument VaR & Additional Instrument VaR
                            if (isAssetOk)
                            {
                                IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                                RiskDataPrismaAssetVaR assetVar = new RiskDataPrismaAssetVaR(recordType, currentAssetLGSRMS, parsingRow);
                                m_AssetVaRData.Add(assetVar);
                            }
                            break;
                            #endregion Instrument VaR & Additional Instrument VaR
                        case recordEndOfFile:
                            break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier RiskMeasure
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadRiskMeasureFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string liquidationGroup = string.Empty;
            string liquidationGroupSplit = string.Empty;

            List<Task> addingDataTasks = new List<Task>();
            Task currentTask;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string recordType = GetRecordType(currentLine);
                    switch (recordType)
                    {
                        case recordLiquidationGroup: // Liquidation Group
                            IOTaskDetInOutFileRow parsiongRowLiquidationGroup = pRiskDataLoader.ParseLine(currentLine);
                            liquidationGroup = RiskDataLoad.GetRowDataValue(parsiongRowLiquidationGroup, "Liquidation Group");
                            currentTask = AddLiquidationGroup(liquidationGroup, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE, parsiongRowLiquidationGroup);
                            addingDataTasks.Add(currentTask);
                            break;

                        case recordLiquidationGroupSplit: // Liquidation Group Split
                            IOTaskDetInOutFileRow parsiongRowLiquidationGroupSplit = pRiskDataLoader.ParseLine(currentLine);
                            liquidationGroupSplit = RiskDataLoad.GetRowDataValue(parsiongRowLiquidationGroupSplit, "Liquidation Group Split");
                            currentTask = AddLiquidationGroupSplit(liquidationGroup, liquidationGroupSplit, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE, parsiongRowLiquidationGroupSplit);
                            addingDataTasks.Add(currentTask);
                            break;

                        // PM 20180903 [24015] Prisma v8.0 : add management of recordTimeToExpiryAdjustment 
                        case recordTimeToExpiryAdjustment: // Time to Expiry Adjustment
                            IOTaskDetInOutFileRow parsiongRowTimeToExpiryAdjustment = pRiskDataLoader.ParseLine(currentLine);
                            RiskDataPrismaTimeToExpiryAdjustment timeToExpiryAdjustmentData = new RiskDataPrismaTimeToExpiryAdjustment(liquidationGroup, liquidationGroupSplit, parsiongRowTimeToExpiryAdjustment);
                            m_TimeToExpiryAdjustmentData.Add(timeToExpiryAdjustmentData);
                            break;

                        case recordRiskMeasureSet: // Risk Measure Set
                            IOTaskDetInOutFileRow parsingRowRiskMeasureSet = pRiskDataLoader.ParseLine(currentLine);
                            string riskMeasureSet = RiskDataLoad.GetRowDataValue(parsingRowRiskMeasureSet, "Risk Measure Set ID");
                            currentTask = AddRiskMeasureSet(liquidationGroup, liquidationGroupSplit, riskMeasureSet, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREFILE, parsingRowRiskMeasureSet);
                            addingDataTasks.Add(currentTask);
                            break;

                        case recordEndOfFile:
                            break;
                    }
                }
            }
            // Attendre la fin du stockage de toutes les données
            Task.WaitAll(addingDataTasks.ToArray());
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier RiskMeasureAggregation
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadRiskMeasureAggregationFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            string liquidationGroup = string.Empty;
            string liquidationGroupSplit = string.Empty;
            List<Task> addingDataTasks = new List<Task>();
            Task currentTask;

            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string recordType = GetRecordType(currentLine);
                    switch (recordType)
                    {
                        case recordLiquidationGroup: // Liquidation Group
                            IOTaskDetInOutFileRow parsiongRowLiquidationGroup = pRiskDataLoader.ParseLine(currentLine);
                            liquidationGroup = RiskDataLoad.GetRowDataValue(parsiongRowLiquidationGroup, "Liquidation Group");
                            currentTask = AddLiquidationGroup(liquidationGroup, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE, parsiongRowLiquidationGroup);
                            addingDataTasks.Add(currentTask);
                            break;

                        case recordLiquidationGroupSplit: // Liquidation Group Split
                            IOTaskDetInOutFileRow parsiongRowLiquidationGroupSplit = pRiskDataLoader.ParseLine(currentLine);
                            liquidationGroupSplit = RiskDataLoad.GetRowDataValue(parsiongRowLiquidationGroupSplit, "Liquidation Group Split");
                            break;

                        case recordRiskMeasure: // Risk Measure
                            IOTaskDetInOutFileRow parsingRowRiskMeasures = pRiskDataLoader.ParseLine(currentLine);
                            currentTask = AddLiquidationGroupSplit(liquidationGroup, liquidationGroupSplit, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE, parsingRowRiskMeasures);
                            addingDataTasks.Add(currentTask);
                            break;

                        case recordRiskMeasureSet: // Risk Measure Set
                            IOTaskDetInOutFileRow parsingRowRiskMeasureSet = pRiskDataLoader.ParseLine(currentLine);
                            string riskMeasureSet = RiskDataLoad.GetRowDataValue(parsingRowRiskMeasureSet, "Risk Measure Set ID");
                            currentTask = AddRiskMeasureSet(liquidationGroup, liquidationGroupSplit, riskMeasureSet, Cst.InputSourceDataStyle.EUREXPRISMA_RISKMEASUREAGGREGATIONFILE, parsingRowRiskMeasureSet);
                            addingDataTasks.Add(currentTask);
                            break;

                        case recordEndOfFile:
                            break;
                    }
                }
            }
            // Attendre la fin du stockage de toutes les données
            Task.WaitAll(addingDataTasks.ToArray());
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier LiquidityFactors
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadLiquidityFactorsFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            int lineNumber = 0;
            int guard = 99999999;
            //
            m_LiquidityFactorData = new List<RiskDataPrismaLiquidityFactor>();
            //
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string firstValue = GetFirstElement(currentLine);
                    if (firstValue != recordEndOfFile)
                    {
                        IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                        RiskDataPrismaLiquidityFactor data = new RiskDataPrismaLiquidityFactor(parsingRow);
                        m_LiquidityFactorData.Add(data);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier MarketCapacities
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadMarketCapacitiesFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            int lineNumber = 0;
            int guard = 99999999;

            m_MarketCapacityData = new List<RiskDataPrismaMarketCapacity>();

            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string firstValue = GetFirstElement(currentLine);
                    if (firstValue != recordEndOfFile)
                    {
                        IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                        string productID = GetRowDataValue(parsingRow, "Product ID");
                        // FI 20190130 [24493] Spheres® doit charger tous les enregistrements de Market Capacities
                        // Cela est nécessaire car sur des positions sur contrat flex néessite les informations du contrat standard
                        RiskDataPrismaMarketCapacity data = new RiskDataPrismaMarketCapacity(productID, parsingRow);
                        m_MarketCapacityData.Add(data);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier FxRates
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadFxRatesFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            IOTaskDetInOutFileRow parsingRowFxSet = default;
            IOTaskDetInOutFileRow parsingRowCurrencyPair = default;
            //
            RiskDataPrismaFxRate fxRate = default;
            //
            int lineNumber = 0;
            int guard = 99999999;
            //
            m_FxRateData = new List<RiskDataPrismaFxRate>();
            //
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string recordType = GetRecordType(currentLine);
                    switch (recordType)
                    {
                        case recordFXSet: // FX SET
                            parsingRowFxSet = pRiskDataLoader.ParseLine(currentLine);
                            break;

                        case recordCurrencyPair: // Currency Pair
                            parsingRowCurrencyPair = pRiskDataLoader.ParseLine(currentLine);
                            break;

                        case recordCurrencyExchangeRate: // Currency Exchange Rate
                            IOTaskDetInOutFileRow parsingRowCurrencyExchangeRate = pRiskDataLoader.ParseLine(currentLine);
                            if (fxRate != default)
                            {
                                m_FxRateData.Add(fxRate);
                            }
                            fxRate = new RiskDataPrismaFxRate(parsingRowFxSet, parsingRowCurrencyPair, parsingRowCurrencyExchangeRate);
                            break;

                        case recordRiskMeasureSet: // Risk Measure Set and Exchange Rate Scenarios
                            IOTaskDetInOutFileRow parsingRowRiskMeasureSet = pRiskDataLoader.ParseLine(currentLine);
                            if (fxRate != default)
                            {
                                fxRate.AddRMS(parsingRowRiskMeasureSet);
                            }
                            break;

                        case recordEndOfFile:
                            break;
                    }
                }
            }
            if (fxRate != default)
            {
                m_FxRateData.Add(fxRate);
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier StlPrices
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadStlPricesFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;

            RiskDataPrismaProduct product = default;
            RiskDataPrismaExpiration expiration = default;
            bool isFirstAssetOfProduct = true;
            bool isFirstAssetOfExpiration = true;

            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string recordType = GetRecordType(currentLine);
                    switch (recordType)
                    {
                        case recordProduct: // Product
                            IOTaskDetInOutFileRow parsingRowProduct = pRiskDataLoader.ParseLine(currentLine);
                            product = new RiskDataPrismaProduct(Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE, parsingRowProduct);
                            isFirstAssetOfProduct = true;
                            break;

                        case recordExpiry: // Expiry
                            IOTaskDetInOutFileRow parsingRowExpiry = pRiskDataLoader.ParseLine(currentLine);
                            expiration = new RiskDataPrismaExpiration(Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE, product, parsingRowExpiry);
                            isFirstAssetOfExpiration = true;
                            break;

                        case recordSerie: // Serie
                            IOTaskDetInOutFileRow parsingRowSerie = pRiskDataLoader.ParseLine(currentLine);
                            RiskDataPrismaSerie serie = new RiskDataPrismaSerie(Cst.InputSourceDataStyle.EUREXPRISMA_STLPRICESFILE, expiration, parsingRowSerie);
                            int currentIdAsset = GetIdAssetETD(serie);
                            if (currentIdAsset > 0)
                            {
                                serie.IdAsset = currentIdAsset;
                                serie = AddSerie(serie, isFirstAssetOfProduct, isFirstAssetOfExpiration);
                                // Affecter de nouveau Product et/ou Expiration
                                if (isFirstAssetOfProduct)
                                {
                                    product = serie.Expiration.Product;
                                    isFirstAssetOfProduct = false;
                                }
                                if (isFirstAssetOfExpiration)
                                {
                                    expiration = serie.Expiration;
                                    isFirstAssetOfExpiration = false;
                                }
                            }
                            break;

                        case recordEndOfFile:
                            break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Rechercher l'Id Asset d'une serie
        /// </summary>
        /// <param name="pSerie">Données de la série</param>
        /// <returns></returns>
        /// FI 20220321 [XXXXX] refactoring
        /// PM 20220701 [XXXXX] Ajout recherche par date puis par nom d'échéance
        private int GetIdAssetETD(RiskDataPrismaSerie pSerie)
        {
            // Recherche de l'asset en premier grace au nom de l'échéance au format YYYYMMDD (à partir de ContractDate)
            MarketAssetETDRequestSettings settings = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityMonthYear);
            MarketAssetETDRequest request = PrismaTools.GetAssetRequestContractDate(pSerie.Expiration.Product.ProductId, pSerie.Expiration, pSerie);

            // Rechercher l'Id de l'asset
            IAssetETDIdent asset = m_DataAsset.GetAsset(settings, request);

            if (asset == default(IAssetETDIdent))
            {
                // Asset non trouvé : nouvelle tentative avec la date d'échéance (à partir de ContractDate)
                settings = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityDate);

                // Rechercher l'Id de l'asset
                asset = m_DataAsset.GetAsset(settings, request);

                // PM 20230328 [XXXXX] Utiliser la recherche format court que pour les Monthly
                //if (asset == default(IAssetETDIdent))
                if ((asset == default(IAssetETDIdent)) && (pSerie.ContractFrequencyEnum == ContractFrequencyEnum.Month))
                    {
                        // Asset non trouvé : nouvelle tentative avec le nom de l'échéance avec l'ancien format
                        settings = PrismaTools.GetAssetETDRequestSettings(AssetETDRequestMaturityMode.MaturityMonthYear);
                    request = PrismaTools.GetAssetRequestContractMonthYear(pSerie.Expiration.Product.ProductId, pSerie.Expiration, pSerie);

                    // Rechercher l'Id de l'asset
                    asset = m_DataAsset.GetAsset(settings, request);
                }
            }
            return (null != asset) ? asset.IdAsset : 0;
        }

        /// <summary>
        /// Ajout d'un Product dans le dictionnaire des Product ou mise à jour des données si celui-ci existe déjà
        /// </summary>
        /// <param name="pProduct">Données du product</param>
        /// <returns></returns>
        private RiskDataPrismaProduct AddProduct(RiskDataPrismaProduct pProduct)
        {
            if (m_ProductData.TryGetValue(pProduct.ProductId, out RiskDataPrismaProduct productAdded))
            {
                productAdded.SetData(pProduct);
            }
            else
            {
                productAdded = pProduct;
                m_ProductData.Add(productAdded.ProductId, productAdded);
            }
            return productAdded;
        }

        /// <summary>
        /// Ajout d'une échéance (Expiration) dans le dictionnaire des échéancee ou mise à jour des données si celle-ci existe déjà
        /// </summary>
        /// <param name="pExpiration">Données de l'échéance</param>
        /// <returns></returns>
        private RiskDataPrismaExpiration AddExpiration(RiskDataPrismaExpiration pExpiration)
        {
            Tuple<string, DateTime> key = new Tuple<string, DateTime>(pExpiration.Product.ProductId, pExpiration.GetExpirationDate());
            if (m_ExpirationData.TryGetValue(key, out RiskDataPrismaExpiration expirationAdded))
            {
                expirationAdded.SetData(pExpiration);
            }
            else
            {
                expirationAdded = pExpiration;
                m_ExpirationData.Add(key, expirationAdded);
            }
            return expirationAdded;
        }

        /// <summary>
        /// Ajout d'un asset (Série) dans le dictionnaire des assets ou mise à jour des données si celui-ci existe déjà
        /// </summary>
        /// <param name="pSerie">Données de l'asset</param>
        /// <param name="pIsFirstAssetOfProduct">L'asset est-il le premier rencontré pour le contrat</param>
        /// <param name="pIsFirstAssetOfExpiration">L'asset est-il le premier rencontré pour l'échéance</param>
        /// <returns></returns>
        private RiskDataPrismaSerie AddSerie(RiskDataPrismaSerie pSerie, bool pIsFirstAssetOfProduct, bool pIsFirstAssetOfExpiration)
        {
            RiskDataPrismaSerie serieAdded;
            semaphoreSerie.Wait();
            try
            {
                if (m_SerieData.TryGetValue(pSerie.IdAsset, out serieAdded))
                {
                    serieAdded.SetData(pSerie);
                }
                else
                {
                    serieAdded = pSerie;
                    m_SerieData.Add(serieAdded.IdAsset, serieAdded);
                }
                if (pIsFirstAssetOfExpiration)
                {
                    serieAdded.Expiration = AddExpiration(serieAdded.Expiration);
                }
                if (pIsFirstAssetOfProduct)
                {
                    serieAdded.Expiration.Product = AddProduct(serieAdded.Expiration.Product);
                }
            }
            finally
            {
                semaphoreSerie.Release();
            }
            return serieAdded;
        }

        /// <summary>
        /// Ajout d'un Liquidation Group dans le dictionnaire des Liquidation Group
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group</param>
        /// <param name="pInputSourceDataStyle">Type de fichier dont provient le Liquidation Group</param>
        /// <param name="pParsingRow">Parsing d'un record Liquidation Group</param>
        private async Task AddLiquidationGroup(string pLiquidationGroup, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            await semaphoreLiquidationGroup.WaitAsync();
            try
            {
                if (m_LiquidationGroupData.TryGetValue(pLiquidationGroup, out RiskDataPrismaLiquidationGroup liquidationGroupData))
                {
                    liquidationGroupData.SetData(pInputSourceDataStyle, pParsingRow);
                }
                else
                {
                    liquidationGroupData = new RiskDataPrismaLiquidationGroup(pLiquidationGroup, pInputSourceDataStyle, pParsingRow);
                    m_LiquidationGroupData.Add(pLiquidationGroup, liquidationGroupData);
                }
            }
            finally
            {
                semaphoreLiquidationGroup.Release();
            }
        }

        /// <summary>
        /// Ajout d'un Liquidation Group Split dans le dictionnaire des Liquidation Group Split
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group du Liquidation Group Split</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split</param>
        /// <param name="pInputSourceDataStyle">Type de fichier dont provient le Liquidation Group Split</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Liquidation Group Split</param>
        /// <returns></returns>
        private async Task AddLiquidationGroupSplit(string pLiquidationGroup, string pLiquidationGroupSplit, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            await semaphoreLiquidationGroupSplit.WaitAsync();
            try
            {
                Tuple<string, string> key = new Tuple<string, string>(pLiquidationGroup, pLiquidationGroupSplit);
                if (m_LiquidationGroupSplitData.TryGetValue(key, out RiskDataPrismaLiquidationGroupSplit liquidationGroupSplitData))
                {
                    liquidationGroupSplitData.SetData(pInputSourceDataStyle, pParsingRow);
                }
                else
                {
                    liquidationGroupSplitData = new RiskDataPrismaLiquidationGroupSplit(pLiquidationGroup, pLiquidationGroupSplit, pInputSourceDataStyle, pParsingRow);
                    m_LiquidationGroupSplitData.Add(key, liquidationGroupSplitData);
                }
            }
            finally
            {
                semaphoreLiquidationGroupSplit.Release();
            }
        }

        /// <summary>
        /// Ajout d'un Risk Measure Set dans le dictionnaire des Risk Measure Set
        /// </summary>
        /// <param name="pLiquidationGroup">Liquidation Group du Risk Measure Set</param>
        /// <param name="pLiquidationGroupSplit">Liquidation Group Split du Risk Measure Set</param>
        /// <param name="pRiskMeasureSet">Risk Measure Set</param>
        /// <param name="pInputSourceDataStyle">Type de fichier dont provient le Risk Measure Set</param>
        /// <param name="pParsingRow">Parsing d'un enregistrement Risk Measure Set</param>
        /// <returns></returns>
        private async Task AddRiskMeasureSet(string pLiquidationGroup, string pLiquidationGroupSplit, string pRiskMeasureSet, Cst.InputSourceDataStyle pInputSourceDataStyle, IOTaskDetInOutFileRow pParsingRow)
        {
            await semaphoreRiskMeasureSet.WaitAsync();
            try
            {
                Tuple<string, string, string> key = new Tuple<string, string, string>(pLiquidationGroup, pLiquidationGroupSplit, pRiskMeasureSet);
                if (m_RiskMeasureSetData.TryGetValue(key, out RiskDataPrismaRiskMeasureSet riskMeasureSetData))
                {
                    riskMeasureSetData.SetData(pInputSourceDataStyle, pParsingRow);
                }
                else
                {
                    riskMeasureSetData = new RiskDataPrismaRiskMeasureSet(pLiquidationGroup, pLiquidationGroupSplit, pRiskMeasureSet, pInputSourceDataStyle, pParsingRow);
                    m_RiskMeasureSetData.Add(key, riskMeasureSetData);
                }
            }
            finally
            {
                semaphoreRiskMeasureSet.Release();
            }
        }

        /// <summary>
        /// Récupération du "Record Type" de la ligne <paramref name="pLine"/> du fichier
        /// </summary>
        /// <param name="pLine">Une ligne du fichier</param>
        /// <returns></returns>
        private static string GetRecordType(string pLine)
        {
            return GetFirstElement(pLine);
        }

        /// <summary>
        /// Retourne les caractères présents entre le début de la ligne <paramref name="pLine"/> et le 1er ";" 
        /// </summary>
        /// <param name="pLine">Une ligne du fichier</param>
        /// <returns></returns>
        private static string GetFirstElement(string pLine)
        {
            string ret = default;
            if (pLine != default)
            {
                int index = pLine.IndexOf(";");
                if (false == index > 0)
                {
                    throw new Exception("Missing ';' char");
                }
                ret = pLine.Substring(0, index);
            }
            return ret;
        }
        #endregion private Methods
        #endregion Methods
    }
}
