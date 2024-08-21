using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;

namespace EFS.TradeInformation
{
    /// <summary>
    /// CCI Amount of commodity per quantity frequency
    /// </summary>
    public class CciGasDeliveryPeriods : IContainerCci, IContainerCciFactory
    {
        #region Membres
        private readonly string _prefix = string.Empty;
        private readonly CciTrade _cciTrade;
        private readonly IGasDeliveryPeriods _gasDeliveryPeriods;


        private readonly CCiPrevailingTime _cciSupplyStartTime;
        private readonly CCiPrevailingTime _cciSupplyEndTime;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            unknown
        }

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        public CCiPrevailingTime CciSupplyStartTime
        {
            get
            {
                return _cciSupplyStartTime;
            }
        }
        public CCiPrevailingTime CciSupplyEndTime
        {
            get
            {
                return _cciSupplyEndTime;
            }
        }
        #endregion

        #region constructor
        public CciGasDeliveryPeriods(CciTrade pCCiTrade, string pPrefix, IGasDeliveryPeriods pGasDeliveryPeriods)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix;

            _gasDeliveryPeriods = pGasDeliveryPeriods;

            _cciSupplyStartTime = new CCiPrevailingTime(pCCiTrade, _prefix + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_supplyStartTime, _gasDeliveryPeriods.SupplyStartTime);
            _cciSupplyEndTime = new CCiPrevailingTime(pCCiTrade, _prefix + CustomObject.KEY_SEPARATOR + TradeCustomCaptureInfos.CCst.Prefix_supplyEndTime, _gasDeliveryPeriods.SupplyEndTime);
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

            CciTools.CreateInstance(this, _gasDeliveryPeriods);

            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.Initialize_FromCci();
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.AddCciSystem();
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.AddCciSystem();
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
                    SQL_Table sql_Table = null;
                    bool isSetting;
                    #endregion Reset variables

                    switch (cciEnum)
                    {

                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.Initialize_FromDocument();
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.Initialize_FromDocument();

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
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    bool isSetting;
                    switch (keyEnum)
                    {

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.Dump_ToDocument();

            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.Dump_ToDocument();
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

            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.ProcessInitialize(pCci);
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.ProcessInitialize(pCci);

            if (null != _cciSupplyStartTime)
            {
                if (_cciSupplyStartTime.IsCci(CCiPrevailingTime.CciEnum.location, pCci))
                {
                    if (null != _cciSupplyEndTime)
                        Ccis.SetNewValue(_cciSupplyEndTime.CciClientId(CCiPrevailingTime.CciEnum.location), pCci.NewValue);  
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.ProcessExecute(pCci);
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.ProcessExecute(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.ProcessExecuteAfterSynchronize(pCci);
            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.ProcessExecuteAfterSynchronize(pCci);
        }
        /// <summary>
        ///  Retourne true si le CCI représente un payer ou un receiver 
        /// </summary>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            Boolean ret = false;
            if (ret == false)
            {
                if (null != _cciSupplyStartTime)
                    ret = _cciSupplyStartTime.IsClientId_PayerOrReceiver(pCci);
            }
            if (ret == false)
            {
                if (null != _cciSupplyEndTime)
                    ret = _cciSupplyEndTime.IsClientId_PayerOrReceiver(pCci);
            }

            return ret;
        }
        /// <summary>
        ///  Nettoyage du dataDocument 
        /// </summary>
        public void CleanUp()
        {
            if (null != _cciSupplyStartTime)
            {
                _cciSupplyStartTime.CleanUp();
                if ((null != _gasDeliveryPeriods.SupplyStartTime) &&
                    (false == CaptureTools.IsDocumentElementValid(_gasDeliveryPeriods.SupplyStartTime.HourMinuteTime.Value)))
                    _gasDeliveryPeriods.SupplyStartTimeSpecified = false;
            }

            if (null != _cciSupplyEndTime)
            {
                _cciSupplyEndTime.CleanUp();
                if ((null != _gasDeliveryPeriods.SupplyEndTime) &&
                    (false == CaptureTools.IsDocumentElementValid(_gasDeliveryPeriods.SupplyEndTime.HourMinuteTime.Value)))
                    _gasDeliveryPeriods.SupplyEndTimeSpecified = false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.RefreshCciEnabled();

            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.SetDisplay(pCci);

            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            if (null != _cciSupplyStartTime)
                _cciSupplyStartTime.Initialize_Document();

            if (null != _cciSupplyEndTime)
                _cciSupplyEndTime.Initialize_Document();
        }
        #endregion

        
    }
}
