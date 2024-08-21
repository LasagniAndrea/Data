using EFS.Common;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using FpML.Interface;
using System;
using System.Collections.Generic;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode MEFFCOM2 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.MEFFCOM2Method"/>)
    /// à l'objet référentiel de la feuille de calcul
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) de sorte à construire le noeud
    /// du calcul par la méthode MEFFCOM2 Margin (<see cref="EfsML.v30.MarginRequirement.MeffMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class MeffCalcMethCom : CalcMethComBase
    {
        #region Accessors
        /// <summary>
        /// Devise pour la chambre de compensation
        /// </summary>
        public string CssCurrency
        {
            get;
            set;
        }

        /// <summary>
        /// All the Inter Commodity Spread done between Margin Class
        /// </summary>
        public MeffInterCommoditySpreadCom[] InterCommoditySpread { get; set; }
        #endregion Accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public MeffCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion
    }

    /// <summary>
    /// Communication object identifying a Meff Marging Class<see cref="EfsML.v30.MarginRequirement.MeffMarginClassParameter"/>, 
    /// child of an object type MeffCalcMethCom <see cref="MeffCalcMethCom"/>, 
    /// used to build the sub chapters of the Meff margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.MeffMarginCalculationMethod"/>)
    /// </summary>
    public sealed class MeffMarginClassCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Margin Class Code
        /// </summary>
        public string MarginClassCode { get; set; }

        /// <summary>
        /// Price Fluctuation Type
        /// </summary>
        public string PriceFluctuationType { get; set; }

        /// <summary>
        /// Price Fluctuation
        /// </summary>
        public decimal PriceFluctuation { get; set; }

        /// <summary>
        /// Number Of Value
        /// </summary>
        public int NumberOfValue { get; set; }

        /// <summary>
        /// Underlying Price
        /// </summary>
        public decimal UnderlyingPrice { get; set; }

        /// <summary>
        /// Worst Case Scenario
        /// </summary>
        public int WorstCaseScenario { get; set; }

        /// <summary>
        /// Class Delta
        /// </summary>
        public decimal ClassDelta { get; set; }

        /// <summary>
        /// Maximum Delta Offset
        /// </summary>
        public decimal MaximumDeltaOffset { get; set; }

        /// <summary>
        /// Delta To Offset
        /// </summary>
        public decimal DeltaToOffset { get; set; }
        
        /// <summary>
        /// Matrice pour le calcul du Net Position Margin Amount
        /// </summary>
        public Dictionary<int, decimal> NetPositionMarginArray { get; set; }

        /// <summary>
        /// Matrice pour les Time Spread
        /// </summary>
        public Dictionary<int, decimal> TimeSpreadMarginArray { get; set; }

        /// <summary>
        /// Matrice de calcul du Commodity Margin
        /// </summary>
        public Dictionary<int, decimal> CommodityMarginArray { get; set; }

        /// <summary>
        /// Net Position Margin Amount
        /// </summary>
        public IMoney NetPositionMarginAmount { get; set; }

        /// <summary>
        /// Time Spread Charge Amount
        /// </summary>
        public IMoney TimeSpreadMarginAmount { get; set; }

        /// <summary>
        /// Commodity Margin Amount
        /// </summary>
        public IMoney CommodityMarginAmount { get; set; }

        /// <summary>
        /// Accumulated loss at close
        /// </summary>
        public decimal AccumulatedLossAtClose { get; set; }
        
        /// <summary>
        /// Margin Class Potential Future Loss
        /// </summary>
        public decimal MarginClassPotentialFutureLoss { get; set; }

        /// <summary>
        /// Delta Potential Future Loss
        /// </summary>
        public decimal DeltaPotentialFutureLoss { get; set; }

        /// <summary>
        /// Inter Commodity Spread Credit Amount
        /// </summary>
        public IMoney InterCommodityCreditAmount { get; set; }

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
        /// partial risk amount relative to the current risk parameter : Not Used
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
    /// Communication object identifying a Meff Marging Maturity<see cref="EfsML.v30.MarginRequirement.MeffMarginMaturityParameter"/>, 
    /// child of an object type MeffCalcMethCom <see cref="MeffCalcMethCom"/>, 
    /// used to build the sub chapters of the Meff margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.MeffMarginCalculationMethod"/>)
    /// </summary>
    public sealed class MeffMarginMaturityCom : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Margin Asset Code
        /// </summary>
        public string MarginAssetCode { get; set; }

        /// <summary>
        /// Maturity Date
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Price
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Matrice pour les Deltas
        /// </summary>
        public Dictionary<int, decimal> DeltaArray { get; set; }

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
        /// partial risk amount relative to the current risk parameter : Not Used
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
    /// Communication object identifying a Meff Inter Commodity Spread <see cref="EfsML.v30.MarginRequirement.MeffInterCommoditySpreadParameter"/>, 
    /// child of an object type MeffCalcMethCom <see cref="MeffCalcMethCom"/>, 
    /// used to build the sub chapters of the Meff margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.MeffMarginCalculationMethod"/>)
    /// </summary>
    public sealed class MeffInterCommoditySpreadCom
    {
        /// <summary>
        /// Spread Priority
        /// </summary>
        public string Priority { get; set; }

        /// <summary>
        /// Discount Type
        /// </summary>
        public string DiscountType { get; set; }

        /// <summary>
        /// Number of realized Spread
        /// </summary>
        public decimal NumberOfSpread { get; set; }

        /// <summary>
        /// Spread Leg Parameters
        /// </summary>
        public MeffInterCommoditySpreadLegCom[] LegParameters { get; set; }
    }

    /// <summary>
    /// Communication object identifying a Meff Inter Commodity Spread Leg <see cref="EfsML.v30.MarginRequirement.MeffInterCommoditySpreadLegParameter"/>, 
    /// child of an object type Meff Inter Commodity Spread <see cref="MeffInterCommoditySpreadCom"/>, 
    /// used to build the sub chapters of the Meff margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.MeffMarginCalculationMethod"/>)
    /// </summary>
    public sealed class MeffInterCommoditySpreadLegCom
    {
        /// <summary>
        /// Leg Margin Class
        /// </summary>
        public MeffMarginClassCom MarginClass { get; set; }

        /// <summary>
        /// Spread Rate
        /// </summary>
        public decimal MarginCredit { get; set; }

        /// <summary>
        /// Delta Per Spread
        /// </summary>
        public decimal DeltaPerSpread { get; set; }

        /// <summary>
        /// Delta Available
        /// </summary>
        public decimal DeltaAvailable { get; set; }

        /// <summary>
        /// Delta Remaining
        /// </summary>
        public decimal DeltaRemaining { get; set; }

        /// <summary>
        /// Delta Consumed
        /// </summary>
        public decimal DeltaConsumed { get; set; }

        /// <summary>
        /// Spread Credit
        /// </summary>
        public decimal SpreadCredit { get; set; }
    }
}
