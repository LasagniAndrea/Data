using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using Tz = EFS.TimeZone;

namespace EFS.TradeInformation
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class CCiPhysicalSwapLeg : IContainerCci, IContainerCciFactory, IContainerCciPayerReceiver
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly IPhysicalLeg _physicalLeg;
        /// <summary>
        /// 
        /// </summary>
        protected CciTradeBase _cciTrade;
        /// <summary>
        /// 
        /// </summary>
        private readonly string _prefix;


        #region Enums
        // EG 20171113 [23509] Add FacilityHasChanged
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("payer")]
            payer,
            [System.Xml.Serialization.XmlEnumAttribute("receiver")]
            receiver,
            [CciGroupAttribute(name = "FacilityHaschanged")]
            [System.Xml.Serialization.XmlEnumAttribute("commodityAsset")]
            commodityAsset,
            unknown
        }
        #endregion

        #region accessor
        /// EG 20171016 [23509] New
        public virtual string DeliveryLocation
        {
            get { return Tz.Tools.UniversalTimeZone; }
        }
        public virtual CommodityContractClassEnum CommodityContractClass
        {
            get { return CommodityContractClassEnum.Undefined; }
        }
        // EG 20221201 [25639] [WI484] New
        public virtual GasProductTypeEnum GasProductType
        {
            get { return GasProductTypeEnum.Undefined; }
        }
        // EG 20221201 [25639] [WI484] New
        public virtual ElectricityProductTypeEnum ElectricityProductType
        {
            get { return ElectricityProductTypeEnum.Undefined; }
        }
        // EG 20221201 [25639] [WI484] New
        public virtual EnvironmentalProductTypeEnum EnvironmentalProductType
        {
            get { return EnvironmentalProductTypeEnum.Undefined; }
        }
        // EG 20221201 [25639] [WI484] New
        public virtual string CommodityProductType
        {
            get { return string.Empty; }
        }
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        #endregion



        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCiTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pGasPhysical"></param>
        public CCiPhysicalSwapLeg(CciTrade pCCiTrade, string pPrefix, IPhysicalLeg pPhysicalLeg)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix;
            _physicalLeg = pPhysicalLeg;
        }



        #region Membres de IContainerCci
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
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
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
        #endregion Membres de IContainerCci

        #region Membres de IContainerCciPayerReceiver
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdPayer
        {
            get { return CciClientId(CciEnum.payer.ToString()); }
        }
        /// <summary>
        /// 
        /// </summary>
        public string CciClientIdReceiver
        {
            get { return CciClientId(CciEnum.receiver.ToString()); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLastValue"></param>
        /// <param name="pNewValue"></param>
        public void SynchronizePayerReceiver(string pLastValue, string pNewValue)
        {
            Ccis.Synchronize(CciClientIdPayer, pLastValue, pNewValue, true);
            Ccis.Synchronize(CciClientIdReceiver, pLastValue, pNewValue, true);
        }
        #endregion IContainerCciPayerReceiver Members

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _physicalLeg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public virtual void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.payer), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.receiver), true, TypeData.TypeDataEnum.@string);
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_FromDocument()
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.payer:
                            data = _physicalLeg.PayerPartyReference.HRef;
                            break;
                        case CciEnum.receiver:
                            data = _physicalLeg.ReceiverPartyReference.HRef;
                            break;
                        case CciEnum.commodityAsset:
                            if (_physicalLeg.CommodityAssetSpecified)
                            {
                                int idAsset = _physicalLeg.CommodityAsset.OTCmlId;
                                SQL_AssetCommodity sql_Asset = new SQL_AssetCommodity(_cciTrade.CSCacheOn, idAsset);
                                if (sql_Asset.IsLoaded && (sql_Asset.RowsCount == 1))
                                {
                                    sql_Table = sql_Asset;
                                    data = sql_Asset.Identifier;
                                }
                            }
                            break;
                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[_prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.payer:
                            _physicalLeg.PayerPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.receiver:
                            _physicalLeg.ReceiverPartyReference.HRef = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.commodityAsset:
                            DumpAssetCommodity_ToDocument(cci, data);
                            _physicalLeg.CommodityAssetSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    case CciEnum.payer:
                        Ccis.Synchronize(CciClientIdReceiver, pCci.NewValue, pCci.LastValue);
                        break;
                    case CciEnum.receiver:
                        Ccis.Synchronize(CciClientIdPayer, pCci.NewValue, pCci.LastValue);
                        break;
                    case  CciEnum.commodityAsset:
                        if (_physicalLeg.CommodityAssetSpecified && (null != pCci.Sql_Table))
                            SetCommodityContractCharacteristics();
                        break;
                    default:
                        break;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecute(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public virtual bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            bool isOk = false;
            isOk = isOk || (CciClientIdPayer == pCci.ClientId_WithoutPrefix);
            isOk = isOk || (CciClientIdReceiver == pCci.ClientId_WithoutPrefix);
            return isOk;

        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void CleanUp()
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void RefreshCciEnabled()
        {
            //Nothing Todo
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public virtual void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.commodityAsset, pCci))
            {
                pCci.Display = string.Empty;
                if (pCci.Sql_Table is SQL_AssetBase asset)
                    pCci.Display = asset.SetDisplay;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Initialize_Document()
        {
            //Nothing Todo
        }
        #endregion

        


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <param name="pData"></param>
        // EG 20221201 [25639] [WI484] Upd
        private void DumpAssetCommodity_ToDocument(CustomCaptureInfo pCci, string pData)
        {
            bool isLoaded = false;
            bool isFound = false;
            pCci.ErrorMsg = string.Empty;
            pCci.Sql_Table = null;

            SQL_AssetCommodityContract sqlAsset = null;

            if (StrFunc.IsFilled(pData))
            {
                SQL_TableWithID.IDType IDTypeSearch = SQL_TableWithID.IDType.Identifier;
                string searchAsset = (string)SystemSettings.GetAppSettings("Spheres_TradeSearch_commodityAsset", typeof(string), IDTypeSearch.ToString());
                string[] aSearchAsset = searchAsset.Split(";".ToCharArray());
                int searchCount = aSearchAsset.Length;
                for (int j = 0; j < searchCount; j += 1)
                {
                    try { IDTypeSearch = (SQL_TableWithID.IDType)Enum.Parse(typeof(SQL_TableWithID.IDType), aSearchAsset[j], true); }
                    catch { continue; }

                    for (int i = 0; i < 3; i++)
                    {
                        string dataToFind = pData.Replace("%", SQL_TableWithID.StringForPERCENT);
                        if (i == 1)
                            dataToFind = pData.Replace(" ", "%") + "%";
                        else if (i == 2)
                            dataToFind = "%" + pData.Replace(" ", "%") + "%";

                        sqlAsset = new SQL_AssetCommodityContract(_cciTrade.CSCacheOn, IDTypeSearch, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes)
                        {
                            CommodityContractClass_In = CommodityContractClass,
                            GasProductType_In = GasProductType,
                            ElectricityProductType_In = ElectricityProductType,
                            EnvironmentalProductType_In = EnvironmentalProductType
                        };

                        _cciTrade.TradeCommonInput.Product.GetMarket(_cciTrade.CSCacheOn, null, out SQL_Market sqlMarket);
                        if (null != sqlMarket)
                            sqlAsset.IdM_In = sqlMarket.Id;

                        sqlAsset.MaxRows = 2; //NB: Afin de retourner au max 2 lignes (s'ignifiant qu'il y en a plus d'une)

                        isLoaded = sqlAsset.IsLoaded;
                        isFound = isLoaded && (1 == sqlAsset.RowsCount);
                        //
                        if (isLoaded)
                            break;
                    }
                    if (isLoaded)
                        break;
                }
            }

            if (isFound)
            {
                pCci.NewValue = sqlAsset.Identifier;
                pCci.Sql_Table = sqlAsset;
                pCci.ErrorMsg = string.Empty;

            }
            else
            {
                pCci.Display = string.Empty;
                if (pCci.IsFilled)
                {
                    if (isLoaded)
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetNotUnique");
                    else
                        pCci.ErrorMsg = Ressource.GetString("Msg_AssetNotFound");
                }
            }

            _physicalLeg.CommodityAssetSpecified = StrFunc.IsFilled(pData);
            if (null != pCci.Sql_Table)
                _physicalLeg.SetAssetCommodityContract((SQL_AssetCommodityContract)pCci.Sql_Table);
            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {

            CustomCaptureInfo cci = Cci(CciEnum.commodityAsset);
            if (null != cci)
                pPage.SetOpenFormReferential(cci, Cst.OTCml_TBL.ASSET_COMMODITY);
        }
        

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SetCommodityContractCharacteristics()
        {
        }

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New 
        public virtual void ResetCciFacilityHasChanged()
        {
        }

    }
}
