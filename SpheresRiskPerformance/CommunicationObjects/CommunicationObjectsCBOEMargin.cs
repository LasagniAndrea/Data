using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.RiskMethods;
using FpML.Interface;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode CBOE Margin 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.CBOEMarginMethod"/>)
    /// à l'objet référentiel de la feuille de calcul
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
    /// du calcul par la méthode CBOE Margin (<see cref="EfsML.v30.MarginRequirement.CboeMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class CboeMarginCalcMethCom : CalcMethComBase
    {
        #region Accessors
        /// <summary>
        /// Maintenance / Initial indicateur
        /// </summary>
        // PM 20191025 [24983] Ajout
        public bool IsMaintenanceAmount { get; set; }

        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency
        {
            get;
            set;
        }
        #endregion Accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public CboeMarginCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant les paramètres d'un contrat<see cref="EfsML.v30.MarginRequirement.CboeMarginContractParameter"/>, 
    /// enfant d'un objet de type  <see cref="CboeMarginCalcMethCom"/>, 
    /// utilisé pour construire les sous-éléments du noeuds de la méthode de calcul CBOE Margin (typeof <see cref="EfsML.v30.MarginRequirement.CboeMarginCalculationMethod"/>)
    /// </summary>
    public sealed class CboeContractMarginCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Contract Information
        /// </summary>
        internal CboeContractParameter Contract { get; set; }

        /// <summary>
        /// Underlying Asset Id
        /// </summary>
        public long UnderlyingAssetId { get; set; }

        /// <summary>
        /// Underlying Asset Category Enum
        /// </summary>
        public Cst.UnderlyingAsset UnderlyingAssetCategoryEnum { get; set; }

        /// <summary>
        /// Underlying Asset Price
        /// </summary>
        public decimal UnderlyingQuote { get; set; }

        /// <summary>
        /// Liste des stratégies calculées pour la position sur le contrat
        /// </summary>
        public List<CboeStrategyMarginCom> StrategyMarginList { get; set; }

        /// <summary>
        /// Montant de risque provenant des stratégies
        /// </summary>
        // PM 20191025 [24983] Rename StrategyMarginAmount to StrategyMarginAmountInit
        public IMoney StrategyMarginAmountInit { get; set; }

        /// <summary>
        /// Montant de risque de maintenance provenant des stratégies
        /// </summary>
        // PM 20191025 [24983] Ajout StrategyMarginAmountMaint
        public IMoney StrategyMarginAmountMaint { get; set; }

        /// <summary>
        /// Montant de risque provenant des positions classiques
        /// </summary>
        // PM 20191025 [24983] Rename NormalMarginAmount to NormalMarginAmountInit
        public IMoney NormalMarginAmountInit { get; set; }

        /// <summary>
        /// Montant de risque de maintenance provenant des positions classiques
        /// </summary>
        // PM 20191025 [24983] Ajout NormalMarginAmountMaint
        public IMoney NormalMarginAmountMaint { get; set; }

        /// <summary>
        /// Message d'erreur lors du chargement des cours
        /// </summary>
        public SystemMSGInfo SystemMsgInfo { get; set; }

        /// <summary>
        /// Set/get the stock coverage elements used to reduce the current positions set, the quantity of each object is the covered quantity 
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<StockCoverageCommunicationObject> StocksCoverage { get; set; }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
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
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant les paramètres d'un asset <see cref="EfsML.v30.MarginRequirement.CboeMarginNormalMarginParameter"/>, 
    /// enfant d'un objet de type  <see cref="CboeContractMarginCom"/>, 
    /// utilisé pour construire les sous-éléments du noeuds de la méthode de calcul CBOE Margin (typeof <see cref="EfsML.v30.MarginRequirement.CboeMarginCalculationMethod"/>)
    /// </summary>
    public sealed class CboeNormalMarginCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        /// <summary>
        /// Message d'erreur lors du chargement des cours
        /// </summary>
        public SystemMSGInfo SystemMsgInfo;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Asset Information
        /// </summary>
        internal CboeAssetExpandedParameter Asset { get; set; }

        /// <summary>
        /// Quantité initiale
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal InitialQuantity { get; set; }

        /// <summary>
        /// Quantité restante
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity { get; set; }

        /// <summary>
        /// Asset Price
        /// </summary>
        public decimal Quote { get; set; }

        /// <summary>
        /// Unit Contract Value
        /// </summary>
        public decimal UnitContractValue { get; set; }

        /// <summary>
        /// Unit Underlying Value
        /// </summary>
        public decimal UnitUnderlyingValue { get; set; }

        /// <summary>
        /// Unit Margin Initial
        /// </summary>
        // PM 20191025 [24983] Rename UnitMargin to UnitMarginInit
        //public decimal UnitMargin { get; set; }
        public decimal UnitMarginInit { get; set; }

        /// <summary>
        /// Unit Margin Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout UnitMarginMaint
        public decimal UnitMarginMaint { get; set; }

        /// <summary>
        /// Unit Minimum margin
        /// </summary>
        public decimal UnitMinimumMargin { get; set; }

        /// <summary>
        /// Valeur du contrat
        /// </summary>
        public IMoney ContractValue { get; set; }
        
        /// <summary>
        /// Minimum margin
        /// </summary>
        public IMoney MinMarginAmount { get; set; }

        /// <summary>
        /// Montant de risque initial
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountInit
        public IMoney MarginAmountInit { get; set; }

        /// <summary>
        /// Montant de risque de maintenance 
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountMaint
        public IMoney MarginAmountMaint { get; set; }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres
        /// <summary>
        /// Partial positions set relative to the current risk parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions { get; set; }

        /// <summary>
        /// the communication objects containing the minimal set of datas to build 
        /// the sub-paragraphes (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// of a margin parameter node
        /// (<see cref="EfsML.Interface.IRiskParameter"/> and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>).
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters { get; set; }

        /// <summary>
        /// partial risk amount relative to the current risk parameter
        /// </summary>
        public IMoney MarginAmount { get; set; }
        #endregion IRiskParameterCommunicationObject Membres
        #region IMissingCommunicationObject Membres
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
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Objet de communication identifiant les calculs de statégies, 
    /// enfant d'un objet de type  <see cref="CboeContractMarginCom"/>, 
    /// utilisé pour construire les sous-éléments du noeuds de la méthode de calcul CBOE Margin (typeof <see cref="EfsML.v30.MarginRequirement.CboeMarginCalculationMethod"/>)
    /// </summary>
    public sealed class CboeStrategyMarginCom
    {
        #region Accessors
        /// <summary>
        /// Asset information
        /// </summary>
        internal CboeAssetExpandedParameter Asset { get; set; }

        /// <summary>
        /// Asset information of combined asset
        /// </summary>
        internal CboeAssetExpandedParameter AssetCombined { get; set; }

        /// <summary>
        /// Quantité restante
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity { get; set; }

        /// <summary>
        /// Quantité restante
        /// </summary>
        // EG 20170127 Qty Long To Decimal
        public decimal QuantityCombined { get; set; }

        /// <summary>
        /// Unit Margin Initial
        /// </summary>
        // PM 20191025 [24983] Rename UnitMargin to UnitMarginInit
        //public decimal UnitMargin { get; set; }
        public decimal UnitMarginInit { get; set; }

        /// <summary>
        /// Unit Margin Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout UnitMarginMaint
        public decimal UnitMarginMaint { get; set; }

        /// <summary>
        /// Contract Value
        /// </summary>
        public IMoney ContractValue { get; set; }

        /// <summary>
        /// Contract Value Combined
        /// </summary>
        public IMoney ContractValueCombined { get; set; }

        /// <summary>
        /// Margin Amount
        /// </summary>
        public IMoney MarginAmount { get; set; }

        /// <summary>
        /// Margin Amount Initial
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountInit
        public IMoney MarginAmountInit { get; set; }

        /// <summary>
        /// Margin Amount Maintenance
        /// </summary>
        // PM 20191025 [24983] Ajout MarginAmountMaint
        public IMoney MarginAmountMaint { get; set; }

        /// <summary>
        /// Type de stratégie calculée
        /// </summary>
        public CboeStrategyTypeEnum StrategyTypeEnum { get; set; }
        #endregion Accessors
    }
}
