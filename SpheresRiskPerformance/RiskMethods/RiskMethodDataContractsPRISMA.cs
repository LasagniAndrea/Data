using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using FixML.Enum;
using System;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Data container pour les paramètres Prisma de groupe de liquidation
    /// <para>Principale table de Spheres référencée: IMPRISMALG_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaLiquidationGroup>.DATASETROWNAME,
        Namespace = DataHelper<PrismaLiquidationGroup>.DATASETNAMESPACE)]
    internal sealed class PrismaLiquidationGroup
    {
        /// <summary>
        /// Id interne du groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALG_H", Order = 1)]
        public int IdLg { get; set; }

        /// <summary>
        /// Identifier du groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string Identifier { get; set; }

        /// <summary>
        /// Type de devise à utiliser
        /// </summary>
        [DataMember(Name = "CURRENCYTYPEFLAG", Order = 3)]
        public PrismaCurrencyTypeFlagEnum CurrencyTypeFlag { get; set; }
    }
    
    /// <summary>
    /// Data container pour les paramètres Prisma de split de groupe de liquidation
    /// <para>Principale table de Spheres référencée: IMPRISMALGS_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaLiquidationGroupSplit>.DATASETROWNAME,
        Namespace = DataHelper<PrismaLiquidationGroupSplit>.DATASETNAMESPACE)]
    internal sealed class PrismaLiquidationGroupSplit
    {
        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALGS_H", Order = 1)]
        public int IdLgs { get; set; }

        /// <summary>
        /// Identifier du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string Identifier { get; set; }

        /// <summary>
        /// Id interne du groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALG_H", Order = 3)]
        public int IdLg { get; set; }

        /// <summary>
        /// Identifier de la méthode risque
        /// </summary>
        [DataMember(Name = "RISKMETHOD", Order = 4)]
        public string RiskMethod { get; set; }

        /// <summary>
        /// Méthode d'aggregation
        /// </summary>
        [DataMember(Name = "AGGREGATIONMETHOD", Order = 5)]
        public PrismaAggregationMethod AggregationMethod { get; set; }

        /// <summary>
        /// Calculate TEA or not
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add IsCalculateTEA
        [DataMember(Name = "ISCALCULATETEA", Order = 6)]
        public bool IsCalculateTEA { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de Time to Expiry Adjustment d'un split de groupe de liquidation
    /// </summary>
    // PM 20180903 [24015] Prisma v8.0 : new class PrismaTimeToExpiryAdjustment 
    [DataContract(Name = DataHelper<PrismaRiskMeasureSet>.DATASETROWNAME,
        Namespace = DataHelper<PrismaRiskMeasureSet>.DATASETNAMESPACE)]
    internal sealed class PrismaTimeToExpiryAdjustment
    {
        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALGS_H", Order = 1)]
        public int IdLgs { get; set; }

        /// <summary>
        /// Id du TEA bucket
        /// </summary>
        [DataMember(Name = "TEABUCKETID", Order = 2)]
        public int TeaBucketId { get; set; }

        /// <summary>
        /// Hedging Weight
        /// </summary>
        [DataMember(Name = "HEDGINGWEIGHT", Order = 3)]
        public decimal HedgingWeight { get; set; }

        /// <summary>
        /// Directional Weight
        /// </summary>
        [DataMember(Name = "DIRECTIONALWEIGHT", Order = 4)]
        public decimal DirectionalWeight { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de jeu de mesure de risque d'un split de groupe de liquidation
    /// <para>Principale table de Spheres référencée: IMPRISMARMSLGS_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaRiskMeasureSet>.DATASETROWNAME,
        Namespace = DataHelper<PrismaRiskMeasureSet>.DATASETNAMESPACE)]
    internal sealed class PrismaRiskMeasureSet
    {
        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGS_H", Order = 1)]
        public int IdRmsLgs { get; set; }

        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALGS_H", Order = 2)]
        public int IdLgs { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMS_H", Order = 3)]
        public int IdRms { get; set; }

        /// <summary>
        /// Identifier du jeu de mesure de risque
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 4)]
        public string Identifier { get; set; }

        /// <summary>
        /// Méthode d'aggregation
        /// <para>Subsample aggregation method</para>
        /// </summary>
        [DataMember(Name = "AGGREGATIONMETHOD", Order = 5)]
        public PrismaAggregationMethod AggregationMethod { get; set; }

        /// <summary>
        /// Ajustement de correlation break maximum
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CorrelationBreakMaxDouble { get; set; }
        //public decimal CorrelationBreakMax
        //{ get { return (decimal)CorrelationBreakMaxDouble; } }
        [DataMember(Name = "CBCAP", Order = 6)]
        public double CorrelationBreakMaxDouble { get { return (double)CorrelationBreakMax; } set { CorrelationBreakMax = (decimal)value; } }
        public decimal CorrelationBreakMax { get; set; }

        /// <summary>
        /// Niveau de confiance utilisé pour le calcul du correlation break
        /// <para>Confidence Level used for the calculation of the correlation break in percernt</para>
        /// <para>0 if correlation break is not used</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CorrelationBreakConfidenceLevelDouble { get; set; }
        //public decimal CorrelationBreakConfidenceLevel
        //{ get { return (decimal)CorrelationBreakConfidenceLevelDouble; } }
        [DataMember(Name = "CBCONFIDENCELEVEL", Order = 7)]
        public double CorrelationBreakConfidenceLevelDouble { get { return (double)CorrelationBreakConfidenceLevel; } set { CorrelationBreakConfidenceLevel = (decimal)value; } }
        public decimal CorrelationBreakConfidenceLevel { get; set; }

        /// <summary>
        /// Ajustement de correlation break minimum
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CorrelationBreakMinDouble { get; set; }
        //public decimal CorrelationBreakMin
        //{ get { return (decimal)CorrelationBreakMinDouble; } }
        [DataMember(Name = "CBFLOOR", Order = 8)]
        public double CorrelationBreakMinDouble { get { return (double)CorrelationBreakMin; } set { CorrelationBreakMin = (decimal)value; } }
        public decimal CorrelationBreakMin { get; set; }

        /// <summary>
        /// Multiplier pour le calcul de l'ajustement de correlation break
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CorrelationBreakMultiplierDouble { get; set; }
        //public decimal CorrelationBreakMultiplier
        //{ get { return (decimal)CorrelationBreakMultiplierDouble; } }
        [DataMember(Name = "CBMULTIPLIER", Order = 9)]
        public double CorrelationBreakMultiplierDouble { get { return (double)CorrelationBreakMultiplier; } set { CorrelationBreakMultiplier = (decimal)value; } }
        public decimal CorrelationBreakMultiplier { get; set; }

        /// <summary>
        /// Taille de la sous fenêtre utilisée pour le calcul du correlation break 
        /// <para>Size of th sub-windows use used for the calculation of correlation break</para>
        /// <para>0 if correlation break is not used</para>
        /// </summary>
        [DataMember(Name = "CBSUBWINDOW", Order = 10)]
        public int CorrelationBreakSubWindow { get; set; }

        /// <summary>
        /// Niveau de confiance de la mesure de risque
        /// <para>Anchor Confidence Level</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double ConfidenceLevelDouble { get; set; }
        //public decimal ConfidenceLevel
        //{ get { return (decimal)ConfidenceLevelDouble; } }
        [DataMember(Name = "CONFIDENCELEVEL", Order = 11)]
        public double ConfidenceLevelDouble { get { return (double)ConfidenceLevel; } set { ConfidenceLevel = (decimal)value; } }
        public decimal ConfidenceLevel { get; set; }

        /// <summary>
        /// Niveau de confiance utilisé pour le calcul de la diversification factor alpha
        /// <para>Confidence level for the calculation of the diversification factor alpha, in percent</para>
        /// <para>0 if Liquidity Risk component is not used</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double AlphaConfidenceLevelDouble { get; set; }
        //public decimal AlphaConfidenceLevel
        //{ get { return (decimal)AlphaConfidenceLevelDouble; } }
        [DataMember(Name = "DFACONFIDENCELEVEL", Order = 12)]
        public double AlphaConfidenceLevelDouble { get { return (double)AlphaConfidenceLevel; } set { AlphaConfidenceLevel = (decimal)value; } }
        public decimal AlphaConfidenceLevel { get; set; }

        /// <summary>
        /// Minimum de diversification factor utilisé pour le calcul du composant de liquidité
        /// <para>Floor for the diversification factor used for the calculation of the liquidity risk component, in percent</para>
        /// <para>0 if liquidity Risk component is not used</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double AlphaFloorDouble { get; set; }
        //public decimal AlphaFloor
        //{ get { return (decimal)AlphaFloorDouble; } }
        [DataMember(Name = "DFAFLOOR", Order = 13)]
        public double AlphaFloorDouble { get { return (double)AlphaFloor; } set { AlphaFloor = (decimal)value; } }
        public decimal AlphaFloor { get; set; }

        /// <summary>
        /// Type du jeu de mesure de risque
        /// <para>H:Historical scenarios, F:Filtered historical scenarios, S:Stressed period scenarios</para>
        /// </summary>
        [DataMember(Name = "HISTORICALSTRESSED", Order = 14)]
        public PrismaHistoricalStressedEnum HistoricalStressed { get; set; }

        /// <summary>
        /// Ajout ou non de l'ajustement correlation break à la mesure de risque
        /// </summary>
        [DataMember(Name = "ISCORRELATIONBREAK", Order = 15)]
        public bool IsCorrelactionBreak { get; set; }

        /// <summary>
        /// Application ou non du composant de liquidité à la mesure de risque
        /// </summary>
        [DataMember(Name = "ISLIQUIDCOMPONENT", Order = 16)]
        public bool IsLiquidityComponent { get; set; }

        /// <summary>
        /// Utiliser ou non la robustesse pour la mesure de risque
        /// </summary>
        [DataMember(Name = "ISUSEROBUSTNESS", Order = 17)]
        public bool IsUseRobustness { get; set; }

        /// <summary>
        /// Type de mesure de risque
        /// </summary>
        [DataMember(Name = "RISKMEASURE", Order = 18)]
        public string RiskMeasureType { get; set; }

        /// <summary>
        /// Facteur d'échelle pour la robustesse
        /// <para>Scaling Factor</para>
        /// <para>Scaling Factor for robustness enhancement</para>
        /// <para>1 if robustness enhancement is not used</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double ScalingFactorDouble { get; set; }
        //public decimal ScalingFactor
        //{ get { return (decimal)ScalingFactorDouble; } }
        [DataMember(Name = "SCALINGFACTOR", Order = 19)]
        public double ScalingFactorDouble { get { return (double)ScalingFactor; } set { ScalingFactor = (decimal)value; } }
        public decimal ScalingFactor { get; set; }

        /// <summary>
        /// Facteur de pondération pour la mesure du risque agrégé
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double WeightingFactorDouble { get; set; }
        //public decimal WeightingFactor
        //{ get { return (decimal)WeightingFactorDouble; } }
        [DataMember(Name = "WEIGHTINGFACTOR", Order = 20)]
        public double WeightingFactorDouble { get { return (double)WeightingFactor; } set { WeightingFactor = (decimal)value; } }
        public decimal WeightingFactor { get; set; }

        /// <summary>
        /// Nombre de pires scénarios 
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout colonne NBWORSTSCENARIO
        [DataMember(Name = "NBWORSTSCENARIO", Order = 21)]
        public int NumberOfWorstScenarios { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de market capacity
    /// <para>Liquidity premiums (ou bid-ask spread) et market Capacities</para>
    /// <para>Principale table de Spheres référencée: IMPRISMAMKTCAPA_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaMarketCapacity>.DATASETROWNAME,
        Namespace = DataHelper<PrismaMarketCapacity>.DATASETNAMESPACE)]
    internal sealed class PrismaMarketCapacity
    {
        /// <summary>
        /// Id interne de market capacity
        /// </summary>
        [DataMember(Name = "IDIMPRISMAMKTCAPA_H", Order = 1)]
        public int IdMktCapa { get; set; }

        /// <summary>
        /// Dérivé du volume de négociation ou d'une valeure similaire
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double MarketCapacityDouble { get; set; }
        //public decimal MarketCapacity
        //{ get { return (decimal)MarketCapacityDouble; } }
        [DataMember(Name = "MARKETCAPACITY", Order = 2)]
        public double MarketCapacityDouble { get { return (double)MarketCapacity; } set { MarketCapacity = (decimal)value; } }
        public decimal MarketCapacity { get; set; }

        /// <summary>
        /// Time to expiry bucket Id
        /// </summary>
        [DataMember(Name = "TTEBUCKETID", Order = 3)]
        public string TimeToExpiryBucketId { get; set; }

        /// <summary>
        /// Moneyness bucket Id (vide pour les futures)
        /// </summary>
        [DataMember(Name = "MONEYNESSBUCKETID", Order = 4)]
        public string MoneynessBucketId { get; set; }

        /// <summary>
        /// Product Id
        /// </summary>
        [DataMember(Name = "PRODUCTID", Order = 5)]
        public string ProductId { get; set; }

        /// <summary>
        /// Type de contrat
        /// </summary>
        [DataMember(Name = "PRODUCTLINE", Order = 6)]
        public string ProductLine { get; set; }

        /// <summary>
        /// P ou C ou null
        /// </summary>
        [DataMember(Name = "PUTCALL", Order = 7)]
        public string PutCall { get; set; }
        public Nullable<PutOrCallEnum> PutOrCall
        {
            get
            {
                Nullable<PutOrCallEnum> retPutOrCall = null;
                if (StrFunc.IsFilled(PutCall))
                {
                    string @value = PutCall;
                    switch (@value)
                    {
                        case "P":
                            @value = "0";
                            break;
                        case "C":
                            @value = "1";
                            break;
                    }
                    retPutOrCall = (PutOrCallEnum)ReflectionTools.EnumParse(new PutOrCallEnum(), @value);
                }
                return retPutOrCall;
            }
        }

        /// <summary>
        /// Prime de liquidité, utilisé pour le calcul de la composante de liquidité
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double LiquidityPremiumDouble { get; set; }
        //public decimal LiquidityPremium
        //{ get { return (decimal)LiquidityPremiumDouble; } }
        [DataMember(Name = "LIQUIDITYPREMIUM", Order = 8)]
        public double LiquidityPremiumDouble { get { return (double)LiquidityPremium; } set { LiquidityPremium = (decimal)value; } }
        public decimal LiquidityPremium { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de facteur de liquidité
    /// <para>Principale table de Spheres référencée: IMPRISMALIQFACT_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaLiquidityFactor>.DATASETROWNAME,
        Namespace = DataHelper<PrismaLiquidityFactor>.DATASETNAMESPACE)]
    internal sealed class PrismaLiquidityFactor
    {
        /// <summary>
        /// Id interne de la class de liquidité
        /// </summary>
        [DataMember(Name = "IDIMPRISMALIQCLASS_H", Order = 1)]
        public int IdLiquidClass { get; set; }

        /// <summary>
        ///  Identifier de la class de liquidité
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string Identifier { get; set; }

        /// <summary>
        /// Pourcentage du seuil inférieur
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double PctMinThresholdDouble { get; set; }
        //public decimal PctMinThreshold
        //{ get { return (decimal)PctMinThresholdDouble; } }
        [DataMember(Name = "PCTMINTHRESHOLD", Order = 3)]
        public double PctMinThresholdDouble { get { return (double)PctMinThreshold; } set { PctMinThreshold = (decimal)value; } }
        public decimal PctMinThreshold { get; set; }

        /// <summary>
        /// Poucrentage du seuil supérieur
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public Nullable<double> PctMaxThresholdDouble { get; set; }
        //public Nullable<decimal> PctMaxThreshold
        //{ get { return (decimal?)PctMaxThresholdDouble; } }
        [DataMember(Name = "PCTMAXTHRESHOLD", Order = 4)]
        public Nullable<double> PctMaxThresholdDouble { get { return (double)PctMaxThreshold; } set { PctMaxThreshold = (decimal)value; } }
        public Nullable<decimal> PctMaxThreshold { get; set; }

        /// <summary>
        /// Facteur de liquidité pour le seuil inférieur
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double MinThresholdFactorDouble { get; set; }
        //public decimal MinThresholdFactor
        //{ get { return (decimal)MinThresholdFactorDouble; } }
        [DataMember(Name = "MINTHRESHOLDFACTOR", Order = 5)]
        public double MinThresholdFactorDouble { get { return (double)MinThresholdFactor; } set { MinThresholdFactor = (decimal)value; } }
        public decimal MinThresholdFactor { get; set; }

        /// <summary>
        /// Facteur de liquidité pour le seuil supérieur
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public Nullable<double> MaxThresholdFactorDouble { get; set; }
        //public Nullable<decimal> MaxThresholdFactor
        //{ get { return (decimal?)MaxThresholdFactorDouble; } }
        [DataMember(Name = "MAXTHRESHOLDFACTOR", Order = 6)]
        public Nullable<double> MaxThresholdFactorDouble { get { return (double)MaxThresholdFactor; } set { MaxThresholdFactor = (decimal)value; } }
        public Nullable<decimal> MaxThresholdFactor { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de taux de change
    /// <remarks>Principale table de Spheres référencée: IMPRISMAFXPAIR_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaExchangeRate>.DATASETROWNAME,
        Namespace = DataHelper<PrismaExchangeRate>.DATASETNAMESPACE)]
    internal sealed class PrismaExchangeRate
    {
        /// <summary>
        /// Id interne du jeu taux de change
        /// </summary>
        [DataMember(Name = "IDIMPRISMAFX_H", Order = 1)]
        public int IdFx { get; set; }

        /// <summary>
        ///  Identifier du jeu taux de change
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 2)]
        public string Identifier { get; set; }

        /// <summary>
        /// Id interne du couple de devise
        /// </summary>
        [DataMember(Name = "IDIMPRISMAFXPAIR_H", Order = 3)]
        public int IdFxPair { get; set; }

        /// <summary>
        ///  Identifier du couple de devise
        /// </summary>
        [DataMember(Name = "CURRENCYPAIR", Order = 4)]
        public string CurrencyPair { get; set; }

        /// <summary>
        /// Taux de change
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double ExchangeRateDouble { get; set; }
        //public decimal ExchangeRate
        //{ get { return (decimal)ExchangeRateDouble; } }
        [DataMember(Name = "EXCHANGERATE", Order = 5)]
        public double ExchangeRateDouble { get { return (double)ExchangeRate; } set { ExchangeRate = (decimal)value; } }
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// Source Currency à partir de CurrencyPair
        /// </summary>
        public string SourceCurrency
        {
            get
            {
                return (StrFunc.IsFilled(CurrencyPair) && (CurrencyPair.Length == 6) ? CurrencyPair.Substring(0, 3) : "");
            }
        }
        /// <summary>
        /// Clearing Currency à partir de CurrencyPair
        /// </summary>
        public string ClearingCurrency
        {
            get
            {
                return (StrFunc.IsFilled(CurrencyPair) && (CurrencyPair.Length == 6) ? CurrencyPair.Substring(3, 3) : "");
            }
        }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de taux de change des jeux de mesure de risque
    /// <remarks>Principale table de Spheres référencée: IMPRISMAFXRMS_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaExchangeRateRMS>.DATASETROWNAME,
        Namespace = DataHelper<PrismaExchangeRateRMS>.DATASETNAMESPACE)]
    internal sealed class PrismaExchangeRateRMS
    {
        /// <summary>
        /// Id interne du couple de devise
        /// </summary>
        [DataMember(Name = "IDIMPRISMAFXPAIR_H", Order = 1)]
        public int IdFxPair { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque 
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMS_H", Order = 2)]
        public int IdRms { get; set; }

        /// <summary>
        /// Numéro du scénario de risque
        /// </summary>
        [DataMember(Name = "SCENARIONO", Order = 3)]
        public int ScenarioNumber { get; set; }

        /// <summary>
        /// Taux de change
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double ExchangeRateDouble { get; set; }
        //public decimal ExchangeRate
        //{ get { return (decimal)ExchangeRateDouble; } }
        [DataMember(Name = "VALUE", Order = 5)]
        public double ExchangeRateDouble { get { return (double)ExchangeRate; } set { ExchangeRate = (decimal)value; } }
        public decimal ExchangeRate { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma des series
    /// <para>Principale table de Spheres référencée: IMPRISMAS_H </para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAsset>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAsset>.DATASETNAMESPACE)]
    internal sealed class PrismaAsset
    {
        /// <summary>
        /// Id interne du product
        /// </summary>
        [DataMember(Name = "IDIMPRISMAP_H", Order = 1)]
        public int IdProduct { get; set; }

        /// <summary>
        /// Id interne de l'expiration
        /// </summary>
        [DataMember(Name = "IDIMPRISMAE_H", Order = 2)]
        public int IdExpiration { get; set; }

        /// <summary>
        /// Id interne de la serie
        /// </summary>
        [DataMember(Name = "IDIMPRISMAS_H", Order = 3)]
        public int IdSerie { get; set; }

        /// <summary>
        /// Product
        /// </summary>
        [DataMember(Name = "PRODUCTID", Order = 4)]
        public string Product { get; set; }

        /// <summary>
        /// Tick size
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double TickSizeDouble { get; set; }
        //public decimal TickSize
        //{ get { return (decimal)TickSizeDouble; } }
        [DataMember(Name = "TICKSIZE", Order = 5)]
        public double TickSizeDouble { get { return (double)TickSize; } set { TickSize = (decimal)value; } }
        public decimal TickSize { get; set; }

        /// <summary>
        /// Tick Value
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double TickValueDouble { get; set; }
        //public decimal TickValue
        //{ get { return (decimal)TickValueDouble; } }
        [DataMember(Name = "TICKVALUE", Order = 6)]
        public double TickValueDouble { get { return (double)TickValue; } set { TickValue = (decimal)value; } }
        public decimal TickValue { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 7)]
        public string Currency  { get; set; }

        /// <summary>
        /// Id interne de la class de liquidité
        /// </summary>
        [DataMember(Name = "IDIMPRISMALIQCLASS_H", Order = 8)]
        public int IdLiquidClass { get; set; }

        /// <summary>
        /// Id interne du liquidation group
        /// </summary>
        [DataMember(Name = "IDIMPRISMALG_H", Order = 9)]
        public int IdLg  { get; set; }

        /// <summary>
        /// Margin Style
        /// </summary>
        [DataMember(Name = "MARGINSTYLE", Order = 10)]
        public PrismaMarginStyleEnum MarginStyle { get; set; }

        /// <summary>
        /// Contract Year
        /// </summary>
        [DataMember(Name = "CYEAR", Order = 11)]
        public int ContractYear { get; set; }

        /// <summary>
        /// Contract Month
        /// </summary>
        [DataMember(Name = "CMONTH", Order = 12)]
        public int ContractMonth  { get; set; }

        /// <summary>
        /// Expiration Year
        /// </summary>
        [DataMember(Name = "YEAR", Order = 13)]
        public int ExpiryYear { get; set; }

        /// <summary>
        /// Expiration Month
        /// </summary>
        [DataMember(Name = "MONTH", Order = 14)]
        public int ExpiryMonth { get; set; }

        /// <summary>
        /// Expiration Day
        /// </summary>
        [DataMember(Name = "DAY", Order = 15)]
        public int ExpiryDay { get; set; }

        /// <summary>
        /// Days To Expiry
        /// </summary>
        [DataMember(Name = "DAYTOEXPIRY", Order = 16)]
        public int DaysToExpiry  { get; set; }

        /// <summary>
        /// Cross margining maturity bucket id
        /// </summary>
        [DataMember(Name = "XMMATBUCKETID", Order = 17)]
        public int CrossMaturityBucketId { get; set; }

        /// <summary>
        /// Days To Expiry Business
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add DaysToExpiryBusiness
        [DataMember(Name = "DAYTOEXPIRYBUSINESS", Order = 18)]
        public int DaysToExpiryBusiness { get; set; }

        /// <summary>
        /// P ou C ou null
        /// </summary>
        /// FI 20140225 [XXXXX] Spheres® autorise la présence des valeurs (P,C,0,1) dans la colonne IMPRISMAS_H.PUTCALL
        [DataMember(Name = "PUTCALL", Order = 19)]
        public string PutCall { get; set; }
        public Nullable<PutOrCallEnum> PutOrCall
        {
            get
            {
                Nullable<PutOrCallEnum> retPutOrCall = null;
                if (StrFunc.IsFilled(PutCall))
                {
                    string @value = PutCall;
                    switch (@value)
                    {
                        case "P":
                            @value = "0";
                            break;
                        case "C":
                            @value = "1";
                            break;
                    }
                    retPutOrCall = (PutOrCallEnum)ReflectionTools.EnumParse(new PutOrCallEnum(), @value);
                }
                return retPutOrCall;
            }
        }

        /// <summary>
        /// Strike price
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double StrikePriceDouble { get; set; }
        //public decimal StrikePrice
        //{ get { return (decimal)StrikePriceDouble; } }
        [DataMember(Name = "STRIKEPRICE", Order = 20)]
        public double StrikePriceDouble { get { return (double)StrikePrice; } set { StrikePrice = (decimal)value; } }
        public decimal StrikePrice { get; set; }

        /// <summary>
        /// Version du contrat
        /// </summary>
        [DataMember(Name = "VERSION", Order = 21)]
        public string Version { get; set; }

        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 22)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Asset Category
        /// </summary>
        [DataMember(Name = "ASSETCATEGORY", Order = 23)]
        public string AssetCategory { get; set; }

        /// <summary>
        /// Neutral Price
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double NeutralPriceDouble { get; set; }
        //public decimal NeutralPrice
        //{ get { return (decimal)NeutralPriceDouble; } }
        [DataMember(Name = "NPRICE", Order = 24)]
        public double NeutralPriceDouble { get { return (double)NeutralPrice; } set { NeutralPrice = (decimal)value; } }
        public decimal NeutralPrice { get; set; }

        /// <summary>
        /// Time to expiry bucket Id
        /// </summary>
        [DataMember(Name = "TTEBUCKETID", Order = 25)]
        public string TimeToExpiryBucketId { get; set; }

        /// <summary>
        /// Moneyness bucket Id (vide pour les futures)
        /// </summary>
        [DataMember(Name = "MONEYNESSBUCKETID", Order = 26)]
        public string MoneynessBucketId { get; set; }

        /// <summary>
        /// Risk Bucket
        /// </summary>
        [DataMember(Name = "RISKBUCKET", Order = 27)]
        public string RiskBucket { get; set; }

        /// <summary>
        /// Trade unit value (Multiplier)
        /// <para>TUV measures the size of one contract</para>
        /// <para>Tick Value/Tick Size * Trade Unit</para>
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double MultiplierDouble { get; set; }
        //public decimal Multiplier
        //{ get { return (decimal)MultiplierDouble; } }
        [DataMember(Name = "TUV", Order = 28)]
        public double MultiplierDouble { get { return (double)Multiplier; } set { Multiplier = (decimal)value; } }
        public decimal Multiplier { get; set; }

        /// <summary>
        /// Trade unit
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double ContractSizeDouble { get; set; }
        //public decimal ContractSize
        //{ get { return (decimal)ContractSizeDouble; } }
        [DataMember(Name = "TU", Order = 29)]
        public double ContractSizeDouble { get { return (double)ContractSize; } set { ContractSize = (decimal)value; } }
        public decimal ContractSize { get; set; }

        /// <summary>
        /// Settlement Price
        /// </summary>
        //PM 20140618 [19911] Ajouté pour Prisma Release 2.0
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double SettlementPriceDouble { get; set; }
        //public decimal SettlementPrice
        //{ get { return (decimal)SettlementPriceDouble; } }
        [DataMember(Name = "STLPRICE", Order = 30)]
        public double SettlementPriceDouble { get { return (double)SettlementPrice; } set { SettlementPrice = (decimal)value; } }
        public decimal SettlementPrice { get; set; }

        /// <summary>
        /// PV Reference Price
        /// </summary>
        //PM 20140612 [19911] Ajouté pour Prisma Release 2.0
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double PVReferencePriceDouble { get; set; }
        //public decimal PVReferencePrice
        //{ get { return (decimal)PVReferencePriceDouble; } }
        [DataMember(Name = "PVPRICE", Order = 31)]
        public double PVReferencePriceDouble { get { return (double)PVReferencePrice; } set { PVReferencePrice = (decimal)value; } }
        public decimal PVReferencePrice { get; set; }

        /// <summary>
        /// Settlement Type
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add SettlementType
        [DataMember(Name = "SETTLTTYPE", Order = 32)]
        public string SettlementType { get; set; }

        /// <summary>
        /// Settlement Method
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add SettlementMethod
        [DataMember(Name = "SETTLTMETHOD", Order = 33)]
        public SettlMethodEnum SettlementMethod { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma des splits de groupe de liquidation des assets
    /// <para>Principale table de Spheres référencée: IMPRISMALGSS_H</para>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAssetLiquidGroupSplit>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAssetLiquidGroupSplit>.DATASETNAMESPACE)]
    internal sealed class PrismaAssetLiquidGroupSplit
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Id interne de la serie
        /// </summary>
        [DataMember(Name = "IDIMPRISMAS_H", Order = 2)]
        public int IdSerie { get; set; }

        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMALGS_H", Order = 3)]
        public int IdLgs { get; set; }

        /// <summary>
        /// Liquidation Horizon
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ne plus utiliser la colonne LH de la table IMPRISMALGSS_H, LiquidationHorizon devient obsolète à ce niveau
        //[DataMember(Name = "LH", Order = 4)]
        //public int LiquidationHorizon { get; set; }

        /// <summary>
        /// Split de groupe de liquidation par défaut ou non
        /// </summary>
        [DataMember(Name = "ISDEFAULT", Order = 4)]
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma des jeux de mesure de risque des splits de groupe de liquidation des assets
    /// <remarks>Principale table de Spheres référencée: IMPRISMARMSLGSS_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAssetRMSLGS>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAssetRMSLGS>.DATASETNAMESPACE)]
    internal sealed class PrismaAssetRMSLGS
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation de l'asset
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGSS_H", Order = 2)]
        public int IdAssetRmsLgs { get; set; }

        /// <summary>
        /// Id interne de la serie
        /// </summary>
        [DataMember(Name = "IDIMPRISMAS_H", Order = 3)]
        public int IdSerie { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGS_H", Order = 4)]
        public int IdRmsLgs { get; set; }

        /// <summary>
        /// Id interne du jeu taux de change
        /// </summary>
        [DataMember(Name = "IDIMPRISMAFX_H", Order = 5)]
        public int IdFx { get; set; }

        /// <summary>
        /// Liquidation Horizon
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout colonne LH
        [DataMember(Name = "LH", Order = 6)]
        public int LiquidationHorizon { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de prix des assets
    /// <remarks>Principale table de Spheres référencée: IMPRISMASPS_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAssetPrice>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAssetPrice>.DATASETNAMESPACE)]
    internal sealed class PrismaAssetPrice
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation de l'asset
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGSS_H", Order = 2)]
        public int IdAssetRmsLgs { get; set; }

        /// <summary>
        /// Numéro du scénario de risque
        /// </summary>
        [DataMember(Name = "SCENARIONO", Order = 3)]
        public int ScenarioNumber { get; set; }

        /// <summary>
        /// Prix du scénario (ScenarioNumber - 1) * LiquidationHorizon + 1
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double Price1Double { get; set; }
        //public decimal Price1
        //{ get { return (decimal)Price1Double; } }
        [DataMember(Name = "PRICE1", Order = 4)]
        public double Price1Double { get { return (double)Price1; } set { Price1 = (decimal)value; } }
        public decimal Price1 { get; set; }

        /// <summary>
        /// Prix du scénario (ScenarioNumber - 1) * LiquidationHorizon + 2
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double Price2Double { get; set; }
        //public decimal Price2
        //{ get { return (decimal)Price2Double; } }
        [DataMember(Name = "PRICE2", Order = 5)]
        public double Price2Double { get { return (double)Price2; } set { Price2 = (decimal)value; } }
        public decimal Price2 { get; set; }

        /// <summary>
        /// Prix du scénario (ScenarioNumber - 1) * LiquidationHorizon + 3
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double Price3Double { get; set; }
        //public decimal Price3
        //{ get { return (decimal)Price3Double; } }
        [DataMember(Name = "PRICE3", Order = 6)]
        public double Price3Double { get { return (double)Price3; } set { Price3 = (decimal)value; } }
        public decimal Price3 { get; set; }

        /// <summary>
        /// Prix du scénario (ScenarioNumber - 1) * LiquidationHorizon + 4
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double Price4Double { get; set; }
        //public decimal Price4
        //{ get { return (decimal)Price4Double; } }
        [DataMember(Name = "PRICE4", Order = 7)]
        public double Price4Double { get { return (double)Price4; } set { Price4 = (decimal)value; } }
        public decimal Price4 { get; set; }

        /// <summary>
        /// Prix du scénario (ScenarioNumber - 1) * LiquidationHorizon + 5
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double Price5Double { get; set; }
        //public decimal Price5
        //{ get { return (decimal)Price5Double; } }
        [DataMember(Name = "PRICE5", Order = 8)]
        public double Price5Double { get { return (double)Price5; } set { Price5 = (decimal)value; } }
        public decimal Price5 { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de compression error des assets
    /// <remarks>Principale table de Spheres référencée: IMPRISMACES_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAssetCompressionError>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAssetCompressionError>.DATASETNAMESPACE)]
    internal sealed class PrismaAssetCompressionError
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation de l'asset
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGSS_H", Order = 2)]
        public int IdAssetRmsLgs { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 3)]
        public string Currency { get; set; }

        /// <summary>
        /// Compression error 1
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CE1Double { get; set; }
        //public decimal CE1
        //{ get { return (decimal)CE1Double; } }
        [DataMember(Name = "CE1", Order = 4)]
        public double CE1Double { get { return (double)CE1; } set { CE1 = (decimal)value; } }
        public decimal CE1 { get; set; }

        /// <summary>
        /// Compression error 2
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CE2Double { get; set; }
        //public decimal CE2
        //{ get { return (decimal)CE2Double; } }
        [DataMember(Name = "CE2", Order = 5)]
        public double CE2Double { get { return (double)CE2; } set { CE2 = (decimal)value; } }
        public decimal CE2 { get; set; }

        /// <summary>
        /// Compression error 3
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CE3Double { get; set; }
        //public decimal CE3
        //{ get { return (decimal)CE3Double; } }
        [DataMember(Name = "CE3", Order = 6)]
        public double CE3Double { get { return (double)CE3; } set { CE3 = (decimal)value; } }
        public decimal CE3 { get; set; }

        /// <summary>
        /// Compression error 4
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CE4Double { get; set; }
        //public decimal CE4
        //{ get { return (decimal)CE4Double; } }
        [DataMember(Name = "CE4", Order = 7)]
        public double CE4Double { get { return (double)CE4; } set { CE4 = (decimal)value; } }
        public decimal CE4 { get; set; }

        /// <summary>
        /// Compression error 5
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double CE5Double { get; set; }
        //public decimal CE5
        //{ get { return (decimal)CE5Double; } }
        [DataMember(Name = "CE5", Order = 8)]
        public double CE5Double { get { return (double)CE5; } set { CE5 = (decimal)value; } }
        public decimal CE5 { get; set; }
    }

    /// <summary>
    /// Data container pour les paramètres Prisma de la Value at Risk des assets
    /// <remarks>Principale table de Spheres référencée: IMPRISMAVARS_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<PrismaAssetVaR>.DATASETROWNAME,
        Namespace = DataHelper<PrismaAssetVaR>.DATASETNAMESPACE)]
    internal sealed class PrismaAssetVaR
    {
        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        [DataMember(Name = "IDASSET", Order = 1)]
        public int IdAsset { get; set; }

        /// <summary>
        /// Id interne de la serie
        /// </summary>
        [DataMember(Name = "IDIMPRISMAS_H", Order = 2)]
        public int IdSerie { get; set; }

        /// <summary>
        /// Id interne du jeu de mesure de risque du split de groupe de liquidation de l'asset
        /// </summary>
        [DataMember(Name = "IDIMPRISMARMSLGSS_H", Order = 3)]
        public int IdAssetRmsLgs { get; set; }

        /// <summary>
        /// Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 4)]
        public string Currency { get; set; }

        /// <summary>
        /// S ou L : Indique sir la VaR concerne une position long ou short
        /// </summary>
        [DataMember(Name = "SHORTLONG", Order = 5)]
        public string ShortLong { get; set; }

        /// <summary>
        /// VaR Type : IVAR / AIVAR
        /// </summary>
        [DataMember(Name = "VARTYPE", Order = 6)]
        public string VaRType { get; set; }

        /// <summary>
        /// VaR Amount
        /// </summary>
        // PM 20180219 [23824] Inversion accesseur (decimal devient maitre)
        //public double VaRAmountDouble { get; set; }
        //public decimal VaRAmount
        //{ get { return (decimal)VaRAmountDouble; } }
        [DataMember(Name = "VARAMOUNT", Order = 7)]
        public double VaRAmountDouble { get { return (double)VaRAmount; } set { VaRAmount = (decimal)value; } }
        public decimal VaRAmount { get; set; }
    }
}
