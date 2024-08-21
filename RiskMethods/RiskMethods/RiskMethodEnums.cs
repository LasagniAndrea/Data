using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// which type of hierarchy generated the deposit request: Clearer or Entity
    /// </summary>
    public enum DepositHierarchyClass
    {
        /// <summary>
        /// unknown hierarchy
        /// </summary>
        UNKNOWN,
        /// <summary>
        /// deposit owned by a marginregoffice actor descending from an Entity
        /// </summary>
        ENTITY,
        /// <summary>
        /// deposit owned by a marginregoffice actor descending from a Clearer (Broker, Clearing House, etc...)
        /// </summary>
        CLEARER
    }

    /// Actual evaluation status of a deposit
    /// </summary>
    public enum DepositStatus
    {
        /// <summary>
        /// not yet evaluated
        /// </summary>
        NOTEVALUATED,
        /// <summary>
        /// Evaluating...
        /// </summary>
        EVALUATING,
        /// <summary>
        /// Evaluated without errors
        /// </summary>
        EVALUATED,
    }

    /// <summary>
    /// Strategy of position reduction for a specific derivative market
    /// </summary>
    [DataContract(Name = "PosStockCoverEnum")]
    public enum PosStockCoverEnum
    {
        /// <summary>
        /// Coverage for Futures and Options sorted by quote desc, maturity desc and strike asc
        /// </summary>
        /// <remarks>
        /// EUROSYS® : COUV_OPT_OR_FUT
        /// </remarks>
        [XmlEnum("Default")]
        [EnumMember(Value = "Default")]
        Default = 0,
        /// <summary>
        /// Priority to Futures, assets ordered by quantity desc, maturity desc (for Futures) and quote desc, maturity desc and strike asc (for Options)
        /// </summary>
        /// <remarks>
        /// EUROSYS® : COUV_STOCK_ALL - FUT/OPT
        /// </remarks>
        [XmlEnum("PriorityStockFuture")]
        [EnumMember(Value = "PriorityStockFuture")]
        PriorityStockFuture = 1,
        /// <summary>
        /// Priority to Options, assets ordered by quote desc, maturity desc and strike asc (for Options) and quantity desc, maturity desc (for Futures)
        /// </summary>
        /// <remarks>
        /// EUROSYS® : COUV_STOCK_ALL - OPT/FUT
        /// </remarks>
        [XmlEnum("PriorityStockOption")]
        [EnumMember(Value = "PriorityStockOption")]
        PriorityStockOption = 2,
        /// <summary>
        /// Futures coverage only, assets ordered by quantity desc, maturity desc
        /// </summary>
        /// <remarks>
        /// EUROSYS® : COUV_STOCK_FUTURE
        /// </remarks>
        [XmlEnum("StockFuture")]
        [EnumMember(Value = "StockFuture")]
        StockFuture = 3,
        /// <summary>
        /// Options coverage only, assets ordered by quote desc, maturity desc and strike asc
        /// </summary>
        /// <remarks>
        /// EUROSYS® : COUV_STOCK_OPTION
        /// </remarks>
        [XmlEnum("StockOption")]
        [EnumMember(Value = "StockOption")]
        StockOption = 4,
        /// <summary>
        /// No coverage
        /// </summary>
        /// <remarks>
        /// No relative value inside EUROSYS®??
        /// </remarks>
        [XmlEnum("None")]
        [EnumMember(Value = "None")]
        None = 5,
    }

    /// <summary>
    /// Type de données utilisées par une méthode pour l'évaluation du déposit
    /// </summary>
    /// PM 20170313 [22833] Ajout enum RiskMethodDataType
    public enum RiskMethodDataTypeEnum
    {
        /// <summary>
        /// Calcul du déposit sur la position
        /// </summary>
        Position,
        /// <summary>
        /// Calcul du déposit sur la valeur des trades
        /// </summary>
        TradeValue,
        /// <summary>
        /// Trades sans calcul de déposit
        /// </summary>
        // PM 20221212 [XXXXX] Ajout
        TradeNoMargin,
    }

    /// <summary>
    /// define the type of the quantity for a specific deposit factor, 
    /// the quantity type is usally attached to a net by asset position
    /// </summary>
    public enum RiskMethodQtyType
    {
        /// <summary>
        /// type put (option), for a quantity related to an open position
        /// </summary>
        Put,
        /// <summary>
        /// type call (option), for a quantity related to an open position
        /// </summary>
        Call,
        /// <summary>
        /// type future , for a quantity related to an open position
        /// </summary>
        Future,
        /// <summary>
        /// type put (option) , for a quantity related to a delivery position 
        /// (an execution/assignation has been performed on an open position on a contract in physical delivery)
        /// </summary>
        ExeAssPut,
        /// <summary>
        /// type call (option) , for a quantity related to a delivery position 
        /// (an execution/assignation has been performed on an open position on a contract in physical delivery)
        /// </summary>
        ExeAssCall,
        /// <summary>
        /// type future, for a quantity related to a position on a future contracti in physical delivery that has passed its expiration date 
        /// </summary>
        FutureMoff,
        /// <summary>
        /// type position on action, for position connected directly to stock asset, not ETD contracts
        /// </summary>
        PositionAction,
    }

    /// <summary>
    /// SPAN Account type for deposit evaluation
    /// </summary>
    [DataContract(Name = "SpanAccountType")]
    public enum SpanAccountType
    {
        /// <summary>
        /// Null account type
        /// </summary>
        Default = 0,
        /// <summary>
        /// Hedger
        /// </summary>
        [XmlEnum("H")]
        [EnumMember(Value = "H")]
        Hedger,
        /// <summary>
        /// Member
        /// </summary>
        [XmlEnum("M")]
        [EnumMember(Value = "M")]
        Member,
        /// <summary>
        /// Normal
        /// </summary>
        [XmlEnum("N")]
        [EnumMember(Value = "N")]
        Normal,
        /// <summary>
        /// Omnibus/Speculator
        /// </summary>
        [XmlEnum("O")]
        [EnumMember(Value = "O")]
        OmnibusSpeculator,
        /// <summary>
        /// Omnibus/Hedger
        /// </summary>
        [XmlEnum("Q")]
        [EnumMember(Value = "Q")]
        OmnibusHedger,
        /// <summary>
        /// Speculator
        /// </summary>
        [XmlEnum("S")]
        [EnumMember(Value = "S")]
        Speculator,
    }
}
