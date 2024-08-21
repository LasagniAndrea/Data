//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Security;
using System.Web.UI;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{

    public enum ActionLogin
    {
        ChangeDB,
        Login,
        Logout,
        Timeout,
        Fail,
    }
    
    
    /// <summary>
    /// 
    /// </summary>
    public sealed class Disconnect
    {
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pUrlRedirect"></param>
        /// EG 20210614 [25500] New Customer Portal
        public static void Run(Page pPage)
        {
            Run(pPage, null);
        }
        /// EG 20210614 [25500] New Customer Portal (Redirection vers la page Welcome du Portail)
        /// EG 20220623 [XXXXX] Refactoring (Shibboleth)
        public static void Run(Page pPage, string pRedirectPage)
        {
            
            SessionTools.CleanUp();
            

            if (SessionTools.IsConnected)
            {
                HttpConnectedUsers acu = new HttpConnectedUsers(HttpContext.Current.Application);
                acu.RemoveConnectedUser(SessionTools.Collaborator_IDA);

                Login_Log.Logout(SessionTools.CS, SessionTools.Collaborator_IDA);

                SessionTools.LogAddLoginInfo(LogLevelEnum.Info, ActionLogin.Logout,
                                        new string[] { SessionTools.Collaborator_IDENTIFIER, SessionTools.Collaborator_AuthenticationType.ToString(),
                                        SessionTools.Data.RDBMS, SessionTools.Data.Server, SessionTools.Data.Database });

                //FI 20120911 Spheres® stocke la dernière CS utilisé avant la déconnexion
                //La méthode ResetLoginInfos supprime cette information
                SessionTools.LastConnectionString = SessionTools.CS;
                SessionTools.ResetLoginInfos();
                SessionTools.ConnectionState = Cst.ConnectionState.LOGOUT;
            }


            // Lorsque la session est en timeout Spheres® force le SignOut, la requête HTTP n'est plus authentifiée 
            // Spheres affiche alors login.aspx (natif à asp lorsque l'authentification est perdue), puis Spheres® Charge recharche la page initialement demandée 
            FormsAuthentication.SignOut();
            // EG 20220623 [XXXXX] Refactoring (Shibboleth)
            // L'usage de "None" permet de ne pas faire de Redirect et de laisser à l'appelant de cette méthode
            // le choix de l'ouverture de la page.
            // Cas du Logout (avec Shibboleth SP) où le Redirect dans un iFrame d'une forme Cross-Site est rejeté par les règles de 
            // sécurité
            if (String.IsNullOrEmpty(pRedirectPage))
                pPage.Response.Redirect(pPage.Request.Url.AbsoluteUri); //Permet l'affichage de la page initialement demandée
            else if ("None" != pRedirectPage)
                pPage.Response.Redirect(pRedirectPage); //Permet l'affichage de la page initialement demandée
        }
    }

    #region class Login_Log
    public sealed class Login_Log
    {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pSession">Représente la session courante</param>
        /// <param name="opDtPreviousLogin">Retourne la date de précédente connexion (date UTC) de l'utilisateur de la session courante</param>
        /// EG 20180423 Analyse du code Correction [CA2200]
        /// FI 20200728 [XXXXX] Refactoring (Alimentation de opDtPreviousLogin. Les colonnes DTFIRSTLOGIN, DTLASTLOGIN sont alimentées avec des date UTC)
        /// PL 20200729 [XXXXX] Manage new columns (DTPENULTLOGIN, ..., PENULTAPPBROWSER)
        public static void Login(string pCS, AppSession pSession, out DateTime opDtPreviousLogin)
        {
            // FI 20200728 [XXXXX] call GetLastLogin
            opDtPreviousLogin = GetLastLogin(pCS, pSession.IdA);

            DataParameters dataParameters;
            string SQLQuery = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LOGIN_L.ToString() + SQLCst.SET;
            SQLQuery += "DTPENULTLOGIN = DTLASTLOGIN, DTPENULTLOGOUT = DTLASTLOGOUT" + Cst.CrLf;
            SQLQuery += " ,PENULTHOSTNAME = HOSTNAME, PENULTAPPNAME = APPNAME, PENULTAPPVERSION = APPVERSION, PENULTAPPBROWSER = APPBROWSER" + Cst.CrLf;
            SQLQuery += " ,DTLASTLOGIN = @DTSYS";
            SQLQuery += " ,DTLASTLOGOUT = " + SQLCst.NULL + Cst.CrLf;
            SQLQuery += " ,HOSTNAME = @HOSTNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER" + Cst.CrLf;
            SQLQuery += " ,TOTALLOGIN = TOTALLOGIN+1" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "IDA = @IDA";

            // FI 20200820 [25468] Date systèmes en UTC (ici DTSYS)
            dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pSession.IdA);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.HOSTNAME), pSession.AppInstance.HostName);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPVERSION), pSession.AppInstance.AppVersion);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPBROWSER), pSession.BrowserInfo);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.APPNAME), pSession.AppInstance.AppName);
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.DTSYS), OTCmlHelper.GetDateSysUTC(pCS));

            int nbRow = DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery, dataParameters.GetArrayDbParameter());
            if (nbRow == 0)
            {
                SQLQuery = StrFunc.AppendFormat(@"{0} (IDA, DTFIRSTLOGIN, DTLASTLOGIN, HOSTNAME, APPNAME, APPVERSION, APPBROWSER, TOTALLOGIN) 
values (@IDA, @DTSYS, @DTSYS, @HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, 1)", SQLCst.INSERT_INTO_DBO + Cst.OTCml_TBL.LOGIN_L.ToString());

                DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery, dataParameters.GetArrayDbParameter());
            }
        }

        /// <summary>
        /// Writing into LOGIN_L.DTLASTLOGOUT
        /// </summary>
        public static void Logout(string pCS, int pIdA)
        {
            // FI 20200728[XXXXX] DTLASTLOGOUT alimenté avec une date UTC
            string SQLQuery = string.Empty;
            SQLQuery += SQLCst.UPDATE_DBO + Cst.OTCml_TBL.LOGIN_L.ToString() + SQLCst.SET;
            SQLQuery += "DTLASTLOGOUT = " + DataHelper.SQLGetDate(SessionTools.CS, true) + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "IDA = @IDA";

            DataParameter dbParam = DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA);
            dbParam.Value = pIdA;

            DataHelper.ExecuteNonQuery(pCS, CommandType.Text, SQLQuery, dbParam.DbDataParameter);
        }

        /// <summary>
        ///  Retourne l'horodatage de la précédente connexion de l'utilisateur {pIdA}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        /// FI 20200728 [XXXXX] Add 
        public static DateTime GetLastLogin(string pCS, int pIdA)
        {
            DateTime ret = DateTime.MinValue;

            string SQLQuery = SQLCst.SELECT + "DTLASTLOGIN" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.LOGIN_L.ToString() + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "IDA = @IDA";
            DataParameters dataParameters = new DataParameters();
            dataParameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDA), pIdA);

            // 20070717 FI utilisation de ExecuteScalar et SetCacheOff
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, SQLQuery, dataParameters.GetArrayDbParameter());
            if (null != obj)
                ret = DateTime.SpecifyKind(Convert.ToDateTime(obj), DateTimeKind.Utc);
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="logProcess"></param>
        /// <param name="logLevel"></param>
        /// <param name="pActionProcess"></param>
        /// <param name="pData"></param>
        /// FI 20240111 [WI793] Refactoring avec usage de logprocess
        public static void AddProcessLog(ProcessLogExtend logProcess, LogLevelEnum logLevel, ActionLogin pActionProcess, string[] pData)
        {
            SysMsgCode sysMsgCode;
            switch (pActionProcess)
            {
                case ActionLogin.Login:
                    sysMsgCode = new SysMsgCode(SysCodeEnum.LOG, 3);
                    break;
                case ActionLogin.Logout:
                    sysMsgCode = new SysMsgCode(SysCodeEnum.LOG, 4);
                    break;
                case ActionLogin.ChangeDB:
                    sysMsgCode = new SysMsgCode(SysCodeEnum.LOG, 5);
                    break;
                case ActionLogin.Fail:
                    sysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 2);
                    break;

                case ActionLogin.Timeout:
                    sysMsgCode = new SysMsgCode(SysCodeEnum.SYS, 1);
                    break;
                default:
                    throw new NotImplementedException($"{pActionProcess} is not implemented");
            }
            logProcess.LogAddDetail(logLevel, sysMsgCode, 0, pData);
        }
    }
    #endregion class Login_Log

    #region class HttpConnectedUsers
    public class HttpConnectedUsers
    {
        private readonly HttpApplicationState _httpApplication;

        #region constructor
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pApplication"></param>
        public HttpConnectedUsers(HttpApplicationState pApplication)
        {
            _httpApplication = pApplication;
        }
        /// <summary>
        /// 
        /// </summary>
        public HttpConnectedUsers()
        {
            _httpApplication = HttpContext.Current.Application;
        }
        #endregion constructor

        /// <summary>
        /// Ajoute une connexion à l'acteur {pIdA}
        /// <para>Retourne LOGINSIMULTANEOUS_ALLUSER si le nbr max de connexion pour tous les utilisateurs est atteint</para>
        /// <para>Retourne LOGINSIMULTANEOUS_USER si le nbr max de connexion pour l'utilisateur est atteint</para>
        /// </summary>
        /// <param name="pIdA"></param>
        /// <param name="pIdModelSafety"></param>
        /// <param name="pMaxAuthorized">Nbr max de connection pour l'utilisateur</param>
        /// <param name="pMaxSimultaneousLoginAuthorized">Nbr max de connection pour tous les utilisateurs</param>
        /// <returns></returns>
        public Cst.ErrLevel AddConnectedUser(int pIdA, int pIdModelSafety, int pMaxAuthorized, int pMaxSimultaneousLoginAuthorized)
        {
            Cst.ErrLevel errLevel;
            try
            {
                ConnectedUsers cus = Read();
                //Contrôle le nombre d'utilisateurs simultanés
                int countForAll = cus.GetNbConnectionAllUsers();
                if (countForAll >= pMaxSimultaneousLoginAuthorized)
                {
                    errLevel = Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER;
                }
                else
                {
                    //Contrôle le nombre d'utilisateurs simultanés pour cet utilisateur
                    int countForIDA = cus.GetNbConnection(pIdA);
                    //(pMaxAuthorized == -1) --> unlimited
                    if ((pMaxAuthorized == -1) || (countForIDA < pMaxAuthorized))
                    {
                        cus.Modify(pIdA, pIdModelSafety, ++countForIDA);
                        Write(cus);
                        errLevel = Cst.ErrLevel.SUCCESS;
                    }
                    else
                    {
                        errLevel = Cst.ErrLevel.LOGINSIMULTANEOUS_USER;
                    }
                }
            }
            catch
            {
                errLevel = Cst.ErrLevel.FAILURE;
            }
            return errLevel;
        }
        
        /// <summary>
        /// Retourne la collection qui représente les utilisateurs connectés dans application
        /// </summary>
        /// <returns></returns>
        public ConnectedUsers GetConnectedUsers()
        {
            ConnectedUsers cus = Read();
            return cus;
        }
        
        /// <summary>
        /// Retire une connexion à l'acteur connecté {pIdA}
        /// </summary>
        /// <param name="pIdA"></param>
        /// <returns></returns>
        public Cst.ErrLevel RemoveConnectedUser(int pIdA)
        {
            Cst.ErrLevel errLevel;
            try
            {
                ConnectedUsers cus = Read();
                int count = cus.GetNbConnection(pIdA);
                cus.Modify(pIdA, 0, --count);
                Write(cus);
                errLevel = Cst.ErrLevel.SUCCESS;
            }
            catch
            {
                errLevel = Cst.ErrLevel.FAILURE;
            }
            return errLevel;
        }
        
        /// <summary>
        /// Lecture de la collection qui représente les utilisateurs connectés dans application
        /// </summary>
        /// <returns></returns>
        private ConnectedUsers Read()
        {
            ConnectedUsers cus = null;
            _httpApplication.Lock();
            try
            {
                cus = (ConnectedUsers)_httpApplication["USERS"];
            }
            catch
            { }
            finally
            {
                if (null == cus)
                    cus = new ConnectedUsers();
            }
            return cus;
        }
        /// <summary>
        /// Eccrirure de le collection qui représente les utilisateurs connectés dans Application
        /// </summary>
        private void Write(ConnectedUsers pConnectedUsers)
        {
            try
            {
                _httpApplication["USERS"] = pConnectedUsers;
            }
            finally
            {
                _httpApplication.UnLock();
            }
        }

        
    }
    #endregion class ApplicationConnectedUsers

    #region class CheckWebConnection
    public static class CheckWebConnection
    {
        #region public CheckSimultaneousLogin
        public static bool CheckSimultaneousLogin(Collaborator pCollaborator, int pMaxSimultaneousLoginAuthorized, ref string pErrMessage)
        {
            //Check and set for simultaneous login
            bool isOk = false;
            //
            HttpConnectedUsers acu = new HttpConnectedUsers();
            Cst.ErrLevel acuErrLevel = acu.AddConnectedUser(pCollaborator.Ida, pCollaborator.IdModelSafety, pCollaborator.SimultaneousLogin, pMaxSimultaneousLoginAuthorized);
            //
            switch (acuErrLevel)
            {
                case Cst.ErrLevel.SUCCESS:
                    isOk = true;
                    break;
                case Cst.ErrLevel.LOGINSIMULTANEOUS_ALLUSER:
                    if (pCollaborator.IsActorSysAdmin)
                        pErrMessage = "FailureConnect_SimultaneousLoginAll_Det";
                    else
                        pErrMessage = "FailureConnect_SimultaneousLoginAll";
                    break;
                case Cst.ErrLevel.LOGINSIMULTANEOUS_USER:
                default:
                    pErrMessage = "FailureConnect_SimultaneousLogin";
                    break;
            }
            return isOk;
        }
        #endregion CheckSimultaneousLogin

        #region public ReadRegistration
        public static bool ReadRegistration(string pSource, ref int opMaxSimultaneousLoginAuthorized, ref int opMaxEntityAuthorized,
            ref string opErrResource, ref string opErrMessage, ref string opDebugStep)
        {
            bool isOk = false;
            int maxSimultaneousLoginAuthorized = -1;
            int maxEntityAuthorized = -1;
            string errResource = string.Empty;
            opDebugStep = "ReadRegistration";
            opErrMessage = string.Empty;
            try
            {
                opDebugStep = "License.Load"; 
                SessionTools.License = License.Load(pSource, Software.Name);
                if ((SessionTools.License == null) || (SessionTools.License.licensee == null))
                {
                    opDebugStep = "License.Missing";
                }
                else
                {
                    opDebugStep = "License.Registration";
                    isOk = SessionTools.License.IsValidRegistration(ref maxSimultaneousLoginAuthorized, ref maxEntityAuthorized);
                }
                
                #region User License (System\RDBMS)
                DbSvrType svrType = DbSvrType.dbUNKNOWN;
                if (isOk)
                {
                    opDebugStep = "License.RDBMS"; 
                    svrType = DataHelper.GetDbSvrType(pSource);
                    if (svrType == DbSvrType.dbORA)
                    {
                        isOk = 
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL9I2) || 
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL10G) || 
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL11G) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL12C) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL18C) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.ORACL19C);
                        if (!isOk)
                            errResource = "FailureConnect_RDBMSOracle";
                    }
                    else if (svrType == DbSvrType.dbSQL)
                    {
                        isOk =
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K) || 
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K5) || 
                            //SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K8) || 
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K12) || 
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K14) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K16) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K17) || 
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K19) ||
                            SessionTools.License.IsLicSystemAuthorised(LimitationSystemEnum.SQLSRV2K22);
                        if (!isOk)
                            errResource = "FailureConnect_RDBMSSQLServer";
                    }
                    else
                    {
                        isOk = false;
                        errResource = "FailureConnect_RDBMSMiscellaneous";
                    }
#if DEBUG
                    isOk = true;
#endif
                }
                #endregion
                #region Check Trial Version //20070621 PL
                if (isOk && SessionTools.License.trialSpecified)
                {
                    opDebugStep = "License.Trial"; 
                    SessionTools.License.trialRemaining = -1;
                    int tmp_trialRemaining = 0;
                    try
                    {
                        #region Check from Database
                        if (isOk)
                        {
                            string sqlSelect = SQLCst.SQL_RDBMS + Cst.CrLf;
                            //Calcul du nombre de jours écoulés entre la création de la table ACTOR et la date jour
                            if (svrType == DbSvrType.dbORA)
                            {
                                sqlSelect += "select ceil( to_date(sysdate) - to_date(created) ) from user_objects where object_name = 'ACTOR'";
                            }
                            else if (svrType == DbSvrType.dbSQL)
                            {
                                sqlSelect += "select datediff(day,crdate,getdate()) from sysobjects where name ='ACTOR' and type='U'";
                            }
                            
                            object obj = DataHelper.ExecuteScalar(pSource, CommandType.Text, sqlSelect);
                            if (null != obj)
                            {
                                int elapsedDays = Convert.ToInt32(obj);
                                tmp_trialRemaining = Math.Max(0, SessionTools.License.trial - elapsedDays);
                            }

                            isOk = (tmp_trialRemaining > 0);
                            if ((SessionTools.License.trialRemaining == -1) || (tmp_trialRemaining < SessionTools.License.trialRemaining))
                            {
                                SessionTools.License.trialRemaining = tmp_trialRemaining;
                            }
                        }
                        #endregion

                        #region Check from EFSSOFTWARE table
                        if (isOk)
                        {
                            string sqlSelect = SQLCst.SELECT + "DATASYSTEM" + Cst.CrLf;
                            sqlSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
                            sqlSelect += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
                            
                            string tmp = string.Empty;
                            object obj = DataHelper.ExecuteScalar(pSource, CommandType.Text, sqlSelect);
                            if (null != obj)
                            {
                                tmp = Convert.ToString(obj);
                            }
                            
                            if (StrFunc.IsEmpty(tmp))
                            {
                                //1ère exécution ou ...: On alimente la colonne DATASYSTEM avec la date jour
                                string sqlUpdate = SQLCst.UPDATE_DBO + Cst.OTCml_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
                                sqlUpdate += "set DATASYSTEM=" + DataHelper.SQLString(Cryptography.Encrypt(DtFunc.DateTimeToStringDateISO(OTCmlHelper.GetDateSys(pSource)))) + Cst.CrLf;
                                sqlUpdate += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);
                                DataHelper.ExecuteNonQuery(pSource, CommandType.Text, sqlUpdate);
                            }
                            else
                            {
                                //Calcul du nombre de jours écoulés entre la date stockée dans EFSSOFTWARE.DATASYSTEM et la date jour
                                tmp = Cryptography.Decrypt(tmp);
                                TimeSpan ts = OTCmlHelper.GetDateSys(pSource) - new DtFunc().StringDateISOToDateTime(tmp);
                                tmp_trialRemaining = Math.Max(0, SessionTools.License.trial - (int)ts.TotalDays);
                                
                                isOk = (tmp_trialRemaining > 0);
                                if ((SessionTools.License.trialRemaining == -1) || (tmp_trialRemaining < SessionTools.License.trialRemaining))
                                {
                                    SessionTools.License.trialRemaining = tmp_trialRemaining;
                                }
                            }
                        }
                        isOk = (SessionTools.License.trialRemaining > 0);
                        #endregion

                        #region Check from File
                        if (isOk)
                        {
                            string fileName = HttpContext.Current.Server.MapPath(@"Temporary/DoNotErase.txt");
                            if (File.Exists(fileName))
                            {
                                bool isExistLine = false;
                                bool isFound = false;
                                string line = string.Empty;
                                StreamReader srSource = new StreamReader(fileName, System.Text.Encoding.Default);
                                while ((line = srSource.ReadLine()) != null)
                                {
                                    isExistLine = true;
                                    line = Cryptography.Decrypt(line);
                                    if (line.StartsWith("trial:"))
                                    {
                                        isFound = true;
                                        break;
                                    }
                                }
                                srSource.Close();

                                if (!isExistLine)
                                {
                                    try
                                    {
                                        //PL 20210315 Pour éviter la maj du fichier en mode DBUG, et ainsi son archivage intempestif.
                                        #if !DEBUG
                                        StreamWriter srOutput = new StreamWriter(fileName, false, System.Text.Encoding.Default);
                                        srOutput.Write(Cryptography.Encrypt("trial:" + DtFunc.DateTimeToStringDateISO(DateTime.Today)));
                                        srOutput.Close();
                                        #endif
                                    }
                                    catch { }
                                }
                                else
                                {
                                    if (isFound)
                                    {
                                        string tmp = line.Substring("trial:".Length);
                                        TimeSpan ts = DateTime.Today - new DtFunc().StringDateISOToDateTime(tmp);
                                        tmp_trialRemaining = Math.Max(0, SessionTools.License.trial - (int)ts.TotalDays);
                                        //
                                        isOk = (tmp_trialRemaining > 0);
                                        if ((SessionTools.License.trialRemaining == -1) || (tmp_trialRemaining < SessionTools.License.trialRemaining))
                                            SessionTools.License.trialRemaining = tmp_trialRemaining;
                                    }
                                    else
                                    {
                                        SessionTools.License.trialRemaining = 0;
                                    }
                                }
                            }
                            else
                            {
                                SessionTools.License.trialRemaining = 0;
                            }
                        }
#endregion
                    }
                    catch
                    {
                        isOk = false;
                    }
                    if (!isOk)
                    {
                        errResource = "FailureConnect_TrialExpiry";
                    }
                }
#endregion
            }
            catch (Exception ex)
            {
                isOk = false; 
                opErrMessage = ex.Message;
                errResource = "FailureConnect_RDBMSError";
            }
            finally
            {
                if (isOk)
                {
                    isOk = (maxSimultaneousLoginAuthorized > 0);
                }
                if (isOk)
                {
                    isOk = (maxEntityAuthorized > 0);
                }
                if (isOk)
                {
                    opMaxSimultaneousLoginAuthorized = maxSimultaneousLoginAuthorized;
                    opMaxEntityAuthorized = maxEntityAuthorized;
                }
                //
                if ((!isOk) && StrFunc.IsEmpty(errResource))
                {
                    errResource = "FailureConnect_License";
                }
            }
            opErrResource = errResource;
            return isOk;
        }
#endregion ReadRegistration
    }
#endregion


#region HackAttemptSafety
    /// <summary>
    ///  Paramètres pour le management des intrusions de connection
    ///  dans le fichier config (web.config | webCustom.config)
    /// </summary>
    /// EG 20151215 [21305] New
    public class HackAttemptSafety
    {
#region Constants
        private readonly string appSettingPrefix = "HackAttemptSafety_{0}_";
#endregion Constants
#region Accessors
        /// <summary>
        /// Nombre maximal de tentatives infructueuses avant verrouillage du Compte|Terminal. 
        /// Non renseigné ou 0 signifie qu'il n'y a aucune limite.
        /// </summary>
        /// EG 20151215 [21305] New
        public int MaxDenials
        {
            get;
            set;
        }
        /// <summary>
        /// Determine le niveau atteint pour affichage d'un warning informatif avant verrouillage du Compte|Terminal. 
        /// Non renseigné signifie que le warning interviendra à MaxDenials - 1.
        /// </summary>
        /// EG 20151215 [21305] New
        public int MaxDenialsWarning
        {
            get;
            set;
        }
        /// <summary>
        /// Durée d'attente avant la réinitialisation du compteur de tentatives infructueuses (en minutes). 
        /// Non renseigné ou 0 signifie qu'il n'y a aucune réinitialisation automatique, un administrateur devra intervenir.
        /// </summary>
        /// EG 20151215 [21305] New
        public int ResetTime
        {
            get;
            set;
        }
        /// <summary>
        /// Facteur appliqué à la valeur « ResetTime » associée, à chaque nouveau verrouillage successif.
        /// Non renseigné ou 0 signifie qu'il n'y a aucun facteur à appliquer.
        /// Exemple : la valeur 2 associé à un ResetTime de 15mn entraine un second ResetTime de 30mn, puis un troisième ResetTime de 60mn, …
        /// </summary>
        /// EG 20151215 [21305] New
        public int FactorResetTime
        {
            get;
            set;
        }
        /// <summary>
        /// Nombre maximal de verrouillage successif autorisé avec réinitialisation du compteur de tentatives infructueuses. 
        /// Une fois ce nombre atteint le compte|Terminal n'est plus réinitialisé automatiquement, un administrateur devra intervenir.
        /// Non renseigné ou 0 signifie qu'il n'y a aucune limite.
        /// </summary>
        /// EG 20151215 [21305] New
        public int MaxResetTime
        {
            get;
            set;
        }
#endregion Accessors
#region Constructors
        public HackAttemptSafety()
        {
        }
        /// <summary>
        /// Alimentation des données lues dans le web.config|webCustom.config
        /// </summary>
        /// <param name="pType">UserName|HostName</param>
        public HackAttemptSafety(string pType)
        {
            string prefix = String.Format(appSettingPrefix, pType);
            MaxDenials = (int)SystemSettings.GetAppSettings(prefix + "MaxDenials", typeof(Int32), 0);
            MaxDenialsWarning = (int)SystemSettings.GetAppSettings(prefix + "MaxDenialsWarning", typeof(Int32), 1);
            ResetTime = (int)SystemSettings.GetAppSettings(prefix + "ResetTime", typeof(Int32), 0);
            FactorResetTime = (int)SystemSettings.GetAppSettings(prefix + "FactorResetTime", typeof(Int32), 0);
            MaxResetTime = (int)SystemSettings.GetAppSettings(prefix + "MaxResetTime", typeof(Int32), 0);
        }
#endregion Constructors
    }
#endregion HackAttemptSafety

    /// <summary>
    /// Statut d'intrusion
    /// NOLOCK = Non verrouillé
    /// LOCKTIME = Verrouillage temporaire
    /// LOCKOUT = Verrouillage définitif (intervention administrateur)
    /// </summary>
    /// EG 20151215 [21305] New 
    public enum LockoutStatus
    {
        NOLOCK,
        LOCKTIME,
        LOCKOUT,
    }
    /// <summary>
    /// Code retour de contrôle des intrusions
    /// FAILURE = Code en erreur (via catch)
    /// LOCKTIME = Verrouillage temporaire
    /// LOCKOUT = Verrouillage définitif (intervention administrateur)
    /// SUCCESS = Pas de verrouillage
    /// </summary>
    /// EG 20151215 [21305] New
    public enum LockoutReturn
    {
        FAILURE,
        LOCKOUT,
        LOCKTIME,
        SUCCESS,
    }

#region LockSafety
    /// <summary>
    /// Gestion des verrouillage de compte|Terminal dans la table LOCKOUT
    /// - Gestion des intrusions par Utilisateur : UserLockout
    /// - Gestion des instrusion par Terminal : HostLockout
    /// </summary>
    /// EG 20151215 [21305] New
    public class LockSafety
    {
#region Members
        /// <summary>
        /// Instrusion utilisateur
        /// </summary>
        public UserLockout User{get;set;}
        /// <summary>
        /// Instrusion terminal
        /// </summary>
        public HostLockout Host {get;set;}
#endregion Members

#region Constructors
        /// <summary>
        /// Initialisation
        /// </summary>
        /// <param name="pCS"chaine de connexion></param>
        /// <param name="pHostName">Terminal appelant</param>
        /// <param name="pLoginName">LoginName appelant</param>
        public LockSafety(string pCS, string pHostName, string pLoginName)
        {
            User = new UserLockout(pCS, pHostName, pLoginName);
            Host = new HostLockout(pCS, pHostName, pLoginName);
        }
#endregion Constructors
#region Methods
        /// <summary>
        /// Mise à jour des tables d'intrusions 
        /// USERLOCK : Utilisateur, si IDA est connu donc LoginName valide dans la table ACTOR
        /// HOSTLOCK : Terminal
        /// </summary>
        /// EG 20151215 [21305] New
        public void Updating()
        {
            bool isOk = true;
            // FI 20200813 [XXXXX] use of GetDateSysUTC
            DateTime dtSys = OTCmlHelper.GetDateSysUTC(User.CS);
            // L'utilisateur est connu donc Mise à jour de USERLOCKOUT
            if (User.IdA.HasValue)
                isOk = User.UpdateLock(dtSys);
            // Mise à jour de HOSTLOCKOUT
            if (isOk)
                Host.UpdateLock(dtSys);
        }
        /// <summary>
        /// Remise à zéro des instrusions
        /// User : Utilisateur, si IDA est connu donc LoginName valide dans la table ACTOR
        /// Host : Terminal
        /// </summary>
        /// EG 20151215 [21305] New
        public void Resetting()
        {
            bool isOk = true;
            // L'utilisateur est connu donc Mise à jour de USERLOCK par Reset des compteurs
            if (User.IdA.HasValue)
                isOk = User.ResetLock();
            // Mise à jour de HOSTLOCKOUT par Reset des compteurs
            if (isOk)
                Host.ResetLock();
        }
#endregion Methods
    }
#endregion Lockout

#region UserLockout
    /// <summary>
    /// Gestion des intrusions (USER)
    /// Hérite de la classe abstraite Lockout
    /// </summary>
    // EG 20151215 [21305] New 
    public class UserLockout : Lockout
    {
#region Accessors
        /// <summary>
        /// Id utilisateur (ACTOR)
        /// </summary>
        public Nullable<int> IdA { set; get; }
        /// <summary>
        /// LoginName
        /// </summary>
        protected override string LockName { get { return LoginName; } }
        /// <summary>
        /// Ressource pour message d'erreur de verrouillage TEMPORAIRE pour utilisateur (USERLOCKOUT) 
        /// </summary>
        protected override string FailureConnect_Locktime { get { return "FailureConnect_UserLocktime"; } }
        /// <summary>
        /// Ressource pour message d'erreur de verrouillage DEFINITIF pour utilisateur (USERLOCKOUT) 
        /// </summary>
        protected override string FailureConnect_Lockout { get { return "FailureConnect_UserLockout"; } }
        /// <summary>
        /// Ressource pour message INFORMATIF d'annonce du prochain verrouillage TEMPORAIRE pour utilisateur (USERLOCKOUT) 
        /// </summary>
        protected override string WarningConnect_BeforeLockTime { get { return "WarningConnect_UserBeforeLocktime"; } }
        /// <summary>
        /// Ressource pour message INFORMATIF d'annonce du prochain verrouillage DEFINITIF pour utilisateur (USERLOCKOUT) 
        /// </summary>
        protected override string WarningConnect_BeforeLockOut { get { return "WarningConnect_UserBeforeLockout"; } }
#endregion Accessors
#region Constructors
        /// <summary>
        /// Initialisation de LOCKOUT utilisateur
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pHostName">Terminal</param>
        /// <param name="pLoginName">Login name</param>
        // EG 20151215 [21305] New 
        public UserLockout(string pCS, string pHostName, string pLoginName) : base(pCS, pHostName, pLoginName, "UserName")
        {
        }
#endregion Constructors
#region Methods
        /// <summary>
        /// Requête de selection dans la table USERLOCKOUT pour un LoginName donné
        /// - avec lecture sur la table ACTOR pour ISALLOWONLOCKOUT pour un HostName donné
        /// et
        /// - sans lecture sur la table ACTOR pour les LOGINNAME inconnu dans la base (IDA = -1)
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        // EG 20240523 [WI941][26663] Upd UserLock peut être posé même sur un acteur inconnu (New Column LOGINNAME alimenté et IDA = -1)
        public override QueryParameters GetQuerySelect()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), LoginName);
            
            string sqlSelect = @"select lo.IDUSERLOCKOUT_L as IDLOCKOUT, lo.DTFIRSTDENIAL, lo.DTLASTDENIAL, lo.TOTALDENIAL, 
            lo.TOTALLOCKOUT, lo.STATUS, lo.RESETTIME, lo.EXTLLINK, ac.IDA, ac.IDENTIFIER as LOGINNAME
            from dbo.ACTOR ac
            left outer join dbo.USERLOCKOUT_L lo on (lo.IDA = ac.IDA)
            where (ac.IDENTIFIER = @LOGINNAME)
            union 
            select lo.IDUSERLOCKOUT_L as IDLOCKOUT, lo.DTFIRSTDENIAL, lo.DTLASTDENIAL, lo.TOTALDENIAL, 
            lo.TOTALLOCKOUT, lo.STATUS, lo.RESETTIME, lo.EXTLLINK, lo.IDA as IDA, lo.LOGINNAME
            from dbo.USERLOCKOUT_L lo 
            where (lo.IDA = -1) and (lo.LOGINNAME = @LOGINNAME)" + Cst.CrLf;
            return new QueryParameters(CS, sqlSelect.ToString(), parameters);
        }
        /// <summary>
        /// Requête de remise à zéro des compteurs (TOTALDENIAL|RESETTIME) et 
        /// déverrouillage du statut (STATUS = NOLOCK) de la table USERLOCKOUT
        /// pour un IDUSERLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryResetDenial()
        {
            DataParameters parameters = CommonParameters2();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.Int32), IdLockout);
            parameters.Add(new DataParameter(CS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), HostName);
            string sqlUpdate = @"update dbo.USERLOCKOUT_L 
            set TOTALDENIAL = 0, RESETTIME = null, STATUS = 'NOLOCK', 
            HOSTNAME = @HOSTNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER
            where (IDUSERLOCKOUT_L = @IDLOCKOUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }
        /// <summary>
        /// Requête de remise à zéro totale des compteurs (TOTALDENIAL|TOTALLOCKOUT|RESETTIME),
        /// des dates de 1ère et dernière intrusion (DTFIRSTDENIAL|DTLASTDENIAL)
        /// déverrouillage du statut (STATUS = NOLOCK) de la table USERLOCKOUT
        /// pour un IDUSERLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryReset()
        {
            DataParameters parameters = CommonParameters2();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.Int32), IdLockout);
            parameters.Add(new DataParameter(CS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), HostName);
            string sqlUpdate = @"update dbo.USERLOCKOUT_L 
            set TOTALDENIAL = 0, TOTALLOCKOUT = 0, DTFIRSTDENIAL = null, DTLASTDENIAL = null, RESETTIME = null, STATUS = 'NOLOCK',
            HOSTNAME = @HOSTNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER
            where (IDUSERLOCKOUT_L = @IDLOCKOUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }
        /// <summary>
        /// Requête d'insertion d'une ligne d'intrusion dans la table USERLOCKOUT pour un utilisateur (IDA) donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        // EG 20240523 [WI941][26663] Add LOGINNAME
        public override QueryParameters GetQueryInsert()
        {
            DataParameters parameters = CommonParameters();
            parameters.Add(new DataParameter(CS, "IDA", DbType.AnsiString, SQLCst.UT_HOST_LEN), IdA.Value);
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), LoginName);
            string sqlInsert = @"insert into dbo.USERLOCKOUT_L
            (IDA, LOGINNAME, DTFIRSTDENIAL, DTLASTDENIAL, TOTALDENIAL, TOTALLOCKOUT, STATUS, RESETTIME, 
            HOSTNAME, APPNAME, APPVERSION, APPBROWSER, EXTLLINK, ROWATTRIBUT)
            values
            (@IDA, @LOGINNAME, @DTFIRSTDENIAL, @DTLASTDENIAL, @TOTALDENIAL, @TOTALLOCKOUT, @STATUS, @RESETTIME, 
            @HOSTNAME, @APPNAME, @APPVERSION, @APPBROWSER, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlInsert.ToString(), parameters);
        }
        /// <summary>
        /// Requête de mise à jour 
        /// des compteurs (TOTALDENIAL|TOTALLOCKOUT|RESETTIME),
        /// des dates de 1ère et dernière intrusion (DTFIRSTDENIAL|DTLASTDENIAL)
        /// du statut de la table USERLOCKOUT pour un IDUSERLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        // EG 20240523 [WI941][26663] Add LOGINNAME
        public override QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = CommonParameters();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.AnsiString, SQLCst.UT_HOST_LEN), IdLockout.Value);
            parameters.Add(new DataParameter(CS, "IDA", DbType.AnsiString, SQLCst.UT_HOST_LEN), IdA.Value);
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_IDENTIFIER_LEN), LoginName);
            string sqlUpdate = @"update dbo.USERLOCKOUT_L
            set IDA = @IDA, LOGINNAME = @LOGINNAME, DTFIRSTDENIAL = @DTFIRSTDENIAL, DTLASTDENIAL = @DTLASTDENIAL, TOTALDENIAL = @TOTALDENIAL, TOTALLOCKOUT = @TOTALLOCKOUT, 
            STATUS = @STATUS, RESETTIME = @RESETTIME, HOSTNAME = @HOSTNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER,
            EXTLLINK = @EXTLLINK, ROWATTRIBUT = @ROWATTRIBUT
            where IDUSERLOCKOUT_L = @IDLOCKOUT" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }

        /// <summary>
        /// Comparaison entre le Hostname passé en paramètre et le poste client (ClientMachineName)
        /// </summary>
        /// <param name="pValue1"></param>
        /// <returns></returns>
        /// EG 20151215 [21305] New
        private bool IsCurrentHostName(string pValue1)
        {
            IPHostEntry hostEntryIP1 = Dns.GetHostEntry(pValue1);
            IPHostEntry hostEntryIP2 = Dns.GetHostEntry(SessionTools.ClientMachineName);
            return (hostEntryIP1.HostName == hostEntryIP2.HostName);
        }
        /// <summary>
        /// Lecture dans la table (USERLOCKOUT)
        /// </summary>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        /// FI 20200810 [XXXXX] add paramater dbTransaction
        // EG 20240523 [WI941][26663] Case IDA > 0
        public override bool SelectLock(IDbTransaction dbTransaction)
        {
            bool isOk = base.SelectLock(dbTransaction);
            if (IdA.HasValue && (0 < IdA.Value))
                SetAllowOnLockout(dbTransaction);
            return isOk;
        }

        /// <summary>
        /// Valorisation de IsAllowOnLockout par Lecture dans la table (ACTORHOST)
        /// Comparaison entre les HOSTNAMEs lus dans la table et le HOSTNAME courant
        /// </summary>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        /// FI 20200810 [XXXXX] add paramater dbTransaction
        private void SetAllowOnLockout(IDbTransaction dbTransaction)
        {
            IsAllowOnLockout = false;

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "IDA", DbType.Int32), IdA.Value);
            string sqlSelect = @"select ah.IDA, ah.ISALLOWONLOCKOUT, ah.HOSTNAME
                from dbo.ACTOR ac
                inner join dbo.ACTORHOST ah on (ah.IDA = ac.IDA)
                where (ac.IDA = @IDA)" + Cst.CrLf;
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect.ToString(), parameters);

            using (IDataReader dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                while (dr.Read())
                {
                    if (false == (dr["HOSTNAME"] is DBNull))
                    {
                        if (IsCurrentHostName(dr["HOSTNAME"].ToString()))
                        {
                            IsAllowOnLockout = !(dr["ISALLOWONLOCKOUT"] is DBNull) && Convert.ToBoolean(dr["ISALLOWONLOCKOUT"]);
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Alimentation des données de la classe par lecture de la table USERLOCKOUT|ACTOR|ACTORHOST
        /// </summary>
        /// <param name="pDr"></param>
        // EG 20151215 [21305] New
        protected override void SetLockout(IDataReader pDr)
        {
            base.SetLockout(pDr);
            if (false == (pDr["IDA"] is DBNull))
                IdA = Convert.ToInt32(pDr["IDA"]);
        }
        // EG 20240523 [WI941][26663] New (case Ida = -1 (For unknown LOGINNAME in DB)
        protected override void SetDefaultLockout()
        {
            base.SetDefaultLockout();
            IdA = -1;
        }
#endregion Methods
    }
#endregion UserLockout
#region HostLockout
    /// <summary>
    /// Gestion des intrusions (HOST)
    /// Hérite de la classe abstraite Lockout
    /// </summary>
    // EG 20151215 [21305] New 
    public class HostLockout : Lockout
    {
#region Accessors
        /// <summary>
        /// Liste des LoginName à l'origine de le(s) instrusion(s)
        /// </summary>
        public List<string> LoginNameList {set;get;}
        /// <summary>
        /// Liste des LoginName pour écriture dans la table
        /// BUT = Eviter les doublons
        /// </summary>
        public string LoginNameToWrite
        {
            get
            {
                string result = string.Empty;
                if (null != LoginNameList)
                {
                    LoginNameList.ForEach(item => { result += item + ";"; });
                    if (false == LoginNameList.Exists(item => item == LoginName))
                        result += LoginName + ";";
                }
                else
                {
                    result = LoginName + ";";
                }
                return result;
            }
        }
        /// <summary>
        /// HostName
        /// </summary>
        protected override string LockName { get { return HostName; } }
        /// <summary>
        /// Ressource pour message d'erreur de verrouillage TEMPORAIRE pour utilisateur (HOSTLOCKOUT) 
        /// </summary>
        protected override string FailureConnect_Locktime { get { return "FailureConnect_HostLocktime"; } }
        /// <summary>
        /// Ressource pour message d'erreur de verrouillage DEFINITIF pour utilisateur (HOSTLOCKOUT) 
        /// </summary>
        protected override string FailureConnect_Lockout { get { return "FailureConnect_HostLockout"; } }
        /// <summary>
        /// Ressource pour message INFORMATIF d'annonce du prochain verrouillage TEMPORAIRE pour utilisateur (HOSTLOCKOUT) 
        /// </summary>
        protected override string WarningConnect_BeforeLockTime { get { return "WarningConnect_HostBeforeLocktime"; } }
        /// <summary>
        /// Ressource pour message INFORMATIF d'annonce du prochain verrouillage DEFINITIF pour utilisateur (HOSTLOCKOUT) 
        /// </summary>
        protected override string WarningConnect_BeforeLockOut { get { return "WarningConnect_HostBeforeLockout"; } }
#endregion Accessors
#region Constructors
        /// <summary>
        /// Initialisation de LOCKOUT terminal
        /// </summary>
        /// <param name="pCS">Chaine de connexion</param>
        /// <param name="pHostName">Terminal</param>
        /// <param name="pLoginName">Login name</param>
        // EG 20151215 [21305] New 
        public HostLockout(string pCS, string pHostName, string pLoginName) : base(pCS, pHostName, pLoginName, "HostName")
        {
        }
#endregion Constructors
#region Methods
        /// <summary>
        /// Requête de selection dans la table HOSTLOCKOUT pour un terminal donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQuerySelect()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), HostName);
            string sqlSelect = @"select lo.IDHOSTLOCKOUT_L as IDLOCKOUT, lo.HOSTNAME, lo.DTFIRSTDENIAL, lo.DTLASTDENIAL, lo.TOTALDENIAL, 
            lo.TOTALLOCKOUT, lo.STATUS, lo.RESETTIME, lo.LOGINNAME, lo.EXTLLINK
            from dbo.HOSTLOCKOUT_L lo
            where (lo.HOSTNAME = @HOSTNAME)" + Cst.CrLf;
            return new QueryParameters(CS, sqlSelect.ToString(), parameters);
        }
        /// <summary>
        /// Requête de remise à zéro des compteurs (TOTALDENIAL|RESETTIME) et 
        /// déverrouillage du statut (STATUS = NOLOCK) de la table HOSTLOCKOUT
        /// pour un IDHOSTLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryResetDenial()
        {
            DataParameters parameters = CommonParameters2();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.Int32), IdLockout);
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_LABEL_LEN), LoginName);
            string sqlUpdate = @"update dbo.HOSTLOCKOUT_L 
            set TOTALDENIAL = 0, RESETTIME = null, STATUS = 'NOLOCK', 
            LOGINNAME = @LOGINNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER
            where (IDHOSTLOCKOUT_L = @IDLOCKOUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }
        /// <summary>
        /// Requête de remise à zéro totale des compteurs (TOTALDENIAL|TOTALLOCKOUT|RESETTIME),
        /// des dates de 1ère et dernière intrusion (DTFIRSTDENIAL|DTLASTDENIAL)
        /// déverrouillage du statut (STATUS = NOLOCK) de la table HOSTLOCKOUT
        /// pour un IDHOSTLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryReset()
        {
            DataParameters parameters = CommonParameters2();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.Int32), IdLockout);
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_LABEL_LEN), LoginName);
            string sqlUpdate = @"update dbo.HOSTLOCKOUT_L 
            set TOTALDENIAL = 0, TOTALLOCKOUT = 0, DTFIRSTDENIAL = null, DTLASTDENIAL = null, RESETTIME = null, STATUS = 'NOLOCK',
            LOGINNAME = @LOGINNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER
            where (IDHOSTLOCKOUT_L = @IDLOCKOUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }
        /// <summary>
        /// Requête d'insertion d'une ligne d'intrusion dans la table HOSTLOCKOUT pour un terminal (HOSTNAME) donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryInsert()
        {
            DataParameters parameters = CommonParameters();
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_LABEL_LEN), LoginNameToWrite);
            string sqlInsert = @"insert into dbo.HOSTLOCKOUT_L
            (HOSTNAME, DTFIRSTDENIAL, DTLASTDENIAL, TOTALDENIAL, TOTALLOCKOUT, STATUS, RESETTIME, 
            LOGINNAME, APPNAME, APPVERSION, APPBROWSER, EXTLLINK, ROWATTRIBUT)
            values
            (@HOSTNAME, @DTFIRSTDENIAL, @DTLASTDENIAL, @TOTALDENIAL, @TOTALLOCKOUT, @STATUS, @RESETTIME, 
            @LOGINNAME, @APPNAME, @APPVERSION, @APPBROWSER, @EXTLLINK, @ROWATTRIBUT)" + Cst.CrLf;
            return new QueryParameters(CS, sqlInsert.ToString(), parameters);
        }
        /// <summary>
        /// Requête de mise à jour 
        /// des compteurs (TOTALDENIAL|TOTALLOCKOUT|RESETTIME),
        /// des dates de 1ère et dernière intrusion (DTFIRSTDENIAL|DTLASTDENIAL)
        /// du statut de la table HOSTLOCKOUT pour un IDHOSTLOCKOUT donné
        /// </summary>
        /// <returns></returns>
        // EG 20151215 [21305] New
        public override QueryParameters GetQueryUpdate()
        {
            DataParameters parameters = CommonParameters();
            parameters.Add(new DataParameter(CS, "IDLOCKOUT", DbType.AnsiString, SQLCst.UT_HOST_LEN), IdLockout.Value);
            parameters.Add(new DataParameter(CS, "LOGINNAME", DbType.AnsiString, SQLCst.UT_LABEL_LEN), LoginNameToWrite);
            string sqlUpdate = @"update dbo.HOSTLOCKOUT_L
            set HOSTNAME = @HOSTNAME, DTFIRSTDENIAL = @DTFIRSTDENIAL, DTLASTDENIAL = @DTLASTDENIAL, TOTALDENIAL = @TOTALDENIAL, TOTALLOCKOUT = @TOTALLOCKOUT, 
            STATUS = @STATUS, RESETTIME = @RESETTIME, LOGINNAME = @LOGINNAME, APPNAME = @APPNAME, APPVERSION = @APPVERSION, APPBROWSER = @APPBROWSER,
            EXTLLINK = @EXTLLINK, ROWATTRIBUT = @ROWATTRIBUT
            where IDHOSTLOCKOUT_L = @IDLOCKOUT" + Cst.CrLf;
            return new QueryParameters(CS, sqlUpdate.ToString(), parameters);
        }
        /// <summary>
        /// Alimentation des données de la classe par lecture de la table HOSTLOCKOUT
        /// </summary>
        /// <param name="pDr"></param>
        // EG 20151215 [21305] New
        protected override void SetLockout(IDataReader pDr)
        {
            base.SetLockout(pDr);
            if (false == (pDr["LOGINNAME"] is DBNull))
            {
                LoginNameList = new List<string>(Convert.ToString(pDr["LOGINNAME"]).Split(new char[] { ';' },
                    StringSplitOptions.RemoveEmptyEntries));
            }
        }
#endregion Methods
    }
#endregion HostLockout
#region Lockout
    /// <summary>
    /// Classe abstraite utilisée par UserLockout|HostLockout
    /// contient les données, méthodes communes à la gestion des instrusions
    /// </summary>
    // EG 20151215 [21305] New
    public abstract class Lockout
    {
#region Members
        /// <summary>
        /// Code retour de contrôle des intrusions
        /// FAILURE = Code en erreur (via catch)
        /// LOCKTIME = Verrouillage temporaire
        /// LOCKOUT = Verrouillage définitif (intervention administrateur)
        /// SUCCESS = Pas de verrouillage
        /// </summary>
        public LockoutReturn lockoutReturn = LockoutReturn.SUCCESS;
        /// <summary>
        /// Liste des messages générés lors des tentatives
        /// </summary>
        public List<Pair<string,string[]>> lockMessage = null;
        /// <summary>
        /// Paramètres de gestion des LOCKOUT lus dans web.cong|webCustom.config
        /// </summary>
        protected HackAttemptSafety hackAttemptSafety; 
#endregion Members
#region Accessors
        public string CS { get; set; }
        
        /// <summary>
        /// Login name
        /// </summary>
        protected string LoginName { set; get; }
        /// <summary>
        /// Terminal
        /// </summary>
        protected string HostName { set; get; }
        /// <summary>
        /// Identifiant du LOCKOUT (IDUSERLOCKOUT|IDHOSTLOCKOUT)
        /// </summary>
        protected Nullable<int> IdLockout { set; get; }
        /// <summary>
        /// Date de 1ère intrusion
        /// </summary>
        protected Nullable<DateTime> DtFirstDenial{set;get;}
        /// <summary>
        /// Date de dernière intrusion
        /// </summary>
        protected Nullable<DateTime> DtLastDenial { set; get; }
        /// <summary>
        /// Nombre total d'intrusions 
        /// </summary>
        protected int TotalDenial{set;get;}
        /// <summary>
        /// Nombre total de verrouillage (LOCKTIME)
        /// </summary>
        protected int TotalLockout { set; get; }
        /// <summary>
        /// Durée d'attente avant la réinitialisation du compteur de tentatives infructueuses (en minutes). 
        /// </summary>
        protected Nullable<int> ResetTime{set;get;}
        /// <summary>
        /// Statut pour intrusion
        /// NOLOCK = Non verrouillé
        /// LOCKTIME = Verrouillage temporaire
        /// LOCKOUT = Verrouillage définitif (intervention administrateur)
        /// </summary>
        protected LockoutStatus Status { set; get; }
        /// <summary>
        /// Lien externe
        /// </summary>
        protected string ExtlLink { set; get; }
        /// <summary>
        /// Détermine si la ligne en cours est verrouillée définitivement
        /// </summary>
        protected bool IsLockout { get { return Status == LockoutStatus.LOCKOUT; } }
        /// <summary>
        /// Détermine l'autorisation ou non du HostName associé (à l'utilisateur)
        /// en cas d'usage d'un compte verrouillé sur un autre HOSTNAME (LOCKOUT sur USERLOCKOUT) pour
        /// cause de tentatives infructueuses dépassées
        /// </summary>
        protected virtual bool IsAllowOnLockout { set; get; }
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        protected virtual string LockName { get { return null; } }
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        protected virtual string FailureConnect_Locktime { get { return null; } }
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        protected virtual string FailureConnect_Lockout { get { return null; } }
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        protected virtual string WarningConnect_BeforeLockTime { get { return null; } }
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        protected virtual string WarningConnect_BeforeLockOut { get { return null; } }
        /// <summary>
        /// Calcul le prochain RESETTIME en fonction du nombre de verrouillage déjà opéré et du FactorResetTime
        /// </summary>
        protected Nullable<int> LockTime 
        {
            get 
            { 
                Nullable<int> _lockTime = null;
                if (0 < hackAttemptSafety.MaxDenials)
                    _lockTime = ((0 < TotalLockout)? hackAttemptSafety.FactorResetTime:1) * hackAttemptSafety.ResetTime * Math.Max(1,TotalLockout);
                return _lockTime;
            }
        }

#endregion Accessors
#region Constructors
        /// <summary>
        /// Initialisation de base
        /// LoginName
        /// HostName
        /// Paramètres de gestion des intrusions (web.cong|webCustom.config)
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pHostName"></param>
        /// <param name="pLoginName"></param>
        /// <param name="pHackAttemptSafetyType"></param>
        public Lockout(string pCS, string pHostName, string pLoginName, string pHackAttemptSafetyType)
        {
            CS = pCS;
            HostName = pHostName;
            LoginName = pLoginName;
            hackAttemptSafety = new HackAttemptSafety(pHackAttemptSafetyType);
        }
#endregion Constructors
#region Methods
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        public virtual QueryParameters GetQuerySelect(){return null;}
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        public virtual QueryParameters GetQueryResetDenial(){return null;}
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        public virtual QueryParameters GetQueryReset(){return null;}
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        public virtual QueryParameters GetQueryInsert(){return null;}
        /// <summary>
        /// voir override UserLockout|HostLockout
        /// </summary>
        public virtual QueryParameters GetQueryUpdate(){return null;}
        /// <summary>
        /// Contrôle la présence d'un LOCK (LOCKTIME|LOCKOUT)
        /// Lecture de la ligne dans la table (USERLOCKOUT-HOSTLOCKOUT)
        /// Si elle existe, contrôle du STATUT
        /// - NOLOCK : RAS
        /// - LOCKTIME : Gestion du RESETTIME 
        ///              Message informatif si encore actif (retourne LOCKTIME)
        ///              sinon remise à zéro des compteurs (retourne SUCCESS)
        /// - LOCKOUT  : Si Lockout utilisateur et IsAllowOnLockout et terminal non locké définitivement Remise à zéro des compteurs (retourne SUCCESS) 
        ///              sinon (retourne LOCKOUT)
        /// </summary>
        public bool ControlLock()
        {
            return ControlLock(LockoutReturn.LOCKOUT);
        }
        public bool ControlLock(LockoutReturn pHostLockoutReturn)
        {
            bool isOk = true;
            // FI 2020810  [XXXXX] Usage de using
            using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
            {
                try
                {
                    if (SelectLock(dbTransaction))
                    {
                        switch (Status)
                        {
                            case LockoutStatus.LOCKTIME:
                                // FI 20200813 [XXXXX] use of GetDateSysUTC
                                DateTime dtSys = OTCmlHelper.GetDateSysUTC(CS);
                                if (DtLastDenial.HasValue && ResetTime.HasValue)
                                {
                                    TimeSpan timeSpan = DtLastDenial.Value.AddMinutes(ResetTime.Value) - dtSys;
                                    double remainingLocked = timeSpan.TotalMinutes;
                                    if (0 < remainingLocked)
                                    {
                                        lockoutReturn = LockoutReturn.LOCKTIME;
                                        AddMessage(FailureConnect_Locktime, LockName, GetRemainedTime(timeSpan, "hms"));
                                    }
                                    else
                                    {
                                        // RAZ du compteur TOTALDENIAL
                                        QueryParameters qryParameters = GetQueryResetDenial();
                                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                        lockoutReturn = LockoutReturn.SUCCESS;
                                    }
                                }
                                break;
                            case LockoutStatus.LOCKOUT:
                                if ((this is UserLockout) && IsAllowOnLockout && (LockoutReturn.LOCKOUT != pHostLockoutReturn))
                                {
                                    QueryParameters qryParameters = GetQueryResetDenial();
                                    DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                                    lockoutReturn = LockoutReturn.SUCCESS;
                                }
                                else
                                {
                                    lockoutReturn = LockoutReturn.LOCKOUT;
                                    AddMessage(FailureConnect_Lockout, LockName);
                                }
                                break;
                        }
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
                catch(Exception)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
            return isOk && (lockoutReturn == LockoutReturn.SUCCESS);
        }

        /// <summary>
        /// Formatage du temps
        /// </summary>
        /// <param name="pFormat">format de retour</param>
        private string GetRemainedTime(TimeSpan pTimespan, string pFormat)
        {
            string remainedTime = string.Empty; ;
            switch (pFormat)
            {
                case "hms":
                    if (pTimespan.Days > 0)
                        remainedTime = String.Format("{0}h {1}mn {2}s", StrFunc.IntegerPadding(Convert.ToInt32(pTimespan.TotalHours), 2), 
                            StrFunc.IntegerPadding(pTimespan.Minutes, 2), StrFunc.IntegerPadding(pTimespan.Seconds, 2));
                    else if (pTimespan.Hours > 0)
                        remainedTime = String.Format("{0}h {1}mn", pTimespan.Hours.ToString(), StrFunc.IntegerPadding(pTimespan.Minutes, 2));
                    else
                        remainedTime = String.Format("{0}mn {1}s", pTimespan.Minutes.ToString(), StrFunc.IntegerPadding(pTimespan.Seconds, 2));
                    break;
                case "m":
                    remainedTime = String.Format("{0}m", pTimespan.TotalMinutes);
                    break;
            }

            return remainedTime;
        }
        /// <summary>
        /// Ajoute un message à la liste lockMessage
        /// </summary>
        /// <param name="pResource">Ressource</param>
        /// <param name="pArgs">Arguments complémentaires (Utilisateur, HostName, Minutage, Nombre de tentatives etc.</param>
        public void AddMessage(string pResource, params string[] pArgs)
        {
            if (null == lockMessage)
                lockMessage = new List<Pair<string, string[]>>();
            Pair<string, string[]> msg = new Pair<string, string[]>(pResource, pArgs);
            lockMessage.Add(msg);
        }
        /// <summary>
        /// Contruction des messages retournés sur la base de la liste lockMessage
        /// - Messages dédié au Web : opErrMsg
        /// - Messages dédié au log : opErrMsgLog
        /// </summary>
        /// <param name="opErrMsg"></param>
        /// <param name="opErrMsgLog"></param>
        public void GetMessage(ref string opErrMsg, ref string opErrMsgLog)
        {
            if (null != lockMessage)
            {
                string msg = string.Empty;
                string msgLog = string.Empty;
                lockMessage.ForEach(item =>
                {
                    if (ArrFunc.IsFilled(item.Second))
                    {
                        msg += Ressource.GetString2(item.First, item.Second) + Cst.HTMLBreakLine;
                        msgLog += Ressource.GetString2(item.First, SessionTools.LogMessageCulture, item.Second);
                    }
                    else
                    {
                        msg += Ressource.GetString(item.First) + Cst.HTMLBreakLine;
                        msgLog += Ressource.GetString(item.First, SessionTools.LogMessageCulture);
                    }
                });
                opErrMsg += msg;
                opErrMsgLog += msgLog;
            }
        }
        /// <summary>
        /// Paramètres communs à USERLOCKOUT|HOSTLOCKOUT
        /// </summary>
        public DataParameters CommonParameters()
        {
            DataParameters parameters = CommonParameters2();
            parameters.Add(new DataParameter(CS, "HOSTNAME", DbType.AnsiString, SQLCst.UT_HOST_LEN), HostName);
            parameters.Add(new DataParameter(CS, "DTFIRSTDENIAL", DbType.DateTime), DtFirstDenial);
            parameters.Add(new DataParameter(CS, "DTLASTDENIAL", DbType.DateTime), DtLastDenial);
            parameters.Add(new DataParameter(CS, "TOTALDENIAL", DbType.Int32), TotalDenial);
            parameters.Add(new DataParameter(CS, "TOTALLOCKOUT", DbType.Int32), TotalLockout);
            parameters.Add(new DataParameter(CS, "RESETTIME", DbType.Int32), ResetTime);
            parameters.Add(new DataParameter(CS, "STATUS", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Status);
            parameters.Add(new DataParameter(CS, "EXTLLINK", DbType.AnsiString, SQLCst.UT_EXTLINK_LEN), ExtlLink);
            parameters.Add(new DataParameter(CS, "ROWATTRIBUT", DbType.AnsiString, SQLCst.UT_ROWATTRIBUT_LEN), Cst.RowAttribut_System);
            return parameters;
        }
        /// <summary>
        /// Paramètres communs à USERLOCKOUT|HOSTLOCKOUT
        /// </summary>
        public DataParameters CommonParameters2()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "APPNAME", DbType.AnsiString, SQLCst.UT_APPNAME_LEN), Software.Name);
            parameters.Add(new DataParameter(CS, "APPVERSION", DbType.AnsiString, SQLCst.UT_APPVERSION_LEN), Software.Version);
            parameters.Add(new DataParameter(CS, "APPBROWSER", DbType.AnsiString, 64), SessionTools.BrowserInfo);
            return parameters;
        }
        /// <summary>
        /// Lecture dans la table (USERLOCKOUT|HOSTLOCKOUT)
        /// </summary>
        /// <param name="dbTransaction"></param>
        /// <returns></returns>
        /// FI 2020810 [XXXXX] parameter dbTransaction
        // EG 20240523 [WI941][26663] Init Data where LOGINNAME not found (IDA = -1)
        public virtual bool SelectLock(IDbTransaction dbTransaction)
        {
            bool isOk = true;
            QueryParameters qryParameters = GetQuerySelect();
            // FI 2020810  [XXXXX] Usage de using
            using (IDataReader dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (dr.Read())
                    SetLockout(dr);
                else
                    SetDefaultLockout();
            }
            return isOk;
        }
        /// <summary>
        /// Mise à jour (INSERT|UPDATE) de la table  (USERLOCKOUT|HOSTLOCKOUT)
        /// - avec mise à jour des compteurs
        /// </summary>
        /// <param name="pDtSys"></param>
        /// <returns></returns>
        public bool UpdateLock(DateTime pDtSys)
        {
            bool isOk = true;
            // FI 2020810  [XXXXX] Usage de using
            using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
            {
                try
                {
                    if (SelectLock(dbTransaction))
                    {
                        SetCountersAfterHackAttempt(pDtSys);
                        QueryParameters qryParameters = IdLockout.HasValue ? GetQueryUpdate() : GetQueryInsert();
                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
                catch(Exception)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
            return isOk;
        }
        /// <summary>
        /// Remise à zéro des compteurs de la table  (USERLOCKOUT|HOSTLOCKOUT) pour un ID donné (IDUSERLOCKOUT|IDHOSTLOCKOUT)
        /// </summary>
        /// <returns></returns>
        public bool ResetLock()
        {
            bool isOk = true;
            // FI 2020810  [XXXXX] Usage de using
            using (IDbTransaction dbTransaction = DataHelper.BeginTran(CS))
            {
                try
                {
                    if (SelectLock(dbTransaction) && IdLockout.HasValue)
                    {
                        QueryParameters qryParameters = GetQueryReset();
                        DataHelper.ExecuteNonQuery(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    }
                    DataHelper.CommitTran(dbTransaction);
                }
                catch (Exception)
                {
                    DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }
            return isOk;
        }
        /// <summary>
        /// Mise à jour des compteurs
        /// </summary>
        /// <param name="pDtSys"></param>
        public void SetCountersAfterHackAttempt(DateTime pDtSys)
        {
            TotalDenial++;
            if (false == DtFirstDenial.HasValue)
                DtFirstDenial = pDtSys;
            DtLastDenial = pDtSys;

            Nullable<int> _lockTime = LockTime;
            string remained = "-";
            if (_lockTime.HasValue)
            {
                TimeSpan timeSpan = new TimeSpan(0, _lockTime.Value, 0);
                remained = GetRemainedTime(timeSpan, "hms");
            }

            // Controle nombre maximal de tentatives infructueuses
            if (TotalDenial == hackAttemptSafety.MaxDenials)
            {
                ResetTime = _lockTime;
                Status = (0 == hackAttemptSafety.ResetTime) || (TotalLockout == hackAttemptSafety.MaxResetTime) ? LockoutStatus.LOCKOUT : LockoutStatus.LOCKTIME;
                if (Status == LockoutStatus.LOCKOUT)
                    AddMessage(FailureConnect_Lockout, LockName);
                else
                    AddMessage(FailureConnect_Locktime, LockName, remained);
                TotalLockout++;
            }
            else
            {
                // Calcul du nombre tentatives restantes
                int remainDenials = hackAttemptSafety.MaxDenials - TotalDenial;
                bool isWarning = (-1 == hackAttemptSafety.MaxDenialsWarning) || (TotalDenial == hackAttemptSafety.MaxDenials - hackAttemptSafety.MaxDenialsWarning);
                if (isWarning)
                {
                    // Warning informatif sur tentative restante avant lockout et intervention obligatoire d'un administrateur 
                    // Affichier ResetTime dans le message
                    if ((0 == hackAttemptSafety.ResetTime) || (TotalLockout == hackAttemptSafety.MaxResetTime))
                        AddMessage(WarningConnect_BeforeLockOut, remainDenials.ToString(), LockName);
                    else
                    {
                        AddMessage(WarningConnect_BeforeLockTime, remainDenials.ToString(), LockName, remained);
                    }
                }
            }
        }
        /// <summary>
        /// Alimentation des données via lecture de la table (USERLOCKOUT|HOSTLOCKOUT)
        /// </summary>
        /// <param name="pDr"></param>
        protected virtual void SetLockout(IDataReader pDr)
        {
            if (false == (pDr["IDLOCKOUT"] is DBNull))
                IdLockout = Convert.ToInt32(pDr["IDLOCKOUT"]);
            if (false == (pDr["DTFIRSTDENIAL"] is DBNull))
            {
                // FI 20200813 [XXXXX] DTFIRSTDENIAL DateTimeKind is UTC 
                DtFirstDenial = Convert.ToDateTime(pDr["DTFIRSTDENIAL"]);
                DtFirstDenial = DateTime.SpecifyKind(DtFirstDenial.Value, DateTimeKind.Utc);
            }
            if (false == (pDr["DTLASTDENIAL"] is DBNull))
            {
                // FI 20200813 [XXXXX] DTFIRSTDENIAL DateTimeKind is UTC 
                DtLastDenial = Convert.ToDateTime(pDr["DTLASTDENIAL"]);
                DtLastDenial = DateTime.SpecifyKind(DtLastDenial.Value, DateTimeKind.Utc);
            }
            TotalDenial = (pDr["TOTALDENIAL"] is DBNull)?0:Convert.ToInt32(pDr["TOTALDENIAL"]);
            TotalLockout = (pDr["TOTALLOCKOUT"] is DBNull) ? 0 : Convert.ToInt32(pDr["TOTALLOCKOUT"]);
            Status = LockoutStatus.NOLOCK;
            if (false == (pDr["STATUS"] is DBNull))
                Status = (LockoutStatus) ReflectionTools.EnumParse(new LockoutStatus(), pDr["STATUS"].ToString());
            if (false == (pDr["RESETTIME"] is DBNull))
                ResetTime = Convert.ToInt32(pDr["RESETTIME"]);
            if (false == (pDr["EXTLLINK"] is DBNull))
                ExtlLink = pDr["EXTLLINK"].ToString();
        }
        // EG 20240523 [WI941][26663] Add (virtual)
        protected virtual void SetDefaultLockout()
        {
            TotalDenial = 0;
            TotalLockout = 0;
            Status = LockoutStatus.NOLOCK;
        }
#endregion Methods
    }
#endregion Lockout


    /// <summary>
    /// Représente un utilisateur connecté
    /// </summary>
    public class ConnectedUser
    {
        /// <summary>
        /// ID système de l'utilisateur
        /// </summary>
        public int ida;
        /// <summary>
        /// ID système du modèle de sécurité (PL 20180919 newness)
        /// </summary>
        public int idModelSafety;
        /// <summary>
        /// Nombre de connexion active
        /// </summary>
        public int nbConnection;

#region constructor
        /// <summary>
        /// Constructor (nbConnection est initialisé à 1) 
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdModelSafety"></param>
        public ConnectedUser(int pIda, int pIdModelSafety)
        {
            ida = pIda;
            idModelSafety = pIdModelSafety;
            nbConnection = 1;
        }
#endregion constructor
    }

    public class ConnectedUsers : CollectionBase
    {
#region Constructor
        public ConnectedUser this[int pIndex]
        {
            get { return (ConnectedUser)this.List[pIndex]; }
        }
#endregion constructor
#region public GetNbConnectionAllUsers
        /// <summary>
        /// Retrourne le nbr de connexion tous acteurs confondus
        /// </summary>
        /// <returns></returns>
        public int GetNbConnectionAllUsers()
        {
            int count = 0;
            foreach (ConnectedUser cu in this)
            {
                count += cu.nbConnection;
            }
            return count;
        }
#endregion GetNbConnectionAllUsers
#region public GetNbConnection
        /// <summary>
        /// Retourne le nbr de connexion d'un acteur
        /// </summary>
        /// <param name="pIda"></param>
        /// <returns></returns>
        public int GetNbConnection(int pIda)
        {
            int count = 0;
            foreach (ConnectedUser cu in this)
            {
                if (cu.ida == pIda)
                {
                    count = cu.nbConnection;
                    break;
                }
            }
            return count;
        }
#endregion public GetNbConnection
#region  public Add
        public void Add(int pIda, int pIdModelSafety)
        {
            this.List.Add(new ConnectedUser(pIda, pIdModelSafety));
        }
        public void Add(ConnectedUser pConnectedUser)
        {
            this.List.Add(pConnectedUser);
        }
#endregion  public Add
#region public Insert
        public void Insert(int pIndex, ConnectedUser pConnectedUser)
        {
            this.List.Insert(pIndex, pConnectedUser);
        }
        public void Insert(int pIndex, int pIda, int pIdModelSafety)
        {
            this.List.Insert(pIndex, new ConnectedUser(pIda, pIdModelSafety));
        }
#endregion Insert
#region public Modify
        /// <summary>
        /// Attribue un nbr de connexion à un acteur de la liste
        /// <para>si le nbr de connexion est inférieur à Zéro, alors supprime l'utilisateur de la liste</para>
        /// </summary>
        /// <param name="pIda"></param>
        /// <param name="pIdModelSafety"></param>
        /// <param name="pCount"></param>
        public void Modify(int pIda, int pIdModelSafety, int pCount)
        {
            if (pCount <= 0)
            {
                this.Remove(pIda);
            }
            else
            {
                bool isFound = false;
                foreach (ConnectedUser cu in this)
                {
                    if (cu.ida == pIda)
                    {
                        isFound = true;
                        cu.nbConnection = pCount;
                        break;
                    }
                }
                if (!isFound)
                {
                    this.Add(pIda, 0);
                    if (pCount > 1)
                        this.Modify(pIda, pIdModelSafety, pCount);
                }
            }
        }
#endregion Modify
        /// <summary>
        /// Supprime de la liste l'acteur {pIda}
        /// </summary>
        /// <param name="pIda"></param>
        public void Remove(int pIda)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].ida == pIda)
                {
                    this.List.RemoveAt(i);
                    break;
                }
            }
        }

    }
}
