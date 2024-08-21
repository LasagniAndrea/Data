using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using FixML.Enum;
using System;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{

    #region SPAN method

    /// <summary>
    /// Data container for the Clearing House Span specific parameters.
    /// </summary>
    /// <remarks>Main Spheres reference table: CSS</remarks>
    // PM 20160404 [22116] Classe qui n'est plus utilisé
    //[DataContract(Name = DataHelper<SpanClearingOrganization>.DATASETROWNAME,
    //    Namespace = DataHelper<SpanClearingOrganization>.DATASETNAMESPACE)]
    //internal sealed class SpanClearingOrganization
    //{
    //    /// <summary>
    //    /// Clearing House internal id
    //    /// </summary>
    //    [DataMember(Name = "IDA", Order = 1)]
    //    public int CSSId
    //    { get; set; }

    //    /// <summary>
    //    /// Gestion des spreads inter group de contrat
    //    /// </summary>
    //    // PM 20160404 [22116] N'est plus utilisé
    //    //[DataMember(Name = "ISIMINTERCOMSPRD", Order = 2)]
    //    //public bool IsInterCommoditySpreading
    //    //{ get; set; }

    //    /// <summary>
    //    /// Methode de calcul du risque pondéré de prix
    //    /// </summary>
    //    // PM 20160404 [22116] N'est plus utilisé
    //    //[DataMember(Name = "IMWEIGHTEDRISKMETH", Order = 3)]
    //    //public string WeightedRiskMethod
    //    //{ get; set; }

    //    /// <summary>
    //    /// Precision de l'arrondie dans le calcul
    //    /// </summary>
    //    // PM 20160404 [22116] N'est plus utilisé
    //    //[DataMember(Name = "IMROUNDINGPREC", Order = 4)]
    //    //public int RoundingPrecision
    //    //{ get; set; }

    //    /// <summary>
    //    /// Direction de l'arrondie dans le calcul
    //    /// </summary>
    //    // PM 20160404 [22116] N'est plus utilisé
    //    //[DataMember(Name = "IMROUNDINGDIR", Order = 5)]
    //    //public string RoundingDirection
    //    //{ get; set; }

    //    /// <summary>
    //    /// Css currency
    //    /// </summary>
    //    // PM 20160404 [22116] Changement d'Order
    //    //[DataMember(Name = "IDC", Order = 6)]
    //    [DataMember(Name = "IDC", Order = 2)]
    //    public string Currency
    //    { get; set; }

    //    /// <summary>
    //    /// SPAN Scan Risk Offset Cap Percentage's
    //    /// </summary>
    //    // PM 20160404 [22116] Non utilisé
    //    // PM 20150930 [21134] Add SCANOFFSETCAPPCT
    //    //[DataMember(Name = "SCANOFFSETCAPPCT", Order = 7)]
    //    //public double ScanRiskOffsetCapPrctDouble
    //    //{ get; set; }
    //    //public decimal ScanRiskOffsetCapPrct
    //    //{ get { return (decimal)ScanRiskOffsetCapPrctDouble; } }
    //}

    /// <summary>
    /// Data container for the Derivative Contract parameters for SPAN.
    /// </summary>
    /// <remarks>Main Spheres reference table: DERIVATIVECONTRACT et IMSPANCONTRACT_H</remarks>
    [DataContract(Name = DataHelper<SpanDerivativeContract>.DATASETROWNAME,
        Namespace = DataHelper<SpanDerivativeContract>.DATASETNAMESPACE)]
    internal sealed class SpanDerivativeContract
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
        /// Category od the current derivative contract (IDDC), F for Futures, O for Options.
        /// </summary>
        [DataMember(Name = "CATEGORY", Order = 3)]
        public string Category
        { get; set; }

        /// <summary>
        /// Contract multiplier of the DERIVATIVECONTRACT table
        /// </summary>
        [DataMember(Name = "CONTRACTMULTIPLIER", Order = 4)]
        public double ContractMultiplierDouble
        { get; set; }
        public decimal ContractMultiplier
        { get { return (decimal)ContractMultiplierDouble; } }

        /// <summary>
        /// Category of the underlying asset
        /// </summary>
        /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag167.html</remarks>
        [DataMember(Name = "ASSETCATEGORY", Order = 5)]
        public Cst.UnderlyingAsset UnlCategory
        { get; set; }

        /// <summary>
        /// Underlying asset internal id (null when UnlCategory = Future)
        /// </summary>
        [DataMember(Name = "IDASSET_UNL", Order = 6)]
        public int UnlAssetId
        { get; set; }

        /// <summary>
        /// Underlying Future contract id (not null only when UnlCategory = Future)
        /// </summary>
        [DataMember(Name = "IDDC_UNL", Order = 7)]
        public int UnlContractId
        { get; set; }

        /// <summary>
        /// Contract currency
        /// </summary>
        [DataMember(Name = "IDC_PRICE", Order = 8)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Contract internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCONTRACT_H", Order = 9)]
        public int SPANContractId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGCONTRACT_H", Order = 10)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Volatility Scan Range Quotation Method
        /// </summary>
        [DataMember(Name = "VOLATSCANQUOTEMETHOD", Order = 11)]
        public string VolatilityScanRangeQuotationMethod
        { get; set; }

        /// <summary>
        /// Price Scan Range Quotation Method
        /// </summary>
        [DataMember(Name = "PRICESCANQUOTEMETHOD", Order = 12)]
        public string PriceScanRangeQuotationMethod
        { get; set; }

        /// <summary>
        /// Price Scan Range Valuation Type
        /// </summary>
        [DataMember(Name = "PRICESCANVALTYPE", Order = 13)]
        public string PriceScanRangeValuationType
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Currency parameters
    /// <remarks>Main Spheres reference table: IMSPANCURRENCY_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanCurrency>.DATASETROWNAME,
        Namespace = DataHelper<SpanCurrency>.DATASETNAMESPACE)]
    internal sealed class SpanCurrency
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// SPAN Currency internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCURRENCY_H", Order = 2)]
        public int SPANCurrencyId
        { get; set; }

        /// <summary>
        /// Currency Code
        /// </summary>
        [DataMember(Name = "IDC", Order = 3)]
        public string Currency
        { get; set; }

        /// <summary>
        /// Currency Exponent
        /// </summary>
        [DataMember(Name = "EXPONENT", Order = 4)]
        public int Exponent
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Currency Conversion parameters
    /// <remarks>Main Spheres reference table: IMSPANCURCONV_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanCurConv>.DATASETROWNAME,
        Namespace = DataHelper<SpanCurConv>.DATASETNAMESPACE)]
    internal sealed class SpanCurConv
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// SPAN Currency Conversion internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCURCONV_H", Order = 2)]
        public int SPANCurrencyConversionId
        { get; set; }

        /// <summary>
        /// Contract Currency Code
        /// </summary>
        [DataMember(Name = "IDC_CONTRACT", Order = 3)]
        public string ContractCurrency
        { get; set; }

        /// <summary>
        /// Margin Currency Code
        /// </summary>
        [DataMember(Name = "IDC_MARGIN", Order = 4)]
        public string MarginCurrency
        { get; set; }

        /// <summary>
        /// Contract / Margin Currency multiplier (FX Rate)
        /// </summary>
        ///PM 20150902 [21385] decimal / double
        [DataMember(Name = "VALUE", Order = 5)]
        public double FXRateDouble
        { get; set; }
        public decimal FXRate
        { get { return (decimal)FXRateDouble; } }

        /// <summary>
        /// Percentage FX Shift Up
        /// </summary>
        ///PM 20150902 [21385] decimal / double
        [DataMember(Name = "SHIFTUP", Order = 6)]
        public double ShiftUpDouble
        { get; set; }
        public decimal ShiftUp
        { get { return (decimal)ShiftUpDouble; } }

        /// <summary>
        /// Percentage FX Shift Down
        /// </summary>
        ///PM 20150902 [21385] decimal / double
        [DataMember(Name = "SHIFTDOWN", Order = 7)]
        public double ShiftDownDouble
        { get; set; }
        public decimal ShiftDown
        { get { return (decimal)ShiftDownDouble; } }
    }

    /// <summary>
    /// Data container for the SPAN Inter Contract Group Spread parameters
    /// <remarks>Main Spheres reference table: IMSPANINTERSPR_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanInterSpread>.DATASETROWNAME,
        Namespace = DataHelper<SpanInterSpread>.DATASETNAMESPACE)]
    internal sealed class SpanInterSpread
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// SPAN Inter Spread internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANINTERSPR_H", Order = 2)]
        public int SPANInterSpreadId
        { get; set; }

        /// <summary>
        /// Combined Group Code
        /// </summary>
        [DataMember(Name = "COMBINEDGROUPCODE", Order = 3)]
        public string CombinedGroupCode
        { get; set; }

        /// <summary>
        /// Spread Priority
        /// </summary>
        [DataMember(Name = "SPREADPRIORITY", Order = 4)]
        public int SpreadPriority
        { get; set; }

        /// <summary>
        /// Inter Spread Method
        /// </summary>
        [DataMember(Name = "INTERSPREADMETHOD", Order = 5)]
        public string InterSpreadMethod
        { get; set; }

        /// <summary>
        /// Credit Rate
        /// </summary>
        [DataMember(Name = "CREDITRATE", Order = 6)]
        public double CreditRateDouble
        { get; set; }
        public decimal CreditRate
        { get { return (decimal)CreditRateDouble; } }

        /// <summary>
        /// Spread Group Type
        /// </summary>
        [DataMember(Name = "SPREADGROUPTYPE", Order = 7)]
        public string SpreadGroupType
        { get; set; }

        /// <summary>
        /// Credit Calculation Method
        /// </summary>
        [DataMember(Name = "CREDITCALCMETHOD", Order = 8)]
        public string CreditCalculationMethod
        { get; set; }

        /// <summary>
        /// Indicateur de Credit Rate par Contract Group
        /// </summary>
        [DataMember(Name = "ISCDTRATESEPARATED", Order = 9)]
        public bool IsCreditRateSeparated
        { get; set; }

        /// <summary>
        /// Eligibility Code
        /// </summary>
        [DataMember(Name = "ELIGIBILITYCODE", Order = 10)]
        public string EligibilityCode
        { get; set; }

        /// <summary>
        /// Minimum Number Of Leg
        /// </summary>
        [DataMember(Name = "MINNUMBEROFLEG", Order = 11)]
        public int MinimumNumberOfLeg
        { get; set; }

        /// <summary>
        /// Offset Rate
        /// </summary>
        [DataMember(Name = "OFFSETRATE", Order = 12)]
        public double OffsetRateDouble
        { get; set; }
        public decimal OffsetRate
        { get { return (decimal)OffsetRateDouble; } }

        /// <summary>
        /// Number Of Leg (Available only for ICE [London SPAN])
        /// </summary>
        ///PM 20151224 [POC -MUREX] Add NUMBEROFLEG
        [DataMember(Name = "NUMBEROFLEG", Order = 13)]
        public int NumberOfLeg
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Inter Contract Group Spread Leg parameters
    /// <remarks>Main Spheres reference table: IMSPANINTERLEG_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanInterLeg>.DATASETROWNAME,
        Namespace = DataHelper<SpanInterLeg>.DATASETNAMESPACE)]
    internal sealed class SpanInterLeg
    {
        /// <summary>
        /// SPAN Inter Spread internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANINTERSPR_H", Order = 1)]
        public int SPANInterSpreadId
        { get; set; }

        /// <summary>
        /// SPAN Inter Spread Leg Number
        /// </summary>
        [DataMember(Name = "LEGNUMBER", Order = 2)]
        public int LegNumber
        { get; set; }

        /// <summary>
        /// Target Leg Indicator
        /// </summary>
        [DataMember(Name = "ISTARGET", Order = 3)]
        public bool IsTargetLeg
        { get; set; }

        /// <summary>
        /// Echange Acronym
        /// </summary>
        [DataMember(Name = "EXCHANGEACRONYM", Order = 4)]
        public string EchangeAcronym
        { get; set; }

        /// <summary>
        /// Contract Group Code
        /// </summary>
        [DataMember(Name = "COMBCOMCODE", Order = 5)]
        public string ContractGroupCode
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 6)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Number of Delta Per Spread
        /// </summary>
        [DataMember(Name = "DELTAPERSPREAD", Order = 7)]
        public double DeltaPerSpreadDouble
        { get; set; }
        public decimal DeltaPerSpread
        { get { return (decimal)DeltaPerSpreadDouble; } }

        /// <summary>
        /// Leg Side
        /// </summary>
        [DataMember(Name = "LEGSIDE", Order = 8)]
        public string LegSide
        { get; set; }

        /// <summary>
        /// Leg Required Indicator
        /// </summary>
        [DataMember(Name = "ISREQUIRED", Order = 9)]
        public bool? IsRequiredNullable
        { get; set; }
        public bool IsRequired
        { get { return IsRequiredNullable == null || (bool)IsRequiredNullable; } }

        /// <summary>
        /// Credit Rate
        /// </summary>
        [DataMember(Name = "CREDITRATE", Order = 10)]
        public double CreditRateDouble
        { get; set; }
        public decimal CreditRate
        { get { return (decimal)CreditRateDouble; } }

        /// <summary>
        /// SPAN Contract Group Tier internal id
        /// </summary>
        // PM 201900201 [24482][24507] Ajout prise en compte des valeurs null lorsque l'échéance est renseignée :
        // Les jambes des spreads intercommodity ayant une échéance de renseignée doivent avoir une valeur de IDIMSPANTIER_H non null.
        // Si l'échéance est renseigné et que IDIMSPANTIER_H est null, il ne faut pas valoriser SPANTierId à 0 (signifiant généralement toute échéance) mais à -1 pour ne faire référence à rien.
        //public int SPANTierId
        //{ get; set; }
        [DataMember(Name = "IDIMSPANTIER_H", Order = 11)]
        public int? SPANTierIdRead
        { get; set; }

        public int SPANTierId
        {
            get
            {
                return SPANTierIdRead ?? (StrFunc.IsFilled(MaturityMonthYear) ? -1 : 0);
            }
        }

        /// <summary>
        /// SPAN Contract Group Inter Spread Maturity
        /// </summary>
        /// PM 20150707 [21104] Ajout colonne MaturityMonthYear
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 12)]
        public string MaturityMonthYear
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Exchange Complex parameters
    /// <remarks>Main Spheres reference table: IMSPAN_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanExchangeComplex>.DATASETROWNAME,
        Namespace = DataHelper<SpanExchangeComplex>.DATASETNAMESPACE)]
    internal sealed class SpanExchangeComplex
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// Exchange Complex
        /// </summary>
        [DataMember(Name = "EXCHANGECOMPLEX", Order = 2)]
        public string ExchangeComplex
        { get; set; }

        /// <summary>
        /// Settlement Session
        /// </summary>
        [DataMember(Name = "SETTLEMENTSESSION", Order = 3)]
        public string SettlementSession
        { get; set; }

        /// <summary>
        /// Is Option Value Limit
        /// </summary>
        [DataMember(Name = "ISOPTIONVALUELIMIT", Order = 4)]
        public bool IsOptionValueLimit
        { get; set; }

        /// <summary>
        /// Business Date and Time
        /// </summary>
        ///PM 20150902 [21385] Added
        [DataMember(Name = "DTBUSINESSTIME", Order = 5)]
        public DateTime DtBusinessTime
        { get; set; }

        /// <summary>
        /// Date of parameter file
        /// </summary>
        ///PM 20150902 [21385] Added
        [DataMember(Name = "DTFILE", Order = 6)]
        public DateTime DtFile
        { get; set; }

        /// <summary>
        /// File identifier
        /// </summary>
        ///PM 20150902 [21385] Added
        [DataMember(Name = "FILEIDENTIFIER", Order = 7)]
        public string FileIdentifier
        { get; set; }

        /// <summary>
        ///File format
        /// </summary>
        ///PM 20150902 [21385] Added
        [DataMember(Name = "FILEFORMAT", Order = 8)]
        public string FileFormat
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Exchange parameters
    /// <remarks>Main Spheres reference table: IMSPANEXCHANGE_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanExchange>.DATASETROWNAME,
        Namespace = DataHelper<SpanExchange>.DATASETNAMESPACE)]
    internal sealed class SpanExchange
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// SPAN Exchange internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANEXCHANGE_H", Order = 2)]
        public int SPANExchangeId
        { get; set; }

        /// <summary>
        /// Exchange Acronym
        /// </summary>
        [DataMember(Name = "EXCHANGEACRONYM", Order = 3)]
        public string ExchangeAcronym
        { get; set; }

        /// <summary>
        /// Exchange Symbol
        /// </summary>
        [DataMember(Name = "EXCHANGESYMBOL", Order = 4)]
        public string ExchangeSymbol
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Combined Group parameters
    /// <remarks>Main Spheres reference table: IMSPANGRPCOMB_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanCombinedGroup>.DATASETROWNAME,
        Namespace = DataHelper<SpanCombinedGroup>.DATASETNAMESPACE)]
    internal sealed class SpanCombinedGroup
    {
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 1)]
        public int SPANExchangeComplexId
        { get; set; }

        /// <summary>
        /// SPAN Combined Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCOMB_H", Order = 2)]
        public int SPANCombinedGroupId
        { get; set; }

        /// <summary>
        /// Combined Group Code
        /// </summary>
        [DataMember(Name = "COMBINEDGROUPCODE", Order = 3)]
        public string CombinedGroupCode
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Contract Group parameters
    /// <remarks>Main Spheres reference table: IMSPANGRPCTR_H </remarks>
    /// </summary>
    ///PM 20150707 [21104]  Use column IDIMSPAN_H on table IMSPANGRPCTR_H instead of IDIMSPANEXCHANGE_H
    [DataContract(Name = DataHelper<SpanContractGroup>.DATASETROWNAME,
        Namespace = DataHelper<SpanContractGroup>.DATASETNAMESPACE)]
    internal sealed class SpanContractGroup
    {
        private int riskMultiplier = 1;
        private int riskExponent = 0;

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 1)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Contract Group Code
        /// </summary>
        [DataMember(Name = "COMBCOMCODE", Order = 2)]
        public string ContractGroupCode
        { get; set; }

        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        [DataMember(Name = "IDIMSPAN_H", Order = 3)]
        public int SPANExchangeComplexId
        { get; set; }

        ///// <summary>
        ///// SPAN Exchange internal id
        ///// </summary>
        //[DataMember(Name = "IDIMSPANEXCHANGE_H", Order = 4)]
        //public int SPANExchangeId
        //{ get; set; }

        /// <summary>
        /// SPAN Combined Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCOMB_H", Order = 4)]
        public int SPANCombinedGroupId
        { get; set; }

        /// <summary>
        /// Option Value Limit indicator
        /// </summary>
        [DataMember(Name = "ISOPTIONVALUELIMIT", Order = 5)]
        public bool IsOptionValueLimit
        { get; set; }

        /// <summary>
        /// Spread Intra Contract Group (InterMonth) Method
        /// </summary>
        [DataMember(Name = "INTRASPREADMETHOD", Order = 6)]
        public string IntraContractSpreadMethod
        { get; set; }

        /// <summary>
        /// Scanning Method and Inter Contract Group Spreading (InterCommodity) Method
        /// </summary>
        [DataMember(Name = "INTERSPREADMETHOD", Order = 7)]
        public string ScanningAndInterCommoditySpreadMethod
        { get; set; }

        /// <summary>
        /// Delivery Charge Method
        /// </summary>
        [DataMember(Name = "DELIVERYCHARGEMETH", Order = 8)]
        public string DeliveryChargeMethod
        { get; set; }

        /// <summary>
        /// Number of delivery month
        /// </summary>
        [DataMember(Name = "NBOFDELIVERYMONTH", Order = 9)]
        public int DeliveryMonthNumber
        { get; set; }

        /// <summary>
        /// Short Option Minimum Method
        /// </summary>
        [DataMember(Name = "SOMMETHOD", Order = 10)]
        public string ShortOptionMinimumMethod
        { get; set; }

        /// <summary>
        /// Short Option Charge Rate
        /// </summary>
        [DataMember(Name = "SOMCHARGERATE", Order = 11)]
        public double ShortOptionChargeRateDouble
        { get; set; }
        public decimal ShortOptionChargeRate
        { get { return (decimal)ShortOptionChargeRateDouble; } }

        /// <summary>
        /// Weighted Risk Method
        /// </summary>
        [DataMember(Name = "WEIGHTEDRISKMETHOD", Order = 12)]
        public string WeightedRiskMethod
        { get; set; }

        /// <summary>
        /// Member Initial to Maintenance Ratio
        /// </summary>
        [DataMember(Name = "MEMBERITOMRATIO", Order = 13)]
        public double MemberInitialToMaintenanceRatioDouble
        { get; set; }
        public decimal MemberInitialToMaintenanceRatio
        { get { return (decimal)MemberInitialToMaintenanceRatioDouble; } }

        /// <summary>
        /// Hedger Initial to Maintenance Ratio
        /// </summary>
        [DataMember(Name = "HEDGERITOMRATIO", Order = 14)]
        public double HedgerInitialToMaintenanceRatioDouble
        { get; set; }
        public decimal HedgerInitialToMaintenanceRatio
        { get { return (decimal)HedgerInitialToMaintenanceRatioDouble; } }

        /// <summary>
        /// Speculator Initial to Maintenance Ratio
        /// </summary>
        [DataMember(Name = "SPECULATITOMRATIO", Order = 15)]
        public double SpeculatorInitialToMaintenanceRatioDouble
        { get; set; }
        public decimal SpeculatorInitialToMaintenanceRatio
        { get { return (decimal)SpeculatorInitialToMaintenanceRatioDouble; } }

        /// <summary>
        /// Member Adjustement Factor
        /// </summary>
        [DataMember(Name = "MEMBERADJFACTOR", Order = 16)]
        public double MemberAdjustementFactorDouble
        { get; set; }
        public decimal MemberAdjustementFactor
        { get { return (decimal)MemberAdjustementFactorDouble; } }

        /// <summary>
        /// Hedger Initial to Maintenance Ratio
        /// </summary>
        [DataMember(Name = "HEDGERADJFACTOR", Order = 17)]
        public double HedgerAdjustementFactorDouble
        { get; set; }
        public decimal HedgerAdjustementFactor
        { get { return (decimal)HedgerAdjustementFactorDouble; } }

        /// <summary>
        /// Speculator Initial to Maintenance Ratio
        /// </summary>
        [DataMember(Name = "SPECULORADJFACTOR", Order = 18)]
        public double SpeculatorAdjustementFactorDouble
        { get; set; }
        public decimal SpeculatorAdjustementFactor
        { get { return (decimal)SpeculatorAdjustementFactorDouble; } }

        /// <summary>
        /// Strategy Spread Intra Contract Group Method
        /// </summary>
        [DataMember(Name = "STRATEGYSPREADMETH", Order = 19)]
        public string StrategySpreadMethod
        { get; set; }

        /// <summary>
        /// Risk Exponent : power of ten to be applied to all risk array values 
        /// </summary>
        [DataMember(Name = "RISKEXPONENT", Order = 20)]
        public int RiskExponent
        {
            get
            {
                return riskExponent;
            }
            set
            {
                riskExponent = value;
                riskMultiplier = (int)System.Math.Pow(10, riskExponent);
            }
        }

        /// <summary>
        /// Multiplier to be applied to all risk array values 
        /// </summary>
        public int RiskMultiplier
        {
            get
            {
                // PM 20131120 [19220] RiskMultiplier doit valoir au minimumm 1 
                return riskMultiplier > 0 ? riskMultiplier : 1;
            }
        }

        /// <summary>
        /// Contract Group Margin Currency
        /// </summary>
        [DataMember(Name = "IDC", Order = 21)]
        public string MarginCurrency
        { get; set; }

        /// <summary>
        /// Indicateur d'utilisation du Lambda
        /// </summary>
        /// PM 20150930 [21134] Nouveau
        [DataMember(Name = "ISUSELAMBDA", Order = 22)]
        public bool IsUseLambda
        { get; set; }

        /// <summary>
        /// Lambda Minimum
        /// </summary>
        /// PM 20150930 [21134] Nouveau
        [DataMember(Name = "LAMBDAMIN", Order = 23)]
        public double LambdaMinDouble
        { get; set; }
        public decimal LambdaMin
        { get { return (decimal)LambdaMinDouble; } }

        /// <summary>
        /// Lambda Maximum
        /// </summary>
        /// PM 20150930 [21134] Nouveau
        [DataMember(Name = "LAMBDAMAX", Order = 24)]
        public double LambdaMaxDouble
        { get; set; }
        public decimal LambdaMax
        { get { return (decimal)LambdaMaxDouble; } }
    }

    /// <summary>
    /// Data container for the SPAN Contract parameters
    /// <remarks>Main Spheres reference table: IMSPANCONTRACT_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanContract>.DATASETROWNAME,
        Namespace = DataHelper<SpanContract>.DATASETNAMESPACE)]
    internal sealed class SpanContract
    {
        /// <summary>
        /// SPAN Contract internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCONTRACT_H", Order = 1)]
        public int SPANContractId
        { get; set; }

        /// <summary>
        /// SPAN Exchange internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANEXCHANGE_H", Order = 2)]
        public int SPANExchangeId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 3)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Contract Symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 4)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Minimum Price Increment Amount
        /// </summary>
        [DataMember(Name = "TICKVALUE", Order = 5)]
        public double TickValueDouble
        { get; set; }
        public decimal TickValue
        { get { return (decimal)TickValueDouble; } }

        /// <summary>
        /// Contract Price Currency
        /// </summary>
        [DataMember(Name = "IDC_PRICE", Order = 6)]
        public string ContractCurrency
        { get; set; }

        /// <summary>
        /// Is an Option contract with Variable Tick
        /// </summary>
        /// PM 20130806 [18876] Contract with Variable Tick Value
        [DataMember(Name = "ISOPTVARIABLETICK", Order = 7)]
        public bool IsOptionVariableTick
        { get; set; }

        /// <summary>
        /// Delta Scaling Factor
        /// </summary>
        ///PM 20150902 [21385] Ajout colonne DELTASCALINGFACTOR
        [DataMember(Name = "DELTASCALINGFACTOR", Order = 8)]
        public double? NullableDeltaScalingFactor
        { get; set; }

        public decimal DeltaScalingFactor
        { get { return (decimal)(NullableDeltaScalingFactor ?? 1); } }

        /// <summary>
        /// Min Price Increment
        /// </summary>
        /// PM 20151127 [21571][21605] Ajout MinPriceInc
        [DataMember(Name = "MINPRICEINCR", Order = 9)]
        public double MinPriceIncDouble
        { get; set; }
        public decimal MinPriceInc
        { get { return (decimal)MinPriceIncDouble; } }


        /// <summary>
        /// Delta Divisor
        /// </summary>
        /// PM 20151127 [21571][21605] Ajout DeltaDen
        [DataMember(Name = "DELTADEN", Order = 10)]
        public double DeltaDenDouble
        { get; set; }
        public decimal DeltaDen
        { get { return (decimal)DeltaDenDouble; } }

        /// <summary>
        /// Scanning Range
        /// </summary>
        /// PM 20151127 [21571][21605] Ajout ScanningRange
        [DataMember(Name = "SCANNINGRANGE", Order = 11)]
        public double ScanningRangeDouble
        { get; set; }
        public decimal ScanningRange
        { get { return (decimal)ScanningRangeDouble; } }
    }

    /// <summary>
    /// Data container for the SPAN Delivery Month parameters
    /// <remarks>Main Spheres reference table: IMSPANDLVMONTH_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanDeliveryMonth>.DATASETROWNAME,
        Namespace = DataHelper<SpanDeliveryMonth>.DATASETNAMESPACE)]
    internal sealed class SpanDeliveryMonth
    {
        /// <summary>
        /// SPAN Delivery Month internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANDLVMONTH_H", Order = 1)]
        public int SPANDeliveryMonthId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 2)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Maturity number
        /// </summary>
        [DataMember(Name = "MONTHNUMBER", Order = 3)]
        public int MonthNumber
        { get; set; }

        /// <summary>
        /// Maturity
        /// </summary>
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 4)]
        public string Maturity
        { get; set; }

        /// <summary>
        /// Consumed Charge Rate
        /// </summary>
        [DataMember(Name = "CONSUMEDCHARGERATE", Order = 5)]
        public double ConsumedChargeRateDouble
        { get; set; }
        public decimal ConsumedChargeRate
        { get { return (decimal)ConsumedChargeRateDouble; } }

        /// <summary>
        /// Remaining Charge Rate
        /// </summary>
        [DataMember(Name = "REMAINCHARGERATE", Order = 6)]
        public double RemainingChargeRateDouble
        { get; set; }
        public decimal RemainingChargeRate
        { get { return (decimal)RemainingChargeRateDouble; } }

        /// <summary>
        /// Delta Sign
        /// </summary>
        [DataMember(Name = "DELTASIGN", Order = 7)]
        public string DeltaSign
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Maturity Tier (group) parameters
    /// <remarks>Main Spheres reference table: IMSPANTIER_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanMaturityTier>.DATASETROWNAME,
        Namespace = DataHelper<SpanMaturityTier>.DATASETNAMESPACE)]
    internal sealed class SpanMaturityTier
    {
        /// <summary>
        /// SPAN Contract Group Tier internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANTIER_H", Order = 1)]
        public int SPANTierId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 2)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Spread Type
        /// </summary>
        [DataMember(Name = "SPREADTYPE", Order = 3)]
        public string TierType
        { get; set; }

        /// <summary>
        /// Tier Number
        /// </summary>
        [DataMember(Name = "TIERNUMBER", Order = 4)]
        public int TierNumber
        { get; set; }

        /// <summary>
        /// Starting MonthYear
        /// </summary>
        [DataMember(Name = "STARTINGMONTHYEAR", Order = 5)]
        public string StartingMonthYear
        { get; set; }

        /// <summary>
        /// Ending MonthYear
        /// </summary>
        [DataMember(Name = "ENDINGMONTHYEAR", Order = 6)]
        public string EndingMonthYear
        { get; set; }

        /// <summary>
        /// Short Option Minimum
        /// </summary>
        [DataMember(Name = "SOMCHARGERATE", Order = 7)]
        public string ShortOptionMinimum
        { get; set; }

        /// <summary>
        /// Starting SPAN Contract Group Tier Number
        /// </summary>
        [DataMember(Name = "STARTTIERNUMBER", Order = 8)]
        public int StartingTierNumber
        { get; set; }

        /// <summary>
        /// Ending SPAN Contract Group Tier Number
        /// </summary>
        [DataMember(Name = "ENDTIERNUMBER", Order = 9)]
        public int EndingTierNumber
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Intra Contract Group Spread parameters
    /// <remarks>Main Spheres reference table: IMSPANINTRASPR_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanIntraSpread>.DATASETROWNAME,
        Namespace = DataHelper<SpanIntraSpread>.DATASETNAMESPACE)]
    internal sealed class SpanIntraSpread
    {
        /// <summary>
        /// SPAN Intra Spread internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANINTRASPR_H", Order = 1)]
        public int SPANIntraSpreadId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANGRPCTR_H", Order = 2)]
        public int SPANContractGroupId
        { get; set; }

        /// <summary>
        /// Intra Spread Type
        /// </summary>
        [DataMember(Name = "SPREADTYPE", Order = 3)]
        public string SpreadType
        { get; set; }

        /// <summary>
        /// Intra Spread Priority
        /// </summary>
        [DataMember(Name = "SPREADPRIORITY", Order = 4)]
        public int SpreadPriority
        { get; set; }

        /// <summary>
        /// Intra Spread Number Of Leg
        /// </summary>
        [DataMember(Name = "NUMBEROFLEG", Order = 5)]
        public int NumberOfLeg
        { get; set; }

        /// <summary>
        /// Intra Spread Charge Rate
        /// </summary>
        [DataMember(Name = "CHARGERATE", Order = 6)]
        public double ChargeRateDouble
        { get; set; }
        public decimal ChargeRate
        { get { return (decimal)ChargeRateDouble; } }
    }

    /// <summary>
    /// Data container for the SPAN Intra Contract Group Spread Leg parameters
    /// <remarks>Main Spheres reference table: IMSPANINTRALEG_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanIntraLeg>.DATASETROWNAME,
        Namespace = DataHelper<SpanIntraLeg>.DATASETNAMESPACE)]
    internal sealed class SpanIntraLeg
    {
        /// <summary>
        /// SPAN Intra Spread internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANINTRASPR_H", Order = 1)]
        public int SPANIntraSpreadId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group Intra Spread Tier internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANTIER_H", Order = 2)]
        public int SPANTierId
        { get; set; }

        /// <summary>
        /// SPAN Contract Group Intra Spread Leg Number
        /// </summary>
        [DataMember(Name = "LEGNUMBER", Order = 3)]
        public int LegNumber
        { get; set; }

        /// <summary>
        /// SPAN Contract Group Intra Spread Maturity
        /// </summary>
        [DataMember(Name = "MATURITYMONTHYEAR", Order = 4)]
        public string MaturityMonthYear
        { get; set; }

        /// <summary>
        /// SPAN Contract Group Delta per Intra Spread
        /// </summary>
        [DataMember(Name = "DELTAPERSPREAD", Order = 5)]
        public double DeltaPerSpreadDouble
        { get; set; }
        public decimal DeltaPerSpread
        { get { return (decimal)DeltaPerSpreadDouble; } }

        /// <summary>
        /// SPAN Contract Group Intra Spread Leg Side
        /// </summary>
        [DataMember(Name = "LEGSIDE", Order = 6)]
        public string LegSide
        { get; set; }
    }

    /// <summary>
    /// Data container for the SPAN Maturity parameters
    /// <remarks>Main Spheres reference table: IMSPANMATURITY_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanMaturity>.DATASETROWNAME,
        Namespace = DataHelper<SpanMaturity>.DATASETNAMESPACE)]
    internal sealed class SpanMaturity
    {
        /// <summary>
        /// SPAN Contract internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCONTRACT_H", Order = 1)]
        public int SPANContractId
        { get; set; }

        /// <summary>
        /// Maturity of Future or Underlying
        /// </summary>
        [DataMember(Name = "FUTMMY", Order = 2)]
        public string FutureMaturity
        { get; set; }

        /// <summary>
        /// Option maturity
        /// </summary>
        [DataMember(Name = "OPTMMY", Order = 3)]
        public string OptionMaturity
        { get; set; }

        /// <summary>
        /// Future Price Scan Range
        /// </summary>
        [DataMember(Name = "FUTPRICESCANRANGE", Order = 4)]
        public double FuturePriceScanRangeDouble
        { get; set; }
        public decimal FuturePriceScanRange
        { get { return (decimal)FuturePriceScanRangeDouble; } set { FuturePriceScanRangeDouble = (double)value; } }

        /// <summary>
        /// Delta Scaling Factor
        /// </summary>
        [DataMember(Name = "DELTASCALINGFACTOR", Order = 5)]
        public double DeltaScalingFactorDouble
        { get; set; }
        public decimal DeltaScalingFactor
        { get { return (decimal)DeltaScalingFactorDouble; } set { DeltaScalingFactorDouble = (double)value; } }
    }

    /// <summary>
    /// Data container for the SPAN Risk Array" parameters
    /// <remarks>Main Spheres reference table: IMSPANARRAY_H </remarks>
    /// </summary>
    [DataContract(Name = DataHelper<SpanRiskArray>.DATASETROWNAME,
        Namespace = DataHelper<SpanRiskArray>.DATASETNAMESPACE)]
    internal sealed class SpanRiskArray
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
        /// SPAN Contract internal id
        /// </summary>
        [DataMember(Name = "IDIMSPANCONTRACT_H", Order = 3)]
        public int SPANContractId
        { get; set; }

        /// <summary>
        /// Underlying contract symbol
        /// </summary>
        [DataMember(Name = "UNLCONTRACTSYMBOL", Order = 4)]
        public string UnderlyingContractSymbol
        { get; set; }

        /// <summary>
        /// Maturity of Future or Underlying
        /// </summary>
        [DataMember(Name = "FUTMMY", Order = 5)]
        public string FutureMaturity
        { get; set; }

        /// <summary>
        /// Option maturity
        /// </summary>
        [DataMember(Name = "OPTMMY", Order = 6)]
        public string OptionMaturity
        { get; set; }

        /// <summary>
        /// Put / Call indicator
        /// </summary>
        [DataMember(Name = "PUTCALL", Order = 7)]
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
        [DataMember(Name = "STRIKEPRICE", Order = 8)]
        public double StrikePriceDouble
        { get; set; }
        public decimal StrikePrice
        { get { return (decimal)StrikePriceDouble; } }

        /// <summary>
        /// Composite Delta
        /// </summary>
        [DataMember(Name = "COMPOSITEDELTA", Order = 9)]
        public double CompositeDeltaDouble
        { get; set; }
        public decimal CompositeDelta
        { get { return (decimal)CompositeDeltaDouble; } }

        /// <summary>
        /// Price
        /// </summary>
        //PM 20150106 [20633] Problème serialization des decimales sous Oracle
        [DataMember(Name = "PRICE", Order = 10)]
        public double PriceDouble
        { get; set; }
        public decimal Price
        { get { return (decimal)PriceDouble; } }

        /// <summary>
        /// Risk Value 1
        /// </summary>
        [DataMember(Name = "RISKVALUE1", Order = 11)]
        public double RiskValue1Double
        { get; set; }
        public decimal RiskValue1
        { get { return (decimal)RiskValue1Double; } }

        /// <summary>
        /// Risk Value 2
        /// </summary>
        [DataMember(Name = "RISKVALUE2", Order = 12)]
        public double RiskValue2Double
        { get; set; }
        public decimal RiskValue2
        { get { return (decimal)RiskValue2Double; } }

        /// <summary>
        /// Risk Value 3
        /// </summary>
        [DataMember(Name = "RISKVALUE3", Order = 13)]
        public double RiskValue3Double
        { get; set; }
        public decimal RiskValue3
        { get { return (decimal)RiskValue3Double; } }

        /// <summary>
        /// Risk Value 4
        /// </summary>
        [DataMember(Name = "RISKVALUE4", Order = 14)]
        public double RiskValue4Double
        { get; set; }
        public decimal RiskValue4
        { get { return (decimal)RiskValue4Double; } }

        /// <summary>
        /// Risk Value 5
        /// </summary>
        [DataMember(Name = "RISKVALUE5", Order = 15)]
        public double RiskValue5Double
        { get; set; }
        public decimal RiskValue5
        { get { return (decimal)RiskValue5Double; } }

        /// <summary>
        /// Risk Value 6
        /// </summary>
        [DataMember(Name = "RISKVALUE6", Order = 16)]
        public double RiskValue6Double
        { get; set; }
        public decimal RiskValue6
        { get { return (decimal)RiskValue6Double; } }

        /// <summary>
        /// Risk Value 7
        /// </summary>
        [DataMember(Name = "RISKVALUE7", Order = 17)]
        public double RiskValue7Double
        { get; set; }
        public decimal RiskValue7
        { get { return (decimal)RiskValue7Double; } }

        /// <summary>
        /// Risk Value 8
        /// </summary>
        [DataMember(Name = "RISKVALUE8", Order = 18)]
        public double RiskValue8Double
        { get; set; }
        public decimal RiskValue8
        { get { return (decimal)RiskValue8Double; } }

        /// <summary>
        /// Risk Value 9
        /// </summary>
        [DataMember(Name = "RISKVALUE9", Order = 19)]
        public double RiskValue9Double
        { get; set; }
        public decimal RiskValue9
        { get { return (decimal)RiskValue9Double; } }

        /// <summary>
        /// Risk Value 10
        /// </summary>
        [DataMember(Name = "RISKVALUE10", Order = 20)]
        public double RiskValue10Double
        { get; set; }
        public decimal RiskValue10
        { get { return (decimal)RiskValue10Double; } }

        /// <summary>
        /// Risk Value 11
        /// </summary>
        [DataMember(Name = "RISKVALUE11", Order = 21)]
        public double RiskValue11Double
        { get; set; }
        public decimal RiskValue11
        { get { return (decimal)RiskValue11Double; } }

        /// <summary>
        /// Risk Value 12
        /// </summary>
        [DataMember(Name = "RISKVALUE12", Order = 22)]
        public double RiskValue12Double
        { get; set; }
        public decimal RiskValue12
        { get { return (decimal)RiskValue12Double; } }

        /// <summary>
        /// Risk Value 13
        /// </summary>
        [DataMember(Name = "RISKVALUE13", Order = 23)]
        public double RiskValue13Double
        { get; set; }
        public decimal RiskValue13
        { get { return (decimal)RiskValue13Double; } }

        /// <summary>
        /// Risk Value 14
        /// </summary>
        [DataMember(Name = "RISKVALUE14", Order = 24)]
        public double RiskValue14Double
        { get; set; }
        public decimal RiskValue14
        { get { return (decimal)RiskValue14Double; } }

        /// <summary>
        /// Risk Value 15
        /// </summary>
        [DataMember(Name = "RISKVALUE15", Order = 25)]
        public double RiskValue15Double
        { get; set; }
        public decimal RiskValue15
        { get { return (decimal)RiskValue15Double; } }

        /// <summary>
        /// Risk Value 16
        /// </summary>
        [DataMember(Name = "RISKVALUE16", Order = 26)]
        public double RiskValue16Double
        { get; set; }
        public decimal RiskValue16
        { get { return (decimal)RiskValue16Double; } }

        /// <summary>
        /// Lot Size
        /// </summary>
        [DataMember(Name = "LOTSIZE", Order = 27)]
        public int LotSize
        { get; set; }

        /// <summary>
        /// Contract Value Factor for variable tick option contract
        /// </summary>
        // PM 20130806 [18876] Contract with Variable Tick Value
        [DataMember(Name = "CONTRACTVALUEFACTOR", Order = 28)]
        public double ContractValueFactorDouble
        { get; set; }
        public decimal ContractValueFactor
        { get { return (decimal)ContractValueFactorDouble; } }
    }
    #endregion SPAN method

}
