#region Using Directives
using System;
using System.Linq;
using System.Reflection;
using System.Collections;


using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.GUI.CCI;
using EFS.GUI.Interface;

using EfsML.Business;
using EfsML.Enum;

using FpML.Enum;
using FpML.Interface;
using FixML.Interface;
using FixML.Enum;
#endregion Using Directives

namespace EFS.TradeInformation
{

    // EG 20180514 [23812] Report
    public class CciProductFXOptionLeg : CciProductBase,  ICciPresentation
    {
        #region Members
        
        private object _fxOptionLeg;
        
        private readonly FxProduct _fxProduct;
        private CciPayment[] _ccifxOptPremium;
        private CciFxFixing[] _cciFixings;	     //Represente les n fixings si option with FxCashSettlement
        private bool _isInitPremiumPayerWithBuyer;

        private FxOptionLegContainer _fxOptionLegContainer;
        private readonly CciEarlyTerminationProvision _cciEarlyTerminationProvision;


        #endregion

        #region Enum
        public enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyer")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("seller")]
            seller,
            #endregion buyer/seller
            #region optionType
            [System.Xml.Serialization.XmlEnumAttribute("optionType")]
            optionType,
            #endregion
            #region expiryDateTime
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryDate")]
            expiryDateTime_expiryDate,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.hourMinuteTime")]
            expiryDateTime_expiryTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.businessCenter")]
            expiryDateTime_expiryTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.cutName")]
            expiryDateTime_cutName,
            #endregion expiryDateTime
            #region exerciseStyle
            [System.Xml.Serialization.XmlEnumAttribute("exerciseStyle")]
            exerciseStyle,
            #endregion
            #region valueDate
            [System.Xml.Serialization.XmlEnumAttribute("valueDate")]
            valueDate,
            #endregion
            #region callCurrencyAmount
            [System.Xml.Serialization.XmlEnumAttribute("callCurrencyAmount.amount")]
            callCurrencyAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("callCurrencyAmount.currency")]
            callCurrencyAmount_currency,
            #endregion callCurrencyAmount
            #region putCurrencyAmount
            [System.Xml.Serialization.XmlEnumAttribute("putCurrencyAmount.amount")]
            putCurrencyAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("putCurrencyAmount.currency")]
            putCurrencyAmount_currency,
            #endregion putCurrencyAmount
            #region fxStrikePrice
            [System.Xml.Serialization.XmlEnumAttribute("fxStrikePrice.rate")]
            fxStrikePrice_rate,
            [System.Xml.Serialization.XmlEnumAttribute("fxStrikePrice.strikeQuoteBasis")]
            fxStrikePrice_strikeQuoteBasis,
            #endregion fxStrikePrice
            #region procedureSpecified
            [System.Xml.Serialization.XmlEnumAttribute("procedure")]
            procedureSpecified,
            #endregion procedureSpecified
            #region cashSettlementTerms
            [System.Xml.Serialization.XmlEnumAttribute("cashSettlementTerms.settlementCurrency")]
            cashSettlementTerms_settlementCurrency,
            #endregion cashSettlementTerms
            RptSide_Side,
            unknown,
        }

        public enum FxProduct
        {
            FxOptionLeg,
            FxBarrierOption,
            FxAverageRateOption
        }
        #endregion Enum
        //
        #region properties
        public FxOptionLegContainer FxOptionLegContainer
        {
            get { return _fxOptionLegContainer; }
        }

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
        
        #region public fxOptionLeg
        public object FxOptionLeg
        {
            get
            {
                return _fxOptionLeg;
            }
        }
        #endregion
        #region public FxOptionPremiumLength
        public int FxOptionPremiumLength
        {
            get { return ArrFunc.IsFilled(_ccifxOptPremium) ? _ccifxOptPremium.Length : 0; }
        }
        #endregion
        #region public CciCountainsCashSettlementTerms
        public bool CciCountainsCashSettlementTerms
        {
            get { return CcisBase.Contains(CciClientId(CciEnum.cashSettlementTerms_settlementCurrency)); }
        }
        #endregion
        #region public FxCashSettlementFixingLength
        public int FxCashSettlementFixingLength
        {
            get { return (this.CciCountainsCashSettlementTerms && ArrFunc.IsFilled(_cciFixings)) ? _cciFixings.Length : 0; }
        }
        #endregion
        #region public isCaptureFilled
        public bool IsCaptureFilled
        {
            get
            {
                return Cci(CciEnum.callCurrencyAmount_amount).IsFilledValue &&
                Cci(CciEnum.putCurrencyAmount_amount).IsFilledValue && 
                Cci(CciEnum.fxStrikePrice_rate).IsFilledValue;
            }
        }
        #endregion
        #region public putCurrencyAmount
        public IMoney PutCurrencyAmount
        {
            get { return ((IFxOptionBase)_fxOptionLeg).PutCurrencyAmount; }
        }
        #endregion
        #region public callCurrencyAmount
        public IMoney CallCurrencyAmount
        {
            get { return ((IFxOptionBase)_fxOptionLeg).CallCurrencyAmount; }
        }
        #endregion
        #region public buyerPartyReference
        public IReference BuyerPartyReference
        {
            get { return ((IFxOptionBase)_fxOptionLeg).BuyerPartyReference; }
        }
        #endregion
        #region public SellerPartyReference
        public IReference SellerPartyReference
        {
            get { return ((IFxOptionBase)_fxOptionLeg).SellerPartyReference; }
        }
        #endregion
        #region  public fxOptionPremium
        public IFxOptionPremium[] FxOptionPremium
        {
            get { return ((IFxOptionBase)_fxOptionLeg).FxOptionPremium; }
        }
        #endregion
        #region public FxStrikePrice
        public IFxStrikePrice FxStrikePrice
        {
            get { return ((IFxOptionBaseNotDigital)_fxOptionLeg).FxStrikePrice; }
        }
        #endregion
        #region public IsInitPremiumPayerWithBuyer
        public bool IsInitPremiumPayerWithBuyer
        {
            get { return _isInitPremiumPayerWithBuyer; }
            set { _isInitPremiumPayerWithBuyer = value; }
        }
        #endregion
        #endregion
        //
        #region Constructors
        public CciProductFXOptionLeg(CciTrade pCciTrade, object pProduct, FxProduct pFxProduct, string pPrefix)
            : this(pCciTrade, pProduct, pFxProduct, pPrefix, -1)
        { }
        // EG 20180514 [23812] Report
        public CciProductFXOptionLeg(CciTrade pCciTrade, object pProduct, FxProduct pFxProduct, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pProduct, pPrefix, pNumber)
        {
            _fxProduct = pFxProduct;
            _cciEarlyTerminationProvision = new CciEarlyTerminationProvision(pCciTrade, pPrefix);
        }
        #endregion Constructors
        
        #region Membres de ITradeCci
        #region public override RetSidePayer
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
            get { return string.Empty; }
        }
        #endregion GetMainCurrency
        #region public override CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get { return string.Empty; } // A revoir
        }
        #endregion CciClientIdMainCurrency
        #endregion Membres de ITrade
        
        #region Membres de IContainerCciPayerReceiver
        #region public override CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer); }
        }
        #endregion CciClientIdPayer
        #region public override CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller); }
        }
        #endregion CciClientIdReceiver
        #region public override SynchronizePayerReceiver
        public override void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {

            CcisBase.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            CcisBase.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
            //
            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].SynchronizePayerReceiver(pLastValue, pNewValue);

        }
        #endregion
        #endregion Membres de IContainerCciPayerReceiver
        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        // EG 20180514 [23812] Report
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxOptionLeg);
            //
            InitializeFxOptionPremium_FromCci();
            //
            if (CciCountainsCashSettlementTerms)
                InitializeFxCashSettlementFixing();

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void AddCciSystem()
        {
            if ((null != _fxOptionLegContainer) && _fxOptionLegContainer.IsOneSide)
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.RptSide_Side), true, TypeData.TypeDataEnum.@string);

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].AddCciSystem();

            for (int i = 0; i < FxCashSettlementFixingLength; i++)
                _cciFixings[i].AddCciSystem();

        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        // EG 20180514 [23812] Report
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
                    FieldInfo fld;
                    #endregion

                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            if (null != BuyerPartyReference)
                                data = BuyerPartyReference.HRef;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            if (null != SellerPartyReference)
                                data = SellerPartyReference.HRef;
                            break;
                        #endregion Seller
                        #region valueDate
                        case CciEnum.valueDate:
                            fld = _fxOptionLeg.GetType().GetField("valueDate");
                            data = ((EFS_Date)fld.GetValue(_fxOptionLeg)).Value;
                            break;
                        #endregion valueDate
                        #region optionType
                        case CciEnum.optionType:
                            if (((bool)(_fxOptionLeg.GetType().GetField("optionTypeSpecified")).GetValue(_fxOptionLeg)))
                            {
                                fld = _fxOptionLeg.GetType().GetField("optionType");
                                data = (fld.GetValue(_fxOptionLeg)).ToString();
                            }
                            break;
                        #endregion
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            data = ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.ExpiryDate.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            data = ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.ExpiryTime.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            data = ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.BusinessCenter;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            if (((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.CutNameSpecified)
                                data = ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.CutName;
                            break;
                        #endregion expiryDateTime
                        #region ExerciseStyle
                        case CciEnum.exerciseStyle:
                            data = ((IFxOptionBaseNotDigital)_fxOptionLeg).ExerciseStyle.ToString();
                            break;
                        #endregion ExerciseStyle
                        #region callCurrencyAmount
                        case CciEnum.callCurrencyAmount_amount:
                            data = CallCurrencyAmount.Amount.Value;
                            if (StrFunc.IsEmpty(CallCurrencyAmount.Id))
                                CallCurrencyAmount.Id = CciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.CALLCURRENCYAMOUNT_REFERENCE, false);
                            break;
                        case CciEnum.callCurrencyAmount_currency:
                            data = CallCurrencyAmount.Currency;
                            break;
                        #endregion PutCurrencyAmount
                        #region PutCurrencyAmount
                        case CciEnum.putCurrencyAmount_amount:
                            data = PutCurrencyAmount.Amount.Value;
                            if (StrFunc.IsEmpty(PutCurrencyAmount.Id))
                                PutCurrencyAmount.Id = CciTrade.DataDocument.GenerateId(TradeCustomCaptureInfos.CCst.PUTCURRENCYAMOUNT_REFERENCE, false);
                            break;
                        case CciEnum.putCurrencyAmount_currency:
                            data = PutCurrencyAmount.Currency;
                            break;
                        #endregion PutCurrencyAmount
                        #region fxStrikePrice
                        case CciEnum.fxStrikePrice_strikeQuoteBasis:
                            data = FxStrikePrice.StrikeQuoteBasis.ToString();
                            break;
                        case CciEnum.fxStrikePrice_rate:
                            data = FxStrikePrice.Rate.Value;
                            break;
                        #endregion fxStrikePrice
                        #region procedureSpecified
                        case CciEnum.procedureSpecified:
                            data = ((IFxOptionBaseNotDigital)_fxOptionLeg).ProcedureSpecified.ToString().ToLower();
                            break;
                        #endregion
                        #region cashSettlementTerms
                        case CciEnum.cashSettlementTerms_settlementCurrency:
                            if (((bool)(_fxOptionLeg.GetType().GetField("cashSettlementTermsSpecified")).GetValue(_fxOptionLeg)))
                            {
                                fld = _fxOptionLeg.GetType().GetField("cashSettlementTerms");
                                data = ((IFxCashSettlement)fld.GetValue(_fxOptionLeg)).SettlementCurrency.Value;
                            }
                            break;
                        #endregion cashSettlementTerms
                        #region Side
                        case CciEnum.RptSide_Side:
                            if (null != _fxOptionLegContainer)
                            {
                                IFixTrdCapRptSideGrp _rptSide = _fxOptionLegContainer.RptSide[0];
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
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
            //
            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].Initialize_FromDocument();
            //
            for (int i = 0; i < FxCashSettlementFixingLength; i++)
            {
                _cciFixings[i].Initialize_FromDocument();
                _cciFixings[i].SetEnabled(_cciFixings[i].IsSpecified);
            }
            //

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Initialize_FromDocument();
        }
        #endregion Initialize_FromDocument
        #region public override Dump_ToDocument
        // EG 20180514 [23812] Report
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
                    FieldInfo fld = null;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region Buyer
                        case CciEnum.buyer:
                            BuyerPartyReference.HRef = data;
                            // FI 20160117 [21916] Appel à RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            if (null != _fxOptionLegContainer)
                                RptSideSetBuyerSeller(BuyerSellerEnum.BUYER);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Buyer
                        #region Seller
                        case CciEnum.seller:
                            SellerPartyReference.HRef = data;
                            // FI 20160117 [21916] Appel à RptSideSetBuyerSeller (harmonisation des produits contenant un RptSide)
                            if (null != _fxOptionLegContainer)
                                RptSideSetBuyerSeller(BuyerSellerEnum.SELLER);

                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Seller
                        #region valueDate
                        case CciEnum.valueDate:
                            fld = _fxOptionLeg.GetType().GetField("valueDate");
                            ((EFS_Date)fld.GetValue(_fxOptionLeg)).Value = data;
                            break;
                        #endregion valueDate
                        #region optionType
                        case CciEnum.optionType:
                            //
                            ((IFxOptionBase)_fxOptionLeg).OptionTypeSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                            {
                                OptionTypeEnum optEnum = (OptionTypeEnum)System.Enum.Parse(typeof(OptionTypeEnum), data, true);
                                ((IFxOptionBase)_fxOptionLeg).OptionType = optEnum;
                            }
                            break;
                        #endregion
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.ExpiryDate.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.ExpiryTime.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.BusinessCenter = data;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.CutNameSpecified = cci.IsFilledValue;
                            if (cci.IsFilledValue)
                                ((IFxOptionBase)_fxOptionLeg).ExpiryDateTime.CutName = data;
                            break;
                        #endregion expiryDateTime
                        #region ExerciseStyle
                        case CciEnum.exerciseStyle:
                            fld = _fxOptionLeg.GetType().GetField("exerciseStyle");
                            ExerciseStyleEnum exEnum = (ExerciseStyleEnum)System.Enum.Parse(typeof(ExerciseStyleEnum), data, true);
                            fld.SetValue(_fxOptionLeg, exEnum);
                            //
                            fld = _fxOptionLeg.GetType().GetField("bermudanExerciseDatesSpecified");
                            fld.SetValue(_fxOptionLeg, (ExerciseStyleEnum.Bermuda == exEnum));
                            break;
                        #endregion
                        #region callCurrencyAmount
                        case CciEnum.callCurrencyAmount_amount:
                            CallCurrencyAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.callCurrencyAmount_currency:
                            CallCurrencyAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region PutCurrencyAmount
                        case CciEnum.putCurrencyAmount_amount:
                            PutCurrencyAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.putCurrencyAmount_currency:
                            PutCurrencyAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        #region Rate
                        case CciEnum.fxStrikePrice_rate:
                            FxStrikePrice.Rate.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer ...
                            break;
                        case CciEnum.fxStrikePrice_strikeQuoteBasis:
                            if (StrFunc.IsFilled(data))
                            {
                                StrikeQuoteBasisEnum QbEnum = (StrikeQuoteBasisEnum)System.Enum.Parse(typeof(StrikeQuoteBasisEnum), data, true);
                                FxStrikePrice.StrikeQuoteBasis = QbEnum;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de recalculer ...
                            }
                            break;
                        #endregion
                        #region procedureSpecified
                        case CciEnum.procedureSpecified:
                            fld = _fxOptionLeg.GetType().GetField("procedureSpecified");
                            fld.SetValue(_fxOptionLeg, cci.IsFilledValue);
                            break;
                        #endregion
                        #region cashSettlementTerms
                        case CciEnum.cashSettlementTerms_settlementCurrency:
                            //
                            fld = _fxOptionLeg.GetType().GetField("cashSettlementTermsSpecified");
                            fld.SetValue(_fxOptionLeg, cci.IsFilledValue);
                            //
                            fld = _fxOptionLeg.GetType().GetField("cashSettlementTerms");
                            ((IFxCashSettlement)fld.GetValue(_fxOptionLeg)).SettlementCurrency.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion
                        #region Side
                        case CciEnum.RptSide_Side:
                            if ((null != _fxOptionLegContainer) && _fxOptionLegContainer.IsOneSide)
                            {
                                IFixTrdCapRptSideGrp _rptSide = _fxOptionLegContainer.RptSide[0];
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
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            //
            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].Dump_ToDocument();

            for (int i = 0; i < FxCashSettlementFixingLength; i++)
                _cciFixings[i].Dump_ToDocument();
            //
            bool fxOptionPremiumSpecified = ((IFxOptionBase)_fxOptionLeg).FxOptionPremiumSpecified;
            fxOptionPremiumSpecified = CciTools.Dump_IsCciContainerArraySpecified(fxOptionPremiumSpecified, _ccifxOptPremium);
            // EG 20160404 Migration vs2013
            if (Cst.Capture.IsModeNewCapture(CcisBase.CaptureMode) || Cst.Capture.IsModeUpdateGen(CcisBase.CaptureMode))
            {
                //Product.SynchronizeExchangeTraded();
                Product.SynchronizeFromDataDocument();
            }

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Dump_ToDocument();

        }

        #endregion Dump_ToDocument
        #region public override ProcessInitialize
        // EG 20180514 [23812] Report
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                bool isCashSettlementTermsSpecified = false;
                FieldInfo fld = _fxOptionLeg.GetType().GetField("cashSettlementTermsSpecified");
                if (null != fld)
                    isCashSettlementTermsSpecified = (bool)fld.GetValue(_fxOptionLeg);
                
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                
                CciCompare[] ccic = null;
                switch (key)
                {
                    #region buyer/seller
                    case CciEnum.buyer:
                        CcisBase.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        //
                        for (int i = 0; i < FxOptionPremiumLength; i++)
                        {
                            CcisBase.Synchronize(_ccifxOptPremium[i].CciClientId(CciPayment.CciEnum.receiver), pCci.LastValue, pCci.NewValue);
                            CcisBase.Synchronize(_ccifxOptPremium[i].CciClientId(CciPayment.CciEnum.payer), pCci.LastValue, pCci.NewValue);
                        }

                        CciTrade.InitializePartySide();
                        break;
                    //
                    case CciEnum.seller:
                        CcisBase.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        for (int i = 0; i < FxOptionPremiumLength; i++)
                        {
                            CcisBase.Synchronize(_ccifxOptPremium[i].CciClientId(CciPayment.CciEnum.receiver), pCci.LastValue, pCci.NewValue);
                            CcisBase.Synchronize(_ccifxOptPremium[i].CciClientId(CciPayment.CciEnum.payer), pCci.LastValue, pCci.NewValue);
                        }
                        CciTrade.InitializePartySide();
                        break;
                    #endregion
                    #region callCurrencyAmount_currency
                    case CciEnum.callCurrencyAmount_currency:
                        bool bFromCallAmount = (CciEnum.callCurrencyAmount_amount == key);
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.callCurrencyAmount_amount), CallCurrencyAmount, bFromCallAmount);

                        // Pre-proposition fixing.currency1 => avec la callCurrencyAmount
                        SetCashSettlementFixingCurrency(true);
                        break;
                    #endregion
                    #region putCurrencyAmount_currency
                    case CciEnum.putCurrencyAmount_currency:
                        bool bFromPutAmount = (CciEnum.putCurrencyAmount_amount == key);
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.putCurrencyAmount_amount), PutCurrencyAmount, bFromPutAmount);
                        // Pre-proposition fixing.currency2 => avec la PutCurrencyAmount
                        SetCashSettlementFixingCurrency(false);
                        break;
                    #endregion
                    #region fxStrikePrice_rate
                    case CciEnum.fxStrikePrice_rate:
                        //
                        if (false == IsCaptureFilled && Cci(CciEnum.fxStrikePrice_rate).IsFilledValue)
                        {
                            // Si Le strike est > 0: Calcul soit de putCurrencyAmount_amount soit callCurrencyAmount_amount
                            ccic = new CciCompare[] {new CciCompare("AmountCur2", Cci(CciEnum.putCurrencyAmount_amount),1), 
													 new CciCompare("AmountCur1" ,Cci(CciEnum.callCurrencyAmount_amount),2)};
                        }
                        //
                        if (ArrFunc.IsFilled(ccic))
                        {
                            //
                            Array.Sort(ccic);
                            //
                            switch (ccic[0].key)
                            {
                                case "AmountCur1":
                                    CalculExchange(false);
                                    break;
                                case "AmountCur2":
                                    CalculExchange(true);
                                    break;
                                case "Rate":
                                    CalculRateFromAmount();
                                    FxStrikePrice.Rate.Value = Cci(CciEnum.fxStrikePrice_rate).NewValue;
                                    break;
                            }
                        }
                        break;
                    #endregion exchangeRate_rate
                    #region fxStrikePrice_strikeQuoteBasis
                    case CciEnum.fxStrikePrice_strikeQuoteBasis:
                        CalculRateFromAmount();
                        SetCashSettlementFixingQuoteBasis();
                        break;
                    #endregion fxStrikePrice_strikeQuoteBasis
                    #region  callCurrencyAmount_amount,putCurrencyAmount_amount
                    case CciEnum.callCurrencyAmount_amount:
                    case CciEnum.putCurrencyAmount_amount:
                        //
                        bool isFromAmountCur1 = (CciClientId(CciEnum.callCurrencyAmount_amount) == pCci.ClientId_WithoutPrefix);
                        string clientIdSource = pCci.ClientId_WithoutPrefix;
                        //
                        string clientIdTarget;
                        if (isFromAmountCur1)
                            clientIdTarget = CciClientId(CciEnum.putCurrencyAmount_amount);
                        else
                            clientIdTarget = CciClientId(CciEnum.callCurrencyAmount_amount);

                        // Si le montant est > 0: Calcul soit de l'autre montant, soit du taux de change
                        if (!IsCaptureFilled && pCci.IsFilledValue)
                        {
                            ccic = new CciCompare[] { new CciCompare("AmountTarget", CcisBase[clientIdTarget], 1), 
                                                      new CciCompare("Rate", Cci(CciEnum.fxStrikePrice_rate), 2) };
                        }
                        if (ArrFunc.IsFilled(ccic))
                        {

                            Array.Sort(ccic);
                            //
                            switch (ccic[0].key)
                            {
                                case "AmountSource":
                                    // Cas particulier où l'utilisateur modifie un champs Montant et c'est ce dernier qui est racalculer => Mise à jour 
                                    CalculExchange((clientIdSource == CciClientId(CciEnum.putCurrencyAmount_amount)));
                                    if (isFromAmountCur1)
                                        CallCurrencyAmount.Amount.Value = Cci(CciEnum.callCurrencyAmount_amount).NewValue;
                                    else
                                        PutCurrencyAmount.Amount.Value = Cci(CciEnum.putCurrencyAmount_amount).NewValue;
                                    break;
                                case "AmountTarget":
                                    CalculExchange((clientIdTarget == CciClientId(CciEnum.putCurrencyAmount_amount)));
                                    break;
                                case "Rate":
                                    CalculRateFromAmount();
                                    break;
                            }
                        }
                        break;
                    #endregion callCurrencyAmount_amount,putCurrencyAmount_amount
                    #region 	cashSettlementTerms_settlementCurrency
                    case CciEnum.cashSettlementTerms_settlementCurrency:
                        if (isCashSettlementTermsSpecified)
                        {
                            SetCashSettlementFixingCurrency(true);
                            SetCashSettlementFixingCurrency(false);
                            SetCashSettlementFixingQuoteBasis();
                            //
                            for (int i = 0; i < this.FxCashSettlementFixingLength; i++)
                            {
                                _cciFixings[i].SetEnabled(true);
                                if (_cciFixings[i].ExistCciAssetFxRate)
                                    Ccis.InitializeCciAssetFxRate(_cciFixings[i].CciClientId(CciFxFixing.CciEnum.assetFxRate), CciClientId(CciEnum.callCurrencyAmount_currency), CciClientId(CciEnum.putCurrencyAmount_currency));
                            }
                        }
                        else
                        {
                            for (int i = 0; i < this.FxCashSettlementFixingLength; i++)
                            {
                                _cciFixings[i].Clear();
                                _cciFixings[i].SetEnabled(false);
                            }
                        }
                        break;
                    #endregion 	cashSettlementTerms_settlementCurrency
                    case CciEnum.RptSide_Side:
                        if ((null != _fxOptionLegContainer) && _fxOptionLegContainer.IsOneSide)
                        {
                            IFixTrdCapRptSideGrp _rptSide = _fxOptionLegContainer.RptSide[0];
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

                    #region default
                    default:

                        break;
                    #endregion default
                }
            }
            
            for (int i = 0; i < FxOptionPremiumLength; i++)
            {
                if ((_ccifxOptPremium[i].IsCci(CciPayment.CciEnumFxOptionPremium.premiumQuote_premiumValue, pCci)) ||
                     (_ccifxOptPremium[i].IsCci(CciPayment.CciEnumFxOptionPremium.premiumQuote_premiumQuoteBasis, pCci)))
                {
                    _ccifxOptPremium[i].SetfxPremiumAmoutFromPremiumQuote(CallCurrencyAmount, PutCurrencyAmount);
                }
                _ccifxOptPremium[i].ProcessInitialize(pCci);
            }
            
            for (int i = 0; i < FxCashSettlementFixingLength; i++)
                _cciFixings[i].ProcessInitialize(pCci);

            if ((false == pCci.HasError) && (pCci.IsFilledValue))
            {
                if (ArrFunc.Count(CciTrade.cciParty) >= 2) //C'est évident mais bon un test de plus
                {
                    if ((null != _fxOptionLegContainer) && _fxOptionLegContainer.IsOneSide)
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

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            //
            if (!isOk)
            {
                for (int i = 0; i < FxOptionPremiumLength; i++)
                {
                    isOk = _ccifxOptPremium[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk)
                        break;
                }
            }
            //
            return isOk;

        }
        #endregion IsClientId_XXXXX
        #region public override CleanUp
        public override void CleanUp()
        {
            #region Fixing
            if (ArrFunc.IsFilled(_cciFixings))
            {
                for (int i = 0; i < _cciFixings.Length; i++)
                    _cciFixings[i].CleanUp();
            }
            #endregion

            #region Premium
            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < _ccifxOptPremium.Length; i++)
                    _ccifxOptPremium[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(FxOptionPremium))
            {
                for (int i = FxOptionPremium.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(FxOptionPremium[i]))
                        ReflectionTools.RemoveItemInArray(_fxOptionLeg, "fxOptionPremium", i);
                }
            }
                //
                ((IFxOptionBase)_fxOptionLeg).FxOptionPremiumSpecified = ArrFunc.IsFilled(FxOptionPremium);
            #endregion Premium

            #region FXCashSettlementFixing
            FieldInfo fld = _fxOptionLeg.GetType().GetField("cashSettlementTermsSpecified");
            bool isSpecified = (null != fld);
            if (isSpecified)
                isSpecified = (bool)fld.GetValue(_fxOptionLeg);
            //				
            if (isSpecified)
            {
                fld = _fxOptionLeg.GetType().GetField("cashSettlementTerms");
                IFxCashSettlement fxCashSettlement = (IFxCashSettlement)fld.GetValue(_fxOptionLeg);
                //
                if (ArrFunc.IsFilled(fxCashSettlement.Fixing))
                {
                    IFxFixing[] fixing = fxCashSettlement.Fixing;
                    for (int i = fixing.Length - 1; -1 < i; i--)
                    {
                        if (false == CaptureTools.IsDocumentElementValid(fixing[i].FixingDate.Value))
                            ReflectionTools.RemoveItemInArray(fxCashSettlement, "fixing", i);
                    }
                }
            }
            #endregion FXCashSettlementFixing
        }
        #endregion
        #region public override SetDisplay
        // EG 20180514 [23812] Report
        public override void SetDisplay(CustomCaptureInfo pCci)
        {

            #region cciFixings
            if (ArrFunc.IsFilled(_cciFixings))
            {
                for (int i = 0; i < _cciFixings.Length; i++)
                    _cciFixings[i].SetDisplay(pCci);
            }
            #endregion
            //			
            #region ccifxOptPremium
            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < _ccifxOptPremium.Length; i++)
                    _ccifxOptPremium[i].SetDisplay(pCci);
            }
            #endregion
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.SetDisplay(pCci);

        }
        #endregion
        #region public override Initialize_Document
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify
        public override void Initialize_Document()
        {

            // FI 20170116 [21916] Mise en commentaire
            //if (Cst.Capture.IsModeInput(ccis.CaptureMode) && (null != _fxOptionLegContainer))
            //    _fxOptionLegContainer.InitRptSide(_cciTrade.CS, CciTradeCommon.TradeCommonInput.IsAllocation);

            // FI 20170116 [21916] call InitializeRptSideElement (harmonisation des produits contenant un RptSide)
            if (null != _fxOptionLegContainer) 
                base.InitializeRptSideElement(); 

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(BuyerPartyReference.HRef) &&
                     StrFunc.IsEmpty(SellerPartyReference.HRef))
                {
                    //HPC est broker ds les template et ne veut pas être 1 contrepartie
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    BuyerPartyReference.HRef = id;
                }
                //
                for (int i = 0; i < FxOptionPremiumLength; i++)
                {
                    if ((_ccifxOptPremium[i].Cci(CciPayment.CciEnum.payer).IsMandatory) && (_ccifxOptPremium[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(FxOptionPremium[i].PayerPartyReference.HRef) &&
                             StrFunc.IsEmpty(FxOptionPremium[i].PayerPartyReference.HRef))
                            FxOptionPremium[i].PayerPartyReference.HRef = BuyerPartyReference.HRef;
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion
        #endregion

        #region Membres de IContainerCciGetInfoButton
        // EG 20180514 [23812] Report
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //
            #region buttons bermuda dates
            if (!isOk)
            {
                isOk = IsCci(CciEnum.exerciseStyle, pCci);
                if (isOk)
                {
                    FieldInfo fld = _fxOptionLeg.GetType().GetField("bermudanExerciseDatesSpecified");
                    bool IsSpecified = (bool)fld.GetValue(_fxOptionLeg);

                    fld = _fxOptionLeg.GetType().GetField("exerciseStyle");
                    ExerciseStyleEnum exerciseStyle = (ExerciseStyleEnum)fld.GetValue(_fxOptionLeg);

                    pCo.Element = "bermudanExerciseDates";
                    pCo.Object = "";
                    pIsSpecified = IsSpecified;
                    pIsEnabled = (exerciseStyle == ExerciseStyleEnum.Bermuda);
                }
            }
            #endregion buttons bermuda dates

            #region Procedure
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.procedureSpecified, pCci);
                if (isOk)
                {
                    FieldInfo fld = _fxOptionLeg.GetType().GetField("procedureSpecified");
                    bool IsSpecified = (bool)fld.GetValue(_fxOptionLeg);

                    pCo.Element = "procedure";
                    pCo.Object = "";
                    pIsSpecified = IsSpecified;
                    pIsEnabled = IsSpecified;
                }
            }
            #endregion buttons bermuda dates

            #region buttons settlementInfo FxOptionPremium
            if (!isOk)
            {
                for (int i = 0; i < FxOptionPremiumLength; i++)
                {
                    isOk = _ccifxOptPremium[i].IsCci(CciPayment.CciEnumPayment.settlementInformation, pCci);
                    if (isOk)
                    {
                        pCo.Element = "settlementInformation";
                        pCo.Object = "fxOptionPremium";
                        pCo.OccurenceValue = i + 1;
                        pIsSpecified = _ccifxOptPremium[i].IsSettlementInfoSpecified;
                        pIsEnabled = _ccifxOptPremium[i].IsSettlementInstructionSpecified;
                        break;
                    }
                }
            }
            #endregion buttons settlementInfo

            #region earlyTerminationProvision
            if (false == isOk)
                isOk = _cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion earlyTerminationProvision

            return isOk;

        }
        #endregion Membres de IContainerCciGetInfoButton
        
        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            
                bool isOk = IsCci(CciEnum.fxStrikePrice_strikeQuoteBasis, pCci);
                //
                if (false == isOk)
                {
                    for (int i = 0; i < FxCashSettlementFixingLength; i++)
                    {
                        isOk = _cciFixings[i].IsClientId_QuoteBasis(pCci);
                        if (isOk)
                            break;
                    }
                }
                //
                if (false == isOk)
                {
                    for (int i = 0; i < FxOptionPremiumLength; i++)
                    {
                        isOk = _ccifxOptPremium[i].IsClientId_QuoteBasis(pCci);
                        if (isOk)
                            break;
                    }
                }
                //
                return isOk;
            
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsCci(CciEnum.fxStrikePrice_strikeQuoteBasis, pCci))
                ret = Cci(CciEnum.callCurrencyAmount_currency).NewValue;
            else
            {
                if (StrFunc.IsEmpty(ret))
                {
                    // Recherche ds les fixings de FxCashSettlement
                    for (int i = 0; i < FxCashSettlementFixingLength; i++)
                    {
                        ret = _cciFixings[i].GetCurrency1(pCci);
                        if (StrFunc.IsFilled(ret))
                            break;
                    }
                }
                //
                if (StrFunc.IsEmpty(ret))
                {
                    for (int i = 0; i < FxOptionPremiumLength; i++)
                    {
                        ret = _ccifxOptPremium[i].GetCurrency1(pCci);
                        if (StrFunc.IsFilled(ret))
                            break;
                    }
                }
            }
            //
            return ret;

        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            if (IsCci(CciEnum.fxStrikePrice_strikeQuoteBasis, pCci))
                ret = Cci(CciEnum.putCurrencyAmount_currency).NewValue;
            else
            {
                if (StrFunc.IsEmpty(ret))
                {
                    // Recherche ds les fixings de FxCashSettlement
                    for (int i = 0; i < FxCashSettlementFixingLength; i++)
                    {
                        ret = _cciFixings[i].GetCurrency2(pCci);
                        if (StrFunc.IsFilled(ret))
                            break;
                    }
                }
                //
                if (StrFunc.IsEmpty(ret))
                {
                    for (int i = 0; i < FxOptionPremiumLength; i++)
                    {
                        ret = _ccifxOptPremium[i].GetCurrency2(pCci);
                        if (StrFunc.IsFilled(ret))
                            break;
                    }
                }
            }
            return ret;

        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis
        
        #region Methods
        #region public SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            switch (_fxProduct)
            {
                case FxProduct.FxOptionLeg:
                    _fxOptionLeg = (IFxOptionLeg)pProduct;
                    _fxOptionLegContainer = new FxOptionLegContainer((IFxOptionLeg)_fxOptionLeg,
                        CciTradeCommon.TradeCommonInput.DataDocument);
                    break;
                case FxProduct.FxBarrierOption:
                    _fxOptionLeg = (IFxBarrierOption)pProduct;
                    break;
                case FxProduct.FxAverageRateOption:
                    _fxOptionLeg = (IFxAverageRateOption)pProduct;
                    break;
            }
            base.SetProduct(pProduct);
        }
        #endregion SetProduct
        //
        #region private InitializeFxOptionPremium_FromCci
        private void InitializeFxOptionPremium_FromCci()
        {
            bool isOk = true;
            int index = -1;
            bool SaveSpecified;
            ArrayList lst = new ArrayList();
            //
            SaveSpecified = ((IFxOptionBase)_fxOptionLeg).FxOptionPremiumSpecified;
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                CciPayment ccipayment =
                    new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.FxOptionPremium, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxOptionPremium, string.Empty, CciClientIdReceiver, string.Empty, string.Empty, string.Empty);
                //				    
                isOk = CcisBase.Contains(ccipayment.CciClientId(CciPayment.CciEnumPayment.payer)) | CcisBase.Contains(ccipayment.CciClientId(CciPayment.CciEnumFxOptionPremium.premiumAmount_amount));
                if (isOk)
                {
                    IFxOptionPremium[] fxOptionPremium = ((IFxOptionBase)_fxOptionLeg).FxOptionPremium;
                    //
                    if (ArrFunc.IsEmpty(fxOptionPremium) || (index == fxOptionPremium.Length))
                    {
                        ReflectionTools.AddItemInArray(_fxOptionLeg, "fxOptionPremium", index);
                        fxOptionPremium = ((IFxOptionBase)_fxOptionLeg).FxOptionPremium;
                    }
                    //
                    if (IsInitPremiumPayerWithBuyer)
                    {
                        if (false == CaptureTools.IsDocumentElementValid(fxOptionPremium[index].PayerPartyReference) && (null != BuyerPartyReference))
                            fxOptionPremium[index].PayerPartyReference = Product.ProductBase.CreatePartyOrAccountReference(BuyerPartyReference.HRef);
                        //
                        if (false == CaptureTools.IsDocumentElementValid(fxOptionPremium[index].ReceiverPartyReference) && (null != SellerPartyReference))
                            fxOptionPremium[index].ReceiverPartyReference = Product.ProductBase.CreatePartyOrAccountReference(SellerPartyReference.HRef);
                    }
                    //                        
                    ccipayment.Payment = fxOptionPremium[index];
                    //
                    lst.Add(ccipayment);
                }
            }
            //
            _ccifxOptPremium = null;
            _ccifxOptPremium = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].Initialize_FromCci();
            //			
            ((IFxOptionBase)_fxOptionLeg).FxOptionPremiumSpecified = SaveSpecified;

        }
        #endregion
        #region private InitializeFxCashSettlementFixing
        private void InitializeFxCashSettlementFixing()
        {
            int index = -1;
            ArrayList lst = new ArrayList();

            bool isOk = CciCountainsCashSettlementTerms;

            lst.Clear();
            while (isOk)
            {
                index += 1;
                CciFxFixing cciFxfixing = new CciFxFixing(CciTrade, index + 1, null, Prefix + "cashSettlementTerms" + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_fixing);
                isOk = CcisBase.Contains(cciFxfixing.CciClientId(CciFxFixing.CciEnum.fixingDate));
                if (isOk)
                {
                    FieldInfo fld = _fxOptionLeg.GetType().GetField("cashSettlementTerms");
                    IFxCashSettlement fxCashSettlement = (IFxCashSettlement)fld.GetValue(_fxOptionLeg);
                    IFxFixing[] fxFixing = fxCashSettlement.Fixing;
                    if (ArrFunc.IsEmpty(fxFixing) || (index == fxFixing.Length))
                        ReflectionTools.AddItemInArray(fxCashSettlement, "fixing", index);
                    cciFxfixing.Fixing = fxCashSettlement.Fixing[index];
                    //
                    lst.Add(cciFxfixing);
                }
            }
            _cciFixings = null;
            _cciFixings = new CciFxFixing[lst.Count];
            for (int i = 0; i < this.FxCashSettlementFixingLength; i++)
            {
                _cciFixings[i] = (CciFxFixing)lst[i];
                _cciFixings[i].Initialize_FromCci();
            }
        }
        #endregion
        #region private CalculExchange
        private void CalculExchange(bool pIsCalculAmountCur2)
        {
            try
            {
                string cur1 = CallCurrencyAmount.Currency;
                string cur2 = PutCurrencyAmount.Currency;
                decimal rate = FxStrikePrice.Rate.DecValue;
                QuoteBasisEnum QbEnum = QuoteBasisEnum.Currency1PerCurrency2;
                if (StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency == FxStrikePrice.StrikeQuoteBasis)
                    QbEnum = QuoteBasisEnum.Currency2PerCurrency1;
                //
                decimal amount = 0;

                if (pIsCalculAmountCur2)
                    amount = CallCurrencyAmount.Amount.DecValue;
                else
                    amount = PutCurrencyAmount.Amount.DecValue;
                //
                if (pIsCalculAmountCur2)
                {
                    EFS_Cash cc2 = new EFS_Cash(CciTrade.CSCacheOn, cur1, cur2, amount, rate, QbEnum);
                    Cci(CciEnum.putCurrencyAmount_amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(cc2.ExchangeAmountRounded);
                }
                else
                {
                    EFS_Cash cc1 = new EFS_Cash(CciTrade.CSCacheOn, cur2, cur1, amount, 1 / rate, QbEnum);
                    Cci(CciEnum.callCurrencyAmount_amount).NewValue = StrFunc.FmtDecimalToInvariantCulture(cc1.ExchangeAmountRounded);
                }
            }
            catch (DivideByZeroException) {/*donn't suppress*/}
            catch (Exception) { throw; }
        }
        #endregion
        #region private CalculRateFromAmount
        private void CalculRateFromAmount()
        {
            try
            {
                decimal amountA = CallCurrencyAmount.Amount.DecValue;
                string curA = CallCurrencyAmount.Currency;
                decimal amountB = PutCurrencyAmount.Amount.DecValue;
                string curB = PutCurrencyAmount.Currency;
                //	
                EFS_Cash calc = new EFS_Cash(CciTrade.CSCacheOn, curA, curB, amountA, 1 / amountB, QuoteBasisEnum.Currency2PerCurrency1);
                decimal rate = 0;
                if ((FxStrikePrice.StrikeQuoteBasis == StrikeQuoteBasisEnum.PutCurrencyPerCallCurrency) && (calc.ExchangeAmount != 0))
                    rate = 1 / calc.ExchangeAmount;
                else
                    rate = calc.ExchangeAmount;
                Cci(CciEnum.fxStrikePrice_rate).NewValue = StrFunc.FmtDecimalToInvariantCulture(rate);
                //				
            }
            catch (DivideByZeroException) { }
            catch (Exception) { throw; }
        }
        #endregion
        #region private SetCashSettlementFixingQuoteBasis
        private void SetCashSettlementFixingQuoteBasis()
        {
            string quoteBasis;
            if (FxStrikePrice.StrikeQuoteBasis == StrikeQuoteBasisEnum.CallCurrencyPerPutCurrency)
                quoteBasis = QuoteBasisEnum.Currency1PerCurrency2.ToString();
            else
                quoteBasis = QuoteBasisEnum.Currency2PerCurrency1.ToString();

            for (int i = 0; i < FxCashSettlementFixingLength; i++)
                CcisBase.SetNewValue(_cciFixings[i].CciClientId(CciFxFixing.CciEnum.quotedCurrencyPair_quoteBasis), quoteBasis);
        }
        #endregion SetCashSettlementFixingQuoteBasis
        #region private SetCashSettlementFixingCurrency
        private void SetCashSettlementFixingCurrency(bool pIsCurrency1)
        {
            bool isCashSettlementTermsSpecified = false;
            FieldInfo fld = _fxOptionLeg.GetType().GetField("cashSettlementTermsSpecified");
            if (null != fld)
                isCashSettlementTermsSpecified = (bool)fld.GetValue(_fxOptionLeg);

            if (isCashSettlementTermsSpecified && this.CciCountainsCashSettlementTerms && this.FxCashSettlementFixingLength > 0)
            {
                CciFxFixing.CciEnum cciEnumCurrency;
                if (pIsCurrency1)
                {
                    cciEnumCurrency = CciFxFixing.CciEnum.quotedCurrencyPair_currencyA;
                    CcisBase.SetNewValue(_cciFixings[0].CciClientId(cciEnumCurrency), CallCurrencyAmount.Currency);
                }
                else
                {
                    cciEnumCurrency = CciFxFixing.CciEnum.quotedCurrencyPair_currencyB;
                    CcisBase.SetNewValue(_cciFixings[0].CciClientId(cciEnumCurrency), PutCurrencyAmount.Currency);
                }
            }
        }
        #endregion
        //
        #endregion Methods

        #region ICciPresentation Membres

        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(_cciFixings))
            {
                for (int i = 0; i < ArrFunc.Count(_cciFixings); i++)
                    _cciFixings[i].DumpSpecific_ToGUI(pPage);
            }

            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < ArrFunc.Count(_ccifxOptPremium); i++)
                    _ccifxOptPremium[i].DumpSpecific_ToGUI(pPage);
            }

            base.DumpSpecific_ToGUI(pPage);
        }


        

        #endregion
    }

}
