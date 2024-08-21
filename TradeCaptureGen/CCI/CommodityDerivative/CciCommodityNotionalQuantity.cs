using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.Interface;
using EfsML.Interface;
using System;

namespace EFS.TradeInformation
{
    /// <summary>
    /// CCI Commodity Notional.
    /// </summary>
    public class CciCommodityNotionalQuantity : IContainerCci, IContainerCciFactory
    {
        #region Membres
        private readonly string _prefix = string.Empty;
        private readonly CciTrade _cciTrade;
        private ICommodityNotionalQuantity _commodityNotionalQuantity;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("quantity")]
            quantity,
            [System.Xml.Serialization.XmlEnumAttribute("quantityUnit")]
            quantityUnit,
            [System.Xml.Serialization.XmlEnumAttribute("quantityFrequency")]
            quantityFrequency,
            unknown
        }

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;

        /// <summary>
        /// 
        /// </summary>
        public ICommodityNotionalQuantity CommodityNotionalQuantity
        {
            get { return _commodityNotionalQuantity; }
            set { _commodityNotionalQuantity = value; }
        }

        #endregion

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCCiTrade"></param>
        /// <param name="pPrefix"></param>
        /// <param name="pCommodityNotionalQuantity"></param>
        public CciCommodityNotionalQuantity(CciTrade pCCiTrade, string pPrefix, ICommodityNotionalQuantity pCommodityNotionalQuantity)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _commodityNotionalQuantity = pCommodityNotionalQuantity;
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
            CciTools.CreateInstance(this, _commodityNotionalQuantity);
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
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
                        case CciEnum.quantity:
                            data = _commodityNotionalQuantity.Quantity.Value;
                            break;
                        case CciEnum.quantityFrequency:
                            data = _commodityNotionalQuantity.QuantityFrequency.Value;
                            break;
                        case CciEnum.quantityUnit:
                            data = _commodityNotionalQuantity.QuantityUnit.Value;
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
                        case CciEnum.quantity:
                            _commodityNotionalQuantity.Quantity = new EFS_Decimal(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        case CciEnum.quantityFrequency:
                            _commodityNotionalQuantity.QuantityFrequency.Value = data;
                            break;
                        case CciEnum.quantityUnit:
                            _commodityNotionalQuantity.QuantityUnit.Value = data;
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
                    case CciEnum.quantity:

                        break;
                    case CciEnum.quantityFrequency:
                        break;
                    case CciEnum.quantityUnit:
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
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            //Nothing TODO
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
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            //Nothing TODO
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.quantityFrequency, pCci))
            {
                pCci.Display = _commodityNotionalQuantity.QuantityFrequency.Value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            //Nothing TODO
        }
        #endregion

    }
}

