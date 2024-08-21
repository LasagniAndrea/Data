#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.Process;
using EfsML.Business;
using EfsML.CorporateActions;
using EfsML.Enum;
using FixML.Enum;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;

#endregion Using Directives

namespace EFS.Spheres
{
    #region LoadTemplateEventArgs
    /// <summary>
    /// Arguments rattachés au délégué Evénement : LoadTemplateHandler
    /// </summary>
    public class LoadTemplateEventArgs : EventArgs
    {
        public bool isLoadRequested;
        public LoadTemplateEventArgs(bool pIsLoadRequested)
        {
            this.isLoadRequested = pIsLoadRequested;
        }
    }
    #endregion LoadTemplateEventArgs

    #region CorporateActionIssuePage
    /// <summary>
    /// Page de saisie, consultation des Corporate actions (publiées (CORPOACTIONISSUE) ou intégrées (CORPOACTION)
    /// ATTENTION PENSER A NICEFILEINPUT.JS si modification UPLOAD
    /// </summary>
    /// FI 20160804 [Migration TFS] Modify
    public partial class CorporateActionIssuePage : PageBase
    {
        #region Members
        protected string xsltFile = @"~\GUIOutput\CorporateActions\ResultsCA.xslt";
        /// <summary>
        /// Délégué d'événement utilisé lors de l'initialisation des dropdowns (CEMode, CEType, etc)
        /// pour éviter la multiplicité des appels à chargement des contrôles liés aux composants simples.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="ddle"></param>
        protected delegate void LoadTemplateHandler(object sender, LoadTemplateEventArgs ddle);
        /// <summary>
        /// Corporate action courante (hors création)
        /// </summary>
        protected CorporateAction m_CorporateAction;
        /// <summary>
        /// Corporate action docs attachées
        /// </summary>
        // EG 20140518 [19913]
        protected CorporateDocs m_CorporateDocs;
        /// <summary>
        /// Source (Provenance) de la Corporate Action (CORPOACTIONISSUE (default) / CORPOACTION)
        /// </summary>
        protected CAInfo m_CAInfo;
        /// <summary>
        /// ID du placeHolder CONTAINER des contrôles dynamiques
        /// </summary>
        private const string plhComponents = "plhCEComponents";
        private const string plhComponents_AI = "plhCEComponents_AI";
        /// <summary>
        /// ID du placeHolder CONTAINER des contrôles des références de notices additionnelles
        /// </summary>
        private const string plhRefNotices = "plhCARefNoticeAdds";
        //PL 20130801 Déplacer dans PageBase.cs
        //private const string SCRIPT_DOFOCUS = @"window.setTimeout('DoFocus()', 1);function DoFocus(){try {document.getElementById('REQUEST_LASTFOCUS').focus();} catch (ex) {}}";
        /// <summary>
        /// classe CSS liée au menu d'appel de la forme (CA intégrées ou publiées)
        /// </summary>
        private string cssClassMaster = "caissue";
        /// <summary>
        /// Dictionnaire des templates d'ajustement
        /// Clé  : Méthode (type AdjustmentMethodOfDerivContractEnum)
        /// Value: Dictionnaire de templates (fichiers XML présents dans répertoire CorporateActions/Templates)
        ///        Clé   : CEType (ou CETYPE + '_' + CECOMBINEDOPERAND + '_' + CECOMBINEDTYPE)
        ///        Valeur: Liste des templates (Paire <Nom du template, Template déserialisé dans classe de type CorporateEventRPocedure>)
        /// </summary>
        public Dictionary<AdjustmentMethodOfDerivContractEnum, Dictionary<string, List<Pair<string, CorporateEventProcedure>>>> m_DicTemplates;
        public Dictionary<string, List<Pair<string, object>>> m_DicTemplates_Additional;

        /// <summary>
        /// Dictionnaire de marchés (ETD exclusivement)
        /// Clé  : idM
        /// Value: données de type MarketIdentification
        /// </summary>
        public Dictionary<int, MarketIdentification> m_DicMarket;
        /// <summary>
        /// Dictionnaire de règles d'arrondi par marché
        /// Clé  : idM
        /// Value: Liste des règles d'arrondi (Paire <Element d'ajustement, règle d'arrondi dans classe de type Rounding>)
        /// </summary>
        public Dictionary<int, CorporateEventMktRules> m_DicMarketRules;
        /// <summary>
        /// Liste des composants simples de la procédure d'ajustement courante
        /// </summary>
        public List<ComponentSimple> m_ListComponentSimples = new List<ComponentSimple>();
        /// <summary>
        /// Liste des composants simples de la procédure d'ajustement courante
        /// </summary>
        public List<ComponentSimple> m_ListComponentSimples_AI = new List<ComponentSimple>();
        /// <summary>
        /// Dictionnaire de marchés des sous-jacents
        /// Clé  : idM
        /// Value: données de type MarketIdentification
        /// </summary>
        public Dictionary<int, MarketIdentification> m_DicUNLMarket;

        private string mainMenuName;
        #endregion Members
        #region Accessors
        #region ActiveCEAdditionalInfo
        /// <summary>
        /// Procédure d'ajustement (type CorporateEventProcedure) attachée au template sélectionné
        /// </summary>
        private AdditionalInfos ActiveCEAdditionalInfo
        {
            get
            {
                AdditionalInfos ai = null;
                if (-1 < ddlCETemplate_AI.SelectedIndex)
                {

                    ai = m_DicTemplates_Additional["AI"].Find(match => match.First == ddlCETemplate_AI.SelectedValue).Second as AdditionalInfos;
                    ai.templateFileName = "AI_" + ddlCETemplate_AI.SelectedValue;
                }
                return ai;
            }
        }
        #endregion ActiveCEAdditionalInfo
        #region ActiveCEAdjustmentContract
        private AdjustmentContract ActiveCEAdjustmentContract
        {
            get
            {
                AdjustmentContract ct = null;
                if (-1 < ddlCEAdjMethod.SelectedIndex)
                {
                    if (m_DicTemplates_Additional["CT"].Exists(match => match.First == ddlCEAdjMethod.SelectedValue))
                    {
                        ct = m_DicTemplates_Additional["CT"].Find(match => match.First == ddlCEAdjMethod.SelectedValue).Second as AdjustmentContract;
                    }
                }
                return ct;
            }
        }
        #endregion ActiveCEAdjustmentContract

        #region ActiveCEProcedure
        /// <summary>
        /// Procédure d'ajustement (type CorporateEventProcedure) attachée au template sélectionné
        /// </summary>
        private CorporateEventProcedure ActiveCEProcedure
        {
            get
            {
                CorporateEventProcedure procedure = new CorporateEventProcedure();
                List<Pair<string, CorporateEventProcedure>> lstTemplates = ListTemplatesAttached;
                if ((null != lstTemplates) && (0 < lstTemplates.Count))
                {
                    // il existe un fichier Template
                    procedure = lstTemplates.Find(match => match.First == ddlCETemplate.SelectedValue).Second;
                }
                else
                {
                    // pas de fichier template on initialise l'élément de base Adjustment
                    // en fonction de la méhthode d'ajustement sélectionnée
                    procedure.underlyers = new CorporateEventUnderlyer[1] { new CorporateEventUnderlyer() };
                    AdjustmentMethodOfDerivContractEnum adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue).Value;
                    switch (adjMethod)
                    {
                        case AdjustmentMethodOfDerivContractEnum.FairValue:
                            procedure.adjustment = new AdjustmentFairValue();
                            break;
                        case AdjustmentMethodOfDerivContractEnum.None:
                            procedure.adjustment = new AdjustmentNone();
                            break;
                        case AdjustmentMethodOfDerivContractEnum.Package:
                            procedure.adjustment = new AdjustmentPackage();
                            break;
                        case AdjustmentMethodOfDerivContractEnum.Ratio:
                            procedure.adjustment = new AdjustmentRatio();
                            break;
                    }
                    procedure.adjustment.method = adjMethod;
                }
                if (null != procedure.adjustment)
                {
                    AdditionalInfos ai = ActiveCEAdditionalInfo;
                    procedure.adjustment.additionalInfoSpecified = (null != ai);
                    if (procedure.adjustment.additionalInfoSpecified)
                    {
                        procedure.adjustment.additionalInfo = ai.additionalInfo;
                        procedure.adjustment.templateFileName_AI = ai.templateFileName;
                    }

                    procedure.adjustment.contract = ActiveCEAdjustmentContract;
                    procedure.adjustment.templateFileName_CT = (null != procedure.adjustment.contract)?"CT_" + ddlCEAdjMethod.SelectedValue:string.Empty;
                }
                return procedure;
            }
        }
        #endregion ActiveCEProcedure
        #region CA_GUID
        /// <summary>
        /// Utilisé comme clé unique de stockage (Session) de la corporate action courante
        /// </summary>
        private string CA_GUID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("CA");
            }
        }
        #endregion CA_GUID
        #region CADOC_GUID
        // EG 20140518 [19913]
        private string CADOC_GUID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("CADOC");
            }
        }
        #endregion CADOC_GUID
        #region IsCombination
        /// <summary>
        /// Retourne si la corporate action est de type combiné
        /// </summary>
        private bool IsCombination
        {
            get {return (ddlCEGroup.SelectedValue == CorporateEventGroupEnum.Combination.ToString());}
        }
        #endregion IsCombination

        #region KeyCEType
        /// <summary>
        /// Clé de recherche d'un template dans le dictionnaire des templates par méthode d'ajustement
        /// déterminée par le(s) type(s) de Corporate Event:
        /// CETYPE dans le cas d'un événement simple
        /// CETYPE + "_" + CE_COMBINEDOPERAND + "_" + CECOMBINEDTYPE] dans le cas d'un événement combiné
        /// </summary>
        private string KeyCEType
        {
            get
            {
                string _keyType = ddlCEType.SelectedValue;
                if (CATools.IsCEGroupCombination(ddlCEGroup.SelectedValue))
                    _keyType += "_" + ddlCECombinedOperand.SelectedValue + "_" + ddlCECombinedType.SelectedValue;
                return _keyType;
            }
        }
        #endregion KeyCEType
        #region ListTemplatesAttached
        /// <summary>
        /// Liste des templates associés au(x) type(s) de Corporate event et la méthode d'ajustement sélectionnés.
        /// </summary>
        private List<Pair<string, CorporateEventProcedure>> ListTemplatesAttached
        {
            get
            {
                List<Pair<string, CorporateEventProcedure>> lstTemplates = new List<Pair<string, CorporateEventProcedure>>();
                if (null != m_DicTemplates)
                {
                    Nullable<AdjustmentMethodOfDerivContractEnum> adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue);
                    if (adjMethod.HasValue)
                    {
                        string _keyType = KeyCEType;
                        if (m_DicTemplates.ContainsKey(adjMethod.Value))
                        {
                            if (m_DicTemplates[adjMethod.Value].ContainsKey(_keyType))
                                lstTemplates.AddRange(m_DicTemplates[adjMethod.Value][_keyType]);

                            if (m_DicTemplates[adjMethod.Value].ContainsKey("All"))
                                lstTemplates.AddRange(m_DicTemplates[adjMethod.Value]["All"]);
                        }
                    }
                }
                return lstTemplates;
            }
        }
        #endregion ListTemplatesAttached
        #region SelectedMktRoundingRules
        /// <summary>
        /// Règles d'arrondi du marché sélectionné 
        /// </summary>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        private List<Pair<AdjustmentElementEnum, Rounding>> SelectedMktRoundingRules
        {
            get
            {
                List<Pair<AdjustmentElementEnum, Rounding>> _list = null;
                MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
                if (null != _market)
                {
                    if (m_DicMarketRules.ContainsKey(_market.SpheresId))
                        _list = m_DicMarketRules[_market.SpheresId].rounding;
                    else
                        _list = CATools.DefaultMktRoundingRules();
                }
                return _list;
            }
        }
        #endregion SelectedMktRoundingRules
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20210329 [25153] Ecriture/Lecture des données en cache
        public Dictionary<int, MarketIdentification> DicMarket
        {
            get {return DataCache.GetData<Dictionary<int, MarketIdentification>>("CA_Market");}
            set {DataCache.SetData<Dictionary<int, MarketIdentification>>("CA_Market", value);}
        }
        /// EG 20210329 Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// Ecriture/Lecture des données en cache
        public Dictionary<int, MarketIdentification> DicUNLMarket
        {
            get {return DataCache.GetData<Dictionary<int, MarketIdentification>>("CA_UNLMarket");}
            set {DataCache.SetData<Dictionary<int, MarketIdentification>>("CA_UNLMarket", value);}
        }
        #endregion Accessors
        #region Constructor
        public CorporateActionIssuePage()
        {
        }
        #endregion Constructor

        #region Events

        #region CreateChildControls
        // EG 20201104 Ajout OpenUrl.js
        // EG 20210222 [XXXXX] Suppression inscription function ClosePage
        // EG 20210222 [XXXXX] Suppression OpenUrl.js (fonctions déplacées dans Referential.js)
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
        // EG 20210317 [25556] Corrections diverses GUI
        // EG 20240619 [WI945] Security : Update outdated components (New QTip2)
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));

            HtmlInputHidden hdn = new HtmlInputHidden();
            hdn.Attributes.Add("onclick", "javascript:" + ClientScript.GetPostBackEventReference(this, null));
            this.Page.Controls.Add(hdn);

            //// Override complet de PageBase (pas d'appel)
            JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
            JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);

            JQuery.UI.WriteInitialisationScripts(this, new JQuery.DatePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.DateTimePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.TimePicker());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Toggle());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.QTip2());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Dialog());
            JQuery.UI.WriteInitialisationScripts(this, new JQuery.Confirm());
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.min.js"));
        }
        #endregion CreateChildControls
        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion InitializeComponent

        #region OnInit
        /// <summary>
        /// Initialisation de la page
        /// 
        /// ● Appel via les corporate actions publiées (table CORPOACTIONISSUE)
        ///   Chaines de requête : PK = "IDCAISSUE" et PKV = sa valeur
        ///
        /// ● Appel via les corporates action intégrées (table CORPOACTION)
        ///   Chaines de requête : PK = "IDCA" et PKV = sa valeur
        /// </summary>
        /// <param name="e"></param>
        // EG 20200831 [25556] Nouvelle interface GUI v10(Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            // FI 20200217 [XXXXX] Ajout de HiddenGUID
            this.Form = frmCorporateAction;
            AntiForgeryControl();
            AddInputHiddenGUID();

            m_CAInfo = new CAInfo(SessionTools.User.IdA, Request.QueryString["PK"], Request.QueryString["PKV"]);
            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
            {
                cssClassMaster = "caissue";
                mainMenuName = "process";
            }
            else if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION)
            {
                cssClassMaster = "caembedded";
                mainMenuName = "about";
            }

            SizeHeightForAlertImmediate = new Pair<int?, int?>(0, 640);
            SizeWidthForAlertImmediate = new Pair<int?, int?>(0, 600);
            PageTools.SetBody(this, false, null);
        }
        #endregion OnInit
        #region OnLoad
        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        protected void Page_Load(object sender, EventArgs e)
        {
            // Gestion du focus suite à postback
            HookOnFocus(this.Page as Control);
            Page.ClientScript.RegisterStartupScript(typeof(CorporateActionIssuePage), "ScriptDoFocus", SCRIPT_DOFOCUS.Replace("REQUEST_LASTFOCUS", Request["__LASTFOCUS"]), true);

            #region Header
            string idMenu = Request.QueryString["IDMENU"].ToString();
            string leftTitle = Ressource.GetString(idMenu, true);
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            // EG 20130725 Timeout sur Block
            JQuery.Block block = new JQuery.Block(idMenu, "Msg_CAProcessInProgress", true)
            {
                Timeout = SystemSettings.GetTimeoutJQueryBlock("CA")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            Control head;
            if (null != this.Header)
                head = (Control)this.Header;
            else
                head = (Control)PageTools.SearchHeadControl(this);

            PageTools.SetHeaderLink(head, "linkCssCA", "~/Includes/CorporateActions.min.css");

            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu.GetIdMenu(IdMenu.Menu.Input)));
            plhHeader.Controls.Add(pnlHeader);
            #endregion Header

            /// Alimentation des tooltips associés aux DDL 
            SetTooltipInformation(ddlCEGroup, "CorporateEventGroupEnum");
            SetTooltipInformation(ddlCEType, "CorporateEventTypeEnum");
            SetTooltipInformation(ddlCEAdjMethod, "AdjustmentMethodOfDerivContractEnum");
            SetOtherTooltip();

            InitializeLinkButton();
            if (IsPostBack)
            {
                // Restauration des contrôles créés dynamiquement après Postback
                RestorePlaceHolder();
                MethodsGUI.SetEventHandler(FindControl(plhComponents).Controls);
                MethodsGUI.SetEventHandler(FindControl(plhComponents_AI).Controls);
            }
            else
            {

                // Initialisation des contrôles de la page
                // Chargement d'un CA existante (CORPOACTION ou CORPOACTIONISSUE) ou
                // Initialisation des contrôles à leurs valeurs par défaut
                InitializeData();
                if (m_CAInfo.Id.HasValue)
                    LoadCorporateAction();
                else
                    SetDefault();

            }
            ComplementaryInitialize();
            SetValidators();

            // Sauvegarde des contrôles créés dynamiquement
            SavePlaceHolder();
        }
        #endregion Page_Load
        protected override void OnPreRender(EventArgs e)
        {
            SetUploadControl();
        }
        //* --------------------------------------------------------------- *//
        // Changement de sélection sur les Dropdowns
        //* --------------------------------------------------------------- *//

        #region OnMarketChanged
        /// <summary>
        /// ● Changement de MARCHE
        ///   Mise à jour des contrôles de Règles d'arrondi
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (URLCANOTICE)
        protected void OnMarketChanged(object sender, EventArgs e)
        {
            ResetMarketRules();
            MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
            if (null != _market)
            {
                if (_market.urlCANoticeSpecified)
                    txtCAURLNotice.Text = _market.urlCANotice;

                if (m_DicMarketRules.ContainsKey(_market.SpheresId))
                {
                    Pair<AdjustmentElementEnum, Rounding> _pair;
                    foreach (AdjustmentElementEnum adjElement in Enum.GetValues(typeof(AdjustmentElementEnum)))
                    {
                        _pair = m_DicMarketRules[_market.SpheresId].rounding.Find(item => item.First == adjElement);
                        if (FindControl(CATools.pfxDDLComponent + adjElement.ToString() + "RoundingDir") is WCDropDownList2 _ddl)
                        {
                            ControlsTools.DDLSelectByValue(_ddl, _pair.Second.direction.ToString());
                            _ddl.Enabled = _pair.Second.isUpdDirection;
                        }
                        _ddl = FindControl(CATools.pfxDDLComponent + adjElement.ToString() + "RoundingPrec") as WCDropDownList2;
                        if (null != _ddl)
                        {
                            ControlsTools.DDLSelectByIndex(_ddl, _pair.Second.precision);
                            _ddl.Enabled = _pair.Second.isUpdPrecision;
                        }
                    }
                }
            }
        }
        #endregion OnMarketChanged
        #region OnCEGroupChanged
        /// <summary>
        /// ● Changement de GROUPE de Corporate Event
        ///   Mise à jour des contrôles Type de corporate action (CEType / CECombinedOperand / CECombinedType)
        ///   Chargement des templates associés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCEGroupChanged(object sender, EventArgs e)
        {
            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments("[code:CorporateEventTypeEnum;customvalue:" + ddlCEGroup.SelectedValue + ";forcedenum:RatioCertified]", false);
            loadEnum.isWithEmpty = true;
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlCEType, SessionTools.CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlCECombinedType, SessionTools.CS, loadEnum);
            ddlCECombinedOperand.Visible = IsCombination;
            ddlCECombinedType.Visible = IsCombination;
            LoadDDL_Template();
        }
        #endregion OnCEGroupChanged
        #region OnCEMethodChanged
        /// <summary>
        /// La méthode d'ajustement CHANGE
        /// Chargement des templates
        /// Lecture des composants simples et création des contrôles associés pour le template sélectionné
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCEMethodChanged(object sender, EventArgs e)
        {
            Nullable<AdjustmentMethodOfDerivContractEnum> adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue);
            bool isEnabled = true;
            if (adjMethod.HasValue)
            {
                ddlCETemplate.Enabled = isEnabled;
                btnCETemplate.Visible = isEnabled;
                btnSeeProcedure.Visible = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION) && isEnabled;
            }
            OnEnumsChanged(sender, e);
            // EG [33415/33420]
            SetDisplayCheckAdjustement();
        }
        #endregion OnCEMethodChanged
        #region OnCETypeChanged
        /// <summary>
        /// ● Changement du TYPE de corporate event 
        ///   Chargement des templates
        ///   Lecture des composants simples associés et création des contrôles WEB pour le template actif
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211020 [XXXXX] Correction gestion 2ème sous-jacent sur DEMerger, Merge et SpinOff) 
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        protected void OnCETypeChanged(object sender, EventArgs e)
        {
            Nullable<CorporateEventTypeEnum> type = CATools.CEType(ddlCEType.SelectedValue);
            bool isVisible = false;
            bool isCAEmbedded = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION);
            if (type.HasValue)
            {
                // Une 2ème saisie de Sous-jacent visible dans le cas d'une Fusion ou Scission
                isVisible = (type.Value == CorporateEventTypeEnum.DeMerger) ||
                            (type.Value == CorporateEventTypeEnum.Merger) ||
                            (type.Value == CorporateEventTypeEnum.SpinOff);
            }

            pnlUnderlyer2.Visible = isVisible;
            lblCEUNLCategory2.Visible = isVisible;
            ddlCEUNLCategory2.Visible = isVisible;
            lblCEUNLMarket2.Visible = isVisible;
            txtCEUNLMarket2.Visible = isVisible;
            lblCEUNLIdentifier2.Visible = isVisible && isCAEmbedded;
            lblCEUNLCode2.Visible = isVisible;
            txtCEUNLCode2.Visible = isVisible;

            OnEnumsChanged(sender, e);
        }
        #endregion OnCETypeChanged
        #region OnCETemplateChanged
        /// <summary>
        /// ● Changement de TEMPLATE
        ///   Chargement de tous ses composants simples
        ///   Création des contrôles associés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCETemplateChanged(object sender, EventArgs e)
        {
            CorporateEventProcedure procedure = ActiveCEProcedure;
            if (null != procedure)
            {
                if (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update)
                {
                    m_CorporateAction.corporateEvent[0].procedure.adjustment.templateFileName = procedure.adjustment.templateFileName;
                    m_CorporateAction.corporateEvent[0].procedure.adjustment.templateFileName_AI = procedure.adjustment.templateFileName_AI;
                    m_CorporateAction.corporateEvent[0].procedure.adjustment.templateFileName_CT = procedure.adjustment.templateFileName_CT;
                }
                m_ListComponentSimples = CATools.AllComponentSimples(procedure);
                CreateControlComponent_ADJ();
            }
        }
        #endregion OnCETemplateChanged

        #region OnCETemplate_AIChanged
        /// <summary>
        /// ● Changement de TEMPLATE Additional info
        ///   Chargement de tous ses composants simples
        ///   Création des contrôles associés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCETemplate_AIChanged(object sender, EventArgs e)
        {
            AdditionalInfos ai = ActiveCEAdditionalInfo;
            if (null != ai)
            {
                if (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update)
                {
                    if (null != m_CorporateAction.corporateEvent[0].procedure.adjustment)
                    {
                        m_CorporateAction.corporateEvent[0].procedure.adjustment.additionalInfoSpecified = (null != ai);
                        if (m_CorporateAction.corporateEvent[0].procedure.adjustment.additionalInfoSpecified)
                        {
                            m_CorporateAction.corporateEvent[0].procedure.adjustment.templateFileName_AI = ai.templateFileName;
                        }
                    }
                }
                m_ListComponentSimples_AI = CATools.AllComponentSimples(ai.additionalInfo);
                CreateControlComponent_AI();
            }
        }
        #endregion OnCETemplate_AIChanged
        #region OnCACfiCodeCategoryChanged
        /// <summary>
        /// ● Changement de catégorie (ALL / FUTURE / OPTION)
        ///   La CA s'applique à TOUS LES CONTRATS / LES CONTRATS FUTURES / LES CONTRATS OPTIONS
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCACfiCodeCategoryChanged(object sender, EventArgs e)
        {
            SetDisplayCheckAdjustement();
        }
        #endregion OnCACfiCodeCategoryChanged

        #region OnEnumsChanged
        /// <summary>
        /// ● Modification d'une valeur de DropDown
        ///   Chargement des templates
        ///   Lectures des composants simples
        ///   Création des contrôles associés
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnEnumsChanged(object sender, EventArgs e)
        {
            LoadDDL_Template();

            bool isLoadRequested = true;
            if (e is LoadTemplateEventArgs args)
                isLoadRequested = args.isLoadRequested;

            if (isLoadRequested)
            {
                CorporateEventProcedure procedure = ActiveCEProcedure;
                if (null != procedure)
                {
                    m_ListComponentSimples = CATools.AllComponentSimples(procedure);
                    CreateControlComponent_ADJ();
                    if (procedure.adjustment.additionalInfoSpecified && (-1 < ddlCETemplate_AI.SelectedIndex) && (0 == m_ListComponentSimples_AI.Count))
                    {
                        m_ListComponentSimples_AI = CATools.AllComponentSimples(procedure.adjustment.additionalInfo);
                        CreateControlComponent_AI();
                    }
                }
            }
        }
        #endregion OnEnumsChanged

        //* --------------------------------------------------------------- *//
        // Notices additionnelles
        //* --------------------------------------------------------------- *//

        #region OnAddDelRefNotice
        /// <summary>
        /// Ajout/Suppression d'une notice additionnelle
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        protected void OnAddDelRefNotice(object sender, CommandEventArgs e)
        {
            if (System.Enum.IsDefined(typeof(Cst.OperatorType), e.CommandName))
            {
                //WCTogglePanel togglePanel = null;
                Cst.OperatorType operatorType = (Cst.OperatorType)System.Enum.Parse(typeof(Cst.OperatorType), e.CommandName, true);
                switch (operatorType)
                {
                    case Cst.OperatorType.add:
                        if (0 == plhCARefNoticeAdds.Controls.Count)
                            CreateRefNoticeAddContainer();
                        else
                        {
                            if (plhCARefNoticeAdds.Controls[0] is WCTogglePanel pnlAdd)
                            {
                                if (pnlAdd.ControlBody is Panel pnlBodyAdd)
                                    CreateRefNoticeAdd(pnlAdd, pnlBodyAdd.Controls[0].Controls.Count + 1);
                                SavePlaceHolder();
                            }
                        }
                        btnDelRefNotice.Visible = true;
                        break;
                    case Cst.OperatorType.substract:
                        if (plhCARefNoticeAdds.Controls[0] is WCTogglePanel pnlDel)
                        {
                            if (pnlDel.ControlBody is Panel pnlBodyDel)
                            {
                                int nbRow = pnlBodyDel.Controls[0].Controls.Count;
                                if (1 == nbRow)
                                {
                                    plhCARefNoticeAdds.Controls.Clear();
                                    btnDelRefNotice.Visible = false;
                                }
                                else
                                {
                                    if (pnlBodyDel.Controls[0].Controls[nbRow - 1] is Panel pnl)
                                        pnlBodyDel.Controls[0].Controls.Remove(pnl);
                                }
                                SavePlaceHolder();

                            }
                        }
                        break;
                }
            }
        }
        #endregion OnAddDelRefNotice

        #endregion Events

        #region Methods
        #region SetDisplayCheckAdjustement
        /// <summary>
        /// Gestion Affichage des CHECKs des éléments d'ajustement
        /// </summary>
        // EG [33415/33420] New
        // EG 20141106 [20253] Equalization payment
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void SetDisplayCheckAdjustement()
        {
            bool isFutureVisible = true;
            bool isOptionVisible = true;
            bool isEqualPaymentFutureAuthorized = false;
            bool isEqualPaymentOptionAuthorized = false;
            Nullable<AdjustmentMethodOfDerivContractEnum> adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue);
            if (adjMethod.HasValue)
            {
                Nullable<CfiCodeCategoryEnum> cfiCodeCategory = CATools.CACfiCodeCategory(ddlCACfiCodeCategory.SelectedValue);

                if (cfiCodeCategory.HasValue)
                {
                    isFutureVisible = (CfiCodeCategoryEnum.Future == cfiCodeCategory.Value);
                    isOptionVisible = (CfiCodeCategoryEnum.Option == cfiCodeCategory.Value);
                }
                //isAdjustment = (adjMethod.Value != AdjustmentMethodOfDerivContractEnum.None);
                bool isAdjustment = (adjMethod.Value == AdjustmentMethodOfDerivContractEnum.Ratio);


                MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
                if (null != _market)
                {
                    if (m_DicMarketRules.ContainsKey(_market.SpheresId))
                    {
                        isEqualPaymentFutureAuthorized = m_DicMarketRules[_market.SpheresId].isEqualPaymentFutureAuthorized;
                        isEqualPaymentOptionAuthorized = m_DicMarketRules[_market.SpheresId].isEqualPaymentOptionAuthorized;
                    }
                }

                lblCEAdjFuture.Visible = isFutureVisible;
                pnlAdjFuture.Visible = isFutureVisible;
                SetDisplayCheckAdjustement("chkCEAdjFutureCSize", isAdjustment, isFutureVisible);
                SetDisplayCheckAdjustement("chkCEAdjFuturePrice", isAdjustment, isFutureVisible);
                SetDisplayCheckAdjustement("chkCEAdjFutureEqualisationPayment", isAdjustment && isEqualPaymentFutureAuthorized, isFutureVisible);


                lblCEAdjOption.Visible = isOptionVisible;
                pnlAdjOption.Visible = isOptionVisible;
                SetDisplayCheckAdjustement("chkCEAdjOptionCSize", isAdjustment, isOptionVisible);
                SetDisplayCheckAdjustement("chkCEAdjOptionPrice", isAdjustment, isOptionVisible);
                SetDisplayCheckAdjustement("chkCEAdjOptionStrikePrice", isAdjustment, isOptionVisible);
                SetDisplayCheckAdjustement("chkCEAdjOptionEqualisationPayment", isAdjustment && isEqualPaymentOptionAuthorized, isOptionVisible);
            }
        }
        private void SetDisplayCheckAdjustement(string pCheckId, bool pIsAdjustment, bool pIsVisible)
        {
            if (FindControl(pCheckId) is WCCheckBox2 chkBox)
            {
                chkBox.Visible = pIsVisible;
                if ((chkBox.Checked && (false == pIsAdjustment)) || (false == pIsVisible))
                    chkBox.Checked = false;
                chkBox.Enabled = pIsAdjustment;
            }
        }
        #endregion SetDisplayCheckAdjustement

        //* --------------------------------------------------------------- *//
        // Initialisation des données 
        //* --------------------------------------------------------------- *//

        #region ApplyRounding
        /// <summary>
        /// Application des arrondis sur le R-Factor certifé
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ApplyRoundingRFactorCertified(object sender, EventArgs e)
        {
            if ((FindControl(CATools.pfxTXTComponent + CATools.RFactorCertified_Id) is WCTextBox2 txtRFactorCertified) && StrFunc.IsFilled(txtRFactorCertified.Text))
            {
                RFactor rFactor = new RFactor();
                SetRounding(rFactor, AdjustmentElementEnum.RFactor);
                if (false == rFactor.roundingSpecified)
                {
                    List<Pair<AdjustmentElementEnum, Rounding>> _roundingRules = SelectedMktRoundingRules;
                    if (null != _roundingRules)
                        rFactor.rounding = _roundingRules.Find(item => item.First == AdjustmentElementEnum.RFactor).Second;
                    rFactor.roundingSpecified = (null != rFactor.rounding);
                }
                if (rFactor.roundingSpecified)
                {
                    EFS_Decimal amount = new EFS_Decimal(DecFunc.DecValue(txtRFactorCertified.Text, Thread.CurrentThread.CurrentCulture));
                    EFS_Round round = new EFS_Round(rFactor.rounding.direction, rFactor.rounding.precision, amount.DecValue);
                    txtRFactorCertified.Text = StrFunc.FmtDecimalToCurrentCulture(round.AmountRounded.ToString(NumberFormatInfo.InvariantInfo));
                    SavePlaceHolder();
                }
            }
        }
        #endregion ApplyRounding
        #region ComplementaryInitialize
        /// <summary>
        /// Initialisation complémentaire
        /// </summary>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211020 [XXXXX] Correction gestion 2ème sous-jacent sur DEMerger, Merge et SpinOff) 
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void ComplementaryInitialize()
        {
            bool isUpdate = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            bool isCAIssue = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE);
            bool isCAEmbedded = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION);
            bool isCombination = IsCombination;
            bool isExistContract = isCAEmbedded && IsExistContractAsset(Cst.OTCml_TBL.CORPOEVENTCONTRACT);
            bool isExistAsset = isCAEmbedded && IsExistContractAsset(Cst.OTCml_TBL.CORPOEVENTASSET);

            lblCAEmbeddedState.Visible = isCAIssue;
            if (lblCAEmbeddedState.Visible)
            {
                if (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update)
                    lblCAEmbeddedState.Text = m_CorporateAction.embeddedState.ToString();
                else
                    lblCAEmbeddedState.Text = CorporateActionEmbeddedStateEnum.NEWEST.ToString();
                lblCAEmbeddedState.CssClass = lblCAEmbeddedState.Text;
            }

            lblIDCA.Visible = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            if (lblIDCA.Visible)
            {
                lblIDCA.Text = "Id: " + m_CorporateAction.spheresid;
                lblIDCA.CssClass = cssClassMaster;
            }

            SetReadOnly(txtCAMarket, roCAMarket);

            SetReadOnly(ddlCEGroup, roCEGroup);
            ttipCEGroup.Visible = isCAIssue;

            SetReadOnly(ddlCEType, roCEType);
            ttipCEType.Visible = isCAIssue;

            ddlCECombinedOperand.Visible = isCombination && isCAIssue;
            ddlCECombinedType.Visible = isCombination && isCAIssue;
            if (isCombination)
                roCEType.Text += "-" + ddlCECombinedOperand.SelectedItem.Text + "-" + ddlCECombinedType.SelectedItem.Text;

            lblIDCE.Visible = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            if (lblIDCE.Visible)
            {
                lblIDCE.Text = "Id: " + m_CorporateAction.spheresid;
                lblIDCE.CssClass = cssClassMaster;
            }

            SetReadOnly(txtCAIdentifier, roCAIdentifier);
            SetReadOnly(txtCARefNotice, roCARefNotice);

            SetReadOnly(txtCEIdentifier, roCEIdentifier);
            SetReadOnly(ddlCEUNLCategory, roCEUNLCategory);
            SetReadOnly(txtCEUNLMarket, roCEUNLMarket);
            SetReadOnly(txtCEUNLCode, roCEUNLCode);
            txtCEUNLIdentifier.Visible = false;
            roCEUNLIdentifier.Text = txtCEUNLIdentifier.Text;
            roCEUNLIdentifier.Visible = isCAEmbedded;
            pnlUnderlyer1.Visible = isCAEmbedded;


            Nullable<CorporateEventTypeEnum> type = CATools.CEType(ddlCEType.SelectedValue);
            bool isVisible = false;
            if (type.HasValue)
            {
                isVisible = (type.Value == CorporateEventTypeEnum.DeMerger) ||
                            (type.Value == CorporateEventTypeEnum.Merger) ||
                            (type.Value == CorporateEventTypeEnum.SpinOff);
            }

            pnlUnderlyer2.Visible = isVisible;
            SetReadOnly(ddlCEUNLCategory2, roCEUNLCategory2,isVisible);
            SetReadOnly(txtCEUNLMarket2, roCEUNLMarket2, isVisible);
            SetReadOnly(txtCEUNLCode2, roCEUNLCode2, isVisible);

            txtCEUNLIdentifier2.Visible = false;
            roCEUNLIdentifier2.Text = txtCEUNLIdentifier2.Text;
            roCEUNLIdentifier2.Visible = isVisible && isCAEmbedded;

            if (false == isCAIssue)
            {
                lblCEUNLCode.Text = Ressource.GetString("lblCEUNLIsinCode");
                lblCEUNLCode2.Text = Ressource.GetString("lblCEUNLIsinCode2");
            }

            SetReadOnly(ddlCEAdjMethod, roCEAdjMethod);
            ttipCEAdjMethod.Visible = isCAIssue;
            SetReadOnly(ddlCETemplate, roCETemplate);
            SetReadOnly(ddlCETemplate_AI, roCETemplate_AI);


            btnRefresh.Visible = isUpdate;
            btnSend.Visible = isCAIssue;
            btnDuplicate.Visible = isCAIssue && isUpdate;
            btnRemove.Visible = isCAIssue;
            btnSeeMsg.Visible = isCAIssue;
            btnSeeProcedure.Visible = isCAEmbedded;
            btnSeeContractResult.Visible = isExistContract;
            btnSeeAssetResult.Visible = isExistAsset;
            btnSeeFinalResult.Visible = isExistContract || isExistAsset;
            if (isCAEmbedded)
            {
                string url = String.Format("List.aspx?Repository=CORPOEVENTCONTRACT&FK={0}&IDMENU=OTC_REF_DATA_CORPOACTIONS_CORPOEVENTCONTRACT&InputMode=5", m_CorporateAction.corporateEvent[0].IdCE);
                btnSeeContractResult.Attributes.Add("onclick", JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_ListReferential));
                url = String.Format("List.aspx?Repository=CORPOEVENTASSET2&FK={0}&IDMENU=OTC_REF_DATA_CORPOACTIONS_CORPOEVENTASSET&InputMode=5", m_CorporateAction.corporateEvent[0].IdCE);
                btnSeeAssetResult.Attributes.Add("onclick", JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_ListReferential));
            }


            Nullable<AdjustmentMethodOfDerivContractEnum> adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue);

            bool isBtnRatio = isCAEmbedded && adjMethod.HasValue && (adjMethod.Value == AdjustmentMethodOfDerivContractEnum.Ratio);
            isBtnRatio &= StrFunc.IsFilled(txtCEEffectiveDate.Text);
            btnTestRatio.Visible = isBtnRatio;
            // EG 20140518 [19913]
            txtMSSQL_File_C.CssClass = EFSCssClass.CaptureOptional;
            txtMSSQL_File_C.Enabled = isCAIssue;
            btnMSSQL_Upload_C.Visible = isCAIssue;
            btnMSSQL_Delete_C.Visible = isCAIssue;

            txtMSSQL_File_E.CssClass = EFSCssClass.CaptureOptional;
            txtMSSQL_File_E.Enabled = isCAIssue;
            btnMSSQL_Upload_E.Visible = isCAIssue;
            btnMSSQL_Delete_E.Visible = isCAIssue;

            txtMSSQL_File_P.CssClass = EFSCssClass.CaptureOptional;
            txtMSSQL_File_F.CssClass = EFSCssClass.CaptureOptional;

            txtORA_File_C.CssClass = EFSCssClass.CaptureOptional;
            txtORA_File_C.Enabled = isCAIssue;
            btnORA_Upload_C.Visible = isCAIssue;
            btnORA_Delete_C.Visible = isCAIssue;

            txtORA_File_E.CssClass = EFSCssClass.CaptureOptional;
            txtORA_File_E.Enabled = isCAIssue;
            btnORA_Upload_E.Visible = isCAIssue;
            btnORA_Delete_E.Visible = isCAIssue;

            txtORA_File_P.CssClass = EFSCssClass.CaptureOptional;
            txtORA_File_F.CssClass = EFSCssClass.CaptureOptional;

        }
        #endregion ComplementaryInitialize
        #region SetReadOnly
        private void SetReadOnly(WebControl pControl, WCTooltipLabel pROControl)
        {
            SetReadOnly(pControl, pROControl, true);
        }
        private void SetReadOnly(WebControl pControl, WCTooltipLabel pROControl, bool pIsVisible)
        {
            bool isCAIssue = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE);
            bool isCAEmbedded = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION);

            pControl.Visible = isCAIssue && pIsVisible;
            pROControl.Visible = isCAEmbedded && pIsVisible;
            if (pControl is TextBox)
                pROControl.Text = (pControl as TextBox).Text;
            else if (pControl is DropDownList)
            {
                if (null != (pControl as DropDownList).SelectedItem)
                    pROControl.Text = (pControl as DropDownList).SelectedItem.Text;
            }
        }
        #endregion SetReadOnly
        #region CreateControlComponentCommon
        /// <summary>
        /// ● Création des contrôles pour les composants SIMPLES et ADDITIONNELS
        ///   Un controle pour la saisie d'une donnée numérique, string ou check
        ///   Un contrôle pour la saisie de la devise (si besoin)
        /// ● Création d'un contrôle pour le R-Factor certified (saisie optionnelle)
        ///   Un controle pour la saisie d'une donnée numérique
        /// </summary>
        /// EG 20140317 [19722]
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void CreateControlComponentCommon(PlaceHolder pPlaceHolder, List<ComponentSimple> pListComponentSimples, bool pIsAddRatioCertified)
        {
            pPlaceHolder.Controls.Clear();
            Table tblComponents = new Table
            {
                Width = Unit.Percentage(100)
            };
            TableRow row = null;
            TableCell cell = new TableCell();
            WCTooltipLabel lblComponent = null;
            WCTextBox2 txtComponent = null;
            WCDropDownList2 ddlComponent = null;
            WCCheckBox2 chkComponent = null;
            #region Composants simples
            if (0 < pListComponentSimples.Count)
            {
                pListComponentSimples.ToList().ForEach(item =>
                {
                    row = new TableRow();
                    cell = new TableCell();

                    lblComponent = new WCTooltipLabel
                    {
                        ID = CATools.pfxLBLComponent + item.Id,
                        Text = Ressource.GetString(CATools.pfxLBLComponent + item.Id, item.descriptionSpecified ? item.description : item.Id),
                        CssClass = "component"
                    };
                    lblComponent.Pty.TooltipTitle = lblComponent.Text;
                    lblComponent.Pty.TooltipContent = Ressource.GetString2(CATools.pfxLBLComponent + "TtipComponent", item.name, item.Id);
                    if (item.noticeSpecified)
                        lblComponent.Pty.TooltipContent += Cst.HTMLBreakLine + item.notice.Replace(Cst.CrLf, Cst.HTMLBreakLine);
                    else if (item.descriptionSpecified)
                        lblComponent.Pty.TooltipContent += Cst.HTMLBreakLine + item.description;
                    cell.Controls.Add(lblComponent);
                    lblComponent.Pty.SetButton("btnclose");
                    row.Cells.Add(cell);

                    cell = new TableCell
                    {
                        Width = Unit.Pixel(50),
                        Text = Cst.HTMLSpace
                    };
                    if ((item.resultSpecified && item.result.result is Money) || (item.resulttype == ResultType.amount))
                    {
                        cell = new TableCell
                        {
                            HorizontalAlign = HorizontalAlign.Right
                        };
                        ddlComponent = new WCDropDownList2
                        {
                            Width = Unit.Pixel(50),
                            ID = CATools.pfxDDLComponent + item.Id
                        };
                        ControlsTools.DDLLoad_Currency(SessionTools.CS, ddlComponent);
                        cell.Controls.Add(ddlComponent);
                    }
                    row.Cells.Add(cell);
                    cell = new TableCell
                    {
                        Width = Unit.Percentage(100),
                        HorizontalAlign = HorizontalAlign.Left
                    };

                    if ((item.resultSpecified && item.result.result is bool) || (item.resulttype == ResultType.check))
                    {
                        chkComponent = new WCCheckBox2
                        {
                            ID = CATools.pfxCHKComponent + item.Id
                        };
                        cell.Controls.Add(chkComponent);
                    }
                    else
                    {
                        txtComponent = new WCTextBox2(CATools.pfxTXTComponent + item.Id)
                        {
                            Width = Unit.Pixel(150)
                        };
                        // EG 20140317 [19722]
                        EFSRegex.TypeRegex regex = EFSRegex.TypeRegex.RegexDecimalExtend;
                        string cssClass = (item.required ? EFSCssClass.CaptureNumeric : EFSCssClass.CaptureNumericOptional);
                        if ((item.resultSpecified && item.result.result is string) || (item.resulttype == ResultType.@info))
                        {
                            regex = EFSRegex.TypeRegex.RegexString;
                            cssClass = (item.required ? EFSCssClass.Capture : EFSCssClass.CaptureOptional);
                            txtComponent.Width = Unit.Percentage(100);
                        }
                        if (item.Id == CATools.RFactorCertified_Id)
                        {
                            txtComponent.AutoPostBack = true;
                            txtComponent.TextChanged += new System.EventHandler(ApplyRoundingRFactorCertified);
                        }
                        SetValidators(txtComponent, regex, item.required, cssClass, CATools.ValidationGroupEnum.CE, lblComponent.Text);

                        cell.Controls.Add(txtComponent);
                    }

                    row.Cells.Add(cell);
                    tblComponents.Rows.Add(row);

                });

                #region RFactorCertified
                if (pIsAddRatioCertified)
                {
                    if (false == m_ListComponentSimples.Exists(item2 => item2.Id == CATools.RFactorCertified_Id))
                    {
                        CorporateEventProcedure procedure = ActiveCEProcedure;
                        if ((null != procedure) && (procedure.adjustment is AdjustmentRatio))
                        {
                            row = new TableRow();
                            cell = new TableCell();
                            lblComponent = new WCTooltipLabel
                            {
                                ID = CATools.pfxLBLComponent + CATools.RFactorCertified_Id,
                                Text = Ressource.GetString(CATools.pfxLBLComponent + CATools.RFactorCertified_Id, true),
                                Width = Unit.Pixel(200)
                            };
                            lblComponent.Pty.TooltipTitle = lblComponent.Text;
                            lblComponent.Pty.TooltipContent = Ressource.GetString2(CATools.pfxLBLComponent + "TtipComponent", CATools.RFactorCertified_Name, CATools.RFactorCertified_Id);
                            lblComponent.Pty.TooltipContent += Cst.HTMLBreakLine + CATools.RFactorCertified_Description;
                            lblComponent.Pty.TooltipContent = CATools.RFactorCertified_Description;
                            lblComponent.Font.Bold = true;
                            cell.Controls.Add(lblComponent);
                            row.Cells.Add(cell);

                            cell = new TableCell
                            {
                                Width = Unit.Pixel(50)
                            };
                            row.Cells.Add(cell);

                            cell = new TableCell
                            {
                                Width = Unit.Pixel(150)
                            };
                            txtComponent = new WCTextBox2(CATools.pfxTXTComponent + CATools.RFactorCertified_Id)
                            {
                                AutoPostBack = true,
                                Width = Unit.Pixel(150)
                            };
                            txtComponent.Font.Bold = true;
                            txtComponent.TextChanged += new System.EventHandler(ApplyRoundingRFactorCertified);
                            SetValidators(txtComponent, EFSRegex.TypeRegex.RegexDecimalExtend, false, EFSCssClass.CaptureNumericOptional, CATools.ValidationGroupEnum.CE);
                            cell.Controls.Add(txtComponent);
                            row.Cells.Add(cell);
                            tblComponents.Rows.Add(row);
                        }
                    }
                }
                #endregion RFactorCertified


                if (tblComponents.HasControls())
                    pPlaceHolder.Controls.Add(tblComponents);
                SavePlaceHolder();
            }
            #endregion Composants simples
        }
        #endregion CreateControlComponentCommon
        #region CreateControlComponent_ADJ
        /// <summary>
        /// ● Création des contrôles pour les composants SIMPLES
        ///   Un controle pour la saisie d'une donnée numérique
        ///   Un contrôle pour la saisie de la devise (si besoin)
        /// ● Création d'un contrôle pour le R-Factor certified (saisie optionnelle)
        ///   Un controle pour la saisie d'une donnée numérique
        /// </summary>
        /// EG 20140317 [19722]
        private void CreateControlComponent_ADJ()
        {
            CreateControlComponentCommon(plhCEComponents, m_ListComponentSimples, true);
        }
        #endregion CreateControlComponent_ADJ
        #region CreateControlComponent_AI
        private void CreateControlComponent_AI()
        {
            CreateControlComponentCommon(plhCEComponents_AI, m_ListComponentSimples_AI, false);
        }
        #endregion CreateControlComponent_AI

        #region CreateRefNoticeAdd
        /// <summary>
        /// ● Création des contrôles pour les références de notices complémentaires.
        ///   Un contrôle pour la référence
        ///   Un contrôle par sa date de publication
        ///   Un contrôle pour le nom de fichier
        /// </summary>
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void CreateRefNoticeAdd(WCTogglePanel pTooglePanel, int pIndex)
        {
            Panel pnl = new Panel();
            WCTooltipLabel lblRN_Reference = new WCTooltipLabel
            {
                ID = CATools.pfxLBLNoticeAdd + "Reference" + pIndex.ToString(),
                Text = Ressource.GetString(CATools.pfxLBLNoticeAdd + "Reference"),
                Width = Unit.Pixel(100)
            };
            pnl.Controls.Add(lblRN_Reference);

            WCTextBox2 txtRN_Reference = new WCTextBox2(CATools.pfxTXTNoticeAdd + "Reference" + pIndex.ToString(), false, true, false);
            SetValidators(txtRN_Reference, EFSRegex.TypeRegex.None, true, EFSCssClass.Capture, CATools.ValidationGroupEnum.CA, lblRN_Reference.Text);
            txtRN_Reference.Width = Unit.Pixel(110);
            CustomValidator cvRN_Reference = new CustomValidator
            {
                ID = txtRN_Reference.ID + "Validator",
                ControlToValidate = txtRN_Reference.ID,
                ValidationGroup = CATools.ValidationGroupEnum.CA.ToString(),
                Display = ValidatorDisplay.None,
                ErrorMessage = "*"
            };
            cvRN_Reference.ServerValidate += new ServerValidateEventHandler(GlobalValidate);
            txtRN_Reference.Controls.Add(cvRN_Reference);
            pnl.Controls.Add(txtRN_Reference);

            WCTextBox2 txtRN_PubDate = new WCTextBox2(CATools.pfxTXTNoticeAdd + "PubDate" + pIndex.ToString());
            SetValidators(txtRN_PubDate, EFSRegex.TypeRegex.RegexDate, true, "DtPicker " + txtRN_PubDate.CssClass.Replace("DtPicker", string.Empty), CATools.ValidationGroupEnum.CA, lblRN_Reference.Text);
            txtRN_PubDate.Width = Unit.Pixel(60);
            pnl.Controls.Add(txtRN_PubDate);

            WCTextBox2 txtRN_FileName = new WCTextBox2(CATools.pfxTXTNoticeAdd + "FileName" + pIndex.ToString());
            SetValidators(txtRN_FileName, EFSRegex.TypeRegex.None, true, EFSCssClass.Capture, CATools.ValidationGroupEnum.CA, lblRN_Reference.Text);
            pnl.Controls.Add(txtRN_FileName);

            pTooglePanel.AddContent(pnl);
        }
        #endregion CreateRefNoticeAdd
        #region CreateRefNoticeAddContainer
        /// <summary>
        /// Création des contrôles pour les références de notices additionnelles
        /// </summary>
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void CreateRefNoticeAddContainer()
        {
            plhCARefNoticeAdds.Controls.Clear();
            WCTogglePanel pnl = new WCTogglePanel(Color.Transparent, Ressource.GetString("lblAdditionalNotices"), "size4", true)
            {
                ID = "pnlAdditionalNotices",
                CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster
            };
            int i = 1;
            if ((null != m_CorporateAction) && m_CorporateAction.refNoticeAddSpecified)
            {
                foreach (RefNoticeIdentification notice in m_CorporateAction.refNoticeAdd)
                {
                    CreateRefNoticeAdd(pnl,i);
                    i++;
                }
            }
            else
            {
                CreateRefNoticeAdd(pnl,i);
            }
            plhCARefNoticeAdds.Controls.Add(pnl);
            SavePlaceHolder();
        }
        #endregion CreateRefNoticeAddContainer

        #region GetUpLoadKey
        // EG 20140518 [19913]
        private Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> GetUpLoadKey(WebControl pControl)
        {
            Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = null;

            Nullable<CATools.DOCTypeEnum> _docType = null;
            Nullable<CATools.SQLRunTimeEnum> _runTime = null;
            if (pControl.ID.Contains(CATools.DOCTypeEnum.MSSQL.ToString()))
                _docType = CATools.DOCTypeEnum.MSSQL;
            else if (pControl.ID.Contains(CATools.DOCTypeEnum.ORA.ToString()))
                _docType = CATools.DOCTypeEnum.ORA;

            if (pControl.ID.EndsWith("_C"))
                _runTime = CATools.SQLRunTimeEnum.CONTROL;
            else if (pControl.ID.EndsWith("_E"))
                _runTime = CATools.SQLRunTimeEnum.EMBEDDED;
            else if (pControl.ID.EndsWith("_P"))
                _runTime = CATools.SQLRunTimeEnum.PRECEDING;
            else if (pControl.ID.EndsWith("_F"))
                _runTime = CATools.SQLRunTimeEnum.FOLLOWING;

            if (_docType.HasValue && _runTime.HasValue)
                _key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(_docType.Value,_runTime.Value);
            return _key;
        }
        #endregion InitializeFileUpload

        // EG 20211108 [XXXXX] Gestion ATTACHEDDOC et NOTEPAD sur toutes les CA (publiées ou intégrées)
        private void InitializeLinkButton()
        {
            // Boutons
            btnRefresh.Text = "<i class='fas fa-sync-alt'></i>";
            btnRecord.Text = "<i class='fa fa-save'></i> " + Ressource.GetString("btnRecord");
            btnCancel.Text = "<i class='fa fa-times'></i> " + Ressource.GetString("btnCancel");
            btnRemove.Text = "<i class='fa fa-trash-alt'></i> " + Ressource.GetString("btnRemove");
            btnDuplicate.Text = "<i class='fa fa-copy'></i> " + Ressource.GetString("btnDuplicate");
            btnSend.Text = "<i class='fa fa-caret-square-right'></i> " + Ressource.GetString(btnSend.ID);
            btnNotepad.Text = "<i class='fas fa-file-alt'></i> ";
            btnAttachedNotice.Text = "<i class='fa fa-paperclip'></i> ";

            btnTestRatio.Text = "<i class='fas fa-calculator'></i> " + Ressource.GetString("btnTestRatio");
            btnSeeAssetResult.Text = "<i class='fas fa-external-link-alt'></i> Asset";
            btnSeeContractResult.Text = "<i class='fas fa-external-link-alt'></i> Contract";
            btnSeeFinalResult.Text = "<i class='fas fa-external-link-alt'></i> Results";
            btnSeeProcedure.Text = "<i class='fas fa-external-link-alt'></i> Procedures";
            btnSeeMsg.Text = "<i class='fas fa-external-link-alt'></i> Message";

            btnAddRefNotice.Text = "<i class='fa fa-plus-square'></i> ";

            btnDelRefNotice.Text = "<i class='fa fa-minus-square'></i> ";

            btnCETemplate.Text = "<i class='fas fa-external-link-alt'></i> XML";
            btnCETemplate_AI.Text = "<i class='fas fa-external-link-alt'></i> XML";

            string upload = "<i class='fas fa-upload'></i>";
            string open = "<i class='fas fa-search'></i>";

            btnMSSQL_Delete_C.Text = upload;
            btnMSSQL_Delete_E.Text = upload;
            btnMSSQL_Delete_F.Text = upload;
            btnMSSQL_Delete_P.Text = upload;

            btnMSSQL_Open_C.Text = open;
            btnMSSQL_Open_E.Text = open;
            btnMSSQL_Open_F.Text = open;
            btnMSSQL_Open_P.Text = open;

            btnMSSQL_Upload_C.Text = upload;
            btnMSSQL_Upload_E.Text = upload;
            btnMSSQL_Upload_F.Text = upload;
            btnMSSQL_Upload_P.Text = upload;

            btnORA_Delete_C.Text = upload;
            btnORA_Delete_E.Text = upload;
            btnORA_Delete_F.Text = upload;
            btnORA_Delete_P.Text = upload;

            btnORA_Open_C.Text = open;
            btnORA_Open_E.Text = open;
            btnORA_Open_F.Text = open;
            btnORA_Open_P.Text = open;

            btnORA_Upload_C.Text = upload;
            btnORA_Upload_E.Text = upload;
            btnORA_Upload_F.Text = upload;
            btnORA_Upload_P.Text = upload;

        }
        #region InitializeData
        /// <summary>
        /// Initialisation Générale des données
        /// ● Chargement des dictionnaires (Templates, Marchés et ses règles d'arrondi)
        /// ● Dropdowns et tooltips 
        /// </summary>
        // EG 20141106 [20253] Equalization payment
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        // EG 20211108 [XXXXX] Gestion ATTACHEDDOC et NOTEPAD sur toutes les CA (publiées ou intégrées)
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void InitializeData()
        {
            divbody.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster;
            bool isCAIssue = (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE);
            bool isCAEmbbeded= (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION);
            pnlCorporateAction.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster;
            chkCAUseURLNotice.Text = Ressource.GetString(chkCAUseURLNotice.ID);
            pnlCorporateEvent.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster;
            pnlNotices.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlCEDescription.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlCEAdjustment.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlCEComponents.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlCEComponents_AI.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlUnderlyers.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            pnlCERoundingRules.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);
            // EG 20140518 [19913]
            pnlSqlScripts.Attributes.Add("class", CSSMode + " " + mainMenuName + " " + cssClassMaster);

            // Chargement du dictionnaire des templates disponibles 
            LoadXMLTemplateFile();
            LoadXMLTemplateFile_Additional();
            // Chargement du dictionnaire des règles de marchés
            LoadMarketRules();
            // Caractéristiques générales des marchés dérivés
            LoadMarket();
            // Caractéristiques générales des marchés sous-jacents
            LoadUNLMarket();

            // Boutons
            btnAttachedNotice.Visible = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            btnNotepad.Visible = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            btnAddRefNotice.Visible = true;
            
            btnAddRefNotice.CommandName = Cst.OperatorType.add.ToString();
            btnDelRefNotice.Visible = (0 < plhCARefNoticeAdds.Controls.Count);
            btnDelRefNotice.CommandName = Cst.OperatorType.substract.ToString();
            txtCADocumentation.TextMode = TextBoxMode.MultiLine;
            txtCADocumentation.MaxLength = 2000;

            string readyStateEnum = "DEPRECATED|EMBEDDED|EXPIRED";
            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
                readyStateEnum += "|PUBLISHED|RESERVED";

            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments("[code:CorporateActionReadyStateEnum;forcedenum:" + readyStateEnum + "]", false);
            ControlsTools.DDLLoad_ENUM(ddlCAReadyState, SessionTools.CS, loadEnum);

            // Catégorie de contrat  (TOUS / FUTURE / OPTION)
            ControlsTools.DDLLoad_ENUM(ddlCACfiCodeCategory, SessionTools.CS, true, "CfiCodeCategoryEnum");
            ddlCACfiCodeCategory.CssClass = EFSCssClass.DropDownListCapture;

            // Evénements
            loadEnum = LoadEnumArguments.GetArguments("[code:CorporateEventGroupEnum;forcedenum:All]", false);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlCEGroup, SessionTools.CS, loadEnum);
            //ControlsTools.DDLLoad_ENUM(ddlCEGroup, CS, false, "CorporateEventGroupEnum");
            ControlsTools.DDLLoad_ENUM(ddlCECombinedOperand, SessionTools.CS, true, "CombinationOperandEnum");
            loadEnum = LoadEnumArguments.GetArguments("[code:SettlSessIDEnum;isdisplayvalueandextendvalue:true;forcedenum:SOD|ITD]", false);
            ControlsTools.DDLLoad_ENUM(ddlCEMode, SessionTools.CS, loadEnum);
            // Info Complémentaires
            LoadDDL_Template_AI();

            // Sous-jacent
            lblCEUNLIdentifier.Visible = isCAEmbbeded;
            roCEUNLIdentifier.Visible = isCAEmbbeded;

            foreach (string s in Enum.GetNames(typeof(Cst.UnderlyingAsset_ETD)))
            {
                ddlCEUNLCategory.Items.Add(new ListItem(s, s));
                ddlCEUNLCategory2.Items.Add(new ListItem(s, s));
            }
            // EG [33415/33420]
            ddlCEUNLCategory2.Items.Insert(0,new ListItem("", ""));

            chkCEAdjFutureCSize.Text = Ressource.GetString("lblCEContractCSize");
            chkCEAdjFutureCSize.TextAlign = TextAlign.Right;
            chkCEAdjFuturePrice.Text = Ressource.GetString("lblCEContractPrice");
            chkCEAdjFuturePrice.TextAlign = TextAlign.Right;
            chkCEAdjFutureEqualisationPayment.Text = Ressource.GetString("lblCEEqualisationPayment");
            chkCEAdjFutureEqualisationPayment.TextAlign = TextAlign.Right;
            chkCEAdjFutureEqualisationPayment.CheckedChanged += new EventHandler(OnCheckedChanged_EqualisationPayment);

            chkCEAdjOptionCSize.Text = Ressource.GetString("lblCEContractCSize");
            chkCEAdjOptionCSize.TextAlign = TextAlign.Right;
            chkCEAdjOptionStrikePrice.Text = Ressource.GetString("lblCEContractStrikePrice");
            chkCEAdjOptionStrikePrice.TextAlign = TextAlign.Right;
            chkCEAdjOptionPrice.Text = Ressource.GetString("lblCEContractPrice");
            chkCEAdjOptionPrice.TextAlign = TextAlign.Right;
            chkCEAdjOptionEqualisationPayment.Text = Ressource.GetString("lblCEEqualisationPayment");
            chkCEAdjOptionEqualisationPayment.TextAlign = TextAlign.Right;
            chkCEAdjOptionEqualisationPayment.CheckedChanged += new EventHandler(OnCheckedChanged_EqualisationPayment);


            // Méthode d'ajustement et règles d'arrondi
            ControlsTools.DDLLoad_ENUM(ddlCEAdjMethod, SessionTools.CS, false, "AdjustmentMethodOfDerivContractEnum");
            ddlCETemplate.CssClass = EFSCssClass.DropDownListCapture;
            foreach (string s in Enum.GetNames(typeof(AdjustmentElementEnum)))
            {
                if (FindControl(CATools.pfxDDLComponent + s + "RoundingDir") is WCDropDownList2 _ddl)
                    ControlsTools.DDLLoad_RoundDir(_ddl);
                _ddl = FindControl(CATools.pfxDDLComponent + s + "RoundingPrec") as WCDropDownList2;
                if (null != _ddl)
                    ControlsTools.DDLLoad_RoundPrec(_ddl);
            }
        }
        #endregion InitializeData

        #region InitializeControlSQLFileName
        // EG 20140518 [19913]
        private void InitializeControlSQLFileName(WCTextBox pTxtFileName)
        {
            Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = GetUpLoadKey(pTxtFileName);
            if (null != _key)
            {
                CorporateDoc _corporateDoc = m_CorporateDocs.GetCorporateDoc(_key);
                if (null != _corporateDoc)
                    pTxtFileName.Text = _corporateDoc.identifier;
                else
                    pTxtFileName.Text = "No script";
            }
        }
        #endregion InitializeFileUpload

        #region IsExistContractAsset
        public bool IsExistContractAsset(Cst.OTCml_TBL pTable)
        {
            bool ret = false;
            if (null != m_CorporateAction)
            {
                QueryParameters qryParameters = null;
                switch (pTable)
                {
                    case Cst.OTCml_TBL.CORPOEVENTCONTRACT:
                        DCQueryEMBEDDED dcQry = new DCQueryEMBEDDED(SessionTools.CS);
                        qryParameters = dcQry.GetQueryExist(CATools.DCWhereMode.IDCE);
                        break;
                    case Cst.OTCml_TBL.CORPOEVENTASSET:
                        ETDQueryEMBEDDED etdQry = new ETDQueryEMBEDDED(SessionTools.CS);
                        qryParameters = etdQry.GetQueryExist();
                        break;
                }
                if (null != qryParameters)
                {
                    foreach (CorporateEvent _event in m_CorporateAction.corporateEvent)
                    {
                        qryParameters.Parameters["IDCE"].Value = _event.IdCE;
                        object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                        if ((null != obj) && BoolFunc.IsTrue(obj))
                        {
                            ret = true;
                            break;
                        }
                    }
                }
            }
            return ret;
        }
        #endregion IsExistContractAsset


        #region LoadDDL_Template
        /// <summary>
        /// Chargement de la DDL des TEMPLATES avec les noms des templates pour le type et la méthode d'ajustement sélectionnés
        /// </summary>
        private void LoadDDL_Template()
        {
            ddlCETemplate.Items.Clear();
            plhCEComponents.Controls.Clear();
            List<Pair<string, CorporateEventProcedure>> templates = ListTemplatesAttached;
            if (null != templates)
                templates.ForEach(item => ddlCETemplate.Items.Add(new ListItem(item.First, item.First)));
            btnCETemplate.Visible = (null != templates);
        }
        #endregion LoadDDL_Template
        #region LoadDDL_Template
        /// <summary>
        /// Chargement de la DDL des TEMPLATES avec les noms des templates pour le type et la méthode d'ajustement sélectionnés
        /// </summary>
        private void LoadDDL_Template_AI()
        {
            ddlCETemplate_AI.Items.Clear();
            //ddlCETemplate_AI.Items.Add(new ListItem(string.Empty, Cst.DDLVALUE_NONE));
            m_DicTemplates_Additional["AI"].ForEach(item => ddlCETemplate_AI.Items.Add(new ListItem(item.First, item.First)));
            btnCETemplate_AI.Visible = (0 < ddlCETemplate_AI.Items.Count);
        }
        #endregion LoadDDL_Template_AI
        #region ResetMarketRules
        /// <summary>
        /// Réinitialisation des règles d'arrondi à leurs valeurs par défaut (Nearest + 4 décimales).
        /// </summary>
        private void ResetMarketRules()
        {
            List<Pair<AdjustmentElementEnum, Rounding>> _defaultRules = CATools.DefaultMktRoundingRules();
            WCDropDownList2 _ddl = null;
            _defaultRules.ForEach(item =>
            {
                _ddl = FindControl(CATools.pfxDDLComponent + item.First.ToString() + "RoundingDir") as WCDropDownList2;
                if (null != _ddl)
                {
                    ControlsTools.DDLSelectByValue(_ddl, item.Second.direction.ToString());
                    _ddl.Enabled = item.Second.isUpdDirection;
                }
                _ddl = FindControl(CATools.pfxDDLComponent + item.First.ToString() + "RoundingPrec") as WCDropDownList2;
                if (null != _ddl)
                {
                    ControlsTools.DDLSelectByIndex(_ddl, item.Second.precision);
                    _ddl.Enabled = item.Second.isUpdPrecision;
                }
            });
            /*
            foreach (string s in Enum.GetNames(typeof(AdjustmentElementEnum)))
            {
                WCDropDownList2 _ddl = FindControl(CATools.pfxDDLComponent + s + "RoundingDir") as WCDropDownList2;
                if (null != _ddl)
                {
                    ControlsTools.DDLSelectByValue(_ddl, Cst.RoundingDirectionSQL.N.ToString());
                    _ddl.Enabled = true;
                }
                _ddl = FindControl(CATools.pfxDDLComponent + s + "RoundingPrec") as WCDropDownList2;
                if (null != _ddl)
                {
                    ControlsTools.DDLSelectByIndex(_ddl, 4);
                    _ddl.Enabled = true;
                }
            }
            */
        }
        #endregion ResetMarketRules
        #region SetDefault
        /// <summary>
        /// Initialisation des contrôles à leurs valeurs par défaut.
        /// </summary>
        // EG 20141106 [20253] Equalization payment
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (URLCANOTICE)
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private void SetDefault()
        {
            ddlCAReadyState.Text = CorporateActionReadyStateEnum.PUBLISHED.ToString();

            ddlCEAdjMethod.Text = AdjustmentMethodOfDerivContractEnum.Ratio.ToString();
            ddlCEGroup.Text = CorporateEventGroupEnum.Distribution.ToString();
            OnCEGroupChanged(ddlCEGroup, null);
            OnCEMethodChanged(ddlCEAdjMethod, null);
            ControlsTools.DDLSelectByValue(ddlCEMode, ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.StartOfDay));

            btnDelRefNotice.Visible = false;

            DateTime dtSys = OTCmlHelper.GetDateSys(SessionTools.CS).Date;
            txtCAPubDate.Text = DtFunc.DateTimeToString(dtSys, DtFunc.FmtShortDate);
            txtCAURLNotice.Text = string.Empty;

            pnlUnderlyer2.Visible = false;
            lblCEUNLCategory2.Visible = false;
            ddlCEUNLCategory2.Visible = false;
            lblCEUNLMarket2.Visible = false;
            txtCEUNLMarket2.Visible = false;
            lblCEUNLIdentifier2.Visible = false;
            lblCEUNLCode2.Visible = false;
            txtCEUNLCode2.Visible = false;

            btnCETemplate.Visible = false;
            btnCETemplate_AI.Visible = true;
            chkCEAdjFutureCSize.Checked = true;
            chkCEAdjFuturePrice.Checked = true;
            chkCEAdjFutureEqualisationPayment.Checked = false;
            chkCEAdjOptionCSize.Checked = true;
            chkCEAdjOptionStrikePrice.Checked = true;
            chkCEAdjOptionPrice.Checked = true;
            chkCEAdjOptionEqualisationPayment.Checked = false;
            btnRemove.Enabled = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);
            // EG 20140518 [19913]
            m_CorporateDocs = new CorporateDocs();
        }
        #endregion SetDefault
        #region SetRounding
        /// <summary>
        /// ● Mise à jour des règle d'arrondi par marché sur les éléments d'ajustement
        ///   Seules les règles d'arrondis différentes de celles par défaut spécifié sur le marché
        ///   actif entraineront l'alimentation de la classe Rounding pour l'élément d'ajustement concerné
        ///   Si pAdjustedElement.roundingSpecified alors règle d'arrondi <> règle par défaut sur le marché
        /// </summary>
        /// <param name="pAdjustedElement">Classe de l'élément d'ajustement</param>
        /// <param name="pNameElement">Nom (Enum) de l'élément d'ajustement</param>
        private void SetRounding<T>(T pTarget, AdjustmentElementEnum pNameElement)
        {
            List<Pair<AdjustmentElementEnum, Rounding>> _roundingRules = SelectedMktRoundingRules;
            if ((FindControl(CATools.pfxDDLComponent + pNameElement.ToString() + "RoundingDir") is WCDropDownList2 ddlRoundingDir) && 
                (FindControl(CATools.pfxDDLComponent + pNameElement.ToString() + "RoundingPrec") is WCDropDownList2 ddlRoundingPrec))
            {
                Pair<AdjustmentElementEnum, Rounding> _rounding = null;
                if (null != _roundingRules)
                {
                    _rounding = _roundingRules.Find(item => item.First == pNameElement);

                    if (pTarget is AdjustmentElement)
                    {
                        AdjustmentElement _target = pTarget as AdjustmentElement;
                        if (null != _rounding)
                        {
                            _target.roundingSpecified =
                            ((_rounding.Second.direction.ToString() != ddlRoundingDir.SelectedValue) ||
                            (_rounding.Second.precision != ddlRoundingPrec.SelectedIndex));
                        }
                        else
                            _target.roundingSpecified = true;

                        _target.rounding = new Rounding(ddlRoundingDir.SelectedValue, false, ddlRoundingPrec.SelectedIndex, false);

                    }
                    else if (pTarget is EqualisationPayment)
                    {
                        EqualisationPayment _target = pTarget as EqualisationPayment;
                        if (null != _rounding)
                        {
                            _target.roundingSpecified =
                            ((_rounding.Second.direction.ToString() != ddlRoundingDir.SelectedValue) ||
                            (_rounding.Second.precision != ddlRoundingPrec.SelectedIndex));
                        }
                        else
                            _target.roundingSpecified = true;

                        _target.rounding = new Rounding(ddlRoundingDir.SelectedValue, false, ddlRoundingPrec.SelectedIndex, false);
                    }
                }
            }
        }
        #endregion SetRounding
        #region SetOtherTooltip
        private void SetOtherTooltip()
        {
            btnRefresh.Pty.TooltipContent = Ressource.GetString("Refresh");
            btnRecord.Pty.TooltipContent = Ressource.GetString("btn" + cssClassMaster + "Record");
            btnTestRatio.Pty.TooltipContent = Ressource.GetString("btnCATestRatio");
            btnSend.Pty.TooltipContent = Ressource.GetString("btnCASend");
            btnDuplicate.Pty.TooltipContent = Ressource.GetString("btnCADuplicate");
            
            btnCancel.Pty.TooltipContent = Ressource.GetString("btnCancel");
            btnRemove.Pty.TooltipContent = Ressource.GetString("btnRemove");
            btnSeeMsg.Pty.TooltipContent = Ressource.GetString("btnSeeCAMsg");
            btnSeeProcedure.Pty.TooltipContent = Ressource.GetString("btnSeeCAProcedure");
            btnSeeContractResult.Pty.TooltipContent = Ressource.GetString("btnSeeContract");
            btnSeeAssetResult.Pty.TooltipContent = Ressource.GetString("btnSeeAsset");
            btnSeeFinalResult.Pty.TooltipContent = Ressource.GetString("btnSeeFinalResult");
        }
        #endregion SetOtherTooltip
        #region SetUploadControl
        // EG 20140518 [19913]
        private void SetUploadControl()
        {
            SetUploadControl(CATools.DOCTypeEnum.MSSQL, CATools.SQLRunTimeEnum.CONTROL);
            SetUploadControl(CATools.DOCTypeEnum.MSSQL, CATools.SQLRunTimeEnum.EMBEDDED);
            SetUploadControl(CATools.DOCTypeEnum.MSSQL, CATools.SQLRunTimeEnum.PRECEDING );
            SetUploadControl(CATools.DOCTypeEnum.MSSQL, CATools.SQLRunTimeEnum.FOLLOWING);
            SetUploadControl(CATools.DOCTypeEnum.ORA, CATools.SQLRunTimeEnum.CONTROL);
            SetUploadControl(CATools.DOCTypeEnum.ORA, CATools.SQLRunTimeEnum.EMBEDDED);
            SetUploadControl(CATools.DOCTypeEnum.ORA, CATools.SQLRunTimeEnum.PRECEDING);
            SetUploadControl(CATools.DOCTypeEnum.ORA, CATools.SQLRunTimeEnum.FOLLOWING);
        }
        #endregion SetUploadControl
        #region SetUploadControl
        // EG 20140518 [19913]
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        private void SetUploadControl(CATools.DOCTypeEnum pDocType,CATools.SQLRunTimeEnum pRunTime)
        {
            string _runTime = ReflectionTools.ConvertEnumToString<CATools.SQLRunTimeEnum>(pRunTime);
            _ = FindControl("btn" + pDocType.ToString() + "_Upload_" + _runTime) as WCToolTipLinkButton;

            Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = new Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum>(pDocType, pRunTime);

            if (FindControl("btn" + pDocType.ToString() + "_Delete_" + _runTime) is WCToolTipLinkButton btnDelete)
                btnDelete.Enabled = m_CorporateDocs.ContainsKey(_key);
            if (FindControl("btn" + pDocType.ToString() + "_Open_" + _runTime) is WCToolTipLinkButton btnOpen)
                btnOpen.Enabled = m_CorporateDocs.ContainsKey(_key); ;
        }
        #endregion SetUploadControl
        #region SetTooltipInformation
        /// <summary>
        /// ● Lien hyperlink pour les énumérateurs.
        ///   CorporateEventGroupEnum
        ///   CorporateEventTypeEnum
        ///   AdjustmentMethodOfDerivContractEnum
        /// </summary>
        /// <param name="pControl">Control de l'énumérateur</param>
        /// <param name="pEnum">Nom de l'énumérateur dans ENUMS</param>
        /// FI 20160804 [Migration TFS] Modify
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        private void SetTooltipInformation(Control pControl, string pEnum)
        {
            if (FindControl(pControl.ID.Replace(Cst.DDL.ToLower(), "ttip")) is WCToolTipPanel btn)
            {
                // FI 20160804 [Migration TFS] Repository à la place de Referential
                string url = String.Format("Referential.aspx?T=Repository&O=ENUMS&M=2&PK=CODE&PKV={0}&F=frmConsult&IDMenu={1}", pEnum, IdMenu.GetIdMenu(IdMenu.Menu.Enums));
                btn.Attributes.Add("onclick", JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_ListReferential));
                btn.CssClass = "fa-icon fas fa-info-circle brown";
                btn.Pty.TooltipTitle = pEnum;
                btn.Pty.TooltipStyle = "qtip-brown";

                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                //if (null != ListEnumsSchemes)
                //{
                //ExtendEnum extendEnum = ListEnumsSchemes[pEnum];
                ExtendEnum extendEnum = DataEnabledEnumHelper.GetDataEnum(SessionTools.CS, pEnum);
                if (null != extendEnum)
                {
                    if (StrFunc.IsFilled(extendEnum.Definition))
                        btn.Pty.TooltipContent = Cst.HTMLBold + extendEnum.Definition + Cst.HTMLEndBold;
                    if (StrFunc.IsFilled(extendEnum.Documentation))
                        btn.Pty.TooltipContent += Cst.HTMLBreakLine + extendEnum.Documentation;
                    btn.Pty.TooltipContent += Cst.HTMLHorizontalLine;
                }
                //}
                btn.Pty.TooltipContent += Ressource.GetString("ClickToSeeEnum");

            }
        }
        #endregion SetTooltipInformation
        #region SetValue
        /// <summary>
        /// ● Mode Consultation/Modification
        ///   Alimentation des contrôles 
        /// </summary>
        /// <param name="pCorporateAction">Corporate action alimentée via lecture v(CORPOACTION/CORPOACTIONISSUE</param>
        // EG [33415/33420] Gestion ResultType.Info
        // EG 20140317 [19722] Gestion ResultType.check
        // EG 20141106 [20253] Equalization payment
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        // EG 20211108 [XXXXX] Gestion ATTACHEDDOC et NOTEPAD sur toutes les CA (publiées ou intégrées)
        // EG 20211020 [XXXXX] Nouvelle gestion des notices (URLNOTICE et USEURLNOTICE)
        private void SetValue()
        {
            LoadTemplateEventArgs loadTemplateEventArgs = new LoadTemplateEventArgs(false);

            int i = 0;
            MarketIdentification _market = GetMarketIdentificationById(m_DicMarket, m_CorporateAction.market.SpheresId);
            if (null != _market)
                txtCAMarket.Text = _market.shortAcronym;

            OnMarketChanged(txtCAMarket, null);

            #region Mise à jour des contrôles CORPORATE ACTION
            if (m_CorporateAction.identifierSpecified)
                txtCAIdentifier.Text = m_CorporateAction.identifier;
            if (m_CorporateAction.displaynameSpecified)
                txtCADisplayName.Text = m_CorporateAction.displayname;
            if (m_CorporateAction.descriptionSpecified)
                txtCADocumentation.Text = m_CorporateAction.description;

            ControlsTools.DDLSelectByValue(ddlCAReadyState, m_CorporateAction.readystate.ToString());
            if (m_CorporateAction.cfiCodeSpecified)
                ControlsTools.DDLSelectByText(ddlCACfiCodeCategory, m_CorporateAction.cfiCode.ToString());

            txtCAURLNotice.Text = m_CorporateAction.urlnotice;
            chkCAUseURLNotice.Checked = m_CorporateAction.refNotice.useurlnoticeSpecified && m_CorporateAction.refNotice.useurlnotice;

            txtCARefNotice.Text = m_CorporateAction.refNotice.value;
            txtCARefNoticeFileName.Text = m_CorporateAction.refNotice.fileName;
            txtCAPubDate.Text = DtFunc.DateTimeToString(m_CorporateAction.pubDate, DtFunc.FmtShortDate);

            btnDelRefNotice.Visible = m_CorporateAction.refNoticeAddSpecified;
            if (m_CorporateAction.refNoticeAddSpecified)
            {
                CreateRefNoticeAddContainer();
                i = 1;
                foreach (RefNoticeIdentification noticeAdd in m_CorporateAction.refNoticeAdd)
                {
                    if (FindControl(CATools.pfxTXTNoticeAdd + "Reference" + i.ToString()) is WCTextBox2 txt)
                        txt.Text = noticeAdd.value;
                    txt = FindControl(CATools.pfxTXTNoticeAdd + "PubDate" + i.ToString()) as WCTextBox2;
                    if (null != txt)
                        txt.Text = DtFunc.DateTimeToString(noticeAdd.pubDate, DtFunc.FmtShortDate);
                    txt = FindControl(CATools.pfxTXTNoticeAdd + "FileName" + i.ToString()) as WCTextBox2;
                    if (null != txt)
                        txt.Text = noticeAdd.fileName;
                    i++;

                }
            }
            #endregion Mise à jour des contrôles CORPORATE ACTION
            #region Mise à jour des contrôles CORPORATE EVENT
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];
            ControlsTools.DDLSelectByValue(ddlCEGroup, corporateEvent.group.ToString());
            OnCEGroupChanged(ddlCEGroup, loadTemplateEventArgs);


            ControlsTools.DDLSelectByValue(ddlCEType, corporateEvent.type.ToString());

            if (corporateEvent.operandSpecified)
                ControlsTools.DDLSelectByValue(ddlCECombinedOperand, corporateEvent.operand.ToString());
            if (corporateEvent.combinedTypeSpecified)
                ControlsTools.DDLSelectByValue(ddlCECombinedType, corporateEvent.combinedType.ToString());

            OnCETypeChanged(ddlCEType, loadTemplateEventArgs);

            if (corporateEvent.identifierSpecified)
                txtCEIdentifier.Text = corporateEvent.identifier;
            if (corporateEvent.exDateSpecified)
                txtCEExDate.Text = DtFunc.DateTimeToString(corporateEvent.exDate, DtFunc.FmtShortDate);
            if (corporateEvent.effectiveDateSpecified)
                txtCEEffectiveDate.Text = DtFunc.DateTimeToString(corporateEvent.effectiveDate, DtFunc.FmtShortDate);

            ControlsTools.DDLSelectByValue(ddlCEMode, ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(corporateEvent.mode));
            #endregion Mise à jour des contrôles CORPORATE EVENT

            if (null != corporateEvent.procedure)
            {

                #region Mise à jour des contrôles SOUS-JACENTS
                if (null != corporateEvent.procedure.underlyers)
                {
                    i = 1;
                    KeyValuePair<int, MarketIdentification> _market2;
                    foreach (CorporateEventUnderlyer underlyer in corporateEvent.procedure.underlyers)
                    {
                        if (i == 1)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEUNLCategory, underlyer.category.ToString());
                            if (underlyer.marketSpecified && underlyer.market.FIXML_SecurityExchangeSpecified)
                            {
                                _market2 = m_DicUNLMarket.Single(item => item.Value.FIXML_SecurityExchange == underlyer.market.FIXML_SecurityExchange);
                                txtCEUNLMarket.Text = _market2.Value.shortAcronym;
                            }

                            txtCEUNLIdentifier.Text = underlyer.identifierSpecified?underlyer.identifier:string.Empty;
                            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
                                txtCEUNLCode.Text = underlyer.caIssueCodeSpecified ? underlyer.caIssueCode : string.Empty;
                            else
                                txtCEUNLCode.Text = underlyer.isinCodeSpecified ? underlyer.isinCode : string.Empty;
                        }
                        else
                        {
                            ControlsTools.DDLSelectByValue(ddlCEUNLCategory2, underlyer.category.ToString());
                            if (underlyer.marketSpecified && underlyer.market.FIXML_SecurityExchangeSpecified)
                            {
                                _market2 = m_DicUNLMarket.Single(item => item.Value.FIXML_SecurityExchange == underlyer.market.FIXML_SecurityExchange);
                                txtCEUNLMarket2.Text = _market2.Value.shortAcronym;
                            }

                            txtCEUNLIdentifier2.Text = underlyer.identifierSpecified ? underlyer.identifier : string.Empty;
                            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
                                txtCEUNLCode2.Text = underlyer.caIssueCodeSpecified ? underlyer.caIssueCode : string.Empty;
                            else
                                txtCEUNLCode2.Text = underlyer.isinCodeSpecified ? underlyer.isinCode : string.Empty;
                        }
                        i++;
                    }
                }
                #endregion Mise à jour des contrôles SOUS-JACENTS

                #region Mise à jour des contrôles METHODES D'AJUSTEMENT
                ControlsTools.DDLSelectByValue(ddlCEAdjMethod, corporateEvent.procedure.adjustment.method.ToString());
                OnCEMethodChanged(ddlCEAdjMethod, loadTemplateEventArgs);

                if (StrFunc.IsFilled(corporateEvent.procedure.adjustment.templateFileName))
                {
                    string[] template = corporateEvent.procedure.adjustment.templateFileName.Split('_');
                    ControlsTools.DDLSelectByValue(ddlCETemplate, template[template.Length - 1]);
                    loadTemplateEventArgs.isLoadRequested = true;
                    OnCETemplateChanged(ddlCETemplate, loadTemplateEventArgs);
                }
                if (StrFunc.IsFilled(corporateEvent.procedure.adjustment.templateFileName_AI))
                {
                    string[] template = corporateEvent.procedure.adjustment.templateFileName_AI.Split('_');
                    ControlsTools.DDLSelectByValue(ddlCETemplate_AI, template[template.Length - 1]);
                    loadTemplateEventArgs.isLoadRequested = true;
                    OnCETemplate_AIChanged(ddlCETemplate_AI, loadTemplateEventArgs);
                }
                #endregion Mise à jour des contrôles METHODES D'AJUSTEMENT

                #region Mise à jour des COMPOSANTS SIMPLES
                SetValue(m_ListComponentSimples);
                SetValue(m_ListComponentSimples_AI);
                #endregion Mise à jour des COMPOSANTS SIMPLES

                if (corporateEvent.procedure.adjustment is AdjustmentRatio ratio)
                {
                    #region Mise à jour du RFACTOR CERTIFIE
                    if (ratio.rFactor.rFactorCertifiedSpecified && ratio.rFactor.rFactorCertified.resultSpecified)
                    {
                        WCTextBox2 txtRFactorCertified = FindControl(CATools.pfxTXTComponent + CATools.RFactorCertified_Id) as WCTextBox2;
                        Result result = ratio.rFactor.rFactorCertified.result;
                        SimpleUnit _simpleUnit = result.result as SimpleUnit;
                        txtRFactorCertified.Text = _simpleUnit.valueRounded.CultureValue;
                    }
                    #endregion Mise à jour du RFACTOR CERTIFIE

                    AdjustmentContract contract = ratio.contract;
                    chkCEAdjFutureCSize.Checked = contract.futureSpecified && contract.future.contractSizeSpecified;
                    // EG 20150218 [20775]
                    //chkCEAdjFuturePrice.Checked = contract.futureSpecified && contract.future.priceSpecified;
                    if (contract.futureSpecified && contract.future.equalisationPaymentSpecified)
                    {
                        chkCEAdjFuturePrice.Checked = true;
                        chkCEAdjFuturePrice.Enabled = false;
                    }
                    else
                    {
                        chkCEAdjFuturePrice.Enabled = true;
                        chkCEAdjFuturePrice.Checked = contract.futureSpecified && contract.future.priceSpecified;
                    }
                    chkCEAdjFutureEqualisationPayment.Checked = contract.futureSpecified && contract.future.equalisationPaymentSpecified;

                    chkCEAdjOptionCSize.Checked = contract.optionSpecified && contract.option.contractSizeSpecified;
                    chkCEAdjOptionStrikePrice.Checked = contract.optionSpecified && contract.option.strikePriceSpecified;
                    // EG 20150218 [20775]
                    //chkCEAdjOptionPrice.Checked = contract.optionSpecified && contract.option.priceSpecified;
                    if (contract.optionSpecified && contract.option.equalisationPaymentSpecified)
                    {
                        chkCEAdjOptionPrice.Checked = true;
                        chkCEAdjOptionPrice.Enabled = false;
                    }
                    else
                    {
                        chkCEAdjOptionPrice.Enabled = true;
                        chkCEAdjOptionPrice.Checked = contract.optionSpecified && contract.option.priceSpecified;
                    }
                    chkCEAdjOptionEqualisationPayment.Checked = contract.optionSpecified && contract.option.equalisationPaymentSpecified;

                    #region Mise à jour des REGLES D'ARRONDI
                    if (ratio.rFactor.roundingSpecified)
                    {
                        ControlsTools.DDLSelectByValue(ddlCERFactorRoundingDir, ratio.rFactor.rounding.direction.ToString());
                        ControlsTools.DDLSelectByIndex(ddlCERFactorRoundingPrec, ratio.rFactor.rounding.precision);
                    }
                    if (contract.futureSpecified)
                    {
                        AdjustmentFuture future = contract.future;
                        if (future.contractSizeSpecified && future.contractSize.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEContractSizeRoundingDir, future.contractSize.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEContractSizeRoundingPrec, future.contractSize.rounding.precision);
                        }
                        if (future.contractMultiplierSpecified && future.contractMultiplier.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEContractMultiplierRoundingDir, future.contractMultiplier.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEContractMultiplierRoundingPrec, future.contractMultiplier.rounding.precision);
                        }
                        if (future.priceSpecified && future.price.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEPriceRoundingDir, future.price.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEPriceRoundingPrec, future.price.rounding.precision);
                        }
                        if (future.equalisationPaymentSpecified && future.equalisationPayment.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEEqualisationPaymentRoundingDir, future.equalisationPayment.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEEqualisationPaymentRoundingPrec, future.equalisationPayment.rounding.precision);
                        }
                    }
                    if (ratio.contract.optionSpecified)
                    {
                        AdjustmentOption option = contract.option;
                        if (option.contractSizeSpecified && option.contractSize.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEContractSizeRoundingDir, option.contractSize.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEContractSizeRoundingPrec, option.contractSize.rounding.precision);
                        }
                        if (option.contractMultiplierSpecified && option.contractMultiplier.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEContractMultiplierRoundingDir, option.contractMultiplier.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEContractMultiplierRoundingPrec, option.contractMultiplier.rounding.precision);
                        }

                        if (option.strikePriceSpecified && option.strikePrice.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEStrikePriceRoundingDir, option.strikePrice.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEStrikePriceRoundingPrec, option.strikePrice.rounding.precision);
                        }
                        if (option.priceSpecified && option.price.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEPriceRoundingDir, option.price.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEPriceRoundingPrec, option.price.rounding.precision);
                        }
                        if (option.equalisationPaymentSpecified && option.equalisationPayment.roundingSpecified)
                        {
                            ControlsTools.DDLSelectByValue(ddlCEEqualisationPaymentRoundingDir, option.equalisationPayment.rounding.direction.ToString());
                            ControlsTools.DDLSelectByIndex(ddlCEEqualisationPaymentRoundingPrec, option.equalisationPayment.rounding.precision);
                        }
                    }
                    #endregion Mise à jour des REGLES D'ARRONDI
                }
            }

            // FileUpload (Script)
            // EG 20140518 [19913]
            if (m_CorporateAction.corporateDocsSpecified)
                m_CorporateDocs = m_CorporateAction.corporateDocs;
            else
                m_CorporateDocs = new CorporateDocs();

            InitializeControlSQLFileName(txtMSSQL_File_C);
            InitializeControlSQLFileName(txtMSSQL_File_E);
            InitializeControlSQLFileName(txtMSSQL_File_P);
            InitializeControlSQLFileName(txtMSSQL_File_F);
            InitializeControlSQLFileName(txtORA_File_C);
            InitializeControlSQLFileName(txtORA_File_E);
            InitializeControlSQLFileName(txtORA_File_P);
            InitializeControlSQLFileName(txtORA_File_F);

            if (false == Cst.Capture.IsModeNewCapture(m_CAInfo.Mode))
            {
                // PM 20240604 [XXXXX] Ajout paramètre pParentGUID
                string urlNotepad = JavaScript.GetUrlNotepad(m_CAInfo.CATable.ToString(), m_CorporateAction.IdCA.ToString(),
                    Request.QueryString["IDMENU"].ToString(), m_CorporateAction.identifier, Cst.ConsultationMode.Normal, "0", "Repository", this.GUID);
                bool isNotepadSpecified = IsExistNotepadOrAttachedDoc(Cst.OTCml_TBL.NOTEPAD, m_CorporateAction.IdCA, out string tooltip);
                btnNotepad.CssClass = "fa-icon " + (isNotepadSpecified ? "green" : String.Empty);
                btnNotepad.Pty.TooltipContent = tooltip;
                btnNotepad.Attributes.Add("onclick", @"OpenNotepad('" + urlNotepad + "','lblIDCA','txtCAIdentifier'); return false;");

                // PM 20240604 [XXXXX] Ajout paramètre pParentGUID
                string urlAttachedNotice = JavaScript.GetUrlAttachedDoc("ATTACHEDDOC", m_CorporateAction.IdCA.ToString(), m_CorporateAction.identifier,
                                                m_CorporateAction.market.identifier, Request.QueryString["IDMENU"].ToString(), m_CAInfo.CATable.ToString(), this.GUID);

                bool isAttachedNoticeSpecified = IsExistNotepadOrAttachedDoc(Cst.OTCml_TBL.ATTACHEDDOC, m_CorporateAction.IdCA, out tooltip);

                btnAttachedNotice.CssClass = "fa-icon " + (isAttachedNoticeSpecified ? "green" : String.Empty);
                btnAttachedNotice.Pty.TooltipContent = tooltip;
                btnAttachedNotice.Attributes.Add("onclick", @"OpenAttachedDoc('" + urlAttachedNotice + "','lblIDCA','txtCAIdentifier'); return false;");
            }
        }

        // EG 20200408 [XXXXX] NEW Modification and enhancement of notice management (URLNOTICE and USEURLNOTICE)
        // EG 20211108 [XXXXX] Gestion ATTACHEDDOC et NOTEPAD sur toutes les CA (publiées ou intégrées)
        private bool IsExistNotepadOrAttachedDoc(Cst.OTCml_TBL pTableSource, int pId, out string pInformation)
        {
            bool isExist = false;
            pInformation = string.Empty;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), m_CAInfo.CATable.ToString());
            parameters.Add(new DataParameter(SessionTools.CS, "ID", DbType.Int32), pId);

            string sqlSelect = String.Format(@"select ac.DISPLAYNAME, doc.DTUPD 
            from dbo.{0} doc 
            inner join dbo.ACTOR ac on (ac.IDA = doc.IDAUPD)
            where (doc.TABLENAME = @TABLENAME) and (doc.ID = @ID)
            order by doc.DTUPD desc", pTableSource.ToString());

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                pInformation = Ressource.GetString(pTableSource == Cst.OTCml_TBL.ATTACHEDDOC ? "btnAttachedDoc" : "btnNotePad");

                isExist = dr.Read();
                if (isExist)
                {
                    DateTimeTz dateTimeTz = new DateTimeTz(Convert.ToDateTime(dr[1]), "Etc/UTC");
                    pInformation += Cst.CrLf + RessourceExtended.GetString_LastModifyBy(dateTimeTz, dr[0].ToString(), true);
                }
            }
            return isExist;
        }

        private void SetValue(List<ComponentSimple> pListComponent)
        {
            CorporateEvent corporateEvent = m_CorporateAction.corporateEvent[0];

            #region Mise à jour des COMPOSANTS SIMPLES
            pListComponent.ForEach(item =>
            {
                if ((ReflectionTools.GetObjectById(corporateEvent.procedure, item.Id) is ComponentSimple component) && component.resultSpecified)
                {
                    WCDropDownList2 ddlComponent = FindControl(CATools.pfxDDLComponent + item.Id) as WCDropDownList2;
                    if (FindControl(CATools.pfxTXTComponent + item.Id) is WCTextBox2 txtComponent)
                    {
                        switch (component.result.itemsElementName)
                        {
                            case ResultType.amount:
                                Money _money = component.result.result as Money;
                                if (null != _money.amount)
                                {
                                    txtComponent.Text = _money.amount.CultureValue;
                                    ControlsTools.DDLSelectByValue(ddlComponent, _money.currency.value);
                                }
                                break;
                            case ResultType.unit:
                                SimpleUnit _simpleUnit = component.result.result as SimpleUnit;
                                if (null != _simpleUnit.value)
                                    txtComponent.Text = _simpleUnit.value.CultureValue;
                                break;
                            case ResultType.info:
                                if (component.result.result is string _info)
                                    txtComponent.Text = _info;
                                break;
                        }
                    }
                    else if ((FindControl(CATools.pfxCHKComponent + item.Id) is WCCheckBox2 chkComponent) && (component.result.itemsElementName == ResultType.check))
                    {
                        bool _checked = BoolFunc.IsTrue(component.result.result);
                        chkComponent.Checked = _checked;
                    }

                }
            });
            #endregion Mise à jour des COMPOSANTS SIMPLES
        }
        #endregion SetValue
        //* --------------------------------------------------------------- *//
        // Chargement des données de base: 
        // MARCHE (+ règle d'arrondi), TEMPLATE (fichiers XML d'ajustement)
        //* --------------------------------------------------------------- *//

        #region LoadCorporateAction
        /// <summary>
        /// ● Chargement d'une Corporate Action 
        ///   Lecture dans la(les) table(s) (CORPOACTIONISSUE ou CORPOACTION/CORPOEVENT/CORPOEVENTNOTICE)
        ///   Initialisation de la classe CorporateAction
        ///   Initilaisation des contrôles WEB 
        /// </summary>
        private void LoadCorporateAction()
        {
            m_CorporateAction = new CorporateAction(SessionTools.CS);
            string templatePath = Request.MapPath(@"CorporateActions\Templates\");
            m_CorporateAction.Load(m_CAInfo,CATools.CAWhereMode.ID, templatePath);
            SetValue();
        }
        #endregion LoadCorporateAction
        #region LoadMarket
        /// <summary>
        /// ● Chargement des marchés de négociation ETD (hors segment) 
        ///   Alimentation du dictionnaire m_DicMarket (Rappel: (Key,Value) = (idM, MarketIdentification))
        /// </summary>
        // PL 20171006 [23469] Original MARKETTYPE deprecated
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        // Ecriture des données en cache
        // EG 20211020 [XXXXX] Nouvelle gestion des notices (URLCANOTICE)
        private void LoadMarket()
        {
            m_DicMarket = new Dictionary<int, MarketIdentification>();
            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += "mk.IDM, mk.IDENTIFIER, mk.DISPLAYNAME, mk.DESCRIPTION, mk.IDMOPERATING, " + Cst.CrLf;
            SQLQuery += "mk.ACRONYM, mk.ISO10383_ALPHA4, mk.EXCHANGESYMBOL, mk.SHORTIDENTIFIER, mk.IDBC, mk.SHORT_ACRONYM," + Cst.CrLf;
            SQLQuery += "mk.FIXML_SECURITYEXCHANGE, mk.URLCANOTICE" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " mk" + Cst.CrLf;
            SQLQuery += SQLCst.WHERE + "(mk.ISTRADEDDERIVATIVE=1)" + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "mk.SHORT_ACRONYM" + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, SQLQuery.ToString());
            if (null != ds)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    MarketIdentification _market = new MarketIdentification
                    {
                        spheresid = row["IDM"].ToString(),
                        marketType = CATools.CAMarketType(Convert.IsDBNull(row["IDMOPERATING"]) ? 0 : Convert.ToInt32(row["IDMOPERATING"])),
                        identifierSpecified = (false == Convert.IsDBNull(row["IDENTIFIER"])),
                        displaynameSpecified = (false == Convert.IsDBNull(row["DISPLAYNAME"])),
                        descriptionSpecified = (false == Convert.IsDBNull(row["DESCRIPTION"])),
                        acronymSpecified = (false == Convert.IsDBNull(row["ACRONYM"])),
                        exchangeSymbolSpecified = (false == Convert.IsDBNull(row["EXCHANGESYMBOL"])),
                        ISO10383_ALPHA4Specified = (false == Convert.IsDBNull(row["ISO10383_ALPHA4"])),
                        shortIdentifierSpecified = (false == Convert.IsDBNull(row["SHORTIDENTIFIER"])),
                        shortAcronymSpecified = (false == Convert.IsDBNull(row["SHORT_ACRONYM"])),
                        FIXML_SecurityExchangeSpecified = (false == Convert.IsDBNull(row["FIXML_SECURITYEXCHANGE"])),
                        idBCSpecified = (false == Convert.IsDBNull(row["IDBC"])),
                        urlCANoticeSpecified = (false == Convert.IsDBNull(row["URLCANOTICE"]))
                    };

                    if (_market.identifierSpecified)
                        _market.identifier = row["IDENTIFIER"].ToString();
                    if (_market.displaynameSpecified)
                        _market.displayname = row["DISPLAYNAME"].ToString();
                    if (_market.descriptionSpecified)
                        _market.description = row["DESCRIPTION"].ToString();
                    if (_market.acronymSpecified)
                        _market.acronym = row["ACRONYM"].ToString();
                    if (_market.exchangeSymbolSpecified)
                        _market.exchangeSymbol = row["EXCHANGESYMBOL"].ToString();
                    if (_market.ISO10383_ALPHA4Specified)
                        _market.ISO10383_ALPHA4 = row["ISO10383_ALPHA4"].ToString();
                    if (_market.shortIdentifierSpecified)
                        _market.shortIdentifier = row["SHORTIDENTIFIER"].ToString();
                    if (_market.shortAcronymSpecified)
                        _market.shortAcronym = row["SHORT_ACRONYM"].ToString();
                    if (_market.FIXML_SecurityExchangeSpecified)
                        _market.FIXML_SecurityExchange = row["FIXML_SECURITYEXCHANGE"].ToString();
                    if (_market.idBCSpecified)
                        _market.idBC= row["IDBC"].ToString();
                    if (_market.urlCANoticeSpecified)
                        _market.urlCANotice = row["URLCANOTICE"].ToString();
                    
                    m_DicMarket.Add(Convert.ToInt32(row["IDM"]), _market);
                }
            }
            DicMarket = m_DicMarket;
        }
        #endregion LoadMarket
        #region LoadMarketRules
        /// <summary>
        /// ● Chargement des règles d'arrondi des marchés par élément
        ///   Ratio
        ///   ContractSize
        ///   ContractMultiplier
        ///   StrikePrice
        ///   Price
        ///   Equalization payment
        /// </summary>
        /// EG 20141106 [20253] Equalization payment
        private void LoadMarketRules()
        {
            m_DicMarketRules = new Dictionary<int, CorporateEventMktRules>();
            IDataReader dr = null;
            try
            {
                CATools.MKTRulesQuery _mktQry = new CATools.MKTRulesQuery(SessionTools.CS);
                QueryParameters qryParameters = _mktQry.GetQuerySelect();
                dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                while (dr.Read())
                {
                    int _idM = Convert.ToInt32(dr["IDM"]);
                    CorporateEventMktRules _mktRules = null;
                    Rounding _rounding = null;
                    Pair<AdjustmentElementEnum, Rounding> _pair = null;

                    if (false == m_DicMarketRules.ContainsKey(_idM))
                    {
                        _mktRules = new CorporateEventMktRules
                        {
                            idM = _idM,
                            renamingContractMethod = CATools.CERenamingContractMethod(dr["RENAMINGMETHOD"].ToString()),
                            rounding = new List<Pair<AdjustmentElementEnum, Rounding>>(),
                            isEqualPaymentFutureAuthorized = Convert.ToBoolean(dr["EQP_FUTISAUTHORIZED"]),
                            isEqualPaymentOptionAuthorized = Convert.ToBoolean(dr["EQP_OPTISAUTHORIZED"])
                        };

                        m_DicMarketRules.Add(_idM, _mktRules);
                    }

                    _mktRules = m_DicMarketRules[_idM];
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.RFactor))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["RFACTOR_RNDDIR"]), Convert.ToBoolean(dr["RFACTOR_ISURNDDIR"]),
                                                Convert.ToInt32(dr["RFACTOR_RNDPREC"]), Convert.ToBoolean(dr["RFACTOR_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.RFactor, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.ContractSize))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["CSIZE_RNDDIR"]), Convert.ToBoolean(dr["CSIZE_ISURNDDIR"]),
                                                Convert.ToInt32(dr["CSIZE_RNDPREC"]), Convert.ToBoolean(dr["CSIZE_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.ContractSize, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.ContractMultiplier))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["CMUL_RNDDIR"]), Convert.ToBoolean(dr["CMUL_ISURNDDIR"]),
                                                Convert.ToInt32(dr["CMUL_RNDPREC"]), Convert.ToBoolean(dr["CMUL_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.ContractMultiplier, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.StrikePrice))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["STRIKE_RNDDIR"]), Convert.ToBoolean(dr["STRIKE_ISURNDDIR"]),
                                                Convert.ToInt32(dr["STRIKE_RNDPREC"]), Convert.ToBoolean(dr["STRIKE_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.StrikePrice, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.Price))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["PRICE_RNDDIR"]), Convert.ToBoolean(dr["PRICE_ISURNDDIR"]),
                                                Convert.ToInt32(dr["PRICE_RNDPREC"]), Convert.ToBoolean(dr["PRICE_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.Price, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                    if (false == m_DicMarketRules[_idM].rounding.Exists(item => item.First == AdjustmentElementEnum.EqualisationPayment))
                    {
                        _rounding = new Rounding(Convert.ToString(dr["EQPAYMENT_RNDDIR"]), Convert.ToBoolean(dr["EQPAYMENT_ISURNDDIR"]),
                                                Convert.ToInt32(dr["EQPAYMENT_RNDPREC"]), Convert.ToBoolean(dr["EQPAYMENT_ISURNDPREC"]));
                        _pair = new Pair<AdjustmentElementEnum, Rounding>(AdjustmentElementEnum.EqualisationPayment, _rounding);
                        _mktRules.rounding.Add(_pair);
                    }
                }
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }

        }
        #endregion LoadMarketRules
        #region LoadMarket
        /// <summary>
        /// ● Chargement des marchés de négociation ETD (hors segment) 
        ///   Alimentation du dictionnaire m_DicMarket (Rappel: (Key,Value) = (idM, MarketIdentification))
        /// </summary>
        // PL 20171006 [23469] Original MARKETTYPE deprecated
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        // Ecriture des données en cache
        private void LoadUNLMarket()
        {
            m_DicUNLMarket = new Dictionary<int, MarketIdentification>();
            StrBuilder SQLQuery = new StrBuilder(SQLCst.SELECT);
            SQLQuery += "mk.IDM, mk.IDENTIFIER, mk.DISPLAYNAME, mk.DESCRIPTION, mk.IDMOPERATING, " + Cst.CrLf;
            SQLQuery += "mk.ACRONYM, mk.ISO10383_ALPHA4, mk.EXCHANGESYMBOL, mk.SHORTIDENTIFIER, mk.IDBC, mk.SHORT_ACRONYM," + Cst.CrLf;
            SQLQuery += "mk.FIXML_SECURITYEXCHANGE" + Cst.CrLf;
            SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString() + " mk" + Cst.CrLf;
            SQLQuery += SQLCst.ORDERBY + "mk.SHORT_ACRONYM" + Cst.CrLf;
            DataSet ds = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, SQLQuery.ToString());
            if (null != ds)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    MarketIdentification _market = new MarketIdentification
                    {
                        spheresid = row["IDM"].ToString(),
                        marketType = CATools.CAMarketType(Convert.IsDBNull(row["IDMOPERATING"]) ? 0 : Convert.ToInt32(row["IDMOPERATING"])),
                        identifierSpecified = (false == Convert.IsDBNull(row["IDENTIFIER"])),
                        displaynameSpecified = (false == Convert.IsDBNull(row["DISPLAYNAME"])),
                        descriptionSpecified = (false == Convert.IsDBNull(row["DESCRIPTION"])),
                        acronymSpecified = (false == Convert.IsDBNull(row["ACRONYM"])),
                        exchangeSymbolSpecified = (false == Convert.IsDBNull(row["EXCHANGESYMBOL"])),
                        ISO10383_ALPHA4Specified = (false == Convert.IsDBNull(row["ISO10383_ALPHA4"])),
                        shortIdentifierSpecified = (false == Convert.IsDBNull(row["SHORTIDENTIFIER"])),
                        shortAcronymSpecified = (false == Convert.IsDBNull(row["SHORT_ACRONYM"])),
                        idBCSpecified = (false == Convert.IsDBNull(row["IDBC"])),
                        FIXML_SecurityExchangeSpecified = (false == Convert.IsDBNull(row["FIXML_SECURITYEXCHANGE"]))
                    };

                    if (_market.identifierSpecified)
                        _market.identifier = row["IDENTIFIER"].ToString();
                    if (_market.displaynameSpecified)
                        _market.displayname = row["DISPLAYNAME"].ToString();
                    if (_market.descriptionSpecified)
                        _market.description = row["DESCRIPTION"].ToString();
                    if (_market.acronymSpecified)
                        _market.acronym = row["ACRONYM"].ToString();
                    if (_market.exchangeSymbolSpecified)
                        _market.exchangeSymbol = row["EXCHANGESYMBOL"].ToString();
                    if (_market.ISO10383_ALPHA4Specified)
                        _market.ISO10383_ALPHA4 = row["ISO10383_ALPHA4"].ToString();
                    if (_market.shortIdentifierSpecified)
                        _market.shortIdentifier = row["SHORTIDENTIFIER"].ToString();
                    if (_market.shortAcronymSpecified)
                        _market.shortAcronym = row["SHORT_ACRONYM"].ToString();
                    if (_market.idBCSpecified)
                        _market.idBC = row["IDBC"].ToString();
                    if (_market.FIXML_SecurityExchangeSpecified)
                        _market.FIXML_SecurityExchange = row["FIXML_SECURITYEXCHANGE"].ToString();

                    m_DicUNLMarket.Add(Convert.ToInt32(row["IDM"]), _market);
                }
            }
            DicUNLMarket = m_DicUNLMarket;
        }
        #endregion LoadUNLMarket
        #region LoadXMLTemplateFile
        /// <summary>
        /// ● Chargement des TEMPLATES d'ajustement disponibles 
        ///   Matérialisés par des fichiers XML présents dans le répertoire CorporateActions/Templates
        ///   Chaque fichier est désérialisé et alimentera un dictionnaire m_DicTemplates 
        ///   
        /// ● Caractéristiques du dictionnaire (m_DicTemplates)
        ///   Key   = AdjustmentMethodOfDerivContractEnum
        ///   Value = Dictionnaire de templates
        ///   
        /// ● Caractéristiques du dictionnaire de templates
        ///   Key   = Type de Corporate event (CEType ou CEType + "_" + CECOMBINEDOPERAND + "_" + CECOMBINEDTYPE)
        ///   Value = Liste des templates (Paire <Nom du template, Template déserialisé de type CorporateEventRPocedure>)
        ///   
        /// ● Règle adoptée pour le nom d'un fichier template
        ///   Corporate action simple   : CEGroup_CEAdjMethod_CEType_Name.xml
        ///   Exemples                  : Distribution_Ratio_RightsIssue_Regular.xml, Distribution_Ratio_RightsIssue_ViaValueSubscriptionRights.xml
        ///   Corporate action combinée : CEGroup_CEAdjMethod_CEType_CECombinedOperand_CECombinedType_Name.xml
        ///   Exemples                  : Combination_Ratio_KRepayment_Together_Split_Regular.xml
        /// </summary>
        // EG 20180426 Analyse du code Correction [CA2202]
        private void LoadXMLTemplateFile()
        {
            m_DicTemplates = new Dictionary<AdjustmentMethodOfDerivContractEnum, Dictionary<string, List<Pair<string, CorporateEventProcedure>>>>();

            string templatePath = Request.MapPath(@"CorporateActions\Templates\");

            // Lectures des templates présents dans le répertoire
            if (Directory.Exists(templatePath))
            {
                DirectoryInfo templateDirectoryInfo = new DirectoryInfo(templatePath);
                FileInfo[] fileInfos = templateDirectoryInfo.GetFiles();
                Regex regex_Regular = new Regex(CATools.GetRegularTemplatePattern());
                Regex regex_Combined = new Regex(CATools.GetCombinedTemplatePattern());
                Regex regex_RatioCertified = new Regex(CATools.GetRatioCertifiedTemplatePattern());
                foreach (FileInfo fileInfo in fileInfos)
                {
                    string fileName = Path.ChangeExtension(fileInfo.Name, null);

                    bool isRegularTemplate = regex_Regular.IsMatch(fileName);
                    bool isCombinedTemplate = regex_Combined.IsMatch(fileName);
                    bool isRatioCertifiedTemplate = regex_RatioCertified.IsMatch(fileName);
                    bool isFileNameConform = false;
                    int nbSplit = 4;
                    if (isCombinedTemplate)
                        nbSplit = 6;

                    if (isRegularTemplate || isCombinedTemplate || isRatioCertifiedTemplate)
                    {
                        string[] _temp = fileName.Split("_".ToCharArray(), nbSplit);

                        // Lecture des informations dans le nom du template XML conformément aux REGEXs
                        CorporateEventGroupEnum? _group = CATools.CEGroup(_temp[0]);
                        AdjustmentMethodOfDerivContractEnum? _method = CATools.CEMethod(_temp[1]);
                        CorporateEventTypeEnum? _type = CATools.CEType(_temp[2]);

                        isFileNameConform = (_group.HasValue && _type.HasValue && _method.HasValue);
                        if (isFileNameConform)
                        {
                            string _keyType = _temp[2];
                            string _templateName = _type.Value.ToString();
                            if (isCombinedTemplate)
                            {
                                CombinationOperandEnum? _oper = CATools.CEOperand(_temp[3]);
                                CorporateEventTypeEnum? _type2 = CATools.CEType(_temp[4]);
                                isFileNameConform &= (_oper.HasValue && _type2.HasValue);
                                if (isFileNameConform)
                                    _keyType += "_" + _temp[3] + "_" + _temp[4];
                            }

                            if (isFileNameConform)
                            {
                                if (nbSplit == _temp.Length)
                                    _templateName = _temp[nbSplit - 1];

                                // Serialization du template
                                CorporateEventProcedure cep = null;
                                using (FileStream XMLTemplate = fileInfo.Open(FileMode.Open, FileAccess.Read))
                                {
                                    cep = SerializationHelper.LoadObjectFromFileStream<CorporateEventProcedure>(XMLTemplate);
                                }
                                if ((null != cep) && (null != cep.adjustment))
                                    cep.adjustment.templateFileName = fileName;
                                Pair<string, CorporateEventProcedure> _procedure = new Pair<string, CorporateEventProcedure>(_templateName, cep);

                                List<Pair<string, CorporateEventProcedure>> _list;
                                if (false == m_DicTemplates.ContainsKey(_method.Value))
                                {
                                    // Le GROUP de CORPORATE EVENT n'existe pas dans le dictionnaire
                                    _list = new List<Pair<string, CorporateEventProcedure>>
                                    {
                                        _procedure
                                    };
                                    Dictionary<string, List<Pair<string, CorporateEventProcedure>>> _typeTemplates = new Dictionary<string, List<Pair<string, CorporateEventProcedure>>>
                                    {
                                        { _keyType, _list }
                                    };
                                    m_DicTemplates.Add(_method.Value, _typeTemplates);
                                }
                                else if (false == m_DicTemplates[_method.Value].ContainsKey(_keyType))
                                {
                                    // Le TYPE de CORPORATE EVENT n'existe pas dans le dictionnaire
                                    _list = new List<Pair<string, CorporateEventProcedure>>
                                    {
                                        _procedure
                                    };
                                    m_DicTemplates[_method.Value].Add(_keyType, _list);
                                }
                                else
                                {
                                    // Template SUPPLEMENTAIRE sur un TYPE de CORPORATE EVENT existant
                                    m_DicTemplates[_method.Value][_keyType].Add(_procedure);
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion LoadXMLTemplateFile
        #region LoadXMLTemplateFile_Additional
        // EG 20180426 Analyse du code Correction [CA2202]
        private void LoadXMLTemplateFile_Additional()
        {
            m_DicTemplates_Additional = new Dictionary<string, List<Pair<string, object>>>();

            string path = Request.MapPath(@"CorporateActions\Templates\Additionals\");
            if (Directory.Exists(path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(path);
                FileInfo[] fileInfos = directoryInfo.GetFiles();
                Regex regex_Regular = new Regex(CATools.GetRegularTemplateAdditionalPattern());
                foreach (FileInfo fileInfo in fileInfos)
                {
                    string fileName = Path.ChangeExtension(fileInfo.Name, null);

                    bool isRegularTemplate = regex_Regular.IsMatch(fileName);

                    if (isRegularTemplate)
                    {
                        string[] _temp = fileName.Split("_".ToCharArray(), 2);
                        string _templateName = _temp[1];

                        // Serialization du template
                        object template = null;
                        Pair<string,object> _pair = new Pair<string,object>();
                        using (FileStream XMLTemplate = fileInfo.Open(FileMode.Open, FileAccess.Read))
                        {
                            switch (_temp[0])
                            {
                                case "AI":
                                    template = SerializationHelper.LoadObjectFromFileStream<AdditionalInfos>(XMLTemplate) as AdditionalInfos;
                                    break;
                                case "CT":
                                    template = SerializationHelper.LoadObjectFromFileStream<AdjustmentContract>(XMLTemplate) as AdjustmentContract;
                                    break;
                            }
                        }
                        if (null != template)
                        {
                            _pair.First = _temp[1];
                            _pair.Second = template;
                            if (false == m_DicTemplates_Additional.ContainsKey(_temp[0]))
                            {
                                List<Pair<string, object>> _lst = new List<Pair<string, object>>
                                {
                                    _pair
                                };
                                m_DicTemplates_Additional.Add(_temp[0], _lst);
                            }
                            else
                            {
                                m_DicTemplates_Additional[_temp[0]].Add(_pair);
                            }
                        }
                    }
                }
            }
        }
        #endregion LoadXMLTemplateFile_Additional

        //* --------------------------------------------------------------- *//
        // Alimentation des classes avec les valeurs des contrôles WEB
        //* --------------------------------------------------------------- *//

        #region FillCorporateActionClass
        /// <summary>
        /// ● Alimentation de la classe CorporateAction
        ///   MarketIdentification : Marché
        ///   SpheresCommonIdentification : Identifiant, Displayname, description
        ///   PubDate, ReadyState
        ///   RefNoticeIdentification : Notice principale
        ///   RefNoticeIdentification[] : Notices additionnelles
        ///   
        /// ● Appel à alimentation de la classe CorporateEvent
        /// </summary>
        /// <returns>item de type CorporateAction</returns>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// EG 20211020 [XXXXX] Nouvelle gestion des notices (URLNOTICE et USEURLNOTICE)
        /// EG 20211028 [XXXXX] Relooking Formulaire de CA (introduction de TogglePanel à la place des tables) 
        private Cst.ErrLevel FillCorporateActionClass(CorporateAction pCorporateAction)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
            if (null != _market)
            {
                pCorporateAction.market = SetMarket(m_DicMarket, txtCAMarket);
                pCorporateAction.identifierSpecified = StrFunc.IsFilled(txtCAIdentifier.Text);
                pCorporateAction.identifier = txtCAIdentifier.Text;
                pCorporateAction.displaynameSpecified = StrFunc.IsFilled(txtCADisplayName.Text);
                pCorporateAction.displayname = txtCADisplayName.Text;
                pCorporateAction.descriptionSpecified = StrFunc.IsFilled(txtCADocumentation.Text);
                pCorporateAction.description = txtCADocumentation.Text;
                pCorporateAction.pubDate = new EFS_Date(txtCAPubDate.Text).DateValue;
                pCorporateAction.readystate = CATools.CAReadyState(ddlCAReadyState.SelectedValue).Value;
                Nullable<CfiCodeCategoryEnum> cfiCode = CATools.CACfiCodeCategory(ddlCACfiCodeCategory.SelectedValue);
                pCorporateAction.cfiCodeSpecified = cfiCode.HasValue;
                if (pCorporateAction.cfiCodeSpecified)
                    pCorporateAction.cfiCode = cfiCode.Value;
                pCorporateAction.refNotice = new RefNoticeIdentification
                {
                    value = txtCARefNotice.Text,
                    pubDate = pCorporateAction.pubDate,
                    fileName = txtCARefNoticeFileName.Text
                };

                pCorporateAction.urlnotice = txtCAURLNotice.Text;
                pCorporateAction.refNotice.useurlnoticeSpecified = StrFunc.IsFilled(txtCAURLNotice.Text);
                pCorporateAction.refNotice.useurlnotice = chkCAUseURLNotice.Checked;
                #region Notices additionnelles
                pCorporateAction.refNoticeAddSpecified = (0 < plhCARefNoticeAdds.Controls.Count);
                if (pCorporateAction.refNoticeAddSpecified)
                {
                    WCTogglePanel toggleRefNotice = plhCARefNoticeAdds.Controls[0] as WCTogglePanel;
                    if (toggleRefNotice != default(WCTogglePanel))
                    {
                        if (toggleRefNotice.ControlBody.Controls[0] is Panel pnlRefNotice)
                        {
                            int i = 1;
                            ArrayList aRefNoticeAdd = new ArrayList();
                            foreach (Panel notice in pnlRefNotice.Controls)
                            {
                                RefNoticeIdentification _refNotice = new RefNoticeIdentification();
                                if ((FindControl(CATools.pfxTXTNoticeAdd + "Reference" + i.ToString()) is WCTextBox2 txt) && StrFunc.IsFilled(txt.Text))
                                {
                                    _refNotice.value = txt.Text;
                                    txt = FindControl(CATools.pfxTXTNoticeAdd + "PubDate" + i.ToString()) as WCTextBox2;
                                    if (null != txt)
                                    {
                                        if (StrFunc.IsFilled(txt.Text))
                                            _refNotice.pubDate = new EFS_Date(txt.Text).DateValue;
                                        else
                                            _refNotice.pubDate = pCorporateAction.pubDate;
                                    }
                                    txt = FindControl(CATools.pfxTXTNoticeAdd + "FileName" + i.ToString()) as WCTextBox2;
                                    if (null != txt)
                                        _refNotice.fileName = txt.Text;

                                    _refNotice.useurlnoticeSpecified = pCorporateAction.refNotice.useurlnoticeSpecified;
                                    _refNotice.useurlnotice = pCorporateAction.refNotice.useurlnotice;
                                    aRefNoticeAdd.Add(_refNotice);
                                    i++;
                                }
                            }
                            pCorporateAction.refNoticeAdd = (RefNoticeIdentification[])aRefNoticeAdd.ToArray(typeof(RefNoticeIdentification));
                        }
                    }
                }
                #endregion Notices additionnelles

                ret = FillCorporateEventClass(pCorporateAction.corporateEvent[0]);

                #region Scripts SQL attachés
                // EG 20140518 [19913]
                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    m_CorporateAction.corporateDocsSpecified = (0 < m_CorporateDocs.Count);
                    if (m_CorporateAction.corporateDocsSpecified)
                        m_CorporateAction.corporateDocs = m_CorporateDocs;
                }
                #endregion Scripts SQL attachés

            }
            return ret;
        }
        #endregion FillCorporateActionClass
        #region FillCorporateEventClass
        /// <summary>
        /// ● Alimentation de la classe CorporateEvent
        ///   Group, Type (+ Type combiné), Mode, Method, dates etc
        /// ● Appel à alimentation de la classe CorporateEventProcedure
        /// </summary>
        /// <returns>item de type CorporateEvent</returns>
        protected Cst.ErrLevel FillCorporateEventClass(CorporateEvent pCorporateEvent)
        {
            pCorporateEvent.group = CATools.CEGroup(ddlCEGroup.SelectedValue).Value;
            pCorporateEvent.type = CATools.CEType(ddlCEType.SelectedValue).Value;

            Nullable<CombinationOperandEnum> operand = CATools.CEOperand(ddlCECombinedOperand.SelectedValue);
            pCorporateEvent.operandSpecified = operand.HasValue;
            if (operand.HasValue)
                pCorporateEvent.operand = operand.Value;

            Nullable<CorporateEventTypeEnum> ceType = CATools.CEType(ddlCECombinedType.SelectedValue);
            pCorporateEvent.combinedTypeSpecified = ceType.HasValue;
            if (ceType.HasValue)
                pCorporateEvent.combinedType = ceType.Value;

            Nullable<FixML.Enum.SettlSessIDEnum> ceMode = CATools.CEMode(ddlCEMode.SelectedValue);
            if (ceMode.HasValue)
                pCorporateEvent.mode = ceMode.Value;

            pCorporateEvent.identifierSpecified = StrFunc.IsFilled(txtCEIdentifier.Text);
            pCorporateEvent.identifier = txtCEIdentifier.Text;
            pCorporateEvent.exDateSpecified = StrFunc.IsFilled(txtCEExDate.Text);
            pCorporateEvent.exDate = new EFS_Date(txtCEExDate.Text).DateValue;
            pCorporateEvent.effectiveDateSpecified = StrFunc.IsFilled(txtCEEffectiveDate.Text);
            pCorporateEvent.effectiveDate = new EFS_Date(txtCEEffectiveDate.Text).DateValue;

            pCorporateEvent.execOrder = 1;

            pCorporateEvent.adjMethod = CATools.CEMethod(ddlCEAdjMethod.SelectedValue).Value;

            switch (m_CAInfo.Mode)
            {
                case Cst.Capture.ModeEnum.New:
                    FillCorporateEventProcedureClass(pCorporateEvent);
                    break;
                case Cst.Capture.ModeEnum.Update:
                    CorporateEventProcedure procedure = ActiveCEProcedure;
                    pCorporateEvent.procedure = procedure;
                    FillUnderlyers(pCorporateEvent.procedure);
                    FillCorporateComponents(pCorporateEvent.procedure);
                    break;
            }

            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FillCorporateEventClass
        #region FillCorporateEventProcedureClass
        /// <summary>
        /// ● Alimentation de la classe CorporateEventProcedure
        ///   sous-jacent(s)
        ///   procédure : par sérialisation et clonage de la procédure de TEMPLATE actif
        /// </summary>
        /// <returns>item de type CorporateEvent</returns>
        protected Cst.ErrLevel FillCorporateEventProcedureClass(CorporateEvent pCorporateEvent)
        {
            CorporateEventProcedure procedure = ActiveCEProcedure;
            if (null != procedure)
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(procedure.GetType(), procedure);
                CorporateEventProcedure cloneProcedure = (CorporateEventProcedure)CacheSerializer.CloneDocument(serializeInfo);
                cloneProcedure.adjustment.templateFileName = procedure.adjustment.templateFileName;
                cloneProcedure.adjustment.templateFileName_AI = procedure.adjustment.templateFileName_AI;
                cloneProcedure.adjustment.templateFileName_CT = procedure.adjustment.templateFileName_CT;

                #region Mise à jour des SOUS-JACENTs
                FillUnderlyers(cloneProcedure);
                #endregion Mise à jour des SOUS-JACENTs

                #region Mise à jour des COMPOSANTS SIMPLES
                FillCorporateComponents(cloneProcedure);
                #endregion Mise à jour des COMPOSANTS SIMPLES

                pCorporateEvent.procedure = cloneProcedure;
                pCorporateEvent.procedureSpecified = true;
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FillCorporateEventProcedureClass


        #region FillCorporateComponents
        /// <summary>
        /// ● Alimentation des composants
        /// </summary>
        /// <returns>item de type CorporateEvent</returns>
        // EG [33415/33420] Gestion ResultType.info
        // EG 20140317 [19722] Gestion ResultType.check
        // EG 20141106 [20253] Equalization payment
        protected Cst.ErrLevel FillCorporateComponents(CorporateEventProcedure pProcedure)
        {
            #region Mise à jour des COMPOSANTS SIMPLES
            FillCorporateComponents(pProcedure, m_ListComponentSimples);
            FillCorporateComponents(pProcedure, m_ListComponentSimples_AI);
            #endregion Mise à jour des COMPOSANTS SIMPLES

            if (pProcedure.adjustment is AdjustmentRatio ratio)
            {

                #region Ratio Method
                // RFactor
                SetRounding(ratio.rFactor, AdjustmentElementEnum.RFactor);
                // RFactorCertified
                WCTextBox2 txtRFactorCertified = FindControl(CATools.pfxTXTComponent + CATools.RFactorCertified_Id) as WCTextBox2;
                ratio.rFactor.rFactorCertifiedSpecified = (null != txtRFactorCertified) && StrFunc.IsFilled(txtRFactorCertified.Text);
                if (ratio.rFactor.rFactorCertifiedSpecified)
                {
                    decimal _value = DecFunc.DecValue(txtRFactorCertified.Text, Thread.CurrentThread.CurrentCulture);
                    ratio.rFactor.rFactorCertifiedSpecified = (0 < _value);
                    if (ratio.rFactor.rFactorCertifiedSpecified)
                    {
                        ratio.rFactor.rFactorCertified = new ComponentSimple
                        {
                            name = CATools.RFactorCertified_Name,
                            Id = CATools.RFactorCertified_Id,
                            description = CATools.RFactorCertified_Description,
                            descriptionSpecified = true,
                            resultSpecified = true,
                            result = new Result
                            {
                                itemsElementName = ResultType.unit,
                                result = new SimpleUnit(_value, _value)
                            }
                        };
                    }
                }

                // Future
                ratio.contract.futureSpecified = chkCEAdjFutureCSize.Checked || chkCEAdjFuturePrice.Checked || chkCEAdjFutureEqualisationPayment.Checked;
                if (ratio.contract.futureSpecified)
                {
                    AdjustmentFuture future = ratio.contract.future;
                    future.contractSizeSpecified = chkCEAdjFutureCSize.Checked;
                    if (future.contractSizeSpecified)
                        SetRounding(future.contractSize, AdjustmentElementEnum.ContractSize);
                    future.contractMultiplierSpecified = chkCEAdjFutureCSize.Checked;
                    SetRounding(future.contractMultiplier, AdjustmentElementEnum.ContractMultiplier);
                    future.priceSpecified = chkCEAdjFuturePrice.Checked;
                    if (future.priceSpecified)
                        SetRounding(future.price, AdjustmentElementEnum.Price);
                    future.equalisationPaymentSpecified = chkCEAdjFutureEqualisationPayment.Checked;
                    if (future.equalisationPaymentSpecified)
                    {
                        future.equalisationPayment = new EqualisationPayment();
                        SetRounding(future.equalisationPayment, AdjustmentElementEnum.EqualisationPayment);
                    }

                }
                // Option
                ratio.contract.optionSpecified = chkCEAdjOptionCSize.Checked || chkCEAdjOptionStrikePrice.Checked ||
                    chkCEAdjOptionPrice.Checked || chkCEAdjOptionEqualisationPayment.Checked;
                if (ratio.contract.optionSpecified)
                {
                    AdjustmentOption option = ratio.contract.option;
                    option.contractSizeSpecified = chkCEAdjOptionCSize.Checked;
                    if (option.contractSizeSpecified)
                        SetRounding(option.contractSize, AdjustmentElementEnum.ContractSize);
                    option.contractMultiplierSpecified = chkCEAdjOptionCSize.Checked;
                    if (option.contractMultiplierSpecified)
                        SetRounding(option.contractMultiplier, AdjustmentElementEnum.ContractMultiplier);
                    option.strikePriceSpecified = chkCEAdjOptionStrikePrice.Checked;
                    if (option.strikePriceSpecified)
                        SetRounding(option.strikePrice, AdjustmentElementEnum.StrikePrice);
                    option.priceSpecified = chkCEAdjOptionPrice.Checked;
                    if (option.priceSpecified)
                        SetRounding(option.price, AdjustmentElementEnum.Price);
                    option.equalisationPaymentSpecified = chkCEAdjOptionEqualisationPayment.Checked;
                    if (option.equalisationPaymentSpecified)
                    {
                        option.equalisationPayment = new EqualisationPayment();
                        SetRounding(option.equalisationPayment, AdjustmentElementEnum.EqualisationPayment);
                    }
                }
                #endregion Ratio Method
            }
            else if (pProcedure.adjustment is AdjustmentFairValue)
            {
                // TBD
            }
            else if (pProcedure.adjustment is AdjustmentPackage)
            {
                // TBD
            }
            return Cst.ErrLevel.SUCCESS;
        }
        protected Cst.ErrLevel FillCorporateComponents(CorporateEventProcedure pProcedure, List<ComponentSimple> pListComponent)
        {
            #region Mise à jour des COMPOSANTS SIMPLES
            pListComponent.ForEach(item =>
            {
                if (ReflectionTools.GetObjectById(pProcedure, item.Id) is ComponentSimple component)
                {
                    WCDropDownList2 ddlComponent = FindControl(CATools.pfxDDLComponent + item.Id) as WCDropDownList2;
                    if (FindControl(CATools.pfxTXTComponent + item.Id) is WCTextBox2 txtComponent)
                    {
                        component.resultSpecified = StrFunc.IsFilled(txtComponent.Text);
                        if (component.resultSpecified)
                        {
                            if (null == component.result)
                                component.InitializeResult();

                            decimal _amount = 0;
                            switch (component.result.itemsElementName)
                            {
                                case ResultType.amount:
                                    _amount = DecFunc.DecValue(txtComponent.Text, Thread.CurrentThread.CurrentCulture);
                                    component.result.result = new Money(_amount, _amount, ddlComponent.SelectedValue);
                                    break;
                                case ResultType.unit:
                                    _amount = DecFunc.DecValue(txtComponent.Text, Thread.CurrentThread.CurrentCulture);
                                    component.result.result = new SimpleUnit(_amount, _amount);
                                    break;
                                case ResultType.info:
                                    if (null == component.result)
                                        component.result = new Result();
                                    component.result.result = txtComponent.Text;
                                    break;
                            }
                        }
                    }
                    else if (FindControl(CATools.pfxCHKComponent + item.Id) is WCCheckBox2 chkComponent)
                    {
                        component.resultSpecified = chkComponent.Checked;
                        if (component.resultSpecified)
                        {
                            if (null == component.result)
                                component.InitializeResult();
                            component.result.result = chkComponent.Checked;
                        }
                    }
                }
            });
            #endregion Mise à jour des COMPOSANTS SIMPLES
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FillCorporateComponents
        #region FillUnderlyers
        /// <summary>
        /// ● Alimentation de la classe CorporateEventProcedure
        ///   sous-jacent(s)
        ///   procédure : par sérialisation et clonage de la procédure de TEMPLATE actif
        /// </summary>
        /// <returns>item de type CorporateEvent</returns>
        // EG [33415/33420]
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected Cst.ErrLevel FillUnderlyers(CorporateEventProcedure pProcedure)
        {
            CorporateEventUnderlyer underlyer = pProcedure.underlyers[0];
            underlyer.category = (Cst.UnderlyingAsset_ETD)Enum.Parse(typeof(Cst.UnderlyingAsset_ETD), ddlCEUNLCategory.SelectedValue, false);
            underlyer.market = SetMarket(m_DicUNLMarket, txtCEUNLMarket);
            underlyer.marketSpecified = true;

            underlyer.identifierSpecified = StrFunc.IsFilled(txtCEUNLIdentifier.Text);
            underlyer.identifier = txtCEUNLIdentifier.Text;

            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
            {
                underlyer.caIssueCodeSpecified = StrFunc.IsFilled(txtCEUNLCode.Text);
                underlyer.caIssueCode = txtCEUNLCode.Text;
            }
            else
            {
                underlyer.isinCodeSpecified = StrFunc.IsFilled(txtCEUNLCode.Text);
                underlyer.isinCode = txtCEUNLCode.Text;
            }
            if (txtCEUNLCode2.Visible && StrFunc.IsFilled(txtCEUNLCode2.Text))
            {
                CorporateEventUnderlyer underlyer2;
                if (1 == pProcedure.underlyers.Length)
                    underlyer2 = new CorporateEventUnderlyer();
                else
                    underlyer2 = pProcedure.underlyers[1];

                underlyer2.category = (Cst.UnderlyingAsset_ETD)Enum.Parse(typeof(Cst.UnderlyingAsset_ETD), ddlCEUNLCategory2.SelectedValue, false);
                underlyer2.market = SetMarket(m_DicUNLMarket, txtCEUNLMarket2);
                underlyer2.marketSpecified = true;
                underlyer2.identifierSpecified = StrFunc.IsFilled(txtCEUNLIdentifier2.Text);
                underlyer2.identifier = txtCEUNLIdentifier2.Text;
                if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTIONISSUE)
                {
                    underlyer2.caIssueCodeSpecified = StrFunc.IsFilled(txtCEUNLCode2.Text);
                    underlyer2.caIssueCode = txtCEUNLCode2.Text;
                }
                else
                {
                    underlyer2.isinCodeSpecified = StrFunc.IsFilled(txtCEUNLCode2.Text);
                    underlyer2.isinCode = txtCEUNLCode2.Text;
                }

                if (1 == pProcedure.underlyers.Length)
                    pProcedure.AddUnderlyer(underlyer2);
            }
            else
            {
                ReflectionTools.RemoveItemInArray(pProcedure, "underlyers", 1);
            }
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FillUnderlyers

        #region SetMarket
        // EG [33415/33420]
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        private MarketIdentification SetMarket(Dictionary<int, MarketIdentification> pDicMarket, TextBox pTxtMarket)
        {
            MarketIdentification _marketSource = GetMarketIdentificationByIdentifier(pDicMarket, pTxtMarket.Text);
            MarketIdentification _marketTarget = new MarketIdentification();
            if (m_CAInfo.CATable == Cst.OTCml_TBL.CORPOACTION)
            {
                // EG [33415/33420]
                _marketTarget.IdM = _marketSource.IdM;
                _marketTarget.identifierSpecified = _marketSource.identifierSpecified;
                _marketTarget.identifier = _marketSource.identifier;
                _marketTarget.displaynameSpecified = _marketSource.displaynameSpecified;
                _marketTarget.displayname = _marketSource.displayname;
                _marketTarget.descriptionSpecified = _marketSource.descriptionSpecified;
                _marketTarget.description = _marketSource.description;
            }
            _marketTarget.ISO10383_ALPHA4Specified = true;
            _marketTarget.ISO10383_ALPHA4 = _marketSource.ISO10383_ALPHA4;
            _marketTarget.idBCSpecified = _marketSource.idBCSpecified;
            _marketTarget.idBC = _marketSource.idBC;
            _marketTarget.marketType = _marketSource.marketType;
            _marketTarget.exchangeSymbolSpecified = _marketSource.exchangeSymbolSpecified;
            _marketTarget.exchangeSymbol = _marketSource.exchangeSymbol;

            _marketTarget.FIXML_SecurityExchangeSpecified = true;
            _marketTarget.FIXML_SecurityExchange = _marketSource.FIXML_SecurityExchange;

            return _marketTarget;
        }
        #endregion SetMarket
        //* --------------------------------------------------------------- *//
        // Actions sur les boutons de la barre d'outils
        //* --------------------------------------------------------------- *//

        #region OnAction
        /// <summary>
        /// ► Actions sur boutons de la barre d'outils
        ///   Enregistrement (btnRecord)
        ///   Suppression (btnRemove)
        ///   Annulation (btnCancel)
        ///   Duplication (btnDuplicate)
        ///   Envoi de message à NormMsgFactory (btnSend)
        ///   
        ///     ● 1er passage  : Test de validité des contrôles (sauf btnCancel) et message de confirmation
        ///     ● 2ème passage : Si validation exécution de la demande
        ///   
        ///   Affichage du message au format NormMsgFactory (btnSeeMsg)
        ///   Affichage de la procédure d'ajustement (btnSeeProcedure)
        ///   Affichage du template source (btnSeeTemplate)
        ///
        ///     ● 1 seul passage  : Test de validité des contrôles si validation exécution de la demande
        /// </summary>
        protected void OnAction(object sender, CommandEventArgs e)
        {
            string eventTarget = Request.Params["__EVENTTARGET"];
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            WebControl ctrl = sender as WebControl;
            bool isValid = false;
            if (StrFunc.IsFilled(eventTarget))
            {
                string message = string.Empty;
                if (StrFunc.IsEmpty(eventArgument))
                {
                    switch (eventTarget)
                    {
                        case "btnRefresh":
                            isValid = true;
                            message = Ressource.GetString2("Msg_CARefresh", txtCAIdentifier.Text, m_CAInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnRecord":
                            isValid = IsAllValid();
                            if (m_CAInfo.Id.HasValue)
                                message = Ressource.GetString2("Msg_CARecord", txtCAIdentifier.Text, m_CAInfo.Id.Value.ToString()) + Cst.CrLf;
                            else
                                message = Ressource.GetString2("Msg_CARecordNew", txtCAIdentifier.Text) + Cst.CrLf;
                            break;
                        case "btnRemove":
                            isValid = IsAllValid();
                            message = Ressource.GetString2("Msg_CARemove", txtCAIdentifier.Text, m_CAInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnCancel":
                            isValid = true;
                            message = Ressource.GetString("Msg_CAReset") + Cst.CrLf;
                            break;
                        case "btnDuplicate":
                            isValid = true;
                            message = Ressource.GetString2("Msg_CADuplicate", txtCAIdentifier.Text, m_CAInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnSend":
                            isValid = IsAllValid();
                            if (m_CAInfo.Id.HasValue)
                                message = Ressource.GetString2("Msg_CASend", txtCAIdentifier.Text, m_CAInfo.Id.Value.ToString()) + Cst.CrLf;
                            else
                                message = Ressource.GetString2("Msg_CASendNew", txtCAIdentifier.Text) + Cst.CrLf;
                            break;
                        case "btnSeeMsg":
                            isValid = IsAllValid();
                            if (isValid)
                                DisplayNormMsg_Message();
                            break;
                        case "btnSeeProcedure":
                            isValid = IsAllValid();
                            if (isValid)
                                DisplayXML_Procedure();
                            break;
                        case "btnCETemplate":
                            isValid = true;
                            DisplayXML_Template();
                            break;
                        case "btnCETemplate_AI":
                            isValid = true;
                            DisplayXML_Template_AI();
                            break;
                    }
                    if (isValid && StrFunc.IsFilled(message))
                        JavaScript.ConfirmStartUpImmediate(this, message, ctrl.ID, "TRUE", "FALSE");
                }
                else if (eventArgument == "TRUE")
                {
                    switch (m_CAInfo.CATable)
                    {
                        case Cst.OTCml_TBL.CORPOACTION:
                            OnActionCAEmbedded(sender, e);
                            break;
                        case Cst.OTCml_TBL.CORPOACTIONISSUE:
                            OnActionCAIssue(sender, e);
                            break;
                    }
                }
            }
        }
        #endregion OnAction

        #region OnOpenFile
        // EG 20140518 [19913]
        protected void OnOpenFile(object sender, CommandEventArgs e)
        {
            WebControl ctrl = sender as WebControl;
            WCTextBox txtFileName = FindControl(ctrl.ID.Replace("Open", "File").Replace("btn", "txt")) as WCTextBox;
            Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = GetUpLoadKey(txtFileName);
            if (null != _key)
            {
                CorporateDoc _corporateDoc = m_CorporateDocs.GetCorporateDoc(_key);
                if (null != _corporateDoc)
                {
                    string fileName = _corporateDoc.FullPathFileName(SessionTools.TemporaryDirectory.PathMapped);
                    if (false == File.Exists(fileName))
                        _corporateDoc.SaveToTemporaryDirectory(SessionTools.TemporaryDirectory.PathMapped);
                    FileInfo file = new FileInfo(fileName);
                    if (Response.IsClientConnected)
                    {
                        Response.Clear();
                        Response.AppendHeader("Content-Disposition", "inline; filename=" + '"' + file.Name + '"');
                        Response.ContentType = "application/octet-stream";
                        Response.TransmitFile(file.FullName);
                        Response.End();
                    }
                }
            }
        }
        #endregion OnOpenFile
        #region OnUpload
        // EG 20140518 [19913]
        // EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected void OnUpload(object sender, CommandEventArgs e)
        {
            MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
            if (null != _market)
            {
                WebControl ctrl = sender as WebControl;
                WCTextBox txtFileName = FindControl(ctrl.ID.Replace("Upload", "File").Replace("btn", "txt")) as WCTextBox;
                Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = GetUpLoadKey(ctrl);
                if (null != _key)
                {
                    if (fileUpload.HasFile)
                    {
                        string FIXML_SecurityExchange = _market.FIXML_SecurityExchange;
                        m_CorporateDocs.SetCorporateDoc(_key, null, FIXML_SecurityExchange, txtCARefNotice.Text, fileUpload.FileBytes);
                        txtFileName.Text = m_CorporateDocs.GetCorporateDoc(_key).identifier;
                    }
                    SetUploadControl(_key.First, _key.Second);
                }
            }
        }
        #endregion OnUpload
        #region OnDeleteFile
        // EG 20140518 [19913]
        protected void OnDeleteFile(object sender, CommandEventArgs e)
        {
            WebControl ctrl = sender as WebControl;
            WCTextBox txtFileName = FindControl(ctrl.ID.Replace("Delete", "File").Replace("btn", "txt")) as WCTextBox;
            Pair<CATools.DOCTypeEnum, CATools.SQLRunTimeEnum> _key = GetUpLoadKey(ctrl);
            if (null != _key)
            {
                m_CorporateDocs.DeleteCorporateDoc(_key);
                txtFileName.Text = "No script";
                SetUploadControl(_key.First, _key.Second);
            }
        }
        #endregion OnDeleteFile


        #region OnActionCAIssue
        /// ► Corporate actions de type PUBLIEES
        ///   Exécution de la demande suite à actions sur boutons de la barre d'outils (2ème passage)
        ///   Enregistrement (btnRecord)
        ///     ● Enregistrement
        ///   Suppression (btnRemove)
        ///   Annulation (btnCancel)
        ///   Duplication (btnDuplicate)
        ///   Envoi de message à NormMsgFactory (btnSend)
        /// </summary>
        protected void OnActionCAIssue(object sender, CommandEventArgs e)
        {
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            if ((sender is WebControl ctrl) && (eventArgument == "TRUE"))
            {
                Cst.ErrLevel ret;
                string resMessage;
                switch (ctrl.ID)
                {
                    case "btnRefresh":
                        Refresh();
                        break;
                    case "btnRecord":
                        resMessage = m_CAInfo.Id.HasValue ? "Msg_CAUpdated" : "Msg_CAInserted";
                        ret = RecordCAIssue(CorporateActionEmbeddedStateEnum.NEWEST);
                        if (Cst.ErrLevel.SUCCESS != ret)
                        {
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                            resMessage += ErrLevelForAlertImmediate.ToString();
                        }

                        MsgForAlertImmediate = Ressource.GetString2(resMessage, txtCAIdentifier.Text, m_CorporateAction.spheresid);

                        if (Cst.ErrLevel.SUCCESS == ret)
                            ResetAll();
                        break;
                    case "btnRemove":
                        resMessage = "Msg_CARemoved";
                        if (null != m_CorporateAction)
                        {
                            ret = m_CorporateAction.Delete();
                            if (Cst.ErrLevel.SUCCESS != ret)
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                resMessage += ErrLevelForAlertImmediate.ToString();
                            }
                            MsgForAlertImmediate = Ressource.GetString2(resMessage, txtCAIdentifier.Text, m_CorporateAction.IdCA.ToString());

                            if (Cst.ErrLevel.SUCCESS == ret)
                                ClosePage();
                        }
                        break;
                    case "btnDuplicate":
                        Duplicate();
                        break;
                    case "btnSend":
                        resMessage = m_CAInfo.Id.HasValue ? "Msg_CAMessageSended" : "Msg_CAInsertedAndMessageSended";
                        ret = RecordCAIssue(CorporateActionEmbeddedStateEnum.REQUESTED);
                        if (Cst.ErrLevel.SUCCESS == ret)
                        {
                            ret = SendNormMsgCAIssue();
                            if (Cst.ErrLevel.SUCCESS != ret)
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                resMessage = m_CAInfo.Id.HasValue ? "Msg_CAMessageNotSended" : "Msg_CAInsertedAndMessageNotSended";
                            }
                            else
                            {
                                m_CAInfo.Id = m_CorporateAction.IdCA;
                                m_CAInfo.Mode = Cst.Capture.ModeEnum.Update;
                                Refresh();
                            }
                        }
                        else
                        {
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                            resMessage += ErrLevelForAlertImmediate.ToString();
                        }
                        MsgForAlertImmediate = Ressource.GetString2(resMessage, txtCAIdentifier.Text, m_CorporateAction.spheresid);
                        break;
                    case "btnCancel":
                        ResetAll();
                        break;
                }
            }
        }
        #endregion OnActionCAIssue
        #region OnActionCAEmbedded
        /// ► Corporate actions de type INTEGREES
        ///   Exécution de la demande suite à actions sur boutons de la barre d'outils (2ème passage)
        ///   ● Enregistrement (btnRecord)
        ///   ● Annulation (btnCancel)
        /// </summary>
        protected void OnActionCAEmbedded(object sender, CommandEventArgs e)
        {
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            if ((sender is WebControl ctrl) && (eventArgument == "TRUE"))
            {
                switch (ctrl.ID)
                {
                    case "btnRefresh":
                        Refresh();
                        break;
                    case "btnRecord":
                        string resMessage = "Msg_CAUpdated";
                        Cst.ErrLevel ret = RecordCAEmbedded();
                        if (Cst.ErrLevel.SUCCESS != ret)
                        {
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                            resMessage += ErrLevelForAlertImmediate.ToString();
                        }
                        MsgForAlertImmediate = Ressource.GetString2(resMessage, txtCAIdentifier.Text, m_CorporateAction.spheresid);
                        break;
                    case "btnCancel":
                        ClosePage();
                        break;
                }
            }
        }
        #endregion OnActionCAEmbedded
        #region OnAdjustmentByRatio
        /// <summary>
        /// Calcul du ratio d'ajustement (Simulation)
        /// </summary>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected void OnAdjustmentByRatio(object sender, CommandEventArgs e)
        {
            CorporateEvent _corporateEvent = m_CorporateAction.corporateEvent[0];
            FillCorporateComponents(_corporateEvent.procedure);

            MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
            if (null != _market)
            {
                CorporateEventProcedure procedure = _corporateEvent.procedure;
                AdjustmentRatio ratio = procedure.adjustment as AdjustmentRatio;
                ProcessCacheContainer cacheContainer = new ProcessCacheContainer(SessionTools.CS, Tools.GetNewProductBase());
                cacheContainer.SetCACumDate(_corporateEvent.effectiveDate, _market.idBC);
                Cst.ErrLevel ret = ratio.rFactor.Evaluate(ratio, procedure.underlyers, SessionTools.User.Entity_IdA, _market.SpheresId, cacheContainer, true);
                if (Cst.ErrLevel.SUCCESS != ret)
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                else
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.SUCCESS;
                MsgForAlertImmediate = ratio.rFactor.Message;
            }
        }
        #endregion AdjustmentByRatio

        /// <summary>
        /// (Future|Option)Soulte (activer|désactiver et checker (Future|Option)Price )
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20150218 [207775]
        public void OnCheckedChanged_EqualisationPayment(object sender, EventArgs e)
        {
            if (sender is WCCheckBox2 chk)
            {
                switch (chk.ID)
                {
                    case "chkCEAdjFutureEqualisationPayment":
                        chkCEAdjFuturePrice.Enabled = (false == chkCEAdjFutureEqualisationPayment.Checked);
                        if (chkCEAdjFutureEqualisationPayment.Checked)
                            chkCEAdjFuturePrice.Checked = true;
                        break;
                    case "chkCEAdjOptionEqualisationPayment":
                        chkCEAdjOptionPrice.Enabled = (false == chkCEAdjOptionEqualisationPayment.Checked);
                        if (chkCEAdjOptionEqualisationPayment.Checked)
                            chkCEAdjOptionPrice.Checked = true;
                        break;
                }
            }
        }
        
        #region DisplayXML_Template
        /// <summary>
        /// ● Affichage de la procédure d'ajustement d'ORIGINE (celle du template actif)
        /// </summary>
        protected void DisplayXML_Template()
        {
            List<Pair<string, CorporateEventProcedure>> lstTemplates = ListTemplatesAttached;
            if ((null != lstTemplates) && (0 < lstTemplates.Count))
            {
                CorporateEventProcedure procedure = lstTemplates.Find(match => match.First == ddlCETemplate.SelectedValue).Second;
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(procedure.GetType(), procedure);
                StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());
                DisplayXml("CorporateActions", "CASource.xml", sb.ToString());
            }
        }
        #endregion DisplayXML_Template
        #region DisplayXML_Template_AI
        /// <summary>
        /// ● Affichage de la procédure d'ajustement d'ORIGINE (celle du template actif)
        /// </summary>
        protected void DisplayXML_Template_AI()
        {
            AdditionalInfos ai = m_DicTemplates_Additional["AI"].Find(match => match.First == ddlCETemplate_AI.SelectedValue).Second as AdditionalInfos;
            EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(ai.GetType(), ai);
            StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());
            DisplayXml("CorporateActions", "CASource_AI.xml", sb.ToString());
        }
        #endregion DisplayXML_Template_AI
        #region DisplayXML_Procedure
        /// <summary>
        /// ● Affichage de la procédure d'ajustement FINALE
        ///   Alimentation des classes
        ///   Construction et sérialisation
        ///   Affichage du fichier résultat
        /// </summary>
        private void DisplayXML_Procedure()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            CorporateAction _corporateAction = null;
            switch (m_CAInfo.Mode)
            {
                case Cst.Capture.ModeEnum.New:
                    _corporateAction = new CorporateAction(SessionTools.CS)
                    {
                        corporateEvent = new CorporateEvent[1] { new CorporateEvent() }
                    };
                    ret = FillCorporateEventProcedureClass(_corporateAction.corporateEvent[0]);
                    break;
                case Cst.Capture.ModeEnum.Update:
                    _corporateAction = m_CorporateAction;
                    FillUnderlyers(_corporateAction.corporateEvent[0].procedure);
                    ret = FillCorporateComponents(_corporateAction.corporateEvent[0].procedure);
                    break;
            }
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                CorporateEventProcedure procedure = _corporateAction.corporateEvent[0].procedure;
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(procedure.GetType(), procedure);
                StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());
                DisplayXml("CorporateActions", "CAFinal.xml", sb.ToString());
            }
        }
        #endregion DisplayXML_Procedure
        #region DisplayNormMsg_Message
        /// <summary>
        /// ● Affichage du message destiné au service NormMsgFactory
        ///   Alimentation des classes
        ///   Construction et sérialisation du message
        ///   Affichage du fichier résultat
        /// </summary>
        private void DisplayNormMsg_Message()
        {
            if (m_CAInfo.Mode == Cst.Capture.ModeEnum.New)
            {
                m_CorporateAction = new CorporateAction(SessionTools.CS)
                {
                    corporateEvent = new CorporateEvent[1] { new CorporateEvent() }
                };
            }
            Cst.ErrLevel ret = FillCorporateActionClass(m_CorporateAction);
            if (Cst.ErrLevel.SUCCESS == ret)
            {
                m_CorporateAction.ConstructNormMsgFactoryMessage();
                if (null != m_CorporateAction.normMsgFactoryMQueue)
                {
                    EFS_SerializeInfoBase serializeInfo =
                        new EFS_SerializeInfoBase(m_CorporateAction.normMsgFactoryMQueue.GetType(), m_CorporateAction.normMsgFactoryMQueue);
                    StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());
                    DisplayXml("CorporateActions", "NMFFinal.xml", sb.ToString());
                }
            }
        }
        #endregion DisplayNormMsg_Message
        #region ResetAll
        /// <summary>
        /// Remise à zéro de la page 
        /// </summary>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected void Duplicate()
        {
            if (null != m_CorporateAction)
            {
                //m_CorporateAction = ReflectionTools.Clone(m_CorporateAction, ReflectionTools.CloneStyle.CloneField) as CorporateAction;
                m_CorporateAction.market = new MarketIdentification();
                m_CorporateAction.refNotice = new RefNoticeIdentification();
                m_CorporateAction.refNoticeAddSpecified = false;
                m_CorporateAction.refNoticeAdd = null;
                m_CorporateAction.readystate = CorporateActionReadyStateEnum.PUBLISHED;
                txtCAMarket.Text = string.Empty;
                m_CorporateAction.IdCA = 0;
                m_CorporateAction.corporateEvent[0].IdCE = 0;
                if (m_CorporateAction.corporateEvent[0].procedure.adjustment is AdjustmentRatio)
                {
                    AdjustmentRatio ratio = m_CorporateAction.corporateEvent[0].procedure.adjustment as AdjustmentRatio;
                    if (ratio.rFactor.rFactorCertifiedSpecified)
                    {
                        ratio.rFactor.rFactorCertifiedSpecified = false;
                        ratio.rFactor.rFactorCertified = null;
                    }
                }
                m_CAInfo.Mode = Cst.Capture.ModeEnum.New;
                m_CAInfo.Id = null;
                m_CAInfo.IdM = null;
                SetValue();
                ComplementaryInitialize();
            }
        }
        #endregion ResetAll
        #region RecordCAIssue
        /// <summary>
        /// ● Enregistrement d'une corporate action PUBLIEE (CORPOACTIONISSUE)
        ///   Alimentation des classes
        ///   Construction du message
        ///   Ecriture dans CORPOACTIONISSUE
        /// </summary>
        private Cst.ErrLevel RecordCAIssue(CorporateActionEmbeddedStateEnum pEmbeddedState)
        {
            if (m_CAInfo.Mode == Cst.Capture.ModeEnum.New)
            {
                m_CorporateAction = new CorporateAction(SessionTools.CS)
                {
                    corporateEvent = new CorporateEvent[1] { new CorporateEvent() }
                };
            }
            m_CorporateAction.embeddedState = pEmbeddedState;
            Cst.ErrLevel ret = FillCorporateActionClass(m_CorporateAction);
            if (ret == Cst.ErrLevel.SUCCESS)
                ret = m_CorporateAction.ConstructNormMsgFactoryMessage();
            if (ret == Cst.ErrLevel.SUCCESS)
            {
                if (null != m_CorporateAction.normMsgFactoryMQueue)
                    ret = m_CorporateAction.Write(m_CAInfo, null);
                else
                    ret = Cst.ErrLevel.MESSAGE_NOTFOUND;
            }
            return ret;
        }
        #endregion RecordCAIssue
        #region RecordCAEmbedded
        /// <summary>
        /// ● Mise à jour seulement (pour la création passage par le mode PUBLICATION obligatoire)
        /// ● Enregistrement d'une corporate action INTEGREE (CORPOACTION/CORPOEVENT/CORPOEVENTNOTICE)
        ///   Alimentation des classes
        ///   Construction du message
        ///   Ecriture dans CORPOACTIONISSUE
        ///   
        /// </summary>
        private Cst.ErrLevel RecordCAEmbedded()
        {
            Cst.ErrLevel ret = FillCorporateActionClass(m_CorporateAction);
            if (ret == Cst.ErrLevel.SUCCESS)
                ret = m_CorporateAction.Write(m_CAInfo, m_CorporateAction.idCAIssue);
            return ret;
        }
        #endregion RecordCAEmbedded
        #region Refresh
        /// <summary>
        /// Refresh de la corporate action
        /// </summary>
        protected void Refresh()
        {
            if (m_CAInfo.Id.HasValue)
            {
                LoadCorporateAction();
                ComplementaryInitialize();
                SetValidators();
                // Sauvegarde des contrôles créés dynamiquement
                SavePlaceHolder();
            }
        }
        #endregion Refresh
        #region ResetAll
        /// <summary>
        /// Remise à zéro de la page 
        /// </summary>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected void ResetAll()
        {

            m_CAInfo = new CAInfo(SessionTools.User.IdA);
            m_CorporateAction = null;

            txtCAMarket.Text = string.Empty;
            txtCAIdentifier.Text = string.Empty;
            txtCADisplayName.Text = string.Empty;
            txtCADocumentation.Text = string.Empty;
            txtCARefNotice.Text = string.Empty;
            txtCARefNoticeFileName.Text = string.Empty;
            txtCAPubDate.Text = string.Empty;
            ddlCACfiCodeCategory.SelectedIndex = -1;

            ddlCEUNLCategory.SelectedIndex = -1;
            txtCEIdentifier.Text = string.Empty;
            txtCEExDate.Text = string.Empty;
            txtCEEffectiveDate.Text = string.Empty;
            txtCEIdentifier.Text = string.Empty;
            txtCEUNLMarket.Text = string.Empty;
            txtCEUNLMarket2.Text = string.Empty;
            txtCEUNLIdentifier.Text = string.Empty;
            txtCEUNLCode.Text = string.Empty;

            Control ctrl = FindControl(plhComponents);
            if (null != ctrl)
                ctrl.Controls.Clear();

            ctrl = FindControl(plhComponents_AI);
            if (null != ctrl)
            {
                ctrl.Controls.Clear();
                m_ListComponentSimples_AI.Clear();
            }

            ctrl = FindControl(plhRefNotices);
            if (null != ctrl)
                ctrl.Controls.Clear();

            ddlCERFactorRoundingDir.SelectedIndex = -1;
            ddlCERFactorRoundingPrec.SelectedIndex = -1;
            ddlCEContractSizeRoundingDir.SelectedIndex = -1;
            ddlCEContractSizeRoundingPrec.SelectedIndex = -1;
            ddlCEContractMultiplierRoundingDir.SelectedIndex = -1;
            ddlCEContractMultiplierRoundingPrec.SelectedIndex = -1;
            ddlCEStrikePriceRoundingDir.SelectedIndex = -1;
            ddlCEStrikePriceRoundingPrec.SelectedIndex = -1;
            ddlCEPriceRoundingDir.SelectedIndex = -1;
            ddlCEPriceRoundingPrec.SelectedIndex = -1;

            lblCAEmbeddedState.Text = CorporateActionEmbeddedStateEnum.NEWEST.ToString();
            btnRefresh.Visible = (m_CAInfo.Mode == Cst.Capture.ModeEnum.Update);

            SetDefault();

        }
        #endregion ResetAll

        #region SendNormMsgCAIssue
        /// <summary>
        /// ● Envoi d'un message d'intégration d'une corporate Action vers le service NormMsgFactory 
        ///   Type de process : Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE
        /// </summary>
        protected Cst.ErrLevel SendNormMsgCAIssue()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            m_CorporateAction.normMsgFactoryMQueue.id = m_CorporateAction.IdCA;
            m_CorporateAction.normMsgFactoryMQueue.identifierSpecified = m_CorporateAction.identifierSpecified;
            m_CorporateAction.normMsgFactoryMQueue.identifier = m_CorporateAction.identifier;

            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                process = Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE,
                connectionString = SessionTools.CS,
                Session = SessionTools.AppSession,
                trackerAttrib = new TrackerAttributes()
                {
                    process = Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE,
                    info = TrackerAttributes.BuildInfo(m_CorporateAction.normMsgFactoryMQueue)
                },
                mQueue = new MQueueBase[1] { m_CorporateAction.normMsgFactoryMQueue }
            };

            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);
            var (isOk, _) = MQueueTaskInfo.SendMultiple(taskInfo);
            if (isOk)
                ret = Cst.ErrLevel.SUCCESS;
            return ret;
        }
        #endregion SendNormMsgCAIssue


        //* --------------------------------------------------------------- *//
        // Sauvegarde des données pour maintien après PostBack (VIEWSTATE)
        //* --------------------------------------------------------------- *//

        #region LoadViewState
        /// <summary>
        /// Lecture des groupes/status dans le ViewState
        /// </summary>
        /// <param name="savedState"></param>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override void LoadViewState(object savedState)
        {
            object[] viewState = (object[])savedState;

            base.LoadViewState(viewState[0]);

            m_DicTemplates = (Dictionary<AdjustmentMethodOfDerivContractEnum, Dictionary<string, List<Pair<string, CorporateEventProcedure>>>>)viewState[1];
            m_ListComponentSimples = (List<ComponentSimple>)viewState[2];
            m_DicMarket = (Dictionary<int, MarketIdentification>)viewState[3];
            m_DicMarketRules = (Dictionary<int, CorporateEventMktRules>)viewState[4];
            m_DicUNLMarket = (Dictionary<int, MarketIdentification>)viewState[5];
            m_CAInfo = (CAInfo)viewState[6];
            m_DicTemplates_Additional = (Dictionary<string, List<Pair<string, object>>>)viewState[7];
            m_ListComponentSimples_AI = (List<ComponentSimple>)viewState[8];
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
            object[] ret = new object[9];
            ret[0] = base.SaveViewState();
            ret[1] = m_DicTemplates;
            ret[2] = m_ListComponentSimples;
            ret[3] = m_DicMarket;
            ret[4] = m_DicMarketRules;
            ret[5] = m_DicUNLMarket;
            ret[6] = m_CAInfo;
            ret[7] = m_DicTemplates_Additional;
            ret[8] = m_ListComponentSimples_AI;
            return ret;
        }
        #endregion SaveViewState

        //* --------------------------------------------------------------- *//
        // Sauvegarde des données créées en run-time 
        // pour maintien après PostBack (SESSION)
        //* --------------------------------------------------------------- *//

        #region RestorePlaceHolder
        /// <summary>
        /// ● Restauration des sauvegardes effectuées dans les variables de SESSION
        ///   Contrôles dynamiques des composants simples
        ///   Contrôles dynamiques des références additionnelles
        ///   Events éventuels rattachés à ces contrôles
        ///   Classe CorporateAction
        /// </summary>
        // EG 20140518 [19913]
        private void RestorePlaceHolder()
        {
            RestorePlaceHolder(plhComponents);
            RestorePlaceHolder(plhComponents_AI);
            // Repositionnement de l'événement TextChanged sur le contrôle dynamique txtRFactorCertified 
            using (WCTextBox2 txtRFactorCertified = FindControl(CATools.pfxTXTComponent + CATools.RFactorCertified_Id) as WCTextBox2)
            {
                if (null != txtRFactorCertified)
                    txtRFactorCertified.TextChanged += new System.EventHandler(ApplyRoundingRFactorCertified);
            }

            RestorePlaceHolder(plhRefNotices);

            // FI 20200518 [XXXXX] Utilisation de DataCache
            m_CorporateAction = DataCache.GetData<CorporateAction>(CA_GUID);
            m_CorporateDocs = DataCache.GetData<CorporateDocs>(CADOC_GUID);
            if (null == m_CorporateDocs)
                m_CorporateDocs = new CorporateDocs();

            SetEventHandler(plhCARefNoticeAdds.Controls);
        }
        /// <summary>
        /// ● Restauration des sauvegardes effectuées dans les variables de SESSION
        ///   Contrôles dynamiques des composants simples (plhCEComponents + "_" + GUID)
        ///   Contrôles dynamiques des références additionnelles (plhCARefNoticeAdds + "_" + GUID)
        /// </summary>
        private void RestorePlaceHolder(string pControlName)
        {
            Control ctrl = FindControl(pControlName);
            if (null != ctrl)
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                ControlCollection controlcollection = DataCache.GetData<ControlCollection>(BuildDataCacheKey(pControlName));
                if (null != controlcollection)
                {
                    try
                    {
                        while (0 != controlcollection.Count)
                            ctrl.Controls.Add(controlcollection[0]);
                    }
                    catch (Exception ex)
                    {
                        ctrl.Controls.AddAt(0, new LiteralControl(System.Environment.NewLine + ex.Message.ToString()));
                    }
                }
            }
        }
        #endregion RestorePlaceholder
        #region SetEventHandler
        /// <summary>
        /// Restauration des événements des contrôles dynamiques
        /// </summary>
        /// <param name="pCtrlcollection"></param>
        private void SetEventHandler(ControlCollection pCtrlcollection)
        {
            if (null != pCtrlcollection)
            {
                foreach (Control ctrl in pCtrlcollection)
                {
                    //
                    if (ctrl is CustomValidator cv)
                    {
                        cv.ServerValidate += new ServerValidateEventHandler(GlobalValidate);
                    }
                    if (null != ctrl.Controls)
                        SetEventHandler(ctrl.Controls);
                }
            }
        }
        #endregion SetEventHandler
        #region SavePlaceHolder
        /// <summary>
        /// ● Sauvegardes dans des variables de SESSION
        ///   Contrôles dynamiques des composants simples  (plhCEComponents + "_" + GUID)
        ///   Contrôles dynamiques des références additionnelles (plhCARefNoticeAdds + "_" + GUID)
        ///   Classe CorporateAction
        /// </summary>
        // EG 20140518 [19913]
        private void SavePlaceHolder()
        {
            SavePlaceHolder(plhComponents);
            SavePlaceHolder(plhRefNotices);
            // FI 20200518 [XXXXX] Utilisation de DataCache
            DataCache.SetData(CA_GUID, m_CorporateAction);
            DataCache.SetData(CADOC_GUID, m_CorporateDocs);
            SavePlaceHolder(plhComponents_AI);
        }
        /// <summary>
        /// ● Sauvegardes d'un contrôle (PlaceHolder) dans une variable de SESSION
        ///   Contrôles dynamiques des composants simples  (plhCEComponents + "_" + GUID)
        ///   Contrôles dynamiques des références additionnelles (plhCARefNoticeAdds + "_" + GUID)
        /// </summary>
        private void SavePlaceHolder(string pControlName)
        {
            Control ctrl = FindControl(pControlName);
            if ((null != ctrl) && (0 < ctrl.Controls.Count))
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                DataCache.SetData<ControlCollection>(BuildDataCacheKey(pControlName), ctrl.Controls);
            }
        }
        #endregion SavePlaceholder

        //* --------------------------------------------------------------- *//
        // Validateurs
        //* --------------------------------------------------------------- *//

        #region ControlToValidate
        /// <summary>
        /// ● Fonction appelée au déclenchement d'un Custom validator
        ///   voir page ASPX, property:ControlToValidate
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        protected void ControlToValidate(object source, ServerValidateEventArgs args)
        {
            string defaultMessage = "InvalidData";
            if (source is CustomValidator cv)
            {
                switch (cv.ControlToValidate)
                {
                    case "txtCAMArket":
                        // Le marché est OBLIGATOIRE
                        args.IsValid = StrFunc.IsFilled(txtCAMarket.Text) && (null != GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "txtCEUNLMArket":
                        // Le marché du sous-jacent est OBLIGATOIRE
                        args.IsValid = StrFunc.IsFilled(txtCEUNLMarket.Text) && (null != GetMarketIdentificationByIdentifier(m_DicUNLMarket, txtCEUNLMarket.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "ddlCEType":
                        // Le type est OBLIGATOIRE
                        args.IsValid = StrFunc.IsFilled(ddlCEType.SelectedValue);
                        defaultMessage = "MissingData";
                        break;
                    case "ddlCECombinedOperand":
                        // L'opérande est OBLIGATOIRE
                        if (CATools.IsCEGroupCombination(ddlCEGroup.SelectedValue))
                        {
                            args.IsValid = StrFunc.IsFilled(ddlCECombinedOperand.SelectedValue);
                            defaultMessage = "MissingData";
                        }
                        break;
                    case "ddlCECombinedType":
                        // Le type combiné est OBLIGATOIRE
                        if (CATools.IsCEGroupCombination(ddlCEGroup.SelectedValue))
                        {
                            args.IsValid = StrFunc.IsFilled(ddlCECombinedType.SelectedValue);
                            defaultMessage = "MissingData";
                        }
                        break;
                    case "ddlCETemplate":
                        // Si méthode par ratio alors un TEMPLATE doit exister et être sélectionné
                        AdjustmentMethodOfDerivContractEnum adjMethod =
                            (AdjustmentMethodOfDerivContractEnum)Enum.Parse(typeof(AdjustmentMethodOfDerivContractEnum), ddlCEAdjMethod.SelectedValue, false);
                        if (adjMethod == AdjustmentMethodOfDerivContractEnum.Ratio)
                        {
                            args.IsValid = StrFunc.IsFilled(args.Value);
                            defaultMessage = CATools.pfxLBLComponent + "TemplateNotImplemented";
                        }
                        break;
                }
                if (false == args.IsValid)
                {
                    // Construction du message d'erreur si non valide.
                    cv.SetFocusOnError = true;
                    cv.ErrorMessage = SetValidatorMessage(cv.ControlToValidate, Ressource.GetString(defaultMessage));
                }
            }
        }
        #endregion ControlToValidate
        #region GlobalValidate
        /// <summary>
        /// ● Fonction appelée au déclenchement d'un Custom validator
        ///   voir page ASPX, property:ControlToValidate
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// Plus de contrôle sur la référence de notice
        protected void GlobalValidate(object source, ServerValidateEventArgs args)
        {
            string defaultMessage = "InvalidData";
            if (source is CustomValidator cv)
            {
                DateTime pubDate = new DtFunc().StringToDateTime(txtCAPubDate.Text);
                DateTime exDate = new DtFunc().StringToDateTime(txtCEExDate.Text);
                DateTime effectiveDate = new DtFunc().StringToDateTime(txtCEEffectiveDate.Text);
                MarketIdentification _market = GetMarketIdentificationByIdentifier(m_DicMarket, txtCAMarket.Text);
                switch (cv.ControlToValidate)
                {
                    case "txtCARefNotice":
                        // La référence de notice doit être conforme à la règle du marché sélectionné
                        // Codage des "Regex" dans RefMarketNoticeChecker
                        if (null != _market)
                            args.IsValid = true; // CATools.RefMarketNoticeChecker(_market.acronym, args.Value);
                        break;
                    case "txtCAPubDate":
                        // La date de publication doit être inférieure à l'ex-Date
                        if (exDate.Date != DateTime.MinValue)
                        {
                            args.IsValid = (pubDate.CompareTo(exDate) <= 0);
                            defaultMessage = "Msg_ValidationRule_CAPubDate";
                        }
                        break;
                    case "txtCEExDate":
                        // L'ex-date doit être supérieure ou égale à la date de publication et <= à la date d'effet
                        if (exDate.Date != DateTime.MinValue)
                        {
                            args.IsValid = (exDate.CompareTo(pubDate) >= 0);
                            defaultMessage = "Msg_ValidationRule_CAExDatePubDate";
                            if (args.IsValid && (effectiveDate.Date != DateTime.MinValue))
                            {
                                args.IsValid = (exDate.CompareTo(effectiveDate) <= 0);
                                defaultMessage = "Msg_ValidationRule_CAExDateEffectiveDate";
                            }
                        }
                        break;
                    default:
                        if (cv.ControlToValidate.StartsWith("txtRN_Reference") && (null != _market))
                        {
                            // Une référence de notice additionnelle doit être conforme à la règle du marché sélectionné
                            // Codage des "Regex" dans RefMarketNoticeChecker
                            args.IsValid = true; // CATools.RefMarketNoticeChecker(_market.acronym, args.Value);
                        }
                        break;
                }
                if (false == args.IsValid)
                {
                    // Construction du message d'erreur si non valide.
                    cv.SetFocusOnError = true;
                    cv.ErrorMessage = SetValidatorMessage(cv.ControlToValidate, Ressource.GetString(defaultMessage));
                }
            }
        }
        #endregion GlobalValidate

        #region IsAllValid
        /// <summary>
        /// Application des validateurs par chaque groupe de validation CA et CE
        /// </summary>
        /// <returns></returns>
        private bool IsAllValid()
        {
            Page.Validate(CATools.ValidationGroupEnum.CA.ToString());
            Page.Validate(CATools.ValidationGroupEnum.CE.ToString());
            return Page.IsValid;
        }
        #endregion IsAllValid

        #region SetValidators
        /// <summary>
        /// Initialisation des validateurs dynamiques
        /// </summary>
        private void SetValidators()
        {
            validSummaryCA.ShowSummary = true;
            validSummaryCA.ValidationGroup = CATools.ValidationGroupEnum.CA.ToString();
            validSummaryCA.DisplayMode = ValidationSummaryDisplayMode.List;
            SetValidators(txtCAIdentifier, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCADisplayName, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCADocumentation, EFSRegex.TypeRegex.None, false, EFSCssClass.CaptureMultilineOptional, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCARefNotice, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCAPubDate, EFSRegex.TypeRegex.RegexDate, true, "DtPicker " + txtCAPubDate.CssClass, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCAURLNotice, false, CATools.ValidationGroupEnum.CA);
            SetValidators(txtCARefNoticeFileName, false, CATools.ValidationGroupEnum.CA);
            validSummaryCE.ShowSummary = true;
            validSummaryCE.ValidationGroup = CATools.ValidationGroupEnum.CE.ToString();
            validSummaryCE.DisplayMode = ValidationSummaryDisplayMode.List;
            SetValidators(txtCEIdentifier, CATools.ValidationGroupEnum.CE);
            SetValidators(txtCEExDate, EFSRegex.TypeRegex.RegexDate, false, "DtPicker " + txtCEExDate.CssClass, CATools.ValidationGroupEnum.CE);
            SetValidators(txtCEEffectiveDate, EFSRegex.TypeRegex.RegexDate, false, "DtPicker " + txtCEEffectiveDate.CssClass, CATools.ValidationGroupEnum.CE);
            SetValidators(txtCEUNLCode, CATools.ValidationGroupEnum.CE);
            SetValidators(txtCEUNLCode2, false, CATools.ValidationGroupEnum.CE);
        }
        private void SetValidators(TextBox pControl, Nullable<CATools.ValidationGroupEnum> pValidationGroup)
        {
            SetValidators(pControl, EFSRegex.TypeRegex.None, true, EFSCssClass.Capture, pValidationGroup);
        }
        private void SetValidators(TextBox pControl, bool pIsMandatory, Nullable<CATools.ValidationGroupEnum> pValidationGroup)
        {
            SetValidators(pControl, EFSRegex.TypeRegex.None, pIsMandatory, pIsMandatory ? EFSCssClass.Capture : EFSCssClass.CaptureOptional, pValidationGroup);
        }
        private void SetValidators(TextBox pControl, EFSRegex.TypeRegex pTypeRegex, bool pIsMandatory, string pCssClass, Nullable<CATools.ValidationGroupEnum> pValidationGroup,
            params string[] pLabelMessage)
        {
            List<Validator> validators = new List<Validator>();
            Validator validator = null;
            if (StrFunc.IsFilled(pCssClass))
                pControl.CssClass = pCssClass;
            #region RequireField
            if (pIsMandatory)
            {
                validator = new Validator(Ressource.GetString("MissingData"), false, true);
                validators.Add(validator);
            }
            #endregion RequireField
            #region Regular Expression
            if (EFSRegex.TypeRegex.None != pTypeRegex)
            {
                validator = new Validator(pTypeRegex, Ressource.GetString("InvalidData"), false, true);
                validators.Add(validator);
            }
            #endregion Regular Expression
            if (0 < validators.Count)
            {
                string errMessage = SetValidatorMessage(ArrFunc.IsFilled(pLabelMessage) ? pLabelMessage[0] : pControl.ID);
                validators.ForEach(item =>
                {
                    item.ErrorMessage = errMessage + Cst.HTMLBreakLine + "[" + item.ErrorMessage + "]";
                    WebControl wcValidator = item.CreateControl(pControl.ID);
                    BaseValidator bv = (BaseValidator)wcValidator;
                    if (pValidationGroup.HasValue)
                    {
                        bv.ValidationGroup = pValidationGroup.Value.ToString();
                    }
                    bv.Display = ValidatorDisplay.None;
                    bv.SetFocusOnError = true;
                    pControl.Controls.Add(wcValidator);
                });
            }
        }
        #endregion SetValidators
        #region SetValidatorMessage
        /// <summary>
        /// Constrcution du titre du message d'un validadteur en fonction du contrôle
        /// </summary>
        /// <param name="pControlID"></param>
        /// <param name="pDefaultMessage"></param>
        /// <returns></returns>
        private string SetValidatorMessage(string pControlID, params string[] pDefaultMessage)
        {
            string errMessage = Cst.HTMLBold + pControlID + Cst.HTMLEndBold;
            string id = string.Empty;
            if (pControlID.StartsWith(Cst.TXT, StringComparison.OrdinalIgnoreCase) ||
                pControlID.StartsWith(Cst.DDL, StringComparison.OrdinalIgnoreCase))
                id = pControlID.Substring(Cst.TXT.Length);
            if (!(FindControl(Cst.LBL + id) is WCTooltipLabel lbl))
                lbl = FindControl(Cst.LBL.ToLower() + id) as WCTooltipLabel;
            if (null != lbl)
                errMessage = Cst.HTMLBold + lbl.Text + Cst.HTMLEndBold;
            else if (StrFunc.IsFilled(id))
                errMessage = Cst.HTMLBold + Ressource.GetString(Cst.LBL.ToLower() + id, false) + Cst.HTMLEndBold;
            if (ArrFunc.IsFilled(pDefaultMessage))
                errMessage += Cst.HTMLBreakLine + "[" + pDefaultMessage[0] + "]";
            return errMessage;
        }
        #endregion SetValidatorMessage

        #region GetFinalResult
        /// <summary>
        /// Résultat du traitement final d'une CA
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20160105 [34091] Test Existence STORED PROCEDURE 
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20220908 [XXXXX][WI418] Suppression de la classe obsolète EFSParameter
        // EG 20220908 [XXXXX] Ajout paramètre sur l'entité
        protected void GetFinalResult(object sender, CommandEventArgs e)
        {
            string CS = SessionTools.CS;
            DbSvrType serverType = DataHelper.GetDbSvrType(CS);
            if (DbSvrType.dbSQL == serverType)
            {
                DataParameters parameters = new DataParameters();
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pDtCA", DbType.DateTime), m_CorporateAction.corporateEvent[0].effectiveDate);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdCA", DbType.Int32), m_CorporateAction.IdCA);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pIdA_Entity", DbType.Int32), SessionTools.User.Entity_IdA);
                parameters.Add(new DataParameter(CS, CommandType.StoredProcedure, "pXMLOutput", DbType.Int32), 1);
                IDbDataParameter paramResult = new DataParameter(CS, CommandType.StoredProcedure, "pResult", DbType.Xml) { Direction = ParameterDirection.Output };
                parameters.Add(paramResult as DataParameter);
                //
                try
                {
                    int retUP = DataHelper.ExecuteNonQuery(CS, CommandType.StoredProcedure, "dbo.UP_RESULT_CA", parameters.GetArrayDbParameter());
                    if (0 != retUP)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(paramResult.Value.ToString());

                        Hashtable param = new Hashtable
                        {
                            { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name }
                        };
                        string xslFile = string.Empty;
                        SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, xsltFile, ref xslFile);
                        string result = XSLTTools.TransformXml(new StringBuilder(paramResult.Value.ToString()), xslFile, param, null);
                        SizeHeightForAlertImmediate = new Pair<int?, int?>(560, 800);
                        SizeWidthForAlertImmediate = new Pair<int?, int?>(1000, null);
                        MsgForAlertImmediate = result;
                    }
                }
                catch (Exception) { throw; }
            }
            else
            {
                MsgForAlertImmediate = "This action is not managed, please contact EFS";
            }

        }
        #endregion GetFinalResult

        #region WebMethods
        [WebMethod]
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// Méthode appelé via JS
        public static List<KeyValuePair<int, string>> LoadDataControl(string currentCtrlId, string identifier)
        {
            string dataCacheKey = string.Empty;
            List<KeyValuePair<int, string>> lst = new List<KeyValuePair<int, string>>();
            if ("txtCAMarket" == currentCtrlId)
                dataCacheKey = "CA_Market";
            else if (("txtCEUNLMarket" == currentCtrlId) || ("txtCEUNLMarket2" == currentCtrlId))
                dataCacheKey = "CA_UNLMarket";

            if (StrFunc.IsFilled(dataCacheKey))
            {
                Dictionary<int, MarketIdentification> dicSource = DataCache.GetData<Dictionary<int, MarketIdentification>>(dataCacheKey);
                lst = dicSource.Where(item => item.Value.shortAcronymSpecified && item.Value.shortAcronym.ToUpper().Contains(identifier.ToUpper())).Select(item => new KeyValuePair<int, string>(item.Key, item.Value.shortAcronym)).ToList();
            }
            return lst;
        }
        #endregion WebMethods


        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// Recherche des données de Marché dans le dictionnaire (via Identifiant)
        public MarketIdentification GetMarketIdentificationByIdentifier(Dictionary<int,MarketIdentification> pDic, string pIdentifier)
        {
            MarketIdentification market = null;
            Nullable<KeyValuePair<int, MarketIdentification>> ret = pDic.FirstOrDefault(item => item.Value.shortAcronym.ToUpper() == pIdentifier.ToUpper());
            if (ret.HasValue)
                market = ret.Value.Value;
            return market;
        }
        /// EG 20210329 [25153] Gestion Autocomplete sur les marchés (TextBox remplace DropdownList)
        /// Recherche des données de Marché dans le dictionnaire (via Id)
        public MarketIdentification GetMarketIdentificationById(Dictionary<int, MarketIdentification> pDic, int pId)
        {
            MarketIdentification market = null;
            Nullable<KeyValuePair<int, MarketIdentification>> ret = pDic.FirstOrDefault(item => item.Key == pId);
            if (ret.HasValue)
                market = ret.Value.Value;
            return market;
        }
        #endregion Methods
    }
    #endregion CorporateActionIssuePage
}
