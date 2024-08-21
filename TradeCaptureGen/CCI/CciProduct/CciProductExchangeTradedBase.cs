#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FpML.Interface;
using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CciProductExchangeTradedBase : CciProductBase
    {
        #region Members



        #endregion Members

        #region Enums
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            buyer,
            seller,
            Category,
        }
        #endregion Enums

        #region Accessors
        /// <summary>
        /// Obtient la collection des ccis
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        /// <summary>
        /// 
        /// </summary>
        protected CciTrade CciTrade
        {
            get { return base.CciTradeCommon as CciTrade; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CciFixTradeCaptureReport CciFixTradeCaptureReport { get; private set; }

        #region CciExchange
        // EG 20171113 [23509] New
        public override CustomCaptureInfo CciExchange
        {
            get
            {
                return CciFixTradeCaptureReport.CciFixInstrument.CciExch;
            }
        }
        /// <summary>
        /// Retourne la colonne SQL utilisée pour alimenter la propriété newValue du cci Exchange
        /// </summary>
        /// FI 20200116 [XXXXX] Add
        public override string CciExchangeColumn
        {
            get
            {
                return CciFixTradeCaptureReport.CciFixInstrument.CciExchColumn;
            }
        }
        #endregion CciExchange

        /// <summary>
        /// 
        /// </summary>
        public ExchangeTradedContainer ExchangeTradedContainer { get; private set; }

        #region IdA_Custodian
        // EG 20171031 [23509] New
        public override Nullable<int> IdA_Custodian
        {
            get
            {
                return ExchangeTradedContainer.IdA_Custodian;
            }
        }
        #endregion IdA_Custodian

        /// <summary>
        /// 
        /// </summary>
        public bool IsExistBookDealer
        {
            get
            {
                bool ret = false;
                IFixParty party = ExchangeTradedContainer.GetDealer();
                if (null != party)
                    ret = (0 < CciTrade.TradeCommonInput.DataDocument.GetOTCmlId_Book(party.PartyId.href));
                return ret;
            }
        }

        
        #endregion Accessors

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pExchangeTraded"></param>
        /// <param name="pPrefix"></param>
        public CciProductExchangeTradedBase(CciTrade pCciTrade, IExchangeTradedBase pExchangeTraded, string pPrefix)
            : this(pCciTrade, pExchangeTraded, pPrefix, -1)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pExchangeTraded"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductExchangeTradedBase(CciTrade pCciTrade, IExchangeTradedBase pExchangeTraded, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pExchangeTraded, pPrefix, pNumber)
        {
        }
        #endregion Constructors

        #region Interfaces

        #region ITradeCci Members
        /// <summary>
        /// 
        /// </summary>
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        /// <summary>
        /// 
        /// </summary>
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion ITradeCci Members

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, ExchangeTradedContainer.ExchangeTraded);
            CciFixTradeCaptureReport.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// EG 20120619 New Take-Up/Give-Up 
        /// FI 20160921 [XXXXX] Modify
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {

            // il n'existe pas de buyer/seller sur les allocation
            // ils existent sur les strategies puisque la saisie est un deal  
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            //
            CciFixTradeCaptureReport.AddCciSystem();
            //
            //Sur les stratgies flexibles, le buyer est peut-être optionnel
            //Si c'est le cas tous les ccis rattaché à ce produit deviennent optionnel 
            //Code nécessaire car les ccis généré automatiquement à partir d'un ciiContainerGlobal sont obligatoire lorsque le cciSource est obligatoire
            CustomCaptureInfo cci = CcisBase[CciClientIdPayer];
            if (false == cci.IsMandatory)
            {
                foreach (CustomCaptureInfo cciItem in CcisBase)
                {
                    if (this.IsCciOfContainer(cciItem.ClientId_WithoutPrefix))
                        cciItem.IsMandatory = false;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            IReference buyer = ExchangeTradedContainer.BuyerPartyReference;
                            if (null != buyer)
                            {
                                // RD 20200921 [25246] Afficher partyId (équivalent à l'Identifier de l'acteur) car hRef est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                                // EG 20201007 [25246] Correction Alimentation data
                                //data = buyer.hRef;
                                IParty party = CciTrade.TradeCommonInput.DataDocument.GetParty(buyer.HRef);
                                if (null != party)
                                    //data = party.partyId;
                                    data = party.Id;
                            }
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            IReference seller = ExchangeTradedContainer.SellerPartyReference;
                            if (null != seller)
                            {
                                // RD 20200921 [25246] Afficher partyId (équivalent à l'Identifier de l'acteur) car hRef est toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                                // EG 20201007 [25246] Correction Alimentation data
                                //data = seller.hRef;
                                IParty party = CciTrade.TradeCommonInput.DataDocument.GetParty(seller.HRef);
                                if (null != party)
                                    //data = party.partyId;
                                    data = party.Id;
                            }
                            break;
                        #endregion Seller
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            CciFixTradeCaptureReport.Initialize_FromDocument();
        }
        /* FI 20200421 [XXXXX] Mise en commentaire
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170130 [22788] Modify 
        public override void Dump_ToDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset variables
                    //
                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            // FI 20170130 [22788] Alimentation de _exchangeTradedContainer.buyerPartyReference
                            //_exchangeTradedContainer.buyerPartyReference.hRef = data;
                            RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);
                            // RD 20170130 [22788] Add test
                            // RD 20200921 [25246] hRef doit être toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                            if (null != _exchangeTradedContainer.buyerPartyReference)
                                //_exchangeTradedContainer.buyerPartyReference.hRef = data;
                                _exchangeTradedContainer.buyerPartyReference.hRef = XMLTools.GetXmlId(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            // FI 20170130 [22788] Alimentation de _exchangeTradedContainer.sellerPartyReference
                            //_exchangeTradedContainer.sellerPartyReference.hRef = data;
                            RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);
                            // RD 20170130 [22788] Add test
                            // RD 20200921 [25246] hRef doit être toujours valorisé par un XmlId (cas des acteurs avec Identifier commençant par un chiffre)
                            if (null != _exchangeTradedContainer.sellerPartyReference)
                                //_exchangeTradedContainer.sellerPartyReference.hRef = data;
                                _exchangeTradedContainer.sellerPartyReference.hRef = XMLTools.GetXmlId(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Seller
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            CciFixTradeCaptureReport.Dump_ToDocument();

            // RD 20120215 
            // En mode Transfert de position, il faut mettre à jour le compte (IFixTrdCapRptSideGrp.Account),
            // en fonction du Book nouvellement saisi sur la partie destinatrice du Transfert
            //
            // La Synchronisation est à faire dans les cas suivants uniquement:
            // - Génération d'un nouveau Trade: 
            //      Création 
            //      Duplication 
            //      Annulation avec remplaçante 
            //      Transfert de position
            // - Modification d'un Trade existant:  
            //      Modification (avec ou sans génération d'évènement)
            //      Modification d'une facture allouée
            // EG 20160404 Migration vs2013
            if (Cst.Capture.IsModeNewCapture(ccis.CaptureMode) || Cst.Capture.IsModeUpdateGen(ccis.CaptureMode))
            {
                //Product.SynchronizeExchangeTraded();
                Product.SynchronizeFromDataDocument();
            }

        }
        */

        /*
        FI 20240307 [WI862] Mise en commentaire (déjà effectué via l'appel à ProductContainerBase.SynchronizeFromDataDocument)
        #region DumpBizDt_ToDocument
        /// <summary>
        /// Dump a clearedDate into DataDocument (FIXML => BizDt)
        /// </summary>
        /// <param name="pData"></param>
        // EG 20171031 [23509] New 
        public override void DumpBizDt_ToDocument(string pData)
        {
            CciFixTradeCaptureReport.DumpBizDt_ToDocument(pData);
        }
        #endregion DumpBizDt_ToDocument
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            bool isOk = IsCciOfContainer(pCci.ClientId_WithoutPrefix);
            if (isOk)
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                {
                    CciEnum key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    switch (key)
                    {
                        case CciEnum.buyer:
                            if (ExchangeTradedContainer.IsOneSide) //=> mise à jour du side associé à l'unique TrdCapRptSideGrp  
                            {
                                if (ExchangeTradedContainer.BuyerPartyReference != null)
                                {
                                    string clientIdSide = CciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_Side);
                                    if (CciTrade.cciParty[0].GetPartyId(true) == ExchangeTradedContainer.BuyerPartyReference.HRef)
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                                    else
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                                }
                            }
                            CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            //
                            if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.buyer).IsMandatory))
                                Clear();
                            //
                            if (false == Cci(CciEnum.buyer).IsMandatory)
                                SetEnabled(pCci.IsFilledValue);
                            //
                            break;
                        case CciEnum.seller:
                            if (ExchangeTradedContainer.IsOneSide) //=> mise à jour du side associé à l'unique TrdCapRptSideGrp  
                            {
                                if (ExchangeTradedContainer.SellerPartyReference != null)
                                {
                                    string clientIdSide = CciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_Side);
                                    if (CciTrade.cciParty[0].GetPartyId(true) == ExchangeTradedContainer.SellerPartyReference.HRef)
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                                    else
                                        CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                                }
                            }
                            CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;

                    }
                }

            }

            CciFixTradeCaptureReport.ProcessInitialize(pCci);

            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(CciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    #region pré proposition de la gestion de position à partir du book
                    //Ne fonctionne que pour allocations, seuls écrans qui affichent la gestion de position 
                    if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
                    {
                        string clientId = CciFixTradeCaptureReport.CciRptSide.CciClientId(CciRptSide.CciEnum.PosEfct);
                        CcisBase.Set(clientId, "IsEnabled", true);
                    }
                    #endregion

                    if (ExchangeTradedContainer.IsOneSide)
                    {
                        #region Préproposition de la contrepartie avec la chambre si l'entité est Clearing Member de la chambre associée au marché
                        //Lorsque le book ou le marché sont renseignés et que la zone chambre est vide
                        if ((CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) ||
                            CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci))
                            &&
                            (CciTrade.cciParty[1].Cci(CciTradeParty.CciEnum.actor).IsEmpty)
                            )
                        {
                            SetCssByEntityClearingMember();
                        }
                        #endregion
                        //
                        #region préproposition de la contrepartie en fonction du ClearingTemplate
                        if (CciTrade.cciParty[1].IsInitFromClearingTemplate)
                        {
                            if (
                                CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) ||
                                CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci) ||
                                CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Sym, pCci)
                                )
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                        }
                        #endregion
                    }
                }

                // EG 20171031 [23509] Upd
                CustomCaptureInfo cciToInitializeDates = pCci;
                if (IsCci(CciEnum.buyer, pCci) || IsCci(CciEnum.seller, pCci))
                    cciToInitializeDates = CciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book);
                if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, cciToInitializeDates) ||
                    CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, cciToInitializeDates))
                {
                    InitializeDates(cciToInitializeDates);
                }
            }

            // RD 20150515 [20954] Alimentation du flux XML avec l'Executing/Clearing Broker
            if (ExchangeTradedContainer.IsOneSide && CciTrade.TradeCommonInput.IsETDandAllocation)
            {
                // La Party à droite
                CciTradeParty cciClearer = CciTrade.cciParty[1];
                // Il s'agit de la saisie d'un Broker sur la partie droite
                bool isCciClearer_Broker = (ArrFunc.IsFilled(cciClearer.cciBroker) && cciClearer.cciBroker[0].IsCci(CciTradeParty.CciEnum.actor, pCci));
                // Il s'agit de la saisie de la Party droite (Voir la méthode CciTradeParty.IntercepActor)                    
                //Lorsque l'utilisateur saisi un acteur BROKER,CLEARER
                //Si l'entité est Membre de la chambre
                //Alors Spheres® positionne 
                // - dans la zone Clearing la chambre
                // - dans la zone broker associé, l'acteur saisi par l'utilsateur 
                bool isCciClearer = cciClearer.IsCci(CciTradeParty.CciEnum.actor, pCci);

                if (isCciClearer_Broker || isCciClearer)
                {
                    IFixTrdCapRptSideGrp[] trdCapRptSideGrp = ExchangeTradedContainer.TradeCaptureReport.TrdCapRptSideGrp;

                    if ((false == pCci.HasError) && (pCci.IsFilledValue))
                    {
                        string brokerId = string.Empty;
                        int brokerIda = 0;

                        if (isCciClearer_Broker)
                        {
                            // On pioche les infos dans le Cci Broker
                            brokerIda = cciClearer.cciBroker[0].GetActorIda();
                            brokerId = cciClearer.cciBroker[0].GetPartyId();
                        }
                        else
                        {
                            // On pioche les infos dans le DataDocument, car la méthode CciTradeParty.IntercepActor modifie directement le DataDocument
                            string clearerPartyId = cciClearer.GetPartyId();
                            if (StrFunc.IsFilled(clearerPartyId))
                            {
                                IPartyTradeInformation _clearerPartyTradeInformation = CciTrade.TradeCommonInput.DataDocument.GetPartyTradeInformation(clearerPartyId);
                                if (_clearerPartyTradeInformation != null & _clearerPartyTradeInformation.BrokerPartyReferenceSpecified)
                                {
                                    brokerId = _clearerPartyTradeInformation.BrokerPartyReference[0].HRef;
                                    if (StrFunc.IsFilled(brokerId))
                                        brokerIda = CciTrade.TradeCommonInput.DataDocument.GetParty(brokerId).OTCmlId;
                                }
                            }
                        }

                        // Création de la Party (IFixParty) pour le Broker, voir la méthode TradeCommonCaptureGen.InsertTradeActorBroker
                        if (brokerIda > 0 && StrFunc.IsFilled(brokerId))
                        {
                            FixML.v50SP1.Enum.PartyRoleEnum brokerRole = FixML.v50SP1.Enum.PartyRoleEnum.ExecutingFirm;
                            if (CciTrade.TradeCommonInput.IsGiveUp(CcisBase.CS, null) &&
                                ActorTools.IsActorWithRole(CSTools.SetCacheOn(CcisBase.CS), brokerIda, RoleActor.CLEARER))
                            {
                                brokerRole = FixML.v50SP1.Enum.PartyRoleEnum.GiveupClearingFirm;
                            }

                            IFixParty fixPartyBroker = RptSideTools.GetParty(trdCapRptSideGrp[0], brokerRole);
                            if (null == fixPartyBroker)
                                fixPartyBroker = RptSideTools.AddParty(trdCapRptSideGrp[0]);

                            RptSideTools.SetParty(fixPartyBroker, brokerId, brokerRole);
                        }
                    }
                    else if (isCciClearer_Broker)
                    {
                        // Suppression d'un Broker sur la partie droite
                        RptSideTools.RemoveParty(trdCapRptSideGrp[0], FixML.v50SP1.Enum.PartyRoleEnum.ExecutingFirm);
                        RptSideTools.RemoveParty(trdCapRptSideGrp[0], FixML.v50SP1.Enum.PartyRoleEnum.GiveupClearingFirm);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            //
            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            CciFixTradeCaptureReport.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            CciFixTradeCaptureReport.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Modify
        public override void Initialize_Document()
        {
            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
                CciFixTradeCaptureReport.Initialize_Document();

            // FI 20161214 [21916] call base.InitializeRptSideElement 
            InitializeRptSideElement();
            CciFixTradeCaptureReport.CciRptSide.RptSide = this.ExchangeTradedContainer.TradeCaptureReport.TrdCapRptSideGrp;

        }

        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            CciFixTradeCaptureReport.RefreshCciEnabled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Modify
        /// FI 20170130 [22788] Modify 
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public override void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            // FI 20170130 [22788] Alimentation de _exchangeTradedContainer.buyerPartyReference
                            //_exchangeTradedContainer.buyerPartyReference.hRef = data;
                            RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);
                            // RD 20170130 [22788] Add test
                            if (null != ExchangeTradedContainer.BuyerPartyReference)
                                ExchangeTradedContainer.BuyerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            // FI 20170130 [22788] Alimentation de _exchangeTradedContainer.sellerPartyReference
                            //_exchangeTradedContainer.sellerPartyReference.hRef = data;
                            RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);
                            // RD 20170130 [22788] Add test
                            if (null != ExchangeTradedContainer.SellerPartyReference)
                                ExchangeTradedContainer.SellerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Seller
                        #region default
                        default:
                            isSetting = false;
                            break;
                            #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                }
            }

            //
            CciFixTradeCaptureReport.Dump_ToDocument();

            // RD 20120215 
            // En mode Transfert de position, il faut mettre à jour le compte (IFixTrdCapRptSideGrp.Account),
            // en fonction du Book nouvellement saisi sur la partie destinatrice du Transfert
            //
            // La Synchronisation est à faire dans les cas suivants uniquement:
            // - Génération d'un nouveau Trade: 
            //      Création 
            //      Duplication 
            //      Annulation avec remplaçante 
            //      Transfert de position
            // - Modification d'un Trade existant:  
            //      Modification (avec ou sans génération d'évènement)
            //      Modification d'une facture allouée
            // EG 20160404 Migration vs2013
            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
            {
                //Product.SynchronizeExchangeTraded();
                Product.SynchronizeFromDataDocument();
            }

        }

        #endregion IContainerCciFactory Members

        #region IContainerCciPayerReceiver Members
        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer); }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion IContainerCciPayerReceiver Members

        #region IContainerCciGetInfoButton Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            CciFixTradeCaptureReport.SetButtonReferential(pCci, pCo);
        }

        #endregion IContainerCciGetInfoButton Members

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// EG 20120619 New Take-Up/Give-Up
        /// FI 20161214 [21916] Modify
        /// FI 20170928 [23452]
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = (CciFixTradeCaptureReport.CciFixInstrument.Cci(CciFixInstrument.CciEnum.Exch));
            if (null != cci)
            {
                pPage.SetMarketCountryImage(cci); // FI 20161214 [21916] call SetMarketCountryImage
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.MARKET);
            }

            if (StrFunc.IsFilled(pPage.ActiveElementForced))
            {
                //Si le focus est positionné sur le contrôle Identifiant(Electronic Trade) Spheres® affiche le panel ds lequel il se trouve
                if (pPage.ActiveElementForced == Cst.TXT + this.CciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.ExecID))
                {
                    ShowPanelElectronicTrade(pPage);
                    ShowPanelElectronicOrder(pPage);
                    // FI 20200121 [XXXXX] Appel à ShowPanelStrategy
                    ShowPanelStrategy(pPage);
                }
                //Si le focus est positionné sur le contrôle Identifiant(Electronic Order) Spheres® affiche le panel ds lequel il se trouve
                else if (pPage.ActiveElementForced == Cst.TXT + this.CciFixTradeCaptureReport.CciRptSide.CciClientId(CciRptSide.CciEnum.OrdID))
                {
                    ShowPanelElectronicOrder(pPage);
                    // FI 20200121 [XXXXX] Appel à ShowPanelStrategy
                    ShowPanelStrategy(pPage);
                }
            }


            // EG 20120619 New Take-Up/Give-Up
            // EG 20120705 17988 Add Test IsAllocation
            // FI 20160921 [XXXXX] use Cst.BUT
            string clientId = StrFunc.AppendFormat("{0}{1}{2}", Cst.BUT.ToString(), CciTrade.cciParty[0].Prefix, "TakeUp");
            if (pPage.FindControl(clientId) is WebControl control)
                control.Style[HtmlTextWriterStyle.Display] = (CciTrade.TradeCommonInput.IsAllocation && CciTrade.TradeCommonInput.IsTakeUp(CcisBase.CS, null) ? "block" : "none");

            clientId = StrFunc.AppendFormat("{0}{1}{2}", Cst.BUT.ToString(), CciTrade.cciParty[1].Prefix, "GiveUp");
            control = pPage.FindControl(clientId) as WebControl;
            if (null != control)
                control.Style[HtmlTextWriterStyle.Display] = (CciTrade.TradeCommonInput.IsAllocation && IsExistBookDealer && CciTrade.TradeCommonInput.IsGiveUp(CcisBase.CS, null) ? "block" : "none");

            // FI 20200117 [25167] call _cciRptSide.DumpSpecific_ToGUI
            CciFixTradeCaptureReport.DumpSpecific_ToGUI(pPage);

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion
        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            //
            //Doit tjs être Enabled (cas des strategies)
            Cci(CciEnum.buyer).IsEnabled = true;
            //
            CciFixTradeCaptureReport.SetEnabled(pIsEnabled);
        }

        /// <summary>
        /// Affecte newValue des ccis gérés par ce CciContainer avec string.Empty
        /// </summary>
        public void Clear()
        {
            if (CcisBase.Contains(CciClientId(CciEnum.Category)))
                Cci(CciEnum.Category).NewValue = string.Empty;

            Cci(CciEnum.buyer).NewValue = string.Empty;
            Cci(CciEnum.seller).NewValue = string.Empty;
            //
            if (null != CciFixTradeCaptureReport)
                CciFixTradeCaptureReport.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {
            ExchangeTradedContainer = new ExchangeTradedContainer((IExchangeTradedBase)pProduct, CciTradeCommon.TradeCommonInput.DataDocument);

            IFixTradeCaptureReport fixTradeCaptureReport = null;
            if (null != ExchangeTradedContainer.ExchangeTraded)
                fixTradeCaptureReport = ExchangeTradedContainer.TradeCaptureReport;

            CciFixTradeCaptureReport = new CciFixTradeCaptureReport(CciTrade, this, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fixTradeCaptureReport, fixTradeCaptureReport);

            base.SetProduct(pProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pInitialString"></param>
        /// <returns></returns>
        public override string ReplaceTradeDynamicConstantsWithValues(CustomCaptureInfo pCci, string pInitialString)
        {
            string ret = pInitialString;

            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                // TRD_MARKET
                if (true == ret.Contains(Cst.TRD_MARKET))
                {
                    string market = Product.GetMarket();
                    ret = ret.Replace(Cst.TRD_MARKET, market);
                }
            }

            ret = base.ReplaceTradeDynamicConstantsWithValues(pCci, ret);

            return ret;
        }

        /// <summary>
        /// Affiche Le panel ElectronicTrade
        /// </summary>
        /// <param name="pPage"></param>
        private void ShowPanelElectronicTrade(CciPageBase pPage)
        {
            // FI 20200121 [XXXXX] Appel à ShowLinkControl
            pPage.ShowLinkControl(Cst.IMG + Prefix + "tblTrade");
        }

        /// <summary>
        /// Affiche Le panel ElectronicOrder
        /// </summary>
        /// <param name="pPage"></param>
        private void ShowPanelElectronicOrder(CciPageBase pPage)
        {
            // FI 20200121 [XXXXX] Appel à ShowLinkControl
            pPage.ShowLinkControl(Cst.IMG + Prefix + "tblorder");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200121 [XXXXX] Add Method
        private void ShowPanelStrategy(CciPageBase pPage)
        {
            // FI 20200121 [XXXXX] Appel à ShowLinkControl
            pPage.ShowLinkControl(Cst.IMG + Prefix + "tblStrategy");
        }



        /* FI 20170116 [21916] Mise en commenatire car méthode private non uilisée
        /// <summary>
        /// Retourne true lorsque l'entité du Dealer est membre compensateur de la chambre associée au maché 
        /// </summary>
        /// <returns></returns>
        private bool isDealerEntityClearingMember()
        {
            bool ret = false;
            int idAEntity = BookTools.GetEntityBook(_cciTrade.CSCacheOn, _cciTrade.cciParty[0].GetBookIdB());
            SQL_Market sqlMarket = ExchangeTradedTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, _exchangeTradedContainer.tradeCaptureReport.Instrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
            //
            if (((null != sqlMarket) && (sqlMarket.IdA > 0)) && (idAEntity > 0))
                ret = CaptureTools.IsActorClearingMember(_cciTrade.CSCacheOn, idAEntity, sqlMarket.Id, true);
            //
            return ret;
        }
          */


        #region IsCciExchange
        // EG 20171031 [23509] Upd
        public override bool IsCciExchange(CustomCaptureInfo pCci)
        {
            return CciFixTradeCaptureReport.CciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci);
        }
        #endregion IsCciExchange

        /// <summary>
        /// Reset des Cci suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New
        public override void ResetCciFacilityHasChanged()
        {
            base.ResetCciFacilityHasChanged();
            
            // Reset des Cci Clearer/Custodian
            if (ExchangeTradedContainer.IsOneSide &&
                CciTrade.cciParty[1].Cci(CciTradeParty.CciEnum.actor).IsFilled)
                CciTrade.cciParty[1].ResetCciFacilityHasChanged();

            // Reset des Cci Exchange et asset
            CciFixTradeCaptureReport.CciFixInstrument.ResetCciFacilityHasChanged();
        }
        #endregion
    }
}
