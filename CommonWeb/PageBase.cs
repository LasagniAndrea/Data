#region Using Directives
using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections;
using System.Linq;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Helpers;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Xml;
using System.Drawing;
using System.Runtime.InteropServices;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using SpheresMenu = EFS.Common.Web.Menu;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;

//using AjaxControlToolkit;
#endregion Using Directives

// EG 20231129 [WI756] Spheres Core : Refactoring Code Analysis with Intellisense
namespace EFS.Common.Web
{
    /// <summary>
    /// Description r�sum�e de PageBase. 
    /// </summary>
    // EG 20180423 Analyse du code Correction [CA1405]
    [ComVisible(false)]
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public class PageBase : Page
    {
        #region Members
        /// <summary>
        /// Identifiant unique de la page
        /// </summary>
        private string _GUID;
        private string _GUIDReferrer;
        /// <summary>
        /// Repr�sente le titre de la page 
        /// </summary>
        private string _pageTitle;
        /// <summary>
        /// Repr�sente l'application
        /// </summary>
        private readonly string _titleSoftware;
        private Cst.CSSModeEnum _cssModeEnum;
        private string _cssColor;
        private int _autoRefresh;
        private bool _abortNoCache;
        private bool _closeAfterAlertImmediate;
        private string _msgForAlertImmediate;
        private ProcessStateTools.StatusEnum _errLevelForAlertImmediate = ProcessStateTools.StatusNoneEnum;
        /// <summary>
        /// Hauteur et largeur de la fen�tre de dialog
        /// </summary>
        // EG 20160404 Migration vs2013
        //private Pair<Pair<Nullable<int>, Nullable<int>>, Pair<Nullable<int>, Nullable<int>>> _sizeForAlertImmediate;
        private Pair<Nullable<int>, Nullable<int>> _sizeHeightForAlertImmediate;
        private Pair<Nullable<int>, Nullable<int>> _sizeWidthForAlertImmediate;
        /// <summary>
        /// Hauteur et largeur maximale de la fen�tre de dialog
        /// </summary>
        private Pair<int, int> _maxSizeForAlertImmediate;

        private string _windowOpen;
        private bool _abortRessource;
        private string _activeElementForced;
        private bool _uniqueButtonSize;
        private bool _setScrollPersistence;
        private HtmlForm _form;
        private readonly string _borderDesign;
        private readonly bool _isDebugClientId;
        private Exception _exceptionLastError;

        /// 20100726 MFThe ScriptManager class could not ensure the right version for the Ajax scripts referenced by the AjaxControlToolkit 4.0
        protected AjaxScriptManager _sm;
        protected bool _isResponseToEnd;

        private HiddenField _hiddenFieldDeFaultControlOnEnter;
        private HiddenField _hiddenLastNoAutoPostbackCtrlChanged;
        private HiddenField _hiddenFieldGUID;
        // EG 20211217 [XXXXX] Maintien de cet �tat (Toggle) avant un post de la page.
        private HiddenField _hiddenFieldToggleStatus;

        private readonly AuditTime _auditTime = null;
        /// <summary>
        ///  Repr�sente la dur�e d'ex�cution limite de la page d�finie dans le fichier de configuration webconfig
        ///  <para>
        ///  Section: HttpRuntime; Propertie:executionTimeout
        ///  </para>
        /// </summary>
        /// FI 20190910 [24914] Add
        private readonly int _defaultScriptTimeout;

        /// <summary>
        /// Script de gestion du dernier focus
        /// </summary>
        protected const string SCRIPT_DOFOCUS = @"window.setTimeout('DoFocus()', 1);function DoFocus(){try {document.getElementById('REQUEST_LASTFOCUS').focus();} catch (ex) {}}";
        #endregion Members

        #region Accessors
        public string VirtualPath(string pPath)
        {
            return HostingEnvironment.ApplicationVirtualPath + "/" + pPath;
        }
        public virtual ContentPlaceHolder MasterPage_ContentPlaceHolder
        {
            get { return null; }
        }
        public virtual string ContentPlaceHolder_ClientID
        {
            get { return null; }
        }
        public virtual string ContentPlaceHolder_UniqueID
        {
            get { return null; }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool CloseAfterAlertImmediate
        {
            get { return _closeAfterAlertImmediate; }
            set { _closeAfterAlertImmediate = value; }
        }

        /// <summary>
        /// Obtient ou D�finit le message qui sera pr�sent� � l'ouverture de la page sur le client
        /// <para>Utilise la m�thode JavaScript.DialogStartUpImmediate</para>
        /// </summary>
        public string MsgForAlertImmediate
        {
            get { return _msgForAlertImmediate; }
            set
            {
                if (StrFunc.IsFilled(value))
                {
                    if (false == StrFunc.ContainsIn(_msgForAlertImmediate, value))
                    {
                        if (StrFunc.IsFilled(_msgForAlertImmediate))
                            _msgForAlertImmediate += Cst.CrLf; ;
                        _msgForAlertImmediate += value;
                    }
                }
            }
        }

        /// <summary>
        /// Obtient ou d�finit le niveau du MsgForAlertImmediate
        /// </summary>
        public ProcessStateTools.StatusEnum ErrLevelForAlertImmediate
        {
            get { return _errLevelForAlertImmediate; }
            set { _errLevelForAlertImmediate = value; }
        }
        /// <summary>
        /// Obtient la hauteur (height,max-height)
        /// et largeur (width, max-width) de la fen�tre de dialog
        /// </summary>
        public Pair<Pair<Nullable<int>, Nullable<int>>, Pair<Nullable<int>, Nullable<int>>> SizeForAlertImmediate
        {
            get { return new Pair<Pair<int?, int?>, Pair<int?, int?>>(_sizeHeightForAlertImmediate, _sizeWidthForAlertImmediate); }
        }
        /// <summary>
        /// Obtient ou d�finit la hauteur (height,max-height)
        /// </summary>
        public Pair<Nullable<int>, Nullable<int>> SizeHeightForAlertImmediate
        {
            get { return _sizeHeightForAlertImmediate; }
            set { _sizeHeightForAlertImmediate = value; }
        }
        /// <summary>
        /// Obtient ou d�finit la largeur (width, max-width)
        /// </summary>
        public Pair<Nullable<int>, Nullable<int>> SizeWidthForAlertImmediate
        {
            get { return _sizeWidthForAlertImmediate; }
            set { _sizeWidthForAlertImmediate = value; }
        }

        /// <summary>
        /// Obtient ou d�finit la hauteur et largeur de la fen�tre de dialog
        /// </summary>
        public Pair<int, int> MaxSizeForAlertImmediate
        {
            get
            {
                Pair<int, int> _pair = _maxSizeForAlertImmediate;
                if (null == _maxSizeForAlertImmediate)
                {
                    _pair.First = 0;
                    _pair.Second = 0;
                }
                return _pair;
            }
            set { _maxSizeForAlertImmediate = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string WindowOpen
        {
            get { return _windowOpen; }
            set
            {
                if (StrFunc.IsFilled(value))
                {
                    if (false == StrFunc.ContainsIn(_windowOpen, value))
                    {
                        if (StrFunc.IsFilled(_windowOpen))
                            _windowOpen += Cst.CrLf; ;
                        _windowOpen += value;
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public Exception ExceptionLastError
        {
            get { return _exceptionLastError; }
            set { _exceptionLastError = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public int AutoRefresh
        {
            get { return _autoRefresh; }
            set { _autoRefresh = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AbortNoCache
        {
            get { return _abortNoCache; }
            set { _abortNoCache = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool AbortRessource
        {
            get { return _abortRessource; }
            set { _abortRessource = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ActiveElementForced
        {
            get { return _activeElementForced; }
            set { _activeElementForced = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UniqueButtonSize
        {
            get { return _uniqueButtonSize; }
            set { _uniqueButtonSize = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual MaintainScrollPageEnum MaintainScrollEnum
        {
            get { return MaintainScrollPageEnum.Automatic; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMaintainScrollManually
        {
            get { return (MaintainScrollPageEnum.Manually == MaintainScrollEnum); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsMaintainScrollAutomatic
        {
            get { return (MaintainScrollPageEnum.Automatic == MaintainScrollEnum); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SetScrollPersistence
        {
            get { return _setScrollPersistence; }
            set { _setScrollPersistence = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CSSColor
        {
            get { return _cssColor; }
            set { _cssColor = value; }
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public Cst.CSSModeEnum CSSModeEnum
        {
            get { return _cssModeEnum; }
            set { _cssModeEnum = value; }
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        public string CSSMode
        {
            get { return _cssModeEnum.ToString(); }
            set { _cssModeEnum = ReflectionTools.ConvertStringToEnum<Cst.CSSModeEnum>(value); }
        }
        /// <summary>
        /// Obtient ou d�finit le titre de la page
        /// <para>Ce titre permet d'alimenter l'attribut Title de du tag Head de la page HTML </para>
        /// </summary>
        public string PageTitle
        {
            get { return _pageTitle; }
            set
            {
                _pageTitle = value;
                SetTitle();
            }
        }

        /// <summary>
        /// Obtient la string PageTitle + "-" +_titleSoftware 
        /// </summary>
        public string PageFullTitle
        {
            get
            {
                string ret = _titleSoftware;
                if (StrFunc.IsFilled(PageTitle))
                    ret = PageTitle + " - " + ret;
                string customizedPrefix = SystemSettings.GetAppSettings("Spheres_PageTitlePrefix", string.Empty);
                return customizedPrefix + ret;
            }
        }

        

        /// <summary>
        /// Obtient un identificateur unique de la page
        /// </summary>
        public string GUID
        {
            get
            {
                string ret = _GUID;
                // FI 20200217 [XXXXX] Add NotSupportedException pour informer le d�veloppeur 
                if (StrFunc.IsEmpty(_GUID))
                {
                    if (false == IsPostBack)
                        throw new NotSupportedException(@"For using GUID property,""PageBase.OnInit"" must be called before.");
                    else
                        throw new NotSupportedException(@"For using GUID property, Form must contains _GUID hidden Control. Please Call AddInputHiddenGUID Method.");
                }
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string GUIDReferrer
        {
            get { return _GUIDReferrer; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsShowAdminTools
        {
            get { return SessionTools.IsSessionSysAdmin && BoolFunc.IsTrue(Request.QueryString["isShowAdminTools"]); }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsDebugJavaScript
        {
            get { return BoolFunc.IsTrue(Request.QueryString["isDebugJavaScript"]); }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsDebugDesign
        {
            get { return BoolFunc.IsTrue(_borderDesign); }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsDebugClientId
        {
            get { return _isDebugClientId; }
        }

        /// <summary>
        ///  Obtient true si sessionAdmin et que l'URL pr�sente isDebug=true 
        /// </summary>
        public virtual bool IsDebug
        {
            //PL 20100709 Add #IF DEBUG
            get
            {
                bool ret = SessionTools.IsSessionSysAdmin;
#if DEBUG
                ret = true;
#endif
                return ret && BoolFunc.IsTrue(Request.QueryString["isDebug"]);
            }
        }

        /// <summary>
        ///  Obtient true si (sessionAdmin ou isTrace=true dans la config) et que l'URL pr�sente isTrace=true 
        /// </summary>
        /// FI 20161114 [RATP] Modify
        /// FI 20190411 [XXXXX] call isTraceAvailable
        public virtual bool IsTrace
        {
            get
            {
                return (IsTraceAvailable && BoolFunc.IsTrue(Request.QueryString["isTrace"]));
            }
        }

        /// <summary>
        ///  Obtient true si (sessionAdmin ou isTraceTime=true dans la config) et que l'URL pr�sente isTraceTime=true 
        /// </summary>
        /// FI 20161114 [RATP] Modify
        /// FI 20190411 [XXXXX] call isTraceTimeAvailable
        public virtual bool IsTraceTime
        {
            get
            {
                return (IsTraceTimeAvailable && BoolFunc.IsTrue(Request.QueryString["isTraceTime"]));
            }
        }

        /// <summary>
        /// Obtient true si la trace "isTrace" est un feature disponible
        /// </summary>
        /// FI 20190411 [XXXXX] Add
        public static Boolean IsTraceAvailable
        {
            get
            {
                return SessionTools.IsSessionSysAdmin ||
                    BoolFunc.IsTrue(ConfigurationManager.AppSettings["isTrace"]);
            }
        }

        /// <summary>
        /// Obtient true si la trace "isTraceTime" est un feature disponible
        /// </summary>
        /// FI 20190411 [XXXXX] Add
        public static Boolean IsTraceTimeAvailable
        {
            get
            {
                return SessionTools.IsSessionSysAdmin ||
                    BoolFunc.IsTrue(ConfigurationManager.AppSettings["isTraceTime"]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool IsFlushRDBMSCache
        {
            get
            {
                Boolean ret = this.IsTraceTime && (Request.QueryString["isTraceTime"].ToString() == "2");
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string BorderDesign
        {
            get { return _borderDesign; }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsBodyColor
        {
            get { return !SessionTools.IsBackgroundWhite; }
        }

        /// <summary>
        /// Retoune La table principale (1er control enfant de Form)
        /// <seealso cref="TableForm"/>
        /// </summary>
        protected Table TableForm
        {
            get
            {
                int i = (null != _sm) ? 1 : 0;
                return ((Table)_form.Controls[i]);
            }
        }

        /// <summary>
        /// Retoune La 1er cellule de la table principale 
        /// <seealso cref="TableForm"/>
        /// </summary>
        protected TableCell CellForm
        {
            get
            {
                return TableForm.Rows[0].Cells[0];
            }
        }

        /// <summary>
        /// Obtient ou d�finit l'objet Form
        /// <remarks>
        ///<para>
        ///  Attention cette property est une red�finition de la property Form native � System.Web.UI.Page
        /// </para>
        /// <para>
        /// Lorsque la form est construite via design il faut affecter cette propery avec la form d�finie en design
        /// </para>
        /// </remarks>
        /// </summary>
        protected new HtmlForm Form
        {
            get
            {
                return _form;
            }
            set
            {
                _form = value;
            }
        }

        /// EG 20220623 [XXXXX] Impl�mentation Authentification via Shibboleth SP et IdP
        protected bool IsFedAuthForm
        {
            get
            {
                return (null != _form) && (_form.Name.StartsWith("frmFedAuth"));
            }
        }


        /// <summary>
        /// Obtient le contr�le qui d�signe le control qui est activ� lorsque l'utilisateur appuie sur la touche {Enter} 
        /// </summary>
        protected HiddenField HiddenFieldDeFaultControlOnEnter
        {
            get
            {
                return _hiddenFieldDeFaultControlOnEnter;
            }
        }

        /// <summary>
        /// Obtient le contr�le __idLastNoAutoPostbackCtrlChanged
        /// <para>Ce contr�le est utilis� pour v�rifier si un contr�le non autopostback a �t� modifi� (old != New)</para>
        /// </summary>
        // FI 20200128 [25182] Add
        public HiddenField HiddenLastNoAutoPostbackCtrlChanged
        {
            get
            {
                return _hiddenLastNoAutoPostbackCtrlChanged;
            }
        }

        /// <summary>
        /// Obtient le contr�le qui indique le GUID de la page
        /// </summary>
        // FI 20200217 [XXXXX] Add
        public HiddenField HiddenFieldGUID
        {
            get
            {
                return _hiddenFieldGUID;
            }
        }

        // EG 20211217 [XXXXX] Maintien de cet �tat (Toggle) avant un post de la page.
        public HiddenField HiddenFieldToggleStatus
        {
            get
            {
                return _hiddenFieldToggleStatus;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual bool ContainsHelpUrlPath
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ScriptManager ScriptManager
        {
            get
            {
                ScriptManager ret;
                
                ret = ScriptManager.GetCurrent(this);
                if ((null == ret) && (null != _sm))
                    ret = _sm;
                
                return ret;
            }
        }

        /// <summary>
        ///  Se Produit lorsqu'une erreur est rencontr�e lors d'un appel AJAX au server et uniquement lors d'un appel via la librairie Microsoft Ajax Library.
        ///  Ecriture dans le log de l'exception rencontr�e
        /// </summary>
        /// <remarks>La librairie Microsoft Ajax Library est utilis�e avec UpdatePanel d�ASP.NET</remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20231004 [WI720] Add Method 
        public virtual void OnAsyncPostBackError(object sender, AsyncPostBackErrorEventArgs e)
        {
            ScriptManager.AsyncPostBackErrorMessage = e.Exception.Message;
            WriteLogException(e.Exception);
        }


        /// <summary>
        /// 
        /// </summary>
        protected virtual bool IsSupportsPartialRendering
        {
            get
            {
                return (false);
            }
        }

   

       

        

        /// <summary>
        /// 
        /// </summary>
        public bool IsResponseToEnd
        {
            get { return _isResponseToEnd; }
            set { _isResponseToEnd = value; }
        }


        /// <summary>
        /// 
        /// </summary>
        /// FI 20140519 add property 
        public string PARAM_EVENTTARGET
        {
            get { return Request.Params["__EVENTTARGET"]; }
        }

        /// <summary>
        /// 
        /// </summary>
        // FI 20140519 add property
        public string PARAM_EVENTARGUMENT
        {
            get { return Request.Params["__EVENTARGUMENT"]; }
        }

        /// <summary>
        /// Obtient ou d�finie le g�n�rateur de journal des actions utilisateurs 
        /// <para>Lorsque cet property est d�finie, Spheres� alimente le log des actions utilisateurs</para>
        /// </summary>
        ///FI 20141021 [20350] Add
        public RequestTrackBuilderBase RequestTrackBuilder
        {
            get;
            set;
        }

        /// <summary>
        ///  Obtient la valeur de la propr�t� ScriptTimeOut pr�sente dansle webCustom.Config
        ///  <para>Obtient -1 s'il nexiste pas de cl� ScriptTimeOut dans le fichier webCustom.Config</para>
        ///  <para>Permet d'augmenter la dur�e limite d'ex�cution de la page web</para>
        /// </summary>
        /// FI 20190910 [24914] Add
        private static int ScriptTimeOut
        {
            get
            {
                return Convert.ToInt32(SystemSettings.GetAppSettings("ScriptTimeout", typeof(System.Int32), -1));
            }
        }




        #endregion Accessors

        #region Constructor
        public PageBase()
        {
            _isDebugClientId = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Spheres_DebugClientId"));
            _borderDesign = SystemSettings.GetAppSettings("Spheres_DebugDesign");
            _abortRessource = false;
            _abortNoCache = false;
            _autoRefresh = 0;
            _setScrollPersistence = true;
            _msgForAlertImmediate = string.Empty;
            //
            if ((this.ToString() == "ASP.default_aspx") || (this.ToString() == "ASP.defaultWest_aspx"))
                _titleSoftware = Software.TitleForDefaultAspx;
            else
                _titleSoftware = Software.Title;
            _pageTitle = string.Empty;
            //
            _isResponseToEnd = false;
            _auditTime = new AuditTime();

            // FI 20190910 [24914] Initialisation de _defaultScriptTimeout
            _defaultScriptTimeout = Server.ScriptTimeout;
        }
        #endregion

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreInit(EventArgs e)
        {
            //
            base.OnPreInit(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void OnClose(object sender, System.EventArgs e)
        {
            ClosePage();
        }

        /// <summary>
        /// Remplace l'appel Page.Response.Redirect(@"Welcome.aspx")
        /// </summary>
        public void ClosePage()
        {
            JavaScript.ExecuteImmediate(this, "ClosePage();", false, true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20220623 [XXXXX] Impl�mentation Authentification via Shibboleth SP et IdP
        protected override void OnInit(System.EventArgs e)
        {
            // Si la forme n'est pas une forme de gestion de Shibboleth (FedAuthLogin, FedAuthLogout ou FedOutTimeout)
            // et que l'on est en authetification Shibbometh mais que la connectionString est vide
            // alors cela est li� � une fin de session, on redirige vers FedAuthLogin.aspx
            if (false == IsFedAuthForm)
            {
                if (SessionTools.IsShibbolethAuthenticationType && String.IsNullOrEmpty(SessionTools.ConnectionString))
                    Response.Redirect("FedAuthLogin.aspx");
            }

            CSSModeEnum = SessionTools.CSSMode;

            // FI 20200130 [XXXXX] Information utilis�e pour notamment trapper les double-post
            // FI 20200401 [XXXXX] Ajout de l'information value
            //Debug.WriteLine(StrFunc.AppendFormat("PageBase.OnInit started at {0}, EventTarget: {1} (Value: {2}), EventArgument: {3}",
            //DtFunc.DateTimeToString(DateTime.Now, DtFunc.FmtISOTime), 
            //__EVENTTARGET, 
            //StrFunc.IsFilled(__EVENTTARGET) && (null != Request.Form[__EVENTTARGET]) ? Request.Form[__EVENTTARGET] :string.Empty,
            //__EVENTARGUMENT));
            //20091106 FI On effectue base.OnInit(e); avant tout 
            base.OnInit(e);

            // FI 20190910 [24914] Alimentation de Server.ScriptTimeout
            if (ScriptTimeOut > 0 && ScriptTimeOut > Server.ScriptTimeout)
            {
                // Augmentation par programmation de la dur�e limite d'ex�cution de la page 
                // Note
                // If you set the debug attribute of the httpRuntime element to true in the Web.config file, the value of ScriptTimeout will be ignored.
                Server.ScriptTimeout = Convert.ToInt32(ScriptTimeOut);
            }

            _sizeHeightForAlertImmediate = new Pair<int?, int?>(null, 400);
            _sizeWidthForAlertImmediate = new Pair<int?, int?>(null, 600);

            InitializeScriptManager();
            // FI 20210218 [XXXXX] Add (pour usage c�t� JS)
            ClientScript.RegisterHiddenField("__CULTURE", Thread.CurrentThread.CurrentUICulture.Name);

            // FI 20200217 [XXXXX] Call SetGUID(); et SetGUIDReferrer();
            SetGUID();
            SetGUIDReferrer();

            #region Checking
            //Check permission IP Address, Secure connection, Authentification, ... (GlopPL � faire �ventuellement)
            // Pas de contr�le si Mode Shibboleth
            if (HttpContext.Current.User.Identity.AuthenticationType != "Shibboleth")
                CheckSessionState();

            // FI 20191209 [XXXXX] Appel � CheckLoginExpire uniquement si !IsPostBack 
            // Economies en requ�tes SQL pour �viter appel � OTCmlHelper.GetDateSys(CS) � chaque post d'une page
            if (!IsPostBack)
                CheckLoginExpire();
            //
            if (!IsPostBack)
                CheckURL();
            #endregion

            #region Cache
            if (!_abortNoCache)
                InactiveCacheClient();
            #endregion Cache

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200818 [XXXXX] Rename SetAutoRefresh
        protected override void OnLoad(System.EventArgs e)
        {
            AddAuditTimeStep("PageBase.OnLoad", true);

            //Use eventually for debug
            //string WindowID = "Request" + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff");
            //Request.SaveAs(SessionTools.TemporaryDirectory.MapPath(@"Request") + @"\" + WindowID,true);        

            SetTitle();

            if (!_abortRessource)
                ApplyResourcesForControls(Controls.GetEnumerator());

            base.OnLoad(e);

            // FI 20200731 [XXXXX] Call SetAutoRefresh
            SetAutoRefresh();
            
            // 20081020 RD - Ticket 16331
            // Cette instruction (Response.End())doit �tre en dehors d'un try catch
            //
            if (_isResponseToEnd)
                Response.End();
            //
            AddAuditTimeStep("PageBase.OnLoad", false);
        }
        /// <summary>
        // Mise � jour du CSSMode post connexion
        /// </summary>
        // EG 20200826 New
        protected void SetCSSMode()
        {
            AspTools.ReadSQLCookie(Cst.SQLCookieElement.CSSMode.ToString(), out string ret);
            CSSMode = ret;
            SessionTools.CSSMode = (Cst.CSSModeEnum)Enum.Parse(typeof(Cst.CSSModeEnum), CSSMode, true);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        //protected override void OnPreRender(System.EventArgs e)
        //{
        //    AddAuditTimeStep("PageBase.OnPreRender", true);
        //    //
        //    ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));
        //    //__ACTIVEELEMENT
        //    string activeElement = Request.Params["__ACTIVEELEMENT"];
        //    //20090622 PL Focus: gestion dynamique
        //    if (StrFunc.IsFilled(ActiveElementForced))
        //        activeElement = ActiveElementForced;
        //    ClientScript.RegisterHiddenField("__ACTIVEELEMENT", activeElement);
        //    //
        //    //FI 20120902 Mise en commentaire car ajout� ds PageBase.js
        //    //JavaScript.SaveActiveElement(this);
        //    //FI 20120902 Mise en commentaire car ajout� ds PageBase.js
        //    //JavaScript.ClosePage(this);
        //    //
        //    //__SCROLLPOS
        //    if (isMaintainScrollManually)
        //    {
        //        ClientScript.RegisterHiddenField("__SCROLLPOS", Request.Params["__SCROLLPOS"]);
        //        JavaScript.SaveScrollPos(this, "BodyID");
        //    }
        //    //
        //    if (IsPostBack)
        //    {
        //        JavaScript.PostBackOnLoad(this, "BodyID", isMaintainScrollManually && SetScrollPersistence, WindowOpen);
        //        try
        //        {
        //            if (StrFunc.IsFilled(activeElement))
        //                SetFocus(activeElement);
        //        }
        //        catch
        //        {
        //            //Catch afin de palier au probleme suivant qui se proquit parfois sur la page de connection ??
        //            //Message	"Page cannot be null. Please ensure that this operation is being performed in the context of an ASP.NET request."	string
        //            //Il est de toute fa�on dommage de planter pour des raisoins de focus, le try catch() semble ici judicieux
        //        }
        //    }
        //    else if (StrFunc.IsFilled(WindowOpen))
        //        JavaScript.FirstPostOnLoad(this, "BodyID", WindowOpen);
        //    //
        //    //if (ArrFunc.IsFilled(_auditTime.step))
        //    //{
        //    //    _auditTime.WriteDebug();
        //    //    //
        //    //    string WindowID = FileTools.GetUniqueName("AuditTime", this.GetType().Name);
        //    //    string write_File = SessionTools.TemporaryDirectory.MapPath("AuditTime") + @"\" + WindowID + ".xml";
        //    //    string open_File = SessionTools.TemporaryDirectory.Path + "AuditTime" + @"/" + WindowID + ".xml";
        //    //    //
        //    //    _auditTime.WriteFile(write_File);
        //    //    //
        //    //    AddScriptWinDowOpenFile(WindowID, open_File, null);
        //    //}
        //    //
        //    AddAuditTimeStep("PageBase.OnPreRender", false);
        //    //
        //    base.OnPreRender(e);
        //}
        protected override void OnPreRender(System.EventArgs e)
        {
            AddAuditTimeStep("PageBase.OnPreRender", true);

            //__ACTIVEELEMENT
            string activeElement = Request.Params["__ACTIVEELEMENT"];

            //20090622 PL Focus: gestion dynamique
            if (StrFunc.IsFilled(ActiveElementForced))
                activeElement = ActiveElementForced;
            ClientScript.RegisterHiddenField("__ACTIVEELEMENT", activeElement);

            PageRender(activeElement);

            //if (ArrFunc.IsFilled(_auditTime.step))
            //{
            //    _auditTime.WriteDebug();
            //    //
            //    string WindowID = FileTools.GetUniqueName("AuditTime", this.GetType().Name);
            //    string write_File = SessionTools.TemporaryDirectory.MapPath("AuditTime") + @"\" + WindowID + ".xml";
            //    string open_File = SessionTools.TemporaryDirectory.Path + "AuditTime" + @"/" + WindowID + ".xml";
            //    //
            //    _auditTime.WriteFile(write_File);
            //    //
            //    AddScriptWinDowOpenFile(WindowID, open_File, null);
            //}

            // FI 20200128 [25182] Post publication HiddenLastNoAutoPostbackCtrlChanged doit �tre effac�
            HiddenField hiddenField = HiddenLastNoAutoPostbackCtrlChanged;
            if (null != hiddenField)
                hiddenField.Value = string.Empty;

            AddAuditTimeStep("PageBase.OnPreRender", false);


            base.OnPreRender(e);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20140519 [19923] add override method
        protected override void OnPreRenderComplete(EventArgs e)
        {
            RequestTrackGen();
            base.OnPreRenderComplete(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected virtual void DialogImmediate()
        {
            //Deplacer de OnPreRender vers Render
            if (StrFunc.IsFilled(MsgForAlertImmediate))
                JavaScript.DialogStartUpImmediate(this, MsgForAlertImmediate, CloseAfterAlertImmediate, ErrLevelForAlertImmediate, SizeForAlertImmediate);
        }

        protected virtual void PageRender(HtmlTextWriter writer)
        {
            AddAuditTimeStep("PageBase.Render", true);
            SetStyleSheet();
            DialogImmediate();
            AddAuditTimeStep("PageBase.Render", false);
            WriteAuditTime();
        }
        // EG 20210222 [XXXXX] SaveScrollPos dans PageBase.js
        // EG 20210224 [XXXXX] Minification PageBase.js
        protected virtual void PageRender(string pActiveElement)
        {
            if (null != ScriptManager)
                ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.min.js"));

            //__SCROLLPOS
            if (IsMaintainScrollManually)
            {
                ClientScript.RegisterHiddenField("__SCROLLPOS", Request.Params["__SCROLLPOS"]);
                JavaScript.CallFunction(this, "document.body.onscroll = SaveScrollPos;");
            }
            
            if (IsPostBack)
            {
                JavaScript.PostBackOnLoad(this, IsMaintainScrollManually && SetScrollPersistence, WindowOpen);
                try
                {
                    if (StrFunc.IsFilled(pActiveElement))
                        SetFocus(pActiveElement);
                }
                catch
                {
                    //Catch afin de palier au probleme suivant qui se proquit parfois sur la page de connection ??
                    //Message	"Page cannot be null. Please ensure that this operation is being performed in the context of an ASP.NET request."	string
                    //Il est de toute fa�on dommage de planter pour des raisoins de focus, le try catch() semble ici judicieux
                }
            }
            else if (StrFunc.IsFilled(WindowOpen))
                JavaScript.FirstPostOnLoad(this, WindowOpen);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            PageRender(writer);
            base.Render(writer);
        }




        /// <summary>
        /// 
        /// </summary>
        /// 20100901 EG Inscription jQuery engine
        /// 20100901 EG Inscription et initialisation jQuery UI (Datepicker, timepiker, toggle)
        /// EG 20170918 [23342] Add WriteInitialisationScripts for DateTimeOffsetPicker and TimeOffsetPicker
        // EG 20210224 [XXXXX] suppresion PageBase.js d�ja appel� dans Render de PageBase
        // EG 20210222 [XXXXX] Suppression Javascript.ClosePage
        // EG 20210505 [25700] FreezeGrid implementation 
        // EG 20240619 [WI945] Security : Update outdated components (QTip2)
        protected override void CreateChildControls()
        {
            //JavaScript.ClosePage(this);
            JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);

            JQuery.UI.WriteInitialisationScripts(this, new JQuery.DatePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.DateTimePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.DateTimeOffsetPicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.TimePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.TimeOffsetPicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Toggle());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Dialog());
            JQuery.UI.WriteInitialisationFreezeGrid(this);
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.QTip2());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Confirm());
            JQuery.UI.WriteInitialisationScripts2(this, new JQuery.Dialog());
            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));
           
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUnload(EventArgs e)
        {
            // FI 20190910 [24914] Mise en place de la valeur par d�faut
            Server.ScriptTimeout = _defaultScriptTimeout;
            base.OnUnload(e);
        }




        /// <summary>
        /// Ins�re l'exception dans les outils de trace (SYSTEM_L, Fichiers Errors, mail etc)
        /// </summary>
        /// <param name="pEx"></param>
        public void WriteLogException(Exception pEx)
        {
            // FI 20210302 [XXXXX] call AspTools.WriteLogException
            AspTools.WriteLogException(this, pEx);
        }
    

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnError(EventArgs e)
        {
            try
            {
                ExceptionLastError = Server.GetLastError();
                if (null != ExceptionLastError)
                {
                    if (ExceptionLastError.GetBaseException() is HttpRequestValidationException)
                    {
                        System.Diagnostics.Debug.Assert(false);
                        Response.Write(ExceptionLastError.ToString());
                        Response.StatusCode = 200;
                        Response.End();
                    }
                    WriteLogException(ExceptionLastError);
                }

                // RD 20200828 [25392]
                if (Context.Handler is IRequiresSessionState || Context.Handler is IReadOnlySessionState)
                    Session["currentError"] = ExceptionLastError;
            }
            catch
            { }
            base.OnError(e);
        }

        /*
         * FI 20200217 [XXXXX] _GUID et _GUIDReferrer ne sont plus stock� dans le ViewState 
         * (Usage de champ cach� __GUID pour pouvoir acc�der au _GUID d�s le on init d'une page)
        /// <summary>
        /// 
        /// </summary>
        /// <param name="savedState"></param>
        protected override void LoadViewState(object savedState)
        {
            object[] viewState = (object[])savedState;
            _GUID = (string)viewState[0];
            _GUIDReferrer = (string)viewState[1];
        }
        */

        /*
         * FI 20200217 [XXXXX] _GUID et _GUIDReferrer ne sont plus stock� dans le ViewState 
         * (Usage de champ cach� __GUID pour pouvoir acc�der au _GUID d�s le on init d'une page)
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override object SaveViewState()
        {

            if (HttpContext.Current == null)
                return null;
            //
            object[] viewState = new object[2];
            viewState[0] = _GUID;
            viewState[1] = _GUIDReferrer;
            return viewState;

        }
         */

        /// <summary>
        /// 
        /// </summary>
        // EG 20210212 [25661] New Protection CSRF(Cross-Site Request Forgery)
        protected virtual void GenerateHtmlForm()
        {
            int border = Convert.ToInt32(BorderDesign);
            // Add TblMain and RowMain and CellMain
            Table table = new Table
            {
                ID = "tblForm",
                BorderColor = Color.Blue,
                CellPadding = 0,
                CellSpacing = 0,
                Width = Unit.Percentage(100)
            };

            if (0 == border)
                table.BorderStyle = BorderStyle.None;
            else
                table.BorderWidth = border;

            TableRow tr = new TableRow
            {
                ID = "rowForm"
            };
            TableCell td = new TableCell
            {
                ID = "cellForm"
            };
            tr.Cells.Add(td);
            table.Rows.Add(tr);
            //
            _form = new HtmlForm();
            _form.Controls.Add(table);
            //Si ScriptManager cr�� par pageBase (Voir onInit) => ScriptManager est le 1er contr�le de la form
            if (null != _sm)
                _form.Controls.AddAt(0, _sm);
            //
            // EG 20210212 [25661]
            AntiForgeryControl();
            AddInputHiddenAutoPostback();

            // FI 20200217 [XXXXX] Add Call AddInputHidenGUID
            AddInputHiddenGUID();

            // EG 20211217 [XXXXX] Maintien de cet �tat (Toggle) avant un post de la page.
            AddInputHiddenToggleStatus();

            //20090210 FI [16483] Add Panel for modal popup
            // EG 201501001 Delete AddPanelModalPopup
            //AddPanelModalPopup();
            //
            //MaintainScrollPositionOnPostBack est positionn� ici
            //car Il faut absolument une form pour utiliser la property MaintainScrollPositionOnPostBack
            MaintainScrollPositionOnPostBack = IsMaintainScrollAutomatic;
            //
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void PageConstruction()
        {

        }

        #region public virtual GetCompleteHelpUrl
        public virtual string GetCompleteHelpUrl(string pUrl)
        {
            return string.Empty;
        }
        public virtual string GetCompleteHelpUrl(string pSchema, string pElement)
        {
            return string.Empty;
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pTitle"></param>
        /// <param name="pUrl"></param>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compl�ments
        // EG 20240619 [WI945] Security : Update outdated components (JQuery, JQueryUI, BlockUI, Cookie & QTip2)
        public virtual void SetQtipHelpOnLine(WebControl pControl, string pTitle, string pUrl)
        {
            pControl.Attributes.Add("title", pTitle);
            pControl.Attributes.Add("qtip-alt", @"<iframe src=""" + HttpContext.Current.Request.ApplicationPath + "/" + pUrl + @"""/>");
            pControl.Attributes.Add("qtip-style", "qtip-green");
            pControl.Attributes.Add("qtip-button", "X");
        }

        /// <summary>
        /// Ajout HtmlInputHidden qui refrence la fonction javascript:_doPostBack
        /// Force la creation du script __doPostBack() 
        /// </summary>
        /// <remarks>
        /// 20090127 FI Ticket 16460
        /// </remarks> 
        protected void AddInputHiddenAutoPostback()
        {

            //Force creation of script __doPostBack()
            HtmlInputHidden hdn = new HtmlInputHidden
            {
                ID = "__AUTOPOSTBACK"
            };
            hdn.Attributes.Add("onclick", "javascript:" + Page.ClientScript.GetPostBackEventReference(this, ""));
            if (null == Form)
                throw new MemberAccessException("Form is null");
            //
            Form.Controls.Add(hdn);

        }

        /// <summary>
        /// Pr�vention des attaques par falsification de requ�te intersites (Cross Site Request Forgery : CSRF ou XSRF)
        /// Cr�ation d'un jeton anti-contrefa�on
        /// Contr�le � chaque postback
        /// 
        /// Erreur  si :
        /// - Le jeton de cookie HTTP qui accompagne une demande valide est manquant
        /// - Le jeton de formulaire est manquant.
        /// - La valeur de jeton du formulaire ne correspond pas � la valeur de jeton du cookie 
        /// 
        /// Remarque sur les jetons : 
        /// Les jetons de formulaire et de cookie sont corr�l�s et d�crypt�s
        /// Une partie gauche de chaque jeton constitue le jeton de s�curit�. 
        /// C'est l'�quivalence de ce jeton de s�curit� qui est contr�l�e.
        /// 
        /// EXEMPLE:
        /// formToken = i411mJIr0mZKrk17g4Hf-0_G6aXOJLkzzGfd5yn2mVsTqj-35j_n0YUUCzFRXoFet3BXUVpBicpL3p-AqPPA3XEXEtykt4X-_MbRIxLQH6M1
        ///    apr�s d�s�rialisation = 01-1A-CF-C9-ED-F1-3E-1E-7D-C9-9E-BE-90-2E-22-91-36-01
        /// cookieToken = Aq81hoVCPIpq3Q6xjBi0EFKKwSFwnKROgS7tyXF393eAN8rdMNZwkVkEgjQokKviKLVST1iWdgDxBt-g3FIughAsczUO7tyWhtz3fs88xMM1
        ///    apr�s d�s�rialisation = 01-1A-CF-C9-ED-F1-3E-1E-7D-C9-9E-BE-90-2E-22-91-36-00-00-00-00
        /// </summary>
        /// EG 20210212 [25661] New Protection CSRF(Cross-Site Request Forgery)
        /// EG 20210216 [25661] Upd Grstion Erreur sur AntiForgery.Validate sur fen�tre login
        ///                     -> d�connexion  alors que d'autres onglets sont ouverts sur l'application
        /// EG 20210217 [25661] CSRF Correctif Erreur sur Tracker                    
        /// EG 20210414 [25661] CSRF Correctif Erreur sur Sommaire           
        /// EG 20210908 [XXXXX] CSRF ajout de la forme Login du portail sur l'exception HttpAntiForgeryException
        /// EG 20220623 [XXXXX] Impl�mentation Authentification via Shibboleth SP et IdP
        protected void AntiForgeryControl()
        {
           
            if (null != Form)  // La forme doit exister
            {

                // G�n�ration automatique du jeton Anti-contrefa�on
                // 
                // Retourne une cha�ne qui contient la valeur de jeton chiffr�e dans un champ HTML masqu�. 
                Form.Controls.Add(new LiteralControl(AntiForgery.GetHtml().ToHtmlString()));
                if (IsPostBack && SessionTools.IsConnected)
                {
                    // Contr�le et valide le fait que les donn�es d'entr�e d'un champ de formulaire HTML proviennent de l'utilisateur qui a envoy� les donn�es.
                    try
                    {
                        AntiForgery.Validate();
                    }
                    catch (Exception ex)
                    {
                        if (ex is System.Web.Mvc.HttpAntiForgeryException)
                        {
                            if (false == 
                                ((Request.RequestContext.HttpContext.User != null) && 
                                Request.RequestContext.HttpContext.User.Identity.IsAuthenticated &&
                                "frmLogin|frmTracker|Sommaire|portalLogin|FedAuthLogin|FedAuthLogout|FedAuthTimeout".Contains(Form.Name)))
                                throw ex;
                        }
                    }
                }
            }
#if DEBUG
            if (null == Form)
                throw new MemberAccessException("Form is null can not add AntiForgeryToken");
#endif
        }
        /// <summary>
        /// Ajout du control id="__idDefaultControlOnEnter"
        /// <para>
        /// Ce dernier d�signe le control qui est activ� lorsque l'utilisateur appuie sur la touche {Enter} 
        /// </para>
        /// <para>Voir la function CheckKey dans pagebase.js</para>
        /// </summary>
        public void AddInputHiddenDeFaultControlOnEnter()
        {
            if (null == HiddenFieldDeFaultControlOnEnter)
            {
                _hiddenFieldDeFaultControlOnEnter = new HiddenField
                {
                    ID = "__idDefaultControlOnEnter"
                };
                if (null == Form)
                    throw new MemberAccessException("Form is null");
                Form.Controls.Add(_hiddenFieldDeFaultControlOnEnter);
            }
        }

        /// <summary>
        /// Ajoute le contr�le HiddenField id="__idLastNoAutoPostbackCtrlChanged"
        /// <para>
        /// Ce contr�le est utilis� pour v�rifier si un contr�le non autopostback a �t� modifi� (old != New) lors de la publication de la page
        /// </para>
        /// <para>Voir la function la m�thode SetNoAutoPostbackCtrlChanged dans pagebase.js</para>
        /// </summary>
        /// FI 20200128 [25182] Add
        protected void AddInputHiddenLastNoAutoPostbackCtrlChanged()
        {
            if (null == Form)
                throw new MemberAccessException("Form is null");

            if (null == _hiddenLastNoAutoPostbackCtrlChanged)
            {
                _hiddenLastNoAutoPostbackCtrlChanged = new HiddenField()
                {
                    ID = "__idLastNoAutoPostbackCtrlChanged",
                    Value = String.Empty
                };
                Form.Controls.Add(_hiddenLastNoAutoPostbackCtrlChanged);
            }

        }

        /// <summary>
        /// Ajoute le contr�le cach� __GUID dans la form et l'alimente avec GUID 
        /// </summary>
        /// FI 20200217 [XXXXX] Add Method
        protected void AddInputHiddenGUID()
        {
            if (null == _hiddenFieldGUID)
            {
                _hiddenFieldGUID = new HiddenField
                {
                    ID = "__GUID"
                };
                if (null == Form)
                    throw new MemberAccessException("Form is null");
                Form.Controls.Add(_hiddenFieldGUID);
            }
            // Initialisation de la valeur lors de (false == IsPostBack)
            if (false == IsPostBack)
                _hiddenFieldGUID.Value = this.GUID;
        }

        // EG 20211217 [XXXXX] Maintien de cet �tat (Toggle) avant un post de la page.
        protected void AddInputHiddenToggleStatus()
        {
            if (null == _hiddenFieldToggleStatus)
            {
                _hiddenFieldToggleStatus = new HiddenField
                {
                    ID = "__TOGGLESTATUS"
                };
                if (null == Form)
                    throw new MemberAccessException("Form is null");
                Form.Controls.Add(_hiddenFieldToggleStatus);
            }
            // Initialisation de la valeur lors de (false == IsPostBack)
            if (false == IsPostBack)
                _hiddenFieldToggleStatus.Value = string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20200217 [XXXXX] Private method
        private void SetGUIDReferrer()
        {
            string GUID = string.Empty;


            string urlReferrer = Convert.ToString(Request.UrlReferrer);
            if (StrFunc.IsFilled(urlReferrer))
            {
                string[] referrer = urlReferrer.Split('?');
                if (ArrFunc.IsFilled(referrer) && (2 == referrer.Length))
                {
                    string[] items = referrer[1].Split('&');
                    if (ArrFunc.IsFilled(items))
                    {
                        foreach (string item in items)
                        {
                            if (item.StartsWith("GUID="))
                            {
                                GUID = item.Replace("GUID=", string.Empty);
                                break;
                            }
                        }
                    }
                }
            }
            // FI 20200217 [XXXXX] Alimentation de _GUIDReferrer
            _GUIDReferrer = GUID;
        }


        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compl�ments
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        private void SetStyleSheet()
        {

            string cssMode = this.CSSMode;
            AspTools.CheckCssMode(ref cssMode);
            HtmlLink linkCss = (HtmlLink)FindControl("linkCss");
            if (linkCss != null)
                linkCss.Href = String.Format(@"~/Includes/EFSTheme-{0}.min.css", cssMode);

            //
            //20081230 PL
            HtmlGenericControl body = (HtmlGenericControl)FindControl("BodyID");
            if (body != null)
            {
                body.Attributes["class"] = cssMode;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pScript"></param>
        /// <param name="pIsStartUp"></param>
        // EG 20160404 Migration vs2013
        public void RegisterScript(string pKey, string pScript, bool pIsStartUp)
        {
            Type csType = this.GetType();

            bool isOk = true;
            if (isOk)
            {
                if (pIsStartUp)
                    isOk = (!ClientScript.IsStartupScriptRegistered(csType, pKey));
                else
                    isOk = (!ClientScript.IsClientScriptBlockRegistered(csType, pKey));
            }
            if (isOk)
            {
                //
                if (pIsStartUp)
                    ClientScript.RegisterStartupScript(csType, pKey, pScript);
                else
                    ClientScript.RegisterClientScriptBlock(csType, pKey, pScript);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pScript"></param>
        public void RegisterScript(string pKey, string pScript)
        {
            RegisterScript(pKey, pScript, false);
        }

        /// <summary>
        /// RegisterClientScriptBlock for open new window with XML data
        /// </summary>
        /// <param name="pFolder"></param>
        /// <param name="pFileIdentifier"></param>
        /// <param name="pInnerXml"></param>
        protected void DisplayXml(string pFolder, string pFileIdentifier, string pInnerXml)
        {
            DisplayXml(pFolder, pFileIdentifier, pInnerXml, string.Empty);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFolder"></param>
        /// <param name="pFileIdentifier"></param>
        /// <param name="pInnerXml"></param>
        /// <param name="msgConfirm"></param>
        // EG 20160404 Migration vs2013
        protected void DisplayXml(string pFolder, string pFileIdentifier, string pInnerXml, string msgConfirm)
        {
            string WindowID = pFileIdentifier + "_" + DateTime.Now.ToString("yyyyMMddHHmmssfffff") + ".xml";
            string write_File = SessionTools.TemporaryDirectory.MapPath(pFolder) + @"\" + WindowID;
            string open_File = SessionTools.TemporaryDirectory.Path + pFolder + @"/" + WindowID;
            //			
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(pInnerXml);
            }
            catch (Exception)
            {
                string strErrXml = @"<?xml version=""1.0"" encoding=""utf-8""?> " + Cst.CrLf;
                strErrXml += "<data>" + Cst.CrLf;
                strErrXml += "<exception>Document is not a valid XML</exception>" + Cst.CrLf;
                strErrXml += "<![CDATA[" + Cst.CrLf + pInnerXml.ToString() + Cst.CrLf + "]]>";
                strErrXml += "</data>";
                xml.LoadXml(strErrXml);
            }
            xml.Save(write_File);
            //
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("<SCRIPT LANGUAGE='JAVASCRIPT' id='" + WindowID + "'>\n");
            if (StrFunc.IsFilled(msgConfirm))
            {
                sbScript.Append("var x=confirm('" + msgConfirm + "');" + "\n");
                sbScript.Append("if(x)" + "\n");
            }
            sbScript.Append("window.open(\"" + open_File + "\",\"_blank\",'fullscreen=no,resizable=yes,height=350,width=700,toolbar=yes,location=yes,scrollbars=yes,status=no');\n");
            sbScript.Append("</SCRIPT>");
            //
            // EG 20160404 Migration vs2013
            //RegisterClientScriptBlock(open_File, sbScript.ToString());
            ClientScript.RegisterClientScriptBlock(this.GetType(), open_File, sbScript.ToString());
            //WindowOpen = sbScript.ToString();

        }

        /// <summary>
        /// 
        /// </summary>
        protected void InactiveCacheClient()
        {
            // Fonction �vitant la gestion du cache cot� client 
            Response.CacheControl = "no-cache";
            Response.AddHeader("Pragma", "no-cache");
            Response.ExpiresAbsolute = DateTime.Now.Date;
            Response.Expires = -1;
        }

        /// <summary>
        /// Active oui/non les validateurs de la page 
        /// </summary>
        /// <param name="pIsEnabled"></param>
        protected void SetValidatorEnabled(bool pIsEnabled)
        {
            foreach (BaseValidator b in Page.Validators)
                b.Enabled = pIsEnabled;
        }

        /// <summary>
        /// Ajoute un script � la page qui ouvre un fichier 
        /// </summary>
        /// <param name="pWindowID"></param>
        /// <param name="open_FpMLFile"></param>
        /// <param name="pMsgConfirm"></param>
        protected void AddScriptWinDowOpenFile(string pWindowID, string open_FpMLFile, string pMsgConfirm)
        {
            StringBuilder sbScript = new StringBuilder();
            sbScript.Append("\n<SCRIPT LANGUAGE='JAVASCRIPT' id='" + pWindowID + "'>\n");
            if (StrFunc.IsFilled(pMsgConfirm))
            {
                sbScript.Append("var x=confirm('" + pMsgConfirm + "');" + "\n");
                sbScript.Append("if(x)" + "\n");
            }
            sbScript.Append("window.open(\"" + open_FpMLFile + "\",\"_blank\",'fullscreen=no,resizable=yes,top=0,left=0,height=350,width=700,location=no,menubar=yes,toolbar=yes,scrollbars=yes,status=yes');\n");
            sbScript.Append("</SCRIPT>\n");
            //20071212 FI Ticket 16012 => Migration Asp2.0
            ClientScript.RegisterClientScriptBlock(GetType(), pWindowID, sbScript.ToString());

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexEnumerator"></param>
        protected virtual void ApplyResourcesForControls(IEnumerator indexEnumerator)
        {
            Type ty;
            string styname;

            while (indexEnumerator.MoveNext())
            {
                ty = indexEnumerator.Current.GetType();
                styname = ty.Name;
                //Debug.WriteLine(styname);
                switch (styname)
                {
                    case "ResourceBasedLiteralControl":
                    case "LiteralControl":
                        break;
                    case "HtmlGenericControl": //20090406 FI Add HtmlGenericControl permet de descendre sous la bailse html lorsqu'elle runat="server" => Cas avec ma nouvelle m�thode BuildPage 
                    case "HtmlForm":
                    case "HtmlTable":
                    case "Table":
                    case "HtmlTableRow":
                    case "TableRow":
                    case "HtmlTableCell":
                    case "TableCell":
                    case "PlaceHolder":
                    case "Panel":
                        Control frm = (Control)indexEnumerator.Current;
                        ApplyResourcesForControls(frm.Controls.GetEnumerator());
                        break;
                    case "Label":
                    case "WCTooltipLabel":
                        Label lbl = (Label)indexEnumerator.Current;
                        if (lbl.ID != null)
                            lbl.Text = Ressource.GetString(lbl.ID.ToString());
                        break;
                    case "ImageButton":
                        ImageButton imgBtn = (ImageButton)indexEnumerator.Current;
                        imgBtn.ToolTip = Ressource.GetString(imgBtn.ID.ToString());
                        break;
                    case "LinkButton":
                        LinkButton lbtn = (LinkButton)indexEnumerator.Current;
                        lbtn.Text = Ressource.GetString(lbtn.ID.ToString());
                        break;
                    case "Button":
                        Button btn = (Button)indexEnumerator.Current;
                        btn.Text = Ressource.GetString(btn.ID.ToString());
                        break;
                    case "CheckBox":
                        CheckBox chk = (CheckBox)indexEnumerator.Current;
                        chk.Text = Ressource.GetString(chk.ID.ToString());
                        break;
                    default:
                        if (styname.IndexOf("_ascx") > 0 || styname.StartsWith("WC"))
                        {
                            Control uctrl = (Control)indexEnumerator.Current;
                            ApplyResourcesForControls(uctrl.Controls.GetEnumerator());
                        }
                        break;
                }
            }

        }

        /// <summary>
        /// Alimente le tag title enfant du tag header de la page HML avec la propri�t� PageFullTitle
        /// <para>Alimentation uniquement si Header est runat="server" </para>
        /// </summary>
        private void SetTitle()
        {
            if (null != this.Header)
                this.Header.Title = PageFullTitle;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool GetIsSupportsPartialRendering()
        {
            bool ret;
            // Lorsque le param�tre Ajax est pr�sent, il est prioritaire
            if (null != Request.QueryString["Ajax"])
                ret = BoolFunc.IsTrue(Request.QueryString["Ajax"]);
            else
                ret = IsSupportsPartialRendering;
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        private void OnResolveScriptReference(object Sender, ScriptReferenceEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20160404 Migration vs2013
        protected virtual void InitializeScriptManager()
        {
            if (null == ScriptManager)
                _sm = new AjaxScriptManager();

            if (StrFunc.IsEmpty(ScriptManager.ID))
                ScriptManager.ID = "ScriptManager";

            ScriptManager.SupportsPartialRendering = GetIsSupportsPartialRendering();
            ScriptManager.EnableScriptGlobalization = true;
            ScriptManager.EnableScriptLocalization = true;
            //
            //20090211 FI [16483] Les scripts du Framework Ajax et de l'AjaxControlToolkit sont extraits automatiquement des Dll respectives (isDebugJavaScript = false par d�faut)
            //si isDebugJavaScript=false  Les scripts des r�pertoires ~/Javascript/System.Web.Extensions et ~/Javascript/AjaxControlToolkit ne sont plus utilis�s, on utilise ceux livr�s � l'int�rieur des Dlls
            // EG 20160404 Migration vs2013
            if (IsDebugJavaScript)
            {
                //ScriptManager.ScriptPath = "~/Javascript";
                foreach (ScriptReference item in ScriptManager.Scripts)
                {
                    item.Path = "~/Javascript";
                }
            }
            ScriptManager.ResolveScriptReference += new EventHandler<ScriptReferenceEventArgs>(OnResolveScriptReference);
            // FI 20231004 [WI720] Spheres � l�coute de l'�v�nement AsyncPostBackError
            if (ScriptManager.SupportsPartialRendering)
                ScriptManager.AsyncPostBackError += new EventHandler<AsyncPostBackErrorEventArgs>(OnAsyncPostBackError);

            string asyncPostBackTimeout = ConfigurationManager.AppSettings["AsyncPostBackTimeout"];
            if (StrFunc.IsFilled(asyncPostBackTimeout))
                ScriptManager.AsyncPostBackTimeout = Convert.ToInt32(asyncPostBackTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pFileType"></param>
        /// <param name="pFileName"></param>
        /// <param name="pPath"></param>
        public void ResponseWriteFile(string pFileType, string pFileName, string pPath)
        {
            this.Response.Clear();
            //On construit la response
            this.Response.ContentType = pFileType;
            //this.Response.ContentEncoding = System.Text.Encoding.Default;
            this.Response.Charset = string.Empty;
            this.Response.AppendHeader("Content-Disposition", "attachment; filename=" + pFileName);
            //this.EnableViewState = false;
            //Et on ecrit la response avec le fichier
            this.Response.WriteFile(pPath + pFileName);
            //Response.Write("sw.ToString()"); 
            this.Response.Flush();
            this.Response.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <param name="pDisplay"></param>
        public virtual void SetStyleDisplay(Control pControl, string pDisplay)
        {
            bool isOk = false;
            if (false == isOk)
            {
                try
                {
                    WebControl webctrl = (WebControl)pControl;
                    ControlsTools.SetStyleList(webctrl.Style, pDisplay);
                    isOk = true;
                }
                catch { isOk = false; }
            }
            //
            if (false == isOk)
            {
                try
                {
                    HtmlControl ctrl = (HtmlControl)pControl;
                    ControlsTools.SetStyleList(ctrl.Style, pDisplay);
                    isOk = true;
                }
                catch { isOk = false; }
            }
            //
            if (false == isOk)
            {
                throw new NotImplementedException("control is not a WebControl or control is not a HtmlControl, control type not implemented");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        public virtual void RemoveStyleDisplay(Control pControl)
        {
            ControlsTools.RemoveStyleDisplay(pControl);
        }

        /// <summary>
        /// Affecte le focus au contr�le sp�cifi�
        /// <para>Cette fonction est une redefinition de la m�thode SetFocus native </para>
        /// <para>Utilisation de ScriptManager.SetFocus si scriptManager est instanci�</para>
        /// <para>Utilisation de base.SetFocus sinon</para>
        /// </summary>
        /// <param name="pClientId"></param>
        public new void SetFocus(string pClientId)
        {
            if (null != ScriptManager)
                ScriptManager.SetFocus(pClientId);
            else
                base.SetFocus(pClientId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLabel"></param>
        public void AddAuditTimeStep(string pLabel)
        {
            if (this.IsTraceTime)
                _auditTime.AddStep(pLabel);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pLabel"></param>
        public void AddAuditTimeStep(string pLabel, bool pIsStart)
        {
            if (this.IsTraceTime)
                _auditTime.AddStep(pLabel, pIsStart, null, null);
        }
        public void AddAuditTimeStep(string pLabel, bool pIsStart, string pInformation)
        {
            if (this.IsTraceTime)
                _auditTime.AddStep(pLabel, pIsStart, pInformation, null);
        }

        /// <summary>
        /// Active les validateurs de la page, demande aux validateurs de v�rifier les informations assign�es
        /// <para>Retourne true si la page est valide, retourne false sinon</para>
        /// </summary>
        /// <returns></returns>
        protected Boolean ValidatePage()
        {
            SetValidatorEnabled(true);
            Page.Validate();
            return Page.IsValid;
        }

        /// <summary>
        /// Validation l'URL
        /// </summary>
        /// FI 20150528 [20178] Modify
        /// EG 20160404 Migration vs2013
        /// FI 20160804 [Migration TFS] (Repository Remplace Referential)
        /// EG 20201116 [25556] Refactoring et gestion de l'URL avec Hyperlink.aspx
        /// EG 20200107 [25556] Correction Bug sur CheckURL (FirstOrDefault)
        /// EG 20200108 [25556] Correction bug sur CheckURL
        /// EG 20210226 [25556] Ajout URLDecode sur rawURL en contr�le final
        /// EG 20230306 [26279] Correctif Menu inaccessible pour un utilisateur NON ADMINISTRATEUR
        private void CheckURL()
        {
            //**************************************************************************************************************
            //WARNING: Par d�faut (en attendant de toutes les g�r�es sp�cifiquement), on conisd�re les pages "accessibles" !
            //**************************************************************************************************************
            bool isMenuAuthorized = true;
            //**************************************************************************************************************
            string rawUrl = Request.RawUrl;
            try
            {
                bool isHyperLink = rawUrl.Contains("hyperlink.aspx");
                SpheresMenu.Menus menus = SessionTools.Menus;
                if (menus != null)
                {
                    if (isHyperLink)
                    {
                        rawUrl = Request.CurrentExecutionFilePath;
                        if (null != Request.QueryString)
                            rawUrl += "?" + Request.QueryString.ToString();
                    }

                    #region Pages ASPX toujours autoris�es
                    if (new[] { 
                        "Default", "NewDefault", "DefaultWest", "Default",
                        "Login", "Sommaire", "About", "Profile", 
                        "LstOpen", "LstSave", "LstCriteria", "LstSelection",
                        "Tracker", "TrackerParam", "TrackerServiceObserver"}.Any(item => rawUrl.Contains("/" + item + ".aspx")))
                    {
                        //UserWithLimitedRights
                        isMenuAuthorized = (false == (SessionTools.User.IsSessionGuest &&
                            new[] { "LstSelection", "Tracker", "TrackerParam", "TrackerServiceObserver" }.Any(item => rawUrl.Contains("/" + item + ".aspx"))));
                    }
                    #endregion Pages ASPX toujours autoris�es

                    #region Pages ASPX autoris�es car accessible via un param GUID
                    else if (new[] { "TradeEvents", "Split", "Ear", "Tracks" }.Any(item => rawUrl.Contains("/" + item + ".aspx")))
                    {
                        isMenuAuthorized = true;
                    }
                    #endregion Pages ASPX autoris�es car accessible via un param GUID

                    #region Pages ASPX pas encore contr�l�es
                    else if (new[] { "Notepad", "DBImageViewer", "FileUpload", "DebtSecCapturePage" }.Any(item => rawUrl.Contains("/" + item + ".aspx")))
                    {
                        //Notepad.aspx?TN=ACTOR
                        //DBImageViewer.aspx?UploadTag=ACTOR
                        //FileUpload.aspx?UploadTag=ATTACHEDDOC&UploadMIMETypes=*/*;&TableName=ATTACHEDDOC&ColumnName=LODOC&KeyColumns=TABLENAME;ID;&KeyValues=TRADE
                        //DebtSecCapturePage.aspx?IDMenu=OTC_INP_DEBTSECURITY&PK=vw_tasset_IDT&PKV=89242&FK=_IDI&FKV=
                        isMenuAuthorized = true;
                    }
                    #endregion Pages ASPX pas encore contr�l�es

                    #region Pages ASPX contr�l�es
                    else if (new[] { "List", "Referential", "TradeCapturePage", "TradeAdminCapturePage", "TradeRiskCapturePage", "EventCapturePage", "RunIO", "OpenTradeAction" }.Any(item => rawUrl.Contains("/" + item + ".aspx")))
                    {

                        isMenuAuthorized = false;

                        string listPage = "/List.aspx";

                        #region Referential.aspx --> Transformation de l'URL en List.aspx
                        if (rawUrl.Contains("Referential.aspx?T="))
                        {
                            string t = this.Request.QueryString["T"] + string.Empty;
                            if (rawUrl.Contains("Referential.aspx?T=" + t + "&O="))
                            {
                                //Warning: &M= doit �tre le 1er param�tre INUTILISE pour que le "matching" fonctionne
                                if (rawUrl.Contains("&M="))
                                    rawUrl = rawUrl.Substring(0, rawUrl.IndexOf("&M="));
                                if (rawUrl.Contains("&IDMenu="))
                                    rawUrl = rawUrl.Substring(0, rawUrl.IndexOf("&IDMenu="));

                                string o = this.Request.QueryString["O"] + string.Empty;
                                rawUrl = rawUrl.Replace(@"Referential.aspx?T=" + t + @"&O=" + o, listPage + "?" + t + @"=" + o);

                                if (t == Cst.ListType.ProcessBase.ToString())
                                {
                                    rawUrl += "&ProcessType=" + "Service";
                                    if (o.Contains("_"))
                                        rawUrl += "&ProcessName=" + o.Substring(o.IndexOf("_") + 1);
                                }

                                Cst.ConsultationMode mode = Cst.ConsultationMode.Normal;
                                if (this.Request.QueryString["M"] == ((int)Cst.ConsultationMode.ReadOnly).ToString())
                                    mode = Cst.ConsultationMode.ReadOnly;

                                switch (mode)
                                {
                                    case Cst.ConsultationMode.ReadOnly:
                                        rawUrl += "&InputMode=" + ((int)Cst.DataGridMode.FormViewer).ToString();
                                        break;
                                    default:
                                        rawUrl += "&InputMode=" + ((int)Cst.DataGridMode.FormInput).ToString();
                                        break;
                                }

                                string titleRes = this.Request.QueryString["TitleRes"] + string.Empty;
                                if (StrFunc.IsFilled(titleRes))
                                    rawUrl += "&TitleRes=" + titleRes;

                                string p;
                                for (int i = 1; i < 10; i++)
                                {
                                    p = this.Request.QueryString["P" + i.ToString()] + string.Empty;
                                    if (StrFunc.IsEmpty(p))
                                        break;
                                    else
                                        rawUrl += "&P" + i.ToString() + "=" + p;
                                }

                                string condApp = this.Request.QueryString["CondApp"] + string.Empty;
                                if (StrFunc.IsFilled(condApp))
                                    rawUrl += "&CondApp=" + condApp;
                            }
                        }
                        #endregion

                        #region List.aspx?Repository=ATTACHEDDOC (--> Notepad)
                        if (rawUrl.Contains(listPage + "?Repository=ATTACHEDDOC"))
                        {
                            //TODO (il faudrait ici transformer l'URL en "List.aspx" sur le r�f�rentiel concern�...)
                            //List.aspx?Repository=ATTACHEDDOC&InputMode=2&IDMenu=ATTACHEDDOC&FK=10688&SubTitle=Acteurs%3a+10002&DA=ACTOR
                            //List.aspx?Repository=ATTACHEDDOC&InputMode=2&IDMenu=ATTACHEDDOC&FK=89467&SubTitle=Trades%3a+%7b1%7d&DA=TRADE
                            isMenuAuthorized = true;
                        }
                        #endregion

                        #region List.aspx?Repository=ACTORAMOUNT_H (--> Historic)
                        if (rawUrl.Contains(listPage + "?Repository=ACTORAMOUNT_H"))
                            rawUrl = rawUrl.Replace("AMOUNT_H", "AMOUNT");
                        #endregion

                        #region List.aspx?Repository=ACTORRATING_H (--> Historic)
                        if (rawUrl.Contains(listPage + "?Repository=ACTORRATING_H"))
                            rawUrl = rawUrl.Replace("RATING_H", "RATING");
                        #endregion

                        // AL 20240607 [WI955] Impersonate mode
                        if (rawUrl.Contains(listPage + "?Repository=ACTORIMPERSONATE"))
                            isMenuAuthorized = true;

                        if (rawUrl.Contains(listPage + "?Repository=ACTORIMPERSONATEDBYME"))
                            isMenuAuthorized = true;

                        #region List.aspx?Repository=xxxx&InputMode=1&OpenerKeyId=xxxx  (--> Button "..." de la saisie ou des filtres du multi-crit�re)
                        else if (rawUrl.Contains(listPage + "?Repository=") && rawUrl.Contains("&InputMode=1&OpenerKeyId="))
                        {
                            //TODO (il faudrait ici peut-�tre contr�ler le droit d'acc�s au r�f�rentiel concern�...)
                            isMenuAuthorized = true;
                        }
                        #endregion

                        #region List.aspx?Trade=ACCDAYBOOK (--> Book entries), Button "...", etc
                        else if (rawUrl.Contains("/Referential.aspx?T=Trade") || rawUrl.Contains(listPage + "?Invoicing=INVOICEFEESDETAIL") ||
                            new[] { "Accounting:ACCDAYBOOK", "CASHFLOWS" }.Any(item => rawUrl.Contains(listPage + "?Trade=" + item)) ||
                            (new[] { "BOOK_VIEWER", "ACTOR", "ASSET_", "CSS", "NETDESIGNATION" }.Any(item => rawUrl.Contains(listPage + "?Repository=" + item)) && rawUrl.Contains("&InputMode=1&OpenerKeyId=")) ||
                            (new[] { "MATURITY", "ASSET_ETD_EXPANDED" }.Any(item => rawUrl.Contains(listPage + "?Consultation=" + item)) && rawUrl.Contains("&InputMode=1&OpenerKeyId=")))
                        {
                            //TODO 
                            //On assimile cela � Trade.aspx pour l'instant...
                            rawUrl = @"/TradeCapturePage.aspx";
                        }
                        #endregion

                        #region TEMPORAIRE PL 20110207
                        else if (rawUrl.Contains(listPage + "?Repository=DERIVATIVECONTRACT2&InputMode=2"))
                        {
                            rawUrl = rawUrl.Replace(@"DERIVATIVECONTRACT2", @"DERIVATIVECONTRACT");
                        }
                        #endregion

                        #region IOTRACK  / IOTRACKCOMPARE
                        // FI 20190828 [24869] remplace IDTRK_L par IDPROCESS_L  -->
                        else if (rawUrl.Contains(listPage + "?Log=IOTRACK"))
                        {
                            rawUrl = rawUrl.Replace("P1=IDTRK_L", "P1=IDPROCESS_L");

                            // RD 20191003 [24869] Traiter le menu IOTRACKCOMPARE
                            if (rawUrl.Contains(listPage + "?Log=IOTRACKCOMPARE_"))
                            {
                                foreach (EFS.SpheresIO.CompareOptions compareOption in Enum.GetValues(typeof(EFS.SpheresIO.CompareOptions)))
                                {
                                    string shortName = EFS.SpheresIO.CompareOptionsAttribute.ConvertToString(compareOption).ToUpper();
                                    if (rawUrl.Contains(listPage + "?Log=IOTRACKCOMPARE_" + shortName))
                                    {
                                        rawUrl = rawUrl.Replace("?Log=IOTRACKCOMPARE_" + shortName, @"?Log=IOTRACKCOMPARE");
                                        break;
                                    }
                                }
                            }
                        }
                        #endregion


                        // FI 20160804 [Migration TFS] (Folder Repository Replace Folder Referential)    
                        #region List.aspx?Log=EOD_POSREQUEST
                        else if (rawUrl.Contains(listPage + "?Repository=ENTITYMARKET") || rawUrl.Contains(listPage + ("?ProcessBase=POSKEEPING")) ||
                            new[] { "EOD_POSREQUEST", "POSACTIONDET", "ACTIONDET", "POSREQUESTPROCESSDET_L", "TRACKER_POSREQUEST" }.Any(item => rawUrl.Contains(listPage + "?Log=" + item)))
                        {
                            // Cl�ture de journ�e EOD / Cl�tures sp�cifiques / PosAction
                            isMenuAuthorized = true;
                        }
                        #endregion

                        #region TRACKER_L
                        else if (new[] { "TRACKER_L", "FULLTRACKER_L"/*, "TRACKERDET_L"*/ }.Any(item => rawUrl.Contains(listPage + "?Log=" + item)))
                        {
                            // FI 20240402 [WI888] TRACKER_L (lorsque ouverture d'un enregistrement du tracker). Spheres� s'appuie sur les droits d'acc�s au menu LOGREQUEST
                            Menu.Menu menu = menus.SelectByIDMenu(IdMenu.GetIdMenu(IdMenu.Menu.LOGREQUEST));
                            isMenuAuthorized = menu.IsEnabled;
                        }
                        #endregion

                        #region CORPOEVENTCONTRACT and CORPOEVENTASSET
                        else if (new[] { "CORPOEVENTCONTRACT", "CORPOEVENTASSET" }.Any(item => rawUrl.Contains(listPage + "?Repository=" + item)))
                        {
                            isMenuAuthorized = true;
                        }
                        #endregion

                        //
                        //20110917 codage en dur avant demo
                        if (StrFunc.IsFilled(rawUrl))
                        {
                            if (rawUrl.Contains("MCO_RPT"))
                                isMenuAuthorized = true;
                        }

                        //
                        #region Contr�le des droits d'acc�s � l'URL via l'objet Menu
                        if (!isMenuAuthorized)
                        {
                            isMenuAuthorized = (null != menus.Cast<SpheresMenu.Menu>().FirstOrDefault(item => item.IsEnabled && item.IsAction && StrFunc.IsFilled(item.Url) && HttpUtility.UrlDecode(rawUrl).Contains(item.Url)));

                            //PL 20111114 Grosse verrue � revoir (FDA est �galement au courant)
                            if ((false == isMenuAuthorized) && rawUrl.Contains("&InputMode=" + ((int)Cst.DataGridMode.FormViewer).ToString()))
                            {
                                rawUrl = rawUrl.Replace("&InputMode=" + ((int)Cst.DataGridMode.FormViewer).ToString(), "&InputMode=" + ((int)Cst.DataGridMode.FormInput).ToString());
                                isMenuAuthorized = (null != menus.Cast<SpheresMenu.Menu>().FirstOrDefault(item => item.IsEnabled && item.IsAction && StrFunc.IsFilled(item.Url) && HttpUtility.UrlDecode(rawUrl).Contains(item.Url)));
                            }
                            else if (false == isMenuAuthorized)
                            {
                                isMenuAuthorized = (null != menus.Cast<SpheresMenu.Menu>().FirstOrDefault(item => item.IsEnabled && item.IsAction && StrFunc.IsFilled(item.Url) && item.Url.Contains(HttpUtility.UrlDecode(rawUrl))));
                            }
                            // EG 20230306 [26279] New
                            // Verrue pour autroriser l'acc�s � la consultation d'un trade depuis un menu autoris� permettant d'ouvrir un trade :
                            // Permet donc depuis Liste D�pouil/Alloc, positions, etc. (autoris�) => Consultation d'UN trade (avec menu Saisie des trades NON autoris�)
                            if ((false == isMenuAuthorized) && isHyperLink)
                                isMenuAuthorized = (null != menus.Cast<SpheresMenu.Menu>().FirstOrDefault(item => item.IsAction && StrFunc.IsFilled(item.Url) && Request.CurrentExecutionFilePath.Contains(item.Url)));
                        }
                        #endregion Contr�le des droits d'acc�s � l'URL via l'objet Menu
                    }
                    #endregion Pages ASPX contr�l�es
                }
            }
            catch { }
            finally
            {
#if DEBUG
                //GLOP FI Code pour tjs passer test => � supprimer
                if (false == isMenuAuthorized)
                    MsgForAlertImmediate = "Attention le menu n'est pas autoris�, Spheres� ouvre tout de m�me l'URL en mode debug";
                //
                isMenuAuthorized = true;
#endif
                #region URL non autoris�e --> Redirect on Unavailable.aspx
                if (!isMenuAuthorized)
                {
                    //---------------------------------------
                    //URL non autoris�e --> Redirect sur page 
                    //---------------------------------------
                    // EG 20160404 Migration vs2013
                    // #warning 20080129 PL A finaliser... (Contr�le des URLs)
                    if (SystemSettings.GetAppSettings("Spheres_DebugValidationURL") == "1")
                    {
                        isMenuAuthorized = true;
                    }
                    else
                    {
                        //#if DEBUG
                        //                                //JavaScript.AlertStartUpImmediate(this, "Debug: Page non autoris�e !" + Cst.CrLf + Cst.CrLf
                        //                                //    + "URL: " + this.Request.RawUrl + Cst.CrLf
                        //                                //    + "Raw: " + rawUrl);
                        //#else
                        Response.Redirect(this.Request.ApplicationPath.ToString() + "/Unavailable.aspx?ErrorMessage=Insufficient%20privilege", true);
                        //#endif
                    }
                }
                #endregion URL non autoris�e --> Redirect on Unavailable.aspx
            }
        }

        /// <summary>
        ///  V�rification que la session server est vivante
        ///  <para>Si a session web est morte Spheres� simule une perte d'authentification afin d'ouvrir login.aspx</para>
        /// </summary>
        private void CheckSessionState()
        {
            if (Request.IsAuthenticated)
            {
                bool isDisconnect = false;
                if (false == SessionTools.IsConnected)
                {
                    //=> Ce produit lorsque la sessionServer a expir�e 
                    isDisconnect = true;
                }
                if (isDisconnect)
                    Disconnect.Run(this);
            }
        }
        /// <summary>
        /// V�rification que la date d'expiration de la session a �t� atteinte
        /// <para>si oui, Spheres� simule une perte d'authentification afin d'afficher login.aspx</para>
        /// </summary>
        private void CheckLoginExpire()
        {
            if (SessionTools.IsConnected)
            {
                bool isLoginExpire = SessionTools.IsLoginExpire();
                if (isLoginExpire)
                {
                    string msg = Ressource.GetString("FailureConnect_LoginTimeExpired");
                    JavaScript.AlertImmediate(this, msg, true);
                }
                //
                if (isLoginExpire)
                    Disconnect.Run(this);
            }
        }

        /// <summary>
        ///  Alimentation de _GUID
        /// </summary>
        private void SetGUID()
        {
            if (false == IsPostBack)
                _GUID = SystemTools.GetNewGUID();
            else
                _GUID = Request.Form["__GUID"];
        }

        /// <summary>
        /// Ecriture de l'audit dans la sortie du debuggueur 
        /// <para>Ecriture de l'audit dans un fichier et ouverture du fichier � l'affichage de la page</para>
        /// </summary>
        private void WriteAuditTime()
        {
            if (ArrFunc.IsFilled(_auditTime.Step))
            {
                _auditTime.WriteDebug();

                string WindowID = FileTools.GetUniqueName("AuditTime", this.GetType().Name);
                string write_File = SessionTools.TemporaryDirectory.MapPath("AuditTime") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "AuditTime" + @"/" + WindowID + ".xml";
                //
                _auditTime.WriteFile(write_File);
                //
                AddScriptWinDowOpenFile(WindowID, open_File, null);
            }
        }

        /// <summary>
        ///  G�n�re une httpReponse qui ouvre un fichier excel avec le contenu du datagrid
        ///  <para>Le rendu HTML du grid doit �tre consitiu� d'�l�ment HMTL simples (table, Tr, Td,..)</para>
        /// </summary>
        /// <param name="pDataGrid"></param>
        /// <param name="pFileName">Le fichier xls se nommera {pFileName}.xls</param>
        /// <param name="pStyleCssDeclaration">D�claration d'une feuille de style</param>
        /// FI 20130417 [18596] ExportGridToExcel
        /// FI 20160308 [21782] Modify
        /// FI 20190318 [24568][24588] Add Param�tre pStyleCssDeclaration
        /// EG 20210126 [25556] Gestion Samesite (Strict) sur cookie
        /// EG 20210216 [25664] S�curit� Cookie
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage en JS)
        protected void ExportGridToExcel(DataGrid pDataGrid, string pStyleCssDeclaration, string pFileName, string pCookieBlockValue)
        {
            //Spheres� r�cup�re le rendu du gatagrid et ecrit la reponse avec le rendu du controle DataGrid
            //Spheres� envoie ensuite au client la sortie actuelle de la page et ferme le socket 

            StringWriter stringWrite = new StringWriter(new StringBuilder(pStyleCssDeclaration));

            HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
            pDataGrid.RenderControl(htmlWrite);
            pDataGrid.EnableViewState = false;

            // RD 20140708 [20181] Le NonBreakSpace (Alt 0160) n'est pas reconnu par Excel comme �tant un s�parateur de groupe
            // Remplacer NonBreakSpace par un vrai espace. 
            //Response.Write(stringWrite.ToString());
            string stringResponse = stringWrite.ToString();
            stringResponse = stringResponse.Replace(Cst.NonBreakSpace, Cst.Space);

            Response.Clear();
            Response.AddHeader("content-disposition", StrFunc.AppendFormat("attachment;filename={0}.xls", pFileName));
            Response.ContentEncoding = Encoding.Default;
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.ContentType = Cst.TypeMIME.Vnd.MsExcel;

            // FI 20160308 [21782] add Content-Length (stringResponse.Length.ToString() = taille en octet du flux du fait de l'usage Encoding.Default)
            // Encoding.Default retourne Windows-1252 (sur un poste Europ�en)
            // Windows-1252 utilis�e sur tous les continents, en Europe de l�Ouest, en Am�rique, et dans une grande partie de l'Afrique ou de l�Oc�anie ainsi que certains pays d�Asie du Sud-Est
            Response.AddHeader("Content-Length", stringResponse.Length.ToString());

            Response.Write(stringResponse);

            // Pour unblockUI de l'overlay d'attente
            Response.AppendCookie(AspTools.InitHttpCookie(RequestTrackExportType.XLS + "fileToken", false, pCookieBlockValue));

            Response.Flush();
            Response.Close();

        }

        /// <summary>
        /// Lecture, Chargement des param�tres d'exportation au format par d�faut
        /// 
        /// </summary>
        /// <param name="pName">Nom du document � exporter</param>
        /// <returns></returns>
        // EG 20190411 [ExportFromCSV] New
        // EG 20190417 [ExportFromCSV] Upd
        // EG 20190419 [ExportFromCSV] Upd
        protected CsvCommonConfigElement GetCsvConfigSetting(string pName)
        {
            CsvCommonConfigElement csvCurrentSettings = null;
            try
            {
                CsvSection csvSection = WebConfigurationManager.GetWebApplicationSection("csvSection") as CsvSection;
                csvCurrentSettings = csvSection.GetSettings(pName);
            }
            catch { }

            // Si aucun param�tre alors initialisation avec nos param�tres par d�faut
            if (null == csvCurrentSettings)
            {
                // Valeur par d�faut si aucune de sp�cifi�e dans fichier Config � savoir :
                // 1. Culture courante
                // 2. DateTimeOffset (valeur par d�faut = CsvPattern.datetimeOffset)
                //    soit ShortDatePattern + " " + LongTimePattern + PatternTms) + Hour of TimeZone
                // 3. DateTime (valeur par d�faut = CsvPattern.date soit ShortDatePattern de la culture)
                //    soit ShortDatePattern
                // 4. Les autres types sont ceux par d�faut selon la culture
                csvCurrentSettings = new CsvDefaultConfigElement
                {
                    Culture = Thread.CurrentThread.CurrentCulture.Name,
                    Patterns = new CsvPatternsCollection()
                };
                csvCurrentSettings.Patterns.Add("dateTimeOffset", CsvPattern.datetimeOffset);
                csvCurrentSettings.Patterns.Add("dateTime", CsvPattern.date);
                csvCurrentSettings.Patterns.Add("decimal", "G");
            }
            return csvCurrentSettings;
        }

        /// <summary>
        /// Envoi la r�ponse de l'exportation au format CSV au client
        /// </summary>
        /// <param name="pResult">Chaine d'exportation</param>
        /// <param name="pFileName"></param>
        /// EG 20210126 [25556] Gestion Samesite (Strict) sur cookie
        /// EG 20210216 [25664] S�curit� Cookie
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage en JS)
        protected void ExportCsvResponse(byte[] pResult, string pFileName, string pCookieBlockValue)
        {
            Response.Clear();
            Response.ContentType = Cst.TypeMIME.Text.Csv;
            Response.AddHeader("content-disposition", StrFunc.AppendFormat("attachment;filename={0}.csv", pFileName));
            Response.ContentEncoding = Encoding.Default;
            Response.Charset = string.Empty;
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            string stringResponse = Encoding.Default.GetString(pResult);
            Response.AddHeader("Content-Length", stringResponse.Length.ToString());
            Response.Write(stringResponse);
            Response.AppendCookie(AspTools.InitHttpCookie(RequestTrackExportType.CSV + "fileToken", false, pCookieBlockValue));
            Response.Flush();
            Response.Close();

        }

        //* --------------------------------------------------------------- *//
        // Gestion du dernier Focus
        //* --------------------------------------------------------------- *//
        #region HookOnFocus
        /// <summary>
        /// Lit r�cursivement tous les controles de la page et positionne
        /// valorise l'attribut __LASTFOCUS si le contr�le est de type : TextBox , DropdownList, ListBox ou button
        /// </summary>
        /// <param name="CurrentControl">Le contr�le a hame�onner.</param>
        protected void HookOnFocus(Control pCurrentControl)
        {
            if ((pCurrentControl is TextBox) || (pCurrentControl is DropDownList) || (pCurrentControl is ListBox))
            {
                (pCurrentControl as WebControl).Attributes.Add("onfocus", "try{document.getElementById('__LASTFOCUS').value=this.id} catch(e) {}");
            }
            else if (pCurrentControl is Button)
            {
                string _dlgDefaultButtonName = GetDefaultDialogButton((WebControl)pCurrentControl);
                (pCurrentControl as WebControl).Attributes.Add("onfocus", "try{document.getElementById('__LASTFOCUS').value='" + _dlgDefaultButtonName + "'} catch(e) {}");
            }

            if (pCurrentControl.HasControls())
            {
                foreach (Control _currentChildControl in pCurrentControl.Controls)
                {
                    this.HookOnFocus(_currentChildControl);
                }
            }
        }
        #endregion HookOnFocus

        /// <summary>
        /// Choix du boutton par d�faut sur une fen�tre de demande de confirmation
        /// </summary>
        /// <param name="CurrentControl">Le bouton source</param>
        private string GetDefaultDialogButton(WebControl pWebControl)
        {
            string _dlgDefaultButtonName;
            switch (pWebControl.ID)
            {
                case "btnRefresh":
                case "btnRecord":
                case "btnCancel":
                case "btnSend":
                case "btnDuplicate":
                    _dlgDefaultButtonName = "btndlg_yes";
                    break;
                default:
                    _dlgDefaultButtonName = "btndlg_no";
                    break;
            }
            return _dlgDefaultButtonName;
        }


        /// <summary>
        /// Retourne le g�n�rateur de journal des actions utilisateurs 
        /// <para>Cette m�thode doit �tre overrid�e lorsque la page doit alimenter le journal des actions utilisateurs</para>
        /// </summary>
        /// FI 20140519 [19923] add virtual method
        protected virtual IRequestTrackBuilder LoadRequestTrackBuilder()
        {
            return null;
        }



        /// <summary>
        ///  Alimentation du journal des actions utilisateurs dans une thread ind�pendant
        /// </summary>
        /// <param name="pRequestTrackBuilder"></param>
        private static void ThreadRequestTrackGen(string pURL, IRequestTrackBuilder pRequestTrackBuilder)
        {
            ThreadRequestTrack threadRequestTrack = new ThreadRequestTrack
            {
                culture = Cst.EnglishCulture,
                Session = SessionTools.AppSession,
                URL = pURL.ToString(),
                errManager = (ErrorManager)HttpContext.Current.Application["LOG"],
                trackBuilder = pRequestTrackBuilder
            };
            ThreadTools.ExecuteTask(threadRequestTrack);
            
        }


        /// <summary>
        /// Retourne le contr�le serveur qui a g�n�rer la publication de la page
        /// <para>Retourne null si le cont�le n'est pas identifi�</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20140519 [19923] add Method
        public Control GetPostBackControl()
        {
            if (false == IsPostBack)
                return null;

            Control control = null;
            //first we will check the "__EVENTTARGET" because if post back made by the controls
            //which used "_doPostBack" function also available in Request.Form collection.
            string ctrlname = PARAM_EVENTTARGET;
            if (ctrlname != null && ctrlname != String.Empty)
            {
                control = Page.FindControl(ctrlname);
            }
            // if __EVENTTARGET is null, the control is a button type and we need to
            // iterate over the form collection to find it
            else
            {
                foreach (string ctl in Page.Request.Form)
                {
                    Control c;
                    //handle ImageButton they having an additional "quasi-property" in their Id which identifies
                    //mouse x and y coordinates
                    if (ctl.EndsWith(".x") || ctl.EndsWith(".y"))
                    {
                        string controlId = ctl.Substring(0, ctl.Length - 2);
                        c = Page.FindControl(controlId);
                    }
                    else
                    {
                        c = Page.FindControl(ctl);
                    }
                    if (c is Button || c is ImageButton)
                    {
                        control = c;
                        break;
                    }
                }
            }
            return control;
        }

        /// <summary>
        /// Alimentation du journal des actions utilisateur
        /// </summary>
        /// FI 20141021 [20350] Modify
        protected virtual void RequestTrackGen()
        {

            if (null != RequestTrackBuilder)
            {
                InitRequestTrackBuilder(RequestTrackBuilder);

                bool isThreadPool = (bool)SystemSettings.GetAppSettings("ThreadPool", typeof(System.Boolean), true);
                if (isThreadPool)
                {
                    ThreadRequestTrackGen(Request.Url.ToString(), RequestTrackBuilder);
                }
                else
                {
                    RequestTrackBuilder.BuildRequestTrack();
                    RequestTrackBuilder.SendRequestTrack();
                }
            }
        }


        /// <summary>
        /// Initilisation de {pRequestTrackBuilder} avec les informations li�es au contexte http
        /// </summary>
        /// <param name="pRequestTrackBuilder"></param>
        private void InitRequestTrackBuilder(RequestTrackBuilderBase pRequestTrackBuilder)
        {
            pRequestTrackBuilder.Cs = SessionTools.CS;
            pRequestTrackBuilder.User = SessionTools.User;
            pRequestTrackBuilder.Session = SessionTools.AppSession;
            pRequestTrackBuilder.Url = Request.RawUrl;
            pRequestTrackBuilder.Timestamp = DateTime.Now;
            pRequestTrackBuilder.PageGuid = this.GUID;
        }

        /// <summary>
        ///  G�n�re une cl� pour l'alimentation dans DataCache d'une variable session d�di�es au maintient d'�tat
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        /// FI 20200225 [XXXXX]
        public string BuildDataCacheKey(string pKey)
        {
            if (StrFunc.IsFilled(pKey))
                return StrFunc.AppendFormat("{0}_{1}", this.GUID, pKey);
            else
                return this.GUID;
        }

        /// <summary>
        /// Applique une directive Refresh dans Current.Response 
        /// </summary>
        /// FI 20200731 [XXXXX] Add
        // EG 20200818 [XXXXX] Rename SetAutoRefresh
        protected virtual void SetAutoRefresh()
        {
            if (_autoRefresh > 0)
                HttpContext.Current.Response.AppendHeader("Refresh", _autoRefresh.ToString());
        }

        #endregion
    }
}

