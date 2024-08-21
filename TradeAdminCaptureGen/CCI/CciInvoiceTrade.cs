#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EfsML.Enum;
using EfsML.Interface;
using FpML.Interface;
using System;
using System.Collections;
#endregion Using Directives

namespace EFS.TradeInformation
{
    /// <summary>
    /// Description résumée de CciInvoiceTrade.
    /// </summary>
    // 20090427 EG Add side (TradeSideEnum)
    // EG 20171003 [23452] tradeDateTime remplace tradeDate et timeStamping
    public class CciInvoiceTrade : IContainerCciFactory, IContainerCci, IContainerCciGetInfoButton
    {

        #region Enums
        #region CciEnum
        // EG 20171003 [23452]
        public enum CciEnum
        {
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
        private IInvoiceTrade m_InvoiceTrade;
        private readonly string m_Prefix;
        private readonly int m_Number;
        private readonly TradeAdminCustomCaptureInfos m_Ccis;
        private SQL_TradeCommon m_SQLTrade;
        private SQL_Instrument m_SQLInstrument;
        private CciInvoiceFee[] m_CciInvoiceFee;
        #endregion Members
        #region Accessors
        #region Ccis
        public TradeAdminCustomCaptureInfos Ccis
        {
            get { return m_Ccis; }
        }
        #endregion Ccis
        #region ExistSQLInstrument
        public bool ExistSQLInstrument
        {
            get { return ((null != m_SQLInstrument) && m_SQLInstrument.IsLoaded); }
        }
        #endregion ExistSQLInstrument
        #region ExistSQLTrade
        public bool ExistSQLTrade
        {
            get { return ((null != m_SQLTrade) && m_SQLTrade.IsLoaded); }
        }
        #endregion ExistSQLTrade
        #region InvoiceFees
        public IInvoiceFees InvoiceFees
        {
            get { return m_InvoiceTrade.InvoiceFees; }
        }
        #endregion InvoiceFees
        #region InvoiceFees
        /*
        public IInvoiceFee[] InvoiceFee
        {
            get 
            { 
                if (null != InvoiceFees)
                    return InvoiceFees.invoiceFee;
                return null;
            }
        }
        */
        #endregion InvoiceFees
        #region InvoiceFeeLength
        public int InvoiceFeeLength
        {
            get { return ArrFunc.IsFilled(m_CciInvoiceFee) ? m_CciInvoiceFee.Length : 0; }
        }
        #endregion InvoiceFeeLength
        #region InvoiceTrade
        public IInvoiceTrade InvoiceTrade
        {
            get { return m_InvoiceTrade; }
            set
            {
                m_InvoiceTrade = value;
                InitializeSQLTable();
            }
        }
        #endregion InvoiceTrade
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
        #region SQLInstrument
        public SQL_Instrument SQLInstrument
        {
            get { return m_SQLInstrument; }
        }
        #endregion SQLInstrument
        #region SQLTrade
        public SQL_TradeCommon SQLTrade
        {
            get { return m_SQLTrade; }
        }
        #endregion SQLTrade
        #endregion Accessors
        #region Constructors
        public CciInvoiceTrade(CciInvoice pCciInvoice, string pPrefix, int pInvoiceTradeNumber, IInvoiceTrade pInvoiceTrade)
        {
            m_CciInvoice = pCciInvoice;
            m_Ccis = pCciInvoice.Ccis;
            m_Number = pInvoiceTradeNumber;
            m_Prefix = pPrefix + NumberPrefix + CustomObject.KEY_SEPARATOR;
            m_InvoiceTrade = pInvoiceTrade;
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
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.trade_otcmlId), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.trade_displayName), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.trade_identifier), true, TypeData.TypeDataEnum.@string);
            CciTools.AddCciSystem(Ccis, Cst.TXT + CciClientId(CciEnum.instrument_displayName), true, TypeData.TypeDataEnum.@string);

            for (int i = 0; i < InvoiceFeeLength; i++)
                m_CciInvoiceFee[i].AddCciSystem();
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
            CciTools.CreateInstance(this, m_InvoiceTrade);
            InitializeInvoiceFee_FromCci();
        }
        #endregion Initialize_FromCci
        #region Initialize_FromDocument
        /// <summary>
        /// Initialisation des CCI à partir des données "PRODUCT" présentes dans les classes du Document XML
        /// </summary>
        public void Initialize_FromDocument()
        {
            string data;
            //string display;
            bool isSetting;
            SQL_Table sql_Table;
            bool isToValidate;
            Type tCciEnum = typeof(CciEnum);
            //CustomCaptureInfosBase.ProcessQueueEnum processQueue;

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
                        case CciEnum.trade_otcmlId:
                            #region Trade (Id)
                            data = m_InvoiceTrade.OTCmlId.ToString();
                            #endregion Trade (Id)
                            break;
                        case CciEnum.trade_identifier:
                            #region Trade (Identifier)
                            if (ExistSQLTrade)
                                data = m_SQLTrade.Identifier;
                            #endregion Trade (Identifier)
                            break;
                        case CciEnum.trade_displayName:
                            #region Trade (DisplayName)
                            if (ExistSQLTrade)
                                data = m_SQLTrade.DisplayName;
                            #endregion Trade (DisplayName)
                            break;
                        case CciEnum.instrument_otcmlId:
                            #region Instrument (Id)
                            data = m_InvoiceTrade.Instrument.OTCmlId.ToString();
                            #endregion Instrument (Id)
                            break;
                        case CciEnum.instrument_identifier:
                            #region Instrument (Identifier)
                            if (ExistSQLInstrument)
                                data = m_SQLInstrument.Identifier;
                            #endregion Instrument (Identifier)
                            break;
                        case CciEnum.instrument_displayName:
                            #region Instrument (DisplayName)
                            if (ExistSQLInstrument)
                                data = m_SQLInstrument.DisplayName;
                            #endregion Instrument (DisplayName)
                            break;
                        case CciEnum.tradeDate:
                            #region TradeDate
                            data = m_InvoiceTrade.TradeDate.Value;
                            #endregion TradeDate
                            break;
                        case CciEnum.trade_inDate:
                            #region InDate
                            data = m_InvoiceTrade.InDate.Value;
                            #endregion InDate
                            break;
                        case CciEnum.trade_outDate:
                            #region OutDate
                            data = m_InvoiceTrade.OutDate.Value;
                            #endregion OutDate
                            break;
                        case CciEnum.trade_currency:
                            #region Currency
                            data = m_InvoiceTrade.Currency.Value;
                            #endregion Currency
                            break;
                        case CciEnum.trade_side:
                            #region Side
                            data = m_InvoiceTrade.Side.ToString();
                            #endregion Side
                            break;
                        case CciEnum.trade_counterparty:
                            #region Counterparty (Identifier)
                            if (null != m_InvoiceTrade.CounterpartyPartyReference)
                            {
                                IParty party = m_CciInvoice.DataDocument.GetParty(m_InvoiceTrade.CounterpartyPartyReference.HRef, PartyInfoEnum.id);
                                if (null != party)
                                    data = party.PartyId;
                            }
                            #endregion Counterparty (Identifier)
                            break;
                        case CciEnum.trade_notional_amount:
                            #region NotionalAmount (Amount)
                            data = m_InvoiceTrade.NotionalAmount.Amount.Value;
                            #endregion NotionalAmount (Amount)
                            break;
                        case CciEnum.trade_notional_currency:
                            #region NotionalAmount (Currency)
                            data = m_InvoiceTrade.NotionalAmount.Currency;
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
            for (int i = 0; i < InvoiceFeeLength; i++)
                m_CciInvoiceFee[i].Initialize_FromDocument();
        }
        #endregion Initialize_FromDocument
        #region IsClientId_PayerOrReceiver
        public bool IsClientId_PayerOrReceiver(CustomCaptureInfo pCci)
        {
            return false;
        }
        #endregion
        #region Dump_ToDocument
        /// <summary>
        /// Déversement des données "PRODUCT" issues des CCI, dans les classes du Document XML
        /// </summary>
        public void Dump_ToDocument()
        {
            for (int i = 0; i < InvoiceFeeLength; i++)
                m_CciInvoiceFee[i].Dump_ToDocument();
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
            for (int i = 0; i < InvoiceFeeLength; i++)
                m_CciInvoiceFee[i].ProcessInitialize(pCci);
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
            bool isButtonMenu = this.IsCci(CciInvoiceTrade.CciEnum.trade_otcmlId, pCci);
            if (isButtonMenu && (null != m_SQLTrade))
            {
                #region Trade
                pCo.ClientId = pCci.ClientId_WithoutPrefix;
                pCo.Menu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade);
                pCo.PK = "IDT";
                pCo.PKV = m_SQLTrade.IdT.ToString();
                #endregion Trade
            }
            if (false == isButtonMenu)
            {
                for (int i = 0; i < InvoiceFeeLength; i++)
                {
                    isButtonMenu = m_CciInvoiceFee[i].IsButtonMenu(pCci, ref pCo);
                    if (isButtonMenu)
                        break;
                }
            }
            return isButtonMenu;

        }
        #endregion IsButtonMenu
        #endregion ITradeGetInfoButton Members

        #endregion Interfaces

        #region Methods
        #region GetInvoiceFee
        public IInvoiceFee[] GetInvoiceFee()
        {
            if (null != InvoiceFees)
                return InvoiceFees.InvoiceFee;
            return null;
        }
        #endregion GetInvoiceFee

        #region InitializeInvoiceFee_FromCci
        private void InitializeInvoiceFee_FromCci()
        {
            bool isOk = true;
            int index = -1;
            ArrayList lst = new ArrayList();
            while (isOk)
            {
                index += 1;
                CciInvoiceFee cciInvoiceFee = new CciInvoiceFee(m_CciInvoice, this, m_Prefix + TradeAdminCustomCaptureInfos.CCst.Prefix_InvoiceFee, index + 1, null);
                //
                isOk = Ccis.Contains(cciInvoiceFee.CciClientId(CciInvoiceFee.CciEnum.feeType));
                if (isOk)
                {
                    if (ArrFunc.IsEmpty(GetInvoiceFee()) || (index == GetInvoiceFee().Length))
                    {
                        ReflectionTools.AddItemInArray(InvoiceFees, "invoiceFee", index);
                        if (ArrFunc.IsFilled(Ccis.TradeCommonInput.FpMLDataDocReader.Party))
                        {
                            // TODO
                        }
                    }
                    //
                    cciInvoiceFee.InvoiceFee = GetInvoiceFee()[index];
                    lst.Add(cciInvoiceFee);
                }
            }
            m_CciInvoiceFee = (CciInvoiceFee[])lst.ToArray(typeof(CciInvoiceFee));
            //
            for (int i = 0; i < InvoiceFeeLength; i++)
                m_CciInvoiceFee[i].Initialize_FromCci();
        }
        #endregion InitializeInvoiceFee_FromCci
        #region InitializeSQLTable
        private void InitializeSQLTable()
        {
            if (null != m_InvoiceTrade)
            {
                if (0 < m_InvoiceTrade.OTCmlId)
                    m_SQLTrade = new SQL_TradeCommon(m_CciInvoice.CS, m_InvoiceTrade.OTCmlId);
                if ((null != m_InvoiceTrade.Instrument) && (0 < m_InvoiceTrade.Instrument.OTCmlId))
                    m_SQLInstrument = new SQL_Instrument(m_CciInvoice.CS, m_InvoiceTrade.Instrument.OTCmlId);
            }
        }
        #endregion InitializeSQLTable
        #endregion Methods
    }
}
