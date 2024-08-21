//using Microsoft.AspNet.Identity;
using System;
using System.Web;
using System.Web.UI;
//using Spheres;


using System.Threading;
using System.Globalization;
using System.Configuration;
using System.Collections.Generic;

using System.Data;
using System.Data.SqlClient;
using System.Drawing;

using System.Web.Security;

using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.Authenticate;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;

using EFS.Common.Web;
using EFS.Common.Log;

using EFS.Restriction;


namespace EFS.Spheres
{
    public partial class Login : ContentPageBase
    {
        #region Members
        private Collaborator clb;
        private string contentTitle;
        #endregion

        public string ContentTitle
        {
            get { return contentTitle; }
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            clb = new Collaborator();
            contentTitle = "Log in";
        }

        protected override void CreateChildControls()
        { }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool isAutoLogin = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
            ClientScript.RegisterHiddenField("__SOFTWARE", Software.Name);
            btnLogin.Text = Ressource.GetString("imgBtnLogin", true);
            btnLogin.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
            txtCollaborator.ToolTip = Ressource.GetString("InputIdentifier");
            txtPassword.ToolTip = Ressource.GetString("InputPwd");
            chkRemember.Visible = isAutoLogin;
            chkRemember.Text = string.Empty;
            lblRemember.ToolTip = Ressource.GetString("InputRemember");
            lblRemember.Text = Ressource.GetString("chkRemember");
            ddlRdbmsName.ToolTip = Ressource.GetString("InputRdbms");
            txtServerName.ToolTip = Ressource.GetString("InputServer");
            txtDatabaseName.ToolTip = Ressource.GetString("InputDatabase");
            pnlDetail.Visible = true;

            string msgRequiredField = Ressource.GetString("ISMANDATORY", true);
            rqvIdentifier.ErrorMessage = msgRequiredField;
            rqvPassword.ErrorMessage = msgRequiredField;

            //Form.DefaultButton = btnLogin.ClientID;
            //JavaScript.SetInitialFocus(txtCollaborator);

            if (!Request.IsAuthenticated)
            {
                // EG 20151215 [21305] New
                alertLogin.Visible = false;
                alertLoginMsg.Text = Ressource.GetString(clb.ErrorResource);
                alertLockout.Visible = false;

                if (!IsPostBack)
                {
                    #region Initialize (Label, DDL, ...)
                    foreach (string rdbms in Enum.GetNames(typeof(RdbmsEnum)))
                        ddlRdbmsName.Items.Add(new ListItem(Ressource.GetString(rdbms, true), rdbms));

                    //txtCollaborator.Attributes.Add("OnChange", "document.getElementById('txtPassword').value=''");
                    //txtPassword.Attributes.Add("OnClick", "this.value=''");
                    txtCollaborator.Attributes.Add("onchange", "$('#" + txtPassword.ClientID + "').val('');");
                    txtPassword.Attributes.Add("onclick", "alert($('#" + txtPassword.ClientID + "').val());$('#" + txtPassword.ClientID + "').val('');");
                    
                    #endregion

                    bool isSourceVisible = IsSourceVisible();
                    SessionTools.IsSourceVisible = isSourceVisible;

                    // FI 20160304 [XXXXX] call LoadControlsFromSystemSettings
                    string connectionMode;
                    LoadControlsFromSystemSettings(out connectionMode);

                    // FI 20160304 [XXXXX] call LoadControlFromCookies
                    LoadControlFromCookies(connectionMode);

                    if (!isSourceVisible)
                    {
                        pnlDetail.Visible = false;
                    }

                    if (this.Request.QueryString["connectionstate"] != "logout")
                    {
                        if ((isAutoLogin && chkRemember.Checked) && (SessionTools.ConnectionState != Cst.ConnectionState.LOGOUT))
                        {
                            if (SessionTools.ConnectionState == Cst.ConnectionState.INIT)
                                LoginProcess();
                            else if (SessionTools.ConnectionState == Cst.ConnectionState.LOGIN)
                                FormsAuthentication.RedirectFromLoginPage(SessionTools.Collaborator.DisplayName, false);
                        }
                    }
                }
            }

        }

        protected void LogIn(object sender, EventArgs e)
        {
            Cst.ErrLevel errLevel = LoginProcess();
        }


        private Cst.ErrLevel LoginProcess()
        {

            Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
            string csConfig = string.Empty;     // Source App provient du webConfig
            DateTime loginExpirationDateTime = new DateTime(9999, 1, 1);
            string errResource = string.Empty;

            string errMsg = string.Empty;
            string errMsgLog = string.Empty;
            string errMsgDet = string.Empty;

            string inputLogin = txtCollaborator.Text;
            string inputPassword = txtPassword.Text;
            string inputRdbms = ddlRdbmsName.SelectedValue;
            string inputServerName = txtServerName.Text;
            string inputDatabaseName = txtDatabaseName.Text;

            Connection connection = null;

            string lastConnectionString = GetLastConnectionString();

            bool isOk = false;

            // EG 20151215 [21305] New
            LockSafety lockSafety = null;
            bool isOkLockSafety = false;
            alertLockout.Visible = false;
            alertLogin.Visible = false;
            if ((inputLogin == lblIdentifier.Text) && (inputPassword == lblPassword.Text))
            {
                #region Tip: Permet d'afficher la "ConnectionString" (sans le password)
                string rdbmsName, serverName, databaseName, userName, pwd;
                rdbmsName = serverName = databaseName = userName = pwd = null;
                SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                        ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

                pwd = "*****"; // Afin de cacher le pwd
                csConfig = SystemSettings.GetMainConnectionString(inputRdbms, inputServerName, inputDatabaseName, userName, pwd);

                if (csConfig.ToLower().IndexOf("password=" + pwd) < 0)
                {
                    //Absence de "Password=*****" dasn la CS (Cas de la présence d'une key "ConnectionString")
                    int posPwdLbl = csConfig.ToLower().IndexOf("password=");
                    int posPwdEnd = csConfig.ToLower().IndexOf(";", posPwdLbl + "password=".Length);
                    string csConfigWithoutPwd = csConfig.Substring(0, posPwdLbl + "password=".Length);
                    csConfigWithoutPwd += pwd + csConfig.Substring(posPwdEnd);
                    csConfig = csConfigWithoutPwd;
                }

                //string infos = source + Cst.CrLf2 + DataHelper.AssembliesInfos(source);
                string infos = string.Empty;
                Dictionary<ComponentTypeEnum, string> assemblies = AssemblyTools.GetDomainAssemblies<Dictionary<ComponentTypeEnum, string>>();
                if (assemblies.TryGetValue(ComponentTypeEnum.Oracle, out infos))
                {
                    infos = csConfig + Cst.CrLf2 + infos;
                }
                else
                {
                    infos = csConfig;
                }
                JavaScript.BootstrapDialog(this, infos, false);
                #endregion
            }
            else if ((inputLogin == lblPassword.Text) && (inputPassword == lblIdentifier.Text))
            {
                #region Tip: Permet d'afficher les "Components"
                string infos = AssemblyTools.GetDomainAssemblies<string>();
                JavaScript.BootstrapDialog(this, infos, false);
                #endregion
            }
            else
            {
                SessionTools.Initialize2(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, Cst.DBStatusEnum.NA.ToString(), clb, loginExpirationDateTime);

                string rdbmsName, serverName, databaseName, userName, pwd;
                rdbmsName = serverName = databaseName = userName = pwd = null;
                SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                        ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

                csConfig = SystemSettings.GetMainConnectionString(inputRdbms, inputServerName, inputDatabaseName, userName, pwd);

                string msgSourceError = string.Empty;
                isOk = true;
                try
                {
                    CheckConnection.CheckConnectionString(csConfig);
                }
                catch (DataHelperException ex)
                {
                    WriteLogException(ex);
                    isOk = false;
                    switch (ex.errorEnum)
                    {
                        case DataHelperErrorEnum.loadDal:
                            errMsg = ex.Message;
                            errMsgLog = ex.Message;
                            //
                            if (ex.InnerException != null)
                                errMsgDet = "[" + ex.InnerException.Message + "]";
                            break;
                        case DataHelperErrorEnum.connection:
                            errResource = "FailureConnect_IncorrectConnectionString";
                            ErrMessage(errResource, ref errMsg, ref errMsgLog);
                            if (ex.InnerException != null)
                                errMsgDet = "[" + ex.InnerException.Message + "]";
                            break;
                    }
                }
            }

            // FI 20160304 [XXXXX] isOkLockSafety uniquement si isOk (cad la connexion du web config est correcte)
            string errLockoutMsg = string.Empty;
            string errLockoutMsgLog = string.Empty;
            if (isOk)
            {
                isOkLockSafety = true;
                // EG 20151215 [21305] New : Gestion des tentatives infructueuses
                lockSafety = new LockSafety(csConfig, SessionTools.ClientMachineName, inputLogin);
                // EG 20151215 [21305] Côté HOST
                isOk = lockSafety.host.ControlLock();
                if (lockSafety.host.lockoutReturn != LockoutReturn.SUCCESS)
                {
                    isOkLockSafety = false;
                    lockSafety.host.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                }
            }

            if (isOk)
            {
                //FI 20140123 Ce code est effectué si isOk uniquement, anciennement ce code était placé à tort avant CheckConnectionString
                //PL 20131014 New feature
                if (SystemSettings.GetAppSettings_Software("_RdbmsName") == "URL")
                {
                    //RdbmsName="URL": L'URL contient les informations de connexion à la BdD (Usage réservé à EFS)
                    if (SessionTools.processLog == null)
                    {
                        //Initialisation du log, qui n'a pu l'être dans Session_Start()
                        SessionTools.InitProcessLog(csConfig);
                    }
                }

                connection = new Connection();

                //PL 20160223 Newness use Keyword "********"
                bool isAutoLogin = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
                if (isAutoLogin && chkRemember.Checked && (inputPassword == "********"))
                {
                    // Read password from cookie
                    HttpCookie cookie = Request.Cookies[AspTools.GetCookieName("LastLogin")];
                    AspTools.ReadCookie(cookie, "Password", out inputPassword);
                    inputPassword = Cryptography.Decrypt(inputPassword);
                }

                //PL 20120426 LDAP Trace
                isOk = connection.LoadCollaborator(csConfig, inputLogin, inputPassword, SessionTools.processLog);
                if (StrFunc.IsFilled(connection.collaborator.LogMessage))
                {
                    JavaScript.BootstrapDialog(this, connection.collaborator.LogMessage, false);
                }

                clb = connection.collaborator;
                if (isOk)
                {
                    clb.SetRDBMSTrace(csConfig, SessionTools.HostName);
                    /// EG 20151215 [21305] Côté USER
                    isOk = lockSafety.user.ControlLock(lockSafety.host.lockoutReturn);
                    if (lockSafety.user.lockoutReturn != LockoutReturn.SUCCESS)
                    {
                        isOkLockSafety = false;
                        lockSafety.user.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                    }
                }
                else
                {
                    errResource = clb.ErrorResource;
                    if (StrFunc.IsEmpty(errResource))
                        errResource = "FailureConnect_" + clb.Validity.ToString();

                    ErrMessage(errResource, ref errMsg, ref errMsgLog);
                }
            }

            #region Contrôle Mot de passe
            if (isOk && (false == clb.IsPwdChecked))
            {
                //NB: On LDAP authentification, PWD is already checked.
                string hash = string.Empty + SystemSettings.GetAppSettings_Software("_Hash");
                if (StrFunc.IsEmpty(hash))
                    hash = Cst.HashAlgorithm.MD5.ToString(); //Default value
                if (StrFunc.IsEmpty(inputPassword))
                    inputPassword = inputLogin;//Tip for EFS
                isOk = clb.IsPwdValid(inputPassword, hash);
                if (!isOk)
                {
                    clb.Validity = Collaborator.ValidityEnum.IncorrectPassword;
                    errResource = "FailureConnect_" + clb.Validity.ToString();
                    ErrMessage(errResource, ref errMsg, ref errMsgLog);
                }
            }
            #endregion Contrôle Mot de passe

            //PL 20120127 Newness
            if (
                (!Software.IsSoftwareOTCmlOrFnOml()) &&
                ((clb.Validity == Collaborator.ValidityEnum.UnknownIdentifier) || (clb.Validity == Collaborator.ValidityEnum.IncorrectPassword))
                )
            {
                clb.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;
                errResource = "FailureConnect_" + clb.Validity.ToString();
                ErrMessage(errResource, ref errMsg, ref errMsgLog);
            }

            int maxSimultaneousLoginAuthorized = 1;
            int maxEntityAuthorized = 1;

            if (isOk)
            {
                string errMessage = string.Empty;
                string debugStep = string.Empty;
                isOk = CheckWebConnection.ReadRegistration(csConfig, ref maxSimultaneousLoginAuthorized, ref maxEntityAuthorized,
                    ref errResource, ref errMessage, ref debugStep);
                if (!isOk)
                {
                    ErrMessage(errResource, ref errMsg, ref errMsgLog);
                    errMsg += Cst.CrLf + " [" + debugStep + "]";
                    errMsgLog += " [" + errMessage + "]";
                }

#if DEBUG
                isOk = true;
                clb.SimultaneousLogin = Math.Max(100, clb.SimultaneousLogin);
                maxSimultaneousLoginAuthorized = Math.Max(100, maxSimultaneousLoginAuthorized);
                maxEntityAuthorized = Math.Max(10, maxEntityAuthorized);
#endif

            }
            if (isOk)
            {
                isOk = CheckWebConnection.CheckSimultaneousLogin(clb, maxSimultaneousLoginAuthorized, ref errResource);
                if (!isOk)//20070713 PL Add du if()
                {
                    errMsg = Ressource.GetString2(errResource, new string[] { maxSimultaneousLoginAuthorized.ToString() });
                    errMsgLog = Ressource.GetString2(errResource, SessionTools.LogMessageCulture, new string[] { maxSimultaneousLoginAuthorized.ToString() });
                }
            }

            #region Contrôle Horaire
            if (isOk)
            {
                if (SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.MODELDAYHOUR))
                {
                    isOk = CheckConnection.CheckModelDayHour(csConfig, clb, ref loginExpirationDateTime);
                    if (!isOk)
                    {
                        errResource = "FailureConnect_LoginTimeUnAllowed";
                        ErrMessage(errResource, ref errMsg, ref errMsgLog);
                    }
                }
            }
            #endregion Contrôle Horaire

            #region Contrôle ConnectionString
            string sourceSession = csConfig;
            if (isOk && clb.IsRdbmsUser)
            {
                sourceSession = SystemSettings.GetMainConnectionString(inputRdbms, inputServerName, inputDatabaseName, clb.Identifier, inputPassword);
                isOk = true;
                try
                {
                    CheckConnection.CheckConnectionString(sourceSession);
                }
                catch (Exception) { isOk = false; }
                if (false == isOk)
                {
                    errResource = "FailureConnect_IncorrectConnectionString";
                    clb.ErrorResource = errResource;
                    if (StrFunc.IsEmpty(errMsg))
                        ErrMessage(errResource, ref errMsg, ref errMsgLog);
                }
            }
            #endregion Contrôle ConnectionString

            if (isOk)
            {
                isOk = SessionTools.Initialize2(clb, sourceSession, inputRdbms, inputServerName, inputDatabaseName, loginExpirationDateTime);

                //20070713 PL New (A finaliser)
                if (isOk)
                {
                    isOk = CheckConnection.CheckMaxEntity(SessionTools.CS, SessionTools.Collaborator, maxEntityAuthorized, ref errResource);
                    if (!isOk)
                    {
                        errMsg = Ressource.GetString2(errResource, new string[] { maxEntityAuthorized.ToString() });
                        errMsgLog = Ressource.GetString2(errResource, SessionTools.LogMessageCulture, new string[] { maxEntityAuthorized.ToString() });
                        //
                        SessionTools.WelcomeMsg = errMsg;
                        //
                        //warning 20070713 PL à retirer et finaliser dans une prochaine version...
                        if (ActorTools.IsUserType_SysAdmin(SessionTools.Collaborator_ROLE))
                        {
                            //PL 20120206 A finaliser dans une prochaine version...
                            HttpConnectedUsers acu = new HttpConnectedUsers();
                            ConnectedUsers cu = acu.GetConnectedUsers();
                            if (cu.Count == 1)
                                isOk = true;
                        }
                    }
                }
            }
            //
            Boolean isChangeDb = false;
            if (isOk)
            {
                errLevel = Cst.ErrLevel.SUCCESS;

                //FI 20120911 valorisation de isChangeDb
                if (StrFunc.IsFilled(lastConnectionString))
                    isChangeDb = (lastConnectionString != SessionTools.CS);

                //FI 20120907 call AddCSHistory
                SessionTools.AddCSHistory();

                SessionTools.SetRestriction();

                //Initialisation de la session pour les copier/coller
                SessionTools.InitSessionClipBoard();

                //FI 20120124 Chargement des enums s'il ne l'ont pas été
                //Ils ne sont pas chargés systématiquement
                ExtendEnumsTools.LoadFpMLEnumsAndSchemes(CS);
                //
                WriteCookie(inputPassword);
                //
                FormsAuthentication.RedirectFromLoginPage(clb.DisplayName, false);
            }
            if (!isOk)
            {
                /// EG 20151215 [21305] Mise à jour tentatives infructueuses
                if (isOkLockSafety)
                {
                    lockSafety.Updating();
                    lockSafety.user.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                    lockSafety.host.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                }
                int lastBreakLine = errLockoutMsg.LastIndexOf(Cst.HTMLBreakLine);
                if (0 < lastBreakLine)
                    errLockoutMsg = errLockoutMsg.Substring(0, lastBreakLine);

                alertLockoutMsg.Text = errLockoutMsg;

                alertLoginMsg.Text = errMsg;
                alertLoginMsg.ToolTip = errMsgDet;
                errLevel = Cst.ErrLevel.LOGINUNSUCCESSFUL;
            }
            //
            if (errLevel != Cst.ErrLevel.SUCCESS)
                SessionTools.ResetLoginInfos();

            #region Write in ProcessLog
            ProcessStateTools.StatusEnum status = (isOk ? ProcessStateTools.StatusSuccessEnum : ProcessStateTools.StatusErrorEnum);
            if (isOk)
            {
                if (IsDatabaseNameEnabled() && (null != SessionTools.processLog) && isChangeDb)
                {
                    //Source de données saississable et différente de celle lue initialement dans le Web.Config, à l'ouverture de session.
                    //--> Fermeture du log courant
                    SessionTools.ProcessLogAddLoginInfo(ProcessStateTools.StatusSuccessEnum, ActionLogin.ChangeDB);

                    if (SessionTools.processLog.cs == lastConnectionString)
                    {
                        //SessionTools.processLog.cs == lastConnectionString 
                        //=> Signifie qu'il n'existe pas une base de donnée spécifique aux logs 
                        //--> Ouverture d'un nouveau log sur la nouvelle base de donnée
                        SessionTools.InitProcessLog(SessionTools.ConnectionString);
                    }
                }
                //
                SessionTools.ProcessLogAddLoginInfo(status, ActionLogin.Login);
                SessionTools.ConnectionState = Cst.ConnectionState.LOGIN;
                /// EG 20151215 [21305] Remise à zéro des compteurs suite à connexion avec SUCCESS
                lockSafety.Resetting();
            }
            else
            {

                SessionTools.ProcessLogAddLoginInfo(status, ActionLogin.Login,
                    new string[] { inputLogin, inputRdbms, inputServerName, inputDatabaseName, errMsgLog + errLockoutMsgLog });
                SessionTools.ConnectionState = Cst.ConnectionState.FAIL;
            }
            #endregion Write in ProcessLog
            alertLockout.Visible = StrFunc.IsFilled(alertLockoutMsg.Text);
            alertLogin.Visible = StrFunc.IsFilled(alertLoginMsg.Text);
            return errLevel;
        }


        private bool IsDatabaseNameEnabled()
        {
            if (IsSourceEnabled())
            {
                return true;
            }
            else
            {
                //PL 20101004 Add test on "ConnectionString"
                string tmpValue = string.Empty + SystemSettings.GetAppSettings_Software("ConnectionString");
                if (tmpValue.Length == 0)
                {
                    tmpValue = string.Empty + SystemSettings.GetAppSettings_Software("_SourceDisplay");
                    if (tmpValue.Length > 0)
                    {
                        tmpValue = tmpValue.ToUpper();
                        return (tmpValue == "ENABLED_DATABASENAME");
                    }
                }
            }
            return false;
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
        private bool IsSourceEnabled()
        {
            string tmpValue = string.Empty + SystemSettings.GetAppSettings_Software("ConnectionString");
            if (tmpValue.Length == 0)
            {
                tmpValue = string.Empty + SystemSettings.GetAppSettings_Software("_SourceDisplay");
                if (tmpValue.Length > 0)
                {
                    tmpValue = tmpValue.ToUpper();
                    return (tmpValue == "ENABLED");
                }
            }
            return false;
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
        /// Initialisation des contrôles txtCollaborator,txtPassword, chkRemember, ddlRdbmsName, txtServerName, txtDatabaseName à partir des cookies
        /// </summary>
        /// <param name="pConnectionMode"></param>
        /// FI 20160304 [XXXXX] Add Method 
        private void LoadControlFromCookies(string pConnectionMode)
        {
            //PL 20160223 Add Spheres version and Encryption some data
            HttpCookie cookie = Request.Cookies[AspTools.GetCookieName("LastLogin")];

            string s;
            bool isCrypt = true;
            AspTools.ReadCookie(cookie, "Version", out s);
            if (String.IsNullOrEmpty(s))
                isCrypt = false; // Avant, lorsque l'on ne stockait pas "Version", les données étaient non cryptées

            AspTools.ReadCookie(cookie, "Identifier", out s);
            txtCollaborator.Text = (isCrypt ? Cryptography.Decrypt(s) : s);

            AspTools.ReadCookie(cookie, "Remember", out s);
            chkRemember.Checked = BoolFunc.IsTrue(s);

            bool isAutoLogin = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
            if (isAutoLogin && chkRemember.Checked)
            {
                //PL 20160223 Newness use Keyword "********"
                //AspTools.ReadCookie(cookie, "Password", out s);
                //s = Cryptography.Decrypt(s);
                s = "********";
                txtPassword.Attributes["value"] = s; //Note: Indispensable pour les TextBox Password
                txtPassword.Text = s;
            }

            bool isSourceEnabled = IsSourceEnabled();
            Boolean isForceDisplayDetail = false;
            if (pConnectionMode != "URL" && isSourceEnabled)
            {
                AspTools.ReadCookie(cookie, "Rdbms", out s);
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

                AspTools.ReadCookie(cookie, "Server", out s);
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
                    AspTools.ReadCookie(cookie, "DBName", out s);
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

            AspTools.ReadCookie(cookie, "Display", out s);
            pnlDetail.Visible = BoolFunc.IsTrue(s);
            if (isForceDisplayDetail)
                pnlDetail.Visible = true;
        }

        /// <summary>
        /// Retourne la dernière dernière connectionString en vigueur
        /// <para>Retourne null en cas de timeout de session web</para>
        /// </summary>
        /// <returns></returns>
        private static string GetLastConnectionString()
        {
            string ret = string.Empty;
            if (StrFunc.IsFilled(SessionTools.LastConnectionString))
            {
                //Spheres® rentre ici lorsque l'utilisateur de déconnecte manuellement
                ret = SessionTools.LastConnectionString;
            }
            else
            {
                //S'il n'y a pas eu de déconnection manuelle, Spheres® récupère la connectionString en vigueur
                //Spheres® rentre notamment ici lorsque le timeout d'authentication est déclenché
                ret = SessionTools.CS;
            }
            return ret;
        }

        private void ErrMessage(string pResource, ref string opErrMsg, ref string opErrMsgLog)
        {
            opErrMsg = Ressource.GetString(pResource);
            opErrMsgLog = Ressource.GetString(pResource, SessionTools.LogMessageCulture);
        }

        //PL 20160223 Add Spheres version and Encryption some data
        private void WriteCookie(string pPassword)
        {
            HttpCookie cookie;
            CookieData[] cookiedata = new CookieData[8];
            cookiedata[0] = new CookieData("Version", Software.MajorMinor);
            cookiedata[1] = new CookieData("Identifier", Cryptography.Encrypt(txtCollaborator.Text));
            cookiedata[2] = new CookieData("Password", chkRemember.Checked ? Cryptography.Encrypt(pPassword) : string.Empty);
            cookiedata[3] = new CookieData("Remember", chkRemember.Checked ? "1" : "0");
            cookiedata[4] = new CookieData("Rdbms", Cryptography.Encrypt(ddlRdbmsName.SelectedValue));
            cookiedata[5] = new CookieData("Server", Cryptography.Encrypt(txtServerName.Text));
            cookiedata[6] = new CookieData("DBName", Cryptography.Encrypt(txtDatabaseName.Text));
            cookiedata[7] = new CookieData("Display", (pnlDetail.Visible) ? "1" : "0");
            Cst.ErrLevel ret = AspTools.WriteCookie("LastLogin", cookiedata, 7, "D", "", out cookie);
            if (ret == Cst.ErrLevel.SUCCESS)
                Response.Cookies.Add(cookie);
        }
    }
}