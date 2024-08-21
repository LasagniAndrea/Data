using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.SessionState;
//
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Log;
using EFS.LoggerClient;
using EFS.LoggerClient.LoggerService;
using EFS.Restriction;
//
//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{

    /// <summary>
    /// Classe qui gère les variables session serveur
    /// <para>Cette classe s'appuie sur HttpContext.Current.</para>
    /// <para>Elle doit être utilisée uniquement lors d'une réponse à une requête Http</para>
    /// </summary>
	// EG 20200924 [XXXXX] Suppression du paramètre d'ouverture de page IsOpenInUniquePage
    public sealed partial class SessionTools
    {
        #region Members
        #endregion Members

        #region Constructor
        public SessionTools() { }
        #endregion Constructor

        #region Accessors

        /// <summary>
        /// Chargement de la page default et gestion d'un éventuel message informatif lors du contrôle de la connexion
        /// Stockage d'un message après connexion lié à problème sur la base (ex : DBCORRUPTED)
        /// Ce message sera lu via la méthode dans CommonWebService depuis default.js
        /// EG 20221121 Affichage message géré via Default.js (Méthode _DisplayMessageAfterOnConnectOk)
        /// EG 20220331 [XXXXX] La variable devient une variable de Session
        public static Tuple<string, ProcessStateTools.StatusEnum, string> MessageAfterOnConnectOk
        {
            get { return (Tuple<string, ProcessStateTools.StatusEnum, string>)HttpSessionStateTools.Get(SessionState, "MessageAfterOnConnectOk"); }
            set { HttpSessionStateTools.Set(SessionState, "MessageAfterOnConnectOk", value); }
        }
        /// <summary>
        /// Obtient l'objet session (accessible depuis HttpContext.Current) 
        /// <para>Obtient null si le thread courant n'est pas celui qui traite la requêe HTTP (cas code Asynchrone)</para>
        /// </summary>
        public static HttpSessionState SessionState
        {
            get
            {
                if (null != HttpContext.Current)
                    return HttpContext.Current.Session;
                return null;
            }
        }

        /// <summary>
        /// Obtient true l'objet HttpContext.Current.Session est disponible
        /// <para>Obtient false, par exemple, si le thread courant n'est pas celui qui traite la requête HTTP (cas code Asynchrone)</para>
        /// <para><seealso cref="SessionState"/></para>
        /// </summary>
        private static bool IsSessionAvailable => HttpSessionStateTools.IsSessionAvailable(SessionState);
        

        /// <summary>
        /// Obtient SessionState.SessionID
        /// <para>Obtient 'Session not Available' si la session est indisponible</para>
        /// </summary>
        public static string SessionID
        {
            get
            {
                string ret = "Session not Available";
                if (IsSessionAvailable)
                    ret = SessionState.SessionID;
                return ret;
            }
        }

        /// <summary>
        /// Obtient les 10 premiers caractères de SessionID si la session est disponible
        /// <para>Obtient 'Session not Available' si la session est indisponible</para>
        /// </summary>
        public static string ShortSessionID
        {
            get
            {
                string ret = "Session not Available";
                if (IsSessionAvailable)
                    ret = HttpSessionStateTools.ShortSessionId(SessionState);
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit la variable session SessionClipBoard
        /// </summary>
        public static SessionClipBoard SessionClipBoard
        {
            get { return (SessionClipBoard)HttpSessionStateTools.Get(SessionState, "SessionClipBoard"); }
            set { HttpSessionStateTools.Set(SessionState, "SessionClipBoard", value); }
        }
        /// <summary>
        /// L'authentification est de type Shibboleth et la session est active
        /// </summary>
        /// EG 20220623 [XXXXX] New
        public static bool IsShibbolethAuthenticationType=> HttpSessionStateTools.IsShibbolethAuthenticationType(SessionState);

        /// <summary>
        /// Obtient ou définit la variable session WELCOMEMSG
        /// </summary>
        public static string WelcomeMsg
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "WELCOMEMSG"); }
            set { HttpSessionStateTools.Set(SessionState, "WELCOMEMSG", value); }
        }

        /// <summary>
        ///  obtient 'DNS host name or IP address of the server' - 'IP address of client'
        /// </summary>
        public static string ServerAndUserHost
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "HOSTNAME"); }
        }

        /// <summary>
        /// Définit la combinaison des host server et client dans une variable session 
        /// </summary>
        public static void SetServerAndUserHost()
        {
            if (IsSessionAvailable)
            {
                SessionState["HOSTNAME"] = AspTools.GetServerAndUserHost();
            }
        }


        /// <summary>
        ///  Obtient l'adresse IP du client
        /// </summary>
        public static string UserHostAddress
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "USERHOSTADDRESS"); }
        }

        /// <summary>
        /// Définit l'adresse IP du client dans une variable session
        /// </summary>
        public static void SetUserHostAddress()
        {
            if (IsSessionAvailable)
            {
                SessionState["USERHOSTADDRESS"] = AspTools.GetUserHostIP();
            }
        }

        
        /// <summary>
        /// Retourne le nom DNS de la machine ou à défaut le UserHostAddress de la dite machine
        /// </summary>
        public static string ClientMachineName
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "CLIENTMACHINENAME"); }
        }


        /// <summary>
        /// Définit la machine du client dans une variable session
        /// </summary>
        public static void SetClientMachineName()
        {
            if (IsSessionAvailable)
            {
                SessionState["CLIENTMACHINENAME"] = AspTools.GetClientMachineName();
            }
        }

        /// <summary>
        /// Obtient les informations concernant le Browser
        /// </summary>
        public static string BrowserInfo
        {
            get => AppSession.BrowserInfo;
        }

        

        /// <summary>
        ///  Obtient true si Spheres® alimente l'audit des actions utilsateurs
        /// </summary>
        ///FI 20140610 [19923] add IsRequestTrackEnabled (GLOP A valoriser)
        public static Boolean IsRequestTrackEnabled
        {
            get
            {
                return Collaborator.TrackMode != Cst.RequestTrackMode.NONE;
            }
        }

        /// <summary>
        ///  Obtient true si Spheres® alimente l'audit des actions utilsateurs pour les consultations (valable pour XML ou LST)
        /// </summary>
        ///FI 20141021 [20350] add 
        public static Boolean IsRequestTrackConsultEnabled
        {
            get
            {
                return (Collaborator.TrackMode == Cst.RequestTrackMode.CONSULT) ||
                    (Collaborator.TrackMode == Cst.RequestTrackMode.ALL);
            }
        }

        /// <summary>
        ///  Obtient true si Spheres® alimente l'audit des actions utilsateurs pour les process 
        /// </summary>
        ///FI 20141021 [20350] add 
        public static Boolean IsRequestTrackProcessEnabled
        {
            get
            {
                return (Collaborator.TrackMode == Cst.RequestTrackMode.PROCESS) ||
                    (Collaborator.TrackMode == Cst.RequestTrackMode.ALL);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsBackgroundWhite
        {
            get { return GetGenericBooleanCookieElement(Cst.SQLCookieElement.BackgroundWhite); }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieElement.BackgroundWhite, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsLookv1
        {
            get { return GetGenericBooleanCookieElement(Cst.SQLCookieElement.Lookv1); }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieElement.Lookv1, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsPagerPositionTopAndBottom
        {
            get { return GetGenericBooleanCookieElement(Cst.SQLCookieElement.PagerPosition); }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieElement.PagerPosition, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int NumberRowByPage
        {
            get
            {
                //20090128 PL Enhance (Add HttpSessionStateTools.Set())
                int tmpvalue;
                try
                {
                    string s;
                    //20090120 PL La ligne suivante plante ???
                    //tmpvalue = IntFunc.IntValue((string)HttpSessionStateTools.Get(SessionState, Cst.SQLCookieElement.NumberRowByPage.ToString()));
                    object o = HttpSessionStateTools.Get(SessionState, Cst.SQLCookieElement.NumberRowByPage.ToString());
                    //20090126 PL Add test (o == null), car sinon on ne lit plus jamais depuis la table SQL
                    if (o == null)
                    {
                        s = null;
                        AspTools.ReadSQLCookie(Cst.SQLCookieElement.NumberRowByPage.ToString(), out s);
                        tmpvalue = Convert.ToInt32(s);
                        //
                        HttpSessionStateTools.Set(SessionState, Cst.SQLCookieElement.NumberRowByPage.ToString(), tmpvalue);
                    }
                    else
                    {
                        s = o.ToString();
                        tmpvalue = IntFunc.IntValue(s);
                    }
                }
                catch
                {
                    tmpvalue = 20;
                }
                if (tmpvalue < 0)
                    tmpvalue = 20;
                return tmpvalue;
            }
            set
            {
                int tmpvalue;
                try
                {
                    tmpvalue = Math.Abs(value);
                    AspTools.WriteSQLCookie(Cst.SQLCookieElement.NumberRowByPage.ToString(), tmpvalue.ToString());
                }
                catch
                {
                    tmpvalue = 20;
                }
                //
                HttpSessionStateTools.Set(SessionState, Cst.SQLCookieElement.NumberRowByPage.ToString(), tmpvalue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string DefaultPage
        {
            get
            {
                //20090128 PL Enhance (Add HttpSessionStateTools.Set())
                string tmpvalue;
                try
                {
                    tmpvalue = (string)HttpSessionStateTools.Get(SessionState, Cst.SQLCookieElement.DefaultPage.ToString());
                    if (StrFunc.IsEmpty(tmpvalue))
                    {
                        AspTools.ReadSQLCookie(Cst.SQLCookieElement.DefaultPage.ToString(), out string s);
                        tmpvalue = s;
                        HttpSessionStateTools.Set(SessionState, Cst.SQLCookieElement.DefaultPage.ToString(), tmpvalue);
                    }
                }
                catch
                {
                    tmpvalue = string.Empty;
                }
                return tmpvalue;
            }
            set
            {
                string tmpvalue;
                try
                {
                    tmpvalue = value;
                    AspTools.WriteSQLCookie(Cst.SQLCookieElement.DefaultPage.ToString(), tmpvalue);
                }
                catch
                {
                    tmpvalue = string.Empty;
                }
                //
                HttpSessionStateTools.Set(SessionState, Cst.SQLCookieElement.DefaultPage.ToString(), tmpvalue);
            }
        }

        /// <summary>
        /// Obtient ou définit le format d'affichage des échéances ETD
        /// </summary>
        /// FI 20171025 [23533] Reafctoring
        public static Cst.ETDMaturityInputFormatEnum ETDMaturityFormat
        {
            get
            {
                return GetCookieState<Cst.ETDMaturityInputFormatEnum>(Cst.SQLCookieElement.ETDMaturityFormat.ToString());
            }
            set
            {
                SetCookieState<Cst.ETDMaturityInputFormatEnum>(Cst.SQLCookieElement.ETDMaturityFormat.ToString(), value);
            }
        }

        /// <summary>
        /// Obtient ou définit le fuseau horaire d'affichage des horodatages de trading
        /// </summary>
        /// FI 20171025 [23533] Add
        public static Cst.TradingTimestampZone TradingTimestampZone
        {
            get
            {
                return GetCookieState<Cst.TradingTimestampZone>(Cst.SQLCookieElement.TradingTimestampZone.ToString());
            }
            set
            {
                SetCookieState<Cst.TradingTimestampZone>(Cst.SQLCookieElement.TradingTimestampZone.ToString(), value);
            }
        }

        /// <summary>
        /// Obtient ou définit la précison d'affichage des horodatages de trading
        /// </summary>
        /// FI 20171025 [23533] Add
        public static Cst.TradingTimestampPrecision TradingTimestampPrecision
        {
            get
            {
                return GetCookieState<Cst.TradingTimestampPrecision>(Cst.SQLCookieElement.TradingTimestampPrecision.ToString());
            }
            set
            {
                SetCookieState<Cst.TradingTimestampPrecision>(Cst.SQLCookieElement.TradingTimestampPrecision.ToString(), value);
            }
        }

        /// <summary>
        /// Obtient ou définit le fuseau horaire d'affichage des horodatages d'audit
        /// </summary>
        /// FI 20171025 [23533] Add
        public static Cst.AuditTimestampZone AuditTimestampZone
        {
            get
            {
                return GetCookieState<Cst.AuditTimestampZone>(Cst.SQLCookieElement.AuditTimestampZone.ToString());
            }
            set
            {
                SetCookieState<Cst.AuditTimestampZone>(Cst.SQLCookieElement.AuditTimestampZone.ToString(), value);
            }
        }

        /// <summary>
        /// Obtient ou définit la précison d'affichage des horodatages d'audit
        /// </summary>
        /// FI 20171025 [23533] Add
        public static Cst.AuditTimestampPrecision AuditTimestampPrecision
        {
            get
            {
                return GetCookieState<Cst.AuditTimestampPrecision>(Cst.SQLCookieElement.AuditTimestampPrecision.ToString());
            }
            set
            {
                SetCookieState<Cst.AuditTimestampPrecision>(Cst.SQLCookieElement.AuditTimestampPrecision.ToString(), value);
            }
        }

        /// <summary>
        /// GUI Mode sauvegardé dans la table COOKIEs
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static Cst.CSSModeEnum CSSMode
        {
            get
            {
                return GetCookieState<Cst.CSSModeEnum>(Cst.SQLCookieElement.CSSMode.ToString());
            }
            set
            {
                SetCookieState<Cst.CSSModeEnum>(Cst.SQLCookieElement.CSSMode.ToString(), value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int TrackerRefreshInterval
        {
            get { return GetRefreshInterval(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerRefreshInterval); }
            set { SetRefreshInterval(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerRefreshInterval, value); }
        }

        /// EG 20201126 [25290] Mécanisme d'ignorement du couple user/pwd saisis lorsque la saisie a été initiée depuis un certain temps
        public static int UserPasswordReset
        {
            get {return Convert.ToInt32(SystemSettings.GetAppSettings("UserPwdResetInterval", "30"));}
        }
        /// <summary>
        /// Obitient ou définit l'historique du tracker
        /// </summary>
        public static string TrackerHistoric
        {
            get { return GetHistoric(Cst.SQLCookieGrpElement.Tracker); }
            set { SetHistoric(Cst.SQLCookieGrpElement.Tracker, value); }
        }

        /// <summary>
        /// Obtient ou définit l'historique des EVENTs pour un trade
        /// </summary>
        // EG 20190419 [EventHistoric Settings] New
        public static string EventHistoric
        {
            get { return GetHistoric(Cst.SQLCookieGrpElement.TradeEvents); }
            set { SetHistoric(Cst.SQLCookieGrpElement.TradeEvents, value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Int64 TrackerAlertProcess
        {
            get
            {
                Int64 defaultValue = Cst.GetPowerOfAllProcess();
                long tmpvalue;
                try
                {
                    object itemSession = HttpSessionStateTools.Get(SessionState, Cst.SQLCookieElement.TrackerAlertProcess.ToString());
                    if (null == itemSession)
                    {
                        AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.Requester, Cst.SQLCookieElement.TrackerAlertProcess.ToString(), out string s);
                        tmpvalue = Convert.ToInt64(s);
                        if (IsSessionAvailable)
                            SessionState[Cst.SQLCookieElement.TrackerAlertProcess.ToString()] = tmpvalue;
                    }
                    else
                        tmpvalue = Convert.ToInt64(itemSession);
                }
                catch { tmpvalue = defaultValue; }
                if (0 > tmpvalue)
                    tmpvalue = defaultValue;
                return tmpvalue;
            }
            set
            {
                Int64 defaultValue = Cst.GetPowerOfAllProcess();
                long tmpvalue;
                try
                {
                    tmpvalue = Convert.ToInt64(Math.Abs(value));
                    AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Requester, Cst.SQLCookieElement.TrackerAlertProcess.ToString(), tmpvalue.ToString());
                }
                catch { tmpvalue = defaultValue; }
                if (IsSessionAvailable)
                    SessionState[Cst.SQLCookieElement.TrackerAlertProcess.ToString()] = tmpvalue;
            }
        }

        /// <summary>
        /// Nouveau tracker (Mode FAVORI)
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static Int64 TrackerGroupFav
        {
            get
            {
                return GetTrackerGroup(Cst.SQLCookieElement.TrackerGroupFav);
            }
            set
            {
                SetTrackerGroup(Cst.SQLCookieElement.TrackerGroupFav, value);
            }
        }

        /// <summary>
        /// Nouveau tracker (Mode DETAIL)
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static Int64 TrackerGroupDetail
        {
            get
            {
                return GetTrackerGroup(Cst.SQLCookieElement.TrackerGroupDetail);
            }
            set
            {
                SetTrackerGroup(Cst.SQLCookieElement.TrackerGroupDetail, value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20190730 Upd (Add paramètre pSQLCookieGrpElement et valeur par défaut)
        public static bool IsTrackerAlert
        {
            get { return GetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerAlert, false); }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerAlert, value); }
        }

        /// <summary>
        /// Defilement des compteurs de statut du tracker
        /// </summary>
        /// EG 20190730 Upd (Add paramètre pSQLCookieGrpElement et valeur par défaut)
        public static bool IsTrackerVelocity
        {
            get
            {
                bool defaultValue = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Tracker_ScrollVelocity", "1"));
                return GetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerVelocity, defaultValue);
            }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerVelocity, value); }
        }
        public static bool IsTrackerRefreshActive
        {
            get { return GetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerRefreshActive, false); }
            set { SetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.Tracker, Cst.SQLCookieElement.TrackerRefreshActive, value); }
        }
        /// <summary>
        /// 
        /// </summary>
        public static int TrackerNbRowPerGroup
        {
            get { return GetNbRowPerGroup(Cst.SQLCookieGrpElement.Tracker); }
            set { SetNbRowPerGroup(Cst.SQLCookieGrpElement.Tracker, value); }
        }

        /// <summary>
        /// Obtient ou définit la varaible session INITIALIZED
        /// </summary>
        public static bool IsInitialized
        {
            get { return BoolFunc.IsTrue(HttpSessionStateTools.Get(SessionState, "INITIALIZED")); }
            set { HttpSessionStateTools.Set(SessionState, "INITIALIZED", value); }
        }

        /// <summary>
        /// Obtient ou définit la variable session Company_CssColor (remplace Company_CssFileName)
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20201130 [25556] Correction CSSColor par defaut si non spécifié sur Entité(gray)
        public static string Company_CssColor
        {
            get { 
                string cssColor = (string) HttpSessionStateTools.Get(SessionState, "Company_CssColor");
                if (StrFunc.IsEmpty(cssColor))
                    cssColor = SystemSettings.GetAppSettings("Spheres_StyleSheetColor", "gray");
                return cssColor;
            }
            set { HttpSessionStateTools.Set(SessionState, "Company_CssColor", value); }
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static string Company_CssMode
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "Company_CssMode"); }
            set { HttpSessionStateTools.Set(SessionState, "Company_CssMode", value); }
        }

        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        public static AdditionalCheckBoxEnum2 ValidityData
        {
            get
            {
                return GetCookieState<AdditionalCheckBoxEnum2>(Cst.SQLCookieElement.ValidityData.ToString());
            }
            set
            {
                SetCookieState<AdditionalCheckBoxEnum2>(Cst.SQLCookieElement.ValidityData.ToString(), value);
            }
        }
        /// <summary>
        /// Obtient ou définit la variable session SOURCEVISIBLE
        /// </summary>
        public static bool IsSourceVisible
        {
            get { return BoolFunc.IsTrue(HttpSessionStateTools.Get(SessionState, "SOURCEVISIBLE")); }
            set { HttpSessionStateTools.Set(SessionState, "SOURCEVISIBLE", value); }
        }

        /// <summary>
        /// Obtient ou définit la variable session DTARCHIVE (Database Archive -> ReadOnly)
        /// </summary>
        public static DateTime DtArchive
        {
            get
            {
                DateTime ret = DateTime.MinValue;
                if (null != HttpSessionStateTools.Get(SessionState, "DTARCHIVE"))
                    ret = (DateTime)HttpSessionStateTools.Get(SessionState, "DTARCHIVE");
                return ret;
            }
            set
            {
                if (DtFunc.IsDateTimeFilled(value))
                    HttpSessionStateTools.Set(SessionState, "DTARCHIVE", value);
                else
                    HttpSessionStateTools.Set(SessionState, "DTARCHIVE", null);
            }
        }
        
        /// <summary>
        /// Exploite la variable session DTARCHIVE (Database Archive -> ReadOnly)
        /// </summary>
        public static bool IsDataArchive
        {
            get { return DtFunc.IsDateTimeFilled(SessionTools.DtArchive); }
        }

        /// <summary>
        /// Obtient la connectionString enrichi avec les attributs Spheres® (gestion de la trace, du cache, etc...) 
        /// <para>Equivalent à CSPlus</para>
        /// </summary>
        public static string CS
        {
            get
            {
                string ret = ConnectionString;

                CSManager csManager = new CSManager(ret);
                if (SessionTools.Collaborator_ISRDBMSTRACE)
                {
                    csManager.IdA = SessionTools.Collaborator_IDA;
                    csManager.HostName = SessionTools.ServerAndUserHost;
                }

                if (SessionTools.IsNoCache)
                {
                    csManager.IsUseCache = false;
                }

                ret = csManager.CsSpheres;

                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit la variable session ConnectionString
        /// </summary>
        public static string ConnectionString
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "ConnectionString"); }
            set { HttpSessionStateTools.Set(SessionState, "ConnectionString", value); }
        }

        /// <summary>
        /// Obtient ou définit la variable session LastConnectionString
        /// </summary>
        /// FI 20120911 New property
        public static string LastConnectionString
        {
            get { return (string)HttpSessionStateTools.Get(SessionState, "LastConnectionString"); }
            set { HttpSessionStateTools.Set(SessionState, "LastConnectionString", value); }
        }
 
        /// <summary>
        /// Historique des SessionConnectionString utilisé par la session
        /// </summary>
        /// FI 20120911 New property
        public static List<string> CSHistory
        {
            get { return HttpSessionStateTools.Get(SessionState, "LstCSHistory") as List<string>; }
            set { HttpSessionStateTools.Set(SessionState, "LstCSHistory", value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsNoCache
        {
            get { return BoolFunc.IsTrue(HttpSessionStateTools.Get(SessionState, "IsNoCache")); }
            set { HttpSessionStateTools.Set(SessionState, "IsNoCache", value); }
        }

        /// <summary>
        /// Obtient ou définit la date d'expiration 
        /// </summary>
        public static DateTime LoginExpirationDateTime
        {
            get
            {
                DateTime ret = new DateTime(9999, 1, 1);
                if (null != HttpSessionStateTools.Get(SessionState, "EXPIRATION"))
                    ret = new DtFunc().StringDateTimeISOToDateTime((string)SessionState["EXPIRATION"]);
                return ret;
            }
            set
            {
                HttpSessionStateTools.Set(SessionState, "EXPIRATION", DtFunc.DateTimeToStringISO(value));
            }
        }

        /// <summary>
        ///  Obtient ou définit l'horodatage (UTC) de la précédente connexion de l'utilisateur
        /// </summary>
        public static DateTime DTLASTLOGIN
        {
            get
            {
                DateTime ret = DateTime.MinValue;
                if (null != HttpSessionStateTools.Get(SessionState, "DTLASTLOGIN"))
                    ret = (DateTime)HttpSessionStateTools.Get(SessionState, "DTLASTLOGIN");
                return ret;
            }
            set { HttpSessionStateTools.Set(SessionState, "DTLASTLOGIN", value); }
        }

        /// <summary>
        ///  Obtient ou définit l'état de connexion de la session
        /// </summary>
        /// EG 20170330 No try/Catch 
        public static Cst.ConnectionState ConnectionState
        {
            get
            {
                Cst.ConnectionState ret = Cst.ConnectionState.INIT;
                if (null != HttpSessionStateTools.Get(SessionState, "ConnectionState"))
                    ret = (Cst.ConnectionState)HttpSessionStateTools.Get(SessionState, "ConnectionState");
                return ret;
            }
            set
            {
                HttpSessionStateTools.Set(SessionState, "ConnectionState", value);
            }
        }

        /// EG 20170330 No try/Catch
        public static CultureInfo LogMessageCulture
        {
            get
            {
                CultureInfo ret = CultureInfo.CreateSpecificCulture("en-GB");
                if (null != HttpSessionStateTools.Get(SessionState, "LogMessageCulture"))
                    ret = (CultureInfo)HttpSessionStateTools.Get(SessionState, "LogMessageCulture");
                return ret;
            }
            set { HttpSessionStateTools.Set(SessionState, "LogMessageCulture", value); }
        }

        #region Collaborator_Culture_xxxxxxx
        public static string Collaborator_Culture_EFSCHAR1
        {
            get
            {
                // [A]nglais is the default value
                string eurosysCultureCode = "A";
                string ISOCHAR2 = Collaborator_Culture_ISOCHAR2;
                //TwoLetterISOLanguageName 
                // FR   FRANCE   
                // GF   FRENCH GUIANA  
                // PF   FRENCH POLYNESIA  
                // TF   FRENCH SOUTHERN TERRITORIES
                // IT   ITALY
                // DE   GERMANY
                // ES   SPAIN
                // (source : http://www.iso.org/iso/en/prods-services/iso3166ma/02iso-3166-code-lists/list-en1.html#af )
                if ("fr" == ISOCHAR2 || "gf" == ISOCHAR2 || "pf" == ISOCHAR2 || "tf" == ISOCHAR2)
                    eurosysCultureCode = "F";
                else if ("it" == ISOCHAR2)
                    eurosysCultureCode = "I";
                else if ("de" == ISOCHAR2)
                    eurosysCultureCode = "D";
                //else if("es" == ISOCHAR2)
                //  eurosysCultureCode = "E";

                return eurosysCultureCode;
            }
        }

        public static string Collaborator_Culture_ISOCHAR2
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName; }
        }

        public static string Collaborator_Culture_ISOCHAR3
        {
            get { return System.Threading.Thread.CurrentThread.CurrentUICulture.ThreeLetterISOLanguageName; }
        }
        #endregion Collaborator_Culture_xxxxxxx

        /// EG 20170330 No try/Catch 
        public static Cst.ActionOnConnect ActionOnConnect
        {
            get
            {
                Cst.ActionOnConnect ret = Cst.ActionOnConnect.NONE;
                if (null != HttpSessionStateTools.Get(SessionState, "ActionOnConnect"))
                    ret = (Cst.ActionOnConnect)HttpSessionStateTools.Get(SessionState, "ActionOnConnect");
                return ret;
            }
            set { HttpSessionStateTools.Set(SessionState, "ActionOnConnect", value); }
        }

        /// <summary>
        /// Obtient ou définit les menus de l'application web
        /// <para>Les menus sont stockés dans une variable session</para>
        /// </summary>
        public static SpheresMenu.Menus Menus
        {
            get
            {
                SpheresMenu.Menus ret = (SpheresMenu.Menus)SessionState["Menus"];
                if (null == ret)
                    InitializeMenu();
                return (SpheresMenu.Menus)SessionState["Menus"];
            }
            set { SessionState["Menus"] = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public static License License
        {
            get
            {
                License license = (License)SessionState["License"];
                if (license == null)
                    license = new License();
                return license;
            }
            set { SessionState["License"] = value; }
        }

        /// <summary>
        /// Obtient true, si la session (l'utilisateur) a la possibilté d'action sur un menu
        /// <para>Cette property est valide uniquement sur les menus sans permission</para>
        /// <para>Pour les menus avec permission, il est impératif de contrôler si l'action est permise, cette property n'est donc pas adaptée</para>
        /// <example>
        /// <para>Exemple:"menu génération d'avis d'opéré"</para>
        /// <para>l'opérateur Guest ne pourra pas générer les messages</para>
        /// </example>
        /// </summary>
        public static bool IsActionEnabled
        {
            get
            {
                bool ret;
                //Sur une base ReadOnly aucune permission/action n'est autorisée. Ceci est vrai même si l'acteur est SYSADMIN.
                //PL 20190304 Add test on IsDataArchive() 
                if (CSTools.IsUserReadOnly(SessionTools.CS) || SessionTools.IsDataArchive)
                {
                    ret = false;
                }
                else
                {
                    if (User.IsSessionGuest)
                    {
                        //Pour un GUEST aucune action n'est autorisée
                        ret = false;

                    }
                    else if ((User.IsSessionSysOper) || (User.IsSessionSysOper))
                    {
                        //Pour un SYS tous les actions sont autorisées
                        ret = true;
                    }
                    else
                    {
                        //Pour un USER les droits d'action sur un menu sont fonction de ses droits sur ce menu
                        ret = true;
                    }
                }
                return ret;
            }
        }

        #region Monitoring
        /// <summary>
        /// Intervalle de raffaichissement de la fenêtre de monitoring
        /// </summary>
        public static int MonitoringRefreshInterval
        {
            get { return GetRefreshInterval(Cst.SQLCookieGrpElement.Monitoring, Cst.SQLCookieElement.MonitoringRefreshInterval); }
            set { SetRefreshInterval(Cst.SQLCookieGrpElement.Monitoring, Cst.SQLCookieElement.MonitoringRefreshInterval, value); }
        }

        /// <summary>
        /// Lecture des éléments candidats à monitoring (Puissance de 2)
        /// </summary>
        public static Int64 MonitoringObserverElement
        {
            get
            {
                Int64 defaultValue = MonitoringTools.GetPowerOfMonitoring();
                long tmpvalue;
                try
                {
                    object itemSession = HttpSessionStateTools.Get(SessionState, Cst.SQLCookieElement.MonitoringObserverElement.ToString());
                    if (null == itemSession)
                    {
                        AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.Monitoring, Cst.SQLCookieElement.MonitoringObserverElement.ToString(), out string s);
                        tmpvalue = Convert.ToInt64(s);
                        if (IsSessionAvailable)
                            SessionState[Cst.SQLCookieElement.MonitoringObserverElement.ToString()] = tmpvalue;
                    }
                    else
                        tmpvalue = Convert.ToInt64(itemSession);
                }
                catch { tmpvalue = defaultValue; }
                if (0 > tmpvalue)
                    tmpvalue = defaultValue;
                return tmpvalue;
            }
            set
            {
                Int64 defaultValue = MonitoringTools.GetPowerOfMonitoring();
                long tmpvalue;
                try
                {
                    tmpvalue = Convert.ToInt64(Math.Abs(value));
                    AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Monitoring, Cst.SQLCookieElement.MonitoringObserverElement.ToString(), tmpvalue.ToString());
                }
                catch { tmpvalue = defaultValue; }
                if (IsSessionAvailable)
                    SessionState[Cst.SQLCookieElement.MonitoringObserverElement.ToString()] = tmpvalue;
            }
        }
        #endregion Monitoring

        
        #endregion Accessors

        #region Methods
        
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieElement"></param>
        /// <param name="pValue"></param>
        /// EG 20190730 Upd (Add paramètre pSQLCookieGrpElement)
        public static void SetGenericBooleanCookieElement(Cst.SQLCookieElement pSQLCookieElement, bool pValue)
        {
            SetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.DBNull, pSQLCookieElement, pValue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pSQLCookieElement"></param>
        /// <param name="pValue"></param>
        public static void SetGenericBooleanCookieElement(Cst.SQLCookieGrpElement pSQLCookieGrpElement, Cst.SQLCookieElement pSQLCookieElement, bool pValue)
        {
            bool tmpvalue;
            try
            {
                tmpvalue = pValue;
                AspTools.WriteSQLCookie(pSQLCookieGrpElement, pSQLCookieElement.ToString(), (tmpvalue ? "1" : "0"));
            }
            catch
            {
                tmpvalue = false;
            }
            //
            HttpSessionStateTools.Set(SessionState, pSQLCookieElement.ToString(), tmpvalue);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieElement"></param>
        /// <returns></returns>
        /// EG 20190730 Upd (Add paramètre pSQLCookieGrpElement et valeur par défaut)
        public static bool GetGenericBooleanCookieElement(Cst.SQLCookieElement pSQLCookieElement)
        {
            return GetGenericBooleanCookieElement(Cst.SQLCookieGrpElement.DBNull, pSQLCookieElement, false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pSQLCookieElement"></param>
        /// <param name="pDefaultValue"></param>
        /// <returns></returns>
        public static bool GetGenericBooleanCookieElement(Cst.SQLCookieGrpElement pSQLCookieGrpElement, Cst.SQLCookieElement pSQLCookieElement, bool pDefaultValue)
        {
            //20090128 PL Refactoring and Enhance (Add SetGenericBooleanCookieElement())
            bool ret;
            try
            {
                object itemSession = HttpSessionStateTools.Get(SessionState, pSQLCookieElement.ToString());

                int tmpvalue;
                if ((null == itemSession) && IsConnected)
                {
                    AspTools.ReadSQLCookie(pSQLCookieGrpElement, pSQLCookieElement.ToString(), out string s);
                    tmpvalue = Convert.ToInt32(s);
                    ret = BoolFunc.IsTrue(tmpvalue);

                    if (pSQLCookieElement == Cst.SQLCookieElement.PagerPosition && s == null)
                        ret = true;

                    SetGenericBooleanCookieElement(pSQLCookieElement, ret);
                }
                else
                {
                    tmpvalue = Convert.ToInt32(itemSession);
                    ret = BoolFunc.IsTrue(tmpvalue);
                }
            }
            catch
            {
                ret = pDefaultValue;
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne par ordre de priorité le fuseau horaire du l'utilisateur, du département, ou de l'entité
        /// </summary>
        /// <returns></returns>
        /// FI 20190327 [24603] Add Method
        /// EG 20221026 [XXXXX] Add default value "Etc/UTC";
        public static string GetCriteriaTimeZone()
        {
            string ret;
            if (StrFunc.IsFilled(Collaborator.Timezone))
                ret = Collaborator.Timezone;
            else if (StrFunc.IsFilled(Collaborator.Department.Timezone))
                ret = Collaborator.Department.Timezone;
            else if (StrFunc.IsFilled(Collaborator.Entity.Timezone))
                ret = Collaborator.Entity.Timezone;
            else
                ret = "Etc/UTC";
            return ret;
        }

        /// <summary>
        /// Alimente une clé (avec cookie) dans session 
        /// <para>Alimente au passage la table Cookie</para>
        /// </summary>
        /// <param name="pCookieElement">Nom du cookie</param>
        /// <typeparam name="T">Nécessaire un enum</typeparam>
        /// <param name="pEnum"></param>
        /// FI 20171025 [23533] Add
        private static void SetCookieState<T>(string pCookieElement, System.Enum pEnum) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            String @defaultValue = ReflectionTools.GetDefaultValue<T>().ToString();
            string @value = pEnum.ToString();
            try
            {
                AspTools.WriteSQLCookie(pCookieElement, pEnum.ToString());
            }
            catch
            {
                @value = defaultValue;
            }
            HttpSessionStateTools.Set(SessionState, pCookieElement, @value);
        }

        /// <summary>
        /// Lecture d'une clé (avec cookie) dans session 
        /// <para>Alimentation de la session avec la valeur par défaut s'il n'existe rien</para>
        /// <param name="pCookieElement"></param>
        /// </summary>
        /// <param name="pCookieElement">Nom du cookie</param>
        /// <typeparam name="T">Nécessaire un enum</typeparam>
        /// <returns></returns>
        /// FI 20171025 [23533] Add
        private static T GetCookieState<T>(string pCookieElement) where T : struct
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            T @default = ReflectionTools.GetDefaultValue<T>();
            T ret;
            try
            {
                string s = (string)HttpSessionStateTools.Get(SessionState, pCookieElement);
                if (StrFunc.IsEmpty(s))
                {
                    AspTools.ReadSQLCookie(pCookieElement, out s);
                    if (StrFunc.IsEmpty(s))
                        s = @default.ToString();

                    //S'il n'existe rien en session Spheres® initialise la session
                    //La valeur vient de la table cookie ou FIX dans le cas contraire
                    HttpSessionStateTools.Set(SessionState, pCookieElement, s);
                }
                ret = (T)Enum.Parse(typeof(T), s, true);
            }
            catch
            {
                ret = @default;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <param name="pLevel"></param>
        /// <returns></returns>
        public static int GetTrackerLevelChoice(Cst.SQLCookieElement pCookieElement, LevelStatusTools.LevelEnum pLevel)
        {
            //20090128 PL Enhance (Add SetTrackerLevelChoice())
            int defaultValue = LevelStatusTools.GetPowerOfAllStatusByLevel(pLevel);
            int tmpvalue;
            //
            try
            {
                object itemSession = HttpSessionStateTools.Get(SessionState, pCookieElement.ToString());
                if (null == itemSession)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.Tracker, pCookieElement.ToString(), out string s);
                    tmpvalue = Convert.ToInt32(s);
                    if (IsSessionAvailable)
                        SessionState[pCookieElement.ToString()] = tmpvalue;
                }
                else
                    tmpvalue = Convert.ToInt32(itemSession);
            }
            catch { tmpvalue = defaultValue; }
            if (0 > tmpvalue)
                tmpvalue = defaultValue;
            return tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <param name="pLevel"></param>
        /// <param name="pValue"></param>
        public static void SetTrackerLevelChoice(Cst.SQLCookieElement pCookieElement, LevelStatusTools.LevelEnum pLevel, int pValue)
        {
            int defaultValue = LevelStatusTools.GetPowerOfAllStatusByLevel(pLevel);
            int tmpvalue;
            try
            {
                tmpvalue = Math.Abs(pValue);
                AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Tracker, pCookieElement.ToString(), tmpvalue.ToString());
            }
            catch { tmpvalue = defaultValue; }
            if (IsSessionAvailable)
                SessionState[pCookieElement.ToString()] = tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLevel"></param>
        /// <returns></returns>
        public static int GetTrackerPowerOfAllStatusByLevel(LevelStatusTools.LevelEnum pLevel)
        {
            LevelStatusTools.StatusEnum statusEnum = new LevelStatusTools.StatusEnum();
            FieldInfo[] statusFlds = statusEnum.GetType().GetFields();
            int powerStatus = 0;
            foreach (FieldInfo statusFld in statusFlds)
            {
                object[] levelAssociateAttrs = statusFld.GetCustomAttributes(typeof(LevelStatusTools.LevelAssociateAttribute), true);
                if (0 < levelAssociateAttrs.Length)
                {
                    foreach (LevelStatusTools.LevelAssociateAttribute levelAssociate in levelAssociateAttrs)
                    {
                        if (pLevel == levelAssociate.Level)
                        {
                            LevelStatusTools.StatusEnum status = (LevelStatusTools.StatusEnum)Enum.Parse(typeof(LevelStatusTools.StatusEnum), statusFld.Name, false);
                            int i = int.Parse(Enum.Format(typeof(LevelStatusTools.StatusEnum), status, "d"));
                            powerStatus += (int)Math.Pow(2, i);
                        }
                    }
                }
            }
            return powerStatus;
        }

        /// <summary>
        /// Lecture des groupes du Tracker
        /// </summary>
        /// <param name="pCookieElement"></param>
        /// <returns></returns>
        private static Int64 GetTrackerGroup(Cst.SQLCookieElement pCookieElement)
        {
            Int64 defaultValue = Cst.GetPowerOfAllGroupTracker();
            long tmpvalue;
            try
            {
                object itemSession = HttpSessionStateTools.Get(SessionState, pCookieElement.ToString());
                if (null == itemSession)
                {
                    AspTools.ReadSQLCookie(Cst.SQLCookieGrpElement.Requester, pCookieElement.ToString(), out string s);
                    tmpvalue = Convert.ToInt64(s);
                    if (IsSessionAvailable)
                        SessionState[pCookieElement.ToString()] = tmpvalue;
                }
                else
                    tmpvalue = Convert.ToInt64(itemSession);
            }
            catch { tmpvalue = defaultValue; }
            if (0 >= tmpvalue)
                tmpvalue = defaultValue;
            return tmpvalue;
        }
        
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private static void SetTrackerGroup(Cst.SQLCookieElement pCookieElement, Int64 pValue)
        {
            Int64 defaultValue = Cst.GetPowerOfAllGroupTracker();
            long tmpvalue;
            try
            {
                tmpvalue = Convert.ToInt64(Math.Abs(pValue));
                AspTools.WriteSQLCookie(Cst.SQLCookieGrpElement.Requester, pCookieElement.ToString(), tmpvalue.ToString());
            }
            catch { tmpvalue = defaultValue; }
            if (IsSessionAvailable)
                SessionState[pCookieElement.ToString()] = tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pSQLCookieElement"></param>
        /// <returns></returns>
        private static int GetRefreshInterval(Cst.SQLCookieGrpElement pSQLCookieGrpElement, Cst.SQLCookieElement pSQLCookieElement)
        {
            //20090128 PL Enhance (Add Set())
            int tmpvalue;
            try
            {
                tmpvalue = Convert.ToInt32(SessionState[pSQLCookieElement.ToString()]);
                if (0 >= tmpvalue)
                {
                    AspTools.ReadSQLCookie(pSQLCookieGrpElement, pSQLCookieElement.ToString(), out string s);
                    tmpvalue = Convert.ToInt32(s);
                    SessionState[pSQLCookieElement.ToString()] = tmpvalue;
                }
            }
            catch { tmpvalue = 600; }
            return tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pSQLCookieElement"></param>
        /// <param name="pValue"></param>
        private static void SetRefreshInterval(Cst.SQLCookieGrpElement pSQLCookieGrpElement, Cst.SQLCookieElement pSQLCookieElement, int pValue)
        {
            int tmpvalue;
            try
            {
                tmpvalue = Math.Abs(pValue);
                AspTools.WriteSQLCookie(pSQLCookieGrpElement, pSQLCookieElement.ToString(), tmpvalue.ToString());
            }
            catch { tmpvalue = 600; }
            SessionState[pSQLCookieElement.ToString()] = tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <returns></returns>
        private static int GetNbRowPerGroup(Cst.SQLCookieGrpElement pSQLCookieGrpElement)
        {
            string element;
            if (Cst.SQLCookieGrpElement.Tracker == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.TrackerNbRowPerGroup.ToString();
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSQLCookieGrpElement.ToString()));
            //
            int tmpvalue;
            try
            {
                tmpvalue = Convert.ToInt32(SessionState[element]);
                if (0 >= tmpvalue)
                {
                    AspTools.ReadSQLCookie(pSQLCookieGrpElement, element, out string s);
                    tmpvalue = Convert.ToInt32(s);
                    SessionState[element] = tmpvalue;
                }
            }
            catch { tmpvalue = 15; }
            //
            if (0 >= tmpvalue)
                tmpvalue = 15;
            //
            return tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pValue"></param>
        private static void SetNbRowPerGroup(Cst.SQLCookieGrpElement pSQLCookieGrpElement, int pValue)
        {
            string element;
            if (Cst.SQLCookieGrpElement.Tracker == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.TrackerNbRowPerGroup.ToString();
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSQLCookieGrpElement.ToString()));

            int tmpvalue;
            try
            {
                tmpvalue = Math.Abs(pValue);
                AspTools.WriteSQLCookie(pSQLCookieGrpElement, element, tmpvalue.ToString());
            }
            catch { tmpvalue = 15; }
            SessionState[element] = tmpvalue;
        }

        /// <summary>
        /// Retourne l'historique des consultations TRACKER
        /// <para>Retourne ToDay, 1D, 7D, 1M, 3M ou Beyond</para>
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <returns></returns>
        // EG 20190419 [EventHistoric Settings] Upd
        private static string GetHistoric(Cst.SQLCookieGrpElement pSQLCookieGrpElement)
        {
            string element;
            if (Cst.SQLCookieGrpElement.Tracker == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.TrackerHistoric.ToString();
            else if (Cst.SQLCookieGrpElement.TradeEvents == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.EventHistoric.ToString();
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSQLCookieGrpElement.ToString()));
            string tmpvalue;
            //
            try
            {
                tmpvalue = (String)SessionState[pSQLCookieGrpElement + "_" + element];
                if (StrFunc.IsEmpty(tmpvalue))
                {
                    //System.Diagnostics.Debug.WriteLine("PL 20200806 - GetHistoric - " + DateTime.Now.ToString() + " - " + pSQLCookieGrpElement + " - " + element);
                    AspTools.ReadSQLCookie(pSQLCookieGrpElement, element, out tmpvalue);
                    SessionState[element] = tmpvalue;
                }
            }
            catch { tmpvalue = "1D"; }

            if (StrFunc.IsEmpty(tmpvalue))
                tmpvalue = "1D";
            return tmpvalue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSQLCookieGrpElement"></param>
        /// <param name="pValue"></param>
        // EG 20190419 [EventHistoric Settings] Upd
        private static void SetHistoric(Cst.SQLCookieGrpElement pSQLCookieGrpElement, string pValue)
        {
            string element;
            if (Cst.SQLCookieGrpElement.Tracker == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.TrackerHistoric.ToString();
            else if (Cst.SQLCookieGrpElement.TradeEvents == pSQLCookieGrpElement)
                element = Cst.SQLCookieElement.EventHistoric.ToString();
            else
                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", pSQLCookieGrpElement.ToString()));

            string tmpvalue;
            try
            {
                AspTools.WriteSQLCookie(pSQLCookieGrpElement, element, pValue);
                tmpvalue = pValue;
            }
            catch { tmpvalue = "1D"; }
            SessionState[element] = tmpvalue;
        }

        /// <summary>
        /// Ajoute dans la variable session LstCSHistory le connectionString courante 
        /// <para>La variable LstCSHistory contient la liste des ConnectionString utilisées avec succès par la session</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20120907 [18113] add AddCSHistory
        public static void AddCSHistory()
        {
            string CS = SessionTools.ConnectionString;

            List<string> lstCSHistory = CSHistory;
            if (null == lstCSHistory)
                lstCSHistory = new List<string>();
            if (false == lstCSHistory.Contains(CS))
                lstCSHistory.Add(CS);

            HttpSessionStateTools.Set(SessionState, "LstCSHistory", lstCSHistory);
        }

        /// <summary>
        /// Obtient true si la date courante est > à la date d'expiration
        /// </summary>
        /// <returns></returns>
        public static bool IsLoginExpire()
        {
            return (OTCmlHelper.GetDateSys(CS) > LoginExpirationDateTime);
        }

        #region SessionClipBoard
        /// <summary>
        /// 
        /// </summary>
        public static void InitSessionClipBoard()
        {
            if (Software.IsSoftwareOTCmlOrFnOml())
                SessionClipBoard = new SessionClipBoard(CS, SessionTools.AppSession);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void DeleteSessionClipBoard()
        {
            try
            {
                if (Software.IsSoftwareOTCmlOrFnOml())
                {
                    SessionClipBoard sessionClipBoard = SessionTools.SessionClipBoard;
                    if (null != sessionClipBoard)
                        sessionClipBoard.Delete();
                }
            }
            catch { } // Voulu don't touch
        }
        #endregion SessionClipBoard

        /// <summary>
        /// 
        /// </summary>
        public static void InitializeMenu()
        {
            if (SessionTools.IsConnected)
            {
                string border = SystemSettings.GetAppSettings("Spheres_DebugDesign");
                string style_display = @" style=""display: none;";
                if (SessionTools.AppSession.BrowserInfo.IndexOf("JS=None") >= 0)
                    style_display = @"";
                Menus = new SpheresMenu.MenuChecking(CS, Collaborator_IDA, ActorAncestor, Collaborator_ROLE, Software.MenuRoot(), style_display, border).Menus;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pClb"></param>
        /// <param name="pSource"></param>
        /// <param name="pRdbmsName"></param>
        /// <param name="pServerName"></param>
        /// <param name="pDatabaseName"></param>
        /// <param name="pLoginExpirationDateTime"></param>
        /// <returns></returns>
        // EG 20180425 Analyse du code Correction [CA2202]
        public static bool Initialize2(Collaborator pClb, string pSource,
                                                  string pRdbmsName, string pServerName, string pDatabaseName,
                                                  DateTime pLoginExpirationDateTime)
        {
            bool isOk = true;
            string source = pSource;

            int dbMajor = 0;
            int dbMinor = 0;
            int dbBuild = 0;
            int dbRevision = 0;
            string dbStatus = Cst.DBStatusEnum.NA.ToString();

            string SQLSelect = SQLCst.SELECT + "MAJOR,MINOR,REVISION,BUILD,DBSTATUS" + Cst.CrLf;
            SQLSelect += SQLCst.FROM_DBO + Cst.EFS_TBL.EFSSOFTWARE.ToString() + Cst.CrLf;
            SQLSelect += SQLCst.WHERE + "IDEFSSOFTWARE=" + DataHelper.SQLString(Software.Name);

            using (IDataReader dr = DataHelper.ExecuteReader(source, CommandType.Text, SQLSelect))
            {
                if (dr.Read())
                {
                    dbMajor = Convert.ToInt32(dr["MAJOR"]);
                    dbMinor = Convert.ToInt32(dr["MINOR"]);
                    dbRevision = Convert.ToInt32(dr["REVISION"]);
                    dbBuild = Convert.ToInt32(dr["BUILD"]);
                    dbStatus = Convert.ToString(dr["DBSTATUS"]);
                }
            }

            Initialize2(source, pRdbmsName, pServerName, pDatabaseName,
                                    dbMajor, dbMinor, dbRevision, dbBuild, dbStatus,
                                    pClb, pLoginExpirationDateTime);

            //DTLASTLOGIN 
            Login_Log.Login(source, SessionTools.AppSession, out DateTime dtLstlogin);
            SessionTools.DTLASTLOGIN = dtLstlogin;

            //IsNoCache
            SessionTools.IsNoCache = BoolFunc.IsFalse(HttpContext.Current.Application["ISDATACACHEENABLED"]);
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pSource"></param>
        /// <param name="pRDBMS"></param>
        /// <param name="pServer"></param>
        /// <param name="pDatabase"></param>
        /// <param name="pDbMajor"></param>
        /// <param name="pDbMinor"></param>
        /// <param name="pDbRevision"></param>
        /// <param name="pDbBuild"></param>
        /// <param name="pDbStatus"></param>
        /// <param name="pClb"></param>
        /// <param name="pLoginExpirationDateTime"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200921 [XXXXX] Nouvelle interface GUI v10 Implémentation AppSettings : Spheres_StyleSheetColor
        public static void Initialize2(string pSource, string pRDBMS, string pServer, string pDatabase,
            int pDbMajor, int pDbMinor, int pDbRevision, int pDbBuild, string pDbStatus, Collaborator pClb, DateTime pLoginExpirationDateTime)
        {
            ConnectionString = pSource;
            //
            Collaborator = pClb;
            //
            ActionOnConnect = pClb.ActionOnConnect;
            string cssColor = pClb.CssColor;
            if (StrFunc.IsEmpty(cssColor))
                cssColor = SystemSettings.GetAppSettings("Spheres_StyleSheetColor");
            Company_CssColor = cssColor;

            //
            Data.RDBMS = pRDBMS;
            Data.Server = pServer;
            Data.Database = pDatabase;
            Data.DatabaseVersionBuild = pDbMajor.ToString() + "." + pDbMinor.ToString() + "." + pDbRevision.ToString() + "." + pDbBuild.ToString();
            Data.DbStatus = pDbStatus;
            //
            WelcomeMsg = string.Empty;
            //
            LoginExpirationDateTime = pLoginExpirationDateTime;

            RemoveTrackerSessionsState();

            MessageAfterOnConnectOk = null;
            //
            IsInitialized = true;
            //
            if (Collaborator.ValidityEnum.Succes == pClb.Validity)
                SetCulture(pClb.Culture);


            // FI 20191227 [XXXXX] Alimentation de la property AppSession
            AppSession.IdA = Collaborator_IDA;
            AppSession.IdA_Identifier = Collaborator_IDENTIFIER;
            AppSession.IdA_Entity = Collaborator_ENTITY_IDA;
            AppSession.IdA_Identifier_Entity = Collaborator_ENTITY_IDENTIFIER;
        }

        /// <summary>
        /// Efface les données de Sessions propres au tracker
        /// </summary>
        // EG 20190926 New
        private static void RemoveTrackerSessionsState()
        {
            if (IsSessionAvailable)
            {
                Cst.SQLCookieElement element = new Cst.SQLCookieElement();
                List<FieldInfo> processFlds = element.GetType().GetFields().ToList().Where(item => item.Name.StartsWith("Tracker")).ToList();
                if (null != processFlds)
                    processFlds.ForEach(item => SessionState.Remove(item.Name));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCulture"></param>
        /// FI 20170215 [XXXXX] Modify
        public static void SetCulture(string pCulture)
        {
            if (pCulture != Cst.CULTURE_SEPARATOR)
            {
                //FI 20170215 [XXXXX] Appel ThreadTools.SetCurrentCulture
                //SystemTools.SetCurrentCulture(pCulture);
                ThreadTools.SetCurrentCulture(pCulture);

                //Save Culture to Cookie
                CultureInfo culture = CultureInfo.CreateSpecificCulture(pCulture);
                AspTools.WriteCookie("UserDefault", "Culture", culture.Name, 1, "Y", "", out HttpCookie cookie);
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }

        /// <summary>
        /// Reset des infos de connection à la base de donnée
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static void ResetLoginInfos()
        {
            //
            Collaborator = null;
            //
            ConnectionString = null;
            Data.RDBMS = null;
            Data.Server = null;
            Data.Database = null;
            Data.DatabaseVersionBuild = null;
            //
            Menus = null;
            //
            ActionOnConnect = Cst.ActionOnConnect.NONE;
            LoginExpirationDateTime = new DateTime();
            Company_CssColor = null;
            //
            IsInitialized = true; //Warning: Ne pas remettre à false
            //
            License = null;
        }

        /// <summary>
        /// Supprime les locks, les restrictions, les données du cache (autocomplete , DataCache)
        /// </summary>
        public static void CleanUp()
        {
            if (IsSessionAvailable)
            {
                if (StrFunc.IsFilled(CS))
                {
                    //FI 20110704 Add Commentaire 
                    //attention même si IsSessionAvailable on peut avoir SessionTools.CS vide
                    //Cela se produit lors d'un timeout serveur web
                    LockTools.UnLockSession(CS, SessionID);
                    UnSetRestriction();
                    DeleteSessionClipBoard();
                }
                // FI 20191227 [XXXXX] Call AutoCompleteDataCache.Clear
                AutoCompleteDataCache.Clear();
                // FI 20200225 [XXXXX] Call DataCache.Clear
                DataCache.Clear();
            }
        }

        /// <summary>
        /// Retourne le résultat de la substitution des mots clefs session par leur valeur dans {pInitialString} 
        /// </summary>
        /// <param name="pInitialString">Chaine qui contient éventuellement les mots clefs</param>
        /// <returns></returns>
        /// FI 20111125 gestion de Cst.IDA_ENTITY
        public static string ReplaceDynamicConstantsWithValues(string pInitialString)
        {
            string finalString = pInitialString;

            //
            //PL 20100623 Add test on "%%"
            if (StrFunc.IsFilled(finalString) && (finalString.IndexOf("%%") >= 0))
            {
                User user = SessionTools.User;

                finalString = ReplaceKeywordInstring(finalString, Cst.IDEFSSOFTWARE, Software.Name);
                finalString = ReplaceKeywordInstring(finalString, Cst.EXTLLINK_USER, SessionTools.Collaborator_EXTLLINK);
                finalString = ReplaceKeywordInstring(finalString, Cst.ROLE_USER, SessionTools.Collaborator_ROLE);
                finalString = ReplaceKeywordInstring(finalString, Cst.IDA_USER, SessionTools.Collaborator_IDA.ToString());

                //PL 20111227 Il existe les constantes %%IDA_ENTITY%% et %%COLUMN_VALUE%%IDA_ENTITY%%
                finalString = ReplaceKeywordInstring(finalString, "%%COLUMN_VALUE%%IDA_ENTITY%%", "%%COLUMN_VALUE%%***%%");
                finalString = ReplaceKeywordInstring(finalString, Cst.IDA_ENTITY, user.Entity_IdA.ToString());
                finalString = ReplaceKeywordInstring(finalString, "%%COLUMN_VALUE%%***%%", "%%COLUMN_VALUE%%IDA_ENTITY%%");

                finalString = ReplaceKeywordInstring(finalString, Cst.IDENTIFIER_USER, SessionTools.Collaborator_IDENTIFIER);
                finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_IDA_USER, SessionTools.Collaborator_ENTITY_IDA.ToString());
                finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_IDENTIFIER_USER, SessionTools.Collaborator_ENTITY_IDENTIFIER);
                if (finalString.IndexOf(Cst.PARENT_BIC_USER.Substring(0, 12)) >= 0)
                {
                    finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_BIC_USER, SessionTools.Collaborator_ENTITY_BIC);
                    finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_BIC4_USER, SessionTools.Collaborator_ENTITY_BIC.Substring(0, 4));
                    finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_BIC6_USER, SessionTools.Collaborator_ENTITY_BIC.Substring(0, 6));
                    finalString = ReplaceKeywordInstring(finalString, Cst.PARENT_BIC8_USER, SessionTools.Collaborator_ENTITY_BIC.Substring(0, 8));
                }
                finalString = ReplaceKeywordInstring(finalString, Cst.SESSIONID, SessionTools.SessionID);
                finalString = ReplaceKeywordInstring(finalString, Cst.SHORTSESSIONID, SessionTools.ShortSessionID);
                finalString = ReplaceKeywordInstring(finalString, Cst.IDA_ANCESTOR, SessionTools.ActorAncestor.GetListIdA_Ancestor());
                finalString = ReplaceKeywordInstring(finalString, Cst.CULTURE_USER_EFSCHAR1, SessionTools.Collaborator_Culture_EFSCHAR1);
                finalString = ReplaceKeywordInstring(finalString, Cst.CULTURE_USER_ISOCHAR2, SessionTools.Collaborator_Culture_ISOCHAR2);
                finalString = ReplaceKeywordInstring(finalString, Cst.CULTURE_USER_ISOCHAR3, SessionTools.Collaborator_Culture_ISOCHAR3);

                //PL 20170403 [23015]
                finalString = ReplaceKeywordInstring(finalString, Cst.EXCLUDEDVALUESFORFEES, Cst.TrdType_ExcludedValuesForFees_ETD);
            }
            return finalString;
        }

        /// <summary>
        /// Remplace un mot clef (ex %%SESSIONID%%) par sa valeur
        /// </summary>
        /// <param name="pkeyword"></param>
        /// <param name="pString"></param>
        private static string ReplaceKeywordInstring(string pInitialString, string pkeyword, string pKeyWordValue)
        {
            string ret = pInitialString;
            ret = ret.Replace(pkeyword + ".ToUpper()", pKeyWordValue.ToUpper());
            ret = ret.Replace(pkeyword, pKeyWordValue);
            return ret;
        }


        /// <summary>
        /// Lecture des catégories autorisées dans l'upload de fichiers
        /// </summary>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static MimeMappingTools.TypeCategoryEnum UploadFile_CategoryAuthorized
        {
            get
            {
                string key = "UploadFile_CategoryAuthorized";
                string category = (string) HttpSessionStateTools.Get(SessionState, key);
                if (StrFunc.IsEmpty(category))
                {
                    category = SystemSettings.GetAppSettings(key, MimeMappingTools.TypeCategoryEnum.defaultUpload.ToString());
                    HttpSessionStateTools.Set(SessionState, key, category);
                }
                return MimeMappingTools.StringMimeTypeToEnum(category);
            }
        }
        /// <summary>
        /// Lecture des types de fichier exclus/autorisés dans l'upload de fichiers
        /// </summary>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static string GetUploadFile_ExtensionFiles(string pPrefix, string pSuffix)
        {
            string key = $"UploadFile_{pPrefix}ExtensionFiles{pSuffix}";
            string typeFiles = (string)HttpSessionStateTools.Get(SessionState, key);
            if (StrFunc.IsEmpty(typeFiles))
            {
                typeFiles = (string)SystemSettings.GetAppSettings(key);
                HttpSessionStateTools.Set(SessionState, key, typeFiles);
            }
            return typeFiles;
        }
        /// <summary>
        /// Lecture de la taille maximale par defaut autorisée pour un upload de fichier
        /// </summary>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static int UploadFile_DefaultContentMaxLength
        {
            get
            {
                string key = "UploadFile_DefaultContentMaxLength";
                string length = (string) HttpSessionStateTools.Get(SessionState, key);
                if (StrFunc.IsEmpty(length))
                {
                    length = SystemSettings.GetAppSettings(key, "0");
                    HttpSessionStateTools.Set(SessionState, key, length);
                }
                return Convert.ToInt32(length);
            }
        }
        /// <summary>
        /// Lecture de la taille maximale pour une catégorie donnée autorisée pour un upload de fichier
        /// </summary>
        /// <param name="pMimeTypeCategory"></param>
        /// <returns></returns>
        /// EG 20220215 [26251][WI582] New : Vulnerability on unrestricted file upload : Enhancement
        public static int GetUploadFile_CategoryContentMaxLength(MimeMappingTools.TypeCategoryEnum pMimeTypeCategory)
        {
            string key = $"UploadFile_{pMimeTypeCategory}ContentMaxLength";
            string length = (string)HttpSessionStateTools.Get(SessionState, key);
            if (StrFunc.IsEmpty(length))
            {
                length = (string) SystemSettings.GetAppSettings(key, UploadFile_DefaultContentMaxLength.ToString());
                HttpSessionStateTools.Set(SessionState, key, length);
            }
            return Convert.ToInt32(length);
        }
        /// <summary>
        /// Retourne true si l'utilisateur connecté est un utilisateur avec "droits restreints".
        /// </summary>
        /// <param name="pRole"></param>
        /// <returns></returns>
        public static bool IsUserWithLimitedRights()
        {
            //NB: on pourra plus tard envisager affiner ce mode via une customisation plus fine depuis le fichier de config.
            return CSTools.IsUserReadOnly(SessionTools.CS) || User.IsSessionGuest;
        }

        /// <summary>
        /// Alimentation de SESSIONRESTRICT
        /// </summary>
        public static void SetRestriction()
        {
            SessionRestrictManager srmng = new SessionRestrictManager(CS, SessionTools.AppSession, Collaborator.NewUser());
            SQLUP.GetId(out int idsessionId, CS, SQLUP.IdGetId.SESSIONRESTRICT);
            srmng.SetRestriction(idsessionId);
        }

        /// <summary>
        /// Alimentation de SESSIONRESTRICT (asynchrone)
        /// </summary>
        /// <returns></returns>
        public static async Task SetRestrictionAsync()
        {
            SessionRestrictManager srmng = new SessionRestrictManager(CS, SessionTools.AppSession, SessionTools.Collaborator.NewUser());
            SQLUP.GetId(out int idsessionId, CS, SQLUP.IdGetId.SESSIONRESTRICT);
            await srmng.SetRestrictionAsync(idsessionId);
        }

        /// <summary>
        /// Suppression de SESSIONRESTRICT------
        /// </summary>
        public static void UnSetRestriction()
        {
            SessionRestrictManager srmng = new SessionRestrictManager(CS, SessionTools.AppSession, Collaborator.NewUser());
            srmng.UnSetRestriction();
        }

        /// <summary>
        /// Retourne une collection avec les formats d'affichage associés à la session
        /// <para>ETDMaturityFormat,AuditTimestampZone,AuditTimestampPrecision,TradingTimestampZone,TradingTimestampPrecision </para>
        /// </summary>
        /// <returns></returns>
        /// FI 20200106 [XXXXX] add 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public static NameValueCollection FmtDisplayCollection()
        {
            NameValueCollection ret = new NameValueCollection
            {
                { "ETDMaturityFormat", SessionTools.ETDMaturityFormat.ToString() },
                { "AuditTimestampZone", SessionTools.AuditTimestampZone.ToString() },
                { "AuditTimestampPrecision", SessionTools.AuditTimestampPrecision.ToString() },
                { "TradingTimestampZone", SessionTools.TradingTimestampZone.ToString() },
                { "TradingTimestampPrecision", SessionTools.TradingTimestampPrecision.ToString() },
                { "CSSMode", SessionTools.CSSMode.ToString() }
            };
            return ret;
        }
        #endregion Methods

        #region processLog
        /// <summary>
        /// Obtient ou définit la variable session <see cref="ProcessLog"/>.  <see cref="ProcessLog"/> alimente le log avec les activités de la session web
        /// </summary>
        public static ProcessLog ProcessLog
        {
            get { return (ProcessLog)HttpSessionStateTools.Get(SessionState, "ProcessLog"); }
            set { HttpSessionStateTools.Set(SessionState, "ProcessLog", value); }
        }

        /// <summary>
        /// Obtient ou définit la variable session <see cref="Logger"/>. <see cref="Logger"/> alimente le log avec les activités de la session web
        /// </summary>
        /// FI 20240111 [WI793] Add
        public static LoggerScope Logger
        {
            get { return (LoggerScope)HttpSessionStateTools.Get(SessionState, "Logger"); }
            set { HttpSessionStateTools.Set(SessionState, "Logger", value); }
        }

        /// <summary>
        ///  Retourne true s'il existe un <see cref="Logger"/> ou un <see cref="ProcessLog"/> actif
        /// </summary>
        /// FI 20240111 [WI793] Add
        public static Boolean ExistLog
        {
            get { return (null != Logger) || (null != ProcessLog); }
        }

        /// <summary>
        ///  Initialiation de <see cref="ProcessLog"/> ou de <see cref="Logger"/>. (<see cref="Logger"/> sera de préférence utilisé s'il a été correctement initialisé)
        ///  <para></para>
        /// </summary>
        /// <param name="pCS"></param>
        /// FI 20240111 [WI793] Add
        public static void InitLog(string pCS)
        {
            if (LoggerManager.IsInitialized)
            {
                SessionTools.InitLogger(pCS);
            }
            else
            {
                SessionTools.InitProcessLog(pCS);
            }
        }


        /// <summary>
        /// Initialise la variable session <see cref="ProcessLog"/>
        /// </summary>
        /// <param name="pCS">CS de la base dédiée au log</param>
        public static void InitProcessLog(string pCS)
        {
            List<string> data = new List<string> {
                "N/A",
                "SessionId [" + SessionTools.SessionID.ToString() + "]",
                SessionTools.ServerAndUserHost, SessionTools.BrowserInfo
            };

            // Alimentation de PROCESS_L
            List<string> dataInfo = new List<string> { "LOG-00001" }; //Session Start
            dataInfo.AddRange(data);
            ProcessLogInfo logInfo = new ProcessLogInfo
            {
                status = ProcessStateTools.StatusNoneEnum.ToString(),
            };
            logInfo.SetMessageAndData(dataInfo.ToArray());
            ProcessLog processLog = new ProcessLog(pCS, Cst.ProcessTypeEnum.WEBSESSION, SessionTools.AppSession, logInfo);
            processLog.SetHeaderStatus(ProcessStateTools.StatusEnum.NONE);
            processLog.SQLWriteHeader();

            SessionTools.ProcessLog = processLog;

            // Alimentation du log détail avec Session Start
            LogAddInfo(LogLevelEnum.Info, "LOG-00002", data.ToArray());
        }

        /// <summary>
        /// Initialise la variable session <see cref="Logger"/>
        /// </summary>
        /// <param name="pCS">CS de la base dédiée au log</param>
        /// FI 20240111 [WI793] Add
        public static void InitLogger(string pCS)
        {
            LogScope scope = new LogScope(Cst.ProcessTypeEnum.WEBSESSION.ToString(), pCS);

            LoggerScope logger = new LoggerScope();
            logger.BeginScope(OTCmlHelper.GetDateSysUTC, scope);

            List<string> data = new List<string> {
                "N/A",
                "SessionId [" + SessionTools.SessionID.ToString() + "]",
                SessionTools.ServerAndUserHost, SessionTools.BrowserInfo
            };

            // Alimentation de PROCESS_L 
            logger.Log(new LoggerData(LogLevelEnum.Info, new SysMsgCode(SysCodeEnum.LOG, 1), 0, LoggerTools.LogParamFromString(data)), true); //Session_Start
            SessionTools.Logger = logger;


            // Alimentation du log détail avec Session Start
            LogAddInfo(LogLevelEnum.Info, "LOG-00002", data.ToArray());
        }

        /// <summary>
        /// Ecriture dans le Log des informations de connexion
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="pActionProcess"></param>
        /// <param name="pData"></param>
        /// FI 20240111 [WI793] Add
        public static void LogAddLoginInfo(LogLevelEnum logLevel, ActionLogin pActionProcess, string[] pData)
        {
            ProcessLogExtend logProcess = new ProcessLogExtend(ProcessLog, Logger);
            Login_Log.AddProcessLog(logProcess, logLevel, pActionProcess, pData);
        }

        /// <summary>
        /// Ecriture dans le Log d'un message
        /// </summary>
        /// <param name="pLogLevel"></param>
        /// <param name="pMessage">message de type <see cref="String"/> ou de type <see cref="SysMsgCode"/></param>
        /// <param name="pData"></param>
        /// FI 20240111 [WI793] Add
        public static void LogAddInfo<T>(LogLevelEnum pLogLevel, T pMessage, string[] pData)
        {
            ProcessLogExtend logProcess = new ProcessLogExtend(ProcessLog, Logger);
            logProcess.LogAddDetail(pLogLevel, pMessage, 0, pData);
        }
        #endregion

        #region Collaborator
        #region Collaborator Accessors
        /// <summary>
        ///  Obtient true si la session est connectée à une base SQL
        ///  <para>Obtient false si la session est terminée (suite à inactivité)</para>
        /// </summary>
        public static bool IsConnected => IsSessionAvailable && (Collaborator_IDA != 0);

        /// <summary>
        /// 
        /// </summary>
        public static bool IsCollaboratorSpecified => (null != Collaborator);

        /// <summary>
        /// Obtient ou définit le collaborator connecté
        /// </summary>
        public static Collaborator Collaborator
        {
            get { return (Collaborator)HttpSessionStateTools.Get(SessionState, "COLLABORATOR"); }
            set
            {
                HttpSessionStateTools.Set(SessionState, "COLLABORATOR", value);
                if (null == value)
                    HttpSessionStateTools.Set(SessionState, "USER", null);
                else
                    HttpSessionStateTools.Set(SessionState, "USER", value.NewUser());
            }
        }

        /// <summary>
        /// Obtient une référence sur l'application
        /// </summary>
        public static AppInstance AppInstance
        {
            get => (AppInstance)HttpContext.Current.Application["APPINSTANCE"];
        }

      

        /// <summary>
        /// 
        /// </summary>
        public static AppSession AppSession
        {
            get => (AppSession)HttpSessionStateTools.Get(SessionState, "AppSession");
        }
        /// <summary>
        /// Définit la Session dans une variable session
        /// </summary>
        public static void SetAppSession()
        {
            if (IsSessionAvailable)
            {
                AppSession appSession = new AppSession(SessionTools.AppInstance)
                {
                    SessionId = SessionTools.SessionID,
                    BrowserInfo = AspTools.GetBrowserInfo(),
                };
                HttpSessionStateTools.Set(SessionState, "AppSession", appSession);
            }
        }

        /// <summary>
        ///  Obtient le user connecté 
        /// </summary>
        public static User User
        {
            get { return (User)HttpSessionStateTools.Get(SessionState, "USER"); }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int Collaborator_IDA
        {
            get
            {
                return (IsCollaboratorSpecified ? Collaborator.Ida : 0);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool Collaborator_ISRDBMSTRACE
        {
            get
            {
                return IsCollaboratorSpecified && Collaborator.IsRDBMSTrace;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_IDENTIFIER
        {
            get
            {
                return (string)(IsCollaboratorSpecified ? Collaborator.Identifier : string.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static Collaborator.AuthenticationTypeEnum Collaborator_AuthenticationType
        {
            get
            {
                return (Collaborator.AuthenticationTypeEnum)(IsCollaboratorSpecified ? Collaborator.AuthenticationType : Collaborator.AuthenticationTypeEnum.NA);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_DISPLAYNAME
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.DisplayName;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_EXTLLINK
        {

            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.ExtlLink;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_ROLE
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified && Collaborator.UserType != null)
                    ret = Collaborator.UserType.ToString();
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static ActorAncestor ActorAncestor
        {
            get
            {
                ActorAncestor ret = null;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.ActorAncestor;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int Collaborator_ENTITY_IDA
        {
            get
            {
                int ret = 0;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.Ida;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool Collaborator_ENTITY_ISENTITY
        {
            get
            {
                bool ret = false;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.IsEntity;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_ENTITY_IDENTIFIER
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.Identifier;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_ENTITY_DISPLAYNAME
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.DisplayName;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_ENTITY_BIC
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.BIC;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static string Collaborator_ENTITY_DESCRIPTION
        {
            get
            {
                string ret = string.Empty;
                if (IsCollaboratorSpecified)
                    ret = Collaborator.Entity.Description;
                return ret;
            }
        }

        //
        /// <summary>
        /// 
        /// </summary>
        public static bool IsSessionSysAdmin
        {
            get
            {
                bool ret = false;
                if (IsCollaboratorSpecified)
                    ret = User.IsSessionSysAdmin;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsSessionSysOper
        {
            get
            {
                bool ret = false;
                if (IsCollaboratorSpecified)
                    ret = User.IsSessionSysOper;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static bool IsSessionGuest
        {
            get
            {
                bool ret = false;
                if (IsCollaboratorSpecified)
                    ret = User.IsSessionGuest;
                return ret;
            }
        }
        #endregion Collaborator Accessors

        #region Collaborator Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultIdentifierEntityOfUser()
        {
            string ret = string.Empty;
            //
            if (Collaborator_ENTITY_ISENTITY && StrFunc.IsFilled(Collaborator_ENTITY_IDENTIFIER))
                ret = Collaborator_ENTITY_IDENTIFIER;
            //
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsForIdXML"></param>
        /// <returns></returns>
        public static string GetDefaultBICEntityOfUser(bool pIsForIdXML)
        {
            string ret = string.Empty;
            //
            if (Collaborator_ENTITY_ISENTITY)
            {
                if (StrFunc.IsFilled(Collaborator_ENTITY_BIC))
                    ret = Collaborator_ENTITY_BIC;
                else
                    ret = Collaborator_ENTITY_IDENTIFIER;
            }
            //
            if (pIsForIdXML)
                ret = XMLTools.GetXmlId(ret);
            //
            return ret;

        }


  
        #endregion Collaborator Methods
        #endregion Collaborator

        #region Class
        /// <summary>
        /// Représente la base de données 
        /// </summary>
        public sealed class Data
        {
            public static string RDBMS
            {
                get { return (string)(HttpSessionStateTools.Get(SessionState, "RDBMS")); }
                set { HttpSessionStateTools.Set(SessionState, "RDBMS", value); }
            }
            public static string Server
            {
                get { return (string)(HttpSessionStateTools.Get(SessionState, "SERVER")); }
                set { HttpSessionStateTools.Set(SessionState, "SERVER", value); }
            }
            public static string Database
            {
                get { return (string)(HttpSessionStateTools.Get(SessionState, "DATABASE")); }
                set { HttpSessionStateTools.Set(SessionState, "DATABASE", value); }
            }
            public static string DatabaseVersionBuild
            {
                get { return (string)(HttpSessionStateTools.Get(SessionState, "DBVERSION")); }
                set { HttpSessionStateTools.Set(SessionState, "DBVERSION", value); }
            }
            public static string DatabaseVersion
            {
                get
                {
                    try
                    {
                        string databaseVersion = DatabaseVersionBuild;
                        string delimStr = ".";
                        char[] delimiter = delimStr.ToCharArray();
                        string[] split = databaseVersion.Split(delimiter);
                        return split[0] + "." + split[1] + "." + split[2];
                    }
                    catch
                    {
                        return "0.0.0";
                    }
                }
            }
            public static string DatabaseNameVersionBuild
            {
                get
                {
                    return Database + " v" + DatabaseVersionBuild;
                }
            }
            public static string DatabaseMajorMinor
            {
                get
                {
                    string ret = "0.0";
                    try
                    {
                        string databaseVersion = DatabaseVersionBuild;
                        string delimStr = ".";
                        char[] delimiter = delimStr.ToCharArray();
                        if (null != databaseVersion)
                        {
                            string[] split = databaseVersion.Split(delimiter);
                            ret = split[0] + "." + split[1];
                        }
                    }
                    catch
                    {
                        return "0.0";
                    }
                    return ret;
                }
            }
            public static int DatabaseVersionBuild_Int
            {
                get
                {
                    try
                    {
                        string databaseVersion = DatabaseVersionBuild;
                        string delimStr = ".";
                        char[] delimiter = delimStr.ToCharArray();
                        string[] split = databaseVersion.Split(delimiter);
                        int major = Convert.ToInt32(split[0]);
                        int minor = Convert.ToInt32(split[1]);
                        int revision = Convert.ToInt32(split[2]);
                        int build = Convert.ToInt32(split[3]);
                        return (major * (1000000000)) + (minor * (1000000)) + (revision * (1000)) + (build);
                    }
                    catch
                    {
                        return 0;
                    }
                }
            }
            public static string DbStatus
            {
                get { return (string)(HttpSessionStateTools.Get(SessionState, "DBSTATUS")); }
                set { HttpSessionStateTools.Set(SessionState, "DBSTATUS", value); }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public sealed class TemporaryDirectory
        {
            #region public Accessors
            /// <summary>
            /// Obtient le chemin virtuel Temporary/{sessionID]/
            /// </summary>
            public static string RelativePath
            {
                get { return @"Temporary/" + SessionID + "/"; }
            }
            /// <summary>
            /// Obtient le chemin virtuel {SpheresWebsite}/Temporary/{sessionID]/ 
            /// <para>{SpheresWebsite} est le chemin d'accès virtuel à l'application</para>
            /// </summary>
            public static string Path
            {
                get { return HttpContext.Current.Request.ApplicationPath + @"/" + RelativePath; }
            }
            /// <summary>
            /// Obtient le chemin physique correspondant à au chemin virtuel Path
            /// <para>Création du répertoire s'il n'existe pas</para>
            /// </summary>
            public static string PathMapped
            {
                get
                {
                    string ret = HttpContext.Current.Server.MapPath(Path);
                    SystemIOTools.CreateDirectory(ret);
                    return ret;
                }
            }
            /// <summary>
            /// Obtient le chemin virtuel Temporary/{sessionID]/Images/
            /// </summary>
            public static string RelativeImagesPath
            {
                get { return RelativePath + @"Images/"; }
            }
            /// <summary>
            /// Obtient le chemin virtuel {SpheresWebsite}/Temporary/{sessionID]/Images/
            /// <para>{SpheresWebsite} est le chemin d'accès virtuel à l'application</para>
            /// </summary>
            public static string ImagesPath
            {
                get { return Path + @"Images/"; }
            }
            /// <summary>
            /// Obtient le chemin physique correspondant au chemin virtuel ImagesPath
            /// <para>Création du répertoire s'il n'existe pas</para>
            /// </summary>
            public static string ImagesPathMapped
            {
                get
                {
                    string ret = HttpContext.Current.Server.MapPath(ImagesPath);
                    SystemIOTools.CreateDirectory(ret);
                    return ret;
                }
            }
            #endregion Accessor

            #region public MapPath
            /// <summary>
            /// Retourne le chemin physique correspondant à {SpheresWebsite}/Temporary/{sessionID]/{pPath} 
            /// </summary>
            /// <param name="pPath">Chemin d'accès virtuel</param>
            /// <returns></returns>
            public static string MapPath(string pPath)
            {

                string ret = HttpContext.Current.Server.MapPath(Path + pPath);
                SystemIOTools.CreateDirectory(ret);
                return ret;
            }
            #endregion public MapPath
        }
        #endregion Class
    }
}
