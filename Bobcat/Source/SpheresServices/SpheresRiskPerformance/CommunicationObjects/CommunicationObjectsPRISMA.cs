using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using FpML.Interface;
using System;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode PRISMA 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.PrismaMethod"/>)
    /// à l'objet référentiel de la feuille de calcul
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
    /// du calcul par la méthode PRISMA Margin (<see cref="EfsML.v30.MarginRequirement.PrismaMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class PrismaCalcMethCom : CalcMethComBase
    {
        #region Members
        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency;

        /// <summary>
        /// Id de l'acteur
        /// </summary>
        //PM 20150416 [20957] Add IdA
        public int IdA;

        /// <summary>
        /// Id du book
        /// </summary>
        //PM 20150416 [20957] Add IdB
        public int IdB;

        /// <summary>
        /// Version de Prisma
        /// </summary>
        //PM 20150417 [20957] Add PrismaRelease
        public string PrismaRelease;
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public PrismaCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor

        #region RBM for Prisma
        // PM 20151116 [21561] Ajout pour RBM
        // PM 20181002 [XXXXX] Désactivation de RBM dans Prisma
        //public TimsEurexMarginCalculationMethodCommunicationObject RbmMethComObj;
        #endregion RBM for Prisma
    }

    /// <summary>
    /// Objet de communication identifiant un groupe de liquidation Prisma <see cref="EfsML.v30.MarginRequirement.PrismaLiquidGroupParameter"/>, 
    /// enfant d'un objet de type PrismaCalcMethCom <see cref="PrismaCalcMethCom"/>, 
    /// utilisé pour construire les sous-éléments du noeud de calcul de risque Prisma (typeof <see cref="EfsML.v30.MarginRequirement.PrismaMarginCalculationMethod"/>)
    /// </summary>
    public sealed class PrismaLiquidGroupCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Id interne du groupe de liquidation
        /// </summary>
        public int IdLg;

        /// <summary>
        /// Liquidation Group Identifier
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Type de devise
        /// </summary>
        public string CurrencyTypeFlag;

        /// <summary>
        /// Devise de clearing
        /// </summary>
        public string ClearingCurrency;

        /// <summary>
        /// Initial Margin
        /// </summary>
        public IMoney[] InitialMargin;

        /// <summary>
        /// Premium Margin
        /// </summary>
        /// PM 20150907 [21236] Add PremiumMargin
        public IMoney[] PremiumMargin;

        /// <summary>
        /// Margin Requirement
        /// </summary>
        /// PM 20150907 [21236] Add MarginRequirement
        public IMoney[] MarginRequirement;
        #endregion Members

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Positions partielles relative aux paramètres de risque courant
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données pour construire
        /// les sous-éléments (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètres de risque
        /// (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant partiel de risque en rapport au paramètre de risque courant
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Définir à vrai si le paramètre actuel n'a pas été trouvé dans l'ensemble des paramètres,
        /// mais il a été bati pour stocker un ensemble d'éléments d'actifs en position pour lesquels aucun paramètre n'a été trouvé.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur pour identifier l'événement paramètre manquant
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant un groupe de liquidation Prisma <see cref="EfsML.v30.MarginRequirement.PrismaLiquidGroupSplitParameter"/>, 
    /// enfant d'un objet de type PrismaCalcMethCom <see cref="PrismaCalcMethCom"/>, 
    /// utilisé pour construire les sous-éléments du noeud de calcul de risque Prisma (typeof <see cref="EfsML.v30.MarginRequirement.PrismaMarginCalculationMethod"/>)
    /// </summary>
    public sealed class PrismaLiquidGroupSplitCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Id interne du split de groupe de liquidation
        /// </summary>
        public int IdLgs;
        
        /// <summary>
        /// Liquidation Group Split Identifier
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Aggregation Method
        /// </summary>
        public string AggregationMethod;

        /// <summary>
        /// Risk Method
        /// </summary>
        public string RiskMethod;

        /// <summary>
        /// Market Risk
        /// </summary>
        public IMoney[] MarketRisk;

        /// <summary>
        /// Time To Expiry Adjustment
        /// </summary>
        // PM 20180903 [24015] Prisma v8.0 : add TimeToExpiryAdjustment
        public IMoney[] TimeToExpiryAdjustment;
        
        /// <summary>
        /// Liquidity Risk
        /// </summary>
        public IMoney[] LiquidityRisk;

        /// <summary>
        /// Représente une liste avec certains résultats intermédiaires évalués pendant l'évaluation de la Liquidity Risk
        /// <para>La clé du dictionnaire est l'IdAsset</para>
        /// </summary>
        public Dictionary<int, PrismaAssetLiquidityRiskCom> AssetLiquidityRisk;

        /// <summary>
        /// Initial Margin
        /// </summary>
        public IMoney[] InitialMargin;

        /// <summary>
        /// Premium Margin
        /// </summary>
        // PM 20140619 [19911] New
        public IMoney[] PremiumMargin;

        /// <summary>
        /// Present Value
        /// </summary>
        // PM 20200826 [25467] New
        public IMoney[] PresentValue;

        /// <summary>
        /// Maximal Lost
        /// </summary>
        // PM 20200826 [25467] New
        public IMoney[] MaximalLost;

        /// <summary>
        /// Long Option Credit
        /// </summary>
        // PM 20140618 [19911] New
        public IMoney[] LongOptionCredit;

        /// <summary>
        /// Total Initial Margin
        /// </summary>
        // PM 20140618 [19911] New
        public IMoney[] TotalInitialMargin;
        #endregion Members

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Positions partielles relative aux paramètres de risque courant
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données pour construire
        /// les sous-éléments (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètres de risque
        /// (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant partiel de risque en rapport au paramètre de risque courant
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Définir à vrai si le paramètre actuel n'a pas été trouvé dans l'ensemble des paramètres,
        /// mais il a été bati pour stocker un ensemble d'éléments d'actifs en position pour lesquels aucun paramètre n'a été trouvé.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur pour identifier l'événement paramètre manquant
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant un groupe de liquidation Prisma <see cref="EfsML.v30.MarginRequirement.PrismaLiquidGroupSplitParameter"/>, 
    /// enfant d'un objet de type PrismaCalcMethCom <see cref="PrismaCalcMethCom"/>, 
    /// utilisé pour construire les sous-éléments du noeud de calcul de risque Prisma (typeof <see cref="EfsML.v30.MarginRequirement.PrismaMarginCalculationMethod"/>)
    /// </summary>
    public sealed class PrismaRiskMeasureSetCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Id interne du jeu de mesure de risque
        /// </summary>
        public int IdRms;

        /// <summary>
        /// Identifier du jeu de mesure de risque
        /// </summary>
        public string Identifier;

        /// <summary>
        /// Type du jeu de mesure de risque
        /// </summary>
        public string HistoricalStressed;
        
        /// <summary>
        /// Aggregation Method
        /// </summary>
        public string AggregationMethod;

        /// <summary>
        /// Niveau de confiance de la mesure de risque
        /// </summary>
        public decimal ConfidenceLevel;

        /// <summary>
        /// Utiliser ou non la robustesse pour la mesure de risque
        /// </summary>
        public bool IsUseRobustness;

        /// <summary>
        /// Facteur d'échelle pour la robustesse
        /// </summary>
        public decimal ScalingFactor;

        /// <summary>
        /// Niveau de confiance utilisé pour le calcul du correlation break
        /// </summary>
        public decimal CorrelationBreakConfidenceLevel;

        /// <summary>
        /// Taille de la sous fenêtre utilisée pour le calcul du correlation break 
        /// </summary>
        public int CorrelationBreakSubWindow;

        /// <summary>
        /// Multiplier pour le calcul de l'ajustement de correlation break
        /// </summary>
        public decimal CorrelationBreakMultiplier;

        /// <summary>
        /// Ajustement de correlation break minimum
        /// </summary>
        public decimal CorrelationBreakMin;

        /// <summary>
        /// Ajustement de correlation break maximum
        /// </summary>
        public decimal CorrelationBreakMax;

        /// <summary>
        /// Market Risk Component
        /// </summary>
        public decimal MarketRiskComponent;

        /// <summary>
        /// Scaled (Weigthed) Market Risk Component 
        /// </summary>
        public decimal ScaledMarketRiskComponent;

        /// <summary>
        /// Application ou non du composant de liquidité à la mesure de risque (issu des parameters)
        /// </summary>
        public bool IsLiquidityComponent;

        /// <summary>
        /// VaR(Value at Risk) selon les spécifications propres au calcul du Liquidity Risk Component
        /// </summary>
        public decimal ValueAtRiskLiquidityComponent;
        
        
        /// <summary>
        /// Niveau de confiance utilisé pour le calcul de l'alpha factor (issu des parameters)
        /// </summary>
        public decimal AlphaConfidenceLevel;

        /// <summary>
        /// Valeur minimum de l'alpha factor (issu des parameters)
        /// </summary>
        public decimal AlphaFloor;

        /// <summary>
        /// Valeur de l'alpha factor 
        /// </summary>
        public decimal AlphaFactor;
        #endregion Members

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Positions partielles relative aux paramètres de risque courant
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données pour construire
        /// les sous-éléments (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètres de risque
        /// (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant partiel de risque en rapport au paramètre de risque courant
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Définir à vrai si le paramètre actuel n'a pas été trouvé dans l'ensemble des paramètres,
        /// mais il a été bati pour stocker un ensemble d'éléments d'actifs en position pour lesquels aucun paramètre n'a été trouvé.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur pour identifier l'événement paramètre manquant
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant un sub sample Prisma <see cref="EfsML.v30.MarginRequirement.PrismaLiquidGroupSplitParameter"/>, 
    /// enfant d'un objet de type PrismaCalcMethCom <see cref="PrismaCalcMethCom"/>, 
    /// utilisé pour construire les sous-éléments du noeud de calcul de risque Prisma (typeof <see cref="EfsML.v30.MarginRequirement.PrismaMarginCalculationMethod"/>)
    /// </summary>
    public sealed class PrismaSubSampleCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Member
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Compression Error
        /// </summary>
        public decimal CompressionError;

        /// <summary>
        /// Value At Risk
        /// </summary>
        public decimal ValueAtRisk;

        /// <summary>
        /// Value At Risk Scaled
        /// </summary>
        public decimal ValueAtRiskScaled;

        /// <summary>
        /// Indique si la Value At Risk (Liquiqidity Component) est spécifié ou non
        /// </summary>
        public Boolean ValueAtRiskLiquidityComponentSpecified;

        /// <summary>
        /// Value At Risk (Liquiqidity Component)
        /// </summary>
        public decimal ValueAtRiskLiquidityComponent;

        /// <summary>
        /// Mean Excess Risk
        /// </summary>
        public decimal MeanExcessRisk;

        /// <summary>
        /// Pure Market Risk
        /// </summary>
        /// PM 20161019 [22174] Prisma 5.0 : Ajout PureMarketRisk
        public decimal PureMarketRisk;

        /// <summary>
        /// Correlation Break Adjustment
        /// </summary>
        public decimal CorrelationBreakAdjustment;

        /// <summary>
        /// Correlation Break Lower Bound
        /// </summary>
        public decimal CbLowerBound;

        /// <summary>
        /// Correlation Break Upper Bound
        /// </summary>
        public decimal CbUpperBound;

        /// <summary>
        /// Compression Adjustment
        /// </summary>
        public decimal CompressionAdjustment;
        
        /// <summary>
        /// Market Risk Component
        /// </summary>
        public decimal MarketRiskComponent;
        #endregion Member

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Positions partielles relative aux paramètres de risque courant
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données pour construire
        /// les sous-éléments (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètres de risque
        /// (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant partiel de risque en rapport au paramètre de risque courant
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Définir à vrai si le paramètre actuel n'a pas été trouvé dans l'ensemble des paramètres,
        /// mais il a été bati pour stocker un ensemble d'éléments d'actifs en position pour lesquels aucun paramètre n'a été trouvé.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur pour identifier l'événement paramètre manquant
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Représente pour un asset, certains résultats intermédaires évalués pendant l'évaluation du LiquidityRisk 
    /// <para></para>
    /// </summary>
    public sealed class PrismaAssetLiquidityRiskCom
    {
        #region Members
        /// <summary>
        /// Trade Unit
        /// </summary>
        public decimal tradeUnit;

        /// <summary>
        ///  net Gross ratio
        /// </summary>
        public decimal netGrossRatio;

        /// <summary>
        /// Liquidity Premium
        /// </summary>
        public decimal liquidityPremium;

        /// <summary>
        /// Liquidity Factor
        /// </summary>
        public decimal liquidityFactor;

        /// <summary>
        /// Risk Measure
        /// </summary>
        public decimal riskMeasure;

        /// <summary>
        /// Liquidity Adjustment
        /// </summary>
        public decimal liquidityAdjustment;

        /// <summary>
        /// Additional Risk Measure
        /// </summary>
        public decimal additionalRiskMeasure;
        #endregion Members
    }
}
