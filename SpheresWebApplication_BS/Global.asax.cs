using System;
using System.Data;
using System.Collections;
using System.Web;
using System.Web.SessionState;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Collections.Generic;

using EFS.Restriction;

//using Spheres;

using System.Web.Optimization;
using System.Web.Configuration;

using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;


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
            //RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Application.Lock();
            ArrayList alConnectedUsers = new ArrayList();
            Application["CONNECTEDUSERS"] = alConnectedUsers;
            //

            ErrorManager ErrMngr = new ErrorManager();
            ErrMngr.InitDatabase2(SystemSettings.GetLogConnectionString(false, string.Empty));
            ErrMngr.InitXMLLogFile(Server.MapPath(SystemSettings.GetAppSettings("Spheres_LogsXMLFile")));
            ErrMngr.InitEmail(SystemSettings.GetAppSettings("Spheres_ErrorEmailSmtpServer"));
            ErrMngr.InitEventLog("Application", Software.Name);
            //
            bool isEFSmLHelp = (bool)SystemSettings.GetAppSettings("EFSmLHelpSchemas", typeof(System.Boolean), false);
            if (isEFSmLHelp)
            {
                XmlDocument doc = new XmlDocument();
                string path = Server.MapPath(@"HelpSchemas\EFSmL_HelpIndexSchemas.xml");
                if (File.Exists(path))
                {
                    doc.Load(path);
                    Application["EFSmL_HelpIndexSchemas"] = doc;
                }
            }
            //
            bool isResetTemporaryFolder = (bool)SystemSettings.GetAppSettings("ResetTemporaryFolder", typeof(System.Boolean), true);
            if (isResetTemporaryFolder)
            {
                //PL 20091105 L'utilisation de "Request" est incompatible avec le mode "pipeline intégré" de IIS7.0
                //AppInstance appInstance = new AppInstance(1, "SYSTEM", "Global.asax", "WebServer",
                //    Software.Name, Software.VersionBuild, "N/A", HttpContext.Current.Request.MapPath(string.Empty));
                //PL 20101125
                AppInstance appInstance = new AppInstance();
                appInstance.IdA = 1;
                appInstance.IdA_Identifier = "SYSTEM";
                appInstance.SessionId = "Global.asax";
                appInstance.HostName = "WebServer";
                //
                AppInstance.CleanTemporaryDirectory(Server.MapPath(@"~/Temporary"));
            }
            //
            Application["LOG"] = ErrMngr;
            //
            Application["ISDATACACHEENABLED"] = BoolFunc.IsTrue(SystemSettings.GetAppSettings("DataCacheEnabled", "false"));
            //
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheEnabledInterval")))
                DataHelper.queryCache.timeSpanDataEnabled = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheEnabledInterval"));
            //
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheCleanInterval")))
                DataHelper.queryCache.timeSpanDataClean = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheCleanInterval"));
            //
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheMinRows")))
                DataHelper.queryCache.minRowsInResult = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheMinRows"));
            //
            if (StrFunc.IsFilled(SystemSettings.GetAppSettings("DataCacheMaxRows")))
                DataHelper.queryCache.maxRowsInResult = IntFunc.IntValue(SystemSettings.GetAppSettings("DataCacheMaxRows"));

            //
            Application.UnLock();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Session_Start(Object sender, EventArgs e)
        {
            SessionTools.ConnectionState = Cst.ConnectionState.INIT;
            SessionTools.LogMessageCulture = CultureInfo.CreateSpecificCulture("en-GB");

            SessionTools.Collaborator = null;
            SessionTools.HostName = string.Empty;
            SessionTools.BrowserInfo = string.Empty;

            string csLog = SystemSettings.GetLogConnectionString(false, System.Web.HttpContext.Current.Request.ApplicationPath);

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

            SessionTools.processLog = null;
            if (isCheckConnectionOk)
            {
                //PL 20150827 Newness 
                bool isInitProcessLog = true;
                string session_HostNameNoLog = SystemSettings.GetAppSettings("Session_HostNameNoLog");
                if (!String.IsNullOrEmpty(session_HostNameNoLog))
                {
                    string[] hostName_NoLog = session_HostNameNoLog.Split(';');
                    if (hostName_NoLog.Length > 0)
                    {
                        foreach (string hostName in hostName_NoLog)
                        {
                            if ((!String.IsNullOrEmpty(hostName)) && SessionTools.HostName.EndsWith(hostName))
                            {
                                isInitProcessLog = false;
                                break;
                            }
                        }
                    }
                }
                if (isInitProcessLog)
                {
                    SessionTools.InitProcessLog(csLog);
                }
                // ticket 17743 blocked activity to make Oracle case insensitive via globalization parameters
                //// set compare strategy RDBMS specific
                //DataHelper.SetCultureParameter(CultureParameter.Compare, csLog);
                //// set sort strategy RDBMS specific
                //DataHelper.SetCultureParameter(CultureParameter.Sort, csLog);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20170215 [XXXXX] Modify
        protected void Application_BeginRequest(Object sender, EventArgs e)
        {
            string SpheresCulture = string.Empty + Request.QueryString["Culture"];
            if (StrFunc.IsEmpty(SpheresCulture))
            {
                HttpCookie cookie = null;
                Cst.ErrLevel ret = Cst.ErrLevel.DATANOTFOUND;

                cookie = Request.Cookies[AspTools.GetCookieName("UserDefault")];
                string s;
                AspTools.ReadCookie(cookie, "Culture", out s);
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

            // FI 20170215 [XXXXX] call ThreadTools.SetCurrentCulture
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
            Exception ex = Server.GetLastError();
            if ((ex is HttpException) && (((HttpException)ex).GetHttpCode() == 404))
            {
                //use web.config to find where we need to redirect
                var config = (CustomErrorsSection)WebConfigurationManager.GetSection("system.web/customErrors");
                string requestedUrl = HttpContext.Current.Request.RawUrl;
                string urlToRedirectTo = config.Errors["404"].Redirect;
                //HttpContext.Current.Server.Transfer(urlToRedirectTo + "&requestedUrl=" + requestedUrl);
                Response.Redirect(urlToRedirectTo + "&requestedUrl=" + requestedUrl, true);
            }
            else
            {
                if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
                {
                    // Session exists
                    Session["currentError"] = ex;
                }
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
            HttpSessionState state = Session;

            //
            //COLLABORATOR n'est pas nécessaierement renseigné
            //(exemple: l'utilisateur ouvre spheres mais ne se connecte jamais
            //=> la session est finalement fermée par le server web et la variable SESSION ne contient pas COLLABORATOR
            //
            Collaborator clb = null;
            if (null != HttpSessionStateTools.Get(Session, "COLLABORATOR"))
                clb = HttpSessionStateTools.Get(Session, "COLLABORATOR") as Collaborator;
            //
            if (null != clb && clb.Ida > 0)
            {
                //lorsque null != clb cela veut dire que l'utilisateur a quitter le browser sans faire un logout
                HttpConnectedUsers acu = new HttpConnectedUsers(Application);
                acu.RemoveConnectedUser(clb.Ida);
            }
            //
            //Liste des connectionStrings valides
            List<string> lstCS = HttpSessionStateTools.Get(Session, "LstCSHistory") as List<string>;
            if (ArrFunc.IsFilled(lstCS))
            {
                foreach (string csItem in lstCS)
                {

                    LockTools.UnLockSession(csItem, state.SessionID);

                    SqlSessionRestrict.Dispose(csItem, state.SessionID);

                    SessionClipBoard.Dispose(csItem, state.SessionID);

                    // FI 20140516 Call DropTemporaryTable
                    DropTemporaryTable(csItem, state);

                    // FI 20160307 [XXXXX] ajout de DeleteEventList et DeleteTradeList
                    EventRDBMSTools.DeleteEventList(csItem, state.SessionID);
                    TradeRDBMSTools.DeleteTradeList(csItem, state.SessionID);
                }
            }
            //
            //ProcessLog et LogMessageCulture sont initialisés  dans Session_Start
            if (HttpSessionStateTools.Get(Session, "ProcessLog") != null)
            {
                ProcessLog processLog = Session["ProcessLog"] as ProcessLog;
                CultureInfo logMessageCulture = Session["LogMessageCulture"] as CultureInfo;
                //
                string identifier = string.Empty;
                Collaborator.AuthenticationTypeEnum authenticationType = Collaborator.AuthenticationTypeEnum.NA;
                if (null != clb)
                {
                    identifier = clb.Identifier;
                    authenticationType = clb.AuthenticationType;
                }
                //
                //Recupération des informations qui concerne la base de donnée sur laquelle l'utilisateur était connecté 
                //à travars cette session
                string rdbms = string.Empty;
                if (null != HttpSessionStateTools.Get(Session, "RDBMS"))
                    rdbms = Session["RDBMS"] as string;
                //
                //FI 20120911 alimentation de la variable server
                //Il y avait là une boulette, puisque rdbms était écrasé
                string server = string.Empty;
                if (null != HttpSessionStateTools.Get(Session, "SERVER"))
                    server = Session["SERVER"] as string;
                //
                string database = string.Empty;
                if (null != HttpSessionStateTools.Get(Session, "DATABASE"))
                    database = Session["DATABASE"] as string;
                //
                Login_Log.AddProcessLog(processLog, logMessageCulture, ProcessStateTools.StatusSuccessEnum,
                                        ActionLogin.Timeout, identifier, authenticationType, rdbms, server, database, null);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Application_End(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Supprime les tables temporaires générées par une session
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="state">Représente la l'état de session</param>
        /// FI 20160127 [21574] Modify
        private static void DropTemporaryTable(string pCS, HttpSessionState state)
        {

            IDataReader dr = null;
            string cmd = string.Empty;
            try
            {
                string shortSessionId = HttpSessionStateTools.ShortSessionId(state);
                if (StrFunc.IsFilled(shortSessionId))
                {
                    if (DataHelper.isDbSqlServer(pCS))
                    {
                        cmd = StrFunc.AppendFormat("select name from sysobjects where name like '%[_]{0}[_]W' and xtype='U'", shortSessionId.ToUpper());
                    }
                    else if (DataHelper.isDbOracle(pCS))
                    {
                        cmd = StrFunc.AppendFormat("select TABLE_NAME from USER_TABLES where TABLE_NAME like '%#_{0}#_W' escape '#'", shortSessionId.ToUpper());
                    }
                    else
                        throw new NotImplementedException("RDBMS not implemented");

                    dr = DataHelper.ExecuteReader(pCS, CommandType.Text, cmd);
                    while (dr.Read())
                    {
                        string option = string.Empty;
                        if (DataHelper.isDbOracle(pCS))
                            option = "purge";

                        cmd = StrFunc.AppendFormat("drop table {0} {1}", dr[0].ToString(), option);

                        DataHelper.ExecuteNonQuery(pCS, CommandType.Text, cmd);
                    }
                }
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    dr.Close();
                    dr.Dispose();
                    dr = null;
                }
            }
        }
    }
}