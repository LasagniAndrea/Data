#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Interface;
using System;

#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoice.
    /// </summary>
    public class CciInvoiceSupplement : CciInvoice, IContainerCci, IContainerCciQuoteBasis
    {
        #region Enums
        #region CciEnum
        public new enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.identifier")]
            correction_trade_identifier,
            correction_trade_displayName,
            correction_trade_otcmlId,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.grossTurnOverAmount.amount")]
            correction_grossTurnOverAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.grossTurnOverAmount.currency")]
            correction_grossTurnOverAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.rebateAmount.amount")]
            correction_totalRebateAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.rebateAmount.currency")]
            correction_totalRebateAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverAmount.amount")]
            correction_netTurnOverAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverAmount.currency")]
            correction_netTurnOverAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverIssueAmount.amount")]
            correction_netTurnOverIssueAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverIssueAmount.currency")]
            correction_netTurnOverIssueAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverAccountingAmount.amount")]
            correction_netTurnOverAccountingAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.netTurnOverAccountingAmount.currency")]
            correction_netTurnOverAccountingAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.amount.amount")]
            correction_netTurnOverAmount_base_amount,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.amount.currency")]
            correction_netTurnOverAmount_base_currency,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.issueAmount.amount")]
            correction_netTurnOverIssueAmount_base_amount,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.issueAmount.currency")]
            correction_netTurnOverIssueAmount_base_currency,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.accountingAmount.amount")]
            correction_netTurnOverAccountingAmount_base_amount,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.accountingAmount.currency")]
            correction_netTurnOverAccountingAmount_base_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.grossTurnOverAmount.amount")]
            correction_grossTurnOverAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.grossTurnOverAmount.currency")]
            correction_grossTurnOverAmount_theoric_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.rebateAmount.amount")]
            correction_totalRebateAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.rebateAmount.currency")]
            correction_totalRebateAmount_theoric_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverAmount.amount")]
            correction_netTurnOverAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverAmount.currency")]
            correction_netTurnOverAmount_theoric_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverIssueAmount.amount")]
            correction_netTurnOverIssueAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverIssueAmount.currency")]
            correction_netTurnOverIssueAmount_theoric_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverAccountingAmount.amount")]
            correction_netTurnOverAccountingAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.netTurnOverAccountingAmount.currency")]
            correction_netTurnOverAccountingAmount_theoric_currency,

            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.tax.amount")]
            correction_taxAmount_initial_amount,
            [System.Xml.Serialization.XmlEnumAttribute("initialInvoiceAmount.tax.currency")]
            correction_taxAmount_initial_currency,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.tax.amount")]
            correction_taxAmount_base_amount,
            [System.Xml.Serialization.XmlEnumAttribute("baseNetInvoiceAmount.tax.currency")]
            correction_taxAmount_base_currency,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.tax.amount")]
            correction_taxAmount_theoric_amount,
            [System.Xml.Serialization.XmlEnumAttribute("theoricInvoiceAmount.tax.currency")]
            correction_taxAmount_theoric_currency,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly IInvoiceSupplement m_InvoiceSupplement;
        private readonly string m_Prefix;
        private SQL_TradeCommon m_SQLTrade;
        #endregion Members
        #region Accessors
        #region ExistSQLTrade
        public bool ExistSQLTrade
        {
            get { return ((null != m_SQLTrade) && m_SQLTrade.IsLoaded); }
        }
        #endregion ExistSQLTrade
        #endregion Accessors
        #region Constructors
        // EG 20180205 [23769] Del tradeLibrary 
        public CciInvoiceSupplement(TradeAdminCustomCaptureInfos pCcis)
            : base(pCcis)
        {
            m_InvoiceSupplement = (IInvoiceSupplement)CurrentTrade.Product;
            m_Prefix = TradeAdminCustomCaptureInfos.CCst.Prefix_Invoice + CustomObject.KEY_SEPARATOR;
            //new EFS_TradeLibrary(TradeCommonInput.FpMLDataDocReader);
            InitializeSQLTable();
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        public override void AddCciSystem()
        {
            base.AddCciSystem();
        }
        #endregion AddCciSystem
        #region Dump_ToDocument
        public override void Dump_ToDocument()
        {
            base.Dump_ToDocument();
        }
        #endregion Dump_ToDocument
        #region Initialize_FromCci
        public override void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_InvoiceSupplement);
            base.Initialize_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public override void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);

            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
                if (cci != null)
                {
                    #region Reset variables
                    data = string.Empty;
                    //display = string.Empty;
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    //processQueue = CustomCaptureInfosBase.ProcessQueueEnum.None;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.correction_grossTurnOverAmount_initial_amount:
                            #region Initial GrossTurnOverAmount (Amount)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.GrossTurnOverAmount.Amount.Value;
                            #endregion Initial GrossTurnOverAmount (Amount)
                            break;
                        case CciEnum.correction_grossTurnOverAmount_initial_currency:
                            #region Initial GrossTurnOverAmount (Currency)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.GrossTurnOverAmount.Currency;
                            #endregion Initial GrossTurnOverAmount (Currency)
                            break;
                        case CciEnum.correction_totalRebateAmount_initial_amount:
                            #region Initial TotalRebateAmount (Amount)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.RebateAmountSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.RebateAmount.Amount.Value;
                            #endregion Initial TotalRebateAmount (Amount)
                            break;
                        case CciEnum.correction_totalRebateAmount_initial_currency:
                            #region Initial TotalRebateAmount (Currency)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.RebateAmountSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.RebateAmount.Currency;
                            #endregion Initial TotalRebateAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAmount_initial_amount:
                            #region Initial NetTurnOverAmount (Amount)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAmount.Amount.Value;
                            #endregion Initial NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAmount_initial_currency:
                            #region Initial NetTurnOverAmount (Currency)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAmount.Currency;
                            #endregion Initial NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_initial_amount:
                            #region Initial NetTurnOverIssueAmount (Amount)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverIssueAmount.Amount.Value;
                            #endregion Initial NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_initial_currency:
                            #region Initial NetTurnOverIssueAmount (Currency)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverIssueAmount.Currency;
                            #endregion Initial NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_initial_amount:
                            #region Initial NetTurnOverIssueAmount (Amount)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAccountingAmountSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAccountingAmount.Amount.Value;
                            #endregion Initial NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_initial_currency:
                            #region Initial NetTurnOverIssueAmount (Currency)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAccountingAmountSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.NetTurnOverAccountingAmount.Currency;
                            #endregion Initial NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAmount_base_amount:
                            #region Base NetTurnOverAmount (Amount)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.Amount.Amount.Value;
                            #endregion Base NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAmount_base_currency:
                            #region Base NetTurnOverAmount (Currency)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.Amount.Currency;
                            #endregion Base NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_base_amount:
                            #region Base NetTurnOverIssueAmount (Amount)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.IssueAmount.Amount.Value;
                            #endregion Base NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_base_currency:
                            #region Base NetTurnOverIssueAmount (Currency)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.IssueAmount.Currency;
                            #endregion Base NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_base_amount:
                            #region Base NetTurnOverIssueAmount (Amount)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.AccountingAmount.Amount.Value;
                            #endregion Base NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_base_currency:
                            #region Base NetTurnOverIssueAmount (Currency)
                            data = m_InvoiceSupplement.BaseNetInvoiceAmount.AccountingAmount.Currency;
                            #endregion Base NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.correction_grossTurnOverAmount_theoric_amount:
                            #region Final GrossTurnOverAmount (Amount)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.GrossTurnOverAmount.Amount.Value;
                            #endregion Final GrossTurnOverAmount (Amount)
                            break;
                        case CciEnum.correction_grossTurnOverAmount_theoric_currency:
                            #region Final GrossTurnOverAmount (Currency)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.GrossTurnOverAmount.Currency;
                            #endregion Final GrossTurnOverAmount (Currency)
                            break;
                        case CciEnum.correction_totalRebateAmount_theoric_amount:
                            #region Final TotalRebateAmount (Amount)
                            if (m_InvoiceSupplement.TheoricInvoiceAmount.RebateAmountSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.RebateAmount.Amount.Value;
                            #endregion Final TotalRebateAmount (Amount)
                            break;
                        case CciEnum.correction_totalRebateAmount_theoric_currency:
                            #region Final TotalRebateAmount (Currency)
                            if (m_InvoiceSupplement.TheoricInvoiceAmount.RebateAmountSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.RebateAmount.Currency;
                            #endregion Final TotalRebateAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAmount_theoric_amount:
                            #region Final NetTurnOverAmount (Amount)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAmount.Amount.Value;
                            #endregion Final NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAmount_theoric_currency:
                            #region Final NetTurnOverAmount (Currency)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAmount.Currency;
                            #endregion Final NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_theoric_amount:
                            #region Final NetTurnOverIssueAmount (Amount)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverIssueAmount.Amount.Value;
                            #endregion Final NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverIssueAmount_theoric_currency:
                            #region Final NetTurnOverIssueAmount (Currency)
                            data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverIssueAmount.Currency;
                            #endregion Final NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_theoric_amount:
                            #region Final NetTurnOverIssueAmount (Amount)
                            if (m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAccountingAmountSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAccountingAmount.Amount.Value;
                            #endregion Final NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.correction_netTurnOverAccountingAmount_theoric_currency:
                            #region Final NetTurnOverIssueAmount (Currency)
                            if (m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAccountingAmountSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.NetTurnOverAccountingAmount.Currency;
                            #endregion Final NetTurnOverIssueAmount (Currency)
                            break;

                        case CciEnum.correction_taxAmount_initial_amount:
                            #region Initial TaxAmount (Amount)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.Tax.Amount.Amount.Value;
                            #endregion Initial TaxAmount (Amount)
                            break;
                        case CciEnum.correction_taxAmount_initial_currency:
                            #region Initial TaxAmount (Currency)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.InitialInvoiceAmount.Tax.Amount.Currency;
                            #endregion Initial TaxAmount (Currency)
                            break;
                        case CciEnum.correction_taxAmount_base_amount:
                            #region Base TaxAmount (Amount)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.BaseNetInvoiceAmount.Tax.Amount.Amount.Value;
                            #endregion Base TaxAmount (Amount)
                            break;
                        case CciEnum.correction_taxAmount_base_currency:
                            #region Base TaxAmount (Currency)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.BaseNetInvoiceAmount.Tax.Amount.Currency;
                            #endregion Base TaxAmount (Currency)
                            break;
                        case CciEnum.correction_taxAmount_theoric_amount:
                            #region Theoric TaxAmount (Amount)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.Tax.Amount.Amount.Value;
                            #endregion Theoric TaxAmount (Amount)
                            break;
                        case CciEnum.correction_taxAmount_theoric_currency:
                            #region Theoric TaxAmount (Currency)
                            if (m_InvoiceSupplement.InitialInvoiceAmount.TaxSpecified)
                                data = m_InvoiceSupplement.TheoricInvoiceAmount.Tax.Amount.Currency;
                            #endregion Theoric TaxAmount (Currency)
                            break;
                        case CciEnum.correction_trade_identifier:
                            #region Invoice Source (Identifier)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.Identifier.Value;
                            #endregion Invoice Source (Identifier)
                            break;
                        case CciEnum.correction_trade_displayName:
                            #region Trade (DisplayName)
                            if (ExistSQLTrade)
                                data = m_SQLTrade.DisplayName;
                            #endregion Trade (DisplayName)
                            break;
                        case CciEnum.correction_trade_otcmlId:
                            #region Invoice Source (Id)
                            data = m_InvoiceSupplement.InitialInvoiceAmount.OtcmlId;
                            #endregion Invoice Source (Id)
                            break;
                        default:
                            #region Default
                            isSetting = false;
                            #endregion Default
                            break;
                    }
                    if (isSetting)
                    {
                        Ccis.InitializeCci(cci, sql_Table, data);
                        if (isToValidate)
                            cci.LastValue = ".";
                    }

                }
            }
            base.Initialize_FromDocument();
        }
        #endregion Initialize_FromDocument
        #region ProcessInitialize
        public override void ProcessInitialize(CustomCaptureInfo pCci)
        {
            base.ProcessInitialize(pCci);
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public override void RefreshCciEnabled()
        {
            base.RefreshCciEnabled();
        }
        #endregion RefreshCciEnabled
        #region SetDisplay
        public override void SetDisplay(CustomCaptureInfo pCci)
        {
            base.SetDisplay(pCci);
        }
        #endregion SetDisplay
        #endregion IContainerCciFactory Members
        #region ITradeGetInfoButton Members
        #region IsButtonMenu
        public override bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            bool isButtonMenu = this.IsCci(CciInvoiceSupplement.CciEnum.correction_trade_otcmlId, pCci);
            if (isButtonMenu && (null != m_SQLTrade))
            {
                #region TradeAdmin
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin);
                pCo.PK = "IDT";
                pCo.PKV = m_SQLTrade.IdT.ToString();
                #endregion TradeAdmin
            }
            if (false == isButtonMenu)
                isButtonMenu = base.IsButtonMenu(pCci, ref pCo);
            return isButtonMenu;
        }
        #endregion IsButtonMenu
        #region SetButtonReferential
        public override void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
            base.SetButtonReferential(pCci, pCo);
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public override bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public override bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #endregion ITradeGetInfoButton Members

        #region IContainerCci Members
        #region Cci
        public CustomCaptureInfo Cci(CciEnum pEnumValue)
        {
            return Ccis[CciClientId(pEnumValue.ToString())];
        }
        public new CustomCaptureInfo Cci(string pClientId_Key)
        {
            return Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public new string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public new string CciContainerKey(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.Substring(m_Prefix.Length);
        }
        #endregion CciContainerKey
        #region IsCci
        public bool IsCci(CciEnum pEnumValue, CustomCaptureInfo pCci)
        {
            return (this.CciClientId(pEnumValue) == pCci.ClientId_WithoutPrefix);
        }
        #endregion IsCci
        #region IsCciOfContainer
        public new bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            bool isOk = Ccis.Contains(pClientId_WithoutPrefix);
            isOk = isOk && (pClientId_WithoutPrefix.StartsWith(m_Prefix));
            return isOk;
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #endregion Interfaces
        #region Methods
        #region GetArrayElementDocumentCount
        public override int GetArrayElementDocumentCount(string pPrefix, string pParentClientId, int pParentOccurs)
        {
            return base.GetArrayElementDocumentCount(pPrefix, pParentClientId, pParentOccurs);
        }
        #endregion GetArrayElementDocumentCount
        #region InitializeSQLTable
        private void InitializeSQLTable()
        {
            if ((null != m_InvoiceSupplement) && (null != m_InvoiceSupplement.InitialInvoiceAmount))
            {
                if (0 < m_InvoiceSupplement.InitialInvoiceAmount.OTCmlId)
                    m_SQLTrade = new SQL_TradeCommon(CS, m_InvoiceSupplement.InitialInvoiceAmount.OTCmlId);
            }
        }
        #endregion InitializeSQLTable
        #endregion Methods
    }
}
