using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Spheres.DataContracts;
using EFS.SpheresRiskPerformance.CommunicationObjects;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using EfsML.Business;
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Volatility Scenario = un point de la courbe
    /// </summary>
    internal sealed class NOMXCFMVolatilityScenario
    {
        #region Members
        /// <summary>
        /// Point Number for PC1
        /// </summary>
        private readonly int m_PC1PointNo;
        /// <summary>
        /// Point Number for PC2
        /// </summary>
        private readonly int m_PC2PointNo;
        /// <summary>
        /// Point Number for PC3
        /// </summary>
        private readonly int m_PC3PointNo;
        /// <summary>
        /// Low
        /// </summary>
        private readonly decimal m_LowValue;
        /// <summary>
        /// Middle
        /// </summary>
        private readonly decimal m_MiddleValue;
        /// <summary>
        /// High
        /// </summary>
        private readonly decimal m_HighValue;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Point Number for PC1
        /// </summary>
        public int PC1PointNo { get { return m_PC1PointNo; } }
        /// <summary>
        /// Point Number for PC2
        /// </summary>
        public int PC2PointNo { get { return m_PC2PointNo; } }
        /// <summary>
        /// Point Number for PC3
        /// </summary>
        public int PC3PointNo { get { return m_PC3PointNo; } }
        /// <summary>
        /// Low
        /// </summary>
        public decimal LowValue { get { return m_LowValue; } }
        /// <summary>
        /// Middle
        /// </summary>
        public decimal MiddleValue { get { return m_MiddleValue; } }
        /// <summary>
        /// High
        /// </summary>
        public decimal HighValue { get { return m_HighValue; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPC1">Coordonnée pour PC1</param>
        /// <param name="pPC2">Coordonnée pour PC2</param>
        /// <param name="pPC3">Coordonnée pour PC3</param>
        /// <param name="pLow">Valeur Low</param>
        /// <param name="pMiddle">Valeur Middle</param>
        /// <param name="pHigh">Valeur High</param>
        public NOMXCFMVolatilityScenario(int pPC1, int pPC2, int pPC3, decimal pLow, decimal pMiddle, decimal pHigh)
        {
            m_PC1PointNo = pPC1;
            m_PC2PointNo = pPC2;
            m_PC3PointNo = pPC3;
            m_LowValue = pLow;
            m_MiddleValue = pMiddle;
            m_HighValue = pHigh;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Retourne le minimum des valeurs Low, Middle et High
        /// </summary>
        /// <returns></returns>
        public decimal MinValue()
        {
            return System.Math.Min(System.Math.Min(LowValue, MiddleValue), HighValue);
        }

        /// <summary>
        /// Retourne le numéro d'enregistrement d'un scenario
        /// </summary>
        /// <param name="pPC2Range">Taille de la dimenssion PC2</param>
        /// <param name="pPC3Range">Taille de la dimenssion PC3</param>
        /// <returns></returns>
        public int RowNumber(int PC2Range, int pPC3Range)
        {
            int rowNo = (PC1PointNo * PC2Range * pPC3Range) + (PC2PointNo * pPC3Range) + PC3PointNo;
            return rowNo;
        }
        #endregion Methods
    }

    /// <summary>
    /// Courbe pondérée par une position
    /// </summary>
    internal sealed class NOMXCFMPositionCurve
    {
        #region Members
        /// <summary>
        /// Position
        /// </summary>
        private List<Pair<PosRiskMarginKey, RiskMarginPosition>> m_Positions;
        /// <summary>
        /// Quantité en position
        /// </summary>
        private decimal m_QtyPos;
        /// <summary>
        /// Side
        /// </summary>
        private string m_FixMLSide;
        /// <summary>
        /// Currency
        /// </summary>
        private string m_Currency;
        /// <summary>
        /// Margin Class
        /// </summary>
        private readonly string m_MarginClass;
        /// <summary>
        /// Curve Name
        /// </summary>
        private readonly string m_CurveName;
        /// <summary>
        /// Correlation Curve Name
        /// </summary>
        private string m_CorrelationCurveName;
        /// <summary>
        /// Paramètres de la courbe
        /// </summary>
        private RiskDataNOMXCFMYieldCurve m_CurveParameters;
        /// <summary>
        /// Points de la courbe
        /// </summary>
        private IEnumerable<NOMXCFMVolatilityScenario> m_Scenario;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Position
        /// </summary>
        public List<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get { return m_Positions; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        /// <summary>
        /// Margin Class
        /// </summary>
        public string MarginClass { get { return m_MarginClass; } }
        /// <summary>
        /// Curve Name
        /// </summary>
        public string CurveName { get { return m_CurveName; } }
        /// <summary>
        /// Correlation Curve Name
        /// </summary>
        public string CorrelationCurveName { get { return m_CorrelationCurveName; } }
        /// <summary>
        /// Scenarios values
        /// </summary>
        public IEnumerable<NOMXCFMVolatilityScenario> Scenario { get { return m_Scenario; } }
        /// <summary>
        /// PC1 Range
        /// </summary>
        public int PC1Range
        {
            get
            {
                return ((m_Scenario != default(IEnumerable<NOMXCFMVolatilityScenario>)) && (m_Scenario.Count() > 0)) ? m_Scenario.Max(s => s.PC1PointNo) + 1 : 0;
            }
        }
        /// <summary>
        /// PC2 Range
        /// </summary>
        public int PC2Range
        {
            get
            {
                return ((m_Scenario != default(IEnumerable<NOMXCFMVolatilityScenario>)) && (m_Scenario.Count() > 0)) ? m_Scenario.Max(s => s.PC2PointNo) + 1 : 0;
            }
        }
        /// <summary>
        /// PC3 Range
        /// </summary>
        public int PC3Range
        {
            get
            {
                return ((m_Scenario != default(IEnumerable<NOMXCFMVolatilityScenario>)) && (m_Scenario.Count() > 0)) ? m_Scenario.Max(s => s.PC3PointNo) + 1 : 0;
            }
        }
        /// <summary>
        /// Curve Parameters
        /// </summary>
        internal RiskDataNOMXCFMYieldCurve CurveParameters
        {
            get { return m_CurveParameters; }
            set
            {
                m_CurveParameters = value;
                if (m_CurveParameters != default(RiskDataNOMXCFMYieldCurve))
                {
                    m_CorrelationCurveName = m_CurveParameters.CorrelationCurveId;
                    m_Currency = m_CurveParameters.Currency;
                }
            }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur (pour clonage)
        /// </summary>
        /// <param name="pSource">Element à cloner</param>
        private NOMXCFMPositionCurve(NOMXCFMPositionCurve pSource)
        {
            if (pSource != default(NOMXCFMPositionCurve))
            {
                m_Positions = pSource.m_Positions;
                m_QtyPos = pSource.m_QtyPos;
                m_FixMLSide = pSource.m_FixMLSide;
                m_Currency = pSource.m_Currency;
                m_MarginClass = pSource.MarginClass;
                m_CurveName = pSource.m_CurveName;
                m_CurveParameters = pSource.m_CurveParameters;
                if (pSource.Scenario != default(IEnumerable<NOMXCFMVolatilityScenario>))
                {
                    IEnumerable<NOMXCFMVolatilityScenario> scenario = pSource.Scenario;
                    m_Scenario = scenario.Select(s => new NOMXCFMVolatilityScenario(s.PC1PointNo, s.PC2PointNo, s.PC3PointNo, s.LowValue, s.MiddleValue, s.HighValue));
                }
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMarginCurve"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pPosition"></param>
        public NOMXCFMPositionCurve(RiskDataNOMXCFMMarginCurve pMarginCurve, string pCurrency, Pair<PosRiskMarginKey, RiskMarginPosition> pPosition)
        {
            if ((pMarginCurve != default) && (pPosition != default))
            {
                m_Positions = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>() { pPosition };
                m_QtyPos = pPosition.Second.Quantity;
                m_FixMLSide = pPosition.First.Side;
                m_Currency = pCurrency;
                m_MarginClass = pMarginCurve.MarginClass;
                m_CurveName = pMarginCurve.IdPrimaryYieldCurve;
                if ((pMarginCurve.Nodes != default(List<RiskDataNOMXCFMCurveNode>)) && (pMarginCurve.Nodes.Count > 0))
                {
                    if (m_FixMLSide == SideTools.RetBuyFIXmlSide())
                    {
                        m_Scenario = pMarginCurve.Nodes.Select(n => new NOMXCFMVolatilityScenario(n.PC1PointNo, n.PC2PointNo, n.PC3PointNo, n.LongLow * m_QtyPos, n.LongMiddle * m_QtyPos, n.LongHigh * m_QtyPos));
                    }
                    else
                    {
                        m_Scenario = pMarginCurve.Nodes.Select(n => new NOMXCFMVolatilityScenario(n.PC1PointNo, n.PC2PointNo, n.PC3PointNo, n.ShortLow * m_QtyPos, n.ShortMiddle * m_QtyPos, n.ShortHigh * m_QtyPos));
                    }
                }
            }
        }

        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMarginClass"></param>
        /// <param name="pCurveName"></param>
        /// <param name="pScenario"></param>
        /// <param name="pCurrency"></param>
        /// <param name="pPositions"></param>
        public NOMXCFMPositionCurve(string pMarginClass, string pCurveName, IEnumerable<NOMXCFMVolatilityScenario> pScenario, string pCurrency, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            m_QtyPos = 0;
            m_FixMLSide = string.Empty;
            m_Currency = pCurrency;
            m_MarginClass = pMarginClass;
            m_CurveName = pCurveName;
            m_Scenario = pScenario;
            if (pPositions != default)
            {
                m_Positions = pPositions.ToList();
            }
            else
            {
                m_Positions = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>();
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajouter les valeurs des scenarios de deux positions utilisant la même courbe
        /// </summary>
        /// <param name="pPosCurve"></param>
        /// <returns></returns>
        public NOMXCFMPositionCurve Add(NOMXCFMPositionCurve pPosCurve)
        {
            NOMXCFMPositionCurve result = Clone();
            result.m_QtyPos = 0;
            result.m_FixMLSide = string.Empty;
            //
            if (result.m_Positions == default(List<Pair<PosRiskMarginKey, RiskMarginPosition>>))
            {
                result.m_Positions = new List<Pair<PosRiskMarginKey,RiskMarginPosition>>();
            }
            if (pPosCurve.m_Positions != default(List<Pair<PosRiskMarginKey, RiskMarginPosition>>))
            {
                result.m_Positions.AddRange(pPosCurve.m_Positions);
            }
            //
            if ((pPosCurve != default(NOMXCFMPositionCurve))
                && (pPosCurve.MarginClass == MarginClass)
                && (pPosCurve.CurveName == CurveName)
                && (pPosCurve.Scenario.Count() == Scenario.Count()))
            {
                result.m_Scenario =
                    from scenario in result.Scenario
                    join addScenario in pPosCurve.Scenario on new { scenario.PC1PointNo, scenario.PC2PointNo, scenario.PC3PointNo } equals new { addScenario.PC1PointNo, addScenario.PC2PointNo, addScenario.PC3PointNo }
                    select new NOMXCFMVolatilityScenario(scenario.PC1PointNo, scenario.PC2PointNo, scenario.PC3PointNo,
                        scenario.LowValue + addScenario.LowValue,
                        scenario.MiddleValue + addScenario.MiddleValue,
                        scenario.HighValue + addScenario.HighValue);
            }
            return result;
        }

        /// <summary>
        /// Cloner
        /// </summary>
        /// <returns></returns>
        public NOMXCFMPositionCurve Clone()
        {
            NOMXCFMPositionCurve clone = new NOMXCFMPositionCurve(this)
            {
                m_Positions = new List<Pair<PosRiskMarginKey, RiskMarginPosition>>(this.m_Positions)
            };
            return clone;
        }

        /// <summary>
        /// Min value from all scenario
        /// </summary>
        /// <returns></returns>
        public decimal MinValue()
        {
            return Scenario.Min(s => s.MinValue());
        }

        /// <summary>
        /// Retourne le numéro d'enregistrement d'un scenario
        /// </summary>
        /// <param name="pScenario"></param>
        /// <returns></returns>
        public int ScenarioRowNumber(NOMXCFMVolatilityScenario pScenario)
        {
            int rowNo = -1;
            if (pScenario != default(NOMXCFMVolatilityScenario))
            {
                rowNo = pScenario.RowNumber(PC2Range, PC3Range);
            }
            return rowNo;
        }
        #endregion Methods
    }

    /// <summary>
    /// Montant issu d'un courbe
    /// </summary>
    internal sealed class NOMXCFMAmountCurve
    {
        #region Members
        /// <summary>
        /// Margin Curve
        /// </summary>
        private readonly NOMXCFMPositionCurve m_MarginCurve;
        /// <summary>
        /// Margin Amount
        /// </summary>
        private readonly decimal m_MarginAmount;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Margin Curve
        /// </summary>
        public NOMXCFMPositionCurve MarginCurve { get { return m_MarginCurve; } }
        /// <summary>
        /// Margin Amount
        /// </summary>
        public decimal MarginAmount { get { return m_MarginAmount; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pSource"></param>
        public NOMXCFMAmountCurve(NOMXCFMPositionCurve pCurve, decimal pAmount)
        {
            if (pCurve != default(NOMXCFMPositionCurve))
            {
                m_MarginCurve = pCurve;
            }
            m_MarginAmount = pAmount;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Obtenir le Margin Amount dans un objet Money
        /// </summary>
        /// <returns></returns>
        public Money GetMoneyMarginAmount()
        {
            Money amount = default;
            if (m_MarginCurve != default(NOMXCFMPositionCurve))
            {
                // Prendre la valeur absolue des montants négatifs
                decimal finalAmount = m_MarginAmount > 0 ? 0 : System.Math.Abs(m_MarginAmount);
                //
                amount = new Money(finalAmount, m_MarginCurve.Currency);
            }
            return amount;
        }
        #endregion Methods
    }

    /// <summary>
    /// Gestion des Curve Correlation Cube
    /// </summary>
    internal sealed class NOMXCFMPositionCorrelationCurve
    {
        #region Members
        /// <summary>
        /// Correlation Cube
        /// </summary>
        private readonly RiskDataNOMXCFMCurveCorrelationCube m_CorrelationCube;
        /// <summary>
        /// Ensemble des Position Curve Source
        /// </summary>
        private readonly IEnumerable<NOMXCFMPositionCurve> m_SourcePositionCurve;
        /// <summary>
        /// Position Curve issu du calcul de la correlation
        /// </summary>
        private NOMXCFMPositionCurve m_ResultPositionCurve;
        /// <summary>
        /// Ensemble des Childs Position Curve
        /// </summary>
        private readonly IEnumerable<NOMXCFMPositionCorrelationCurve> m_ChildsPositionCurve;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Correlation Cube
        /// </summary>
        public RiskDataNOMXCFMCurveCorrelationCube CorrelationCube { get { return m_CorrelationCube; } }
        /// <summary>
        /// Ensemble des Position Curve Source
        /// </summary>
        public IEnumerable<NOMXCFMPositionCurve> SourcePositionCurve { get { return m_SourcePositionCurve; } }
        /// <summary>
        /// Position Curve issu du calcul de la correlation
        /// </summary>
        public NOMXCFMPositionCurve ResultPositionCurve { get { return m_ResultPositionCurve; } }
        /// <summary>
        /// Ensemble des Childs Position Curve
        /// </summary>
        public IEnumerable<NOMXCFMPositionCorrelationCurve> ChildsPositionCurve { get { return m_ChildsPositionCurve; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pPositionCurve"></param>
        public NOMXCFMPositionCorrelationCurve(RiskDataNOMXCFMCurveCorrelationCube pCorrelationCube, IEnumerable<NOMXCFMPositionCurve> pSourcePositionCurve, IEnumerable<NOMXCFMPositionCorrelationCurve> pChildsPositionCurve)
        {
            m_CorrelationCube = pCorrelationCube;
            m_SourcePositionCurve = pSourcePositionCurve;
            m_ChildsPositionCurve = pChildsPositionCurve;
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pChildsPositionCurve"></param>
        public NOMXCFMPositionCorrelationCurve(IEnumerable<NOMXCFMPositionCorrelationCurve> pChildsPositionCurve)
        {
            m_ChildsPositionCurve = pChildsPositionCurve;
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Calcul d'une nouvelle courbe en appliquant la correlation entre plusieurs courbes
        /// </summary>
        /// <returns></returns>
        public NOMXCFMPositionCurve ProceedCorrelation()
        {
            m_ResultPositionCurve = default;
            if ((m_CorrelationCube != default)
                && (m_SourcePositionCurve != default)
                && (m_SourcePositionCurve.Count() >= 0))
            {
                List<NOMXCFMPositionCurve> neighboursCurve = new List<NOMXCFMPositionCurve>();
                foreach (NOMXCFMPositionCurve curve in m_SourcePositionCurve)
                {
                    // Construction de la curve de voisinage
                    NOMXCFMPositionCurve neighbour = NeighboursCurve(m_CorrelationCube.CurveId, curve, m_CorrelationCube.OverlapPC1, m_CorrelationCube.OverlapPC2, m_CorrelationCube.OverlapPC3);
                    neighboursCurve.Add(neighbour);
                }
                // Ajouter les valeurs minimum de chaque correlation curve
                m_ResultPositionCurve = neighboursCurve.Aggregate((a, b) => a.Add(b));
            }
            return m_ResultPositionCurve;
        }

        /// <summary>
        /// Construction d'une nouvelle courbe en appliquant une fenêtre sur chaque dimension de la courbe source
        /// </summary>
        /// <param name="pCurveName">Nom de la nouvelle courbe</param>
        /// <param name="pCurve">Courbe d'origine</param>
        /// <param name="pWindowPC1">Fenêtre sur PC1</param>
        /// <param name="pWindowPC2">Fenêtre sur PC2</param>
        /// <param name="pWindowPC3">Fenêtre sur PC3</param>
        /// <returns></returns>
        private static NOMXCFMPositionCurve NeighboursCurve(string pCurveName, NOMXCFMPositionCurve pCurve, int pWindowPC1, int pWindowPC2, int pWindowPC3)
        {
            NOMXCFMPositionCurve neighboursCurve = default;
            if (pCurve != default(NOMXCFMPositionCurve))
            {
                List<NOMXCFMVolatilityScenario> newScenario = new List<NOMXCFMVolatilityScenario>();
                foreach (NOMXCFMVolatilityScenario value in pCurve.Scenario)
                {
                    int decalPC1 = (pWindowPC1 - 1) / 2;
                    int decalPC2 = (pWindowPC2 - 1) / 2;
                    int decalPC3 = (pWindowPC3 - 1) / 2;
                    //
                    // Recherche des points voisins
                    IEnumerable<NOMXCFMVolatilityScenario> neighbours =
                        from scenario in pCurve.Scenario
                        where (scenario.PC1PointNo >= (value.PC1PointNo - decalPC1)) && (scenario.PC1PointNo <= (value.PC1PointNo + decalPC1))
                        && (scenario.PC2PointNo >= (value.PC2PointNo - decalPC2)) && (scenario.PC2PointNo <= (value.PC2PointNo + decalPC2))
                        && (scenario.PC3PointNo >= (value.PC3PointNo - decalPC3)) && (scenario.PC3PointNo <= (value.PC3PointNo + decalPC3))
                        select scenario;
                    //
                    // Calcul des nouvelles valeurs du scénario
                    NOMXCFMVolatilityScenario neighboursScenario = MinNeighbours(value, neighbours);
                    newScenario.Add(neighboursScenario);
                }
                neighboursCurve = new NOMXCFMPositionCurve(pCurve.MarginClass, pCurveName, newScenario, pCurve.Currency, pCurve.Positions);
            }
            return neighboursCurve;
        }

        /// <summary>
        /// Création d'un nouveau scénario pour le point du scénario <see cref="pSource"/> en prenant le minimum des scénarios <see cref="pNeighbours"/>
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pNeighbours"></param>
        /// <returns></returns>
        private static NOMXCFMVolatilityScenario MinNeighbours(NOMXCFMVolatilityScenario pSource, IEnumerable<NOMXCFMVolatilityScenario> pNeighbours)
        {
            NOMXCFMVolatilityScenario newScenario = default;
            if (pSource != default(NOMXCFMVolatilityScenario))
            {
                if (pNeighbours != default(IEnumerable<NOMXCFMVolatilityScenario>))
                {
                    decimal low = pNeighbours.Min(n => n.LowValue);
                    decimal middle = pNeighbours.Min(n => n.MiddleValue);
                    decimal high = pNeighbours.Min(n => n.HighValue);
                    newScenario = new NOMXCFMVolatilityScenario(pSource.PC1PointNo, pSource.PC2PointNo, pSource.PC3PointNo, low, middle, high);
                }
                else
                {
                    newScenario = pSource;
                }
            }
            return newScenario;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe contenant le résultat du calcul et des étapes intermediaires
    /// </summary>
    internal sealed class NOMXCFMEvaluationData
    {
        #region Members
        /// <summary>
        /// Ensemble des courbes de risque avec montants finaux 
        /// </summary>
        private readonly List<NOMXCFMAmountCurve> m_MarginCurveAmount;
        /// <summary>
        /// Ensemble des montants de risque finaux calculés
        /// </summary>
        private List<Money> m_MarginAmountList;
        /// <summary>
        /// Position détaillée avec courbe
        /// </summary>
        public IEnumerable<NOMXCFMPositionCurve> DetailedPositionCurve;
        /// <summary>
        /// Position cumulée par courbe
        /// </summary>
        public List<NOMXCFMPositionCurve> CumulatedPositionCurve;
        /// <summary>
        /// Position sans paramètres de calcul : sans courbe
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> PositionWithoutCurve;
        /// <summary>
        /// Poisition avec courbe mais sans paramètres complémentaire pour la courbe
        /// </summary>
        public IEnumerable<NOMXCFMPositionCurve> CurveWithoutParameters;
        /// <summary>
        /// Courbes des positions calculées avec l'arborescence de correlation
        /// </summary>
        public IEnumerable<NOMXCFMPositionCorrelationCurve> PositionCorrelationCurve;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des courbes de risque avec montants finaux 
        /// </summary>
        public List<NOMXCFMAmountCurve> MarginCurveAmount
        {
            get { return m_MarginCurveAmount; }
        }
        /// <summary>
        /// Ensemble des montants de risque finaux calculés
        /// </summary>
        public List<Money> MarginAmountList
        {
            get { return m_MarginAmountList; }
        }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        public NOMXCFMEvaluationData()
        {
            m_MarginCurveAmount = new List<NOMXCFMAmountCurve>();
            m_MarginAmountList = new List<Money>();
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Calcul du montant associé à une courbe
        /// </summary>
        /// <param name="pPositionCurve"></param>
        public void CalcAmount(IEnumerable<NOMXCFMPositionCurve> pPositionCurve)
        {
            if (pPositionCurve != default(IEnumerable<NOMXCFMPositionCurve>))
            {
                foreach (NOMXCFMPositionCurve curve in pPositionCurve)
                {
                    // Calcul du montant pour chaque courbe
                    decimal value = curve.MinValue();
                    NOMXCFMAmountCurve amount = new NOMXCFMAmountCurve(curve, value);
                    m_MarginCurveAmount.Add(amount);
                }
            }
        }

        /// <summary>
        /// Construire les montants de déposit sous la forme d'une liste de Money par devise
        /// </summary>
        /// <returns></returns>
        public List<Money> GenerateMoneyAmount()
        {
            if (m_MarginCurveAmount != default(List<NOMXCFMAmountCurve>))
            {
                IEnumerable<Money> allMoneyAmount =
                    from amount in m_MarginCurveAmount
                    select amount.GetMoneyMarginAmount();
                //
                // Faire la somme des montants par devise
                m_MarginAmountList = (
                    from amount in allMoneyAmount
                    group amount by amount.Currency into amountByCur
                    select new Money(amountByCur.Select(m => m.Amount.DecValue).Sum(), amountByCur.Key)
                    ).ToList();
            }
            return m_MarginAmountList;
        }
        #endregion Methods
    }

    /// <summary>
    /// Classe de calcul du déposit par la méthode NASDAQ-OMX CFM
    /// </summary>
    /// PM 20190222 [24326] Nouvelle méthode NOMX_CFM
    public sealed class NOMXCFMMethod : BaseMethod
    {
        #region Members
        #region Referentiel Parameters
        /// <summary>
        /// Donnée référentiel Asset
        /// </summary>
        internal IEnumerable<AssetExpandedParameter> m_AssetExpandedParameters = default;
        #endregion Referentiel Parameters
        #region NOMX CFM Parameters
        /// <summary>
        /// 
        /// </summary>
        internal DateTime m_DtBusiness = default;
        /// <summary>
        /// Ensemble des Margin Curves
        /// </summary>
        internal IEnumerable<RiskDataNOMXCFMMarginCurve> m_MarginCurvesParameters = default;
        /// <summary>
        /// Hierarchie des Correlation Curves
        /// </summary>
        internal RiskDataNOMXCFMCurveHierarchie m_CurveCorrelationHierarchieParameters = default;
        /// <summary>
        /// Ensemble des Yield Curves
        /// </summary>
        internal IEnumerable<RiskDataNOMXCFMYieldCurve> m_YieldCurvesParameters = default;
        /// <summary>
        /// Price de chaque Asset
        /// </summary>
        internal Dictionary<int, decimal> m_AssetPrices = default;
        #endregion NOMX CFM Parameters
        #endregion Members

        #region Override base accessors
        /// <summary>
        /// Type de la Methode
        /// </summary>
        public override InitialMarginMethodEnum Type
        {
            get { return InitialMarginMethodEnum.NOMX_CFM; }
        }

        /// <summary>
        /// Requête utilisée pour connaître l'existance de paramètres de risque pour une date donnée
        /// <remarks>Utilise le paramètres DTBUSINESS</remarks>
        /// </summary>
        //protected override string QueryExistRiskParameter
        //{
        //    get { return string.Empty; }
        //}
        #endregion Override base accessors

        #region Override base methods
        /// <summary>
        /// Arrondi un montant en utilisant la règle par défaut et la précision donnée.
        /// </summary>
        /// <param name="pAmount">Montant à arrondir</param>
        /// <param name="pPrecision">Precision d'arrondi</param>
        /// <returns>Le montant arrondi, lorsque le chiffre des décimales à arrondir vaut 5, l'arrondie est réalisé en prenant la valeur la plus éloignée de zéro</returns>
        protected override decimal RoundAmount(decimal pAmount, int pPrecision)
        {
            return System.Math.Round(pAmount, pPrecision, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Charge les paramètres spécifiques à la méthode.
        /// </summary>
        /// <param name="pCS">connection string</param>
        /// <param name="pAssetETDCache">Collection d'assets contenant tous les assets en position</param>
        protected override void LoadSpecificParameters(string pCS, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            bool isLoadParametersFromfile = (m_IdIOTaskRiskData != 0);
            m_DtBusiness = GetRiskParametersDate(pCS);
            //
            using (IDbConnection connection = DataHelper.OpenConnection(pCS))
            {
                // ASSETEXPANDED_ALLMETHOD
                m_AssetExpandedParameters = LoadParametersAssetExpanded(connection);
            }
            //
            if (isLoadParametersFromfile)
            {
                LoadSpecificParametersFromFile(m_DtBusiness, pAssetETDCache);
            }
        }

        /// <summary>
        /// Libère les paramètres spécifiques à la méthode.
        /// </summary>
        protected override void ResetSpecificParameters()
        {
            m_AssetExpandedParameters = default;
            m_MarginCurvesParameters = default;
            m_CurveCorrelationHierarchieParameters = default;
            m_YieldCurvesParameters = default;
            m_AssetPrices = default;
        }

        /// <summary>
        /// Calcul du montant de déposit pour la position d'un book d'un acteur
        /// </summary>
        /// <param name="pActorId">L'acteur de la position à évaluer</param>
        /// <param name="pBookId">Le book de la position à évaluer</param>
        /// <param name="pDepositHierarchyClass">type de hierarchie pour le couple Actor/Book</param>
        /// <param name="pRiskDataToEvaluate">La position pour laquelle calculer le déposit</param>
        /// <param name="opMethodComObj">Valeur de retour contenant toutes les données à passer à la feuille de calcul
        /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
        /// de la méthode de calcul (<see cref="EfsML.v30.MarginRequirement.MarginCalculationMethod"/> 
        /// et <see cref="EfsML.Interface.IMarginCalculationMethod"/>)</param>
        /// <returns>Le montant de déposit correspondant à la position</returns>
        public override List<Money> EvaluateRiskElementSpecific(
            int pActorId, int pBookId, DepositHierarchyClass pDepositHierarchyClass,
            RiskData pRiskDataToEvaluate,
            out IMarginCalculationMethodCommunicationObject opMethodComObj)
        {
            List<Money> riskAmounts = null;
            //
            // Creation de l'objet de communication du détail du calcul
            NOMXCFMCalcMethCom methodComObj = new NOMXCFMCalcMethCom();
            opMethodComObj = methodComObj;                          // Affectation de l'objet de communication du détail du calcul en sortie
            methodComObj.MarginMethodType = this.Type;              // Affectation du type de méthode de calcul
            methodComObj.CssCurrency = m_CssCurrency;               // Affectation de la devise de calcul
            methodComObj.IdA = pActorId;                            // Affectation de l'id de l'acteur
            methodComObj.IdB = pBookId;                             // Affectation de l'id du book
            methodComObj.DtParameters = m_DtBusiness;               // Date Business utilisée par la lecture des paramètres de calcul
            //
            IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positionsToEvaluate = default;
            if (pRiskDataToEvaluate != default(RiskData))
            {
                positionsToEvaluate = pRiskDataToEvaluate.GetPositionAsEnumerablePair();

                if ((positionsToEvaluate != null) && (positionsToEvaluate.Count() > 0))
                {
                    IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> positions;

                    // Grouper les positions par asset (the side of the new merged assets will be set with regards to the long and short quantities)
                    positions = PositionsGrouping.GroupPositionsByAsset(positionsToEvaluate);
                    // Ne garder que les positions dont la quantité est différente de 0
                    positions = from pos in positions
                                where (pos.Second.Quantity != 0)
                                select pos;

                    // Coverage short call and short futures (this one modify the position quantity)
                    IEnumerable<CoverageSortParameters> inputCoverage = GetSortParametersForCoverage(positions);
                    // Reduction de la position couverte
                    Pair<IEnumerable<StockCoverageCommunicationObject>, IEnumerable<StockCoverageDetailCommunicationObject>> coveredQuantities =
                        ReducePosition(pActorId, pBookId, pDepositHierarchyClass, inputCoverage, ref positions);
                    methodComObj.UnderlyingStock = coveredQuantities.Second;

                    // Calculer les montants de risque
                    NOMXCFMEvaluationData result = EvaluateRisk(methodComObj, positions);
                    riskAmounts = result.MarginAmountList;
                }
            }
            if (riskAmounts == null)
            {
                riskAmounts = new List<Money>();
            }

            if (riskAmounts.Count == 0)
            {
                // Si aucun montant, créer un montant à zéro
                if (StrFunc.IsEmpty(this.m_CssCurrency))
                {
                    // Si aucune devise de renseignée, utiliser l'euro
                    riskAmounts.Add(new Money(0, "EUR"));
                }
                else
                {
                    riskAmounts.Add(new Money(0, this.m_CssCurrency));
                }
            }
            //
            methodComObj.MarginAmounts = riskAmounts.ToArray();
            //
            return riskAmounts;
        }

        /// <summary>
        /// Get a collection of sorting parameter needed by coverage strategies
        /// </summary>
        /// <param name="pGroupedPositionsByIdAsset">Positions of the current risk element</param>
        /// <returns>A collection of sorting parameters in order to be used inside of the ReducePosition method</returns>
        protected override IEnumerable<CoverageSortParameters> GetSortParametersForCoverage(IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pGroupedPositionsByIdAsset)
        {
            IEnumerable<CoverageSortParameters> retPos =
                from position in pGroupedPositionsByIdAsset
                join assetRef in m_AssetExpandedParameters on position.First.idAsset equals assetRef.AssetId
                where (false == assetRef.PutOrCall.HasValue) || (assetRef.PutOrCall == PutOrCallEnum.Call)
                join price in m_AssetPrices on assetRef.AssetId equals price.Key
                select new CoverageSortParameters
                {
                    AssetId = position.First.idAsset,
                    ContractId = assetRef.ContractId,
                    MaturityYearMonth = decimal.Parse(assetRef.MaturityDate.Year.ToString() + assetRef.MaturityDate.Month.ToString() + assetRef.MaturityDate.Day.ToString()),
                    Multiplier = assetRef.ContractMultiplier,
                    Quote = price.Value,
                    StrikePrice = assetRef.StrikePrice,
                    Type = assetRef.PutOrCall.HasValue ? RiskMethodQtyType.Call : RiskMethodQtyType.Future,
                };
            return retPos;
        }

        /// <summary>
        /// Lecture d'informations complémentaire pour les Marchés/Chambre de compensation utilisant la méthode courante 
        /// </summary>
        /// <param name="pEntityMarkets">La collection de entity/market attaché à la chambre de compensation courante</param>
        public override void BuildMarketParameters(IEnumerable<EntityMarketWithCSS> pEntityMarkets)
        {
            base.BuildMarketParameters(pEntityMarkets);
        }
        #endregion Override base methods

        #region Methods
        /// <summary>
        /// Chargement des paramètres de calcul à partir des fichiers
        /// </summary>
        /// <param name="pDtBusiness"></param>
        /// <param name="pAssetETDCache"></param>
        private void LoadSpecificParametersFromFile(DateTime pDtBusiness, Dictionary<int, SQL_AssetETD> pAssetETDCache)
        {
            Logger.Write();

            if (pAssetETDCache != default(Dictionary<int, SQL_AssetETD>))
            {

                // Objet qui contiendra les paramètres de calcul lus lors de l'import
                RiskDataLoadNOMX fileCFMData = new RiskDataLoadNOMX(this.Type, pAssetETDCache.Values);

                // Lancement de l'import
                RiskDataImportTask import = new RiskDataImportTask(ProcessInfo.Process, IdIOTaskRiskData);
                import.ProcessTask(pDtBusiness, fileCFMData);

                Logger.Log(new LoggerData(LogLevelEnum.None, new SysMsgCode(SysCodeEnum.LOG, 1078), 1));
                
                // Déverser les données dans les classes de calcul
                DumpFileParametersToData(fileCFMData);
            }
        }

        /// <summary>
        /// Déverse les données provenant des fichiers de paramètres dans les classes de paramètres utilisées par le calcul du déposit
        /// </summary>
        /// <param name="pFileData"></param>
        private void DumpFileParametersToData(RiskDataLoadNOMX pFileData)
        {
            if (pFileData != default(RiskDataLoadNOMX))
            {
                m_MarginCurvesParameters = pFileData.MarginCurves;
                m_CurveCorrelationHierarchieParameters = pFileData.CurveCorrelationHierarchie;
                m_YieldCurvesParameters = pFileData.YieldCurves;
                //
                // Application du PriceDecimalLocator sur les prix lus
                m_AssetPrices = (
                    from assetRef in m_AssetExpandedParameters
                    join priceRead in pFileData.Prices on assetRef.AssetId equals priceRead.IdAsset
                    into assetPrice
                    from price in assetPrice.DefaultIfEmpty(new RiskDataNOMXPrice(assetRef.AssetId))
                    select new { price.IdAsset, Price = (decimal)(price.MarginPrice / System.Math.Pow(10, assetRef.PriceDecimalLocator)) }
                    ).ToDictionary(p => p.IdAsset, p => p.Price);
            }
        }

        /// <summary>
        ///  Evalue les montants de dépot de garantie
        /// </summary>
        /// <param name="pMethodComObj">Pour la construction du log</param>
        /// <param name="pPositions">Position</param>
        /// <returns>Données calculés</returns>
        private NOMXCFMEvaluationData EvaluateRisk(NOMXCFMCalcMethCom pMethodComObj, IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> pPositions)
        {
            // Données résulants du calcul
            NOMXCFMEvaluationData result = new NOMXCFMEvaluationData();
            //
            // Prendre les Margin Parameters pour le calcul CFM
            IEnumerable<RiskDataNOMXCFMMarginCurve> marginCurvesParameters = m_MarginCurvesParameters.Where(c => c.MarginClass == "CFM");
            //
            // Associer chaque position avec ces Margin Parameters
            var positionCurve =
                from pos in pPositions
                join curveParam in marginCurvesParameters on pos.First.idAsset equals curveParam.IdAsset
                into curve
                from posCurve in curve.DefaultIfEmpty()
                select new { Position = pos, Curve = posCurve };
            //
            // Position n'ayant pas de Margin Parameters
            result.PositionWithoutCurve =
                from posCurve in positionCurve
                where (posCurve.Curve == default)
                select posCurve.Position;
            //
            // Appliquer la quantité en position aux Margin Parameters
            result.DetailedPositionCurve =
                from posCurve in positionCurve
                join asset in m_AssetExpandedParameters on posCurve.Position.First.idAsset equals asset.AssetId
                where (posCurve.Curve != default)
                select new NOMXCFMPositionCurve(posCurve.Curve, asset.Currency, posCurve.Position);
            //
            // Regrouper par courbe
            var posScenarioGroup =
                from posCurve in result.DetailedPositionCurve
                group posCurve by new { posCurve.MarginClass, posCurve.CurveName } into posCurveGroup
                select posCurveGroup;
            //
            // Cumul des valeurs par courbe
            result.CumulatedPositionCurve = new List<NOMXCFMPositionCurve>();
            foreach (var posCurve in posScenarioGroup)
            {
                NOMXCFMPositionCurve cumulCurve = posCurve.Aggregate((a, b) => a.Add(b));
                result.CumulatedPositionCurve.Add(cumulCurve);
            }
            //
            // Recherche des paramètres de chaque courbe
            var curveWithParam =
                from curve in result.CumulatedPositionCurve
                join curveParam in m_YieldCurvesParameters on curve.CurveName equals curveParam.YieldCurveId
                into correCurve
                from curveCorrelation in correCurve.DefaultIfEmpty()
                select new { PositionCurve = curve, CurveParam = curveCorrelation };
            //
            // Courbes n'ayant pas de Curve parameters
            result.CurveWithoutParameters =
                from curve in curveWithParam
                where curve.CurveParam == default(RiskDataNOMXCFMYieldCurve)
                select curve.PositionCurve;
            //
            // Calcul des montants pour les courbes sans paramètres de correlation
            result.CalcAmount(result.CurveWithoutParameters);
            //
            // Affecter les paramétres de la courbe à chaque courbe de position
            curveWithParam = curveWithParam.Where(c => c.CurveParam != default(RiskDataNOMXCFMYieldCurve));
            foreach(var posCurve in curveWithParam)
            {
                posCurve.PositionCurve.CurveParameters = posCurve.CurveParam;
            }
            //
            result.PositionCorrelationCurve = ProceedCurveTree(result);
            //
            // Calcul du resultat sur les courbes racines
            if (result.PositionCorrelationCurve != default(IEnumerable<NOMXCFMPositionCorrelationCurve>))
            {
                IEnumerable<NOMXCFMPositionCurve> positionCurveRoots = result.PositionCorrelationCurve.Select(c => c.ResultPositionCurve);
                // Calcul des montants pour les courbes avec correlation
                result.CalcAmount(positionCurveRoots);
            }
            //
            // Calcul des montants globaux
            result.GenerateMoneyAmount();
            //
            // Construction du log
            SetCalculationLog(pMethodComObj, result);
            //
            return result;
        }

        /// <summary>
        /// Traitement de l'arborescence de calcul des Correlation Curve
        /// </summary>
        /// <param name="pResult"></param>
        /// <returns></returns>
        private IEnumerable<NOMXCFMPositionCorrelationCurve> ProceedCurveTree(NOMXCFMEvaluationData pResult)
        {
            IEnumerable<NOMXCFMPositionCorrelationCurve> positionCorrelationCurveTree = default;
            if (pResult != default(NOMXCFMEvaluationData))
            {
                // Ensemble des position curve sans correlation parent
                List<NOMXCFMPositionCurve> curveWithoutParentCorrelation = new List<NOMXCFMPositionCurve>();
                //
                // Recherche des racines des courbes dans la hiérachie des curveCorrelation
                List<RiskDataNOMXCFMCurveCorrelationCube> roots = new List<RiskDataNOMXCFMCurveCorrelationCube>();
                foreach (NOMXCFMPositionCurve curve in pResult.CumulatedPositionCurve)
                {
                    RiskDataNOMXCFMCurveCorrelationCube correlationRoot = m_CurveCorrelationHierarchieParameters.GetRoot(curve.CurveParameters, curve.MarginClass);
                    if (correlationRoot == default)
                    {
                        curveWithoutParentCorrelation.Add(curve);
                    }
                    else
                    {
                        roots.Add(correlationRoot);
                    }
                }
                // Calcul des montants pour les courbes sans correlation 
                pResult.CalcAmount(curveWithoutParentCorrelation);
                //
                // Traiter l'ensemble de la hierarchie des Correlation Curve à partir des racines
                positionCorrelationCurveTree = ProceedChilds(pResult, roots);
            }
            return positionCorrelationCurveTree;
        }

        /// <summary>
        /// Calcul des Correlation Curve enfants de manière récursive
        /// </summary>
        /// <param name="pResult"></param>
        /// <param name="pCorrelationCube"></param>
        /// <returns></returns>
        private IEnumerable<NOMXCFMPositionCorrelationCurve> ProceedChilds(NOMXCFMEvaluationData pResult, IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> pCorrelationCube)
        {
            List<NOMXCFMPositionCorrelationCurve> correlationCurve = new List<NOMXCFMPositionCorrelationCurve>();
            if ((pResult != default(NOMXCFMEvaluationData)) && (pCorrelationCube != default))
            {
                // Parcours des Correlation Curve
                foreach (RiskDataNOMXCFMCurveCorrelationCube correlation in pCorrelationCube)
                {
                    // Selectionner les PositionCurve dont c'est la Correlation Curve
                    IEnumerable<NOMXCFMPositionCurve> curvePos = pResult.CumulatedPositionCurve.Where(c => (c.CorrelationCurveName == correlation.CurveId) && (c.MarginClass == correlation.MarginClass));
                    //
                    // Selectionner les Correlation Curve enfants
                    IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> childs = m_CurveCorrelationHierarchieParameters.GetChilds(correlation);
                    IEnumerable<NOMXCFMPositionCorrelationCurve> correlationCurveChilds = default;
                    if (childs.Count() > 0)
                    {
                        // S'il y a des Correlation Curve enfants, commencer par les traiter
                        correlationCurveChilds = ProceedChilds(pResult, childs);
                        if ((correlationCurveChilds != default(IEnumerable<NOMXCFMPositionCorrelationCurve>)) && (correlationCurveChilds.Count() > 0))
                        {
                            // Des Correlation Curve enfants ont été traiter, lire et ajouter leur Position Curve
                            IEnumerable<NOMXCFMPositionCurve> subCurvePos = correlationCurveChilds.Select(c => c.ResultPositionCurve);
                            curvePos = curvePos.Concat(subCurvePos);
                        }
                    }
                    // Test s'il y a des Curve à traiter
                    if (curvePos.Count() > 0)
                    {
                        NOMXCFMPositionCorrelationCurve newCorrelationCurve = new NOMXCFMPositionCorrelationCurve(correlation, curvePos, correlationCurveChilds);
                        newCorrelationCurve.ProceedCorrelation();
                        //
                        correlationCurve.Add(newCorrelationCurve);
                    }
                }
            }
            return correlationCurve;
        }

        /// <summary>
        /// Construction du log du calcul
        /// </summary>
        /// <param name="pMethodComObj"></param>
        /// <param name="pResult"></param>
        private void SetCalculationLog(NOMXCFMCalcMethCom pMethodComObj, NOMXCFMEvaluationData pResult)
        {
            List<NOMXCFMResultCurveAmountCom> resultCurveAmountCom = new List<NOMXCFMResultCurveAmountCom>();
            NOMXCFMResultDetailCom calculationDetail = new NOMXCFMResultDetailCom();
            List<NOMXCFMResultCurveCom> resultSingleCurveCom = new List<NOMXCFMResultCurveCom>();
            List<NOMXCFMResultCurveCom> resultCorrelationCurveCom = new List<NOMXCFMResultCurveCom>();
            pMethodComObj.Missing = false;
            //
            // Log courbe avec montant final
            if ((pResult.MarginCurveAmount != default(List<NOMXCFMAmountCurve>))
                && (pResult.MarginCurveAmount.Count() > 0))
            {
                IEnumerable<NOMXCFMResultCurveAmountCom> posCurveAmount =
                    from posCurve in pResult.MarginCurveAmount
                    select new NOMXCFMResultCurveAmountCom
                    {
                        CurveName = posCurve.MarginCurve.CurveName,
                        MarginClass = posCurve.MarginCurve.MarginClass,
                        MarginAmount = posCurve.GetMoneyMarginAmount(),
                        Missing = false,
                        Positions = posCurve.MarginCurve.Positions,
                    };
                resultCurveAmountCom.AddRange(posCurveAmount);
            }
            //
            // Log position sans courbe
            if ((pResult.PositionWithoutCurve != default)
                && (pResult.PositionWithoutCurve.Count() > 0))
            {
                NOMXCFMResultCurveCom posMissingCurve = new NOMXCFMResultCurveCom
                {
                    CurveName = "Missing",
                    Positions = pResult.PositionWithoutCurve,
                    Missing = true
                };
                pMethodComObj.Missing = true;
                resultSingleCurveCom.Add(posMissingCurve);
            }
            //
            // Log position avec courbe sans parametres de correlation
            if ((pResult.CurveWithoutParameters != default(IEnumerable<NOMXCFMPositionCurve>))
                && (pResult.CurveWithoutParameters.Count() > 0))
            {
                IEnumerable<NOMXCFMResultCurveCom> posCurveNoparam = SetCurveCalculationLog(pResult.CurveWithoutParameters, true);
                //
                resultSingleCurveCom.AddRange(posCurveNoparam);
            }
            // Log position cumulée par courbe
            if ((pResult.CumulatedPositionCurve != default(IEnumerable<NOMXCFMPositionCurve>))
                && (pResult.CumulatedPositionCurve.Count() > 0))
            {
                IEnumerable<NOMXCFMResultCurveCom> posCumulCurve = SetCurveCalculationLog(pResult.CumulatedPositionCurve, false);
                //
                resultSingleCurveCom.AddRange(posCumulCurve);
            }
            // Log position avec courbe de correlation
            if ((pResult.PositionCorrelationCurve != default(IEnumerable<NOMXCFMPositionCorrelationCurve>))
                && (pResult.PositionCorrelationCurve.Count() > 0))
            {
                resultCorrelationCurveCom = SetChildCurveCalculationLog(pResult.PositionCorrelationCurve);
            }
            //
            pMethodComObj.Parameters = resultCurveAmountCom.ToArray();
            //
            calculationDetail.CorrelationCurveDetail = resultCorrelationCurveCom;
            calculationDetail.SingleCurveDetail = resultSingleCurveCom;
            //
            pMethodComObj.CalculationDetail = calculationDetail;
        }

        /// <summary>
        /// Construction du log de la hiérarchie des correlation curves
        /// </summary>
        /// <param name="pCorrelationCurves"></param>
        /// <returns></returns>
        private List<NOMXCFMResultCurveCom> SetChildCurveCalculationLog(IEnumerable<NOMXCFMPositionCorrelationCurve> pCorrelationCurves)
        {
            List<NOMXCFMResultCurveCom> childsCurveLog;
            if ((pCorrelationCurves != default(IEnumerable<NOMXCFMPositionCorrelationCurve>)) && (pCorrelationCurves.Count() > 0))
            {
                childsCurveLog = (
                    from pos in pCorrelationCurves
                    select new NOMXCFMResultCurveCom
                    {
                        CurveName = pos.ResultPositionCurve.CurveName,
                        MarginClass = pos.CorrelationCube.MarginClass,
                        Missing = false,
                        // Pas de log des positions
                        //Positions = pos.ResultPositionCurve.Positions,
                        ChildCurves = SetChildOrSourceCurveCalculationLog(pos),
                        IsChildCurve = true,
                        OverlapPC1 = pos.CorrelationCube.OverlapPC1,
                        OverlapPC2 = pos.CorrelationCube.OverlapPC2,
                        OverlapPC3 = pos.CorrelationCube.OverlapPC3,
                    }).ToList();
            }
            else
            {
                childsCurveLog = default;
            }
            return childsCurveLog;
        }

        /// <summary>
        /// Construction du log de la sous-hiérarchie des correlation curves
        /// </summary>
        /// <param name="pCorrelationCurve"></param>
        /// <returns></returns>
        private List<NOMXCFMResultCurveCom> SetChildOrSourceCurveCalculationLog(NOMXCFMPositionCorrelationCurve pCorrelationCurve)
        {
            List<NOMXCFMResultCurveCom> childsOrSourceCurveLog;
            if (pCorrelationCurve != default(NOMXCFMPositionCorrelationCurve))
            {
                // Child curve ou si pas de child alors curve source
                if ((pCorrelationCurve.ChildsPositionCurve != default(IEnumerable<NOMXCFMPositionCorrelationCurve>))
                    && (pCorrelationCurve.ChildsPositionCurve.Count() > 0))
                {
                    childsOrSourceCurveLog = SetChildCurveCalculationLog(pCorrelationCurve.ChildsPositionCurve);
                }
                else
                {
                    childsOrSourceCurveLog = (
                        from curve in pCorrelationCurve.SourcePositionCurve
                        select new NOMXCFMResultCurveCom
                        {
                            CurveName = curve.CurveName,
                            MarginClass = curve.MarginClass,
                            Missing = false,
                            IsChildCurve = false,
                        }).ToList();
                }
            }
            else
            {
                childsOrSourceCurveLog = default;
            } 
            return childsOrSourceCurveLog;
        }

        /// <summary>
        /// Construction du log d'un ensemble de curve
        /// </summary>
        /// <param name="pCurves"></param>
        /// <param name="pIsMissing"></param>
        /// <returns></returns>
        private List<NOMXCFMResultCurveCom> SetCurveCalculationLog(IEnumerable<NOMXCFMPositionCurve> pCurves, bool pIsMissing)
        {
            List<NOMXCFMResultCurveCom> curveLog;
            if ((pCurves != default(IEnumerable<NOMXCFMPositionCurve>)) && (pCurves.Count() > 0))
            {
                curveLog = (
                    from curve in pCurves
                    select new NOMXCFMResultCurveCom
                    {
                        CurveName = curve.CurveName,
                        MarginClass = curve.MarginClass,
                        Missing = pIsMissing,
                        Positions = curve.Positions,
                        Scenarios = SetScenarioLog(curve.Scenario),
                        IsChildCurve = false,
                    }).ToList();
            }
            else
            {
                curveLog = default;
            }
            return curveLog;
        }

        /// <summary>
        /// Construction du log des scénarios
        /// </summary>
        /// <param name="pScenario"></param>
        /// <returns></returns>
        private List<NOMXCFMScenarioCom> SetScenarioLog(IEnumerable<NOMXCFMVolatilityScenario> pScenario)
        {
            List<NOMXCFMScenarioCom> scenarioLog;
            if (pScenario != default(IEnumerable<NOMXCFMVolatilityScenario>))
            {
                scenarioLog = (
                    from sc in pScenario
                    select new NOMXCFMScenarioCom
                    {
                        PC1PointNo = sc.PC1PointNo,
                        PC2PointNo = sc.PC2PointNo,
                        PC3PointNo = sc.PC3PointNo,
                        LowValue = sc.LowValue,
                        MiddleValue = sc.MiddleValue,
                        HighValue = sc.HighValue,
                    }).ToList();
            }
            else
            {
                scenarioLog = default;
            }
            return scenarioLog;
        }

        #region TEST
        /// <summary>
        /// Non utilisé / Test uniquement
        /// </summary>
        private void TestNeighbours()
        {
            Dictionary<int, List<int>> scenarios = new Dictionary<int, List<int>>();
            for (int i = 1; i <= 125; i += 1 )
            {
                scenarios.Add(i, new List<int> { i });
            }
            Dictionary<int, List<int>> step1 = Neighbours(scenarios, 3, 1, 5);
            Dictionary<int, List<int>> step2 = Neighbours(step1, 3, 5, 5*5);
            Dictionary<int, List<int>> step3 = Neighbours(step2, 3, 5*5, 5*5*5);

            scenarios = new Dictionary<int, List<int>>();
            for (int i = 1; i <= 27; i += 1)
            {
                scenarios.Add(i, new List<int> { i });
            }
            step1 = Neighbours(scenarios, 5, 1, 3);
            step2 = Neighbours(step1, 5, 3, 3 * 3);
            step3 = Neighbours(step2, 5, 3 * 3, 3 * 3 * 3);
        }

        /// <summary>
        /// Non utilisé / Test uniquement
        /// </summary>
        /// <param name="pN"></param>
        /// <param name="pWSize"></param>
        /// <param name="pStep"></param>
        /// <param name="pMod"></param>
        /// <returns></returns>
        private Dictionary<int, List<int>> Neighbours(Dictionary<int, List<int>> pN, int pWSize, int pStep, int pMod)
        {

            int count = pN.Count();
            for (int i = 1; i <= count; i += 1)
            {
                int level = (int)System.Math.Floor((decimal)((i-1) / pMod));
                //
                int decal = (pWSize - 1) / 2;
                List<int> w = new List<int>();
                for (int j = -1 * decal; j <= decal; j += 1)
                {
                    int v = i + pStep * j;
                    if (((int)System.Math.Floor((decimal)((v -1) / pMod)) == level) && (v > 0) && (v <= count))
                    {
                        w.Add(v);
                    }
                }
                //
                if (pN.TryGetValue(i, out List<int> scenario))
                {
                    foreach (int j in w)
                    {
                        if (false == scenario.Exists(s => s == j))
                        {
                            scenario.Add(j);
                        }
                    }
                }
            }
            return pN;
        }
        #endregion TEST
        #endregion Methods
    }
}
