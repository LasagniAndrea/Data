#region using
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI.CCI;
using EFS.GUI.SimpleControls;
using EFS.TradeInformation;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

#endregion using

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de SubTradeEvents.
    /// </summary>
    public partial class SubTradeEvents : CapturePageBase
    {
        #region Members
        private string m_ParentGUID;
        private EventCaptureGen m_EventCaptureGen;
        //private string m_IdMenu;
        private EventInputGUI m_InputGUI;
        private EventInput m_Input;
        private string m_ScreenName;
        private string m_ScreenParent;
        private string m_Title;
        // FI 20200518 [XXXXX] n'est plus utlisé
        //private string m_DocInnerXmlClone;
        #endregion Members

        #region Accessors
        #region CtrlEvent
        private Control CtrlEvent
        {
            get
            {
                Control ret = null;
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                    ret = ctrlContainer.FindControl("txtEvent");
                return ret;
            }
        }
        #endregion CtrlEvent
        
        #region Input
        public EventInput Input
        {
            get { return m_Input; }
        }
        #endregion Input
        #region InputGUI
        protected override InputGUI InputGUI
        {
            get { return m_InputGUI; }
        }
        #endregion InputGUI
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] rename
        protected  string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] rename
        protected  string ParentInputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        
        

        #region NamePlaceHolder
        protected override string NamePlaceHolder
        {
            get { return "phSubEvent"; }
        }
        #endregion NamePlaceHolder
        #region Object
        protected override ICustomCaptureInfos Object
        {
            get { return m_EventCaptureGen.Input; }
        }
        #endregion Object
        #region ScreenName
        protected override string ScreenName
        {
            get { return m_ScreenName; }
        }
        #endregion ScreenName
        #region SubTitleLeft
        protected override string SubTitleLeft
        {
            get { return Ressource.GetString(m_ScreenParent); }
        }
        #endregion SubTitleLeft
        #region Title
        public new string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }
        #endregion Title

        #endregion Accessors

        #region Constructors
        public SubTradeEvents() { }
        #endregion Constructors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            m_ParentGUID = Request.QueryString["GUID"];
            m_ScreenName = Request.QueryString["Screen"];
            m_ScreenParent = Request.QueryString["ScreenParent"];
            base.OnInit(e);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200903 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override void OnLoad(EventArgs e)
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //m_InputGUI = (EventInputGUI)Session[InputGUISessionID];
            //m_Input = (EventInput)Session[InputSessionID];
            m_InputGUI = DataCache.GetData<EventInputGUI>(ParentInputGUISessionID); 
            m_Input =  DataCache.GetData<EventInput>(ParentInputSessionID);


            // EG 201110523 Bidouille de merde pour retrouver le GUID qui donne le TradeCommonInput
            // du trade courant
            // FI 20200518 [XXXXX] La bidouille d'Eric ne suffit pas lors d'un post de la page => Alimentation d'une varaible Session
            TradeCommonInput tradeCommonInput;
            if (false == IsPostBack)
            {
                tradeCommonInput = DataCache.GetData<TradeCommonInput>(GUIDReferrer + "_Input");
                DataCache.SetData(BuildDataCacheKey("TradeInput"), tradeCommonInput);
            }
            else
                tradeCommonInput = DataCache.GetData<TradeCommonInput>(BuildDataCacheKey("TradeInput"));

            m_EventCaptureGen = new EventCaptureGen(tradeCommonInput)
            {
                Input = m_Input
            };

            if (m_EventCaptureGen.Input.SQLProduct.IsAdministrativeProduct)
                m_InputGUI.MainMenuClassName = "invoicing";
            PageConstruction();

            if (false == IsPostBack)
            {
                m_EventCaptureGen.Input.CloneCurrentEvent();
                bool existCustomScreen = LoadSubScreen();
                if (existCustomScreen)
                {
                    Input.CustomCaptureInfos.LoadDocument(InputGUI.CaptureMode, (CciPageBase)this);
                    // FI 20200518 [XXXXX] Utilisation de DataCache
                    // m_DocInnerXmlClone = InputGUI.DocXML.InnerXml;
                    DataCache.SetData(BuildDataCacheKey("DocInnerXml"), InputGUI.DocXML.InnerXml);
                }
            }
            else
            {
                RestorePlaceHolder();
            }

            SavePlaceHolder();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            string eventTarget = Request.Params["__EVENTTARGET"];
            if (IsPostBack)
            {
                if (null != Input.CustomCaptureInfos && (Cst.Capture.IsModeInput(InputGUI.CaptureMode)))
                    Input.CustomCaptureInfos.UpdCaptureAndDisplay(this);

                //Bouton Add Or Delete Item
                if ("OnAddOrDeleteItem" == eventTarget)
                    OnAddOrDeleteItem();

            }
            base.OnPreRender(e);

        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            //
            Cst.Capture.MenuValidateEnum captureMenuValidateEnum =
                (Cst.Capture.MenuValidateEnum)Enum.Parse(typeof(Cst.Capture.MenuValidateEnum), e.CommandName);
            //
            switch (captureMenuValidateEnum)
            {
                case Cst.Capture.MenuValidateEnum.Record:
                    Page.Validate();
                    if (Page.IsValid)
                    {
                        //JavaScript.SubmitOpenerAndSelfClose((PageBase)this, "tblMenu:mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                        JavaScript.SubmitOpenerAndSelfClose((PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                    }

                    break;
                case Cst.Capture.MenuValidateEnum.Annul:
                    Input.EventDocReader.@event[Input.CurrentEventIndex] = Input.CurrentEventClone;
                    InputGUI.DocXML.InnerXml = DataCache.GetData<String>(BuildDataCacheKey("DocInnerXml"));
                    break;
            }
            Input.CurrentEventClone = null;
        }
        
        #endregion Events

        #region Methods
        /// <summary>
        /// 
        /// </summary>
        protected override void AddToolBarFooter()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20161124 [22634] Modify
        /// EG 20200826 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        protected override void AddToolBarHeader()
        {
            Panel divAllToolBar = new Panel() { ID = "tblMenu", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };

            // Validation (Enregistrer, Annuler, Fermer)
            AddButtonsValidate(divAllToolBar);
            AddTradeEvent(divAllToolBar);
            CellForm.Controls.Add(divAllToolBar);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTr"></param>
        /// EG 20200826 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc)
        private void AddTradeEvent(WebControl pCtrlContainer)
        {
            Panel divTradeEvent = new Panel() { ID = "tblMenuTrade", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };
            FpMLTextBox txtTrade = new FpMLTextBox(null, Input.TradeIdentifier, 80, "Trade", false, "txtTrade",
                null, new Validator(), new Validator(EFSRegex.TypeRegex.RegexString, "N° trade", true))
            {
                ReadOnly = true
            };
            divTradeEvent.Controls.Add(txtTrade);
            FpMLTextBox txtEvent = new FpMLTextBox(null, Input.CurrentEventIdentifier, 80, "Event", false, "txtEvent",
                null, new Validator(), new Validator(EFSRegex.TypeRegex.RegexString, "N° event", true))
            {
                ReadOnly = true
            };
            divTradeEvent.Controls.Add(txtEvent);
            pCtrlContainer.Controls.Add(divTradeEvent);
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200903 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        protected override void PageConstruction()
        {
            string idMenu = Request.QueryString["IDMenu"];
            
            HtmlPageTitle titleLeft = new HtmlPageTitle(TitleLeft, SubTitleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle(TitleRight);
            PageTitle = Ressource.GetString(idMenu);
            GenerateHtmlForm();

            FormTools.AddBanniere(this, Form, titleLeft, titleRight, idMenu, m_EventCaptureGen.TradeCommonInput.Product.IsADM? "invoicing":"input");
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null, InputGUI.IdMenu);
            if (m_EventCaptureGen.TradeCommonInput.Product.IsADM)
                PageTools.SetHeaderLinkIcon(this, IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin));


            ToolBarBinding();
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <returns></returns>
        protected override string GetDisplayKey(DisplayKeyEnum pKey)
        {
            string ret;
            switch (pKey)
            {
                case DisplayKeyEnum.DisplayKey_Event:
                    ret = Ressource.GetString(ScreenName);
                    break;
                default:
                    ret = string.Empty;
                    break;
            }
            return ret;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pParentClientId"></param>
        /// <param name="pParentOccurs"></param>
        /// <returns></returns>
        protected override int InitializeNodeObjectArray(CustomObject pCo, XmlNode pNode, string pParentClientId, int pParentOccurs)
        {
            bool isArrayManaged = (EventCustomCaptureInfos.CCst.Prefix_eventProcess == pCo.ClientId) && ArrFunc.IsFilled(Input.CurrentEvent.process);
            isArrayManaged = isArrayManaged || (EventCustomCaptureInfos.CCst.Prefix_eventChild == pCo.ClientId) && ArrFunc.IsFilled(Input.CurrentEventChilds);
            isArrayManaged = isArrayManaged || (EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingIRD == pCo.ClientId) && ArrFunc.IsFilled(Input.CurrentEvent.pricing2);
            //
            int occurs = int.Parse(XMLTools.GetNodeAttribute(pNode, "occurs"));
            //
            if (isArrayManaged)
            {
                int docOccurs = 0;
                if (EventCustomCaptureInfos.CCst.Prefix_eventProcess == pCo.ClientId)
                    docOccurs = ArrFunc.Count(Input.CurrentEvent.process);
                else if (EventCustomCaptureInfos.CCst.Prefix_eventChild == pCo.ClientId)
                    docOccurs = ArrFunc.Count(Input.CurrentEventChilds);
                else if (EventCustomCaptureInfos.CCst.Prefix_eventDet_pricingIRD == pCo.ClientId)
                    docOccurs = ArrFunc.Count(Input.CurrentEvent.pricing2);

                if ("OnAddOrDeleteItem" != Request.Params["__EVENTTARGET"])
                    occurs = System.Math.Max(docOccurs, occurs);
                XMLTools.SetNodeAttribute(pNode, "addsuboperator", Convert.ToString(false == IsModeConsult));
            }
            //
            return occurs;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RestorePlaceHolder()
        {
            Control ctrl = PlaceHolder;
            if (null != ctrl)
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                ControlCollection controlcollection = DataCache.GetData<ControlCollection>(BuildDataCacheKey("phl"));
                if (null != controlcollection)
                {
                    try
                    {
                        while (0 != controlcollection.Count)
                        {
                            ctrl.Controls.Add(controlcollection[0]);
                        }
                    }
                    catch (Exception ex)
                    {
                        ctrl.Controls.AddAt(0, new LiteralControl(System.Environment.NewLine + ex.Message.ToString()));
                    }
                }
            }
            // FI 20200518 [XXXXX] Mise en commentaire
            //m_DocInnerXmlClone = (string)Session[DocInnerXmlGUID];
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void SavePlaceHolder()
        {
            // FI 20200518 [XXXXX] Utilisation DataCache
            Control ctrl = PlaceHolder;
            if ((null != ctrl) && (0 < ctrl.Controls.Count))
                DataCache.SetData<ControlCollection>(BuildDataCacheKey("phl"), ctrl.Controls);
            
            // FI 20200518 [XXXXX] Mis en commentaire
            //Session[DocInnerXmlGUID] = m_DocInnerXmlClone;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void SetToolBarsStyle()
        {
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                #region Validate
                #region BtnRecord && BtnCancel
                bool isVisible;
                if (ctrlContainer.FindControl("BtnRecord") is WebControl record)
                {
                    isVisible = (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode)) ||
                        (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode));
                    bool isRecordVisible = isVisible && Input.IsEventFilled;
                    if (isRecordVisible)
                        ControlsTools.RemoveStyleDisplay(record);
                    else
                        record.Style[HtmlTextWriterStyle.Display] = "none";

                    if (isRecordVisible)
                        record.Enabled = SessionTools.License.IsLicProductAuthorised_Add(Input.SQLProduct.Identifier);
                }
                if (ctrlContainer.FindControl("BtnCancel") is WebControl cancel)
                {
                    isVisible = (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode)) ||
                                (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode));
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(cancel);
                    else
                        cancel.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion BtnRecord && BtnCancel
                #endregion Validate

                #region Consult
                if (ctrlContainer.FindControl("divtrade") is WebControl div)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) ||
                                Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(div);
                    else
                        div.Style[HtmlTextWriterStyle.Display] = "none";
                }

                if (CtrlTrade is FpMLTextBox trade)
                    trade.IsLocked = true;

                if (ctrlContainer.FindControl("divevent") is WebControl divEvent)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode) ||
                                Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(divEvent);
                    else
                        divEvent.Style[HtmlTextWriterStyle.Display] = "none";
                }

                if (CtrlEvent is FpMLTextBox @event)
                    @event.IsLocked = true;

                if (ctrlContainer.FindControl("tbrConsult") is WebControl tbrConsult)
                {
                    isVisible = Cst.Capture.IsModeConsult(InputGUI.CaptureMode);
                    if (isVisible)
                        ControlsTools.RemoveStyleDisplay(tbrConsult);
                    else
                        tbrConsult.Style[HtmlTextWriterStyle.Display] = "none";
                }
                #endregion Consult
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTr"></param>
        protected override void AddToolBarReport(Panel pPnlParent)
        {
            //pas de toolbar Report
        }
        #endregion Methods
    }
}
