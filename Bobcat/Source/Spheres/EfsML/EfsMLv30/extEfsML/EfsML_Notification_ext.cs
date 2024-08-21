#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.Interface;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Notification;
using EfsML.v30.Doc;
using EfsML.v30.Fix;
using EfsML.v30.Shared;
using FpML.Interface;
using FpML.v44.Doc;
using FpML.v44.Fx;
using FpML.v44.Ird;
using FpML.v44.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
#endregion using directives


namespace EfsML.v30.Notification
{
    /// <summary>
    /// Représente une notification (édition, confirmation)
    /// </summary>
    public partial class NotificationDocument : INotificationDocument
    {
        #region  constructor
        /// <summary>
        /// Constructor vierge
        /// </summary>
        /// FI 20150427 [20987] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20160530 [21885] Modify
        /// FI 20170217 [22862] Modify
        public NotificationDocument()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version30;

            // FI 20150427 [20987] Mise en place des instanciations suivantes 
            commonData = new CommonData();
            tradeCciMatch = new List<TradeCciMatch>();
            trades = new List<TradesReport>();
            // FI 20150623 [21149]  unsettledTrades et settledTrades
            unsettledTrades = new List<TradesReport>();
            settledTrades = new List<TradesReport>();

            posTrades = new List<PosTrades>();
            // FI 20150623 [21149] add stlPosTrades
            stlPosTrades = new List<PosTrades>();

            posActions = new List<PosActions>();
            // FI 20150623 [21149] add stlPosActions
            stlPosActions = new List<PosActions>();

            posSynthetics = new List<PosSynthetics>();
            stlPosSynthetics = new List<PosSynthetics>();

            cashPayments = new List<CashPayments>();
            // FI 20160530 [21885] Add
            collaterals = new List<CollateralsReport>();
            // FI 20160613 [22256] Add
            underlyingStocks = new List<UnderlyingStocksReport>();

            // FI 20170217 [22862] Add
            dlvTrades = new List<DlvTrades>();

        }
        #endregion  constructor

        #region IConfirmationMessageDocument Membres
        //EfsMLDocumentVersionEnum IConfirmationMessageDocument.EfsMLversion
        //{
        //    get { return this.EfsMLversion; }
        //    set { this.EfsMLversion = value; }
        //}
        INotificationHeader INotificationDocument.Header
        {
            get { return this.header; }
            set { this.header = (NotificationHeader)value; }
        }
        INotificationHeader INotificationDocument.CreateNotificationHeader()
        {
            return new NotificationHeader();
        }
        IRoutingCreateElement INotificationDocument.CreateRoutingCreateElement()
        {
            return new RoutingCreateElement();
        }
        INotificationRouting INotificationDocument.CreateNotificationRouting()
        {
            return new NotificationRouting();
        }
        INotificationRoutingCopyTo INotificationDocument.CreateNotificationRoutingCopyTo()
        {
            return new NotificationRoutingCopyTo();
        }
        IReference INotificationDocument.CreatePartyReference()
        {
            return new PartyReference();
        }
        IDataDocument INotificationDocument.DataDocument
        {
            get
            {
                return this.dataDocument;
            }
            set
            {
                dataDocument = (EfsDocument)value;
            }
        }
        bool INotificationDocument.TradeSortingSpecified
        {
            get { return this.tradeSortingSpecified; }
            set { tradeSortingSpecified = value; }
        }
        ReportTradeSort INotificationDocument.TradeSorting
        {
            get { return this.tradeSorting; }
            set { tradeSorting = value; }
        }
        bool INotificationDocument.EventsSpecified
        {
            get { return this.eventsSpecified; }
            set { eventsSpecified = value; }
        }
        Events INotificationDocument.Events
        {
            get { return this.events; }
            set { events = value; }
        }
        bool INotificationDocument.HierarchicalEventsSpecified
        {
            get { return this.hierarchicalEventsSpecified; }
            set { hierarchicalEventsSpecified = value; }
        }
        HierarchicalEvents INotificationDocument.HierarchicalEvents
        {
            get { return this.hierarchicalEvents; }
            set { hierarchicalEvents = value; }
        }
        bool INotificationDocument.NotepadSpecified
        {
            get { return this.notepadSpecified; }
            set { notepadSpecified = value; }
        }
        CDATA[] INotificationDocument.Notepad
        {
            get { return this.notepad; }
            set { notepad = value; }
        }
        //
        bool INotificationDocument.DetailsSpecified
        {
            get { return this.detailsSpecified; }
            set { detailsSpecified = value; }
        }
        IConfirmationMessageDetails INotificationDocument.Details
        {
            get { return this.details; }
            set { details = (ConfirmationMessageDetails)value; }
        }


        Boolean INotificationDocument.TradesSpecified
        {
            get { return tradesSpecified; }
            set { tradesSpecified = value; }
        }
        List<TradesReport> INotificationDocument.Trades
        {
            get { return this.trades; }
            set { trades = value; }
        }

        Boolean INotificationDocument.UnsettledTradesSpecified
        {
            get { return this.unsettledTradesSpecified; }
            set { this.unsettledTradesSpecified = value; }
        }

        List<TradesReport> INotificationDocument.UnsettledTrades
        {
            get { return this.unsettledTrades; }
            set { this.unsettledTrades = value; }
        }


        Boolean INotificationDocument.SettledTradesSpecified
        {
            get { return this.settledTradesSpecified; }
            set { this.settledTradesSpecified = value; }
        }

        List<TradesReport> INotificationDocument.SettledTrades
        {
            get { return this.settledTrades; }
            set { this.settledTrades = value; }
        }

        /// <summary>
        ///  Actions sur positions 
        /// </summary>
        /// FI 20150427 [20987] posActions est de type List
        bool INotificationDocument.PosActionsSpecified
        {
            get { return this.posActionsSpecified; }
            set { posActionsSpecified = value; }
        }
        List<PosActions> INotificationDocument.PosActions
        {
            get { return this.posActions; }
            set { posActions = value; }
        }
        
        /// <summary>
        ///  Actions sur positions 
        /// </summary>
        /// FI 20150623 [20987] posActions est de type List
        bool INotificationDocument.StlPosActionsSpecified
        {
            get { return this.stlPosActionsSpecified; }
            set { stlPosActionsSpecified = value; }
        }
        List<PosActions> INotificationDocument.StlPosActions
        {
            get { return this.stlPosActions; }
            set { stlPosActions = value; }
        }



        Boolean INotificationDocument.PosTradesSpecified
        {
            get { return this.posTradesSpecified; }
            set { posTradesSpecified = value; }
        }

        List<PosTrades> INotificationDocument.PosTrades
        {
            get { return this.posTrades; }
            set { posTrades = value; }
        }


        Boolean INotificationDocument.StlPosTradesSpecified
        {
            get { return this.stlPosTradesSpecified; }
            set { stlPosTradesSpecified = value; }
        }

        List<PosTrades> INotificationDocument.StlPosTrades
        {
            get { return this.stlPosTrades; }
            set { stlPosTrades = value; }
        }




        bool INotificationDocument.PosSyntheticsSpecified
        {
            get { return this.posSyntheticsSpecified; }
            set { posSyntheticsSpecified = value; }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// FI 20150427 [20987] posActions est de type List de PosSynthetics
        List<PosSynthetics> INotificationDocument.PosSynthetics
        {
            get { return this.posSynthetics; }
            set { posSynthetics = value; }
        }


        bool INotificationDocument.StlPosSyntheticsSpecified
        {
            get { return this.stlPosSyntheticsSpecified; }
            set { stlPosSyntheticsSpecified = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20150709 [XXXXX] Add
        List<PosSynthetics> INotificationDocument.StlPosSynthetics
        {
            get { return this.stlPosSynthetics; }
            set { stlPosSynthetics = value; }
        }



        bool INotificationDocument.DlvTradesSpecified
        {
            get { return this.dlvTradesSpecified; }
            set { dlvTradesSpecified = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170217 [22862] Add
        List<DlvTrades> INotificationDocument.DlvTrades
        {
            get { return this.dlvTrades; }
            set { dlvTrades = value; }
        }



        bool INotificationDocument.CommonDataSpecified
        {
            get { return this.commonDataSpecified; }
            set { commonDataSpecified = value; }
        }
        CommonData INotificationDocument.CommonData
        {
            get { return this.commonData; }
            set { commonData = value; }
        }

        IConfirmationTradeDetail INotificationDocument.CreateTradeDetail()
        {
            return new ConfirmationTradeDetail();
        }
        IConfirmationMessageDetails INotificationDocument.CreateConfirmationMessageDetails()
        {
            return new ConfirmationMessageDetails();
        }


        Boolean INotificationDocument.TradeCciMatchSpecified
        {
            get { return this.tradeCciMatchsSpecified; }
            set { this.tradeCciMatchsSpecified = value; }
        }

        List<TradeCciMatch> INotificationDocument.TradeCciMatch
        {
            get { return this.tradeCciMatch; }
            set { this.tradeCciMatch = value; }
        }


        Boolean INotificationDocument.CashPaymentsSpecified
        {
            get { return cashPaymentsSpecified; }
            set { cashPaymentsSpecified = value; }
        }

        List<CashPayments> INotificationDocument.CashPayments
        {
            get { return this.cashPayments; }
            set { cashPayments = value; }
        }

        Boolean INotificationDocument.CollateralsSpecified
        {
            get { return collateralsSpecified; }
            set { collateralsSpecified = value; }
            
            
        }

        List<CollateralsReport> INotificationDocument.Collaterals
        {
            get { return this.collaterals; }
            set { collaterals = value; }
        }

        Boolean INotificationDocument.UnderlyingStocksSpecified
        {
            get { return underlyingStocksSpecified; }
            set { underlyingStocksSpecified = value; }


        }

        List<UnderlyingStocksReport> INotificationDocument.UnderlyingStocks
        {
            get { return this.underlyingStocks; }
            set { underlyingStocks = value; }
        }


        #endregion

        #region IEfsDocument
        EfsMLDocumentVersionEnum IEfsDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value; }
        }
        #endregion

        #region IRepositoryDocument Members
        IRepository IRepositoryDocument.Repository
        {
            get { return this.repository; }
            set { this.repository = (EfsML.v30.Repository.Repository)value; }
        }
        
        bool IRepositoryDocument.RepositorySpecified
        {
            set { this.repositorySpecified = value; }
            get { return this.repositorySpecified; }
        }

        IRepository IRepositoryDocument.CreateRepository()
        {
            return (IRepository)new EfsML.v30.Repository.Repository();
        }

        /// <summary>
        /// Retourne tous les assets présents dans le repository
        /// </summary>
        /// <returns></returns>
        /// FI 20150807 [XXXXX] Add
        /// FI 20151019 [21317] Modify
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {
            List<IAssetRepository> ret = new List<IAssetRepository>();
            if (repositorySpecified)
            {
                ret =
                 ((from asset in repository.assetEquity
                   select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetIndex
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetRateIndex
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetETD
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetFxRate
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetDebtSecurity // FI 20151019 [21317]
                          select (IAssetRepository)asset)
                  .Concat(from asset in repository.assetCommodity   // FI 20170116 [21916] Add
                           select (IAssetRepository)asset)
                          ).ToList();
            }
            return ret;
        }
        #endregion IRepositoryDocument Members

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDim"></param>
        /// <returns></returns>
        ReportTradeSortKeys CreateReportTradeSortKeys(int pDim)
        {
            ReportTradeSortKeys ret = new ReportTradeSortKeys
            {
                key = new ReportTradeSortKey[pDim]
            };
            for (int i = 0; i < ArrFunc.Count(ret.key); i++)
                ret.key[i] = new ReportTradeSortKey();
            
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IEfsDocument INotificationDocument.CreateEfsDocument()
        {
            return new EfsDocument();
        }


        #endregion Methods
    }


    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationHeader : INotificationHeader
    {
        #region Accessors
        #region OTCmlId
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public int OTCmlId
        {
            get { return Convert.ToInt32(otcmlId); }
            set { otcmlId = value.ToString(); }
        }
        #endregion OTCmlId
        #endregion Accessors

        #region Constructor
        public NotificationHeader()
        {
            softApplication = new SoftApplication();
        }
        #endregion Constructor

        #region ISpheresId Membres
        int ISpheresId.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        string ISpheresId.OtcmlId
        {
            get { return otcmlId; }
            set { otcmlId = value; }
        }
        #endregion

        #region IConfirmationMessageHeader Membres
        /// <summary>
        /// 
        /// </summary>
        NotificationId[] INotificationHeader.ConfirmationMessageId
        {
            get { return this.confirmationMessageId; }
            set { this.confirmationMessageId = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        NotificationConfirmationSystemIds INotificationHeader.NotificationConfirmationSystemIds
        {
            get { return this.notificationConfirmationSystemIds; }
            set { this.notificationConfirmationSystemIds = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        EFS_DateTimeUTC INotificationHeader.CreationTimestamp
        {
            get { return this.creationTimestamp; }
            set { this.creationTimestamp = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        bool INotificationHeader.PreviousCreationTimestampSpecified
        {
            get { return this.previousCreationTimestampSpecified; }
            set { this.previousCreationTimestampSpecified = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        PreviousDateTimeUTC[] INotificationHeader.PreviousCreationTimestamp
        {
            get { return this.previousCreationTimestamp; }
            set { this.previousCreationTimestamp = value; }
        }



        /// <summary>
        /// 
        /// </summary>
        EFS_Date INotificationHeader.ValueDate
        {
            get { return this.valueDate; }
            set { this.valueDate = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.ValueDate2Specified
        {
            get { return this.valueDate2Specified; }
            set { this.valueDate2Specified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        EFS_Date INotificationHeader.ValueDate2
        {
            get { return this.valueDate2; }
            set { this.valueDate2 = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        INotificationRouting INotificationHeader.SendBy
        {
            get { return this.sendBy; }
            set { this.sendBy = (NotificationRouting)value; }
        }

        /// <summary>
        /// 
        /// </summary>
        INotificationRouting[] INotificationHeader.SendTo
        {
            get { return this.sendTo; }
            set { this.sendTo = (NotificationRouting[])value; }
        }

        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.CopyToSpecified
        {
            get { return this.copyToSpecified; }
            set { this.copyToSpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        INotificationRoutingCopyTo[] INotificationHeader.CopyTo
        {
            get { return this.copyTo; }
            set { this.copyTo = (NotificationRoutingCopyTo[])value; }
        }

        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.SoftApplicationSpecified
        {
            get { return this.softApplicationSpecified; }
            set { this.softApplicationSpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        ISoftApplication INotificationHeader.SoftApplication
        {
            get { return this.softApplication; }
            set { this.softApplication = (SoftApplication)value; }
        }

        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.ReportCurrencySpecified
        {
            get { return this.reportingCurrencySpecified; }
            set { this.reportingCurrencySpecified = value; }
        }
        /// <summary>
        /// Obtient ou définit la devise de reporting
        /// </summary>
        ICurrency INotificationHeader.ReportCurrency
        {
            get { return this.reportingCurrency; }
            set { this.reportingCurrency = (Currency)value; }
        }

        bool INotificationHeader.ReportFeeDisplaySpecified
        {
            get { return this.reportingFeeDisplaySpecified; }
            set { this.reportingFeeDisplaySpecified = value; }
        }
        string INotificationHeader.ReportFeeDisplay
        {
            get { return this.reportingFeeDisplay; }
            set { this.reportingFeeDisplay = (string)value; }
        }

        bool INotificationHeader.ReportSettingsSpecified
        {
            get { return this.reportSettingsSpecified; }
            set { this.reportSettingsSpecified = value; }
        }
        ReportSettings INotificationHeader.ReportSettings
        {
            get { return this.reportSettings; }
            set { this.reportSettings = (ReportSettings)value; }
        }
        #endregion
    }


    #region ConfirmationMessageDetails
    public partial class ConfirmationMessageDetails : IConfirmationMessageDetails
    {
        #region IConfirmationMessageDetails Members
        IConfirmationTradeDetail[] IConfirmationMessageDetails.TradeDetail
        {
            get
            {
                return this.tradeDetail;
            }
            set
            {
                if (ArrFunc.IsFilled(value))
                {
                    tradeDetail = new ConfirmationTradeDetail[value.Length];
                    value.CopyTo(tradeDetail, 0);
                }
            }
        }
        #endregion IConfirmationMessageDetails Members
    }
    #endregion ConfirmationMessageDetails

    /// <summary>
    /// 
    /// </summary>
    public partial class ConfirmationTradeDetail : IConfirmationTradeDetail
    {
        #region IConfirmationTradeDetail Members
        string IConfirmationTradeDetail.OtcmlId
        {
            get { return this.otcmlId; }
            set { this.otcmlId = value; }
        }
        bool IConfirmationTradeDetail.TradeHeaderSpecified
        {
            get { return this.tradeHeaderSpecified; }
            set { tradeHeaderSpecified = value; }
        }
        ITradeHeader IConfirmationTradeDetail.TradeHeader
        {
            get { return this.tradeHeader; }
            set { tradeHeader = (TradeHeader)value; }
        }
        bool IConfirmationTradeDetail.ProductSpecified
        {
            get { return this.productSpecified; }
            set { productSpecified = value; }
        }
        IConfirmationTradeProduct IConfirmationTradeDetail.Product
        {
            get { return this.product; }
            set { product = (ConfirmationTradeProduct)value; }
        }
        bool IConfirmationTradeDetail.AdditionalPaymentSpecified
        {
            get { return this.additionalPaymentSpecified; }
            set { additionalPaymentSpecified = value; }
        }
        IPayment[] IConfirmationTradeDetail.AdditionalPayment
        {
            get { return this.additionalPayment; }
            set { additionalPayment = (Payment[])value; }
        }
        bool IConfirmationTradeDetail.OtherPartyPaymentSpecified
        {
            get { return this.otherPartyPaymentSpecified; }
            set { otherPartyPaymentSpecified = value; }
        }
        IPayment[] IConfirmationTradeDetail.OtherPartyPayment
        {
            get { return this.otherPartyPayment; }
            set { otherPartyPayment = (Payment[])value; }
        }
        bool IConfirmationTradeDetail.ExtendsSpecified
        {
            get { return this.extendsSpecified; }
            set { extendsSpecified = value; }
        }
        ITradeExtends IConfirmationTradeDetail.Extends
        {
            get { return this.extends; }
            set { extends = (TradeExtends)value; }
        }
        bool IConfirmationTradeDetail.ExternalLinkSpecified
        {
            get { return this.externalLinkSpecified; }
            set { externalLinkSpecified = value; }
        }
        string IConfirmationTradeDetail.ExternalLink
        {
            get { return this.externalLink; }
            set { externalLink = value; }
        }
        IConfirmationTradeProduct IConfirmationTradeDetail.CreateProduct()
        {
            return new ConfirmationTradeProduct();
        }
        #endregion IConfirmationTradeDetail Members
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class ConfirmationTradeProduct : IConfirmationTradeProduct
    {
        #region Members
        #endregion Members
        #region IConfirmationTradeProduct Membres

        bool IConfirmationTradeProduct.ProductSpecified
        {
            get { return this.productSpecified; }
            set { this.productSpecified = value; }
        }

        IProductBase IConfirmationTradeProduct.Product
        {
            get { return this.product; }
            set { this.product = (Product)value; }
        }

        bool IConfirmationTradeProduct.SubProductSpecified
        {
            get { return this.subProductSpecified; }
            set { this.subProductSpecified = value; }
        }
        IConfirmationTradeProduct[] IConfirmationTradeProduct.SubProduct
        {
            get { return this.subProduct; }
            set { this.subProduct = (ConfirmationTradeProduct[])value; }
        }
        bool IConfirmationTradeProduct.StreamSpecified
        {
            get { return this.streamSpecified; }
            set { this.streamSpecified = value; }
        }
        IInterestRateStream[] IConfirmationTradeProduct.Stream
        {
            get { return this.stream; }
            set { this.stream = (InterestRateStream[])value; }
        }
        bool IConfirmationTradeProduct.LegSpecified
        {
            get { return this.legSpecified; }
            set { this.legSpecified = value; }
        }
        IFxLeg[] IConfirmationTradeProduct.Leg
        {
            get { return this.leg; }
            set { this.leg = (FxLeg[])value; }
        }
        bool IConfirmationTradeProduct.ExchangeTradedDerivativeSpecified
        {
            get { return this.exchangeTradedDerivativeSpecified; }
            set { this.exchangeTradedDerivativeSpecified = value; }
        }
        IExchangeTradedDerivative IConfirmationTradeProduct.ExchangeTradedDerivative
        {
            get { return this.exchangeTradedDerivative; }
            set { this.exchangeTradedDerivative = (ExchangeTradedDerivative)value; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRouting : INotificationRouting
    {
        #region IConfirmationRouting Membres
        IReference INotificationRouting.PartyReference
        {
            get { return this.partyReference; }
            set { this.partyReference = (PartyReference)value; }
        }
        bool INotificationRouting.PartyReferenceSpecified
        {
            get { return this.partyReferenceSpecified; }
            set { this.partyReferenceSpecified = value; }
        }
        #endregion

        #region IRoutingPartyReference Membres
        bool IRoutingPartyReference.HRefSpecified
        {
            get { return this.hrefSpecified; }
            set { this.hrefSpecified = value; }
        }
        string IRoutingPartyReference.HRef
        {
            get { return href; }
            set { href = value; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationRoutingCopyTo : INotificationRoutingCopyTo
    {
        #region ConfirmationRoutingCopyTo Membres
        bool INotificationRoutingCopyTo.IsBcc
        {
            get { return isBcc; }
            set { isBcc = value; }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class CashBalanceReport : IProduct 
    {
        #region IProduct Members
        object IProduct.Product { get { return this; } }
        IProductBase IProduct.ProductBase { get { return this; } }
        #endregion IProduct Members

    }

}