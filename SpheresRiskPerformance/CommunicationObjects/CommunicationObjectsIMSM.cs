using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using FpML.Enum;
using System;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode IMSM 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.IMSMMethod"/>)
    /// à l'objet référentiel de la feuille de calcul
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
    /// du calcul par la méthode IMSM (<see cref="EfsML.v30.MarginRequirement.IMSMCalculationMethod"/> 
    /// </summary>
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class IMSMCalcMethCom : CalcMethComBase
    {
        #region Members
        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency { set; get; }

        /// <summary>
        /// Date d'application du déposit
        /// </summary>
        public DateTime DtIMSM { set; get; }

        /// <summary>
        /// Business Center utilisé pour la date d'application du déposit
        /// </summary>
        public string BusinessCenter { set; get; }

        /// <summary>
        /// Paramètres globaux de la méthode
        /// </summary>
        public IMSMGlobalParameterCom IMSMParameter { set; get; }

        /// <summary>
        /// Taux de change
        /// </summary>
        /// PM 20170808 [23371] Ajout
        public IMExchangeRateParameterCom[] ExchangeRate { set; get; }

        /// <summary>
        /// Données statistiques utlisées par la méthode
        /// </summary>
        public IMSMExposureCom Exposure { set; get; }

        /// <summary>
        /// Données statistiques courantes utlisées par la méthode CESM
        /// </summary>
        /// PM 20170808 [23371] Ajout
        public IMSMCurrentExposureCom[] CurrentExposure { set; get; }

        /// <summary>
        /// Données du calcul
        /// </summary>
        public CESMCalculationCom CESMCalculationData { set; get; }

        /// <summary>
        /// Données du calcul
        /// </summary>
        public IMSMCalculationCom IMSMCalculationData { set; get; }
        #endregion Members

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public IMSMCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Communication object enfant de IMSMGlobalParameterCom
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMSMCESMParameterCom
    {
        #region Members
        /// <summary>
        /// Identifiant du Commodity Contract
        /// </summary>
        public string ContractIdentifier { set; get; }

        /// <summary>
        /// Id interne du Commodity Contract
        /// </summary>
        public int IdCC { set; get; }

        /// <summary>
        /// Margin Parameter pour les Achats
        /// </summary>
        public decimal MarginParameterBuy { set; get; }

        /// <summary>
        /// Margin Parameter pour les Ventes
        /// </summary>
        public decimal MarginParameterSell { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom
    /// </summary>
    public sealed class IMSMGlobalParameterCom
    {
        #region Members
        /// <summary>
        /// Indicateur de prise en considération des jours fériés
        /// </summary>
        public bool IsWithHolidayAdjustment { set; get; }
        /// <summary>
        /// Taille de la fenêtre de prise en compte des données statistiques
        /// </summary>
        public int WindowSizeStatistic { set; get; }
        /// <summary>
        /// Taille de la fenêtre de la partie maximum du calcul de l'IMSM
        /// </summary>
        public int WindowSizeMaximum { set; get; }
        /// <summary>
        /// Paramètre EWMA (Exponentially Weighted Moving Average / Moyennes Mobiles Pondérées Expontiellement) pour calculer la volatilité dans le calcul de l'écart-type
        /// </summary>
        public decimal EWMAFactor { set; get; }
        /// <summary>
        /// Multiplicateur pour l'écart-type utilisé dans la partie statistique du calcul de l'IMSM
        /// </summary>
        public decimal Alpha { set; get; }
        /// <summary>
        /// Multiplicateur pour la partie déterministe du calcul de l'IMSM
        /// </summary>
        public decimal Beta { set; get; }
        /// <summary>
        /// Minimum absolu initial de l'IMSM pour les "MinIMSMInitialWindowSize"(30) premiers jours après l'admission
        /// </summary>
        public decimal MinIMSMInitial { set; get; }
        /// <summary>
        /// Nombre de jours pendant lesquels appliquer le montant minimum absolu initial de l'IMSM
        /// </summary>
        public int MinIMSMInitialWindowSize { set; get; }
        /// <summary>
        /// Minimum absolu de l'IMSM
        /// </summary>
        public decimal MinIMSM { set; get; }
        /// <summary>
        /// Calcul uniquement du Current Exposure Spot Margin de l'ECC
        /// </summary>
        // PM 20200910 [25482] Ajout IsCalcCESMOnly
        public bool IsCalcCESMOnly { set; get; }
        /// <summary>
        /// CESM Parameters
        /// </summary>
        /// PM 20170808 [23371] Ajout
        public IMSMCESMParameterCom[] CESMParameters { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom contenant les données statistiques
    /// </summary>
    public sealed class IMSMExposureCom
    {
        #region Members
        /// <summary>
        /// Ensemble des "Exposure" compris dans la fenêtre de calcul
        /// </summary>
        public Dictionary<DateTime, decimal> Exposure { set; get; }

        /// <summary>
        /// T0Exposure
        /// </summary>
        public decimal T0Exposure { set; get; }

        /// <summary>
        /// Date minimum de la fenetre de données statistiques
        /// </summary>
        public DateTime WindowDateMin { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom contenant les données statistiques courantes pour un commodity contract
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMSMCurrentExposureCom
    {
        #region Members
        /// <summary>
        /// Identifiant interne du Commodity Contract
        /// </summary>
        public int IdCC { set; get; }

        /// <summary>
        /// Identifiant interne de l'Asset du Commodity Contract
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        public int IdAsset { set; get; }

        /// <summary>
        /// Identifiant de l'Asset du Commodity Contract
        /// </summary>
        // PM 20200910 [25482] Ajout IdAsset
        public string AssetIdentifier { set; get; }

        /// <summary>
        /// Exposure à l'achat
        /// </summary>
        public decimal ExposureBuy { set; get; }

        /// <summary>
        ///  Exposure à la vente
        /// </summary>
        public decimal ExposureSell { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom contenant les données du calcul IMSM
    /// </summary>
    public sealed class IMSMCalculationCom
    {
        #region Members
        /// <summary>
        /// Nombre de jours pour l'ajustement jour férié
        /// </summary>
        // PM 20231027 [XXXXX] Le Factor Lambda Holiday Adjustment passe de int à decimal
        public decimal HolydayAdjDays { set; get; }

        /// <summary>
        /// Montant de l'ajustement jour férié
        /// </summary>
        public decimal HolydayAdjAmount { set; get; }

        /// <summary>
        /// Date effective du calcul du déposit
        /// </summary>
        public DateTime EffectiveImsmDate { set; get; }

        /// <summary>
        /// Date de début d'existance d'une activité
        /// </summary>
        /// PM 20170602 [23212] Renommage de FirstActivityDate en AgreementDate
        public DateTime AgreementDate { set; get; }
        
        /// <summary>
        /// T0Exposure
        /// </summary>
        public decimal T0Exposure { set; get; }

        /// <summary>
        /// Moyenne des statistiques
        /// </summary>
        public decimal Mean { set; get; }

        /// <summary>
        /// Nombre de datapoint
        /// </summary>
        public decimal NoDataPoint { set; get; }

        /// <summary>
        /// Standard Deviation
        /// </summary>
        public decimal StandardDeviation { set; get; }

        /// <summary>
        /// Standard Deviation with Security factor
        /// </summary>
        public decimal SDS { set; get; }

        /// <summary>
        /// Maximum sur la fenêtre réduite de données
        /// </summary>
        public decimal MaxShortWindow { set; get; }

        /// <summary>
        /// Beta factor * Maximum sur la fenêtre réduite de données
        /// </summary>
        public decimal BetaMax { set; get; }
        
        /// <summary>
        /// Montant du déposit
        /// </summary>
        public decimal MainImsm { set; get; }

        /// <summary>
        /// Montant du déposit arrondi
        /// </summary>
        /// PM 20170808 [23371] Ajout
        public decimal RoundedImsm { set; get; }
        #endregion Members
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom contenant les données du calcul CESM
    /// </summary>
    public sealed class CESMCalculationCom
    {
        /// <summary>
        /// Montant du déposit CESM
        /// </summary>
        public decimal CESMAMount { set; get; }
    }

    /// <summary>
    /// Communication object enfant de IMSMCalcMethCom contenant les taux de change
    /// </summary>
    /// PM 20170808 [23371] New
    public sealed class IMExchangeRateParameterCom : IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1032);
        #endregion static Members

        /// <summary>
        /// Devise 1
        /// </summary>
        public string Currency1 { get; set; }
        /// <summary>
        /// Devise 2
        /// </summary>
        public string Currency2 { get; set; }
        /// <summary>
        /// Taux de change
        /// </summary>
        public decimal Rate { get; set; }
        /// <summary>
        /// Base de cotation
        /// </summary>
        public QuoteBasisEnum QuoteBasis { get; set; }
        #endregion Members

        #region Accessors
        /// <summary>
        /// Base de cotation sous la forme Devise./Devise
        /// </summary>
        public string QuoteBasisString
        { get { return (QuoteBasis == FpML.Enum.QuoteBasisEnum.Currency1PerCurrency2) ? Currency2 + "./" + Currency1 : Currency1 + "./" + Currency2; } }
        #endregion Accessors

        #region IMissingCommunicationObject Members
        /// <summary>
        /// Set to true when the current parameter has not been found in the parameters set, 
        /// but it has been built to stock one set of asset elements in position and no parameters have been found for them.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Error code to log the missing parameter event
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01032"; }
            get { return m_SysMsgCode; }
        }
        #endregion IMissingCommunicationObject Members

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCurrency1"></param>
        /// <param name="pCurrency2"></param>
        /// <param name="pQuoteBasis"></param>
        /// <param name="pRate"></param>
        /// <param name="pMissing"></param>
        public IMExchangeRateParameterCom(string pCurrency1, string pCurrency2, QuoteBasisEnum pQuoteBasis, decimal pRate, bool pMissing)
        {
            Currency1 = pCurrency1;
            Currency2 = pCurrency2;
            QuoteBasis = pQuoteBasis;
            Rate = pRate;
            Missing = pMissing;
        }
        #endregion Constructor
    }
}
