#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Authenticate;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI.WebControls;

//using AjaxControlToolkit;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense

namespace EFS.Common.Web
{
    /// <summary>
    /// Classe container des données élémentaires pour la connexion
    /// Ces données sont alimentées par les valeurs des contrôles de la page 
    /// Login appelante (Login du CustomerPortal ou Login de Spheres)
    /// </summary>
    /// EG 20210614 [25500] New Customer Portal
    public class LoginData
    {
        // Identifiant de connexion
        public string Identifier { get; set; }
        // Mot de passe de connexion
        public string Password { get; set; }
        // RDBMS associé
        public string Rdbms { get; set; }
        // Nom du serveur de données
        public string ServerName { get; set; }
        // Nom de la base
        public string DatabaseName { get; set; }
        // Libellé associé à l'identifiant
        public string LblIdentifier { get; set; }
        // Libellé associé au mot de passe
        public string LblPassword { get; set; }
        // Login automatique
        public bool IsAutoLogin {get;set;}
        // Le placeHolder des contrôles RDBMS est-il visible
        public bool IsPlhRdbmsVisible { get; set; }
        // Bouton détail
        public LinkButton BtnDetail { get; set; }
        // Message d'erreur de Lockout
        public string LockoutMessage { get; set; }
        // Message d'erreur de connexion 
        public KeyValuePair<string, string> LoginMessage { get; set; }
        // AL 20240607 [WI955] Impersonate mode
        public string InpersonateIdentifier { get; set; }
    }

    /// <summary>
    /// Classe de base pour la connexion utilisée 
    /// - par la forme Login du portail client
    /// - par la forme Login de Spheres
    /// </summary>
    /// EG 20210614 [25500] New Customer Portal
    public class LoginBase : PageBase
    {
        protected Collaborator clb;

        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            AntiForgeryControl();
            clb = new Collaborator();
            AutoRefresh = SessionTools.UserPasswordReset;
        }

        /// <summary>
        /// Connexion à la base
        /// </summary>
        /// <param name="pLoginData"></param>
        /// <returns></returns>
        // EG 20151215 [21305] Refactoring
        // FI 20160304 [XXXXX] Modify
        // FI 20160307 [XXXXX] Modify
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210209 [25660] Upd
        /// EG 20220613 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
        /// EG 20220826 [XXXXX][WI413]Gestion licence sur usage de Shibboleth SP avec Spheres
        /// EG 20220220 [26251] Validity User (SUPPRESSION de UnknownIdentifier et IncorrectPassword, on GARDE UnknownIdentifierOrIncorrectPassword) 
        // EG 20240523 [WI941][26663] Update Virtualisation de la méthode + Gestion du ControlLock
        protected virtual Cst.ErrLevel LoginProcess(LoginData pLoginData)
        {
            Cst.ErrLevel errLevel = Cst.ErrLevel.UNDEFINED;
            string csConfig = string.Empty;     // Source App provient du webConfig
            DateTime loginExpirationDateTime = new DateTime(9999, 1, 1);
            string errResource = string.Empty;

            string errMsg = string.Empty;
            string errMsgLog = string.Empty;
            string errMsgDet = string.Empty;

            Connection connection = null;

            DisplayImageBtnDetail(pLoginData);

            string lastConnectionString = GetLastConnectionString();

            bool isOk = false;

            LockSafety lockSafety = null;
            bool isOkLockSafety = false;
            
            // AL 20240530 Update for Impersonation mode
            if (pLoginData.Identifier.Contains("=>"))
            {
                int posImp = (pLoginData.Identifier.IndexOf("=>"));
                pLoginData.InpersonateIdentifier = pLoginData.Identifier.Substring(posImp + 2);
                pLoginData.Identifier = pLoginData.Identifier.Substring(0, posImp);
            }

            if ((pLoginData.Identifier == pLoginData.LblIdentifier) && (pLoginData.Password == pLoginData.LblPassword))
            {
                #region Tip: Permet d'afficher la "ConnectionString" (sans le password)
                string rdbmsName, serverName, databaseName, userName, pwd;
                rdbmsName = serverName = databaseName = userName = pwd = null;
                SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                        ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

                pwd = "*****"; // Afin de cacher le pwd
                csConfig = SystemSettings.GetMainConnectionString(pLoginData.Rdbms, pLoginData.ServerName, pLoginData.DatabaseName, userName, pwd);

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
                JavaScript.AlertImmediate(this, infos, false);
                #endregion 
            }
            else if ((pLoginData.Identifier == pLoginData.LblPassword) && (pLoginData.Password == pLoginData.LblIdentifier))
            {
                #region Tip: Permet d'afficher les "Components"
                string infos = AssemblyTools.GetDomainAssemblies<String>();
                JavaScript.AlertImmediate(this, infos, false);
                #endregion
            }
            else
            {
                SessionTools.Initialize2(string.Empty, string.Empty, string.Empty, string.Empty, 0, 0, 0, 0, Cst.DBStatusEnum.NA.ToString(), clb, loginExpirationDateTime);

                string rdbmsName, serverName, databaseName, userName, pwd;
                rdbmsName = serverName = databaseName = userName = pwd = null;
                SystemSettings.GetDefaultConnectionCharacteristics(false, this.Request.ApplicationPath,
                                        ref rdbmsName, ref serverName, ref databaseName, ref userName, ref pwd);

                csConfig = SystemSettings.GetMainConnectionString(pLoginData.Rdbms, pLoginData.ServerName, pLoginData.DatabaseName, userName, pwd);

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
                    switch (ex.ErrorEnum)
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

            if (isOk)
            {
                //Initialisation du log, qui n'a pu l'être dans Session_Start()
                if (false == SessionTools.ExistLog)
                    SessionTools.InitLog(csConfig);
            }

            // FI 20160304 [XXXXX] isOkLockSafety uniquement si isOk (cad la connexion du web config est correcte)
            string errLockoutMsg = string.Empty;
            string errLockoutMsgLog = string.Empty;
            if (isOk)
            {
                /* FI 20200811 [XXXXX] Mise en commentaire
                // FI 20191211 [XXXXX] Appel à ResetDatesysCol afin de réinitialisée la date système courante
                OTCmlHelper.ResetDatesysCol(csConfig);
                // FI 20200810 [XXXXX] Alimentation de la date système de suite post connexion réussie
                // (pour éviter que l'appel à cette méthode soit effectuée alors qu'il existe une transaction => provoquant l'ouverture de connexion supplémentaires)
                OTCmlHelper.GetDateSys(csConfig);
                */
                // FI 20200811 [XXXXX]
                // => Appel à SynchroDatesysCol pour que le 1er appel à OTCmlHelper.GetDateSys(cs) ne provoque pas l'ouverture d'une nouvelle connexion s'il existe une transaction courante 
                OTCmlHelper.SynchroDatesysCol(csConfig);


                isOkLockSafety = true;
                // EG 20151215 [21305] New : Gestion des tentatives infructueuses
                lockSafety = new LockSafety(csConfig, SessionTools.ClientMachineName, pLoginData.Identifier);
                // EG 20151215 [21305] Côté HOST
                isOk = lockSafety.Host.ControlLock();
                if (lockSafety.Host.lockoutReturn != LockoutReturn.SUCCESS)
                {
                    isOkLockSafety = false;
                    lockSafety.Host.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                }
            }

            if (isOk)
            {

                connection = new Connection();

                //PL 20160223 Newness use Keyword "********"
                bool isAutoLogin_Allowed = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));
                if (isAutoLogin_Allowed && pLoginData.IsAutoLogin && (pLoginData.Password == "********"))
                {
                    // Read password from cookie
                    HttpCookie cookie = Request.Cookies[AspTools.GetCookieName("LastLogin")];
                    //AspTools.ReadCookie(cookie, "Password", out inputPassword);
                    string inputPassword = pLoginData.Password;
                    AspTools.ReadCookie(cookie, "I2", out inputPassword);
                    pLoginData.Password = Cryptography.Decrypt(inputPassword);
                }

                //PL 20120426 LDAP Trace
                // FI 20240111 [WI793] appel à ProcessLogExtend
                LogAddDetail logAddDetail = new ProcessLogExtend(SessionTools.ProcessLog, SessionTools.Logger).LogAddDetail;
                // AL 20240530 Update for Impersonation mode
                isOk = connection.LoadCollaborator(csConfig, pLoginData.Identifier, pLoginData.Password, logAddDetail, SessionTools.IsShibbolethAuthenticationType, pLoginData.InpersonateIdentifier);
                if (StrFunc.IsFilled(connection.Collaborator.LogMessage))
                {
                    JavaScript.AlertImmediate(this, connection.Collaborator.LogMessage, false);
                }

                clb = connection.Collaborator;
                /// Les Contrôles User sont opérés (même si non trouvé dans la base)
                clb.SetRDBMSTrace(csConfig, SessionTools.AppSession.AppInstance.HostName);
                /// EG 20151215 [21305] Côté USER
                isOk = lockSafety.User.ControlLock(lockSafety.User.lockoutReturn);
                if (lockSafety.User.lockoutReturn != LockoutReturn.SUCCESS)
                {
                    isOkLockSafety = false;
                    lockSafety.User.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                }
            }

            if (isOk)
            {
                // EG 20220613 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
                // Pas de contrôle du mot de passe sur une authetification Shibboleth 
                if (false == SessionTools.IsShibbolethAuthenticationType)
                {
                    #region Contrôle Mot de passe
                    if (false == clb.IsPwdChecked)
                    {
                        //NB: On LDAP authentification, PWD is already checked.
                        if (StrFunc.IsEmpty(pLoginData.Password))
                        {
                            //Tip for EFS: permet de se connecter rapidement (sans saisie du mot de passe) avec un user RDBMS dont le mot de passe est identique à l'Identifiant.
                            //PL 20160315 WARNING! Ce Tip empêche également la faille suivante dans le cadre d'une authentification mixte: LDAP;RDBMS
                            //                     Scenario: Connexion sans saisie de mot de passe
                            //                               LDAP:  Error. Mot de passe incorrect. 
                            //                               RDBMS: Success. Actor existant, sans mot de passe.
                            pLoginData.Password = pLoginData.Identifier;
                        }
                        isOk = clb.IsPwdValid(csConfig, pLoginData.Password);
                        if (!isOk)
                        {
                            clb.Validity = Collaborator.ValidityEnum.UnknownIdentifierOrIncorrectPassword;
                            errResource = "FailureConnect_" + clb.Validity.ToString();
                            ErrMessage(errResource, ref errMsg, ref errMsgLog);
                        }
                    }
                    #endregion Contrôle Mot de passe
                }
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

            #region Contrôle FedAuth
            // EG 20220826 [XXXXX][WI413] New
            if (isOk && SessionTools.IsShibbolethAuthenticationType)
            {
                isOk = SessionTools.License.IsLicFunctionalityAuthorised(LimitationFunctionalityEnum.SHIBBOLETHSP);
                if (!isOk)
                {
                    errResource = "FailureConnect_FedAuthShibboleth";
                    ErrMessage(errResource, ref errMsg, ref errMsgLog);
                }
            }

            #endregion Contrôle FedAuth 

            #region Contrôle ConnectionString
            string sourceSession = csConfig;
            if (isOk && clb.IsRdbmsUser)
            {
                sourceSession = SystemSettings.GetMainConnectionString(pLoginData.Rdbms, pLoginData.ServerName, pLoginData.DatabaseName, clb.Identifier, pLoginData.Password);
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
                /* FI 20200811 [XXXXX] Mise en commentaire
                // FI 20191211 [XXXXX] Appel à ResetDatesysCol afin de réinitialisée la date système courante
                OTCmlHelper.ResetDatesysCol(sourceSession);
                // FI 20200810 [XXXXX] Alimentation de la date système de suite post connexion réussie
                // (pour éviter que l'appel à cette méthode soit effectuée alors qu'il existe une transaction => provoquant l'ouverture de connexion supplémentaires)
                OTCmlHelper.GetDateSys(sourceSession);
                */
                // FI 20200811 [XXXXX]
                // => Appel à SynchroDatesysCol pour que le 1er appel à OTCmlHelper.GetDateSys(cs) ne provoque pas l'ouverture d'une nouvelle connexion s'il existe une transaction courante 
                OTCmlHelper.SynchroDatesysCol(sourceSession);

                isOk = SessionTools.Initialize2(clb, sourceSession, pLoginData.Rdbms, pLoginData.ServerName, pLoginData.DatabaseName, loginExpirationDateTime);

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
            // EG 20210209 [25660] L'algorithme de hachage MD5 est déprécié. Méthode de transformation douce lors de la connexion
            if (isOk && clb.IsPwdMustBeUpdateAfterNewHashAlgorithm)
            {
                clb.IsPwdMustBeUpdateAfterNewHashAlgorithm = false;
                clb.UpdatePwdWithNewHashAlgorithm(SessionTools.CS, pLoginData.Password);
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

                // FI 20181122 [XXXXX] Alimentation de SESSIONRESTRICT uniquement si l'acteur n'est pas SysAdmin
                if ((SessionTools.User.IsApplySessionRestrict()))
                {
                    // FI [XXXX] Alimentation de SESSIONRESTRICT en mode Asynchrone 
                    // FI [XXXX] Alimentation de SESSIONRESTRICT en mode Asynchrone (Utilisation de Task.Run) parce que
                    // 1/ les inserts sont effectués dès l'appel Task.Run
                    // 2/ Avec l'usage de  RegisterAsyncTask, la page attend l'exécution de toutes les tâches asynchrones avant de rendre la main (ce qui n'est pas tip top dans notre cas)  
                    // 
                    //RegisterAsyncTask(new PageAsyncTask(SessionTools.SetRestrictionAsync));

                    SQLUP.GetId(out int idsessionId, SessionTools.CS, SQLUP.IdGetId.SESSIONRESTRICT);
                    SessionRestrictManager srmng = new SessionRestrictManager(SessionTools.CS, SessionTools.AppSession, SessionTools.Collaborator.NewUser());
                    srmng.SetRestriction(idsessionId);
                }

                //Initialisation de la session pour les copier/coller
                SessionTools.InitSessionClipBoard();

                //FI 20120124 Chargement des enums s'il ne l'ont pas été
                //Ils ne sont pas chargés systématiquement
                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper. Les enums sont chargés lorsque nécessaire
                //ExtendEnumsTools.LoadFpMLEnumsAndSchemes(SessionTools.CS);

                //
                //PL 20190304 Test in progress...
                SessionTools.DtArchive = SystemSettings.ReadDtArchive(SessionTools.CS);
                //----------------------------------------------------
                // EG 20200826 Mise à jour du CSSMode post connexion
                //----------------------------------------------------
                SetCSSMode();
                WriteCookie(pLoginData);

                FormsAuthentication.RedirectFromLoginPage(clb.DisplayName, false);
            }
            if (!isOk)
            {
                /// EG 20151215 [21305] Mise à jour tentatives infructueuses
                if (isOkLockSafety)
                {
                    lockSafety.Updating();
                    lockSafety.User.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                    lockSafety.Host.GetMessage(ref errLockoutMsg, ref errLockoutMsgLog);
                }
                int lastBreakLine = errLockoutMsg.LastIndexOf(Cst.HTMLBreakLine);
                if (0 < lastBreakLine)
                    errLockoutMsg = errLockoutMsg.Substring(0, lastBreakLine);

                pLoginData.LockoutMessage = errLockoutMsg;
                pLoginData.LoginMessage = new KeyValuePair<string, string>(errMsg, errMsgDet);
                errLevel = Cst.ErrLevel.LOGINUNSUCCESSFUL;
            }
            //
            if (errLevel != Cst.ErrLevel.SUCCESS)
                SessionTools.ResetLoginInfos();

            #region Write in log
            LogLevelEnum logLevel = (isOk ? LogLevelEnum.Info : LogLevelEnum.Error);
            if (isOk)
            {
                bool isInitLog = false; 

                // PM 20200818 [XXXXX] New Log
                if (IsDatabaseNameEnabled() && isChangeDb)
                {
                    SessionTools.LogAddLoginInfo(logLevel, ActionLogin.ChangeDB,
                        new string[] { SessionTools.Collaborator_IDENTIFIER, SessionTools.Collaborator_AuthenticationType.ToString(),
                                        SessionTools.Data.RDBMS, SessionTools.Data.Server, SessionTools.Data.Database });

                    //Source de données saississable et différente de celle lue initialement dans le Web.Config, à l'ouverture de session.
                    //--> Fermeture du log courant
                    //
                    if (null != SessionTools.Logger)
                    {
                        isInitLog = SessionTools.Logger.LogScope.ConnectionString == lastConnectionString;
                    }
                    else if (null != SessionTools.ProcessLog)
                    {
                        //SessionTools.processLog.cs == lastConnectionString 
                        //=> Signifie qu'il n'existe pas une base de donnée spécifique aux logs 
                        //--> Ouverture d'un nouveau log sur la nouvelle base de donnée
                        isInitLog = (SessionTools.ProcessLog.cs == lastConnectionString);
                    }
                }

                if (isInitLog)
                    SessionTools.InitLog(SessionTools.ConnectionString);

                SessionTools.LogAddLoginInfo(logLevel, ActionLogin.Login, new string[] { SessionTools.Collaborator_IDENTIFIER, SessionTools.Collaborator_AuthenticationType.ToString(),
                                        SessionTools.Data.RDBMS, SessionTools.Data.Server, SessionTools.Data.Database });

                SessionTools.ConnectionState = Cst.ConnectionState.LOGIN;
                // EG 20151215 [21305] Remise à zéro des compteurs suite à connexion avec SUCCESS
                lockSafety.Resetting();
            }
            else
            {
                SessionTools.LogAddLoginInfo(logLevel, ActionLogin.Fail,
                    new string[] { pLoginData.Identifier, SystemSettings.GetAppSettings("AuthenticateType", "NA"), pLoginData.Rdbms, pLoginData.ServerName, pLoginData.DatabaseName, errMsgLog + errLockoutMsgLog });
                
                SessionTools.ConnectionState = Cst.ConnectionState.FAIL;
            }
            #endregion Write in ProcessLog
            return errLevel;
        }


        // PL 20160223 Add Spheres version and Encryption some data
        // PL 20180903 Use new light key (I1, I2, X1, X2, C1, ...) 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void WriteCookie(LoginData pLoginData)
        {
            //PL 20180903 Si Source non accessible (ENABLED) plus de sauvegarde dans le cookie.
            //            Si AutoLogin non autorisé (FALSE) plus de sauvegarde dans le cookie.
            bool isSourceEnabled = IsSourceEnabled();
            bool isAutoLogin_Allowed = BoolFunc.IsTrue(SystemSettings.GetAppSettings("AutoLogin"));

            int i = 4 + (isAutoLogin_Allowed && pLoginData.IsAutoLogin ? 1 : 0) + (isSourceEnabled ? 3 : 0);

            CookieData[] cookiedata = new CookieData[i];
            i = -1;
            cookiedata[++i] = new CookieData("Version", Software.MajorMinor);
            cookiedata[++i] = new CookieData("I1", Cryptography.Encrypt(pLoginData.Identifier));
            if (isAutoLogin_Allowed && pLoginData.IsAutoLogin)
                cookiedata[++i] = new CookieData("I2", Cryptography.Encrypt(pLoginData.Password));
            cookiedata[++i] = new CookieData("X1", pLoginData.IsAutoLogin ? "1" : "0");
            cookiedata[++i] = new CookieData("X2", (pLoginData.IsPlhRdbmsVisible) ? "1" : "0");
            if (isSourceEnabled)
            {
                cookiedata[++i] = new CookieData("C1", Cryptography.Encrypt(pLoginData.Rdbms));
                cookiedata[++i] = new CookieData("C2", Cryptography.Encrypt(pLoginData.ServerName));
                cookiedata[++i] = new CookieData("C3", Cryptography.Encrypt(pLoginData.DatabaseName));
            }

            Cst.ErrLevel ret = AspTools.WriteCookie("LastLogin", cookiedata, 7, "D", "", out HttpCookie cookie);
            if (ret == Cst.ErrLevel.SUCCESS)
                Response.Cookies.Add(cookie);
        }

        protected bool IsDatabaseNameEnabled()
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


        protected bool IsSourceEnabled()
        {
            //PL 20101004 Add test on "ConnectionString"
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

        protected void ErrMessage(string pResource, ref string opErrMsg, ref string opErrMsgLog)
        {
            opErrMsg = Ressource.GetString(pResource);
            opErrMsgLog = Ressource.GetString(pResource, SessionTools.LogMessageCulture);
        }

        /// <summary>
        /// Retourne la dernière connectionString en vigueur
        /// <para>Retourne null en cas de timeout de session web</para>
        /// </summary>
        /// <returns></returns>
        private /*static*/ string GetLastConnectionString()
        {
            string ret;
            if (StrFunc.IsFilled(SessionTools.LastConnectionString))
            {
                //Spheres® rentre ici lorsque l'utilisateur de déconnecte manullement
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

        protected void DisplayImageBtnDetail(LoginData pLoginData)
        {
            if (null != pLoginData.BtnDetail)
                pLoginData.BtnDetail.Text = String.Format(@"<i class='fas fa-chevron-circle-{0}'></i> ", pLoginData.IsPlhRdbmsVisible ? "up" : "down") + Ressource.GetString("btnDetail");
        }


        protected virtual LoginData SetData(string pPassword)
        {
            return null;
        }
    }

}

