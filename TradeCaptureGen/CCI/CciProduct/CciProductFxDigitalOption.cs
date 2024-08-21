#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Business;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    // EG 20180514 [23812] Report
    public class CciProductFXDigitalOption : CciProductBase
    {
        #region Members
        
        private IFxDigitalOption _fxOpt;
        
        private CciPayment[] _ccifxOptPremium;
        private CciFxTrigger[] _ccifxTrigger;
        private CciFxBarrier[] _ccifxBarrier;
        private readonly CciEarlyTerminationProvision _cciEarlyTerminationProvision;
        #endregion Members
        //
        #region Enum
        public enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
            seller,
            #endregion buyer/seller
            #region expiryDateTime
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryDate.")]
            expiryDateTime_expiryDate,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.hourMinuteTime")]
            expiryDateTime_expiryTime_hourMinuteTime,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.expiryTime.businessCenter")]
            expiryDateTime_expiryTime_businessCenter,
            [System.Xml.Serialization.XmlEnumAttribute("expiryDateTime.cutName")]
            expiryDateTime_cutName,
            #endregion expiryDateTime
            #region valueDate
            [System.Xml.Serialization.XmlEnumAttribute("valueDate")]
            valueDate,
            #endregion valueDate
            #region quotedCurrencyPair
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.quoteBasis")]
            quotedCurrencyPair_quoteBasis,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency1")]
            quotedCurrencyPair_currencyA,
            [System.Xml.Serialization.XmlEnumAttribute("quotedCurrencyPair.currency2")]
            quotedCurrencyPair_currencyB,
            #endregion quotedCurrencyPair
            #region spotRate
            [System.Xml.Serialization.XmlEnumAttribute("spotRate")]
            spotRate,
            #endregion spotRate
            #region triggerPayout
            [System.Xml.Serialization.XmlEnumAttribute("triggerPayout.amount")]
            triggerPayout_amount,
            [System.Xml.Serialization.XmlEnumAttribute("triggerPayout.currency")]
            triggerPayout_currency,
            [System.Xml.Serialization.XmlEnumAttribute("triggerPayout.payoutStyle")]
            triggerPayout_payoutStyle,
            [System.Xml.Serialization.XmlEnumAttribute("triggerPayout.settlementInformation")]
            triggerPayout_settlementInformation,
            #endregion triggerPayout
            #region Triggers
            fxEuropeanTriggerSpecified,
            fxAmericanTriggerSpecified,
            fxTrigger,
            #endregion triggerPayout
            unknown,
        }
        #endregion Enum
        //
        #region Public property
        public int FxOptionPremiumLength
        {
            get { return ArrFunc.IsFilled(_ccifxOptPremium) ? _ccifxOptPremium.Length : 0; }
        }
        public int FxTriggerLength
        {
            get { return ArrFunc.IsFilled(_ccifxTrigger) ? _ccifxTrigger.Length : 0; }
        }
        public int FxBarrierLength
        {
            get { return ArrFunc.IsFilled(_ccifxBarrier) ? _ccifxBarrier.Length : 0; }
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
        #endregion  public property
        //
        #region Constructors
        public CciProductFXDigitalOption(CciTrade pCciTrade, IFxDigitalOption pFxDigital, string pPrefix)
            : this(pCciTrade, pFxDigital, pPrefix, -1)
        { }
        // EG 20180514 [23812] Report
        public CciProductFXDigitalOption(CciTrade pCciTrade, IFxDigitalOption pFxDigital, string pPrefix, int pNumber)
            : base((CciTradeCommonBase)pCciTrade, (IProduct)pFxDigital, pPrefix, pNumber)
        {
            _cciEarlyTerminationProvision = new CciEarlyTerminationProvision(CciTrade, pPrefix);
        }
        #endregion Constructors

        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        // EG 20180514 [23812] Report
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxOpt);
            // Premium
            InitializeFxOptionPremium_FromCci();
            // triggers
            InitializeTriggerEuropean_FromCci();
            if (FxTriggerLength == 0)
                InitializeTriggerAmerican_FromCci();
            // Barriers
            InitializeBarrier_FromCci();

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

            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.buyer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.seller), true, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].AddCciSystem();

            for (int i = 0; i < FxTriggerLength; i++)
                _ccifxTrigger[i].AddCciSystem();

            for (int i = 0; i < FxBarrierLength; i++)
                _ccifxBarrier[i].AddCciSystem();

            if (CcisBase.Contains(CciClientId(CciEnum.fxEuropeanTriggerSpecified)) || CcisBase.Contains(CciClientId(CciEnum.fxAmericanTriggerSpecified)))
            {
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.fxEuropeanTriggerSpecified), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(CcisBase, Cst.DDL + CciClientId(CciEnum.fxAmericanTriggerSpecified), false, TypeData.TypeDataEnum.@bool);
                CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.fxTrigger), false, TypeData.TypeDataEnum.@string);
            }
        }

        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        // EG 20180514 [23812] Report
        public override void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region Buyer/Seller
                        case CciEnum.buyer:
                            data = _fxOpt.BuyerPartyReference.HRef;
                            break;
                        case CciEnum.seller:
                            data = _fxOpt.SellerPartyReference.HRef;
                            break;
                        #endregion Buyer/Seller
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            data = _fxOpt.ExpiryDateTime.ExpiryDate.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            data = _fxOpt.ExpiryDateTime.ExpiryTime.Value;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            data = _fxOpt.ExpiryDateTime.BusinessCenter;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            if (_fxOpt.ExpiryDateTime.CutNameSpecified)
                                data = _fxOpt.ExpiryDateTime.CutName;
                            break;
                        #endregion expiryDateTime
                        #region valueDate
                        case CciEnum.valueDate:
                            data = _fxOpt.ValueDate.Value;
                            break;
                        #endregion valueDate
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            data = _fxOpt.QuotedCurrencyPair.QuoteBasis.ToString();
                            break;
                        case CciEnum.quotedCurrencyPair_currencyA:
                            data = _fxOpt.QuotedCurrencyPair.Currency1;
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            data = _fxOpt.QuotedCurrencyPair.Currency2;
                            break;
                        #endregion quotedCurrencyPair
                        #region spotRate
                        case CciEnum.spotRate:
                            if (_fxOpt.SpotRateSpecified)
                                data = _fxOpt.SpotRate.Value;
                            break;
                        #endregion spotRate
                        #region triggerPayout
                        case CciEnum.triggerPayout_amount:
                            data = _fxOpt.TriggerPayout.Amount.Value;
                            break;
                        case CciEnum.triggerPayout_currency:
                            data = _fxOpt.TriggerPayout.Currency;
                            break;
                        case CciEnum.triggerPayout_payoutStyle:
                            data = _fxOpt.TriggerPayout.PayoutStyle.ToString();
                            break;
                        case CciEnum.triggerPayout_settlementInformation:
                            // 20090403 RD 16536
                            if (null == _fxOpt.TriggerPayout.SettlementInformation || (false == _fxOpt.TriggerPayout.SettlementInformationSpecified))
                                data = Cst.SettlementTypeEnum.None.ToString();
                            else if (_fxOpt.TriggerPayout.SettlementInformation.StandardSpecified)
                                data = _fxOpt.TriggerPayout.SettlementInformation.Standard.ToString();
                            else if (_fxOpt.TriggerPayout.SettlementInformation.InstructionSpecified)
                                data = Cst.SettlementInformationType.Instruction.ToString();
                            else
                                data = Cst.SettlementTypeEnum.None.ToString();
                            break;
                        #endregion triggerPayout
                        #region triggers
                        case CciEnum.fxEuropeanTriggerSpecified:
                            data = _fxOpt.TypeTriggerEuropeanSpecified.ToString().ToLower();
                            break;
                        case CciEnum.fxAmericanTriggerSpecified:
                            data = _fxOpt.TypeTriggerAmericanSpecified.ToString().ToLower();
                            break;
                        #endregion trigger
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
            for (int i = 0; i < FxTriggerLength; i++)
                _ccifxTrigger[i].Initialize_FromDocument();
            //
            for (int i = 0; i < FxBarrierLength; i++)
                _ccifxBarrier[i].Initialize_FromDocument();

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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        #region Buyer/seller
                        case CciEnum.buyer:
                            _fxOpt.BuyerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            break;
                        case CciEnum.seller:
                            _fxOpt.SellerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de recalculer les BCs
                            break;
                        #endregion Buyer/seller
                        #region expiryDateTime
                        case CciEnum.expiryDateTime_expiryDate:
                            _fxOpt.ExpiryDateTime.ExpiryDate.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_hourMinuteTime:
                            _fxOpt.ExpiryDateTime.ExpiryTime.Value = data;
                            break;
                        case CciEnum.expiryDateTime_expiryTime_businessCenter:
                            _fxOpt.ExpiryDateTime.BusinessCenter = data;
                            break;
                        case CciEnum.expiryDateTime_cutName:
                            _fxOpt.ExpiryDateTime.CutNameSpecified = cci.IsFilledValue;
                            if (_fxOpt.ExpiryDateTime.CutNameSpecified)
                                _fxOpt.ExpiryDateTime.CutName = data;
                            break;
                        #endregion expiryDateTime
                        #region valueDate
                        case CciEnum.valueDate:
                            _fxOpt.ValueDate.Value = data;
                            break;
                        #endregion valueDate
                        #region quotedCurrencyPair
                        case CciEnum.quotedCurrencyPair_quoteBasis:
                            QuoteBasisEnum QbEnum = (QuoteBasisEnum)System.Enum.Parse(typeof(QuoteBasisEnum), data, true);
                            _fxOpt.QuotedCurrencyPair.QuoteBasis = QbEnum;
                            DumpTriggerquotedCurrencyPair();
                            break;
                        case CciEnum.quotedCurrencyPair_currencyA:
                            _fxOpt.QuotedCurrencyPair.Currency1 = data;
                            DumpTriggerquotedCurrencyPair();
                            break;
                        case CciEnum.quotedCurrencyPair_currencyB:
                            _fxOpt.QuotedCurrencyPair.Currency2 = data;
                            DumpTriggerquotedCurrencyPair();
                            break;
                        #endregion quotedCurrencyPair
                        #region spotRate
                        case CciEnum.spotRate:
                            _fxOpt.SpotRateSpecified = cci.IsFilledValue;
                            _fxOpt.SpotRate.Value = data;
                            break;
                        #endregion spotRate
                        #region triggerPayout
                        case CciEnum.triggerPayout_payoutStyle:
                            PayoutEnum payoutEnum = (PayoutEnum)System.Enum.Parse(typeof(PayoutEnum), data, true);
                            _fxOpt.TriggerPayout.PayoutStyle = payoutEnum;
                            break;
                        case CciEnum.triggerPayout_amount:
                            _fxOpt.TriggerPayout.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.triggerPayout_currency:
                            _fxOpt.TriggerPayout.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.triggerPayout_settlementInformation:
                            ISettlementInformation stl = _fxOpt.TriggerPayout.SettlementInformation;
                            bool isSettlementInformationSpecified = (Cst.SettlementTypeEnum.None.ToString() != data) && StrFunc.IsFilled(data);
                            //
                            stl.StandardSpecified = false;
                            stl.InstructionSpecified = false;
                            _fxOpt.TriggerPayout.SettlementInformationSpecified = isSettlementInformationSpecified;
                            //	
                            if (isSettlementInformationSpecified)
                            {
                                stl.StandardSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() != data);
                                stl.InstructionSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() == data);
                                //
                                if (stl.StandardSpecified)
                                    stl.Standard = (StandardSettlementStyleEnum)System.Enum.Parse(typeof(StandardSettlementStyleEnum), data);
                            }
                            break;
                        #endregion triggerPayout
                        #region triggers
                        case CciEnum.fxEuropeanTriggerSpecified:
                            _fxOpt.TypeTriggerEuropeanSpecified = cci.IsFilledValue;
                            DumpTriggerquotedCurrencyPair();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.fxAmericanTriggerSpecified:
                            _fxOpt.TypeTriggerAmericanSpecified = cci.IsFilledValue;
                            DumpTriggerquotedCurrencyPair();
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion trigger
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
            _fxOpt.FxOptionPremiumSpecified = CciTools.Dump_IsCciContainerArraySpecified(_fxOpt.FxOptionPremiumSpecified, _ccifxOptPremium);
            //
            for (int i = 0; i < this.FxTriggerLength; i++)
                _ccifxTrigger[i].Dump_ToDocument();
            //
            if (ArrFunc.IsFilled(_ccifxTrigger))
            {
                if (_ccifxTrigger[0].IsAmerican)
                {
                    _fxOpt.TypeTriggerEuropeanSpecified = false;
                    _fxOpt.TypeTriggerAmericanSpecified = CciTools.Dump_IsCciContainerArraySpecified(_fxOpt.TypeTriggerAmericanSpecified, _ccifxTrigger);
                }
                else
                {
                    _fxOpt.TypeTriggerAmericanSpecified = false;
                    _fxOpt.TypeTriggerEuropeanSpecified = CciTools.Dump_IsCciContainerArraySpecified(_fxOpt.TypeTriggerEuropeanSpecified, _ccifxTrigger);
                }
                //
                CcisBase.SetNewValue(CciClientId(CciEnum.fxAmericanTriggerSpecified), _fxOpt.TypeTriggerAmericanSpecified.ToString());
                CcisBase.SetNewValue(CciClientId(CciEnum.fxEuropeanTriggerSpecified), _fxOpt.TypeTriggerEuropeanSpecified.ToString());
            }
            //
            for (int i = 0; i < this.FxBarrierLength; i++)
                _ccifxBarrier[i].Dump_ToDocument();
            _fxOpt.FxBarrierSpecified = CciTools.Dump_IsCciContainerArraySpecified(_fxOpt.FxBarrierSpecified, _ccifxBarrier);
            //
            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.Dump_ToDocument();

        }
        #endregion
        #region public override ProcessInitialize
        // EG 20180514 [23812] Report
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {

            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //		
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                //
                switch (key)
                {
                    #region quotedCurrencyPair
                    case CciEnum.quotedCurrencyPair_quoteBasis:
                        break;
                    case CciEnum.quotedCurrencyPair_currencyA:
                        break;
                    case CciEnum.quotedCurrencyPair_currencyB:
                        break;
                    #endregion quotedCurrencyPair
                    //
                    #region triggerPayout
                    case CciEnum.triggerPayout_amount:
                    case CciEnum.triggerPayout_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.triggerPayout_amount),
                            _fxOpt.TriggerPayout.Amount, _fxOpt.TriggerPayout.Currency, (CciEnum.triggerPayout_amount == key));
                        break;
                    #endregion
                    //
                    #region trigger
                    case CciEnum.fxEuropeanTriggerSpecified:
                        if (Cci(CciEnum.fxEuropeanTriggerSpecified).NewValue == "true")
                            Cci(CciEnum.fxAmericanTriggerSpecified).NewValue = "false";
                        else
                            Cci(CciEnum.fxAmericanTriggerSpecified).NewValue = "true";
                        break;
                    case CciEnum.fxAmericanTriggerSpecified:
                        if (Cci(CciEnum.fxAmericanTriggerSpecified).NewValue == "true")
                            Cci(CciEnum.fxEuropeanTriggerSpecified).NewValue = "false";
                        else
                            Cci(CciEnum.fxEuropeanTriggerSpecified).NewValue = "true";
                        break;
                    #endregion trigger
                    //
                    #region Default
                    default:

                        break;
                        #endregion Default
                }
            }
            //
            for (int i = 0; i < FxOptionPremiumLength; i++)
                _ccifxOptPremium[i].ProcessInitialize(pCci);
            //
            for (int i = 0; i < FxTriggerLength; i++)
                _ccifxTrigger[i].ProcessInitialize(pCci);
            //
            for (int i = 0; i < FxBarrierLength; i++)
                _ccifxBarrier[i].ProcessInitialize(pCci);

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.ProcessInitialize(pCci);


        }
        #endregion ProcessInitialize
        #region public override IsClientId_PayerOrReceiver
        public override bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            //
            for (int i = 0; i < this.FxOptionPremiumLength; i++)
            {
                isOk = _ccifxOptPremium[i].IsClientId_PayerOrReceiver(pCci);
                if (isOk) break;
            }
            //
            if (!isOk)
            {
                for (int i = 0; i < this.FxBarrierLength; i++)
                {
                    isOk = _ccifxBarrier[i].IsClientId_PayerOrReceiver(pCci);
                    if (isOk) break;
                }
            }
            //
            return isOk;
        }
        #endregion IsClientId_XXX
        #region public override CleanUp
        public override void CleanUp()
        {
            #region Premium
            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < _ccifxOptPremium.Length; i++)
                    _ccifxOptPremium[i].CleanUp();
            }
            //
            if (ArrFunc.IsFilled(_fxOpt.FxOptionPremium))
            {
                for (int i = _fxOpt.FxOptionPremium.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_fxOpt.FxOptionPremium[i].PayerPartyReference.HRef))
                        ReflectionTools.RemoveItemInArray(_fxOpt, "fxOptionPremium", i);
                }
            }
            _fxOpt.FxOptionPremiumSpecified = (ArrFunc.IsFilled(_fxOpt.FxOptionPremium));
            #endregion Prpemium
            //
            #region Trigger
            if (ArrFunc.IsFilled(_ccifxTrigger))
            {
                for (int i = 0; i < _ccifxTrigger.Length; i++)
                    _ccifxTrigger[i].CleanUp();
            }
            //typeTriggerEuropean
            if (ArrFunc.IsFilled(_fxOpt.TypeTriggerEuropean))
            {
                for (int i = _fxOpt.TypeTriggerEuropean.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_fxOpt.TypeTriggerEuropean[i].TriggerRate))
                        ReflectionTools.RemoveItemInArray(_fxOpt, "typeTriggerEuropean", i);
                }
            }
            _fxOpt.TypeTriggerEuropeanSpecified = (ArrFunc.IsFilled(_fxOpt.TypeTriggerEuropean));
            //
            //typeTriggerAmerican
            if (ArrFunc.IsFilled(_fxOpt.TypeTriggerAmerican))
            {
                for (int i = _fxOpt.TypeTriggerAmerican.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_fxOpt.TypeTriggerAmerican[i].TriggerRate))
                        ReflectionTools.RemoveItemInArray(_fxOpt, "typeTriggerAmerican", i);
                }
            }
            _fxOpt.TypeTriggerAmericanSpecified = (ArrFunc.IsFilled(_fxOpt.TypeTriggerAmerican));
            #endregion Trigger
            //
            #region Barrier
            if (ArrFunc.IsFilled(_ccifxBarrier))
            {
                for (int i = 0; i < _ccifxBarrier.Length; i++)
                    _ccifxBarrier[i].CleanUp();
            }
            if (ArrFunc.IsFilled(_fxOpt.FxBarrier))
            {
                for (int i = _fxOpt.FxBarrier.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_fxOpt.FxBarrier[i].TriggerRate))
                        ReflectionTools.RemoveItemInArray(_fxOpt, "fxBarrier", i);
                }
            }
            _fxOpt.FxBarrierSpecified = (ArrFunc.IsFilled(_fxOpt.FxBarrier));
            #endregion Barrier
        }
        #endregion CleanUp
        #region public override SetDisplay
        // EG 20180514 [23812] Report
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < _ccifxOptPremium.Length; i++)
                    _ccifxOptPremium[i].SetDisplay(pCci);
            }

            if (ArrFunc.IsFilled(_ccifxTrigger))
            {
                for (int i = 0; i < _ccifxTrigger.Length; i++)
                    _ccifxTrigger[i].SetDisplay(pCci);
            }

            if (ArrFunc.IsFilled(_ccifxBarrier))
            {
                for (int i = 0; i < _ccifxBarrier.Length; i++)
                    _ccifxBarrier[i].SetDisplay(pCci);
            }

            if (null != _cciEarlyTerminationProvision)
                _cciEarlyTerminationProvision.SetDisplay(pCci);
        }
        #endregion CleanUp
        #region public override Initialize_Document
        public override void Initialize_Document()
        {

            if (Cst.Capture.IsModeNew(CcisBase.CaptureMode) && (false == CcisBase.IsPreserveData))
            {
                string id = string.Empty;
                //
                if (StrFunc.IsEmpty(_fxOpt.BuyerPartyReference.HRef) &&
                     StrFunc.IsEmpty(_fxOpt.SellerPartyReference.HRef))
                {
                    id = CciTrade.GetIdFirstPartyCounterparty();
                    id = StrFunc.IsFilled(id) ? id : TradeCustomCaptureInfos.PartyUnknown;
                    _fxOpt.BuyerPartyReference.HRef = id;
                }

                for (int i = 0; i < FxOptionPremiumLength; i++)
                {
                    if ((_ccifxOptPremium[i].Cci(CciPayment.CciEnum.payer).IsMandatory) &&
                        (_ccifxOptPremium[i].Cci(CciPayment.CciEnum.receiver).IsMandatory))
                    {
                        if (StrFunc.IsEmpty(_fxOpt.FxOptionPremium[i].PayerPartyReference.HRef) &&
                             StrFunc.IsEmpty(_fxOpt.FxOptionPremium[i].PayerPartyReference.HRef))
                            _fxOpt.FxOptionPremium[i].PayerPartyReference.HRef = _fxOpt.BuyerPartyReference.HRef;
                    }
                }
                //
                if (TradeCustomCaptureInfos.PartyUnknown == id)
                    CciTrade.AddPartyUnknown();
            }

        }
        #endregion
        #endregion
        
        #region Membres de ITradeCci
        #region RetSidePayer
        public override string RetSidePayer { get { return SideTools.RetBuySide(); } }
        #endregion
        #region RetSideReceiver
        public override string RetSideReceiver { get { return SideTools.RetSellSide(); } }
        #endregion
        #region GetMainCurrency
        public override string GetMainCurrency
        {
            get
            {
                return _fxOpt.QuotedCurrencyPair.Currency1;
            }
        }
        #endregion GetMainCurrency
        #region CciClientIdMainCurrency
        public override string CciClientIdMainCurrency
        {
            get
            {
                return CciClientId(CciEnum.quotedCurrencyPair_currencyA);
            }
        }
        #endregion
        #endregion  Membres de ITrade
        
        #region Membres de IContainerCciPayerReceiver
        #region  CciClientIdPayer
        public override string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.buyer.ToString()); }
        }
        #endregion CciClientIdPayer
        #region CciClientIdReceiver
        public override string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.seller.ToString()); }
        }
        #endregion CciClientIdReceiver
        #region SynchronizePayerReceiver
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

        #region Membres de ITradeGetInfoButton
        // EG 20180514 [23812] Report
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {

            bool isOk = false;
            //
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                isOk = this.IsCci(CciEnum.fxTrigger, pCci);
                if (isOk)
                {
                    isOk = true;
                    if (_fxOpt.TypeTriggerAmericanSpecified)
                        pCo.Element = "typeTriggerAmerican";
                    else
                        pCo.Element = "typeTriggerEuropean";
                    pCo.Object = "product";
                    //
                    pIsSpecified = (_fxOpt.TypeTriggerEuropeanSpecified) || (_fxOpt.TypeTriggerAmericanSpecified);
                    pIsEnabled = true;
                }
                //
                if (!isOk)
                {
                    isOk = this.IsCci(CciEnum.triggerPayout_settlementInformation, pCci);
                    if (isOk)
                    {
                        isOk = true;
                        pCo.Element = "settlementInformation";
                        pCo.Object = "triggerPayout";
                        //
                        pIsSpecified = _fxOpt.TriggerPayout.SettlementInformationSpecified;
                        pIsEnabled = (pIsSpecified) && _fxOpt.TriggerPayout.SettlementInformation.InstructionSpecified;
                    }
                }
            }

            #region buttons settlementInfo FxOptionPremiumLength
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
                        //
                        pIsSpecified = _ccifxOptPremium[i].IsSettlementInfoSpecified;
                        pIsEnabled = _ccifxOptPremium[i].IsSettlementInstructionSpecified;
                        break;
                    }
                }
            }
            #endregion buttons settlementInfo
            //
            #region earlyTerminationProvision
            if (false == isOk)
                isOk = _cciEarlyTerminationProvision.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            #endregion earlyTerminationProvision

            return isOk;

        }
        #endregion Membres de ITradeGetInfoButton

        #region Membres de ITradeCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {

            bool isOk = false;
            //
            if (false == isOk)
                isOk = IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci);
            //
            if (false == isOk)
            {
                // Recherche ds les fixings de FxCashSettlement
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
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyA).NewValue;
            else
            {
                if (StrFunc.IsEmpty(ret))
                {
                    // Recherche ds les fixings de FxCashSettlement
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
            if (IsCci(CciEnum.quotedCurrencyPair_quoteBasis, pCci))
                ret = Cci(CciEnum.quotedCurrencyPair_currencyB).NewValue;
            else
            {
                if (StrFunc.IsEmpty(ret))
                {
                    // Recherche ds les fixings de FxCashSettlement
                    for (int i = 0; i < FxOptionPremiumLength; i++)
                    {
                        ret = _ccifxOptPremium[i].GetCurrency2(pCci);
                        if (StrFunc.IsFilled(ret))
                            break;
                    }
                }
            }
            //
            return ret;

        }
        #endregion
        #endregion Membres de ITradeGetInfoButton
        
        #region Methods
        #region public override SetProduct
        public override void SetProduct(IProduct pProduct)
        {
            _fxOpt = (IFxDigitalOption)pProduct;
            base.SetProduct(pProduct);
        }
        #endregion
        //
        #region private InitializeFxOptionPremium_FromCci
        private void InitializeFxOptionPremium_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool saveSpecified;
            string clientIdDefaultCurrency = CciClientId(CciEnum.triggerPayout_currency);
            //
            ArrayList lst = new ArrayList();
            //
            saveSpecified = _fxOpt.FxOptionPremiumSpecified;
            lst.Clear();

            while (isOk)
            {
                index += 1;

                CciPayment cciFxPremium = new CciPayment(CciTrade, index + 1, null, CciPayment.PaymentTypeEnum.FxOptionPremium, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxOptionPremium, string.Empty, string.Empty, string.Empty, clientIdDefaultCurrency, string.Empty);

                isOk = CcisBase.Contains(cciFxPremium.CciClientId(CciPayment.CciEnumFxOptionPremium.payer));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_fxOpt.FxOptionPremium) || (index == _fxOpt.FxOptionPremium.Length))
                        ReflectionTools.AddItemInArray(_fxOpt, "fxOptionPremium", index);
                    cciFxPremium.Payment = _fxOpt.FxOptionPremium[index];
                    lst.Add(cciFxPremium);
                }
            }

            _ccifxOptPremium = (CciPayment[])lst.ToArray(typeof(CciPayment));
            for (int i = 0; i < lst.Count; i++)
                _ccifxOptPremium[i].Initialize_FromCci();
            //			
            _fxOpt.FxOptionPremiumSpecified = saveSpecified;

        }
    
    #endregion
    #region private InitializeTriggerEuropean_FromCci
    private void InitializeTriggerEuropean_FromCci()
    {

        bool isOk = true;
        int index = -1;
        bool saveSpecified;
        //
        ArrayList lst = new ArrayList();
        //
        saveSpecified = _fxOpt.TypeTriggerEuropeanSpecified;
        lst.Clear();
        //
        while (isOk)
        {
            index += 1;
            //
            CciFxTrigger cciFxTrig = new CciFxTrigger(CciTrade, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxEuropeanTrigger);
            //
            isOk = CcisBase.Contains(cciFxTrig.CciClientId(CciFxTrigger.CciEnum.triggerCondition));
            if (isOk)
            {
                if (ArrFunc.IsEmpty(_fxOpt.TypeTriggerEuropean) || (index == _fxOpt.TypeTriggerEuropean.Length))
                    ReflectionTools.AddItemInArray(_fxOpt, "typeTriggerEuropean", index);
                cciFxTrig.FxTrigger = _fxOpt.TypeTriggerEuropean[index];
                //
                lst.Add(cciFxTrig);
            }
        }
        //
        _ccifxTrigger = (CciFxTrigger[])lst.ToArray(typeof(CciFxTrigger));
        for (int i = 0; i < lst.Count; i++)
        {
            _ccifxTrigger[i].Initialize_FromCci();
            _ccifxTrigger[i].SetDefaultClientIdCurrency(CciClientId(CciEnum.quotedCurrencyPair_currencyA), CciClientId(CciEnum.quotedCurrencyPair_currencyB));
        }
        //			
        _fxOpt.TypeTriggerEuropeanSpecified = saveSpecified;
        //

    }

        #endregion
        #region private InitializeTriggerAmerican_FromCci
        private void InitializeTriggerAmerican_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool saveSpecified;
            //
            ArrayList lst = new ArrayList();
            //
            saveSpecified = _fxOpt.TypeTriggerAmericanSpecified;
            lst.Clear();
            //
            while (isOk)
            {
                index += 1;
                //
                CciFxTrigger cciFxTrig = new CciFxTrigger(CciTrade, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxAmericanTrigger);
                //
                isOk = CcisBase.Contains(cciFxTrig.CciClientId(CciFxTrigger.CciEnum.touchCondition));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_fxOpt.TypeTriggerAmerican) || (index == _fxOpt.TypeTriggerAmerican.Length))
                        ReflectionTools.AddItemInArray(_fxOpt, "typeTriggerAmerican", index);
                    cciFxTrig.FxTrigger = _fxOpt.TypeTriggerAmerican[index];
                    //
                    lst.Add(cciFxTrig);
                }
            }
            //
            _ccifxTrigger = (CciFxTrigger[])lst.ToArray(typeof(CciFxTrigger));
            for (int i = 0; i < lst.Count; i++)
            {
                _ccifxTrigger[i].Initialize_FromCci();
                _ccifxTrigger[i].SetDefaultClientIdCurrency(CciClientId(CciEnum.quotedCurrencyPair_currencyA), CciClientId(CciEnum.quotedCurrencyPair_currencyB));
            }
            //			
            _fxOpt.TypeTriggerAmericanSpecified = saveSpecified;
            //

        }
        #endregion
        #region private InitializeBarrier_FromCci
        private void InitializeBarrier_FromCci()
        {

            bool isOk = true;
            int index = -1;
            bool saveSpecified;
            //
            System.Collections.ArrayList lst = new System.Collections.ArrayList();
            //
            saveSpecified = _fxOpt.FxBarrierSpecified;
            lst.Clear();

            while (isOk)
            {
                index += 1;
                //
                CciFxBarrier cciFxBarrier = new CciFxBarrier(CciTrade, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxBarrier);
                //
                isOk = CcisBase.Contains(cciFxBarrier.CciClientId(CciFxBarrier.CciEnum.fxBarrierType));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_fxOpt.FxBarrier) || (index == _fxOpt.FxBarrier.Length))
                        ReflectionTools.AddItemInArray(_fxOpt, "fxBarrier", index);
                    cciFxBarrier.FxBarrier = _fxOpt.FxBarrier[index];
                    //
                    lst.Add(cciFxBarrier);
                }
            }
            //
            _ccifxBarrier = (CciFxBarrier[])lst.ToArray(typeof(CciFxBarrier));
            for (int i = 0; i < lst.Count; i++)
            {
                _ccifxBarrier[i].Initialize_FromCci();
                _ccifxBarrier[i].SetDefaultClientIdCurrency(CciClientId(CciEnum.quotedCurrencyPair_currencyA.ToString()), CciClientId(CciEnum.quotedCurrencyPair_currencyB.ToString()));
            }
            //			
            _fxOpt.FxBarrierSpecified = saveSpecified;
            //

        }

        #endregion
        #region private DumpTriggerquotedCurrencyPair
        private void DumpTriggerquotedCurrencyPair()
        {

            if (_fxOpt.TypeTriggerAmericanSpecified)
            {
                if (ArrFunc.IsEmpty(_fxOpt.TypeTriggerAmerican))
                    ReflectionTools.AddItemInArray(_fxOpt, "typeTriggerAmerican", 0);
                for (int i = 0; i < _fxOpt.TypeTriggerAmerican.Length; i++)
                {
                    _fxOpt.TypeTriggerAmerican[i].QuotedCurrencyPair.QuoteBasis = _fxOpt.QuotedCurrencyPair.QuoteBasis;
                    _fxOpt.TypeTriggerAmerican[i].QuotedCurrencyPair.Currency1 = _fxOpt.QuotedCurrencyPair.Currency1;
                    _fxOpt.TypeTriggerAmerican[i].QuotedCurrencyPair.Currency2 = _fxOpt.QuotedCurrencyPair.Currency2;
                }
            }

            if (_fxOpt.TypeTriggerEuropeanSpecified)
            {
                if (ArrFunc.IsEmpty(_fxOpt.TypeTriggerEuropean))
                    ReflectionTools.AddItemInArray(_fxOpt, "typeTriggerEuropean", 0);
                for (int i = 0; i < _fxOpt.TypeTriggerEuropean.Length; i++)
                {
                    _fxOpt.TypeTriggerEuropean[i].QuotedCurrencyPair.QuoteBasis = _fxOpt.QuotedCurrencyPair.QuoteBasis;
                    _fxOpt.TypeTriggerEuropean[i].QuotedCurrencyPair.Currency1 = _fxOpt.QuotedCurrencyPair.Currency1;
                    _fxOpt.TypeTriggerEuropean[i].QuotedCurrencyPair.Currency2 = _fxOpt.QuotedCurrencyPair.Currency2;
                }
            }

        }
        #endregion
        #endregion
        #region Membre de ICciPresentation
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(_ccifxTrigger))
            {
                for (int i = 0; i < ArrFunc.Count(_ccifxTrigger); i++)
                    _ccifxTrigger[i].DumpSpecific_ToGUI(pPage);
            }
            
            if (ArrFunc.IsFilled(_ccifxOptPremium))
            {
                for (int i = 0; i < ArrFunc.Count(_ccifxOptPremium); i++)
                    _ccifxOptPremium[i].DumpSpecific_ToGUI(pPage);
            }
            if (ArrFunc.IsFilled(_ccifxBarrier))
            {
                for (int i = 0; i < ArrFunc.Count(_ccifxBarrier); i++)
                    _ccifxBarrier[i].DumpSpecific_ToGUI(pPage);
            }

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion        

    }

}
