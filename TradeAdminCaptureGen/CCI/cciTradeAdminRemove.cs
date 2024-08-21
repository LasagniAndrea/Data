#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.TradeLink;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    public class CciTradeAdminRemove : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            identifier,
            displayName,
            description,
            date,
            comments,
            warning,
            lstInvoice,
            lstCreditNote,
            lstAdditionalInvoice,
            lstInvoiceSettlement,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly  string _prefix;
        /// EG 20150513 [20513] Type RemoveTradeMsg remplace RemoveTrade
        private readonly RemoveTradeMsg _removeTrade;
        private readonly LinkedTradeAdminRemove _linkedTradeAdminRemove;
        
        private readonly TradeCommonCustomCaptureInfos _ccis;

        //private StringDictionary _linkedTradeIdentifier;
        //private StringDictionary _linkedTradeId;
        #endregion Members

        #region Accessors
        #region ccis
        public TradeCommonCustomCaptureInfos Ccis => _ccis;
        #endregion ccis
        //#region LinkedTradeId
        //public StringDictionary LinkedTradeId
        //{
        //    get { return _linkedTradeId; }
        //}
        //#endregion LinkedTradeId
        #region Note
        public string Note
        {
            get 
            { 
                if (null != _removeTrade)
                    return _removeTrade.note;
                return string.Empty;
            }
        }
        #endregion Note

        #endregion Accessors

        #region Constructors
        /// EG 20150513 [20513] Type RemoveTradeMsg remplace RemoveTrade
        public CciTradeAdminRemove(CciTradeCommonBase pTrade, RemoveTradeMsg pRemoveTrade, LinkedTradeAdminRemove pLinkTradeAdminRemove, string pPrefix)
        {
            _prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            _ccis = pTrade.Ccis;
            _removeTrade = pRemoveTrade;
            _linkedTradeAdminRemove  = pLinkTradeAdminRemove;
        }
        #endregion Constructors
        #region Members
        private string SetLinkedTradeToCci(CciEnum pCciEnum)
        {
            string ret = "-";
            TradeLinkDataIdentification key = TradeLinkDataIdentification.NA;
            if (pCciEnum == CciEnum.lstInvoice)
                key = TradeLinkDataIdentification.InvoiceIdentifier;
            else if (pCciEnum == CciEnum.lstAdditionalInvoice)
                key = TradeLinkDataIdentification.AddInvoiceIdentifier;
            else if (pCciEnum == CciEnum.lstCreditNote)
                key = TradeLinkDataIdentification.CreditNoteIdentifier;
            else if (pCciEnum == CciEnum.lstInvoiceSettlement)
                key = TradeLinkDataIdentification.InvoiceSettlementIdentifier;

            if ((key != TradeLinkDataIdentification.NA) &&  _linkedTradeAdminRemove.LinkedTradeIdentifier.ContainsKey(key.ToString()))
                ret = _linkedTradeAdminRemove.LinkedTradeIdentifier[key.ToString()];

            return ret;
        }
        #endregion Members
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Dump_ToDocument
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
        #endregion  Dump_ToDocument
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, _removeTrade);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        // EG 20140826 Gestion des commentaires sur annulation
        public void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;

            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = _ccis[_prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    //display = string.Empty;
                    isSetting = true;
                    sql_Table = null;
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
                        case CciEnum.comments:
                            data = _removeTrade.note;
                            break;
                        case CciEnum.warning:
                            data = Ressource.GetString("TradeAdminRemoveWarning_" + _ccis.TradeCommonInput.SQLProduct.Identifier);
                            break;
                        case CciEnum.lstInvoice:
                        case CciEnum.lstAdditionalInvoice:
                        case CciEnum.lstCreditNote:
                        case CciEnum.lstInvoiceSettlement:
                            data = SetLinkedTradeToCci(keyEnum);
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
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_PayerOrReceiver
        #region ProcessInitialize
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessInitialize
        #region ProcessExecute
        public void ProcessExecute(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecute
        #region ProcessExecuteAfterSynchronize
        // EG 20091208
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
        }
        #endregion RefreshCciEnabled
        #region RemoveLastItemInArray
        public void RemoveLastItemInArray(string _)
        {
        }
        #endregion RemoveLastItemInArray
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return _ccis[CciClientId(pEnumValue.ToString())];
        }

        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return _ccis[CciClientId(pClientId_Key)];
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
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(_prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(_prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #endregion Interfaces
    }
}
