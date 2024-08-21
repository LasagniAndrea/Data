#region using directives
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Reflection;

using EFS.ACommon;
using EFS.Common;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;


using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Notification;


using EfsML.v30;
using EfsML.v30.Doc;
using EfsML.v30.Fix;
using EfsML.v30.LoanDeposit;
using EfsML.v30.Security;
using EfsML.v30.Security.Shared;
using EfsML.v30.Shared;
//using EfsML.v30.Repository;
//using EfsML.v30.AssetDef;

using FpML.Enum;
using FpML.Interface;

using FpML.v44.Enum;
using FpML.v44.BondOption;
using FpML.v44.Cd;
using FpML.v44.CorrelationSwaps;
using FpML.v44.CreditEvent;
using FpML.v44.DividendSwaps;
using FpML.v44.Doc;
using FpML.v44.Eq;
using FpML.v44.Eq.Shared;
using FpML.v44.Eqd;
using FpML.v44.Main;
using FpML.v44.Option;
using FpML.v44.Option.Shared;
using FpML.v44.Shared;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.ReturnSwaps;
using FpML.v44.VarianceSwaps;

using FixML.v50SP1;
using FixML.Enum;
#endregion using directives




namespace EfsML.v30.Notification
{

    /// <summary>
    /// Représente une notification 
    /// <para>le root eq confirmationMessage pour des raisons de compabilité</para>
    /// </summary>
    /// FI 20140728 [20255] add tradeCciMatch
    /// FI 20150218 [20275] Modify
    /// FI 20150427 [20987] Modify
    /// FI 20160530 [21885] Modify
    /// FI 20170217 [22862] Modify
    [System.Xml.Serialization.XmlTypeAttribute(Namespace = "http://www.efs.org/2007/EFSmL-3-0")]
    [System.Xml.Serialization.XmlRootAttribute("confirmationMessage", Namespace = "http://www.efs.org/2005/EFSmL-3-0", IsNullable = false)]
    public partial class NotificationDocument
    {
        #region Members
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EfsMLDocumentVersionEnum EfsMLversion;
        /// <summary>
        /// En-tête de notification
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("header", Order = 1)]
        public NotificationHeader header;
        /// <summary>
        /// Représente les trades utilisés par l'état
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("dataDocument", Order = 2)]
        public EfsDocument dataDocument;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeSortingSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeSorting", Order = 3)]
        public ReportTradeSort tradeSorting;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool notepadSpecified;
        /// <summary>
        /// Représente toutes les notes des trades utilisés par l'état
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("notepad", Order = 4)]
        public CDATA[] notepad;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool detailsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("details", Order = 5)]
        public ConfirmationMessageDetails details;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool eventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("events", Order = 6)]
        public Events events;
        //
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool hierarchicalEventsSpecified;
        [System.Xml.Serialization.XmlElementAttribute("events", Order = 7)]
        public HierarchicalEvents hierarchicalEvents;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean tradesSpecified;
        /// <summary>
        /// Représente les trades négociés
        /// </summary>
        /// FI 20150427 [20987] Modify transactions est de type list
        [System.Xml.Serialization.XmlElementAttribute("trades", Order = 8)]
        public List<TradesReport> trades;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean unsettledTradesSpecified;
        /// <summary>
        /// Représente les trades non encore réglés
        /// </summary>
        /// FI 20150623 [21149] add
        [System.Xml.Serialization.XmlElementAttribute("unsettledTrades", Order = 9)]
        public List<TradesReport> unsettledTrades;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean settledTradesSpecified;
        /// <summary>
        /// Représente les trades non encore réglés
        /// </summary>
        /// FI 20150623 [21149] add
        [System.Xml.Serialization.XmlElementAttribute("settledTrades", Order = 10)]
        public List<TradesReport> settledTrades;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean posActionsSpecified;
        /// <summary>
        /// Représente les actions sur positions
        /// </summary>
        /// FI 20150427 [20987] Modify transactions est de type list
        [System.Xml.Serialization.XmlElementAttribute("posActions", Order = 11)]
        public List<PosActions> posActions;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean stlPosActionsSpecified;
        /// <summary>
        /// Représente les actions sur positions - settlement date
        /// </summary>
        /// FI 20150623 [21149] add
        [System.Xml.Serialization.XmlElementAttribute("stlPosActions", Order = 12)]
        public List<PosActions> stlPosActions;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean posTradesSpecified;
        /// <summary>
        /// Représente les positions detaillées
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("posTrades", Order = 13)]
        public List<PosTrades> posTrades;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean stlPosTradesSpecified;
        /// <summary>
        /// Représente les positions detaillées (settlement date)
        /// </summary>
        /// FI 20150623 [21149]
        [System.Xml.Serialization.XmlElementAttribute("stlPosTrades", Order = 14)]
        public List<PosTrades> stlPosTrades;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool posSyntheticsSpecified;
        /// <summary>
        /// Représente les positions synthétiques
        /// </summary>
        /// FI 20150427 [20987] posSynthetics est de type list de PosSynthetics
        //[XmlArray(ElementName = "posSynthetics", Order = 11)]
        //[XmlArrayItemAttribute("posSynthetic")]
        [System.Xml.Serialization.XmlElementAttribute("posSynthetics", Order = 15)]
        public List<PosSynthetics> posSynthetics;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool stlPosSyntheticsSpecified;
        /// <summary>
        /// Représente les positions synthétiques (settlement date)
        /// </summary>
        /// FI 20150708 [21149] Add
        [System.Xml.Serialization.XmlElementAttribute("stlPosSynthetics", Order = 16)]
        public List<PosSynthetics> stlPosSynthetics;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool dlvTradesSpecified;
        /// <summary>
        /// Représente les trades Futures pour lesquels il existe un paiement d'unre livraison de ss-jacent (cas de livraisons périodique de ss-jacent commodity)
        /// <para>Exemple contrat Future avec livraisons périodiques de Gaz</para>
        /// </summary>
        /// FI 20170217 [22862] Add
        [System.Xml.Serialization.XmlElementAttribute("dlvTrades", Order = 17)]
        public List<DlvTrades> dlvTrades;



        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean cashPaymentsSpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150128 [20275] Add
        /// FI 20150427 [20987] cashPayments est de type list de CashPayments
        //[XmlArray(ElementName = "cashPayments", Order = 12)]
        //[XmlArrayItemAttribute("cashPayment")]
        [System.Xml.Serialization.XmlElementAttribute("cashPayments", Order = 18)]
        public List<CashPayments> cashPayments;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean collateralsSpecified;

        /// <summary>
        ///  Liste des dépôts de garantie (avec les haircuts appliqués par chambre de compensation)
        ///  <para>Elément présent uniquement sur les report de type SYNTHESIS</para>
        /// </summary>
        /// FI 20160530 [21885] Add
        [System.Xml.Serialization.XmlElementAttribute("collaterals", Order = 19)]
        public List<CollateralsReport> collaterals;


        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public Boolean underlyingStocksSpecified;

        /// <summary>
        ///  Liste des positions Actions utilisées pour la réduction des postions ETD Short futures et Short call 
        ///  <para>Elément présent uniquement sur les report de type SYNTHESIS</para>
        /// </summary>
        /// FI 20160530 [21885] Add
        [System.Xml.Serialization.XmlElementAttribute("underlyingStocks", Order = 20)]
        public List<UnderlyingStocksReport> underlyingStocks;
        

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool commonDataSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("commonData", Order = 21)]
        public CommonData commonData;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool repositorySpecified;
        /// <summary>
        /// 
        /// </summary>
        /// FI 20130621 [18745] représente les éléments du référentiel
        [System.Xml.Serialization.XmlElementAttribute("repository", Order = 22)]
        public Repository.Repository repository;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeCciMatchsSpecified;

        /// <summary>
        /// 
        /// </summary>
        [XmlArray(ElementName = "tradeCciMatchs", Order = 23)]
        [XmlArrayItemAttribute("tradeCciMatch")]
        public List<TradeCciMatch> tradeCciMatch;

        #endregion Members
    }

    /// <summary>
    /// En-tête d'un message de confirmation
    /// </summary>
    public partial class NotificationHeader
    {
        #region Members
        /// <summary>
        /// IdMCO => Id unique du message 
        /// </summary>
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        /// <summary>
        /// Identification Message
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("confirmationMessageId", Order = 1)]
        public NotificationId[] confirmationMessageId;

        /// <summary>
        /// Identification du canal de communication
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("notificationConfirmationSystemIds", Order = 2)]
        public NotificationConfirmationSystemIds notificationConfirmationSystemIds;

        /// <summary>
        /// date Generation
        /// </summary>
        /// FI 20140808 [20549] creationTimestamp est de type EFS_DateTimeUTC
        [System.Xml.Serialization.XmlElementAttribute("creationTimestamp", Order = 3)]
        public EFS_DateTimeUTC creationTimestamp;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool previousCreationTimestampSpecified;

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        [System.Xml.Serialization.XmlElementAttribute("previousCreationTimestamp", Order = 4)]
        public PreviousDateTimeUTC[] previousCreationTimestamp;


        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("valueDate", Order = 5)]
        public EFS_Date valueDate;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool valueDate2Specified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("valueDate2", Order = 6)]
        public EFS_Date valueDate2;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sendBy", Order = 7)]
        public NotificationRouting sendBy;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("sendTo", Order = 8)]
        public NotificationRouting[] sendTo;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool copyToSpecified;
        /// <summary>
        /// /
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("copyTo", Order = 9)]
        public NotificationRoutingCopyTo[] copyTo;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool softApplicationSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("softApplication", Order = 10)]
        public SoftApplication softApplication;

        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportingCurrencySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingCurrency", Order = 11)]
        public Currency reportingCurrency;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportingFeeDisplaySpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportingFeeDisplay", Order = 12)]
        public string reportingFeeDisplay;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool reportSettingsSpecified;
        /// <summary>
        /// 
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("reportSettings", Order = 13)]
        public ReportSettings reportSettings;
        #endregion Members
    }

    /// <summary>
    /// Représente le détail d'un message de facturation
    /// </summary>
    [System.Xml.Serialization.XmlRootAttribute("confirmationMessageDetails", Namespace = "http://www.efs.org/2005/EFSmL-3-0", IsNullable = false)]
    public partial class ConfirmationMessageDetails
    {
        /// <summary>
        /// Représente les trades impliqués dans la facture
        /// </summary>
        [System.Xml.Serialization.XmlElementAttribute("tradeDetail", Order = 1)]
        public ConfirmationTradeDetail[] tradeDetail;
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ConfirmationTradeDetail
    {
        #region Members
        [System.Xml.Serialization.XmlAttributeAttribute("OTCmlId", DataType = "normalizedString")]
        public string otcmlId;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool tradeHeaderSpecified;
        [System.Xml.Serialization.XmlElementAttribute("tradeHeader", Order = 1)]
        public TradeHeader tradeHeader;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool productSpecified;
        [System.Xml.Serialization.XmlElementAttribute("product", Order = 2)]
        public ConfirmationTradeProduct product;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool additionalPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("additionalPayment", Order = 3)]
        public Payment[] additionalPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool otherPartyPaymentSpecified;
        [System.Xml.Serialization.XmlElementAttribute("otherPartyPayment", Order = 4)]
        public Payment[] otherPartyPayment;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool extendsSpecified;

        [System.Xml.Serialization.XmlElementAttribute("tradeExtends", Order = 5)]
        public TradeExtends extends;

        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool externalLinkSpecified;
        [System.Xml.Serialization.XmlElementAttribute("externalLink", Order = 6)]
        public string externalLink;

        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ConfirmationTradeProduct
    {
        #region Members
        // EG 20140702 New build FpML4.4 CorrelationSwapOption removed
        // EG 20140702 New build FpML4.4 VarianceSwapOption removed
        // EG 20140702 New build FpML4.4 DividendSwapTransactionSupplementOption removed
        // EG 20140702 New build FpML4.4 VarianceSwapTransactionSupplement addedd
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool productSpecified;
        [System.Xml.Serialization.XmlElementAttribute("bondOption", typeof(BondOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("brokerEquityOption", typeof(BrokerEquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("bulletPayment", typeof(BulletPayment), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("buyAndSellBack", typeof(BuyAndSellBack), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("capFloor", typeof(CapFloor), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("correlationSwap", typeof(CorrelationSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        //[System.Xml.Serialization.XmlElementAttribute("correlationSwapOption", typeof(CorrelationSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwap", typeof(CreditDefaultSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("creditDefaultSwapOption", typeof(CreditDefaultSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurity", typeof(DebtSecurity), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("debtSecurityTransaction", typeof(DebtSecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplement", typeof(DividendSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        //[System.Xml.Serialization.XmlElementAttribute("dividendSwapTransactionSupplementOption", typeof(DividendSwapTransactionSupplementOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeBase", typeof(EquityDerivativeBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeLongFormBase", typeof(EquityDerivativeLongFormBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityDerivativeShortFormBase", typeof(EquityDerivativeShortFormBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityForward", typeof(EquityForward), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityOption", typeof(EquityOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equityOptionTransactionSupplement", typeof(EquityOptionTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equitySwapTransactionSupplement", typeof(EquitySwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("equitySecurityTransaction", typeof(EquitySecurityTransaction), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", typeof(ExchangeTradedDerivative), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fra", typeof(Fra), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxAverageRateOption", typeof(FxAverageRateOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxBarrierOption", typeof(FxBarrierOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxDigitalOption", typeof(FxDigitalOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxSimpleOption", typeof(FxOptionLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxSingleLeg", typeof(FxLeg), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("fxSwap", typeof(FxSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("loanDeposit", typeof(LoanDeposit.LoanDeposit), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("nettedSwapBase", typeof(NettedSwapBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("optionBase", typeof(OptionBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("optionBaseExtended", typeof(OptionBaseExtended), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("repo", typeof(Repo), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwap", typeof(ReturnSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("returnSwapBase", typeof(ReturnSwapBase), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("securityLending", typeof(SecurityLending), Namespace = "http://www.efs.org/2007/EFSmL-3-0", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("strategy", typeof(Strategy), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("swap", typeof(Swap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("swaption", typeof(Swaption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("termDeposit", typeof(TermDeposit), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwap", typeof(VarianceSwap), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        //[System.Xml.Serialization.XmlElementAttribute("varianceSwapOption", typeof(VarianceSwapOption), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        [System.Xml.Serialization.XmlElementAttribute("varianceSwapTransactionSupplement", typeof(VarianceSwapTransactionSupplement), Namespace = "http://www.fpml.org/2007/FpML-4-4", Order = 1)]
        public Product product;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool subProductSpecified;
        [System.Xml.Serialization.XmlElementAttribute("subProduct", Order = 2)]
        public ConfirmationTradeProduct[] subProduct;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool streamSpecified;
        [System.Xml.Serialization.XmlElementAttribute("stream", Order = 3)]
        public InterestRateStream[] stream;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool legSpecified;
        [System.Xml.Serialization.XmlElementAttribute("leg", Order = 4)]
        public FxLeg[] leg;
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool exchangeTradedDerivativeSpecified;
        [System.Xml.Serialization.XmlElementAttribute("exchangeTradedDerivative", Order = 5)]
        public ExchangeTradedDerivative exchangeTradedDerivative;
        #endregion Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRouting : RoutingPartyReference
    {
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool partyReferenceSpecified;
        [System.Xml.Serialization.XmlElementAttribute("partyRelativeTo", Order = 1)]
        public PartyReference partyReference;
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRoutingCopyTo : NotificationRouting
    {
        [System.Xml.Serialization.XmlAttributeAttribute("bcc", DataType = "boolean")]
        public bool isBcc;
    }



}
