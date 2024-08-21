#region Using Directives
using System;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.CCI;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciTradeRemove.
    /// </summary>
    /// EG 20150515 _removeTrade de type RemoveTradeMsg
    public class CciTradeRemove : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            identifier,
            displayName,
            description,
            date,
            removeFutureEvent,
            comments,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums

        #region Members
        private readonly string _prefix;
        private readonly RemoveTradeMsg _removeTrade;
        private readonly TradeCommonCustomCaptureInfos _ccis;
        #endregion Members

        #region accessors
        public TradeCommonCustomCaptureInfos Ccis => _ccis;
        #endregion

        #region Constructors
        /// EG 20150515 _removeTrade de type RemoveTradeMsg
        public CciTradeRemove(CciTradeCommonBase pTrade, RemoveTradeMsg pRemoveTrade, string pPrefix)
        {
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _ccis = pTrade.Ccis;
            _removeTrade = pRemoveTrade;
        }
        #endregion Constructors

        #region Methodes
        /// <summary>
        /// 
        /// </summary>
        public void AddCciSystem()
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

            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = _ccis[_prefix + enumName];
                if ((cci != null) && (cci.HasChanged))
                {
                    #region Reset variables
                    data = cci.NewValue;
                    isSetting = true;
                    processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables
                    //
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.date:
                            #region RemoveDate
                            _removeTrade.actionDate = new DtFunc().StringDateISOToDateTime(data);
                            break;
                            #endregion RemoveDate
                        case CciEnum.comments:
                            #region Comments
                            _removeTrade.note = data;
                            _removeTrade.noteSpecified = StrFunc.IsFilled(data);
                            break;
                            #endregion Comments
                        case CciEnum.removeFutureEvent:
                            #region Remove Future Events
                            _removeTrade.removeFutureEvent = Convert.ToBoolean(data);
                            break;
                            #endregion Remove Future Events
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
            CciTools.CreateInstance(this, _removeTrade);
        }

        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
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
                    //display = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                    //
                    //20091016 FI [Rebuild identification] use TradeCommonInput.identification
                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.identifier:
                            data = _ccis.TradeCommonInput.Identification.Identifier;
                            break;
                        case CciEnum.displayName:
                            data = _ccis.TradeCommonInput.Identification.Displayname;
                            break;
                        case CciEnum.description:
                            data = _ccis.TradeCommonInput.Identification.Description;
                            break;
                        case CciEnum.date:
                            data = DtFunc.DateTimeToString(_removeTrade.actionDate, DtFunc.FmtISODate);
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
        /// <returns></returns>
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        public void RefreshCciEnabled()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPrefix"></param>
        public void RemoveLastItemInArray(string _)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCci"></param>
        public void SetDisplay(CustomCaptureInfo pCci)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue.ToString())];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_Key"></param>
        /// <returns></returns>
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <returns></returns>
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_Key"></param>
        /// <returns></returns>
        public string CciClientId(string pClientId_Key)
        {
            return _prefix + pClientId_Key;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <param name="pCci"></param>
        /// <returns></returns>
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEnumValue"></param>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClientId_WithoutPrefix"></param>
        /// <returns></returns>
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(_prefix));
        }
        #endregion Methodes
    }
}
