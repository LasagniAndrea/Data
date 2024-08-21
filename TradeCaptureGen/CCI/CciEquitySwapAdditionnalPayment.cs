#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum.Tools;
using FpML.Enum;
using FpML.Interface;
using System;
using System.Collections;
using System.Globalization;
using Tz = EFS.TimeZone;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciPayment. 
    /// </summary>
    public class CciReturnSwapAdditionalPayment : IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver, IContainerCciSpecified
    {
        #region Members
        private readonly CciTrade _cciTrade;
        private readonly CciProductReturnSwap _cciReturnSwap;
        private IReturnSwapAdditionalPayment _payment;

        private readonly int _number;
        private readonly string _prefix;
        private readonly string _defaultPaymentType;
        private string _clientIdDefaultReceiver;
        private readonly string _clientIdDefaultBDC;
        private readonly string _clientIdDefaultCurrency;
        #endregion Members

        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentAmount.paymentAmount.amount")]
            additionalPaymentAmount_paymentAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentAmount.paymentAmount.currency")]
            additionalPaymentAmount_paymentAmount_currency,
            //Adjustable 
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateAdjustableDate.unadjustedDate")]
            additionalPaymentDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateAdjustableDate.dateAdjustments.businessDayConvention")]
            additionalPaymentDate_adjustableDate_dateAdjustments_bDC,
            //RelativeDate
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.periodMultiplier")]
            additionalPaymentDate_relativeDate_periodMultiplier,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.period")]
            additionalPaymentDate_relativeDate_period,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.dateRelativeTo")]
            additionalPaymentDate_relativeDate_dateRelativeTo,
            [System.Xml.Serialization.XmlEnumAttribute("additionalPaymentDate.adjustableOrRelativeDateRelativeDate.businessDayConvention")]
            additionalPaymentDate_relativeDate_bDC,
            // Type			
            [System.Xml.Serialization.XmlEnumAttribute("paymentType")]
            paymentType,
            unknown
        }
        #endregion Enum

        #region Constructor
        public CciReturnSwapAdditionalPayment(CciTrade pCciTrade, CciProductReturnSwap pCciReturnSwap, int pPaymentNumber, IReturnSwapAdditionalPayment _, string pPrefixPayment,
                            string pDefaultPaymentType, string pClientIdDefaultReceiver, string pClientIdDefaultBDC, string pClientIdDefaultCurrency)
        {
            _cciReturnSwap = pCciReturnSwap; 
            _cciTrade = pCciTrade;
            _number = pPaymentNumber;  
            _prefix = pPrefixPayment + Number + CustomObject.KEY_SEPARATOR;
            _defaultPaymentType = pDefaultPaymentType;
            _clientIdDefaultReceiver = pClientIdDefaultReceiver;
            _clientIdDefaultBDC = pClientIdDefaultBDC;
            _clientIdDefaultCurrency = pClientIdDefaultCurrency;
        }
        #endregion Constructor

        #region Property
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        public string Number
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = _number.ToString();
                return ret;
            }
        }
        public bool ExistNumber { get { return (0 < _number); } }
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        public bool IsModeAjustablePaymentDate { get { return Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate)); } }
        public string ClientIdDefaultReceiver
        {
            set { _clientIdDefaultReceiver = value; }
        }
        public IReturnSwapAdditionalPayment Payment
        {
            set { _payment = value; }
            get { return _payment; }
        }
        #endregion 
        

        #region Membres de IContainerCciFactory
        #region public Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _payment);
            //
            if (this.IsModeAjustablePaymentDate && (null == _payment.AdditionalPaymentDate.AdjustableDate))
                _payment.AdditionalPaymentDate.AdjustableDate = _payment.AdditionalPaymentDate.CreateAdjustableDate;
            else if (null == _payment.AdditionalPaymentDate.RelativeDate)
                _payment.AdditionalPaymentDate.RelativeDate = _payment.AdditionalPaymentDate.CreateRelativeDate;
            if (Ccis.Contains(CciClientId(CciEnum.paymentType)) && (null == _payment.PaymentType))
                _payment.PaymentType = _payment.CreatePaymentType;
        }
        #endregion Initialize_FromCci
        #region public AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            ArrayList PayersReceivers = new ArrayList
            {
                CciClientIdPayer,
                CciClientIdReceiver
            };

            IEnumerator ListEnum = PayersReceivers.GetEnumerator();
            while (ListEnum.MoveNext())
            {
                string clientId_WithoutPrefix = ListEnum.Current.ToString();
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, true, TypeData.TypeDataEnum.@string);
            }
            Ccis[CciClientIdReceiver].IsMandatory = Ccis[CciClientIdPayer].IsMandatory;
            // Capture With AdjustableDate
            if (IsModeAjustablePaymentDate)
            {
                //Nécessaire pour le recalcul des BCs 
                string clientId_WithoutPrefix = CciClientId(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }
            else
            {
                //Nécessaire pour le recalcul des BCs 
                string clientId_WithoutPrefix = CciClientId(CciEnum.additionalPaymentDate_relativeDate_bDC);
                CciTools.AddCciSystem(Ccis, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            }
        }

        #endregion AddCciSystem
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
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
                        #region Payer
                        case CciEnum.payer:
                            data = _payment.PayerPartyReference.HRef;
                            break;
                        #endregion Payer
                        #region Receiver
                        case CciEnum.receiver:
                            data = _payment.ReceiverPartyReference.HRef;
                            break;
                        #endregion Receiver
                        #region PaymentDate
                        #region AdjustableDate
                        case CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate:
                            // On pourrait déterminer la date en fonction du relative s'il est paramétré
                            if (_payment.AdditionalPaymentDate.AdjustableDateSpecified)
                                data = _payment.AdditionalPaymentDate.AdjustableDate.UnadjustedDate.Value;
                            break;
                        case CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC:
                            if (_payment.AdditionalPaymentDate.AdjustableDateSpecified)
                                data = _payment.AdditionalPaymentDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                            break;
                        #endregion AdjustableDate
                        #region RelativeDate
                        case CciEnum.additionalPaymentDate_relativeDate_dateRelativeTo:
                            if (_payment.AdditionalPaymentDate.RelativeDateSpecified)
                                data = _payment.AdditionalPaymentDate.RelativeDate.DateRelativeToValue;
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_period:
                            if (_payment.AdditionalPaymentDate.RelativeDateSpecified)
                                data = _payment.AdditionalPaymentDate.RelativeDate.Period.ToString();
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_periodMultiplier:
                            if (_payment.AdditionalPaymentDate.RelativeDateSpecified)
                                data = _payment.AdditionalPaymentDate.RelativeDate.PeriodMultiplier.Value;
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_bDC:
                            if (_payment.AdditionalPaymentDate.RelativeDateSpecified)
                                data = _payment.AdditionalPaymentDate.RelativeDate.BusinessDayConvention.ToString();
                            break;

                        #endregion RelativeDate
                        #endregion PaymentDate
                        #region paymentAmount_amount
                        case CciEnum.additionalPaymentAmount_paymentAmount_amount:
                            if (_payment.AdditionalPaymentAmount.PaymentAmountSpecified)
                                data = _payment.AdditionalPaymentAmount.PaymentAmount.Amount.Value;
                            break;
                        #endregion paymentAmount
                        #region paymentAmount_currency
                        case CciEnum.additionalPaymentAmount_paymentAmount_currency:
                            if (_payment.AdditionalPaymentAmount.PaymentAmountSpecified)
                                data = _payment.AdditionalPaymentAmount.PaymentAmount.Currency;
                            break;
                        #endregion paymentAmount_currency
                        #region PaymentType
                        case CciEnum.paymentType:
                            if (_payment.PaymentTypeSpecified)
                                data = _payment.PaymentType.Value;
                            break;
                        #endregion PaymentType
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            string clientId;
            string clientId_Element;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            foreach (CustomCaptureInfo cci in Ccis)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if (cci.HasChanged && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Element = CciContainerKey(cci.ClientId_WithoutPrefix);
                    //
                    CciEnum elt = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    switch (elt)
                    {
                        #region PaymentPayer/Receiver
                        case CciEnum.payer:
                        case CciEnum.receiver:
                            if (CciEnum.payer == elt)
                                _payment.PayerPartyReference.HRef = data;
                            else
                                _payment.ReceiverPartyReference.HRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin de préproposer les BCs

                            break;
                        #endregion PaymentPayer/Receiver
                        #region PaymentAmount
                        case CciEnum.additionalPaymentAmount_paymentAmount_amount:
                            _payment.AdditionalPaymentAmount.PaymentAmountSpecified = cci.IsFilledValue;
                            _payment.AdditionalPaymentAmount.PaymentAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;

                        case CciEnum.additionalPaymentAmount_paymentAmount_currency:
                            _payment.AdditionalPaymentAmount.PaymentAmountSpecified = cci.IsFilledValue;
                            _payment.AdditionalPaymentAmount.PaymentAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        #endregion PaymentAmount
                        #region PaymentDate
                        #region PaymentDateadjustedDate
                        case CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate:
                            _payment.AdditionalPaymentDate.AdjustableDateSpecified = cci.IsFilledValue;
                            _payment.AdditionalPaymentDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        case CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC:
                            DumpPaymentBDA(true);
                            break;
                        #endregion PaymentDateadjustedDate
                        #region PaymentDateRelativeDate
                        case CciEnum.additionalPaymentDate_relativeDate_dateRelativeTo:
                            _payment.AdditionalPaymentDate.RelativeDateSpecified = cci.IsFilledValue;
                            _payment.AdditionalPaymentDate.RelativeDate.DateRelativeToValue = data;
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_period:
                            PeriodEnum periodEnum = StringToEnum.Period(data);
                            _payment.AdditionalPaymentDate.RelativeDate.Period = periodEnum;
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_periodMultiplier:
                            _payment.AdditionalPaymentDate.RelativeDate.PeriodMultiplier.Value = data;
                            break;
                        case CciEnum.additionalPaymentDate_relativeDate_bDC:
                            DumpPaymentBDA(false);
                            break;
                        #endregion  PaymentDateRelativeDate
                        #endregion PaymentDate
                        #region PaymentType
                        case CciEnum.paymentType:
                            _payment.PaymentTypeSpecified = cci.IsFilledValue;
                            _payment.PaymentType.Value = data;
                            break;
                        #endregion PaymentType
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocumentPayment
        #region ProcessInitialize
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (key)
                {
                    #region Payer/Receiver: Calcul des BCs
                    case CciEnum.payer:
                        Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        bool isClear = StrFunc.IsEmpty(pCci.NewValue);
                        if (isClear)
                            ClearPaymentData();
                        else
                            PaymentInitialize();
                        //						DumpPaymentBDA();
                        break;
                    case CciEnum.receiver:
                        Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        //						DumpPaymentBDA();
                        break;
                    #endregion
                    #region Currency: Arrondi du notional et Calcul des BCs
                    case CciEnum.additionalPaymentAmount_paymentAmount_amount:
                    case CciEnum.additionalPaymentAmount_paymentAmount_currency:
                        Ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.additionalPaymentAmount_paymentAmount_amount), _payment.AdditionalPaymentAmount.PaymentAmount, (CciEnum.additionalPaymentAmount_paymentAmount_amount == key));
                        break;
                    #endregion
                    default:

                        break;
                }
            }
        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091207 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;
        }
        #endregion
        #region IsClientId_Bdc
        //		public bool IsClientId_Bdc(CustomCaptureInfo pCci)
        //		{
        //			bool isOk = false;
        //			isOk = (CciClientId(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC  ) == pCci.ClientId_WithoutPrefix);
        //			isOk = isOk ||(CciClientId(CciEnum.additionalPaymentDate_relativeDate_bDC ) == pCci.ClientId_WithoutPrefix);
        //			return isOk;
        //		}
        #endregion
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC, pCci))
            {
                if (_payment.AdditionalPaymentDate.AdjustableDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _payment.AdditionalPaymentDate.AdjustableDate.DateAdjustments);
            }
            if (IsCci(CciEnum.additionalPaymentDate_relativeDate_bDC, pCci))
            {
                if (_payment.AdditionalPaymentDate.RelativeDateSpecified)
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, _payment.AdditionalPaymentDate.RelativeDate.GetAdjustments);
            }
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #endregion

        #region Membres de IContainerCciPayerReceiver
        #region  CciClientIdPayer/receiver
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }
        #endregion
        #region SynchronizePayerReceiver
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue);
        }
        #endregion
        #endregion

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(_prefix);
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci

        #region private sub
        /// EG 20140426 TypeReferentialInfo.Asset remplace TypeReferentialInfo.Equity
        private void DumpPaymentBDA(bool pbModeBusinessDayAdjustment)
        {
            string clientIdEquity = _cciReturnSwap.CciReturnSwapReturnLeg[0].CciClientId(CciReturnLeg.CciEnumUnderlyer.underlyer_underlyingAsset);    // A reprendre si basket (non gere pur l'instant)
            CciBC cciBC = new CciBC(_cciTrade)
            {
                { clientIdEquity, CciBC.TypeReferentialInfo.Asset }
            };
            IBusinessDayAdjustments bda;

            string clientIdBdc;
            if (pbModeBusinessDayAdjustment)
            {
                clientIdBdc = CciClientId(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC);
                bda = _payment.AdditionalPaymentDate.AdjustableDate.DateAdjustments;
            }
            else
            {
                clientIdBdc = CciClientId(CciEnum.additionalPaymentDate_relativeDate_bDC);
                bda = _payment.AdditionalPaymentDate.RelativeDate.GetAdjustments;
            }
            //
            Ccis.DumpBDC_ToDocument(bda, clientIdBdc, CciClientId(TradeCustomCaptureInfos.CCst.PAYMENT_BUSINESS_CENTERS_REFERENCE), cciBC);
        }
        public void PaymentInitialize()
        {
            bool IsExistCciDefaultReceiver = Ccis.Contains(_clientIdDefaultReceiver);
            bool IsExistCciDefaultCurrency = Ccis.Contains(_clientIdDefaultCurrency);
            bool IsExistCciDefaultBDC = Ccis.Contains(_clientIdDefaultBDC);

            bool IsOk;
            //
            string defaultReceiver;
            string defaultBDC;
            string defaultCurrency;
            //
            if (!IsExistCciDefaultReceiver)
                _clientIdDefaultReceiver = CciClientIdReceiver;

            IsOk = (Ccis.Contains(_clientIdDefaultReceiver));
            if (IsOk)
            {
                defaultReceiver = Ccis[_clientIdDefaultReceiver].NewValue;
                //
                if (IsExistCciDefaultBDC)
                    defaultBDC = Ccis[_clientIdDefaultBDC].NewValue;
                else
                    defaultBDC = BusinessDayConventionEnum.NONE.ToString();
                //
                if (IsExistCciDefaultCurrency)
                    defaultCurrency = Ccis[_clientIdDefaultCurrency].NewValue;
                else
                    defaultCurrency = string.Empty;
                //
                SetPaymentData(defaultReceiver, defaultBDC, defaultCurrency, _defaultPaymentType);
            }
        }

        private void ClearPaymentData()
        {
            CustomCaptureInfo cci;
            //Receiver			
            if (Ccis.Contains(CciClientId(CciEnum.receiver)))
            {
                cci = this.Cci(CciEnum.receiver);
                cci.NewValue = string.Empty;
            }
            //Date			
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate);
                cci.NewValue = string.Empty;
                cci.IsMandatory = true;
            }
            //BDC
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC);
                cci.NewValue = string.Empty;
            }

            //RelativeDate	
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_relativeDate_dateRelativeTo)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_relativeDate_dateRelativeTo);
                cci.NewValue = string.Empty;
            }

            //RelativeDate	
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_relativeDate_period)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_relativeDate_period);
                cci.NewValue = string.Empty;
            }

            //RelativeDate	
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_relativeDate_periodMultiplier)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_relativeDate_periodMultiplier);
                cci.NewValue = string.Empty;
            }

            //RelativeDate	
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_relativeDate_bDC)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_relativeDate_bDC);
                cci.NewValue = string.Empty;
            }

            //PaymentType
            if (Ccis.Contains(CciClientId(CciEnum.paymentType)))
            {
                cci = this.Cci(CciEnum.paymentType);
                cci.NewValue = string.Empty;
            }
            //PaymentAmount
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentAmount_paymentAmount_amount)))
            {
                cci = this.Cci(CciEnum.additionalPaymentAmount_paymentAmount_amount);
                cci.NewValue = string.Empty;
            }
            //PaymentCurrency
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentAmount_paymentAmount_currency)))
            {
                cci = this.Cci(CciEnum.additionalPaymentAmount_paymentAmount_currency);
                cci.NewValue = string.Empty;
            }

        }
        /// EG 20171004 [23452] TradeDateTime
        private void SetPaymentData(string pReceiver, string pBusinessDayConvention, string pCurrency, string pPaymentType)
        {
            CustomCaptureInfo cci;

            //Receiver			
            if (Ccis.Contains(CciClientId(CciEnum.receiver)))
            {
                cci = this.Cci(CciEnum.receiver);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pReceiver;
            }
            //Date			
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_adjustableDate_unadjustedDate);
                if (StrFunc.IsEmpty(cci.LastValue))
                {
                    //    cci.NewValue = _cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDate).NewValue;
                    //cci.NewValue = Tz.Tools.DateToStringISO(_cciTrade.cciTradeHeader.Cci(CciTradeHeader.CciEnum.tradeDateTime).NewValue);
                    cci.NewValue = Tz.Tools.DateToStringISO(_cciTrade.cciMarket[0].Cci(CciMarketParty.CciEnum.executionDateTime).NewValue);
                }
            }
            //BDC
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC)))
            {
                cci = this.Cci(CciEnum.additionalPaymentDate_adjustableDate_dateAdjustments_bDC);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pBusinessDayConvention;
            }
            //PaymentType
            if (Ccis.Contains(CciClientId(CciEnum.paymentType)))
            {
                cci = this.Cci(CciEnum.paymentType);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pPaymentType;
            }
            //PaymentAmount
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentAmount_paymentAmount_amount)))
            {
                cci = this.Cci(CciEnum.additionalPaymentAmount_paymentAmount_amount);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = 0.ToString("n", CultureInfo.InvariantCulture);
            }
            //PaymentCurrency
            if (Ccis.Contains(CciClientId(CciEnum.additionalPaymentAmount_paymentAmount_currency)))
            {
                cci = this.Cci(CciEnum.additionalPaymentAmount_paymentAmount_currency);
                if (StrFunc.IsEmpty(cci.LastValue))
                    cci.NewValue = pCurrency;
            }

        }

        #endregion Private sub

    }
}