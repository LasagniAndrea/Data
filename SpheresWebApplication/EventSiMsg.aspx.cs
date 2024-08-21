#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Settlement.Message;
using FpML.Enum;
using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de EventSiMsgPage.
    /// </summary>
    /// FI 20160804 [Migration TFS] Modify 
    public partial class EventSiMsgPage : PageBase
    {
        #region Members
        protected bool m_IsPayer;
        protected int m_IdE;
        protected DataSet m_DsEventSiMsg;
        protected DataSet m_DsEvent;
        private string m_ParentGUID;
        private TradeCommonInputGUI m_TradeCommonInputGUI;
        private TradeCommonInput m_TradeCommonInput;
        //FI 20160804 [Migration TFS] new folder
        //protected string m_XSLEventMsg_Payer = @"~\OTCml\XSL_Files\Message\Swift\Category2\MT202_SWIFT.xslt";
        //protected string m_XSLEventMsg_Receiver = @"~\OTCml\XSL_Files\Message\Swift\Category2\MT210_SWIFT.xslt";
        protected string m_XSLEventMsg_Payer = @"~\Message\Swift\Category2\MT202_SWIFT.xslt";
        protected string m_XSLEventMsg_Receiver = @"~\Message\Swift\Category2\MT210_SWIFT.xslt";

        
        protected string m_Mt202Identifier = "SPHERES_MT202_SWIFT";
        protected string m_Mt210Identifier = "SPHERES_MT210_SWIFT";
        ISettlementMessageDocument m_StlMsgDoc;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        private DataTable DtEvent
        {
            get
            {
                return m_DsEvent.Tables[0];
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        /// <summary>
        /// 
        /// </summary>
        public TradeCommonInput TradeCommonInput
        {
            set { m_TradeCommonInput = value; }
            get { return m_TradeCommonInput; }
        }


        /// <summary>
        /// Obtient true si les variables session sont disponibles
        /// <remarks>Lorsque les variables session sont non disponibles cette page s'autoclose</remarks>
        /// </summary>
        protected bool IsSessionVariableAvailable
        {

            get { return (null != m_TradeCommonInput); }
        }


        #endregion Accessors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);
            InitializeComponent();
            AbortRessource = true;
            m_IdE = Convert.ToInt32(Request.QueryString["IdE"]);
            
            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Utilisation de DataCache
            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputSessionID);
                
            
            string payerReceiver = Request.QueryString["P_R"].ToUpper();
            m_IsPayer = (payerReceiver == PayerReceiverEnum.Payer.ToString().ToUpper());
            
            string title = Ressource.GetString("SettlementInformationsMessage", true);
            GenerateHtmlForm();
            HtmlPageTitle titleLeft = new HtmlPageTitle(title);
            this.PageTitle = title;
            

            string idMenu = string.Empty;
            string cssMode = string.Empty;
            if (IsSessionVariableAvailable)
            {
                idMenu = m_TradeCommonInputGUI.IdMenu;
                cssMode = m_TradeCommonInputGUI.CssMode;
            }


            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, cssMode, false, string.Empty, idMenu);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLoad(object sender, System.EventArgs e)
        {
            PageConstruction();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnXmlClick(object sender, EventArgs e)
        {
            if (null != m_StlMsgDoc)
            {
                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(m_StlMsgDoc);
                StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
                DisplayXml("EventSiMsg_XML", "EventSiMsg_" + m_IdE, sb.ToString());
            }
        }

        #endregion Events
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et compléments
        private void DisplayEventMsg()
        {
            LoadEvent();
            if (0 < DtEvent.Rows.Count)
            {
                LoadSettlementMessageDocument();
                Table tblEventSiMsg = new Table
                {
                    Width = Unit.Percentage(100)
                };
                tblEventSiMsg.Rows.Add(new TableRow());
                tblEventSiMsg.Rows.Add(new TableRow());
                tblEventSiMsg.Rows.Add(new TableRow());
                TableCell td = new TableCell
                {
                    ColumnSpan = 2,
                    VerticalAlign = VerticalAlign.Middle,
                    Width = Unit.Percentage(100)
                };
                td.Controls.Add(GetTableTitleEventMsg());
                tblEventSiMsg.Rows[0].Cells.Add(td);
                td = new TableCell
                {
                    ColumnSpan = 2,
                    VerticalAlign = VerticalAlign.Middle,
                    Width = Unit.Percentage(100)
                };
                td.Controls.Add(GetTableSubTitleEventMsg());
                tblEventSiMsg.Rows[1].Cells.Add(td);
                string msg = TransformSettlementMessageDocument();
                msg = Cst.HTMLBreakLine + msg.Replace(Cst.Lf, Cst.HTMLBreakLine);
                td = new TableCell
                {
                    VerticalAlign = VerticalAlign.Middle,
                    Width = Unit.Percentage(100),
                    Text = msg
                };
                tblEventSiMsg.Rows[2].Cells.Add(td);
                td = new TableCell
                {
                    VerticalAlign = VerticalAlign.Middle
                };
                WCToolTipLinkButton btn = new WCToolTipLinkButton()
                {
                    ID = "btnXML",
                    CssClass = "fa-icon",
                    AccessKey = "X",
                    CausesValidation = false,
                    Text = "<i class='fas fa-external-link-alt'></i> XML"
                };
                td.Controls.Add(btn);
                tblEventSiMsg.Rows[2].Cells.Add(td);
                Control plhEventSiMsg = FindControl("plhEventSiMsg");
                if (null != plhEventSiMsg)
                    plhEventSiMsg.Controls.Add(tblEventSiMsg);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            //
            Form.ID = "frmEventSiMsg";
            PlaceHolder plhEventSiMsg = new PlaceHolder
            {
                ID = "plhEventSiMsg"
            };
            CellForm.Controls.Add(plhEventSiMsg);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetMsgName()
        {
            return (m_IsPayer ? m_Mt202Identifier : m_Mt210Identifier);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Table GetTableSubTitleEventMsg()
        {
            DataRow dr = DtEvent.Rows[0];
            Table tblTitle = new Table
            {
                CssClass = "DataGrid",
                CellSpacing = 0,
                CellPadding = 1,
                Width = Unit.Percentage(100)
            };
            TableRow tr = new TableRow();
            tr.Cells.Add(TableTools.AddHeaderCell("Payer", false, 100, UnitEnum.Pixel, 1, true));
            tr.Cells.Add(TableTools.AddHeaderCell(dr["PAYER_IDENTIFIER"].ToString(), false, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Receiver", false, 100, UnitEnum.Pixel, 1, true));
            tr.Cells.Add(TableTools.AddHeaderCell(dr["RECEIVER_IDENTIFIER"].ToString(), false, 0, UnitEnum.Pixel, 0, true));
            tblTitle.Rows.Add(tr);
            return tblTitle;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Table GetTableTitleEventMsg()
        {
            DataRow dr = DtEvent.Rows[0];
            Table tblTitle = new Table
            {
                CssClass = "DataGrid",
                CellSpacing = 0,
                CellPadding = 1,
                Width = Unit.Percentage(100)
            };
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_HeaderStyle",
            };
            tr.Cells.Add(TableTools.AddHeaderCell("Message", true, 0, UnitEnum.Pixel, 2, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Trade", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Instrument", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("EventCode", true, 0, UnitEnum.Pixel, 2, true));
            tr.Cells.Add(TableTools.AddHeaderCell("EventDates", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Valorisation", true, 0, UnitEnum.Pixel, 2, true));
            tblTitle.Rows.Add(tr);
            tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            tr.Cells.Add(TableTools.AddCell(GetMsgName(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(dr["DTSTMFORCED"]).ToShortDateString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(dr["TRADE_IDENTIFIER"].ToString() + " [" + dr["IDT"].ToString() + "]", HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(dr["INSTR_IDENTIFIER"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(dr["EVENTCODE"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(dr["EVENTTYPE"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(dr["DTSTLFORCED"]).ToShortDateString(), HorizontalAlign.Center));
            Decimal amount = Convert.ToDecimal(dr["AMOUNT"] is DBNull ? 0 : dr["AMOUNT"]);
            string currency = dr["IDC"].ToString();
            EFS_Round efs_Round = new EFS_Round(SessionTools.CS, currency, amount);
            tr.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(efs_Round.AmountRounded.ToString(NumberFormatInfo.InvariantInfo)), HorizontalAlign.Right));
            tr.Cells.Add(TableTools.AddCell(currency, HorizontalAlign.Center));
            tblTitle.Rows.Add(tr);
            return tblTitle;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetXsltFile()
        {
            string ret = string.Empty;

            SQL_SettlementMessage sqlStlMessage = new SQL_SettlementMessage(SessionTools.CS, GetMsgName());
            SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, sqlStlMessage.XsltFile, ref ret);

            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.OnLoad);
        }
        /// <summary>o
        /// 
        /// </summary>
        private void LoadEvent()
        {
            string CS = SessionTools.CS;
            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDE), m_IdE);
            if (m_IsPayer)
                parameters.Add(new DataParameter(CS, "PAYER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Payer.ToString());
            else
                parameters.Add(new DataParameter(CS, "RECEIVER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), PayerReceiverEnum.Receiver.ToString());
            parameters.Add(new DataParameter(CS, "NETMETHOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), NettingMethodEnum.None);
            //
            string sQuery = SQLCst.SELECT + "e.IDE As ID, e.IDT, e.EVENTCODE, e.EVENTTYPE," + Cst.CrLf;
            if (m_IsPayer)
            {
                sQuery += "e.IDA_PAY As IDA_SENDERPARTY, e.IDA_REC As IDA_RECEIVERPARTY," + Cst.CrLf;
                sQuery += "@PAYER As PAYER_RECEIVER," + Cst.CrLf;
            }
            else
            {
                sQuery += "e.IDA_REC As IDA_SENDERPARTY, e.IDA_PAY As IDA_RECEIVERPARTY," + Cst.CrLf;
                sQuery += "@RECEIVER As PAYER_RECEIVER," + Cst.CrLf;
            }
            sQuery += "esi.IDA_STLOFFICE as IDA_SENDER, esi.IDA_MSGRECEIVER as IDA_RECEIVER,esi.IDA_CSS,";
            sQuery += DataHelper.SQLString("N/A", "SCREF") + "," + Cst.CrLf;
            sQuery += @"@NETMETHOD As NETMETHOD,0 As IDNETCONVENTION,0 As IDNETDESIGNATION," + Cst.CrLf;
            //
            sQuery += "e.VALORISATION As AMOUNT, e.UNIT as IDC," + Cst.CrLf;
            sQuery += "ec_stm.DTEVENT as DTSTM, ec_stm.DTEVENTFORCED as DTSTMFORCED," + Cst.CrLf;
            sQuery += "ec_stl.DTEVENT as DTSTL, ec_stl.DTEVENTFORCED as DTSTLFORCED," + Cst.CrLf;
            sQuery += "t.IDENTIFIER as TRADE_IDENTIFIER," + Cst.CrLf;
            sQuery += "i.IDI,i.IDENTIFIER as INSTR_IDENTIFIER, i.DISPLAYNAME as INSTR_DISPLAYNAME," + Cst.CrLf;
            sQuery += "payer.IDENTIFIER as PAYER_IDENTIFIER,receiver.IDENTIFIER as RECEIVER_IDENTIFIER" + Cst.CrLf;
            sQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT.ToString() + " e " + Cst.CrLf;
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTSI.ToString() + " esi on (esi.IDE = e.IDE)" + Cst.CrLf;
            if (m_IsPayer)
                sQuery += SQLCst.AND + " esi.PAYER_RECEIVER=@PAYER";
            else
                sQuery += SQLCst.AND + " esi.PAYER_RECEIVER=@RECEIVER";
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " payer on (payer.IDA = e.IDA_PAY)" + Cst.CrLf;
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " receiver on (receiver.IDA = e.IDA_REC)" + Cst.CrLf;
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + " ec_stl on (ec_stl.IDE = e.IDE) and (ec_stl.EVENTCLASS='STL')" + Cst.CrLf;
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS.ToString() + " ec_stm on (ec_stm.IDE = e.IDE) and (ec_stm.EVENTCLASS='STM')" + Cst.CrLf;
            sQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.TRADE.ToString() + " t on (t.IDT = e.IDT)" + Cst.CrLf;
            sQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.TRADEINSTRUMENT.ToString() + " ti on (ti.IDT = e.IDT) and (ti.INSTRUMENTNO = e.INSTRUMENTNO)" + Cst.CrLf;
            sQuery += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.INSTRUMENT.ToString() + " i on (i.IDI = ti.IDI)" + Cst.CrLf;
            //
            sQuery += SQLCst.WHERE + "e.IDE = @IDE";
            //
            m_DsEvent = DataHelper.ExecuteDataset(CS, CommandType.Text, sQuery, parameters.GetArrayDbParameter());
        }
        /// <summary>
        /// 
        /// </summary>
        private void LoadSettlementMessageDocument()
        {

            SettlementMessagePaymentStructure evt = new SettlementMessagePaymentStructure(DtEvent.Rows[0], Cst.OTCml_EventIdScheme, TradeCommonInput.EfsMlVersion);
            m_StlMsgDoc = TradeCommonInput.Product.ProductBase.CreateSettlementMessageDocument();
            SettlementMessageDocumentContainer stlMsgDocContainer = new SettlementMessageDocumentContainer(m_StlMsgDoc);
            stlMsgDocContainer.Initialize(SessionTools.CS, evt.id, new SettlementMessagePaymentStructure[] { evt }, GetMsgName());

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string TransformSettlementMessageDocument()
        {
            string ret = string.Empty;
            if (null != m_StlMsgDoc)
            {
                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(m_StlMsgDoc);
                StringBuilder sb = CacheSerializer.Serialize(serializeInfo);
                StrBuilder sbNoalias = new StrBuilder(XSLTTools.RemoveXmlnsAlias(sb));
                ret = XSLTTools.TransformXml(sbNoalias.StringBuilder, GetXsltFile(), null, null);
            }
            return ret;

        }
        
        /// <summary>
        /// 
        /// </summary>
        protected override void PageConstruction()
        {
            if (IsSessionVariableAvailable)
            {
                DisplayEventMsg();
            }
            else
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_SessionVariableNotAvailable");
                
                CloseAfterAlertImmediate = true;
            }
        }

        #endregion Methods
    }
}
