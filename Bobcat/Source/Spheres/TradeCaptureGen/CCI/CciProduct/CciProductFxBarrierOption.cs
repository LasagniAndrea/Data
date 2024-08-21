#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{

    public class CciProductFxBarrierOption : CciProductFXOptionLeg
    {
        #region Members
        private readonly IFxBarrierOption _fxBarrierOption;
        private CciFxBarrier[] _cciFxBarrier;
        #endregion Members

        #region Enum
        public new enum CciEnum
        {
            #region buyer/seller
            [System.Xml.Serialization.XmlEnumAttribute("buyerPartyReference")]
            buyer,
            [System.Xml.Serialization.XmlEnumAttribute("sellerPartyReference")]
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
            // --------------------------
            // 20070725 PL Ticket:n°15600
            #region cashSettlementTerms
            [System.Xml.Serialization.XmlEnumAttribute("cashSettlementTerms.settlementCurrency")]
            cashSettlementTerms_settlementCurrency,
            #endregion cashSettlementTerms
            // --------------------------
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
            #region fxRebateBarrierSpecified
            [System.Xml.Serialization.XmlEnumAttribute("fxRebateBarrier")]
            fxRebateBarrier,
            fxRebateBarrierSpecified,
            #endregion fxRebateBarrierSpecified
            unknown,
        }
        #endregion Enum

        #region properties
        #region fxBarrierLength
        public int FxBarrierLength
        {
            get { return ArrFunc.IsFilled(_cciFxBarrier) ? _cciFxBarrier.Length : 0; }
        }
        #endregion
        #region public isCciTriggerPayoutSpecified
        /// <summary>
        /// Obteint true si le cciContainer associé aux trigger Payout est renseigné
        /// </summary>
        public bool IsCciTriggerPayoutSpecified
        {
            get { return CcisBase[CciClientId(CciEnum.triggerPayout_payoutStyle.ToString())].IsFilledValue || CcisBase[CciClientId(CciEnum.triggerPayout_amount.ToString())].IsFilledValue; }
        }
        #endregion
        #region public isRebateSettlementInformationSpecified
        /// <summary>
        /// Retourne true si _fxBarrierOption.triggerPayout.settlementInformation.instructionSpecified
        /// </summary>
        public bool IsRebateSettlementInformationSpecified
        {
            get
            {
                ISettlementInformation stl = _fxBarrierOption.TriggerPayout.SettlementInformation;
                return (null != stl) && stl.InstructionSpecified;
            }
        }
        #endregion
        #endregion Public property

        #region constructor
        public CciProductFxBarrierOption(CciTrade pCciTrade, IFxBarrierOption pFxBarrierOption, string pPrefix)
            : this(pCciTrade, pFxBarrierOption, pPrefix, -1)
        { }
        public CciProductFxBarrierOption(CciTrade pCciTrade, IFxBarrierOption pFxBarrierOption, string pPrefix, int pNumber)
            : base(pCciTrade, pFxBarrierOption, CciProductFXOptionLeg.FxProduct.FxBarrierOption, pPrefix, pNumber)
        {
            _fxBarrierOption = (IFxBarrierOption)pFxBarrierOption;
        }
        #endregion constructor

        

        #region Membres de IContainerCciFactory
        #region public override Initialize_FromCci
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _fxBarrierOption);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + CciClientId(CciEnum.fxRebateBarrier.ToString()), false, TypeData.TypeDataEnum.@string);
            InitializeFxBarrier_FromCci();

            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region public override AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
            for (int i = 0; i < this.FxBarrierLength; i++)
                _cciFxBarrier[i].AddCciSystem();
        }
        #endregion AddCciSystem
        #region public override Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
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
                        #region spotRate
                        case CciEnum.spotRate:
                            if (_fxBarrierOption.SpotRateSpecified)
                                data = _fxBarrierOption.SpotRate.Value;
                            break;
                        #endregion spotRate
                        #region triggerPayout
                        case CciEnum.triggerPayout_amount:
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                                data = _fxBarrierOption.TriggerPayout.Amount.Value;
                            break;
                        case CciEnum.triggerPayout_currency:
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                                data = _fxBarrierOption.TriggerPayout.Currency;
                            break;
                        case CciEnum.triggerPayout_payoutStyle:
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                                data = _fxBarrierOption.TriggerPayout.PayoutStyle.ToString();
                            break;
                        case CciEnum.triggerPayout_settlementInformation:
                            // 20090403 RD 16536
                            if (null == _fxBarrierOption.TriggerPayout.SettlementInformation || (false == _fxBarrierOption.TriggerPayout.SettlementInformationSpecified))
                                data = Cst.SettlementTypeEnum.None.ToString();
                            else if (_fxBarrierOption.TriggerPayoutSpecified && _fxBarrierOption.TriggerPayout.SettlementInformation.StandardSpecified)
                                data = _fxBarrierOption.TriggerPayout.SettlementInformation.Standard.ToString();
                            else if (_fxBarrierOption.TriggerPayoutSpecified && _fxBarrierOption.TriggerPayout.SettlementInformation.InstructionSpecified)
                                data = Cst.SettlementInformationType.Instruction.ToString();
                            else
                                data = Cst.SettlementTypeEnum.None.ToString();
                            break;
                        #endregion triggerPayout
                        #region fxRebateBarrierSpecified
                        case CciEnum.fxRebateBarrierSpecified:
                            data = _fxBarrierOption.FxRebateBarrierSpecified.ToString().ToLower();
                            break;
                        #endregion fxRebateBarrierSpecified
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
            for (int i = 0; i < this.FxBarrierLength; i++)
                _cciFxBarrier[i].Initialize_FromDocument();
            //
            base.Initialize_FromDocument();

        }
        #endregion Initialize_FromDocument
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
                    
                    #region init varaible
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    #endregion
                    //
                    switch (cciEnum)
                    {
                        #region spotRate
                        case CciEnum.spotRate:
                            _fxBarrierOption.SpotRateSpecified = cci.IsFilledValue;
                            _fxBarrierOption.SpotRate.Value = data;
                            break;
                        #endregion spotRate
                        #region callCurrencyAmount_currency
                        case CciEnum.callCurrencyAmount_currency:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion callCurrencyAmount_currency
                        #region putCurrencyAmount_currency
                        case CciEnum.putCurrencyAmount_currency:
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion putCurrencyAmount_currency
                        #region triggerPayout
                        case CciEnum.triggerPayout_payoutStyle:
                            _fxBarrierOption.TriggerPayoutSpecified = this.IsCciTriggerPayoutSpecified;
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                            {
                                if (System.Enum.IsDefined(typeof(PayoutEnum), data))
                                {
                                    PayoutEnum payoutEnum = (PayoutEnum)System.Enum.Parse(typeof(PayoutEnum), data, true);
                                    _fxBarrierOption.TriggerPayout.PayoutStyle = payoutEnum;
                                }
                            }
                            break;
                        case CciEnum.triggerPayout_amount:
                            _fxBarrierOption.TriggerPayoutSpecified = this.IsCciTriggerPayoutSpecified;
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                            {
                                _fxBarrierOption.TriggerPayout.Amount.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            }
                            break;
                        case CciEnum.triggerPayout_currency:
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                            {
                                _fxBarrierOption.TriggerPayout.Currency = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            }
                            break;
                        case CciEnum.triggerPayout_settlementInformation:
                            if (_fxBarrierOption.TriggerPayoutSpecified)
                            {
                                ISettlementInformation stl = _fxBarrierOption.TriggerPayout.SettlementInformation;
                                bool isSettlementInformationSpecified = (Cst.SettlementTypeEnum.None.ToString() != data) && StrFunc.IsFilled(data);
                                //
                                stl.StandardSpecified = false;
                                stl.InstructionSpecified = false;
                                _fxBarrierOption.TriggerPayout.SettlementInformationSpecified = isSettlementInformationSpecified;
                                //	
                                if (isSettlementInformationSpecified)
                                {
                                    stl.StandardSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() != data);
                                    stl.InstructionSpecified = (Cst.SettlementTypeEnum.Instruction.ToString() == data);
                                    //
                                    if (stl.StandardSpecified)
                                        stl.Standard = (StandardSettlementStyleEnum)System.Enum.Parse(typeof(StandardSettlementStyleEnum), data);
                                }
                            }
                            break;
                        #endregion triggerPayout
                        #region fxRebateBarrierSpecified
                        case CciEnum.fxRebateBarrierSpecified:
                            _fxBarrierOption.FxRebateBarrierSpecified = cci.IsFilledValue;
                            break;
                        #endregion fxRebateBarrierSpecified
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
            //
            for (int i = 0; i < this.FxBarrierLength; i++)
                _cciFxBarrier[i].Dump_ToDocument();
            //
            base.Dump_ToDocument();

        }
        #endregion Dump_ToDocument
        #region public override ProcessInitialize
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
                    #region triggerPayout
                    case CciEnum.triggerPayout_amount:
                    case CciEnum.triggerPayout_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.triggerPayout_amount.ToString()),
                            _fxBarrierOption.TriggerPayout.Amount, _fxBarrierOption.TriggerPayout.Currency, (CciEnum.triggerPayout_amount == key));
                        break;
                    #endregion
                    #region callCurrencyAmount_currency
                    case CciEnum.callCurrencyAmount_currency:
                        for (int i = 0; i < FxBarrierLength; i++)
                        {
                            if (!_cciFxBarrier[i].ExistCciAssetFxRate)
                                CcisBase.SetNewValue(_cciFxBarrier[i].CciClientId(CciFxBarrier.CciEnum.quotedCurrencyPair_currencyA), _fxBarrierOption.CallCurrencyAmount.Currency);
                        }
                        break;
                    #endregion callCurrencyAmount_currency
                    #region putCurrencyAmount_currency
                    case CciEnum.putCurrencyAmount_currency:
                        for (int i = 0; i < FxBarrierLength; i++)
                        {
                            if (!_cciFxBarrier[i].ExistCciAssetFxRate)
                                CcisBase.SetNewValue(_cciFxBarrier[i].CciClientId(CciFxBarrier.CciEnum.quotedCurrencyPair_currencyB), _fxBarrierOption.PutCurrencyAmount.Currency);
                        }
                        break;
                    #endregion putCurrencyAmount_currency
                    #region Default
                    default:
                        //System.Diagnostics.Debug.WriteLine("PROCESSS NON GERE: " + pCci.ClientId_WithoutPrefix);
                        break;
                        #endregion Default
                }
            }
            //
            for (int i = 0; i < FxBarrierLength; i++)
                _cciFxBarrier[i].ProcessInitialize(pCci);
            //
            base.ProcessInitialize(pCci);
            //

        }
        #endregion ProcessInitialize
        #region public override CleanUp
        public override void CleanUp()
        {


            for (int i = 0; i < FxBarrierLength; i++)
                _cciFxBarrier[i].CleanUp();
            //
            if (ArrFunc.IsFilled(_fxBarrierOption.FxBarrier))
            {
                for (int i = _fxBarrierOption.FxBarrier.Length - 1; -1 < i; i--)
                {
                    if (false == CaptureTools.IsDocumentElementValid(_fxBarrierOption.FxBarrier[i].TriggerRate))
                        ReflectionTools.RemoveItemInArray(_fxBarrierOption, "fxBarrier", i);
                }
            }
            //
            base.CleanUp();

        }
        #endregion
        #region public override SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            for (int i = 0; i < FxBarrierLength; i++)
                _cciFxBarrier[i].SetDisplay(pCci);
            base.SetDisplay(pCci);
        }
        #endregion
        #endregion Membres de IContainerCciFactory

        #region Membres de IContainerCciQuoteBasis
        #region public override IsClientId_QuoteBasis
        public override bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            for (int i = 0; i < this.FxBarrierLength; i++)
            {
                isOk = _cciFxBarrier[i].IsClientId_QuoteBasis(pCci);
                if (isOk)
                    break;
            }
            if (!isOk)
                isOk = base.IsClientId_QuoteBasis(pCci);
            return isOk;
        }
        #endregion
        #region public override GetCurrency1
        public override string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            for (int i = 0; i < this.FxBarrierLength; i++)
            {
                ret = _cciFxBarrier[i].GetCurrency1(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }

            if (StrFunc.IsEmpty(ret))
                ret = base.GetCurrency1(pCci);
            return ret;
        }
        #endregion
        #region public override GetCurrency2
        public override string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            for (int i = 0; i < this.FxBarrierLength; i++)
            {
                ret = _cciFxBarrier[i].GetCurrency2(pCci);
                if (StrFunc.IsFilled(ret))
                    break;
            }
            if (StrFunc.IsEmpty(ret))
                ret = base.GetCurrency2(pCci);
            return ret;
        }
        #endregion
        #endregion Membres de IContainerCciQuoteBasis

        #region Membres de ITradeGetInfoButton
        #region public override bool SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            bool isOk = false;
            #region buttons settlementInformation triggerPayout
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.triggerPayout_settlementInformation, pCci);
                if (isOk)
                {
                    pCo.Element = "settlementInformation";
                    pCo.Object = "triggerPayout";
                    pIsSpecified = _fxBarrierOption.TriggerPayoutSpecified;
                    pIsEnabled = IsRebateSettlementInformationSpecified;
                }
            }
            #endregion  buttons settlementInformation triggerPayout

            #region  buttons Rebate Barrier
            if (!isOk)
            {
                isOk = this.IsCci(CciEnum.fxRebateBarrier, pCci);
                if (isOk)
                {
                    pCo.Object = "product";
                    pCo.Element = "fxRebateBarrier";
                    pIsSpecified = _fxBarrierOption.FxRebateBarrierSpecified;
                    pIsEnabled = _fxBarrierOption.FxRebateBarrierSpecified;
                }
            }
            #endregion  buttons Rebate Barrier
            // Autres		
            if (!isOk)
                isOk = base.SetButtonZoom(pCci, pCo, ref pIsSpecified, ref pIsEnabled);
            return isOk;
        }
        #endregion
        #endregion Membres de ITradeGetInfoButton

        #region Membres de ICciPresentation
        public override void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (ArrFunc.IsFilled(_cciFxBarrier))
            {
                for (int i = 0; i < ArrFunc.Count(_cciFxBarrier); i++)
                    _cciFxBarrier[i].DumpSpecific_ToGUI(pPage);
            }

            base.DumpSpecific_ToGUI(pPage);
        }
        #endregion

        #region Private Methods
        #region InitializeFxBarrier_FromCci
        private void InitializeFxBarrier_FromCci()
        {

            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            //
            lst.Clear();
            while (isOk)
            {
                index += 1;
                //
                CciFxBarrier cciFxBar = new CciFxBarrier(CciTrade, index + 1, null, Prefix + TradeCustomCaptureInfos.CCst.Prefix_fxBarrier);

                isOk = CcisBase.Contains(cciFxBar.CciClientId(CciFxBarrier.CciEnum.triggerRate));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(_fxBarrierOption.FxBarrier) || (index == _fxBarrierOption.FxBarrier.Length))
                        ReflectionTools.AddItemInArray(_fxBarrierOption, "fxBarrier", index);
                    cciFxBar.FxBarrier = _fxBarrierOption.FxBarrier[index];
                    //
                    lst.Add(cciFxBar);
                }
            }
            //
            _cciFxBarrier = null;
            _cciFxBarrier = (CciFxBarrier[])lst.ToArray(typeof(CciFxBarrier));
            for (int i = 0; i < FxBarrierLength; i++)
            {
                _cciFxBarrier[i].Initialize_FromCci();
                _cciFxBarrier[i].SetDefaultClientIdCurrency(CciClientId(CciEnum.callCurrencyAmount_currency.ToString()), CciClientId(CciEnum.putCurrencyAmount_currency.ToString()));
            }

        }
        #endregion InitializeFxBarrier_FromCci
        #endregion
    }
 
}
