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
    /// Description résumée de CciAvailableInvoice.
    /// </summary>
    public class CciAvailableInvoice : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {

        #region Enums
        #region CciEnum
        public enum CciEnum
        {
            selected,
            date,
            identifier,
            otcmlId,
            amount_amount,
            amount_currency,
            issueAmount_amount,
            issueAmount_currency,
            accountingAmount_amount,
            accountingAmount_currency,
            allocatedAmount_amount,
            allocatedAmount_currency,
            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly CciInvoiceSettlement m_CciInvoiceSettlement;
        private IAvailableInvoice m_AvailableInvoice;
        private readonly string m_Prefix;
        private readonly int m_Number;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private SQL_TradeCommon m_SQLTrade;
        #endregion Members
        #region Accessors
        #region AvailableAmounts
        public INetInvoiceAmounts AvailableAmounts
        {
            get
            {
                if (null != m_AvailableInvoice)
                    return m_AvailableInvoice.AvailableAmounts;
                return null;
            }
        }
        #endregion AvailableAmounts
        #region AvailableInvoice
        public IAvailableInvoice AvailableInvoice
        {
            get { return m_AvailableInvoice; }
            set
            {
                m_AvailableInvoice = value;
                InitializeSQLTable();
            }
        }
        #endregion AvailableInvoice
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region ExistSQLTrade
        public bool ExistSQLTrade
        {
            get { return ((null != m_SQLTrade) && m_SQLTrade.IsLoaded); }
        }
        #endregion ExistSQLTrade
        #region IsModeConsult
        public bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(Ccis.CaptureMode); }
        }
        #endregion IsModeConsult
        #region Number
        private string NumberPrefix
        {
            get
            {
                string ret = string.Empty;
                if (0 < m_Number)
                    ret = m_Number.ToString();
                return ret;
            }
        }

        #endregion Number
        #endregion Accessors
        #region Constructors
        public CciAvailableInvoice(CciInvoiceSettlement pCciInvoiceSettlement, string pPrefix, int pAvailableInvoiceNumber, IAvailableInvoice pAvailableInvoice)
        {
            m_CciInvoiceSettlement = pCciInvoiceSettlement;
            m_Ccis = pCciInvoiceSettlement.Ccis;
            m_Number = pAvailableInvoiceNumber;
            m_Prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            m_AvailableInvoice = pAvailableInvoice;
            InitializeSQLTable();
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// 
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.otcmlId), false, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.CHK + CciClientId(CciEnum.selected), true, TypeData.TypeDataEnum.@bool);
        }
        #endregion AddCciSystem
        #region CleanUp
        public void CleanUp()
        {
        }
        #endregion CleanUp
        #region Initialize_Document
        public void Initialize_Document()
        {
        }
        #endregion Initialize_Document
        #region Initialize_FromCci
        public void Initialize_FromCci()
        {
            CciTools.CreateInstance(this, m_AvailableInvoice);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        public void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            //CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            //
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
                        case CciEnum.otcmlId:
                            #region Invoice (Id)
                            if (StrFunc.IsFilled(m_AvailableInvoice.OtcmlId))
                                data = m_AvailableInvoice.OTCmlId.ToString();
                            #endregion Invoice (Id)
                            break;
                        case CciEnum.identifier:
                            #region Invoice (Identifier)
                            if (ExistSQLTrade)
                                data = m_SQLTrade.Identifier;
                            #endregion Invoice (Identifier)
                            break;
                        case CciEnum.date:
                            #region InvoiceDate
                            if (null != m_AvailableInvoice.InvoiceDate)
                                data = m_AvailableInvoice.InvoiceDate.Value;
                            #endregion InvoiceDate
                            break;
                        case CciEnum.amount_amount:
                            #region Available NetTurnOverAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.Amount.Amount.Value;
                            #endregion Available NetTurnOverAmount (Amount)
                            break;
                        case CciEnum.amount_currency:
                            #region Available NetTurnOverAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.Amount.Currency;
                            #endregion Available NetTurnOverAmount (Currency)
                            break;
                        case CciEnum.issueAmount_amount:
                            #region Available NetTurnOverIssueAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.IssueAmount.Amount.Value;
                            #endregion Available NetTurnOverIssueAmount (Amount)
                            break;
                        case CciEnum.issueAmount_currency:
                            #region Available NetTurnOverIssueAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.IssueAmount.Currency;
                            #endregion Available NetTurnOverIssueAmount (Currency)
                            break;
                        case CciEnum.accountingAmount_amount:
                            #region Available NetTurnOverAccountingAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.AccountingAmount.Amount.Value;
                            #endregion Available NetTurnOverAccountingAmount (Amount)
                            break;
                        case CciEnum.accountingAmount_currency:
                            #region Available NetTurnOverAccountingAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                data = m_AvailableInvoice.AvailableAmounts.AccountingAmount.Currency;
                            #endregion Available NetTurnOverAccountingAmount (Currency)
                            break;
                        case CciEnum.allocatedAmount_amount:
                            #region AllocatedAmount (Amount)
                            if (null != m_AvailableInvoice.AllocatedAccountingAmount)
                                data = m_AvailableInvoice.AllocatedAccountingAmount.Amount.Value;
                            #endregion AllocatedAmount (Amount)
                            break;
                        case CciEnum.allocatedAmount_currency:
                            #region AllocatedAmount (Currency)
                            if (null != m_AvailableInvoice.AllocatedAccountingAmount)
                                data = m_AvailableInvoice.AllocatedAccountingAmount.Currency;
                            #endregion AllocatedAmount (Currency)
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
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion
        #region Dump_ToDocument
        public void Dump_ToDocument()
        {
            bool isSetting;
            string data;
            CustomCaptureInfosBase.ProcessQueueEnum processQueue;
            Type tCciEnum = typeof(CciEnum);
            foreach (string enumName in Enum.GetNames(tCciEnum))
            {
                CustomCaptureInfo cci = Ccis[m_Prefix + enumName];
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
                            #region InvoiceDate
                            m_AvailableInvoice.InvoiceDate.Value = data;
                            #endregion InvoiceDate
                            break;
                        case CciEnum.identifier:
                            #region Identifier
                            m_AvailableInvoice.Identifier.Value = data;
                            #endregion Identifier
                            break;
                        case CciEnum.otcmlId:
                            #region Id
                            m_AvailableInvoice.OtcmlId = data;
                            #endregion Id
                            break;
                        case CciEnum.amount_amount:
                            #region AvailableAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                m_AvailableInvoice.AvailableAmounts.Amount.Amount.Value = data;
                            #endregion AvailableAmount (Amount)
                            break;
                        case CciEnum.amount_currency:
                            #region AvailableAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                m_AvailableInvoice.AvailableAmounts.Amount.Currency = data;
                            #endregion AvailableAmount (Currency)
                            break;
                        case CciEnum.issueAmount_amount:
                            #region AvailableIssueAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                            {
                                m_AvailableInvoice.AvailableAmounts.IssueAmountSpecified = true;
                                m_AvailableInvoice.AvailableAmounts.IssueAmount.Amount.Value = data;
                            }
                            #endregion AvailableIssueAmount (Amount)
                            break;
                        case CciEnum.issueAmount_currency:
                            #region AvailableIssueAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                m_AvailableInvoice.AvailableAmounts.IssueAmount.Currency = data;
                            #endregion AvailableIssueAmount (Currency)
                            break;
                        case CciEnum.accountingAmount_amount:
                            #region AvailableAccountingAmount (Amount)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                            {
                                m_AvailableInvoice.AvailableAmounts.AccountingAmountSpecified = cci.IsFilledValue;
                                m_AvailableInvoice.AvailableAmounts.AccountingAmount.Amount.Value = data;
                            }
                            #endregion AvailableAccountingAmount (Amount)
                            break;
                        case CciEnum.accountingAmount_currency:
                            #region AvailableAccountingAmount (Currency)
                            if (null != m_AvailableInvoice.AvailableAmounts)
                                m_AvailableInvoice.AvailableAmounts.AccountingAmount.Currency = data;
                            #endregion AvailableAccountingAmount (Currency)
                            break;
                        case CciEnum.allocatedAmount_amount:
                            #region AllocatedAmount (Amount)
                            m_AvailableInvoice.AllocatedAccountingAmountSpecified = cci.IsFilledValue;
                            if (m_AvailableInvoice.AllocatedAccountingAmountSpecified)
                                m_AvailableInvoice.AllocatedAccountingAmount.Amount.Value = data;
                            #endregion AllocatedAmount (Amount)
                            break;
                        case CciEnum.allocatedAmount_currency:
                            #region AllocatedAmount (Currency)
                            if (m_AvailableInvoice.AllocatedAccountingAmountSpecified)
                                m_AvailableInvoice.AllocatedAccountingAmount.Currency = data;
                            #endregion AllocatedAmount (Currency)
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
        #endregion Dump_ToDocument
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
        }
        #endregion ProcessInitialize
        #region RefreshCciEnabled
        public void RefreshCciEnabled()
        {
            bool isEnabled = (0 < AvailableInvoice.OTCmlId);
            isEnabled &= (0 < m_CciInvoiceSettlement.InvoiceSettlement.UnallocatedAmount.Amount.DecValue);
            Ccis.Set(CciClientId(CciEnum.selected), "IsEnabled", isEnabled);
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
            return m_Ccis[CciClientId(pEnumValue.ToString())];
        }
        public CustomCaptureInfo Cci(string pClientId_Key)
        {
            return m_Ccis[CciClientId(pClientId_Key)];
        }
        #endregion Cci
        #region CciClientId
        public string CciClientId(CciEnum pEnumValue)
        {
            return CciClientId(pEnumValue.ToString());
        }
        public string CciClientId(string pClientId_Key)
        {
            return m_Prefix + pClientId_Key;
        }
        #endregion CciClientId
        #region CciContainerKey
        public string CciContainerKey(string pClientId_WithoutPrefix)
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
        public bool IsCciOfContainer(string pClientId_WithoutPrefix)
        {
            return pClientId_WithoutPrefix.StartsWith(m_Prefix);
        }
        #endregion IsCciOfContainer
        #endregion IContainerCci Members
        #region ITradeGetInfoButton Members
        #region SetButtonReferential
        public void SetButtonReferential(CustomCaptureInfo pCci, CustomObjectButtonReferential pCo)
        {
        }
        #endregion SetButtonReferential
        #region SetButtonScreenBox
        public bool SetButtonScreenBox(CustomCaptureInfo pCci, CustomObjectButtonScreenBox pCo, ref bool pIsObjSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonScreenBox
        #region SetButtonZoom
        public bool SetButtonZoom(CustomCaptureInfo pCci, CustomObjectButtonFpmlObject pCo, ref bool pIsSpecified, ref bool pIsEnabled)
        {
            return false;
        }
        #endregion SetButtonZoom
        #region IsButtonMenu
        public bool IsButtonMenu(CustomCaptureInfo pCci, ref CustomObjectButtonInputMenu pCo)
        {
            bool isButtonMenu = this.IsCci(CciEnum.identifier, pCci);
            if (isButtonMenu && (null != m_SQLTrade))
            {
                #region Trade
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin);
                pCo.PK = "IDT";
                pCo.PKV = m_SQLTrade.Id.ToString();
                #endregion Trade
            }
            return isButtonMenu;
        }
        #endregion IsButtonMenu
        #endregion ITradeGetInfoButton Members
        #endregion Interfaces

        #region Methods
        #region InitializeSQLTable
        private void InitializeSQLTable()
        {
            if (null != m_AvailableInvoice)
            {
                if (0 < m_AvailableInvoice.OTCmlId)
                    m_SQLTrade = new SQL_TradeCommon(m_CciInvoiceSettlement.CS, m_AvailableInvoice.OTCmlId);
            }
        }
        #endregion InitializeSQLTable
        #endregion Methods
    }
}
