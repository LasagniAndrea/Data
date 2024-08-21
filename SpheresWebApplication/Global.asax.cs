using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;

using SecuritySwitch;
using SecuritySwitch.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.SessionState;
using System.Xml;

namespace EFS.Spheres
{
    public class Global : HttpApplication
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Start(object sender, EventArgs e)
        {
            Application.Lock();

            // FI 20210223 [XXXXX] CleanTemporaryDirectory fait en priorité car La trace pourrait utiliser ce répertoire comme répertoire de travail
            bool isResetTemporaryFolder = (bool)SystemSettings.GetAppSettings("ResetTemporaryFolder", typeof(System.Boolean), true);
            if (isResetTemporaryFolder)
                AppInstance.CleanTemporaryDirectory(Server.MapPath(@"~/Temporary"));

            // FI 20210108 [XXXXX] Initialisation du fichier trace
            AppInstance appInstance = new AppInstance()
            {
                HostName = Server.MachineName,
                AppName = Software.Name,
                AppVersion = Software.Version,
                AppRootFolder = Server.MapPath(string.Empty),
            };

            // FI 20210628 [XXXXX] 5 tentatives pour initiliser la trace
            Exception exceptionTrace = null;
            int NumberOfRetries = 5;
            for (int i = 1; i <= NumberOfRetries; ++i)
            {
                try
                {
                    appInstance.InitilizeTraceManager();
                    break;
                }
                catch (Exception exception) when (FileTools.IsFileUsedException(exception) && i <= NumberOfRetries)
                {
                    // Cette exception se produit lorsqu'il y a tentative de renommer une trace déjà existante alors que la trace est ouverte par un autre process
                    Thread.Sleep(1000);
                }
                catch (Exception exception)
                {
                    exceptionTrace = exception; // Exception sevère => pas de nouvelle tentative
                    break;
                }
            }

            // FI 20210108 [XXXXX] Initialisation de la trace dans DataHelper
            SpheresTraceSource spheresTrace = AppInstance.TraceManager.SpheresTrace;
            if (null != spheresTrace)
            {
                DataHelper.traceQueryError = AppInstance.TraceManager.TraceError;
                DataHelper.traceQueryWarning = AppInstance.TraceManager.TraceWarning;
                DataHelper.sqlDurationLimit = spheresTrace.SqlDurationLimit;
            }


            // FI 20210108 [XXXXX] add Trace Info
            AppInstance.TraceManager.TraceInformation(this, $"Application Start (Name:{Software.Name}, Version:{Software.Version})");

            // FI 20210108 [XXXXX] Inscriptions des DLL
            AppInstance.TraceManager.TraceInformation(this, StrFunc.AppendFormat("Assemblies info:{0}{1}", Cst.CrLf, AssemblyTools.GetAppInstanceAssemblies<string>(appInstance)));

            if (AppInstance.TraceManager.IsSpheresTraceAvailable)
                AppInstance.TraceManager.TraceInformation(this, $"SqlDurationLimit: {EFS.Common.AppInstance.TraceManager.SpheresTrace.SqlDurationLimit}");


            // FI 20240111 [WI793] call 
            InitLoggerClient();

            ErrorManager ErrMngr = new ErrorManager();
            ErrMngr.InitDatabase2(SystemSettings.GetLogConnectionString(false, string.Empty));
            ErrMngr.InitXMLLogFile(Server.MapPath(SystemSettings.GetAppSettings("Spheres_LogsXMLFile")));
            ErrMngr.InitEmail(SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpServer"));
            ErrMngr.InitEventLog("Application", Software.Name);
            Application["LOG"] = ErrMngr;

            // FI 20210628 [XXXXX] Inscription dans le log de l'éventuelle exception rencontrée lors de l'initialisation de la trace
            if (null != exceptionTrace)
                ErrMngr.Write(new ErrorBlock(exceptionTrace, null, "Global.asax"));

            ArrayList alConnectedUsers = new ArrayList();
            Application["CONNECTEDUSERS"] = alConnectedUsers;

            bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
            if (isEFSmLHelp)
            {
                XmlDocument doc = new XmlDocument();
                // FI 20160804 [Migration TFS] Modify
                //string path = Server.MapPath(@"OTCml\XML_Files\Common\EFSmL_HelpIndexSchemas.xml");
                string path = Server.MapPath(@"HelpSchemas\EFSmL_HelpIndexSchemas.xml");
                if (File.Exists(path))
                {
                    doc.Load(path);
                    Application["EFSmL_HelpIndexSchemas"] = doc;
                }
            }

            Application["ISDATACACHEENABLED"] = BoolFunc.IsTrue(SystemSettings.GetAppSettings("DataCacheEnabled", "false"));
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheEnabledInterval")))
                DataHelper.queryCache.TimeSpanDataEnabled = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheEnabledInterval"));
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheCleanInterval")))
                DataHelper.queryCache.TimeSpanDataClean = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheCleanInterval"));
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheMinRows")))
                DataHelper.queryCache.MinRowsInResult = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheMinRows"));
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheMaxRows")))
                DataHelper.queryCache.MaxRowsInResult = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheMaxRows"));

            Application.UnLock();

        }

        /// <summary>
        ///  Initialisation éventuelle du Client du service de log (voir <seealso cref="LoggerManager"/>) 
        /// </summary>
        /// FI 20240111 [WI793] usage de LogAddDetail
        private void InitLoggerClient()
        {
            if (BoolFunc.IsTrue(SystemSettings.GetAppSettings("RemoteLogEnabled", "true")))
            {
                LoggerManager.Enable();
                LoggerManager.Initialize((AppInstance.MasterAppInstance.AppNameInstance, AppInstance.MasterAppInstance.AppVersion, AppInstance.MasterAppInstance.AppNameVersion, 1), AppInstance.TraceManager);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_Start(Object sender, EventArgs e)
        {
            // FI 20210108 [XXXXX] add Trace Info
            AppInstance.TraceManager.TraceInformation(this, $"Session Start (id: {SessionTools.SessionID},ShorId: {SessionTools.ShortSessionID})");

            SessionTools.SetAppSession();
            SessionTools.Collaborator = null;
            SessionTools.SetServerAndUserHost();
            SessionTools.SetUserHostAddress();
            SessionTools.SetClientMachineName();

            SessionTools.ConnectionState = Cst.ConnectionState.INIT;
            SessionTools.LogMessageCulture = CultureInfo.CreateSpecificCulture("en-GB");


            string csLog = SystemSettings.GetLogConnectionString(false, HttpContext.Current.Request.ApplicationPath);

            // FI 20140120 [] add StrFunc.IsFilled(csLog);
            bool isCheckConnectionOk = StrFunc.IsFilled(csLog);
            if (isCheckConnectionOk)
            {
                try
                {
                    DataHelper.CheckConnection(csLog);
                }
                catch
                {
                    isCheckConnectionOk = false;
                }
            }

            SessionTools.ProcessLog = null;
            SessionTools.Logger = null;

            if (isCheckConnectionOk)
            {
                // FI 20200811 [XXXXX]
                // => Appel à SynchroDatesysCol pour que le 1er appel à OTCmlHelper.GetDateSys(cs) ne provoque pas l'ouverture d'une nouvelle connexion s'il existe une transaction courante 
                OTCmlHelper.SynchroDatesysCol(csLog);

                if (IsInitLog())
                {
                    SessionTools.InitLog(csLog);
                }
            }
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns></returns>
        /// PL 20150827 Newness 
        private static Boolean IsInitLog()
        {
            bool isInitLog = true;
            string session_HostNameNoLog = SystemSettings.GetAppSettings("Session_HostNameNoLog");
            if (!String.IsNullOrEmpty(session_HostNameNoLog))
            {
                string[] hostName_NoLog = session_HostNameNoLog.Split(';');
                if (hostName_NoLog.Length > 0)
                {
                    foreach (string hostName in hostName_NoLog)
                    {
                        if ((!String.IsNullOrEmpty(hostName)) && SessionTools.ServerAndUserHost.EndsWith(hostName))
                        {
                            isInitLog = false;
                            break;
                        }
                    }
                }
            }
            return isInitLog;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20170215 [XXXXX] Modify
        /// EG 20220623 [XXXXX] Implémentation Authentification via Shibboleth SP et IdP
        /// EG 20220629 [XXXXX] Implémentation Nouvelle page par defaut si non spécifiée
        /// EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            FirstRequestInitialization.Initialize(HttpContext.Current);

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //PL 20171206 A supprimer éventuellement dans le futur...
            //PL 20240226 Ajout d'un sous-test, car l'URL "http://<server>/Spheres" entre dans le premier IF(), tout comme l'URL "http://<server>/Spheres/"
            //            Sans ce nouveau test RewritePath soulevait l'erreur: The virtual path '/Default.aspx' maps to another application, which is not allowed
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            if (Request.AppRelativeCurrentExecutionFilePath == "~/")
            {
                if (Request.RawUrl.EndsWith("/"))
                {
                    //ex. "http://<server>/Spheres/"
                    HttpContext.Current.RewritePath("Default.aspx");
                }
                else
                {
                    //ex. "http://<server>/Spheres"
                    HttpContext.Current.RewritePath(Request.ApplicationPath + "/Default.aspx");
                }
            }
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            string SpheresCulture = string.Empty + Request.QueryString["Culture"];
            if (StrFunc.IsEmpty(SpheresCulture))
            {
                Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;

                HttpCookie cookie = Request.Cookies[AspTools.GetCookieName("UserDefault")];
                AspTools.ReadCookie(cookie, "Culture", out string s);
                if (StrFunc.IsFilled(s))
                {
                    SpheresCulture = s;
                    ret = Cst.ErrLevel.SUCCESS;
                }

                if (ret != Cst.ErrLevel.SUCCESS)
                {
                    if (ArrFunc.IsFilled(Request.UserLanguages))
                        SpheresCulture = Request.UserLanguages[0].Trim();
                    //
                    if (StrFunc.IsEmpty(SpheresCulture))
                        SpheresCulture = System.Globalization.CultureInfo.CurrentCulture.Name;
                }
            }
            //20061113 PL If() suivant ajouté
            //20150720 PL If() suivant déplacé
            if (SpheresCulture.Trim().Length == 2)
            {
                if (SpheresCulture == Cst.FrenchCulture.Substring(0, 2))
                    SpheresCulture = Cst.FrenchCulture;
                else if (SpheresCulture == Cst.ItalianCulture.Substring(0, 2))
                    SpheresCulture = Cst.ItalianCulture;
                else
                    SpheresCulture = Cst.EnglishCulture;
            }

            // FI 20170215 [XXXXX]
            ThreadTools.SetCurrentCulture(SpheresCulture);

            // 20060124 RD             
            if (!File.Exists(Request.PhysicalPath.ToString()))
            {
                string RelativeFilePath = Request.Url.AbsolutePath.Remove(0, Request.ApplicationPath.Length).ToLower();
                if (RelativeFilePath.IndexOf("portal") >= 0)
                {
                    string sIdMenu;
                    sIdMenu = Request.QueryString["IdMenu"];
                    Response.Redirect(Request.ApplicationPath.ToString() + "/UnderConstruction.aspx?IdMenu=" + sIdMenu, true);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_Error(Object sender, EventArgs e)
        {
            // RD 20200828 [25392]
            Exception ex = null;
            if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
                ex = (Exception)Session["currentError"];

            if (ex == null)
            {
                ex = Server.GetLastError();

                if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
                    Session["currentError"] = ex;
            }

            if ((ex is HttpException exception) && (exception.GetHttpCode() == 404))
            {
                //use web.config to find where we need to redirect
                var config = (CustomErrorsSection)WebConfigurationManager.GetSection("system.web/customErrors");
                string requestedUrl = HttpContext.Current.Request.RawUrl;
                string urlToRedirectTo = config.Errors["404"].Redirect;

                Response.Redirect(urlToRedirectTo + "?aspxerrorpath=" + requestedUrl + "&ErrorMessage=Page%20Not%20found", true);
            }
        }

        /// <summary>
        /// Session_End will fire when 
        ///  the Session has not been used in the timeout time defined.
        ///  after call Session.Abandon()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20110703 refactoring => spheres® n'utilise plus SessionTools car HttpContext.Current is null
        /// => Utilisation de HttpSessionStateTools
        /// FI 20120907 [18113] refactoring => usage de la variable session LstCSHistory
        /// FI 20160307 [XXXXX] Ajout de DeleteEventList et DeleteTradeList
        protected void Session_End(Object sender, EventArgs e)
        {
            // FI 20210108 [XXXXX] add Trace Info
            AppInstance.TraceManager.TraceInformation(this, $"Session End (id: {Session.SessionID},ShorId: {HttpSessionStateTools.ShortSessionId(Session)})");


            //COLLABORATOR n'est pas nécessaierement renseigné
            //(exemple: l'utilisateur ouvre spheres mais ne se connecte jamais
            //=> la session est finalement fermée par le server web et la variable SESSION ne contient pas COLLABORATOR
            //
            Collaborator clb = null;
            if (null != HttpSessionStateTools.Get(Session, "COLLABORATOR"))
                clb = HttpSessionStateTools.Get(Session, "COLLABORATOR") as Collaborator;
            if (null != clb && clb.Ida > 0)
            {
                //lorsque null != clb cela veut dire que l'utilisateur a quitter le browser sans faire un logout
                HttpConnectedUsers acu = new HttpConnectedUsers(Application);
                acu.RemoveConnectedUser(clb.Ida);
            }

            //Liste des connectionStrings valides
            List<string> lstCS = HttpSessionStateTools.Get(Session, "LstCSHistory") as List<string>;
            if (ArrFunc.IsFilled(lstCS))
            {
                foreach (string csItem in lstCS)
                {

                    LockTools.UnLockSession(csItem, Session.SessionID);

                    SqlSessionRestrict.CleanUp(csItem, Session.SessionID);

                    SessionClipBoard.CleanUp(csItem, Session.SessionID);

                    // FI 20140516 Call DropTemporaryTable
                    DropTemporaryTable(csItem, Session);

                    // FI 20160307 [XXXXX] ajout de DeleteEventList et DeleteTradeList
                    EventRDBMSTools.DeleteEventList(csItem, Session.SessionID);
                    TradeRDBMSTools.DeleteTradeList(csItem, Session.SessionID);
                }
            }

            // FI 20240112 [XXXXX] Appel à LogSessionEnd
            LogSessionEnd();

            // FI 20191227 [XXXXX] Add
            if (null != HttpSessionStateTools.Get(Session, AutoCompleteDataCache.Key))
                AutoCompleteDataCache.Clear();

            // FI 20200225 [XXXXX] Call DataCache.Clear
            if (null != HttpSessionStateTools.Get(Session, DataCache.Key))
                DataCache.Clear();
        }

        /// <summary>
        /// Alimentation du log avec Session End 
        /// </summary>
        /// FI 20240112 [WI804] Add Method
        private void LogSessionEnd()
        {
            //ProcessLog ou Logger sont initialisés  dans Session_Start
            if ((HttpSessionStateTools.Get(Session, "ProcessLog") != null) || (HttpSessionStateTools.Get(Session, "Logger") != null))
            {

                Collaborator clb = null;
                if (null != HttpSessionStateTools.Get(Session, "COLLABORATOR"))
                    clb = HttpSessionStateTools.Get(Session, "COLLABORATOR") as Collaborator;

                
                string identifier = "N/A";
                if (null != clb)
                    identifier = clb.Identifier;

                string ServerAndUserHost = ((null != HttpSessionStateTools.Get(Session, "HOSTNAME")) ? (Session["HOSTNAME"] as string) : string.Empty);
                AppSession appSession = ((null != HttpSessionStateTools.Get(Session, "AppSession")) ? (Session["AppSession"] as AppSession) : null);

                string browserInfo = string.Empty;
                if (null != appSession)
                    browserInfo = appSession.BrowserInfo;

                ProcessLog processLogSession = HttpSessionStateTools.Get(Session, "ProcessLog") as ProcessLog;
                LoggerScope loggerSession = HttpSessionStateTools.Get(Session, "Logger") as LoggerScope;


                // Attention identifier,sessionId, ServerAndUserHost  et browserInfo doivent être en phase avec l'alimentation de Session start
                // Voir InitProcessLog, ou InitLogger
                ProcessLogExtend logProcess = new ProcessLogExtend(processLogSession, loggerSession);
                logProcess.LogAddDetail(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 6), 0, new string[] { identifier, "SessionId [" + Session.SessionID + "]", ServerAndUserHost, browserInfo });

            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_End(object sender, EventArgs e)
        {
            // FI 20210108 [XXXXX] add Trace Info
            AppInstance.TraceManager.TraceInformation(this, $"Application End (Name: {Software.Name}, Version: {Software.Version})");

            // FI 20210111 [XXXXX] Call Dispose
            Application.Lock();
            if (null != Application["LOG"])
            {
                ErrorManager errorManager = (ErrorManager)Application["LOG"];
                errorManager.Dispose();
            }
            Application.UnLock();
        }

        /// <summary>
        /// Supprime les tables temporaires générées par une session
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="state">Représente la l'état de session</param>
        /// FI 20160127 [21574] Modify
        // EG 20180426 Analyse du code Correction [CA2202]
        private static void DropTemporaryTable(string pCS, HttpSessionState state)
        {
            try
            {
                string shortSessionId = HttpSessionStateTools.ShortSessionId(state);
                if (StrFunc.IsFilled(shortSessionId))
                {
                    string cmd;
                    if (DataHelper.IsDbSqlServer(pCS))
                    {
                        cmd = StrFunc.AppendFormat("select name from sysobjects where name like '%[_]{0}[_]W' and xtype='U'", shortSessionId.ToUpper());
                    }
                    else if (DataHelper.IsDbOracle(pCS))
                    {
                        cmd = StrFunc.AppendFormat("select TABLE_NAME from USER_TABLES where TABLE_NAME like '%#_{0}#_W' escape '#'", shortSessionId.ToUpper());
                    }
                    else
                        throw new NotImplementedException("RDBMS not implemented");

                    using (IDataReader dr = DataHelper.ExecuteReader(pCS, CommandType.Text, cmd))
                    {
                        while (dr.Read())
                        {
                            string option = string.Empty;
                            if (DataHelper.IsDbOracle(pCS))
                                option = "purge";

                            cmd = StrFunc.AppendFormat("drop table {0} {1}", dr[0].ToString(), option);

                            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, cmd);
                        }
                    }
                }
            }
            catch (Exception) { throw; }
        }

        protected void SecuritySwitch_EvaluateRequest(object sender, EvaluateRequestEventArgs e)
        {
            if (e.Context.Items["pageElements"] is Hashtable pageElements && e.Settings.Mode != Mode.Off)
            {
                e.ExpectedSecurity =
                    Convert.ToString(pageElements["forceHttps"]) == "1" ? RequestSecurity.Secure : RequestSecurity.Insecure;
            }
        }

        /// <summary>
        ///  Alimentation de Application à partir de la 1er Request
        /// </summary>
        /// https://mvolo.com/iis7-integrated-mode-request-is-not-available-in-this-context-exception-in-applicationstart/
        class FirstRequestInitialization

        {

            private static bool s_InitializedAlready = false;

            private static readonly Object s_lock = new Object();


            /// <summary>
            ///  Initialize only on the first request
            /// </summary>
            /// <param name="context"></param>
            public static void Initialize(HttpContext context)

            {
                if (s_InitializedAlready)
                {
                    return;
                }

                lock (s_lock)
                {
                    if (s_InitializedAlready)
                    {
                        return;
                    }
                    // Perform first-request initialization here …
                    // L'alimentation de HOSTNAME et APPINSTANCE devrait être effectué ici une fois pour toute plutôt que dans Session_Start

                    context.Application.Lock();
                    string hostname = new Uri(HttpContext.Current.Request.Url.ToString()).Host;
                    context.Application["HOSTNAME"] = hostname;

                    AppInstance.MasterAppInstance.HostName = hostname;
                    context.Application["APPINSTANCE"] = AppInstance.MasterAppInstance;

                    context.Application.UnLock();

                    s_InitializedAlready = true;
                }
            }
        }
    }
}