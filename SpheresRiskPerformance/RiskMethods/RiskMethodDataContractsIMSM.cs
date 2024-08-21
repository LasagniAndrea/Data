using EFS.ApplicationBlocks.Data;
using System;
using System.Runtime.Serialization;

namespace EFS.SpheresRiskPerformance.RiskMethods
{
    #region IMSM method
    /// <summary>
    /// Data container for the Security Addon parameters for IMSM.
    /// </summary>
    /// <remarks>Spheres reference table: IMSMADDON_H</remarks>
    [DataContract(Name = DataHelper<IMSMSecurityAddonParameter>.DATASETROWNAME,
        Namespace = DataHelper<IMSMSecurityAddonParameter>.DATASETNAMESPACE)]
    internal sealed class IMSMSecurityAddonParameter
    {
        /// <summary>
        /// Number of the DataPoint
        /// </summary>
        [DataMember(Name = "DATAPOINTNUMBER", Order = 1)]
        public int DataPoint
        { get; set; }

        /// <summary>
        /// Security Addon associated to the DataPoint
        /// </summary>
        [DataMember(Name = "SECURITYADDON", Order = 2)]
        public double SecurityAddonDouble
        { get; set; }
        public decimal SecurityAddon
        { get { return (decimal)SecurityAddonDouble; } }
    }

    /// <summary>
    /// Data container for the Holiday Adjustment parameters for IMSM.
    /// </summary>
    /// <remarks>Spheres reference table: IMSMHOLIDAYADJ_H</remarks>
    [DataContract(Name = DataHelper<IMSMHolidayAdjustmentParameter>.DATASETROWNAME,
        Namespace = DataHelper<IMSMHolidayAdjustmentParameter>.DATASETNAMESPACE)]
    internal sealed class IMSMHolidayAdjustmentParameter
    {
        /// <summary>
        /// Calculation Date
        /// </summary>
        [DataMember(Name = "CALCULATIONDATE", Order = 1)]
        public DateTime CalculationDate
        { get; set; }

        /// <summary>
        /// Effective Date
        /// </summary>
        [DataMember(Name = "EFFECTIVEDATE", Order = 2)]
        public DateTime EffectiveDate
        { get; set; }

        /// <summary>
        /// Lambda Factor
        /// </summary>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        [DataMember(Name = "LAMBDAFACTOR", Order = 3)]
        public decimal LambdaFactor
        { get; set; }
    }

    /// <summary>
    /// Class de chargement des dates d'agrément
    /// </summary>
    /// PM 20170602 [23212] Ajout
    [DataContract(Name = DataHelper<IMSMAgreementDate>.DATASETROWNAME, Namespace = DataHelper<IMSMAgreementDate>.DATASETNAMESPACE)]
    internal sealed class IMSMAgreementDate
    {
        #region Accessors
        /// <summary>
        /// Id interne de l'acteur
        /// </summary>
        [DataMember(Name = "IDA", Order = 1)]
        public int IdA
        { get; set; }

        /// <summary>
        /// Date de signature de l'agrément
        /// </summary>
        [DataMember(Name = "DTSIGNATURE", Order = 2)]
        public DateTime DtAgreement
        { get; set; }

        /// <summary>
        /// Id interne de l'acteur CSS
        /// </summary>
        public int IdCSS
        { get; set; }
        #endregion Accessors
    }

    /// <summary>
    /// Class de chargement des Margin Parameters pour le CESM (Current Exposure Spot Market)
    /// </summary>
    /// PM 20170808 [23371] Ajout
    [DataContract(Name = DataHelper<IMSMCESMMarginParameter>.DATASETROWNAME, Namespace = DataHelper<IMSMCESMMarginParameter>.DATASETNAMESPACE)]
    internal sealed class IMSMCESMMarginParameter
    {
        #region Accessors
        /// <summary>
        /// Identifiant du Commodity Contract
        /// </summary>
        [DataMember(Name = "IDENTIFIER", Order = 1)]
        public string ContractIdentifier
        { get; set; }
        
        /// <summary>
        /// Id interne du Commodity Contract
        /// </summary>
        [DataMember(Name = "IDCC", Order = 2)]
        public int IdCC
        { get; set; }

        /// <summary>
        /// Margin Parameter pour les Achats
        /// </summary>
        [DataMember(Name = "MP_BUY", Order = 3)]
        public double MarginParameterBuyDouble
        { get; set; }
        public decimal MarginParameterBuy
        { get { return (decimal)MarginParameterBuyDouble; } }

        /// <summary>
        /// Margin Parameter pour les Ventes
        /// </summary>
        [DataMember(Name = "MP_SELL", Order = 4)]
        public double MarginParameterSellDouble
        { get; set; }
        public decimal MarginParameterSell
        { get { return (decimal)MarginParameterSellDouble; } }
        #endregion Accessors
    }
    #endregion IMSM method
}
