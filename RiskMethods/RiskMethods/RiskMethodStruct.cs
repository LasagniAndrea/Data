using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Hierarchies;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class de stockage de la cotation d'un asset
    /// </summary>
    public sealed class AssetQuoteParameter
    {
        /// <summary>
        /// Underlying Asset Id
        /// </summary>
        public int AssetId;

        /// <summary>
        /// Underlying Asset Category
        /// </summary>
        public Cst.UnderlyingAsset AssetCategoryEnum;

        /// <summary>
        /// Cotation
        /// </summary>
        public decimal Quote;

        /// <summary>
        /// Données reçues lors de la lecture de la cotation
        /// </summary>
        public string IdMarketEnv;
        /// <summary>
        /// 
        /// </summary>
        public string IdValScenario;
        /// <summary>
        /// Adjusted Time
        /// </summary>
        public DateTime AdjustedTime;
        /// <summary>
        /// Quote Side
        /// </summary>
        public string QuoteSide;
        /// <summary>
        /// Quote Timing
        /// </summary>
        public string QuoteTiming;
        /// <summary>
        /// Message de retour de la lecture de cotation
        /// </summary>
        public SystemMSGInfo SystemMsgInfo;
    }

    /// <summary>
    /// Struct including the mandatory values to enable different coverage strategies
    /// </summary>
    /// <remarks>
    /// <seealso cref="EFS.SpheresRiskPerformance.RiskMethods.BaseMethod.ReducePosition"/>
    /// </remarks>
    public struct CoverageSortParameters
    {
        /// <summary>
        /// Identifies the sort parameter.
        /// internal id of the underlying asset to link the sort parameter to the ETD asset in position  and the relative stock parameter
        /// </summary>
        public int AssetId;

        /// <summary>
        /// Identifies the sort parameter.
        /// Used to get the relative contract category 
        /// </summary>
        public RiskMethodQtyType Type;

        /// <summary>
        /// internal id of the etd contract to link the sort parameter to the to the relative stock parameter,
        /// in case one specific stock parameter exists for the derivative contract
        /// </summary>
        public int ContractId;

        /// <summary>
        /// ETD asset quote (J-1), used to sort the positions 
        /// </summary>
        public decimal Quote;

        /// <summary>
        /// Maturity rule, used to sort the positions
        /// </summary>
        public decimal MaturityYearMonth;

        /// <summary>
        /// Struke price, used to sort the positions
        /// </summary>
        public decimal StrikePrice;

        /// <summary>
        /// Contract multiplier
        /// </summary>
        public decimal Multiplier;
    }

    /// <summary>
    /// Specific market risk parameters
    /// </summary>
    /// <remarks>Add members as will to satisfy new methods needs</remarks>
    public struct MarketParameter
    {
        /// <summary>
        /// Market internal id
        /// </summary>
        public int MarketId;

        /// <summary>
        /// Cross margin activiaton for the current market (and entity)
        /// </summary>
        public bool CrossMarginActivated;

        /// <summary>
        /// Default coverage strategy
        /// </summary>
        public PosStockCoverEnum StockCoverageType;

        /// <summary>
        /// Business Center
        /// </summary>
        public string BusinessCenter;
    }

    /// <summary>
    /// Specific actor (role: MarginReqOffice) risk parameters
    /// </summary>
    /// <remarks>
    /// Add members as will to satisfy new methods needs.</remarks>
    public class MarginReqOfficeParameter
    {
        /// <summary>
        /// internal Id of the marginreqoffice actor which use this set of parameters
        /// </summary>
        /// <value>a valid Spheres id actor value, 0 values are not allowed</value>
        public int ActorId;

        /// <summary>
        /// internal Id of the marginreqoffice book where this set of parameters is active
        /// </summary>
        /// <value>When 0 then the parameters set is valid for all the actor's books</value>
        public int BookId;

        /// <summary>
        /// Internal Id of the clearing house where this set of parameters is active
        /// </summary>
        /// <value>When 0 then the parameters set is valid for all the clearing house</value>
        public int CssId;

        /// <summary>
        /// Internal Id of the market where this set of parameters is active
        /// </summary>
        /// <value>When 0 then the parameters set is valid for all the markets</value>
        public int MarketId;

        /// <summary>
        /// Default coverage strategy fro the current actor
        /// </summary>
        public PosStockCoverEnum StockCoverageType;

        /// <summary>
        /// Multiplier ratio affecting the final deposit amount
        /// </summary>
        /// <value>when -1 the value does not exist and must not be considered, 
        /// when >= 0 the value is valid and mus replace the other ratios</value>
        public decimal WeightingRatio;

        /// <summary>
        /// SPAN Account type
        /// </summary>
        public SpanAccountType SpanAccountType;

        /// <summary>
        /// Flag indicator to include the maintenance amount for a deposit calculate with the SPAN method.
        /// When true the amount will be considered.
        /// </summary>
        public bool SpanMaintenanceAmountIndicator;

        /// <summary>
        /// SPAN Scan Risk Offset Cap Percentage's
        /// </summary>
        /// PM 20150930 [21134] Add ScanRiskOffsetCapPrct
        public decimal ScanRiskOffsetCapPrct;

        /// <summary>
        /// Indique s'il faut calculer un déposit pour les positions en attente de livraison (exercée/assignée) lorsque la méthode de calcul le permet
        /// </summary>
        /// PM 20170106 [22633] Add IsInitialMarginForExeAssPosition
        public bool IsInitialMarginForExeAssPosition;

        /// <summary>
        /// Constructeur de la structure
        /// </summary>
        public MarginReqOfficeParameter()
        {
            // PM 20170106 [22633] Initialisation de IsInitialMarginForExeAssPosition à True pour rétrocompatibilité
            IsInitialMarginForExeAssPosition = true;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public struct RiskMarginPositionAction
    {
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity;

        public decimal LongCrtVal;

        public decimal ShortCrtVal;

        public string Currency;
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public RiskMarginPositionAction(decimal pQuantity, decimal pLongCrtVal, decimal pShortCrtVal, string pCurrency)
        {
            this.Quantity = pQuantity;

            this.LongCrtVal = pLongCrtVal;

            this.ShortCrtVal = pShortCrtVal;

            this.Currency = pCurrency;
        }
    }

    /// <summary>
    /// key identifying a position action
    /// </summary>
    /// <remarks>the key has not a comparer associated, DO NOT use inside Dictonary collection or any hash table</remarks>
    public class PosActionRiskMarginKey : PosRiskMarginKey
    {
        /// <summary>
        /// Symbol of the equity contract for the stocks in position
        /// </summary>
        public string derivativeContractSymbol;

        /// <summary>
        /// maturity date of the position action (expressed in day/month/year)
        /// </summary>
        public DateTime maturityDate;

        /// <summary>
        /// Get the identifier of the position action (the identiier is composed by the symbol + the maturity date)
        /// </summary>
        public string PosActionIdentifier
        {
            get
            {
                return String.Concat(
                    derivativeContractSymbol,
                    " ",
                    maturityDate > DateTime.MinValue
                        ? maturityDate.ToShortDateString() : Cst.NotAvailable);
            }
        }
    }

    /// <summary>
    /// Class representing a deposit
    /// </summary>
    /// <remarks>any deposit object will be used by one method, clearing house only</remarks>
    public sealed class Deposit
    {
        /// <summary>
        /// Current evaluation status
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlAttribute(AttributeName = "grossmargining")]
        public bool IsGrossMargining { get; set; }

        /// <summary>
        /// Current evaluation status
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlAttribute("status")]
        public DepositStatus Status { get; set; }

        List<Money> m_Amounts = new List<Money>();

        /// <summary>
        /// Amounts of the deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public List<Money> Amounts
        {
            get { return m_Amounts; }
            set { m_Amounts = value; }
        }

        /// <summary>
        /// Weighting ratio amounts of the deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public List<Money> WeightingRatioAmounts
        {
            get;
            set;
        }

        /// <summary>
        /// root element (in other words the deposit item extracted from the factors list )
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public RiskElement Root
        {
            get;
            set;
        }

        List<RiskElement> m_Factors;

        /// <summary>
        /// Elements contributing to evaluate the deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray]
        public List<RiskElement> Factors
        {
            get { return m_Factors; }
            set { m_Factors = value; }
        }

        // PM 20170313 [22833] Changement de type de m_PairsActorBookConstitutingPosition : Pair<int, int>[] => RiskActorBook[]
        //Pair<int, int>[]  m_PairsActorBookConstitutingPosition;

        ///// <summary>
        ///// Actors/books in position for the current deposit
        ///// </summary>
        //[ReadOnly(true)]
        //[Browsable(false)]
        //[XmlArray("ActorsBooks")]
        //public Pair<int, int>[] PairsActorBookConstitutingPosition
        //{
        //    get { return m_PairsActorBookConstitutingPosition; }
        //    set { m_PairsActorBookConstitutingPosition = value; }
        //}

        RiskActorBook[] m_PairsActorBookConstitutingPosition;

        /// <summary>
        /// Actors/books in position for the current deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray("ActorsBooks")]
        public RiskActorBook[] PairsActorBookConstitutingPosition
        {
            get { return m_PairsActorBookConstitutingPosition; }
            set { m_PairsActorBookConstitutingPosition = value; }
        }

        /// <summary>
        /// Deposit main communication object, used to build the calculation sheet (log)
        /// </summary>
        /// <remarks>not serialized at the moment</remarks>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlIgnore]
        public IMarginCalculationMethodCommunicationObject[] MarginCalculationMethods
        {
            get;
            set;
        }

        List<Pair<int, int>> m_ActorBookAncestors;

        /// <summary>
        /// ancestors of the actor affected by the current deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public List<Pair<int, int>> ActorBookAncestors
        {
            get { return m_ActorBookAncestors; }
            set { m_ActorBookAncestors = value; }
        }

        /// <summary>
        /// Result of the previous evaluation, may be null
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        public TradeRisk PrevResult
        {
            get;
            set;
        }

        /// <summary>
        /// which type of hierarchy generated the deposit request: Clearer or Entity
        /// </summary>
        [XmlAttribute("originatingfrom")]
        public DepositHierarchyClass HierarchyClass
        {
            get;
            set;
        }

        /// <summary>
        /// Flag to signal that a deposit is not in position
        /// </summary>
        [XmlAttribute("notinposition")]
        public bool NotInPosition
        {
            get;
            set;
        }

        /// <summary>
        /// Weighting ratio for the deposit object
        /// </summary>
        [XmlAttribute("wratio")]
        public decimal WeightingRatio
        {
            get;
            set;
        }

        /// <summary>
        /// Indique si un deposit a été calculé de façon incomplete (ou même pas calculé)
        /// </summary>
        // PM 20220202 Ajout IsIncomplete
        [XmlAttribute("isIncomplete")]
        public bool IsIncomplete
        { get; set; }

        /// <summary>
        /// Get an empty RiskEvaluationElement, serialization purpose only
        /// </summary>
        public Deposit()
        { }

        /// <summary>
        /// Get a well initialized deposit element
        /// </summary>
        /// <param name="pRoot">starting element</param>
        /// <param name="pIsGrossMargining">gross margining evaluation when true</param>
        /// <param name="pFactors">elements (inherited and directly owned) contributing to evaluate the deposit</param>
        /// <param name="pPairsActorBookConstitutingPosition">actors books contributing to the deposit evaluation</param>
        /// <param name="pPrevResult">Result object of the previous evaluation, may be null</param>
        /// <param name="pHierarchyClass">is the deposit affecting an actor descending from an actor entity or from a clearer?</param>
        /// <param name="pActorBookAncestors">ancestors of the actor affected by the current deposit</param>
        /// <param name="pWeightingRatio">multiplication factor of the deposit amount</param>
        /// PM 20170313 [22833] Changement de type du paramètre pPairsActorBookConstitutingPosition : IEnumerable<Pair<int, int>> => IEnumerable<RiskActorBook>
        public Deposit(
            RiskElement pRoot,
            bool pIsGrossMargining,
            IEnumerable<RiskElement> pFactors,
            //IEnumerable<Pair<int, int>> pPairsActorBookConstitutingPosition,
            IEnumerable<RiskActorBook> pPairsActorBookConstitutingPosition,
            DepositHierarchyClass pHierarchyClass,
            TradeRisk pPrevResult,
            decimal pWeightingRatio,
            IEnumerable<Pair<int, int>> pActorBookAncestors)
        {
            Root = pRoot;

            IsGrossMargining = pIsGrossMargining;

            if (pFactors != null)
            {
                Factors = pFactors.ToList();
            }

            if (pPairsActorBookConstitutingPosition != null)
            {
                PairsActorBookConstitutingPosition = pPairsActorBookConstitutingPosition.ToArray();
            }

            if (pActorBookAncestors != null)
            {
                ActorBookAncestors = pActorBookAncestors.ToList();
            }

            PrevResult = pPrevResult;

            WeightingRatio = pWeightingRatio;

            HierarchyClass = pHierarchyClass;

            Status = DepositStatus.NOTEVALUATED;
        }

        /// <summary>
        /// Get a well initialized deposit element
        /// </summary>
        /// <param name="pRoot">starting element</param>
        /// <param name="pIsGrossMargining">gross margining evaluation when true</param>
        /// <param name="pFactors">elements (inherited and directly owned) contributing to evaluate the deposit</param>
        /// <param name="pPrevResult">Result object of the previous evaluation, may be null</param>
        /// <param name="pHierarchyClass">is the deposit affecting an actor descending from an actor entity or from a clearer?</param>
        /// <param name="pPairsActorBookConstitutingPosition">actors books contributing to the deposit evaluation</param>
        /// <param name="pWeightingRatio">multiplication factor of the deposit amount</param>
        /// PM 20170313 [22833] Changement de type de pPairsActorBookConstitutingPosition : IEnumerable<Pair<int, int>> => IEnumerable<RiskActorBook>
        public Deposit(
           RiskElement pRoot,
           bool pIsGrossMargining,
           IEnumerable<RiskElement> pFactors,
           IEnumerable<RiskActorBook> pPairsActorBookConstitutingPosition,
           DepositHierarchyClass pHierarchyClass,
           TradeRisk pPrevResult,
           decimal pWeightingRatio) :
           this(pRoot, pIsGrossMargining, pFactors, pPairsActorBookConstitutingPosition, pHierarchyClass, pPrevResult, pWeightingRatio, null)
        {

        }


    }
}
