using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.Rights;
using System;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    public class PageBaseTemplate : PageBase
    {
        #region public Constants
        //Control ID
        public const string CtrlFooterLeft = "footerLeft";
        public const string CtrlFooterRight = "footerRight";
        public const string DDL_COLUMNNAME = "ddlColumnName";
        public const string DDL_OPERATOR = "ddlOperator";
        public const string TXT_VALUE = "txtValue";
        public const string TXT_IDVALUE = "txtIDValue";
        #endregion public Constants
        //
        #region Members
        //public string regExString;     

        /// <summary>
        /// Représente la consultation courante
        /// </summary>
        public string idLstConsult;
        /// <summary>
        /// Représente la modèle courant de la consultation
        /// </summary>
        public string idLstTemplate;
        /// <summary>
        /// Représente le propriétaire du modèle courant
        /// </summary>
        public int idA;

        public string urlThis;
        public string dataFrom__EVENTTARGET;
        public string dataFrom__EVENTARGUMENT;
        public bool isClose;
        public bool isReloadPage;
        
        public bool isAllowedToOverWrite = true;
        public LstConsult ThisConsultation;
        private string _parentGUID;
        private string _idMenu;
        protected string defaultButton;
        //
        #endregion
        //
        #region Accessors
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected string MainClass
        {
            get
            {
                return this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual string SpecificSubTitle
        {
            get { return string.Empty; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200527 [XXXXX] 
        protected virtual string PageName
        {
            get { throw new NotSupportedException("PageName must be overridden"); }
        }
        /// <summary>
        /// 
        /// </summary>
        protected virtual PlaceHolder PlhMain
        {
            get { return null; }
        }


        /// <summary>
        /// Obtient l'identifiant unique GUID de la page List.aspx 
        /// </summary>
        protected string ParentGUID
        {
            get { return _parentGUID; }
        }


        /// <summary>
        /// Obtient l'identifiant IDMenu de la page List.aspx 
        /// </summary>
        protected string IdMenu
        {
            get { return _idMenu; }
        }

        #endregion

        #region constructor
        public PageBaseTemplate()
        {

        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("PageBaseTemplate.OnInit", true);

            InitializeComponent();
            base.OnInit(e);
            AntiForgeryControl();
            dataFrom__EVENTTARGET = string.Empty;
            dataFrom__EVENTARGUMENT = string.Empty;
            AbortRessource = true;
            //20071212 FI Ticket 16012 => Migration Asp2.0
            //SmartNavigation = false;// WARNING: necessaire sinon erreur 'Pointeur invalide' lors de post-back		
            //              
            //Getting IDLSTCONSULT, IDA, IDLSTTEMPLATE from queryString, and setting ViewState custom datas				
            //Consultation courante
            idLstConsult = Server.UrlDecode(Request.QueryString["C"]);
            //Nom du modèle 
            idLstTemplate = Server.UrlDecode(Request.QueryString["T"]);
            //Propriétaire du modèle
            idA = IntFunc.IntValue(Request.QueryString["A"]);
            if (idA == 0)
                idA = SessionTools.User.IdA;
            //                
            _idMenu = Request.QueryString["IDMenu"];
            _parentGUID = Request.QueryString["ParentGUID"];
            //
            dataFrom__EVENTTARGET = Request.Params["__EVENTTARGET"];
            dataFrom__EVENTARGUMENT = Request.Params["__EVENTARGUMENT"];
            isReloadPage = (dataFrom__EVENTTARGET == "PAGE" && dataFrom__EVENTARGUMENT == "RELOAD");
            //
            ThisConsultation = new LstConsult(SessionTools.CS, idLstConsult, string.Empty);
            string qT1 = Request.QueryString["T1"];
            //header
            string leftTitle;
            //20090603 PL Add UrlDecode()
            //qT1 = HttpUtility.UrlDecode(qT1, System.Text.Encoding.Default);
            if (StrFunc.IsEmpty(qT1))
                leftTitle = Ressource.GetString("Consultation", true) + ": " + HtmlTools.HTMLBold(ThisConsultation.Title);
            else
                leftTitle = qT1 + ": " + HtmlTools.HTMLBold(HttpUtility.UrlDecode(Request.QueryString["T2"], System.Text.Encoding.UTF8));
            //
            PageTitle = HtmlTools.HTMLBold_Remove(leftTitle);
            //
            if (StrFunc.IsFilled(SpecificSubTitle))
                PageTitle += " - " + SpecificSubTitle;
            //
            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle, SpecificSubTitle);
            if (ReferentialWeb.IsReferential(ThisConsultation.IdLstConsult))
            {
                string ObjectName = ThisConsultation.ReferentialShortIdConsult();
                string particularTitle = ReferentialTools.ParticularTitle(IdMenu, ObjectName);
                if (StrFunc.IsFilled(particularTitle))
                    titleLeft.Title += @"\" + particularTitle;
            }
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu));
            PlhMain.Controls.Add(pnlHeader);

            CreateControls();

            //PL 20121123 New
            PageTools.SetBody(this, false, null);
            AddInputHiddenDeFaultControlOnEnter();
            HiddenFieldDeFaultControlOnEnter.Value = defaultButton;

            // FI 20200217 [XXXXX] Call AddInputHiddenGUID
            AddInputHiddenGUID();

            AddAuditTimeStep("PageBaseTemplate.OnInit", true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("PageBaseTemplate.OnPreRender", true);

            if ((!IsPostBack) || isReloadPage)
                LoadDataControls();

            DataBind();

            base.OnPreRender(e);
            
            AddAuditTimeStep("PageBaseTemplate.OnPreRender", false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override void LoadViewState(object savedState)
        {
            object[] viewState = (object[])savedState;
            base.LoadViewState(viewState[0]);
            idLstConsult = (string)viewState[1];
            idLstTemplate = (string)viewState[2];
            idA = (int)viewState[3];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override object SaveViewState()
        {
            if (HttpContext.Current == null)
                return null;

            object[] ret = new object[4];
            ret[0] = base.SaveViewState();
            ret[1] = idLstConsult;
            ret[2] = idLstTemplate;
            ret[3] = idA;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PageLoad(object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void CreateControls() { }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void LoadDataControls() { }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void SaveData() { }

        /// <summary>
        /// 
        /// </summary>
        protected void SaveTemplate(string pInitialNameLstTemplate, string pNewNameLstTemplate)
        {

            //on verifie les droits du user sur ce template
            bool hasUserRightSave = ThisConsultation.template.HasUserRight(SessionTools.CS, SessionTools.User, RightsTypeEnum.SAVE);
            //
            //Template temporaire et identifiant inchangé et (user est propriétaire ou disposant des droits pour enregister les modifs d'un template qui n'est pas le sien)
            if (ThisConsultation.template.IsTemporary && (pNewNameLstTemplate == pInitialNameLstTemplate)
                && (hasUserRightSave || ThisConsultation.template.IsUserOwner(SessionTools.Collaborator_IDA)))
            {
                //il s'agit d'un [SAVE] : on lui enlève son statut 'temporaire'
                ThisConsultation.template.Update(SessionTools.CS);
                ThisConsultation.DuplicateTemplate(SessionTools.CS, ThisConsultation.template.IDLSTTEMPLATE, ThisConsultation.template.IDA,
                    pNewNameLstTemplate, ThisConsultation.template.IDA, true);

                ThisConsultation.template.IDLSTTEMPLATE = pNewNameLstTemplate;
            }
            else
            {
                //Si l'identifiant a été modifié depuis l'ouverture ou ( si l'utilisateur n'en est pas le propriétaire et qu'il n'a pas les droits pour sauver )
                if ((pNewNameLstTemplate != pInitialNameLstTemplate) ||
                    (!hasUserRightSave && !ThisConsultation.template.IsUserOwner(SessionTools.Collaborator_IDA)))
                {
                    //il s'agit d'un [SAVE AS]
                    bool isTemporary = ThisConsultation.template.IsTemporary;
                    string sourceIdLstTemplate;
                    if (isTemporary)
                        sourceIdLstTemplate = ThisConsultation.template.IDLSTTEMPLATE;
                    else
                        sourceIdLstTemplate = pInitialNameLstTemplate;

                    ThisConsultation.template.IDLSTTEMPLATE = pNewNameLstTemplate;

                    bool alreadyExists = false;
                    //si on ne passe pas apres une demande confirmation pour overwrite (postback de confirm()), on verifie si le template existe
                    if (!isAllowedToOverWrite)
                        alreadyExists = ReferentialWeb.ExistsTemplate(SessionTools.CS, idLstConsult, ThisConsultation.template.IDLSTTEMPLATE, SessionTools.Collaborator_IDA);
                    //
                    //si le template existe deja; confirmation d'ecrasement
                    if (alreadyExists)
                    {
                        //idLstTemplateInitial = initialNameLstTemplate;
                        string msgConfirm;
                        msgConfirm = Ressource.GetStringForJS("Msg_AlreadyExistsTemplateOverWrite");
                        msgConfirm = msgConfirm.Replace("{0}", ThisConsultation.template.IDLSTTEMPLATE);
                        //20070330 PL JavaScript.ConfirmOnLoad(this, (isClose ? "CONFIRMSAVEQUIT":"CONFIRMSAVE"), msgConfirm);
                        JavaScript.ConfirmOnStartUp(this, (isClose ? "CONFIRMSAVEQUIT" : "CONFIRMSAVE"), msgConfirm);
                        //
                        return;
                    }
                    //sinon on l'insere et on ecrase le cas echeant
                    else
                    {
                        ThisConsultation.InsertOverWriteTemplateWithCopyChildsFrom(SessionTools.CS, sourceIdLstTemplate, ThisConsultation.template.IDA);
                        if (isTemporary)
                        {
                            //Suppression du template temporaire
                            //ThisConsultation.template.Delete(CS, idLstConsult, sourceIdLstTemplate, ThisConsultation.template.IDA, true);
                            ReferentialWeb.Delete(SessionTools.CS, idLstConsult, sourceIdLstTemplate, ThisConsultation.template.IDA, true);
                        }
                    }


                }
                else
                    //Sinon il s'agit d'un [UPDATE]
                    ThisConsultation.template.Update(SessionTools.CS);
            }
            idLstTemplate = ThisConsultation.template.IDLSTTEMPLATE;
            idA = ThisConsultation.template.IDA;
            //            
            ReferentialWeb.WriteTemplateSession(idLstConsult, idLstTemplate, idA, ParentGUID);
        }
        /// <summary>
        /// Création puis chargement d'un template temporaire créer à partir du template existant 
        /// <para>Cette action est menée uniquement si le template n'est pas déjà un template temporaire</para>
        /// </summary>
        /// FI 20201209 [XXXXX] Add
        protected void CreateTemporaryTemplate()
        {
            //if this consultation is not already a temporary copy, create a temporary copy of the template and load it   
            if (!ThisConsultation.template.IsTemporary)
            {
                // creation d'un modèle temporaire
                // FI 20200602 [25370] Appel à la méthode CreateCopyTemporaryTemplate
                Pair<string, int> retCreateCopyTemporaryTemplate = ThisConsultation.CreateCopyTemporaryTemplate(SessionTools.CS);
                idLstTemplate = retCreateCopyTemporaryTemplate.First;
                idA = retCreateCopyTemporaryTemplate.Second;

                //Load the temporary copy
                ThisConsultation.template.Load(SessionTools.CS, idLstConsult, idLstTemplate, idA);
            }
        }

        #region protected virtual Reset
        protected virtual void Reset() { }
        #endregion
        //
        #region AddToolBar
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual Panel AddToolBar()
        {
            Panel pnlToolBar = new Panel
            {
                ID = "divtoolbar",
                CssClass = MainClass
            };
            PlhMain.Controls.Add(pnlToolBar);
            return pnlToolBar;
        }
        #endregion CreateToolBar

        #region public AddButtonApply
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonApply()
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnApply", "fa fa-check", true);
            btn.CausesValidation = true;
            btn.Click += new EventHandler(this.ButApply_Click);
            return btn;
        }
        #endregion public AddButtonApply
        #region public AddButtonOK
        //PL 20121123 void to string
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonOk()
        {
            return AddButtonOk(null);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonOk(string pOnclick)
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-check", true);
            btn.CausesValidation = true;
            if (pOnclick != null)
                btn.OnClientClick = pOnclick;
            btn.Click += new EventHandler(this.ButOK_Click);
            return btn;
        }
        #endregion
        #region public AddButtonOkSave
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonOkAndSave()
        {
            return AddButtonOkAndSave(null);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonOkAndSave(string pOnclick)
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnOkAndSave", "fa fa-save", true);
            btn.CausesValidation = true;
            if (pOnclick != null)
                btn.OnClientClick = pOnclick;
            btn.Click += new EventHandler(this.ButOKAndSave_Click);
            return btn;
        }
        #endregion
        #region public AddButtonCancel
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonCancel()
        {
            return ControlsTools.GetAwesomeButtonCancel(true);
        }
        #endregion
        #region public AddButtonOkQuery
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonOkQuery()
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-check", true);
            btn.Click += new EventHandler(this.ButOKQuery_Click);
            return btn;
        }
        #endregion
        #region public AddButtonReset
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public WCToolTipLinkButton AddButtonReset()
        {
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnReset", "far fa-window-restore", true);
            btn.CausesValidation = true;
            btn.Click += new EventHandler(this.ButReset_Click);
            return btn;
        }
        #endregion
        //
        #region protected NewLabel
        protected Label NewLabel(string pID)
        {
            Label lbl = new Label
            {
                ID = pID
            };
            if (pID.StartsWith(Cst.LBL))
                pID = "lbl" + pID.Substring(Cst.LBL.Length);
            lbl.Text = Ressource.GetString(pID);
            return lbl;
        }
        #endregion
        #region protected AddSpace
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void AddSpace(Control ctrl)
        {
            Label lblSpace = new Label
            {
                Text = Cst.HTMLSpace
            };
            ctrl.Controls.Add(lblSpace);
        }
        #endregion
        #region protected AddHR
        protected void AddHR(PlaceHolder ctrl)
        {
            LiteralControl ltrHR = new LiteralControl
            {
                Text = Cst.HTMLHorizontalLine
            };
            ctrl.Controls.Add(ltrHR);
        }
        #endregion
        //
        #region protected SetTextBoxData
        protected void SetTextBoxData(string pCtrlID, string data)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pCtrlID);
            if (ctrlFound != null)
                ((WCTextBox)ctrlFound).Text = data;
        }
        #endregion
        #region protected GetTextBoxData
        protected string GetTextBoxData(string pCtrlID)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pCtrlID);
            if (ctrlFound != null)
                return ((WCTextBox)ctrlFound).Text;
            else
                return string.Empty;
        }
        #endregion
        #region protected SetTextBoxEnabled
        protected void SetTextBoxEnabled(string pCtrlID, bool isEnabled)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pCtrlID);
            if (ctrlFound != null)
                ((WCTextBox)ctrlFound).Enabled = isEnabled;
            return;
        }
        #endregion
        //
        #region protected NewRowDDLCapture
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel NewRowDDLCapture(DropDownList pDDL, string pID)
        {
            Panel pnl = new Panel();
            Label lbl = NewLabel(Cst.LBL + pID.Substring(3));
            pnl.Controls.Add(lbl);

            pDDL.ID = pID;
            pDDL.CssClass = EFSCssClass.DropDownListCapture;
            pnl.Controls.Add(pDDL);
            return pnl;
        }
        #endregion

        #region protected AddDDLData
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210330 [XXXXX] Gestion message si pas de template
        public Panel AddDDLData(string pID, bool pIsAutoPostBack)
        {
            Panel pnl = new Panel();
            pnl.Attributes.Add("style", "display:flex;");

            Label lblLabel = NewLabel(Cst.LBL + pID.Substring(3));
            lblLabel.Width = Unit.Pixel(100);
            pnl.Controls.Add(lblLabel);

            DropDownList ddl = new DropDownList
            {
                Width = Unit.Pixel(300),
                ID = pID,
                CssClass = EFSCssClass.DropDownListCapture,

                AutoPostBack = pIsAutoPostBack,
                EnableViewState = true
            };
            pnl.Controls.Add(ddl);

            if (pID.Contains("Query"))
            {
                Label lblMessage = NewLabel(Cst.LBL + pID.Substring(3) + "_MSG");
                lblMessage.Width = Unit.Pixel(300);
                lblMessage.Text = Ressource.GetString("Msg_NoTemplate");
                pnl.Controls.Add(lblMessage);
            }
            return pnl;
        }
        #endregion
        #region protected NewRowDDLRight
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel NewRowDDLRight(string pID, int pLabelWidth)
        {
            Panel pnl = new Panel();
            Label lbl = NewLabel(Cst.LBL + pID.Substring(3));

            if (pLabelWidth != 0)
                lbl.Width = pLabelWidth;
            pnl.Controls.Add(lbl);
            DropDownList ddlRight = new DropDownList();
            ControlsTools.DDLLoad_RightType(ddlRight);
            ddlRight.CssClass = EFSCssClass.DropDownListCapture;
            ddlRight.ID = pID;
            ddlRight.EnableViewState = true;
            pnl.Controls.Add(ddlRight);

            return pnl;
        }
        #endregion
        #region protected NewRowCheckbox
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel NewRowCheckbox(bool pIsWithLeftLabel, string pID, params WebControl[] pWebControl)
        {
            Panel pnl = new Panel();

            if (pIsWithLeftLabel)
            {
                Label lbl = NewLabel(Cst.LBL + pID.Substring(3));
                lbl.Text = Ressource.GetString(pID, true);
                pnl.Controls.Add(lbl);
            }

            CheckBox chk = new CheckBox
            {
                ID = pID,
                Text = pIsWithLeftLabel ? string.Empty : Ressource.GetString(pID, true),
                EnableViewState = true
            };
            pnl.Controls.Add(chk);

            if (null != pWebControl)
            {
                foreach (WebControl wc in pWebControl)
                {
                    if (wc != null)
                        pnl.Controls.Add(wc);
                }
            }

            return pnl;
        }
        #endregion
        #region protected AddRadioButton
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel AddRadioButton(string pID, string groupeName)
        {
            Panel pnl = new Panel();
            RadioButton rb = new RadioButton
            {
                ID = pID,
                GroupName = groupeName,
                Text = Ressource.GetString(pID, true),
                EnableViewState = true
            };
            pnl.Controls.Add(rb);
            return pnl;
        }
        #endregion
        #region NewCapture2
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected Panel NewRowCapture2(string pID, bool pIsCaptureVisible = false, bool pIsOptional = false, bool pIsMultiLine = false, int pLabelWidth = 0, int pDataWidth = 0, string pRegEx = null)
        {
            Panel pnl = new Panel();

            Label lbl = NewLabel(Cst.LBL + pID.Substring(3));
            lbl.Visible = pIsCaptureVisible;

            if (pLabelWidth != 0)
                lbl.Width = pLabelWidth;

            pnl.Controls.Add(lbl);

            WCTextBox txt = new WCTextBox(pID, (pIsOptional ? string.Empty : Ressource.GetString("ISMANDATORY", true)), pRegEx, true, Ressource.GetString("ISMANDATORY", true))
            {
                Visible = pIsCaptureVisible
            };

            if (pDataWidth != 0)
                txt.Width = pDataWidth;

            if (pIsMultiLine)
                txt.TextMode = TextBoxMode.MultiLine;

            if (pIsMultiLine)
                txt.CssClass = (pIsOptional?EFSCssClass.CaptureMultilineOptional:EFSCssClass.CaptureMultiline);
            else
            {
                txt.CssClass = (pIsOptional ? EFSCssClass.CssClassEnum.txtCaptureOptional.ToString() : EFSCssClass.CssClassEnum.txtCapture.ToString());
            }

            pnl.Controls.Add(txt);
            pnl.Visible = pIsCaptureVisible;

            return pnl;
        }
        #endregion
        //	
        #region protected SetDDLData
        public void SetDDLData(string pID, string data)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null && StrFunc.IsFilled(data) && ((DropDownList)ctrlFound).Items.Count > 0)
                ((DropDownList)ctrlFound).SelectedValue = data;
        }
        #endregion
        #region protected GetDDLData
        /// <summary>
        /// Obtient SelectedValue du contrôle tel que ID = {pID}
        /// <para>Obteint string.Empty si le contôle n'existe pas</para>
        /// </summary>
        /// <param name="pID"></param>
        /// <returns></returns>
        protected string GetDDLData(string pID)
        {
            Control ctrlFound = PlhMain.FindControl(pID);

            if (null != ctrlFound)
                return ((DropDownList)ctrlFound).SelectedValue;
            else
                return string.Empty;
        }
        #endregion
        #region protected SetDDLEnabled
        protected void SetDDLEnabled(string pID, bool isEnabled)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                ((DropDownList)ctrlFound).Enabled = isEnabled;
            return;
        }
        #endregion
        //
        #region protected SetCheckBoxData
        protected void SetCheckBoxData(string pID, bool pData)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                ((CheckBox)ctrlFound).Checked = pData;
        }
        #endregion
        #region protected GetCheckBoxData
        protected bool GetCheckBoxData(string pID)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                return ((CheckBox)ctrlFound).Checked;
            else
                return false;
        }
        #endregion
        #region protected SetCheckBoxEnabled
        protected void SetCheckBoxEnabled(string pID, bool pIsEnabled)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                ((CheckBox)ctrlFound).Enabled = pIsEnabled;
            return;
        }
        #endregion
        //
        #region protected SetRadioButtonData
        protected void SetRadioButtonData(string pID, bool data)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                ((RadioButton)ctrlFound).Checked = data;
        }
        #endregion
        #region protected GetRadioButtonData
        protected bool GetRadioButtonData(string pID)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                return ((RadioButton)ctrlFound).Checked;
            else
                return false;
        }
        #endregion
        #region protected SetRadioButtonEnabled
        protected void SetRadioButtonEnabled(string pID, bool isEnabled)
        {
            Control ctrlFound;
            ctrlFound = PlhMain.FindControl(pID);
            if (ctrlFound != null)
                ((RadioButton)ctrlFound).Enabled = isEnabled;
        }
        #endregion

        #region protected SetFooter
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected Panel SetFooter(string pFooterLeft, string pFooterRight, string pPrefixForCtrl)
        {
            HtmlPageTitle titleLeft = new HtmlPageTitle(null, null, pFooterLeft, Color.Black,
                pPrefixForCtrl + CtrlFooterLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle(null, null, pFooterRight, Color.Black,
                pPrefixForCtrl + CtrlFooterRight);

            Panel pnl = new Panel
            {
                ID = "divlstfooter"
            };
            Label lbl = new Label();
            lbl.Font.Size = FontUnit.XXSmall;
            lbl.ForeColor = (Color.Transparent == titleLeft.Color ? Color.Red : titleLeft.Color);
            lbl.Text = titleLeft.Footer;
            if (StrFunc.IsFilled(titleLeft.Id))
                lbl.ID = titleLeft.Id;
            pnl.Controls.Add(lbl);

            if (StrFunc.IsFilled(titleRight.Footer))
            {
                lbl = new Label();
                lbl.Font.Size = FontUnit.XXSmall;
                lbl.ForeColor = (Color.Transparent == titleRight.Color ? Color.Red : titleRight.Color);
                lbl.Text = titleRight.Footer;
                if (StrFunc.IsFilled(titleRight.Id))
                    lbl.ID = titleRight.Id;
                pnl.Controls.Add(lbl);
            }
            return pnl;
        }
        #endregion Others

        /// <summary>
        /// Verify the user rights to modify the current LST template. 
        /// When the user has no enough rights to modify the template, the record buttons (if they exist) will be disabled and
        /// a warning message will be displayed on the page.
        /// </summary>
        protected void VerifyUserRightsLstTemplate()
        {
            if (ThisConsultation == null || ThisConsultation.template == null)
            {
                throw new ArgumentException(@"Consultation object or relate tempate is null. 
                        to verify user rights with VerifyUserRightsToModLstTemplate you need to load a template.");
            }

            bool hasUserRightModify = true;

            bool hasUserRightSave = true;

            if (SessionTools.User.IdA != ThisConsultation.template.IDA)
            {
                hasUserRightModify = ThisConsultation.template.HasUserRight(SessionTools.CS, SessionTools.User, RightsTypeEnum.MODIFY);

                hasUserRightSave = ThisConsultation.template.HasUserRight(SessionTools.CS, SessionTools.User, RightsTypeEnum.SAVE);
            }

            WCTooltipLabel lblMissingModPermissions = ControlsTools.GetLabelMissingUserRightsLstTemplate(PlhMain);

            if (!hasUserRightSave)
            {
                if (PlhMain.FindControl("btnOkAndSave_footer") is WCToolTipLinkButton btnOkAndSave)
                {
                    btnOkAndSave.Enabled = false;
                }

                if (lblMissingModPermissions != null)
                {
                    lblMissingModPermissions.Text = String.Format(Ressource.GetString("lblMissingUserSaveRightsLstTemplate"),
                        ThisConsultation.template.DISPLAYNAME);
                    lblMissingModPermissions.Visible = true;
                }
            }

            if (!hasUserRightModify)
            {
                if (PlhMain.FindControl("btnOk_footer") is WCToolTipLinkButton btnOk)
                {
                    btnOk.Enabled = false;
                }

                if (lblMissingModPermissions != null)
                {
                    // the previous label lblMissingUserSaveRightsLstTemplate will be overwritten by the next one, being more critical
                    lblMissingModPermissions.Text = String.Format(Ressource.GetString("lblMissingUserModRightsLstTemplate"),
                        ThisConsultation.template.DISPLAYNAME);
                    lblMissingModPermissions.Visible = true;
                }

            }
        }
        //		
        #region public Events Click
        // EG 20210222 [XXXXX] Appel OpenerRefresh (présent dans PageBase.js) en remplacement de OpenerCallRefresh
        // EG 20210222 [XXXXX] Appel DoPostBackImmediate (présent dans PageBase.js)
        public void ButApply_Click(object sender, System.EventArgs e)
        {
            isClose = false;
            //
            SaveData();
            JavaScript.CallFunction(this, String.Format("OpenerRefresh('{0}','SELFRELOAD_')", PageName));
            if (isReloadPage)
                JavaScript.CallFunction(this, "DoPostBackImmediate('PAGE','RELOAD')");
        }
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présent dans PageBase.js) en remplacement de OpenerCallRefreshAndClose2
        public void ButOK_Click(object sender, System.EventArgs e)
        {
            isClose = true;
            //
            SaveData();
            // Pas de OpenerRefreshAndClose s'il existe un message
            if (StrFunc.IsEmpty(this.MsgForAlertImmediate))
            {
                // Only lstCriteria form (filter) generate a reload.
                string mode = (PageName == "LstCriteriaPage") ? "SELFRELOAD_" : "SELFCLEAR_";
                JavaScript.CallFunction(this, String.Format("OpenerRefreshAndClose('{0}','{1}')", PageName, mode));
            }
        }
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présent dans PageBase.js) en remplacement de OpenerCallRefreshAndClose2
        public void ButOKAndSave_Click(object sender, System.EventArgs e)
        {
            // 1- Le même comportement que le bouton "Ok" 
            isClose = true;
            SaveData();
            //
            // 2- En plus de Sauvegarder le template avec le même NOM 
            // donc ici sauvegarder le template et pas le template temporaraire)
            string TemplateName = ThisConsultation.template.IDLSTTEMPLATE_WithoutPrefix;
            
            SaveTemplate(TemplateName, TemplateName);

            //FI 20110713 pas de OpenerCallRefreshAndClose s'il existe un message
            if (StrFunc.IsEmpty(this.MsgForAlertImmediate))
                // 3- RefreshAndClose
                JavaScript.CallFunction(this, String.Format("OpenerRefreshAndClose('{0}','{1}')", PageName, "SELFRELOAD_"));
        }
        // EG 20210222 [XXXXX] Suppression inscription AutoClose
        // EG 20210222 [XXXXX] Appel DoPostBackImmediate (présent dans PageBase.js)
        public void ButOKQuery_Click(object sender, System.EventArgs e)
        {
            SaveData();
            //
            if (isClose)
                JavaScript.CallFunction(this, "self.close();");
            //
            if (isReloadPage)
                JavaScript.CallFunction(this, "DoPostBackImmediate('PAGE','RELOAD')");
        }
        // EG 20210222 [XXXXX] Appel DoPostBackImmediate (présent dans PageBase.js)
        public void ButReset_Click(object sender, System.EventArgs e)
        {
            Reset();
            //
            if (isReloadPage)
                JavaScript.CallFunction(this, "DoPostBackImmediate('PAGE','RELOAD')");
        }
        #endregion public Events Click
        //


        #region private InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "PageBaseTemplate";
            this.Load += new System.EventHandler(this.PageLoad);
        }
        #endregion

    }
}
