#region Using Directives
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;

using EFS.Referential;
using EFS.TradeInformation;

using EfsML.Enum;
using EfsML.Enum.Tools;

#endregion Using Directives
namespace EFS.Spheres
{
    /// <summary>
    /// 
    /// </summary>
    public partial class TradeAdminEventsPage : TradeCommonEventsPage
    {
        #region Members
        private TradeAdminInputGUI m_InputGUI;
        private TradeAdminInput m_Input;
        private TradeAdminHeaderBanner m_TradeAdminHeaderBanner;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return m_TradeAdminHeaderBanner; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get { return m_Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return m_InputGUI; }
        }

        #endregion Accessors

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            JavaScript.OpenSubTradeAdminEvents(this, m_ParentGUID);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {

            InitializeComponent();
            m_ParentGUID = Request.QueryString["GUID"];

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //m_InputGUI = (TradeAdminInputGUI)Session[InputGUISessionID];
            //m_Input = (TradeAdminInput)Session[InputSessionID];

            m_InputGUI = DataCache.GetData<TradeAdminInputGUI>(ParentInputGUISessionID);
            m_Input = DataCache.GetData<TradeAdminInput>(ParentInputSessionID);

            base.OnInit(e);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmTradeAdminEvents";
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override string GetWhereCalculationLevel()
        {
            string sqlWhere = string.Empty;
            if (IsWithoutCalculationLevel)
            {
                sqlWhere += SQLCst.AND + "tblmain.EVENTTYPE!=" + DataHelper.SQLString(EventTypeFunc.CapRebate);
                sqlWhere += SQLCst.AND + "tblmain.EVENTTYPE!=" + DataHelper.SQLString(EventTypeFunc.BracketRebate);
                sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.Reset);
            }
            sqlWhere += SQLCst.AND + "((tblmain.IDE_SOURCE is null)" + SQLCst.OR + "(es.FAMILY = " + DataHelper.SQLString(Cst.ProductFamily_INV) + "))";
            return sqlWhere;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableCell"></param>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void InitHeaderBanner(Control pCtrlContainer)
        {
            m_TradeAdminHeaderBanner = new TradeAdminHeaderBanner(this, GUID, pCtrlContainer, m_Input, m_InputGUI, false);
            m_TradeAdminHeaderBanner.AddControls();
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(OnLoad);
        }
        #endregion InitializeComponent

    }

}
