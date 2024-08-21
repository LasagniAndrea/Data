using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FixML.Interface;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Linq;
using System.Web.UI;


namespace EFS.TradeInformation
{
    /// <summary>
    ///
    /// </summary>
    public class CciProductCommoditySpot : CciProductBase,  IContainerCciPayerReceiver, IContainerCciFactory
    {

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        /// EG 20171025 [23509] Upd
        /// EG 20171113 [23509] Upd Add FacilityHaschanged
        public enum CciEnum
        {
            /// <summary>
            /// effectiveDate (= terminationDate)
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute("effectiveDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            effectiveDate_adjustableDate_unadjustedDate,

            /// <summary>
            /// termination
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute("termination.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            terminationDate_adjustableDate_unadjustedDate,

            [System.Xml.Serialization.XmlEnumAttribute("settlementCurrency")]
            settlementCurrency,

            /// <summary>
            /// Représente le marché
            /// </summary>
            /// FI 20200116 [25141] Add CciColumnValue attribute
            [CciColumnValue(Column = "SHORT_ACRONYM", IOColumn = "FIXML_SecurityExchange")]
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("gasPhysicalLeg.commodityAsset.exchangeId")]
            gasPhysicalLeg_commodityAsset_exchangeId,

            /// <summary>
            /// Représente le marché
            /// </summary>
            /// FI 20200116 [25141] Add CciColumnValue attribute
            [CciColumnValue(Column = "SHORT_ACRONYM", IOColumn = "FIXML_SecurityExchange")]
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("electricityPhysicalLeg.commodityAsset.exchangeId")]
            electricityPhysicalLeg_commodityAsset_exchangeId,

            /// <summary>
            /// Représente le marché
            /// </summary>
            // EG 20221201 [25639] [WI484] New
            [CciColumnValue(Column = "SHORT_ACRONYM", IOColumn = "FIXML_SecurityExchange")]
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("environmentalPhysicalLeg.commodityAsset.exchangeId")]
            environmentalPhysicalLeg_commodityAsset_exchangeId,
            //exch,

            /// <summary>
            /// 
            /// </summary>
            /// FI 20170116 [21916] add 
            /// => pour simplifier l'importation
            /// - evite l'usage des cci payer/receiver dans physicalLeg et fixedLeg
            /// - mapping partager pour les ETD, ESE, COMS
            buyer,

            /// <summary>
            /// 
            /// </summary>
            /// => pour simplifier l'importation
            /// - evite l'usage des cci payer/receiver dans physicalLeg et fixedLeg
            /// - mapping partager pour les ETD, ESE, COMS
            seller,

            /// <summary>
            /// 
            /// </summary>
            /// FI 20170116 [21916] add
            /// (harmonisation des produits contenant un RptSide)
            /// => Utiliser par l'importation des trades ALLOC  
            RptSide_Side,

            unknown
        }
        #region Members

        
        private CommoditySpotContainer _commoditySpotContainer;
        private ICommoditySpot _commoditySpot;

        private CciFixedPriceSpotLeg _cciFixedPriceSpotLeg;
        private CciRptSide _cciRptSide;

        #endregion

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get
            {
                return base.CcisBase as TradeCustomCaptureInfos;
            }
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
        public CCiPhysicalSwapLeg CciPhysicalSwapLeg
        {
            get; 
            private set;
        }

        /// <summary>
        /// Enum qui permet d'accéder au cci marché
        /// </summary>
        /// EG 20171025 [23509] New
        // EG 20221201 [25639] [WI484] Upd
        public CciEnum ExchangeEnumValue
        {
            get
            {
                CciEnum ret = CciEnum.unknown;
                if (CciPhysicalSwapLeg is CCiGasPhysicalLeg)
                    ret = CciEnum.gasPhysicalLeg_commodityAsset_exchangeId;
                else if (CciPhysicalSwapLeg is CCiElectricityPhysicalLeg)
                    ret = CciEnum.electricityPhysicalLeg_commodityAsset_exchangeId;
                else if (CciPhysicalSwapLeg is CciEnvironmentalPhysicalLeg)
                    ret = CciEnum.environmentalPhysicalLeg_commodityAsset_exchangeId;
                return ret;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        /// EG 20171031 [23509] New
        public override bool IsCciExchange(CustomCaptureInfo pCci)
        {
            return IsCci<CciEnum>(ExchangeEnumValue, pCci);
        }
        /// <summary>
        /// Obtient le cci qui représente le marché 
        /// </summary>
        /// FI 20171222 [XXXXX] Add
        public override CustomCaptureInfo CciExchange
        {
            get
            {
                return Cci(ExchangeEnumValue);
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
                return CciTools.GetColumn<CciEnum>(ExchangeEnumValue, CcisBase.IsModeIO);
            }
        }

        #endregion accessor

        #region constructor
        /// <summary>
        /// Constructor CommoditySpot est le seul produit du trade
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pCommoditySpot"></param>
        /// <param name="pPrefix"></param>
        public CciProductCommoditySpot(CciTrade pCciTrade, ICommoditySpot pCommoditySpot, string pPrefix)
            : this(pCciTrade, pCommoditySpot, pPrefix, -1)
        { 
        }
        /// <summary>
        /// Constructor si le CommoditySpot rentre dans une strategy
        /// </summary>
        /// <param name="pCciTrade"></param>
        /// <param name="pCommoditySpot"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pNumber"></param>
        public CciProductCommoditySpot(CciTrade pCciTrade, ICommoditySpot pCommoditySpot, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pCommoditySpot, pPrefix, pNumber)
        {
        }
        #endregion constructor

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        // EG 20171004 [22374][23452] Upd
        // EG 20171025 [23509] Upd
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            //Autopostback
            //Control control = pPage.GetCciControl(_cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.timeStamping).ClientId);
            //WCTextBox2 txt = control as WCTextBox2;
            //if (null == txt)
            //    throw new NullReferenceException("Control timeStamping doesn't exist");
            //txt.AutoPostBack = true;

            //control = pPage.GetCciControl(_cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDate).ClientId);
            //txt = control as WCTextBox2;
            //if (null == txt)
            //    throw new NullReferenceException("Control tradeDate doesn't exist");
            //txt.AutoPostBack = true;

            Control control = pPage.GetCciControl(CciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.executionDateTime).ClientId);
            if (!(control is WCZonedDateTime txt))
                throw new NullReferenceException("Control tradeDateTime doesn't exist");
            txt.AutoPostBack = true;

            if (StrFunc.IsFilled(pPage.ActiveElementForced))
            {
                //Si le focus est positionné sur le contrôle Identifiant(Electronic Trade) Spheres® affiche le panel ds lequel il se trouve
                if (pPage.ActiveElementForced == Cst.TXT + this._cciRptSide.CciClientId(CciRptSide.CciEnum.ExecRefID))
                {
                    //ShowPanelElectronicTrade
                    pPage.ShowLinkControl(Cst.IMG + Prefix + "tblTrade");
                    //ShowPanelElectronicOrder(pPage);
                    pPage.ShowLinkControl(Cst.IMG + Prefix + "tblorder");
                }
                else if (pPage.ActiveElementForced == Cst.TXT + this._cciRptSide.CciClientId(CciRptSide.CciEnum.OrdID))
                {
                    //Si le focus est positionné sur le contrôle Identifiant(Electronic Order) Spheres® affiche le panel ds lequel il se trouve
                    pPage.ShowLinkControl(Cst.IMG + Prefix + "tblorder");
                }
            }

            //_cciFixedPriceSpotLeg.DumpSpecific_ToGUI();

            if (null != CciPhysicalSwapLeg)
                CciPhysicalSwapLeg.DumpSpecific_ToGUI(pPage);

            CustomCaptureInfo cci = Cci(ExchangeEnumValue);
            if (null != cci)
            {
                pPage.SetMarketCountryImage(cci);
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.MARKET);
            }

            // FI 20200120 [25167] Call DisplayExecRefID
            DisplayExecRefID(pPage);

            // FI 20200117 [25167] call _cciRptSide.DumpSpecific_ToGUI
            _cciRptSide.DumpSpecific_ToGUI(pPage);

            base.DumpSpecific_ToGUI(pPage);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override string GetData(string pKey, CustomCaptureInfo pCci)
        {
            string ret = string.Empty;

            if (StrFunc.IsEmpty(pKey) && IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string cliendId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum enumKey = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                    enumKey = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                switch (enumKey)
                {
                    case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                        pKey = "T";
                        break;
                }

                if (StrFunc.IsEmpty(pKey))
                    pKey = "E";
            }

            if (StrFunc.IsFilled(pKey))
            {
                switch (pKey.ToUpper())
                {
                    case "E":
                        ret = Cci(CciEnum.effectiveDate_adjustableDate_unadjustedDate).NewValue;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsCci_Issuer(CustomCaptureInfo pCci)
        {
            return base.IsCci_Issuer(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        // EG 20221201 [25639] [WI484] Upd Gestion des contrats en fonction du type
        public override void SetButtonReferential(CustomCaptureInfo pCci, Common.Web.CustomObjectButtonReferential pCo)
        {
            if (CciPhysicalSwapLeg.IsCci(CCiPhysicalSwapLeg.CciEnum.commodityAsset, pCci))
            {
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Referential = "ASSET_COMMODITYCONTRACT";
                pCo.Title = "OTC_REF_DATA_UNDERASSET_COMMODITYCONTRACT";
                pCo.DynamicArgument = null;
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.SqlColumn = "IDENTIFIER";
                pCo.Condition = "TRADE_INPUT";
                pCo.Fk = null;

                StringDynamicData market = new StringDynamicData(TypeData.TypeDataEnum.@int.ToString(), "IDM", _commoditySpot.IdM.ToString());
                StringDynamicData commodityClass = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "COMMODITYCLASS", CciPhysicalSwapLeg.CommodityContractClass.ToString());
                StringDynamicData commodityType = new StringDynamicData(TypeData.TypeDataEnum.@string.ToString(), "COMMODITYTYPE", CciPhysicalSwapLeg.CommodityProductType);

                bool useCommodityProductType = (CciPhysicalSwapLeg.CommodityProductType != "Undefined");

                if ((_commoditySpot.IdM > 0) && useCommodityProductType)
                {
                    pCo.Condition = "TRADE_INPUT3";
                    pCo.DynamicArgument = new string[3] { market.Serialize(), commodityClass.Serialize(), commodityType.Serialize() };
                }
                else if ((_commoditySpot.IdM > 0))
                {
                    pCo.Condition = "TRADE_INPUT";
                    pCo.DynamicArgument = new string[2] { market.Serialize(), commodityClass.Serialize()};
                }
                else if (useCommodityProductType)
                {
                    pCo.Condition = "TRADE_INPUT4";
                    pCo.DynamicArgument = new string[2] { commodityClass.Serialize(), commodityType.Serialize() };
                }
                else
                {
                    pCo.Condition = "TRADE_INPUT2";
                    pCo.DynamicArgument = new string[1] { commodityClass.Serialize()};
                }
            }
            else if (this.IsCciExchange(pCci))
            {
                // FI 2020016 [25141] Mise à jour de Param en fonction du facility saisi
                CustomCaptureInfo cciFacility = this.CciTrade.CciFacilityParty.Cci(CciMarketParty.CciEnum.identifier);
                if (null != cciFacility.Sql_Table)
                {
                    pCo.Param = new string[] { ((SQL_Market)cciFacility.Sql_Table).XmlId };
                    pCo.Condition = "COM_MARKET_FACILITY";
                }
            }

            base.SetButtonReferential(pCci, pCo);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        // EG 20221201 [25639] [WI484] Upd
        public override void SetProduct(IProduct pProduct)
        {
            
            _commoditySpot = (ICommoditySpot)pProduct;
            _commoditySpotContainer = new CommoditySpotContainer(_commoditySpot, CciTradeCommon.TradeCommonInput.DataDocument);

            //fixedLeg
            _cciFixedPriceSpotLeg = new CciFixedPriceSpotLeg(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fixedLeg, _commoditySpot.FixedLeg);

            _cciRptSide = new CciRptSide(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_RptSide, _commoditySpot.RptSide);

            //commodityPhysicalLeg
            if (_commoditySpot.IsGas)
                CciPhysicalSwapLeg = new CCiGasPhysicalLeg(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_gasPhysicalLeg, (IGasPhysicalLeg)_commoditySpot.PhysicalLeg);
            else if (_commoditySpot.IsElectricity)
                CciPhysicalSwapLeg = new CCiElectricityPhysicalLeg(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_electricityPhysicalLeg, (IElectricityPhysicalLeg)_commoditySpot.PhysicalLeg);
            else if (_commoditySpot.IsEnvironmental)
                CciPhysicalSwapLeg = new CciEnvironmentalPhysicalLeg(CciTrade, Prefix + TradeCustomCaptureInfos.CCst.Prefix_environmentalPhysicalLeg, (IEnvironmentalPhysicalLeg)_commoditySpot.PhysicalLeg);
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", _commoditySpot.PhysicalLeg.ToString()));

            base.SetProduct(pProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsObjSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, Common.Web.CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return base.SetButtonScreenBox(pCci, pCo, ref pIsObjSpecified, ref pIsEnabled);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, Common.Web.CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = base.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            if (false == isOk)
                isOk = _cciFixedPriceSpotLeg.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;
        }

        #region Membres de ITradeCci
        /// <summary>
        /// Retourne BUY (Le payer du produit achète)
        /// </summary>
        public override string RetSidePayer
        {
            get { return SideTools.RetBuySide(); }
        }

        /// <summary>
        /// Retourne SELL (Le receiver du produit vend)
        /// </summary>
        public override string RetSideReceiver
        {
            get { return SideTools.RetSellSide(); }
        }

        /// <summary>
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                return _commoditySpot.SettlementCurrency.Value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override string CciClientIdMainCurrency
        {
            get { return CciClientId(CciEnum.settlementCurrency); }
        }

        #endregion  Membres de ITradeCci

        #region Membres de IContainerCciFactory

        /// <summary>
        /// 
        /// </summary>
        // FI 20170116 [21916] Modify
        // EG 20171025 [23509] Upd
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
                    bool isFilled = StrFunc.IsFilled(data);
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    
                    switch (cciEnum)
                    {
                        case CciEnum.buyer:
                            _commoditySpot.BuyerPartyReference.HRef = cci.NewValue;
                            RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.seller:
                            _commoditySpot.SellerPartyReference.HRef = cci.NewValue;
                            RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.RptSide_Side:
                            // FI 20170116 [21916]  (harmonisation des produits contenant un RptSide)
                            if (_commoditySpotContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = _commoditySpotContainer.RptSide[0];
                                _rptSide.SideSpecified = StrFunc.IsFilled(data);
                                if (_rptSide.SideSpecified)
                                {
                                    SideEnum sideEnum = (SideEnum)ReflectionTools.EnumParse(_rptSide.Side, data);
                                    _rptSide.Side = sideEnum;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                            _commoditySpot.EffectiveDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                            if (_commoditySpot.EffectiveDate.AdjustableDateSpecified)
                                _commoditySpot.EffectiveDate.AdjustableDate.UnadjustedDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High; // Por calculer Date de règlement
                            break;
                        case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                            _commoditySpot.TerminationDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                            if (_commoditySpot.TerminationDate.AdjustableDateSpecified)
                                _commoditySpot.TerminationDate.AdjustableDate.UnadjustedDate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High; // Por calculer la qté Totale
                            break;
                        case CciEnum.settlementCurrency:
                            _commoditySpot.SettlementCurrency.Value = data;
                            break;
                        //case CciEnum.exch:
                        //    DumpExchange_ToDocument(cci, data);
                        //    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                        //    break;
                        default:
                            if (cciEnum == ExchangeEnumValue)
                            {
                                DumpExchange_ToDocument(cci, data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                break;
                            }
                            else
                            {
                                isSetting = false;
                            }
                            break;
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            _cciFixedPriceSpotLeg.Dump_ToDocument();

            CciPhysicalSwapLeg.Dump_ToDocument();

            _cciRptSide.Dump_ToDocument();

            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
            {
                /* FI 20170116 [21916] mise en commentaire du fait de l'existence d'un buyer/seller
                if (ccis.IsInQueue(CciClientIdPayer))
                    this.RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);

                if (ccis.IsInQueue(CciClientIdReceiver))
                    this.RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);
                */
                Product.SynchronizeFromDataDocument();
            }

        }

        // EG 20171025 [23509] New
        private void DumpExchange_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;
            if (StrFunc.IsFilled(pData))
            {

                // FI 20200115 [25141] Appel CciTools.GetColumn pour obtenir la colonne qui alimente le cci
                SQL_TableWithID.IDType IDTypeSearch = CciTools.ParseColumn(CciTools.GetColumn<CciEnum>(ExchangeEnumValue, CcisBase.IsModeIO));

                // FI 20200115 [25141] Appel à la méthode CciTools.IsMarketValid
                CciTools.IsMarketValid(CciTradeCommon.CSCacheOn, pData, IDTypeSearch,
                    out SQL_Market sqlMarket, out bool isLoaded, out bool isFound, CcisBase.User, CcisBase.SessionId);

                if (isFound)
                {
                    pCci.Sql_Table = sqlMarket;
                    // FI 20200115 [XXXXX] Utilisation de CciExchColumn
                    pCci.NewValue = (sqlMarket.GetFirstRowColumnValue(CciExchangeColumn)) as string;

                    _commoditySpot.IdM = sqlMarket.Id;

                    //Ajout de la party dans le DataDocument
                    int idClearingHouse = sqlMarket.IdA;
                    if (idClearingHouse > 0)
                    {
                        SQL_Actor sqlClearingHouse = new SQL_Actor(CciTradeCommon.CSCacheOn, idClearingHouse);
                        if (sqlClearingHouse.IsLoaded)
                            CciTrade.DataDocument.AddParty(sqlClearingHouse);
                    }

                    //sauvegarder le dernier marché utilisé
                    if (StrFunc.IsFilled(pData))
                        Ccis.TradeCommonInput.SetDefault(CommonInput.DefaultEnum.market, pCci.NewValue);

                }
                else
                {
                    if (isLoaded)
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotUnique"), pData);
                    else
                        pCci.ErrorMsg = CciTools.BuildCciErrMsg(Ressource.GetString("Msg_MarketNotFound"), pData);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        /// FI 20170214 [21916] Modify
        /// EG 20171016 [23509] Upd
        public override void AddCciSystem()
        {
            // FI 20170116 [21916] add businessDate en tant que cci System
            // Cette donnée spécifique à Spheres® et sans équivalence sur les trades issus de ECC 
            // Ajouté ici en cci System pour que sa détermination soit gérée par Spheres® lors de l'importation des trades 
            //string cliendIdBizDt = _cciTrade.cciTradeHeader.CciClientId(CciTradeHeader.CciEnum.businessDate.ToString());
            //CciTools.AddCciSystem(ccis, Cst.TXT + cliendIdBizDt, true, TypeData.TypeDataEnum.datetime);

            //FI 20180301 [23814] Mise en commentaire => businessDate n'est pas un ccisystem
            //string cliendIdBizDt = _cciTrade.cciTradeHeader.CciClientId(CciTradeHeader.CciEnum.clearedDate.ToString());
            //CciTools.AddCciSystem(ccis, Cst.TXT + cliendIdBizDt, true, TypeData.TypeDataEnum.datetime);

            // FI 20170116 [21916]
            // Ajout des buyer/seller
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            // FI 20170116 [21916]  (harmonisation des produits contenant un RptSide)
            // Pour l'importation des allocations
            if (_commoditySpotContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            CciTools.AddCciSystem(CcisBase, Cst.TXT + CciClientId(CciEnum.settlementCurrency), true, TypeData.TypeDataEnum.@string);
            //FI 20170214 [21916] le marché est obligatoire
            //CciTools.AddCciSystem(ccis, Cst.DDL + CciClientId(CciEnum.exch), true, TypeData.TypeDataEnum.@string);

            _cciFixedPriceSpotLeg.AddCciSystem();
            CciPhysicalSwapLeg.AddCciSystem();
            _cciRptSide.AddCciSystem();

            base.AddCciSystem();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _commoditySpot);

            _cciFixedPriceSpotLeg.Initialize_FromCci();
            CciPhysicalSwapLeg.Initialize_FromCci();
            _cciRptSide.Initialize_FromCci();

            base.Initialize_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.buyer:
                            data = _commoditySpot.BuyerPartyReference.HRef;
                            break;
                        case CciEnum.seller:
                            data = _commoditySpot.SellerPartyReference.HRef;
                            break;
                        case CciEnum.RptSide_Side:
                            // FI 20170116 [21916]  (harmonisation des produits contenant un RptSide)
                            IFixTrdCapRptSideGrp _rptSide = _commoditySpotContainer.RptSide[0];
                            if (_rptSide.SideSpecified)
                                data = ReflectionTools.ConvertEnumToString<SideEnum>(_rptSide.Side);
                            break;
                        case CciEnum.effectiveDate_adjustableDate_unadjustedDate:
                            if (_commoditySpot.EffectiveDate.AdjustableDateSpecified)
                                data = _commoditySpot.EffectiveDate.AdjustableDate.UnadjustedDate.Value;
                            break;
                        case CciEnum.terminationDate_adjustableDate_unadjustedDate:
                            if (_commoditySpot.TerminationDate.AdjustableDateSpecified)
                                data = _commoditySpot.TerminationDate.AdjustableDate.UnadjustedDate.Value;
                            break;
                        case CciEnum.settlementCurrency:
                            data = _commoditySpot.SettlementCurrency.Value;
                            break;
                        default:
                            if (cciEnum == ExchangeEnumValue)
                            {
                                CommoditySpotContainer commoditySpot = new CommoditySpotContainer(_commoditySpot, CciTrade.TradeCommonInput.DataDocument);
                                // data contient le FIXML_SecurityExchange du marché
                                data = commoditySpot.GetMarket(CciTrade.CSCacheOn, null, out SQL_Market sqlMarket);

                                // FI 2020015 [XXXXX] consédération de CciExchangeColumn pour alimenter le cci
                                if (StrFunc.IsFilled(data) && CciExchangeColumn != "FIXML_SecurityExchange")
                                    data = sqlMarket.GetFirstRowColumnValue(CciExchangeColumn) as string;

                                sql_Table = sqlMarket;
                            }
                            else
                            {
                                isSetting = false;
                            }
                            break;
                    }

                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }

            _cciFixedPriceSpotLeg.Initialize_FromDocument();

            //if (null != _cciGasPhysicalLeg)
            //    _cciGasPhysicalLeg.Initialize_FromDocument();

            //if (null != _cciElectricityPhysicalLeg)
            //    _cciElectricityPhysicalLeg.Initialize_FromDocument();

            CciPhysicalSwapLeg.Initialize_FromDocument();

            _cciRptSide.Initialize_FromDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20171004 [22374][23452] Upd
        // EG 20171016 [23509] Upd
        // EG 20171025 [23509] Upd
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {

                    case CciEnum.buyer:
                        CcisBase.SetNewValue(_cciFixedPriceSpotLeg.CciClientId(CciFixedPriceSpotLeg.CciEnum.payer), pCci.NewValue);
                        CcisBase.SetNewValue(CciPhysicalSwapLeg.CciClientId(CCiPhysicalSwapLeg.CciEnum.receiver), pCci.NewValue);

                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.seller:
                        CcisBase.SetNewValue(_cciFixedPriceSpotLeg.CciClientId(CciFixedPriceSpotLeg.CciEnum.receiver), pCci.NewValue);
                        CcisBase.SetNewValue(CciPhysicalSwapLeg.CciClientId(CCiPhysicalSwapLeg.CciEnum.payer), pCci.NewValue);

                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.RptSide_Side:
                        // FI 20170116 [21916]  (harmonisation des produits contenant un RptSide)
                        if (_commoditySpotContainer.IsOneSide)
                        {
                            IFixTrdCapRptSideGrp _rptSide = _commoditySpotContainer.RptSide[0];
                            if (_rptSide.SideSpecified)
                            {
                                string clientId = string.Empty;
                                if (_rptSide.Side == SideEnum.Buy)
                                    clientId = CciClientIdPayer;
                                else if (_rptSide.Side == SideEnum.Sell)
                                    clientId = CciClientIdReceiver;
                                if (StrFunc.IsFilled(clientId))
                                    CcisBase.SetNewValue(clientId, CciTrade.cciParty[0].GetPartyId(true));
                            }
                        }
                        break;

                    default:
                        if (key == ExchangeEnumValue)
                        {
                            //lastValue est valorisé à * pour provoquer le dump sur le CciEnum.commodityAsset(l'asset sera alors non valide)
                            CcisBase.Set(CciPhysicalSwapLeg.CciClientId(CCiPhysicalSwapLeg.CciEnum.commodityAsset), "lastValue", "*");
                        }
                        break;
                }
            }

            _cciFixedPriceSpotLeg.ProcessInitialize(pCci);
            CciPhysicalSwapLeg.ProcessInitialize(pCci);
            _cciRptSide.ProcessInitialize(pCci);

            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(_commoditySpot.RptSide) == 1)
                {
                    #region Préproposition de la contrepartie avec la chambre si l'entité est Clearing Member de la chambre associée au marché
                    //Lorsque le book ou le marché sont renseignés et que la zone chambre est vide => pré-proposition de la chambre
                    if ((CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) || IsCci(ExchangeEnumValue, pCci))
                            && (CciTrade.cciParty[1].Cci(CciTradeParty.CciEnum.actor).IsEmpty))
                    {
                        SetCssByEntityClearingMember();
                    }
                    #endregion

                    #region préproposition de la contrepartie en fonction du ClearingTemplate
                    if (CciTrade.cciParty[1].IsInitFromClearingTemplate)
                    {
                        if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci) || IsCci(ExchangeEnumValue, pCci))
                            CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                    }
                    #endregion


                    // EG 20171031 [23509] Upd
                    CustomCaptureInfo cciToInitializeDates = pCci;
                    if (IsCci(CciEnum.buyer, pCci) || IsCci(CciEnum.seller, pCci))
                        cciToInitializeDates = CciTrade.cciParty[0].Cci(CciTradeParty.CciEnum.book);
                    if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, cciToInitializeDates) ||
                        IsCci(ExchangeEnumValue, cciToInitializeDates))
                    {
                        InitializeDates(cciToInitializeDates);
                    }

                    if (CciTrade.cciMarket[0].IsCci(CciMarketParty.CciEnum.executionDateTime, pCci) || IsCci(ExchangeEnumValue, pCci))
                    {
                        // EG 20171031 [23509] Upd
                        //InitializeBusinessDateFromExecution();
                        InitializeDelivryStartEnd();
                    }

                    /* FI 20180301 [23814] Utilisation de clearedDate sur _cciTrade.cciMarket[0]
                    if (_cciTrade.cciTradeHeader.IsCci(CciTradeHeader.CciEnum.clearedDate, pCci) ||
                        IsCci(CciEnum.effectiveDate_adjustableDate_unadjustedDate, pCci))
                    {
                        InitializePaymentDate();
                    }*/

                    if (CciTrade.cciMarket[0].IsCci(CciMarketParty.CciEnum.clearedDate, pCci) ||
                        IsCci(CciEnum.effectiveDate_adjustableDate_unadjustedDate, pCci))
                    {
                        InitializePaymentDate();
                    }
                }

                if (CciPhysicalSwapLeg.IsCci(CCiPhysicalSwapLeg.CciEnum.commodityAsset, pCci))
                {
                    SetCommodityContractCharacteristics();
                }

                InitializeQuantity(pCci);
                IntializeGrossAmount(pCci);

            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool ret = _cciFixedPriceSpotLeg.IsClientId_PayerOrReceiver(pCci);
            if (false == ret)
                ret = CciPhysicalSwapLeg.IsClientId_PayerOrReceiver(pCci);

            if (false == ret)
                ret = _cciRptSide.IsClientId_PayerOrReceiver(pCci);

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecute(CustomCaptureInfo pCci)
        {
            _cciFixedPriceSpotLeg.ProcessExecute(pCci);
            CciPhysicalSwapLeg.ProcessExecute(pCci);
            _cciRptSide.ProcessExecute(pCci);

            base.ProcessExecute(pCci);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            _cciFixedPriceSpotLeg.ProcessExecuteAfterSynchronize(pCci);
            CciPhysicalSwapLeg.ProcessExecuteAfterSynchronize(pCci);
            _cciRptSide.ProcessExecuteAfterSynchronize(pCci);

            base.ProcessExecuteAfterSynchronize(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            _cciFixedPriceSpotLeg.CleanUp();
            CciPhysicalSwapLeg.CleanUp();
            _cciRptSide.CleanUp();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void RefreshCciEnabled()
        {
            _cciFixedPriceSpotLeg.RefreshCciEnabled();
            CciPhysicalSwapLeg.RefreshCciEnabled();
            _cciRptSide.RefreshCciEnabled();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161214 [21916] Modify
        public override void Initialize_Document()
        {
            if (_commoditySpot.IdM == 0 && (CcisBase.CaptureMode == Cst.Capture.ModeEnum.New))
            {
                string defaultMarket = string.Empty;
                if (Ccis.TradeCommonInput.IsDefaultSpecified(CommonInput.DefaultEnum.market))
                    defaultMarket = (string)Ccis.TradeCommonInput.GetDefault(CommonInput.DefaultEnum.market);

                if (StrFunc.IsFilled(defaultMarket))
                {
                    //Warning: Ici on dispose d'un IDENTIFIER (et non pas d'un FIXML_SecurityExchange)
                    SQL_Market sqlMarket = new SQL_Market(CciTrade.CSCacheOn, SQL_TableWithID.IDType.Identifier, defaultMarket, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, CcisBase.User, CcisBase.SessionId);
                    if (sqlMarket.LoadTable(new string[] { "IDM" }))
                        _commoditySpot.IdM = sqlMarket.Id;
                }
            }

            _cciFixedPriceSpotLeg.Initialize_Document();
            CciPhysicalSwapLeg.Initialize_Document();

            // FI 20161214 [21916] call base.InitializeRptSideElement 
            InitializeRptSideElement();
            _cciRptSide.RptSide = _commoditySpot.RptSide;

            base.Initialize_Document();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20171025 [23509] Upd
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(ExchangeEnumValue, pCci))
            {
                if (null != pCci.Sql_Table)
                    CciTools.SetMarKetDisplay(pCci);
            }

            _cciFixedPriceSpotLeg.SetDisplay(pCci);
            CciPhysicalSwapLeg.SetDisplay(pCci);
            _cciRptSide.SetDisplay(pCci);
        }



        #endregion

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        ///  Retourne le payer du commodity Spot (=l'acheteur, = le payer du fixedLeg)
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override string CciClientIdPayer
        {
            get
            {
                return CciClientId(CciEnum.buyer);
            }
        }
        /// <summary>
        ///  Retourne le receiver du commodity Spot (=le vendeur , = le receveur du fixedLeg)
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override string CciClientIdReceiver
        {
            get
            {
                return CciClientId(CciEnum.seller);
            }
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

            _cciFixedPriceSpotLeg.SynchronizePayerReceiver(pLastValue, pNewValue);
            CciPhysicalSwapLeg.SynchronizePayerReceiver(pLastValue, pNewValue);
        }
        #endregion

        /// <summary>
        /// Pré-propostions diverses en fonction de l'asset
        /// </summary>
        // FI 20170116 [21916] Modify
        // EG 20171025 [23509] Upd 
        private void SetCommodityContractCharacteristics()
        {
            if (CciPhysicalSwapLeg.Cci(CCiPhysicalSwapLeg.CciEnum.commodityAsset).Sql_Table is SQL_AssetCommodityContract sqlAsset)
            {
                // FI 20200901 [XXXXX] Alimentation de Cci(ExchangeEnumValue).NewValue en fonction CciExchangeColumn
                if (StrFunc.IsFilled(_commoditySpot.PhysicalLeg.CommodityAsset.ExchangeId.Value) && CciExchangeColumn != "FIXML_SecurityExchange")
                {
                    // data contient le FIXML_SecurityExchange du marché
                    _commoditySpotContainer.GetMarket(CciTrade.CSCacheOn, null, out SQL_Market sqlMarket);
                    if (null != sqlMarket)
                        Cci(ExchangeEnumValue).NewValue = sqlMarket.GetFirstRowColumnValue(CciExchangeColumn) as string;
                }
                else
                    Cci(ExchangeEnumValue).NewValue = _commoditySpot.PhysicalLeg.CommodityAsset.ExchangeId.Value;


                Cci(CciEnum.settlementCurrency).NewValue = sqlAsset.IdC;

                // FI 20170116 [21916] Test Présence cci
                CustomCaptureInfo cci = _cciFixedPriceSpotLeg.Cci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_priceUnit);
                if (null != cci)
                    cci.NewValue = sqlAsset.CommodityContract_UnitOfPrice;
                else
                    _commoditySpot.FixedLeg.FixedPrice.PriceUnit.Value = sqlAsset.CommodityContract_UnitOfPrice;

                // FI 20170116 [21916] Test Présence cci
                cci = _cciFixedPriceSpotLeg.Cci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_priceCurrency);
                if (null != cci)
                    cci.NewValue = sqlAsset.IdC;
                else
                    _commoditySpot.FixedLeg.FixedPrice.Currency.Value = sqlAsset.IdC;

                InitializeDelivryStartEnd();
            }
        }

        /// <summary>
        /// Pré-propostion des dates de livaison de l'asset
        /// </summary>
        private void InitializeDelivryStartEnd()
        {
            if (CciPhysicalSwapLeg.Cci(CCiPhysicalSwapLeg.CciEnum.commodityAsset).Sql_Table is SQL_AssetCommodityContract sqlAsset)
            {
                if (sqlAsset.CommodityContract_TradableType == "Spot")
                {
                    //Contrat Spot => Livraison le lendemain    
                    DateTime deliveryStart = CciTrade.DataDocument.TradeDate.Add(new TimeSpan(1, 0, 0, 0)); //Ajout de 1 jour
                    Cci(CciEnum.effectiveDate_adjustableDate_unadjustedDate).NewValue = DtFunc.DateTimeToStringDateISO(deliveryStart);

                    CommodityQuantityFrequencyEnum frequency = default;
                    if (System.Enum.IsDefined(typeof(CommodityQuantityFrequencyEnum), sqlAsset.CommodityContract_FrequencyQuantity))
                        frequency = (CommodityQuantityFrequencyEnum)ReflectionTools.EnumParse(new CommodityQuantityFrequencyEnum(), sqlAsset.CommodityContract_FrequencyQuantity);

                    DateTime deliveryEnd = deliveryStart;
                    switch (frequency)
                    {
                        case CommodityQuantityFrequencyEnum.PerCalendarDay: // cas du gaz normalement
                            deliveryEnd = deliveryStart.Add(new TimeSpan(1, 0, 0, 0)); //Ajout de 1 jour
                            break;
                        case CommodityQuantityFrequencyEnum.PerHour:
                            deliveryEnd = deliveryStart;
                            break;
                        case CommodityQuantityFrequencyEnum.PerSettlementPeriod: // cas de l'electricité
                            //les valeurs duration d'une settlement Period sont dans l'enum SettlementPeriodDurationEnum (nécessairement < 1 jour)
                            deliveryEnd = deliveryStart;
                            break;
                        default:
                            break;
                    }
                    Cci(CciEnum.terminationDate_adjustableDate_unadjustedDate).NewValue = DtFunc.DateTimeToStringDateISO(deliveryEnd);
                }
                else if (sqlAsset.CommodityContract_TradableType == "Intraday")
                {
                    //Contrat Intraday => Livraison le jour même ou le lendemain
                    DateTime deliveryStart = CciTrade.DataDocument.TradeDate;
                    DateTime deliveryEnd = deliveryStart;

                    Cci(CciEnum.effectiveDate_adjustableDate_unadjustedDate).NewValue = DtFunc.DateTimeToStringDateISO(deliveryStart);
                    Cci(CciEnum.terminationDate_adjustableDate_unadjustedDate).NewValue = DtFunc.DateTimeToStringDateISO(deliveryEnd);
                }
            }
        }

        /// <summary>
        /// pré-propostion de la date de règlement
        /// </summary>
        // EG 20221201 [25639] [WI484] Upd
        private void InitializePaymentDate()
        {
            DateTime dtNextBiz = CciTrade.Product.GetNextBusinessDate(CciTrade.CSCacheOn);

            DateTime dtEffective = DateTime.MinValue; //dtEffective ou aussi DtLivraisonStart 
            if (_commoditySpot.EffectiveDate.AdjustableDateSpecified)
                dtEffective = _commoditySpot.EffectiveDate.AdjustableDate.UnadjustedDate.DateValue;


            if ((dtNextBiz == DateTime.MinValue) && (dtEffective == DateTime.MinValue))
            {
                //Nothing TO DO
            }
            else
            {
                DateTime dtPayment = DateTime.MinValue;
                if (dtEffective != DateTime.MinValue)
                {
                    if (dtEffective.CompareTo(dtNextBiz) > 0)
                    {
                        dtPayment = dtEffective;

                        IProductBase productBase =CciTrade.Product.ProductBase;
                        // Utiliser DayTypeEnum.ExchangeBusiness à la place de DayTypeEnum.Business, car on est en présence d'un BC du marché
                        IOffset offset = productBase.CreateOffset(PeriodEnum.D, 0, DayTypeEnum.ExchangeBusiness);

                        string idBC = string.Empty;
                        CciTrade.Product.GetMarket(CciTrade.CSCacheOn, null, out SQL_Market sql_Market);
                        if ((null != sql_Market) && sql_Market.LoadTable(new string[] { "IDBC" }))
                            idBC = sql_Market.IdBC;

                        if (StrFunc.IsFilled(idBC))
                        {
                            IBusinessCenters bcs = productBase.CreateBusinessCenters(idBC);
                            dtPayment = Tools.ApplyOffset(CciTrade.CSCacheOn, dtPayment, offset, bcs);
                        }
                    }
                    else
                    {
                        dtPayment = dtNextBiz;
                    }
                }

                if (dtPayment != DateTime.MinValue)
                {
                    _cciFixedPriceSpotLeg.Cci(CciFixedPriceSpotLeg.CciEnum.paymentDate).NewValue = DtFunc.DateTimeToStringDateISO(dtPayment);
                    _cciFixedPriceSpotLeg.CciGrossAmount.PaymentDateInitialize(true);

                    //Mise en place du businessCenter 
                    string idBC = string.Empty;
                    CciTrade.Product.GetMarket(CciTrade.CSCacheOn, null, out SQL_Market sql_Market);
                    if ((null != sql_Market) && sql_Market.LoadTable(new string[] { "IDBC" }))
                        idBC = sql_Market.IdBC;

                    IPayment payment = ((IPayment)_cciFixedPriceSpotLeg.CciGrossAmount.Payment);
                    // EG 20221201 [25639] [WI484] New
                    payment.Id = null;
                    payment.PaymentDate.Efs_id = "paymentDate";
                    payment.PaymentDate.DateAdjustments.BusinessCentersDefineSpecified = StrFunc.IsFilled(idBC);
                    if (payment.PaymentDate.DateAdjustments.BusinessCentersDefineSpecified)
                    {
                        payment.PaymentDate.DateAdjustments.BusinessCentersDefine = CciTrade.Product.ProductBase.CreateBusinessCenters(idBC);
                        payment.PaymentDate.DateAdjustments.BusinessCentersNoneSpecified = false;
                        payment.PaymentDate.DateAdjustments.BusinessCentersReferenceSpecified = false;
                    }
                    // EG 20221201 [25639] [WI484] New 
                    // La date de livraison et de paiement de la jambe physique pointe sur la date de paiement
                    // du GAM part référence.
                    if (_commoditySpot.IsEnvironmental)
                    {
                        // EG 20221125 New les éléments DeliveryDate et PaymentDate de la jambe physique (Environmental) sont calés à celle de la jambe fixe (PaymentDate du GAM)
                        IEnvironmentalPhysicalLeg physicalLeg = _commoditySpot.PhysicalLeg as IEnvironmentalPhysicalLeg;
                        physicalLeg.DeliveryDate.AdjustableDateSpecified = false;
                        physicalLeg.DeliveryDate.RelativeDateSpecified = true;
                        physicalLeg.DeliveryDate.RelativeDate.DateRelativeToValue = "paymentDate";
                        physicalLeg.DeliveryDate.RelativeDate.BusinessDayConvention = BusinessDayConventionEnum.NotApplicable;
                        physicalLeg.DeliveryDate.RelativeDate.Period = PeriodEnum.D;
                        physicalLeg.DeliveryDate.RelativeDate.PeriodMultiplier.IntValue = 0;

                        physicalLeg.PaymentDate.Period = PeriodEnum.D;
                        physicalLeg.PaymentDate.PeriodMultiplier.IntValue = 0;
                        physicalLeg.PaymentDate.BusinessDayConvention = BusinessDayConventionEnum.NotApplicable;
                    }
                }
            }
        }

        /* FI 20180301 [23814] Mise en commentiare car la méthode n'est plus appellée
        /// <summary>
        ///  pré-proposition des dates de compensation et de transaction en fonction de ENTITYMARKET 
        /// </summary>
        /// EG 20171016 [23509] Upd
        private void InitializeDateFromEntityMarket()
        {
            int idAEntity = _cciTrade.DataDocument.GetFirstEntity(_cciTrade.CSCacheOn);

            if (idAEntity > 0)
            {
                SQL_Market sqlMarket = null;
                _cciTrade.Product.GetMarket(_cciTrade.CSCacheOn, null, out sqlMarket);

                if (null != sqlMarket)
                {
                    DateTime dt = OTCmlHelper.GetDateBusiness(_cciTrade.CS, idAEntity, sqlMarket.Id, IdA_Custodian);
                    if (DtFunc.IsDateTimeFilled(dt))
                    {
                        //_cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDate).NewValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                        //_cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.businessDate).NewValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                        _cciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.executionDateTime).NewValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                        _cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.clearedDate).NewValue = DtFunc.DateTimeToString(dt, DtFunc.FmtISODate);
                    }
                }
            }
        }
        */
        /// <summary>
        ///  Recalcul éventuel de totalPhysicalQuantity_quantity ou de 
        /// </summary>
        /// <param name="pCCi"></param>
        // EG 20221201 [25639] [WI484] Upd
        // EG 20230505 [XXXXX] [WI617] DeliveryStartDateTime & DeliveryEndDateTime optional => controls for Trade template
        private void InitializeQuantity(CustomCaptureInfo pCci)
        {
            if (CciPhysicalSwapLeg is CCiGasPhysicalLeg cciGasLeg)
            {
                //Boolean isFromQty = cciGasLeg.cciDeliveryQuantity.cciPhysicalQuantity[0].IsCci(CciCommodityNotionalQuantity.CciEnum.quantity, pCci);
                Boolean isFromQty =
                (ArrFunc.IsFilled(cciGasLeg.CciDeliveryQuantity.CciPhysicalQuantity) &&
                    cciGasLeg.CciDeliveryQuantity.CciPhysicalQuantity[0].IsCci(CciCommodityNotionalQuantity.CciEnum.quantity, pCci));

                Boolean isFromTotalQty = cciGasLeg.CciDeliveryQuantity.IsCci(CciGasPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity, pCci);

                IGasPhysicalLeg physicalLeg = _commoditySpot.PhysicalLeg as IGasPhysicalLeg;
                if (isFromQty)
                {
                    string qtyUnit = physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityUnit.Value;
                    decimal qty = physicalLeg.DeliveryQuantity.PhysicalQuantity[0].Quantity.DecValue;

                    decimal qtyTotal = qty; //Egalité sur le gas
                    if (IsWattUnit(qtyUnit) && IsWattUnit(_commoditySpotContainer.TotalPhysicalQuantity.QuantityUnit.Value))
                        qtyTotal = ConvertWattUnit(qty, qtyUnit, _commoditySpotContainer.TotalPhysicalQuantity.QuantityUnit.Value);

                    CcisBase.SetNewValue(cciGasLeg.CciDeliveryQuantity.CciClientId(CciGasPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity), StrFunc.FmtDecimalToInvariantCulture(qtyTotal));
                }
                else if (isFromTotalQty)
                {
                    string qtyTotalUnit = _commoditySpotContainer.TotalPhysicalQuantity.QuantityUnit.Value;
                    decimal qtyTotal = _commoditySpotContainer.TotalPhysicalQuantity.Quantity.DecValue;

                    decimal qty = qtyTotal; //Egalité sur le gas
                    if (IsWattUnit(qtyTotalUnit) && IsWattUnit(physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityUnit.Value))
                        qty = ConvertWattUnit(qty, qtyTotalUnit, physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityUnit.Value);

                    CustomCaptureInfo cci = null;
                    if (ArrFunc.IsFilled(cciGasLeg.CciDeliveryQuantity.CciPhysicalQuantity))
                    {
                        cci = cciGasLeg.CciDeliveryQuantity.CciPhysicalQuantity[0].Cci(CciCommodityNotionalQuantity.CciEnum.quantity);
                        if (null != cci && (StrFunc.IsEmpty(cci.NewValue)))
                            cci.NewValue = StrFunc.FmtDecimalToInvariantCulture(qty);
                    }
                    if (null == cci)
                        physicalLeg.DeliveryQuantity.PhysicalQuantity[0].Quantity.DecValue = qty;
                }
                else
                {
                    //Nothing TODO
                }
            }
            else if (CciPhysicalSwapLeg is CCiElectricityPhysicalLeg cciElectricityLeg)
            {
                if (
                    (ArrFunc.IsFilled(cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity) &&
                    cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity[0].IsCci(CciCommodityNotionalQuantity.CciEnum.quantity, pCci)) ||
                    cciElectricityLeg.CciDeliveryQuantity.IsCci(CciElectricityPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity, pCci) ||
                    /* Livraison Début */
                    this.IsCci(CciEnum.effectiveDate_adjustableDate_unadjustedDate, pCci) ||
                    cciElectricityLeg.CciElectricitySettlementPeriods[0].CciStartTime.IsCci(CCiPrevailingTime.CciEnum.hourMinuteTime, pCci) ||
                    /* Livraison Fin */
                    cciElectricityLeg.CciElectricitySettlementPeriods[0].CciEndTime.IsCci(CCiPrevailingTime.CciEnum.hourMinuteTime, pCci) ||
                    this.IsCci(CciEnum.terminationDate_adjustableDate_unadjustedDate, pCci))
                {
                    IElectricityPhysicalLeg physicalLeg = _commoditySpot.PhysicalLeg as IElectricityPhysicalLeg;
                    if (
                        DtFunc.IsDateTimeFilled(_commoditySpotContainer.DeliveryStartDate) &&
                        StrFunc.IsFilled(_commoditySpotContainer.DeliveryStartTime.HourMinuteTime.Value) &&
                        DtFunc.IsDateTimeFilled(_commoditySpotContainer.DeliveryEndDate) &&
                        StrFunc.IsFilled(_commoditySpotContainer.DeliveryEndTime.HourMinuteTime.Value))
                    {

                        Boolean isFromTotalQty = cciElectricityLeg.CciDeliveryQuantity.IsCci(CciElectricityPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity, pCci);
                        Boolean isFromQty = false;

                        if ((ArrFunc.IsFilled(cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity) &&
                                                CcisBase.Contains(cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity[0].CciClientId(CciCommodityNotionalQuantity.CciEnum.quantity))))
                        {
                            // Toutes modifications sur cci autres que  totalPhysicalQuantity_quantity s'apparente à une modification de quantity
                            isFromQty = (!isFromTotalQty);
                        }
                        else
                        {
                            // Toutes modifications  s'apparente à une modification de Totalquantity
                            isFromTotalQty = true;
                        }


                        SettlementPeriodDurationEnum? settlementPeriodDurationEnum;
                        if (physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityFrequency.Value == CommodityQuantityFrequencyEnum.PerSettlementPeriod.ToString())
                            settlementPeriodDurationEnum = physicalLeg.SettlementPeriods[0].Duration;
                        else
                            settlementPeriodDurationEnum = SettlementPeriodDurationEnum.OneHour;

                        // EG 20230505 [XXXXX] [WI617]
                        Nullable<DateTimeOffset> dateTimeStart = _commoditySpotContainer.DeliveryStartDateTime;
                        Nullable<DateTimeOffset> dateTimeEnd = _commoditySpotContainer.DeliveryEndDateTime;

                        decimal coef = 0;
                        if (dateTimeStart.HasValue && dateTimeEnd.HasValue)
                        {
                            if (dateTimeEnd.Value.CompareTo(dateTimeStart.Value) >= 0)
                            {
                                TimeSpan timeSpan = (dateTimeEnd.Value - dateTimeStart.Value);
                                coef = timeSpan.Days * 24;
                                switch (settlementPeriodDurationEnum)
                                {
                                    case SettlementPeriodDurationEnum.OneHour:
                                    case SettlementPeriodDurationEnum.TwoHours:
                                        coef += timeSpan.Hours;
                                        break;
                                    case SettlementPeriodDurationEnum.FifteenMinutes:
                                    case SettlementPeriodDurationEnum.ThirtyMinutes:
                                        coef += timeSpan.Hours;
                                        coef += timeSpan.Minutes / new Decimal(60);
                                        break;
                                    default:
                                        throw new NotImplementedException(StrFunc.AppendFormat("settlementPeriodDurationEnum (value:{0}) is not implemented"));
                                }
                            }
                        }
                        if (isFromQty)
                        {
                            string qtyUnit = physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityUnit.Value;
                            decimal qty = physicalLeg.DeliveryQuantity.PhysicalQuantity[0].Quantity.DecValue;

                            decimal result = qty * coef;

                            decimal qtyTotal = ConvertWattUnit(result, qtyUnit, _commoditySpotContainer.TotalPhysicalQuantity.QuantityUnit.Value);
                            CcisBase.SetNewValue(cciElectricityLeg.CciDeliveryQuantity.CciClientId(CciElectricityPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity), StrFunc.FmtDecimalToInvariantCulture(qtyTotal));
                        }
                        else if (isFromTotalQty)
                        {
                            string qtyTotalUnit = _commoditySpotContainer.TotalPhysicalQuantity.QuantityUnit.Value;
                            decimal qtyTotal = _commoditySpotContainer.TotalPhysicalQuantity.Quantity.DecValue;

                            decimal result = decimal.Zero;
                            if (coef > 0)
                                result = qtyTotal / coef;

                            decimal qty = ConvertWattUnit(result, qtyTotalUnit, physicalLeg.DeliveryQuantity.PhysicalQuantity[0].QuantityUnit.Value);


                            CustomCaptureInfo cci = null;
                            if (ArrFunc.IsFilled(cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity))
                            {
                                cci = cciElectricityLeg.CciDeliveryQuantity.CciPhysicalQuantity[0].Cci(CciCommodityNotionalQuantity.CciEnum.quantity);
                                if (null != cci && (StrFunc.IsEmpty(cci.NewValue)))
                                    cci.NewValue = StrFunc.FmtDecimalToInvariantCulture(qty);
                            }
                            if (null == cci)
                                physicalLeg.DeliveryQuantity.PhysicalQuantity[0].Quantity.DecValue = qty;
                        }
                        else
                            throw new NotImplementedException("isFromQty = false and isFromTotalQty =false");

                    }
                }
            }
            // EG 20221201 [25639] [WI484] New
            else if (CciPhysicalSwapLeg is CciEnvironmentalPhysicalLeg)
            {
                //Nothing TODO
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", CciPhysicalSwapLeg.ToString()));
            }
        }

        /// <summary>
        /// Convertie des [KW,KWh],[MW,MWh],[GW,GWh]  
        /// <para>Cette méthode considère qu'il existe une equivalence entre l'unité de puissance (ex KW) et l'unité de consommation (ex KWh)</para>
        /// </summary>
        /// <param name="pQty"></param>
        /// <param name="pUnitSource"></param>
        /// <param name="pUnitTarget"></param>
        /// <returns></returns>
        private static Decimal ConvertWattUnit(Decimal pQty, string pUnitSource, string pUnitTarget)
        {
            Nullable<Double> coef = null;

            switch (pUnitTarget)
            {
                case "KW":
                case "KWh":
                    switch (pUnitSource)
                    {
                        case "KW":
                        case "KWh":
                            coef = 1;
                            break;
                        case "MW":
                        case "MWh":
                            coef = Math.Pow(1, 3);
                            break;
                        case "GW":
                        case "GWh":
                            coef = Math.Pow(1, 6);
                            break;
                    }
                    break;

                case "MW":
                case "MWh":
                    switch (pUnitSource)
                    {
                        case "KW":
                        case "KWh":
                            coef = 1 / Math.Pow(1, 3);
                            break;
                        case "MW":
                        case "MWh":
                            coef = 1;
                            break;
                        case "GW":
                        case "GWh":
                            coef = Math.Pow(1, 3);
                            break;
                    }
                    break;
                case "GW":
                    switch (pUnitSource)
                    {
                        case "KW":
                        case "KWh":
                            coef = 1 / Math.Pow(1, 6);
                            break;
                        case "MW":
                        case "MWh":
                            coef = 1 / Math.Pow(1, 3);
                            break;
                        case "GW":
                        case "GWh":
                            coef = 1;
                            break;
                    }
                    break;
            }

            decimal ret = coef.HasValue ? Convert.ToDecimal(coef) * pQty : Decimal.Zero;
            return ret;
        }

        /// <summary>
        /// Retourne true si kW ou kWh ou MW ou MWh ou GW ou GWh
        /// </summary>
        /// <param name="pUnit"></param>
        /// <returns></returns>
        private static Boolean IsWattUnit(string pUnit)
        {
            return (pUnit.EndsWith("kW") || pUnit.EndsWith("kWh") || pUnit.EndsWith("MW") || pUnit.EndsWith("MWh") || pUnit.EndsWith("GW") || pUnit.EndsWith("GWh"));
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20221201 [25639] [WI484] Upd
        private void IntializeGrossAmount(CustomCaptureInfo pCci)
        {
            IUnitQuantity totalPhysicalQuantity = null;

            Boolean isToCalc = false;

            if (CciPhysicalSwapLeg is CCiGasPhysicalLeg cciGasLeg)
            {
                if (cciGasLeg.CciDeliveryQuantity.IsCci(CciGasPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity, pCci) ||
                    _cciFixedPriceSpotLeg.IsCci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_price, pCci))
                {
                    totalPhysicalQuantity = ((IGasPhysicalLeg)_commoditySpot.PhysicalLeg).DeliveryQuantity.TotalPhysicalQuantity;
                    isToCalc = true;
                }
            }
            else if (CciPhysicalSwapLeg is CCiElectricityPhysicalLeg cciElectricityLeg)
            {
                if (cciElectricityLeg.CciDeliveryQuantity.IsCci(CciElectricityPhysicalQuantity.CciEnum.totalPhysicalQuantity_quantity, pCci) ||
                    _cciFixedPriceSpotLeg.IsCci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_price, pCci))
                {
                    totalPhysicalQuantity = ((IElectricityPhysicalLeg)_commoditySpot.PhysicalLeg).DeliveryQuantity.TotalPhysicalQuantity;
                    isToCalc = true;
                }
            }
            // EG 20221201 [25639] [WI484] New
            else if (CciPhysicalSwapLeg is CciEnvironmentalPhysicalLeg cciEnvironmentalLeg)
            {
                if (cciEnvironmentalLeg.IsCci(CciEnvironmentalPhysicalLeg.CciEnum.numberOfAllowances_quantity, pCci) ||
                    _cciFixedPriceSpotLeg.IsCci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_price, pCci))
                {
                    totalPhysicalQuantity = ((IEnvironmentalPhysicalLeg)_commoditySpot.PhysicalLeg).NumberOfAllowances;
                }
                isToCalc = true;
            }
            else
            {
                throw new NotImplementedException(StrFunc.AppendFormat("type:{0} is not implemented", CciPhysicalSwapLeg.ToString()));
            }

            if (isToCalc && (null != totalPhysicalQuantity))
            {
                string priceUnit = _commoditySpot.FixedLeg.FixedPrice.PriceUnit.Value;
                if (totalPhysicalQuantity.QuantityUnit.Value == priceUnit)
                {
                    //Sur l'ectricité les prix peuvent être négatifs
                    decimal amount = totalPhysicalQuantity.Quantity.DecValue * Math.Abs(_commoditySpot.FixedLeg.FixedPrice.Price.DecValue);
                    string currency = _commoditySpot.FixedLeg.FixedPrice.Currency.Value;
                    IMoney money = CciTrade.CurrentTrade.Product.ProductBase.CreateMoney(amount, currency);

                    CcisBase.SetNewValue(_cciFixedPriceSpotLeg.CciGrossAmount.CciClientId(CciPayment.CciEnum.amount), StrFunc.FmtDecimalToInvariantCulture(money.Amount.DecValue), false);
                    CcisBase.SetNewValue(_cciFixedPriceSpotLeg.CciGrossAmount.CciClientId(CciPayment.CciEnum.currency), money.Currency, false);
                }

                if (_cciFixedPriceSpotLeg.IsCci(CciFixedPriceSpotLeg.CciEnum.fixedPrice_price, pCci))
                {
                    CustomCaptureInfo cciPayer = _cciFixedPriceSpotLeg.CciGrossAmount.Cci(CciPayment.CciEnum.payer);
                    CustomCaptureInfo cciReceiver = _cciFixedPriceSpotLeg.CciGrossAmount.Cci(CciPayment.CciEnum.receiver);

                    bool isPositivePrice = (Math.Sign(DecFunc.DecValue(pCci.NewValue)) >= 0);
                    if (isPositivePrice)
                    {
                        cciPayer.NewValue = _commoditySpot.BuyerPartyReference.HRef;
                        cciReceiver.NewValue = _commoditySpot.SellerPartyReference.HRef;
                    }
                    else
                    {
                        cciPayer.NewValue = _commoditySpot.SellerPartyReference.HRef;
                        cciReceiver.NewValue = _commoditySpot.BuyerPartyReference.HRef;
                    }
                }
            }
        }


        /// <summary>
        /// Reset des Cci suite à modification de la plateforme
        /// </summary>
        /// EG 20171113 [23509] New
        public override void ResetCciFacilityHasChanged()
        {
            base.ResetCciFacilityHasChanged();
            
            // Reset des Cci Clearer/Custodian
            if ((_commoditySpotContainer.IsOneSide) &&
                CciTrade.cciParty[1].Cci(CciTradeParty.CciEnum.actor).IsFilled)
                CciTrade.cciParty[1].ResetCciFacilityHasChanged();

            // Reset des Cci Exchange et asset
            CciPhysicalSwapLeg.ResetCciFacilityHasChanged();
            //Cci(ExchangeEnumValue).Reset();
        }

        /// <summary>
        /// Affichage éventuel du ExecRefID dans le Header du WCTableH tblTrade
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200120 [25167] Add Method
        private void DisplayExecRefID(CciPageBase pPage)
        {
            string id = CciTrade.Ccis.GetIdTableHProduct("tblTradeH");
            if (StrFunc.IsFilled(id))
            {
                if (pPage.FindControl(id) is WCTogglePanel pnl)
                {
                    string data = (_commoditySpot.RptSide[0].ExecRefIdSpecified ? _commoditySpot.RptSide[0].ExecRefId : string.Empty);
                    TradeCustomCaptureInfos.SetLinkInfoInTogglePanel(pnl, "ExecRefId", data, null);
                }
            }
        }
    }
}


