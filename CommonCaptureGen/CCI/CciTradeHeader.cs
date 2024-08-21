#region using directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using FpML.Interface;
using System;
using System.Linq;
#endregion using directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de TradeHeader.
    /// </summary>
    // EG 20140702 New Introduction de BusinessDate dans l'objet TradeDate (EfsML)
    // EG 20171025 [23509] Upd plus de timeStamping, clearedDate remplace businessDate
    public class CciTradeHeader : IContainerCciFactory, IContainerCci
    {
        #region Enums
        #region CciEnum
        /// EG 20140702 New businessDate
        /// EG 20171003 [23452]
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("scope")]
            scope,
            [System.Xml.Serialization.XmlEnumAttribute("tradeDate.efs_id")]
            tradeDate,
            [System.Xml.Serialization.XmlEnumAttribute("clearedDate")]
            clearedDate,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly ITradeHeader tradeHeader;
        private readonly string prefix;
        private readonly CciTradeCommonBase cciTrade;
        private readonly TradeCommonCustomCaptureInfos _ccis;
        #endregion Members
        #region Accessors
        public TradeCommonCustomCaptureInfos Ccis => _ccis;
        #endregion Accessors
        #region Constructors
        public CciTradeHeader(CciTradeCommonBase pCciTrade, ITradeHeader pTradeHeader, string pPrefix)
        {

            prefix = pPrefix + CustomObject.KEY_SEPARATOR;
            tradeHeader = pTradeHeader;
            cciTrade = pCciTrade;
            _ccis = pCciTrade.Ccis;
        }

        public CciTradeHeader(CciTradeCommonBase pTrade, ITradeHeader pTradeHeader)
            : this(pTrade, pTradeHeader, TradeCommonCustomCaptureInfos.CCst.Prefix_tradeHeader) { }
        #endregion Constructors

        #region IContainerCciFactory Members
        #region AddCciSystem
        public void AddCciSystem()
        {
        }
        #endregion AddCciSystem
        #region CleanUP
        public void CleanUp()
        {
            cciTrade.DataDocument.CleanUpTradeHeader();
            //
            //partyTradeIdentifier
            if (ArrFunc.IsFilled(tradeHeader.PartyTradeIdentifier))
            {
                for (int i = tradeHeader.PartyTradeIdentifier.Length - 1; -1 < i; i--)
                {
                    //20091113 FI appel CaptureTools.IsDocumentElementValid avec l'interface IpartyTradeIdentifier
                    //20090930 FI [16770] si IsDebtSecurity alors on ne test que la présence du partyTradeIdentifier.partyReference 
                    bool isRemove = (false == CaptureTools.IsDocumentElementValid(tradeHeader.PartyTradeIdentifier[i], Ccis.TradeCommonInput.Product.IsDebtSecurity));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(tradeHeader, "partyTradeIdentifier", i);
                }
            }
            //partyTradeInformation
            if (ArrFunc.IsFilled(tradeHeader.PartyTradeInformation))
            {
                for (int i = tradeHeader.PartyTradeInformation.Length - 1; -1 < i; i--)
                {
                    //FI 20091113 appel à CaptureTools.IsDocumentElementValid avec l'insterface IpartyTradeInformation
                    bool isRemove = (false == CaptureTools.IsDocumentElementValid(tradeHeader.PartyTradeInformation[i]));
                    if (isRemove)
                        ReflectionTools.RemoveItemInArray(tradeHeader, "partyTradeInformation", i);
                }
            }
            tradeHeader.PartyTradeInformationSpecified = ArrFunc.IsFilled(tradeHeader.PartyTradeInformation);

        }
        #endregion CleanUP
        /* FI 20200421 [XXXXX] Mise en commentaire
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        /// EG 20140702 New businessDate
        /// FI 20161214 [21916] Modify
        /// EG 20171003 [23452] Add CciEnum.tradeDateTime, Remove CciEnum.tradeDate et CciEnum.timeStamping
        // EG 20171009 [23452] Apply ClientId_DumpToDocument
        // EG 20171016 [23509] Upd clearedDate remplace businessDate
        // EG 20171025 [23509] Upd clearedDate remplace businessDate, cutoff géré sur executionDateTime
        public void Dump_ToDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                string cliendId = prefix + enumName;

                bool isOk = true;
                if (null != ccis.ClientId_DumpToDocument)
                    isOk = ccis.ClientId_DumpToDocument.Contains(cliendId);

                if (isOk)
                {
                    CustomCaptureInfo cci = ccis[cliendId];
                    //if ((cci != null) && (cci.HasChanged))
                    if (cci != null)
                    {
                        #region Reset variables
                        string data = cci.NewValue;
                        bool isSetting = true;
                        CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                        #endregion Reset variables

                        CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                        switch (keyEnum)
                        {
                            case CciEnum.tradeDate:
                                tradeHeader.tradeDate.Value = data;
                                if (StrFunc.IsEmpty(tradeHeader.tradeDate.efs_id))
                                    tradeHeader.tradeDate.efs_id = TradeCommonCustomCaptureInfos.CCst.TRADEDATE_REFERENCE;
                                // FI 20161214 [21916] pour gestion du CutOff sur commonditySpot
                                //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                break;
                            case CciEnum.clearedDate:
                                tradeHeader.clearedDate.Value = data;
                                tradeHeader.clearedDateSpecified = StrFunc.IsFilled(data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                break;
                            case CciEnum.scope:
                                #region Invoicing Scope
                                cciTrade.DumpInvoicingScope_ToDocument(cci, data);
                                processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                                #endregion Invoicing Scope
                                break;
                            default:
                                #region default
                                isSetting = false;
                                #endregion default
                                break;
                        }
                        if (isSetting)
                            ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                    }
                }
            }
        }
        #endregion  Dump_ToDocument
        */
        #region DumpSpecific_ToGUI
        public void DumpSpecific_ToGUI(CciPageBase pPage)
        {
            pPage.SetOpenFormReferential(Cci(CciEnum.scope), Cst.OTCml_TBL.INVOICINGRULES);
        }
        #endregion DumpSpecific_ToGUI
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, tradeHeader);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// EG 20140702 New businessDate
        /// EG 20171003 [23452] Add  plus de timeStamping et businessDate, add clearedDate
        public void Initialize_FromDocument()
        {
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    bool isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.tradeDate:
                            #region TradeDate
                            data = tradeHeader.TradeDate.Value;
                            #endregion TradeDate
                            break;
                        case CciEnum.clearedDate:
                            if (tradeHeader.ClearedDateSpecified)
                                data = tradeHeader.ClearedDate.Value;
                            break;
                        case CciEnum.scope:
                            #region InvoicingScope
                            if (null != cciTrade.Invoice)
                            {
                                SQL_InvoicingRules sql_InvoicingRules = new SQL_InvoicingRules(cciTrade.CSCacheOn, cciTrade.Invoice.Scope.OTCmlId);
                                if (sql_InvoicingRules.IsLoaded)
                                {
                                    data = sql_InvoicingRules.Identifier;
                                    sql_Table = (SQL_Table)sql_InvoicingRules;
                                }
                            }
                            #endregion InvoicingScope
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.InitializeCci(cci, sql_Table, data);
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
        // EG 20091208 New
        public void ProcessExecuteAfterSynchronize(CustomCaptureInfo pCci)
        {

        }
        #endregion ProcessExecuteAfterSynchronize

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
        #region SetDisplay
        public void SetDisplay(CustomCaptureInfo pCci)
        {
            if (IsCci(CciEnum.scope, pCci) && (null != pCci.Sql_Table))
            {
                SQL_InvoicingRules sql_InvoicingRules = (SQL_InvoicingRules)pCci.Sql_Table;
                pCci.Display = pCci.Sql_Table.FirstRow["DISPLAYNAME"].ToString();
                if (0 < sql_InvoicingRules.IdATrader)
                {
                    pCci.Display += " [" + Ressource.GetString("tradeHeader_party_trader_identifier");
                    pCci.Display += ":" + sql_InvoicingRules.TraderIdentifier + "]";
                }
            }

        }
        #endregion SetDisplay

        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        /// EG 20140702 New businessDate
        /// FI 20161214 [21916] Modify
        /// EG 20171003 [23452] Add CciEnum.tradeDateTime, Remove CciEnum.tradeDate et CciEnum.timeStamping
        /// EG 20171009 [23452] Apply ClientId_DumpToDocument
        /// EG 20171016 [23509] Upd clearedDate remplace businessDate
        /// EG 20171025 [23509] Upd clearedDate remplace businessDate, cutoff géré sur executionDateTime
        /// FI 20200421 [XXXXX] Usage de ccis.ClientId_DumpToDocument
        public void Dump_ToDocument()
        {
            foreach (string clientId in Ccis.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = Ccis[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    switch (cciEnum)
                    {
                        case CciEnum.tradeDate:
                            tradeHeader.TradeDate.Value = data;
                            if (StrFunc.IsEmpty(tradeHeader.TradeDate.Efs_id))
                                tradeHeader.TradeDate.Efs_id = TradeCommonCustomCaptureInfos.CCst.TRADEDATE_REFERENCE;
                            // FI 20161214 [21916] pour gestion du CutOff sur commonditySpot
                            //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.clearedDate:
                            tradeHeader.ClearedDate.Value = data;
                            tradeHeader.ClearedDateSpecified = StrFunc.IsFilled(data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        case CciEnum.scope:
                            #region Invoicing Scope
                            cciTrade.DumpInvoicingScope_ToDocument(cci, data);
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            #endregion Invoicing Scope
                            break;
                        default:
                            #region default
                            isSetting = false;
                            #endregion default
                            break;
                    }
                    if (isSetting)
                        Ccis.Finalize(cci.ClientId_WithoutPrefix, processQueue);
                }
            }
        }
        #endregion  Dump_ToDocument
        #endregion IContainerCciFactory Members
        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
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
            return prefix + pClientId_Key;
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
        #region IsCciClientId
        public bool IsCciClientId(CciEnum pEnumValue, string pClientId_WithoutPrefix)
        {
            return (CciClientId(pEnumValue) == pClientId_WithoutPrefix);
        }
        #endregion IsCciClientId
        #region IsCciOfContainer
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return (pClientId_WithoutPrefix.StartsWith(prefix));
        }
        #endregion
        #endregion IContainerCci
    }
}
