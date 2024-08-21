using System.Xml.Serialization;

namespace EFS.SpheresRiskPerformance.Enum
{
    /// <summary>
    /// Evaluation mode to specify a serialisation type for the risk evaluation results (trades initial margin)
    /// </summary>
    public enum RiskEvaluationMode
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
        /// it can contains the positions set of an actor/book (which one affected by or a counterparty of this one) 
        /// if this set is not already there in some inherited elements type of ELEMENT
        /// </summary>
        DEPOSIT,
        /// <summary>
        /// Additional calculation element built for an actor/book when the OnAllBook flag is true, 
        /// generating a deposit record in the environment 
        /// and containing the positions set of the actor/book affected by the deposit 
        /// </summary>
        ADDITIONALDEPOSIT
    }

    /// <summary>
    /// Standard parameters types 
    /// </summary>
    // PM 20190401 [24625][24387] Ajout ECC_AMBO & NA
    public enum ExpressionType
    {
        /// <summary>
        /// Non applicable
        /// </summary>
        [XmlEnum("NA")]
        NA,
        /// <summary>
        /// The parameter is used as a stright multiplier for the quantity in position
        /// </summary>
        [XmlEnum("FixedAmount")]
        FixedAmount,
        /// <summary>
        /// The parameter is used as a percentage of the contract value 
        /// </summary>
        [XmlEnum("Percentage")]
        Percentage,
        /// <summary>
        /// Valeur pour le calcul de l'Additional Margin BoM (AMBO) de l'ECC
        /// </summary>
        [XmlEnum("ECC_AMBO")]
        ECC_AMBO
    }
}
