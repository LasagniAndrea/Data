using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.CommonCSV;
using EFS.Controls;
using EFS.Process;
using EFS.Referential;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Ear;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Configuration;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de [!output SAFE_CLASS_NAME].
    /// </summary>
    /// FI 20200602 [25370] Implemente ITemplateDataGridPage
    public partial class LstReferentialPage : PageBase, ITemplateDataGridPage
    {


        /// <summary>
        /// "OnProcessClick"
        /// </summary>
        const string Target_OnProcessClick = "OnProcessClick";

        /// <summary>
        ///  Représente les exports Excel
        /// </summary>
        private enum ExportExcelType
        {
            /// <summary>
            /// Exportation Excel du grid
            /// </summary>
            ExportGrid,
            /// <summary>
            /// Exportation Excel du jeu de donné
            /// </summary>
            ExportDataset,
        }

        /// <summary>
        /// Représente les valeurs de retour de la méthode ProcessRemoveAlloc
        /// </summary>
        private enum ProcessRemoveAllocReturn
        {
            /// <summary>
            /// Le trade a été traité correctement
            /// </summary>
            Ok,
            /// <summary>
            /// Le trade n'a pas été traité, il est reservé
            /// </summary>
            AlreadyReserved,
            /// <summary>
            /// Le trade n'a pas été traité, il est déjà supprimé
            /// </summary>
            AlreadyRemoved,
            /// <summary>
            /// Le trade n'a pas été traité, il ne possède pas d'évènements
            /// </summary>
            NoEvent,
            /// <summary>
            /// 
            /// </summary>
            /// FI 20190903 [24872] Add
            Locked,
            /// <summary>
            /// 
            /// </summary>
            /// FI 20190903 [24872] Add
            UnvalidDateBusiness,
        }

        #region Members
        /// <summary>
        /// Liste des erreurs inattendues rencontrées (lorsque on est en mode sans Echec)
        /// </summary>
        private List<SpheresException2> _alException;

        /// <summary>
        /// Liste des messages d'erreurs affiché sur le render
        /// </summary>
        private ArrayList _alErrorMessages;

        /// <summary>
        ///  liste des dynamicArgument 
        /// </summary>
        /// FI 20200205 [XXXXX] Usage de ReferentialsReferentialStringDynamicData
        private Dictionary<string, ReferentialsReferentialStringDynamicData> _dynamicArgs;

        /* FI 20200224 [XXXXX] Mis en commentaires
        /// <summary>
        ///  Obtient ou définit les _customObjects associés 
        /// </summary>
        private CustomObjects _customObjects;
        */
        /// <summary>
        /// 
        /// </summary>
        protected string mainTitle, subTitle, subTitleRight;
        protected string mainMenuClassName;
        //
        private bool m_IsProcess;
        private bool m_IsSpheresProcess;
        private bool m_IsConsultation;
        // EG 20091110
        private string m_ProcessBase;
        private string m_ProcessType;
        private string m_ProcessName;

        // FI 20240119 [WI819] Add
        private Boolean m_IsProcessExecuted;

        /// <summary>
        ///  
        /// </summary>
        /// FI 20200602 [25370] Add
        private Boolean isInitReferential;

        /// <summary>
        ///  si true, indique que les valeurs des contrôles CustomObjects seront initialisés à partir de LSTPARAM
        ///  <PARAM>Ce cas se produit lorsque l'utilisateur choisit un nouveau modèle de consultation</PARAM>
        /// </summary>
        /// FI 20200602 [25370] Add
        private Boolean isLoadLSTParam;


        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property InfosLstWhere
        /// </summary>
        /// FI 20200225 [XXXXX] add
        private string DataCacheKeyInfosLstWhere
        {
            get { return BuildDataCacheKey("InfosLstWhere"); }
        }
        /// <summary>
        /// critère courant
        /// </summary>
        /// FI 20190103 [XXXXX] Add
        /// FI 20200224 [XXXXX] Mis en place d'un maintient d'état
        /// FI 20200225 [XXXXX] Usage de DataCache pour le maintient d'état
        private InfosLstWhere[] InfosLstWhere
        {
            get
            {
                return DataCache.GetData<InfosLstWhere[]>(DataCacheKeyInfosLstWhere);
            }
            set
            {
                DataCache.SetData<InfosLstWhere[]>(DataCacheKeyInfosLstWhere, value);
            }
        }

        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property InfosLstOrderBy
        /// </summary>
        /// FI 20200225 [XXXXX] add
        private string DataCacheKeyInfosOrderBy
        {
            get { return BuildDataCacheKey("InfosLstOrderBy"); }
        }

        /// <summary>
        /// Tri courant
        /// </summary>
        /// FI 20191003 [XXXX] Add
        /// FI 20200224 [XXXXX] Misen place d'un maintient d'état
        /// FI 20200225 [XXXXX] Usage de DataCache pour le maintient d'état
        private ArrayList[] InfosLstOrderBy
        {
            get
            {
                return DataCache.GetData<ArrayList[]>(DataCacheKeyInfosOrderBy);
            }
            set
            {
                DataCache.SetData<ArrayList[]>(DataCacheKeyInfosOrderBy, value);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private int m_IDIOTask;


        // EG 20130613 Menu Export Change type ImageButton to object 
        private object m_toolbar_Clicked;
        private LinkButton m_btnRunProcess_Clicked;
        /* FI 202
        /// <summary>
        /// Contrôle HiddenField qui contient l'identifiant unique pour la page
        /// </summary>
        private HiddenField _hiddenFieldGUID;
         */
        #endregion Members

        #region accessors
        // EG 20190411 [ExportFromCSV] To close BlockUI
        protected string ExportTokenValue
        {
            get
            {
                string hiddenValue = string.Empty;
                HtmlInputHidden ctrl = (HtmlInputHidden)this.FindControl("exportToken_value");
                if (null != ctrl)
                    hiddenValue = ctrl.Value;
                return hiddenValue;
            }
        }
        /// <summary>
        /// Obtient le process courant 
        /// <para>Obtient Cst.ProcessTypeEnum.NA si la page n'est pas en mode process</para>
        /// </summary>
        protected Cst.ProcessTypeEnum SpheresProcessType
        {
            get
            {
                Cst.ProcessTypeEnum ret = Cst.ProcessTypeEnum.NA;
                if (m_IsSpheresProcess)
                    ret = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), m_ProcessName.ToUpper(), true);
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit le mode sans Echec
        /// </summary>
        public bool IsNoFailure
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient les dynamics arguments 
        /// </summary>
        protected Dictionary<string, ReferentialsReferentialStringDynamicData> DynamicArgs
        {
            get
            {
                return _dynamicArgs;
            }
        }

        /// <summary>
        /// Obtient la clé d'accès à la variable session de maintient d'état de la property CustomObjects
        /// </summary>
        /// FI 20200225 [XXXXX] Add
        private string DataCacheKeyCustomObjects
        {
            get { return BuildDataCacheKey("CustomObjects"); }
        }

        /// <summary>
        /// Obtient ou définit les customObjects, les customObjects sont déclarés ds le fichier GUI.xm
        /// </summary>
        /// FI 20200224 [XXXXX] Mise en place d'un maintient d'état
        /// FI 20200225 [XXXXX] Usage de DataCache pour le maintient d'état
        public CustomObjects CustomObjects
        {
            get
            {
                return DataCache.GetData<CustomObjects>(DataCacheKeyCustomObjects);
            }
            set
            {
                DataCache.SetData<CustomObjects>(DataCacheKeyCustomObjects, value);
            }
        }
        /// <summary>
        ///  Obtient true lorsque qu'il est possible de lancer un Process Spheres (Service) alors qu'aucune ligne n'est selectionée
        ///  <para>Si tel est le cas, le service NormMsgFactory est sollicité</para>
        /// </summary>
        /// FI 20180605 [24001] Add
        private Boolean IsSpheresProcessTypeUsingNormMsgFactory
        {
            get
            {
                Boolean ret = m_IsSpheresProcess;
                ret &= this.efsdtgRefeferentiel.IsDataAvailable;
                ret &= (efsdtgRefeferentiel.DsData.Tables[0].Columns.Count == 0);
                // FI 2019103 [XXXX] il ne faut pas de critère
                ret &= ArrFunc.IsEmpty(this.InfosLstWhere);

                // Seuls les EARS sont actuellement gérés
                if (ret)
                {
                    switch (SpheresProcessType)
                    {
                        case Cst.ProcessTypeEnum.EARGEN:
                        case Cst.ProcessTypeEnum.ACCOUNTGEN:
                            // NormMsgFactory nécessite le paramètre ENTITY soit renseigné avec une valeur > 0
                            ret &= (this.FindControl(Cst.DDL + "ENTITY") is DropDownList ddl) && (IntFunc.IntValue(ddl.SelectedItem.Value) > 0);
                            break;
                        default:
                            ret = false;
                            break;
                    }
                }
                return ret;
            }
        }



        /// <summary>
        ///  Obtient true si (sessionAdmin ou isTrace=true dans la config ou dans l'URL ou si la check chkSQLTrace est cochée)  
        /// </summary>
        /// FI 20191104 Add
        public override bool IsTrace
        {
            get
            {
                return base.IsTrace || chkSQLTrace.Checked;
            }
        }
        /// <summary>
        ///  Retourne  l'identifiant du contrôle si publication de la page suite à la modification d'un contrôle CustomObject
        /// </summary>
        /// FI 20200602 [25370]
        public string ClientIdCustomObjectChanged
        {
            get;
            private set;
        }

        /// <summary>
        /// Retourne true si publication de la page suite à désactivation des critères optionnels  
        /// </summary>
        /// FI 20200602 [25370] Refactoring
        public bool IsOptionalFilterDisabled
        {
            get
            {
                return IsPostBack && StrFunc.IsFilled(PARAM_EVENTTARGET) && PARAM_EVENTTARGET == "lnkDisabledFilter";
            }
        }


        /// <summary>
        /// Retourne la position du critère optionnel désactivé si publication de la page suite à désactivation d'un critère optionnel
        /// <para>Retourne -1 dans les autres cas</para>
        /// </summary>
        /// FI 20200602 [25370] Refactoring
        public int PositionFilterDisabled
        {
            get
            {
                int ret = -1;
                if (IsPostBack && PARAM_EVENTTARGET.StartsWith("imgDisabledFilterPosition"))
                    ret = StrFunc.GetSuffixNumeric2(PARAM_EVENTTARGET);
                return ret;
            }
        }


        #endregion
        #region Methods
        /// <summary>
        /// 
        /// </summary>
        // EG 20190123 DataGridSelectAll |DataGridCheckedChanged in ControlsTools.js
        // EG 20210222 [XXXXX] Suppression de tous les appls de fonctions dans Javascript.cs (désormais présente dans PageBase.js)
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ControlsTools.js"));
            // FI 20221026 [XXXXX] Add
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/List.min.js"));

            // FI 20210408 [XXXXX] ajout référence à ReferentialCommon
            // Nécessaire pour l'appel à la function JS GetReferential
            if (this.efsdtgRefeferentiel.IsGridSelectMode)
                ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));

            base.CreateChildControls();
        }

        /// <summary>
        /// Retourne la classe CSS pour ajuster les checks sur les grid de traitement avec le FreezeGrid
        /// </summary>
        // EG 20210505 [25700] FreezeGrid implementation 
        private string GetFreezeCheckColumnCssClass()
        {
            string ret;
            switch (efsdtgRefeferentiel.ObjectName.ToLower())
            {
                case "poskeeping_denbulk":
                    ret = "sel1314";
                    break;
                case "poskeeping_bulk":
                case "poskeeping_transferbulk":
                case "transferbulk":
                    ret = "sel2304";
                    break;
                default:
                    ret = "sel-1304"; // Valeur par défaut = (margin: -1px 3px 0px 4px;)
                    break;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// EG 20120327 Classe Utilisée dans le calcul des frais manquants (Ticket 17706)
        /// FI 20170306 [22225] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200923 [XXXXX] Add LoadDynamicArgRunTime post chargement de ceux dispo en GUI
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        // EG 20210505 [25700] FreezeGrid implementation 
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start LstReferentialPage.OnInit");

            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //PL 20181205 Test in progress... Test successfully !
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
            //this.Server.ScriptTimeout = 120; //Override "executionTimeout" on web.config
            //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

            try
            {
                base.OnInit(e);

                if (StrFunc.IsFilled(efsdtgRefeferentiel.IDMenu))
                    mainMenuClassName = ControlsTools.MainMenuName(efsdtgRefeferentiel.IDMenu);
                else if (0 < Request.QueryString.Count)
                    mainMenuClassName = Request.QueryString.Keys[0].ToLower();

                if (mainMenuClassName == "consultation")
                    mainMenuClassName = "repository";

                divDtg.CssClass = mainMenuClassName + " " + GetFreezeCheckColumnCssClass();

                // FI 20200225 [XXXXX] call CheckSessionState
                CheckSessionState();

                // FI 20190327 [24568][24588] 
                // Mise en commentaire de l'usage de la méthode GetPostBackControl. 
                // Cela amène la dégradation suivante: pagination et tri ne fonctionne plus 
                /*
                // FI 20190318 [24568][24588] Initialisation isModeExportExcel dès le OnInit de manière à ce que le grid passe en Template Column
                Control co = ((PageBase)this.Page).GetPostBackControl();
                if (null!= co &&  co.ID == "imgMSExcel")
                    efsdtgRefeferentiel.isModeExportExcel = true;
                */

                // FI 20221114 [XXXX] Add ToolTip and LiteralControl
                Panel panelFiller = FindControl("tbFiller") as Panel;
                // ToolTip = "Filler" => pour appliquer CSS div[id^="tblist"] > div[title="Filler"] (voir EFSThemeCommon.css)
                panelFiller.ToolTip = "Filler";
                // new LiteralControl(Cst.HTMLSpace) => pour appliquer la règle CSS  div[id^=tblist] > div:not(:first-child):not(:last-child):not(:empty) (voir EFSThemeCommon.css)
                panelFiller.Controls.Add(new LiteralControl(Cst.HTMLSpace));


                chkSQLTrace.Enabled = IsTraceTimeAvailable;
                chkSQLTrace.Visible = chkSQLTrace.Enabled;
                chkSQLTrace.Text = Ressource.GetString("chkSQLTrace");
                chkSQLTrace.TextAlign = TextAlign.Left;

                /* FI 20200602 [25370] Mise en commentaire
                // FI 20190327 [24568][24588] 
                if (__EVENTTARGET == "imgMSExcel")
                    efsdtgRefeferentiel.isModeExportExcel = true;
                */
                _alException = new List<SpheresException2>();
                _alErrorMessages = new ArrayList();
                IsNoFailure = true;

                /* FI 20200602 [25370] Mise en commentaire
                efsdtgRefeferentiel.isNoFailure = isNoFailure;
                */

                InitializeComponent();
                // 20090127 FI Ticket 16460 => Ajout systematique d'un do_Postback
                this.Form = this.frmConsult;
                AntiForgeryControl();
                AddInputHiddenAutoPostback();
                AddInputHiddenGUID();

                AbortRessource = true;

                m_IsConsultation = StrFunc.IsFilled(Request.QueryString[Cst.ListType.Consultation.ToString()]);

                #region IsProcess
                m_IsProcess = false;
                // EG 20091110
                m_ProcessBase = Request.QueryString["ProcessBase"];
                m_ProcessType = Request.QueryString["ProcessType"];
                m_ProcessName = Request.QueryString["ProcessName"];

                //
                if (StrFunc.IsFilled(m_ProcessType) && StrFunc.IsFilled(m_ProcessName))
                {
                    m_IsProcess = Cst.ListProcess.IsListProcess(m_ProcessType);
                    if (m_IsProcess)
                    {
                        m_IsSpheresProcess = Cst.ListProcess.IsService(m_ProcessType);
                        if (m_IsSpheresProcess && (Cst.ProcessTypeEnum.NA == SpheresProcessType))
                        {
                            m_IsProcess = false;
                            m_IsSpheresProcess = false;
                        }
                    }
                }
                #endregion IsProcess

                #region IOTASK
                m_IDIOTask = Convert.ToInt32(Request.QueryString["IOTask"]);
                #endregion IOTASK

                #region Initialize datagrid
                efsdtgRefeferentiel.LoadDataError += new LoadDataErrorEventHandler(OnLoadDataError);

                efsdtgRefeferentiel.IsCheckboxColumn = m_IsProcess;
                if (m_IsProcess)
                {
                    //Cas particuliers pour INVOICINGGEN et FEESCALCULATION
                    if (m_IsSpheresProcess &&
                        (
                            (Cst.ProcessTypeEnum.INVOICINGGEN == SpheresProcessType) ||
                            (Cst.ProcessTypeEnum.ACTIONGEN == SpheresProcessType)
                        /*||  FI 20170306 [22225] mise en commentaire
                        (Cst.ProcessTypeEnum.FEESCALCULATION == SpheresProcessType)*/
                        )
                    )
                        efsdtgRefeferentiel.IsCheckboxColumn = false;

                    //Si m_IsProcess: Pas de pagination personalisée, mais chargement complet du jeu de résulat.
                    //NB: Ceci est nécessaire pour que le post des messages s'applique à toutes les lignes du jeu de résultat.
                    efsdtgRefeferentiel.PagingType = TemplateDataGrid.PagingTypeEnum.NativePaging;
                }

                /* FI 20200602 [25370] Mise en commentaire
                if (this.__EVENTTARGET == "lnkDisabledFilter")
                    efsdtgRefeferentiel.isApplyOptionalFilter = false;
                // EG 20160308 Migration vs2013
                if (StrFunc.IsFilled(this.__EVENTTARGET) && this.__EVENTTARGET.StartsWith("imgDisabledFilterPosition"))
                    efsdtgRefeferentiel.positionFilterDisabled = StrFunc.GetSuffixNumeric2(this.__EVENTTARGET);
                */
                #endregion Initialize datagrid

                #region CustomObjects
                // FI 20200224 [XXXXX] LoadCustomObjects() appelé unquement  si false == Page.IsPostBack 
                // du fait de l'existence d'un maintient d'état
                if (false == Page.IsPostBack)
                    LoadCustomObjects();
                // Create GUI && First initialisation GUI
                if (null != CustomObjects)
                {
                    // FI 20200602 [25370] call BuildCustomObjectsControl
                    BuildCustomObjectsControl();
                    if (!IsPostBack)
                    {
                        // FI 20200602 [25370]  Instruction Linq
                        //for (int i = 0; i < ArrFunc.Count(CustomObjects.customObject); i++)
                        //    InitControlCustomObject(CustomObjects.customObject[i]);
                        // Alimente 
                        foreach (CustomObject co in CustomObjects.customObject.Where(x => x.IsControlData))
                            SetCustomObjectControlDefault(co);
                    }
                }
                #endregion CustomObjects

                /*
                #region Add un identifiant unique sur la page [on ne prend pas pagebase.GUID car ici est non valorisée lors d'un postback]
                _hiddenFieldGUID = new HiddenField();
                _hiddenFieldGUID.ID = "__GUID";
                Form.Controls.Add(_hiddenFieldGUID);

                if (!IsPostBack)
                    _hiddenFieldGUID.Value = GUID; //ici pagebase.GUID est valorisé si !IsPostBack uniquement
                else
                    _hiddenFieldGUID.Value = Request.Form["__GUID"];
                #endregion
                */

                LoadDynamicArg();
                // FI 20201201 [XXXXX] Chargement des DynamicArgRunTime avant efsdtgRefeferentiel.LoadReferential (
                LoadDynamicArgRunTime();

                // FI 20210305 [XXXXX] Call SetClientIdCustomObjectChanged
                SetClientIdCustomObjectChanged();

                InitIsInitReferential();

                isLoadLSTParam = (false == IsPostBack) || (PARAM_EVENTTARGET == "LstOpenPage");
                efsdtgRefeferentiel.LoadReferential(DynamicArgs, isInitReferential, isLoadLSTParam);


                // FI 20200224 [XXXXX]Appel à LoadInfosLst uniquement quand nécessaire
                if (isInitReferential)
                    LoadInfosLst();

                // FI 20190910 [24914] Add
                if (efsdtgRefeferentiel.Referential.ScriptTimeoutSpecified && efsdtgRefeferentiel.Referential.ScriptTimeout > Server.ScriptTimeout)
                    Server.ScriptTimeout = efsdtgRefeferentiel.Referential.ScriptTimeout;

                // RD 20110520 [17464]
                if (efsdtgRefeferentiel.LocalLstConsult.template.IsRefreshIntervalSpecified)
                    AutoRefresh = efsdtgRefeferentiel.LocalLstConsult.template.REFRESHINTERVAL;

                //FI 20141016 [XXXXX] focus sur imgRefresh
                if (false == IsPostBack)
                    SetFocus(imgRefresh);
                else if (StrFunc.IsFilled(this.PARAM_EVENTARGUMENT) && (PARAM_EVENTARGUMENT.ToUpper() == "SELFRELOAD_"))
                    SetFocus(imgRefresh);

                //FI 20150302 [XXXXX] DefaultButton sur imgRefresh
                //Par défaut le l'appuie sur le bonton entrée provoque le rafraîchissement du grid
                this.frmConsult.DefaultButton = imgRefresh.ClientID;

            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End LstReferentialPage.OnInit");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("Start LstReferentialPage.OnPreRender");

            try
            {
                this.DataBind();
               
                //Si des modifications ont été effectués sur le grid; on disable [Critères] et [Rafraîchir] et enable [Enregistrer] et [Annuler]
                imgViewerCriteria.Enabled = (false == efsdtgRefeferentiel.IsDataModified) && (false == efsdtgRefeferentiel.IsLocked);

                imgRefresh.Enabled = (false == efsdtgRefeferentiel.IsDataModified) && (false == efsdtgRefeferentiel.IsLocked);

                imgAddNew.Enabled = (false == efsdtgRefeferentiel.IsLocked);
                imgRecord.Enabled = (true == efsdtgRefeferentiel.IsDataModified) && (false == efsdtgRefeferentiel.IsLocked);
                imgCancel.Enabled = (true == efsdtgRefeferentiel.IsDataModified) && (false == efsdtgRefeferentiel.IsLocked);

                if (IsMonoProcessGenerate(SpheresProcessType))
                {
                    if (FindControl("txtSearch") is TextBox txtSearch)
                    {
                        if (txtSearch.Text.Length > 0)
                            imgRunProcess.Style.Add("opacity", "0.4");
                        else
                            imgRunProcess.Style.Remove("opacity");
                        imgRunProcess.Enabled = SessionTools.IsActionEnabled && (txtSearch.Text.Length == 0);
                    }
                }

                // FI 20240119 [WI819] Call RunProcess
                if (Target_OnProcessClick == PARAM_EVENTTARGET && StrFunc.IsFilled(PARAM_EVENTARGUMENT) && "ProcessCancel" != PARAM_EVENTARGUMENT)
                    RunProcess();

                base.OnPreRender(e);
            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End LstReferentialPage.OnPreRender");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20190411 [ExportFromCSV]
        protected override void OnPreRenderComplete(EventArgs e)
        {
            AddAuditTimeStep("Start LstReferentialPage.OnPreRenderComplete");

            try
            {

                // FI 20210115 [XXXXX] Call DisplayLastRefrech
                DisplayLastRefresh();

                // FI 20210115 [XXXXX] Call DisplayZip
                DisplayZip();

                // EG 20141230 [20350]
                // base.OnPreRenderComplete(e);
                //DisplayLastQuery est effectué ici car le chargement de la source de donnée est effectué sur le OnPreRender du datagrid
                if (IsTrace)
                {
                    DisplayPreSelectCommand();
                    DisplayLastQuery();
                }

                //ExportZip,_isExportZip,ExportSQL effectués ici car le chargement de la source de donnée est effectué sur le OnPreRender du datagrid
                if (null != m_toolbar_Clicked)
                {
                    if (m_toolbar_Clicked is Control control)
                    {
                        switch (control.ID)
                        {
                            case "imgSQL":
                                ExportSQL();
                                break;
                            case "imgZIP":
                                // FI 20211228 [XXXXX] ExportZIP remplacé par ExportZIPAsync
                                //ExportZIP();
                                break;
                            case "imgMSExcel":
                                ExportExcel(ExportExcelType.ExportGrid);
                                break;
                            case "imgCSV":
                                ExportCSV();
                                break;
                        }
                    }
                    // EG 20130613 Menu Export Changement de type : skmMenu.MenuItemClickEventArgs
                    else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs mnu)
                    {
                        if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                            ExportPdf(mnu.Argument);
                    }
                }

                if (null != m_btnRunProcess_Clicked)
                {
                    //Poste d'une confirmation en cas de demande de traitement
                    CheckAndConfirmRequest();
                }

                if (m_IsProcess)
                {
                    if (m_IsProcessExecuted)
                    {
                        // FI 20141021 [20350] call SetRequestTrackBuilderListProcess
                        SetRequestTrackBuilderListProcess();
                    }
                }
                else
                {
                    // FI 20141021 [20350] call SetRequestTrackBuilderListLoad
                    SetRequestTrackBuilderListLoad();
                }


                //FI 20141021 [20350] base.OnPreRenderComplete est désormais call en fin de proc
                base.OnPreRenderComplete(e);
            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End LstReferentialPage.OnPreRenderComplete");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                AddAuditTimeStep("Start LstReferentialPage.Render");

                DisplayInfo();

                AddAuditTimeStep("End LstReferentialPage.Render");

                base.Render(writer);
            }
            catch (Exception ex) { TrapException(ex); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20090921  Encode URL Parameters (idLstTemplate)
        /// FI 20140630 [20101] upd
        // EG 20190411 [ExportFromCSV] Add CSV button and Upd others (for Font Awesome)
        // EG 20200720 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        // EG 20201126 [25556] Gestion CSSColor Header Datagrid en provenance de LSTTEMPLATE
        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        // EG 20210204 [25590] Nouvelle gestion des mimeTypes autorisés dans l'upload d'un fichier
        // EG 20210222 [XXXXX] Appel RefreshPage (présentes dans PageBase.js) en remplacement de RefreshPage2
        // EG 20210322 [25556] Correction CSS sur consultation LST(la couleur est associée au menu, example : gray+ repository)
        // EG 20210505 [25700] FreezeGrid implementation 
        // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
        // EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
        protected void Page_Load(object sender, System.EventArgs e)
        {
            AddAuditTimeStep("Start LstReferentialPage.Page_Load");

            try
            {
                /* Il n'y a plus alimentation de  Data_CacheName
                // FI 20130417 [18596] Data_CacheName est désormais initialisé une fois pour toute
                //efsdtgRefeferentiel.Data_CacheName = ((PageBase)this.Page).GUID;
                */
                PageTools.SetHead(this, "Referential", null, efsdtgRefeferentiel.IDMenu);

                string url = string.Empty;
                string urlParms = string.Empty;
                string urlParmsAdd = string.Empty;
                string urlWhere = string.Empty;
                string urlParentGUID = string.Empty;
                //PL 20121126 Rename templateName to idLstConsult 
                string idLstConsult = string.Empty;
                string idLstTemplate = string.Empty;
                int idA_TemplateOwner = 0;

                string title_ConsultationType = string.Empty;
                string title_SpheresMenu = string.Empty;

                // FI 20200602 [25370] Call UpdateCustomObjectsControl
                UpdateCustomObjectsControl();

                if (StrFunc.IsFilled(Request.QueryString["TitleRes"]))
                    title_ConsultationType = Ressource.GetString(Request.QueryString["TitleRes"], true);
                else
                    title_ConsultationType = Ressource.GetString(efsdtgRefeferentiel.Title, string.Empty, true);

                if (m_IsConsultation)
                {
                    #region Mode CONSULTATION
                    if (StrFunc.IsEmpty(title_ConsultationType))
                        title_ConsultationType = Ressource.GetString("Consultation", "Consultation", true);
                    title_SpheresMenu = efsdtgRefeferentiel.LocalLstConsult.Title;

                    //subTitle = "[" + efsdtgRefeferentiel.localLstConsult.template.titleDisplayName + "]";

                    idLstConsult = Request.QueryString["Consultation"];
                    idLstTemplate = efsdtgRefeferentiel.LocalLstConsult.template.IDLSTTEMPLATE;
                    idA_TemplateOwner = efsdtgRefeferentiel.LocalLstConsult.template.IDA;

                    urlWhere = "&Type=" + Cst.ListType.Repository.ToString();
                    #endregion Mode CONSULTATION
                }
                else
                {
                    #region Mode REFERENTIAL / PROCESS / ...
                    title_SpheresMenu = Ressource.GetMenu_Fullname(efsdtgRefeferentiel.IDMenu.ToString(), efsdtgRefeferentiel.TitleMenu);
                    // EG 20151019 [21465] New Complément de ressource titre pour un menu avec plusieurs parents
                    if (SessionTools.Menus.HasSeveralParent(efsdtgRefeferentiel.IDMenu))
                    {
                        EFS.Common.Web.Menu.Menu mnuParent = SessionTools.Menus.GetMenuParentById(efsdtgRefeferentiel.IDMenuSys);
                        if (null != mnuParent)
                            title_SpheresMenu += " - " + Ressource.GetMenu_Fullname(mnuParent.IdMenu);
                    }

                    if (StrFunc.IsEmpty(title_SpheresMenu) || "null" == title_SpheresMenu)
                    {
                        if (StrFunc.IsFilled(efsdtgRefeferentiel.Referential.Ressource))
                            title_SpheresMenu = Ressource.GetString(efsdtgRefeferentiel.Referential.Ressource, true);
                        else
                            title_SpheresMenu = Ressource.GetString(efsdtgRefeferentiel.ObjectName, true);
                    }

                    string consultName = Request.QueryString["P1"];
                    consultName += efsdtgRefeferentiel.ObjectName;
                    idLstConsult = ReferentialWeb.PrefixForReferential + efsdtgRefeferentiel.Title + consultName;
                    idLstTemplate = efsdtgRefeferentiel.IdLstTemplate;
                    idA_TemplateOwner = efsdtgRefeferentiel.IdA;

                    urlWhere = "&DDL=" + HttpUtility.UrlEncode(efsdtgRefeferentiel.sessionName_LstColumn) + "&Type=" + HttpUtility.UrlEncode(efsdtgRefeferentiel.Title);
                    #endregion Mode REFERENTIAL / PROCESS
                }

                if (null != efsdtgRefeferentiel.LocalLstConsult)
                {
                    subTitle = GetSubTitle(efsdtgRefeferentiel.LocalLstConsult.template).Item1;
                    subTitleRight = GetSubTitle(efsdtgRefeferentiel.LocalLstConsult.template).Item2;
                }

                mainTitle = title_ConsultationType + ": <b>" + title_SpheresMenu + "</b>";
                urlParentGUID = "&ParentGUID=" + this.GUID;

                divalltoolbar.Attributes.Add("class", this.CSSMode + " " + mainMenuClassName);
                divbody.Attributes.Add("class", this.CSSMode + " " + mainMenuClassName);

                #region Additional CheckBox
                // RD 20120131 
                // Affichage des données valides et des données mises à jour aujourd’hui 
                if (IsPostBack == false)
                {
                    InitDdlValidityAndTodayUpdate();
                }
                //
                efsdtgRefeferentiel.Referential.isValidDataOnly = efsdtgRefeferentiel.Referential.ExistsColumnsDateValidity && (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.ActivatedData.ToString());
                efsdtgRefeferentiel.Referential.isUnValidDataOnly = efsdtgRefeferentiel.Referential.ExistsColumnsDateValidity && (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.DeactivatedData.ToString());

                efsdtgRefeferentiel.Referential.isDailyNewDataOnly = (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayCreateData.ToString() ||
                    (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayData.ToString()));
                efsdtgRefeferentiel.Referential.isDailyUpdDataOnly = (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayUpdateData.ToString() ||
                    (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayData.ToString()));
                efsdtgRefeferentiel.Referential.isDailyUserNewDataOnly = (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayUserCreateData.ToString() ||
                    (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayUserData.ToString()));
                efsdtgRefeferentiel.Referential.isDailyUserUpdDataOnly = (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayUserUpdateData.ToString() ||
                    (ddlValidityData.SelectedValue == AdditionalCheckBoxEnum2.TodayUserData.ToString()));
                #endregion

                #region Button properties
                string resModele = Ressource.GetString("ViewerModel_Title");

                imgRefresh.Visible = true;
                imgRefresh.Pty.TooltipContent = Ressource.GetString("btnRefresh") + Cst.GetAccessKey(imgRefresh.AccessKey);
                imgRefresh.Attributes.Add("onclick", StrFunc.AppendFormat("RefreshPage('{0}','SELFRELOAD_');return false;", imgRefresh.ClientID));

                imgAddNew.Visible = false;
                imgAddNew.Text = @"<i class='fa fa-plus-circle'></i>" + Ressource.GetString("btnAddNew");

                string resProcess = GetResourceBtnProcess();
                imgRunProcess.Visible = m_IsProcess;
                imgRunProcess.Enabled = SessionTools.IsActionEnabled;
                imgRunProcess.Text = @"<i class='fa fa-caret-square-right'></i>" + Ressource.GetMulti(resProcess);
                imgRunProcess.Pty.TooltipContent = Ressource.GetMulti(resProcess, 1) + Cst.GetAccessKey(imgRunProcess.AccessKey);
                imgRunProcess.CommandName = m_ProcessType;
                imgRunProcess.CommandArgument = m_ProcessName;

                string resIO = GetResourceBtnIO();
                imgRunProcessIO.Visible = IsDisplayBtnIO();
                imgRunProcessIO.Enabled = SessionTools.IsActionEnabled;
                imgRunProcessIO.Text = @"<i class='fa fa-caret-square-right'></i>" + Ressource.GetMulti(resIO);
                imgRunProcessIO.Pty.TooltipContent = Ressource.GetMulti(resIO, 1) + Cst.GetAccessKey(imgRunProcessIO.AccessKey);
                imgRunProcessIO.CommandName = Cst.ListProcess.ListProcessTypeEnum.Service.ToString();
                imgRunProcessIO.CommandArgument = Cst.ProcessIOType.IO.ToString();


                string resRunProcessAndIO = GetResourceBtnProcessAndIO();
                imgRunProcessAndIO.Visible = IsDisplayBtnProcessAndIO();
                imgRunProcessAndIO.Enabled = SessionTools.IsActionEnabled;
                imgRunProcessAndIO.Text = @"<i class='fa fa-caret-square-right'></i>" + Ressource.GetMulti(resRunProcessAndIO);
                imgRunProcessAndIO.Pty.TooltipContent = Ressource.GetMulti(resRunProcessAndIO, 1) + Cst.GetAccessKey(imgRunProcessAndIO.AccessKey);
                imgRunProcessAndIO.CommandName = Cst.ListProcess.ListProcessTypeEnum.Service.ToString();
                imgRunProcessAndIO.CommandArgument = Cst.ProcessIOType.ProcessAndIO.ToString();

                imgRecord.Visible = efsdtgRefeferentiel.IsGridInputMode;
                imgRecord.Text = @"<i class='fa fa-save'></i>" + Ressource.GetString("btnRecord");

                imgCancel.Visible = efsdtgRefeferentiel.IsGridInputMode;
                imgCancel.Text = @"<i class='fa fa-times-circle'></i>" + Ressource.GetString("btnCancel");

                //tbAction.Visible = imgAddNew.Visible || imgRecord.Visible || imgCancel.Visible ||
                //    imgRunProcess.Visible || imgRunProcessAndIO.Visible || imgRunProcessIO.Visible;

                lblViewerModel.Text = resModele;

                urlParms = "?C=" + HttpUtility.UrlEncode(idLstConsult) + "&T=" + HttpUtility.UrlEncode(idLstTemplate) + "&A=" + idA_TemplateOwner.ToString();
                urlParmsAdd = "&T1=" + HttpUtility.UrlEncode(title_ConsultationType) + "&T2=" + HttpUtility.UrlEncode(title_SpheresMenu);
                urlParmsAdd += "&IDMenu=" + efsdtgRefeferentiel.IDMenu;

                //btnViewerOpen
                url = "LstOpen.aspx" + urlParms + urlParmsAdd + urlParentGUID;
                imgViewerOpen.Visible = true;
                //imgViewerOpen.Pty.TooltipTitle = resModele;
                imgViewerOpen.Pty.TooltipContent = Ressource.GetString("btnViewerOpenToolTip") + Cst.GetAccessKey(imgViewerOpen.AccessKey);
                imgViewerOpen.OnClientClick = JavaScript.GetWindowOpen(url, "LstOpen_" + GUID, JavaScript.WindowOpenAttribut.ModeEnum.openRequestedPopup) + ";return false;";

                //btnViewerSave
                url = "LstSave.aspx" + urlParms + urlParmsAdd + urlParentGUID;
                imgViewerSave.Visible = true;
                //imgViewerSave.Pty.TooltipTitle = resModele;
                imgViewerSave.Pty.TooltipContent = Ressource.GetString("btnViewerSaveToolTip") + Cst.GetAccessKey(imgViewerSave.AccessKey);
                imgViewerSave.OnClientClick = JavaScript.GetWindowOpen(url, "LstSave_" + GUID, JavaScript.WindowOpenAttribut.ModeEnum.openRequestedPopup) + ";return false;";

                //btnViewerCriteria
                url = "LstCriteria.aspx" + urlParms + urlParmsAdd + urlWhere + urlParentGUID;
                imgViewerCriteria.Visible = true;
                imgViewerCriteria.Pty.TooltipContent = Ressource.GetString("btnViewerCriteriaToolTip") + Cst.GetAccessKey(imgViewerCriteria.AccessKey);
                imgViewerCriteria.OnClientClick = JavaScript.GetWindowOpen(url, "LstCriteria_" + GUID, JavaScript.WindowOpenAttribut.ModeEnum.openRequestedPopup) + ";return false;";

                //btnViewerDisplay && btnViewerSort
                imgViewerDisplay.Visible = m_IsConsultation;
                imgViewerSort.Visible = m_IsConsultation;
                if (m_IsConsultation)
                {
                    if (SessionTools.User.IsSessionGuest)
                    {
                        //UserWithLimitedRights
                        imgViewerDisplay.Visible = false;
                    }
                    else
                    {
                        url = "LstSelection.aspx" + urlParms + urlParmsAdd + "&TYPE=" + LstSelectionTypeEnum.DISP.ToString() + urlParentGUID;
                        //imgViewerDisplay.Pty.TooltipTitle = resModele;
                        imgViewerDisplay.Pty.TooltipContent = Ressource.GetString("btnViewerDisplayToolTip") + Cst.GetAccessKey(imgViewerDisplay.AccessKey);
                        imgViewerDisplay.OnClientClick = JavaScript.GetWindowOpen(url, "LstDisplay_" + GUID, JavaScript.WindowOpenAttribut.ModeEnum.openRequestedPopup) + ";return false;";

                    }

                    url = "LstSelection.aspx" + urlParms + urlParmsAdd + "&TYPE=" + LstSelectionTypeEnum.SORT.ToString() + urlParentGUID;
                    //imgViewerSort.Pty.TooltipTitle = resModele;
                    imgViewerSort.Pty.TooltipContent = Ressource.GetString("btnViewerSortToolTip") + Cst.GetAccessKey(imgViewerSort.AccessKey);
                    imgViewerSort.OnClientClick = JavaScript.GetWindowOpen(url, "LstSort_" + GUID, JavaScript.WindowOpenAttribut.ModeEnum.openRequestedPopup) + "; return false;";
                }
                #endregion Button properties

                // FI 20200224 [XXXXX] Mise en commenatire (Appel effectué dans on Init) 
                //LoadInfosLst();
                LoadTblSearch();
                LoadTblInfos(InfosLstWhere, InfosLstOrderBy);

                efsdtgRefeferentiel.IsBindData = true;

                if (this.PARAM_EVENTTARGET == "PostAfterLoadDataSourceError")
                {
                    string[] stringSeparators = new string[] { "{-}" };
                    String[] aEventArg = this.PARAM_EVENTARGUMENT.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (ArrFunc.IsFilled(aEventArg))
                    {
                        if (ArrFunc.Count(aEventArg) >= 2 && aEventArg[1] == "ForceReload")
                        {
                            //On recharche la source de donnée sans les critères
                            efsdtgRefeferentiel.IsLoadData = true;
                            _alErrorMessages.Add(Ressource.GetString("Msg_DisabledLstWhere"));
                            //Disable LstWhere
                            efsdtgRefeferentiel.LocalLstConsult.template.ISENABLEDLSTWHERE = false;
                            efsdtgRefeferentiel.LocalLstConsult.template.SetIsEnabledLstWhere2(SessionTools.CS);
                        }
                        _alErrorMessages.Add(aEventArg[0]);
                    }
                }

                if ((null != efsdtgRefeferentiel.LocalLstConsult) &&
                    (null != efsdtgRefeferentiel.LocalLstConsult.template.CSSCOLOR) &&
                    (0 < efsdtgRefeferentiel.LocalLstConsult.template.CSSCOLOR.Length))
                {
                    CSSColor = efsdtgRefeferentiel.LocalLstConsult.template.CSSCOLOR;
                    // EG 20210322 [25556] la couleur est associée au menu)
                    divDtg.CssClass = CSSColor + " " + mainMenuClassName + " " + GetFreezeCheckColumnCssClass();
                }
                //FI 20100512 SetPaging écrase la property AllowPaging
                //Spheres conserve celle spécifiée dans le view State
                //Sauf dans 2 cas (voir les cas plus bas)
                if (StrFunc.IsFilled(this.PARAM_EVENTTARGET) && PARAM_EVENTTARGET.Contains("lnkDisabledPager"))
                {
                    //1er cas l'utilisateur veut afficher le jeu de résultat sur 1 page
                    efsdtgRefeferentiel.SetPaging(-1);
                    //FI 20140630 [20101] CurrentPageIndex = 0 => il n'y a une unique page et son index est 0
                    //=> Cela permet d'avoir les n° de lignes corrects
                    efsdtgRefeferentiel.CurrentPageIndex = 0;
                }
                else if (StrFunc.IsFilled(this.PARAM_EVENTARGUMENT) && (PARAM_EVENTARGUMENT.ToUpper() == "SELFRELOAD_"))
                {
                    if (efsdtgRefeferentiel.LocalLstConsult.template != null)
                        //2ème cas l'utilisateur recharge les caractéristiques de son template (Bouton recharger ou modification du filtre etc...)
                        efsdtgRefeferentiel.SetPaging(efsdtgRefeferentiel.LocalLstConsult.template.ROWBYPAGE);
                }
                //
                if ((efsdtgRefeferentiel.IsFormInputMode || efsdtgRefeferentiel.IsGridInputMode)
                     && (efsdtgRefeferentiel.Referential.Create))
                {
                    imgAddNew.Visible = !m_IsSpheresProcess;

                    if (efsdtgRefeferentiel.IsGridInputMode)
                    {
                        imgAddNew.Click += new EventHandler(efsdtgRefeferentiel.AddItem);
                    }
                    else
                    {
                        //Cas particulier pour le ref ATTACHEDDOC(S) -> Add ouvre Upload.aspx
                        if (efsdtgRefeferentiel.ObjectName == Cst.OTCml_TBL.ATTACHEDDOC.ToString()
                            || efsdtgRefeferentiel.ObjectName == Cst.OTCml_TBL.ATTACHEDDOCS.ToString())
                        {
                            #region ATTACHEDDOC/ATTACHEDDOCS
                            string[] keyColumns = new string[2];
                            keyColumns[0] = "TABLENAME";
                            keyColumns[1] = "ID";
                            string[] keyValues = new string[2];
                            //PL 20160914 Use DecodeDA()
                            //keyValues[0] = Request.QueryString["DA"];
                            keyValues[0] = Ressource.DecodeDA(Request.QueryString["DA"]);
                            keyValues[1] = Request.QueryString["FK"];
                            string[] keyDatatypes = new string[2];
                            keyDatatypes[0] = TypeData.TypeDataEnum.@string.ToString();
                            keyDatatypes[1] = (efsdtgRefeferentiel.ObjectName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() ? TypeData.TypeDataEnum.@integer.ToString() : TypeData.TypeDataEnum.@string.ToString());
                            // EG 20220215 [26251][WI582] Upd : Vulnerability on unrestricted file upload : Enhancement
                            // PM 20240604 [XXXXX] Ajout GUID 
                            imgAddNew.Attributes.Add("onclick",
                                JavaScript.GetWindowOpenUpload(efsdtgRefeferentiel.ObjectName, efsdtgRefeferentiel.ObjectName, "LODOC", keyColumns, keyValues, keyDatatypes, string.Empty, string.Empty, string.Empty, string.Empty, this.GUID) + @"return false;");
                            #endregion
                        }
                        else if (efsdtgRefeferentiel.ObjectName == Cst.OTCml_TBL.ACTOR.ToString())
                        {
                            #region ACTOR
                            //ADDCLIENTGLOP Test in progress...
                            Control head = (null != this.Header) ? (Control)this.Header : (Control)PageTools.SearchHeadControl(this);

                            string url_ = efsdtgRefeferentiel.GetURLForInsert();
                            if (StrFunc.IsFilled(url_))
                            {
                                //TBD ... (Récupération d'une URL, donc sans window.open())
                                url_ = url_.Substring(@"window.open(""".Length);
                                // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
                                foreach (Cst.HyperLinkTargetEnum item in Enum.GetValues(typeof(Cst.HyperLinkTargetEnum)))
                                {
                                    if (url_.IndexOf(item.ToString()) > -1)
                                    {
                                        int thirdArgs = url_.IndexOf($@""",""{item}"",");
                                        if (-1 < thirdArgs)
                                            url_ = url_.Substring(0, thirdArgs);
                                        else
                                            url_ = url_.Substring(0, url_.IndexOf($@""",""{item}"""));
                                        break;
                                    }
                                }

                                //TBD ressource
                                string message = "";
                                message += @"<table id=""tblAddActor""><tr><td colspan=""3"">";
                                //message += @"<span class='title'>" + Ressource.GetString("lblTemplate") + "</span>";
                                //message += @"</td></tr><tr><td colspan=""3"">";
                                message += @"<span class='formula'>" + Ressource.GetString("SelectActorTemplate") + "</span>";
                                message += @"</td></tr><tr><td>";
                                message += @"<span>" + Cst.HTMLSpace2 + Ressource.GetString("lblTemplate") + "</span>";
                                message += @"</td><td>&nbsp;</td><td>";
                                message += @"<select name=""ddlSelect"" id=""ddlSelect"" class=""ddlCapture"" onchange=""ddlSelect_Change();"">" + Cst.CrLf;
                                string option = @"<option selected=""selected"" value=""{0}"">{1}</option>" + Cst.CrLf;
                                message += String.Format(option, "0", "");

                                #region Load Actor Templates
                                option = @"<option value=""{0}"">{1}</option>" + Cst.CrLf;
                                string SQLSelect = SQLCst.SELECT + @"IDA,IDENTIFIER,DISPLAYNAME" + Cst.CrLf;
                                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + Cst.CrLf;
                                SQLSelect += SQLCst.WHERE + "ISTEMPLATE=" + DataHelper.SQL_Bit_True.ToString();
                                SQLSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, Cst.OTCml_TBL.ACTOR);
                                SQLSelect += SQLCst.ORDERBY + "IDENTIFIER";
                                using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect))
                                {
                                    while (dr.Read())
                                    {
                                        string text = dr["IDENTIFIER"].ToString();
                                        if ((text != dr["DISPLAYNAME"].ToString()) && (!text.StartsWith(dr["DISPLAYNAME"].ToString())))
                                        {
                                            if (dr["DISPLAYNAME"].ToString().StartsWith(text))
                                            {
                                                text = dr["DISPLAYNAME"].ToString();
                                            }
                                            else
                                            {
                                                text += " - " + dr["DISPLAYNAME"].ToString();
                                            }
                                            if (text.Length > 100)
                                            {
                                                text = text.Substring(0, 97) + "...";
                                            }
                                        }
                                        message += JavaScript.JSString(String.Format(option, dr["IDA"].ToString(), text));
                                    }
                                }

                                #endregion
                                message += @"</select>";
                                message += @"</td></tr><tr><td colspan=""3"">";
                                message += @"<span class='title'>" + Ressource.GetString("NewActor") + "</span>";
                                message += @"</td></tr><tr><td>";
                                message += @"<span name=""lblActor"" id=""lblActor"" disabled>" + Cst.HTMLSpace2 + Ressource.GetString("ActorIdentifier") + "</span>";
                                message += @"</td><td>&nbsp;</td><td>";
                                message += @"<input type=""text"" name=""txtActor"" id=""txtActor"" class=""txtCaptureOptional"" width=""100%"" disabled />" + Cst.CrLf;
                                message += @"</td></tr><tr><td>";
                                message += @"<span name=""lblBook"" id=""lblBook"" disabled>" + Cst.HTMLSpace2 + Ressource.GetString("BookIdentifier") + "</span>";
                                message += @"</td><td>&nbsp;</td><td>";
                                message += @"<input type=""text"" name=""txtBook"" id=""txtBook"" class=""txtCaptureOptional"" width=""100%"" disabled=""true"" disabled />";
                                message += @"</td></tr></table>";

                                string title = Ressource.GetString("CreateNewActor");
                                string js = "OpenAddActorConfirm(" + JavaScript.JSString(title) + "," + JavaScript.HTMLString(message) + "," +
                                    JavaScript.JSString("addactor") + ",0,0,0,0," + JavaScript.JSString(url_) + "," + JavaScript.JSString("TBD") + ");";
                                imgAddNew.Attributes.Add("onclick", js + @"return false;");
                            }
                            #endregion
                        }
                        else
                        {
                            imgAddNew.Attributes.Add("onclick", efsdtgRefeferentiel.GetURLForInsert());
                        }
                    }
                }

                //
                if ((false == efsdtgRefeferentiel.Referential.Create) &&
                    (false == efsdtgRefeferentiel.Referential.Modify) &&
                    (false == efsdtgRefeferentiel.Referential.Remove))
                {
                    imgRecord.Enabled = false;
                }

                //ImageButton for MSExcel.aspx
                imgMSExcel.Pty.TooltipContent = Ressource.GetString("ExportToMSExcel") + Cst.GetAccessKey(imgMSExcel.AccessKey);
                imgMSExcel.Attributes.Add("onclick", String.Format("BlockUIForExport('{0}','{1}', '{2}');return true;", imgMSExcel.Pty.TooltipContent, RequestTrackExportType.XLS, efsdtgRefeferentiel.ObjectName));
                imgMSExcel.Click += new EventHandler(OnToolbar_Click);

                //Csv
                imgCSV.Pty.TooltipContent = Ressource.GetString("ExportToCSV") + Cst.GetAccessKey(imgCSV.AccessKey);
                imgCSV.Attributes.Add("onclick", String.Format("BlockUIForExport('{0}','{1}', '{2}');return true;", imgCSV.Pty.TooltipContent, RequestTrackExportType.CSV, efsdtgRefeferentiel.ObjectName));
                imgCSV.Click += new EventHandler(OnCSV_Click);
                //ImageButton for SQL insert statement
                imgSQL.Visible = IsDisplaySQLInsertCommand();
                imgSQL.Pty.TooltipContent = "Export to SQL Insert Statement";//Ressource.GetString("ExportToSQL") + Cst.GetAccessKey(imgSQL.AccessKey);
                imgSQL.Attributes.Add("onclick", String.Format("BlockUIForExport('{0}','{1}', '{2}');return true;", imgSQL.Pty.TooltipContent, RequestTrackExportType.SQL, efsdtgRefeferentiel.ObjectName));
                imgSQL.Click += new EventHandler(OnToolbar_Click);

                //ImageButton for ZIP transformation
                imgZIP.Pty.TooltipContent = Ressource.GetString("ExportToZIP") + Cst.GetAccessKey(imgZIP.AccessKey);
                imgZIP.Attributes.Add("onclick", String.Format("BlockUIForExport('{0}','{1}', '{2}');return true;", imgZIP.Pty.TooltipContent, RequestTrackExportType.ZIP, efsdtgRefeferentiel.ObjectName));
                imgZIP.Click += new EventHandler(OnToolbar_Click);

                //ImageButton for PrintAll
                //TODO Il faudra continuer pour :
                //        - appeler un post de la page avec AllowPaging=false, afin d'avoir toutes les données
                //        - gérer le pb de largeur d'impression...
                imgPrintAll.Pty.TooltipContent = Ressource.GetString("PrintDatagrid") + Cst.GetAccessKey(imgPrintAll.AccessKey);
                imgPrintAll.Attributes.Add("onclick", StrFunc.AppendFormat("CallPrint('{0}','{1}');return false;", divHeader.ID, divDtg.ID));

                SetHeader();

                PageTitle = HtmlTools.HTMLBold_Remove(mainTitle);
                // EG 20130725 Timeout sur Block
                JQuery.Block block = new JQuery.Block((PageBase)this.Page)
                {
                    Timeout = SystemSettings.GetTimeoutJQueryBlock(efsdtgRefeferentiel.IDMenu.ToUpper(), efsdtgRefeferentiel.ObjectName.ToUpper())
                };
                JQuery.UI.WriteInitialisationScripts(this, block);
                //PL 20130422 ADDCLIENTGLOP AddActor
                JQuery.UI.WriteInitialisationScripts(this, new JQuery.AddActor());
                JQuery.UI.WriteInitialisationScripts(this, new JQuery.ActorTransfer());

                // EG 20130613 Menu Export Création du menu et de ses items
                AddButtonChildPDF();

                divtoolbar.Visible = imgAddNew.Visible || imgRecord.Visible || imgCancel.Visible ||
                    imgRunProcess.Visible || imgRunProcessAndIO.Visible || imgRunProcessIO.Visible;

                if (efsdtgRefeferentiel.LocalLstConsult.template.FREEZECOL > 0)
                    divDtg.Attributes.Add("frz-col", efsdtgRefeferentiel.LocalLstConsult.template.FREEZECOL.ToString());
                else if (efsdtgRefeferentiel.Referential.freezeColumnSpecified)
                    divDtg.Attributes.Add("frz-col", efsdtgRefeferentiel.Referential.freezeColumn);
                else
                    divDtg.Attributes.Remove("frz-col");
            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End LstReferentialPage.Page_Load");
        }

        /// <summary>
        ///  Appel d'un process 
        /// </summary>
        /// FI 20240119 [WI819] Add method RunProcess
        private void RunProcess()
        {
            m_IsProcessExecuted = CallProcess();
            if (m_IsProcessExecuted)
            {
                if (StrFunc.IsFilled(Request.QueryString["ProcessLoadData"]))
                    efsdtgRefeferentiel.IsLoadData = BoolFunc.IsTrue(Request.QueryString["ProcessLoadData"]);
            }
        }


        /// <summary>
        /// Création du menu Export et de ses items en fonction de la présence des tags efsdtgRefeferentiel.referential.XSLFileName
        /// dans le fichier XML
        /// 1. Pas de tags = Export PDF par défaut de base
        /// 2. 1 seul tag = Une ligne de menu principale (action)
        /// 3. n tags     = Une ligne de menu principale (titre) + n items (action)
        /// </summary>
        /// EG 20130613 Menu Export Changement de type : skmMenu.MenuItemClickEventArgs
        /// FI 20150528 [20178] Modify
        // EG 20190411 [ExportFromCSV] Font-awesome
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200928 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Add mainMenuClass on cssClass
        protected void AddButtonChildPDF()
        {
            string res_title = "Export";
            string res_tooltip = Ressource.GetString("ExportToPDF");
            skmMenu.MenuItemParent menuPDF = new skmMenu.MenuItemParent(1);

            menuPDF[0].eText = res_title;
            menuPDF[0].aToolTip = res_tooltip;

            int nbXSLFile = 0;
            // FI 20150528 [20178] Add  SessionTools.IsActionEnabled
            if (efsdtgRefeferentiel.Referential.XSLFileNameSpecified && SessionTools.IsActionEnabled)
            {
                nbXSLFile = efsdtgRefeferentiel.Referential.XSLFileName.GetLength(0);
                switch (nbXSLFile)
                {
                    case 0:
                        menuPDF[0].eText = res_title;
                        menuPDF[0].aToolTip = res_tooltip;
                        menuPDF[0].eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                        break;
                    case 1:
                        ReferentialsReferentialXSLFileName _item = efsdtgRefeferentiel.Referential.XSLFileName[0];
                        menuPDF[0].eText = (_item.titleSpecified ? Ressource.GetString(_item.title, true) : res_title);
                        menuPDF[0].aToolTip = (_item.tooltipSpecified ? Ressource.GetString(_item.tooltip, true) : res_tooltip);
                        menuPDF[0].eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                        if (StrFunc.IsFilled(_item.Value))
                            menuPDF[0].eArgument = _item.Value;
                        break;
                    default:
                        menuPDF[0] = new skmMenu.MenuItemParent(nbXSLFile);
                        ArrayList aMenuRoot = new ArrayList();
                        foreach (ReferentialsReferentialXSLFileName item in efsdtgRefeferentiel.Referential.XSLFileName)
                        {
                            skmMenu.MenuItemParent menuRoot = new skmMenu.MenuItemParent(0)
                            {
                                eText = (item.titleSpecified ? Ressource.GetString(item.title, true) : res_title),
                                aToolTip = (item.tooltipSpecified ? Ressource.GetString(item.tooltip, true) : res_tooltip),
                                eCommandName = Cst.Capture.MenuEnum.Report.ToString()
                            };
                            if (StrFunc.IsFilled(item.Value))
                                menuRoot.eArgument = item.Value;
                            aMenuRoot.Add(menuRoot);
                        }
                        menuPDF[0].subItems = (skmMenu.MenuItemParent[])aMenuRoot.ToArray(typeof(skmMenu.MenuItemParent));
                        menuPDF[0].eText = res_title;
                        menuPDF[0].aToolTip = res_tooltip;
                        break;
                }
            }
            else
            {
                menuPDF[0].eCommandName = Cst.Capture.MenuEnum.Report.ToString();
            }

            menuPDF[0].Enabled = true;
            menuPDF[0].eImageUrl = "fas fa-icon fa-file-pdf";

            //PL 20130906 Newness [TRIM 18633]
            if ((nbXSLFile == 0) || ((nbXSLFile == 1) && StrFunc.IsEmpty(efsdtgRefeferentiel.Referential.XSLFileName[0].Value)))
            {
                //menuPDF[0].Enabled = false;
                menuPDF[0].Hidden = true;
            }

            Panel pnlSkmMenu = new Panel
            {
                CssClass = mainMenuClassName + " skmnu"
            };
            tbPDF.Controls.Add(pnlSkmMenu);

            skmMenu.Menu mnuToolBar = new skmMenu.Menu(0, "mnuPDF", null, null, null, null, null)
            {
                DataSource = menuPDF.InitXmlWriter(),
                Layout = skmMenu.MenuLayout.Horizontal,
                LayoutDOWN = skmMenu.MenuLayoutDOWN.DOWN
            };
            mnuToolBar.MenuItemClick += new skmMenu.MenuItemClickedEventHandler(OnReport);
            pnlSkmMenu.Controls.Add(mnuToolBar);
            tbPDF.DataBind();
            bool isExistPDF = (pnlSkmMenu.HasControls() && pnlSkmMenu.Controls[0].HasControls() &&
                               pnlSkmMenu.Controls[0].Controls[0].HasControls() && pnlSkmMenu.Controls[0].Controls[0].Controls[0].HasControls());
            tbPDF.Style.Add(HtmlTextWriterStyle.Display, isExistPDF ? "table-cell" : "none");
        }

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void InitializeComponent()
        {
            imgRunProcess.Command += new CommandEventHandler(this.OnProcess_Click);
            imgRunProcessIO.Command += new CommandEventHandler(this.OnProcess_Click);
            imgRunProcessAndIO.Command += new CommandEventHandler(this.OnProcess_Click);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void OnProcess_Click(object sender, CommandEventArgs e)
        {
            m_btnRunProcess_Clicked = (LinkButton)sender;
            efsdtgRefeferentiel.IsLoadData = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20190411 [ExportFromCSV]
        protected void OnCSV_Click(object sender, System.EventArgs e)
        {
            m_toolbar_Clicked = (Control)sender;
            efsdtgRefeferentiel.IsLoadData = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20130417 [18596] Modify(add ExportExcel) 
        /// FI 20160308 [21782] Modify 
        protected void OnToolbar_Click(object sender, System.EventArgs e)
        {
            /// EG 20130613 Changement de cast (suite à Export PDF)
            m_toolbar_Clicked = (Control)sender;

            if (efsdtgRefeferentiel.AllowCustomPaging)
            {
                //Pas de pagination customisée, la source de donnée est chargée en totalité 
                efsdtgRefeferentiel.PagingType = TemplateDataGrid.PagingTypeEnum.NativePaging;
                //Il faut reloader la source de donnée du Datagrid
                efsdtgRefeferentiel.IsLoadData = true;
            }
            else
            {
                //20091029 FI [download File] Add else pour ne pas reloader lorsque que datagrid est déjà totalement chargé
                efsdtgRefeferentiel.IsLoadData = false;
            }

            // FI 20130417 [18596] SetPaging -1 pour que le grid charge toute les lignes
            // EG 20130613 Changement de cast (suite à Export PDF)
            if (((Control)m_toolbar_Clicked).ID == "imgMSExcel")
            {
                efsdtgRefeferentiel.SetPaging(-1);
                /* FI 20200602 [25370] Mis en commentaire
                // FI 20160308 [21782] isModeExportExcel = true;
                efsdtgRefeferentiel.isModeExportExcel = true;
                */
            }
            else if (((Control)m_toolbar_Clicked).ID == "imgZIP")
            {
                // FI 20211228 [XXXXX] Enregistrement de ExportZIPAsync
                RegisterAsyncTask(new PageAsyncTask(ExportZIPAsync));
            }

            // FI 20130417 [18596] mise en commentaire  
            ////Cas particulier de l'Export MS Excel®
            //if (m_toolbar_Clicked.ID == "imgMSExcel")
            //{
            //    //FI 20120212 [] add filename in URL 
            //    string url = StrFunc.AppendFormat("~/MSExcel.aspx?dvMSExcel={0}&filename={1}",
            //            efsdtgRefeferentiel.Data_CacheName, efsdtgRefeferentiel.ObjectName);
            //    Page.Response.Redirect(url, false);
            //}

        }
        /// EG 20130613 Appeler par click sur item menu Export PDF
        /// Valorisation de m_toolbar_Clicked par l'argument de l'item
        protected void OnReport(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            m_toolbar_Clicked = e;
            if (efsdtgRefeferentiel.AllowCustomPaging)
            {
                //Pas de pagination customisée, la source de donnée est chargée en totalité 
                efsdtgRefeferentiel.PagingType = TemplateDataGrid.PagingTypeEnum.NativePaging;
                //Il faut reloader la source de donnée du Datagrid
                efsdtgRefeferentiel.IsLoadData = true;
            }
            else
            {
                //20091029 FI [download File] Add else pour ne pas reloader lorsque que datagrid est déjà totalement chargé
                efsdtgRefeferentiel.IsLoadData = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRecord_Click(object sender, System.EventArgs e)
        {
            efsdtgRefeferentiel.SubmitChanges(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCancel_Click(object sender, System.EventArgs e)
        {
            efsdtgRefeferentiel.IsDataModified = false;
            JavaScript.ScriptOnStartUp(this, "RefreshPage", "RefreshPage");
        }

        /// <summary>
        ///  Appel d'un process. Retourne true si le process a réellement été exécuté
        /// </summary>
        /// FI 20240119 [WI819] Refactoring
        private Boolean CallProcess()
        {
            Boolean ret = false;
            string[] commandArguments = PARAM_EVENTARGUMENT.Split(';');
            if (ArrFunc.Count(commandArguments) > 1)
            {
                if (Enum.IsDefined(typeof(Cst.ListProcess.ListProcessTypeEnum), commandArguments[0]))
                {
                    Cst.ListProcess.ListProcessTypeEnum ListProcess =
                        (Cst.ListProcess.ListProcessTypeEnum)Enum.Parse(typeof(Cst.ListProcess.ListProcessTypeEnum), commandArguments[0]);
                    string processName = commandArguments[1];

                    Nullable<Cst.ProcessIOType> processIoType = null;
                    if (Cst.ListProcess.IsService(ListProcess) && Enum.IsDefined(typeof(Cst.ProcessIOType), commandArguments[1]))
                    {
                        processIoType = (Cst.ProcessIOType)Enum.Parse(typeof(Cst.ProcessIOType), commandArguments[1]);
                        processName = SpheresProcessType.ToString();
                    }

                    string additionalArguments = string.Empty;
                    if ((4 == commandArguments.Length) && ("Args" == commandArguments[2]))
                        additionalArguments = commandArguments[3];

                    #region Only Rows with ISSELECTED=true
                    DataTable dt = efsdtgRefeferentiel.DsData.Tables[0];
                    if ((false == IsSpheresProcessTypeUsingNormMsgFactory))
                    {
                        DataRow[] rows = dt.Select("ISSELECTED=true");
                        if ((ArrFunc.Count(rows) < ArrFunc.Count(dt.Rows)) || efsdtgRefeferentiel.Referential.HasEditableColumns)
                        {
                            DataTable dtToProcess = dt.Clone();
                            for (int i = 0; i < ArrFunc.Count(rows); i++)
                                dtToProcess.LoadDataRow(rows[i].ItemArray, LoadOption.OverwriteChanges);
                            dt = dtToProcess;
                        }
                    }
                    #endregion Only Rows with ISSELECTED=true;
                    #region Entity
                    int entityId = 0;
                    string entityName = string.Empty;
                    if (this.FindControl(Cst.DDL + "ENTITY") is DropDownList ddl)
                    {
                        entityId = IntFunc.IntValue(ddl.SelectedItem.Value);
                        entityName = ddl.SelectedItem.Text;
                    }
                    #endregion

                    switch (ListProcess)
                    {
                        case Cst.ListProcess.ListProcessTypeEnum.Service:
                            if (Enum.IsDefined(typeof(Cst.ProcessTypeEnum), processName.ToUpper()))
                            {
                                Cst.ProcessTypeEnum processType = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), processName, true);
                                if (IsSpheresProcessTypeUsingNormMsgFactory)
                                    CallProcessServiceUsingNormsgFactory(processType);
                                else if (0 < dt.Rows.Count)
                                    CallProcessService2(processType, entityId, entityName, dt, processIoType);
                                ret = true;
                            }
                            break;
                        case Cst.ListProcess.ListProcessTypeEnum.ProcessCSharp:
                            if (0 < dt.Rows.Count)
                            {
                                CallProcessCsharp(processName, entityName, dt, additionalArguments);
                                ret = true;
                            }
                            break;
                        case Cst.ListProcess.ListProcessTypeEnum.StoredProcedure:
                            if (0 < dt.Rows.Count)
                            {
                                CallProcessStoredProcedure(processName);
                                ret = true;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Alimente le placeHolder chargé d'afficher l'entête de la page
        /// <para>
        /// Alimente le placeHolder avec une bannière alimentée avec le titre de la consultation
        /// </para>
        /// </summary>
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20210331 [25556] Affichage du propriétaire du template actif avant date du dernier refresh
        private void SetHeader()
        {
            HtmlPageTitle titleLeft = new HtmlPageTitle(mainTitle, subTitle);
            //FI 20110817 Alimentation du toolTip sur le titre
            if (StrFunc.IsFilled(efsdtgRefeferentiel.IDMenu))
            {
                SpheresMenu.Menu mnu = SessionTools.Menus.SelectByIDMenu(efsdtgRefeferentiel.IDMenu);
                if (null != mnu)
                    titleLeft.TitleTooltip = mnu.GetToolTip();

                string particularTitle = ReferentialTools.ParticularTitle(efsdtgRefeferentiel.IDMenu, efsdtgRefeferentiel.ObjectName);
                if (StrFunc.IsFilled(particularTitle))
                    titleLeft.Title += @"\" + particularTitle;
            }

            HtmlPageTitle titleRight = new HtmlPageTitle(subTitleRight);
            plhHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, titleRight, efsdtgRefeferentiel.IDMenu.ToString(), mainMenuClassName));
        }

        /// <summary>
        /// Affichage de la dernière query exécutée pour alimenter le Datagrid
        /// </summary>
        private void DisplayLastQuery()
        {

            if (null != efsdtgRefeferentiel.LastQuery)
            {
                string WindowID = FileTools.GetUniqueName("LoadDataSource", "lastQuery");
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
                xmlRoot.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", efsdtgRefeferentiel.LastQuery.Second);

                xmlRoot.AppendChild(xmlDoc.CreateCDataSection(efsdtgRefeferentiel.LastQuery.First));
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

        /// <summary>
        /// Affichage de la command exécutée dans preselect
        /// </summary>
        private void DisplayPreSelectCommand()
        {

            if (ArrFunc.IsFilled(efsdtgRefeferentiel.LastPreSelectCommand))
            {
                Pair<string, TimeSpan>[] lastPreSelectCommand = efsdtgRefeferentiel.LastPreSelectCommand;

                string WindowID = FileTools.GetUniqueName("LoadDataSource", "preSelectCommand");
                string write_File = SessionTools.TemporaryDirectory.MapPath("Datagrid") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "Datagrid" + @"/" + WindowID + ".xml";

                XmlDocument xmlDoc = new XmlDocument
                {
                    PreserveWhitespace = true
                };
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("PreSelect Command use, File: {0}", write_File);
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("preSelectCommand");
                xmlDoc.AppendChild(xmlRoot);
                //
                if (ArrFunc.Count(lastPreSelectCommand) == 1)
                {
                    xmlRoot.AppendChild(xmlDoc.CreateCDataSection(lastPreSelectCommand[0].First));
                    xmlRoot.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                    //FI 20160810 [XXXXX] le format {0:hh:mm:ss.fff} ne fonctionne pas => remplacer par @"{0:hh\:mm\:ss\.fff}"
                    xmlRoot.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", lastPreSelectCommand[0].Second);

                }
                else
                {
                    for (int i = 0; i < ArrFunc.Count(lastPreSelectCommand); i++)
                    {
                        XmlElement xmlElement = xmlDoc.CreateElement("preSelectCommand" + i.ToString());
                        xmlElement.AppendChild(xmlDoc.CreateCDataSection(lastPreSelectCommand[i].First));
                        xmlElement.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                        //FI 20160810 [XXXXX] le format {0:hh:mm:ss.fff} ne fonctionne pas => remplacer par @"{0:hh\:mm\:ss\.fff}"
                        xmlElement.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", lastPreSelectCommand[i].Second);
                        xmlRoot.AppendChild(xmlElement);
                    }
                }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLoadDataError(object sender, LoadDataErrorEventArgs e)
        {
            string error = Ressource.GetString(e.message) + Cst.CrLf + Cst.CrLf + "(" + e.errorCode + ")";
            //En cas d'erreur lors du chargement il ne faut evidemment pas recharger la source de donnée
            efsdtgRefeferentiel.IsLoadData = false;
            // pas de bind du datagrid puisque la page est repostée dans la foulée par un ConfirmStartUpImmediate ou un DialogStartUpImmediate
            efsdtgRefeferentiel.IsBindData = false;
            //
            string message = Ressource.GetString("Msg_ErrorOccurs");
            message += Cst.CrLf2 + "Error:" + Cst.CrLf;
            message += error;
            if (efsdtgRefeferentiel.Referential.HasLstWhereClause && efsdtgRefeferentiel.LocalLstConsult.template.ISENABLEDLSTWHERE)
            {
                string postback_target = "PostAfterLoadDataSourceError";
                string postback_argument = error + "{-}";
                message += Cst.CrLf2 + Ressource.GetString("Msg_ContinueRefreshDataGridAsk");
                message += Cst.CrLf + Ressource.GetString("Msg_DisabledLstWhere");
                JavaScript.ConfirmStartUpImmediate((PageBase)this.Page, message, ProcessStateTools.StatusError, postback_target, postback_argument + "ForceReload", postback_argument + "Undo");
            }
            else
            {
                JavaScript.DialogStartUpImmediate((PageBase)this.Page, message, false, ProcessStateTools.StatusErrorEnum);
            }
            _alErrorMessages.Add(error);
            //
            //Ecriture du message d'erreur dans le log pour du diag
            string msgError = StrFunc.AppendFormat("Error on LoadDataSource:{0}", e.errorCode); ;
            WriteLogException(new Exception(msgError));
        }

        /// <summary>
        /// Exportation PDF avec transformation XSLT
        /// </summary>
        /// <param name="pFileName">Fichier XSLT associé pour la transformation</param>
        /// EG 20130613 Nouveau paramètre pFileName
        /// FI 20160804 [Migration TFS] Modify
        // EG 20190411 [ExportFromCSV] Cookie to close BlockUI
        // EG 20210126 [25556] Gestion Samesite (Strict) sur cookie
        // EG 20210216 [25664] Sécurité Cookie
        // EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage en JS)
        private void ExportPdf(string pFileName)
        {
            if (efsdtgRefeferentiel.IsDataAvailable)
            {
                // FI 20160804 [Migration TFS] 
                //string xsltFile = @"~\Spheres\XSL_Files\DataGrid\default-landscape-pdf.xsl";
                string xsltFile = @"~\GUIOutput\DataGrid\default-landscape-pdf.xsl";
                //xsltFile = @"~\GUIOutput\DataGrid\TEST-landscape-pdf.xslt";

                if (StrFunc.IsFilled(pFileName))
                    xsltFile = pFileName;

                bool isFound = File.Exists(xsltFile);
                if (!isFound)
                {
                    string tmp_xslMapping = string.Empty;
                    try
                    {
                        isFound = SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, xsltFile, ref tmp_xslMapping);
                    }
                    catch { isFound = false; }

                    if (isFound)
                    {
                        xsltFile = tmp_xslMapping;
                    }
                    else
                    {
                        xsltFile = SessionTools.AppSession.AppInstance.GetFilepath(xsltFile);
                        isFound = File.Exists(xsltFile);
                    }
                }

                if (isFound)
                {
                    string key = "PDF_" + efsdtgRefeferentiel.Title + "_" + efsdtgRefeferentiel.ObjectName;
                    string physicalPath = SessionTools.TemporaryDirectory.MapPath(key);
                    string relativePath = SessionTools.TemporaryDirectory.Path + key;
                    string WindowID = FileTools.GetUniqueName(efsdtgRefeferentiel.Title, efsdtgRefeferentiel.ObjectName);

                    DataSet dsReferential = efsdtgRefeferentiel.DsData;
                    efsdtgRefeferentiel.SetTableColumnMapping();

                    StringBuilder sbReferential = new StringBuilder();

                    #region Referential XML Construction
                    TextWriter writer = new StringWriter(sbReferential);
                    XmlTextWriter xw = new XmlTextWriter(writer);
                    dsReferential.WriteXml(xw);
                    writer.Close();
                    //
                    XmlDocument xmlReferential = new XmlDocument();
                    XmlNode root = xmlReferential.CreateNode(XmlNodeType.Element, "Referential", "");
                    xmlReferential.AppendChild(root);
                    //
                    //Add attribute TableName
                    XmlAttribute attName = xmlReferential.CreateAttribute("TableName");
                    attName.Value = dsReferential.Tables[0].TableName;
                    root.Attributes.Append(attName);
                    //Add attribute timestamp
                    XmlAttribute attTimestamp = xmlReferential.CreateAttribute("timestamp");
                    attTimestamp.Value = DtFunc.DateTimeToStringDateISO(OTCmlHelper.GetDateSys(SessionTools.CS));
                    root.Attributes.Append(attTimestamp);

                    // Titre
                    XmlNode newRow = xmlReferential.CreateNode(XmlNodeType.Element, "Title", "");
                    XmlNode attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                    attColumnValue.Value = mainTitle.Replace(Cst.HTMLBold, string.Empty).Replace(Cst.HTMLEndBold, String.Empty);
                    newRow.AppendChild(attColumnValue);
                    root.AppendChild(newRow);

                    // SubTitle
                    newRow = xmlReferential.CreateNode(XmlNodeType.Element, "SubTitle", "");
                    attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                    attColumnValue.Value = subTitle;
                    newRow.AppendChild(attColumnValue);
                    root.AppendChild(newRow);

                    //Ajout d'un element data qui contient les dynamicArg
                    if (null != DynamicArgs)
                    {
                        StringDynamicDatas sdds = ConvertDynamicArgs();
                        //
                        XmlDocument docDA = new XmlDocument();
                        docDA.LoadXml(sdds.Serialize());
                        //
                        XmlNode xmlNode = xmlReferential.ImportNode(docDA.DocumentElement, true);
                        root.AppendChild(xmlNode);
                    }

                    //Ajout des rows
                    XmlDocument xmlDataSet = new XmlDocument();
                    xmlDataSet.LoadXml(sbReferential.ToString());
                    // EG 20130618 Complément d'information sur XML (TO DO)
                    XmlNodeList tableRows = xmlDataSet.SelectNodes(dsReferential.DataSetName + "/" + dsReferential.Tables[0].TableName);
                    foreach (XmlNode tableRow in tableRows)
                    {
                        newRow = xmlReferential.CreateNode(XmlNodeType.Element, "Row", "");
                        root.AppendChild(newRow);
                        //
                        XmlNodeList tableRowColumns = tableRow.ChildNodes;
                        // EG 20160308 Migration vs2013
                        //bool isHide = false;
                        foreach (XmlNode column in tableRowColumns)
                        {
                            XmlNode newColumn = xmlReferential.CreateNode(XmlNodeType.Element, "Column", "");
                            // Nom de la colonne
                            XmlAttribute attColumnName = xmlReferential.CreateAttribute("ColumnName");
                            attColumnName.Value = column.Name;
                            newColumn.Attributes.Append(attColumnName);

                            int idxCol = efsdtgRefeferentiel.Referential.GetIndexDataGrid(column.Name);
                            if (0 <= idxCol)
                            {
                                ReferentialsReferentialColumn rrc = efsdtgRefeferentiel.Referential[idxCol];
                                if (false == rrc.IsHideInDataGrid)
                                {
                                    // Ressource
                                    if (rrc.RessourceSpecified)
                                    {
                                        attColumnName = xmlReferential.CreateAttribute("RessourceName");
                                        attColumnName.Value = Ressource.GetMulti(rrc.Ressource, 0);
                                        newColumn.Attributes.Append(attColumnName);
                                    }
                                    // DataType
                                    string dataType = "UT_ENUM_MANDATORY";
                                    switch (rrc.DataTypeEnum)
                                    {
                                        case TypeData.TypeDataEnum.@bool:
                                        case TypeData.TypeDataEnum.@bool2h:
                                        case TypeData.TypeDataEnum.@bool2v:
                                        case TypeData.TypeDataEnum.@boolean:
                                            dataType = "UT_BOOLEAN";
                                            break;
                                        case TypeData.TypeDataEnum.@date:
                                            dataType = "UT_DATE";
                                            break;
                                        case TypeData.TypeDataEnum.@datetime:
                                            dataType = "UT_DATETIME";
                                            break;
                                        case TypeData.TypeDataEnum.@dec:
                                        case TypeData.TypeDataEnum.@decimal:
                                            dataType = "UT_VALUE";
                                            break;
                                        case TypeData.TypeDataEnum.@int:
                                        case TypeData.TypeDataEnum.@integer:
                                            dataType = "UT_NUMBER";
                                            break;
                                        case TypeData.TypeDataEnum.@string:
                                            dataType = "UT_IDENTIFIER";
                                            break;
                                    }
                                    attColumnName = xmlReferential.CreateAttribute("DataType");
                                    attColumnName.Value = dataType;
                                    newColumn.Attributes.Append(attColumnName);

                                    XmlNodeList columnValues = column.ChildNodes;
                                    foreach (XmlNode columnValue in columnValues)
                                    {
                                        if (columnValue.NodeType == XmlNodeType.Text)
                                        {
                                            attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                                            attColumnValue.Value = columnValue.Value;
                                            newColumn.AppendChild(attColumnValue);
                                        }
                                    }
                                    newRow.AppendChild(newColumn);
                                }
                            }
                            else
                            {
                                XmlNodeList columnValues = column.ChildNodes;
                                foreach (XmlNode columnValue in columnValues)
                                {
                                    if (columnValue.NodeType == XmlNodeType.Text)
                                    {
                                        attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                                        attColumnValue.Value = columnValue.Value;
                                        newColumn.AppendChild(attColumnValue);
                                    }
                                }
                                newRow.AppendChild(newColumn);
                            }
                        }
                    }
                    //
                    sbReferential = new StringBuilder(xmlReferential.InnerXml);
                    //
                    if (IsTrace)
                    {
                        string write_xmlFile = physicalPath + @"\" + WindowID + "_DataSet.xml";
                        string open_xmlFile = relativePath + @"/" + WindowID + "_DataSet.xml";
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.LoadXml(sbReferential.ToString());
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_xmlFile);
                        AddScriptWinDowOpenFile(WindowID + "_DataSet", open_xmlFile, string.Empty);
                    }
                    #endregion

                    #region Referential XSLT Transformation
                    Hashtable param = new Hashtable
                    {
                        /// FI 20160804 [Migration TFS] Mise en commentaire du paramètre
                        { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name }
                    };

                    string transformResult = XSLTTools.TransformXml(sbReferential, xsltFile, param, null);

                    if (IsTrace)
                    {
                        string write_xslFile = physicalPath + @"\" + WindowID + "_Transform.xsl";
                        string open_xslFile = relativePath + @"/" + WindowID + "_Transform.xsl";
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(xsltFile);
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_xslFile);
                        AddScriptWinDowOpenFile(WindowID + "_Transform", open_xslFile, string.Empty);
                        //
                        string write_txtFile = physicalPath + @"\" + WindowID + "_Transform_Result.txt";
                        string open_txtFile = relativePath + @"/" + WindowID + "_Transform_Result.txt";
                        xmldoc = new XmlDocument();
                        xmldoc.LoadXml(transformResult);
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_txtFile);
                        AddScriptWinDowOpenFile(WindowID + "_Transform_Result", open_txtFile, string.Empty);
                    }
                    #endregion

                    string write_File = physicalPath + @"\" + WindowID + ".PDF";
                    string open_File = relativePath + @"/" + WindowID + ".PDF";
                    // EG 20160308 Migration vs2013
                    //byte[] binaryResult = FopEngine_V2.TransformToByte(CS, transformResult, physicalPath);
                    //if (null != binaryResult)
                    //{
                    //    FileTools.WriteBytesToFile(binaryResult, write_File, true);
                    //    AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
                    //}
                    if (Cst.ErrLevel.SUCCESS == FopEngine.WritePDF(SessionTools.CS, transformResult, physicalPath, write_File))
                    {
                        AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
                        Response.AppendCookie(AspTools.InitHttpCookie(RequestTrackExportType.PDF + "fileToken", false, ExportTokenValue));
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20190411 [ExportFromCSV] Cookie to close BlockUI
        /// EG 20210126 [25556] Gestion Samesite (Strict) sur cookie
        /// EG 20210216 [25664] Sécurité Cookie
        /// EG 20210308 [25664] Add parameter IsHTTPOnly sur Ecriture de Cookies (pour partage en JS)
        private void ExportSQL()
        {
            Type tClassProcess = typeof(Repository);
            Object obj = tClassProcess.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
            MethodInfo method = tClassProcess.GetMethod("SQLExport");
            if (null != method)
            {
                string key = "SQL_" + efsdtgRefeferentiel.Title + "_" + efsdtgRefeferentiel.ObjectName;
                string physicalPath = SessionTools.TemporaryDirectory.MapPath(key);
                string relativePath = SessionTools.TemporaryDirectory.Path + key;
                string WindowID = FileTools.GetUniqueName(efsdtgRefeferentiel.Title, efsdtgRefeferentiel.ObjectName);

                string write_File = physicalPath + @"\" + WindowID + ".HTML";
                string open_File = relativePath + @"/" + WindowID + ".HTML";

                Object[] args = new Object[] { this, efsdtgRefeferentiel.Referential, efsdtgRefeferentiel.DsData.Tables[0], write_File };
                _ = tClassProcess.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, obj, args, null, null, null);

                Response.AppendCookie(AspTools.InitHttpCookie(RequestTrackExportType.SQL + "fileToken", false, ExportTokenValue));
                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }

        }

        /// <summary>
        /// Préparation à la génération d'un fichier CSV
        /// Si non existence du DataSet (DsData) alors exécution de la query pour l'alimenter
        /// Transformation du DataSet en DataReader et appel à la construction du CSV
        /// </summary>
        /// EG 20190411 [ExportFromCSV] New
        // EG 20210308 [XXXXX] Mise à jour package CsvHelper 12.3.2 vers 13.0.0
        // EG 20210308 [XXXXX] CsvConfiguration devient OverrideCsvConfiguration
        private void ExportCSV()
        {
            // Chargement du DataSet
            if (false == efsdtgRefeferentiel.IsDataFilled)
                efsdtgRefeferentiel.CsvLoadData();

            if (efsdtgRefeferentiel.IsDataFilled)
            {
                // Chargement des paramètres du fichier de configuration
                CsvCommonConfigElement csvCurrentSettings = GetCsvConfigSetting(efsdtgRefeferentiel.ObjectName);
                OverrideCsvConfiguration csvConfiguration = new OverrideCsvConfiguration(csvCurrentSettings);
                // Génération du CSV
                byte[] result = CsvTools.ExportToCSV(efsdtgRefeferentiel.DsData, csvConfiguration, efsdtgRefeferentiel.Referential);
                // Retourne  un httpReponse pour ouvrir ou sauvegarder le fichier au format CSV
                ExportCsvResponse(result, efsdtgRefeferentiel.ObjectName, ExportTokenValue);
            }
        }

        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayInfo()
        {
            //20090925 FI gestion du message d'erreur
            if (ArrFunc.IsFilled(_alException))
            {
                _alErrorMessages.Add(Ressource.GetString("Msg_Unexpected_ErrorOccurs"));
                foreach (SpheresException2 ex in _alException)
                    _alErrorMessages.Add(ex.ToString());
            }
            //
            //20090925 FI gestion du message d'erreur
            if (ArrFunc.IsFilled(efsdtgRefeferentiel.AlException))
            {
                _alErrorMessages.Add(Ressource.GetString("Msg_Unexpected_ErrorOccurs"));
                foreach (SpheresException2 ex in efsdtgRefeferentiel.AlException)
                    _alErrorMessages.Add(ex.ToString());
            }
            //
            #region Error
            string msgError = string.Empty;
            bool isError = (ArrFunc.IsFilled(_alErrorMessages));
            //
            if (isError)
            {
                for (int i = 0; i < ArrFunc.Count(_alErrorMessages); i++)
                    msgError += _alErrorMessages[i].ToString() + Cst.CrLf2;
                //
                msgError = Ressource.GetString2("Msg_Error", msgError);
                //
                litMsgErr.Text = "<pre class='Msg_Alert'>" + msgError + "</pre>";
            }
            //
            ErrorSummary.Visible = isError;
            ErrorSummary.ToolTip = msgError;
            #endregion
            //20090925 FI gestion du message de warning   
            //RD 20110519 gestion du message de information 
            #region Warning & Info
            bool isWarning = false;
            bool isInfo = false;
            string msgWarning = string.Empty;
            string msgInfo = string.Empty;
            //
            if (false == isError)
            {
                bool isEmptyData = ((false == efsdtgRefeferentiel.IsDataAvailable) || (efsdtgRefeferentiel.TotalRowsData == 0));
                // FI 20240522 [WI937] IsLoadData ou IsSelfNoLoad => même combat
                if (efsdtgRefeferentiel.IsLoadData || efsdtgRefeferentiel.IsSelfNoLoad)
                {
                    if (isEmptyData)
                    {
                        msgWarning = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NoInformation"));
                        //
                        if (m_IsConsultation)
                        {
                            //PL 20101102 Suppression du double affiche de "Aucune information !"
                            //MsgForAlertImmediate += msgWarning + Cst.CrLf + Ressource.GetString2("Msg_NoInformationDet");
                            MsgForAlertImmediate += Ressource.GetString2("Msg_NoInformationDet");
                        }
                        //
                        imgRunProcess.Enabled = false;
                        imgRunProcessIO.Enabled = false;
                        imgRunProcessAndIO.Enabled = false;
                    }
                    else
                    {
                        if (efsdtgRefeferentiel.IsVirtualItemCountError)
                        {
                            msgWarning = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NbRowCountError"));
                            msgWarning += Cst.CrLf + "[" + efsdtgRefeferentiel.LastErrorQueryCount + "]";
                            MsgForAlertImmediate += msgWarning;
                        }
                        else
                        {
                            int top = (int)(SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(System.Int32), -1));
                            if ((efsdtgRefeferentiel.IsDataAvailable) && (top > -1) && (efsdtgRefeferentiel.TotalRowsData == top))
                            {
                                msgWarning = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NbRowMax", top.ToString(), Cst.Space));
                                MsgForAlertImmediate += Ressource.GetString2("Msg_NbRowMax", top.ToString(), Cst.HTMLBreakLine2);
                            }
                        }
                    }
                }
                else if (IsPostBack && (StrFunc.IsFilled(PARAM_EVENTARGUMENT) && (PARAM_EVENTARGUMENT.ToUpper() == "SEARCHDATA")))
                {
                    int top = (int)(SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(System.Int32), -1));
                    if ((efsdtgRefeferentiel.IsDataAvailable) && (top > -1) && (efsdtgRefeferentiel.TotalRowsData == top))
                        msgWarning = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NbRowMax", top.ToString(), Cst.Space));

                    // FI 2019103 [XXXX] Add
                    if (isEmptyData)
                    {
                        imgRunProcess.Enabled = false;
                        imgRunProcessIO.Enabled = false;
                        imgRunProcessAndIO.Enabled = false;
                    }
                }
                else if (((false == IsPostBack) && isEmptyData) || efsdtgRefeferentiel.IsSelfClear)
                {
                    // RD 20110520 [17464]
                    // A la première ouverture, si le DataGrid est vide alors afficher un message "Information"
                    // FI 20180605 [24001] Msg particulier si IsSpheresProcessTypeUsingNormMsgFactory 
                    if (IsSpheresProcessTypeUsingNormMsgFactory)
                        msgInfo = Ressource.GetString2("Msg_NotLoad2");
                    else
                        msgInfo = Ressource.GetString2("Msg_NotLoad");


                    imgRunProcess.Enabled = IsSpheresProcessTypeUsingNormMsgFactory && SessionTools.IsActionEnabled;
                    imgRunProcessIO.Enabled = false;
                    imgRunProcessAndIO.Enabled = false;

                }

                isWarning = StrFunc.IsFilled(msgWarning);
                if (isWarning)
                    litMsgWarning.Text = "<pre class='Msg_Warning'>" + msgWarning + "</pre>";
                else
                {
                    isInfo = StrFunc.IsFilled(msgInfo);
                    if (isInfo)
                        litMsgInformation.Text = "<pre class='Msg_Information_1'>" + msgInfo + "</pre>";
                }
            }

            InformationSummary.Visible = isInfo;
            InformationSummary.ToolTip = msgInfo;

            WarningSummary.Visible = isWarning;
            WarningSummary.ToolTip = msgWarning;
            #endregion Warning & Info
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20210115 [XXXXX]
        // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
        private void DisplayZip()
        {
            #region bouton zip visible oui/non
            Boolean isZipVisible = false;
            if (efsdtgRefeferentiel.IsDataSourceAvailable)
            {
                DataTable dt = (efsdtgRefeferentiel.DsData.Tables[0]);
                isZipVisible = (Cst.OTCml_TBL.VW_MCO.ToString() == dt.TableName);
                isZipVisible |= (Cst.OTCml_TBL.VW_INVMCO.ToString() == dt.TableName);
                isZipVisible |= (Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString() == dt.TableName);
                isZipVisible |= (Cst.OTCml_TBL.ATTACHEDDOC.ToString() == dt.TableName);
                isZipVisible |= (Cst.OTCml_TBL.VW_ATTACHEDDOC_TRACKER_L.ToString() == dt.TableName);
                isZipVisible |= (("MCO" == dt.TableName) && m_IsConsultation);
            }
            imgZIP.Visible = isZipVisible;
        }

        /// <summary>
        /// Affiche l'horodatage du dernier rafraichissement des datas (et la durée SQL nécessaire)
        /// </summary>
        /// FI 20210115 [XXXXX]
        private void DisplayLastRefresh()
        {
            // FI 20210112 [XXXXX] Call DisPlayLastRefresh or EraseLastRefresh
            if (efsdtgRefeferentiel.IsLoadData)
                SetLastRefresh();
            else if (efsdtgRefeferentiel.IsSelfClear)
                EraseLastRefresh();
            //Remarque : il y a un maintient de l'état du contrôle via le viewstate             
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        // FI 20210112 [XXXXX] Add
        // EG 20210331 [25556] Affichage du propriétaire du template actif avant date du dernier refresh
        private void EraseLastRefresh()
        {
            if (this.FindControl("lblRightSubtitle") is Label ctrl)
            {
                ctrl.Text = efsdtgRefeferentiel.LocalLstConsult.template.titleOwner;
                ctrl.Visible = true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        private void TrapException(Exception pEx)
        {
            if (IsNoFailure)
            {
                SpheresException2 ex = SpheresExceptionParser.GetSpheresException(null, pEx);
                _alException.Add(ex);
                ((PageBase)Page).WriteLogException(ex);
            }
            else
            {
                throw pEx;
            }
        }

        /// <summary>
        /// Initialise une nouvelle instance de  _dynamicArgs à partir des customObjects et des DA présents dans l'URL
        /// <para></para>
        /// </summary>
        private void LoadDynamicArg()
        {
            _dynamicArgs = new Dictionary<string, ReferentialsReferentialStringDynamicData>();

            // Alimentation à partir des customObjects
            if (null != CustomObjects)
            {
                MQueueparameter[] mqp = GetCustomObjectParameter2();
                if (ArrFunc.IsFilled(mqp))
                {
                    for (int i = 0; i < mqp.Length; i++)
                    {

                        ReferentialsReferentialStringDynamicData sDD = new ReferentialsReferentialStringDynamicData(
                            mqp[i].dataType.ToString(), mqp[i].id, mqp[i].Value)
                        {
                            source = DynamicDataSourceEnum.GUI
                        };
                        _dynamicArgs.Add(mqp[i].id, sDD);
                    }
                }
                // FI 20200602 [25370] call CheckDynamicArgs
                CheckDynamicArgs();
            }
            // Alimentation à partir des DA
            if (StrFunc.IsFilled(Request.QueryString["DA"]))
            {
                //PL 20160914 Use DecodeDA()
                //Dictionary<string, StringDynamicData> dynamicArgs2 = ReferentialTools.CalcDynamicArgumentFromHttpParameter2(Request.QueryString["DA"]);
                Dictionary<string, ReferentialsReferentialStringDynamicData> dynamicArgsURL = ReferentialTools.CalcDynamicArgumentFromHttpParameter2(Ressource.DecodeDA(Request.QueryString["DA"]));
                if (ArrFunc.IsFilled(dynamicArgsURL))
                {
                    foreach (string key in dynamicArgsURL.Keys)
                    {
                        if (false == _dynamicArgs.ContainsKey(key))
                        {
                            // Ajout d'un Dynamic Argument de type URL s'il n'existe pas en tant que GUI
                            _dynamicArgs.Add(key, dynamicArgsURL[key]);
                        }
                        else
                        {
                            if (false == IsPostBack)
                            {
                                // si (false == IsPostBack) Le _dynamicArgs (GUI) existant est écrasé par la dynamic argument présent dans l'URL (cette initialisation écrase par exemple la valeur par défaut)
                                // Par la suite (IsPostBack) le _dynamicArgs présent dans l'URL n'est plus considéré. 
                                //
                                // Remarque => On rentre dans ce cas lorsque Spheres® génère un template temporaire (prefix *). LSTPARAM est alors également initialisé avec les valeurs présentes dans L'URL 
                                // voir alimentation du paramètre pIsNewTemporaryTemplate avant l'appel à ReferentialWeb.GetTemplate 
                                DynamicDataSourceEnum previousSource = _dynamicArgs[key].source;
                                _dynamicArgs[key] = dynamicArgsURL[key];
                                _dynamicArgs[key].source = _dynamicArgs[key].source | previousSource;
                            }
                        }
                    }
                }
            }
        }
        // EG 20200923 [XXXXX] New 
        private void LoadDynamicArgRunTime()
        {
            // Alimentation en dur d'autre _dynamicArgs. ces derniers seront de type RUNTIME
            switch (SpheresProcessType)
            {
                case Cst.ProcessTypeEnum.EARGEN:
                    EARAddDynamicArg(); // FI 20180907 [24160] Appel à EARAddDynamicArg
                    break;
                case Cst.ProcessTypeEnum.RIMGEN:
                    RIMGENAddDynamicArg();  // FI 20190523 [XXXXX] add
                    break;
                case Cst.ProcessTypeEnum.CMGEN:
                    // FI 20240522 [WI937] cet appel n'est plus nécessaire
                    //CMGENAddDynamicArg();
                    break;
            }
        }
        /// <summary>
        /// Ajout en dur d'un Dynamic Argument pour le lancement des EARS
        /// </summary>
        /// FI 20180907 [24160] Add Method
        /// FI 20200205 [XXXXX] Usage de ReferentialsReferentialStringDynamicData
        private void EARAddDynamicArg()
        {

            if (false == _dynamicArgs.Keys.Contains("DATE1"))
                throw new NullReferenceException("DynamicData DATE1 Missing");

            StringDynamicData sddDate1 = _dynamicArgs["DATE1"];
            DateTime date1 = new DtFunc().StringDateISOToDateTime(sddDate1.value);

            DateTime dtStart = EARTools.LaunchProcessGetStartDate(SessionTools.CS, date1);

            _dynamicArgs.Add("DTSTART", new ReferentialsReferentialStringDynamicData()
            {
                datatypeSpecified = true,
                datatype = sddDate1.datatype.ToString(),
                name = "DTSTART",
                value = DtFunc.DateTimeToStringDateISO(dtStart),
                source = DynamicDataSourceEnum.RUNTIME
            });
        }

        /// <summary>
        /// Ajout en dur d'un Dynamic Argument pour le lancement des éditions
        /// </summary>
        /// FI 20190523 [XXXXX]
        /// FI 20200205 [XXXXX] Usage de ReferentialsReferentialStringDynamicData
        private void RIMGENAddDynamicArg()
        {
            //FI 20190523 [XXXXX] Mise en place du paramètre {ISMULTIPARTIES} 
            // Existe-t-il des messages MULTI-PARTIES valides oui/non ?
            string query = "select count(1) from dbo.CNFMESSAGE cnfmsg where cnfmsg.MSGTYPE='MULTI-PARTIES'";
            query += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "cnfmsg");

            int nbRow = 0;
            object obj = DataHelper.ExecuteScalar(CSTools.SetCacheOn(SessionTools.CS), CommandType.Text, query);
            if (null != obj)
                nbRow = Convert.ToInt32(obj);

            _dynamicArgs.Add("ISMULTIPARTIES", new ReferentialsReferentialStringDynamicData()
            {
                datatypeSpecified = true,
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = "ISMULTIPARTIES",
                value = (nbRow > 0) ? "true" : "false",
                source = DynamicDataSourceEnum.RUNTIME
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20190614 [23912] Add 
        /// FI 20200205 [XXXXX] Usage de ReferentialsReferentialStringDynamicData
        private void CMGENAddDynamicArg()
        {

            if (false == _dynamicArgs.Keys.Contains("DATE1"))
                throw new NullReferenceException("DynamicData DATE1 Missing ");

            StringDynamicData sddDate1 = _dynamicArgs["DATE1"];
            DateTime date1 = new DtFunc().StringDateISOToDateTime(sddDate1.value);

            DateTime dtStart = date1.AddMonths(-2);
            _dynamicArgs.Add("DTSTART", new ReferentialsReferentialStringDynamicData()
            {
                datatypeSpecified = true,
                datatype = sddDate1.datatype.ToString(),
                name = "DTSTART",
                value = DtFunc.DateTimeToStringDateISO(dtStart),
                source = DynamicDataSourceEnum.RUNTIME
            });
        }

        /// <summary>
        /// Alimente la table TblInfos avec les critères et les tri appliqués par la consultation
        /// </summary>
        /// FI 20150120 [XXXXX] Modify
        /// EG 20151222 Refactoring
        /// EG 20190926 EnableViewState = False pour ddlSearch
        /// FI 2019103 [XXXX] Refactoring => chgt de signature
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        /// EG 20210222 [XXXXX] Appel RefreshPage (présentes dans PageBase.js) en remplacement de RefreshPage2
        /// FI 20220930 [XXXXX] lblFilters et imgDisabledFilter sont désormais présents dans le panel WhereDataRow (à l'image du panel OrderByDataRow)
        private void LoadTblInfos(InfosLstWhere[] pInfosLstWhere, ArrayList[] pInfosOrder)
        {
            if (divInfos != null)
            {
                #region Infos Where
                if (ArrFunc.Count(pInfosLstWhere) > 0)
                {
                    string mode = (efsdtgRefeferentiel.ListType == Cst.ListType.Repository) ? "SELFRELOAD_" : "SELFCLEAR_";

                    Panel dataPnl = new Panel
                    {
                        ID = divInfos.ID + "WhereDataRow",
                        CssClass = mainMenuClassName
                    };

                    dataPnl.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("lblFilters") + ":", string.Empty));

                    bool isContainsOptionalFilter = false;
                    if (ArrFunc.IsFilled(pInfosLstWhere))
                        isContainsOptionalFilter = pInfosLstWhere.Where(x => (false == x.IsMandatory)).Count() > 0;

                    if (isContainsOptionalFilter)
                    {
                        // FI 20221026 [XXXXX] lnkDisabledFilter is now a Label
                        Label control = new Label
                        {
                            ID = "lnkDisabledFilter",
                            CssClass = mainMenuClassName,
                            Text = @"<i class='fa'></i>",
                            ToolTip = Ressource.GetString("disableFiltersInformation")
                        };

                        if (mode == "SELFRELOAD_")
                            // FI 20201222 [XXXXX] Call RefreshPage2 avec argument SELFRELOAD_ (même comportement que lorsque l'on change un filtre via lstCriteria)
                            // FI 20221026 [XXXXX] postback only if mode == "SELFRELOAD_"
                            control.Attributes.Add("onclick", StrFunc.AppendFormat($"RefreshPage('{control.ClientID}','{mode}');return false;"));

                        dataPnl.Controls.Add(control);
                    }


                    for (int i = 0; i < ArrFunc.Count(pInfosLstWhere); i++)
                    {
                        if (i != 0)
                            dataPnl.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("And", true), HtmlTools.cssInfosAnd));

                        Panel dataPnlFilter = new Panel();
                        // RD 20170227 [xxxxx] Remplacet <brnobold/> par espace
                        dataPnlFilter.Controls.Add(HtmlTools.NewLabel(pInfosLstWhere[i].ColumnIdentifier.Replace(Cst.HTMLBreakLine, Cst.Space).Replace("<brnobold/>", Cst.Space), string.Empty));
                        dataPnlFilter.Controls.Add(HtmlTools.NewLabel(pInfosLstWhere[i].GetDisplayOperator(), string.Empty));

                        // Ne pas afficher la "valeur" pour les opérateurs: Checked et Unchecked
                        if (("Checked" != pInfosLstWhere[i].Operator) && ("Unchecked" != pInfosLstWhere[i].Operator))
                            dataPnlFilter.Controls.Add(HtmlTools.NewLabel(pInfosLstWhere[i].LstValue, string.Empty));

                        if (false == pInfosLstWhere[i].IsMandatory)
                        {
                            Label control = new Label
                            {
                                ID = "imgDisabledFilterPosition" + pInfosLstWhere[i].Position.ToString(),
                                CssClass = "fa-icon",
                                Text = @"<i class='fa fa-times'></i>",
                                ToolTip = Ressource.GetString("disablethisFilter")
                            };

                            // FI 20201222 [XXXXX] Call RefreshPage2 avec argument SELFRELOAD_ (même comportement que lorsque l'on change un filtre via lstCriteria)
                            // FI 20221026 [XXXXX] postback only if mode == "SELFRELOAD_"
                            if (mode == "SELFRELOAD_")
                                control.Attributes.Add("onclick", StrFunc.AppendFormat($"RefreshPage('{control.ClientID}','{mode}');return false;"));
                            dataPnlFilter.Controls.Add(control);
                        }
                        dataPnl.Controls.Add(dataPnlFilter);
                    }

                    //Add Filters Infos
                    divInfos.Controls.Add(dataPnl);
                }
                #endregion

                #region Infos OrderBy
                if (ArrFunc.IsFilled(pInfosOrder) && ArrFunc.IsFilled(pInfosOrder[0]))
                {
                    Panel dataPnlOrder = new Panel
                    {
                        ID = divInfos.ID + "OrderByDataRow",
                        CssClass = HtmlTools.cssInfosSort
                    };
                    dataPnlOrder.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("SortedBy") + ":", string.Empty));
                    Panel dataPnlOrderdet = new Panel();

                    for (int i = 0; i < pInfosOrder[0].Count; i++)
                    {
                        if (i != 0)
                            dataPnlOrderdet.Controls.Add(HtmlTools.NewLabel(",", HtmlTools.cssInfosAnd));
                        //PL 20181115 Add Replace(Cst.HTMLBreakLine,Cst.HTMLSpace)
                        dataPnlOrderdet.Controls.Add(HtmlTools.NewLabel(pInfosOrder[0][i].ToString().Replace(Cst.HTMLBreakLine, Cst.HTMLSpace), string.Empty));
                        dataPnlOrderdet.Controls.Add(HtmlTools.NewLabel(pInfosOrder[1][i].ToString().ToUpper(), string.Empty));
                    }
                    dataPnlOrder.Controls.Add(dataPnlOrderdet);
                    divInfos.Controls.Add(dataPnlOrder);

                }
                #endregion

                divInfos.CssClass = this.mainMenuClassName;
            }
        }

        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void LoadTblSearch()
        {
            WCTextBox2 txt = new WCTextBox2("txtSearch", false, false, false, "txtCapture", null)
            {
                CssClass = "txtSearch",
                AutoPostBack = false,
                EnableViewState = true,
                AutoCompleteType = AutoCompleteType.Search
            };
            txt.Attributes.Add("onblur", "TXTSearchRefresh(event);");
            txt.Attributes.Add("onkeydown", "TXTSearchRefresh(event);");

            WCDropDownList2 ddl = new WCDropDownList2(false, HtmlTools.cssinfosMaskHide)
            {
                IsSetTextOnTitle = true,
                ID = "ddlSearch",
                EnableViewState = false
            };
            ControlsTools.DDLLoadEnum<Cst.DisplayMask>(ddl, false, true, string.Empty);
            ddl.Attributes.Add("onchange", "DDLSearchRefresh();");
            ddl.Attributes.Add("onblur", "DDLSearchRefresh();");

            Panel panelDDL = new Panel()
            {
                ID = "tbSearchDLL"
            };
            panelDDL.Controls.Add(ddl);

            Panel panelTXT = new Panel()
            {
                ID = "tbSearchTXT"
            };
            panelTXT.Controls.Add(txt);



            tbSearch.Controls.Add(panelDDL);
            tbSearch.Controls.Add(panelTXT);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string IsNoLockProcess(DataRow[] pRowsSelected)
        {
            string ret = string.Empty;
            IdMenu.Menu? menu = IdMenu.ConvertToMenu(efsdtgRefeferentiel.IDMenu);
            if (null != menu)
            {
                switch (menu)
                {
                    case IdMenu.Menu.POSKEEPING_UNCLEARING:
                        ret = LockTools.SearchProcessLocks(SessionTools.CS, TypeLockEnum.ENTITYMARKET, LockTools.Shared, SessionTools.AppSession);
                        break;
                    case IdMenu.Menu.InputTrade_CLEARSPEC:
                        // EG 20140522 Sur une clôture spécifique les lignes demandées sont de fait sur la même série donc : LECTURE de la row[0]
                        DataRow row = pRowsSelected[0];
                        Nullable<int> idEM = null;
                        if (row.Table.Columns.Contains("IDEM") && (false == Convert.IsDBNull(row["IDEM"])))
                            idEM = Convert.ToInt32(row["IDEM"]);
                        string identifier = string.Empty;
                        if (row.Table.Columns.Contains("ENTITY_IDENTIFIER") && (false == Convert.IsDBNull(row["ENTITY_IDENTIFIER"])))
                            identifier = row["ENTITY_IDENTIFIER"].ToString();
                        if (row.Table.Columns.Contains("MARKET") && (false == Convert.IsDBNull(row["MARKET"])))
                            identifier += " - " + row["MARKET"].ToString();
                        LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, idEM.Value, identifier, LockTools.Exclusive);
                        ret = LockTools.SearchProcessLocks(SessionTools.CS, lockEntityMarket, SessionTools.AppSession);
                        break;
                    case IdMenu.Menu.POSKEEPING_CLEARINGBULK:
                        ret = LockTools.SearchProcessLocks(SessionTools.CS, TypeLockEnum.ENTITYMARKET, LockTools.Exclusive, SessionTools.AppSession);
                        break;
                    default:
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Contrôle suite à une demande de traitement
        /// <para>Si la demande est validée, poste une confirmation pour ce traitement </para>
        /// </summary>
        /// FI 20180605 [24001] Refactoring pour gérer IsSpheresProcessTypeUsingNomrMsgFactory
        /// EG 20201014 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private void CheckAndConfirmRequest()
        {
         
            DataRow[] rows = null;

            string msgAlert = string.Empty;
            if (false == IsSpheresProcessTypeUsingNormMsgFactory)
            {
                rows = ((DataView)efsdtgRefeferentiel.DataSource).Table.Select("ISSELECTED=true");
                if (ArrFunc.IsEmpty(rows))
                    msgAlert = Ressource.GetString("Msg_TradesNoneSelected"); //Aucune ligne sélectionnée!
                else
                    // EG 20140522 Refactoring Contrôle traitement LockProcess
                    msgAlert = IsNoLockProcess(rows);
            }

            if (StrFunc.IsEmpty(msgAlert))
            {
                string processMsg = string.Empty;
                if (IsSpheresProcessTypeUsingNormMsgFactory)
                    processMsg += Ressource.GetString("Msg_Process_Confirm_NormMsgFactory");
                else if (1 == ArrFunc.Count(rows))
                    processMsg += Ressource.GetString("Msg_Process_ConfirmOne");
                else
                    processMsg += Ressource.GetString2("Msg_Process_Confirm", ArrFunc.Count(rows).ToString());

                #region Poser la question à l'utilisateur pour confirmer le lancement du Traitement
                if (efsdtgRefeferentiel.ObjectName == "POSKEEPING_TRANSFERBULK")
                {
                    JavaScript.TransferStartUpImmediate(this, processMsg, Target_OnProcessClick, m_btnRunProcess_Clicked.CommandName + ";" + m_btnRunProcess_Clicked.CommandArgument, "ProcessCancel");
                }
                else
                {
                    if (false == IsSpheresProcessTypeUsingNormMsgFactory)
                    {
                        // EG 20201014 [XXXXX] Correction
                        string additionnalMsg = m_btnRunProcess_Clicked.Text;
                        int i = additionnalMsg.IndexOf("</i>");
                        if (0 < i)
                            additionnalMsg = additionnalMsg.Remove(0, i + 4);
                        processMsg += Cst.CrLf + Cst.CrLf + additionnalMsg + " ?";
                    }
                    JavaScript.ConfirmStartUpImmediate(this, processMsg, Target_OnProcessClick, m_btnRunProcess_Clicked.CommandName + ";" + m_btnRunProcess_Clicked.CommandArgument, "ProcessCancel");
                }
                #endregion
            }
            else
                JavaScript.DialogStartUpImmediate((PageBase)this, msgAlert, false, ProcessStateTools.StatusWarningEnum);
        }

        /// <summary>
        /// Vérifie que le dossier (FileWatcher) ou la queue (MSMQ), suffix inclu, est bien accessible.
        /// <para>NB: si non accessible, une erreur est déclenchée.</para>
        /// </summary>
        /// <param name="pProcess"></param>
        // PL 20220614 Use MOMSettings.CheckMOMSettings()
        private void CheckMOMSettings(string pCS, Cst.ProcessTypeEnum pProcess, int pIdA_Entity)
        {
            MOMSettings settings = MOMSettings.LoadMOMSettings(pProcess);

            //PL 20221025 New signature of CheckMOMSettings() with 3 parameters
            settings.CheckMOMSettings(pCS, pProcess, pIdA_Entity);
        }

        /// <summary>
        /// Retourne true si le bouton IO est présent
        /// </summary>
        /// <returns></returns>
        private bool IsDisplayBtnIO()
        {
            bool ret = (m_IDIOTask > 0);

            if (m_IsSpheresProcess)
            {
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = true;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si le bouton ProcessAndIO est présent
        /// </summary>
        /// <returns></returns>
        private bool IsDisplayBtnProcessAndIO()
        {
            bool ret = false;

            if (m_IsSpheresProcess)
            {
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RIMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = true;
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne la Ressource du bounton btnIO 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnIO()
        {
            string ret = Request.QueryString["IORes"];
            if (StrFunc.IsEmpty(ret))
            {
                ret = "btnIO";
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = "btnIO_SendMsg";
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne le nom de la Ressource du bounton btnProcess 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnProcess()
        {
            string ret = Request.QueryString["ProcessRes"];
            //
            if (m_IsSpheresProcess)
            {
                if (StrFunc.IsEmpty(ret))
                {
                    ret = "btnGenerate";
                    switch (SpheresProcessType)
                    {
                        case Cst.ProcessTypeEnum.CIGEN:
                        case Cst.ProcessTypeEnum.CMGEN:
                        case Cst.ProcessTypeEnum.RIMGEN:
                        case Cst.ProcessTypeEnum.RMGEN:
                            ret += "_" + SpheresProcessType.ToString();
                            break;
                    }
                }
            }
            else
                ret = "btnProcess";
            return ret;
        }
        /// <summary>
        /// Retourne le nom de la Ressource du bounton btnProcessAndIO 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnProcessAndIO()
        {
            string ret = string.Empty;
            if (m_IsSpheresProcess)
                ret = "btnProcessAndIO_" + SpheresProcessType;
            return ret;
        }

        /// <summary>
        /// Convertie la property dynamicArgs en StringDynamicDatas
        /// </summary>
        /// <returns></returns>
        // EG 20210324 [XXXXX] Correction Cast StringDynamicData
        private StringDynamicDatas ConvertDynamicArgs()
        {
            StringDynamicDatas ret = null;
            //
            if (null != DynamicArgs)
            {
                List<StringDynamicData> lst = new List<StringDynamicData>();
                IEnumerator listDA = DynamicArgs.Values.GetEnumerator();
                while (listDA.MoveNext())
                {
                    StringDynamicData sddSource = (StringDynamicData)listDA.Current;
                    StringDynamicData sdd = new StringDynamicData()
                    {
                        dataformat = sddSource.dataformat,
                        datatype = sddSource.datatype,
                        datatypeSpecified = sddSource.datatypeSpecified,
                        spheresLib = sddSource.spheresLib,
                        sql = sddSource.sql,
                        name = sddSource.name,
                        value = sddSource.value,
                    };
                    lst.Add(sdd);
                }
                ret = new StringDynamicDatas
                {
                    data = lst.ToArray()
                };
            }
            return ret;
        }

        /* FI 20200602 [25370] Mise en commentaire 
        /// <summary>
        /// Alimente les customObjects avec les valeurs des dynamicArguments passés via l'URL
        /// </summary>
        /// EG 20170822 [23342] New DateTimeOffset
        /// FI 20200205 [XXXXX] Refactoring 
        private void SetCustomControlURLDA()
        {
            if ((null != CustomObjects) && (null != dynamicArgs) && (StrFunc.IsFilled(Request.QueryString["DA"])))
            {
                // FI 20200205 [XXXXX] utilisation instruction Link
                // Considération des DynamicArguments passés dans l'URL pour alimenter les contrôles GUI 
                List<ReferentialsReferentialStringDynamicData> lst = (from item in DynamicArgs.Values.Where(x => x.source.HasFlag(DynamicDataSourceEnum.URL))
                                                                      select item).ToList();
                if (lst.Count > 0)
                {
                    foreach (var item in lst)
                        // FI 20200205 [XXXXX] utilisation  de SetCustomObjectControl
                        SetCustomObjectControl(item as StringDynamicData);
                }
            }
        }
        */

        /// <summary>
        /// Alimente les controles customObjects avec les valeurs des dynamicArguments de type GUI (dont les valeurs initiales peuvent être remplacées par les données présentes dans LSTPARAM)
        /// </summary>
        /// FI 20200602 [25370] Add
        private void SetCustomObjectControlGUIDA()
        {
            if ((null != CustomObjects) && (null != DynamicArgs))
            {
                foreach (ReferentialsReferentialStringDynamicData item in DynamicArgs.Values.Where(x => x.source.HasFlag(DynamicDataSourceEnum.GUI)))
                {
                    CustomObject co = CustomObjects.customObject.Where(x => x.ClientId == item.name && x.IsControlData).FirstOrDefault();
                    if (null == co)
                        throw new InvalidProgramException($"CustomObject {item.name} not found");
                    SetCustomObjectControl2(co, item.GetDataValue(SessionTools.CS, null));
                }
            }
        }

        /// <summary>
        /// Alimente le contrôle associé au CustomObject {pCustomObj} avec {pValue}
        /// </summary>
        /// <param name="pCustomObj"></param>
        /// <param name="pValue">valeur au format ISO</param>
        /// FI 20200602 [25370] add 
        private void SetCustomObjectControl2(CustomObject pCustomObj, string pValue)
        {
            Control control = this.FindControl(pCustomObj.CtrlClientId);
            if (null == control)
                throw new NotSupportedException($"control {pCustomObj.CtrlClientId}");

            if (TypeData.IsTypeDateTimeOffset(pCustomObj.DataType))
            {
                if (!(control is WCZonedDateTime timestamp))
                    throw new NotSupportedException($"control {control.ClientID} is not a WCZonedDateTime");

                if (StrFunc.IsFilled(pValue))
                {
                    DateTimeOffset dt = DateTimeOffset.Parse(pValue, CultureInfo.CurrentCulture.DateTimeFormat);
                    DateTimeFormatInfo dfi = DtFunc.DateTimeOffsetPattern;
                    if (timestamp.IsAltTime)
                    {
                        timestamp.Text = dt.LocalDateTime.ToString(DtFunc.FmtShortDate, dfi);
                        timestamp.Time.Text = dt.LocalDateTime.ToString(DtFunc.FmtLongTime, dfi);
                    }
                    else
                    {
                        timestamp.Text = dt.LocalDateTime.ToString(DtFunc.FmtDateLongTime, dfi);
                    }
                }
                else
                {
                    timestamp.Text = null;
                    if (timestamp.IsAltTime)
                        timestamp.Time.Text = null;
                }
            }
            else if (TypeData.IsTypeDate(pCustomObj.DataType))
            {
                if (!(control is WCTextBox2 txtBox))
                    throw new NotSupportedException($"control {control.ClientID} is not a WCTextBox2");
                if (StrFunc.IsFilled(pValue))
                    txtBox.Text = new DtFunc().GetDateTimeString(pValue, DtFunc.FmtShortDate);
                else
                    txtBox.Text = null;
            }
            else if (TypeData.IsTypeBool(pCustomObj.DataType)) // 20200923 [XXXXX] Add
            {
                if (!(this.FindControl(pCustomObj.CtrlClientId) is WCCheckBox2 chkBox))
                    throw new NotSupportedException($"control {control.ClientID} is not a WCCheckBox2");
                chkBox.Checked = BoolFunc.IsTrue(pValue);
            }
            else if (TypeData.IsTypeString(pCustomObj.DataType) || TypeData.IsTypeInt(pCustomObj.DataType)) // 20200923 [XXXXX] Add
            {
                if (!(this.FindControl(pCustomObj.CtrlClientId) is WCDropDownList2 ddlBox))
                {
                    if (!(this.FindControl(pCustomObj.CtrlClientId) is WCTextBox2 txtBox))
                    {
                        if (this.FindControl(pCustomObj.CtrlClientId) is Label lbl)
                        {
                            lbl.Text = "(id:" + pValue + ")";
                            lbl.Font.Size = FontUnit.Smaller;
                        }
                        else
                            throw new NotSupportedException($"control {control.ClientID} is not a WCDropDownList2 or WCTextBox2 or Label ");
                    }
                    else
                    {
                        txtBox.Text = pValue;
                    }
                }
                else
                {
                    ControlsTools.DDLSelectByValue(ddlBox, pValue); // 20200923 [XXXXX] Add
                }
            }
            else if (TypeData.IsTypeTime(pCustomObj.DataType)) // FI 20210119 [XXXXX] Gestion de type time  
            {
                if (!(control is WCTextBox2 txtBox))
                    throw new NotSupportedException($"control {control.ClientID} is not a WCTextBox2");
                if (StrFunc.IsFilled(pValue))
                    txtBox.Text = new DtFunc().GetTimeString(pValue, DtFunc.FmtLongTime);
                else
                    txtBox.Text = null;
            }
            else if (TypeData.IsTypeDec(pCustomObj.DataType)) // FI 20210322 [XXXXX] Gestion de type dec 
            {
                if (!(control is WCTextBox2 txtBox))
                    throw new NotSupportedException($"control {control.ClientID} is not a WCTextBox2");

                if (StrFunc.IsFilled(pValue))
                    txtBox.Text = StrFunc.FmtDecimalToCurrentCulture(pValue);
                else
                    txtBox.Text = null;
            }
        }
        /// <summary>
        /// Alimente le contrôle associé au CustomObject {pCustomObj} avec valeur par défaut (lorsque définie)
        /// </summary>
        /// <param name="pCustomObj"></param>
        /// FI 20200602 [25370] Add
        private void SetCustomObjectControlDefault(CustomObject pCustomObj)
        {
            string value = GetCustomObjectControlDefault(pCustomObj);
            SetCustomObjectControl2(pCustomObj, value);
        }

        /// <summary>
        ///  Retourne la Valeur par défaut pour alimentation du contrôle associé au CustomObject {pCustomObj} 
        /// </summary>
        /// <param name="pCustomObj"></param>
        /// <returns></returns>
        /// FI 20200602 [25370] Add
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private string GetCustomObjectControlDefault(CustomObject pCustomObj)
        {
            string value = null;

            if (TypeData.IsTypeDate(pCustomObj.DataType) || TypeData.IsTypeDateTimeOffset(pCustomObj.DataType))
            {
                string defaultValue = (pCustomObj.IsMandatory ? DtFunc.TODAY : string.Empty);
                if (pCustomObj.ContainsDefaultValue)
                    defaultValue = pCustomObj.DefaultValue;

                if (StrFunc.IsFilled(defaultValue))
                {
                    DtFuncML dtFuncML = new DtFuncML(SessionTools.CS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, 0, null);

                    string fmt = TypeData.IsTypeDateTimeOffset(pCustomObj.DataType) ? DtFunc.FmtTZISOLongDateTime : DtFunc.FmtISODate;
                    value = dtFuncML.GetDateTimeString(defaultValue, fmt);
                }
            }
            else if (TypeData.IsTypeInt(pCustomObj.DataType) || TypeData.IsTypeString(pCustomObj.DataType))
            {
                string defaultValue = null;
                if (pCustomObj.ContainsDefaultValue)
                    defaultValue = pCustomObj.DefaultValue;
                else if (pCustomObj.ClientId == "ENTITY") // Petit cas particulier (compatibilité ascendante)
                    defaultValue = "ENTITY";

                if (StrFunc.IsFilled(defaultValue))
                {
                    value = defaultValue;
                    if ((defaultValue == "ITM") && (StrFunc.IsFilled(Request.QueryString["P1"])) && (Request.QueryString["P1"].StartsWith("ABN"))) // Petit cas particulier (compatibilité ascendante)
                        value = "NTM";
                }

                if (StrFunc.IsFilled(defaultValue))
                {
                    if (defaultValue == "ENTITY" && SessionTools.User.Entity_IdA > 0)
                        value = SessionTools.User.Entity_IdA.ToString();
                }
            }
            else if (pCustomObj.ContainsDefaultValue)
            {
                value = pCustomObj.DefaultValue;
            }
            return value;
        }


        /// <summary>
        /// Initialisation des checks Validity et TodayUpdate
        /// </summary>
        /// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200107 [25560] Gestion valeur par defaut des données visibles dans les référentiels 
        // EG 20210222 [XXXXX] Appel RefreshPage (présentes dans PageBase.js) en remplacement de RefreshPage2
        // EG [XXXXX][WI437] Nouvelles options de filtrage des données sur les référentiels
        private void InitDdlValidityAndTodayUpdate()
        {

            ddlValidityData.IsSetTextOnTitle = false; // FI 20201222 [XXXXX] false puisqu'il y a post de la page
            ControlsTools.DDLLoadEnum<AdditionalCheckBoxEnum2>(ddlValidityData, false, true, string.Empty);
            ddlValidityData.Attributes.Add("onchange", StrFunc.AppendFormat("RefreshPage('{0}','SELFRELOAD_')", ddlValidityData.ClientID)); // FI 20201222 [XXXXX] Call RefreshPage2
            ddlValidityData.EnableViewState = true;
            ddlValidityData.Visible = efsdtgRefeferentiel.Referential.ExistsColumnsDateValidity ||
                efsdtgRefeferentiel.Referential.ExistsColumnsINS && efsdtgRefeferentiel.Referential.ExistsColumnsUPD;
            ddlValidityData.Parent.Visible = ddlValidityData.Visible;
            ddlValidityData.AutoPostBack = false;
            if (ddlValidityData.Visible)
            {
                ddlValidityData.SelectedValue = SessionTools.ValidityData.ToString();
                ListItem item;
                if (false == efsdtgRefeferentiel.Referential.ExistsColumnsDateValidity)
                {
                    item = ddlValidityData.Items.FindByValue(AdditionalCheckBoxEnum2.ActivatedData.ToString());
                    if (null != item)
                        item.Enabled = false;
                    item = ddlValidityData.Items.FindByValue(AdditionalCheckBoxEnum2.DeactivatedData.ToString());
                    if (null != item)
                        item.Enabled = false;
                }
                // Ajout de l'utilisateur
                item = ddlValidityData.Items.FindByValue(AdditionalCheckBoxEnum2.TodayUserData.ToString());
                item.Text += " " + SessionTools.Collaborator.Identifier;
                item = ddlValidityData.Items.FindByValue(AdditionalCheckBoxEnum2.TodayUserCreateData.ToString());
                item.Text += " " + SessionTools.Collaborator.Identifier;
                item = ddlValidityData.Items.FindByValue(AdditionalCheckBoxEnum2.TodayUserUpdateData.ToString());
                item.Text += " " + SessionTools.Collaborator.Identifier;
            }
        }
        /// <summary>
        /// Appel à un traitement en c# qui s'applique sur le jeu de résultat {dt} 
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <param name="pEntityName"></param>
        /// <param name="dt"></param>
        /// FI 20130423 [18601] Refactor
        /// EG 20151019 [21465] Refactoring (Par defaut si non spécifiés dans URL : className = "EfsML.Business.PosKeepingTools" et assemblyName = "EFS.EfsML";
        /// FI 20170616 [XXXXX] Modify
        private void CallProcessCsharp(string pProcessName, string pEntityName, DataTable dt, string pAdditionalArguments)
        {
            string msgReturn = string.Empty;
            object codeReturn = null;

            try
            {
                string methodName = pProcessName;
                switch (methodName)
                {
                    case "AddPosRequestRemoveAlloc":
                        codeReturn = AddPosRequestRemoveAlloc(dt);
                        break;
                    // FI 20170616 [XXXXX]  add UpdateTrade 
                    // => Doit être utilisé par EFS uniquement (fonctionnalité cachée) 
                    case "UpdateTrade":
                        codeReturn = ProcessUpdateTrade(dt);
                        break;
                    case "CalcUTI":
                    case "CalcTradeUTI":
                        codeReturn = CalcUTI(CSTools.SetCacheOn(SessionTools.CS), dt);
                        break;
                    case "CalcPositionUTI":
                        codeReturn = CalcPositionUTI(CSTools.SetCacheOn(SessionTools.CS), dt);
                        break;
                    default:
                        // FI 20180719 [24034] Spheres® contrôle que la date de traitement est bien la date courante avant de faire le post
                        // => Dans le cas contraire, Spheres® demande à l'utilisateur de rafraichir le tableau et d'effectuer une nouvelle demande si nécessaire 
                        Boolean isRequestOk = true;
                        if ((methodName == "AddPosRequestEndOfDay") || (methodName == "AddPosRequestClosingDay"))
                        {
                            string sqlQuery = @"select EMKEY, DTENTITY from dbo.VW_ENTITY_CSSCUSTODIAN";
                            DataRow[] rowcssCustian = DataHelper.ExecuteDataTable(SessionTools.CS, sqlQuery).Select();
                            var request = (from item in dt.Rows.Cast<DataRow>()
                                           select new
                                           {
                                               emKey = Convert.ToString(item["EMKEY"]),
                                               dtBusiness = Convert.ToDateTime(item["DTENTITY"])
                                           });
                            foreach (var item in request)
                            {
                                isRequestOk = (rowcssCustian.Where(x => (Convert.ToString(x["EMKEY"]) == item.emKey) &&
                                                                    (Convert.ToDateTime(x["DTENTITY"]) == item.dtBusiness))).Count() == 1;

                                if (false == isRequestOk)
                                    break;
                            }
                            if (false == isRequestOk)
                            {
                                msgReturn = Ressource.GetString("Msg_PROCESS_EOD_REQUEST_ERROR") + Cst.CrLf;
                            }
                        }

                        if (isRequestOk)
                        {
                            bool isInvoke = IsInvokeMember(pProcessName, pEntityName, pAdditionalArguments, dt, out Type tClassProcess, out methodName, out object[] args);
                            if (isInvoke)
                            {
                                codeReturn = ReflectionTools.InvokeMethod(tClassProcess, methodName.Trim(), args);
                                SizeWidthForAlertImmediate = new Pair<int?, int?>(null, null);
                                if ((tClassProcess.Equals(typeof(PosKeepingTools)) && (methodName.Trim() == "AddPosRequestClearingSPEC")))
                                {
                                    int idTSource = Convert.ToInt32(((MQueueparameters)args[2])["IDT"].Value);
                                    SetRequestTrackBuilderTradeProcess(idTSource, RequestTrackProcessEnum.ClearingSpecific);
                                }
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                msgReturn = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                msgReturn += ex.Message + Cst.CrLf;
                if (null != ex.InnerException)
                    msgReturn += ex.InnerException.Message;
            }
            finally
            {
                if (StrFunc.IsEmpty(msgReturn))
                {
                    if (codeReturn == null)
                    {
                        msgReturn = Ressource.GetString("Msg_ProcessUndone");
                        CloseAfterAlertImmediate = true;
                    }
                    else if (codeReturn.GetType().Equals(typeof(Cst.ErrLevelMessage)))
                    {
                        msgReturn = ((Cst.ErrLevelMessage)codeReturn).Message;
                        if (Cst.ErrLevel.SUCCESS != ((Cst.ErrLevelMessage)codeReturn).ErrLevel)
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                        else
                            CloseAfterAlertImmediate = true;
                    }
                    else if (codeReturn.GetType().Equals(typeof(Cst.ErrLevel)))
                    {
                        if (Cst.ErrLevel.SUCCESS == (Cst.ErrLevel)codeReturn)
                        {
                            msgReturn = Ressource.GetString("Msg_ProcessSuccessfull");
                            CloseAfterAlertImmediate = true;
                        }
                        else
                            msgReturn = Ressource.GetString("Msg_ProcessIncomplete");
                    }
                }
                MsgForAlertImmediate = msgReturn;
            }
        }

        /// <summary>
        /// Génération des messages pour les services Spheres®
        /// <para>Poste des messages pour les enregistrements sélectionnés</para>
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pEntityName"></param>
        /// <param name="dt"></param>
        /// FI 20170306 [22225] Modify
        /// EG 20190214 Gestion DTBUSINESS pour CASHBALANCE
        /// EG 20210322 [XXXXX] Add Recette Facturation (Ajout identifiant du trade ADM dans message informatif)
        /// EG 20240109 [WI801] Invoicing : Suppression et Validation de factures simulées prise en charge par le service
        private void CallProcessService(Cst.ProcessTypeEnum pProcessName, int pEntityId, string pEntityName, DataTable pDt, Nullable<Cst.ProcessIOType> pProcessIoType)
        {
            // FI 20180328 [23871] add dtsys
            DateTime dtSys = OTCmlHelper.GetDateSys(SessionTools.CS);

            int rowsCount = pDt.Rows.Count;

            bool isMonoProcessGenerate = IsMonoProcessGenerate(pProcessName);
            Cst.ProcessTypeEnum[] process = new Cst.ProcessTypeEnum[1] { pProcessName };
            switch (pProcessName)
            {
                case Cst.ProcessTypeEnum.CLOSINGGEN:
                    process = new Cst.ProcessTypeEnum[2] { Cst.ProcessTypeEnum.ACCRUALSGEN, Cst.ProcessTypeEnum.LINEARDEPGEN };
                    break;
                default:
                    break;
            }

            #region Boucle principale
            MQueueTaskInfo taskInfo = null;
            string msgDisplay = string.Empty;
            List<Pair<int, string>> lstInfoForFinalMessage = new List<Pair<int, string>>();

            foreach (Cst.ProcessTypeEnum currentProcessType in process)
            {
                taskInfo = new MQueueTaskInfo
                {
                    process = currentProcessType,
                    connectionString = SessionTools.CS,
                    Session = SessionTools.AppSession,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = currentProcessType
                    }
                };


                //PL 20221025 New signature of CheckMOMSettings() with 3 parameters
                CheckMOMSettings(SessionTools.CS, currentProcessType, taskInfo.Session.IdA_Entity);

                // RD 20121109 / Utilisation d'une seule liste de paramètres commune
                MQueueparameters listCommonCustomParameter = new MQueueparameters()
                {
                    parameter = GetCustomObjectParameter2()
                };
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                List<IdInfo> listIdInfo = new List<IdInfo>();
                MQueueAttributes mQueueAttributes = null;

                if (isMonoProcessGenerate)
                {
                    #region UN SEUL MESSAGE POSTE
                    taskInfo.trackerAttrib.process = currentProcessType;
                    MQueueBase mQueue = null;
                    SQL_Criteria criteria = SQLReferentialData.GetSQLCriteria(SessionTools.CS, efsdtgRefeferentiel.Referential);
                    mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = SessionTools.CS,
                        id = pEntityId,
                        identifier = pEntityName
                    };
                    mQueueAttributes.criteria = criteria;

                    // RD 20121109
                    // Pour eviter de référencer les même objets, Il faut clonner la liste de paramètres commune, car on est dans une boucle,
                    mQueueAttributes.parameters = listCommonCustomParameter.Clone();
                    // EG 20121012 SetDataTracker dans MQueueTools
                    taskInfo.trackerAttrib.info = TrackerAttributes.BuildInfo(currentProcessType, mQueueAttributes.parameters);
                    if (Cst.ProcessTypeEnum.INVOICINGGEN == SpheresProcessType)
                    {
                        mQueue = new InvoicingGenMQueue(mQueueAttributes);
                        msgDisplay = Ressource.GetString2("Msg_InvoicingPostedMessage",
                            Convert.ToString(taskInfo.trackerAttrib.info.Find(match => match.Key.ToString() == "DATA2").Value),
                            Convert.ToString(taskInfo.trackerAttrib.info.Find(match => match.Key.ToString() == "DATA1").Value));
                    }
                    listMQueue.Add(mQueue);
                    #endregion UN SEUL MESSAGE POSTE
                }
                else
                {
                    #region PLUSIEURS MESSAGES POSTES
                    //Ajout des données DATA1..DATA5 pour TRACKER (Tooltip entre autre)
                    // RD 20121109 / Utilisation d'une seule liste de paramètres commune
                    // EG 20121012 SetDataTracker dans MQueueTools
                    taskInfo.trackerAttrib.info = TrackerAttributes.BuildInfo(currentProcessType, listCommonCustomParameter);
                    #region Boucle par ligne sélectionnée du Datagrid
                    foreach (DataRow row in pDt.Rows)
                    {
                        mQueueAttributes = new MQueueAttributes()
                        {
                            connectionString = SessionTools.CS,
                            //Ajout des paramètres des customObjects
                            // RD 20121109 / 
                            // Pour eviter de référencer les même objets, Il faut clonner la liste de paramètres commune, car on est dans une boucle,
                            parameters = listCommonCustomParameter.Clone()
                        };
                        //Ajout des paramètres additionels (CMGEN, RMGEN, RIMGEN...)
                        mQueueAttributes.AdditionalParameters(currentProcessType, pProcessIoType);

                        IdInfo idInfo = null;
                        MQueueBase mQueue = null;
                        string identifier = string.Empty;
                        string gProduct = string.Empty;
                        // RD 20121119 / Bug génération des EARs / GPRODUCT manquant dans le message MQueue
                        if (pDt.Columns.Contains("GPRODUCT") && (false == Convert.IsDBNull(row["GPRODUCT"])))
                            gProduct = row["GPRODUCT"].ToString();
                        //if (0 == i)
                        if (pDt.Rows.IndexOf(row) == 0)
                        {
                            taskInfo.trackerAttrib.gProduct = gProduct;
                            taskInfo.trackerAttrib.caller = efsdtgRefeferentiel.IDMenu;
                        }
                        //
                        #region Set idInfo
                        switch (currentProcessType)
                        {
                            case Cst.ProcessTypeEnum.INVCANCELSIMUL:

                                idInfo = new IdInfo() { id = Convert.ToInt32(row["IDT"]) };
                                identifier = row["IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                mQueue = new InvoicingCancelationSimulationGenMQueue(mQueueAttributes);
                                break;

                            case Cst.ProcessTypeEnum.INVVALIDSIMUL:

                                idInfo = new IdInfo() { id = Convert.ToInt32(row["IDT"]) };
                                identifier = row["IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                mQueueAttributes.AddParameter("IDI", Convert.ToInt32(row["IDI"]));
                                mQueueAttributes.AddParameter("IDA_ENTITY", Convert.ToInt32(row["IDA_ENTITY"]));
                                mQueueAttributes.AddParameter("DTTRADE", Convert.ToDateTime(row["DTTRADE"]));
                                mQueueAttributes.AddParameter("IDSTPRIORITY", row["IDSTPRIORITY"].ToString());
                                mQueueAttributes.AddParameter("IDSTACTIVATION", row["IDSTACTIVATION"].ToString());
                                mQueueAttributes.AddParameter("SCREENNAME", row["SCREENNAME"].ToString());

                                mQueue = new InvoicingValidationSimulationGenMQueue(mQueueAttributes);
                                break;

                            case Cst.ProcessTypeEnum.RIMGEN:
                                #region RIMGEN
                                idInfo = new IdInfo()
                                {
                                    id = Convert.ToInt32(row["IDA"])
                                };
                                identifier = row["ACSENDTO_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                string cnfType = row["CNFTYPE"].ToString();
                                if (Enum.IsDefined(typeof(NotificationTypeEnum), cnfType))
                                {
                                    if (null == mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFTYPE])
                                        mQueueAttributes.AddParameter(ReportInstrMsgGenMQueue.PARAM_CNFTYPE, cnfType);
                                    else
                                        mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFTYPE].SetValue(cnfType);
                                }
                                else
                                    throw new Exception(StrFunc.AppendFormat("Notification Type  {0} is not defined", cnfType));

                                string cnfClass = row["CNFCLASS"].ToString();
                                Nullable<NotificationClassEnum> cnfClassEnum = ReflectionTools.ConvertStringToEnumOrNullable<NotificationClassEnum>(cnfClass);
                                if (cnfClassEnum.HasValue)
                                {
                                    if (null == mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFCLASS])
                                        mQueueAttributes.AddParameter(ReportInstrMsgGenMQueue.PARAM_CNFCLASS, cnfClass);
                                    else
                                        mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFCLASS].SetValue(cnfClass);

                                    if (NotificationClassEnum.MULTITRADES == cnfClassEnum)
                                    {
                                        MQueueparameter parameter = new MQueueparameter(ReportInstrMsgGenMQueue.PARAM_BOOK, TypeData.TypeDataEnum.integer);
                                        parameter.SetValue(Convert.ToInt32(row["IDB"]), row["BOOK_IDENTIFIER"].ToString());
                                        mQueueAttributes.AddParameter(parameter);
                                    }
                                }
                                else
                                    throw new Exception(StrFunc.AppendFormat("Notification Class {0} is not defined", cnfClassEnum.ToString()));

                                mQueue = new ReportInstrMsgGenMQueue(mQueueAttributes);
                                #endregion RIMGEN
                                break;
                            case Cst.ProcessTypeEnum.ESRNETGEN:
                                #region ESRNETGEN
                                identifier = row["NETMETHOD"].ToString();
                                idInfo = new IdInfo() {
                                    id = Convert.ToInt32(row["NETID"]),
                                    idInfos = new DictionaryEntry[] { new DictionaryEntry("nettingmethod", identifier) }
                                };
                                
                                
                                #endregion ESRNETGEN
                                break;
                            case Cst.ProcessTypeEnum.MSOGEN:
                                #region MSOGEN
                                idInfo = new IdInfo()
                                {
                                    id = Convert.ToInt32(row["IDSTLMESSAGE"])
                                };
                                identifier = row["IDENTIFIER"].ToString();
                                #endregion MSOGEN
                                break;
                            case Cst.ProcessTypeEnum.CMGEN:
                            case Cst.ProcessTypeEnum.RMGEN:
                                #region CMGEN / RMGEN
                                idInfo = new IdInfo() { id = Convert.ToInt32(row["IDMCO"]) };
                                if (row.Table.Columns.Contains("IDENTIFIER_INVOICE"))
                                    identifier = row["IDENTIFIER_INVOICE"].ToString();
                                #endregion CMGEN / RMGEN
                                break;
                            case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                            case Cst.ProcessTypeEnum.CASHBALANCE:
                                #region RISKPERFORMANCE / CASHBALANCE
                                idInfo = new IdInfo() { id = Convert.ToInt32(row["ENTITY_IDA"]) };
                                identifier = row["ENTITY_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                //PL 20120521 New feature ----------------------------------------------------
                                //RD 20120627 Code déplacé ici, pour avoir en premier le paramètre PARAM_DATE1
                                //ainsi dans la table PROCESS_L on aura le paramètre PARAM_DATE1 dans la colonne DATA1
                                //car dans la vue VW_MARGINTRACK, on s'attend à PARAM_DATE1 dans DATA1
                                //cette vue est utilisée par la consultation "Logs: Déposit" 
                                // PM 20150520 [20575] Remplacement de DTMARKET par DTENTITY
                                ////DateTime dtBusiness = Convert.ToDateTime(row["DTMARKET"]);
                                DateTime dtBusiness = Convert.ToDateTime(row["DTENTITY"]);
                                mQueueAttributes.AddParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS, dtBusiness);
                                //----------------------------------------------------------------------------
                                if (Cst.ProcessTypeEnum.RISKPERFORMANCE == currentProcessType)
                                {
                                    MQueueparameter parameter = new MQueueparameter(RiskPerformanceMQueue.PARAM_CSSCUSTODIAN, TypeData.TypeDataEnum.integer);
                                    parameter.SetValue(Convert.ToInt32(row["CSS_IDA"]), row["CSS_IDENTIFIER"].ToString());
                                    mQueueAttributes.AddParameter(parameter);
                                }
                                else
                                {
                                    string dtInfo = mQueueAttributes.parameters[RiskPerformanceMQueue.PARAM_DTBUSINESS].Value;
                                    DictionaryEntry info = new DictionaryEntry("DATA2", dtInfo);
                                    int index = taskInfo.trackerAttrib.info.FindIndex(match => match.Key.ToString() == "DATA2");
                                    if (-1 < index)
                                    {
                                        info = taskInfo.trackerAttrib.info[index];
                                        info.Value = dtInfo;
                                    }
                                    else
                                        taskInfo.trackerAttrib.info.Add(info);
                                }
                                mQueue = new RiskPerformanceMQueue(currentProcessType, mQueueAttributes);
                                #endregion RISKPERFORMANCE / CASHBALANCE
                                break;

                            case Cst.ProcessTypeEnum.CASHINTEREST:
                                idInfo = new IdInfo() { id = Convert.ToInt32(row["CBO_IDA"]) };
                                identifier = row["CBO_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_AMOUNTTYPE, row["AMOUNTTYPE"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_IDC, row["IDC"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_PERIOD, row["PERIOD"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_PERIODMLTP, Convert.ToInt32(row["PERIODMLTP"]));

                                mQueue = new CashBalanceInterestMQueue(mQueueAttributes);
                                break;

                            case Cst.ProcessTypeEnum.FEESCALCULATION: // FI 20180328 [23871] Add case FEESCALCULATION
                                identifier = row["IDENTIFIER"].ToString();
                                
                                idInfo = new IdInfo() {
                                    id = Convert.ToInt32(row["IDT"]),
                                    idInfos = new DictionaryEntry[]{new DictionaryEntry("IDDATA", Convert.ToInt32(row["IDT"])),
                                                                   new DictionaryEntry("IDDATAIDENT", "TRADE"),
                                                                   new DictionaryEntry("IDDATAIDENTIFIER", identifier),
                                                                   new DictionaryEntry("GPRODUCT", gProduct) }
                                };
                                                                
                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                FeesCalculationSettingsMode1 feesCalculationSetting = new FeesCalculationSettingsMode1
                                {
                                    actionDate = dtSys,
                                    noteSpecified = true,
                                    note = "Fees Calculation",
                                    mode = (Cst.FeesCalculationMode) Enum.Parse(typeof(Cst.FeesCalculationMode), 
                                    mQueueAttributes.parameters["FEESCALCULATIONMODE"].Value),

                                    feeSpecified = Convert.ToInt32(mQueueAttributes.parameters["FEE"].Value) > 0,
                                    feeSheduleSpecified = Convert.ToInt32(mQueueAttributes.parameters["FEESCHEDULE"].Value) > 0,
                                    feeMatrixSpecified = Convert.ToInt32(mQueueAttributes.parameters["FEEMATRIX"].Value) > 0,
                                    // FI 20180424 [23871] Alimentation de partyRole
                                    partyRoleSpecified = StrFunc.IsFilled(mQueueAttributes.parameters["PARTYROLE"].Value) && (mQueueAttributes.parameters["PARTYROLE"].Value != Cst.DDLVALUE_ALL)
                                };

                                if (feesCalculationSetting.feeSpecified)
                                {
                                    feesCalculationSetting.fee.otcmlId = mQueueAttributes.parameters["FEE"].Value;
                                    feesCalculationSetting.fee.identifier = mQueueAttributes.parameters["FEE"].ExValue;
                                }
                                if (feesCalculationSetting.feeSheduleSpecified)
                                {
                                    feesCalculationSetting.feeShedule.otcmlId = mQueueAttributes.parameters["FEESCHEDULE"].Value;
                                    feesCalculationSetting.feeShedule.identifier = mQueueAttributes.parameters["FEESCHEDULE"].ExValue;
                                }
                                if (feesCalculationSetting.feeMatrixSpecified)
                                {
                                    feesCalculationSetting.feeMatrix.otcmlId = mQueueAttributes.parameters["FEEMATRIX"].Value;
                                    feesCalculationSetting.feeMatrix.identifier = mQueueAttributes.parameters["FEEMATRIX"].ExValue;
                                }
                                if (feesCalculationSetting.partyRoleSpecified)
                                    feesCalculationSetting.partyRole = ReflectionTools.ConvertStringToEnum<FixML.v50SP1.Enum.PartyRoleEnum>(mQueueAttributes.parameters["PARTYROLE"].Value);

                                TradeActionMQueue tradeActionMQueue = new TradeActionMQueue
                                {
                                    tradeActionCode = TradeActionCode.TradeActionCodeEnum.FeesCalculation,
                                    actionMsgs = new FeesCalculationSettingsMode1[1] { feesCalculationSetting }
                                };

                                TradeActionGenMQueue tradeActionGenMQueue = new TradeActionGenMQueue(mQueueAttributes)
                                {
                                    item = new TradeActionMQueue[] { tradeActionMQueue }
                                };

                                mQueue = tradeActionGenMQueue;
                                mQueue.parameters = new MQueueparameters()
                                {
                                    parameter = mQueueAttributes.parameters.parameter
                                };
                                break;


                            default:
                                #region Autres services
                                identifier = row["IDENTIFIER"].ToString();
                                string idDataIdent = "TRADE";
                                if (Cst.ProductGProduct_ADM == gProduct)
                                    idDataIdent = "TRADEADMIN";
                                else if (Cst.ProductGProduct_ASSET == gProduct)
                                    idDataIdent = "DEBTSECURITY";
                                else if (Cst.ProductGProduct_RISK == gProduct)
                                    idDataIdent = "TRADERISK";

                                idInfo = new IdInfo()
                                {
                                    id = Convert.ToInt32(row["IDT"]),
                                    idInfos = new DictionaryEntry[]{new DictionaryEntry("IDDATA", Convert.ToInt32(row["IDT"])),
                                                                    new DictionaryEntry("IDDATAIDENT", idDataIdent),
                                                                    new DictionaryEntry("IDDATAIDENTIFIER", identifier),
                                                                    new DictionaryEntry("GPRODUCT", gProduct)}
                                };
                                if (1 == rowsCount)
                                {
                                    mQueueAttributes.id = idInfo.id;
                                    mQueueAttributes.identifier = identifier;
                                    // RD 20121119 / Bug génération des EARs / GPRODUCT manquant dans le message MQueue
                                    mQueueAttributes.idInfo = idInfo;
                                    List<DictionaryEntry> infoTracker = new List<DictionaryEntry>
                                    {
                                        new DictionaryEntry("IDDATA", idInfo.id),
                                        new DictionaryEntry("IDDATAIDENT", idDataIdent),
                                        new DictionaryEntry("IDDATAIDENTIFIER", identifier)
                                    };
                                    if (false == taskInfo.trackerAttrib.info.Exists(match => match.Key.ToString() == "DATA1"))
                                        infoTracker.Add(new DictionaryEntry("DATA1", identifier));
                                    taskInfo.trackerAttrib.info.AddRange(infoTracker);

                                    switch (currentProcessType)
                                    {
                                        case Cst.ProcessTypeEnum.ACCOUNTGEN:
                                            mQueue = new AccountGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EARGEN:
                                            mQueue = new EarGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EVENTSGEN:
                                            mQueue = new EventsGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EVENTSVAL:
                                            mQueue = new EventsValMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.MTMGEN:
                                            mQueue = new MarkToMarketGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.LINEARDEPGEN:
                                            mQueue = new LinearDepGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.ACCRUALSGEN:
                                            mQueue = new AccrualsGenMQueue(mQueueAttributes);
                                            break;
                                    }
                                }
                                #endregion Autres services
                                break;
                        }
                        if (lstInfoForFinalMessage.Count < 5)
                        {
                            Pair<int, string> display = null;
                            if (null != mQueue)
                            {
                                if (mQueue.identifierSpecified && mQueue.idSpecified)
                                {
                                    if (false == lstInfoForFinalMessage.Exists(match => match.First == mQueue.id))
                                        display = new Pair<int, string>(mQueue.id, mQueue.identifier);
                                }
                                else if ((mQueue.idInfoSpecified) && StrFunc.IsFilled(identifier))
                                {
                                    if ((false == lstInfoForFinalMessage.Exists(match => match.First == mQueue.idInfo.id)))
                                        display = new Pair<int, string>(mQueue.idInfo.id, identifier);
                                }
                            }
                            else if ((null != idInfo) && StrFunc.IsFilled(identifier))
                            {
                                if ((false == lstInfoForFinalMessage.Exists(match => match.First == idInfo.id)))
                                    display = new Pair<int, string>(idInfo.id, identifier);
                            }
                            if (null != display)
                            {
                                if (4 == lstInfoForFinalMessage.Count)
                                    display.Second += "...";
                                lstInfoForFinalMessage.Add(display);
                            }
                        }
                        #endregion Set idInfo
                        //
                        if (null != mQueue)
                            listMQueue.Add(mQueue);
                        else if (null != idInfo)
                            listIdInfo.Add(idInfo);
                    }
                    #endregion Boucle par ligne sélectionnée du Datagrid
                    #endregion PLUSIEURS MESSAGES POSTES
                }

                #region Postage du(des) message(s)
                if (ArrFunc.IsFilled(listMQueue))
                {
                    taskInfo.mQueue = listMQueue.ToArray();
                    taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.mQueue) == 1 ? taskInfo.mQueue[0].idInfo : null);
                }
                else if (ArrFunc.IsFilled(listIdInfo))
                {
                    taskInfo.idInfo = listIdInfo.ToArray();
                    if (ArrFunc.IsFilled(mQueueAttributes.parameters.parameter))
                        taskInfo.mQueueParameters = new MQueueparameters() { parameter = mQueueAttributes.parameters.parameter };
                    taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.idInfo) == 1 ? taskInfo.idInfo[0] : null);
                }

                MQueueTaskInfo.SendMultipleThreadPool(taskInfo, true);
                #endregion Postage du(des) message(s)
            }
            #endregion Boucle principale

            #region Message de réponse
            string msgReturn = "Msg_PROCESS_GENERATE_" + ((1 == rowsCount) || isMonoProcessGenerate ? "MONO" : "MULTI");
            int nbSend = 0;
            if (null != taskInfo.idInfo)
                nbSend = ArrFunc.Count(taskInfo.idInfo);
            else if (null != taskInfo.mQueue)
                nbSend = ArrFunc.Count(taskInfo.mQueue);

            if (0 < lstInfoForFinalMessage.Count)
                lstInfoForFinalMessage.ForEach(display => { msgDisplay += display.Second + ", "; });

            MsgForAlertImmediate = Ressource.GetString2(msgReturn, nbSend.ToString(), msgDisplay);
            #endregion Message de réponse
        }


        /// <summary>
        /// Génération des messages pour les services Spheres®
        /// <para>Poste des messages pour les enregistrements sélectionnés</para>
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pEntityName"></param>
        /// <param name="dt"></param>
        /// FI 20190219 [XXXXX] New 
        private void CallProcessService2(Cst.ProcessTypeEnum pProcessName, int pEntityId, string pEntityName, DataTable pDt, Nullable<Cst.ProcessIOType> pProcessIoType)
        {
            // FI 20190219 [XXXXX] Fonction CallProcessServiceCashBalance spécifique pour ne rien casser dans la méthode classique CallProcessService
            switch (pProcessName)
            {
                case Cst.ProcessTypeEnum.CASHBALANCE:
                    CallProcessServiceCashBalance(pEntityId, pDt);
                    break;
                default:
                    CallProcessService(pProcessName, pEntityId, pEntityName, pDt, pProcessIoType);
                    break;
            }
        }

        /// <summary>
        /// Génération des messages pour les services Spheres®
        /// <para>Poste des messages pour les enregistrements sélectionnés</para>
        /// </summary>
        /// FI 20190219 [XXXXX] Une demande tracker pour chaque demande
        private void CallProcessServiceCashBalance(int pEntityId, DataTable pDt)
        {
            Cst.ProcessTypeEnum currentProcessType = Cst.ProcessTypeEnum.CASHBALANCE;

            //PL 20221025 New signature of CheckMOMSettings() with 3 parameters
            CheckMOMSettings(SessionTools.CS, currentProcessType, pEntityId);

            string msgDisplay = string.Empty;
            List<Pair<int, string>> lstInfoForFinalMessage = new List<Pair<int, string>>();
            MQueueparameters listCommonCustomParameter = new MQueueparameters() { parameter = GetCustomObjectParameter2() };

            foreach (DataRow row in pDt.Rows)
            {
                DateTime dtBusiness = Convert.ToDateTime(row["DTENTITY"]);

                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString= SessionTools.CS,
                    parameters = listCommonCustomParameter.Clone()
                };
                mQueueAttributes.AddParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS, dtBusiness);

                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = SessionTools.CS,
                    Session = SessionTools.AppSession,
                    process = currentProcessType,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = currentProcessType,
                        caller = efsdtgRefeferentiel.IDMenu,
                        info = TrackerAttributes.BuildInfo(currentProcessType, mQueueAttributes.parameters),
                    },
                };
                
                IdInfo idInfo = new IdInfo() { id = Convert.ToInt32(row["ENTITY_IDA"]) };
                
                mQueueAttributes.id = idInfo.id;
                mQueueAttributes.identifier = row["ENTITY_IDENTIFIER"].ToString();
                mQueueAttributes.idInfo = idInfo;

                MQueueBase mQueue = new RiskPerformanceMQueue(currentProcessType, mQueueAttributes);
                taskInfo.mQueue = new MQueueBase[1] { mQueue };

                taskInfo.SetTrackerAckWebSessionSchedule(mQueue.idInfo);
                


                if (lstInfoForFinalMessage.Count < 5)
                {
                    Pair<int, string> display = null;
                    if (mQueue.identifierSpecified && mQueue.idSpecified)
                    {
                        if (false == lstInfoForFinalMessage.Exists(match => match.First == mQueue.id))
                            display = new Pair<int, string>(mQueue.id, mQueue.identifier);
                    }
                    if (null != display)
                    {
                        if (4 == lstInfoForFinalMessage.Count)
                            display.Second += "...";
                        lstInfoForFinalMessage.Add(display);
                    }
                }

                var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo);
                if (!isOk)
                    throw new SpheresException2("MQueueTaskInfo.SendMultiple", errMsg);
            }

            string msgReturn = "Msg_PROCESS_GENERATE_" + ((1 == pDt.Rows.Count) ? "MONO" : "MULTI");

            if (0 < lstInfoForFinalMessage.Count)
                lstInfoForFinalMessage.ForEach(x => { msgDisplay += x.Second + ", "; });

            MsgForAlertImmediate = Ressource.GetString2(msgReturn, pDt.Rows.Count.ToString(), msgDisplay);

        }

        /// <summary>
        /// Retourne true lorsque le grid affiche les enregistrements dans le grid sans offire la possibilité de choisir certains enregistrement via les checks
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        private static Boolean IsMonoProcessGenerate(Cst.ProcessTypeEnum pProcessName)
        {
            Boolean ret = false;

            switch (pProcessName)
            {
                case Cst.ProcessTypeEnum.INVOICINGGEN:
                    // FI 20170306 [22225] FEESCALCULATION est désormais un traitement unitaire (un Message Queue par trade)
                    //case Cst.ProcessTypeEnum.FEESCALCULATION:
                    ret = true;
                    break;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        private void CallProcessStoredProcedure(string pProcessName)
        {
            string msgReturn = string.Empty;
            try
            {
                Cst.ErrLevel ret = SQLUP.RunUP(out int returnValue, SessionTools.CS, pProcessName,
                efsdtgRefeferentiel.LocalLstConsult.template.IDLSTCONSULT,
                efsdtgRefeferentiel.LocalLstConsult.template.IDLSTTEMPLATE,
                efsdtgRefeferentiel.LocalLstConsult.template.IDA);
                if (Cst.ErrLevel.SUCCESS == ret)
                    msgReturn = Ressource.GetString("Msg_ProcessSuccessfull");
                else
                    msgReturn = Ressource.GetString("Msg_ProcessIncomplete");
            }
            catch { msgReturn = Ressource.GetString("Msg_ProcessUndone"); }
            finally
            {
                MsgForAlertImmediate = msgReturn;
            }
        }

        /// <summary>
        /// Charge _customObjects à partir du fichier desriptif xxxxxxxxxx.GUI.xml
        /// </summary>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify       
        private void LoadCustomObjects()
        {
            // FI Attention cette affectation est très importante et doit être conservée
            // => Elle alimente le DataCache associé aux CustomObjects (voir la méthode CheckSessionState pour aller plus loin )
            CustomObjects = null;

            List<string> objectNameAvailable =
                ReferentialTools.GetObjectNameForDeserialize(efsdtgRefeferentiel.IDMenu, efsdtgRefeferentiel.ObjectName);

            Cst.ListType? objectType;
            if (efsdtgRefeferentiel.IsConsultation)
            {
                //FI 20120215 Ds le cas des consultation Le fichier GUI est  présent sous le répertoire  consultation
                objectType = Cst.ListType.Consultation;
            }
            else
            {
                objectType = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), efsdtgRefeferentiel.Title);
            }

            if (null == objectType)
                throw new NullReferenceException("objectType is null");

            bool isFind = false;
            string xmlFile = string.Empty;

            // FI 20121002 [18161] usage de la méthode ReferentialTools.GetObjectXMLFile en priorité
            // Cela permet de récupérer les fichiers éventuellement présents dans FILECONFIG
            for (int i = 0; i < ArrFunc.Count(objectNameAvailable); i++)
            {
                xmlFile = ReferentialTools.GetObjectXMLFile(objectType.Value, objectNameAvailable[i]);
                if (StrFunc.IsFilled(xmlFile))
                {
                    xmlFile = Path.ChangeExtension(xmlFile, null) + ".GUI.xml";
                    //isFind = File.Exists(xmlFile); //PL 20170131 Set comment and use below SearchFile()
                }
                else //PL 20170131 New - (utile aux consultations LST pour lesquelles il n'existe pas de fichier XML physique) 
                {
                    xmlFile = StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.GUI.xml", objectType.Value.ToString(), objectNameAvailable[i]);
                }
                isFind = SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, xmlFile, ref xmlFile);

                if (isFind)
                    break;
            }

            //FI 20160804 [Migration TFS] Suppression du pavé suivant (Il n'existe plus de folder spécifique à un sotfware
            /*
             if (false == isFind)
            {
                // FI 20121002 [18161]  je conserve cette boucle pour ne pas dégrader
                // en théorie cela ne sert à rien puisque ReferentialTools.GetObjectXMLFile doit nécessairement retourné le XML lorsq'uil existe 
                for (int i = 0; i < ArrFunc.Count(objectNameAvailable); i++)
                {
                    List<string> softwareFolder = FileTools.SoftwareFolder();
                    softwareFolder.Add(string.Empty);
                    for (int j = 0; j < ArrFunc.Count(softwareFolder); j++)
                    {
                        string software = softwareFolder[j];
                        // FI 20160804 [Migration TFS] Modify
                        //string folder = SessionTools.NewAppInstance().MapPath(software) + @"\" + "XML_Files" + @"\" + objectType;
                        string folder = SessionTools.NewAppInstance().MapPath(string.Empty) + @"\" + "PDIML" + @"\" + objectType;
                        xmlFile = folder + @"\" + objectNameAvailable[i] + ".GUI.xml";
                        isFind = File.Exists(xmlFile);
                        if (isFind)
                            break;
                    }
                    if (isFind)
                        break;
                }
            }
            */

            // EG 20151019 [21465] New le fichier GUI associé au fichier XML est spécifié dans l'URL (GUIName)
            if ((false == isFind) && (1 == objectNameAvailable.Count) && StrFunc.IsFilled(efsdtgRefeferentiel.GUIName))
            {
                xmlFile = ReferentialTools.GetObjectXMLFile(objectType.Value, efsdtgRefeferentiel.GUIName + ".GUI");
                isFind = StrFunc.IsFilled(xmlFile) && File.Exists(xmlFile);
            }

            if (isFind)
            {
                // FI 20200602 [25370] using
                using (StreamReader streamReader = new StreamReader(xmlFile))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomObjects));
                    CustomObjects = (CustomObjects)xmlSerializer.Deserialize(streamReader);
                }

                //PL 20180903 Pour gérer le cas d'un fichier GUI où tous les éléments ont été mis en commentaire.
                if (CustomObjects.customObject == null)
                    CustomObjects = null;
            }

            // FI 20200602 [25370] Call
            SetCustomObjectLinked();

            // FI 20200602 [25370] Call
            CheckCustomObject();
        }

        /// <summary>
        /// Retourne true si La page affiche le boutton Exportation SQL
        /// </summary>
        /// <returns></returns>
        private bool IsDisplaySQLInsertCommand()
        {
            bool ret = false;

            ret = (SessionTools.Collaborator_IDENTIFIER.ToUpper() == "SYSADM" || SessionTools.Collaborator_IDENTIFIER.ToUpper() == "EFS");
            if ((!ret))
            {
                //PL 20170214 Newness
                ret = SessionTools.Collaborator.IsActorSysAdmin
                      && (bool)SystemSettings.GetAppSettings("SQLExportAllowed", typeof(System.Boolean), false);
            }
#if DEBUG
            ret = true;
#endif
            ret &= ((string.Empty + Request.QueryString[Cst.ListType.Repository.ToString()]).ToString().Length > 0);

            return ret;
        }

        /// <summary>
        /// Export Excel du datagrid ou du jeu de  donnée
        /// </summary>
        /// FI 20130417 [18596] add Method ExportExcel
        /// FI 20160308 [21782] Modify
        // EG 20190411 [ExportFromCSV] Parameter to close BlockUI
        private void ExportExcel(ExportExcelType pExporType)
        {
            if (pExporType == ExportExcelType.ExportGrid)
            {
                // FI 20190318 [24568][24588] S'il existe des lignes d'aggegation => Spheres® les affiche en évidence via l'usage d'une feuille de style css
                // Pour info tous les lignes, cellules du grid ne contiennent pas de CSS
                string cssStyleDeclaration = string.Empty;
                if (efsdtgRefeferentiel.Referential.HasAggregateColumns)
                {
                    cssStyleDeclaration = @"
<style>
tr.DataGrid_GroupStyle td
{
	font-weight: bold;
	background-color: #E0E0E0;
}
tr.DataGrid_GroupStyle1 td
{
	font-weight: bold;
	background-color: #E5E5E5;
}
tr.DataGrid_GroupStyle2 td
{
	font-weight: bold;
	background-color: #EAEAEA;
}
</style>
";
                }

                ExportGridToExcel(efsdtgRefeferentiel, cssStyleDeclaration, efsdtgRefeferentiel.ObjectName, ExportTokenValue);
            }
            else if (pExporType == ExportExcelType.ExportDataset)
            {
                //FI 20120212 [] add filename in URL 
                // FI 20200518 [XXXXX] use DataCacheKeyDsData
                string url = StrFunc.AppendFormat("~/MSExcel.aspx?dvMSExcel={0}&filename={1}",
                        efsdtgRefeferentiel.DataCacheKeyDsData, efsdtgRefeferentiel.ObjectName);
                Page.Response.Redirect(url, false);
            }
        }

        /// <summary>
        /// Demande d'annulation d'une allocation Spheres® 
        ///  <para>- annule le trade (le trade est reserved en sortie)</para>
        ///  <para>- génère une entrée dans POSREQUEST</para>
        /// </summary>
        /// <param name="pTrade">Repésente le trade (IDT et IDENTIFIER)</param>
        /// <param name="pPosRequest">Repésente la demande d'annulation générée</param>
        /// <param name="pQueue">Repésente le message queue pour traiter la demande d'annulation par les services</param>
        /// FI 20190903 [24872] Add Method
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private static ProcessRemoveAllocReturn ProcessRemoveTradeAlloc2(string pCS, AppSession session, Pair<int, string> pTrade,
            out IPosRequest pPosRequest, out PosKeepingMQueue pQueue)
        {
            ProcessRemoveAllocReturn ret = ProcessRemoveAllocReturn.Ok;

            int idT = pTrade.First;
            string identifier = pTrade.Second;


            pPosRequest = null;
            pQueue = null;

            LockObject lockTrade = null;
            try
            {
                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    lockTrade = new LockObject(TypeLockEnum.TRADE, idT, identifier, LockTools.Exclusive);
                    Lock lck = new Lock(pCS, null, lockTrade, session, Cst.Capture.GetLabel(Cst.Capture.ModeEnum.RemoveOnly));
                    bool isLockSuccessful = LockTools.LockMode1(lck, out Lock lckExisting);
                    if (false == isLockSuccessful)
                        ret = ProcessRemoveAllocReturn.Locked;
                }

                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    bool IsTradeRemoved = TradeRDBMSTools.IsTradeRemove(pCS, null, idT);
                    if (IsTradeRemoved)
                        ret = ProcessRemoveAllocReturn.AlreadyRemoved;
                }

                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    bool IsEventsGenerated = TradeRDBMSTools.IsEventExist(pCS, null, idT, EventCodeFunc.Trade);
                    if (false == IsEventsGenerated)
                        ret = ProcessRemoveAllocReturn.NoEvent;
                }

                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    SQL_TradeStSys sqlTradeStSys = new SQL_TradeStSys(pCS, idT);
                    if (sqlTradeStSys.IdStUsedBy != Cst.STATUSREGULAR)
                        ret = ProcessRemoveAllocReturn.AlreadyReserved;
                }

                if (ret == ProcessRemoveAllocReturn.Ok)
                {

                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.IDT), idT);
                    string query = @"select IDT, QTY, IDI, GPRODUCT, IDEM, IDM, DTBUSINESS, IDA_ENTITY, IDA_CSSCUSTODIAN, ISCUSTODIAN, ISCSS
from dbo.VW_TRADE_ALLOC t
where IDT = @IDT";

                    QueryParameters qryParameters = new QueryParameters(pCS, query, dp);
                    DataTable dt = DataHelper.ExecuteDataTable(pCS, qryParameters.Query, dp.GetArrayDbParameter());
                    if (dt.Rows.Count == 0)
                        throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not loaded", idT));

                    DataRow row = dt.Rows[0];

                    int idI = Convert.ToInt32(row["IDI"]);
                    int idAEntity = Convert.ToInt32(row["IDA_ENTITY"]);
                    int idEM = Convert.ToInt32(row["IDEM"]);
                    int idM = Convert.ToInt32(row["IDM"]);
                    DateTime dtBusiness = Convert.ToDateTime(row["DTBUSINESS"]);
                    Decimal qty = Convert.ToDecimal(row["QTY"]);

                    Nullable<int> idACSS = null;
                    if (Convert.ToBoolean(row["ISCSS"]))
                        idACSS = Convert.ToInt32(row["IDA_CSSCUSTODIAN"]);

                    Nullable<int> idACustodian = null;
                    if (Convert.ToBoolean(row["ISCUSTODIAN"]))
                        idACustodian = Convert.ToInt32(row["IDA_CSSCUSTODIAN"]);

                    string gProduct = row["GPRODUCT"].ToString();

                    DateTime currentDtBusiness = OTCmlHelper.GetDateBusiness(CSTools.SetCacheOn(pCS), null, idAEntity, idM, idACustodian);


                    if (ret == ProcessRemoveAllocReturn.Ok)
                    {
                        if (dtBusiness < currentDtBusiness)
                            ret = ProcessRemoveAllocReturn.UnvalidDateBusiness;
                    }

                    if (ret == ProcessRemoveAllocReturn.Ok)
                    {
                        Decimal availableQuantity = PosKeepingTools.GetAvailableQuantity(pCS, null, dtBusiness, idT);

                        pPosRequest = (IPosRequestRemoveAlloc)PosKeepingTools.CreatePosRequestRemoveAlloc(
                        pCS,
                        Tools.GetNewProductBase(),
                        idT,
                        identifier,
                        idAEntity, idACSS, idACustodian, idEM,
                        dtBusiness,
                        qty,
                        availableQuantity,
                        string.Empty,
                        (ProductTools.GroupProductEnum)ReflectionTools.EnumParse(new ProductTools.GroupProductEnum(), gProduct));

                        IDbTransaction dbTransaction = null;
                        try
                        {
                            dbTransaction = DataHelper.BeginTran(pCS);

                            PosKeepingTools.FillPosRequest(pCS, dbTransaction, pPosRequest, session);

                            #region  Update StatusStUsedBy
                            SQL_TradeCommon sqlTradeStSys = new SQL_TradeCommon(pCS, idT);
                            string SQLQuery = sqlTradeStSys.GetQueryParameters(
                                        new string[] { "TRADE.IDT", "IDSTUSEDBY", "DTSTUSEDBY", "IDASTUSEDBY", "LIBSTUSEDBY" }).QueryReplaceParameters;

                            DataSet ds = DataHelper.ExecuteDataset(pCS, CommandType.Text, SQLQuery);
                            if (dt.Rows.Count == 0)
                                throw new Exception(StrFunc.AppendFormat("TRADE STATUS (id:{0}) is not loaded", idT));

                            DataRow dr = ds.Tables[0].Rows[0];

                            dr.BeginEdit();
                            dr["IDSTUSEDBY"] = Cst.StatusUsedBy.RESERVED.ToString();
                            dr["LIBSTUSEDBY"] = Cst.ProcessTypeEnum.POSKEEPREQUEST.ToString();
                            // FI 20200820 [25468] Dates systèmes en UTC
                            dr["DTSTUSEDBY"] = OTCmlHelper.GetDateSysUTC(pCS);
                            dr["IDASTUSEDBY"] = session.IdA;
                            dr.EndEdit();
                            #endregion Update StatusStUsedBy

                            DataHelper.ExecuteDataAdapter(dbTransaction, SQLQuery, dt);

                            pQueue = PosKeepingTools.BuildPosKeepingRequestMQueue(pCS, pPosRequest, null);

                            DataHelper.CommitTran(dbTransaction);
                        }
                        catch
                        {
                            if (null != dbTransaction)
                                DataHelper.RollbackTran(dbTransaction);
                            throw;
                        }
                        finally
                        {
                            if (null != dbTransaction)
                                dbTransaction.Dispose();
                        }
                    }
                }
            }
            finally
            {
                if (null != lockTrade)
                    LockTools.UnLock(pCS, lockTrade, session.SessionId);
            }
            return ret;
        }


        /// <summary>
        ///  Passe en modification sur l'ensemble des trades 
        ///  <para>Méthode qui peut être utilisé pour réinitailiser tous les évènements des trades d'une journée</para>
        ///  <para>=> Doit être utilisé par EFS uniquement (fonctionnalité cachée) </para>
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private Cst.ErrLevelMessage ProcessUpdateTrade(DataTable pDataTable)
        {

            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            if (pDataTable.Rows.Count > 0)
            {
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                
                foreach (DataRow dr in pDataTable.Rows)
                {
                    UpdateTrade(dr, out MQueueBase queue);
                    listMQueue.Add(queue);
                }

                //Envoi des messages Mqueue générés
                if (ArrFunc.IsFilled(listMQueue))
                {
                    MQueueTaskInfo taskInfo = new MQueueTaskInfo
                    {
                        connectionString = SessionTools.CS,
                        process = Cst.ProcessTypeEnum.TRADECAPTURE,
                        Session = SessionTools.AppSession,
                        mQueue = listMQueue.ToArray(),
                        trackerAttrib = new TrackerAttributes()
                        {
                            process = Cst.ProcessTypeEnum.TRADECAPTURE,
                            caller = "UpdateTrade"
                        }
                    };

                    var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo);
                    if (!isOk)
                        throw new SpheresException2("MQueueTaskInfo.SendMultiple", errMsg);
                }
            }
            return ret;
        }

        /// <summary>
        /// Passe en modification sur un trade
        /// </summary>
        /// <param name="pRow"></param>
        /// FI 20170616 [XXXXX] Add Method
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        /// EG 20201005 [XXXXX] Ajout test création message pour le service de génération des événeements
        private void UpdateTrade(DataRow pRow, out MQueueBase pMqueue)
        {

            int indexDataKeyField = efsdtgRefeferentiel.Referential.IndexColSQL_DataKeyField;
            if (false == (indexDataKeyField > -1))
                throw new Exception("Consultation without DataKeyField");

            string columnDataKeyField = efsdtgRefeferentiel.Referential.Column[indexDataKeyField].DataField;
            if (false == pRow.Table.Columns.Contains(columnDataKeyField))
                throw new Exception(StrFunc.AppendFormat("Consultation without column {0}", columnDataKeyField));

            int idT = Convert.ToInt32(pRow[columnDataKeyField]);
            if (false == (idT > 0))
                throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not valid", idT));

            TradeCaptureGen tradeCaptureGen = new TradeCaptureGen();
            bool isLoad = tradeCaptureGen.Load(SessionTools.CS, null, idT, Cst.Capture.ModeEnum.Update, SessionTools.User, SessionTools.SessionID, true);
            if (false == isLoad)
                throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not loaded", idT));

            TradeInput tradeInput = tradeCaptureGen.Input;

            CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
            {
                user = SessionTools.User,
                session = SessionTools.AppSession,
                licence = SessionTools.License
            };

            TradeRecordSettings recordSettings = new TradeRecordSettings
            {
                displayName = tradeInput.Identification.Displayname,
                description = tradeInput.Identification.Description,
                extLink = tradeInput.Identification.Extllink,
                idScreen = tradeInput.SQLLastTradeLog.ScreenName,
                isCheckValidationRules = false,
                isCheckValidationXSD = false,
                isCheckLicense = false,
                isCheckActionTuning = true
            };

            using (IDbTransaction dbTransaction = DataHelper.BeginTran(SessionTools.CS))
            {
                try
                {
                    string identifier = tradeInput.Identifier;
                    // FI 20170404 [23039] Utilisation de underlying et trader
                    tradeCaptureGen.CheckAndRecord(SessionTools.CS, dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), Cst.Capture.ModeEnum.Update,
                        captureSessionInfo, recordSettings,
                        ref identifier, ref idT, out Pair<int, string>[] underlying, out Pair<int, string>[] trader);
                    
                    DataHelper.CommitTran(dbTransaction);

                }
                catch
                {
                    if (DataHelper.IsTransactionValid(dbTransaction))
                        DataHelper.RollbackTran(dbTransaction);
                    throw;
                }
            }

            pMqueue = null;
            if (tradeInput.IsInstrumentEvents())
            {
                IdInfo idInfo = new IdInfo()
                {
                    id = tradeInput.IdT,
                    idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", tradeInput.SQLProduct.GProduct) }
                };

                MQueueAttributes mQueueAttributes = new MQueueAttributes
                {
                    connectionString = SessionTools.CS,
                    idInfo = idInfo,
                    id = tradeInput.IdT,
                    identifier = tradeInput.Identifier
                };

                MQueueBase mQueue = new EventsGenMQueue(mQueueAttributes);
                mQueue.parameters[EventsGenMQueue.PARAM_DELEVENTS].SetValue(true);

                pMqueue = mQueue;
            }
        }

        /// <summary>
        ///  Pour chaque trade présent dans le jeu de résultat, Spheres® 
        ///  <para>- annule le trade</para>
        ///  <para>- génère une entrée dans POSREQUEST</para>
        ///  <para>- Envoi un message au service pour traiter le POSREQUEST d'annulation</para>
        /// </summary>
        /// <param name="pDt">Représente les enregistrements sélectionnés</param>
        /// FI 20130423 [18601] Add Method 
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        private Cst.ErrLevelMessage AddPosRequestRemoveAlloc(DataTable pDataTable)
        {
            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            // FI 20190903 [24872] recherche de columnDataKeyField placé ici pour etre appel qu'une 1 seule fois
            int indexDataKeyField = efsdtgRefeferentiel.Referential.IndexColSQL_DataKeyField;
            if (false == (indexDataKeyField > -1))
                throw new Exception("Consultation without DataKeyField");

            string columnDataKeyField = efsdtgRefeferentiel.Referential.Column[indexDataKeyField].DataField;
            if (false == pDataTable.Columns.Contains(columnDataKeyField))
                throw new Exception(StrFunc.AppendFormat("Consultation without column {0}", columnDataKeyField));


            if (pDataTable.Rows.Count > 0)
            {
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                string succesMessage = string.Empty;
                string errorMessage = string.Empty;

                foreach (DataRow dr in pDataTable.Rows)
                {
                    int idT = Convert.ToInt32(dr[columnDataKeyField]);
                    if (false == idT > 0)
                        throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not valid", idT));

                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(SessionTools.CS, idT);
                    if (false == sqlTrade.LoadTable(new string[] { "TRADE.IDT,IDENTIFIER" }))
                        throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not loaded", idT));

                    string identifier = sqlTrade.Identifier;
                    ProcessRemoveAllocReturn result = ProcessRemoveTradeAlloc2(SessionTools.CS, SessionTools.AppSession,
                                new Pair<int, string>(idT, identifier), out IPosRequest posRequest, out PosKeepingMQueue mQueue);

                    if (result == ProcessRemoveAllocReturn.Ok)
                    {
                        StrFunc.BuildStringListElement(ref succesMessage, identifier, 4);
                        listMQueue.Add(mQueue);
                    }
                    else
                    {
                        string err = string.Empty;
                        switch (result)
                        {
                            case ProcessRemoveAllocReturn.AlreadyRemoved:
                            case ProcessRemoveAllocReturn.AlreadyReserved:
                            case ProcessRemoveAllocReturn.NoEvent:
                            case ProcessRemoveAllocReturn.UnvalidDateBusiness:
                            case ProcessRemoveAllocReturn.Locked:
                                err = StrFunc.AppendFormat("{0} ({1})", identifier, Ressource.GetString("lbl" + result.ToString()));
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", result.ToString()));
                        }
                        StrFunc.BuildStringListElement(ref errorMessage, err, -1);
                    }
                }

                //Envoi des messages Mqueue générés
                if (ArrFunc.IsFilled(listMQueue))
                {
                    MQueueTaskInfo taskInfo = new MQueueTaskInfo
                    {
                        process = Cst.ProcessTypeEnum.POSKEEPREQUEST,
                        connectionString = SessionTools.CS,
                        Session = SessionTools.AppSession,
                        trackerAttrib = new TrackerAttributes
                        {
                            process = Cst.ProcessTypeEnum.POSKEEPREQUEST,
                            gProduct = Cst.ProductGProduct_FUT,
                            caller = efsdtgRefeferentiel.IDMenu
                        },
                        mQueue = listMQueue.ToArray()
                    };
                    // FI 20231012 [26536] Appel à taskInfo.SetTrackerAckWebSessionSchedule
                    taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.mQueue) == 1 ? taskInfo.mQueue[0].idInfo : null);

                    MQueueTaskInfo.SendMultipleThreadPool(taskInfo, true);
                }

                int nbMessageSendToService = ArrFunc.Count(listMQueue);
                int nbMessageNotSendToService = pDataTable.Rows.Count - ArrFunc.Count(listMQueue);
                if (nbMessageSendToService > 0 && nbMessageNotSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.SUCCESS;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_GENERATE_NOGENERATE_MULTI", nbMessageSendToService.ToString(), succesMessage, nbMessageNotSendToService.ToString(), errorMessage);
                }
                else if (nbMessageSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.SUCCESS;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_GENERATE_DATA", nbMessageSendToService.ToString(), succesMessage);
                }
                else if (nbMessageNotSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.FAILURE;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_NOGENERATE_DATA", nbMessageNotSendToService.ToString(), errorMessage);
                }
            }
            else
            {
                ret.ErrLevel = Cst.ErrLevel.FAILURE;
                ret.Message = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
            }

            return ret;
        }

        /// <summary>
        /// Retourne la classe, la méthode et les arguments qu'il est nécessaire d'invoquer pour traiter le process {processName}
        /// </summary>
        /// <param name="processName">Nom du process</param>
        /// <param name="entityName"></param>
        /// <param name="dt">Jeu de résultat (données cochées sur le grid)</param>
        /// <param name="tClassProcess">Représente la class</param>
        /// <param name="methodName">Représente la méthode</param>
        /// <param name="args">Représente les arguments à passer à la méthode</param>
        /// <returns></returns>
        /// FI 20130423 [18601] add Method
        /// EG 20151019 [21465] Passage du nom de la méthode C# seule (pour alleger URL) => (Par defaut : EfsML.Business.PosKeepingTools est utilisée comme classe)
        private bool IsInvokeMember(string processName, string entityName, string pAdditionalArguments, DataTable dt,
                            out Type tClassProcess, out string methodName, out Object[] args)
        {
            bool isInvoke = false;

            tClassProcess = null;
            methodName = processName;
            args = null;

            if (Software.IsSoftwareVision())
            {
                isInvoke = true;
                tClassProcess = typeof(Vision);
                args = new Object[] {this,
                                         efsdtgRefeferentiel.Referential.TableName,
                                         efsdtgRefeferentiel.LocalLstConsult.template.IDA,
                                         efsdtgRefeferentiel.LocalLstConsult.template.IDLSTTEMPLATE,
                                         efsdtgRefeferentiel.LocalLstConsult.template.IDLSTCONSULT.Trim()};
            }
            else if (Software.IsSoftwarePortal())
            {
                isInvoke = true;
                tClassProcess = typeof(Portal);
            }
            else if (Software.IsSoftwareSpheres())
            {
                // EG 20151019 [21465] New
                string[] methodInfo = processName.Split('|');
                string className;
                string assemblyName;
                if (1 < methodInfo.Length)
                {
                    className = methodInfo[0];
                    if (2 == methodInfo.Length)
                    {
                        assemblyName = methodInfo[0].Substring(0, methodInfo[0].LastIndexOf("."));
                        methodName = methodInfo[1];
                    }
                    else
                    {
                        assemblyName = methodInfo[1];
                        methodName = methodInfo[2];
                    }
                    isInvoke = true;
                    tClassProcess = Type.GetType(className + "," + assemblyName, true, false);
                }
                else if (1 == methodInfo.Length)
                {
                    methodName = methodInfo[0];
                    isInvoke = true;
                    tClassProcess = typeof(EfsML.Business.PosKeepingTools);
                }

                if (isInvoke)
                {
                    MQueueparameters parameters = new MQueueparameters() { parameter = GetCustomObjectParameter2() };
                    if (StrFunc.IsFilled(pAdditionalArguments))
                        args = new Object[] { SessionTools.CS, dt, parameters, entityName, pAdditionalArguments };
                    else
                        args = new Object[] { SessionTools.CS, dt, parameters, entityName };
                }
            }

            return isInvoke;
        }

        /// <summary>
        /// Calcul des UTI sur des trades qui en sont dépourvus.
        /// <para>(avec calcul du PUTI dans la foulée)</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataTable">Contient les enregistrements candidats</param>
        /// <returns></returns>
        private static Cst.ErrLevelMessage CalcUTI(string pCS, DataTable pDataTable)
        {
            return UTITools.CalcAndRecordUTI(pCS, null, UTIType.UTI, pDataTable, SessionTools.AppSession.IdA);
        }

        /// <summary>
        /// Calcul des PUTI sur des trades qui en sont dépourvus.
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pDataTable">Contient les enregistrements candidats</param>
        /// <returns></returns>
        private static Cst.ErrLevelMessage CalcPositionUTI(string pCS, DataTable pDataTable)
        {
            return UTITools.CalcAndRecordUTI(pCS, null, UTIType.PUTI, pDataTable, SessionTools.AppSession.IdA);
        }



        /// <summary>
        /// Retourne le type d'export demandé par l'utilisateur 
        /// <para>Fonction appelée lors du log des actions utilisateur (RequestTrack)</para>
        /// </summary>
        /// <param name="pReportName">Retourne le nom du report lorsque l'export est de type PDF</param>
        /// <returns></returns>
        /// FI 20140519 [19923] Add method
        /// FI 20160804 [Migration TFS] Modify
        // EG 20190411 [ExportFromCSV]
        private RequestTrackExportType GetRequestTrackExportType(out string pReportName)
        {
            RequestTrackExportType ret = default;
            pReportName = string.Empty;


            if (null != m_toolbar_Clicked)
            {
                if (m_toolbar_Clicked is Control control)
                {
                    switch (control.ID)
                    {
                        case "imgSQL":
                            ret = RequestTrackExportType.SQL;
                            break;
                        case "imgZIP":
                            ret = RequestTrackExportType.ZIP;
                            break;
                        case "imgCSV":
                            ret = RequestTrackExportType.CSV;
                            break;
                        case "imgMSExcel":
                            ret = RequestTrackExportType.XLS;
                            break;
                    }
                }
                else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs mnu)
                {
                    if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                    {
                        // FI 20160804 [Migration TFS] New folder
                        switch (mnu.Argument)
                        {
                            case @"~\GUIOutput\List\FinFlows-pdf.xsl":
                                pReportName = "CashFlows";
                                break;
                            case @"~\GUIOutput\List\FinFlowsByActor-pdf.xsl":
                                pReportName = "CashFlowsByActor";
                                break;
                            case @"~\GUIOutput\List\FinStatMember-pdf.xsl":
                                pReportName = "FinStatMember";
                                break;
                            default:
                                pReportName = null;
                                break;
                        }
                    }
                }

            }
            return ret;

        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si Chargement du grid
        /// </summary>
        /// FI 20141021 [20350] Add
        // EG 20190411 [ExportFromCSV]
        private void SetRequestTrackBuilderListLoad()
        {
            Boolean isTrack = SessionTools.IsRequestTrackConsultEnabled && efsdtgRefeferentiel.Referential.RequestTrackSpecified;

            if (isTrack)
            {
                Boolean isAuto = false;
                if (false == IsPostBack)
                {
                    //Alimentation du log si chgt du grid à l'ouverture ou si auto rafraichissement 
                    isTrack = (efsdtgRefeferentiel.IsLoadDataOnStart || AutoRefresh > 0);

                    // Mode auto
                    isAuto = true; //default
                    if (null != Request.UrlReferrer)
                    {
                        // L'ouverture d'un référentiel enfant est considérée manuelle
                        // (exemple consultation des positions détaillées à partir des positions synthéthique)
                        //  test sur Request.UrlReferrer de manière à considérer le cas ou l'utilisateur saisie une URL avec FK
                        if ((Request.UrlReferrer.LocalPath == Request.Url.LocalPath) && StrFunc.IsFilled(Request.QueryString["FK"]))
                            isAuto = false;
                    }
                }
                else
                {
                    // RequestTrackBuilder peut-être déjà être renseigné (ex en cas de suppression d'un enregistrement)
                    isTrack &= (null == RequestTrackBuilder);

                    //si purge du grid => Spheres n'alimente pas le log
                    isTrack &= (false == efsdtgRefeferentiel.IsSelfClear);
                }

                if (isTrack)
                {
                    RequestTrackActionEnum action = RequestTrackActionEnum.ListLoad;
                    if (false == IsPostBack)
                    {
                        action = RequestTrackActionEnum.ListLoad;
                    }
                    else
                    {
                        if (null != m_toolbar_Clicked)
                        {
                            if (m_toolbar_Clicked is Control control)
                            {
                                switch (control.ID)
                                {
                                    case "imgSQL":
                                    case "imgZIP":
                                    case "imgMSExcel":
                                    case "imgCSV":
                                        action = RequestTrackActionEnum.ListExport;
                                        break;
                                }
                            }
                            else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs mnu)
                            {
                                if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                                    action = RequestTrackActionEnum.ListExport;
                            }
                        }
                        else if (efsdtgRefeferentiel.IsSelfReload)
                        {
                            action = RequestTrackActionEnum.ListLoad;
                        }
                        else
                        {
                            Control co = GetPostBackControl();
                            if (null != co)
                            {
                                // Control serveur du grid (Changement de page, zone text qui permet de saisir une page)
                                if (co.ClientID.StartsWith("efsdtgRefeferentiel"))
                                {
                                    action = RequestTrackActionEnum.ListLoad;
                                }
                            }
                            else if (this.PARAM_EVENTTARGET.StartsWith("efsdtgRefeferentiel"))
                            {
                                // Control client qui s'appuie sur la publication ASP:javascript:__do_postback
                                // "Afficher sur 1 page" passe ici 
                                action = RequestTrackActionEnum.ListLoad;
                            }
                        }
                    }

                    // Remarque => Utilisation de GetRowPage afin de ne récupérer uniquement que les enregistrements affichés sur la page courante
                    DataRow[] row = efsdtgRefeferentiel.GetRowPage(true);

                    RequestTrackListBuilder builder = new RequestTrackListBuilder
                    {
                        action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(action, isAuto ? RequestTrackActionMode.auto : RequestTrackActionMode.manual),
                        isConsultation = this.efsdtgRefeferentiel.IsConsultation,
                        lstConsult = this.efsdtgRefeferentiel.LocalLstConsult,
                        referential = this.efsdtgRefeferentiel.Referential,
                        parameter = GetCustomObjectParameter2(),
                        Row = row
                    };

                    if (builder.action.First == RequestTrackActionEnum.ListExport)
                    {
                        builder.exportType = GetRequestTrackExportType(out string reportName);
                        builder.reportName = reportName;
                    }
                    RequestTrackBuilder = builder;
                }

            }
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si lancement d'un process
        /// </summary>
        /// FI 20141021 [20350] Add
        private void SetRequestTrackBuilderListProcess()
        {
            Boolean isTrack = SessionTools.IsRequestTrackProcessEnabled && efsdtgRefeferentiel.Referential.RequestTrackSpecified;
            isTrack &= m_IsProcess && IsPostBack && (Target_OnProcessClick == PARAM_EVENTTARGET);
            isTrack &= (false == IsSpheresProcessTypeUsingNormMsgFactory);
            isTrack &= (null == RequestTrackBuilder);
            

            if (isTrack)
            {
                // Remarque => Utilisation de efsdtgRefeferentiel.DsData afin de récupérer toutes les lignes cochées (même celles présentes dans les pages autres que la page courante) 
                DataRow[] row = efsdtgRefeferentiel.DsData.Tables[0].Select("ISSELECTED=true");
                
                Nullable<RequestTrackProcessEnum> processType = null;
                Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(efsdtgRefeferentiel.IDMenu);
                if (menu.HasValue)
                {
                    switch (menu)
                    {

                        case IdMenu.Menu.POSKEEPING_UPDATENTRY:
                            processType = RequestTrackProcessEnum.UpdateEntry;
                            break;
                        case IdMenu.Menu.POSKEEPING_CLEARINGBULK:
                            processType = RequestTrackProcessEnum.ClearingBulk;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING:
                            processType = RequestTrackProcessEnum.UnClearing;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING_MOF:
                            processType = RequestTrackProcessEnum.UnClearing_MOF;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING_MOO:
                            processType = RequestTrackProcessEnum.UnClearing_MOO;
                            break;
                        case IdMenu.Menu.PROCESS_FO_REMOVEALLOC:
                        case IdMenu.Menu.PROCESS_OTC_REMOVEALLOC:
                            processType = RequestTrackProcessEnum.Remove;
                            break;
                    }
                }

                if (null != processType)
                {
                    RequestTrackListBuilder builder = new RequestTrackListBuilder
                    {
                        action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ListProcess, RequestTrackActionMode.manual),
                        ProcessType = processType,
                        isConsultation = this.efsdtgRefeferentiel.IsConsultation,
                        lstConsult = this.efsdtgRefeferentiel.LocalLstConsult,
                        referential = this.efsdtgRefeferentiel.Referential,
                        parameter = GetCustomObjectParameter2(),
                        Row = row
                    };
                    RequestTrackBuilder = builder;
                }
            }
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si lancement d'un process sur un trade
        /// </summary>
        private void SetRequestTrackBuilderTradeProcess(int pIdT, RequestTrackProcessEnum pProcessType)
        {
            if (SessionTools.IsRequestTrackProcessEnabled)
            {
                EFS_TradeLibrary trade = new EFS_TradeLibrary(SessionTools.CS, null, pIdT);
                SpheresIdentification tradeIdentificaton = TradeRDBMSTools.GetTradeIdentification(SessionTools.CS, null, pIdT);

                RequestTrackTradeBuilder builder = new RequestTrackTradeBuilder
                {
                    DataDocument = trade.DataDocument,
                    TradeIdentIdentification = tradeIdentificaton,
                    ProcessType = pProcessType,
                    action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual)
                };
                this.RequestTrackBuilder = builder;
            }
        }

        /// <summary>
        ///  Solicitation du service NorMsgFactory
        /// </summary>
        /// <param name="pProcess"></param>
        /// FI 20180605 [24001] Add
        private void CallProcessServiceUsingNormsgFactory(Cst.ProcessTypeEnum pProcess)
        {
            NormMsgFactoryMQueue Mqueue = ConstructNormMsgFactoryMessage(pProcess);

            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                connectionString = SessionTools.CS,
                process = Cst.ProcessTypeEnum.NORMMSGFACTORY,
                Session = SessionTools.AppSession,
                trackerAttrib = new TrackerAttributes()
                {
                    process = Cst.ProcessTypeEnum.NORMMSGFACTORY,
                    info = TrackerAttributes.BuildInfo(pProcess, Mqueue.buildingInfo.parameters)
                },
                mQueue = new MQueueBase[1] { Mqueue }
            };
            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);

            int idTRK_L = 0;
            MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);

            MsgForAlertImmediate = Ressource.GetString2("Msg_PROCESS_GENERATE_NORMMSGFACTORRY",
                LogTools.IdentifierAndId(taskInfo.process.ToString(), idTRK_L));

        }

        /// <summary>
        /// Retourne un Messque Queue de type NormMsgFactory
        /// </summary>
        /// <param name="pProcess"></param>
        /// <returns></returns>
        /// FI 20180605 [24001] Add
        private NormMsgFactoryMQueue ConstructNormMsgFactoryMessage(Cst.ProcessTypeEnum pProcess)
        {
            MQueueparameters parameters = new MQueueparameters() { parameter = GetCustomObjectParameter2() };

            MQueueAttributes mQueueAttributes = new MQueueAttributes() { connectionString = SessionTools.CS };
            NormMsgFactoryMQueue ret = new NormMsgFactoryMQueue(mQueueAttributes)
            {
                buildingInfo = new NormMsgBuildingInfo
                {
                    processType = pProcess
                }
            };
            switch (pProcess)
            {
                case Cst.ProcessTypeEnum.EARGEN:
                case Cst.ProcessTypeEnum.ACCOUNTGEN:
                    string p1 = Request.QueryString["P1"];
                    if (StrFunc.IsEmpty(p1))
                        throw new ArgumentException("Parameter P1 is mandatory");

                    parameters.Add(
                        new MQueueparameter(NormMsgFactoryMQueue.PARAM_REQUESTTYPE.ToString(), TypeData.TypeDataEnum.@string) { Value = p1 });

                    ret.buildingInfo.idSpecified = false;
                    ret.buildingInfo.identifierSpecified = false;
                    ret.buildingInfo.parameters = parameters;

                    ret.buildingInfo.parametersSpecified = true;
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("Process: {0} is not implemented", pProcess.ToString()));
            }
            return ret;
        }

        /// <summary>
        ///  Lecture des informations critères (LSTWHERE) et tri existantes (LSTORDERBY)
        /// </summary>
        /// FI 2019103 [XXXX] Add Method
        /// EG 20210419 [XXXXX] Upd Nouveau paramètre - Usage du businessCenter de l'entité
        private void LoadInfosLst()
        {
            if (m_IsConsultation)
                InfosLstWhere = efsdtgRefeferentiel.LocalLstConsult.GetInfoWhere(SessionTools.CS, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter);
            else
                InfosLstWhere = efsdtgRefeferentiel.LocalLstConsult.GetInfoWhereFromReferencial(SessionTools.CS, efsdtgRefeferentiel.Referential, SessionTools.User.Entity_IdA, SessionTools.User.Entity_BusinessCenter);

            InfosLstOrderBy = efsdtgRefeferentiel.LocalLstConsult.GetInfoOrderBy(SessionTools.CS);
        }
        /// <summary>
        /// Affiche La date (hh:mm:ss) à laquelle les données ont été chargées et le temps SQL nécessaire dans le tooltip
        /// </summary>
        /// FI 2019103 [XXXX] Add Method
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210331 [25556] Affichage du propriétaire du template actif avant date du dernier refresh
        private void SetLastRefresh()
        {
            if (this.FindControl("lblRightSubtitle") is Label ctrl)
            {
                try
                {
                    ctrl.Visible = true;

                    // FI 20200820 [25468] dates systemes en UTC,  format de date  selon le profil avec précison à la seconde
                    // FI Lecture de la date courante (=> normalement il aurait fallu afficher la date de chargement des données et non pas la date courante)
                    string dtLastRefresh = DtFuncExtended.DisplayTimestampUTC(OTCmlHelper.GetDateSysUTC(SessionTools.CS), new AuditTimestampInfo()
                    {
                        Collaborator = SessionTools.Collaborator,
                        TimestampZone = SessionTools.AuditTimestampZone,
                        Precision = Cst.AuditTimestampPrecision.Second
                    });
                    ctrl.Text = efsdtgRefeferentiel.LocalLstConsult.template.titleOwner + " " + $"{Ressource.GetString("Msg_LastRefresh")} {dtLastRefresh}";

                    // FI 20210112 [XXXXX] Add tooltip information
                    // FI Lecture du temps SQL nécessaire au cahrgement des données
                    ctrl.ToolTip = string.Empty;
                    if (null != efsdtgRefeferentiel.LastQueryTotalTimeSpan)
                        ctrl.ToolTip = $"{Ressource.GetString("Msg_LastSQLDuration")} {efsdtgRefeferentiel.LastQueryTotalTimeSpan.Value:mm\\:ss\\.ff}";
                }
                catch
                {
                    ctrl.Visible = false;
                }
            }
        }
        /// <summary>
        ///  Retourne le jeux de donnée autocomplete du contôle "txtSearch"
        ///  <para>Cette Méthode est utilisé par JQuery Widget autocomplete</para>
        /// </summary>
        /// <param name="guid">Identifiant unique de la page</param>
        /// <param name="request">Donnée saisi par l'utilisateur</param>
        [WebMethod]
        public static List<String> LoadSearchData(string request, string guid)
        {
            List<String> ret = new List<string>();

            AutoCompleteKey autoCompleteKey = new AutoCompleteKey()
            {
                pageGuId = guid,
                controlId = "txtSearch"
            };

            IEnumerable<string> lstData = AutoCompleteDataCache.GetData<List<string>>(autoCompleteKey);
            if (null != lstData)
            {
                lstData = lstData.Distinct();
                // FI 20200107 [XXXXX] call OrderAutocompletata
                ret = AutocompleteTools.OrderAutocompletata(lstData, request).ToList();
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="disabledFilterControlId"></param>
        /// <param name="guid">Identifiant unique de la page</param>
        /// FI 20221026 [XXXXX] Add
        [WebMethod]
        public static void DisabledFilter(string disabledFilterControlId, string guid)
        {
            if (false == SessionTools.IsConnected)
                throw new NullReferenceException("the session is unavailable");

            bool isOptionalFilterDisabled = false;
            int positionFilterDisabled = -1;

            if (disabledFilterControlId == "lnkDisabledFilter")
                isOptionalFilterDisabled = true;
            else if (disabledFilterControlId.StartsWith("imgDisabledFilter"))
                positionFilterDisabled = StrFunc.GetSuffixNumeric2(disabledFilterControlId);

            if ((positionFilterDisabled > -1) || isOptionalFilterDisabled)
            {
                LstConsult localConsult = DataCache.GetData<LstConsult>($"{guid}_LstConsult");
                if (null == localConsult)
                    throw new NullReferenceException("the localConsult is null");

                string IdLstTemplate = localConsult.template.IDLSTTEMPLATE;
                int IdA = localConsult.template.IDA;

                if (!ReferentialWeb.IsTemporary(IdLstTemplate))
                {
                    Pair<string, int> retCopy = localConsult.CreateCopyTemporaryTemplate(SessionTools.CS);
                    IdLstTemplate = retCopy.First;
                    IdA = retCopy.Second;
                    ReferentialWeb.WriteTemplateSession(localConsult.IdLstConsult, IdLstTemplate, IdA, guid);
                }

                localConsult.LoadTemplate(SessionTools.CS, IdLstTemplate, IdA, false);

                if (isOptionalFilterDisabled)
                {
                    localConsult.template.ISENABLEDLSTWHERE = false;
                    localConsult.template.SetIsEnabledLstWhere2(SessionTools.CS);
                }
                else if (positionFilterDisabled > -1)
                {
                    localConsult.template.SetEnabledLstWhereElement(SessionTools.CS, positionFilterDisabled, false);
                }

                // nouveau template courant (Temporaire) ou modifications de filtres => Appel à InitReferential nécessaire pour tout recharger
                DataCache.SetData<Boolean>($"{guid}_IsForceInitReferential", true);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">Identifiant unique de la page</param>
        /// <returns></returns>
        /// FI 20221026 [XXXXX] Add
        [WebMethod]
        public static LstConsult.LstTemplate GetCurrentTemplate(string guid)
        {
            if (false == SessionTools.IsConnected)
                throw new NullReferenceException("the session is unavailable");

            LstConsult localConsult = DataCache.GetData<LstConsult>($"{guid}_LstConsult");
            if (null == localConsult)
                throw new NullReferenceException("the localConsult is null");

            return localConsult.template;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="guid">Identifiant unique de la page</param>
        /// <returns></returns>
        /// FI 20221026 [XXXXX] Add
        [WebMethod]
        public static Tuple<string, string> GetCurrentTemplateSubTitle(string guid)
        {
            if (false == SessionTools.IsConnected)
                throw new NullReferenceException("the session is unavailable");

            LstConsult localConsult = DataCache.GetData<LstConsult>($"{guid}_LstConsult");
            if (null == localConsult)
                throw new NullReferenceException("the localConsult is null");

            return GetSubTitle(localConsult.template);
        }

        /// <summary>
        ///  Vérification que les variables session dédiées au maintient d'état sont encore accessibles
        ///  <para>Si elles ne sont plus accessibles, Spheres® ouvre de nouveau l'URL de manière à ne pas être en PostBack, les variables session vont alors de nouveau être initialiées</para>
        /// </summary>
        /// FI 20200225 [XXXXX] Add Method
        private void CheckSessionState()
        {
            // La méthode ne teste pas toutes les variables sessions de maintient d'état
            // Si les variables ci dessous n'existent plus Spheres® suppose que toutes les variables sessions de maintient d'état n'existent plus 
            if (IsPostBack)
            {
                if (false == DataCache.ContainsKey(DataCacheKeyCustomObjects) ||
                    (false == DataCache.ContainsKey(this.efsdtgRefeferentiel.DataCacheKeyReferential)))
                    Response.Redirect(this.Request.Url.AbsoluteUri);
            }
        }

        /// <summary>
        /// Mise à jour des contrôles pilotés par des CustomObjects
        /// </summary>
        /// FI 20200602 [25370] add 
        private void UpdateCustomObjectsControl()
        {
            //FI 20110812 [17537] Asp ne maintient pas l'état, la combo perd sa valeur ??? 
            //PL 20150511 Refactoring
            if (null != this.CustomObjects)
            {
                // FI 20200602 [25370] Les contrôles sont systématiquement enable=true
                // Il n'y a plus de pb de maintient d'état
                //if (IsPostBack)
                //{
                //    for (int i = 0; i < ArrFunc.Count(CustomObjects.customObject); i++)
                //    {
                //        if (CustomObjects.customObject[i].Control == CustomObject.ControlEnum.dropdown)
                //        {
                //            string clientId = CustomObjects.customObject[i].ClientId;
                //            DropDownList ddl = (DropDownList)this.FindControl(Cst.DDL + CustomObjects.customObject[i].ClientId);
                //            string ddlValue = Request.Params[Cst.DDL + clientId];
                //            ControlsTools.DDLSelectByValue(ddl, ddlValue);
                //        }
                //    }
                //}



                Boolean isSetCustomObjectControlGUIDA = false;
                if (isLoadLSTParam)
                {
                    isSetCustomObjectControlGUIDA = true;
                }
                else if (IsPostBack && StrFunc.IsFilled(ClientIdCustomObjectChanged))
                {
                    // Si le contrôle à l'origine de la publication de la page est lié (*) à plusieurs contôles alors Spheres® alimente tous les contrôles CustomObjects à partir des dynamicArgs de type GUI
                    // (*) le contôle peut provoquer des mofifications potentielles sur plusieurs autres contrôles en cascade
                    // Rq : Les liens sont déclarés dans l'attribut Misc du CustomObject (linked)
                    CustomObject co = this.CustomObjects.customObject.Where(x => x.IsControlData && x.ClientId == ClientIdCustomObjectChanged).FirstOrDefault();
                    isSetCustomObjectControlGUIDA = StrFunc.IsFilled(co.GetMiscValue("linked", string.Empty));
                }
                if (isSetCustomObjectControlGUIDA)
                    SetCustomObjectControlGUIDA();

                // Cas particulier en dur s'il existe DDLPERIODREPORTTYPE et TXTDATE2
                CustomObject co1 = CustomObjects.customObject.Where(x => x.CtrlClientId == "DDLPERIODREPORTTYPE").FirstOrDefault();
                CustomObject co2 = CustomObjects.customObject.Where(x => x.CtrlClientId == "TXTDATE2").FirstOrDefault();
                if ((null != co2) && (null != co1))
                {
                    if (!(FindControl(co2.CtrlClientId) is WCTextBox2 txt))
                        throw new InvalidProgramException("Ctrl TXTDATE2 not found");
                    txt.ReadOnly = (DynamicArgs["PERIODREPORTTYPE"].value != "5");
                }
            }
        }

        /// <summary>
        ///  Si publication de la page, Contrôle et/ou initilise les dynamicArgs DATE1 (ou DTBUSINESS) et DATE2 
        /// </summary>
        /// FI 20200602 [25370] Add
        private void CheckDynamicArgs()
        {

            if (IsPostBack)
            {

                Boolean isOk = (DynamicArgs.ContainsKey("DATE1") && DynamicArgs.ContainsKey("DATE2")) ||
                                     (!DynamicArgs.ContainsKey("DATE1")) && DynamicArgs.ContainsKey("DTBUSINESS") && DynamicArgs.ContainsKey("DATE2");
                if (isOk)
                {
                    Boolean isDate1 = DynamicArgs.ContainsKey("DATE1");

                    ReferentialsReferentialStringDynamicData dynamicArgDt1 = isDate1 ? DynamicArgs["DATE1"] : DynamicArgs["DTBUSINESS"];
                    ReferentialsReferentialStringDynamicData dynamicArgDt2 = DynamicArgs["DATE2"];


                    if (PARAM_EVENTTARGET == Cst.DDL + "PERIODREPORTTYPE")
                    {
                        string periodType = DynamicArgs["PERIODREPORTTYPE"].value;

                        DateTime firstDateDefaultValue = DateTime.MinValue;
                        DateTime secondDateValue = DateTime.MinValue;

                        switch (periodType)
                        {
                            case "1": //Daily
                                break;
                            case "2": //Weekly
                                firstDateDefaultValue = DateTime.Today.AddDays(1 - (int)DateTime.Today.DayOfWeek).AddDays(-7);
                                secondDateValue = firstDateDefaultValue.AddDays(6);
                                break;
                            case "3": //Monthly
                                firstDateDefaultValue = DateTime.Today.AddDays(1 - DateTime.Today.Day).AddMonths(-1);
                                secondDateValue = firstDateDefaultValue.AddMonths(1).AddDays(-1);
                                break;
                            case "4": //Yearly
                                firstDateDefaultValue = DateTime.Today.AddDays(1 - DateTime.Today.DayOfYear).AddYears(-1);
                                secondDateValue = firstDateDefaultValue.AddYears(1).AddDays(-1);
                                break;
                            case "5": //Other
                                break;
                        }

                        // Alimentation de dynamicArgDt1
                        if (firstDateDefaultValue != DateTime.MinValue)
                            dynamicArgDt1.value = DtFunc.DateTimeToString(firstDateDefaultValue, DtFunc.FmtISODate);

                        // Alimentation de dynamicArgDt2
                        if (periodType == "1") //Daily
                            dynamicArgDt2.value = dynamicArgDt1.value;
                        else if (secondDateValue != DateTime.MinValue)
                            dynamicArgDt2.value = DtFunc.DateTimeToString(secondDateValue, DtFunc.FmtISODate);
                    }
                    else
                    {
                        DateTime date1 = new DtFunc().StringDateISOToDateTime(dynamicArgDt1.value);
                        DateTime date2 = new DtFunc().StringDateISOToDateTime(dynamicArgDt2.value);

                        Boolean isExistsPeriofType = DynamicArgs.ContainsKey("PERIODREPORTTYPE");

                        Boolean defaultBehavior = !isExistsPeriofType;
                        if (isExistsPeriofType)
                        {
                            string periodType = DynamicArgs["PERIODREPORTTYPE"].value;
                            switch (periodType)
                            {
                                case "1": //Daily
                                    dynamicArgDt2.value = DtFunc.DateTimeToString(date1, DtFunc.FmtISODate);
                                    break;
                                case "2": //Weekly
                                    dynamicArgDt2.value = DtFunc.DateTimeToString(date1.AddDays(7).AddDays(-1), DtFunc.FmtISODate);
                                    break;
                                case "3": //Monthly
                                    dynamicArgDt2.value = DtFunc.DateTimeToString(date1.AddMonths(1).AddDays(-1), DtFunc.FmtISODate);
                                    break;
                                case "4": //Yearly
                                    dynamicArgDt2.value = DtFunc.DateTimeToString(date1.AddYears(1).AddDays(-1), DtFunc.FmtISODate);
                                    break;
                                case "5": //Other
                                    defaultBehavior = true;
                                    break;
                            }
                        }

                        if (defaultBehavior)
                        {
                            if (-1 < date1.CompareTo(date2))
                            {
                                if (PARAM_EVENTTARGET.Contains(isDate1 ? "DATE1" : "DTBUSINESS"))
                                    dynamicArgDt2.value = dynamicArgDt1.value;
                                else if (PARAM_EVENTTARGET.Contains("DATE2"))
                                    dynamicArgDt1.value = dynamicArgDt2.value;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Mise en place d'un lien, par défaut, entre certains CustomObjects 
        /// <para>Les liens sont généralement déclarés dans les attributs misc dans les CustomObjects</para>
        /// </summary>
        /// FI 20200602 [25370] Add Method
        private void SetCustomObjectLinked()
        {
            if (null != this.CustomObjects)
            {
                IEnumerable<CustomObject> col =
                            CustomObjects.customObject.Where(x => x.IsControlData && StrFunc.IsEmpty(x.GetMiscValue("linked")) &&
                            ArrFunc.ExistInArrayString(new string[4] { "DATE1", "DTBUSINESS", "DATE2", "PERIODREPORTTYPE" }, x.ClientId));

                if (col.Count() > 1)
                {
                    // Exemple s'il DATE1 (ou DTBUSINESS) et DATE2 => Un lien est créé automatiquement signifiant que la valeur de DATE1 peut influencer sur DATE2 et vis-et-versa
                    foreach (CustomObject item in col)
                    {
                        if (item.ContainsMisc)
                            item.Misc += ";linked:defaultLink";
                        else
                            item.Misc = "linked:defaultLink";
                    }
                }
            }
        }


        /// <summary>
        /// Retourne un array de MQueueparameter à partir des contrôles générés depuis les customObject.
        /// </summary>
        /// <returns></returns>
        /// FI 20200602 [25370] Reecriture
        private MQueueparameter[] GetCustomObjectParameter2()
        {
            MQueueparameter[] ret = null;
            if (CustomObjects != null)
            {
                List<MQueueparameter> al = new List<MQueueparameter>();
                foreach (CustomObject item in CustomObjects.customObject.Where(x => x.IsControlData))
                {
                    if ((false == item.ContainsDataType) || (false == Enum.IsDefined(typeof(TypeData.TypeDataEnum), item.DataType)))
                        throw new InvalidProgramException($"datatype {item.DataType} invalid for {item.ClientId}");

                    MQueueparameter parameter = new MQueueparameter()
                    {
                        id = item.ClientId,
                        dataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), item.DataType, true)
                    };

                    switch (parameter.dataType)
                    {
                        case TypeData.TypeDataEnum.date:
                        case TypeData.TypeDataEnum.datetime:
                        case TypeData.TypeDataEnum.time:
                            #region date,datetime,time
                            if (IsPostBack)
                            {
                                if (StrFunc.IsFilled(Request.Form[Cst.TXT + parameter.id]))
                                    parameter.SetValue(new DtFunc().StringToDateTime(Request.Form[Cst.TXT + parameter.id]));
                            }
                            else
                            {
                                if (!(this.FindControl(Cst.TXT + parameter.id) is WCTextBox2 txtbox))
                                    throw new InvalidProgramException($"Control for {parameter.id} not found");
                                if (StrFunc.IsFilled(txtbox.Text))
                                    parameter.SetValue(new DtFunc().StringToDateTime(txtbox.Text));
                            }
                            #endregion
                            break;
                        case TypeData.TypeDataEnum.@string:
                        case TypeData.TypeDataEnum.text:
                        case TypeData.TypeDataEnum.integer:
                        case TypeData.TypeDataEnum.@int: // FI 20210323 [XXXXX] Add
                        case TypeData.TypeDataEnum.@decimal:
                        case TypeData.TypeDataEnum.dec:
                            #region string,text,integer,decimal
                            if (IsPostBack)
                            {
                                if (ArrFunc.ExistInArray(Request.Form.AllKeys, $"{Cst.TXT}{parameter.id}"))
                                {
                                    if (StrFunc.IsFilled(Request.Form[$"{Cst.TXT}{parameter.id}"]))
                                        parameter.SetValue(Request.Form[$"{Cst.TXT}{parameter.id}"]);
                                }
                                else if (ArrFunc.ExistInArray(Request.Form.AllKeys, $"{Cst.DDL}{parameter.id}"))
                                {
                                    string value = Request.Params[$"{Cst.DDL}{parameter.id}"];
                                    if (StrFunc.IsFilled(value))
                                    {
                                        WCDropDownList2 ddl = this.FindControl($"{Cst.DDL}{parameter.id}") as WCDropDownList2;
                                        ControlsTools.DDLSelectByValue(ddl, value);
                                        parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                    }
                                }
                                else
                                    throw new InvalidProgramException($"Control for {parameter.id} not found");
                            }
                            else
                            {
                                if (this.FindControl($"{Cst.TXT}{parameter.id}") is WCTextBox2 txtbox)
                                {
                                    if (StrFunc.IsFilled(txtbox.Text))
                                        parameter.SetValue(txtbox.Text);
                                }
                                else
                                {
                                    if (this.FindControl($"{Cst.DDL}{parameter.id}") is WCDropDownList2 ddl)
                                    {
                                        if (StrFunc.IsFilled(ddl.SelectedValue))
                                            parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                    }
                                    else
                                    {
                                        throw new InvalidProgramException($"Control for {parameter.id} not found");
                                    }
                                }
                            }

                            #endregion
                            break;
                        case TypeData.TypeDataEnum.@bool:
                            // Sur les checkBox ASP a un comportement un peu particulier 
                            // Si la checkBox est cochée Request.Form la checkBox avec la valeur "on"
                            // Sinon Request.Form ne contient pas la checkBox
                            if (this.IsPostBack)
                            {
                                if (StrFunc.IsFilled(Request.Form[$"{Cst.CHK}{parameter.id}"]))
                                    parameter.SetValue(true);
                                else
                                    parameter.SetValue(false);
                            }
                            else
                            {
                                //FI 20091106 [16722] use WCCheckBox2
                                if (FindControl($"{Cst.CHK}{parameter.id}") is WCCheckBox2 checkbox)
                                {
                                    parameter.SetValue(checkbox.Checked);
                                }
                                else
                                {
                                    throw new InvalidProgramException($"Control for {parameter.id} not found");
                                }
                            }
                            break;
                    }
                    al.Add(parameter);
                }
                ret = al.ToArray();
            }
            return ret;
        }
        /// <summary>
        /// Contrôle sur les CustomObject (généré à partir du fichier GUI)
        /// <para>Retourne une InvalidProgramException s'ils sont incorrectement paramétrés</para>
        /// </summary>
        /// FI 20200602 [25370] Add
        private void CheckCustomObject()
        {
            if (null != CustomObjects)
            {
                /* Tous les CustomObjects doivent être IsAutoPostBack (*)
                 * Cela afin de publier la page en cas de modification. Ceci afin de généré un template temporaire et d'y stocker les paramètres GUI en vigueur dans la table LSTPARAM
                 * (*) sauf s'ils sont déclarés lstParam:false
                 */
                IEnumerable<string> custom = from item in CustomObjects.customObject.Where(x => (x.IsControlData && x.GetMiscValue("lstParam", "true") == "true" && (false == x.IsAutoPostBack)))
                                             select item.ClientId;
                if (custom.Count() > 0)
                    throw new InvalidProgramException($"CustomObjects {StrFunc.StringArrayList.StringArrayToStringList(custom.ToArray(), false)} is/are not postback. Please add postback attribute");
            }
        }
        /// <summary>
        /// Ajoute les contôles CustomObjects dans le panel plhParamProcess
        /// </summary>
        /// FI 20200602 [25370] Add
        private void BuildCustomObjectsControl()
        {
            if (null != CustomObjects)
            {
                //Tous les contôles de saisie doivent être Enabled=true du fait de l'usage de la méthode GetCustomObjectParameter2 (*) sur le onInit de la page
                //(*) GetCustomObjectParameter2 effectue des lectures depuis Request.Form
                Boolean isControlDataEnabled = true;

                Panel pnl = CustomObjects.CreateTable(this, isControlDataEnabled, mainMenuClassName);
                plhParamProcess.Controls.Add(pnl);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200731 [XXXXX] Add
        // EG 20200818 [XXXXX] Rename SetAutoRefresh
        protected override void SetAutoRefresh()
        {
            timerRefresh.Enabled = (AutoRefresh > 0);
            if (timerRefresh.Enabled)
                timerRefresh.Interval = AutoRefresh * 1000;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20200731 [XXXXX] Add
        protected void OnTimerRefresh(object sender, EventArgs e)
        {
            //Pour l'instant rien => Le rafraisshement du prid se fait automatiquement
        }

        /// <summary>
        ///  Alimentation de la property ClientIdCustomObjectChanged
        /// </summary>
        /// FI 20210305 [XXXXX] add Method
        private void SetClientIdCustomObjectChanged()
        {
            string ret = string.Empty;
            if (IsPostBack && StrFunc.IsFilled(PARAM_EVENTTARGET) && (null != CustomObjects))
            {
                if (PARAM_EVENTTARGET == "imgRefresh")
                {
                    //Remarque: ici on essaie de trapper le cas suivant
                    //L'utilisateur appuie sur entrée alors qu'il vient de modifier un contrôle GUI 
                    //Dans ce cas __EVENTTARGET retourne "imgRefresh" puisque imgRefresh est le DefaultButton de ce formulaire (voir OnInit)
                    //Toutefois ClientIdCustomObjectChanged serra alimenté avec le contrôle GUI modifié (ceci est nécessaire car il y a maj de LSTPARAM dans ce cas)  
                    List<string> lstKeyGUI = (from item in DynamicArgs.Where(x => x.Value.source == DynamicDataSourceEnum.GUI)
                                              select item.Key).ToList();

                    foreach (string key in lstKeyGUI)
                    {
                        string previousValue = efsdtgRefeferentiel.Referential.dynamicArgs[key].value;
                        string currentValue = DynamicArgs[key].value;
                        if (previousValue != currentValue)
                        {
                            CustomObject co = CustomObjects.customObject.Where(x => x.IsControlData && x.ClientId == key).FirstOrDefault();
                            if (null != co)
                            {
                                ret = co.ClientId;
                                // FI 20210326 [XXXXX] Alimentation de ActiveElementForced pour que le focus soit appliqué sur le contrôle 
                                ActiveElementForced = co.CtrlClientId;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    CustomObject co = CustomObjects.customObject.Where(x => x.CtrlClientId == PARAM_EVENTTARGET).FirstOrDefault();
                    if (null != co)
                        ret = co.ClientId;
                }

                ClientIdCustomObjectChanged = ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20211228 [XXXXX] Add Method
        // EG 20240207 [WI825] Logs: Harmonization data of consultation (VW_ATTACHEDDOC_TRACKER_L)
        private async Task ExportZIPAsync(CancellationToken cancellationToken)
        {
            DatetimeProfiler dtProfiler = new DatetimeProfiler();

            DataTable tblMain = ((DataView)efsdtgRefeferentiel.DataSource).Table;

            string path = FileTools.GetUniqueName("EXPORTZIP", tblMain.TableName);
            path = SessionTools.TemporaryDirectory.MapPath(path);
            path += @"\";

            bool isOk = false;

            #region Ecriture des fichiers dans un répertoire temporaire
            if (efsdtgRefeferentiel.IsDataSourceAvailable)
            {
                string alias = efsdtgRefeferentiel.Referential.AliasTableName;
                string cs = SessionTools.CS;

                Boolean isMCO = (tblMain.TableName == Cst.OTCml_TBL.VW_MCO.ToString()) ||
                            (tblMain.TableName == Cst.OTCml_TBL.VW_INVMCO.ToString()) ||
                            (tblMain.TableName == Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString()) ||
                            (tblMain.TableName == "MCO" && m_IsConsultation);

                Boolean isATTACHEDDOC = (tblMain.TableName == Cst.OTCml_TBL.ATTACHEDDOC.ToString()) ||
                                        (tblMain.TableName == Cst.OTCml_TBL.VW_ATTACHEDDOC_TRACKER_L.ToString());

                if (!(isMCO || isATTACHEDDOC))
                    throw new NotSupportedException($"table: {tblMain.TableName} is not supported");

                if (isMCO)
                {
                    //FI 20190830 [XXXXX] Message InvalidProgramException si la colonne IDMCO n'existe pas 
                    DataColumn colIDMCO = (from item in tblMain.Columns.Cast<DataColumn>().Where(x => x.ColumnName == "IDMCO")
                                           select item).FirstOrDefault();
                    if (null == colIDMCO)
                        throw new InvalidProgramException("Column IDMCO doesn't exists");
                }

                List<LOFileColumn> lstLOFileColumn = new List<LOFileColumn>();
                try
                {
                    Boolean existColumnISSELECTED = tblMain.Columns.Contains("ISSELECTED");
                    foreach (DataRow row in tblMain.Rows.Cast<DataRow>().Where(x => (existColumnISSELECTED && Convert.ToBoolean(x["ISSELECTED"]) || (false == existColumnISSELECTED))))
                    {

                        Boolean isAddItem = false;

                        string tableName = null;
                        string columnName_Data = null;
                        string columnName_FileName = null;
                        string columnName_Type = null;
                        string[] keyColumns = null;
                        string[] keyValues = null;
                        string[] keyDatatypes = null;

                        if (isMCO)
                        {
                            tableName = tblMain.TableName;
                            if ((alias == "mco_rpt") ||
                                (alias == "mco_rpt_finper"))
                            {
                                tableName = Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString();
                            }

                            columnName_FileName = "IDMCO";
                            if (tblMain.Columns.Contains("DOCNAME") && !(row["DOCNAME"] is DBNull))
                                columnName_FileName = "DOCNAME";

                            keyColumns = new string[] { "IDMCO" };
                            keyValues = new string[] { row["IDMCO"].ToString() };
                            keyDatatypes = new string[] { "int" };

                            if (tblMain.Columns.Contains("LOCNFMSGBIN") && !(row["LOCNFMSGBIN"] is DBNull))
                            {
                                isAddItem = true;
                                columnName_Data = "LOCNFMSGBIN";
                                columnName_Type = "DOCTYPEMSGBIN";
                            }
                            else if (tblMain.Columns.Contains("LOCNFMSGTXT") && !(row["LOCNFMSGTXT"] is DBNull))
                            {
                                isAddItem = true;
                                columnName_Data = "LOCNFMSGTXT";
                                columnName_Type = "DOCTYPEMSGTXT";
                            }
                        }
                        else if ((tblMain.TableName == Cst.OTCml_TBL.ATTACHEDDOC.ToString()) ||
                                (tblMain.TableName == Cst.OTCml_TBL.VW_ATTACHEDDOC_TRACKER_L.ToString()))
                        {
                            isAddItem = true;

                            tableName = tblMain.TableName;
                            columnName_FileName = "DOCNAME";
                            columnName_Data = "LODOC";
                            columnName_Type = "DOCTYPE";

                            keyColumns = new string[] { "TABLENAME", "ID", "DOCNAME" };
                            keyValues = new string[] { row["TABLENAME"].ToString(), row["ID"].ToString(), row["DOCNAME"].ToString() };
                            keyDatatypes = new string[] { "string", "int", "string" };
                        }

                        if (isAddItem)
                        {
                            LOFileColumn dbfc = new LOFileColumn(cs, tableName,
                            columnName_Data, columnName_FileName, columnName_Type,
                            keyColumns, keyValues, keyDatatypes);
                            lstLOFileColumn.Add(dbfc);
                        }

                        if (isMCO)
                        {
                            //FI 20160411 [XXXXX] Ajout du flux XML de messagerie 
                            //- Facilite la mise à dispo d'exemple dans I:\INST_SPHERES\Reports (exemples-maquettes)
                            //- Permet au support de récupérer le flux XML et son rendu 
                            Boolean isAddXmlFlow = IsTrace;
#if DEBUG
                            isAddXmlFlow = true;
#endif
                            if (isAddXmlFlow)
                            {
                                LOFileColumn dbfc = new LOFileColumn(cs, tableName,
                                    "CNFMSGXML", columnName_FileName, "DOCTYPEMSGTXT",
                                    keyColumns, keyValues, keyDatatypes);

                                lstLOFileColumn.Add(dbfc);
                            }
                        }
                    }

                    isOk = (0 < lstLOFileColumn.Count);
                    if (isOk)
                    {
                        IEnumerable<IGrouping<string, LOFileColumn>> grp = lstLOFileColumn.GroupBy(x => $"Table:{x.TableName};Column:{x.ColumnName}");

                        foreach (IGrouping<string, LOFileColumn> grpItem in grp)
                        {
                            LOFileColumn[] LOFileColumn = grpItem.Select(x => x).ToArray();

                            //Spheres® execute n requêtes (lstquery) en parallèle de type "select union all" constitué au maximum de 30 select 
                            int divisor = 5;
                            int nbSelectMax = Math.Min(LOFileColumn.Count() / divisor + 1, 30);
                            /* 1 select Max par requête jusqu'à 4 fichiers, puis 
                             * 2 select Max par requête jusqu'à 9 fichiers, puis 
                             * 3 select Max par requête jusqu'à 14 fichiers, puis 
                             * 4 select max par requête jusqu'à 19 fichiers, etc */

                            List<string> lstquery = new List<string>(); //Liste des requêtes de type "select union all"
                            int quot = Math.DivRem(LOFileColumn.Length, nbSelectMax, out int result);
                            int max = (result > 0) ? quot + 1 : quot;
                            for (int i = 0; i < max; i++)
                            {
                                StrBuilder query = new StrBuilder();
                                for (int j = i * nbSelectMax; j <= Math.Min(((i + 1) * nbSelectMax) - 1, LOFileColumn.Length - 1); j++)
                                    query.Append(LOFileColumn[j].GetSqlSelect() + Cst.CrLf + SQLCst.UNIONALL);
                                lstquery.Add(StrFunc.Before(query.ToString(), Cst.CrLf + SQLCst.UNIONALL, OccurenceEnum.Last));
                            }

                            // Execution des n requêtes de chargement en parallèle
                            List<Task<DataTable>> lstTask = new List<Task<DataTable>>();
                            foreach (string item in lstquery)
                            {
                                Task<DataTable> taskRow = Task.Run<DataTable>(() =>
                                {
                                    return DataHelper.ExecuteDataTable(cs, item);
                                }, cancellationToken);
                                lstTask.Add(taskRow);
                            }

                            while (0 < lstTask.Count)
                            {
                                Task<DataTable> firstFinishedTask = await Task.WhenAny(lstTask);
                                lstTask.Remove(firstFinishedTask);

                                // Dès qu'une requête de chargement est terminée => Ecriture en parrallèle des n fichiers (si possible)
                                await firstFinishedTask.ContinueWith(x =>
                                {
                                    IEnumerable<DataRow> rows = x.Result.Rows.Cast<DataRow>();
                                    Boolean isExistHTMLFile = rows.Where(r => (r["FILETYPE"].ToString() == Cst.TypeMIME.Text.Html)).Count() > 0;
                                    if (isExistHTMLFile)
                                    {
                                        //Ecriture synchrone
                                        foreach (DataRow row in rows)
                                        {
                                            LOFile loFile = new LOFile(row["FILENAME"].ToString(), row["FILETYPE"].ToString(), row["FILECONTENT"]);
                                            AspTools.WriteLOFile(loFile, path);
                                        }
                                    }
                                    else
                                    {
                                        //Ecriture asynchrone
                                        List<Task> lstTaskWrite = new List<Task>();
                                        foreach (DataRow row in rows)
                                        {
                                            lstTaskWrite.Add(Task.Run(() =>
                                            {
                                                LOFile loFile = new LOFile(row["FILENAME"].ToString(), row["FILETYPE"].ToString(), row["FILECONTENT"]);
                                                AspTools.WriteLOFile(loFile, path);
                                            }));
                                        }
                                        Task.WaitAll(lstTaskWrite.ToArray());
                                    }
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    isOk = false;
                    // FI 20240502 [XXXXX] call WriteLogException
                    WriteLogException(ex);
                    MsgForAlertImmediate = "Error on open Write File :" + ex.Message;
                }
            }
            #endregion

            #region Génération du fichier ZIP
            string zipFile = string.Empty;
            if (isOk)
            {
                try
                {
                    zipFile = ZipCompress.CompressFolder(path, path, tblMain.TableName);
                }
                catch (Exception ex)
                {
                    // FI 20240502 [XXXXX] call WriteLogException
                    WriteLogException(ex);
                    isOk = false;
                    MsgForAlertImmediate = "Error on generate Zip File :" + ex.Message;
                }
            }
            System.Diagnostics.Debug.WriteLine($"ExportZIPAsync:{dtProfiler.GetTimeSpan().TotalSeconds}");
            #endregion

            #region Ouverture du fichier ZIP
            if (isOk)
            {
                try
                {
                    // EG 20190416 [ExportFromCSV] Correction Post-Fusion Step5
                    Response.AppendCookie(AspTools.InitHttpCookie(RequestTrackExportType.ZIP + "fileToken", false, ExportTokenValue));
                    AspTools.OpenBinaryFile(this, zipFile, Cst.TypeMIME.Application.XZipCompressed, true);
                }
                catch (Exception ex)
                {
                    // FI 20240502 [XXXXX] call WriteLogException
                    WriteLogException(ex);
                    isOk = false;
                    MsgForAlertImmediate = "Error on open Zip File :" + ex.Message;
                }
            }
            #endregion
        }
        /// <summary>
        ///  Retourne subTitLeft, subTitleRight
        /// </summary>
        /// FI 20221026 [XXXXX] Add
        /// <param name="template"></param>
        /// <returns></returns>
        private static Tuple<string, string> GetSubTitle(LstConsult.LstTemplate template)
        {
            Tuple<string, string> ret;

            string subTitleLeft = Ressource.DecodeMenu_Shortname2(template.subTitle);
            if (template.IsTemporary)
                subTitleLeft = @"<i class='fas fa-star-of-life'></i>" + subTitleLeft;
            subTitleLeft = Ressource.DecodeMenu_Shortname2(Ressource.GetString("ViewerModel_Title")) + ": " + subTitleLeft;

            string subTitleRight = template.titleOwner;

            ret = new Tuple<string, string>(subTitleLeft, subTitleRight);

            return ret;
        }

        /// <summary>
        ///  Initialisation de isInitReferential
        /// </summary>
        /// FI 20221026 [XXXXX] Add
        private void InitIsInitReferential()

        {
            if (DataCache.GetData<Boolean>($"{this.GUID}_IsForceInitReferential"))
            {
                isInitReferential = true;
                DataCache.SetData<Boolean>($"{this.GUID}_IsForceInitReferential", false);
            }
            else
            {
                // FI 20200602 [25370] valorisation de isInitReferential et isLoadLSTParam
                isInitReferential = (false == IsPostBack) || (efsdtgRefeferentiel.IsSelfReload && (PARAM_EVENTTARGET != "imgRefresh")) || efsdtgRefeferentiel.IsSelfClear || StrFunc.IsFilled(ClientIdCustomObjectChanged);
            }
        }

    }
    #endregion Methods
}