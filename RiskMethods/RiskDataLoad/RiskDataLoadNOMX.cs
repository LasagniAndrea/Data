using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.IO;
//
using EfsML.Enum;

namespace EFS.SpheresRiskPerformance
{
    /// <summary>
    /// Margin class curve
    /// </summary>
    public sealed class RiskDataNOMXCFMMarginCurve
    {
        #region Members
        /// <summary>
        /// Id Asset
        /// </summary>
        private readonly int m_IdAsset;
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        private readonly NOMXSeries m_Serie;
        /// <summary>
        /// Decimal In Value
        /// </summary>
        private readonly int m_DecimalInValue;
        /// <summary>
        /// Decimal In Discount Factor
        /// </summary>
        private readonly int m_DecimalInDiscountFactor;
        /// <summary>
        /// Primary Yield Curve Name
        /// </summary>
        private readonly string m_IdPrimaryYieldCurve;
        /// <summary>
        /// Primary Curve Correlation Cube Name
        /// </summary>
        private readonly string m_IdPrimaryCurveCorrelationCube;
        /// <summary>
        /// Secondary Yield Curve Name
        /// </summary>
        private readonly string m_IdSecondaryYieldCurve;
        /// <summary>
        /// Secondary Curve Correlation Cube Name
        /// </summary>
        private readonly string m_IdSecondaryCurveCorrelationCube;
        /// <summary>
        /// Closing Date
        /// </summary>
        private readonly DateTime m_ClosingDate;
        /// <summary>
        /// Margin Class
        /// </summary>
        private readonly string m_MarginClass;
        /// <summary>
        /// Nodes
        /// </summary>
        private readonly List<RiskDataNOMXCFMCurveNode> m_Nodes;
        #endregion Données lues à partir du fichier
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } }
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        public NOMXSeries Serie { get { return m_Serie; } }
        /// <summary>
        /// Decimal In Value
        /// </summary>
        public int DecimalInValue { get { return m_DecimalInValue; } }
        /// <summary>
        /// Decimal In Discount Factor
        /// </summary>
        public int DecimalInDiscountFactor { get { return m_DecimalInDiscountFactor; } }
        /// <summary>
        /// Primary Yield Curve Name
        /// </summary>
        public string IdPrimaryYieldCurve { get { return m_IdPrimaryYieldCurve; } }
        /// <summary>
        /// Primary Curve Correlation Cube Name
        /// </summary>
        public string IdPrimaryCurveCorrelationCube { get { return m_IdPrimaryCurveCorrelationCube; } }
        /// <summary>
        /// Secondary Yield Curve Name
        /// </summary>
        public string IdSecondaryYieldCurve { get { return m_IdSecondaryYieldCurve; } }
        /// <summary>
        /// Secondary Curve Correlation Cube Name
        /// </summary>
        public string IdSecondaryCurveCorrelationCube { get { return m_IdSecondaryCurveCorrelationCube; } }
        /// <summary>
        /// Closing Date
        /// </summary>
        public DateTime ClosingDate { get { return m_ClosingDate; } }
        /// <summary>
        /// Margin Class
        /// </summary>
        public string MarginClass { get { return m_MarginClass; } }
        /// <summary>
        /// Nodes
        /// </summary>
        public List<RiskDataNOMXCFMCurveNode> Nodes { get { return m_Nodes; } }
        #endregion Données lues à partir du fichier
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pSerie"></param>
        /// <param name="pValues"></param>
        public RiskDataNOMXCFMMarginCurve(int pIdAsset, NOMXSeries pSerie, string[] pValues)
        {
            m_IdAsset = pIdAsset;
            m_Serie = pSerie;
            m_Nodes = new List<RiskDataNOMXCFMCurveNode>();
            if ((pValues != default) && (pValues.Count() == 15))
            {
                m_DecimalInValue = IntFunc.IntValue(pValues[7]);
                m_IdPrimaryYieldCurve = pValues[8];
                m_IdPrimaryCurveCorrelationCube = pValues[9];
                m_IdSecondaryYieldCurve = pValues[10];
                m_IdSecondaryCurveCorrelationCube = pValues[11];
                m_DecimalInDiscountFactor = IntFunc.IntValue(pValues[12]);
                m_MarginClass = pValues[14];
                string closeDate = pValues[13].Trim();
                if (closeDate.Length == 8)
                {
                    m_ClosingDate = DtFunc.ParseDate(pValues[13], DtFunc.FmtDateyyyyMMdd, null);
                }
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Ajout des risk values d'un Curve Node 
        /// </summary>
        /// <param name="pNode"></param>
        public void AddNode(RiskDataNOMXCFMCurveNode pNode)
        {
            if (pNode != default(RiskDataNOMXCFMCurveNode))
            {
                Nodes.Add(pNode);
            }
        }
        #endregion Methods
    }

    /// <summary>
    /// Curve Node risk values
    /// </summary>
    public sealed class RiskDataNOMXCFMCurveNode
    {
        #region Members
        #region Données lues à partir du fichier
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
        /// Long Low
        /// </summary>
        private readonly decimal m_LongLow;
        /// <summary>
        /// Short Low
        /// </summary>
        private readonly decimal m_ShortLow;
        /// <summary>
        /// Long Middle
        /// </summary>
        private readonly decimal m_LongMiddle;
        /// <summary>
        /// Short Middle
        /// </summary>
        private readonly decimal m_ShortMiddle;
        /// <summary>
        /// Long High
        /// </summary>
        private readonly decimal m_LongHigh;
        /// <summary>
        /// Short High
        /// </summary>
        private readonly decimal m_ShortHigh;
        /// <summary>
        /// Long Discount
        /// </summary>
        private readonly decimal m_LongDiscount;
        /// <summary>
        /// Short Discount
        /// </summary>
        private readonly decimal m_ShortDiscount;
        #endregion Données lues à partir du fichier
        #endregion Members

        #region Accessors
        #region Données lues à partir du fichier
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
        /// Long Low
        /// </summary>
        public decimal LongLow { get { return m_LongLow; } }
        /// <summary>
        /// Short Low
        /// </summary>
        public decimal ShortLow { get { return m_ShortLow; } }
        /// <summary>
        /// Long Middle
        /// </summary>
        public decimal LongMiddle { get { return m_LongMiddle; } }
        /// <summary>
        /// Short Middle
        /// </summary>
        public decimal ShortMiddle { get { return m_ShortMiddle; } }
        /// <summary>
        /// Long High
        /// </summary>
        public decimal LongHigh { get { return m_LongHigh; } }
        /// <summary>
        /// Short High
        /// </summary>
        public decimal ShortHigh { get { return m_ShortHigh; } }
        /// <summary>
        /// Long Discount
        /// </summary>
        public decimal LongDiscount { get { return m_LongDiscount; } }
        /// <summary>
        /// Short Discount
        /// </summary>
        public decimal ShortDiscount { get { return m_ShortDiscount; } }
        #endregion Données lues à partir du fichier
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pValues"></param>
        public RiskDataNOMXCFMCurveNode(string[] pValues, int pNbDecimal, int pNbDecDiscountFactor)
        {
            if ((pValues != default) && (pValues.Count() == 11 ))
            {
                int diviseurValue = (int)System.Math.Pow(10, pNbDecimal);
                int diviseurDiscountFactor = (int)System.Math.Pow(10, pNbDecDiscountFactor);
                //
                m_PC1PointNo = IntFunc.IntValue(pValues[0]);
                m_PC2PointNo = IntFunc.IntValue(pValues[1]);
                m_PC3PointNo = IntFunc.IntValue(pValues[2]);
                m_LongLow = DecFunc.DecValue(pValues[3]) / diviseurValue;
                m_ShortLow = DecFunc.DecValue(pValues[4]) / diviseurValue;
                m_LongMiddle = DecFunc.DecValue(pValues[5]) / diviseurValue;
                m_ShortMiddle = DecFunc.DecValue(pValues[6]) / diviseurValue;
                m_LongHigh = DecFunc.DecValue(pValues[7]) / diviseurValue;
                m_ShortHigh = DecFunc.DecValue(pValues[8]) / diviseurValue;
                m_LongDiscount = DecFunc.DecValue(pValues[9]) / diviseurDiscountFactor;
                m_ShortDiscount = DecFunc.DecValue(pValues[10]) / diviseurDiscountFactor;
            }
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Curve Correlation Cube
    /// </summary>
    public sealed class RiskDataNOMXCFMCurveCorrelationCube
    {
        #region Members
        #region Données lues à partir du fichier
        /// <summary>
        /// Curve Name
        /// </summary>
        private readonly string m_CurveId;
        /// <summary>
        /// Parent Curve Name
        /// </summary>
        private readonly string m_ParentCurveId;
        /// <summary>
        /// Overlap PC1
        /// </summary>
        private readonly int m_OverlapPC1;
        /// <summary>
        /// Overlap PC2
        /// </summary>
        private readonly int m_OverlapPC2;
        /// <summary>
        /// Overlap PC3
        /// </summary>
        private readonly int m_OverlapPC3;
        /// <summary>
        /// Currency
        /// </summary>
        private readonly string m_Currency;
        /// <summary>
        /// Margin Classe
        /// </summary>
        private readonly string m_MarginClass;
        /// <summary>
        /// Volatility Correlation
        /// </summary>
        private readonly int m_VolatilityCorrelation;
        #endregion Données lues à partir du fichier
        //
        private RiskDataNOMXCFMCurveCorrelationCube m_ParentCurve = default;
        #endregion Members

        #region Accessors
        #region Données lues à partir du fichier
        /// <summary>
        /// Curve Name
        /// </summary>
        public string CurveId { get { return m_CurveId; } }
        /// <summary>
        /// Parent Curve Name
        /// </summary>
        public string ParentCurveId { get { return m_ParentCurveId; } }
        /// <summary>
        /// Overlap PC1
        /// </summary>
        public int OverlapPC1 { get { return m_OverlapPC1; } }
        /// <summary>
        /// Overlap PC2
        /// </summary>
        public int OverlapPC2 { get { return m_OverlapPC2; } }
        /// <summary>
        /// Overlap PC3
        /// </summary>
        public int OverlapPC3 { get { return m_OverlapPC3; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        /// <summary>
        /// Margin Classe
        /// </summary>
        public string MarginClass { get { return m_MarginClass; } }
        /// <summary>
        /// Volatility Correlation
        /// </summary>
        public int VolatilityCorrelation { get { return m_VolatilityCorrelation; } }
        #endregion Données lues à partir du fichier
        //
        /// <summary>
        /// Object Parent Curve
        /// </summary>
        public RiskDataNOMXCFMCurveCorrelationCube ParentCurve { get { return m_ParentCurve; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement</param>
        public RiskDataNOMXCFMCurveCorrelationCube(IOTaskDetInOutFileRow pParsingRow)
        {
            m_CurveId = RiskDataLoad.GetRowDataValue(pParsingRow, "CCCId");
            m_ParentCurveId = RiskDataLoad.GetRowDataValue(pParsingRow, "UpperCCCId");
            m_OverlapPC1 = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "OverlapPC1"));
            m_OverlapPC2 = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "OverlapPC2"));
            m_OverlapPC3 = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "OverlapPC3"));
            m_Currency = RiskDataLoad.GetRowDataValue(pParsingRow, "Currency");
            m_MarginClass = RiskDataLoad.GetRowDataValue(pParsingRow, "MarginClass");
            m_VolatilityCorrelation = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "VolatilityCorr"));
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Recherche la courbe parent parmis une liste de courbe
        /// </summary>
        /// <param name="pCorrelationCurves"></param>
        /// <returns></returns>
        public bool SetParent(List<RiskDataNOMXCFMCurveCorrelationCube> pCorrelationCurves)
        {
            bool parentFound = false;
            if (StrFunc.IsFilled(m_ParentCurveId) && (pCorrelationCurves != default(List<RiskDataNOMXCFMCurveCorrelationCube>)))
            {
                m_ParentCurve = pCorrelationCurves.FirstOrDefault(c => (c.CurveId == ParentCurveId) && (c.MarginClass == MarginClass));
                parentFound = (m_ParentCurve != default);
            }
            return parentFound;
        }
        #endregion Methods
    }

    /// <summary>
    /// Curve Hiérarchie
    /// </summary>
    public sealed class RiskDataNOMXCFMCurveHierarchie
    {
        #region Members
        /// <summary>
        /// Ensemble des Correlation Curves
        /// </summary>
        private readonly List<RiskDataNOMXCFMCurveCorrelationCube> m_CorrelationCurves = new List<RiskDataNOMXCFMCurveCorrelationCube>();
        #endregion Members

        #region Accessors
        /// <summary>
        /// Ensemble des Correlation Curves
        /// </summary>
        public List<RiskDataNOMXCFMCurveCorrelationCube> CorrelationCurves { get { return m_CorrelationCurves; } }
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement</param>
        public RiskDataNOMXCFMCurveHierarchie(List<RiskDataNOMXCFMCurveCorrelationCube> pCorrelationCurves)
        {
            if (pCorrelationCurves != default(List<RiskDataNOMXCFMCurveCorrelationCube>))
            {
                m_CorrelationCurves = pCorrelationCurves;
                m_CorrelationCurves.ForEach(c => c.SetParent(m_CorrelationCurves));
            }
        }
        #endregion Constructors

        #region Methods
        /// <summary>
        /// Retourne les CurveCorrelationCube sans parents
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> GetRoots()
        {
            IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> roots = m_CorrelationCurves.Where( c => c.ParentCurve == default);
            return roots;
        }

        /// <summary>
        /// Retourne la CurveCorrelationCube racine de la YieldCurve dans la Hiérarchie
        /// </summary>
        /// <returns></returns>
        public RiskDataNOMXCFMCurveCorrelationCube GetRoot(RiskDataNOMXCFMYieldCurve pCurve, string pMarginClass)
        {
            RiskDataNOMXCFMCurveCorrelationCube root = default;
            if (pCurve != default(RiskDataNOMXCFMYieldCurve))
            {
                IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> correlationCurves = m_CorrelationCurves.Where(c => c.MarginClass == pMarginClass);
                if (correlationCurves.Count() > 0)
                {
                    root = correlationCurves.FirstOrDefault(c => c.CurveId == pCurve.CorrelationCurveId);
                    if (root != default)
                    {
                        while (root.ParentCurve != default)
                        {
                            root = root.ParentCurve;
                        }
                    }
                }
            }
            return root;
        }
        
        /// <summary>
        /// Retourne les CurveCorrelationCube enfant de <param name="pParent"/>
        /// </summary>
        /// <param name="pParent"></param>
        /// <returns></returns>
        public IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> GetChilds(RiskDataNOMXCFMCurveCorrelationCube pParent)
        {
            IEnumerable<RiskDataNOMXCFMCurveCorrelationCube> childs;
            if (pParent != default)
            {
                childs = m_CorrelationCurves.Where(c => c.ParentCurve == pParent);
            }
            else
            {
                childs = new List<RiskDataNOMXCFMCurveCorrelationCube>();
            }
            return childs;
        }
        #endregion Methods
    }

    /// <summary>
    /// Yield Curve
    /// </summary>
    public sealed class RiskDataNOMXCFMYieldCurve
    {
        #region Members
        #region Données lues à partir du fichier
        /// <summary>
        /// Curve Name
        /// </summary>
        private readonly string m_YieldCurveId;
        /// <summary>
        /// Correlation Curve Name
        /// </summary>
        private readonly string m_CorrelationCurveId;
        /// <summary>
        /// Min Num Node
        /// </summary>
        private readonly int m_MinNumNode;
        /// <summary>
        /// Min Num Day
        /// </summary>
        private readonly int m_MinNumDay;
        /// <summary>
        /// Dec In Yield
        /// </summary>
        private readonly int m_DecInYield;
        /// <summary>
        /// Dec Discount Factor
        /// </summary>
        private readonly int m_DecDiscountFactor;
        /// <summary>
        /// Country
        /// </summary>
        private readonly string m_Country;
        /// <summary>
        /// Currency
        /// </summary>
        private readonly string m_Currency;
        /// <summary>
        /// Construction Method
        /// </summary>
        private readonly int m_ConstructionMethod;
        /// <summary>
        /// Day Count Convention
        /// </summary>
        private readonly int m_DayCountConvention;
        /// <summary>
        /// Discoun tMethod
        /// </summary>
        private readonly int m_DiscountMethod;
        #endregion Données lues à partir du fichier
        #endregion Members

        #region Accessors
        #region Données lues à partir du fichier
        /// <summary>
        /// Curve Name
        /// </summary>
        public string YieldCurveId { get { return m_YieldCurveId; } }
        /// <summary>
        /// Correlation Curve Name
        /// </summary>
        public string CorrelationCurveId { get { return m_CorrelationCurveId; } }
        /// <summary>
        /// Min Num Node
        /// </summary>
        public int MinNumNode { get { return m_MinNumNode; } }
        /// <summary>
        /// Min Num Day
        /// </summary>
        public int MinNumDay { get { return m_MinNumDay; } }
        /// <summary>
        /// Dec In Yield
        /// </summary>
        public int DecInYield { get { return m_DecInYield; } }
        /// <summary>
        /// Dec Discount Factor
        /// </summary>
        public int DecDiscountFactor { get { return m_DecDiscountFactor; } }
        /// <summary>
        /// Country
        /// </summary>
        public string Country { get { return m_Country; } }
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get { return m_Currency; } }
        /// <summary>
        /// Construction Method
        /// </summary>
        public int ConstructionMethod { get { return m_ConstructionMethod; } }
        /// <summary>
        /// Day Count Convention
        /// </summary>
        public int DayCountConvention { get { return m_DayCountConvention; } }
        /// <summary>
        /// Discoun tMethod
        /// </summary>
        public int DiscountMethod { get { return m_DiscountMethod; } }
        #endregion Données lues à partir du fichier
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pParsingRow">Parsing d'un enregistrement</param>
        public RiskDataNOMXCFMYieldCurve(IOTaskDetInOutFileRow pParsingRow)
        {
            m_YieldCurveId = RiskDataLoad.GetRowDataValue(pParsingRow, "CRVId");
            m_CorrelationCurveId = RiskDataLoad.GetRowDataValue(pParsingRow, "CCCId");
            m_MinNumNode = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "MinNumNodes"));
            m_MinNumDay = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "MinNumDays"));
            m_DecInYield = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "DecInYield"));
            m_DecDiscountFactor = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "DecInFactor"));
            m_Country = RiskDataLoad.GetRowDataValue(pParsingRow, "CountryName");
            m_Currency = RiskDataLoad.GetRowDataValue(pParsingRow, "Currency");
            m_ConstructionMethod = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "CurveMethod"));
            m_DayCountConvention = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "DayCountConv"));
            m_DiscountMethod = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "DiscountMethod"));
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Price
    /// </summary>
    public sealed class RiskDataNOMXPrice
    {
        #region Members
        /// <summary>
        /// Id Asset
        /// </summary>
        private readonly int m_IdAsset;
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        private readonly NOMXSeries m_Serie;
        /// <summary>
        /// Margin Price
        /// </summary>
        private readonly int m_MarginPrice;
        #endregion Données lues à partir du fichier
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } }
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        public NOMXSeries Serie { get { return m_Serie; } }
        /// <summary>
        /// Margin Price
        /// </summary>
        public int MarginPrice { get { return m_MarginPrice; } }
        #endregion Données lues à partir du fichier
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pSerie"></param>
        /// <param name="pParsingRow">Parsing d'un enregistrement</param>
        public RiskDataNOMXPrice(int pIdAsset, NOMXSeries pSerie, IOTaskDetInOutFileRow pParsingRow)
        {
            m_IdAsset = pIdAsset;
            m_Serie = pSerie;
            m_MarginPrice = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "MargP"));
        }
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset"></param>
        public RiskDataNOMXPrice(int pIdAsset)
        {
            m_IdAsset = pIdAsset;
            m_Serie = default;
            m_MarginPrice = 0;
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// Margin Vector Point
    /// </summary>
    public sealed class RiskDataNOMXMarginVectorPoint
    {
        #region Members
        private readonly int m_IdAsset;
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        private readonly NOMXSeries m_Serie;
        /// <summary>
        /// Représente un point de la matrice 
        /// <para>Valeurs comprises entre 0 et 31</para>
        /// </summary>
        private readonly int m_Point;
        /// <summary>
        /// Cours spot du ss-jacent 
        /// </summary>
        private readonly int m_Spot;
        /// <summary>
        /// Held Low volatility (Bid Low vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_HeldLow;
        /// <summary>
        /// Written Low volatility (Ask Low vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_WrittenLow;
        /// <summary>
        /// Held Middle volatility (Bid Mid vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_HeldMiddle;
        /// <summary>
        /// Written Middle volatility (Ask Mid vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_WrittenMiddle;
        /// <summary>
        /// Held High volatility (Bid High vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_HeldHigh;
        /// <summary>
        /// Written High volatility (Ask High vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        private readonly int m_WrittenHigh;
        #endregion Données lues à partir du fichier
        #endregion Members

        #region Accessors
        /// <summary>
        /// Id Asset
        /// </summary>
        public int IdAsset { get { return m_IdAsset; } }
        #region Données lues à partir du fichier
        /// <summary>
        /// Asset
        /// </summary>
        public NOMXSeries Serie { get { return m_Serie; } }
        /// <summary>
        /// Représente un point de la matrice 
        /// <para>Valeurs comprises entre 0 et 31</para>
        /// </summary>
        public int Point { get { return m_Point; } }
        /// <summary>
        /// Cours spot du ss-jacent 
        /// </summary>
        public int Spot { get { return m_Spot; } }
        /// <summary>
        /// Held Low volatility (Bid Low vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int HeldLow { get { return m_HeldLow; } }
        /// <summary>
        /// Written Low volatility (Ask Low vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int WrittenLow { get { return m_WrittenLow; } }
        /// <summary>
        /// Held Middle volatility (Bid Mid vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int HeldMiddle { get { return m_HeldMiddle; } }
        /// <summary>
        /// Written Middle volatility (Ask Mid vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int WrittenMiddle { get { return m_WrittenMiddle; } }
        /// <summary>
        /// Held High volatility (Bid High vol)
        /// <para>(Held signifie acheter, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int HeldHigh { get { return m_HeldHigh; } }
        /// <summary>
        /// Written High volatility (Ask High vol) 
        /// <para>(written signifie vendre, terminologie souvent utilisée dans le contexte option)</para>
        /// </summary>
        public int WrittenHigh { get { return m_WrittenHigh; } }
        #endregion Données lues à partir du fichier
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pIdAsset"></param>
        /// <param name="pSerie"></param>
        /// <param name="pParsingRow">Parsing d'un enregistrement</param>
        public RiskDataNOMXMarginVectorPoint(int pIdAsset, NOMXSeries pSerie, IOTaskDetInOutFileRow pParsingRow)
        {
            m_IdAsset = pIdAsset;
            m_Serie = pSerie;
            m_Point = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Point"));
            m_Spot = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "Spot"));
            m_HeldLow = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "LowH"));
            m_WrittenLow = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "LowW"));
            m_HeldMiddle = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "MidH"));
            m_WrittenMiddle = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "MidW"));
            m_HeldHigh = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "HigH"));
            m_WrittenHigh = IntFunc.IntValue(RiskDataLoad.GetRowDataValue(pParsingRow, "HigW"));
        }
        #endregion Constructors

        #region Methods
        #endregion Methods
    }

    /// <summary>
    /// RiskData loader for Nasdaq-OMX CFM method
    /// </summary>
    public class RiskDataLoadNOMX : RiskDataLoad
    {
        #region Members
        #region Données importées
        /// <summary>
        /// Ensemble des Margin Curves
        /// </summary>
        private readonly List<RiskDataNOMXCFMMarginCurve> m_MarginCurves;

        /// <summary>
        /// Hierarchie des Correlation Curves
        /// </summary>
        private RiskDataNOMXCFMCurveHierarchie m_CurveCorrelationHierarchie;

        /// <summary>
        /// Ensemble des Yield Curves
        /// </summary>
        private readonly List<RiskDataNOMXCFMYieldCurve> m_YieldCurves;

        /// <summary>
        /// Ensemble des Prices
        /// </summary>
        private readonly List<RiskDataNOMXPrice> m_Prices;

        /// <summary>
        /// Margin Vector
        /// </summary>
        private readonly List<RiskDataNOMXMarginVectorPoint> m_MarginVector;
        #endregion Données importées

        #region Semaphore
        // PM 20190318 [24601] Ajout semaphores pour gestion plusieur fichiers de même type
        /// <summary>
        /// Semaphore pour l'ajout des Cours (assets)
        /// </summary>
        private readonly static SemaphoreSlim semaphoreFMS = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Semaphore pour l'ajout des Risk Cubes
        /// </summary>
        private readonly static SemaphoreSlim semaphoreRCT = new SemaphoreSlim(1, 1);
        /// <summary>
        /// Semaphore pour l'ajout des Margin Vector
        /// </summary>
        private readonly static SemaphoreSlim semaphoreVCT = new SemaphoreSlim(1, 1);
        #endregion Semaphore
        #endregion Members

        #region Accessors
        #region Données importées
        /// <summary>
        /// Ensemble des Margin Curves
        /// </summary>
        public List<RiskDataNOMXCFMMarginCurve> MarginCurves { get { return m_MarginCurves; } }

        /// <summary>
        /// Hierarchie des Correlation Curves
        /// </summary>
        public RiskDataNOMXCFMCurveHierarchie CurveCorrelationHierarchie { get { return m_CurveCorrelationHierarchie; } }

        /// <summary>
        /// Ensemble des Yield Curves
        /// </summary>
        public List<RiskDataNOMXCFMYieldCurve> YieldCurves { get { return m_YieldCurves; } }

        /// <summary>
        /// Ensemble des Prices
        /// </summary>
        public List<RiskDataNOMXPrice> Prices { get { return m_Prices; } }

        /// <summary>
        /// Margin Vector
        /// </summary>
        public List<RiskDataNOMXMarginVectorPoint> MarginVector { get { return m_MarginVector; } }
        #endregion Données importées
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMethodType">Nom de la méthode de calcul pour laquelle charger les données</param>
        /// <param name="pAssetETD">Assets d'un  même marché pour lesquels charger les données</param>
        /// PM 20190222 [24326] Ajout m_MethodType
        ///public RiskDataLoadNOMX(RiskDataAsset pAsset): base(pAsset)
        /// FI 20220321 [XXXXX] pAsset de type IEnumerable<IAssetETDFields>
        public RiskDataLoadNOMX(InitialMarginMethodEnum pMethodType, IEnumerable<IAssetETDIdent> pAssetETD)
            : base(pMethodType, pAssetETD)
        {
            m_MarginCurves = new List<RiskDataNOMXCFMMarginCurve>();
            m_YieldCurves = new List<RiskDataNOMXCFMYieldCurve>();
            m_Prices = new List<RiskDataNOMXPrice>();
            m_MarginVector = new List<RiskDataNOMXMarginVectorPoint>();
        }
        #endregion Constructors
        
        #region Methods
        /// <summary>
        /// Chargement des données via le RiskDataElementLoader
        /// </summary>
        /// <param name="pRiskDataLoader">Objet de chargement du flux de données d'un element d'importation</param>
        /// <returns></returns>
        public override Cst.ErrLevel LoadFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret;
            if (pRiskDataLoader != default(RiskDataElementLoader))
            {
                switch (pRiskDataLoader.TaskInput.InputSourceDataStyle)
                {
                    case Cst.InputSourceDataStyle.NASDAQOMXCCTFILE:
                        if (m_MethodType == InitialMarginMethodEnum.NOMX_CFM)
                        {
                            ret = LoadCCTFile(pRiskDataLoader);
                        }
                        else
                        {
                            ret = Cst.ErrLevel.NOTHINGTODO;
                        }
                        break;
                    case Cst.InputSourceDataStyle.NASDAQOMXFMSFILE:
                        ret = LoadFMSFile(pRiskDataLoader);
                        break;
                    case Cst.InputSourceDataStyle.NASDAQOMXRCTFILE:
                        if (m_MethodType == InitialMarginMethodEnum.NOMX_CFM)
                        {
                            ret = LoadRCTFile(pRiskDataLoader);
                        }
                        else
                        {
                            ret = Cst.ErrLevel.NOTHINGTODO;
                        }
                        break;
                    case Cst.InputSourceDataStyle.NASDAQOMXYCTFILE:
                        if (m_MethodType == InitialMarginMethodEnum.NOMX_CFM)
                        {
                            ret = LoadYCTFile(pRiskDataLoader);
                        }
                        else
                        {
                            ret = Cst.ErrLevel.NOTHINGTODO;
                        }
                        break;
                    case Cst.InputSourceDataStyle.NASDAQOMXVCTFILE:
                        if (m_MethodType == InitialMarginMethodEnum.OMX_RCAR)
                        {
                            ret = LoadVCTFile(pRiskDataLoader);
                        }
                        else
                        {
                            ret = Cst.ErrLevel.NOTHINGTODO;
                        }
                        break;
                    default:
                        ret = Cst.ErrLevel.NOTHINGTODO;
                        break;
                }
            }
            else
            {
                 ret = Cst.ErrLevel.INCORRECTPARAMETER;
            }
            return ret;
        }

        #region private Methods
        /// <summary>
        /// Chargement des données du fichier CCT : Curve Correlation Cubes
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadCCTFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            List<RiskDataNOMXCFMCurveCorrelationCube> correlationCurves = new List<RiskDataNOMXCFMCurveCorrelationCube>();
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                    RiskDataNOMXCFMCurveCorrelationCube ccc = new RiskDataNOMXCFMCurveCorrelationCube(parsingRow);
                    correlationCurves.Add(ccc);
                }
            }
            //
            m_CurveCorrelationHierarchie = new RiskDataNOMXCFMCurveHierarchie(correlationCurves);
            //
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier YCT : Yield Curve Names
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadYCTFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(currentLine);
                    RiskDataNOMXCFMYieldCurve curve = new RiskDataNOMXCFMYieldCurve(parsingRow);
                    m_YieldCurves.Add(curve);
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier FMS : Margin Series Volatilities and Prices, Final
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadFMSFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    NOMXSeries serie = NOMXSeries.ParseNOMXSeries(currentLine);
                    if ((serie != default) && serie.IsToImport())
                    {
                        int idAsset = GetIdAssetETD(serie);
                        if (idAsset > 0)
                        {
                            // Ajout IdAsset en début de ligne pour compatibilité avec Parsing utilisé dans I/O
                            string enhancedLine = idAsset.ToString() + '\t' + currentLine;
                            IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(enhancedLine);
                            RiskDataNOMXPrice price = new RiskDataNOMXPrice(idAsset, serie, parsingRow);
                            //
                            // PM 20190318 [24601] Ajout gestion plusieur fichiers de même type
                            semaphoreFMS.Wait();
                            try
                            {
                                m_Prices.Add(price);
                            }
                            finally
                            {
                                semaphoreFMS.Release();
                            } 
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier RCT : Risk Cubes for Instrument
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadRCTFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            bool isToImport = false;
            RiskDataNOMXCFMMarginCurve curve = default;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    string[] separator = new string[] { "\t" };
                    string[] splittedLine = currentLine.Split(separator, StringSplitOptions.None);
                    //
                    if (splittedLine.Count() > 11)
                    {
                        // Lecture enregistrement : Margin Class Curve
                        //
                        NOMXSeries serie = NOMXSeries.ParseNOMXSeries(currentLine);
                        if (serie != default)
                        {
                            isToImport = serie.IsToImport();
                            if (isToImport)
                            {
                                int idAsset = GetIdAssetETD(serie);
                                if (idAsset > 0)
                                {
                                    curve = new RiskDataNOMXCFMMarginCurve(idAsset, serie, splittedLine);
                                    //
                                    // PM 20190318 [24601] Ajout gestion plusieur fichiers de même type
                                    semaphoreRCT.Wait();
                                    try
                                    {
                                        m_MarginCurves.Add(curve);
                                    }
                                    finally
                                    {
                                        semaphoreRCT.Release();
                                    } 
                                }
                                else
                                {
                                    isToImport = false;
                                }
                            }
                        }
                        else
                        {
                            isToImport = false;
                        }
                    }
                    else if ((isToImport) && (curve != default))
                    {
                        // Lecture enregistrement : Instrument Curve Node Values
                        //
                        RiskDataNOMXCFMCurveNode node = new RiskDataNOMXCFMCurveNode(splittedLine, curve.DecimalInValue, curve.DecimalInDiscountFactor);
                        curve.AddNode(node);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Chargement des données du fichier VCT : Margin Vector Information
        /// </summary>
        /// <param name="pRiskDataLoader"></param>
        /// <returns></returns>
        private Cst.ErrLevel LoadVCTFile(RiskDataElementLoader pRiskDataLoader)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.SUCCESS;
            //
            int lineNumber = 0;
            int guard = 99999999;
            while (lineNumber++ < guard)
            {
                string currentLine = pRiskDataLoader.StreamReader.ReadLine();
                if (currentLine == null)
                {
                    break;
                }
                else
                {
                    NOMXSeries serie = NOMXSeries.ParseNOMXSeries(currentLine);
                    if ((serie != default) && serie.IsToImport())
                    {
                        int idAsset = GetIdAssetETD(serie);
                        if (idAsset > 0)
                        {
                            // Ajout IdAsset en début de ligne pour compatibilité avec ancien Parsing utilisé dans I/O
                            string enhancedLine = idAsset.ToString() + '\t' + currentLine;
                            IOTaskDetInOutFileRow parsingRow = pRiskDataLoader.ParseLine(enhancedLine);
                            RiskDataNOMXMarginVectorPoint vectorPoint = new RiskDataNOMXMarginVectorPoint(idAsset, serie, parsingRow);
                            //
                            // PM 20190318 [24601] Ajout gestion plusieur fichiers de même type
                            semaphoreVCT.Wait();
                            try
                            {
                                m_MarginVector.Add(vectorPoint);
                            }
                            finally
                            {
                                semaphoreVCT.Release();
                            } 
                        }
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Rechercher l'Id Asset d'une serie
        /// </summary>
        /// <param name="pSerie">Données de la série</param>
        /// <returns>null si l'asset n'est pas trouvé</returns>
        /// FI 20220321 [XXXXX] Refactoring
        private int GetIdAssetETD(NOMXSeries pSerie)
        {
            MarketAssetETDRequest request = NOMXTools.GetAssetETDRequest(pSerie);
            MarketAssetETDRequestSettings settings = NOMXTools.GetAssetETDRequestSettings(pSerie);
            // Rechercher de l'asset
            IAssetETDIdent asset = m_DataAsset.GetAsset(settings, request);
            return (null != asset) ? asset.IdAsset : 0;
        }
        #endregion private Methods
        #endregion Methods
    }
}
