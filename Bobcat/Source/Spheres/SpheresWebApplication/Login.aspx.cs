using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de [!output SAFE_CLASS_NAME].
    /// </summary>
    /// EG 20151215 [21305] Refactoring, Add LockSafety
    /// EG 20201126 [25290] Mécanisme d'ignorement du couple user/pwd saisis lorsque la saisie a été initiée depuis un certain temps
    /// EG 20210614 [25500] New Customer Portal
    public partial class LoginPage : LoginBase
    {


        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        /// EG 20210614 [25500] New Customer Portal
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            Form = frmLogin;
            base.OnInit(e);
        }

        /// EG 20210614 [25290] Demande de connection le statut ConnectionState passe à LOGINREQUEST et le timer est désactivé
        /// EG 20210614 [25500] New Customer Portal
        /// EG 20240523 [WI941][26663] Upd : La connexion n'est pas exécutée si la page est en mode CAPTCHA, 
        /// c'est à dire  : SessionTools.ConnectionState != Cst.ConnectionState.CAPTCHA
        /// Le cas se présente après : 
        ///  - une connexion en erreur
        ///  - et SystemSettings.GetAppSettings("CaptchaSecurityCheck") = 1
        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            if (SessionTools.ConnectionState != Cst.ConnectionState.CAPTCHA)
            {
                SessionTools.ConnectionState = Cst.ConnectionState.LOGINREQUEST;
                SetAutoRefresh();
                divErrLockout.Visible = false;
                LoginData data = SetData(txtPassword.Text);
                LoginProcess(data);
                divErrLockout.Visible = StrFunc.IsFilled(data.LockoutMessage);
                lblLockoutMsg.Text = data.LockoutMessage;
                lblLoginMsg.Visible = (StrFunc.IsFilled(data.LoginMessage.Key));
                lblLoginMsg.Text = data.LoginMessage.Key;
                lblLoginMsg.ToolTip = data.LoginMessage.Value;
                SetAutoRefresh();
            }
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20210614 [25500] New Customer Portal
        protected void BtnDetail_Click(object sender, System.EventArgs e)
        {
            plhRDBMS.Visible = !plhRDBMS.Visible;
            LoginData data = SetData(txtPassword.Text);
            DisplayImageBtnDetail(data);
        }

        /// FI 20160301 [21966] Modify
        /// FI 20160304 [XXXXX] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20210614 [25500] New Customer Portal
        /// EG 20230301 [XXXXX] Ajout version de Spheres dans le Header de la fenêtre Login(+ Shibboleth adaptation for FedAuth.css)
        /// EG 20240523 [WI941][26663] Security : Account blocking - Add Captcha elements
        protected void Page_Load(object sender, System.EventArgs e)
        {
            // FI 20210806 [XXXXX] Add message Msg_WaitingConnection
            JQuery.Block block = new JQuery.Block((PageBase)this.Page)
            {
                Width = "150px",
                Timeout = SystemSettings.GetTimeoutJQueryBlock("LG")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);
            btnLogin.OnClientClick = $"Block({JavaScript.HTMLString(Ressource.GetString("Msg_WaitingConnection"))});stopResetTimer();return true;";

            PageTools.SetHead(this, "Login", null, null);
            ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
            divlogin.CssClass = CSSMode;
            divErrLockout.CssClass = CSSMode;
            divbody.CssClass = CSSMode;

            bool isAutoLogin_Allowed = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
            chkAutoLogin.Visible = isAutoLogin_Allowed;

            // EG 20240523 [WI941][26663] New
            btnCaptcha.Visible = false;
            btnCaptcha.Text = Ressource.GetString("lblCaptcha");
            btnCaptcha.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

            btnLogin.Text = "<i class='fas fa-sign-in-alt'></i> " + Ressource.GetString("imgBtnLogin", true);
            btnLogin.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");

            txtCollaborator.ToolTip = Ressource.GetString("InputIdentifier");
            txtPassword.ToolTip = Ressource.GetString("InputPwd");
            chkAutoLogin.ToolTip = Ressource.GetString("InputRemember");
            ddlRdbmsName.ToolTip = Ressource.GetString("InputRdbms");
            txtServerName.ToolTip = Ressource.GetString("InputServer");
            txtDatabaseName.ToolTip = Ressource.GetString("InputDatabase");

            lblSpheresVersion.Text = Software.NameMajorMinorType;
            lblSpheresVersion.ToolTip = Software.CopyrightFull;

            // FI 20160301 [21966] 
            // - Mise en place d'un DefaultButton sur le formulaire
            this.frmLogin.DefaultButton = btnLogin.ClientID;
            JavaScript.SetInitialFocus(txtCollaborator);

            if (!Request.IsAuthenticated)
            {
                // EG 20151215 [21305] New
                lblLoginMsg.Text = Ressource.GetString(clb.ErrorResource);
                lblLockoutMsg.Text = string.Empty;
                divErrLockout.Visible = false;

                // EG 20240523 [WI941][26663] New
                TooglePageToCaptchaMode();

                if (!IsPostBack)
                {
                    #region Initialize (Label, DDL, ...)
                    foreach (string rdbms in Enum.GetNames(typeof(RdbmsEnum)))
                        ddlRdbmsName.Items.Add(new ListItem(Ressource.GetString(rdbms, true), rdbms));

                    txtCollaborator.Attributes.Add("OnChange", "document.getElementById('txtPassword').value=''");
                    txtPassword.Attributes.Add("OnClick", "this.value=''");
                    #endregion

                    bool isSourceVisible = IsSourceVisible();
                    SessionTools.IsSourceVisible = isSourceVisible;

                    // FI 20160304 [XXXXX] call LoadControlsFromSystemSettings
                    LoadControlsFromSystemSettings(out string connectionMode);

                    // FI 20160304 [XXXXX] call LoadControlFromCookies
                    LoadControlFromCookies(connectionMode);

                    if (!isSourceVisible)
                    {
                        btnDetail.Visible = false;
                        plhRDBMS.Visible = false;
                    }

                    LoginData data = SetData(txtPassword.Text);
                    DisplayImageBtnDetail(data);

                    //if ((isAutoLogin_Allowed && chkAutoLogin.Checked) && (!SessionTools.IsInitialized))
                    //Test du Cst.ConnectionState.LOGOUT pour ne pas se reconnecter lorsque le user vient de se déconnecté (il est alors en mode Cst.ConnectionState.LOGOUT)
                    if (this.Request.QueryString["connectionstate"] != "logout")
                    {
                        //20090120 PL Add test on "connectionstate" (see also Banner.aspx)
                        if ((isAutoLogin_Allowed && chkAutoLogin.Checked) && (SessionTools.ConnectionState != Cst.ConnectionState.LOGOUT))
                        {
                            // FI 20160301 [21966] Refactoring pour ne pas rentrer 2 fois dans LoginProcess 
                            // Si nouvelle session, la page login.aspx est ouverte 2 fois (sommaire.aspx et tracker.aspx => 2 pages qui nécessitent une authentification 
                            // - La permière fois on rentre dans LoginProcess puisque Cst.ConnectionState.INIT
                            // - La seconde fois on redirige dirctement vers la page demandée puisque si la précédente connexion est ok on vérifie la condition (SessionTools.ConnectionState == Cst.ConnectionState.LOGIN)
                            if (SessionTools.ConnectionState == Cst.ConnectionState.INIT)
                                LoginProcess(data);
                               
                            else if (SessionTools.ConnectionState == Cst.ConnectionState.LOGIN)
                                FormsAuthentication.RedirectFromLoginPage(SessionTools.Collaborator.DisplayName, false);
                        }
                    }
                }
            }
        }

        /// EG 20201126 [25290] Mécanisme d'ignorement du couple user/pwd saisis lorsque la saisie a été initiée depuis un certain temps
        protected void OnReset(object sender, EventArgs e)
        {
            txtCollaborator.Text = string.Empty;
            txtPassword.Text = string.Empty;
        }
        /// EG 20201126 [25290] Mécanisme d'ignorement du couple user/pwd saisis lorsque la saisie a été initiée depuis un certain temps
        protected override void SetAutoRefresh()
        {
            timerReset.Enabled = (AutoRefresh > 0);
            timerReset.Enabled = false;
            if (timerReset.Enabled)
                timerReset.Interval = AutoRefresh * 1000;
        }

        /// <summary>
        /// Initialisation des contrôles ddlRdbmsName, txtServerName, txtDatabaseName à partir du fichier de configuration
        /// </summary>
        /// <param name="pConnectionMode"></param>
        /// FI 20160304 [XXXXX] Add Method 
        private void LoadControlsFromSystemSettings(out string pConnectionMode)
        {
            bool isSourceEnabled = IsSourceEnabled();

            string rdbmsName, serverName, databaseName, userName, pwd;
            rdbmsName = serverName = databaseName = userName = pwd = null;
            pConnectionMode = SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

            if (rdbmsName.Length > 0)
            {
                //isExistAppSetting_RdbmsName = true;
                ControlsTools.DDLSelectByValue(ddlRdbmsName, rdbmsName);
                ddlRdbmsName.Enabled = isSourceEnabled;
            }
            if (serverName.Length > 0)
            {
                //isExistAppSetting_ServerName = true;
                txtServerName.Text = serverName;
                txtServerName.Enabled = isSourceEnabled;
            }
            if (databaseName.Length > 0)
            {
                //isExistAppSetting_DatabaseName = true;
                txtDatabaseName.Text = databaseName;
                txtDatabaseName.Enabled = isSourceEnabled;
            }
        }

        /// <summary>
        /// Initialisation des contrôles txtCollaborator,txtPassword, chkAutoLogin, ddlRdbmsName, txtServerName, txtDatabaseName à partir des cookies
        /// </summary>
        /// <param name="pConnectionMode"></param>
        /// FI 20160304 [XXXXX] Add Method 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void LoadControlFromCookies(string pConnectionMode)
        {
            //PL 20160223 Add Spheres version and Encryption some data
            HttpCookie cookie = Request.Cookies[AspTools.GetCookieName("LastLogin")];

            bool isCrypt = true;
            AspTools.ReadCookie(cookie, "Version", out string s);
            if (String.IsNullOrEmpty(s))
                isCrypt = false; // Avant, lorsque l'on ne stockait pas "Version", les données étaient non cryptées

            AspTools.ReadCookie(cookie, "I1", "Identifier", out s);
            txtCollaborator.Text = (isCrypt ? Cryptography.Decrypt(s) : s);

            AspTools.ReadCookie(cookie, "X1", "Remember", out s);
            chkAutoLogin.Checked = BoolFunc.IsTrue(s);

            bool isAutoLogin_Allowed = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
            if (isAutoLogin_Allowed && chkAutoLogin.Checked)
            {
                //PL 20160223 Newness use Keyword "********"
                //AspTools.ReadCookie(cookie, "Password", out s);
                //s = Cryptography.Decrypt(s);
                s = "********";
                txtPassword.Attributes["value"] = s; //Note: Indispensable pour les TextBox Password
                txtPassword.Text = s;
            }

            Boolean isForceDisplayDetail = false;
            if (pConnectionMode != "URL" && IsSourceEnabled())
            {
                AspTools.ReadCookie(cookie, "C1", "Rdbms", out s);
                if (String.IsNullOrEmpty(s))
                {
                    s = RdbmsEnum.SQLSRV2K12.ToString(); //Default
                    if (ddlRdbmsName.SelectedIndex > -1)
                        s = ddlRdbmsName.SelectedValue; //Contient ce qui éventuellement existe dans le fichier de config

                    isForceDisplayDetail = true;
                }
                else if (isCrypt)
                {
                    s = Cryptography.Decrypt(s);
                }
                ControlsTools.DDLSelectByValue(ddlRdbmsName, s);

                AspTools.ReadCookie(cookie, "C2", "Server", out s);
                if (String.IsNullOrEmpty(s))
                {
                    s = "(Local)";//Default
                    if (StrFunc.IsFilled(txtServerName.Text))
                        s = txtServerName.Text; //Contient ce qui éventuellement existe dans le fichier de config
                    isForceDisplayDetail = true;
                }
                else if (isCrypt)
                {
                    s = Cryptography.Decrypt(s);
                }
                txtServerName.Text = s;

                bool isDatabaseNameEnabled = IsDatabaseNameEnabled();
                if (isDatabaseNameEnabled)
                {
                    AspTools.ReadCookie(cookie, "C3", "DBName", out s);
                    if (String.IsNullOrEmpty(s))
                    {
                        //Default
                        s = "SPHERES";
                        if (this.Request.ApplicationPath.StartsWith(@"/OTC") && !this.Request.ApplicationPath.EndsWith(@"/OTC"))
                            s += this.Request.ApplicationPath.Replace(@"/OTC", string.Empty);

                        if (StrFunc.IsFilled(txtDatabaseName.Text))
                            s = txtDatabaseName.Text; //Contient ce qui éventuellement existe dans le fichier de config
                    }
                    else if (isCrypt)
                    {
                        s = Cryptography.Decrypt(s);
                    }
                    if (isDatabaseNameEnabled)
                    {
                        txtDatabaseName.Text = s;
                    }
                }
            }

            AspTools.ReadCookie(cookie, "X2", "Display", out s);
            plhRDBMS.Visible = BoolFunc.IsTrue(s);
            if (isForceDisplayDetail)
                plhRDBMS.Visible = true;
        }

        private bool IsSourceVisible()
        {
            string tmpValue = string.Empty + SystemSettings.GetAppSettings_Software("_SourceDisplay");
            if (tmpValue.Length > 0)
            {
                tmpValue = tmpValue.ToUpper();
                return (tmpValue != "HIDE");
            }
            return true;
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            this.ID = "frmLogin";
        }

        protected void ChkAutoLogin_CheckedChanged(object sender, System.EventArgs e)
        {
            LoginData data = SetData(string.Empty);
            WriteCookie(data);
        }

        protected override LoginData SetData(string pPassword)
        {
            LoginData data = new LoginData
            {
                LblIdentifier = lblIdentifier.Text,
                LblPassword = lblPassword.Text,
                Identifier = txtCollaborator.Text,
                Password = pPassword,
                Rdbms = ddlRdbmsName.SelectedValue,
                ServerName = txtServerName.Text,
                DatabaseName = txtDatabaseName.Text,

                BtnDetail = btnDetail,
                IsAutoLogin = chkAutoLogin.Checked,
                IsPlhRdbmsVisible = plhRDBMS.Visible
            };

            return data;

        }
        /// <summary>
        /// Override de la méthode de base pour gestion de la page Login
        /// en fonction du retour de la demande de connexion
        /// Si ERREUR de connexion 
        /// - Affichage des messages d'erreurs
        /// - Si mode CAPTCHA ACTIVE alors:
        ///    - Affichage du bouton d'appel à la forme CAPTCHA
        ///    - Non affichage de tous les autres contrôles de la page LOGIN
        ///    - Mise à jour du statut de connexion 
        /// Sinon
        ///    - Classique
        /// </summary>
        /// <param name="pLoginData"></param>
        /// <returns></returns>
        // EG 20240523 [WI941][26663] New
        protected override Cst.ErrLevel LoginProcess(LoginData pLoginData)
        {
            Cst.ErrLevel ret = base.LoginProcess(pLoginData);
            if (Cst.ErrLevel.SUCCESS != ret)
            {
                if (BoolFunc.IsTrue(SystemSettings.GetAppSettings("CaptchaSecurityCheck")))
                    SessionTools.ConnectionState = Cst.ConnectionState.CAPTCHA;
                TooglePageToCaptchaMode();
            }
            return ret;
        }
        /// <summary>
        /// Appel à la page CAPTCHA
        /// </summary>
        // EG 20240523 [WI941][26663] New
        protected void BtnCaptcha_Click(object sender, EventArgs e)
        {
            Response.Redirect("CaptchaPage.aspx", true);
        }

        /// <summary>
        /// Gestion des contrôles à affichier dans la page login après tentative de connexion 
        /// en erreur
        /// En fonction du Mode : SessionTools.ConnectionState
        /// </summary>
        /// <param name="pErrLevel"></param>
        // EG 20240523 [WI941][26663] New
        protected void TooglePageToCaptchaMode(Cst.ErrLevel pErrLevel = Cst.ErrLevel.INITIALIZE)
        {
            bool isCaptchaMode = (SessionTools.ConnectionState == Cst.ConnectionState.CAPTCHA);
            lblLoginMsg.Visible = (false == isCaptchaMode) || (pErrLevel!= Cst.ErrLevel.INITIALIZE);
            lblIdentifier.Visible = (false == isCaptchaMode);
            txtCollaborator.Visible = (false == isCaptchaMode);
            txtPassword.Visible = (false == isCaptchaMode);
            lblPassword.Visible = (false == isCaptchaMode);
            chkAutoLogin.Visible = (false == isCaptchaMode);
            btnCaptcha.Visible = isCaptchaMode;
            btnLogin.Visible = (false == isCaptchaMode);
            btnDetail.Visible = (false == isCaptchaMode);
            plhRDBMS.Visible = (false == isCaptchaMode) && plhRDBMS.Visible;
        }
    }
}
