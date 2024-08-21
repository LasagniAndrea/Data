using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Data;
using System.Threading;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;


namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de [!output SAFE_CLASS_NAME].
    /// </summary>
    // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction Forme Profile (non déconnecté) + Feuille de style 
    // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
    public partial class ProfilePage : PageBase
    {
        #region Members
        private bool isConnected;
        protected string leftTitle, rightTitle;
        private bool isDefaultOpener;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20171025 [23533] Modify
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction Forme Profile (non déconnecté) + Feuille de style 
        // EG 20200924 [XXXXX] Suppression du paramètre d'ouverture de page IsOpenInUniquePage
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        // EG 20220909 [XXXXX] Correction sur DDL ddlAuditTimestampPrecision exclusion de la valeur DDMM.
        protected void Page_Load(object sender, System.EventArgs e)
        {
            leftTitle = Ressource.GetString("btnProfil");
            rightTitle = null;

            isConnected = SessionTools.IsConnected;
            PageTools.SetHead(this, "Profile", null, null);

            imgOk.Text = @"<i class='fa fa-check'></i>" + Ressource.GetString("btnOk");
            imgCancel.Text = @"<i class='fa fa-times'></i>" + Ressource.GetString("btnCancel");
            imgApply.Text = @"<i class='fa fa-check'></i>" + Ressource.GetString("btnApply");
            imgReset.Text = @"<i class='far fa-window-restore'></i>" + Ressource.GetString("btnReset");
            SetCssMode("divalltoolbar");
            SetCssMode("divbody");
            SetCssMode("divculture");
            SetCssMode("divstyle");
            SetCssMode("divpreferences");
            SetCssMode("divactions");
            SetCssMode("divreset");

            HtmlContainerControl ctrl = (HtmlContainerControl)this.FindControl("lblCultureTitle");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblInformationTitle");
            ctrl = (HtmlContainerControl)this.FindControl("lblPreferences");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblPreferences");
            ctrl = (HtmlContainerControl)this.FindControl("lblActions");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblActions");
            ctrl = (HtmlContainerControl)this.FindControl("lblReset");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("Reset");
            ctrl = (HtmlContainerControl)this.FindControl("lblOthers");
            if (null != ctrl)
                ctrl.InnerText = Ressource.GetString("lblOthers");
            
            // FI 20171025 [23533]
            lblTradingTimestampZone.Text = Ressource.GetString("lblTradingTimestampZone");
            lblTradingTimestampPrecision.Text = Ressource.GetString("lblTradingTimestampPrecision");
            lblAuditTimestampZone.Text = Ressource.GetString("lblAuditTimestampZone");
            lblAuditTimestampPrecision.Text = Ressource.GetString("lblAuditTimestampPrecision");
            lblCSSMode.Text = Ressource.GetString("lblCSSMode");

            //LP 20240709 [WI1001] add user info
            SetInfoVisibility();
            SetUserInfo();
            SetParentInfo();

            HtmlTable table;
            Panel pnl;
            btnChangePassword.Text = "<i class='fas fa-key'></i>";
            btnShowDataCache.Text = "<i class='fas fa-search'></i>";
            if (isConnected)
            {
                if (!SessionTools.IsSessionSysAdmin)
                {
                    table = (HtmlTable)this.FindControl("tblReset");
                    if (table != null)
                        table.Visible = false;
                    pnl = (Panel)this.FindControl("divReset");
                    if (pnl != null)
                        pnl.Visible = false;

                }

                if (SessionTools.User.IsSessionGuest)
                {
                    //UserWithLimitedRights
                    //---------------------
                    //PREFERENCES
                    //lblDefaultPage.Enabled = false;
                    //ddlDefaultPage.Enabled = false;
                    lblDefaultPage.Visible = false;
                    ddlDefaultPage.Visible = false;

                    lblTrackerRefreshInterval.Visible = false;
                    txtTrackerRefreshInterval.Visible = false;
                    lblTrackerNbRowPerGroup.Visible = false;
                    txtTrackerNbRowPerGroup.Visible = false;
                    lblTrackerAlert.Visible = false;
                    chkTrackerAlert.Visible = false;
                    lblTrackerVelocity.Visible = false;
                    chkTrackerVelocity.Visible = false;

                    //ACTIONS
                    chkTrackerLog.Visible = false;
                    chkProcessLog.Visible = false;
                    chkSystemLog.Visible = false;
                    chkRDBMSLog.Visible = false;
                    chkCache.Visible = false;

                    lblLoginLog.Visible = false;
                    imgLoginLog.Visible = false;

                    // EG 20151215 [21305] New
                    lblUserLock.Visible = false;
                    imgUserLock.Visible = false;
                    lblHostLock.Visible = false;
                    imgHostLock.Visible = false;

                }
            }
            else
            {
                if (divpreferences != null)
                    divpreferences.Visible = false;
                if (divactions != null)
                    divactions.Visible = false;
                if (divreset != null)
                    divreset.Visible = false;
            }

            // EG 20151215 [21305] New
            // FI 20210728 [XXXXX] visible si IsSessionSysAdmin (avant c'était uniquement si isShowAdminTools)
            btnShowDataCache.Visible = SessionTools.IsSessionSysAdmin;
            btnShowDataCache.Text = "<i class='fas fa-search'></i>";
            // FI 20210728 [XXXXX] visible si IsSessionSysAdmin (avant c'était uniquement si isShowAdminTools)
            lblCache.Visible = SessionTools.IsSessionSysAdmin;
            this.PageTitle = leftTitle;

            //Message de confirmation de changement du password OK
            if ("succes" == Request.QueryString["Msg"])
                JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), Ressource.GetString("Msg_PWChangedWithSuccess", false), false, ProcessStateTools.StatusNoneEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);

            if (!IsPostBack)
            {
                ControlsTools.DDLLoad_Culture(ddlCulture, isConnected && (!Software.IsSoftwarePortal()) && (!SessionTools.IsUserWithLimitedRights()));
                ControlsTools.DDLSelectByValue(ddlCulture, Thread.CurrentThread.CurrentUICulture.Name);

                if (isConnected)
                {
                    bool isChangePasswordEnabled = false;
                    string SQLSelect = SQLCst.SELECT + "ms.PWDMODPERMITTED as PWDMODPERMITTED" + Cst.CrLf;
                    SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a" + Cst.CrLf;
                    SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MODELSAFETY.ToString() + " ms on (ms.IDMODELSAFETY = a.IDMODELSAFETY)" + Cst.CrLf;
                    SQLSelect += SQLCst.WHERE;
                    SQLSelect += "a.IDA=" + SessionTools.Collaborator_IDA.ToString();
                    object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, SQLSelect);
                    if (null != obj)
                        isChangePasswordEnabled = Convert.ToBoolean(obj);
                    btnChangePassword.Enabled = isChangePasswordEnabled;
                    btnChangePassword.Text = "<i class='fas fa-key'></i>";

                    txtNumberRowByPage.Text = SessionTools.NumberRowByPage.ToString();
                    chkPagerPosition.Checked = SessionTools.IsPagerPositionTopAndBottom;

                    ControlsTools.DDLLoad_DefaultPage(ddlDefaultPage);
                    ControlsTools.DDLSelectByValue(ddlDefaultPage, SessionTools.DefaultPage);

                    // FI 20171025 [23533] chgt des ddl
                    //ControlsTools.DDLLoad_ETDMaturityFormat(ddlETDMaturityFormat);
                    ControlsTools.DDLLoadEnum<Cst.ETDMaturityInputFormatEnum>(ddlETDMaturityFormat, false, true, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlETDMaturityFormat, SessionTools.ETDMaturityFormat.ToString());
                    
                    // Trading                    
                    ControlsTools.DDLLoadEnum<Cst.TradingTimestampZone>(ddlTradingTimestampZone, false, true, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlTradingTimestampZone, SessionTools.TradingTimestampZone.ToString());
                    ControlsTools.DDLLoadEnum<Cst.TradingTimestampPrecision>(ddlTradingTimestampPrecision, false, true, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlTradingTimestampPrecision, SessionTools.TradingTimestampPrecision.ToString());
                    
                    // Audit
                    ControlsTools.DDLLoadEnum<Cst.AuditTimestampZone>(ddlAuditTimestampZone, false, true, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlAuditTimestampZone, SessionTools.AuditTimestampZone.ToString());
                    // EG 20220909 [XXXXX] Correction sur DDL ddlAuditTimestampPrecision exclusion de la valeur DDMM.
                    ControlsTools.DDLLoadEnum<Cst.AuditTimestampPrecision>(ddlAuditTimestampPrecision, false, true, string.Empty,Cst.AuditTimestampPrecision.DDMM.ToString(),true);
                    ControlsTools.DDLSelectByValue(ddlAuditTimestampPrecision, SessionTools.AuditTimestampPrecision.ToString());

                    // CSSMode
                    ControlsTools.DDLLoadEnum<Cst.CSSModeEnum>(ddlCSSMode, false, false, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlCSSMode, SessionTools.CSSMode.ToString());

                    // ValidityData
                    ControlsTools.DDLLoadEnum<AdditionalCheckBoxEnum2>(ddlProfilValidityData, false, true, string.Empty);
                    ControlsTools.DDLSelectByValue(ddlProfilValidityData, SessionTools.ValidityData.ToString());

                    txtTrackerRefreshInterval.Text = SessionTools.TrackerRefreshInterval.ToString();
                    txtTrackerNbRowPerGroup.Text = SessionTools.TrackerNbRowPerGroup.ToString();
                    chkTrackerAlert.Checked = SessionTools.IsTrackerAlert;
                    chkTrackerVelocity.Checked = SessionTools.IsTrackerVelocity;

                    string url = JavaScript.GetWindowOpen("List.aspx?Log=" + Cst.OTCml_TBL.LOGIN_L.ToString() + "&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.AdminSafetyLoginLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.Collaborator_IDA.ToString(), Cst.WindowOpenStyle.EfsML_FormReferential);
                    imgLoginLog.Attributes.Add("onclick", url);
                    // EG 20151215 [21305] New
                    url = JavaScript.GetWindowOpen("List.aspx?Log=USERLOCKOUT_L&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.UserLockoutLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.Collaborator_IDA.ToString(), Cst.WindowOpenStyle.EfsML_FormReferential);
                    imgUserLock.Attributes.Add("onclick", url);
                    url = JavaScript.GetWindowOpen("List.aspx?Log=HOSTLOCKOUT_L&InputMode=2&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.HostLockoutLog) +
                        "&M=" + Cst.ConsultationMode.ReadOnly + "&FK=" + SessionTools.ClientMachineName, Cst.WindowOpenStyle.EfsML_FormReferential);
                    imgHostLock.Attributes.Add("onclick", url);

                    //LP 20240712 [WI1001] link to actor details
                    url = JavaScript.GetWindowOpen("Referential.aspx?T=Repository&O=ACTOR&PK=IDA&PKV={0}&IDMenu=" + IdMenu.GetIdMenu(IdMenu.Menu.Actor), Cst.WindowOpenStyle.EfsML_FormReferential);
                    imgUserLink.Attributes.Add("onclick", string.Format(url, SessionTools.Collaborator_IDA));
                    imgDeptOrDeskLink.Attributes.Add("onclick", string.Format(url, SessionTools.Collaborator.Department.Ida));
                    imgEntityLink.Attributes.Add("onclick", string.Format(url, SessionTools.Collaborator.Entity.Ida));

                    // AL 20240530 Update for Impersonation mode
                    url = JavaScript.GetWindowOpen(
                        "List.aspx?Repository=ACTORIMPERSONATE&InputMode=2"                  
                        , Cst.WindowOpenStyle.EfsML_FormReferential);

                    imgImpersonate.Attributes.Add("onclick", url);


                    url = JavaScript.GetWindowOpen("List.aspx?Repository=ACTORIMPERSONATEDBYME&InputMode=2", Cst.WindowOpenStyle.EfsML_FormReferential);
                    imgImpersonatedByMe.Attributes.Add("onclick", url);
                    imgImpersonatedByMe.Visible = !SessionTools.IsSessionSysAdmin;
                    lblImpersonatedByMe.Visible = !SessionTools.IsSessionSysAdmin;
                }
            }
            chkPagerPosition.Text = string.Empty;
            chkTrackerAlert.Text = string.Empty;
            chkTrackerVelocity.Text = string.Empty;

            #region Header
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, new HtmlPageTitle(leftTitle), null, IdMenu.GetIdMenu(IdMenu.Menu.Admin)));
            plhMain.Controls.Add(pnlHeader);
            #endregion Header
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction
        private void SetCssMode(string pId)
        {
            if (this.FindControl(pId) is Panel pnl)
                pnl.Attributes.Add("class", this.CSSMode + " admin");
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20220624 [XXXXX] Test opener pour refresh (Default ou OldDefault)
        // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN : Cet appel est requis par le Concepteur Web Form ASP.NET.
            //
            isDefaultOpener = (false == String.IsNullOrEmpty(Request.QueryString["default"]));
            InitializeComponent();
            base.OnInit(e);

            Form = frmProfile;
            AntiForgeryControl();
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnOk_Click(object sender, System.EventArgs e)
        {
            SaveProfile();
            Reload();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnCancel_Click(object sender, System.EventArgs e)
        {
            Reload();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnApply_Click(object sender, System.EventArgs e)
        {
            SaveProfile();
            Reload();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnChangePassword_Click(object sender, System.EventArgs e)
        {
            Response.Redirect("ChangePwd.aspx?FROMPROFIL=1");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnReset_Click(object sender, System.EventArgs e)
        {
            if (chkTrackerLog.Checked)
                ResetLog(Cst.OTCml_TBL.TRACKER_L);
            if (chkProcessLog.Checked)
                ResetLog(Cst.OTCml_TBL.PROCESS_L);
            if (chkSystemLog.Checked)
                ResetLog(Cst.OTCml_TBL.SYSTEM_L);
            if (chkRDBMSLog.Checked)
                ResetLog(Cst.OTCml_TBL.RDBMS_L);
            if (chkCache.Checked)
                ResetCache();
            if (chkDefault.Checked)
                ResetDefault();
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResetDefault()
        {
            /*
            #warning 20050919 PL NORELEASE Code à remplacer par une fonctionnalité à part entière
            //-------------------------------------------------------------------------
            LockTools.UnLockSession(SessionTools.CS, SessionTools.SessionID);
            //-------------------------------------------------------------------------
            */

            AspTools.PurgeSQLCookie(out int opNbRowsDeleted);
            string message;
            if (opNbRowsDeleted > 0)
            {
                message = Ressource.GetString("Msg_ProcessSuccessfull");
                message += Cst.CrLf;
                message += Ressource.GetString2("Msg_DeletedRows", opNbRowsDeleted.ToString());
            }
            else if (opNbRowsDeleted == 0)
                message = Ressource.GetString("Msg_NothingToDelete");
            else
                message = Ressource.GetString("Msg_ProcessUndone");

            //JavaScript.AlertImmediate((PageBase)this, message, false);
            JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), message, false, ProcessStateTools.StatusNoneEnum,
                JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// Purge du cache
        /// </summary>
        private void ResetCache()
        {
            int ret = 0;
            string msg;
            try
            {
                ret = ClearDataCache();
                ret += AutoCompleteDataCache.Clear();
                msg = GetMessageRemove(ret, string.Empty);
            }
            catch (TimeoutException ex)
            {
                msg = ex.Message;
                msg = msg + Cst.CrLf2 + GetMessageRemove(ret, string.Empty);
            }
            if (StrFunc.IsFilled(msg))
                JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), msg, false, ProcessStateTools.StatusNoneEnum,
                    JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void BtnShowDataCache_Click(object sender, EventArgs e)
        {
            ShowDataCache();
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20171025 [23533] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200924 [XXXXX] Suppression du paramètre d'ouverture de page IsOpenInUniquePage
        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        private void SaveProfile()
        {

            string prevCulture = Thread.CurrentThread.CurrentUICulture.Name;
            string culture = ddlCulture.SelectedItem.Value.ToString();
            SessionTools.SetCulture(culture);

            if (isConnected)
            {
                // Purge de SessionTools.Menus si changement de culture (permet un reload des menus dans la nouvelle culture)
                if (culture != prevCulture)
                    SessionTools.Menus = null;
                #region Update ACTOR table
                CSManager csManager = new CSManager(SessionTools.CS);
                if (false == csManager.IsUserReadOnly())
                {
                    DataParameters dataParameters = new DataParameters();
                    dataParameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), SessionTools.Collaborator_IDA);
                    dataParameters.Add(new DataParameter(SessionTools.CS, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), culture);
                    //
                    string SQLUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.ACTOR + @" 
                                   " + SQLCst.SET + @" CULTURE=@CULTURE 
                                   " + SQLCst.WHERE + @" IDA=@IDA";
                    //
                    DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, SQLUpdate, dataParameters.GetArrayDbParameter());
                }
                #endregion

                SessionTools.CSSMode = (Cst.CSSModeEnum)Enum.Parse(typeof(Cst.CSSModeEnum), ddlCSSMode.SelectedValue, true);
                SessionTools.ValidityData = (AdditionalCheckBoxEnum2)Enum.Parse(typeof(AdditionalCheckBoxEnum2), ddlProfilValidityData.SelectedValue, true);
                SessionTools.NumberRowByPage = Convert.ToInt32(txtNumberRowByPage.Text);
                SessionTools.IsPagerPositionTopAndBottom = chkPagerPosition.Checked; ;
                SessionTools.DefaultPage = ddlDefaultPage.SelectedValue;
                SessionTools.ETDMaturityFormat = (Cst.ETDMaturityInputFormatEnum)Enum.Parse(typeof(Cst.ETDMaturityInputFormatEnum), ddlETDMaturityFormat.SelectedValue, true);
                // FI 20171025 [23533] Trading et audit 
                SessionTools.TradingTimestampZone = (Cst.TradingTimestampZone)Enum.Parse(typeof(Cst.TradingTimestampZone), ddlTradingTimestampZone.SelectedValue, true);
                SessionTools.TradingTimestampPrecision = (Cst.TradingTimestampPrecision)Enum.Parse(typeof(Cst.TradingTimestampPrecision), ddlTradingTimestampPrecision.SelectedValue, true);
                SessionTools.AuditTimestampZone = (Cst.AuditTimestampZone)Enum.Parse(typeof(Cst.AuditTimestampZone), ddlAuditTimestampZone.SelectedValue, true);
                SessionTools.AuditTimestampPrecision = (Cst.AuditTimestampPrecision)Enum.Parse(typeof(Cst.AuditTimestampPrecision), ddlAuditTimestampPrecision.SelectedValue, true);
                SessionTools.TrackerRefreshInterval = Convert.ToInt32(txtTrackerRefreshInterval.Text);
                SessionTools.TrackerNbRowPerGroup = Convert.ToInt32(txtTrackerNbRowPerGroup.Text);
                SessionTools.IsTrackerAlert = chkTrackerAlert.Checked;
                SessionTools.IsTrackerVelocity = chkTrackerVelocity.Checked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20210614 [25500] New Customer Portal
        /// EG 20220624 [XXXXX] Test opener pour refresh (default ou oldDefault)

        /// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        private void Reload()
        {
            string js = string.Empty;
            if (Software.IsSoftwarePortal() || isDefaultOpener)
            {
                js += "window.parent.location.reload();";

            }
            else
            {
                //Reload du frame "mnu" qui contient login.aspx ou sommaire.apsx
                //Chacune de ses 2 pages se chargeant de faire un reload autres frames 
                //(ces frames derniers contenant les banners et welcome)
                //string js = "parent.mnu.location.reload(true);";
                js = "var mnu = parent.document.getElementById('mnu');";
                js += "var main = parent.document.getElementById('main');";
                js += "var tracker = parent.document.getElementById('boardInfo');";
                js += "var banner = parent.document.getElementById('banner');";
                js += "if (null != main) {main.src = 'Welcome.aspx';}";
                js += "if (null != banner) {banner.src = 'Banner.aspx';}";
                js += "if (null != mnu) {if (mnu.contentWindow) mnu.contentWindow.location.reload(true); else if (mnu.contentDocument) mnu.contentDocument.location.reload(true);}";
                js += "if (null != tracker) {if (tracker.contentWindow) tracker.contentWindow.location.reload(true); else if (tracker.contentDocument) tracker.contentDocument.location.reload(true);}";
            }
            JavaScript.ExecuteImmediate((PageBase)this, js, false);
            //JavaScript.ExecuteImmediate((PageBase)this, "Reload();", false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTbl"></param>
        private void ResetLog(Cst.OTCml_TBL pTbl)
        {
            AspTools.ResetLog(pTbl, out int opNbRowsDeleted);
            string message2 = Cst.CrLf + "Table: " + pTbl.ToString() + Cst.CrLf;
            string message = GetMessageRemove(opNbRowsDeleted, message2);

            JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("lblActions"), message, false, ProcessStateTools.StatusNoneEnum,
                JavaScript.DefautHeight, JavaScript.DefautWidth);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static int ClearDataCache()
        {
            return DataHelper.queryCache.Clear() + DataEnabledHelper.ClearCache(); ;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pNbRowsDeleted"></param>
        /// <param name="pMessage2"></param>
        /// <returns></returns>
        private static string GetMessageRemove(int pNbRowsDeleted, string pMessage2)
        {
            string ret;
            if (pNbRowsDeleted > 0)
            {
                ret = "Process completed successfully !" + Cst.CrLf;
                ret += pMessage2;
                ret += pNbRowsDeleted.ToString() + " row(s) removed.";
            }
            else if (pNbRowsDeleted == 0)
            {
                ret = "No data to remove !";
                ret += pMessage2;
            }
            else
            {
                ret = "Process not made !";
                ret += pMessage2;
            }
            return ret;
        }

        /// <summary>
        /// Affiche les requêtes présentes ds le cache SQL
        /// </summary>
        private void ShowDataCache()
        {
            string WindowID = "Diag" + "_" + "Cache" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff");
            string write_FpMLFile = SessionTools.TemporaryDirectory.MapPath("Diagnostic_xml") + @"\" + WindowID + ".xml";
            string open_FpMLFile = SessionTools.TemporaryDirectory.Path + "Diagnostic_xml" + @"/" + WindowID + ".xml";
            //
            DataHelper.queryCache.Diag.WriteElements(write_FpMLFile);
            //
            AddScriptWinDowOpenFile(WindowID, open_FpMLFile, string.Empty);
        }

        //LP 20240712 [WI1001] set the info of user
        private void SetUserInfo()
        {
            lblIdentifierInfo.Text = SessionTools.Collaborator.GetIdentifierDisplayNameId(null);
            lblIdentifierInfo.CssClass = (SessionTools.IsSessionSysAdmin) ? "adminInfo" : "userInfo";
        }

        //LP 20240712 [WI1001] set visibility of dept and entity line
        private void SetInfoVisibility()
        {
            lblDeptOrDesk.Visible = lblDeptOrDeskInfo.Visible = imgDeptOrDeskLink.Visible = SessionTools.Collaborator.Department.IsLoaded;
            lblEntity.Visible = lblEntityInfo.Visible = imgEntityLink.Visible = SessionTools.Collaborator.Entity.IsLoaded;
        }

        //LP 20240712 [WI1001] Set the info of Department and Entity
        private void SetParentInfo()
        {
            if (SessionTools.Collaborator.Department.IsLoaded)
            {
                //lblDeptOrDeskInfo.Text = SessionTools.Collaborator.Department.IdentifierDisplayNameId + Cst.HTMLSpace + SessionTools.Collaborator.Department.Role + Cst.HTMLSpace;
                lblDeptOrDeskInfo.Text = SessionTools.Collaborator.GetIdentifierDisplayNameId(SessionTools.Collaborator.Department);
                lblDeptOrDeskInfo.CssClass = "deptInfo";
            }

            if (SessionTools.Collaborator.Entity.IsLoaded)
            {
                //lblEntityInfo.Text = SessionTools.Collaborator.Entity.IdentifierDisplayNameId + Cst.HTMLSpace + SessionTools.Collaborator.Entity.Role + Cst.HTMLSpace;
                lblEntityInfo.Text = SessionTools.Collaborator.GetIdentifierDisplayNameId(SessionTools.Collaborator.Entity);
                lblEntityInfo.CssClass = "entityInfo";
            }
        }
    }
}