using System.Xml.Serialization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;

using EFS.SpheresRiskPerformance.Hierarchies;
using EFS.Common;

using EfsML.Interface;

using FpML.v44.Shared;
using EfsML.v30.MarginRequirement;
using EFS.SpheresRiskPerformance.CalculationSheet;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    
    /// <summary>
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

        Pair<int, int>[]  m_PairsActorBookConstitutingPosition;

        /// <summary>
        /// Actors/books in posiiton for the current deposit
        /// </summary>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlArray("ActorsBooks")]
        public Pair<int, int>[] PairsActorBookConstitutingPosition
        {
            get { return m_PairsActorBookConstitutingPosition; }
            set { m_PairsActorBookConstitutingPosition = value; }
        }

        /// <summary>
        /// Deposit parameters, used to buidl the log
        /// </summary>
        /// <remarks>not serialized at the moment</remarks>
        [ReadOnly(true)]
        [Browsable(false)]
        [XmlIgnore]
        public ICalculationSheetMethod MarginCalculationMethod
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
        /// <param name="pActorBookAncestors">ancestors of the actor affected by the current deposit</param>
        public Deposit(
            RiskElement pRoot,
            bool pIsGrossMargining,
            IEnumerable<RiskElement> pFactors,
            IEnumerable<Pair<int, int>> pPairsActorBookConstitutingPosition,
            TradeRisk pPrevResult,
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

            Status = DepositStatus.NOTEVALUATED;
        }

        /// <summary>
        /// Get a well initialized deposit element
        /// </summary>
        /// <param name="pRoot">starting element</param>
        /// <param name="pIsGrossMargining">gross margining evaluation when true</param>
        /// <param name="pFactors">elements (inherited and directly owned) contributing to evaluate the deposit</param>
        /// <param name="pPrevResult">Result object of the previous evaluation, may be null</param>
        /// <param name="pPairsActorBookConstitutingPosition">actors books contributing to the deposit evaluation</param>
        public Deposit(
           RiskElement pRoot,
           bool pIsGrossMargining,
           IEnumerable<RiskElement> pFactors,
           IEnumerable<Pair<int, int>> pPairsActorBookConstitutingPosition,
           TradeRisk pPrevResult) :
            this(pRoot, pIsGrossMargining, pFactors, pPairsActorBookConstitutingPosition, pPrevResult, null)
        {

        }
    }

}