using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EfsML.Business;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

namespace EFS.Spheres
{
    #region GroupTracker
    [Serializable]
    public class GroupTracker
    {
        public Cst.GroupTrackerEnum group;
        public List<Pair<ProcessStateTools.StatusEnum, int>> status;
        public GroupTracker()
        {
        }
        public GroupTracker(Cst.GroupTrackerEnum pGroup, ProcessStateTools.StatusEnum pStatus)
        {
            group = pGroup;
            status = new List<Pair<ProcessStateTools.StatusEnum, int>>
            {
                new Pair<ProcessStateTools.StatusEnum, int> { First = pStatus, Second = 0 }
            };
        }
    }
    #endregion GroupTracker
    #region Tracker Page
    // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    // EG 20220112 [XXXXX] Suppression de tout ce qui fait référence au plugin de scrolling pour afficher les totaux (plus exploité)
    public partial class TrackerPage : PageBase
    {

        /// <summary>
        /// Obtient la dernière Query exécutée pour charger le tracker
        /// </summary>
        /// FI 20180910 [XXXXX] Add
        public Pair<string, TimeSpan> LastQuery
        {
            get;
            private set;
        }

        #region Members
        //public Dictionary<Cst.GroupTrackerEnum, string> m_GroupTrackerName;
        public Dictionary<Cst.GroupTrackerEnum, Pair<string, bool>> m_GroupTrackerName;
        public Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>> m_ReadyStateName;
        public Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>> m_StatusName;
        public Dictionary<ProcessStateTools.ReadyStateEnum, Pair<int, List<GroupTracker>>> m_TrackerList;

        /// <summary>
        /// Response members
        /// </summary>
        private MessageQueue m_QueueResponse;
        private MQueueSendInfo m_SendInfo;
        private string m_FullPathRecipient;
        private int messageNo;
        private string m_SessionID;
        private string m_CS;
        private readonly ArrayList m_DialogMessage;
        private bool m_IsTrackerAlert;
        private Int64 m_TrackerAlertProcess;
        #endregion Members
        #region Accessors
        #region IsFileWatcher
        /// <summary>
        /// Message géré avec FileWatcher
        /// </summary>
        /// EG 20131211 Text Null sur m_SendInfo et MOMSetting
        private bool IsFileWatcher
        {
            get
            {
                return (null != m_SendInfo) && (null != m_SendInfo.MOMSetting) && (Cst.MOM.MOMEnum.FileWatcher == m_SendInfo.MOMSetting.MOMType);
            }
        }
        #endregion IsFileWatcher
        #region IsMsMQueue
        /// <summary>
        /// Message géré avec MSMQueue
        /// </summary>
        /// EG 20131211 Text Null sur m_SendInfo et MOMSetting
        private bool IsMsMQueue
        {
            get
            {
                return (null != m_SendInfo) && (null != m_SendInfo.MOMSetting) && (Cst.MOM.MOMEnum.MSMQ == m_SendInfo.MOMSetting.MOMType);
            }
        }
        #endregion IsMsMQueue
        #region FilterFile
        private string FilterFile
        {
            get
            {
                StrBuilder filterFile = new StrBuilder();
                filterFile += EFS.SpheresService.ServiceTools.GetQueueSuffix(Cst.ServiceEnum.SpheresResponse)
                    + "???????";                            //15 ? -> ProcessType (avec X à Droite)   
                filterFile += "_" + "???????????????????";	//19 ? -> System date (yyyyMMddHHmmssfffff)
                for (int i = 1; i <= 3; i++)
                    filterFile += "_" + "????????";			//n*3 ? -> Trade status 
                filterFile += "_" + m_SessionID;			//SessionId
                filterFile += ".xml";
                return filterFile.ToString();
            }
        }
        #endregion FilterFile
        #region FullPathRecipient
        /// <summary>
        /// Chemin complet de lecture/stockage des messages REPONSE
        /// </summary>
        private string FullPathRecipient
        {
            get
            {
                if (StrFunc.IsEmpty(m_FullPathRecipient) && (null != m_SendInfo))
                {
                    m_FullPathRecipient = m_SendInfo.MOMSetting.MOMPath;
                    if (IsFileWatcher)
                        m_FullPathRecipient += @"\";
                    m_FullPathRecipient += EFS.SpheresService.ServiceTools.GetQueueSuffix(m_CS, Cst.ServiceEnum.SpheresResponse, SessionTools.Collaborator_ENTITY_IDA);
                }
                return m_FullPathRecipient;
            }
        }
        #endregion FullPathRecipient
        #region RemoveFilterFile
        /// <summary>
        /// 
        /// </summary>
        private string RemoveFilterFile
        {
            get
            {
                StrBuilder filterFile = new StrBuilder();
                filterFile += EFS.SpheresService.ServiceTools.GetQueueSuffix(Cst.ServiceEnum.SpheresResponse)
                    + "???????";//15 ? -> ProcessType (avec X à Droite)   
                filterFile += "_" + "???????????????????";	//19 ? -> System date (yyyyMMddHHmmssfffff)
                for (int i = 1; i <= 3; i++)
                    filterFile += "_" + "????????";			//n*3 ? -> Trade status 
                filterFile += "_*";
                filterFile += ".xml";
                return filterFile.ToString();
            }
        }
        #endregion RemoveFilterFile
        #endregion Accessors
        #region Constructor
        public TrackerPage()
        {
            m_DialogMessage = new ArrayList();
        }
        #endregion Constructor

        #region Methods
        #region Page_Load
        /// <summary>
        /// Chargement de la page du tracker
        /// . Construction des dictionnaires de gestion du Tracker (READYSTATE, GROUP, STATUS et HELP)
        /// . Chargement du tracker
        /// . Lecture des messages de réponse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200818 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker via Contrôle Hidden pour stocker la valeur
        // EG 20200818 [XXXXX] Plus de bouton pour activer/désactiver le refresh automatique du tracker (à l'image de List.aspx - aller dans les paramètres du tracker pour modification) 
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Nouveaux Boutons Favori/Groupes
        // EG 20220623 [XXXXX] Test sur OpenResponseRecipient (Nouvelle page default)
        // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        protected void Page_Load(object sender, EventArgs e)
        {
            hidMaskGroup.Attributes["title"] = Ressource.GetString("imgBtnMaskGroup");
            hidHisto.Value = SessionTools.TrackerHistoric;
            bool isLoadParam = StrFunc.IsFilled(Request.Params["__EVENTARGUMENT"]) && ("LoadParam" == Request.Params["__EVENTARGUMENT"]);
            bool isTimerRefresh = StrFunc.IsFilled(Request.Params["__EVENTTARGET"]) && ("timerRefresh" == Request.Params["__EVENTTARGET"]);
            bool isLoadSession = (false == IsPostBack) || isLoadParam || isTimerRefresh;
            if (false == isLoadSession)
                isLoadSession = StrFunc.IsFilled(Request.Params["imgautorefresh.x"]) || StrFunc.IsFilled(Request.Params["imgrefresh.x"]);
            SetHead(this, "Tracker - Spheres © 2023 EFS");

            if (isLoadSession)
            {
                //Construction du tracker et Chargement des données
                CreateReadyStateData();
                DisplayGroup();
                LoadData();
            }
            /// EG 20220623 [XXXXX] Shibboleth
            // Lecture des éventuels messages REPONSE (Appel si le tracker est présent dans la page oldDefault.aspx et non pas dans default.aspx)
            if (String.IsNullOrEmpty(Request.QueryString["default"]))
                OpenResponseRecipient();
            SetAttributesTimerRefresh();
            MaskGroup();
            btnautorefresh.Enabled = (AutoRefresh > 0);
            btnautorefresh.OnClientClick = (btnautorefresh.Enabled? "Block();":string.Empty);
        }
        #endregion Page_Load
        #region OnInit
        /// <summary>
        /// Initialisation de la page
        /// . Paramètres de gestion des messages de réponse 
        /// . Timer de raffraichissement
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        // EG 20200818 [XXXXX] Plus de bouton pour activer/désactiver le refresh automatique du tracker (à l'image de List.aspx - aller dans les paramètres du tracker pour modification) 
        // EG 20210128 [XXXXX] Ajout et gestion paramètre sur l'ouverture du tracker pour spécifier qu'il est détaché de la Main.
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20210217 [25661] CSRF Correctif Erreur sur Tracker                    
        override protected void OnInit(EventArgs e)
        {
            this.AbortRessource = true;
            base.OnInit(e);

            Form = frmTracker;
            AntiForgeryControl();

            toolsbar.CssClass = this.CSSMode + " " + SessionTools.Company_CssColor;
            if ((null != Request.QueryString["float"]) && (Request.QueryString["float"] == "1"))
                trackerContainer.Attributes.Add("class", "float");

            // Chargement des paramètres de lecture des messages de RESPONSE
            m_IsTrackerAlert = SessionTools.IsTrackerAlert;
            m_TrackerAlertProcess = SessionTools.TrackerAlertProcess;
            m_SendInfo = new MQueueSendInfo()
            {
                MOMSetting = MOMSettings.LoadMOMSettings(Cst.ProcessTypeEnum.NA)
            };
            m_SessionID = SessionTools.SessionID;
            m_CS = SessionTools.CS;

            AutoRefresh = SessionTools.TrackerRefreshInterval;
        }
        #endregion OnInit
        #region OnUnLoad
        protected override void OnUnload(EventArgs e)
        {
            // Purge des messages REPONSE
            RemoveResponseRecipient();
        }
        #endregion OnUnLoad
        #region CreateChildControls
        // EG 20210222 [XXXXX] Suppression inscription function RefreshPage
        // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
        protected override void CreateChildControls()
        {
            //JavaScript.RefreshPage(this);
            JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);
            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));

            // EG 20130725 Timeout sur Block
            JQuery.Block block = new JQuery.Block("OnBoardTracker", "Msg_WaitingRefresh", false)
            {
                Width = "100px",
                Timeout = SystemSettings.GetTimeoutJQueryBlock(frmTracker.Name.ToUpper())
            };
            JQuery.UI.WriteInitialisationScripts((PageBase)this.Page, block);

            HtmlInputHidden hdn = new HtmlInputHidden();
            hdn.Attributes.Add("onclick", "javascript:" + ClientScript.GetPostBackEventReference(this, null));
            this.Page.Controls.Add(hdn);
        }
        #endregion CreateChildControls

        #region SetHead
        /// <summary>
        /// En-tête de page HTML (Title, CSS, METATAG ...)
        /// </summary>
        /// <param name="pPage"></param>
        /// <param name="pTitle"></param>
        /// <remarks>
        /// </remarks> 
        // EG 20180525 [23979] IRQ Processing
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        private void SetHead(PageBase pPage, string pTitle)
        {

            if ((null == pPage.Header) && (null == PageTools.SearchHeadControl(pPage)))
                throw new NotSupportedException("This page is missing a head runat server. Please add <head runat=\"server\" />.");
            //
            Control head;
            if (null != pPage.Header)
                head = (Control)pPage.Header;
            else
                head = (Control)PageTools.SearchHeadControl(pPage);
            // Add Title
            PageTools.SetHeaderTitle(head, "titlePage", pTitle);
            // Meta Tag
            PageTools.SetMetaTag(head);
            // Meta Tag
            //Add linkCssCommon
            PageTools.SetHeaderLink(head, "linkCssAwesome", "~/Includes/fontawesome-all.min.css");
            PageTools.SetHeaderLink(head, "linkCssCommon", "~/Includes/EFSThemeCommon.min.css");
            PageTools.SetHeaderLink(head, "linkCssCustomCommon", "~/Includes/CustomThemeCommon.css");
            PageTools.SetHeaderLink(head, "linkCssUISprites", "~/Includes/EFSUISprites.min.css");

            //Add linkCss
            string cssColor = string.Empty;
            AspTools.CheckCssColor(ref cssColor);
            pPage.CSSColor = cssColor;

            string cssMode = this.CSSMode;
            AspTools.CheckCssMode(ref cssMode);

            PageTools.SetHeaderLink(head, "linkCss", String.Format(@"~/Includes/EFSTheme-{0}.min.css", cssMode));

            JQuery.UI.WriteHeaderLink(head, JQuery.Engines.JQuery);
            PageTools.SetHeaderLink(head, "linkCssTracker", "~/Includes/tracker.min.css");
            //add linkIcon
            string linkImage = @"~/Images/ico/" + ControlsTools.GetIconPageMenu(null) + ".ico";
            PageTools.SetHeaderLink(head, "linkIcon", "shortcut icon", "image/x-ico", linkImage);
            linkImage = linkImage.Replace("ico", "png");
            PageTools.SetHeaderLink(head, "linkIcon2", "shortcut icon", "image/x-png", linkImage);

            //
            HtmlGenericControl style = new HtmlGenericControl("style");
            style.Attributes.Add("type", "text/css");
            style.InnerText = "THEAD { DISPLAY: table-header-group }  TFOOT { DISPLAY: table-footer-group }";
            head.Controls.Add(style);
        }
        #endregion SetHead

        #region OnRefresh
        /// <summary>
        /// Evénement déclenché pour un raffraichissement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRefresh(object sender, EventArgs e)
        {
            LoadData();
        }
        #endregion OnRefresh

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void SwapGroupDetail(object sender, EventArgs e)
        {
            MaskGroup();
        }
        // EG 20200922 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Nouveaux Boutons Favori/Groupes + Diverses Corrections
        private void MaskGroup()
        {
            if (!IsPostBack)
            {
                AspTools.ReadSQLCookie("MaskGroup", out string cookieValue);
                if (StrFunc.IsFilled(cookieValue))
                    hidMaskGroup.Value = cookieValue;
                else
                {
                    hidMaskGroup.Value = "all";
                    AspTools.WriteSQLCookie("MaskGroup", hidMaskGroup.Value);
                }
            }
            else
            {
                AspTools.WriteSQLCookie("MaskGroup", hidMaskGroup.Value);
            }
            btnMaskGroup.Text = String.Format("<i class='fas fa-{0}'></i>", (hidMaskGroup.Value == "all") ? "layer-group" : "star");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20180910 [XXXXX] Add
        protected override void OnPreRenderComplete(EventArgs e)
        {
            AddAuditTimeStep("Start Tracker.OnPreRenderComplete");
            // Affichage de la requête si isTrace =1
            if (IsTrace)
                DisplayLastQuery();

            AddAuditTimeStep("End Tracker.OnPreRenderComplete");

            // FI 20190107 [24431] 
            base.OnPreRenderComplete(e); 
        }

        #region OnAutoRefresh
        /// <summary>
        /// Evénement déclenché pour un raffraichissement automatique de la page (Mode: Enabled/Disabled)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnAutoRefresh(object sender, EventArgs e)
        {
            timerRefresh.Enabled = !timerRefresh.Enabled;
            SessionTools.IsTrackerRefreshActive = timerRefresh.Enabled;
            SetTimerInterval();
        }
        #endregion OnAutoRefresh
        #region SetAttributesTimerRefresh
        /// <summary>
        /// Changement de l'image de raffraichissement automatique du tracker en fonction de son statut
        /// </summary>
        protected void SetAttributesTimerRefresh()
        {
            if (timerRefresh.Enabled)
            {
                btnautorefresh.CssClass = "fa-icon start";
                btnautorefresh.Pty.TooltipContent = Ressource.GetString("StopRefresh");
            }
            else
            {
                btnautorefresh.CssClass = "fa-icon stop";
                btnautorefresh.Pty.TooltipContent = Ressource.GetString(btnautorefresh.Enabled?"StartRefresh":"ActiveRefresh");
            }
        }
        #endregion SetAttributesTimerRefresh

        // EG 20200818 [XXXXX] Plus de bouton pour activer/désactiver le refresh automatique du tracker (à l'image de List.aspx - aller dans les paramètres du tracker pour modification) 
        protected override void SetAutoRefresh()
        {
            timerRefresh.Enabled = (AutoRefresh > 0) && SessionTools.IsTrackerRefreshActive;
            SetTimerInterval();
        }
        protected void SetTimerInterval()
        {
            if (timerRefresh.Enabled)
                timerRefresh.Interval = AutoRefresh * 1000;
            SetAttributesTimerRefresh();
        }
        #region LoadData
        /// <summary>
        /// Chargement du tracker
        /// . Dictionnaires (READYSTATE / GROUP / STATUS / HELP)
        /// . Remplissage des onglets (READYSTATE, HELP) et accordéons (GROUP par READYSTATE)
        /// . Compteurs scrollés
        /// </summary>
        protected void LoadData()
        {
            LoadHeaderData();
            FillHeaderData();
            FillHeaderHelp();
        }
        #endregion LoadData
        #region LoadHeaderData
        /// <summary>
        /// Chargement dictionnaires (READYSTATE / GROUP / STATUS / HELP)
        /// </summary>
        /// FI 20180910 [XXXXX] Modi (Mise en place d'un trace si istrace=1)
        /// EG 20200818 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker via Contrôle Hidden pour stocker la valeur
        /// EG 20210419 [XXXXX] Calcul date histo pour 1D (Décalage d'1 jour ouvré sur la base de la date système et du BC de l'entité en vigueur)
        protected void LoadHeaderData()
        {
            IDataReader dr = null;
            IDbTransaction dbTransaction = null;
            ResetGroup();
            try
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(SessionTools.CS, "ISMARKED", DbType.Boolean), false);

                StrBuilder query = new StrBuilder(SQLCst.SELECT);
                query += "count(*) as TOTAL, tk.READYSTATE, tk.GROUPTRACKER, tk.STATUSTRACKER" + Cst.CrLf;
                query += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRACKER_L.ToString() + " tk" + Cst.CrLf;
                query += SQLCst.WHERE + "(ISMARKED=@ISMARKED)" + Cst.CrLf;

                string histo = hidHisto.Value;
                if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
                {
                    // FI 20200810 [XXXXX] Convertion de la date en UTC
                    //DateTime dtReference = new DtFunc().StringToDateTime("-" + histo);
                    //if (dtReference != DateTime.MinValue)
                    //    dtReference = DtFuncExtended.ConvertTimeToTz(new DateTimeTz(dtReference, SessionTools.GetCriteriaTimeZone()), "Etc/UTC").Date;

                    DateTime dtReference = Tools.GetTrackerDtHisto(SessionTools.CS, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter, histo, SessionTools.GetCriteriaTimeZone());
                    parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.DTINSDATETIME2), dtReference);
                    query += SQLCst.AND + "(" + DataHelper.SQLIsNull(SessionTools.CS, "tk.DTUPD", "tk.DTINS") + " >= @DTINS)";
                }

                //PL 20130703 Newness TEST
                bool isTrackerApplyRestrict = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Spheres_TrackerApplyRestrict"));
                if ((!SessionTools.User.IsSessionSysAdmin) && isTrackerApplyRestrict)
                {
                    parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDAINS), SessionTools.User.IdA);
                    query += SQLCst.AND + "(tk.IDAINS=@IDAINS)" + Cst.CrLf;
                }

                query += SQLCst.GROUPBY + "tk.READYSTATE, tk.GROUPTRACKER, tk.STATUSTRACKER" + Cst.CrLf;

                QueryParameters queryParameters = new QueryParameters(SessionTools.CS, query.ToString(), parameters);

                LastQuery = new Pair<string, TimeSpan>
                {
                    First = DataHelper.SqlCommandToDisplay(SessionTools.CS, queryParameters)
                };

                DatetimeProfiler dtExec = new DatetimeProfiler(DateTime.Now);

                //PL 20200806 Do not use dbTransaction on Oracle, in order not to cause the opening of a new physical connection in the connection pool ADO.NET
                if (DataHelper.IsDbOracle(SessionTools.CS))
                {
                    dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                }
                else
                {
                    dbTransaction = DataHelper.BeginTran(SessionTools.CS, IsolationLevel.ReadUncommitted);
                    dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                }

                int i = 0;
                while (dr.Read())
                {
                    if (i == 0)
                        LastQuery.Second = dtExec.GetTimeSpan();

                    ProcessStateTools.ReadyStateEnum readyState = default;
                    if ((false == Convert.IsDBNull(dr["READYSTATE"]) &&
                        Enum.IsDefined(typeof(ProcessStateTools.ReadyStateEnum), dr["READYSTATE"].ToString())))
                        readyState = (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), dr["READYSTATE"].ToString());

                    Cst.GroupTrackerEnum group = default;
                    if ((false == Convert.IsDBNull(dr["GROUPTRACKER"]) &&
                        Enum.IsDefined(typeof(Cst.GroupTrackerEnum), dr["GROUPTRACKER"].ToString())))
                        group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), dr["GROUPTRACKER"].ToString());

                    // RD 20151026 [21482] Use explicitly 'NA' as default value
                    //ProcessStateTools.StatusEnum status = default(ProcessStateTools.StatusEnum);
                    ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.NA;
                    if ((false == Convert.IsDBNull(dr["STATUSTRACKER"]) &&
                        Enum.IsDefined(typeof(ProcessStateTools.StatusEnum), dr["STATUSTRACKER"].ToString())))
                        status = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), dr["STATUSTRACKER"].ToString());

                    int total = Convert.ToInt32(dr["TOTAL"]);

                    #region readyState
                    m_TrackerList[readyState].Second.Find(match => match.group == group).status.Find(match => match.First == status).Second = total;
                    m_TrackerList[readyState].Second.Find(match => match.group == Cst.GroupTrackerEnum.ALL).status.Find(match => match.First == status).Second += total;
                    #endregion readyState
                    i++;
                }

                if (i == 0)
                    LastQuery.Second = dtExec.GetTimeSpan();

                SetTotalByReadyState();
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    dr.Close();
                }
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
            }
        }
        #endregion LoadHeaderData
        #region FillHeaderData
        /// <summary>
        /// Chargement du tracker en fonction des dictionnaires (READYSTATE / GROUP / STATUS)
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200728 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correctifs et compléments
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Label replace hyperlink on each group label
        protected void FillHeaderData()
        {
            WCTooltipLabel lblCounterStatusReadyState = null;
            WCTooltipLabel lblCounterStatus = null;
            WCTooltipLabel lblCounterStatus2 = null;
            WCTooltipLabel lblGroup = null;
            WCToolTipLinkButton btnContent = null;
            WCToolTipHyperlink lnkCtrl = null;
            Nullable<ProcessStateTools.StatusEnum> status = null;
            Nullable<ProcessStateTools.StatusEnum> statusReadyState = null;
            string imageurl = string.Empty;
            string tooltip = string.Empty;
            string group = string.Empty;
            string resShowDetail = Ressource.GetString("ShowDetails");
            string idSuffix = null;

            btnparam.Pty.TooltipContent = Ressource.GetString("Tools");
            btnobserver.Pty.TooltipContent = Ressource.GetString("ServicesObserver");
            btnfloatbar.Attributes.Add("onclick", "TrackerFloat();return false;");
            btnfloatbar.Pty.TooltipContent = Ressource.GetString("TrackerOnNewWindow");
            btnfloatbar.Style.Add("float", "right");
            btndetail.Attributes.Add("onclick", GetURLTrackerDetail(null, null, null));
            btndetail.Pty.TooltipContent = Ressource.GetString("Zoom");
            btnrefresh.Pty.TooltipContent = Ressource.GetString("Refresh");
            btnmonitoring.Pty.TooltipContent = Ressource.GetString("OnBoardMonitoring");

            foreach (ProcessStateTools.ReadyStateEnum readyState in m_TrackerList.Keys)
            {
                statusReadyState = null;

                #region Onglet Title
                int total = m_TrackerList[readyState].First;
                lnkCtrl = FindControl("readystate_" + readyState.ToString()) as WCToolTipHyperlink;
                if (null != lnkCtrl)
                    lnkCtrl.Text = m_ReadyStateName[readyState].First;
                lblCounterStatusReadyState = FindControl("readystate_cpt_" + readyState.ToString()) as WCTooltipLabel;
                if (null != lblCounterStatusReadyState)
                {
                    lblCounterStatusReadyState.Text = (99 < total) ? "+99" : total.ToString();
                    lblCounterStatusReadyState.CssClass = CSS.SetCssClassTracker(readyState.ToString());
                    lblCounterStatusReadyState.Pty.SetButton("btnclose");
                    lblCounterStatusReadyState.Pty.TooltipTitle = m_ReadyStateName[readyState].First + " : " + total.ToString();
                    lblCounterStatusReadyState.Pty.TooltipContent = GetToolTipCounterForReadyState(readyState);
                }
                #endregion Onglet Title

                m_TrackerList[readyState].Second.ForEach(item =>
                    {
                        status = null;
                        idSuffix = readyState.ToString() + "_" + item.group.ToString();
                        group = item.group.ToString();
                        lblCounterStatus = FindControl("cpt_" + idSuffix) as WCTooltipLabel;
                        lblCounterStatus2 = FindControl("xcpt_" + idSuffix) as WCTooltipLabel;
                        btnContent = FindControl("content_" + idSuffix) as WCToolTipLinkButton;

                        #region Status Icon (onglet et ascenseur)
                        if (null != lblCounterStatus)
                        {
                            if (item.status.Exists(match => (match.First == ProcessStateTools.StatusErrorEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusErrorEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusWarningEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusWarningEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusPendingEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusPendingEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusUnknownEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusUnknownEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusProgressEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusProgressEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusNoneEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusNoneEnum;
                            else if (item.status.Exists(match => (match.First == ProcessStateTools.StatusSuccessEnum) && (match.Second > 0)))
                                status = ProcessStateTools.StatusSuccessEnum;

                            statusReadyState = SetStatusReadyState(statusReadyState, status);
                            lblCounterStatus.Visible = status.HasValue;
                            if (status.HasValue)
                            {
                                Pair<ProcessStateTools.StatusEnum, int> value = item.status.Find(match => match.First == status);
                                lblCounterStatus.Text = (99 < value.Second)?"+99":value.Second.ToString();
                                lblCounterStatus.Attributes.Add("status", status.Value.ToString());
                                lblCounterStatus.CssClass = CSS.SetCssClassTracker("cpt" + status.Value.ToString());
                            }
                        }
                        #endregion Status Icon
                        #region Group Title
                        lblGroup = FindControl("lblGroup_" + idSuffix) as WCTooltipLabel;
                        if (null != lblGroup)
                            lblGroup.Text = m_GroupTrackerName[item.group].First;
                        #endregion Group Title
                        #region Detail Icon
                        if (null != btnContent)
                        {
                            //btnContent.Visible = status.HasValue;
                            btnContent.Visible = lblCounterStatus.Visible;
                            if (status.HasValue)
                            {
                                //btnContent.CssClass = CSS.SetCssClass(CSS.Trk.groupcontent);
                                btnContent.Pty.TooltipContent = Cst.HTMLBold + m_GroupTrackerName[item.group].First + Cst.HTMLEndBold + Cst.CrLf + resShowDetail;
                                btnContent.Attributes.Add("onclick",GetURLTrackerDetail(item.group, readyState, null));
                            }
                        }
                        #endregion Detail Icon

                        if ((null != lblCounterStatus) && status.HasValue)
                        {
                            lblCounterStatus.Pty.SetButton("btnclose");
                            lblCounterStatus.Pty.TooltipTitle = m_GroupTrackerName[item.group].First + " : " + item.status.Sum(elem => elem.Second).ToString();
                            lblCounterStatus.Pty.TooltipContent = GetToolTipCounterForGroup(readyState, item.group);
                        }

                        #region Status 2
                        if (null != lblCounterStatus2)
                        {
                            if ((status == ProcessStateTools.StatusEnum.ERROR) &&
                                 (item.status.Exists(match => (match.First == ProcessStateTools.StatusWarningEnum) && (match.Second > 0))))
                            {
                                status = ProcessStateTools.StatusEnum.WARNING;
                                lblCounterStatus2.Visible = status.HasValue;
                                Pair<ProcessStateTools.StatusEnum, int> value = item.status.Find(match => match.First == status);
                                lblCounterStatus2.Text = (99 < value.Second) ? "+99" : value.Second.ToString();
                                lblCounterStatus2.Attributes.Add("status", status.Value.ToString());
                                lblCounterStatus2.CssClass = CSS.SetCssClassTracker("cpt" + status.Value.ToString());

                                lblCounterStatus2.Pty.SetButton("btnclose");
                                lblCounterStatus2.Pty.TooltipTitle = m_GroupTrackerName[item.group].First + " : " + item.status.Sum(elem => elem.Second).ToString();
                                lblCounterStatus2.Pty.TooltipContent = GetToolTipCounterForGroup(readyState, item.group);
                            }
                            else
                            {
                                status = null;
                            }
                            lblCounterStatus2.Visible = status.HasValue;
                        }

                        #endregion Status 2
                    });
                if (statusReadyState.HasValue)
                    lblCounterStatusReadyState.CssClass = CSS.SetCssClass("cpt" + statusReadyState.Value.ToString().ToLower());
                else
                    lblCounterStatusReadyState.CssClass = CSS.SetCssClass("cptnone");
            }
        }
        #endregion FillHeaderData
        #region SetStatusReadyState
        private Nullable<ProcessStateTools.StatusEnum> SetStatusReadyState(Nullable<ProcessStateTools.StatusEnum> pStatusReadyState, Nullable<ProcessStateTools.StatusEnum> pStatusGroup)
        {
            Nullable<ProcessStateTools.StatusEnum> _status = pStatusReadyState;
            if (false == pStatusReadyState.HasValue)
            {
                _status = pStatusGroup;
            }
            else if (pStatusGroup.HasValue)
            {
                switch (pStatusGroup.Value)
                {
                    case ProcessStateTools.StatusEnum.ERROR:
                        _status = pStatusGroup;
                        break;
                    case ProcessStateTools.StatusEnum.WARNING:
                        if (_status.Value != ProcessStateTools.StatusEnum.ERROR)
                            _status = pStatusGroup;
                        break;
                    case ProcessStateTools.StatusEnum.PENDING:
                        if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) && (_status.Value != ProcessStateTools.StatusEnum.WARNING))
                            _status = pStatusGroup;
                        break;
                    case ProcessStateTools.StatusEnum.NA:
                        if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                            (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                            (_status.Value != ProcessStateTools.StatusEnum.PENDING))
                            _status = pStatusGroup;
                        break;
                    case ProcessStateTools.StatusEnum.PROGRESS:
                        if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                            (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                            (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                            (_status.Value != ProcessStateTools.StatusEnum.NA))
                            _status = pStatusGroup;
                        break;
                    case ProcessStateTools.StatusEnum.NONE:
                        if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                            (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                            (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                            (_status.Value != ProcessStateTools.StatusEnum.PROGRESS) &&
                            (_status.Value != ProcessStateTools.StatusEnum.NA))
                            _status = pStatusGroup;
                        break;
                    default:
                        if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                          (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                          (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                          (_status.Value != ProcessStateTools.StatusEnum.NA) &&
                          (_status.Value != ProcessStateTools.StatusEnum.PROGRESS) &&
                          (_status.Value != ProcessStateTools.StatusEnum.NONE))
                            _status = pStatusGroup;
                        break;
                }
            }
            return _status;
        }

        #endregion SetStatusReadyState
        #region FillHeaderHelp
        /// <summary>
        /// Chargement de l'aide en ligne (GROUP / READYSTATE / STATUS)
        /// </summary>
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Label replace hyperlink on each group label
        /// EG 20220503 [XXXXX] Add Gestion des doublons sur l'onglet Aide (Clear des controles enfant sur panel)
        protected void FillHeaderHelp()
        {
            #region Onglet Title
            WCToolTipHyperlink lnkCtrl = FindControl("tracker_HELP") as WCToolTipHyperlink;
            if (null != lnkCtrl)
                lnkCtrl.Text = "?";
            #endregion Onglet Title

            #region Accordions title
            WCTooltipLabel lblGroup = FindControl("lblgroup_HELP_GROUP") as WCTooltipLabel;
            if (null != lnkCtrl)
                lblGroup.Text = Ressource.GetString("GROUPTRACKER");
            lblGroup = FindControl("lblgroup_HELP_READYSTATE") as WCTooltipLabel;
            if (null != lblGroup)
                lblGroup.Text = Ressource.GetString("ReadyState");
            lblGroup = FindControl("lblgroup_HELP_STATUS") as WCTooltipLabel;
            if (null != lblGroup)
                lblGroup.Text = Ressource.GetString("StatusTracker");

            #endregion Accordions title
            #region Accordions Body

            #endregion Accordions title
            #region Accordions Body
            if (FindControl("detgroup_HELP_GROUP") is Panel pnlCtrl)
            {
                pnlCtrl.Controls.Clear();
                pnlCtrl.Controls.Add(SetHelpGroup());
            }
            pnlCtrl = FindControl("detgroup_HELP_READYSTATE") as Panel;
            if (null != pnlCtrl)
            {
                pnlCtrl.Controls.Clear();
                pnlCtrl.Controls.Add(SetHelpReadyState());
            }
            pnlCtrl = FindControl("detgroup_HELP_STATUS") as Panel;
            if (null != pnlCtrl)
            {
                pnlCtrl.Controls.Clear();
                pnlCtrl.Controls.Add(SetHelpStatus());
            }
            #endregion Accordions Body
        }
        #endregion FillHeaderHelp

        #region SetHelpGroup
        /// <summary>
        /// Aide en ligne GROUP
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Table SetHelpGroup()
        {
            Table tbl = new Table();
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                ColumnSpan = 2,
                Text = Ressource.GetString("HELP_GROUP", string.Empty),
                CssClass = "preamble"
            };
            tr.Cells.Add(td);
            tbl.Rows.Add(tr);
            foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
            {
                if (group != Cst.GroupTrackerEnum.ALL)
                {
                    tr = new TableRow();
                    td = new TableCell();
                    Panel pnlLed = new Panel
                    {
                        CssClass = "fa-icon fas fa-circle gray"
                    };
                    td.Controls.Add(pnlLed);

                    tr.Cells.Add(td);
                    td = new TableCell
                    {
                        Text = m_GroupTrackerName[group].First + " (" + group.ToString().ToLower() + ")",
                        CssClass = "title"
                    };
                    tr.Cells.Add(td);
                    tbl.Rows.Add(tr);

                    tr = new TableRow();
                    td = new TableCell
                    {
                        ColumnSpan = 2,
                        Text = Ressource.GetString("HELP_GROUP_" + group, string.Empty),
                        CssClass = "body"
                    };
                    tr.Cells.Add(td);
                    tbl.Rows.Add(tr);
                }
            }
            return tbl;
        }
        #endregion SetHelpGroup
        #region SetHelpReadyState
        /// <summary>
        /// Aide en ligne READYSTATE
        /// </summary>
        /// <returns></returns>
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Table SetHelpReadyState()
        {
            Table tbl = new Table();
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                ColumnSpan = 2,
                Text = Ressource.GetString("HELP_TRACKERREADYSTATE", string.Empty),
                CssClass = "preamble"
            };
            tr.Cells.Add(td);
            tbl.Rows.Add(tr);

            
            foreach (ProcessStateTools.ReadyStateEnum readyState in Enum.GetValues(typeof(ProcessStateTools.ReadyStateEnum)))
            {
                tr = new TableRow();
                td = new TableCell();

                string color = m_ReadyStateName[readyState].Second.ToLower();
                Panel pnlLed = new Panel
                {
                    CssClass = String.Format("fa-icon fas fa-circle {0}", color)
                };
                td.Controls.Add(pnlLed);

                tr.Cells.Add(td);
                td = new TableCell
                {
                    Text = m_ReadyStateName[readyState].First + " (" + readyState.ToString().ToLower() + ")",
                    CssClass = readyState.ToString()
                };
                tr.Cells.Add(td);
                tbl.Rows.Add(tr);

                tr = new TableRow();
                td = new TableCell
                {
                    ColumnSpan = 2,
                    Text = Ressource.GetString("HELP_TRACKERREADYSTATE_" + readyState.ToString(), string.Empty),
                    CssClass = "body"
                };
                tr.Cells.Add(td);
                tbl.Rows.Add(tr);
            }
            return tbl;
        }
        #endregion SetHelpReadyState
        #region SetHelpStatus
        /// <summary>
        /// Aide en ligne STATUS
        /// </summary>
        /// <returns></returns>
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private Table SetHelpStatus()
        {
            Table tbl = new Table();
            TableRow tr = new TableRow();
            TableCell td = new TableCell
            {
                ColumnSpan = 2,
                Text = Ressource.GetString("HELP_TRACKERSTATUS", string.Empty),
                CssClass = "preamble"
            };
            tr.Cells.Add(td);
            tbl.Rows.Add(tr);

            foreach (ProcessStateTools.StatusEnum status in Enum.GetValues(typeof(ProcessStateTools.StatusEnum)))
            {
                if (status != ProcessStateTools.StatusEnum.NA)
                {
                    string color = m_StatusName[status].Second.ToLower();
                    
                    Panel pnlLed = new Panel
                    {
                        CssClass = String.Format("fa-icon fas fa-circle {0}", color)
                    };

                    tr = new TableRow();

                    td = new TableCell();
                    td.Controls.Add(pnlLed);

                    tr.Cells.Add(td);
                    td = new TableCell
                    {
                        Text = m_StatusName[status].First + " (" + status.ToString().ToLower() + ")",
                        CssClass = status.ToString()
                    };
                    tr.Cells.Add(td);
                    tbl.Rows.Add(tr);

                    tr = new TableRow();
                    td = new TableCell
                    {
                        ColumnSpan = 2,
                        Text = Ressource.GetString("HELP_TRACKERSTATUS_" + status.ToString(), string.Empty),
                        CssClass = "body"
                    };
                    tr.Cells.Add(td);
                    tbl.Rows.Add(tr);
                }
            }
            return tbl;
        }
        #endregion SetHelpStatus

        #region GetToolTipCounterForGroup
        /// <summary>
        /// Gestion des Tooltip par READYSTATE/GROUP
        /// </summary>
        /// <param name="pReadyState"></param>
        /// <param name="pGroupTracker"></param>
        /// <returns></returns>
        private string GetToolTipCounterForGroup(ProcessStateTools.ReadyStateEnum pReadyState, Cst.GroupTrackerEnum pGroupTracker)
        {
            StringWriter sw = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(sw);
            PlaceHolder plh = new PlaceHolder();
            Label lbl = null;
            GroupTracker groupTracker = m_TrackerList[pReadyState].Second.Find(match => match.group == pGroupTracker);
            if (null != groupTracker)
            {
                groupTracker.status.ForEach(item =>
                {
                    if (0 < item.Second)
                    {
                        lbl = new Label
                        {
                            Text = m_StatusName[item.First].First + " : " + item.Second.ToString(),
                            CssClass = "cpt " + item.First.ToString()
                        };
                        // Link sur Tracker DETAIL
                        lbl.Attributes.Add("onclick", GetURLTrackerDetail(groupTracker.group, pReadyState, item.First));
                        plh.Controls.Add(lbl);
                        plh.Controls.Add(new LiteralControl(Cst.CrLf));
                    }
                });
            }
            plh.RenderControl(writer);
            // EG 20160404 Migration vs2013
            //return sw.ToString();
            return HttpUtility.HtmlDecode(sw.ToString());
        }
        #endregion GetToolTipCounterForGroup
        #region GetToolTipCounterForReadyState
        /// <summary>
        /// Gestion des Tooltip par READYSTATE
        /// </summary>
        /// <param name="pReadyState"></param>
        /// <param name="pGroupTracker"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private string GetToolTipCounterForReadyState(ProcessStateTools.ReadyStateEnum pReadyState)
        {
            StringWriter sw = new StringWriter();
            HtmlTextWriter writer = new HtmlTextWriter(sw);
            PlaceHolder plh = new PlaceHolder();
            Label lblTitle = null;
            Label lbl = null;
            int totalGroup = 0;
            int pos = 0;

            m_TrackerList[pReadyState].Second.ForEach(group =>
                {
                    totalGroup = 0;
                    if (Cst.GroupTrackerEnum.ALL != group.group)
                    {
                        group.status.ForEach(status =>
                        {
                            if (0 < status.Second)
                            {
                                lbl = new Label
                                {
                                    Text = m_StatusName[status.First].First + " : " + status.Second.ToString(),
                                    CssClass = "cpt " + status.First.ToString()
                                };
                                // Link sur Tracker DETAIL
                                lbl.Attributes.Add("onclick", GetURLTrackerDetail(group.group, pReadyState, status.First));
                                plh.Controls.Add(lbl);
                                plh.Controls.Add(new LiteralControl(Cst.CrLf));
                                totalGroup += status.Second;
                                
                            }
                        });
                        if (group.status.Exists(match => match.Second > 0))
                        {
                            lblTitle = new Label
                            {
                                Text = m_GroupTrackerName[group.group].First + " : " + totalGroup.ToString(),
                                CssClass = "cpt subtitle"
                            };
                            // Link sur Tracker DETAIL
                            lblTitle.Attributes.Add("onclick", GetURLTrackerDetail(group.group, pReadyState, null));

                            plh.Controls.Add(new LiteralControl(Cst.CrLf));
                            plh.Controls.AddAt(pos, lblTitle);
                            plh.Controls.AddAt(pos + 1, new LiteralControl(Cst.CrLf));
                            plh.Controls.AddAt(pos + 1, new LiteralControl(Cst.CrLf));

                            pos = plh.Controls.Count;
                        }
                    }
                });
            if (0 < plh.Controls.Count)
            {
                plh.RenderControl(writer);
                // EG 20190123 Upd
                return HttpUtility.HtmlDecode(sw.ToString());
            }
            else
                return Ressource.GetString("ERROR");
        }
        #endregion GetToolTipCounterForReadyState

        #region LoadViewState
        /// <summary>
        /// Lecture des groupes/status dans le ViewState
        /// </summary>
        /// <param name="savedState"></param>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void LoadViewState(object savedState)
        {
            object[] viewState = (object[])savedState;
            
            base.LoadViewState(viewState[0]);

            m_GroupTrackerName = (Dictionary<Cst.GroupTrackerEnum, Pair<string, bool>>)viewState[1];
            m_ReadyStateName = (Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>>)viewState[2];
            m_StatusName = (Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>>)viewState[3];

            //m_TrackerList = (Dictionary<ProcessStateTools.ReadyStateEnum, Pair<int, List<GroupTracker>>>)viewState[5];
            if ((null == m_TrackerList) || ("SELFRELOAD_" == Request.Params["__EVENTARGUMENT"]))
            {
                hidHisto.Value = SessionTools.TrackerHistoric;
                CreateReadyStateData();
                DisplayGroup();
                LoadData();
            }
        }
        #endregion LoadViewState
        #region SaveViewState
        /// <summary>
        /// Sauvegarde des groupes/status dans le ViewState
        /// </summary>
        /// <returns></returns>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override object SaveViewState()
        {
            if (HttpContext.Current == null)
                return null;
            //
            object[] ret = new object[4];
            ret[0] = base.SaveViewState();
            ret[1] = m_GroupTrackerName;
            ret[2] = m_ReadyStateName;
            ret[3] = m_StatusName;
            return ret;
        }
        #endregion SaveViewState
        #region ResetGroup
        /// <summary>
        /// RAZ des compteurs pour chaque readystate/status
        /// </summary>
        private void ResetGroup()
        {
            if (null != m_TrackerList)
            {
                foreach (ProcessStateTools.ReadyStateEnum readyState in Enum.GetValues(typeof(ProcessStateTools.ReadyStateEnum)))
                {
                    if (m_TrackerList.ContainsKey(readyState))
                    {
                        m_TrackerList[readyState].First = 0;
                        m_TrackerList[readyState].Second.ForEach(group => { group.status.ForEach(item => { item.Second = 0; }); });
                    }
                }
            }
        }
        #endregion ResetGroup

        #region CreateReadyStateData
        /// <summary>
        ///  Création de la liste des groupes de traitements pour chaque readyState
        ///  Création de la liste des status pour chaque groupe de traitements
        /// </summary>
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// FI 20201102 [XXXXX] Usage de ReflectionTools.GetEnumResource et de ReflectionTools.GetAttribute
        private void CreateReadyStateData()
        {
            m_TrackerList = new Dictionary<ProcessStateTools.ReadyStateEnum, Pair<int, List<GroupTracker>>>();
            m_GroupTrackerName = new Dictionary<Cst.GroupTrackerEnum, Pair<string,bool>>();
            m_ReadyStateName = new Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>>();
            m_StatusName = new Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>>();

            List<GroupTracker> _lstGroup = null;
            
            long powerGroupSelect = SessionTools.TrackerGroupDetail;
            foreach (ProcessStateTools.ReadyStateEnum readyState in Enum.GetValues(typeof(ProcessStateTools.ReadyStateEnum)))
            {
                if (false == m_ReadyStateName.ContainsKey(readyState))
                {
                    ProcessStateTools.HelpAssociateAttribute help = ReflectionTools.GetAttribute<ProcessStateTools.HelpAssociateAttribute>(readyState);
                    m_ReadyStateName[readyState] = new Pair<string, string>()
                    {
                        First = ReflectionTools.GetEnumResource<ProcessStateTools.ReadyStateEnum>(readyState, string.Empty),
                        Second = (help != null) ? help.ColorName : "Black"
                    };
                }
                foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
                {
                    string hexValue = Enum.Format(typeof(Cst.GroupTrackerEnum), group, "x");
                    bool isSelected = (0 < (powerGroupSelect & int.Parse(hexValue, System.Globalization.NumberStyles.HexNumber)));

                    if (false == m_GroupTrackerName.ContainsKey(group))
                    {
                        #region Création du Groupe
                        m_GroupTrackerName.Add(group, new Pair<string,bool>(group.ToString(),isSelected));
                        m_GroupTrackerName[group].First = ReflectionTools.GetEnumResource<Cst.GroupTrackerEnum>(group, string.Empty);
                        #endregion Création du Groupe
                    }
                    foreach (ProcessStateTools.StatusEnum status in Enum.GetValues(typeof(ProcessStateTools.StatusEnum)))
                    {
                        if (false == m_StatusName.ContainsKey(status))
                        {
                            ProcessStateTools.HelpAssociateAttribute help = ReflectionTools.GetAttribute<ProcessStateTools.HelpAssociateAttribute>(status);
                            m_StatusName[status] = new Pair<string, string>
                            {
                                First = ReflectionTools.GetEnumResource<ProcessStateTools.StatusEnum>(status, "TRACKERSTATUS_"),
                                Second = (help != null) ? help.ColorName : "Black"
                            };
                        }

                        if (false == m_TrackerList.ContainsKey(readyState))
                        {
                            _lstGroup = new List<GroupTracker>
                            {
                                new GroupTracker(group, status)
                            };
                            m_TrackerList.Add(readyState, new Pair<int, List<GroupTracker>>(0, _lstGroup));
                        }
                        else if (false == m_TrackerList[readyState].Second.Exists(match => match.group == group))
                        {
                            m_TrackerList[readyState].Second.Add(new GroupTracker(group, status));
                        }
                        else
                        {
                            GroupTracker groupTracker = m_TrackerList[readyState].Second.Find(match => match.group == group);
                            groupTracker.status.Add(new Pair<ProcessStateTools.StatusEnum, int> { First = status, Second = 0 });
                        }
                    }
                }
            }
        }
        #endregion CreateReadyStateData
        #region SetTotalByReadyState
        /// <summary>
        /// Alimentation des compteurs TOTAUX par READYSTATE
        /// </summary>
        private void SetTotalByReadyState()
        {
            if ((null != m_TrackerList) && (0 < m_TrackerList.Count))
            {
                foreach (ProcessStateTools.ReadyStateEnum readyState in m_TrackerList.Keys)
                {
                    int total = 0;
                    m_TrackerList[readyState].Second.ForEach(item => 
                    {
                        if (item.group == Cst.GroupTrackerEnum.ALL)
                            item.status.ForEach(status => { total += status.Second; });
                    });
                    m_TrackerList[readyState].First = total;
                }
            }
        }
        #endregion SetTotalByReadyState

        #region DisplayGroup
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayGroup()
        {
            if (hidMaskGroup.Value != "all")
            {
                Panel pnlGroup = null;
                m_GroupTrackerName.ToList().ForEach(group =>
                {
                    if (group.Key != Cst.GroupTrackerEnum.ALL)
                    {
                        string id = "group_";
                        m_ReadyStateName.ToList().ForEach(readyState =>
                        {
                            pnlGroup = FindControl(id + readyState.Key + "_" + group.Key) as Panel;
                            if (null != pnlGroup)
                                pnlGroup.Visible = group.Value.Second;
                        });
                    }
                });
            }
        }
        #endregion DisplayGroup

        #region IsTrackerAlertProcess
        /// <summary>
        /// Un message de réponse est-il attendu pour ce traitement
        /// </summary>
        /// <param name="pProcessType">Traitement</param>
        /// <returns></returns>
        private bool IsTrackerAlertProcess(Cst.ProcessTypeEnum pProcessType)
        {
            int i = int.Parse(Enum.Format(typeof(Cst.ProcessTypeEnum), pProcessType, "d"));
            return (0 < (m_TrackerAlertProcess & Convert.ToInt64(Math.Pow(2, i))));
        }
        #endregion IsTrackerAlertProcess
        #region CallBackFileWatcher
        /// <summary>
        /// Lecture et affichage des messages de réponse en mode FileWatcher
        /// </summary>
        private void CallBackFileWatcher()
        {

            if (StrFunc.IsFilled(FullPathRecipient))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(FullPathRecipient);
                if (directoryInfo.Exists)
                {
                    FileInfo[] fileInfos = directoryInfo.GetFiles(FilterFile);
                    int i = 0;
                    if (ArrFunc.IsFilled(fileInfos))
                    {
                        foreach (FileInfo fileInfo in fileInfos)
                        {
                            ResponseRequestMQueue queue = (ResponseRequestMQueue)MQueueTools.ReadFromFile(fileInfo.FullName);
                            FileTools.FileDelete2(fileInfo.FullName);
                            if (IsTrackerAlertProcess(queue.requestProcess))
                            {
                                if (i <= 10)
                                    AddDialogMessage(queue);
                                else if (i == 11)
                                    break;
                                i++;
                            }
                        }
                    }
                    DisplayDialogMessage();
                }
            }

        }
        #endregion CallBackFileWatcher

        #region AddDialogMessage
        /// <summary>
        /// Ajout d'un message de réponse en fonction d'un message de réponse MQueue
        /// </summary>
        /// <param name="pQueue">Message de réponse</param>
        // EG 20180525 [23979] IRQ Processing
        private void AddDialogMessage(ResponseRequestMQueue pQueue)
        {

            Cst.ProcessTypeEnum process = pQueue.requestProcess;
            string dtTracker = string.Empty;
            if (pQueue.header.requesterSpecified)
            {
                // FI 20201106 [XXXXX] Affichage de dtTracker en fonction du profil 
                //dtTracker =  DtFunc.DateTimeToString(pQueue.header.requester.date, DtFunc.FmtDateLongTime);
                dtTracker = DtFuncExtended.DisplayTimestampUTC(pQueue.header.requester.date, new AuditTimestampInfo
                {
                    Collaborator = SessionTools.Collaborator,
                    TimestampZone = SessionTools.AuditTimestampZone,
                    Precision = Cst.AuditTimestampPrecision.Minute
                });


                // FI 20201106 [XXXXX] 
                //if (pQueue.header.requester.idPROCESSSpecified)
                //{

                //    ProcessLogQuery processLogQuery = new ProcessLogQuery(m_CS);
                //    QueryParameters qry = processLogQuery.GetQuerySelectPROCESS_L(m_CS);
                //    qry.Parameters["IDPROCESS_L"].Value = pQueue.header.requester.idPROCESS;

                //    using (IDataReader dr = DataHelper.ExecuteReader(m_CS, CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter()))
                //    {
                //        if (dr.Read())
                //        {
                //            if (Enum.IsDefined(typeof(Cst.ProcessTypeEnum), dr["PROCESS"].ToString()))
                //                process = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), dr["PROCESS"].ToString(), true);
                //        }
                //    }
                //}
            }



            string message;
            if (pQueue.idInfoSpecified && ArrFunc.IsFilled(pQueue.idInfo.idInfos) &&
                (process == Cst.ProcessTypeEnum.IO))
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_IOTASK",
                    dtTracker,
                    pQueue.GetStringValueIdInfoByKey("identifier"),
                    pQueue.GetStringValueIdInfoByKey("displayName"));
            else if (process == Cst.ProcessTypeEnum.POSKEEPREQUEST)
            {
                message = GetMessageResponse(pQueue, dtTracker);
            }
            else
            {
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_GEN",
                    Ressource.GetString(process.ToString()),
                    dtTracker,
                    pQueue.nbMessage.ToString());
            }
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
            if (0 < pQueue.nbError)
            {
                status = ProcessStateTools.StatusErrorEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_ERROR");
            }
            else if (0 < pQueue.nbWarning)
            { 
                status = ProcessStateTools.StatusWarningEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_WARNING");
            }
            else if (0 < pQueue.nbNone)
            {
                status = ProcessStateTools.StatusNoneEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_NONE");
            }
            if (pQueue.nbMessage > (pQueue.nbError + pQueue.nbWarning + pQueue.nbSucces))
            {
                status = ProcessStateTools.StatusWarningEnum;
                message += Ressource.GetString("Msg_PROCESS_RESPONSE_INCOMPLET");
            }
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "white");
                writer.AddAttribute(HtmlTextWriterAttribute.Target, Cst.HyperLinkTargetEnum._blank.ToString());
                // EG 20170125 [Refactoring URL] Upd
                //writer.AddAttribute(HtmlTextWriterAttribute.Href, PageTools.PageToCall(Cst.OTCml_TBL.TRACKER_L.ToString(), pQueue.idTRK_L.ToString(), string.Empty));
                writer.AddAttribute(HtmlTextWriterAttribute.Href, SpheresURL.GetURL(IdMenu.Menu.TRACKER_L, pQueue.idTRK_L.ToString()));
                writer.RenderBeginTag(HtmlTextWriterTag.A);  // Start Tag A
                writer.Write(Ressource.GetString("ShowLog"));
                writer.RenderEndTag();  // End Tag A
                AddAdditionalLinkToResponse(writer, pQueue);
            }
            m_DialogMessage.Add(new JQuery.DialogMessage(status, message, stringWriter.ToString()));

        }
        #endregion AddDialogMessage
        #region DisplayDialogMessage
        /// <summary>
        /// Affichage du message final de réponse en fonction d'un message de réponse MQueue
        /// </summary>
        /// <param name="pQueue"></param>
        private void DisplayDialogMessage()
        {
            int i = 1;
            if ((null != m_DialogMessage) && (0 < m_DialogMessage.Count))
            {
                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;

                StringWriter stringWriter = new StringWriter();
                using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
                {
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);  // Start Tag Table
                    foreach (JQuery.DialogMessage dialog in m_DialogMessage)
                    {
                        if (status != ProcessStateTools.StatusErrorEnum)
                        {
                            if (dialog.Status == ProcessStateTools.StatusErrorEnum)
                                status = ProcessStateTools.StatusErrorEnum;
                            else if (dialog.Status == ProcessStateTools.StatusWarningEnum)
                                status = ProcessStateTools.StatusWarningEnum;
                            else if (dialog.Status == ProcessStateTools.StatusUnknownEnum)
                                status = ProcessStateTools.StatusUnknownEnum;
                        }
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Start Tag Tr
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, dialog.Status.ToString().ToLower(), true);
                        writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                        writer.Write(dialog.Message.Replace(Cst.CrLf, Cst.HTMLBreakLine));
                        writer.RenderEndTag(); // End Tag Td

                        writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                        writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                        writer.Write(dialog.Link);
                        writer.RenderEndTag(); // End Tag Td
                        writer.RenderEndTag(); // End Tag Tr
                        if (i < m_DialogMessage.Count)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Tr); // Start Tag Tr
                            writer.AddAttribute(HtmlTextWriterAttribute.Colspan, "2", true);
                            writer.RenderBeginTag(HtmlTextWriterTag.Td); // Start Tag Td
                            writer.Write(Cst.HTMLSpace);
                            writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                            writer.RenderEndTag();
                            writer.RenderEndTag(); // End Tag Td
                            writer.RenderEndTag();
                        }
                        i++;
                    }
                    writer.RenderEndTag(); // End Tag Table 
                }
                JQuery.OpenDialog openDialog = new JQuery.OpenDialog(Ressource.GetString("REQUESTERRESPONSE"), stringWriter.ToString(), status)
                {
                    MaxHeight = 250,
                    MaxWidth = 400,
                    Height = "200",
                    Width = "400"
                };
                JQuery.UI.WriteInitialisationScripts(this, openDialog);
            }
        }
        #endregion DisplayDialogMessage
        #region OpenResponseRecipient
        /// <summary>
        /// Lecture des messages de réponse
        /// </summary>
        /// EG 20101109 Delete test m_IsRequesterAlert. 
        /// La queue est toujours à l'écoute mais le message sera traité ou non dans la procédure : OnCallBackMSMQueue
        private void OpenResponseRecipient()
        {
            // EG 20131211 Test null sur m_SendInfo
            try
            {
                if ((null != m_SendInfo) && (m_SendInfo.IsInfoValid))
                {
                    if (IsFileWatcher)
                    {
                        if (m_IsTrackerAlert)
                            CallBackFileWatcher();
                    }
                    else if (IsMsMQueue)
                    {
                        m_QueueResponse = new MessageQueue(FullPathRecipient);
                        IAsyncResult ar = m_QueueResponse.BeginPeek(TimeSpan.FromSeconds(3.0), messageNo++, new AsyncCallback(OnCallBackMSMQueue));
                        Thread.Sleep(TimeSpan.FromSeconds(3.0));
                    }
                }
            }
            catch (Exception ex)
            {
                base.WriteLogException(ex);
                DisplayErrorMessage(ex);
            }
        }
        #endregion OpenResponseRecipient
        #region DisplayErrorMessage
        private void DisplayErrorMessage(Exception pEx)
        {
            bool isDisplayMessage = true;
            if (pEx.GetType() == typeof(MessageQueueException))
            {
                MessageQueueException mqex = (MessageQueueException)pEx;
                if (mqex.MessageQueueErrorCode == MessageQueueErrorCode.IOTimeout || mqex.MessageQueueErrorCode == MessageQueueErrorCode.MessageAlreadyReceived)
                    isDisplayMessage = false;
            }
            if (isDisplayMessage)
            {
                string message = $"An error occurred while processing your request.{Cst.HTMLBreakLine}{Cst.HTMLBreakLine}Message: {ExceptionTools.GetMessageExtended(pEx)}";

                JQuery.OpenDialog openDialog = new JQuery.OpenDialog("Tracker", message, ProcessStateTools.StatusEnum.ERROR)
                {
                    Height = "150",
                    MaxHeight = 250,
                    MaxWidth = 400
                };
                JQuery.UI.WriteInitialisationScripts(this, openDialog);
                JQuery.Dialog dialog = new JQuery.Dialog("Tracker")
                {
                    Width = "240",
                    Height = "200"
                };
                JQuery.UI.WriteInitialisationScripts(this, dialog);
            }
        }
        #endregion
        #region RemoveResponseRecipient
        /// <summary>
        /// Suppression des anciens messages de réponse (traité ou non ) inférieur à date jour
        /// </summary>
        private void RemoveResponseRecipient()
        {
            try
            {
                if (IsFileWatcher)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(FullPathRecipient);
                    if (directoryInfo.Exists)
                    {
                        FileInfo[] fileInfos = directoryInfo.GetFiles(RemoveFilterFile);
                        if (ArrFunc.IsFilled(fileInfos))
                        {
                            foreach (FileInfo fileInfo in fileInfos)
                            {
                                DateTime dtFile = new DtFunc().StringyyyyMMddToDateTime(fileInfo.Name.Substring(16, 8));
                                if (dtFile.Date < DateTime.Now.Date)
                                    FileTools.FileDelete2(fileInfo.FullName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                base.WriteLogException(ex);
                DisplayErrorMessage(ex);
            }
        }
        #endregion RemoveResponseRecipient
        #region AddAdditionalLinkToResponse
        /// <summary>
        /// Ajout des hyperlink pour accès au LOG et infos IO ou POSREQUEST
        /// </summary>
        /// <param name="pWriter"></param>
        /// <param name="pQueue"></param>
        // EG 20170125 [Refactoring URL] Upd
        // EG 20201030 [XXXXX] Ajout PARAM1 sur IOTRACK (&P1=IDPROCESS_L) 
        private void AddAdditionalLinkToResponse(HtmlTextWriter pWriter, ResponseRequestMQueue pQueue)
        {
            // EG 20170125 [Refactoring URL] Upd
            // string pageName = string.Empty;
            // string id = string.Empty;
            //if (pQueue.requestProcess == Cst.ProcessTypeEnum.IO)
            //{
            //    pageName = Cst.OTCml_TBL.IOTRACK.ToString();
            //    id = pQueue.idProcess.ToString();
            //}
            //else if (pQueue.requestProcess == Cst.ProcessTypeEnum.POSKEEPREQUEST)
            //{
            //    pageName = Cst.OTCml_TBL.POSREQUEST.ToString();
            //    id = pQueue.idTRK_L.ToString();
            //}

            string url = string.Empty;
            string id = pQueue.idProcess.ToString();
            if (pQueue.requestProcess == Cst.ProcessTypeEnum.IO)
                url = SpheresURL.GetURL(IdMenu.Menu.IOTRACK, id, null, SpheresURL.LinkEvent.href, Cst.ConsultationMode.Normal, "&P1=IDPROCESS_L", null);
            else if (pQueue.requestProcess == Cst.ProcessTypeEnum.POSKEEPREQUEST)
            {
                url = SpheresURL.GetURL(IdMenu.Menu.TrackerPosRequest, pQueue.idTRK_L.ToString(), pQueue.id.ToString(), 
                    SpheresURL.LinkEvent.href, Cst.ConsultationMode.Normal, null, null);            
            }

            if (StrFunc.IsFilled(url))
            {
                pWriter.RenderBeginTag(HtmlTextWriterTag.Br);
                pWriter.RenderEndTag();
                pWriter.AddAttribute(HtmlTextWriterAttribute.Class, "white");
                pWriter.AddAttribute(HtmlTextWriterAttribute.Target, Cst.HyperLinkTargetEnum._blank.ToString());
                pWriter.AddAttribute(HtmlTextWriterAttribute.Href, url);
                pWriter.RenderBeginTag(HtmlTextWriterTag.A);  // Start Tag A
                pWriter.Write(Ressource.GetString("ShowResult"));
                pWriter.RenderEndTag();  // End Tag A
            }
        }
        #endregion AddAdditionalLinkToResponse

        #region GetURLTrackerDetail
        /// <summary>
        /// Construction de l'hyperlink pour accèder au LOG détail du tracker en fonction des paramètres optionels:
        /// . GROUP
        /// . READYSTATE
        /// . STATUS
        /// </summary>
        /// <param name="pGroupTracker"></param>
        /// <param name="pReadyState"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200818 [XXXXX] Réduction Accès ReadSQLCookie (TrackerHistoric) sur Tracker via Contrôle Hidden pour stocker la valeur
        private string GetURLTrackerDetail(Nullable<Cst.GroupTrackerEnum> pGroupTracker,
                                           Nullable<ProcessStateTools.ReadyStateEnum> pReadyState,
                                           Nullable<ProcessStateTools.StatusEnum> pStatus)
        {
            string groupTracker = "null";
            string readyState = "null";
            string statusTracker = "null";
            string dateTracker = "null";
            string urlParam1 = "null";
            string title = string.Empty;
            if (pGroupTracker.HasValue && (pGroupTracker.Value != Cst.GroupTrackerEnum.ALL))
            {
                groupTracker = "'" + pGroupTracker.Value.ToString() + "'";
                title = m_GroupTrackerName[pGroupTracker.Value].First;
            }
            if (pReadyState.HasValue)
            {
                readyState = "'" + pReadyState.Value.ToString() + "'";
                title += (StrFunc.IsFilled(title) ? " - " : "") + m_ReadyStateName[pReadyState.Value].First;
            }
            if (pStatus.HasValue)
            {
                statusTracker = "'" + pStatus.Value.ToString() + "'";
                title += (StrFunc.IsFilled(title) ? " - " : "") + m_StatusName[pStatus.Value].First;
            }


            // FI 20200602[25370] La date est systématiquement alimentée depuis  TrackerHistoric
            // Rq cette date peut etre null si Beyond
            string histo = hidHisto.Value;
            Nullable<DateTime> dtReference = null;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
                dtReference = new DtFunc().StringToDateTime("-" + histo);

            if (dtReference.HasValue)
                dateTracker = "'" + DtFuncML.DateTimeToStringDateISO(dtReference.Value) + "'";

            // RD 20130118 [18349] Ajout "&P1=ONLYSIO" pour Vision et Portail
            if (false == Software.IsSoftwareOTCmlOrFnOml())
                urlParam1 = "'" + IdMenu.Param_ONLYSIO + "'";

            // FI 20200602[25370] isLoadData s'il existe au minimum un filtre
            Boolean isLoadData = (pGroupTracker.HasValue && (pGroupTracker.Value != Cst.GroupTrackerEnum.ALL)) || pReadyState.HasValue || pStatus.HasValue;


            string url = "TrackerDetail(" + isLoadData.ToString().ToLower() + "," + urlParam1 + "," + groupTracker + "," + readyState + "," + statusTracker + "," + dateTracker + ",'" + title + "');return false;";
            return url;
        }

        #endregion GetURLTrackerDetail
        #region OnCallBackMSMQueue
        /// <summary>
        /// Lecture des messages de réponse à partir d'une queue MSMQUEUE
        /// </summary>
        /// <param name="ar"></param>
        /// EG 20101109 Ajout test m_IsRequesterAlert sur l'ajout d'un message. On n'affiche que 10 réponse dans un message et on sort.
        protected void OnCallBackMSMQueue(IAsyncResult ar)
        {
            MessageEnumerator mqe = null;
            try
            {
                Message msg = m_QueueResponse.EndPeek(ar);
                string lblMessage = string.Empty;
                if (StrFunc.IsFilled(m_SessionID))
                {
                    int i = 0;
                    mqe = m_QueueResponse.GetMessageEnumerator2();
                    mqe.Reset();
                    bool isContinue = mqe.MoveNext();
                    if (isContinue)
                    {
                        while (isContinue)
                        {
                            lblMessage = mqe.Current.Label;
                            if (lblMessage.IndexOf("_" + m_SessionID) > -1)
                            {
                                msg = mqe.RemoveCurrent();
                                ResponseRequestMQueue queue = (ResponseRequestMQueue)MQueueTools.ReadFromMessage(msg);
                                if (m_IsTrackerAlert && IsTrackerAlertProcess(queue.requestProcess))
                                {
                                    AddDialogMessage(queue);
                                    if (i == 11)
                                        isContinue = false;
                                    i++;

                                }
                            }
                            else
                                isContinue = mqe.MoveNext();
                        }
                    }

                }
            }
            finally
            {
                if (null != mqe)
                    mqe.Dispose();
                m_QueueResponse.BeginPeek();
            }
        }
        #endregion OnCallBackMSMQueue

        #region GetMessageResponse
        /// <summary>
        /// Structuration du message de réponse posté par le service de tenue de Position en fonction du type RequestType
        /// </summary>
        /// <param name="pQueue">Le message de réponse</parparam>
        /// <param name="pDtRequester">La date de la demande</param>
        // EG 20231129 [WI762] End of Day processing : Possibility to request processing without initial margin(Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin)
        private string GetMessageResponse(ResponseRequestMQueue pQueue, string pDtRequester)
        {
            string message;
            Nullable<Cst.PosRequestTypeEnum> requestType = pQueue.GetEnumValueIdInfoByKey<Cst.PosRequestTypeEnum>("REQUESTTYPE");
            // EG 20151102 [21465]
            if (requestType.HasValue && (requestType.Value != default))
            {
                string resResquestType = Ressource.GetString(requestType.ToString());
                string trade = pQueue.GetStringValueIdInfoByKey("TRADE");
                if (StrFunc.IsFilled(trade) && (Cst.PosRequestTypeEnum.UpdateEntry != requestType.Value))
                {
                    // Identifiant Trade
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_TRADE",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("TRADE"),
                        pQueue.nbMessage.ToString());
                }
                else if ((Cst.PosRequestTypeEnum.EndOfDay == requestType) ||
                         (Cst.PosRequestTypeEnum.EndOfDayWithoutInitialMargin == requestType) ||
                         (Cst.PosRequestTypeEnum.ClosingDay == requestType) ||
                         (Cst.PosRequestTypeEnum.RemoveEndOfDay == requestType))
                {
                    // Entité/Chambre
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_ENTITYCSSCUSTODIAN",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("ENTITY"),
                        pQueue.GetStringValueIdInfoByKey("CSSCUSTODIAN"),
                        pQueue.GetStringValueIdInfoByKey("DTBUSINESS"),
                        pQueue.nbMessage.ToString());

                }
                else
                {
                    // Clé de position
                    message = Ressource.GetString2("Msg_RESPONSE_POSREQUEST_KEYPOS",
                        Ressource.GetString(pQueue.requestProcess.ToString()),
                        resResquestType,
                        pDtRequester,
                        pQueue.GetStringValueIdInfoByKey("MARKET"),
                        pQueue.GetStringValueIdInfoByKey("ASSET"),
                        pQueue.GetStringValueIdInfoByKey("DEALER") + " [" +
                        pQueue.GetStringValueIdInfoByKey("BOOKDEALER") + "]",
                        pQueue.GetStringValueIdInfoByKey("CLEARER") + " [" +
                        pQueue.GetStringValueIdInfoByKey("BOOKCLEARER") + "]",
                        pQueue.nbMessage.ToString());
                }
            }
            else
            {
                // EG 20151102 [21465] Ajout Message générique du tracker
                string trackerMessage = GetTrackerMessage(pQueue.idTRK_L, SessionTools.Collaborator_Culture_ISOCHAR2);
                message = Ressource.GetString2("Msg_PROCESS_RESPONSE_GEN",
                    Ressource.GetString(pQueue.ProcessType.ToString()) + (StrFunc.IsFilled(trackerMessage) ? "\r\n\r\n" + trackerMessage : string.Empty),
                    pDtRequester,
                    pQueue.nbMessage.ToString());
            }
            return message;
        }
        #endregion GetMessageResponse
        #region GetTrackerMessage
        private string GetTrackerMessage(int pIdTRK_L, string pCulture)
        {
            string message = string.Empty;

            IDataReader dr = null;
            IDbTransaction dbTransaction = null;
            try
            {
                dbTransaction = DataHelper.BeginTran(m_CS, IsolationLevel.ReadUncommitted);
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(m_CS, "IDTRK_L", DbType.Int64, SQLCst.UT_ENUM_MANDATORY_LEN), pIdTRK_L);
                parameters.Add(new DataParameter(m_CS, "CULTURE", DbType.AnsiString, SQLCst.UT_CULTURE_LEN), pCulture);

                string sqlQuery = @"select tk.IDTRK_L, tk.SYSCODE, tk.SYSNUMBER, case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'Request' else" + Cst.CrLf;
                sqlQuery += DataHelper.SQLIsNull(m_CS, "smd.SHORTMESSAGE", "smd_gb.SHORTMESSAGE") + @"end as SHORTMESSAGETRACKER, 
                case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'No Message' else" + Cst.CrLf;
                sqlQuery += DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS, DataHelper.SQLReplace(m_CS,
                            DataHelper.SQLIsNull(m_CS, "smd.MESSAGE", "smd_gb.MESSAGE"),
                            "'{1}'", DataHelper.SQLIsNull(m_CS, "tk.DATA1", "'{1}'")),
                            "'{2}'", DataHelper.SQLIsNull(m_CS, "tk.DATA2", "'{2}'")),
                            "'{3}'", DataHelper.SQLIsNull(m_CS, "tk.DATA3", "'{3}'")),
                            "'{4}'", DataHelper.SQLIsNull(m_CS, "tk.DATA4", "'{4}'")),
                            "'{5}'", DataHelper.SQLIsNull(m_CS, "tk.DATA5", "'{5}'"));
                sqlQuery += @"end as MESSAGETRACKER
                from dbo.TRACKER_L tk
                left outer join dbo.SYSTEMMSG sm on (sm.SYSCODE = tk.SYSCODE) and (sm.SYSNUMBER = tk.SYSNUMBER)
                left outer join dbo.SYSTEMMSGDET smd on (sm.SYSCODE = sm.SYSCODE) and (smd.SYSNUMBER = sm.SYSNUMBER) and (smd.CULTURE = @CULTURE)
                left outer join dbo.SYSTEMMSGDET smd_gb on (smd_gb.SYSCODE = sm.SYSCODE) and (smd_gb.SYSNUMBER = sm.SYSNUMBER) and (smd_gb.CULTURE = 'en')
                where (tk.IDTRK_L = @IDTRK_L)" + Cst.CrLf;

                QueryParameters qryParameters = new QueryParameters(m_CS, sqlQuery.ToString(), parameters);

                dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                if (dr.Read())
                    message = dr["MESSAGETRACKER"].ToString();
            }
            catch (Exception) { throw; }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
                if (null != dbTransaction)
                    DataHelper.RollbackTran(dbTransaction);
            }
            return message;
        }
        #endregion GetTrackerMessage



        /// <summary>
        /// Affichage de la trace SQL
        /// </summary>
        /// FI 20180910 [XXXXX] Add
        private void DisplayLastQuery()
        {

            if (null != LastQuery)
            {
                string WindowID = FileTools.GetUniqueName("LoadTracker", "lastQuery");
                string write_File = SessionTools.TemporaryDirectory.MapPath("Datagrid") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "Datagrid" + @"/" + WindowID + ".xml";

                XmlDocument xmlDoc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("Last query use, File: {0}", write_File);
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("lastQuery");
                xmlDoc.AppendChild(xmlRoot);
                //
                xmlRoot.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                //FI 20160810 [XXXXX] le format {0:hh:mm:ss.fff} ne fonctionne pas => remplacer par @"{0:hh\:mm\:ss\.fff}"
                xmlRoot.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", LastQuery.Second);

                xmlRoot.AppendChild(xmlDoc.CreateCDataSection(LastQuery.First));
                //
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
                {
                    Indent = true
                };
                XmlWriter xmlWritter = XmlTextWriter.Create(write_File, xmlWriterSettings);
                //
                xmlDoc.Save(xmlWritter);
                //   
                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }
        }
        #endregion Methods
    }
    #endregion Tracker Page
}
