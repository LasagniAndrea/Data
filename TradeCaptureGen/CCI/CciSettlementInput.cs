#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Linq;
#endregion Using Directives

namespace EFS.TradeInformation
{
    #region public class CciSettlementInput
    /// <summary>
    /// Description résumée de CciSettlementInformations.
    /// </summary>
    public class CciSettlementInput : ContainerCciBase,  IContainerCciFactory, IContainerCciSpecified
    {
        #region Members
        ISettlementInput stlInput;
        
        private readonly CciTradeBase cciTrade;
        #endregion Members

        #region CciEnum
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("settlementContext.currency")]
            settlementContext_currency,
            [System.Xml.Serialization.XmlEnumAttribute("settlementContext.cashSecurities")]
            settlementContext_cashSecurities,
            [System.Xml.Serialization.XmlEnumAttribute("settlementInputInfo.cssCriteria.cssCriteriaCss")]
            settlementInputInfo_cssCriteria_cssCriteriaCss,
            details,
            unknown
        }
        #endregion Enum

        #region constructor
        public CciSettlementInput(CciTradeBase pCciTrade, int pSettlementInfoNumber, string pPrefix, ISettlementInput pSettlementInput)
            : base(pPrefix, pSettlementInfoNumber, pCciTrade.Ccis)
        {
            cciTrade = pCciTrade;

            stlInput = pSettlementInput;
            

        }
        #endregion constructor

        #region Membres de IContainerCciSpecified
        public bool IsSpecified { get { return (Cci(CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss).IsFilled); } }
        #endregion

        #region Accessors
        public TradeCustomCaptureInfos Ccis
        {
            get { return base.CcisBase as TradeCustomCaptureInfos; }
        }
        public ISettlementInput SettlementInput
        {
            set { stlInput = value; }
            get { return stlInput; }
        }
        

        #endregion accessors

        #region Membres de IContainerCciFactory
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, stlInput);
        }
        #endregion Initialize_FromCci
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116 [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            string clientId_WithoutPrefix;
            
            clientId_WithoutPrefix = CciClientId(CciEnum.details);
            CciTools.AddCciSystem(CcisBase, Cst.BUT + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            
            clientId_WithoutPrefix = CciClientId(CciEnum.settlementContext_currency);
            CciTools.AddCciSystem(CcisBase, Cst.DDL + clientId_WithoutPrefix, false, TypeData.TypeDataEnum.@string);
            

            // Il y a création du cci settlementContext_currency ici => il faut donc instancier stlInput.settlementContext.currency 
            CciTools.CreateInstance(this, stlInput);
        }
        #endregion AddCciSystem
        #region Dump_ToDocument
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140331 [19793] Tuning => Boucle sur l'enum plutôt que les ccis
        public void Dump_ToDocument()
        {
            foreach (string clientId in CcisBase.ClientId_DumpToDocument.Where(x => IsCciOfContainer(x)))
            {
                string cliendId_Key = CciContainerKey(clientId);
                if (Enum.IsDefined(typeof(CciEnum), cliendId_Key))
                {
                    CustomCaptureInfo cci = CcisBase[clientId];
                    CciEnum cciEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), cliendId_Key);

                    #region Reset variables
                    string data = cci.NewValue;
                    bool isSetting = true;
                    CustomCaptureInfosBase.ProcessQueueEnum processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables


                    switch (cciEnum)
                    {

                        #region currency
                        case CciEnum.settlementContext_currency:
                            stlInput.SettlementContext.CurrencySpecified = StrFunc.IsFilled(data);
                            stlInput.SettlementContext.Currency = data;
                            break;
                        #endregion
                        //
                        #region cashSecurities
                        case CciEnum.settlementContext_cashSecurities:
                            stlInput.SettlementContext.CashSecuritiesSpecified = StrFunc.IsFilled(data);
                            if (stlInput.SettlementContext.CashSecuritiesSpecified)
                                stlInput.SettlementContext.CashSecurities = (CashSecuritiesEnum)Enum.Parse(typeof(CashSecuritiesEnum), data, true);
                            break;
                        #endregion
                        //
                        #region cssCriteria_cssCriteriaCss
                        case CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss:
                            SQL_Css sql_css = null;
                            bool isLoaded = false;
                            cci.ErrorMsg = string.Empty;
                            //
                            if (StrFunc.IsFilled(data))
                            {
                                //Check if actor is a valid css
                                for (int i = 0; i < 2; i++)
                                {
                                    string dataToFind = data;
                                    if (i == 1)
                                        dataToFind = data.Replace(" ", "%") + "%";
                                    sql_css = new SQL_Css(cciTrade.CSCacheOn, dataToFind, SQL_Table.ScanDataDtEnabledEnum.Yes);
                                    isLoaded = sql_css.IsLoaded && (sql_css.RowsCount == 1);
                                    //
                                    if (isLoaded)
                                        break;
                                }
                                //
                                if (isLoaded)
                                {
                                    cci.NewValue = sql_css.Identifier;
                                    cci.Sql_Table = sql_css;
                                    cci.ErrorMsg = string.Empty;
                                    //
                                    stlInput.SettlementInputInfo.CssCriteria.Css.OTCmlId = sql_css.Id;
                                    stlInput.SettlementInputInfo.CssCriteria.Css.Value = sql_css.Identifier;
                                }
                                //
                                cci.ErrorMsg = ((false == isLoaded) ? Ressource.GetString("Msg_CssNotFound") : string.Empty);
                            }
                            if (false == isLoaded)
                            {
                                cci.Sql_Table = null;
                                stlInput.SettlementInputInfo.CssCriteria.Css.OTCmlId = 0;
                                stlInput.SettlementInputInfo.CssCriteria.Css.Value = string.Empty;
                                // Use for cleanup=> si css vide => SettlementInput sera vidé
                                stlInput.SettlementInputInfo.SettlementInformation.StandardSpecified = false;
                                stlInput.SettlementInputInfo.SettlementInformation.InstructionSpecified = false;
                            }
                            //
                            stlInput.SettlementInputInfo.CssCriteria.CssSpecified = isLoaded;
                            stlInput.SettlementInputInfo.SettlementInformation.StandardSpecified = stlInput.SettlementInputInfo.CssCriteria.CssSpecified;
                            //
                            processQueue = CustomCaptureInfosBase.ProcessQueueEnum.High;
                            break;
                        #endregion
                        //
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.Finalize(cci.ClientId_WithoutPrefix, processQueue);

                    stlInput.SettlementInputInfo.CssCriteriaSpecified = stlInput.SettlementInputInfo.CssCriteria.CssSpecified || stlInput.SettlementInputInfo.CssCriteria.CssInfoSpecified;
                }
            }
        }
        #endregion  Dump_ToDocument
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        /// FI 20140331 [19793] Tuning => Boucle sur l'enum plutôt que les ccis
        public void Initialize_FromDocument()
        {
            foreach (CciEnum cciEnum in Enum.GetValues(typeof(CciEnum)))
            {
                CustomCaptureInfo cci = Cci(cciEnum);
                if (cci != null)
                {
                    #region Reset variables
                    string data = string.Empty;
                    Boolean isSetting = true;
                    SQL_Table sql_Table = null;
                    #endregion Reset variables
                                        
                    switch (cciEnum)
                    {
                        #region currency
                        case CciEnum.settlementContext_currency:
                            if (stlInput.SettlementContext.CurrencySpecified)
                                data = stlInput.SettlementContext.Currency;
                            break;
                        #endregion currency
                        #region cashSecurities
                        case CciEnum.settlementContext_cashSecurities:
                            if (stlInput.SettlementContext.CashSecuritiesSpecified)
                                data = stlInput.SettlementContext.CashSecurities.ToString();
                            break;
                        #endregion cashSecurities
                        #region css
                        case CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss:
                            if (stlInput.SettlementInputInfo.CssCriteriaSpecified && stlInput.SettlementInputInfo.CssCriteria.CssSpecified)
                            {
                                SQL_Css sql_Css = null;
                                string css = stlInput.SettlementInputInfo.CssCriteria.Css.Value;
                                int idCss = stlInput.SettlementInputInfo.CssCriteria.Css.OTCmlId;
                                //
                                if (idCss > 0)
                                    sql_Css = new SQL_Css(cciTrade.CSCacheOn, idCss);
                                else if (StrFunc.IsFilled(css))
                                    sql_Css = new SQL_Css(cciTrade.CSCacheOn, css);
                                //
                                if (sql_Css.IsLoaded)
                                {
                                    cci.Sql_Table = sql_Css;
                                    data = sql_Css.Identifier;
                                }
                            }
                            break;
                        #endregion css
                        #region default
                        default:
                            isSetting = false;
                            break;
                        #endregion
                    }
                    if (isSetting)
                        CcisBase.InitializeCci(cci, sql_Table, data);
                }
            }
        }
        #endregion
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
        /// <summary>
        /// Initialization others data following modification
        /// </summary>
        /// <param name="pProcessQueue"></param>
        /// <param name="pCci"></param>
        public void ProcessInitialize(CustomCaptureInfo pCci)
        {
            if (IsCciOfContainer(pCci.ClientId_WithoutPrefix))
            {
                string clientId_Element = CciContainerKey(pCci.ClientId_WithoutPrefix);
                //
                CciEnum elt = CciEnum.unknown;
                if (System.Enum.IsDefined(typeof(CciEnum), clientId_Element))
                    elt = (CciEnum)System.Enum.Parse(typeof(CciEnum), clientId_Element);
                //
                switch (elt)
                {
                    #region settlementInputInfo_cssCriteria_cssCriteriaCss
                    case CciEnum.settlementInputInfo_cssCriteria_cssCriteriaCss:
                        if (pCci.IsFilled)
                        {
                            SQL_Css sqlCss = (SQL_Css)pCci.Sql_Table;
                            if ((null != sqlCss) && StrFunc.IsFilled(sqlCss.IdC))
                                CcisBase.SetNewValue(CciClientId(CciEnum.settlementContext_currency), sqlCss.IdC);
                        }
                        else
                        {
                            CcisBase.SetNewValue(CciClientId(CciEnum.settlementContext_currency), string.Empty);
                        }
                        break;
                    #endregion

                    #region Default
                    default:

                        break;
                        #endregion Default
                }
            }
        }
        #endregion ProcessInitialize
        #region IsClientId_XXXXX
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion IsClientId_XXXXX
        #region CleanUP
        public void CleanUp() {
        }
        #endregion CleanUP
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
        #endregion

    }
    #endregion
}
