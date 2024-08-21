#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.ApplicationBlocks.Data.Extension;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI.Interface;
using EFS.Process;
using EfsML.ClosingReopeningPositions;
using EfsML.Enum;
using FixML.Enum;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
#endregion Using Directives

namespace EFS.Spheres
{
    #region ClosingReopeningPosition
    // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
    public partial class ClosingReopeningPositionPage : ActionPage
    {
        #region Members
        private readonly string cssClassMaster = "actionRq";
        private List<MapDataReaderRow> m_LstIdInstr = null;
        private List<MapDataReaderRow> m_LstFeeAction = null;
        private List<MapDataReaderRow> m_LstDefineExtlID = null;
        // EG 20231030 [WI725]
        private List<MapDataReaderRow> m_LstActionRequestFilters = null;
        protected ARQInfo m_ARQInfo;
        protected ClosingReopeningAction m_ClosingReopeningAction;
        private readonly string mainMenuName = "process";
        #endregion Members

        #region Accessors
        /// <summary>
        /// Retourne L'id (ID) d'un identifiant (IDENTIFIER) donné
        /// </summary>
        /// <param name="pLst">Source de recherche</param>
        /// <param name="pTypeAssociated">filtre associé (Actor|Book|GrpActor|GrpBook ou CommodityContract|DerivativeContract|GrpContract|Market|GrpMarket)</param>
        /// <param name="pIdentifier">Identifiant</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public int GetIdByIdentifier(List<MapDataReaderRow> pLst, string pTypeAssociated, string pIdentifier)
        {
            int id = 0;
            MapDataReaderRow ret = pLst.FirstOrDefault(item =>
            (item["TYPEDATA"].Value.ToString() == pTypeAssociated) && (item["IDENTIFIER"].Value.ToString().ToUpper()) == pIdentifier.ToUpper());
            if (null != ret)
                id = Convert.ToInt32(ret["ID"].Value);
            return id;
        }
        /// <summary>
        /// Retourne L'identifiant (IDENTIFIER) associé à un id (ID) 
        /// </summary>
        /// <param name="pLst">Source de recherche</param>
        /// <param name="pTypeAssociated">filtre associé (Actor|Book|GrpActor|GrpBook ou CommodityContract|DerivativeContract|GrpContract|Market|GrpMarket)</param>
        /// <param name="pId">ID</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public string GetIdentifierById(List<MapDataReaderRow> pLst, string pTypeAssociated, int pId)
        {
            string identifier = string.Empty;
            MapDataReaderRow ret = pLst.FirstOrDefault(item => (item["TYPEDATA"].Value.ToString() == pTypeAssociated) && (Convert.ToInt32(item["ID"].Value) == pId));
            if (null != ret)
                identifier = ret["IDENTIFIER"].Value.ToString();
            return identifier;
        }
        /// <summary>
        /// Retourne L'identifiant (IDENTIFIER) associé à un id (ID) 
        /// pour un type donné (CommodityContract|DerivativeContract|GrpContract|Market|GrpMarket)
        /// </summary>
        /// <param name="pId">ID</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public string GetIdContractIdentifier(int pId)
        {
            return GetIdentifierById(LstIdContract, ddlTypeContract.SelectedValue, pId);
        }
        /// <summary>
        /// Retourne L'ID associé à un identifiant (IDENTIFIER)
        /// pour un type donné (CommodityContract|DerivativeContract|GrpContract|Market|GrpMarket)
        /// </summary>
        /// <param name="pId">ID</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public int GetIdContractId(string pIdentifier)
        {
            return GetIdByIdentifier(LstIdContract, ddlTypeContract.SelectedValue, pIdentifier);
        }
        /// <summary>
        /// Retourne L'Ientifiant (IDENTIFIER) associé à un id (ID) côté DEALER
        /// pour un type donné (Actor|Book|GrpActor|GrpBook)
        /// </summary>
        /// <param name="pId">ID</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public string GetIdDealerIdentifier(int pId)
        {
            return GetIdentifierById(LstIdDealer, ddlTypeDealer_C.SelectedValue, pId);
        }
        /// <summary>
        /// Retourne L'Ientifiant (IDENTIFIER) associé à un id (ID) côté CLEARER
        /// pour un type donné (ACTOR|BOOK|GRPACTOR|GRPBOOK)
        /// </summary>
        /// <param name="pId">ID</param>
        /// <returns></returns>
        public string GetIdClearerIdentifier(int pId)
        {
            return GetIdentifierById(LstIdClearer, ddlTypeClearer_C.SelectedValue, pId);
        }
        // EG 20240520 [WI930] New
        public string GetIdCssCustodianIdentifier(int pId)
        {
            return GetIdentifierById(LstIdCssCustodian, ddlTypeCssCustodian_C.SelectedValue, pId);
        }
        /// <summary>
        /// Retourne L'ID associé à un identifiant (IDENTIFIER) côté DEALER
        /// pour un type donné (Actor|Book|GrpActor|GrpBook)
        /// </summary>
        /// <param name="pIdentifier">IDENTIFIER</param>
        /// <returns></returns>
        public int GetIdDealerId(string pIdentifier)
        {
            return GetIdByIdentifier(LstIdDealer, ddlTypeDealer_C.SelectedValue, pIdentifier);
        }
        /// <summary>
        /// Retourne L'ID associé à un identifiant (IDENTIFIER) côté CLEARER
        /// pour un type donné (Actor|Book|GrpActor|GrpBook)
        /// </summary>
        /// <param name="pIdentifier">IDENTIFIER</param>
        /// <returns></returns>
        // EG 20200318 [24683] New (implementation autocomplete)
        public int GetIdClearerId(string pIdentifier)
        {
            return GetIdByIdentifier(LstIdClearer, ddlTypeClearer_C.SelectedValue, pIdentifier);
        }
        // EG 20240520 [WI930] New
        public int GetIdCssCustodianId(string pIdentifier)
        {
            return GetIdByIdentifier(LstIdCssCustodian, ddlTypeCssCustodian_C.SelectedValue, pIdentifier);
        }
        /// <summary>
        /// Retourne l'Identifant (IDMENU) sur la base d'un IDPERMISSION
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        public string GetFeeActionIdentifier(int pId)
        {
            string identifier = string.Empty;
            MapDataReaderRow ret = m_LstFeeAction.FirstOrDefault(item => (Convert.ToInt32(item["IDPERMISSION"].Value) == pId));
            if (null != ret)
                identifier = ret["IDMENU"].Value.ToString();
            return identifier;
        }
        /// <summary>
        /// Retourne les dates ENABLED/DISABLED des filtres additionels
        /// des filtres de CLosing/Reopening
        /// </summary>
        /// <returns></returns>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
        public DateTime GetARQFilterDates(string pIdentifier, string pColumn)
        {
            DateTime dt = DateTime.MinValue;
            MapDataReaderRow ret = m_LstActionRequestFilters.FirstOrDefault(item => item["IDENTIFIER"].Value.ToString() == pIdentifier);
            if (null != ret)
                dt = Convert.IsDBNull(ret[pColumn].Value) ? DateTime.MinValue : Convert.ToDateTime(ret[pColumn].Value);
            return dt;
        }

        // EG 20200318 [24683] New (implementation autocomplete)
        public List<MapDataReaderRow> LstIdContract
        {
            get
            {
                return DataCache.GetData<List<MapDataReaderRow>>("CRP_IDContract");
            }
            set
            {
                DataCache.SetData<List<MapDataReaderRow>>("CRP_IDContract", value);
            }
        }

        // EG 20200318 [24683] New (implementation autocomplete)
        public List<MapDataReaderRow> LstIdDealer
        {
            get
            {
                return DataCache.GetData<List<MapDataReaderRow>>("CRP_IDDealer");
            }
            set
            {
                DataCache.SetData<List<MapDataReaderRow>>("CRP_IDDealer", value);
            }
        }

        // EG 20200318 [24683] New (implementation autocomplete)
        public List<MapDataReaderRow> LstIdClearer
        {
            get
            {
                return DataCache.GetData<List<MapDataReaderRow>>("CRP_IDClearer");
            }
            set
            {
                DataCache.SetData<List<MapDataReaderRow>>("CRP_IDClearer", value);
            }
        }
        // EG 20240520 [WI930] New
        public List<MapDataReaderRow> LstIdCssCustodian
        {
            get
            {
                return DataCache.GetData<List<MapDataReaderRow>>("CRP_IDCssCustodian");
            }
            set
            {
                DataCache.SetData<List<MapDataReaderRow>>("CRP_IDCssCustodian", value);
            }
        }
        #region ARQ_GUID
        /// <summary>
        /// Utilisé comme clé unique de stockage (Session) de l'action courante
        /// </summary>
        private string ARQ_GUID
        {
            get
            {
                // FI 20200518 [XXXXX] Use BuildDataCacheKey
                return BuildDataCacheKey("ARQ");
            }
        }
        #endregion ARQ_GUID
        #endregion Accessors
        #region Constructor
        public ClosingReopeningPositionPage()
        {
        }
        #endregion Constructor

        #region Events

        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion InitializeComponent

        #region OnAction
        /// <summary>
        /// ► Actions sur boutons de la barre d'outils
        ///   Enregistrement (btnRecord)
        ///   Suppression (btnRemove)
        ///   Annulation (btnCancel)
        ///   Duplication (btnDuplicate)
        ///   
        ///     ● 1er passage  : Test de validité des contrôles (sauf btnCancel) et message de confirmation
        ///     ● 2ème passage : Si validation exécution de la demande
        /// </summary>
        // EG 20190613 [24683] Upd
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210222 [XXXXX] Appel OpenerRefreshAndClose (présentes dans PageBase.js) en remplacement de OpenerCallRefreshAndClose
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
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
                            message = Ressource.GetString2("Msg_ARQRefresh", txtIdentifier.Text, m_ARQInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnRecord":
                            isValid = IsAllValid();
                            if (m_ARQInfo.Id.HasValue)
                                message = Ressource.GetString2("Msg_ARQRecord", txtIdentifier.Text, m_ARQInfo.Id.Value.ToString()) + Cst.CrLf;
                            else
                                message = Ressource.GetString2("Msg_ARQRecordNew", txtIdentifier.Text) + Cst.CrLf;
                            break;
                        case "btnRemove":
                            isValid = IsAllValid();
                            message = Ressource.GetString2("Msg_ARQRemove", txtIdentifier.Text, m_ARQInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnCancel":
                            isValid = true;
                            message = Ressource.GetString("Msg_ARQReset") + Cst.CrLf;
                            break;
                        case "btnDuplicate":
                            isValid = true;
                            message = Ressource.GetString2("Msg_ARQDuplicate", txtIdentifier.Text, m_ARQInfo.Id.Value.ToString()) + Cst.CrLf;
                            break;
                        case "btnSend":
                            isValid = IsAllValid();
                            if (m_ARQInfo.Id.HasValue)
                                message = Ressource.GetString2("Msg_CASend", txtIdentifier.Text, m_ARQInfo.Id.Value.ToString()) + Cst.CrLf;
                            else
                                message = Ressource.GetString2("Msg_CASendNew", txtIdentifier.Text) + Cst.CrLf;
                            break;
                        case "btnSeeMsg":
                            DisplayInstructions();
                            break;
                    }
                    if (isValid && StrFunc.IsFilled(message))
                        JavaScript.ConfirmStartUpImmediate(this, message, ctrl.ID, "TRUE", "FALSE");
                }
                else if (eventArgument == "TRUE")
                {
                    //   Exécution de la demande suite à actions sur boutons de la barre d'outils (2ème passage)
                    //   Enregistrement (btnRecord)
                    //     ● Enregistrement
                    //   Suppression (btnRemove)
                    //   Annulation (btnCancel)
                    //   Duplication (btnDuplicate)
                    //   Envoi de message à NormMsgFactory (btnSend)

                    Cst.ErrLevel ret;
                    switch (eventTarget)
                    {
                        case "btnRefresh":
                            Refresh();
                            break;
                        case "btnRecord":
                            message = m_ARQInfo.Id.HasValue ? "Msg_CAUpdated" : "Msg_CAInserted";
                            ret = Record();
                            if (Cst.ErrLevel.SUCCESS != ret)
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                message += ErrLevelForAlertImmediate.ToString();
                                MsgForAlertImmediate = Ressource.GetString2(message, txtIdentifier.Text, m_ClosingReopeningAction.spheresid);
                            }
                            else
                            {
                                JavaScript.CallFunction(this, "OpenerRefreshAndClose(null,null)");
                            }
                            break;
                        case "btnRemove":
                            message = "Msg_ARQRemoved";
                            if (null != m_ClosingReopeningAction)
                            {
                                ret = m_ClosingReopeningAction.Delete();
                                if (Cst.ErrLevel.SUCCESS != ret)
                                {
                                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                    message += ErrLevelForAlertImmediate.ToString();
                                    MsgForAlertImmediate = Ressource.GetString2(message, txtIdentifier.Text, m_ClosingReopeningAction.IdARQ.ToString());
                                }
                                else
                                {
                                    JavaScript.CallFunction(this, "OpenerRefreshAndClose(null,null)");
                                }
                            }
                            break;
                        case "btnCancel":
                            ResetAll();
                            break;
                        case "btnDuplicate":
                            Duplicate();
                            break;
                        case "btnSend":
                            message = m_ARQInfo.Id.HasValue ? "Msg_CAMessageSended" : "Msg_CAInsertedAndMessageSended";
                            ret = Record();
                            if (Cst.ErrLevel.SUCCESS == ret)
                            {
                                ret = SendNormMsgARQ();
                                if (Cst.ErrLevel.SUCCESS != ret)
                                {
                                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                    message = m_ARQInfo.Id.HasValue ? "Msg_CAMessageNotSended" : "Msg_CAInsertedAndMessageNotSended";
                                }
                                else
                                {
                                    m_ARQInfo.Id = m_ClosingReopeningAction.IdARQ;
                                    m_ARQInfo.Mode = Cst.Capture.ModeEnum.Update;
                                    Refresh();
                                }
                            }
                            else
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                                message += ErrLevelForAlertImmediate.ToString();
                            }
                            MsgForAlertImmediate = Ressource.GetString2(message, txtIdentifier.Text, m_ClosingReopeningAction.spheresid);
                            break;
                    }
                }
            }
        }
        #endregion OnAction
        #region OnInit
        /// <summary>
        /// Initialisation de la page
        /// </summary>
        /// <param name="e"></param>
        // EG 20190613 [24683] Upd
        // EG 20200901 [25556] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            Form = frmClosingReopeningPosition;
            AntiForgeryControl();
            AddInputHiddenGUID();

            m_ARQInfo = new ARQInfo(SessionTools.User.IdA, Request.QueryString["PKV"]);

            SizeHeightForAlertImmediate = new Pair<int?, int?>(0, 640);
            SizeWidthForAlertImmediate = new Pair<int?, int?>(0, 600);
        }
        #endregion OnInit
        #region Page_Load
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20190318 Upd (TransferModeEnum replaceTransferClosingModeEnum|TransferReopeningModeEnum) ClosingReopening Step3
        // EG 20190613 [24683] Upd
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        // EG 20210315 [24683] Ajout tooltip sur Referentiel
        // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
        protected void Page_Load(object sender, EventArgs e)
        {
            // Gestion du focus suite à postback
            HookOnFocus(this.Page as Control);
            Page.ClientScript.RegisterStartupScript(typeof(ClosingReopeningPositionPage), "ScriptDoFocus", SCRIPT_DOFOCUS.Replace("REQUEST_LASTFOCUS", Request["__LASTFOCUS"]), true);

            #region Header
            string idMenu = Request.QueryString["IDMENU"].ToString();
            string leftTitle = Ressource.GetString(idMenu, true);
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            // EG 20130725 Timeout sur Block
            JQuery.Block block = new JQuery.Block(idMenu, "Msg_ARQProcessInProgress", true)
            {
                Timeout = SystemSettings.GetTimeoutJQueryBlock("CRPRequest")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            Control head;
            if (null != this.Header)
                head = (Control)this.Header;
            else
                head = (Control)PageTools.SearchHeadControl(this);

            PageTools.SetHeaderLink(head, "linkCssAction", "~/Includes/ClosingReopeningPosition.min.css");

            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu.GetIdMenu(IdMenu.Menu.Input)));
            plhHeader.Controls.Add(pnlHeader);
            #endregion Header

            /// Alimentation des tooltips associés aux DDL 
            SetTooltipEnum(ddlRequestType, "PosRequestTypeEnum");
            SetTooltipEnum(ddlTiming, "SettlSessIDEnum");
            SetTooltipEnum(ddlMode_C, "TransferModeEnum");
            SetTooltipEnum(ddlOtherPrice_C, "TransferPriceEnum");
            SetTooltipEnum(ddlMode_O, "TransferModeEnum");
            SetTooltipEnum(ddlOtherPrice_O, "TransferPriceEnum");
            SetOtherTooltip();
            SetRessources();
            SetToolTipInfo(txtIDDealer_C, "MsgIdDealer_C");
            SetToolTipInfo(txtIDClearer_C, "MsgIdClearer_C");
            // EG 20240520 [WI930] New
            SetToolTipInfo(txtIDCssCustodian_C, "MsgIdCssCustodian_C");

            // Chargement de la liste des IDINSTR
            LoadIDInstr();
            // Chargement de la liste des IDCONTRACT
            LoadIDContract();
            // Chargement de la liste des ACTOR|BOOK|GRPACTOR,GRPBOOK (COUNTERPARTY|CLIENT)
            LoadIdParty(false);
            // Chargement de la liste des ACTOR|BOOK|GRPACTOR,GRPBOOK (CLEARER|CUSTODIAN)
            LoadIdParty(true);
            // Chargement de la liste des ACTOR (CSS|CUSTODIAN)
            LoadIdCssCustodian();
            // Chargement des liens externes ACTOR|BOOK
            LoadDefineExtlID();
            // Chargement de la liste des actions possibles pour application des frais 
            LoadFeeAction();
            // Chargement de la liste des filtres additionnels de détermination des trades candidats
            LoadARQFilters();

            InitializeLinkButton();
            if (false == IsPostBack)
            {
                // Initialisation des contrôles de la page
                InitializeData();
                if (m_ARQInfo.Id.HasValue)
                    LoadClosingReopeningAction();
                else
                    SetDefault();
            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //m_ClosingReopeningAction = (ClosingReopeningAction)Session[ARQ_GUID];
                m_ClosingReopeningAction = DataCache.GetData<ClosingReopeningAction>(ARQ_GUID);
            }

            ComplementaryInitialize();
            SetValidators();
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[ARQ_GUID] = m_ClosingReopeningAction;
            DataCache.SetData<ClosingReopeningAction>(ARQ_GUID, m_ClosingReopeningAction);
        }
        #endregion Page_Load
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20201104 [24683] Ajout d'un bouton AttachedDoc
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private void InitializeLinkButton()
        {
            btnRefresh.Text = "<i class='fas fa-sync-alt'></i>";
            btnRecord.Text = "<i class='fa fa-save'></i> " + Ressource.GetString("btnRecord");
            btnCancel.Text = "<i class='fa fa-times'></i> " + Ressource.GetString("btnCancel");
            btnRemove.Text = "<i class='fa fa-trash-alt'></i> " + Ressource.GetString("btnRemove");
            btnDuplicate.Text = "<i class='fa fa-copy'></i> " + Ressource.GetString("btnDuplicate");
            btnSend.Text = "<i class='fa fa-caret-square-right'></i> " + Ressource.GetString(btnSend.ID);
            btnAttachedResults.Text = "<i class='fas fa-paperclip'></i>";
            btnSeeMsg.Text = "<i class='fa fa-caret-square-right'></i> Message";
        }
        #region CreateChildControls

        /// <summary>
        /// 
        /// </summary>
        /// EG 20200318 [24683] New (implementation autocomplete)
        /// FI 20210304 [XXXXX] autocomplete is now in aspx page
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
        #endregion CreateChildControls
        //* --------------------------------------------------------------- *//
        // Changement de sélection sur les Dropdowns
        //* --------------------------------------------------------------- *//
        #region OnRequestTypeChanged
        /// <summary>
        /// Changement du TYPE d'action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRequestTypeChanged(object sender, EventArgs e)
        {

            Cst.PosRequestTypeEnum requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(ddlRequestType.SelectedValue);
            Control tblReopeningPosition = FindControl("tblReopeningPositions");
            if (null != tblReopeningPosition)
                tblReopeningPosition.Visible = (requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition);
        }
        #endregion OnRequestTypeChanged
        #region OnGProductTypeInstrChanged
        protected void OnGProductTypeInstrChanged(object sender, EventArgs e)
        {
            SetDDLIDInstr();
        }
        #endregion OnGProductTypeInstrChanged
        #region OnTypeContractChanged
        // EG 20200318 [24683] Upd (implementation autocomplete)
        protected void OnTypeContractChanged(object sender, EventArgs e)
        {
            txtIDContract.Text = string.Empty;
        }
        #endregion OnTypeContractChanged
        #region OnTypeDealerClearerChanged
        // EG 20200318 [24683] Upd (implementation autocomplete)
        // EG 20240520 [WI930] Upd
        protected void OnTypeDealerClearerCssCustodianChanged(object sender, EventArgs e)
        {
            if ((sender as Control).ID.Contains("Dealer"))
                txtIDDealer_C.Text = string.Empty;
            else if ((sender as Control).ID.Contains("Clearer"))
                txtIDClearer_C.Text = string.Empty;
            else if ((sender as Control).ID.Contains("CssCustodian"))
                txtIDCssCustodian_C.Text = string.Empty;
        }
        #endregion OnTypeDealerClearerChanged
        #region OnMethodClosingChanged
        protected void OnMethodClosingChanged(object sender, EventArgs e)
        {
            SetDDLClosingPrice();
        }
        #endregion OnMethodClosingChanged
        #region OnMethodOpeningChanged
        protected void OnMethodOpeningChanged(object sender, EventArgs e)
        {
            SetDDLReopeningPrice();
        }
        #endregion OnMethodOpeningChanged
        #region SetDDLEnabledOrDisablePrice
        protected void SetDDLEnabledOrDisablePrice(DropDownList pDdlPrice, bool pIsEnable, TransferPriceEnum pTransferPrice)
        {
            ListItem item = pDdlPrice.Items.FindByText(pTransferPrice.ToString());
            if (null != item)
            {
                if (pIsEnable)
                    item.Attributes.Remove("disabled");
                else
                    item.Attributes.Add("disabled","disabled");
            }
        }
        #endregion SetDDLEnabledOrDisablePrice
        #endregion Events

        #region Methods

        /// <summary>
        /// ● Affichage du message destiné au service NormMsgFactory
        ///   Alimentation des classes
        ///   Construction et sérialisation du message
        ///   Affichage du fichier résultat
        /// </summary>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private void DisplayInstructions()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            ClosingReopeningAction _closingReopeningAction = null;
            switch (m_ARQInfo.Mode)
            {
                case Cst.Capture.ModeEnum.New:
                    _closingReopeningAction = new ClosingReopeningAction(SessionTools.CS);
                    break;
                case Cst.Capture.ModeEnum.Update:
                    _closingReopeningAction = m_ClosingReopeningAction;
                    break;
            }
            if (null != _closingReopeningAction)
                ret = FillClass(_closingReopeningAction);

            if (Cst.ErrLevel.SUCCESS == ret)
            {
                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(_closingReopeningAction.GetType(), _closingReopeningAction);
                StringBuilder sb = CacheSerializer.Serialize(serializeInfo, new UTF8Encoding());
                DisplayXml("ClosingReopening", "ClosingReopening.xml", sb.ToString());
            }
        }

        /// <summary>
        /// ● Envoi d'un message d'intégration d'une corporate Action vers le service NormMsgFactory 
        ///   Type de process : Cst.ProcessTypeEnum.CORPOACTIONINTEGRATE
        /// </summary>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        protected Cst.ErrLevel SendNormMsgARQ()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            m_ClosingReopeningAction.normMsgFactoryMQueue.id = m_ClosingReopeningAction.IdARQ;
            m_ClosingReopeningAction.normMsgFactoryMQueue.identifierSpecified = m_ClosingReopeningAction.identifierSpecified;
            m_ClosingReopeningAction.normMsgFactoryMQueue.identifier = m_ClosingReopeningAction.identifier;

            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                process = Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE,
                connectionString = SessionTools.CS,
                Session = SessionTools.AppSession,
                trackerAttrib = new TrackerAttributes()
                {
                    process = Cst.ProcessTypeEnum.CLOSINGREOPENINGINTEGRATE,
                    info = TrackerAttributes.BuildInfo(m_ClosingReopeningAction.normMsgFactoryMQueue)
                },
                mQueue = new MQueueBase[1] { m_ClosingReopeningAction.normMsgFactoryMQueue }
            };

            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);
            var (isOk, _) = MQueueTaskInfo.SendMultiple(taskInfo);
            if (isOk)
                ret = Cst.ErrLevel.SUCCESS;
            return ret;
        }
        //* --------------------------------------------------------------- *//
        // Initialisation des données 
        //* --------------------------------------------------------------- *//

        #region ComplementaryInitialize
        /// <summary>
        /// Initialisation complémentaire
        /// </summary>
        // EG 20190613 [24683] Upd
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private void ComplementaryInitialize()
        {
            bool isUpdate = (m_ARQInfo.Mode == Cst.Capture.ModeEnum.Update);

            lblIDARQ.Visible = (m_ARQInfo.Mode == Cst.Capture.ModeEnum.Update);
            if (lblIDARQ.Visible)
            {
                lblIDARQ.Text = "Id: " + m_ClosingReopeningAction.spheresid;
                lblIDARQ.CssClass = cssClassMaster;
            }

            btnRefresh.Visible = isUpdate;
            btnDuplicate.Visible = isUpdate;
            btnRemove.Visible = isUpdate;
            btnSend.Visible = false;
        }
        #endregion ComplementaryInitialize
        #region SetDDLIDInstr
        private void SetDDLIDInstr()
        {
            ddlIDInstr.Items.Clear();
            string typeInstr = ddlTypeInstr.SelectedValue;
            if (StrFunc.IsFilled(typeInstr))
            {
                TypeInstrEnum instr = ReflectionTools.ConvertStringToEnum<TypeInstrEnum>(typeInstr);
                if (TypeInstrEnum.GrpProduct == instr)
                {
                    ControlsTools.DDLLoad_ProductGProduct_Trading(ddlIDInstr, true);
                }
                else if (null != m_LstIdInstr)
                {
                    ddlIDInstr.Items.Add(new ListItem(string.Empty, string.Empty));
                    IEnumerable<ListItem> filter = m_LstIdInstr.Where(item => item["TYPEINSTR"].Value.ToString() == typeInstr).
                        Select(item => new ListItem(item["IDENTIFIER"].Value.ToString(), item["ID"].Value.ToString()));

                    if (0 < filter.Count())
                        ddlIDInstr.Items.AddRange(filter.ToArray());
                }
            }
        }
        #endregion SetDDLIDInstr()
        #region SetDDLLinkParty
        private void SetDDLLinkParty(DropDownList pDDLLinkParty)
        {
            pDDLLinkParty.Items.Clear();
            pDDLLinkParty.Items.Add(new ListItem(string.Empty, string.Empty));
            if (null != m_LstDefineExtlID)
            {
                IEnumerable<ListItem> filter = m_LstDefineExtlID.Select(item =>
                    new ListItem("[" + item["TABLENAME"].Value.ToString() + "] " + item["DISPLAYNAME"].Value.ToString(), item["TABLENAME"].Value.ToString() + "|" + item["IDENTIFIER"].Value.ToString()));

                if (0 < filter.Count())
                    pDDLLinkParty.Items.AddRange(filter.ToArray());
            }
        }
        #endregion SetDDLIdParty
        #region SetDDLFeeAction
        private void SetDDLFeeAction(DropDownList pDDLFeeAction)
        {
            pDDLFeeAction.Items.Clear();
            pDDLFeeAction.Items.Add(new ListItem(string.Empty, string.Empty));
            if (null != m_LstFeeAction)
            {
                IEnumerable<ListItem> filter = m_LstFeeAction.Select(item => new ListItem(Ressource.GetString(item["MNU_DESC"].Value.ToString()), item["IDPERMISSION"].Value.ToString()));
                if (0 < filter.Count())
                    pDDLFeeAction.Items.AddRange(filter.ToArray());
            }
        }
        #endregion SetDDLFeeAction
        #region SetDDLActionRequestFilter
        /// <summary>
        /// Chargement de la Dropdown des filtres additionnels
        /// </summary>
        /// <param name="pDDLActionRequestFilter"></param>
        // EG 20231030 [WI725] New Closing/Reopening : LoadARQFilters
        private void SetDDLActionRequestFilter(DropDownList pDDLActionRequestFilter)
        {
            pDDLActionRequestFilter.Items.Clear();
            pDDLActionRequestFilter.Items.Add(new ListItem(string.Empty, string.Empty));
            if (null != m_LstActionRequestFilters)
            {
                IEnumerable<ListItem> filter = m_LstActionRequestFilters.Select(item => new ListItem(item["DISPLAYNAME"].Value.ToString(), item["IDENTIFIER"].Value.ToString()));
                if (0 < filter.Count())
                    pDDLActionRequestFilter.Items.AddRange(filter.ToArray());
            }
        }
        #endregion SetDDLFeeAction

        #region SetDDLClosingPrice
        // EG 20190318 Upd (TransferModeEnum replace TransferClosingModeEnum|TransferReopeningModeEnum) ClosingReopening Step3
        // EG 20190613 [24683] Upd
        // EG 20210408 [24683] CRP - Implémentation Moyenne Pondérée sur Prix de négo
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory
        private void SetDDLClosingPrice()
        {
            Cst.PosRequestTypeEnum requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(ddlRequestType.SelectedValue);
            TransferModeEnum mode = ReflectionTools.ConvertStringToEnum<TransferModeEnum>(ddlMode_C.SelectedItem.Text);
            if (chkIsSumClosingAmount.Visible)
            {
                chkIsSumClosingAmount.Enabled = (mode != TransferModeEnum.ReverseTrade);
                if (false == chkIsSumClosingAmount.Enabled)
                    chkIsSumClosingAmount.Checked = false;
            }
            if (chkIsDelisting.Visible)
            {
                chkIsDelisting.Enabled = (requestType == Cst.PosRequestTypeEnum.ClosingPosition);
                if (false == chkIsDelisting.Enabled)
                    chkIsDelisting.Checked = false;
            }
        }
        #endregion SetDDLClosingPrice
        #region SetDDLReopeningPrice
        // EG 20190318 Upd (TransferModeEnum replaceTransferClosingModeEnum|TransferReopeningModeEnum) ClosingReopening Step3
        // EG 20210408 [24683] CRP - Implémentation Moyenne Pondérée sur Prix de négo
        private void SetDDLReopeningPrice()
        {
            _ = ReflectionTools.ConvertStringToEnum<TransferModeEnum>(ddlMode_O.SelectedItem.Text);
            //SetDDLEnabledOrDisablePrice(ddlFUTPrice_O, (mode == TransferModeEnum.Trade), TransferPriceEnum.TradingPrice);
            //SetDDLEnabledOrDisablePrice(ddlEQTYPrice_O, (mode == TransferModeEnum.Trade), TransferPriceEnum.TradingPrice);
            //SetDDLEnabledOrDisablePrice(ddlOtherPrice_O, (mode == TransferModeEnum.Trade), TransferPriceEnum.TradingPrice);
        }
        #endregion SetDDLReopeningPrice

        #region InitializeData
        /// <summary>
        /// Initialisation Générale des données
        /// </summary>
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
        // EG 20240520 [WI930] Upd
        private void InitializeData()
        {
            string CS = SessionTools.CS;
            divbody.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster;

            pnlCharacteristicsGen.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster; ;
            pnlInstrumentalEnvironnment.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster; ;
            pnlClosingReopeningPositions.CssClass = CSSMode + " " + mainMenuName + " " + cssClassMaster; ;

            // RequestType
            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments("[code:PosRequestTypeEnum;forcedenum:CLOSINGPOS|CLOSINGREOPENINGPOS]", false);
            ControlsTools.DDLLoad_ENUM(ddlRequestType, CS, loadEnum);
            loadEnum = LoadEnumArguments.GetArguments("[code:ActionRequestReadyStateEnum;]", true);
            ControlsTools.DDLLoad_ENUM(ddlReadyState, CS, loadEnum);
            // Timing
            loadEnum = LoadEnumArguments.GetArguments("[code:SettlSessIDEnum;forcedenum:SOD|EOD|EODSOD]", true);
            ControlsTools.DDLLoad_ENUM(ddlTiming, CS, loadEnum);


            LoadGProductTypeContractAndTypeInstr();

            LoadARQFilters();
            LoadClosingReopeningInstructions();

            txtDescription.TextMode = TextBoxMode.MultiLine;
            txtDescription.MaxLength = 2000;
        }
        #endregion InitializeData

        //* --------------------------------------------------------------- *//
        // Chargement des données de base: 
        // TYPEINSTR, IDINSTR, TYPECONTRACT, IDCONTRACT, 
        //* --------------------------------------------------------------- *//
        #region LoadGProductTypeContractAndTypeInstr
        private void LoadGProductTypeContractAndTypeInstr()
        {
            string CS = SessionTools.CS;
            // Type de contrat
            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments("[code:TypeContractEnum;forcedenum:None]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlTypeContract, CS, loadEnum);
            // Type Instrument
            loadEnum = LoadEnumArguments.GetArguments("[code:TypeInstrEnum;forcedenum:None]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlTypeInstr, CS, loadEnum);
        }
        #endregion LoadGProductTypeContractAndTypeInstr
        #region LoadClosingReopeningInstructions
        // EG 20190318 Upd (TransferModeEnum replaceTransferClosingModeEnum|TransferReopeningModeEnum) ClosingReopening Step3
        // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
        // EG 20240520 [WI930] Upd
        private void LoadClosingReopeningInstructions()
        {
            string CS = SessionTools.CS;
            // Entity
            ControlsTools.DDLLoad_ActorEntity(CS, ddlEntity, true, false, false, true);
            // Closing Party type
            LoadEnumArguments loadEnum = LoadEnumArguments.GetArguments("[code:TypePartyEnum;forcedenum:All|None]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlTypeDealer_C, CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlTypeClearer_C, CS, loadEnum);
            // EG 20240520 [WI930] New
            loadEnum = LoadEnumArguments.GetArguments("[code:TypePartyEnum;forcedenum:All|None|Book|GrpBook]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlTypeCssCustodian_C, CS, loadEnum);
            // Closing/Opening methods
            loadEnum = LoadEnumArguments.GetArguments("[code:TransferModeEnum;forcedenum:Trade|SyntheticPosition|LongShortPosition]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlMode_C, CS, loadEnum);
            loadEnum = LoadEnumArguments.GetArguments("[code:TransferModeEnum;forcedenum:ReverseTrade|ReverseSyntheticPosition|ReverseLongShortPosition]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlMode_O, CS, loadEnum);
            // Closing/Opening Price choice
            loadEnum = LoadEnumArguments.GetArguments("[code:TransferPriceEnum;forcedenum:Zero]", true);
            loadEnum.isExcludeForcedEnum = true;
            ControlsTools.DDLLoad_ENUM(ddlFUTPrice_O, CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlFUTPrice_C, CS, loadEnum);
            loadEnum = LoadEnumArguments.GetArguments("[code:TransferPriceEnum;]", true);
            ControlsTools.DDLLoad_ENUM(ddlEQTYPrice_C, CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlEQTYPrice_O, CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlOtherPrice_C, CS, loadEnum);
            ControlsTools.DDLLoad_ENUM(ddlOtherPrice_O, CS, loadEnum);
            // Closing/Opening Fee action
            SetDDLFeeAction(ddlFeeAction_C);
            SetDDLFeeAction(ddlFeeAction_O);
            SetDDLLinkParty(ddlLinkDealer_O);
            SetDDLLinkParty(ddlLinkClearer_O);
            // EG 20240520 [WI930] New
            SetDDLLinkParty(ddlLinkCssCustodian_O);
            SetDDLActionRequestFilter(ddlARQFilter);


        }
        #endregion LoadClosingReopeningInstructions
        #region LoadIDContract
        // EG 20190613 [24683] Upd
        // EG 20200318 [24683] Upd (implementation autocomplete)
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory (IsinUnderlyerContract & FIXML_SecurityExchange)
        private void LoadIDContract()
        {
            string CS = SessionTools.CS;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGCONTRACT_LEN), "CLOSINGREOPENING");

            string sqlQuery = @"select 'GrpMarket' as TYPEDATA, g.IDGMARKET as ID, g.IDENTIFIER
            from dbo.GMARKET g
            inner join dbo.GMARKETROLE gr on (gr.IDGMARKET = g.IDGMARKET) and (gr.IDROLEGMARKET = @ROLE)
            union all
            select 'Market' as TYPEDATA, IDM as ID, SHORT_ACRONYM as IDENTIFIER
            from dbo.VW_MARKET_IDENTIFIER
            union all
            select 'GrpContract' as TYPEDATA, g.IDGCONTRACT as ID, g.IDENTIFIER
            from dbo.GCONTRACT g
            inner join dbo.GCONTRACTROLE gr on (gr.IDGCONTRACT = g.IDGCONTRACT) and (gr.IDROLEGCONTRACT = @ROLE)
            union all
            select 'DerivativeContract' as TYPEDATA, dc.IDDC as ID, m.FIXML_SecurityExchange || ' - ' || dc.IDENTIFIER as IDENTIFIER
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = dc.IDM)
            union all
            select 'CommodityContract' as TYPEDATA, cc.IDCC as ID, m.FIXML_SecurityExchange || ' - ' || cc.IDENTIFIER as IDENTIFIER
            from dbo.COMMODITYCONTRACT cc
            inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = cc.IDM)
            union all
            select distinct 'IsinUnderlyerContract' as TYPEDATA, ast.IDASSET as ID, m.FIXML_SecurityExchange || ' - ' || munl.FIXML_SecurityExchange || ' / ' || ast.ISINCODE as IDENTIFIER
            from dbo.DERIVATIVECONTRACT dc
            inner join dbo.VW_ASSET ast on (ast.ASSETCATEGORY = dc.ASSETCATEGORY) and (ast.IDASSET = dc.IDASSET_UNL)
            inner join dbo.VW_MARKET_IDENTIFIER m on (m.IDM = dc.IDM)
            inner join dbo.VW_MARKET_IDENTIFIER munl on (munl.IDM = ast.IDM)
            order by TYPEDATA, IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                LstIdContract = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadIDContract
        #region LoadIDInstr
        // EG 20190613 [24683] Upd
        private void LoadIDInstr()
        {
            string CS = SessionTools.CS;
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGINSTR_LEN), "CLOSINGREOPENING");

            string sqlQuery = @"select 'Product' as TYPEINSTR, p.IDP as ID, p.IDENTIFIER, p.GPRODUCT
            from dbo.PRODUCT p
            where (p.GPRODUCT not in ('ADM','ASSET','RISK'))
            union all
            select 'GrpInstr' as TYPEINSTR, g.IDGINSTR as ID, g.IDENTIFIER, '*' as GPRODUCT
            from dbo.GINSTR g
            inner join dbo.GINSTRROLE gr on (gr.IDGINSTR = g.IDGINSTR) and (gr.IDROLEGINSTR = @ROLE)
            union all
            select 'Instr' as TYPEINSTR, i.IDI as ID, i.IDENTIFIER, p.GPRODUCT
            from dbo.INSTRUMENT i
            inner join dbo.PRODUCT p on (p.IDP = i.IDP) and (p.GPRODUCT not in ('ADM','ASSET','RISK'))";

            QueryParameters qryParameters = new QueryParameters(CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                m_LstIdInstr = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadIDInstr
        #region LoadIdParty
        // EG 20190613 [24683] Upd
        // EG 20200318 [24683] Upd (implementation autocomplete)
        private void LoadIdParty(bool pIsClearer)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGACTOR_LEN), "CLOSINGREOPENING");

            string roles = @"'COUNTERPARTY','CLIENT'"; 
            if (pIsClearer)
                roles = @"'CLEARER','CUSTODIAN','CCLEARINGCOMPART'";

            string sqlQuery = String.Format(@"select 'GrpActor' as TYPEDATA, g.IDGACTOR as ID, g.IDENTIFIER
            from dbo.GACTOR g
            inner join dbo.GACTORROLE gr on (gr.IDGACTOR = g.IDGACTOR) and (gr.IDROLEGACTOR = @ROLE)
            where g.IDGACTOR in (
                select ag.IDGACTOR
                from dbo.ACTORG ag
                inner join dbo.ACTOR ac on (ac.IDA = ag.IDA)
                inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR in ({0}))
                )

            union

            select 'GrpBook' as TYPEDATA, g.IDGBOOK as ID, g.IDENTIFIER
            from dbo.GBOOK g
            inner join dbo.GBOOKROLE gr on (gr.IDGBOOK = g.IDGBOOK) and (gr.IDROLEGBOOK = @ROLE)
            where g.IDGBOOK in (
                select bg.IDGBOOK
                from dbo.BOOKG bg
				inner join dbo.BOOK bk on (bk.IDB = bg.IDB)
                inner join dbo.ACTOR ac on (ac.IDA = bk.IDA)
                inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR in ({0}))
                )

            union

            select 'Actor' as TYPEDATA, a.IDA as ID, a.IDENTIFIER
            from dbo.ACTOR a
            inner join dbo.ACTORROLE ar on (ar.IDA = a.IDA) and (ar.IDROLEACTOR in ({0}))

            union

            select 'Book' as TYPEDATA, b.IDB as ID, b.IDENTIFIER
            from dbo.BOOK b
            inner join dbo.ACTORROLE ar on (ar.IDA = b.IDA) and (ar.IDROLEACTOR in ({0}))
            order by TYPEDATA, IDENTIFIER", roles);

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                if (pIsClearer)
                    LstIdClearer = DataReaderExtension.DataReaderMapToList(dr);
                else
                    LstIdDealer = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadIdParty
        #region LoadIdCssCustodian
        // EG 20240520 [WI930] New
        private void LoadIdCssCustodian()
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ROLE", DbType.AnsiStringFixedLength, SQLCst.UT_ROLEGACTOR_LEN), "CLOSINGREOPENING");

            string sqlQuery = $@"select 'GrpActor' as TYPEDATA, g.IDGACTOR as ID, g.IDENTIFIER
            from dbo.GACTOR g
            inner join dbo.GACTORROLE gr on (gr.IDGACTOR = g.IDGACTOR) and (gr.IDROLEGACTOR = @ROLE)
            where g.IDGACTOR in (
                select ag.IDGACTOR
                from dbo.ACTORG ag
                inner join dbo.ACTOR ac on (ac.IDA = ag.IDA)
                inner join dbo.ACTORROLE ar on (ar.IDA = ac.IDA) and (ar.IDROLEACTOR = '{RoleActor.CSS}')
                )
            union
            select 'Actor' as TYPEDATA, a.IDA as ID, a.IDENTIFIER
            from dbo.ACTOR a
            inner join dbo.ACTORROLE ar on (ar.IDA = a.IDA) and (ar.IDROLEACTOR = '{RoleActor.CSS}')
            order by TYPEDATA, IDENTIFIER";

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, parameters);
            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter()))
            {
                LstIdCssCustodian = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadIdCssCustodian
        #region LoadFeeAction
        /// <summary>
        /// Chargement des actions autorisées pour les frais de Fermeture et réouverture de position
        /// </summary>
        // EG 20190613 [24683] New
        // EG 20231030 [WI725] New Closing/Reopening : Add Correction for Fees
        private void LoadFeeAction()
        {
            string sqlQuery = @"select MNU_DESC, IDPERMISSION, MENU_DESCRIPTION, IDMENU
            from dbo.VW_ALL_VW_PERMIS_MENU
            where (IDMENU in ('OTC_INP_TRD', 'OTC_INP_TRD_POT', 'OTC_INP_TRD_POC')) and (PERMISSION = 'Create')";

            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlQuery, null))
            {
                m_LstFeeAction = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadFeeAction
        #region LoadDefineExtlID
        private void LoadDefineExtlID()
        {
            string sqlQuery = @"select ext.TABLENAME, ext.IDENTIFIER, ext.DISPLAYNAME, ext.DESCRIPTION
            from dbo.DEFINEEXTLID ext
            where ext.TABLENAME in ('ACTOR','BOOK') and ext.IDENTIFIER like 'CRP_%'";

            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlQuery, null))
            {
                m_LstDefineExtlID = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        #endregion LoadDefineExtlID

        #region LoadClosingReopeningAction
        private void LoadClosingReopeningAction()
        {
            m_ClosingReopeningAction = new ClosingReopeningAction(SessionTools.CS);
            m_ClosingReopeningAction.Load(m_ARQInfo, ARQTools.ARQWhereMode.ID);
            SetValue();

        }
        #endregion LoadClosingReopeningAction

        #region LoadARQFilters
        /// <summary>
        /// Chargement des filtres additionnels dans une liste
        /// </summary>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFilters
        private void LoadARQFilters()
        {
            string sqlQuery = @"select IDENTIFIER, DISPLAYNAME, DESCRIPTION, DTENABLED, DTDISABLED
            from dbo.ACTIONREQUESTFILTER";

            using (IDataReader dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, sqlQuery, null))
            {
                m_LstActionRequestFilters = DataReaderExtension.DataReaderMapToList(dr);
            }
        }
        # endregion LoadARQFilters
        //* --------------------------------------------------------------- *//
        // Alimentation des classes avec les valeurs des contrôles WEB
        //* --------------------------------------------------------------- *//
        #region FillClass
        /// <summary>
        /// Alimentation de la classe ClosingReopeningAction
        /// </summary>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        private Cst.ErrLevel FillClass(ClosingReopeningAction pClosingReopeningAction)
        {
            pClosingReopeningAction.identifierSpecified = StrFunc.IsFilled(txtIdentifier.Text);
            pClosingReopeningAction.identifier = txtIdentifier.Text;
            pClosingReopeningAction.displaynameSpecified = StrFunc.IsFilled(txtDisplayName.Text);
            pClosingReopeningAction.displayname = txtDisplayName.Text;
            pClosingReopeningAction.descriptionSpecified = StrFunc.IsFilled(txtDescription.Text);
            pClosingReopeningAction.description = txtDescription.Text;
            pClosingReopeningAction.effectiveDate = new EFS_Date(txtEffectiveDate.Text).DateValue;
            pClosingReopeningAction.effectiveEndDateSpecified = StrFunc.IsFilled(txtEffectiveEndDate.Text);
            if (pClosingReopeningAction.effectiveEndDateSpecified)
                pClosingReopeningAction.effectiveEndDate = new EFS_Date(txtEffectiveEndDate.Text).DateValue;
            pClosingReopeningAction.readystate = ARQTools.ActionRequestReadyStateEnum.RESERVED;
            if (StrFunc.IsFilled(ddlReadyState.SelectedItem.Text))
                pClosingReopeningAction.readystate = ARQTools.ReadyState(ddlReadyState.SelectedValue).Value;
            pClosingReopeningAction.requestType = ReflectionTools.ConvertStringToEnum<Cst.PosRequestTypeEnum>(ddlRequestType.SelectedValue);

            pClosingReopeningAction.timing = ReflectionTools.ConvertStringToEnum<SettlSessIDEnum>(ddlTiming.SelectedValue);


            pClosingReopeningAction.Environment = FillClassEnvironmentInstructions();
            pClosingReopeningAction.EnvironmentSpecified = pClosingReopeningAction.Environment.IsSpecified;

            pClosingReopeningAction.EntitySpecified = StrFunc.IsFilled(ddlEntity.SelectedItem.Text);
            if (pClosingReopeningAction.EntitySpecified)
                pClosingReopeningAction.Entity = Convert.ToInt32(ddlEntity.SelectedValue);

            pClosingReopeningAction.Closing = FillClassClosingInstructions();

            pClosingReopeningAction.ReopeningSpecified = (pClosingReopeningAction.requestType == Cst.PosRequestTypeEnum.ClosingReopeningPosition);
            if (pClosingReopeningAction.ReopeningSpecified)
                pClosingReopeningAction.Reopening = FillClassReopeningInstructions();

            pClosingReopeningAction.ArqFilterSpecified = StrFunc.IsFilled(ddlARQFilter.SelectedItem.Text);
            if (pClosingReopeningAction.ArqFilterSpecified)
                pClosingReopeningAction.ArqFilter = FillARQFiltersInstructions(ddlARQFilter);
            return Cst.ErrLevel.SUCCESS;
        }
        #endregion FillClass
        #region FillARQFiltersInstructions
        /// <summary>
        /// Initialisation de la classe du Filtre additionnel
        /// </summary>
        /// <param name="pDDLFilter"></param>
        /// <returns></returns>
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER
        private ActionFilter FillARQFiltersInstructions(DropDownList pDDLFilter)
        {
            ActionFilter arqFilter = new ActionFilter
            {
                identifierSpecified = true,
                identifier = pDDLFilter.SelectedValue,
                displaynameSpecified = true,
                displayname = pDDLFilter.SelectedItem.Text,
                dtEnabled = GetARQFilterDates(pDDLFilter.SelectedValue, "DTENABLED"),
                dtDisabled = GetARQFilterDates(pDDLFilter.SelectedValue, "DTDISABLED"),
            };
            return arqFilter;
        }
        #endregion FillARQFiltersInstructions
        #region FillClassActorInstructions
        // EG 20200318 [24683] Upd (implementation autocomplete)
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
            // EG 20240520 [WI930] Upd
        private ActorInstructions FillClassActorInstructions(DropDownList pDDLTypeActor, TextBox pTXTIdActor)
        {
            ActorInstructions actor = null;
            if (StrFunc.IsFilled(pDDLTypeActor.SelectedItem.Text))
            {
                actor = new ActorInstructions
                {
                    type = ReflectionTools.ConvertStringToEnum<TypePartyEnum>(pDDLTypeActor.SelectedItem.Value),
                    identifier = pTXTIdActor.Text.ToUpper().Trim()
                };

                if (pDDLTypeActor.ID.Contains("Dealer"))
                    actor.value = GetIdDealerId(actor.identifier);
                else if (pDDLTypeActor.ID.Contains("Clearer"))
                    actor.value = GetIdClearerId(actor.identifier);
                else if (pDDLTypeActor.ID.Contains("CssCustodian"))
                    actor.value = GetIdCssCustodianId(actor.identifier);
            }
            return actor;
        }
        #endregion FillClassActorInstructions
        #region FillClassClosingInstructions
        /// <summary>
        /// Sauvegarde des instructions de fermeture dans la classe
        /// </summary>
        /// <returns></returns>
        // EG 20190318 Upd (TransferModeEnum replaceTransferClosingModeEnum) ClosingReopening Step3
        // EG 20190613 [24683] Upd
        // EG 20200318 [24683] Upd (implementation autocomplete)
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20240520 [WI930] New cssCustodian
        private ClosingInstructions FillClassClosingInstructions()
        {
            ClosingInstructions closing = new ClosingInstructions
            {
                Mode = ReflectionTools.ConvertStringToEnum<TransferModeEnum>(ddlMode_C.SelectedItem.Text),
                isSumClosingAmountSpecified = (chkIsSumClosingAmount.Enabled),
                isDelistingSpecified = chkIsDelisting.Enabled,
                dealer = FillClassActorInstructions(ddlTypeDealer_C, txtIDDealer_C),
                clearer = FillClassActorInstructions(ddlTypeClearer_C, txtIDClearer_C),
                cssCustodian = FillClassActorInstructions(ddlTypeCssCustodian_C,txtIDCssCustodian_C),
                Price = FillClassPriceInstructions(ddlEQTYPrice_C, ddlFUTPrice_C, ddlOtherPrice_C),
                FeeAction = FillClassFeeActionInstructions(ddlFeeAction_C),
            };
            if (closing.isSumClosingAmountSpecified)
                closing.isSumClosingAmount = chkIsSumClosingAmount.Checked;
            if (closing.isDelistingSpecified)
                closing.isDelisting = chkIsDelisting.Checked;
            closing.dealerSpecified = (null != closing.dealer);
            closing.clearerSpecified = (null != closing.clearer);
            closing.cssCustodianSpecified = (null != closing.cssCustodian);
            closing.FeeActionSpecified = (0 < closing.FeeAction.value);
            return closing;
        }
        #endregion FillClassClosingInstructions
        #region FillClassEnvironmentInstructions
        /// <summary>
        /// Sauvegarde des instructions environnementales dans la classe
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd
        // EG 20200318 [24683] Upd (implementation autocomplete)
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        private EnvironmentInstructions FillClassEnvironmentInstructions()
        {
            EnvironmentInstructions env = new EnvironmentInstructions
            {
                instrSpecified = StrFunc.IsFilled(ddlTypeInstr.SelectedItem.Text),
                contractSpecified = StrFunc.IsFilled(ddlTypeContract.SelectedItem.Text)
            };

            if (env.instrSpecified)
            {
                env.instr = new TypeInstrument
                {
                    type = ReflectionTools.ConvertStringToEnum<TypeInstrEnum>(ddlTypeInstr.SelectedValue),
                    value = ddlIDInstr.SelectedValue,
                    identifier = ddlIDInstr.SelectedItem.Text.ToUpper().Trim()
                };
            }


            if (env.contractSpecified)
            {
                env.contract = new TypeContract
                {
                    type = ReflectionTools.ConvertStringToEnum<TypeContractEnum>(ddlTypeContract.SelectedValue),
                    value = GetIdContractId(txtIDContract.Text.ToUpper().Trim()),
                    identifier = txtIDContract.Text.ToUpper().Trim()
                };
            }
            return env;
        }
        #endregion FillClassEnvironmentInstructions
        #region FillClassFeeActionInstructions
        /// <summary>
        /// Sauvegarde des instructions de calcul de frais pour la
        /// fermeture et/ou la réouverture
        /// </summary>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        private FeeActionInstructions FillClassFeeActionInstructions(DropDownList pDDLFeeAction)
        {
            FeeActionInstructions fee = new FeeActionInstructions();
            if (0 < pDDLFeeAction.SelectedIndex)
            {
                fee.value = Convert.ToInt32(pDDLFeeAction.SelectedValue);
                fee.identifier = GetFeeActionIdentifier(fee.value);
            }
            return fee;
        }
        #endregion FillClassFeeActionInstructions
        #region FillClassLinkActorInstructions
        /// <summary>
        /// Sauvegarde des instructions acteurs pour la réouverture dans la classe
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd
        private LinkActorInstructions FillClassLinkActorInstructions(DropDownList pDDLLinkActor)
        {
            LinkActorInstructions linkActor = null;
            if (StrFunc.IsFilled(pDDLLinkActor.SelectedItem.Text))
            {
                linkActor = new LinkActorInstructions();
                string[] link = pDDLLinkActor.SelectedValue.Split('|');
                linkActor.table = link[0];
                linkActor.column = link[1];
            }
            return linkActor;
        }
        #endregion FillClassLinkActorInstructions
        #region FillClassPriceInstructions
        /// <summary>
        /// Sauvegarde des instructions de prix utilisés dans la classe
        /// </summary>
        /// <returns></returns>
        // EG 20190613 [24683] Upd
        private PriceInstructions FillClassPriceInstructions(DropDownList pDDLEQTYPrice, DropDownList pDDLFUTPrice, DropDownList pDDLOtherPrice)
        {
            PriceInstructions price = new PriceInstructions
            {
                EqtyPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pDDLEQTYPrice.SelectedItem.Text),
                FutPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pDDLFUTPrice.SelectedItem.Text),
                OtherPrice = ReflectionTools.ConvertStringToEnum<TransferPriceEnum>(pDDLOtherPrice.SelectedItem.Text)
            };
            return price;
        }
        #endregion FillClassPriceInstructions
        #region FillClassReopeningInstructions
        /// <summary>
        /// Sauvegarde des instructions de réouverture dans la classe
        /// </summary>
        /// <returns></returns>
        // EG 20190318 Upd (TransferModeEnum TransferReopeningModeEnum) ClosingReopening Step3
        // EG 20190613 [24683] Upd
        // EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20240520 [WI930] New cssCustodian
        private ReopeningInstructions FillClassReopeningInstructions()
        {
            ReopeningInstructions reopening = new ReopeningInstructions
            {
                Mode = ReflectionTools.ConvertStringToEnum<TransferModeEnum>(ddlMode_O.SelectedItem.Text),
                dealer = FillClassLinkActorInstructions(ddlLinkDealer_O),
                clearer = FillClassLinkActorInstructions(ddlLinkClearer_O),
                cssCustodian = FillClassLinkActorInstructions(ddlLinkCssCustodian_O),
                Price = FillClassPriceInstructions(ddlEQTYPrice_O, ddlFUTPrice_O, ddlOtherPrice_O),
                FeeAction = FillClassFeeActionInstructions(ddlFeeAction_O)
            };
            reopening.dealerSpecified = (null != reopening.dealer);
            reopening.clearerSpecified = (null != reopening.clearer);
            reopening.cssCustodianSpecified = (null != reopening.cssCustodian);

            reopening.FeeActionSpecified = (0 < reopening.FeeAction.value);
            return reopening;
        }
        #endregion FillClassReopeningInstructions

        //* --------------------------------------------------------------- *//
        // Actions sur les boutons de la barre d'outils
        //* --------------------------------------------------------------- *//
        #region Duplicate
        /// <summary>
        /// Duplication de la demande
        /// </summary>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        protected void Duplicate()
        {
            if (null != m_ClosingReopeningAction)
            {
                m_ClosingReopeningAction.readystate = ARQTools.ActionRequestReadyStateEnum.REGULAR;
                m_ClosingReopeningAction.IdARQ = 0;
                m_ARQInfo.Mode = Cst.Capture.ModeEnum.New;
                m_ARQInfo.Id = null;
                SetValue();
                ComplementaryInitialize();
            }
        }
        #endregion Duplicate


        #region Record
        /// <summary>
        /// Enregistrement du context dans la base
        /// </summary>
        /// <returns></returns>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        private Cst.ErrLevel Record()
        {
            if (m_ARQInfo.Mode == Cst.Capture.ModeEnum.New)
                m_ClosingReopeningAction = new ClosingReopeningAction(SessionTools.CS);

            Cst.ErrLevel ret = FillClass(m_ClosingReopeningAction);
            if (ret == Cst.ErrLevel.SUCCESS)
                ret = m_ClosingReopeningAction.ConstructNormMsgFactoryMessage();
            if (ret == Cst.ErrLevel.SUCCESS)
                ret = m_ClosingReopeningAction.Write(m_ARQInfo);
            return ret;
        }
        #endregion Record
        #region Refresh
        /// <summary>
        /// Réactualisation du context en cours
        /// </summary>
        protected void Refresh()
        {
            if (m_ARQInfo.Id.HasValue)
            {
                LoadClosingReopeningAction();
                ComplementaryInitialize();
                SetValidators();
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //Session[ARQ_GUID] = m_ClosingReopeningAction;
                DataCache.SetData<ClosingReopeningAction>(ARQ_GUID, m_ClosingReopeningAction);
            }
        }
        #endregion Refresh
        #region ResetAll
        /// <summary>
        /// Remise à zéro de la page 
        /// </summary>
        // EG 20200318 [24683] Upd (implementation autocomplete)
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add cssCustodian
        protected void ResetAll()
        {
            // Caractéristiques générales
            txtIdentifier.Text = string.Empty;
            txtDisplayName.Text = string.Empty;
            txtDescription.Text = string.Empty;
            txtEffectiveDate.Text = string.Empty;
            txtEffectiveEndDate.Text = string.Empty;
            ddlRequestType.SelectedIndex = -1;
            ddlReadyState.SelectedIndex = -1;
            ddlTiming.SelectedIndex = -1;

            // Environnement instrumental
            ddlTypeInstr.SelectedIndex = -1;
            ddlIDInstr.SelectedIndex = -1;
            ddlTypeContract.SelectedIndex = -1;
            txtIDContract.Text = string.Empty;

            // Entité 
            ddlEntity.SelectedIndex = -1;

            // ARQFilter
            ddlARQFilter.SelectedIndex = -1;

            // Closing data
            ddlMode_C.SelectedIndex = -1;
            ddlTypeDealer_C.SelectedIndex = -1;
            txtIDDealer_C.Text = string.Empty;
            ddlTypeClearer_C.SelectedIndex = -1;
            txtIDClearer_C.Text = string.Empty;
            txtIDCssCustodian_C.Text = string.Empty;
            ddlEQTYPrice_C.SelectedIndex = -1;
            ddlFUTPrice_C.SelectedIndex = -1;
            ddlOtherPrice_C.SelectedIndex = -1;
            ddlFeeAction_C.SelectedIndex = -1;

            // ReOpening data
            ddlMode_O.SelectedIndex = -1;
            ddlLinkDealer_O.SelectedIndex = -1;
            ddlLinkClearer_O.SelectedIndex = -1;
            ddlLinkCssCustodian_O.SelectedIndex = -1;
            ddlEQTYPrice_O.SelectedIndex = -1;
            ddlFUTPrice_O.SelectedIndex = -1;
            ddlOtherPrice_O.SelectedIndex = -1;
            ddlFeeAction_O.SelectedIndex = -1;

            m_ARQInfo = new ARQInfo(SessionTools.User.IdA);
            //m_ActionRequest = null;
            btnRefresh.Visible = (m_ARQInfo.Mode == Cst.Capture.ModeEnum.Update);
            btnRemove.Enabled = (m_ARQInfo.Mode == Cst.Capture.ModeEnum.Update);

            SetDefault();

        }
        #endregion ResetAll

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
            m_ARQInfo = (ARQInfo)viewState[1];
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

            object[] ret = new object[2];
            ret[0] = base.SaveViewState();
            ret[1] = m_ARQInfo;

            return ret;
        }
        #endregion SaveViewState

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
        /// EG 20210408 [24683] Modification Gestion des contrôles de validation
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20240520 [WI930] Add cssCustodian
        protected void ControlToValidate(object source, ServerValidateEventArgs args)
        {
            string defaultMessage = "InvalidData";
            if (source is CustomValidator cv)
            {
                DropDownList ddl = FindControl(cv.ControlToValidate) as DropDownList; 
                switch (cv.ControlToValidate)
                {
                    case "txtIDContract":
                        args.IsValid = (String.IsNullOrEmpty(ddlTypeContract.SelectedValue) && String.IsNullOrEmpty(txtIDContract.Text)) ||
                                        (StrFunc.IsFilled(ddlTypeContract.SelectedValue) && StrFunc.IsFilled(txtIDContract.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "txtIDDealer_C":
                        args.IsValid = (String.IsNullOrEmpty(ddlTypeDealer_C.SelectedValue) && String.IsNullOrEmpty(txtIDDealer_C.Text)) ||
                                        (StrFunc.IsFilled(ddlTypeDealer_C.SelectedValue) && StrFunc.IsFilled(txtIDDealer_C.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "txtIDClearer_C":
                        args.IsValid = (String.IsNullOrEmpty(ddlTypeClearer_C.SelectedValue) && String.IsNullOrEmpty(txtIDClearer_C.Text)) ||
                                        (StrFunc.IsFilled(ddlTypeClearer_C.SelectedValue) && StrFunc.IsFilled(txtIDClearer_C.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "txtIDCsscustodian_C":
                        args.IsValid = (String.IsNullOrEmpty(ddlTypeCssCustodian_C.SelectedValue) && String.IsNullOrEmpty(txtIDCssCustodian_C.Text)) ||
                                        (StrFunc.IsFilled(ddlTypeCssCustodian_C.SelectedValue) && StrFunc.IsFilled(txtIDCssCustodian_C.Text));
                        defaultMessage = "MissingData";
                        break;
                    case "ddlTypeDealer_C":
                        args.IsValid = StrFunc.IsFilled(ddlTypeDealer_C.SelectedValue) || 
                                       (String.IsNullOrEmpty(ddlTypeDealer_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeClearer_C.SelectedValue) || (ddlRequestType.SelectedValue == "CLOSINGPOS"))) ||
                                       (String.IsNullOrEmpty(ddlTypeDealer_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeCssCustodian_C.Text) || (ddlRequestType.SelectedValue == "CLOSINGPOS"))); 
                        break;
                    case "ddlTypeClearer_C":
                        args.IsValid = StrFunc.IsFilled(ddlTypeClearer_C.SelectedValue) || 
                                       (String.IsNullOrEmpty(ddlTypeClearer_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeDealer_C.SelectedValue)|| (ddlRequestType.SelectedValue == "CLOSINGPOS"))) ||
                                       (String.IsNullOrEmpty(ddlTypeClearer_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeCssCustodian_C.Text) || (ddlRequestType.SelectedValue == "CLOSINGPOS")));
                        break;
                    case "ddlTypeCssCustodian_C":
                        args.IsValid = StrFunc.IsFilled(ddlTypeCssCustodian_C.SelectedValue) ||
                                       (String.IsNullOrEmpty(ddlTypeCssCustodian_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeDealer_C.SelectedValue) || (ddlRequestType.SelectedValue == "CLOSINGPOS"))) ||
                                       (String.IsNullOrEmpty(ddlTypeCssCustodian_C.SelectedValue) && (StrFunc.IsFilled(ddlTypeClearer_C.Text) || (ddlRequestType.SelectedValue == "CLOSINGPOS")));
                        break;
                    case "ddlIDInstr":
                        args.IsValid = String.IsNullOrEmpty(ddlTypeInstr.SelectedValue) || (StrFunc.IsFilled(ddlTypeInstr.SelectedValue) && StrFunc.IsFilled(ddl.SelectedValue));
                        defaultMessage = "MissingData";
                        break;
                    case "ddlLinkDealer_O":
                        if (null != ddl)
                            args.IsValid = (String.IsNullOrEmpty(ddlTypeDealer_C.SelectedValue) && String.IsNullOrEmpty(ddl.SelectedValue))|| 
                                           (StrFunc.IsFilled(ddlTypeDealer_C.SelectedValue) && StrFunc.IsFilled(ddl.SelectedValue));
                        break;
                    case "ddlLinkClearer_O":
                        if (null != ddl)
                            args.IsValid = (String.IsNullOrEmpty(ddlTypeClearer_C.SelectedValue) && String.IsNullOrEmpty(ddl.SelectedValue)) ||
                                           (StrFunc.IsFilled(ddlTypeClearer_C.SelectedValue) && StrFunc.IsFilled(ddl.SelectedValue));
                        break;
                    case "ddlLinkCssCustodian_O":
                        if (null != ddl)
                            args.IsValid = (String.IsNullOrEmpty(ddlTypeCssCustodian_C.SelectedValue) && String.IsNullOrEmpty(ddl.SelectedValue)) ||
                                           (StrFunc.IsFilled(ddlTypeCssCustodian_C.SelectedValue) && StrFunc.IsFilled(ddl.SelectedValue));
                        break;
                    case "ddlRequestType":
                    case "ddlTiming":
                    case "ddlMode_C":
                    case "ddlEQTYPrice_C":
                    case "ddlFUTPrice_C":
                    case "ddlOtherPrice_C":
                    case "ddlMode_O":
                    case "ddlEQTYPrice_O":
                    case "ddlFUTPrice_O":
                    case "ddlOtherPrice_O":
                        // Données obligatoires
                        if (null != ddl)
                            args.IsValid = StrFunc.IsFilled(ddl.SelectedValue);
                        defaultMessage = "MissingData";
                        break;
                    default: 
                        break;
                }
                if (false == args.IsValid)
                {
                    // Construction du message d'erreur si non valide.
                    cv.SetFocusOnError = true;
                    cv.ErrorMessage = "<div>" + ValidatorTools.SetValidatorMessage(Page, cv.ControlToValidate, Ressource.GetString(defaultMessage)) + "</div>";
                }
            }
        }
        #endregion ControlToValidate

        #region IsAllValid
        /// <summary>
        /// Application des validateurs par chaque groupe de validation
        /// </summary>
        /// <returns></returns>
        private bool IsAllValid()
        {
            Page.Validate(ValidatorTools.ValidationGroupEnum.MAIN.ToString());
            Page.Validate(ValidatorTools.ValidationGroupEnum.PNL1.ToString());
            Page.Validate(ValidatorTools.ValidationGroupEnum.PNL2.ToString());
            Page.Validate(ValidatorTools.ValidationGroupEnum.PNL3.ToString());
            return Page.IsValid;
        }
        #endregion IsAllValid

        #region SetButtonAttachedDoc
        /// <summary>
        /// Initalisaton du bouton AttachedDoc
        /// </summary>
        // EG 20201104 [24683] New
        protected void SetButtonAttachedDoc()
        {
            if (0 < m_ClosingReopeningAction.SpheresId)
            {
                //FI 20140930 [XXXXX] Utilisation  de InputGUI.IdMenu
                // PM 20240604 [XXXXX] Ajout paramètre pParentGUID
                string urlAttachedDoc = JavaScript.GetUrlAttachedDoc("ATTACHEDDOC", m_ClosingReopeningAction.spheresid,m_ClosingReopeningAction.identifier, m_ClosingReopeningAction.description, 
                    IdMenu.GetIdMenu(IdMenu.Menu.InputClosingReopeningPosition), "ACTIONREQUEST", this.GUID);
                bool isAttachedDocSpecified = IsExistNotePadAttachedDoc(m_ClosingReopeningAction.SpheresId);
                btnAttachedResults.ToolTip = Ressource.GetString("btnAttachedDoc");
                btnAttachedResults.CssClass = String.Format("fa-icon {0}", isAttachedDocSpecified ? "green" : string.Empty);
                btnAttachedResults.Text = "<i class='fas fa-paperclip'></i>";
                btnAttachedResults.OnClientClick = @"OpenAttachedDoc('" + urlAttachedDoc + "','lblIDARQ','txtARQIdentifier'); return false;";
            }
        }
        #endregion SetButtonAttachedDoc
        /// <summary>
        /// Test d'existence de documents rattaché au context CRP courant
        /// </summary>
        /// <param name="pId"></param>
        /// <returns></returns>
        // EG 20201104 [24683] New
        private bool IsExistNotePadAttachedDoc(int pId)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), "ACTIONREQUEST");
            parameters.Add(new DataParameter(SessionTools.CS, "ID", DbType.Int32), pId);
            string sqlSelect = @"select 1 from dbo.ATTACHEDDOC np where np.TABLENAME = @TABLENAME and np.ID = @ID";
            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, parameters);
            object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return (null != obj) && BoolFunc.IsTrue(obj);
        }

        #region SetDefault
        /// <summary>
        /// Initialisation des contrôles à leurs valeurs par défaut.
        /// </summary>
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        private void SetDefault()
        {
            // Caractéristiques générales
            DateTime dtSys = OTCmlHelper.GetDateSys(SessionTools.CS).Date;
            txtEffectiveDate.Text = DtFunc.DateTimeToString(dtSys, DtFunc.FmtShortDate);
            txtEffectiveEndDate.Text = DtFunc.DateTimeToString(dtSys, DtFunc.FmtShortDate);
            ControlsTools.DDLSelectByValue(ddlRequestType, ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.ClosingPosition));
            OnRequestTypeChanged(ddlRequestType, null);
            ControlsTools.DDLSelectByValue(ddlReadyState, ReflectionTools.ConvertEnumToString<ARQTools.ActionRequestReadyStateEnum>(ARQTools.ActionRequestReadyStateEnum.REGULAR));
            ControlsTools.DDLSelectByValue(ddlTiming, ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(SettlSessIDEnum.StartOfDay));
        }
        #endregion SetDefault
        #region SetValue
        /// <summary>
        /// Chargement des contrôles de la page avec la classe
        /// </summary>
        // EG 20201104 [24683] Gestion du bouton AttachedDoc
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        private void SetValue()
        {
            if (m_ClosingReopeningAction.identifierSpecified)
                txtIdentifier.Text = m_ClosingReopeningAction.identifier;
            if (m_ClosingReopeningAction.displaynameSpecified)
                txtDisplayName.Text = m_ClosingReopeningAction.displayname;
            if (m_ClosingReopeningAction.descriptionSpecified)
                txtDescription.Text = m_ClosingReopeningAction.description;

            SetButtonAttachedDoc();

            ControlsTools.DDLSelectByValue(ddlReadyState, m_ClosingReopeningAction.readystate.ToString());
            ControlsTools.DDLSelectByValue(ddlRequestType, ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(m_ClosingReopeningAction.requestType));
            ControlsTools.DDLSelectByValue(ddlTiming, ReflectionTools.ConvertEnumToString<SettlSessIDEnum>(m_ClosingReopeningAction.timing));
            OnRequestTypeChanged(ddlRequestType, null);
            txtEffectiveDate.Text = DtFunc.DateTimeToString(m_ClosingReopeningAction.effectiveDate, DtFunc.FmtShortDate);
            if (m_ClosingReopeningAction.effectiveEndDateSpecified)
                txtEffectiveEndDate.Text = DtFunc.DateTimeToString(m_ClosingReopeningAction.effectiveEndDate, DtFunc.FmtShortDate);

            if (m_ClosingReopeningAction.EntitySpecified)
                ControlsTools.DDLSelectByValue(ddlEntity, m_ClosingReopeningAction.Entity.ToString());

            if (m_ClosingReopeningAction.ArqFilterSpecified)
                ControlsTools.DDLSelectByValue(ddlARQFilter, m_ClosingReopeningAction.ArqFilter.identifier);

            SetValueEnvironmentInstructions();
            SetValueClosingInstructions();
            SetValueReopeningInstructions();
        }
        #endregion SetValue
        #region SetValueEnvironmentInstructions
        /// <summary>
        /// Chargement des contrôles des instructions environnementales de la page avec la classe
        /// </summary>
        // EG 20200318 [24683] Upd (implementation autocomplete)
        private void SetValueEnvironmentInstructions()
        {
            if (m_ClosingReopeningAction.EnvironmentSpecified)
            {
                if (m_ClosingReopeningAction.Environment.instrSpecified)
                {
                    ControlsTools.DDLSelectByValue(ddlTypeInstr, m_ClosingReopeningAction.Environment.instr.type.ToString());
                    OnGProductTypeInstrChanged(ddlTypeInstr, null);
                    ControlsTools.DDLSelectByValue(ddlIDInstr, m_ClosingReopeningAction.Environment.instr.value);
                }

                if (m_ClosingReopeningAction.Environment.contractSpecified)
                {
                    ControlsTools.DDLSelectByValue(ddlTypeContract, m_ClosingReopeningAction.Environment.contract.type.ToString());
                    if (m_ClosingReopeningAction.Environment.contract.type == TypeContractEnum.IsinUnderlyerContract)
                    {
                        MQueueparameter _parameter = m_ClosingReopeningAction.normMsgFactoryMQueue.buildingInfo.parameters["CONTRACTTYPE"];
                        txtIDContract.Text = _parameter.name;
                    }
                    else
                    {
                        txtIDContract.Text = GetIdContractIdentifier(m_ClosingReopeningAction.Environment.contract.value);
                    }
                }
            }
        }
        #endregion SetValueEnvironmentInstructions
        #region SetValueClosingInstructions
        /// <summary>
        /// Chargement des contrôles des instructions de fermeture de la page avec la classe
        /// </summary>
        // EG 20190318 Upd (TransferModeEnum replaceTransferClosingModeEnum) ClosingReopening Step3
        // EG 20200318 [24683] Upd (implementation autocomplete)
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20240520 [WI930] Add cssCustodian
        private void SetValueClosingInstructions()
        {
            ClosingInstructions closing =  m_ClosingReopeningAction.Closing;
            ControlsTools.DDLSelectByValue(ddlMode_C, ReflectionTools.ConvertEnumToString<TransferModeEnum>(closing.Mode));
            ControlsTools.DDLSelectByValue(ddlEQTYPrice_C, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(closing.Price.EqtyPrice));
            ControlsTools.DDLSelectByValue(ddlFUTPrice_C, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(closing.Price.FutPrice));
            ControlsTools.DDLSelectByValue(ddlOtherPrice_C, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(closing.Price.OtherPrice));
            if (closing.FeeActionSpecified)
                ControlsTools.DDLSelectByValue(ddlFeeAction_C, closing.FeeAction.value.ToString());

            if (closing.isSumClosingAmountSpecified)
                chkIsSumClosingAmount.Checked = closing.isSumClosingAmount;

            if (closing.isDelistingSpecified)
                chkIsDelisting.Checked = closing.isDelisting;

            OnMethodClosingChanged(ddlMode_C, null);

            if (closing.dealerSpecified)
            {
                ControlsTools.DDLSelectByValue(ddlTypeDealer_C, closing.dealer.type.ToString());
                txtIDDealer_C.Text = GetIdDealerIdentifier(closing.dealer.value);
            }
            if (closing.clearerSpecified)
            {
                ControlsTools.DDLSelectByValue(ddlTypeClearer_C, closing.clearer.type.ToString());
                txtIDClearer_C.Text = GetIdClearerIdentifier(closing.clearer.value);
            }
            if (closing.cssCustodianSpecified)
            {
                ControlsTools.DDLSelectByValue(ddlTypeCssCustodian_C, closing.cssCustodian.type.ToString());
                txtIDCssCustodian_C.Text = GetIdCssCustodianIdentifier(closing.cssCustodian.value);
            }
        }
        #endregion SetValueClosingInstructions
        #region SetValueReopeningInstructions
        /// <summary>
        /// Chargement des contrôles des instructions de réouverture de la page avec la classe
        /// </summary>
        // EG 20190318 Upd (TransferModeEnum TransferReopeningModeEnum) ClosingReopening Step3
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20240520 [WI930] Add cssCustodian
        private void SetValueReopeningInstructions()
        {
            if (m_ClosingReopeningAction.ReopeningSpecified)
            {
                ReopeningInstructions reopening = m_ClosingReopeningAction.Reopening;
                ControlsTools.DDLSelectByValue(ddlMode_O, ReflectionTools.ConvertEnumToString<TransferModeEnum>(reopening.Mode));
                ControlsTools.DDLSelectByValue(ddlEQTYPrice_O, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(reopening.Price.EqtyPrice));
                ControlsTools.DDLSelectByValue(ddlFUTPrice_O, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(reopening.Price.FutPrice));
                ControlsTools.DDLSelectByValue(ddlOtherPrice_O, ReflectionTools.ConvertEnumToString<TransferPriceEnum>(reopening.Price.OtherPrice));

                if (reopening.FeeActionSpecified)
                    ControlsTools.DDLSelectByValue(ddlFeeAction_O, reopening.FeeAction.value.ToString());

                if (reopening.dealerSpecified)
                    ControlsTools.DDLSelectByValue(ddlLinkDealer_O, reopening.dealer.table + "|" + reopening.dealer.column);
                if (reopening.clearerSpecified)
                    ControlsTools.DDLSelectByValue(ddlLinkClearer_O, reopening.clearer.table + "|" + reopening.clearer.column);
                if (reopening.cssCustodianSpecified)
                    ControlsTools.DDLSelectByValue(ddlLinkCssCustodian_O, reopening.cssCustodian.table + "|" + reopening.cssCustodian.column);
            }
        }
        #endregion SetValueReopeningInstructions
        #region SetRessources
        /// <summary>
        /// Chargemlent des ressources
        /// </summary>
        /// EG 20210408 [24683] Modification Ressource Date d'effet
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        // EG 20231030 [WI725] New Closing/Reopening : ARQFILTER + EFFECTIVEENDDATE (action perpetuelle)
        // EG 20240520 [WI930] Add cssCustodian
        private void SetRessources()
        {
            // Boutons
            lblIdentifier.Text = Ressource.GetString("IDENTIFIER");
            lblDisplayName.Text = Ressource.GetString("DISPLAYNAME");
            lblDescription.Text = Ressource.GetString("DESCRIPTION");
            lblReadyState.Text = Ressource.GetString("Statut");
            lblRequestType.Text = Ressource.GetString("COL_TYPE_REQUEST");
            lblTiming.Text = Ressource.GetString("requestTiming");
            lblEffectiveDate.Text = Ressource.GetString("lblARQEffectiveDate");
            lblEffectiveEndDate.Text = Ressource.GetString("lblARQEffectiveEndDate");

            lblInstrumentalEnvironnment.Text = Ressource.GetString("InstrumentCriteria").Replace(Cst.HTMLBreakLine, string.Empty);
            lblTypeInstr.Text = Ressource.GetMulti("TYPEINSTR", 1);
            lblIDInstr.Text = Ressource.GetMulti("IDINSTR", 1);
            lblTypeContract.Text = Ressource.GetMulti("TYPECONTRACT", 1);
            lblIDContract.Text = Ressource.GetMulti("IDCONTRACT", 1);
            lblARQFilter.Text = Ressource.GetString("lblARQFilter");
            //chkIsSumClosingAmount.Visible = false;
            chkIsSumClosingAmount.Text = Ressource.GetString("lblSumClosingAmount_C");
            chkIsSumClosingAmount.TextAlign = TextAlign.Right;

            chkIsDelisting.Text = Ressource.GetString("lblDelisting_C");
            chkIsDelisting.TextAlign = TextAlign.Right;
            lblTypeDealer_C.Text = Ressource.GetMulti("TYPEDEALER", 1);
            lblIDDealer_C.Text = Ressource.GetMulti("IDDEALER", 1);
            lblTypeClearer_C.Text = Ressource.GetMulti("TYPECLEARER", 1);
            lblIDClearer_C.Text = Ressource.GetMulti("IDCLEARER", 1);
            lblTypeCssCustodian_C.Text = Ressource.GetMulti("TYPECSSCUSTODIAN", 1);
            lblIDCssCustodian_C.Text = Ressource.GetMulti("IDCSSCUSTODIAN", 1);
            lblChoicePrice_C.Text = Ressource.GetString("lblChoicePrice");
            lblEQTYPrice_C.Text = "PremiumStyle";
            lblFUTPrice_C.Text = "FuturesStyleMarkToMarket";
            lblOtherPrice_C.Text = Ressource.GetString("lblOthers");
            lblFeeAction_C.Text = Ressource.GetString("lblFeeAction");

            lblLinkDealer_O.Text = Ressource.GetMulti("lblMappingDealer");
            lblLinkClearer_O.Text = Ressource.GetMulti("lblMappingClearer");
            lblLinkCssCustodian_O.Text = Ressource.GetMulti("lblMappingCssCustodian");
            lblChoicePrice_O.Text = Ressource.GetString("lblChoicePrice");
            lblEQTYPrice_O.Text = "PremiumStyle";
            lblFUTPrice_O.Text = "FuturesStyleMarkToMarket";
            lblOtherPrice_O.Text = Ressource.GetString("lblOthers");
            lblFeeAction_O.Text = Ressource.GetString("lblFeeAction");


        }
        #endregion SetRessources

        #region SetTooltipEnum
        /// <summary>
        /// ● Lien hyperlink pour les énumérateurs de la page
        ///   CorporateEventGroupEnum
        ///   CorporateEventTypeEnum
        ///   AdjustmentMethodOfDerivContractEnum
        /// </summary>
        /// <param name="pControl">Control de l'énumérateur</param>
        /// <param name="pEnum">Nom de l'énumérateur dans ENUMS</param>
        // EG 20200901 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        // EG 20210315 [24683] Changement de nom de méthode (remplace SetTooltipInformation)
        private void SetTooltipEnum(Control pControl, string pEnum)
        {
            if (FindControl(pControl.ID.Replace(Cst.DDL.ToLower(), "ttip")) is WCToolTipPanel btn)
            {
                string url = String.Format("Referential.aspx?T=Repository&O=ENUMS&M=2&PK=CODE&PKV={0}&F=frmConsult&IDMenu={1}", pEnum, IdMenu.GetIdMenu(IdMenu.Menu.Enums));
                btn.Attributes.Add("onclick", JavaScript.GetWindowOpen(url, Cst.WindowOpenStyle.EfsML_ListReferential));
                btn.CssClass = "fa-icon fas fa-info-circle brown";
                btn.Attributes.Add("qtip-style", "qtip-brown");
                btn.Pty.TooltipTitle = pEnum;
                btn.Pty.TooltipStyle = "qtip-brown";

                // FI 20240731 [XXXXX] Mise en commentaire => use DataEnabledEnum/DataEnabledEnumHelper
                //ExtendEnums ListEnumsSchemes = ExtendEnumsTools.ListEnumsSchemes;
                //if (null != ListEnumsSchemes)
                //{
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
        #endregion SetTooltipEnum
        #region SetToolTipInfo
        // EG 20210315 [24683] Nouvelle méthode (tooltip informatif sur Referentiel)
        private void SetToolTipInfo(Control pControl, string pResMessage)
        {
            if (FindControl(pControl.ID.Replace(Cst.TXT.ToLower(), "ttip")) is WCToolTipPanel btn)
            {
                btn.CssClass = "fa-icon fas fa-info-circle blue";
                if (FindControl(pControl.ID.Replace(Cst.TXT.ToLower(), Cst.LBL.ToLower())) is WCTooltipLabel lbl)
                    btn.Pty.TooltipTitle = lbl.Text;
                btn.Pty.TooltipContent = Ressource.GetString(pResMessage);
            }
        }
        #endregion SetToolTipInfo
        #region SetOtherTooltip
        /// EG 20230901 [WI700] ClosingReopeningPosition - Delisting action - NormMsgFactory 
        private void SetOtherTooltip()
        {
            btnRefresh.Pty.TooltipContent = Ressource.GetString("Refresh");
            btnDuplicate.Pty.TooltipContent = Ressource.GetString("btnCADuplicate");
            btnSend.Pty.TooltipContent = Ressource.GetString("btnCASend");
            btnCancel.Pty.TooltipContent = Ressource.GetString("btnCancel");
            btnRemove.Pty.TooltipContent = Ressource.GetString("btnRemove");
            btnSeeMsg.Pty.TooltipContent = Ressource.GetString("btnSeeCAMsg");
        }
        #endregion SetOtherTooltip


        #region SetValidators
        /// <summary>
        /// Initialisation des validateurs dynamiques
        /// </summary>
        // EG 20231030 [WI725] New Closing/Reopening : EFFECTIVEENDDATE (action perpetuelle)
        private void SetValidators()
        {
            // Erreurs sur Caractéristiques générales
            validGen.ShowSummary = true;
            validGen.ValidationGroup = ValidatorTools.ValidationGroupEnum.MAIN.ToString();
            validGen.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
            ValidatorTools.SetValidators(txtIdentifier, ValidatorTools.ValidationGroupEnum.MAIN);
            ValidatorTools.SetValidators(txtDisplayName, ValidatorTools.ValidationGroupEnum.MAIN);
            ValidatorTools.SetValidators(txtDescription, EFSRegex.TypeRegex.None, false, EFSCssClass.CaptureMultilineOptional, ValidatorTools.ValidationGroupEnum.MAIN);
            ValidatorTools.SetValidators(txtEffectiveDate, EFSRegex.TypeRegex.RegexDate, true, "DtPicker " + txtEffectiveDate.CssClass, ValidatorTools.ValidationGroupEnum.MAIN);
            ValidatorTools.SetValidators(txtEffectiveEndDate, EFSRegex.TypeRegex.RegexDate, false, "DtPicker " + txtEffectiveEndDate.CssClass, ValidatorTools.ValidationGroupEnum.MAIN);
            // Erreurs sur Environnement instrumental
            validInstr.ShowSummary = true;
            validInstr.ValidationGroup = ValidatorTools.ValidationGroupEnum.PNL1.ToString();
            validInstr.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
            // Erreurs sur fermeture et ouverture de positions
            validClosingPositions.ShowSummary = true;
            validClosingPositions.ValidationGroup = ValidatorTools.ValidationGroupEnum.PNL2.ToString();
            validClosingPositions.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;

            validReopeningPositions.ShowSummary = true;
            validReopeningPositions.ValidationGroup = ValidatorTools.ValidationGroupEnum.PNL3.ToString();
            validReopeningPositions.DisplayMode = ValidationSummaryDisplayMode.SingleParagraph;
        }
        #endregion SetValidators
        #endregion Methods

        #region WebMethods
        [WebMethod]
        // EG 20200318 [24683] New (implementation autocomplete)
        // EG 20240520 [WI930] Add cssCustodian
        public static List<KeyValuePair<int, string>> LoadDataControl(string currentCtrlId, string typeAssociated, string identifier)
        {
            string dataCacheKey = string.Empty;
            if ("txtIDContract" == currentCtrlId)
                dataCacheKey = "CRP_IDContract";
            else if ("txtIDDealer_C" == currentCtrlId)
                dataCacheKey = "CRP_IDDealer";
            else if ("txtIDClearer_C" == currentCtrlId)
                dataCacheKey = "CRP_IDClearer";
            else if ("txtIDCssCustodian_C" == currentCtrlId)
                dataCacheKey = "CRP_IDCssCustodian";

            List<MapDataReaderRow> lstSource = DataCache.GetData<List<MapDataReaderRow>>(dataCacheKey);
            List<KeyValuePair<int, string>> lst = new List<KeyValuePair<int, string>>();
            lst = (lstSource.Where(item => (item["TYPEDATA"].Value.ToString() == typeAssociated) &&
            (item["IDENTIFIER"].Value.ToString().ToUpper().Contains(identifier.ToUpper())))).
            Select(item => new KeyValuePair<int, string>(Convert.ToInt32(item["ID"].Value), item["IDENTIFIER"].Value.ToString())).ToList();
            return lst;
        }
        #endregion WebMethods
    }
    #endregion ClosingReopeningPosition

    // EG 20201104 [24683] Ajout OpenUrl.js
    // EG 20210222 [XXXXX] Suppression inscription function ClosePage
    // EG 20210222 [XXXXX] Suppression OpenUrl.js (fonctions déplacées dans Referential.js)
    // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
    // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
    public class ActionPage : PageBase
    {
        #region CreateChildControls
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
            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));
        }
        #endregion CreateChildControls
        #region OnLoad
        /// <summary>
        /// Chargement de la page
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        #endregion OnLoad

    }
}
