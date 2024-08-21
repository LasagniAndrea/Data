using System;
using System.Collections.Generic;
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.RiskMethods;
using EfsML.Enum;
using FixML.Enum;
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{

    #region SpanMethod

    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the SPAN calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.SPANMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a SPAN margin calculation node (<see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class SpanMarginCalcMethCom : CalcMethComBase, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1031);
        #endregion static Members

        /// <summary>
        /// Account Type
        /// </summary>
        public SpanAccountType AccountType { get; set; }

        /// <summary>
        /// Maintenance / Initial indicateur
        /// </summary>
        public bool IsMaintenanceAmount { get; set; }

        /// <summary>
        /// AdditionalMarginBoM (AMBO)
        /// </summary>
        // PM 20190401 [24625][24387] New
        public SpanAdditionalMarginBoMCom AdditionalMarginBoM { get; set; }

        /// <summary>
        /// Additional Add On
        /// </summary>
        // PM 20190801 [24717] Ajout AdditionalAddOn
        public decimal AdditionalAddOn { get; set; }

        /// <summary>
        /// Gestion du calcul du Concentration Risk Margin de l'ECC
        /// </summary>
        // PM 20190801 [24717] Ajout
        public bool IsCalcECCConcentrationRiskMargin { get; set; }

        /// <summary>
        /// Concentration Risk Margin de l'ECC
        /// </summary>
        // PM 20190801 [24717] Ajout
        public ECCConcentrationRiskMarginCom ConcentrationRiskMargin { get; set; }

        /// <summary>
        /// Clearing house currency
        /// </summary>
        public string CssCurrency { get; set; }
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Set to true when the current parameter has not been found in the parameters set, 
        /// but it has been built to stock one set of asset elements in position and no parameters have been found for them.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Error code to log the missing parameter event
        /// </summary>
        public SysMsgCode ErrorCode
        {
            // PM 20130814 [18883] Ajout log explicite en cas de paramètres manquants sur la clearing house
            
            //get { return "SYS-01031"; }
            get { return m_SysMsgCode; }
        }
        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public SpanMarginCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion
    }

    /// <summary>
    /// Communication object identifying a SPAN Exchange Complex <see cref=" EfsML.v30.MarginRequirement.SpanExchangeComplexParameter"/>, 
    /// child of an object type SPAN calculation method <see cref="SpanMarginCalcMethCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanExchangeComplexCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// SPAN Exchange Complex internal id
        /// </summary>
        public long SPANExchangeComplexId { get; set; }
        
        /// <summary>
        /// Exchange Complex identifier
        /// </summary>
        public string ExchangeComplex { get; set; }

        /// <summary>
        /// Limitation de l'Option Value par Combined Commodity
        /// </summary>
        public bool IsOptionValueLimit { get; set; }

        /// <summary>
        /// Settlement Session
        /// </summary>
        ///PM 20150902 [21385] Added
        public string SettlementSession { get; set; }

        /// <summary>
        /// Business Date and Time
        /// </summary>
        ///PM 20150902 [21385] Added
        public DateTime DtBusinessTime { get; set; }

        /// <summary>
        /// Date of parameter file
        /// </summary>
        ///PM 20150902 [21385] Added
        public DateTime DtFile { get; set; }

        /// <summary>
        /// File identifier
        /// </summary>
        ///PM 20150902 [21385] Added
        public string FileIdentifier { get; set; }

        /// <summary>
        ///File format
        /// </summary>
        ///PM 20150902 [21385] Added
        public string FileFormat { get; set; }

        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney[] NetOptionValue { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney[] RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney[] RiskMaintenanceAmount { get; set; }

        /// <summary>
        /// Initial Requierement Amount
        /// </summary>
        public IMoney[] InitialRequierementAmount { get; set; }

        /// <summary>
        /// Maintenance Requierement Amount
        /// </summary>
        public IMoney[] MaintenanceRequierementAmount { get; set; }

        /// <summary>
        /// All the Super Inter Commodity Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] SuperInterCommoditySpread { get; set; }

        /// <summary>
        /// All the Inter Commodity Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] InterCommoditySpread { get; set; }

        /// <summary>
        /// All the Inter Exchange Spread done on the Exchange Complex
        /// </summary>
        public SpanInterCommoditySpreadCom[] InterExchangeSpread { get; set; }

        /// <summary>
        /// One Factor Credit on the Exchange Complex
        /// </summary>
        /// PM 20150930 [21134] Add OneFactorCredit
        public SpanOneFactorCreditCom OneFactorCredit { get; set; }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Set to true when the current parameter has not been found in the parameters set, 
        /// but it has been built to stock one set of asset elements in position and no parameters have been found for them.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Error code to log the missing parameter event
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Communication object identifying a SPAN Contract Group (Combined Commodity) <see cref="EfsML.v30.MarginRequirement.SpanCombinedCommodityGroupParameter"/>, 
    /// child of an object type SPAN Exchange Complex <see cref="SpanExchangeComplexCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanCombinedGroupCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// SPAN Combined Group internal id
        /// </summary>
        public long SPANCombinedGroupId { get; set; }
        
        /// <summary>
        /// Contract group identifier
        /// </summary>
        public string CombinedGroup { get; set; }

        /// <summary>
        /// Long Option Value
        /// </summary>
        public IMoney[] LongOptionValue { get; set; }

        /// <summary>
        /// Short Option Value
        /// </summary>
        public IMoney[] ShortOptionValue { get; set; }

        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney[] NetOptionValue { get; set; }
        
        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney[] RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney[] RiskMaintenanceAmount { get; set; }

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Communication object identifying a SPAN Contract Group (Combined Commodity) <see cref="EfsML.v30.MarginRequirement.SpanCombinedCommodityParameter"/>, 
    /// child of an object type SPAN Combined Group <see cref="SpanCombinedGroupCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanContractGroupCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// SPAN Identifier du Contract Group
        /// </summary>
        public long SPANContractGroupId { get; set; }

        /// <summary>
        /// Contract group identifier
        /// </summary>
        public string ContractGroup { get; set; }

        /// <summary>
        /// Strategy Spread Charge Method Code
        /// </summary>
        public string StrategySpreadChargeMethod { get; set; }

        /// <summary>
        /// Inter Month Spread Charge Method Code
        /// </summary>
        public string InterMonthSpreadChargeMethod { get; set; }

        /// <summary>
        /// Delivery Month Charge Method Code
        /// </summary>
        public string DeliveryMonthChargeMethod { get; set; }

        /// <summary>
        /// Weighted Risk Calculation Method
        /// </summary>
        public MarginWeightedRiskCalculationMethodEnum WeightedRiskMethod { get; set; }

        /// <summary>
        /// Utilisation de la méthode One-factor (Lambda)
        /// </summary>
        /// PM 20150930 [21134] Ajout IsUseLambda
        public bool IsUseLambda { get; set; }

        /// <summary>
        /// Lambda Maximum
        /// </summary>
        /// PM 20150930 [21134] Ajout LambdaMax
        public decimal LambdaMax { get; set; }

        /// <summary>
        /// Lambda Minimum
        /// </summary>
        /// PM 20150930 [21134] Ajout LambdaMin
        public decimal LambdaMin { get; set; }

        /// <summary>
        /// Long Option Value
        /// </summary>
        public IMoney LongOptionValue { get; set; }

        /// <summary>
        /// Short Option Value
        /// </summary>
        public IMoney ShortOptionValue { get; set; }

        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney NetOptionValue { get; set; }

        /// <summary>
        /// Short Option Minimum
        /// </summary>
        public IMoney ShortOptionMinimum { get; set; }

        /// <summary>
        /// Delta Net
        /// </summary>
        public decimal DeltaNet { get; set; }

        /// <summary>
        /// Delta Net Remaining
        /// </summary>
        public decimal DeltaNetRemaining { get; set; }

        /// <summary>
        /// Active Scenario
        /// </summary>
        public int ActiveScenario { get; set; }

        /// <summary>
        /// Short Scanning Risk Amount
        /// </summary>
        public IMoney ShortScanRiskAmount { get; set; }

        /// <summary>
        /// Long Scanning Risk Amount
        /// </summary>
        public IMoney LongScanRiskAmount { get; set; }

        /// <summary>
        /// Scanning Risk Amount
        /// </summary>
        public IMoney ScanRiskAmount { get; set; }

        /// <summary>
        /// Price Risk Amount
        /// </summary>
        public IMoney PriceRiskAmount { get; set; }

        /// <summary>
        /// Time Risk Amount
        /// </summary>
        public IMoney TimeRiskAmount { get; set; }

        /// <summary>
        /// Volatility Risk Amount
        /// </summary>
        public IMoney VolatilityRiskAmount { get; set; }

        /// <summary>
        /// Normal Weighted Risk Amount
        /// </summary>
        public IMoney NormalWeightedRiskAmount { get; set; }

        /// <summary>
        /// Capped Weighted Risk Amount
        /// </summary>
        public IMoney CappedWeightedRiskAmount { get; set; }

        /// <summary>
        /// Weighted Risk Amount
        /// </summary>
        public IMoney WeightedRiskAmount { get; set; }

        /// <summary>
        /// Strategy Spread Charge Amount
        /// </summary>
        public IMoney StrategySpreadChargeAmount { get; set; }

        /// <summary>
        /// Inter Month Spread Charge Amount
        /// </summary>
        public IMoney IntraSpreadChargeAmount { get; set; }

        /// <summary>
        /// Delivery Month Charge Amount
        /// </summary>
        public IMoney DeliveryMonthChargeAmount { get; set; }
        
        /// <summary>
        /// Inter Commodity Spread Credit Amount
        /// </summary>
        public IMoney InterCommodityCreditAmount { get; set; }

        /// <summary>
        /// Inter Exchange Spread Credit Amount
        /// </summary>
        public IMoney InterExchangeCreditAmount { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney RiskMaintenanceAmount { get; set; }

        /// <summary>
        /// All Long Scan Risk Scenario Values
        /// </summary>
        public Dictionary<int, decimal> RiskValueLong { get; set; }

        /// <summary>
        /// All Short Scan Risk Scenario Values
        /// </summary>
        public Dictionary<int, decimal> RiskValueShort { get; set; }

        /// <summary>
        /// All Scan Risk Scenario Values
        /// </summary>
        public Dictionary<int, decimal> RiskValue { get; set; }

        /// <summary>
        /// Delta
        /// </summary>
        public SpanDeltaCom[] MaturityDelta { get; set; }

        /// <summary>
        /// Detail of Strategy Spread Charge
        /// </summary>
        public SpanIntraCommoditySpreadCom[] StrategyParameters { get; set; }

        /// <summary>
        /// Detail of Intra Commodity Spread Charge
        /// </summary>
        public SpanIntraCommoditySpreadCom[] IntraCommodityParameters { get; set; }

        /// <summary>
        /// Detail of Delivery Month Charge
        /// </summary>
        public SpanDeliveryMonthChargeCom[] DeliveryMonthParameters { get; set; }

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter : Not Used
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Communication object identifying a SPAN Contract (Commodity) <see cref="EfsML.v30.MarginRequirement.SpanContractParameter"/>, 
    /// child of an object type SPAN Contract Group <see cref="SpanContractGroupCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanContractCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// SPAN Identifier du Contract
        /// </summary>
        public long SPANContractId { get; set; }

        /// <summary>
        /// Contract identifier
        /// </summary>
        public string Contract { get; set; }

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter : Not Used
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Communication object identifying a SPAN Delta
    /// </summary>
    public sealed class SpanDeltaCom
    {
        /// <summary>
        /// Period
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Delta Net
        /// </summary>
        public decimal DeltaNet { get; set; }

        /// <summary>
        /// Delta Net Remaining
        /// </summary>
        public decimal DeltaNetRemaining { get; set; }
    }

    /// <summary>
    /// Communication object identifying a SPAN Asset <see cref="EfsML.v30.MarginRequirement.SpanContractParameter"/>, 
    /// child of an object type SPAN Contract <see cref="SpanContractCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanAssetCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Id de l'Asset
        /// </summary>
        public long AssetId;
        /// <summary>
        /// Id du Contract (paramètre SPAN)
        /// </summary>
        public long SPANContractId;
        /// <summary>
        /// Echéance Future (ou sous-jacent)
        /// </summary>
        public string FutureMaturity;
        /// <summary>
        /// Echéance Option
        /// </summary>
        public string OptionMaturity;
        /// <summary>
        /// Put or Call
        /// </summary>
        public Nullable<PutOrCallEnum> PutOrCall;
        /// <summary>
        /// Strike Price
        /// </summary>
        public decimal StrikePrice;
        /// <summary>
        /// Side
        /// </summary>
        public SideEnum Side;
        /// <summary>
        /// Quantity
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity;
        /// <summary>
        /// Delta
        /// </summary>
        public decimal Delta;
        /// <summary>
        /// Valeurs de risque
        /// </summary>
        public List<KeyValuePair<int, decimal>> RiskValue;
        /// <summary>
        /// Valeur liquidative de l'option
        /// </summary>
        public decimal OptionValue;
        /// <summary>
        /// Valeur minimum de l'option
        /// </summary>
        public decimal ShortOptionMinimum;
        /// <summary>
        /// Devise
        /// </summary>
        public string Currency;

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter : Not Used
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Communication object identifying a SPAN Inter Commodity Spread <see cref="EfsML.v30.MarginRequirement.SpanInterCommoditySpreadParameter"/>, 
    /// child of an object type SPAN Exchange Complex <see cref="SpanExchangeComplexCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanInterCommoditySpreadCom
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SpanInterCommoditySpreadCom()
        {
            IsOffsetChargeMethod = false;
        }

        /// <summary>
        /// Inter Commodity Spread Priority
        /// </summary>
        public int SpreadPriority { get; set; }

        /// <summary>
        /// Inter Commodity Spread Method
        /// </summary>
        public string InterSpreadMethod { get; set; }

        /// <summary>
        /// Number of realized Spread
        /// </summary>
        public decimal NumberOfSpread { get; set; }

        /// <summary>
        /// Limit Number of Spread
        /// </summary>
        public decimal NumberOfSpreadLimit { get; set; }

        /// <summary>
        /// Spread Rate
        /// </summary>
        public decimal SpreadRate { get; set; }

        /// <summary>
        /// Spread Rate Separated or Global
        /// </summary>
        public bool IsSeparatedSpreadRate { get; set; }

        /// <summary>
        /// Method With Offset Charge
        /// </summary>
        public bool IsOffsetChargeMethod { get; set; }
        
        /// <summary>
        /// Offset Charge
        /// </summary>
        public decimal OffsetCharge { get; set; }

        /// <summary>
        /// Portfolio ScanRisk
        /// </summary>
        public decimal PortfolioScanRisk { get; set; }

        /// <summary>
        /// Portfolio Risk
        /// </summary>
        public decimal PortfolioRisk { get; set; }

        /// <summary>
        /// Spread ScanRisk
        /// </summary>
        public decimal SpreadScanRisk { get; set; }

        /// <summary>
        /// New Delta Available
        /// </summary>
        public decimal DeltaAvailable { get; set; }

        /// <summary>
        /// Spread Leg Parameters
        /// </summary>
        public SpanInterCommoditySpreadLegCom[] LegParameters { get; set; }
    }

    /// <summary>
    /// Communication object identifying a SPAN Inter Commodity Spread Leg <see cref="EfsML.v30.MarginRequirement.SpanInterCommoditySpreadLegParameter"/>, 
    /// child of an object type SPAN Inter Commodity Spread <see cref="SpanInterCommoditySpreadCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanInterCommoditySpreadLegCom
    {
        /// <summary>
        /// Exchange Acronym
        /// </summary>
        public string ExchangeAcronym { get; set; }

        /// <summary>
        /// Combined Commodity Code
        /// </summary>
        public string CombinedCommodityCode { get; set; }

        /// <summary>
        /// Spread Rate
        /// </summary>
        public decimal SpreadRate { get; set; }

        /// <summary>
        /// Tier Number
        /// </summary>
        public int TierNumber { get; set; }

        /// <summary>
        /// Maturity
        /// </summary>
        public string Maturity { get; set; }

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        public decimal DeltaPerSpread { get; set; }

        /// <summary>
        /// Delta Available
        /// </summary>
        public decimal DeltaAvailable { get; set; }

        /// <summary>
        /// Delta Remaining
        /// </summary>
        public decimal DeltaRemaining { get; set; }

        /// <summary>
        /// Computed Delta Consumed
        /// </summary>
        public decimal ComputedDeltaConsumed { get; set; }

        /// <summary>
        /// Realy Delta Consumed
        /// </summary>
        public decimal RealyDeltaConsumed { get; set; }

        /// <summary>
        /// Weighted Risk
        /// </summary>
        public decimal WeightedRisk { get; set; }

        /// <summary>
        /// Spread Credit
        /// </summary>
        public decimal SpreadCredit { get; set; }

        /// <summary>
        /// La jambe est-elle obligatoire
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// La jambe est-elle la jambe cible
        /// </summary>
        public bool IsTarget { get; set; }

        /// <summary>
        /// Contract Group dans le cas de Scan based Spread
        /// </summary>
        public SpanContractGroupCom ContractGroup { get; set; }
    }

    /// <summary>
    /// Communication object identifying a SPAN Intra Commodity Spread <see cref="EfsML.v30.MarginRequirement.SpanIntraCommoditySpreadParameter"/>, 
    /// child of an object type SPAN Contract Group <see cref="SpanContractGroupCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanIntraCommoditySpreadCom
    {
        /// <summary>
        /// Intra Commodity Spread Priority
        /// </summary>
        public int SpreadPriority { get; set; }

        /// <summary>
        /// Number of Leg
        /// </summary>
        public int NumberOfLeg { get; set; }

        /// <summary>
        /// Charge Rate
        /// </summary>
        public decimal ChargeRate { get; set; }

        /// <summary>
        /// Spread Charge
        /// </summary>
        public decimal SpreadCharge { get; set; }

        /// <summary>
        /// Number of realized Spread
        /// </summary>
        public decimal NumberOfSpread { get; set; }

        /// <summary>
        /// Intra Commodity Spread Legs
        /// </summary>
        public SpanIntraCommoditySpreadLegCom[] SpreadLeg { get; set; }
    }

    /// <summary>
    /// Communication object identifying a SPAN Intra Commodity Spread Leg <see cref="EfsML.v30.MarginRequirement.SpanIntraCommoditySpreadLegParameter"/>, 
    /// child of an object type SPAN Intra Commodity Spread <see cref="SpanIntraCommoditySpreadCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanIntraCommoditySpreadLegCom
    {
        /// <summary>
        /// Leg Number
        /// </summary>
        public int LegNumber { get; set; }

        /// <summary>
        /// Leg Side
        /// </summary>
        public string LegSide { get; set; }

        /// <summary>
        /// Tier Number
        /// </summary>
        public int TierNumber { get; set; }

        /// <summary>
        /// Maturity
        /// </summary>
        public string Maturity { get; set; }

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        public decimal DeltaPerSpread { get; set; }

        /// <summary>
        /// Assumed Long Side
        /// </summary>
        public string AssumedLongSide { get; set; }

        /// <summary>
        /// Delta Long
        /// </summary>
        public decimal DeltaLong { get; set; }

        /// <summary>
        /// Delta Short
        /// </summary>
        public decimal DeltaShort { get; set; }

        /// <summary>
        /// Delta Consumed
        /// </summary>
        public decimal DeltaConsumed { get; set; }
    }

    /// <summary>
    /// Communication object identifying a SPAN Delivery Month Charge <see cref="EfsML.v30.MarginRequirement.SpanDeliveryMonthChargeParameter"/>, 
    /// child of an object type SPAN Contract Group <see cref="SpanContractGroupCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    public sealed class SpanDeliveryMonthChargeCom
    {
        /// <summary>
        /// Echeance en livraison
        /// </summary>
        public string Maturity { get; set; }

        /// <summary>
        /// Signe de Delta à Considérer
        /// </summary>
        public string DeltaSign { get; set; }

        /// <summary>
        /// Deltas déja utilisés pour des spreads
        /// </summary>
        public decimal DeltaNetUsed { get; set; }

        /// <summary>
        /// Deltas non encore utilisés pour des spreads
        /// </summary>
        public decimal DeltaNetRemaining { get; set; }

        /// <summary>
        /// Taux de charge pour des deltas déjà utilisés dans des spreads
        /// </summary>
        public decimal ConsumedChargeRate { get; set; }

        /// <summary>
        /// Taux de charge pour des deltas non encore utilisés dans des spreads
        /// </summary>
        public decimal RemainingChargeRate { get; set; }

        /// <summary>
        /// Charge de livraison
        /// </summary>
        public decimal DeliveryCharge { get; set; }

    }
    
    /// <summary>
    /// Communication object identifying a SPAN One-factor model credit <see cref="EfsML.v30.MarginRequirement.SpanOneFactorCreditParameter"/>, 
    /// child of an object type SPAN Contract Group <see cref="SpanContractGroupCom"/>, 
    /// used to build the sub chapters of the SPAN margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.SpanMarginCalculationMethod"/>)
    /// </summary>
    /// PM 20150930 [21134] New
    public sealed class SpanOneFactorCreditCom
    {
        /// <summary>
        /// Final General Risk Lambda Max
        /// </summary>
        public decimal FinalGeneralRiskLMax { get; set; }

        /// <summary>
        /// Final General Risk Lambda Min
        /// </summary>
        public decimal FinalGeneralRiskLMin { get; set; }

        /// <summary>
        /// Residual Iodiosyncratic Risk Max
        /// </summary>
        public decimal IdiosyncraticRiskLMax { get; set; }

        /// <summary>
        /// Residual Iodiosyncratic Risk Min
        /// </summary>
        public decimal IdiosyncraticRiskLMin { get; set; }

        /// <summary>
        /// ScanRisk Offset Lambda Max
        /// </summary>
        public decimal ScanRiskOffsetLMax { get; set; }

        /// <summary>
        /// ScanRisk Offset Lambda Min
        /// </summary>
        public decimal ScanRiskOffsetLMin { get; set; }

        /// <summary>
        /// ScanRisk Offset
        /// </summary>
        public decimal ScanRiskOffset { get; set; }

        /// <summary>
        /// ScanRisk Global
        /// </summary>
        public decimal GlobalScanRisk { get; set; }

        /// <summary>
        /// Offset Percentage
        /// </summary>
        public decimal OffsetPercentage { get; set; }

        /// <summary>
        /// Offset Percentage Cap
        /// </summary>
        public decimal OffsetMax { get; set; }
    }

    /// <summary>
    /// Communication object identifying ECC Additional Margin BoM
    /// </summary>
    // PM 20190401 [24625][24387] New
    public sealed class SpanAdditionalMarginBoMCom : IRiskParameterCommunicationObject
    {
        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter : Not Used
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Communication object pour les paramètres de volume de marché
    /// </summary>
    public sealed class ECCMarketVolumeCom
    {
        #region Members
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        public string CombinedCommodityStress { get; set; }

        /// <summary>
        /// Market Volume
        /// </summary>
        public decimal MarketVolume { get; set; }
        #endregion Members
    }

    /// <summary>
    /// Communication object pour le détail du Concentration Risk Margin de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConRiskMarginUnitCom
    {
        #region Members
        /// <summary>
        /// Absolute Cumulative Position Size
        /// </summary>
        public decimal AbsoluteCumulativePosition { get; set; }

        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        public string CombinedCommodityStress { get; set; }

        /// <summary>
        /// Daily Market Volume
        /// </summary>
        public decimal DailyMarketVolume { get; set; }

        /// <summary>
        /// LiquidationPeriod
        /// </summary>
        public decimal LiquidationPeriod { get; set; }

        /// <summary>
        /// Weighted Absolute Cumulative Position Size
        /// </summary>
        public decimal WeightedAbsCumulPosition { get; set; }

        //public Dictionary<int, decimal> m_PositionSize = default;
        #endregion Members
    }

    /// <summary>
    /// Communication object pour les montants Concentration Risk Margin de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConRiskMarginAmountCom
    {
        /// <summary>
        /// Absolute Cumulative Position Size
        /// </summary>
        public decimal AbsoluteCumulativePosition { get; set; }

        /// <summary>
        /// Concentration Risk Margin
        /// </summary>
        public IMoney ConcentrationRiskMargin { get; set; }

        /// <summary>
        /// Liquidation Period
        /// </summary>
        public decimal LiquidationPeriod { get; set; }

        /// <summary>
        /// Weighted Absolute Cumulative Position Size
        /// </summary>
        public decimal WeightedAbsCumulPosition { get; set; }

        /// <summary>
        /// Détail pour chaque Combined Commodity Stress
        /// </summary>
        public ECCConRiskMarginUnitCom[] ConcentrationRiskMarginUnits { get; set; }

        /// <summary>
        /// Paramètres de volume de marché
        /// </summary>
        public ECCMarketVolumeCom[] MarketVolume { get; set; }
    }

    /// <summary>
    /// Communication object pour le Concentration Risk Margin de l'ECC
    /// </summary>
    // PM 20190801 [24717] Ajout
    public sealed class ECCConcentrationRiskMarginCom
    {
        /// <summary>
        /// Additional Add On
        /// </summary>
        public decimal AdditionalAddOn { get; set; }

        /// <summary>
        /// Concentration Risk Margin de l'ECC
        /// </summary>
        public ECCConRiskMarginAmountCom[] ConcentrationRiskMarginAmounts { get; set; }

        /// <summary>
        /// Paramètre Market Volume de marché de l'ECC
        /// </summary>
        public ECCMarketVolumeCom[] MarketVolume { get; set; }
    }
    #endregion SpanMethod

}
