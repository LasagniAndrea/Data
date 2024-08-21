#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FixML.Enum;
using FixML.Interface;
using FpML.Interface;
using System;
using System.Linq;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// </summary>
    public class CciProductFX : CciProductBase, ICciPresentation
    {
        #region Members
        private CciFXLeg[] _cciFxLeg;
        
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            //[System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            //buyer,
            //[System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
            //seller,
            RptSide_Side,
            unknown,
        }
        #endregion

        #region accessors
        public FxLegContainer FxLegContainer { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public int FxLegLength
        {
            get
            {
                return ArrFunc.Count(_cciFxLeg);
            }
        }
        
        /// <summary>
        /// 
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
        #endregion

        #region constructor
        public CciProductFX(CciTrade pCciTrade, IFxLeg pFxLeg, string pPrefix)
            : this(pCciTrade, pFxLeg, pPrefix, -1)
        {
        }
        public CciProductFX(CciTrade pCciTrade, IFxLeg pFxLeg, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pFxLeg, pPrefix, pNumber)
        {
         
        }

        public CciProductFX(CciTrade pCciTrade, IFxSwap pFxSwap, string pPrefix)
            : this(pCciTrade, pFxSwap, pPrefix, -1)
        {
        }
        public CciProductFX(CciTrade pCciTrade, IFxSwap pFxSwap, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pFxSwap, pPrefix, pNumber)
        {
            
        }
        #endregion constructor

        #region Membres de ITradeCci
        #region RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetSellSide(); } }
        #endregion
        #region RetSideReceiver
        public override string RetSideReceiver
        {
            // Le Receiver de la devise 1 est l'acheteur (Achat EUR/USD => Reçoit EUR et Paye l'USD
            get { return SideTools.RetBuySide(); }
        }
        #endregion
        #region GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                IFxLeg fxLeg = GetFirstLeg();
                return fxLeg.ExchangedCurrency1.PaymentAmount.Currency;

            }
        }
        #endregion
        #region CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get
            {
                return _cciFxLeg[0].CciPaymentCur1.CciClientId(CciPayment.CciEnumPayment.paymentAmount_currency);
            }
        }
        #endregion
        #endregion

        #region Membres de IContainerCciPayerReceiver
        #region CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return _cciFxLeg[0].CciPaymentCur1.CciClientIdPayer; }
        }
        #endregion
        #region CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return _cciFxLeg[0].CciPaymentCur1.CciClientIdReceiver; }
        }
        #endregion
        #region SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            for (int i = 0; i < FxLegLength; i++)
            {
                _cciFxLeg[i].CciPaymentCur1.SynchronizePayerReceiver(pLastValue, pNewValue);
                _cciFxLeg[i].CciPaymentCur2.SynchronizePayerReceiver(pLastValue, pNewValue);
            }

        }
        #endregion
        #endregion

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20161114 [RATP] Modify
        public override void Initialize_FromCci()
        {
            if (CciTrade.Product.Product.ProductBase.IsFxSwap)
                InitializeFxSwap_FromCci();

            // Il faudrait Ajouter les legs non existants dans le Fpml Document et paramétrés dans screenObject
            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].Initialize_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161114 [RATP]  add Method
        private void InitializeFxSwap_FromCci()
        {
            Boolean isAddItemInArray = false;

            int index = -1;
            IFxSwap fxSwap = (IFxSwap)CciTrade.Product.Product;

            bool isOk = true;
            while (isOk)
            {
                index += 1;
                //
                CciFXLeg cciFxLegCurrent = new CciFXLeg(CciTrade, TradeCustomCaptureInfos.CCst.Prefix_fx, index + 1, null);
                //
                isOk = CcisBase.Contains(cciFxLegCurrent.CciClientId(CciFXLeg.CciEnum.valueDate));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(fxSwap.FxSingleLeg) || (index == fxSwap.FxSingleLeg.Length))
                    {
                        isAddItemInArray = true;
                        ReflectionTools.AddItemInArray(fxSwap, "fxSingleLeg", index);
                        if (ArrFunc.IsFilled(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                        {
                            // FI 20161114 [RATP] GLOP 
                            // Normalement on ne va pas lire FpMLDataDocReader.party[1].id (voir stream des swap)
                            // Fais ici car pb de synchronisation des payers/receivers (A Revoir après le POC) 

                            string party1Id = Ccis.TradeCommonInput.FpMLDataDocReader.Party[0].Id;
                            string party2Id = string.Empty;
                            if (Ccis.TradeCommonInput.FpMLDataDocReader.Party.Length > 1)
                                party2Id = Ccis.TradeCommonInput.FpMLDataDocReader.Party[1].Id;

                            fxSwap.FxSingleLeg[index].ExchangedCurrency1.PayerPartyReference.HRef = party1Id;
                            fxSwap.FxSingleLeg[index].ExchangedCurrency1.ReceiverPartyReference.HRef = party2Id;

                            fxSwap.FxSingleLeg[index].ExchangedCurrency2.PayerPartyReference.HRef = party2Id;
                            fxSwap.FxSingleLeg[index].ExchangedCurrency2.ReceiverPartyReference.HRef = party1Id;
                        }
                    }
                }
            }

            if (isAddItemInArray)
                SetProduct((IProduct)fxSwap);
        }


        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            if ((null != FxLegContainer) && FxLegContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            //if (!ccis.Contains(CciClientId(CciEnum.buyer)))
            //    ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string));

            //if (!ccis.Contains(CciClientId(CciEnum.seller)))
            //    ccis.Add(new CustomCaptureInfo(Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string));

            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].AddCciSystem();

        }
        #endregion
        #region public override Initialize_FromDocument
        public override void Initialize_FromDocument()
        {

            foreach (CustomCaptureInfo cci in CcisBase)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    // EG 20160404 Migration vs2013
                    //CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string cliendId_Key = this.CciContainerKey(cci.ClientId_WithoutPrefix);
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);
                    //
                    switch (key)
                    {
                        #region Side
                        case CciEnum.RptSide_Side:
                            if (null != FxLegContainer)
                            {
                                IFixTrdCapRptSideGrp _rptSide = FxLegContainer.RptSide[0];
                                if (_rptSide.SideSpecified)
                                    data = ReflectionTools.ConvertEnumToString<SideEnum>(_rptSide.Side);
                            }
                            break;
                        #endregion Side
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }

            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].Initialize_FromDocument();
        }

        #endregion
        #region public override Dump_ToDocument
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
                        #region Side
                        case CciEnum.RptSide_Side:
                            if ((null != FxLegContainer) && FxLegContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = FxLegContainer.RptSide[0];
                                _rptSide.SideSpecified = StrFunc.IsFilled(data);
                                if (_rptSide.SideSpecified)
                                {
                                    SideEnum sideEnum = (SideEnum)ReflectionTools.EnumParse(_rptSide.Side, data);
                                    _rptSide.Side = sideEnum;
                                }
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion Side

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }

                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            for (int i = 0; i < _cciFxLeg.Length; i++)
            {
                SynchronizeNextLeg(i);
                _cciFxLeg[i].Dump_ToDocument();
            }
            // EG 20160404 Migration vs2013
            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
            {
                //Product.SynchronizeExchangeTraded();
                Product.SynchronizeFromDataDocument();
            }
        }

        #endregion
        #region public override ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    case CciEnum.RptSide_Side:
                        if ((null != FxLegContainer) && FxLegContainer.IsOneSide)
                        {
                            IFixTrdCapRptSideGrp _rptSide = FxLegContainer.RptSide[0];
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

                        break;
                }
            }

            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].ProcessInitialize(pCci);
            //  
            if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                CciTrade.InitializePartySide();

            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(CciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    if ((null != FxLegContainer) && FxLegContainer.IsOneSide)
                    {
                        #region Préproposition de la contrepartie en fonction du ClearingTemplate
                        if (CciTrade.cciParty[1].IsInitFromClearingTemplate)
                        {
                            if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                        }
                        #endregion
                    }
                }
            }


        }
        #endregion
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            //
            for (int i = 0; i < _cciFxLeg.Length; i++)
            {
                isOk = _cciFxLeg[i].IsClientId_PayerOrReceiver(pCci);
                if (isOk)
                    break;
            }
            return isOk;

        }
        #endregion

        #region public override CleanUp
        public override void CleanUp()
        {
            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].CleanUp();
            //
            if (Product.IsFxSwap)
            {
                IFxSwap fxSwap = (IFxSwap)Product.Product;
                if (ArrFunc.IsFilled(fxSwap.FxSingleLeg))
                {
                    for (int i = fxSwap.FxSingleLeg.Length - 1; -1 < i; i--)
                    {
                        if (false == CaptureTools.IsDocumentElementValid(fxSwap.FxSingleLeg[i].ExchangedCurrency1.PayerPartyReference.HRef))
                            ReflectionTools.RemoveItemInArray(fxSwap, "fxSingleLeg", i);
                    }
                }
            }

        }
        #endregion  CleanUp
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < _cciFxLeg.Length; i++)
                _cciFxLeg[i].SetDisplay(pCci);

        }
        #endregion  SetDisplay
        #region public override Initialize_Document
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override void Initialize_Document()
        {
            // FI 20170116 [21916] Mise en commentaire
            //if (Cst.Capture.IsModeInput(ccis.CaptureMode) && (null != _fxLegContainer))
            //    _fxLegContainer.InitRptSide(_cciTrade.CS, CciTradeCommon.TradeCommonInput.IsAllocation);

            // FI 20170116 [21916] call InitializeRptSideElement (harmonisation des produits contenant un RptSide)
            if (null != FxLegContainer)
                base.InitializeRptSideElement(); 


            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                IFxLeg[] fxLeg = null;
                //FI GLOP STRATEGY  
                if (CciTrade.Product.IsFxLeg)
                {
                    fxLeg = CciTrade.DataDocument.CurrentProduct.ProductBase.CreateFxLegs(1);
                    fxLeg[0] = (IFxLeg)Ccis.TradeCommonInput.Product.Product;

                }
                else if (CciTrade.Product.IsFxSwap)
                {
                    fxLeg = ((IFxSwap)CciTrade.DataDocument.CurrentProduct.Product).FxSingleLeg;
                }
                //
                for (int i = 0; i < ArrFunc.Count(fxLeg); i++)
                {
                    if (StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef) &&
                        StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency1.ReceiverPartyReference.HRef))
                    {
                        fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef = fxLeg[i].ExchangedCurrency2.ReceiverPartyReference.HRef;
                        fxLeg[i].ExchangedCurrency1.ReceiverPartyReference.HRef = fxLeg[i].ExchangedCurrency2.PayerPartyReference.HRef;
                    }
                    if (StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency2.PayerPartyReference.HRef) &&
                        StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency2.ReceiverPartyReference.HRef))
                    {
                        fxLeg[i].ExchangedCurrency2.PayerPartyReference.HRef = fxLeg[i].ExchangedCurrency1.ReceiverPartyReference.HRef;
                        fxLeg[i].ExchangedCurrency2.ReceiverPartyReference.HRef = fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef;
                    }
                    //
                    if (StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef) &&
                        StrFunc.IsEmpty(fxLeg[i].ExchangedCurrency1.ReceiverPartyReference.HRef))
                    {
                        // si payerPartyReference/receiver dev1 = string.emty alors forcement  payerPartyReference/receiver dev2 = string.empty
                        if (1 == i)
                        {
                            fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef = fxLeg[i - 1].ExchangedCurrency1.ReceiverPartyReference.HRef;
                            fxLeg[i].ExchangedCurrency1.ReceiverPartyReference.HRef = fxLeg[i - 1].ExchangedCurrency1.PayerPartyReference.HRef;
                            //
                            fxLeg[i].ExchangedCurrency2.PayerPartyReference.HRef = fxLeg[i - 1].ExchangedCurrency2.ReceiverPartyReference.HRef;
                            fxLeg[i].ExchangedCurrency2.ReceiverPartyReference.HRef = fxLeg[i - 1].ExchangedCurrency2.PayerPartyReference.HRef;
                        }
                        else
                        {
                            if (StrFunc.IsEmpty(id))
                            {
                                //20080523 FI Mise en commentaire, s'il n'y a pas partie il mettre unknown 
                                //HPC est broker ds les template et ne veut pas être 1 contrepartie
                                //id = GetIdFirstPartyCounterparty();
                                id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                            }
                            //
                            fxLeg[i].ExchangedCurrency1.PayerPartyReference.HRef = id;
                            fxLeg[i].ExchangedCurrency2.ReceiverPartyReference.HRef = id;
                        }
                    }
                    //
                    if (TradeCustomCaptureInfos.PartyUnknown == id)
                        CciTrade.AddPartyUnknown();
                }
            }

        }
        #endregion
        #endregion
        
        #region Membres de ITradeGetInfoButton
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //
            #region Legs
            for (int i = 0; i < _cciFxLeg.Length; i++)
            {
                if (_cciFxLeg[i].IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    if (_cciFxLeg[i].CciExchangeRate.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                    {
                        string key = _cciFxLeg[i].CciExchangeRate.CciContainerKey(pCci.ClientId_WithoutPrefix);
                        CciExchangeRate.CciEnum elt = CciExchangeRate.CciEnum.unknown;
                        //
                        if (System.Enum.IsDefined(typeof(CciExchangeRate.CciEnum), key))
                            elt = (CciExchangeRate.CciEnum)System.Enum.Parse(typeof(CciExchangeRate.CciEnum), key);
                        //
                        isOk = true;
                        switch (elt)
                        {
                            case CciExchangeRate.CciEnum.rate:
                                pCo.Element = "sideRates";
                                pCo.Object = "exchangeRate";
                                pCo.ObjectIndexValue = i;
                                pCo.Occurence = "Item";
                                pIsSpecified = _cciFxLeg[i].IsSideRatesSpecified;
                                pIsEnabled = pIsSpecified;
                                break;
                            default:
                                isOk = false;
                                break;
                        }
                    }
                    //					
                    #region exchangedCurrency1 && exchangedCurrency2
                    for (int j = 1; j < 3; j++)
                    {
                        if (!isOk)
                        {
                            PropertyInfo pty = _cciFxLeg[i].GetType().GetProperty("CciPaymentCur" + j);
                            CciPayment cciPayment = (CciPayment)pty.GetValue(_cciFxLeg[i], null);
                            //
                            if ((null != cciPayment) && cciPayment.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                            {
                                string keyPayment = cciPayment.CciContainerKey(pCci.ClientId_WithoutPrefix);
                                CciPayment.CciEnumPayment eltPayment = CciPayment.CciEnumPayment.unknown;
                                //
                                if (System.Enum.IsDefined(typeof(CciPayment.CciEnumPayment), keyPayment))
                                    eltPayment = (CciPayment.CciEnumPayment)System.Enum.Parse(typeof(CciPayment.CciEnumPayment), keyPayment);
                                //
                                isOk = true;
                                switch (eltPayment)
                                {
                                    case CciPayment.CciEnumPayment.settlementInformation:
                                        pCo.Element = "settlementInformation";
                                        pCo.Object = "exchangedCurrency" + j;
                                        pCo.ObjectIndexValue = i;
                                        pCo.OccurenceValue = i + 1;
                                        pCo.CopyTo = "item";
                                        pIsSpecified = cciPayment.IsSettlementInfoSpecified;
                                        pIsEnabled = cciPayment.IsSettlementInstructionSpecified;
                                        break;
                                    default:
                                        isOk = false;
                                        break;
                                }
                            }
                        }
                    }
                    #endregion exchangedCurrency1 && exchangedCurrency2
                }
            }
            #endregion Legs

            //
            return isOk;

        }
        #endregion SetButtonZoom
        #endregion Membres de ITradeGetInfoButton
        
        #region Membres de IContainerCciQuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            if (!isOk)
            {
                for (int i = 0; i < FxLegLength; i++)
                {
                    isOk = _cciFxLeg[i].IsClientId_QuoteBasis(pCci);
                    if (isOk)
                        break;
                }
            }
            return isOk;

            //

        }
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            CciFXLeg cciFxLeg = GetCciFxLeg(pCci.ClientId_WithoutPrefix);
            if (null != cciFxLeg)
                ret = cciFxLeg.GetCurrency1(pCci);
            //
            return ret;

        }
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            CciFXLeg cciFxLeg = GetCciFxLeg(pCci.ClientId_WithoutPrefix);
            if (null != cciFxLeg)
                ret = cciFxLeg.GetCurrency2(pCci);
            //
            return ret;

        }
        #endregion
        
        #region methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            if (Tools.IsInterfaceOf(pProduct, EfsML.Enum.InterfaceEnum.IFxSwap))
            {
                string prefix2 = Prefix.Remove(Prefix.LastIndexOf(CustomObject.KEY_SEPARATOR));

                IFxSwap fxSwap = (IFxSwap)pProduct;
                
                _cciFxLeg = new CciFXLeg[ArrFunc.Count(fxSwap.FxSingleLeg)];
                for (int i = 0; i < FxLegLength; i++)
                    _cciFxLeg[i] = new CciFXLeg(CciTrade, prefix2, i + 1, fxSwap.FxSingleLeg[i]); ;
            }
            else if (Tools.IsInterfaceOf(pProduct, EfsML.Enum.InterfaceEnum.IFxLeg))
            {
                string prefix2 = Prefix.Remove(Prefix.LastIndexOf(CustomObject.KEY_SEPARATOR));

                IFxLeg fxLeg = (IFxLeg)pProduct;
                
                _cciFxLeg = new CciFXLeg[1];
                _cciFxLeg[0] = new CciFXLeg(CciTrade, prefix2, -1, fxLeg);
                FxLegContainer = new FxLegContainer(fxLeg, CciTradeCommon.TradeCommonInput.DataDocument);
            }

            base.SetProduct(pProduct);
        }
        #endregion
        //
        #region private GetFirstLeg
        private IFxLeg GetFirstLeg()
        {
            IFxLeg ret = null;
            //FI GLOP STRATEGY
            if (CciTrade.Product.IsFxLeg)
                ret = (IFxLeg)CciTrade.Product.Product;
            else if (CciTrade.Product.IsFxSwap)
                ret = ((IFxSwap)CciTrade.Product.Product).FxSingleLeg[0];
            //
            return ret;

        }
        #endregion GetFirstLeg
        #region private SynchronizeNextLeg
        private void SynchronizeNextLeg(int pCurrentIndex)
        {
            int currentIndex = pCurrentIndex;
            int nextIndex = currentIndex + 1;
            bool isOkSynchronize = (_cciFxLeg.Length > nextIndex);
            //
            if (isOkSynchronize)
            {
                foreach (CustomCaptureInfo cci in CcisBase)
                {
                    //On ne traite que les contrôle dont le contenu à changé
                    if ((cci.HasChanged) && (_cciFxLeg[currentIndex].IsCciOfContainer(cci.ClientId_WithoutPrefix)))
                    {
                        string clientIdKey = _cciFxLeg[currentIndex].CciContainerKey(cci.ClientId_WithoutPrefix);

                        if (null != _cciFxLeg[nextIndex].Cci(clientIdKey))
                        {
                            CustomCaptureInfo cciTarget = _cciFxLeg[nextIndex].Cci(clientIdKey);
                            //
                            bool isPaymentCur1 = (_cciFxLeg[nextIndex].CciPaymentCur1.IsCciOfContainer(cciTarget.ClientId_WithoutPrefix));
                            bool isPaymentCur2 = (_cciFxLeg[nextIndex].CciPaymentCur2.IsCciOfContainer(cciTarget.ClientId_WithoutPrefix));
                            //
                            if ((isPaymentCur1) || (isPaymentCur2))
                            {
                                CciPayment cciPayment;

                                #region Payment
                                if (isPaymentCur1)
                                    cciPayment = _cciFxLeg[nextIndex].CciPaymentCur1;
                                else
                                    cciPayment = _cciFxLeg[nextIndex].CciPaymentCur2;
                                //
                                string strkey = cciPayment.CciContainerKey(cciTarget.ClientId_WithoutPrefix);
                                CciPayment.CciEnumPayment key = CciPayment.CciEnumPayment.unknown;
                                if (System.Enum.IsDefined(typeof(CciPayment.CciEnumPayment), strkey))
                                    key = (CciPayment.CciEnumPayment)System.Enum.Parse(typeof(CciPayment.CciEnumPayment), strkey);
                                //
                                switch (key)
                                {
                                    case CciPayment.CciEnumPayment.paymentAmount_currency:
                                        cciTarget.NewValue = cci.NewValue;
                                        cciTarget.IsInputByUser = true;
                                        break;
                                    case CciPayment.CciEnumPayment.paymentAmount_amount:
                                        if (isPaymentCur1)
                                        {
                                            cciTarget.NewValue = cci.NewValue;
                                            cciTarget.IsInputByUser = true;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                                #endregion Payment
                            }
                            else
                            {
                                if (_cciFxLeg[nextIndex].CciExchangeRate.IsCciOfContainer(cciTarget.ClientId_WithoutPrefix))
                                {
                                    #region Rate
                                    string strkey = _cciFxLeg[nextIndex].CciExchangeRate.CciContainerKey(cciTarget.ClientId_WithoutPrefix);
                                    CciExchangeRate.CciEnum key = CciExchangeRate.CciEnum.unknown;
                                    if (System.Enum.IsDefined(typeof(CciExchangeRate.CciEnum), strkey))
                                        key = (CciExchangeRate.CciEnum)System.Enum.Parse(typeof(CciExchangeRate.CciEnum), strkey);
                                    //
                                    switch (key)
                                    {
                                        case CciExchangeRate.CciEnum.quotedCurrencyPair_quoteBasis:
                                            cciTarget.NewValue = cci.NewValue;
                                            cciTarget.IsInputByUser = true;
                                            break;
                                        case CciExchangeRate.CciEnum.rate:
                                            if (CciFXLeg.TypeFxEnum.FORWARD == _cciFxLeg[nextIndex].TypeFx)
                                            {
                                                _cciFxLeg[nextIndex].CciExchangeRate.Cci(CciExchangeRate.CciEnum.spotRate).NewValue = _cciFxLeg[currentIndex].CciExchangeRate.Cci(CciExchangeRate.CciEnum.rate).NewValue;
                                                _cciFxLeg[nextIndex].CciExchangeRate.Cci(CciExchangeRate.CciEnum.spotRate).IsInputByUser = true;
                                            }
                                            break;
                                        default:
                                            break;
                                    }
                                    #endregion Rate
                                }
                            }
                        }
                    }
                }
            }

        }
        #endregion SynchronizeNextLeg
        #region private GetIndexFxLeg
        private int GetIndexFxLeg(string pClientId_WithoutPrefix)
        {
            int ret = -1;
            for (int i = 0; i < FxLegLength; i++)
            {
                if (_cciFxLeg[i].IsCciOfContainer(pClientId_WithoutPrefix))
                {
                    ret = i;
                    break;
                }
            }
            return ret;

        }
        #endregion GetIndexFxLeg
        #region private GetCciFxLeg
        private CciFXLeg GetCciFxLeg(string pClientId_WithoutPrefix)
        {
            CciFXLeg ret = null;
            int index = GetIndexFxLeg(pClientId_WithoutPrefix);
            if (-1 < index)
                ret = _cciFxLeg[index];
            return ret;

        }
        #endregion GetCciFxLeg
        #endregion

        #region ICciPresentation Membres
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(_cciFxLeg))
            {
                for (int i = 0; i < ArrFunc.Count(_cciFxLeg); i++)
                {
                    _cciFxLeg[i].DumpSpecific_ToGUI(pPage);
                }
            }

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion
    }
    
}
