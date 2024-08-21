using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    // EG 20200928 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
    public partial class BusinessMonitoringPage : PageBase
    {
        protected string _mainMenuClassName;
        // EG 20160404 Migration vs2013
        /// <summary>
        /// Script de gestion du dernier focus
        /// </summary>
        //private const string SCRIPT_DOFOCUS = @"window.setTimeout('DoFocus()', 1);function DoFocus(){try {document.getElementById('REQUEST_LASTFOCUS').focus();} catch (ex) {}}";

        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="e"></param>
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            Form = frmBusinessMonitoring;
            AntiForgeryControl();
        }

        // EG 20180525 [23979] IRQ Processing
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et compléments
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        protected void Page_Load(object sender, EventArgs e)
        {          
            
            Page.ClientScript.RegisterStartupScript(typeof(BusinessMonitoringPage), "ScriptDoFocus", SCRIPT_DOFOCUS.Replace("REQUEST_LASTFOCUS", Request["__LASTFOCUS"]), true);

            // set header
            string idMenu = Request.QueryString["IdMenu"];
            _mainMenuClassName = ControlsTools.MainMenuName(idMenu);
            string leftTitle = Ressource.GetString(idMenu, true);
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            Control head;
            if (null != this.Header)
                head = (Control)this.Header;
            else
                head = (Control)PageTools.SearchHeadControl(this);

            PageTools.SetHeaderLink(head, "linkCssAwesome", "~/Includes/fontawesome-all.min.css");
            PageTools.SetHeaderLink(head, "linkCssCommon", "~/Includes/EFSThemeCommon.min.css");
            PageTools.SetHeaderLink(head, "linkCssUISprites", "~/Includes/EFSUISprites.min.css");

            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, idMenu));
            plhHeader.Controls.Add(pnlHeader);


            Table tbl = new Table
            {
                ID = "tblMonitoringParameters",
                CellPadding = 3,
                CellSpacing = 0
            };

            TableRow tr = new TableRow();
            TableCell td = new TableCell();
            td.Style.Add(HtmlTextWriterStyle.PaddingTop, "3px");
            td.Style.Add(HtmlTextWriterStyle.Padding, "4px");
            td.HorizontalAlign = HorizontalAlign.Left;

            Label lbDtMonitoring = new Label
            {
                CssClass = EFSCssClass.Label
            };
            TableTools.WriteSpaceInCell(td);
            lbDtMonitoring.Text = "Data di monitoring";
            td.Controls.Add(lbDtMonitoring);

            Label dtMonitoring = new Label
            {
                CssClass = EFSCssClass.LabelDisplayForeignKey
            };
            TableTools.WriteSpaceInCell(td);
            DateTime dtSys = DateTime.Now;
            dtMonitoring.Text = DtFunc.DateTimeToString(dtSys, DtFunc.FmtDateLongTime);
            td.Controls.Add(dtMonitoring);

            Label lbEntity = new Label
            {
                CssClass = EFSCssClass.Label
            };
            TableTools.WriteSpaceInCell(td);
            TableTools.WriteSpaceInCell(td);
            TableTools.WriteSpaceInCell(td);
            lbEntity.Text = Ressource.GetString("Entity_Title");
            td.Controls.Add(lbEntity);

            TextBox entity = new TextBox
            {
                CssClass = EFSCssClass.LabelDisplayForeignKey
            };
            TableTools.WriteSpaceInCell(td);
            entity.Text = GetEntity();
            td.Controls.Add(entity);

            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Attributes.Add("onclick", "__doPostBack('0','SELFRELOAD_');");
            TableTools.WriteSpaceInCell(td);
            TableTools.WriteSpaceInCell(td);
            TableTools.WriteSpaceInCell(td);
            td.Controls.Add(btnRefresh);

            tr.Controls.Add(td);
            tbl.Controls.Add(tr);

            WCBodyPanel bodyPanel = ControlsTools.CreateBodyPanel(tbl, CSSModeEnum, _mainMenuClassName);
            bodyPanel.AddContent(tbl);
            pnlParameters.Controls.Add(bodyPanel);

            DateTime maxTimeFromQuoteTable = GetMaxTimeFromQuoteTable();
            Int32 maxIdCBRequest = GetMaxIdCBRequest();

            DataTable BusinessContent = DtIOInterfaces("IO", "INPUT", "Inputs");
            if (null != BusinessContent)
            {
                GridView Inputs = DrawMonitorGridView(BusinessContent);
                gvInputs.Controls.Add(Inputs);
            }

            BusinessContent = DtClearingParameters(maxTimeFromQuoteTable);
            if (null != BusinessContent)
            {
                GridView ClearingParameters = DrawMonitorGridView(BusinessContent);
                gvClearingParameters.Controls.Add(ClearingParameters);
            }

            BusinessContent = DtIOInterfaces("IO", "OUTPUT", "Outputs");
            if (null != BusinessContent)
            {
                GridView Outputs = DrawMonitorGridView(BusinessContent);
                gvOutputs.Controls.Add(Outputs);
            }

            BusinessContent = DtEndOfDay("CLO", "POSREQUEST");
            if (null != BusinessContent)
            {
                GridView EndOfDay = DrawMonitorGridView(BusinessContent);
                gvEndOfDay.Controls.Add(EndOfDay);
            }

            BusinessContent = DtCashBalance(maxIdCBRequest);
            if (null != BusinessContent)
            {
                GridView CashBalance = DrawMonitorGridView(BusinessContent);
                gvCashBalance.Controls.Add(CashBalance);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public string GetNow()
        {
            string ret = DateTime.Now.ToString();
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetEntity()
        {
            string CS = SessionTools.CS;
            string entity = null;
            //
            IDataReader dr = null;
            try
            {
                StrBuilder query = new StrBuilder();
                if (DataHelper.IsDbSqlServer(CS))
                    query += SQLCst.SELECT_TOP1 + " a.DISPLAYNAME as DISPLAYNAME " + Cst.CrLf;
                else
                    query += SQLCst.SELECT + " a.DISPLAYNAME as DISPLAYNAME " + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a " + Cst.CrLf;
                query += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTORROLE.ToString() + " ar " + SQLCst.ON + " ar.IDA = a.IDA " + Cst.CrLf;
                query += SQLCst.WHERE + " ar.IDROLEACTOR = 'ENTITY' " + Cst.CrLf;
                if (DataHelper.IsDbOracle(CS))
                    query += SQLCst.AND + " ROWNUM = 1 " + Cst.CrLf;
                dr = DataHelper.ExecuteReader(CS, CommandType.Text, query.ToString(), null);

                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DISPLAYNAME"]))
                    {
                        entity = Convert.ToString((dr["DISPLAYNAME"]));
                    }
                }
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
            return entity;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DateTime GetMaxTimeFromQuoteTable()
        {
            string CS = SessionTools.CS;
            DateTime ret = DateTime.MinValue;

            StrBuilder query = new StrBuilder();
            query += SQLCst.SELECT + SQLCst.MAX + " (TIME) as TIME " + Cst.CrLf;
            query += SQLCst.FROM_DBO + Cst.OTCml_TBL.QUOTE_ETD_H.ToString() + Cst.CrLf;
            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, query.ToString(), null))
            {
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["TIME"]))
                    {
                        ret = Convert.ToDateTime((dr["TIME"]));
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Int32 GetMaxIdCBRequest()
        {
            string CS = SessionTools.CS;
            Int32 ret = 0;
            //
            IDataReader dr = null;
            try
            {
                StrBuilder query = new StrBuilder();
                query += SQLCst.SELECT + SQLCst.MAX + " (IDCBREQUEST) as IDCBREQUEST " + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST.ToString() + Cst.CrLf;
                query += SQLCst.WHERE + " IDA_CBO is null " + Cst.CrLf;
                dr = DataHelper.ExecuteReader(CS, CommandType.Text, query.ToString(), null);

                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["IDCBREQUEST"]))
                    {
                        ret = Convert.ToInt32((dr["IDCBREQUEST"]));
                    }
                }
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetBusinessDateFromCBRequest(Int32 pIdCBRequest)
        {
            string CS = SessionTools.CS;
            string ret = null;
            //
            IDataReader dr = null;
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, "IDCBREQUEST", DbType.Int32), pIdCBRequest);
                StrBuilder query = new StrBuilder();
                query += SQLCst.SELECT + " DTBUSINESS as DTBUSINESS " + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST.ToString() + Cst.CrLf;
                query += SQLCst.WHERE + " IDCBREQUEST = @IDCBREQUEST " + Cst.CrLf;
                dr = DataHelper.ExecuteReader(CS, CommandType.Text, query.ToString(), parameters.GetArrayDbParameter());

                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr["DTBUSINESS"]))
                    {
                        ret = Convert.ToString((dr["DTBUSINESS"]));
                    }
                }
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pDataSource"></param>
        /// <returns></returns>
        private GridView DrawMonitorGridView(DataTable pDataSource)
        {

            GridView Grid = new GridView
            {
                CellSpacing = 0,
                CellPadding = 4,

                CssClass = "DataGrid"
            };
            // using initial margin consultation class
            Grid.RowStyle.CssClass = "DataGrid_FlatHouse";
            // using standard consultation class
            //Grid.RowStyle.CssClass = "DataGrid_ItemStyle";
            //Grid.AlternatingRowStyle.CssClass = "DataGrid_AlternatingItemStyle";
            Grid.AlternatingRowStyle.Wrap = false;
            Grid.RowStyle.Wrap = false;
            Grid.SelectedRowStyle.CssClass = "DataGrid_SelectedItemStyle";
            Grid.SelectedRowStyle.Wrap = false;
            Grid.HeaderStyle.CssClass = "DataGrid_HeaderStyle";
            Grid.HeaderStyle.Wrap = false;
            Grid.HeaderStyle.Font.Size = 8;
            Grid.RowDataBound += new GridViewRowEventHandler(Grid_RowDataBound);
            Grid.DataSource = pDataSource;
            Grid.DataBind();

            return Grid;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Grid_RowDataBound(Object sender, GridViewRowEventArgs e)
        {

            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                int index = GetColumnIndexByName(e.Row, "Cassa di compensazione");
                if (index != 99)
                {
                    e.Row.Cells[index].Width = new Unit("250px");
                }

                index = GetColumnIndexByName(e.Row, "Entità");
                if (index != 99)
                {
                    e.Row.Cells[index].Width = new Unit("250px");
                }


                // temporary condiction 
                if (e.Row.Cells[1].Text == "ERROR")
                    e.Row.Cells[1].ForeColor = System.Drawing.Color.Red;
                if (e.Row.Cells[1].Text == "SUCCESS")
                    e.Row.Cells[1].ForeColor = System.Drawing.Color.Green;
                if (e.Row.Cells[1].Text == "WARNING")
                    e.Row.Cells[1].ForeColor = System.Drawing.Color.Orange;

                if (e.Row.Cells[2].Text == "ERROR")
                    e.Row.Cells[2].ForeColor = System.Drawing.Color.Red;
                if (e.Row.Cells[2].Text == "SUCCESS")
                    e.Row.Cells[2].ForeColor = System.Drawing.Color.Green;
                if (e.Row.Cells[2].Text == "WARNING")
                    e.Row.Cells[2].ForeColor = System.Drawing.Color.Orange;

                // testing condiction
                // Display Status cell with status color
                //index = GetColumnIndexByName(e.Row, "Status");
                //if (index != 99)
                //{
                //    if (e.Row.Cells[index].Text == "ERROR")
                //        e.Row.Cells[index].ForeColor = System.Drawing.Color.Red;
                //    if (e.Row.Cells[index].Text == "SUCCESS")
                //        e.Row.Cells[index].ForeColor = System.Drawing.Color.Green;
                //    if (e.Row.Cells[index].Text == "WARNING")
                //        e.Row.Cells[index].ForeColor = System.Drawing.Color.Orange;
                //}
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        int GetColumnIndexByName(GridViewRow row, string columnName)
        {
            int columnIndex = 0;
            foreach (DataControlFieldCell cell in row.Cells)
            {
                if (cell.ContainingField is BoundField field)
                    if (field.DataField.Equals(columnName))
                        break;
                // keep 99 index we don't have the correct name
                columnIndex = 99;
            }
            return columnIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGroupTracker"></param>
        /// <param name="pInOut"></param>
        /// <param name="pIdMonitoringWindows"></param>
        /// <returns></returns>
        private DataTable DtIOInterfaces(string pGroupTracker, string pInOut, string pIdMonitoringWindows)
        {
            string CS = SessionTools.CS;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "GROUPTRACKER", DbType.String), pGroupTracker);
            parameters.Add(new DataParameter(CS, "IN_OUT", DbType.String), pInOut);
            parameters.Add(new DataParameter(CS, "IDMONITORINGWINDOW", DbType.String), pIdMonitoringWindows);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "case when task.DISPLAYNAME = ioel.DISPLAYNAME then task.DISPLAYNAME else task.DISPLAYNAME + ' - ' + ioel.DISPLAYNAME end as TASK, " + Cst.CrLf;
            sqlSelect += "case when iotrack.IDIOTRACK is not null then iotrack.STATUSRETURN else tracker.STATUSTRACKER end as STATUSTRACKER," + Cst.CrLf;
            sqlSelect += "case when iotrack.IDIOTRACK is not null then iotrack.DTINS else case when tracker.STATUSTRACKER = 'PROGRESS' then tracker.DTINS else tracker.DTUPD end end as DTSTATUS, " + Cst.CrLf;
            sqlSelect += "iotrack.DATA1 as DATA1, iotrack.DATA2 as DATA2, iotrack.DATA3 as DATA3, iotrack.DATA4 as DATA4, iotrack.DATA5 as DATA5 " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " task " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EXTLID.ToString() + " ex " + SQLCst.ON + " ex.ID = task.IDIOTASK " + SQLCst.AND + " ex.TABLENAME='IOTASK' " + SQLCst.AND + " ex.IDENTIFIER = 'IDMONITORINGWINDOW' " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOTASKDET.ToString() + " taskdet " + SQLCst.ON + " taskdet.IDIOTASK = task.IDIOTASK " + Cst.CrLf;
            if (pInOut == "INPUT")
            {
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOINPUT.ToString() + " ioel " + SQLCst.ON + " ioel.IDIOINPUT = taskdet.IDIOELEMENT " + Cst.CrLf;
            }
            else if (pInOut == "OUTPUT")
            {
                sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.IOOUTPUT.ToString() + " ioel " + SQLCst.ON + " ioel.IDIOOUTPUT = taskdet.IDIOELEMENT " + Cst.CrLf;
            }
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EXTLID.ToString() + " ex2 " + SQLCst.ON + " ex2.ID = taskdet.IDIOTASKDET " + SQLCst.AND + " ex2.TABLENAME='IOTASKDET' " + SQLCst.AND + " ex2.IDENTIFIER = 'ISMONITORED' " + Cst.CrLf;
            sqlSelect += "left outer join ( " + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + SQLCst.MAX + " (DTINS) as DTINS, " + SQLCst.MAX + "(IDTRK_L) as IDTRK_L, " + "IDDATAIDENT, IDDATAIDENTIFIER " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " GROUPTRACKER=@GROUPTRACKER " + Cst.CrLf;
            sqlSelect += SQLCst.AND + " DATA2=@IN_OUT" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "IDDATAIDENT, IDDATAIDENTIFIER ) gtracker " + SQLCst.ON + " gtracker.IDDATAIDENTIFIER = task.IDENTIFIER " + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + " tracker " + SQLCst.ON + " tracker.IDTRK_L = gtracker.IDTRK_L " + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.PROCESS_L.ToString() + " procs " + SQLCst.ON + " procs.IDTRK_L = gtracker.IDTRK_L " + Cst.CrLf;
            sqlSelect += SQLCst.LEFTJOIN_DBO + Cst.OTCml_TBL.IOTRACK.ToString() + " iotrack " + SQLCst.ON + " iotrack.IDPROCESS_L = procs.IDPROCESS_L and iotrack.IDIOTASKDET = taskdet.IDIOTASKDET " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + "task.IN_OUT = @IN_OUT" + Cst.CrLf;
            sqlSelect += SQLCst.AND + " ex.VALUE=@IDMONITORINGWINDOW" + Cst.CrLf;
            sqlSelect += SQLCst.AND + " ex2.VALUE='true'" + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];

            dt.Columns["TASK"].ColumnName = "Task";
            dt.Columns["STATUSTRACKER"].ColumnName = "Status";
            dt.Columns["DTSTATUS"].ColumnName = "Data/ora status";

            if (pInOut == "INPUT")
            {
                dt.Columns["DATA1"].ColumnName = "Linee ignorate";
                dt.Columns["DATA2"].ColumnName = "Linee scartate";
                dt.Columns["DATA3"].ColumnName = "Records inseriti";
                dt.Columns["DATA4"].ColumnName = "Records modificati";
                dt.Columns["DATA5"].ColumnName = "Records rimossi";
            }

            else if (pInOut == "OUTPUT")
            {
                dt.Columns["DATA1"].ColumnName = "Linee lette nel db";      // Read rows from database
                dt.Columns["DATA2"].ColumnName = "Linee scartate";   // Rejected rows by data controls
                dt.Columns["DATA3"].ColumnName = "Linee ignorate";   // Ignored rows by setting
                dt.Columns["DATA4"].ColumnName = "Linee scritte";    // Written rows to destination
                dt.Columns["DATA5"].ColumnName = "Files esportati";  // Exported files
            }

            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private DataTable DtClearingParameters(DateTime pMaxTime)
        {
            string CS = SessionTools.CS;
            string BusinessDateShortFormat = DtFunc.DateTimeToString(pMaxTime, DtFunc.FmtShortDate);

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "MAXTIME", DbType.DateTime), pMaxTime);
            parameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.String), BusinessDateShortFormat);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "ClearingHouse, @DTBUSINESS as DtBusiness, case when QuotationCount > 0 then 'SUCCESS' else null end as Status, StatusDate, QuotationCount" + Cst.CrLf;
            sqlSelect += "from ( " + Cst.CrLf;
            sqlSelect += "select a.IDENTIFIER as ClearingHouse, max( q.DTINS) as StatusDate, count(*) as QuotationCount " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.QUOTE_ETD_H.ToString() + " q " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARKET.ToString() + " m " + SQLCst.ON + " m.IDM = q.IDM " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.CSS.ToString() + " css " + SQLCst.ON + " css.IDA = m.IDA " + Cst.CrLf;
            sqlSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a " + SQLCst.ON + " a.IDA = css.IDA " + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " q.TIME = @MAXTIME " + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + " q.TIME, css.IDA, a.IDENTIFIER ) CH " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];

            dt.Columns["ClearingHouse"].ColumnName = "Cassa di compensazione";
            dt.Columns["DtBusiness"].ColumnName = "Data di compensazione";
            dt.Columns["Status"].ColumnName = "Status";
            dt.Columns["StatusDate"].ColumnName = "Data/ora status";
            dt.Columns["QuotationCount"].ColumnName = "Quotazioni importate";

            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGroupTracker"></param>
        /// <param name="pIdDataIdent"></param>
        /// <returns></returns>
        private DataTable DtEndOfDay(string pGroupTracker, string pIdDataIdent)
        {
            string CS = SessionTools.CS;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "GROUPTRACKER", DbType.String), pGroupTracker);
            parameters.Add(new DataParameter(CS, "IDDATAIDENT", DbType.String), pIdDataIdent);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "tr.DATA2 as CLEARINGHOUSE, " + Cst.CrLf;
            sqlSelect += " substring(tr.DATA3,1,10) as CLEARINGDATE, " + Cst.CrLf;
            sqlSelect += "tr.STATUSTRACKER as STATUSTRACKER, case tr.STATUSTRACKER when 'PROGRESS' then tr.DTINS else tr.DTUPD end as DTSTATUS" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + " tr " + Cst.CrLf;
            sqlSelect += "inner join ( " + Cst.CrLf;
            sqlSelect += SQLCst.SELECT + SQLCst.MAX + " (DTINS) as DTINS, " + SQLCst.MAX + "(IDTRK_L) as IDTRK_L, " + SQLCst.MAX + " (DATA3) as CLEARINGDATE, DATA1,DATA2 " + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " GROUPTRACKER=@GROUPTRACKER " + Cst.CrLf;
            sqlSelect += SQLCst.AND + " IDDATAIDENT=@IDDATAIDENT" + Cst.CrLf;
            sqlSelect += SQLCst.GROUPBY + "DATA1, DATA2 ) tr2 " + SQLCst.ON + " tr2.IDTRK_L = tr.IDTRK_L " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["CLEARINGDATE"] = DtFunc.DateTimeToString(Convert.ToDateTime(dt.Rows[i]["CLEARINGDATE"]), DtFunc.FmtShortDate); ;
            }
            dt.AcceptChanges();

            dt.Columns["CLEARINGHOUSE"].ColumnName = "Cassa di compensazione";
            dt.Columns["CLEARINGDATE"].ColumnName = "Data di compensazione";
            dt.Columns["STATUSTRACKER"].ColumnName = "Status";
            dt.Columns["DTSTATUS"].ColumnName = "Data/ora status";

            return dt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pGroupTracker"></param>
        /// <param name="pData"></param>
        /// <returns></returns>
        private DataTable DtCashBalance(Int32 pMaxIdCBRequest)
        {
            string CS = SessionTools.CS;
            string businessDate = DtFunc.DateTimeToString(Convert.ToDateTime(GetBusinessDateFromCBRequest(pMaxIdCBRequest)), DtFunc.FmtShortDate);

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDCBREQUEST", DbType.Int32), pMaxIdCBRequest);
            parameters.Add(new DataParameter(CS, "DTBUSINESS", DbType.String), businessDate);
            StrBuilder sqlSelect = new StrBuilder();
            sqlSelect += SQLCst.SELECT + "@DTBUSINESS as DTBUSINESS, STATUS, DTEND" + Cst.CrLf;
            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.CBREQUEST.ToString() + Cst.CrLf;
            sqlSelect += SQLCst.WHERE + " IDCBREQUEST = @IDCBREQUEST " + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, sqlSelect.ToString(), parameters.GetArrayDbParameter());
            DataTable dt = ds.Tables[0];

            dt.Columns["DTBUSINESS"].ColumnName = "Data di compensazione";
            dt.Columns["STATUS"].ColumnName = "Status";
            dt.Columns["DTEND"].ColumnName = "Data/ora status";

            return dt;
        }
    }
}
