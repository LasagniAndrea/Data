using System;
using System.Collections.Generic;
//
using EFS.LoggerClient.LoggerService;
using EFS.SpheresRiskPerformance.CommunicationObjects.Interfaces;
//
using EfsML.Enum;
//
using FpML.Interface;

namespace EFS.SpheresRiskPerformance.CommunicationObjects
{
    /// <summary>
    /// Objet de communication decrivant l'ensemble minimum de données que doit passer l'objet de calcul de la méthode None 
    /// à l'objet référentiel de la feuille de calcul de sorte à construire le noeud du calcul par la méthode None
    /// </summary>
    /// PM 20230818 [XXXXX] Remplacement de l'implémentation de IMarginCalculationMethodCommunicationObject par l'héritage de CalcMethComBase
    public sealed class NoMarginCalcMethCom : CalcMethComBase, IMissingCommunicationObject
    {
        #region Members
        #region static Members
        private readonly static SysMsgCode m_SysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1031);
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
        /// Collection des trades concernés groupés par asset
        /// </summary>
        public List<NoMarginMethAssetGroupCom> AssetGroup;

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
        }
        #endregion
        #endregion Members

        #region Accessors
        #endregion Accessors

        #region Constructor
        /// <summary>
        /// Constructeur
        /// </summary>
        public NoMarginCalcMethCom()
        {
            UnderlyingStock = new List<StockCoverageDetailCommunicationObject>();
        }
        #endregion Constructor
    }

    /// <summary>
    /// Objet de communication pour une collection de trades d'un asset
    /// </summary>
    public class NoMarginMethAssetGroupCom
    {
        #region Members
        /// <summary>
        /// Id interne du marché
        /// </summary>
        public int IdM;

        /// <summary>
        /// Identifiant du marché
        /// </summary>
        public string MarketIdentifier;

        /// <summary>
        /// Id interne de l'instrument
        /// </summary>
        public int IdI;

        /// <summary>
        /// Id interne du contrat
        /// </summary>
        public int IdContract;

        /// <summary>
        /// Identifiant du contrat
        /// </summary>
        public string ContractIdentifier;

        /// <summary>
        /// Id interne de l'asset
        /// </summary>
        public int IdAsset;

        /// <summary>
        /// Identifiant de l'asset
        /// </summary>
        public string AssetIdentifier;

        /// <summary>
        /// Catégorie de l'asset
        /// </summary>
        public string AssetCategory;

        /// <summary>
        /// Devise
        /// </summary>
        public string Currency;

        /// <summary>
        /// Collection des trades concernés
        /// </summary>
        public List<NoMarginMetTradesCom> Trades;
        #endregion Members
    }

    /// <summary>
    /// Objet de communication pour une collection de trades
    /// </summary>
    public class NoMarginMetTradesCom
    {
        #region Members
        /// <summary>
        /// Id interne du trade
        /// </summary>
        public int IdT;

        /// <summary>
        /// Identifiant du trade
        /// </summary>
        public string TradeIdentifier;

        /// <summary>
        /// Id interne de l'acteur dealer
        /// </summary>
        public int IdA_Dealer;

        /// <summary>
        /// Id interne du book du dealer
        /// </summary>
        public int IdB_Dealer;

        /// <summary>
        /// Id interne de l'acteur clearer
        /// </summary>
        public int IdA_Clearer;

        /// <summary>
        /// Id interne du book du clearer
        /// </summary>
        public int IdB_Clearer;

        /// <summary>
        /// Date business du trade
        /// </summary>
        public DateTime DtBusiness;

        /// <summary>
        /// Horodatage du trade
        /// </summary>
        public DateTime DtTimestamp;

        /// <summary>
        /// Date d'execution du trade
        /// </summary>
        public DateTime DtExecution;

        /// <summary>
        /// Time zone des dates
        /// </summary>
        public string TzFacility;

        /// <summary>
        /// Sens du trade côté dealer
        /// </summary>
        public string Side;

        /// <summary>
        /// Quantité d'asset du trade
        /// </summary>
        public decimal Quantity;

        /// <summary>
        /// Prix du trade
        /// </summary>
        public decimal Price;
        #endregion Members
    }
}
