using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.Rights;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    public partial class LstSavePage : PageBaseTemplate
    {
        #region accessor
        protected override string SpecificSubTitle
        {
            get
            {
                string ret = Ressource.GetString("btnViewerSave");
                return ret.Replace("...", string.Empty).Trim();
            }
        }
        protected override PlaceHolder PlhMain
        {
            get
            {
                return plhControl;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200527 [XXXXX] Add
        protected override string PageName
        {
            get { return "LstSavePage"; }
        }
        #endregion accessor

        #region constructor
        public LstSavePage()
        {
            isAllowedToOverWrite = false;
        }
        #endregion

        #region protected override CreateControls
        // EG 20180423 Analyse du code Correction [CA2200]
        protected override void CreateControls()
        {
            try
            {
                CreateToolBar();
                CreateControlsData();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
        #region protected override LoadDataControls
        // RD 20110520 [17464]
        // Not load systematically the datagrid when menu opened
        // EG 20210505 [25700] FreezeGrid implementation 
        protected override void LoadDataControls()
        {
            ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, idA);
            //On recupère les données dans la base       
            if (ThisConsultation.template.IDLSTTEMPLATE != null)
            {
                SetTextBoxData(Cst.TXT + "INITIALLSTTEMPLATE", ThisConsultation.template.IDLSTTEMPLATE_WithoutPrefix);
                SetTextBoxData(Cst.TXT + "IDLSTCONSULT", ThisConsultation.template.IDLSTCONSULT);
                SetTextBoxData(Cst.TXT + "NEWLSTTEMPLATE", ThisConsultation.template.IDLSTTEMPLATE_WithoutPrefix);
                SetTextBoxData(Cst.TXT + "DISPLAYNAME", StrFunc.IsFilled(ThisConsultation.template.DISPLAYNAME) ? ThisConsultation.template.DISPLAYNAME : ThisConsultation.template.IDLSTTEMPLATE_WithoutPrefix);
                SetTextBoxData(Cst.TXT + "DESCRIPTION", ThisConsultation.template.DESCRIPTION);
                SetCheckBoxData(Cst.CHK + "TEMPLATEDEFAULT", ThisConsultation.template.ISDEFAULT);

                SetCheckBoxData(Cst.CHK + "ISLOADONSTART", ThisConsultation.template.ISLOADONSTART);
                SetTextBoxData(Cst.TXT + "REFRESHINTERVAL", ThisConsultation.template.IsRefreshIntervalSpecified ?
                                                            ThisConsultation.template.REFRESHINTERVAL.ToString() : string.Empty);
                SetTextBoxData(Cst.TXT + "ROWBYPAGE", ThisConsultation.template.ROWBYPAGE.ToString());
                SetTextBoxData(Cst.TXT + "FREEZECOL", ThisConsultation.template.FREEZECOL.ToString());
                SetDDLData(Cst.DDL + "CSSFILENAME", ThisConsultation.template.CSSCOLOR);

                if (!SessionTools.IsSessionGuest) //PL 20150601 GUEST New feature
                {
                    SetDDLData(Cst.DDL + "RightPublic", ThisConsultation.template.RIGHTPUBLIC);
                    SetDDLData(Cst.DDL + "RightEntity", ThisConsultation.template.RIGHTENTITY);
                    SetDDLData(Cst.DDL + "RightDepartment", ThisConsultation.template.RIGHTDEPARTMENT);
                    SetDDLData(Cst.DDL + "RightDesk", ThisConsultation.template.RIGHTDESK);
                }

                SetTextBoxData(Cst.TXT + "IDA", ThisConsultation.template.IDA.ToString());
                SetTextBoxData(Cst.TXT + "DTUPD", DtFunc.DateTimeToStringyyyyMMdd(ThisConsultation.template.DTUPD));
                SetTextBoxData(Cst.TXT + "IDAUPD", ThisConsultation.template.IDAUPD.ToString());
                SetTextBoxData(Cst.TXT + "DTINS", DtFunc.DateTimeToStringyyyyMMdd(ThisConsultation.template.DTINS));
                SetTextBoxData(Cst.TXT + "IDAINS", ThisConsultation.template.IDAINS.ToString());
                SetTextBoxData(Cst.TXT + "EXTLLINK", ThisConsultation.template.EXTLLINK);
                SetTextBoxData(Cst.TXT + "ROWATTRIBUT", ThisConsultation.template.ROWATTRIBUT);
                SetTextBoxData(Cst.OTCml_COL.ROWVERSION.ToString(), ThisConsultation.template.ROWVERSION);

                bool isEnabled = ThisConsultation.template.HasUserRight(SessionTools.CS, SessionTools.User, RightsTypeEnum.SAVE);

                if (false == isEnabled)
                    SetTextBoxData(Cst.TXT + "DISPLAYNAME", ThisConsultation.template.IDLSTTEMPLATE);

                SetTextBoxEnabled(Cst.TXT + "DISPLAYNAME", isEnabled);
                SetTextBoxEnabled(Cst.TXT + "DESCRIPTION", isEnabled);
                SetCheckBoxEnabled(Cst.CHK + "TEMPLATEDEFAULT", isEnabled);
                SetCheckBoxEnabled(Cst.CHK + "ISLOADONSTART", isEnabled);
                SetTextBoxEnabled(Cst.TXT + "REFRESHINTERVAL", isEnabled);
                SetTextBoxEnabled(Cst.TXT + "ROWBYPAGE", isEnabled);
                SetTextBoxEnabled(Cst.TXT + "FREEZECOL", isEnabled);
                SetDDLEnabled(Cst.DDL + "CSSFILENAME", isEnabled);
                if (!SessionTools.IsSessionGuest) //PL 20150601 GUEST New feature
                {
                    SetDDLEnabled(Cst.DDL + "RightPublic", isEnabled);
                    SetDDLEnabled(Cst.DDL + "RightEntity", isEnabled);
                    SetDDLEnabled(Cst.DDL + "RightDepartment", isEnabled);
                    SetDDLEnabled(Cst.DDL + "RightDesk", isEnabled);
                }

                //string pageFooter1 = Ressource.GetString_CreatedBy(Convert.ToDateTime(ThisConsultation.template.DTINS), ThisConsultation.template.IDAINSDisplayName);
                //string pageFooter2 = Ressource.GetString_LastModifyBy(Convert.ToDateTime(ThisConsultation.template.DTUPD), ThisConsultation.template.IDAUPDDisplayName);
                // FI 20200820 [25468] date systèmes en UTC 
                string pageFooter1 = RessourceExtended.GetString_CreatedBy(new DateTimeTz(ThisConsultation.template.DTINS, "Etc/UTC"), ThisConsultation.template.IDAINSDisplayName, true);
                // FI 20200820 [25468] date systèmes en UTC
                string pageFooter2 = RessourceExtended.GetString_LastModifyBy(new DateTimeTz(ThisConsultation.template.DTUPD, "Etc/UTC"), ThisConsultation.template.IDAUPDDisplayName, true);

                PlhMain.Controls.Add(SetFooter(pageFooter1, pageFooter2, "1"));
            }
        }
        #endregion
        #region protected override SaveData
        // RD 20110520 [17464]
        // Not load systematically the datagrid when menu opened
        // EG 20210505 [25700] FreezeGrid implementation 
        protected override void SaveData()
        {
            ThisConsultation.LoadTemplate(SessionTools.CS, idLstTemplate, idA);

            string rowByPage = GetTextBoxData(Cst.TXT + "ROWBYPAGE");
            int iRowByPage = 0;
            if (StrFunc.IsFilled(rowByPage))
            {
                try { iRowByPage = Convert.ToInt32(rowByPage); }
                catch { iRowByPage = 0; }
            }

            string freezeCol = GetTextBoxData(Cst.TXT + "FREEZECOL");
            int iFreezeCol = 0;
            if (StrFunc.IsFilled(freezeCol))
            {
                try { iFreezeCol = Convert.ToInt32(freezeCol); }
                catch { iFreezeCol = 0; }
            }

            string refreshInterval = GetTextBoxData(Cst.TXT + "REFRESHINTERVAL");
            int iRefreshInterval = 0;
            if (StrFunc.IsFilled(refreshInterval))
            {
                try
                {
                    //FI 20110723 Ajout d'un petit message qui va bien Msg_LstConsultRefresh
                    iRefreshInterval = Convert.ToInt32(refreshInterval);
                    if ((iRefreshInterval > 0) && (iRefreshInterval < ReferentialWeb.MinRefreshInterval))
                    {
                        iRefreshInterval = ReferentialWeb.MinRefreshInterval;
                        this.MsgForAlertImmediate = Ressource.GetString("Msg_LstConsultRefresh");
                    }
                }
                catch
                {
                    iRefreshInterval = 0;
                }
            }

            bool isLoadDataOnStart = GetCheckBoxData(Cst.CHK + "ISLOADONSTART");
            if ((false == isLoadDataOnStart) && (iRefreshInterval > 0))
            {
                isLoadDataOnStart = true;
                this.MsgForAlertImmediate = Ressource.GetString("Msg_LstConsultRefreshOnStart");
            }

            ThisConsultation.template.IDLSTCONSULT = GetTextBoxData(Cst.TXT + "IDLSTCONSULT");
            ThisConsultation.template.DISPLAYNAME = GetTextBoxData(Cst.TXT + "DISPLAYNAME");
            ThisConsultation.template.DESCRIPTION = GetTextBoxData(Cst.TXT + "DESCRIPTION");
            ThisConsultation.template.ISDEFAULT = GetCheckBoxData(Cst.CHK + "TEMPLATEDEFAULT");
            ThisConsultation.template.ISLOADONSTART = isLoadDataOnStart;
            ThisConsultation.template.REFRESHINTERVAL = iRefreshInterval;
            ThisConsultation.template.ROWBYPAGE = iRowByPage;
            ThisConsultation.template.FREEZECOL = iFreezeCol;
            ThisConsultation.template.CSSCOLOR = GetDDLData(Cst.DDL + "CSSFILENAME");
            if (SessionTools.IsSessionGuest) //PL 20150601 GUEST New feature
            {
                //GUEST: on affecte en "dur" les droits 
                ThisConsultation.template.RIGHTPUBLIC = RightsTypeEnum.NONE.ToString();
                ThisConsultation.template.RIGHTENTITY = RightsTypeEnum.REMOVE.ToString();
                ThisConsultation.template.RIGHTDEPARTMENT = RightsTypeEnum.REMOVE.ToString();
                ThisConsultation.template.RIGHTDESK = RightsTypeEnum.NONE.ToString();
            }
            else
            {
                //On affecte les nouveaux droits qu'apres verif des droits actuels
                if (SessionTools.IsSessionSysAdmin || SessionTools.IsSessionSysOper)
                    ThisConsultation.template.RIGHTPUBLIC = GetDDLData(Cst.DDL + "RightPublic");
                ThisConsultation.template.RIGHTENTITY = GetDDLData(Cst.DDL + "RightEntity");
                ThisConsultation.template.RIGHTDEPARTMENT = GetDDLData(Cst.DDL + "RightDepartment");
                ThisConsultation.template.RIGHTDESK = GetDDLData(Cst.DDL + "RightDesk");
            }
            //Invisibles
            ThisConsultation.template.IDA = Convert.ToInt32(GetTextBoxData(Cst.TXT + "IDA"));
            ThisConsultation.template.DTINS = new DtFunc().StringyyyyMMddToDateTime(GetTextBoxData(Cst.TXT + "DTINS"));
            ThisConsultation.template.IDAINS = Convert.ToInt32(GetTextBoxData(Cst.TXT + "IDAINS"));
            ThisConsultation.template.EXTLLINK = GetTextBoxData(Cst.TXT + "EXTLLINK");
            ThisConsultation.template.ROWATTRIBUT = GetTextBoxData(Cst.TXT + "ROWATTRIBUT");
            ThisConsultation.template.ROWVERSION = GetTextBoxData(Cst.OTCml_COL.ROWVERSION.ToString());

            string initialNameLstTemplate = GetTextBoxData(Cst.TXT + "INITIALLSTTEMPLATE");
            string newNameLstTemplate = GetTextBoxData(Cst.TXT + "NEWLSTTEMPLATE");

            SaveTemplate(initialNameLstTemplate, newNameLstTemplate);

            LoadDataControls();

            ReferentialWeb.WriteTemplateSession(idLstConsult, idLstTemplate, idA, this.ParentGUID);
        }
        #endregion
        #region protected override OnInit
        override protected void OnInit(EventArgs e)
        {
            InitializeComponent();
            this.Form = frmLstSave;
            base.OnInit(e);
        }
        #endregion
        #region protected override OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        #endregion

        #region private InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.AbortRessource = true;
            this.ID = "frmLstSave";

        }
        #endregion
        #region private Page_Load
        // EG 20210222 [XXXXX] Suppression inscription function EnabledDisabled, IsLessThenValue, EnabledDisabledLessThenValue et Set_Reset
        // EG 20210222 [XXXXX] Appel OpenerRefresh (présentes dans PageBase.js) en remplacement de OpenerCallRefresh
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présentes dans PageBase.js) en remplacement de OpenerCallRefreshAndClose2
        // EG 20210222 [XXXXX] Suppression inscription function SetFocus
        protected void Page_Load(object sender, System.EventArgs e)
        {
            PageTools.SetHead(this, this.Title, null, null);

            //JavaScript.EnabledDisabled(this);
            //JavaScript.IsLessThenValue(this);
            //JavaScript.EnabledDisabledLessThenValue(this);
            //JavaScript.Set_Reset(this);

            //si on re-post pour confirm
            if (dataFrom__EVENTTARGET == "CONFIRMSAVE" || dataFrom__EVENTTARGET == "CONFIRMSAVEQUIT")
            {
                //confirmation pour ecraser
                if (dataFrom__EVENTARGUMENT == "TRUE")
                {
                    isAllowedToOverWrite = true;
                    SaveData();
                    if (dataFrom__EVENTTARGET == "CONFIRMSAVE")
                        JavaScript.CallFunction(this, String.Format("OpenerRefresh('{0}','SELFRELOAD_')", PageName));
                    else
                        JavaScript.CallFunction(this, String.Format("OpenerRefreshAndClose('{0}','{1}')", PageName, "SELFRELOAD_"));
                }
                else
                {
                    //remettre l'ancien ID si on annule la modif
                    //20070330 PL/FI introduction d'un try/catch car plantage
                    try
                    {
                        //idLstTemplateInitial = ViewState[this.ClientID + "idLstTemplateInitial"].ToString();
                        //SetTextBoxData (Cst.TXT+"NEWLSTTEMPLATE", idLstTemplateInitial);
                        SetTextBoxData(Cst.TXT + "NEWLSTTEMPLATE", idLstTemplate);
                    }
                    catch { }
                }
            }

            if (!IsPostBack)
            {
                //on mets le focus surt 'Identifiant' lors du premier affichage
                JavaScript.CallFunction(this, "SetFocus('" + Cst.TXT + "NEWLSTTEMPLATE" + "')");
            }
        }
        #endregion

        #region CreateToolBar
        private void CreateToolBar()
        {
            Panel pnlToolBar = AddToolBar();
            WCToolTipLinkButton btn = AddButtonOk();
            defaultButton = btn.ID;
            pnlToolBar.Controls.Add(btn);
            AddSpace(pnlToolBar);
            pnlToolBar.Controls.Add(AddButtonCancel());
            AddSpace(pnlToolBar);
            pnlToolBar.Controls.Add(AddButtonApply());
        }
        #endregion CreateToolBar
        #region CreateControlsData
        // RD 20110520 [17464]
        // Not load systematically the datagrid when menu opened
        // EG 20210505 [25700] FreezeGrid implementation 
        private void CreateControlsData()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = MainClass };
            Panel pnlData = new Panel() { ID = "divlstsave", CssClass = ControlsTools.MainMenuName(IdMenu) };

            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "INITIALLSTTEMPLATE"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "IDLSTCONSULT"));

            // Identifier
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "NEWLSTTEMPLATE", true));
            // DisplayName
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "DISPLAYNAME", true));
            // Description
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "DESCRIPTION", true, true, true));
            // IsDefault
            pnlData.Controls.Add(NewRowCheckbox(true, Cst.CHK + "TEMPLATEDEFAULT"));
            // HR           
            //tblMain.Controls.Add(NewHRRow("HR", 10));
            // ISLOADONSTART
            pnlData.Controls.Add(NewRowCheckbox(true, Cst.CHK + "ISLOADONSTART"));
            // REFRESHINTERVAL            
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "REFRESHINTERVAL", true, true));
            // RowByPage            
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "ROWBYPAGE", true, true));
            // FreezeColumn number
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "FREEZECOL", true, true));
            // CSS file
            DropDownList ddlCssFileName = new DropDownList();
            ddlCssFileName.Style.Add(HtmlTextWriterStyle.Width, "280");
            ControlsTools.DDLLoad_CSSColor(ddlCssFileName, true);
            pnlData.Controls.Add(NewRowDDLCapture(ddlCssFileName, Cst.DDL + "CSSFILENAME"));
            // Space
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "IDA"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "DTUPD"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "IDAUPD"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "DTINS"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "IDAINS"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "EXTLLINK"));
            pnlData.Controls.Add(NewRowCapture2(Cst.TXT + "ROWATTRIBUT"));
            pnlData.Controls.Add(NewRowCapture2(Cst.OTCml_COL.ROWVERSION.ToString()));

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu) };
            togglePanel.SetHeaderTitle(Ressource.GetString("Characteristics"));
            togglePanel.AddContent(pnlData);
            pnlBody.Controls.Add(togglePanel);

            if (!SessionTools.IsSessionGuest)
            {
                pnlData = new Panel
                {
                    ID = "divlstsave2",
                    CssClass = ControlsTools.MainMenuName(IdMenu)
                };

                if (SessionTools.IsSessionSysAdmin || SessionTools.IsSessionSysOper)
                    pnlData.Controls.Add(NewRowDDLRight(Cst.DDL + "RightPublic", 0));
                pnlData.Controls.Add(NewRowDDLRight(Cst.DDL + "RightEntity", 0));
                pnlData.Controls.Add(NewRowDDLRight(Cst.DDL + "RightDepartment", 0));
                pnlData.Controls.Add(NewRowDDLRight(Cst.DDL + "RightDesk", 0));

                togglePanel = new WCTogglePanel() { CssClass = this.CSSMode + " " + ControlsTools.MainMenuName(IdMenu) };
                togglePanel.SetHeaderTitle(Ressource.GetString("Permissions"));
                togglePanel.AddContent(pnlData);
                pnlBody.Controls.Add(togglePanel);
            }

            PlhMain.Controls.Add(pnlBody);

        }
        #endregion
    }
}