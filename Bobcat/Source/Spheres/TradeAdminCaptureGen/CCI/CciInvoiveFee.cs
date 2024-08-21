#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceFee.
    /// </summary>
    /// EG 20171003 [23452] tradeDateTime remplace tradeDate et timeStamping
    public class CciInvoiceFee : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {

        #region Enums
        #region CciEnum
        // EG 20171003 [23452]
        public enum CciEnum
        {
            [System.Xml.Serialization.XmlEnumAttribute("OTCmlId")]
            otcmlId,
            [System.Xml.Serialization.XmlEnumAttribute("feeType")]
            feeType,
            [System.Xml.Serialization.XmlEnumAttribute("feeAmount.amount")]
            feeAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeAmount.currency")]
            feeAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeBaseAmount.amount")]
            feeBaseAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeBaseAmount.currency")]
            feeBaseAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeInitialAmount.amount")]
            feeInitialAmount_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeInitialAmount.currency")]
            feeInitialAmount_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeAccountingAmount.amount")]
            feeAmount_Accounting_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeAccountingAmount.currency")]
            feeAmount_Accounting_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeBaseAccountingAmount.amount")]
            feeBaseAmount_Accounting_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeBaseAccountingAmount.currency")]
            feeBaseAmount_Accounting_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeInitialAccountingAmount.amount")]
            feeInitialAmount_Accounting_amount,
            [System.Xml.Serialization.XmlEnumAttribute("feeInitialAccountingAmount.currency")]
            feeInitialAmount_Accounting_currency,
            [System.Xml.Serialization.XmlEnumAttribute("feeDate")]
            feeDate,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.OTCmlId")]
            idFeeSchedule,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.identifier")]
            feeScheduleIdentifier,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.formulaDCF")]
            formulaDCF,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.duration")]
            duration,
            //PL 20141023
            //[System.Xml.Serialization.XmlEnumAttribute("feeSchedule.assessmentBasisValue")]
            //assessmentBasisValue,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.assessmentBasisValue1")]
            assessmentBasisValue1,
            [System.Xml.Serialization.XmlEnumAttribute("feeSchedule.assessmentBasisValue2")]
            assessmentBasisValue2,

            [System.Xml.Serialization.XmlEnumAttribute("identifier")]
            trade_identifier,
            trade_displayName,
            trade_description,
            [System.Xml.Serialization.XmlEnumAttribute("OTCmlId")]
            trade_otcmlId,
            [System.Xml.Serialization.XmlEnumAttribute("instrument")]
            instrument_identifier,
            instrument_displayName,
            instrument_description,
            [System.Xml.Serialization.XmlEnumAttribute("instrument.OTCmlId")]
            instrument_otcmlId,
            [System.Xml.Serialization.XmlEnumAttribute("tradeDate")]
            tradeDate,
            [System.Xml.Serialization.XmlEnumAttribute("orderEntered")]
            orderEntered,
            [System.Xml.Serialization.XmlEnumAttribute("inDate")]
            trade_inDate,
            [System.Xml.Serialization.XmlEnumAttribute("outDate")]
            trade_outDate,
            [System.Xml.Serialization.XmlEnumAttribute("currency")]
            trade_currency,
            [System.Xml.Serialization.XmlEnumAttribute("side")]
            trade_side,
            [System.Xml.Serialization.XmlEnumAttribute("counterpartyPartyReference")]
            trade_counterparty,
            [System.Xml.Serialization.XmlEnumAttribute("notionalAmount.amount")]
            trade_notional_amount,
            [System.Xml.Serialization.XmlEnumAttribute("notionalAmount.currency")]
            trade_notional_currency,

            unknown,
        }
        #endregion CciEnum
        #endregion Enums
        #region Members
        private readonly CciInvoice m_CciInvoice;
        private readonly CciInvoiceTrade m_CciInvoiceTrade;
        private IInvoiceFee m_InvoiceFee;
        private readonly string m_Prefix;
        private readonly int m_Number;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private SQL_Event m_SQLEvent;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }

        #endregion Ccis
        #region ExistSQLEvent
        public bool ExistSQLEvent
        {
            get { return ((null != m_SQLEvent) && m_SQLEvent.IsLoaded); }
        }
        #endregion ExistSQLEvent
        #region InvoiceFee
        public IInvoiceFee InvoiceFee
        {
            get { return m_InvoiceFee; }
            set
            {
                m_InvoiceFee = value;
                InitializeSQLTable();
            }
        }
        #endregion InvoiceFee
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
        public CciInvoiceFee(CciInvoice pCciInvoice, CciInvoiceTrade pCciInvoiceTrade, string pPrefix, int pInvoiceTradeNumber, IInvoiceFee pInvoiceFee)
        {
            m_CciInvoice = pCciInvoice;
            m_CciInvoiceTrade = pCciInvoiceTrade;
            m_Ccis = pCciInvoice.Ccis;
            m_Number = pInvoiceTradeNumber;
            m_Prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            m_InvoiceFee = pInvoiceFee;
            InitializeSQLTable();
        }
        #endregion Constructors
        #region Interfaces
        #region IContainerCciFactory Members
        #region AddCciSystem
        /// <summary>
        /// Adding missing controls that are necessary for process intilialize
        /// </summary>
        /// FI 20170116  [21916] Modify (use AddCciSystem Method)
        public void AddCciSystem()
        {
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.otcmlId), true, TypeData.TypeDataEnum.@string);
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
            CciTools.CreateInstance(this, m_InvoiceFee);
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        public void Initialize_FromDocument()
        {
            string data;
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
                    isSetting = true;
                    isToValidate = false;
                    sql_Table = null;
                    #endregion Reset variables

                    CciEnum keyEnum = (CciEnum)System.Enum.Parse(typeof(CciEnum), enumName);
                    switch (keyEnum)
                    {
                        case CciEnum.otcmlId:
                            #region Fee (Id = IDE)
                            data = m_InvoiceFee.OtcmlId;
                            #endregion Fee (Id = IDE)
                            break;
                        case CciEnum.feeType:
                            #region FeeType (EVENTTYPE)
                            data = m_InvoiceFee.FeeType.Value;
                            #endregion FeeType (EVENTTYPE)
                            break;
                        case CciEnum.feeDate:
                            #region FeeDate (Invoice Date)
                            data = m_InvoiceFee.FeeDate.Value;
                            #endregion FeeDate (Invoice Date)
                            break;
                        case CciEnum.feeAmount_amount:
                            #region FeeAmount (Amount)
                            data = m_InvoiceFee.FeeAmount.Amount.Value;
                            #endregion FeeAmount (Amount)
                            break;
                        case CciEnum.feeAmount_currency:
                            #region FeeAmount (Currency)
                            data = m_InvoiceFee.FeeAmount.Currency;
                            #endregion FeeAmount (Currency)
                            break;
                        case CciEnum.feeBaseAmount_amount:
                            #region FeeBaseAmount (Amount)
                            data = m_InvoiceFee.FeeBaseAmount.Amount.Value;
                            #endregion FeeBaseAmount (Amount)
                            break;
                        case CciEnum.feeBaseAmount_currency:
                            #region FeeBaseAmount (Currency)
                            data = m_InvoiceFee.FeeBaseAmount.Currency;
                            #endregion FeeBaseAmount (Currency)
                            break;
                        case CciEnum.feeInitialAmount_amount:
                            #region FeeInitialAmount (Amount)
                            data = m_InvoiceFee.FeeInitialAmount.Amount.Value;
                            #endregion FeeInitialAmount (Amount)
                            break;
                        case CciEnum.feeInitialAmount_currency:
                            #region FeeInitialAmount (Currency)
                            data = m_InvoiceFee.FeeInitialAmount.Currency;
                            #endregion FeeInitialAmount (Currency)
                            break;
                        case CciEnum.feeAmount_Accounting_amount:
                            #region FeeAccountingAmount (Amount)
                            data = m_InvoiceFee.FeeAccountingAmount.Amount.Value;
                            #endregion FeeAccountingAmount (Amount)
                            break;
                        case CciEnum.feeAmount_Accounting_currency:
                            #region FeeAccountingAmount (Currency)
                            data = m_InvoiceFee.FeeAccountingAmount.Currency;
                            #endregion FeeAccountingAmount (Currency)
                            break;
                        case CciEnum.feeBaseAmount_Accounting_amount:
                            #region FeeBaseAccountingAmount (Amount)
                            data = m_InvoiceFee.FeeBaseAccountingAmount.Amount.Value;
                            #endregion FeeBaseAccountingAmount (Amount)
                            break;
                        case CciEnum.feeBaseAmount_Accounting_currency:
                            #region FeeBaseAccountingAmount (Currency)
                            data = m_InvoiceFee.FeeBaseAccountingAmount.Currency;
                            #endregion FeeBaseAccountingAmount (Currency)
                            break;
                        case CciEnum.feeInitialAmount_Accounting_amount:
                            #region FeeInitialAccountingAmount (Amount)
                            data = m_InvoiceFee.FeeInitialAccountingAmount.Amount.Value;
                            #endregion FeeInitialAccountingAmount (Amount)
                            break;
                        case CciEnum.feeInitialAmount_Accounting_currency:
                            #region FeeInitialAccountingAmount (Currency)
                            data = m_InvoiceFee.FeeInitialAccountingAmount.Currency;
                            #endregion FeeInitialAccountingAmount (Currency)
                            break;
                        case CciEnum.idFeeSchedule:
                            #region FeeSchedule (Id)
                            data = m_InvoiceFee.FeeSchedule.OtcmlId;
                            #endregion FeeSchedule (Id)
                            break;
                        case CciEnum.feeScheduleIdentifier:
                            #region FeeSchedule (Identifier)
                            if (m_InvoiceFee.FeeScheduleSpecified && m_InvoiceFee.FeeSchedule.IdentifierSpecified)
                                data = m_InvoiceFee.FeeSchedule.Identifier.Value;
                            #endregion FeeSchedule (Identifier)
                            break;
                        case CciEnum.formulaDCF:
                            #region FormulaDCF
                            if (m_InvoiceFee.FeeScheduleSpecified && m_InvoiceFee.FeeSchedule.FormulaDCFSpecified)
                                data = m_InvoiceFee.FeeSchedule.FormulaDCF.Value;
                            #endregion FormulaDCF
                            break;
                        case CciEnum.duration:
                            #region Duration
                            if (m_InvoiceFee.FeeScheduleSpecified && m_InvoiceFee.FeeSchedule.DurationSpecified)
                                data = m_InvoiceFee.FeeSchedule.Duration.Value;
                            #endregion Duration
                            break;
                        case CciEnum.assessmentBasisValue1:
                            //PL 20141023
                            #region AssessmentBasisValue1
                            if (m_InvoiceFee.FeeScheduleSpecified && m_InvoiceFee.FeeSchedule.AssessmentBasisValue1Specified)
                                data = m_InvoiceFee.FeeSchedule.AssessmentBasisValue1.Value;
                            #endregion AssessmentBasisValue1
                            break;
                        case CciEnum.assessmentBasisValue2:
                            //PL 20141023
                            #region AssessmentBasisValue2
                            if (m_InvoiceFee.FeeScheduleSpecified && m_InvoiceFee.FeeSchedule.AssessmentBasisValue2Specified)
                                data = m_InvoiceFee.FeeSchedule.AssessmentBasisValue2.Value;
                            #endregion AssessmentBasisValue2
                            break;
                        case CciEnum.trade_otcmlId:
                            #region Trade (Id)
                            data = m_CciInvoiceTrade.InvoiceTrade.OTCmlId.ToString();
                            #endregion Trade (Id)
                            break;
                        case CciEnum.trade_identifier:
                            #region Trade (Identifier)
                            if (null != m_CciInvoiceTrade.InvoiceTrade.Identifier)
                                data = m_CciInvoiceTrade.InvoiceTrade.Identifier.Value;
                            #endregion Trade (Identifier)
                            break;
                        case CciEnum.trade_displayName:
                            #region Trade (DisplayName)
                            if (m_CciInvoiceTrade.ExistSQLTrade)
                                data = m_CciInvoiceTrade.SQLTrade.DisplayName;
                            #endregion Trade (DisplayName)
                            break;
                        case CciEnum.instrument_otcmlId:
                            #region Instrument (Id)
                            data = m_CciInvoiceTrade.InvoiceTrade.Instrument.OTCmlId.ToString();
                            #endregion Instrument (Id)
                            break;
                        case CciEnum.instrument_identifier:
                            #region Instrument (Identifier)
                            if (m_CciInvoiceTrade.ExistSQLInstrument)
                                data = m_CciInvoiceTrade.SQLInstrument.DisplayName;
                            #endregion Instrument (Identifier)
                            break;
                        case CciEnum.instrument_displayName:
                            #region Instrument (DisplayName)
                            if (m_CciInvoiceTrade.ExistSQLInstrument)
                                data = m_CciInvoiceTrade.SQLInstrument.DisplayName;
                            #endregion Instrument (DisplayName)
                            break;
                        case CciEnum.tradeDate:
                            #region TradeDate
                            if (null != m_CciInvoiceTrade.InvoiceTrade.TradeDate)
                                data = m_CciInvoiceTrade.InvoiceTrade.TradeDate.Value;
                            #endregion TradeDate
                            break;
                        case CciEnum.orderEntered:
                            #region OrderEntered
                            //data = m_CciInvoiceTrade.InvoiceTrade.orderEntered.Value;
                            //// Set TimeZone of TradeDateTime
                            //Ccis.InitializeCci(CciZone, sql_Table, m_CciInvoiceTrade.InvoiceTrade.orderEntered.tzdbid);
                            #endregion OrderEntered
                            break;
                        case CciEnum.trade_inDate:
                            #region TradeDate
                            if (null != m_CciInvoiceTrade.InvoiceTrade.InDate)
                                data = m_CciInvoiceTrade.InvoiceTrade.InDate.Value;
                            #endregion TradeDate
                            break;
                        case CciEnum.trade_outDate:
                            #region TradeDate
                            if (null != m_CciInvoiceTrade.InvoiceTrade.OutDate)
                                data = m_CciInvoiceTrade.InvoiceTrade.OutDate.Value;
                            #endregion TradeDate
                            break;
                        case CciEnum.trade_currency:
                            #region Currency
                            if (null != m_CciInvoiceTrade.InvoiceTrade.Currency)
                                data = m_CciInvoiceTrade.InvoiceTrade.Currency.Value;
                            #endregion Currency
                            break;
                        case CciEnum.trade_side:
                            #region Side
                            data = m_CciInvoiceTrade.InvoiceTrade.Side.ToString();
                            #endregion Side
                            break;
                        case CciEnum.trade_counterparty:
                            #region Counterparty (Identifier)
                            if (null != m_CciInvoiceTrade.InvoiceTrade.CounterpartyPartyReference)
                            {
                                IParty party = m_CciInvoice.DataDocument.GetParty(m_CciInvoiceTrade.InvoiceTrade.CounterpartyPartyReference.HRef, PartyInfoEnum.id);
                                if (null != party)
                                    data = party.PartyId;
                            }
                            #endregion Counterparty (Identifier)
                            break;
                        case CciEnum.trade_notional_amount:
                            #region NotionalAmount (Amount)
                            data = m_CciInvoiceTrade.InvoiceTrade.NotionalAmount.Amount.Value;
                            #endregion NotionalAmount (Amount)
                            break;
                        case CciEnum.trade_notional_currency:
                            #region NotionalAmount (Currency)
                            data = m_CciInvoiceTrade.InvoiceTrade.NotionalAmount.Currency;
                            #endregion NotionalAmount (Currency)
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
            bool isButtonMenu = this.IsCci(CciInvoiceFee.CciEnum.otcmlId, pCci);
            if (isButtonMenu && (null != m_SQLEvent))
            {
                #region Event
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputEvent);
                pCo.PK = "IDE";
                pCo.PKV = m_SQLEvent.Id.ToString();
                pCo.FKV = m_SQLEvent.IdT.ToString();
                #endregion Event
            }
            else
                if (false == isButtonMenu)
                {
                    isButtonMenu = this.IsCci(CciEnum.trade_otcmlId, pCci);
                    if (isButtonMenu && m_CciInvoiceTrade.ExistSQLTrade)
                    {
                        #region Trade
                        pCo.ClientId = pCci.ClientId_WithoutPrefix;
                        pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade);
                        pCo.PK = "IDT";
                        pCo.PKV = m_CciInvoiceTrade.SQLTrade.IdT.ToString();
                        #endregion Trade
                    }
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
            if (null != m_InvoiceFee)
            {
                if (0 < m_InvoiceFee.OTCmlId)
                    m_SQLEvent = new SQL_Event(m_CciInvoice.CS, m_InvoiceFee.OTCmlId);
            }
        }
        #endregion InitializeSQLTable
        #endregion Methods
    }
}
