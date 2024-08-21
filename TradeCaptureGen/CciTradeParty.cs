#region Using Directives
using System;
using System.Collections;
using EFS.Actor;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.EFSTools;

using EFS.GUI;
using EFS.GUI.CCI;

using EfsML;
using EfsML.Business;
using EfsML.Confirmation;


using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciTradeParty
    public class CciTradeParty : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            side,
            actor,
            frontId,
            book,
            localClassDerv,
            iasClassDerv,
            hedgeClassDerv,
            hedgeFolder,
            hedgeFactor,
            fxClass,
            localClassNDrv,
            iasClassNDrv,
            hedgeClassNDrv,
            folder,
            unknown,
        }
        #endregion CciEnum
        #region PartyType
        public enum PartyType
        {
            broker,
            party,
        }
        #endregion PartyType
        #endregion Enums
        //
        #region Members
        public int number;
        public string prefix;
        private string prefixParent;
        public PartyType partyType;
        //
        public CciTrader[] cciTrader; //Traders de la party
        public CciTrader[] cciSales;  //Sales de la party (n'existe pas sur un cciParty de type Broker)
        //
        public CciTradeParty[] cciBroker; // Brokers rattaché à la party 
        public CciTradeParty cciBrokerParent; //si le cciTradeParty se charge d'un broker rattaché à une party => cciBrokerParent contient la party Parent
        //
        public TradeCommonCustomCaptureInfos ccis;
        public CciTradeCommonBase cciTrade;
        //
        private bool _isInitTradeSide;
        private bool _isInitFromPartyTemplate;
        private bool _isActorSYSTEMAuthorized;
        #endregion Members
        //
        #region Accessors
        #region IsModeConsult
        public bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(ccis.CaptureMode); }
        }
        #endregion IsModeConsult
        #region IsPartyBroker
        public bool IsPartyBroker
        {
            get { return (PartyType.broker == partyType); }
        }
        #endregion IsPartyBroker
        #region TraderLength
        public int TraderLength
        {
            get { return ArrFunc.IsFilled(cciTrader) ? cciTrader.Length : 0; }
        }
        #endregion TraderLength
        #region SalesLength
        public int SalesLength
        {
            get { return ArrFunc.IsFilled(cciSales) ? cciSales.Length : 0; }
        }
        #endregion SalesLength
        #region BrokerLength
        public int BrokerLength
        {
            get { return ArrFunc.IsFilled(cciBroker) ? cciBroker.Length : 0; }
        }
        #endregion
        #region isBrokerOfParty
        public bool isBrokerOfParty
        {
            get { return (null != cciBrokerParent); }
        }
        #endregion

        #region isInitFromPartyTemplate
        public bool isInitFromPartyTemplate
        {
            get
            {
                return _isInitFromPartyTemplate; 
            }
            set
            {
                _isInitFromPartyTemplate = value;
            }
        }
        #endregion
        #region isInitTradeSide
        public bool isInitTradeSide
        {
            get
            {
                return _isInitTradeSide;
            }
            set
            {
                _isInitTradeSide = value;
            }
        }
        #endregion
        #region isActorSYSTEMAuthorized
        /// <summary>
        // Acteur "SYSTEM" autorisé 
        // Exemple la contrepartie d'un trade de type DebtSecurity peut-être l'acteur SYSTEM
        /// </summary>
        public bool isActorSYSTEMAuthorized
        {
            get
            {
                return _isActorSYSTEMAuthorized;
            }
            set
            {
                _isActorSYSTEMAuthorized = value;
            }
        }
        #endregion
        #endregion Accessors
        //
        #region Constructors
        public CciTradeParty(CciTradeCommonBase pTrade, int plNumber, PartyType pType, string pPrefixParent)
        {
            cciTrade = pTrade;
            ccis = pTrade.Ccis;
            partyType = pType;
            number = plNumber;
            prefixParent = pPrefixParent;
            prefix = pPrefixParent + pType.ToString() + number.ToString() + CustomObject.KEY_SEPARATOR;
            //
            _isInitTradeSide = true;
            _isInitFromPartyTemplate = true;
            _isActorSYSTEMAuthorized = false;
        }
        #endregion Constructors
        //
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
            if (false == ccis.Contains(CciClientId(CciEnum.side)) && (PartyType.party == partyType))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.side), true, TypeData.TypeDataEnum.@string));
        }
        #endregion AddCciSystem
        #region CleanUP
        public void CleanUp()
        {
            CleanUp(false);
        }
        public void CleanUp(bool pRemoveAll)
        {
            try
            {

                if (ArrFunc.IsFilled(cciTrader))
                {
                    for (int i = 0; i < cciTrader.Length; i++)
                        cciTrader[i].CleanUp();
                }
                //
                if (ArrFunc.IsFilled(cciSales))
                {
                    for (int i = 0; i < cciSales.Length; i++)
                        cciSales[i].CleanUp();
                }
                //
                if (ArrFunc.IsFilled(cciBroker))
                {
                    for (int i = 0; i < cciBroker.Length; i++)
                        cciBroker[i].CleanUp();
                }
                //
                IPartyTradeIdentifier partyTradeIdentifier = GetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (false == pRemoveAll)
                    {
                        if (ArrFunc.IsFilled(partyTradeIdentifier.tradeId))
                        {
                            for (int i = partyTradeIdentifier.tradeId.Length - 1; -1 < i; i--)
                            {
                                bool isRemove = false;
                                isRemove = StrFunc.IsEmpty(partyTradeIdentifier.tradeId[i].scheme) || StrFunc.IsEmpty(partyTradeIdentifier.tradeId[i].Value);
                                if (isRemove)
                                    ReflectionTools.RemoveItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), i);
                            }
                        }
                    }
                    partyTradeIdentifier.tradeIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.tradeId) && (false == pRemoveAll);
                    //
                    if (ArrFunc.IsFilled(partyTradeIdentifier.linkId))
                    {
                        if (false == pRemoveAll)
                        {
                            for (int i = partyTradeIdentifier.linkId.Length - 1; -1 < i; i--)
                            {
                                bool isRemove = false;
                                isRemove = StrFunc.IsEmpty(partyTradeIdentifier.linkId[i].linkIdScheme) || StrFunc.IsEmpty(partyTradeIdentifier.linkId[i].Value);
                                if (isRemove)
                                    ReflectionTools.RemoveItemInArray(partyTradeIdentifier, "linkId", i);
                            }
                        }
                    }
                    partyTradeIdentifier.linkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.linkId) && (false == pRemoveAll);
                    //
                }
                //
                IPartyTradeInformation partyTradeInformation = GetPartyTradeInformation();
                if (null != partyTradeInformation)
                {
                    if (ArrFunc.IsFilled(partyTradeInformation.trader))
                    {
                        for (int i = partyTradeInformation.trader.Length - 1; -1 < i; i--)
                        {
                            bool isToRemove = false;
                            isToRemove = StrFunc.IsEmpty(partyTradeInformation.trader[i].Value) || pRemoveAll;
                            if (isToRemove)
                                ReflectionTools.RemoveItemInArray(partyTradeInformation, "trader", i);
                        }
                    }
                    if (ArrFunc.IsFilled(partyTradeInformation.sales))
                    {
                        for (int i = partyTradeInformation.sales.Length - 1; -1 < i; i--)
                        {
                            bool isToRemove = false;
                            isToRemove = StrFunc.IsEmpty(partyTradeInformation.sales[i].Value) || pRemoveAll;
                            if (isToRemove)
                                ReflectionTools.RemoveItemInArray(partyTradeInformation, "sales", i);
                        }
                    }
                    if (ArrFunc.IsFilled(partyTradeInformation.brokerPartyReference))
                    {
                        for (int i = partyTradeInformation.brokerPartyReference.Length - 1; -1 < i; i--)
                        {
                            bool isToRemove = false;
                            isToRemove = StrFunc.IsEmpty(partyTradeInformation.brokerPartyReference[i].hRef) || pRemoveAll;
                            if (isToRemove)
                                ReflectionTools.RemoveItemInArray(partyTradeInformation, "brokerPartyReference", i);
                        }
                    }
                    //
                    if (ArrFunc.IsEmpty(partyTradeInformation.trader) && ArrFunc.IsEmpty(partyTradeInformation.sales)  && ArrFunc.IsEmpty(partyTradeInformation.brokerPartyReference)  )
                    {
                        int posInArray = cciTrade.DataDocument.GetPartyTradeInformationPosInArray(GetPartyId());
                        //
                        if (posInArray > -1)
                            ReflectionTools.RemoveItemInArray(cciTrade.DataDocument.tradeHeader, "partyTradeInformation", posInArray);
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.CleanUp", ex); }
        }
        #endregion CleanUP
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                bool isFilled;
                string data;
                string partyReferenceHref;
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = ccis[prefix + enumName];
                    if ((cci != null) && (cci.HasChanged))
                    {
                        #region Reset variables
                        partyReferenceHref = string.Empty;
                        data = cci.newValue;
                        isSetting = true;
                        isFilled = StrFunc.IsFilled(data);
                        processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        #endregion Reset variables
                        //
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.side:
                                #region side
                                //Notion inexistante en FpML
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion side
                                break;
                            case CciEnum.actor:
                                #region Partyactor/Brokeractor
                                DumpParty_ToDocument(cci, data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion Partyactor/Brokeractor
                                break;
                            case CciEnum.book:
                                #region Book
                                DumpBook_ToDocument(cci, data);
                                //
                                if (_isInitFromPartyTemplate)
                                {
                                    //20081124 FI Initialiation from PARTYTEMPLATE 
                                    bool isOk = CaptureTools.DumpTrader_ToDocument_FromBook(cciTrade.CS, cciTrade.DataDocument, GetPartyId());
                                    if (isOk)
                                    {
                                        ccis.IsToSynchronizeWithDocument = true;
                                        CaptureTools.DumpSales_ToDocument_FromFirstTrader(cciTrade.CS, cciTrade.DataDocument, GetPartyId());
                                    }
                                }
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion Book
                                break;
                            case CciEnum.localClassDerv:
                                #region PartyLocalClassDerv
                                DumpLocalClassDerv_ToDocument(data);
                                #endregion PartyLocalClassDerv
                                break;
                            case CciEnum.localClassNDrv:
                                #region PartyLocalClassNDrv
                                DumpLocalClassNDrv_ToDocument(data);
                                #endregion PartyLocalClassNDrv
                                break;
                            case CciEnum.iasClassDerv:
                                #region PartyIASClassDerv
                                DumpIASClassDerv_ToDocument(data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion PartyIASClassDerv
                                break;
                            case CciEnum.iasClassNDrv:
                                #region PartyIASClassNDrv
                                DumpIASClassNDrv_ToDocument(data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion PartyIASClassNDrv
                                break;
                            case CciEnum.fxClass:
                                #region PartyFxClass
                                DumpFxClass_ToDocument(data);
                                #endregion PartyFxClass
                                break;
                            case CciEnum.hedgeClassDerv:
                                #region PartyHedgeClassDerv
                                DumpHedgeClassDerv_ToDocument(data);
                                #endregion PartyHedgeClassDerv
                                break;
                            case CciEnum.hedgeClassNDrv:
                                #region PartyHedgeClassNDrv
                                DumpHedgeClassNDrv_ToDocument(data);
                                #endregion PartyHedgeClassNDrv
                                break;
                            case CciEnum.hedgeFolder:
                                #region PartyHedgeFolder
                                DumpHedgeFolder_ToDocument(cci, data);
                                #endregion PartyHedgeFolder
                                break;
                            case CciEnum.hedgeFactor:
                                #region PartyHedgeFactor
                                DumpHedgeFactor_ToDocument(cci, data);
                                #endregion PartyHedgeFactor
                                break;
                            case CciEnum.frontId:
                                #region PartyTradeId/PartyBrokerTradeId
                                DumpFrontId_ToDocument(cci, data);
                                #endregion PartyTradeId/PartyBrokerTradeId
                                break;
                            case CciEnum.folder:
                                #region PartyFolder/PartyBrokerFolder
                                DumpFolderId_ToDocument(cci, data);
                                #endregion PartyFolder/PartyBrokerFolder
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                            ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
                //
                for (int i = 0; i < TraderLength; i++)
                    cciTrader[i].Dump_ToDocument();
                for (int i = 0; i < SalesLength; i++)
                    cciSales[i].Dump_ToDocument();
                //
                for (int i = 0; i < BrokerLength; i++)
                    cciBroker[i].Dump_ToDocument();
                //
                IPartyTradeInformation partyTradeInfo = GetPartyTradeInformation();
                if (null != partyTradeInfo)
                {
                    partyTradeInfo.traderSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.traderSpecified, cciTrader);
                    partyTradeInfo.salesSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.salesSpecified, cciSales);
                    partyTradeInfo.brokerPartyReferenceSpecified = CciTools.Dump_IsCciContainerArraySpecified(partyTradeInfo.brokerPartyReferenceSpecified, cciBroker);
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.Dump_ToDocument", ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            try
            {
                InitializeTrader_FromCci();
                InitializeSales_FromCci();
                InitializeBroker_FromCci();
                //
                if (ArrFunc.IsFilled(cciBroker))
                {
                    for (int i = 0; i < cciBroker.Length; i++)
                        cciBroker[i].Initialize_FromCci();
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("TradeParty.Initialize_FromCci", ex); }
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            try
            {
                string data;
                string display;
                bool isToValidate;
                bool isSetting;
                SQL_Table sql_Table;
                IParty party;
                IPartyTradeIdentifier partyTradeIdentifier;
                string partyId;

                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = ccis[prefix + enumName];
                    if (cci != null)
                    {
                        #region Reset variables
                        sql_Table = null;
                        party = null;
                        partyTradeIdentifier = GetPartyTradeIdentifier();
                        partyId = string.Empty;
                        data = string.Empty;
                        isSetting = true;
                        isToValidate = false;
                        #endregion Reset variables
                        //
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.side:
                                #region Side
                                //Notion inexistante en FpML
                                #endregion Side
                                break;
                            case CciEnum.actor:
                                #region actor
                                if (PartyType.broker == partyType)
                                {
                                    #region Broker
                                    if (isBrokerOfParty)
                                    {
                                        partyId = String.Empty;
                                        IPartyTradeInformation partyTradeInfo = cciBrokerParent.GetPartyTradeInformation();
                                        if (null != partyTradeInfo)
                                            partyId = partyTradeInfo.brokerPartyReference[number - 1].hRef;
                                    }
                                    else
                                    {
                                        if (cciTrade.CurrentTrade.brokerPartyReferenceSpecified)
                                            partyId = cciTrade.CurrentTrade.brokerPartyReference[number - 1].hRef;
                                    }
                                    if (StrFunc.IsFilled(partyId))
                                    {
                                        party = cciTrade.DataDocument.GetParty(partyId);
                                        if (null != party)
                                        {
                                            SQL_Actor sql_Actor = new SQL_Actor(cciTrade.CS, party.partyId);
                                            sql_Actor.AddRoleRange(new RoleActor[] { GetRole()  });
                                            if (sql_Actor.IsLoaded)
                                            {
                                                data = sql_Actor.Identifier;
                                                sql_Table = (SQL_Table)sql_Actor;
                                            }
                                            else
                                                data = party.partyId;
                                        }
                                    }
                                    #endregion Broker
                                }
                                else
                                {
                                    // Affichage des parties ds l'ordre définie ds currentFpMLDataDocReader.party[]
                                    // Test sur lastValue Pour gérer le cas où Ds le DOC il y a EntityOfUSer (Ds ce cas newValue = "XXX" et lastValue =EntityOfUSer 
                                    #region Party
                                    // Recherche des Ccis qui contiennent le payer et le Receiver
                                    CustomCaptureInfo cciPartyPay = ccis[cciTrade.CciClientIdPayer];
                                    CustomCaptureInfo cciPartyRec = ccis[cciTrade.CciClientIdReceiver];
                                    string lastValue = string.Empty;
                                    //
                                    IParty tradeparty = null;
                                    int num = 0;
                                    for (int i = 0; i < ArrFunc.Count(cciTrade.DataDocument.party); i++)
                                    {
                                        if (
                                            (cciTrade.DataDocument.party[i].id == cciPartyPay.lastValue) ||
                                            (cciTrade.DataDocument.party[i].id == cciPartyRec.lastValue)
                                            )
                                        {
                                            num++;
                                            if (num == number)
                                            {
                                                tradeparty = cciTrade.DataDocument.party[i];
                                                if (null != tradeparty)
                                                    break;
                                            }
                                        }
                                    }
                                    //Chargement du cii à partir des infos contenus ds currentFpMLDataDocReader.party[i]
                                    if (null != tradeparty)
                                    {
#warning 20060613 En cas d'integration d'un trade externe (sans otcmlId) il faut que partyId soit alimenté avec un ACTOR.IDENTIFIER (Prevoir L'utilisation de la fonction ActorTools.GetIdA)
                                        SQL_Actor sql_Actor = null;
                                        if (StrFunc.IsFilled(tradeparty.otcmlId) && tradeparty.OTCmlId > 0)
                                            sql_Actor = new SQL_Actor(cciTrade.CS, tradeparty.OTCmlId);
                                        else if (StrFunc.IsFilled( tradeparty.partyId))  
                                            sql_Actor = new SQL_Actor(cciTrade.CS, tradeparty.partyId);
                                        //
                                        if (null!=sql_Actor)
                                            sql_Actor.AddRoleRange(new RoleActor[] { GetRole()  });
                                        //
                                        if ((null!=sql_Actor) && sql_Actor.IsLoaded)
                                        {
                                            //Affectation .xmlId
                                            //Ne pas toucher => Permet d'afficher les trades même si le xmlId d'un acteur a changé (Si son code bic ou identifier a changé)
                                            sql_Actor.xmlId = tradeparty.id;
                                            //
                                            data = sql_Actor.Identifier;
                                            sql_Table = (SQL_Table)sql_Actor;
                                            isToValidate = (tradeparty.OTCmlId == 0);
                                        }
                                        else
                                        {
                                            isToValidate = (tradeparty.id == TradeCommonCustomCaptureInfos.PartyUnknown);
                                            data = tradeparty.id;
                                        }
                                    }
                                    #endregion Party
                                }
                                #endregion actor
                                break;
                            case CciEnum.book:
                                #region book
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.bookIdSpecified))
                                {
                                    SQL_Book sql_Book = null;
                                    int idA = 0;
                                    if (null != Cci(CciEnum.actor).sql_Table)
                                        idA = ((SQL_Actor)Cci(CciEnum.actor).sql_Table).Id;
                                    //
                                    if (StrFunc.IsFilled(partyTradeIdentifier.bookId.otcmlId) && partyTradeIdentifier.bookId.OTCmlId > 0)
                                        sql_Book = new SQL_Book(cciTrade.CS, partyTradeIdentifier.bookId.OTCmlId);
                                    else
                                    {
                                        if (idA > 0)
                                            sql_Book = new SQL_Book(cciTrade.CS, SQL_TableWithID.IDType.Identifier, partyTradeIdentifier.bookId.Value);
                                        else
                                            sql_Book = new SQL_Book(cciTrade.CS, SQL_TableWithID.IDType.Identifier, partyTradeIdentifier.bookId.Value, SQL_Table.ScanDataDtEnabledEnum.No, idA);
                                    }
                                    //new SQL_Book(cciTrade.cs, SQL_TableWithID.IDType.Id, partyTradeIdentifier.bookId.otcmlId);
                                    if (sql_Book.IsLoaded)
                                    {
                                        data = sql_Book.Identifier;
                                        sql_Table = (SQL_Table)sql_Book;
                                        isToValidate = (partyTradeIdentifier.bookId.OTCmlId == 0);
                                    }
                                    else
                                        data = party.partyId;
                                }
                                #endregion book
                                break;
                            case CciEnum.localClassDerv:
                                #region LocalClassDerv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.localClassDervSpecified))
                                    data = partyTradeIdentifier.localClassDerv.Value;
                                #endregion LocalClassDerv
                                break;
                            case CciEnum.localClassNDrv:
                                #region LocalClassNDrv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.localClassNDrvSpecified))
                                    data = partyTradeIdentifier.localClassNDrv.Value;
                                #endregion LocalClassNDrv
                                break;
                            case CciEnum.iasClassDerv:
                                #region IASClassDerv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.iasClassDervSpecified))
                                    data = partyTradeIdentifier.iasClassDerv.Value;
                                #endregion IASClassDerv
                                break;
                            case CciEnum.iasClassNDrv:
                                #region IASClassNDrv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.iasClassNDrvSpecified))
                                    data = partyTradeIdentifier.iasClassNDrv.Value;
                                #endregion IASClassNDrv
                                break;
                            case CciEnum.fxClass:
                                #region FxClass
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.fxClassSpecified))
                                    data = partyTradeIdentifier.fxClass.Value;
                                #endregion FxClass
                                break;
                            case CciEnum.hedgeClassDerv:
                                #region HedgeClassDerv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.hedgeClassDervSpecified))
                                    data = partyTradeIdentifier.hedgeClassDerv.Value;
                                #endregion HedgeClassDerv
                                break;
                            case CciEnum.hedgeClassNDrv:
                                #region HedgeClassNDrv
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.hedgeClassNDrvSpecified))
                                    data = partyTradeIdentifier.hedgeClassNDrv.Value;
                                #endregion HedgeClassNDrv
                                break;
                            case CciEnum.hedgeFolder:
                                #region HedgeFolder
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.linkIdSpecified))
                                {
                                    foreach (ILinkId linkId in partyTradeIdentifier.linkId)
                                    {
                                        if (Cst.OTCmL_hedgingFolderid == linkId.linkIdScheme)
                                        {
                                            data = linkId.Value;
                                            break;
                                        }
                                    }
                                }
                                #endregion HedgeFolder
                                break;
                            case CciEnum.hedgeFactor:
                                #region HedgeFactor
                                if ((null != partyTradeIdentifier) && (partyTradeIdentifier.linkIdSpecified))
                                {
                                    foreach (ILinkId linkId in partyTradeIdentifier.linkId)
                                    {
                                        if (Cst.OTCmL_hedgingFolderid == linkId.linkIdScheme)
                                        {
                                            if (StrFunc.IsFilled(linkId.factor))
                                                data = linkId.factor;
                                            break;
                                        }
                                    }
                                }
                                #endregion HedgeFactor
                                break;
                            case CciEnum.frontId:
                                #region TradeId
                                if ((null != partyTradeIdentifier) && (null != partyTradeIdentifier.tradeId))
                                {
                                    foreach (ISchemeId tId in partyTradeIdentifier.tradeId)
                                    {
                                        if (Cst.OTCml_FrontTradeIdScheme == tId.scheme)
                                        {
                                            data = tId.Value;
                                            break;
                                        }
                                    }
                                }
                                #endregion TradeId
                                break;
                            case CciEnum.folder:
                                #region PartyFolder
                                if (null != partyTradeIdentifier && partyTradeIdentifier.linkIdSpecified)
                                {
                                    //20080430 PL Correction de BUG avec ISchemeId ??? (a vour avec EG)
                                    foreach (ILinkId linkId in partyTradeIdentifier.linkId)
                                    {
                                        if (Cst.OTCml_FolderIdScheme == linkId.linkIdScheme)
                                        {
                                            data = linkId.Value;
                                            break;
                                        }
                                    }
                                }
                                #endregion PartyFolder
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                        {
                            ccis.InitializeCci(cci, sql_Table, data);
                            if (isToValidate)
                                cci.lastValue = ".";
                        }
                    }
                }
                ////20080514 RD Ticket 16108
                //// L'appel est fait ici à cause de sql_actor qui n'est pas encore connu dans this.InitializeTrader_FromCci()
                InitializeTrader_FromCci();
                InitializeSales_FromCci();
                InitializeBroker_FromCci();
                //
                for (int i = 0; i < TraderLength; i++)
                {
                    if (null != cciTrader[i].trader)
                        cciTrader[i].Initialize_FromDocument();
                }
                //
                for (int i = 0; i < SalesLength; i++)
                {
                    if (null != cciSales[i].trader)
                        cciSales[i].Initialize_FromDocument();
                }
                //
                for (int i = 0; i < BrokerLength; i++)
                {
                    cciBroker[i].Initialize_FromDocument();
                }
                //
                if (partyType == PartyType.party)
                    SetIsMandatoryOnClass();
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.Initialize_FromDocument", ex); }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
            {
                if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string localClassDerv, iasClassDerv, hedgeClassDerv, hedgeFolder, hedgeFactor;
                    string localClassNDrv, iasClassNDrv, hedgeClassNDrv;
                    string fxClass;
                    SQL_Book sql_Book;
                    bool isMandatory;
                    string cliendid_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendid_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendid_Key);
                    //
                    switch (key)
                    {
                        case CciEnum.side:
                            #region side
                            cciTrade.SynchronizePayerReceiverFromSide();
                            #endregion side
                            break;
                        case CciEnum.actor:
                            #region actor
                            SQL_Actor sql_Actor = (SQL_Actor)pCci.sql_Table;
                            SQL_Actor sql_ActorLast = (SQL_Actor)pCci.lastSql_Table;
                            string newValue = pCci.newValue;
                            string lastValue = pCci.lastValue;
                            bool isActorValid = pCci.IsFilledValue && (null != sql_Actor);
                            //
                            if (null != sql_Actor)
                                newValue = sql_Actor.xmlId;
                            //
                            if (null != sql_ActorLast)
                                lastValue = sql_ActorLast.xmlId;
                            //	
                            if (partyType == PartyType.party)
                                cciTrade.SynchronizePayerReceiver(lastValue, newValue);
                            else
                                cciTrade.SynchronizePayerReceiverOtherPartyPayment(lastValue, newValue);
                            //
                            cciTrade.SetClientIdDefaultReceiverOtherPartyPaymentReceiver();
                            cciTrade.SynchronizeParty(pCci);
                            //
                            #region Book, Class, Folder, TradeID
                            CustomCaptureInfo bookCci = ccis[CciClientId(CciEnum.book)];
                            CustomCaptureInfo localClassDervCci = ccis[CciClientId(CciEnum.localClassDerv)];
                            CustomCaptureInfo localClassNDrvCci = ccis[CciClientId(CciEnum.localClassNDrv)];
                            CustomCaptureInfo iasClassDervCci = ccis[CciClientId(CciEnum.iasClassDerv)];
                            CustomCaptureInfo iasClassNDrvCci = ccis[CciClientId(CciEnum.iasClassNDrv)];
                            CustomCaptureInfo fxClassCci = ccis[CciClientId(CciEnum.fxClass)];
                            CustomCaptureInfo hedgeClassDervCci = ccis[CciClientId(CciEnum.hedgeClassDerv)];
                            CustomCaptureInfo hedgeClassNDrvCci = ccis[CciClientId(CciEnum.hedgeClassNDrv)];
                            CustomCaptureInfo hedgeFolderCci = ccis[CciClientId(CciEnum.hedgeFolder)];
                            CustomCaptureInfo hedgeFactorCci = ccis[CciClientId(CciEnum.hedgeFactor)];
                            CustomCaptureInfo folderCci = ccis[CciClientId(CciEnum.folder)];
                            CustomCaptureInfo frontIdCci = ccis[CciClientId(CciEnum.frontId)];
                            //
                            if (isActorValid & (PartyType.party == partyType))
                            {
                                bool isBookFilled = (null != bookCci) && (null != bookCci.sql_Table); // Il existe dejà un book
                                bool isFindDefaultBook = (null != bookCci) && (false == isBookFilled);
                                //
                                if (isBookFilled)
                                {
                                    int IdBook = ((SQL_Book)bookCci.sql_Table).Id;
                                    SQL_Book sqlbook = new SQL_Book(cciTrade.CS, SQL_TableWithID.IDType.Id, IdBook.ToString(), SQL_Table.ScanDataDtEnabledEnum.Yes, sql_Actor.Id);
                                    isFindDefaultBook = (false == sqlbook.IsFound);
                                }
                                //
                                if (isFindDefaultBook)
                                {
                                    bool isOk = false;
                                    bookCci.Reset();
                                    ////20081124 FI Recherche du book en fonction du parametrage PartyTemplates
                                    if (_isInitFromPartyTemplate)
                                    {
                                        PartyTemplates partyTemplates = new PartyTemplates();
                                        partyTemplates.Load(cciTrade.CS, cciTrade.DataDocument.idI, ((SQL_Actor)Cci(CciEnum.actor).sql_Table).Id, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                        if (ArrFunc.Count(partyTemplates.partyTemplate) == 1)
                                        {
                                            if (partyTemplates.partyTemplate[0].idBSpecified)
                                            {
                                                isOk = true;
                                                bookCci.newValue = partyTemplates.partyTemplate[0].bookIdentifier;
                                            }
                                            else
                                            {
                                                // Les specs suivantes ont été abandonnées
                                                // s'il existe 1 seule ligne pour l'acteur et l'instrument et quelle ne comporte pas de Book
                                                // On lit la table BOOK pour l'acteur et l'entité trouvée et s'il n'existe qu'1 seul book on le pré-propose
                                            }
                                        }
                                    }
                                    if (false == isOk)
                                    {
                                        SQL_Book sqlbook = new SQL_Book(cciTrade.CS, SQL_TableWithID.IDType.Identifier, "%", SQL_Table.ScanDataDtEnabledEnum.Yes, sql_Actor.Id);
                                        if ((sqlbook.IsLoaded) && (sqlbook.Dt.Rows.Count == 1))
                                            bookCci.newValue = sqlbook.Identifier;
                                    }
                                }
                                //
                                //20081124 FI Pour rentrer dans DumpBook afin de mettre à jour partyTradeIdentifier
                                //Pour initialiser trader, Sales depuis PartyTemplate
                                if (_isInitFromPartyTemplate)
                                {
                                    if ((isBookFilled) && (bookCci.newValue == bookCci.lastValue))
                                        bookCci.lastValue = ".";
                                }
                            }
                            //
                            if (null != localClassDervCci)
                                localClassDervCci.Reset();
                            if (null != localClassNDrvCci)
                                localClassNDrvCci.Reset();
                            if (null != iasClassDervCci)
                                iasClassDervCci.Reset();
                            if (null != iasClassNDrvCci)
                                iasClassNDrvCci.Reset();
                            if (null != fxClassCci)
                                fxClassCci.Reset();
                            if (null != hedgeClassDervCci)
                                hedgeClassDervCci.Reset();
                            if (null != hedgeClassNDrvCci)
                                hedgeClassNDrvCci.Reset();
                            if (null != hedgeFolderCci)
                                hedgeFolderCci.Reset();
                            if (null != hedgeFactorCci)
                                hedgeFactorCci.Reset();
                            if (null != folderCci)
                                folderCci.Reset();
                            if (null != frontIdCci)
                                frontIdCci.Reset();
                            //
                            SetIsMandatoryOnClass();
                            //
                            for (int i = 0; i < TraderLength; i++)
                                cciTrader[i].Clear();
                            for (int i = 0; i < SalesLength; i++)
                                cciSales[i].Clear();
                            for (int i = 0; i < BrokerLength; i++)
                                cciBroker[i].Cci(CciEnum.actor).newValue = string.Empty;     
                            #endregion Book, Class, Folder, TradeID
                            #endregion actor
                            break;
                        case CciEnum.book:
                            #region Book
                            #region  Préproposition de Class
                            localClassDerv = iasClassDerv = hedgeClassDerv = hedgeFolder = hedgeFactor = localClassNDrv = iasClassNDrv = hedgeClassNDrv = fxClass = string.Empty;
                            sql_Book = (SQL_Book)pCci.sql_Table;
                            bool isIFRS = ccis.SQLInstrument.IsIFRS;
                            //
                            if (null != sql_Book)
                            {
                                if (ccis.SQLProduct.IsDerivative)
                                {
                                    //Derivative
                                    localClassDerv = sql_Book.LocalClassDerv;
                                    if (isIFRS)
                                    {
                                        iasClassDerv = sql_Book.IASClassDerv;
                                        hedgeClassDerv = sql_Book.HedgeClassDerv;
                                    }
                                }
                                else if (ccis.SQLProduct.IsFxAndNotOption)
                                {
                                    //Fx
                                    fxClass = sql_Book.FxClass;
                                }
                                else if (false == ccis.SQLProduct.IsBulletPayment)
                                {
                                    //No Derivative
                                    localClassNDrv = sql_Book.LocalClassNDrv;
                                    if (isIFRS)
                                    {
                                        iasClassNDrv = sql_Book.IASClassNDrv;
                                        hedgeClassNDrv = sql_Book.HedgeClassNDrv;
                                    }
                                }
                                //
                                //hedgeFolder = sql_Book.HedgeFolder;
                                //hedgeFactor = sql_Book.HedgeFactor;
                            }
                            //
                            SetIsMandatoryOnClass();
                            //
                            ccis.SetNewValue(CciClientId(CciEnum.localClassDerv), localClassDerv);
                            ccis.SetNewValue(CciClientId(CciEnum.iasClassDerv), iasClassDerv);
                            ccis.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), hedgeClassDerv);
                            ccis.SetNewValue(CciClientId(CciEnum.hedgeFolder), hedgeFolder);
                            ccis.SetNewValue(CciClientId(CciEnum.hedgeFactor), hedgeFactor);
                            ccis.SetNewValue(CciClientId(CciEnum.localClassNDrv), localClassNDrv);
                            ccis.SetNewValue(CciClientId(CciEnum.iasClassNDrv), iasClassNDrv);
                            ccis.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), hedgeClassNDrv);
                            ccis.SetNewValue(CciClientId(CciEnum.fxClass), fxClass);
                            #endregion  Préproposition de Class
                            #endregion Book


                            break;
                        case CciEnum.iasClassDerv:
                            #region IASClassDerv
                            isMandatory = (pCci.newValue == "HEDGING");
                            Cci(CciEnum.hedgeClassDerv).isMandatory = isMandatory;
                            if (isMandatory)
                            {
                                //Préproposition de HedgeClassDerv depuis BOOK
                                hedgeClassDerv = string.Empty;
                                sql_Book = (SQL_Book)Cci(CciEnum.book).sql_Table;
                                if (null != sql_Book)
                                {
                                    if (ccis.SQLInstrument.IsIFRS)
                                        hedgeClassDerv = sql_Book.HedgeClassDerv;
                                }
                                ccis.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), hedgeClassDerv);
                            }
                            else
                                ccis.SetNewValue(CciClientId(CciEnum.hedgeClassDerv), string.Empty);
                            #endregion IASClassDerv
                            break;
                        case CciEnum.iasClassNDrv:
                            #region IASClassNDrv
                            isMandatory = (pCci.newValue == "HEDGING");
                            Cci(CciEnum.hedgeClassNDrv).isMandatory = isMandatory;
                            if (isMandatory)
                            {
                                //Préproposition de HedgeClassNDrv depuis BOOK
                                hedgeClassNDrv = string.Empty;
                                sql_Book = (SQL_Book)Cci(CciEnum.book).sql_Table;
                                if (null != sql_Book)
                                {
                                    if (ccis.SQLInstrument.IsIFRS)
                                        hedgeClassNDrv = sql_Book.HedgeClassNDrv;
                                }
                                ccis.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), hedgeClassNDrv);
                            }
                            else
                                ccis.SetNewValue(CciClientId(CciEnum.hedgeClassNDrv), string.Empty);
                            #endregion IASClassNDrv
                            break;
                    }
                }
                //
                for (int i = 0; i < TraderLength; i++)
                    cciTrader[i].ProcessInitialize(pCci);
                //
                for (int i = 0; i < SalesLength; i++)
                    cciSales[i].ProcessInitialize(pCci);
                //
                for (int i = 0; i < BrokerLength; i++)
                    cciBroker[i].ProcessInitialize(pCci);
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.ProcessInitialize", ex); }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            try
            {
                bool isEnabled = this.IsSpecified;
                bool isFrontEnabled = isEnabled;
                bool isFolderEnabled = isEnabled;
                //
                if (isBrokerOfParty)
                {
                    int index = cciBrokerParent.number - 1;
                    int indexVs = (index == 1) ?

                    if (cciTrade.cciParty[indexVs].BrokerLength > 0)
                    {
                        for (int i = 0; i < cciTrade.cciParty[indexVs].BrokerLength; i++)
                        {
                            if (cciTrade.cciParty[indexVs].cciBroker[i].GetPartyId() == GetPartyId())
                            {
                                //Front
                                if (isFrontEnabled)
                                {
                                    if (null != cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.frontId))
                                        isFrontEnabled = cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.frontId).IsEmptyValue;
                                }
                                //Folder
                                if (isFolderEnabled)
                                {
                                    if (null != cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.folder))
                                        isFolderEnabled = cciTrade.cciParty[indexVs].cciBroker[i].Cci(CciEnum.folder).IsEmptyValue;
                                }
                                //Trader A Faire plus Tard
                            }
                        }
                    }
                }
                //
                ccis.Set(CciClientId(CciEnum.side), "isEnabled", isEnabled);
                ccis.Set(CciClientId(CciEnum.frontId), "isEnabled", isFrontEnabled);
                ccis.Set(CciClientId(CciEnum.folder), "isEnabled", isFolderEnabled);
                //
                for (int i = 0; i < TraderLength; i++)
                    cciTrader[i].RefreshCciEnabled();
                //
                for (int i = 0; i < SalesLength; i++)
                    cciSales[i].RefreshCciEnabled();
                //
                for (int i = 0; i < BrokerLength; i++)
                    cciBroker[i].RefreshCciEnabled();
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.RefreshCciEnabled", ex); }
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string pPrefix)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            try
            {
                if (IsCci(CciEnum.actor, pCci) && (null != pCci.sql_Table))
                    pCci.display = pCci.sql_Table.FirstRow["DISPLAYNAME"].ToString();
                //
                if (IsCci(CciEnum.book, pCci) && (null != pCci.sql_Table))
                    pCci.display = pCci.sql_Table.FirstRow["FULLNAME"].ToString();

                for (int i = 0; i < this.TraderLength; i++)
                    cciTrader[i].SetDisplay(pCci);
                //
                for (int i = 0; i < this.SalesLength; i++)
                    cciSales[i].SetDisplay(pCci);
                //
                for (int i = 0; i < this.BrokerLength; i++)
                    cciBroker[i].SetDisplay(pCci);

            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("TradeParty.SetDisplay", ex); }
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = false;
            isOk = ccis.Contains(pClientId_WithoutPrefix);
            isOk = isOk && (pClientId_WithoutPrefix.StartsWith(prefix));
            return isOk;
        }
        #endregion
        #endregion IContainerCci Members
        #region IContainerCciSpecified Members
        public bool IsSpecified { get { return ccis.Contains(CciClientId(CciEnum.actor)) && (null != Cci(CciEnum.actor).sql_Table); } }
        #endregion IContainerCciSpecified Members
        //
        #region Methods
        #region DumpBook_ToDocument
        /// <summary>
        /// Dump a Book into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpBook_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                SQL_Book sql_Book = null;
                bool isLoaded = false;
                bool isFound = false;
                //
                SQL_Actor sql_Actor = ((SQL_Actor)ccis[CciClientId(CciEnum.actor)].sql_Table);
                IPartyTradeIdentifier partyTradeIdentifier = GetPartyTradeIdentifier();
                //
                if (StrFunc.IsFilled(pData))
                {
                    //Check if book is a valid book for actor (Scan BOOK via IDENTIFIER & IDA)
                    int idA = 0;
                    if (null != sql_Actor)
                        idA = sql_Actor.Id;
                    //
                    //Check if actor is a valid entity, counterparty,... (Scan ACTOR via IDENTIFIER)
                    for (int i = 0; i < 2; i++)
                    {
                        string dataToFind = pData;
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        //
                        //sql_Book = new SQL_Book(cciTrade.cs, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.No, idA);
                        //20080208 PL
                        sql_Book = new SQL_Book(cciTrade.CS, SQL_TableWithID.IDType.Identifier, dataToFind, SQL_Table.ScanDataDtEnabledEnum.No, idA, SQL_Table.RestrictEnum.Yes, ccis.SessionId, ccis.IsSessionAdmin);
                        //
                        isLoaded = sql_Book.IsLoaded;
                        isFound = isLoaded && (sql_Book.RowsCount == 1);
                        //
                        //Test supplementiare et specifique aux Books 
                        //car la Vue VW_BOOK_VIEWER peut remonter plusieurs lignes avec le même Book (Seul FK est différent) Ds ce cas on le considère valide.  
                        if ((false == isFound) && sql_Book.IsLoaded && (sql_Book.RowsCount > 1))
                        {
                            isFound = true;
                            string identifier = sql_Book.Identifier;
                            for (int j = 1; j < sql_Book.RowsCount; j++)
                            {
                                if (sql_Book.GetColumnValue(j, "IDENTIFIER").ToString() != identifier)
                                {
                                    isFound = false;
                                    break;
                                }
                            }
                        }
                        //
                        if (isFound)
                            break;
                    }
                }
                //			
                pCci.errorMsg = string.Empty;
                if (isFound)
                {
                    if (null == sql_Actor)
                    {
                        string identifier = string.Empty;
                        sql_Actor = new SQL_Actor(cciTrade.CS, sql_Book.IdA, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.SessionId, ccis.IsSessionAdmin);
                        sql_Actor.AddRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY });
                        if (sql_Actor.IsLoaded)
                            identifier = sql_Actor.Identifier;
                        if (StrFunc.IsEmpty(identifier))
                        {
                            ActorAncestor aa = new ActorAncestor(cciTrade.CS, sql_Book.IdA, null, true);
                            int max = aa.GetLevelLength();
                            for (int i = 1; i < max; i++)
                            {
                                string lst = aa.GetListIdA_ActorByLevel(i, ";");
                                string[] idA = StrFunc.StringArrayList.StringListToStringArray(lst);
                                for (int j = 0; j < ArrFunc.Count(idA); j++)
                                {
                                    if (StrFunc.IsFilled(idA[j]))
                                    {
                                        SQL_Actor sql_ActorCurrent = new SQL_Actor(cciTrade.CS, Convert.ToInt32(idA[j]), SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.SessionId, ccis.IsSessionAdmin);
                                        sql_ActorCurrent.AddRoleRange(new RoleActor[] { RoleActor.COUNTERPARTY });
                                        if (sql_ActorCurrent.IsLoaded)
                                            identifier = sql_ActorCurrent.Identifier;
                                        if (StrFunc.IsFilled(identifier))
                                            break;
                                    }
                                }
                                if (StrFunc.IsFilled(identifier))
                                    break;
                            }
                        }
                        //
                        if (StrFunc.IsFilled(identifier))
                            DumpParty_ToDocument(ccis[CciClientId(CciEnum.actor)], identifier);
                    }
                    //
                    pCci.newValue = sql_Book.Identifier;
                    pCci.sql_Table = sql_Book;
                    if (!sql_Book.IsEnabled)
                        pCci.errorMsg = Ressource.GetString("Msg_BookDisabled");
                    //	
                    partyTradeIdentifier = SetPartyTradeIdentifier();
                    if (null != partyTradeIdentifier)
                    {
                        partyTradeIdentifier.bookId.Value = sql_Book.Identifier;
                        partyTradeIdentifier.bookId.scheme = Cst.OTCml_BookIdScheme;
                        partyTradeIdentifier.bookId.bookNameSpecified = true;
                        partyTradeIdentifier.bookId.bookName = sql_Book.DisplayName;
                        partyTradeIdentifier.bookId.OTCmlId = sql_Book.Id;
                        partyTradeIdentifier.bookIdSpecified = true;
                    }
                    //
                    DumpTradeSide_ToDocument(cciTrade.DataDocument.GetParty(GetPartyId()));
                }
                else
                {
                    pCci.sql_Table = null;
                    if (pCci.IsFilled || (pCci.IsEmpty && pCci.isMandatory))
                    {
                        if (isLoaded && (sql_Book.RowsCount > 1))
                            pCci.errorMsg = Ressource.GetString("Msg_BookNotUnique");
                        else
                            pCci.errorMsg = Ressource.GetString("Msg_BookNotFound");
                    }
                    //
                    if (null != partyTradeIdentifier)
                    {
                        partyTradeIdentifier.bookId.Value = pCci.newValue;
                        partyTradeIdentifier.bookId.scheme = Cst.OTCml_BookIdScheme;
                        partyTradeIdentifier.bookId.bookNameSpecified = false;
                        partyTradeIdentifier.bookId.bookName = null;
                        partyTradeIdentifier.bookId.OTCmlId = 0;
                        partyTradeIdentifier.bookIdSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("TradeParty.DumpBook_ToDocument", ex); }
        }
        #endregion DumpBook_ToDocument
        #region DumpFolderId_ToDocument
        /// <summary>
        /// Dump a FolderId into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFolderId_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                ILinkId linkId = null;
                //					
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                //
                if (null != partyTradeIdentifier)
                {
                    bool isFilled = StrFunc.IsFilled(pData);

                    if (StrFunc.IsFilled(pCci.lastValue))
                    {
                        if (isFilled)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCml_FolderIdScheme);
                        }
                        else
                        {
                            //Find LastValue and remove
                            #region Remove linkId
                            int arrCounter = 0;
                            //20080519 PL Correction de BUG avec ISchemeId 
                            foreach (ILinkId lId in partyTradeIdentifier.linkId)
                            {
                                if (Cst.OTCml_FolderIdScheme == lId.linkIdScheme)
                                {
                                    ReflectionTools.RemoveItemInArray(partyTradeIdentifier, "linkId", arrCounter);
                                    break;
                                }
                                arrCounter++;
                            }
                            #endregion Remove linkId
                        }
                    }
                    else if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCml_FolderIdScheme);
                        if (null == linkId)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                            if (null == linkId)
                            {
                                ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                                linkId = partyTradeIdentifier.linkId[partyTradeIdentifier.linkId.Length - 1];
                            }
                        }
                    }
                    //
                    partyTradeIdentifier.linkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.linkId);
                    //
                    if (isFilled)
                    {
                        pCci.errorMsg = string.Empty;
                        linkId.Value = pData;
                        linkId.linkIdScheme = Cst.OTCml_FolderIdScheme;
                        linkId.id = null;
                    }
                    else
                    {
                        pCci.errorMsg = (pCci.isMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpFolderId_ToDocument", ex); }
        }
        #endregion DumpFolderId_ToDocument
        #region DumpFrontId_ToDocument
        /// <summary>
        /// Dump a TradeId (Front Id) into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFrontId_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                ISchemeId tradeId = null;
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    bool isFilled = StrFunc.IsFilled(pData);

                    if (StrFunc.IsFilled(pCci.lastValue))
                    {
                        if (isFilled)
                        {
                            tradeId = partyTradeIdentifier.GetTradeIdFromScheme(Cst.OTCml_FrontTradeIdScheme);
                        }
                        else
                        {
                            //Find LastValue and remove
                            #region Remove tradeId
                            int arrCounter = 0;
                            foreach (ISchemeId tId in partyTradeIdentifier.tradeId)
                            {
                                if (Cst.OTCml_FrontTradeIdScheme == tId.scheme)
                                {
                                    ReflectionTools.RemoveItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), arrCounter);
                                    break;
                                }
                                arrCounter++;
                            }
                            #endregion Remove tradeId
                        }
                    }
                    else if (isFilled)
                    {
                        tradeId = partyTradeIdentifier.GetTradeIdFromScheme(Cst.OTCml_FrontTradeIdScheme);
                        if (null == tradeId)
                        {
                            tradeId = partyTradeIdentifier.GetTradeIdWithNoScheme();
                            if (null == tradeId)
                            {
                                ReflectionTools.AddItemInArray(partyTradeIdentifier, partyTradeIdentifier.GetTradeIdMemberName(), 0);
                                tradeId = partyTradeIdentifier.tradeId[partyTradeIdentifier.tradeId.Length - 1];
                            }
                        }
                    }
                    partyTradeIdentifier.tradeIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.tradeId);
                    //
                    if (isFilled)
                    {
                        partyTradeIdentifier.tradeIdSpecified = true;
                        pCci.errorMsg = string.Empty;
                        tradeId.Value = pData;
                        tradeId.scheme = Cst.OTCml_FrontTradeIdScheme;
                        tradeId.id = null;
                    }
                    else
                    {
                        pCci.errorMsg = (pCci.isMandatory ? Ressource.GetString("Msg_FrontIdNotFilled") : string.Empty);
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpFrontId_ToDocument", ex); }
        }
        #endregion DumpFrontId_ToDocument
        #region DumpFxClass_ToDocument
        /// <summary>
        /// Dump a Fx Class into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpFxClass_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.fxClass.Value = pData;
                        partyTradeIdentifier.fxClass.scheme = Cst.OTCmL_FxClassScheme;
                        partyTradeIdentifier.fxClassSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.fxClass.Value = string.Empty;
                        partyTradeIdentifier.fxClass.scheme = string.Empty;
                        partyTradeIdentifier.fxClassSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpFxClass_ToDocument", ex); }
        }
        #endregion DumpFxClass_ToDocument
        #region DumpHedgeClassDerv_ToDocument
        /// <summary>
        /// Dump a Hedge Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeClassDerv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {

                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.hedgeClassDerv.Value = pData;
                        partyTradeIdentifier.hedgeClassDerv.scheme = Cst.OTCmL_HedgeClassDervScheme;
                        partyTradeIdentifier.hedgeClassDervSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.hedgeClassDerv.Value = string.Empty;
                        partyTradeIdentifier.hedgeClassDerv.scheme = string.Empty;
                        partyTradeIdentifier.hedgeClassDervSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpHedgeClassDerv_ToDocument", ex); }
        }
        #endregion DumpHedgeClassDerv_ToDocument
        #region DumpHedgeClassNDrv_ToDocument
        /// <summary>
        /// Dump a Hedge Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeClassNDrv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();

                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.hedgeClassNDrv.Value = pData;
                        partyTradeIdentifier.hedgeClassNDrv.scheme = Cst.OTCmL_HedgeClassNDrvScheme;
                        partyTradeIdentifier.hedgeClassNDrvSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.hedgeClassNDrv.Value = string.Empty;
                        partyTradeIdentifier.hedgeClassNDrv.scheme = string.Empty;
                        partyTradeIdentifier.hedgeClassNDrvSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpHedgeClassNDrv_ToDocument", ex); }
        }
        #endregion DumpHedgeClassNDrv_ToDocument
        #region DumpHedgeFactor_ToDocument
        /// <summary>
        /// Dump a Hedge Factor into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeFactor_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                ILinkId linkId = null;
                //					
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                //
                if (null != partyTradeIdentifier)
                {
                    bool isFilled = StrFunc.IsFilled(pData);

                    if (StrFunc.IsFilled(pCci.lastValue))
                    {
                        if (isFilled)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        }
                    }
                    else if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        if (null == linkId)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                            if (null == linkId)
                            {
                                ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                                linkId = partyTradeIdentifier.linkId[partyTradeIdentifier.linkId.Length - 1];
                            }
                        }
                    }
                    //
                    partyTradeIdentifier.linkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.linkId);
                    //
                    if (isFilled)
                    {
                        linkId.linkIdScheme = Cst.OTCmL_hedgingFolderid;
                        linkId.factor = pData;
                    }
                    else
                    {
                        pCci.errorMsg = (pCci.isMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpHedgeFactor_ToDocument", ex); }
        }
        #endregion DumpHedgeFactor_ToDocument
        #region DumpHedgeFolder_ToDocument
        /// <summary>
        /// Dump a Hedge Folder into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpHedgeFolder_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                ILinkId linkId = null;
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    bool isFilled = StrFunc.IsFilled(pData);
                    if (StrFunc.IsFilled(pCci.lastValue))
                    {
                        if (isFilled)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        }
                        else
                        {
                            //Find LastValue and remove
                            #region Remove linkId
                            int arrCounter = 0;
                            foreach (ISchemeId lId in partyTradeIdentifier.linkId)
                            {
                                if (Cst.OTCmL_hedgingFolderid == lId.scheme)
                                {
                                    ReflectionTools.RemoveItemInArray(partyTradeIdentifier, "linkId", arrCounter);
                                    break;
                                }
                                arrCounter++;
                            }
                            #endregion Remove linkId
                        }
                    }
                    else if (isFilled)
                    {
                        linkId = partyTradeIdentifier.GetLinkIdFromScheme(Cst.OTCmL_hedgingFolderid);
                        if (null == linkId)
                        {
                            linkId = partyTradeIdentifier.GetLinkIdWithNoScheme();
                            if (null == linkId)
                            {
                                ReflectionTools.AddItemInArray(partyTradeIdentifier, "linkId", 0);
                                linkId = partyTradeIdentifier.linkId[partyTradeIdentifier.linkId.Length - 1];
                            }
                        }
                    }
                    //
                    partyTradeIdentifier.linkIdSpecified = ArrFunc.IsFilled(partyTradeIdentifier.linkId);
                    //
                    if (isFilled)
                    {
                        pCci.errorMsg = string.Empty;
                        linkId.Value = pData;
                        linkId.linkIdScheme = Cst.OTCmL_hedgingFolderid;
                        linkId.id = null;
                        //linkId.factor = string.Empty;
                    }
                    else
                    {
                        pCci.errorMsg = (pCci.isMandatory ? Ressource.GetString("Msg_FolderIdNotFilled") : string.Empty);
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpHedgeFolder_ToDocument", ex); }
        }
        #endregion DumpHedgeFolder_ToDocument
        #region DumpIASClassDerv_ToDocument
        /// <summary>
        /// Dump a IAS Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpIASClassDerv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.iasClassDerv.Value = pData;
                        partyTradeIdentifier.iasClassDerv.scheme = Cst.OTCmL_IASClassDervScheme;
                        partyTradeIdentifier.iasClassDervSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.iasClassDerv.Value = string.Empty;
                        partyTradeIdentifier.iasClassDerv.scheme = string.Empty;
                        partyTradeIdentifier.iasClassDervSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpIASClassDerv_ToDocument", ex); }
        }
        #endregion DumpIASClassDerv_ToDocument
        #region DumpIASClassNDrv_ToDocument
        /// <summary>
        /// Dump a IAS Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpIASClassNDrv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.iasClassNDrv.Value = pData;
                        partyTradeIdentifier.iasClassNDrv.scheme = Cst.OTCmL_IASClassNDrvScheme;
                        partyTradeIdentifier.iasClassNDrvSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.iasClassNDrv.Value = string.Empty;
                        partyTradeIdentifier.iasClassNDrv.scheme = string.Empty;
                        partyTradeIdentifier.iasClassNDrvSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpIASClassNDrv_ToDocument", ex); }
        }
        #endregion DumpIASClassNDrv_ToDocument
        #region DumpLocalClassDerv_ToDocument
        /// <summary>
        /// Dump a Local Class Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpLocalClassDerv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.localClassDerv.Value = pData;
                        partyTradeIdentifier.localClassDerv.scheme = Cst.OTCmL_LocalClassDervScheme;
                        partyTradeIdentifier.localClassDervSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.localClassDerv.Value = string.Empty;
                        partyTradeIdentifier.localClassDerv.scheme = string.Empty;
                        partyTradeIdentifier.localClassDervSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpLocalClassDerv_ToDocument", ex); }
        }
        #endregion DumpLocalClassDerv_ToDocument
        #region DumpLocalClassNDrv_ToDocument
        /// <summary>
        /// Dump a Local Class No Derivative into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpLocalClassNDrv_ToDocument(string pData)
        {
            try
            {
                IPartyTradeIdentifier partyTradeIdentifier = SetPartyTradeIdentifier();
                if (null != partyTradeIdentifier)
                {
                    if (StrFunc.IsFilled(pData))
                    {
                        partyTradeIdentifier.localClassNDrv.Value = pData;
                        partyTradeIdentifier.localClassNDrv.scheme = Cst.OTCmL_LocalClassNDrvScheme;
                        partyTradeIdentifier.localClassNDrvSpecified = true;
                    }
                    else
                    {
                        partyTradeIdentifier.localClassNDrv.Value = string.Empty;
                        partyTradeIdentifier.localClassNDrv.scheme = string.Empty;
                        partyTradeIdentifier.localClassNDrvSpecified = false;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpLocalClassNDrv_ToDocument", ex); }
        }
        #endregion DumpLocalClassNDrv_ToDocument
        #region DumpParty_ToDocument
        /// <summary>
        /// Dump a party (party, broker) into DataDocument
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        private void DumpParty_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            try
            {
                string party_id = string.Empty;
                SQL_Actor sql_Actor = null;
                IParty party = null;
                bool isLoaded = false;
                bool isFound = false;
                //
                if (StrFunc.IsFilled(pData))
                {
                    //Check if actor is a valid entity, counterparty,... (Scan ACTOR via IDENTIFIER)
                    for (int i = 0; i < 2; i++)
                    {
                        sql_Actor = null;
                        string dataToFind = pData;
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        //
                        if ((isActorSYSTEMAuthorized) && ("SYSTEM" == dataToFind.ToUpper()))
                            sql_Actor = new SQL_Actor(cciTrade.CS, dataToFind, SQL_Table.RestrictEnum.No, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.SessionId, true);
                        //                        
                        if (null == sql_Actor)
                        {
                            sql_Actor = new SQL_Actor(cciTrade.CS, dataToFind, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.SessionId, ccis.IsSessionAdmin);
                            sql_Actor.AddRoleRange(new RoleActor[] { GetRole() });
                        }
                        //	
                        isLoaded = sql_Actor.IsLoaded;
                        isFound = isLoaded && (sql_Actor.RowsCount == 1);
                        if (isFound)
                            break;
                    }
                }
                //
                if ((PartyType.party == partyType))
                {
                    string clientIdCtr = prefixParent + partyType.ToString() + (number == 1 ? "2" : "1") + CustomObject.KEY_SEPARATOR + CciEnum.actor.ToString();
                    if (ccis.Contains(clientIdCtr))
                    {
                        bool isReplaceByUnknown = false;
                        if (isFound)
                        {
                            if (StrFunc.IsFilled(ccis[clientIdCtr].newValue) && ccis[clientIdCtr].newValue.ToLower() == sql_Actor.Identifier.ToLower())
                                isReplaceByUnknown = true;
                        }
                        else
                        {
                            if (StrFunc.IsEmpty(pData) && ccis[clientIdCtr].IsEmptyValue)
                                isReplaceByUnknown = true;
                        }
                        //
                        if (isReplaceByUnknown)
                        {
                            isLoaded = false;
                            isFound = false;
                            pData = TradeCommonCustomCaptureInfos.PartyUnknown;
                            pCci.newValue = pData;
                        }
                    }
                }
                //
                party_id = (isFound ? sql_Actor.xmlId : pCci.newValue);
                //
                #region Remove Last Party if is not use
                //if (StrFunc.IsFilled(pCci.lastValue))
                //{
                    bool isLastInUSe = false;
                    //
                    if (StrFunc.IsFilled(pCci.lastValue))
                    {
                        #region isLastParty is In use
                        for (int i = 0; i < cciTrade.cciParty.Length; i++)
                        {
                            if (!(i == (number - 1) && (partyType == CciTradeParty.PartyType.party)))
                            {
                                if (pCci.lastValue == cciTrade.cciParty[i].Cci(CciEnum.actor).newValue)
                                    isLastInUSe = true;
                                if (isLastInUSe)
                                    break;
                            }
                            if (false == isLastInUSe)
                            {
                                for (int j = 0; j < cciTrade.cciParty[i].BrokerLength; j++)
                                {
                                    if (pCci.lastValue == cciTrade.cciParty[i].cciBroker[j].Cci(CciEnum.actor).newValue)
                                        isLastInUSe = true;
                                    if (isLastInUSe)
                                        break;
                                }
                            }
                        }
                        //	
                        if (false == isLastInUSe)
                        {
                            //
                            for (int i = 0; i < cciTrade.BrokerLength; i++)
                            {
                                if (!(i == (number - 1) && (partyType == CciTradeParty.PartyType.broker)))
                                {
                                    if (pCci.lastValue == cciTrade.cciBroker[i].Cci(CciEnum.actor).newValue)
                                        isLastInUSe = true;
                                    if (isLastInUSe)
                                        break;
                                }
                            }
                        }
                        #endregion IsParty In use
                    }
                    //
                    string lastParty_id = pCci.lastValue;
                    if (null != pCci.lastSql_Table)
                        lastParty_id = ((SQL_Actor)pCci.lastSql_Table).xmlId;
                    //
                    if (false == isLastInUSe)
                    {
                        cciTrade.DataDocument.RemoveParty(lastParty_id);
                        // Sur les écrans avec brokers indépendants (isBrokerOfParty = false)
                        // On réinitialize l'écran, car le nombre de broker dans le datadocument doit être en phase avec les broker à l'écran (index && count identiques)  
                        // Le 1er cciPartyBroker alimente BrokerPartyReference 0, le 2ème cciPartyBroker alimente BrokerPartyReference 1, etc....
                        if (IsPartyBroker && (false == isBrokerOfParty))
                            cciTrade.InitializeBroker_FromCci();
                    }
                    //
                    if ((PartyType.party == partyType))
                    {
                        #region Remove TradeSide
                        if (_isInitTradeSide)
                        {
                            if (cciTrade.CurrentTrade.tradeSideSpecified)
                            {
                                int arrCounter = 0;
                                foreach (ITradeSide tradeSide in cciTrade.CurrentTrade.tradeSide)
                                {
                                    if (tradeSide.id == Tools.GetTradeSideIdFromActor(lastParty_id))
                                    {
                                        ReflectionTools.RemoveItemInArray(cciTrade.CurrentTrade, "tradeSide", arrCounter);
                                        break;
                                    }
                                    arrCounter++;
                                }
                            }
                        }
                        #endregion Remove TradeSide
                    }
                //}
                #endregion Remove Last Party if is not use
                //
                #region Add Party
                if (StrFunc.IsFilled(pData))
                    party = cciTrade.DataDocument.AddParty(party_id);
                #endregion Add Party
                //
                #region Add brokerPartyReference if is IsPartyBroker
                if (IsPartyBroker)
                {
                    if (false == isBrokerOfParty)
                    {
                        //use Number Alimentation de 
                        IReference partyRef = cciTrade.CurrentTrade.brokerPartyReference[number - 1];
                        partyRef.hRef = party_id;
                        cciTrade.CurrentTrade.brokerPartyReferenceSpecified = ArrFunc.IsFilled(cciTrade.CurrentTrade.brokerPartyReference);
                    }
                    else
                    {
                        //Lorsque l'on supprime un broker et que ce dernier est utilisé ailleurs (isLastInUSe= true)
                        // On vient mettre à blanc la le pointeur existant dans partyTradeInfo du cciBrokerParent
                        if (isLastInUSe)
                        {
                            IPartyTradeInformation partyTradeInfo = cciTrade.DataDocument.GetPartyTradeInformation(cciBrokerParent.GetPartyId());
                            if (null != partyTradeInfo)
                                partyTradeInfo.brokerPartyReference[number - 1].hRef = party_id;
                        }
                    }
                }
                #endregion Add brokerPartyReference if is IsPartyBroker
                //
                if (isFound)
                {
                    pCci.newValue = sql_Actor.Identifier;
                    pCci.sql_Table = sql_Actor;
                    pCci.errorMsg = string.Empty;
                    //
                    Tools.SetParty(party, sql_Actor);
                    //20090415 FI Création du partyTradeIdentifier (si non Renseigné CleanUp fera le menage)
                    SetPartyTradeIdentifier();
                    CreatePartyTradeInformation();
                    //
                    //20081124 FI Initialize_FromCci associée à la contrepartie 
                    //Cette dernière initialize les traders,sales, brokers en fonction des zones présentes sur l'écran 
                    if (null != cciBrokerParent)
                        cciBrokerParent.Initialize_FromCci();
                    else
                        Initialize_FromCci();
                    //
                    // l'alimentation de partyTradeInfo doit être effectué après Initialize_FromCci 
                    //(cet dernière crée  partyTradeInfo s'il n'existe pas 
                    if (IsPartyBroker && isBrokerOfParty)
                    {
                        cciTrade.DataDocument.AddBroker(party_id);
                        IPartyTradeInformation partyTradeInfo = cciTrade.DataDocument.GetPartyTradeInformation(cciBrokerParent.GetPartyId());
                        partyTradeInfo.brokerPartyReference[number - 1].hRef = party_id;
                    }
                    //
                    DumpTradeSide_ToDocument(party);
                }
                else
                {
                    pCci.errorMsg = string.Empty;
                    pCci.sql_Table = null;
                    //					
                    if (pCci.IsFilled)
                    {
                        if (cciTrade.IsStEnvTemplate)
                        {
                            if (pCci.newValue != Cst.FpML_EntityOfUserIdentifier)
                                pCci.errorMsg = Ressource.GetString("Msg_ActorNotFound");
                        }
                        else
                        {
                            if (isLoaded)
                                pCci.errorMsg = Ressource.GetString("Msg_ActorNotUnique");
                            else
                                pCci.errorMsg = Ressource.GetString("Msg_ActorNotFound");
                        }
                    }
                    //
                    if (null != party)
                    {
                        party.id = party_id;
                        party.OTCmlId = 0;
                        party.partyId = party_id;
                        party.partyName = string.Empty;
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpParty_ToDocument", ex); }
        }
        #endregion DumpParty_ToDocument
        #region DumpTradeSide_ToDocument
        private void DumpTradeSide_ToDocument(IParty pParty)
        {
            try
            {
                if (_isInitTradeSide)
                {
                    if (partyType == PartyType.party)
                    {
                        cciTrade.DataDocument.SetTradeSide(cciTrade.CS, pParty);
                    }
                    else if (partyType == PartyType.broker)
                    {
                        //Si Broker
                        for (int i = 0; i < cciTrade.PartyLength; i++)
                        {
                            IParty party = cciTrade.DataDocument.GetParty(cciTrade.cciParty[i].GetPartyId());
                            if (null != party)
                                cciTrade.DataDocument.SetTradeSide(cciTrade.CS, party);
                        }
                    }
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.DumpTradeSide_ToDocument", ex); }
        }
        #endregion DumpTradeSide_ToDocument

        #region GetCalculationAgent
        public string GetCalculationAgent()
        {
            CustomCaptureInfo cci = ccis[CciClientId(CciEnum.book.ToString())];
            if ((null != cci) && (null != cci.sql_Table))
            {
                SQL_Book book = (SQL_Book)cci.sql_Table;
                if ((null != book) && (0 < book.IdA_Entity))
                    return GetPartyId();
            }
            return null;
        }
        #endregion GetCalculationAgent
        #region GetCalculationAgentBC
        public string GetCalculationAgentBC()
        {
            CustomCaptureInfo cci = ccis[CciClientId(CciTradeParty.CciEnum.book.ToString())];
            if ((null != cci) && (null != cci.sql_Table))
            {
                SQL_Book book = (SQL_Book)cci.sql_Table;
                if ((null != book) && (0 < book.IdA_Entity))
                {
                    SQL_Actor actor = new SQL_Actor(cciTrade.CS, book.IdA_Entity);
                    actor.WithInfoEntity = true;
                    if (actor.IsLoaded)
                        return actor.IdBC;
                }
            }
            return null;
        }
        #endregion GetCalculationAgentBC
        #region GetPartyId
        public string GetPartyId()
        {
            try
            {
                string ret = string.Empty;
                //
                CustomCaptureInfo cci = Cci(CciEnum.actor);
                //
                if ((null != cci) && (null != cci.sql_Table))
                    ret = ((SQL_Actor)ccis[CciClientId(CciEnum.actor)].sql_Table).xmlId;
                //
                return ret;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.GetPartyId", ex); }
        }
        #endregion GetPartyId
        #region GetPartyTradeIdentifier
        private IPartyTradeIdentifier GetPartyTradeIdentifier()
        {
            try
            {
                string partyId = GetPartyId();
                return  cciTrade.DataDocument.GetPartyTradeIdentifier(partyId);
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.GetPartyTradeIdentifier", ex); }
        }
        #endregion GetPartyTradeIdentifier
        #region GetPartyTradeInformation
        public IPartyTradeInformation GetPartyTradeInformation()
        {
            try
            {
                IPartyTradeInformation retPartyTradeInformation = null;
                string partyId = GetPartyId();
                retPartyTradeInformation = cciTrade.DataDocument.GetPartyTradeInformation(partyId);
                return retPartyTradeInformation;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.GetPartyTradeInformation", ex); }
        }
        #endregion GetPartyTradeInformation
        #region GetRole
        private RoleActor GetRole()
        {
            try
            {
                RoleActor ret = RoleActor.NONE;
                //
                if (IsPartyBroker)
                    ret = RoleActor.BROKER;
                else if (this.cciTrade.DataDocument.isDebtSecurity)
                    ret = RoleActor.ISSUER;
                else
                    ret = RoleActor.COUNTERPARTY;
                return ret;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.GetRole", ex); }
        }
        #endregion
        #region GetActorIda
        public int GetActorIda()
        {
            int ret = -99;
            SQL_Actor sql_Actor = null;
            try
            {
                sql_Actor = (SQL_Actor)Cci(CciEnum.actor).sql_Table;
                if (null != sql_Actor)
                    ret = sql_Actor.Id;
            }
            catch { ret = -99; }
            //
            return ret;
        }
        #endregion GetActorIda
        #region GetBookIdB
        public int GetBookIdB()
        {
            int ret = -99;
            SQL_Book sql_book = null;
            try
            {
                sql_book = (SQL_Book)Cci(CciEnum.book).sql_Table;
                if (null != sql_book)
                    ret = sql_book.Id;
            }
            catch { ret = -99; }
            //
            return ret;
        }
        #endregion GetBookIdB

        #region InitializePartySide
        public void InitializePartySide()
        {
            try
            {
                string idPayer = string.Empty;
                string lastIdPayer = string.Empty;
                //
                if (null != ccis[cciTrade.CciClientIdPayer])
                {
                    idPayer = ccis[cciTrade.CciClientIdPayer].newValue;
                    lastIdPayer = ccis[cciTrade.CciClientIdPayer].lastValue;
                }
                //
                CustomCaptureInfo cciSide = Cci(CciEnum.side);
                CustomCaptureInfo cciParty = Cci(CciEnum.actor);
                //
                if ((null != cciSide) && (null != cciParty))
                {
                    SQL_Actor sql_Actor = (SQL_Actor)cciParty.sql_Table;
                    if (((null != sql_Actor) && (idPayer == sql_Actor.xmlId)) ||                                //cas Normal
                          (lastIdPayer == cciParty.lastValue && lastIdPayer == Cst.FpML_EntityOfUserIdentifier) || //2eme Cas pour EntityOfUSer
                          (idPayer == cciParty.newValue)                                                           //3eme Cas {unknown},
                        )
                        cciSide.newValue = cciTrade.RetSidePayer;
                    else
                        cciSide.newValue = cciTrade.RetSideReceiver;

                    #region Garde fou pour que les sens du DO et de la CTR soit tjs opposés
                    if (this.number == 2)
                    {
                        if (cciSide.newValue == ccis[cciTrade.cciParty[0].CciClientId(CciTradeParty.CciEnum.side)].newValue)
                        {
                            if (cciSide.newValue == cciTrade.RetSideReceiver)
                                cciSide.newValue = cciTrade.RetSidePayer;
                            else
                                cciSide.newValue = cciTrade.RetSideReceiver;
                        }
                    }
                    #endregion Garde fou pour que les sens du DO et de la CTR soit tjs opposés
                    //
                    cciSide.lastValue = cciSide.newValue;
                }
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.InitializePartySide", ex); }
        }
        #endregion InitializePartySide
        #region InitializeTrader_FromCci
        private void InitializeTrader_FromCci()
        {
            try
            {
                bool bSavPartyTradeInformationSpecified;
                bool isOk = true;
                int index = -1;
                ArrayList lst = new ArrayList();
                IPartyTradeInformation partyTradeInformation = null;
                bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified;

                while (isOk)
                {
                    index += 1;
                    CciTrader ccitraderCurrent = new CciTrader(this, index + 1, prefix, CciTrader.TraderTypeEnum.trader);
                    isOk = ccis.Contains(ccitraderCurrent.CciClientId(CciTrader.CciEnum.identifier));
                    if (isOk)
                    {
                        if (ccis.CaptureMode == Cst.Capture.ModeEnum.Consult || ccis.CaptureMode == Cst.Capture.ModeEnum.RemoveOnly)
                            partyTradeInformation = GetPartyTradeInformation();
                        else
                            partyTradeInformation = CreatePartyTradeInformation();
                        //
                        if (null != partyTradeInformation)
                        {
                            bool bSavSpecified = partyTradeInformation.traderSpecified;
                            if (ArrFunc.IsEmpty(partyTradeInformation.trader) || (index == partyTradeInformation.trader.Length))
                                ReflectionTools.AddItemInArray(partyTradeInformation, "trader", index);
                            //
                            ccitraderCurrent.trader = partyTradeInformation.trader[index];
                            partyTradeInformation.traderSpecified = bSavSpecified;
                        }
                        lst.Add(ccitraderCurrent);
                    }
                }
                //
                cciTrader = null;
                cciTrader = (CciTrader[])lst.ToArray(typeof(CciTrader));
                //
                cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified = bSavPartyTradeInformationSpecified;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.InitializeTrader_FromCci", ex); }
        }
        #endregion InitializeTrader_FromCci
        #region InitializeSales_FromCci
        private void InitializeSales_FromCci()
        {
            try
            {
                bool bSavPartyTradeInformationSpecified;
                bool isOk = true;
                int index = -1;
                ArrayList lst = new ArrayList();
                IPartyTradeInformation partyTradeInformation = null;
                bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified;

                while (isOk)
                {
                    index += 1;
                    CciTrader cciSalesCurrent = new CciTrader(this, index + 1, prefix, CciTrader.TraderTypeEnum.sales);
                    isOk = ccis.Contains(cciSalesCurrent.CciClientId(CciTrader.CciEnum.identifier));
                    if (isOk)
                    {
                        if (ccis.CaptureMode == Cst.Capture.ModeEnum.Consult || ccis.CaptureMode == Cst.Capture.ModeEnum.RemoveOnly)
                            partyTradeInformation = GetPartyTradeInformation();
                        else
                            partyTradeInformation = CreatePartyTradeInformation();
                        //
                        if (null != partyTradeInformation)
                        {
                            bool bSavSpecified = partyTradeInformation.traderSpecified;
                            if (ArrFunc.IsEmpty(partyTradeInformation.sales) || (index == partyTradeInformation.sales.Length))
                                ReflectionTools.AddItemInArray(partyTradeInformation, "sales", index);
                            //
                            cciSalesCurrent.trader = partyTradeInformation.sales[index];
                            partyTradeInformation.salesSpecified = bSavSpecified;
                        }
                        lst.Add(cciSalesCurrent);
                    }
                }
                //
                cciSales = null;
                cciSales = (CciTrader[])lst.ToArray(typeof(CciTrader));
                //
                cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified = bSavPartyTradeInformationSpecified;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.InitializeSales_FromCci", ex); }
        }
        #endregion InitializeSales_FromCci

        #region InitializeBroker_FromCci
        private void InitializeBroker_FromCci()
        {
            try
            {
                bool bSavPartyTradeInformationSpecified;
                bool isOk = true;
                int index = -1;
                ArrayList lst = new ArrayList();
                IPartyTradeInformation partyTradeInformation = null;
                bSavPartyTradeInformationSpecified = cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified;   
                //
                while (isOk)
                {
                    index += 1;
                    CciTradeParty ccibrokerCurrent = new CciTradeParty(cciTrade, index + 1, PartyType.broker, prefix);
                    ccibrokerCurrent.cciBrokerParent = this;
                    isOk = ccis.Contains(ccibrokerCurrent.CciClientId(CciTradeParty.CciEnum.actor));
                    if (isOk)
                    {
                        if (ccis.CaptureMode == Cst.Capture.ModeEnum.Consult || ccis.CaptureMode == Cst.Capture.ModeEnum.RemoveOnly)
                            partyTradeInformation = GetPartyTradeInformation();
                        else
                            partyTradeInformation = CreatePartyTradeInformation();
                        //
                        if (null != partyTradeInformation)
                        {
                            bool bSavSpecified = partyTradeInformation.brokerPartyReferenceSpecified; 
                            
                            if ((ArrFunc.IsEmpty(partyTradeInformation.brokerPartyReference) || (index == partyTradeInformation.brokerPartyReference.Length)))
                                ReflectionTools.AddItemInArray(partyTradeInformation, "brokerPartyReference", index);
                            //
                            partyTradeInformation.brokerPartyReferenceSpecified = bSavSpecified;
                        }
                        lst.Add(ccibrokerCurrent);
                    }
                }
                //
                cciBroker = null;
                cciBroker = (CciTradeParty[])lst.ToArray(typeof(CciTradeParty));
                //
                cciTrade.CurrentTrade.tradeHeader.partyTradeInformationSpecified = bSavPartyTradeInformationSpecified;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.InitializeTrader_FromCci", ex); }
        }
        #endregion InitializeTrader_FromCci

        #region SetIsMandatoryFromBook
        private void SetIsMandatoryOnClass()
        {
            try
            {
                bool isIFRS = ccis.SQLInstrument.IsIFRS;
                CustomCaptureInfo cci = Cci(CciEnum.book);
                bool isMandatory = false;
                //
                if ((null != cci) && (null != cci.sql_Table))
                {
                    SQL_Book sql_Book = (SQL_Book)cci.sql_Table;
                    isMandatory = ((null != sql_Book) && (sql_Book.IdA_Entity > 0));
                    //
                    if (isMandatory)
                    {
                        isMandatory = false;
                        if (null != ccis[CciClientId(CciEnum.actor)].sql_Table)
                        {
                            int ida = ((SQL_Actor)ccis[CciClientId(CciEnum.actor)].sql_Table).Id;
                            isMandatory = (false == ActorTools.IsActorClient(cciTrade.CS, ida));
                        }
                    }
                }
                //Derv
                ccis.Set(CciClientId(CciEnum.localClassDerv), "isMandatory", isMandatory);
                //20070808 FI ticket 15636 => add isIFRS
                ccis.Set(CciClientId(CciEnum.iasClassDerv), "isMandatory", isMandatory && isIFRS);
                //20070808 FI ticket 15636 => mise en commentaire, isMandatory est affecté en cascade par la mise à jour de iasClassDerv  
                //ccis.Set(CciClientId(CciEnum.hedgeClassDerv),"isMandatory",isMandatory); 
                //NDrv
                ccis.Set(CciClientId(CciEnum.localClassNDrv), "isMandatory", isMandatory);
                //20070808 FI ticket 15636 => add isIFRS
                ccis.Set(CciClientId(CciEnum.iasClassNDrv), "isMandatory", isMandatory && isIFRS);
                //20070808 FI ticket 15636 => mise en commentaire, isMandatory est affecté en cascade par la mise à jour de iasClassDerv  
                //ccis.Set(CciClientId(CciEnum.hedgeClassNDrv),"isMandatory",isMandatory); isMandatory est affecté en cascade par la mise à jour de iasClassNDrv
                //Fx
                ccis.Set(CciClientId(CciEnum.fxClass), "isMandatory", isMandatory);
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.SetIsMandatoryOnClass", ex); }
        }
        #endregion SetIsMandatoryFromBook
        #region SetPartyTradeIdentifier
        private IPartyTradeIdentifier SetPartyTradeIdentifier()
        {
            try
            {
                string partyId = GetPartyId();
                IPartyTradeIdentifier ret = null;
                if (StrFunc.IsFilled(partyId))
                {
                    ret = cciTrade.DataDocument.GetPartyTradeIdentifier(partyId);
                    //
                    if (null == ret)
                        ret = cciTrade.DataDocument.GetPartyTradeIdentifierEmpty();
                    //
                    if (null == ret)
                    {
                        #region Add partyTradeIdentifier
                        ReflectionTools.AddItemInArray(cciTrade.DataDocument.tradeHeader, "partyTradeIdentifier", 0);
                        ret = cciTrade.DataDocument.tradeHeader.partyTradeIdentifier[cciTrade.CurrentTrade.tradeHeader.partyTradeIdentifier.Length - 1];
                        #endregion Add partyTradeIdentifier
                    }
                    //
                    if (StrFunc.IsEmpty(ret.partyReference.hRef))
                        ret.partyReference.hRef = partyId;
                }
                return ret;
            }
            catch (OTCmlException ex) { throw ex; }
            catch (Exception ex) { throw new OTCmlException("CciTradeParty.setPartyTradeIdentifier", ex); }
        }
        #endregion SetPartyTradeIdentifier

        #region CreatePartyTradeInformation
        private IPartyTradeInformation CreatePartyTradeInformation()
        {
            IPartyTradeInformation ret = GetPartyTradeInformation();
            //
            if (null == ret)
                ret = cciTrade.DataDocument.AddPartyTradeInformation(GetPartyId());
            //
            return ret;
        }
        #endregion

        #endregion Methods
    }
    #endregion CciTradeParty
}
