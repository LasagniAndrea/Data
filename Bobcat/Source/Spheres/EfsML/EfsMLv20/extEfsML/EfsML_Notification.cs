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

using EfsML.v20;

using FpML.Enum;
using FpML.Interface;

using FpML.v42.Enum;
using FpML.v42.Shared;
using FpML.v42.Doc;
#endregion using directives


namespace EfsML.v20.Notification
{
    /// <summary>
    /// 
    /// </summary>
    public partial class NotificationDocument : INotificationDocument
    {
        #region  constructor
        public NotificationDocument()
        {
            EfsMLversion = EfsMLDocumentVersionEnum.Version20;
        }
        #endregion  constructor

        #region IConfirmationMessageDocument Membres
        //EfsMLDocumentVersionEnum IConfirmationMessageDocument.EfsMLversion
        //{
        //    get { return this.EfsMLversion; }
        //    set { this.EfsMLversion = value; }
        //}
        INotificationHeader INotificationDocument.header
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
        IDataDocument INotificationDocument.dataDocument
        {
            get { return this.dataDocument; }
            set { dataDocument = (DataDocument)value; }
        }
        bool INotificationDocument.tradeSortingSpecified
        {
            get { return false; }
            set { ; }
        }
        ReportTradeSort INotificationDocument.tradeSorting
        {
            get { return null; }
            set { ; }
        }
        bool INotificationDocument.eventsSpecified
        {
            get { return this.eventsSpecified; }
            set { eventsSpecified = value; }
        }
        Events INotificationDocument.events
        {
            get { return this.events; }
            set { events = value; }
        }
        bool INotificationDocument.hierarchicalEventsSpecified
        {
            get { return this.hierarchicalEventsSpecified; }
            set { hierarchicalEventsSpecified = value; }
        }
        HierarchicalEvents INotificationDocument.hierarchicalEvents
        {
            get { return this.hierarchicalEvents; }
            set { hierarchicalEvents = value; }
        }
        bool INotificationDocument.notepadSpecified
        {
            get { return this.notepadSpecified; }
            set { notepadSpecified = value; }
        }
        CDATA[] INotificationDocument.notepad
        {
            get { return this.notepad; }
            set { notepad = value; }
        }
        bool INotificationDocument.detailsSpecified
        {
            get { return false; }
            set { ; }
        }
        IConfirmationMessageDetails INotificationDocument.details
        {
            get { return null; }
            set { ;}
        }

        Boolean INotificationDocument.tradesSpecified
        {
            get { return false; }
            set { ;}
        }

        List<TradesReport> INotificationDocument.trades
        {
            get { return null; }
            set { ;}
        }



        Boolean INotificationDocument.unsettledTradesSpecified
        {
            get { return false; }
            set { ;}
        }

        List<TradesReport> INotificationDocument.unsettledTrades
        {
            get { return null; }
            set { ;}
        }


        Boolean INotificationDocument.settledTradesSpecified
        {
            get { return false; }
            set { ;}
        }

        List<TradesReport> INotificationDocument.settledTrades
        {
            get { return null; }
            set { ;}
        }

        bool INotificationDocument.posActionsSpecified
        {
            get { return false; }
            set { ; }
        }
        List<PosActions> INotificationDocument.posActions
        {
            get { return null; }
            set { ;}
        }

        bool INotificationDocument.stlPosActionsSpecified
        {
            get { return false; }
            set { ; }
        }
        List<PosActions> INotificationDocument.stlPosActions
        {
            get { return null; }
            set { ;}
        }




        Boolean INotificationDocument.posTradesSpecified
        {
            get { return false; }
            set { ;}
        }
        List<PosTrades> INotificationDocument.posTrades
        {
            get { return null; }
            set { ;}
        }


        Boolean INotificationDocument.stlPosTradesSpecified
        {
            get { return false; }
            set { ;}
        }
        List<PosTrades> INotificationDocument.stlPosTrades
        {
            get { return null; }
            set { ;}
        }




        bool INotificationDocument.posSyntheticsSpecified
        {
            get { return false; }
            set { ; }
        }
        List<PosSynthetics> INotificationDocument.posSynthetics
        {
            get { return null; }
            set { ;}
        }


        bool INotificationDocument.stlPosSyntheticsSpecified
        {
            get { return false; }
            set { ; }
        }
        List<PosSynthetics> INotificationDocument.stlPosSynthetics
        {
            get { return null; }
            set { ;}
        }

        bool INotificationDocument.dlvTradesSpecified
        {
            get { return false; }
            set { ; }
        }
        List<DlvTrades> INotificationDocument.dlvTrades
        {
            get { return null; }
            set { ;}
        }

        bool INotificationDocument.cashPaymentsSpecified
        {
            get { return false; }
            set { ; }
        }

        List<CashPayments> INotificationDocument.cashPayments
        {
            get { return null; }
            set { ;}
        }




        IConfirmationTradeDetail INotificationDocument.CreateTradeDetail()
        {
            return null;
        }
        IConfirmationMessageDetails INotificationDocument.CreateConfirmationMessageDetails()
        {
            return null;
        }

        bool INotificationDocument.commonDataSpecified
        {
            get { return false; }
            set { ; }
        }
        CommonData INotificationDocument.commonData
        {
            get { return null; }
            set { ;}
        }

        IEfsDocument INotificationDocument.CreateEfsDocument()
        {
            return new EfsDocument();
        }

        Boolean INotificationDocument.tradeCciMatchSpecified
        {
            get { return false; }
            set { ;}
        }

        List<TradeCciMatch> INotificationDocument.tradeCciMatch
        {
            get { return null; }
            set { ;}
        }
        
        Boolean INotificationDocument.collateralsSpecified
        {
            get { return false; }
            set { ;}
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160530 [21885] Add
        List<CollateralsReport> INotificationDocument.collaterals
        {
            get { return null; }
            set { ;}
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        Boolean INotificationDocument.underlyingStocksSpecified
        {
            get { return false; }
            set { ;}
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160613 [22256] Add
        List<UnderlyingStocksReport> INotificationDocument.underlyingStocks 
        {
            get { return null; }
            set { ;}
        }

        #endregion

        #region IEfsDocument
        EfsMLDocumentVersionEnum IEfsDocument.EfsMLversion
        {
            get { return this.EfsMLversion; }
            set { this.EfsMLversion = value; }
        }
        #endregion

        #region IRepositoryDocument
        bool IRepositoryDocument.repositorySpecified
        {
            get { return false; }
            set { ; }
        }
        IRepository IRepositoryDocument.repository
        {
            get { return null; }
            set { ;}
        }
        IRepository IRepositoryDocument.CreateRepository()
        {
            return null;
        }

        /// <summary>
        /// Retourne une liste vide
        /// </summary>
        /// <returns></returns>
        /// FI 20150708 [XXXXX] Retourne une liste vide
        List<IAssetRepository> IRepositoryDocument.GetAllRepositoryAsset()
        {
            return new List<IAssetRepository>();
        }
        #endregion

    }
    

    /// <summary>
    /// En-tête d'une notification
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
        //
        #region Constructor
        public NotificationHeader()
        {
            softApplication = new SoftApplication();
        }
        #endregion Constructor
        //
        #region ISpheresId Membres
        /// <summary>
        /// 
        /// </summary>
        int ISpheresId.OTCmlId
        {
            get { return this.OTCmlId; }
            set { this.OTCmlId = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        string ISpheresId.otcmlId
        {
            get { return otcmlId; }
            set { otcmlId = value; }
        }
        #endregion


        #region INotificationHeader Membres
        NotificationId[] INotificationHeader.confirmationMessageId
        {
            get { return this.confirmationMessageId; }
            set { this.confirmationMessageId = value; }
        }
        NotificationConfirmationSystemIds INotificationHeader.notificationConfirmationSystemIds
        {
            get { return this.notificationConfirmationSystemIds; }
            set { this.notificationConfirmationSystemIds = value; }
        }

        /// <summary>
        /// Obtient ou définit la date de création
        /// </summary>
        /// FI 20140808 [20549] creationTimestamp est de type EFS_DateTimeUTC
        EFS_DateTimeUTC INotificationHeader.creationTimestamp
        {
            get { return this.creationTimestamp; }
            set { this.creationTimestamp = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        bool INotificationHeader.previousCreationTimestampSpecified
        {
            get { return this.previousCreationTimestampSpecified; }
            set { this.previousCreationTimestampSpecified = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160624 [22286] Add
        PreviousDateTimeUTC[] INotificationHeader.previousCreationTimestamp
        {
            get { return this.previousCreationTimestamp; }
            set { this.previousCreationTimestamp = value; }
        }



        /// <summary>
        /// 
        /// </summary>
        EFS_Date INotificationHeader.valueDate
        {
            get { return this.valueDate; }
            set { this.valueDate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.valueDate2Specified
        {
            get { return this.valueDate2Specified; }
            set { this.valueDate2Specified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        EFS_Date INotificationHeader.valueDate2
        {
            get { return this.valueDate2; }
            set { this.valueDate2 = value; }
        }


        INotificationRouting INotificationHeader.sendBy
        {
            get { return this.sendBy; }
            set { this.sendBy = (NotificationRouting)value; }
        }
        INotificationRouting[] INotificationHeader.sendTo
        {
            get { return this.sendTo; }
            set { this.sendTo = (NotificationRouting[])value; }
        }
        bool INotificationHeader.copyToSpecified
        {
            get { return this.copyToSpecified; }
            set { this.copyToSpecified = value; }
        }
        INotificationRoutingCopyTo[] INotificationHeader.copyTo
        {
            get { return this.copyTo; }
            set { this.copyTo = (NotificationRoutingCopyTo[])value; }
        }
        bool INotificationHeader.softApplicationSpecified
        {
            get { return this.softApplicationSpecified; }
            set { this.softApplicationSpecified = value; }
        }
        ISoftApplication INotificationHeader.softApplication 
        {
            get { return this.softApplication; }
            set { this.softApplication = (SoftApplication)  value; }
        }
        /// <summary>
        /// 
        /// </summary>
        bool INotificationHeader.reportCurrencySpecified
        {
            get { return this.reportCurrencySpecified; }
            set { this.reportCurrencySpecified = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        ICurrency INotificationHeader.reportCurrency
        {
            get { return this.reportCurrency; }
            set { this.reportCurrency = (Currency)value; }
        }
        bool INotificationHeader.reportFeeDisplaySpecified
        {
            get { return this.reportFeeDisplaySpecified; }
            set { this.reportFeeDisplaySpecified = value; }
        }
        string INotificationHeader.reportFeeDisplay
        {
            get { return this.reportFeeDisplay; }
            set { this.reportFeeDisplay = (string)value; }
        }
        bool INotificationHeader.reportSettingsSpecified
        {
            get { return this.reportSettingsSpecified; }
            set { this.reportSettingsSpecified = value; }
        }
        ReportSettings INotificationHeader.reportSettings
        {
            get { return this.reportSettings; }
            set { this.reportSettings = (ReportSettings)value; }
        }
        #endregion
    }
    

    /// <summary>
    /// Représente un destinataire
    /// </summary>
    public partial class NotificationRouting : INotificationRouting
    {
        #region IConfirmationRouting Membres
        IReference INotificationRouting.partyReference
        {
            get { return this.partyReference; }
            set { this.partyReference = (PartyReference)value; }
        }
        bool INotificationRouting.partyReferenceSpecified
        {
            get { return this.partyReferenceSpecified; }
            set { this.partyReferenceSpecified = value; }
        }
        #endregion

        #region IRoutingPartyReference Membres
        bool IRoutingPartyReference.hRefSpecified
        {
            get { return this.hrefSpecified; }
            set { this.hrefSpecified = value; }
        }
        string IRoutingPartyReference.hRef
        {
            get { return href; }
            set { href = value; }
        }
        #endregion
    }


    /// <summary>
    /// Représente un destinataire en copie
    /// </summary>
    public partial class NotificationRoutingCopyTo : INotificationRoutingCopyTo
    {
        #region IConfirmationRoutingCopyTo Membres
        bool INotificationRoutingCopyTo.isBcc
        {
            get { return isBcc; }
            set { isBcc = value; }
        }
        #endregion
    }
    

}
