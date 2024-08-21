#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Audit;
using EFS.Common;
using EFS.Common.Web;
using EFS.Restriction;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Enum;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
#endregion Using Directives

namespace EFS.Spheres
{
    // EG 20200922 [XXXXX] Suppression DTTRANSAC et Ajout DTBUSINESS, DTORDERENTERED et DTEXECUTION dans Piste d'audit avec TimeZone spécifiée dans Profile)
    public partial class TracksPage : PageBase
    {
        #region Members

        protected static TradeTrack m_TradeTrack;
        protected string m_CurrentTradeXML;
        protected DataSet m_DsAuditTrack;
        protected const string DDLTRACKSELECTED_ID = "__TRACKSELECTED";
        protected const string DDLTRACKTOCOMPARE_ID = "__TRACKTOCOMPARE";
        private string m_ParentGUID;
        private TradeCommonInputGUI m_TradeCommonInputGUI;
        private TradeCommonInput m_TradeCommonInput;
        private AuditTimestampInfo auditTimestampInfo;
        private TradingTimestampInfo tradingTimestampInfo;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }

        /// <summary>
        /// 
        /// </summary>
        public TradeCommonInput TradeCommonInput
        {
            set { m_TradeCommonInput = value; }
            get { return m_TradeCommonInput; }
        }
        /// <summary>
        /// Obtient true si les variables session sont disponibles
        /// <remarks>Lorsque les variables session sont non disponibles cette page s'autoclose</remarks>
        /// </summary>
        protected bool IsSessionVariableAvailable
        {

            get { return (null != m_TradeCommonInput); }
        }

        #endregion Accessors
        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20220207 [XXXX] Incoherece sur Identifiant de trade Piste d'audit
        protected void OnCompareClick(object sender, EventArgs e)
        {
            if (Cst.ErrLevel.SUCCESS == ConstructTracks())
                CompareTradeTracks();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        // EG 20200922 [XXXXX] Suppression DTTRANSAC et Ajout DTBUSINESS, DTORDERENTERED et DTEXECUTION dans Piste d'audit avec TimeZone spécifiée dans Profile)
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            // FI 20200820 [XXXXXX] dates systèmes en UTC
            // Affichage selon le profile utilisateur 
            auditTimestampInfo = new AuditTimestampInfo()
            {
                TimestampZone = SessionTools.AuditTimestampZone,
                Collaborator = SessionTools.Collaborator,
                Precision = SessionTools.AuditTimestampPrecision
            };

            tradingTimestampInfo = new TradingTimestampInfo()
            {
                TimestampZone = SessionTools.TradingTimestampZone,
                Precision = SessionTools.TradingTimestampPrecision,
            };

            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //m_TradeCommonInputGUI = (TradeCommonInputGUI)Session[InputGUISessionID];
            //m_TradeCommonInput = (TradeCommonInput)Session[InputSessionID];

            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputSessionID);

            PageConstruction();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLoad(object sender, System.EventArgs e)
        {
            if (IsSessionVariableAvailable)
            {
                if (false == IsPostBack)
                {
                    m_TradeTrack = null;
                    ConstructTracks();
                }
                DisplayTracks();
            }
        }

        #region OnPreRender
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            XMLCompare.RegisterXMLDiffAnchorFunction(this);
        }
        #endregion OnPreRender
        #region OnRefreshClick
        protected void OnRefreshClick(object sender, EventArgs e)
        {
            if (Cst.ErrLevel.SUCCESS == ConstructTracks())
                DisplayTracks();
        }
        #endregion OnRefreshClick
        #region OnViewTradeClick
        // EG 20210324 [25562] Gestion de la piste d'audit post UPGV10 (absence TRADE_P, TRADEXML_P)
        // EG 20220207 [XXXX] Incoherece sur Identifiant de trade Piste d'audit
        protected void OnViewXMLTradeClick(object sender, EventArgs e)
        {
            if (Cst.ErrLevel.SUCCESS == ConstructTracks())
            {
                EFS.Audit.Track currentTrack = m_TradeTrack.track[GetIndexDDLTracks(DDLTRACKSELECTED_ID)];
                XmlDocument xmlDocument = new XmlDocument();
                if (String.IsNullOrEmpty(currentTrack.tradeXML))
                {
                    _ = AddXmlToPlaceHolder("phDiffXMLTracks", "Trade" + " [" + GetLabelTrack(DDLTRACKSELECTED_ID) + "]", Ressource.GetString("Msg_AuditTrackUnavailable"));
                }
                else
                {
                    xmlDocument.LoadXml(currentTrack.tradeXML);
                    StringWriter textWriter = new StringWriter();
                    XmlTextWriter xmlWriter = new XmlTextWriter(textWriter)
                    {
                        Formatting = Formatting.Indented
                    };
                    xmlDocument.Save(xmlWriter);
                    _ = AddXmlToPlaceHolder("phDiffXMLTracks", "Trade" + " [" + GetLabelTrack(DDLTRACKSELECTED_ID) + "]", textWriter.ToString());
                    xmlWriter.Close();
                }
            }
        }
        #endregion OnViewTradeClick
        #region OnXmlClick
        protected void OnXmlClick(object sender, EventArgs e)
        {
            foreach (EFS.Audit.Track track in m_TradeTrack.track)
                track.SetTradeXML();
            //
            EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(TradeTrack), m_TradeTrack)
            {
                IsXMLTrade = true,
                Source = Cst.Track_Name,
                NameSpace = Cst.Track_Namespace
            };
            StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
            DisplayXml("Tracks_XML", "Tracks_" + m_TradeCommonInput.Identifier, sb.ToString());

        }
        #endregion OnXmlClick
        #endregion Events
        #region Methods
        // EG 20190611 Upd (Bouton Font Awesome) Refresh, Print, InfoLegend
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            pnlToolBar.Controls.Add(AddPanelButton());
            pnlToolBar.Controls.Add(new Panel() { ID = "divtbaudit3" });

            CellForm.Controls.Add(pnlToolBar);
        }
        private Panel AddPanelButton()
        {
            Panel pnl = new Panel() { ID = "divtbaudit1" };
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Click += new EventHandler(OnRefreshClick);
            btnRefresh.OnClientClick = "javascript:RemoveAudit();return true;";
            pnl.Controls.Add(btnRefresh);

            WCToolTipLinkButton btnViewTrade = ControlsTools.GetAwesomeButtonAction("btnXMLTrade", "fa fa-search", true);
            btnViewTrade.Click += new EventHandler(this.OnViewXMLTradeClick);
            btnViewTrade.OnClientClick = "javascript:RemoveAudit();return true;";
            pnl.Controls.Add(btnViewTrade);

            WCToolTipLinkButton btnCompare = ControlsTools.GetAwesomeButtonAction("btnXMLCompare", "fa fa-search", true);
            btnCompare.Click += new EventHandler(this.OnCompareClick);
            btnCompare.OnClientClick = "javascript:RemoveAudit();return true;";
            pnl.Controls.Add(btnCompare);

            HtmlInputHidden hih = new HtmlInputHidden() { ID = DDLTRACKSELECTED_ID, Value = "0" };
            pnl.Controls.Add(hih);

            hih = new HtmlInputHidden() { ID = DDLTRACKTOCOMPARE_ID, Value = "0" };
            pnl.Controls.Add(hih);

            return pnl;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        private EFS.Audit.Confirm AddConfirm(DataRow pRow)
        {
            EFS.Audit.Confirm confirm = new EFS.Audit.Confirm();
            //
            // RD 20091228 [16809] Confirmation indicators for each party
            if (false == Convert.IsDBNull(pRow["ISNCMINI"]))
                confirm.initialSend = Convert.ToBoolean(pRow["ISNCMINI"]);
            if (false == Convert.IsDBNull(pRow["ISNCMINT"]))
                confirm.interimSend = Convert.ToBoolean(pRow["ISNCMINT"]);
            if (false == Convert.IsDBNull(pRow["ISNCMFIN"]))
                confirm.finalSend = Convert.ToBoolean(pRow["ISNCMFIN"]);
            //
            return confirm;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="palStatus"></param>
        /// <param name="pRow"></param>
        /// <param name="pStatusType"></param>
        private void AddStatus(ref ArrayList palStatus, DataRow pRow, StatusTypeEnum pStatusType)
        {
            DataRow[] childRows = pRow.GetChildRows(pRow.Table.TableName + "_St" + pStatusType.ToString());
            if (0 < childRows.Length)
            {
                string statusName = pStatusType.ToString().ToUpper();
                Audit.Status status;
                if ((StatusTypeEnum.Match == pStatusType) || (StatusTypeEnum.Check == pStatusType))
                {
                    foreach (DataRow rowStatus in childRows)
                    {
                        if (Convert.ToDateTime(rowStatus["DTST" + statusName]) <= Convert.ToDateTime(pRow["DTSYS"]))
                        {
                            status = new EFS.Audit.Status(pStatusType, rowStatus["IDST" + statusName].ToString(),
                                DtFuncExtended.DisplayTimestampUTC(DateTime.SpecifyKind(Convert.ToDateTime(rowStatus["DTST" + statusName]), DateTimeKind.Utc), auditTimestampInfo),
                                rowStatus["DISPLAYNAME"].ToString(), rowStatus["BACKCOLOR"].ToString(), rowStatus["FORECOLOR"].ToString());

                            palStatus.Add(status);
                        }
                    }
                }
                else
                {
                    status = new EFS.Audit.Status(pStatusType, pRow["IDST" + statusName].ToString(),
                        DtFuncExtended.DisplayTimestampUTC(DateTime.SpecifyKind(Convert.ToDateTime(pRow["DTST" + statusName]), DateTimeKind.Utc), auditTimestampInfo),
                        childRows[0]["DISPLAYNAME"].ToString(), childRows[0]["BACKCOLOR"].ToString(), childRows[0]["FORECOLOR"].ToString());
                    palStatus.Add(status);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRow"></param>
        /// <returns></returns>
        // EG 20171025 [23509] Add businessDate, dtExecution, dtOrderEntered, tzFacility
        // EG 20200922 [XXXXX] Suppression DTTRANSAC et Ajout DTBUSINESS, DTORDERENTERED et DTEXECUTION dans Piste d'audit avec TimeZone spécifiée dans Profile)
        // EG 20210324 [25562] Gestion de la piste d'audit post UPGV10 (absence TRADE_P, TRADEXML_P)
        private EFS.Audit.Track AddTrack(DataRow pRow)
        {
            EFS.Audit.Track track = new EFS.Audit.Track
            {
                action = pRow["ACTION"].ToString(),
                date = DtFuncExtended.DisplayTimestampUTC(DateTime.SpecifyKind(Convert.ToDateTime(pRow["DTSYS"]), DateTimeKind.Utc), auditTimestampInfo)
            };
            if (StrFunc.IsFilled(pRow["SCREENNAME"].ToString()))
                track.screenName = pRow["SCREENNAME"].ToString();
            if (StrFunc.IsFilled(pRow["HOSTNAME"].ToString()))
                track.hostName = pRow["HOSTNAME"].ToString();

            #region User
            track.user = new Identifier(pRow["IDA"].ToString(), pRow["IDA_IDENTIFIER"].ToString(), pRow["IDA_DISPLAYNAME"].ToString());
            #endregion User
            #region Application
            if (StrFunc.IsFilled(pRow["APPNAME"].ToString()))
                track.application = new Application(pRow["APPVERSION"].ToString(), pRow["APPBROWSER"].ToString(), pRow["APPNAME"].ToString());
            #endregion Application
            #region Data

            tradingTimestampInfo.DataRow = pRow;
            track.data = new Data();
            if (false == Convert.IsDBNull(pRow["DTTRADE"]))
                track.data.tradeDate = Convert.ToDateTime(pRow["DTTRADE"]).ToShortDateString();
            if (false == Convert.IsDBNull(pRow["DTBUSINESS"]))
                track.data.businessDate = Convert.ToDateTime(pRow["DTBUSINESS"]).ToShortDateString();
            if (false == Convert.IsDBNull(pRow["DTORDERENTERED"]))
            {
                DateTimeTz dtZonedSource = new DateTimeTz(Convert.ToDateTime(pRow["DTORDERENTERED"]), "Etc/UTC");
                track.data.orderEntered = DtFuncExtended.DisplayTimestampTrading(dtZonedSource, tradingTimestampInfo);
            }
            if (false == Convert.IsDBNull(pRow["DTEXECUTION"]))
            {
                DateTimeTz dtZonedSource = new DateTimeTz(Convert.ToDateTime(pRow["DTEXECUTION"]), "Etc/UTC");
                track.data.execution = DtFuncExtended.DisplayTimestampTrading(dtZonedSource, tradingTimestampInfo);
            }
            if (false == Convert.IsDBNull(pRow["TZFACILITY"]))
                track.data.tzFacility = pRow["TZFACILITY"].ToString();

            #region Status
            ArrayList alStatus = new ArrayList();
            AddStatus(ref alStatus, pRow, StatusTypeEnum.Match);
            AddStatus(ref alStatus, pRow, StatusTypeEnum.Check);
            AddStatus(ref alStatus, pRow, StatusTypeEnum.Environment);
            AddStatus(ref alStatus, pRow, StatusTypeEnum.Activation);
            AddStatus(ref alStatus, pRow, StatusTypeEnum.Priority);
            track.data.status = (EFS.Audit.Status[])alStatus.ToArray(typeof(EFS.Audit.Status));
            #endregion Status
            #region Confirm
            track.data.confirm = AddConfirm(pRow);
            #endregion Confirm
            #endregion Data
            #region TradeXML
            if (false == Convert.IsDBNull(pRow["TRADEXML"]))
                track.tradeXML = pRow["TRADEXML"].ToString();
            #endregion TradeXML
            return track;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPlaceHolderName"></param>
        /// <param name="pTitle"></param>
        /// <param name="pXmlFragment"></param>
        /// <returns></returns>
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        private TextBox AddXmlToPlaceHolder(string pPlaceHolderName, string pTitle, string pXmlFragment)
        {
            Control ctrl = FindControl(pPlaceHolderName);
            TextBox txt = new TextBox
            {
                Text = pXmlFragment,
                TextMode = TextBoxMode.MultiLine,
                Rows = 9,
                ReadOnly = true,
                EnableViewState = false
            };
            if (null != ctrl)
            {
                ctrl.Controls.Clear();

                Panel pnl = new Panel();
                Label lbl = new Label
                {
                    Text = pTitle
                };
                pnl.Controls.Add(lbl);
                ctrl.Controls.Add(pnl);
                pnl = new Panel();
                pnl.Controls.Add(txt);
                ctrl.Controls.Add(pnl);
            }
            return txt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        // EG 20210126 [25556] Minification des fichiers JQuery et des CSS
        // EG 20210324 [25562] Gestion de la piste d'audit post UPGV10 (absence TRADE_P, TRADEXML_P)
        public Cst.ErrLevel CompareTradeTracks()
        {
            Cst.ErrLevel ret = Cst.ErrLevel.ABORTED;

            string tradeXMLSelected = m_TradeTrack.track[GetIndexDDLTracks(DDLTRACKSELECTED_ID)].tradeXML;
            string tradeXMLToCompare = m_TradeTrack.track[GetIndexDDLTracks(DDLTRACKTOCOMPARE_ID)].tradeXML;

            if (String.IsNullOrEmpty(tradeXMLSelected) || String.IsNullOrEmpty(tradeXMLSelected))
            {
                AddXmlToPlaceHolder("phDiffXMLTracks", "Trade" + " [" + GetLabelTrack(DDLTRACKSELECTED_ID) + "]", Ressource.GetString("Msg_AuditTrackUnavailable"));
            }
            else
            {
                XMLCompare xmlCompare = new XMLCompare(tradeXMLSelected, tradeXMLToCompare, ref ret);

                if (Cst.ErrLevel.SUCCESS == ret)
                {
                    string labelTracksSelected = "Trade" + " [" + GetLabelTrack(DDLTRACKSELECTED_ID) + "]";
                    string labelTracksToCompare = "Trade" + " [" + GetLabelTrack(DDLTRACKTOCOMPARE_ID) + "]";
                    PlaceHolder plhResult = xmlCompare.CreatePlaceHolder(0, "Compare Results", labelTracksSelected, labelTracksToCompare);
                    Control ctrl = FindControl("phDiffXMLTracks");
                    if (null != ctrl)
                    {
                        ctrl.Controls.Clear();
                        ctrl.Controls.Add(plhResult);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20171025 [23509] Add businessDate, dtExecution, dtOrderEntered, tzFacility
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        // EG 20210324 [25562] Gestion de la piste d'audit post UPGV10 (absence TRADE_P, TRADEXML_P)
        private Cst.ErrLevel ConstructTracks()
        {

            #region Queries Construction
            #region Trade & TRADETRAIL

            string sqlRestrictInstr = string.Empty;
            if (SessionTools.User.IsApplySessionRestrict())
            {
                SessionRestrictHelper sr = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, false);
                sqlRestrictInstr += sr.GetSQLInstr(string.Empty, "ns.IDI") + Cst.CrLf;
            }

            string sqlSelect = @"select tl.IDT_L, tr.IDT, tr.IDENTIFIER, tr.IDI, tr.DISPLAYNAME, tr.DTTRADE, tr.DTEXECUTION, tr.DTORDERENTERED, tr.TZFACILITY, tr.DTBUSINESS, 
            tr.IDSTENVIRONMENT, tr.DTSTENVIRONMENT, 'StatusEnvironment' as CODESTENVIRONMENT, 
            tr.IDSTACTIVATION,  tr.DTSTACTIVATION,  'StatusActivation'  as CODESTACTIVATION,
            tr.IDSTPRIORITY,    tr.DTSTPRIORITY,    'StatusPriority'    as CODESTPRIORITY,
            trx.TRADEXML, 
            ns.IDENTIFIER as IDI_IDENTIFIER, ns.DISPLAYNAME as IDI_DISPLAYNAME, 
            tl.ACTION, tl.DTSYS, tl.HOSTNAME, tl.APPNAME, tl.SCREENNAME,
            tl.IDA, ac.IDENTIFIER as IDA_IDENTIFIER, ac.DISPLAYNAME as IDA_DISPLAYNAME,
            tl.APPVERSION, tl.APPBROWSER, 
            case when tab.ISNCMINI = 1 or tas.ISNCMINI=1 then 1 else 0 end as ISNCMINI,
            case when tab.ISNCMINT = 1 or tas.ISNCMINT=1 then 1 else 0 end as ISNCMINT,
            case when tab.ISNCMINT = 1 or tas.ISNCMINT=1 then 1 else 0 end as ISNCMFIN" + Cst.CrLf;

            string sqlFrom = String.Format(@"from dbo.TRADE tr
            inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT) 
            inner join dbo.TRADETRAIL tl on (tl.IDT = tr.IDT) and (tl.IDT_L= (select MAX(tl2.IDT_L) from dbo.TRADETRAIL tl2 where (tl2.IDT = tl.IDT)))
            inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) {0}
            inner join dbo.ACTOR ac on (ac.IDA = tl.IDA)
            inner join dbo.TRADEACTOR tab on (tab.IDT = tr.IDT) and (tab.IDROLEACTOR = 'COUNTERPARTY') and (tab.BUYER_SELLER = '{1}')
            inner join dbo.TRADEACTOR tas on (tas.IDT = tr.IDT) and (tas.IDROLEACTOR = 'COUNTERPARTY') and (tas.BUYER_SELLER = '{2}')", 
            sqlRestrictInstr, SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.BUYER), SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.SELLER)) + Cst.CrLf;

            string sqlFrom2 = String.Format(@"from dbo.TRADETRAIL tl
            inner join dbo.ACTOR ac on (ac.IDA = tl.IDA)
            left outer join dbo.TRADE_P tr on (tr.IDT = tl.IDT) and (tr.IDTRADE_P = tl.IDTRADE_P)
            left outer join dbo.TRADEXML_P trx on (trx.IDT = tl.IDT) and (trx.IDTRADEXML_P = tl.IDTRADEXML_P)
            left outer join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI) {0}
            left join dbo.TRADEACTOR tab on (tab.IDT = tr.IDT) and (tab.IDROLEACTOR = 'COUNTERPARTY') and (tab.BUYER_SELLER = '{1}')
            left join dbo.TRADEACTOR tas on (tas.IDT = tr.IDT) and (tas.IDROLEACTOR = 'COUNTERPARTY') and (tas.BUYER_SELLER = '{2}')", 
            sqlRestrictInstr, SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.BUYER), SideTools.FirstUCaseBuyerSeller(BuyerSellerEnum.SELLER)) + Cst.CrLf;
            #endregion sqlFrom

            #region sqlWhere
            string sqlWhere = @"where (tr.IDT = @IDT)" + Cst.CrLf;
            string sqlWhere2 = @"where (tl.IDT = @IDT)" + Cst.CrLf;

            string SQLOrderBy = @"order by tl.DTSYS desc";

            #region Final Query
            string sqlQuery = sqlSelect + sqlFrom + sqlWhere;
            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            sqlQuery += sqlSelect + sqlFrom2 + sqlWhere2 + SQLOrderBy;
            sqlQuery += SQLCst.SEPARATOR_MULTISELECT;
            #endregion Final Query
            #endregion Trade & TRADETRAIL

            #region Enum
            sqlQuery += @"select CODE, VALUE, BACKCOLOR, FORECOLOR, VALUE as DISPLAYNAME from dbo.VW_ALL_ENUM" + SQLCst.SEPARATOR_MULTISELECT;
            #endregion Enum

            #region TradeStMatch
            sqlQuery += @"select tr.IDT, tr.IDSTMATCH, tr.DTINS as DTSTMATCH, stm.BACKCOLOR, stm.FORECOLOR, stm.DISPLAYNAME
            from dbo.TRADESTMATCH tr
            inner join dbo.STMATCH stm on (stm.IDSTMATCH = tr.IDSTMATCH)
            where (tr.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            sqlQuery += @"select tr.IDT, tr.IDSTMATCH, tr.DTINS as DTSTMATCH, stm.BACKCOLOR, stm.FORECOLOR, stm.DISPLAYNAME
            from dbo.TRADESTMATCH_P tr
            inner join dbo.STMATCH stm on (stm.IDSTMATCH = tr.IDSTMATCH)
            where (tr.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            #endregion TradeStMatch

            #region TradeStCheck
            sqlQuery += @"select tr.IDT, tr.IDSTCHECK, tr.DTINS as DTSTCHECK, stc.BACKCOLOR, stc.FORECOLOR, stc.DISPLAYNAME
            from dbo.TRADESTCHECK tr
            inner join dbo.STCHECK stc on (stc.IDSTCHECK = tr.IDSTCHECK)
            where (tr.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            sqlQuery += @"select tr.IDT, tr.IDSTCHECK, tr.DTINS as DTSTCHECK, stc.BACKCOLOR, stc.FORECOLOR, stc.DISPLAYNAME
            from dbo.TRADESTCHECK_P tr
            inner join dbo.STCHECK stc on (stc.IDSTCHECK = tr.IDSTCHECK)
            where (tr.IDT = @IDT)" + SQLCst.SEPARATOR_MULTISELECT;
            #endregion TradeStCheck

            #endregion Queries Construction
            // RD 20091228 [16809] Confirmation indicators for each party
            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), m_TradeCommonInput.IdT);

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, dp);

            m_DsAuditTrack = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            m_DsAuditTrack.DataSetName = "TradeTrack";
            #region Tables
            DataTable dtTrade = m_DsAuditTrack.Tables[0];
            dtTrade.TableName = "trade";
            DataTable dtTrade_l = m_DsAuditTrack.Tables[1];
            dtTrade_l.TableName = "tradetrail";
            DataTable dtFpML_Enum = m_DsAuditTrack.Tables[2];
            DataTable dtStMatch = m_DsAuditTrack.Tables[3];
            DataTable dtStMatch_p = m_DsAuditTrack.Tables[4];
            DataTable dtStCheck = m_DsAuditTrack.Tables[5];
            DataTable dtStCheck_p = m_DsAuditTrack.Tables[6];

            #endregion Tables
            #region Relation Settings
            DataColumn[] childColumn = new DataColumn[2] { dtFpML_Enum.Columns["CODE"], dtFpML_Enum.Columns["VALUE"] };
            #region Stenvironment
            DataColumn[] parentColumn = new DataColumn[2] { dtTrade.Columns["CODESTENVIRONMENT"], dtTrade.Columns["IDSTENVIRONMENT"] };
            DataRelation rel = new DataRelation(dtTrade.TableName + "_StEnvironment", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            parentColumn = new DataColumn[2] { dtTrade_l.Columns["CODESTENVIRONMENT"], dtTrade_l.Columns["IDSTENVIRONMENT"] };
            rel = new DataRelation(dtTrade_l.TableName + "_StEnvironment", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            #endregion Stenvironment
            #region StActivation
            parentColumn = new DataColumn[2] { dtTrade.Columns["CODESTACTIVATION"], dtTrade.Columns["IDSTACTIVATION"] };
            rel = new DataRelation(dtTrade.TableName + "_StActivation", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            parentColumn = new DataColumn[2] { dtTrade_l.Columns["CODESTACTIVATION"], dtTrade_l.Columns["IDSTACTIVATION"] };
            rel = new DataRelation(dtTrade_l.TableName + "_StActivation", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            #endregion StActivation
            #region StPriority
            parentColumn = new DataColumn[2] { dtTrade.Columns["CODESTPRIORITY"], dtTrade.Columns["IDSTPRIORITY"] };
            rel = new DataRelation(dtTrade.TableName + "_StPriority", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            parentColumn = new DataColumn[2] { dtTrade_l.Columns["CODESTPRIORITY"], dtTrade_l.Columns["IDSTPRIORITY"] };
            rel = new DataRelation(dtTrade_l.TableName + "_StPriority", parentColumn, childColumn, false);
            m_DsAuditTrack.Relations.Add(rel);
            #endregion StPriority
            #region StMatch
            rel = new DataRelation(dtTrade.TableName + "_StMatch", dtTrade.Columns["IDT"], dtStMatch.Columns["IDT"], false);
            m_DsAuditTrack.Relations.Add(rel);
            rel = new DataRelation(dtTrade_l.TableName + "_StMatch", dtTrade_l.Columns["IDT"], dtStMatch_p.Columns["IDT"], false);
            m_DsAuditTrack.Relations.Add(rel);
            #endregion StMatch
            #region StCheck
            rel = new DataRelation(dtTrade.TableName + "_StCheck", dtTrade.Columns["IDT"], dtStCheck.Columns["IDT"], false);
            m_DsAuditTrack.Relations.Add(rel);
            rel = new DataRelation(dtTrade_l.TableName + "_StCheck", dtTrade_l.Columns["IDT"], dtStCheck_p.Columns["IDT"], false);
            m_DsAuditTrack.Relations.Add(rel);
            #endregion StCheck
            #endregion Relation Settings
            #region TradeTrack Settings
            if (0 != dtTrade.Rows.Count)
            {
                ArrayList aTrack = new ArrayList();
                m_TradeTrack = new TradeTrack();

                #region Trade & Instrument
                DataRow row = dtTrade.Rows[0];
                m_TradeTrack.trade = new Trade(m_TradeCommonInput.IdT, m_TradeCommonInput.Identifier, row["DISPLAYNAME"].ToString(),
                    row["IDI"].ToString(), row["IDI_IDENTIFIER"].ToString(), row["IDI_DISPLAYNAME"].ToString());
                #endregion Trade & Instrument

                #region Track
                int idT_L = Convert.ToInt32(row["IDT_L"]);
                aTrack.Add(AddTrack(row));

                if (0 != dtTrade_l.Rows.Count)
                {
                    foreach (DataRow row_l in dtTrade_l.Rows)
                    {
                        if (idT_L != Convert.ToInt32(row_l["IDT_L"]))
                            aTrack.Add(AddTrack(row_l));
                    }
                }
                m_TradeTrack.track = (EFS.Audit.Track[])aTrack.ToArray(typeof(EFS.Audit.Track));
                #endregion Track
            }
            #endregion TradeTrack Settings
            return Cst.ErrLevel.SUCCESS;

        }

        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };

            Panel pnlTracks = new Panel() { ID = "pnlTracks", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "phTracks"
            };
            pnlTracks.Controls.Add(ph);
            pnlBody.Controls.Add(pnlTracks);

            Panel pnlDiffXMLTracks = new Panel() { ID = "pnlDiffXMLTracks", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "phDiffXMLTracks"
            };
            pnlDiffXMLTracks.Controls.Add(ph);
            pnlBody.Controls.Add(pnlDiffXMLTracks);

            CellForm.Controls.Add(pnlBody);
        }

        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) Correction et compléments
        private void AddFooter()
        {
            Panel pnlFooter = new Panel() { ID = "divfooter", CssClass = CSSMode + " track " + m_TradeCommonInputGUI.MainMenuClassName };
            WCToolTipLinkButton btn = new WCToolTipLinkButton()
            {
                ID = "btnXML",
                CssClass = "fa-icon",
                AccessKey = "X",
                CausesValidation = false,
                Text = "<i class='fas fa-external-link-alt'></i> XML"
            };
            btn.Click += new EventHandler(OnXmlClick);
            btn.OnClientClick = "javascript:RemoveAudit();return true;";
            pnlFooter.Controls.Add(btn);
            CellForm.Controls.Add(pnlFooter);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20160804 [Migration TFS] Modify
        public Cst.ErrLevel DisplayTracks()
        {
            if (null != m_TradeTrack)
            {
                m_CurrentTradeXML = m_TradeTrack.track[0].tradeXML;

                #region Tracks construction
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(typeof(TradeTrack), m_TradeTrack);
                StringBuilder sbTrack = CacheSerializer.Serialize(serializerInfo);
                // FI 20160804 [Migration TFS] GUIOutput Folder
                //string xslTrackFile = SessionTools.NewAppInstance().SearchFile(SessionTools.CS, @"~\OTCML\XSL_Files\Track\Track.xslt");      
                string xslTrackFile = string.Empty;
                SessionTools.AppSession.AppInstance.SearchFile2(SessionTools.CS, @".\GUIOutput\Track\Track.xslt", ref xslTrackFile);
                #endregion Tracks construction

                #region Events XSLT application
                Hashtable param = new Hashtable
                {
                    { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name },
                    { "pTrackSelected", GetIndexDDLTracks(DDLTRACKSELECTED_ID) + 1 },
                    { "pTrackToCompare", GetIndexDDLTracks(DDLTRACKTOCOMPARE_ID) + 1 }
                };
                string retTransForm = XSLTTools.TransformXml(sbTrack.Replace("xmlns=", "xmln="), xslTrackFile, param, null).ToString();
                #endregion Event XSLT application

                #region Add html transformation to placeHolder
                Control ctrl = FindControl("phTracks");
                if (null != ctrl)
                {
                    ctrl.Controls.Clear();
                    LiteralControl lit = new LiteralControl(retTransForm);
                    ctrl.Controls.Add(lit);
                }
                #endregion Add html transformation to placeHolder
            }
            return Cst.ErrLevel.SUCCESS;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200729 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correctifs et compléments (Track, Banner, Tracker ...)
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmTracks";
            if (IsSessionVariableAvailable)
            {
                AddToolBar();
                AddBody();
                AddFooter();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(OnLoad);
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void PageConstruction()
        {

            AbortRessource = true;
            InitializeComponent();

            string mainRessource = string.Empty;
            string cssMode = string.Empty;
            string idMenu = string.Empty;
            string identifier = string.Empty;
            if (IsSessionVariableAvailable)
            {
                mainRessource = m_TradeCommonInputGUI.MainRessource;
                identifier = m_TradeCommonInput.Identifier;
                idMenu = m_TradeCommonInputGUI.IdMenu;
                cssMode = m_TradeCommonInputGUI.CssMode;
            }

            string title = Ressource.GetString2("TracksOf", mainRessource, identifier);
            PageTitle = title;
            HtmlPageTitle titleLeft = new HtmlPageTitle(title);
            GenerateHtmlForm();
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, idMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, cssMode, false, null, idMenu);

            if (IsSessionVariableAvailable)
            {
                HtmlGenericControl body = PageTools.SearchBodyControl(this);
                body.Attributes.Add("onload", @"ChangeTrack('TrackFirst');");
            }
            else
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_SessionVariableParentNotAvailable");
                CloseAfterAlertImmediate = true;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdDDLTracks"></param>
        /// <returns></returns>
        private int GetIndexDDLTracks(string pIdDDLTracks)
        {
            int itemIndex = 0;
            HtmlInputHidden ctrl = (HtmlInputHidden)FindControl(pIdDDLTracks);
            if (null != ctrl)
                itemIndex = Convert.ToInt32(ctrl.Value);
            return itemIndex;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdDDLTracks"></param>
        /// <returns></returns>
        private string GetLabelTrack(string pIdDDLTracks)
        {
            EFS.Audit.Track track = m_TradeTrack.track[GetIndexDDLTracks(pIdDDLTracks)];
            return track.action + " - " + track.date;
        }
        #endregion Methods
    }
}
