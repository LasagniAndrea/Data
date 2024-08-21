using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Interface;
using FpML.Interface;
using System;

// TODO FI 20130321 cette espace de nom doit sortir de SpheresIO
// Il faudra nécessairement le faire lorsque l'on faudra offrir une saisie de POSREQUEST sur une position 
// PM 20180219 [23824] Déplacé dans CommonIO à partir de SpheresIO (PosRequestPositionInput.cs)
namespace EFS.PosRequestInformation
{
    /// <summary>
    /// Représente une saisie d'une demande POSREQUEST sur position
    /// </summary>
    public class PosRequestPositionInput : ICustomCaptureInfos
    {
        #region Members
        #endregion Members

        #region accessors
        /// <summary>
        /// 
        /// </summary>
        public IPosRequestPositionDocument DataDocument
        {
            get;
            set;
        }
        /// <summary>
        /// 
        /// </summary>
        public PosRequestCustomCaptureInfos CustomCaptureInfos
        {
            set;
            get;
        }

        /// <summary>
        /// Obtient l'instrument du POSREQUEST
        /// </summary>
        public SQL_Instrument SqlInstrument
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient le produit associé à l'instruement du POSREQUEST
        /// </summary>
        public SQL_Product SqlProduct
        {
            get;
            private set;
        }

        /// <summary>
        /// Obtient la nature Option/Future de l'instrument du POSREQUEST lorsque ce dernier est un ETD
        /// </summary>
        public Nullable<CfiCodeCategoryEnum> Categorie
        {
            private set;
            get;
        }
        #endregion accessors

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        public PosRequestPositionInput()
        {
            DataDocument = Tools.GetNewProductBase().CreatePosRequestPositionDocument();
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIsGetDefaultOnInitializeCci"></param>
        /// FI 20181025 [24279] Add Method
        public void InitializeCustomCaptureInfos(string pCS, Boolean pIsGetDefaultOnInitializeCci)
        {
            CustomCaptureInfos = new PosRequestCustomCaptureInfos(pCS, this, null, string.Empty, pIsGetDefaultOnInitializeCci);
            CustomCaptureInfos.InitializeCciContainer();
        }
        


        #region ICustomCaptureInfos Membres
        public CustomCaptureInfosBase CcisBase
        {
            get { return (CustomCaptureInfosBase)CustomCaptureInfos; }
        }
        #endregion

        #region Method

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdPR"></param>
        public IPosRequest LoadDataDocument(string pCS, int pIdPR)
        {
            IProductBase productBase = Tools.GetNewProductBase();
            this.DataDocument = productBase.CreatePosRequestPositionDocument();

            IPosRequest posRequest = PosKeepingTools.GetPosRequest(pCS, null, productBase, pIdPR);
            if (null != posRequest)
            {
                if (false == (posRequest.PosKeepingKeySpecified))
                    throw new Exception(StrFunc.AppendFormat("PosRequest (id:{0}) doesn't contains posKeepingKey", posRequest.IdPR));

                SetInstrument(pCS, SQL_TableWithID.IDType.Id, posRequest.PosKeepingKey.IdI.ToString());

                SetDataDocument(CSTools.SetCacheOn(pCS), posRequest, productBase);
            }
            
            
            
            return posRequest;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pType"></param>
        /// <param name="pIdentifier"></param>
        public void SetInstrument(string pCS, SQL_TableWithID.IDType pType, string pIdentifier)
        {
            if (StrFunc.IsEmpty(pIdentifier))
                throw new ArgumentException("empty value for parameter pIdentifier is not allowed");

            SqlInstrument = new SQL_Instrument(pCS, pType, pIdentifier, SQL_Table.RestrictEnum.No,
                                                        SQL_TableWithID.ScanDataDtEnabledEnum.No, null, string.Empty);
            if (false == SqlInstrument.LoadTable())
                throw new Exception("Instrument not found");

            SqlProduct = new SQL_Product(CSTools.SetCacheOn(pCS), SqlInstrument.IdP);
            if (false == SqlProduct.LoadTable())
                throw new Exception("product not found");

            Categorie = Tools.GetETDInstrumentCategory(SqlInstrument);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="posRequest"></param>
        /// <param name="productBase"></param>
        public void SetDataDocument(string pCS, IPosRequest posRequest, IProductBase productBase)
        {
            PosRequestContainer posRequestContainer = new PosRequestContainer(posRequest);

            DataDocument.RequestType = posRequest.RequestType;
            DataDocument.RequestMode = posRequest.RequestMode;
            DataDocument.ClearingBusinessDate.DateValue = posRequest.DtBusiness;
            // EG 20150920 [21374] Int (int32) to Long (Int64) 
            // EG 20170127 Qty Long To Decimal
            DataDocument.Qty.DecValue = posRequest.Qty;

            //Entity
            if (posRequest.PosKeepingKey.IdA_EntityDealer == 0)
                throw new Exception("PosRequest doesn't constains Entity");
            SQL_Actor sqlActorEntity = new SQL_Actor(pCS, posRequest.PosKeepingKey.IdA_EntityDealer);
            if (false == sqlActorEntity.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                throw new Exception("Entity not found");
            Tools.SetActorId(DataDocument.ActorEntity, sqlActorEntity);

            //Dealer
            DataDocument.ActorDealerSpecified = (posRequest.PosKeepingKey.IdA_Dealer > 0);
            if (DataDocument.ActorDealerSpecified)
            {
                SQL_Actor sqlActor = new SQL_Actor(pCS, posRequest.PosKeepingKey.IdA_Dealer);
                if (false == sqlActor.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                    throw new Exception("Dealer not found");
                Tools.SetActorId(DataDocument.ActorDealer, sqlActor);
            }

            DataDocument.BookDealerSpecified = (posRequest.PosKeepingKey.IdB_Dealer > 0);
            if (DataDocument.BookDealerSpecified)
            {
                SQL_Book sqlBook = new SQL_Book(pCS, posRequest.PosKeepingKey.IdB_Dealer);
                if (false == sqlBook.LoadTable(new string[] { "IDB", "IDENTIFIER", "DISPLAYNAME" }))
                    throw new Exception("book Dealer not found");
                Tools.SetBookId(DataDocument.BookDealer, sqlBook);
            }

            //Clearer
            SQL_Actor sqlActorClearer = new SQL_Actor(pCS, posRequest.PosKeepingKey.IdA_Clearer);
            if (false == sqlActorClearer.LoadTable(new string[] { "IDA", "IDENTIFIER", "DISPLAYNAME" }))
                throw new Exception("Clearer not found");
            Tools.SetActorId(DataDocument.ActorClearer, sqlActorClearer);

            SQL_Book sqlBookClearer = new SQL_Book(pCS, posRequest.PosKeepingKey.IdB_Clearer);
            if (false == sqlBookClearer.LoadTable(new string[] { "IDB", "IDENTIFIER", "DISPLAYNAME" }))
                throw new Exception("book Clearer not found");
            // FI 20130925 [18990]
            //Tools.SetBookId(DataDocument.bookDealer, sqlBookClearer);
            Tools.SetBookId(DataDocument.BookClearer, sqlBookClearer);

            DataDocument.NotesSpecified = posRequest.NotesSpecified;
            if (DataDocument.NotesSpecified)
                DataDocument.Notes = new EFS_String(posRequest.Notes);

            IFixInstrument fixInstrument = posRequestContainer.NewFixInstrument(pCS, productBase);
            DataDocument.Instrmt = fixInstrument ?? throw new Exception("Asset is not specified");

            if (posRequestContainer.PosRequest.DetailBase is IPosRequestDetPositionOption detail)
            {
                DataDocument.IsPartialExecutionAllowed.BoolValue = detail.PartialExecutionAllowed;
                // RD 20200120 [25114] Use detail.feeCalculationSpecified
                if (detail.FeeCalculationSpecified)
                    DataDocument.IsFeeCalculation.BoolValue = detail.FeeCalculation;
                DataDocument.IsAbandonRemainingQty.BoolValue = detail.AbandonRemainingQty;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsETD"></param>
        /// <param name="pIsEST"></param>
        /// <param name="pCategorie"></param>
        public void GetMainInstrumentInfo(out Boolean pIsETD, out Boolean pIsEST, out Nullable<CfiCodeCategoryEnum> pCategorie)
        {
            pCategorie = Categorie;
            pIsETD = SqlProduct.IsExchangeTradedDerivative;
            pIsEST = SqlProduct.IsEquitySecurityTransaction;
        }


        /// <summary>
        /// Retourne une nouvelle instance de la classe chargée d'alimenter postRequest
        /// <para>La classe instanciée est fonction du type d'action (Abandon, Exercice, Correction, etc...)</para>
        /// </summary>
        /// <returns></returns>
        // RD 20210906 [25803] PosRequestTypeEnum : Add NEX (OptionNotExercised) & NAS (OptionNotAssigned)
        public IPosRequest NewPostRequest(string pCS)
        {

            IProductBase productBase = Tools.GetNewProductBase();
            int idEntity = DataDocument.ActorEntity.OTCmlId;
            if (idEntity <= 0)
                throw new Exception("No entity found in dataDocument");

            // Market
            SQL_Market sqlMarket = null;
            string exchange = DataDocument.Instrmt.SecurityExchange;
            if (StrFunc.IsFilled(exchange))
            {
                SQL_Market sqlMarketItem = new SQL_Market(pCS, SQL_TableWithID.IDType.FIXML_SecurityExchange, exchange, SQL_Table.ScanDataDtEnabledEnum.No);
                if (sqlMarketItem.LoadTable())
                    sqlMarket = sqlMarketItem;
            }
            if ((sqlMarket == null))
                throw new Exception("No market found in dataDocument");

            // EntityMarket
            SQL_EntityMarket sqlEntityMarket = new SQL_EntityMarket(pCS, null, idEntity, sqlMarket.Id, null);
            // EG 20151102 [21465][21112] Refactoring / Add IDA_CUSTODIAN
            bool isFoundEntityMarket = sqlEntityMarket.LoadTable(new string[] { "IDEM, IDA_CUSTODIAN, ety.IDA, mk.IDA, DTMARKET" });

            if ((false == isFoundEntityMarket))
                throw new Exception("No EntityMarket Found in dataDocument");


            IPosRequest posRequest;
            switch (DataDocument.RequestType)
            {
                case Cst.PosRequestTypeEnum.OptionExercise:
                case Cst.PosRequestTypeEnum.OptionAssignment:
                case Cst.PosRequestTypeEnum.OptionAbandon:
                case Cst.PosRequestTypeEnum.OptionNotExercised:
                case Cst.PosRequestTypeEnum.OptionNotAssigned:
                    posRequest = (IPosRequest)productBase.CreatePosRequestPositionOption();
                    IPosRequestPositionOption posRequestOption = (IPosRequestPositionOption)posRequest;

                    posRequestOption.RequestType = DataDocument.RequestType;
                    posRequestOption.RequestMode = DataDocument.RequestMode;
                    posRequestOption.DtBusiness = DataDocument.ClearingBusinessDate.DateValue;

                    // EG 20151102 [21465] Refactoring / Contrôle DTBUSINESS
                    if (posRequestOption.DtBusiness != sqlEntityMarket.DtMarket)
                    {
                        EFS_Date dtMarket = new EFS_Date
                        {
                            DateValue = sqlEntityMarket.DtMarket
                        };
                        string message = String.Format(Ressource.GetString("denOption_ERRActionCantBePerformedAtBusinessDate") + " [{0}], " +
                            Ressource.GetString("DTMARKET_") + " [{1}]", DataDocument.ClearingBusinessDate.Value, dtMarket.Value);
                        throw new Exception(message);
                    }

                    posRequestOption.QtySpecified = StrFunc.IsFilled(DataDocument.Qty.Value);
                    // EG 20150920 [21374] Int (int32) to Long (Int64) 
                    // EG 20170127 Qty Long To Decimal
                    posRequestOption.Qty = DataDocument.Qty.DecValue;

                    int idAsset = IntFunc.IntValue2(DataDocument.Instrmt.SecurityId);
                    posRequestOption.PosKeepingKeySpecified = true;
                    posRequestOption.SetPosKey(SqlInstrument.IdI, Cst.UnderlyingAsset.ExchangeTradedContract, idAsset,
                        DataDocument.ActorDealer.OTCmlId, DataDocument.BookDealer.OTCmlId, DataDocument.ActorClearer.OTCmlId, DataDocument.BookClearer.OTCmlId);

                    int idClearerEntity = 0;
                    if (DataDocument.BookClearer.OTCmlId > 0)
                        idClearerEntity = Book.BookTools.GetEntityBook(pCS, DataDocument.BookClearer.OTCmlId);

                    posRequestOption.SetAdditionalInfo(DataDocument.ActorEntity.OTCmlId, idClearerEntity);

                    posRequestOption.NotesSpecified = DataDocument.NotesSpecified;
                    posRequestOption.Notes = DataDocument.Notes.Value;

                    posRequestOption.SetDetail(DataDocument.IsPartialExecutionAllowed.BoolValue, DataDocument.IsFeeCalculation.BoolValue, DataDocument.IsAbandonRemainingQty.BoolValue, DataDocument.PaymentFees);

                    // EG 20151019 [21112] New
                    posRequestOption.SetIdentifiers(DataDocument.ActorEntity.Value, DataDocument.ActorClearer.Value, DataDocument.Instrmt.SecurityExchange,
                        SqlInstrument.Identifier, DataDocument.Instrmt.ExchangeSymbol, DataDocument.ActorDealerSpecified ? DataDocument.ActorDealer.Value : "N/A",
                        DataDocument.BookDealerSpecified ? DataDocument.BookDealer.Value : "N/A",
                        DataDocument.ActorClearer.Value, DataDocument.BookClearer.Value);
                    break;
                default:
                    throw new NotImplementedException($"RequestType:{DataDocument.RequestType} is not implemented");
            }

            posRequest.IdEMSpecified = true;
            posRequest.IdEM = sqlEntityMarket.IdEM;
            posRequest.IdA_Entity = sqlEntityMarket.IdA;
            // EG 20151019 [21112] New
            posRequest.SetIdA_CssCustodian(sqlEntityMarket.IdA_CSS, sqlEntityMarket.IdA_CUSTODIAN);
            //posRequest.idA_Css = sqlEntityMarket.idA_CSS;
            //posRequest.idA_CustodianSpecified = false;

            return posRequest;
        }

        #endregion Method
    }
}