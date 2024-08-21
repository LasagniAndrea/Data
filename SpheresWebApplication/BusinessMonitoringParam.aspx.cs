#region Using Directives
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de BusinessMonitoringParamPage.
    /// </summary>
    public partial class BusinessMonitoringParamPage : PageBase
    {
        #region Members
        public Dictionary<Pair<MonitoringTools.MonitoringGroupEnum,string>, List<Pair<MonitoringTools.MonitoringElementEnum, string>>> m_DicMonitoringElement;
        protected Int64 m_MonitoringObserverElement; 
        protected int m_RefreshTime;
        #endregion Members
        #region PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            AbortRessource = true;
            string title = Ressource.GetString("OnBoardMonitoring");
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
                Form.ID = "BusinessMonitoringParam";
                AddButton();
                AddBody();
        }
        #endregion GenerateHtmlForm

        #region OnValidation
        private void OnValidation(object sender, EventArgs e)
        {

                Control ctrl = FindControl("TXTREFRESHINTERVAL");
                if (null != ctrl)
                    SessionTools.MonitoringRefreshInterval = Convert.ToInt32(((TextBox)ctrl).Text);

                ctrl = FindControl("LSTMONITORINGELEMENT");
                if (null != ctrl)
                {
                if (ctrl is CheckBoxList chkList)
                {
                    Int64 powerElement = 0;
                    foreach (ListItem item in chkList.Items)
                    {
                        if (item.Selected)
                        {
                            MonitoringTools.MonitoringElementEnum element =
                                (MonitoringTools.MonitoringElementEnum)Enum.Parse(typeof(MonitoringTools.MonitoringElementEnum), item.Value, false);
                            int i = int.Parse(Enum.Format(typeof(MonitoringTools.MonitoringElementEnum), element, "d"));
                            powerElement += Convert.ToInt64(Math.Pow(2, i));
                        }
                    }
                    SessionTools.MonitoringObserverElement = powerElement;
                }
            }
                StringBuilder sb = new StringBuilder();
                sb.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='RefreshOpener'>\n");
                sb.Append("window.opener.Refresh('','LoadParam');");
                sb.Append("self.close();");
                sb.Append("</SCRIPT>\n");

                if (!Page.ClientScript.IsClientScriptBlockRegistered("RefreshOpener"))
                    Page.ClientScript.RegisterClientScriptBlock(GetType(), "RefreshOpener", sb.ToString());
        }
        #endregion OnValidation
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
            m_RefreshTime = SessionTools.MonitoringRefreshInterval;
            m_MonitoringObserverElement = SessionTools.MonitoringObserverElement;
            m_DicMonitoringElement = MonitoringTools.CreateDicMonitoringElement();
        }
        #endregion OnInit

        #region AddButton
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
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
        #region CreateControlMonitoring
        #endregion CreateControlProcess
        #region AddBody
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) 
        private void AddBody()
        {
            #region General
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " repository" };
            WCTogglePanel pnl = new WCTogglePanel();
            pnl.SetHeaderTitle(Ressource.GetString("DisplayGen"));
            pnl.CssClass = this.CSSMode + " red";
            Table table = new Table
            {
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                BorderStyle = BorderStyle.None,
                CellPadding = 4,
                CellSpacing = 0
            };
            #region RefreshInterval
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                Wrap = false,
                Text = Ressource.GetString("lblREFRESHINTERVAL")
            };
            tr.Cells.Add(td);
            td = new TableCell
            {
                Width = Unit.Percentage(100)
            };
            TextBox txtRefreshInterval = new TextBox
            {
                Text = SessionTools.MonitoringRefreshInterval.ToString(),
                CssClass = "txtCaptureNumeric",
                ID = "TXTREFRESHINTERVAL"
            };
            td.Controls.Add(txtRefreshInterval);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            #endregion RefreshInterval
            pnl.AddContent(table);
            pnlBody.Controls.Add(pnl);

            #region Alert response
            pnl = new WCTogglePanel();
            pnl.SetHeaderTitle(Ressource.GetString("DisplayMonitoringObserverElement"));
            pnl.CssClass = this.CSSMode + " red";

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
                Text = Ressource.GetString("SelectMonitoringObserverElement")
            };
            tr.Cells.Add(td);
            table.Rows.Add(tr);

            tr = new TableRow();
            td = new TableCell();

            OptionGroupCheckBoxList lstElement = 
                MonitoringTools.CreateControlCheckListMonitoringElement(m_DicMonitoringElement, SessionTools.MonitoringObserverElement);
            td.Controls.Add(lstElement);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            pnl.AddContent(table);
            pnlBody.Controls.Add(pnl);

            #endregion Alert response
            CellForm.Controls.Add(pnlBody);
            #endregion General
        }
        #endregion AddBody

        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "MonitoringParam";
            this.Load += new System.EventHandler(this.OnLoad);
        }
        #endregion InitializeComponent
    }
}
