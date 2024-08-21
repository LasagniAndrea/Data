
using System;
using System.Collections;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
//
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;




namespace EFS.GUI.CCI
{
    
    /// <summary>
    /// Description résumée de cciPageDesign.
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    public class CciPageDesign : CciPageBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// FI 20210621 [XXXXX] Add 
        protected  const string PostBackForValidationArg = "PostBackForValidation";

        /// <summary>
        /// 
        /// </summary>
        // FI 20200117 [25167] add
        public const string PrefixTableHID = "div";
        // FI 20240129 [WI830] add
        private int UniqueTableHID =1;

        #region Enums
        #region DisplayKeyEnum
        // EG 20110308 HPC Nb ligne de frais sur facture
        protected enum DisplayKeyEnum
        {
            DisplayKey_Instrument,
            DisplayKey_Event,
            DisplayKey_EventPosition,
            DisplayKey_InvoiceTrade,
            DisplayKey_InvoiceFee,
        }

        #endregion DisplayKeyEnum
        #endregion Enums
        #region Members
        private int m_DebugCounter;
        private readonly ArrayList m_AlTable;
        private readonly ArrayList m_AlTableRow;
        private readonly ArrayList m_AlTableHeaderRow;
        private readonly ArrayList m_AlTableCellContainer;
        private readonly ArrayList m_AlTablePanelContainer;

        private Table m_CurrentTable;
        private TableRow m_CurrentRow;
        private TableRow m_CurrentHeaderRow;
        private TableRow m_CurrentFooterRow;
        private TableCell m_CurrentCellContainer;
        private Panel m_CurrentPanelContainer;
        //

        /// <summary>
        /// Représente le contrôle server  qui stocke les contrôles servers associés au ccis
        /// </summary>
        private PlaceHolder _placeHolder;
        #endregion Members
        //
        #region Accessors

        /// <summary>
        /// 
        /// </summary>
        public override bool ContainsHelpUrlPath
        {
            get
            {
                bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
                string helpUrl = SystemSettings.GetAppSettings(isEFSmLHelp ? "EFSmLHelpSchemasUrl" : "FpMLHelpUrl");
                return (StrFunc.IsFilled(helpUrl));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual Control CurrentContainer
        {
            get
            {
                if (null != m_CurrentPanelContainer)
                    return m_CurrentPanelContainer;
                else
                    return m_CurrentCellContainer;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        protected virtual string GetDisplayKey(DisplayKeyEnum pKey)
        {
            return pKey.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pItemOccurs"></param>
        /// <returns></returns>
        protected virtual string GetDisplayKey(DisplayKeyEnum pKey, int pItemOccurs)
        {
            return pKey.ToString();
        }

        /// <summary>
        ///  Retourne true si l'action est de type consultation
        /// </summary>
        public virtual bool IsModeConsult
        {
            get
            {
                bool ret = true;
                if (null != InputGUI)
                    ret = Cst.Capture.IsModeConsult(InputGUI.CaptureMode);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsModeUpdatePostEvts
        {
            get
            {
                bool ret = true;
                if (null != InputGUI)
                    ret = Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode);
                return ret;
            }
        }

        /// <summary>
        /// Saisie en mode Modification des frais non facturés
        /// </summary>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        public virtual bool IsModeUpdateFeesUninvoiced
        {
            get
            {
                bool ret = true;
                if (null != InputGUI)
                    ret = Cst.Capture.IsModeUpdateFeesUninvoiced(InputGUI.CaptureMode);
                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsModeUpdateLocked
        {
            get
            {
                return IsModeUpdatePostEvts;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string NamePlaceHolder
        {
            get { return "phOTCml"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string NameUpdatePanel
        {
            get { return "updPanel"; }
        }

        /// <summary>
        /// Obtient true si le placehoder contient des control
        /// </summary>
        protected bool IsPlaceHolderLoaded
        {
            get
            {
                Control ctrl = _placeHolder;
                return ((null != ctrl) && ctrl.HasControls());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string SubTitleLeft
        {
            get { return "SubTitleLeft"; }
        }

        /// <summary>
        /// Obtient la ressources associé au menu
        /// </summary>
        protected virtual string TitleLeft
        {
            get
            {
                string ret = string.Empty;
                if (null != InputGUI)
                {
                    //ret = Ressource.GetString(InputGUI.IdMenu);
                    ret = Ressource.GetMenu_Fullname(InputGUI.IdMenu);
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual string TitleRight
        {
            get
            {
                string ret = string.Empty;
                if (null != InputGUI)
                    ret = Cst.Capture.GetLabel(InputGUI.CaptureMode);
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual int TradeIdT
        {
            get { return 0; }
            set { }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsDebugDesign
        {
            get
            {
                return BoolFunc.IsTrue(Request.QueryString["isDebugDesign"]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool IsDebugClientId
        {
            get
            {
                return BoolFunc.IsTrue(Request.QueryString["isDebugClientId"]);
            }
        }

        /// <summary>
        ///  Obtient true lorsque le bouton enregistrer est visible 
        /// </summary>
        /// FI 20140708 [20179] Modify => Gestion de IsModeMatch
        protected virtual bool IsBtnRecordVisible
        {
            get
            {
                return (Cst.Capture.IsModeInput(InputGUI.CaptureMode) ||
                        Cst.Capture.IsModeMatch(InputGUI.CaptureMode));
            }
        }

        /// <summary>
        ///  Obtient true lorsque le bouton annuler est visible 
        /// </summary>
        protected virtual bool IsBtnCancelVisible
        {
            get
            {
                return (IsBtnRecordVisible);
            }
        }

        /// <summary>
        /// Obtient ou définit un indicateur qui signale que le dernier post de la page a provoqué un rechargement complet de la page 
        /// <para>Cet indicateur est stocké dans une variable session afin d'être accessible en cas de double post</para>
        /// <para>voir la méthode UpdatePlaceHolder</para>
        /// </summary>
        /// FI 20100427 [16970] Add isLastPostUpdatePlaceHolder
        /// FI 20200518 [XXXXX] Utilisation de DataCache et BuildDataCacheKey
        protected bool IsLastPostUpdatePlaceHolder
        {
            get
            {
                
                return DataCache.GetData<Boolean>(BuildDataCacheKey("isLastPostUpdatePlaceHolder"));
            }
            set
            {
                DataCache.SetData(BuildDataCacheKey("isLastPostUpdatePlaceHolder"), value);
            }
        }

        /// <summary>
        /// Obtient ou définit un indicateur qui active on non le chargement des ccis à partir des contôles de la page
        /// <para>si true, les ccis sont alimentés à partir des contôles de la page</para>
        /// <para>si false, les ccis ne sont pas alimentés à partir des contôles de la page</para>
        /// </summary>
        ///FI 20100427 [16970] Add isLoadCcisFromGUI
        protected bool IsLoadCcisFromGUI
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient le contrôle server qui stocke les contrôles servers associés au ccis
        /// </summary>
        public override PlaceHolder PlaceHolder
        {
            get { return _placeHolder; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsSupportsPartialRendering
        {
            get { return true; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override MaintainScrollPageEnum MaintainScrollEnum
        {
            get
            {
                return MaintainScrollPageEnum.Automatic;
            }
        }

        /// <summary>
        /// Obtient/Définit le menu Modification
        /// </summary>
        /// FI 20170621 [XXXXX] Modify
        protected skmMenu.MenuItemParent MnuModeModify
        {
            set;
            get;
        }

        
        #endregion Accessors
        //
        #region Constructors
        public CciPageDesign()
        {
            //m_DebugCounter = 0;
            m_AlTable = new ArrayList();
            m_AlTableRow = new ArrayList();
            m_AlTableHeaderRow = new ArrayList();
            m_AlTableCellContainer = new ArrayList();
            m_AlTablePanelContainer = new ArrayList();
        }
        #endregion Constructors
        //
        #region Events
        #endregion Events
        //
        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarDuplicationInfo(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbrduplicate", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName};
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.Template,"mnuDuplication", null, null, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// Purge des contrôles existants sous le placeHorder
        /// </summary>
        protected virtual void ClearPlaceHolder()
        {

            Control ctrl = _placeHolder;
            if (null != ctrl)
                ctrl.Controls.Clear();
            InputGUI.ClearGUI();
        }

        /// <summary>
        /// Sauvegarde des contrôles existants sous le placeHorder dans InputGUI.Controls
        /// </summary>
        protected virtual void SavePlaceHolder()
        {
            AddAuditTimeStep("Start CciPageDesign.SavePlaceHolder");
            
            Control ctrl = _placeHolder;
            if ((null != ctrl) && (0 < ctrl.Controls.Count))
                InputGUI.Controls = ctrl.Controls;
            
            AddAuditTimeStep("End CciPageDesign.SavePlaceHolder");
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void RestorePlaceHolder()
        {

            AddAuditTimeStep("Start CciPageDesign.RestorePlaceHolder");
            
            Control ctrl = _placeHolder;
            if (null != ctrl)
            {
                if (null != InputGUI)
                {
                    ControlCollection  controlcollection = InputGUI.Controls;
                    if (null != controlcollection)
                    {
                        try
                        {
                            while (0 != controlcollection.Count)
                            {
                                ctrl.Controls.Add(controlcollection[0]);
                            }
                        }
                        catch (Exception ex)
                        {
                            ctrl.Controls.AddAt(0, new LiteralControl(System.Environment.NewLine + ex.Message));
                        }
                    }
                }
            }
            //
            AddAuditTimeStep("End CciPageDesign.RestorePlaceHolder");
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20210222 [XXXXX] Suppression Capture.js (fonctions déjà présentes dans PageBase.js)
        // EG 20210222 [XXXXX] Suppression OpenUrl.js (fonctions déplacées dans Referential.js)
        // EG 20210222 [XXXXX] Suppression Validators.js (fonctions déplacées dans PageBase.js)
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ControlsTools.js"));
            base.CreateChildControls();
            JavaScript.ScriptOnStartUp(this, "DisableValidators();", "DisableValidators");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start CciPageDesign.OnInit");
            
            //FI 20100427 [16970] m_isLoadCcisFromGUI est par défaut initialisé à true
            IsLoadCcisFromGUI = true;


            base.OnInit(e);

            if (IsPostBack)
            {
                if ((Enum.IsDefined(typeof(Cst.Capture.MenuConsultEnum), Request.Params["__EVENTARGUMENT"])) ||
                    (Enum.IsDefined(typeof(Cst.Capture.ModeEnum), Request.Params["__EVENTARGUMENT"])))
                    SetScrollPersistence = false;
            }
            AddAuditTimeStep("End CciPageDesign.OnInit");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("Start CciPageDesign.OnPreRender");

            RefreshHeader("tblHeader");
            RefreshHeader("divHeader");
            RefreshButtonsValidate();

            RefreshMnuAction();
            RefreshMnuConsult();
            RefreshDuplicationInfo();
            RefreshMnuReport();
            //RefreshMnuResultAction();

            RefreshFooter();

            RefreshMnuMode(); // Laisser toujours en dernier avant base.OnPreRender

            try
            {
                if (false == IsPostBack)
                    SetInitialFocus();
            }
            catch
            {
                // dommage de planter pour des histoires de focus
            }

            // FI 20200128 [25182] Fait dans PageBase
            /*
            // RD 20121213 [18307] / Gestion du double post
            if (this.HidenLastCtrlChanged != null)
                HidenLastCtrlChanged.Value = "false";
            */


            base.OnPreRender(e);

            AddAuditTimeStep("End CciPageDesign.OnPreRender");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void Render(HtmlTextWriter writer)
        {
            AddAuditTimeStep("Start CciPageDesign.Render");

            CSSColor = InputGUI.CssColor;

            SetToolBarsStyle();

            SetDeFaultControlOnEnter();

            AddAuditTimeStep("End CciPageDesign.Render");

            base.Render(writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20180130 [23749] Modify
        /// EG 20210929 [XXXXX] Pas de sous-titre (action) à droite de la barre de titre, déjà présente dans le titre principal 
        protected override void PageConstruction()
        {
            AddAuditTimeStep("Start CciPageDesign.PageConstruction");
            
            HtmlPageTitle titleLeft = new HtmlPageTitle(TitleLeft, SubTitleLeft);
            //HtmlPageTitle titleRight = new HtmlPageTitle(TitleRight);
            PageTitle = TitleLeft;
            GenerateHtmlForm();
            // FI 20180130 [23749] Set  Form.ID, set form@name attribut in HTML   
            Form.ID = "CCIPage";
            //
            FormTools.AddBanniere(this, Form, titleLeft, null, string.Empty, InputGUI.IdMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, string.Empty, false, string.Empty, InputGUI.IdMenu);

            //
            //Binding des ToolBars
            ToolBarBinding();

            // FI 20200128 [25182] 
            AddInputHiddenLastNoAutoPostbackCtrlChanged();

            AddAuditTimeStep("End CciPageDesign.PageConstruction");
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            AddToolBarHeader();
            AddHeader();
            AddValidatorSummary();
            CreateAndLoadPlaceHolder();
            AddExternalLink();
            AddToolBarFooter();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 201211 [18224] refactoring
        protected override void SearchObjectScreen()
        {
            AddAuditTimeStep("Start CciPageDesign.SearchObjectScreen");

            base.SearchObjectScreen();
            // FI 20200121 [XXXXX] Cette boucle n'est plus nécessaire (fait dans WCLinkButtonOpenBanner.OnPreRender)
            //if (IsPlaceHolderLoaded)
            //{
            //    WCLinkButtonOpenBanner[] lnk = InputGUI.GetLinkButtonOpenBanner();
            //    if (ArrFunc.IsFilled(lnk))
            //    {
            //        foreach (WCLinkButtonOpenBanner lnkItem in lnk)
            //            lnkItem.SetLinkControlDisplay();
            //    }
            //}

            AddAuditTimeStep("End CciPageDesign.SearchObjectScreen");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnAction(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnReport(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnScreen(object sender, EventArgs e)
        {
            // 20090817 RD Bug en cas de Screen inconnu
            DropDownList ddlSender = null;
            //
            if ((null != sender))
                ddlSender = sender as DropDownList;
            //
            if (null != ddlSender)
                InputGUI.CurrentIdScreen = ddlSender.Text;
            else
                InputGUI.CurrentIdScreen = Cst.FpML_ScreenFullCapture;
            //
            Cst.Capture.MenuConsultEnum menuConsultEnum = Cst.Capture.MenuConsultEnum.SetScreen;
            OnConsult(null, new skmMenu.MenuItemClickEventArgs(menuConsultEnum.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnMode(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnConsult(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnValidate(object sender, CommandEventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnClose(object sender, ImageClickEventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnTemplate(object sender, EventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnResultAction(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnRefreshClick(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Purge la page, dessine les contôles, recharge les ccis et déverse leur contenu dans la page  
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected virtual void UpdatePlaceHolder()
        {

            AddAuditTimeStep("Start CciPageDesign.UpdatePlaceHolder");

            //Sauvegarde de l'état initial des contrôles WCLinkButtonOpenBanner
            WCLinkButtonOpenBanner[] linkOpenBanner = InputGUI.GetLinkButtonOpenBanner();

            //Purge des contrôles de la page
            ClearPlaceHolder();

            //purge de la collection cci
            Object.CcisBase.Reset();

            //Chargement des contrôles 
            SearchObjectScreen();

            //Spheres Restaure l'état précédent des contrôles associés aux WCLinkButtonOpenBanner
            if (ArrFunc.IsFilled(linkOpenBanner))
            {
                for (int i = 0; i < ArrFunc.Count(linkOpenBanner); i++)
                {
                    //Retour à l'état initial des contôles WCLinkButtonOpenBanner
                    if (this.FindControl(linkOpenBanner[i].ID) is WCLinkButtonOpenBanner lnk)
                    {
                        lnk.CssClass = linkOpenBanner[i].CssClass;
                    }
                }
            }
            
            //Alimentation des contôles à partir du document  
            Object.CcisBase.LoadDocument(InputGUI.CaptureMode, this);
            //
            SavePlaceHolder();
            //
            AddAuditTimeStep("End CciPageDesign.UpdatePlaceHolder");

        }

        /// <summary>
        /// Efface l'écran (ClearPlaceHolder) et reconstruit l'écran
        /// </summary>
        protected virtual void UpdateScreen()
        {
            ClearPlaceHolder();
            SearchObjectScreen();
            SavePlaceHolder();
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnAddOrDeleteItem()
        {
            string arg = Request.Params["__EVENTARGUMENT"];
            string[] args = arg.Split(';');
            if (ArrFunc.IsFilled(args) && args.Length > 0)
            {
                // Init From Arg
                m_AddRemovePrefix = args[0];
                m_AddRemoveOperatorType = (Cst.OperatorType)Enum.Parse(typeof(Cst.OperatorType), args[1], true);
                string xPath = args[2];
                //Load XML
                if (null != InputGUI.DocXML)
                {
                    XmlNode node = InputGUI.DocXML.SelectSingleNode(xPath);
                    if ((null != node) && StrFunc.IsFilled(XMLTools.GetNodeAttribute(node, "occurs")))
                    {
                        m_AddRemoveNode = node;
                        //
                        UpdatePlaceHolder();
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnZoom()
        {

        }
        /// <summary>
        ///  Ajoute une toolbar dans la form
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarHeader()
        {
            Panel divAllToolBar = new Panel() { ID = "tblMenu", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };

            // Validation (Enregistrer, Annuler, Fermer)
            AddButtonsValidate(divAllToolBar);
            // Duplication
            AddToolBarDuplicationInfo(divAllToolBar);
            // Consultation (Trade Number, First, Previous, Next, Last, Search)
            AddToolBarConsult(divAllToolBar);
            // Trade Mode (Création / Modification et Instrument)
            AddToolBarMode(divAllToolBar);
            // Instrument / Template / Screen List
            AddToolBarGroupProductList(divAllToolBar);
            // Toolbar Action (Aperçus(events,EAR,ACC.etc..) - Bloc-Notes et Pièces jointes)
            AddToolBarAction(divAllToolBar);
            // Toolbar Report (editions pdf)
            AddToolBarReport(divAllToolBar);
            //100%
            if (IsSupportsPartialRendering)
            {
                Panel pnl = new Panel();
                UpdateProgress progress = new UpdateProgress
                {
                    AssociatedUpdatePanelID = "updPanel",
                    ProgressTemplate = new ProgressIndicator(Ressource.GetString("Empty")),
                    DisplayAfter = 300,
                    DynamicLayout = false
                };
                pnl.Controls.Add(progress);
                divAllToolBar.Controls.Add(pnl);
            }
            // Result Trade Action
            if (false == (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode)))
                AddToolBarResultAction(divAllToolBar);

            CellForm.Controls.Add(divAllToolBar);
        }

        /// <summary>
        ///         
        ///  
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarConsult(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbrconsult", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName };
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.Mode, "mnuConsult", null, null, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnConsult);
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarGroupProductList(Panel pPnlParent)
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarMode(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbrmode", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName };
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.Mode, "mnuMode", null, null, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnMode);
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarAction(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbraction", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName };
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.Action, "mnuAction", CstCSS.BannerMenu, CstCSS.BannerMenu, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnAction);
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarResultAction(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbrresultaction", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName };
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.ResultAction, "mnuResultAction", CstCSS.BannerMenu, CstCSS.BannerMenu, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnResultAction);
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void AddToolBarReport(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "tbrreport", CssClass = this.CSSMode + " skmnu " + InputGUI.MainMenuClassName };
            skmMenu.Menu mnuToolBar = new skmMenu.Menu((int)Cst.Capture.MenuEnum.Report, "mnuReport", CstCSS.BannerMenu, CstCSS.BannerMenu, null, null, null);
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);

            mnuToolBar.DataSource = mnu.InitXmlWriter();
            mnuToolBar.Layout = skmMenu.MenuLayout.Horizontal;
            mnuToolBar.LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN;
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnReport);
            pnl.Controls.Add(mnuToolBar);
            pPnlParent.Controls.Add(pnl);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void AddHeader()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AddValidatorSummary()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20201014 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et réactivation de isSupportsPartialRendering dans tous les cas
        protected virtual void CreateAndLoadPlaceHolder()
        {

            Table table = new Table
            {
                ID = "tblDetail",
                Width = Unit.Percentage(100),
                Height = Unit.Percentage(100),
                BorderStyle = (IsDebugDesign ? BorderStyle.Solid : BorderStyle.None),
                BorderColor = Color.CornflowerBlue,
                CellPadding = 0,
                CellSpacing = 0
            };

            TableRow tr = new TableRow
            {
                ID = "trDetail"
            };
            // Row 1 - Cell 1
            TableCell td = new TableCell
            {
                ID = "tdDetail"
            };

            Panel pnlBody = new Panel
            {
                EnableViewState = false,
                ID = "divbody",
                CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName
            };

            _placeHolder = new PlaceHolder
            {
                EnableViewState = false,
                ID = NamePlaceHolder
            };

            Control control;
            if (IsSupportsPartialRendering)
            {
                control = new UpdatePanel
                {
                    EnableViewState = false,
                    ID = NameUpdatePanel
                };
                ((UpdatePanel)control).ContentTemplateContainer.Controls.Add(_placeHolder);
            }
            else
            {
                control = _placeHolder;
            }
            pnlBody.Controls.Add(control);
            td.Controls.Add(pnlBody);
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            //
            CellForm.Controls.Add(table);

        }

        // EG 20160119 Refactoring Footer
        protected virtual void AddButtonsFooter(WebControl pCtrlContainer)
        {
            pCtrlContainer.Controls.Add(GetButtonFooter_POST());
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual void AddExternalLink()
        {
        }
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected virtual void AddToolBarFooter()
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTr"></param>
        // EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
        protected virtual void AddButtonsValidate(WebControl pCtrlContainer)
        {
            // Enregistrer
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnRecord", "fa fa-save", true);
            btn.CommandName = Cst.Capture.MenuValidateEnum.Record.ToString();
            btn.Command += new CommandEventHandler(this.OnValidate);
            if (StrFunc.IsFilled((Request.Params["__EVENTARGUMENT"])))
                btn.CommandArgument = Request.Params["__EVENTARGUMENT"];
            btn.CausesValidation = false;
            btn.Pty.TooltipContent = btn.Text + Cst.GetAccessKey(btn.AccessKey);

            AddInputHiddenDeFaultControlOnEnter();
            HiddenFieldDeFaultControlOnEnter.Value = btn.ClientID;

            pCtrlContainer.Controls.Add(btn);

            // Cancel 
            btn = ControlsTools.GetAwesomeButtonCancel(true);
            btn.CommandName = Cst.Capture.MenuValidateEnum.Annul.ToString();
            btn.Command += new CommandEventHandler(this.OnValidate);
            if (StrFunc.IsFilled((Request.Params["__EVENTARGUMENT"])))
                btn.CommandArgument = Request.Params["__EVENTARGUMENT"];
            btn.CausesValidation = false;
            btn.AccessKey = "C";
            btn.Pty.TooltipContent = btn.Text + Cst.GetAccessKey(btn.AccessKey);
            pCtrlContainer.Controls.Add(btn);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_POST()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnPOST","P","POST", "PostAndRefreshToolTip","fas fa-sync");
            btn.Click += new EventHandler(OnRefreshClick);
            return btn;
        }
        protected virtual WCToolTipLinkButton GetLinkButtonFooter(string pId, string pAccessKey, string pText, string pTooltip)
        {
            return GetLinkButtonFooter(pId, pAccessKey, pText, pTooltip, "fas fa-external-link-alt");
        }

        // EG 20200902 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et compléments
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Désactivation du tabIndex
        protected virtual WCToolTipLinkButton GetLinkButtonFooter(string pId, string pAccessKey, string pText, string pTooltip, string pFontAwesome)
        {
            WCToolTipLinkButton btn = new WCToolTipLinkButton
            {
                ID = pId,
                CssClass = "fa-icon",
                AccessKey = pAccessKey,
                TabIndex = -1,
                CausesValidation = false,
                Text = String.Format(@"<i class='{0}'></i> {1}", pFontAwesome, pText)
            };
            btn.Pty.TooltipContent = Ressource.GetString(pTooltip) + Cst.GetAccessKey(pAccessKey);
            return btn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMenuItems"></param>
        /// <param name="pMenuItemClick"></param>
        /// <returns></returns>
        protected string GetMenuImageUrl(skmMenu.MenuItemCollection pMenuItems, skmMenu.MenuItemClickEventArgs pMenuItemClick)
        {
            string imageUrl = string.Empty;
            if (null != pMenuItems)
            {
                for (int i = 0; i < pMenuItems.Count; i++)
                {
                    if (pMenuItems[i].CommandName != pMenuItemClick.CommandName)
                    {
                        imageUrl = GetMenuImageUrl(pMenuItems[i].SubItems, pMenuItemClick);
                    }
                    else
                        imageUrl = pMenuItems[i].ImageUrl;

                    if (StrFunc.IsFilled(imageUrl))
                        break;
                }
            }
            return imageUrl;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuCreation()
        {


            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnCreation",
                aToolTip = Ressource.GetString("Creation"),
                Enabled = InputGUI.IsCreateAuthorised,
                eImageUrl = "fas fa-icon fa-plus-circle"
            };
            if (InputGUI.IsCreateAuthorised)
            {
                ret.eCommandName = Cst.Capture.ModeEnum.New.ToString();
                ret.eBlockUIMessage = ret.aToolTip + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += Ressource.GetString("NotAllowed");
            }
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuConsult()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnConsult",
                aToolTip = Ressource.GetString("btnSearch"),
                eImageUrl = "fas fa-icon fa-search",
                eCommandName = Cst.Capture.ModeEnum.Consult.ToString()
            };
            ret.eBlockUIMessage = ret.aToolTip + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuDuplicate()
        {

            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnDuplication",
                aToolTip = Ressource.GetString("Duplication"),
                Enabled = InputGUI.IsCreateAuthorised,
                eImageUrl = "fas fa-icon fa-copy"
            };
            ret.eBlockUIMessage = ret.aToolTip + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");

            if (InputGUI.IsCreateAuthorised)
            {
                ret.eCommandName = Cst.Capture.ModeEnum.Duplicate.ToString();
            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += " " + Ressource.GetString("NotAllowed");
            }
            return ret;

        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuModify()
        {


            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnModification",
                aToolTip = Ressource.GetString("Modification"),
                Enabled = InputGUI.IsModifyAuthorised,
                eImageUrl = "fas fa-icon fa-edit",
            };
            ret.eBlockUIMessage = ret.aToolTip + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");

            if (InputGUI.IsModifyAuthorised)
            {
                ret.eCommandName = Cst.Capture.ModeEnum.Update.ToString();
            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += Ressource.GetString("NotAllowed");
            }
            return ret;

        }

        /// <summary>
        /// Retourne le contenu du toolbar action 
        /// </summary>
        /// <returns></returns>
        protected virtual skmMenu.MenuItemParent GetMenuItemParentMnuAction()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(1);
            return ret;

        }

        /// <summary>
        /// Retourne le contenu du toolbar Consult
        /// </summary>
        /// <returns></returns>
        protected virtual skmMenu.MenuItemParent GetMenuItemParentMnuConsult()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(1);
            return ret;
        }

        /// <summary>
        /// Retourne le contenu de la toolbar Mode
        /// </summary>
        /// <returns></returns>
        /// <returns></returns>
        protected virtual skmMenu.MenuItemParent GetMenuItemParentMnuMode()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(1);
            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual skmMenu.MenuItemParent GetMenuItemParentMnuReport()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(1);
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected virtual skmMenu.MenuItemParent GetInfoDuplication()
        {
            return new skmMenu.MenuItemParent(1);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void RefreshHeader(string pId)
        {

            #region Header
            Control ctrlContainer = FindControl(pId);
            Control ctrl;
            if (null != ctrlContainer)
            {
                ctrl = ctrlContainer.FindControl("titleLeft");
                if (null != ctrl)
                {
                    if (ctrl is TableCell cell)
                        cell.Text = TitleLeft;
                    else if (ctrl is Label label)
                        label.Text = TitleLeft;
                }

                ctrl = ctrlContainer.FindControl("subtitleLeft");
                if (null != ctrl)
                {
                    if (ctrl is TableCell cell)
                        cell.Text = SubTitleLeft;
                    else if (ctrl is Label label)
                        label.Text = SubTitleLeft;
                }

                ctrl = ctrlContainer.FindControl("titleRight");
                if (null != ctrl)
                {
                    if (ctrl is TableCell cell)
                        cell.Text = TitleRight;
                    else if (ctrl is Label label)
                        label.Text = TitleRight;
                }
            }
            #endregion Header
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void RefreshButtonsValidate()
        {

        }

        /// <summary>
        /// Refresh toolbar Action 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240619 [WI969] Refactoring
        protected virtual void RefreshMnuAction()
        {
            if (FindControl("tblMenu") is Control ctrlContainer)
            {
                if (ctrlContainer.FindControl("mnuAction") is skmMenu.Menu ctrlMenu)
                {
                    ctrlMenu.Items.Clear();
                    skmMenu.MenuItemParent mnu = GetMenuItemParentMnuAction();
                    ctrlMenu.DataSource = mnu.InitXmlWriter();
                    if (ctrlContainer.FindControl("tbraction") is Control ctrl)
                        ctrl.DataBind();
                }
            }
        }

        /// <summary>
        /// Refresh toolbar Consultation
        /// </summary>
        protected virtual void RefreshMnuConsult()
        {
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Control ctrl = ctrlContainer.FindControl("mnuConsult");
                if (null != ctrl)
                {
                    skmMenu.Menu ctrlMenu = (skmMenu.Menu)ctrl;
                    ctrlMenu.Items.Clear();
                    skmMenu.MenuItemParent mnu = GetMenuItemParentMnuConsult();
                    ctrlMenu.DataSource = mnu.InitXmlWriter();
                    ctrl = ctrlContainer.FindControl("tbrconsult");
                    if (null != ctrl)
                        ctrl.DataBind();
                }
            }
        }

        /// <summary>
        /// Refresh toolbar Mode
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void RefreshMnuMode()
        {
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Control ctrl = ctrlContainer.FindControl("mnuMode");
                if (null != ctrl)
                {
                    skmMenu.Menu ctrlMenu = (skmMenu.Menu)ctrl;
                    ctrlMenu.Items.Clear();
                    skmMenu.MenuItemParent mnu = GetMenuItemParentMnuMode();
                    ctrlMenu.DataSource = mnu.InitXmlWriter();
                    ctrl = ctrlContainer.FindControl("tbrmode");
                    if (null != ctrl)
                        ctrl.DataBind();
                }
            }
        }

        /// <summary>
        /// Refresh toolbar Report
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void RefreshMnuReport()
        {
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Control ctrl = ctrlContainer.FindControl("mnuReport");
                if (null != ctrl)
                {
                    skmMenu.Menu ctrlMenu = (skmMenu.Menu)ctrl;
                    ctrlMenu.Items.Clear();
                    if (false == Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode))
                    {
                        skmMenu.MenuItemParent mnu = GetMenuItemParentMnuReport();
                        ctrlMenu.DataSource = mnu.InitXmlWriter();
                        ctrl = ctrlContainer.FindControl("tbrreport");
                        if (null != ctrl)
                            ctrl.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void RefreshMnuInstrument()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20230207 [XXXXX] Trade Input: Affichage du trade en cours de duplication
        protected virtual void RefreshDuplicationInfo()
        {
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Control ctrl = ctrlContainer.FindControl("mnuDuplication");
                if (null != ctrl)
                {
                    skmMenu.Menu ctrlMenu = (skmMenu.Menu)ctrl;
                    ctrlMenu.Items.Clear();
                    skmMenu.MenuItemParent mnu = GetInfoDuplication();
                    ctrlMenu.DataSource = mnu.InitXmlWriter();
                    ctrl = ctrlContainer.FindControl("tbrduplicate");
                    if (null != ctrl)
                        ctrl.DataBind();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void RefreshFooter()
        {
        }

        /// <summary>
        /// Applique le focus sur le bouton BtnRecord ou le BtnCancel on le 1er control de la saisie light
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void SetInitialFocus()
        {

            string clientId = string.Empty;

            Control btnFocus = null;
            Control ctrlContainer = FindControl("tblMenu");

            if ((IsBtnRecordVisible))
                btnFocus = ctrlContainer.FindControl("BtnRecord");
            else if (IsBtnCancelVisible)
                btnFocus = ctrlContainer.FindControl("BtnCancel");
            //
            if (null != btnFocus)
                clientId = btnFocus.ClientID;
            //
            if (IsBtnRecordVisible)
            {
                //En Mode saisie (isBtnRecordVisible = true)
                //On postionne le focus sur le 1er control enabled de la saisie light
                if ((null != Object) && ArrFunc.Count(Object.CcisBase) > 0)
                {
                    CustomCaptureInfo cci = Object.CcisBase.GetFirstCciEnabled();
                    if (null != cci)
                        clientId = cci.ClientId;
                }
            }
            //
            if (StrFunc.IsFilled(clientId))
                SetFocus(clientId);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SetToolBarsStyle()
        {
        }

        /// <summary>
        /// Définit le control sur lequel sera appliquer le click lorsque la touche entré est actionnée
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual void SetDeFaultControlOnEnter()
        {
            //20090923 FI butRecord est le bonton par défaut lorsque la touche entré est actionnée
            HiddenFieldDeFaultControlOnEnter.Value = string.Empty;
            //
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Control ctrl = ctrlContainer.FindControl("BtnRecord");
                if (null != ctrl)
                {
                    if (IsBtnRecordVisible)
                        HiddenFieldDeFaultControlOnEnter.Value = ctrl.ClientID;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual void ToolBarBinding()
        {
            Control ctrlContainer = FindControl("tblMenu");
            Control ctrl;
            if (null != ctrlContainer)
            {
                // Warning : MnuTemplate (0) and MnuScreen (4) are binding on Render Event
                for (int i = 1; i < 4; i++)
                {
                    string name = Enum.ToObject(typeof(Cst.Capture.MenuEnum), i).ToString();
                    ctrl = ctrlContainer.FindControl("tbr" + name);
                    if (null != ctrl)
                        ctrl.DataBind();
                }
            }
        }

        /// <summary>
        ///  Retourne true si le contrôle de la page représenté par un CustomObject est en lecture seule
        ///  <para>Retourne true si mode consultation</para>
        /// </summary>
        /// <param name="pCo"></param>
        /// <returns></returns>
        /// FI 20140708 [20179] Modify => Gestion du mode IsModeMatch
        protected virtual bool IsControlModeConsult(CustomObject pCo)
        {
            Boolean ret = IsModeConsult;
            //FI 20140708 [20179] Les contrôles sont en mode readOnly si CaptureMode = Match (aucune saisie possible)
            if (false == ret)
            {
                if (null != InputGUI)
                    ret = Cst.Capture.IsModeMatch(InputGUI.CaptureMode);
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void EraseTable()
        {
            m_CurrentTable = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableSettings"></param>
        /// FI 20170928 [23452] Modify
        protected override void CreateTable(TableSettings pTableSettings)
        {

            if (IsDebugDesign)
            {
                m_DebugCounter++;
                System.Diagnostics.Debug.WriteLine("    debugCounter(" + m_DebugCounter.ToString() + ")");
            }
            //
            Color debugColor = Color.Red;
            //
            VerticalAlign vAlign = VerticalAlign.NotSet;
            if (StrFunc.IsFilled(pTableSettings.VerticalAlign))
                vAlign = (VerticalAlign)System.Enum.Parse(typeof(VerticalAlign), pTableSettings.VerticalAlign, true);
            //
            if (null != m_CurrentTable)
            {
                //Save current table
                m_AlTable.Add(m_CurrentTable);
                m_AlTableRow.Add(m_CurrentRow);
                m_AlTableHeaderRow.Add(m_CurrentHeaderRow);
                m_AlTableHeaderRow.Add(m_CurrentFooterRow);
                m_AlTableCellContainer.Add(m_CurrentCellContainer);
                m_AlTablePanelContainer.Add(m_CurrentPanelContainer);
            }
            //currentCellContainer
            m_CurrentCellContainer = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                VerticalAlign = vAlign,
                Wrap = false,
                ColumnSpan = GetColspan(pTableSettings.ColSpan),
                RowSpan = GetRowspan(pTableSettings.RowSpan)
            };
            // FI 20170928 [23452] Application de Style si rowStyle est renseigné
            if (StrFunc.IsFilled(pTableSettings.CellStyle))
                m_CurrentCellContainer.Style.Value = pTableSettings.CellStyle;
            ControlsTools.SetFixedCol(m_CurrentCellContainer, pTableSettings.FixedCol);
            //
            //
            m_CurrentPanelContainer = null;
            if (StrFunc.IsFilled(pTableSettings.PanelOverFlow))
            {
                m_CurrentPanelContainer = new Panel();
                string[] overFlowValue = pTableSettings.PanelOverFlow.Split(';');
                m_CurrentPanelContainer.Height = Unit.Pixel(Convert.ToInt32(overFlowValue[0]));
                if (2 == overFlowValue.Length)
                    m_CurrentPanelContainer.Width = Unit.Pixel(Convert.ToInt32(overFlowValue[1]));
                m_CurrentPanelContainer.CssClass = "tableHolder";
                m_CurrentCellContainer.Controls.Add(m_CurrentPanelContainer);
            }
            //
            //currentTable
            m_CurrentTable = new Table();
            if (StrFunc.IsFilled(pTableSettings.Id))
                m_CurrentTable.ID = pTableSettings.Id;
            //
            m_CurrentTable.Attributes["controlalignleft"] = BoolFunc.IsTrue(pTableSettings.ControlAlignLeft).ToString();
            //
            m_CurrentTable.CellPadding = 0;
            m_CurrentTable.CellSpacing = 0;
            //
            m_CurrentTable.Width = Unit.Percentage(100);
            //
            ControlsTools.SetStyleList(m_CurrentTable.Style, pTableSettings.Style);
            //
            #region Border
            // 2008/10/03 EG test existence class dans style
            if (StrFunc.IsEmpty(m_CurrentTable.Style["class"]))
            {
                m_CurrentTable.BorderStyle = BorderStyle.None;
                m_CurrentTable.BorderWidth = Unit.Parse("0");
            }
            GridLines gridLines = GridLines.None;
            if (StrFunc.IsFilled(pTableSettings.GridLines))
            {
                gridLines = (GridLines)System.Enum.Parse(typeof(GridLines), pTableSettings.GridLines, true);
                m_CurrentTable.BorderStyle = BorderStyle.Solid;
                m_CurrentTable.CellPadding = 0;
                m_CurrentTable.CellSpacing = 0;
                m_CurrentTable.BorderWidth = Unit.Point(1);

                //m_CurrentTable.CellPadding = 2;
                //m_CurrentTable.CellSpacing = 1;
            }
            //
            m_CurrentTable.GridLines = gridLines;

            if (StrFunc.IsFilled(pTableSettings.BorderColor))
                m_CurrentTable.BorderColor = Color.FromName(pTableSettings.BorderColor);
            if (IsDebugDesign)
                m_CurrentTable.GridLines = GridLines.Both;

            if (IsDebugDesign)
            {
                m_CurrentTable.BorderWidth = Unit.Parse("2");
                int item = m_AlTable.Count;
                switch (item)
                {
                    case 0:
                        if (debugColor == Color.Blue)
                            debugColor = Color.Red;
                        else
                            debugColor = Color.Blue;
                        m_CurrentTable.BorderColor = debugColor;
                        break;
                    case 5:
                        m_CurrentTable.BorderColor = Color.DarkGray;
                        break;
                    case 1:
                    case 6:
                        m_CurrentTable.BorderColor = Color.Black;
                        break;
                    case 2:
                    case 7:
                        m_CurrentTable.BorderColor = Color.DarkRed;
                        break;
                    case 3:
                    case 8:
                        m_CurrentTable.BorderColor = Color.DarkGreen;
                        break;
                    case 4:
                    case 9:
                        m_CurrentTable.BorderColor = Color.DarkBlue;
                        break;
                }
            }
            #endregion Border
            //
            if (IsDebugDesign)
            {
                System.Diagnostics.Debug.WriteLine("CreateTable(" + (m_AlTable.Count + 1).ToString() + ")");
                m_CurrentRow = new TableRow();
                TableCell tableCell = new TableCell();
                Label lbl = new Label
                {
                    Text = m_DebugCounter.ToString() + "/" + (m_AlTable.Count + 1).ToString(),
                    ForeColor = debugColor
                };
                tableCell.Controls.Add(lbl);
                m_CurrentRow.Controls.Add(tableCell);
                m_CurrentTable.Controls.Add(m_CurrentRow);
            }
            //currentRow
            m_CurrentRow = new TableRow();
            m_CurrentHeaderRow = new TableHeaderRow();
            m_CurrentFooterRow = new TableHeaderRow();

        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void WriteTable()
        {

            if (null == m_CurrentTable)
                return;
            const int FULL_Colspan = 999;
            bool isTableInCellWithFullColspan = (m_CurrentCellContainer.ColumnSpan == FULL_Colspan);

            //Write current row
            WriteRowToTable();

            #region Add, if necessary, a last column with width 100% for align to left all data
            bool isExistCellWithFullColspan;
            bool isExistCellWithWidth100Percent;
            int totalPercentCellWithWidthInPercent;
            int maxnbCellInRow = 0;
            int nbCellInRow;
            bool isCurrentCellWithFullColspan;

            #region Count max column by row, for each rows
            foreach (TableRow tableRow in m_CurrentTable.Controls)
            {
                isExistCellWithFullColspan = false;
                isExistCellWithWidth100Percent = false;
                totalPercentCellWithWidthInPercent = 0;
                nbCellInRow = 0;
                foreach (TableCell tableCell in tableRow.Controls)
                {
                    isCurrentCellWithFullColspan = (tableCell.ColumnSpan == FULL_Colspan);
                    isExistCellWithFullColspan |= isCurrentCellWithFullColspan;
                    nbCellInRow++;
                    if (!isCurrentCellWithFullColspan)
                    {
                        if (tableCell.ColumnSpan > 1)
                            nbCellInRow = nbCellInRow + tableCell.ColumnSpan - 1;

                        if (tableCell.Width.Equals(Unit.Percentage(100)))
                            isExistCellWithWidth100Percent = true;
                        else if (tableCell.Width.ToString().EndsWith("%"))
                            totalPercentCellWithWidthInPercent += Convert.ToInt32(tableCell.Width.ToString().Replace("%", string.Empty));
                    }

                    if (IsDebugDesign && isExistCellWithWidth100Percent)
                        tableCell.BackColor = Color.Orange;
                }
                isExistCellWithWidth100Percent |= (totalPercentCellWithWidthInPercent >= 100);

                if (!isExistCellWithFullColspan)
                {
                    if (nbCellInRow > maxnbCellInRow)
                        maxnbCellInRow = nbCellInRow;
                }

                if (isExistCellWithWidth100Percent)
                    break;
            }
            #endregion

            #region Complete all rows
            for (int i = 0; i < m_CurrentTable.Controls.Count; i++)
            {
                isExistCellWithFullColspan = false;
                isExistCellWithWidth100Percent = false;
                totalPercentCellWithWidthInPercent = 0;
                nbCellInRow = 0;
                //
                TableRow tableRow = (TableRow)m_CurrentTable.Controls[i];
                //
                #region Count total columns on current row
                foreach (TableCell tableCell in tableRow.Controls)
                {
                    isCurrentCellWithFullColspan = (tableCell.ColumnSpan == FULL_Colspan);
                    isExistCellWithFullColspan |= isCurrentCellWithFullColspan;
                    nbCellInRow++;
                    if (!isCurrentCellWithFullColspan)
                    {
                        if (tableCell.ColumnSpan > 1)
                            nbCellInRow = nbCellInRow + tableCell.ColumnSpan - 1;
                        //
                        if (tableCell.Width.Equals(Unit.Percentage(100)))
                            isExistCellWithWidth100Percent = true;
                        else if (tableCell.Width.ToString().EndsWith("%"))
                            totalPercentCellWithWidthInPercent += Convert.ToInt32(tableCell.Width.ToString().Replace("%", string.Empty));
                    }
                }
                isExistCellWithWidth100Percent |= (totalPercentCellWithWidthInPercent >= 100);
                #endregion

                if (isExistCellWithFullColspan)
                {
                    #region Set colspan on columns with "FULL_Colspan"
                    foreach (TableCell tableCell in tableRow.Controls)
                    {
                        if ((tableCell.ColumnSpan == FULL_Colspan))
                        {
                            //NB: +1 pour la colonne courante, et +1 pour celel qui est automatiquement ajoutée plus bas avec un width 100%
                            tableCell.ColumnSpan = Math.Max(0, maxnbCellInRow - nbCellInRow + 1 + 1);
                            break;
                        }
                    }
                    #endregion
                }
                //
                //200809025 PL On ignore les lignes avec un FullColspan
                if (null != m_CurrentTable.Attributes["controlalignleft"])
                {
                    if (BoolFunc.IsTrue(m_CurrentTable.Attributes["controlAlignLeft"]))
                    {
                        if ((!isExistCellWithFullColspan) && (!isExistCellWithWidth100Percent) && (nbCellInRow == maxnbCellInRow))
                        {
                            #region Ajout d'une Nième colonne avec un colspan et un width=100%, sur la ligne disposant du maximum de colonne
                            string data = Cst.HTMLSpace;
                            TableCell tCell = new TableCell
                            {
                                ColumnSpan = maxnbCellInRow - nbCellInRow,
                                Width = Unit.Percentage(100)
                            };
                            if (IsDebugDesign)
                            {
                                tCell.ForeColor = Color.Blue;
                                tCell.BackColor = Color.Yellow;
                                data = tCell.ColumnSpan.ToString();
                            }
                            tCell.Controls.Add(new LiteralControl(data));
                            tableRow.Controls.Add(tCell);
                            #endregion
                        }
                    }
                    m_CurrentTable.Attributes.Remove("controlalignleft");
                }

            }
            #endregion
            #endregion

            int item = m_AlTable.Count - 1;
            if (item < 0)
            {
                #region Write current table in PlaceHolder
                Control ctrl = _placeHolder;
                if (null != ctrl)
                {
                    if (0 < m_CurrentTable.Style.Count)
                    {
                        if (BoolFunc.IsTrue(m_CurrentTable.Style["istableB"]))
                        {
                            WCBodyPanel bodyPanel = CreateBodyPanel(m_CurrentTable);
                            bodyPanel.AddContent(m_CurrentTable);
                            ctrl.Controls.Add(bodyPanel);
                            TableTools.RemoveStyleTableBody(m_CurrentTable);
                        }
                        else if (BoolFunc.IsTrue(m_CurrentTable.Style["istableH"]))
                        {
                            WCTogglePanel togglePanel = CreateTogglePanel(m_CurrentTable);
                            togglePanel.AddContent(m_CurrentTable);
                            ctrl.Controls.Add(togglePanel);

                            TableTools.RemoveStyleTableHeader(m_CurrentTable);
                        }
                        else
                        {
                            SetStyleForTable(m_CurrentTable);
                            ctrl.Controls.Add(m_CurrentTable);
                        }
                    }
                    else
                        ctrl.Controls.Add(m_CurrentTable);
                }
                #endregion
                m_CurrentTable = null;
            }
            else
            {
                #region Write current table in top table
                //Retrieve top row and remove in arraylist
                m_CurrentRow = (TableRow)m_AlTableRow[item];
                m_AlTableRow.RemoveAt(item);
                //Add current table in cell and cell in top row
                if (0 < m_CurrentTable.Style.Count)
                {
                    if (BoolFunc.IsTrue(m_CurrentTable.Style["istableB"]))
                    {
                        WCBodyPanel bodyPanel = CreateBodyPanel(m_CurrentTable);
                        bodyPanel.AddContent(m_CurrentTable);
                        CurrentContainer.Controls.Add(bodyPanel);
                        TableTools.RemoveStyleTableBody(m_CurrentTable);
                    }
                    else if (BoolFunc.IsTrue(m_CurrentTable.Style["istableH"]))
                    {
                        WCTogglePanel togglePanel = CreateTogglePanel(m_CurrentTable);
                        togglePanel.AddContent(m_CurrentTable);
                        CurrentContainer.Controls.Add(togglePanel);
                        TableTools.RemoveStyleTableHeader(m_CurrentTable);
                    }
                    else
                    {
                        SetStyleForTable(m_CurrentTable);
                        CurrentContainer.Controls.Add(m_CurrentTable);
                    }
                }
                else
                    CurrentContainer.Controls.Add(m_CurrentTable);
                //
                m_CurrentRow.Controls.Add(m_CurrentCellContainer);
                #endregion
                //Retrieve top table and remove in arraylist                
                m_CurrentTable = (Table)m_AlTable[item];
                m_AlTable.RemoveAt(item);

                m_CurrentCellContainer = (TableCell)m_AlTableCellContainer[item];
                m_AlTableCellContainer.RemoveAt(item);

                m_CurrentPanelContainer = (Panel)m_AlTablePanelContainer[item];
                m_AlTablePanelContainer.RemoveAt(item);

                if (isTableInCellWithFullColspan)
                    WriteRowToTable();

            }

        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void SetStyleForTable(Table pCurrentTable)
        {
            Color bkColor = Color.Transparent;
            Color borderColor = Color.Transparent;
            string cssClass = null;

            if (0 < pCurrentTable.Style.Count)
            {
                if (null != m_CurrentTable.Style["class"])
                    cssClass = pCurrentTable.Style["class"];
                //
                if (null != pCurrentTable.Style[HtmlTextWriterStyle.BackgroundColor])
                {
                    bkColor = Color.FromName(pCurrentTable.Style[HtmlTextWriterStyle.BackgroundColor]);
                    borderColor = bkColor;
                }
                //
                if (null != pCurrentTable.Style[HtmlTextWriterStyle.BorderColor])
                    borderColor = Color.FromName(pCurrentTable.Style[HtmlTextWriterStyle.BorderColor]);
            }

            if (StrFunc.IsFilled(cssClass))
            {
                pCurrentTable.CssClass = cssClass;
                pCurrentTable.Style.Remove("class");
            }
            else
            {
                if (null != bkColor)
                    pCurrentTable.BackColor = bkColor;
                if (null != borderColor)
                    pCurrentTable.BorderColor = borderColor;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pTypeControl"></param>
        /// <param name="pParentOccurs"></param>
        /// FI 20121126 [18224] add parameter pControl 
        /// EG 20170822 [23342] New CustomObject.ControlEnum.timestamp (WriteTimestamp)
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void WriteControl(CustomObject pCo, CustomObject.ControlEnum pTypeControl, int pParentOccurs, out Control pControl)
        {
            pControl = null;

            if (IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteControl(" + pCo.CtrlClientId + ")");
            //
            switch (pTypeControl)
            {
                //Look
                case CustomObject.ControlEnum.banner:
                    WriteBanner((CustomObjectBanner)pCo);
                    break;
                case CustomObject.ControlEnum.openbanner:
                case CustomObject.ControlEnum.buttonbanner:
                case CustomObject.ControlEnum.ddlbanner:
                    WriteLinkOpenBanner((ICustomObjectOpenBanner)pCo, pParentOccurs, out pControl);
                    break;
                case CustomObject.ControlEnum.br:
                    WriteRowToTable();
                    break;
                case CustomObject.ControlEnum.rowheader:
                    WriteRowHeaderToTable((CustomObjectRowHeader)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.rowfooter:
                    WriteRowFooterToTable((CustomObjectRowFooter)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.hr:
                case CustomObject.ControlEnum.hline:
                    WriteHR(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.fill:
                case CustomObject.ControlEnum.space:
                    WriteSpace(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.vline:
                    WriteDivider(pCo);
                    break;
                //Data
                case CustomObject.ControlEnum.button:
                    WriteButton((CustomObjectButton)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.checkbox:
                    WriteCheckBox(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.htmlcheckbox:
                    WriteHtmlInputCheckBox(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.htmlselect:
                case CustomObject.ControlEnum.dropdown:
                case CustomObject.ControlEnum.optgroupdropdown: // 20090909 RD / DropDownList avec OptionGroup
                    WriteDropDown((CustomObjectDropDown)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.display:
                case CustomObject.ControlEnum.label:
                    WriteLabel(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.cellheader:
                    WriteCellHeader(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.cellfooter:
                    WriteCellFooter(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.hiddenbox:
                    WriteHiddenBox(pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.hyperlink:
                    WriteHyperlink((CustomObjectHyperLink)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.image:
                    WriteImage((CustomObjectImage)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.timestamp:
                    WriteTimestamp((CustomObjectTimestamp)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.textbox:
                case CustomObject.ControlEnum.quickinput:
                    WriteTextBox((CustomObjectTextBox)pCo, out pControl);
                    break;
                case CustomObject.ControlEnum.panel:
                    WritePanel((CustomObjectPanel)pCo, out pControl);
                    break;
                //Error
                default:
                    // Impossible
                    break;
            }
        }

        /// <summary>
        /// Set Display on control and tablecell Parent
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pStyleDisplay"></param>
        public override void SetStyleDisplay(Control pControl, string pStyleDisplay)
        {

            base.SetStyleDisplay(pControl, pStyleDisplay);
            //
            TableCell cell = (TableCell)pControl.Parent;
            if (null != cell)
            {
                ControlsTools.SetStyleList(cell.Style, pStyleDisplay);
                //
                IAttributeAccessor controlAttributAccessor = (IAttributeAccessor)pControl;
                if (BoolFunc.IsTrue(controlAttributAccessor.GetAttribute("isddlbanner")))
                {
                    TableRow row = (TableRow)(cell.Parent);
                    if (null != row)
                        ControlsTools.SetStyleList(row.Style, pStyleDisplay);
                }
            }

        }

        /// <summary>
        /// Remove Display style on control and tablecell Parent
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pStyleDisplay"></param>
        public override void RemoveStyleDisplay(Control pControl)
        {
            ControlsTools.RemoveStyleDisplay(pControl);
            TableCell cell = (TableCell)pControl.Parent;
            if (null != cell)
            {
                ControlsTools.RemoveStyleDisplay(cell);
                IAttributeAccessor controlAttributAccessor = (IAttributeAccessor)pControl;
                if (BoolFunc.IsTrue(controlAttributAccessor.GetAttribute("isddlbanner")))
                {
                    TableRow row = (TableRow)(cell.Parent);
                    if (null != row)
                        ControlsTools.RemoveStyleDisplay(row);
                }
            }

        }

        /// <summary>
        /// Retourne true si Spheres® doit annuler l'enregistrement en cours et auto-générer une nouvelle publication de la page pour procéder à l'enregistrement (simulation d'un click sur Enregistrer).
        /// <para>La nouvelle publication aura pour argument PostBackForValidation</para>
        /// </summary>
        /// FI 20200128 [25182] Refactoring 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected bool PostBackForValidation()
        {
            Boolean ret = false;
            if (PARAM_EVENTARGUMENT != PostBackForValidationArg) //(*)
            {
                // FI 20100427 [16970]
                // isLastPostUpdatePlaceHolder = true, 
                // Signifie qu'une double publication est en cours. 
                // L'utilisateur vient de cliquer sur enregistrer (publication de la page) alors qu'il vient tout juste de modifier un contrôle autopostback (provoquant aussi une publication via onblur)
                // De plus la publication issue du contrôle a provoqué un rechargement complet de la page (exemple lorsqu'il existe déjà des frais, ils sont généralement supprimés).
                //
                // => Dans ce cas Spheres®  
                // - annule l'enregistrement en cours (lors de la 2ème publication, celle provoquée par le bouton enregistrer) 
                // - lors de la 2ème publication, Spheres® ne prend pas en considération des modifications (puisque déjà effectuées lors de la 1er publication) 
                // - procède de nouveau à l'enregistrement (via PostBackForValidation)...
                // - lors du nouvel enregistrement Spheres® proposera les frais nouvellement recalculés s'ils ont été supprimés

                // FI 20200128 [25182]
                // isNoAutoPostbackCtrlChanged = true,
                // Signifie qu'un contrôle non autopostback a été modifié par l'utilisateur avant de faire le clic sur le bouton Enregister.
                // => Cela entraine généralement un recalcul des frais (Si le contôle est autre que dans les frais)
                // => Dans ce cas Spheres®, 
                // - annule l'enregistrement en cours 
                // - prend en considération la modification (dans OnPreRender grace à isLoadCcisFromGUI = true plus bas), 
                // - supprime potentiellement les frais (dans OnPreRender) (Si le contôle est autre que dans les frais)
                //-  procède de nouveau à l'enregistrement (via PostBackForValidation)...
                // - lors du nouvel enregistrement Spheres® proposera les frais nouvellement recalculés s'ils ont été supprimés

                // (*) 
                // La condition if au dessus est utile uniquement si la condition isExistNoAutoPostbackCtrlChanged == true est vérifiée
                // Sans cette condition, Spheres® doublerait le procéder qui consiste à annuler l'enregistrement en cours pour republier la page afin de générer l'enregistrement 
                // la doublement se produit puisque isLastPostUpdatePlaceHolder peut passer à true (Cas souvent contaté puisqu'il y a généralement suppression des frais)
                

                bool isNoAutoPostbackCtrlChanged = false;
                string lastNoAutoPostbackCtrlChanged = string.Empty;
                HiddenField hiddenField = HiddenLastNoAutoPostbackCtrlChanged;
                if (null != hiddenField)
                {
                    lastNoAutoPostbackCtrlChanged = hiddenField.Value;
                    isNoAutoPostbackCtrlChanged = StrFunc.IsFilled(lastNoAutoPostbackCtrlChanged);
                }

                // FI 20210621 [XXXXX] On ne considère plus isLastPostUpdatePlaceHolder depuis que le Message de confirmation (Record/Annul) utilise une Dialog JQuery (à la place de window.confirm) 
                //ret = isLastPostUpdatePlaceHolder || isNoAutoPostbackCtrlChanged;
                ret = isNoAutoPostbackCtrlChanged;
                if (ret)
                {
                    System.Diagnostics.Debug.WriteLine("PostBackForValidation __idLastNoAutoPostbackCtrlChanged:{0}, isLastPostUpdatePlaceHolder:{1}", 
                        StrFunc.IsEmpty(lastNoAutoPostbackCtrlChanged) ? "Empty" : lastNoAutoPostbackCtrlChanged, IsLastPostUpdatePlaceHolder);

                    IsLoadCcisFromGUI = isNoAutoPostbackCtrlChanged;

                    Control ctrlContainer = FindControl("tblMenu");
                    Control ctrl = ctrlContainer.FindControl("BtnRecord");
                    string js = ClientScript.GetPostBackEventReference(ctrl, PostBackForValidationArg);
                    JavaScript.ScriptOnStartUp(this, js, "PostBackForValidation");
                }
            }
            return ret;
        }

        /// <summary>
        /// Création d'un togglePanel
        /// </summary>
        /// <param name="pCurrentTable"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Désactivation du tabIndex
        // EG 20211217 [XXXXX] Ajout d'une image matérialisant l'état (ouverture/fermeture) d'un panel avec en-tête, Maintien de cet état avant un post de la page.[XXXXX] Ajout d'une image matérialisant l'état (ouverture/fermeture) d'un panel avec en-tête,
        private WCTogglePanel CreateTogglePanel(Table pCurrentTable)
        {
            #region Color
            Color backgroundColor = Color.Transparent;
            string mainClass = this.CSSMode + " " + InputGUI.MainMenuClassName;
            #endregion Color

            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-mainclass"]))
                mainClass = mainClass.Replace(InputGUI.MainMenuClassName, pCurrentTable.Style["tblH-mainclass"]);

            // Cas de l'instrument
            bool isToggleInstrument = StrFunc.IsFilled(pCurrentTable.Style["tblH-titleF"]) && (pCurrentTable.Style["tblH-titleF"] == "DisplayKey_Instrument");
            if (isToggleInstrument)
                mainClass = mainClass.Replace(InputGUI.MainMenuClassName, InputGUI.CssColor + " toggleinstr");

            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-bkgH-color"]))
                backgroundColor = Color.FromName(pCurrentTable.Style["tblH-bkgH-color"] + "!important");

            #region Title
            string clientId = string.Empty;
            string title = string.Empty;
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-titleR"]))//R Comme Resource
                title = Ressource.GetString(pCurrentTable.Style["tblH-titleR"]);
            else if (StrFunc.IsFilled(pCurrentTable.Style["tblH-titleF"]))
            {
                CustomObject co = new CustomObject
                {
                    Caption = pCurrentTable.Style["tblH-titleF"]//F Comme Function
                };
                // EG 20220406 [XXXXX][WI613] Upd
                title = GetCaptionAndClientIdBanner(co, out clientId, GetTogglePanelItemOccurs(pCurrentTable));
            }
            else if (StrFunc.IsFilled(pCurrentTable.Style["tblH-title"]))
                title = pCurrentTable.Style["tblH-title"];
            else if (StrFunc.IsFilled(pCurrentTable.Style["tblH-titleBlank"]))
                title = Cst.HTMLSpace;
            #endregion Title

            #region GenericControl
            string cssClass = "size3";
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-css"]))
                cssClass = pCurrentTable.Style["tblH-css"];
            #endregion GenericControl

            bool isReverse = false;
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-reverse"]))
                isReverse = BoolFunc.IsTrue(pCurrentTable.Style["tblH-reverse"]);

            if (isReverse && StrFunc.IsEmpty(pCurrentTable.Style["tblH-mainclass"]))
                mainClass = this.CSSMode + " gray";

            WCTogglePanel togglePanel = new WCTogglePanel(backgroundColor, title, cssClass, isReverse)
            {
                CssClass = mainClass
            };

            if (StrFunc.IsFilled(pCurrentTable.ID))
                togglePanel.ID = $"{PrefixTableHID}{pCurrentTable.ID}";
            else
            {
                // FI 20240129 [WI830] S'il n'existe pas mise en place d'un ID unique pour le dv
                // Exemple de div unique : divUID5
                togglePanel.ID = $"{PrefixTableHID}UID{UniqueTableHID}";
                UniqueTableHID++;
            }

            if (StrFunc.IsFilled(clientId))
                togglePanel.ControlHeaderTitle.ID = clientId;

            #region Expand / Link
            string startDisplay = pCurrentTable.Style["tblH-startdisplay"];
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-linkid"]))
            {
                string tableLinkId = pCurrentTable.Style["tblH-linkid"];
                if (StrFunc.IsFilled(tableLinkId) && IsDynamicId)
                    tableLinkId = GetCurrentIntancePrefix() + tableLinkId;
                // FI 20200120 [XXXXX] Appel a AddLinkButtonOpenBanner
                togglePanel.AddLinkButtonOpenBanner(tableLinkId, startDisplay);
                InputGUI.AddLinkButtonOpenBanner(togglePanel.LinkButtonOpenBanner);
            }
            // Gestion du Panel (visible/invisible)
            if (startDisplay == "collapse")
            {
                togglePanel.ControlHeader.CssClass += " closed";
                togglePanel.ControlBody.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            #endregion Expand / Link

            #region ButtonBanner
            bool isLinkButtonBanner = StrFunc.IsFilled(pCurrentTable.Style["tblH-btn-img"]);
            if (isLinkButtonBanner)
            {
                string css = pCurrentTable.Style["tblH-btn-img"];
                WCToolTipLinkButton btn = new WCToolTipLinkButton();
                if (StrFunc.IsFilled(pCurrentTable.Style["tblH-btn-id"]))
                    btn.ID = pCurrentTable.Style["tblH-btn-id"];
                if (StrFunc.IsFilled(pCurrentTable.Style["tblH-btn-key"]))
                    btn.AccessKey = pCurrentTable.Style["tblH-btn-key"];
                btn.CssClass = "fa-icon";
                btn.TabIndex = -1;
                btn.Text = String.Format(@"<i class='{0}'></i>", css);
                btn.Pty.TooltipContent = Ressource.GetString(btn.ID + "ToolTip") + Cst.GetAccessKey(btn.AccessKey);
                btn.CausesValidation = false;
                ControlsTools.RemoveStyleDisplay(btn);
                if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                {
                    //FI 20140206 [19564] Les bouton UIT doivent être affichés en modification sans génération des évènements
                    if (false == btn.ID.StartsWith("btnUTIParty"))
                        ControlsTools.SetStyleList(btn.Style, "display:none");
                }
                else if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
                    ControlsTools.SetStyleList(btn.Style, "display:none");
                else if (
                    (false == Cst.Capture.IsModeUpdate(InputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeNewCapture(InputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeInput(InputGUI.CaptureMode))
                    // FI 20130314 use  false == Cst.Capture.IsModeInput(InputGUI.CaptureMode)
                    // UNDONE 20110222 MF le calculator sera toujour activé pour les tables de calcul des exercises
                    //  "tblExeAssPartyPaymentBlock" is  same string of the table id  defined inside tradeAction.xml  
                    //&& (pCurrentTable.Style["tblH-linkid"] != "tblExeAssPartyPaymentBlock")
                    )
                {
                    ControlsTools.SetStyleList(btn.Style, "display:none");
                }

                string onclick = string.Empty;
                if (StrFunc.IsFilled(pCurrentTable.Style["tblH-btn-blockUI"]))
                {
                    //EG 20120613 BlockUI New
                    string blockUIMessage = Ressource.GetString(pCurrentTable.Style["tblH-btn-blockUI"]);
                    onclick = "Block(" + JavaScript.HTMLBlockUIMessage(this.Page, blockUIMessage) + ");";
                }
                onclick += "PostBack('" + pCurrentTable.Style["tblH-btn-click"] + "');return false;";
                btn.OnClientClick = onclick;
                togglePanel.AddContentHeader(btn, true);
            }
            #endregion ButtonBanner

            #region TableHeader
            if (StrFunc.IsFilled(pCurrentTable.Style["tblH-headerlinkid"]))
            {
                string headerLinkId = pCurrentTable.Style["tblH-headerlinkid"];
                if (StrFunc.IsFilled(headerLinkId) && IsDynamicId)
                    headerLinkId = GetCurrentIntancePrefix() + headerLinkId;
                // FI 20200120 [XXXXX] Appel à SetHeaderControlId
                togglePanel.SetHeaderControlId(headerLinkId);
            }
            #endregion Expand

            #region HelpOnLine
            if (StrFunc.IsFilled(title))
            {
                string helpSchema = string.Empty;
                string helpElement = string.Empty;
                if (StrFunc.IsFilled(pCurrentTable.Style["helpelement"]))
                {
                    if (StrFunc.IsFilled(pCurrentTable.Style["helpschema"]))
                        helpSchema = pCurrentTable.Style["helpschema"];
                    if (StrFunc.IsFilled(pCurrentTable.Style["helpelement"]))
                        helpElement = pCurrentTable.Style["helpelement"];
                    togglePanel.SetHelpToHeaderTitle(this, helpSchema, helpElement);
                }
            }
            #endregion HelpOnLine

            return togglePanel;
        }
        /// <summary>
        /// Recupération du numéro (ItemOccurs) présent sur l'ID du togglePanel
        /// Utilisé pour récupérer les informations des trades (IDENTIFIER/INSTRUMENT) 
        /// affichés dans une facture
        /// </summary>
        /// <param name="pTable"></param>
        /// <returns></returns>
        /// EG 20230406 [WI613] New
        /// EG 20230526 [WI640] Refactoring
        private Nullable<int> GetTogglePanelItemOccurs(Table pTable)
        {
            Nullable<int> itemOccurs = null; 
            if (StrFunc.IsFilled(pTable.ClientID))
            {
                string result = string.Empty;
                MatchCollection matches = Regex.Matches(pTable.ClientID, "[0-9]");
                foreach (Match match in matches) { result += match.Value; }
                if (StrFunc.IsFilled(result))
                    itemOccurs = Convert.ToInt32(result);
            }
            return itemOccurs;
        }
        /// <summary>
        /// Création d'un panel simple
        /// </summary>
        /// <param name="pCurrentTable"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        private WCBodyPanel CreateBodyPanel(Table pCurrentTable)
        {
            #region Color
            Color backgroundColor = Color.Transparent;
            Color borderColor = Color.Transparent;
            string mainClass = this.CSSMode + " " + InputGUI.MainMenuClassName;
            #endregion Color

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-mainclass"]))
                mainClass = mainClass.Replace(InputGUI.MainMenuClassName, pCurrentTable.Style["tblB-mainclass"]);

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-bkg-color"]))
                backgroundColor = Color.FromName(pCurrentTable.Style["tblB-bkg-color"] + "!important");

            if (StrFunc.IsFilled(pCurrentTable.Style["tblB-border-color"]))
                borderColor = Color.FromName(pCurrentTable.Style["tblB-border-color"] + "!important");

            WCBodyPanel bodyPanel = new WCBodyPanel(mainClass, backgroundColor, borderColor)
            {
                CssClass = mainClass
            };
            if (StrFunc.IsFilled(pCurrentTable.ID))
                bodyPanel.ID = PrefixTableHID + pCurrentTable.ID;

            return bodyPanel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        private void WriteCellToRow(TableCell pCell)
        {
            //
            if (null == m_CurrentTable)
                CreateTable(new TableSettings());
            //
            m_CurrentRow.Controls.Add(pCell);
            //
            bool isLastCell = (pCell.ColumnSpan == 999);
            if (isLastCell)
                WriteRowToTable();
        }
        /// <summary>
        /// 
        /// </summary>
        private void WriteRowToTable()
        {
            if (m_CurrentRow.Cells.Count > 0)
            {
                m_CurrentTable.Rows.Add(m_CurrentRow);
                m_CurrentRow = new TableRow();
            }
            else if (m_CurrentHeaderRow.Cells.Count > 0)
            {
                m_CurrentTable.Rows.Add(m_CurrentHeaderRow);
                m_CurrentHeaderRow = new TableHeaderRow();
            }
            else if (m_CurrentFooterRow.Cells.Count > 0)
            {
                m_CurrentTable.Rows.Add(m_CurrentFooterRow);
                m_CurrentFooterRow = new TableFooterRow();
            }

        }

        /// <summary>
        /// Write open banner to HTML.   
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pParentOccurs"></param>
        /// <param name="pControl">Retourne le control DDL lorsque pCustomObject est de type CustomObjectDDLBanner</param>
        /// FI 20121126 [18224] add parameter pControl
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void WriteLinkOpenBanner(ICustomObjectOpenBanner pCustomObject, int pParentOccurs, out Control pControl)
        {
            bool isButtonBanner = pCustomObject.GetType().Equals(typeof(CustomObjectButtonBanner));
            bool isDDLBanner = pCustomObject.GetType().Equals(typeof(CustomObjectDDLBanner));
            pCustomObject.ItemOccurs = pParentOccurs;
            WCLinkButtonOpenBanner imgBanner = null;

            if (pCustomObject.ContainsTableLinkId)
            {
                string tableLinkId = pCustomObject.TableLinkId;
                if (StrFunc.IsFilled(tableLinkId))
                {
                    if (IsDynamicId)
                        tableLinkId = GetCurrentIntancePrefix() + tableLinkId;
                    else if (0 < pParentOccurs)
                        tableLinkId += pParentOccurs.ToString();
                }


                imgBanner = new WCLinkButtonOpenBanner(_placeHolder, tableLinkId, pCustomObject.StartDisplay.ToLower());
                InputGUI.AddLinkButtonOpenBanner(imgBanner);

            }
            //
            WriteBanner((CustomObject)pCustomObject, pCustomObject.LevelIntValue, imgBanner, isButtonBanner, isDDLBanner, out pControl);
        }

        /// <summary>
        /// Write  banner to HTML.   
        /// </summary>
        /// <param name="pCustomObject"></param>
        private void WriteBanner(CustomObjectBanner pCustomObject)
        {
            WriteBanner(pCustomObject, pCustomObject.LevelIntValue, null, false, false, out _);
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pLevel"></param>
        /// <param name="pImgOpenBanner"></param>
        /// <param name="pIsButtonBanner"></param>
        /// <param name="pIsDDLBanner"></param>
        /// <param name="pControl">Retourne le control DDL lorsque pIsDDLBanner = true</param>
        /// FI 20121126 [18224] add parameter pControl
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Désactivation du tabIndex
        private void WriteBanner(CustomObject pCustomObject, int pLevel, LinkButton pImgOpenBanner, bool pIsLinkButtonBanner, bool pIsDDLBanner, out Control pControl)
        {
            pControl = null;
            string caption = GetCaptionAndClientIdBanner(pCustomObject, out string clientId);

            Table bannerTable = new Table();
            TableRow bannerRow = new TableRow();
            TableCell bannerCell = new TableCell();

            #region Get HelpUrl
            string helpUrl = string.Empty;
            if (pCustomObject.ContainsHelpSchema && pCustomObject.ContainsHelpElement && ContainsHelpUrlPath)
                helpUrl = GetCompleteHelpUrl(pCustomObject.HelpSchema, pCustomObject.HelpElement);
            else if (pCustomObject.ContainsHelpUrl && this.ContainsHelpUrlPath)
                helpUrl = GetCompleteHelpUrl(pCustomObject.HelpUrl);
            #endregion Get HelpUrl

            #region Banner Level
            if (1 == pLevel)
            {
                #region Banner Level 1
                bannerTable.Height = Unit.Percentage(100);
                bannerTable.Width = Unit.Percentage(100);
                bannerTable.BorderColor = Color.CornflowerBlue;
                bannerTable.BorderWidth = Unit.Parse("0");
                bannerTable.CellPadding = 0;
                bannerTable.CellSpacing = 0;
                //
                // Divs
                Panel p1 = new Panel
                {
                    CssClass = "banniereCaptureMargin"
                };
                Panel p2 = new Panel
                {
                    CssClass = "banniereCaptureBlock"
                };
                Panel p3 = new Panel
                {
                    CssClass = "banniereButtonBlock"
                };
                Panel p4 = new Panel
                {
                    CssClass = "banniereText"
                };
                Panel p5 = new Panel
                {
                    CssClass = "banniereText"
                };
                if (StrFunc.IsFilled(clientId))
                    p5.ID = clientId;

                if (StrFunc.IsFilled(helpUrl))
                {
                    p5.Style.Add(HtmlTextWriterStyle.Cursor, "help");
                    this.SetQtipHelpOnLine(p5, caption, helpUrl);
                }

                p5.Controls.Add(new LiteralControl(Cst.HTMLSpace + Cst.HTMLSpace + caption));
                p4.Controls.Add(p5);
                p3.Controls.Add(p4);
                p2.Controls.Add(p3);
                p1.Controls.Add(p2);

                TableCell pnlCell = new TableCell();
                pnlCell.Controls.Add(p1);

                TableRow pnlRow = new TableRow();
                pnlRow.Controls.Add(pnlCell);

                // Table
                Table pnlTable = new Table
                {
                    Width = Unit.Percentage(100),
                    BorderWidth = Unit.Parse("0"),
                    CellPadding = 0,
                    CellSpacing = 0
                };
                pnlTable.Controls.Add(pnlRow);
                bannerCell.Controls.Add(pnlTable);
                bannerRow.Controls.Add(bannerCell);
                bannerTable.Controls.Add(bannerRow);
                #endregion Banner Level 1
            }
            else
            {
                #region Banner Level 1<>n
                #region Label
                if (pCustomObject.ContainsCaption)
                {
                    bannerCell = new TableCell
                    {
                        Wrap = false
                    };
                    #region Image for Expand/Collapse banner
                    if (null != pImgOpenBanner)
                        bannerCell.Controls.Add(pImgOpenBanner);
                    #endregion Image for Expand/Collapse banner
                    #region Label
                    WCTooltipLabel lbl = new WCTooltipLabel
                    {
                        Text = caption,
                        BackColor = Color.Transparent,
                        CssClass = pCustomObject.ContainsCssClass ? pCustomObject.CssClass : "lblCaptureReadOnly"
                    };
                    if (StrFunc.IsFilled(clientId))
                        lbl.ID = clientId;
                    if (pCustomObject.ContainsToolTip)
                    {
                        lbl.Pty.TooltipTitle = caption;
                        lbl.Pty.TooltipContent = pCustomObject.ResourceToolTip;
                    }
                    #region Font Size
                    int fontSize;
                    switch (Math.Abs(pLevel))
                    {
                        case 5:
                            fontSize = 6;
                            break;
                        case 4:
                            fontSize = 7;
                            break;
                        case 3:
                            fontSize = 8;
                            break;
                        case 0:
                            fontSize = 9;
                            break;
                        case 2:
                        default:
                            fontSize = 10;
                            break;
                    }
                    lbl.Font.Size = FontUnit.Point(fontSize);
                    #endregion Font Size
                    #region HelpUrl
                    if (StrFunc.IsFilled(helpUrl))
                    {
                        lbl.Style.Add(HtmlTextWriterStyle.Cursor, "help");
                        //lbl.Attributes.Add("onclick", JavaScript.GetWindowOpen(helpUrl, new JavaScript.WindowOpenAttribut(true)));
                        this.SetQtipHelpOnLine(lbl, lbl.Text, helpUrl);
                    }
                    #endregion HelpUrl
                    bannerCell.Controls.Add(lbl);
                    #endregion Label
                    TableTools.WriteSpaceInCell(bannerCell);
                    bannerRow.Controls.Add(bannerCell);
                }
                #endregion Label
                #region LinkButtonBanner
                if (pIsLinkButtonBanner)
                {
                    WCToolTipLinkButton btn = new WCToolTipLinkButton
                    {
                        ID = "btnFees",
                        CssClass = "fa-icon",
                        TabIndex = -1,
                        Text = @"<i class='fas fa-calculator'></i>",
                        AccessKey = "F",
                        CausesValidation = false
                    };
                    btn.Pty.TooltipContent = Ressource.GetString("btnFeesToolTip") + Cst.GetAccessKey(btn.AccessKey);

                    ControlsTools.RemoveStyleDisplay(btn);
                    if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                        ControlsTools.SetStyleList(btn.Style, "display:none");
                    else if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
                        ControlsTools.SetStyleList(btn.Style, "display:none");
                    else if ((false == Cst.Capture.IsModeUpdate(InputGUI.CaptureMode)) &&
                             (false == Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode)))
                        ControlsTools.SetStyleList(btn.Style, "display:none");

                    //Warning: Attention "OnFeesClick" est un mot clé !
                    btn.OnClientClick = "PostBack('" + "OnFeesClick" + "');return false;";

                    bannerCell = new TableCell();
                    bannerCell.Controls.Add(btn);
                    bannerRow.Controls.Add(bannerCell);
                }
                #endregion
                #region DDLBanner
                if (pIsDDLBanner)
                {
                    bannerCell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
                    pControl = bannerCell.Controls[0];
                    bannerRow.Controls.Add(bannerCell);
                }
                #endregion
                #region Hr
                if (1 < pLevel)
                {
                    TableCell cell = new TableCell
                    {
                        Width = Unit.Percentage(100)
                    };
                    #region div
                    HtmlGenericControl div = new HtmlGenericControl("div");
                    div.Attributes.Add("class", "hr");
                    div.Style.Add(HtmlTextWriterStyle.Height, (pLevel == 2 ? "3px" : "1px"));
                    LiteralControl hr = new LiteralControl("<hr/>");
                    if (pCustomObject.ContainsStyle)
                        ControlsTools.SetStyleList(div.Style, pCustomObject.Style);
                    div.Controls.Add(hr);
                    cell.Controls.Add(div);
                    #endregion div
                    bannerRow.Controls.Add(cell);
                }
                #endregion Hr
                bannerTable.Style.Add(HtmlTextWriterStyle.VerticalAlign, "middle");
                bannerTable.Width = Unit.Percentage(100);
                bannerTable.Controls.Add(bannerRow);
                #endregion Banner Level 1<>n
            }
            #endregion Banner Level

            #region Add Banner to Cell
            TableCell cellBanner = new TableCell();
            cellBanner.Controls.Add(bannerTable);
            #region Width
            cellBanner.Width = Unit.Percentage(100);
            //RD 20080923 : j'affecte le Width pour le DropDown
            //if (pCustomObject.ContainsWidth)
            //    cell.Width = pCustomObject.UnitWidth;
            #endregion Width
            #region ColSpan & RowSpan
            if (false == pCustomObject.ContainsColspan)
                pCustomObject.Colspan = "-1";
            if (false == pCustomObject.ContainsRowspan)
                pCustomObject.Rowspan = "1";
            //
            if (pCustomObject.ContainsColspan)
                cellBanner.ColumnSpan = pCustomObject.ColspanIntValue;
            if (pCustomObject.ContainsRowspan)
                cellBanner.RowSpan = pCustomObject.RowspanIntValue;
            #endregion ColSpan & RowSpan
            #endregion Add Banner to Cell

            WriteCellToRow(cellBanner);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        /// EG 20110308 HPC Nb ligne de frais sur facture
        /// EG 20220406 [XXXXX][WI613] New
        private string GetCaptionAndClientIdBanner(CustomObject pCustomObject, out string pClientId)
        {
            return GetCaptionAndClientIdBanner(pCustomObject, out pClientId, null);
        }
        /// EG 20220406 [XXXXX][WI613] New
        private string GetCaptionAndClientIdBanner(CustomObject pCustomObject, out string pClientId, Nullable<int> pDefaultItemOccurs)
        {
            string clientid = string.Empty; // pas de Id sur les banner sauf les DisplayKey
            string caption = pCustomObject.Resource;
            if (caption == DisplayKeyEnum.DisplayKey_Instrument.ToString())
            {
                caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_Instrument);
                clientid = DisplayKeyEnum.DisplayKey_Instrument.ToString();
            }
            else if (caption == DisplayKeyEnum.DisplayKey_Event.ToString())
            {
                caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_Event);
                clientid = DisplayKeyEnum.DisplayKey_Event.ToString();
            }
            else if (caption == DisplayKeyEnum.DisplayKey_EventPosition.ToString())
            {
                caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_EventPosition);
                clientid = DisplayKeyEnum.DisplayKey_EventPosition.ToString();
            }
            else if (caption == DisplayKeyEnum.DisplayKey_InvoiceTrade.ToString())
            {
                // EG 20220406 [XXXXX][WI613] New
                if (pCustomObject is CustomObjectOpenBanner banner)
                {
                    caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_InvoiceTrade, banner.ItemOccurs);
                    clientid = DisplayKeyEnum.DisplayKey_InvoiceTrade.ToString() + pCustomObject.ClientId;
                }
                else if (pDefaultItemOccurs.HasValue)
                {
                    caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_InvoiceTrade, pDefaultItemOccurs.Value);
                }
            }
            else if (caption == DisplayKeyEnum.DisplayKey_InvoiceFee.ToString())
            {
                caption = GetDisplayKey(DisplayKeyEnum.DisplayKey_InvoiceFee);
                clientid = DisplayKeyEnum.DisplayKey_InvoiceFee.ToString();
            }
            pClientId = clientid;
            return caption;
        }

        /// <summary>
        /// Write line (HR) to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteHR(CustomObject pCustomObject, out Control pControl)
        {
            string clientId = pCustomObject.CtrlClientId;
            if (IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteHR() " + clientId);
            //
            #region cell
            TableCell cell = new TableCell
            {
                Width = Unit.Percentage(100)
            };
            if (pCustomObject.ContainsWidth)
                cell.Width = pCustomObject.UnitWidth;
            if (pCustomObject.ContainsHeight)
                cell.Height = pCustomObject.UnitHeight;
            if (false == pCustomObject.ContainsColspan)
                pCustomObject.Colspan = "-1";
            if (false == pCustomObject.ContainsRowspan)
                pCustomObject.Rowspan = "1";
            if (pCustomObject.ContainsColspan)
                cell.ColumnSpan = pCustomObject.ColspanIntValue;
            if (pCustomObject.ContainsRowspan)
                cell.RowSpan = pCustomObject.RowspanIntValue;
            #endregion cell
            //
            #region div
            HtmlGenericControl div = new HtmlGenericControl("div");
            div.Attributes.Add("class", "hr");
            //HtmlGenericControl hr = new HtmlGenericControl("hr");
            LiteralControl hr = new LiteralControl("<hr/>");
            if (pCustomObject.ContainsAttributes)
                ControlsTools.SetAttributesList(div.Attributes, pCustomObject.Attributes);
            if (pCustomObject.ContainsStyle)
                ControlsTools.SetStyleList(div.Style, pCustomObject.Style);
            div.Style.Add(HtmlTextWriterStyle.Height, "1px");
            div.Controls.Add(hr);
            #endregion div

            pControl = div;

            cell.Controls.Add(div);

            WriteCellToRow(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteSpace(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200903 [XXXXX] Correction BUG Intégration du GUID dans paramètre ouverture page(CustomObjectButtonInputMenu)
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        private void WriteButton(CustomObjectButton pCustomObject, out Control pControl)
        {
            string clientId = pCustomObject.CtrlClientId;

            if (IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteButton() " + clientId);
            //
            bool isImageButton = pCustomObject.ContainsImageUrl;
            bool isButOnclick = pCustomObject.ContainsOnClick;
            bool isButFull = pCustomObject.GetType().Equals(typeof(CustomObjectButtonFpmlObject));
            bool isButReferential = pCustomObject.GetType().Equals(typeof(CustomObjectButtonReferential));
            bool isButItemArray = pCustomObject.GetType().Equals(typeof(CustomObjectButtonItemArray));
            bool isButScreenBox = pCustomObject.GetType().Equals(typeof(CustomObjectButtonScreenBox));
            bool isButMenu = pCustomObject.GetType().Equals(typeof(CustomObjectButtonInputMenu));
            bool isLinkButton = (isButFull || isButReferential || isButItemArray || isImageButton);
            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,
                Width = Unit.Pixel(10)
            };
            if (pCustomObject.ContainsWidth)
                cell.Width = pCustomObject.UnitWidth;
            if (pCustomObject.ContainsHeight)
                cell.Height = pCustomObject.UnitHeight;
            if (pCustomObject.ContainsColspan)
                cell.ColumnSpan = pCustomObject.ColspanIntValue;
            if (pCustomObject.ContainsRowspan)
                cell.RowSpan = pCustomObject.RowspanIntValue;

            //ImageButton 
            WebControl ctrl;
            if (isLinkButton)
                ctrl = new WCToolTipLinkButton();
            else if (isImageButton)
                ctrl = new WCToolTipImageButton();
            else
                ctrl = new WCToolTipButton();

            ctrl.ID = clientId;
            if (BoolFunc.IsFalse(SystemSettings.GetAppSettings("Spheres_InputButton_TabActivation")))
                ctrl.TabIndex = -1;

            if (isLinkButton)
            {
                WCToolTipLinkButton button = (WCToolTipLinkButton)ctrl;
                string cssClass = string.Empty;

                if (isImageButton)
                    cssClass = pCustomObject.ImageUrl;
                else if (isButReferential)
                    cssClass = "fas fa-ellipsis-h";
                else if (isButItemArray)
                    cssClass = pCustomObject.CssClass;

                pCustomObject.CssClass = String.Format("fa-icon {0}", pCustomObject.ContainsCaption ? "caption" : "input");
                button.Text = String.Format(@"<i class='{0}'></i> {1}", cssClass, pCustomObject.ContainsCaption ? " " + pCustomObject.Caption : string.Empty);

                button.Pty.TooltipContent = pCustomObject.ResourceToolTip + Cst.GetAccessKey(button.AccessKey);
                if (pCustomObject.ContainsToolTipTitle)
                    button.Pty.TooltipTitle = pCustomObject.ResourceToolTipTitle;

                if (IsDebugClientId)
                    button.ToolTip = ctrl.ID;
                button.CausesValidation = false;
            }
            else if (isImageButton)
            {
                WCToolTipImageButton imgButton = (WCToolTipImageButton)ctrl;
                imgButton.ImageAlign = ImageAlign.Left;
                imgButton.ImageUrl = pCustomObject.ImageUrl;
                imgButton.Pty.TooltipContent = pCustomObject.ResourceToolTip + Cst.GetAccessKey(imgButton.AccessKey);
                if (pCustomObject.ContainsToolTipTitle)
                    imgButton.Pty.TooltipTitle = pCustomObject.ResourceToolTipTitle;

                imgButton.CausesValidation = false;
                if (IsDebugClientId)
                    imgButton.ToolTip = ctrl.ID;
            }
            else
            {
                WCToolTipButton button = (WCToolTipButton)ctrl;
                if (false == CSS.IsCssClassMain(pCustomObject.CssClass))
                    button.Text = pCustomObject.Resource;
                if (pCustomObject.ContainsMisc)
                {
                    int padleft = Convert.ToInt32(pCustomObject.GetMiscValue("spacepadleft"));
                    if ((padleft) > 0)
                        button.Text = Cst.Space.PadLeft(padleft, Convert.ToChar(Cst.Space)) + button.Text;
                }

                button.Pty.TooltipContent = pCustomObject.ResourceToolTip + Cst.GetAccessKey(button.AccessKey);
                if (pCustomObject.ContainsToolTipTitle)
                    button.Pty.TooltipTitle = pCustomObject.ResourceToolTipTitle;

                if (IsDebugClientId)
                    button.ToolTip = ctrl.ID;
                button.CausesValidation = false;
            }

            if (pCustomObject.ContainsCssClass)
                ctrl.CssClass = pCustomObject.CssClass;

            if (pCustomObject.ContainsStyle)
                ControlsTools.SetStyleList(ctrl.Style, pCustomObject.Style);

            ctrl.Visible = true;
            if (isButReferential)
            {
                // EG 20120620 DisplayMode attribute New
                if (pCustomObject.DisplayModeValue.HasValue)
                    ctrl.Visible = (pCustomObject.DisplayModeValue.Value == InputGUI.CaptureMode);
                else
                    ctrl.Visible = (false == IsControlModeConsult(pCustomObject));
            }

            if (isButReferential)
            {
                CustomObjectButtonReferential co = (CustomObjectButtonReferential)pCustomObject;
                ControlsTools.SetAttributeOnCLickButtonReferential(co, ctrl);
                InputGUI.AddCustomObjectButtonReferential(co);
            }
            else if (isButFull)
            {
                CustomObjectButtonFpmlObject co = (CustomObjectButtonFpmlObject)pCustomObject;
                ControlsTools.SetAttributeOnClickOnControlZoomFpmlObject(co, ctrl);
            }
            else if (isButItemArray)
            {
                CustomObjectButtonItemArray co = (CustomObjectButtonItemArray)pCustomObject;
                SetAttributeOnClickOnButtonItemArray(co, ctrl);
            }
            else if (isButScreenBox)
            {
                CustomObjectButtonScreenBox co = (CustomObjectButtonScreenBox)pCustomObject;
                ControlsTools.SetAttributeOnClickOnButtonScreenBox(co, ctrl);
            }
            else if (isButMenu)
            {
                CustomObjectButtonInputMenu co = (CustomObjectButtonInputMenu)pCustomObject;
                ControlsTools.SetAttributeOnClickOnButtonMenu(this, co, ctrl);
            }
            else if (isButOnclick)
            {
                ControlsTools.SetAttributeOnClickOnButton(this.Page, pCustomObject, ctrl);
            }
            ctrl.Attributes.Add("isLockedModifyPostEvts", pCustomObject.IsLockedModifyPostEvts.ToString());
            ctrl.Attributes.Add("isLockedModifyFeesUninvoiced", pCustomObject.IsLockedModifyFeesUninvoiced.ToString());
            ctrl.Attributes.Add("isLockedAllocatedInvoice", pCustomObject.IsLockedAllocatedInvoice.ToString());

            cell.Controls.Add(ctrl);

            pControl = ctrl;

            WriteCellToRow(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <returns></returns>
        private TableCell WriteCell(CustomObject pCustomObject)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            //
            Label lbl = null;
            if (ArrFunc.IsFilled(cell.Controls))
            {
                for (int i = 0; i < cell.Controls.Count; i++)
                {
                    if (cell.Controls[i].GetType().Equals(typeof(Label)))
                    {
                        lbl = (Label)cell.Controls[i];
                        break;
                    }
                }
            }
            if (null != lbl)
            {
                if (pCustomObject.Caption == DisplayKeyEnum.DisplayKey_Event.ToString())
                    lbl.Text = GetDisplayKey(DisplayKeyEnum.DisplayKey_Event);
            }
            return cell;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteCellHeader(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = WriteCell(pCustomObject);
            pControl = cell;
            WriteCellToHeaderRow(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteCellFooter(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = WriteCell(pCustomObject);
            pControl = cell;
            WriteCellToFooterRow(cell);
        }

        /// <summary>
        /// Write Label webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteLabel(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            //
            Label lbl = null;
            if (ArrFunc.IsFilled(cell.Controls))
            {
                for (int i = 0; i < cell.Controls.Count; i++)
                {
                    if (cell.Controls[i].GetType().Equals(typeof(Label)))
                    {
                        lbl = (Label)cell.Controls[i];
                        break;
                    }
                }
            }
            
            if (null != lbl)
            {
                if (pCustomObject.Caption == DisplayKeyEnum.DisplayKey_Event.ToString())
                    lbl.Text = GetDisplayKey(DisplayKeyEnum.DisplayKey_Event);
            }
            
            pControl = lbl;

            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write DropDown webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteDropDown(CustomObjectDropDown pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write CheckBox webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteCheckBox(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write CheckBox HtmlInputCheckBox to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteHtmlInputCheckBox(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write divider (vertical line) to HTML.
        /// </summary>
        /// <newpara>Description : Write divider (vertical line) to HTML.</newpara>
        /// <param name="pCustomObject"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteDivider(CustomObject pCustomObject)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write Image webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteImage(CustomObjectImage pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write TextBox webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteTextBox(CustomObjectTextBox pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        /// <summary>
        /// Write Timestamp webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// EG 20170918 [23342] New
        private void WriteTimestamp(CustomObjectTimestamp pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }


        /// <summary>
        /// Write HtmlInputHidden webcontrol to HTML.
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteHiddenBox(CustomObject pCustomObject, out Control pControl)
        {
            string clientId = pCustomObject.CtrlClientId;
            if (IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WriteHiddenBox() " + clientId);

            HtmlInputHidden hidden = new HtmlInputHidden
            {
                ID = clientId
            };

            pControl = hidden;

            PlaceHolder.Controls.Add(hidden);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteHyperlink(CustomObject pCustomObject, out Control pControl)
        {
            TableCell cell = pCustomObject.WriteCell(this, IsControlModeConsult(pCustomObject));
            pControl = cell.Controls[0];
            WriteCellToRow(cell);
        }

        private void WritePanel(CustomObjectPanel pCustomObject, out Control pControl)
        {
            string clientId = pCustomObject.CtrlClientId;

            if (IsDebugDesign)
                System.Diagnostics.Debug.WriteLine("WritePanel() " + clientId);

            TableCell cell = new TableCell
            {
                HorizontalAlign = HorizontalAlign.Left,
                Wrap = false,
                Width = Unit.Pixel(10)
            };
            if (pCustomObject.ContainsWidth)
                cell.Width = pCustomObject.UnitWidth;
            if (pCustomObject.ContainsHeight)
                cell.Height = pCustomObject.UnitHeight;
            if (pCustomObject.ContainsColspan)
                cell.ColumnSpan = pCustomObject.ColspanIntValue;
            if (pCustomObject.ContainsRowspan)
                cell.RowSpan = pCustomObject.RowspanIntValue;

            WCToolTipPanel ctrl = new WCToolTipPanel
            {
                ID = clientId
            };
            if (pCustomObject.ContainsMisc)
            {
                ctrl.Pty.TooltipContent = pCustomObject.ResourceToolTip;
                if (pCustomObject.ContainsToolTipTitle)
                    ctrl.Pty.TooltipTitle = pCustomObject.ResourceToolTipTitle;

                if (IsDebugClientId)
                    ctrl.ToolTip = ctrl.ID;
            }
            if (pCustomObject.ContainsCssClass)
                ctrl.CssClass = pCustomObject.CssClass;

            if (pCustomObject.ContainsStyle)
                ControlsTools.SetStyleList(ctrl.Style, pCustomObject.Style);

            ctrl.Visible = true;
            cell.Controls.Add(ctrl);
            pControl = ctrl;
            WriteCellToRow(cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pColspan"></param>
        private static int GetColspan(string pColspan)
        {
            int columnSpan = 1;
            if (StrFunc.IsFilled(pColspan))
            {
                columnSpan = IntFunc.IntValue2(pColspan);
                bool isLastCell = (columnSpan == -1);
                if (isLastCell)
                    columnSpan = 999;
            }
            return columnSpan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRowspan"></param>
        private static int GetRowspan(string pRowspan)
        {
            int rowSpan = 1;
            if (StrFunc.IsFilled(pRowspan))
                rowSpan = IntFunc.IntValue2(pRowspan);
            return rowSpan;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteRowHeaderToTable(CustomObject pCustomObject, out Control pControl)
        {
            m_CurrentHeaderRow = new TableHeaderRow
            {
                CssClass = "rowHeader"
            };
            if (pCustomObject.ContainsStyle)
                ControlsTools.SetStyleList(m_CurrentHeaderRow.Style, pCustomObject.Style);
            if (pCustomObject.ContainsCssClass)
                m_CurrentHeaderRow.CssClass += " " + pCustomObject.CssClass;
            m_CurrentHeaderRow.TableSection = TableRowSection.TableHeader;

            pControl = m_CurrentHeaderRow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCustomObject"></param>
        /// <param name="pControl"></param>
        /// FI 20121126 [18224] add parameter pControl
        private void WriteRowFooterToTable(CustomObject pCustomObject, out Control pControl)
        {
            m_CurrentFooterRow = new TableFooterRow
            {
                CssClass = "rowFooter",
                TableSection = TableRowSection.TableFooter
            };
            if (pCustomObject.ContainsStyle)
                ControlsTools.SetStyleList(m_CurrentFooterRow.Style, pCustomObject.Style);
            if (pCustomObject.ContainsCssClass)
                m_CurrentFooterRow.CssClass += " " + pCustomObject.CssClass;
            pControl = m_CurrentFooterRow;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        private void WriteCellToHeaderRow(TableCell pCell)
        {
            //
            if (null == m_CurrentTable)
                CreateTable(new TableSettings());
            m_CurrentHeaderRow.Controls.Add(pCell);
            //
            bool isLastCell = (pCell.ColumnSpan == 999);
            if (isLastCell)
                WriteRowToTable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        private void WriteCellToFooterRow(TableCell pCell)
        {
            //
            if (null == m_CurrentTable)
                CreateTable(new TableSettings());
            m_CurrentFooterRow.Controls.Add(pCell);
            //
            bool isLastCell = (pCell.ColumnSpan == 999);
            if (isLastCell)
                WriteRowToTable();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pControl"></param>
        private static void SetAttributeOnClickOnButtonItemArray(CustomObjectButtonItemArray pCo, WebControl pControl)
        {
            bool isOk = (null != pControl) && (null != pCo) && pCo.IsOk;
            //			
            if (isOk)
            {
                // Onclick
                string args = pCo.Prefix + ";" + pCo.OperatorType + ";" + pCo.XPath;
                args = JavaScript.JSValidString(args);
                string onclientClick = "PostBack('" + "OnAddOrDeleteItem" + "','" + args + "');return false;";
                //20090421 FI/EG Add WCToolTipImageButton && WCToolTipButton
                ControlsTools.SetOnClientClick(pControl, onclientClick);
            }
        }

        #endregion methods
    }
    
}
