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
    #region CciLocalization
    /// <summary>
    /// CciLocalization
    /// </summary>
    public class CciLocalization : IContainerCciFactory, IContainerCci, IContainerCciSpecified
    {
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("countryOfIssue")]
            countryOfIssue,
            [System.Xml.Serialization.XmlEnumAttribute("stateOrProvinceOfIssue")]
            stateOrProvinceOfIssue,
            [System.Xml.Serialization.XmlEnumAttribute("localeOfIssue")]
            localeOfIssue,
            unknown
        }
        #region Members
        private readonly TradeCustomCaptureInfos _ccis;
        private readonly CciTradeBase cciTrade;
        public ILocalization localization;
        private readonly string prefix;
        #endregion
        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return _ccis; }
        }
        #endregion
        #region constructor
        public CciLocalization(CciTradeBase pTrade, string pPrefix, ILocalization pLocalization)
        {
            cciTrade = pTrade;
            _ccis = cciTrade.Ccis;
            localization = pLocalization;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
        }
        #endregion constructor
        //
        #region AddCciSystem
        public void AddCciSystem()
        {
            //Don't erase
            CreateInstance();
        }
        #endregion AddCciSystem
        #region private CreateInstance
        private void CreateInstance()
        {
            CciTools.CreateInstance(this, localization, "CciEnum");
        }
        #endregion
        //
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
                        #endregion
                        //
                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.countryOfIssue:
                                #region countryOfIssue
                                if(localization.CountryOfIssueSpecified)
                                    data = localization.CountryOfIssue.Value;
                                #endregion countryOfIssue
                                break;
                            case CciEnum.localeOfIssue:
                                #region localeOfIssue
                                if (localization.LocaleOfIssueSpecified)
                                    data = localization.LocaleOfIssue.Value;
                                #endregion localeOfIssue
                                break;
                            case CciEnum.stateOrProvinceOfIssue:
                                #region stateOrProvinceOfIssue
                                if (localization.StateOrProvinceOfIssueSpecified)
                                    data = localization.StateOrProvinceOfIssue.Value;
                                #endregion stateOrProvinceOfIssue
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
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
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
                        case CciEnum.countryOfIssue:
                            #region countryOfIssue
                            localization.CountryOfIssue.Value = data;
                            localization.CountryOfIssueSpecified = StrFunc.IsFilled(data);
                            break;
                            #endregion countryOfIssue                        
                        case CciEnum.localeOfIssue:
                            #region localeOfIssue
                            localization.LocaleOfIssue.Value = data;
                            localization.LocaleOfIssueSpecified = StrFunc.IsFilled(data);
                            break;
                            #endregion localeOfIssue
                        case CciEnum.stateOrProvinceOfIssue:
                            #region stateOrProvinceOfIssue
                            localization.StateOrProvinceOfIssue.Value = data;
                            localization.StateOrProvinceOfIssueSpecified = StrFunc.IsFilled(data);
                            break;
                            #endregion stateOrProvinceOfIssue
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
        #region public Clear
        public void Clear()
        {
            CciTools.SetCciContainer(this, "NewValue", string.Empty);
        }
        #endregion
        #region Initialize_Document
        public void Initialize_Document()
        {
            localization.LocaleOfIssue.Scheme = "http://www.euro-finance-systems.fr/otcml/commune";
            localization.StateOrProvinceOfIssue.Scheme = "http://www.euro-finance-systems.fr/otcml/stateOrProvince";
        }
        #endregion Initialize_Document
        //
        #region Membres de IContainerCci
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
        public bool IsSpecified { get { return Cci(CciEnum.countryOfIssue).IsFilled; } }
        #endregion
    }
    #endregion
}
