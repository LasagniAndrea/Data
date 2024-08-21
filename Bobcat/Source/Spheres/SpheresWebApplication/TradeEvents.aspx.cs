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
    public partial class TradeEvents : TradeCommonEventsPage
    {
        #region Members
        private TradeInputGUI m_InputGUI;
        private TradeInput m_Input;
        private TradeHeaderBanner m_TradeHeaderBanner;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return m_TradeHeaderBanner; }
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
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            InitializeComponent();
            
            m_ParentGUID = Request.QueryString["GUID"];

            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //m_InputGUI = Session[InputGUISessionID] as TradeInputGUI;
            //m_Input = Session[InputSessionID] as TradeInput;

            m_InputGUI = DataCache.GetData<TradeInputGUI>(ParentInputGUISessionID);
            m_Input = DataCache.GetData<TradeInput>(ParentInputSessionID);

            base.OnInit(e);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            if (IsSessionVariableAvailable)
                JavaScript.OpenSubTradeEvents(this, m_ParentGUID);
        }
        /// <summary>
        /// 
        /// </summary>
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmTradeEvents";
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
                sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.CalculationPeriod);
                sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.NominalStep);
                sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.Reset);
            }
            else
                sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.SelfAverage);
            sqlWhere += SQLCst.AND + "tblmain.EVENTCODE!=" + DataHelper.SQLString(EventCodeFunc.UnderlyerValuationDate);
            return sqlWhere;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTableCell"></param>
        // EG 20200724 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void InitHeaderBanner(Control pCtrlContainer)
        {
            m_TradeHeaderBanner = new TradeHeaderBanner(this, GUID, pCtrlContainer, m_Input, m_InputGUI, false);
            m_TradeHeaderBanner.AddControls();
        }
        /// <summary>
        /// 
        /// </summary>
        private void InitializeComponent()
        {
            this.Load += new System.EventHandler(OnLoad);
        }

        #endregion Methods
    }
}
