using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{

    /// <summary>
    ///// Actual evaluation status of a deposit
    ///// </summary>
    //public enum DepositStatus
    //{
    //    /// <summary>
    //    /// not yet evaluated
    //    /// </summary>
    //    NOTEVALUATED,
    //    /// <summary>
    //    /// Evaluating...
    //    /// </summary>
    //    EVALUATING,
    //    /// <summary>
    //    /// Evaluated without errors
    //    /// </summary>
    //    EVALUATED,
    //}

    ///// <summary>
    ///// which type of hierarchy generated the deposit request: Clearer or Entity
    ///// </summary>
    //public enum DepositHierarchyClass
    //{
    //    /// <summary>
    //    /// unknown hierarchy
    //    /// </summary>
    //    UNKNOWN,
    //    /// <summary>
    //    /// deposit owned by a marginregoffice actor descending from an Entity
    //    /// </summary>
    //    ENTITY,
    //    /// <summary>
    //    /// deposit owned by a marginregoffice actor descending from a Clearer (Broker, Clearing House, etc...)
    //    /// </summary>
    //    CLEARER
    //}

    ///// <summary>
    ///// define the type of the quantity for a specific deposit factor, 
    ///// the quantity type is usally attached to a net by asset position
    ///// </summary>
    //public enum RiskMethodQtyType
    //{
    //    /// <summary>
    //    /// type put (option), for a quantity related to an open position
    //    /// </summary>
    //    Put,
    //    /// <summary>
    //    /// type call (option), for a quantity related to an open position
    //    /// </summary>
    //    Call,
    //    /// <summary>
    //    /// type future , for a quantity related to an open position
    //    /// </summary>
    //    Future,
    //    /// <summary>
    //    /// type put (option) , for a quantity related to a delivery position 
    //    /// (an execution/assignation has been performed on an open position on a contract in physical delivery)
    //    /// </summary>
    //    ExeAssPut,
    //    /// <summary>
    //    /// type call (option) , for a quantity related to a delivery position 
    //    /// (an execution/assignation has been performed on an open position on a contract in physical delivery)
    //    /// </summary>
    //    ExeAssCall,
    //    /// <summary>
    //    /// type future, for a quantity related to a position on a future contracti in physical delivery that has passed its expiration date 
    //    /// </summary>
    //    FutureMoff,
    //    /// <summary>
    //    /// type position on action, for position connected directly to stock asset, not ETD contracts
    //    /// </summary>
    //    PositionAction,
    //}

    //// TODO MF à déplacer dans le projet Common (Constant.Cs) lors du passage au framework 3.5 du projet même (à cause d'EnumMember)
    ///// <summary>
    ///// Strategy of position reduction for a specific derivative market
    ///// </summary>
    //[DataContract(Name = "PosStockCoverEnum")]
    //public enum PosStockCoverEnum
    //{
    //    /// <summary>
    //    /// Coverage for Futures and Options sorted by quote desc, maturity desc and strike asc
    //    /// </summary>
    //    /// <remarks>
    //    /// EUROSYS® : COUV_OPT_OR_FUT
    //    /// </remarks>
    //    [XmlEnum("Default")]
    //    [EnumMember(Value = "Default")]
    //    Default = 0,
    //    /// <summary>
    //    /// Priority to Futures, assets ordered by quantity desc, maturity desc (for Futures) and quote desc, maturity desc and strike asc (for Options)
    //    /// </summary>
    //    /// <remarks>
    //    /// EUROSYS® : COUV_STOCK_ALL - FUT/OPT
    //    /// </remarks>
    //    [XmlEnum("PriorityStockFuture")]
    //    [EnumMember(Value = "PriorityStockFuture")]
    //    PriorityStockFuture = 1,
    //    /// <summary>
    //    /// Priority to Options, assets ordered by quote desc, maturity desc and strike asc (for Options) and quantity desc, maturity desc (for Futures)
    //    /// </summary>
    //    /// <remarks>
    //    /// EUROSYS® : COUV_STOCK_ALL - OPT/FUT
    //    /// </remarks>
    //    [XmlEnum("PriorityStockOption")]
    //    [EnumMember(Value = "PriorityStockOption")]
    //    PriorityStockOption = 2,
    //    /// <summary>
    //    /// Futures coverage only, assets ordered by quantity desc, maturity desc
    //    /// </summary>
    //    /// <remarks>
    //    /// EUROSYS® : COUV_STOCK_FUTURE
    //    /// </remarks>
    //    [XmlEnum("StockFuture")]
    //    [EnumMember(Value = "StockFuture")]
    //    StockFuture = 3,
    //    /// <summary>
    //    /// Options coverage only, assets ordered by quote desc, maturity desc and strike asc
    //    /// </summary>
    //    /// <remarks>
    //    /// EUROSYS® : COUV_STOCK_OPTION
    //    /// </remarks>
    //    [XmlEnum("StockOption")]
    //    [EnumMember(Value = "StockOption")]
    //    StockOption = 4,
    //    /// <summary>
    //    /// No coverage
    //    /// </summary>
    //    /// <remarks>
    //    /// No relative value inside EUROSYS®??
    //    /// </remarks>
    //    [XmlEnum("None")]
    //    [EnumMember(Value = "None")]
    //    None = 5,

    //}

    /// <summary>
    /// PutCall indicator (null for dc futures)
    /// </summary>
    /// <remarks>http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag201.html</remarks>
    public enum PutCallIndicator
    {
        /// <summary>
        /// default value, For futures (0 or NULL)
        /// </summary>
        Futures = 0,

        /// <summary>
        /// Put indicator, for Options contracts
        /// </summary>
        Put = 1,

        /// <summary>
        /// Call indicator, for Options contracts
        /// </summary>
        Call = 2,
    }

    /// <summary>
    /// side of the exchange rate (currency per currency), the side depends on the sign of the margin amount to convert
    /// </summary>
    [DataContract(Name = "FxRateSide")]
    public enum ExchangeRateSide
    {

        /// <summary>
        /// No side is provided
        /// </summary>
        None = 0,

        /// <summary>
        /// Exchange rate for credit amount (usually expressed with a negative sign)
        /// </summary>
        [XmlEnum("Bid")]
        [EnumMember(Value = "Bid")]
        Credit = 1,

        /// <summary>
        /// Exchange rate for margin amount (usually expressed with a positive sign)
        /// </summary>
        [XmlEnum("Ask")]
        [EnumMember(Value = "Ask")]
        Debit = 2,
    }

    /// <summary>
    /// Maturity switch activation status. 
    /// When activated (value: Y) the maturity factor enters the risk evaluation process for the current derivative contract.
    /// </summary>
    /// <value>Y -> Yes, N -> Not</value>
    [DataContract(Name = "MaturitySwitch")]
    public enum MaturitySwitch
    {
        /// <summary>
        /// default/null switch
        /// </summary>
        Default = 0,
        /// <summary>
        /// Yes. The maturity factor will be used in the risk evaluation process
        /// </summary>
        [XmlEnum("Y")]
        [EnumMember(Value = "Y")]
        Yes = 1,
        /// <summary>
        /// Not. The maturity factor will NOT be used in the risk evaluation process
        /// </summary>
        [XmlEnum("N")]
        [EnumMember(Value = "N")]
        Not = 2
    }

    /// <summary>
    /// Margin style of a margin class. Related to the evaluation method of a derivative contract.
    /// When the margin style has value "future style" then the premium margin will not be calculated.
    /// </summary>
    /// <value>F -> Future style, T -> Traditional (premium) style</value>
    [DataContract(Name = "MarginStyle")]
    public enum MarginStyle
    {
        /// <summary>
        /// default/null style
        /// </summary>
        Default = 0,
        /// <summary>
        /// Traditional (premium) style. The premium will be calculated
        /// </summary>
        [XmlEnum("T")]
        [EnumMember(Value = "T")]
        Traditional = 1,
        /// <summary>
        /// Futures style. The premium will NOT be calculated
        /// </summary>
        [XmlEnum("F")]
        [EnumMember(Value = "F")]
        Futures = 2
    }

    ///// <summary>
    ///// Type de données utilisées par une méthode pour l'évaluation du déposit
    ///// </summary>
    ///// PM 20170313 [22833] Ajout enum RiskMethodDataType
    //public enum RiskMethodDataTypeEnum
    //{
    //    /// <summary>
    //    /// Calcul du déposit sur la position
    //    /// </summary>
    //    Position,
    //    /// <summary>
    //    /// Calcul du déposit sur la valeur des trades
    //    /// </summary>
    //    TradeValue,
    //}

    ///// <summary>
    ///// Struct including the mandatory values to enable different coverage strategies
    ///// </summary>
    ///// <remarks>
    ///// <seealso cref="EFS.SpheresRiskPerformance.RiskMethods.BaseMethod.ReducePosition"/>
    ///// </remarks>
    //public struct CoverageSortParameters
    //{

    //    /// <summary>
    //    /// Identifies the sort parameter.
    //    /// internal id of the underlying asset to link the sort parameter to the ETD asset in position  and the relative stock parameter
    //    /// </summary>
    //    internal int AssetId;

    //    /// <summary>
    //    /// Identifies the sort parameter.
    //    /// Used to get the relative contract category 
    //    /// </summary>
    //    internal RiskMethodQtyType Type;

    //    /// <summary>
    //    /// internal id of the etd contract to link the sort parameter to the to the relative stock parameter,
    //    /// in case one specific stock parameter exists for the derivative contract
    //    /// </summary>
    //    internal int ContractId;

    //    /// <summary>
    //    /// ETD asset quote (J-1), used to sort the positions 
    //    /// </summary>
    //    internal decimal Quote;

    //    /// <summary>
    //    /// Maturity rule, used to sort the positions
    //    /// </summary>
    //    internal decimal MaturityYearMonth;

    //    /// <summary>
    //    /// Struke price, used to sort the positions
    //    /// </summary>
    //    internal decimal StrikePrice;

    //    /// <summary>
    //    /// Contract multiplier
    //    /// </summary>
    //    internal decimal Multiplier;
    //}

    ///// <summary>
    ///// key identifying a position action
    ///// </summary>
    ///// <remarks>the key has not a comparer associated, DO NOT use inside Dictonary collection or any hash table</remarks>
    //internal class PosActionRiskMarginKey : PosRiskMarginKey
    //{
    //    /// <summary>
    //    /// Symbol of the equity contract for the stocks in position
    //    /// </summary>
    //    public string derivativeContractSymbol;

    //    /// <summary>
    //    /// maturity date of the position action (expressed in day/month/year)
    //    /// </summary>
    //    public DateTime maturityDate;

    //    /// <summary>
    //    /// Get the identifier of the position action (the identiier is composed by the symbol + the maturity date)
    //    /// </summary>
    //    public string PosActionIdentifier
    //    {
    //        get
    //        {
    //            return String.Concat(
    //                derivativeContractSymbol,
    //                " ",
    //                maturityDate > DateTime.MinValue
    //                    ? maturityDate.ToShortDateString() : Cst.NotAvailable);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 
    ///// </summary>
    //internal struct RiskMarginPositionAction
    //{
    //    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    //    // EG 20170127 Qty Long To Decimal
    //    public decimal Quantity;

    //    public decimal LongCrtVal;

    //    public decimal ShortCrtVal;

    //    public string Currency;
    //    // EG 20150920 [21374] Int (int32) to Long (Int64) 
    //    // EG 20170127 Qty Long To Decimal
    //    public RiskMarginPositionAction(decimal pQuantity, decimal pLongCrtVal, decimal pShortCrtVal, string pCurrency)
    //    {
    //        this.Quantity = pQuantity;

    //        this.LongCrtVal = pLongCrtVal;

    //        this.ShortCrtVal = pShortCrtVal;

    //        this.Currency = pCurrency;
    //    }
    //}

    ///// <summary>
    ///// Specific market risk parameters
    ///// </summary>
    ///// <remarks>Add members as will to satisfy new methods needs</remarks>
    //public struct MarketParameter
    //{
    //    /// <summary>
    //    /// Market internal id
    //    /// </summary>
    //    public int MarketId;

    //    /// <summary>
    //    /// Cross margin activiaton for the current market (and entity)
    //    /// </summary>
    //    public bool CrossMarginActivated;

    //    /// <summary>
    //    /// Default coverage strategy
    //    /// </summary>
    //    public PosStockCoverEnum StockCoverageType;

    //    /// <summary>
    //    /// Business Center
    //    /// </summary>
    //    public string BusinessCenter;
    //}

    ///// <summary>
    ///// Specific actor (role: MarginReqOffice) risk parameters
    ///// </summary>
    ///// <remarks>
    ///// Add members as will to satisfy new methods needs.</remarks>
    //public class MarginReqOfficeParameter
    //{
    //    /// <summary>
    //    /// internal Id of the marginreqoffice actor which use this set of parameters
    //    /// </summary>
    //    /// <value>a valid Spheres id actor value, 0 values are not allowed</value>
    //    public int ActorId;

    //    /// <summary>
    //    /// internal Id of the marginreqoffice book where this set of parameters is active
    //    /// </summary>
    //    /// <value>When 0 then the parameters set is valid for all the actor's books</value>
    //    public int BookId;

    //    /// <summary>
    //    /// Internal Id of the clearing house where this set of parameters is active
    //    /// </summary>
    //    /// <value>When 0 then the parameters set is valid for all the clearing house</value>
    //    public int CssId;

    //    /// <summary>
    //    /// Internal Id of the market where this set of parameters is active
    //    /// </summary>
    //    /// <value>When 0 then the parameters set is valid for all the markets</value>
    //    public int MarketId;

    //    /// <summary>
    //    /// Default coverage strategy fro the current actor
    //    /// </summary>
    //    public PosStockCoverEnum StockCoverageType;

    //    /// <summary>
    //    /// Multiplier ratio affecting the final deposit amount
    //    /// </summary>
    //    /// <value>when -1 the value does not exist and must not be considered, 
    //    /// when >= 0 the value is valid and mus replace the other ratios</value>
    //    public decimal WeightingRatio;

    //    /// <summary>
    //    /// SPAN Account type
    //    /// </summary>
    //    public SpanAccountType SpanAccountType;

    //    /// <summary>
    //    /// Flag indicator to include the maintenance amount for a deposit calculate with the SPAN method.
    //    /// When true the amount will be considered.
    //    /// </summary>
    //    public bool SpanMaintenanceAmountIndicator;

    //    /// <summary>
    //    /// SPAN Scan Risk Offset Cap Percentage's
    //    /// </summary>
    //    /// PM 20150930 [21134] Add ScanRiskOffsetCapPrct
    //    public decimal ScanRiskOffsetCapPrct;

    //    /// <summary>
    //    /// Indique s'il faut calculer un déposit pour les positions en attente de livraison (exercée/assignée) lorsque la méthode de calcul le permet
    //    /// </summary>
    //    /// PM 20170106 [22633] Add IsInitialMarginForExeAssPosition
    //    public bool IsInitialMarginForExeAssPosition;

    //    /// <summary>
    //    /// Constructeur de la structure
    //    /// </summary>
    //    public MarginReqOfficeParameter()
    //    {
    //        // PM 20170106 [22633] Initialisation de IsInitialMarginForExeAssPosition à True pour rétrocompatibilité
    //        IsInitialMarginForExeAssPosition = true;
    //    }
    //}

    ///// <summary>
    ///// Class representing a deposit
    ///// </summary>
    ///// <remarks>any deposit object will be used by one method, clearing house only</remarks>
    //public sealed class Deposit
    //{
    //    /// <summary>
    //    /// Current evaluation status
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlAttribute(AttributeName = "grossmargining")]
    //    public bool IsGrossMargining { get; set; }

    //    /// <summary>
    //    /// Current evaluation status
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlAttribute("status")]
    //    public DepositStatus Status { get; set; }

    //    List<Money> m_Amounts = new List<Money>();

    //    /// <summary>
    //    /// Amounts of the deposit
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlArray]
    //    public List<Money> Amounts
    //    {
    //        get { return m_Amounts; }
    //        set { m_Amounts = value; }
    //    }

    //    /// <summary>
    //    /// Weighting ratio amounts of the deposit
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlArray]
    //    public List<Money> WeightingRatioAmounts
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// root element (in other words the deposit item extracted from the factors list )
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    public RiskElement Root
    //    {
    //        get;
    //        set;
    //    }

    //    List<RiskElement> m_Factors;

    //    /// <summary>
    //    /// Elements contributing to evaluate the deposit
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlArray]
    //    public List<RiskElement> Factors
    //    {
    //        get { return m_Factors; }
    //        set { m_Factors = value; }
    //    }

    //    // PM 20170313 [22833] Changement de type de m_PairsActorBookConstitutingPosition : Pair<int, int>[] => RiskActorBook[]
    //    //Pair<int, int>[]  m_PairsActorBookConstitutingPosition;

    //    ///// <summary>
    //    ///// Actors/books in position for the current deposit
    //    ///// </summary>
    //    //[ReadOnly(true)]
    //    //[Browsable(false)]
    //    //[XmlArray("ActorsBooks")]
    //    //public Pair<int, int>[] PairsActorBookConstitutingPosition
    //    //{
    //    //    get { return m_PairsActorBookConstitutingPosition; }
    //    //    set { m_PairsActorBookConstitutingPosition = value; }
    //    //}

    //    RiskActorBook[] m_PairsActorBookConstitutingPosition;

    //    /// <summary>
    //    /// Actors/books in position for the current deposit
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlArray("ActorsBooks")]
    //    public RiskActorBook[] PairsActorBookConstitutingPosition
    //    {
    //        get { return m_PairsActorBookConstitutingPosition; }
    //        set { m_PairsActorBookConstitutingPosition = value; }
    //    }

    //    /// <summary>
    //    /// Deposit main communication object, used to build the calculation sheet (log)
    //    /// </summary>
    //    /// <remarks>not serialized at the moment</remarks>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    [XmlIgnore]
    //    public IMarginCalculationMethodCommunicationObject[] MarginCalculationMethods
    //    {
    //        get;
    //        set;
    //    }

    //    List<Pair<int, int>> m_ActorBookAncestors;

    //    /// <summary>
    //    /// ancestors of the actor affected by the current deposit
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    public List<Pair<int, int>> ActorBookAncestors
    //    {
    //        get { return m_ActorBookAncestors; }
    //        set { m_ActorBookAncestors = value; }
    //    }

    //    /// <summary>
    //    /// Result of the previous evaluation, may be null
    //    /// </summary>
    //    [ReadOnly(true)]
    //    [Browsable(false)]
    //    public TradeRisk PrevResult
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// which type of hierarchy generated the deposit request: Clearer or Entity
    //    /// </summary>
    //    [XmlAttribute("originatingfrom")]
    //    public DepositHierarchyClass HierarchyClass
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Flag to signal that a deposit is not in position
    //    /// </summary>
    //    [XmlAttribute("notinposition")]
    //    public bool NotInPosition
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Weighting ratio for the deposit object
    //    /// </summary>
    //    [XmlAttribute("wratio")]
    //    public decimal WeightingRatio
    //    {
    //        get;
    //        set;
    //    }

    //    /// <summary>
    //    /// Get an empty RiskEvaluationElement, serialization purpose only
    //    /// </summary>
    //    public Deposit()
    //    { }

    //    /// <summary>
    //    /// Get a well initialized deposit element
    //    /// </summary>
    //    /// <param name="pRoot">starting element</param>
    //    /// <param name="pIsGrossMargining">gross margining evaluation when true</param>
    //    /// <param name="pFactors">elements (inherited and directly owned) contributing to evaluate the deposit</param>
    //    /// <param name="pPairsActorBookConstitutingPosition">actors books contributing to the deposit evaluation</param>
    //    /// <param name="pPrevResult">Result object of the previous evaluation, may be null</param>
    //    /// <param name="pHierarchyClass">is the deposit affecting an actor descending from an actor entity or from a clearer?</param>
    //    /// <param name="pActorBookAncestors">ancestors of the actor affected by the current deposit</param>
    //    /// <param name="pWeightingRatio">multiplication factor of the deposit amount</param>
    //    /// PM 20170313 [22833] Changement de type du paramètre pPairsActorBookConstitutingPosition : IEnumerable<Pair<int, int>> => IEnumerable<RiskActorBook>
    //    public Deposit(
    //        RiskElement pRoot,
    //        bool pIsGrossMargining,
    //        IEnumerable<RiskElement> pFactors,
    //        //IEnumerable<Pair<int, int>> pPairsActorBookConstitutingPosition,
    //        IEnumerable<RiskActorBook> pPairsActorBookConstitutingPosition,
    //        DepositHierarchyClass pHierarchyClass,
    //        TradeRisk pPrevResult,
    //        decimal pWeightingRatio,
    //        IEnumerable<Pair<int, int>> pActorBookAncestors)
    //    {
    //        Root = pRoot;

    //        IsGrossMargining = pIsGrossMargining;

    //        if (pFactors != null)
    //        {
    //            Factors = pFactors.ToList();
    //        }

    //        if (pPairsActorBookConstitutingPosition != null)
    //        {
    //            PairsActorBookConstitutingPosition = pPairsActorBookConstitutingPosition.ToArray();
    //        }

    //        if (pActorBookAncestors != null)
    //        {
    //            ActorBookAncestors = pActorBookAncestors.ToList();
    //        }

    //        PrevResult = pPrevResult;

    //        WeightingRatio = pWeightingRatio;

    //        HierarchyClass = pHierarchyClass;

    //        Status = DepositStatus.NOTEVALUATED;
    //    }

    //    /// <summary>
    //    /// Get a well initialized deposit element
    //    /// </summary>
    //    /// <param name="pRoot">starting element</param>
    //    /// <param name="pIsGrossMargining">gross margining evaluation when true</param>
    //    /// <param name="pFactors">elements (inherited and directly owned) contributing to evaluate the deposit</param>
    //    /// <param name="pPrevResult">Result object of the previous evaluation, may be null</param>
    //    /// <param name="pHierarchyClass">is the deposit affecting an actor descending from an actor entity or from a clearer?</param>
    //    /// <param name="pPairsActorBookConstitutingPosition">actors books contributing to the deposit evaluation</param>
    //    /// <param name="pWeightingRatio">multiplication factor of the deposit amount</param>
    //    /// PM 20170313 [22833] Changement de type de pPairsActorBookConstitutingPosition : IEnumerable<Pair<int, int>> => IEnumerable<RiskActorBook>
    //    public Deposit(
    //       RiskElement pRoot,
    //       bool pIsGrossMargining,
    //       IEnumerable<RiskElement> pFactors,
    //       IEnumerable<RiskActorBook> pPairsActorBookConstitutingPosition,
    //       DepositHierarchyClass pHierarchyClass,
    //       TradeRisk pPrevResult,
    //       decimal pWeightingRatio) :
    //       this(pRoot, pIsGrossMargining, pFactors, pPairsActorBookConstitutingPosition, pHierarchyClass, pPrevResult, pWeightingRatio, null)
    //    {

    //    }

        
    //}

    ///// <summary>
    ///// Class de stockage de la cotation d'un asset
    ///// </summary>
    //public sealed class AssetQuoteParameter
    //{
    //    /// <summary>
    //    /// Underlying Asset Id
    //    /// </summary>
    //    public int AssetId;

    //    /// <summary>
    //    /// Underlying Asset Category
    //    /// </summary>
    //    public Cst.UnderlyingAsset AssetCategoryEnum;

    //    /// <summary>
    //    /// Cotation
    //    /// </summary>
    //    public decimal Quote;

    //    /// <summary>
    //    /// Données reçues lors de la lecture de la cotation
    //    /// </summary>
    //    public string IdMarketEnv;
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    public string IdValScenario;
    //    /// <summary>
    //    /// Adjusted Time
    //    /// </summary>
    //    public DateTime AdjustedTime;
    //    /// <summary>
    //    /// Quote Side
    //    /// </summary>
    //    public string QuoteSide;
    //    /// <summary>
    //    /// Quote Timing
    //    /// </summary>
    //    public string QuoteTiming;
    //    /// <summary>
    //    /// Message de retour de la lecture de cotation
    //    /// </summary>
    //    public SystemMSGInfo SystemMsgInfo;
    //}

    ///// <summary>
    ///// helper class containing all the extensions valid for the risk method namespace
    ///// </summary>
    //public static class RiskMethodExtensions
    //{
    //    /// <summary>
    //    /// Extends the DateTime type to return the numeric year month of the current date 
    //    /// </summary>
    //    /// <param name="businessDate"></param>
    //    /// <returns>the numeric representation of the input date in YYYYMM</returns>
    //    public static decimal InNumericYearMonth(this DateTime businessDate)
    //    {
    //        return (businessDate.Year * 100) + businessDate.Month;
    //    }

    //    /// <summary>
    //    /// Extends the RiskMarginPosition struct to get the right margin sign of a net quantity, according with the TIMS market specification.
    //    /// </summary>
    //    /// <remarks>
    //    /// The net assignation/exercise/liquidation quantities come with the right sign. no manipulation is required.
    //    /// The net short/long (and position actions) quantities come with an absolute value, 
    //    /// a manipulation is required according with the following rules: 
    //    /// <list type="">
    //    /// <item>For net short open quantities, the margin represents the liquidation costs and thus the corresponding margin requirements,
    //    /// which has positive sign. 
    //    /// </item>
    //    /// <item>On the other hand, the margin of net long quantities represents the liquidation proceeds and consequently the margin credit, 
    //    /// which has a NEGATIVE sign. </item>
    //    /// </list>
    //    /// </remarks>
    //    /// <param name="position">the quantity to transform</param>
    //    /// <param name="pQtyType">execution/assingation indicator</param>
    //    /// <param name="side">Side of the position, http://www.fixprotocol.org/FIXimate3.0/en/FIX.5.0SP2/tag54.html</param>
    //    /// <returns>the same quantity value, but with the right margin sign</returns>
    //    public static decimal QuantityWithMarginSign(this RiskMarginPosition position, RiskMethodQtyType pQtyType, string side)
    //    {
    //        // EG 20150920 [21374] Int (int32) to Long (Int64) 
    //        // EG 20170127 Qty Long To Decimal
    //        decimal signedquantity = 0;

    //        switch (pQtyType)
    //        {
    //            case RiskMethodQtyType.ExeAssCall:
    //            case RiskMethodQtyType.ExeAssPut:
    //            case RiskMethodQtyType.FutureMoff:

    //                signedquantity = position.ExeAssQuantity;

    //                break;


    //            case RiskMethodQtyType.PositionAction:
    //            case RiskMethodQtyType.Future:
    //            case RiskMethodQtyType.Call:
    //            case RiskMethodQtyType.Put:
    //            default:

    //                signedquantity = (side == "2") ?
    //                    position.Quantity
    //                    :
    //                    // long quantity
    //                    (-1) * position.Quantity;

    //                break;
    //        }

    //        return signedquantity;

    //    }

    //    /// <summary>
    //    /// Extends RiskMarginPosition struct in order to transcode from RiskMethodQtyType internal enum to Fix PosType enum
    //    /// </summary>
    //    /// <remarks>the case RiskMethodQtyType.PositionAction is treated partially, 
    //    /// beacuse no cross margin scenario can be recognized, attend to get PosType.PA ever. no PosType.XM can be returned</remarks>
    //    /// <param name="position">the position dataset</param>
    //    /// <param name="pQtyType">the quantity type we want to transcode to Fix </param>
    //    /// <returns></returns>
    //    public static PosType GetPosType(this RiskMarginPosition position, RiskMethodQtyType pQtyType)
    //    {
    //        // by default we consider allocations
    //        PosType type = default(PosType);

    //        switch(pQtyType) 
    //        {
    //            case RiskMethodQtyType.ExeAssCall:
    //            case RiskMethodQtyType.ExeAssPut:

    //                type = position.ExeAssQuantity > 0 ? PosType.AS : PosType.EX;

    //                break; 

    //            case RiskMethodQtyType.FutureMoff:
 
    //                type = PosType.DN;

    //                break;

    //            case RiskMethodQtyType.PositionAction:

    //                type = PosType.PA;

    //                break;

    //            case RiskMethodQtyType.Future:
    //            case RiskMethodQtyType.Call:
    //            case RiskMethodQtyType.Put:
    //            default:

    //                type =  PosType.ALC;

    //                break;
    //        }

    //        return type;


    //    }

    //    /// <summary>
    //    /// Fournit la valeur de l'enum RiskMethodQtyType correspondant pour la Catégorie de DerivativeContract et le type d'option Call/Put
    //    /// </summary>
    //    /// <param name="pCategory">Catégorie de DerivativeContract ('F'uture ou 'O'ption)</param>
    //    /// <param name="pPutOrCall">Indicateur FIX du type d'option Call ou Put pour une option</param>
    //    /// <returns></returns>
    //    public static RiskMethodQtyType GetTypeFromCategoryPutCall(Nullable<CfiCodeCategoryEnum> pCategory, Nullable<PutOrCallEnum> pPutOrCall)
    //    {
    //        RiskMethodQtyType type = default;

    //        switch (pCategory)
    //        {
    //            case CfiCodeCategoryEnum.Option:
    //                if (pPutOrCall == PutOrCallEnum.Put)
    //                {
    //                    type = RiskMethodQtyType.Put;
    //                }
    //                else if (pPutOrCall == PutOrCallEnum.Call)
    //                {
    //                    type = RiskMethodQtyType.Call;
    //                }
    //                break;
    //            case CfiCodeCategoryEnum.Future:
    //            default:
    //                type = RiskMethodQtyType.Future;
    //                break;
    //        }
    //        return type;
    //    }

    //    /// <summary>
    //    /// Calcul la valeur d'une position sur un contrat
    //    /// </summary>
    //    /// <param name="pQuantity">Quantité en position</param>
    //    /// <param name="pPrice">Prix unitaire du contrat</param>
    //    /// <param name="pMultiplier">Multiplieur du contrat</param>
    //    /// <param name="pInstrumentNum">Numérateur du prix du contrat</param>
    //    /// <param name="pInstrumentDen">Dénominateur du prix du contrat</param>
    //    /// <returns>La quantité valorisée</returns>
    //    // EG 20150920 [21374] Int (int32) to Long (Int64)  
    //    // EG 20170127 Qty Long To Decimal
    //    public static decimal ContractValue(decimal pQuantity, decimal pPrice, decimal pMultiplier, int pInstrumentNum, int pInstrumentDen)
    //    {
    //        if (pInstrumentNum <= 0) pInstrumentNum = 1;
    //        return pQuantity * pMultiplier * ExchangeTradedDerivativeTools.ToBase100(pPrice, pInstrumentNum, pInstrumentDen);
    //    }
    //    /// <summary>
    //    /// Calcul la valeur d'une position sur un contrat
    //    /// </summary>
    //    /// <param name="pQuantity">Quantité en position</param>
    //    /// <param name="pPrice">Prix unitaire du contrat</param>
    //    /// <param name="pMultiplier">Multiplieur du contrat</param>
    //    /// <param name="pInstrumentNum">Numérateur du prix du contrat</param>
    //    /// <param name="pInstrumentDen">Dénominateur du prix du contrat</param>
    //    /// <param name="pCashFlowCalculationMethod">Méthode d'arrondie de la valorisation</param>
    //    /// <param name="pRoundDir">Direction de l'arrondie</param>
    //    /// <param name="pRoundPrec">Précision de l'arrondie</param>
    //    /// <returns>La quantité valorisée</returns>
    //    /// PM 20150707 [21104] New
    //    // EG 20150920 [21374] Int (int32) to Long (Int64)  
    //    // EG 20170127 Qty Long To Decimal
    //    public static decimal ContractValue(decimal pQuantity, decimal pPrice, decimal pMultiplier, int pInstrumentNum, int pInstrumentDen, CashFlowCalculationMethodEnum pCashFlowCalculationMethod, string pRoundDir, int pRoundPrec)
    //    {
    //        if (pInstrumentNum <= 0) pInstrumentNum = 1;
    //        decimal price100 = ExchangeTradedDerivativeTools.ToBase100(pPrice, pInstrumentNum, pInstrumentDen);
    //        decimal value = ExchangeTradedDerivativeTools.CashFlowValorization(pCashFlowCalculationMethod, price100, 0, pMultiplier, pQuantity, pRoundDir, pRoundPrec);
    //        return value;
    //    }

    //    /// <summary>
    //    /// Calcul la valeur d'une position sur un contrat
    //    /// </summary>
    //    /// <param name="pAssetParameter">Caractèristique de l'asset</param>
    //    /// <param name="pQuantity">Quantité en position</param>
    //    /// <param name="pPrice">Prix unitaire du contrat</param>
    //    /// <returns>La quantité valorisée</returns>
    //    /// PM 20150707 [21104] New
    //    // EG 20150920 [21374] Int (int32) to Long (Int64)  
    //    // EG 20170127 Qty Long To Decimal
    //    internal static decimal ContractValue(AssetExpandedParameter pAssetParameter, decimal pQuantity, decimal pPrice)
    //    {
    //        return ContractValue(pQuantity, pPrice, pAssetParameter.ContractMultiplier, pAssetParameter.InstrumentNum, pAssetParameter.InstrumentDen, pAssetParameter.CashFlowCalcMethodEnum, pAssetParameter.RoundDir, pAssetParameter.RoundPrec);
    //    }

    //    /// <summary>
    //    /// Calcul la valeur d'une position sur un contrat
    //    /// </summary>
    //    /// <param name="pAssetParameter">Caractèristique de l'asset</param>
    //    /// <param name="pQuantity">Quantité en position</param>
    //    /// <param name="pPrice">Prix unitaire du contrat</param>
    //    /// <param name="pUseContractValueFactor">Indique s'il faut utiliser le ContractValueFactor</param>
    //    /// <param name="pContractValueFactor">ContractValueFactor</param>
    //    /// <returns>La quantité valorisée</returns>
    //    /// PM 20150707 [21104] New
    //    // EG 20150920 [21374] Int (int32) to Long (Int64)  
    //    // EG 20170127 Qty Long To Decimal
    //    internal static decimal ContractValue(AssetExpandedParameter pAssetParameter, decimal pQuantity, decimal pPrice, bool pUseContractValueFactor, decimal pContractValueFactor)
    //    {
    //        decimal cvf = (pUseContractValueFactor && (pContractValueFactor != 0)) ? pContractValueFactor : pAssetParameter.ContractMultiplier;
    //        return ContractValue(pQuantity, pPrice, cvf, pAssetParameter.InstrumentNum, pAssetParameter.InstrumentDen, pAssetParameter.CashFlowCalcMethodEnum, pAssetParameter.RoundDir, pAssetParameter.RoundPrec);
    //    }

    //    /// <summary>
    //    /// Delete all the actor ids (we found in position) related to the current session
    //    /// </summary>
    //    /// <param name="pHierarchy">instance object (not used by now)</param>
    //    /// <param name="pCS">the connection string to the DB</param>
    //    /// <param name="pSessionId">session id of the current application instance</param>
    //    /// <remarks>20120712 MF Ticket 18004</remarks>
    //    // EG 20180803 PERF New
    //    public static void TruncateImActor(this ActorRoleHierarchy pHierarchy, string pCS)
    //    {
    //        string queryTruncate = DataContractHelper.GetQuery(DataContractResultSets.TRUNCATEIMACTOR);

    //        if (String.IsNullOrEmpty(queryTruncate))
    //        {
    //            return;
    //        }

    //        CommandType queryType = DataContractHelper.GetType(DataContractResultSets.TRUNCATEIMACTOR);
    //        using (IDbConnection connection = DataHelper.OpenConnection(pCS))
    //        {
    //            DataHelper.ExecuteNonQuery(connection, queryType, queryTruncate);
    //        }
    //    }

    //    /// <summary>
    //    /// Insert all the loaded actors inside the IMACTOR table
    //    /// </summary>
    //    /// <param name="pHierarchy">instance object</param>
    //    /// <param name="pCS">the connection string to the DB</param>
    //    /// <param name="pSessionId">session id of the current application instance</param>
    //    /// <remarks>20120712 MF Ticket 18004</remarks>
    //    // EG 20180803 PERF Suppresion SESSIONID non utilisée avec IMACTOR_{BuildTableId}_W, IMACTOR_{BuildTableId}_W, IMASSET_ETD_{BuildTableId}_W
    //    // EG 20181119 PERF Correction post RC (Step 2)
    //    public static void InsertImActor(this ActorRoleHierarchy pHierarchy, string pCS, SvrInfoConnection pSvrInfoConnection, string pTableId)
    //    {
    //        string queryInsert = DataContractHelper.GetQuery(DataContractResultSets.INSERTIMACTOR);

    //        CommandType queryType = DataContractHelper.GetType(DataContractResultSets.INSERTIMACTOR);

    //        List<ActorNode> actors = new List<ActorNode>();
    //        actors.Add(pHierarchy.Root);
    //        // Using Union instead AddRange to ge rid of elements duplicated
    //        actors = actors.Union(pHierarchy.Root.FindChilds(actornode => actornode != null && actornode.Built)).ToList();
    //        Dictionary<string, object> dbParameterValues = new Dictionary<string, object>();

    //        using (IDbConnection connection = DataHelper.OpenConnection(pCS))
    //        {
    //            foreach (ActorNode actor in actors)
    //            {
    //                int idA = actor.Id;
    //                // UNDONE 20120712 MF the book is actually not used (propose the column suppression onto IMACTOR table)
    //                int idB = 0;
    //                dbParameterValues.Add("IDA", idA);
    //                dbParameterValues.Add("IDB", idB);

    //                DataHelper.ExecuteNonQuery(connection, queryType, queryInsert,
    //                    DataContractHelper.GetDbDataParameters(DataContractResultSets.INSERTIMACTOR, dbParameterValues));

    //                dbParameterValues.Remove("IDA");
    //                dbParameterValues.Remove("IDB");
    //            }
    //        }
    //        if (pSvrInfoConnection.isOracle)
    //            DataHelper.UpdateStatTable(pCS, String.Format("IMACTOR_{0}_W", pTableId).ToUpper());
    //    }
    //}   


}