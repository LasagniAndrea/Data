#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using System.Web.UI.WebControls;   

using EFS.ACommon;
using EFS.Actor;
using EFS.Book;
using EFS.Common;
using EFS.Common.Web;
using EFS.EFSTools;
using EFS.GUI.CCI;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.v30.Fix;

using FixML.Interface; 
using FixML.Enum;
using FixML.v50SP1;
using FixML.v50SP1.Enum;

using FpML.Interface;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public abstract class CciProductListedDerivativeBase : CciProductBase, IContainerCci
    {
        #region Members
        private ListedDerivativeContainer _listedDerivative;
        private string _prefix;
        private CciTrade _cciTrade;

        private RoleActor[] _roleActorClearingOrganization;
        private RoleActor[] _roleActorClearer;
        private CciFixTradeCaptureReport _cciFixTradeCaptureReport;
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
            unknown,
        }
        #endregion Enums
        #region Accessors
        /// <summary>
        /// Obtient la collection des ccis
        /// </summary>
        public TradeCustomCaptureInfos ccis
        {
            get { return _cciTrade.ccis; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CciTrade cciTrade
        {
            get { return _cciTrade; }
        }
        /// <summary>
        /// 
        /// </summary>
        public CciFixTradeCaptureReport cciFixTradeCaptureReport
        {
            get { return _cciFixTradeCaptureReport; }
        }
        /// <summary>
        /// 
        /// </summary>
        public ListedDerivativeContainer listedDerivative
        {
            get { return _listedDerivative; }
        }
        #endregion Accessors

        #region Constructors
        public CciProductListedDerivativeBase(CciTrade pCciTrade, IListedDerivative pListedDerivative, string pPrefix)
            : this(pCciTrade, pListedDerivative, pPrefix, -1)
        {
        }
        public CciProductListedDerivativeBase(CciTrade pCciTrade, IListedDerivative pListedDerivative, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pListedDerivative, pPrefix, pNumber)
        {

            _roleActorClearingOrganization = new RoleActor[] { RoleActor.CSS, RoleActor.CLEARINGCOMPART };
            _roleActorClearer = new RoleActor[] { RoleActor.CLEARER };
            //
            _cciTrade = pCciTrade;
            _prefix = prefix;
            //
            SetProduct((IProduct)pListedDerivative);
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
            CciTools.CreateInstance(this, _listedDerivative.ListedDerivative);
            cciFixTradeCaptureReport.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void AddCciSystem()
        {

            // il n'existe pas de buyer/seller sur les allocation
            // ils existent sur les strategies puisque la saisie est un deal  
            if (!ccis.Contains(CciClientId(CciEnum.buyer.ToString())))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string));

            if (!ccis.Contains(CciClientId(CciEnum.seller.ToString())))
                ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string));
            //
            cciFixTradeCaptureReport.AddCciSystem();
            //
            //Sur les stratgies flexibles, le buyer est peut-être optionnel
            //Si c'est le cas tous les ccis rattaché à ce produit deviennent optionnel 
            //Code nécessaire car les ccis généré automatiquement à partir d'un ciiContainerGlobal sont obligatoire lorsque le cciSource est obligatoire
            CustomCaptureInfo cci = ccis[CciClientIdPayer];
            if (false == cci.isMandatory)
            {
                foreach (CustomCaptureInfo cciItem in ccis)
                {
                    if (this.IsCciOfContainer(cciItem.ClientId_WithoutPrefix))
                        cciItem.isMandatory = false;
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
                            IReference buyer = _listedDerivative.buyerPartyReference;
                            if (null != buyer)
                                data = buyer.hRef;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            IReference seller = _listedDerivative.sellerPartyReference;
                            if (null != seller)
                                data = seller.hRef;
                            break;
                        #endregion Seller
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        ccis.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            cciFixTradeCaptureReport.Initialize_FromDocument();
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Dump_ToDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.newValue;
                    bool isSetting = true;
                    IParty party = null;
                    #endregion Reset variables
                    //
                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            party = _cciTrade.DataDocument.GetParty(data, PartyInfoEnum.id);
                            if (null != party)
                            {
                                if (_listedDerivative.isOneSide)
                                {
                                    #region allocation
                                    //
                                    if (_cciTrade.cciParty[0].GetPartyId(true) == party.id)
                                    {
                                        _listedDerivative.SetBuyerSeller(party.id, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                                    }
                                    else
                                    {
                                        if (ActorTools.IsActorWithRole(_cciTrade.CSCacheOn, party.OTCmlId, _roleActorClearingOrganization))
                                        {
                                            _listedDerivative.SetBuyerSeller(party.id, SideEnum.Buy, PartyRoleEnum.ClearingOrganization);
                                        }
                                        else if (ActorTools.IsActorWithRole(_cciTrade.CSCacheOn, party.OTCmlId, _roleActorClearer))
                                        {
                                            _listedDerivative.SetBuyerSeller(party.id, SideEnum.Buy, PartyRoleEnum.ClearingFirm);
                                        }

                                    }
                                    #endregion
                                }
                                else if (_listedDerivative.isTwoSide)
                                {
                                    #region Execution, Intermediation
                                    _listedDerivative.SetBuyerSeller(party.id, SideEnum.Buy, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                                    #endregion
                                }
                            }
                            //
                            if (null != _listedDerivative.buyerPartyReference)
                            {
                                _listedDerivative.buyerPartyReference.hRef = data;
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            party = _cciTrade.DataDocument.GetParty(data, PartyInfoEnum.id);
                            //
                            if (null != party)
                            {
                                if (_listedDerivative.isOneSide)
                                {
                                    #region allocation
                                    if (_cciTrade.cciParty[0].GetPartyId(true) == party.id)
                                    {
                                        _listedDerivative.SetBuyerSeller(party.id, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                                    }
                                    else
                                    {
                                        if (ActorTools.IsActorWithRole(_cciTrade.CSCacheOn, party.OTCmlId, _roleActorClearingOrganization))
                                        {
                                            _listedDerivative.SetBuyerSeller(party.id, SideEnum.Sell, PartyRoleEnum.ClearingOrganization);
                                        }
                                        else if (ActorTools.IsActorWithRole(_cciTrade.CSCacheOn, party.OTCmlId, _roleActorClearer))
                                        {
                                            _listedDerivative.SetBuyerSeller(party.id, SideEnum.Sell, PartyRoleEnum.ClearingFirm);
                                        }
                                    }
                                    #endregion
                                }
                                else if (_listedDerivative.isTwoSide)
                                {
                                    #region execution, intermediation
                                    _listedDerivative.SetBuyerSeller(party.id, SideEnum.Sell, PartyRoleEnum.BuyerSellerReceiverDeliverer);
                                    #endregion execution, intermediation
                                }
                            }
                            //
                            if (null != _listedDerivative.sellerPartyReference)
                            {
                                _listedDerivative.sellerPartyReference.hRef = data;
                            }
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
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
            cciFixTradeCaptureReport.Dump_ToDocument();
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
            if (Cst.Capture.IsModeNewCapture(ccis.CaptureMode) || Cst.Capture.IsModeUpdateGen(ccis.CaptureMode))
                product.SynchronizeListedDerivative();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            string clientId_Key;
            bool isOk = IsCciOfContainer(pCci.ClientId_WithoutPrefix);
            if (isOk)
            {
                #region CciListedDerivative
                CciEnum key = CciEnum.unknown;
                clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region Buyer/Seller
                    case CciEnum.buyer:
                        if (_listedDerivative.isOneSide) //=> mise à jour du side associé à l'unique TrdCapRptSideGrp  
                        {
                            if (_listedDerivative.buyerPartyReference != null)
                            {
                                string clientIdSide = cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_Side);
                                if (_cciTrade.cciParty[0].GetPartyId(true) == _listedDerivative.buyerPartyReference.hRef)
                                    ccis.SetNewValue(clientIdSide, ReflectionTools.EnumValueName(SideEnum.Buy));
                                else
                                    ccis.SetNewValue(clientIdSide, ReflectionTools.EnumValueName(SideEnum.Sell));
                            }
                        }
                        ccis.Synchronize(CciClientIdReceiver, pCci.newValue, pCci.lastValue);
                        //
                        if (StrFunc.IsEmpty(pCci.newValue) && (false == Cci(CciEnum.buyer).isMandatory))
                            Clear();
                        //
                        if (false == Cci(CciEnum.buyer).isMandatory)
                            SetEnabled(pCci.IsFilledValue);
                        //
                        break;
                    case CciEnum.seller:
                        if (_listedDerivative.isOneSide) //=> mise à jour du side associé à l'unique TrdCapRptSideGrp  
                        {
                            if (_listedDerivative.sellerPartyReference != null)
                            {
                                string clientIdSide = cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_Side);
                                if (_cciTrade.cciParty[0].GetPartyId(true) == _listedDerivative.sellerPartyReference.hRef)
                                    ccis.SetNewValue(clientIdSide, ReflectionTools.EnumValueName(SideEnum.Sell));
                                else
                                    ccis.SetNewValue(clientIdSide, ReflectionTools.EnumValueName(SideEnum.Buy));
                            }
                        }
                        ccis.Synchronize(CciClientIdPayer, pCci.newValue, pCci.lastValue);
                        break;
                    #endregion Buyer/Seller
                    #region default
                    default:
                        isOk = false;
                        break;
                    #endregion default
                }
                #endregion CciListedDerivative
            }
            //
            cciFixTradeCaptureReport.ProcessInitialize(pCci);
            //
            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(_cciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    #region pré proposition de la gestion de position à partir du book
                    //Ne fonctionne que pour allocations, seuls écrans qui affichent la gestion de position 
                    if (cciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
                    {
                        string clientId = cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_PosEfct);
                        ccis.Set(clientId, "isEnabled", true);
                        if (null != cciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book).sql_Table)
                        {
                            SQL_Book sqlBook = (SQL_Book)(cciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book).sql_Table);
                            if (!_listedDerivative.category.HasValue || _listedDerivative.category.Value == CfiCodeCategoryEnum.Future)
                            {
                                if (StrFunc.IsFilled(sqlBook.FuturesPosEffect))
                                    ccis.SetNewValue(clientId, sqlBook.FuturesPosEffect);
                                ccis.Set(clientId, "isEnabled", sqlBook.IsUpdatableFuturesPosEffect);
                            }
                            else if (_listedDerivative.category == CfiCodeCategoryEnum.Option)
                            {
                                if (StrFunc.IsFilled(sqlBook.OptionsPosEffect))
                                    ccis.SetNewValue(clientId, sqlBook.OptionsPosEffect);
                                ccis.Set(clientId, "isEnabled", sqlBook.IsUpdatableOptionsPosEffect);
                            }
                        }
                    }
                    #endregion

                    if (_listedDerivative.isOneSide)
                    {
                        #region Préproposition de la contrepartie avec la chambre si l'entité est Clearing Member de la chambre associée au marché
                        //Lorsque le book ou le marché sont renseignés et que la zone chambre est vide
                        if ((_cciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) ||
                            _cciFixTradeCaptureReport.cciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci))
                            &&
                            (_cciTrade.cciParty[1].Cci(CciTradeParty.CciEnum.actor).IsEmpty)
                            )
                        {
                            int idAEntity = BookTools.GetEntityBook(_cciTrade.CSCacheOn, _cciTrade.cciParty[0].GetBookIdB());
                            SQL_Market sqlMarket = ListedDerivativeTools.LoadSqlMarketFromFixInstrument(_cciTrade.CSCacheOn, _listedDerivative.tradeCaptureReport.Instrument, SQL_Table.ScanDataDtEnabledEnum.Yes);
                            //
                            if (((null != sqlMarket) && (sqlMarket.IdA > 0)) && (idAEntity > 0))
                            {
                                bool isActorClearing = CaptureTools.IsActorClearingMember(_cciTrade.CSCacheOn, idAEntity, sqlMarket.Id, true);
                                if (isActorClearing)
                                {
                                    SQL_Actor sqlActor = new SQL_Actor(_cciTrade.CSCacheOn, sqlMarket.IdA, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, ccis.SessionId, ccis.IsSessionAdmin);
                                    if (sqlActor.LoadTable(new string[] { "IDENTIFIER" }))
                                    {
                                        string clientId = _cciTrade.cciParty[1].CciClientId(CciTradeParty.CciEnum.actor);
                                        ccis.SetNewValue(clientId, sqlActor.Identifier, false);
                                    }
                                }
                            }
                        }
                        #endregion
                        //
                        #region préproposition de la contrepartie en fonction du ClearingTemplate
                        if (_cciTrade.cciParty[1].isInitFromClearingTemplate)
                        {
                            if (
                                _cciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) ||
                                _cciFixTradeCaptureReport.cciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci) ||
                                _cciFixTradeCaptureReport.cciFixInstrument.IsCci(CciFixInstrument.CciEnum.Sym, pCci)
                                )
                                _cciTrade.SetCciClearerOrBrokerFromClearingTemplate();
                        }
                        #endregion
                    }
                }
                //
                if (_cciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) ||
                            _cciFixTradeCaptureReport.cciFixInstrument.IsCci(CciFixInstrument.CciEnum.Exch, pCci))
                {
                    int idAEntity = _cciTrade.DataDocument.GetFirstEntity(_cciTrade.CSCacheOn);
                    if ((idAEntity) > 0)
                    {
                        DateTime dt = Tools.GetDateBusiness(_cciTrade.CS, _cciTrade.DataDocument);
                        if (DtFunc.IsDateTimeFilled(dt))
                        {
                            cciFixTradeCaptureReport.Cci(CciFixTradeCaptureReport.CciEnum.BizDt).newValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                            _cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDate).newValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                        }
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
            cciFixTradeCaptureReport.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            cciFixTradeCaptureReport.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_Document()
        {
            if (Cst.Capture.IsModeNew(ccis.CaptureMode) && (false == ccis.IsPreserveData))
            {
                string id = string.Empty;
                cciFixTradeCaptureReport.Initialize_Document();
            }
            //
            if (Cst.Capture.IsModeInput(ccis.CaptureMode))
                InitializeTrdCapRptSideGrp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            cciFixTradeCaptureReport.RefreshCciEnabled();
        }

        #endregion IContainerCciFactory Members

        #region IContainerCci Members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Cci(pEnumValue.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion

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
            ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
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
            cciFixTradeCaptureReport.SetButtonReferential(pCci, pCo);
        }

        #endregion IContainerCciGetInfoButton Members

        #region ICciPresentation Membres
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cci = (cciFixTradeCaptureReport.cciFixInstrument.Cci(CciFixInstrument.CciEnum.Exch));
            if ((null != cci) && (null != cci.sql_Table))
            {
                WCToolTipImage img = pPage.FindControl(Cst.IMG + cci.ClientId_WithoutPrefix) as WCToolTipImage;
                if (null != img)
                {
                    SQL_Market sqlMarket = (SQL_Market)cci.sql_Table;
                    //
                    img.Style.Remove("display");
                    img.Pty.TooltipContent = sqlMarket.IdCountry;
                    img.ImageUrl = @"~/Images/PNG/Flags/" + sqlMarket.IdCountry + ".png"; // 
                }
            }
            //
            cci = cciFixTradeCaptureReport.cciFixInstrument.Cci(CciFixInstrument.CciEnum.Exch);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.MARKET);
            //
            if (StrFunc.IsFilled(pPage.ActiveElementForced))
            {
                //Si le focus est positionné sur le contrôle Identifiant(Electronic Trade) Spheres® affiche le panel ds lequel il se trouve
                if (pPage.ActiveElementForced == Cst.TXT + this.cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.ExecID))
                {
                    ShowPanelElectronicTrade(pPage);
                    ShowPanelElectronicOrder(pPage);
                }
                //Si le focus est positionné sur le contrôle Identifiant(Electronic Order) Spheres® affiche le panel ds lequel il se trouve
                else if (pPage.ActiveElementForced == Cst.TXT + this.cciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.RptSide_OrdID))
                {
                    ShowPanelElectronicOrder(pPage);
                }
            }
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
            CciTools.SetCciContainer(this, "isEnabled", pIsEnabled);
            //
            //Doit tjs être Enabled (cas des strategies)
            Cci(CciEnum.buyer).isEnabled = true;
            //
            cciFixTradeCaptureReport.SetEnabled(pIsEnabled);
        }

        /// <summary>
        /// Affecte newValue des ccis gérés par ce CciContainer avec string.Empty
        /// </summary>
        public void Clear()
        {
            if (ccis.Contains(CciClientId(CciEnum.Category)))
                Cci(CciEnum.Category).newValue = string.Empty;

            Cci(CciEnum.buyer).newValue = string.Empty;
            Cci(CciEnum.seller).newValue = string.Empty;
            //
            if (null != _cciFixTradeCaptureReport)
                _cciFixTradeCaptureReport.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {
            _listedDerivative = new ListedDerivativeContainer((IListedDerivative)pProduct);
            //
            IFixTradeCaptureReport fixTradeCaptureReport = null;
            if (null != _listedDerivative.ListedDerivative)
                fixTradeCaptureReport = _listedDerivative.tradeCaptureReport;
            _cciFixTradeCaptureReport = new CciFixTradeCaptureReport(_cciTrade, this, prefix + TradeCustomCaptureInfos.CCst.Prefix_fixTradeCaptureReport, fixTradeCaptureReport);
            //
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
            //
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                // TRD_MARKET
                if (true == ret.Contains(Cst.TRD_MARKET))
                {
                    string market = product.GetMarket();
                    ret = ret.Replace(Cst.TRD_MARKET, market);
                }
            }
            //
            ret = base.ReplaceTradeDynamicConstantsWithValues(pCci, ret);
            //
            return ret;

        }

        /// <summary>
        /// Initialisation de trdCapRptSideGrp
        /// <para>Mise en place des dispositions pour un fonctionnement correct de la saisie light</para>
        /// <para>Mise en place de 2 TrdCapRptSideGrp si Execution ou intermediation</para>
        /// </summary>
        /// <param name="pType"></param>
        private void InitializeTrdCapRptSideGrp()
        {
            IFixTrdCapRptSideGrp[] trdCapRptSideGrp = _listedDerivative.tradeCaptureReport.TrdCapRptSideGrp;
            //
            if (cciTradeCommon.TradeCommonInput.IsETDandAllocation)
            {
                //Si allocation il faut au minimum 1 TrdCapRptSideGrp
                bool isOk = (ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp) >= 1);
                if (false == isOk)
                {
                    int newPos = ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp);
                    ReflectionTools.AddItemInArray(_listedDerivative.tradeCaptureReport, "RptSide", newPos);
                }
                //
                //Spheres conserve le 1er TrdCapRptSideGrp => Tous les autres sont supprimés
                if (ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp) > 1)
                {
                    for (int i = ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp) - 1; i > 0; i--)
                    {
                        if (ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp[i].Parties) > 0)
                        {
                            for (int j = 0; j < ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp[i].Parties); j++)
                            {
                                IFixParty fixParty = _listedDerivative.tradeCaptureReport.TrdCapRptSideGrp[i].Parties[j];
                                if (fixParty.PartyIdSpecified)
                                    cciTradeCommon.TradeCommonInput.DataDocument.RemoveParty(fixParty.PartyId.href);
                            }
                        }
                        ReflectionTools.RemoveItemInArray(_listedDerivative.tradeCaptureReport, "RptSide", i);
                    }
                }
            }
            else if (cciTradeCommon.TradeCommonInput.IsDeal)
            {
                //Si allocation il faut au minimum 2 TrdCapRptSideGrp (1 de sens Achat, 1 de sens Vente) 
                bool isOk = (ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp) >= 2);
                while (false == isOk)
                {
                    int newPos = ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp);
                    ReflectionTools.AddItemInArray(_listedDerivative.tradeCaptureReport, "RptSide", newPos);
                    isOk = (ArrFunc.Count(_listedDerivative.tradeCaptureReport.TrdCapRptSideGrp) >= 2);
                }
                //
                if (isOk)
                {
                    IFixTrdCapRptSideGrp trdCapRptSide = null;
                    trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(SideEnum.Buy);
                    if (null == trdCapRptSide)
                    {
                        trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(null);
                        trdCapRptSide.Side = SideEnum.Buy;
                        trdCapRptSide.SideSpecified = true;
                    }
                    //
                    trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(SideEnum.Sell);
                    if (null == trdCapRptSide)
                    {
                        trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(null);
                        trdCapRptSide.Side = SideEnum.Sell;
                        trdCapRptSide.SideSpecified = true;
                    }
                    //
                    for (int k = 0; k < 2; k++)
                    {
                        if (0 == k)
                            trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(SideEnum.Buy);
                        else
                            trdCapRptSide = _listedDerivative.GetTrdCapRptSideGrp(SideEnum.Sell);
                        //
                        if (ArrFunc.IsFilled(trdCapRptSide.Parties))
                        {
                            for (int j = ArrFunc.Count(trdCapRptSide.Parties) - 1; j > 0; j--)
                            {
                                if (trdCapRptSide.Parties[j].PartyIdSpecified && trdCapRptSide.Parties[j].PartyRoleSpecified)
                                {
                                    if (trdCapRptSide.Parties[j].PartyRole != PartyRoleEnum.BuyerSellerReceiverDeliverer)
                                    {
                                        cciTradeCommon.TradeCommonInput.DataDocument.RemoveParty(trdCapRptSide.Parties[j].PartyId.href);
                                        ReflectionTools.RemoveItemInArray(trdCapRptSide, "Pty", j);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Affiche Le panel ElectronicTrade
        /// </summary>
        /// <param name="pPage"></param>
        private static void ShowPanelElectronicTrade(CciPageBase pPage)
        {
            WCImageButtonOpenBanner imageBanner = pPage.placeHolder.FindControl(Cst.IMG + "listedDerivative_tblTrade") as WCImageButtonOpenBanner;
            if (null != imageBanner)
            {
                imageBanner.SetImageCollapse();
                imageBanner.SetLinkControlDisplay();
            }
        }
        /// <summary>
        /// Affiche Le panel ElectronicOrder
        /// </summary>
        /// <param name="pPage"></param>
        private static void ShowPanelElectronicOrder(CciPageBase pPage)
        {
            WCImageButtonOpenBanner imageBanner = pPage.placeHolder.FindControl(Cst.IMG + "listedDerivative_tblorder") as WCImageButtonOpenBanner;
            if (null != imageBanner)
            {
                imageBanner.SetImageCollapse();
                imageBanner.SetLinkControlDisplay();
            }
        }
        /// <summary>
        /// Affiche Le panel Caractéristiques Actif
        /// </summary>
        private static void ShowPanelAsset(CciPageBase pPage)
        {
            WCImageButtonOpenBanner imageBanner = pPage.placeHolder.FindControl(Cst.IMG + "exchangeTradedDerivative_FIXML_TrdCapRpt_Instrmt_tblAssetETDDesc") as WCImageButtonOpenBanner;
            if (null != imageBanner)
            {
                imageBanner.SetImageCollapse();
                imageBanner.SetLinkControlDisplay();
            }
        }
        #endregion

    }
}
