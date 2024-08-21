#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.TradeInformation;
using EfsML;
using EfsML.Business;
using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Web.UI;
#endregion using directives


namespace EFS.Spheres
{
    public partial class TradeActionPage : TradeCommonActionPage
	{
		#region Members
		private TradeActionEvents  m_TradeActionEvents;
		private TradeInputGUI      m_TradeInputGUI; // FI 20200518 [XXXXX] Rename m_InputGUI
        private TradeInput         m_TradeInput; // FI 20200518 [XXXXX] Rename m_Input
        private TradeHeaderBanner m_TradeHeaderBanner;
        #endregion Members
        #region Accessors
        

        #region CurrentActionEvent
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        public override TradeActionEventBase CurrentActionEvent
        {
            get {return m_TradeActionEvents.GetEvent(m_CurrentIdE);}
        }
        #endregion CurrentActionEvent
        #region ActionRootName
        protected override string ActionRootName
        {
            get { return "TradeAction"; }
        }
        #endregion ActionRootName
    
        #region TradeCommonActionEvents
        /// EG 20150513 [20513] TradeCommonActionEvents devient TradeActionEventsBase
        protected override TradeActionEventsBase TradeCommonActionEvents
        {
            get { return m_TradeActionEvents; }
        }
        #endregion TradeCommonActionEvents
        
        #region TradeCommonInput
        public override TradeCommonInput TradeCommonInput
        {
            get { return m_TradeInput; }
        }
        #endregion TradeCommonInput
        #region TradeCommonInputGUI
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_TradeInputGUI; }
        }
        #endregion TradeCommonInputGUI
        #region TradeInput
        public TradeInput TradeInput
        {
            get { return m_TradeInput; }
        }
        #endregion TradeInput
        #endregion Accessors
        #region Events
        #region OnInit
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // FI 20200518 [XXXXX] Use DataCache
            //m_InputGUI = (TradeInputGUI)Session[InputGUISessionID];
            //m_Input = (TradeInput)Session[InputSessionID];
            m_TradeInputGUI = DataCache.GetData<TradeInputGUI>(ParentInputGUISessionID);
            m_TradeInput = DataCache.GetData<TradeInput>(ParentInputSessionID);

            PageConstruction();
        }
		#endregion
		#region Render
		protected override void Render(HtmlTextWriter writer)
		{
			m_TradeHeaderBanner.DisplayHeader(true,true);
            
			base.Render (writer);
        }
        #endregion Render
        #region OnLoad
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnLoad(System.EventArgs e)
        {

            if (false == IsPostBack)
            {
                string idMenu = TradeActionType.GetMenuActionType(m_TradeActionType);
                m_InputActionGUI = new InputGUI(SessionTools.User, idMenu, string.Empty);
                m_InputActionGUI.InitializeFromMenu(CSTools.SetCacheOn(SessionTools.CS));
                m_InputActionGUI.CssColor = AspTools.GetCssColorDefault(m_TradeInput.SQLInstrument);
                m_InputActionGUI.CssMode = SessionTools.CSSMode.ToString();
            }
            else
            {
                // FI 20200518 [XXXXX] Use DataCache
                //m_InputActionGUI = (InputGUI)Session[InputActionGUISessionID];
                m_InputActionGUI = DataCache.GetData<InputGUI>(InputActionGUISessionID);
            }
            
            base.OnLoad(e);

        }
        #endregion OnLoad
        #endregion Events
        #region Methods
        #region DeserializeEvents
        // EG 20180423 Analyse du code Correction [CA2200]
        public override Cst.ErrLevel DeserializeEvents(DataSet pDsEvents)
        {
            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            TextWriter writer = null;
            try
            {
                #region Deserialize
                StringBuilder sb = new StringBuilder();
                writer = new StringWriter(sb);
                pDsEvents.WriteXml(writer);
                sb.Insert(pDsEvents.DataSetName.Length + 2, "<gProduct>" + m_TradeInput.SQLProduct.GProduct + "</gProduct>");
                sb.Insert(pDsEvents.DataSetName.Length + 2, "<tradeActionType>" + m_TradeActionType.ToString() + "</tradeActionType>");
                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(sb.ToString());
                m_TradeActionEvents = (TradeActionEvents)CacheSerializer.Deserialize(serializeInfo);
                // FI 20200518 [XXXXX] Use DataCache
                //Session[TradeActionEventsSessionID] = m_TradeActionEvents;
                DataCache.SetData<TradeActionEvents>(TradeActionEventsSessionID, m_TradeActionEvents);

                ret = Cst.ErrLevel.SUCCESS;
                #endregion Deserialize
            }
            finally
            {
                if (null != writer)
                    writer.Close();
            }
            return ret;
        }
        #endregion DeserializeEvents
        #region DisplayEvents
        /// EG 20150513 [20513] m_Input.DataDocument (DataDocumentContainer) est passé en argument à CreateControls 
        /// en lieu et place de m_Input.FpMLDocReader (IDocument) 
        public override void DisplayEvents()
        {
            Control ctrl = PlaceHolderTradeActionEvents;
            if (null != ctrl)
            {
                ctrl.Controls.Clear();
                if (null != m_TradeActionEvents)
                {
                    FullConstructor fullCtor = new FullConstructor(m_TradeInput.FpMLDataDocReader.Version);
                    ctrl.Controls.Add(m_TradeActionEvents.CreateControls(m_TradeInput.SQLProduct.Family, m_TradeActionType, m_TradeActionMode,
                        m_CurrentIdE, m_TradeInput.DataDocument, fullCtor));
                    CreateCurrentAction();
                }
                else
                    ctrl.Controls.Add(new LiteralControl("No Events"));
            }
        }
        #endregion DisplayEvents
        #region GenerateHtmlForm
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void GenerateHtmlForm()
		{
			base.GenerateHtmlForm(); 
			m_TradeHeaderBanner = new TradeHeaderBanner(this,GUID,CellForm,m_TradeInput,m_TradeInputGUI,false);
            m_TradeHeaderBanner.AddControls();  
			m_TradeHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();
            AddBody();
		
		}
		#endregion GenerateHtmlForm
        #region GetActionEvent
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        public override TradeActionEventBase GetActionEvent(int pIdE)
        {
            return m_TradeActionEvents.GetEvent(pIdE);
        }
        #endregion GetActionEvent
        #region SetActionMQueue
        protected override MQueueBase SetActionMQueue(SortedList pSortedAction, IdInfo pIdInfo)
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = SessionTools.CS,
                id = TradeCommonInput.IdT,
                idInfo = pIdInfo
            };

            return new TradeActionGenMQueue(mQueueAttributes, pSortedAction);
        }
        #endregion SetActionMQueue
        #region SetTradeActionEvents
        protected override void SetTradeActionEvents()
        {
            // FI 20200518 [XXXXX] Use DataCache
            m_TradeActionEvents = DataCache.GetData<TradeActionEvents>(TradeActionEventsSessionID);
        }
        #endregion SetTradeActionEvents
        #endregion Methods
    }
}