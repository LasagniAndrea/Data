#region Using Directives
using System;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;


using EFS.GUI.CCI;

using EfsML;


#endregion Using Directives

namespace EFS.TradeInformation
{
    #region CciEventProcess
    public class CciEventProcess : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            process,
            idStProcess,
            /// <summary>
            /// Cii chargé d'acceuillir la date UTC (Date et time dans 1 ccis)
            /// </summary>
            dtStProcess,
            dtStProcess_date,
            dtStProcess_time,
            extlLink,

            screen,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        public EventCustomCaptureInfos ccis;
        private EventProcess eventProcess;
        private readonly string prefix;
        private readonly int number;
        #endregion Members
        #region Accessors
        #region CS
        public string CS
        {
            get { return ccis.CS; }
        }
        #endregion CS

        #region EventProcess
        public EventProcess EventProcess
        {
            set
            {
                eventProcess = (EventProcess)value;
            }
            get
            {
                return eventProcess;
            }
        }
        #endregion EventProcess
        #region ExistNumber
        private bool ExistNumber
        {
            get
            {
                return (0 < number);
            }
        }
        #endregion ExistNumber
        #region Number
        private string Number
        {
            get
            {
                string ret = string.Empty;
                if (ExistNumber)
                    ret = number.ToString();
                return ret;
            }
        }
        #endregion Number
        #endregion Accessors
        #region Constructor
        public CciEventProcess(CciEvent pCciEvent, int pNumber, EventProcess pEventProcess, string pPrefix)
        {
            number = pNumber;
            prefix = pPrefix + this.Number + CustomObject.KEY_SEPARATOR;
            ccis = pCciEvent.ccis;
            eventProcess = pEventProcess;
        }
        #endregion Constructors

        #region Methods
        #region Clear
        public void Clear()
        {
            ccis.Set(CciClientId(CciEnum.dtStProcess_date), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.dtStProcess_time), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.idStProcess), "NewValue", string.Empty);
            ccis.Set(CciClientId(CciEnum.extlLink), "NewValue", string.Empty);
        }
        #endregion Clear
        #region  SetEnabled
        public void SetEnabled(Boolean pIsEnabled)
        {
            ccis.Set(CciClientId(CciEnum.dtStProcess_date), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.dtStProcess_time), "IsEnabled", pIsEnabled);
            ccis.Set(CciClientId(CciEnum.extlLink), "IsEnabled", pIsEnabled);
            Cci(CciEnum.process).IsEnabled = true;
        }
        #endregion SetEnabled
        #endregion Methods

        #region Interface Methods
        #region IContainerCciFactory members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region Initialize_FromCci
        public void Initialize_FromCci() { }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {

            string data;
            bool isSetting;
            SQL_Table sql_Table;

            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    isSetting = true;
                    sql_Table = null;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.process:
                            #region Process
                            if (eventProcess.processSpecified)
                                data = eventProcess.process;
                            break;
                        #endregion Process
                        case CciEnum.idStProcess:
                            #region IdStProcess
                            if (eventProcess.idStProcessSpecified)
                                data = eventProcess.idStProcess;
                            break;
                        #endregion IdStProcess
                        case CciEnum.dtStProcess:
                        case CciEnum.dtStProcess_date:
                        case CciEnum.dtStProcess_time:
                            #region DtStProcess
                            if (eventProcess.dtStProcessSpecified)
                                data = eventProcess.dtStProcess.Value;
                            break;
                        #endregion DtStProcess
                        case CciEnum.extlLink:
                            #region ExternalLink
                            if (eventProcess.extlLinkSpecified)
                                data = eventProcess.extlLink;
                            break;
                        #endregion ExternalLink
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.InitializeCci(cci, sql_Table, data);
                }
            }
            if (false == Cci(CciEnum.process).IsMandatory)
                SetEnabled(Cci(CciEnum.process).IsFilledValue);

        }
        #endregion Initialize_FromDocument
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;

            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = ccis[prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.process:
                            #region Process
                            if (StrFunc.IsEmpty(cci.NewValue) && (false == Cci(CciEnum.process).IsMandatory))
                                Clear();
                            else
                                eventProcess.process = data;
                            eventProcess.processSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.Low;
                            break;
                        #endregion Process
                        case CciEnum.idStProcess:
                            #region IdStProcess
                            eventProcess.idStProcess = data;
                            eventProcess.idStProcessSpecified = StrFunc.IsFilled(data);
                            break;
                        #endregion IdStProcess
                        case CciEnum.extlLink:
                            #region ExternalLink
                            eventProcess.extlLinkSpecified = StrFunc.IsFilled(data);
                            eventProcess.extlLink = data;
                            #endregion ExternalLink
                            break;
                        default:
                            isSetting = false;
                            break;
                    }
                    if (isSetting)
                        ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
            if (false == Cci(CciEnum.process).IsMandatory)
                SetEnabled(Cci(CciEnum.process).IsFilledValue);
        }
        #endregion Dump_ToDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
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

        }
        #endregion ProcessInitialize
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
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
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return prefix + pClientId_Key;
        }
        #endregion
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnum)
        {
            return Cci(pEnum.ToString());
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return ccis[CciClientId(pClientId_Key)];
        }

        #endregion
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return  (pClientId_WithoutPrefix.StartsWith(prefix));
        }
        #endregion
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(prefix.Length);
        }
        #endregion
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #endregion IContainerCci Members
        #region IContainerCciGetInfoButton Members
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion IContainerCciGetInfoButton Members
        #endregion Interface Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// FI 20200820 [25468] Add Method
        /// EG 20201014 [XXXXX] Test EventProcess.dtStProcessSpecified
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            CustomCaptureInfo cciItem = Cci(CciEnum.dtStProcess);
            if ((pPage.FindControl(cciItem.ClientId) is WCTextBox2 ctrl) && EventProcess.dtStProcessSpecified)
            {
                ctrl.Text = DtFuncExtended.DisplayTimestampAudit(new DateTimeTz(EventProcess.dtStProcess.DateTimeValue, "Etc/UTC"), new AuditTimestampInfo()
                {
                    Collaborator = SessionTools.Collaborator,
                    TimestampZone = SessionTools.AuditTimestampZone,
                    Precision = SessionTools.AuditTimestampPrecision
                });
            }
        }
    }
    #endregion CciEventProcess
}
