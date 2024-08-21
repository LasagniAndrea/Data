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
    public partial class DebtSecEvents : TradeCommonEventsPage
    {
        #region Members
        private DebtSecInputGUI m_InputGUI;
        private DebtSecInput m_Input;
        private DebtSecHeaderBanner m_HeaderBanner;
        #endregion Members

        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return m_HeaderBanner; }
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
            m_InputGUI = DataCache.GetData<DebtSecInputGUI>(ParentInputGUISessionID);
            m_Input = DataCache.GetData<DebtSecInput>(ParentInputSessionID);

            base.OnInit(e);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            Form.ID = "frmDebtSecEvents";
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            if (IsSessionVariableAvailable)
                JavaScript.OpenSubDebtSecEvents(this, m_ParentGUID);
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
            m_HeaderBanner = new DebtSecHeaderBanner(this, GUID, pCtrlContainer, m_Input, m_InputGUI, false);
            m_HeaderBanner.AddControls();
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
