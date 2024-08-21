using System;
using System.Collections.Generic;
//
using EFS.Common;
using EFS.Common.Log;
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using FpML.v44.Shared;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode None 
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul par la méthode None
    /// </summary>
    public sealed class EuronextVarCalcMethCom : CalcMethComBase, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        private static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1031);
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
        /// Compteurs d'éléments
        /// </summary>
        public MarginCounterCom CounterInfo = new MarginCounterCom();

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarPositionsCom Positions = new EuronextVarPositionsCom();

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarAssetIncompleteCom[] AssetIncomplet;

        /// <summary>
        /// Détail des éléments de calcul par sector
        /// </summary>
        public EuronextVarCalcSectorCom[] EuronextVarSectorDetail;

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
            // Log en cas de paramètres manquants sur la clearing house
            get { return m_SysMsgCode; }
            set { m_SysMsgCode = value; }
        }
        #endregion
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// 
        /// </summary>
        public EuronextVarCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Détail des éléments de calcul par sector
    /// </summary>
    public sealed class EuronextVarCalcSectorCom
    {
        #region Members
        public EuronextVarSector Sector;

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarPositionsCom Positions = new EuronextVarPositionsCom();

        /// <summary>
        ///  Paramètres EuronextVar
        /// </summary>
        public EuronextVarParametersCom EuronextVarParameters;

        /// <summary>
        /// Delivery Parameters
        /// </summary>
        public EuronextVarDeliveryParametersCom[] DeliveryParameters;

        /// <summary>
        /// 
        /// </summary>
        public (int TypeS, int TypeU) LookbackPeriod;

        /// <summary>
        /// 
        /// </summary>
        public ((Decimal DecValue, int IntValue) TypeS, (Decimal DecValue, int IntValue) TypeU) ObservationNumber;

        /// <summary>
        /// 
        /// </summary>
        public (int TypeS, int TypeU) LookbackPeriodDelivery;

        /// <summary>
        /// 
        /// </summary>
        public ((Decimal DecValue, int IntValue) TypeS, (Decimal DecValue, int IntValue) TypeU) ObservationNumberDelivery;

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarMarkToMarketCom MarkToMarket;

        /// <summary>
        /// Détail des éléments de calcul par groupe
        /// </summary>
        public EuronextVarCalcGroupCom[] EuronextVarGroupDetail;

        /// <summary>
        /// 
        /// </summary>
        public (string Function, Money Amount) SectorAdditionalMargin;

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarCalcMoneyCom GroupTotalMargin;

        /// <summary>
        /// Détail des éléments de calcul des échéances proche en livraison physique
        /// </summary>
        public EuronextVarCalcPhyDlyNearExpiryCom[] EuronextVarNearExpiryDetail;

        /// <summary>
        /// 
        /// </summary>
        public (string Function, Money Amount) NearExpiryTotalMargin;

        /// <summary>
        /// Détail des éléments de calcul des livraisons physique
        /// </summary>
        public EuronextVarCalcPhysicalDeliveryCom[] EuronextVarPhysicalDeliveryDetail;
        
        /// <summary>
                                                                                        /// 
                                                                                        /// </summary>
        public (string Function, Money Amount) PhysicalDeliveryTotalMargin;

        /// <summary>
        /// 
        /// </summary>
        public (string Function, Money Amount) SectorTotalMargin;

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarCalcMoneyCom TotalMargin;
        #endregion Members
    }

    /// <summary>
    /// Détail des éléments de calcul par product group
    /// </summary>
    public sealed class EuronextVarCalcGroupCom
    {
        #region Members
        public string Group;

        /// <summary>
        /// 
        /// </summary>
        public (EuronextVarExpectedShortfallCom TypeS, EuronextVarExpectedShortfallCom TypeU) ExpectedShortfallBook;

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<string, (EuronextVarExpectedShortfallCom TypeS, EuronextVarExpectedShortfallCom TypeU)> ExpectedShortfallDecorrelation;

        /// <summary>
        /// 
        /// </summary>
        public ((string Function, decimal Value) TypeS, (string Function, decimal Value) TypeU) DecorellationExpectedShortfall;

        /// <summary>
        /// 
        /// </summary>
        public ((string Function, decimal Value) TypeS, (string Function, decimal Value) TypeU) DecorrelationAddOnResult;

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarCalcCom AdditionalMargin;
        #endregion Members

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public EuronextVarCalcGroupCom()
        {
            ExpectedShortfallBook.TypeU = new EuronextVarExpectedShortfallCom();
            ExpectedShortfallBook.TypeS = new EuronextVarExpectedShortfallCom();
            ExpectedShortfallDecorrelation = new Dictionary<string, (EuronextVarExpectedShortfallCom TypeS, EuronextVarExpectedShortfallCom TypeU)>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Détail des éléments de calcul par pour les échéances proche en livraison physique
    /// </summary>
    public sealed class EuronextVarCalcPhyDlyNearExpiryCom
    {
        #region Members
        /// <summary>
        /// Isin Code
        /// </summary>
        public string IsinCode;

        /// <summary>
        /// Increase Percentage
        /// </summary>
        public decimal IncreasePercentage;

        /// <summary>
        /// Expected Shortfall
        /// </summary>
        public (EuronextVarExpectedShortfallCom TypeS, EuronextVarExpectedShortfallCom TypeU) ExpectedShortfallAssetNearExpiry;

        /// <summary>
        /// RiskMeasure Initial Margin
        /// </summary>
        public EuronextVarCalcCom RiskMeasureMargin;

        /// <summary>
        /// Floor Margin
        /// </summary>
        public (string Function, decimal Value) FloorMargin;

        /// <summary>
        /// Near Expiry Physical Delivery Margin
        /// </summary>
        public (string Function, decimal Value) NearExpiryMargin;
        #endregion Members
    }

    /// <summary>
    /// Détail des éléments de calcul par pour les livraisons physique
    /// </summary>
    public sealed class EuronextVarCalcPhysicalDeliveryCom
    {
        #region Members
        /// <summary>
        /// Isin Code
        /// </summary>
        public string IsinCode;

        /// <summary>
        /// Extra Percentage
        /// </summary>
        public decimal ExtraPercentage;

        /// <summary>
        /// Expected Shortfall
        /// </summary>
        public (EuronextVarExpectedShortfallCom TypeS, EuronextVarExpectedShortfallCom TypeU) ExpectedShortfallAssetDelivery;

        /// <summary>
        /// RiskMeasure Initial Margin
        /// </summary>
        public EuronextVarCalcCom RiskMeasureMargin;

        /// <summary>
        /// Increased Margin
        /// </summary>
        public (string Function, decimal Value) IncreasedMargin;
        
        /// <summary>
        /// Floor Margin
        /// </summary>
        public (string Function, decimal Value) FloorMargin;

        /// <summary>
        /// Physical Delivery Margin
        /// </summary>
        public (string Function, decimal Value) DeliveryMargin;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class EuronextVarPositionsCom : IMissingCommunicationObject
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1034);

        /// <summary>
        /// Positions exclues du calcul car sur des asset erronés
        /// </summary>
        public Pair<PosRiskMarginKey, RiskMarginPosition>[] DiscartedPositions { get; set; }

        /// <summary>
        /// Positions utilisés par le calcul
        /// </summary>
        public Pair<PosRiskMarginKey, RiskMarginPosition>[] ConsideredPositions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool Missing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public SysMsgCode ErrorCode
        {
            get { return m_SysMsgCode; }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class EuronextVarParametersCom
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal OrdinaryConfidenceLevel;
        /// <summary>
        /// 
        /// </summary>
        public decimal StressedConfidenceLevel;
        /// <summary>
        /// 
        /// </summary>
        public decimal DecorrelationParameter;
        /// <summary>
        /// 
        /// </summary>
        public decimal OrdinaryWeight;
        /// <summary>
        /// 
        /// </summary>
        public decimal StressedWeight;
        /// <summary>
        /// Holding Period
        /// </summary>
        public int HoldingPeriod;
        /// <summary>
        /// Sub-portfolio separator : number of markets days between evaluation date and expiry date of the physical delivery futures
        /// </summary>
        public int SubPortfolioSeparatorDaysNumber;
    }

    /// <summary>
    /// Delivery Parameters
    /// </summary>
    public sealed class EuronextVarDeliveryParametersCom
    {
        /// <summary>
        /// Symbol Code du Contrat
        /// </summary>
        public string ContractCode;
        /// <summary>
        /// Devise
        /// </summary>
        public string Currency;
        /// <summary>
        /// Sens
        /// </summary>
        public string Sens;
        /// <summary>
        /// Extra Percentage
        /// </summary>
        public decimal ExtraPercentage;
        /// <summary>
        /// Margin Percentage
        /// </summary>
        public decimal MarginPercentage;
        /// <summary>
        /// Fee Percentage
        /// </summary>
        public decimal FeePercentage;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarExpectedShortfallCom
    {
        /// <summary>
        /// 
        /// </summary>
        public Dictionary<int, decimal> ExtremeEvents;

        /// <summary>
        /// 
        /// </summary>
        public (string Funcion, decimal Value) Result;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarMarkToMarketCom : IMissingCommunicationObject
    {
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1038);

        /// <summary>
        /// 
        /// </summary>
        public EuronextVarMarkToMarketPosCom[] MarkToMarketPos;

        /// <summary>
        /// 
        /// </summary>
        public (string Function, Money Value) MarkToMarketAmount;

        /// <summary>
        /// Code d'erreur
        /// </summary>
        public SysMsgCode ErrorCode
        {
            get { return m_SysMsgCode; }
        }

        /// <summary>
        /// Missing status
        /// </summary>
        public bool Missing
        {
            get;
            set;
        }
    }

    /// <summary>
    ///  Détail d'un calcul de MarkToMarket
    /// </summary>
    public class EuronextVarMarkToMarketPosCom
    {
        /// <summary>
        ///  Rpérente la position pour laquelle un MarkToMarket a été calculé
        /// </summary>
        public Pair<PosRiskMarginKey, RiskMarginPosition> Position;

        /// <summary>
        /// Prix de l'asset ETD. Renseigné uniquement lorsque nécessaire (Valeur à zéro si le prix est inexistant ou non certifié)
        /// </summary>
        public Nullable<decimal> Price;
        /// <summary>
        ///  Retourne true lorsque le prix <see cref="Price"/> ,nécessaire au MarkToMarket, est inexistant
        /// </summary>
        public Tuple<Boolean, SystemMSGInfo> PriceMissing = new Tuple<bool, SystemMSGInfo>(false, null);

        /// <summary>
        /// Prix de de l'actif ss jacent de l'asset ETD. Renseigné uniquement lorsque nécessaire (Valeur à zéro si le prix est inexistant ou non certifié)
        /// </summary>
        public Nullable<decimal> UnderlyingPrice;
        /// <summary>
        ///  Retourne true lorsque le prix <see cref="UnderlyingPrice"/>, nécessaire au MarkToMarket, est inexistant
        /// </summary>
        public Tuple<Boolean, SystemMSGInfo> UnderlyingPriceMissing = new Tuple<bool, SystemMSGInfo>(false, null);

        /// <summary>
        /// Multiplier de l'asset ETD
        /// </summary>
        public decimal Multiplier;

        /// <summary>
        /// Résultat du MartToMarket de la position <see cref="Position"/>
        /// </summary>
        public Money MarkToMarket;

        /// <summary>
        ///  Obtient true dès qu'un prix (<see cref="Price"/> ou <see cref="UnderlyingPrice"/>) est inexistant
        /// </summary>
        public bool MissingPrice
        {
            get { return (PriceMissing.Item1 || UnderlyingPriceMissing.Item1); }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalcMoneyCom
    {
        /// <summary>
        /// 
        /// </summary>
        public Decimal Value1;
        /// <summary>
        /// 
        /// </summary>
        public Decimal Value2;
        /// <summary>
        /// 
        /// </summary>
        public (string Function, Money Amount) Result;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarCalcCom
    {
        /// <summary>
        /// 
        /// </summary>
        public decimal Value1;
        /// <summary>
        /// 
        /// </summary>
        public decimal Value2;
        /// <summary>
        /// 
        /// </summary>
        public (string Function, decimal Value) Result;
    }

    /// <summary>
    /// 
    /// </summary>
    public class EuronextVarAssetIncompleteCom
    {
        public Tuple<int, string> Asset;
        public int NbScenarioTypeS;
        public int NbScenarioTypeU;
    }
}
