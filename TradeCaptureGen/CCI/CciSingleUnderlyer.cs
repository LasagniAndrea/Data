using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using FpML.Interface;
using FpML.v44.Shared;
using System;
using System.Reflection;


namespace EFS.TradeInformation
{

    public class CciSingleUnderlyer : IContainerCci, IContainerCciFactory, IContainerCciSpecified 
    {
        #region Members
        public ISingleUnderlyer singleUnderlyer;
        public string prefix;
        private readonly CciTradeBase cciTrade;
        #endregion

        #region Enum
        // EG 20150410 [20513] BANCAPERTA
        public enum CciEnum
        {
            #region underlyingAsset
            [System.Xml.Serialization.XmlEnumAttribute("equity")]
            equity,
            [System.Xml.Serialization.XmlEnumAttribute("index")]
            index,
            [System.Xml.Serialization.XmlEnumAttribute("bond")]
            bond,
            [System.Xml.Serialization.XmlEnumAttribute("debtSecurity")]
            debtSecurity,
            #endregion
            #region openUnits
            [System.Xml.Serialization.XmlEnumAttribute("openUnits")]
            openUnits,
            #endregion
            unknown,
        }
        #endregion Enum

        #region AssetTypeEnum
        // EG 20150410 [20513] BANCAPERTA
        public enum AssetTypeEnum
        {
            bond,
            debtSecurity,
            cash,
            convertibleBond,
            deposit,
            equity,
            exchangeTradedFund,
            future,
            fxRate,
            index,
            loan,
            mortgage, mutualFund,
            rateIndex,
            simpleCreditDefaultSwap,
            simpleFra,
            simpleIrsSwap,
        }
        #endregion AssetTypeEnum

        #region Acessors
        // EG 20150410 [20513] BANCAPERTA
        // EG 20180423 Analyse du code Correction [CA1065]
        public AssetTypeEnum AssetType
        {
            get
            {
                AssetTypeEnum ret;
                if (Ccis.Contains(CciClientId(CciEnum.bond)))
                    ret = AssetTypeEnum.bond;
                else if (Ccis.Contains(CciClientId(CciEnum.debtSecurity)))
                    ret = AssetTypeEnum.debtSecurity;
                else if (Ccis.Contains(CciClientId(CciEnum.index)))
                    ret = AssetTypeEnum.index;
                else if (Ccis.Contains(CciClientId(CciEnum.equity)))
                    ret = AssetTypeEnum.equity;
                else
                    throw new InvalidOperationException(" asset type  Not Implemented");
                return ret;
            }
        }
        public TradeCustomCaptureInfos Ccis => cciTrade.Ccis;
        #endregion

        #region Constructor
        public CciSingleUnderlyer(CciTradeBase pCciTrade, ISingleUnderlyer pSingleUnderlyer, string pPrefix)
        {
            cciTrade = pCciTrade;
            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            singleUnderlyer = pSingleUnderlyer;
        }
        #endregion

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

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, singleUnderlyer);
            //
            IUnderlyingAsset asset = singleUnderlyer.UnderlyingAsset;
            //
            if (null == asset.InstrumentId)
                asset.InstrumentId = new InstrumentId[] { new InstrumentId() };
            if (null == asset.Description)
                asset.Description = new EFS_String(string.Empty);
            if (null == asset.Currency)
                asset.Currency = new Currency();
            if (null == asset.ExchangeId)
                asset.ExchangeId = new ExchangeId();
            if (null == asset.ClearanceSystem)
                asset.ClearanceSystem = new ClearanceSystem();
            if (null == asset.Definition)
                asset.Definition = new ProductReference();
        }
        #endregion
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string data;
            bool isSetting;
            SQL_Table sql_Table;
            string clientId_Key;
            //
            foreach (CustomCaptureInfo cci in Ccis)
            {
                if (IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        #region asset
                        case CciEnum.bond:
                        case CciEnum.equity:
                        case CciEnum.index:
                            try
                            {
                                IUnderlyingAsset underlyingAsset = (IUnderlyingAsset)(singleUnderlyer.UnderlyingAsset);
                                int idAsset = underlyingAsset.OTCmlId;
                                if (idAsset > 0)
                                {
                                    SQL_AssetBase sql_asset = GetAssetToFind(key, idAsset);
                                    if (sql_asset.IsLoaded)
                                    {
                                        data = sql_asset.Identifier;
                                        sql_Table = sql_asset;
                                    }
                                }
                            }
                            catch
                            {
                                cci.Sql_Table = null;
                                data = string.Empty;
                            }
                            break;
                        #endregion

                        #region openUnits
                        case CciEnum.openUnits:
                            if (singleUnderlyer.OpenUnitsSpecified)
                                data = singleUnderlyer.OpenUnits.Value;
                            break;
                        #endregion

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        #endregion
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        public void Dump_ToDocument()
        {
            string data;
            string clientId_Key;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            foreach (CustomCaptureInfo cci in Ccis)
            {
                //On ne traite que les contrôle dont le contenu à changé
                if ((cci.HasChanged) && IsCciOfContainer(cci.ClientId_WithoutPrefix))
                {
                    #region Reset variables
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    clientId_Key = CciContainerKey(cci.ClientId_WithoutPrefix);
                    data = cci.NewValue;
                    bool isSetting = true;
                    #endregion Reset variables
                    //
                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);
                    //
                    switch (key)
                    {
                        #region asset
                        case CciEnum.bond:
                        case CciEnum.equity:
                        case CciEnum.index:
                            SQL_AssetBase sql_asset = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            if (StrFunc.IsFilled(data))
                            {
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";

                                    sql_asset = GetAssetToFind(key, dataToFind);
                                    isLoaded = sql_asset.IsLoaded && (sql_asset.RowsCount == 1);
                                    if (isLoaded)
                                        break;
                                }
                                //
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_asset.Identifier;
                                    cci.Sql_Table = sql_asset;

                                    IUnderlyingAsset underlyingAsset = (IUnderlyingAsset)singleUnderlyer.UnderlyingAsset;
                                    underlyingAsset.OTCmlId = sql_asset.Id;
                                    underlyingAsset.InstrumentId[0].Value = sql_asset.Identifier;
                                    underlyingAsset.DescriptionSpecified = ObjFunc.IsFilled(sql_asset.Description);
                                    underlyingAsset.Description = new EFS_String(sql_asset.Description);
                                    underlyingAsset.CurrencySpecified = ObjFunc.IsFilled(sql_asset.IdC);
                                    if (underlyingAsset.CurrencySpecified)
                                    {
                                        underlyingAsset.Currency.Value = sql_asset.IdC;
                                    }

                                    underlyingAsset.ExchangeIdSpecified = ObjFunc.IsFilled(sql_asset.IdM);
                                    if (underlyingAsset.ExchangeIdSpecified)
                                    {
                                        //PL 20130208 ISO-GLOP
                                        //equityAsset.exchangeId.Value = sql_asset.Market_Identifier;
                                        underlyingAsset.ExchangeId.Value = sql_asset.Market_FIXML_SecurityExchange;
                                        underlyingAsset.ExchangeId.OTCmlId = sql_asset.IdM;
                                    }

                                    underlyingAsset.ClearanceSystemSpecified = ObjFunc.IsFilled(sql_asset.ClearanceSystem);
                                    if (underlyingAsset.ClearanceSystemSpecified)
                                    {
                                        underlyingAsset.ClearanceSystem.Value = sql_asset.ClearanceSystem;
                                    }
                                }

                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_AssetNotFound") : string.Empty);
                            }
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;//Afin de préproposer les BCs
                            break;
                        #endregion

                        #region openUnits
                        case CciEnum.openUnits:
                            singleUnderlyer.OpenUnitsSpecified = StrFunc.IsFilled(data);
                            if (singleUnderlyer.OpenUnitsSpecified)
                                singleUnderlyer.OpenUnits.Value = data;
                            break;
                        #endregion

                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion default
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

        }
        #endregion
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
                    default:
                        break;
                }
            }
        }
        #endregion
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region CleanUp
        public void CleanUp()
        {
            // TODO : OK
        }
        #endregion
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            try
            {
                if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
                {
                    string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);

                    CciEnum key = CciEnum.unknown;
                    if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                        key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                    //
                    switch (key)
                    {
                        case CciEnum.equity:
                        case CciEnum.bond:
                        case CciEnum.index:
                            if (null != singleUnderlyer)
                            {
                                if (((ISingleUnderlyer)singleUnderlyer).UnderlyingAsset.DescriptionSpecified)
                                    pCci.Display = ((ISingleUnderlyer)singleUnderlyer).UnderlyingAsset.Description.Value;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex) { throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, ex); }
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

        #region private GetAssetToFind
        private SQL_AssetBase GetAssetToFind(CciEnum pCciEnum, int pIdAsset)
        {
            return GetAssetToFind(pCciEnum, SQL_TableWithID.IDType.Id, pIdAsset.ToString(), SQL_Table.ScanDataDtEnabledEnum.No);
        }
        private SQL_AssetBase GetAssetToFind(CciEnum pCciEnum, string pAsset)
        {
            return GetAssetToFind(pCciEnum, SQL_TableWithID.IDType.Identifier, pAsset, SQL_Table.ScanDataDtEnabledEnum.Yes);
        }
        private SQL_AssetBase GetAssetToFind(CciEnum pCciEnum, SQL_TableWithID.IDType pIdType, string pDataToFind, SQL_Table.ScanDataDtEnabledEnum pScanMode)
        {

            SQL_AssetBase sql_asset;
            switch (pCciEnum)
            {
                case CciEnum.equity:
                    sql_asset = new SQL_AssetEquity(cciTrade.CSCacheOn, pIdType, pDataToFind, pScanMode);
                    break;
                case CciEnum.index:
                    sql_asset = new SQL_AssetIndex(cciTrade.CSCacheOn, pIdType, pDataToFind, pScanMode);
                    break;
                case CciEnum.bond:
                    sql_asset = new SQL_AssetDebtSecurity(cciTrade.CSCacheOn, pIdType, pDataToFind, pScanMode);
                    break;
                default:
                    throw new NotImplementedException("Asset Type not implemented");
            }
            return sql_asset;
        }
        #endregion GetAssetToFind

        #region IContainerCciSpecified Membres

        // EG 20150410 [20513] BANCAPERTA
        public bool IsSpecified
        {
            get
            {
                Boolean ret = false;
                switch (this.AssetType)
                {
                    case AssetTypeEnum.index:
                        ret = (null != Cci(CciEnum.index).Sql_Table);
                        break;
                    case AssetTypeEnum.debtSecurity:
                        ret = (null != Cci(CciEnum.debtSecurity).Sql_Table);
                        break;
                    case AssetTypeEnum.bond:
                        ret = (null != Cci(CciEnum.bond).Sql_Table);
                        break;
                    case AssetTypeEnum.equity:
                        ret = (null != Cci(CciEnum.equity).Sql_Table);
                        break;
                    default:
                        break;
                }
                return ret;
            }
        }

        #endregion
    }
}