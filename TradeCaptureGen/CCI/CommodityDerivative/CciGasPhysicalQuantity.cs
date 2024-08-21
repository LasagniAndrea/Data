using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace EFS.TradeInformation
{
    /// <summary>
    /// CCI Amount of commodity per quantity frequency
    /// </summary>
    public class CciGasPhysicalQuantity :  IContainerCci, IContainerCciFactory
    {
        readonly string _prefix = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        private readonly CciTrade _cciTrade;
        /// <summary>
        /// 
        /// </summary>
        private readonly IGasPhysicalQuantity _gasPhysicalQuantity;
        
        /// <summary>
        /// 
        /// </summary>
        private CciCommodityNotionalQuantity[] _cciPhysicalQuantity;


        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("totalPhysicalQuantity.quantityUnit")]
            totalPhysicalQuantity_quantityUnit,
            [System.Xml.Serialization.XmlEnumAttribute("totalPhysicalQuantity.quantity")]
            totalPhysicalQuantity_quantity,
            unknown
        }


        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        public CciCommodityNotionalQuantity[] CciPhysicalQuantity
        {
            get
            {
                return _cciPhysicalQuantity;
            }
        }

        public IGasPhysicalQuantity GasPhysicalQuantity
        {
            get
            {
                return _gasPhysicalQuantity;
            }
        }

        #endregion

        #region constructor
        public CciGasPhysicalQuantity(CciTrade pCCiTrade, string pPrefix, IGasPhysicalQuantity pGasPhysicalQuantity)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _gasPhysicalQuantity = pGasPhysicalQuantity;
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
            CciTools.CreateInstance( this, _gasPhysicalQuantity);
            InitializePhysicalQuantity_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
            
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.AddCciSystem();
            }
        }
        /// <summary>
        /// 
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
                        case CciEnum.totalPhysicalQuantity_quantity:
                            if (_gasPhysicalQuantity.TotalPhysicalQuantitySpecified)
                                data = _gasPhysicalQuantity.TotalPhysicalQuantity.Quantity.Value;
                            break;
                        case CciEnum.totalPhysicalQuantity_quantityUnit:
                            if (_gasPhysicalQuantity.TotalPhysicalQuantitySpecified)
                                data = _gasPhysicalQuantity.TotalPhysicalQuantity.QuantityUnit.Value;
                            break;
                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }


            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.Initialize_FromDocument();
            }
        }
        /// <summary>
        /// 
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
                        case CciEnum.totalPhysicalQuantity_quantity:
                            _gasPhysicalQuantity.TotalPhysicalQuantitySpecified = StrFunc.IsFilled(data);
                            if (_gasPhysicalQuantity.TotalPhysicalQuantitySpecified)
                                _gasPhysicalQuantity.TotalPhysicalQuantity.Quantity.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.totalPhysicalQuantity_quantityUnit:
                            _gasPhysicalQuantity.TotalPhysicalQuantity.QuantityUnit.Value = data;
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.Dump_ToDocument();
            }

        }

        /// <summary>
        /// 
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

            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                {
                    item.ProcessInitialize(pCci);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.ProcessExecute(pCci);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.ProcessExecuteAfterSynchronize(pCci);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            Boolean ret = false;
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                {
                    ret = item.IsClientId_PayerOrReceiver(pCci);
                    if (ret)
                        break;
                }
            }
            return ret;

        }
        /// <summary>
        ///  
        /// </summary>
        public void CleanUp()
        {
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.CleanUp();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {

            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.RefreshCciEnabled();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.SetDisplay(pCci);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            if (ArrFunc.IsFilled(_cciPhysicalQuantity))
            {
                foreach (CciCommodityNotionalQuantity item in _cciPhysicalQuantity)
                    item.Initialize_Document();
            }
        }
        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsPremium"></param>
        private void InitializePhysicalQuantity_FromCci()
        {
            ArrayList lst = new ArrayList();
            Boolean saveSpecified = _gasPhysicalQuantity.PhysicalQuantitySpecified;

            ICommodityNotionalQuantity[] quantity = _gasPhysicalQuantity.PhysicalQuantity;

            bool isOk = true;
            int index = -1;
            while (isOk)
            {
                index += 1;
                string prefix = TradeCustomCaptureInfos.CCst.Prefix_physicalQuantity;
                if (index > 0)
                    prefix += index.ToString();

                CciCommodityNotionalQuantity cciQuantityItem = new CciCommodityNotionalQuantity(_cciTrade, _prefix + prefix, null);

                isOk = Ccis.Contains(cciQuantityItem.CciClientId(CciCommodityNotionalQuantity.CciEnum.quantity));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(quantity) || (index == quantity.Length))
                    {
                        ReflectionTools.AddItemInArray(_gasPhysicalQuantity, "physicalQuantity", index);
                        quantity = _gasPhysicalQuantity.PhysicalQuantity;
                    }

                    cciQuantityItem.CommodityNotionalQuantity = _gasPhysicalQuantity.PhysicalQuantity[index];
                    lst.Add(cciQuantityItem);
                }
            }

            CciCommodityNotionalQuantity[] cciQuantity = new CciCommodityNotionalQuantity[lst.Count];
            for (int i = 0; i < lst.Count; i++)
            {
                cciQuantity[i] = (CciCommodityNotionalQuantity)lst[i];
                cciQuantity[i].Initialize_FromCci();
            }

            _cciPhysicalQuantity = cciQuantity;
            _gasPhysicalQuantity.PhysicalQuantitySpecified = saveSpecified;
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
