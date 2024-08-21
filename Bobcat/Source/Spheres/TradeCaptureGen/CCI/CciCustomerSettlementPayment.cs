#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;
using System.Reflection;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciCustomerSettlementPayment.
    /// </summary>
    public class CciCustomerSettlementPayment : IContainerCci, IContainerCciFactory, IContainerCciSpecified , ICciPresentation
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("currency")]
            currency,
            [System.Xml.Serialization.XmlEnumAttribute("amount")]
            amount,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private ICustomerSettlementPayment customerSettlementPayment;
        private readonly string prefix;
        private readonly CciTradeBase trade;
        private readonly TradeCustomCaptureInfos _ccis;
        private CciExchangeRate _cciExchangeRate;
        #endregion
        #region Accessors
        #region ccis
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        #endregion ccis
        #region cciExchangeRate
        public CciExchangeRate CciExchangeRate
        {
            get { return _cciExchangeRate; }
        }
        #endregion cciExchangeRate
        #region CustomerSettlementPayment
        public ICustomerSettlementPayment CustomerSettlementPayment
        {
            set { customerSettlementPayment = value; }
            get { return customerSettlementPayment; }
        }
        #endregion CustomerSettlementPayment
        #endregion Accessors
        #region Constructors
        public CciCustomerSettlementPayment(CciTradeBase pTrade, ICustomerSettlementPayment pCustomerSettlementPayment, string pPrefix)
        {
            trade = pTrade;
            _ccis = pTrade.Ccis;
            customerSettlementPayment = pCustomerSettlementPayment;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            //
            if (null != customerSettlementPayment)
                _cciExchangeRate = new CciExchangeRate(pTrade, customerSettlementPayment.Rate, prefix + "rate");
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCci members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue)];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci members
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
            _cciExchangeRate.AddCciSystem();
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
            _cciExchangeRate.CleanUp();
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            try
            {
                bool isSetting;
                string data = string.Empty;
                Type tCciEnum = typeof(CciEnum);
                CustomCaptureInfosBase.ProcessQueueEnum processQueue;
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    CustomCaptureInfo cci = Ccis[prefix + enumName];
                    if ((cci != null) && (cci.HasChanged))
                    {
                        isSetting = true;
                        data = cci.NewValue;
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.amount:
                                #region amount
                                customerSettlementPayment.AmountSpecified = StrFunc.IsFilled(data);
                                if (customerSettlementPayment.AmountSpecified)
                                    customerSettlementPayment.Amount.Value = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion amount
                                break;
                            case CciEnum.currency:
                                #region currency
                                customerSettlementPayment.Currency = data;
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion currency
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                            Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
                _cciExchangeRate.Dump_ToDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, customerSettlementPayment);
            if (null == _cciExchangeRate)
                _cciExchangeRate = new CciExchangeRate(trade, customerSettlementPayment.Rate, prefix + "rate");
            _cciExchangeRate.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            try
            {
                string data;
                bool isSetting;
                SQL_Table sql_Table;
                //
                Type tCciEnum = typeof(CciEnum);
                foreach (string enumName in Enum.GetNames(tCciEnum))
                {
                    CustomCaptureInfo cci = Ccis[prefix + enumName];
                    if (cci != null)
                    {
                        #region Reset variables
                        data = string.Empty;
                        isSetting = true;
                        sql_Table = null;
                        #endregion Reset variables

                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.amount:
                                #region amount
                                if (customerSettlementPayment.AmountSpecified)
                                    data = customerSettlementPayment.Amount.Value;
                                #endregion amount
                                break;
                            case CciEnum.currency:
                                #region currency
                                data = customerSettlementPayment.Currency;
                                #endregion currency
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                            Ccis.InitializeCci(cci, sql_Table, data);
                    }
                }
                //Look
                if (false == Cci(CciEnum.currency).IsMandatory)
                    SetEnabled(Cci(CciEnum.currency).IsFilledValue);
                //
                if (null != _cciExchangeRate)
                    _cciExchangeRate.Initialize_FromDocument();
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (this.IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);
                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                switch (key)
                {
                    case CciEnum.currency:
                        if (pCci.IsEmpty)
                            Clear();
                        SetEnabled(pCci.IsFilled);
                        break;
                    default:
                        break;
                }
            }
            _cciExchangeRate.ProcessInitialize(pCci);
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
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            _cciExchangeRate.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCciSpecified Members
        public bool IsSpecified { get { return Cci(CciEnum.currency).IsFilled; } }
        #endregion IContainerCciSpecified Members
        #endregion Interfaces
        #region Methods
        #region Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
            _cciExchangeRate.Clear();
        }
        #endregion Clear
        #region SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
            //Doit tjs être Enabled 
            Cci(CciEnum.currency).IsEnabled = true;
            _cciExchangeRate.SetEnabled(pIsEnabled);
        }
        #endregion SetEnabled
        #endregion Methods

        #region ICciPresentation Membres

        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            if (null != _cciExchangeRate)
                _cciExchangeRate.DumpSpecific_ToGUI(pPage);
        }

        #endregion
    }
}
