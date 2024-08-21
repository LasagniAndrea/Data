#region using directives
using EFS.ACommon;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.Interface;
using EFS.GUI.SimpleControls;
using EFS.TradeInformation;
using FpML.Interface;
using System;
using System.Collections;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
#endregion using directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de CustomFpMLObject.
    /// </summary>
    /// <revision>
    ///     <version>1.1.9.2</version><date>20070925</date><author>EG</author>
    ///     <EurosysSupport>N° 15774</EurosysSupport>
    ///     <comment>
    ///     Conflit en sauvegarde de trade multi-utilisateurs
    ///     Suppression de la variable statique m_FpmlDocument.
    ///     Remplace par FpmlDocReaderCustom dans InputTrade.
    ///		</comment>
    /// </revision>
    public partial class CustomFpMLObjectPage : PageBase
    {
        #region Enums
        #region ObjectOccurenceEnum
        private enum ObjectOccurenceEnum
        {
            All,
            AllExceptFirst,
            AllExceptLast,
            AllExceptFirstAndLast,
            First,
            FirstAndLast,
            Item,
            Last,
            None,
        }
        #endregion ObjectOccurenceEnum
        #endregion Enums
        #region Members
        private string m_ParentGUID;
        private TradeCommonInputGUI m_TradeCommonInputGUI;
        private TradeCommonInput m_TradeCommonInput;
        private FullConstructor m_FullCtor;
        private string m_FpMLObject;
        private string m_FpMLElement;
        private ObjectOccurenceEnum m_OccurenceEnum;
        private ObjectOccurenceEnum m_CopyToEnum;
        private int m_ObjectIndex;
        private int m_ItemOccurence;
        private int m_ItemCopyTo;
        private string m_Title;
        private string m_TitleRight;
        private bool m_IsZoomOnModeReadOnly;
        private IDocument m_cloneDocument;
        #endregion Members
        #region Accessors
        #region isSupportsPartialRendering
        /// <summary>
        /// l'utilisation de l'updatePanel (AJAX) ralentit la page 
        /// </summary>
        protected override bool IsSupportsPartialRendering
        {
            get
            {
                return false;
            }
        }
        #endregion
        #region FullConstructorSessionID
        protected string FullConstructorSessionID
        {
            get { return m_ParentGUID + "_FULLCTOR"; }
        }
        #endregion FullConstructorSessionID
        #region FpMLElement
        public string FpMLElement
        {
            get { return m_FpMLElement; }
            set { m_FpMLElement = value; }
        }
        #endregion FpMLElement
        #region FpMLObject
        public string FpMLObject
        {
            get { return m_FpMLObject; }
            set { m_FpMLObject = value; }
        }
        #endregion FpMLObject
        #region InputGUISessionID
        protected string InputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }
        #endregion InputGUISessionID
        #region InputSessionID
        protected string InputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        #endregion InputSessionID
        #region IsModeConsult
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsModeConsult
        {
            get { return Cst.Capture.IsModeConsult(m_TradeCommonInputGUI.CaptureMode) || m_IsZoomOnModeReadOnly; }
        }
        #endregion IsModeConsult
        #region IsStEnvironment_Template
        //20081128 FI [ticket 16435] IsStEnvironment_Template doit être une property public
        public bool IsStEnvironment_Template
        {
            get { return m_TradeCommonInput.TradeStatus.IsStEnvironment_Template; }
        }
        #endregion IsStEnvironment_Template
        #region IsScreenFullCapture
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsScreenFullCapture
        {
            get { return true; }
        }
        #endregion IsScreenFullCapture

        #region IsModeUpdatePostEvts
        // ****** Don't touch this property. Use by Full FpML Interface
        public bool IsModeUpdatePostEvts
        {
            get { return false; }
        }
        #endregion IsModeUpdatePostEvts
        #region IsZoomOnFull
        //FI 20091118 [16744] Add IsZoomOnFull
        /// <summary>
        /// Obtient true pour indiquer que la page en cours est pas un Zoom vers la saisie Full
        /// </summary>
        public bool IsZoomOnFull
        {
            get
            {
                return true;
            }
        }
        #endregion

        #region Title
        public new string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }
        #endregion Title
        #region TitleRight
        public string TitleRight
        {
            get { return m_TitleRight; }
            set { m_TitleRight = value; }
        }
        #endregion TitleRight
        #region TradeCommonInput
        public TradeCommonInput TradeCommonInput
        {
            set { m_TradeCommonInput = value; }
            get { return m_TradeCommonInput; }
        }
        #endregion TradeCommonInput

        #region CloneDocumentSessionID
        protected string CloneDocumentSessionID
        {
            get { return m_ParentGUID + "_CLONEDOCUMENT"; }
        }
        #endregion CloneDocumentSessionID

        #endregion Accessors
        #region Constructors
        public CustomFpMLObjectPage()
        {
        }
        #endregion Constructors
        #region Events
        #region OnInit
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);
            AbortRessource = true;
            #region QueryString
            m_ParentGUID = Request.QueryString["GUID"];
            Title = Request.QueryString["Title"];
            FpMLObject = Request.QueryString["Object"];
            try
            {
                m_ObjectIndex = 0;
                if (StrFunc.IsFilled(Request.QueryString["ObjectOccurence"]))
                    m_ObjectIndex = Convert.ToInt32(Request.QueryString["ObjectOccurence"]);
            }
            catch (FormatException) { m_ObjectIndex = 0; }
            FpMLElement = Request.QueryString["Element"];
            string occurence = Request.QueryString["Occurence"];
            string copyTo = Request.QueryString["CopyTo"];
            m_IsZoomOnModeReadOnly = Convert.ToBoolean(Request.QueryString["Readonly"]);
            #endregion QueryString

            // FI 20200518 [XXXXX] Utilisation de DataCache
            m_TradeCommonInputGUI = DataCache.GetData<TradeCommonInputGUI>(InputGUISessionID);
            m_TradeCommonInput = DataCache.GetData<TradeCommonInput>(InputSessionID);
            m_FullCtor = DataCache.GetData<FullConstructor>(FullConstructorSessionID);
            m_cloneDocument = DataCache.GetData<IDocument>(CloneDocumentSessionID);

            if (null == m_FullCtor)
            {
                m_FullCtor = new FullConstructor(m_TradeCommonInputGUI.CssMode);
                if (ArrFunc.IsEmpty(m_FullCtor.ListFpMLReference))
                {
                    m_FullCtor.LoadListFpMLReference(m_TradeCommonInput.FpMLDocReader);
                    foreach (IParty party in m_TradeCommonInput.DataDocument.Party)
                        m_FullCtor.LoadEnumObjectReference("PartyReference", string.Empty, string.Empty, party.OtcmlId, party.Id);
                }
            }
            FpMLOccurenceAndCopyToEnum(occurence, copyTo);
            //
            if (m_IsZoomOnModeReadOnly)
                TitleRight = Cst.Capture.GetLabel(Cst.Capture.ModeEnum.Consult);
            else
                TitleRight = Cst.Capture.GetLabel(m_TradeCommonInputGUI.CaptureMode);

            string title = Ressource.GetString("ComplementaryCapture");
            HtmlPageTitle htmltitleLeft = new HtmlPageTitle(title, Title);
            HtmlPageTitle htmlTitleRight = new HtmlPageTitle(TitleRight);
            GenerateHtmlForm();
            //
            FormTools.AddBanniere(this, Form, htmltitleLeft, htmlTitleRight, null, m_TradeCommonInputGUI.IdMenu);
            PageTools.BuildPage(this, Form, PageFullTitle, m_TradeCommonInputGUI.CssMode, false, null, m_TradeCommonInputGUI.IdMenu);
            //
            AddInputHiddenDeFaultControlOnEnter();

        }
        #endregion OnInit
        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            if (IsPostBack)
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                CaptureTools.CompleteCollection(this, DataCache.GetData<ControlCollection>(BuildDataCacheKey("phFpMLObject")));
                RestorePlaceHolder();
                MethodsGUI.SetEventHandler(FindControl("phFpMLObject").Controls);
            }
            else
            {
                LoadFpMLObject();
            }
            //
            DisplayButtonsAndTradeNumber();
            //
            SavePlaceHolder();
            //

        }
        #endregion OnLoad
        #region Render
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void Render(HtmlTextWriter writer)
        {
            if (null != m_TradeCommonInputGUI.CssColor)
                CSSColor = m_TradeCommonInputGUI.CssColor;
            if (null != m_TradeCommonInputGUI.CssMode)
                CSSMode = m_TradeCommonInputGUI.CssMode;
            base.Render(writer);
        }
        #endregion Render
        #region OnValidate (btnRecord - btnRemove - btnCancel)
        private void OnValidate(object sender, CommandEventArgs e)
        {
            //
            Cst.Capture.MenuValidateEnum captureMenuValidateEnum =
                (Cst.Capture.MenuValidateEnum)Enum.Parse(typeof(Cst.Capture.MenuValidateEnum), e.CommandName);
            //
            switch (captureMenuValidateEnum)
            {
                case Cst.Capture.MenuValidateEnum.Record:
                    //20091118 FI [16748] autorisation de la saisie partielle si mode Template
                    bool isValid = true;
                    //
                    if (false == (this.IsStEnvironment_Template))
                    {
                        Page.Validate();
                        isValid = Page.IsValid;
                    }
                    //
                    if (isValid)
                    {
                        SaveFpMLObject();
                        AddScriptSubmitOpenerAndSelfClose();
                        //JavaScript.SubmitOpenerAndSelfClose((PageBase)this, "tblMenu:mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                        //JavaScript.SubmitOpenerAndSelfClose((PageBase)this, "tblMenu$mnuConsult", Cst.Capture.MenuConsultEnum.SetScreen.ToString());
                    }
                    else
                    {
                        bool isModeConfirm = (null != e.CommandArgument) && ("TRUE" == e.CommandArgument.ToString());
                        //
                        if (false == isModeConfirm)
                        {
                            //JavaScript.ScriptOnStartUp(this, "javascript:__doPostBack('" + "tblMenu:btnRecord" + "','" + "TRUE" + "');", "PostBackImmediate");
                            JavaScript.ScriptOnStartUp(this, "javascript:__doPostBack('" + "tblMenu$btnRecord" + "','" + "TRUE" + "');", "PostBackImmediate");
                        }
                    }
                    break;
                case Cst.Capture.MenuValidateEnum.Annul:
                    break;
                case Cst.Capture.MenuValidateEnum.Close:
                    break;
            }
        }
        #endregion OnValidate (btnRecord - btnRemove - btnCancel)
        #endregion Events
        #region Methods
        #region AddButtonsValidate
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        private void AddButtonsValidate(WebControl pCtrlContainer)
        {
            // Enregistrer
            WCToolTipLinkButton btn = ControlsTools.GetAwesomeButtonAction("btnRecord", "fa fa-save", true);
            btn.CommandName = Cst.Capture.MenuValidateEnum.Record.ToString();
            btn.Command += new CommandEventHandler(this.OnValidate);
            if (StrFunc.IsFilled((Request.Params["__EVENTARGUMENT"])))
                btn.CommandArgument = Request.Params["__EVENTARGUMENT"];
            btn.CausesValidation = false;
            btn.Pty.TooltipContent = btn.Text + Cst.GetAccessKey(btn.AccessKey);

            AddInputHiddenDeFaultControlOnEnter();
            HiddenFieldDeFaultControlOnEnter.Value = btn.ClientID;

            pCtrlContainer.Controls.Add(btn);

            // Cancel 
            btn = ControlsTools.GetAwesomeButtonCancel(true);
            btn.CommandName = Cst.Capture.MenuValidateEnum.Annul.ToString();
            btn.Command += new CommandEventHandler(this.OnValidate);
            btn.AccessKey = "C";
            btn.Pty.TooltipContent = btn.Text + Cst.GetAccessKey(btn.AccessKey);
            pCtrlContainer.Controls.Add(btn);
        }
        #endregion AddButtonsValidate
        #region AddToolBar
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        private void AddToolBar()
        {
            Panel divAllToolBar = new Panel() { ID = "tblMenu", CssClass = this.CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };

            // Validation (Enregistrer, Annuler, Fermer)
            AddButtonsValidate(divAllToolBar);
            AddTrade(divAllToolBar);
            CellForm.Controls.Add(divAllToolBar);
        }
        #endregion AddToolBar
        #region AddTrade
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        private void AddTrade(WebControl pCtrlContainer)
        {
            Panel divTrade = new Panel() { ID = "tblMenuTrade", CssClass = this.CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            FpMLTextBox txtTrade = new FpMLTextBox(null, m_TradeCommonInput.LastIdentifier, 250, "Trade", false, "txtTrade",
                null, new Validator(), new Validator(EFSRegex.TypeRegex.RegexString, "N° trade", true))
            {
                Width = Unit.Percentage(80),
                ReadOnly = true
            };
            divTrade.Controls.Add(txtTrade);
            pCtrlContainer.Controls.Add(divTrade);
        }
        #endregion AddTrade
        #region AddValidatorSummary
        private void AddValidatorSummary()
        {
            Panel pnlSummary = new Panel
            {
                ID = "pnlSummary",
                EnableViewState = false
            };
            pnlSummary.Style[HtmlTextWriterStyle.Overflow] = "auto";

            // ValidationSummary
            ValidationSummary vsum = new ValidationSummary
            {
                ID = "ValidationSummary",
                ShowMessageBox = false,
                ShowSummary = true,
                CssClass = "ValidationSummary",
                HeaderText = Ressource.GetString("FpMLHeaderSummary")
            };
            pnlSummary.Controls.Add(vsum);
            CellForm.Controls.Add(pnlSummary);
        }
        #endregion AddValidatorSummary
        #region AddBody
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        private void AddBody()
        {
            Panel divBody = new Panel() { ID = "divbody", CssClass = this.CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName };
            PlaceHolder ph = new PlaceHolder
            {
                EnableViewState = false,
                ID = "phFpMLObject"
            };
            WCTogglePanel togglePanel = new WCTogglePanel(){CssClass = this.CSSMode + " " + m_TradeCommonInputGUI.MainMenuClassName};
            togglePanel.SetHeaderTitle(Ressource.GetString("CharacteristicsAdd"));
            togglePanel.AddContent(ph);
            divBody.Controls.Add(togglePanel);
            CellForm.Controls.Add(divBody);
        }
        #endregion AddBody
        #region CreateChildControls
        /// <summary>
        /// Restauration des scripts de fonctions javascript rattachées à des contrôles par création d'attribut (exemple: onclick)
        /// </summary>
        // EG 20210222 [XXXXX] Suppression Capture.js
        // EG 20210222 [XXXXX] Suppression Validators.js (fonctions déplacées dans PageBase.js)
        // EG 20210222 [XXXXX] Suppression FpMLCopyPaste.js (fonctions déplacées dans PageBase.js)
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));
            base.CreateChildControls();
            JavaScript.ScriptOnStartUp(this, "DisableValidators();", "DisableValidators");
        }
        #endregion CreateChildControls
        #region DisplayButtonsAndTradeNumber
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        private void DisplayButtonsAndTradeNumber()
        {
            if (FindControl("tblMenu") is WebControl ctrlContainer)
            {
                bool setDefaultOnEnter = false;

                bool isInLine = Cst.Capture.IsModeConsult(m_TradeCommonInputGUI.CaptureMode) ||
                    Cst.Capture.IsModeUpdatePostEvts(m_TradeCommonInputGUI.CaptureMode) ||
                    Cst.Capture.IsModeUpdate(m_TradeCommonInputGUI.CaptureMode) ||
                    Cst.Capture.IsModeRemoveOnlyAll(m_TradeCommonInputGUI.CaptureMode);

                if (ctrlContainer.FindControl("tblMenuTrade") is WebControl ctrl)
                {
                    if (isInLine)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }

                ctrl = ctrlContainer.FindControl("txtTrade") as WebControl;
                if (null != ctrl) ((TextBox)ctrl).Enabled = false;
                //
                ctrl = ctrlContainer.FindControl("btnRecord") as WebControl;
                // 20090729 RD Afficher le bouton "Record" pour le mode UpdatePostEvts
                isInLine = ((Cst.Capture.IsModeUpdateGen(m_TradeCommonInputGUI.CaptureMode) ||
                              Cst.Capture.IsModeNewCapture(m_TradeCommonInputGUI.CaptureMode))
                              &&
                             (false == m_IsZoomOnModeReadOnly));
                if (null != ctrl)
                {
                    if (isInLine)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                //
                ctrl = ctrlContainer.FindControl("btnCancel") as WebControl;
                if (null != ctrl)
                {
                    if (isInLine)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style[HtmlTextWriterStyle.Display] = "none";
                }
                //
                if (isInLine && (false == setDefaultOnEnter))
                {
                    setDefaultOnEnter = true;
                    ctrl = ctrlContainer.FindControl("btnRecord") as WebControl;
                    HiddenFieldDeFaultControlOnEnter.Value = ctrl.ClientID;
                    if (false == IsPostBack)
                        SetFocus(ctrl.ClientID);
                }
                //
                ctrl = ctrlContainer.FindControl("btnClose") as WebControl;
                // 20090729 RD Ne pas afficher le bouton "Close" pour le mode UpdatePostEvts
                isInLine = (Cst.Capture.IsModeConsult(m_TradeCommonInputGUI.CaptureMode) || m_IsZoomOnModeReadOnly);
                if (null != ctrl)
                {
                    if (isInLine)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ((TableCell)ctrl).Style[HtmlTextWriterStyle.Display] = "none";

                    if (isInLine && (false == setDefaultOnEnter))
                    {
                        HiddenFieldDeFaultControlOnEnter.Value = ctrl.ClientID;
                        if (false == IsPostBack)
                            SetFocus(ctrl.ClientID);
                    }
                }
            }

        }
        #endregion DisplayButtonsAndTradeNumber
        #region FpMLOccurenceAndCopyToEnum
        private void FpMLOccurenceAndCopyToEnum(string pOccurence, string pCopyTo)
        {
            #region occurence
            try
            {
                if (StrFunc.IsFilled(pOccurence))
                {
                    m_ItemOccurence = Convert.ToInt32(pOccurence);
                    m_OccurenceEnum = ObjectOccurenceEnum.Item;
                }
                else
                    m_OccurenceEnum = ObjectOccurenceEnum.All;
            }
            catch (FormatException)
            {
                if (System.Enum.IsDefined(typeof(ObjectOccurenceEnum), pOccurence))
                    m_OccurenceEnum = (ObjectOccurenceEnum)System.Enum.Parse(typeof(ObjectOccurenceEnum), pOccurence, true);
                else
                    m_OccurenceEnum = ObjectOccurenceEnum.None;
            }
            #endregion occurence
            #region copyTo
            ObjectOccurenceEnum copyToEnum;
            try
            {
                if (StrFunc.IsFilled(pCopyTo))
                {
                    m_ItemCopyTo = Convert.ToInt32(pCopyTo);
                    copyToEnum = ObjectOccurenceEnum.Item;
                }
                else
                    copyToEnum = ObjectOccurenceEnum.None;
            }
            catch (FormatException)
            {
                if (System.Enum.IsDefined(typeof(ObjectOccurenceEnum), pCopyTo))
                    copyToEnum = (ObjectOccurenceEnum)System.Enum.Parse(typeof(ObjectOccurenceEnum), pCopyTo, true);
                else
                    copyToEnum = ObjectOccurenceEnum.None;
            }
            #endregion copyTo
            #region copyTo / occurence integrity
            switch (m_OccurenceEnum)
            {
                case ObjectOccurenceEnum.Item:
                    if ((ObjectOccurenceEnum.None == copyToEnum) ||
                        (ObjectOccurenceEnum.All == copyToEnum) ||
                        (ObjectOccurenceEnum.Item == copyToEnum))
                        m_CopyToEnum = copyToEnum;
                    else
                        m_CopyToEnum = ObjectOccurenceEnum.None;
                    break;
                case ObjectOccurenceEnum.First:
                    #region All except first
                    if ((ObjectOccurenceEnum.None == copyToEnum) ||
                        (ObjectOccurenceEnum.Last == copyToEnum) ||
                        (ObjectOccurenceEnum.AllExceptFirst == copyToEnum) ||
                        (ObjectOccurenceEnum.AllExceptFirstAndLast == copyToEnum) ||
                        (ObjectOccurenceEnum.Item == copyToEnum))
                        m_CopyToEnum = copyToEnum;
                    else
                        m_CopyToEnum = ObjectOccurenceEnum.None;
                    break;
                #endregion All except first
                case ObjectOccurenceEnum.Last:
                    #region All except last
                    if ((ObjectOccurenceEnum.None == copyToEnum) ||
                        (ObjectOccurenceEnum.First == copyToEnum) ||
                        (ObjectOccurenceEnum.AllExceptLast == copyToEnum) ||
                        (ObjectOccurenceEnum.AllExceptFirstAndLast == copyToEnum))
                        m_CopyToEnum = copyToEnum;
                    else
                        m_CopyToEnum = ObjectOccurenceEnum.None;
                    break;
                #endregion All except last
                case ObjectOccurenceEnum.All:
                default:
                    m_CopyToEnum = ObjectOccurenceEnum.None;
                    break;
            }
            #endregion copyTo / occurence integrity

        }
        #endregion FpMLOccurenceAndCopyToEnum
        #region GenerateHtmlForm
        // EG 20200828 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Ensemble des écrans de saisie au format FpML
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();
            AddToolBar();
            AddValidatorSummary();
            AddBody();
        }
        #endregion GenerateHtmlForm
        #region private GetObjectWithNewId
        private object GetObjectWithNewId(object pObj, int pSuffixNumber)
        {
            object newObj = null;
            bool isNewId = false;
            if (ReflectionTools.IsExistFieldName(pObj, "efs_id"))
            {
                newObj = ReflectionTools.Clone(pObj, ReflectionTools.CloneStyle.CloneField);
                ArrayList arrList = ReflectionTools.GetObjectByName(newObj, "efs_id", false);
                for (int k = 0; k < arrList.Count; k++)
                {
                    EFS_Id id = (EFS_Id)arrList[k];
                    if (StrFunc.IsFilled(id.Value))
                    {
                        id.Value = StrFunc.PutOffSuffixNumeric(id.Value) + (pSuffixNumber).ToString();
                        isNewId = true;
                    }
                }
            }

            object ret;
            //
            if (isNewId)
                ret = newObj;
            else
                ret = pObj;
            //
            return ret;

        }
        #endregion GetObjectWithNewId
        #region GetStartEndParameters
        private void GetStartEndParameters(ObjectOccurenceEnum pOccurence, int pItemOccurence, out int pStart, ref int pEnd)
        {


            int start = 0;
            int end = pEnd;

            if (ObjectOccurenceEnum.All == pOccurence)
                start = 0;
            else if (ObjectOccurenceEnum.AllExceptFirst == pOccurence)
                start++;
            else if (ObjectOccurenceEnum.AllExceptLast == pOccurence)
                end--;
            else if (ObjectOccurenceEnum.AllExceptFirstAndLast == pOccurence)
            {
                start++;
                end--;
            }
            else if (ObjectOccurenceEnum.First == pOccurence)
                end = start + 1;
            else if (ObjectOccurenceEnum.Last == pOccurence)
                start = end - 1;
            else if (ObjectOccurenceEnum.Item == pOccurence)
            {
                start = pItemOccurence - 1;
                end = pItemOccurence;
            }
            else
                end = start;
            //
            pStart = start;
            pEnd = end;

        }
        #endregion GetStartEndParameters
        #region LoadFpMLObject
        private void LoadFpMLObject()
        {

            Control phFpMLObject = FindControl("phFpMLObject");
            if (null != phFpMLObject)
            {
                // 20070925 EG Ticket 15774
                m_cloneDocument = (IDocument)m_TradeCommonInput.CloneDocument();

                PlaceHolder plh = new PlaceHolder();
                int start;
                int end;
                ArrayList aObject;
                if (StrFunc.IsFilled(FpMLObject))
                {
                    aObject = ReflectionTools.GetObjectByName(m_cloneDocument, FpMLObject, false);
                    if ((null != aObject) && (0 < aObject.Count))
                    {
                        end = aObject.Count;
                        object obj = aObject[m_ObjectIndex];
                        GetStartEndParameters(m_OccurenceEnum, m_ItemOccurence, out start, ref end);
                        FieldInfo fld;
                        for (int i = start; i < end; i++)
                        {
                            Type tObject = obj.GetType();
                            if (tObject.IsArray)
                                obj = ((Array)obj).GetValue(m_ItemOccurence - 1);

                            fld = obj.GetType().GetField(FpMLElement);
                            if (null != fld)
                            {
                                SetFldVisible(fld, true);
                                m_FullCtor.Display(obj, fld, ref plh, null, null, true, true, true);
                                phFpMLObject.Controls.Add(plh);
                            }
                        }
                    }
                }
                else
                {
                    aObject = ReflectionTools.GetObjectByName(m_cloneDocument, FpMLElement, false);
                    if ((null != aObject) && (0 < aObject.Count))
                    {
                        end = aObject.Count;
                        GetStartEndParameters(m_OccurenceEnum, m_ItemOccurence, out start, ref end);
                        for (int i = start; i < end; i++)
                        {
                            m_FullCtor.Read(aObject[i], ref plh, null, null);
                            phFpMLObject.Controls.Add(plh);
                        }
                    }
                }
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //Session[CloneDocumentSessionID] = m_cloneDocument;
                DataCache.SetData<IDocument>(CloneDocumentSessionID, m_cloneDocument);
            }
        }
        #endregion LoadFpMLObject
        #region RestorePlaceHolder
        private void RestorePlaceHolder()
        {
            Control ctrl = FindControl("phFpMLObject");
            if (null != ctrl)
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                ControlCollection controlcollection = DataCache.GetData<ControlCollection>(BuildDataCacheKey("phFpMLObject"));
                if (null != controlcollection)
                {

                    try
                    {
                        while (0 != controlcollection.Count)
                            ctrl.Controls.Add(controlcollection[0]);
                    }
                    catch (Exception ex)
                    {
                        ctrl.Controls.AddAt(0, new LiteralControl(System.Environment.NewLine + ex.Message.ToString()));
                    }
                }
            }
        }
        #endregion RestorePlaceholder
        #region SaveFpMLObject
        private void SaveFpMLObject()
        {

            if (ObjectOccurenceEnum.None != m_CopyToEnum)
            {
                int start;
                int end;
                ArrayList aObject;
                object objSource;
                if (StrFunc.IsFilled(FpMLObject))
                {
                    // 20070925 EG Ticket 15774
                    aObject = ReflectionTools.GetObjectByName(m_cloneDocument, FpMLObject, false);
                    if ((null != aObject) && (0 < aObject.Count))
                    {
                        objSource = aObject[m_ObjectIndex];
                        Type tObject = objSource.GetType();
                        if (tObject.IsArray)
                        {
                            int nbLength = ((Array)objSource).Length;
                            if (ObjectOccurenceEnum.Item == m_OccurenceEnum)
                                objSource = ((Array)objSource).GetValue(m_ItemOccurence - 1);
                            else if (ObjectOccurenceEnum.First == m_OccurenceEnum)
                                objSource = ((Array)objSource).GetValue(0);
                            else if (ObjectOccurenceEnum.Last == m_OccurenceEnum)
                                objSource = ((Array)objSource).GetValue(nbLength - 1);
                            else
                                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                    "Incorrect CopyTo ObjectOccurenceEnum {0}", m_CopyToEnum.ToString());
                        }
                        //
                        end = aObject.Count;
                        GetStartEndParameters(m_CopyToEnum, m_ItemCopyTo, out start, ref end);
                        //
                        FieldInfo fld = objSource.GetType().GetField(FpMLElement);
                        object obj = fld.GetValue(objSource);
                        //
                        FieldInfo fldSpecified = objSource.GetType().GetField(FpMLElement + Cst.FpML_SerializeKeySpecified);
                        object objSpecified = null;
                        if (null != fldSpecified)
                            objSpecified = fldSpecified.GetValue(objSource);
                        //
                        for (int i = start; i < end; i++)
                        {
                            // Generation de Id Unique
                            obj = GetObjectWithNewId(obj, i + 1);
                            //
                            fld = aObject[i].GetType().GetField(FpMLElement);
                            fld.SetValue(aObject[i], obj);
                            //
                            fldSpecified = aObject[i].GetType().GetField(FpMLElement + Cst.FpML_SerializeKeySpecified);
                            if (null != fldSpecified)
                                fldSpecified.SetValue(aObject[i], objSpecified);
                        }
                    }
                }
                else
                {
                    aObject = ReflectionTools.GetObjectByName(m_cloneDocument, FpMLElement, false);
                    if ((null != aObject) && (0 < aObject.Count))
                    {

                        if (ObjectOccurenceEnum.Item == m_OccurenceEnum)
                            objSource = aObject[m_ItemOccurence - 1];
                        else if (ObjectOccurenceEnum.First == m_OccurenceEnum)
                            objSource = aObject[0];
                        else if (ObjectOccurenceEnum.Last == m_OccurenceEnum)
                            objSource = aObject[aObject.Count - 1];
                        else
                            throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                "Incorrect CopyTo ObjectOccurenceEnum {0}", m_CopyToEnum.ToString());

                        end = aObject.Count;
                        GetStartEndParameters(m_CopyToEnum, m_ItemCopyTo, out start, ref end);
                        for (int i = start; i < end; i++)
                        {
                            // Generation de Id Unique
                            aObject[i] = GetObjectWithNewId(objSource, i + 1);
                        }

                    }

                }
            }
            // 20070925 EG Ticket 15774
            m_TradeCommonInput.FpMLDataDocReader = (IDataDocument)m_cloneDocument;
            //Cette étape repositionne les pointeurs sur le dataDocument dans les cciContainers
            m_TradeCommonInput.CustomCaptureInfos.InitializeCciContainer();

        }
        #endregion SaveFpMLObject
        #region SavePlaceHolder
        private void SavePlaceHolder()
        {
            Control ctrl = FindControl("phFpMLObject");
            if ((null != ctrl) && (0 < ctrl.Controls.Count))
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //Session[GUID] = ctrl.Controls;
                DataCache.SetData<ControlCollection>(BuildDataCacheKey("phFpMLObject"), ctrl.Controls);
            }
        }
        #endregion SavePlaceholder
        #region SetFldVisible
        private void SetFldVisible(FieldInfo pFieldInfo, bool pIsVisible)
        {

            if (pFieldInfo.IsDefined(typeof(ArrayDivGUI), true))
            {
                object[] attributes = pFieldInfo.GetCustomAttributes(typeof(ArrayDivGUI), false);
                ArrayDivGUI arrayDivGUI = (ArrayDivGUI)attributes[0];
                arrayDivGUI.IsChildVisible = pIsVisible;
            }
            else if (pFieldInfo.IsDefined(typeof(OpenDivGUI), true))
            {
                object[] attributes = pFieldInfo.GetCustomAttributes(typeof(OpenDivGUI), false);
                OpenDivGUI openDivGUI = (OpenDivGUI)attributes[0];
                openDivGUI.IsVisible = pIsVisible;
            }

        }
        #endregion SetFldVisible

        #region AddScriptSubmitOpenerAndSelfClose
        private void AddScriptSubmitOpenerAndSelfClose()
        {
            JavaScript.JSStringBuilder sb = new JavaScript.JSStringBuilder();
            sb.AppendLine("window.opener.PostBack('" + "tblMenu$mnuConsult" + "','" + Cst.Capture.MenuConsultEnum.SetScreen.ToString() + "');");
            sb.AppendLine("	self.close();");
            this.RegisterScript("SubmitOpenerAndSelfClose", sb.ToString());
        }
        #endregion
        #endregion Methods
    }
}
