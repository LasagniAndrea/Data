using System.Runtime.Serialization;
//
using EFS.ApplicationBlocks.Data;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    /// <summary>
    /// Data container pour les volumes de marché pour le calcul du Concentration Risk Margin ECC.
    /// </summary>
    /// <remarks>Main Spheres reference table: IMECCCONR_H</remarks>
    [DataContract(Name = DataHelper<ECCRiskMarketVolume>.DATASETROWNAME,
        Namespace = DataHelper<ECCRiskMarketVolume>.DATASETNAMESPACE)]
    public sealed class ECCRiskMarketVolume
    {
        #region Members
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        [DataMember(Name = "COMBCOMSTRESS", Order = 1)]
        public string CombinedCommodityStress
        { get; set; }

        /// <summary>
        /// Market Volume
        /// </summary>
        [DataMember(Name = "MARKETVOLUME", Order = 2)]
        public double MarketVolumeDouble
        { get; set; }
        public decimal MarketVolume
        { get { return (decimal)MarketVolumeDouble; } }
        #endregion Members
    }

    /// <summary>
    /// Data container pour les Internal Id des Derivative Contracts des Combined Commodity Stress Group.
    /// </summary>
    /// <remarks>Main Spheres reference table: GCONTRACT, CONTRACTG</remarks>
    [DataContract(Name = DataHelper<ECCRiskMarketVolume>.DATASETROWNAME,
        Namespace = DataHelper<ECCRiskMarketVolume>.DATASETNAMESPACE)]
    public sealed class ECCRiskCombComStressContract
    {
        #region Members
        /// <summary>
        /// Combined Commodity Stress
        /// </summary>
        [DataMember(Name = "COMBCOMSTRESS", Order = 1)]
        public string CombinedCommodityStress
        { get; set; }

        /// <summary>
        /// Derivative Contract Symbol
        /// </summary>
        [DataMember(Name = "CONTRACTSYMBOL", Order = 2)]
        public string ContractSymbol
        { get; set; }

        /// <summary>
        /// Derivative Contract internal id
        /// </summary>
        [DataMember(Name = "IDDC", Order = 3)]
        public int ContractId
        { get; set; }
        #endregion Members
    }
}
