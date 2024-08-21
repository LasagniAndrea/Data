using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EFS.TradeInformation
{
    /// <summary>
    /// Cci de gestion de l'exntension de la classe EnvironmentalPhysicalLeg
    /// - Caractéristique de production de l'énergie dans les cas d'une GoO.
    /// </summary>
    // EG 20221201 [25639] [WI484] New
    public class CciEnvironmentalProductionFeatures : IContainerCci, IContainerCciFactory
    {
        #region Membres
        private readonly string _prefix = string.Empty;
        private readonly CciTrade _cciTrade;
        private readonly IEnvironmentalProductionFeatures _productionFeatures;

        public CciProductionDeviceScheme[] _cciProductionDevice;

        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            technology,
            region,
            device,
            unknown
        }

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;

        public CciProductionDeviceScheme[] CciProductionDevice
        {
            get { return _cciProductionDevice; }
        }

        public int DeviceLength
        {
            get { return ArrFunc.IsFilled(_cciProductionDevice) ? _cciProductionDevice.Length : 0; }
        }

        #endregion

        #region constructor
        public CciEnvironmentalProductionFeatures(CciTrade pCCiTrade, string pPrefix, IEnvironmentalProductionFeatures pProductionFeatures)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _productionFeatures = pProductionFeatures;
        }
        #endregion

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

        #region Membres de IContainerCciFactory
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _productionFeatures);
            InitializeProductionDevice_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.technology), true, TypeData.TypeDataEnum.@string);

            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.AddCciSystem();

        }
        /// <summary>
        /// Affectation les ccis par lecture du dataDocument
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
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.technology:
                            if (_productionFeatures.TechnologySpecified)
                                data = ReflectionTools.ConvertEnumToString<CommodityTechnologyTypeEnum>(_productionFeatures.Technology);
                            break;
                        case CciEnum.region:
                            if (_productionFeatures.RegionSpecified)
                                data = _productionFeatures.Region.Value.ToString();
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.Initialize_FromDocument();

        }
        /// <summary>
        /// Affectation du dataDocument à partir des ccis 
        /// </summary>
        public void Dump_ToDocument()
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
                        case CciEnum.technology:
                            _productionFeatures.TechnologySpecified = StrFunc.IsFilled(data);
                            if (_productionFeatures.TechnologySpecified)
                                _productionFeatures.Technology = ReflectionTools.ConvertStringToEnumOrDefault<CommodityTechnologyTypeEnum>(data);
                            break;
                        case CciEnum.region:
                            _productionFeatures.RegionSpecified = StrFunc.IsFilled(data);
                            if (_productionFeatures.RegionSpecified)
                            {
                                _productionFeatures.Region.Value = data;
                                _productionFeatures.Region.Scheme = "http://www.efsml.org/coding-scheme/environmental-production-region";
                            }
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.Dump_ToDocument();

        }
        /// <summary>
        /// Affectation (pré-proposition) d'un cci à partir du cci {pCCi}. {pCCi} vient d'être modifié.
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Key = CciContainerKey(pCci.ClientId_WithoutPrefix);

                CciEnum key = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Key))
                    key = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Key);

                switch (key)
                {
                    default:
                        break;
                }
            }

            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.ProcessInitialize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            //Nothing TODO
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.ProcessExecute(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.ProcessExecuteAfterSynchronize(pCci);
        }
        /// <summary>
        ///  Retourne true si le CCI représente un payer ou un receiver 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;

        }
        /// <summary>
        ///  Nettoyage du dataDocument 
        /// </summary>
        public void CleanUp()
        {
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.CleanUp();
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.RefreshCciEnabled();

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.SetDisplay(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            foreach (CciProductionDeviceScheme item in _cciProductionDevice)
                item.Initialize_Document();

        }
        #endregion

        private void InitializeProductionDevice_FromCci()
        {
            IScheme[] device = _productionFeatures.Device;

            bool isOk = true;
            int index = -1;

            ArrayList lst = new ArrayList();
            lst.Clear();
            while (isOk)
            {
                index += 1;

                CciProductionDeviceScheme cciDeviceItem = new CciProductionDeviceScheme(_cciTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_environmentalProductionDevice, index + 1, null);

                isOk = Ccis.Contains(cciDeviceItem.CciClientId(CciSecurityIdSourceScheme.CciEnum.scheme));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(device) || (index == device.Length))
                    {
                        ReflectionTools.AddItemInArray(_productionFeatures, "device", index);
                        device = _productionFeatures.Device;
                    }
                    lst.Add(cciDeviceItem);
                }
            }

            CciProductionDeviceScheme[] cciDevice = new CciProductionDeviceScheme[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                cciDevice[i] = (CciProductionDeviceScheme)lst[i];
                cciDevice[i].Initialize_FromCci();
            }
            _cciProductionDevice = cciDevice;

        }

        /// <summary>
        /// Reset des Ccis suite à modification de la plateforme
        /// </summary>
        // EG 20171113 [23509] New 
        public void ResetCciFacilityHasChanged()
        {
            List<CciEnum> lst = Enum.GetValues(typeof(CciEnum)).Cast<CciEnum>().ToList();
            lst.ForEach(item =>
            {
                CustomCaptureInfo cci = Cci(item);
                if (null != cci)
                    cci.Reset();
            });
        }
    }
}
