#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;

using EFS.GUI.CCI;
using EFS.Status;
using EFS.Permission;
using EFS.Tuning;
using EFS.Restriction;

using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FixML.v50SP1.Enum;
using FpML.Interface;

using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Classe qui représente un trade (contient le dataDocument...)  
    /// <para>Contient la collection cci pour consulter, modifier le trade</para>
    /// </summary>
    /// EG 20150515 m_RemoveTrade de type RemoveTradeMsg
    public class TradeCommonInput : CommonInput, ICustomCaptureInfos
    {
        #region Members
        /// <summary>
        /// Représente l'identification du trade 
        /// </summary>
        protected SpheresIdentification m_identification;
        /// <summary>
        /// Représente le trade [représentation SQL]
        /// </summary>
        protected SQL_TradeCommon m_SQLTrade;
        /// <summary>
        /// Représente les liens présents dans TRADELINK pour le trade
        /// </summary>
        protected SQL_TradeLink m_SQLTradeLink;
        /// <summary>
        /// 
        /// </summary>
        protected SQL_LastTrade_L m_SQLLastTradeLog;
        /// <summary>
        /// Représente le DataDocument
        /// </summary>
        protected DataDocumentContainer m_DataDocument;
        /// <summary>
        /// Représente les traitements opérés avec succès sur le Trade
        /// </summary>
        protected Cst.ProcessTypeEnum[] m_EventProcessInSucces;
        /// <summary>
        /// Représente l'instrument du trade [représentation SQL]
        /// </summary>
        protected SQL_Instrument m_SQLInstrument;
        /// <summary>
        /// Représente le product du trade [représentation SQL]
        /// </summary>
        protected SQL_Product m_SQLProduct;

        /// <summary>
        /// Représente les directives d'annulation d'un trade, d'une facture, d'un titre etc..
        /// <para>Ce membre est valorisé uniquement lors d'une annulation (avec ou sans remplaçante)</para>
        /// </summary>
        protected RemoveTradeMsg m_RemoveTrade;

        /// <summary>
        /// Représente les directives d'optional early termination provision pour un FX option
        /// </summary>
        // EG 20180514 [23812] Report
        protected FxOptionalEarlyTerminationProvisionMsg m_FxOETMsg;

        /// <summary>
        /// Représente la liste des trades liés à ce Trade.
        /// <para>Ce membre est facultatif</para>
        /// </summary>
        protected List<TradeLink.TradeLink> m_TradeLink;
        /// <summary>
        /// 
        /// </summary>
        private TradeCommonCustomCaptureInfos m_CustomCaptureInfos;
        //FI 20120226 [] Mise en commentaire cette données se trouvent dans la class TradeImportInput
        ///// <summary>
        ///// 
        ///// </summary>
        //private FullCustomCaptureInfo[] m_FullCustomCaptureInfos;
        /// <summary>
        /// Représente les directives des parties pour ce qui concerne l'envoi de notification 
        /// </summary>
        private readonly TradeNotification m_TradeNotification;
        /// <summary>
        /// Représente les statuts associé au trade
        /// </summary>
        private TradeStatus m_TradeStatus;

        //
        //private EfsML.v30.Doc.EfsDocument m_efsDataDocument;
        #endregion Members

        #region Accessors

        /// <summary>
        /// Obtient ou Définit les élements d'identification d'un trade (Identifier,DisplayName,Description,etc...)  
        /// </summary>
        ///20091016 FI [Rebuild identification] add m_identification
        [System.Xml.Serialization.XmlIgnore()]
        public SpheresIdentification Identification
        {
            get { return m_identification; }
            set { m_identification = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlArray("customCaptureInfos")]
        [System.Xml.Serialization.XmlArrayItem("customCaptureInfo", typeof(CustomCaptureInfoDynamicData))]
        public TradeCommonCustomCaptureInfos CustomCaptureInfos
        {
            set { m_CustomCaptureInfos = value; }
            get { return m_CustomCaptureInfos; }
        }

        ///// <summary>
        ///// 
        ///// </summary>
        //[System.Xml.Serialization.XmlArray("fullCustomCaptureInfos")]
        //[System.Xml.Serialization.XmlArrayItem("fullCustomCaptureInfo", typeof(FullCustomCaptureInfo))]
        //public FullCustomCaptureInfo[] FullCustomCaptureInfos
        //{
        //    set { m_FullCustomCaptureInfos = value; }
        //    get { return m_FullCustomCaptureInfos; }
        //}

        //
        //#region importDataDocument
        //[System.Xml.Serialization.XmlElementAttribute("EfsML", Namespace = "http://www.efs.org/2007/EFSmL-3-0", Type = typeof(EfsML.v30.Doc.EfsDocument))]
        //public EfsML.v30.Doc.EfsDocument EfsML
        //{
        //    get { return m_efsDataDocument; }
        //    set { m_efsDataDocument  = value; }
        //}
        //#endregion importDataDocument
        //

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public CustomCaptureInfosBase CcisBase
        {
            get { return (CustomCaptureInfosBase)m_CustomCaptureInfos; }
        }

        /// <summary>
        /// Obtient le trade courant du dataDocument
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public ITrade CurrentTrade
        {
            get { return DataDocument.CurrentTrade; }
        }

        /// <summary>
        /// Obtient et définit le dataDocument
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public DataDocumentContainer DataDocument
        {
            set { m_DataDocument = value; }
            get { return m_DataDocument; }
        }

        /// <summary>
        /// Obtient le product courant dans le dataDocument
        /// <para>Généralement le product du 1er trade</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public ProductContainer Product
        {
            get
            {
                ProductContainer ret = null;
                if (null != m_DataDocument)
                    ret = m_DataDocument.CurrentProduct;
                return ret;
            }
        }

        /// <summary>
        /// Obtient une signature,une qualification qui caractérise le trade
        /// </summary>
        /// EG 20190730 Upd (Gestion TrdType sur DebtSecurityTransaction)
        [System.Xml.Serialization.XmlIgnore()]
        public virtual string DisplayNameInstrument
        {
            get
            {
                string ret = string.Empty;
                if (IsTradeFound)
                {
                    // EG 20100208 Add productContainer
                    ProductContainer productContainer = DataDocument.CurrentProduct;
                    if (productContainer.IsSwaption)
                    {
                        ISwaption swaption = (ISwaption)productContainer.Product;
                        if (swaption.ExerciseAmericanSpecified)
                            ret = "American";
                        if (swaption.ExerciseEuropeanSpecified)
                            ret = "European";
                        if (swaption.ExerciseBermudaSpecified)
                            ret = "Bermuda";
                        //
                    }
                    if (productContainer.IsFxDigitalOption)
                    {
                        IFxDigitalOption fxdigOpt = (IFxDigitalOption)productContainer.Product;
                        if (fxdigOpt.TypeTriggerAmericanSpecified)
                            ret = "American";
                        if (fxdigOpt.TypeTriggerEuropeanSpecified)
                            ret = "European";
                        //
                    }
                    if (productContainer.IsCapFloor)
                    {
                        //
                        CapFloorContainer capFloor = new CapFloorContainer((ICapFloor)productContainer.Product);
                        InterestRateStreamContainer irs = new InterestRateStreamContainer(capFloor.CapFloor.Stream);
                        //
                        if (irs.IsCapped && (false == irs.IsFloored))
                        {
                            if (capFloor.IsCap())
                            {
                                ret = "Cap";
                            }
                            else if (capFloor.IsCorridor())
                            {
                                ret = "Corridor";
                            }
                            //
                            if (StrFunc.IsEmpty(ret))
                            {
                                // l'opération est qualifée exotic   
                                ret = "Cap exotic";
                            }
                        }
                        else if (irs.IsFloored && (false == irs.IsCapped))
                        {
                            if (capFloor.IsFloor())
                            {
                                ret = "Floor";
                            }
                            //
                            if (StrFunc.IsEmpty(ret))
                            {
                                // l'opération est qualifée exotic   
                                ret = "Floor exotic";
                            }
                        }
                        else if (irs.IsCapped && irs.IsFloored)
                        {
                            if (capFloor.IsStraddle())
                            {
                                ret = "Straddle";
                            }
                            else if (capFloor.IsStrangle())
                            {
                                ret = "Strangle";
                            }
                            else if (capFloor.IsCollar())
                            {
                                ret = "Collar";
                            }
                            if (StrFunc.IsEmpty(ret))
                            {
                                // l'opération est qualifée exotic   
                                ret = "CapFloor exotic";
                            }
                        }
                    }
                    else if (productContainer.IsExchangeTradedDerivative)
                    {
                        ExchangeTradedDerivativeContainer etd = 
                            new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productContainer.Product, DataDocument);
                        if (etd.TradeCaptureReport.TrdTypeSpecified && etd.TradeCaptureReport.TrdType != TrdTypeEnum.RegularTrade)
                            ret = etd.TradeCaptureReport.TrdType.ToString();
                    }
                    else if (productContainer.IsDebtSecurityTransaction)
                    {
                        DebtSecurityTransactionContainer dst =
                            new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productContainer.Product, DataDocument);
                        if (dst.DebtSecurityTransaction.TrdTypeSpecified &&
                            dst.DebtSecurityTransaction.TrdType != TrdTypeEnum.RegularTrade)
                            ret = dst.DebtSecurityTransaction.TrdType.ToString();
                    }

                    if (StrFunc.IsFilled(ret) && (ret != SQLInstrument.DisplayName))
                    {
                        //ret = SQLInstrument.DisplayName + Cst.Space + "[" + ret + "]";
                        ret = SQLInstrument.DisplayName + Cst.Space + ret;
                    }
                    else
                    {
                        ret = SQLInstrument.DisplayName;
                    }
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public IDataDocument FpMLDataDocReader
        {
            get
            {
                return (IDataDocument)m_DataDocument.DataDocument;
            }
            set
            {
                //FI 20111005 => Nouvelle instance de m_DataDocument (currentTrade et currentProduct) sont synchros avec l'object value
                m_DataDocument = new DataDocumentContainer(value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public IDocument FpMLDocReader
        {
            get { return m_DataDocument.Document; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public EfsMLDocumentVersionEnum EfsMlVersion
        {
            get { return ((IEfsDocument)DataDocument.Document).EfsMLversion; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool ExistEventProcessInSucces
        {
            get { return ArrFunc.IsFilled(m_EventProcessInSucces); }
        }

        /// <summary>
        /// Obtient l'identifier du trade
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public string Identifier
        {
            get
            {
                //20091016 FI [Rebuild identification] use m_identification
                string ret = string.Empty;
                if (IsTradeFound)
                    ret = m_identification.Identifier;
                return ret;
            }
        }

        /// <summary>
        /// Obtient l'identifier d'origine du trade
        /// </summary>
        // RD 20150807 Add
        [System.Xml.Serialization.XmlIgnore()]
        public string LastIdentifier
        {
            get
            {
                string ret = string.Empty;
                if (IsTradeFound)
                {
                    if(StrFunc.IsFilled(m_identification.LastIdentifier))
                        ret = m_identification.LastIdentifier;
                    else
                        ret = m_identification.Identifier;
                }
                return ret;
            }
        }

        /// <summary>
        /// Obtient l'identifiant non significatif du trade (IDT)
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public int IdT
        {
            get
            {
                int ret = 0;
                if (IsTradeFound)
                    ret = m_SQLTrade.Id;
                return ret;
            }
        }

        //FI 20170116 [21916] property abandonnée car n'est plus uitlisé
        /*
        /// <summary>
        /// Obtient true s'il existe une action dans POSACTION pour la date business courante
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsEventsPosAction
        {
            get;
            private set;
        }
        */

        /// <summary>
        /// Obtient true s'il existe un CashBalance (inpliquant ce trade) dont la date est inférieure ou égale à la date business date en vigueur
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsExistCashBalance
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la quantité disponible sur une Alloc
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        /// </summary>
        /// EG 20150920 [21314] Int (int32) to Long (Int64)
        // EG 20170127 Qty Long To Decimal
        [System.Xml.Serialization.XmlIgnore()]
        public decimal AvailableQuantity
        {
            get;
            private set;
        }
        /// <summary>
        /// Retourne la ClearingBusinessDate sur une Alloc
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize (donc non valorisée dans le cadre d'une création de trade!)</para>
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime ClearingBusinessDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne la BusinessDate courante (celle présente dans ENTITYMARKET)
        /// <para>Retourne DateTime.minValue si Trade autre que alloc</para>
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        /// </summary>
        /// FI 20121113 [18224] Sauvegarde de la date business Courante, cela évite de faire des   
        [System.Xml.Serialization.XmlIgnore()]
        public DateTime CurrentBusinessDate
        {
            get;
            private set;
        }


        /// <summary>
        /// Obtient true si les évènements sont générés
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsEventsGenerated
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true si ETD ou si ESE ou si RTS et Allocation
        /// </summary>
        public bool IsETDorESEorRTSAllocation
        {
            get
            {
                bool ret = false;
                if (IsTradeFound)
                {
                    ret = SQLProduct.IsExchangeTradedDerivative 
                        || SQLProduct.IsESE
                        || (SQLProduct.IsRTS && TradeStatus.IsStBusiness_Alloc);
                }
                return ret;
            }
        }
        
        /// <summary>
        /// Obtient true si Allocation => Produit ExchangeTradedDerivative et businessType = 'ALLOCATION'
        /// <para>Les strategies homogènes sur produits listés ne sont pas des allocations</para>
        /// <para>Si allocation alors les contreparties sont le DO et le compensateur</para>
        /// </summary>
        /// FI 20120703 [17982] Refactoring IsETDandAllocation ne vaut true que si IsTradeFound = true;
        public bool IsETDandAllocation
        {
            get
            {
                bool ret = false;
                if (IsTradeFound)
                {
                    ret = TradeStatus.IsStBusiness_Alloc;
                    ret &= SQLProduct.IsExchangeTradedDerivative;
                }
                return ret;
            }
        }
        /// EG 20150624 [21151] New  
        public bool IsESEandAllocation
        {
            get
            {
                bool ret = false;
                if (IsTradeFound)
                {
                    ret = TradeStatus.IsStBusiness_Alloc;
                    ret &= SQLProduct.IsEquitySecurityTransaction;
                }
                return ret;
            }
        }

        /// EG 20150624 [21151] New  
        public bool IsDSTandAllocation
        {
            get
            {
                bool ret = false;
                if (IsTradeFound)
                {
                    ret = TradeStatus.IsStBusiness_Alloc;
                    ret &= SQLProduct.IsDebtSecurityTransaction;
                }
                return ret;
            }
        }


        /// <summary>
        /// Obtient true si Allocation => businessType = 'ALLOCATION'
        /// <para>Les strategies homogènes sur produits listés ne sont pas des allocations</para>
        /// <para>Si allocation alors les contreparties sont le Dealer et le compensateur/custodian</para>
        /// </summary>
        public bool IsTradeFoundAndAllocation
        {
            get
            {
                return IsTradeFound && IsAllocation;
            }
        }

        /// <summary>
        /// Obtient true s'il existe au moins un OPP issu d'un Barème portant sur un Scope "OrderId" ou "FolderId" avec une MIN/MAX
        /// </summary>
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        //PL 2020117 [25099] New
        public bool IsExistOPP_OnFeeScopeOrderIdOrFolderId_WithMinMax
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true si Allocation ET si la Business date est inférieure à la Business date en vigueur.
        /// </summary>
        /// <para>Cette donnée est alimentée par la méthode SearchAndDeserialize</para>
        public bool IsAllocationFromPreviousBusinessDate
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient true si allocation =>  businessType = 'ALLOCATION'
        /// <para>Les strategies homogènes sur produits listés ne sont pas des allocations</para>
        /// <para>Si allocation alors les contreparties sont le dealer et le compensateur</para>
        /// </summary>
        public bool IsAllocation
        {
            get
            {
                return TradeStatus.IsStBusiness_Alloc;
            }
        }

        /// <summary>
        /// Retourne true si ce n'est pas une allocation (exécution, intermédiation, Pre-trade etc..)
        /// <para>Si Deal alors il existe systématiquement 2 contreparties (cas systématique sur les OTC)</para>
        /// </summary>
        public bool IsDeal
        {
            get
            {
                bool ret = (!TradeStatus.IsStBusiness_Alloc);
                return ret;
            }
        }

        /// <summary>
        /// Retourne true si ce n'est pas une Allocation (Exécution, Intermédiation, Pre-trade etc..) et si Produit est ExchangeTradedDerivative
        /// <para>Si Deal alors il existe systématiquement 2 contreparties</para>
        /// <para>Cela concerne aussi les strategies</para>
        /// </summary>
        public bool IsDealETD
        {
            get
            {
                bool ret = false;
                if (IsDeal && IsTradeFound)
                    ret = SQLProduct.IsExchangeTradedDerivative;
                return ret;
            }
        }

        /// <summary>
        /// Retourne l'IdA de l'entité au sein du DataDocument après chargement (SearchAndDeserialize)
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public int EntityOnLoad
        {
            get;
            private set;
        }

        [System.Xml.Serialization.XmlIgnore()]
        public bool IsTradeFound
        {
            get { return ((null != m_SQLTrade) && m_SQLTrade.IsFound); }
        }

        /// <summary>
        /// Le trade existe AVEC son TRADEXML
        /// </summary>
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (New Accessor)
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsTradeFoundWithXML
        {
            get { return IsTradeFound  && (false == String.IsNullOrEmpty(m_SQLTrade.TradeXml)); }
        }
        /// <summary>
        /// Le trade existe SANS son TRADEXML
        /// </summary>
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (New Accessor)
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsTradeFoundWithoutXML
        {
            get { return IsTradeFound && (String.IsNullOrEmpty(m_SQLTrade.TradeXml)); }
        }
        /// <summary>
        /// Obtient true si le trade est annulé
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsTradeRemoved
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient true si le trade est close
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public bool IsTradeClosed
        {
            get
            {
                bool ret = false;
                if (IsTradeFound)
                    ret = m_SQLTrade.RowAttribut == Cst.RowAttribut_InvoiceClosed;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        /// EG 20150515 m_RemoveTrade de type RemoveTradeMsg
        public RemoveTradeMsg RemoveTrade
        {
            get { return m_RemoveTrade; }
        }

        [System.Xml.Serialization.XmlIgnore()]
        // EG 20180514 [23812] Report
        public FxOptionalEarlyTerminationProvisionMsg FxOETMsg
        {
            get { return m_FxOETMsg; }
        }

        /// <summary>
        /// La liste des trades liés à ce Trade.
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public List<TradeLink.TradeLink> TradeLink
        {
            get { return m_TradeLink; }
            set { m_TradeLink = (List<TradeLink.TradeLink>)value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_Instrument SQLInstrument
        {
            get { return m_SQLInstrument; }
            set { m_SQLInstrument = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_Product SQLProduct
        {
            get { return m_SQLProduct; }
            set { m_SQLProduct = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_TradeCommon SQLTrade
        {
            set { m_SQLTrade = value; }
            get { return m_SQLTrade; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_TradeLink SQLTradeLink
        {
            get { return m_SQLTradeLink; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public SQL_LastTrade_L SQLLastTradeLog
        {
            get { return m_SQLLastTradeLog; }
        }

        /// <summary>
        /// Obtient les dérectives de messagerie
        /// </summary>
        [System.Xml.Serialization.XmlIgnore()]
        public TradeNotification TradeNotification
        {
            get { return m_TradeNotification; }
        }

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool TradeStatusSpecified;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElement("tradeStatus", typeof(TradeStatus))]
        public TradeStatus TradeStatus
        {
            set { m_TradeStatus = value; }
            get { return m_TradeStatus; }
        }

        /// <summary>
        /// Give-Up (Allocation émise)
        /// <para>Définition: Trade ALLOC alloué à un "Clearing Broker", acteur de rôle CLEARER (et BROKER)</para>
        /// <para>Un trade est considéré comme un "Give-Up", dès lors qu'il porte sur un Book sans gestion de position.</para>
        /// <para>Le "Clearing Broker" (GiveUp Clearing) peut on non être présent sur le trade</para>
        /// </summary>
        /// 
        public bool IsGiveUp(string pCS, IDbTransaction pDbTransaction)
        {
            return IsETDandAllocation && (false == IsPosKeepingOnBookDealer(pCS, pDbTransaction));
        }

        /// <summary>
        /// Take-Up (Allocation reçue)
        /// <para>Déf.: Trade ALLOC reçu d'un "Executing Broker", acteur de rôle BROKER</para>
        /// <remarks>Rq.: Un trade est considéré comme un "Take-Up", dès lors qu'il existe sur le trade "Broker" qui:
        /// <para>     - ne soit pas l'Entité de gestion du Book du dealer</para>
        /// <para>     - ne soit pas le Dealer*</para>
        /// <para>     - ne soit pas le Clearing Broker si le trade est également un Give-Up</para>
        /// <para>* Dans le cas d'un trade exécuté par un NCM, chez le GCM détenteur de Spheres®, le trade aura pour Dealer et Executing Broker, le NCM.</para>
        /// <para>  Cependant le trade ne sera pas pour autant un Take-Up, il aboutit automatiquement chez le GCM.</para>
        /// </remarks>
        /// /// </summary>
        /// EG 20120619 New Take-Up/Give-Up
        public bool IsTakeUp(string pCS, IDbTransaction pDbTransaction)
        {
            return IsETDandAllocation && IsExistTakeUpExecutingBroker(pCS, pDbTransaction);
        }

        /// <summary>
        /// <remarks>Warning: Anciennement "IsExistExecutingBrokerNotDealer"</remarks>
        /// </summary>
        // EG 20120619 New Take-Up/Give-Up
        // PL 20120720 Add pIsGiveUp
        // EG 20180307 [23769] Gestion dbTransaction
        private bool IsExistTakeUpExecutingBroker(string pCS, IDbTransaction pDbTransaction)
        {
            bool ret = false;
            if (IsETDandAllocation)
            {
                ExchangeTradedContainer etd = new ExchangeTradedContainer((IExchangeTradedBase)Product.Product);

                IFixParty dealerFixParty = etd.GetDealer();
                if (null != dealerFixParty)
                {
                    IParty dealerParty = DataDocument.GetParty(dealerFixParty.PartyId.href);
                    if (null != dealerParty)
                    {
                        //FI 20121113 [18224] add  CSTools.SetCacheOn
                        bool isGiveUp = IsGiveUp(pCS, pDbTransaction);
                        ret = DataDocument.IsExistExecutingBrokerWithExclude(CSTools.SetCacheOn(pCS), pDbTransaction, dealerParty.OTCmlId, isGiveUp);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Get the position keeping flag of the dealer book
        /// </summary>
        /// EG 20140731 Refactoring (RptSideContainer)
        /// FI 20161005 [XXXXX] Modify
        /// FI 20170116 [21916] Modift
        // EG 20180307 [23769] Gestion dbTransaction
        public Boolean IsPosKeepingOnBookDealer(string pCS, IDbTransaction pDbTransaction)
        {
            bool ret = false;
            if (IsTradeFoundAndAllocation)
            {
                // FI 20170116 [21916] call Product.RptSide
                //RptSideProductContainer rptSide = Product.RptSide(CS, IsAllocation);
                RptSideProductContainer rptSide = Product.RptSide();
                // FI 20161005 [XXXXX] Add NotImplementedException
                if (null == rptSide)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", Product.ProductBase.ToString()));
                ret = rptSide.IsPosKeepingOnBookDealer(CSTools.SetCacheOn(pCS), pDbTransaction);
            }
            return ret;
        }

        /// <summary>
        /// Get the position keeping flag of the clearer book
        /// </summary>
        /// EG 20140731 Refactoring (RptSideContainer)
        /// FI 20161005 [XXXXX] Modify
        /// FI 20170116 [21916] Modify
        public Boolean IsPosKeepingOnBookClearer(string pCS, IDbTransaction pDbTransaction)
        {
            bool ret = false;
            if (IsTradeFoundAndAllocation)
            {
                //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)Product.product, DataDocument);
                //ret = etd.IsPosKeepingOnBookClearer(CSTools.SetCacheOn(CS), DataDocument);

                // FI 20170116 [21916] call  Product.RptSide()
                //RptSideProductContainer _rptSideContainer = Product.RptSide(CS, IsAllocation);
                RptSideProductContainer _rptSideContainer = Product.RptSide();

                // FI 20161005 [XXXXX] Add NotImplementedException
                if (null == _rptSideContainer)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", Product.ProductBase.ToString()));

                ret = _rptSideContainer.IsPosKeepingOnBookClearerCustodian(CSTools.SetCacheOn(pCS), pDbTransaction);
            }
            return ret;

        }
        #endregion Accessors


        #region Constructor
        public TradeCommonInput()
            : base()
        {
            m_TradeStatus = new TradeStatus();
            m_TradeNotification = new TradeNotification();
        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier"></param>
        public virtual void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pTradeIdentifier)
        {
            InitializeSqlTrade(pCS, pDbTransaction, pTradeIdentifier, SQL_TableWithID.IDType.Identifier);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        public virtual void InitializeSqlTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType)
        {
            if (SQL_TableWithID.IDType.Id == pIdType)
            {
                m_SQLTrade = new SQL_TradeCommon(pCS, IntFunc.IntValue(pId));
            }
            else if (SQL_TableWithID.IDType.Identifier == pIdType)
            {
                m_SQLTrade = new SQL_TradeCommon(pCS, pId);
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("parameter type {0} is not supported", pIdType.ToString()));
            }
            m_SQLTrade.DbTransaction = pDbTransaction;
        }

        /// <summary>
        /// Retourne la source du Product
        /// </summary>
        public string ProductSource()
        {
            string ret = string.Empty;
            if (IsTradeFound)
                ret = m_SQLProduct.Source;
            return ret;
        }

        /// <summary>
        /// Retourne true  si l'instrument du trade génère des évènements
        /// </summary>
        public bool IsInstrumentEvents()
        {
            bool ret = false;
            if (IsTradeFound)
                ret = m_SQLInstrument.IsEvents;
            return ret;
        }

        /// <summary>
        /// Retourne true si l'instrument du trade ne génère pas les  évènements d'intérêts
        /// </summary>
        public bool IsInstrumentNoINTEvents()
        {

            bool ret = false;
            if (IsTradeFound)
                ret = m_SQLInstrument.IsNoINTEvents;
            return ret;

        }

        /// <summary>
        /// Retourne true si l'instrument gère les extend
        /// </summary>
        public bool IsExtend()
        {
            bool ret = false;
            if (IsTradeFound)
                ret = m_SQLInstrument.IsExtend && (EfsMlVersion != EfsMLDocumentVersionEnum.Version20);
            return ret;
        }

        /// <summary>
        /// Retourne true si le trade a déjà été facturé
        /// </summary>
        /// EG 20140902 Add (null != SQLTradeLink)
        public bool IsTradeInInvoice()
        {
            bool ret = false;
            if (IsTradeFound)
                ret = (null != SQLTradeLink) && SQLTradeLink.IsInvoiced;
            return ret;
        }

        /// <summary>
        /// Retourne l'identifier du template à l'origine du trade
        /// <para>Cette propriété s'appuie sur la valeur de la colonnne TRADE.IDT_TEMPLATE</para>
        /// </summary>
        // EG 20180205 [23769] Add dbTransaction  
        public string GetTemplateIdentifier(string pCS)
        {
            return GetTemplateIdentifier(pCS, null);
        }
        // EG 20180205 [23769] Add dbTransaction  
        public string GetTemplateIdentifier(string pCS, IDbTransaction pDbTransaction)
        {
            string ret = string.Empty;
            if (IsTradeFound)
            {
                if (0 != m_SQLTrade.IdT_Template)
                    ret = TradeRDBMSTools.GetTradeIdentifier(pCS, pDbTransaction, m_SQLTrade.IdT_Template);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pUser"></param>
        /// <param name="pIsSessionId"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20141107 [20441] Modification de signature 
        // EG 20180425 Analyse du code Correction [CA2235]
        public virtual void InitializeCustomCaptureInfos(string pCS, User pUser, string pIsSessionId, bool pIsGetDefaultOnInitializeCci)
        {
            m_CustomCaptureInfos = new TradeCommonCustomCaptureInfos(pCS, this, pUser, pIsSessionId, pIsGetDefaultOnInitializeCci);
            m_CustomCaptureInfos.InitializeCciContainer();
        }

        /// <summary>
        /// Initialisation des données non renseignées 
        /// <para>Notamment pour alimenter les données obligatoire vis à vis du XSD</para>
        /// </summary>
        // EG 20171025 [23509] Ininitalisation de l'heure sur OrderEntered
        // EG 20171031 [23509] Upd
        // EG 20171115 [23509] Upd
        public virtual void SetDefaultValue(string pCS, IDbTransaction pDbTransaction)
        {
            if (DataDocument.CurrentProduct.IsTradeMarket)
            {
                Nullable<DateTimeOffset> dtOrderEntered = DataDocument.GetOrderEnteredDateTimeOffset();
                if (false == dtOrderEntered.HasValue)
                {
                    ITradeProcessingTimestamps timestamps = DataDocument.CurrentProduct.ProductBase.CreateTradeProcessingTimestamps();
                    // FI 20200903 [XXXXX] Utilisation de OTCmlHelper.GetDateSysUTC
                    DateTimeOffset dtSys = Tz.Tools.FromTimeZone(OTCmlHelper.GetDateSysUTC(pCS), Tz.Tools.UniversalTimeZone).Value;
                    timestamps.OrderEntered = Tz.Tools.ToString(dtSys);
                    timestamps.OrderEnteredSpecified = true;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override void Clear()
        {
            //20091016 FI [Rebuild identification] purge de m_identification
            base.Clear();
            m_SQLTrade = null;
            m_SQLLastTradeLog = null;
            m_identification = null;
            m_SQLInstrument = null;
            m_SQLProduct = null;
            IsEventsGenerated = false;
            //FI 20170116 [21916] property IsEventsPosAction abandonnée car n'est plus utilisée
            //IsEventsPosAction = false;
            IsTradeRemoved = false;
        }

        /// <summary>
        /// Obtient un clone du DataDocument
        /// </summary>
        /// <returns></returns>
        public IDocument CloneDocument()
        {
            return (IDocument)DataDocument.CloneDataDocument();
        }

        /// <summary>
        /// Retourne la tiste des traitements en succès 
        /// </summary>
        /// <returns></returns>
        public Cst.ProcessTypeEnum[] GetEventProcessInSucces()
        {
            return m_EventProcessInSucces;
        }

        /// <summary>
        /// Retourne la date à pré-proposée  en fonction de l'action (exercide, abandon , correction,..)
        /// </summary>
        /// <param name="pMode"></param>
        /// <returns></returns>
        public virtual DateTime GetDefaultDateAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            DateTime ret = DateTime.MinValue;
            //
            if (Cst.Capture.IsModeRemove(pMode))
            {
                //DateTime dtSysBusiness = OTCmlHelper.GetDateSysBusiness(CS);
                //DateTime dtSysBusiness = Tools.GetDateBusiness(CS, DataDocument);
                //FI 20121113 [18224] Lecture de la date courante via CurrentBusinessDate
                DateTime dtSysBusiness = this.CurrentBusinessDate;  
                DateTime tradeDate = CurrentTrade.AdjustedTradeDate;
                DateTime removeMinDate = dtSysBusiness;
                DateTime removeMaxDate = dtSysBusiness;
                //
                if (0 < DateTime.Compare(tradeDate, removeMinDate))
                    removeMinDate = tradeDate;
                //
                if (0 < DateTime.Compare(removeMinDate, removeMaxDate))
                    ret = removeMaxDate;
                else
                    ret = removeMinDate;
            }
            return ret;
        }

        /// <summary>
        ///Initialisation du Statut StActivation depuis le Statut StActivation d'un autre Trade (IDT = pIdt)
        /// </summary>        
        /// <param name="pIdt"></param>
        /// <returns></returns>
        public bool InitStActivationFromTrade(string pCS, IDbTransaction pDbTransaction, int pIdt)
        {
            bool ret = false;

            if (pIdt > 0)
            {
                SQL_TradeStSys sqlTemplateStSys = new SQL_TradeStSys(pCS, pIdt)
                {
                    DbTransaction = pDbTransaction
                };
                if (sqlTemplateStSys.IsLoaded)
                {
                    TradeStatus.stActivation.CurrentSt = sqlTemplateStSys.IdStActivation;
                    ret = true;
                }
            }
            else
                TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.REGULAR.ToString();

            return ret;
        }

        /// <summary>
        ///Initialisation des Statuts User depuis les Parties et leur rôle joués
        /// </summary>
        // EG 20180307 [23769] Gestion dbTransaction
        public void InitStUserFromPartiesRole(string pCS, IDbTransaction pDbTransaction)
        {
            ActorRoleCollection tradeActorRole = DataDocument.GetActorRole(pCS, pDbTransaction);
            foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)))
            {
                if (StatusTools.IsStatusUser(statusEnum))
                    m_TradeStatus.InitStUserFromACTORROLE(pCS, pDbTransaction, tradeActorRole, statusEnum);
            }
        }

        /// <summary>
        ///Initialisation des statuts User depuis les statuts User d'un autre Trade
        /// </summary>        
        /// <param name="pIdt"></param>
        /// <returns></returns>
        public void InitStUserFromTrade(string pCS, IDbTransaction pDbTransaction, int pIdt)
        {
            if (pIdt > 0)
            {
                StatusCollection[] stUsers = TradeStatus.stUsers;
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                for (int i = 0; i < stUsers.Length; i++)
                {
                    stUsers[i].InitializeFromTrade(pCS, pDbTransaction, pIdt);
                    EFS.Status.Status[] stList = stUsers[i].Status;
                    for (int j = 0; j < stList.Length; j++)
                    {
                        //stList[j].ExtendEnum = ListEnumsSchemes[stUsers[i].StatusEnum.ToString()];
                        stList[j].ExtendEnum =  DataEnabledEnumHelper.GetDataEnum(pCS, stUsers[i].StatusEnum.ToString());
                    }
                }
            }
        }

        /// <summary>
        ///Initialisation des Statuts User (StCheck / StMatch) depuis ACTIONTUNING
        /// </summary>
        public void InitStUserFromTuning(ActionTuning pActionTuning)
        {
            if ((null != pActionTuning) && pActionTuning.DrSpecified)
            {
                foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)))
                {
                    if (StatusTools.IsStatusUser(statusEnum))
                        m_TradeStatus.InitStUserFromACTIONTUNING(statusEnum, pActionTuning.Dr);
                }
            }
        }

        /// <summary>
        /// Initialise les membres dédiés à une action (Remove,Exercice,CorrectionOfQunatity,etc...)
        /// </summary>
        /// <param name="pMode"></param>
        /// EG 20150515 m_RemoveTrade de type RemoveTradeMsg
        public virtual void InitializeForAction(string pCS, Cst.Capture.ModeEnum pMode)
        {
            if (Cst.Capture.IsModeRemove(pMode))
                m_RemoveTrade = new RemoveTradeMsg(IdT, Identifier, GetDefaultDateAction(pCS, pMode));
        }

        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public virtual void InitializeInvoicedFees(string pCS)
        {
            InitializeInvoicedFees(pCS, IdT);
        }
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public virtual void InitializeInvoicedFees(string pCS, int pIdT)
        {
        }
        /// <summary>
        /// Intervertit tous les payers receivers existants dans le datadocument lorsqu'ils sont les contreparties
        /// <para>Sont exclus les payment "Fee" ou "Brokerage" </para>
        /// </summary>
        /// <exception cref="NotImplementedException : lorsque la fonctionnalité est indisponible sur le produit"></exception> 
        ///FI 20100409 [16939] Modify ReversePartyReference (avant c'était une méthode virtuelle vide de contenu)
        /// EG 20140702 New ReturnSwap
        public void ReversePartyReference()
        {
            //
            ProductContainer product = DataDocument.CurrentProduct;
            if (product.IsFxLeg || product.IsFxSwap || product.IsBulletPayment)
            {
                // Ne rien faire => voir le traitement global sur les payments
            }
            else if (product.IsFxTermDeposit)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsFxOption)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsFra)
            {
                ReverseProductPartyReference(product, true);
            }
            //
            else if (product.IsSwap || product.IsCapFloor)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsLoanDeposit)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsReturnSwap)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsCommoditySpot)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsEQD)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsDebtSecurityTransaction)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsRepo || product.IsBuyAndSellBack)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsExchangeTradedDerivative)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsEquitySecurityTransaction)
            {
                ReverseProductPartyReference(product, true);
            }
            else if (product.IsStrategy)
            {
                ReverseProductPartyReference(product, true);
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("product {0} is not managed, please contact EFS", product.Product.GetType().Name));
            //	
            ArrayList al = ReflectionTools.GetObjectsByType(CurrentTrade, product.ProductBase.TypeofPayment, false);
            if (ArrFunc.IsFilled(al))
            {
                for (int i = al.Count - 1; -1 < i; i--)
                {
                    IPayment payment = (IPayment)al[i];
                    if (payment.PaymentTypeSpecified)
                    {
                        if ("Fee" == payment.PaymentType.Value || "Brokerage" == payment.PaymentType.Value)
                            al.RemoveAt(i);
                    }
                }
            }
            // Traitement Global
            ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
            //
        }

        /// <summary>
        /// Chargement complet d'un trade
        /// <para>Usage adapté pour la saisie</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier">Identifiant du trade</param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser">représente le user connecté sur Spheres®</param>
        /// <param name="pSessionId">Identifiant de la session du user</param>
        /// <returns></returns>
        public bool SearchAndDeserialize(string pCS, string pTradeIdentifier, Cst.Capture.ModeEnum pCaptureMode, User pUser, string pSessionId)
        {
            return SearchAndDeserialize(pCS, null, pTradeIdentifier, SQL_TableWithID.IDType.Identifier, pCaptureMode, pUser, pSessionId);
        }
        /// <summary>
        /// Chargement complet d'un trade
        /// <para>Usage adapté pour la saisie</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId">Identifiant du trade</param>
        /// <param name="pIdType">Type d'identifiant</param>
        /// <param name="pCaptureMode"></param>
        /// <param name="pUser">représente le user connecté sur Spheres®</param>
        /// <param name="pSessionId">Identifiant de la session du user</param>
        /// <returns></returns>
        /// EG 20140731 Refactoring (RptSideContainer)
        /// EG 20150331 [POC] Test Existence Trade (non fongible) dans CashBalance (à une date inférieure à CurrentBusinessDate) pour empêcher la modification totale
        /// FI 20161005 [XXXXX] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        /// EG 20170510 [21153] Add parameter to SetDataDocument (pour ne pas désérialiser le XML) 
        // EG 20180205 [23769] Use dbTransaction  
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (SetDocument with IsTradeFoundWithXML parameter)
        public bool SearchAndDeserialize(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, Cst.Capture.ModeEnum pCaptureMode, User pUser, string pSessionId)
        {
            Clear();

            bool isOk = SearchTrade(pCS, pDbTransaction, pId, pIdType, pUser, pSessionId);

            // EG 20140214 INITIALISATION DES STATUS avant SetDataDocument 
            if (isOk)
                m_TradeStatus.Initialize(pCS, pDbTransaction, m_SQLTrade.Id);

            // EG 20170510 [21153]
            if (isOk)
                SetDataDocument(pCS, pDbTransaction, pCaptureMode, IsTradeFoundWithXML);
            
            if (isOk)
            {
                m_TradeNotification.InitializeFromTrade(pCS, pDbTransaction, m_SQLTrade.Id);
                //FI 20111115 Sur la consultation d'un instrument non géré (CreditDefaultSwap) 
                //Spheres® doit pouvoir afficher un écran Full
                SetTradeNotificationOrder();
            }
            
            if (isOk)
            {
                // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                m_SQLLastTradeLog = new SQL_LastTrade_L(pCS, SQLTrade.Id, 
                    new PermissionEnum[] { PermissionEnum.Create, PermissionEnum.Modify, PermissionEnum.ModifyPostEvts, PermissionEnum.ModifyFeesUninvoiced, PermissionEnum.Remove })
                {
                    DbTransaction = pDbTransaction
                };
                m_SQLLastTradeLog.LoadTable();
            }
            
            if (isOk)
            {
                // 20081209 RD 16414
                if ((this.SQLProduct.GProduct == Cst.ProductGProduct_RISK) || (this.SQLProduct.GProduct == Cst.ProductGProduct_ADM))
                    m_SQLTradeLink = new SQL_TradeLink(pCS, SQLTrade.Id, this.SQLProduct.GProduct);
                else
                    m_SQLTradeLink = new SQL_TradeLink(pCS, SQLTrade.Id);

                m_SQLTradeLink.DbTransaction = pDbTransaction;

                // EG 20120620 ADD GPRODUCT pour IDT_A et IDT_B
                // EG 20150716 ADD IDSTACTIVATION for IDT_A et IDT_B
                m_SQLTradeLink.LoadTable(new string[]{"tl.ACTION as ACTION_A","tl.IDT","tl.IDA","tl.DTSYS",
                                                          "IDT_A","t_A.IDENTIFIER as IDENTIFIER_A", "p_A.GPRODUCT as GPRODUCT_A", "t_A.IDSTACTIVATION as IDSTACTIVATION_A", 
                                                          "IDT_B","t_B.IDENTIFIER as IDENTIFIER_B", "p_B.GPRODUCT as GPRODUCT_B", "t_B.IDSTACTIVATION as IDSTACTIVATION_B",
                                                          "LINK","MESSAGE","DATA1","DATA1IDENT","DATA2","DATA2IDENT","DATA3","DATA3IDENT"});
            }
            
            if (isOk)
            {
                if (false == m_TradeStatus.IsStEnvironment_Template)
                {

                    IsTradeRemoved = TradeRDBMSTools.IsTradeRemove(pCS, pDbTransaction, m_SQLTrade.IdT);

                    IsEventsGenerated = TradeRDBMSTools.IsEventExist(pCS, pDbTransaction, m_SQLTrade.IdT, EventCodeFunc.Trade);

                    // FI 20170116 [21916] property IsEventsPosAction abandonnée car n'est plus utilisée
                    //IsEventsPosAction = false; 
                    IsExistCashBalance = false;
                    IsAllocationFromPreviousBusinessDate = false;
                    AvailableQuantity = 0;
                    ClearingBusinessDate = DateTime.MinValue;
                    CurrentBusinessDate = DateTime.MinValue;

                    if (IsTradeFoundAndAllocation)
                    {
                        // FI 20121113 [18224] alimentation de CurrentBusinessDate
                        // EG 20180301 [23769] Use dbTransaction  
                        CurrentBusinessDate = Tools.GetDateBusiness(CSTools.SetCacheOn(pCS), pDbTransaction, this.DataDocument);
                        //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)Product.product, DataDocument);
                        //ClearingBusinessDate = etd.ClearingBusinessDate;
                        //FI 20170116 [21916] call Product.RptSide() => l'élément RptSide est déjà valorisé via l'appel à SetDataDocument 
                        //RptSideProductContainer _rptSideContainer = Product.RptSide(pCS, IsAllocation);
                        RptSideProductContainer _rptSideContainer = Product.RptSide();
                        // FI 20161005 [XXXXX] Add  NotImplementedException
                        if (null == _rptSideContainer)
                            throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", Product.ProductBase.ToString()));

                        ClearingBusinessDate = _rptSideContainer.ClearingBusinessDate;
                        if (DtFunc.IsDateTimeFilled(CurrentBusinessDate))
                        {
                            // FI 20170116 [21916] property IsEventsPosAction est abandonné car jamais utilisé 
                            // IsEventsPosAction = TradeRDBMSTools.IsExistPosAction(pCS, m_SQLTrade.IdT, CurrentBusinessDate);

                            // EG 20151130 [20979] 
                            // EG 20180307 [23769] Gestion dbTransaction
                            AvailableQuantity = PosKeepingTools.GetAvailableQuantity(pCS, pDbTransaction, CurrentBusinessDate, m_SQLTrade.IdT);

                            // FI 20161214 [21916] Valorisation IsExistCashBalance
                            // EG 20180307 [23769] Gestion dbTransaction
                            IsExistCashBalance = TradeRDBMSTools.IsExistInCashBalance(pCS, pDbTransaction, m_SQLTrade.IdT, CurrentBusinessDate);

                            //PL 20130809 New
                            IsAllocationFromPreviousBusinessDate = (ClearingBusinessDate < CurrentBusinessDate);

                            //PL20200117 [25099] New
                            if (IsEventsGenerated && this.DataDocument.OtherPartyPaymentSpecified)
                            {
                                if (this.DataDocument.OtherPartyPayment[0].Efs_Payment == null)
                                {
                                    PaymentTools.CalcPayments(pCS, pDbTransaction, this.DataDocument.CurrentProduct.Product,
                                        this.DataDocument.OtherPartyPayment, this.DataDocument);
                                }
                                                                
                                bool existsPayment_OnFeeScopeOrderId = false;
                                bool existsPayment_OnFeeScopeFolderId = false;
                                if (PaymentTools.IsExistsPayment_OnFeeScopeOrderIdOrFolderId_WithMinMax(this.DataDocument.OtherPartyPayment, ref existsPayment_OnFeeScopeOrderId, ref existsPayment_OnFeeScopeFolderId))
                                {
                                    IsExistOPP_OnFeeScopeOrderIdOrFolderId_WithMinMax = (existsPayment_OnFeeScopeOrderId || existsPayment_OnFeeScopeFolderId);
                                }
                            }
                        }
                    }
                }

                if (IsEventsGenerated)
                {
                    Cst.ProcessTypeEnum[] lstProcess = new Cst.ProcessTypeEnum[] 
                    { 
                        Cst.ProcessTypeEnum.ACCOUNTGEN, 
                        Cst.ProcessTypeEnum.CMGEN, 
                        Cst.ProcessTypeEnum.EARGEN, 
                        Cst.ProcessTypeEnum.ESRGEN, 
                        Cst.ProcessTypeEnum.ESRNETGEN, 
                        Cst.ProcessTypeEnum.ESRSTDGEN, 
                        Cst.ProcessTypeEnum.RIMGEN,  
                    };
                    m_EventProcessInSucces = TradeRDBMSTools.GetListEventProcess(pCS, pDbTransaction, m_SQLTrade.IdT, lstProcess, null, ProcessStateTools.StatusSuccessEnum);
                }
            }

            if (isOk)
                EntityOnLoad = DataDocument.GetFirstEntity(CSTools.SetCacheOn(pCS), pDbTransaction);

            return isOk;
        }
        /// <summary>
        /// Chargement à minima d'un trade
        /// <para>Charge m_SQLTrade, m_identification , m_SQLInstrument et m_SQLProduct si le trade existe</para>
        /// <para>Charge m_DataDocument</para>
        /// <para>Charge m_TradeStatus (les statuts du trade)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        /// EG 20170510 [21153] Add parameter pIsMustBeDeserialized
        public bool SearchAndDeserializeShortForm(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            return SearchAndDeserializeShortForm(pCS, pDbTransaction, pId, pIdType, pUser, pSessionId, true);
        }
        // EG 20180307 [23769] Gestion dbTransaction
        public bool SearchAndDeserializeShortForm(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId, bool pIsMustBeDeserialized)
        {
            Clear();

            bool isOk = SearchTrade(pCS, pDbTransaction, pId, pIdType, pUser, pSessionId);
            if (isOk)
            {

                m_TradeStatus.Initialize(pCS, pDbTransaction, m_SQLTrade.IdT);
                // EG 20170510 [21153]
                SetDataDocument(pCS, pDbTransaction, Cst.Capture.ModeEnum.Consult, pIsMustBeDeserialized);
            }
            return isOk;
        }

        /// <summary>
        /// Reset des statuts User: NewValue from CurrentValue
        /// </summary>
        public void ResetStUser()
        {
            ResetStUser(true);
        }

        /// <summary>
        /// Reset des statuts User
        /// <para>pIsFromCurrent = true : NewValue from CurrentValue</para>
        /// <para>pIsFromCurrent = false : NewValue = 0</para>
        /// </summary>
        /// <param name="pIsFromCurrent"></param>
        public void ResetStUser(bool pIsFromCurrent)
        {
            foreach (StatusEnum statusEnum in Enum.GetValues(typeof(StatusEnum)))
            {
                if (StatusTools.IsStatusUser(statusEnum))
                {
                    if (pIsFromCurrent)
                        m_TradeStatus.ResetStUserNewValueFromCurrent(statusEnum);
                    else
                        m_TradeStatus.ResetStUserNewValue(statusEnum);
                }
            }
        }

        /// <summary>
        /// Intervertit tous les payers receivers existants dans le product {pProduct} lorsqu'ils sont les contreparties
        /// </summary>
        /// <param name="pProduct"></param>
        /// <param name="pIsExcludePayment">si true le traitement ne traite pas les objets qui implemente IPayment</param>
        /// <exception cref="NotImplementedException lorsque le produit n'est pas géré"></exception>
        ///FI 20100409 [16939] Add ReverseProductPartyReference
        /// EG 20140702 New ReturnSwap (ReverseSide)
        /// EG 20150331 [POC] FxLeg|FxOptionLeg
        /// FI 20161214 [21916] Modify
        protected void ReverseProductPartyReference(ProductContainer pProduct, bool pIsExceptIPayment)
        {
            ProductContainer productItem = pProduct;
            if (productItem.IsFxLeg || productItem.IsFxSwap || productItem.IsBulletPayment)
            {
                if (pIsExceptIPayment)
                {
                    //Il n'y a rien à faire puisque ces produits sont constitués de payment
                }
                else
                {
                    //FI 20100409 A faire si un jour 
                    throw new NotImplementedException(StrFunc.AppendFormat("product {0} is not managed, please contact EFS", productItem.Product.GetType().Name));
                }
            }
            ArrayList al;
            RptSideProductContainer _rptSide;
            //
            if (productItem.IsFxTermDeposit)
            {
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "initialPayerReference", "initialReceiverReference");
            }
            else if (productItem.IsFxOption)
            {
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
                //
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofFxOptionPremium, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
            }
            else if (productItem.IsFra)
            {
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
            }
            else if (productItem.IsSwap || productItem.IsCapFloor)
            {
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofStream, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
            }
            else if (productItem.IsLoanDeposit)
            {
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofLoadDepositStream, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
            }
            else if (productItem.IsEQD)
            {
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
                //
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofFxOptionPremium, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
            }
            else if (productItem.IsDebtSecurityTransaction)
            {
                IDebtSecurityTransaction debtSecurityTransaction = (IDebtSecurityTransaction)productItem.Product;
                al = new ArrayList
                {
                    debtSecurityTransaction
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
                //
                // le receiver du stream est l'acheteur, le payer du stream est l'émetteur (il ne bouge pas)
                if (debtSecurityTransaction.SecurityAssetSpecified && debtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
                {
                    for (int j = 0; j < ArrFunc.Count(debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream); j++)
                        debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream[j].ReceiverPartyReference.HRef = debtSecurityTransaction.BuyerPartyReference.HRef;
                }

                _rptSide = new DebtSecurityTransactionContainer((IDebtSecurityTransaction)productItem.Product, DataDocument);
                // FI 20161214 [21916] le RptSide est déjà présent, via SearchAndDeserialize  => Non nécessaire de faire appel à  InitRptSide
                //_rptSide.InitRptSide(CS, IsAllocation);
                _rptSide.ReverseSide();
            }
            else if (productItem.IsRepo || productItem.IsBuyAndSellBack)
            {
                ISaleAndRepurchaseAgreement saleAndRepurchaseAgreement = (ISaleAndRepurchaseAgreement)productItem.Product;
                //
                //CashStream
                al = new ArrayList();
                for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.CashStream); i++)
                    al.Add(saleAndRepurchaseAgreement.CashStream);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
                //
                //Spot
                al = new ArrayList();
                for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.SpotLeg); i++)
                {
                    IDebtSecurityTransaction debtSecurityTransaction = saleAndRepurchaseAgreement.SpotLeg[i].DebtSecurityTransaction;
                    al.Add(debtSecurityTransaction);
                    //
                    if (debtSecurityTransaction.SecurityAssetSpecified && debtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
                    {
                        for (int j = 0; j < ArrFunc.Count(debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream); j++)
                            debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream[j].ReceiverPartyReference.HRef = debtSecurityTransaction.BuyerPartyReference.HRef;
                    }
                }
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
                //
                //Forward
                al = new ArrayList();
                if (saleAndRepurchaseAgreement.ForwardLegSpecified)
                {
                    for (int i = 0; i < ArrFunc.Count(saleAndRepurchaseAgreement.ForwardLeg); i++)
                    {
                        IDebtSecurityTransaction debtSecurityTransaction = saleAndRepurchaseAgreement.ForwardLeg[i].DebtSecurityTransaction;
                        al.Add(debtSecurityTransaction);
                        //
                        if (debtSecurityTransaction.SecurityAssetSpecified && debtSecurityTransaction.SecurityAsset.DebtSecuritySpecified)
                        {
                            for (int j = 0; j < ArrFunc.Count(debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream); j++)
                                debtSecurityTransaction.SecurityAsset.DebtSecurity.Stream[j].ReceiverPartyReference.HRef = debtSecurityTransaction.BuyerPartyReference.HRef;
                        }
                    }
                }
                if (ArrFunc.IsFilled(al))
                    ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
            }
            else if (productItem.IsExchangeTradedDerivative)
            {
                #region ExchangeTradedDerivative
                _rptSide = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)productItem.Product, DataDocument);
                _rptSide.ReverseSide();
                #endregion ExchangeTradedDerivative
            }
            else if (productItem.IsReturnSwap)
            {
                #region ReturnSwap
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");
                //
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofReturnLeg, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");
                //
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofInterestLeg, false);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");

                _rptSide = new ReturnSwapContainer((IReturnSwap)productItem.Product, DataDocument);
                // FI 20161214 [21916] le RptSide est déjà présent, via SearchAndDeserialize  => Non nécessaire de faire appel à  InitRptSide
                //_rptSide.InitRptSide(CS, IsAllocation);
                _rptSide.ReverseSide();
                #endregion ReturnSwap
            }
            else if (productItem.IsCommoditySpot)
            {
                #region CommoditySpot
                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofFinancialLeg, true);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");

                al = ReflectionTools.GetObjectsByType(productItem.Product, productItem.ProductBase.TypeofPhysicalLeg, true);
                ReverseObjectsPartyReference(al, "payerPartyReference", "receiverPartyReference");

                _rptSide = new CommoditySpotContainer((ICommoditySpot)productItem.Product, DataDocument);
                _rptSide.ReverseSide();
                #endregion CommoditySpot
            }
            // EG 20150331 (POC] FxLeg
            else if (productItem.IsFxLeg)
            {
                #region FxLeg
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");

                _rptSide = new FxLegContainer((IFxLeg)productItem.Product, DataDocument);
                // FI 20161214 [21916] le RptSide est déjà présent, via SearchAndDeserialize  => Non nécessaire de faire appel à  InitRptSide
                //_rptSide.InitRptSide(CS, IsAllocation);
                _rptSide.ReverseSide();
                #endregion FxLeg
            }
            // EG 20150331 [POC] FxOptionLeg
            else if (productItem.IsFxSimpleOption)
            {
                al = new ArrayList
                {
                    productItem.Product
                };
                ReverseObjectsPartyReference(al, "buyerPartyReference", "sellerPartyReference");

                _rptSide = new FxOptionLegContainer((IFxOptionLeg)productItem.Product, DataDocument);
                // FI 20161214 [21916] le RptSide est déjà présent, via SearchAndDeserialize => Non nécessaire de faire appel à  InitRptSide
                //_rptSide.InitRptSide(CS, IsAllocation);
                _rptSide.ReverseSide();

            }
            else if (productItem.IsEquitySecurityTransaction)
            {
                _rptSide = new EquitySecurityTransactionContainer((IEquitySecurityTransaction)(productItem.Product));
                _rptSide.ReverseSide();
            }
            else if (productItem.IsStrategy)
            {
                StrategyContainer strategy = new StrategyContainer((IStrategy)productItem.Product);
                ProductContainer[] subProduct = strategy.GetSubProduct();
                for (int i = 0; i < ArrFunc.Count(subProduct); i++)
                {
                    ReverseProductPartyReference(subProduct[i], pIsExceptIPayment);
                }
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("product {0} is not managed, please contact EFS", productItem.Product.GetType().Name));
        }

        /// <summary>
        ///  Met à jour les indicateurs de confirmation sur les 2 conterparties avec {pValue} 
        /// </summary>
        /// <param name="pValue">Si true les 2 contreparties reçoivent les messages init,int et ter, si False c'est le contraire</param>
        public void SetTradeNotification(bool pValue)
        {
            int i = -1;
            foreach (IParty party in DataDocument.Party)
            {
                if (DataDocument.IsPartyCounterParty(party))
                {
                    i++;
                    TradeNotification.PartyNotification[i].SetConfirmation(pValue);
                    TradeNotification.PartyNotification[i].IdActor = party.OTCmlId;
                }
            }
        }

        /// <summary>
        /// Intervertit tous les payers receivers existants dans les objets présents dans {pListObject}
        /// <para>Reverse uniquement si les acteurs sont Buyer ou seller du trade</para>
        /// </summary>
        /// <param name="pListObject"></param>
        /// <param name="pElementName1">Membre payer/buyer des objects</param>
        /// <param name="pElementName2">Membre raceiver/seller des objects</param>
        /// FI 20100409 [16939] Add ReverseObjectsPartyReference
        /// FI 20170116 [21916] Modify
        private void ReverseObjectsPartyReference(ArrayList pListObject, string pElementName1, string pElementName2)
        {
            if (ArrFunc.IsFilled(pListObject))
            {
                for (int i = 0; i < ArrFunc.Count(pListObject); i++)
                {
                    // FI 20170116 [21916] Utilisation de la méthode GetObjectByNameSorted 
                    //=> il se peut que le pElementName1/pElementName2 soient issus d'une clase de base (exemple : EfsML.v30.CommodityDerivative.FinancialSwapLeg)  
                    //IReference partyRef1 = (IReference)ReflectionTools.GetObjectByName(pListObject[i], pElementName1, true)[0];
                    //IReference partyRef2 = (IReference)ReflectionTools.GetObjectByName(pListObject[i], pElementName2, true)[0];

                    IReference partyRef1 = (IReference)ReflectionTools.GetObjectByNameSorted(pListObject[i], pElementName1, true)[0];
                    IReference partyRef2 = (IReference)ReflectionTools.GetObjectByNameSorted(pListObject[i], pElementName2, true)[0];
                    
                    IParty party1 = DataDocument.GetParty(partyRef1.HRef);
                    IParty party2 = DataDocument.GetParty(partyRef2.HRef);
                    bool isReverse = DataDocument.IsPartyBuyer(party1) || DataDocument.IsPartySeller(party1);
                    isReverse = isReverse && DataDocument.IsPartyBuyer(party2) || DataDocument.IsPartySeller(party2);
                    
                    if (isReverse)
                    {
                        string tmp = partyRef1.HRef;
                        partyRef1.HRef = partyRef2.HRef;
                        partyRef2.HRef = tmp;
                    }
                }
            }
        }

        /// <summary>
        /// Retourne true si le trade est accessible, ds ce cas il existe et l'utilisateur possède les droits nécessaires 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeIdentifier"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        protected virtual bool CheckIsTradeFound(string pCS, string pTradeIdentifier, User pUser, string pSessionId)
        {
            return CheckIsTradeFound(pCS, null, pTradeIdentifier, SQL_TableWithID.IDType.Identifier, pUser, pSessionId);
        }

        /// <summary>
        /// Retourne true si le trade est accessible, ds ce cas il existe et l'utilisateur possède les droits nécessaires 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        /// <returns></returns>
        // EG 20180205 [23769] Use dbTransaction  
        protected virtual bool CheckIsTradeFound(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
            bool isTemplate = TradeRDBMSTools.IsTradeTemplate(pCS, pDbTransaction, pId, pIdType);
            if (isTemplate)
                stEnv = Cst.StatusEnvironment.TEMPLATE;

            string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);

            SQL_TradeCommon sqlTrade = new SQL_TradeCommon(pCS, pIdType, sqlTradeId,
                                    stEnv, SQL_Table.RestrictEnum.Yes,
                                    pUser, pSessionId, m_SQLTrade.RestrictProduct)
            {
                DbTransaction = pDbTransaction
            };
            bool ret = sqlTrade.IsFound;
            return ret;
        }

        /// <summary>
        /// Alimente les items de TradeNotification dans le même ordre que les parties
        /// <para>
        /// la 1er partie contrepartie (en générale BUYER or SELLER) alimente TradeNotification.partyNotification[0]
        /// </para>
        /// <para>
        /// la 2nd partie contrepartie (en général BUYER or SELLER) alimente TradeNotification.partyNotification[1]
        /// </para>
        /// </summary>
        /// FI 20170907 [XXXXX] Modify
        private void SetTradeNotificationOrder()
        {
            ArrayList al = new ArrayList();

            if (ArrFunc.IsFilled(DataDocument.Party)) // FI 20170907 [XXXXX] add If
            {
                foreach (IParty party in DataDocument.Party)
                {
                    if (DataDocument.IsPartyCounterParty(party))
                    {
                        ActorNotification actorNotification = m_TradeNotification.GetActorNotification(party.OTCmlId);
                        if (null != actorNotification)
                            al.Add(actorNotification);
                    }
                }
            }

            if (ArrFunc.Count(al) != 2)
            {
                if (ArrFunc.Count(al) == 0)
                    al.Add(new ActorNotification(true));
                if (ArrFunc.Count(al) == 1)
                    al.Add(new ActorNotification((false == IsAllocation)));
            }

            m_TradeNotification.PartyNotification = (ActorNotification[])al.ToArray(typeof(ActorNotification));
        }

        /// <summary>
        /// Recherche du trade 
        /// <para>Retourne true si le trade existe</para>
        /// <para>Charge m_SQLTrade, m_identification , m_SQLInstrument et m_SQLProduct si le trade existe</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pId"></param>
        /// <param name="pIdType"></param>
        /// <param name="pUser"></param>
        /// <param name="pSessionId"></param>
        // EG 20171025 [23509] Add DTTIMESTAMP, DTEXECUTION, DTORDERENTERED, TZFACILITY
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private bool SearchTrade(string pCS, IDbTransaction pDbTransaction, string pId, SQL_TableWithID.IDType pIdType, User pUser, string pSessionId)
        {
            InitializeSqlTrade(pCS, pDbTransaction, string.Empty, pIdType);

            bool isOk = CheckIsTradeFound(pCS, pDbTransaction, pId, pIdType, pUser, pSessionId);

            if (isOk)
            {
                string sqlTradeId = pId.Replace("%", SQL_TableWithID.StringForPERCENT);
                InitializeSqlTrade(pCS, pDbTransaction, sqlTradeId, pIdType);
                m_SQLTrade.IsAddRowVersion = true;
                m_SQLTrade.IsWithTradeXML = true;

                isOk = m_SQLTrade.LoadTable(new string[]{"TRADE.IDT","TRADE.IDI","TRADE.IDENTIFIER","TRADE.DISPLAYNAME","TRADE.DESCRIPTION","TRADE.DTTRADE",
                    "TRADE.DTTIMESTAMP","TRADE.DTEXECUTION","TRADE.DTORDERENTERED","TRADE.TZFACILITY", "TRADE.DTBUSINESS", "TRADE.IDT_SOURCE","TRADE.IDT_TEMPLATE","TRADE.SOURCE","TRADE.DTSYS",
                    "TRADE.IDSTENVIRONMENT","TRADE.DTSTENVIRONMENT","TRADE.IDASTENVIRONMENT",
                    "TRADE.IDSTBUSINESS","TRADE.DTSTBUSINESS","TRADE.IDASTBUSINESS",
                    "TRADE.IDSTACTIVATION","TRADE.DTSTACTIVATION","TRADE.IDASTACTIVATION",
                    "TRADE.IDSTPRIORITY","TRADE.DTSTPRIORITY","TRADE.IDASTPRIORITY",
                    "TRADE.IDSTUSEDBY","TRADE.DTSTUSEDBY","TRADE.IDASTUSEDBY","TRADE.LIBSTUSEDBY",
                    "TRADE.IDM","TRADE.IDM_FACILITY","TRADE.ASSETCATEGORY", "TRADE.IDASSET",
                    "TRADE.SIDE","TRADE.QTY", "TRADE.UNITQTY","TRADE.PRICE","TRADE.UNITPRICE", "TRADE.TYPEPRICE",
                    "TRADE.STRIKEPRICE","TRADE.UNITSTRIKEPRICE", "TRADE.ACCINTRATE",
                    "TRADE.EXECUTIONID","TRADE.TRDTYPE", "TRADE.TRDSUBTYPE","TRADE.SECONDARYTRDTYPE","TRADE.ORDERID", "TRADE.ORDERTYPE",
                    "TRADE.IDA_DEALER","TRADE.IDB_DEALER", "TRADE.IDA_CLEARER","TRADE.IDB_CLEARER",
                    "TRADE.IDA_BUYER", "TRADE.IDB_BUYER","TRADE.IDA_SELLER","TRADE.IDB_SELLER",
                    "TRADE.IDA_RISK", "TRADE.IDB_RISK","TRADE.IDA_ENTITY","TRADE.IDA_CSSCUSTODIAN",
                    "TRADE.DTINUNADJ", "TRADE.DTINADJ","TRADE.DTOUTUNADJ","TRADE.DTOUTADJ",
                    "TRADE.DTSETTLT", "TRADE.DTDLVYSTART","TRADE.DTDLVYEND","TRADE.TZDLVY",
                    "TRADE.POSITIONEFFECT", "TRADE.RELTDPOSID","TRADE.INPUTSOURCE",
                    "trx.TRADEXML","trx.EFSMLVERSION","TRADE.EXTLLINK","TRADE.ROWATTRIBUT"});
            }

            if (isOk)
            {
                m_identification = new SpheresIdentification
                {
                    OTCmlId = m_SQLTrade.Id,
                    Identifier = m_SQLTrade.Identifier,
                    Displayname = m_SQLTrade.DisplayName,
                    Description = m_SQLTrade.Description,
                    Extllink = m_SQLTrade.ExtlLink
                };
            }

            if (isOk)
            {
                m_SQLInstrument = new SQL_Instrument(CSTools.SetCacheOn(pCS), m_SQLTrade.IdI)
                {
                    DbTransaction = pDbTransaction
                };
                isOk = m_SQLInstrument.IsLoaded;
            }

            if (isOk)
            {
                m_SQLProduct = new SQL_Product(CSTools.SetCacheOn(pCS), m_SQLInstrument.IdP)
                {
                    DbTransaction = pDbTransaction
                };
                isOk = m_SQLProduct.IsLoaded;
            }

            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsUpdProductId"></param>
        /// FI 20140206 [19564] Modification => Recherche des UTI présents dans la table TRADEID
        /// EG 20140702 New ReturnSwap (ReverseSide)
        /// FI 20140902 [XXXXX] Modification de la signature de la méthode
        /// FI 20141217 [20574] Modify
        /// EG 20150331 (POC] FxLeg|FxOptionLeg
        /// FI 20161214 [21916] Modify
        /// EG 20170510 [21153] Add parameter pIsMustBeDeserialized
        /// EG 20171016 [23509] Use dbTransaction  
        // EG 20180307 [23769] Gestion dbTransaction
        private void SetDataDocument(string pCS, IDbTransaction pDbTransaction, Cst.Capture.ModeEnum pCaptureMode, bool pIsMustBeDeserialized)
        {
            if (pIsMustBeDeserialized)
            {
                EFS_SerializeInfo serializerInfo = new EFS_SerializeInfo(m_SQLTrade.TradeXml);
                IDataDocument dataDoc = (IDataDocument)CacheSerializer.Deserialize(serializerInfo);

                if ((false == (ArrFunc.Count(dataDoc.Trade) > 0)))
                    throw new Exception(StrFunc.AppendFormat(@"Error on deserialize trade ""{0}"", trade is null", m_SQLTrade.Identifier));
                if (null == dataDoc.FirstTrade.Product)
                    throw new Exception(StrFunc.AppendFormat(@"Error on deserialize trade ""{0}"", product is null", m_SQLTrade.Identifier));

                m_DataDocument = new DataDocumentContainer(dataDoc);
            }
            //
            //Alimentation des données non serializable de l'interface ISecurityAsset
            ISecurityAsset[] secAsset = m_DataDocument.GetSecurityAsset();
            if (ArrFunc.IsFilled(secAsset))
            {
                foreach (ISecurityAsset secAssetItem in secAsset)
                {
                    SecurityAssetContainer securityAsset = new SecurityAssetContainer(secAssetItem);
                    securityAsset.SetIssuer(m_DataDocument);
                    securityAsset.IsNewAssetSpecified = false;
                    securityAsset.IdTTemplateSpecified = false; //impossible de déterminer le template à partir du dataDocument
                }
            }
            if (m_DataDocument.CurrentProduct.IsSwap)
            {
                //Alimentation des données non serializable de l'interface ISWAP
                SwapContainer swap = new SwapContainer((ISwap)m_DataDocument.CurrentProduct.Product, m_DataDocument);
                swap.SetFlagSynchronous();
            }
            else if (m_DataDocument.CurrentProduct.IsSwaption)
            {
                //Alimentation des données non serializable de l'interface ISWAP
                ISwaption swaption = (ISwaption)m_DataDocument.CurrentProduct.Product;
                SwapContainer swap = new SwapContainer(swaption.Swap, m_DataDocument);
                swap.SetFlagSynchronous();
            }

            // FI 20161214 [21916] Appel à currentProduct.RptSide(CS, IsAllocation) en remplacement de la liste des if 
            // Alimentation de l'élément RptSide sur les produits où cet élément n'est pas serializé
            // EG 20180307 [23769] Gestion dbTransaction
            RptSideProductContainer rptSideContainer = m_DataDocument.CurrentProduct.RptSide(pCS, pDbTransaction, IsAllocation);
            if (null != rptSideContainer)
            {
                if (Cst.Capture.IsModeNewCapture(pCaptureMode) || Cst.Capture.IsModeUpdate(pCaptureMode))
                    InitPosEffect(rptSideContainer); // // FI 20161214 [21916] InitPosEffect est du coup appelé systématiquement
            }

            //Cas particulier où otcmlId n'est pas renseigné (Par Ex Trade simul sur Oracle ) => Ds ce cas alimentation de otcmlId 
            bool isUpdProductId = (0 == Product.ProductBase.ProductType.OTCmlId || StrFunc.IsEmpty(Product.ProductBase.Id));
            //
            //Par ex si chgt de template, le template peut avoir un element product incohérent (OTCmId différent de l’instrument courant)
            //Voir paramétrage : création des instruments (Les templates peuvent être issus d'autres instruments)
            isUpdProductId |= (pCaptureMode != Cst.Capture.ModeEnum.Consult);
            //
            if (isUpdProductId)
                Tools.SetProduct(CSTools.SetCacheOn(pCS), pDbTransaction, Product, SQLInstrument.Id);


            if (IsTradeFoundAndAllocation && (false == m_TradeStatus.IsStEnvironment_Template))
                // FI 20141217 [20574] Appel à UpdateMissingAllocationUTI
                DataDocument.UpdateMissingAllocationUTI(pCS, pDbTransaction, m_SQLTrade.IdT);

            // EG 20171025 [23509] Mise en conformité du dataDocument (Facility, ExecutionDateTime , etc.)
            // EG 20240531 [WI926] Add Parameter pIsTemplate
            if (IsTradeFound && (false == Cst.Capture.IsModeNew(pCaptureMode)))
                DataDocument.UpdateMissingTimestampAndFacility(pCS, pDbTransaction, m_SQLTrade, TradeStatus.IsStEnvironment_Template);

        }

        /// <summary>
        /// Retourne true si le trade est un trade jour
        /// </summary>
        /// EG 20140731 Refactoring (Gestion ReturnSwap et EquitySecurityTransaction)
        /// FI 20161005 [XXXXX] Modify
        /// FI 20170116 [21916] Modify
        public bool IsAllocEntry(string pCS, IDbTransaction pDbTransaction)
        {
            bool _isAllocEntry = false;
            if (IsTradeFoundAndAllocation)
            {
                DateTime dtBusiness = Tools.GetDateBusiness(CSTools.SetCacheOn(pCS), pDbTransaction, this.DataDocument);
                // FI 20140328 [19793] ne plus utiliser le constructeur avec pCS car cela génère des requête abusivement
                //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer(CS, (IExchangeTradedDerivative)Product.product);
                //ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer((IExchangeTradedDerivative)Product.product, DataDocument);
                //_isAllocEntry = (etd.ClearingBusinessDate.Date == dtBusiness.Date);

                // FI 20170116 [21916] Modify call Product.RptSide()
                //RptSideProductContainer rptSideContainer = Product.RptSide(CS, IsAllocation);
                RptSideProductContainer rptSideContainer = Product.RptSide();
                // FI 20161005 [XXXXX] Add  NotImplementedException
                if (null == rptSideContainer)
                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented ", Product.ProductBase.ToString()));

                _isAllocEntry = (rptSideContainer.ClearingBusinessDate.Date == dtBusiness.Date);
            }
            return _isAllocEntry;
        }



        /// <summary>
        /// Mise à jour des position Effect sur RptSide en fonction de SQLInstrument.FungibilityMode
        /// </summary>
        /// <param name="pRptSideProductContainer"></param>
        private void InitPosEffect(RptSideProductContainer pRptSideProductContainer)
        {
            if (ArrFunc.IsFilled(pRptSideProductContainer.RptSide))
            {
                foreach (IFixTrdCapRptSideGrp item in pRptSideProductContainer.RptSide)
                {
                    switch (SQLInstrument.FungibilityMode)
                    {
                        case FungibilityModeEnum.OPENCLOSE:
                            // OK RAS
                            break;
                        case FungibilityModeEnum.CLOSE:
                            // Si PositionEffect est renseigné il est écrasé par close
                            // Permet de placer Close lorsque les écrans n'affichent pas cette donnée
                            if (item.PositionEffectSpecified)
                                item.PositionEffect = PositionEffectEnum.Close;
                            break;
                        case FungibilityModeEnum.NONE:
                            // Si PositionEffect est renseigné il est supprimé
                            item.PositionEffect = default;
                            item.PositionEffectSpecified = false;
                            break;
                    }
                }
            }
        }


        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public class TradeCommonInputGUI : InputGUI
    {
        #region Members
        private string m_TemplateIdentifier;
        private int m_TemplateIdT;
        protected ActorRoleCollection m_ActorRole;
        protected int m_IdP;
        protected int m_IdI;
        #endregion Members

        #region Accessors
        /// <summary>
        ///  Retourne l'identifiant du regroupement des produits 
        /// </summary>
        public virtual Cst.SQLCookieGrpElement GrpElement
        {
            get { return Cst.SQLCookieGrpElement.DBNull; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string TemplateIdentifier
        {
            get { return m_TemplateIdentifier; }
            set { m_TemplateIdentifier = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int TemplateIdT
        {
            get { return m_TemplateIdT; }
            set { m_TemplateIdT = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ActorRoleCollection ActorRole
        {
            get { return m_ActorRole; }
            set { m_ActorRole = value; }
        }

        /// <summary>
        /// Par défaut Obteint  "trade" quelque soit la culture
        /// <para>Doit être overridé</para>
        /// </summary>
        public virtual string MainRessource
        {
            get { return "trade"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdI
        {
            set { m_IdI = value; }
            get { return m_IdI; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int IdP
        {
            set { m_IdP = value; }
            get { return m_IdP; }
        }
        #endregion Accessors

        #region Constructors
        public TradeCommonInputGUI(string pIdMenu, User pUSer, string pXMLFilePath)
            : base(pUSer, pIdMenu, pXMLFilePath)
        {

        }
        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initialisation de IDP,IDI,TEMPLATE,SCREEN par défaut
        /// <para>Lecture de COOKIE ou de paramétrage des défauts sur INSTRUMENT (INSTRUMENTGUI)</para>
        /// </summary>
        public void Initialize()
        {

            bool IsInitFromCookies = true;
            if (IdI > 0)
            {
                //Si IDI est spécifié, on considère élements définis sur le COOKIE uniquement s'il se rapporte à cet IDI
                int idITmp = 0;
                AspTools.ReadSQLCookie(GrpElement, Cst.EnumElement.Instrument.ToString(), out string cookieValue);
                if (StrFunc.IsFilled(cookieValue))
                    idITmp = int.Parse(cookieValue);
                
                IsInitFromCookies = (IdI == idITmp);
            }
            // Recherche du Template et du screen dans COOKIE
            // COOKIE contient la dernière sélection de l'utilisateur
            if (IsInitFromCookies)
                InitFromCookies();
            //
            //Lorsque la lecture du COOKIE n'aboutie pas, on effectue une recherche par défaut
            if (StrFunc.IsEmpty(m_TemplateIdentifier))
                InitFirstTemplateUsedAsDefault();

        }

        /// <summary>
        /// Query qui permet de récupérer un instrument accessible pour lequel il existe une template, un screen 
        /// </summary>
        /// <returns></returns>
        protected QueryParameters GetQueryDefaultScreenAndTemplate()
        {

            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(new DataParameter(SessionTools.CS, "Full", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Cst.Capture.GUIType.Full.ToString());
            //
            StrBuilder sqlQuery = new StrBuilder(SQLCst.SELECT);
            sqlQuery += "p.IDENTIFIER as Product, p.IDP as IDProduct, i.IDI as IDInstrument," + Cst.CrLf;
            sqlQuery += "t.IDKEY as IDT_Template,t.IDENTIFIER as IDENTIFIER_Template, ig.SCREENNAME as IDScreen" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i" + Cst.CrLf;
            sqlQuery += OTCmlHelper.GetSQLJoin(SessionTools.CS, Cst.OTCml_TBL.PRODUCT, true, "i.IDP", "p") + Cst.CrLf;
            sqlQuery += SQLCst.AND + GetSQLRestrictProduct("p");

            if (false == SessionTools.IsSessionSysAdmin)
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(this.User, SessionTools.SessionID, true);
                sqlQuery += srh.GetSQLInstr(string.Empty, "i.IDI") + Cst.CrLf;
                srh.SetParameter(SessionTools.CS, sqlQuery.ToString(), dataParameters);
            }

            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.VW_TRADE_INSTR_TEMPLATE.ToString() + " t on (t.IDPARENT = i.IDI)" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.INSTRUMENTGUI.ToString() + " ig on (ig.IDI = i.IDI and ig.GUITYPE != @Full)" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(" + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "i") + ")" + Cst.CrLf;
            //            
            if (m_IdI > 0) // lorsque IDI est préciser sur la page aspx.
            {
                dataParameters.Add(new DataParameter(SessionTools.CS, "IDI", DbType.Int32), m_IdI);
                sqlQuery += SQLCst.AND + "i.IDI=@IDI" + Cst.CrLf;
            }
            //
            sqlQuery += SQLCst.ORDERBY + " p.IDENTIFIER, i.IDENTIFIER, t.IDENTIFIER, ig.SCREENNAME" + Cst.CrLf;
            //    
            return new QueryParameters(SessionTools.CS, sqlQuery.ToString(), dataParameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20100402 Add  TemplateIdT
        public void InitFirstTemplateUsedAsDefault()
        {
            IDataReader dr = null;
            try
            {
                TemplateIdentifier = string.Empty;
                TemplateIdT = 0;
                CurrentIdScreen = string.Empty;
                //
                QueryParameters qryParameters = GetQueryDefaultScreenAndTemplate();
                //
                if (null != qryParameters)
                {
                    //20070717 FI utilisation de ExecuteDataTable pour le cache
                    dr = DataHelper.ExecuteDataTable(SessionTools.CS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()).CreateDataReader();
                    if (dr.Read())
                    {
                        string product = Convert.ToString(dr["Product"]);

                        IdP = Convert.ToInt32(dr["IDProduct"]);
                        IdI = Convert.ToInt32(dr["IDInstrument"]);
                        TemplateIdentifier = Convert.ToString(dr["IDENTIFIER_Template"]);
                        TemplateIdT = Convert.ToInt32(dr["IDT_Template"]);
                        CurrentIdScreen = Convert.ToString(dr["IDScreen"]);
                    }
                }
            }
            catch { throw; }
            finally
            {
                if (null != dr)
                    dr.Close();
            }
        }

        /// <summary>
        /// Lecture de IDI,IDP,TEMPLATE,SCREEN depuis COOKIES
        /// </summary>
        public void InitFromCookies()
        {

            AspTools.ReadSQLCookie(GrpElement, Cst.EnumElement.Product.ToString(), out string cookieValue);
            if (StrFunc.IsFilled(cookieValue))
                IdP = int.Parse(cookieValue);
            
            AspTools.ReadSQLCookie(GrpElement, Cst.EnumElement.Instrument.ToString(), out cookieValue);
            if (StrFunc.IsFilled(cookieValue))
                IdI = int.Parse(cookieValue);
            
            AspTools.ReadSQLCookie(GrpElement, Cst.EnumElement.Template.ToString(), out cookieValue);
            TemplateIdentifier = cookieValue;
            
            AspTools.ReadSQLCookie(GrpElement, Cst.EnumElement.Screen.ToString(), out cookieValue);
            CurrentIdScreen = cookieValue;
        }

        /// <summary>
        /// Initialise actionTuning
        /// </summary>
        /// <param name="pCs"></param>
        public void InitActionTuning(string pCs)
        {
            InitActionTuning(pCs, m_IdI);
        }

        /// <summary>
        /// Retourne l'expression SQL qui donne les produits accessibles
        /// </summary>
        /// <returns></returns>
        public virtual string GetSQLRestrictProduct()
        {
            return GetSQLRestrictProduct("pr");
        }

        /// <summary>
        /// Retourne l'expression SQL qui donne les produits accessibles
        /// </summary>
        /// <param name="pSqlAlias"></param>
        /// <returns></returns>
        public virtual string GetSQLRestrictProduct(string pSqlAlias)
        {
            return string.Empty;
        }

        #endregion Methods
    }

}
