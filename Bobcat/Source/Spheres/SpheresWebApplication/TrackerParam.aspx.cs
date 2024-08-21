#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Process;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TrackerParamPage.
    /// </summary>
    public partial class TrackerParamPage : PageBase
    {
        #region Members
        public Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> m_ServiceObserver;
        protected bool m_IsTrackerAlert;
        protected Int64 m_TrackerAlertProcess; 
        protected int m_RefreshTime;
        #endregion Members
        #region PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            AbortRessource = true;
            string title = Ressource.GetString("OnBoardTracker");
            string subTitle = Ressource.GetString("Parameters_Title");
            this.PageTitle = title;
            GenerateHtmlForm();
            HtmlPageTitle titleLeft = new HtmlPageTitle(title, subTitle);
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, IdMenu.GetIdMenu(IdMenu.Menu.Admin));
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, IdMenu.GetIdMenu(IdMenu.Menu.Admin));
        }
        #endregion PageConstruction
        #region GenerateHtmlForm
        protected override void GenerateHtmlForm()
        {
                base.GenerateHtmlForm();
                Form.ID = "TrackerParam";
                AddButton();
                CreateAndLoadPlaceHolder();
        }
        #endregion GenerateHtmlForm

        #region OnValidation
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void OnValidation(object sender, EventArgs e)
        {

            Control ctrl = FindControl("TXTREFRESHINTERVAL");
            if (null != ctrl)
                SessionTools.TrackerRefreshInterval = Convert.ToInt32(((TextBox)ctrl).Text);
            ctrl = FindControl("CHKREFRESHACTIVE");
            if (null != ctrl)
                SessionTools.IsTrackerRefreshActive = ((CheckBox)ctrl).Checked;
            ctrl = FindControl("TXTNBROWPERGROUP");
            if (null != ctrl)
                SessionTools.TrackerNbRowPerGroup = Convert.ToInt32(((TextBox)ctrl).Text);
            ctrl = FindControl("CHKTRACKERALERT");
            if (null != ctrl)
                SessionTools.IsTrackerAlert = ((CheckBox)ctrl).Checked;

            ctrl = FindControl("LSTPROCESS");
            if (null != ctrl)
            {
                if (ctrl is CheckBoxList chkList)
                {
                    Int64 powerProcess = 0;
                    foreach (ListItem item in chkList.Items)
                    {
                        if (item.Selected)
                        {
                            Cst.ProcessTypeEnum process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), item.Value, false);
                            int i = int.Parse(Enum.Format(typeof(Cst.ProcessTypeEnum), process, "d"));
                            powerProcess += Convert.ToInt64(Math.Pow(2, i));
                        }
                    }
                    SessionTools.TrackerAlertProcess = powerProcess;
                }
            }

            SessionTools.TrackerGroupFav = GetValueGroup("LSTGROUPFAV");
            SessionTools.TrackerGroupDetail = GetValueGroup("LSTGROUPDETAIL");

            ctrl = FindControl("chkHistoToDay");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "0D";

                ctrl = FindControl("chkHisto1D");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "1D";

                ctrl = FindControl("chkHisto7D");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "7D";

                ctrl = FindControl("chkHisto1M");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "1M";

                ctrl = FindControl("chkHisto3M");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "3M";

                ctrl = FindControl("chkHistoBeyond");
                if (null != ctrl && ((RadioButton)ctrl).Checked)
                    SessionTools.TrackerHistoric = "Beyond";

                StringBuilder sb = new StringBuilder();
                sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='RefreshOpener'>\n");
                sb.Append("window.opener._Refresh('','LoadParam');");
                sb.Append("self.close();");
                sb.Append("</SCRIPT>\n");

                if (!Page.ClientScript.IsClientScriptBlockRegistered("RefreshOpener"))
                    Page.ClientScript.RegisterClientScriptBlock(GetType(), "RefreshOpener", sb.ToString());
        }
        #endregion OnValidation

        #region GetValueGroup
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Int64 GetValueGroup(string pId)
        {
            Int64 powerProcess = 0;
            Control ctrl = FindControl(pId);
            if (null != ctrl)
            {
                if (ctrl is CheckBoxList chkList)
                {
                    foreach (ListItem item in chkList.Items)
                    {
                        if (item.Selected)
                        {
                            Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), item.Value, false);
                            string hexValue = Enum.Format(typeof(Cst.GroupTrackerEnum), group, "x");
                            powerProcess += int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber);
                        }
                    }
                }
            }
            return powerProcess;
        }
        #endregion GetValueGroup
        #region OnLoad
        protected void OnLoad(object sender, System.EventArgs e)
        {
            PageConstruction();

            AddInputHiddenDeFaultControlOnEnter();
            HiddenFieldDeFaultControlOnEnter.Value = "btnOk";
            if (false == IsPostBack)
            {
                try
                {
                    SetFocus(HiddenFieldDeFaultControlOnEnter.Value);
                }
                catch { }
            }
        }
        #endregion OnLoad
        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            m_RefreshTime = SessionTools.TrackerRefreshInterval;
            m_IsTrackerAlert = SessionTools.IsTrackerAlert;
            m_TrackerAlertProcess = SessionTools.TrackerAlertProcess;
            m_ServiceObserver = ProcessTools.CreateDicServiceProcess();
        }
        #endregion OnInit

        #region AddButton
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddButton()
        {
            Panel pnl = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " admin" };
            //Valider
            WCToolTipLinkButton btnOk = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-check", true);
            btnOk.Click += new EventHandler(this.OnValidation);
            pnl.Controls.Add(btnOk);
            //Annuler
            WCToolTipLinkButton btnCancel = ControlsTools.GetAwesomeButtonCancel(false);
            pnl.Controls.Add(btnCancel);
            CellForm.Controls.Add(pnl);
        }
        #endregion AddButton
        #region CreateAndLoadPlaceHolder
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateAndLoadPlaceHolder()
        {

            #region General
            PlaceHolder plhTracker = new PlaceHolder
            {
                ID = "plhTrackerParam"
            };
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode};

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " admin" };
            togglePanel.SetHeaderTitle(Ressource.GetString("DisplayGen"));

            Table table = new Table();
            TableRow tr = null;
            TableCell td = null;
            table.Width = Unit.Percentage(100);
            table.Height = Unit.Percentage(100);
            table.BorderStyle = BorderStyle.None;
            table.CellPadding = 4;
            table.CellSpacing = 0;
            #region RefreshInterval
            tr = new TableRow();
            td = new TableCell
            {
                Wrap = false,
                Text = Ressource.GetString("lblTrackerRefreshInterval")
            };
            tr.Cells.Add(td);
            td = new TableCell();
            //td.Width = Unit.Percentage(100);
            TextBox txtRefreshInterval = new TextBox
            {
                Text = SessionTools.TrackerRefreshInterval.ToString(),
                CssClass = "txtCaptureNumeric",
                ID = "TXTREFRESHINTERVAL"
            };
            txtRefreshInterval.TextChanged += new EventHandler(OnIntervalChange);
            txtRefreshInterval.AutoPostBack = true;
            td.Controls.Add(txtRefreshInterval);

            CheckBox chkRefreshActive = new CheckBox()
            {
                ID = "CHKREFRESHACTIVE",
                Text = "Actif",
                Enabled = (Convert.ToInt32(txtRefreshInterval.Text) > 0),
                Checked = SessionTools.IsTrackerRefreshActive && (Convert.ToInt32(txtRefreshInterval.Text) > 0)
            };
            td.Controls.Add(chkRefreshActive);

            tr.Cells.Add(td);
            AddGroupSelection(tr);

            table.Rows.Add(tr);
            #endregion RefreshInterval
            #region NbRowPerGroup
            tr = new TableRow();
            td = new TableCell
            {
                Wrap = false,
                Text = Ressource.GetString("lblTrackerNbRowPerGroup")
            };
            tr.Cells.Add(td);
            td = new TableCell();
            //td.Width = Unit.Percentage(100);
            TextBox txtNbRowPerGroup = new TextBox
            {
                Text = SessionTools.TrackerNbRowPerGroup.ToString(),
                CssClass = "txtCaptureNumeric",
                ID = "TXTNBROWPERGROUP"
            };
            td.Controls.Add(txtNbRowPerGroup);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            #endregion NbRowPerGroup
            AddRowHistoric(table);
            togglePanel.AddContent(table);
            pnlBody.Controls.Add(togglePanel);

            #region Alert response
            togglePanel = new WCTogglePanel() { CssClass = CSSMode + " admin" };
            togglePanel.SetHeaderTitle(Ressource.GetString("DisplayAlertTracker"));
            table = new Table
            {
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                BorderStyle = BorderStyle.None,
                CellPadding = 4,
                CellSpacing = 0
            };
            CheckBox chkTrackerAlert = new CheckBox
            {
                Checked = SessionTools.IsTrackerAlert,
                ID = "CHKTRACKERALERT",
                Text = Ressource.GetString("lblTrackerAlert"),
                TextAlign = TextAlign.Left,
                CssClass = "size5"
            };
            togglePanel.AddContentHeader(chkTrackerAlert, true);

            table = new Table
            {
                CssClass = "Standard"
            };
            tr = new TableRow();
            td = new TableCell
            {
                BorderStyle = BorderStyle.None,
                Wrap = false,
                CssClass = "Msg_Information",
                Text = Ressource.GetString("SelectAlertTrackerProcess")
            };
            tr.Cells.Add(td);
            table.Rows.Add(tr);

            tr = new TableRow();
            td = new TableCell();

            OptionGroupCheckBoxList lstProcess = ControlsTools.CreateControlCheckListProcess(m_ServiceObserver, m_TrackerAlertProcess);
            td.Controls.Add(lstProcess);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            togglePanel.AddContent(table);
            pnlBody.Controls.Add(togglePanel);
            #endregion Alert response

            plhTracker.Controls.Add(pnlBody);
            CellForm.Controls.Add(plhTracker);
            #endregion General
        }
        #endregion CreateAndLoadPlaceHolder

        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "TrackerParam";
            this.Load += new System.EventHandler(this.OnLoad);
        }
        #endregion InitializeComponent
        #region AddRowHistoric
        private void AddRowHistoric(Table pTable)
        {
            string hisToricValue = SessionTools.TrackerHistoric;
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                Wrap = false,
                Text = Ressource.GetString("TrackerHistoric")
            };
            tr.Cells.Add(td);
            td = new TableCell();

            RadioButton chkToday = new RadioButton
            {
                ID = "chkHistoToDay",
                Text = Ressource.GetString("TrackerToDay"),
                Checked = false,
                GroupName = "historic"
            };
            chkToday.Checked = ("0D" == hisToricValue);
            td.Controls.Add(chkToday);

            RadioButton chk1d = new RadioButton
            {
                ID = "chkHisto1D",
                Text = Ressource.GetString("Tracker1D"),
                Checked = false,
                GroupName = "historic"
            };
            chk1d.Checked = ("1D" == hisToricValue);
            td.Controls.Add(chk1d);

            RadioButton chk7d = new RadioButton
            {
                Text = Ressource.GetString("Tracker7D"),
                Checked = false,
                ID = "chkHisto7D",
                GroupName = "historic"
            };
            chk7d.Checked = ("7D" == hisToricValue);
            td.Controls.Add(chk7d);

            RadioButton chk1m = new RadioButton
            {
                Text = Ressource.GetString("Tracker1M"),
                Checked = false,
                ID = "chkHisto1M",
                GroupName = "historic"
            };
            chk1m.Checked = ("1M" == hisToricValue);
            td.Controls.Add(chk1m);

            RadioButton chk3m = new RadioButton
            {
                Text = Ressource.GetString("Tracker3M"),
                Checked = false,
                ID = "chkHisto3M",
                GroupName = "historic"
            };
            chk3m.Checked = ("3M" == hisToricValue);
            td.Controls.Add(chk3m);

            RadioButton chkBeyond = new RadioButton
            {
                Text = Ressource.GetString("TrackerBeyond"),
                Checked = false,
                ID = "chkHistoBeyond",
                GroupName = "historic"
            };
            chkBeyond.Checked = ("Beyond" == hisToricValue);
            td.Controls.Add(chkBeyond);

            tr.Cells.Add(td);
            pTable.Rows.Add(tr);
        }
        #endregion AddRowHistoric

        #region AddGroupSelection
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void AddGroupSelection(TableRow pTableRow)
        {
            TableCell td = new TableCell
            {
                Wrap = false,
                RowSpan = 3
            };
            Label lbl = new Label
            {
                CssClass = "paramGroupTracker",
                Text = "FAVORIS"
            };
            CheckBoxList lstGroup = ControlsTools.CreateCheckBoxListGroupTracker(SessionTools.TrackerGroupFav);
            lstGroup.ID = "LSTGROUPFAV";
            td.Controls.Add(lbl);
            td.Controls.Add(lstGroup);
            pTableRow.Cells.Add(td);

            td = new TableCell
            {
                Wrap = false,
                RowSpan = 3
            };
            lbl = new Label
            {
                CssClass = "paramGroupTracker",
                Text = "DETAIL"
            };
            lstGroup = ControlsTools.CreateCheckBoxListGroupTracker(SessionTools.TrackerGroupDetail);
            lstGroup.ID = "LSTGROUPDETAIL";
            td.Controls.Add(lbl);
            td.Controls.Add(lstGroup);

            pTableRow.Cells.Add(td);

        }
        #endregion AddGroupSelection


        private void OnIntervalChange(object sender, System.EventArgs e)
        {
            if (FindControl("CHKREFRESHACTIVE") is CheckBox ctrl)
            {
                int interval = Convert.ToInt32((sender as TextBox).Text);
                ctrl.Enabled = interval > 0;
                ctrl.Checked = SessionTools.IsTrackerRefreshActive && (interval > 0);
            }
        }

    }
}
