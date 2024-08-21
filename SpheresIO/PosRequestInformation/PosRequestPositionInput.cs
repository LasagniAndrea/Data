// PM 20180219 [23824] Déplacé dans CommonIO

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using EFS.Actor;
//using EFS.ApplicationBlocks.Data;
//using EFS.ACommon;
//using EFS.Common;
 
//using EFS.GUI.CCI;
//using EFS.GUI.Interface;

//using EfsML.Enum;
//using FixML.Enum;

//using FixML.Interface;
//using FpML.Interface;
//using EfsML.Interface;

//using EfsML.Business;
//using EfsML;



//// TODO FI 20130321 cette espace de nom doit sortir de SpheresIO
//// Il faudra nécessairement le faire lorsque l'on faudra offrir une saisie de POSREQUEST sur une position 
//namespace EFS.PosRequestInformation
//{
//    /// <summary>
//    /// Représente une saisie d'une demande POSREQUEST sur position
//    /// </summary>
//    public class PosRequestPositionInput : ICustomCaptureInfos
//    {
//        #region Members
//        #endregion Members

//        #region accessors
//        /// <summary>
//        /// 
//        /// </summary>
//        public IPosRequestPositionDocument dataDocument
//        {
//            get;
//            set;
//        }
        
//        /// <summary>
//        /// 
//        /// </summary>
//        public PosRequestCustomCaptureInfos customCaptureInfos
//        {
//            set;
//            get;
//        }

//        /// <summary>
//        /// Obtient l'instrument du POSREQUEST
//        /// </summary>
//        public SQL_Instrument sqlInstrument
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Obtient le produit associé à l'instruement du POSREQUEST
//        /// </summary>
//        public SQL_Product sqlProduct
//        {
//            get;
//            private set;
//        }

//        /// <summary>
//        /// Obtient la nature Option/Future de l'instrument du POSREQUEST lorsque ce dernier est un ETD
//        /// </summary>
//        public Nullable<CfiCodeCategoryEnum> categorie
//        {
//            private set;
//            get;
//        }
//        #endregion accessors

//        #region constructor
//        /// <summary>
//        /// 
//        /// </summary>
//        public PosRequestPositionInput()
//        {
//            customCaptureInfos = new PosRequestCustomCaptureInfos();
//            dataDocument = Tools.GetNewProductBase().CreatePosRequestPositionDocument();
//        }
//        #endregion

//        #region ICustomCaptureInfos Membres
//        public CustomCaptureInfosBase CcisBase
//        {
//            get { return (CustomCaptureInfosBase)customCaptureInfos; }
//        }
//        #endregion

//        #region Method

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pCS"></param>
//        /// <param name="pIdPR"></param>
//        public IPosRequest LoadDataDocument(string pCS, int pIdPR)
//        {
//            IProductBase productBase = Tools.GetNewProductBase();
//            this.dataDocument = productBase.CreatePosRequestPositionDocument();

//            IPosRequest posRequest = PosKeepingTools.GetPosRequest(pCS, null, productBase, pIdPR);
//            if (null != posRequest)
//            {
//                if (false == (posRequest.posKeepingKeySpecified))
//                    throw new Exception(StrFunc.AppendFormat("PosRequest (id:{0}) doesn't contains posKeepingKey", posRequest.idPR));

//                SetInstrument(pCS, SQL_TableWithID.IDType.Id, posRequest.posKeepingKey.idI.ToString());

//                SetDataDocument(CSTools.SetCacheOn(pCS), posRequest, productBase);
//            }
//            return posRequest;
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pCS"></param>
//        /// <param name="pType"></param>
//        /// <param name="pIdentifier"></param>
//        public void SetInstrument(string pCS, SQL_TableWithID.IDType pType, string pIdentifier)
//        {
//            if (StrFunc.IsEmpty(pIdentifier))
//                throw new ArgumentException("empty value for parameter pIdentifier is not allowed");

//            sqlInstrument = new SQL_Instrument(pCS, pType, pIdentifier, SQL_Table.RestrictEnum.No,
//                                                        SQL_TableWithID.ScanDataDtEnabledEnum.No, null, string.Empty);
//            if (false == sqlInstrument.LoadTable())
//                throw new Exception("Instrument not found");

//            sqlProduct = new SQL_Product(CSTools.SetCacheOn(pCS), sqlInstrument.IdP);
//            if (false == sqlProduct.LoadTable())
//                throw new Exception("product not found");

//            categorie = Tools.GetETDInstrumentCategory(sqlInstrument);

//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pCS"></param>
//        /// <param name="posRequest"></param>
//        /// <param name="productBase"></param>
//        public void SetDataDocument(string pCS, IPosRequest posRequest, IProductBase productBase)
//        {
//            PosRequestContainer posRequestContainer = new PosRequestContainer(posRequest);

//            dataDocument.requestType = posRequest.requestType;
//            dataDocument.requestMode = posRequest.requestMode;
//            dataDocument.clearingBusinessDate.DateValue = posRequest.dtBusiness;
//            // EG 20150920 [21374] Int (int32) to Long (Int64) 
//            // EG 20170127 Qty Long To Decimal
//            dataDocument.qty.DecValue = posRequest.qty;

//            //Entity
//            if (posRequest.posKeepingKey.idA_EntityDealer == 0)
//                throw new Exception("PosRequest doesn't constains Entity");
//            SQL_Actor sqlActorEntity = new SQL_Actor(pCS, posRequest.posKeepingKey.idA_EntityDealer);
//            if (false == sqlActorEntity.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
//                throw new Exception("Entity not found");
//            Tools.SetActorId(dataDocument.actorEntity, sqlActorEntity);

//            //Dealer
//            dataDocument.actorDealerSpecified = (posRequest.posKeepingKey.idA_Dealer > 0);
//            if (dataDocument.actorDealerSpecified)
//            {
//                SQL_Actor sqlActor = new SQL_Actor(pCS, posRequest.posKeepingKey.idA_Dealer);
//                if (false == sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
//                    throw new Exception("Dealer not found");
//                Tools.SetActorId(dataDocument.actorDealer, sqlActor);
//            }

//            dataDocument.bookDealerSpecified = (posRequest.posKeepingKey.idB_Dealer > 0);
//            if (dataDocument.bookDealerSpecified)
//            {
//                SQL_Book sqlBook = new SQL_Book(pCS, posRequest.posKeepingKey.idB_Dealer);
//                if (false == sqlBook.LoadTable(new string[] { "IDB", "IDENTIFIER", "DISPLAYNAME" }))
//                    throw new Exception("book Dealer not found");
//                Tools.SetBookId(dataDocument.bookDealer, sqlBook);
//            }

//            //Clearer
//            SQL_Actor sqlActorClearer = new SQL_Actor(pCS, posRequest.posKeepingKey.idA_Clearer);
//            if (false == sqlActorClearer.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
//                throw new Exception("Clearer not found");
//            Tools.SetActorId(dataDocument.actorClearer, sqlActorClearer);

//            SQL_Book sqlBookClearer = new SQL_Book(pCS, posRequest.posKeepingKey.idB_Clearer);
//            if (false == sqlBookClearer.LoadTable(new string[] { "IDB", "IDENTIFIER", "DISPLAYNAME" }))
//                throw new Exception("book Clearer not found");
//            // FI 20130925 [18990]
//            //Tools.SetBookId(dataDocument.bookDealer, sqlBookClearer);
//            Tools.SetBookId(dataDocument.bookClearer, sqlBookClearer);

//            dataDocument.notesSpecified = posRequest.notesSpecified;
//            if (dataDocument.notesSpecified)
//                dataDocument.notes = new EFS_String(posRequest.notes);

//            IFixInstrument fixInstrument = posRequestContainer.NewFixInstrument(pCS, productBase);
//            if (null == fixInstrument)
//                throw new Exception("Asset is not specified");
//            dataDocument.Instrmt = fixInstrument;

//            IPosRequestDetPositionOption detail = posRequestContainer.posRequest.Detail as IPosRequestDetPositionOption;
//            if (null != detail)
//            {
//                dataDocument.isPartialExecutionAllowed.BoolValue = detail.partialExecutionAllowed;
//                dataDocument.isFeeCalculation.BoolValue = detail.feeCalculation;
//                dataDocument.isAbandonRemainingQty.BoolValue = detail.abandonRemainingQty;
//            }
//        }

//        /// <summary>
//        /// 
//        /// </summary>
//        /// <param name="pIsETD"></param>
//        /// <param name="pIsEST"></param>
//        /// <param name="pCategorie"></param>
//        public void GetMainInstrumentInfo(out Boolean pIsETD, out Boolean pIsEST, out Nullable<CfiCodeCategoryEnum> pCategorie)
//        {
//            pCategorie = categorie;
//            pIsETD = sqlProduct.IsExchangeTradedDerivative;
//            pIsEST = sqlProduct.IsEquitySecurityTransaction;
//        }


//        /// <summary>
//        /// Retourne une nouvelle instance de la classe chargée d'alimenter postRequest
//        /// <para>La classe instanciée est fonction du type d'action (Abandon, Exercice, Correction, etc...)</para>
//        /// </summary>
//        /// <returns></returns>
//        // EG 20180307 [23769] Gestion dbTransaction
//        public IPosRequest NewPostRequest(string pCS)
//        {

//            IProductBase productBase = Tools.GetNewProductBase();

//            IPosRequest posRequest = null;

//            int idEntity = dataDocument.actorEntity.OTCmlId;
//            if (idEntity <= 0)
//                throw new Exception("No entity found in dataDocument");

//            // Market
//            SQL_Market sqlMarket = null;
//            string exchange = dataDocument.Instrmt.SecurityExchange;
//            if (StrFunc.IsFilled(exchange))
//            {
//                SQL_Market sqlMarketItem = new SQL_Market(pCS, SQL_TableWithID.IDType.FIXML_SecurityExchange, exchange, SQL_Table.ScanDataDtEnabledEnum.No);
//                if (sqlMarketItem.LoadTable())
//                    sqlMarket = sqlMarketItem;
//            }
//            if ((sqlMarket == null))
//                throw new Exception("No market found in dataDocument");

//            // EntityMarket
//            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(pCS, null, idEntity, sqlMarket.Id, null);
//            // EG 20151102 [21465][21112] Refactoring / Add IDA_CUSTODIAN
//            bool isFoundEntityMarket = sqlEntityMarket.LoadTable(new string[] { "IDEM, IDA_CUSTODIAN, ety.IDA, mk.IDA, DTMARKET" });

//            if ((false == isFoundEntityMarket))
//                throw new Exception("No EntityMarket Found in dataDocument");

//            switch (dataDocument.requestType)
//            {
//                case Cst.PosRequestTypeEnum.OptionExercise:
//                case Cst.PosRequestTypeEnum.OptionAssignment:
//                case Cst.PosRequestTypeEnum.OptionAbandon:
//                    posRequest = (IPosRequest)productBase.CreatePosRequestPositionOption();
//                    IPosRequestPositionOption posRequestOption = (IPosRequestPositionOption)posRequest;

//                    posRequestOption.requestType = dataDocument.requestType;
//                    posRequestOption.requestMode = dataDocument.requestMode;
//                    posRequestOption.dtBusiness = dataDocument.clearingBusinessDate.DateValue;

//                    // EG 20151102 [21465] Refactoring / Contrôle DTBUSINESS
//                    if (posRequestOption.dtBusiness != sqlEntityMarket.dtMarket)
//                    {
//                        EFS_Date dtMarket = new EFS_Date();
//                        dtMarket.DateValue = sqlEntityMarket.dtMarket;
//                        string message = String.Format(Ressource.GetString("denOption_ERRActionCantBePerformedAtBusinessDate") + " [{0}], " +
//                            Ressource.GetString("DTMARKET_") + " [{1}]", dataDocument.clearingBusinessDate.Value, dtMarket.Value);
//                        throw new Exception(message);
//                    }

//                    posRequestOption.qtySpecified = StrFunc.IsFilled(dataDocument.qty.Value);
//                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
//                    // EG 20170127 Qty Long To Decimal
//                    posRequestOption.qty = dataDocument.qty.DecValue;

//                    int idAsset = IntFunc.IntValue2(dataDocument.Instrmt.SecurityId);
//                    posRequestOption.posKeepingKeySpecified = true;
//                    posRequestOption.SetPosKey(sqlInstrument.IdI, Cst.UnderlyingAsset.ExchangeTradedContract, idAsset, 
//                        dataDocument.actorDealer.OTCmlId, dataDocument.bookDealer.OTCmlId, dataDocument.actorClearer.OTCmlId, dataDocument.bookClearer.OTCmlId);

//                    int idClearerEntity = 0;
//                    if (dataDocument.bookClearer.OTCmlId > 0)
//                        idClearerEntity = Book.BookTools.GetEntityBook(pCS, dataDocument.bookClearer.OTCmlId);

//                    posRequestOption.SetAdditionalInfo(dataDocument.actorEntity.OTCmlId, idClearerEntity);

//                    posRequestOption.notesSpecified = dataDocument.notesSpecified;
//                    posRequestOption.notes = dataDocument.notes.Value;

//                    posRequestOption.SetDetail(dataDocument.isPartialExecutionAllowed.BoolValue, dataDocument.isFeeCalculation.BoolValue, dataDocument.isAbandonRemainingQty.BoolValue, dataDocument.paymentFees);

//                    // EG 20151019 [21112] New
//                    posRequestOption.SetIdentifiers(dataDocument.actorEntity.Value, dataDocument.actorClearer.Value, dataDocument.Instrmt.SecurityExchange,
//                        sqlInstrument.Identifier, dataDocument.Instrmt.ExchangeSymbol, dataDocument.actorDealerSpecified ? dataDocument.actorDealer.Value : "N/A",
//                        dataDocument.bookDealerSpecified ? dataDocument.bookDealer.Value : "N/A", 
//                        dataDocument.actorClearer.Value, dataDocument.bookClearer.Value);
//                    break;
//                default:
//                    throw new NotImplementedException("RequestType [{0}] is not implemented");

//            }

//            posRequest.idEMSpecified = true;
//            posRequest.idEM = sqlEntityMarket.idEM;
//            posRequest.idA_Entity = sqlEntityMarket.idA;
//            // EG 20151019 [21112] New
//            posRequest.SetIdA_CssCustodian(sqlEntityMarket.idA_CSS, sqlEntityMarket.idA_CUSTODIAN);
//            //posRequest.idA_Css = sqlEntityMarket.idA_CSS;
//            //posRequest.idA_CustodianSpecified = false;

//            return posRequest;
//        }

//        #endregion Method
//    }
//}