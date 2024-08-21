#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciPayment. 
    /// </summary>
    public class CciSimplePayment : IContainerCciFactory, IContainerCci, IContainerCciPayerReceiver, IContainerCciSpecified, IContainerCciQuoteBasis, ICciPresentation
    {
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payerPartyReference")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiverPartyReference")]
            receiver,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.amount")]
            paymentAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("paymentAmount.currency")]
            paymentAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableDate.unadjustedDate")]
            paymentDate_adjustableDate_unadjustedDate,
            [System.Xml.Serialization.XmlEnumAttribute("paymentDate.adjustableDate.dateAdjustments.businessDayConvention")]
            paymentDate_adjustableDate_dateAdjustments_bDC,

            paymentAmountOrigin_amount,
            paymentAmountOrigin_currency,

            unknown,
        }

        #region Members
        private IMoney _simplePaymentOrigine; 
        private ISimplePayment _simplePayment;
        private readonly int _number;
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly string _prefix;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }

        public bool IsPaymentEqualOrigin
        {
            get
            {
                bool ret = true;
                if (null != _simplePaymentOrigine)
                {
                    ret = (_simplePaymentOrigine.Currency == _simplePayment.PaymentAmount.Currency);
                    ret &= (_simplePaymentOrigine.Amount.DecValue == _simplePayment.PaymentAmount.Amount.DecValue);
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ISimplePayment SimplePayment
        {
            set
            {
                _simplePayment = (ISimplePayment)value;
            }
            get
            {
                return _simplePayment;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IMoney SimplePaymentOrigine
        {
            set
            {
                _simplePaymentOrigine  = (IMoney)value;
            }
            get
            {
                return _simplePaymentOrigine;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private bool ExistNumber { get { return (0 < _number); } }

        /// <summary>
        /// 
        /// </summary>
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = _number.ToString();
                return ret;
            }
        }
        #endregion Accessors

        #region Constructors
        public CciSimplePayment(CciTradeBase pCciTrade, int pPaymentNumber, ISimplePayment pPayment, IMoney pPaymentOrigine, string pPrefixPayment)
        {
            _ccis = pCciTrade.Ccis;
            _number = pPaymentNumber;  // Use property Number
            _prefix = pPrefixPayment + NumberPrefix + CustomObject.KEY_SEPARATOR;
            _simplePayment = pPayment;
            _simplePaymentOrigine = pPaymentOrigine;
        }
        #endregion Constructors

        #region Interfaces
        #region IContainerCciFactory Members
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
            //
            //Don't erase
            CreateInstance();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
            
        }

        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {

            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if ((cci != null) && (cci.HasChanged))
                {
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    string data = cci.NewValue;
                    bool isSetting = true;
                    //
                    switch (cciEnum)
                    {
                        case CciEnum.payer:
                        case CciEnum.receiver:
                            if (CciEnum.payer == cciEnum)
                                _simplePayment.PayerPartyReference.HRef = data;
                            else
                                _simplePayment.ReceiverPartyReference.HRef = data;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        //
                        case CciEnum.paymentAmount_amount:
                            _simplePayment.PaymentAmount.Amount.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;

                        case CciEnum.paymentAmount_currency:
                            _simplePayment.PaymentAmount.Currency = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;//Afin d'arrondir PaymentAmount
                            break;
                        //
                        case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                            _simplePayment.PaymentDate.AdjustableDateSpecified = StrFunc.IsFilled(data);
                            if (_simplePayment.PaymentDate.AdjustableDateSpecified)
                                _simplePayment.PaymentDate.AdjustableDate.UnadjustedDate.Value = data;
                            break;
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    //
                    if (isSetting)
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
                CreateInstance();
            
        }

        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
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
                        #endregion Reset variables

                        switch (cciEnum)
                        {
                            case CciEnum.payer:
                            case CciEnum.receiver:
                                string field = cciEnum.ToString() + "PartyReference";
                            // EG 20160404 Migration vs2013
                            //string clientId_Key;
                            FieldInfo fld = this._simplePayment.GetType().GetField(field);
                            //
                            if (null != fld)
                                    data = ((IReference)fld.GetValue(_simplePayment)).HRef;
                                break;
                            //----
                            case CciEnum.paymentAmount_amount:
                                data = _simplePayment.PaymentAmount.Amount.Value;
                                break;
                            //----
                            case CciEnum.paymentAmount_currency:
                                data = _simplePayment.PaymentAmount.Currency;
                                break;
                            //----

                            case CciEnum.paymentAmountOrigin_amount:
                                if (null != _simplePaymentOrigine)
                                    data = _simplePaymentOrigine.Amount.Value;
                                break;

                            case CciEnum.paymentAmountOrigin_currency:
                                if (null != _simplePaymentOrigine)
                                    data = _simplePaymentOrigine.Currency;
                                break;
                            //----
                            case CciEnum.paymentDate_adjustableDate_unadjustedDate:
                                if (_simplePayment.PaymentDate.AdjustableDateSpecified)
                                    data = _simplePayment.PaymentDate.AdjustableDate.UnadjustedDate.Value;
                                break;

                            //----
                            case CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC:
                                if (_simplePayment.PaymentDate.AdjustableDateSpecified)
                                    data = _simplePayment.PaymentDate.AdjustableDate.DateAdjustments.BusinessDayConvention.ToString();
                                break;
                            #region default
                            default:
                                isSetting = false;
                                break;
                            #endregion
                        }
                        //
                        if (isSetting)
                            _ccis.InitializeCci(cci, sql_Table, data);
                        //
                    }
                }
                //Look
                if (false == Cci(CciEnum.payer).IsMandatory)
                    SetEnabled(Cci(CciEnum.payer).IsFilledValue);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
                bool isOk = (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
                isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
                return isOk;
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            try
            {
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                    //
                    CciEnum key = CciEnum.unknown;
                    if (Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    switch (key)
                    {

                        case CciEnum.payer:
                            _ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                            if (StrFunc.IsEmpty(pCci.NewValue) && (false == Cci(CciEnum.payer).IsMandatory))
                                Clear();
                            //
                            if (false == Cci(CciEnum.payer).IsMandatory)
                                SetEnabled(pCci.IsFilledValue);
                            //
                            break;
                        case CciEnum.receiver:
                            _ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                            break;


                        case CciEnum.paymentAmount_amount:
                        case CciEnum.paymentAmount_currency:
                            _ccis.ProcessInitialize_AroundAmount(CciClientId(CciEnum.paymentAmount_amount), _simplePayment.PaymentAmount,
                                (CciEnum.paymentAmount_currency == key));
                            break;

                        default:
                            break;
                    }
                }


            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void RefreshCciEnabled()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void RemoveLastItemInArray(string _)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            try
            {
                if (IsCci(CciEnum.paymentDate_adjustableDate_dateAdjustments_bDC, pCci))
                    Ccis.SetDisplayBusinessDayAdjustments(pCci, SimplePayment.PaymentDate.AdjustableDate.DateAdjustments);
            }
            catch (Exception) { throw; }
        }
        #endregion IContainerCciFactory Members

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            try
            {
                bool isAlways = IsSpecified;
                //Payer
                _ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, isAlways);
                //
                //Receiver
                //Pas de synchrosisation du receiver lorsqu'il est à blanc et que l'utilisateur remplace la counterpartie à blanc par une donnée
                isAlways = IsSpecified;
                _ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, isAlways);
            }
            catch (Exception) { throw; }
        }
        #endregion

        #region Membres de IContainerCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
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
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion ITradeCci

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return Cci(CciEnum.payer).IsFilled; } }
        #endregion IContainerCciSpecified

        #region Membres de ITradeCciQuoteBasis
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_QuoteBasis(CustomCaptureInfo pCci)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public string GetCurrency1(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public string GetCurrency2(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public string GetBaseCurrency(CustomCaptureInfo pCci)
        {
            string ret = string.Empty;
            return ret;
        }
        #endregion

        #region Membres de  ICciPresentation Membres
        // EG 20180423 Analyse du code Correction [CA2200]
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
        }
        #endregion
        #endregion Interfaces

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void Clear()
        {
            try
            {
                CciTools.SetCciContainer(this, "CciEnum", "NewValue", string.Empty);
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        // EG 20180423 Analyse du code Correction [CA2200]
        public void SetEnabled(Boolean pIsEnabled)
        {
            try
            {
                CciTools.SetCciContainer(this, "CciEnum", "IsEnabled", pIsEnabled);
                //Doit tjs être Enabled 
                Cci(CciEnum.payer).IsEnabled = true;
                //
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20180423 Analyse du code Correction [CA2200]
        private void CreateInstance()
        {
            try
            {
                CciTools.CreateInstance(this, _simplePayment, "CciEnum");
            }
            catch (Exception) { throw; }
        }
        #endregion
    }
}