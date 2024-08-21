using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
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

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de CodeEvent.
    /// </summary>
    /// FI 20160804 [Migration TFS] Modify
    public partial class CodeEventPage : PageBase
	{
		#region Members
		//protected string xsltFile = @"~\OTCML\XSL_Files\Event\EventEnums.xslt";
        protected string xsltFile = @"~\GUIOutput\Event\EventEnums.xslt";

		private string m_ParentGUID;
		private TradeCommonInputGUI m_TradeCommonInputGUI;
		// FI 20200518 [XXXXX] Mise en commentaire
		//private TradeCommonInput m_TradeCommonInput;
		#endregion Members
		#region Accessors
		#region InputGUISessionID
		protected string ParentInputGUISessionID
		{
			get { return m_ParentGUID + "_GUI"; }
		}
		#endregion InputGUISessionID


		#endregion Accessors
		#region Events
		#region OnInit
		override protected void OnInit(EventArgs e)
		{
			InitializeComponent();

			base.OnInit(e);

			AbortRessource = true;

			m_ParentGUID = Request.QueryString["GUID"];
			if (StrFunc.IsEmpty(m_ParentGUID))
				throw new ArgumentException("Argument GUID expected");

			// FI 20200518 [XXXXX] Utilisation de DataCache
			//m_TradeCommonInputGUI = (TradeCommonInputGUI)Session[InputGUISessionID];
			//m_TradeCommonInput = (TradeCommonInput)Session[InputSessionID];

			m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(ParentInputGUISessionID);
			

			PageConstruction();
		}
		#endregion OnInit
		#region OnLoad
		protected void OnLoad(object sender, System.EventArgs e)
		{
			DisplayEventEnums();
		}
		#endregion OnLoad
		#region OnRefreshClick
		private void OnRefreshClick(object sender, EventArgs e)
		{
			DisplayEventEnums();
		}
		#endregion OnRefreshClick
		#endregion Events
		#region Methods
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
		protected void AddToolBar()
		{
			Panel pnlToolBar = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };

			WCToolTipLinkButton btnRefresh = ControlsTools.GetToolTipLinkButtonRefresh();
			btnRefresh.Click += new EventHandler(OnRefreshClick);
			pnlToolBar.Controls.Add(btnRefresh);

			WCToolTipLinkButton btnExpand = ControlsTools.GetAwesomeButtonExpandCollapse("fa fa-minus-square",true);
			pnlToolBar.Controls.Add(btnExpand);

			//pnlToolBar.Controls.Add(ControlsTools.GetToolTipLinkButtonPrint("tblCodeEvent"));

			CellForm.Controls.Add(pnlToolBar);
		}
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		protected void AddBody()
		{
            PlaceHolder plhCodeEvent = new PlaceHolder
            {
                ID = "plhCodeEvent"
            };
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
			plhCodeEvent.Controls.Add(pnlBody);
			CellForm.Controls.Add(plhCodeEvent);
		}

		#region DisplayEventEnums
		// EG 20180423 Analyse du code Correction [CA2200]
		// EG 20180423 Analyse du code Correction [CA2202]
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
		private Cst.ErrLevel DisplayEventEnums()
		{
			string CS = SessionTools.CS;
			Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
			#region Query 
			string SQLQuery = SQLCst.SELECT + "CODE,EXTCODE,URI,DEFINITION,DOCUMENTATION,DEFINED" + Cst.CrLf;
			SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_ENUMS + Cst.CrLf;
			SQLQuery += SQLCst.WHERE + "CODE in (" + DataHelper.SQLString("EventCode") + "," + Cst.CrLf;
			SQLQuery += DataHelper.SQLString("EventType") + "," + DataHelper.SQLString("EventClass") + ")";
			SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
			SQLQuery += SQLCst.SELECT + "CODE,VALUE,EXTVALUE,SOURCE,DOCUMENTATION" + Cst.CrLf;
			SQLQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.VW_ALL_ENUM + Cst.CrLf;
			SQLQuery += SQLCst.ORDERBY + "CODE,VALUE";
			SQLQuery += SQLCst.SEPARATOR_MULTISELECT;
			#endregion Query 

            TextWriter writer = null;
			try
			{
                //20090722 FI SetCacheOn 
                DataSet dsEnums             = DataHelper.ExecuteDataset(CSTools.SetCacheOn(CS),CommandType.Text,SQLQuery);
				dsEnums.DataSetName         = "ExtendEnums";
				DataTable dtExtendEnum      = dsEnums.Tables[0];
				dtExtendEnum.TableName      = "ExtendEnum";
				DataTable dtExtendEnumValue = dsEnums.Tables[1];
				dtExtendEnumValue.TableName = "ExtendEnumValue";
				#region Mapping
				dtExtendEnum.Columns["CODE"].ColumnMapping           = MappingType.Attribute;
				dtExtendEnum.Columns["EXTCODE"].ColumnMapping        = MappingType.Attribute;
				dtExtendEnumValue.Columns["CODE"].ColumnMapping      = MappingType.Hidden;
				dtExtendEnumValue.Columns["VALUE"].ColumnMapping     = MappingType.Attribute;
				dtExtendEnumValue.Columns["EXTVALUE"].ColumnMapping  = MappingType.Attribute;
                #endregion Mapping
                #region Relation
                DataRelation rel = new DataRelation("Enums_Enum", dtExtendEnum.Columns["CODE"], dtExtendEnumValue.Columns["CODE"], false)
                {
                    Nested = true
                };
                dsEnums.Relations.Add(rel);
				#endregion Relation
				#region Event enums XML Construction
                StringBuilder sb = new StringBuilder();
                writer = new StringWriter(sb);
                XmlDocument doc = new XmlDocument();
                dsEnums.WriteXml(writer);
                doc.LoadXml(sb.ToString());
                //writer.Close();
                //
                Hashtable param = new Hashtable
                {
                    { "pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name }
                };

                string xslFile = string.Empty;
                SessionTools.AppInstance.SearchFile2(SessionTools.CS, xsltFile, ref xslFile);      
                string retTransForm = XSLTTools.TransformXml(sb, xslFile, param, null).ToString();
				#endregion Event enums XSLT application
				#region Add html transformation to placeHolder
				Control ctrl = FindControl("divbody");
				if (null != ctrl)
				{
					ctrl.Controls.Clear();
                    LiteralControl lit = new LiteralControl(retTransForm);
					ctrl.Controls.Add(lit);
				}
				#endregion Add html transformation to placeHolder
				//writer.Close();
				ret = Cst.ErrLevel.SUCCESS;
			}
			catch (Exception)
			{
				throw;
			}
            finally
            {
                if (null != writer)
                    writer.Close();
            }
			return ret;
		}
		#endregion DisplayEventEnums
		#region GenerateHtmlForm
		// EG 20200825 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
		protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm();
			Form.ID = "frmCodeEvent";
			AddToolBar();
			AddBody();
		}
		#endregion
        #region InitializeComponent
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(this.OnLoad);
        }
		#endregion InitializeComponent
		#region PageConstruction
		// EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
		protected override void PageConstruction()
        {
            GenerateHtmlForm();
            string title = Ressource.GetString("TBL_EVENTENUMS", true);
            HtmlPageTitle titleLeft = new HtmlPageTitle(title);
            this.PageTitle = title;
            FormTools.AddBanniere(this, Form, titleLeft, new HtmlPageTitle(), null, m_TradeCommonInputGUI.IdMenu);
            PageTools.BuildPage(this, Form, this.PageFullTitle, m_TradeCommonInputGUI.CssMode, false, null, m_TradeCommonInputGUI.IdMenu);
		}
		#endregion PageConstruction
		#endregion Methods

	}
}
