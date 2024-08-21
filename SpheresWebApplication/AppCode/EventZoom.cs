#region Using Directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.Referential;
using EFS.TradeInformation;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Threading;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// 
    /// </summary>
    public partial class EventZoomPage : PageBase
	{
		#region Members
		protected DataSet m_DsEvent;
		protected int  idT;
		protected int idE;
		protected string m_EventArgument;
		protected string m_XSLEvent;

		protected string m_ParentGUID;
		protected TradeCommonInputGUI m_TradeCommonInputGUI;
        protected TradeCommonInput    m_TradeCommonInput;

		protected string[] m_TableNames;
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

        #endregion Accessors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            base.OnInit(e);
            //
            AbortRessource = true;

            idE = Convert.ToInt32(Request.QueryString["IdE"]);
            m_ParentGUID = Request.QueryString["GUID"];

            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Use DataCache
            //m_TradeCommonInputGUI = (TradeCommonInputGUI)Session[InputGUISessionID];
            //m_TradeCommonInput = (TradeCommonInput)Session[InputSessionID];
            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(ParentInputSessionID);

            idT = m_TradeCommonInput.IdT;
            m_TableNames = new string[3];

            GenerateHtmlForm();
            string title = Ressource.GetString(Request.QueryString["Title"], true);
            PageTitle = title;

            PageTools.BuildPage(this, Form, PageFullTitle, m_TradeCommonInputGUI.CssMode, false, string.Empty, m_TradeCommonInputGUI.IdMenu);
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        protected void OnLoad(object sender, System.EventArgs e)
        {
            DisplayEvent();
        }
		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void OnRefresh(object sender, EventArgs e)
		{
			DisplayEvent();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        protected void ConstructDataSetEvent()
        {
            

            GetSelectEvent();
            m_DsEvent.DataSetName = "Events";
            #region Events
            DataTable dtEvent = m_DsEvent.Tables[0];
            dtEvent.TableName = m_TableNames[0];
            DataTable dtEventClass = m_DsEvent.Tables[1];
            dtEventClass.TableName = "ClassEvent";
            DataTable dtEventAsset = m_DsEvent.Tables[2];
            dtEventAsset.TableName = "Asset" + m_TableNames[0];
            #region Relations Setting
            DataColumn dcIdE = dtEvent.Columns["IDE"];
            DataColumn dcIdE_Class = dtEventClass.Columns["IDE"];
            DataColumn dcIdE_Asset = dtEventAsset.Columns["IDE"];
            DataRelation rel = new DataRelation("Class_" + m_TableNames[0], dcIdE, dcIdE_Class, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            rel = new DataRelation("Asset_" + m_TableNames[0], dcIdE, dcIdE_Asset, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            #endregion Relations Setting
            #endregion Events
            #region Child events
            DataTable dtEventChild = m_DsEvent.Tables[3];
            dtEventChild.TableName = m_TableNames[1];
            DataTable dtEventClassChild = m_DsEvent.Tables[4];
            dtEventClassChild.TableName = "Class" + m_TableNames[1];
            DataTable dtEventAssetChild = m_DsEvent.Tables[5];
            dtEventAssetChild.TableName = "Asset" + m_TableNames[1];

            #region Relations Setting
            dcIdE = dtEventChild.Columns["IDE"];
            dcIdE_Class = dtEventClassChild.Columns["IDE"];
            dcIdE_Asset = dtEventAssetChild.Columns["IDE"];
            rel = new DataRelation("Class_" + m_TableNames[1], dcIdE, dcIdE_Class, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            rel = new DataRelation("Asset_" + m_TableNames[1], dcIdE, dcIdE_Asset, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            #endregion Relations Setting

            #endregion Child events
            #region SubChild events
            DataTable dtEventSubChild = m_DsEvent.Tables[6];
            dtEventSubChild.TableName = m_TableNames[2];
            DataTable dtEventClassSubChild = m_DsEvent.Tables[7];
            dtEventClassSubChild.TableName = "Class" + m_TableNames[2];
            DataTable dtEventAssetSubChild = m_DsEvent.Tables[8];
            dtEventAssetSubChild.TableName = "Asset" + m_TableNames[2];

            #region Relations Setting
            dcIdE = dtEventSubChild.Columns["IDE"];
            dcIdE_Class = dtEventClassSubChild.Columns["IDE"];
            dcIdE_Asset = dtEventAssetSubChild.Columns["IDE"];
            rel = new DataRelation("Class_" + m_TableNames[2], dcIdE, dcIdE_Class, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            rel = new DataRelation("Asset_" + m_TableNames[2], dcIdE, dcIdE_Asset, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            #endregion Relations Setting

            #endregion SubChild events

            #region Relations Setting
            dcIdE = dtEventChild.Columns["IDE"];
            DataColumn dcIdE_Event = dtEventSubChild.Columns["IDE_EVENT"];
            rel = new DataRelation(m_TableNames[2], dcIdE, dcIdE_Event, false)
            {
                Nested = true
            };
            m_DsEvent.Relations.Add(rel);
            #endregion Relations Setting

        }
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
        // EG 20180423 Analyse du code Correction [CA2200]
        protected void DisplayEvent()
        {
            XmlTextWriter xmlWriter = null;
            try
            {
                ConstructDataSetEvent();

                Control plhEvent = FindControl("plhEvent");
                plhEvent.Controls.Clear();
                StringBuilder sb = new StringBuilder();
                xmlWriter = new XmlTextWriter(new StringWriter(sb));
                m_DsEvent.WriteXml(xmlWriter);
                if ((null != plhEvent) && (0 < m_DsEvent.Tables[0].Rows.Count))
                    plhEvent.Controls.Add(new LiteralControl(TransformEvent(sb.ToString())));

            }
            catch (Exception) { throw; }
            finally
            {
                if (null != xmlWriter)
                    xmlWriter.Close();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm();
            Form.ID = "frmEvent";
            AddToolBar();
            AddBody();
		}
        protected void AddToolBar()
        {
            Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
            btnRefresh.Click += new EventHandler(OnRefresh);
            pnlToolBar.Controls.Add(btnRefresh);
            pnlToolBar.Controls.Add(ControlsTools.GetButtonPrint("tblEvents"));
            CellForm.Controls.Add(pnlToolBar);
        }

        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void AddBody()
        {
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            WCTogglePanel pnl = new WCTogglePanel();
            PlaceHolder plhEvent = new PlaceHolder() { ID = "plhEvent" };
            pnl.AddContent(plhEvent);
            pnlBody.Controls.Add(pnl);
            CellForm.Controls.Add(pnlBody);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected void GetSelectEvent()
        {
            //ArrayList aTemp;
            //ArrayList aTemp2;
            SQLReferentialData.SQLSelectParameters sqlSelectParameters;
            #region Event

            string m_XMLRefEvent = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTZOOM");
            string m_XMLRefEventClass = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTCLASS");
            string m_XMLRefEventAsset = ReferentialTools.GetObjectXMLFile(Cst.ListType.Event, "EVENTASSET");
            ReferentialTools.DeserializeXML_ForModeRO(m_XMLRefEvent, out ReferentialsReferential refEvent);
            ReferentialTools.DeserializeXML_ForModeRO(m_XMLRefEventClass, out ReferentialsReferential refEventClass);
            ReferentialTools.DeserializeXML_ForModeRO(m_XMLRefEventAsset, out ReferentialsReferential refEventAsset);

            StrBuilder sqlEventClass = new StrBuilder();
            //sqlEventClass += SQLReferentialData.GetSQLSelect(CS, refEventClass, "e.IDT=@IDT and (@IDE=@IDE)", false, false, out aTemp, out aTemp2).Query;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventClass, "e.IDT=@IDT and (@IDE=@IDE)");
            sqlEventClass += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;

            StrBuilder sqlEventAsset = new StrBuilder();
            //sqlEventAsset += SQLReferentialData.GetSQLSelect(CS, refEventAsset, "e.IDT=@IDT and (@IDE=@IDE)", false, false, out aTemp, out aTemp2).Query;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventAsset, "e.IDT=@IDT and (@IDE=@IDE)");
            sqlEventAsset += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;

            StrBuilder sqlEvent = new StrBuilder();
            //sqlEvent += SQLReferentialData.GetSQLSelect(CS, refEvent, "IDE=@IDE and (@IDT=@IDT)", false, false, out aTemp, out aTemp2).Query + SQLCst.SEPARATOR_MULTISELECT;
            //sqlEvent += SQLReferentialData.GetSQLSelect(CS, refEventClass, "e.IDE=@IDE and (@IDT=@IDT)", false, false, out aTemp, out aTemp2).Query + SQLCst.SEPARATOR_MULTISELECT;
            //sqlEvent += SQLReferentialData.GetSQLSelect(CS, refEventAsset, "e.IDE=@IDE and (@IDT=@IDT)", false, false, out aTemp, out aTemp2).Query + SQLCst.SEPARATOR_MULTISELECT;
            //sqlEvent += SQLReferentialData.GetSQLSelect(CS, refEvent, "IDE_EVENT=@IDE and (@IDT=@IDT)", false, false, out aTemp, out aTemp2).Query + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEvent, "IDE=@IDE and (@IDT=@IDT)");
            sqlEvent += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventClass, "e.IDE=@IDE and (@IDT=@IDT)");
            sqlEvent += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEventAsset, "e.IDE=@IDE and (@IDT=@IDT)");
            sqlEvent += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEvent, "IDE_EVENT=@IDE and (@IDT=@IDT)");
            sqlEvent += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;
            
            sqlEvent += sqlEventClass.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            sqlEvent += sqlEventAsset.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            
            //sqlEvent += SQLReferentialData.GetSQLSelect(CS, refEvent, "IDT=@IDT and (@IDE=@IDE)", false, false, out aTemp, out aTemp2).Query + SQLCst.SEPARATOR_MULTISELECT;
            sqlSelectParameters = new SQLReferentialData.SQLSelectParameters(SessionTools.CS, refEvent, "IDT=@IDT and (@IDE=@IDE)");
            sqlEvent += SQLReferentialData.GetSQLSelect(sqlSelectParameters).Query;

            sqlEvent += sqlEventClass.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            sqlEvent += sqlEventAsset.ToString() + SQLCst.SEPARATOR_MULTISELECT;
            #endregion Event

            DataParameters dp = new DataParameters();
            dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), idT);
            dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), idE);

            QueryParameters qp = new QueryParameters(SessionTools.CS, sqlEvent.ToString(), dp);

            m_DsEvent = DataHelper.ExecuteDataset(SessionTools.CS, CommandType.Text, qp.Query, qp.Parameters.GetArrayDbParameter());
        }
		/// <summary>
		/// 
		/// </summary>
		private void InitializeComponent()
		{
			this.Load += new System.EventHandler(this.OnLoad);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pEvents"></param>
		/// <returns></returns>
        protected string TransformEvent(string pEvents)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(pEvents);

            string xslt = m_XSLEvent;

            Hashtable param = new Hashtable
            {
                { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name }
            };

            return XSLTTools.TransformXml(sb, xslt, param, null);
        }
		#endregion Methods
	}
}
