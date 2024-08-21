using System.Collections.Generic;
using System.Linq;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Class de position pour le calcul du Concentration Risk Margin de l'ECC
    /// </summary>
    public abstract class ECCConRiskMarginPosition
    {
        #region Members
        protected int m_ContractId = 0;
        protected string m_ContractSymbol = default;
        protected decimal m_SignedQuantity = 0;
        protected decimal m_PositionSize = 0;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Contract Id
        /// </summary>
        public int ContractId
        {
            get { return m_ContractId; }
        }
        /// <summary>
        /// Contract Symbol
        /// </summary>
        public string ContractSymbol
        {
            get { return m_ContractSymbol; }
        }
        /// <summary>
        /// Signed Quantity
        /// </summary>
        public decimal SignedQuantity
        {
            get { return m_SignedQuantity; }
        }
        /// <summary>
        /// Position Size
        /// </summary>
        public decimal PositionSize
        {
            get { return m_PositionSize; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractId"></param>
        /// <param name="pSignedQuantity"></param>
        /// <param name="pPositionSize"></param>
        public ECCConRiskMarginPosition(string pContractSymbol, int pContractId, decimal pSignedQuantity, decimal pPositionSize)
        {
            m_ContractSymbol = pContractSymbol;
            m_ContractId = pContractId;
            m_SignedQuantity = pSignedQuantity;
            m_PositionSize = pPositionSize;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractId"></param>
        public ECCConRiskMarginPosition(string pContractSymbol, int pContractId)
        {
            m_ContractSymbol = pContractSymbol;
            m_ContractId = pContractId;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class de position par Asset pour le calcul du Concentration Risk Margin de l'ECC
    /// </summary>
    public class ECCConRiskMarginAssetPosition : ECCConRiskMarginPosition
    {
        #region Members
        private readonly int m_AssetId = 0;
        private readonly decimal m_ContractMultiplier = 0;
        #endregion Members
        #region Accessors
        /// <summary>
        /// m_Asset Id
        /// </summary>
        public int AssetId
        {
            get { return m_AssetId; }
        }
        /// <summary>
        /// Contract Multiplier
        /// </summary>
        public decimal ContractMultiplier
        {
            get { return m_ContractMultiplier; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pAssetId"></param>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractId"></param>
        /// <param name="pContractMultiplier"></param>
        /// <param name="pSignedQuantity"></param>
        /// <param name="pPositionSize"></param>
        public ECCConRiskMarginAssetPosition(int pAssetId, string pContractSymbol, int pContractId, decimal pContractMultiplier, decimal pSignedQuantity, decimal pPositionSize)
            : base(pContractSymbol, pContractId, pSignedQuantity, pPositionSize)
        {
            m_AssetId = pAssetId;
            m_ContractMultiplier = pContractMultiplier;
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class de position par Contract pour le calcul du Concentration Risk Margin de l'ECC
    /// </summary>
    public class ECCConRiskMarginContractPosition : ECCConRiskMarginPosition
    {
        #region Members
        private readonly IEnumerable<ECCConRiskMarginAssetPosition> m_AssetPosition = default;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Asset Position
        /// </summary>
        public IEnumerable<ECCConRiskMarginAssetPosition> AssetPosition
        {
            get { return m_AssetPosition; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractId"></param>
        /// <param name="pSignedQuantity"></param>
        /// <param name="pPositionSize"></param>
        /// <param name="pAssetPosition"></param>
        public ECCConRiskMarginContractPosition(string pContractSymbol, int pContractId, decimal pSignedQuantity, decimal pPositionSize, IEnumerable<ECCConRiskMarginAssetPosition> pAssetPosition)
            : base(pContractSymbol, pContractId, pSignedQuantity, pPositionSize)
        {
            m_AssetPosition = pAssetPosition;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pContractSymbol"></param>
        /// <param name="pContractId"></param>
        /// <param name="pAssetPosition"></param>
        public ECCConRiskMarginContractPosition(string pContractSymbol, int pContractId, IEnumerable<ECCConRiskMarginAssetPosition> pAssetPosition)
            : base(pContractSymbol, pContractId)
        {
            m_AssetPosition = pAssetPosition;
            if (pAssetPosition != default(IEnumerable<ECCConRiskMarginAssetPosition>))
            {
                m_SignedQuantity = pAssetPosition.Select(p => p.SignedQuantity).Sum();
                m_PositionSize = m_AssetPosition.Select(p => p.PositionSize).Sum();
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class de calcul intermédiaire du Concentration Risk Margin de l'ECC
    /// </summary>
    public class ECCConRiskMarginUnit
    {
        #region Members
        public readonly decimal AdditionalAddOn;
        private readonly string m_CombinedCommodityStress = default;
        private readonly decimal m_DailyMarketVolume = 0;
        private readonly decimal m_AbsoluteCumulativePosition = 0;
        private readonly Dictionary<int, ECCConRiskMarginContractPosition> m_PositionSize = default;
        private readonly decimal? m_LiquidationPeriod = default;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        public string CombinedCommodityStress
        {
            get {return m_CombinedCommodityStress;}
        }
        /// <summary>
        /// Daily Market Volume
        /// </summary>
        public decimal DailyMarketVolume
        {
            get {return m_DailyMarketVolume;}
        }
        /// <summary>
        /// Absolute Cumulative Position Size
        /// </summary>
        public decimal AbsoluteCumulativePosition
        {
            get {return m_AbsoluteCumulativePosition;}
        }
        /// <summary>
        /// Position Size
        /// </summary>
        public Dictionary<int, ECCConRiskMarginContractPosition> PositionSize
        {
            get {return m_PositionSize;}
        }
        /// <summary>
        /// LiquidationPeriod
        /// </summary>
        public decimal? LiquidationPeriod
        {
            get {return m_LiquidationPeriod;}
        }
        /// <summary>
        /// Weighted Absolute Cumulative Position Size
        /// </summary>
        public decimal WeightedAbsCumulPosition
        {
            get { return m_LiquidationPeriod.HasValue ? m_AbsoluteCumulativePosition * m_LiquidationPeriod.Value : 0; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pMarketVolume"></param>
        /// <param name="pPositionSize"></param>
        /// <param name="pAddOnDays"></param>
        public ECCConRiskMarginUnit(ECCRiskMarketVolume pMarketVolume, Dictionary<int, ECCConRiskMarginContractPosition> pPositionSize, decimal pAddOnDays)
        {
            if ((pMarketVolume != default(ECCRiskMarketVolume)) && (pPositionSize != default(Dictionary<int, ECCConRiskMarginContractPosition>)))
            {
                AdditionalAddOn = pAddOnDays;
                m_CombinedCommodityStress = pMarketVolume.CombinedCommodityStress;
                m_DailyMarketVolume = pMarketVolume.MarketVolume;
                m_PositionSize = pPositionSize;
                m_AbsoluteCumulativePosition = System.Math.Abs(pPositionSize.Select(p => p.Value.PositionSize).Sum());
                m_LiquidationPeriod = (m_DailyMarketVolume != 0) ? System.Math.Abs(m_AbsoluteCumulativePosition/m_DailyMarketVolume) + AdditionalAddOn : default;
            }
        }
        #endregion Constructors
    }

    /// <summary>
    /// Class de calcul du Concentration Risk Margin de l'ECC
    /// </summary>
    public class ECCConRiskMargin
    {
        #region Members
        private readonly IEnumerable<ECCConRiskMarginUnit> m_ConcentrationRiskMarginUnits = default;
        private readonly Money m_ConcentrationRiskMargin = default;
        private readonly decimal m_AbsoluteCumulativePosition = 0;
        private readonly decimal m_WeightedAbsCumulPosition = 0;
        private readonly decimal m_LiquidationPeriod = 0;
        private readonly Money m_SpanAmount = default;
        #endregion Members
        #region Accessors
        /// <summary>
        /// Ensemble des ConcentrationRiskMarginUnits
        /// </summary>
        public IEnumerable<ECCConRiskMarginUnit> ConcentrationRiskMarginUnits
        {
            get { return m_ConcentrationRiskMarginUnits; }
        }
        /// <summary>
        /// Span Amount
        /// </summary>
        public Money SpanAmount
        {
            get { return m_SpanAmount; }
        }
        /// <summary>
        /// Concentration Risk Margin
        /// </summary>
        public Money ConcentrationRiskMargin
        {
            get { return m_ConcentrationRiskMargin; }
        }
        /// <summary>
        /// Absolute Cumulative Position
        /// </summary>
        public decimal AbsoluteCumulativePosition
        {
            get { return m_AbsoluteCumulativePosition; }
        }
        /// <summary>
        /// Weighted Absolute Cumulative Position
        /// </summary>
        public decimal WeightedAbsCumulPosition
        {
            get { return m_WeightedAbsCumulPosition; }
        }
        /// <summary>
        /// Liquidation Period
        /// </summary>
        public decimal LiquidationPeriod
        {
            get { return m_LiquidationPeriod; }
        }
        #endregion Accessors
        #region Constructors
        /// <summary>
        /// Constructeur
        /// </summary>
        /// <param name="pConRiskMrg"></param>
        /// <param name="pSpanAmount"></param>
        public ECCConRiskMargin(IEnumerable<ECCConRiskMarginUnit> pConRiskMrg, Money pSpanAmount)
        {
            if (pConRiskMrg != default(IEnumerable<ECCConRiskMarginUnit>))
            {
                m_ConcentrationRiskMarginUnits = pConRiskMrg;
                m_SpanAmount = pSpanAmount;
                m_AbsoluteCumulativePosition = pConRiskMrg.Sum(c => c.AbsoluteCumulativePosition);
                m_WeightedAbsCumulPosition = pConRiskMrg.Sum(c => c.WeightedAbsCumulPosition);
                m_LiquidationPeriod = (m_AbsoluteCumulativePosition > 0) ? m_WeightedAbsCumulPosition / m_AbsoluteCumulativePosition : 0;
                m_ConcentrationRiskMargin = new Money(
                    pSpanAmount.Amount.DecValue * (decimal)System.Math.Max(0, System.Math.Sqrt((double)m_LiquidationPeriod / 2) - 1),
                    pSpanAmount.Currency);
            }
        }
        #endregion Constructors
    }
}
