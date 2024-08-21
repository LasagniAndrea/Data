using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EFS.TradeInformation
{
    /// <summary>
    /// CCI Specifies a set of Settlement Periods associated with an Electricity Transaction for delivery on an Applicable Day or for a series of Applicable Days.
    /// </summary>
    public class CciSettlementPeriods : IContainerCci, IContainerCciFactory
    {
        #region Membres
        private readonly string _prefix = string.Empty;
        private readonly CciTrade _cciTrade;
        private ISettlementPeriods _settlementPeriods;
        private readonly CCiPrevailingTime _cciStartTime;
        private readonly CCiPrevailingTime _cciEndTime;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("duration")]
            duration,
            
            unknown
        }

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        public TradeCustomCaptureInfos Ccis => _cciTrade.Ccis;
        public CCiPrevailingTime CciStartTime
        {
            get
            {
                return _cciStartTime;
            }
        }
        public CCiPrevailingTime CciEndTime
        {
            get
            {
                return _cciEndTime;
            }
        }

        

        /// <summary>
        /// 
        /// </summary>
        public ISettlementPeriods SettlementPeriods
        {
            get
            {
                return _settlementPeriods;
            }
            set
            {
                _settlementPeriods = value;
            }
        }

        #endregion

        #region constructor
        public CciSettlementPeriods(CciTrade pCCiTrade, string pPrefix, ISettlementPeriods pSettlementPeriods)
        {
            _cciTrade = pCCiTrade;
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;

            _settlementPeriods = pSettlementPeriods;

            IPrevailingTime prevailingTime = null;
            if (null != _settlementPeriods)
                prevailingTime = _settlementPeriods.StartTime.Time;
            _cciStartTime = new CCiPrevailingTime(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_startTime, prevailingTime);

            prevailingTime = null;
            if (null != _settlementPeriods)
                prevailingTime = _settlementPeriods.EndTime.Time;
            _cciEndTime = new CCiPrevailingTime(pCCiTrade, _prefix + TradeCustomCaptureInfos.CCst.Prefix_endTime, prevailingTime);

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

            CciTools.CreateInstance(this, _settlementPeriods);

            if (null != _cciStartTime)
                _cciStartTime.Initialize_FromCci();
            if (null != _cciEndTime)
                _cciEndTime.Initialize_FromCci();
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.DDL + CciClientId(CciEnum.duration), true, TypeData.TypeDataEnum.@string);

            if (null != _cciStartTime)
                _cciStartTime.AddCciSystem();
            if (null != _cciEndTime)
                _cciEndTime.AddCciSystem();
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
                        case CciEnum.duration:
                            data = ReflectionTools.ConvertEnumToString<SettlementPeriodDurationEnum>(_settlementPeriods.Duration);
                            break;

                        default:
                            isSetting = false;
                            break;
                    }

                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
                }
            }

            if (null != _cciStartTime)
                _cciStartTime.Initialize_FromDocument();
            if (null != _cciEndTime)
                _cciEndTime.Initialize_FromDocument();
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
                        case CciEnum.duration:
                            if (StrFunc.IsFilled(data))
                                _settlementPeriods.Duration = (SettlementPeriodDurationEnum)ReflectionTools.EnumParse(_settlementPeriods.Duration, data);;
                            break;

                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }

            if (null != _cciStartTime)
                _cciStartTime.Dump_ToDocument();
            if (null != _cciEndTime)
                _cciEndTime.Dump_ToDocument();
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

            if (null != _cciStartTime)
                _cciStartTime.ProcessInitialize(pCci);
            if (null != _cciEndTime)
                _cciEndTime.ProcessInitialize(pCci);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {
            if (null != _cciStartTime)
                _cciStartTime.ProcessExecute(pCci);
            if (null != _cciEndTime)
                _cciEndTime.ProcessExecute(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {
            if (null != _cciStartTime)
                _cciStartTime.ProcessExecuteAfterSynchronize(pCci);
            if (null != _cciEndTime)
                _cciEndTime.ProcessExecuteAfterSynchronize(pCci);
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
                if (null != _cciStartTime)
                    ret = _cciStartTime.IsClientId_PayerOrReceiver(pCci);
            }
            if (ret == false)
            {
                if (null != _cciEndTime)
                    ret = _cciEndTime.IsClientId_PayerOrReceiver(pCci);
            }

            return ret;
        }
        /// <summary>
        ///  Nettoyage du dataDocument 
        /// </summary>
        public void CleanUp()
        {
            if (null != _cciStartTime)
                _cciStartTime.CleanUp();

            if (null != _cciEndTime)
                _cciEndTime.CleanUp();

        }
        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {
            if (null != _cciStartTime)
                _cciStartTime.RefreshCciEnabled();

            if (null != _cciEndTime)
                _cciEndTime.RefreshCciEnabled();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (null != _cciStartTime)
                _cciStartTime.SetDisplay(pCci);

            if (null != _cciEndTime)
                _cciEndTime.SetDisplay(pCci);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Initialize_Document()
        {
            if (null != _cciStartTime)
                _cciStartTime.Initialize_Document();
            
            if (null != _cciEndTime)
                _cciEndTime.Initialize_Document();
        }
        #endregion

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
            if (null != _cciStartTime)
                _cciStartTime.ResetCciFacilityHasChanged();
            if (null != _cciEndTime)
                _cciEndTime.ResetCciFacilityHasChanged();
        }

    }
}
