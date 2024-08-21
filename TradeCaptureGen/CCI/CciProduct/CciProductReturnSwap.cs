#region Using Directives
using System;
using System.Linq;
using System.Collections;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;

using EfsML.Enum;
using EfsML.Business;

using FpML.Interface;

using FixML.Interface;
using FixML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{

    /// <summary>
    /// Description résumée de TradeEquitySwap.
    /// </summary>
    public class CciProductReturnSwap : CciProductBase
    {
        #region Members
        private IReturnSwap _returnSwap;
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
            seller,
            [System.Xml.Serialization.XmlEnumAttribute("earlyTermination")]
            earlyTermination,
            [System.Xml.Serialization.XmlEnumAttribute("extraordinaryEvents")]
            extraordinaryEvents,
            /// <summary>
            /// 
            /// </summary>
            RptSide_Side,
            unknown,
        }
        #endregion

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public ReturnSwapContainer ReturnSwapContainer { get; private set; }
        
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
        
        public int ReturnLegLength
        {
            get { return ArrFunc.IsFilled(CciReturnSwapReturnLeg) ? CciReturnSwapReturnLeg.Length : 0; }
        }
        // EG 20231024 [XXXXX] RTS / Corrections diverses : cciInterestLength
        public int InterestLegLength
        {
            get { return ArrFunc.IsFilled(CciReturnSwapInterestLeg) ? CciReturnSwapInterestLeg.Length : 0; }
        }
        public int AdditionalPaymentLength
        {
            get { return ArrFunc.IsFilled(CciAdditionalPayment) ? CciAdditionalPayment.Length : 0; }
        }

        public CciReturnLeg[] CciReturnSwapReturnLeg { get; private set; }
        public CciInterestLeg[] CciReturnSwapInterestLeg { get; private set; }
        public CciReturnSwapAdditionalPayment[] CciAdditionalPayment { get; private set; }
        #endregion

        #region constructor
        public CciProductReturnSwap(CciTrade pCciTrade, IReturnSwap pReturnSwap, string pPrefix)
            : this(pCciTrade, pReturnSwap, pPrefix, -1)
        { }
        public CciProductReturnSwap(CciTrade pCciTrade, IReturnSwap pReturnSwap, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pReturnSwap, pPrefix, pNumber)
        {
        }
        #endregion constructor

        #region Membres de ITradeCci
        #region  public override RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion RetSidePayer
        #region public override RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion RetSideReceiver
        #region public override GetMainCurrency
        /// <summary>
        /// Return the main currency for a product
        /// </summary>
        /// <returns></returns>
        public override string GetMainCurrency
        {
            get
            {
                return CciReturnSwapReturnLeg[0].GetUnderlyingAssetCurrency;
            }
        }
        #endregion GetMainCurrency
        #region public override CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get
            {
                return CciReturnSwapReturnLeg[0].CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_currency);
            }
        }
        #endregion CciClientIdMainCurrency
        #endregion  Membres de ITrade

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// Obtient le cci cciEnum.buyer
        /// </summary>
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer); }
        }
        /// <summary>
        /// Obtient le cci cciEnum.seller
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

            for (int i = 0; i < ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].SynchronizePayerReceiver(pLastValue, pNewValue);

            for (int i = 0; i < InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].SynchronizePayerReceiver(pLastValue, pNewValue);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].SynchronizePayerReceiver(pLastValue, pNewValue);
        }

        #endregion

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {

            if (ReturnSwapContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.earlyTermination), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.extraordinaryEvents), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].AddCciSystem();

            for (int i = 0; i < this.InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].AddCciSystem();

            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].AddCciSystem();

        }
        
        /// <summary>
        /// 
        /// </summary>
        public override void Initialize_FromCci()
        {

            CciTools.CreateInstance(this, _returnSwap);

            if (null == _returnSwap.BuyerPartyReference)
                _returnSwap.BuyerPartyReference = _returnSwap.CreatePartyOrTradeSideReference;

            if (null == _returnSwap.SellerPartyReference)
                _returnSwap.SellerPartyReference = _returnSwap.CreatePartyOrTradeSideReference;

            if (null == _returnSwap.ExtraordinaryEvents)
                _returnSwap.ExtraordinaryEvents = _returnSwap.CreateExtraordinaryEvents;

            // 1 early termination only
            if (null == _returnSwap.EarlyTermination)
                _returnSwap.EarlyTermination = _returnSwap.CreateEarlyTermination;
            if (null == _returnSwap.EarlyTermination[0].PartyReference)
                _returnSwap.EarlyTermination[0].PartyReference = _returnSwap.CreatePartyReference;
            if (null == _returnSwap.EarlyTermination[0].StartingDate)
                _returnSwap.EarlyTermination[0].StartingDate = _returnSwap.EarlyTermination[0].CreateStartingDate;

            InitializeReturnLeg_FromCci();
            InitializeInterestLeg_FromCci();
            InitializeAdditionalPayment_FromCci();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override void Initialize_Document()
        {
            // FI 20161214 Mise en commentaire 
            //if (Cst.Capture.IsModeInput(ccis.CaptureMode))
            //    _returnSwapContainer.InitRptSide(_cciTrade.CS, CciTradeCommon.TradeCommonInput.IsAllocation);

            // FI 20161214 [21916] call base.InitializeRptSideElement  (harmonisation des produits contenant un RptSide)
            InitializeRptSideElement();
        }

        /// <summary>
        /// 
        /// </summary>
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
                        #region buyer
                        case CciEnum.buyer:
                            data = _returnSwap.BuyerPartyReference.HRef;
                            if (StrFunc.IsEmpty(data) && (null != ((IReturnSwapLeg)_returnSwap.ReturnLeg[0]).ReceiverPartyReference))
                                data = ((IReturnSwapLeg)_returnSwap.ReturnLeg[0]).ReceiverPartyReference.HRef;
                            break;
                        #endregion buyer
                        #region Seller
                        case CciEnum.seller:
                            data = _returnSwap.SellerPartyReference.HRef;
                            if (StrFunc.IsEmpty(data) && (null != ((IReturnSwapLeg)_returnSwap.ReturnLeg[0]).PayerPartyReference))
                                data = ((IReturnSwapLeg)_returnSwap.ReturnLeg[0]).PayerPartyReference.HRef;
                            break;
                        #endregion seller
                        #region Side
                        case CciEnum.RptSide_Side:
                            IFixTrdCapRptSideGrp _rptSide = ReturnSwapContainer.RptSide[0];
                            if (_rptSide.SideSpecified)
                                data = ReflectionTools.ConvertEnumToString<SideEnum>(_rptSide.Side);
                            break;
                        #endregion Side
                        #region earlyTermination
                        case CciEnum.earlyTermination:
                            data = _returnSwap.EarlyTerminationSpecified.ToString().ToLower();
                            break;
                        #endregion
                        #region extraordinaryEvents
                        case CciEnum.extraordinaryEvents:
                            data = _returnSwap.ExtraordinaryEventsSpecified.ToString().ToLower();
                            break;
                        #endregion
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
            //
            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].Initialize_FromDocument();

            for (int i = 0; i < this.InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].Initialize_FromDocument();

            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Initialize_FromDocument();

        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20140925 [XXXXX] Modify
        /// FI 20170116 [21916] Modify
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
                        #region Buyer
                        case CciEnum.buyer:
                            _returnSwap.BuyerPartyReferenceSpecified = cci.IsFilledValue;
                            _returnSwap.BuyerPartyReference.HRef = data;
                            // FI 20170116 [21916] Call RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            RptSideSetBuyerSeller(BuyerSellerEnum.BUYER); 

                            //FI 20140925 [XXXXX] Appel de la méthode ccis.SetFundingAndMargin
                            //Le test _returnSwap.buyerPartyReference.hRef != _returnSwap.sellerPartyReference.hRef est absolument nécessaire
                            //Lorsque l'utilisateur change de sens du trade, il peut y avoir cette égalité 
                            if ((false == CcisBase.IsModeIO) && (_returnSwap.BuyerPartyReference.HRef != _returnSwap.SellerPartyReference.HRef))
                                Ccis.SetFundingAndMargin(CciTradeCommon.CSCacheOn);

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;

                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            _returnSwap.SellerPartyReferenceSpecified = cci.IsFilledValue;
                            _returnSwap.SellerPartyReference.HRef = data;
                            // FI 20170116 [21916] Call RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);

                            //FI 20140925 [XXXXX] Appel de la méthode ccis.SetFundingAndMargin
                            //Le test _returnSwap.buyerPartyReference.hRef != _returnSwap.sellerPartyReference.hRef est absolument nécessaire
                            //Lorsque l'utilisateur change de sens du trade, il peut y avoir cette égalité 
                            if ((false == CcisBase.IsModeIO) && (_returnSwap.BuyerPartyReference.HRef != _returnSwap.SellerPartyReference.HRef))
                                Ccis.SetFundingAndMargin(CciTradeCommon.CSCacheOn);

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Seller
                        #region Side
                        case CciEnum.RptSide_Side:
                            if (ReturnSwapContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = ReturnSwapContainer.RptSide[0];
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

                        #region earlyTermination
                        case CciEnum.earlyTermination:
                            _returnSwap.EarlyTerminationSpecified = cci.IsFilledValue;
                            break;
                        #endregion
                        #region extraordinaryEvents
                        case CciEnum.extraordinaryEvents:
                            _returnSwap.ExtraordinaryEventsSpecified = cci.IsFilledValue;
                            break;
                        #endregion
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

            for (int i = 0; i < ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].Dump_ToDocument();
            _returnSwap.ReturnLegSpecified = CciTools.Dump_IsCciContainerArraySpecified(_returnSwap.ReturnLegSpecified, CciReturnSwapReturnLeg);

            for (int i = 0; i < InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].Dump_ToDocument();
            _returnSwap.InterestLegSpecified = CciTools.Dump_IsCciContainerArraySpecified(_returnSwap.InterestLegSpecified, CciReturnSwapInterestLeg);

            for (int i = 0; i < AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Dump_ToDocument();
            _returnSwap.AdditionalPaymentSpecified = CciTools.Dump_IsCciContainerArraySpecified(_returnSwap.AdditionalPaymentSpecified, CciAdditionalPayment);

            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
                Product.SynchronizeFromDataDocument();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20140805 [XXXXX] Modify
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].DumpSpecific_ToGUI(pPage);

            //Add FI 20140805 
            for (int i = 0; i < this.InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].DumpSpecific_ToGUI(pPage);

            base.DumpSpecific_ToGUI(pPage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        ///FI 20140815 [XXXXX] Modify
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
                    #region Buyer/Seller
                    case CciEnum.buyer:
                        if (ReturnSwapContainer.IsOneSide)
                        {
                            if (null != _returnSwap.SellerPartyReference)
                            {
                                string clientIdSide = CciClientId(CciEnum.RptSide_Side);
                                if (CciTrade.cciParty[0].GetPartyId(true) == _returnSwap.SellerPartyReference.HRef)
                                    CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                                else
                                    CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                            }
                        }
                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.seller:
                        if (ReturnSwapContainer.IsOneSide)
                        {
                            if (null != _returnSwap.BuyerPartyReference)
                            {
                                string clientIdSide = CciClientId(CciEnum.RptSide_Side);
                                if (CciTrade.cciParty[0].GetPartyId(true) == _returnSwap.BuyerPartyReference.HRef)
                                    CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Buy));
                                else
                                    CcisBase.SetNewValue(clientIdSide, ReflectionTools.ConvertEnumToString<SideEnum>(SideEnum.Sell));
                            }
                        }
                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    #endregion
                    case CciEnum.RptSide_Side:
                        if (ReturnSwapContainer.IsOneSide)
                        {
                            IFixTrdCapRptSideGrp _rptSide = ReturnSwapContainer.RptSide[0];
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
            // FI 20140901 [XXXXX] le code est déplacé en fin de méthode => Pas de raison particulière 
            // C'est plus propre à mon sens
            // 
            ////PL 20140723 Test à finaliser... (Code copier depuis la classe CciProductExchangeTradedBase)
            //if ((false == pCci.HasError) && (pCci.IsFilledValue))
            //{
            //    if (ArrFunc.Count(_cciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
            //    {
            //        if (_returnSwapContainer.IsOneSide)
            //        {
            //            #region Préproposition de la contrepartie en fonction du ClearingTemplate
            //            if (_cciTrade.cciParty[1].isInitFromClearingTemplate)
            //            {
            //                if (_cciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
            //                    _cciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
            //                //FI 20140815 [XXXXX] initialisation si l'aaset change
            //                else if ((returnLegLength > 0 &&
            //                        (_cciReturnLeg[0].IsCci(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset, pCci))))
            //                    _cciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
            //            }
            //            #endregion
            //        }
            //    }
            //}

            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].ProcessInitialize(pCci);

            /* FI 20140925 [XXXXX] Mise en commentaire 
             * Ce code est remplacé par l'appel de la méthode TradeCustomCaptureInfos.SetFundingAndMargin
            if ((this.returnLegLength > 0) && _cciReturnLeg[0].IsSetFundingAndMargin)
            {
                _cciReturnLeg[0].IsSetFundingAndMargin = false;

                //PL 20140723 A finaliser...
                string errMsg = null;
                ProcessStateTools.StatusEnum errStatus = ProcessStateTools.StatusEnum.NA;
                Exception exception = null;
                FeeRequest feeRequest = new FeeRequest(_cciTrade.CS, (TradeInput)CciTradeCommon.TradeCommonInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade));
                if (TradeCaptureGen.SetFundingAndMargin(feeRequest, ref errMsg, ref errStatus, ref exception))
                {
                    //Pour forcer la réinitialisation et afficher les données issues des barèmes.
                    CciTradeCommon.TradeCommonInput.CustomCaptureInfos.IsToSynchronizeWithDocument = true;
                }
                feeRequest = null;
            }
            */

            for (int i = 0; i < this.InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].ProcessInitialize(pCci);

            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].ProcessInitialize(pCci);

            if ((pCci.ClientId_WithoutPrefix == CciClientIdPayer) || (pCci.ClientId_WithoutPrefix == CciClientIdReceiver))
                CciTrade.InitializePartySide();


            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(CciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    if (ReturnSwapContainer.IsOneSide)
                    {
                        #region Préproposition de la contrepartie en fonction du ClearingTemplate
                        if (CciTrade.cciParty[1].IsInitFromClearingTemplate)
                        {
                            if (CciTrade.cciParty[0].IsCci(CciTradeParty.CciEnum.book, pCci))
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                            //FI 20140815 [XXXXX] initialisation si l'aaset change
                            else if ((ReturnLegLength > 0 &&
                                    (CciReturnSwapReturnLeg[0].IsCci(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset, pCci))))
                                CciTrade.SetCciClearerOrBrokerFromClearingTemplate(false);
                        }
                        #endregion
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        /// FI 20140708 [XXXXX] Modify
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            //FI 20140708 [XXXXX] Add test sur CciClientIdPayer et CciClientIdReceiver
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);

            if (!isOk)
            {
                for (int i = 0; i < this.ReturnLegLength; i++)
                {
                    isOk = CciReturnSwapReturnLeg[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            if (!isOk)
            {
                for (int i = 0; i < this.InterestLegLength; i++)
                {
                    isOk = CciReturnSwapInterestLeg[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            if (!isOk)
            {
                for (int i = 0; i < this.AdditionalPaymentLength; i++)
                {
                    isOk = CciAdditionalPayment[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            return isOk;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].SetDisplay(pCci);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void CleanUp()
        {
            // Remove EquityLeg
            if (ArrFunc.IsFilled(_returnSwap.ReturnLeg))
            {
                for (int i = _returnSwap.ReturnLeg.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(((IReturnSwapLeg)_returnSwap.ReturnLeg[i]).PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_returnSwap, "returnLeg", i);
                    else
                        _returnSwap.ReturnLeg[i].RateOfReturn.ValuationPriceInterimSpecified = ((null != CciReturnSwapReturnLeg[i].CciValuationPriceInterim) && (CciReturnSwapReturnLeg[i].CciValuationPriceInterim.IsSpecified));
                }
            }
            _returnSwap.ReturnLegSpecified = ArrFunc.IsFilled(_returnSwap.ReturnLeg);

            // Remove interestLeg
            if (ArrFunc.IsFilled(_returnSwap.InterestLeg))
            {
                for (int i = _returnSwap.InterestLeg.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(((IReturnSwapLeg)_returnSwap.InterestLeg[i]).PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_returnSwap, "interestLeg", i);
                }
            }
            _returnSwap.InterestLegSpecified = ArrFunc.IsFilled(_returnSwap.InterestLeg);

            // Remove additionalPayment
            if (ArrFunc.IsFilled(_returnSwap.AdditionalPayment))
            {
                for (int i = _returnSwap.AdditionalPayment.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_returnSwap.AdditionalPayment[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_returnSwap, "additionalPayment", i);
                }
            }
            _returnSwap.AdditionalPaymentSpecified = ArrFunc.IsFilled(_returnSwap.AdditionalPayment);
        }

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            for (int i = 0; i < ReturnLegLength; i++)
            {
                CciReturnSwapReturnLeg[i].SetButtonReferential(pCci, pCo);
            }
        }

        #region Membres de ITradeGetInfoButton
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pCo"></param>
        /// <param name="pIsSpecified"></param>
        /// <param name="pIsEnabled"></param>
        /// <returns></returns>
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.earlyTermination, pCci);
                if (isOk)
                {
                    isOk = true;
                    pCo.Object = "product";
                    pCo.Element = "earlyTermination";
                    pCo.CopyTo = "All";
                    pIsSpecified = _returnSwap.EarlyTerminationSpecified;
                    pIsEnabled = _returnSwap.EarlyTerminationSpecified;
                }
            }
            if (false == isOk)
            {
                isOk = this.IsCci(CciEnum.extraordinaryEvents, pCci);
                if (isOk)
                {
                    isOk = true;
                    pCo.Object = "product";
                    pCo.Element = "extraordinaryEvents";
                    pCo.CopyTo = "All";
                    pIsSpecified = _returnSwap.ExtraordinaryEventsSpecified;
                    pIsEnabled = _returnSwap.ExtraordinaryEventsSpecified;
                }
            }
            if (false == isOk)
            {
                for (int i = 0; i < ReturnLegLength; i++)
                {
                    isOk = CciReturnSwapReturnLeg[i].SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                    if (isOk)
                        break;
                }
            }
            if (false == isOk)
            {
                for (int i = 0; i < InterestLegLength; i++)
                {
                    isOk = CciReturnSwapInterestLeg[i].SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
                    if (isOk)
                        break;
                }
            }
            return isOk;
        }
        #endregion Membres de ITradeGetInfoButton

        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            for (int i = 0; i < ReturnLegLength; i++)
            {
                isOk = CciReturnSwapReturnLeg[i].IsClientId_QuoteBasis(pCci);
                if (isOk)
                    break;
            }
            return isOk;
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            CciReturnLeg returnLeg = GetCciReturnLeg(pCci.ClientId_WithoutPrefix);
            if (null != returnLeg)
                ret = returnLeg.GetCurrency1;
            return ret;
        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            CciReturnLeg returnLeg = GetCciReturnLeg(pCci.ClientId_WithoutPrefix);
            if (null != returnLeg)
                ret = returnLeg.GetCurrency2;
            return ret;
        }
        #endregion
        #endregion

        #region methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProduct"></param>
        public override void SetProduct(IProduct pProduct)
        {
            _returnSwap = (IReturnSwap)pProduct;
            ReturnSwapContainer = new ReturnSwapContainer(_returnSwap, CciTradeCommon.TradeCommonInput.DataDocument);
            if (_returnSwap.ReturnLegSpecified)
            {
                CciReturnSwapReturnLeg = new CciReturnLeg[1]
                {
                    new CciReturnLeg(CciTrade, 1, _returnSwap, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_returnLeg)
                };
            }
            if (_returnSwap.InterestLegSpecified)
            {
                CciReturnSwapInterestLeg = new CciInterestLeg[1]
                {
                    new CciInterestLeg(CciTrade, 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_interestLeg)
                };
            }
            base.SetProduct(pProduct);
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeAdditionalPayment_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified;
            ArrayList lst = new ArrayList();

            SaveSpecified = _returnSwap.AdditionalPaymentSpecified;

            lst.Clear();
            while (isOk)
            {
                index += 1;

                CciReturnSwapAdditionalPayment payment = new CciReturnSwapAdditionalPayment(CciTrade, this, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_additionalPayment,
                                                                    "Marge", String.Empty, string.Empty, CciClientIdMainCurrency);

                isOk = CcisBase.Contains(payment.CciClientId(CciReturnSwapAdditionalPayment.CciEnum.payer)) ||
                       CcisBase.Contains(payment.CciClientId(CciReturnSwapAdditionalPayment.CciEnum.receiver));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_returnSwap.AdditionalPayment) || (index == _returnSwap.AdditionalPayment.Length))
                        ReflectionTools.AddItemInArray(_returnSwap, "additionalPayment", index);
                    payment.Payment = _returnSwap.AdditionalPayment[index];

                    lst.Add(payment);
                }
            }

            CciAdditionalPayment = (CciReturnSwapAdditionalPayment[])lst.ToArray(typeof(CciReturnSwapAdditionalPayment));
            for (int i = 0; i < this.AdditionalPaymentLength; i++)
                CciAdditionalPayment[i].Initialize_FromCci();

            _returnSwap.AdditionalPaymentSpecified = SaveSpecified;
        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeInterestLeg_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool SaveSpecified;
            System.Collections.ArrayList lst = new System.Collections.ArrayList();
            //
            SaveSpecified = _returnSwap.InterestLegSpecified;
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                CciInterestLeg interestLeg = new CciInterestLeg(CciTrade, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_interestLeg);
                isOk = CcisBase.Contains(interestLeg.CciClientId(CciInterestLeg.CciEnum.receiver)) || CcisBase.Contains(interestLeg.CciClientId(CciInterestLeg.CciEnum.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_returnSwap.InterestLeg) || (index == _returnSwap.InterestLeg.Length))
                        ReflectionTools.AddItemInArray(_returnSwap, "interestLeg", index);
                    interestLeg.InterestLeg = _returnSwap.InterestLeg[index];
                    lst.Add(interestLeg);
                }
            }
            CciReturnSwapInterestLeg = (CciInterestLeg[])lst.ToArray(typeof(CciInterestLeg));
            for (int i = 0; i < this.InterestLegLength; i++)
                CciReturnSwapInterestLeg[i].Initialize_FromCci();
            //
            _returnSwap.InterestLegSpecified = SaveSpecified;

        }

        /// <summary>
        /// 
        /// </summary>
        private void InitializeReturnLeg_FromCci()
        {
            bool isOk = true;
            int index = -1;
            bool SaveSpecified;
            ArrayList lst = new ArrayList();
            //
            SaveSpecified = _returnSwap.ReturnLegSpecified;
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                CciReturnLeg retleg = new CciReturnLeg(CciTrade, index + 1, _returnSwap, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_returnLeg);
                isOk = CcisBase.Contains(retleg.CciClientId(CciReturnLeg.CciEnum.payer)) ||
                       CcisBase.Contains(retleg.CciClientId(CciReturnLeg.CciEnum.receiver));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_returnSwap.ReturnLeg) || (index == _returnSwap.ReturnLeg.Length))
                        ReflectionTools.AddItemInArray(_returnSwap, "returnLeg", index);
                    retleg.ReturnLeg = _returnSwap.ReturnLeg[index];
                    //					
                    lst.Add(retleg);
                }
            }
            CciReturnSwapReturnLeg = (CciReturnLeg[])lst.ToArray(typeof(CciReturnLeg));
            for (int i = 0; i < this.ReturnLegLength; i++)
                CciReturnSwapReturnLeg[i].Initialize_FromCci();
            //
            _returnSwap.ReturnLegSpecified = SaveSpecified;


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        private CciReturnLeg GetCciReturnLeg(string pClientId_WithoutPrefix)
        {
            CciReturnLeg cciReturnLeg = null;
            int index = GetIndexReturnLeg(pClientId_WithoutPrefix);
            if (-1 < index)
                cciReturnLeg = this.CciReturnSwapReturnLeg[index];
            return cciReturnLeg;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        private int GetIndexReturnLeg(string pClientId_WithoutPrefix)
        {
            int ret = -1;
            for (int i = 0; i < this.ReturnLegLength; i++)
            {
                if (CciReturnSwapReturnLeg[i].IsCciOfContainer(pClientId_WithoutPrefix))
                {
                    ret = i;
                    break;
                }
            }
            return ret;
        }

        #endregion
    }

}
