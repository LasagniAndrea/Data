#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceRebateBracketCalculation. 
    /// </summary>
    public class CciInvoiceRebateBracketCalculation : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            #region RebateBracketCalculation
            [System.Xml.Serialization.XmlEnumAttribute("bracket.lowValue")]
            lowValue,
            [System.Xml.Serialization.XmlEnumAttribute("bracket.highValue")]
            highValue,
            [System.Xml.Serialization.XmlEnumAttribute("rate")]
            rate,
            [System.Xml.Serialization.XmlEnumAttribute("amount.amount")]
            amount,
            [System.Xml.Serialization.XmlEnumAttribute("amount.currencuy")]
            currency,
            #endregion RebateBracketCalculation
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        //private CciInvoiceRebateBracket m_CciInvoiceRebateBracket;
        private IRebateBracketCalculation m_RebateBracketCalculation;
        private readonly string m_Prefix;
        private readonly int m_Number;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        #endregion Members
        #region Accessors
        #region Bracket
        public IBracket Bracket
        {
            get { return m_RebateBracketCalculation.Bracket; }
            set { m_RebateBracketCalculation.Bracket = value; }
        }
        #endregion Bracket
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region Number
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < m_Number)
                    ret = m_Number.ToString();
                return ret;
            }
        }

        #endregion Number
        #region RebateBracketCalculation
        public IRebateBracketCalculation RebateBracketCalculation
        {
            get { return m_RebateBracketCalculation; }
            set { m_RebateBracketCalculation = value; }
        }
        #endregion RebateBracketCalculation
        #endregion Accessors
        #region Constructors
        public CciInvoiceRebateBracketCalculation(CciInvoiceRebateBracket pCciInvoiceRebateBracket, string pPrefix, int pInvoiceRebateBracketCalculationNumber, IRebateBracketCalculation pRebateBracketCalculation)
        {
            //m_CciInvoiceRebateBracket = pCciInvoiceRebateBracket;
            m_Ccis = pCciInvoiceRebateBracket.Ccis;
            m_Number = pInvoiceRebateBracketCalculationNumber;
            m_Prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            m_RebateBracketCalculation = pRebateBracketCalculation;
        }
        #endregion Constructors
        #region Interface
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.lowValue:
                            #region LowValue
                            Bracket.LowValueSpecified = cci.IsFilled;
                            Bracket.LowValue.Value = data;
                            #endregion LowValue
                            break;
                        case CciEnum.highValue:
                            #region HighValue
                            Bracket.HighValueSpecified = cci.IsFilled;
                            Bracket.HighValue.Value = data;
                            #endregion HighValue
                            break;
                        case CciEnum.rate:
                            #region Rate
                            m_RebateBracketCalculation.BracketRate.Value = data;
                            #endregion Rate
                            break;
                        case CciEnum.amount:
                            #region Amount
                            m_RebateBracketCalculation.BracketAmountSpecified = StrFunc.IsDecimalFilled(data);
                            m_RebateBracketCalculation.BracketAmount.Amount.Value = data;
                            #endregion Amount
                            break;
                        case CciEnum.currency:
                            #region Amount
                            m_RebateBracketCalculation.BracketAmount.Currency = data;
                            #endregion Amount
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
            CciTools.CreateInstance(this, m_RebateBracketCalculation);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.lowValue:
                            #region LowValue
                            if (Bracket.LowValueSpecified)
                                data = Bracket.LowValue.Value;
                            #endregion LowValue
                            break;
                        case CciEnum.highValue:
                            #region HighValue
                            if (Bracket.HighValueSpecified)
                                data = Bracket.HighValue.Value;
                            #endregion HighValue
                            break;
                        case CciEnum.rate:
                            #region Rate
                            data = m_RebateBracketCalculation.BracketRate.Value;
                            #endregion ManagementType
                            break;
                        case CciEnum.amount:
                            #region Amount
                            data = m_RebateBracketCalculation.BracketAmount.Amount.Value;
                            m_RebateBracketCalculation.BracketAmountSpecified = StrFunc.IsDecimalFilled(data);
                            #endregion Amount
                            break;
                        case CciEnum.currency:
                            #region Currency
                            data = m_RebateBracketCalculation.BracketAmount.Currency;
                            #endregion Currency
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }
                }
            }
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
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
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
        }
        #endregion ProcessInitialize
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
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
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
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #endregion Interface

        #region Methods
        #endregion Methods
    }
}