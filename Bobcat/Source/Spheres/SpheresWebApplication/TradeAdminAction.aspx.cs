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
    public partial class TradeAdminActionPage : TradeCommonActionPage
    {
        #region Members
        private TradeAdminActionEvents m_TradeAdminActionEvents;
        private TradeAdminInputGUI m_InputGUI;
        private TradeAdminInput m_Input;
        private TradeAdminHeaderBanner m_TradeHeaderBanner;
        #endregion Members
        #region Accessors
        #region CurrentActionEvent
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        public override TradeActionEventBase CurrentActionEvent
        {
            get { return m_TradeAdminActionEvents.GetEvent(m_CurrentIdE); }
        }
        #endregion CurrentActionEvent
        #region ActionRootName
        protected override string ActionRootName
        {
            get { return "TradeAdminAction"; }
        }
        #endregion ActionRootName
        #region Input
        protected TradeAdminInput Input
        {
            set { m_Input = value; }
            get { return m_Input; }
        }
        #endregion Input
        #region TradeCommonActionEvents
        /// EG 20150513 [20513] TradeCommonActionEvents devient TradeActionEventsBase
        protected override TradeActionEventsBase TradeCommonActionEvents
        {
            get { return m_TradeAdminActionEvents; }
        }
        #endregion TradeCommonActionEvents
        #region TradeCommonInput
        public override TradeCommonInput TradeCommonInput
        {
            get { return m_Input; }
        }
        #endregion TradeCommonInput
        #region TradeCommonInputGUI
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_InputGUI; }
        }
        #endregion TradeCommonInputGUI
        #region TradeInput
        public TradeAdminInput TradeInput
        {
            get { return m_Input; }
        }
        #endregion TradeInput
        #endregion Accessors
        #region Events
        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // FI 20200518 [XXXXX] Utilisation de DataCache 
            //m_InputGUI = (TradeAdminInputGUI)Session[InputGUISessionID];
            //m_Input = (TradeAdminInput)Session[InputSessionID];

            m_InputGUI = DataCache.GetData<TradeAdminInputGUI>(ParentInputGUISessionID);
            m_Input = DataCache.GetData<TradeAdminInput>(ParentInputSessionID);

            PageConstruction();
        }
        #endregion
        #region Render
        protected override void Render(HtmlTextWriter writer)
        {
            m_TradeHeaderBanner.DisplayHeader(true,true);
            base.Render(writer);
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
                m_InputActionGUI.CssColor = AspTools.GetCssColorDefault(m_Input.SQLInstrument);
                m_InputActionGUI.CssMode = SessionTools.CSSMode.ToString();

            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
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
                sb.Insert(pDsEvents.DataSetName.Length + 2, "<tradeAdminActionType>" + m_TradeActionType.ToString() + "</tradeAdminActionType>");
                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(sb.ToString());
                m_TradeAdminActionEvents = (TradeAdminActionEvents)CacheSerializer.Deserialize(serializeInfo);
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //Session[TradeActionEventsSessionID] = m_TradeAdminActionEvents;
                DataCache.SetData<TradeAdminActionEvents>(TradeActionEventsSessionID, m_TradeAdminActionEvents);

                ret = Cst.ErrLevel.SUCCESS;
                #endregion Deserialize
            }
            catch (Exception) { throw; }
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
                if (null != m_TradeAdminActionEvents)
                {
                    FullConstructor fullCtor = new FullConstructor(m_Input.FpMLDataDocReader.Version);
                    ctrl.Controls.Add(m_TradeAdminActionEvents.CreateControls(m_TradeActionType, m_TradeActionMode, m_CurrentIdE, m_Input.DataDocument, fullCtor));
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
            m_TradeHeaderBanner = new TradeAdminHeaderBanner(this, GUID, CellForm, Input, m_InputGUI, false);
            m_TradeHeaderBanner.AddControls(); 
            m_TradeHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();
            AddBody();
        }
        #endregion GenerateHtmlForm
        #region GetActionEvent
        /// EG 20150513 [20513] TradeCommonActionEvent devient TradeActionEventBase
        public override TradeActionEventBase GetActionEvent(int pIdE)
        {
            return m_TradeAdminActionEvents.GetEvent(pIdE);
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

            if (TradeActionType.TradeActionTypeEnum.InvoicingCorrectionEvents == m_TradeActionType)
                return new InvoicingCorrectionGenMQueue(mQueueAttributes, pSortedAction);
            else
                return new TradeActionGenMQueue(mQueueAttributes, pSortedAction);
        }
        #endregion SetActionNew_MQueue
        #region SetTradeActionEvents
        protected override void SetTradeActionEvents()
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            m_TradeAdminActionEvents = DataCache.GetData<TradeAdminActionEvents>(TradeActionEventsSessionID);
        }
        #endregion SetTradeActionEvents
        #endregion Methods
    }
}