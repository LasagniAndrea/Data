using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;

using System;

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciRemoveAllocation.
    /// </summary>
    public class CciUnderlyingDelivery : IContainerCci, ICciPresentation
    {
        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            quantity,
            settlMethod,
            physicalFactor,
            unlAssetCategory,
            currentDeliveryStep,
            date,
            deliveryStep,
            unknown,
        }

        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected string _prefix;
        /// <summary>
        /// 
        /// </summary>
        private readonly TradeUnderlyingDelivery _underlyingDelivery;
        /// <summary>
        /// 
        /// </summary>
        protected TradeCustomCaptureInfos _ccis;
        /// <summary>
        /// 
        /// </summary>
        protected CciTradeCommonBase _cciTrade;
        #endregion Members

        #region accessors
        public TradeCustomCaptureInfos Ccis
        {
            get
            {
                return _ccis;
            }
        }
        #endregion

        #region Constructors
        public CciUnderlyingDelivery(CciTradeCommonBase pTrade, TradeUnderlyingDelivery pUnderlyingDelivery, string pPrefix)
        {
            _underlyingDelivery = pUnderlyingDelivery;

            _cciTrade = pTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _ccis = (TradeCustomCaptureInfos)pTrade.Ccis;

        }
        #endregion Constructors

        #region IContainerCciFactory Members
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            //string physicalFactor = CciTools.GetFieldVariableName(new { _underlyingDelivery.physicalFactor }, typeof(TradeUnderlyingDelivery));
            //bool physicalFactorEnabled = (_underlyingDelivery.settlementMethodEnum == SettlMethodEnum.PhysicalSettlement);
            //ccis.Set(CciClientId(physicalFactor), "IsEnabled", physicalFactorEnabled);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo _)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void CleanUp()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        public void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = _ccis[_prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.date:
                            _underlyingDelivery.date.Value = data;
                            break;
                        case CciEnum.deliveryStep:
                            _underlyingDelivery.deliveryStep = data;
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
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
            CciTools.CreateInstance(this, _underlyingDelivery);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = _ccis[_prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.quantity:
                            data = _underlyingDelivery.quantity.Value;
                            break;
                        case CciEnum.settlMethod:
                            data = _underlyingDelivery.settlMethod.Value;
                            break;
                        case CciEnum.physicalFactor:
                            data = _underlyingDelivery.physicalFactor.Value;
                            break;
                        case CciEnum.unlAssetCategory:
                            data = _underlyingDelivery.unlAssetCategory.Value;
                            break;
                        case CciEnum.date:
                            data = _underlyingDelivery.date.Value;
                            break;
                        case CciEnum.currentDeliveryStep:
                            data = _underlyingDelivery.currentDeliveryStep;
                            break;
                        default:
                            #region default
                            isSetting = false;
                            break;
                            #endregion default
                    }
                    if (isSetting)
                        _ccis.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo _)
        {
        }
        #endregion IContainerCciFactory Members

        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue.ToString())];
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
            return _prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        public bool IsCci(string pEnumValue, CustomCaptureInfo pCci)
        {
            return (CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        #endregion
        #endregion IContainerCci Members

        #region ICciPresentation Membres
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
        }
        #endregion
    }
}
