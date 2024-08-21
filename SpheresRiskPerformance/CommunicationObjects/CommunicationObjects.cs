using System;
using System.Collections.Generic;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
using EFS.SpheresRiskPerformance.Enum;
//
using EfsML.Enum;
//
using FixML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    #region Common
    /// <summary>
    /// </summary>
    public sealed class SpreadPositionCommunicationObject
    {

        /// <summary>
        /// Asset id de la position en spread
        /// </summary>
        public int AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// Initial quantity
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get;

            set;
        }
        
        /// <summary>
        /// Spread quantity
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public decimal SpreadQuantity
        {
            get;

            set;
        }

        /// <summary>
        /// Spread Ratio
        /// </summary>
        public decimal Ratio
        {
            get;

            set;
        }

        /// <summary>
        /// Spread amount
        /// </summary>
        public IMoney Amount
        {
            get;

            set;
        }
    }

    /// <summary>
    /// Communication object describing the minimal set of datas to pass 
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build the Delivery node (<see cref="EfsML.v30.MarginRequirement.DeliveryMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    public class DeliveryMarginCalculationMethodCommunicationObject : IMarginCalculationMethodCommunicationObject
    {
        #region IMarginCalculationMethodCommunicationObject Membres
        /// <summary>
        /// Date des parametres de calcul utilisés
        /// </summary>
        /// PM 20150507 [20575] Ajout DtParameters
        public DateTime DtParameters { set; get; }

        /// <summary>
        /// the communication objects containing the data set (typeof <see cref="DeliveryParameterCommunicationObject"/>)
        /// to build the main chapters (typeof <see cref="EfsML.v30.MarginRequirement.DeliveryParameter"/>) 
        /// of the delivery  node
        /// (typeof <see cref="EfsML.v30.MarginRequirement.DeliveryMarginCalculationMethod"/>)
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        ///  amounts relative to the whole set of the parameters
        /// </summary>
        public IMoney[] MarginAmounts
        {
            get;

            set;
        }

        /// <summary>
        ///  Ne s'applique pas ici (Obtient une liste vide)
        /// </summary>
        /// FI 20160613 [22256] Add
        public IEnumerable<StockCoverageDetailCommunicationObject> UnderlyingStock
        {
            get;
            set;
        }

        /// <summary>
        /// Type de Methode
        /// </summary>
        // PM 20200910 [25481] Ajout type de méthode
        public InitialMarginMethodEnum MarginMethodType { get; set; }

        /// <summary>
        /// Margin Method Name
        /// </summary>
        // PM 20230817 [XXXXX] Ajout MarginMethodName
        public string MarginMethodName
        {
            get { return MarginMethodType.ToString().Replace("_", " "); }
        }

        /// <summary>
        /// Version de la méthode
        /// </summary>
        // PM 20230817 [XXXXX] Ajout MethodVersion
        public decimal MethodVersion { set; get; }

        /// <summary>
        /// Indique si un deposit a été calculé de façon incomplete (ou même pas calculé)
        /// </summary>
        // PM 20220202 Ajout IsIncomplete
        public bool IsIncomplete { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public DeliveryMarginCalculationMethodCommunicationObject()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();    
        }
        #endregion
    }

    /// <summary>
    /// the communication class containing the data set
    /// to build the main chapters (typeof <see cref="EfsML.v30.MarginRequirement.DeliveryParameter"/>) 
    /// of the delivery node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.DeliveryMarginCalculationMethod"/>)
    /// </summary>
    public class DeliveryParameterCommunicationObject : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode1004 = new SysMsgCode(SysCodeEnum.SYS, 1004);
        private readonly static SysMsgCode m_SysMsgCode1005 = new SysMsgCode(SysCodeEnum.SYS, 1005);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Internal id of the asset etd
        /// </summary>
        public int AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// modality used to evaluate the asset positions.
        /// <list type="">
        /// <item>modality FixedAmount, risk amount => quantity * risk value * contract size</item>
        /// <item>modality Percentage, risk amount => quantity * risk value(%) * contract size * "asset quote"</item>
        /// </list>
        /// </summary>
        /// <remarks>"asset quote" is the quote of the underlyer asset at the current business date</remarks>
        public ExpressionType ExpressionType
        {
            get;

            set;
        }

        /// <summary>
        /// Etape de livraison
        /// </summary>
        // PM 20130911 [17949] ajout DeliveryStep
        public InitialMarginDeliveryStepEnum DeliveryStep { get; set; }

        /// <summary>
        /// Valeur d'évaluation du déposit de livraison
        /// </summary>
        public decimal DeliveryValue
        {
            get;
            set;
        }

        /// <summary>
        /// Asset quote
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <item>"asset quote" is for future contracts the quote of the asset at the current business date</item>
        /// <item>"asset quote" is for option contracts the quote of the underlyer asset at the current business date</item>
        /// </list>
        /// </remarks>
        public decimal Quote
        {
            get;

            set;
        }

        /// <summary>
        /// Contract size (using the contract size of the DERIVATIVECONTRACT table)
        /// </summary>
        public decimal Size
        {
            get;

            set;
        }

        /// <summary>
        /// Settlement date
        /// </summary>
        public DateTime? SettlementDate
        {
            get;

            set;

        }

        /// <summary>
        /// Underlyer category
        /// </summary>
        /// <remarks>for Futures contracts, the underlyer category will be forced to Futures</remarks>
        public Cst.UnderlyingAsset? UnderlyerCategory
        {
            get;

            set;
        }

        /// <summary>
        /// Id de l'environnement de marché
        /// </summary>
        public string IdMarketEnv
        {
            get;

            set;
        }
        /// <summary>
        /// Id du scénario
        /// </summary>
        public string IdValScenario
        {
            get;

            set;
        }
        /// <summary>
        /// Adjusted Time
        /// </summary>
        public DateTime? AdjustedTime
        {
            get;

            set;
        }
        /// <summary>
        /// Quote Side
        /// </summary>
        public string QuoteSide
        {
            get;

            set;
        }
        /// <summary>
        /// Quote Timing
        /// </summary>
        public string QuoteTiming
        {
            get;

            set;
        }

        /// <summary>
        /// Complementary information in case of missing quote
        /// </summary>
        public SystemMSGInfo SystemMsgInfo { get; set; }

        /// <summary>
        /// Error code for a missing quote related to an asset ETD
        /// </summary>
        public SysMsgCode ErrorCodeFuture
        {
            
            //get { return "SYS-01005"; }
            get { return m_SysMsgCode1005; }
        }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Partial positions set relative to the current delivery parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// empty collection
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        /// partial risk amount relative to the current delivery parameter
        /// </summary>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion

        #region IMissingCommunicationObject Membres

        /// <summary>
        /// Quote is missing
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

        /// <summary>
        /// Error code for a missing quote
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01004"; }
            get { return m_SysMsgCode1004; }
        }

        #endregion
    }

    /// <summary>
    /// Class representing a risk margin part (premium, additional, spread, etc..) as defined for the TIMS IDEM/EUREX method
    /// </summary>
    public class TimsDecomposableParameterCommunicationObject
    {
        /// <summary>
        /// components consituting the decomposable risk parameter
        /// </summary>
        public IEnumerable<TimsFactorCommunicationObject> Factors
        {
            get;

            set;
        }

        /// <summary>
        /// Total amount of the decomposable risk parameter
        /// </summary>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        /// <summary>
        /// Optional starting currency, not null when the amount is resulting by an exchange rate conversion from this currency
        /// to the amount currency
        /// </summary>
        public string CurrencyFrom
        { 
            get; 
            
            set; 
        }
        
    }

    /// <summary>
    /// Class representing a factor of TimsDecomposableParameterCommunicationObject (for the TIMS IDEM/EUREX method)
    /// </summary>
    public class TimsFactorCommunicationObject
    {
        /// <summary>
        /// Factor identifier, can be null, used in alternance with the asset internal id, when AssetId is null, 
        /// the Identifier is used to identify the factor.
        /// When Identifier is not null then the factor object is related to a group of positions.
        /// </summary>
        /// <remarks>the {Identifier, AssetId} pair must be unique for any factors list, at least one of the pair elementst must be null
        /// </remarks>
        public string Identifier
        {
            get;

            set;
        }

        /// <summary>
        /// Asset internal id of , can be null. Used in alternance with the Identifier field, when Identifier is null, 
        /// the asset id is used to identify the factor. When asset Id is not null then the factor object is related to a net by asset position.
        /// </summary>
        /// <remarks>the {Identifier, AssetId} pair must be unique for any factors list, at least one of the pair elements must be null
        /// </remarks>
        public int? AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// Position type, identify the type of the current Quantity. Usually null when AssetID is null.
        /// </summary>
        public PosType? PosType
        {
            get;

            set;
        }

        /// <summary>
        /// Minimum rate
        /// </summary>
        public decimal? MinimumRate
        {
            get;

            set;
        }

        /// <summary>
        /// Strike price of the asset, it MAY be not null when the factor is related to a specific option asset and the related
        /// quantity has been exercised/assigned/delivered
        /// </summary>
        public decimal? StrikePrice
        {
            get;

            set;
        }

        /// <summary>
        /// Quote used to calculate the amount, can be the current closing asset etd quote, or the closing underlying asset quote, 
        /// or a margin parameter, depending on the factor PosType or the specific risk margin where the factor has been used in
        /// </summary>
        public decimal? Quote
        {
            get;

            set;
        }

        /// <summary>
        /// Quantity related to the actual factor, null when the factor is aggregated and then not related to a specific asset
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public decimal? Quantity
        {
            get;

            set;
        }

        /// <summary>
        /// short options comepnsated quantity,  
        /// not null where the short option compensation (cross margining for a margin class) has been used
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64)  
        // EG 20170127 Qty Long To Decimal
        public decimal? CompensatedQuantity
        { 
            get;

            set;
        }

        /// <summary>
        /// Multiplier related to the actual asset ETD, null when the factor is aggregated and then not related to a specific asset
        /// </summary>
        public decimal? Multiplier
        {
            get;

            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public decimal? DvpAmount
        {
            get;

            set;
        }

        /// <summary>
        /// Delivery date, not null when the factor is related to a specific asset and the related quantity has been assigned/exercised or
        /// it is in delivering
        /// </summary>
        public DateTime? DeliveryDate
        {
            get;

            set;
        }

        /// <summary>
        /// Risk array, not null when the factor to an additional margin evaluation (either for a specific asset either for an aggregated factor).
        /// This amounts collection is the potential additional liquidation amount. This amount is necessary when a portfolio of positions 
        /// in a margin class or a margin group (if applicable) must be liquidated, 
        /// not at the immediate liquidation costs / proceeds based on the day's settlement prices, 
        /// but at the liquidation costs / proceeds resulting from the substitution of an assumed future unfavorable “worst-case” price.
        /// </summary>
        public TimsFactorCommunicationObject[] RiskArray
        {
            get;


            set;

        }

        /// <summary>
        /// Risk amount for the factor, must be NOT null
        /// </summary>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        /// <summary>
        /// Indique s'il s'agit du Spot Month
        /// </summary>
        public bool? SpotMonth
        {
            get;

            set;
        }

        /// <summary>
        /// element index inside the risk array vector
        /// </summary>
        /// <remarks>actually used for the TIMS EUREX method only</remarks>
        public int? RiskArrayIndex
        {
            get;

            set;
        }

        /// <summary>
        /// used just for short call element only, 
        /// when true the standard risk margin value (inside <seealso cref="TimsFactorCommunicationObject.Quote"/>) 
        /// has been replaced by the short option adjustement value
        /// </summary>
        /// <remarks>actually used for the TIMS EUREX method only</remarks>
        public bool? ShortAdj
        {
            get;

            set;
        }

        /// <summary>
        /// Maturity value, given in numeric format (AAAAMM, AAMM). 
        /// </summary>
        public decimal? MaturityYearMonth
        {
            get;

            set;
        }

        /// <summary>
        /// Maturity factor, it is not null when MaturityYearMonth is not null
        /// </summary>
        public decimal? MaturityFactor
        {
            get;

            set;
        }

        /// <summary>
        /// Indicator whether the projected underlying price is lesser, equals or greater than the closing price
        /// </summary>
        /// <value>D -> downside ; N -> neutral ; U -> Upside</value>
        public string QuoteUnlVsQuote_Indicator
        {
            get;

            set;
        }

        /// <summary>
        /// volatility  indicator 
        /// </summary>
        /// <value>D -> downside ; N -> neutral ; U -> Upside</value>
        public string Volatility_Indicator
        {
            get;

            set;
        }
    }
    #endregion Common

    #region CustomMethod

    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the CUSTOM calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.CustomMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a CUSTOM margin calculation node (<see cref="EfsML.v30.MarginRequirement.CustomMarginCalculationMethod"/> 
    /// and <see cref="EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces.IMarginCalculationMethodCommunicationObject"/>)
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public class CustomMarginCalculationMethodCommunicationObject : CalcMethComBase
    {
        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public CustomMarginCalculationMethodCommunicationObject()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// the communication class containing the data set
    /// to build the main chapters (typeof <see cref="EfsML.v30.MarginRequirement.CustomContractParameter"/>) 
    /// of the CUSTOM margin calculation node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.CustomMarginCalculationMethod"/>)
    /// </summary>
    public class CustomContractParameterCommunicationObject : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1002);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Internal id of the derivative contract
        /// </summary>
        public int ContractId
        {
            get;

            set;
        }

        /// <summary>
        /// identifier of the derivative contract
        /// </summary>
        public string Identifier
        {
            get;

            set;
        }

        /// <summary>
        /// sub-modality of the standard method used to evaluate the contract positions.
        /// <list type="">
        /// <item>modality FixedAmount, risk amount => quantity * risk value</item>
        /// <item>modality Percentage, risk amount => quantity * risk value(%) * contract multiplier * "asset quote"</item>
        /// </list>
        /// </summary>
        /// <remarks>"asset quote" is for future contracts the quote of the asset at the current business date,  
        /// "asset quote" is for option contracts the quote of the underlyer asset at the current business date</remarks>
        public ExpressionType ExpressionType
        {
            get;

            set;
        }

        /// <summary>
        /// Asset quote
        /// </summary>
        /// <remarks>
        /// <list type="">
        /// <item>"asset quote" is for future contracts the quote of the asset at the current business date</item>
        /// <item>"asset quote" is for option contracts the quote of the underlyer asset at the current business date</item>
        /// </list>
        /// </remarks>
        public decimal Quote
        {
            get;

            set;
        }

        /// <summary>
        /// Contract multiplier (using the contract multiplier of the DERIVATIVECONTRACT table)
        /// </summary>
        public decimal Multiplier
        {
            get;

            set;
        }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Partial positions set relative to the current CUSTOM contract parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// the communication objects containing the data set to build 
        /// the sub-paragraphes (<see cref="CustomAmountParameterCommunicationObject"/> 
        /// and <see cref="EfsML.v30.MarginRequirement.RiskParameter"/>) 
        /// for the current CUSTOM contract parameter
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        /// partial risk amount relative to the current CUSTOM contract parameter
        /// </summary>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion

        #region IMissingCommunicationObject Membres

        /// <summary>
        /// The current CUSTOM parameter does not exist in the Spheres environement and it has been built on the ly using defualt values
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

        /// <summary>
        /// Erro code for missing custom parameter
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01002"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// the communication class containing the data set
    /// to build the paragraphe (typeof <see cref="EfsML.v30.MarginRequirement.CustomContractParameter"/>) 
    /// of the CUSTOM contract parameter node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.CustomAmountParameter"/>)
    /// </summary>
    public class CustomAmountParameterCommunicationObject : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// risk value that times the quantity in position
        /// </summary>
        public decimal RiskValue
        {
            get;

            set;
        }

        /// <summary>
        /// CUSTOM amount parameter type
        /// <list type="">
        /// <listheader>CUSTOM amount parameter type</listheader>
        /// <item>Normal</item>
        /// <item>Straddle</item>
        /// <item>LongCall</item>
        /// <item>LongPut</item>
        /// <item>ShortCall</item>
        /// <item>ShortPut</item>
        /// </list>
        /// </summary>
        public string Type
        {
            get;

            set;
        }

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Partial positions set relative to the current CUSTOM amount parameter
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// empty collection
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        /// partial risk amount relative to the current CUSTOM amount parameter
        /// </summary>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion
    }

    #endregion CustomMethod

    #region TimsIdemMethod

    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the TIMS calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.TimsIdemMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a TIMS margin calculation node (<see cref="EfsML.v30.MarginRequirement.TimsIdemMarginCalculationMethod"/> 
    /// and <see cref="EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces.IMarginCalculationMethodCommunicationObject"/>)
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class TimsIdemMarginCalculationMethodCommunicationObject : CalcMethComBase
    {
        // UNDONE MF 20111110 l'activation du cross margin peut être différent par marché, mettre à disposition une structure..
        //  ...qui puisse comprendre une liste d'activations
        /// <summary>
        /// get/set the cross margin activation status, 
        /// when at least one of the markets has the cross margin activated the property value is true
        /// </summary>
        public bool CrossMarginActivated { get; set; }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public TimsIdemMarginCalculationMethodCommunicationObject()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion
    }

    /// <summary>
    /// communication object identifying a TIMS IDEM product group <see cref="EfsML.v30.MarginRequirement.TimsIdemProductParameter"/>, 
    /// child of an obj type tims calculation method <see cref="TimsIdemMarginCalculationMethodCommunicationObject"/>, 
    /// used to build the sub chapters of the TIMS margin calculation node  (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemMarginCalculationMethod"/>)
    /// </summary>
    public sealed class TimsIdemProductParameterCommunicationObject : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Product group identifier
        /// </summary>
        public string Product
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Spread margin amount for the current product group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Spread
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Mtm margin amount for the current product group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Mtm
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Premium margin amount for the current product group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Premium
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Additional margin amount for the current product group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Additional
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Minimum margin amount for the current product group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Minimum
        {
            get;

            set;
        }

        /// <summary>
        /// Contains all the spread positions linked to the current group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> SpreadPositions
        {
            get;

            set;
        }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Contains all the positions linked to the current group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// communication objects containing objects typeof <see cref="TimsIdemClassParameterCommunicationObject"/>
        /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemClassParameter"/>) 
        /// of the product node
        /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemProductParameter"/>)
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the total margin amount for the current product group
        /// </summary>
        /// <remarks>may NOT be null</remarks>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion

        #region IMissingCommunicationObject Membres

        /// <summary>
        /// Set to true when the current parameter has not been found in the TIMS IDEM parameters set, 
        /// but it has been built to stock one set of asset elements in position and no TIMS parameters have been found for them.
        /// The group identifier will be set using the contract symbol of the assets. 
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

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
    /// Class communication objects defining objects typeof <see cref="TimsIdemClassParameterCommunicationObject"/>
    /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemClassParameter"/>) 
    /// of the father product node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemProductParameter"/>)
    /// </summary>
    public sealed class TimsIdemClassParameterCommunicationObject : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Class identifier
        /// </summary>
        public string Class
        {
            get;

            set;
        }


        /// <summary>
        /// Contract symbols list
        /// </summary>
        /// <remarks>a "class" group can contain one or more ETD contrats</remarks>
        public IEnumerable<string> ContractSymbols
        {
            get;

            set;
        }

        /// <summary>
        /// "Class file" contract multipliers list
        /// </summary>
        public IEnumerable<decimal> ClassFileContractMultipliers
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Spread margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Spread
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Mtm margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Mtm
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Premium margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Premium
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Additional margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Additional
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Minimum margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Minimum
        {
            get;

            set;
        }

        /// <summary>
        /// Contains all the positions for spread linked to the class group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> SpreadPositions
        {
            get;

            set;
        }

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Contains all the positions linked to the class group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// communication objects containing objects typeof <see cref="TimsIdemContractParameterCommunicationObject"/>
        /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemContractParameter"/>) 
        /// of the class node
        /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemClassParameter"/>)
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the total margin amount for the current class group
        /// </summary>
        /// <remarks>may NOT be null</remarks>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion
    }

    /// <summary>
    /// communication objects idenrtifying objects typeof <see cref="TimsIdemContractParameterCommunicationObject"/>
    /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemContractParameter"/>) 
    /// of the father class node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsIdemClassParameter"/>)
    /// </summary>
    public sealed class TimsIdemContractParameterCommunicationObject : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Contract parameter identifier (a ETD contract symbol) 
        /// </summary>
        public string Contract
        {
            get;

            set;
        }

        /// <summary>
        /// Contract offset percentage, it is the weigth of he contract in comparison with the other contracts inside the same class.
        /// the total amount of this contract it is reduced timing that with this offset before passing it to compute the total for the
        /// whole class.
        /// </summary>
        /// <value>from 0 (0%) to 1(100%)</value>
        public decimal Offset
        {
            get;

            set;
        }

        /// <summary>
        /// Contract display name
        /// </summary>
        public string Description
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Spread margin amount for the current class group
        /// </summary>
        /// <remarks>can be null</remarks>
        //PM 20141113 [20491] Ajout du calcul des spreads au niveau Contract
        public TimsDecomposableParameterCommunicationObject Spread
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Mtm margin amount for the current contract
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Mtm
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Premium margin amount for the current contract
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Premium
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Additional margin amount for the current contract
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Additional
        { get; set; }

        /// <summary>
        /// Montant d'Additional margin tenant compte de l'offset pour le contract courant
        /// </summary>
        /// <remarks>Peut être null</remarks>
        /// PM 20170516 [23118][23157] Ajout AdditionalWithOffset
        public TimsDecomposableParameterCommunicationObject AdditionalWithOffset
        { get; set; }

        /// <summary>
        /// Set/get the stock coverage elements used to reduce the current positions set, the quantity of each object is the covered quantity 
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<StockCoverageCommunicationObject> StocksCoverage
        {
            get;

            set;
        }

        /// <summary>
        /// Contains all the positions for spread linked to the class group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> SpreadPositions
        {
            get;

            set;
        }

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Contains all the positions linked to the contract
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// empty list, null
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get { return null; }

            set { }
        }

        /// <summary>
        /// Set/get the total margin amount for the current contract
        /// </summary>
        /// <remarks>may NOT be null</remarks>
        public IMoney MarginAmount
        {
            get;

            set;
        }

        #endregion
    }


    #endregion TimsIdemMethod

    #region TimsEurexMethod

    /// <summary>
    /// Communication object describing the minimal set of datas to pass from the TIMS EUREX calculation method object 
    /// (<see cref="EFS.SpheresRiskPerformance.RiskMethods.TimsEUREXMethod"/>)
    /// to the calculation sheet repository object
    /// (<see cref="EFS.SpheresRiskPerformance.CalculationSheet.CalculationSheetRepository"/>) in order to
    /// build a TIMS EUREX margin calculation node (<see cref="EfsML.v30.MarginRequirement.TimsEurexMarginCalculationMethod"/> 
    /// </summary>
    /// FI 20160613 [22256] Modify
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class TimsEurexMarginCalculationMethodCommunicationObject : CalcMethComBase
    {
        /// <summary>
        /// Clearing house currency
        /// </summary>
        public string CssCurrency
        {
            get;
            set;
        }

        /// <summary>
        /// Set/ get the needed rate exchange to convert the multy-currency amount to the clearing house specific currency
        /// </summary>
        public IEnumerable<TimsEurexExchRateCommunicationObject> ExchRates
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the cross margin amount for the current actor/book
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<TimsDecomposableParameterCommunicationObject> Cross
        {
            get;

            set;
        }

        /// <summary>
        /// Set/ get the NOT crossed amounts list, not null when the cross margin is executed
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<IMoney> NotCrossedMarginAmounts
        {
            get;

            set;
        }

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        public TimsEurexMarginCalculationMethodCommunicationObject()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion
    }

    /// <summary>
    /// Margin group communication object, containing the data set 
    /// to build the main chapters (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexGroupParameter"/>) 
    /// of the TIMS EUREX margin calculation node
    /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexMarginCalculationMethod"/>)
    /// </summary>
    public class TimsEurexGroupParameterCommunicationObject : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Margin Group identifier
        /// </summary>
        public string Group
        {
            get;
            set;
        }

        /// <summary>
        /// A margin group may consist of margin classes based on different currencies.
        /// </summary>
        /// <remarks>may NOT be null or empty</remarks>
        public IMoney[] MarginAmounts
        {
            get;
            set;
        }

        /// <summary>
        /// “Out-of-the-money” minimum is part of the calculation of the short option adjustment 
        /// for the determination of margin requirements in short option positions.
        /// </summary>
        public decimal OutOfTheMoneyMinValue
        {
            get;
            set;
        }

        /// <summary>
        /// Group offset percentage, it is the minimum weigth among the class offsets .
        /// </summary>
        /// <value>from 0 (0%) to 1(100%)</value>
        public decimal Offset
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the multi-currency Premium margin amounts for the current group
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<TimsDecomposableParameterCommunicationObject> Premiums
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the multi-currency Spread margin amounts for the current group
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<TimsDecomposableParameterCommunicationObject> Spreads
        {
            get;

            set;
        }
        
        /// <summary>
        /// Set/get the multi-devise Addtional margin amounts for the current group
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<TimsDecomposableParameterCommunicationObject> Additionals
        {
            get;

            set;
        }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Positions set related to this margin class
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;
            set;
        }

        /// <summary>
        /// communication objects containing margin class objects (typeof <see cref="TimsEurexClassParameterCommunicationObject"/>)
        /// to build the margin class sub-chapters (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexClassParameter"/>) 
        /// for the current margin group
        /// (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexGroupParameter"/>)
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;
            set;
        }

        /// <summary>
        /// Get Set first margin group amount. Not used.
        /// </summary>
        [Obsolete(@"Use the IMoney collection MarginAmounts, 
                    because a margin group may consist of margin classes based on different currencies", true)]
        public IMoney MarginAmount
        {
            get { return MarginAmounts[0]; }
            set { MarginAmounts[0] = value; }
        }

        #endregion

        #region IMissingCommunicationObject Membres

        /// <summary>
        /// Set to true when the current parameter has not been found in the EUREX parameters set, 
        /// and it has been built to stock all the positions for which no parameters have been found.
        /// We build a single missing parameter identified by the name "N/A", 
        /// then all the series without parameters will be placed inside of one parameter.
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

        /// <summary>
        /// We will produce a warning for each built missing parameter
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Margin class communication objects used
    /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexClassParameter"/>) 
    /// for a margin group (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexGroupParameter"/>)
    /// </summary>
    public sealed class TimsEurexClassParameterCommunicationObject : IRiskParameterCommunicationObject
    {
        /// <summary>
        /// Margin Class identifier
        /// </summary>
        public string Class
        {
            get;
            set;
        }

        /// <summary>
        /// Class offset percentage, it is the minimum weigth among the class contracts .
        /// it is the weigth of the class in comparison with the other classes inside the same group.
        /// The total credit additional margin of this class it is reduced, timing that with the min offset 
        /// before passing it to compute the total for the whole group.
        /// </summary>
        /// <value>from 0 (0%) to 1(100%)</value>
        public decimal Offset
        {
            get;

            set;
        }

        /// <summary>
        /// Used to adjust the max additional amount (worst evaluated scenario) 
        /// when the maturity switch is activated for the current margin class
        /// </summary>
        public decimal MaturityFactor
        {
            get;

            set;
        }

        /// <summary>
        /// amounts to be paid for futures spread positions of the spot month for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        public decimal BackMonthSpreadRate
        {
            get;
            set;
        }

        /// <summary>
        /// amounts to be paid for futures spread positions of the back months for all spread types
        /// (e.g. Mar/Jun, Jun/Sep or Mar/Sep). These amounts are used to determine the futures spread margin.
        /// </summary>
        public decimal SpotMonthSpreadRate
        {
            get;
            set;
        }

        /// <summary>
        /// Contract symbols list
        /// </summary>
        /// <remarks>a "class" group can contain one or more ETD contrats</remarks>
        public IEnumerable<string> ContractSymbols
        {
            get;
            set;
        }

        /// <summary>
        /// Set/get the Premium margin amount for the current margin class
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Premium
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Liquidating margin amount for the current margin class
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Liquidating
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Spread margin amount for the current margin class 
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Spread
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the additional margin amount for the current margin class 
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Additional
        {
            get;

            set;
        }

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Contains all the positions linked to the class group (can contain the positions for multiple derivative contrats)
        /// </summary>
        public IEnumerable<EFS.Common.Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;
            set;
        }

        /// <summary>
        /// Contracts of the current class
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get;
            set;
        }

        /// <summary>
        /// Set/get the total margin amount for the current class group. 
        /// All products in a margin class must have the same product currency.
        /// </summary>
        /// <remarks>may NOT be null</remarks>
        public IMoney MarginAmount
        {
            get;
            set;
        }

        #endregion
    }

    /// <summary>
    /// (Margin) contract communication objects used
    /// to build the paragraphes (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexContractParameter"/>) 
    /// for a margin class (typeof <see cref="EfsML.v30.MarginRequirement.TimsEurexClassParameter"/>)
    /// </summary>
    public sealed class TimsEurexContractParameterCommunicationObject : IRiskParameterCommunicationObject, IMissingCommunicationObject
    {
        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1001);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Contract parameter identifier (ETD contract symbol) 
        /// </summary>
        public string Contract
        {
            get;

            set;
        }

        /// <summary>
        /// Contract offsets percentage.
        /// </summary>
        /// <value>from 0 (0%) to 1(100%)</value>
        public IEnumerable<decimal> Offsets
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Premium margin amount for the current contract
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Premium
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Liquidating margin amount for the current contract
        /// </summary>
        public TimsDecomposableParameterCommunicationObject Liquidating
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the Additional margin amount for the current contract
        /// </summary>
        /// <remarks>can be null</remarks>
        public TimsDecomposableParameterCommunicationObject Additional
        {
            get;

            set;
        }

        /// <summary>
        /// Set/get the stock coverage elements used to reduce the current positions set, the quantity of each object is the covered quantity 
        /// </summary>
        /// <remarks>can be null</remarks>
        public IEnumerable<StockCoverageCommunicationObject> StocksCoverage
        {
            get;

            set;
        }

        /// <summary>
        /// short options quantities where the short option compensation (cross margining for a margin class) has been used
        /// </summary>
        public IEnumerable<ShortOptionCompensationCommunicationObject> CompensatedShortOptionQuantities
        {
            get;

            set;
        }
        #endregion Accessors

        #region IRiskParameterCommunicationObject Membres

        /// <summary>
        /// Contains all the positions linked to the contract
        /// </summary>
        public IEnumerable<Pair<PosRiskMarginKey, RiskMarginPosition>> Positions
        {
            get;

            set;
        }

        /// <summary>
        /// empty list, contract element does not have sub-parameters
        /// </summary>
        public IRiskParameterCommunicationObject[] Parameters
        {
            get { return null; }

            set { }
        }

        /// <summary>
        /// Set/get the total margin amount for the current contract
        /// </summary>
        /// <remarks>may NOT be null</remarks>
        public IMoney MarginAmount
        {
            get;
            set;
        }

        #endregion

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// Set to true when the current parameter has not been found in the EUREX parameters set, 
        /// and it has been built to stock all the positions for which no parameters have been found.
        /// We build a single missing parameter identified by the name "N/A", 
        /// then all the series without parameters will be placed inside of one parameter.
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

        /// <summary>
        /// We will produce a warning for each built missing parameter
        /// </summary>
        public SysMsgCode ErrorCode
        {
            
            //get { return "SYS-01001"; }
            get { return m_SysMsgCode; }
        }
        #endregion
    }

    /// <summary>
    /// Communication object containing all the used exchange rate in order to convert multi-currency amounts inside of
    /// an eurex group communication object (<seealso cref="TimsEurexGroupParameterCommunicationObject"/>) from the 
    /// original currency to the target currency
    /// </summary>
    public sealed class TimsEurexExchRateCommunicationObject : IMissingCommunicationObject
    {
        #region Enum
        /// <summary>
        /// Type of the possible errors related to this communication object
        /// </summary>
        public enum ErrorTypeList
        {
            /// <summary>
            /// default error related to a missing quote
            /// </summary>
            Default = 0,
            /// <summary>
            /// error related to a disabled fx asset
            /// </summary>
            FxAssetDisabled = 1,
        };
        #endregion Enum

        #region static Members
        
        private readonly static SysMsgCode m_SysMsgCode1012 = new SysMsgCode(SysCodeEnum.SYS, 1012);
        private readonly static SysMsgCode m_SysMsgCode1013 = new SysMsgCode(SysCodeEnum.SYS, 1013);
        #endregion static Members

        #region Accessors
        /// <summary>
        /// Get/Set original currency
        /// </summary>
        public string CurrencyFrom
        {
            get;

            set;
        }

        /// <summary>
        /// Get/Set target currency
        /// </summary>
        public string CurrencyTo
        {
            get;

            set;
        }

        /// <summary>
        /// Get/Set the exchange rate for margin (debit) amount
        /// </summary>
        public decimal RateDebit
        {
            get;

            set;
        }

        /// <summary>
        /// Get/Set the exchange rate for credit amount
        /// </summary>
        public decimal RateCredit
        {
            get;

            set;
        }

        /// <summary>
        /// Complementary information in case of missing exchange rate
        /// </summary>
        public SystemMSGInfo SystemMsgInfo { get; set; }

        /// <summary>
        /// Error type, its value may be used IFF Missing is true
        /// </summary>
        public ErrorTypeList ErrorType
        {
            get;

            set;
        }
        #endregion Accessors

        #region IMissingCommunicationObject Membres
        /// <summary>
        /// True when the currency is missing or disabled or can not be retrieven
        /// </summary>
        public bool Missing
        {
            get;

            set;
        }

        /// <summary>
        /// Error codes returned from the communication object, its value may be used IFF Missing is true
        /// </summary>
        public SysMsgCode ErrorCode
        {
            get 
            {
                switch (ErrorType)
                {
                    case ErrorTypeList.FxAssetDisabled:
                        
                        //return "SYS-01012";
                        return m_SysMsgCode1012;

                    case ErrorTypeList.Default:
                    default:
                        
                        //return "SYS-01013";
                        return m_SysMsgCode1013;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Element definition for a short option compensation communication object. 
    /// When short option positions are embedded in complex portfolios, 
    /// a major part of the risk may be compensated either by long option positions or by corresponding future positions. 
    /// For the uncompensated part of short options positions short option minimum is used. 
    /// This cross margining is provided within a margin class.
    /// </summary>
    public sealed class ShortOptionCompensationCommunicationObject
    {
        
        /// <summary>
        /// Asset id of the short option compensated by a long option position (or corresponding future position) 
        /// defined in the same margin class
        /// </summary>
        public int AssetId
        {
            get;

            set;
        }

        /// <summary>
        /// Compensated quantity
        /// </summary>
        // EG 20150920 [21374] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        public decimal Quantity
        {
            get;

            set;
        }
    }

    #endregion TimsEurexMethod
}
