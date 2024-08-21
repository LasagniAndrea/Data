#region Using Directives
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Process;
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
	/// Description résumée de TrackerServiceObserver.
	/// </summary>
	public partial class TrackerServiceObserverPage : PageBase
    {
        #region Members
        public Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> m_ServiceObserver;
        #endregion Members
        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            PageConstruction();
        }
        #endregion OnInit
        #region PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            AbortRessource = true;
            string title = Ressource.GetString("ServicesObserver");
            string subTitle = Ressource.GetString("ServicesObserverMessage");
            this.PageTitle = title;

            GenerateHtmlForm();

            HtmlPageTitle titleLeft = new HtmlPageTitle(title, subTitle);
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, IdMenu.GetIdMenu(IdMenu.Menu.Admin));
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, IdMenu.GetIdMenu(IdMenu.Menu.Admin));
        }
        #endregion PageConstruction
        #region GenerateHtmlForm
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm();
			Form.ID="frmTrackerServiceObserver";
			AddButton();
            m_ServiceObserver = ProcessTools.CreateDicServiceProcess();

            PlaceHolder plhTracker = new PlaceHolder() { ID = "plhTrackerServiceObserver" };

            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode };
            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " admin" };
            togglePanel.SetHeaderTitle(Ressource.GetString("ServicesList"));
            OptionGroupCheckBoxList lstProcess = ControlsTools.CreateControlCheckListProcess(m_ServiceObserver,0);
            togglePanel.AddContent(lstProcess);
            pnlBody.Controls.Add(togglePanel);
            plhTracker.Controls.Add(pnlBody);
            CellForm.Controls.Add(plhTracker);
		}
		#endregion GenerateHtmlForm


		#region OnSendClick
        private void OnSendClick(object sender, EventArgs e)
        {
            Control ctrl = FindControl("LSTPROCESS");
            if (null != ctrl)
            {
                int countMessage = 0;
                string serviceDisplay = string.Empty;
                  
                foreach (ListItem item in ((CheckBoxList)ctrl).Items)
                {
                    if (item.Selected)
                    {
                        Cst.ProcessTypeEnum processType = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), item.Value);

                        MQueueparameter paramObserver = new MQueueparameter(MQueueBase.PARAM_ISOBSERVER, TypeData.TypeDataEnum.@bool);
                        paramObserver.SetValue(true);

                        MQueueTaskInfo taskInfo = new MQueueTaskInfo
                        {
                            process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), item.Value),
                            connectionString = SessionTools.CS,
                            Session = SessionTools.AppSession,
                            trackerAttrib = new TrackerAttributes()
                            {
                                process = processType,
                                caller = MQueueBase.PARAM_ISOBSERVER
                            },
                            
                            idInfo = new IdInfo[1] { new IdInfo() },
                            mQueueParameters = new MQueueparameters()
                            {
                                parameter = new MQueueparameter[] { paramObserver }
                            }
                        };
                        taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.idInfo[0]);

                        MQueueTaskInfo.SendMultiple(taskInfo);
                        
                        serviceDisplay += item.Text + Cst.CrLf;
                        countMessage++;
                    }
                }
                if (0 < countMessage)
                {
                    string RessourceName = "Msg_SERVICEOBSERVER_GENERATE_" + ((1 == countMessage) ? "MONO" : "MULTI");
                    JavaScript.DialogImmediate(this, Ressource.GetString2(RessourceName, countMessage.ToString(), serviceDisplay));
                }
                else
                    JavaScript.DialogImmediate((PageBase)this, Ressource.GetString("Msg_ServicesNoneSelected"), false);
            }
        }
		#endregion OnSendClick
		#region OnLoad
		protected void OnLoad(object sender, System.EventArgs e)
		{
		}
        #endregion OnLoad

        #region AddButton
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddButton()
        {
            Panel pnl = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " admin" };
            //Valider
            WCToolTipLinkButton btnOk = ControlsTools.GetAwesomeButtonAction("btnSend", "fa fa-caret-square-right", true);
            btnOk.Click += new EventHandler(this.OnSendClick);
            pnl.Controls.Add(btnOk);

            WCToolTipPanel pnlMessage = new WCToolTipPanel() { CssClass = "fa-icon fa fa-info-circle" };
            pnl.Controls.Add(pnlMessage);

            Label lblMessage = new Label() 
            {
                CssClass = EFSCssClass.Msg_Information,
                Text = Ressource.GetString("Msg_SelectedServicesObserver")
            };
            pnl.Controls.Add(lblMessage);

            CellForm.Controls.Add(pnl);
        }
        #endregion AddButton
        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
		{    
			this.ID = "frmTrackerServiceObserver";
			this.Load += new System.EventHandler(this.OnLoad);
		}
		#endregion InitializeComponent
		
	}
}
