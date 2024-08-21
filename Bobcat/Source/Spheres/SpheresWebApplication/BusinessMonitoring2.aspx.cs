#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.Referential;
using EfsML.CorporateActions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EFS.Spheres
{
    #region BusinessMonitoring2Page
    public class EntityMarketMonitoring
    {
        public int IdEM { get; set; }
        public int IdA_Entity { set; get; }
        public int IdA_CSS { set; get; }
        public int IdM { set; get; }
        public DateTime DtMarketPrev { get; set; }
        public DateTime DtMarket { get; set; }
        public DateTime DtMarketNext { get; set; }
        public SpheresCommonIdentification css;
        public MarketIdentification market;

        public EntityMarketMonitoring() { }
    }

    #region MonitoringGrid
    public class MonitoringGrid : HierarchicalGrid
    {
        #region Accessors
        #endregion Accessors
        #region Constructors
        public MonitoringGrid(): base()
        {
        }
        #endregion Constructors
        #region Events
        #region OnItemDataBound
        override protected void OnItemDataBound(DataGridItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item ||
                e.Item.ItemType == ListItemType.AlternatingItem ||
                e.Item.ItemType == ListItemType.SelectedItem)
            {
                SetCssClassStatus(e.Item);
                SetRequestTypeValue(e.Item);
                SetCssClassForDtMarketPrev(e.Item);
            }
            base.OnItemDataBound(e);
        }
        #endregion OnItemDataBound
        #region OnDataBinding
        private void OnDataBinding(object sender, System.EventArgs e)
        {
        }
        #endregion OnDataBinding
        #endregion Events
        #region Methods
        #region SetCssClassStatus
        // EG 20180525 [23979] IRQ Processing
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        private void SetCssClassStatus(DataGridItem pDataGridItem)
        {
            string color = string.Empty;
            TableCell cell = this.GetDataGridItemCell(pDataGridItem, "STATUS", 1);
            if ((null != cell) && StrFunc.IsFilled(cell.Text))
            {
                switch (cell.Text.ToLower())
                {
                    case "success":
                        color = "green";
                        break;
                    case "info":
                        color = "blue";
                        break;
                    case "error":
                        color = "red";
                        break;
                    case "warning":
                        color = "orange";
                        break;
                }
                if (StrFunc.IsFilled(color))
                {
                    WCToolTipPanel imgRowState = new WCToolTipPanel
                    {
                        CssClass = String.Format("fa-icon fa fa-circle {0}", color)
                    };
                    pDataGridItem.Cells[0].Controls.Add(imgRowState);
                    cell.CssClass = "txt_status_" + cell.Text.ToLower();
                }
                else if (cell.Text.ToUpper().Equals(ProcessStateTools.StatusInterrupt))
                {
                    WCToolTipPanel imgRowState = new WCToolTipPanel();
                    imgRowState.Style.Add(HtmlTextWriterStyle.PaddingLeft, "2px");
                    imgRowState.CssClass = "fa-icon fa fa-skull black";
                    pDataGridItem.Cells[0].Controls.Add(imgRowState);
                    cell.CssClass = "txt_status_" + cell.Text.ToLower();
                }
            }
        }
        #endregion SetCssClassStatus

        #region SetRequestTypeValue
        private void SetRequestTypeValue(DataGridItem pDataGridItem)
        {
            TableCell cell = this.GetDataGridItemCell(pDataGridItem, "EXTENDVALUE_REQUESTTYPE", 1);
            if ((null != cell) && StrFunc.IsFilled(cell.Text))
            {
                cell.Text = Ressource.GetString(cell.Text, true);
            }
        }
        #endregion SetRequestTypeValue

        #region SetCssClassForDtMarketPrev
        private void SetCssClassForDtMarketPrev(DataGridItem pDataGridItem)
        {
            // PM 20150602 [20575] Utilisation de DTENTITY à la place de DTMARKET
            //TableCell cellDtMarket = this.GetDataGridItemCell(pDataGridItem, "DTMARKET", 1);
            TableCell cellDtMarket = this.GetDataGridItemCell(pDataGridItem, "DTENTITY", 1);
            TableCell cellDtBusiness = this.GetDataGridItemCell(pDataGridItem, "DTBUSINESS", 1);
            if ((null != cellDtMarket) && (null != cellDtBusiness))
            {
                if (cellDtMarket.Text != cellDtBusiness.Text)
                    pDataGridItem.CssClass = "dtMarketPrev";
            }
        }
        #endregion SetCssClassForDtMarketPrev
        #endregion Methods
    }
    #endregion MonitoringGrid

    public partial class BusinessMonitoring2Page : PageBase
    {
        #region Members
        public List<SpheresCommonIdentification> m_LstEntity;
        public Dictionary<int, List<Pair<RoleActor,SpheresCommonIdentification>>> m_LstCssCustodian;
        public Dictionary<int, List<MarketIdentification>> m_LstMarket;
        public List<Pair<string, string>> m_OptGroup;
        protected string xmlResult;
        private Int64 m_MonitoringElement;
        protected ReferentialsReferential result;
        #endregion Members
        #region Accessors
        #region IsMonitoringCahsBalance
        private bool IsMonitoringCahsBalance { get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.CASHBALANCE); } }
        #endregion IsMonitoringCahsBalance
        #region IsMonitoringClosingDay
        private bool IsMonitoringClosingDay { get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.ClosingDay); } }
        #endregion IsMonitoringClosingDay
        #region IsMonitoringEndOfDay
        private bool IsMonitoringEndOfDay { get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.EndOfDay); } }
        #endregion IsMonitoringEndOfDay
        #region IsMonitoringInput
        private bool IsMonitoringInput { get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.INPUT); }}
        #endregion IsMonitoringInput
        #region IsMonitoringOutput
        private bool IsMonitoringOutput{ get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.OUTPUT); }}
        #endregion IsMonitoringOutput
        #region IsMonitoringQuote
        private bool IsMonitoringQuote { get { return IsMonitoringElement(MonitoringTools.MonitoringElementEnum.EDSP); } }
        #endregion IsMonitoringQuote
        #endregion Accessors
        #region Constructor
        public BusinessMonitoring2Page()
        {
        }
        #endregion Constructor

        #region Events

        #region CreateChildControls
        // EG 20210222 Suppression inscription script ClosePage
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));
            //JavaScript.ClosePage(this);
            base.CreateChildControls();

            //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));

            HtmlInputHidden hdn = new HtmlInputHidden();
            hdn.Attributes.Add("onclick", "javascript:" + ClientScript.GetPostBackEventReference(this, null));
            this.Page.Controls.Add(hdn);

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
        // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);

            Form = frmBusinessMonitoring;
            AntiForgeryControl();

            pnlBMResults.Visible = false;
            int time = SessionTools.MonitoringRefreshInterval * 1000;
            timerRefresh.Enabled = (time > 0);
            if (timerRefresh.Enabled)
                timerRefresh.Interval = time;
            SetAttributesTimerRefresh();
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
        // EG 20180525 [23979] IRQ Processing
        // EG 20190125 DOCTYPE Conformity HTML5
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        protected void Page_Load(object sender, EventArgs e)
        {
            // Gestion du focus suite à postback
            HookOnFocus(this.Page as Control);
            Page.ClientScript.RegisterStartupScript(typeof(BusinessMonitoring2Page), "ScriptDoFocus", SCRIPT_DOFOCUS.Replace("REQUEST_LASTFOCUS", Request["__LASTFOCUS"]), true);

            #region Header
            string idMenu = Request.QueryString["IDMENU"].ToString();
            string leftTitle = Ressource.GetString(idMenu, true);
            this.PageTitle = leftTitle;
            PageTools.SetHead(this, leftTitle, null, null);

            // EG 20130725 Timeout sur Block
            JQuery.Block block = new JQuery.Block(idMenu, "Msg_WaitingRefresh", true)
            {
                Timeout = SystemSettings.GetTimeoutJQueryBlock("BM")
            };
            JQuery.UI.WriteInitialisationScripts(this, block);

            Control head;
            if (null != this.Header)
                head = (Control)this.Header;
            else
                head = (Control)PageTools.SearchHeadControl(this);

            PageTools.SetHeaderLink(head, "linkCssAwesome", "~/Includes/fontawesome-all.min.css");
            PageTools.SetHeaderLink(head, "linkCssCommon", "~/Includes/EFSThemeCommon.min.css");
            PageTools.SetHeaderLink(head, "linkCssUISprites", "~/Includes/EFSUISprites.min.css");
            PageTools.SetHeaderLink(head, "linkCssMonitoring", "~/Includes/Monitoring.min.css");

            HtmlPageTitle titleLeft = new HtmlPageTitle(leftTitle);
            Panel pnlHeader = new Panel
            {
                ID = "divHeader"
            };
            pnlHeader.Controls.Add(ControlsTools.GetBannerPage(this, titleLeft, null, IdMenu.GetIdMenu(IdMenu.Menu.Admin)));
            plhHeader.Controls.Add(pnlHeader);

            SetRessource();

            m_MonitoringElement = SessionTools.MonitoringObserverElement;
            pnlBMIO.Visible = IsMonitoringInput || IsMonitoringOutput;
            pnlBMQuote.Visible = IsMonitoringQuote;
            pnlBMEOD.Visible = IsMonitoringEndOfDay || IsMonitoringClosingDay;
            pnlBMCashBalance.Visible = IsMonitoringCahsBalance;

            if (false == IsPostBack)
                InitializeData();
            else
            {
                string _value = ddlBMCssCustodian.SelectedValue;
                LoadDDLCssCustodian();
                ControlsTools.DDLSelectByValue(ddlBMCssCustodian, _value);
            }
            divbody.Attributes.Add("class", CSSMode);
            DisplayEntityMarket();

            #endregion Header
        }
        #endregion Page_Load

        //* --------------------------------------------------------------- *//
        // Changement de sélection sur les Dropdowns
        //* --------------------------------------------------------------- *//
        #region OnAction
        // EG 20150610 [19606][19607]
        protected void OnAction(object sender, CommandEventArgs e)
        {
            string eventTarget = Request.Params["__EVENTTARGET"];
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            bool isValid = false;
            if (StrFunc.IsFilled(eventTarget))
            {
                string message = string.Empty;
                if (StrFunc.IsEmpty(eventArgument))
                {
                    switch (eventTarget)
                    {
                        case "btntbrefresh":
                        case "timerRefresh":
                            isValid = true;
                            string entity = ddlBMEntity.SelectedItem.Text.Replace("<", string.Empty).Replace(">", string.Empty);
                            // EG 20150610 [19606][19607]
                            string cssCustodian = (null != ddlBMCssCustodian.SelectedItem) ?
                                ddlBMCssCustodian.SelectedItem.Text.Replace("<", string.Empty).Replace(">", string.Empty) : string.Empty;
                            if (0 < ddlBMEntity.SelectedIndex)
                                entity = LogTools.IdentifierAndId(entity, ddlBMEntity.SelectedValue);
                            if (0 < ddlBMCssCustodian.SelectedIndex)
                                cssCustodian = LogTools.IdentifierAndId(cssCustodian, ddlBMCssCustodian.SelectedValue);
                            message = Ressource.GetString2("Msg_BMRefresh", entity, cssCustodian) + Cst.CrLf;
                            break;
                        case "btntbautorefresh":
                            timerRefresh.Enabled = !timerRefresh.Enabled;
                            SetAttributesTimerRefresh();
                            break;
                    }
                    if (isValid && StrFunc.IsFilled(message))
                        JavaScript.ConfirmStartUpImmediate(this, message, eventTarget, "TRUE", "FALSE");
                }
                else if (eventArgument == "TRUE")
                {
                    DisplayResults();
                }
            }
        }
        #endregion OnAction

        #region OnEntityChanged
        protected void OnEntityChanged(object sender, EventArgs e)
        {
            LoadDDLCssCustodian();
        }
        #endregion OnEntityChanged
        #region OnCheckOverFlowChanged
        protected void OnCheckOverFlowChanged(object sender, EventArgs e)
        {
            CheckBox obj = sender as CheckBox;
            if ("chkovf_EnLargeAll" == obj.ID)
            {
                chkovf_EntityMarket.Checked = obj.Checked;
                chkovf_IO.Checked = obj.Checked;
                chkovf_Quote.Checked = obj.Checked;
                chkovf_EOD.Checked = obj.Checked;
                chkovf_CashBalance.Checked = obj.Checked;
                DisplayEntityMarket();
                if (pnlBMResults.Visible)
                    DisplayResults();
            }
            else
            {
                if (FindControl("pnlBM" + obj.ID.Replace("chkovf_", string.Empty)) is Panel pnl)
                {
                    if ("pnlBMEntityMarket" == pnl.ID)
                    {
                        DisplayEntityMarket();
                        if (pnlBMResults.Visible)
                            DisplayResults();
                    }
                    else
                        DisplayResults();
                }
                chkovf_EnLargeAll.Checked = chkovf_EntityMarket.Checked && chkovf_IO.Checked && 
                                            chkovf_Quote.Checked && chkovf_EOD.Checked && chkovf_CashBalance.Checked;
            }
        }
        #endregion OnCheckOverFlowChanged

        #region OnRefresh
        /// <summary>
        /// Evénement déclenché pour un raffraichissement de la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnRefresh(object sender, EventArgs e)
        {
            DisplayResults();
        }
        #endregion OnRefresh

        #endregion Events

        #region Methods
        //* --------------------------------------------------------------- *//
        // Actions sur les boutons de la barre d'outils
        //* --------------------------------------------------------------- *//
        #region InitializeData
        /// <summary>
        /// Initialisation Générale des données
        /// </summary>
        private void InitializeData()
        {
            ddlBMEntity.Items.Clear();
            ddlBMCssCustodian.CssClass = EFSCssClass.DropDownListJQOptionGroup;
            ddlBMCssCustodian.Items.Clear();
            LoadEntityMarket();
            OnEntityChanged(ddlBMEntity, null);
        }
        #endregion InitializeData
        #region IsMonitoringElement
        private bool IsMonitoringElement(MonitoringTools.MonitoringElementEnum pMonitoringElement)
        {
            int i = int.Parse(Enum.Format(typeof(MonitoringTools.MonitoringElementEnum), pMonitoringElement, "d"));
            return (0 < (m_MonitoringElement & Convert.ToInt64(Math.Pow(2, i))));
        }
        #endregion IsMonitoringElement

        #region LoadEntityMarket
        // EG 20150610 [19606][19607]
        private void LoadEntityMarket()
        {
            string CS = SessionTools.CS;
            int _idEM = 0;
            int _idA_Entity = 0;
            int _idA_CssCustodian = 0;
            int _idM = 0;

            string table = Cst.OTCml_TBL.VW_MARKET_IDENTIFIER.ToString();
            //PM 20150521 [20575] Gestion DTENTITY
            string SQLQuery = @"select em.IDEM, em.DTENTITYPREV, em.DTENTITY, em.DTENTITYNEXT, 
            ety.IDA as IDA_ENTITY, ety.IDENTIFIER as ETY_IDENTIFIER, ety.DISPLAYNAME  as ETY_DISPLAYNAME, ety.DESCRIPTION as ETY_DESCRIPTION, 
            mk.IDM, mk.IDENTIFIER as MK_IDENTIFIER, mk.DISPLAYNAME as MK_DISPLAYNAME, mk.DESCRIPTION as MK_DESCRIPTION, 
            mk.ISO10383_ALPHA4, mk.EXCHANGESYMBOL, mk.SHORTIDENTIFIER, mk.IDBC, mk.SHORT_ACRONYM, 
            css.IDA as IDA_CSSCUSTODIAN, css.IDENTIFIER as CSSCUSTODIAN_IDENTIFIER, css.DISPLAYNAME as CSSCUSTODIAN_DISPLAYNAME, css.DESCRIPTION as CSSCUSTODIAN_DESCRIPTION,
            case when em.IDA_CUSTODIAN is null then 0 else 1 end as ISCUSTODIAN, 
            case when em.IDA_CUSTODIAN is null then 1 else 0 end as ISCSS
            from dbo.ENTITYMARKET em 
            inner join dbo.ACTOR ety on (ety.IDA = em.IDA) 
            inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = em.IDM)
            inner join dbo.ACTOR css on (css.IDA = isnull(em.IDA_CUSTODIAN, mk.IDA))
            order by ETY_IDENTIFIER, ISCUSTODIAN, CSSCUSTODIAN_IDENTIFIER" + Cst.CrLf;

            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, SQLQuery);
            if (null != ds)
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    _idEM = Convert.ToInt32(row["IDEM"]);
                    _idA_Entity = Convert.ToInt32(row["IDA_ENTITY"]);
                    _idA_CssCustodian = Convert.ToInt32(row["IDA_CSSCUSTODIAN"]);
                    _idM = Convert.ToInt32(row["IDM"]);

                    // Chargement des Entités
                    if (null == m_LstEntity)
                        m_LstEntity = new List<SpheresCommonIdentification>();
                    if (false == m_LstEntity.Exists(key=>key.SpheresId ==_idA_Entity))
                    {
                        SpheresCommonIdentification _entity = new SpheresCommonIdentification
                        {
                            SpheresId = _idA_Entity,
                            identifierSpecified = (false == Convert.IsDBNull(row["ETY_IDENTIFIER"])),
                            identifier = row["ETY_IDENTIFIER"].ToString(),
                            displaynameSpecified = (false == Convert.IsDBNull(row["ETY_DISPLAYNAME"])),
                            displayname = row["ETY_DISPLAYNAME"].ToString(),
                            descriptionSpecified = (false == Convert.IsDBNull(row["ETY_DESCRIPTION"])),
                            description = row["ETY_DESCRIPTION"].ToString()
                        };
                        m_LstEntity.Add(_entity);
                    }
                    // Chargement des Chambres
                    RoleActor _roleActor = Convert.ToBoolean(row["ISCUSTODIAN"]) ? RoleActor.CUSTODIAN : RoleActor.CSS;

                    SpheresCommonIdentification _css = new SpheresCommonIdentification
                    {
                        SpheresId = _idA_CssCustodian,
                        identifierSpecified = (false == Convert.IsDBNull(row["CSSCUSTODIAN_IDENTIFIER"])),
                        identifier = row["CSSCUSTODIAN_IDENTIFIER"].ToString(),
                        displaynameSpecified = (false == Convert.IsDBNull(row["CSSCUSTODIAN_DISPLAYNAME"])),
                        displayname = row["CSSCUSTODIAN_DISPLAYNAME"].ToString(),
                        descriptionSpecified = (false == Convert.IsDBNull(row["CSSCUSTODIAN_DESCRIPTION"])),
                        description = row["CSSCUSTODIAN_DESCRIPTION"].ToString()
                    };
                    if (null == m_LstCssCustodian)
                        m_LstCssCustodian = new Dictionary<int, List<Pair<RoleActor, SpheresCommonIdentification>>>();

                    Pair<RoleActor, SpheresCommonIdentification> _pair = new Pair<RoleActor, SpheresCommonIdentification>(_roleActor, _css);

                    if (false == m_LstCssCustodian.ContainsKey(-1))
                    {
                        List<Pair<RoleActor, SpheresCommonIdentification>> _lst = new List<Pair<RoleActor, SpheresCommonIdentification>>
                        {
                            _pair
                        };
                        m_LstCssCustodian.Add(-1, _lst);

                    }
                    else if (false == m_LstCssCustodian[-1].Exists(css => css.Second.SpheresId == _idA_CssCustodian))
                    {
                        m_LstCssCustodian[-1].Add(_pair);
                    }

                    if (false == m_LstCssCustodian.ContainsKey(_idA_Entity))
                    {
                        List<Pair<RoleActor, SpheresCommonIdentification>> _lst = new List<Pair<RoleActor, SpheresCommonIdentification>>
                        {
                            _pair
                        };
                        m_LstCssCustodian.Add(_idA_Entity, _lst);

                    }
                    else if (false == m_LstCssCustodian[_idA_Entity].Exists(css => css.Second.SpheresId == _idA_CssCustodian))
                    {
                        m_LstCssCustodian[_idA_Entity].Add(_pair);
                    }


                    // Chargement des Marchés
                    MarketIdentification _market = new MarketIdentification
                    {
                        SpheresId = _idM,
                        identifierSpecified = (false == Convert.IsDBNull(row["MK_IDENTIFIER"])),
                        identifier = row["MK_IDENTIFIER"].ToString(),
                        displaynameSpecified = (false == Convert.IsDBNull(row["MK_DISPLAYNAME"])),
                        displayname = row["MK_DISPLAYNAME"].ToString(),
                        descriptionSpecified = (false == Convert.IsDBNull(row["MK_DESCRIPTION"])),
                        description = row["MK_DESCRIPTION"].ToString(),
                        exchangeSymbolSpecified = (false == Convert.IsDBNull(row["EXCHANGESYMBOL"])),
                        ISO10383_ALPHA4Specified = (false == Convert.IsDBNull(row["ISO10383_ALPHA4"])),
                        shortIdentifierSpecified = (false == Convert.IsDBNull(row["SHORTIDENTIFIER"])),
                        shortAcronymSpecified = (false == Convert.IsDBNull(row["SHORT_ACRONYM"])),
                        idBCSpecified = (false == Convert.IsDBNull(row["IDBC"]))
                    };

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

                    if (null == m_LstMarket)
                        m_LstMarket = new Dictionary<int, List<MarketIdentification>>();
                    if (false == m_LstMarket.ContainsKey(_idA_CssCustodian))
                    {
                        List<MarketIdentification> _lst = new List<MarketIdentification>
                        {
                            _market
                        };
                        m_LstMarket.Add(_idA_CssCustodian, _lst);
                    }
                    else
                    {
                        m_LstMarket[_idA_CssCustodian].Add(_market);
                    }
                }

                // EG 20150610 [19606][19607]
                if (null == m_LstEntity)
                {
                    SQLQuery = @"select ety.IDA as IDA_ENTITY, ac.IDENTIFIER as ETY_IDENTIFIER, ac.DISPLAYNAME as ETY_DISPLAYNAME, ac.DESCRIPTION as ETY_DESCRIPTION
                    from dbo.ENTITY ety
                    inner join dbo.ACTOR ac on (ac.IDA = ety.IDA) 
                    order by ac.IDENTIFIER" + Cst.CrLf;
                    ds = DataHelper.ExecuteDataset(CS, CommandType.Text, SQLQuery);
                    if (null != ds)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            _idA_Entity = Convert.ToInt32(row["IDA_ENTITY"]);

                            // Chargement des Entités
                            if (null == m_LstEntity)
                                m_LstEntity = new List<SpheresCommonIdentification>();
                            if (false == m_LstEntity.Exists(key => key.SpheresId == _idA_Entity))
                            {
                                SpheresCommonIdentification _entity = new SpheresCommonIdentification
                                {
                                    SpheresId = _idA_Entity,
                                    identifierSpecified = (false == Convert.IsDBNull(row["ETY_IDENTIFIER"])),
                                    identifier = row["ETY_IDENTIFIER"].ToString(),
                                    displaynameSpecified = (false == Convert.IsDBNull(row["ETY_DISPLAYNAME"])),
                                    displayname = row["ETY_DISPLAYNAME"].ToString(),
                                    descriptionSpecified = (false == Convert.IsDBNull(row["ETY_DESCRIPTION"])),
                                    description = row["ETY_DESCRIPTION"].ToString()
                                };
                                m_LstEntity.Add(_entity);
                            }
                        }
                    }
                }
                // EG 20150610 [19606][19607]
                if (null != m_LstEntity)
                {
                    m_LstEntity.ForEach(entity => ddlBMEntity.Items.Add(new ListItem(entity.identifier, entity.spheresid)));
                    ddlBMEntity.Items.Insert(0, new ListItem(Ressource.GetString("ActorEntity_ALL"), "-1"));
                }
            }
        }
        #endregion LoadEntityMarket


        #region LoadDDLCssCustodian
        // EG 20150610 [19606][19607]
        protected void LoadDDLCssCustodian()
        {
            if ((-1 < ddlBMEntity.SelectedIndex) && (null != m_LstCssCustodian))
            {
                int _idA = Convert.ToInt32(ddlBMEntity.SelectedValue);
                ddlBMCssCustodian.Items.Clear();

                m_OptGroup = new List<Pair<string, string>>();
                ListItem _li = null;
                //  Toutes activités : Toutes chambres / Tous custodians
                if (m_LstCssCustodian[_idA].Exists(csscustodian => csscustodian.First == RoleActor.CSS || csscustodian.First == RoleActor.CUSTODIAN))
                {
                    _li = new ListItem(Ressource.GetString("ActorCssCustodian_ALL"), "-1");
                    _li.Attributes.Add("optiongroup", "CSSCUSTODIAN");
                    m_OptGroup.Add(new Pair<string, string>("CSSCUSTODIAN", Ressource.GetString("ActorCssCustodian_ALL_Grp")));
                    ddlBMCssCustodian.Items.Add(_li);
                }

                string _actorText = string.Empty;
                m_LstCssCustodian[_idA].ForEach(csscustodian =>
                {
                    if (false == m_OptGroup.Exists(key => key.First == csscustodian.First.ToString()))
                    {
                        _actorText = "Actor" + StrFunc.FirstUpperCase(csscustodian.First.ToString().ToLower()) + "_ALL";
                        _li = new ListItem(Ressource.GetString(_actorText), (csscustodian.First == RoleActor.CSS?"-2":"-3"));
                        _li.Attributes.Add("optiongroup", csscustodian.First.ToString());
                        m_OptGroup.Add(new Pair<string, string>(csscustodian.First.ToString(), Ressource.GetString(_actorText + "_Grp")));
                        ddlBMCssCustodian.Items.Add(_li);
                    }

                    string identifier = csscustodian.Second.identifier;
                    if (csscustodian.Second.displaynameSpecified)
                        identifier += " - " + csscustodian.Second.displayname;

                    _li = new ListItem(identifier, csscustodian.Second.spheresid);
                    _li.Attributes.Add("optiongroup", csscustodian.First.ToString());

                    ddlBMCssCustodian.Items.Add(_li);
                }
                );
            }
            if (null != m_OptGroup)
                JQuery.WriteSelectOptGroupScripts(this, ddlBMCssCustodian.ClientID, m_OptGroup, false);
        }
        #endregion LoadDDLCssCustodian

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
            
            m_LstEntity = (List<SpheresCommonIdentification>)viewState[1];
            m_LstCssCustodian = (Dictionary<int, List<Pair<RoleActor, SpheresCommonIdentification>>>)viewState[2];
            m_LstMarket = (Dictionary<int, List<MarketIdentification>>)viewState[3];
            m_OptGroup = (List<Pair<string, string>>)viewState[4];
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

            object[] ret = new object[5];
            ret[0] = base.SaveViewState();
            ret[1] = m_LstEntity;
            ret[2] = m_LstCssCustodian;
            ret[3] = m_LstMarket;
            ret[4] = m_OptGroup;
            return ret;
        }
        #endregion SaveViewState

        #region SetAttributesTimerRefresh
        /// <summary>
        /// Changement de l'image de raffraichissement automatique du monitoring en fonction de son statut
        /// </summary>
        protected void SetAttributesTimerRefresh()
        {
            if (timerRefresh.Enabled)
            {
                btntbautorefresh.CssClass = "start";
                btntbautorefresh.Pty.TooltipContent = Ressource.GetString("StopRefresh");
            }
            else
            {
                btntbautorefresh.CssClass = "stop";
                btntbautorefresh.Pty.TooltipContent = Ressource.GetString("StartRefresh");
            }
        }
        #endregion SetAttributesTimerRefresh
        #region SetRessource
        private void SetRessource()
        {
            lblBMEntity.Text = Ressource.GetMulti("Entity_Title");
            lblBMEntityMarket.Text = Ressource.GetMulti("ENTITYMARKET_REF");
            lblBMCssCustodian.Text = Ressource.GetMulti("CssCustodian");
            lblBMIO.Text = Ressource.GetString("INPUT") + " / " + Ressource.GetString("OUTPUT");
            lblBMQuote.Text = Ressource.GetString("EDSP");
            lblBMEOD.Text = Ressource.GetString("GroupTrackerCLO");
            lblBMCashBalance.Text = Ressource.GetString("CASHBALANCE");
            btntbrefresh.Pty.TooltipContent = Ressource.GetString("Refresh");
            btntbparam.Pty.TooltipContent = Ressource.GetString("Tools");
            lblLastUpdate.Text = string.Empty;
            chkovf_EnLargeAll.Text = Ressource.GetString("EnlargeAll");
            string enLarge = Ressource.GetString("Enlarge");
            chkovf_EntityMarket.Text = enLarge;
            chkovf_IO.Text = enLarge;
            chkovf_Quote.Text = enLarge;
            chkovf_EOD.Text = enLarge;
            chkovf_CashBalance.Text = enLarge;
        }
        #endregion SetRessource
        #region DisplayResults
        private void DisplayResults()
        {
            pnlBMResults.Visible = (0 < m_MonitoringElement);
            if (IsMonitoringElement(MonitoringTools.MonitoringElementEnum.INPUT) || 
                IsMonitoringElement(MonitoringTools.MonitoringElementEnum.OUTPUT))
                DisplayIO();

            if (IsMonitoringElement(MonitoringTools.MonitoringElementEnum.EDSP))
                DisplayQuote();

            if (IsMonitoringElement(MonitoringTools.MonitoringElementEnum.EndOfDay) || IsMonitoringElement(MonitoringTools.MonitoringElementEnum.ClosingDay))
                DisplayEndOfDay();

            if (IsMonitoringElement(MonitoringTools.MonitoringElementEnum.CASHBALANCE))
                DisplayCashBalance();

            lblLastUpdate.Text = DtFunc.DateTimeToString(DateTime.Now, DtFunc.FmtDateLongTime);
        }
        #endregion DisplayResults
        #region DisplayEntityMarket()
        // EG 20150610 [19606][19607]
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayEntityMarket()
        {
            pnlBMEntityMarket.CssClass = "admin " + (chkovf_EntityMarket.Checked ? "withoutovf" : "withovf");
            plhBMEntityMarket.Controls.Clear();
            InitializeReferential(Cst.ListType.Repository, "ENTITYMARKET");
            result.SQLWhereSpecified = true;
            result.SQLWhere = new ReferentialsReferentialSQLWhere[1] { new ReferentialsReferentialSQLWhere() };
            result.SQLWhere[0].SQLWhereSpecified = true;
            result.SQLWhere[0].SQLWhere = @"(IDA_ENTITY = case when -1= @ENTITY then IDA_ENTITY else @ENTITY end) and 
          (IDA_CSSCUSTODIAN=case when (@CSSCUSTODIAN<0) then IDA_CSSCUSTODIAN else @CSSCUSTODIAN end) and 
          (ISCUSTODIAN=case when (@CSSCUSTODIAN=-2) then 0 else ISCUSTODIAN end) and 
          (ISCSS=case when (@CSSCUSTODIAN=-3) then 0 else ISCSS end)";

            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ENTITY", DbType.Int32), 0 < ddlBMEntity.Items.Count ? Convert.ToInt32(ddlBMEntity.SelectedValue):-1);
            parameters.Add(new DataParameter(SessionTools.CS, "CSSCUSTODIAN", DbType.Int32), 0 < ddlBMCssCustodian.Items.Count ? Convert.ToInt32(ddlBMCssCustodian.SelectedValue) : -1);
            plhBMEntityMarket.Controls.Add(SetDataGrid("dgBMEntityMarket", result, xmlResult, parameters));
            // EG 20150610 [19606][19607] Pas d'activité via ENTITYMARKET
            if (null == m_LstCssCustodian)
                lblBMEntityMarket.Text = Ressource.GetMulti("ENTITYMARKET_REF") + " - " + Ressource.GetMulti("NoActivity_Title").Replace(Cst.HTMLBreakLine, string.Empty);
        }
        #endregion DisplayEntityMarket()
        #region DisplayIO
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayIO()
        {
            string CS = SessionTools.CS;
            pnlBMIO.CssClass = "views " + (chkovf_IO.Checked ? "withoutovf" : "withovf");
            plhBMIO.Controls.Clear();
            InitializeReferential("MONITORING_IO");
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Cst.GroupTrackerEnum.IO);
            parameters.Add(new DataParameter(CS, "INPUT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), IsMonitoringInput ? 
                ReflectionTools.ConvertEnumToString<MonitoringTools.MonitoringElementEnum>(MonitoringTools.MonitoringElementEnum.INPUT) : MonitoringTools.MonitoringElementEnum.NA.ToString());
            parameters.Add(new DataParameter(CS, "OUTPUT", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), IsMonitoringOutput ?
                ReflectionTools.ConvertEnumToString<MonitoringTools.MonitoringElementEnum>(MonitoringTools.MonitoringElementEnum.OUTPUT) : MonitoringTools.MonitoringElementEnum.NA.ToString());
            plhBMIO.Controls.Add(SetDataGrid("dgBMIO", result, xmlResult, parameters));
        }
        #endregion DisplayIO
        #region DisplayCashBalance
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayCashBalance()
        {
            pnlBMCashBalance.CssClass = "process "+ (chkovf_CashBalance.Checked ? "withoutovf" : "withovf");
            plhBMCashBalance.Controls.Clear();
            InitializeReferential("MONITORING_CASHBALANCE");
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ENTITY", DbType.Int32), Convert.ToInt32(ddlBMEntity.SelectedValue));
            plhBMCashBalance.Controls.Add(SetDataGrid("dgBMCashBalance", result, xmlResult, parameters));
        }
        #endregion DisplayCashBalance
        #region DisplayQuote
        // EG 20150610 [19606][19607]
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayQuote()
        {
            pnlBMQuote.CssClass = "about " + (chkovf_Quote.Checked ? "withoutovf" : "withovf");
            plhBMQuote.Controls.Clear();
            InitializeReferential("MONITORING_QUOTE");
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ENTITY", DbType.Int32), Convert.ToInt32(ddlBMEntity.SelectedValue));
            parameters.Add(new DataParameter(SessionTools.CS, "CSSCUSTODIAN", DbType.Int32), 0 < ddlBMCssCustodian.Items.Count ? Convert.ToInt32(ddlBMCssCustodian.SelectedValue) : -1);
            plhBMQuote.Controls.Add(SetDataGrid("dgBMQuote", result, xmlResult, parameters));
        }
        #endregion DisplayQuote
        #region DisplayEndOfDay
        // EG 20150610 [19606][19607]
        // EG 20200831 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        private void DisplayEndOfDay()
        {
            string CS = SessionTools.CS;
            pnlBMEOD.CssClass = "process " + (chkovf_EOD.Checked ? "withoutovf" : "withovf");
            plhBMEOD.Controls.Clear();
            InitializeReferential("MONITORING_EOD");
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(CS, "ENTITY", DbType.Int32), Convert.ToInt32(ddlBMEntity.SelectedValue));
            parameters.Add(new DataParameter(CS, "CSSCUSTODIAN", DbType.Int32), 0 < ddlBMCssCustodian.Items.Count ? Convert.ToInt32(ddlBMCssCustodian.SelectedValue) : -1);
            parameters.Add(new DataParameter(CS, "EOD", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), IsMonitoringEndOfDay ? 
                ReflectionTools.ConvertEnumToString<MonitoringTools.MonitoringElementEnum>(MonitoringTools.MonitoringElementEnum.EndOfDay) : MonitoringTools.MonitoringElementEnum.NA.ToString());
            parameters.Add(new DataParameter(CS, "CLOSINGDAY", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), IsMonitoringClosingDay ? 
                ReflectionTools.ConvertEnumToString<MonitoringTools.MonitoringElementEnum>(MonitoringTools.MonitoringElementEnum.ClosingDay) : MonitoringTools.MonitoringElementEnum.NA.ToString());
            plhBMEOD.Controls.Add(SetDataGrid("dgBMEOD", result, xmlResult, parameters));
        }
        #endregion DisplayEndOfDay
        #region InitializeReferential
        private void InitializeReferential(string pFileName)
        {
            InitializeReferential(Cst.ListType.Monitoring, pFileName);
        }
        private void InitializeReferential(Cst.ListType pType, string pFileName)
        {
            xmlResult = ReferentialTools.GetObjectXMLFile(pType, pFileName);

            ReferentialTools.DeserializeXML_ForModeRO(xmlResult, out ReferentialsReferential referential);
            ReferentialTools.InitializeReferentialForGrid(referential);
            // FI 20200220 [XXXXX] Appel à ReferentialTools.CreateControls
            ReferentialTools.CreateControls((PageBase)this.Page, referential, "grid", false, false);
            result = referential;
        }
        #endregion InitializeReferential
        #region SetDataGrid
        // EG 20201014 [XXXXX] Correction sqlSelect evaluation
        private MonitoringGrid SetDataGrid(string pID, ReferentialsReferential pReferential, string pXMLFile, DataParameters pParameters)
        {
            string CS = SessionTools.CS;
            SQLReferentialData.SQLSelectParameters sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(CS, pReferential);
            string sqlSelect = SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query + Cst.CrLf;
            string sqlOrderBy = SQLReferentialData.GetSQLOrderBy(CS, pReferential);
            sqlSelect += Cst.CrLf + sqlOrderBy;
            QueryParameters qryParameters = new QueryParameters(CS, sqlSelect, pParameters);
            DataSet ds = DataHelper.ExecuteDataset(CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            MonitoringGrid grid = new MonitoringGrid();
            grid.LoadReferential(pXMLFile);
            //// Masquage des colonnes liées à ENTITY si une ENTITE est sélectionnée
            if (0 < ddlBMEntity.SelectedIndex)
            {
                HideInDataGrid(grid, "ENTITY_IDENTIFIER");
                HideInDataGrid(grid, "ENTITY_DISPLAYNAME");
            }
            if ((0 < ddlBMCssCustodian.SelectedIndex) &&
                (0 <= Convert.ToInt32(ddlBMCssCustodian.SelectedValue)))
            {
                HideInDataGrid(grid, "CSSCUSTODIAN_IDENTIFIER");
                HideInDataGrid(grid, "CSSCUSTODIAN_DISPLAYNAME");
            }
            grid.DataSource = ds.Tables[0];
            grid.ID = pID;
            grid.DataBind();
            return grid;
        }
        #endregion SetDataGrid
        #region HideInDataGrid
        private void HideInDataGrid(MonitoringGrid pGrid , string pColumnName)
        {
            int i = pGrid.GetBoundColumnIndex(pColumnName);
            if (-1 < i)
                pGrid.Columns[i].Visible = false;
        }
        #endregion HideInDataGrid
        #endregion Methods


    }
    #endregion BusinessMonitoringPage
}
