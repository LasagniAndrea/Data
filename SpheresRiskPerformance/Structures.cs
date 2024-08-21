using System;
using System.Xml.Serialization;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Filtering and grouping modes for net positions
    /// </summary>
    internal enum GroupingNetPositions
    {
        /// <summary>
        /// Considering a Clearing HOuse aggregation , for each book involved 
        ///     in the risk evaluation, we extract its net positions on trades owned by the Clearing House passed as argument.
        /// </summary>
        ClearingHouse,
        /// <summary>
        /// when we consider the Market aggregation , for each book involved 
        ///     in the risk evaluation, we extract its net positions on trades on derivative contracts owned by the market passed as argument.
        /// </summary>
        [Obsolete("Eurosys legacy, le tri par marché n'est peut-être pas utile pour Spheres", false)]
        Market,
    }

    /// <summary>
    /// Evaluation mode to specify a serialisation type for the risk evaluation results (trades initial margin)
    /// </summary>
    internal enum RiskEvaluationMode
    { 
        /// <summary>
        /// Choosing a normal mode we serialize the risk evaluation results as trade "initial margin".
        /// </summary>
        Normal,
        /// <summary>
        /// Choosing a simulation mode we serialize the risk evaluation results as trade "initial margin", 
        ///     but flagging thoses trades as virtual.
        /// </summary>
        Simulation,
    }

    /// <summary>
    /// Risk evaluation timing 
    /// </summary>
    /// <remarks>
    /// Do not confuse with SettlSessIDEnum (the risk timing enum contains just two items)
    /// </remarks>
    public enum RiskEvaluationTiming
    {
        /// <summary>
        /// The risk evaluation is done at the end of the day
        /// </summary>
        [XmlEnum("EOD")]
        EndOfDay,
        /// <summary>
        /// The risk evaluation is done during the business hours 
        /// </summary>
        /// <remarks>a new evaluaton with EndOfDay timing will be performed at the end of the business day</remarks>
        [XmlEnum("IDY")]
        Intraday,
    }

    /// <summary>
    /// Existing risk revaluation mode
    /// </summary>
    public enum RiskRevaluationMode
    { 
        /// <summary>
        /// The risk evaluation of the existing risk will not be performed
        /// </summary>
        DoNotEvaluate,
        /// <summary>
        /// A new risk evaluation will be added to the existing risk evaluations
        /// </summary>
        NewEvaluation,
        /// <summary>
        /// A new risk evaluation will replace the existing risk evaluation
        /// </summary>
        EvaluateWithUpdate
    }

    /// <summary>
    /// Type of evaluation function in order to evaluate a position set, owned by a calculation element
    /// </summary>
    public enum RiskElementEvaluation
    {
        /// <summary>
        /// Deposit evaluated over the global position, default value
        /// </summary>
        SumPosition = 0,
        /// <summary>
        /// Deposit evaluated over the sum of the deposits
        /// </summary>
        SumDeposit,
    }

    /// <summary>
    /// Type of risk element
    /// </summary>
    public enum RiskElementClass
    {
        /// <summary>
        /// Default, calculation element containing the positions set of one or more actor/book, 
        /// to be used to evaluate a risk element type of DEPOSIT
        /// </summary>
        ELEMENT,
        /// <summary>
        /// Calculation element generating a deposit record in the environment, 
        /// and containing the positions set of an actor/book affected by the deposit 
        /// if it is not already there in some inherited elements type of ELEMENT
        /// </summary>
        DEPOSIT,
        /// <summary>
        /// Additional calculation element built for an actor/book when the OnAllBook flag is true, 
        /// generating a deposit record in the environment, 
        /// and containing the positions set of the actor/book affected by the deposit 
        /// </summary>
        ADDITIONALDEPOSIT
    }

    /// <summary>
    /// Recognized risk methods 
    /// </summary>
    public enum RiskMethodType
    {
        /// <summary>
        /// Standard method
        /// </summary>
        STANDARD
    }

    /// <summary>
    /// parameters types for the standard method
    /// </summary>
    public enum ExpressionTypeStandardMethod
    {
        /// <summary>
        /// The parameter is used as a stright multiplier for the quantity in position
        /// </summary>
        [XmlEnum("FixedAmount")]
        FixedAmount,
        /// <summary>
        /// The parameter is used as a percentage of the contract value (contract value => "Quantity * Contract Multiplier * Quote price")
        /// </summary>
        [XmlEnum("Percentage")]
        Percentage
    }

    /// <summary>
    /// Parameters collection for the risk margin evaluation
    /// </summary>
    /// <remarks>
    /// This collection is built parsing the input request received from the service mqueue
    /// </remarks>
    internal struct RiskPerformanceProcessInfo
    {
        /// <summary>
        /// Net positions grouping type
        /// </summary>
        internal GroupingNetPositions Grouping;

        /// <summary>
        /// Clearing House or Market Identifier, depending by the Grouping value
        /// </summary>
        internal string GroupingIdentifier;

        /// <summary>
        /// Delete the previous evaluation
        /// </summary>
        internal bool Reset;

        /// <summary>
        /// Evaluation mode
        /// </summary>
        internal RiskEvaluationMode Mode;

        /// <summary>
        /// Risk Timing
        /// </summary>
        internal RiskEvaluationTiming Timing;

        /// <summary>
        /// internal id of the entity for which the risk evaluation request is generated
        /// </summary>
        internal int Entity;

        /// <summary>
        /// internal id of the actor MARGINREQOFFICE for which the risk evaluation request is generated 
        /// </summary>
        /// <value>is optional, can be null</value>
        internal Nullable<int> MarginReqOfficeChild;
    }
}