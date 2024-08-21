using EFS.ACommon;
using EFS.Common.Web;
using System;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;



namespace EFS.Spheres
{
    #region SelProductPage
    /// <summary>
    /// Description résumée de SelProduct.
    /// </summary>
    // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public partial class SelProductPage : PageBase
	{
		#region Members
        /// <summary>
        /// 
        /// </summary>
        protected SelCommonProduct m_SelCommonProduct;
        protected string _mainMenuClassName;

        #endregion Members

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		// EG 20210222 [XXXXX] Suppression inscription function PostBack et OnCtrlChanged
        protected void Page_Load(object sender, System.EventArgs e)
        {
            m_SelCommonProduct.Bind(Cst.EnumElement.Product);

            //JavaScript.PostBack((PageBase)this);
            //JavaScript.OnCtrlChanged((PageBase)this);
            JavaScript.DisplayDescription((PageBase)this);
            JavaScript.DisplayDescriptionStartUp((PageBase)this, m_SelCommonProduct.IsDescChecked);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();

            Cst.SQLCookieGrpElement grpElement = (Cst.SQLCookieGrpElement)Enum.Parse(typeof(Cst.SQLCookieGrpElement), Request.QueryString["GrpElement"], true);

            m_SelCommonProduct = new SelCommonProduct(this, grpElement, null, grpElement.ToString(), true, Cst.Capture.ModeEnum.New, false);

            base.OnInit(e);
            _mainMenuClassName = ControlsTools.MainMenuName(IdMenu.GetIdMenu(IdMenu.Menu.Input));
            PageConstruction();

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            UpdateSelectEnabled();
        }
        
		#endregion Events

		#region Methods
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private WCTogglePanel CreatePanel( Cst.EnumElement pElement)
        {
            m_SelCommonProduct.NumberOfSections++;
            WCTogglePanel togglePanel = new WCTogglePanel(Color.Transparent, pElement.ToString(), "size5", false) 
            {
                ID = "div-selprd-" + pElement.ToString(),
                CssClass = CSSMode + " " + _mainMenuClassName,
                Width = Unit.Percentage(100)
            };

            OptionGroupDropDownList ddl = new OptionGroupDropDownList()
            {
                EnableViewState = true,
                CssClass = "ddlCapture",
                ID = "lst" + pElement.ToString(),
                AutoPostBack = true
            };
            ddl.SelectedIndexChanged += new System.EventHandler(m_SelCommonProduct.OnSelectedElementChanged);
            ddl.Attributes.Add("onchange", "OnCtrlChanged('" + ddl.UniqueID + "','');");

            togglePanel.AddContent(ddl);

            Label lbl = new Label()
            {
                Text = string.Empty,
                ID = "lbl" + pElement.ToString() + "-displayname",
            };
            lbl.Font.Size = FontUnit.Larger;
            lbl.Attributes.Add("cursor", "hand");
            togglePanel.AddContent(lbl);

            if (m_SelCommonProduct.IsDescChecked)
            {
                Panel pnl = new Panel() { ID = "div" + pElement.ToString() + "-description" };
                TextBox txt = new TextBox() 
                {
                    ID = "lbl" + pElement.ToString() + "-description",
                    TabIndex = -1,
                    Text = string.Empty,
                    ReadOnly = true,
                    TextMode = TextBoxMode.MultiLine,
                };
                pnl.Controls.Add(txt);
                togglePanel.AddContent(pnl);
            }
            return togglePanel;
        }

        /// <summary>
        /// 
        /// </summary>
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmProduct";
            AddToolBar();
            AddBody();
        }

        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " +_mainMenuClassName };
            WCToolTipLinkButton btnOkRecord = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-check", true);
            btnOkRecord.Click += new EventHandler(m_SelCommonProduct.OnSelectClick);
            pnlToolBar.Controls.Add(btnOkRecord);
            
            WCToolTipLinkButton btnCancel = ControlsTools.GetAwesomeButtonCancel(true);
            pnlToolBar.Controls.Add(btnCancel);

            CheckBox chkDescription = new CheckBox()
            {
                Text = Ressource.GetString("Detail"),
                ID = "chkDescription",
                CssClass = "lblCaptureTitle",
                EnableViewState = true,
                Checked = m_SelCommonProduct.IsDescChecked
            };
            chkDescription.Attributes.Add("onclick", @"DisplayDescription(this.checked);");
            chkDescription.Style.Add(HtmlTextWriterStyle.PaddingLeft, "20px");
            pnlToolBar.Controls.Add(chkDescription);
            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + _mainMenuClassName };

           pnlBody.Controls.Add( CreatePanel(Cst.EnumElement.Product));
           pnlBody.Controls.Add(CreatePanel(Cst.EnumElement.Instrument));
           pnlBody.Controls.Add(CreatePanel(Cst.EnumElement.Template));
           pnlBody.Controls.Add(CreatePanel(Cst.EnumElement.Screen));
           CellForm.Controls.Add(pnlBody);
        }


        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void PageConstruction()
        {
            AbortRessource = true;
            PageTitle = Ressource.GetString("INSTRUMENTSELECT");
            HtmlPageTitle titleLeft = new HtmlPageTitle(Ressource.GetString("INSTRUMENTSELECT"));
            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), string.Empty, IdMenu.GetIdMenu(IdMenu.Menu.Input));
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, IdMenu.GetIdMenu(IdMenu.Menu.Input));

        }
        
        /// <summary>
        /// 
        /// </summary>
        private void UpdateSelectEnabled()
		{
            // Set Select button enabled/disabled
            if (FindControl("btnOk") is Button ddlSelect)
            {
                ddlSelect.Enabled = true; // Reset to true
                string[] names = Enum.GetNames(typeof(Cst.EnumElement));
                for (int i = 0; i < names.Length; i++)
                {
                    if (FindControl("lst" + names[i]) is DropDownList ddl)
                        ddlSelect.Enabled &= (0 < ddl.Items.Count);
                }
            }
        }
		
        #endregion Methods
    }
	#endregion
}

