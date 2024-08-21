#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciClassification
    /// <summary>
    /// CciLocalization
    /// </summary>
    public class CciClassification : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        #region Enum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("debtSecurityClass")]
            debtSecurityClass,
            [System.Xml.Serialization.XmlEnumAttribute("cfiCode")]
            cfiCode,
            [System.Xml.Serialization.XmlEnumAttribute("productTypeCode")]
            productTypeCode,
            [System.Xml.Serialization.XmlEnumAttribute("financialInstrumentProductTypeCode")]
            financialInstrumentProductTypeCode,
            [System.Xml.Serialization.XmlEnumAttribute("symbol")]
            symbol,
            [System.Xml.Serialization.XmlEnumAttribute("symbolSfx")]
            symbolSfx,
            unknown
        }
        #endregion
        #region Members
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase cciTrade;
        public IClassification classification;
        private readonly string prefix;
        #endregion
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        #endregion
        #region constructor
        public CciClassification(CciTradeBase pTrade, string pPrefix, IClassification pClassification)
        {
            cciTrade = pTrade;
            _ccis = cciTrade.Ccis;
            classification = pClassification;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion constructor
        //
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();
        }
        #endregion AddCciSystem
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {

        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
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
                    #endregion
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.debtSecurityClass:
                            #region debtSecurityClass
                            if (classification.DebtSecurityClassSpecified)
                                data = classification.DebtSecurityClass.Value;
                            #endregion debtSecurityClass
                            break;
                        case CciEnum.cfiCode:
                            #region cfiCode
                            if (classification.CfiCodeSpecified)
                                data = classification.CfiCode.Value;
                            #endregion cfiCode
                            break;
                        case CciEnum.financialInstrumentProductTypeCode:
                            #region financialInstrumentProductTypeCode
                            if (classification.FinancialInstrumentProductTypeCodeSpecified)
                                data = classification.FinancialInstrumentProductTypeCode.ToString();
                            #endregion financialInstrumentProductTypeCode
                            break;
                        case CciEnum.productTypeCode:
                            #region productTypeCode
                            if (classification.ProductTypeCodeSpecified)
                                data = classification.ProductTypeCode.ToString();
                            #endregion productTypeCode
                            break;
                        case CciEnum.symbol:
                            #region symbol
                            if (classification.SymbolSpecified)
                                data = classification.Symbol.Value;
                            #endregion symbol
                            break;
                        case CciEnum.symbolSfx:
                            #region symbolSfx
                            if (classification.SymbolSfxSpecified)
                                data = classification.SymbolSfx.Value;
                            #endregion symbolSfx
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
                }
            }

        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
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
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            string clientId;
            string clientId_Key;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            //
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId = cci.ClientId;
                    data = cci.NewValue;
                    isSetting = true;
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.debtSecurityClass:
                            #region debtSecurityClass
                            classification.DebtSecurityClass.Value = data;
                            classification.DebtSecurityClassSpecified = StrFunc.IsFilled(data);
                            #endregion debtSecurityClass
                            break;
                        case CciEnum.cfiCode:
                            #region cfiCode
                            classification.CfiCode.Value = data;
                            classification.CfiCodeSpecified = StrFunc.IsFilled(data);
                            #endregion cfiCode
                            break;
                        case CciEnum.financialInstrumentProductTypeCode:
                            #region financialInstrumentProductTypeCode
                            // 20090729 RD Eviter un plantage si la donnée est vide
                            if (StrFunc.IsFilled(data))
                            {
                                FinancialInstrumentProductTypeCodeEnum financialInstrumentProductTypeCodeEnum = (FinancialInstrumentProductTypeCodeEnum)System.Enum.Parse(typeof(FinancialInstrumentProductTypeCodeEnum), data);
                                classification.FinancialInstrumentProductTypeCode = financialInstrumentProductTypeCodeEnum;
                            }
                            //
                            classification.FinancialInstrumentProductTypeCodeSpecified = StrFunc.IsFilled(data);
                            #endregion financialInstrumentProductTypeCode
                            break;
                        case CciEnum.productTypeCode:
                            #region productTypeCode
                            // 20090729 RD Eviter un plantage si la donnée est vide
                            if (StrFunc.IsFilled(data))
                            {
                                ProductTypeCodeEnum productTypeCodeEnum = (ProductTypeCodeEnum)System.Enum.Parse(typeof(ProductTypeCodeEnum), data);
                                classification.ProductTypeCode = productTypeCodeEnum;
                            }
                            //
                            classification.ProductTypeCodeSpecified = StrFunc.IsFilled(data);
                            #endregion productTypeCode
                            break;
                        case CciEnum.symbol:
                            #region symbol
                            classification.Symbol.Value = data;
                            classification.SymbolSpecified = StrFunc.IsFilled(data);
                            #endregion symbol
                            break;
                        case CciEnum.symbolSfx:
                            #region symbolSfx
                            classification.SymbolSfx.Value = data;
                            classification.SymbolSfxSpecified = StrFunc.IsFilled(data);
                            #endregion symbolSfx
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    //
                    if (isSetting)
                        _ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion Dump_ToDocument
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {

        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion SetDisplay
        #region SetEnabled
        public void SetEnabled(bool pIsEnabled)
        {
            CciTools.SetCciContainer(this, "IsEnabled", pIsEnabled);
        }
        #endregion SetEnabled
        #region Initialize_Document
        public void Initialize_Document()
        {
            classification.DebtSecurityClass.Scheme = "http://www.euro-finance-systems.fr/otcml/SecurityClass";
        }
        #endregion Initialize_Document
        #endregion
        //
        #region  IContainerCci Membres
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }

        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }

        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(prefix);
        }
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion
        //
        #region IContainerCciSpecified Membres
        public bool IsSpecified { get { return (Cci(CciEnum.cfiCode).IsFilled || Cci(CciEnum.debtSecurityClass).IsFilled || Cci(CciEnum.financialInstrumentProductTypeCode).IsFilled || Cci(CciEnum.productTypeCode).IsFilled); } }
        #endregion
        //
        #region Methods
        #region private CreateInstance
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, classification, "CciEnum");
        }
        #endregion
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);

        }
        #endregion
        #endregion
    }
    #endregion
}
