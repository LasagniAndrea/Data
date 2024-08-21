#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Enum.Tools;
using System;
using System.Collections;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de EventSiPage.
    /// </summary>
    public partial class EventSiPage : PageBase
    {
        #region Members
        protected DataSet m_DsEventSi;
        protected DataParameter m_ParamIdE;
        protected bool m_IsDeliveryMessage;
        protected string m_XSLEventSi;
        private string m_ParentGUID;
        private TradeCommonInputGUI m_TradeCommonInputGUI;
        private TradeCommonInput m_TradeCommonInput;
        #endregion Members
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }

        /// /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string InputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        
        #region TradeCommonInput
        public TradeCommonInput TradeCommonInput
        {
            set { m_TradeCommonInput = value; }
            get { return m_TradeCommonInput; }
        }
        #endregion TradeCommonInput

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
        #region OnInit
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            AbortRessource = true;
            m_ParamIdE = new DataParameter(SessionTools.CS, CommandType.Text, "IDE", DbType.Int64)
            {
                Value = Convert.ToInt32(Request.QueryString["IdE"])
            };
            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Utilisation de DataCache
            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(InputSessionID);

            PageConstruction();

        }
        #endregion OnInit
        #region OnLoad
        protected void OnLoad(object sender, System.EventArgs e)
        {
            if (IsSessionVariableAvailable)
            {
                DisplayEventSi();
                if (Request.Params["__EVENTTARGET"] == "DspSIXmlPayer")
                    DisplayEventSiXml(FpML.Enum.PayerReceiverEnum.Payer);
                else if (Request.Params["__EVENTTARGET"] == "DspSIXmlReceiver")
                    DisplayEventSiXml(FpML.Enum.PayerReceiverEnum.Receiver);
            }
            else
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_SessionVariableParentNotAvailable");
                CloseAfterAlertImmediate = true;
            }
        }
        #endregion OnLoad
        #endregion Events
        #region Methods
        #region CreateChildControls
        protected override void CreateChildControls()
        {
            JavaScript.OpenEventSiMsg(this, m_ParentGUID);
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        #region DisplayEventSi
        private void DisplayEventSi()
        {

            GetSelectEventSi();
            m_DsEventSi.DataSetName = "EventSi";
            DataTable dtEventSi = m_DsEventSi.Tables[0];
            dtEventSi.TableName = "EventSi";
            Table tblEventSi = new Table
            {
                Width = Unit.Percentage(100)
            };
            tblEventSi.Rows.Add(new TableRow());
            tblEventSi.Rows.Add(new TableRow());

            if (0 < dtEventSi.Rows.Count)
            {
                m_IsDeliveryMessage = EventClassFunc.IsDeliveryMessage(dtEventSi.Rows[0]["EVENTCLASS"].ToString());
                TableCell td;
                foreach (DataRow row in dtEventSi.Rows)
                {
                    td = new TableCell
                    {
                        Width = Unit.Percentage(50),
                        Text = TransformEventSi(row)
                    };
                    td.Style.Add(HtmlTextWriterStyle.VerticalAlign, "top");
                    if (null != tblEventSi)
                        tblEventSi.Rows[1].Cells.Add(td);
                }
                Control plhEventSi = FindControl("plhEventSi");
                if (null != plhEventSi)
                {
                    td = new TableCell
                    {
                        Width = Unit.Percentage(100),
                        ColumnSpan = dtEventSi.Rows.Count
                    };
                    td.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                    td.Controls.Add(TitleEventSi(dtEventSi.Rows[0]));
                    tblEventSi.Rows[0].Cells.Add(td);
                    plhEventSi.Controls.Add(tblEventSi);
                }
            }

        }
        #endregion DataGridEvents
        #region DisplayEventSiXml
        private void DisplayEventSiXml(FpML.Enum.PayerReceiverEnum pPayerReceiver)
        {

            StringBuilder sb = null;
            //
            if (null != m_DsEventSi)
            {
                DataTable dtEventSi = m_DsEventSi.Tables[0];
                if (0 < dtEventSi.Rows.Count)
                {
                    foreach (DataRow row in dtEventSi.Rows)
                    {
                        if (row["PAYER_RECEIVER"].ToString().ToLower() == pPayerReceiver.ToString().ToLower())
                        {
                            sb = new StringBuilder();
                            sb.Append(row["SIXML"].ToString());
                            break;
                        }
                    }
                }
            }
            if (null != sb)
                DisplayXml("EventSi_XML", "EventSi_" + pPayerReceiver + "_" + m_ParamIdE.Value, sb.ToString());

        }
        #endregion DisplayEventSiXml
        #region GenerateHtmlForm
        protected override void GenerateHtmlForm()
        {

            base.GenerateHtmlForm();
            Form.ID = "frmEventSi";
            PlaceHolder plhEventSi = new PlaceHolder
            {
                ID = "plhEventSi"
            };
            CellForm.Controls.Add(plhEventSi);

        }
        #endregion GenerateHtmlForm
        #region GetSelectEventSi
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        private void GetSelectEventSi()
        {
            string XMLRefEventSi = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTSI");
            ReferentialTools.DeserializeXML_ForModeRO(XMLRefEventSi, out ReferentialsReferential refEventSi);

            SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventSi, "e.IDE=@IDE");
            QueryParameters query = SQLReferentialData.GetSQLSelect(sqlSelectParameters);
            query.Parameters.Add(m_ParamIdE);

            m_DsEventSi = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
        }
        #endregion GetSelectEventSi
        #region InitializeComponent
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.OnLoad);
        }
        #endregion InitializeComponent
        #region PageConstruction
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void PageConstruction()
        {
            GenerateHtmlForm();
            string title = Ressource.GetString("settlementInformations", true);
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
            PageTools.BuildPage(this, Form, PageFullTitle, cssMode, false, null, idMenu);
        }
        #endregion PageConstruction
        #region TitleEventSi
        private Table TitleEventSi(DataRow pRow)
        {
            Table tblTitle = new Table
            {
                CssClass = "DataGrid",
                CellSpacing = 0,
                CellPadding = 1,
                Width = Unit.Percentage(100)
            };


            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_HeaderStyle"
            };
            tr.Cells.Add(TableTools.AddHeaderCell("Trade", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Instrument", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("EventDates", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("EventCode", true, 0, UnitEnum.Pixel, 2, true));
            tr.Cells.Add(TableTools.AddHeaderCell("StartPeriod", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("EndPeriod", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("Valorisation", true, 0, UnitEnum.Pixel, 2, true));
            tblTitle.Rows.Add(tr);

            tr = new TableRow
            {
                CssClass = "DataGrid_ItemStyle"
            };
            tr.Cells.Add(TableTools.AddCell(pRow["TRADE_IDENTIFIER"].ToString() + " [" + pRow["IDT"].ToString() + "]",
                HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(pRow["INSTR_IDENTIFIER"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(pRow["DTEVENT"]).ToShortDateString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(pRow["EVENTCODE"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(pRow["EVENTTYPE"].ToString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(pRow["DTSTARTADJ"]).ToShortDateString(), HorizontalAlign.Center));
            tr.Cells.Add(TableTools.AddCell(Convert.ToDateTime(pRow["DTENDADJ"]).ToShortDateString(), HorizontalAlign.Center));
            //
            Decimal amount = Convert.ToDecimal(pRow["VALORISATION"] is DBNull ? 0 : pRow["VALORISATION"]);
            string currency = pRow["UNIT"].ToString();
            EFS_Round efs_Round = new EFS_Round(SessionTools.CS, currency, amount);

            tr.Cells.Add(TableTools.AddCell(StrFunc.FmtDecimalToCurrentCulture(efs_Round.AmountRounded.ToString(NumberFormatInfo.InvariantInfo)), HorizontalAlign.Right));
            tr.Cells.Add(TableTools.AddCell(currency, HorizontalAlign.Center));
            //
            tblTitle.Rows.Add(tr);
            return tblTitle;

        }
        #endregion TitleEventSi
        #region TransformEventSi
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        private string TransformEventSi(DataRow pRow)
        {
            int idA_Css = Convert.ToInt32(pRow["IDA_CSS"].ToString());
            bool isBookEntry = SettlementTools.IsCssBookEntry(SessionTools.CS, idA_Css);

            StringBuilder sb = new StringBuilder();
            sb.Append(pRow["SIXML"].ToString());
            
            StrBuilder sbNoalias = new StrBuilder(XSLTTools.RemoveXmlnsAlias(sb));

            Hashtable param = new Hashtable
            {
                { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name },
                { "pGUID", m_ParentGUID },
                { "pIDE", m_ParamIdE.Value.ToString() },
                { "pPayerReceiver", pRow["PAYER_RECEIVER"].ToString() },
                { "pPayer", pRow["ACTORPAYER_IDENTIFIER"].ToString() },
                { "pBookPayer", pRow["BOOKPAYER_IDENTIFIER"].ToString() },
                { "pReceiver", pRow["ACTORRECEIVER_IDENTIFIER"].ToString() },
                { "pBookReceiver", pRow["BOOKRECEIVER_IDENTIFIER"].ToString() },
                { "pIsBookEntry", isBookEntry ? "1" : "0" },
                { "pIsDeliveryMessage", m_IsDeliveryMessage ? "1" : "0" }
            };

            if (StrFunc.IsEmpty(m_XSLEventSi))
            {
                // FI 20160804 [Migration TFS] 
                //m_XSLEventSi = ReferentialTools.GetObjectXSLFile(Cst.ListType.Event, "EventSi");
                SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, @"~\GUIOutput\Event\EventSi.xslt", ref m_XSLEventSi);    
            }
            
            return XSLTTools.TransformXml(sbNoalias.StringBuilder, m_XSLEventSi, param, null);
        }
        #endregion TransformEventSi
        #endregion Methods
    }
}
