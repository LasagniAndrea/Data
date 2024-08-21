using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.GUI.Interface;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.TradeLink;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using EfsML.Repository;
using EfsML.v30.AssetDef;
using EfsML.v30.CashBalance;
using EfsML.v30.Notification;
using EfsML.v30.Shared;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Tz = EFS.TimeZone;

namespace EfsML.Notification
{

    /// <summary>
    /// Classe qui pilote un INotificationDocument
    /// </summary>
    public class NotificationDocumentContainer
    {

        #region Membres
        /// <summary>
        /// 
        /// </summary>
        /// FI 2020623 [XXXXX] Add
        private SetErrorWarning _setErrorWarning = null;

        
        /// <summary>
        /// Représente le message de notification
        /// </summary>
        private readonly INotificationDocument _notificationDocument;

        /// <summary>
        /// Représente le CB fictif s'il y a lieu et les trades CB qui le constituent (La liste est triée par date)
        /// <para>ce membre est not null lorsqu'il y a générération d'un édition de synthèse periodic</para>
        /// <para>ce membre est null dans les autres cas</para>
        /// </summary>
        /// FI 20150427 [20987] Add
        /// FI 20150930 [21311] Modify (Modification de type)  
        private Pair<DataDocumentContainer, List<SQL_TradeRisk>> _cashBalancePeriodic;

        /// <summary>
        /// Réprésente les différents produits des trades impliqués dans un CB 
        /// <para>Permet de optimiser le traitement de manière à jouer les requêtes qui sont nécessaires uniquement</para>
        /// <para>ce membre est not null lorsqu'il y a générération d'un édition de synthèse </para>
        /// <para>ce membre est null dans les autres cas</para>
        /// </summary>
        private CashBalanceProductEnv productEnv;

        #endregion

        #region accessors
        /// <summary>
        /// Obtient le message de notification
        /// </summary>
        public INotificationDocument NotificationDocument
        {
            get { return _notificationDocument; }
        }
        /// <summary>
        /// Obtient la culture
        /// </summary>
        /// FI 20161114 [RATP] Modify (property set is public)
        public string Culture
        {
            get;
            set;
        }
        /// <summary>
        /// Obtient la devise de reporting (1er élément => IdC, 2nd élément le code ISO4217 (3 caractères))
        /// </summary>
        /// FI 20150930 [21311] Modify (Usage d'une pair)
        public Pair<string, string> ReportCurrency
        {
            get;
            private set;
        }
        /// <summary>
        /// Obtient le type d'affichage des frais sur le report
        /// </summary>
        public ReportFeeDisplayEnum ReportFeeDisplay
        {
            get;
            private set;
        }
        #endregion

        #region constructor
        public NotificationDocumentContainer(INotificationDocument pConfirmationMessageDoc)
        {
            _notificationDocument = pConfirmationMessageDoc;
        }
        #endregion
        #region Methods

        /// <summary>
        /// Initialise le delegue chargé d'écrire dans le log
        /// </summary>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20150520 [XXXX] Add Method
        public void InitializeDelegate(SetErrorWarning pSetErrorWarning)
        {
            _setErrorWarning = pSetErrorWarning;
        }

        /// <summary>
        /// Alimente confirmationMessageDoc
        /// </summary>
        /// <remarks>methode appelée depuis les traitements, le message existe nécessairement</remarks>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO">IdMCO du message (identifiant unique du message sous spheres®)</param>
        /// <param name="pCnfMessage">Représente le message</param>
        /// <param name="pNcs">Canal de communication destiné à envoyer le message</param>
        /// <param name="pConfirmationChain">Chaîne de confirmation</param>
        /// <param name="pInciItem">Liste des destinataires</param>
        /// <param name="pIdT">Liste des trades</param>
        /// <param name="pDate"></param>
        /// <param name="pDate2"></param>
        /// <param name="pIdInci_SendBy"></param>
        /// <param name="pSession"></param>
        /// FI 20120829 [18048] Modification de la signature ajout de la colonne pDate2
        /// FI 20140731 [20179] Modify
        /// FI 20141218 [20574] Modify
        /// FI 20150331 [XXPOC] Modify
        /// FI 20150407 [XXPOC] Modify
        /// FI 20150427 [20987] Modify
        /// FI 20150522 [20275] Modify
        /// FI 20160223 [21919] Modify
        /// FI 20160530 [21885] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        public void Initialize(string pCS, int pIdMCO, CnfMessage pCnfMessage, NotificationConfirmationSystem pNcs,
            ConfirmationChain pConfirmationChain, InciItem[] pInciItem, int[] pIdT, DateTime pDate, Nullable<DateTime> pDate2,
            int pIdInci_SendBy, AppSession pSession)
        {

            //Control before start
            if (null == pCnfMessage)
                throw new ArgumentNullException("pCnfMessage is null");
            if (null == pNcs)
                throw new ArgumentNullException("pNcs is null");
            // FI 20150427 [20987] add
            if (ArrFunc.IsEmpty(pIdT))
                throw new ArgumentNullException("pIdT is null");

            // FI 20190515 [23912] Mise en commentaire => Alimentation effectuée via SetSpecificElement
            //List<int> idT = new List<int>(pIdT);
            
            if (pCnfMessage.IsMulti && (false == pCnfMessage.NotificationType.HasValue))
                throw new NullReferenceException(StrFunc.AppendFormat("Message (Identifier:{0}).NotificationType is not specified", pCnfMessage.identifier));

            NotificationDocumentSettings settings = pCnfMessage.GetSettings();

            SetNotificationVersion(pCnfMessage);

            //Alimentation des éléments du document en fonction du type de message
            // RD 20160912 [22447] Add pDate2
            // FI 20190515 [23912] Alimentation de List<int> idT
            List<int> idT = SetSpecificElement(pCS, pIdT,
                                                pCnfMessage.NotificationClass, pCnfMessage.NotificationType,
                                                pConfirmationChain,
                                                pDate, pDate2);

            //Alimentation de confirmationMessageDoc.dataDocument
            SetDataDocument(pCS, idT.ToArray(), pCnfMessage.NotificationClass, pCnfMessage.NotificationType);

            //Alimentation des frais 
            SetOppToTrades2(pCS);

            //Les fontId, folderId, orderId,  UTI des trades sont copiés dans les NotificationDocument.commonData.trade
            UpdCommonDataTrade2(pCS);

            //FI 20141218 [20574] Appel à UpdCommonDataPositionUTI
            //Les PUTI des trades sont copiés dans les NotificationDocument.commonData.trade
            UpdCommonDataPositionUTI(pCS);

            // FI 20160530 [21885] Call SetCollCollateralReportSynthesis
            if (pCnfMessage.IsNotificationType(NotificationTypeEnum.SYNTHESIS))
                SetCollateralReportSynthesis(pCS);

            //Alimentation de confirmationMessageDoc.tradeSorting
            //Définition du Tri des Trades (Datadocument avec plusieurs trades => avis d'opérés uniquement) 
            //Le tri n'est pas fait, il est fait dans le xslt
            ReportSortEnum[] orderBy = pCnfMessage.GetOrderBy();
            SetTradeSorting(orderBy);

            //Alimentation de confirmationMessageDoc.Header
            SetHeader(pCS, pIdMCO, pDate, pDate2, pCnfMessage, pNcs, pConfirmationChain, pInciItem, pIdInci_SendBy, pSession.AppInstance);

            // FI 20150930 [21311] Call of BuildCashBalanceExchangeStream (Appel effectué ici puisque reportCurrency est valorisé par ma méthode SetHeader)
            if (pCnfMessage.IsNotificationType(NotificationTypeEnum.SYNTHESIS))
                BuildCashBalanceExchangeStream(pCS);

            //Alimentation du repository
            // FI 20200520 [XXXXX] Add SQL cache
            SetRepository(CSTools.SetCacheOn(pCS), pCnfMessage);

            if (IsInvoiceMessage())
                SetInvoicingTradeDetails(pCS, settings);

            //Ajout des évènements
            SetEvent(pCS, pSession.SessionId, pCnfMessage, idT.ToArray(), pDate);

            //Ajout des bloc Notes
            SetNotepad(pCS, idT.ToArray(), settings);

            //Mise à jour en fonction des informations présentes dans le repositity
            UpdateAfterRepository();

            //FI 20150522 [20275] Appel à UpdateAfterReportSettings
            //Mise à jour en fonction du paramétrage du message
            UpdateAfterReportSettings(pDate, pDate2);

            //Mise à jour du DataDocument 
            if (pCnfMessage.NotificationType.HasValue)
                UpdateDataDocument2(pCS, pCnfMessage.NotificationType.Value);

            // FI 20150331 [XXPOC] Call SetUnspecifiedField
            SetUnspecifiedField();
        }

        /// <summary>
        ///  Ajoute une liste de trade dans le document de messagerie de manière asynchrone
        /// </summary>
        /// FI 20200520 [XXXXX] Mise en place de Task
        /// FI 20200606 [XXXXX] Refactoring PLINQ (AsParallel)
        /// FI 20211123 [XXXXX] Remove previons Refactoring (Refactoring PLINQ (AsParallel))
        private static async Task MsgDocAddTradeAsync(string pCS, DataDocumentContainer pMsgDataDocument, int[] pIdT,
           NotificationClassEnum pNotificationClass, Nullable<NotificationTypeEnum> pNotificationType)
        {

            List<Task<DataDocumentContainer>> lst = new List<Task<DataDocumentContainer>>();
            foreach (int item in pIdT)
            {
                lst.Add(
                         Task<DataDocumentContainer>.Run(() =>
                         {
                             return BuildTrade(pCS, item, pNotificationClass, pNotificationType);
                         }
                         ));
            }
            // FI 20220518 [XXXXX] use ConfigureAwait(false)
            await Task.WhenAll(lst.ToArray()).ConfigureAwait(false);
            
            foreach (Task<DataDocumentContainer> item in lst)
            {
                // Ajout du trade en cours dans la collection des Trades du msgDataDocument
                pMsgDataDocument.AddTrade(item.Result.CurrentTrade);

                //Ajout des Parties du Trade en cours dans les Parties du msgDataDocument
                foreach (IParty party in item.Result.Party)
                    pMsgDataDocument.AddParty(party);
            }

        }

        /// <summary>
        ///  Initialisation de RptSide pour les produits pour lesquels il n'existe pas en natif (ReturnSwap, FxLeg,etc...) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="tradeDataDocument"></param>
        /// FI 20130331 [XXPOC] add Method
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        private static void InitRptSide(string pCS, DataDocumentContainer tradeDataDocument)
        {
            // FI 20170116 [21916] call tradeDataDocument.currentProduct.RptSide(pCS, true);
            RptSideProductContainer rptSideProductContainer = tradeDataDocument.CurrentProduct.RptSide(pCS, true);
            if (null != rptSideProductContainer)
                tradeDataDocument.CurrentProduct.SynchronizeFromDataDocument();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pRouting"></param>
        private void AddSendTo(string pCs, DataDocumentContainer pDataDocument, IRouting pRouting)
        {
            ReflectionTools.AddItemInArray(this.NotificationDocument.Header, "sendTo", 0);
            INotificationRouting confirmRouting = NotificationDocument.Header.SendTo[ArrFunc.Count(this.NotificationDocument.Header.SendTo) - 1];
            ConfirmationTools.InitializeConfirmationRouting(pRouting, confirmRouting);
            //href on party
            AddHRefPartyOnConfirmationRouting(pCs, pDataDocument, confirmRouting);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pRouting"></param>
        private void AddCopyTo(string pCs, DataDocumentContainer pDataDocument, IRouting pRouting)
        {
            ReflectionTools.AddItemInArray(this.NotificationDocument.Header, "copyTo", 0);
            INotificationRouting confirmRouting = NotificationDocument.Header.CopyTo[ArrFunc.Count(this.NotificationDocument.Header.CopyTo) - 1];
            ConfirmationTools.InitializeConfirmationRouting(pRouting, confirmRouting);
            //href on party
            AddHRefPartyOnConfirmationRouting(pCs, pDataDocument, confirmRouting);
            //
            NotificationDocument.Header.CopyToSpecified = ArrFunc.IsFilled(NotificationDocument.Header.CopyTo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pConfirmationRouting"></param>
        private static void AddHRefPartyOnConfirmationRouting(string pCs, DataDocumentContainer pDataDocument, INotificationRouting pConfirmationRouting)
        {
            RoutingContainer routingContainer = new RoutingContainer(pConfirmationRouting);
            int ida = routingContainer.GetRoutingIdA(pCs);
            if (ida > 0)
            {
                IParty party = pDataDocument.GetParty(ida.ToString(), PartyInfoEnum.OTCmlId);
                pConfirmationRouting.HRefSpecified = (null != party);
                if (pConfirmationRouting.HRefSpecified)
                    pConfirmationRouting.HRef = party.Id;
            }

        }

        /// <summary>
        /// Affecte {pConfirmationRouting.partyReference} si l'acteur {pIdA} est présent est une partie dans le DataDocument
        /// </summary>
        /// <param name="pCs"></param>
        /// <param name="pDataDocument"></param>
        /// <param name="pConfirmationRouting"></param>
        /// <param name="pIdA"></param>
        private void AddPartyReferenceConfirmationRouting(DataDocumentContainer pDataDocument, INotificationRouting pConfirmationRouting, int pIdA)
        {
            IParty party = pDataDocument.GetParty(pIdA.ToString(), PartyInfoEnum.OTCmlId);
            pConfirmationRouting.PartyReferenceSpecified = (null != party);
            if (pConfirmationRouting.PartyReferenceSpecified)
            {
                pConfirmationRouting.PartyReference = NotificationDocument.CreatePartyReference();
                pConfirmationRouting.PartyReference.HRef = party.Id;
            }
        }

        /// <summary>
        /// Retourne les identifications du message
        /// </summary>
        /// <param name="pCnfMessage"></param>
        /// <returns></returns>
        private static NotificationId[] GetNotificationId(CnfMessage pCnfMessage)
        {
            NotificationId[] ret = null;

            ArrayList al = new ArrayList
            {
                new NotificationId(Cst.OTCml_CNFMessageIdScheme, pCnfMessage.idCnfMessage.ToString()),
                new NotificationId(Cst.OTCml_CNFMessageIdentifierScheme, pCnfMessage.identifier),
                new NotificationId(Cst.OTCml_CNFMessageMSGTypeScheme, pCnfMessage.msgType)
            };
            if (StrFunc.IsFilled(pCnfMessage.extlLink))
                al.Add(new NotificationId(Cst.OTCml_CNFMessageExtlLinkScheme, pCnfMessage.extlLink));

            if (ArrFunc.IsFilled(al))
                ret = (NotificationId[])al.ToArray(typeof(NotificationId));

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNcs"></param>
        /// <returns></returns>
        private static NotificationConfirmationSystemId[] GetNotificationConfirmationSystemId(NotificationConfirmationSystem pNcs)
        {

            NotificationConfirmationSystemId[] ret = null;
            //
            ArrayList al = new ArrayList
            {
                new NotificationConfirmationSystemId(Cst.OTCml_NotificationConfirmationSystemIdScheme, pNcs.idNcs.ToString()),
                new NotificationConfirmationSystemId(Cst.OTCml_NotificationConfirmationSystemIdentifierScheme, pNcs.identifier)
            };
            if (StrFunc.IsFilled(pNcs.extlLink))
                al.Add(new NotificationConfirmationSystemId(Cst.OTCml_NotificationConfirmationSystemIdentifierScheme, pNcs.extlLink));
            if (ArrFunc.IsFilled(al))
                ret = (NotificationConfirmationSystemId[])al.ToArray(typeof(NotificationConfirmationSystemId));
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSettings"></param>
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void SetInvoicingTradeDetails(string pCS, NotificationDocumentSettings pSettings)
        {
            // RD 20130129 [18259] Enrichissement de l'élément Repository avec les infos des trades inclus dans une facture
            DataDocumentContainer cnfdataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);

            IInvoice invoice = (IInvoice)cnfdataDocument.CurrentProduct.Product;
            ArrayList aTradeDetails = new ArrayList();

            foreach (IInvoiceTrade trade in invoice.InvoiceDetails.InvoiceTrade)
            {

                try
                {

                    int[] idGInstr = null;
                    bool isDetailRequested = false;

                    #region Lecture du trade de marché et de CNFMESSAGEDET
                    SQL_TradeTransaction sql_Trade = new SQL_TradeTransaction(pCS, trade.OTCmlId)
                    {
                        IsWithTradeXML = true
                    };
                    if (sql_Trade.IsLoaded)
                    {
                        //FI 20110622 [17486] Il faut un paramétrage ds CNFMESSAGEDET sur extlLink pour afficher le trade dans le detail
                        //isDetailRequested = StrFunc.IsFilled(sql_Trade.ExtlLink);
                        // Lecture des paramètres d'enrichissement de CNFMESSAGEDET pour les faxctures
                        // en fonction de l'instrument du trade
                        // FI 20200520 [XXXXX] Add SQL cache
                        idGInstr = InstrTools.GetGrpInstr(CSTools.SetCacheOn(pCS), sql_Trade.IdI, RoleGInstr.CNF, true);
                        if (ArrFunc.IsEmpty(idGInstr))
                            idGInstr = new int[1] { 0 };

                        isDetailRequested = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI);
                    }
                    #endregion Lecture du trade de marché et de CNFMESSAGEDET
                    if (isDetailRequested)
                    {
                        // Désérialisation du trade
                        EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(sql_Trade.TradeXml);
                        IDataDocument dataDocument = (IDataDocument)CacheSerializer.Deserialize(serializeInfo);
                        //
                        DataDocumentContainer dataDocumentContainer = new DataDocumentContainer(dataDocument);
                        IConfirmationTradeDetail tradeDetail = NotificationDocument.CreateTradeDetail();
                        tradeDetail.OtcmlId = trade.OtcmlId;
                        //
                        #region TradeHeader
                        tradeDetail.TradeHeaderSpecified = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI, InvoicingTradeDetailEnum.header);
                        if (tradeDetail.TradeHeaderSpecified)
                            tradeDetail.TradeHeader = dataDocumentContainer.TradeHeader;
                        #endregion TradeHeader
                        //
                        #region Product
                        tradeDetail.ProductSpecified = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI, InvoicingTradeDetailEnum.product);
                        if (tradeDetail.ProductSpecified)
                        {
                            tradeDetail.Product = tradeDetail.CreateProduct();
                            tradeDetail.Product.ProductSpecified = true;
                            tradeDetail.Product.Product = (IProductBase)dataDocumentContainer.CurrentProduct.Product;
                        }
                        #endregion Product
                        //
                        #region OtherPartyPayment
                        if (dataDocumentContainer.OtherPartyPaymentSpecified)
                        {
                            tradeDetail.OtherPartyPaymentSpecified = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI, InvoicingTradeDetailEnum.otherPartyPayment);
                            if (tradeDetail.OtherPartyPaymentSpecified)
                                tradeDetail.OtherPartyPayment = dataDocumentContainer.OtherPartyPayment;
                        }
                        #endregion OtherPartyPayment
                        //
                        #region Extends
                        if (dataDocumentContainer.ExtendsSpecified)
                        {
                            tradeDetail.ExtendsSpecified = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI, InvoicingTradeDetailEnum.extends);
                            if (tradeDetail.ExtendsSpecified)
                                tradeDetail.Extends = dataDocumentContainer.Extends;
                        }
                        #endregion Extends
                        //
                        #region ExternalLink
                        if (StrFunc.IsFilled(sql_Trade.ExtlLink))
                        {
                            //FI 20110622 [17486] externalLink du trade détail n'est affiché que si demandé par le paramétrage
                            tradeDetail.ExternalLinkSpecified = pSettings.IsMessageDetRequested(sql_Trade.IdP, idGInstr[0], sql_Trade.IdI, InvoicingTradeDetailEnum.extlLink);
                            if (tradeDetail.ExternalLinkSpecified)
                                tradeDetail.ExternalLink = sql_Trade.ExtlLink;
                        }
                        #endregion ExternalLink
                        //
                        #region Repository
                        // Enrichissement du Repository de chaque trade attaché
                        // RD 20130722 [18745] Synthesis Report
                        //dataDocumentContainer.SetRepository(pCs, cnfdataDocument, null, null, NotificationDocument.EfsMLversion, DateTime.MinValue);
                        // RD 20151002 [21426] Spot rate in Invoice Report
                        ICurrency reportingCurrency = dataDocumentContainer.CurrentProduct.ProductBase.CreateCurrency(this.ReportCurrency.Second);
                        Nullable<DateTime> dtFixing = invoice.InvoiceDate.DateValue; ;
                        // FI 20200520 [XXXXX] Add SQL cache
                        dataDocumentContainer.SetRepository(CSTools.SetCacheOn(pCS), cnfdataDocument, reportingCurrency, dtFixing, NotificationDocument.EfsMLversion, DateTime.MinValue);
                        #endregion Repository

                        aTradeDetails.Add(tradeDetail);
                    }
                }
                catch (Exception ex)
                {
                    // FI 20240430 [XXXXX] Ecriture dans le log de l'exception
                    string tradeId = LogTools.IdentifierAndId((null != trade.Identifier) ? trade.Identifier.Value : string.Empty, trade.OTCmlId);
                    Logger.Log(new LoggerData(LogLevelEnum.Error, $"An error occured on trade: <b>{tradeId}</b>. Error Message: <b>{ExceptionTools.GetMessageExtended(ex)}</b>"));
                    throw;
                }
            }
            NotificationDocument.DetailsSpecified = (0 < aTradeDetails.Count);
            if (NotificationDocument.DetailsSpecified)
                SetDetails(aTradeDetails);
        }

        /// <summary>
        /// Alimente confirmationMessageDoc.tradeSorting
        /// <para>Cette méhode n'effectue aucun tri</para>
        /// </summary>
        /// <param name="pOrderBy"></param>
        private void SetTradeSorting(ReportSortEnum[] pOrderBy)
        {

            ReportTradeSort tradeSort = null;

            if (ArrFunc.IsFilled(pOrderBy))
            {
                //Create ReportTradeSort
                tradeSort = new ReportTradeSort
                {

                    //Alimentation de Keys 
                    keys = CreateReportTradeSortKeys(ArrFunc.Count(pOrderBy))
                };
                //
                for (int i = 0; i < ArrFunc.Count(pOrderBy); i++)
                {
                    tradeSort.keys.key[i].scheme = Cst.OTCml_RepositoryReportTradeSortScheme;
                    tradeSort.keys.key[i].Value = pOrderBy[i].ToString();
                    tradeSort.keys.key[i].Id = "sortKey" + ((int)(i + 1)).ToString();
                }
            }

            NotificationDocument.TradeSortingSpecified = (null != tradeSort);
            if (NotificationDocument.TradeSortingSpecified)
                NotificationDocument.TradeSorting = tradeSort;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTradeDetails"></param>
        private void SetDetails(ArrayList pTradeDetails)
        {
            if (null == NotificationDocument.Details)
                NotificationDocument.Details = NotificationDocument.CreateConfirmationMessageDetails();

            IConfirmationTradeDetail[] detail = (IConfirmationTradeDetail[])pTradeDetails.ToArray(typeof(IConfirmationTradeDetail));
            NotificationDocument.Details.TradeDetail = detail;

        }

        /// <summary>
        /// Alimentation de confirmationMessageDoc.Header
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdMCO"></param>
        /// <param name="pDate"></param>
        /// <param name="pDate2"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pNcs"></param>
        /// <param name="pConfirmationChain"></param>
        /// <param name="pInciItem">Liste des destinataires</param>
        /// <param name="pIdInci_SendBy"></param>
        /// <param name="pAppInstance"></param>
        /// FI 20120829 [18048] modification de la signature de la fonction => ajout de pDate2
        /// FI 20150930 [21311] Modify
        /// FI 20171120 [23580] Modify
        private void SetHeader(string pCS,
            int pIdMCO, DateTime pDate,
            Nullable<DateTime> pDate2,
            CnfMessage pCnfMessage,
            NotificationConfirmationSystem pNcs,
            ConfirmationChain pConfirmationChain,
            InciItem[] pInciItem,
            int pIdInci_SendBy,
            AppInstance pAppInstance)
        {
            NotificationId[] messageId = GetNotificationId(pCnfMessage);
            NotificationConfirmationSystemId[] notificationConfirmationSystemId = GetNotificationConfirmationSystemId(pNcs);
            
            DataDocumentContainer dataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);
            
            NotificationDocument.Header = NotificationDocument.CreateNotificationHeader();
            //Id unique pour la transaction 
            NotificationDocument.Header.OTCmlId = pIdMCO;
            //stlMessageId Type de Message (202,210 etc...)
            NotificationDocument.Header.ConfirmationMessageId = messageId;
            //NCS
            NotificationDocument.Header.NotificationConfirmationSystemIds = new NotificationConfirmationSystemIds(notificationConfirmationSystemId);


            //Date Creation
            // FI 20140808 [20549] add date au format UTC  
            DateTime dtCreation = OTCmlHelper.GetDateSys(pCS, out DateTime dtCreationUTC);

            //FI 20171120 [23580] dtCreation exprimée dans le timeZone de l'acteur émetteur  
            SQL_Actor sqlActorSendBy = pConfirmationChain[SendEnum.SendBy].sqlActor;
            if (StrFunc.IsFilled(sqlActorSendBy.TimeZone))
            {
                TimeZoneInfo tzInfoTarget = Tz.Tools.GetTimeZoneInfoFromTzdbId(sqlActorSendBy.TimeZone);
                dtCreation = TimeZoneInfo.ConvertTimeFromUtc(dtCreationUTC, tzInfoTarget);
            }

            NotificationDocument.Header.CreationTimestamp = new EFS_DateTimeUTC
            {
                DateTimeValue = dtCreation,
                DateTimeValueUTC = dtCreationUTC,
                tzdbIdSpecified = StrFunc.IsFilled(sqlActorSendBy.TimeZone)
            };
            //FI 20171120 [23580] Alimentation de tzdbId
            if (NotificationDocument.Header.CreationTimestamp.tzdbIdSpecified)
                NotificationDocument.Header.CreationTimestamp.tzdbId = sqlActorSendBy.TimeZone;

            //Date Value
            NotificationDocument.Header.ValueDate = new EFS_Date
            {
                DateValue = pDate
            };

            //FI 20120829 [18048] alimentation de valueDate2
            NotificationDocument.Header.ValueDate2 = new EFS_Date();
            NotificationDocument.Header.ValueDate2Specified = pDate2.HasValue;
            if (NotificationDocument.Header.ValueDate2Specified)
                NotificationDocument.Header.ValueDate2.DateValue = pDate2.Value;

            //SendBy
            int idASendBy = pConfirmationChain[SendEnum.SendBy].IdActor;
            NotificationRoutingActorsBuilder actorInfoSendBy = new NotificationRoutingActorsBuilder(NotificationDocument.CreateRoutingCreateElement());
            // FI 20190515 [23912] Mise en place du cache SQL
            actorInfoSendBy.Load(CSTools.SetCacheOn(pCS), new int[] { idASendBy });
            IRouting routingSendBy = actorInfoSendBy.GetRouting(idASendBy);
            if (null != routingSendBy)
            {
                NotificationDocument.Header.SendBy = this.NotificationDocument.CreateNotificationRouting();
                ConfirmationTools.InitializeConfirmationRouting(routingSendBy, NotificationDocument.Header.SendBy);
                //
                // FI 20200520 [XXXXX] Add SQL cache
                AddHRefPartyOnConfirmationRouting(CSTools.SetCacheOn(pCS), dataDocument, NotificationDocument.Header.SendBy);
                //
                //FI 20120528 isSendTo_Client était tjs à false comme isSendTo_Broker
                //Aujourd'hui il sont valorisés du fait de la correction en même data, avant il y leur valeur était tjs false
                //Pour ne pas changer le comportement de Spheres®, le 1er if est mis en commentaire
                //if (pConfirmationChain.isSendTo_Client(pCS) || pConfirmationChain.isSendTo_Broker)
                //    AddPartyReferenceConfirmationRouting(pCS, dataDocument, confirmationMessageDoc.header.sendBy, pConfirmationChain[SendEnum.SendTo].idActor);
                //else
                // FI 20200520 [XXXXX] Add SQL cache
                AddPartyReferenceConfirmationRouting(dataDocument, NotificationDocument.Header.SendBy, idASendBy);
            }

            //SendTo, ReportCurrency
            string idCReportCurrency = string.Empty;
            bool isFirstTo = false;
            for (int i = 0; i < ArrFunc.Count(pInciItem); i++)
            {
                NotificationRoutingActorsBuilder actorInfoSendTo =
                    new NotificationRoutingActorsBuilder(pInciItem[i].addressIdent, dataDocument.CurrentProduct.IdI, NotificationDocument.CreateRoutingCreateElement());
                // FI 20190515 [23912] Mise en place du cache SQL
                actorInfoSendTo.Load(CSTools.SetCacheOn(pCS), new int[] { pInciItem[i].idA });
                //
                IRouting routing = actorInfoSendTo.GetRouting(pInciItem[i].idA);
                //
                if (null != routing)
                {
                    if (pInciItem[i].isTo)
                    {
                        // FI 20200520 [XXXXX] Add SQL cache
                        AddSendTo(CSTools.SetCacheOn(pCS), dataDocument, routing);
                        //
                        if (false == isFirstTo)
                            isFirstTo = true;
                        if (isFirstTo)
                        {
                            if (StrFunc.IsEmpty(Culture))
                                Culture = actorInfoSendTo.GetCulture(pInciItem[i].idA);
                            if (StrFunc.IsEmpty(idCReportCurrency))
                                idCReportCurrency = actorInfoSendTo.GetReportCurrency(pInciItem[i].idA);
                        }
                    }
                    
                    if (pInciItem[i].isCC)
                    {
                        // FI 20200520 [XXXXX] Add SQL cache
                        AddCopyTo(CSTools.SetCacheOn(pCS), dataDocument, routing);
                    }

                    if (pInciItem[i].isBCC)
                    {
                        // FI 20200520 [XXXXX] Add SQL cache
                        AddCopyTo(CSTools.SetCacheOn(pCS), dataDocument, routing);
                    }
                }
            }


            //ReportCurrency            
            //La devise de reporting est 
            // - la devise de contrevaleur lorsqu'elle existe (ou)
            // - la devise de reporting du destinataire (ou)
            // - la devise de reporting de l'entité 
            if (dataDocument.CurrentProduct.IsCashBalance)
            {
                CashBalance cashBalance = (CashBalance)dataDocument.CurrentProduct.Product;
                if (cashBalance.exchangeCashBalanceStreamSpecified)
                    idCReportCurrency = Tools.GetIdC(pCS, cashBalance.exchangeCashBalanceStream.currency.Value);
            }
            if (StrFunc.IsEmpty(idCReportCurrency))
            {
                SQL_Actor sqlSendTo = pConfirmationChain[SendEnum.SendTo].sqlContactOffice;
                if (StrFunc.IsFilled(sqlSendTo.IdCCnf))
                    idCReportCurrency = sqlSendTo.IdCCnf;
                //
                SQL_Actor sqlSendBy = pConfirmationChain[SendEnum.SendBy].sqlContactOffice;
                if (StrFunc.IsFilled(sqlSendBy.IdCCnf))
                    idCReportCurrency = sqlSendBy.IdCCnf;
            }
            // La devise de référence "EUR" par défaut défaut
            if (StrFunc.IsEmpty(idCReportCurrency))
                idCReportCurrency = "EUR";

            //FI 20150930 [21311] Alimentation de this.reportCurrency de type Pair
            SQL_Currency sqlCurrency = new SQL_Currency(CSTools.SetCacheOn(pCS), SQL_Currency.IDType.IdC, idCReportCurrency);
            if (false == sqlCurrency.LoadTable(new string[] { "IDC", "ISO4217_ALPHA3" }))
                throw new NullReferenceException(StrFunc.AppendFormat("Currency [IDC:{0}] is not found", idCReportCurrency));
            this.ReportCurrency = new Pair<string, string>(idCReportCurrency, sqlCurrency.Iso4217);

            //SoftWare
            NotificationDocument.Header.SoftApplicationSpecified = true;
            NotificationDocument.Header.SoftApplication.Name = new EFS_String(pAppInstance.AppName);
            NotificationDocument.Header.SoftApplication.Version = new EFS_String(pAppInstance.AppVersion);

            // RD 20130722 [18745] Synthesis Report
            if (NotificationDocument.EfsMLversion <= EfsMLDocumentVersionEnum.Version30)
            {
                NotificationDocument.Header.ReportCurrencySpecified = true;
                NotificationDocument.Header.ReportCurrency = dataDocument.CurrentProduct.ProductBase.CreateCurrency(ReportCurrency.Second);
            }
            else
            {
                NotificationDocument.Header.ReportSettingsSpecified = true;
                if (NotificationDocument.Header.ReportSettings == null)
                    NotificationDocument.Header.ReportSettings = new ReportSettings();

                NotificationDocument.Header.ReportSettings.reportCurrencySpecified = true;
                NotificationDocument.Header.ReportSettings.reportCurrency = ReportCurrency.Second;
            }


            //ReportingFeeDisplay:
            SetReportFeeDisplay(CSTools.SetCacheOn(pCS), dataDocument);

            // RD 20130722 [18745] Synthesis Report
            if (NotificationDocument.EfsMLversion <= EfsMLDocumentVersionEnum.Version30)
            {
                //MF 20120904 Ticket 18106 - the display style must exist if we want to generate the transaction report as the postion actions report
                //confirmationMessageDoc.header.reportFeeDisplaySpecified = (this.reportFeeDisplay != MessageBuilderFeeDisplayEnum.NA);
                NotificationDocument.Header.ReportFeeDisplaySpecified = true;
                NotificationDocument.Header.ReportFeeDisplay = this.ReportFeeDisplay.ToString();
            }
            else
            {
                NotificationDocument.Header.ReportSettingsSpecified = true;
                if (NotificationDocument.Header.ReportSettings == null)
                    NotificationDocument.Header.ReportSettings = new ReportSettings();

                NotificationDocument.Header.ReportSettings.reportFeeDisplaySpecified = true;
                NotificationDocument.Header.ReportSettings.reportFeeDisplay = this.ReportFeeDisplay.ToString();
            }

            //PL 20091231 New ADDRESSCOMPL.CULTURE
            if (StrFunc.IsEmpty(Culture))
            {
                Culture = pConfirmationChain.GetDefaultLanquage(CSTools.SetCacheOn(pCS));
            }
            if (StrFunc.IsEmpty(Culture))
            {
                Culture = "en-GB";
            }

            SetReportSettings(CSTools.SetCacheOn(pCS), pCnfMessage, pIdInci_SendBy, pConfirmationChain[SendEnum.SendBy].IdAContactOffice);

            UpdateHTitle(pDate, pDate2);

        }

        /// <summary>
        /// Interprétation des mots clés potentiellement présent dans le paramétragde du titre du report
        /// <para>Spheres® met à disposition plusieurs mots clés pouvant être utilisé pour constituer le titre final</para>
        /// <param name="pDate"></param>
        /// <param name="pDate2"></param>
        /// </summary>
        /// FI 20150522 [XXXXX] Add metho
        /// FI 20160318 [XXXXX] Modify
        private void UpdateHTitle(DateTime pDate, Nullable<DateTime> pDate2)
        {
            if (NotificationDocument.Header.ReportSettingsSpecified)
            {
                ReportSettings settings = NotificationDocument.Header.ReportSettings;
                if (settings.headerFooterSpecified && StrFunc.IsFilled(settings.headerFooter.hTitle))
                {
                    CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(Culture);

                    Pair<Interval, string> interval = GetDateTimeInterval(pDate, pDate2 ?? pDate, cultureInfo);

                    // FI 20160318 [XXXXX] Passage du paramètre cultureInfo pour FMTDATE1 et FMTDATE2
                    settings.headerFooter.hTitle = StrFuncExtended.ReplaceObjectFieldBasic(settings.headerFooter.hTitle,
                                    new
                                    {
                                        FMTDATE1 = DtFunc.DateTimeToString(pDate, DtFunc.FmtShortDate, cultureInfo),
                                        FMTDATE2 = pDate2.HasValue ? DtFunc.DateTimeToString(pDate2.Value, DtFunc.FmtShortDate, cultureInfo) : DtFunc.DateTimeToString(pDate, DtFunc.FmtShortDate, cultureInfo),
                                        INTERVAL_PERIOD = interval.First.Period,
                                        INTERVAL_PERIODMLTP = interval.First.periodMultiplier.IntValue,
                                        INTERVAL_LBL = interval.Second
                                    }, cultureInfo);
                }
            }
        }


        /// <summary>
        /// Aliemente confirmationMessageDoc.dataDocument.Repository
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCnfMessage"></param>
        /// FI 20120830 [18048] dtFixing vaut date fin de période demandée sur les extraits de compte
        /// Modification de la signature de la fonction
        /// RD 20120927 [18150] dtFixing vaut date fin de période sur le Relevé d'intérêts, 
        /// qui correspond à la date du trade CashBalanceInterest
        /// FI 20140903 [20275] Modify
        /// FI 20150129 [20275] Modify
        /// FI 20150427 [20987] Modify
        /// FI 20150525 [20987] Modify
        /// RD 20160506 [21749] Modify
        /// FI 20160530 [21885] Modify
        /// FI 20160613 [22256] Modify
        /// FI 20170217 [22862] Modify
        private void SetRepository(string pCS, CnfMessage pCnfMessage)
        {
            DataDocumentContainer dataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);

            ICurrency reportingCurrency = dataDocument.CurrentProduct.ProductBase.CreateCurrency(this.ReportCurrency.Second);
            Nullable<DateTime> dtFixing = NotificationDocument.Header.ValueDate.DateValue;

            // RD 20131015 [19067] Extrait de compte / un taux de change par jour        
            //if (pCnfMessage.notificationType.HasValue && pCnfMessage.notificationType.Value == NotificationTypeEnum.FINANCIALPERIODIC)
            //    dtFixing = NotificationDocument.header.valueDate2.DateValue;
            if (dataDocument.CurrentProduct.IsCashBalanceInterest)
                dtFixing = dataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;


           // FI 20140903 [20275] add dtRefForDtEnbled
            DateTime dtRefForDtEnabled = NotificationDocument.Header.ValueDate.DateValue;

            for (int i = 0; i < ArrFunc.Count(dataDocument.Trade); i++)
            {
                dataDocument.SetCurrentTrade(dataDocument.Trade[i]);

                // RD 20131015 [19067] Extrait de compte / un taux de change par jour        
                if (pCnfMessage.NotificationType.HasValue)
                {
                    switch (pCnfMessage.NotificationType.Value)
                    {
                        case NotificationTypeEnum.FINANCIALPERIODIC:
                        case NotificationTypeEnum.SYNTHESIS: // FI 20150522 [20987] add
                            dtFixing = dataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;
                            //FI 20140903 [20275] dtRefForDtEnabled
                            dtRefForDtEnabled = dataDocument.CurrentTrade.TradeHeader.TradeDate.DateValue;

                            //RD 20160506 [21749] For trade resulting from CA, use ClearingBusinessDate as dtRefForDtEnabled, 
                            // because the new DC is enabled from CA date.
                            if (dataDocument.CurrentProduct.ProductBase.IsExchangeTradedDerivative)
                            {
                                ExchangeTradedContainer product = new ExchangeTradedContainer((IExchangeTradedDerivative)dataDocument.CurrentProduct.Product);
                                if (product.TradeCaptureReport.TrdType == TrdTypeEnum.CorporateAction)
                                    dtRefForDtEnabled = product.TradeCaptureReport.ClearingBusinessDate.DateValue;
                            }
                            break;
                    }
                }

                dataDocument.SetRepository(pCS, null, reportingCurrency, dtFixing, NotificationDocument.EfsMLversion, dtRefForDtEnabled);
            }

            // FI 20150129 [20275] Add Alimentation de Repository à partir d'éléments non présents dans le NotificationDocument.dataDocument
            // FI 20150427 [20987] usage du from from et du distinct
            if (_notificationDocument.CashPaymentsSpecified)
            {
                var paymentType = (from cashPayments in _notificationDocument.CashPayments
                                   from cashPayment in cashPayments.cashPayment
                                   where cashPayment.paymentTypeSpecified == true
                                   select cashPayment.paymentType).Distinct();

                foreach (string item in paymentType)
                    RepositoryTools.AddEnumRepository(pCS, dataDocument.Repository, "PaymentType", item);

                // FI 20160530 [21885] add book in repository
                var idB = (from cashPayments in _notificationDocument.CashPayments
                           from cashPayment in cashPayments.cashPayment
                           select cashPayment.idb).Distinct();
                foreach (int item in idB)
                    RepositoryTools.AddBookRepository(pCS, dataDocument.Repository, item);
            }

            // FI 20150709 [XXXXX] Add (GLOP, il faudra faire mieux vu les lignes précédentes)
            if (_notificationDocument.StlPosSyntheticsSpecified)
            {
                var paymentType = (from stlPosSynthetics in _notificationDocument.StlPosSynthetics
                                   from posSynthetic in stlPosSynthetics.posSynthetic.Where(x => x.skpSpecified)
                                   from skp in posSynthetic.skp
                                   where skp.paymentTypeSpecified == true
                                   select skp.paymentType).Distinct();
                foreach (string item in paymentType)
                    RepositoryTools.AddEnumRepository(pCS, dataDocument.Repository, "PaymentType", item);
            }



            //FI 20150407 [XXPOC]  Les trades Fx (fpml) n'ont pas réellement des assets mais plutôt des paires de devise 
            //On ajoute l'asset par défaut pour la pair de devise concernée qui se trouve dans TRADEINSTRUMENT
            //Ce dernier n'est pas présente dans le trade fpML
            // FI 20150427 [20987] Modify
            if (_notificationDocument.TradesSpecified || _notificationDocument.PosTradesSpecified)
            {
                // FI 20150623 [21149] use GetAllTrades and GetAllPosTrades
                /*var trade = 
                (from tradesItem in NotificationDocument.trades
                 from item in tradesItem.trade.Where(t => t.gProduct == Cst.ProductGProduct_FX)
                 select (PosTradeCommon)item).Concat(
                    from posTradesItem in NotificationDocument.posTrades
                    from item in posTradesItem.trade.Where(t => t.gProduct == Cst.ProductGProduct_FX)
                    select (PosTradeCommon)item);*/
                var trade = (from item in GetAllTrades(null).Where(t => t.gProduct == Cst.ProductGProduct_FX)
                             select (PosTradeCommon)item).Concat(
                                from posTradesItem in GetAllPosTrades(null).Where(t => t.gProduct == Cst.ProductGProduct_FX)
                                select (PosTradeCommon)posTradesItem);

                if (trade.Count() > 0)
                {
                    var asset = (from item in trade
                                 where (item.idAssetSpecified)
                                 select new { item.assetCategory, item.idAsset }).Distinct();

                    foreach (var item in asset)
                        RepositoryTools.AddAssetRepository(pCS, dataDocument.Repository, new Pair<Cst.UnderlyingAsset, int>(item.assetCategory, item.idAsset));
                }
            }

            // FI 20150807 [XXXXX]  sur un report monthly il peut exister des positions avec des assets pour lesquels il n'existe pas de trade dans le dataDocument
            if (NotificationDocument.PosSyntheticsSpecified || NotificationDocument.StlPosSyntheticsSpecified)
            {
                // asset déjà présent dans le repository
                var assetRepository = (from item in ((IRepositoryDocument)dataDocument.DataDocument).GetAllRepositoryAsset()
                                       select (
                                       new
                                       {
                                           IdAsset = item.OTCmlId,
                                           item.AssetCategory
                                       })).Distinct().ToList();

                // asset présents dans toutes les positions synthétiques
                var assetPosSynthetics = (from item in GetAllPositionSynthetic(null)
                                          select (
                                            new
                                            {
                                                item.IdAsset,
                                                item.AssetCategory
                                            })).Distinct().ToList();

                // Ajout des assets non encore présents 
                foreach (var item in assetPosSynthetics)
                {
                    if (false == assetRepository.Contains(item))
                        RepositoryTools.AddAssetRepository(pCS, dataDocument.Repository, new Pair<Cst.UnderlyingAsset, int>(item.AssetCategory, item.IdAsset));
                }
            }

            // FI 20160613 [22256] Ajout des asset Equity présents sous NotificationDocument.underlyingStocks
            if (NotificationDocument.UnderlyingStocksSpecified)
            {
                // Ajout des assets non encore présents 
                var assetEquity = (from underlyingStocksItem in NotificationDocument.UnderlyingStocks
                                   from underlyingStockItem in underlyingStocksItem.underlyingStock
                                   select (new
                                               {
                                                   underlyingStockItem.idAsset
                                               })).Distinct().ToList();

                foreach (var item in assetEquity)
                    RepositoryTools.AddAssetRepository(pCS, dataDocument.Repository, new Pair<Cst.UnderlyingAsset, int>(Cst.UnderlyingAsset.EquityAsset, item.idAsset));

                // Ajout des books non encore présents 
                var book = (from underlyingStocksItem in NotificationDocument.UnderlyingStocks
                            from underlyingStockItem in underlyingStocksItem.underlyingStock
                            select (new
                            {
                                underlyingStockItem.idB
                            })).Distinct().ToList();

                // Ajout des books non encore présents 
                foreach (var item in book)
                    RepositoryTools.AddBookRepository(pCS, dataDocument.Repository, item.idB);
            }

            // FI 20190515 [23912] Call SetRepositoryActorRoutingId 
            SetRepositoryActorRoutingId(pCS, dataDocument.Repository);

            // Sur les documents >= version 3.5 l'élément repository est au même niveau que dataDocument. Ce n'est pas un enfant
            if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
            {
                dataDocument.RepositorySpecified = false;

                NotificationDocument.RepositorySpecified = true;
                NotificationDocument.Repository = dataDocument.Repository;
            }
        }
        
        /// <summary>
        /// Alimentation pour chaque acteur du repository des email, téléphone, addresses, etc.. 
        /// </summary>
        /// FI 20190515 [23912] Add Method 
        public static void SetRepositoryActorRoutingId(string pCS, IRepository pRepository)
        {
            if (pRepository.ActorSpecified)
            {
                int[] idA = (from item in pRepository.Actor
                             select item.OTCmlId).ToArray();

                NotificationRoutingActorsBuilder builder = new NotificationRoutingActorsBuilder(new RoutingCreateElement())
                {
                    ScanDataDtEnabled = ActorsAddressBase.ScanDataDtEnabledEnum.No
                };
                builder.Load(CSTools.SetCacheOn(pCS), idA);

                foreach (int itemIdA in idA)
                {
                    IActorRepository actorRepository = pRepository.Actor.Where(x => x.OTCmlId == itemIdA).FirstOrDefault();
                    if (null == actorRepository)
                        throw new InvalidOperationException(StrFunc.AppendFormat("Actor {0} doesn't exist in repository", itemIdA.ToString()));

                    string info = string.Empty;
                    // Telephone 
                    info = builder.GetPhoneNumber(itemIdA, ActorsAddressBase.PhoneNumberTypeEnum.Telephone);
                    actorRepository.TelephoneNumberSpecified = StrFunc.IsFilled(info);
                    if (actorRepository.TelephoneNumberSpecified)
                        actorRepository.TelephoneNumber = info;

                    // Mobile
                    info = builder.GetPhoneNumber(itemIdA, ActorsAddressBase.PhoneNumberTypeEnum.Mobilephone);
                    actorRepository.MobileNumberSpecified = StrFunc.IsFilled(info);
                    if (actorRepository.MobileNumberSpecified)
                        actorRepository.MobileNumber = info;

                    // Email
                    info = builder.GetEMail(itemIdA);
                    actorRepository.EmailSpecified = StrFunc.IsFilled(info);
                    if (actorRepository.EmailSpecified)
                        actorRepository.Email = info;

                    // Web
                    info = builder.GetWeb(itemIdA);
                    actorRepository.WebSpecified = StrFunc.IsFilled(info);
                    if (actorRepository.WebSpecified)
                        actorRepository.Web = info;

                    // Address
                    IRoutingIdsAndExplicitDetails routingIdsAndExplicitDetails = builder.GetRoutingIdsAndExplicitDetails(itemIdA);
                    actorRepository.AddressSpecified = routingIdsAndExplicitDetails.RoutingAddressSpecified;
                    if (actorRepository.AddressSpecified)
                    {
                        actorRepository.Address = routingIdsAndExplicitDetails.RoutingAddress;
                        if (null != actorRepository.Address.Country)
                            actorRepository.Address.Country.Scheme = null;
                    }
                }
            }
        }

        /// <summary>
        /// Alimente la règle d'affichage des frais
        ///<para>- BRO_FEE: Aucune distinction.</para>
        ///<para>- BRO_TRDCLRFEE: Distinction entre commissions de Trading et de Clearing.</para>
        ///<para>- TRDCLRBRO_FEE: Distinction entre courtages de Trading et de Clearing.</para>
        ///<para>- TRDCLRBRO_TRDCLRFEE: Distinction entre courtages de Trading et de Clearing et distinction entre commissions de Trading et de Clearing.</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// RD 20120926 [18146] Gérer deux nouvelles règles (Règles 5 et 6)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20200106 [XXXXX] ajout parenthèse fermante
        private void SetReportFeeDisplay(string pCS, DataDocumentContainer pDataDocument)
        {
            ReportFeeDisplay = ReportFeeDisplayEnum.NA;

            if (IsSetReportFeeDisplay(pDataDocument))
            {
                string sql = @"select distinct f.PAYMENTTYPE
                from dbo.FEE f
                where (f.PAYMENTTYPE in ('Brokerage','TradingBrokerage','ClearingBrokerage','Fee','TradingFee','ClearingFee') and " + OTCmlHelper.GetSQLDataDtEnabled(pCS, "f") + ")";
                //
                DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, sql, null);
                DataTable dt = ds.Tables[0];
                //
                DataRow[] drBrokerage = dt.Select("PAYMENTTYPE='Brokerage'");
                DataRow[] drTradingBrokerage = dt.Select("PAYMENTTYPE='TradingBrokerage'");
                DataRow[] drClearingBrokerage = dt.Select("PAYMENTTYPE='ClearingBrokerage'");
                DataRow[] drFee = dt.Select("PAYMENTTYPE='Fee'");
                DataRow[] drTradingFee = dt.Select("PAYMENTTYPE='TradingFee'");
                DataRow[] drClearingFee = dt.Select("PAYMENTTYPE='ClearingFee'");
                //
                bool isBrokerage = (ArrFunc.Count(drBrokerage) > 0);
                bool isTradingBrokerage = (ArrFunc.Count(drTradingBrokerage) > 0);
                bool isClearingBrokerage = (ArrFunc.Count(drClearingBrokerage) > 0);
                bool isFee = (ArrFunc.Count(drFee) > 0);
                bool isTradingFee = (ArrFunc.Count(drTradingFee) > 0);
                bool isClearingFee = (ArrFunc.Count(drClearingFee) > 0);
                // -------------------------------------------------------------
                // Règle 1:
                // -------------------------------------------------------------
                // Frais activés: 
                //  - Brokerage 
                //  - Fee
                // Frais désactivés ou inexistants:  
                //  - TradingBrokerage 
                //  - ClearingBrokerage
                //  - TradingFee 
                //  - ClearingFee
                if ((isBrokerage && isFee) && !(isTradingBrokerage || isClearingBrokerage || isTradingFee || isClearingFee))
                    ReportFeeDisplay = ReportFeeDisplayEnum.BRO_FEE;
                // -------------------------------------------------------------
                // Règle 2:
                // -------------------------------------------------------------
                // Frais activés: 
                //  - TradingBrokerage 
                //  - ClearingBrokerage
                //  - Fee
                // Frais désactivés ou inexistants: 
                //  - Brokerage 
                //  - TradingFee 
                //  - ClearingFee
                else if ((isTradingBrokerage && isClearingBrokerage && isFee) && !(isBrokerage || isTradingFee || isClearingFee))
                    ReportFeeDisplay = ReportFeeDisplayEnum.TRDCLRBRO_FEE;
                // -------------------------------------------------------------
                // Règle 3:
                // -------------------------------------------------------------
                // Frais activés:  
                //  - Brokerage 
                //  - TradingFee 
                //  - ClearingFee
                // Frais désactivés ou inexistants:
                //  - TradingBrokerage 
                //  - ClearingBrokerage
                //  - Fee
                else if ((isBrokerage && isTradingFee && isClearingFee) && !(isTradingBrokerage || isClearingBrokerage || isFee))
                    ReportFeeDisplay = ReportFeeDisplayEnum.BRO_TRDCLRFEE;
                // -------------------------------------------------------------
                // Règle 4:
                // -------------------------------------------------------------
                // Frais activés:  
                //  - TradingBrokerage 
                //  - ClearingBrokerage
                //  - TradingFee 
                //  - ClearingFee
                // Frais désactivés ou inexistants:
                //  - Brokerage 
                //  - Fee
                else if ((isTradingBrokerage && isClearingBrokerage && isTradingFee && isClearingFee) && !(isBrokerage || isFee))
                    ReportFeeDisplay = ReportFeeDisplayEnum.TRDCLRBRO_TRDCLRFEE;
                // -------------------------------------------------------------
                // Règle 5 (même comportement que la règle 2)
                // -------------------------------------------------------------
                // Frais activés: 
                //  - TradingBrokerage 
                //  - ClearingBrokerage 
                //  - TradingFee 
                //  - ClearingFee
                //  - Fee
                // Frais désactivés ou inexistants: 
                //  - Brokerage
                else if ((isTradingBrokerage && isClearingBrokerage && isFee && isTradingFee && isClearingFee) && !(isBrokerage))
                    ReportFeeDisplay = ReportFeeDisplayEnum.TRDCLRBRO_FEE;
                // -------------------------------------------------------------
                // Règle 6 (même comportement que la règle 3)
                // -------------------------------------------------------------
                // Frais activés: 
                //  - TradingBrokerage 
                //  - ClearingBrokerage  
                //  - Brokerage
                //  - TradingFee 
                //  - ClearingFee
                // Frais désactivés ou inexistants:
                //  - Fee
                else if ((isTradingBrokerage && isClearingBrokerage && isFee && isTradingFee && isClearingFee) && !(isBrokerage))
                    ReportFeeDisplay = ReportFeeDisplayEnum.BRO_TRDCLRFEE;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDim"></param>
        /// <returns></returns>
        private ReportTradeSortKeys CreateReportTradeSortKeys(int pDim)
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
        /// Alimente confirmationMessageDoc.dataDocument
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">idT du trade</param>
        /// <param name="pNotificationClass">classe de notification</param>
        /// <param name="pNotificationType">type de notification</param>
        /// FI 20150708 [XXXXX] Modify
        private void SetDataDocument(string pCS, int[] pIdT,
            NotificationClassEnum pNotificationClass, Nullable<NotificationTypeEnum> pNotificationType)
        {

            IDataDocument msgDataDoc = (IDataDocument)NotificationDocument.CreateEfsDocument();
            if (null != msgDataDoc.Trade)
                msgDataDoc.Trade = null;
            if (null != msgDataDoc.Party)
                msgDataDoc.Party = null;

            //le msgDataDocument peut contenir plusieurs trades
            DataDocumentContainer msgDataDocument = new DataDocumentContainer(msgDataDoc);

            // FI 20150427 [20987] => Ajout du trade fictif si SYNTHESIS periodic en tant que 1er trade
            if (null != _cashBalancePeriodic)
            {
                msgDataDocument.AddTrade(_cashBalancePeriodic.First.CurrentTrade);
                foreach (IParty party in _cashBalancePeriodic.First.Party)
                    msgDataDocument.AddParty(party); // FI 20150708 [XXXXX]
            }

            //Ajout des trades
            MsgDocAddTradeAsync(pCS, msgDataDocument, pIdT, pNotificationClass, pNotificationType).Wait();

            //Facturation 
            if (pNotificationClass == NotificationClassEnum.MONOTRADE)
            {
                // Ordonner la Facture, s'il s'agit du Product ADM. 
                // Dans une facture dataDocument.Trade ne contient qu'un trade (qui représente la facture) 
                if ((msgDataDocument.CurrentProduct.ProductBase.IsADM) &&
                        (false == msgDataDocument.CurrentProduct.IsInvoiceSettlement))
                {
                    msgDataDocument.CurrentProduct.SetInvoicingOrder(CSTools.SetCacheOn(pCS));
                    //RD 20130108 [18259] Alléger le flux XML d'un trade facturation
                    msgDataDocument.CurrentProduct.UpdateTradeforInvoicingReport();
                }
            }

            // Enrichissement de toutes les parties du msgDataDocument
            // FI 20200520 [XXXXX] Add SQL cache
            msgDataDocument.SetAdditionalInfoOnParty(CSTools.SetCacheOn(pCS));

            //Affectation du dataDocument du ConfirmationMessageDocumentContainer (de dataDocument contient plusieurs trades)
            NotificationDocument.DataDocument = msgDataDocument.DataDocument;
        }

        /// <summary>
        /// Alimente confirmationMessageDoc.events ou confirmationMessageDoc.hierarchicalEvents
        /// </summary>
        /// FI 20120830 [18048] Refactoring => add variable isLoadEventTrigger
        /// Le document ne contient plus nécessairement des évènements déclencheur
        /// C'est notamment le cas pour l'extrait de compte 
        private void SetEvent(string pCS, string pSessionId, CnfMessage pCnfMessage, int[] pIdT, DateTime pDate)
        {
            NotificationDocument.HierarchicalEventsSpecified = false;
            NotificationDocument.EventsSpecified = false;

            bool isLoadEvent = IsLoadEvent(pCnfMessage);
            if (isLoadEvent)
            {
                int[] idE = LoadEvents(pCS, pSessionId, pIdT, pCnfMessage, pDate, out int[] idETrigger);

                if (ArrFunc.IsFilled(idE))
                {
                    NotificationDocumentSettings settings = pCnfMessage.GetSettings();
                    Nullable<NotificationTypeEnum> notificationType = pCnfMessage.NotificationType;
                    _ = IsInvoiceMessage();
                    bool isReportPosAction = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSACTION));
                    bool isReportPosition = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSITION));
                    bool isReportPositionSynthetic = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSSYNTHETIC));
                    _ = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.ALLOCATION));
                    _ = (notificationType.HasValue &&
                        (
                        (notificationType.Value == NotificationTypeEnum.FINANCIAL) ||
                        (notificationType.Value == NotificationTypeEnum.FINANCIALPERIODIC)
                        ));

                    //
                    // Le flux XML sera enrichi par EventDet que pour
                    // - Les Report Position: 
                    //      * pour afficher la quantité restante en position (EVENTDET_ETD)
                    //      * pour afficher le cours de compensation (EVENTDET_ETD)
                    // - Le nouveau Scheme définit par l'utilisateur
                    //
                    bool isLoadEventDet = (isReportPosition || isReportPositionSynthetic || isReportPosAction);
                    bool isLoadEventFee = isReportPosAction;

                    DataSetEventLoadSettings dataSettings = new DataSetEventLoadSettings(true, settings.IsUseEventSi, isLoadEventDet, false, false, false, isLoadEventFee)
                    {
                        restricColEventEnum = DataSetEventLoadSettings.EventRestrictEnum.ISCNFMESSAGING,
                        isLoadEventSI = false
                    };
                    if (isLoadEventDet)
                    {
                        //
                        // Le flux XML sera enrichi par EventDet que pour l'Event LPC/UMG
                        // Si le besoin s'exprime à l'avenir pour d'autres Events, il sera probablement de remplacer le type string par string[] 
                        // dans DataSetEventLoadSettings.eventCodeWithDet

                        if (isReportPosition || isReportPositionSynthetic)
                        {
                            dataSettings.eventCodeWithDet = EventCodeFunc.LinkedProductClosing;
                            dataSettings.eventTypeWithDet = EventTypeFunc.UnrealizedMargin;
                        }
                    }

                    DataSetEvent dsEvent = new DataSetEvent(pCS);
                    dsEvent.Load(null, pSessionId, pIdT, idE, dataSettings);

                    NotificationDocument.HierarchicalEventsSpecified = (settings.IsUseChildEvents);
                    NotificationDocument.EventsSpecified = (false == settings.IsUseChildEvents);

                    if (NotificationDocument.EventsSpecified)
                    {
                        NotificationDocument.Events = dsEvent.GetEvents();
                        NotificationDocument.EventsSpecified = (null != NotificationDocument.Events) &&
                                                                  ArrFunc.IsFilled(NotificationDocument.Events.@event);
                        NotificationDocument.HierarchicalEventsSpecified = false;
                    }
                    else if (NotificationDocument.HierarchicalEventsSpecified)
                    {
                        ArrayList alEvent = new ArrayList();
                        for (int i = 0; i < ArrFunc.Count(idETrigger); i++)
                        {
                            HierarchicalEventContainer hEventContainer = dsEvent.GetChildEvents(idETrigger[i]);
                            if (null != hEventContainer)
                                alEvent.Add(hEventContainer.hEvent);
                        }
                        if (ArrFunc.IsFilled(alEvent))
                        {
                            if (null == NotificationDocument.HierarchicalEvents)
                                NotificationDocument.HierarchicalEvents = new HierarchicalEvents();
                            NotificationDocument.HierarchicalEvents.hEvent = (HierarchicalEvent[])alEvent.ToArray(typeof(HierarchicalEvent));
                        }
                        NotificationDocument.HierarchicalEventsSpecified = ArrFunc.IsFilled(NotificationDocument.HierarchicalEvents.hEvent);
                        NotificationDocument.EventsSpecified = false;
                    }
                }

                // 20120826 MF Ticket 18073 START
                Event[] eventsList = NotificationDocument.EventsSpecified ?
                    NotificationDocument.Events.@event : null;

                if (eventsList != null && NotificationDocument.HierarchicalEventsSpecified)
                {
                    eventsList = eventsList.Union(NotificationDocument.HierarchicalEvents.hEvent).ToArray();
                }
                else if (NotificationDocument.HierarchicalEventsSpecified)
                {
                    eventsList = NotificationDocument.HierarchicalEvents.hEvent;
                }
                // FI 20120829 null != eventsList => garde-fou pour ne pas planter dans AddConvertedEventsQuotePrices
                // eventsList devrait toutefois etre toujours alimenté
                if (null != eventsList)
                {
                    // Add the converted prices for each event quote prices
                    AddConvertedEventsQuotePrices(pCS, eventsList);
                }
                // 20120826 MF Ticket 18073 END
            }
        }

        /// <summary>
        /// Retourne la liste des évènements qui seront présents dans le flux XML
        /// <para>- les évènements trigger avec leurs enfants ou</para>
        /// <para>- les évènements trigger</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pIdT"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdELevel0">Retourne les évènements trigger (de niveau 0)</param>
        /// <returns></returns>
        /// FI 20160223 [21919] Add method 
        // EG 20220523 [XXXXX] Corrections diverses liées à la saisie OTC - OMIGRADE
        private int[] LoadEvents(string pCS, string pSessionId, int[] pIdT, CnfMessage pCnfMessage, DateTime pDate, out int[] pIdELevel0)
        {

            int[] idE = null;

            NotificationDocumentSettings settings = pCnfMessage.GetSettings();
            Nullable<NotificationTypeEnum> notificationType = pCnfMessage.NotificationType;

            bool isInvoiceMessage = IsInvoiceMessage();
            bool isReportPosAction = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSACTION));
            bool isReportPosition = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSITION));
            bool isReportPositionSynthetic = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.POSSYNTHETIC));
            bool isReportAlloc = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.ALLOCATION));
            bool isISDA = (notificationType.HasValue && (notificationType.Value == NotificationTypeEnum.ISDA));
            bool isReportFinancial = (notificationType.HasValue &&
                (
                (notificationType.Value == NotificationTypeEnum.FINANCIAL) ||
                (notificationType.Value == NotificationTypeEnum.FINANCIALPERIODIC)
                ));


            bool isUseEventForced = (false == isISDA);
            isUseEventForced &= (false == isReportFinancial);
            isUseEventForced &= (false == isReportAlloc);
            isUseEventForced &= (false == isReportPosition);
            isUseEventForced &= (false == isReportPositionSynthetic);
            isUseEventForced &= (false == isReportPosAction);
            isUseEventForced |= isInvoiceMessage;


            // Le flux XML sera enrichi par l'Event LPP/PRM que pour
            // - Les Avis d'opéré: pour afficher la valeur de la prime
            // - Les Report Position: pour afficher la valeur de la prime
            // - Les nouveaux Scheme définis par l'utilisateur
            //
            bool isAddEvent_LPP_PRM = (isReportPosition || isReportAlloc);
            //
            // Le flux XML sera enrichi par l'Event LPC/UMG que pour
            // - Les Report Position: 
            //      * pour afficher la valeur du résulat potentiel
            //      * pour afficher la quantité restante en position (EVENTDET_ETD)
            //      * pour afficher le cours de compensation (EVENTDET_ETD)
            // - Les nouveaux Schemes définis par l'utilisateur
            //
            bool isAddEvent_LPC_UMG = (isReportPosition || isReportPositionSynthetic);
            //
            // Le flux XML sera enrichi par l'Event LPC/LOV que pour
            // - Les Report Position: 
            //      * pour afficher la valeur liquidative
            // - Les nouveaux Schemes définis par l'utilisateur
            //
            bool isAddEvent_LPC_LOV = (isReportPosition || isReportPositionSynthetic);
            Boolean isUseTRADELIST = ArrFunc.Count(pIdT) > TradeRDBMSTools.SqlINListMax;
            try
            {
                string innerJoinOnTtrade = string.Empty;
                if (isUseTRADELIST)
                {
                    TradeRDBMSTools.DeleteTradeList(pCS, pSessionId);
                    TradeRDBMSTools.InsertTradeList(pCS, pIdT, pSessionId);
                    innerJoinOnTtrade = TradeRDBMSTools.SqlInnerTRADELIST;
                }
                else
                {
                    innerJoinOnTtrade = StrFunc.AppendFormat("inner join dbo.TRADE lst_t on lst_t.IDT=ev.IDT and {0}", DataHelper.SQLColumnIn(pCS, "lst_t.IDT", pIdT, TypeData.TypeDataEnum.integer));
                }

                DataTable dt = null;
                if (settings.IsUseChildEvents)
                {
                    QueryParameters qry = GetQueryEventTree(pCS, pSessionId, pCnfMessage.eventTrigger, pDate, isUseEventForced, innerJoinOnTtrade,
                                isAddEvent_LPP_PRM, isAddEvent_LPC_UMG, isAddEvent_LPC_LOV);

                    DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

                    dt = ds.Tables[0].Clone();
                    foreach (DataTable item in ds.Tables)
                    {
                        foreach (DataRow rowItem in item.Rows)
                        {
                            dt.ImportRow(rowItem);
                        }
                    }
                }
                else
                {
                    QueryParameters qry = GetQueryEvent(pCS, pSessionId, pCnfMessage.eventTrigger, pDate, isUseEventForced, innerJoinOnTtrade,
                                isAddEvent_LPP_PRM, isAddEvent_LPC_UMG, isAddEvent_LPC_LOV);
                    dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                }
                if (null == dt)
                    throw new NullReferenceException("datatable: dt is null");

                // En cas d'ajout d'évènements supplémentaires (autres que déclencheurs comme par exemple le LPC_LOV)
                // Ces derniers sont nécessairement de niveau 0 et ils pourraient être enfant d'un évènement déclencheur
                // => Ajout d'un distinct
                //DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                idE = (from row in dt.Rows.Cast<DataRow>()
                       select (new
                       {
                           IdE = Convert.ToInt32(row["IDE"])
                       })).Distinct().Select(x => x.IdE).ToArray();

                // Liste des évènements déclencheurs  (identique à idE si false == settings.isUseChildEvents)
                pIdELevel0 = (from row in dt.Rows.Cast<DataRow>().Where(x => Convert.ToInt32(x["LVL"]) == 0)
                              select Convert.ToInt32(row["IDE"])).ToArray();

            }
            finally
            {
                if (isUseTRADELIST)
                {
                    // 4- Suppression la table TRADELIST 
                    TradeRDBMSTools.DeleteTradeList(pCS, pSessionId);
                }
            }

            return idE;
        }

        /// <summary>
        ///  Retourne des requêtes recursives qui retournent les évènements déclencheurs et leurs enfants.
        ///  <para>Possibilité d'ajouter certains évènements en plus des évènements déclencheurs</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pEventTrigger"></param>
        /// <param name="pDate"></param>
        /// <param name="pIsUseEventForced"></param>
        /// <param name="pInnerJoinTrade"></param>
        /// <param name="pIsAddEvent_LPP_PRM"></param>
        /// <param name="pIsAddEvent_LPC_UMG"></param>
        /// <param name="pIsAddEVENT_LPC_LOV"></param>
        /// <returns></returns>
        /// FI 20160223 [21919] Add method 
        private static QueryParameters GetQueryEventTree(string pCS, string pSessionId, EventTrigger[] pEventTrigger, DateTime pDate,
            Boolean pIsUseEventForced, string pInnerJoinTrade,
            Boolean pIsAddEvent_LPP_PRM,
            Boolean pIsAddEvent_LPC_UMG,
            Boolean pIsAddEVENT_LPC_LOV)
        {

            QueryParameters ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            if (StrFunc.ContainsIn(pInnerJoinTrade, "@SESSIONID"))
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);

            StrBuilder query = new StrBuilder();
            foreach (EventTrigger item in pEventTrigger)
            {
                if (query.Length > 0)
                    query += SQLCst.SEPARATOR_MULTISELECT;
                query += GetQueryEventTreeStart(pCS, item.eventCode.ToString(), (item.eventTypeSpecified ? item.eventType.ToString() : string.Empty), item.eventClass.ToString(), pIsUseEventForced, pInnerJoinTrade);
            }

            if (pIsAddEvent_LPP_PRM)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPP) && x.eventTypeSpecified && x.eventType == EventTypeEnum.PRM);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.SEPARATOR_MULTISELECT;

                    query += GetQueryEventTreeStart(pCS, "LPP", "PRM", string.Empty, pIsUseEventForced, pInnerJoinTrade);
                }

                eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPP) && x.eventTypeSpecified && x.eventType == EventTypeEnum.HPR);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.SEPARATOR_MULTISELECT;

                    query += GetQueryEventTreeStart(pCS, "LPP", "HPR", string.Empty, pIsUseEventForced, pInnerJoinTrade);
                }
            }

            if (pIsAddEvent_LPC_UMG)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPC) && x.eventTypeSpecified && x.eventType == EventTypeEnum.UMG);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.SEPARATOR_MULTISELECT;

                    query += GetQueryEventTreeStart(pCS, "LPC", "UMG", "REC", pIsUseEventForced, pInnerJoinTrade);
                }
            }

            if (pIsAddEVENT_LPC_LOV)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPC) && x.eventTypeSpecified && x.eventType == EventTypeEnum.LOV);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.SEPARATOR_MULTISELECT;

                    query += GetQueryEventTreeStart(pCS, "LPC", "LOV", "REC", pIsUseEventForced, pInnerJoinTrade);
                }
            }

            ret = new QueryParameters(pCS, query.ToString(), dp);
            return ret;
        }

        /// <summary>
        ///  Requête qui retourne les évènements roots et leurs enfants pour une liste de trades
        ///  <para>Syntaxe spécifique à chaque moteur SQL</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pEventCodeStart">EventCode de l'évènement root</param>
        /// <param name="pEventTypeStart">EventType de l'évènement root (null accepté)</param>
        /// <param name="pEventClassStart">EventClass de l'évènement root(null accepté) </param>
        /// <param name="pIsUseEventForced">utilisé pour EVENTCLASS</param>
        /// <param name="pInnerJoinTrade">inner join sur Trade</param>
        /// <returns></returns>
        /// FI 20160223 [21919] Add method 
        /// RD 20160608 [22241] Modify 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static string GetQueryEventTreeStart(string pCS, string pEventCodeStart, string pEventTypeStart,
                                                        string pEventClassStart, Boolean pIsUseEventForced, string pInnerJoinTrade)
        {

            string eventTypeRestrict = string.Empty;
            if (StrFunc.IsFilled(pEventTypeStart))
                eventTypeRestrict = StrFunc.AppendFormat("and ev.EVENTTYPE = '{0}'", pEventTypeStart);
            DbSvrType dbSvrType = (DataHelper.GetDbSvrType(pCS));

            string ret;
            switch (dbSvrType)
            {
                case DbSvrType.dbSQL:
                    #region SQLSERVER

                    string innerJoinEventClass = string.Empty;
                    if (StrFunc.IsFilled(pEventClassStart))
                    {
                        string columnDT = pIsUseEventForced ? "ec.DTEVENTFORCED" : "ec.DTEVENT";
                        innerJoinEventClass = StrFunc.AppendFormat("inner join dbo.EVENTCLASS ec on ec.IDE = ev.IDE and ec.EVENTCLASS='{0}' and {1}=@DT", pEventClassStart, columnDT);
                    }

                    ret = StrFunc.AppendFormat(@"with tree (IDT, IDE, IDE_EVENT, LEVEL, EVENTCODE , EVENTTYPE ) as 
                    (
                        select  ev.IDT, ev.IDE, ev.IDE_EVENT, 0 as LEVEL, ev.EVENTCODE, ev.EVENTTYPE
	                    from dbo.EVENT ev
                        {2}    
                        {3}    
                        where ev.EVENTCODE = '{0}' {1}

	                    union all

                        select ev2.IDT, ev2.IDE, ev2.IDE_EVENT, t.LEVEL +1, ev2.EVENTCODE, ev2.EVENTTYPE
                        from dbo.EVENT ev2
                        inner join tree t on t.IDE = ev2.IDE_EVENT
                    )
                    select tree.IDT, tree.IDE, tree.IDE_EVENT, tree.LEVEL as LVL, tree.EVENTCODE, tree.EVENTTYPE, SPACE(2 * tree.level) + tree.EVENTCODE + '|' + tree.EVENTTYPE  as LABEL
                    from tree
                    order by tree.IDT, tree.IDE",
                    pEventCodeStart, eventTypeRestrict, pInnerJoinTrade, innerJoinEventClass);
                    #endregion
                    break;

                case DbSvrType.dbORA:
                    string eventClassRestrict;

                    #region ORA
                    if (StrFunc.IsFilled(pEventClassStart))
                    {
                        string columnDT = pIsUseEventForced ? "ec.DTEVENTFORCED" : "ec.DTEVENT";
                        eventClassRestrict = StrFunc.AppendFormat(@"and exists (select 1 from dbo.EVENTCLASS ec 
                                                                                where (ec.IDE = ev.IDE) and (ec.EVENTCLASS='{0}') and ({1}=@DT)) ", pEventClassStart, columnDT);
                    }
                    else
                    {
                        // RD 20160608 [22241] Pour Oracle, la présence dans la requête de tous les paramètres est obligatoire
                        eventClassRestrict = "and (@DT = @DT)";
                    }

                    ret = StrFunc.AppendFormat(@"select  ev.IDT, ev.IDE, ev.IDE_EVENT, level-1 as LVL, ev.EVENTCODE,ev.EVENTTYPE,  lpad(' ',2*(level-1)) ||  ev.EVENTCODE || '|' ||  ev.EVENTTYPE  as LABEL
                    from dbo.EVENT ev 
                    {2}    
                    start with (ev.EVENTCODE = '{0}' {1} {3})
                    connect by nocycle prior ev.IDE = ev.IDE_EVENT
                    order by ev.IDT, ev.IDE",
                    pEventCodeStart, eventTypeRestrict, pInnerJoinTrade, eventClassRestrict);
                    #endregion
                    break;

                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("svrType :{0} is not implemented", dbSvrType.ToString()));
            }

            return ret;
        }


        /// <summary>
        ///  Retourne une requête qui retourne les évènements déclencheurs.
        ///  <para>Possibilité d'ajouter certains évènements en plus des évènements déclencheurs</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSessionId"></param>
        /// <param name="pEventTrigger"></param>
        /// <param name="pDate"></param>
        /// <param name="pIsUseEventForced"></param>
        /// <param name="pInnerJoinOnTrade"></param>
        /// <param name="pIsAddEvent_LPP_PRM">si true, chargement du LPP/PRM</param>
        /// <param name="pIsAddEvent_LPC_UMG">si true, chargement du LPP/UMG</param>
        /// <param name="pIsAddEVENT_LPC_LOV">si true, chargement du LPP/LOV</param>
        /// <returns></returns>
        /// FI 20160223 [21919] Add method 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryEvent(string pCS, string pSessionId, EventTrigger[] pEventTrigger, DateTime pDate, Boolean pIsUseEventForced, string pInnerJoinOnTrade,
            Boolean pIsAddEvent_LPP_PRM,
            Boolean pIsAddEvent_LPC_UMG,
            Boolean pIsAddEVENT_LPC_LOV)
        {

            QueryParameters ret = null;

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            if (StrFunc.ContainsIn(pInnerJoinOnTrade, "@SESSIONID"))
                dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.SESSIONID), pSessionId);


            string eventClass = pIsUseEventForced ? "ec.DTEVENTFORCED" : "ec.DTEVENT";

            StrBuilder query = new StrBuilder();
            foreach (EventTrigger item in pEventTrigger)
            {
                string eventCode = item.eventCode.ToString();
                string eventType = item.eventTypeSpecified ? StrFunc.AppendFormat("and EVENTTYPE = '{0}'", item.eventType.ToString()) : string.Empty;

                string queryTrigger = StrFunc.AppendFormat(@"select ev.IDE, ev.IDE_EVENT, 0 as LVL, ev.EVENTCODE, ev.EVENTTYPE, ev.IDT
                from dbo.EVENT ev 
                {0}
                inner join dbo.EVENTCLASS ec on ec.IDE = ev.IDE and {3}= @DT
                where ev.EVENTCODE='{1}' {2}",
                pInnerJoinOnTrade, eventCode, eventType, eventClass);

                if (query.Length > 0)
                    query += SQLCst.UNION;

                query += queryTrigger;
            }


            if (pIsAddEvent_LPP_PRM)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPC) && x.eventTypeSpecified && x.eventType == EventTypeEnum.PRM);
                if (null == eventtriger)
                {

                    if (query.Length > 0)
                        query += SQLCst.UNION;

                    query += StrFunc.AppendFormat(@"select ev.IDE, ev.IDE_EVENT, 0 as LVL, ev.EVENTCODE, ev.EVENTTYPE, ev.IDT
                    from dbo.EVENT ev 
                    {0}
                    where ev.EVENTCODE='LPP' and ev.EVENTTYPE in ('PRM', 'HPR')", pInnerJoinOnTrade);
                }
            }

            if (pIsAddEvent_LPC_UMG)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPC) && x.eventTypeSpecified && x.eventType == EventTypeEnum.UMG);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.UNION;

                    query += StrFunc.AppendFormat(@"select ev.IDE, ev.IDE_EVENT, 0 as LVL, ev.EVENTCODE, ev.EVENTTYPE, ev.IDT
                    from dbo.EVENT ev 
                    inner join dbo.EVENTCLASS ec on ec.IDE = ev.IDE and {1}=@DT
                    {0}
                    where ev.EVENTCODE='LPC' and ev.EVENTTYPE ='UMG'", pInnerJoinOnTrade, eventClass);
                }
            }

            if (pIsAddEVENT_LPC_LOV)
            {
                EventTrigger eventtriger = pEventTrigger.FirstOrDefault(x => (x.eventCode == EventCodeEnum.LPC) && x.eventTypeSpecified && x.eventType == EventTypeEnum.LOV);
                if (null == eventtriger)
                {
                    if (query.Length > 0)
                        query += SQLCst.UNION;

                    query += StrFunc.AppendFormat(@"select ev.IDE, ev.IDE_EVENT, 0 as LVL, ev.EVENTCODE, ev.EVENTTYPE, ev.IDT
                    from dbo.EVENT ev 
                    inner join dbo.EVENTCLASS ec on ec.IDE = ev.IDE and {1}=@DT
                    {0}
                    where ev.EVENTCODE='LPC' and ev.EVENTTYPE ='LOV'", pInnerJoinOnTrade, eventClass);
                }
            }

            ret = new QueryParameters(pCS, query.ToString(), dp);

            return ret;
        }


        /// <summary>
        /// Add to the events the converted prices detail. The converted prices detail exists only for ETD products, 
        /// only the event with the ETD details specified will enter the process
        /// </summary>
        /// <param name="pCS">connection string. 
        /// Warning: this method loads one SQL contract for each given event. 
        /// We will use SetCacheOn on the given current connection string.</param>
        /// <param name="pEventsList">the event list to fill with the converted prices details. Only the event with the ETD details
        /// specified will enter the process</param>
        /// 20120826 MF Ticket 18073
        /// FI 20151109 [21533] Modify
        private void AddConvertedEventsQuotePrices(string pCS, Event[] pEventsList)
        {
            IEnumerable<Event> eventsWithSpecified = pEventsList.Where(elem => (elem.details != null) && elem.detailsSpecified);

            foreach (Event currEvent in eventsWithSpecified)
            {
                string tradeIdentifier = currEvent.identifierTrade;

                // RD 20140414 [19815] Utiliser le Cst.OTCml_TradeIdScheme pour récupérer l'id du trade, et non pas le premier Id trouvé qui peut être un UTI, un id du trader, ....
                //ITrade relatedTrade = this._notificationDocument.dataDocument.trade.Where(
                //    elem => elem.tradeHeader.partyTradeIdentifier[0].tradeId[0].Value == tradeIdentifier).FirstOrDefault();
                ITrade relatedTrade = this._notificationDocument.DataDocument.Trade.Where(
                    elem => Tools.GetScheme(elem.TradeHeader.PartyTradeIdentifier[0].TradeId, Cst.OTCml_TradeIdScheme).Value == tradeIdentifier).FirstOrDefault();

                if ((null != relatedTrade) && relatedTrade.Product.ProductBase.IsExchangeTradedDerivative)
                {
                    if (relatedTrade.Product is IExchangeTradedDerivative etd)
                    {
                        // FI 20151109 [21533] Modify Appel à ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument
                        SQL_DerivativeContract sql_DerivativeContract =
                        ExchangeTradedDerivativeTools.LoadSqlDerivativeContractFromFixInstrument(CSTools.SetCacheOn(pCS), null, etd.TradeCaptureReport.Instrument);

                        if (null == sql_DerivativeContract)
                            throw new NullReferenceException(StrFunc.AppendFormat("DerivativeContract is null for trade (Identifier:{0})", tradeIdentifier));

                        AddConvertedEventQuotePrices(currEvent, sql_DerivativeContract);
                    }
                }
            }
        }

        /// <summary>
        /// Add to the event and all his child Events the converted prices detail. 
        /// Only the event with the ETD details specified will enter the process
        /// </summary>
        /// <param name="pCurrentEvent"></param>
        /// <param name="pSql_DerivativeContract"></param>
        private void AddConvertedEventQuotePrices(Event pCurrentEvent, SQL_DerivativeContract pSql_DerivativeContract)
        {
            EventDetails details = pCurrentEvent.details;

            if (details != null)
            {
                if (null != details.quotePrice)
                {
                    string convertedClosingPrice = details.quotePrice.ToConvertedFractionalPartString
                        (pSql_DerivativeContract.PriceNumericBase, pSql_DerivativeContract.InstrumentDen,
                        pSql_DerivativeContract.PriceMultiplier, pSql_DerivativeContract.PriceFormatStyle);

                    details.ConvertedPrices.ConvertedClearingPriceSpecified = true;
                    details.ConvertedPrices.ConvertedClearingPrice = convertedClosingPrice;
                }
                // RD 20140725 [20212] Add new element ConvertedSettltPrice                                                        
                if (null != details.settltPrice)
                {
                    string convertedSettltPrice = details.settltPrice.ToConvertedFractionalPartString
                        (pSql_DerivativeContract.PriceNumericBase, pSql_DerivativeContract.InstrumentDen,
                        pSql_DerivativeContract.PriceMultiplier, pSql_DerivativeContract.PriceFormatStyle);

                    details.ConvertedPrices.ConvertedSettltPriceSpecified = true;
                    details.ConvertedPrices.ConvertedSettltPrice = convertedSettltPrice;
                }
                // RD 20140730 [20212] Add new element ConvertedClosingPrice
                if (null != details.closingPrice)
                {
                    string convertedClosingPrice = details.closingPrice.ToConvertedFractionalPartString
                        (pSql_DerivativeContract.PriceNumericBase, pSql_DerivativeContract.InstrumentDen,
                        pSql_DerivativeContract.PriceMultiplier, pSql_DerivativeContract.PriceFormatStyle);

                    details.ConvertedPrices.ConvertedClosingPriceSpecified = true;
                    details.ConvertedPrices.ConvertedClosingPrice = convertedClosingPrice;
                }
            }

            // RD 20140730 [20212] Add ConvertedPrice for child Events
            if (pCurrentEvent is HierarchicalEvent @event)
            {
                HierarchicalEvent[] childEvents = @event.hEvent;

                if (childEvents != null)
                {
                    foreach (Event childEvent in childEvents)
                        AddConvertedEventQuotePrices(childEvent, pSql_DerivativeContract);
                }
            }
        }

        /// <summary>
        /// Retourne true si le produit du trade est IsADM et pas IsInvoiceSettlement
        /// </summary>
        /// <returns></returns>
        private bool IsInvoiceMessage()
        {
            DataDocumentContainer msgDataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);
            bool ret = (msgDataDocument.CurrentProduct.ProductBase.IsADM) &&
                        (false == msgDataDocument.CurrentProduct.IsInvoiceSettlement);
            return ret;
        }

        /// <summary>
        /// Retourne true si les caractéristiques d'affichage des frais sur le reports sont à alimenter dans le flux XML. 
        /// C'est valable pour les ETD 
        /// <para></para>
        /// </summary>
        /// <returns></returns>
        /// FI 20140820 [20275] Modify
        /// FI 20151019 [21317] Modify
        private bool IsSetReportFeeDisplay(DataDocumentContainer pDataDocument)
        {
            bool ret = (pDataDocument.CurrentProduct.ProductBase.IsExchangeTradedDerivative)/* ||
                ((pDataDocument.currentProduct.productBase.IsADM) &&
                (false == pDataDocument.currentProduct.IsInvoiceSettlement) && 
                pDataDocument.currentProduct.IsOnlyDerivativeInvoicingReport)*/
                                                                               ;
            // FI 20140820 [20275] add IsReturnSwap et IsEquitySecurityTransaction
            ret = ret || pDataDocument.CurrentProduct.ProductBase.IsReturnSwap;
            ret = ret || pDataDocument.CurrentProduct.ProductBase.IsEquitySecurityTransaction;
            ret = ret || pDataDocument.CurrentProduct.ProductBase.IsDebtSecurityTransaction; // FI 20151019 [21317] Add (puisquu'il existe déjà IsEquitySecurityTransaction

            return ret;
        }

        /// <summary>
        /// Alimente confirmationMessageDoc.events
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pSettings"></param>
        // EG 20180426 Analyse du code Correction [CA2202]
        private void SetNotepad(string pCS, int[] pIdT, NotificationDocumentSettings pSettings)
        {
            NotificationDocument.NotepadSpecified = false;

            if (pSettings.IsUseNotepad)
            {
                ArrayList lstNotePad = new ArrayList();
                using (IDataReader dr = TradeRDBMSTools.LoadTradeNodepad(pCS, pIdT))
                {
                    while (dr.Read())
                    {
                        if (null != dr[0])
                        {
                            StrBuilder np = new StrBuilder(dr[0].ToString());
                            int idt = Convert.ToInt32(dr[1]);
                            //
                            if (idt > 0 && np.Length > 0)
                            {
                                CDATA notePad = new CDATA(np.StringBuilder.ToString(), dr[2].ToString());
                                lstNotePad.Add(notePad);
                            }
                        }
                    }
                }

                // RD 20100830 [] Si on a un seul NotePad, l'attribut Id est supprimé pour garder une compatibilité ascendante
                if (lstNotePad.Count == 1)
                    ((CDATA)lstNotePad[0]).Id = string.Empty;

                NotificationDocument.NotepadSpecified = (lstNotePad.Count > 0);
                if (NotificationDocument.NotepadSpecified)
                    NotificationDocument.Notepad = (CDATA[])lstNotePad.ToArray(typeof(CDATA));

            }
        }

        /// <summary>
        /// <para>Alimentation des actions sur positions en date {pDate}</para>
        /// <para>Contient les actions en date {pDate} et les décompensations en date {pDate}</para>
        /// <para>Alimente NotificationDocument.posActions</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Liste des trades du message</param>
        /// <param name="pNotificationClass">Représente le message</param>
        /// <param name="pConfirmationChain">Représente la chaîne de confirmation</param>
        /// <param name="pDate"></param>
        ///FI 20120426 [17703] add SetPosAction
        ///FI 20140731 [XXXXX] Modify (Tuning)
        ///FI 20140821 [20275] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetPosActions(string pCS, int[] pIdT, NotificationClassEnum pNotificationClass, ConfirmationChain pConfirmationChain, DateTime pDate)
        {

            //Sur les éditions, l'entité est tjs l'émetteur des messages
            //Les dealers des trades sont des clients ou des comptes maison
            //Restriction sur ENTITE pour récupérer toutes les actions en rapport avec l'entité
            //La liste des trades est représenté par le paramètre pIdT
            string additionnalJoin = @"inner join dbo.BOOK b on (b.IDB = tr.IDB_DEALER) and (b.IDA_ENTITY = @ENTITY)";
            if (pNotificationClass == NotificationClassEnum.MULTITRADES)
                additionnalJoin += " and (b.IDB = @IDB)";

            additionnalJoin += Cst.CrLf + GetJoinTrade(pCS, pIdT, "tr.IDT", false);


            QueryParameters qry = GetQueryPosAction(pCS, pDate, additionnalJoin, PostionType.Business);

            DataParameters dp = qry.Parameters;
            dp.Add(new DataParameter(pCS, "ENTITY", DbType.Int32), pConfirmationChain[SendEnum.SendBy].IdActor);
            if (pNotificationClass == NotificationClassEnum.MULTITRADES)
                dp.Add(new DataParameter(pCS, "IDB", DbType.Int32), pConfirmationChain[SendEnum.SendTo].IdBook);


            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            DataRow[] row = dt.Select();
            if (ArrFunc.IsFilled(row))
            {
                AddPosActions(pCS, NotificationDocument.PosActions, pDate, row, NotificationDocument.EfsMLversion);
                NotificationDocument.PosActionsSpecified = true;
            }
        }



        /// <summary>
        /// Alimente l'élément trades avec les trades négociés le jour {pDate}
        /// <para>Alimente NotificationDocument.trades</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">liste des trades négociés en date {pDate}</param>
        /// <param name="pDate">Représente le jour</param>
        /// FI 20150622 [21124] Modify
        /// FI 20150623 [21149] Modify
        private void SetTrades(string pCS, int[] pIdT, DateTime pDate)
        {
            // FI 20130625 peut-être faut-il eviter l'usage du in et charger toutes les trades à cette date
            // et faire la restriction sur le jeu de résultat avec LINQ pour ne considérer que les trades présents dans {pIdT}??? 
            string additionnalJoin = GetJoinTrade(pCS, pIdT, "t.IDT", true);

            // FI 20150622 [21124] passage du paramètre additionnalJoin
            QueryParameters qry = GetQueryTradesReport3(pCS,  additionnalJoin, pDate, TradeReportTypeEnum.tradeOfDay, null);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());

            //Ajout des trades jours
            DataRow[] row = dt.Select();
            if (ArrFunc.IsFilled(row))
            {
                AddTradesReport(pCS, NotificationDocument.Trades, pDate, row);
                NotificationDocument.TradesSpecified = true;
            }
        }

        /// <summary>
        /// Alimentation de tous les trades avec les mêmes caractéristiques suivantes que le trade {pIdT}
        /// <para>- Même numéro d'ordre</para>
        /// <para>- Même Dealer (Actor/Book)</para>
        /// <para>- Même Asset</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Trade déclencheur de la confirmation de l'ordre</param>
        /// <param name="pDate">Date de traitement</param>
        /// FI 20190515 [23912] Add Method 
        /// RD 20200106 [25144] Modify 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetTradesOfOrder(string pCS, int pIdT, DateTime pDate)
        {
            // RD 20200106 [25144] Inclure uniquement les trades avec les mêmes caractéristiques suivantes que le trade déclencheur de la confirmation:
            // - Même numéro d'ordre
            // - Même Dealer (Actor/Book)
            // - Même Asset
            // RD 20210420 [xxxxx] Utiliser le paramètre @IDT
            //string additionnalJoin = @"inner join dbo.TRADE trOrder on  (trOrder.IDT= t.IDT)  
            string additionnalJoin = @"inner join dbo.TRADE trOrder on (trOrder.IDT=@IDT)  
            and (trOrder.ORDERID=t.ORDERID)
            and (trOrder.IDA_DEALER=t.IDA_DEALER)
            and (trOrder.IDB_DEALER=t.IDB_DEALER)
            and (trOrder.IDASSET=t.IDASSET)
            and (trOrder.ASSETCATEGORY=t.ASSETCATEGORY)";

            QueryParameters qry = GetQueryTradesReport3(pCS,  additionnalJoin, pDate, TradeReportTypeEnum.tradeOfOrder, null);
            qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            //Ajout des trades de l'order
            DataRow[] row = dt.Select();
            if (ArrFunc.IsFilled(row))
            {
                AddTradesReport(pCS, NotificationDocument.Trades, pDate, row);
                NotificationDocument.TradesSpecified = true;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCnfMessage"></param>
        /// <param name="pIdInci_SendBy"></param>
        /// <param name="pIDAContactOffice_SendBy"></param>
        /// FI 20141209 [XXXXX] Modify
        /// FI 20150128 [20742] Modify
        /// FI 20160530 [21885] Modify
        /// FI 20160530 [21885] Modify
        /// FI 20160613 [22256] Modify
        /// FI 20160624 [22286] Modify
        private void SetReportSettings(string pCS, CnfMessage pCnfMessage, int pIdInci_SendBy, int pIDAContactOffice_SendBy)
        {

            // FI 20141209 [XXXXX] add AMOUNTFORMAT, NOACTIVITYMSG, SECTIONBANNER, ASSETBANNER, SUMMARYUTI
            // FI 20150128 [20742] add SECTIONTITLE
            // FI 20150515 [XXXXX] add TIMESTAMPTYPE
            // FI 20150522 [XXXXX] add ISTOTALONCTR
            // CC 20160615 [22259] add HBOOKOWNERIDLABEL
            // FI 20160530 [21885] add COLLATERAL_SOD
            // FI 20160613 [22256] add COLLATERAL_UOD

            string query = @" 
                select 
                    isnull(nullif(cnfmsg_hf.HLFIRSTPG,'Default'),inci_hf.HLFIRSTPG) as HLFIRSTPG, 
                    case when cnfmsg_hf.HLFIRSTPG='Custom' then cnfmsg_hf.HLFIRSTPGCUSTOM when cnfmsg_hf.HLFIRSTPG='Default' and inci_hf.HLFIRSTPG='Custom' then inci_hf.HLFIRSTPGCUSTOM  else cnfmsg_hf.HLFIRSTPGCUSTOM end as HLFIRSTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.HCFIRSTPG,'Default'),inci_hf.HCFIRSTPG) as HCFIRSTPG, 
                    case when cnfmsg_hf.HCFIRSTPG='Custom' then cnfmsg_hf.HCFIRSTPGCUSTOM when cnfmsg_hf.HCFIRSTPG='Default' and inci_hf.HCFIRSTPG='Custom' then inci_hf.HCFIRSTPGCUSTOM  else cnfmsg_hf.HCFIRSTPGCUSTOM end as HCFIRSTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.HRFIRSTPG,'Default'),inci_hf.HRFIRSTPG) as HRFIRSTPG, 
                    case when cnfmsg_hf.HRFIRSTPG='Custom' then cnfmsg_hf.HRFIRSTPGCUSTOM when cnfmsg_hf.HRFIRSTPG='Default' and inci_hf.HRFIRSTPG='Custom' then inci_hf.HRFIRSTPGCUSTOM  else cnfmsg_hf.HRFIRSTPGCUSTOM end as HRFIRSTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.HLALLPG,'Default'),inci_hf.HLALLPG) as HLALLPG, 
                    case    when cnfmsg_hf.HLALLPG='Custom' then 
                                cnfmsg_hf.HLALLPGCUSTOM 
                            when cnfmsg_hf.HLALLPG='Default' and inci_hf.HLALLPG='Custom' then 
                                inci_hf.HLALLPGCUSTOM  
                            else cnfmsg_hf.HLALLPGCUSTOM end as HLALLPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.HCALLPG,'Default'),inci_hf.HCALLPG) as HCALLPG, 
                    case when cnfmsg_hf.HCALLPG='Custom' then cnfmsg_hf.HCALLPGCUSTOM when cnfmsg_hf.HCALLPG='Default' and inci_hf.HCALLPG='Custom' then inci_hf.HCALLPGCUSTOM  else cnfmsg_hf.HCALLPGCUSTOM end as HCALLPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.HRALLPG,'Default'),inci_hf.HRALLPG) as HRALLPG, 
                    case when cnfmsg_hf.HRALLPG='Custom' then cnfmsg_hf.HRALLPGCUSTOM when cnfmsg_hf.HRALLPG='Default' and inci_hf.HRALLPG='Custom' then inci_hf.HRALLPGCUSTOM  else cnfmsg_hf.HRALLPGCUSTOM end as HRALLPGCUSTOM,
                    isnull(cnfmsg_hf.HCOLOR,inci_hf.HCOLOR) as HCOLOR,
                    isnull(cnfmsg_hf.HRULESIZE,inci_hf.HRULESIZE) as HRULESIZE,
                    isnull(cnfmsg_hf.HRULECOLOR,inci_hf.HRULECOLOR) as HRULECOLOR,
                    cnfmsg_hf.HTITLE as HTITLE,
                    isnull(nullif(cnfmsg_hf.HTITLE_POSITION,'Default'),inci_hf.HTITLE_POSITION) as HTITLE_POSITION,
                    isnull(cnfmsg_hf.HBOOKIDLABEL,inci_hf.HBOOKIDLABEL) as HBOOKIDLABEL,
                    isnull(cnfmsg_hf.HBOOKOWNERIDLABEL,inci_hf.HBOOKOWNERIDLABEL) as HBOOKOWNERIDLABEL,
                    isnull(cnfmsg_hf.HDTBUSINESSLABEL,inci_hf.HDTBUSINESSLABEL) as HDTBUSINESSLABEL,
                    isnull(cnfmsg_hf.HFORMULA,inci_hf.HFORMULA) as HFORMULA,
                    isnull(nullif(cnfmsg_hf.FLLASTPG,'Default'),inci_hf.FLLASTPG) as FLLASTPG, 
                    case when cnfmsg_hf.FLLASTPG='Custom' then cnfmsg_hf.FLLASTPGCUSTOM when cnfmsg_hf.FLLASTPG='Default' and inci_hf.FLLASTPG='Custom' then inci_hf.FLLASTPGCUSTOM else cnfmsg_hf.FLLASTPGCUSTOM end as FLLASTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.FCLASTPG,'Default'),inci_hf.FCLASTPG) as FCLASTPG, 
                    case when cnfmsg_hf.FCLASTPG='Custom' then cnfmsg_hf.FCLASTPGCUSTOM when cnfmsg_hf.FCLASTPG='Default' and inci_hf.FCLASTPG='Custom' then inci_hf.FCLASTPGCUSTOM else cnfmsg_hf.FCLASTPGCUSTOM end as FCLASTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.FRLASTPG,'Default'),inci_hf.FRLASTPG) as FRLASTPG, 
                    case when cnfmsg_hf.FRLASTPG='Custom' then cnfmsg_hf.FRLASTPGCUSTOM when cnfmsg_hf.FRLASTPG='Default' and inci_hf.FRLASTPG='Custom' then inci_hf.FRLASTPGCUSTOM else cnfmsg_hf.FRLASTPGCUSTOM end as FRLASTPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.FLALLPG,'Default'),inci_hf.FLALLPG) as FLALLPG, 
                    case when cnfmsg_hf.FLALLPG='Custom' then cnfmsg_hf.FLALLPGCUSTOM when cnfmsg_hf.FCALLPG='Default' and inci_hf.FLALLPG='Custom' then inci_hf.FLALLPGCUSTOM else cnfmsg_hf.FLALLPGCUSTOM end as FLALLPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.FCALLPG,'Default'),inci_hf.FCALLPG) as FCALLPG, 
                    case when cnfmsg_hf.FCALLPG='Custom' then cnfmsg_hf.FCALLPGCUSTOM when cnfmsg_hf.FCALLPG='Default' and inci_hf.FCALLPG='Custom' then inci_hf.FCALLPGCUSTOM else cnfmsg_hf.FCALLPGCUSTOM end as FCALLPGCUSTOM,
                    isnull(nullif(cnfmsg_hf.FRALLPG,'Default'),inci_hf.FRALLPG) as FRALLPG, 
                    case when cnfmsg_hf.FRALLPG='Custom' then cnfmsg_hf.FRALLPGCUSTOM when cnfmsg_hf.FRALLPG='Default' and inci_hf.FRALLPG='Custom' then inci_hf.FRALLPGCUSTOM else cnfmsg_hf.FRALLPGCUSTOM end as FRALLPGCUSTOM,
                    isnull(cnfmsg_hf.FFORMULA,inci_hf.FFORMULA) as FFORMULA,
                    isnull(nullif(cnfmsg_hf.FLEGEND,'Default'),inci_hf.FLEGEND) as FLEGEND,
                    isnull(cnfmsg_hf.FRULESIZE,inci_hf.FRULESIZE) as FRULESIZE,
                    isnull(cnfmsg_hf.FRULECOLOR,inci_hf.FRULECOLOR) as FRULECOLOR,
                    isnull(nullif(cnfmsg_hf.SORT, 'Default'),inci_hf.SORT) as SORT,      
                    cnfmsg_hf.SUMMARYSTRATEGIES,cnfmsg_hf.SUMMARYCASHFLOWS,cnfmsg_hf.SUMMARYFEES,cnfmsg_hf.SUMMARYUTI,
                    
                    isnull(nullif(inci_hf.TRADENUMBERIDENT,'Default'),cnfmsg_hf.TRADENUMBERIDENT) as TRADENUMBERIDENT,
                    isnull(nullif(inci_hf.TIMESTAMPTYPE,'Default'),cnfmsg_hf.TIMESTAMPTYPE) as TIMESTAMPTYPE,
                    
                    isnull(nullif(cnfmsg_hf.AMOUNTFORMAT,'Default'),inci_hf.AMOUNTFORMAT) as AMOUNTFORMAT,
                    isnull(cnfmsg_hf.POSAMOUNTFORECOLOR,inci_hf.POSAMOUNTFORECOLOR) as POSAMOUNTFORECOLOR,
                    isnull(cnfmsg_hf.POSAMOUNTBACKCOLOR,inci_hf.POSAMOUNTBACKCOLOR) as POSAMOUNTBACKCOLOR,                    
                    isnull(cnfmsg_hf.NEGAMOUNTFORECOLOR,inci_hf.NEGAMOUNTFORECOLOR) as NEGAMOUNTFORECOLOR,
                    isnull(cnfmsg_hf.NEGAMOUNTBACKCOLOR,inci_hf.NEGAMOUNTBACKCOLOR) as NEGAMOUNTBACKCOLOR,
                    
                    isnull(inci_hf.ISTOTALONCTR,cnfmsg_hf.ISTOTALONCTR) as ISTOTALONCTR,

                    isnull(cnfmsg_hf.NOACTIVITYMSGFONTSIZE,inci_hf.NOACTIVITYMSGFONTSIZE) as NOACTIVITYMSGFONTSIZE,
                    isnull(cnfmsg_hf.NOACTIVITYMSGCOLOR,inci_hf.NOACTIVITYMSGCOLOR) as NOACTIVITYMSGCOLOR,

                    isnull(cnfmsg_hf.EXPIRYINDICATORDAYS,inci_hf.EXPIRYINDICATORDAYS) as EXPIRYINDICATORDAYS,
                    isnull(cnfmsg_hf.EXPIRYINDICATORCOLOR,inci_hf.EXPIRYINDICATORCOLOR) as EXPIRYINDICATORCOLOR,

                    isnull(cnfmsg_hf.SECTIONBANNERSTYLE,inci_hf.SECTIONBANNERSTYLE) as SECTIONBANNERSTYLE,
                    isnull(cnfmsg_hf.SECTIONBANNERALIGN,inci_hf.SECTIONBANNERALIGN) as SECTIONBANNERALIGN,
                    isnull(cnfmsg_hf.SECTIONBANNERFONTSIZE,inci_hf.SECTIONBANNERFONTSIZE) as SECTIONBANNERFONTSIZE,
                    isnull(cnfmsg_hf.SECTIONBANNERFORECOLOR,inci_hf.SECTIONBANNERFORECOLOR) as SECTIONBANNERFORECOLOR,
                    isnull(cnfmsg_hf.SECTIONBANNERBACKCOLOR,inci_hf.SECTIONBANNERBACKCOLOR) as SECTIONBANNERBACKCOLOR,

                    isnull(cnfmsg_hf.ASSETBANNERSTYLE,inci_hf.ASSETBANNERSTYLE) as ASSETBANNERSTYLE,
                    isnull(cnfmsg_hf.ASSETBANNERALIGN,inci_hf.ASSETBANNERALIGN) as ASSETBANNERALIGN,
                    isnull(cnfmsg_hf.ASSETBANNERFONTSIZE,inci_hf.ASSETBANNERFONTSIZE) as ASSETBANNERFONTSIZE,
                    isnull(cnfmsg_hf.ASSETBANNERFORECOLOR,inci_hf.ASSETBANNERFORECOLOR) as ASSETBANNERFORECOLOR,
                    isnull(cnfmsg_hf.ASSETBANNERBACKCOLOR,inci_hf.ASSETBANNERBACKCOLOR) as ASSETBANNERBACKCOLOR,
                    
                    isnull(cnfmsg_hf.SECTIONTITLE,inci_hf.SECTIONTITLE) as SECTIONTITLE,
                    
                    isnull(cnfmsg_hf.JOURNALENTRIES,inci_hf.JOURNALENTRIES) as JOURNALENTRIES,
                    isnull(cnfmsg_hf.COLLATERAL_SOD,inci_hf.COLLATERAL_SOD) as COLLATERAL_SOD,
                    isnull(cnfmsg_hf.COLLATERAL_UOD,inci_hf.COLLATERAL_UOD) as COLLATERAL_UOD,

                    isnull(cnfmsg_hf.HCANCELMSGFONTSIZE,inci_hf.HCANCELMSGFONTSIZE) as HCANCELMSGFONTSIZE,
                    isnull(cnfmsg_hf.HCANCELMSGCOLOR,inci_hf.HCANCELMSGCOLOR) as HCANCELMSGCOLOR,
                    isnull(cnfmsg_hf.HCANCELMSG,inci_hf.HCANCELMSG) as HCANCELMSG

                    from dbo.INCIHEADFOOT inci_hf
                    full outer join dbo.CNFMESSAGEHEADFOOT cnfmsg_hf on (cnfmsg_hf.IDCNFMESSAGE=@IDCNFMESSAGE)
                    where ((cnfmsg_hf.IDCNFMESSAGE is null) 
                    or ((cnfmsg_hf.IDCNFMESSAGE=@IDCNFMESSAGE)
                        and ((cnfmsg_hf.CULTURE=@CULTURE) or (cnfmsg_hf.ISDEFAULTCULTURE=1 
                        and not exists (select 1 from dbo.CNFMESSAGEHEADFOOT where IDCNFMESSAGE=@IDCNFMESSAGE and CULTURE=@CULTURE)))
                        and ((cnfmsg_hf.IDA=@IDA) or (cnfmsg_hf.IDA is null))
                    ))
                    and ((inci_hf.IDINCI is null) 
                    or ((inci_hf.IDINCI=@IDINCI)
                        and ((inci_hf.CULTURE=@CULTURE) or (inci_hf.ISDEFAULTCULTURE=1 
                        and not exists (select 1 from dbo.INCIHEADFOOT where IDINCI=@IDINCI and CULTURE=@CULTURE)))
                    ))";

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "IDA", DbType.Int32), pIDAContactOffice_SendBy);
            dp.Add(new DataParameter(pCS, "IDINCI", DbType.Int32), pIdInci_SendBy);
            dp.Add(new DataParameter(pCS, "IDCNFMESSAGE", DbType.Int32), pCnfMessage.idCnfMessage);
            dp.Add(new DataParameter(pCS, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), Culture);

            QueryParameters qry = new QueryParameters(pCS, query, dp);
            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());

            INotificationHeader messageHeader = NotificationDocument.Header;

            if (null != ds && ArrFunc.IsFilled(ds.Tables) && ArrFunc.IsFilled(ds.Tables[0].Rows))
            {
                messageHeader.ReportSettingsSpecified = true;
                if (messageHeader.ReportSettings == null)
                    messageHeader.ReportSettings = new ReportSettings();

                messageHeader.ReportSettings.headerFooterSpecified = true;
                messageHeader.ReportSettings.headerFooter = new HeaderFooter();

                HeaderFooter headerFooter = messageHeader.ReportSettings.headerFooter;

                DataRow dr = ds.Tables[0].Rows[0];

                // Header
                headerFooter.hLFirstPG = dr["HLFIRSTPG"].ToString();
                headerFooter.hLFirstPGCustom = (Convert.DBNull == dr["HLFIRSTPGCUSTOM"] ? null : dr["HLFIRSTPGCUSTOM"].ToString());
                headerFooter.hLFirstPGCustomSpecified = StrFunc.IsFilled(headerFooter.hLFirstPGCustom);
                //
                headerFooter.hCFirstPG = dr["HCFIRSTPG"].ToString();
                headerFooter.hCFirstPGCustom = (Convert.DBNull == dr["HCFIRSTPGCUSTOM"] ? null : dr["HCFIRSTPGCUSTOM"].ToString());
                headerFooter.hCFirstPGCustomSpecified = StrFunc.IsFilled(headerFooter.hCFirstPGCustom);
                //
                headerFooter.hRFirstPG = dr["HRFIRSTPG"].ToString();
                headerFooter.hRFirstPGCustom = (Convert.DBNull == dr["HRFIRSTPGCUSTOM"] ? null : dr["HRFIRSTPGCUSTOM"].ToString());
                headerFooter.hRFirstPGCustomSpecified = StrFunc.IsFilled(headerFooter.hRFirstPGCustom);
                //
                headerFooter.hLAllPG = dr["HLALLPG"].ToString();
                headerFooter.hLAllPGCustom = (Convert.DBNull == dr["HLALLPGCUSTOM"] ? null : dr["HLALLPGCUSTOM"].ToString());
                headerFooter.hLAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.hLAllPGCustom);
                //
                headerFooter.hCAllPG = dr["HCALLPG"].ToString();
                headerFooter.hCAllPGCustom = (Convert.DBNull == dr["HCALLPGCUSTOM"] ? null : dr["HCALLPGCUSTOM"].ToString());
                headerFooter.hCAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.hCAllPGCustom);
                //
                headerFooter.hRAllPG = dr["HRALLPG"].ToString();
                headerFooter.hRAllPGCustom = (Convert.DBNull == dr["HRALLPGCUSTOM"] ? null : dr["HRALLPGCUSTOM"].ToString());
                headerFooter.hRAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.hRAllPGCustom);
                //
                headerFooter.hColor = (Convert.DBNull == dr["HCOLOR"] ? null : dr["HCOLOR"].ToString());
                headerFooter.hColorSpecified = StrFunc.IsFilled(headerFooter.hColor);
                //
                headerFooter.hRuleSize = (Convert.DBNull == dr["HRULESIZE"] ? 0 : Convert.ToInt32(dr["HRULESIZE"]));
                headerFooter.hRuleSizeSpecified = (headerFooter.hRuleSize > 0);
                headerFooter.hRuleColor = dr["HRULECOLOR"].ToString();
                headerFooter.hTitle = dr["HTITLE"].ToString();
                headerFooter.hTitlePosition = dr["HTITLE_POSITION"].ToString();
                //
                headerFooter.hBookIDLabel = (Convert.DBNull == dr["HBOOKIDLABEL"] ? null : dr["HBOOKIDLABEL"].ToString());
                headerFooter.hBookIDLabelSpecified = StrFunc.IsFilled(headerFooter.hBookIDLabel);
                headerFooter.hBookOwnerIDLabel = (Convert.DBNull == dr["HBOOKOWNERIDLABEL"] ? null : dr["HBOOKOWNERIDLABEL"].ToString());
                headerFooter.hBookOwnerIDLabelSpecified = StrFunc.IsFilled(headerFooter.hBookOwnerIDLabel);
                headerFooter.hDTBusinessLabel = (Convert.DBNull == dr["HDTBUSINESSLABEL"] ? null : dr["HDTBUSINESSLABEL"].ToString());
                headerFooter.hDTBusinessLabelSpecified = StrFunc.IsFilled(headerFooter.hDTBusinessLabel);
                //
                headerFooter.hFormula = (Convert.DBNull == dr["HFORMULA"] ? null : dr["HFORMULA"].ToString());
                headerFooter.hFormulaSpecified = StrFunc.IsFilled(headerFooter.hFormula);

                // Footer
                headerFooter.fLLastPG = dr["FLLASTPG"].ToString();
                headerFooter.fLLastPGCustom = (Convert.DBNull == dr["FLLASTPGCUSTOM"] ? null : dr["FLLASTPGCUSTOM"].ToString());
                headerFooter.fLLastPGCustomSpecified = StrFunc.IsFilled(headerFooter.fLLastPGCustom);
                //
                headerFooter.fCLastPG = dr["FCLASTPG"].ToString();
                headerFooter.fCLastPGCustom = (Convert.DBNull == dr["FCLASTPGCUSTOM"] ? null : dr["FCLASTPGCUSTOM"].ToString());
                headerFooter.fCLastPGCustomSpecified = StrFunc.IsFilled(headerFooter.fCLastPGCustom);
                //
                headerFooter.fRLastPG = dr["FRLASTPG"].ToString();
                headerFooter.fRLastPGCustom = (Convert.DBNull == dr["FRLASTPGCUSTOM"] ? null : dr["FRLASTPGCUSTOM"].ToString());
                headerFooter.fRLastPGCustomSpecified = StrFunc.IsFilled(headerFooter.fRLastPGCustom);
                //
                headerFooter.fLAllPG = dr["FLALLPG"].ToString();
                headerFooter.fLAllPGCustom = (Convert.DBNull == dr["FLALLPGCUSTOM"] ? null : dr["FLALLPGCUSTOM"].ToString());
                headerFooter.fLAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.fLAllPGCustom);
                //
                headerFooter.fCAllPG = dr["FCALLPG"].ToString();
                headerFooter.fCAllPGCustom = (Convert.DBNull == dr["FCALLPGCUSTOM"] ? null : dr["FCALLPGCUSTOM"].ToString());
                headerFooter.fCAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.fCAllPGCustom);
                //
                headerFooter.fRAllPG = dr["FRALLPG"].ToString();
                headerFooter.fRAllPGCustom = (Convert.DBNull == dr["FRALLPGCUSTOM"] ? null : dr["FRALLPGCUSTOM"].ToString());
                headerFooter.fRAllPGCustomSpecified = StrFunc.IsFilled(headerFooter.fRAllPGCustom);
                //
                headerFooter.fRuleSize = (Convert.DBNull == dr["FRULESIZE"] ? 0 : Convert.ToInt32(dr["FRULESIZE"]));
                headerFooter.fRuleSizeSpecified = (headerFooter.fRuleSize > 0);
                headerFooter.fRuleColor = dr["FRULECOLOR"].ToString();
                headerFooter.fRuleColorSpecified = StrFunc.IsFilled(headerFooter.fRuleColor);
                //
                headerFooter.fFormula = (Convert.DBNull == dr["FFORMULA"] ? null : dr["FFORMULA"].ToString());
                headerFooter.fFormulaSpecified = StrFunc.IsFilled(headerFooter.fFormula);
                //
                headerFooter.fLegend = dr["FLEGEND"].ToString();

                headerFooter.sort = dr["SORT"].ToString();
                headerFooter.summaryStrategies = dr["SUMMARYSTRATEGIES"].ToString();
                headerFooter.summaryCashFlows = dr["SUMMARYCASHFLOWS"].ToString();
                headerFooter.summaryFees = dr["SUMMARYFEES"].ToString();
                headerFooter.summaryUTI = dr["SUMMARYUTI"].ToString();

                headerFooter.tradeNumberIdent = Cst.TradeIdentificationEnum.Default;
                if (Convert.DBNull != dr["TRADENUMBERIDENT"])
                    headerFooter.tradeNumberIdent = (Cst.TradeIdentificationEnum)System.Enum.Parse(typeof(Cst.TradeIdentificationEnum), dr["TRADENUMBERIDENT"].ToString());

                headerFooter.timestampStyle = Cst.TimestampType.None;
                if (Convert.DBNull != dr["TIMESTAMPTYPE"])
                    headerFooter.timestampStyle = (Cst.TimestampType)System.Enum.Parse(typeof(Cst.TimestampType), dr["TIMESTAMPTYPE"].ToString());

                //headerFooter.amount
                headerFooter.amount.format = dr["AMOUNTFORMAT"].ToString();
                headerFooter.amount.positiveValue.foreColor = (Convert.DBNull == dr["POSAMOUNTFORECOLOR"] ? null : dr["POSAMOUNTFORECOLOR"].ToString());
                headerFooter.amount.positiveValue.backColor = (Convert.DBNull == dr["POSAMOUNTBACKCOLOR"] ? null : dr["POSAMOUNTBACKCOLOR"].ToString());
                headerFooter.amount.positiveValueSpecified = headerFooter.amount.positiveValue.IsFilled();
                headerFooter.amount.negativeValue.foreColor = (Convert.DBNull == dr["NEGAMOUNTFORECOLOR"] ? null : dr["NEGAMOUNTFORECOLOR"].ToString());
                headerFooter.amount.negativeValue.backColor = (Convert.DBNull == dr["NEGAMOUNTBACKCOLOR"] ? null : dr["NEGAMOUNTBACKCOLOR"].ToString());
                headerFooter.amount.negativeValueSpecified = headerFooter.amount.negativeValue.IsFilled();
                headerFooter.amountSpecified = headerFooter.amount.IsFilled();

                //headerFooter.totalInBaseCurrency
                headerFooter.totalInBaseCurrency = Convert.ToBoolean(dr["ISTOTALONCTR"]) ? YesNoEnum.Yes : YesNoEnum.No;

                //headerFooter.noActivityMsg
                headerFooter.noActivityMsg.fontsize = (Convert.DBNull == dr["NOACTIVITYMSGFONTSIZE"] ? null : dr["NOACTIVITYMSGFONTSIZE"].ToString());
                headerFooter.noActivityMsg.foreColor = (Convert.DBNull == dr["NOACTIVITYMSGCOLOR"] ? null : dr["NOACTIVITYMSGCOLOR"].ToString());
                headerFooter.noActivityMsgSpecified = headerFooter.noActivityMsg.IsFilled();

                //headerFooter.expiryIndicator
                headerFooter.expiryIndicator.daysSpecified = (Convert.DBNull != dr["EXPIRYINDICATORDAYS"]);
                if (headerFooter.expiryIndicator.daysSpecified)
                    headerFooter.expiryIndicator.days = Convert.ToInt32(dr["EXPIRYINDICATORDAYS"]);
                headerFooter.expiryIndicator.foreColor = (Convert.DBNull == dr["EXPIRYINDICATORCOLOR"] ? null : dr["EXPIRYINDICATORCOLOR"].ToString());
                headerFooter.expiryIndicatorSpecified = headerFooter.expiryIndicator.IsFilled();

                //headerFooter.sectionBanner
                headerFooter.sectionBanner.style = (Convert.DBNull == dr["SECTIONBANNERSTYLE"] ? null : dr["SECTIONBANNERSTYLE"].ToString());
                headerFooter.sectionBanner.align = (Convert.DBNull == dr["SECTIONBANNERALIGN"] ? null : dr["SECTIONBANNERALIGN"].ToString());
                headerFooter.sectionBanner.fontsize = (Convert.DBNull == dr["SECTIONBANNERFONTSIZE"] ? null : dr["SECTIONBANNERFONTSIZE"].ToString());
                headerFooter.sectionBanner.foreColor = (Convert.DBNull == dr["SECTIONBANNERFORECOLOR"] ? null : dr["SECTIONBANNERFORECOLOR"].ToString());
                headerFooter.sectionBanner.backColor = (Convert.DBNull == dr["SECTIONBANNERBACKCOLOR"] ? null : dr["SECTIONBANNERBACKCOLOR"].ToString());
                headerFooter.sectionBannerSpecified = headerFooter.sectionBanner.IsFilled();

                //headerFooter.assetBanner
                headerFooter.assetBanner.style = (Convert.DBNull == dr["ASSETBANNERSTYLE"] ? null : dr["ASSETBANNERSTYLE"].ToString());
                headerFooter.assetBanner.align = (Convert.DBNull == dr["ASSETBANNERALIGN"] ? null : dr["ASSETBANNERALIGN"].ToString());
                headerFooter.assetBanner.fontsize = (Convert.DBNull == dr["ASSETBANNERFONTSIZE"] ? null : dr["ASSETBANNERFONTSIZE"].ToString());
                headerFooter.assetBanner.foreColor = (Convert.DBNull == dr["SECTIONBANNERFORECOLOR"] ? null : dr["ASSETBANNERFORECOLOR"].ToString());
                headerFooter.assetBanner.backColor = (Convert.DBNull == dr["ASSETBANNERBACKCOLOR"] ? null : dr["ASSETBANNERBACKCOLOR"].ToString());
                headerFooter.assetBannerSpecified = headerFooter.assetBanner.IsFilled();

                // FI 20160624 [22286] add hCancelMsg
                headerFooter.hCancelMsg.fontsize = (Convert.DBNull == dr["HCANCELMSGFONTSIZE"] ? null : dr["HCANCELMSGFONTSIZE"].ToString());
                headerFooter.hCancelMsg.foreColor = (Convert.DBNull == dr["HCANCELMSGCOLOR"] ? null : dr["HCANCELMSGCOLOR"].ToString());
                headerFooter.hCancelMsg.msg = (Convert.DBNull == dr["HCANCELMSG"] ? null : dr["HCANCELMSG"].ToString());
                headerFooter.hCancelMsgSpecified = headerFooter.hCancelMsg.IsFilled();

                SetSectionTitle(headerFooter, (Convert.DBNull == dr["SECTIONTITLE"] ? null : dr["SECTIONTITLE"].ToString()));

                SetJournalEntries(headerFooter, (Convert.DBNull == dr["JOURNALENTRIES"] ? null : dr["JOURNALENTRIES"].ToString()));
                // FI 20160530 [21885] SetCollateral
                SetCollateral(headerFooter, (Convert.DBNull == dr["COLLATERAL_SOD"] ? null : dr["COLLATERAL_SOD"].ToString()));
                // FI 20160613 [22256] SetUnderlyingStock
                SetUnderlyingStock(headerFooter, (Convert.DBNull == dr["COLLATERAL_UOD"] ? null : dr["COLLATERAL_UOD"].ToString()));



            }
        }

        /// <summary>
        ///  Alimente pHeaderFooter.journalEntry à partir des informations présentes dans la colonne JOURNALENTRIES
        ///  <para>Lorsque la colonne est non renseignée,journalEntry est aliménté avec les valeurs par défaut</para>
        /// </summary>
        /// <param name="pHeaderFooter"></param>
        /// <param name="pColumnJNEntries">Valeur de la colonne JOURNALENTRIES</param>
        /// FI 20150413 [20275] Add method
        /// FI 20150413 [20275] Add method
        /// FI 20151019 [21317] Modify 
        private static void SetJournalEntries(HeaderFooter pHeaderFooter, string pColumnJNEntries)
        {
            string journalEntriesValue = pColumnJNEntries;

            List<LabelDescription> journalEntries = new List<LabelDescription>();
            Dictionary<string, string> dic = GetKeyValue(journalEntriesValue);
            if (ArrFunc.IsFilled(dic))
            {
                foreach (string key in dic.Keys)
                    journalEntries.Add(new LabelDescription() { key = key, description = dic[key] });
            }

            string defaultDescFDA = "{ASSET_ALTIDENTIFIER}.overflow-ellipsis(82tl) {ASSET_ISINCODE} / {FMTQTY} at {PCTRATE}% ({RESDAYCOUNT})";
            if ((false == journalEntries.Exists(x => x.key == "FDA")))
                journalEntries.Add(new LabelDescription() { key = "FDA", description = defaultDescFDA });

            if ((false == journalEntries.Exists(x => x.key == "BWA")))
                journalEntries.Add(new LabelDescription() { key = "BWA", description = defaultDescFDA });

            // FI 20150727 [XXXXX] add Default sur SKP
            string defaultDescSKP = "{ASSET_ALTIDENTIFIER}.overflow-ellipsis(82tl) {ASSET_ISINCODE} / {FMTQTY}";
            if ((false == journalEntries.Exists(x => x.key == "SKP")))
                journalEntries.Add(new LabelDescription() { key = "SKP", description = defaultDescSKP });

            // FI 20151019 [21317] add Default sur INT (interest/coupon)
            string defaultDescINT = "{ASSET_ALTIDENTIFIER}.overflow-ellipsis(82tl) {ASSET_ISINCODE} / {FMTQTY} at {PCTRATE}% ({RESDAYCOUNT})";
            if ((false == journalEntries.Exists(x => x.key == "INT")))
                journalEntries.Add(new LabelDescription() { key = "INT", description = defaultDescINT });

            string defaultDescCashPayment = @"<choose><when test=""{SIDE}='CR'"">Deposit/Credit</when><otherwise>Withdrawal/Debit</otherwise></choose>({EXTTYPE})";
            if ((false == journalEntries.Exists(x => x.key == "***")))
                journalEntries.Add(new LabelDescription() { key = "***", description = defaultDescCashPayment });

            pHeaderFooter.journalEntrySpecified = (journalEntries.Count > 0);
            if (pHeaderFooter.journalEntrySpecified)
                pHeaderFooter.journalEntry = journalEntries;
        }

        /// <summary>
        /// Alimente pHeaderFooter.sectionTitle à partir des informations présentes dans la colonne SECTIONTITLE
        /// </summary>
        /// <param name="pHeaderFooter"></param>
        /// <param name="pColumnSectionTitle"></param>
        /// FI 20150413 [20275] Add method
        private static void SetSectionTitle(HeaderFooter pHeaderFooter, string pColumnSectionTitle)
        {
            string sectionsValues = pColumnSectionTitle;

            List<SectionTitle> sections = new List<SectionTitle>();
            Dictionary<string, string> dic = GetKeyValue(sectionsValues);
            if (ArrFunc.IsFilled(dic))
            {
                foreach (string key in dic.Keys)
                    sections.Add(new SectionTitle() { key = key, title = dic[key] });
            }

            pHeaderFooter.sectionTitleSpecified = (sections.Count > 0);
            if (pHeaderFooter.sectionTitleSpecified)
                pHeaderFooter.sectionTitle.section = sections;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColumnValue"></param>
        /// <returns></returns>
        /// FI 20151016 [21458] Modify 
        private static Dictionary<string, string> GetKeyValue(string pColumnValue)
        {
            Dictionary<string, string> ret = new Dictionary<string, string>();

            if (StrFunc.IsFilled(pColumnValue))
            {
                string[] res = pColumnValue.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                if (ArrFunc.Count(res) > 0)
                {
                    foreach (string item in res)
                    {
                        string[] title = item.Split(':');
                        if ((ArrFunc.Count(title) == 2) && (false == ret.Keys.Contains(title[0])))
                            ret.Add(title[0].Trim(), title[1].Trim().Replace(@"\r\n", Cst.CrLf)); // FI 20151016 [21458] interprétation du \r\n
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///  Nettoie le product CashBalance de manière à ce qu'il ne contiennent uniquement le strict nécessaire pour générer une édition de type FINANCIALPERIODIC
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTrade"></param>
        /// FI 20120731 [18048]  
        private void UpdateTradeforFinancialReportPeriodic(string pCS, ITrade pTrade)
        {
            RemoveTradeHeaderLinkId(pTrade.TradeHeader);
            pTrade.TradeSideSpecified = false;

            CashBalance cashBalance = (CashBalance)pTrade.Product;
            UpdateProductforFinancialReportPeriodic(pCS, cashBalance);
        }

        /// <summary>
        /// set null linkId element 
        /// </summary>
        /// <param name="pPartyTradeIdentifier"></param>
        private static void RemoveTradeHeaderLinkId(ITradeHeader pTradeHeader)
        {
            if (ArrFunc.IsFilled(pTradeHeader.PartyTradeIdentifier))
            {
                foreach (IPartyTradeIdentifier partyTradeIdentifier in pTradeHeader.PartyTradeIdentifier)
                {
                    partyTradeIdentifier.LinkIdSpecified = false;
                }
            }
        }

        /// <summary>
        ///  Mise à jour du produit cashBalance pour une édition de type FINANCIALPERIODIC
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pProduct"></param>
        /// FI 20130701 [18745] appel à la méthode CashBalanceCalcAjustedDate
        /// FI 20140910 [20275] Modify (Modifications effectuées de façon empirique)
        private void UpdateProductforFinancialReportPeriodic(string pCS, CashBalance pProduct)
        {
            CashBalance cashBalance = pProduct;
            cashBalance.settings = null;

            CashBalanceCalcAjustedDate(pCS, cashBalance);

            foreach (CashBalanceStream stream in cashBalance.cashBalanceStream)
            {
                //suppression des éléments non nécessaires
                stream.marginRequirement = null;

                stream.cashUsedSpecified = false;
                stream.cashUsed = null;

                stream.collateralAvailable = null;
                stream.collateralUsedSpecified = false;

                stream.collateralUsed = null;

                stream.uncoveredMarginRequirementSpecified = false;
                stream.uncoveredMarginRequirement = null;

                stream.previousMarginConstituentSpecified = false;
                stream.previousMarginConstituent = null;

                stream.realizedMarginSpecified = false;
                stream.realizedMargin = null;

                stream.unrealizedMarginSpecified = false;
                stream.unrealizedMargin = null;

                stream.liquidatingValueSpecified = false;
                stream.liquidatingValue = null;

                //FI 20140910 [20275] funding,forwardCashPayment,equityBalance etc... sont mis à null
                stream.fundingSpecified = false;
                stream.funding = null;

                stream.forwardCashPaymentSpecified = false;
                stream.forwardCashPayment = null;

                stream.equityBalanceSpecified = false;
                stream.equityBalance = null;

                stream.equityBalanceWithForwardCashSpecified = false;
                stream.equityBalanceWithForwardCash = null;

                stream.excessDeficitSpecified = false;
                stream.excessDeficit = null;

                stream.excessDeficitWithForwardCashSpecified = false;
                stream.excessDeficitWithForwardCash = null;


                //Suppression de certains éléments parmi les éléments nécessaires
                #region Purge cashAvailable

                //cashAvailable => suppression de la date, elle est déjà présente  dans trade\tradeHeader\tradeDate 
                stream.cashAvailable.dateReferenceSpecified = false;
                stream.cashAvailable.dateDefineSpecified = false;


                CashBalancePayment cashBalancePayment = stream.cashAvailable.constituent.cashBalancePayment;
                //cashBalancePayment => suppression de la date, elle est déjà présente  dans trade\tradeHeader\tradeDate 
                cashBalancePayment.dateDefineSpecified = false;
                cashBalancePayment.dateReferenceSpecified = false;
                //FI 20140910 [20275] => suppression de la date sur cashDeposit et sur cashWithdrawal
                if (cashBalancePayment.cashDepositSpecified)
                {
                    cashBalancePayment.cashDeposit.dateDefineSpecified = false;
                    cashBalancePayment.cashDeposit.dateReferenceSpecified = false;
                }
                if (cashBalancePayment.cashWithdrawalSpecified)
                {
                    cashBalancePayment.cashWithdrawal.dateDefineSpecified = false;
                    cashBalancePayment.cashWithdrawal.dateReferenceSpecified = false;
                }


                CashFlows cashFlows = stream.cashAvailable.constituent.cashFlows;
                DetailedContractPayment[] fee = cashFlows.constituent.fee;
                if (ArrFunc.IsFilled(fee))
                {
                    foreach (DetailedContractPayment item in fee)
                    {
                        item.paymentSource = null;
                        if (ArrFunc.IsFilled(item.tax))
                        {
                            foreach (Tax tax in item.tax)
                            {
                                tax.taxSource = null;
                                if (ArrFunc.IsFilled(tax.taxDetail))
                                {
                                    foreach (TaxSchedule taxDetail in tax.taxDetail)
                                        taxDetail.taxSource = null;
                                }
                            }
                        }
                    }
                }
                #endregion

                #region purge cashBalance
                //cashBalance => suppression de la date, elle est déjà présente  dans trade\tradeHeader\tradeDate 
                stream.cashBalance.dateDefineSpecified = false;
                stream.cashBalance.dateReferenceSpecified = false;
                #endregion

            }
            //
            if (cashBalance.exchangeCashBalanceStreamSpecified)
            {
                //FI 20120731 [18048] GLOP 
                //Que faire ??
                //Spheres® ne gère pas les cashBalances avec devise de contrevaleur ACU 
                //Il faudra revoir cela 
            }
        }

        /// <summary>
        ///  Calcul de la date ajustée si une adjustableDate est spécifié
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSimplePayment"></param>
        /// FI 20150427 [20987] Modify
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CalcAdjustedDate(string pCS, ISimplePayment pSimplePayment, DataDocumentContainer pDataDocument)
        {
            // FI 20150427 [20987] test sur pSimplePayment.paymentDate != null => dans les états périodiques paymentDate == null
            if ((null != pSimplePayment.PaymentDate) && pSimplePayment.PaymentDate.AdjustableDateSpecified)
            {
                // FI 20200520 [XXXXX] Add SQL cache
                EFS_AdjustableDate calc = new EFS_AdjustableDate(CSTools.SetCacheOn(pCS), pSimplePayment.PaymentDate.AdjustableDate, pDataDocument);
                pSimplePayment.PaymentDate.AdjustedDate.DateValue = calc.adjustedDate.DateValue;
                pSimplePayment.PaymentDate.AdjustedDateSpecified = true;
                pSimplePayment.PaymentDate.AdjustableDateSpecified = false;
                pSimplePayment.PaymentDate.RelativeDateSpecified = false;
            }
        }

        /// <summary>
        ///  Calcul de la date ajustée si une adjustableDate est spécifié
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPayment"></param>
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CalcAdjustedDate(string pCS, IPayment pPayment, DataDocumentContainer pDataDocument)
        {
            if (pPayment.PaymentDateSpecified)
            {
                EFS_AdjustableDate calc = new EFS_AdjustableDate(pCS, pPayment.PaymentDate, pDataDocument);
                pPayment.AdjustedPaymentDate = calc.adjustedDate.DateValue;
                pPayment.AdjustedPaymentDateSpecified = true;
                pPayment.PaymentDateSpecified = false; ;
            }
        }

        /// <summary>
        ///  Recherche des trades en positions business Date et positions setllement Date en date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// <param name="pIsAddCommonDataTrade"></param>
        /// FI 20150623 [21149] Modify
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modity
        /// FI 20160412 [22069] Modify
        /// FI 20161214 [21916] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// FI 20200519 [XXXXX] Add pIsAddCommonDataTrade
        private void SetPosTradesReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate, Boolean pIsAddCommonDataTrade)
        {
            // FI 20160225 [XXXXX] Add test 
            // FI 20160412 [22069] Excluding Cashpayment
            // FI 20161214 [21916] Add ExistFungibilityProduct
            if (productEnv.Count(true) > 0 && productEnv.ExistFungibilityProduct())
            {
                // FI 20150622 [21124] utilisation de additionnalJoin
                string additionnalJoin = @"inner join dbo.TRADELINK tlcb on (tlcb.IDT_B = t.IDT) and (tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance') and (tlcb.IDT_A = @IDT)";

                // Recherche des trades en positions rattachées au cash-Balance {pidT}
                QueryParameters qry = GetQueryTradePosition2(pCS, pDate, PostionType.Business, additionnalJoin, productEnv);
                qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                if (StrFunc.IsFilled(qry.Query))
                {
                    DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                    DataRow[] row = dt.Select();
                    if (ArrFunc.IsFilled(row))
                    {
                        AddPosTrades(pCS, NotificationDocument.PosTrades, pDate, row);
                        NotificationDocument.PosTradesSpecified = true;
                    }
                }

                // FI 20151019 [21317] Recherche des positions en settlement uniquement s'il existe une activité titre 
                bool isExistSEC = productEnv.ExistSEC();
                if (isExistSEC)
                {
                    // Recherche des trades en positons en date setlement rattachées au cash-Balance {pidT}
                    qry = GetQueryTradePosition2(pCS, pDate, PostionType.Settlement, additionnalJoin, productEnv);
                    qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                    DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                    DataRow[] row = dt.Select();
                    if (ArrFunc.IsFilled(row))
                    {
                        AddPosTrades(pCS, NotificationDocument.StlPosTrades, pDate, row);
                        NotificationDocument.StlPosTradesSpecified = true;
                    }
                }

                // FI 20200519 [XXXXX] Alimentation si pIsAddCommonDataTrade
                if (pIsAddCommonDataTrade)
                {
                    List<PosTrade> posAllTrades = GetAllPosTrades(pDate);
                    foreach (PosTrade item in posAllTrades)
                        AddCommonDataTrade(pCS, item);
                }
            }
        }

        /// <summary>
        ///  Ajoute d'un item dans {pLstPosTrades}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="lstPosTrades"></param>
        /// <param name="pDate">Date d'observation</param>
        /// <param name="pDr">Liste des enregistrements pris en considération à la date d'observation</param>
        // EG 20190730 Set assetMeasure associated with closingPrice and rate (AccrualInterestRate)
        private void AddPosTrades(string pCS, List<PosTrades> pLstPosTrades, DateTime pDate, DataRow[] pDr)
        {
            List<PosTrade> lstTradePosition = new List<PosTrade>();
            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
            PosTrades posTrades = pLstPosTrades.Find(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate));
            PosTrade trade = null;

            foreach (DataRow dr in pDr)
            {
                if (posTrades != null)
                {
                    trade = posTrades.trade.Find(x => x.OTCmlId == Convert.ToInt32(dr["IDT"]));
                    if (trade != null)
                        continue;
                }

                trade = new PosTrade();
                InitPosTradeCommon(trade, dr);

                trade.assetMeasureClosingPriceSpecified = (dr.Table.Columns.Contains("ASSETMEASURE") &&
                    (false == Convert.IsDBNull(dr["ASSETMEASURE"])));
                if (trade.assetMeasureClosingPriceSpecified)
                    trade.assetMeasureClosingPrice = ReflectionTools.ConvertStringToEnum<AssetMeasureEnum>(dr["ASSETMEASURE"].ToString());

                // FI 20150320 [XXPOC] closing price est exprimé dans la base du DC 
                // closing price était exprimée en base 100 avant cette modification
                trade.closingPriceSpecified = (false == Convert.IsDBNull(dr["QUOTEPRICE"]));
                if (trade.closingPriceSpecified)
                {
                    trade.closingPrice = Convert.ToDecimal(dr["QUOTEPRICE"]);
                    trade.fmtClosingPriceSpecified = true;
                    trade.fmtClosingPrice = BuildFmtPrice(pCS,
                                new Pair<Cst.UnderlyingAsset, int>(trade.assetCategory, trade.idAsset),
                                trade.closingPrice, null);
                }


                //UMG
                trade.umgSpecified = (dr["UMGAMOUNT"] != Convert.DBNull);
                if (trade.umgSpecified)
                {
                    trade.umg = new ReportAmountSide
                    {
                        amount = (dr["UMGAMOUNT"] != Convert.DBNull) ? Convert.ToDecimal(dr["UMGAMOUNT"]) : Decimal.Zero,
                        currency = Convert.ToString(dr["UMGUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["UMGSIDE"]))
                    };
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    trade.umg.sideSpecified = (trade.umg.amount > 0);
                }

                //LOV
                trade.novSpecified = (dr["LOVAMOUNT"] != Convert.DBNull);
                if (trade.novSpecified)
                {
                    trade.nov = new ReportAmountSide
                    {
                        amount = Convert.ToDecimal(dr["LOVAMOUNT"]),
                        currency = Convert.ToString(dr["LOVUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["LOVSIDE"]))
                    };
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    trade.nov.sideSpecified = (trade.nov.amount > 0);
                }

                //PRM
                trade.prmSpecified = (dr["PRMAMOUNT"] != Convert.DBNull);
                if (trade.prmSpecified)
                {
                    trade.prm = new ReportAmountSide
                    {
                        amount = Convert.ToDecimal(dr["PRMAMOUNT"]),
                        currency = Convert.ToString(dr["PRMUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["PRMSIDE"]))
                    };
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    trade.prm.sideSpecified = (trade.prm.amount > 0);
                }

                //MKV
                trade.mkvSpecified = (dr["MKVAMOUNT"] != Convert.DBNull);
                if (trade.mkvSpecified)
                {
                    trade.mkv = new ReportAmountSide
                    {
                        amount = Convert.ToDecimal(dr["MKVAMOUNT"]),
                        currency = Convert.ToString(dr["MKVUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["MKVSIDE"]))
                    };
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    trade.mkv.sideSpecified = (trade.mkv.amount > 0);
                }

                //PAM
                trade.pamSpecified = (dr["PAMAMOUNT"] != Convert.DBNull);
                if (trade.pamSpecified)
                {
                    trade.pam = new ReportAmountSide
                    {
                        amount = Convert.ToDecimal(dr["PAMAMOUNT"]),
                        currency = Convert.ToString(dr["PAMUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["PAMSIDE"]))
                    };
                    trade.pam.sideSpecified = (trade.pam.amount > 0);
                }

                //AIN
                trade.ainSpecified = (dr["AINAMOUNT"] != Convert.DBNull);
                if (trade.ainSpecified)
                {
                    trade.ain = new ReportAmountAccruedInterest
                    {
                        amount = Convert.ToDecimal(dr["AINAMOUNT"]),
                        currency = Convert.ToString(dr["AINUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["AINSIDE"]))
                    };
                    trade.ain.sideSpecified = (trade.ain.amount > 0);
                    trade.ain.daysSpecified = (dr["AINDAYS"] != Convert.DBNull);
                    if (trade.ain.daysSpecified)
                        trade.ain.days = Convert.ToInt32(dr["AINDAYS"]);
                    trade.ain.rateSpecified = (dr["AINRATE"] != Convert.DBNull);
                    trade.ain.fmtRateSpecified = trade.ain.rateSpecified;
                    if (trade.ain.rateSpecified)
                    {
                        // FI 20191203 [XXXXX] Alimentation de fmtRate (4 decimale min)
                        trade.ain.rate = Convert.ToDecimal(dr["AINRATE"]);
                        trade.ain.fmtRate = StrFunc.FmtDecimalToInvariantCulture(trade.ain.rate, 4);
                    }
                }

                //MGR
                trade.mgrSpecified = (dr["MGRAMOUNT"] != Convert.DBNull);
                if (trade.mgrSpecified)
                {
                    trade.mgr = new ReportAmountSideRatio
                    {
                        amount = Convert.ToDecimal(dr["MGRAMOUNT"]),
                        currency = Convert.ToString(dr["MGRUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["MGRSIDE"]))
                    };
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    trade.mgr.sideSpecified = (trade.mgr.amount > 0);
                    trade.mgr.factor = Convert.ToDecimal(dr["MGRFACTOR"]);
                }

                lstTradePosition.Add(trade);

                //AddCommonDataTrade(pCS, trade);

            }

            if (ArrFunc.IsFilled(lstTradePosition))
            {
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message 
                //pLstPosTrades.Add(new PosTrades() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), trade = lstTradePosition });
                //PosTrades posTrades = pLstPosTrades.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                //if (null == posTrades)
                //    throw new NotSupportedException("no trade found");

                if (posTrades == null)
                {
                    posTrades = new PosTrades() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), trade = lstTradePosition };
                    pLstPosTrades.Add(posTrades);
                }
                else
                    posTrades.trade.AddRange(lstTradePosition);

                foreach (PosTrade item in lstTradePosition)
                    CalcPosTradesSubSubTotal(pCS, posTrades, item);
            }
        }

        /// <summary>
        /// Alimentation des positions détaillés en date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Représente tous les trades qui constituent la position</param>
        /// <param name="pDate"></param>
        // RD 20191112 [25062] Modify
        private void SetPosTrades(string pCS, int[] pIdT, DateTime pDate)
        {
            // RD 20191112 [25062] Don't use trade business date criteria for Position
            //string additionalJoin = GetJoinTrade(pCS, pIdT, "t.IDT", true);
            string additionalJoin = GetJoinTrade(pCS, pIdT, "t.IDT", false);

            // Recherche des trades en positons  
            QueryParameters qry = GetQueryTradePosition2(pCS, pDate, PostionType.Business, additionalJoin, null);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            DataRow[] row = dt.Select();
            if (ArrFunc.IsFilled(row))
            {
                AddPosTrades(pCS, NotificationDocument.PosTrades, pDate, row);
                NotificationDocument.PosTradesSpecified = true;
            }

        }

        /// <summary>
        /// Alimente la liste {pIdT} avec les trades exécuté ou non réglé ou réglés en date {pDate} 
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pDate"></param>
        /// FI 20150623 [21149] Add Method
        private void AddAllTrades(List<int> pIdT, DateTime pDate)
        {
            List<TradeReport> allTrades = GetAllTrades(pDate);
            foreach (TradeReport item in allTrades)
            {
                if (false == pIdT.Contains(item.OTCmlId))
                {
                    pIdT.Add(item.OTCmlId);
                    if ((item.tradeSrcSpecified) && false == pIdT.Contains(item.tradeSrc.OTCmlId))
                        pIdT.Add(item.tradeSrc.OTCmlId);
                }
            }
        }

        /// <summary>
        /// Alimente la liste {pIdT} avec les trades en positon (posTrades + stlPosTrades)
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pDate">Date d'observation</param>
        /// FI 20150623 [21149] Add Method
        private void AddAllPosTrades(List<int> pIdT, DateTime pDate)
        {
            List<PosTrade> allTrades = GetAllPosTrades(pDate);
            foreach (PosTrade item in allTrades)
            {
                if (false == pIdT.Contains(item.OTCmlId))
                    pIdT.Add(item.OTCmlId);
            }
        }

        /// <summary>
        /// Alimente la liste {pIdT} avec les trades qui ont subi des actions sur position
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pDate">Date d'observation</param>
        /// FI 20150623 [21149] Add Method
        private void AddAllPosActionsTrades(List<int> pIdT, DateTime pDate)
        {
            List<PosAction> allPosActions = GetAllPosAction(pDate);

            foreach (PosAction item in allPosActions)
            {
                if (false == pIdT.Contains(item.trades.trade.OTCmlId))
                    pIdT.Add(item.trades.trade.OTCmlId);

                if ((item.trades.trade2Specified) && (false == pIdT.Contains(item.trades.trade2.OTCmlId)))
                {
                    pIdT.Add(item.trades.trade2.OTCmlId);
                }
            }
        }

        /// <summary>
        /// Alimente la liste {pIdT} avec les trades qui donne lieu à paiement contre livraison de ss-jacent
        /// </summary>
        /// <param name="pIdT"></param>
        /// <param name="pDate">Date d'observation</param>
        /// FI 20170217 [22862] Add 
        private void AddDvlTrades(List<int> pIdT, DateTime pDate)
        {
            List<DlvTrade> dlvTrades = GetDeliveryTrade(pDate);

            foreach (DlvTrade item in dlvTrades)
            {
                if (false == pIdT.Contains(item.OTCmlId))
                    pIdT.Add(item.OTCmlId);
            }
        }





  



        /// <summary>
        /// Mise à jour du DataDocument
        /// <para>En générale, cette méthode supprime tous les élements non nécessaires pour réduire la taille du flux</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pNotificationType"></param>
        private void UpdateDataDocument2(string pCS, NotificationTypeEnum pNotificationType)
        {
            IDataDocument dataDocument = NotificationDocument.DataDocument;

            switch (pNotificationType)
            {
                case NotificationTypeEnum.FINANCIALPERIODIC:
                case NotificationTypeEnum.FINANCIAL:
                    if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
                    {
                        ReplaceProductCashBalance(pCS, dataDocument.FirstTrade);
                    }
                    else if (pNotificationType == NotificationTypeEnum.FINANCIALPERIODIC)
                    {
                        // RD 20131016 [18585] Bug sur l'extrait de compte / il existe plusieurs trades CashBalance
                        //UpdateTradeforFinancialReportPeriodic(pCS, dataDocument.firstTrade);
                        for (int i = 0; i < ArrFunc.Count(dataDocument.Trade); i++)
                            UpdateTradeforFinancialReportPeriodic(pCS, dataDocument.Trade[i]);
                    }
                    break;
                case NotificationTypeEnum.SYNTHESIS:
                case NotificationTypeEnum.ALLOCATION:
                case NotificationTypeEnum.POSACTION:
                case NotificationTypeEnum.POSITION:
                case NotificationTypeEnum.ORDERALLOC: // FI 20190515 [23912]
                    if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
                    {
                        // FI 20130704 [18745] ReplaceProductCashBalance ne doit être effectué que sur SYNTHESIS
                        // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                        //if (pNotificationType.Value == NotificationTypeEnum.SYNTHESIS)
                        //    ReplaceProductCashBalance(pCS, dataDocument.firstTrade);
                        if (pNotificationType == NotificationTypeEnum.SYNTHESIS)
                        {
                            ITrade[] trade = (from item in dataDocument.Trade where item.Product.ProductBase.IsCashBalance select item).ToArray();

                            for (int i = 0; i < ArrFunc.Count(trade); i++)
                                ReplaceProductCashBalance(pCS, trade[i]);
                        }
                        CleanTradeDataDocument(dataDocument);
                    }
                    break;
                case NotificationTypeEnum.POSSYNTHETIC:
                    if (NotificationDocument.EfsMLversion == EfsMLDocumentVersionEnum.Version30)
                        dataDocument.Trade = null;
                    break;
            }
        }



        /// <summary>
        /// Alimente la donnée ITrade.tradeId
        /// </summary>
        /// <param name="pTrade"></param>
        /// FI 20130621 [18745] add Method
        private static void SetTradeId(ITrade pTrade)
        {
            if (false == pTrade.TradeIdSpecified)
            {
                string identifier = string.Empty;
                if (ArrFunc.IsFilled(pTrade.TradeHeader.PartyTradeIdentifier))
                {
                    foreach (ITradeIdentifier partyTradeIdentifier in pTrade.TradeHeader.PartyTradeIdentifier)
                    {
                        ISchemeId tradeId = partyTradeIdentifier.GetTradeIdFromScheme(Cst.OTCml_TradeIdScheme);
                        if ((null != tradeId) && (tradeId.Value != "0"))
                        {
                            identifier = tradeId.Value;
                            break;
                        }
                    }
                }
                pTrade.TradeIdSpecified = StrFunc.IsFilled(identifier);
                if (pTrade.TradeIdSpecified)
                    pTrade.TradeId = identifier;
            }
        }

        /// <summary>
        /// Purge des trades à l'extrême pour ne conserver que le strict minimum 
        /// </summary>
        /// <param name="pDataDocument"></param>
        /// FI 20130621 [18745]
        /// FI 20140821 [20275] Modify
        /// FI 20140218 [20275] Modify
        /// FI 20150316 [20275] Modify
        /// FI 20150331 [XXPOC] Modify
        /// FI 20151019 [21317] Modify
        private static void CleanTradeDataDocument(IDataDocument pDataDocument)
        {
            for (int i = 0; i < ArrFunc.Count(pDataDocument.Trade); i++)
            {
                ITrade trade = pDataDocument.Trade[i];

                trade.BrokerPartyReferenceSpecified = false;
                trade.CalculationAgentSpecified = false;
                trade.DocumentationSpecified = false;
                trade.ExtendsSpecified = false;
                trade.GoverningLawSpecified = false;
                trade.NettingInformationInputSpecified = false;
                trade.OtherPartyPaymentSpecified = false;
                trade.SettlementInputSpecified = false;
                trade.TradeIntentionSpecified = false;
                trade.TradeSideSpecified = false;

                trade.TradeHeader = null;

                trade.Product.ProductBase.Id = null;
                // FI 20140218 [20275] productType est conservé (sauf scheme) de mainière à afficher l'IdI
                trade.Product.ProductBase.ProductType.Scheme = null;

                RemoveCurrencyScheme(trade.Product.ProductBase);
                // FI 20150331 [XXPOC] Appel à RemoveBusinessCenterScheme
                RemoveBusinessCenterScheme(trade.Product.ProductBase);

                if (trade.Product.ProductBase.IsExchangeTradedDerivative)
                {
                    IExchangeTradedDerivative etdProduct = trade.Product as IExchangeTradedDerivative;
                    etdProduct.TradeCaptureReport.Instrument.AlternateId = null;
                }
                else if (trade.Product.ProductBase.IsEquitySecurityTransaction)
                {
                    // FI 20140821 [20275] add EquitySecurityTransaction
                    IEquitySecurityTransaction estProduct = trade.Product as IEquitySecurityTransaction;
                    estProduct.TradeCaptureReport.Instrument.AlternateId = null;
                    // FI 20150316 [20275] grossAmount est à null
                    estProduct.GrossAmount = null;
                }
                else if (trade.Product.ProductBase.IsDebtSecurityTransaction)
                {
                    IDebtSecurityTransaction dstProduct = trade.Product as IDebtSecurityTransaction;
                    dstProduct.GrossAmount = null;
                    // FI 20151019 [21317] debtSecuritySpecified à false pour alléger le flux
                    dstProduct.SecurityAsset.DebtSecuritySpecified = false;
                }
                else if (trade.Product.ProductBase.IsReturnSwap)
                {
                    // FI 20140821 [20275] Reduction au minimum des ReturnSwap
                    // FI 20140901 [20275] pas de buyer, seller, payer, receiver (RptSide suffit) 
                    // FI 20140901 [20275] pas de returnLeg.rateOfReturn.marginRatio (Lecture des évènements pour cela)
                    IReturnSwap returnSwap = trade.Product as IReturnSwap;
                    returnSwap.BuyerPartyReferenceSpecified = false;
                    returnSwap.SellerPartyReferenceSpecified = false;

                    if (returnSwap.ReturnLegSpecified)
                    {
                        foreach (IReturnLeg returnLeg in returnSwap.ReturnLeg)
                        {
                            returnLeg.PayerPartyReference = null;
                            returnLeg.ReceiverPartyReference = null;
                            returnLeg.EffectiveDate = null;
                            returnLeg.TerminationDate = null;
                            if (returnLeg.Underlyer.UnderlyerBasketSpecified)
                            {
                                //FI 20140821 [20275] TODO Que faire ici ? En attendant underlyerBasketSpecified = false;
                                returnLeg.Underlyer.UnderlyerBasketSpecified = false;
                            }
                            else if (returnLeg.Underlyer.UnderlyerSingleSpecified)
                            {
                                ISingleUnderlyer underlyer = returnLeg.Underlyer.UnderlyerSingle;
                                // Les informations suivantes seront dans le repository
                                underlyer.UnderlyingAsset.DescriptionSpecified = false;
                                underlyer.UnderlyingAsset.ClearanceSystemSpecified = false;
                                underlyer.UnderlyingAsset.ExchangeIdSpecified = false;
                                underlyer.UnderlyingAsset.CurrencySpecified = false;

                                //FI 20140821 [20275] TODO peut-être moins systématique ? 
                                foreach (IScheme sheme in underlyer.UnderlyingAsset.InstrumentId)
                                    sheme.Scheme = null;

                                if (underlyer is IFxRateAsset fxrate)
                                {
                                    fxrate.QuotedCurrencyPair = null;
                                    fxrate.RateSourceSpecified = false;
                                }
                            }
                            returnLeg.RateOfReturn.PaymentDates = null;
                            returnLeg.RateOfReturn.ValuationPriceInterimSpecified = false;
                            returnLeg.RateOfReturn.ValuationPriceFinal = null;
                            returnLeg.RateOfReturn.MarginRatioSpecified = false;
                            returnLeg.ReturnSwapAmount = null;
                        }
                    }

                    returnSwap.InterestLegSpecified = false;
                    returnSwap.ExtraordinaryEventsSpecified = false;
                    returnSwap.AdditionalPaymentSpecified = false;
                    returnSwap.EarlyTerminationSpecified = false;
                    returnSwap.PrincipalExchangeFeaturesSpecified = false;
                }
                else if (trade.Product.ProductBase.IsFxLeg)
                {
                    IFxLeg fxLeg = trade.Product as IFxLeg;
                    //exchangedCurrency1
                    fxLeg.ExchangedCurrency1.PayerPartyReference = null;
                    fxLeg.ExchangedCurrency1.ReceiverPartyReference = null;
                    fxLeg.ExchangedCurrency1.PaymentDateSpecified = false;
                    fxLeg.ExchangedCurrency1.PaymentAmount.Id = null;
                    //exchangedCurrency2
                    fxLeg.ExchangedCurrency2.PayerPartyReference = null;
                    fxLeg.ExchangedCurrency2.ReceiverPartyReference = null;
                    fxLeg.ExchangedCurrency2.PaymentDateSpecified = false;
                    fxLeg.ExchangedCurrency2.PaymentAmount.Id = null;
                    //NDF
                    if (fxLeg.NonDeliverableForwardSpecified)
                    {
                        IFxCashSettlement fxCashSettlement = fxLeg.NonDeliverableForward;
                        foreach (IFxFixing item in fxCashSettlement.Fixing)
                            CleanFxFixing(item);
                    }
                }
                else if (trade.Product.ProductBase.IsFxOptionLeg)
                {
                    IFxOptionBase fxOptBase = trade.Product as IFxOptionBase;
                    fxOptBase.CallCurrencyAmount.Id = null;
                    fxOptBase.PutCurrencyAmount.Id = null;
                    fxOptBase.BuyerPartyReference = null;
                    fxOptBase.SellerPartyReference = null;
                    fxOptBase.FxOptionPremiumSpecified = false; /*La prime sera mise à dispo dans l'élément trade du jour*/

                    IFxOptionLeg fxOpt = trade.Product as IFxOptionLeg;
                    if (fxOpt.CashSettlementTermsSpecified)
                    {
                        IFxCashSettlement fxCashSettlement = fxOpt.CashSettlementTerms;
                        foreach (IFxFixing item in fxCashSettlement.Fixing)
                            CleanFxFixing(item);
                    }
                }
                else if (trade.Product.ProductBase.IsCommoditySpot)
                {
                    ICommoditySpot commoditySpot = trade.Product as ICommoditySpot;
                    commoditySpot.FixedLeg = null;
                    commoditySpot.PhysicalLeg = null;
                    commoditySpot.EffectiveDate = null;
                    commoditySpot.TerminationDate = null;
                    commoditySpot.SettlementCurrency = null;
                }
            }
        }

        /// <summary>
        /// Purge d'un fxFixing pour ne conserver que le strict minimum 
        /// <para>Les informations seront présentes dans le repository rattaché à l'asset fxRate</para>
        /// </summary>
        /// <param name="fxFixing"></param>
        /// FI 2015031 [XXPOC] Add 
        private static void CleanFxFixing(IFxFixing fxFixing)
        {
            if (fxFixing.PrimaryRateSource.OTCmlId > 0)
            {
                fxFixing.PrimaryRateSource.RateSource.Scheme = null;

                fxFixing.PrimaryRateSource.RateSourcePageSpecified = false;
                fxFixing.PrimaryRateSource.RateSourcePageHeadingSpecified = false;
                fxFixing.QuotedCurrencyPair = null;
                fxFixing.FixingTime = null;
            }
        }

        /// <summary>
        /// Affecte les schemes de tous les objects ICurrency présent dans {pProduct} avec null
        /// </summary>
        /// <param name="pProduct"></param>
        private static void RemoveCurrencyScheme(IProductBase pProduct)
        {
            ArrayList al = ReflectionTools.GetObjectsByType2(pProduct, pProduct.TypeofCurrency, true);
            if (al.Count > 0)
            {
                ICurrency[] currency = (ICurrency[])al.ToArray(typeof(ICurrency));
                foreach (ICurrency item in currency)
                    item.CurrencyScheme = null;
            }
        }

        /// <summary>
        /// Affecte les schemes de tous les objects IusinessCenter présent dans {pProduct} avec null
        /// </summary>
        /// <param name="pProduct"></param>
        /// FI 20150331 [XXPOC] Add Method 
        private static void RemoveBusinessCenterScheme(IProductBase pProduct)
        {
            ArrayList al = ReflectionTools.GetObjectsByType2(pProduct, pProduct.TypeofBusinessCenter, true);
            if (al.Count > 0)
            {
                IBusinessCenter[] bc = (IBusinessCenter[])al.ToArray(typeof(IBusinessCenter));
                foreach (IBusinessCenter item in bc)
                    item.BusinessCenterScheme = null;
            }
        }

        /// <summary>
        /// Alimentation des frais sur les trades
        /// </summary>
        /// FI 20130621 [18745] 
        /// FI 20140808 [20275] modify add parameter pCS
        /// FI 20141209 [XXXXX] modify
        /// EG 20150331 [XXPOC] FxLeg|FxOptionLeg
        /// FI 20150427 [20987] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20150825 [21287] Modify
        private void SetOppTrades(string pCS)
        {
            if (NotificationDocument.TradesSpecified || NotificationDocument.UnsettledTradesSpecified || NotificationDocument.SettledTradesSpecified)
            {
                SetOppfromDataDocument();

                //********************
                // Modification des frais pour prendre en considération les restitutions de frais en cas de transfert, correction 
                // S'applique uniquement aux unsettledTrades et settledTrades
                //******************** 
                if (NotificationDocument.UnsettledTradesSpecified || NotificationDocument.SettledTradesSpecified)
                {
                    // Rq : il peut y avoir plusieurs dates si état périodique
                    // Chaque date est traité séparément. Pour une date donnée, un trade donné est soit dans unsettledTrades soit dans settledTrades
                    var bizDates =
                    (from itemTrades in NotificationDocument.UnsettledTrades.Concat(NotificationDocument.SettledTrades)
                     select new DtFunc().StringDateISOToDateTime(itemTrades.bizDt)).Distinct();

                    foreach (var bizDateItem in bizDates)
                    {
                        List<TradeReport> trade = GetUnsettledAndSettledTrades(bizDateItem);
                        int[] idt = (from item in trade
                                     select item.OTCmlId).ToArray();

                        List<IPosactionTradeFee> posActionTradeFee = LoadPosactionTradeFee(pCS, bizDateItem, idt);

                        List<int> idT = (from item in posActionTradeFee select item.IdT).Distinct().ToList();
                        foreach (int itemTrade in idT)
                        {

                            // Frais portés par le trade à la saisie
                            ReportFee[] feeNego = trade.Where(x => x.OTCmlId == itemTrade).First().fee;

                            // Un trade pourrait être corrigé et/ou transféré à une date et dans les 2 cas il pourrait y avoir des restitutions de frais
                            // Cette liste contient donc au maximum 2 items  
                            List<ReportFee[]> feePosAction = (from item in (posActionTradeFee.Where(x => x.IdT == itemTrade))
                                                              select item.Fee).ToList();

                            // Fusion des frais par paymentType
                            ReportFee[] allfee = (from itemFeePosAction in feePosAction
                                                  from feeAction in itemFeePosAction
                                                  select feeAction).Concat(feeNego).ToArray();
                            ReportFee[] newReportFee = MergeFee(allfee);

                            TradeReport tradeReport = trade.Where(x => x.OTCmlId == itemTrade).First();
                            tradeReport.feeSpecified = true;
                            tradeReport.fee = newReportFee;
                        }
                    }
                }
            }
        }



        /// <summary>
        ///  Retourne la requête qui charge toutes les actions sur positions à la date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pAdditionalJoin"></param>
        /// <param name="pPosType"></param>
        /// <returns></returns>
        /// FI 20140122 [] Lecture de la colonne EVENTDET_ETD.SETTLTPRICE100
        /// RD 20140730 [20212] Lecture de la colonne EVENTDET_ETD.CLOSINGPRICE100 pour les Liquidations de Futures
        /// FI 20140820 [20275] Modify
        /// EG 20141208 Add POSITIONEFFECT (POSACTIONDET)
        /// FI 20150218 [20275] Modify 
        /// FI 20150522 [20275] Modify
        /// FI 20150617 [21124] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20151019 [21317] Modify  
        /// FI 20160229 [21830] Modify
        /// FI 20170524 [XXXXX] Modify 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryPosAction(string pCS, DateTime pDate, string pAdditionalJoin, PostionType pPosType)
        {
            string query = string.Empty;


            switch (pPosType)
            {
                case PostionType.Business:
                    #region Business
                    //La requête effectue une union car un trade compensé et décompensé le même jour donne naissance à 2 enregistrements 

                    //Remarque la requête coté UNCLEARING effectue isnull(pad.DTUPD,pr.DTINS)as DTSYS
                    //Ceci afin de palier au bug suivant pad.DTUPD est non renseigné en cas de décompenation (ceci est corrigé depuis le 01/05/2012)

                    //FI 20120430 Spheres exclue les POSACTIONS telles que REQUESTTYPE = 'UNCLEARING'
                    //Ces enregistrements matérialisent la qté compensée restante lorqu'il y a décompensation partielle

                    // RD 20130722 [18745] Charger le cours du sous-jacent

                    // EG 20130925 Refactoring (voir sqlCommand.xml => <item name = POSACTIONDET_SELECT>
                    // FI 20140820 [20275] add ASSETCATEGORY et ASSETCATEGORY2
                    // FI 20150218 [20275] add GPRODUCT, TRDTYPE, TRDSUBTYPE, TRDTYPE2, TRDSUBTYPE2
                    // FI 20150522 [20275] add ti.DTOUTADJ et ti2.DTOUTADJ
                    // FI 20150617 [21124] Utilisation de l'EVENTCLASS 'VAL'
                    // FI 20151019 [21317] add FAMILY  
                    // FI 20160229 [21830] Reecriture de l'expression table dont le nom d'alias est price
                    //                     => Lecture de CLOSINGPRICE100 si MOF 
                    //                     => Lecture de SETTLTPRICE100 si EXE,ASS
                    query = @"select rs.IDPA, rs.IDPADET, rs.REQUESTTYPE, rs.REQUESTMODE, rs.DTBUSINESS, rs.DTUNCLEARING, rs.QTY, rs.DTSYS, rs.DEACTIV, 
                    p.GPRODUCT,p.FAMILY,
                    tr.IDT as TRADE_IDT, tr.IDENTIFIER as TRADE_IDENTIFIER,
                    tr.IDASSET, tr.ASSETCATEGORY, tr.PRICE, tr.TRDTYPE, tr.TRDSUBTYPE, tr.DTOUTADJ,
                    tr2.IDT as TRADE2_IDT, tr2.IDENTIFIER as TRADE2_IDENTIFIER, 
                    tr2.IDASSET as IDASSET2, tr2.ASSETCATEGORY as ASSETCATEGORY2, tr2.PRICE as PRICE2, tr2.TRDTYPE as TRDTYPE2 ,tr2.TRDSUBTYPE as TRDSUBTYPE2, tr2.DTOUTADJ as DTOUTADJ2,
                    price.QUOTEPRICE100 as UNDERLYERPRICE
                    from
                    (
	                    select IDPA, IDPADET, REQUESTTYPE, REQUESTMODE, DTBUSINESS, DTUNCLEARING, QTY,
	                    TRADE_IDT, TRADE2_IDT, TRADELINK_IDT, DTINS as DTSYS, 0 as DEACTIV, POSITIONEFFECT                            
	                    from dbo.VW_POSACTIONDET
                        where DTBUSINESS = @DT
	
	                    union all
    
	                    select IDPA, IDPADET, REQUESTTYPE, REQUESTMODE, DTBUSINESS,  DTUNCLEARING,  QTY,
	                    TRADE_IDT, TRADE2_IDT, TRADELINK_IDT, DTUPD as DTSYS, 1 as DEACTIV, POSITIONEFFECT
	                    from dbo.VW_POSACTIONDET 
                        where DTUNCLEARING = @DT
                    ) rs
                    inner join dbo.TRADE tr on (tr.IDT = rs.TRADE_IDT)
                    inner join dbo.INSTRUMENT i on (i.IDI = tr.IDI)
                    inner join dbo.PRODUCT p on (p.IDP = i.IDP)
                    left outer join dbo.TRADE tr2 on (tr2.IDT = rs.TRADE2_IDT)
                    left outer join 
                    (
	                    select ev.IDT, case ev.EVENTCODE when 'MOF' then evdet.CLOSINGPRICE100 else evdet.SETTLTPRICE100 end as QUOTEPRICE100, epad.IDPADET 
	                    from dbo.EVENT ev
                        inner join dbo.EVENTCLASS ec on (ec.IDE = ev.IDE) and (ec.EVENTCLASS in ('PHY','CSH')) and (ec.DTEVENT = @DT)
                        inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ev.IDE)
                        left outer join dbo.EVENTDET evdet on (evdet.IDE = ev.IDE)
                        where ev.EVENTCODE in ('EXE', 'AEX', 'ABN', 'AAB', 'ASS', 'AAS', 'MOF') 
                    ) price  on (price.IDPADET = rs.IDPADET) and (price.IDT = rs.TRADE_IDT)
                    {0}";
                    // FI 20170524 [XXXXX] query est alimentée ici
                    query = StrFunc.AppendFormat(query, StrFunc.IsFilled(pAdditionalJoin) ? pAdditionalJoin : string.Empty);
                    #endregion
                    break;
                case PostionType.Settlement:
                    #region Settlement
                    /* Requête destinée à alimenter Closed positions - settlement date 
                       REQUESTTYPE in ('UNCLEARING','CLEARSPEC','CLEARBULK','CLEAREOD','ENTRY','UPDENTRY') qui sont les types présents dans "purchase and sales"
                     */
                    query = @"select pos.IDPA, pos.IDPADET, pos.REQUESTTYPE, pos.REQUESTMODE, pos.DTBUSINESS, pos.DTUNCLEARING, pos.QTY, pos.DTINS as DTSYS, 0 as DEACTIV, 
                    p.GPRODUCT,p.FAMILY,
                    tr.IDT as TRADE_IDT, tr.IDENTIFIER as TRADE_IDENTIFIER,
                    tr.IDASSET, tr.ASSETCATEGORY, tr.PRICE, tr.TRDTYPE, tr.TRDSUBTYPE, tr.DTOUTADJ,
                    tr2.IDT as TRADE2_IDT, tr2.IDENTIFIER as TRADE2_IDENTIFIER, 
                    tr2.IDASSET as IDASSET2, tr2.ASSETCATEGORY as ASSETCATEGORY2, tr2.PRICE as PRICE2, tr2.TRDTYPE as TRDTYPE2, tr2.TRDSUBTYPE as TRDSUBTYPE2, tr2.DTOUTADJ as DTOUTADJ2,
                    null as UNDERLYERPRICE
                    from dbo.VW_POSACTIONDET pos
                    inner join dbo.TRADE tr on (tr.IDT = pos.TRADE_IDT) 
                    inner join dbo.TRADE tr2 on (tr2.IDT = pos.TRADE2_IDT) 
                    inner join dbo.INSTRUMENT i on (i.IDI = tr.IDI)
                    inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.GPRODUCT='SEC')
                    {0}
                    {1} (pos.DTBUSINESS <= @DT) and (pos.REQUESTTYPE in ('UNCLEARING','CLEARSPEC','CLEARBULK','CLEAREOD','ENTRY','UPDENTRY'))
                    and (tr.DTSETTLT = @DT)";

                    // FI 20170524 [XXXXX] query est alimentée ici
                    query = StrFunc.AppendFormat(query,
                        (StrFunc.IsFilled(pAdditionalJoin) ? pAdditionalJoin : string.Empty),
                        (StrFunc.IsFilled(pAdditionalJoin) && pAdditionalJoin.StartsWith("where") ? "and" : "where"));

                    #endregion
                    break;
            }



            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }

        /// <summary>
        /// Alimentation des frais rattachés aux actions sur positions 
        /// <para>Alimentation de {posActions}.fee</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="posActions">actions sur position</param>
        /// FI 20140903 [20275] Modify
        /// FI 20150119 [XXXXX] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20150825 [21287] Modify (Modification de signature => Utilisation de  Liste de IPosactionTradeFee)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static void SetPosActionFee(string pCS, List<IPosactionTradeFee> posActions)
        {
            if (ArrFunc.IsFilled(posActions))
            {
                /*
                la requête les frais associés aux positions sur actions
                Chaque frais peut appraître de 1 à n fois s'il existe de 0 tax à  n taxes  
                 1 fois si pas de tax
                 1 fois si 1 tax
                 2 fois si 2 tax
                 n fois si n tax
                 */

                // FI 20140903 [20275] Utilisation de TRADEINSTRUMENT
                // FI 20150119 [XXXXX] Add column TAX_TYPE
                string query = @"select 
                epadet.IDPADET, e.IDE, e.IDT, 
                e.VALORISATION as OPPAMOUNT, e.UNIT as OPPUNIT , e.EVENTTYPE as OPPEVENTTYPE,
                efee.PAYMENTTYPE as OPPPAYMENTTYPE, 
                case when e.IDB_PAY = tr.IDB_DEALER then 'DR' else 'CR' end as OPPSIDE,                 

                taxdet.IDENTIFIER as TAX_IDENTIFIER, etaxfee.TAXRATE as TAX_RATE, etaxfee.TAXTYPE as TAX_TYPE,                         
                etax.VALORISATION as TAX_AMOUT, etax.UNIT as TAX_UNIT
   
                from dbo.EVENT e
                #RESTRICT_POSACTIONDET#
                inner join dbo.EVENTFEE efee on (efee.IDE = e.IDE) 
                inner join dbo.TRADE tr on tr.IDT = e.IDT

                left outer join dbo.EVENT etax on (etax.IDE_EVENT = e.IDE and etax.EVENTCODE = 'OPP' and etax.EVENTTYPE = 'TAX')
                left outer join dbo.EVENTFEE etaxfee on (etaxfee.IDE = etax.IDE) 
                left outer join dbo.TAXDET taxdet on  (taxdet.IDTAXDET = etaxfee.IDTAXDET)

                where (e.EVENTCODE = 'OPP') and (e.EVENTTYPE != 'TAX') and ((e.IDB_PAY = tr.IDB_DEALER) or (e.IDB_REC = tr.IDB_DEALER))

                order by epadet.IDPADET, e.IDE";

                // FI 20130625 peut-être faut-il eviter l'usage du in et charger toutes les actions à cette date
                // et faire la restriction sur le jeu de résultat avec LINQ ???
                string innerJoin = GetJoinEventPosActionDet(pCS, posActions);
                query = query.Replace("#RESTRICT_POSACTIONDET#", innerJoin);

                DataParameters dp = new DataParameters();
                // FI 20150623 [21149] Mise en commentaire               
                //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);

                QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (ArrFunc.IsFilled(dt.Rows))
                {
                    foreach (IPosactionTradeFee posAction in posActions)
                    {
                        DataRow[] row = dt.Select(StrFunc.AppendFormat("IDPADET = {0} and IDT = {1}", posAction.IdPosActionDet, posAction.IdT));
                        if (ArrFunc.IsFilled(row))
                        {
                            // FI 20150709 [XXXXX] Alimentation de eventType
                            var fee =
                            (from DataRow rowItem in row
                             select new
                             {
                                 idE = Convert.ToInt32(rowItem["IDE"]),
                                 amount = Convert.ToDecimal(rowItem["OPPAMOUNT"]),
                                 currency = Convert.ToString(rowItem["OPPUNIT"]),
                                 paymentType = Convert.ToString(rowItem["OPPPAYMENTTYPE"]),
                                 eventType = Convert.ToString(rowItem["OPPEVENTTYPE"]),
                                 side = Convert.ToString(rowItem["OPPSIDE"])
                             }
                             ).Distinct();

                            posAction.FeeSpecified = (fee.Count() > 0);
                            if (posAction.FeeSpecified)
                            {
                                posAction.Fee = new ReportFee[fee.Count()];
                                int i = 0;
                                foreach (var feeItem in fee)
                                {
                                    posAction.Fee[i] = new ReportFee
                                    {
                                        paymentTypeSpecified = StrFunc.IsFilled(feeItem.paymentType),
                                        eventTypeSpecified = true,
                                        eventType = feeItem.eventType,
                                        amount = feeItem.amount,
                                        currency = feeItem.currency,
                                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), feeItem.side)
                                    };
                                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                                    posAction.Fee[i].sideSpecified = (posAction.Fee[i].amount > 0);
                                    if (posAction.Fee[i].paymentTypeSpecified)
                                        posAction.Fee[i].paymentType = feeItem.paymentType;

                                    DataRow[] rowFee = dt.Select(StrFunc.AppendFormat("IDPADET = {0} and IDE = {1}",
                                        posAction.IdPosActionDet, feeItem.idE));

                                    var tax =
                                        (from DataRow rowFeeItem in rowFee
                                         select new
                                         {
                                             taxIdentier =
                                                rowFeeItem["TAX_IDENTIFIER"] == Convert.DBNull ?
                                                    "NOTAX" : Convert.ToString(rowFeeItem["TAX_IDENTIFIER"]),

                                             taxAmount =
                                                rowFeeItem["TAX_AMOUT"] == Convert.DBNull ?
                                                    0 : Convert.ToDecimal(rowFeeItem["TAX_AMOUT"]),
                                             taxUnit =
                                                rowFeeItem["TAX_UNIT"] == Convert.DBNull ?
                                                    "NOUNIT" : Convert.ToString(rowFeeItem["TAX_UNIT"]),

                                             taxRate =
                                                 rowFeeItem["TAX_RATE"] == Convert.DBNull ?
                                                    0 : Convert.ToDecimal(rowFeeItem["TAX_RATE"]),

                                             taxType =
                                              rowFeeItem["TAX_TYPE"] == Convert.DBNull ?
                                                 "NOTAXTYPE" : Convert.ToString(rowFeeItem["TAX_TYPE"])
                                         });

                                    posAction.Fee[i].taxSpecified = (tax.First().taxIdentier != "NOTAX");
                                    if (posAction.Fee[i].taxSpecified)
                                    {
                                        posAction.Fee[i].tax = new ReportTaxAmount[tax.Count()];

                                        int j = 0;
                                        foreach (var taxItem in tax)
                                        {
                                            posAction.Fee[i].tax[j] = new ReportTaxAmount
                                            {
                                                taxId = taxItem.taxIdentier,
                                                //FI 20150119 [XXXXX] Alimentation de taxType
                                                taxType = taxItem.taxType,
                                                rate = taxItem.taxRate,
                                                amount = taxItem.taxAmount
                                            };
                                            j++;
                                        }
                                    }

                                    i++;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne une restriction SQL de type Inner pour réduire les jeu de résultat des requêtes aux seuls évènements rattachés aux actions sur positions {posActions} 
        /// <para>L'alias de la table EVENT est obligatoitement e</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="posActions"></param>
        /// <returns></returns>
        /// FI 20150825 [21287] Modify (Modification de signature => Utilisation de  Liste de IPosactionTradeFee)
        private static string GetJoinEventPosActionDet(string pCS, List<IPosactionTradeFee> posActions)
        {
            string ret = SQLCst.X_INNER + "1=0";
            if (ArrFunc.IsFilled(posActions))
            {
                int[] idPosactionDet =
                            (from IPosactionTradeFee pos in posActions
                             select pos.IdPosActionDet).ToArray();

                SQLWhere sqlWhere = new SQLWhere();
                sqlWhere.Append("(" + DataHelper.SQLColumnIn(pCS, "padet.IDPADET", idPosactionDet, TypeData.TypeDataEnum.integer, false, true) + ")");
                // FI 20150623 [21149] pas besoin de rajouter cette restriction => on a déjà les idPosactionDet
                //sqlWhere.Append("(padet.DTBUSINESS = @DT or padet.DTUNCLEARING = @DT)");

                ret = "inner join dbo.EVENTPOSACTIONDET epadet on (epadet.IDE = e.IDE)" + Cst.CrLf;
                ret += SQLCst.X_INNER + "(select IDPADET from dbo.VW_POSACTIONDET padet " + Cst.CrLf + sqlWhere.ToString() + ") lst on (lst.IDPADET=epadet.IDPADET)";
            }
            return ret;
        }

        /// <summary>
        /// Retourne la requête de chargement des positions détaillés à la date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate">date d'observation</param>
        /// <param name="postionType">Type de position (Business ou Règlement)</param>
        /// <param name="pAdditionJoin">Représente une restriction SQL de manière à ne considérer qu'une liste de trade</param>
        /// <returns></returns>
        /// FI 20140820 [20275] Modify 
        /// FI 20150113 [20672] Modify
        /// FI 20150320 [XXPOC] Modify
        /// FI 20150407 [XXPOC] Modify
        /// FI 20150617 [21124] Modify
        /// FI 20150709 [XXXXX] Add
        /// FI 20151019 [21317] Modify
        /// FI 20170530 [XXXXX] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryPositionSynthetic(string pCS, DateTime pDate, PostionType postionType, string pAdditionJoin, CashBalanceProductEnv pProductEnv)
        {
            string query = string.Empty;

            switch (postionType)
            {
                case PostionType.Business:
                    #region requête en date business
                    // RD 20130717 [18833] Correction de la requête pour éviter le produit cartèsiens dans le cas d'existence de plusieurs événements LOV à des dates différentes
                    // RD 20131018 [18600] Chargement du montant UMG
                    // FI 20140820 [20275] Utilisation des colonnes TRADEINSTRUMENT.IDB_DEALER et TRADEINSTRUMENT.ASSETCATEGORY
                    // FI 20150113 [20672] Modifification de la requête (usage de la clé usuelle de Spheres®  IDA_DEALER,IDB_DEALER,IDA_CLEARER,IDB_CLEARER,IDI,IDASSET,ASSETCATEGORY ) 
                    // FI 20150320 [XXPOC] Lecture du QUOTEPRICE à la place du QUOTEPRICE100
                    // FI 20150407 [XXPOC] Restriction aux seuls instruments Fongibles
                    // FI 20150617 [21124] Utilisation de l'EVENTCLASS 'VAL'
                    // FI 20151019 [21317] Mise en place de 2 requêtes 

                    /* Query1 
                     * =>  query pour les futures, les Cfds et les FX  
                     * il existe nécessairement un évènement UMG lorsque le trade est en position 
                     * => Utilisation de l'évènement UMG pour optimiser les perfs (umgdet.DAILYQUANTITY donne la qté en position)
                     */
                    string query1 = StrFunc.AppendFormat(@"select posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, 
                    posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY,
                    posdet.SIDE,
                    sum(posdet.DAILYQUANTITY) as TOTALDAILYQTY, sum(posdet.PRICE * posdet.DAILYQUANTITY) as TOTALPRICE, 
                    posdet.QUOTEPRICE as QUOTEPRICE, posdet.QUOTEDELTA, 
                    posdet.LOVCURRENCY,
                    sum(case when posdet.LOV_IDB_PAY = posdet.IDB_DEALER then -1 * posdet.LOVAMOUNT 
                    else case when posdet.LOV_IDB_REC = posdet.IDB_DEALER then posdet.LOVAMOUNT 
                    else 0 end end) as LOVAMOUNT,
                    posdet.UMGCURRENCY,
                    sum(case when posdet.UMG_IDB_PAY = posdet.IDB_DEALER then -1 * posdet.UMGAMOUNT 
                    else case when posdet.UMG_IDB_REC = posdet.IDB_DEALER then posdet.UMGAMOUNT 
                    else 0 end end) as UMGAMOUNT
                    from (
                        select  t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER,
                        t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE, t.PRICE,
                        umgdet.DAILYQUANTITY, umgdet.QUOTEPRICE, umgdet.QUOTEDELTA,
                        umg.UNIT as UMGCURRENCY, umg.VALORISATION as UMGAMOUNT, umg.IDB_PAY as UMG_IDB_PAY, umg.IDB_REC as UMG_IDB_REC,
                        lov.UNIT as LOVCURRENCY, lov.VALORISATION as LOVAMOUNT, lov.IDB_PAY as LOV_IDB_PAY, lov.IDB_REC as LOV_IDB_REC,
                        t.IDENTIFIER as TRADEIDENTIFIER
                        from dbo.TRADE t
                        inner join dbo.INSTRUMENT i on i.IDI = t.IDI and i.FUNGIBILITYMODE in ('{1}','{2}') 
                        inner join dbo.PRODUCT p on p.IDP = i.IDP and p.GPRODUCT in ('FX','FUT','OTC') 
                        inner join dbo.EVENT umg on umg.IDT = t.IDT and umg.EVENTCODE='LPC' and umg.EVENTTYPE='UMG'
                        inner join dbo.EVENTCLASS umgval on umgval.IDE = umg.IDE and umgval.EVENTCLASS = 'VAL' and umgval.DTEVENT = @DT
                        inner join dbo.EVENTDET umgdet on umgdet.IDE = umg.IDE
                        left outer join (
                            select lov.IDT, lov.UNIT, lov.VALORISATION, lov.IDB_PAY, lov.IDB_REC
                            from dbo.EVENT lov        
                            inner join dbo.EVENTCLASS lovval on (lovval.IDE = lov.IDE) and (lovval.EVENTCLASS = 'VAL') and (lovval.DTEVENT = @DT)
                            where (lov.EVENTCODE='LPC' and lov.EVENTTYPE='LOV')
                        ) lov on (lov.IDT = t.IDT)
                        {0}
                    ) posdet
                    group by    posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY, 
                                posdet.QUOTEPRICE, posdet.QUOTEDELTA, posdet.SIDE, posdet.LOVCURRENCY, posdet.UMGCURRENCY", pAdditionJoin, FungibilityModeEnum.OPENCLOSE, FungibilityModeEnum.CLOSE);



                    /* Query2 
                    * =>  query pour les A/V de titres
                    * il existe nécessairement un évènement MKV lorsque le trade est en position 
                    * => Utilisation de cet évènement MKV pour optimiser les perfs (mkvdet.DAILYQUANTITY donne la qté en position)
                    */
                    string query2 = StrFunc.AppendFormat(@"select posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, 
                    posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY,
                    posdet.SIDE,
                    sum(posdet.DAILYQUANTITY) as TOTALDAILYQTY, sum(posdet.PRICE * posdet.DAILYQUANTITY) as TOTALPRICE, 
                    posdet.QUOTEPRICE as QUOTEPRICE, null as QUOTEDELTA, 
                    null as LOVCURRENCY,
                    0 as LOVAMOUNT,
                    posdet.UMGCURRENCY,
                    sum(case when posdet.UMG_IDB_PAY = posdet.IDB_DEALER then -1 * posdet.UMGAMOUNT 
                    else case when posdet.UMG_IDB_REC = posdet.IDB_DEALER then posdet.UMGAMOUNT 
                    else 0 end end) as UMGAMOUNT
                    from (
                        select t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER,
                        t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE, t.PRICE,
                        mkvdet.DAILYQUANTITY, mkvdet.QUOTEPRICE,
                        umg.UNIT as UMGCURRENCY, umg.VALORISATION as UMGAMOUNT, umg.IDB_PAY as UMG_IDB_PAY, umg.IDB_REC as UMG_IDB_REC,
                        t.IDENTIFIER as TRADEIDENTIFIER
                        from dbo.TRADE t
                        inner join dbo.INSTRUMENT i on i.IDI = t.IDI and i.FUNGIBILITYMODE in ('{1}','{2}') 
                        inner join dbo.PRODUCT p on p.IDP = i.IDP and p.GPRODUCT in ('SEC') 
                        inner join dbo.EVENT mkv on mkv.IDT = t.IDT and mkv.EVENTCODE='LPC' and mkv.EVENTTYPE='MKV'
                        inner join dbo.EVENTCLASS mkvval on mkvval.IDE = mkv.IDE and mkvval.EVENTCLASS = 'VAL' and mkvval.DTEVENT = @DT
                        inner join dbo.EVENTDET mkvdet on mkvdet.IDE = mkv.IDE
                        left outer join (
                            select umg.IDT, umg.UNIT, umg.VALORISATION, umg.IDB_PAY, umg.IDB_REC
                            from dbo.EVENT umg
                            inner join dbo.EVENTCLASS umgval on (umgval.IDE = umg.IDE) and (umgval.EVENTCLASS = 'VAL') and (umgval.DTEVENT = @DT)
                            where (umg.EVENTCODE='LPC' and umg.EVENTTYPE='UMG')
                        ) umg on (umg.IDT = t.IDT)
                        {0}
                    ) posdet
                    group by    posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY, 
                                posdet.QUOTEPRICE, posdet.SIDE, posdet.UMGCURRENCY", pAdditionJoin, FungibilityModeEnum.OPENCLOSE, FungibilityModeEnum.CLOSE);
                    #endregion

                    Boolean isQuery1 = true;
                    if (null != pProductEnv)
                        isQuery1 = pProductEnv.ExistFUT() || pProductEnv.ExistFX() || pProductEnv.ExistOTC();

                    Boolean isQuery2 = true;
                    if (null != pProductEnv)
                        isQuery2 = pProductEnv.ExistSEC();

                    StrBuilder strQuery = new StrBuilder();
                    if (isQuery1)
                        strQuery += query1;

                    if (isQuery2)
                    {
                        if (strQuery.Length > 0)
                            strQuery += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                        strQuery += query2;
                    }

                    query = strQuery.ToString();

                    break;

                case PostionType.Settlement:
                    #region requête en date Settlement

                    // FI 20150716 [20892] Modify seuls les trades ti.DTSETTLT <= @DT sont en position (avant il y avait tr.DTBUSINESS <= @DT )
                    // FI 20151019 [21317] Tous les montants sont à null => aucun montant retourné par la requête car ces montants sont fonction de la position en date business
                    // Les montants sont donc disponibles uniquement sur l'élément possynt (positionn synthétic business) 
                    // FI 20170530 [XXXXX] Utilisation de VW_TRADE_POSSEC puisque VW_TRADE_POSOTC ne contient plus les GPRODUCT = 'SEC' en v6.0
                    query = StrFunc.AppendFormat(@"select t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER, t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE,
                    sum(t.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0)) as TOTALDAILYQTY, 
                    sum(t.PRICE * (t.QTY - isnull(pos.QTY_BUY, 0) - isnull(pos.QTY_SELL, 0))) as TOTALPRICE, 
                    null as QUOTEPRICE, null, null as QUOTEDELTA,
                    null as LOVCURRENCY, null as LOVAMOUNT,
                    null as UMGCURRENCY, null as UMGAMOUNT
                    from dbo.VW_TRADE_POSSEC t
                    {0}
                    left outer join 
                    (

                        select pad.IDT_BUY as IDT,
                        sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1 then isnull(pad.QTY,0) else 0 end) as QTY_BUY, 0 as QTY_SELL
	                    from dbo.TRADE alloc 
	                    inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                        left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_SELL) 
                        where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                        group by pad.IDT_BUY
	                  
                        union all
	                  
                        select pad.IDT_SELL as IDT,
	                    0 as QTY_BUY, sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1  then isnull(pad.QTY,0) else 0 end) as QTY_SELL
	                    from dbo.TRADE alloc 
	                    inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                        left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_BUY) 
                        where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                        group by pad.IDT_SELL

                    ) pos on (pos.IDT = t.IDT)
                    where ((t.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0)) > 0) and (t.DTSETTLT <= @DT) and (@DT <= t.DTOUTADJ)
                    group by t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER, t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE", pAdditionJoin);

                    #endregion
                    break;
            }

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate); // FI 20201006 [XXXXX] DbType.Date
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="EfsMLversion"></param>
        /// <param name="lovAmount"></param>
        /// <param name="lovCurrency"></param>
        /// <param name="posSynthetic"></param>
        private static void PosSyntheticLov(EfsMLDocumentVersionEnum EfsMLversion, decimal lovAmount, string lovCurrency,
            PosSynthetic posSynthetic)
        {
            if (EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
            {
                posSynthetic.lovAmountSpecified = false;
                posSynthetic.lovCurrencySpecified = false;
                posSynthetic.lovAmount = decimal.Zero;
                posSynthetic.lovCurrency = null;

                posSynthetic.lovSpecified = StrFunc.IsFilled(lovCurrency); ;
                if (posSynthetic.lovSpecified)
                {
                    posSynthetic.lov = new ReportAmountSide
                    {
                        amount = System.Math.Abs(lovAmount),
                        currency = lovCurrency
                    };
                    if (lovAmount >= 0)
                        posSynthetic.lov.side = CrDrEnum.CR;
                    else
                        posSynthetic.lov.side = CrDrEnum.DR;
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    posSynthetic.lov.sideSpecified = (posSynthetic.lov.amount > 0);
                }
            }
            else
            {
                posSynthetic.lovSpecified = false;

                posSynthetic.lovCurrencySpecified = StrFunc.IsFilled(lovCurrency); ;
                posSynthetic.lovAmountSpecified = posSynthetic.lovCurrencySpecified;
                if (posSynthetic.lovCurrencySpecified)
                {
                    posSynthetic.lovAmount = lovAmount;
                    posSynthetic.lovCurrency = lovCurrency;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="EfsMLversion"></param>
        /// <param name="umgAmount"></param>
        /// <param name="umgCurrency"></param>
        /// <param name="posSynthetic"></param>
        private static void PosSyntheticUmg(EfsMLDocumentVersionEnum EfsMLversion, decimal umgAmount, string umgCurrency,
            PosSynthetic posSynthetic)
        {
            if (EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
            {
                posSynthetic.umgSpecified = StrFunc.IsFilled(umgCurrency); ;
                if (posSynthetic.umgSpecified)
                {
                    posSynthetic.umg = new ReportAmountSide
                    {
                        amount = System.Math.Abs(umgAmount),
                        currency = umgCurrency
                    };
                    if (umgAmount >= 0)
                        posSynthetic.umg.side = CrDrEnum.CR;
                    else
                        posSynthetic.umg.side = CrDrEnum.DR;
                    //FI 20141208 [XXXXX] alimentation de sideSpecified
                    posSynthetic.umg.sideSpecified = (posSynthetic.umg.amount > 0);

                }
            }
        }



        /// <summary>
        /// Définit la version du document
        /// <para>
        /// La version 3.5 existe depuis la mise en place du report NotificationTypeEnum.SYNTHESIS
        /// </para>
        /// <para>
        /// La version 3.5 devra être utilisée à terme sur toutes les éditions parce que le flux généré est plus légé    
        /// </para>
        /// </summary>
        private void SetNotificationVersion(CnfMessage pCnfMessage)
        {
            if (NotificationDocument.EfsMLversion == EfsMLDocumentVersionEnum.Version30)
            {
                //possibilité de migrer les états en version >=3.5
                string version = SystemSettings.GetAppSettings("Notification_EfsMLversion");
                if (StrFunc.IsFilled(version))
                    NotificationDocument.EfsMLversion = (EfsMLDocumentVersionEnum)System.Enum.Parse(typeof(EfsMLDocumentVersionEnum), version);

                // Certains messages sont obligatoirement en version 3.5
                if (pCnfMessage.NotificationType.HasValue)
                {
                    switch (pCnfMessage.NotificationType.Value)
                    {
                        case NotificationTypeEnum.SYNTHESIS:
                        case NotificationTypeEnum.ORDERALLOC: // FI 20190515 [23912] 
                            NotificationDocument.EfsMLversion = EfsMLDocumentVersionEnum.Version35;
                            break;
                    }
                }
            }
        }

        /// <summary>
        ///  Ajoute le trade dans NotificationDocument.commonData.trade
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCS"></param>
        /// <param name="pTrade">Représente un trade jour (de type Trade) ou un trade en position (de type PosTrade) ou un trade impliqué dans une action ( de type PosActionTrade)</param>
        /// FI 20150218 [20275] Add method
        /// FI 20170217 [22862] Modify
        private void AddCommonDataTrade<T>(string pCS, T pTrade)
        {
            if (NotificationDocument.CommonDataSpecified == false)
            {
                NotificationDocument.CommonData = new CommonData();
                NotificationDocument.CommonDataSpecified = true;
            }

            int idT;
            string identifier;
            Nullable<decimal> lastPrice = null;
            Pair<Cst.UnderlyingAsset, int> asset = null;
            Nullable<DateTime> dtOut = null;

            // FI 20170217 [22862] test is PosTradeCommon
            // if (pTrade is TradeReport || pTrade is PosTrade || pTrade is DlvTrade)
            if (pTrade is PosTradeCommon)
            {
                PosTradeCommon trade = pTrade as PosTradeCommon;
                idT = trade.OTCmlId;
                identifier = trade.tradeIdentifier;

                if (trade.assetCategorySpecified)
                    asset = new Pair<Cst.UnderlyingAsset, int>(trade.assetCategory, trade.idAsset);

                if (trade.priceSpecified)
                    lastPrice = trade.price;

                if (trade.dtOutSpecified)
                    dtOut = trade.dtOut;
            }
            else if (pTrade is PosActionTrade)
            {
                PosActionTrade trade = pTrade as PosActionTrade;
                idT = trade.OTCmlId;
                identifier = trade.tradeIdentifier;

                asset = new Pair<Cst.UnderlyingAsset, int>(trade.assetCategory, trade.idAsset);
                lastPrice = trade.price;

                dtOut = trade.dtOut;
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("Object:{0} is not implemented!", pTrade.ToString()));

            CommonTradeAlloc find = NotificationDocument.CommonData.trade.Find(item => item.OTCmlId == idT);
            if (null == find)
            {
                CommonTradeAlloc trade = new CommonTradeAlloc
                {
                    OTCmlId = idT,
                    tradeIdentifier = identifier,
                    expDtSpecified = (null != dtOut)
                };

                if ((null != asset) && lastPrice.HasValue)
                {
                    trade.lastPriceSpecified = true;
                    trade.lastPrice = lastPrice.Value;

                    trade.fmtLastPriceSpecified = true;
                    trade.fmtLastPrice = BuildFmtPrice(pCS, asset, lastPrice.Value, null);
                }

                if (trade.expDtSpecified)
                    trade.expDt = DtFunc.DateTimeToStringDateISO(dtOut.Value);

                NotificationDocument.CommonData.trade.Add(trade);
            }
        }

        /// <summary>
        ///  Retourne la requête qui charge toutes les positions détaillées à la date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="postionType"></param>
        /// <param name="pAdditionJoin"></param>
        /// <param name="pProductEnv"></param>
        /// <returns></returns>
        /// FI 20151019 [21317] Add 
        /// FI 20161214 [21916] Add
        /// FI 20170530 [XXXXX] Modify 
        // EG 20190730 New column read ASSETMEASURE, RATE (AINRATE) in EVENTDET table
        // RD 20191112 [25062] Modify
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryTradePosition2(string pCS, DateTime pDate, PostionType postionType, string pAdditionJoin, CashBalanceProductEnv pProductEnv)
        {
            string query = string.Empty;

            switch (postionType)
            {
                case PostionType.Business:
                    #region Requête des trades en positions en business date
                    // RD 20130722 [18745] Ajout du cours de clôture
                    // RD 20140626 [20152] Ajout du montant de la prim (PRM et HPR)
                    // FI 20140820 [20275] Utilisation des colonnes TRADEINSTRUMENT.IDB_DEALER et TRADEINSTRUMENT.ASSETCATEGORY
                    // FI 20140820 [20275] TODO pourquoi sous select sur lov ? je fais la même chose pour mkv, et mgr
                    // FI 20140901 [20275] alias mgf ajout des critères date sur la jointure  
                    // FI 20150320 [XXPOC] Lecture du QUOTEPRICE à la place du QUOTEPRICE100
                    // FI 20150522 [20275] Add DTOUTADJ
                    // FI 20150617 [21124] Utilisation de l'EVENTCLASS 'VAL'
                    // FI 20151019 [21317] Add FAMILY
                    // FI 20151019 [21317] Usage de 2 requêtes
                    // => Permet l'amélioration des perfs pour un client qui ne serait que mono Activité (Ex SIGMA)
                    // FI 20161214 [21916] Add QTYUNIT 
                    // RD 20191112 [25062] Add column ASSETMEASURE, RATE (AINRATE) query1

                    /* Query1 
                     * =>  query pour les futures, les Cfds et les FX  
                     * il existe nécessairement un évènement UMG lorsque le trade est en position 
                     * => Utilisation de l'évènement UMG pour optimiser les perfs (umgdet.DAILYQUANTITY donne la qté en position)
                     */
                    string query1 = @"select t.IDT, t.IDENTIFIER, 
                    t.IDB_DEALER as IDB, case t.SIDE when '1' then 'Buy' else 'Sell' end as BUYSELL, t.IDI, t.ASSETCATEGORY, t.IDASSET, t.PRICE, t.DTOUTADJ,
                    p.GPRODUCT, p.FAMILY, i.FUNGIBILITYMODE,            
                    umgdet.DAILYQUANTITY as QTY, null as QTYUNIT, 
                    null as ASSETMEASURE,
                    umgdet.QUOTEPRICE as QUOTEPRICE,
                    umg.UNIT as UMGUNIT, umg.VALORISATION as UMGAMOUNT, 
                    case when umg.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as UMGSIDE,
                    lov.UNIT as LOVUNIT, lov.VALORISATION as LOVAMOUNT, 
                    case when lov.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as LOVSIDE,
                    prm.UNIT as PRMUNIT, prm.VALORISATION as PRMAMOUNT, 
                    case when prm.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as PRMSIDE,
                    null as MKVAMOUNT, null as MKVUNIT, null as MKVSIDE,
                    null as PAMAMOUNT, null as PAMUNIT, null as PAMSIDE,
                    null as AINAMOUNT, null as AINUNIT, null as AINSIDE, null as AINDAYS, null as AINRATE,
                    mgr.VALORISATION as MGRAMOUNT, mgr.UNIT as MGRUNIT, 
                    case when mgr.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as MGRSIDE,
                    mgf.VALORISATION as MGRFACTOR 
                    from dbo.TRADE t
                    inner join dbo.INSTRUMENT i on i.IDI = t.IDI 
                    inner join dbo.PRODUCT p on p.IDP = i.IDP and p.GPRODUCT in ('FX','FUT','OTC') 
                    inner join dbo.EVENT umg on umg.IDT = t.IDT and umg.EVENTCODE='LPC' and umg.EVENTTYPE='UMG'
                    inner join dbo.EVENTCLASS umgval on umgval.IDE = umg.IDE and umgval.EVENTCLASS = 'VAL' and umgval.DTEVENT = @DT
                    inner join dbo.EVENTDET umgdet on umgdet.IDE = umg.IDE

                    left outer join (
                        select e.IDT, e.UNIT, e.VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DT)
                        where (e.EVENTCODE='LPC') and (e.EVENTTYPE='LOV')
                    ) lov on (lov.IDT = t.IDT)

                    left outer join (
                        select e.IDT, e.UNIT, sum(e.VALORISATION) as VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        where (e.EVENTCODE='LPP') and (e.EVENTTYPE in ('PRM','HPR')) 
                        group by e.IDT, e.UNIT, e.IDB_PAY
                    ) prm on (prm.IDT = t.IDT)

                    left outer join (
                        select e.IDT, e.UNIT, e.VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DT)
                        where (e.EVENTCODE='LPC') and (e.EVENTTYPE='MGR')
                    ) mgr on (mgr.IDT = t.IDT)

                    left outer join dbo.EVENT mgf on mgf.IDT = t.IDT and mgf.EVENTCODE='LPP' and mgf.EVENTTYPE='MGF' and mgf.DTSTARTUNADJ<=@DT and mgf.DTENDUNADJ>@DT
                    {0}";

                    /* Query2 
                    * =>  query pour les A/V de titres
                    * il existe nécessairement un évènement MKV lorsque le trade est en position 
                    * => Utilisation de cet évènement MKV pour optimiser les perfs (mkvdet.DAILYQUANTITY donne la qté en position)
                    * => Lecture du LPC/PAM et LPC/AIN
                    */
                    string query2 = @"select t.IDT, t.IDENTIFIER, 
                    t.IDB_DEALER as IDB, case t.SIDE when '1' then 'Buy' else 'Sell' end as BUYSELL, t.IDI, t.ASSETCATEGORY, t.IDASSET, t.PRICE, t.DTOUTADJ,
                    p.GPRODUCT, p.FAMILY, i.FUNGIBILITYMODE,            
                    mkvdet.DAILYQUANTITY as QTY, null as QTYUNIT, 
                    case when mkvdet.QUOTEPRICE is null then umgdet.ASSETMEASURE else mkvdet.ASSETMEASURE end as ASSETMEASURE,
                    isnull(mkvdet.QUOTEPRICE, umgdet.QUOTEPRICE)as QUOTEPRICE,
                    umg.UNIT as UMGUNIT, umg.VALORISATION as UMGAMOUNT, 
                    case when umg.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as UMGSIDE,
                    null as LOVUNIT, null as LOVAMOUNT, null as LOVSIDE,
                    null as PRMUNIT, null as PRMAMOUNT, null as PRMSIDE,
                    mkv.VALORISATION as MKVAMOUNT, mkv.UNIT as MKVUNIT,
                    case when mkv.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as MKVSIDE,
                    pam.VALORISATION as PAMAMOUNT, pam.UNIT as PAMUNIT,
                    case when pam.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as PAMSIDE,
                    ain.VALORISATION as AINAMOUNT, ain.UNIT as AINUNIT,
                    case when ain.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end  as AINSIDE, aindet.TOTALOFDAY  as AINDAYS, aindet.RATE as AINRATE,
                    null as MGRAMOUNT, null as MGRUNIT, null as MGRSIDE, null as MGRFACTOR 
                    from dbo.TRADE t
                    inner join dbo.INSTRUMENT i on i.IDI = t.IDI 
                    inner join dbo.PRODUCT p on p.IDP = i.IDP and GPRODUCT = 'SEC'
                    inner join dbo.EVENT mkv on mkv.IDT = t.IDT and mkv.EVENTCODE='LPC' and mkv.EVENTTYPE='MKV'
                    inner join dbo.EVENTCLASS mkvval on mkvval.IDE = mkv.IDE and mkvval.EVENTCLASS = 'VAL' and mkvval.DTEVENT = @DT
                    inner join dbo.EVENTDET mkvdet on mkvdet.IDE = mkv.IDE

                    left outer join (
                        select e.IDE, e.IDT, e.UNIT, e.VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DT)
                        where (e.EVENTCODE='LPC') and (e.EVENTTYPE='UMG')
                    ) umg on (umg.IDT = t.IDT)
                    left outer join dbo.EVENTDET umgdet on umgdet.IDE = umg.IDE

                    left outer join (
                        select e.IDE, e.IDT, e.UNIT, e.VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DT)
                        where (e.EVENTCODE='LPC') and (e.EVENTTYPE='MKP')
                    ) pam on (pam.IDT = t.IDT)
                    left outer join dbo.EVENTDET pamdet on pamdet.IDE = pam.IDE

                    left outer join (
                        select e.IDE, e.IDT, e.UNIT, e.VALORISATION, e.IDB_PAY
                        from dbo.EVENT e
                        inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.EVENTCLASS = 'VAL') and (ec.DTEVENT = @DT)
                        where (e.EVENTCODE='LPC') and (e.EVENTTYPE='MKA')
                    ) ain on (ain.IDT = t.IDT)
                    left outer join dbo.EVENTDET aindet on aindet.IDE = ain.IDE

                    {0}";
                    Boolean isQuery1 = true;
                    if (null != pProductEnv)
                        isQuery1 = pProductEnv.ExistFUT() || pProductEnv.ExistFX() || pProductEnv.ExistOTC();

                    Boolean isQuery2 = true;
                    if (null != pProductEnv)
                        isQuery2 = pProductEnv.ExistSEC();

                    StrBuilder strQuery = new StrBuilder();
                    if (isQuery1)
                        strQuery += query1;

                    if (isQuery2)
                    {
                        if (strQuery.Length > 0)
                            strQuery += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                        strQuery += query2;
                    }

                    query = strQuery.ToString();


                    #endregion
                    break;
                case PostionType.Settlement:
                    #region Requête des trades en positions en settlement date
                    // FI 20150716 [20892] Modify seuls les trades ti.DTSETTLT <= @DT sont en position (avant il y avait tr.DTBUSINESS <= @DT )
                    // FI 20151019 [21317] Add FAMILY
                    // FI 20151019 [21317] Tous les montants sont à null => aucun montant retourné par la requête car ces montants sont fonction de la position en date business
                    // Les montants sont donc disponibles uniquement sur l'élément posdet (positionn détaillé business) 
                    // FI 20161214 [21916] Add UNITQTY 
                    // FI 20170530 [XXXXX] Utilisation de VW_TRADE_POSSEC puisque VW_TRADE_POSOTC ne contient plus les GPRODUCT = 'SEC' en v6.0
                    query = @"select t.IDT, t.IDENTIFIER, 
                    t.IDB_DEALER as IDB, case t.SIDE when '1' then 'Buy' else 'Sell' end as BUYSELL, t.IDI, t.ASSETCATEGORY, t.IDASSET, t.PRICE, t.DTOUTADJ,
                    t.GPRODUCT, t.FAMILY, t.FUNGIBILITYMODE,            
                    t.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0) as QTY, null as UNITQTY, 
                    null as QUOTEPRICE,
                    null as UMGUNIT, null as UMGAMOUNT, null as UMGSIDE,
                    null as LOVUNIT, null as LOVAMOUNT, null as LOVSIDE,
                    null as PRMUNIT, null as PRMAMOUNT, null as PRMSIDE,
                    null as MKVAMOUNT, null as MKVUNIT, null as MKVSIDE,
                    null as MGRAMOUNT, null as MGRUNIT, null as MGRSIDE,
                    null as PAMAMOUNT, null as PAMUNIT, null as PAMSIDE,
                    null as AINAMOUNT, null as AINUNIT, null as AINSIDE,
                    null as MGRFACTOR 
                    from dbo.VW_TRADE_POSSEC t
                    {0}
                    left outer join 
                    (

                        select pad.IDT_BUY as IDT,
                        sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1 then isnull(pad.QTY,0) else 0 end) as QTY_BUY, 0 as QTY_SELL
	                    from dbo.TRADE alloc 
	                    inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                        left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_SELL) 
                        where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                        group by pad.IDT_BUY
	                  
                        union all
	                  
                        select pad.IDT_SELL as IDT,
	                    0 as QTY_BUY, sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1  then isnull(pad.QTY,0) else 0 end) as QTY_SELL
	                    from dbo.TRADE alloc 
	                    inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                        inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                        left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_BUY) 
                        where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                        group by pad.IDT_SELL

                    ) pos on (pos.IDT = t.IDT)
                    where ((t.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0)) > 0) and (t.DTSETTLT <= @DT) and (@DT <= t.DTOUTADJ)";
                    #endregion
                    break;
            }

            query = StrFunc.AppendFormat(query, StrFunc.IsFilled(pAdditionJoin) ? pAdditionJoin : string.Empty);

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }



        /// <summary>
        ///  Retourne la requête qui charge toutes les paiements TER, DVA de  à la date {pDate}(Paiement lié à livraison de ss-jacent commodity). 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pAdditionJoin"></param>
        /// <returns></returns>
        /// FI 20170217 [22862] Add
        /// FI 20170230 [22862] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryTradeDelivery(string pCS, DateTime pDate, string pAdditionJoin)
        {

            // FI 20170230 [22862] VAL à la place de STL
            string query = @"select t.IDT, t.IDENTIFIER, 
            t.IDB_DEALER as IDB, case t.SIDE when '1' then 'Buy' else 'Sell' end as BUYSELL,
            p.GPRODUCT, p.FAMILY, i.FUNGIBILITYMODE,            
            t.IDI, t.ASSETCATEGORY, t.IDASSET, t.PRICE, 
            t.DTOUTADJ,
            dvaParentdet.DAILYQUANTITY as QTY, null as QTYUNIT, 
            dvadet.SETTLTPRICE as SETTLTPRICE,
            dva.UNIT as DVAUNIT, dva.VALORISATION as DVAAMOUNT,case when dva.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as DVASIDE,
            dvadet.DAILYQUANTITY as PHYQTY, dvadet.UNITDAILYQUANTITY as PHYQTYUNIT
            from dbo.TRADE t
            inner join dbo.INSTRUMENT i on i.IDI = t.IDI 
            inner join dbo.PRODUCT p on p.IDP = i.IDP and p.GPRODUCT in ('FUT') 
            inner join dbo.VW_ASSET_ETD_EXPANDED assetetd on assetetd.IDASSET=t.IDASSET and t.ASSETCATEGORY='ExchangeTradedContract' and assetetd.CATEGORY='F'

            inner join dbo.EVENT dva on dva.IDT = t.IDT and dva.EVENTCODE='TER' and dva.EVENTTYPE='DVA'
            inner join dbo.EVENTCLASS dvaval on dvaval.IDE = dva.IDE and dvaval.EVENTCLASS = 'VAL' and dvaval.DTEVENT = @DT

            inner join dbo.EVENTDET dvadet on dvadet.IDE = dva.IDE
            inner join dbo.EVENT dvaParent on dvaParent.IDE = dva.IDE_EVENT 
            inner join dbo.EVENTDET dvaParentdet on dvaParentdet.IDE = dvaParent.IDE
            {0}";
            query = StrFunc.AppendFormat(query, StrFunc.IsFilled(pAdditionJoin) ? pAdditionJoin : string.Empty);

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }




        /// <summary>
        /// Retourne une restriction SQL de type inner pour que le jeu de résultat d'une requête se limite aux  trades {pIdT}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pColumnIdT"></param>
        /// <param name="pAddDtBusiness">true si édition et false sinon</param>
        /// <returns></returns>
        /// FI 20141218 [20574] Add parameter pColumnIdT
        /// EG 20150116 [20699] Add t2.IDENTIFIER 
        private static string GetJoinTrade(string pCS, int[] pIdT, string pColumnIdT, Boolean pAddDtBusiness)
        {
            string ret = SQLCst.X_INNER + "1=0";

            if (ArrFunc.IsFilled(pIdT))
            {
                SQLWhere sqlWhere = new SQLWhere();
                sqlWhere.Append(@"(" + DataHelper.SQLColumnIn(pCS, "t2.IDT", pIdT, TypeData.TypeDataEnum.integer, false, true) + ")");
                if (pAddDtBusiness)
                    sqlWhere.Append("t2.DTBUSINESS = @DT");

                // EG 20150116 [20699] Add t2.IDENTIFIER
                ret =
                    StrFunc.AppendFormat(
                    SQLCst.X_INNER + "(select t2.IDT, t2.IDENTIFIER from dbo.TRADE t2 " + Cst.CrLf + sqlWhere.ToString() + ") lst on (lst.IDT={0})", pColumnIdT);
            }
            return ret;
        }

        /// <summary>
        /// Alimentation de certains éléments du confirmationMessageDoc.
        /// <para>Alimentation en fonction du type/class du message</para>
        /// <para>Retourne la liste des trades impliqués pour alimentation de confirmationMessageDoc.dataDocument</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Liste des trades</param>
        /// <param name="pNotificationClass">classe de notification</param>
        /// <param name="pNotificationType">type de notification</param>
        /// <param name="pConfirmationChain"></param>
        /// <param name="pDate">Date début</param>
        /// <param name="pDate2">Dade fin (Etat de synthèse périodique)</param>
        /// FI 20140801 [20255] Modify
        /// FI 20150127 [20275] Modify
        /// FI 20150127 [20275] Modify
        /// FI 20151019 [21317] Modify
        /// FI 20160118 [21781] Modify
        /// RD 20160912 [22447] Add pDate2
        /// EG 20210322 [XXXXX] Add Recette Facturation (Test pNotificationType.hasValue)
        private List<int> SetSpecificElement(string pCS, int[] pIdT,
                                        NotificationClassEnum pNotificationClass, Nullable<NotificationTypeEnum> pNotificationType,
                                        ConfirmationChain pConfirmationChain, DateTime pDate, Nullable<DateTime> pDate2)
        {

            List<int> idT = new List<int>(pIdT);

            switch (pNotificationClass)
            {
                case NotificationClassEnum.MONOTRADE:
                    // FI 20140801 [20255] 
                    // Les informations de matching sont insérées uniquement sur les confirmations (NotificationClassEnum.MONOTRADE)
                    SetTradeCciMatch(pCS, pIdT);
                    if (pNotificationType.HasValue)
                    {
                        switch (pNotificationType.Value)
                        {
                            case NotificationTypeEnum.ORDERALLOC: // FI 20190515 [23912]
                                SetTradesOfOrder(pCS, idT[0], pDate);
                                AddAllTrades(idT, pDate);
                                List<TradeReport> tradesReport = GetAllTrades(pDate);
                                foreach (TradeReport item in tradesReport)
                                    AddCommonDataTrade(pCS, item);
                                break;
                            default:
                                break;
                        }
                    }
                    break;

                case NotificationClassEnum.MULTITRADES:
                case NotificationClassEnum.MULTIPARTIES:
                    switch (pNotificationType.Value)
                    {
                        case NotificationTypeEnum.ALLOCATION:
                            if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
                            {
                                SetTrades(pCS, pIdT, pDate);
                                // FI 20190515 [23912] Appel à AddCommonDataTrade
                                List<TradeReport> tradesReport = GetAllTrades(pDate);
                                foreach (TradeReport item in tradesReport)
                                    AddCommonDataTrade(pCS, item);
                            }
                            break;

                        case NotificationTypeEnum.POSACTION:
                            //FI 20120426 [17703]   
                            //Lorsque l'édition est de type POSACTION, Spheres® alimente l'éléments posActions avant tout
                            SetPosActions(pCS, pIdT, pNotificationClass, pConfirmationChain, pDate);

                            //Spheres® ajoute les trades liés à la liste des trades qui ont subis une action sur Position 
                            AddAllPosActionsTrades(idT, pDate);

                            // FI 20190515 [23912] Alimentation de commonData
                            List<PosAction> posactions = GetAllPosAction(pDate);
                            foreach (PosAction posAction in posactions)
                            {
                                AddCommonDataTrade(pCS, posAction.trades.trade);
                                if (posAction.trades.trade2Specified)
                                    AddCommonDataTrade(pCS, posAction.trades.trade2);
                            }
                            break;

                        case NotificationTypeEnum.POSITION:
                            if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
                            {
                                SetPosTrades(pCS, pIdT, pDate);
                                // FI 20190515 [23912] Alimentation de commonData
                                List<PosTrade> posAllTrades = GetAllPosTrades(pDate);
                                foreach (PosTrade item in posAllTrades)
                                    AddCommonDataTrade(pCS, item);
                            }
                            break;

                        case NotificationTypeEnum.POSSYNTHETIC:
                            SetPosSynthetics(pCS, pIdT, pDate);
                            break;

                        case NotificationTypeEnum.SYNTHESIS:
                            // FI 20151019 [21317] Add producctEnv (Optimisation du traitement)
                            productEnv = new CashBalanceProductEnv(pCS, new List<int>(pIdT));

                            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                            //if (ArrFunc.Count(pIdT) > 1)
                            //    SetSynthesisOfPeriod(pCS, pIdT.ToArray(), pIdT);
                            //else if (ArrFunc.Count(pIdT) == 1)
                            //    SetSynthesisOfDay(pCS, pIdT[0], pDate, pIdT);
                            //else
                            //    throw new NotSupportedException("pIdT is empty");
                            if (ArrFunc.Count(pIdT) == 0)
                                throw new NotSupportedException("pIdT is empty");

                            switch (pNotificationClass)
                            {
                                case NotificationClassEnum.MULTITRADES:
                                    // FI 20150427 [20987] call SetSynthesisOfPeriod or SetSynthesisOfDay
                                    //if (ArrFunc.Count(pIdT) > 1)
                                    // FI 20180925 [24164] => test sur pDate2. 
                                    // Lorsqu'il y a édition périodique (1 month, 1 year,..) pDate2 est renseigné
                                    if (pDate2.HasValue)
                                        SetSynthesisOfPeriod(pCS, pIdT, idT);
                                    else if (ArrFunc.Count(pIdT) == 1)
                                        SetSynthesisOfDay2(pCS, pIdT[0], pDate, idT);
                                    break;

                                case NotificationClassEnum.MULTIPARTIES:
                                    if (pDate2.HasValue && (pDate2.Equals(pDate) == false))
                                        throw new NotImplementedException(StrFunc.AppendFormat("Periodic {0} {1} message is not managed, please contact EFS",
                                            pNotificationClass.ToString(), pNotificationType.Value.ToString()));

                                    int[] trade = (from item in pIdT orderby item select item).ToArray();

                                    for (int i = 0; i < ArrFunc.Count(trade); i++)
                                        SetSynthesisOfDay2(pCS, trade[i], pDate, idT);
                                    break;
                            }
                            break;
                    }
                    break;
            }

            // FI 20160118 [21781] call SetAveragePrice
            UpdFmtAveragePrice();

            // FI 20190515 [23912] Add Return
            return idT;
        }

        /// <summary>
        /// Mise à jour du nombre de decimale des moyennes 
        /// <para>Concerne les éléments trades, unsettledTrades, settledTrades, posTrades </para>
        /// </summary>
        /// FI 20160118 [21781] Add Method
        /// FI 20160126 [XXXXX] suppression du try cath (avait été mis par précaution avant sortie de la 5.1 pour VCL)
        private void UpdFmtAveragePrice()
        {
            if (NotificationDocument.TradesSpecified)
            {
                foreach (TradesReport item in NotificationDocument.Trades)
                    FmtAvg(NotificationDocument.CommonData, item.trade.Cast<PosTradeCommon>().ToList(), item.subTotal);
            }
            if (NotificationDocument.UnsettledTradesSpecified)
            {
                foreach (TradesReport item in NotificationDocument.UnsettledTrades)
                    FmtAvg(NotificationDocument.CommonData, item.trade.Cast<PosTradeCommon>().ToList(), item.subTotal);
            }
            if (NotificationDocument.SettledTradesSpecified)
            {
                foreach (TradesReport item in NotificationDocument.SettledTrades)
                    FmtAvg(NotificationDocument.CommonData, item.trade.Cast<PosTradeCommon>().ToList(), item.subTotal);
            }

            if (NotificationDocument.PosTradesSpecified)
            {
                foreach (PosTrades item in NotificationDocument.PosTrades)
                    FmtAvg(NotificationDocument.CommonData, item.trade.Cast<PosTradeCommon>().ToList(), item.subTotal);
            }
            // FI 20191204 [XXXXX] Add Call of FmtAvg method for each NotificationDocument.stlPosTrades
            if (NotificationDocument.StlPosTradesSpecified)
            {
                foreach (PosTrades item in NotificationDocument.StlPosTrades)
                    FmtAvg(NotificationDocument.CommonData, item.trade.Cast<PosTradeCommon>().ToList(), item.subTotal);
            }
        }

        /// <summary>
        ///  Mise en place d'un état de synthèse sur une journée (usage de tâches Asynchrone)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdT"></param>
        /// FI 20200519 [XXXXX] Add Method
        private void SetSynthesisOfDay2(string pCS, int pIdTCashBalance, DateTime pDate, List<int> pIdT)
        {
            // FI 20200519 [XXXXX] Alimentation si pIsAddCommonDataTrade
            List<Task> lstTask = new List<Task>
            {

                //Alimentation des trades du jour, des trades en attente de règlement, des trades réglés  
                Task.Run(() => SetTradesReportSynthesis(pCS, pIdTCashBalance, pDate, false)).ContinueWith(r =>SetAinDaysInDebtSecurityTransaction(pCS)),
                //Alimentation des actions sur positions du jour
                Task.Run(() => SetPosActionsReportSynthesis(pCS, pIdTCashBalance, pDate, false)),
                //Alimentation des positions du jour
                Task.Run(() => SetPosTradesReportSynthesis(pCS, pIdTCashBalance, pDate, false)),
                //Alimentation des position synthétiques
                Task.Run(() => SetPosSyntheticsReportSynthesis(pCS, pIdTCashBalance, pDate)).ContinueWith(r =>
                    SetPositionSyntheticFundingAmountReportSynthesis(pCS, pIdTCashBalance, pDate)).ContinueWith(r2 =>
                    SetPositionSyntheticSafekeepingReportSynthesis(pCS, pIdTCashBalance, pDate)),
                Task.Run(() => SetDvlTradesReportSynthesis(pCS, pIdTCashBalance, pDate, false)),
                //Alimentation des versements/retraits
                Task.Run(() => SetCashPaymentReportSynthesis(pCS, pIdTCashBalance, pDate)),
                Task.Run(() => SetUnderlyingStockSynthesis(pCS, pIdTCashBalance, pDate))
            };

            try
            {
                Task.WaitAll(lstTask.ToArray());
            }
            catch (AggregateException ae)
            {
                throw ae.Flatten();
            }

            AddAllTrades(pIdT, pDate);
            AddAllPosActionsTrades(pIdT, pDate);
            AddAllPosTrades(pIdT, pDate);
            AddDvlTrades(pIdT, pDate);

            IEnumerable<PosTradeCommon> tradesReport = GetAllTrades(pDate).Cast<PosTradeCommon>().Union(
                                                            GetAllPosTrades(pDate).Cast<PosTradeCommon>()).Union(
                                                                GetDeliveryTrade(pDate).Cast<PosTradeCommon>());
            foreach (PosTradeCommon item in tradesReport)
                AddCommonDataTrade(pCS, item);

            List<PosAction> posactions = GetAllPosAction(pDate);
            foreach (PosAction posAction in posactions)
            {
                AddCommonDataTrade(pCS, posAction.trades.trade);
                if (posAction.trades.trade2Specified)
                    AddCommonDataTrade(pCS, posAction.trades.trade2);
            }

            CopyUMGInUnsettledTrades(pDate);
        }

        /// <summary>
        /// Alimente unsettledTrades.umg avec posTrades.umg
        /// <para>Facilite la réalisation de l'XSL</para>
        /// </summary>
        /// <param name="pDate"></param>
        /// FI 20150716 [20892] Add Method
        private void CopyUMGInUnsettledTrades(DateTime pDate)
        {
            if (NotificationDocument.UnsettledTradesSpecified)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate);
                List<PosTrade> posTrades = NotificationDocument.PosTrades.Where(x => x.bizDt == sDate)
                       .DefaultIfEmpty(new PosTrades() { bizDt = sDate, trade = new List<PosTrade>() }).FirstOrDefault().trade;

                List<TradeReport> unsettledTrades = NotificationDocument.UnsettledTrades.Where(x => x.bizDt == sDate)
                        .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() }).FirstOrDefault().trade;

                foreach (PosTrade trade in posTrades)
                {
                    TradeReport unsettledtrade = unsettledTrades.FirstOrDefault(x => x.OTCmlId == trade.OTCmlId);
                    if (null != unsettledtrade)
                    {
                        unsettledtrade.umgSpecified = trade.umgSpecified;
                        if (unsettledtrade.umgSpecified)
                            unsettledtrade.umg = trade.umg;
                    }
                }
            }
        }


        /// <summary>
        ///  Mise en place d'un état de synthèse sur une période (Date2>Date1)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance">Liste des trades CB de la période</param>
        /// <param name="pIdT">Liste des trades qui seront présents dans le document de messagerie (les trades pIdTCashBalance ne sont pas présents dans la liste)</param>
        /// FI 20150427 [20987] Add
        /// FI 20150623 [21149] Modify
        /// FI 20150707 [XXXXX] Modify
        /// FI 20150709 [XXXXX] Modify
        /// FI 20150716 [20892] Modify
        /// FI 20160613 [22256] Modify
        /// FI 20170217 [22862] Modify
        private void SetSynthesisOfPeriod(string pCS, int[] pIdTCashBalance, List<int> pIdT)
        {
            if (ArrFunc.IsEmpty(pIdTCashBalance))
                throw new ArgumentException("pIdTCashBalance is null");

            //tri au par date 
            int[] trade =
                (from item in pIdTCashBalance
                 orderby item
                 select item).ToArray();

            List<SQL_TradeRisk> tradeRisk = new List<SQL_TradeRisk>();
            for (int i = 0; i < ArrFunc.Count(trade); i++)
            {
                SQL_TradeRisk item = new SQL_TradeRisk(pCS, trade[i]);
                if (false == item.LoadTable(new string[] { "TRADE.IDT, TRADE.IDENTIFIER, TRADE.IDI, TRADE.DTBUSINESS" }))
                    throw new NullReferenceException(StrFunc.AppendFormat("trade (idT:{0}) not found", trade[i].ToString() ));
                tradeRisk.Add(item);

                Boolean isLastDay = (i == ArrFunc.Count(pIdTCashBalance) - 1);

                int idT = item.IdT;
                DateTime date = Convert.ToDateTime(item.GetFirstRowColumnValue("DTBUSINESS"));

                // Alimentation des trades du jour, des trades en attente de règlement, des trades réglés  
                // FI 20150623 [21124] Appel à SetTradesReportSynthesis et AddAllTrades
                // FI 20220708 [26097] Paramètre pIsAddCommonDataTrade à true
                SetTradesReportSynthesis(pCS, idT, date, isLastDay , true);
                AddAllTrades(pIdT, date);

                // Alimentation des actions sur positions
                // FI 20150623 [21124] Appel à SetPosActionReportSynthesis2 et AddAllPosActionsTrades
                SetPosActionsReportSynthesis(pCS, idT, date, true);
                AddAllPosActionsTrades(pIdT, date);

                //Alimentation des trades en position le dernier jour
                if (isLastDay)
                {
                    // FI 20150623 [21124] Appel à SetPositionTradeReportSynthesis2 et AddAllPosTrades
                    SetPosTradesReportSynthesis(pCS, idT, date, true);
                    AddAllPosTrades(pIdT, date);
                    // FI 20150716 [20892] Call CopyUMGInUnsettledTrades
                    CopyUMGInUnsettledTrades(date);

                    // FI 20160613 [22256] Add 
                    SetUnderlyingStockSynthesis(pCS, idT, date);
                }

                //Alimentation des position synthétiques
                //Rq Il est nécessaire de charger les posittions synthetiques car les FDA et BWA sont des élments enfants des positions synthétiques
                SetPosSyntheticsReportSynthesis(pCS, idT, date);

                // FI 20170217 [22862] call SetDvlTradesReportSynthesis && AddDvlTrades
                SetDvlTradesReportSynthesis(pCS, idT, date, true);
                AddDvlTrades(pIdT, date);

                //Alimentation Funding Amount sur les positions synthétiques
                SetPositionSyntheticFundingAmountReportSynthesis(pCS, idT, date);

                // FI 20150128 [20275] Appel à SetTradeCashPaymentReportSynthesis
                SetCashPaymentReportSynthesis(pCS, idT, date);

                // FI 20150709 [XXXXX] Add SetPositionSyntheticSafekeepingReportSynthesis
                SetPositionSyntheticSafekeepingReportSynthesis(pCS, idT, date);

            }

            // FI 20151019 [21317] Call SetAinDaysInTrades
            SetAinDaysInDebtSecurityTransaction(pCS);

            BuildCashBalancePeriodic(pCS, tradeRisk);

            //Suppression des trades CB
            foreach (int item in pIdTCashBalance)
                pIdT.Remove(item);

        }

        /// <summary>
        /// Retourne true si des évènements doivent être insérés dans le flux 
        /// </summary>
        /// <param name="pCnfMessage"></param>
        /// <returns></returns>
        private Boolean IsLoadEvent(CnfMessage pCnfMessage)
        {
            Nullable<NotificationTypeEnum> notificationType = pCnfMessage.NotificationType;
            NotificationDocumentSettings settings = pCnfMessage.GetSettings();

            /* 
             Remarque 
             * Ajouts des évènements systématiquement sur les éditons élémentaires (sauf POSSYNTHETIC) 
             * Pour les autres édtions (Synthèse par exemple) les évènements sont ajoutés si le paramétrage du message est  isUseChildEvents = true
             */

            bool ret = ArrFunc.IsFilled(pCnfMessage.eventTrigger);
            if (ret)
            {
                if (NotificationDocument.EfsMLversion >= EfsMLDocumentVersionEnum.Version35)
                {
                    //les évènements triggers ne sont plus insérés dans le trade sauf si la case "Nécessite la mise à disposition des événements dans le flux XML" est cochée	
                    ret = settings.IsUseChildEvents;
                }
                else if (notificationType.HasValue)
                {
                    switch (notificationType.Value)
                    {
                        case NotificationTypeEnum.POSSYNTHETIC:
                            //RD 20120821 [18087] New report type: POSSYNTHETIC
                            //les évènements triggers ne sont plus insérés dans le trade sauf si la case "Nécessite la mise à disposition des événements dans le flux XML" est cochée	
                            ret = settings.IsUseChildEvents;
                            break;
                    }
                }
            }

            return ret;
        }

        /// <summary>
        ///  Remplace le produit CashBalance par un CashBalanceReport
        ///  <para>CashBalanceReport</para>
        ///  <para>- est plus simple (facilite son exploitation)</para>
        ///  <para>- est moins lourd (Serialization)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTrade"></param>
        /// FI 20130707 [18745] add method
        /// FI 20140910 [20275] Modify
        /// FI 20150320 [XXPOC] Modify
        /// FI 20150323 [XXPOC] Modify
        /// FI 20150324 [XXPOC] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20161027 [22151] Modify
        private void ReplaceProductCashBalance(string pCS, ITrade pTrade)
        {
            ITrade trade = pTrade;
            if (false == trade.Product.ProductBase.IsCashBalance)
                throw new Exception("Trade is not a CashBalance");

            CashBalance cashBalance = (CashBalance)trade.Product;
            CashBalanceCalcAjustedDate(pCS, cashBalance);

            CashBalanceReport rcashBalance = new CashBalanceReport();

            // RD 20131018 [18600] Ajout du Book 
            if (pTrade.TradeHeader.PartyTradeIdentifier[0].BookIdSpecified)
                rcashBalance.Acct = pTrade.TradeHeader.PartyTradeIdentifier[0].BookId.Value;

            rcashBalance.BizDt = pTrade.TradeHeader.TradeDate.Value;
            rcashBalance.entityPartyReference = cashBalance.entityPartyReference;
            rcashBalance.cashBalanceOfficePartyReference = cashBalance.cashBalanceOfficePartyReference;
            rcashBalance.timing = cashBalance.timing;
            rcashBalance.productId = cashBalance.productId;
            rcashBalance.productType = cashBalance.productType;

            int cashBalanceStreamLenght = ArrFunc.Count(cashBalance.cashBalanceStream);
            rcashBalance.cashBalanceStream = new CashBalanceStreamReport[cashBalanceStreamLenght];
            for (int i = 0; i < cashBalanceStreamLenght; i++)
            {
                rcashBalance.cashBalanceStream[i] = new CashBalanceStreamReport();
                CashBalanceStreamReport rcbs = rcashBalance.cashBalanceStream[i];
                CashBalanceStream cbs = cashBalance.cashBalanceStream[i];

                rcbs.currency = cbs.currency.Value;

                rcbs.marginRequirement = NewCssExchangeCashPositionReport(cbs.marginRequirement, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.cashAvailable = NewCashAvailableReport(cbs.cashAvailable, rcashBalance.cashBalanceOfficePartyReference);

                // FI 20150324 [XXPOC] paramètre isReverse à true sur le cashUsed
                rcbs.cashUsedSpecified = cbs.cashUsedSpecified;
                if (rcbs.cashUsedSpecified)
                    rcbs.cashUsed = NewCashPositionReport(cbs.cashUsed, rcashBalance.cashBalanceOfficePartyReference, true);

                // FI 20150324 [XXPOC] paramètre isReverse à true sur le collateralAvailable
                rcbs.collateralAvailable = NewExchangeCashPositionReport(cbs.collateralAvailable, rcashBalance.cashBalanceOfficePartyReference, true);

                rcbs.collateralUsedSpecified = cbs.collateralUsedSpecified;
                if (rcbs.collateralUsedSpecified)
                    rcbs.collateralUsed = NewCashPositionReport(cbs.collateralUsed, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.uncoveredMarginRequirementSpecified = cbs.uncoveredMarginRequirementSpecified;
                if (rcbs.uncoveredMarginRequirementSpecified)
                    rcbs.uncoveredMarginRequirement = NewCashPositionReport(cbs.uncoveredMarginRequirement, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.marginCallSpecified = cbs.marginCallSpecified;
                if (rcbs.marginCallSpecified)
                    rcbs.marginCall = NewSimplePaymentReport(cbs.marginCall, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.cashBalance = NewCashPositionReport(cbs.cashBalance, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.previousMarginConstituentSpecified = cbs.previousMarginConstituentSpecified;
                if (rcbs.previousMarginConstituentSpecified)
                    rcbs.previousMarginConstituent = NewPreviousMarginConstituentReport(cbs.previousMarginConstituent, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.realizedMarginSpecified = cbs.realizedMarginSpecified;
                if (rcbs.realizedMarginSpecified)
                    rcbs.realizedMargin = NewMarginConstituentReport(cbs.realizedMargin, rcashBalance.cashBalanceOfficePartyReference, rcbs.currency);

                rcbs.unrealizedMarginSpecified = cbs.unrealizedMarginSpecified;
                if (rcbs.unrealizedMarginSpecified)
                    rcbs.unrealizedMargin = NewMarginConstituentReport(cbs.unrealizedMargin, rcashBalance.cashBalanceOfficePartyReference, rcbs.currency);

                rcbs.liquidatingValueSpecified = cbs.liquidatingValueSpecified;
                if (rcbs.liquidatingValueSpecified)
                    rcbs.liquidatingValue = NewOptionLiquidatingValueReport(cbs.liquidatingValue, rcashBalance.cashBalanceOfficePartyReference);

                // FI 20150623 [21149] Add
                rcbs.marketValueSpecified = cbs.marketValueSpecified;
                if (rcbs.marketValueSpecified)
                    rcbs.marketValue = NewDetailedCashPositionReport(cbs.marketValue, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.fundingSpecified = cbs.fundingSpecified;
                if (rcbs.fundingSpecified)
                    rcbs.funding = NewDetailedCashPositionReport(cbs.funding, rcashBalance.cashBalanceOfficePartyReference);

                // FI 20150323 [XXPOC] add rcbs.borrowing 
                rcbs.borrowingSpecified = cbs.borrowingSpecified;
                if (rcbs.borrowingSpecified)
                    rcbs.borrowing = NewDetailedCashPositionReport(cbs.borrowing, rcashBalance.cashBalanceOfficePartyReference);

                // FI 20150320 [XXPOC] add rcbs.unsettledCash
                rcbs.unsettledCashSpecified = cbs.unsettledCashSpecified;
                if (rcbs.unsettledCashSpecified)
                {
                    rcbs.unsettledCash = NewDetailedCashPositionReport(cbs.unsettledCash, rcashBalance.cashBalanceOfficePartyReference);
                    if ((rcbs.unsettledCash.dateDetailSpecified) && ArrFunc.Count(rcbs.unsettledCash.dateDetail) > 4)
                        rcbs.unsettledCash.dateDetail = ReduceArrayDetailedDateAmountReport(rcbs.unsettledCash);
                }


                rcbs.forwardCashPaymentSpecified = cbs.forwardCashPaymentSpecified;
                if (rcbs.forwardCashPaymentSpecified)
                {
                    rcbs.forwardCashPayment = new CashBalancePaymentReport();
                    CopyCashBalancePaymentReport(rcbs.forwardCashPayment, cbs.forwardCashPayment, rcashBalance.cashBalanceOfficePartyReference);
                }

                rcbs.equityBalanceSpecified = cbs.equityBalanceSpecified;
                if (rcbs.equityBalanceSpecified)
                    rcbs.equityBalance = NewCashPositionReport(cbs.equityBalance, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.equityBalanceWithForwardCashSpecified = cbs.equityBalanceWithForwardCashSpecified;
                if (rcbs.equityBalanceWithForwardCashSpecified)
                    rcbs.equityBalanceWithForwardCash = NewCashPositionReport(cbs.equityBalanceWithForwardCash, rcashBalance.cashBalanceOfficePartyReference);

                // FI 20150623 [21149] Add
                rcbs.totalAccountValueSpecified = cbs.totalAccountValueSpecified;
                if (rcbs.totalAccountValueSpecified)
                    rcbs.totalAccountValue = NewCashPositionReport(cbs.totalAccountValue, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.excessDeficitSpecified = cbs.excessDeficitSpecified;
                if (rcbs.excessDeficitSpecified)
                    rcbs.excessDeficit = NewCashPositionReport(cbs.excessDeficit, rcashBalance.cashBalanceOfficePartyReference);

                rcbs.excessDeficitWithForwardCashSpecified = cbs.excessDeficitWithForwardCashSpecified;
                if (rcbs.excessDeficitWithForwardCashSpecified)
                    rcbs.excessDeficitWithForwardCash = NewCashPositionReport(cbs.excessDeficitWithForwardCash, rcashBalance.cashBalanceOfficePartyReference);

            }

            rcashBalance.exchangeCashBalanceStreamSpecified = cashBalance.exchangeCashBalanceStreamSpecified;
            if (rcashBalance.exchangeCashBalanceStreamSpecified)
                rcashBalance.exchangeCashBalanceStream = NewExchangeCashBalanceStreamReport(cashBalance.exchangeCashBalanceStream, rcashBalance.cashBalanceOfficePartyReference);

            rcashBalance.fxRateSpecified = cashBalance.fxRateSpecified;
            if (rcashBalance.fxRateSpecified)
                rcashBalance.fxRate = cashBalance.fxRate;

            rcashBalance.settings = cashBalance.settings;

            // FI 20161027 [22151] Alimentation de endOfDayStatus
            rcashBalance.endOfDayStatusSpecified = cashBalance.endOfDayStatusSpecified;
            if (rcashBalance.endOfDayStatusSpecified)
                rcashBalance.endOfDayStatus = cashBalance.endOfDayStatus;

            //Replace du prodcuct CashBalance classique par le product CashBalance adapté au report
            trade.Product = rcashBalance;
        }

        /// <summary>
        /// Calcul des dates de valeurs sur :
        /// <para>
        /// - les flux  variationMargin, premium, cashSettlement, et frais
        /// </para>
        /// <para>
        /// - l 'appel restitution du déposit
        /// </para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pCashBalance"></param>
        /// FI 20130701 [18745] Add method
        // EG 20180205 [23769] Upd DataDocumentContainer parameter (substitution to the static class EFS_CURRENT)  
        private void CashBalanceCalcAjustedDate(string pCS, CashBalance pCashBalance)
        {
            DataDocumentContainer dataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);
            foreach (CashBalanceStream stream in pCashBalance.cashBalanceStream)
            {

                CashFlowsConstituent cashFlowsC = stream.cashAvailable.constituent.cashFlows.constituent;

                //variationMargin => calcul de la date de paymentDate ajustée => pour alimentation de la colonne date valeur
                ContractSimplePayment variationMargin = cashFlowsC.variationMargin;
                CalcAdjustedDate(pCS, variationMargin, dataDocument);

                //premium => calcul de la date de paymentDate ajustée => pour alimentation de la colonne date valeur
                ContractSimplePayment premium = cashFlowsC.premium;
                CalcAdjustedDate(pCS, premium, dataDocument);

                //cashSettlement => calcul de la date de paymentDate ajustée => pour alimentation de la colonne date valeur
                ContractSimplePayment cashSettlement = cashFlowsC.cashSettlement;
                CalcAdjustedDate(pCS, cashSettlement, dataDocument);

                DetailedContractPayment[] fee = cashFlowsC.fee;

                if (ArrFunc.IsFilled(fee))
                {
                    foreach (DetailedContractPayment item in fee)
                    {
                        CalcAdjustedDate(pCS, item, dataDocument);
                    }
                }

                if (stream.marginCallSpecified)
                    CalcAdjustedDate(pCS, stream.marginCall, dataDocument);

            }
        }

        /// <summary>
        /// Retourne un nouvel objet CssExchangeCashPositionReport à patir de CssExchangeCashPosition 
        /// </summary>
        /// <param name="pCssExchangeCashPosition"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20150314 [XXPOC] Modify Method
        private static CssExchangeCashPositionReport NewCssExchangeCashPositionReport(CssExchangeCashPosition pCssExchangeCashPosition, IReference pCashBalanceOffice)
        {
            CssExchangeCashPositionReport ret = new CssExchangeCashPositionReport();

            // FI 20150314 [XXPOC] paramètre à false
            CopyExchangeCashPositionToReport(ret, pCssExchangeCashPosition, pCashBalanceOffice, false);

            ret.detailSpecified = pCssExchangeCashPosition.detailSpecified;
            if (ret.detailSpecified)
            {
                ret.detail = new CssAmountReport[ArrFunc.Count(pCssExchangeCashPosition.detail)];
                for (int i = 0; i < ArrFunc.Count(pCssExchangeCashPosition.detail); i++)
                {
                    ret.detail[i] = new CssAmountReport();
                    CopyAmountSideToReport(ret.detail[i], pCssExchangeCashPosition.detail[i], ret.currency);
                    ret.detail[i].cssHrefSpecified = pCssExchangeCashPosition.detail[i].cssHrefSpecified;
                    if (ret.detail[i].cssHrefSpecified)
                        ret.detail[i].cssHref = pCssExchangeCashPosition.detail[i].cssHref;
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet CashAvailableReport à patir de CashAvailable 
        /// </summary>
        /// <param name="pCashAvailable"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20140909 [20275] Modify
        /// FI 20140922 [20275] Modify 
        /// FI 20150320 [XXPOC] Modify
        /// FI 20150324 [XXPOC] Modify
        /// FI 20150709 [XXXXX] Modify
        private static CashAvailableReport NewCashAvailableReport(CashAvailable pCashAvailable, IReference pCashBalanceOffice)
        {
            CashAvailableReport ret = new CashAvailableReport();

            // FI 20150324 [XXPOC] paramètre pIsReverse à false
            CopyExchangeCashPositionToReport(ret, pCashAvailable, pCashBalanceOffice, false);

            //FI 20140922 [20275] constituent est désormais facultatif
            ret.constituentSpecified = pCashAvailable.constituentSpecified;
            if (ret.constituentSpecified)
            {
                ret.constituent = new CashAvailableConstituentReport
                {
                    //previousCashBalance
                    previousCashBalance = new CashPositionReport(),
                    //cashBalancePayment
                    cashBalancePayment = new CashBalancePaymentReport(),
                    //cashFlows
                    cashFlows = new CashFlowsReport
                    {
                        //cashFlows constituent
                        constituent = new CashFlowsConstituentReport()
                    }
                };

                CopyCashPositionToReport(ret.constituent.previousCashBalance, pCashAvailable.constituent.previousCashBalance, pCashBalanceOffice);
                CopyCashBalancePaymentReport(ret.constituent.cashBalancePayment, pCashAvailable.constituent.cashBalancePayment, pCashBalanceOffice);
                CopyCashPositionToReport(ret.constituent.cashFlows, pCashAvailable.constituent.cashFlows, pCashBalanceOffice);

                //cashFlows constituent variationMargin
                ret.constituent.cashFlows.constituent.variationMargin =
                    NewContractSimplePaymentReport(pCashAvailable.constituent.cashFlows.constituent.variationMargin, pCashBalanceOffice);

                //cashFlows constituent premium
                ret.constituent.cashFlows.constituent.premium =
                    NewContractSimplePaymentReport(pCashAvailable.constituent.cashFlows.constituent.premium, pCashBalanceOffice);

                //cashFlows constituent cashSettlement
                // FI 20150320 [XXPOC] Call NewContractSimplePaymentConstituentReport
                ret.constituent.cashFlows.constituent.cashSettlement =
                    NewContractSimplePaymentConstituentReport(pCashAvailable.constituent.cashFlows.constituent.cashSettlement, pCashBalanceOffice);

                //cashFlows constituent fee
                ret.constituent.cashFlows.constituent.fee = new ContractPaymentReport[ArrFunc.Count(pCashAvailable.constituent.cashFlows.constituent.fee)];
                for (int i = 0; i < ArrFunc.Count(pCashAvailable.constituent.cashFlows.constituent.fee); i++)
                {
                    ret.constituent.cashFlows.constituent.fee[i] =
                        NewContractPaymentReport(pCashAvailable.constituent.cashFlows.constituent.fee[i], pCashBalanceOffice);
                }
                // FI 20150709 [XXXXX] Alimentation de safekeeping
                ret.constituent.cashFlows.constituent.safekeeping = new ContractPaymentReport[ArrFunc.Count(pCashAvailable.constituent.cashFlows.constituent.safekeeping)];
                for (int i = 0; i < ArrFunc.Count(pCashAvailable.constituent.cashFlows.constituent.safekeeping); i++)
                {
                    ret.constituent.cashFlows.constituent.safekeeping[i] =
                        NewContractPaymentReport(pCashAvailable.constituent.cashFlows.constituent.safekeeping[i], pCashBalanceOffice);
                }

                // PM 20170911 [23408] Add equalisationPayment
                //cashFlows constituent equalisationPayment
                ret.constituent.cashFlows.constituent.equalisationPaymentSpecified = pCashAvailable.constituent.cashFlows.constituent.equalisationPaymentSpecified;
                if (ret.constituent.cashFlows.constituent.equalisationPaymentSpecified)
                {
                    ret.constituent.cashFlows.constituent.equalisationPayment =
                        NewContractSimplePaymentReport(pCashAvailable.constituent.cashFlows.constituent.equalisationPayment, pCashBalanceOffice);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet OptionLiquidatingValueReport à patir de OptionLiquidatingValue 
        /// </summary>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20140910 [20275] Add method
        private static OptionLiquidatingValueReport NewOptionLiquidatingValueReport(OptionLiquidatingValue pOptionLiquidatingValue, IReference pCashBalanceOffice)
        {
            OptionLiquidatingValueReport ret = new OptionLiquidatingValueReport();
            CopyCashPositionToReport(ret, pOptionLiquidatingValue, pCashBalanceOffice);

            ret.longOptionValueSpecified = pOptionLiquidatingValue.longOptionValueSpecified;
            if (ret.longOptionValueSpecified)
                ret.longOptionValue = NewCashPositionReport(pOptionLiquidatingValue.longOptionValue, pCashBalanceOffice);

            ret.shortOptionValueSpecified = pOptionLiquidatingValue.shortOptionValueSpecified;
            if (ret.shortOptionValueSpecified)
                ret.shortOptionValue = NewCashPositionReport(pOptionLiquidatingValue.shortOptionValue, pCashBalanceOffice);

            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet CashPositionReport à patir de CashPosition 
        /// </summary>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20140910 [20275] Modify 
        /// FI 20150324 [XXPOC] Modify 
        private static CashPositionReport NewCashPositionReport(CashPosition pCashPosition, IReference pCashBalanceOffice)
        {
            // FI 20150324 [XXPOC] call  NewCashPositionReport method 
            return NewCashPositionReport(pCashPosition, pCashBalanceOffice, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMarginConstituent"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20150630 [XXXX]Add
        private static AssetMarginConstituentReport NewAssetMarginConstituentReport(AssetMarginConstituent pMarginConstituent, IReference pCashBalanceOffice)
        {
            AssetMarginConstituentReport ret = new AssetMarginConstituentReport();
            CopyCashPositionToReport(ret, pMarginConstituent, pCashBalanceOffice, false);

            ret.assetCategorySpecified = pMarginConstituent.assetCategorySpecified;
            if (ret.assetCategorySpecified)
                ret.assetCategory = pMarginConstituent.assetCategory;

            return ret;
        }


        /// <summary>
        /// Retourne un nouvel objet CashPositionReport à patir de CashPosition 
        /// </summary>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// <returns></returns>
        /// FI 20150324 [XXPOC] add Method
        private static CashPositionReport NewCashPositionReport(CashPosition pCashPosition, IReference pCashBalanceOffice, Boolean pIsReverse)
        {
            CashPositionReport ret = new CashPositionReport();
            // FI 20140910 [20275] appel de la méthode CopyCashPositionToReport
            // FI 20150324 [XXPOC] passage du paramètre  
            CopyCashPositionToReport(ret, pCashPosition, pCashBalanceOffice, pIsReverse);
            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet ExchangeCashPositionReport à patir de ExchangeCashPosition 
        /// </summary>
        /// <param name="pExchangeCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20150324 [XXPOC] Modify add paramètre pIsReverse
        private static ExchangeCashPositionReport NewExchangeCashPositionReport(ExchangeCashPosition pExchangeCashPosition, IReference pCashBalanceOffice, Boolean pIsReverse)
        {
            ExchangeCashPositionReport ret = new ExchangeCashPositionReport();
            // FI 20150324 [XXPOC] Modify passage du paramètre pIsReverse
            CopyExchangeCashPositionToReport(ret, pExchangeCashPosition, pCashBalanceOffice, pIsReverse);
            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet ContractSimplePaymentReport à patir de ContractSimplePayment 
        /// </summary>
        /// <param name="pPayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] Add Method
        /// FI 20140909 [20275] Rename NewDerivativeContractSimplePaymentReport to NewContractSimplePaymentReport
        /// FI 20140909 [20275] Modify
        /// FI 20150122 [XXXXX] Moddy
        /// FI 20150330 [XXPOC] Moddy 
        private static ContractSimplePaymentReport NewContractSimplePaymentReport(ContractSimplePayment pPayment, IReference pCashBalanceOffice)
        {
            ContractSimplePaymentReport ret = new ContractSimplePaymentReport();
            // FI 20150330 [XXPOC] Call CopyContractSimplePaymentReport
            CopyContractSimplePaymentReport(ret, pPayment, pCashBalanceOffice);
            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet DerivativeContractPaymentReport à patir de DerivativeContractPayment 
        /// </summary>
        /// <param name="pPayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] Add Method
        /// FI 20140909 [20275] Rename NewDerivativeContractPaymentReport to NewContractPaymentReport
        /// FI 20140909 [20275] Modify
        /// FI 20141209 [XXXXX] Modify
        private static ContractPaymentReport NewContractPaymentReport(DetailedContractPayment pPayment, IReference pCashBalanceOffice)
        {
            ContractPaymentReport ret = new ContractPaymentReport
            {
                amount = pPayment.paymentAmount.Amount.DecValue,
                currency = pPayment.paymentAmount.Currency,
                side = GetSideCashBalance(pPayment.receiverPartyReference, pCashBalanceOffice),
                paymentDate = pPayment.adjustedPaymentDate.Value,
                paymentTypeSpecified = pPayment.paymentTypeSpecified,
                detailSpecified = pPayment.detailSpecified
            };

            //FI 20141208 [XXXXX] alimentation de sideSpecified
            ret.sideSpecified = (ret.amount > 0);
            if (ret.paymentTypeSpecified)
                ret.paymentType = pPayment.paymentType.Value;

            if (ret.detailSpecified)
            {
                ret.detail = new ContractAmountAndTaxReport[ArrFunc.Count(pPayment.detail)];
                for (int i = 0; i < ArrFunc.Count(pPayment.detail); i++)
                {
                    ret.detail[i] = new ContractAmountAndTaxReport();

                    // FI 20150122 [XXXXX] Mise en commentaire CopyAmountSideToReport est appelé ds CopyContractAmountToReport  
                    //CopyAmountSideToReport(ret.detail[i], pPayment.detail[i], ret.currency);
                    CopyContractAmountToReport(ret.detail[i], pPayment.detail[i], ret.currency);

                    ret.detail[i].taxSpecified = pPayment.detail[i].taxSpecified;
                    if (ret.detail[i].taxSpecified)
                    {
                        ret.detail[i].tax = new ReportAmountSide();
                        CopyAmountSideToReport(ret.detail[i].tax, pPayment.detail[i].tax, ret.currency);
                    }
                }
            }
            // FI 20141209 [XXXXX] appel de la méthode CopyPaymentTaxToReportFee
            CopyPaymentTaxToReportFee(ret, pPayment);

            return ret;
        }

        /// <summary>
        /// Alimente les taxes dans {fee} à partir des taxes présentes dans {payment}
        /// </summary>
        /// <param name="fee"></param>
        /// <param name="payment"></param>
        /// FI 20141209 [XXXXX] Add Method
        /// FI 20150119 [XXXXX] Modify
        /// FI 20150518 [20987] Modify
        // RD 20160108 [21735] Modify        
        private static void CopyPaymentTaxToReportFee(ReportFee pFee, IPayment pPayment)
        {
            if (null == pPayment)
                throw new ArgumentException("pPayment is null");
            if (null == pFee)
                throw new ArgumentException("pFee is null");

            pFee.taxSpecified = false;

            if (ArrFunc.IsFilled(pPayment.Tax))
            {
                // RD 20160108 [21735] Add condition "where taxdetail.taxAmountSpecified"
                IEnumerable<ITaxSchedule> lstTaxShedule =
                                                from tax in pPayment.Tax
                                                from taxdetail in tax.TaxDetail
                                                where taxdetail.TaxAmountSpecified
                                                select taxdetail;

                pFee.taxSpecified = (lstTaxShedule.Count() > 0);
                if (pFee.taxSpecified)
                {
                    pFee.tax = new ReportTaxAmount[lstTaxShedule.Count()];
                    int j = 0;
                    foreach (ITaxSchedule taxSchedule in lstTaxShedule)
                    {
                        pFee.tax[j] = new ReportTaxAmount
                        {
                            amount = taxSchedule.TaxAmount.Amount.Amount.DecValue
                        };
                        // FI 20150518 [20987] add if
                        if (null != taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailScheme))
                            pFee.tax[j].taxId = taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailScheme).Value;
                        // FI 20150518 [20987] add if
                        // FI 20150119 [XXXXX] Alimentation de taxType
                        if (null != taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailTypeScheme))
                            pFee.tax[j].taxType = taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailTypeScheme).Value;
                        // FI 20150518 [20987] add if
                        if (null != taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme))
                            pFee.tax[j].rate = Convert.ToDecimal(taxSchedule.TaxSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryTaxDetailRateScheme).Value);
                        j++;
                    }
                }
            }
        }

        /// <summary>
        /// Retourne un nouvel objet SimplePaymentReport à patir de SimplePayment 
        /// </summary>
        /// <param name="pPayment"></param>
        /// FI 20130701 [18745] add Method
        private static SimplePaymentReport NewSimplePaymentReport(SimplePayment pPayment, IReference pCashBalanceOffice)
        {
            SimplePaymentReport ret = new SimplePaymentReport
            {
                amount = pPayment.paymentAmount.Amount.DecValue,
                currency = pPayment.paymentAmount.Currency,
                side = GetSideCashBalance(pPayment.receiverPartyReference, pCashBalanceOffice)
            };
            //FI 20141208 [XXXXX] alimentation de sideSpecified
            ret.sideSpecified = (ret.amount > 0);

            // FI 20150427 [20987] Modify add (null != pPayment.paymentDate)
            if ((null != pPayment.paymentDate) && pPayment.paymentDate.adjustedDateSpecified)
                ret.paymentDate = pPayment.paymentDate.adjustedDate.Value;

            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet PreviousMarginConstituentReport à patir de PreviousMarginConstituent 
        /// </summary>
        /// <param name="pPreviousMarginConstituent"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20150324 [XXPOC] Modify
        private static PreviousMarginConstituentReport NewPreviousMarginConstituentReport(PreviousMarginConstituent pPreviousMarginConstituent, IReference pCashBalanceOffice)
        {
            PreviousMarginConstituentReport ret = new PreviousMarginConstituentReport
            {
                marginRequirement = NewCashPositionReport(pPreviousMarginConstituent.marginRequirement, pCashBalanceOffice),
                cashAvailable = NewCashPositionReport(pPreviousMarginConstituent.cashAvailable, pCashBalanceOffice),
                // FI 20150324 [XXPOC] pIsReverse = true;
                cashUsed = NewCashPositionReport(pPreviousMarginConstituent.cashUsed, pCashBalanceOffice, true),
                // FI 20150324 [XXPOC] pIsReverse = true;
                collateralAvailable = NewCashPositionReport(pPreviousMarginConstituent.collateralAvailable, pCashBalanceOffice, true),
                collateralUsed = NewCashPositionReport(pPreviousMarginConstituent.collateralUsed, pCashBalanceOffice),
                uncoveredMarginRequirement = NewCashPositionReport(pPreviousMarginConstituent.uncoveredMarginRequirement, pCashBalanceOffice)
            };

            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet MarginConstituentReport à patir de MarginConstituent 
        /// </summary>
        /// <param name="pPreviousMarginConstituent"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// FI 20140910 [20275] Modify Method
        /// FI 20150320 [XXPOC] Modify
        private static MarginConstituentReport NewMarginConstituentReport(MarginConstituent pMarginConstituent, IReference pCashBalanceOffice, string pCurrency)
        {
            MarginConstituentReport ret = new MarginConstituentReport
            {
                globalAmountSpecified = pMarginConstituent.globalAmountSpecified,
                futureSpecified = pMarginConstituent.futureSpecified,
                optionSpecified = pMarginConstituent.optionSpecified,
                otherSpecified = pMarginConstituent.otherSpecified,
                detailSpecified = pMarginConstituent.detailSpecified
            };

            if (ret.globalAmountSpecified)
                ret.globalAmount = NewCashPositionReport(pMarginConstituent.globalAmount, pCashBalanceOffice);
            if (ret.futureSpecified)
                ret.future = NewCashPositionReport(pMarginConstituent.future, pCashBalanceOffice);
            if (ret.optionSpecified)
            {
                ret.option = new OptionMarginConstituentReport[ArrFunc.Count(pMarginConstituent.option)];
                for (int i = 0; i < ArrFunc.Count(pMarginConstituent.option); i++)
                {
                    ret.option[i] = new OptionMarginConstituentReport();
                    CopyCashPositionToReport(ret.option[i], pMarginConstituent.option[i], pCashBalanceOffice);
                    ret.option[i].valuationMethod = pMarginConstituent.option[i].valuationMethod;
                }
            }
            if (ret.otherSpecified)
            {
                ret.other = new AssetMarginConstituentReport[ArrFunc.Count(pMarginConstituent.other)];
                for (int i = 0; i < ArrFunc.Count(pMarginConstituent.other); i++)
                    ret.other[i] = NewAssetMarginConstituentReport(pMarginConstituent.other[i], pCashBalanceOffice);
            }
            if (ret.detailSpecified)
            {
                ret.detail = new ContractAmountReport[ArrFunc.Count(pMarginConstituent.detail)];
                for (int i = 0; i < ArrFunc.Count(pMarginConstituent.detail); i++)
                {
                    ret.detail[i] = new ContractAmountReport();

                    // FI 20150122 [XXXXX] Mise en commentaire CopyAmountSideToReport est appelé ds CopyContractAmountToReport  
                    //CopyAmountSideToReport(ret.detail[i], pPayment.detail[i], ret.currency);
                    CopyContractAmountToReport(ret.detail[i], pMarginConstituent.detail[i], pCurrency);
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente un CashPositionReport à partir d'un CashPosition
        /// </summary>
        /// <param name="pCashPosReport"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// RD 20150310 [20857] add Method
        private static void CopyCashPositionToReport(CashPositionReport pCashPosReport, CashPosition pCashPosition, IReference pCashBalanceOffice)
        {
            CopyCashPositionToReport(pCashPosReport, pCashPosition, pCashBalanceOffice, false);
        }

        /// <summary>
        /// Alimente un CashPositionReport à partir d'un CashPosition
        /// <para>- Inversion du sens si {pIsReverse} est à true</para>
        /// </summary>
        /// <param name="pCashPosReport"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// FI 20130701 [18745] add Method
        /// RD 20150310 [20857] add pIsReverse
        private static void CopyCashPositionToReport(CashPositionReport pCashPosReport, CashPosition pCashPosition, IReference pCashBalanceOffice, bool pIsReverse)
        {
            pCashPosReport.amount = pCashPosition.GetAmount().DecValue;
            pCashPosReport.currency = pCashPosition.GetCurrency();
            // RD 20150310 [20857] utilisation pIsReverse
            pCashPosReport.side = GetSideCashBalance(pCashPosition.receiverPartyReference, pCashBalanceOffice, pIsReverse);
            //FI 20141208 [XXXXX] alimentation de sideSpecified
            pCashPosReport.sideSpecified = (pCashPosReport.amount > 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashBalancePaymentReport"></param>
        /// <param name="pCashBlancePayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// FI 20140909 [20275] add Method
        /// RD 20150310 [20857] Modify
        private static void CopyCashBalancePaymentReport(CashBalancePaymentReport pCashBalancePaymentReport, CashBalancePayment pCashBalancePayment, IReference pCashBalanceOffice)
        {
            // RD 20150310 [20857] inversion du sens du cashPayment
            CopyCashPositionToReport(pCashBalancePaymentReport, pCashBalancePayment, pCashBalanceOffice, true);

            pCashBalancePaymentReport.cashDepositSpecified = pCashBalancePayment.cashDepositSpecified;
            if (pCashBalancePaymentReport.cashDepositSpecified)
                pCashBalancePaymentReport.cashDeposit = NewDetailedCashPaymentReport(pCashBalancePayment.cashDeposit, pCashBalanceOffice);

            pCashBalancePaymentReport.cashWithdrawalSpecified = pCashBalancePayment.cashWithdrawalSpecified;
            if (pCashBalancePaymentReport.cashWithdrawalSpecified)
                pCashBalancePaymentReport.cashWithdrawal = NewDetailedCashPaymentReport(pCashBalancePayment.cashWithdrawal, pCashBalanceOffice);

        }

        /// <summary>
        /// Génère une instance DetailedCashPaymentReport à partir d'une instance de  DetailedCashPayment
        /// </summary>
        /// <param name="pDetailedCashPayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// RD 20150310 [20857] Modify
        private static DetailedCashPaymentReport NewDetailedCashPaymentReport(DetailedCashPayment pDetailedCashPayment, IReference pCashBalanceOffice)
        {
            DetailedCashPaymentReport ret = new DetailedCashPaymentReport();
            // RD 20150310 [20857] inversion du sens du détail du cashPayment
            CopyCashPositionToReport(ret, pDetailedCashPayment, pCashBalanceOffice, true);

            ret.detailSpecified = pDetailedCashPayment.detailSpecified;

            if (ret.detailSpecified)
            {
                ret.detail = new CashPaymentDetailReport[ArrFunc.Count(ret.detail)];
                for (int i = 0; i < ArrFunc.Count(ret.detail); i++)
                {
                    ret.detail[i] = new CashPaymentDetailReport();
                    // RD 20150310 [20857] inversion du sens du détail du cashPayment
                    CopyAmountSideToReport(ret.detail[i], pDetailedCashPayment.detail[i], ret.currency, true);
                    ret.detail[i].paymentTypeSpecified = pDetailedCashPayment.detail[i].paymentTypeSpecified;
                    if (ret.detail[i].paymentTypeSpecified)
                        ret.detail[i].paymentType = pDetailedCashPayment.detail[i].paymentType.Value;
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente un ReportAmountSide à partir d'un AmountSide
        /// </summary>
        /// <param name="pReportAmoutSide"></param>
        /// <param name="pAmountSide"></param>
        /// <param name="pCurrency"></param>
        /// RD 20150310 [20857] add Method
        private static void CopyAmountSideToReport(ReportAmountSide pReportAmoutSide, AmountSide pAmountSide, string pCurrency)
        {
            CopyAmountSideToReport(pReportAmoutSide, pAmountSide, pCurrency, false);
        }

        /// <summary>
        /// Alimente un ReportAmountSide à partir d'un AmountSide
        /// <para>- Inversion du sens si {pIsReverse} est à true</para>/// </summary>
        /// <param name="pReportAmoutSide"></param>
        /// <param name="pAmountSide"></param>
        /// <param name="pCurrency"></param>
        /// FI 20140910 [20275] Add Method
        /// RD 20150310 [20857] Add pIsReverse and Modify
        private static void CopyAmountSideToReport(ReportAmountSide pReportAmoutSide, AmountSide pAmountSide, string pCurrency, bool pIsReverse)
        {
            pReportAmoutSide.side = pAmountSide.AmtSide;
            // RD 20150310 [20857] inversion du sens si pIsReverse est à true
            if (pIsReverse)
                pReportAmoutSide.side = (pReportAmoutSide.side == CrDrEnum.DR ? CrDrEnum.CR : CrDrEnum.DR);
            //FI 20141208 [XXXXX] alimentation de sideSpecified
            pReportAmoutSide.sideSpecified = pAmountSide.AmtSideSpecified;
            pReportAmoutSide.amount = pAmountSide.Amt;
            pReportAmoutSide.currency = pCurrency;
        }

        /// <summary>
        /// Alimente un ContractAmountReport à partir d'un ContractAmount
        /// </summary>
        /// <param name="pContractAmountReport"></param>
        /// <param name="pContractAmount"></param>
        /// FI 20140910 [20275] Add Method
        /// FI 20150122 [XXXXX] Modification de signature  
        private static void CopyContractAmountToReport(ContractAmountReport pContractAmountReport, ContractAmount pContractAmount, string pCurrency)
        {

            //FI 20150122 [XXXXX] Appel à CopyAmountSideToReport
            CopyAmountSideToReport(pContractAmountReport, pContractAmount, pCurrency);

            // FI 20140909 [20275] Alimentation de .idAsset et assetCategory
            pContractAmountReport.idAssetSpecified = pContractAmount.otcmlIdSpecified;
            if (pContractAmountReport.idAssetSpecified)
                pContractAmountReport.idAsset = pContractAmount.OTCmlId;

            pContractAmountReport.assetCategorySpecified = pContractAmount.assetCategorySpecified;
            if (pContractAmountReport.assetCategorySpecified)
                pContractAmountReport.assetCategory = pContractAmount.assetCategory;

            pContractAmountReport.SymSpecified = pContractAmount.SymSpecified;
            if (pContractAmountReport.SymSpecified)
                pContractAmountReport.sym = pContractAmount.Sym;

            pContractAmountReport.exchSpecified = pContractAmount.ExchSpecified;
            if (pContractAmountReport.exchSpecified)
                pContractAmountReport.exch = pContractAmount.Exch;
        }

        /// <summary>
        /// Alimente un ExchangeCashPositionReport à partir d'un ExchangeCashPosition
        /// </summary>
        /// <param name="pExCashPosReport"></param>
        /// <param name="pExchangeCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// FI 20130701 [18745] add Method
        /// FI 20150324 [XXPOC] Modify add paramètre pIsReverse
        private static void CopyExchangeCashPositionToReport(ExchangeCashPositionReport pExCashPosReport, ExchangeCashPosition pExchangeCashPosition, IReference pCashBalanceOffice, Boolean pIsReverse)
        {
            /// FI 20150324 [XXPOC] passage du paramètre pIsReverse
            CopyCashPositionToReport(pExCashPosReport, pExchangeCashPosition, pCashBalanceOffice, pIsReverse);

            pExCashPosReport.exchangeAmountSpecified = pExchangeCashPosition.exchangeAmountSpecified;
            if (pExCashPosReport.exchangeAmountSpecified)
            {
                pExCashPosReport.exchangeAmount.amount = pExchangeCashPosition.GetAmount().DecValue;
                pExCashPosReport.exchangeAmount.currency = pExchangeCashPosition.GetCurrency();
                pExCashPosReport.exchangeAmount.side = pExCashPosReport.side;
                //FI 20141208 [XXXXX] alimentation de sideSpecified
                pExCashPosReport.exchangeAmount.sideSpecified = pExCashPosReport.sideSpecified;
            }

            pExCashPosReport.exchangeFxRateReferenceSpecified = pExchangeCashPosition.exchangeFxRateReferenceSpecified;
            if (pExCashPosReport.exchangeFxRateReferenceSpecified)
                pExCashPosReport.exchangeFxRateReference = pExchangeCashPosition.exchangeFxRateReference;
        }

        /// <summary>
        /// Retourne un nouvel objet ExchangeCashBalanceStreamReport à partir de ExchangeCashBalanceStream 
        /// </summary>
        /// <param name="pExchangeCashBalanceStream"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] Add Method
        /// FI 20140912 [20275] Modify Prise en considération des nouveaux elements liquidatingValue,funding,forwardCashPayment,...etc
        /// FI 20140922 [20275] Modify
        /// FI 20150320 [XXPOC] Modify
        /// FI 20150323 [XXPOC] Modify
        /// FI 20150324 [XXPOC] Modify
        private static ExchangeCashBalanceStreamReport NewExchangeCashBalanceStreamReport(ExchangeCashBalanceStream pExchangeCashBalanceStream, IReference pCashBalanceOffice)
        {
            ExchangeCashBalanceStreamReport ret = new ExchangeCashBalanceStreamReport
            {
                currency = pExchangeCashBalanceStream.currency.Value,
                marginRequirementSpecified = pExchangeCashBalanceStream.marginRequirementSpecified,
                //FI 20140922 [20275] cashAvailable est de type CashAvailableReport
                cashAvailableSpecified = pExchangeCashBalanceStream.cashAvailableSpecified,
                cashUsedSpecified = pExchangeCashBalanceStream.cashUsedSpecified,
                collateralAvailableSpecified = pExchangeCashBalanceStream.collateralAvailableSpecified,
                collateralUsedSpecified = pExchangeCashBalanceStream.collateralUsedSpecified,
                uncoveredMarginRequirementSpecified = pExchangeCashBalanceStream.uncoveredMarginRequirementSpecified,
                marginCallSpecified = pExchangeCashBalanceStream.marginCallSpecified,
                cashBalanceSpecified = pExchangeCashBalanceStream.cashBalanceSpecified,
                previousMarginConstituentSpecified = pExchangeCashBalanceStream.previousMarginConstituentSpecified,
                realizedMarginSpecified = pExchangeCashBalanceStream.realizedMarginSpecified,
                unrealizedMarginSpecified = pExchangeCashBalanceStream.unrealizedMarginSpecified,
                liquidatingValueSpecified = pExchangeCashBalanceStream.liquidatingValueSpecified,
                marketValueSpecified = pExchangeCashBalanceStream.marketValueSpecified,
                fundingSpecified = pExchangeCashBalanceStream.fundingSpecified,
                borrowingSpecified = pExchangeCashBalanceStream.borrowingSpecified,
                unsettledCashSpecified = pExchangeCashBalanceStream.unsettledCashSpecified,
                forwardCashPaymentSpecified = pExchangeCashBalanceStream.forwardCashPaymentSpecified,
                equityBalanceSpecified = pExchangeCashBalanceStream.equityBalanceSpecified,
                equityBalanceWithForwardCashSpecified = pExchangeCashBalanceStream.equityBalanceWithForwardCashSpecified,
                totalAccountValueSpecified = pExchangeCashBalanceStream.totalAccountValueSpecified,
                excessDeficitSpecified = pExchangeCashBalanceStream.excessDeficitSpecified,
                excessDeficitWithForwardCashSpecified = pExchangeCashBalanceStream.excessDeficitWithForwardCashSpecified
            };

            if (ret.marginRequirementSpecified)
                ret.marginRequirement = NewCashPositionReport(pExchangeCashBalanceStream.marginRequirement, pCashBalanceOffice);
            if (ret.cashAvailableSpecified)
                ret.cashAvailable = NewCashAvailableReport(pExchangeCashBalanceStream.cashAvailable, pCashBalanceOffice);
            if (ret.cashUsedSpecified)
                ret.cashUsed = NewCashPositionReport(pExchangeCashBalanceStream.cashUsed, pCashBalanceOffice);
            if (ret.collateralAvailableSpecified)
                // FI 20150324 [XXPOC] isreverse sur le collateralAvailable
                ret.collateralAvailable = NewCashPositionReport(pExchangeCashBalanceStream.collateralAvailable, pCashBalanceOffice, true);
            if (ret.collateralUsedSpecified)
                // FI 20150324 [XXPOC] isreverse sur le collateralAvailable
                ret.collateralUsed = NewCashPositionReport(pExchangeCashBalanceStream.collateralUsed, pCashBalanceOffice, true);
            if (ret.uncoveredMarginRequirementSpecified)
                ret.uncoveredMarginRequirement = NewCashPositionReport(pExchangeCashBalanceStream.uncoveredMarginRequirement, pCashBalanceOffice);
            if (ret.marginCallSpecified)
                ret.marginCall = NewSimplePaymentReport(pExchangeCashBalanceStream.marginCall, pCashBalanceOffice);
            if (ret.cashBalanceSpecified)
                ret.cashBalance = NewCashPositionReport(pExchangeCashBalanceStream.cashBalance, pCashBalanceOffice);
            if (ret.previousMarginConstituentSpecified)
                ret.previousMarginConstituent = NewPreviousMarginConstituentReport(pExchangeCashBalanceStream.previousMarginConstituent, pCashBalanceOffice);
            if (ret.realizedMarginSpecified)
                ret.realizedMargin = NewMarginConstituentReport(pExchangeCashBalanceStream.realizedMargin, pCashBalanceOffice, ret.currency);
            if (ret.unrealizedMarginSpecified)
                ret.unrealizedMargin = NewMarginConstituentReport(pExchangeCashBalanceStream.unrealizedMargin, pCashBalanceOffice, ret.currency);
            if (ret.liquidatingValueSpecified)
                ret.liquidatingValue = NewOptionLiquidatingValueReport(pExchangeCashBalanceStream.liquidatingValue, pCashBalanceOffice);
            // FI 20150623 [21149] Add
            if (ret.marketValueSpecified)
                ret.marketValue = NewDetailedCashPositionReport(pExchangeCashBalanceStream.marketValue, pCashBalanceOffice);
            if (ret.fundingSpecified)
                ret.funding = NewDetailedCashPositionReport(pExchangeCashBalanceStream.funding, pCashBalanceOffice);
            // FI 20150323 [XXPOC] Add borrowing
            if (ret.borrowingSpecified)
                ret.borrowing = NewDetailedCashPositionReport(pExchangeCashBalanceStream.borrowing, pCashBalanceOffice);
            // FI 20150623 [21149] Appel à ReduceArrayDetailedDateAmountReport
            if (ret.unsettledCashSpecified)
            {
                ret.unsettledCash = NewDetailedCashPositionReport(pExchangeCashBalanceStream.unsettledCash, pCashBalanceOffice);
                if ((ret.unsettledCash.dateDetailSpecified) && ArrFunc.Count(ret.unsettledCash.dateDetail) > 4)
                    ret.unsettledCash.dateDetail = ReduceArrayDetailedDateAmountReport(ret.unsettledCash);
            }
            if (ret.forwardCashPaymentSpecified)
            {
                ret.forwardCashPayment = new CashBalancePaymentReport();
                CopyCashBalancePaymentReport(ret.forwardCashPayment, pExchangeCashBalanceStream.forwardCashPayment, pCashBalanceOffice);
            }
            if (ret.equityBalanceSpecified)
                ret.equityBalance = NewCashPositionReport(pExchangeCashBalanceStream.equityBalance, pCashBalanceOffice);
            if (ret.equityBalanceWithForwardCashSpecified)
                ret.equityBalanceWithForwardCash = NewCashPositionReport(pExchangeCashBalanceStream.equityBalanceWithForwardCash, pCashBalanceOffice);
            // FI 20150623 [21149] Add
            if (ret.totalAccountValueSpecified)
                ret.totalAccountValue = NewCashPositionReport(pExchangeCashBalanceStream.totalAccountValue, pCashBalanceOffice);
            if (ret.excessDeficitSpecified)
                ret.excessDeficit = NewCashPositionReport(pExchangeCashBalanceStream.excessDeficit, pCashBalanceOffice);
            if (ret.excessDeficitWithForwardCashSpecified)
                ret.excessDeficitWithForwardCash = NewCashPositionReport(pExchangeCashBalanceStream.excessDeficitWithForwardCash, pCashBalanceOffice);
            return ret;
        }

        /// <summary>
        /// Retourne le sens CR, DR 
        /// </summary>
        /// <param name="pReceiver"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// RD 20150310 [20857] add Method
        private static CrDrEnum GetSideCashBalance(IReference pReceiver, IReference pCashBalanceOffice)
        {
            return GetSideCashBalance(pReceiver, pCashBalanceOffice, false);
        }

        /// <summary>
        /// Retourne le sens CR, DR 
        /// <para>- Inversion du sens si {pIsReverse} est à true</para>
        /// </summary>
        /// <param name="pReceiver"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <param name="pIsReverse"></param>
        /// <returns></returns>
        /// FI 20130701 [18745] add Method
        /// RD 20150310 [20857] add pIsReverse and modify
        private static CrDrEnum GetSideCashBalance(IReference pReceiver, IReference pCashBalanceOffice, bool pIsReverse)
        {
            CrDrEnum ret;
            if (pReceiver.HRef == pCashBalanceOffice.HRef)
                ret = CrDrEnum.CR;
            else
                ret = CrDrEnum.DR;

            /// RD 20150310 [20857] inversion du sens si pIsReverse est à true
            if (pIsReverse)
                ret = (ret == CrDrEnum.DR ? CrDrEnum.CR : CrDrEnum.DR);

            return ret;
        }

        /// <summary>
        /// Retourne un nouvel objet DetailedCashPositionReport à patir de DetailedCashPosition 
        /// </summary>
        /// <param name="pCashPosition"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20140910 [20275] Add Method
        /// FI 20150115 [XXXXX] Modify
        /// FI 20150622 [21124] Modify Method
        private static DetailedCashPositionReport NewDetailedCashPositionReport(DetailedCashPosition pDetailedCashPosition, IReference pCashBalanceOffice)
        {
            DetailedCashPositionReport ret = new DetailedCashPositionReport();

            CopyCashPositionToReport(ret, pDetailedCashPosition, pCashBalanceOffice);

            ret.detailSpecified = pDetailedCashPosition.detailSpecified;
            if (ret.detailSpecified)
            {
                // FI 20150115 [XXXXX] Modify
                ret.detail = new ContractAmountReport[ArrFunc.Count(pDetailedCashPosition.detail)];
                for (int i = 0; i < ArrFunc.Count(pDetailedCashPosition.detail); i++)
                {
                    ret.detail[i] = new ContractAmountReport();
                    CopyContractAmountToReport(ret.detail[i], pDetailedCashPosition.detail[i], ret.currency);
                }
            }

            // FI 20150622 [21124] Alimentation de dateDetail 
            ret.dateDetailSpecified = pDetailedCashPosition.dateDetailSpecified;
            if (ret.dateDetailSpecified)
            {
                ret.dateDetail = new DetailedDateAmountReport[ArrFunc.Count(pDetailedCashPosition.dateDetail)];
                for (int i = 0; i < ArrFunc.Count(pDetailedCashPosition.dateDetail); i++)
                {
                    ret.dateDetail[i] = NewDetailedDateAmountToReport(pDetailedCashPosition.dateDetail[i], ret.currency);
                }
            }

            return ret;
        }

        /// <summary>
        /// Alimentation de l'élément tradeCciMatch
        /// <para>Spheres® alimente le document avec les données non matchée (MATCHSTATUS==MISMATCH)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">lsite des trades (pour les confirmation la liste est renseignée avec 1 seule trade)</param>
        /// FI 20140731 [20179] Add method
        /// FI 20141218 [20574] Modify
        /// EG 20150116 [20699] Replace t.IDENTIFER par lst.IDENTIFIER
        private void SetTradeCciMatch(string pCS, int[] pIdT)
        {
            /// EG 20150116 [20699] Replace t.IDENTIFER par lst.IDENTIFIER
            string query = StrFunc.AppendFormat(@"select 
            tm.IDT, lst.IDENTIFIER, tm.CLIENTID, tm.MATCHSTATUS, tm.DATATYPE, tm.VALUE, tm.LABELCULTURE, tm.LABELVALUE, tm.LABELVALUE_EN
            from dbo.TRADESTMATCHCCI tm
            {0}
            where tm.MATCHSTATUS='{1}'", GetJoinTrade(pCS, pIdT, "tm.IDT", false), Cst.MatchEnum.mismatch.ToString().ToUpper());

            List<TradeCciMatch> lstTradeCciMatch = new List<TradeCciMatch>();
            DataTable dt = DataHelper.ExecuteDataTable(pCS, query);
            if (dt.Rows.Count > 0)
            {
                List<DataRow> rows = dt.Rows.Cast<DataRow>().ToList();

                // lstIdT => liste des différents trade
                List<int> lstIdT = (from item in rows
                                    select Convert.ToInt32(item["IDT"])).Distinct().ToList();

                // Il existe autant de lstTradeCciMatch de de item dans lstIdT
                lstTradeCciMatch = (from idt in lstIdT
                                    select new TradeCciMatch() { OTCmlId = idt }).ToList();

                // Alimentation de chaque lstTradeCciMatch
                foreach (TradeCciMatch item in lstTradeCciMatch)
                {
                    DataRow[] row = (from rowItem in rows
                                     where (Convert.ToInt32(rowItem["IDT"]) == item.OTCmlId)
                                     select rowItem).ToArray();
                    // Identifier du trade
                    item.tradeIdentifier = Convert.ToString(row.First()["IDENTIFIER"]);
                    // Liste des données sujettes à contrôle
                    item.cciMatch = new List<CciMatch>();
                    foreach (DataRow rowcci in row)
                    {
                        item.cciMatch.Add(new CciMatch()
                        {
                            clientId = rowcci["CLIENTID"].ToString(),
                            dataType = rowcci["DATATYPE"].ToString(),
                            label = new List<CciLabel>(2){
                                        new CciLabel(){culture= rowcci["LABELCULTURE"].ToString() , value =rowcci["LABELVALUE"].ToString()},
                                        new CciLabel(){culture= "en-GB" , value =rowcci["LABELVALUE_EN"].ToString()
                                        }
                            },
                            value = rowcci["VALUE"].ToString(),
                            status = rowcci["MATCHSTATUS"].ToString()
                        });
                    }
                }
            }

            NotificationDocument.TradeCciMatchSpecified = ArrFunc.IsFilled(lstTradeCciMatch);
            if (NotificationDocument.TradeCciMatchSpecified)
                NotificationDocument.TradeCciMatch = lstTradeCciMatch;
        }

       

        /// <summary>
        /// Mise à jour du frontId et FolderId et UTI   
        /// </summary>
        /// FI 20140910 [20275] add Method
        /// FI 20141217 [20574] Refactoring Mise à jour des UTI des trades
        /// FI 20150115 [XXXXX] Modify
        /// FI 20150115 [XXXXX] Modify
        /// EG 20150331 [XXPOC] FxLeg|FxOptionLeg
        /// FI 20150623 [21149] Modify
        /// FI 20150702 [XXXXX] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify (Chgt de signature => add pCS)
        /// EG 20171025 [23509] Set timeStamp with Time of DtExecution
        private void UpdCommonDataTrade2(string pCS)
        {
            if (_notificationDocument.CommonDataSpecified)
            {
                foreach (CommonTradeAlloc tradeItem in _notificationDocument.CommonData.trade)
                {

                    ITrade trade =
                    (from item in NotificationDocument.DataDocument.Trade
                     where item.TradeId == tradeItem.tradeIdentifier
                     select item).FirstOrDefault();

                    if (null != trade)
                    {
                        DataDocumentContainer tradeDatadoc = new DataDocumentContainer(_notificationDocument.DataDocument);
                        tradeDatadoc.SetCurrentTrade(trade);

                        // FI 20161214 [21916] Appel à RptSide()
                        RptSideProductContainer product = tradeDatadoc.CurrentProduct.RptSide();
                        if (null == product)
                            throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented", tradeDatadoc.CurrentProduct.ProductBase.ProductName));


                        // FI 20190515 [23912] Add
                        tradeItem.tzFacility = tradeDatadoc.GetTimeZoneFacility();
                        
                        // FI 20150515 [XXXXX] trdDt et timeStamp sont alimentés à partie de trade.tradeHeader
                        tradeItem.trdDt = trade.TradeHeader.TradeDate.Value;
                        tradeItem.trdTm = trade.TradeHeader.TradeDate.TimeStampHHMMSS;

                        // FI 20190515 [23912] Alimentation de ordDt et ordTm
                        IPartyTradeInformation partyTradeInformation = tradeDatadoc.GetPartyTradeInformationFacility();
                        if (null != partyTradeInformation && partyTradeInformation.TimestampsSpecified && partyTradeInformation.Timestamps.OrderEnteredSpecified)
                        {
                            // FI 20190729  [XXXXX] Utilisation de la méthode GetTradeTimeZone 
                            //=> Le timezone n'est pas rensigné surla plateforme XOFF. Dans ce cas Sphreres utilise le timezone associé à l'entité
                            // FI 20200520 [XXXXX] Add SQL cache
                            string tzdbid = tradeDatadoc.GetTradeTimeZone(CSTools.SetCacheOn(pCS));
                            TimeZoneInfo tzInfo = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbid);
                            DateTime orderEnteredDateTime = TimeZoneInfo.ConvertTimeFromUtc(partyTradeInformation.Timestamps.OrderEnteredDateTimeOffset.Value.UtcDateTime, tzInfo);

                            string orderEnteredDateTimeValue = Tz.Tools.ToString(orderEnteredDateTime);

                            tradeItem.ordDt = Tz.Tools.DateToStringISO(orderEnteredDateTimeValue);
                            tradeItem.ordTm = Tz.Tools.TimeToStringISO(orderEnteredDateTimeValue);
                        }

                        tradeItem.bizDt = DtFunc.DateTimeToStringDateISO(product.ClearingBusinessDate);

                        // FI 20170116 [21916] Alimentation de idAsset/assetCategory 
                        Pair<Cst.UnderlyingAsset, int> asset = product.GetUnderlyingAsset(CSTools.SetCacheOn(pCS));
                        if (null != asset)
                        {
                            tradeItem.assetCategoryIdSpecified = true;
                            tradeItem.assetCategory = asset.First.ToString();
                            tradeItem.idAssetSpecified = true;
                            tradeItem.idAsset = asset.Second;
                        }

                        if (product.ProductBase.IsFx)
                        {
                            if (product.ProductBase.IsFxLeg)
                            {
                                tradeItem.valDtSpecified = ((IFxLeg)product.Product).FxDateValueDateSpecified;
                                if (tradeItem.valDtSpecified)
                                    tradeItem.valDt = ((IFxLeg)product.Product).FxDateValueDate.Value;
                            }
                            else if (product.ProductBase.IsFxOption)
                            {
                                IFxOptionBase option = ((IFxOptionBase)product.Product);
                                tradeItem.valDtSpecified = true;
                                tradeItem.valDt = option.ValueDate.Value;

                                // FI 20150702 [XXXXX] Alimentation de stlDt
                                tradeItem.stlDtSpecified = option.FxOptionPremiumSpecified;
                                if (tradeItem.stlDtSpecified)
                                    tradeItem.stlDt = option.FxOptionPremium[0].PremiumSettlementDate.Value;
                            }
                        }
                        else if ((product.IsDebtSecurityTransaction) || (product.IsEquitySecurityTransaction))
                        {
                            // FI 20150623 [21149] GLOP (il faudrait plutôt lire la colonne TRADEINSTRUMENT.DTSETTLT) 
                            IPayment grossAmount = null;
                            if (product.IsEquitySecurityTransaction)
                                grossAmount = ((IEquitySecurityTransaction)product.Product).GrossAmount;
                            else if (product.IsDebtSecurityTransaction)
                                grossAmount = ((IDebtSecurityTransaction)product.Product).GrossAmount;

                            // FI 20150702 [XXXXX] Alimentation de stlDt
                            tradeItem.stlDtSpecified = grossAmount.PaymentDateSpecified;
                            if (tradeItem.stlDtSpecified)
                                tradeItem.stlDt = grossAmount.PaymentDate.UnadjustedDate.Value;
                        }
                        else if (product.ProductBase.IsCommoditySpot) //FI 20161214 [21916] Add CommoditySpot
                        {
                            CommoditySpotContainer commoditySpotContainer = new CommoditySpotContainer((ICommoditySpot)product.Product);

                            Nullable<DateTimeOffset> startDate = commoditySpotContainer.DeliveryStartDateTime;
                            Nullable<DateTimeOffset> endDate = commoditySpotContainer.DeliveryEndDateTime;
                            IPrevailingTime endTime = commoditySpotContainer.DeliveryEndTime;

                            tradeItem.dlvStartSpecified = startDate.HasValue;
                            if (tradeItem.dlvStartSpecified)
                                tradeItem.dlvStart = DtFunc.DateTimeToStringDateISO(commoditySpotContainer.DeliveryStartDateTime.Value.UtcDateTime);

                            tradeItem.dlvEndSpecified = endDate.HasValue;
                            if (tradeItem.dlvEndSpecified)
                                tradeItem.dlvEnd = DtFunc.DateTimeToStringDateISO(commoditySpotContainer.DeliveryEndDateTime.Value.UtcDateTime);

                            tradeItem.dlvTimezoneSpecified = (null != endTime) && StrFunc.IsFilled(endTime.Location.Value);
                            if (tradeItem.dlvTimezoneSpecified)
                                tradeItem.dlvTimezone = endTime.Location.Value;

                            IPayment grossAmount = commoditySpotContainer.CommoditySpot.FixedLeg.GrossAmount;
                            tradeItem.stlDtSpecified = grossAmount.PaymentDateSpecified;
                            if (tradeItem.stlDtSpecified)
                                tradeItem.stlDt = grossAmount.PaymentDate.UnadjustedDate.Value;
                        }
                        else
                        {
                            tradeItem.stlDtSpecified = false;
                            tradeItem.valDtSpecified = false;
                        }

                        tradeItem.frontIdSpecified = false;
                        tradeItem.folderIdSpecified = false;
                        tradeItem.utiSpecified = false;
                        tradeItem.orderIdSpecified = false;

                        // FI 20190515 [23912] Alimentation de orderId
                        tradeItem.orderIdSpecified = product.RptSide[0].OrderIdSpecified;
                        if (tradeItem.orderIdSpecified)
                            tradeItem.orderId = product.RptSide[0].OrderId;

                        // FI 20190515 [23912] Alimentation des traders
                        if (tradeDatadoc.CurrentTrade.TradeHeader.PartyTradeInformationSpecified)
                        {
                            List<TraderReport> traders = new List<TraderReport>();
                            foreach (IPartyTradeInformation item in tradeDatadoc.CurrentTrade.TradeHeader.PartyTradeInformation.Where(x => x.TraderSpecified))
                            {
                                foreach (ITrader traderItem in item.Trader)
                                {
                                    traders.Add(new TraderReport()
                                    {
                                        OTCmlId = traderItem.OTCmlId,
                                        // RD 20190917 [24947] Le trader n'est pas reconnu 
                                        //traderIdentifier = traderItem.name,
                                        traderIdentifier = traderItem.Identifier,
                                        partyReferenceSpecified = true,
                                        partyReference = new PartyReference(item.PartyReference)
                                    });
                                }
                            }
                            tradeItem.traderSpecified = (traders.Count > 0);
                            if (tradeItem.traderSpecified)
                                tradeItem.trader = traders;
                        }

                        IFixParty dealer = product.GetDealer();
                        if (null == dealer)
                            throw new Exception(StrFunc.AppendFormat("Trade :{0}, no dealer found", trade.TradeId));

                        IParty partyDealer = tradeDatadoc.GetParty(dealer.PartyId.href);
                        if (null == partyDealer)
                            throw new Exception(StrFunc.AppendFormat("Trade :{0}, party:{1} not found", trade.TradeId, dealer.PartyId.href));

                        // FI 20150115 [XXXXX] use partyDealer.id
                        IPartyTradeIdentifier tradeIdentifierDealer = tradeDatadoc.GetPartyTradeIdentifier(partyDealer.Id);
                        if (tradeIdentifierDealer != null)
                        {
                            IScheme frontIdScheme = Tools.GetScheme(tradeIdentifierDealer.TradeId, Cst.OTCml_FrontTradeIdScheme);
                            if (frontIdScheme != null)
                            {
                                tradeItem.frontIdSpecified = StrFunc.IsFilled(frontIdScheme.Value);
                                if (tradeItem.frontIdSpecified)
                                    tradeItem.frontId = frontIdScheme.Value;
                            }

                            if (tradeIdentifierDealer.LinkIdSpecified)
                            {
                                ILinkId[] linkId = tradeIdentifierDealer.LinkId;
                                if (ArrFunc.IsFilled(linkId))
                                {
                                    ILinkId folderId = (from ILinkId item in linkId
                                                        where item.LinkIdScheme == Cst.OTCml_FolderIdScheme
                                                        select item).FirstOrDefault();
                                    if (null != folderId)
                                    {
                                        tradeItem.folderIdSpecified = StrFunc.IsFilled(folderId.Value);
                                        if (tradeItem.folderIdSpecified)
                                            tradeItem.folderId = folderId.Value;
                                    }
                                }
                            }

                            if (null != tradeIdentifierDealer)
                            {
                                IScheme utiSchemeDealer = Tools.GetScheme(tradeIdentifierDealer.TradeId, Cst.OTCml_TradeIdUTISpheresScheme);
                                if (utiSchemeDealer != null)
                                {
                                    if (StrFunc.IsFilled(utiSchemeDealer.Value))
                                    {
                                        tradeItem.utiSpecified = true;
                                        tradeItem.uti.utiDealer = utiSchemeDealer.Value;
                                    }
                                }
                            }
                        }

                        IFixParty clearer = product.GetClearerCustodian();
                        if (null == clearer)
                            throw new Exception(StrFunc.AppendFormat("Trade :{0}, no clearer found ", trade.TradeId));

                        IParty partyClearer = tradeDatadoc.GetParty(clearer.PartyId.href);
                        if (null == partyClearer)
                            throw new Exception(StrFunc.AppendFormat("Trade :{0}, party:{1} not found ", trade.TradeId, clearer.PartyId.href));

                        // FI 20150115 [XXXXX] use partyClearer.id
                        IPartyTradeIdentifier tradeIdentifierClearer = tradeDatadoc.GetPartyTradeIdentifier(partyClearer.Id);
                        if (null != tradeIdentifierClearer)
                        {
                            IScheme utiSchemeClearer = Tools.GetScheme(tradeIdentifierClearer.TradeId, Cst.OTCml_TradeIdUTISpheresScheme);
                            if (utiSchemeClearer != null)
                            {
                                if (StrFunc.IsFilled(utiSchemeClearer.Value))
                                {
                                    tradeItem.utiSpecified = true;
                                    tradeItem.uti.utiClearer = utiSchemeClearer.Value;
                                }
                            }
                        }

                    }
                }
            }
        }


        /// <summary>
        ///  Alimentation des UTIs de type POSITION en s'appuyant sur les trades présents sous commonData.trade
        /// </summary>
        /// <param name="pNotificationType"></param>
        /// FI 20141218 [20574] Add Methode
        /// FI 20150113 [20672] Modify
        /// FI 20150427 [20987] Modify
        /// FI 20150709 [XXXXX] Modify
        /// EG 20180426 Analyse du code Correction [CA2202]
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void UpdCommonDataPositionUTI(string pCS)
        {
            if (_notificationDocument.CommonDataSpecified)
            {
                int[] idT = (from item in _notificationDocument.CommonData.trade
                             select item.OTCmlId).ToArray();

                if (ArrFunc.IsFilled(idT))
                {

                    string query = StrFunc.AppendFormat(@"select tr.IDT, puti.IDPOSUTI, putidealer.UTI as PUTI_DEALER, puticlearer.UTI as PUTI_CLEARER
                            from dbo.TRADE tr
                            inner join dbo.POSUTI puti on  puti.IDI=tr.IDI and puti.IDASSET = tr.IDASSET and tr.ASSETCATEGORY={1} and
                                                           puti.IDA_DEALER = tr.IDA_DEALER and puti.IDB_DEALER = tr.IDB_DEALER and
                                                           puti.IDA_CLEARER = tr.IDA_CLEARER and puti.IDB_CLEARER = tr.IDB_CLEARER
                            left outer join dbo.POSUTIDET putidealer on putidealer.IDPOSUTI = puti.IDPOSUTI and  putidealer.IDA = tr.IDA_DEALER and putidealer.UTISCHEME='{0}'
                            left outer join dbo.POSUTIDET puticlearer on puticlearer.IDPOSUTI = puti.IDPOSUTI and  puticlearer.IDA = tr.IDA_CLEARER and puticlearer.UTISCHEME='{0}'",
                    Cst.OTCml_TradeIdUTISpheresScheme, "'" + Cst.UnderlyingAsset.ExchangeTradedContract + "'");

                    query += GetJoinTrade(pCS, idT, "tr.IDT", false);

                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, query))
                    {
                        while (dr.Read())
                        {
                            int idTrade = Convert.ToInt32(dr["IDT"]);

                            CommonTradeAlloc trade =
                                    (from item in NotificationDocument.CommonData.trade
                                     where item.OTCmlId == idTrade
                                     select item).FirstOrDefault();

                            if (null == trade)
                                throw new NotSupportedException(StrFunc.AppendFormat("trade idT:{0} not found", idTrade.ToString()));

                            AddPosUTI(dr, _notificationDocument.CommonData, trade);
                        }
                    }
                }
            }

            // FI 20150113
            // FI 20150709 [XXXXX] appel de la méthode GetAllPositionSynthetic
            if ((_notificationDocument.EfsMLversion == EfsMLDocumentVersionEnum.Version35) &&
                _notificationDocument.PosSyntheticsSpecified || _notificationDocument.StlPosSyntheticsSpecified)
            {
                if (_notificationDocument.CommonData == null)
                    _notificationDocument.CommonData = new CommonData();
                _notificationDocument.CommonDataSpecified = true;

                // FI 20150427 [20987] use var poskey charché via un distinct
                var poskey =
                    (from posSynthetic in GetAllPositionSynthetic(null)
                     select new
                     {
                         posSynthetic.posKey.idA_Dealer,
                         posSynthetic.posKey.idB_Dealer,
                         posSynthetic.posKey.idA_Clearer,
                         posSynthetic.posKey.idB_Clearer,
                         posSynthetic.posKey.idI,
                         posSynthetic.posKey.idAsset,
                         posSynthetic.posKey.assetCategory
                     }).Distinct().ToList();

                StrBuilder query = new StrBuilder();
                foreach (var key in poskey)
                {
                    string queryItem = StrFunc.AppendFormat("select {0} as IDA_DEALER, {1} as IDB_DEALER, {2} as IDA_CLEARER, {3} as IDB_CLEARER, {4} as IDI, {5} as IDASSET, {6} as ASSETCATEGORY {7}",
                                    key.idA_Dealer, key.idB_Dealer, key.idA_Clearer, key.idB_Clearer,
                                    key.idI, key.idAsset, "'" + key.assetCategory + "'", DataHelper.SQLFromDual(pCS));

                    if (query.Length > 0)
                        query += StrFunc.AppendFormat("{0}union all{0}", Cst.CrLf);

                    query += queryItem;
                }


                String query2 = StrFunc.AppendFormat(@"select pos.IDA_DEALER, pos.IDB_DEALER, pos.IDA_CLEARER, pos.IDB_CLEARER, pos.IDI, 
                        pos.IDASSET, pos.ASSETCATEGORY, puti.IDPOSUTI, putidealer.UTI as PUTI_DEALER, puticlearer.UTI as PUTI_CLEARER 
                        from
                        (
                        {2}
                        )pos
                        inner join dbo.POSUTI puti on  puti.IDI=pos.IDI and puti.IDASSET = pos.IDASSET and pos.ASSETCATEGORY={1} and
                                                       puti.IDA_DEALER = pos.IDA_DEALER and puti.IDB_DEALER = pos.IDB_DEALER and
                                                       puti.IDA_CLEARER = pos.IDA_CLEARER and puti.IDB_CLEARER = pos.IDB_CLEARER

                        left outer join dbo.POSUTIDET putidealer on putidealer.IDPOSUTI = puti.IDPOSUTI and  putidealer.IDA = puti.IDA_DEALER and putidealer.UTISCHEME='{0}'
                        left outer join dbo.POSUTIDET puticlearer on puticlearer.IDPOSUTI = puti.IDPOSUTI and  puticlearer.IDA = puti.IDA_CLEARER and puticlearer.UTISCHEME='{0}'",
                Cst.OTCml_TradeIdUTISpheresScheme, "'" + Cst.UnderlyingAsset.ExchangeTradedContract + "'", query);

                using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, query2))
                {
                    while (dr.Read())
                    {
                        int idADealer = Convert.ToInt32(dr["IDA_DEALER"]);
                        int idBDealer = Convert.ToInt32(dr["IDB_DEALER"]);
                        int idAClearer = Convert.ToInt32(dr["IDA_CLEARER"]);
                        int idBClearer = Convert.ToInt32(dr["IDB_CLEARER"]);
                        int idI = Convert.ToInt32(dr["IDI"]);
                        int idAsset = Convert.ToInt32(dr["IDASSET"]);
                        string assetCategory = Convert.ToString(dr["ASSETCATEGORY"]);

                        List<PosSynthetic> pos =
                                    (from item in GetAllPositionSynthetic(null)
                                     where (item.posKey.idA_Dealer == idADealer && item.posKey.idB_Dealer == idBDealer &&
                                            item.posKey.idA_Clearer == idAClearer && item.posKey.idB_Clearer == idBClearer &&
                                            item.posKey.idI == idI && item.posKey.idAsset == idAsset && item.posKey.assetCategory.ToString() == assetCategory)
                                     select item).ToList();


                        if (null == pos)
                            throw new NotSupportedException(StrFunc.AppendFormat("position Dealer:({0},{1}), Clearer:({2},{3}), Asset:({4},{5},{6}) not found",
                                idADealer, idBDealer, idAClearer, idBClearer, idI, idAsset, assetCategory));

                        foreach (PosSynthetic item in pos)
                            AddPosUTI(dr, _notificationDocument.CommonData, item);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <param name="commonData"></param>
        /// <param name="positionUTIContainer"></param>
        /// FI 20150113 [20672] Add AddPosUTI
        private static void AddPosUTI<T>(IDataReader dr, CommonData commonData, T positionUTIContainer)
        {

            string putiDealer = (Convert.DBNull == dr["PUTI_DEALER"]) ? string.Empty : dr["PUTI_DEALER"].ToString();
            string putiClearer = (Convert.DBNull == dr["PUTI_CLEARER"]) ? string.Empty : dr["PUTI_CLEARER"].ToString();

            string hRef = StrFunc.AppendFormat("POSUTI.ID.{0}", dr["IDPOSUTI"].ToString());

            PositionUTIId puti = commonData.positionUTI.Find(item => item.id == hRef);
            if (null == puti)
            {

                puti = new PositionUTIId
                {
                    id = hRef
                };
                if (StrFunc.IsFilled(putiDealer))
                    puti.uti.utiDealer = putiDealer;
                if (StrFunc.IsFilled(putiClearer))
                    puti.uti.utiClearer = putiClearer;

                commonData.positionUTISpecified = true;
                commonData.positionUTI.Add(puti);
            }

            if (positionUTIContainer is CommonTradeAlloc)
            {
                CommonTradeAlloc trade = positionUTIContainer as CommonTradeAlloc;
                trade.positionUtiRefSpecified = true;
                trade.positionUtiRef.href = hRef;
            }
            else if (positionUTIContainer is PosSynthetic)
            {
                PosSynthetic pos = positionUTIContainer as PosSynthetic;
                pos.positionUtiRefSpecified = true;
                pos.positionUtiRef.href = hRef;
            }
            else
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", positionUTIContainer.GetType().ToString()));
        }

    
        /// <summary>
        /// Alimentation des frais et calcul du net lorsque nécessaire (produit EquitySecurtyTransaction). S'applique aux éléments
        /// <para>- NotificationDocument.trades.trade</para>
        /// <para>- NotificationDocument.unsettledTrades.trade</para>
        /// <para>- NotificationDocument.settledTrades.trade</para>
        /// </summary>
        /// <param name="pCS"></param>
        private void SetOppToTrades2(string pCS)
        {
            SetOppTrades(pCS);
            SetNetTrades();
        }

        /// <summary>
        /// Retourne la requête de chargement des FDA par position synthétique à la date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlRestrictTrade">Représente une restriction SQL de manière à ne considérer qu'une liste de trade</param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150127 [20275] Add Method
        /// FI 20150324 [XXPOC] Modify
        /// FI 20150617 [21124] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryPositionSyntheticFundingAmount(string pCS, string pSqlRestrictTrade, DateTime pDate)
        {
            // FI 20150324 [XXPOC] gestion du borrowing
            // FI 20150617 [21124] Utilisation de l'EVENTCLASS 'VAL'
            string query = StrFunc.AppendFormat(@"select posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY,
            posdet.SIDE,
            posdet.FDA_EVENTTYPE as EVENTTYPE,
            posdet.FDA_CURRENCY as CURRENCY,
            sum(case when posdet.FDA_IDB_PAY = posdet.IDB_DEALER then -1 * posdet.FDA_AMOUNT 
                     else case when posdet.FDA_IDB_REC = posdet.IDB_DEALER then posdet.FDA_AMOUNT 
                     else 0 end end) as AMOUNT
            from (
                select t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER,
                t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE, 
                fda.EVENTTYPE as FDA_EVENTTYPE,
                fda.UNIT as FDA_CURRENCY, fda.VALORISATION as FDA_AMOUNT, 
                fda.IDB_PAY as FDA_IDB_PAY, fda.IDB_REC as FDA_IDB_REC,
                t.IDENTIFIER as TRADEIDENTIFIER
                from dbo.TRADE t
                inner join dbo.EVENT umg on umg.IDT = t.IDT and umg.EVENTCODE='LPC' and umg.EVENTTYPE='UMG'
                inner join dbo.EVENTCLASS umgval on umgval.IDE = umg.IDE and umgval.EVENTCLASS = 'VAL' and umgval.DTEVENT = @DATE1
                inner join dbo.EVENT fda on fda.IDT = t.IDT and fda.EVENTCODE='LPP' and fda.EVENTTYPE in ('FDA','BWA')   
                inner join dbo.EVENTCLASS fdaval on fdaval.IDE = fda.IDE and fdaval.EVENTCLASS = 'VAL' and fdaval.DTEVENT = @DATE1
                {0}
            ) posdet
            group by    posdet.IDA_DEALER, posdet.IDB_DEALER, posdet.IDA_CLEARER, posdet.IDB_CLEARER, 
                        posdet.IDI, posdet.IDASSET, posdet.ASSETCATEGORY, 
                        posdet.SIDE, posdet.FDA_EVENTTYPE, posdet.FDA_CURRENCY", pSqlRestrictTrade);

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "DATE1", DbType.Date), pDate); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qry = new QueryParameters(pCS, query, dp);
            return qry;
        }

        /// <summary>
        ///  Retourne la requête qui permet d'obtenir les caractéristiques des remunérations FDA et BWA en date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlRestrictTrade">Représente une restriction SQL de manière à ne considérer qu'une liste de trade</param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150127 [20275] Add Method
        /// FI 20150326 [XXPOC] Modify
        /// FI 20150617 [21124] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryPositionSyntheticFundingAmountRate(string pCS, string pSqlRestrictTrade, DateTime pDate)
        {
            // FI 20150326 [XXPOC] Add BWA
            // FI 20150617 [21124] Utilisation de l'EVENTCLASS 'VAL'
            string query = StrFunc.AppendFormat(@"select t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER,
            t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE, 
            fda.EVENTTYPE, fda.UNIT,
            fdadet.DCF, fdadet.TOTALOFDAY, isnull(fdadet.RATE,0) as RATE, isnull(fdadet.SPREAD,0) as SPREAD
            from dbo.TRADE t
            inner join dbo.EVENT umg on umg.IDT = t.IDT and umg.EVENTCODE = 'LPC' and umg.EVENTTYPE = 'UMG'
            inner join dbo.EVENTCLASS umgval on umgval.IDE = umg.IDE and umgval.EVENTCLASS = 'VAL' and umgval.DTEVENT = @DATE1
            inner join dbo.EVENT fda on fda.IDT = t.IDT and fda.EVENTCODE = 'LPP' and fda.EVENTTYPE in ('FDA' , 'BWA')   
            inner join dbo.EVENTCLASS fdaval on fdaval.IDE = fda.IDE and fdaval.EVENTCLASS = 'VAL' and fdaval.DTEVENT = @DATE1
            inner join dbo.EVENTDET fdadet on  fdadet.IDE = fda.IDE               
            {0}", pSqlRestrictTrade);

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(pCS, "DATE1", DbType.Date), pDate); // FI 20201006 [XXXXX] DbType.Date

            QueryParameters qry = new QueryParameters(pCS, query, dp);
            return qry;
        }

        /// <summary>
        /// Alimentation des éléments fda et bwr
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQry"></param>
        /// <param name="pDate"></param>
        /// FI 20150127 [20275] Add Method
        /// FI 20150326 [XXPOC] Modify
        /// FI 20150427 [20987] Modify (ajout du paramètre Date)
        /// FI 20150710 [XXXXX] Modify
        // EG 20180426 Analyse du code Correction [CA2202]
        private void SetPosSyntheticFundingAmountFromQuery(string pCS, QueryParameters pQry, DateTime pDate)
        {
            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, pQry.Query, pQry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (NotificationDocument.PosSyntheticsSpecified == false)
                        throw new NotSupportedException("NotificationDocument.posSynthetics is not specified, call SetPositionSynthetic method");


                    // FI 20150427 [20987] Recherche des positions à la date pDate
                    PosSynthetics posSynthetics = NotificationDocument.PosSynthetics.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                    if (null == posSynthetics)
                        throw new NotSupportedException(StrFunc.AppendFormat("NotificationDocument.posSynthetics is not specified for date {0}", DtFunc.DateTimeToStringDateISO(pDate)));

                    // FI 20150710 [XXXXX] Ajout du side
                    PositionKey posKey = new PositionKey()
                    {
                        idA_Dealer = Convert.ToInt32(dr["IDA_DEALER"]),
                        idB_Dealer = Convert.ToInt32(dr["IDB_DEALER"]),
                        idA_Clearer = Convert.ToInt32(dr["IDA_CLEARER"]),
                        idB_Clearer = Convert.ToInt32(dr["IDB_CLEARER"]),
                        idI = Convert.ToInt32(dr["IDI"]),
                        idAsset = Convert.ToInt32(dr["IDASSET"]),
                        assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), Convert.ToString(dr["ASSETCATEGORY"]))
                    };
                    int side = Convert.ToInt32(dr["SIDE"]);

                    PosSynthetic posItem = posSynthetics.posSynthetic.Find(x => x.posKey.idA_Dealer == posKey.idA_Dealer &&
                                                                                       x.posKey.idB_Dealer == posKey.idB_Dealer &&
                                                                                       x.posKey.idA_Clearer == posKey.idA_Clearer &&
                                                                                       x.posKey.idB_Clearer == posKey.idB_Clearer &&
                                                                                       x.posKey.idI == posKey.idI &&
                                                                                       x.posKey.idAsset == posKey.idAsset &&
                                                                                       x.posKey.assetCategory == posKey.assetCategory &&
                                                                                       x.side == side);
                    if (null == posItem)
                        throw new NotSupportedException(StrFunc.AppendFormat("position Dealer:({0},{1}), Clearer:({2},{3}), Asset:({4},{5},{6}), Side:({7})  not found",
                            posKey.idA_Dealer, posKey.idB_Dealer, posKey.idA_Clearer, posKey.idB_Clearer, posKey.idI, posKey.idAsset, posKey.assetCategory, side.ToString()));


                    string eventType = Convert.ToString(dr["EVENTTYPE"]);
                    ReportAmountSideFundingAmountBase fdabase = null;
                    switch (eventType)
                    {
                        case "FDA":
                            fdabase = new ReportAmountSideFundingAmount();
                            posItem.fdaSpecified = true;
                            posItem.fda.Add((ReportAmountSideFundingAmount)fdabase);
                            break;
                        case "BWA":
                            fdabase = new ReportAmountSideBorrowingAmount();
                            posItem.bwaSpecified = true;
                            posItem.bwa.Add((ReportAmountSideBorrowingAmount)fdabase);
                            break;
                        default:
                            throw new NotImplementedException(StrFunc.AppendFormat("eventType:{0} is not implemented", eventType));
                    }

                    decimal amount = Convert.ToDecimal(dr["AMOUNT"]);
                    string currency = Convert.ToString(dr["CURRENCY"]);
                    fdabase.amount = System.Math.Abs(amount);
                    fdabase.currency = currency;
                    fdabase.sideSpecified = (fdabase.amount > 0);
                    if (fdabase.sideSpecified)
                    {
                        // FI 20150427 [20987] Add Montant arrondi pour avoir le même comportement que le cashBalance
                        EFS_Cash cash = new EFS_Cash(pCS, fdabase.amount, fdabase.currency);
                        fdabase.amount = cash.AmountRounded;

                        if (amount > 0)
                            fdabase.side = CrDrEnum.CR;
                        else
                            fdabase.side = CrDrEnum.DR;
                    }
                }
            }
        }

        /// <summary>
        ///  Alimentation des funding et borrowing Amount en date {pDate} pris en compte dans le cashBalance {pIdTCashBalance} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20150127 [20275] Add Method
        /// FI 20150324 [XXPOC] Modify
        /// FI 20150427 [20987] Modify  
        private void SetPositionSyntheticFundingAmountReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate)
        {
            Boolean isFunding = productEnv.ExistFunding();

            if (isFunding)
            {
                string sqlRestictTrade = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = t.IDT 
                                            and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT";

                //FDA and BWA
                QueryParameters qry = GetQueryPositionSyntheticFundingAmount(pCS, sqlRestictTrade, pDate);
                qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                SetPosSyntheticFundingAmountFromQuery(pCS, qry, pDate);

                // Alimentation du détail => éléments fdr et bwr 
                if (NotificationDocument.PosSyntheticsSpecified)
                {
                    PosSynthetics posSynthetics = NotificationDocument.PosSynthetics.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                    if ((null != posSynthetics) && (null != posSynthetics.posSynthetic.Find(x => x.fdaSpecified || x.bwaSpecified)))
                    {
                        qry = GetQueryPositionSyntheticFundingAmountRate(pCS, sqlRestictTrade, pDate);
                        qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);
                        SetPosSyntheticFundingAmountRateFromQuery(pCS, qry, posSynthetics.posSynthetic);
                    }
                }
            }
        }

        /// <summary>
        ///  Alimentation des éléments fdr (FundingRate) et bwr (BorrowingRate) 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQry">Requête qui retourne les caractéristiques des rémunération FDA et BWA pour une date donnée</param>
        /// <param name="pPosSynthetics">Représente les positions synthétiques pour une date donnée</param>
        /// FI 20150324 [XXPOC] Modify
        /// FI 20150427 [20987] Modify  
        private static void SetPosSyntheticFundingAmountRateFromQuery(string pCS, QueryParameters pQry, List<PosSynthetic> pPosSynthetics)
        {
            if (null == pPosSynthetics)
                throw new ArgumentNullException("Argument pPosSynthetics is null");

            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, pQry.Query, pQry.Parameters.GetArrayDbParameter());

            foreach (PosSynthetic posSynth in pPosSynthetics.Where(x => x.fdaSpecified || x.bwaSpecified))
            {
                for (int i = 0; i < 2; i++)
                {
                    string eventType = string.Empty;
                    ReportAmountSideFundingAmountBase[] lst = null;
                    switch (i)
                    {
                        case 0:
                            lst = posSynth.fdaSpecified ? posSynth.fda.ToArray() : null;
                            eventType = "FDA";
                            break;
                        case 1:
                            lst = posSynth.bwaSpecified ? posSynth.bwa.ToArray() : null;
                            eventType = "BWA";
                            break;
                        default:
                            throw new NotSupportedException("Not supported exception");
                    }

                    if (ArrFunc.IsFilled(lst))
                    {
                        foreach (ReportAmountSideFundingAmountBase item in lst)
                        {
                            string filterExpression = StrFunc.AppendFormat("IDA_DEALER={0} and IDB_DEALER={1} and IDA_CLEARER={2} and IDB_CLEARER={3} and IDI={4} and IDASSET={5} and ASSETCATEGORY='{6}' and UNIT='{7}' and EVENTTYPE='{8}'",
                                         posSynth.posKey.idA_Dealer, posSynth.posKey.idB_Dealer,
                                         posSynth.posKey.idA_Clearer, posSynth.posKey.idB_Clearer,
                                         posSynth.posKey.idI, posSynth.posKey.idAsset, posSynth.posKey.assetCategory,
                                         item.currency, eventType);

                            DataRow[] dr = ds.Tables[0].Select(filterExpression);

                            // FI 20151019 [21317] Usage de la méthode StringToEnum.DayCountFraction
                            var res =
                                (from row in dr
                                 select new
                                 {
                                     rate = Convert.ToDecimal(row["RATE"]),
                                     spread = (Convert.DBNull != row["SPREAD"]) ? Convert.ToDecimal(row["SPREAD"]) : decimal.Zero,
                                     dcf = EfsML.Enum.Tools.StringToEnum.DayCountFraction(Convert.ToString(row["DCF"])),
                                     days = Convert.ToInt32(row["TOTALOFDAY"])
                                 }).Distinct();


                            if (res.Count() == 1)
                            {
                                var resRate = res.First();
                                InsterestRate rate = new InsterestRate()
                                {
                                    rate = resRate.rate,
                                    spread = resRate.spread,
                                    dcf = resRate.dcf,
                                    days = resRate.days
                                };


                                if (i == 0)
                                {
                                    ((ReportAmountSideFundingAmount)item).fdrSpecified = true;
                                    ((ReportAmountSideFundingAmount)item).Fdr = rate;
                                }
                                else if (i == 1)
                                {
                                    ((ReportAmountSideBorrowingAmount)item).bwrSpecified = true;
                                    ((ReportAmountSideBorrowingAmount)item).Bwr = rate;
                                }
                                else
                                    throw new NotSupportedException("Not supported exception");
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Alimentation des versements/retraits en date {pDate} pris en compte dans le cashBalance {pIdTCashBalance} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20150128 [20275] Add Method
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modity
        /// FI 20160229 [XXXXX] Modity
        /// FI 20160412 [22069] Modify
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private void SetCashPaymentReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate)
        {
            if (productEnv.Count() > 0) // FI 20160225 [XXXXX] Add Test, // FI 20160412 [22069] use method count()
            {
                if (productEnv.ExistCashPayment()) // add test 
                {
                    string sqlRestictTrade = @"inner join dbo.TRADELINK tlcb on (tlcb.IDT_B = t.IDT) and (tlcb.LINK = 'CashPaymentInCashBalance') and (tlcb.IDT_A = @IDT)";

                    //Dépots/Retraits et même dividend...etc
                    QueryParameters query = GetQueryTradeCashPayment(pCS, sqlRestictTrade, pDate);
                    query.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);
                    SetCashPaymentFromQuery(pCS, query, pDate);
                }

                // FI 20151019 [21317] Alimentation des coupons (En commentaire pour l'instant)
                bool isExistSEC = productEnv.ExistSEC();
                if (isExistSEC)
                {
                    string sqlRestictTrade = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = t.IDT 
                                            and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT";

                    QueryParameters query = GetQueryTradeCoupon(pCS, sqlRestictTrade, pDate);
                    query.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                    SetCashPaymentFromQuery(pCS, query, pDate);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlRestrictTrade">Représente une restriction SQL de manière à ne considérer qu'une liste de trade</param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150128 [20275] Add Method
        /// RD 20150205 [20275] 
        /// - Add VALDT 
        /// - use TRADEINSTRUMENT.TRDTYPE as PAYMENTTYPE isntead of XQuery
        /// FI 20151016 [21458] Modify
        /// FI 20151019 [21317] Modify
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryTradeCashPayment(string pCS, string pSqlRestrictTrade, DateTime pDate)
        {
            // FI 20151019 [21317] Ajout de la colonne EXISTDETAIL
            string query = StrFunc.AppendFormat(@"select t.IDT, t.IDENTIFIER, t.DTTRADE, t.DISPLAYNAME, t.DESCRIPTION,
            ta_cp.IDA, ta_cp.IDB,
            isnull(case when e_cp.IDA_PAY = ta_cp.IDA then +1 else -1 end * e_cp.VALORISATION, 0)  as AMOUNT,
            e_cp.UNIT as UNIT, e_cp.EVENTTYPE,
            ec_cp.DTEVENT as VALDT,
            t.TRDTYPE as PAYMENTTYPE,
            0 as EXISTDETAIL
            from dbo.TRADE t
            inner join dbo.INSTRUMENT i on (i.IDI=t.IDI)
            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.IDENTIFIER='cashPayment') 
            inner join dbo.EVENT e_cp on (e_cp.IDT=t.IDT) and (e_cp.EVENTCODE='STA') and (e_cp.IDSTACTIVATION='REGULAR')
            inner join dbo.EVENTCLASS ec_cp on (ec_cp.IDE=e_cp.IDE) and (ec_cp.EVENTCLASS='VAL') and (ec_cp.DTEVENT=@DT) 
            inner join dbo.TRADEACTOR ta_ent on (ta_ent.IDT=t.IDT) and (ta_ent.IDROLEACTOR='ENTITY')
            inner join dbo.TRADEACTOR ta_cp on (ta_cp.IDT=t.IDT) and (ta_cp.IDROLEACTOR='COUNTERPARTY') and (ta_cp.IDA!=ta_ent.IDA) 
            {0}
            where (t.IDSTACTIVATION='REGULAR')", pSqlRestrictTrade);


            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }


        /// <summary>
        /// Requête qui recherche les tombée de coupon sur trade DebtSecurityTransaction 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlRestrictTrade">Représente une restriction SQL de manière à ne considérer qu'une liste de trade</param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20151019 [21317] Add 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryTradeCoupon(string pCS, string pSqlRestrictTrade, DateTime pDate)
        {

            string query = StrFunc.AppendFormat(@"select t.IDT, t.IDENTIFIER,t.DISPLAYNAME, t.DESCRIPTION,
            t.IDA_DEALER as IDA, t.IDB_DEALER as IDB,
            case when e.IDA_PAY = t.IDA_DEALER then -1 else +1 end * e.VALORISATION  as AMOUNT,
            e.UNIT as UNIT, e.EVENTTYPE,
            ecrec.DTEVENT as DTTRADE, ecval.DTEVENT as VALDT,
            'Coupon' as PAYMENTTYPE,
            1 as EXISTDETAIL, 
            edet.DAILYQUANTITY as QTY, 
            t.IDASSET, t.ASSETCATEGORY,
            edet.DAILYQUANTITY,edet.DCF, edet.TOTALOFDAY, edet.RATE as RATE, edet.SPREAD as SPREAD
            from dbo.TRADE t
            inner join dbo.INSTRUMENT i on (i.IDI=t.IDI)
            inner join dbo.PRODUCT p on (p.IDP=i.IDP) and (p.IDENTIFIER = 'DebtSecurityTransaction')
            inner join dbo.EVENT e on (e.IDT=t.IDT) and (e.EVENTCODE='INT') and (e.EVENTTYPE='INT')
            inner join dbo.EVENTCLASS ecval on (ecval.IDE=e.IDE) and (ecval.EVENTCLASS='VAL') and (ecval.DTEVENT=@DT)
            inner join dbo.EVENTCLASS ecrec on (ecrec.IDE=e.IDE) and (ecrec.EVENTCLASS='REC')
            inner join dbo.EVENTDET edet on edet.IDE = e.IDE
            {0}", pSqlRestrictTrade);

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQry"></param>
        /// <param name="pDate"></param>
        /// FI 20150128 [20275] Add Method
        /// RD 20150205 [20275] Add VALDT
        /// FI 20150427 [20987] Modify
        /// FI 20151016 [21458] Modify
        /// FI 20151019 [21317] Modify
        private void SetCashPaymentFromQuery(string pCS, QueryParameters pQry, DateTime pDate)
        {
            List<CashPayment> lstCashPayment = new List<CashPayment>();
            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
            CashPayments cashPayments = NotificationDocument.CashPayments.Find(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate));
            CashPayment cashPayment = null;

            IDataReader dr = null;
            try
            {
                dr = DataHelper.ExecuteReader(pCS, CommandType.Text, pQry.Query, pQry.Parameters.GetArrayDbParameter());
                while (dr.Read())
                {
                    if (cashPayments != null)
                    {
                        cashPayment = cashPayments.cashPayment.Find(x => x.OTCmlId == Convert.ToInt32(dr["IDT"]));
                        if (cashPayment != null)
                            continue;
                    }

                    //CashPayment cashPayment = new CashPayment();
                    decimal amount = Convert.ToDecimal(dr["AMOUNT"]);
                    string currency = Convert.ToString(dr["UNIT"]);

                    cashPayment = new CashPayment
                    {
                        OTCmlId = Convert.ToInt32(dr["IDT"]),
                        tradeIdentifier = Convert.ToString(dr["IDENTIFIER"]),
                        displayNameSpecified = (Convert.DBNull != dr["DISPLAYNAME"]),
                        descriptionSpecified = (Convert.DBNull != dr["DESCRIPTION"]),
                        ida = Convert.ToInt32(dr["IDA"]),
                        idb = Convert.ToInt32(dr["IDB"]),
                        trdDt = DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(dr["DTTRADE"])),
                        valDt = DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(dr["VALDT"])),
                        paymentTypeSpecified = (Convert.DBNull != dr["PAYMENTTYPE"]),
                        eventType = Convert.ToString(dr["EVENTTYPE"]),
                        amount = System.Math.Abs(amount),
                        currency = currency,
                        detailSpecified = BoolFunc.IsTrue(dr["EXISTDETAIL"])
                    };

                    // FI 20151016 [21458] add displayName
                    if (cashPayment.displayNameSpecified)
                        cashPayment.displayName = Convert.ToString(dr["DISPLAYNAME"]);
                    // FI 20151016 [21458] add description
                    if (cashPayment.descriptionSpecified)
                        cashPayment.description = Convert.ToString(dr["DESCRIPTION"]);
                    if (cashPayment.paymentTypeSpecified)
                        cashPayment.paymentType = Convert.ToString(dr["PAYMENTTYPE"]);

                    cashPayment.sideSpecified = (cashPayment.amount > 0);
                    if (cashPayment.sideSpecified)
                    {
                        if (amount > 0)
                            cashPayment.side = CrDrEnum.CR;
                        else
                            cashPayment.side = CrDrEnum.DR;
                    }

                    // FI 20151019 [21317] Alimentation de cashPayment.detail
                    if (cashPayment.detailSpecified)
                    {
                        cashPayment.detail = new CashPaymentDet
                        {
                            qtySpecified = (Convert.DBNull != dr["QTY"]),
                            idAssetSpecified = (Convert.DBNull != dr["IDASSET"]),
                            assetCategorySpecified = (Convert.DBNull != dr["ASSETCATEGORY"]),
                            cprSpecified = (Convert.DBNull != dr["RATE"])
                        };

                        // EG 20170127 Qty Long To Decimal
                        if (cashPayment.detail.qtySpecified)
                            cashPayment.detail.qty = Convert.ToDecimal(dr["QTY"]);
                        if (cashPayment.detail.idAssetSpecified)
                            cashPayment.detail.idAsset = Convert.ToInt32(dr["IDASSET"]);
                        if (cashPayment.detail.assetCategorySpecified)
                            cashPayment.detail.assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), dr["ASSETCATEGORY"].ToString());

                        if (cashPayment.detail.cprSpecified)
                        {
                            cashPayment.detail.cpr = new InsterestRate()
                            {
                                rate = Convert.ToDecimal(dr["RATE"]),
                                spread = (Convert.DBNull != dr["SPREAD"]) ? Convert.ToDecimal(dr["SPREAD"]) : decimal.Zero,
                                dcf = EfsML.Enum.Tools.StringToEnum.DayCountFraction(Convert.ToString(dr["DCF"])),
                                days = Convert.ToInt32(dr["TOTALOFDAY"])
                            };
                        }
                    }

                    lstCashPayment.Add(cashPayment);
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    dr.Close();
                    dr.Dispose();
                }
            }

            if (false == NotificationDocument.CashPaymentsSpecified)
                NotificationDocument.CashPaymentsSpecified = ArrFunc.IsFilled(lstCashPayment);

            if (ArrFunc.IsFilled(lstCashPayment))
            {
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                //NotificationDocument.cashPayments.Add(new CashPayments() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), cashPayment = lstCashPayment });
                if (cashPayments == null)
                {
                    cashPayments = new CashPayments() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), cashPayment = lstCashPayment };
                    NotificationDocument.CashPayments.Add(cashPayments);
                }
                else
                    cashPayments.cashPayment.AddRange(lstCashPayment);
            }
        }


        /// <summary>
        ///  Calcul des cours moyens à l'achat et la vente avec l'arrivée du trade  {pTrade} 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCS"></param>
        /// <param name="subTotal"></param>
        /// <param name="pTrade"></param>
        /// FI 20150218 [20275] Add Method
        /// FI 20150313 [XXXXX] Modify
        /// FI 20150331 [XXPOC] Modify (use PosTradeCommon)
        private static void CalcSubTotal(string pCS, PositionSubTotal subTotal, PosTradeCommon pTrade)
        {
            if (pTrade.idAssetSpecified == false)
                throw new NullReferenceException("asset is not specified");
            if (pTrade.assetCategorySpecified == false)
                throw new NullReferenceException("asset category is not specified");
            if (pTrade.qtySpecified == false)
                throw new NullReferenceException("qty is not specified");
            if (pTrade.priceSpecified == false)
                throw new NullReferenceException("price is not specified");

            Pair<Cst.UnderlyingAsset, int> asset = new Pair<Cst.UnderlyingAsset, int>(pTrade.assetCategory, pTrade.idAsset);
            SideEnum buySell = pTrade.side;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            decimal qty = pTrade.qty;
            Decimal price = pTrade.price;

            int instrumentDen = 100;
            if (asset.First == Cst.UnderlyingAsset.ExchangeTradedContract)
            {
                // FI 20200520 [XXXXX] Add SQL cache
                SQL_AssetETD assetETD = new SQL_AssetETD(CSTools.SetCacheOn(pCS), asset.Second);
                if (false == assetETD.LoadTable(new string[] { "INSTRUMENTDEN" }))
                    throw new Exception(StrFunc.AppendFormat("Asset ETD (id:{0}) doesn't exist", asset.Second));
                instrumentDen = assetETD.DrvContract_InstrumentDen;
            }

            subTotal.builder.EvaluateWeightedAverage(price, qty, instrumentDen, buySell);

            switch (buySell)
            {
                case SideEnum.Buy:
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    subTotal.@long.qty = Convert.ToDecimal(subTotal.builder.LongAveragePrice.Quantity);
                    // FI 20150313 [XXXXX] Spheres n'arrondi plus les prix selon les règles de la devise (9 decimal max)
                    // Usage du format {0:0.#########}" pour que avgPx de type decimal soit serializer avec 9 décimal. 
                    // Standard retenue du fait que la colonne TRAINSTRUMENT.PRICE est en UT_PRICE (9 decimal)  
                    subTotal.@long.avgPx = Convert.ToDecimal(String.Format("{0:0.#########}", subTotal.builder.LongAveragePrice.PriceBase100));
                    // FI 20150417 [XXXXX] Appel de la méthode AssetTools.GetReportFmtPrice
                    subTotal.@long.fmtAvgPx = BuildFmtPrice(pCS, asset, subTotal.@long.avgPx, 100);

                    break;

                case SideEnum.Sell:
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    subTotal.@short.qty = Convert.ToDecimal(subTotal.builder.ShortAveragePrice.Quantity);
                    // FI 20150315 [XXXXX] Spheres n'arrondi plus les prix selon les règles de la devise (9 decimal max)
                    // Usage du format {0:0.#########}" pour que avgPx de type decimal soit serializer avec 9 décimal. 
                    // Standard retenue du fait que la colonne TRAINSTRUMENT.PRICE est en UT_PRICE (9 decimal)  
                    subTotal.@short.avgPx = Convert.ToDecimal(String.Format("{0:0.#########}", subTotal.builder.ShortAveragePrice.PriceBase100));
                    // FI 20150417 [XXXXX] Appel de la méthode AssetTools.GetReportFmtPrice
                    subTotal.@short.fmtAvgPx = BuildFmtPrice(pCS, asset, subTotal.@short.avgPx, 100);
                    break;
            }
        }

        /// <summary>
        /// Retourne true si le trade  {pTrade} match avec restrictions spécifées ds predicate
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        /// FI 20150218 [20275] Add Method
        private static Boolean IsTradeMatch(TradeReport trade, Predicate[] predicate)
        {
            if (ArrFunc.IsEmpty(predicate))
                throw new ArgumentNullException("arg :predicate is null");

            Boolean ret = false;
            ret = predicate.Where(x => x.trdType == trade.trdType).Count() > 0;
            return ret;
        }

        /// <summary>
        /// Alimentation des EVENTYPE 'RMG','SCU','PRM' rattachés aux actions sur positions 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="posActions"></param>
        /// <param name="pDate"></param>
        /// FI 20150218 [20275] Add Method
        /// FI 20150623 [21149] Modify
        /// FI 20150623 [21149] Modify 
        /// FI 20150825 [21287] Modify de signature (Utilisation d'une liste de PosAction)
        /// FI 20150827 [21287] Modify
        /// FI 20151019 [21317] Modify 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static void SetPosActionEventType(string pCS, List<PosAction> posActions)
        {

            if (ArrFunc.IsFilled(posActions))
            {
                // FI 20150827 [21287] add GAM (Restitution du GAM lorsque POC ou POT)
                // FI 20151019 [21317] add AIN et PAM (Restitution du AIN et PAM lorsque POC ou POT (composant du GAM))
                string query = StrFunc.AppendFormat(@"select epadet.IDPADET, e.IDE, e.IDT, e.EVENTTYPE,
                e.VALORISATION as AMOUNT, e.UNIT as UNIT, 
                case when e.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as SIDE
                from dbo.EVENT e
                #RESTRICT_POSACTIONDET#
                inner join dbo.TRADE t on t.IDT = e.IDT
                where e.EVENTTYPE in ('RMG','SCU','PRM', 'GAM','AIN','PAM')
                order by epadet.IDPADET, e.IDE");

                // FI 20130625 peut-être faut-il eviter l'usage du in et charger toutes les actions à cette date
                // et faire la restriction sur le jeu de résultat avec LINQ ???
                string innerJoin = GetJoinEventPosActionDet(pCS, (from item in posActions select (IPosactionTradeFee)item).ToList());
                query = query.Replace("#RESTRICT_POSACTIONDET#", innerJoin);

                DataParameters dp = new DataParameters();
                // FI 20150623 [21149] Mise en commentaire
                //dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);

                QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                if (ArrFunc.IsFilled(dt.Rows))
                {
                    foreach (PosAction posAction in posActions)
                    {
                        DataRow[] rows = dt.Select(StrFunc.AppendFormat("IDPADET = {0} and IDT = {1}", posAction.IdPosActionDet, posAction.trades.trade.OTCmlId));
                        ReportAmountSide amount = new ReportAmountSide();
                        if (ArrFunc.IsFilled(rows))
                        {
                            foreach (DataRow row in rows)
                            {
                                EventTypeEnum eventType = (EventTypeEnum)System.Enum.Parse(typeof(EventTypeEnum), Convert.ToString(row["EVENTTYPE"]));
                                switch (eventType)
                                {
                                    case EventTypeEnum.RMG:
                                    case EventTypeEnum.PRM:
                                    case EventTypeEnum.SCU:
                                    case EventTypeEnum.GAM:
                                    case EventTypeEnum.AIN:
                                    case EventTypeEnum.PAM:
                                        amount = new ReportAmountSide()
                                        {
                                            amount = Convert.ToDecimal(row["AMOUNT"]),
                                            currency = Convert.ToString(row["UNIT"]),
                                            side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(row["SIDE"])),
                                            sideSpecified = (Convert.ToDecimal(row["AMOUNT"]) > 0)
                                        };
                                        break;
                                    default:
                                        throw new NotImplementedException(StrFunc.AppendFormat("EVENTTYPE:{0} is not implemented", eventType.ToString()));
                                }

                                switch (eventType)
                                {
                                    case EventTypeEnum.RMG:
                                        posAction.rmgSpecified = true;
                                        if (posAction.rmgSpecified)
                                            posAction.rmg = amount;
                                        break;
                                    case EventTypeEnum.SCU:
                                        posAction.scuSpecified = true;
                                        if (posAction.scuSpecified)
                                            posAction.scu = amount;
                                        break;
                                    case EventTypeEnum.PRM:
                                        posAction.prmSpecified = true;
                                        if (posAction.prmSpecified)
                                            posAction.prm = amount;
                                        break;
                                    case EventTypeEnum.GAM:
                                        posAction.gamSpecified = true;
                                        if (posAction.gamSpecified)
                                            posAction.gam = amount;
                                        break;
                                    case EventTypeEnum.AIN:
                                        posAction.ainSpecified = true;
                                        if (posAction.ainSpecified)
                                            posAction.ain = amount;
                                        break;
                                    case EventTypeEnum.PAM:
                                        posAction.pamSpecified = true;
                                        if (posAction.pamSpecified)
                                            posAction.pam = amount;
                                        break;

                                    default:
                                        throw new NotImplementedException(StrFunc.AppendFormat("EVENTTYPE:{0} is not implemented", eventType.ToString()));
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Retourne un nouvel objet ContractSimplePaymentConstituentReport à patir de ContractSimplePaymentConstituent 
        /// </summary>
        /// <param name="pPayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20150320 [XXPOC] Add Method
        private static ContractSimplePaymentConstituentReport NewContractSimplePaymentConstituentReport(ContractSimplePaymentConstituent pPayment, IReference pCashBalanceOffice)
        {
            ContractSimplePaymentConstituentReport ret = new ContractSimplePaymentConstituentReport();

            CopyContractSimplePaymentReport(ret, pPayment, pCashBalanceOffice);

            ret.optionSpecified = pPayment.optionSpecified;
            if (ret.optionSpecified)
            {
                ret.option = new OptionMarginConstituentReport[ArrFunc.Count(pPayment.option)];
                for (int i = 0; i < ArrFunc.Count(pPayment.option); i++)
                {
                    ret.option[i] = new OptionMarginConstituentReport();
                    CopyCashPositionToReport(ret.option[i], pPayment.option[i], pCashBalanceOffice);
                    ret.option[i].valuationMethod = pPayment.option[i].valuationMethod;
                }
            }

            ret.otherSpecified = pPayment.otherSpecified;
            if (ret.otherSpecified)
                ret.other = NewCashPositionReport(pPayment.other, pCashBalanceOffice);

            return ret;
        }

        /// <summary>
        /// Copie ds ContractSimplePaymentReport à patir d'un ContractSimplePayment 
        /// </summary>
        /// <param name="pPaymentReport"></param>
        /// <param name="pPayment"></param>
        /// <param name="pCashBalanceOffice"></param>
        /// <returns></returns>
        /// FI 20150320 [XXPOC] Add Method
        private static void CopyContractSimplePaymentReport(ContractSimplePaymentReport pPaymentReport, ContractSimplePayment pPayment, IReference pCashBalanceOffice)
        {
            pPaymentReport.amount = pPayment.paymentAmount.Amount.DecValue;
            pPaymentReport.currency = pPayment.paymentAmount.Currency;

            pPaymentReport.side = GetSideCashBalance(pPayment.receiverPartyReference, pCashBalanceOffice);
            //FI 20141208 [XXXXX] alimentation de sideSpecified
            pPaymentReport.sideSpecified = (pPaymentReport.amount > 0);

            // FI 20150427 [20987] pPayment.paymentDate n'est pas renseigné sur les CB "fictif" périodique
            if ((null != pPayment.paymentDate) && pPayment.paymentDate.adjustedDateSpecified)
                pPaymentReport.paymentDate = pPayment.paymentDate.adjustedDate.Value;

            pPaymentReport.detailSpecified = pPayment.detailSpecified;
            if (pPaymentReport.detailSpecified)
            {
                pPaymentReport.detail = new ContractAmountReport[ArrFunc.Count(pPayment.detail)];
                for (int i = 0; i < ArrFunc.Count(pPayment.detail); i++)
                {
                    pPaymentReport.detail[i] = new ContractAmountReport();
                    // FI 20150122 [XXXXX] Mise en commentaire CopyAmountSideToReport est appelé ds CopyContractAmountToReport  
                    //CopyAmountSideToReport(ret.detail[i], pPayment.detail[i], ret.currency);
                    CopyContractAmountToReport(pPaymentReport.detail[i], pPayment.detail[i], pPaymentReport.currency);
                }
            }
        }


        /// <summary>
        /// Mise à jour des champs specified à false de manière à serializer uniquement le stricte nécessaire 
        /// </summary>
        /// FI 20150331 [XXPOC] Add method
        /// FI 20150427 [20987] Modify
        /// FI 20150623 [21149] Modify
        /// FI 20150825 [21287] Modify
        /// FI 20170217 [22862] Modify
        private void SetUnspecifiedField()
        {
            // FI 20150623 [21149] Appel à GetAllTrades
            var trade = from item in GetAllTrades(null)
                        select new
                        {
                            key = "trade",
                            posTradeCommon = (PosTradeCommon)item
                        };

            // FI 20150623 [21149] Appel à GetAllPosTrades
            // FI 20170217 [22862] Add  DeliveryTrade
            var posTrade =
                from item in
                    GetAllPosTrades(null).Cast<PosTradeCommon>().Concat(
                        from item in
                            GetDeliveryTrade(null).Cast<PosTradeCommon>()
                        select item)
                select new
                {
                    key = "posTrade",
                    posTradeCommon = (PosTradeCommon)item
                };


            foreach (var item in trade.Concat(posTrade))
            {
                item.posTradeCommon.gProductSpecified = false;
                item.posTradeCommon.familySpecified = false;
                item.posTradeCommon.fungibilityModeSpecified = false;
                item.posTradeCommon.idISpecified = false;
                item.posTradeCommon.idAssetSpecified = false;
                item.posTradeCommon.assetCategorySpecified = false;
                item.posTradeCommon.priceSpecified = false;
                item.posTradeCommon.idbSpecified = false;
                item.posTradeCommon.sideSpecified = false;
                item.posTradeCommon.dtOutSpecified = false;

                switch (item.key)
                {
                    case "trade":
                        // FI 20150825 [21287] Mise en commentaire la qté est maintenant serilizée 
                        // Sur les unSettlementTrade et settlementTrade la qté est différente de la qté négociée s'il existe des transferts et/ou corections
                        // ds commondataTrade, il y a la qté négociée
                        //item.posTradeCommon.qtySpecified = false; //=> pas de serilization de la qté. Cette donnée est présente dans commondataTrade 
                        break;
                    case "posTrade":
                        if (item.posTradeCommon.gProduct == Cst.ProductGProduct_FX)
                            item.posTradeCommon.qtySpecified = false; //=> pas de qté si trades FX (rq: la colonne EVENTDET.DAILYQUANTITY est renseigné à 1 pour l'évènement "UMG")
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Key:{0} is not implemented", item.key));
                }
            }
        }



        /// <summary>
        ///  Mise à jour qui nécessite lecture des éléments présents dans repository
        /// </summary>
        /// FI 20150413 [20275] Add method
        /// FI 20150709 [XXXXX] Add method
        /// FI 20160530 [21885] Modify
        /// FI 20170201 [21916] Modify
        private void UpdateAfterRepository()
        {
            // FI 20190515 [23912] Les méthodes suivantes sont systématiquement appellées quelque que soit le type de Messagerie
            UpdateFundingAmountLabel();

            UpdateCashPaymentLabel();

            UpdateSavekeepingLabel();

            UpdateNominalInDebtSecurityTransaction();

            // FI 20160530 [21885] call UpdateCollateralLabel
            UpdateCollateralLabel();

            // FI 20160613 [22256] call UpdateUnderlyingStockLabel
            UpdateUnderlyingStockLabel();

            // FI 20170201 [21916] Call UpdatefmtQty();
            UpdatefmtQty();

        }

        /// <summary>
        /// Mise à jour en fonction des paramétrages présents dans CNFMESSAGEHEADFOOT
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDate2"></param>
        /// FI 20150522 [20275] add method
        /// FI 20150605 [XXXXX] Modify
        private void UpdateAfterReportSettings(DateTime pDate, Nullable<DateTime> pDate2)
        {
            if (NotificationDocument.Header.ReportSettingsSpecified)
            {
                HeaderFooter reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;
                UpdateExpiryIndicator(pDate, pDate2, reportSettings);
            }
        }

        /// <summary>
        ///  Mise à jour de l'indicateur de proximité d'échéance 
        /// </summary>
        /// <param name="pDate"></param>
        /// <param name="pDate2"></param>
        /// <param name="pReportSettings"></param>
        /// FI 20150522 [20275] add method
        /// FI 20150624 [XXXXX] Modify
        private void UpdateExpiryIndicator(DateTime pDate, Nullable<DateTime> pDate2, HeaderFooter pReportSettings)
        {
            if (pReportSettings.expiryIndicatorSpecified)
            {
                if (pReportSettings.expiryIndicator.daysSpecified)
                {
                    DateTime dtRef = pDate2 ?? pDate;
                    int days = pReportSettings.expiryIndicator.days;

                    foreach (CommonTradeAlloc trade in this.NotificationDocument.CommonData.trade.Where(x => x.expDtSpecified))
                    {
                        DateTime dtExp = DtFunc.ParseDate(trade.expDt, DtFunc.FmtISODate, null);
                        int dateDiff = (dtExp - dtRef).Days;
                        trade.expIndSpecified = dateDiff >= 0 && (dateDiff <= days);
                        if (trade.expIndSpecified)
                            trade.expInd = dateDiff; //FI 20150624 [XXXXX] Affichage de dateDiff
                    }
                    if (NotificationDocument.RepositorySpecified)
                    {
                        foreach (IAssetETDRepository asset in this.NotificationDocument.Repository.AssetETD.Where(x => x.MaturityDateSpecified))
                        {
                            DateTime dtExp = asset.MaturityDate;
                            int dateDiff = (dtExp - dtRef).Days;

                            asset.ExpIndSpecified = dateDiff >= 0 && dateDiff <= days;
                            if (asset.ExpIndSpecified)
                                asset.ExpInd = dateDiff; //FI 20150624 [XXXXX] Affichage de dateDiff
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mise à jour des Labels sur les FundingAmount
        /// </summary>
        /// FI 20150413 [20275] add method
        /// FI 20150427 [20987] Modify
        private void UpdateFundingAmountLabel()
        {
            HeaderFooter reportSettings = null;
            if (NotificationDocument.Header.ReportSettingsSpecified)
                reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;

            if ((reportSettings != null) && reportSettings.journalEntrySpecified && NotificationDocument.PosSyntheticsSpecified)
            {
                // FI 20150427 [20987] Use allPosSynthetics
                var allPosSynthetics = from posSynthetics in NotificationDocument.PosSynthetics
                                       from item in posSynthetics.posSynthetic
                                       select item;

                foreach (PosSynthetic item in allPosSynthetics.Where(x => (x.fdaSpecified || x.bwaSpecified)))
                {
                    var fda =
                        (from fdaItem in item.fda.Where(x => x.fdrSpecified)
                         select (ReportAmountSideFundingAmountBase)fdaItem).Concat(
                                from bwrItem in item.bwa.Where(x => x.bwrSpecified)
                                select (ReportAmountSideFundingAmountBase)bwrItem);

                    foreach (ReportAmountSideFundingAmountBase fdaItem in fda)
                    {
                        LabelDescription desc = null;

                        if (fdaItem is ReportAmountSideFundingAmount)
                            desc = reportSettings.journalEntry.Where(x => x.key == "FDA").FirstOrDefault();
                        else if (fdaItem is ReportAmountSideBorrowingAmount)
                            desc = reportSettings.journalEntry.Where(x => x.key == "BWA").FirstOrDefault();
                        else
                            throw new NotImplementedException(StrFunc.AppendFormat("type (name:{0}) is not implemented", fdaItem.GetType().ToString()));

                        if (null != desc)
                        {
                            IAssetRepository asset = RepositoryTools.LoadAssetRepository(NotificationDocument.Repository, new Pair<int, Cst.UnderlyingAsset>(item.IdAsset, item.AssetCategory));
                            if (null == asset)
                                throw new NotSupportedException(StrFunc.AppendFormat("Asset (id:{0}, cat:{0}) not found in repository", item.IdAsset, item.AssetCategory.ToString()));


                            CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(this.Culture);

                            string dcf = ReflectionTools.ConvertEnumToString<DayCountFractionEnum>(fdaItem.rate.dcf);
                            string pctRate = (fdaItem.rate.rate * 100).ToString("0.#0##########", cultureInfo); //2 décimales au minimum
                            string fmtQty = item.qty.ToString("n0", cultureInfo); //=>présence du séparateur de millier


                            string isinCode = string.Empty;
                            switch (item.AssetCategory)
                            {
                                case Cst.UnderlyingAsset.EquityAsset:
                                    isinCode = ((IAssetEquityRepository)asset).ISINCode;
                                    break;
                                case Cst.UnderlyingAsset.Bond:
                                    isinCode = ((IAssetDebtSecurityRepository)asset).ISINCode;
                                    break;
                                case Cst.UnderlyingAsset.ExchangeTradedContract:
                                    isinCode = ((IAssetETDRepository)asset).ISINCode;
                                    break;
                            }

                            string resDay = Ressource.GetString("Report-Day", cultureInfo).ToLower();
                            string resDays = Ressource.GetString("Report-Days", cultureInfo).ToLower();
                            fdaItem.label = StrFuncExtended.ReplaceObjectField(desc.description, new
                            {
                                SIDE = item.side.ToString(),
                                QTY = item.qty,
                                FMTQTY = fmtQty,
                                ASSET_CATEGORY = item.AssetCategory,
                                ASSET_IDENTIFIER = asset.Identifier,
                                ASSET_ALTIDENTIFIER = asset.AltIdentifier,
                                ASSET_DISPLAYNAME = asset.Displayname,
                                ASSET_DESCRIPTION = asset.Description,
                                ASSET_ISINCODE = isinCode,
                                RATE = fdaItem.rate.rate,
                                PCTRATE = pctRate,
                                DAYCOUNT = fdaItem.rate.days,
                                RESDAYCOUNT = StrFunc.AppendFormat("{0} {1}", fdaItem.rate.days.ToString(), (fdaItem.rate.days > 1) ? resDays : resDay),
                                DAYCOUNTFRACTION = dcf
                            }, cultureInfo);
                            fdaItem.labelSpecified = StrFunc.IsFilled(fdaItem.label);
                        }
                    }
                }
            }
        }




        /// <summary>
        /// Mise à jour des Labels sur les cashPayment
        /// </summary>
        /// FI 20150413 [20275] add method
        /// FI 20150427 [20987] Modify
        /// FI 20151019 [21317] Modify 
        private void UpdateCashPaymentLabel()
        {
            HeaderFooter reportSettings = null;
            if (NotificationDocument.Header.ReportSettingsSpecified)
                reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;

            if ((null != reportSettings) && reportSettings.journalEntrySpecified && NotificationDocument.CashPaymentsSpecified)
            {
                // FI 20150427 [20987] use allCashPayments
                var allCashPayments = from itemCashPayments in NotificationDocument.CashPayments
                                      from item in itemCashPayments.cashPayment
                                      select item;

                foreach (CashPayment item in allCashPayments)
                {
                    LabelDescription desc =
                        desc = reportSettings.journalEntry.Where(x => x.key == item.eventType).FirstOrDefault();
                    if (null == desc)
                        desc = reportSettings.journalEntry.Where(x => x.key == "***").FirstOrDefault();

                    if (null != desc)
                    {
                        CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(this.Culture);
                        string extType = string.Empty;
                        if (item.paymentTypeSpecified)
                        {
                            IEnumRepository enumRepository = RepositoryTools.LoadEnumValue(this.NotificationDocument.Repository, "PaymentType", item.paymentType);
                            if (null != enumRepository)
                                extType = enumRepository.ExtValue;
                        }
                        // FI 20151019 [21317] cas où detailSpecified
                        if (item.detailSpecified)
                        {
                            CashPaymentDet detail = item.detail;
                            #region asset
                            IAssetRepository asset = null;
                            string isinCode = string.Empty;
                            if (detail.idAssetSpecified)
                            {
                                asset = RepositoryTools.LoadAssetRepository(NotificationDocument.Repository, new Pair<int, Cst.UnderlyingAsset>(detail.idAsset, detail.assetCategory));
                                if (null == asset)
                                    throw new NotSupportedException(StrFunc.AppendFormat("Asset (id:{0}, cat:{0}) not found in repository", detail.idAsset, detail.assetCategory.ToString()));

                                switch (detail.assetCategory)
                                {
                                    case Cst.UnderlyingAsset.EquityAsset:
                                        isinCode = ((IAssetEquityRepository)asset).ISINCode;
                                        break;
                                    case Cst.UnderlyingAsset.Bond:
                                        isinCode = ((IAssetDebtSecurityRepository)asset).ISINCode;
                                        break;
                                    case Cst.UnderlyingAsset.ExchangeTradedContract:
                                        isinCode = ((IAssetETDRepository)asset).ISINCode;
                                        break;
                                }
                            }
                            #endregion
                            #region Qty
                            // EG 20170127 Qty Long To Decimal
                            decimal qty = 0;
                            string fmtQty = decimal.Zero.ToString();
                            if (detail.qtySpecified)
                            {
                                qty = item.detail.qty;
                                fmtQty = detail.qty.ToString("n0", cultureInfo); //=>présence du séparateur de millier
                            }
                            #endregion Qty
                            #region rate
                            string resDayCount = string.Empty;
                            string dcf = string.Empty;
                            decimal rate = decimal.Zero;
                            string pctRate = string.Empty;
                            int dayCount = 0;
                            if (detail.cprSpecified)
                            {
                                string resDay = Ressource.GetString("Report-Day", cultureInfo).ToLower();
                                string resDays = Ressource.GetString("Report-Days", cultureInfo).ToLower();

                                dcf = ReflectionTools.ConvertEnumToString<DayCountFractionEnum>(detail.cpr.dcf);
                                rate = detail.cpr.rate;
                                pctRate = (detail.cpr.rate * 100).ToString("0.#0##########", cultureInfo); //2 décimales au minimum
                                dayCount = detail.cpr.days;
                                resDayCount = StrFunc.AppendFormat("{0} {1}", dayCount.ToString(), (dayCount > 1) ? resDays : resDay);
                            }
                            #endregion

                            item.label = StrFuncExtended.ReplaceObjectField(desc.description, new
                            {
                                //COMMON
                                TRADE_IDENTIFIER = item.tradeIdentifier,
                                TRADE_DISPLAYNAME = item.displayName,
                                TRADE_DESCRIPTION = item.description,
                                TYPE = item.paymentType,
                                EXTTYPE = extType,
                                EVENTTYPE = item.eventType,
                                SIDE = item.side.ToString(),
                                //ASSET
                                ASSET_CATEGORY = (asset != null) ? asset.AssetCategory.ToString() : string.Empty,
                                ASSET_IDENTIFIER = (asset != null) ? asset.Identifier : string.Empty,
                                ASSET_ALTIDENTIFIER = (asset != null) ? asset.AltIdentifier : string.Empty,
                                ASSET_DISPLAYNAME = (asset != null) ? asset.Displayname : string.Empty,
                                ASSET_DESCRIPTION = (asset != null) ? asset.Description : string.Empty,
                                ASSET_ISINCODE = isinCode,
                                //QTY
                                QTY = qty,
                                FMTQTY = fmtQty,
                                //RATE
                                RATE = rate,
                                PCTRATE = pctRate,
                                DAYCOUNT = dayCount,
                                RESDAYCOUNT = resDayCount,
                                DAYCOUNTFRACTION = dcf
                            }, cultureInfo);
                        }
                        else
                        {
                            item.label = StrFuncExtended.ReplaceObjectField(desc.description, new
                            {
                                TRADE_IDENTIFIER = item.tradeIdentifier,
                                TRADE_DISPLAYNAME = item.displayName,
                                TRADE_DESCRIPTION = item.description,
                                TYPE = item.paymentType,
                                EXTTYPE = extType,
                                EVENTTYPE = item.eventType,
                                SIDE = item.side.ToString()
                            }, cultureInfo);
                        }

                        item.labelSpecified = StrFunc.IsFilled(item.label);
                    }
                }
            }
        }

        /// <summary>
        /// Mise en place d'un CashBalance fictif à partir des n cashBlances {pTradeRisk}
        /// <para>Affectation du membre _cashBalancePeriodic</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeRisk">Liste des trades CashBbalance trié par date</param>
        /// FI 20150427 [20987] Add
        /// FI 20150930 [21311] Modify
        /// FI 20160119 [21748] Modify
        /// FI 20160530 [21885] Modify 
        /// RD 20171212 [23633] Modify
        private void BuildCashBalancePeriodic(string pCS, List<SQL_TradeRisk> pTradeRisk)
        {
            // FI 20150930 [21311] Appel à LoadFlowsCashBalancePeriodic
            DataRow[] row = LoadFlowsCashBalancePeriodic(pCS, pTradeRisk);

            var actor = (from item in row
                         select new
                         {
                             idA_Risk = Convert.ToInt32(item["IDA_RISK"]),
                             idB_Risk = Convert.ToInt32(item["IDB_RISK"]),
                             idA_Entity = Convert.ToInt32(item["IDA_ENTITY"])
                         }).Distinct().ToList();

            if (actor.Count > 1)
                throw new NotSupportedException("several {IDA_RISK,IDB_RISK,IDA_ENTITY} found");

            int idARisk = actor[0].idA_Risk;
            SQL_Actor actorRisk = new SQL_Actor(pCS, idARisk);
            // RD 20171212 [23633] Add column TIMEZONE
            actorRisk.LoadTable(new string[] { "IDA, IDENTIFIER, DISPLAYNAME, DESCRIPTION, BIC, TIMEZONE" });

            int idBRisk = actor[0].idB_Risk;
            SQL_Book bookRisk = new SQL_Book(pCS, idBRisk);
            bookRisk.LoadTable(new string[] { "IDB, IDENTIFIER, DISPLAYNAME, DESCRIPTION" });

            //FI 20160119 [21748] Chgt de la devise de compta (Utilisé comme devise pivot si nécessaire)
            int idAEntity = actor[0].idA_Entity;
            SQL_Actor actorEntity = new SQL_Actor(pCS, idAEntity)
            {
                WithInfoEntity = true
            };
            // RD 20171212 [23633] Add column TIMEZONE
            actorEntity.LoadTable(new string[] { "ACTOR.IDA, ACTOR.IDENTIFIER, ACTOR.DISPLAYNAME, ACTOR.DESCRIPTION, ACTOR.BIC, ACTOR.TIMEZONE, ent.IDA as IDA1, isnull(ent.IDCACCOUNT, 'EUR')  as IDCACCOUNT" });
            if (false == actorEntity.IsEntityExist)
                throw new NotSupportedException(StrFunc.AppendFormat("Actor (id:{0}) is not an entity", idAEntity.ToString()));


            ITrade trade = new FpML.v44.Doc.Trade();
            trade.TradeIdSpecified = true;
            trade.TradeId = StrFunc.AppendFormat(StrFunc.AppendFormat("From{0}To{1}", pTradeRisk.First().Identifier, pTradeRisk.Last().Identifier));

            trade.Product = new CashBalance();
            trade.TradeHeader.TradeDate.Value = DtFunc.DateTimeToString(Convert.ToDateTime(pTradeRisk.Last().GetFirstRowColumnValue("DTBUSINESS")), DtFunc.FmtISODate);

            IDataDocument document = new EfsML.v30.Doc.EfsDocument();
            document.Trade = null;
            document.Party = null;

            DataDocumentContainer doc = new DataDocumentContainer(document);
            doc.AddTrade(trade);

            ProductContainer product = new ProductContainer(trade.Product, doc);
            Tools.SetProduct(pCS, product, pTradeRisk[0].IdI);

            CashBalance cashBalance = (CashBalance)product.Product;

            IParty partyActorRisk = doc.AddParty(actorRisk);
            IPartyTradeIdentifier partyActorTradeIdentifier = doc.AddPartyTradeIndentifier(partyActorRisk.Id, true);
            Tools.SetBookId(partyActorTradeIdentifier.BookId, bookRisk);
            partyActorTradeIdentifier.BookIdSpecified = true;

            IParty partyEntity = doc.AddParty(actorEntity);

            cashBalance.cashBalanceOfficePartyReference = new PartyReference(actorRisk.XmlId);
            cashBalance.entityPartyReference = new PartyReference(actorEntity.XmlId);
            cashBalance.timing = SettlSessIDEnum.EndOfDay;
            cashBalance.settings = LoadCashBalanceSetting(pCS, partyActorRisk.OTCmlId, partyActorRisk.Id);


            List<String> cur = (from item in row
                                select Convert.ToString(item["UNIT"])).Distinct().ToList();

            if (ArrFunc.IsEmpty(cur))
                throw new NotSupportedException("No currency flows");

            /* Mise en place des streams */
            List<CashBalanceStream> cbs = new List<CashBalanceStream>();
            foreach (string itemCur in cur)
            {
                cbs.Add(new CashBalanceStream());
                CashBalanceStream cbsItem = cbs.Last();

                DataRow[] rowcur = (from itemRow in row
                                    where (Convert.ToString(itemRow["UNIT"]) == itemCur)
                                    select itemRow).ToArray();

                BuidCashBalanceStream(pCS, cbsItem, itemCur, rowcur, partyActorRisk.Id, partyEntity.Id);

            }
            cashBalance.cashBalanceStream = cbs.ToArray();

            /* Mise en place de l'exchangeStream */
            if (cashBalance.settings.cashBalanceCurrencySpecified)
            {
                string IdCExchangeStream = cashBalance.settings.cashBalanceCurrency.Value;
                string idcPivot = actorEntity.IdCAccount;

                // FI 20150930 [21311] Appel à ConvertCashBalanceFlows
                // FI 20160119 [21748] Passage de la devise pivot
                cashBalance.exchangeCashBalanceStream = new ExchangeCashBalanceStream();
                cashBalance.exchangeCashBalanceStreamSpecified =
                        ConvertCashBalanceFlows(pCS, row, IdCExchangeStream, idcPivot, trade.TradeHeader.TradeDate.DateValue, 
                        out List<IdentifiedFxRate> fxRate,  _setErrorWarning);

                // FI 20150930 [21311] exchangeCashBalanceStream existe uniquement si les cours de change nécessaires sont présents
                if (cashBalance.exchangeCashBalanceStreamSpecified)
                {
                    BuidCashBalanceStream(pCS, cashBalance.exchangeCashBalanceStream, IdCExchangeStream, row, partyActorRisk.Id, partyEntity.Id);

                    cashBalance.fxRateSpecified = ArrFunc.IsFilled(fxRate);
                    if (cashBalance.fxRateSpecified)
                        cashBalance.fxRate = fxRate.ToArray();
                }
            }

            // FI 20160530 [21885] call BuildPosCollateral
            BuildCollateral(pCS, doc, pTradeRisk.Last().IdT);

            _cashBalancePeriodic = new Pair<DataDocumentContainer, List<SQL_TradeRisk>>(doc, pTradeRisk);
        }

        /// <summary>
        ///  Copie dans le document CashBalance {pDoc}, le détail des dépôts de garantie présent dans le trade CashBalance  {pIdT} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDoc">dataDocument CB </param>
        /// <param name="pIdT">Trade CB source </param>
        /// FI 20160530 [21885] Add
        // EG 20180426 Analyse du code Correction [CA2202]
        private static void BuildCollateral(string pCS, DataDocumentContainer pDoc, int pIdT)
        {
            if (!(pDoc.CurrentProduct.Product is CashBalance cashBalance))
                throw new NullReferenceException("Current product is not as CashBalance");

            // FI 20160530 [21885] => Cght du détail des dépôts de garantie présent sur le dernier trade CB 
            Dictionary<string, PosCollateral[]> collateral = LoadCollateral(pCS, pIdT);

            if (collateral.Keys.Count > 0)
            {
                List<string> lstCssHref = new List<string>();

                foreach (string currency in collateral.Keys)
                {
                    CashBalanceStream stream = cashBalance.cashBalanceStream.Where(x => x.currency.Value == currency).FirstOrDefault();
                    if (null == stream)
                        throw new NullReferenceException(StrFunc.AppendFormat("There is no stream with currency {0}", currency));

                    stream.collateralSpecified = ArrFunc.IsFilled(collateral[currency]);
                    if (stream.collateralSpecified)
                        stream.collateral = collateral[currency];


                    var cssHref = from collateralItem in stream.collateral
                                  from haircutItem in collateralItem.haircut.Where(x => x.cssHrefSpecified &&
                                                                (false == lstCssHref.Contains(x.cssHref)) &&
                                                                (null == pDoc.GetParty(x.cssHref, PartyInfoEnum.id)))
                                  select haircutItem.cssHref;

                    foreach (string item in cssHref)
                        lstCssHref.Add(item);
                }

                if (lstCssHref.Count > 0)
                {
                    // Il est supposé que qu'un acteur chambre n'a jamais un identifiant numerique, ni les caratères "+" et "/" 
                    string where = DataHelper.SQLColumnIn(pCS, "isnull(BIC,IDENTIFIER)", lstCssHref, TypeData.TypeDataEnum.@string);
                    string query = StrFunc.AppendFormat("select IDA, IDENTIFIER, BIC from dbo.ACTOR where {0}", where);
                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, query))
                    {
                        while (dr.Read())
                        {
                            SQL_Actor sqlActorCss = new SQL_Actor(CSTools.SetCacheOn(pCS), Convert.ToInt32(dr["IDA"]));
                            sqlActorCss.LoadTable();
                            pDoc.AddParty(sqlActorCss);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Chgt du détail du collateral présent sur un trade CashBalance 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">IDT du Trade CB</param>
        /// <returns></returns>
        // FI 20160530 [21885] Add
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // RD 20210304 Use trx.TRADEXML instead of t.TRADEXML            
        private static Dictionary<string, PosCollateral[]> LoadCollateral(string pCS, int pIdT)
        {
            Dictionary<string, PosCollateral[]> ret = new Dictionary<string, PosCollateral[]>();
            DbSvrType dbSvrType = (DataHelper.GetDbSvrType(pCS));
            string query;
            switch (dbSvrType)
            {
                case DbSvrType.dbSQL:
                    #region SQLSERVER
                    query = @"WITH XMLNAMESPACES 
                    (
                        'http://www.fpml.org/2007/FpML-4-4' as fpml,
                        'http://www.efs.org/2007/EFSmL-3-0' as efs
                    )
                    select e.UNIT as UNIT, 
                    trx.TRADEXML.query('efs:EfsML/fpml:trade/efs:cashBalance/efs:cashBalanceStream[./efs:currency/text()=sql:column(""e.UNIT"")]/efs:collaterals') as COLLATERAL
                    from dbo.TRADEXML trx
                    inner join dbo.EVENT e on (e.IDT = trx.IDT) and (e.EVENTCODE = 'CBS')
                    where (trx.IDT = @IDT)";
                    break;
                #endregion SQLSERVER
                case DbSvrType.dbORA:
                    #region ORACLE
                    query = @"select e.UNIT as UNIT,
                    EXTRACT(trx.TRADEXML,'efs:EfsML/fpml:trade/efs:cashBalance/efs:cashBalanceStream[./efs:currency/text()=''' || e.UNIT || ''']/efs:collaterals',
                    'xmlns:efs=""http://www.efs.org/2007/EFSmL-3-0"" xmlns:fpml=""http://www.fpml.org/2007/FpML-4-4""') as COLLATERAL
                    from dbo.TRADEXML trx
                    inner join dbo.EVENT e on (e.IDT = trx.IDT) and (e.EVENTCODE = 'CBS')
                    where (trx.IDT = @IDT)";
                    break;
                #endregion ORACLE
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RDBMS ({0}) not implemented", dbSvrType.ToString()));
            }
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdT);

            QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, qryParameters.Query.ToString(), qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    string cur = dr["UNIT"].ToString();
                    string xmlCollateral = Convert.IsDBNull(dr["COLLATERAL"]) ? null : dr["COLLATERAL"].ToString();

                    if (StrFunc.IsFilled(xmlCollateral))
                    {
                        System.IO.StringReader reader = new System.IO.StringReader(xmlCollateral);

                        System.Xml.Serialization.XmlRootAttribute root = new System.Xml.Serialization.XmlRootAttribute
                        {
                            ElementName = "collaterals",
                            Namespace = "http://www.efs.org/2007/EFSmL-3-0"
                        };

                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(PosCollaterals), root);
                        PosCollaterals result = (PosCollaterals)serializer.Deserialize(reader);
                        ret.Add(cur, result.collateral);
                    }
                }
            }
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20150427 [20987] Add 
        /// FI 20150427 [20987] GLOP Il faut tester DTENABLED
        private static CashBalanceSettings LoadCashBalanceSetting(string pCS, int pIdA, string pIdAHref)
        {

            CashBalanceSettings ret = new CashBalanceSettings();

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);
            string query = @"select CBSCOPE, EXCHANGEIDC, ISUSEAVAILABLECASH, CASHANDCOLLATERAL, ISMANAGEMENTBALANCE, MGCCALCMETHOD, CBCALCMETHOD, CBIDC
                            from dbo.CASHBALANCE where IDA = @IDA";

            QueryParameters queryParameters = new QueryParameters(pCS, query, dp);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            if (dt.Rows.Count == 0)
                throw new NotSupportedException(StrFunc.AppendFormat("CashBalance settings not found for actor (IdA:{0})", pIdA.ToString()));

            DataRow dr = dt.Rows[0];

            ret.cashAndCollateral = (CashAndCollateralLocalizationEnum)System.Enum.Parse(typeof(CashAndCollateralLocalizationEnum), Convert.ToString(dr["CASHANDCOLLATERAL"]), true);

            ret.cashBalanceCurrencySpecified = (Convert.DBNull != dr["CBIDC"]);
            if (ret.cashBalanceCurrencySpecified)
                ret.cashBalanceCurrency = new Currency(Convert.ToString(dr["CBIDC"]));

            ret.cashBalanceMethodSpecified = (Convert.DBNull != dr["CBCALCMETHOD"]);
            if (ret.cashBalanceMethodSpecified)
                ret.cashBalanceMethod = (CashBalanceCalculationMethodEnum)System.Enum.Parse(typeof(CashBalanceCalculationMethodEnum), Convert.ToString(dr["CBCALCMETHOD"]), true);

            ret.cashBalanceOfficePartyReference.href = pIdAHref;

            ret.exchangeCurrencySpecified = (Convert.DBNull != dr["EXCHANGEIDC"]);
            if (ret.exchangeCurrencySpecified)
                ret.exchangeCurrency = new Currency(Convert.ToString(dr["EXCHANGEIDC"]));

            ret.managementBalance = new EFS_Boolean(BoolFunc.IsTrue(dr["ISMANAGEMENTBALANCE"]));

            ret.marginCallCalculationMethod =
                 (MarginCallCalculationMethodEnum)System.Enum.Parse(typeof(MarginCallCalculationMethodEnum), Convert.ToString(dr["MGCCALCMETHOD"]), true);

            ret.scope =
                (GlobalElementaryEnum)System.Enum.Parse(typeof(GlobalElementaryEnum), Convert.ToString(dr["CBSCOPE"]), true);

            ret.useAvailableCash = new EFS_Boolean(BoolFunc.IsTrue(dr["ISUSEAVAILABLECASH"]));

            return ret;
        }


        /// <summary>
        /// Charge les taux de change nécessaires pour établier des contrevaleurs en pIdC2 à partir d'une devise pIdC1 à la date {DtBusiness}
        /// <para>Rerourne une liste vide si la contrevaleur en devise {pIdC2} est impossible par manque des cotations nécessaires</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="DtBusiness"></param>
        /// <param name="pIdC1"></param>
        /// <param name="pIdC2">Devise de contrevaleurs</param>
        /// <param name="pIdCPivot">Devise pivot (Elle sera utilisée s'il n'existe pas de teux de change entre pIdC1 et pIdC2)</param>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20160119 [21748] Modification de signature (La méthode retourne une liste) 
        /// FI 20200623 [XXXXX] Add SetErrorWarning
        private static List<IdentifiedFxRate> LoadFxRate(string pCS, DateTime DtBusiness, string pIdC1, string pIdC2, string pIdCPivot, SetErrorWarning pSetErrorWarning)
        {
            List<IdentifiedFxRate> retRate = new List<IdentifiedFxRate>();
            IProductBase productBase = Tools.GetNewProductBase();
            

            //Recherche prix Mid/close
            KeyQuote keyQuote = new KeyQuote(pCS, DtBusiness, null, null, FpML.Enum.QuotationSideEnum.OfficialClose, QuoteTimingEnum.Close);

            KeyAssetFxRate keyAssetFXRate = new KeyAssetFxRate
            {
                IdC1 = pIdC1,
                IdC2 = pIdC2
            };
            keyAssetFXRate.SetQuoteBasis();

            SQL_Quote quote = new SQL_Quote(pCS, QuoteEnum.FXRATE, AvailabilityEnum.Enabled, productBase, keyQuote, keyAssetFXRate);

            bool isLoaded = quote.IsLoaded;
            bool ret = isLoaded;
            if (isLoaded)
            {
                ret = isLoaded && (quote.QuoteValueCodeReturn == Cst.ErrLevel.SUCCESS);
                if (ret)
                {
                    IdentifiedFxRate rate = BuildIdentifiedFxRate(keyAssetFXRate.IdC1, keyAssetFXRate.IdC2, keyAssetFXRate.QuoteBasis, quote.IdQuote, quote.QuoteValue);
                    retRate.Add(rate);
                }
            }
            

            if (ret == false)
            {
                // Recherche du cours en devise 1 et devise2 en passant par une devise pivot 
                ret = quote.GetQuoteByPivotCurrency(pIdCPivot, out KeyQuoteFxRate quote_IdC1vsPivot, out KeyQuoteFxRate quote_IdC2vsPivot);
                if (ret)
                {
                    decimal exchangeRate = quote.QuoteValue;

                    IdentifiedFxRate rate = BuildIdentifiedFxRate(pIdC1, pIdCPivot, quote_IdC1vsPivot.QuoteBasis, quote_IdC1vsPivot.IdQuote, quote_IdC1vsPivot.QuoteValue);
                    retRate.Add(rate);

                    IdentifiedFxRate rate2 = BuildIdentifiedFxRate(pIdC2, pIdCPivot, quote_IdC2vsPivot.QuoteBasis, quote_IdC2vsPivot.IdQuote, quote_IdC2vsPivot.QuoteValue);
                    retRate.Add(rate2);

                    IdentifiedFxRate rate3 = BuildIdentifiedFxRate(keyAssetFXRate.IdC1, keyAssetFXRate.IdC2, keyAssetFXRate.QuoteBasis, 0, exchangeRate);
                    retRate.Add(rate3);
                }
            }
            //Génération d'un message d'erreur si Spheres® n'obtient pas de taux de change entre  pIdC1 et pIdC2
            if (ret == false)
            {
                // FI 20210308 [XXXXX] Spheres® génère un warning si la contrevaleur n'existe pas, l'exchangeStream est incorrect 
                if (null != pSetErrorWarning)
                    pSetErrorWarning.Invoke(ProcessStateTools.StatusWarningEnum);

                // FI 20210308 [XXXXX] Spheres® génère un warning si la contrevaleur n'existe pas, l'exchangeCashBalanceStream sera par conséquent faux 
                
                Logger.Log(new LoggerData(LogLevelEnum.Warning, quote.SystemMsgInfo.SysMsgCode, 3, quote.SystemMsgInfo.LogParamDatas));
            }

            return retRate;
        }

        /// <summary>
        ///  Retourne un un taux de change entre 2 devise
        /// </summary>
        /// <param name="pIdC1">Devise 1</param>
        /// <param name="pIdC2">Devise 2</param>
        /// <param name="pQuoteBasisEnum"></param>
        /// <param name="pIdQuote">Id non significatif de la cotation</param>
        /// <param name="pQuoteValue">valeur de la cotation</param>
        /// <returns></returns>
        /// FI 20160119 [21748] Add methode
        private static IdentifiedFxRate BuildIdentifiedFxRate(string pIdC1, string pIdC2, QuoteBasisEnum pQuoteBasisEnum, int pIdQuote, decimal pQuoteValue)
        {
            IdentifiedFxRate rate = new IdentifiedFxRate
            {
                OTCmlId = pIdQuote,
                rate = new EFS_Decimal(pQuoteValue),
                quotedCurrencyPair = new QuotedCurrencyPair(pIdC1, pIdC2, pQuoteBasisEnum)
            };
            switch (pQuoteBasisEnum)
            {
                case QuoteBasisEnum.Currency1PerCurrency2:
                    rate.Id = StrFunc.AppendFormat("{0}per{1}", pIdC1, pIdC2);
                    break;
                case QuoteBasisEnum.Currency2PerCurrency1:
                    rate.Id = StrFunc.AppendFormat("{0}per{1}", pIdC2, pIdC1);
                    break;
            }
            return rate;
        }



        /// <summary>
        /// Alimente un CashBalanceStream ou ExchangeCashBalanceStream à partir des flux de n CashBalance 
        /// </summary>
        /// <param name="pCS"></param>
        /// <typeparam name="T"></typeparam>
        /// <param name="pCurr">Devise du stream</param>
        /// <param name="rowcur">Liste des flux (issus des évènements), ces enregistrements doivent être dans la devise {pCurr}</param>
        /// <param name="partyActorRiskid"></param>
        /// <param name="partyEntityid"></param>
        /// FI 20150701 [XXXXX] Modify
        /// FI 20150715 [XXXXX] Modify
        /// FI 20150930 [21311] Modify 
        private static void BuidCashBalanceStream<T>(string pCS, T cashBalanceStream, string pCurr, DataRow[] pRowCurr, string partyActorRiskid, string partyEntityid)
        {
            ExchangeCashBalanceStream ecs = null;
            CashBalanceStream cbs = null;

            if (null == cashBalanceStream)
                throw new ArgumentNullException("cashBalanceStream argument is null");

            if (false == (cashBalanceStream is ExchangeCashBalanceStream || cashBalanceStream is CashBalanceStream))
                throw new ArgumentException("cashBalanceStream is not valid");

            bool isCashBalanceStream = cashBalanceStream is CashBalanceStream;
            if (isCashBalanceStream)
            {
                cbs = cashBalanceStream as CashBalanceStream;
                cbs.currency = new Currency(pCurr);
            }
            else
            {
                ecs = cashBalanceStream as ExchangeCashBalanceStream;
                ecs.currency = new Currency(pCurr);
            }

            /* marginRequirement - MGR */
            CashPosition marginRequirement = null;
            if (isCashBalanceStream)
            {
                cbs.marginRequirement = new CssExchangeCashPosition();
                marginRequirement = cbs.marginRequirement;
            }
            else
            {
                ecs.marginRequirement = new CashPosition();
                ecs.marginRequirementSpecified = true;
                marginRequirement = ecs.marginRequirement;
            }
            InitCashPositionFromEventType(pRowCurr, pCurr, "MGR", marginRequirement, partyActorRiskid, partyEntityid);


            /* cashAvailable - CSA */
            // FI 20150930 [21311] Lecture du flux CSA (Cash Available du dernier arrêté)
            // FI 20150930 [21311] add  isExistRow
            CashAvailable cashAvailable = null;
            Boolean isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "CSA").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.cashAvailable = new CashAvailable();
                cashAvailable = cbs.cashAvailable;
            }
            else
            {
                ecs.cashAvailable = new CashAvailable();
                ecs.cashAvailableSpecified = isExistRow;
                cashAvailable = ecs.cashAvailable;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "CSA", cashAvailable, partyActorRiskid, partyEntityid);

            cashAvailable.constituentSpecified = true;
            /* CashAvailable - constituent - cashBalancePayment - CBP */
            cashAvailable.constituent.cashBalancePayment = new CashBalancePayment();
            InitCashPositionFromEventType(pRowCurr, pCurr, "CBP", cashAvailable.constituent.cashBalancePayment, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - previousCashBalance - PCB */
            cashAvailable.constituent.previousCashBalance = new CashPosition();
            InitCashPositionFromEventType(pRowCurr, pCurr, "PCB", cashAvailable.constituent.previousCashBalance, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - (VMG, PRM, SCU, FEE, SKP) */
            // FI 20150715 [XXXXX] add SKP
            // FI 20150930 [21311] Considération des frais avec leurs taxes 
            cashAvailable.constituent.cashFlows = new CashFlows();
            // PM 20170911 [23408] Ajout EQP
            DataRow[] result = (from item in pRowCurr
                                where (
                                        Convert.ToString(item["EVENTTYPE"]) == "VMG" ||
                                        Convert.ToString(item["EVENTTYPE"]) == "PRM" ||
                                        Convert.ToString(item["EVENTTYPE"]) == "SCU" ||
                                        Convert.ToString(item["EVENTTYPE"]) == "FEE" ||
                                        Convert.ToString(item["EVENTTYPE"]) == "SKP" ||
                                        Convert.ToString(item["EVENTTYPE"]) == "EQP")
                                select item).ToArray();
            decimal amount = Decimal.Zero;
            if (ArrFunc.IsFilled(result))
                amount = (from item in result select Convert.ToDecimal(item["AMT"])).Sum();
            InitCashPosition(cashAvailable.constituent.cashFlows, amount, pCurr, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - constituent - VMG */
            cashAvailable.constituent.cashFlows.constituent.variationMargin = new ContractSimplePayment();
            InitSimplePaymentFromEventType(pRowCurr, pCurr, "VMG", cashAvailable.constituent.cashFlows.constituent.variationMargin, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - constituent - PRM */
            cashAvailable.constituent.cashFlows.constituent.premium = new ContractSimplePayment();
            InitSimplePaymentFromEventType(pRowCurr, pCurr, "PRM", cashAvailable.constituent.cashFlows.constituent.premium, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - constituent - SCU */
            cashAvailable.constituent.cashFlows.constituent.cashSettlement = new ContractSimplePaymentConstituent();
            InitSimplePaymentFromEventType(pRowCurr, pCurr, "SCU", cashAvailable.constituent.cashFlows.constituent.cashSettlement, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - constituent - EQP */
            // PM 20170911 [23408] Ajout EQP
            cashAvailable.constituent.cashFlows.constituent.equalisationPayment = new ContractSimplePayment();
            InitSimplePaymentFromEventType(pRowCurr, pCurr, "EQP", cashAvailable.constituent.cashFlows.constituent.equalisationPayment, partyActorRiskid, partyEntityid);
            cashAvailable.constituent.cashFlows.constituent.equalisationPaymentSpecified = (cashAvailable.constituent.cashFlows.constituent.equalisationPayment.paymentAmount.Amount.DecValue != 0);

            /* CashAvailable - constituent - CashFlows - constituent - FEE */
            cashAvailable.constituent.cashFlows.constituent.fee = BuilFee(pCS, "FEE", pRowCurr, pCurr, partyActorRiskid, partyEntityid);

            /* CashAvailable - constituent - CashFlows - constituent - SKP */
            // FI 20150715 [XXXXX] add SKP
            cashAvailable.constituent.cashFlows.constituent.safekeeping = BuilFee(pCS, "SKP", pRowCurr, pCurr, partyActorRiskid, partyEntityid);

            /* cashUsed - CSU*/
            CashPosition cashUsed = null;
            //FI 20150930 [21311] add  isExistRow
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "CSU").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.cashUsed = new CashPosition();
                cbs.cashUsedSpecified = isExistRow;
                cashUsed = cbs.cashUsed;
            }
            else
            {
                ecs.cashUsed = new CashPosition();
                ecs.cashUsedSpecified = isExistRow;
                cashUsed = ecs.cashUsed;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "CSU", cashUsed, partyActorRiskid, partyEntityid);

            /* collateralAvailable - CLA */
            //FI 20150930 [21311] add  isExistRow
            CashPosition collateralAvailable = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "CLA").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.collateralAvailable = new CollateralAvailable();
                collateralAvailable = cbs.collateralAvailable;
            }
            else
            {
                ecs.collateralAvailable = new CashPosition();
                ecs.collateralAvailableSpecified = isExistRow;
                collateralAvailable = ecs.collateralAvailable;
            }
            if (isCashBalanceStream || ((false == isCashBalanceStream) && ecs.collateralAvailableSpecified))
                InitCashPositionFromEventType(pRowCurr, pCurr, "CLA", collateralAvailable, partyActorRiskid, partyEntityid);

            /* collateralUsed - CLU */
            //FI 20150930 [21311] add  isExistRow
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "CLU").Count() > 0);
            CashPosition collateralUsed = null;
            if (isCashBalanceStream)
            {
                cbs.collateralUsed = new CashPosition();
                cbs.collateralUsedSpecified = isExistRow;
                collateralUsed = cbs.collateralUsed;
            }
            else
            {
                ecs.collateralUsed = new CashPosition();
                ecs.collateralUsedSpecified = isExistRow;
                collateralUsed = ecs.collateralUsed;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "CLU", collateralUsed, partyActorRiskid, partyEntityid);


            /* uncoveredMarginRequirement - UMR */
            //FI 20150930 [21311] add  isExistRow
            CashPosition uncoveredMarginRequirement = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "UMR").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.uncoveredMarginRequirement = new CashPosition();
                cbs.uncoveredMarginRequirementSpecified = isExistRow;
                uncoveredMarginRequirement = cbs.uncoveredMarginRequirement;
            }
            else
            {
                ecs.uncoveredMarginRequirement = new CashPosition();
                ecs.uncoveredMarginRequirementSpecified = isExistRow;
                uncoveredMarginRequirement = ecs.uncoveredMarginRequirement;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "UMR", uncoveredMarginRequirement, partyActorRiskid, partyEntityid);

            /* marginCall - MGC */
            //FI 20150930 [21311] add  isExistRow
            SimplePayment marginCall = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "MGC").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.marginCall = new SimplePayment();
                cbs.marginCallSpecified = isExistRow;
                marginCall = cbs.marginCall;
            }
            else
            {
                ecs.marginCall = new SimplePayment();
                ecs.marginCallSpecified = isExistRow;
                marginCall = ecs.marginCall;
            }
            if (isExistRow)
                InitSimplePaymentFromEventType(pRowCurr, pCurr, "MGC", marginCall, partyActorRiskid, partyEntityid);


            /* cashBalance - CSB */
            //FI 20150930 [21311] add  isExistRow
            CashPosition cashBalance = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "CSB").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.cashBalance = new CashPosition();
                cashBalance = cbs.cashBalance;
            }
            else
            {
                ecs.cashBalance = new CashPosition();
                ecs.cashBalanceSpecified = isExistRow;
                cashBalance = ecs.cashBalance;
            }
            if (isCashBalanceStream || (false == isCashBalanceStream && ecs.cashBalanceSpecified))
                InitCashPositionFromEventType(pRowCurr, pCurr, "CSB", cashBalance, partyActorRiskid, partyEntityid);

            /*previousMarginConstituent*/
            //FI 20150930 [21311] Alimentation de previousMarginConstituent
            PreviousMarginConstituent previousMarginConstituent = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "PREVIOUS_MGR").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.previousMarginConstituent = new PreviousMarginConstituent();
                cbs.previousMarginConstituentSpecified = isExistRow;
                previousMarginConstituent = cbs.previousMarginConstituent;
            }
            else
            {
                ecs.previousMarginConstituent = new PreviousMarginConstituent();
                ecs.previousMarginConstituentSpecified = isExistRow;
                previousMarginConstituent = ecs.previousMarginConstituent;
            }
            if (isExistRow)
            {
                previousMarginConstituent.marginRequirement = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_MGR", previousMarginConstituent.marginRequirement, partyActorRiskid, partyEntityid);

                previousMarginConstituent.cashAvailable = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_CSA", previousMarginConstituent.cashAvailable, partyActorRiskid, partyEntityid);
                previousMarginConstituent.cashUsed = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_CSU", previousMarginConstituent.cashUsed, partyActorRiskid, partyEntityid);

                previousMarginConstituent.collateralAvailable = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_CLA", previousMarginConstituent.collateralAvailable, partyActorRiskid, partyEntityid);
                previousMarginConstituent.collateralUsed = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_CLU", previousMarginConstituent.collateralUsed, partyActorRiskid, partyEntityid);

                previousMarginConstituent.uncoveredMarginRequirement = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "PREVIOUS_UMR", previousMarginConstituent.uncoveredMarginRequirement, partyActorRiskid, partyEntityid);
            }

            /* realizedMargin*/
            //FI 20150930 [21311] add isExistRow
            MarginConstituent realizedMargin = null;
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "RMG").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.realizedMargin = new MarginConstituent();
                cbs.realizedMarginSpecified = isExistRow;
                realizedMargin = cbs.realizedMargin;
            }
            else
            {
                ecs.realizedMargin = new MarginConstituent();
                ecs.realizedMarginSpecified = isExistRow;
                realizedMargin = ecs.realizedMargin;
            }
            if (isExistRow)
            {
                realizedMargin.globalAmountSpecified = true;
                realizedMargin.globalAmount = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "RMG", realizedMargin.globalAmount, partyActorRiskid, partyEntityid);

                /* realizedMargin - FUT */
                realizedMargin.future = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "FUTRMG", realizedMargin.future, partyActorRiskid, partyEntityid);
                realizedMargin.futureSpecified = (realizedMargin.future.amount.Amount.DecValue > decimal.Zero);

                /* realizedMargin - OPT */
                realizedMargin.option = new OptionMarginConstituent[2] { new OptionMarginConstituent(  ){
                                                                                valuationMethod = FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket }, 
                                                                                    new OptionMarginConstituent(){
                                                                                valuationMethod = FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle}};

                InitCashPositionFromEventType(pRowCurr, pCurr, "OPTRMG_FSO", realizedMargin.option[0], partyActorRiskid, partyEntityid);
                InitCashPositionFromEventType(pRowCurr, pCurr, "OPTRMG_PSO", realizedMargin.option[1], partyActorRiskid, partyEntityid);
                realizedMargin.optionSpecified = (from item in realizedMargin.option
                                                  where item.amount.amount.DecValue > decimal.Zero
                                                  select item).Count() > 0;
            }


            /* unrealizedMargin */
            MarginConstituent unrealizedMargin = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => Convert.ToString(x["EVENTTYPE"]) == "UMG").Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.unrealizedMargin = new MarginConstituent();
                cbs.unrealizedMarginSpecified = isExistRow;
                unrealizedMargin = cbs.unrealizedMargin;
            }
            else
            {
                ecs.unrealizedMargin = new MarginConstituent();
                ecs.unrealizedMarginSpecified = isExistRow;
                unrealizedMargin = ecs.unrealizedMargin;
            }

            if (isExistRow)
            {
                unrealizedMargin.globalAmountSpecified = true;
                unrealizedMargin.globalAmount = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "UMG", unrealizedMargin.globalAmount, partyActorRiskid, partyEntityid);

                /* unrealizedMargin - FUT */
                unrealizedMargin.future = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "FUTUMG", unrealizedMargin.future, partyActorRiskid, partyEntityid);
                unrealizedMargin.futureSpecified = (unrealizedMargin.future.amount.Amount.DecValue > decimal.Zero);

                /* unrealizedMargin - OPT */
                unrealizedMargin.option = new OptionMarginConstituent[2] { new OptionMarginConstituent(  ){
                                                                                valuationMethod = FixML.v50SP1.Enum.FuturesValuationMethodEnum.FuturesStyleMarkToMarket }, 
                                                                                    new OptionMarginConstituent(){
                                                                                valuationMethod = FixML.v50SP1.Enum.FuturesValuationMethodEnum.PremiumStyle}};

                InitCashPositionFromEventType(pRowCurr, pCurr, "OPTUMG_FSO", unrealizedMargin.option[0], partyActorRiskid, partyEntityid);
                InitCashPositionFromEventType(pRowCurr, pCurr, "OPTUMG_PSO", unrealizedMargin.option[1], partyActorRiskid, partyEntityid);
                unrealizedMargin.optionSpecified = (from item in unrealizedMargin.option
                                                    where item.amount.amount.DecValue > decimal.Zero
                                                    select item).Count() > 0;
            }


            /* liquidatingValue */
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "OVL") || (Convert.ToString(x["EVENTTYPE"]) == "OVS")).Count() > 0);
            OptionLiquidatingValue liquidatingValue = null;
            if (isCashBalanceStream)
            {
                cbs.liquidatingValue = new OptionLiquidatingValue();
                cbs.liquidatingValueSpecified = isExistRow;
                liquidatingValue = cbs.liquidatingValue;
            }
            else
            {
                ecs.liquidatingValue = new OptionLiquidatingValue();
                ecs.liquidatingValueSpecified = isExistRow;
                liquidatingValue = ecs.liquidatingValue;
            }
            if (isExistRow)
            {
                InitCashPositionFromEventType(pRowCurr, pCurr, new string[] { "OVL", "OVS" }, liquidatingValue, partyActorRiskid, partyEntityid);

                /* liquidatingValue - OVL */
                liquidatingValue.longOptionValue = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "OVL", liquidatingValue.longOptionValue, partyActorRiskid, partyEntityid);
                liquidatingValue.longOptionValueSpecified = liquidatingValue.longOptionValue.amount.amount.DecValue > decimal.Zero;

                /* liquidatingValue - OVS */
                liquidatingValue.shortOptionValue = new CashPosition();
                InitCashPositionFromEventType(pRowCurr, pCurr, "OVS", liquidatingValue.shortOptionValue, partyActorRiskid, partyEntityid);
                liquidatingValue.shortOptionValueSpecified = liquidatingValue.shortOptionValue.amount.amount.DecValue > decimal.Zero;
            }

            /* FI 20150701 [XXXXX] Add MKV */
            /* marketValue - MKV */
            DetailedCashPosition marketValue = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "MKV")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.marketValue = new DetailedCashPosition();
                cbs.marketValueSpecified = isExistRow;
                marketValue = cbs.marketValue;
            }
            else
            {
                ecs.marketValue = new DetailedCashPosition();
                ecs.marketValueSpecified = isExistRow;
                marketValue = ecs.marketValue;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "MKV", marketValue, partyActorRiskid, partyEntityid);

            /* funding - FDA */
            DetailedCashPosition funding = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "FDA")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.funding = new DetailedCashPosition();
                cbs.fundingSpecified = isExistRow;
                funding = cbs.funding;
            }
            else
            {
                ecs.funding = new DetailedCashPosition();
                ecs.fundingSpecified = isExistRow;
                funding = ecs.funding;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, EventTypeFunc.FundingAmount, funding, partyActorRiskid, partyEntityid);


            /* borrowing - BRA */
            DetailedCashPosition borrowing = null;
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "BRA")).Count() > 0);
            //FI 20150930 [21311] add isExistRow
            if (isCashBalanceStream)
            {
                cbs.borrowing = new DetailedCashPosition();
                cbs.borrowingSpecified = isExistRow;
                borrowing = cbs.borrowing;
            }
            else
            {
                ecs.borrowing = new DetailedCashPosition();
                ecs.borrowingSpecified = isExistRow;
                borrowing = ecs.borrowing;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, EventTypeFunc.BorrowingAmount, borrowing, partyActorRiskid, partyEntityid);

            /* unsettledCash - UST */
            DetailedCashPosition unsettledCash = null;
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "UST")).Count() > 0);
            //FI 20150930 [21311] add isExistRow
            if (isCashBalanceStream)
            {
                cbs.unsettledCash = new DetailedCashPosition();
                cbs.unsettledCashSpecified = isExistRow;
                unsettledCash = cbs.unsettledCash;
            }
            else
            {
                ecs.unsettledCash = new DetailedCashPosition();
                ecs.unsettledCashSpecified = isExistRow;
                unsettledCash = ecs.unsettledCash;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "UST", unsettledCash, partyActorRiskid, partyEntityid);

            /* forwardCashPayment - FCP */
            CashBalancePayment forwardCashPayment = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "FCP")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.forwardCashPayment = new CashBalancePayment();
                cbs.forwardCashPaymentSpecified = isExistRow;
                forwardCashPayment = cbs.forwardCashPayment;
            }
            else
            {
                ecs.forwardCashPayment = new CashBalancePayment();
                ecs.forwardCashPaymentSpecified = isExistRow;
                forwardCashPayment = ecs.forwardCashPayment;
            }
            if (isExistRow)
            {
                InitCashPositionFromEventType(pRowCurr, pCurr, "FCP", forwardCashPayment, partyActorRiskid, partyEntityid);
                forwardCashPayment.cashWithdrawalSpecified = false;
                forwardCashPayment.cashDepositSpecified = false;
            }


            /* equityBalance - E_B */
            CashPosition equityBalance = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "E_B")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.equityBalance = new CashPosition();
                cbs.equityBalanceSpecified = isExistRow;
                equityBalance = cbs.equityBalance;
            }
            else
            {
                ecs.equityBalance = new CashPosition();
                ecs.equityBalanceSpecified = isExistRow;
                equityBalance = ecs.equityBalance;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "E_B", equityBalance, partyActorRiskid, partyEntityid);

            /* equityBalanceWithForwardCash - EBF */
            CashPosition equityBalanceWithForwardCash = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "EBF")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.equityBalanceWithForwardCash = new CashPosition();
                cbs.equityBalanceWithForwardCashSpecified = isExistRow;
                equityBalanceWithForwardCash = cbs.equityBalanceWithForwardCash;
            }
            else
            {
                ecs.equityBalanceWithForwardCash = new CashPosition();
                ecs.equityBalanceWithForwardCashSpecified = isExistRow;
                equityBalanceWithForwardCash = ecs.equityBalanceWithForwardCash;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "EBF", equityBalanceWithForwardCash, partyActorRiskid, partyEntityid);


            /* FI 20150701 [XXXXX] Add TAV */
            /* totalAccountValue - TAV */
            CashPosition totalAccountValue = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "TAV")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.totalAccountValue = new CashPosition();
                cbs.totalAccountValueSpecified = isExistRow;
                totalAccountValue = cbs.totalAccountValue;
            }
            else
            {
                ecs.totalAccountValue = new CashPosition();
                ecs.totalAccountValueSpecified = isExistRow;
                totalAccountValue = ecs.totalAccountValue;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "TAV", totalAccountValue, partyActorRiskid, partyEntityid);

            /* excessDeficit - E_D*/
            CashPosition excessDeficit = null;
            //FI 20150930 [21311] add isExistRow
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "E_D")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.excessDeficit = new CashPosition();
                cbs.excessDeficitSpecified = isExistRow;
                excessDeficit = cbs.excessDeficit;
            }
            else
            {
                ecs.excessDeficit = new CashPosition();
                ecs.excessDeficitSpecified = isExistRow;
                excessDeficit = ecs.excessDeficit;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "E_D", excessDeficit, partyActorRiskid, partyEntityid);

            /*excessDeficitWithForwardCash*/
            CashPosition excessDeficitWithForwardCash = null;
            isExistRow = (pRowCurr.Where(x => (Convert.ToString(x["EVENTTYPE"]) == "EDF")).Count() > 0);
            if (isCashBalanceStream)
            {
                cbs.excessDeficitWithForwardCash = new CashPosition();
                cbs.excessDeficitWithForwardCashSpecified = isExistRow;
                excessDeficitWithForwardCash = cbs.excessDeficitWithForwardCash;
            }
            else
            {
                ecs.excessDeficitWithForwardCash = new CashPosition();
                ecs.excessDeficitWithForwardCashSpecified = isExistRow;
                excessDeficitWithForwardCash = ecs.excessDeficitWithForwardCash;
            }
            if (isExistRow)
                InitCashPositionFromEventType(pRowCurr, pCurr, "EDF", excessDeficitWithForwardCash, partyActorRiskid, partyEntityid);
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pEventType"></param>
        /// <param name="pCur"></param>
        /// <param name="rowcur"></param>
        /// <param name="cashFlowsConstituent"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        /// FI 20150715 [XXXXX] Modify (add paramètre pEventType) 
        private static DetailedContractPayment[] BuilFee(string pCS, string pEventType, DataRow[] rowcur, string pCur, string partyActorRiskId, string partyEntityId)
        {

            DetailedContractPayment[] ret = null;
            List<DetailedContractPayment> lstPayment = new List<DetailedContractPayment>();

            /* CashAvailable - constituent - CashFlows - constituent - FEE */
            DataRow[] result = (from item in rowcur
                                where ((Convert.ToString(item["EVENTTYPE"]) == pEventType) && (Convert.ToString(item["UNIT"]) == pCur))
                                select item).ToArray();

            string columnPAYMENTTYPE = StrFunc.AppendFormat("{0}PAYMENTTYPE", pEventType);
            string columnIDTAX = StrFunc.AppendFormat("{0}IDTAX", pEventType);
            string columnIDTAXDET = StrFunc.AppendFormat("{0}IDTAXDET", pEventType);
            string columnTAXCOUNTRY = StrFunc.AppendFormat("{0}TAXCOUNTRY", pEventType);
            string columnTAXTYPE = StrFunc.AppendFormat("{0}TAXTYPE", pEventType);
            string columnTAXRATE = StrFunc.AppendFormat("{0}TAXRATE", pEventType);

            List<String> feeType = (from item in result
                                    select Convert.ToString(item[columnPAYMENTTYPE])).Distinct().ToList();


            foreach (String itemFeeType in feeType)
            {
                lstPayment.Add(new DetailedContractPayment());
                DetailedContractPayment payment = lstPayment.Last();

                DataRow[] feeHT = (from item in result
                                   where (Convert.ToString(item[columnPAYMENTTYPE]) == itemFeeType && Convert.ToInt32(item[columnIDTAX]) == -1)
                                   select item).ToArray();

                decimal feeAmount = Decimal.Zero;
                if (ArrFunc.IsFilled(feeHT))
                    feeAmount = (from DataRow itemRow in feeHT select Convert.ToDecimal(itemRow["AMT"])).Sum();


                payment.payerPartyReference.href = (feeAmount <= decimal.Zero) ? partyActorRiskId : partyEntityId;
                payment.receiverPartyReference.href = (feeAmount > decimal.Zero) ? partyActorRiskId : partyEntityId;
                payment.paymentDate = null;
                payment.paymentAmount = new Money(System.Math.Abs(feeAmount), pCur);
                payment.paymentTypeSpecified = (itemFeeType != "NotSpecified");
                if (payment.paymentTypeSpecified)
                    payment.paymentType = new PaymentType() { Value = itemFeeType };


                payment.paymentSourceSpecified = false; /// FI 20150427 [20987] 

                DataRow[] feeTax = (from item in result
                                    where (Convert.ToString(item[columnPAYMENTTYPE]) == itemFeeType && Convert.ToInt32(item[columnIDTAX]) != -1)
                                    select item).ToArray();

                if (ArrFunc.IsFilled(feeTax))
                {
                    var grpTax = (from item in feeTax
                                  select new
                                  {
                                      IdTax = Convert.ToInt32(item[columnIDTAX])
                                  }).ToList().Distinct();

                    List<Tax> lstGrpTax = new List<Tax>();
                    foreach (var item in grpTax)
                    {
                        Tax taxGrp = new Tax();

                        // FI 20200520 [XXXXX] Add SQL cache
                        SQL_Tax sqlTax = new SQL_Tax(CSTools.SetCacheOn(pCS), item.IdTax);
                        sqlTax.LoadTable(new string[] { "IDTAX,IDENTIFIER" });

                        // Tax Source
                        taxGrp.taxSource.statusSpecified = false;
                        taxGrp.taxSource.spheresId = new SpheresId[2] { new SpheresId(), new SpheresId() };

                        // Tax Group
                        taxGrp.taxSource.spheresId[0].scheme = Cst.OTCml_RepositoryTaxScheme;
                        taxGrp.taxSource.spheresId[0].OTCmlId = sqlTax.Id;
                        taxGrp.taxSource.spheresId[0].Value = sqlTax.Identifier;
                        //
                        // EventType
                        taxGrp.taxSource.spheresId[1].scheme = Cst.OTCml_RepositoryFeeEventTypeScheme;
                        taxGrp.taxSource.spheresId[1].Value = itemFeeType;

                        lstGrpTax.Add(taxGrp);
                    }

                    payment.tax = lstGrpTax.ToArray();

                    foreach (Tax itemTax in payment.tax)
                    {
                        // Rq: Choix délivéré de lire les informations présentes dans EVENTFEE plutôt que lire le paramétrage TAXDET 
                        // En effet le taux de d'une même taxe pourrait évoluer entre le début de la fin de période tu traitement d'édtion périodique
                        // Dans ce cas dans le détail de l'état Spheres® affiche autant de détail qu'il y a eu de changement de taux
                        // Comportement vu avec RD 
                        var taxShedule = (from item in feeTax
                                          where Convert.ToInt32(item[columnIDTAX]) == itemTax.taxSource.spheresId[0].OTCmlId
                                          select new
                                          {
                                              IdTaxDet = Convert.ToInt32(item[columnIDTAXDET]),
                                              taxCountry = Convert.ToString(item[columnTAXCOUNTRY]),
                                              taxType = Convert.ToString(item[columnTAXTYPE]),
                                              taxRate = Convert.ToDecimal(item[columnTAXRATE]),
                                          }).ToList().Distinct();

                        if (taxShedule.Count() > 0)
                        {

                            List<TaxSchedule> lstTaxSchedule = new List<TaxSchedule>();
                            foreach (var itemTaxShedule in taxShedule)
                            {
                                DataRow[] feeTaxSxhedule = (from item in result
                                                            where (Convert.ToString(item[columnPAYMENTTYPE]) == itemFeeType &&
                                                                   Convert.ToInt32(item[columnIDTAX]) == itemTax.taxSource.spheresId[0].OTCmlId &&
                                                                   Convert.ToInt32(item[columnIDTAXDET]) == itemTaxShedule.IdTaxDet &&
                                                                   Convert.ToString(item[columnTAXCOUNTRY]) == itemTaxShedule.taxCountry &&
                                                                   Convert.ToString(item[columnTAXTYPE]) == itemTaxShedule.taxType &&
                                                                   Convert.ToDecimal(item[columnTAXRATE]) == itemTaxShedule.taxRate)
                                                            select item).ToArray();

                                decimal taxAmount = Decimal.Zero;
                                if (ArrFunc.IsFilled(feeTaxSxhedule))
                                    taxAmount = (from DataRow itemRow in feeTaxSxhedule select Convert.ToDecimal(itemRow["AMT"])).Sum();

                                // FI 20200520 [XXXXX] Add SQL cache
                                SQL_TaxDet sqlTaxDet = new SQL_TaxDet(CSTools.SetCacheOn(pCS), itemTaxShedule.IdTaxDet);
                                sqlTaxDet.LoadTable(new string[] { "IDTAXDET,IDENTIFIER" });

                                ITaxSchedule taxSchedule = new TaxSchedule();
                                // Amount calculation
                                taxSchedule.TaxAmountSpecified = true;
                                taxSchedule.TaxAmount = taxSchedule.CreateTripleInvoiceAmounts;
                                taxSchedule.TaxAmount.Amount = new Money(System.Math.Abs(taxAmount), pCur);

                                // Source Tax Details
                                taxSchedule.TaxSource = new SpheresSource();
                                ISpheresSource source = taxSchedule.TaxSource;
                                source.SpheresId = new SpheresId[4] { new SpheresId(), new SpheresId(), new SpheresId(), new SpheresId() };

                                // Identifier TAXDET
                                source.SpheresId[0].Scheme = Cst.OTCml_RepositoryTaxDetailScheme;
                                source.SpheresId[0].OTCmlId = sqlTaxDet.Id;
                                source.SpheresId[0].Value = sqlTaxDet.Identifier;

                                // Country
                                source.SpheresId[1].Scheme = Cst.OTCml_RepositoryTaxDetailCountryScheme;
                                source.SpheresId[1].Value = itemTaxShedule.taxCountry;

                                // Type
                                source.SpheresId[2].Scheme = Cst.OTCml_RepositoryTaxDetailTypeScheme;
                                source.SpheresId[2].Value = itemTaxShedule.taxType;

                                // Rate
                                source.SpheresId[3].Scheme = Cst.OTCml_RepositoryTaxDetailRateScheme;
                                source.SpheresId[3].Value = StrFunc.FmtDecimalToInvariantCulture(itemTaxShedule.taxRate);

                                lstTaxSchedule.Add((TaxSchedule)taxSchedule);

                            }
                            if (ArrFunc.IsFilled(lstTaxSchedule))
                                itemTax.taxDetail = lstTaxSchedule.ToArray();
                        }
                    }
                }
            }

            ret = lstPayment.ToArray();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCashPosition"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurr"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitCashPosition(CashPosition pCashPosition, decimal pAmount, string pCurr, string partyActorRiskId, string partyEntityId)
        {
            pCashPosition.payerPartyReference.href = (pAmount <= decimal.Zero) ? partyActorRiskId : partyEntityId;
            pCashPosition.receiverPartyReference.href = (pAmount > decimal.Zero) ? partyActorRiskId : partyEntityId;
            pCashPosition.dateReferenceSpecified = false; // FI 20150427 [20987] 
            pCashPosition.amount = new Money(System.Math.Abs(pAmount), pCurr);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowcur"></param>
        /// <param name="pCurr"></param>
        /// <param name="pEventType"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitCashPositionFromEventType(DataRow[] rowcur, string pCurr, string pEventType, CashPosition pCashPosition, string partyActorRiskId, string partyEntityId)
        {
            InitCashPositionFromEventType(rowcur, pCurr, new string[1] { pEventType }, pCashPosition, partyActorRiskId, partyEntityId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowcur"></param>
        /// <param name="pCurr"></param>
        /// <param name="pEventType"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="?"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitCashPositionFromEventType(DataRow[] rowcur, string pCurr, string[] pEventType, CashPosition pCashPosition, string partyActorRiskId, string partyEntityId)
        {
            DataRow[] result = (from item in rowcur
                                where (ArrFunc.ExistInArrayString(pEventType, Convert.ToString(item["EVENTTYPE"])) && Convert.ToString(item["UNIT"]) == pCurr)
                                select item).ToArray();

            decimal amount = Decimal.Zero;
            if (ArrFunc.IsFilled(result))
                amount = (from DataRow itemRow in result select Convert.ToDecimal(itemRow["AMT"])).Sum();

            InitCashPosition(pCashPosition, amount, pCurr, partyActorRiskId, partyEntityId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowcur"></param>
        /// <param name="pCurr"></param>
        /// <param name="pEventType"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitSimplePaymentFromEventType(DataRow[] rowcur, string pCurr, string pEventType, SimplePayment pCashPosition, string partyActorRiskId, string partyEntityId)
        {
            InitSimplePaymentFromEventType(rowcur, pCurr, new string[] { pEventType }, pCashPosition, partyActorRiskId, partyEntityId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowcur"></param>
        /// <param name="pCurr"></param>
        /// <param name="pEventType"></param>
        /// <param name="pCashPosition"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitSimplePaymentFromEventType(DataRow[] rowcur, string pCurr, string[] pEventType, SimplePayment pCashPosition, string partyActorRiskId, string partyEntityId)
        {
            DataRow[] result = (from item in rowcur
                                where (ArrFunc.ExistInArrayString(pEventType, Convert.ToString(item["EVENTTYPE"])) && Convert.ToString(item["UNIT"]) == pCurr)
                                select item).ToArray();

            decimal amount = Decimal.Zero;
            if (ArrFunc.IsFilled(result))
                amount = (from DataRow itemRow in result select Convert.ToDecimal(itemRow["AMT"])).Sum();

            InitSimplePayment(pCashPosition, amount, pCurr, partyActorRiskId, partyEntityId);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSimplePayment"></param>
        /// <param name="pAmount"></param>
        /// <param name="pCurr"></param>
        /// <param name="partyActorRiskId"></param>
        /// <param name="partyEntityId"></param>
        private static void InitSimplePayment(SimplePayment pSimplePayment, decimal pAmount, string pCurr, string partyActorRiskId, string partyEntityId)
        {
            pSimplePayment.payerPartyReference.href = (pAmount <= decimal.Zero) ? partyActorRiskId : partyEntityId;
            pSimplePayment.receiverPartyReference.href = (pAmount > decimal.Zero) ? partyActorRiskId : partyEntityId;
            pSimplePayment.paymentDate = null;
            pSimplePayment.paymentAmount = new Money(System.Math.Abs(pAmount), pCurr);
        }


        /// <summary>
        ///  Requêtes de lecture des évènements de n CB afin de constitué un trade CB fictif. 
        ///  <para>Généralement il y a somme des flux ( exemple sommes des prime, sommes des variationMargin,etc..)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeRisk">Représente les liste des trades CB de la periode tri par ordre croissant</param>
        /// <returns></returns>
        /// FI 20150701 [XXXXX] Modify
        /// FI 20150710 [XXXXX] Modify
        /// RD 20170918 [23434] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryCashBalancePeriodic(string pCS, List<SQL_TradeRisk> pTradeRisk)
        {
            if (ArrFunc.IsEmpty(pTradeRisk))
                throw new ArgumentException("EmptyArray is not allowed");

            int firsTrade = pTradeRisk[0].IdT;
            DateTime dtStart = Convert.ToDateTime(pTradeRisk[0].GetFirstRowColumnValue("DTBUSINESS"));

            int lastTrade = pTradeRisk[pTradeRisk.Count() - 1].IdT;
            DateTime dtEnd = Convert.ToDateTime(pTradeRisk[pTradeRisk.Count() - 1].GetFirstRowColumnValue("DTBUSINESS"));

            List<int> idT = (from item in pTradeRisk select item.IdT).ToList();

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTEND), dtEnd);

            // FI 20150701 [XXXXX] add marketValue, totalAccountValue
            // FI 20150710 [XXXXX] add SKP
            // FI 20150930 [21311] add CSA (Cash Available from Last CB)
            // FI 20151006 [21311] Add (Exclusion des OPP où  ev.IDA_PAY != ev.IDA_REC 
            //                                 car bizarrement Spheres® génère des évènements OPP  (ev.IDA_PAY == ev.IDA_REC) lorsqu'il n'existe pas de frais sur un stream CashBalance)
            // Il faudra supprimer ce code lorsque cette faille sera corrigée (voir ticket 21432)
            // 
            // FI 201510 [21513] Add LPP,GAM en tant que CashSettlement (LPP,GAM => Gross AMount sur les debtSecurityTransaction et EquitySecurityTransaction) 
            // FI 201510 [21513] Add INT,INT en tant que CashSettlement (INT,INT => full coupon sur les debtSecurityTransaction) 
            // PM 20170911 [23408] Ajout LPC/EQP
            // RD 20170918 [23434] Utiliser "inner join dbo.EVENT e_parent on (e_parent.IDE=ev.IDE_EVENT)" à la place de "inner join dbo.EVENT e_parent on (e_parent.IDT = ev.IDT) and (e_parent.IDE=ev.IDE_EVENT)", pour deux raisons:
            // 1- La colonne IDE est une PK de la table EVENT, alors le critère sur cette colonne suffit
            // 2- Il n'existe pas d'index sur la table EVENT sur les colonnes "IDT,IDE" ce qui créé un problème de performance 
            string query = StrFunc.AppendFormat(@"select ev.UNIT, t.IDA_ENTITY as IDA_ENTITY, t.IDA_RISK ,t.IDB_RISK,
            sum(
                case 
                /* marginRequirement => Last */	 
	            when amt.EVENTTYPE = 'MGR' and t.IDT in ({1}) then 1
                /* cashBalancePayment */
	            when amt.EVENTTYPE = 'CBP' then 1
	            /* Previous CB  => First */
	            when amt.EVENTTYPE = 'PCB' and t.IDT in ({0}) then 1
                /* variationMargin */
	            when amt.EVENTTYPE = 'VMG' then 1
	            /* premium */
	            when amt.EVENTTYPE = 'PRM' then 1
	            /* cashSettlement */
	            when amt.EVENTTYPE = 'SCU' then 1
	            /* equalisationPayment */
	            when amt.EVENTTYPE = 'EQP' then 1
	            /* FEE and TAX   */
	            when amt.EVENTTYPE = 'FEE' then 1
                /* SKP and TAX   */
	            when amt.EVENTTYPE = 'SKP' then 1
                /* CashAvailable => Last */
                when amt.EVENTTYPE = 'CSA' and t.IDT in ({1}) then 1
	            /* cashUsed => Last */	 
                when amt.EVENTTYPE = 'CSU' and t.IDT in ({1}) then 1
                /* collateralAvailable => Last */	 
                when amt.EVENTTYPE = 'CLA' and t.IDT in ({1}) then 1
                /* collateralUsed => Last */	 
                when amt.EVENTTYPE = 'CLU' and t.IDT in ({1}) then 1
                /* uncoveredMarginRequirement =>  Last */	 
                when amt.EVENTTYPE = 'UMR' and t.IDT in ({1}) then 1
                /* marginCall =>  Last */ 
                when amt.EVENTTYPE = 'MGC' and t.IDT in ({1}) then 1
                /* cashBalance =>  Last */ 
                when amt.EVENTTYPE = 'CSB' and t.IDT in ({1}) then 1
                /* realizedMargin */ 
                when amt.EVENTTYPE = 'RMG' then 1
                when amt.EVENTTYPE = 'FUTRMG' then 1
                when amt.EVENTTYPE = 'OPTRMG_FSO' then 1
                when amt.EVENTTYPE = 'OPTRMG_PSO' then 1
                /* unrealizedMargin =>  Last */ 
                when amt.EVENTTYPE = 'UMG' and t.IDT in ({1}) then 1
                when amt.EVENTTYPE = 'FUTUMG' and t.IDT in ({1}) then 1
                when amt.EVENTTYPE = 'OPTUMG_FSO' and t.IDT in ({1}) then 1
                when amt.EVENTTYPE = 'OPTUMG_PSO' and t.IDT in ({1}) then 1
                /*liquidatingValueLong =>  Last */
                when amt.EVENTTYPE = 'OVL' and t.IDT in ({1}) then 1
                /*liquidatingValueShort =>  Last */
                when amt.EVENTTYPE = 'OVS' and t.IDT in ({1}) then 1
                /*marketValue =>  Last */
                when amt.EVENTTYPE = 'MKV' and t.IDT in ({1}) then 1
                /*funding*/
                when amt.EVENTTYPE = 'FDA' then 1
                /*borrowing*/
                when amt.EVENTTYPE = 'BWA' then 1
                /*unsettledCash  =>  Last */
                when amt.EVENTTYPE = 'UST'  and t.IDT in ({1}) then 1  
                /*forwardCashPayment =>  Last */
                when amt.EVENTTYPE = 'FCP'  and t.IDT in ({1}) then 1
                /*equityBalance =>  Last */
                when amt.EVENTTYPE = 'E_B' and t.IDT in ({1}) then 1
                /*equityBalanceWithForwardCash*/
                when amt.EVENTTYPE = 'EBF' and t.IDT in ({1}) then 1
                /*totalAccountValue =>  Last */
                when amt.EVENTTYPE = 'TAV' and t.IDT in ({1}) then 1
                /*excessDeficit =>  Last */
                when amt.EVENTTYPE = 'E_D' and t.IDT in ({1}) then 1
                /*excessDeficitWithForwardCash =>  Last*/
                when amt.EVENTTYPE = 'EDF' and t.IDT in ({1}) then 1
                else 0 end * amt.VALORISATION * (case when amt.IDA_PAY = t.IDA_RISK  then -1 else 1 end)
            ) as AMT,
            amt.EVENTTYPE,
            amt.FEEPAYMENTTYPE as FEEPAYMENTTYPE, 
            amt.FEEIDTAX as FEEIDTAX,
            amt.FEEIDTAXDET as FEEIDTAXDET,
            amt.FEETAXCOUNTRY as FEETAXCOUNTRY,
            amt.FEETAXTYPE as FEETAXTYPE,
            amt.FEETAXRATE as FEETAXRATE,
            amt.SKPPAYMENTTYPE as SKPPAYMENTTYPE, 
            amt.SKPIDTAX as SKPIDTAX,
            amt.SKPIDTAXDET as SKPIDTAXDET,
            amt.SKPTAXCOUNTRY as SKPTAXCOUNTRY,
            amt.SKPTAXTYPE as SKPTAXTYPE,
            amt.SKPTAXRATE as SKPTAXRATE
            from dbo.EVENT ev  
		    inner join dbo.TRADE t on (t.IDT = ev.IDT)
            inner join dbo.EVENT e_stream on (e_stream.IDT = ev.IDT) and (e_stream.EVENTCODE='CBS') and (e_stream.UNIT = ev.UNIT) and (e_stream.STREAMNO = ev.STREAMNO)
            {2}

            inner join (

                select ev.IDT, 
			    case 
                    when (ev.EVENTCODE='OPP') then 'FEE'
                    when (ev.EVENTCODE='SKP') then 'SKP'
                    when (ev.EVENTCODE='LPC' and ev.EVENTTYPE='RMG') then 'RMG'
			        when (ev.EVENTCODE='LFC' and ev.EVENTTYPE='RMG') then 'FUTRMG' 	
			        when (ev.EVENTCODE='LOC' and ev.EVENTTYPE='RMG' and e_parent.EVENTCODE='FSO' ) then 'OPTRMG_FSO' 	
			        when (ev.EVENTCODE='LOC' and ev.EVENTTYPE='RMG' and e_parent.EVENTCODE='PSO' ) then 'OPTRMG_PSO' 	
			 
			        when (ev.EVENTCODE='LPC' and ev.EVENTTYPE='UMG') then 'UMG' 	
			        when (ev.EVENTCODE='LFC' and ev.EVENTTYPE='UMG') then 'FUTUMG' 	
			        when (ev.EVENTCODE='LOC' and ev.EVENTTYPE='UMG' and e_parent.EVENTCODE='FSO' ) then 'OPTUMG_FSO' 	
			        when (ev.EVENTCODE='LOC' and ev.EVENTTYPE='UMG' and e_parent.EVENTCODE='PSO' ) then 'OPTUMG_PSO' 	
            
                    when (ev.EVENTCODE = 'INT' and ev.EVENTTYPE='INT') then 'SCU'   			 
                    when (ev.EVENTCODE = 'LPP' and ev.EVENTTYPE='GAM') then 'SCU'   

			        else ev.EVENTTYPE end as EVENTTYPE, 

		        case  when ev.EVENTCODE='OPP' and evfee.IDTAXDET is not null then isnull(evfee_parent.PAYMENTTYPE,'NotSpecified')
			          when ev.EVENTCODE='OPP' then isnull(evfee.PAYMENTTYPE,'NotSpecified') else 'N/A' end as FEEPAYMENTTYPE,
                case  when ev.EVENTCODE='OPP' then isnull(evfee.IDTAX,-1) else -1 end as FEEIDTAX,		
                case  when ev.EVENTCODE='OPP' then isnull(evfee.IDTAXDET,-1) else -1 end as FEEIDTAXDET,
                case  when ev.EVENTCODE='OPP' then isnull(evfee.TAXCOUNTRY,'N/A') else 'N/A' end as FEETAXCOUNTRY,
                case  when ev.EVENTCODE='OPP' then isnull(evfee.TAXTYPE,'N/A') else 'N/A' end as FEETAXTYPE,
                case  when ev.EVENTCODE='OPP' then isnull(evfee.TAXRATE,-1) else -1 end as FEETAXRATE,
		
                case  when ev.EVENTCODE='SKP' and evfee.IDTAXDET is not null then isnull(evfee_parent.PAYMENTTYPE,'NotSpecified')
			          when ev.EVENTCODE='SKP' then isnull(evfee.PAYMENTTYPE,'NotSpecified') else 'N/A' end as SKPPAYMENTTYPE,
                case  when ev.EVENTCODE='SKP' then isnull(evfee.IDTAX,-1) else -1 end as SKPIDTAX,		
                case  when ev.EVENTCODE='SKP' then isnull(evfee.IDTAXDET,-1) else -1 end as SKPIDTAXDET,
                case  when ev.EVENTCODE='SKP' then isnull(evfee.TAXCOUNTRY,'N/A') else 'N/A' end as SKPTAXCOUNTRY,
                case  when ev.EVENTCODE='SKP' then isnull(evfee.TAXTYPE,'N/A') else 'N/A' end as SKPTAXTYPE,
                case  when ev.EVENTCODE='SKP' then isnull(evfee.TAXRATE,-1) else -1 end as SKPTAXRATE,

		        ev.IDA_PAY, ev.IDB_PAY, ev.IDA_REC, ev.IDB_REC, ev.UNIT, isnull(ev.VALORISATION,0) as VALORISATION, 
                ev.STREAMNO,    
                e_parent.EVENTCODE as EVENT_PARENT
		        from dbo.EVENT ev
		        inner join dbo.TRADE t on t.IDT = ev.IDT 
                {2}
		        left outer join dbo.EVENTFEE evfee on evfee.IDE = ev.IDE 
		        inner join dbo.EVENT e_parent on (e_parent.IDE=ev.IDE_EVENT)
		        left outer join dbo.EVENTFEE evfee_parent on evfee_parent.IDE = ev.IDE_EVENT
		
		        where ( (ev.IDSTACTIVATION = 'REGULAR') or (ev.IDSTACTIVATION = 'DEACTIV' and ev.DTSTACTIVATION > @DTEND) ) and 
					        ((ev.EVENTCODE = 'LPC' and 
                                ev.EVENTTYPE in ('CBP','CLA','CLU','CSB','CSA','CSU','E_B','E_D','EBF','EDF','EQP','FCP','FDA','BWA','UST','MGC','MGR','OVL','OVS','PCB','PRM','RMG','SCU','UMG','UMR','VMG','MKV','TAV')) or
					            (ev.EVENTCODE in ('LFC','LOC') and ev.EVENTTYPE in ('RMG','UMG')) or
					            (ev.EVENTCODE in ('OPP','SKP') and (ev.IDA_PAY != ev.IDA_REC) ))
					 
            ) amt on (amt.IDT = ev.IDT) and (amt.UNIT = ev.UNIT) and (amt.STREAMNO = ev.STREAMNO)	
            
            where (ev.EVENTCODE = 'LPC') and (ev.EVENTTYPE = 'MGR') and ((ev.IDSTACTIVATION = 'REGULAR' ) or (ev.IDSTACTIVATION = 'DEACTIV' and ev.DTSTACTIVATION > @DTEND))
            group by  ev.UNIT, t.IDA_RISK, t.IDB_RISK, t.IDA_ENTITY, amt.EVENTTYPE, 
                      amt.FEEPAYMENTTYPE, amt.FEEIDTAX, amt.FEEIDTAXDET, amt.FEETAXCOUNTRY, amt.FEETAXTYPE, amt.FEETAXRATE,
                      amt.SKPPAYMENTTYPE, amt.SKPIDTAX, amt.SKPIDTAXDET, amt.SKPTAXCOUNTRY, amt.SKPTAXTYPE, amt.SKPTAXRATE",
            firsTrade.ToString(), lastTrade.ToString(), GetJoinTrade(pCS, idT.ToArray(), "t.IDT", false));

            QueryParameters qryParameters = new QueryParameters(pCS, query, dp);

            return qryParameters;
        }

        /// <summary>
        /// <para>Retourne un interval entre date1 et date2 (First)</para> 
        /// <para>Retourne un libellé en fonction de la culture retenue pour les périodes usuelles (1D, 1W, 1M, 1Y) (Second)</para>
        /// </summary>
        /// <param name="pDate1"></param>
        /// <param name="pDate2"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        private static Pair<Interval, string> GetDateTimeInterval(DateTime pDate1, DateTime pDate2, CultureInfo culture)
        {
            //// 1 mois
            //DateTime date1 = new DateTime(2015, 01, 01);
            //DateTime date2 = new DateTime(2015, 01, 31);
            //Interval interval = GetDateTimeInterval(date1, date2);

            //// 2 mois
            //date1 = new DateTime(2015, 01, 01);
            //date2 = new DateTime(2015, 02, 28);
            //interval = GetDateTimeInterval(date1, date2);

            //// 7 mois
            //date1 = new DateTime(2015, 10, 01);
            //date2 = new DateTime(2016, 04, 30);
            //interval = GetDateTimeInterval(date1, date2);

            //// 1 an
            //date1 = new DateTime(2015, 01, 01);
            //date2 = new DateTime(2015, 12, 31);
            //interval = GetDateTimeInterval(date1, date2);


            //// 2 an
            //date1 = new DateTime(2015, 01, 01);
            //date2 = new DateTime(2016, 12, 31);
            //interval = GetDateTimeInterval(date1, date2);

            //// 1 week
            //date1 = new DateTime(2015, 05, 04);
            //date2 = new DateTime(2015, 05, 08);
            //interval = GetDateTimeInterval(date1, date2);

            //// 1 week
            //date1 = new DateTime(2015, 05, 11);
            //date2 = new DateTime(2015, 05, 15);
            //interval = GetDateTimeInterval(date1, date2);

            //// 3 week
            //date1 = new DateTime(2015, 05, 04);
            //date2 = new DateTime(2015, 05, 22);
            //interval = GetDateTimeInterval(date1, date2);

            //// 4 week
            //date1 = new DateTime(2015, 05, 04);
            //date2 = new DateTime(2015, 05, 31);
            //interval = GetDateTimeInterval(date1, date2);  

            if (pDate1.CompareTo(pDate2) > 0)
            {
                DateTime dtTmp = pDate2;
                pDate2 = pDate1;
                pDate1 = dtTmp;
            }


            Pair<Interval, string> ret = null;
            //entre 1 et 11 mois ?
            if (pDate1.Day == 1)
            {
                for (int i = 0; i < 11; i++)
                {
                    int year = pDate1.AddMonths(i).Year;
                    int month = pDate1.AddMonths(i).Month;
                    int days = DateTime.DaysInMonth(year, month);

                    DateTime dtEnd = new DateTime(year, month, days);
                    if (dtEnd.CompareTo(pDate2) == 0)
                        ret = new Pair<Interval, string>(new Interval(PeriodEnum.M, i + 1), string.Empty);

                    if (null != ret)
                        break;
                }
            }

            //Nbr d'année ?
            if (null == ret)
            {
                if ((pDate1.Day == 1) && (pDate1.Month == 1) && (pDate2.Day == 31) && (pDate2.Month == 12))
                {
                    int i = pDate2.Year - pDate1.Year;
                    ret = new Pair<Interval, string>(new Interval(PeriodEnum.Y, i + 1), string.Empty);
                }
            }

            if (null == ret)
            {
                //entre 1 et 4 semaine ?
                for (int i = 0; i < 4; i++)
                {
                    if ((pDate1.DayOfWeek == DayOfWeek.Monday) && (((pDate2 - pDate1).Days == 4 + (i * 7)) || ((pDate2 - pDate1).Days == 6 + (i * 7))))
                        ret = new Pair<Interval, string>(new Interval(PeriodEnum.W, i + 1), string.Empty);

                    if (null != ret)
                        break;
                }
            }

            // par défaut différence en nbr de jour
            if (null == ret)
                ret = new Pair<Interval, string>(new Interval(PeriodEnum.D, (pDate2 - pDate1).Days + 1), string.Empty);

            if (ret.First.periodMultiplier.IntValue == 1)
            {
                switch (ret.First.period)
                {
                    case PeriodEnum.D:
                        ret.Second = Ressource.GetString("PERIODENUM_D", culture);
                        break;
                    case PeriodEnum.W:
                        ret.Second = Ressource.GetString("PERIODENUM_W", culture);
                        break;
                    case PeriodEnum.M:
                        ret.Second = Ressource.GetString("PERIODENUM_M", culture);
                        break;
                    case PeriodEnum.Y:
                        ret.Second = Ressource.GetString("PERIODENUM_Y", culture);
                        break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Génère une instance DetailedDateAmountReport à partir d'une instance de DetailedDateAmount
        /// </summary>
        /// <param name="pDetailedDateAmountReport"></param>
        /// <param name="pCurrency"></param>
        /// FI 20150622 [21124] Add Method
        private static DetailedDateAmountReport NewDetailedDateAmountToReport(DetailedDateAmount pDetailedDateAmount, string pCurrency)
        {
            DetailedDateAmountReport ret = new DetailedDateAmountReport();

            CopyAmountSideToReport(ret, pDetailedDateAmount, pCurrency);

            ret.valueDate = pDetailedDateAmount.ValueDate;

            ret.detailSpecified = pDetailedDateAmount.detailSpecified;
            if (ret.detailSpecified)
            {
                ret.detail = new ContractAmountReport[pDetailedDateAmount.detail.Count()];
                for (int i = 0; i < ArrFunc.Count(pDetailedDateAmount.detail); i++)
                {
                    ret.detail[i] = new ContractAmountReport();
                    CopyContractAmountToReport(ret.detail[i], pDetailedDateAmount.detail[i], pCurrency);
                }
            }

            return ret;
        }

        /// <summary>
        /// Retourne un array de DetailedDateAmountReport constitué de 4 éléments au maximum
        /// <para>le 4ème élément est le résultat d'un aggrégation du 4ème élément et des éléments postérieurs</para>
        /// </summary>
        /// <param name="pDetailedCashPositionReport"></param>
        /// <returns></returns>
        /// FI 20150622 [21124] Add method
        private static DetailedDateAmountReport[] ReduceArrayDetailedDateAmountReport(DetailedCashPositionReport pDetailedCashPositionReport)
        {
            DetailedDateAmountReport[] ret = null;

            if ((pDetailedCashPositionReport.dateDetailSpecified) && ArrFunc.Count(pDetailedCashPositionReport.dateDetail) > 4)
            {
                DateTime dtMax = (from item in pDetailedCashPositionReport.dateDetail
                                  select item.valueDate).OrderBy(orderby => orderby.Date).ToArray()[3];

                List<DetailedDateAmountReport> detail2 = (from item in pDetailedCashPositionReport.dateDetail
                                                          where (item.valueDate >= dtMax)
                                                          select item).ToList();
                decimal sum = (from item in detail2
                               select item).Sum(x => x.amount * ((x.side == CrDrEnum.CR) ? 1 : -1));

                DetailedDateAmountReport detailDtMax = new DetailedDateAmountReport
                {
                    currency = pDetailedCashPositionReport.currency,
                    amount = ((decimal)System.Math.Abs(sum)),
                    valueDate = dtMax
                };
                detailDtMax.sideSpecified = (detailDtMax.amount > 0);
                if (detailDtMax.sideSpecified)
                    detailDtMax.side = ((sum > 0) ? CrDrEnum.CR : CrDrEnum.DR);

                List<ContractAmountReport> lst = new List<ContractAmountReport>();
                foreach (DetailedDateAmountReport item in detail2)
                    lst.AddRange(item.detail);
                detailDtMax.detail = lst.ToArray();


                ret = new DetailedDateAmountReport[4];
                Array.Copy(pDetailedCashPositionReport.dateDetail, ret, 3);
                ret[3] = detailDtMax;
            }
            else
            {
                ret = pDetailedCashPositionReport.dateDetail;
            }

            return ret;
        }

        /// <summary>
        /// Ajout d'un item dans {lstTradesReport}
        /// </summary>
        /// <param name="pLstTradesReport"></param>
        /// <param name="pDate">Date d'observation</param>
        /// <param name="pDr">jeu de donnée avec les trades</param>
        /// FI 20150623 [21149] add Method
        /// FI 20150702 [XXXXX] Modify
        /// FI 20151019 [21317] Modify 
        /// FI 20161214 [21916] Modify
        /// FI 20170217 [22859] Modify
        // EG 20190730 ESE and DST use now TrdType and TrdSubType
        private static void AddTradesReport(string pCS, List<TradesReport> pLstTradesReport, DateTime pDate, DataRow[] pDr)
        {
            List<TradeReport> lstTradeOfDay = new List<TradeReport>();
            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
            TradesReport trades = pLstTradesReport.Find(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate));
            TradeReport trade = null;

            foreach (DataRow dr in pDr)
            {
                if (trades != null)
                {
                    trade = trades.trade.Find(x => x.OTCmlId == Convert.ToInt32(dr["IDT"]));
                    if (trade != null)
                        continue;
                }

                trade = new TradeReport();

                InitPosTradeCommon(trade, dr);

                trade.dtTransac = Convert.ToDateTime(dr["DTTRADE"]);
                trade.dtBusiness = Convert.ToDateTime(dr["DTBUSINESS"]);

                if (dr["DTSETTLT"] != Convert.DBNull)
                    trade.dtSettlt = Convert.ToDateTime(dr["DTSETTLT"]);

                trade.posResultSpecified = ("None" != Convert.ToString(dr["POSRESULT"]));
                if (trade.posResultSpecified)
                    trade.posResult = Convert.ToString(dr["POSRESULT"]);

                /// FI 20151019 [21317] Appel à InitTrdTypeTrdSubType 
                switch (trade.gProduct)
                {
                    case Cst.ProductGProduct_COM: // FI 20161214 [21916] add GProduct_COM
                    case Cst.ProductGProduct_OTC:
                    case Cst.ProductGProduct_FX:
                        InitTrdTypeTrdSubType(trade, dr, false);
                        break;
                    case Cst.ProductGProduct_FUT:
                        InitTrdTypeTrdSubType(trade, dr, true);
                        break;
                    case Cst.ProductGProduct_SEC: // FI 20150316 [20275] add case SEC
                        InitTrdTypeTrdSubType(trade, dr, true);
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("GPRODUCT:{0} is not implemented", trade.gProduct));
                }

                trade.prmSpecified = (dr["PRMAMOUNT"] != Convert.DBNull);
                if (trade.prmSpecified)
                {
                    trade.prm = new ReportAmountSideSettlementDate()
                    {
                        amount = Convert.ToDecimal(dr["PRMAMOUNT"]),
                        currency = Convert.ToString(dr["PRMUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["PRMSIDE"])),
                        // FI 20150702 [XXXXX] stlDtSpecified => false (la date de rglt de la prime est présente dans commonData.trade.stlDt)
                        //stlDt = DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(dr["PRMSTLDT"])),
                        //stlDtSpecified = ((trade.gProduct != Cst.ProductGProduct_FUT))
                        stlDtSpecified = false

                    };
                    trade.prm.sideSpecified = (trade.prm.amount > 0);
                }

                trade.gamSpecified = (dr["GAMAMOUNT"] != Convert.DBNull);
                if (trade.gamSpecified)
                {
                    trade.gam = new ReportAmountSideSettlementDate()
                    {
                        amount = Convert.ToDecimal(dr["GAMAMOUNT"]),
                        currency = Convert.ToString(dr["GAMUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["GAMSIDE"])),
                        // FI 20150702 [XXXXX] stlDtSpecified => false  (la date de rglt est présente dans commonData.trade.stlDt)
                        //stlDt = DtFunc.DateTimeToStringDateISO(Convert.ToDateTime(dr["GAMSTLDT"])),
                        stlDtSpecified = false
                    };
                    trade.gam.sideSpecified = (trade.gam.amount > 0);
                }

                // FI 20151019 [21317] Alimentation de PAM
                trade.pamSpecified = (dr["PAMAMOUNT"] != Convert.DBNull);
                if (trade.pamSpecified)
                {
                    trade.pam = new ReportAmountSide()
                    {
                        amount = Convert.ToDecimal(dr["PAMAMOUNT"]),
                        currency = Convert.ToString(dr["PAMUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["PAMSIDE"])),
                    };
                    trade.pam.sideSpecified = (trade.pam.amount > 0);
                }

                // FI 20151019 [21317] Alimentation de l'AIN
                trade.ainSpecified = (dr["AINAMOUNT"] != Convert.DBNull);
                if (trade.ainSpecified)
                {
                    trade.ain = new ReportAmountAccruedInterest()
                    {
                        amount = Convert.ToDecimal(dr["AINAMOUNT"]),
                        currency = Convert.ToString(dr["AINUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["AINSIDE"])),
                        daysSpecified = false
                    };
                    trade.ain.sideSpecified = (trade.ain.amount > 0);
                }

                // FI 20150218 [20275] Alimentation du trade source
                // FI 20170217 [22859] Ajout du trade source si cascading cascading/shifting
                trade.tradeSrcSpecified = (dr["SRC_IDT"] != Convert.DBNull); ;
                switch (trade.trdType)
                {
                    case TrdTypeEnum.PortfolioTransfer:
                        if (false == (trade.trdSubTypeSpecified && trade.trdSubType == TrdSubTypeEnum.InternalTransferOrAdjustment))
                            trade.tradeSrcSpecified = false; // On ne dévoile le trade d'origine s'il ne s'agit pas d'un transfert interne 
                        break;
                    default:
                        //nothing to do
                        break;
                }
                if (trade.tradeSrcSpecified)
                {
                    trade.tradeSrc = new TradeIdentification
                    {
                        OTCmlId = Convert.ToInt32(dr["SRC_IDT"]),
                        tradeIdentifier = Convert.ToString(dr["SRC_IDENTIFIER"])
                    };
                }

                lstTradeOfDay.Add(trade);
            }


            if (ArrFunc.IsFilled(lstTradeOfDay))
            {
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                //pLstTradesReport.Add(new TradesReport() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), trade = lstTradeOfDay });

                //TradesReport trades = pLstTradesReport.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                //if (null == trades)
                //    throw new NotSupportedException("no trade found");

                if (trades == null)
                {
                    trades = new TradesReport() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), trade = lstTradeOfDay };
                    pLstTradesReport.Add(trades);
                }
                else
                    trades.trade.AddRange(lstTradeOfDay);

                foreach (TradeReport item in lstTradeOfDay)
                    CalcTradesSubTotal(pCS, trades, item);

                List<PositionSubTotal> noQtyElement =
                    (from item in trades.subTotal
                     where ((item.@long.qty == 0) && (item.@short.qty == 0))
                     select item).ToList();

                if (noQtyElement.Count() > 0)
                {
                    foreach (PositionSubTotal item in noQtyElement)
                        trades.subTotal.Remove((PositionSubTotal)item);
                }

            }
        }

        /// <summary>
        /// Alimente les membres trdType et trdSubType de {pTrade}
        /// </summary>
        /// <param name="pTrade"></param>
        /// <param name="pDr">Jeu de donnée</param>
        /// <param name="isProductWithTrdType">
        /// <para>True si le produit possède en natif ces informations (Ex ETD, ou EquitySecurityTransaction)</para>
        /// <para>False Sinon</para>
        /// </param>
        /// FI 20151019 [21317] Add Methode
        private static void InitTrdTypeTrdSubType(TradeReport pTrade, DataRow pDr, Boolean isProductWithTrdType)
        {
            /***********************************************************************
             * 
             * ATTENTION Toute modification dans cette méthode a nécessairement des répercutions dans l'alimentation de POSACTION (voir méthode AddPosActions)
             * 
             ***********************************************************************/

            if (isProductWithTrdType)
            {
                pTrade.trdType = TrdTypeEnum.RegularTrade;
                if (pDr["TRDTYPE"] != Convert.DBNull)
                    pTrade.trdType = ReflectionTools.ConvertStringToEnum<TrdTypeEnum>(pDr["TRDTYPE"].ToString());

                pTrade.trdSubTypeSpecified = (pDr["TRDSUBTYPE"] != Convert.DBNull);
                if (pTrade.trdSubTypeSpecified)
                    pTrade.trdSubType = ReflectionTools.ConvertStringToEnum<TrdSubTypeEnum>(pDr["TRDSUBTYPE"].ToString());
            }
            else
            {
                pTrade.trdType = TrdTypeEnum.RegularTrade;
                pTrade.trdSubTypeSpecified = false;
                if ((pDr["SRC_IDT"] != Convert.DBNull))
                {
                    pTrade.trdType = TrdTypeEnum.PortfolioTransfer;
                    pTrade.trdSubTypeSpecified = true;
                    pTrade.trdSubType = TrdSubTypeEnum.InternalTransferOrAdjustment;// Un transfert est tjs considéré interne.
                }
                else if (pTrade.dtTransac < pTrade.dtBusiness)
                {
                    pTrade.trdType = TrdTypeEnum.LateTrade;
                }
            }
        }

        /// <summary>
        ///  Prise en compte du trade {pTrade} dans le calcul des prix moyens 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradesReport"></param>
        /// <param name="pTrade">Nouveau trade rentrant</param>
        /// FI 20150623 [21149] Add
        /// FI 20170116 [21916] Modify
        /// FI 20170228 [22859] Modify
        /// FI 20170203 [22859] Modify
        private static void CalcTradesSubTotal(string pCS, TradesReport pTradesReport, TradeReport pTrade)
        {
            if (null == pTradesReport)
                throw new ArgumentNullException("pTradesReport is null");

            if (null == pTrade)
                throw new ArgumentNullException("pTrade is null");


            // FI 20170116 [21916] Modify (nouvelle règle => Les prix moyens son calculés s'il existe une qté )
            // FI 20150331 [XXPOC]
            // Pas de sous-totaux pour les instruments non fungible (Ex GPRODUCT='FX')
            //if (pTrade.fungibilityMode == FungibilityModeEnum.NONE)
            //    return;
            if (false == pTrade.qtySpecified)
                return;

            PositionSubTotal subTotal = pTradesReport.subTotal.Find(x =>
               (x.idI == pTrade.idI) && (x.idAsset == pTrade.idAsset) && (x.assetCategory == pTrade.assetCategory)
                && (x.idb == pTrade.idb) && (x.predicateSpecified == false));

            if (null == subTotal)
            {
                subTotal = new PositionSubTotal();
                pTradesReport.subTotal.Add(subTotal);
                subTotal.idb = pTrade.idb;
                subTotal.idI = pTrade.idI;
                subTotal.idAsset = pTrade.idAsset;
                subTotal.assetCategory = pTrade.assetCategory;
            }
            CalcSubTotal(pCS, subTotal, pTrade);


            PositionSubTotal subTotal2 = pTradesReport.subTotal.Find(x =>
               (x.idI == pTrade.idI) && (x.idAsset == pTrade.idAsset) && (x.assetCategory == pTrade.assetCategory)
                && (x.idb == pTrade.idb) && (x.predicateSpecified == true));

            if (null == subTotal2)
            {
                // subTotal2
                subTotal2 = new PositionSubTotal();
                pTradesReport.subTotal.Add(subTotal2);
                subTotal2.idb = pTrade.idb;
                subTotal2.idI = pTrade.idI;
                subTotal2.idAsset = pTrade.idAsset;
                subTotal2.assetCategory = pTrade.assetCategory;
                subTotal2.predicateSpecified = true;

                // IMPORTANT - IMPORTANT - IMPORTANT  
                // Les types présents ici sont ceux qui seront affichés dans la section TRADECONFIRMATIONS
                // IMPORTANT - IMPORTANT - IMPORTANT  

                // RD 20160219 [21943] Add SplitTrade and MergedTrade
                // FI 20170228 [22859] Add Cascading and Shifting
                // FI 20170203 [22859] Rem Cascading and Shifting
                subTotal2.predicate = new Predicate[5]{ 
                            new Predicate(){ trdType = TrdTypeEnum.RegularTrade},
                            new Predicate(){ trdType = TrdTypeEnum.OTC},
                            new Predicate(){ trdType = TrdTypeEnum.BlockTrade},
                            new Predicate(){ trdType = TrdTypeEnum.SplitTrade},
                            new Predicate(){ trdType = TrdTypeEnum.MergedTrade}
                            /*new Predicate(){ trdType = TrdTypeEnum.Cascading},
                            new Predicate(){ trdType = TrdTypeEnum.Shifting}*/
                };
            }

            if (IsTradeMatch(pTrade, subTotal2.predicate))
                CalcSubTotal(pCS, subTotal2, pTrade);

        }

        /// <summary>
        ///  Prise en compte du trade {pPosTrade} dans le Calcul des cours moyen 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pPosTrade"></param>
        /// FI 20150623 [21149] add Method
        private static void CalcPosTradesSubSubTotal(string pCS, PosTrades pPosTrades, PosTrade pPosTrade)
        {
            if (null == pPosTrades)
                throw new ArgumentNullException("pPosTrades is null");

            if (null == pPosTrade)
                throw new ArgumentNullException("pPosTrade is null");


            // FI 20170116 [21916] Modify (nouvelle règle => Les prix moyens son calculés s'il existe une qté )
            // FI 20150331 [XXPOC]
            // Pas de sous-totaux pour les instruments non fungible (Ex GPRODUCT='FX')
            //if (pTrade.fungibilityMode == FungibilityModeEnum.NONE)
            //    return;
            if (false == pPosTrade.qtySpecified)
                return;


            PositionSubTotal subTotal = pPosTrades.subTotal.Find(x =>
                (x.idI == pPosTrade.idI) && (x.idAsset == pPosTrade.idAsset) && (x.assetCategory == pPosTrade.assetCategory)
                && (x.idb == pPosTrade.idb) && (x.predicateSpecified == false));

            if (null == subTotal)
            {
                subTotal = new PositionSubTotal
                {
                    idb = pPosTrade.idb,
                    idI = pPosTrade.idI,
                    idAsset = pPosTrade.idAsset,
                    assetCategory = pPosTrade.assetCategory
                };
                pPosTrades.subTotal.Add(subTotal);
            }

            CalcSubTotal(pCS, subTotal, pPosTrade);

        }

        /// <summary>
        ///  Almentation de {pTrade} à partir d'un jeu de résultat
        /// </summary>
        /// <param name="trade"></param>
        /// <param name="dr"></param>
        /// FI 20150623 [21149] add Method
        /// FI 20151019 [21317] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify
        private static void InitPosTradeCommon(PosTradeCommon pTrade, DataRow pDr)
        {
            if (null == pTrade)
                throw new ArgumentException("pTrade is null");
            if (null == pDr)
                throw new ArgumentException("pDr is null");


            pTrade.OTCmlId = Convert.ToInt32(pDr["IDT"]);
            pTrade.tradeIdentifier = Convert.ToString(pDr["IDENTIFIER"]);

            pTrade.sideSpecified = true;
            pTrade.side = (SideEnum)System.Enum.Parse(typeof(SideEnum), Convert.ToString(pDr["BUYSELL"]));

            pTrade.idbSpecified = true;
            pTrade.idb = Convert.ToInt32(pDr["IDB"]);

            pTrade.gProductSpecified = true;
            pTrade.gProduct = Convert.ToString(pDr["GPRODUCT"]);

            // FI 20151019 [21317] Alimentation de Family
            pTrade.familySpecified = true;
            pTrade.family = Convert.ToString(pDr["FAMILY"]);


            pTrade.fungibilityModeSpecified = true;
            pTrade.fungibilityMode = (FungibilityModeEnum)System.Enum.Parse(typeof(FungibilityModeEnum), Convert.ToString(pDr["FUNGIBILITYMODE"]));

            pTrade.idISpecified = true;
            pTrade.idI = Convert.ToInt32(pDr["IDI"]);

            pTrade.idAssetSpecified = (false == Convert.IsDBNull(pDr["IDASSET"]));
            if (pTrade.idAssetSpecified)
            {
                pTrade.idAsset = Convert.ToInt32(pDr["IDASSET"]);

                pTrade.assetCategorySpecified = true;
                pTrade.assetCategory = (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), Convert.ToString(pDr["ASSETCATEGORY"]));
            }

            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            pTrade.qtySpecified = (false == Convert.IsDBNull(pDr["QTY"]));
            if (pTrade.qtySpecified)
                pTrade.qty = Convert.ToDecimal(pDr["QTY"]);

            /* FI 20170116 [21916] Mise en commentaire => Déplacé sous Asset 
            // FI 20161214 [21916] Add QTYUNIT
            pTrade.qtyUnitSpecified = (false == Convert.IsDBNull(pDr["QTYUNIT"]));
            if (pTrade.qtyUnitSpecified)
                pTrade.qtyUnit = Convert.ToString(pDr["QTYUNIT"]);
             */

            pTrade.priceSpecified = (false == Convert.IsDBNull(pDr["PRICE"]));
            if (pTrade.priceSpecified)
                pTrade.price = Convert.ToDecimal(pDr["PRICE"]);

            // FI 20150522 [20275] add
            // FI 20150702 [XXXXX] DateTime.MaxValue.Date à la place de DateTime.MaxValue puisque DateTime.MaxValue possèse 23h59s59
            pTrade.dtOutSpecified = ((false == Convert.IsDBNull(pDr["DTOUTADJ"])) && (Convert.ToDateTime(pDr["DTOUTADJ"]).CompareTo(DateTime.MaxValue.Date) != 0));
            if (pTrade.dtOutSpecified)
                pTrade.dtOut = Convert.ToDateTime(pDr["DTOUTADJ"]);

        }


        /// <summary>
        /// Alimentation 
        /// <para>- des trades négociés en {pDate}</para>
        /// <para>- des trades en attente de règlement en {pDate}</para>
        /// <para>- des trades réglés en {pDate}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance">Trade CashBalance</param>
        /// <param name="pDate">date d'observation</param>
        /// <param name="pIsAddCommonDataTrade"></param>
        /// FI 20150707 [XXXXX] New
        /// FI 20200519 [XXXXX] Add pIsAddCommonDataTrade
        private void SetTradesReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate, Boolean pIsAddCommonDataTrade)
        {
            SetTradesReportSynthesis(pCS, pIdTCashBalance, pDate, true, pIsAddCommonDataTrade);
        }


        /// <summary>
        /// Alimentation 
        /// <para>- des trades négociés en {pDate}</para>
        /// <para>- des trades en attente de règlement en {pDate}</para>
        /// <para>- des trades réglés en {pDate}</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance">Trade CashBalance</param>
        /// <param name="pDate">date d'observation</param>
        /// <param name="pIsAddUnsettledTrades">permet de ne pas ajouter les unsettledTrades</param>
        /// FI 20150623 [21149] Add Method
        /// FI 20150707 [XXXXX] Modify
        /// FI 20151019 [21317] Modify 
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modity
        /// FI 20160412 [22069] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20200519 [XXXXX] Add parameter pIsAddCommonDataTrade
        private void SetTradesReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate, Boolean pIsAddUnsettledTrades, Boolean pIsAddCommonDataTrade)
        {
            if (productEnv.Count(true) > 0) // FI 20160225 [XXXXX] Add test, // FI 20160412 [22069] Excluding Cashpayment
            {
                // FI 20150622 [21124] utilisation de additionnalJoin
                string additionnalJoin = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = t.IDT 
                                                                        and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT";

                // FI 20151019 [21317] utilisation de productEnv.ExistSEC pour optimisation 
                // (Il est notamment inutile d'entreprendre l'alimentation des éléments unsettledTrades et settledTrades s'il n'y a pas d'activité Titre)
                // FI 20161214 [21916] productEnv.ExistSEC ou productEnv.ExistCOM
                /*
                Boolean isAll = productEnv.ExistSEC() || productEnv.ExistCOM();
                QueryParameters qry = GetQueryTradesReport2(pCS, _getInfoConnection,  additionnalJoin, pDate, (isAll ? TradeReportTypeEnum.all : TradeReportTypeEnum.tradeOfDay));
                */
                // FI 20190212 [24528] Utilisation de isSTL
                TradeReportTypeEnum tradeReportTypeEnum = TradeReportTypeEnum.tradeOfDay;
                Boolean isSTL = productEnv.ExistSEC() || productEnv.ExistCOM();
                if (isSTL)
                {
                    if (pIsAddUnsettledTrades)
                        tradeReportTypeEnum = TradeReportTypeEnum.tradeOfDay | TradeReportTypeEnum.unsetttledTrade | TradeReportTypeEnum.setttledTrade;
                    else
                        tradeReportTypeEnum = TradeReportTypeEnum.tradeOfDay | TradeReportTypeEnum.setttledTrade;
                }

                // FI 20190212 [24528] La requête ne remonte que le strict nécessaire
                QueryParameters qry = GetQueryTradesReport3(pCS,  additionnalJoin, pDate, tradeReportTypeEnum, productEnv);
                qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());


                // FI 20151019 [21317] Usage de la colonne TRADEREPORTTYPE pour appliquer les restrictions 
                //Ajout des trades jours
                DataRow[] row = dt.Select(StrFunc.AppendFormat("[TRADEREPORTTYPE]='{0}'", TradeReportTypeEnum.tradeOfDay.ToString()));
                if (ArrFunc.IsFilled(row))
                {
                    AddTradesReport(pCS, NotificationDocument.Trades, pDate, row);
                    NotificationDocument.TradesSpecified = true;
                }


                if (isSTL)
                {
                    // FI 20150707 [XXXXX] use pIsAddUnsettledTrades
                    if (pIsAddUnsettledTrades)
                    {
                        //Ajout des trades unsettled
                        row = dt.Select(StrFunc.AppendFormat("[TRADEREPORTTYPE]='{0}'", TradeReportTypeEnum.unsetttledTrade.ToString()));
                        if (ArrFunc.IsFilled(row))
                        {
                            AddTradesReport(pCS, NotificationDocument.UnsettledTrades, pDate, row);
                            NotificationDocument.UnsettledTradesSpecified = true;
                        }
                    }

                    //Ajout des trades settled
                    row = dt.Select(StrFunc.AppendFormat("[TRADEREPORTTYPE]='{0}'", TradeReportTypeEnum.setttledTrade.ToString()));
                    if (ArrFunc.IsFilled(row))
                    {
                        AddTradesReport(pCS, NotificationDocument.SettledTrades, pDate, row);
                        NotificationDocument.SettledTradesSpecified = true;
                    }
                }

                // FI 20200519 [XXXXX] Alimentation si pIsAddCommonDataTrade
                if (pIsAddCommonDataTrade)
                {
                    List<TradeReport> tradesReport = GetAllTrades(pDate);
                    foreach (TradeReport item in tradesReport)
                        AddCommonDataTrade(pCS, item);
                }
            }
        }

        /// <summary>
        /// Sur les trades debtSecurityTransaction, détermination du nombre de jours des intérêts courus entre la date de détachemment du dernier coupon 
        /// et la date de règlement
        /// </summary>
        // FI 20151019 [21317]
        private void SetAinDaysInDebtSecurityTransaction(string pCS)
        {
            Boolean isExistTrade = GetAllTrades(null).Exists(x => x.gProductSpecified && x.family == Cst.ProductFamily_DSE);
            if (isExistTrade)
            {
                List<TradeReport> lstTrade = GetAllTrades(null).Where(x => x.gProductSpecified && x.family == Cst.ProductFamily_DSE && x.ainSpecified).ToList();
                int[] idAsset = (from item in lstTrade select new { item.idAsset }).Distinct().Select(x => x.idAsset).ToArray();

                string query = StrFunc.AppendFormat(@"select  e.IDT, e.DTSTARTADJ, e.DTENDADJ
                                    from dbo.EVENT e 
                                    where  {0}
                                    and e.EVENTCODE = 'INT' and e.EVENTTYPE = 'INT' order by e.IDT asc, e.DTSTARTADJ desc",
                                    DataHelper.SQLColumnIn(pCS, "e.IDT", idAsset, TypeData.TypeDataEnum.@int));

                QueryParameters qryParameters = new QueryParameters(pCS, query, new DataParameters());
                DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (ArrFunc.IsFilled(dt.Rows))
                {
                    foreach (TradeReport item in lstTrade)
                    {
                        DataRow[] rows = dt.Select(StrFunc.AppendFormat("IDT={0} and DTSTARTADJ<=#{1}# and DTENDADJ>#{1}#",
                            item.idAsset.ToString(), DtFunc.DateTimeToStringDateISO(item.dtSettlt)));

                        if (ArrFunc.IsFilled(rows))
                        {
                            item.ain.days = (item.dtSettlt - Convert.ToDateTime(rows[0]["DTSTARTADJ"])).Days;
                            item.ain.daysSpecified = true;
                        }
                    }
                }
            }
        }



        /// <summary>
        /// Retourne la liste des trades présents dans les éléments trades, unsettledTrades, settledTrades
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150623 [21149] Add Method
        private List<TradeReport> GetAllTrades(Nullable<DateTime> pDate)
        {
            List<TradeReport> ret = new List<TradeReport>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.Trades.Where(x => x.bizDt == sDate)
                    .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() })
                    .FirstOrDefault().trade.Concat(
                            NotificationDocument.UnsettledTrades.Where(x => x.bizDt == sDate)
                            .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() })
                            .FirstOrDefault().trade).Concat(
                                        NotificationDocument.SettledTrades.Where(x => x.bizDt == sDate)
                                        .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() })
                                        .FirstOrDefault().trade)).ToList();
            }
            else
            {
                ret = (from itemTrades in NotificationDocument.Trades.Concat(NotificationDocument.UnsettledTrades).Concat(NotificationDocument.SettledTrades)
                       from item in itemTrades.trade
                       select item).ToList();

            }

            return ret;
        }


        /// <summary>
        /// Retourne la liste des trades présents dans les éléments posTrades et stlPosTrades
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150623 [21149] Add Method
        private List<PosTrade> GetAllPosTrades(Nullable<DateTime> pDate)
        {
            List<PosTrade> ret = new List<PosTrade>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.PosTrades.Where(x => x.bizDt == sDate)
                    .DefaultIfEmpty(new PosTrades() { bizDt = sDate, trade = new List<PosTrade>() })
                    .FirstOrDefault().trade.Concat(
                            NotificationDocument.StlPosTrades.Where(x => x.bizDt == sDate)
                            .DefaultIfEmpty(new PosTrades() { bizDt = sDate, trade = new List<PosTrade>() })
                            .FirstOrDefault().trade)).ToList();
            }
            else
            {
                ret = (from itemTrades in NotificationDocument.PosTrades.Concat(NotificationDocument.StlPosTrades)
                       from item in itemTrades.trade
                       select item).ToList();
            }

            return ret;
        }

        /// <summary>
        /// Retourne la liste des trades présents dans les éléments posActions et stlPosActions
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150623 [21149] Add Method
        private List<PosAction> GetAllPosAction(Nullable<DateTime> pDate)
        {
            List<PosAction> ret = new List<PosAction>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.PosActions.Where(x => x.bizDt == sDate)
                    .DefaultIfEmpty(new PosActions() { bizDt = sDate, posAction = new List<PosAction>() })
                    .FirstOrDefault().posAction.Concat(
                            NotificationDocument.StlPosActions.Where(x => x.bizDt == sDate)
                            .DefaultIfEmpty(new PosActions() { bizDt = sDate, posAction = new List<PosAction>() })
                            .FirstOrDefault().posAction)).ToList();
            }
            else
            {
                ret = (from itemPos in NotificationDocument.PosActions.Concat(NotificationDocument.StlPosActions)
                       from item in itemPos.posAction
                       select item).ToList();
            }

            return ret;
        }

        /// <summary>
        /// Retourne la liste des trades présents dans les éléments posSynthetics et stlposSynthetics
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150709 [XXXXX] Add
        private List<PosSynthetic> GetAllPositionSynthetic(Nullable<DateTime> pDate)
        {
            List<PosSynthetic> ret = new List<PosSynthetic>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.PosSynthetics.Where(x => x.bizDt == sDate)
                    .DefaultIfEmpty(new PosSynthetics() { bizDt = sDate, posSynthetic = new List<PosSynthetic>() })
                    .FirstOrDefault().posSynthetic.Concat(
                            NotificationDocument.StlPosSynthetics.Where(x => x.bizDt == sDate)
                            .DefaultIfEmpty(new PosSynthetics() { bizDt = sDate, posSynthetic = new List<PosSynthetic>() })
                            .FirstOrDefault().posSynthetic)).ToList();
            }
            else
            {
                ret = (from itemPos in NotificationDocument.PosSynthetics.Concat(NotificationDocument.StlPosSynthetics)
                       from item in itemPos.posSynthetic
                       select item).ToList();
            }

            return ret;
        }



        /// <summary>
        /// Retourne la liste des trades présents dans l'élément dlvTrades
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20170217 [22862] Add
        private List<DlvTrade> GetDeliveryTrade(Nullable<DateTime> pDate)
        {
            List<DlvTrade> ret = new List<DlvTrade>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.DlvTrades.Where(x => x.bizDt == sDate)
                    .DefaultIfEmpty(new DlvTrades() { bizDt = sDate, trade = new List<DlvTrade>() }).FirstOrDefault().trade).ToList();
            }
            else
            {
                ret = (from itemDlv in NotificationDocument.DlvTrades
                       from item in itemDlv.trade
                       select item).ToList();
            }

            return ret;
        }


        /// <summary>
        /// <para>Alimentation des actions sur positions en date {pDate} (lorsque le message est de type NotificationTypeEnum.SYNTHESIS)</para>
        /// <para>Alimente NotificationDocument.posActions et NotificationDocument.stlPosActions</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20150623 [21149] Add method
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modity
        /// FI 20160412 [22069] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170217 [22859] Modify
        /// FI 20170524 [XXXXX] Modify 
        /// FI 20200519 [XXXXX] Add pIsAddCommonDataTrade
        private void SetPosActionsReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate, Boolean pIsAddCommonDataTrade)
        {
            // FI 20160225 [XXXXX] Add test 
            // FI 20160412 [22069] Excluding CasPayment
            // FI 20161214 [21916] Add ExistFungibilityProduct
            if (productEnv.Count(true) > 0 && productEnv.ExistFungibilityProduct())
            {
                // FI 20170217 [22859] Modification de la restriction (varaible additionnalJoin)
                // Les trades sources d'un Cascading, Shifting ne possèdent pas de flux (sauf s'il existe des frais) et sont absents du trades CB   
                // Modification de la requête pour les prendre en considération 
                /*
                string additionnalJoin = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = tr.IDT 
                                                                        and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT";
                 */

                // FI 20170524 [XXXXX] Modify
                // additionnalJoin commence par where (avant il y avait un retour chariot avant)
                // => additionnalJoin doit commencer par where car ds GetQueryPosAction on test pAdditionalJoin.StartsWith("where")
                // D'autres part correction d'une énorme boulette (il y avait un codage en dur sur l'IDT=1079, remplacement de cette valeur par @IDT)

                string additionnalJoin = StrFunc.AppendFormat(@"where exists(  
	select 1 
	from dbo.TRADELINK tlcb where tlcb.IDT_B = tr.IDT 
    and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT
    union all
    select 1 
	from  dbo.TRADELINK tlcas 
	inner join dbo.TRADELINK tlcb on tlcb.IDT_B = tlcas.IDT_A
	and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT
	where tlcas.IDT_B = tr.IDT and  tlcas.LINK in ('{0}','{1}')
)", TradeLinkType.PositionAfterCascading, TradeLinkType.PositionAfterShifting);


                //Alimentation de NotificationDocument.posActions
                QueryParameters qry = GetQueryPosAction(pCS, pDate, additionnalJoin, PostionType.Business);
                qry.Parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdTCashBalance);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                DataRow[] row = dt.Select();
                if (ArrFunc.IsFilled(row))
                {
                    AddPosActions(pCS, NotificationDocument.PosActions, pDate, row, NotificationDocument.EfsMLversion);
                    NotificationDocument.PosActionsSpecified = true;
                }

                /// FI 20151019 [21317] stlPosActions alimenté uniquement s'il existe des opérations sur titres (Equity ou DebtSecurity) 
                Boolean isExistSEC = productEnv.ExistSEC();
                if (isExistSEC)
                {
                    //Alimentation de NotificationDocument.stlPosActions
                    qry = GetQueryPosAction(pCS, pDate, additionnalJoin, PostionType.Settlement);
                    qry.Parameters.Add(new DataParameter(pCS, "IDT", DbType.Int32), pIdTCashBalance);

                    dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                    row = dt.Select();
                    if (ArrFunc.IsFilled(row))
                    {
                        AddPosActions(pCS, NotificationDocument.StlPosActions, pDate, row, NotificationDocument.EfsMLversion);
                        NotificationDocument.StlPosActionsSpecified = true;
                    }
                }

                // FI 20200519 [XXXXX] Alimentation si pIsAddCommonDataTrade
                if (pIsAddCommonDataTrade)
                {
                    List<PosAction> posactions = GetAllPosAction(pDate);
                    foreach (PosAction posAction in posactions)
                    {
                        AddCommonDataTrade(pCS, posAction.trades.trade);
                        if (posAction.trades.trade2Specified)
                            AddCommonDataTrade(pCS, posAction.trades.trade2);
                    }
                }
            }
        }

        /// <summary>
        /// Ajoute un item à {pLstPosActions}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstPosActions">liste des actions sur positions par date</param>
        /// <param name="pDate">Date d'observation</param>
        /// <param name="pDr">Liste des enregistrements pris en considération à la date d'observation</param>
        /// <param name="pVersion">Version du document</param>
        /// FI 20151019 [21317] Modify 
        private static void AddPosActions(string pCS, List<PosActions> pLstPosActions, DateTime pDate, DataRow[] pDr, EfsMLDocumentVersionEnum pVersion)
        {
            List<PosAction> lstPosAction = new List<PosAction>();
            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
            PosActions posActions = pLstPosActions.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
            PosAction posAction = null;

            foreach (DataRow dr in pDr)
            {
                if (posActions != null)
                {
                    posAction = posActions.posAction.Find(x => x.IdPosActionDet == Convert.ToInt32(dr["IDPADET"]));
                    if (posAction != null)
                        continue;
                }

                int idT = Convert.ToInt32(dr["TRADE_IDT"]);

                posAction = new PosAction
                {
                    IdPosActionDet = Convert.ToInt32(dr["IDPADET"]),
                    requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(dr["REQUESTTYPE"].ToString()),
                    requestMode = ReflectionTools.ConvertStringToEnum<SettlSessIDEnum>(dr["REQUESTMODE"].ToString()),
                    dtBusiness = DtFunc.DateTimeToString(Convert.ToDateTime(dr["DTBUSINESS"]), DtFunc.FmtISODate),
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    qty = Convert.ToDecimal(dr["QTY"]),
                    // FI 20200820 [25468] Dates systemes en UTC (DTSYS est une date UTC)
                    dtSys = DateTime.SpecifyKind(Convert.ToDateTime(dr["DTSYS"]), DateTimeKind.Utc)
                };

                if (BoolFunc.IsTrue(dr["DEACTIV"].ToString()))
                {
                    posAction.dtUnclearingSpecified = true;
                    if (posAction.dtUnclearingSpecified)
                        posAction.dtUnclearing = DtFunc.DateTimeToString(Convert.ToDateTime(dr["DTUNCLEARING"]), DtFunc.FmtISODate);
                }
                //
                PosActionTrade trade = posAction.trades.trade;
                trade.OTCmlId = idT;
                trade.tradeIdentifier = Convert.ToString(dr["TRADE_IDENTIFIER"]);
                trade.idAsset = Convert.ToInt32(dr["IDASSET"]);
                // FI 20140820 [20275] Alimentation de assetCategory
                trade.assetCategory = ReflectionTools.ConvertStringToEnum<Cst.UnderlyingAsset>(dr["ASSETCATEGORY"].ToString());
                trade.price = (Convert.IsDBNull(dr["PRICE"]) ? 0 : Convert.ToDecimal(dr["PRICE"]));
                trade.gProduct = Convert.ToString(dr["GPRODUCT"]);
                trade.family = Convert.ToString(dr["FAMILY"]);
                // FI 20150522 [20275] Alimentation de trade.dtOut
                if (false == Convert.IsDBNull(dr["DTOUTADJ"]) && (Convert.ToDateTime(dr["DTOUTADJ"]).CompareTo(DateTime.MaxValue) != 0))
                    trade.dtOut = Convert.ToDateTime(dr["DTOUTADJ"]);

                posAction.trades.trade2Specified = (dr["TRADE2_IDT"] != Convert.DBNull);
                if (posAction.trades.trade2Specified && posAction.requestType == Cst.PosRequestTypeEnum.PositionTransfer)
                {
                    Nullable<TrdTypeEnum> trdTyp2 = null;
                    Nullable<TrdSubTypeEnum> trdSubTyp2 = null;
                    switch (trade.gProduct)
                    {
                        case Cst.ProductGProduct_FUT:
                        case Cst.ProductGProduct_SEC:
                            // FI 20150316 [20275] add case SEC
                            // FI 20151019 [21317] gestion DSE
                            if (trade.family == Cst.ProductFamily_LSD || trade.family == Cst.ProductFamily_ESE)
                            {
                                if (false == Convert.IsDBNull(dr["TRDTYPE2"]))
                                    trdTyp2 = ReflectionTools.ConvertStringToEnum<TrdTypeEnum>(dr["TRDTYPE2"].ToString());
                                if (false == Convert.IsDBNull(dr["TRDSUBTYPE2"]))
                                    trdSubTyp2 = ReflectionTools.ConvertStringToEnum<TrdSubTypeEnum>(dr["TRDSUBTYPE2"].ToString());
                            }
                            else if (trade.family == Cst.ProductFamily_DSE)
                            {
                                // Un tensfert est néessairement interne
                                trdTyp2 = TrdTypeEnum.PortfolioTransfer;
                                trdSubTyp2 = TrdSubTypeEnum.InternalTransferOrAdjustment;
                            }
                            else
                            {
                                string logTrade = LogTools.IdentifierAndId(trade.tradeIdentifier, trade.OTCmlId);
                                throw new NotImplementedException(StrFunc.AppendFormat("Family(identifier:{0}) is not implemeted. Error occurs when loading trade {1}", trade.family, logTrade));
                            }
                            break;

                        case Cst.ProductGProduct_OTC: // Un tensfert est néessairement interne
                            trdTyp2 = TrdTypeEnum.PortfolioTransfer;
                            trdSubTyp2 = TrdSubTypeEnum.InternalTransferOrAdjustment;
                            break;
                        default:
                            {
                                string logTrade = LogTools.IdentifierAndId(trade.tradeIdentifier, trade.OTCmlId);
                                throw new NotImplementedException(StrFunc.AppendFormat("GPRODUCT:{0} is not implemented. Error occurs when loading trade {1}", trade.family, logTrade));
                            }
                    }
                    // FI 20150218 [20275] Le trade2 est ajouté uniquement si transfertInterne 
                    posAction.trades.trade2Specified = (trdTyp2.HasValue && trdTyp2.Value == TrdTypeEnum.PortfolioTransfer &&
                        trdSubTyp2.HasValue && trdSubTyp2.Value == TrdSubTypeEnum.InternalTransferOrAdjustment);
                }

                if (posAction.trades.trade2Specified)
                {
                    PosActionTrade trade2 = posAction.trades.trade2;
                    trade2.OTCmlId = Convert.ToInt32(dr["TRADE2_IDT"]);
                    trade2.tradeIdentifier = Convert.ToString(dr["TRADE2_IDENTIFIER"]);
                    trade2.idAsset = Convert.ToInt32(dr["IDASSET2"]);
                    trade2.assetCategory = ReflectionTools.ConvertStringToEnum<Cst.UnderlyingAsset>(dr["ASSETCATEGORY2"].ToString());
                    trade2.price = (Convert.IsDBNull(dr["PRICE2"]) ? 0 : Convert.ToDecimal(dr["PRICE2"]));
                    // FI 20150522 [20275] Alimentation de trade2.dtOut
                    if (false == Convert.IsDBNull(dr["DTOUTADJ2"]) && (Convert.ToDateTime(dr["DTOUTADJ2"]).CompareTo(DateTime.MaxValue) != 0))
                        trade2.dtOut = Convert.ToDateTime(dr["DTOUTADJ2"]);
                }

                // RD 20130722 [18745] Synthesis Report
                if (pVersion >= EfsMLDocumentVersionEnum.Version35)
                {
                    posAction.underlyerPriceSpecified = (dr["UNDERLYERPRICE"] != Convert.DBNull);
                    if (posAction.underlyerPriceSpecified)
                    {
                        PosActionTrade posTrade = posAction.trades.trade;
                        posAction.underlyerPrice = Convert.ToDecimal(dr["UNDERLYERPRICE"]);
                        posAction.fmtUnderlyerPriceSpecified = true;
                        posAction.fmtUnderlyerPrice = BuildFmtPrice(CSTools.SetCacheOn(pCS),
                            new Pair<Cst.UnderlyingAsset, int>(posTrade.assetCategory, posTrade.idAsset),
                                posAction.underlyerPrice, 100);
                    }
                }
                lstPosAction.Add(posAction);
            }

            if (ArrFunc.IsFilled(lstPosAction))
            {
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
                //pLstPosActions.Add(new PosActions() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), posAction = lstPosAction });
                if (posActions == null)
                {
                    posActions = new PosActions() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), posAction = lstPosAction };
                    pLstPosActions.Add(posActions);
                }
                else
                    posActions.posAction.AddRange(lstPosAction);

                //Dans la version <= 3.0, les frais, le RMG et le SCU sont dans les évènements 
                if (pVersion >= EfsMLDocumentVersionEnum.Version35)
                {
                    // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
                    //PosActions posActions = pLstPosActions.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                    //if (null == posActions)
                    //    throw new NotSupportedException("posAction is null ");

                    //List<IPosactionTradeFee> posActionsFee = (from item in posActions.posAction select (IPosactionTradeFee)item).ToList();

                    //SetPosActionFee(pCS, posActionsFee);
                    //SetPosActionEventType(pCS, posActions.posAction);

                    List<IPosactionTradeFee> posActionsFee = (from item in lstPosAction select (IPosactionTradeFee)item).ToList();

                    SetPosActionFee(pCS, posActionsFee);
                    SetPosActionEventType(pCS, lstPosAction);
                }
            }
        }


        /// <summary>
        /// Alimentation des frais sur les trades du jour
        /// <para>Les frais sont récupérés sur les otherPartyPayment des trades (lecture du flux EfsML)</para>
        /// </summary>
        /// FI 20150702 [XXXXX] Add
        /// FI 20150707 [XXXXX] Modidfy
        private void SetNetTrades()
        {
            if (NotificationDocument.TradesSpecified || NotificationDocument.UnsettledTradesSpecified || NotificationDocument.SettledTradesSpecified)
            {
                // FI 20150707 [XXXXX] Le net est calculé s'il n'existea pas de frais ou si touls les frais sont dans la devise du GAM
                List<TradeReport> trades = (from item in GetAllTrades(null).Where(t => t.gamSpecified &&
                                                                                            (
                                                                                            (t.feeSpecified == false)
                                                                                            ||
                                                                                            ((t.feeSpecified) &&
                                                                                             (t.fee.Count() == t.fee.Where(f => f.currency == t.gam.currency).Count()))
                                                                                            ))
                                            select item).ToList();

                foreach (TradeReport trade in trades)
                {
                    trade.netSpecified = true;

                    trade.net = new ReportAmountSideSettlementDate
                    {
                        stlDtSpecified = false,
                        currency = trade.gam.currency
                    };

                    if (trade.feeSpecified)
                    {
                        decimal sumResult = (from fee in trade.fee.Where(f => f.currency == trade.gam.currency) select fee).Sum(f => (f.side == CrDrEnum.CR) ? f.amount : -1 * f.amount);
                        trade.net.amount = ((trade.gam.side == CrDrEnum.CR) ? trade.gam.amount : (-1) * trade.gam.amount) + sumResult;

                        trade.net.sideSpecified = (trade.net.amount != decimal.Zero);
                        if (trade.net.sideSpecified)
                        {
                            trade.net.side = (trade.net.amount > 0) ? CrDrEnum.CR : CrDrEnum.DR;
                            trade.net.amount = System.Math.Abs(trade.net.amount);
                        }
                    }
                    else
                    {
                        trade.net.amount = trade.gam.amount;
                        trade.net.sideSpecified = trade.gam.sideSpecified;
                        if (trade.net.sideSpecified)
                            trade.net.side = trade.gam.side;

                    }
                }
            }
        }


        /// <summary>
        /// Alimentation des positions synthetiques en date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Représente tous les trades qui constituent la position</param>
        /// <param name="pDate"></param>
        /// FI 20151019 [] Modify
        private void SetPosSynthetics(string pCS, int[] pIdT, DateTime pDate)
        {
            string additionalJoin = GetJoinTrade(pCS, pIdT, "t.IDT", false);

            // Recherche des positons synthetiques 
            QueryParameters qry = GetQueryPositionSynthetic(pCS, pDate, PostionType.Business, additionalJoin, null);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            DataRow[] row = dt.Select();
            if (ArrFunc.IsFilled(row))
            {
                AddPosSynthetics(pCS, NotificationDocument.PosSynthetics, pDate, row);
                NotificationDocument.PosSyntheticsSpecified = true;
            }

        }


        /// <summary>
        /// Alimentation des positions synthetiques en date {pDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT">Représente tous les trades qui constituent la position</param>
        /// <param name="pDate"></param>
        /// FI 20150709 [XXXXX] Modify
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modity
        /// FI 20160412 [22069] Modify
        /// FI 20161214 [21916] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetPosSyntheticsReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate)
        {
            // FI 20160225 [XXXXX] Add Test 
            // FI 20160412 [22069] Excluding CashPayment 
            // FI 20161214 [21916] Modify
            if (productEnv.Count(true) > 0 && productEnv.ExistFungibilityProduct())
            {
                string additionalJoin = @"inner join dbo.TRADELINK tlcb on (tlcb.IDT_B = t.IDT) and (tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance') and (tlcb.IDT_A = @IDT)";

                // Recherche des positons synthetiques 
                QueryParameters qry = GetQueryPositionSynthetic(pCS, pDate, PostionType.Business, additionalJoin, this.productEnv);
                qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                DataRow[] row = dt.Select();
                if (ArrFunc.IsFilled(row))
                {
                    AddPosSynthetics(pCS, NotificationDocument.PosSynthetics, pDate, row);
                    NotificationDocument.PosSyntheticsSpecified = true;
                }


                // FI 20151019 [21317]  Add isExistSEC
                bool isExistSEC = productEnv.ExistSEC();
                if (isExistSEC)
                {
                    // FI 20150709 [XXXXX] Alimentation de stlPosSynthetics
                    // Recherche des positons synthetiques en date de règlement
                    qry = GetQueryPositionSynthetic(pCS, pDate, PostionType.Settlement, additionalJoin, productEnv);
                    qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                    dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                    row = dt.Select();
                    if (ArrFunc.IsFilled(row))
                    {
                        AddPosSynthetics(pCS, NotificationDocument.StlPosSynthetics, pDate, row);
                        NotificationDocument.StlPosSyntheticsSpecified = true;
                    }

                }
            }
        }

        /// <summary>
        /// Ajout d'un item dans {pLstPosSynthetics}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pLstPosSynthetics"></param>
        /// <param name="pDate"></param>
        /// <param name="pDr"></param>
        /// FI 20150707 [XXXXX] Add 
        private void AddPosSynthetics(string pCS, List<PosSynthetics> pLstPosSynthetics, DateTime pDate, DataRow[] pDr)
        {

            List<PosSynthetic> lstPosSynt = new List<PosSynthetic>();
            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
            PosSynthetics posSynthetics = pLstPosSynthetics.Find(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate));
            PosSynthetic posSynthetic = null;

            foreach (DataRow dr in pDr)
            {

                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
                Pair<Cst.UnderlyingAsset, int> asset = new Pair<Cst.UnderlyingAsset, int>(
                    (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), Convert.ToString(dr["ASSETCATEGORY"])), Convert.ToInt32(dr["IDASSET"]));

                int side = Convert.ToInt32(dr["SIDE"]);

                PositionKey posKey = new PositionKey()
                {
                    idA_Dealer = Convert.ToInt32(dr["IDA_DEALER"]),
                    idB_Dealer = Convert.ToInt32(dr["IDB_DEALER"]),
                    idA_Clearer = Convert.ToInt32(dr["IDA_CLEARER"]),
                    idB_Clearer = Convert.ToInt32(dr["IDB_CLEARER"]),
                    idI = Convert.ToInt32(dr["IDI"]),
                    idAsset = asset.Second,
                    assetCategory = asset.First
                };

                if (posSynthetics != null)
                {
                    posSynthetic = posSynthetics.posSynthetic.Find(x =>
                        x.posKey.idA_Dealer == posKey.idA_Dealer &&
                        x.posKey.idB_Dealer == posKey.idB_Dealer &&
                        x.posKey.idA_Clearer == posKey.idA_Clearer &&
                        x.posKey.idB_Clearer == posKey.idB_Clearer &&
                        x.posKey.idI == posKey.idI &&
                        x.posKey.idAsset == posKey.idAsset &&
                        x.posKey.assetCategory == posKey.assetCategory &&
                        x.side == side);

                    if (posSynthetic != null)
                        continue;
                }

                //Pair<Cst.UnderlyingAsset, int> asset = new Pair<Cst.UnderlyingAsset, int>(
                //                   (Cst.UnderlyingAsset)System.Enum.Parse(typeof(Cst.UnderlyingAsset), Convert.ToString(dr["ASSETCATEGORY"])), Convert.ToInt32(dr["IDASSET"]));

                //PosSynthetic posSynthetic = new PosSynthetic();
                //// FI 20150113 [20672] Alimentation de posSynthetic.posKey
                //posSynthetic.posKey.idA_Dealer = Convert.ToInt32(dr["IDA_DEALER"]);
                //posSynthetic.posKey.idB_Dealer = Convert.ToInt32(dr["IDB_DEALER"]);
                //posSynthetic.posKey.idA_Clearer = Convert.ToInt32(dr["IDA_CLEARER"]);
                //posSynthetic.posKey.idB_Clearer = Convert.ToInt32(dr["IDB_CLEARER"]);
                //posSynthetic.posKey.idI = Convert.ToInt32(dr["IDI"]);
                //posSynthetic.posKey.assetCategory = asset.First;
                //posSynthetic.posKey.idAsset = asset.Second;
                //posSynthetic.side = Convert.ToInt32(dr["SIDE"]);

                posSynthetic = new PosSynthetic
                {
                    posKey = posKey,
                    side = side,

                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    qty = Convert.ToDecimal(dr["TOTALDAILYQTY"]),
                    closingPriceSpecified = (false == Convert.IsDBNull(dr["QUOTEPRICE"])),
                    deltaSpecified = (false == Convert.IsDBNull(dr["QUOTEDELTA"]))
                };

                // FI 20150313 [XXXXX] Spheres n'arrondi plus les prix selon les règles de la devise (9 decimal max)
                // Usage du format {0:0.#########}" pour que avgPrice de type decimal soit serializer avec 9 décimal. 
                // Standard retenue du fait que la colonne TRAINSTRUMENT.PRICE est en UT_PRICE (9 decimal)  

                // FI 20170529 [XXXXX] Correction problème rencontré en recette 
                // La qté peut être à zéro => pas de calcul de prix moyen
                // (Exemple : Sur les Equity, évènement MKV, EVENTDET.DAILYQUANTITY est à zéro si les positions sont totalement clôturées)
                // (=> voir ticket 22415 pour aller plus loin)
                // RD 20160805 [22415] To avoid divide by zero.
                if (posSynthetic.qty != 0)
                {
                    Decimal avgPrice = Convert.ToDecimal(dr["TOTALPRICE"]) / posSynthetic.qty;
                    posSynthetic.avgPrice = Convert.ToDecimal(String.Format("{0:0.#########}", avgPrice));
                    posSynthetic.fmtAvgPrice = BuildFmtPrice(pCS,
                        new Pair<Cst.UnderlyingAsset, int>(posSynthetic.posKey.assetCategory, posSynthetic.posKey.idAsset),
                        posSynthetic.avgPrice, 100);
                }

                // FI 20150320 [XXPOC] closing price est exprimé dans la base du DC 
                // closing price était exprimée en base 100 avant cette modification
                if (posSynthetic.closingPriceSpecified)
                {
                    posSynthetic.closingPrice = Convert.ToDecimal(dr["QUOTEPRICE"]);
                    posSynthetic.fmtClosingPriceSpecified = true;
                    posSynthetic.fmtClosingPrice = BuildFmtPrice(pCS,
                                new Pair<Cst.UnderlyingAsset, int>(posSynthetic.posKey.assetCategory, posSynthetic.posKey.idAsset),
                                posSynthetic.closingPrice, null);
                }

                if (posSynthetic.deltaSpecified)
                    posSynthetic.delta = Convert.ToDecimal(dr["QUOTEDELTA"]);

                decimal lovAmount = Convert.IsDBNull(dr["LOVAMOUNT"]) ? decimal.Zero : Convert.ToDecimal(dr["LOVAMOUNT"]);
                string lovCurrency = Convert.IsDBNull(dr["LOVCURRENCY"]) ? string.Empty : Convert.ToString(dr["LOVCURRENCY"]);
                decimal umgAmount = Convert.IsDBNull(dr["UMGAMOUNT"]) ? decimal.Zero : Convert.ToDecimal(dr["UMGAMOUNT"]);
                string umgCurrency = Convert.IsDBNull(dr["UMGCURRENCY"]) ? string.Empty : Convert.ToString(dr["UMGCURRENCY"]);

                PosSyntheticLov(NotificationDocument.EfsMLversion, lovAmount, lovCurrency, posSynthetic);
                PosSyntheticUmg(NotificationDocument.EfsMLversion, umgAmount, umgCurrency, posSynthetic);

                lstPosSynt.Add(posSynthetic);

            }

            if (ArrFunc.IsFilled(lstPosSynt))
            {
                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
                //pLstPosSynthetics.Add(new PosSynthetics() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), posSynthetic = lstPosSynt });
                if (posSynthetics == null)
                    pLstPosSynthetics.Add(new PosSynthetics() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), posSynthetic = lstPosSynt });
                else
                    posSynthetics.posSynthetic.AddRange(lstPosSynt);
            }

        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20150709 [XXXXX] Add
        /// FI 20151019 [21317] Modify 
        /// FI 20170530 [XXXXX] Modify
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetPositionSyntheticSafekeepingReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate)
        {
            if (false == NotificationDocument.StlPosSyntheticsSpecified)
                return;

            // FI 20151019 [21317] Modify seuls les trades ti.DTSETTLT <= @DT sont en position (avant il y avait tr.DTBUSINESS <= @DT )
            // FI 20170530 [XXXXX] Utilisation de VW_TRADE_POSSEC puisque VW_TRADE_POSOTC ne contient plus les GPRODUCT = 'SEC' en v6.0
            string query = @"select e.UNIT as SKPUNIT, e.EVENTTYPE as SKPEVENTTYPE, efee.PAYMENTTYPE as SKPPAYMENTTYPE, 
            sum(case when e.IDB_PAY = t.IDB_DEALER then -1 else 1 end * e.VALORISATION) as SKPAMOUNT,
            t.SIDE, t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER, t.IDI, t.IDASSET, t.ASSETCATEGORY
            from dbo.EVENT e
            inner join dbo.EVENTFEE efee on (efee.IDE = e.IDE)
            inner join dbo.VW_TRADE_POSSEC t on (t.IDT = e.IDT)
            inner join dbo.EVENTCLASS ec on (ec.IDE = e.IDE) and (ec.DTEVENT = @DT) and (ec.EVENTCLASS='VAL')
            inner join dbo.TRADELINK tlcb on (tlcb.IDT_B = e.IDT) and (tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance') and (tlcb.IDT_A = @IDT)
            left outer join 
            (

                select pad.IDT_BUY as IDT,
                sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1 then isnull(pad.QTY,0) else 0 end) as QTY_BUY, 0 as QTY_SELL
	            from dbo.TRADE alloc 
	            inner join dbo.POSACTIONDET pad on (pad.IDT_BUY = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_SELL) 
                where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                group by pad.IDT_BUY
	                  
                union all
	                  
                select pad.IDT_SELL as IDT,
	            0 as QTY_BUY, sum(case when isnull(tr.DTSETTLT,@DATE1) <= @DATE1  then isnull(pad.QTY,0) else 0 end) as QTY_SELL
	            from dbo.TRADE alloc 
	            inner join dbo.POSACTIONDET pad on (pad.IDT_SELL = alloc.IDT)
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                left outer join dbo.TRADE tr on (tr.IDT = pad.IDT_BUY) 
                where ((pa.DTOUT is null or pa.DTOUT > @DT) and (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT)))
                group by pad.IDT_SELL

            ) pos on (pos.IDT = tr.IDT)
            where (e.EVENTCODE = 'SKP') and (e.EVENTTYPE != 'TAX') and (@DT between tr.DTSETTLT and tr.DTOUTADJ) and ((tr.QTY - isnull(pos.QTY_BUY,0) - isnull(pos.QTY_SELL, 0)) > 0) 
            group by e.UNIT, efee.PAYMENTTYPE, e.EVENTTYPE, t.IDA_DEALER, t.IDB_DEALER, t.IDA_CLEARER, t.IDB_CLEARER, t.IDI, t.IDASSET, t.ASSETCATEGORY, t.SIDE";

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

            QueryParameters qry = new QueryParameters(pCS, query, dp);
            DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
            if (dt.Rows.Count > 0)
            {

                PosSynthetics stlPosSynthetics = NotificationDocument.StlPosSynthetics.Where(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate)).FirstOrDefault();
                if (null == stlPosSynthetics)
                    throw new NotSupportedException(StrFunc.AppendFormat("NotificationDocument.stlPosSynthetics is not specified for date {0}", DtFunc.DateTimeToStringDateISO(pDate)));

                // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message                
                foreach (PosSynthetic item in stlPosSynthetics.posSynthetic.Where(x => (false == x.skpSpecified)))
                {
                    DataRow[] row = dt.Select(StrFunc.AppendFormat("IDA_DEALER={0} and IDB_DEALER={1} and IDA_CLEARER={2} and IDB_CLEARER={3} and IDI ={4} and IDASSET={5} and ASSETCATEGORY='{6}' and SIDE ='{7}'",
                                    item.posKey.idA_Dealer.ToString(),
                                    item.posKey.idB_Dealer.ToString(),
                                    item.posKey.idA_Clearer.ToString(),
                                    item.posKey.idB_Clearer.ToString(),
                                    item.posKey.idI.ToString(),
                                    item.posKey.idAsset.ToString(),
                                    item.posKey.assetCategory.ToString(),
                                    item.side.ToString()));

                    if (ArrFunc.IsFilled(row))
                    {
                        item.skpSpecified = true;
                        item.skp = new List<ReportFeeLabel>();
                        foreach (DataRow dr in row)
                        {
                            item.skp.Add(new ReportFeeLabel());

                            ReportFeeLabel skp = item.skp.Last();
                            decimal amount = Convert.ToDecimal(dr["SKPAMOUNT"]);
                            string currency = Convert.ToString(dr["SKPUNIT"]);
                            skp.amount = System.Math.Abs(amount);
                            skp.currency = currency;
                            skp.paymentTypeSpecified = (dr["SKPPAYMENTTYPE"] != Convert.DBNull);
                            if (skp.paymentTypeSpecified)
                                skp.paymentType = Convert.ToString(dr["SKPPAYMENTTYPE"]);

                            skp.eventTypeSpecified = true;
                            skp.eventType = Convert.ToString(dr["SKPEVENTTYPE"]);

                            skp.sideSpecified = (skp.amount > 0);
                            if (skp.sideSpecified)
                            {
                                EFS_Cash cash = new EFS_Cash(pCS, skp.amount, skp.currency);
                                skp.amount = cash.AmountRounded;

                                if (amount > 0)
                                    skp.side = CrDrEnum.CR;
                                else
                                    skp.side = CrDrEnum.DR;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mise à jour des Labels Savekeeping
        /// </summary>
        /// FI 20150709 [XXXXX] Add (GLOP, il faudra faire mieux vu la méthode UpdateCashPaymentLabel)
        private void UpdateSavekeepingLabel()
        {
            HeaderFooter reportSettings = null;
            if (NotificationDocument.Header.ReportSettingsSpecified)
                reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;

            if ((null != reportSettings) && reportSettings.journalEntrySpecified && NotificationDocument.StlPosSyntheticsSpecified)
            {
                var allItems = from items in NotificationDocument.StlPosSynthetics
                               from item in items.posSynthetic.Where(x => x.skpSpecified)
                               select item;

                foreach (PosSynthetic itemPos in allItems)
                {
                    foreach (ReportFeeLabel item in itemPos.skp)
                    {
                        LabelDescription desc =
                            desc = reportSettings.journalEntry.Where(x => x.key == "SKP").FirstOrDefault();
                        if (null == desc)
                            desc = reportSettings.journalEntry.Where(x => x.key == "***").FirstOrDefault();

                        if (null != desc)
                        {
                            IAssetRepository asset = RepositoryTools.LoadAssetRepository(NotificationDocument.Repository, new Pair<int, Cst.UnderlyingAsset>(itemPos.IdAsset, itemPos.AssetCategory));
                            if (null == asset)
                                throw new NotSupportedException(StrFunc.AppendFormat("Asset (id:{0}, cat:{0}) not found in repository", itemPos.IdAsset, itemPos.AssetCategory.ToString()));


                            CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(this.Culture);
                            string extType = string.Empty;
                            if (item.paymentTypeSpecified)
                            {
                                IEnumRepository enumRepository = RepositoryTools.LoadEnumValue(this.NotificationDocument.Repository, "PaymentType", item.paymentType);
                                if (null != enumRepository)
                                    extType = enumRepository.ExtValue;
                            }

                            string fmtQty = itemPos.qty.ToString("n0", cultureInfo); //=>présence du séparateur de millier

                            string isinCode = string.Empty;
                            switch (itemPos.AssetCategory)
                            {
                                case Cst.UnderlyingAsset.EquityAsset:
                                    isinCode = ((IAssetEquityRepository)asset).ISINCode;
                                    break;
                                case Cst.UnderlyingAsset.ExchangeTradedContract:
                                    isinCode = ((IAssetETDRepository)asset).ISINCode;
                                    break;
                            }

                            item.label = StrFuncExtended.ReplaceObjectField(desc.description, new
                            {
                                SIDE = item.side.ToString(),
                                QTY = itemPos.qty,
                                FMTQTY = fmtQty,
                                ASSET_CATEGORY = itemPos.AssetCategory,
                                ASSET_IDENTIFIER = asset.Identifier,
                                ASSET_ALTIDENTIFIER = asset.AltIdentifier,
                                ASSET_DISPLAYNAME = asset.Displayname,
                                ASSET_DESCRIPTION = asset.Description,
                                ASSET_ISINCODE = isinCode,
                                TYPE = item.paymentType,
                                EXTTYPE = extType,
                                EVENTTYPE = item.eventType
                            }, cultureInfo);
                            item.labelSpecified = StrFunc.IsFilled(item.label);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sur les DebtSecurityTransaction calcul du nominal de chaque trade en position ou trade négocié  
        /// </summary>
        /// FI 20151019 [21317] Add UpdateNominalInDebtSecurityTransaction
        private void UpdateNominalInDebtSecurityTransaction()
        {
            if (NotificationDocument.RepositorySpecified && NotificationDocument.Repository.AssetDebtSecuritySpecified)
            {

                foreach (IAssetDebtSecurityRepository itemAsset in NotificationDocument.Repository.AssetDebtSecurity.Where(x => x.ParValueSpecified))
                {
                    List<PosTradeCommon> trade = (from item in
                                                      GetAllTrades(null).Where(x => x.qtySpecified &&
                                                          x.family == Cst.ProductFamily_DSE &&
                                                          x.idAsset == itemAsset.OTCmlId && x.assetCategory == Cst.UnderlyingAsset.Bond)
                                                  select (PosTradeCommon)item).ToList();


                    List<PosTradeCommon> posTrade = (from item in
                                                         GetAllPosTrades(null).Where(x => x.qtySpecified
                                                             && x.family == Cst.ProductFamily_DSE
                                                             && x.idAsset == itemAsset.OTCmlId && x.assetCategory == Cst.UnderlyingAsset.Bond)
                                                     select (PosTradeCommon)item).ToList();

                    List<PosTradeCommon> posTradeCommon = (trade.Concat(posTrade)).ToList();

                    foreach (PosTradeCommon item in posTradeCommon)
                    {
                        item.nomSpecified = true;
                        item.nom = new ReportAmountSide()
                        {
                            amount = item.qty * itemAsset.ParValue.amount,
                            currency = itemAsset.ParValue.currency
                        };
                    }
                }
            }
        }


        /// <summary>
        /// Retourne la liste des trades présents dans les éléments unsettledTrades, settledTrades
        /// </summary>
        /// <param name="pDate"></param>
        /// <returns></returns>
        /// FI 20150825 [21287] Add Method
        private List<TradeReport> GetUnsettledAndSettledTrades(Nullable<DateTime> pDate)
        {
            List<TradeReport> ret = new List<TradeReport>();

            if (pDate.HasValue)
            {
                string sDate = DtFunc.DateTimeToStringDateISO(pDate.Value);
                ret =
                    (NotificationDocument.UnsettledTrades.Where(x => x.bizDt == sDate)
                            .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() })
                            .FirstOrDefault().trade).Concat(
                                        NotificationDocument.SettledTrades.Where(x => x.bizDt == sDate)
                                        .DefaultIfEmpty(new TradesReport() { bizDt = sDate, trade = new List<TradeReport>() })
                                        .FirstOrDefault().trade).ToList();
            }
            else
            {
                ret = (from itemTrades in NotificationDocument.UnsettledTrades.Concat(NotificationDocument.SettledTrades)
                       from item in itemTrades.trade
                       select item).ToList();

            }

            return ret;
        }

        /// <summary>
        /// Regroupement des frais par paymentType
        /// </summary>
        /// <param name="allfee"></param>
        /// <returns></returns>
        /// FI 20150825 [21287] Add Method
        private ReportFee[] MergeFee(ReportFee[] pFee)
        {
            if (null == pFee)
                throw new ArgumentNullException("pFee is null");

            List<ReportFee> ret = new List<ReportFee>();

            List<ReportFee> feeNopaymentType = (from item in pFee.Where(x => (false == x.paymentTypeSpecified)) select item).ToList();
            foreach (ReportFee item in feeNopaymentType)
            {
                item.paymentTypeSpecified = true;
                item.paymentType = "NO_PAYMENTTYPE";
            }


            List<string> paymentType = ((from item in pFee.Where(x => x.paymentTypeSpecified)
                                         select item.paymentType).Distinct()).ToList();

            foreach (string itemPaymentType in paymentType)
            {
                List<string> currency = ((from item in pFee.Where(x => x.paymentTypeSpecified && x.paymentType == itemPaymentType)
                                          select item.currency).Distinct()).ToList();

                foreach (string itemCurrency in currency)
                {
                    List<ReportFee> feeselected = pFee.Where(x => x.currency == itemCurrency && x.paymentType == itemPaymentType).ToList();

                    ReportFee fee = new ReportFee() { paymentTypeSpecified = true, paymentType = itemPaymentType, currency = itemCurrency, sideSpecified = true, eventTypeSpecified = false };
                    fee.amount = feeselected.Sum(x => (((x.side == CrDrEnum.CR) ? 1 : -1) * x.amount));

                    if (fee.amount > 0)
                        fee.side = CrDrEnum.CR;
                    else if (fee.amount < 0)
                        fee.side = CrDrEnum.DR;
                    else
                        fee.sideSpecified = false;

                    fee.amount = System.Math.Abs(fee.amount);

                    fee.taxSpecified = ((from item in feeselected.Where(x => x.taxSpecified) select item).Count() > 0);
                    if (fee.taxSpecified)
                    {
                        List<string> taxId = ((from item in feeselected
                                               from itemTax in item.tax
                                               select itemTax.taxId).Distinct()).ToList();

                        List<ReportTaxAmount> newTax = new List<ReportTaxAmount>();
                        foreach (string itemTaxId in taxId)
                        {
                            ReportTaxAmount tax = new ReportTaxAmount() { taxId = itemTaxId };
                            tax.amount = (from item in feeselected.Where(x => x.taxSpecified)
                                          from itemTax in item.tax.Where(x => x.taxId == itemTaxId)
                                          select new { item.side, itemTax.amount }).Sum(x => (((x.side == CrDrEnum.CR) ? 1 : -1) * x.amount));
                            tax.amount = System.Math.Abs(tax.amount);

                            ReportTaxAmount firstTax = (from item in feeselected.Where(x => x.taxSpecified)
                                                        from itemTax in item.tax.Where(x => x.taxId == itemTaxId)
                                                        select itemTax).First();

                            tax.taxType = firstTax.taxType;
                            tax.rate = firstTax.rate;

                            newTax.Add(tax);
                        }
                        fee.tax = newTax.ToArray();
                    }
                    ret.Add(fee);
                }
            }


            feeNopaymentType = (from item in ret.Where(x => (x.paymentType == "NO_PAYMENTTYPE")) select item).ToList();
            foreach (ReportFee item in feeNopaymentType)
            {
                item.paymentTypeSpecified = false;
                item.paymentType = string.Empty;
            }


            return ret.ToArray();
        }

        /// <summary>
        /// Recherche des actions de type POC ou POT  avec restitution de frais qui s'applique à une liste de trade jusqu'à la date {pDate} 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDate"></param>
        /// <param name="pIdT">Liste des trades du scope</param>
        /// <returns></returns>
        /// FI 20150825 [21287] Add Method
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private List<IPosactionTradeFee> LoadPosactionTradeFee(string pCS, DateTime pDate, int[] pIdT)
        {
            List<IPosactionTradeFee> ret = new List<IPosactionTradeFee>();

            if (ArrFunc.IsFilled(pIdT))
            {
                // Recherche des actions "Correction", "Suppression" opérés sur les trades pour lesquels il existe des restitutions de frais
                string restrictTrade = "(" + DataHelper.SQLColumnIn(pCS, "pad.IDT_CLOSING", pIdT, TypeData.TypeDataEnum.integer, false, true) + ")";
                string query = StrFunc.AppendFormat(@"select pad.IDPADET as IDPADET,  pad.IDT_CLOSING as IDT
                from dbo.POSACTIONDET pad
                inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                inner join dbo.POSREQUEST pr on pr.IDPR = pa.IDPR and pr.REQUESTTYPE in ('POC','POT')
                where (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT))
                and {0}
                and exists (
                    select 1 from dbo.EVENT e
				    inner join dbo.EVENTPOSACTIONDET epad on epad.IDE = e.IDE and epad.IDPADET = pad.IDPADET
				    where e.EVENTCODE = 'OPP'
                )", restrictTrade);

                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(pCS,DataParameter.ParameterEnum.DT), pDate); // FI 20201006 [XXXXX] DbType.Date

                QueryParameters qry = new QueryParameters(pCS, query, dp);
                DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());

                foreach (DataRow row in dt.Rows)
                {
                    ret.Add(new PosActionTradeFee()
                    {
                        idPosActionDet = Convert.ToInt32(row["IDPADET"]),
                        idT = Convert.ToInt32(row["IDT"])
                    });
                }
                SetPosActionFee(pCS, ret);
            }
            return ret;
        }

        /// <summary>
        ///  Alimentation des frais des trades (trades, unsettledTrades, settledTrades) à partir des informations du datadocument
        /// </summary>
        /// FI 20150825 [21287] Add Method
        /// FI 20151019 [21317] Modify
        /// FI 20161214 [21916] Modify
        private void SetOppfromDataDocument()
        {
            List<TradeReport> allTrade = GetAllTrades(null);

            foreach (TradeReport item in allTrade)
            {
                IEnumerable<ITrade> trades =
                        (from trade in NotificationDocument.DataDocument.Trade
                         where trade.TradeId == item.tradeIdentifier
                         select trade);

                bool isOk = (null != trades);

                if (isOk)
                {
                    ITrade tradeRef = trades.First();

                    // FI 20161214 [21916] nouvelle méthode pour alimentation de product (Utilisation de al méthode RptSide();)
                    DataDocumentContainer tradeDatadoc = new DataDocumentContainer(NotificationDocument.DataDocument);
                    tradeDatadoc.SetCurrentTrade(tradeRef);
                    RptSideProductContainer product = tradeDatadoc.CurrentProduct.RptSide();
                    if (null == product)
                        throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not implemented", tradeDatadoc.CurrentProduct.ProductBase.ProductName));

                    if (tradeRef.OtherPartyPaymentSpecified)
                    {
                        IFixParty dealer = product.GetDealer(); // sur ce produit rptSide est initialisé lors de chargement du trade

                        IEnumerable<IPayment> payments =
                            (from payment in tradeRef.OtherPartyPayment
                             where (payment.PayerPartyReference.HRef == dealer.PartyId.href ||
                                    payment.ReceiverPartyReference.HRef == dealer.PartyId.href)
                             select payment);

                        item.feeSpecified = (payments.Count() > 0);
                        if (item.feeSpecified)
                        {
                            item.fee = new ReportFee[payments.Count()];
                            int i = 0;
                            foreach (IPayment payment in payments)
                            {
                                item.fee[i] = new ReportFee();

                                ReportFee fee = item.fee[i];
                                fee.amount = payment.PaymentAmount.Amount.DecValue;
                                fee.currency = payment.PaymentAmount.Currency;
                                fee.paymentTypeSpecified = payment.PaymentTypeSpecified;
                                if (payment.PaymentTypeSpecified)
                                    fee.paymentType = payment.PaymentType.Value;
                                if (payment.PayerPartyReference.HRef == dealer.PartyId.href)
                                    fee.side = CrDrEnum.DR;
                                else
                                    fee.side = CrDrEnum.CR;
                                //FI 20141208 [XXXXX] alimentation de sideSpecified
                                fee.sideSpecified = (fee.amount > 0);
                                // FI 20141209 [XXXXX] appel de la méthode CopyPaymentTaxToReportFee
                                CopyPaymentTaxToReportFee(fee, payment);
                                i++;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Génère le stream en contrevaleur s'il n'existe pas dans le trade de cashBalance (Valable uniquement sur les CB en CashBalanceCalculationMethodEnum.CSBDEFAULT)
        /// </summary>
        /// FI 20150930 [21311] Add
        /// FI 20160119 [21748] Modify
        private void BuildCashBalanceExchangeStream(string pCS)
        {
            DataDocumentContainer dataDocument = new DataDocumentContainer(NotificationDocument.DataDocument);
            if (false == dataDocument.CurrentProduct.IsCashBalance)
                throw new Exception("trade[0] is not an cashBalance");

            Boolean isPeriodic = (null != _cashBalancePeriodic);

            // RD 20160912 [22447] Manage MULTIPARTIES SYNTHESIS Message
            ITrade[] trade = (from item in dataDocument.Trade where item.Product.ProductBase.IsCashBalance select item).ToArray();

            for (int i = 0; i < ArrFunc.Count(trade); i++)
            {
                dataDocument.SetCurrentTrade(trade[i]);

                CashBalance cashBalance = (CashBalance)dataDocument.CurrentProduct.Product;

                Boolean isOk = cashBalance.settings.cashBalanceMethodSpecified &&
                   cashBalance.settings.cashBalanceMethod == CashBalanceCalculationMethodEnum.CSBDEFAULT &&
                   (false == cashBalance.settings.cashBalanceCurrencySpecified);

                if (isOk) // pas de exchage en présence d'un seul stream si celui-ci est déjà en devise de reporting 
                {
                    if (ArrFunc.Count(cashBalance.cashBalanceStream) == 1 && cashBalance.cashBalanceStream[0].currency.Value == this.ReportCurrency.Second)
                        isOk = false;
                }

                if (isOk)
                {
                    string idCExchangeStream = ReportCurrency.First;

                    cashBalance.settings.cashBalanceCurrencySpecified = true;
                    cashBalance.settings.cashBalanceCurrency = new Currency(ReportCurrency.Second);

                    List<SQL_TradeRisk> sqlTradeRisk = new List<SQL_TradeRisk>();
                    DateTime date = dataDocument.TradeHeader.TradeDate.DateValue;

                    if (isPeriodic)
                        sqlTradeRisk = _cashBalancePeriodic.Second;
                    else
                        sqlTradeRisk.Add(new SQL_TradeRisk(pCS, dataDocument.CurrentTrade.TradeId));

                    #region chgt des flux nécessaires à bâtir l' exchangeStream
                    DataRow[] row = LoadFlowsCashBalancePeriodic(pCS, sqlTradeRisk);

                    List<String> cur = (from item in row
                                        select Convert.ToString(item["UNIT"])).Distinct().ToList();
                    if (ArrFunc.IsEmpty(cur))
                        throw new NotSupportedException("No currency flows");

                    var actor = (from item in row
                                 select new
                                 {
                                     idA_Risk = Convert.ToInt32(item["IDA_RISK"]),
                                     idB_Risk = Convert.ToInt32(item["IDB_RISK"]),
                                     idA_Entity = Convert.ToInt32(item["IDA_ENTITY"])
                                 }).Distinct().ToList();
                    if (actor.Count > 1)
                        throw new NotSupportedException("several {IDA_RISK,IDB_RISK,IDA_ENTITY} found");

                    //FI 20160119 [21748] Chgt de la devise de compta (Utilisé comme devise pivot si nécessaire)
                    int idAEntity = actor[0].idA_Entity;
                    // FI 20200520 [XXXXX] Add SQL cache
                    SQL_Actor actorEntity = new SQL_Actor(CSTools.SetCacheOn(pCS), idAEntity)
                    {
                        WithInfoEntity = true
                    };
                    actorEntity.LoadTable(new string[] { "ACTOR.IDA, ACTOR.IDENTIFIER, ACTOR.DISPLAYNAME, ACTOR.DESCRIPTION, ACTOR.BIC, ent.IDA as IDA1, isnull(ent.IDCACCOUNT, 'EUR')  as IDCACCOUNT" });
                    if (false == actorEntity.IsEntityExist)
                        throw new NotSupportedException(StrFunc.AppendFormat("Actor (id:{0}) is not an entity", idAEntity.ToString()));

                    #endregion

                    #region construction de cashBalance.exchangeCashBalanceStream
                    cashBalance.exchangeCashBalanceStream = new ExchangeCashBalanceStream();

                    // exchangeCashBalanceStream existe uniquement si les cours de change nécessaires sont présents
                    cashBalance.exchangeCashBalanceStreamSpecified =
                        ConvertCashBalanceFlows(pCS, row, idCExchangeStream, actorEntity.IdCAccount, date, out List<IdentifiedFxRate> fxRate,  _setErrorWarning);

                    if (cashBalance.exchangeCashBalanceStreamSpecified)
                    {
                        int idARisk = actor[0].idA_Risk;
                        // FI 20200520 [XXXXX] Add SQL cache
                        SQL_Actor actorRisk = new SQL_Actor(CSTools.SetCacheOn(pCS), idARisk);
                        actorRisk.LoadTable(new string[] { "IDA, IDENTIFIER, DISPLAYNAME, DESCRIPTION, BIC" });

                        // rowcur pour considérer uniquement les enregistrements qui sont ds la devise de l'exchangeStream
                        // Pour gérer le cas où des fixings sont absents => Ds ce cas certains enregistrements restent ds leur devise
                        DataRow[] rowcur = (from itemRow in row
                                            where (Convert.ToString(itemRow["UNIT"]) == idCExchangeStream)
                                            select itemRow).ToArray();

                        BuidCashBalanceStream(pCS, cashBalance.exchangeCashBalanceStream, idCExchangeStream, rowcur, actorRisk.XmlId, actorEntity.XmlId);

                        cashBalance.fxRateSpecified = ArrFunc.IsFilled(fxRate);
                        if (cashBalance.fxRateSpecified)
                            cashBalance.fxRate = fxRate.ToArray();
                    }
                    #endregion
                }
            }
        }

        /// <summary>
        ///  Chargement des flux nécessaires à la constitution d'un trade CB à partir de n Trades cashBalance
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTradeRisk"></param>
        /// <returns></returns>
        // FI 20150930 [21311] Add Method
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        // EG 20200106 [XXXXX] Alias tr à la place de t sur IDA_RISK
        private static DataRow[] LoadFlowsCashBalancePeriodic(string pCS, List<SQL_TradeRisk> pTradeRisk)
        {
            QueryParameters query = GetQueryCashBalancePeriodic(pCS, pTradeRisk);

            DataTable dt = DataHelper.ExecuteDataTable(pCS, query.Query, query.Parameters.GetArrayDbParameter());
            if (ArrFunc.IsEmpty(dt.Rows))
                throw new NotSupportedException("No Event flows");

            #region Query ORA
            string queryOraxml = StrFunc.AppendFormat(@"select ev.UNIT, tr.IDA_ENTITY, tr.IDA_RISK , tr.IDB_RISK,
            /* marginRequirement */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:marginRequirement/efs:amount/amount)', '{1}') as PREVIOUS_MGR,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:marginRequirement/payerPartyReference/@href)', '{1}') as PREVIOUS_MGR_PAYER,

            /* cashAvailable */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:cashAvailable/efs:amount/amount)', '{1}') as PREVIOUS_CSA,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:cashAvailable/payerPartyReference/@href)', '{1}') as PREVIOUS_CSA_PAYER,
            /* cashUsed */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:cashUsed/efs:amount/amount)', '{1}') as PREVIOUS_CSU,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:cashUsed/payerPartyReference/@href)', '{1}') as PREVIOUS_CSU_PAYER,

            /* collateralAvailable */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:collateralAvailable/efs:amount/amount)', '{1}') as PREVIOUS_CLA,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:collateralAvailable/payerPartyReference/@href)', '{1}') as PREVIOUS_CLA_PAYER,
            /* collateralUsed */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:collateralUsed/efs:amount/amount)', '{1}') as PREVIOUS_CLU,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:collateralUsed/payerPartyReference/@href)', '{1}') as PREVIOUS_CLU_PAYER,

            /* uncoveredMarginRequirement */
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:uncoveredMarginRequirement/efs:amount/amount)', '{1}') as PREVIOUS_UMR,
            EXTRACTVALUE(trx.TRADEXML,'({0}/efs:uncoveredMarginRequirement/payerPartyReference/@href)', '{1}') as PREVIOUS_UMR_PAYER,

            /* PartyId Risk */
            EXTRACTVALUE(trx.TRADEXML,'(efs:EfsML/party[@OTCmlId=""'|| tr.IDA_RISK  ||'""]/@id)', '{1}') as PARTY_RISK_ID
            from dbo.TRADE tr 
            inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'CBS') and (ev.EVENTTYPE = 'AMT')
            where (tr.IDT = @IDT)", 
            @"efs:EfsML/trade/efs:cashBalance/efs:cashBalanceStream[efs:currency/text()=""'|| ev.UNIT ||'""]/efs:previousMarginConstituent",
            OTCmlHelper.GetXMLNamespace_3_0(pCS));
            #endregion Query ORA

            #region Query SQL Server
            string querySqlServerxml = StrFunc.AppendFormat(@"
            WITH XMLNAMESPACES ('http://www.fpml.org/2007/FpML-4-4' as fpml,'http://www.efs.org/2007/EFSmL-3-0' as efs,
            DEFAULT 'http://www.fpml.org/2007/FpML-4-4')

            select ev.UNIT, tr.IDA_ENTITY, tr.IDA_RISK, tr.IDB_RISK,

            /* marginRequirement */
            trx.TRADEXML.query('{0}/efs:marginRequirement/efs:amount/amount/text()')as PREVIOUS_MGR,
            trx.TRADEXML.value('({0}/efs:marginRequirement/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_MGR_PAYER,

            /* cashAvailable */
            trx.TRADEXML.query('{0}/efs:cashAvailable/efs:amount/amount/text()')as PREVIOUS_CSA,
            trx.TRADEXML.value('({0}/efs:cashAvailable/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_CSA_PAYER,
            /* cashUsed */
            trx.TRADEXML.query('{0}/efs:cashUsed/efs:amount/amount/text()')as PREVIOUS_CSU,
            trx.TRADEXML.value('({0}/efs:cashUsed/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_CSU_PAYER,

            /* collateralAvailable */
            trx.TRADEXML.query('{0}/efs:collateralAvailable/efs:amount/amount/text()')as PREVIOUS_CLA,
            trx.TRADEXML.value('({0}/efs:collateralAvailable/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_CLA_PAYER,
            /* collateralUsed */
            trx.TRADEXML.query('{0}/efs:collateralUsed/efs:amount/amount/text()')as PREVIOUS_CLU,
            trx.TRADEXML.value('({0}/efs:collateralUsed/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_CLU_PAYER,

            /* uncoveredMarginRequirement */
            trx.TRADEXML.query('{0}/efs:uncoveredMarginRequirement/efs:amount/amount/text()')as PREVIOUS_UMR,
            trx.TRADEXML.value('({0}/efs:uncoveredMarginRequirement/efs:amount/amount/payerPartyReference/@href)[1]','varchar(64)') as PREVIOUS_UMR_PAYER,

            trx.TRADEXML.value('(efs:EfsML/party[@OTCmlId=sql:column(""tr.IDA_RISK"")]/@id) [1]','varchar(64)') as PARTY_RISK_ID

            from dbo.TRADE tr 
            inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT)
            inner join dbo.EVENT ev on (ev.IDT = tr.IDT) and (ev.EVENTCODE = 'CBS') and (ev.EVENTTYPE = 'AMT')
            where (tr.IDT = @IDT)", 
            @"efs:EfsML/trade/efs:cashBalance/efs:cashBalanceStream[efs:currency/text()=sql:column(""ev.UNIT"")]/efs:previousMarginConstituent");
            #endregion

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pTradeRisk.First().Id);

            if (DataHelper.IsDbOracle(pCS))
                query = new QueryParameters(pCS, queryOraxml, dp);
            else if (DataHelper.IsDbSqlServer(pCS))
                query = new QueryParameters(pCS, querySqlServerxml, dp);
            else
                throw new NotImplementedException("RDBMS not implemented");

            DataTable dtPreviousMarginConstituent = DataHelper.ExecuteDataTable(pCS, query.Query, query.Parameters.GetArrayDbParameter());

            if (dtPreviousMarginConstituent.Rows.Count > 0)
            {
                foreach (DataRow item in dtPreviousMarginConstituent.Rows)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        string key = "N_A";
                        switch (i)
                        {
                            case 0:
                                key = "MGR";
                                break;
                            case 1:
                                key = "CSA";
                                break;
                            case 2:
                                key = "CSU";
                                break;
                            case 3:
                                key = "CLA";
                                break;
                            case 4:
                                key = "CLU";
                                break;
                            case 5:
                                key = "UMR";
                                break;
                        }
                        string keyPayer = StrFunc.AppendFormat("PREVIOUS_{0}_PAYER", key);
                        string keyAmt = StrFunc.AppendFormat("PREVIOUS_{0}", key);


                        DataRow newRow = dt.NewRow();
                        newRow["UNIT"] = item["UNIT"];
                        newRow["IDA_ENTITY"] = item["IDA_ENTITY"];
                        newRow["IDA_RISK"] = item["IDA_RISK"];
                        newRow["IDB_RISK"] = item["IDB_RISK"];
                        newRow["AMT"] = (item["PARTY_RISK_ID"] == item[keyPayer]) ?
                                        DecFunc.DecValue(item[keyAmt].ToString()) * -1 : DecFunc.DecValue(item[keyAmt].ToString());
                        newRow["EVENTTYPE"] = keyAmt;

                        for (int j = 0; j < 2; j++)
                        {
                            string keyFee = (j == 0) ? "FEE" : "SKP";

                            newRow[keyFee + "PAYMENTTYPE"] = "N/A";
                            newRow[keyFee + "IDTAX"] = -1;
                            newRow[keyFee + "IDTAXDET"] = -1;
                            newRow[keyFee + "TAXCOUNTRY"] = "N/A";
                            newRow[keyFee + "TAXTYPE"] = "N/A";
                            newRow[keyFee + "TAXRATE"] = -1;
                        }

                        dt.Rows.Add(newRow);
                    }
                }
            }

            DataRow[] row = new DataRow[dt.Rows.Count];
            dt.Rows.CopyTo(row, 0);

            return row;
        }

        /// <summary>
        ///  Mise à jour de tous les flux en devise de {pExchangeCur}. 
        ///  <para>Retourne true s'il existe tous les tx de change vis à vis de {pExchangeCur}</para>
        ///  <para>Les montants sont arrondis en fonction des propriétés de {pExchangeCur}</para> 
        ///  <para>Retourne les fixing utlisés</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pRow">Représente les flux dans leur devise d'origine</param>
        /// <param name="pExchangeCur">Devise de contrevaleur</param>
        /// <param name="pIdCPivot">Devise pivot à utiliser pour déterminer les contrevaleurs</param>
        /// <param name="pDate">date de lecture des fixings</param>
        /// <param name="oFxRate">Liste des fixing utilisés</param>
        /// <param name="pSetErrorWarning"></param>
        /// FI 20150930 [21311] Add Method
        /// FI 20160119 [21748] Modify (Add devise pivot dans la recherche des taux de change)
        /// RD 20170119 [22797] Modify
        private static Boolean ConvertCashBalanceFlows(string pCS, DataRow[] pRow, string pExchangeCur, string pIdCPivot, DateTime pDate,
            out List<IdentifiedFxRate> oFxRate,  SetErrorWarning pSetErrorWarning)
        {
            Boolean ret = true;

            if (null == pRow)
                throw new ArgumentNullException("pRow is null");
            if (StrFunc.IsEmpty(pExchangeCur))
                throw new ArgumentNullException("pExchangeCur is null");

            oFxRate = new List<IdentifiedFxRate>();

            List<String> cur = (from item in pRow
                                select Convert.ToString(item["UNIT"])).Distinct().ToList();

            foreach (string itemCur in cur.Where(x => x != pExchangeCur))
            {
                // FI 20160119 [21748] LoadFxRate retourne une liste maintenant
                // FI 20200520 [XXXXX] Add SQL cache
                List<IdentifiedFxRate> fxRate = LoadFxRate(CSTools.SetCacheOn(pCS), pDate, itemCur, pExchangeCur, pIdCPivot,  pSetErrorWarning);
                if (fxRate.Count > 0)
                {
                    foreach (IdentifiedFxRate item in fxRate)
                    {
                        if (null == oFxRate.Find(x => x.Id == item.Id))
                            oFxRate.Add(item);
                    }
                }
                else
                    ret = false;
            }


            foreach (DataRow itemRow in pRow.Where(x => (Convert.ToString(x["UNIT"]) != pExchangeCur)))
            {
                string idCsrc = Convert.ToString(itemRow["UNIT"]);
                decimal amtsrc = Convert.ToDecimal(itemRow["AMT"]);

                IdentifiedFxRate rate = (from itemFxRate in oFxRate
                                         where (((itemFxRate.quotedCurrencyPair.currency1.Value == pExchangeCur) && (itemFxRate.quotedCurrencyPair.currency2.Value == idCsrc))
                                                  ||
                                                  ((itemFxRate.quotedCurrencyPair.currency2.Value == pExchangeCur) && (itemFxRate.quotedCurrencyPair.currency1.Value == idCsrc))
                                                )
                                         select itemFxRate).FirstOrDefault();
                if (null != rate)
                {
                    itemRow["UNIT"] = pExchangeCur;
                    if (amtsrc != decimal.Zero)
                    {
                        // RD 20170119 [22797]
                        // Les deux devises "idCsrc" et "pExchangeCur" doivent correspondre, respectivement, aux deux devises
                        // "rate.quotedCurrencyPair.currency1" et "rate.quotedCurrencyPair.currency2"
                        // Si ce n'est pas le cas, il faut réajuster le paramètre "pQuotedBasis" du constructeur "EFS_Cash"
                        //EFS_Cash cash = new EFS_Cash(pCS, idCsrc, pExchangeCur, amtsrc, rate.rate.DecValue, rate.quotedCurrencyPair.quoteBasis);
                        QuoteBasisEnum quoteBasis = rate.quotedCurrencyPair.quoteBasis;
                        if (rate.quotedCurrencyPair.currency1.Value != idCsrc)
                        {
                            if (QuoteBasisEnum.Currency1PerCurrency2 == quoteBasis)
                                quoteBasis = QuoteBasisEnum.Currency2PerCurrency1;
                            else if (QuoteBasisEnum.Currency2PerCurrency1 == quoteBasis)
                                quoteBasis = QuoteBasisEnum.Currency1PerCurrency2;
                        }
                        // FI 20200520 [XXXXX] Add SQL cache
                        EFS_Cash cash = new EFS_Cash(CSTools.SetCacheOn(pCS), idCsrc, pExchangeCur, amtsrc, rate.rate.DecValue, quoteBasis);
                        itemRow["AMT"] = cash.ExchangeAmountRounded;
                    }
                }
                else
                {
                    // Nothing => S'il n'existe pas de taux de change, les enregistrements restent inchangés. 
                    // Ils seront ignorés dans la constitution du exchangeCashBalanceStream qui sera par conséquent faux 
                    // Le traitement sera en warning (Un message de warning est inscrit dans le log) 
                    // Une fois la cotation renseignée l'utilisateur devra Regenérer l'édition
                }
            }

            return ret;
        }



        /// <summary>
        ///  Retourne le prix {pPrice} formaté selon les règles de formatage spécifique au éditions (reporting)  
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAsset">Représente l'asset</param>
        /// <param name="pPrice">Représente le prix</param>
        /// <param name="pPriceNumericBase">base du prix(optionel)
        /// <para>asset ETD: Si non renseigné, spheres® considère que le prix est dans la base du contrat</para>
        /// <para>asset autre: Ce paramètre est ignoré. Les prix sont nécessairement en base 100</para>
        /// </param>
        /// <returns></returns>
        /// FI 20150218 [20275] Add method 
        /// FI 20150417 [XXXXX] Modify
        private static string BuildFmtPrice(string pCS, Pair<Cst.UnderlyingAsset, int> pAsset, decimal pPrice, Nullable<int> pPriceNumericBase)
        {
            string ret;
            // FI 20150417 [XXXXX] la méthode .ToConvertedFractionalPartString est utilisée sur les ETD uniquement
            if (pAsset.First == Cst.UnderlyingAsset.ExchangeTradedContract)
            {
                AssetTools.GetAssetETDPriceInfo(CSTools.SetCacheOn(pCS), pAsset.Second,
                    out int instrumentDen, out _, out _,
                    out Cst.PriceFormatStyle priceFormatStyle, out int priceNumericBase, out decimal priceMultiplier);

                int currentBase = instrumentDen;
                if (pPriceNumericBase.HasValue)
                    currentBase = pPriceNumericBase.Value;

                int priceDisplayBase = (priceNumericBase > 0 ? priceNumericBase : instrumentDen);

                ret = new EFS_Decimal(pPrice).ToConvertedFractionalPartString(
                                                            priceDisplayBase, currentBase,
                                                            priceMultiplier, priceFormatStyle);
            }
            else
            {
                // FI 20150417 [XXXXX] asset autre que ETD formatage classique 
                // Affichage de toute les decimales significative avec un minimum de 4 decimales
                ret = StrFunc.FmtDecimalToInvariantCulture(pPrice, 4);
            }
            return ret;
        }


        /// <summary>
        ///  Mise à jour du nombre de decimale des Prix moyen formatés
        ///  <para>Si le nbr de décimale de la moyenne est supérieure aux nbr de decimal des éléments qui la compose, le nbr de décimale retenu est {nbr de decimal des éléments}+1 </para>
        /// </summary>
        /// <param name="pCommonData"></param>
        /// <param name="pTrades">Liste de trade</param>
        /// <param name="pSubTotal">Liste des ss-totaux associé aux trades {pTrade}</param>
        /// FI 20160118 [21781] Add Method
        private static void FmtAvg(CommonData pCommonData, List<PosTradeCommon> pTrades, List<PositionSubTotal> pSubTotal)
        {

            if (null == pCommonData)
                throw new ArgumentNullException("pCommonData is null");
            if (null == pTrades)
                throw new ArgumentNullException("pTrades is null");
            if (null == pSubTotal)
                throw new ArgumentNullException("pSubTotal is null");

            //decSeparator => format invariant
            string decSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;

            foreach (PositionSubTotal subTotal in pSubTotal.Where(x =>
                                                        false == (
                                                        (x.assetCategory == Cst.UnderlyingAsset.ExchangeTradedContract) &&
                                                        (IsFmtPrice(x.@long.fmtAvgPx) || IsFmtPrice(x.@short.fmtAvgPx)))))
            {
                int maxTradePrecision = 0;
                foreach (PosTradeCommon item in pTrades.Where(x =>
                                            x.idb == subTotal.idb &&
                                            x.idI == subTotal.idI &&
                                            x.idAsset == subTotal.idAsset &&
                                            x.assetCategory == subTotal.assetCategory))
                {
                    if (null == pCommonData.trade)
                        throw new NullReferenceException("pCommonData.trade is null");

                    CommonTradeAlloc trade = pCommonData.trade.Where(x => x.OTCmlId == item.OTCmlId).FirstOrDefault();
                    if (null == trade)
                        throw new NotSupportedException(StrFunc.AppendFormat("trade (idT:{0}) is not Found in commonData", item.OTCmlId));

                    int precision = StrFunc.After(trade.fmtLastPrice, decSeparator).Length;
                    if (precision > maxTradePrecision)
                        maxTradePrecision = precision;
                }

                int subAvgPrecision = System.Math.Max(StrFunc.After(subTotal.@long.fmtAvgPx, decSeparator).Length, StrFunc.After(subTotal.@short.fmtAvgPx, decSeparator).Length);
                if (subAvgPrecision > maxTradePrecision)
                {
                    subAvgPrecision = maxTradePrecision + 1;
                    String format = "{0:0." + ("0".PadRight(subAvgPrecision, '0')) + "}";

                    subTotal.@long.fmtAvgPx = String.Format(format, subTotal.@long.avgPx);
                    subTotal.@short.fmtAvgPx = String.Format(format, subTotal.@short.avgPx);
                }
            }
        }

        /// <summary>
        /// Retourne true si le prix est déjà formaté selon les règles spcéfiées sur le DerivativeContract 
        /// </summary>
        /// <returns></returns>
        /// FI 20160118 [21781] Add Method
        /// Fonction Identique au template IsFmtPrice (voir fichier Shared_Report_v2_Tools.xslt)
        /// Toute modification dans ce template est à reporter dans cette méthode et vis-et-versa
        private static bool IsFmtPrice(string pPrice)
        {
            Boolean ret = false;
            if (StrFunc.IsFilled(pPrice))
            {
                ret = pPrice.Contains("'") ||
                        pPrice.Contains("^") ||
                        ((pPrice.Contains("-") && (false == pPrice.StartsWith("-")))) ||
                         pPrice.Contains("/");
            }
            return ret;
        }

        /// <summary>
        /// Alimentation de l'élement collateral à partir du cashBalance
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20160530 [21885] add
        /// EG 20171016 [23509] Upd
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private void SetCollateralReportSynthesis(string pCS)
        {
            ITrade trade = null;
            DateTime dtBusiness = DateTime.MinValue;
            DataDocumentContainer document = null;

            if (null != _cashBalancePeriodic)
            {

                document = _cashBalancePeriodic.First;
                trade = _cashBalancePeriodic.First.CurrentTrade;
                dtBusiness = Convert.ToDateTime(_cashBalancePeriodic.Second.Last().GetFirstRowColumnValue("DTBUSINESS"));

            }
            else
            {
                trade = this.NotificationDocument.DataDocument.FirstTrade;
                //dtBusiness = this.NotificationDocument.dataDocument.firstTrade.tradeHeader.tradeDate.BusinessDate;
                dtBusiness = this.NotificationDocument.DataDocument.FirstTrade.TradeHeader.ClearedDate.DateValue;
                document = new DataDocumentContainer(this.NotificationDocument.DataDocument);
            }

            if (false == trade.Product.ProductBase.IsCashBalance)
                throw new Exception("Trade is not a CashBalance");

            CashBalance cashBalance = (CashBalance)trade.Product;

            List<CollateralReport> lstCollateral = new List<CollateralReport>();

            if ((cashBalance.cashBalanceStream.Where(x => x.collateralSpecified).Count() > 0))
            {
                int[] idpos = (from stream in cashBalance.cashBalanceStream.Where(x => x.collateralSpecified)
                               from collateral in stream.collateral
                               select collateral.OTCmlId).ToArray();

                string sqlQuery = StrFunc.AppendFormat(@"select IDPOSCOLLATERAL, IDENTIFIER, DISPLAYNAME, DESCRIPTION  
                from dbo.POSCOLLATERAL 
                where {0}", DataHelper.SQLColumnIn(pCS, "IDPOSCOLLATERAL", idpos, TypeData.TypeDataEnum.integer, false, true));

                DataTable dt = DataHelper.ExecuteDataTable(pCS, sqlQuery);

                foreach (CashBalanceStream stream in cashBalance.cashBalanceStream.Where(x => x.collateralSpecified))
                {
                    foreach (PosCollateral collateral in stream.collateral)
                    {
                        string filterExpression = StrFunc.AppendFormat("IDPOSCOLLATERAL={0}", collateral.otcmlId);
                        DataRow[] row = dt.Select(filterExpression);
                        if (false == ArrFunc.IsFilled(row))
                            throw new Exception(StrFunc.AppendFormat("No collateral found ({0})", filterExpression));

                        CssValueReport[] ccsValReport = new CssValueReport[collateral.haircut.Count()];
                        for (int i = 0; i < ArrFunc.Count(ccsValReport); i++)
                        {
                            Nullable<int> idAcss = null;
                            if (collateral.haircut[i].cssHrefSpecified)
                            {
                                idAcss = document.GetOTCmlId_Party(collateral.haircut[i].cssHref, PartyInfoEnum.id);
                                if (false == idAcss.HasValue)
                                    throw new NullReferenceException(StrFunc.AppendFormat("party (href:{0}) not found in dataDocument", collateral.haircut[i].cssHref));
                            }

                            ccsValReport[i] = new CssValueReport()
                                {
                                    idASpecified = collateral.haircut[i].cssHrefSpecified,
                                    idA = collateral.haircut[i].cssHrefSpecified ? idAcss.Value : 0,
                                    value = collateral.haircut[i].Value
                                };
                        }

                        lstCollateral.Add(
                            new CollateralReport()
                            {
                                OTCmlId = collateral.OTCmlId,
                                identifier = (String)row[0]["IDENTIFIER"],

                                displayName = (Convert.DBNull == row[0]["DISPLAYNAME"]) ? string.Empty : (string)row[0]["DISPLAYNAME"],
                                displayNameSpecified = Convert.DBNull != row[0]["DISPLAYNAME"],

                                description = (Convert.DBNull == row[0]["DESCRIPTION"]) ? string.Empty : (string)row[0]["DESCRIPTION"],
                                descriptionSpecified = Convert.DBNull != row[0]["DESCRIPTION"],

                                idB = collateral.bookId.OTCmlId,

                                idAsset = collateral.asset.OTCmlId,
                                assetCategory = collateral.asset.assetCategory,

                                valuation = collateral.valuation.Amt,
                                currency = stream.currency.Value,
                                sideSpecified = collateral.valuation.AmtSideSpecified,
                                side = collateral.valuation.AmtSide,

                                qtySpecified = collateral.valuation.QtySpecified,
                                qty = collateral.valuation.Qty,

                                haircut = ccsValReport
                            }
                        );
                    }
                }
            }

            if (false == NotificationDocument.CollateralsSpecified)
                NotificationDocument.CollateralsSpecified = ArrFunc.IsFilled(lstCollateral);

            if (ArrFunc.IsFilled(lstCollateral))
                NotificationDocument.Collaterals.Add(new CollateralsReport() { bizDt = DtFunc.DateTimeToStringDateISO(dtBusiness), collateral = lstCollateral });
        }

        /// <summary>
        /// Mise à jour des Labels présents dans les collateraux 
        /// </summary>
        /// FI 20160530 [21885] Add
        /// FI 20160613 [22256] Modify
        private void UpdateCollateralLabel()
        {
            HeaderFooter reportSettings = null;

            if (NotificationDocument.Header.ReportSettingsSpecified)
                reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;

            if ((null != reportSettings) && reportSettings.collateralSpecified && NotificationDocument.CollateralsSpecified)
            {
                IEnumerable<IAssetLabel> allCollateral = from itemCollaterals in NotificationDocument.Collaterals
                                                         from item in itemCollaterals.collateral
                                                         select (IAssetLabel)item;
                // FI 20160613 [22256] call UpdateLabelAsset
                UpdateLabelAsset(allCollateral, reportSettings.collateral, NotificationDocument.Repository, this.Culture);
            }
        }

        /// <summary>
        ///  Alimente pHeaderFooter.collateral à partir des informations présentes dans la colonne COLLATERAL
        ///  <para>Lorsque la colonne est non renseignée,collateral est alimenté avec les valeurs par défaut</para>
        /// </summary>
        /// <param name="pHeaderFooter"></param>
        /// <param name="pColumnCollateral"></param>
        /// FI 20160530 [21885] Add
        private static void SetCollateral(HeaderFooter pHeaderFooter, string pColumn)
        {
            string colValue = pColumn;

            List<LabelDescription> labelDescription = new List<LabelDescription>();
            //Dictionary<string, string> dic = GetKeyValue(colValue);
            //if (ArrFunc.IsFilled(dic))
            //{
            //    foreach (string key in dic.Keys)
            //        labelDescription.Add(new LabelDescription() { key = key, description = dic[key] });
            //}


            string defaultDesc = "{ASSET_ALTIDENTIFIER}.overflow-ellipsis(60tl) {ASSET_ISINCODE}";
            if ((false == labelDescription.Exists(x => x.key == "***")))
                labelDescription.Add(new LabelDescription() { key = "***", description = defaultDesc });

            pHeaderFooter.collateralSpecified = (labelDescription.Count > 0);
            if (pHeaderFooter.collateralSpecified)
                pHeaderFooter.collateral = labelDescription.First();
        }

        /// <summary>
        ///  Alimente pHeaderFooter.underlyingStock à partir des informations présentes dans la colonne COLLATERAL_UOD
        ///  <para>Lorsque la colonne est non renseignée,underlyingStock est alimenté avec les valeurs par défaut</para>
        /// </summary>
        /// <param name="pHeaderFooter"></param>
        /// <param name="pColumnCollateral"></param>
        /// FI 20160613 [22256] Add
        private static void SetUnderlyingStock(HeaderFooter pHeaderFooter, string pColumn)
        {
            string colValue = pColumn;

            List<LabelDescription> labelDescription = new List<LabelDescription>();
            //Dictionary<string, string> dic = GetKeyValue(colValue);
            //if (ArrFunc.IsFilled(dic))
            //{
            //    foreach (string key in dic.Keys)
            //        labelDescription.Add(new LabelDescription() { key = key, description = dic[key] });
            //}
            string defaultDesc = "{ASSET_ALTIDENTIFIER}.overflow-ellipsis(60tl) {ASSET_ISINCODE}";
            if ((false == labelDescription.Exists(x => x.key == "***")))
                labelDescription.Add(new LabelDescription() { key = "***", description = defaultDesc });

            pHeaderFooter.underlyingStockSpecified = (labelDescription.Count > 0);
            if (pHeaderFooter.underlyingStockSpecified)
                pHeaderFooter.underlyingStock = labelDescription.First();
        }

        /// <summary>
        ///  Alimentation de NotificationDocument.underlyingStocks 
        ///  <para>(Liste des positions actions déposées et utilisées pour la réduction des postions ETD Short futures et Short call à une date donné)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20160613 [22256] Add 
        private void SetUnderlyingStockSynthesis(string pCS, int pIdTCashBalance, DateTime pDate)
        {
            if (productEnv.ExistMarginRequirement())
            {
                string sqlRestictTrade = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = t.IDT 
                                            and tlcb.LINK = 'MarginRequirementInCashBalance' and tlcb.IDT_A = @IDT";

                QueryParameters query = GetQueryUnderlyingStock(pCS, sqlRestictTrade, pDate);
                query.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                SetUnderlyingStockFromQuery(pCS, query, pDate);
            }
        }

        /// <summary>
        ///  Recherche dans les flux XML des trades MRO les positions actions utilisées pour réduire les positions ETD short Call et short Future
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSqlRestrictTrade">Représente une restriction SQL de manière à ne considérer qu'une liste de trade MRO</param>
        /// <param name="pDate"></param>
        /// <returns></returns>
        // FI 20160613 [22256] Add 
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryUnderlyingStock(string pCS, string pSqlRestrictTrade, DateTime pDate)
        {
            String querySelect;
            DbSvrType dbSvrType = (DataHelper.GetDbSvrType(pCS));
            switch (dbSvrType)
            {
                case DbSvrType.dbSQL:
                    #region SQLSERVER
                    querySelect = @"WITH XMLNAMESPACES ('http://www.fpml.org/2007/FpML-4-4' as fpml,'http://www.efs.org/2007/EFSmL-3-0' as efs)
                    select trx.TRADEXML.query('efs:EfsML/fpml:trade/efs:marginRequirement/efs:underlyingStocks') as UNDERLYINGSTOCKS";
                    break;
                    #endregion SQLSERVER
                case DbSvrType.dbORA:
                    #region ORACLE
                    querySelect = @"select EXTRACT(trx.TRADEXML,'efs:EfsML/fpml:trade/efs:marginRequirement/efs:underlyingStocks',
                    'xmlns:efs=""http://www.efs.org/2007/EFSmL-3-0"" xmlns:fpml=""http://www.fpml.org/2007/FpML-4-4""') as UNDERLYINGSTOCKS";
                    break;
                    #endregion ORACLE
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("RDBMS ({0}) not implemented", dbSvrType.ToString()));
            }

            string queryFrom = StrFunc.AppendFormat(@"from dbo.TRADE t
            inner join dbo.INSTRUMENT i on (i.IDI = t.IDI)
            inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.IDENTIFIER = 'marginRequirement') 
            inner join dbo.TRADEXML trx on (trx.IDT = t.IDT)
            {0}
            where (t.DTBUSINESS = @DT) and (t.IDSTACTIVATION='REGULAR')", pSqlRestrictTrade);

            string query = StrFunc.AppendFormat(@"{0}{1}", querySelect + Cst.CrLf, queryFrom);

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }

        /// <summary>
        /// Alimentation de l'élément NotificationDocument.underlyingStocks
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pQry"></param>
        /// <param name="pDate"></param>
        /// FI 20160613 [22256] Add 
        // EG 20180426 Analyse du code Correction [CA2202]
        // EG 20200106 [XXXXX] Add xsiToRemove pour éviter plantage alias namespace non trouvé (bidouille de merde)
        private void SetUnderlyingStockFromQuery(string pCS, QueryParameters pQry, DateTime pDate)
        {
            List<UnderlyingStockReport> lstUnderlyingStock = new List<UnderlyingStockReport>();

            using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, pQry.Query, pQry.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    string xml = Convert.IsDBNull(dr["UNDERLYINGSTOCKS"]) ? null : dr["UNDERLYINGSTOCKS"].ToString();

                    if (StrFunc.IsFilled(xml))
                    {
                        string xsiToRemove = @"xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:type=""efs:EquityAsset""";
                        xml = xml.Replace(xsiToRemove, string.Empty);
                        System.IO.StringReader reader = new System.IO.StringReader(xml);

                        System.Xml.Serialization.XmlRootAttribute root = new System.Xml.Serialization.XmlRootAttribute
                        {
                            ElementName = "underlyingStocks",
                            Namespace = "http://www.efs.org/2007/EFSmL-3-0"
                        };

                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(EfsML.v30.MarginRequirement.UnderlyingStocks), root);
                        EfsML.v30.MarginRequirement.UnderlyingStocks result = (EfsML.v30.MarginRequirement.UnderlyingStocks)serializer.Deserialize(reader);
                        if (ArrFunc.IsFilled(result.underlyingStock))
                        {

                            foreach (EfsML.v30.MarginRequirement.UnderlyingStock item in result.underlyingStock)
                            {
                                lstUnderlyingStock.Add(new UnderlyingStockReport()
                                {
                                    idB = item.bookId.OTCmlId,
                                    OTCmlId = item.OTCmlId,

                                    displayName = string.Empty,
                                    displayNameSpecified = false,
                                    description = string.Empty,
                                    descriptionSpecified = false,

                                    idAsset = item.equity.OTCmlId,

                                    qtyAvailable = item.qtyAvailable,
                                    qtyUsedFut = item.qtyUsedFut,
                                    qtyUsedOpt = item.qtyUsedOpt,
                                }
                                );
                            }
                        }
                    }
                }
            }

            if (lstUnderlyingStock.Count > 0)
            {
                int[] idposEquity = (from item in lstUnderlyingStock
                                     select item.OTCmlId).ToArray();

                string sqlQuery = StrFunc.AppendFormat(@"select IDPOSEQUSECURITY, IDENTIFIER, DISPLAYNAME, DESCRIPTION, POSSTOCKCOVER
                from dbo.POSEQUSECURITY 
                where {0}", DataHelper.SQLColumnIn(pCS, "IDPOSEQUSECURITY", idposEquity, TypeData.TypeDataEnum.integer, false, true));

                DataTable dt = DataHelper.ExecuteDataTable(pCS, sqlQuery);

                foreach (UnderlyingStockReport item in lstUnderlyingStock)
                {
                    string filterExpression = StrFunc.AppendFormat("IDPOSEQUSECURITY={0}", item.otcmlId);
                    DataRow[] row = dt.Select(filterExpression);
                    if (false == ArrFunc.IsFilled(row))
                        throw new Exception(StrFunc.AppendFormat("No Equity found ({0})", filterExpression));

                    item.identifier = Convert.ToString(row[0]["IDENTIFIER"]);
                    item.displayName = (Convert.DBNull == row[0]["DISPLAYNAME"]) ? string.Empty : Convert.ToString(row[0]["DISPLAYNAME"]);
                    item.displayNameSpecified = Convert.DBNull != row[0]["DISPLAYNAME"];

                    item.description = (Convert.DBNull == row[0]["DESCRIPTION"]) ? string.Empty : Convert.ToString(row[0]["DESCRIPTION"]);
                    item.descriptionSpecified = Convert.DBNull != row[0]["DESCRIPTION"];

                    item.posStockCoverEum = Convert.ToString(row[0]["POSSTOCKCOVER"]);
                }
            }

            if (false == NotificationDocument.UnderlyingStocksSpecified)
                NotificationDocument.UnderlyingStocksSpecified = ArrFunc.IsFilled(lstUnderlyingStock);

            if (ArrFunc.IsFilled(lstUnderlyingStock))
                NotificationDocument.UnderlyingStocks.Add(new UnderlyingStocksReport() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), underlyingStock = lstUnderlyingStock });
        }

        /// <summary>
        /// Mise à jour des Labels présents dans les underlyingStock 
        /// </summary>
        // FI 20160613 [22256] add
        private void UpdateUnderlyingStockLabel()
        {
            HeaderFooter reportSettings = null;
            if (NotificationDocument.Header.ReportSettingsSpecified)
                reportSettings = NotificationDocument.Header.ReportSettings.headerFooter;

            if ((null != reportSettings) && reportSettings.underlyingStockSpecified && NotificationDocument.UnderlyingStocksSpecified)
            {
                IEnumerable<IAssetLabel> allItems = from itemUnderlyingStocks in NotificationDocument.UnderlyingStocks
                                                    from item in itemUnderlyingStocks.underlyingStock
                                                    select (IAssetLabel)item;

                UpdateLabelAsset(allItems, reportSettings.underlyingStock, NotificationDocument.Repository, this.Culture);
            }
        }

        /// <summary>
        ///  Mise à jour de la propertie label d'un objet en fonction de la derscription définie pour l'asset 
        /// </summary>
        /// <param name="pAssetLabel">Objet qui possède un label et un asset</param>
        /// <param name="desc">Représente la description de l'asset</param>
        /// <param name="pRepository">Contient l'asset</param>
        /// <param name="pCulture"></param>
        /// FI 20160613 [22256] Add
        private static void UpdateLabelAsset(IEnumerable<IAssetLabel> pAssetLabel, LabelDescription pDesc, IRepository pRepository, string pCulture)
        {
            foreach (IAssetLabel item in pAssetLabel)
            {
                IAssetRepository asset = RepositoryTools.LoadAssetRepository(pRepository, new Pair<int, Cst.UnderlyingAsset>(item.IdAsset, item.AssetCategory));
                if (null == asset)
                    throw new NotSupportedException(StrFunc.AppendFormat("Asset (id:{0}, cat:{0}) not found in repository", item.IdAsset, item.AssetCategory.ToString()));

                CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(pCulture);

                string isinCode = string.Empty;
                switch (item.AssetCategory)
                {
                    case Cst.UnderlyingAsset.Bond:
                        isinCode = ((IAssetDebtSecurityRepository)asset).ISINCode;
                        break;
                    case Cst.UnderlyingAsset.EquityAsset:
                        isinCode = ((IAssetEquityRepository)asset).ISINCode;
                        break;
                    case Cst.UnderlyingAsset.Cash:
                        isinCode = String.Empty;
                        break;
                }

                item.Label = StrFuncExtended.ReplaceObjectField(pDesc.description, new
                {
                    ASSET_CATEGORY = item.AssetCategory,
                    ASSET_IDENTIFIER = asset.Identifier,
                    ASSET_ALTIDENTIFIER = asset.AltIdentifier,
                    ASSET_DISPLAYNAME = asset.Displayname,
                    ASSET_DESCRIPTION = asset.Description,
                    ASSET_ISINCODE = isinCode,

                }, cultureInfo);
                item.LabelSpecified = StrFunc.IsFilled(item.Label);
            }
        }

        /// <summary>
        /// Mise à jour des fmtQty en fonction des caractéristiques des assets
        /// </summary>
        /// FI 20170201 [21916] Add Method
        /// FI 20170217 [22862] Modify
        /// RD 20170426 [23107] Modify
        /// FI 20170531 [XXXXX] Modify
        private void UpdatefmtQty()
        {
            // FI 20191023 [XXXXX] passage à 4 decimales (nécessaire pour les GILTS)
            string defaultFormat = "0.####";

            List<Pair<IAssetRepository, string>> assetfmt = new List<Pair<IAssetRepository, string>>();
            _notificationDocument.GetAllRepositoryAsset().ForEach(x =>
            assetfmt.Add(new Pair<IAssetRepository, string>()
            {
                First = x,
                Second = defaultFormat
            }));

            foreach (Pair<IAssetRepository, string> assetItem in assetfmt.Where(x => x.First.AssetCategory == Cst.UnderlyingAsset.Commodity))
            {
                IAssetCommodityRepository assetCommodity = (IAssetCommodityRepository)assetItem.First;
                if (assetCommodity.QtyScaleSpecified)
                {
                    assetItem.Second = "0.";
                    if (assetCommodity.QtyScale > 0)
                        for (int i = 0; i < assetCommodity.QtyScale; i++)
                            assetItem.Second += "0";
                    assetItem.Second = assetItem.Second.PadRight(5, '#');
                }
            }


            foreach (Pair<IAssetRepository, string> assetItem in assetfmt)
            {
                /*trades/unsettledTrades/settledTrades*/
                IEnumerable<TradesReport> tradesReport =
                from itemTrades in NotificationDocument.Trades.Concat(NotificationDocument.UnsettledTrades).Concat(NotificationDocument.SettledTrades)
                select itemTrades;

                foreach (TradesReport tradesReportItem in tradesReport)
                {
                    foreach (TradeReport tradeItem in tradesReportItem.trade.Where(x => x.idAsset == assetItem.First.OTCmlId && x.assetCategory == assetItem.First.AssetCategory))
                    {
                        tradeItem.fmtQtySpecified = true;
                        tradeItem.fmtQty = tradeItem.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    foreach (PositionSubTotal subTotalItem in tradesReportItem.subTotal.Where(x => x.idAsset == assetItem.First.OTCmlId && x.assetCategory == assetItem.First.AssetCategory))
                    {
                        subTotalItem.@long.fmtQty = subTotalItem.@long.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                        subTotalItem.@short.fmtQty = subTotalItem.@short.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                /*posTrades*/
                // FI 20170531 [XXXXX] concat de stlPosTrades
                IEnumerable<PosTrades> posTrades =
                from itemTrades in NotificationDocument.PosTrades.Concat(NotificationDocument.StlPosTrades)
                select itemTrades;
                foreach (PosTrades posTradesItem in posTrades)
                {
                    foreach (PosTrade posTradeItem in posTradesItem.trade.Where(x => x.qtySpecified && x.idAsset == assetItem.First.OTCmlId && x.assetCategory == assetItem.First.AssetCategory))
                    {
                        posTradeItem.fmtQtySpecified = true;
                        posTradeItem.fmtQty = posTradeItem.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }

                    foreach (PositionSubTotal subTotalItem in posTradesItem.subTotal.Where(x => x.idAsset == assetItem.First.OTCmlId && x.assetCategory == assetItem.First.AssetCategory))
                    {
                        subTotalItem.@long.fmtQty = subTotalItem.@long.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                        subTotalItem.@short.fmtQty = subTotalItem.@short.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                /*dlvTrades*/
                IEnumerable<DlvTrades> dlvTrade =
                from itemTrades in NotificationDocument.DlvTrades
                select itemTrades;
                foreach (DlvTrades dlvTradesItem in dlvTrade)
                {
                    foreach (DlvTrade dlvTradeItem in dlvTradesItem.trade.Where(x => x.qtySpecified && x.idAsset == assetItem.First.OTCmlId && x.assetCategory == assetItem.First.AssetCategory))
                    {
                        dlvTradeItem.fmtQtySpecified = true;
                        dlvTradeItem.fmtQty = dlvTradeItem.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }


                /*posActions*/
                // FI 20170531 [XXXXX] concat de stlPosActions
                foreach (PosActions posActionsItem in NotificationDocument.PosActions.Concat(NotificationDocument.StlPosActions))
                {
                    foreach (PosAction posActionItem in posActionsItem.posAction.Where(x => x.trades.trade.idAsset == assetItem.First.OTCmlId && x.trades.trade.assetCategory == assetItem.First.AssetCategory))
                    {
                        posActionItem.fmtQty = posActionItem.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                /*posSynthetics*/
                foreach (PosSynthetics posSyntheticsItem in NotificationDocument.PosSynthetics)
                {
                    foreach (PosSynthetic posSyntheticItem in posSyntheticsItem.posSynthetic.Where(x => x.IdAsset == assetItem.First.OTCmlId && x.AssetCategory == assetItem.First.AssetCategory))
                    {
                        posSyntheticItem.fmtQty = posSyntheticItem.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }

                /*cashPayments*/
                foreach (CashPayments cashPaymentsItem in NotificationDocument.CashPayments)
                {
                    // RD 20170426 [23107] x.detailSpecified en premier
                    //foreach (CashPayment cashPaymentItem in cashPaymentsItem.cashPayment.Where(x => x.detail.qtySpecified && x.detailSpecified && x.detail.idAsset == assetItem.First.OTCmlId && x.detail.assetCategory == assetItem.First.assetCategory))
                    foreach (CashPayment cashPaymentItem in cashPaymentsItem.cashPayment.Where(x => x.detailSpecified && x.detail.qtySpecified && x.detail.idAsset == assetItem.First.OTCmlId && x.detail.assetCategory == assetItem.First.AssetCategory))
                    {
                        cashPaymentItem.detail.fmtQtySpecified = true;
                        cashPaymentItem.detail.fmtQty = cashPaymentItem.detail.qty.ToString(assetItem.Second, System.Globalization.CultureInfo.InvariantCulture);
                    }
                }


            }


            //FI 20170217 [22862] add 
            /*dlvTrades Physical delivery*/
            foreach (DlvTrades dlvItem in NotificationDocument.DlvTrades)
            {
                foreach (DlvTrade tradeItem in dlvItem.trade.Where(x => x.phyQtySpecified))
                {
                    tradeItem.phyQty.fmtQtySpecified = true;
                    tradeItem.phyQty.fmtQty = tradeItem.phyQty.qty.ToString(defaultFormat, System.Globalization.CultureInfo.InvariantCulture);
                }
            }
        }



        /// <summary>
        ///  Recherche des trades Fut pour lesquels il existe un paiement en rapport avec livraison d'un ss-jacent en date {pdate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdTCashBalance"></param>
        /// <param name="pDate"></param>
        /// FI 20170217 [22862] Add
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// FI 20200519 [XXXXX] Add pIsAddCommonDataTrade
        private void SetDvlTradesReportSynthesis(string pCS, int pIdTCashBalance, DateTime pDate, Boolean pIsAddCommonDataTrade)
        {
            if (productEnv.Count(true) > 0 && productEnv.ExistFUT())
            {
                string additionnalJoin = @"inner join dbo.TRADELINK tlcb on tlcb.IDT_B = t.IDT and tlcb.LINK = 'ExchangeTradedDerivativeInCashBalance' and tlcb.IDT_A = @IDT";

                QueryParameters qry = GetQueryTradeDelivery(pCS, pDate, additionnalJoin);
                qry.Parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), pIdTCashBalance);

                if (StrFunc.IsFilled(qry.Query))
                {
                    DataTable dt = DataHelper.ExecuteDataTable(pCS, qry.Query, qry.Parameters.GetArrayDbParameter());
                    DataRow[] row = dt.Select();
                    if (ArrFunc.IsFilled(row))
                    {
                        AddDeliveryTrades(pCS, NotificationDocument.DlvTrades, pDate, row);
                        NotificationDocument.DlvTradesSpecified = true;
                    }
                }
                
                // FI 20200519 [XXXXX] Alimentation si pIsAddCommonDataTrade
                if (pIsAddCommonDataTrade)
                {
                    List<DlvTrade> dlvTrades = GetDeliveryTrade(pDate);
                    foreach (DlvTrade item in dlvTrades)
                        AddCommonDataTrade(pCS, item);
                }
            }
        }



        /// <summary>
        ///  Ajoute d'un item dans {pLstDeliveryTrades}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="lstPosTrades"></param>
        /// <param name="pDate">Date d'observation</param>
        /// <param name="pDr">Liste des enregistrements pris en considération à la date d'observation</param>
        /// FI 20170217 [22862] Add
        private void AddDeliveryTrades(string pCS, List<DlvTrades> pLstDeliveryTrades, DateTime pDate, DataRow[] pDr)
        {
            List<DlvTrade> lstDeliveryTrade = new List<DlvTrade>();
            DlvTrades deliveryTrades = pLstDeliveryTrades.Find(x => x.bizDt == DtFunc.DateTimeToStringDateISO(pDate));

            foreach (DataRow dr in pDr)
            {
                DlvTrade trade = null;

                if (deliveryTrades != null)
                {
                    trade = deliveryTrades.trade.Find(x => x.OTCmlId == Convert.ToInt32(dr["IDT"]));
                    if (trade != null)
                        continue;
                }

                trade = new DlvTrade();
                InitPosTradeCommon(trade, dr);

                trade.stlPriceSpecified = (false == Convert.IsDBNull(dr["SETTLTPRICE"]));
                if (trade.stlPriceSpecified)
                {
                    trade.stlPrice = Convert.ToDecimal(dr["SETTLTPRICE"]);
                    trade.fmtStlPriceSpecified = true;
                    trade.fmtStlPrice = BuildFmtPrice(pCS,
                                new Pair<Cst.UnderlyingAsset, int>(trade.assetCategory, trade.idAsset), trade.stlPrice, null);
                }

                //DVA (paiement)
                trade.dvaSpecified = (dr["DVAAMOUNT"] != Convert.DBNull);
                if (trade.dvaSpecified)
                {
                    trade.dva = new ReportAmountSide
                    {
                        amount = Convert.ToDecimal(dr["DVAAMOUNT"]),
                        currency = Convert.ToString(dr["DVAUNIT"]),
                        side = (CrDrEnum)System.Enum.Parse(typeof(CrDrEnum), Convert.ToString(dr["DVASIDE"]))
                    };
                    trade.dva.sideSpecified = (trade.dva.amount > 0);
                }

                //PHY (Qté livrée)
                trade.phyQtySpecified = (dr["PHYQTY"] != Convert.DBNull);
                if (trade.phyQtySpecified)
                {
                    trade.phyQty = new PhysicalDelivery
                    {
                        qty = Convert.ToDecimal(dr["PHYQTY"]),
                        qtyUnitSpecified = (dr["PHYQTYUNIT"] != Convert.DBNull),
                        qtyUnit = dr["PHYQTYUNIT"].ToString()
                    };
                }

                lstDeliveryTrade.Add(trade);
            }

            if (ArrFunc.IsFilled(lstDeliveryTrade))
            {

                if (deliveryTrades == null)
                {
                    deliveryTrades = new DlvTrades() { bizDt = DtFunc.DateTimeToStringDateISO(pDate), trade = lstDeliveryTrade };
                    pLstDeliveryTrades.Add(deliveryTrades);
                }
                else
                    deliveryTrades.trade.AddRange(lstDeliveryTrade);

                // FI 20170217 [22862] pas de calcul de moyenne  (du moins pour l'instant)
                //foreach (PosTrade item in lstTradePosition)
                //    CalcPosTradesSubSubTotal(pCS, posTrades, item);
            }
        }

        /// <summary>
        /// Retourne la requête qui charge les trades
        /// <para>Les trades annulés sont exclus</para>
        /// <para>Les trades "Position Opening" résultants d'une initialisation du progiciel Spheres® sont exclus</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pAdditionalJoin"></param>
        /// <param name="pDate"></param>
        /// <param name="pGproduct">Restriction à liste de GPRODUCT</param>
        /// <param name="pReportType"></param>
        /// <returns></returns>
        /// FI 20150825 [21287] Add (Remplace GetQueryTradesReport)
        /// FI 20151019 [21317] Modify 
        /// FI 20151019 [21317] Modify 
        /// FI 20160225 [XXXXX] Modify 
        /// RD 20161212 [22660] Modify
        /// FI 20161214 [21916] Modify
        /// FI 20170217 [22859] Modify
        /// FI 20180327 [23839] Modify
        /// FI 20190201 [24495] Add GetInfoConnection 
        /// FI 20190212 [24528] GetQueryTradesReport3 remplace GetQueryTradesReport2 (la Requête dynamique en foinction de l'activité (COM,SEC) et fonction de pReportType
        /// EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private static QueryParameters GetQueryTradesReport3(string pCS,  string pAdditionalJoin, DateTime pDate, TradeReportTypeEnum pReportType, CashBalanceProductEnv pCashBalanceProductEnv)
        {
            // RD 20130722 [18745] 
            // FI 20140731 [XXXXX] use TRADEINSTRUMENT.IDB_DEALER
            // FI 20140813 [20275] la requête récupère ASSETCATEGORY
            // FI 20150218 [20275] Alimentation de la colonne IDT_SOURCE (trade source en lorsque le trade est le résultat d'un transfert Interne)
            // FI 20150218 [20275] Alimentation des colonnes IDI, GPRODUCT, DTTRADE, DTBUSINESS, TRDTYPE, SRC_IDT, SRC_IDENTIFIER
            // FI 20150316 [20275] Add and (ec.EVENTCLASS='REC') and GAM
            // FI 20150331 [XXPOC] Gestion des Option sur FX et des FXLG add  or (e.EVENTTYPE='CU1') or (e.EVENTTYPE='CCU')
            // FI 20150316 [20275] Add DTOUTADJ
            /*
            // FI 20150617 [21124] Mise en place du where t.DTBUSINESS = @DT en lieu et place de la jointure suivante 
                                                    inner join dbo.EVENT e on (e.IDT=t.IDT) and (e.EVENTCODE='STA') and ((e.EVENTTYPE='QTY') or (e.EVENTTYPE='CU1') or (e.EVENTTYPE='CCU'))
                                                    inner join dbo.EVENTCLASS ec on (ec.IDE=e.IDE) and (ec.DTEVENT=@DT) and (ec.EVENTCLASS='REC')
              Sur la famille Equity le STAT/QTY	REC	est TrdDt et  STAT/QTY	VAL/STL sont en Settldt
            */
            // FI 20150702 [XXXXX] Suppression de la lecture de la date STL sur les évènement GAM  et PRM 
            // FI 20150825 [21287] sur les unsetttledTrade et setttledTrade, la qté et le GAM prennent en considération les éventuelles corrections et transferts
            // FI 20151019 [21317] add FAMILY,add AIN (Montant des intérêts courus sur DebtSecurityTransaction)
            // FI 20151019 [21317] add #TradeReportType# et mise en place d'un UNIONALL si pReportType= TradeReportTypeEnum.all
            // FI 20151019 [21317] add PAM
            // FI 20160225 [XXXXX] jointure sur TRADEINSTRUMENT identique au fichier XML (MCO_RIMGEN.xml) 
            // RD 20161212 [22660] Add the ENTRY action to the Where clause
            // FI 20161214 [21916] Add QTYUNIT
            // FI 20170217 [22859] Alimentation de trade source lorsque le trades est le résultat d'un cascading ou shifting
            // FI 20180327 [23839] Modification de l'alimentation de POSRESULT
            // FI 20190201 [24495] sur Oracle 11g seule la requête "tradeOfDay" est supportée. Sur Oracle12C et plus, toutes les requêtes fonctionnent   


            SvrInfoConnection svrInfoConnection = DataHelper.GetSvrInfoConnection(pCS);
            bool isOraDBVer11OrLower = (svrInfoConnection.IsOracle) && !(svrInfoConnection.IsOraDBVer12cR1OrHigher);
            if (isOraDBVer11OrLower)
            {
                if (((pReportType & TradeReportTypeEnum.unsetttledTrade) == TradeReportTypeEnum.unsetttledTrade) ||
                    ((pReportType & TradeReportTypeEnum.setttledTrade) == TradeReportTypeEnum.setttledTrade))
                    throw new NotSupportedException(StrFunc.AppendFormat("Oracle version {0} is not supported", svrInfoConnection.ServerVersion));
            }

            // FI 20231222 [WI788] Alimentation de POSRESULT lorsque RequestType = 'CLOSINGPOS' (évènement 'OCP')
            string queryBase = StrFunc.AppendFormat(@"select '#TradeReportType#' as TRADEREPORTTYPE, 
            t.IDT, t.IDENTIFIER, t.DTTRADE, t.DTBUSINESS, t.DTSETTLT,
            p.GPRODUCT, p.FAMILY,
            ns.FUNGIBILITYMODE,
            t.IDI, t.ASSETCATEGORY, t.IDASSET, t.IDB_DEALER as IDB, case t.SIDE when '1' then 'Buy' else 'Sell' end as BUYSELL, t.PRICE, 
            #QTY#, t.UNITQTY as QTYUNIT, t.TRDTYPE, t.TRDSUBTYPE, t.DTOUTADJ, 
            prm.VALORISATION as PRMAMOUNT, prm.UNIT as PRMUNIT,
            case when prm.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as PRMSIDE,
            #GAM#,
            #AIN#,
            #PAM#,
            case when (ns.FUNGIBILITYMODE = 'NONE') then
                'None'
                 when exists(
                               select 1
                               from dbo.POSREQUEST pos 
                               where (pos.DTBUSINESS={4}) and (pos.REQUESTTYPE in('ENTRY','UPDENTRY','EOD_UPDENTRY'))
                               ) then 
                                case when exists (
                                                   select 1
                                                   from  dbo.EVENT ofs 
                                                   inner join dbo.POSACTIONDET pad on (pad.IDT_CLOSING=ofs.IDT)
									               inner join dbo.POSACTION pa  on (pa.IDPA=pad.IDPA) 
									               inner join dbo.POSREQUEST pos  on (pos.IDPR = pa.IDPR) and (pos.DTBUSINESS = {4}) and (pos.REQUESTTYPE in('ENTRY','UPDENTRY','EOD_UPDENTRY'))
									               inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ofs.IDE) and (epad.IDPADET = pad.IDPADET)
									               where (ofs.IDT= t.IDT) and (ofs.EVENTCODE='OFS') and (ofs.EVENTTYPE='TOT')
                                                   union all
                                                   select 1
                                                   from  dbo.EVENT ofs 
                                                   inner join dbo.POSACTIONDET pad on (pad.IDT_CLOSING=ofs.IDT)
									               inner join dbo.POSACTION pa on (pa.IDPA=pad.IDPA) 
									               inner join dbo.POSREQUEST pos on (pos.IDPR = pa.IDPR) and (pos.DTBUSINESS = {4}) and (pos.REQUESTTYPE ='CLOSINGPOS')
									               inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ofs.IDE) and (epad.IDPADET = pad.IDPADET)
									               where (ofs.IDT= t.IDT) and (ofs.EVENTCODE ='OCP') and (ofs.EVENTTYPE='TOT')
                                                 ) then 'Close' 
                                     when exists (

                                                   select 1
                                                   from  dbo.EVENT ofs 
                                                   inner join dbo.POSACTIONDET pad on (pad.IDT_CLOSING=ofs.IDT)
									               inner join dbo.POSACTION pa  on (pa.IDPA=pad.IDPA) 
									               inner join dbo.POSREQUEST pos  on (pos.IDPR = pa.IDPR) and (pos.DTBUSINESS = {4}) and (pos.REQUESTTYPE in('ENTRY','UPDENTRY','EOD_UPDENTRY'))
									               inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ofs.IDE) and (epad.IDPADET = pad.IDPADET)
                                                   where (ofs.IDT= t.IDT) and (ofs.EVENTCODE='OFS') and (ofs.EVENTTYPE='PAR')
                                                   union  all
                                                   select 1
                                                   from  dbo.EVENT ofs 
                                                   inner join dbo.POSACTIONDET pad on (pad.IDT_CLOSING=ofs.IDT)
									               inner join dbo.POSACTION pa  on (pa.IDPA=pad.IDPA) 
									               inner join dbo.POSREQUEST pos  on (pos.IDPR = pa.IDPR) and (pos.DTBUSINESS = {4}) and (pos.REQUESTTYPE ='CLOSINGPOS')
									               inner join dbo.EVENTPOSACTIONDET epad on (epad.IDE = ofs.IDE) and (epad.IDPADET = pad.IDPADET)
                                                   where (ofs.IDT= t.IDT) and (ofs.EVENTCODE='OCP') and (ofs.EVENTTYPE='PAR')
                                                ) then 'Partial Close' 
                                     else 'Open' 
                                     end 
                 else 'Unavailable' 
            end as POSRESULT,
            tscr.IDT as SRC_IDT, tscr.IDENTIFIER as SRC_IDENTIFIER
            from dbo.TRADE t
            inner join dbo.INSTRUMENT ns on ns.IDI = t.IDI 
            inner join dbo.PRODUCT p on p.IDP = ns.IDP  #GPRODUCT_RESTRICTION#
            left outer join dbo.EVENT prm on (prm.IDT=t.IDT) and (prm.EVENTCODE='LPP') and (prm.EVENTTYPE='PRM')
            left outer join dbo.EVENT gam on (gam.IDT=t.IDT) and (gam.EVENTCODE='LPP') and (gam.EVENTTYPE='GAM')
            left outer join dbo.EVENT pam on (pam.IDT=t.IDT) and (pam.EVENTCODE='LPP') and (pam.EVENTTYPE='PAM')
            left outer join dbo.EVENT ain on (ain.IDT=t.IDT) and (ain.EVENTCODE='LPP') and (ain.EVENTTYPE='AIN')
            left outer join dbo.TRADELINK tl on tl.IDT_A = t.IDT and tl.LINK in('{0}','{1}','{2}')
            left outer join dbo.TRADE tscr on (tscr.IDT = tl.IDT_B) 
            {3}", TradeLinkType.PositionTransfert, TradeLinkType.PositionAfterCascading, TradeLinkType.PositionAfterShifting, 
            pAdditionalJoin, isOraDBVer11OrLower ? "@DT" : "t.DTBUSINESS");


            // FI 20151019 [21317] reecriture 
            // FI 20151019 [21317] add AIN
            // FI 20161214 [21916] add gamval alias
            string queryBase2SEC = queryBase + @"
            left outer join dbo.EVENTCLASS gamval on (gamval.IDE = gam.IDE) and (gamval.EVENTCLASS='VAL')
            inner join 
            ( 
                select tr.IDT, isnull(pa.QTY,0) as PAQTY, isnull(pa.GAM,0) as PAGAM, isnull(pa.AIN,0) as PAAIN, isnull(pa.PAM,0) as PAPAM
                from dbo.TRADE tr
                inner join dbo.VW_INSTR_PRODUCT pr on ( pr.IDI = tr.IDI) and (pr.GPRODUCT = 'SEC')
                left outer join (
                    select pad.IDT_CLOSING as IDT, 
                    sum(isnull(pad.QTY,0)) as QTY,
                    sum(isnull(case when trclosing.IDA_DEALER=gam.IDA_PAY then -1 else 1 end * gam.VALORISATION,0)) as GAM,
                    sum(isnull(case when trclosing.IDA_DEALER=ain.IDA_PAY then -1 else 1 end * ain.VALORISATION,0)) as AIN,
                    sum(isnull(case when trclosing.IDA_DEALER=pam.IDA_PAY then -1 else 1 end * pam.VALORISATION,0)) as PAM
                    from dbo.POSACTIONDET pad
                    inner join dbo.TRADE trclosing on (trclosing.IDT = pad.IDT_CLOSING)
                    inner join dbo.POSACTION pa on (pa.IDPA = pad.IDPA) 
                    inner join dbo.POSREQUEST pr on pr.IDPR = pa.IDPR and pr.REQUESTTYPE in ('POC','POT')
                    left outer join 
                    ( 
			             select gam.VALORISATION, gam.IDA_PAY, gam.IDA_REC, evpad.IDPADET  
			             from dbo.EVENT gam
			             inner join dbo.EVENTPOSACTIONDET evpad on evpad.IDE = gam.IDE
			             where  (gam.EVENTCODE='LPC') and (gam.EVENTTYPE='GAM') 
                    ) gam on gam.IDPADET = pad.IDPADET 
                    left outer join 
                    ( 
			             select ain.VALORISATION, ain.IDA_PAY, ain.IDA_REC, evpad.IDPADET  
			             from dbo.EVENT ain
			             inner join dbo.EVENTPOSACTIONDET evpad on evpad.IDE = ain.IDE
			             where  (ain.EVENTCODE='LPC') and (ain.EVENTTYPE='AIN') 
                    ) ain on ain.IDPADET = pad.IDPADET 
                    left outer join 
                    ( 
			             select pam.VALORISATION, pam.IDA_PAY, pam.IDA_REC, evpad.IDPADET  
			             from dbo.EVENT pam
			             inner join dbo.EVENTPOSACTIONDET evpad on evpad.IDE = pam.IDE
			             where  (pam.EVENTCODE='LPC') and (pam.EVENTTYPE='PAM') 
                    ) pam on pam.IDPADET = pad.IDPADET 

                    where (pa.DTBUSINESS <= @DT) and ((pad.DTCAN is null) or (pad.DTCAN > @DT))
                    group by pad.IDT_CLOSING
                ) pa on (pa.IDT = tr.IDT)
                where (tr.IDSTBUSINESS='ALLOC') and (tr.IDSTENVIRONMENT='REGULAR') and (tr.IDSTACTIVATION='REGULAR') and ((tr.QTY - isnull(pa.QTY,0)) > 0) and (@DT <= tr.DTOUTADJ)

            ) posdet on posdet.IDT = t.IDT";


            queryBase = queryBase.Replace("#GAM#",
            @"gam.VALORISATION as GAMAMOUNT, gam.UNIT as GAMUNIT, case when gam.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end as GAMSIDE");
            queryBase = queryBase.Replace("#AIN#",
            @"case when p.FAMILY='DSE' then ain.VALORISATION else null end as AINAMOUNT, 
              case when p.FAMILY='DSE' then ain.UNIT else null end as AINUNIT, 
              case when p.FAMILY='DSE' then case when ain.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end else null end as AINSIDE");

            queryBase = queryBase.Replace("#PAM#",
            @"case when p.FAMILY='DSE' then pam.VALORISATION else null end as PAMAMOUNT, 
              case when p.FAMILY='DSE' then pam.UNIT else null end as PAMUNIT, 
              case when p.FAMILY='DSE' then case when pam.IDB_PAY = t.IDB_DEALER then 'DR' else 'CR' end else null end as PAMSIDE");
            queryBase = queryBase.Replace("#QTY#", "t.QTY");

            // FI 20161214 [21916] add queryBase2COM
            string queryBase2COM = queryBase + Cst.CrLf + @"left outer join dbo.EVENTCLASS gamval on (gamval.IDE = gam.IDE) and (gamval.EVENTCLASS='VAL')";


            queryBase2SEC = queryBase2SEC.Replace("#GAM#",
                @"ABS((case when t.IDA_DEALER = gam.IDA_PAY then -1 else 1 end * isnull(gam.VALORISATION,0)) + posdet.PAGAM) as GAM, 
                   gam.UNIT as GAMUNIT,
                   case when (case when t.IDA_DEALER = gam.IDA_PAY then -1 else 1 end * isnull(gam.VALORISATION,0)) + posdet.PAGAM <=0 then 'DR'  else 'CR' end as GAMSIDE");

            queryBase2SEC = queryBase2SEC.Replace("#AIN#",
                @"case when p.FAMILY='DSE' then
                        ABS((case when t.IDA_DEALER = ain.IDA_PAY then -1 else 1 end * isnull(ain.VALORISATION,0)) + posdet.PAAIN) 
                   else null end as AINAMOUNT, 
                   case when p.FAMILY='DSE' then                   
                        ain.UNIT 
                   else null end as AINUNIT,
                   case when p.FAMILY='DSE' then
                        case when (case when t.IDA_DEALER = ain.IDA_PAY then -1 else 1 end * isnull(ain.VALORISATION,0)) + posdet.PAAIN <=0 then 'DR'  else 'CR' end 
                   else null end as AINSIDE");

            queryBase2SEC = queryBase2SEC.Replace("#PAM#",
                @"case when p.FAMILY='DSE' then
                        ABS((case when t.IDA_DEALER = pam.IDA_PAY then -1 else 1 end * isnull(pam.VALORISATION,0)) + posdet.PAPAM) 
                  else null end as PAMAMOUNT, 
                  case when p.FAMILY='DSE' then
                        pam.UNIT 
                  else null end as PAMUNIT,
                  case when p.FAMILY='DSE' then
                        case when (case when t.IDA_DEALER = pam.IDA_PAY then -1 else 1 end * isnull(pam.VALORISATION,0)) + posdet.PAPAM <=0 then 'DR'  else 'CR' end 
                  else null end as PAMSIDE");

            queryBase2SEC = queryBase2SEC.Replace("#QTY#", "t.QTY - posdet.PAQTY");


            // FI 20161214 [21916] Replace de #GPRODUCT_RESTRICTION#
            queryBase = queryBase.Replace("#GPRODUCT_RESTRICTION#", string.Empty);
            queryBase2COM = queryBase2COM.Replace("#GPRODUCT_RESTRICTION#", "and p.GPRODUCT='COM'");
            queryBase2SEC = queryBase2SEC.Replace("#GPRODUCT_RESTRICTION#", "and p.GPRODUCT='SEC'");


            //tradeOfDay
            SQLWhere sqlWhere = new SQLWhere("(t.DTBUSINESS = @DT) and (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR') and (t.IDSTACTIVATION = 'REGULAR')");
            string queryTradeOfDay = queryBase.Replace("#TradeReportType#", TradeReportTypeEnum.tradeOfDay.ToString());
            queryTradeOfDay += sqlWhere.ToString();
            queryTradeOfDay += @" and ((t.TRDTYPE is null) or (t.TRDTYPE != '1000') or(t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))"; 


            //unsetttledTrade
            // FI 20161214 [21916]  Utilisation de la date valeur du grossAmount
            //sqlWhere = new SQLWhere("((t.DTBUSINESS<=@DT) and (ti.DTSETTLT is not null and ti.DTSETTLT>@DT))");
            sqlWhere = new SQLWhere("(t.DTBUSINESS <= @DT) and (gamval.DTEVENT > @DT) and (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR') and (t.IDSTACTIVATION = 'REGULAR')");
            string queryUnsetttledTrade = String.Empty;
            if ((null == pCashBalanceProductEnv) || pCashBalanceProductEnv.ExistSEC())
            {
                queryUnsetttledTrade += queryBase2SEC.Replace("#TradeReportType#", TradeReportTypeEnum.unsetttledTrade.ToString());
                queryUnsetttledTrade += sqlWhere.ToString();
            }
            if ((null == pCashBalanceProductEnv) || pCashBalanceProductEnv.ExistCOM())
            {
                if (StrFunc.IsFilled(queryUnsetttledTrade))
                    queryUnsetttledTrade += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                queryUnsetttledTrade += queryBase2COM.Replace("#TradeReportType#", TradeReportTypeEnum.unsetttledTrade.ToString());
                queryUnsetttledTrade += sqlWhere.ToString();
            }
            queryUnsetttledTrade += @" and ((t.TRDTYPE is null) or (t.TRDTYPE != '1000') or(t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))";



            //setttledTrade
            // RD/PL 20160617 [22244] Dans la section setttledTrade doivent apparaitre les trades suivants:
            // 1- Trades avec une date de règlement égale à la date business en cours (donc trades du jour et antérieurs)
            // 2- Trades du jour avec date de règlement inférieur à la date business en cours (cas des trades saisie en retard disposant d’un flux "passé")
            //sqlWhere = new SQLWhere("((t.DTBUSINESS<=@DT) and (ti.DTSETTLT is not null and ti.DTSETTLT=@DT))");
            // FI 20161214 [21916]  Utilisation de la date valeur du grossAmount
            //sqlWhere = new SQLWhere("((ti.DTSETTLT=@DT) or (t.DTBUSINESS=@DT and ti.DTSETTLT<@DT))");
            sqlWhere = new SQLWhere("((gamval.DTEVENT = @DT) or (t.DTBUSINESS = @DT and gamval.DTEVENT < @DT)) and (t.IDSTBUSINESS = 'ALLOC') and (t.IDSTENVIRONMENT = 'REGULAR') and (t.IDSTACTIVATION = 'REGULAR')");
            string querySetttledTrade = string.Empty;
            if ((null == pCashBalanceProductEnv) || pCashBalanceProductEnv.ExistSEC())
            {
                querySetttledTrade += queryBase2SEC.Replace("#TradeReportType#", TradeReportTypeEnum.setttledTrade.ToString());
                querySetttledTrade += sqlWhere.ToString();
            }
            if ((null == pCashBalanceProductEnv) || pCashBalanceProductEnv.ExistCOM())
            {
                if (StrFunc.IsFilled(querySetttledTrade))
                    querySetttledTrade += Cst.CrLf + SQLCst.UNIONALL + Cst.CrLf;
                querySetttledTrade += queryBase2COM.Replace("#TradeReportType#", TradeReportTypeEnum.setttledTrade.ToString());
                querySetttledTrade += sqlWhere.ToString();
            }
            querySetttledTrade += @" and ((t.TRDTYPE is null) or (t.TRDTYPE != '1000') or(t.TRDTYPE = '1000' and t.TRDSUBTYPE is not null))";

            // FI 20190515 [23912] add TradeOfOrder
            // RD 20190718 [23912] Use DTTRADE instead of DTBUSINESS
            // RD 20190718 [23912] Don't use sqlWhere, we considere all trades of the order            
            //TradeOfOrder
            //sqlWhere = new SQLWhere("(t.DTBUSINESS<=@DT)");
            //sqlWhere = new SQLWhere("(t.DTTRADE<=@DT)");
            string queryTradeOfOrder = queryBase.Replace("#TradeReportType#", TradeReportTypeEnum.tradeOfOrder.ToString());
            //queryTradeOfOrder += sqlWhere.ToString();
            
            string query = string.Empty;
            if (((pReportType & TradeReportTypeEnum.tradeOfDay) == TradeReportTypeEnum.tradeOfDay) && StrFunc.IsFilled(queryTradeOfDay))
                query += queryTradeOfDay;

            if (((pReportType & TradeReportTypeEnum.unsetttledTrade) == TradeReportTypeEnum.unsetttledTrade) && StrFunc.IsFilled(queryUnsetttledTrade))
            {
                if (StrFunc.IsFilled(query))
                    query += Cst.CrLf + SQLCst.UNIONALL;
                query += queryUnsetttledTrade;
            }

            if (((pReportType & TradeReportTypeEnum.setttledTrade) == TradeReportTypeEnum.setttledTrade) && StrFunc.IsFilled(querySetttledTrade))
            {
                if (StrFunc.IsFilled(query))
                    query += Cst.CrLf + SQLCst.UNIONALL;

                query += querySetttledTrade;
            }
            // FI 20190515 [23912] add TradeOfOrder
            if (((pReportType & TradeReportTypeEnum.tradeOfOrder) == TradeReportTypeEnum.tradeOfOrder) && StrFunc.IsFilled(queryTradeOfOrder))
            {
                if (StrFunc.IsFilled(query))
                    query += Cst.CrLf + SQLCst.UNIONALL;
                query += queryTradeOfOrder;
            }

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DT), pDate);
            QueryParameters qry = new QueryParameters(pCS, query, dp);

            return qry;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pNotificationClass"></param>
        /// <param name="pNotificationType"></param>
        /// <returns></returns>
        /// FI 20200520 [XXXXX] Add
        private static DataDocumentContainer BuildTrade(string pCS, int pIdT, NotificationClassEnum pNotificationClass, Nullable<NotificationTypeEnum> pNotificationType)
        {

            //Charge le Trade en cours
            EFS_TradeLibrary tradeLibrary = new EFS_TradeLibrary(pCS, null, pIdT);
            DataDocumentContainer tradeDataDocument = tradeLibrary.DataDocument;

            switch (pNotificationClass)
            {
                case NotificationClassEnum.MULTIPARTIES:
                case NotificationClassEnum.MULTITRADES:
                    InitRptSide(pCS, tradeDataDocument);
                    break;
                case NotificationClassEnum.MONOTRADE:
                    // FI 20190515 [23912] call InitRptSide
                    if (pNotificationType.HasValue && pNotificationType.Value == NotificationTypeEnum.ORDERALLOC)
                        InitRptSide(pCS, tradeDataDocument);
                    break;
            }

            if (tradeDataDocument.CurrentProduct.IsExchangeTradedDerivative)
            {
                // FI 20141217 [20574] call UpdateMissingAllocationUTI
                tradeDataDocument.UpdateMissingAllocationUTI(pCS, pIdT);
            }

            //Alimentation de tradeSide si inexistant pour le Trade en cours
            if (false == tradeDataDocument.CurrentTrade.TradeSideSpecified)
            {
                // FI 20200520 [XXXXX] Add SQL cache
                tradeDataDocument.SetTradeSide(CSTools.SetCacheOn(pCS));
            }

            //Alimentation de Documentation pour le Trade en cours
            if ((false == tradeDataDocument.CurrentTrade.DocumentationSpecified) ||
                (false == tradeDataDocument.CurrentTrade.Documentation.MasterAgreementSpecified))
            {
                // FI 20200520 [XXXXX] Add SQL cache
                tradeDataDocument.SetDocumentation(CSTools.SetCacheOn(pCS));
            }

            //FI 20120731 [18048] Call UpdateTradeforFinancialReportPeriodic
            //FI 20130621 [18745] mise en commentaire cette méthode est appellée plus tard dans le processus
            //if (pNotificationType.HasValue && pNotificationType.Value == NotificationTypeEnum.FINANCIALPERIODIC)
            //    UpdateTradeforFinancialReportPeriodic(pCS, tradeDataDocument.currentTrade);
            SetTradeId(tradeDataDocument.CurrentTrade);

            if (tradeDataDocument.CurrentProduct.IsTradeMarket)
            {
                SQL_TradeTransaction sqlTrade = new SQL_TradeTransaction(pCS, pIdT);
                if (sqlTrade.LoadTable(new string[] { "TRADE.IDT", "DTTRADE", "DTBUSINESS", "DTEXECUTION", "DTORDERENTERED", "DTTIMESTAMP", "TZFACILITY" }))
                {
                    // EG 20240531 [WI926] Add Parameter pIsTemplate
                    tradeDataDocument.UpdateMissingTimestampAndFacility(pCS, sqlTrade, false);
                    // Mise à jour 
                    IPartyTradeInformation partyTradeInformation = tradeDataDocument.GetPartyTradeInformationFacility();
                    if ((null != partyTradeInformation) && partyTradeInformation.ExecutionDateTimeSpecified)
                    {
                        // FI 20200520 [XXXXX] Add SQL cache
                        string tzdbid = tradeDataDocument.GetTradeTimeZone(CSTools.SetCacheOn(pCS));
                        // FI 20180110 [23708] Utilisation de TimeZoneInfo.ConvertTimeFromUtc
                        // Il ne faut pas utiliser dtExecutionInTimeZone.Value.LocalDateTime qui est la date selon le fuseau du serveur
                        //Nullable<DateTimeOffset> dtExecutionInTimeZone = Tz.Tools.FromTimeZone(partyTradeInformation.executionDateTimeOffset.Value, tzdbid);
                        //string executionDateTimeValue = Tz.Tools.ToString(dtExecutionInTimeZone.Value.LocalDateTime);

                        TimeZoneInfo tzInfo = Tz.Tools.GetTimeZoneInfoFromTzdbId(tzdbid);
                        DateTime executionDateTime = TimeZoneInfo.ConvertTimeFromUtc(partyTradeInformation.ExecutionDateTimeOffset.Value.UtcDateTime, tzInfo);
                        string executionDateTimeValue = Tz.Tools.ToString(executionDateTime);

                        tradeDataDocument.CurrentTrade.TradeHeader.TradeDate.Value = Tz.Tools.DateToStringISO(executionDateTimeValue);
                        tradeDataDocument.CurrentTrade.TradeHeader.TradeDate.TimeStampHHMMSS = Tz.Tools.TimeToStringISO(executionDateTimeValue);
                    }
                }
            }

            return tradeDataDocument;
        }

        #endregion
    }
}
