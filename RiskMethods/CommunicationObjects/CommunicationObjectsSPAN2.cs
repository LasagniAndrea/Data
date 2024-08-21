using System;
using System.Collections.Generic;
//
using EFS.ACommon;
using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.RiskMethods;
//
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble de données que doit passer l'objet de calcul de la méthode SPAN 2
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul de la méthode SPAN 2
    /// </summary>
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class SPAN2CalcMethCom : CalcMethComBase, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        // Code Erreur par défaut
        private static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1035);
        #endregion static Members

        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency;

        /// <summary>
        /// Id de l'acteur
        /// </summary>
        public int IdA;

        /// <summary>
        /// Id du book
        /// </summary>
        public int IdB;

        /// <summary>
        /// Type de compte SPAN et donc de méthode à utiliser
        /// </summary>
        public SpanAccountType SpanAccountType = SpanAccountType.Member;

        /// <summary>
        /// Indique si la méthode doit effectuer un calcul de montant de Maintenance
        /// </summary>
        public bool IsMaintenanceAmount = true;

        /// <summary>
        /// EOD / ITD
        /// </summary>
        public string CycleCode;

        /// <summary>
        /// Base URL pour le calcul CME SPAN 2
        /// </summary>
        public string BaseUrl;

        /// <summary>
        /// Id du User URL pour le calcul CME SPAN 2
        /// </summary>
        public string UserId;

        /// <summary>
        ///  Message XML de demande de calcul
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        public List<string> XmlRequestMessage;

        /// <summary>
        /// Message XML reçue en réponse
        /// </summary>
        // PM 20230929 [XXXXX] Changement de type : string => List<string>
        public List<string> XmlResponseMessage;

        /// <summary>
        /// Message Json de demande de calcul
        /// </summary>
        public string JsonRequestMessage;

        /// <summary>
        /// Message Json de réponse détaillée du calcul
        /// </summary>
        public string JsonResponseMessage;

        /// <summary>
        /// Message d'erreur
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// Timing: Intraday,EndOfDay
        /// </summary>
        public SettlSessIDEnum Timing;

        /// <summary>
        /// Compteurs d'éléments
        /// </summary>
        public MarginCounterCom CounterInfo = new MarginCounterCom();

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public Span2TotalAmountCom RequierementAmounts;

        /// <summary>
        /// Type de devise pour le résultat du calcul de déposit
        /// </summary>
        // PM 20231030 [26547][WI735] Ajout MarginCurrencyTypeEnum
        public Cst.InitialMarginCurrencyTypeEnum MarginCurrencyTypeEnum;

        /// <summary>
        /// Indicateur d'essai d'écarter les positions sur des assets erronés
        /// </summary>
        // PM 20230209 [XXXXX] Ajout
        public bool IsTryExcludeWrongPosition { get; set; }

        /// <summary>
        /// Positions exclues du calcul car sur des asset erronés
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> DiscartedPositions { get; set; }

        /// <summary>
        /// Positions considérées dans le calcul
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> ConsideredPositions { get; set; }

        /// <summary>
        /// Liste d'erreurs
        /// </summary>
        // PM 20230830 [26470] Ajout
        public List<(SysMsgCode, List<LogParam>)> ErrorList { get; set; }

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Mis à vrai lorsqu'une erreur s'est produite.
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// Code d'erreur
        /// </summary>
        public SysMsgCode ErrorCode
        {
            get { return m_SysMsgCode; }
            set { m_SysMsgCode = value; }
        }
        #endregion
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public SPAN2CalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Classe de compteurs
    /// </summary>
    public sealed class MarginCounterCom
    {
        #region Members
        private long m_NbAssetParameters;
        private long m_NbInitialPosition;
        private long m_NbNettedPosition;
        private long m_NbReducedPosition;
        private long m_NbActivePosition;
        private long m_NbDiscartedPosition;
        private long m_NbConsideredPosition;
        private long m_NbSpanRiskPosition;
        private long m_NbProcessedPosition;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Nombre de paramètres d'asset
        /// </summary>
        public long NbAssetParameters
        {
            get { return m_NbAssetParameters; }
            set { m_NbAssetParameters = value; }
        }

        /// <summary>
        /// Nombre de positions initiales
        /// </summary>
        public long NbInitialPosition
        {
            get { return m_NbInitialPosition; }
            set { m_NbInitialPosition = value; }
        }

        /// <summary>
        /// Nombre de positions nettes
        /// </summary>
        public long NbNettedPosition
        {
            get { return m_NbNettedPosition; }
            set { m_NbNettedPosition = value; }
        }

        /// <summary>
        /// Nombre de positions réduites
        /// </summary>
        public long NbReducedPosition
        {
            get { return m_NbReducedPosition; }
            set { m_NbReducedPosition = value; }
        }

        /// <summary>
        /// Nombre de positions non échues
        /// </summary>
        public long NbActivePosition
        {
            get { return m_NbActivePosition; }
            set { m_NbActivePosition = value; }
        }

        /// <summary>
        /// Nombre de positions écartées du calcul
        /// </summary>
        public long NbDiscartedPosition
        {
            get { return m_NbDiscartedPosition; }
            set { m_NbDiscartedPosition = value; }
        }

        /// <summary>
        /// Nombre de positions considérées dans le calcul
        /// </summary>
        public long NbConsideredPosition
        {
            get { return m_NbConsideredPosition; }
            set { m_NbConsideredPosition = value; }
        }

        /// <summary>
        /// Compteur de position du message de calcul
        /// </summary>
        public long NbSpanRiskPosition
        {
            get { return m_NbSpanRiskPosition; }
            set { m_NbSpanRiskPosition = value; }
        }

        /// <summary>
        /// Compteur de position réellement évaluée
        /// </summary>
        public long NbProcessedPosition
        {
            get { return m_NbProcessedPosition; }
            set { m_NbProcessedPosition = value; }
        }
        #endregion Accessors
    }

    /// <summary>
    /// Objet de communication identifiant les montants globaux calculés par SPAN 2, 
    /// enfant d'un objet de type SPAN 2 calculation method <see cref="SPAN2CalcMethCom"/>, 
    /// utilisé pour construire les sous noeuds du noeud SPAN 2 margin calculation (typeof <see cref="EfsML.v30.MarginRequirement.Span2MarginCalculationMethod"/>)
    /// </summary>
    public sealed class Span2TotalAmountCom
    {
        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney NetOptionValue { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney RiskMaintenanceAmount { get; set; }

        /// <summary>
        /// Total Initial Margin Amount
        /// </summary>
        public IMoney TotalInitialMarginAmount { get; set; }

        /// <summary>
        /// Total Maintenance Margin Amount
        /// </summary>
        public IMoney TotalMaintenanceMarginAmount { get; set; }
    }

    /// <summary>
    /// Objet de communication identifiant une CCP SPAN 2 (Ex Exchange Complex SPAN) <see cref=" EfsML.v30.MarginRequirement.Span2CCPParameter"/>, 
    /// enfant d'un objet de type SPAN 2 calculation method <see cref="SPAN2CalcMethCom"/>, 
    /// utilisé pour construire les sous noeuds du noeud SPAN 2 margin calculation (typeof <see cref="EfsML.v30.MarginRequirement.Span2MarginCalculationMethod"/>)
    /// </summary>
    public sealed class Span2CCPCom : IRiskParameterCommunicationObject
    {
        #region Accessors
        /// <summary>
        /// Clearing Organization identifier
        /// </summary>
        public string ClearingOrganization { get; set; }

        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney[] NetOptionValue { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney[] RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney[] RiskMaintenanceAmount { get; set; }

        /// <summary>
        /// Total Initial Margin Amount
        /// </summary>
        public IMoney[] TotalInitialMarginAmount { get; set; }

        /// <summary>
        /// Total Maintenance Margin Amount
        /// </summary>
        public IMoney[] TotalMaintenanceMarginAmount { get; set; }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Position partielle relative aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données permettant de construire
        /// les sous-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètre de risque (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant de risque partiel relatif aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Objet de communication identifiant un POD SPAN 2 (Ex Combined Commodity SPAN) 
    /// enfant d'un objet de type SPAN 2 CCP <see cref="Span2CCPCom"/>, 
    /// utilisé pour construire les sous noeuds du noeud SPAN 2 margin calculation (typeof <see cref="EfsML.v30.MarginRequirement.Span2MarginCalculationMethod"/>)
    /// </summary>
    public sealed class Span2PodCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Contract group identifier
        /// </summary>
        public string ContractGroup { get; set; }

        /// <summary>
        /// Méthode de calcul utilisée pour ce groupe de contrats
        /// </summary>
        public string MarginMethod { get; set; }

        /// <summary>
        /// Long Non Option Value
        /// </summary>
        public IMoney LongNonOptionValue { get; set; }

        /// <summary>
        /// Short Non Option Value
        /// </summary>
        public IMoney ShortNonOptionValue { get; set; }

        /// <summary>
        /// Long Option Value (Equity Style)
        /// </summary>
        public IMoney LongOptionValue { get; set; }

        /// <summary>
        /// Short Option Value (Equity Style)
        /// </summary>
        public IMoney ShortOptionValue { get; set; }

        /// <summary>
        /// Long Option Value (Futures Style)
        /// </summary>
        public IMoney LongOptionFuturesStyleValue { get; set; }

        /// <summary>
        /// Short Option Value (Futures Style)
        /// </summary>
        public IMoney ShortOptionFuturesStyleValue { get; set; }

        /// <summary>
        /// Net Option Value
        /// </summary>
        public IMoney NetOptionValue { get; set; }

        /// <summary>
        /// Short Option Minimum
        /// </summary>
        public IMoney ShortOptionMinimum { get; set; }

        /// <summary>
        /// Scanning Risk Amount
        /// </summary>
        public IMoney ScanRiskAmount { get; set; }

        /// <summary>
        /// Inter Comodity Volatility Credit Amount
        /// </summary>
        public IMoney InterCommodityVolatilityCredit { get; set; }

        /// <summary>
        /// Inter Month Spread Charge Amount
        /// </summary>
        public IMoney IntraSpreadChargeAmount { get; set; }

        /// <summary>
        /// Delivery Month Charge Amount
        /// </summary>
        public IMoney DeliveryMonthChargeAmount { get; set; }

        /// <summary>
        /// Inter Commodity Spread Credit Amount
        /// </summary>
        public IMoney InterCommodityCreditAmount { get; set; }

        /// <summary>
        /// Inter Exchange Spread Credit Amount
        /// </summary>
        public IMoney InterExchangeCreditAmount { get; set; }

        /// <summary>
        /// Full Value Component
        /// </summary>
        public IMoney FullValueComponent { get; set; }

        /// <summary>
        /// Concentration Component
        /// </summary>
        public IMoney ConcentrationComponent { get; set; }

        /// <summary>
        /// Hvar Component
        /// </summary>
        public IMoney HvarComponent { get; set; }

        /// <summary>
        /// Liquidity Component
        /// </summary>
        public IMoney LiquidityComponent { get; set; }

        /// <summary>
        /// Stress Component
        /// </summary>
        public IMoney StressComponent { get; set; }

        /// <summary>
        /// Implied Offset
        /// </summary>
        public IMoney ImpliedOffset { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney RiskMaintenanceAmount { get; set; }

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Position partielle relative aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données permettant de construire
        /// les sous-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètre de risque (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant de risque partiel relatif aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }

    /// <summary>
    /// Objet de communication identifiant un Product Group SPAN 2
    /// enfant d'un objet de type SPAN 2 POD <see cref="Span2PodCom"/>, 
    /// utilisé pour construire les sous noeuds du noeud SPAN 2 margin calculation (typeof <see cref="EfsML.v30.MarginRequirement.Span2MarginCalculationMethod"/>)
    /// </summary>
    public class Span2ProductGroupCom : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Product group identifier
        /// </summary>
        public string ProductGroup { get; set; }

        /// <summary>
        /// Concentration Component
        /// </summary>
        public IMoney ConcentrationComponent { get; set; }

        /// <summary>
        /// Hvar Component
        /// </summary>
        public IMoney HvarComponent { get; set; }

        /// <summary>
        /// Liquidity Component
        /// </summary>
        public IMoney LiquidityComponent { get; set; }

        /// <summary>
        /// Stress Component
        /// </summary>
        public IMoney StressComponent { get; set; }

        /// <summary>
        /// Initial Risk Amount
        /// </summary>
        public IMoney RiskInitialAmount { get; set; }

        /// <summary>
        /// Maintenance Risk Amount
        /// </summary>
        public IMoney RiskMaintenanceAmount { get; set; }

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Position partielle relative aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// Les objets de communication contenant l'ensemble minimal de données permettant de construire
        /// les sous-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// d'un noeud de paramètre de risque (<see cref="EfsML.Interface.IRiskParameter"/> et <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// Montant de risque partiel relatif aux paramètres de risque courant
        /// <remarks>Non utilisé</remarks>
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
    }
}
