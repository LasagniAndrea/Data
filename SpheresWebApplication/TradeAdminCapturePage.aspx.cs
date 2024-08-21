#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.TradeInformation;
using EfsML.Enum.Tools;
using EfsML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
//
//20071212 FI Ticket 16012 => Migration Asp2.0

#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TradeAdminCapturePage.
    /// </summary>
    public partial class TradeAdminCapturePage : TradeCommonCapturePage
    {
        #region Members
        private TradeAdminCaptureGen m_CaptureGen;
        private TradeAdminInputGUI m_InputGUI;
        private TradeAdminHeaderBanner m_TradeHeaderBanner;
        #endregion Members
        #region Accessors
        #region IsExistPartialUnallocatedAmount
        protected bool IsExistPartialUnallocatedAmount
        {
            get
            {
                bool isExistPartialUnallocatedAmount = false;
                if (TradeCommonInput.Product.IsInvoiceSettlement)
                {
                    IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)TradeCommonInput.Product.Product;
                    decimal unallocatedAmount = invoiceSettlement.UnallocatedAmount.Amount.DecValue;
                    decimal settlementAmount = invoiceSettlement.SettlementAmount.Amount.DecValue;
                    isExistPartialUnallocatedAmount = (0 < unallocatedAmount) && (settlementAmount != unallocatedAmount);
                }
                return isExistPartialUnallocatedAmount;
            }
        }
        #endregion IsExistUnallocatedAmount
        #region IsExistTotalUnallocatedAmount
        protected bool IsExistTotalUnallocatedAmount
        {
            get
            {
                bool isExistTotalUnallocatedAmount = false;
                if (TradeCommonInput.Product.IsInvoiceSettlement)
                {
                    IInvoiceSettlement invoiceSettlement = (IInvoiceSettlement)TradeCommonInput.Product.Product;
                    decimal unallocatedAmount = invoiceSettlement.UnallocatedAmount.Amount.DecValue;
                    decimal settlementAmount = invoiceSettlement.SettlementAmount.Amount.DecValue;
                    isExistTotalUnallocatedAmount = (unallocatedAmount == settlementAmount);
                }
                return isExistTotalUnallocatedAmount;
            }
        }
        #endregion IsExistTotalUnallocatedAmount
        #region IsMenuModifyAllocatedInvoiceAuthorised
        protected bool IsMenuModifyAllocatedInvoiceAuthorised
        {
            get
            {
                bool isMenuModifyAllocatedInvoiceAuthorised = InputGUI.IsModifyAuthorised &&
                                                              TradeCommonInput.IsTradeFound &&
                                                              (false == TradeCommonInput.IsTradeRemoved);
                if (isMenuModifyAllocatedInvoiceAuthorised)
                    isMenuModifyAllocatedInvoiceAuthorised = IsExistPartialUnallocatedAmount || IsExistTotalUnallocatedAmount;

                return isMenuModifyAllocatedInvoiceAuthorised;
            }
        }
        #endregion IsMenuModifyAllocatedInvoiceAuthorised
        #region IsMenuModifyWithGenEvtAuthorised
        protected bool IsMenuModifyWithGenEvtAuthorised
        {
            get
            {
                bool isMenuModifyWithGenEvtAuthorised = InputGUI.IsModifyAuthorised &&
                                                        TradeCommonInput.IsTradeFound &&
                                                        (false == TradeCommonInput.IsTradeRemoved);
                if (isMenuModifyWithGenEvtAuthorised)
                    isMenuModifyWithGenEvtAuthorised = IsExistTotalUnallocatedAmount;

                return isMenuModifyWithGenEvtAuthorised;
            }
        }
        #endregion IsMenuModifyWithGenEvtAuthorised
        #region IsMenuModifyWithoutGenEvtAuthorised
        protected bool IsMenuModifyWithoutGenEvtAuthorised
        {
            get
            {
                bool isMenuModifyWithoutGenEvtAuthorised = InputGUI.IsModifyPostEvtsAuthorised &&
                                                           TradeCommonInput.IsTradeFound &&
                                                           (false == TradeCommonInput.IsTradeRemoved); ;
                return isMenuModifyWithoutGenEvtAuthorised;
            }
        }
        #endregion IsMenuModifyWithoutGenEvtAuthorised
        #region IsModeUpdateLocked
        public override bool IsModeUpdateLocked
        {
            get { return Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode); }
        }
        #endregion IsModeUpdateLocked
        #region Object
        protected override ICustomCaptureInfos Object
        {
            get { return m_CaptureGen.Input; }
        }
        #endregion Object
        #region ResAdminTrade
        // EG 20091208
        protected string ResAdminTrade
        {
            get
            {
                string ressource = "Invoice";
                if (null != TradeCommonInput.Product)
                {
                    if (TradeCommonInput.Product.ProductBase.IsAdditionalInvoice)
                        ressource = "AdditionalInvoice";
                    else if (TradeCommonInput.Product.ProductBase.IsCreditNote)
                        ressource = "CreditNote";
                    else if (TradeCommonInput.Product.ProductBase.IsInvoiceSettlement)
                        ressource = "InvoiceSettlement";
                }
                return ressource;
            }
        }
        #endregion ResAdminTrade

        
        // FI 20140930 [XXXXX] Mise en commentaire
        //protected override Cst.SQLCookieGrpElement ProductGrpElement
        //{
        //    get { return Cst.SQLCookieGrpElement.SelADMProduct; }
        //}
        

        #region TradeCommonCaptureGen
        public override TradeCommonCaptureGen TradeCommonCaptureGen
        {
            get { return (TradeCommonCaptureGen)m_CaptureGen; }
        }
        #endregion TradeCommonCaptureGen
        #region TradeCommonHeaderBanner
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return (TradeCommonHeaderBanner)m_TradeHeaderBanner; }
        }
        #endregion TradeCommonHeaderBanner
        #region TradeCommonInput
        public override TradeCommonInput TradeCommonInput
        {
            get { return m_CaptureGen.TradeCommonInput; }
        }
        #endregion TradeCommonInput
        #region TradeCommonInputGUI
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_InputGUI; }
        }
        #endregion TradeCommonInputGUI
        #region TradeAdminInput
        public TradeAdminInput TradeAdminInput
        {
            get { return m_CaptureGen.Input; }
        }
        #endregion TradeAdminInput
        #region TradeKey
        protected override CapturePageBase.TradeKeyEnum TradeKey
        {
            get
            {
                return TradeKeyEnum.TradeAdmin;
            }
        }
        #endregion
        #region XML_FilesPath
        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS]
        protected override string XML_FilesPath
        {
            //get { return @"XML_Files\CustomTradeAdmin"; }
            get { return @"CCIML\CustomTradeAdmin"; }
        }
        #endregion XML_FilesPath
        // EG 20230526 [WI640] Gestion des parties PAYER/RECEIVER sur facturation (BENEFICIARY/PAYER)
        protected override bool IsDynamicId
        {
            get
            {
                return true;
            }
        }
        #region protected override objectContext
        /// <summary>
        /// Obtient le context [utilisé pour la recherche des objets de design]
        /// </summary>
        protected override string ObjectContext
        {
            get
            {
                string ret = string.Empty;
                //
                ArrayList al = new ArrayList();
                if (StrFunc.IsFilled(base.ObjectContext))
                    al.Add(base.ObjectContext);
                //
                if (ArrFunc.IsFilled(al))
                    ret = StrFunc.StringArrayList.StringArrayToStringList((String[])al.ToArray(typeof(String)), false);
                //   
                return ret;
            }
        }
        #endregion
        #endregion Accessors

        #region Events
        #region OnInit
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            m_CaptureGen = new TradeAdminCaptureGen();
            m_CaptureGen.Input.InitDefault(m_DefaultParty, m_DefaultCurrency, m_DefaultBusinessCenter);
        }
        #endregion OnInit
        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            if (false == IsPostBack)
            {
                string idMenu = Request.QueryString["IDMenu"];
                if (StrFunc.IsEmpty(idMenu))
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeAdmin);

                //Request.ServerVariables["APPL_PHYSICAL_PATH"]
                m_InputGUI = new TradeAdminInputGUI(idMenu, SessionTools.User, XML_FilesPath);
                m_InputGUI.InitializeFromMenu(SessionTools.CS); 
            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //m_InputGUI = (TradeAdminInputGUI)Session[InputGUISessionID];
                //m_FullCtor = (FullConstructor)Session[FullConstructorSessionID];
                //m_CaptureGen.Input = (TradeAdminInput)Session[InputSessionID];

                m_InputGUI = DataCache.GetData<TradeAdminInputGUI>(InputGUISessionID);
                m_FullCtor = DataCache.GetData<FullConstructor>(FullConstructorSessionID);
                m_CaptureGen.Input = DataCache.GetData<TradeAdminInput>(InputSessionID);
            }
            //
            base.OnLoad(e);
        }
        #endregion OnLoad
        #region OnPreRender
        // EG 20200918 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et Compléments
        protected override void OnPreRender(EventArgs e)
        {

            string eventTarget = Request.Params["__EVENTTARGET"];
            ((TextBox)CtrlTrade).Attributes["oldvalue"] = TradeIdentifier;
            //
            if (IsPostBack)
            {
                if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                {
                    if ((null != TradeAdminInput.CustomCaptureInfos) && (Cst.Capture.IsModeInput(InputGUI.CaptureMode)))
                    {
                        // FI 20200402 [XXXXX] Add test (isLoadCcisFromGUI && (false == IsDblPostbackFromAutocompleteControl)) comme sur toutes les pages qui héritent de TradeCommonCapturePage
                        if (IsLoadCcisFromGUI && (false == IsDblPostbackFromAutocompleteControl))
                        {
                            //Mise a jour From GUI
                            TradeAdminInput.CustomCaptureInfos.Initialize_FromGUI(this);
                            //Mise a jour  Document Fmpl
                            TradeAdminInput.CustomCaptureInfos.Dump_ToDocument(0);

                            // 20090107 EG Laisser entre Dump_ToDocument(0) et IsToSynchronizeWithDocument
                            if ("ExecFunction" == eventTarget)
                                ExecFunction();
                        }

                        //Mise à jour de l'IHM
                        if (TradeAdminInput.CustomCaptureInfos.IsToSynchronizeWithDocument)
                        {
                            //Refaire toutes les étapes pour pouvoir afficher les nouveautés du Document Fpml
                            TradeAdminInput.CustomCaptureInfos.IsToSynchronizeWithDocument = false;
                            TradeAdminInput.CustomCaptureInfos.CciTradeAdmin.SetPartyInOrder();
                            UpdatePlaceHolder();
                        }
                        else
                        {
                            TradeAdminInput.CustomCaptureInfos.Dump_ToGUI(this);
                        }
                        // 20091207 EG 
                        if ("ExecFunction" == eventTarget)
                        {
                            ExecFunctionAfterSynchronize();
                            TradeAdminInput.CustomCaptureInfos.Dump_ToGUI(this);
                        }

                    }
                    //Bouton Add Or Delete Item
                    if ("OnAddOrDeleteItem" == eventTarget)
                        OnAddOrDeleteItem();
                    //
                    if (Cst.OTCml_ScreenBox == eventTarget)
                        OnZoomScreenBox();
                }
            }
            //Bouton zoom sur un section de l'écran Full (ie: Settlement Instruction)
            if (Cst.FpML_ScreenFullCapture == eventTarget)
            {
                TradeAdminInput.CustomCaptureInfos.CciTradeAdmin.SetPartyInOrder();
                OnZoomFpml();
            }
            //
            if (TradeAdminInput.IsTradeFound && (Cst.Capture.IsModeNewCapture(m_InputGUI.CaptureMode) ||
                                                 Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode)))
            {
                #region Initialisation du status en fonction des actors et de leurs rôles
                // Initialisation réalisée ici sur le PreRender à cause de la saisie light
                ActorRoleCollection actorRoleNew = TradeCommonInput.DataDocument.GetActorRole(CSTools.SetCacheOn(SessionTools.CS));
                if ((null == m_InputGUI.ActorRole) || m_InputGUI.ActorRole.CompareTo(actorRoleNew) != 0)
                {
                    m_InputGUI.ActorRole = actorRoleNew;
                    //Reinit des default user status (Check/Match) from ACTORROLE et ACTIONTUNING
                    TradeAdminInput.ResetStUser();
                    TradeAdminInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS), null);
                    TradeAdminInput.InitStUserFromTuning(m_InputGUI.ActionTuning);
                }
                #endregion
                // RD 20091228 [16809] Confirmation indicators for each party
            }

            // FI 20200518 [XXXXX] Utilisation des properties Session
            //Session[InputGUISessionID] = m_InputGUI;
            //Session[InputSessionID] = TradeAdminInput;
            //Session[FullConstructorSessionID] = m_FullCtor;
            DataCache.SetData(InputGUISessionID, m_InputGUI);
            DataCache.SetData(InputSessionID, TradeAdminInput);
            DataCache.SetData(FullConstructorSessionID, m_FullCtor);

            SetValidatorEnabled(false);

            base.OnPreRender(e);
            // EG 20091130 SetToolTip to buttons
            SetToolTipButton();

            if (TradeAdminInput.IsTradeFound) // && (TradeAdminInput.Product.IsInvoice || TradeAdminInput.Product.IsAdditionalInvoice))
                SetClosedStatus();

            // A faire absolument apres base.OnPreRender 
            #region PageSubTitle
            string subTitle = TitleLeft;
            if (StrFunc.IsFilled(SubTitleLeft))
                subTitle += " - " + SubTitleLeft;
            if ("0" != TradeIdentifier)
                subTitle += " - " + TradeIdentifier;
            PageTitle = subTitle;
            #endregion PageSubTitle

        }
        #endregion OnPreRender
        #region OnValidate (btnRecord)
        // EG 20171115 Upd CaptureSessionInfo
        // EG 20210623 Lecture des arguments de validation du postBack associés aux boutons OK/CANCEL de la fenêtre de validation :
        // - OK = TRUE ou CheckValidationRule;TRUE
        // - CANCEL = FALSE ou CheckValidationRule;FALSE
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            // FI 20210622 [XXXXX] Add isValidateOk pour prendre le choix de l'utilsateur lorsqu'une demande de confirmation est demandée (ConfirmInputTrade) 
            //bool isValidateOk = (null == e.CommandArgument) || Convert.ToString(e.CommandArgument).Contains("TRUE");

            // FI 20240125 [WI826] ajout de PostBackForValidationArg
            bool isValidateOk = (null == e.CommandArgument) || Convert.ToString(e.CommandArgument).Contains("TRUE") || Convert.ToString(e.CommandArgument) == PostBackForValidationArg;

            if (isValidateOk)
            {
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                {
                    Control ctrl = ctrlContainer.FindControl("mnuMode");
                    Cst.Capture.MenuValidateEnum captureMenuValidateEnum =
                        (Cst.Capture.MenuValidateEnum)System.Enum.Parse(typeof(Cst.Capture.MenuValidateEnum), e.CommandName);

                switch (captureMenuValidateEnum)
                {
                    case Cst.Capture.MenuValidateEnum.Close:
                        ClosePage();
                        break;
                    case Cst.Capture.MenuValidateEnum.Record:
                        #region Recording TradeAdmin
                        string target = "tblMenu$btnRecord";
                        string CheckValidationRuleStep = "CheckValidationRule";
                            string eventTarget = Request.Params["__EVENTTARGET"];
                        string eventStep = string.Empty;
                        string eventArgument = string.Empty;

                        if (null != e.CommandArgument)
                        {
                            string[] commandArguments = ((string)e.CommandArgument.ToString()).Split(';');
                            // EG 20210623 Correction alimentation  de eventStep et eventArgument
                            if (ArrFunc.IsFilled(commandArguments))
                            {
                                if (commandArguments.Length > 1)
                                    eventStep = commandArguments[0];
                                eventArgument = commandArguments[commandArguments.Length - 1];
                            }
                        }

                        // On identifie s'il s'agit d'une réponse de l'utilisateur et le type de réponse de l'utilisateur (TRUE/FALSE),
                        bool isModeConfirm_CheckValidationRule_Ok = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("TRUE" == eventArgument);
                        bool isModeConfirm_CheckValidationRule_Cancel = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("FALSE" == eventArgument);
                        bool isModeConfirm_CheckValidationRule = isModeConfirm_CheckValidationRule_Ok || isModeConfirm_CheckValidationRule_Cancel;
                        //bool isModeConfirm_CheckValidationRule = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("TRUE" == eventArgument);
                        bool isOk = true;
                        
                        if ((isOk) && (false == m_CaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)))
                        isOk = ValidatePage();

                       
                        if (isOk)
                        {
                            if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                            {
                                TradeAdminInput.CustomCaptureInfos.SaveCapture(Page);
                                TradeAdminInput.CustomCaptureInfos.CciTradeAdmin.SetPartyInOrder();
                            }
                        }

                            if (isOk)
                            {
                                //20100311 PL-StatusBusiness
                                if ((false == TradeCommonCaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)) &&
                                    (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType))
                                {
                                    TradeAdminInput.CustomCaptureInfos.FinaliseAll();
                                    string errMsg = TradeAdminInput.CustomCaptureInfos.GetErrorMessage();
                                    isOk = StrFunc.IsEmpty(errMsg);
                                    if (false == isOk)
                                    {
                                        ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                                        MsgForAlertImmediate = Ressource.GetString("Msg_ErrorsInForm") + Cst.CrLf + Cst.CrLf + errMsg;
                                    }
                                }
                            }
                            // Validation Rules : Existe-il des Erreurs ??
                            if (isOk)
                            {
                                if (false == m_CaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode) &&
                                   (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)))
                                {
                                    CheckTradeAdminValidationRule check = new CheckTradeAdminValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode);
                                    isOk = check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckTradeAdminValidationRule.CheckModeEnum.Error);
                                    if (false == isOk)
                                    {
                                        ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                                        MsgForAlertImmediate = Ressource.GetString("Msg_ValidationRuleError") + Cst.CrLf + Cst.CrLf + Cst.CrLf + check.GetConformityMsg();
                                    }
                                }
                            }
                            // Validation Rules : Existe-il des warnings ??
                            if (isOk)
                            {
                                if (m_CaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode) &&
                                   (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)))
                                {
                                    if (false == isModeConfirm_CheckValidationRule)
                                    {
                                        CheckTradeAdminValidationRule check = new CheckTradeAdminValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode);
                                        check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckTradeAdminValidationRule.CheckModeEnum.Warning);
                                        string msgValidationrules = check.GetConformityMsg();
                                        isOk = (false == StrFunc.IsFilled(msgValidationrules));
                                        if (false == isOk)
                                        {
                                            string msgConfirm = string.Empty;
                                            msgConfirm += Ressource.GetString("Msg_ValidationRuleWarning");
                                            msgConfirm += Cst.CrLf + Cst.CrLf + Cst.CrLf + msgValidationrules + Cst.CrLf;
                                            msgConfirm += GetRessourceConfirmTrade();
                                            string data = (Cst.Capture.IsModeNewOrDuplicate(m_InputGUI.CaptureMode) ? string.Empty : this.TradeIdentifier);
                                            msgConfirm = msgConfirm.Replace("{0}", data);
                                            JavaScript.ConfirmStartUpImmediate(this, msgConfirm, ProcessStateTools.StatusWarning, target, CheckValidationRuleStep + ";TRUE", CheckValidationRuleStep + ";FALSE");
                                        }
                                    }
                                }
                            }

                            if (isOk)
                            {
                                // RD 20091228 [16809] Confirmation indicators for each party                             
                                TradeAdminCaptureGen.ErrorLevel lRet = RecordCapture();
                                if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))// FI 20140805 [XXXXX] Utilisation de TradeCommonCaptureGen.IsRecordInSuccess
                                {
                                    OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                                }
                                else
                                {
                                    // En cas d'erreur
                                    // En mode creation ou duplication, on efface pas l'écran (= On conserve les données avant enregistrement) 
                                    // En mode Modification, on efface pas l'écran sauf si Pb de ROWVERSION
                                    if ((false == Cst.Capture.IsModeNewOrDuplicate(m_InputGUI.CaptureMode)) && (lRet == TradeAdminCaptureGen.ErrorLevel.ROWVERSION_ERROR))
                                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                                }
                            }
                            #endregion Recording Trade
                            break;
                        case Cst.Capture.MenuValidateEnum.Annul:
                            #region Cancelling CaptureTrade
                            if (null != ctrl)
                            {
                                string command;
                                if (Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeDuplicate(m_InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeRemove(m_InputGUI.CaptureMode))
                                    command = Cst.Capture.ModeEnum.Consult.ToString();
                                else
                                    command = Cst.Capture.ModeEnum.New.ToString();
                                //
                                OnMode(ctrl, new skmMenu.MenuItemClickEventArgs(command));
                            }
                            #endregion Cancelling CaptureTrade
                            break;
                    }
                }
            }
        }
        #endregion OnValidate (btnRecord)
        #endregion Events
        #region Methods
        

        #region AddButtonsFooter
        /// <summary>
        /// Ajoute dans la cellule les boutons XML,EXPORT,CONFIRM,POST,LOG
        /// </summary>
        // EG 20160119 Refactoring Footer
        protected override void AddButtonsFooter(WebControl pCtrl)
        {
            try
            {
                //UserWithLimitedRights
                bool isSessionGuest = SessionTools.User.IsSessionGuest;

                if (!isSessionGuest)
                {
                    pCtrl.Controls.Add(GetButtonFooter_XML());
                    pCtrl.Controls.Add(GetButtonFooter_EXPORT());
                }
                pCtrl.Controls.Add(GetButtonFooter_CONFIRM());
                pCtrl.Controls.Add(GetButtonFooter_POST());
                if (!isSessionGuest)
                {
                    pCtrl.Controls.Add(GetButtonFooter_LOG());
                }
            }
            catch (Exception) { throw; }
        }
        #endregion AddButtonsFooter

        #region AddHeader
        protected override void AddHeader()
        {
            m_TradeHeaderBanner = new TradeAdminHeaderBanner(this, GUID, CellForm, TradeAdminInput, m_InputGUI,
                (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)));
            m_TradeHeaderBanner.AddControls();

        }
        #endregion AddHeader

        #region ExecFunction
        public override void ExecFunction()
        {
            string arg = Request.Params["__EVENTARGUMENT"];
            if ((null != arg) && m_CaptureGen.Input.CustomCaptureInfos.Contains(arg))
            {
                CustomCaptureInfo cci = m_CaptureGen.Input.CustomCaptureInfos[arg];
                m_CaptureGen.Input.CustomCaptureInfos.CciTradeAdmin.ProcessExecute(cci);
            }
        }
        #endregion ExecFunction
        #region ExecFunctionAfterSynchronize
        // EG 20091207 New
        public override void ExecFunctionAfterSynchronize()
        {
            
            string arg = Request.Params["__EVENTARGUMENT"];
            if ((null != arg) && m_CaptureGen.Input.CustomCaptureInfos.Contains(arg))
            {
                CustomCaptureInfo cci = m_CaptureGen.Input.CustomCaptureInfos[arg];
                m_CaptureGen.Input.CustomCaptureInfos.CciTradeAdmin.ProcessExecuteAfterSynchronize(cci);
            }

        }
        #endregion ExecFunctionAfterSynchronize

        #region GetAllocatedAmountInvoiceDates
        // EG 20110205 Test DBNULL
        //EG 20120613 Add Parameters
        private decimal GetAllocatedAmountInvoiceDates()
        {
            decimal allocatedAmount = 0;
            IDataReader dr = null;
            DataParameters dbParam = new DataParameters();
            dbParam.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), TradeCommonInput.IdT);    
            dbParam.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.EVENTCLASS), EventClassFunc.Settlement);    
            dbParam.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.EVENTTYPE), EventTypeFunc.NetTurnOverIssueAmount);    
            dbParam.Add(new DataParameter(SessionTools.CS, "IDSTACTIVATION", DbType.AnsiString, SQLCst.UT_STATUS_LEN), Cst.StatusActivation.REGULAR.ToString());
            try
            {
                string SQLSelect = SQLCst.SELECT + "SUM(e.VALORISATION)" + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.EVENT + " e" + Cst.CrLf;
                SQLSelect += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.EVENTCLASS + " ec" + Cst.CrLf;
                SQLSelect += SQLCst.ON + "(ec.IDE = e.IDE)" + SQLCst.AND + "(ec.EVENTCLASS = @EVENTCLASS)" + Cst.CrLf;
                SQLSelect += SQLCst.WHERE + "(e.EVENTTYPE = @EVENTTYPE)" + Cst.CrLf;
                SQLSelect += SQLCst.AND + "(e.IDT = @IDT)" + Cst.CrLf;
                SQLSelect += SQLCst.AND + "(e.IDSTACTIVATION = @IDSTACTIVATION)" + Cst.CrLf;
                QueryParameters qryParameters = new QueryParameters(SessionTools.CS, SQLSelect, dbParam);
                dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter() );
                if (dr.Read())
                {
                    if (false == Convert.IsDBNull(dr.GetValue(0)))
                        allocatedAmount = Convert.ToDecimal(dr.GetValue(0));
                }
                return allocatedAmount;
            }
            finally
            {
                if (null != dr)
                {
                    // EG 20160404 Migration vs2013
                    //dr.Close();
                    dr.Dispose();
                }
            }
        }
        #endregion GetEventMnuTradeAction
        #region GetDisplayKey
        // EG 20110308 HPC Nb ligne de frais sur facture
        protected override string GetDisplayKey(DisplayKeyEnum pKey)
        {
            string ret;
            switch (pKey)
            {
                case DisplayKeyEnum.DisplayKey_InvoiceFee:
                    ret = TradeAdminInput.DisplayInvoiceFee(NumberRowByPage);
                    break;
                default:
                    ret = string.Empty;
                    break;
            }
            return ret;
        }
        protected override string GetDisplayKey(DisplayKeyEnum pKey, int pItemOccurs)
        {
            string ret;
            switch (pKey)
            {
                case DisplayKeyEnum.DisplayKey_InvoiceTrade:
                    ret = TradeAdminInput.DisplayInvoiceTradeBanner(SessionTools.CS, pItemOccurs);
                    break;
                default:
                    ret = string.Empty;
                    break;
            }
            return ret;
        }
        #endregion GetDisplayKey

        #region GetMenuDuplicate
        //EG 20120613 BlockUI New
        // EG 2020902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override skmMenu.MenuItemParent GetMenuDuplicate()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnDuplication",
                aToolTip = Ressource.GetString("Duplication"),
                eImageUrl = "fas fa-icon fa-copy",
                Enabled = InputGUI.IsCreateAuthorised && TradeCommonInput.IsTradeFound && IsExistTotalUnallocatedAmount
            };
            if (ret.Enabled)
            {
                ret.eCommandName = Cst.Capture.ModeEnum.Duplicate.ToString();
                ret.eBlockUIMessage = ret.aToolTip + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += " " + Ressource.GetString("NotAllowed");
            }
            return ret;
        }
        #endregion GetMenuDuplicate
        #region GetMenuModify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override skmMenu.MenuItemParent GetMenuModify()
        {

            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);

            bool isOk = (InputGUI.IsModifyAuthorised || InputGUI.IsModifyPostEvtsAuthorised);
            isOk = isOk && TradeCommonInput.IsTradeFound;
            isOk = isOk && (false == TradeCommonInput.IsTradeRemoved);

            if (isOk)
            {
                if (TradeCommonInput.Product.IsInvoiceSettlement)
                {
                    ret = new skmMenu.MenuItemParent(3);
                    GetMenuModifyWithGenEvt(ret);
                    GetMenuModifyWithoutGenEvt(ret);
                    GetMenuModifyAllocatedInvoice(ret);
                }
                else
                {
                    ret = new skmMenu.MenuItemParent(2);
                    GetMenuModifyWithGenEvt(ret);
                    GetMenuModifyWithoutGenEvt(ret);
                }
            }

            ret.aID = "btnModification";
            ret.aToolTip = Ressource.GetString("Modification");
            ret.Enabled = isOk;
            ret.eImageUrl = "fas fa-icon fa-edit";
            if (false == isOk)
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += Ressource.GetString("NotAllowed");
            }
            return ret;

        }
        #endregion GetMenuModify
        #region GetMenuModifyWithGenEvt
        //EG 20120613 BlockUI New
        private void GetMenuModifyWithGenEvt(skmMenu.MenuItemParent pMnu)
        {
            skmMenu.MenuItemParent mnuchild = pMnu[0];
            mnuchild.eText = Ressource.GetString("ModificationGenExpiries");
            mnuchild.Enabled = IsMenuModifyWithGenEvtAuthorised;
            if (IsMenuModifyWithGenEvtAuthorised)
            {
                mnuchild.eCommandName = Cst.Capture.ModeEnum.Update.ToString();
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }

        }
        #endregion GetMenuModify
        #region GetMenuModifyWithoutGenEvt
        //EG 20120613 BlockUI New
        private void GetMenuModifyWithoutGenEvt(skmMenu.MenuItemParent pMnu)
        {
            skmMenu.MenuItemParent mnuchild = pMnu[1];
            mnuchild.eText = Ressource.GetString("ModificationNoGenExpiries");
            mnuchild.Enabled = IsMenuModifyWithoutGenEvtAuthorised;
            if (IsMenuModifyWithoutGenEvtAuthorised)
            {
                mnuchild.eCommandName = Cst.Capture.ModeEnum.UpdatePostEvts.ToString();
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }

        }
        #endregion GetMenuModifyWithoutGenEvt
        #region GetMenuModifyAllocatedInvoice
        //EG 20120613 BlockUI New
        private void GetMenuModifyAllocatedInvoice(skmMenu.MenuItemParent pMnu)
        {
            skmMenu.MenuItemParent mnuchild = pMnu[2];
            mnuchild.eText = Ressource.GetString("ModificationAllocatedInvoice");
            mnuchild.Enabled = IsMenuModifyAllocatedInvoiceAuthorised;
            if (IsMenuModifyAllocatedInvoiceAuthorised)
            {
                mnuchild.eCommandName = Cst.Capture.ModeEnum.UpdateAllocatedInvoice.ToString();
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }

        }
        #endregion GetMenuModifyAllocatedInvoice

        #region GetMenuAuditAction
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override skmMenu.MenuItemParent GetMenuAuditAction()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eText = "TRD.ACT",
                eImageUrl = "fas fa-icon fa-external-link-alt",
                aToolTip = Ressource.GetString("QTip_TradeAdminAuditAction"),
                eUrl = @"OpenTradeAuditAction(" + TradeCommonInput.IdT + ",'" + Ressource.GetString("Trade") + ": " + TradeCommonInput.Identifier + "'); return false;"
            };
            return ret;
        }
        #endregion GetMenuAuditAction

        /// <summary>
        /// <para>Vérifie si l'action a un sens ds ce cas pIsHidden = false</para>
        /// <para>Vérifie si l'action est possible pIsEnabled = true</para>
        /// <para>Si l'action est impossible, retourne la raison  </para>
        /// </summary>
        /// <param name="pMenuInput"></param>
        /// <param name="pIsHidden"></param>
        /// <param name="pIsEnabled"></param>
        /// <param name="pDisabledReason"></param> 
        /// EG 20091126 Test NTA présent
        /// EG 20110401 Add Test sur existence des EVTS pour Annulation
        /// FI 20130308[] add parameter pDisabledReason (non géré ici pour l'instant)
        protected override void IsActionAuthorized(string pMenuInput, out bool pIsHidden, out bool pIsEnabled, out string pDisabledReason)
        {
            pDisabledReason = null;

            bool isHidden = false;
            bool isEnabled = true;
            if ((TradeAdminInput.IsTradeFound) && IdMenu.IsMenuTradeAdminInput(pMenuInput))
            {
                Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(pMenuInput);

                switch (menu)
                {
                    case IdMenu.Menu.InputTradeAdmin_RMV: // Suppression de trade administratif (Facture/ Avoir / Règlement)
                        isEnabled = TradeCommonInput.Product.IsInvoiceSettlement ||
                                    (false == TradeAdminInput.IsAllocatedInvoiceDates) ||
                                    (0 == TradeAdminInput.AllocatedAmountInvoiceDates) ||
                                    TradeCommonInput.IsEventsGenerated;
                        isHidden = false;
                        break;
                    case IdMenu.Menu.InputTradeAdmin_COR:  //Correction de facture
                        // EG 20100311 Remove Test (false == TradeAdminInput.IsAllocatedInvoiceDates)
                        //isEnabled = TradeAdminInput.IsStEnvironment_Regular && (false == TradeAdminInput.IsAllocatedInvoiceDates);
                        isEnabled = TradeAdminInput.TradeStatus.IsStEnvironment_Regular && TradeCommonInput.Product.IsInvoice;
                        // EG 20091126 Test NTA présent
                        if (isEnabled)
                        {
                            IInvoice invoice = (IInvoice)TradeCommonInput.Product.Product;
                            isEnabled = invoice.NetTurnOverAccountingAmountSpecified && (0 < invoice.NetTurnOverAccountingAmount.Amount.DecValue);
                        }
                        isHidden = (false == TradeCommonInput.Product.IsInvoice);
                        break;
                    default:
                        isHidden = true;
                        break;
                }
                //
                isEnabled = isEnabled && (false == TradeAdminInput.IsTradeRemoved) && TradeAdminInput.TradeStatus.IsCurrentStUsedBy_Regular;
            }
            pIsHidden = isHidden;
            pIsEnabled = isEnabled;
        }
        
        #region IsControlModeConsult
        /// <summary>
        ///  Obtient true si le contrôle est uniquement consultatble
        /// </summary>
        /// <param name="pCo"></param>
        /// <returns></returns>
        protected override bool IsControlModeConsult(CustomObject pCo)
        {
            bool ret = base.IsControlModeConsult(pCo);
            if (false == ret)
            {
                if (Cst.Capture.IsModeRemoveOnly(InputGUI.CaptureMode))
                    ret = (false == pCo.ClientId.StartsWith(TradeCommonCustomCaptureInfos.CCst.Prefix_tradeAdminRemove));

                if (false == ret)
                {
                    if (Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode))
                        ret = pCo.IsLockedCreate;
                    else if (Cst.Capture.IsModeUpdate(InputGUI.CaptureMode))
                        ret = pCo.IsLockedModify;
                    else if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                        ret = pCo.IsLockedModifyPostEvts;
                    else if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
                        ret = pCo.IsLockedAllocatedInvoice;
                }
            }
            return ret;
        }
        #endregion IsControlModeConsult


        #region SetInputSessionID
        public override void SetInputSession()
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[InputSessionID] = TradeAdminInput;
            DataCache.SetData(InputSessionID, TradeAdminInput);
        }
        #endregion SetInputSessionID

        #region SetClosedStatus
        // EG 20200921 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections
        private void SetClosedStatus()
        {
            if ((this.FindControl(m_TradeHeaderBanner.ctrlDspProcess + (m_TradeHeaderBanner.NbDisplayProcess - 1)) is WCTooltipLabel ctrl) && 
                (false == Cst.Capture.IsModeNewCapture(TradeCommonInputGUI.CaptureMode)))
            {
                if (TradeAdminInput.Product.IsInvoiceSettlement || TradeAdminInput.Product.IsCreditNote)
                {
                    ctrl.Visible = false;
                }
                else
                {
                    TradeAdminInput.AllocatedAmountInvoiceDates = GetAllocatedAmountInvoiceDates();
                    TradeAdminInput.IsAllocatedInvoiceDates = (0 < TradeAdminInput.AllocatedAmountInvoiceDates);

                    string data = "Allocated";

                    if (TradeAdminInput.IsAllocatedInvoiceDates || TradeAdminInput.IsInvoiceClosed)
                    {
                        if (TradeAdminInput.IsInvoiceClosed)
                        {
                            ctrl.Style.Add(HtmlTextWriterStyle.BackgroundColor, CstCSSColor.green);
                            ctrl.Pty.TooltipContent = Ressource.GetString(data + "Green_TradeState", string.Empty);
                        }
                        else if (TradeAdminInput.IsAllocatedInvoiceDates)
                        {
                            ctrl.Style.Add(HtmlTextWriterStyle.BackgroundColor, CstCSSColor.blue);
                            ctrl.Pty.TooltipContent = Ressource.GetString(data + "Blue_TradeState", string.Empty);
                        }
                        ctrl.ForeColor = Color.White;
                        ctrl.Font.Bold = true;
                        ctrl.Style.Add("border-radius", "0px");
                        ctrl.Style.Add(HtmlTextWriterStyle.PaddingLeft, "5px");
                        ctrl.Style.Add(HtmlTextWriterStyle.PaddingRight, "5px");
                    }
                    else
                    {
                        ctrl.ForeColor = Color.Black;
                        ctrl.Style.Add(HtmlTextWriterStyle.BackgroundColor, Color.Transparent.ToString());
                        ctrl.Pty.TooltipContent = Ressource.GetString(data + "Black_TradeState", string.Empty);
                    }
                    ctrl.Visible = true;
                    ctrl.Text = data;
                    ctrl.Pty.TooltipTitle = data;
                }
            }
        }
        #endregion SetClosedStatus

        #region SetToolTipButton
        private void SetToolTipButton()
        {
            if (FindControl("btnCustomConfirm") is WCToolTipImageButton imgButton)
                imgButton.Pty.TooltipContent = Ressource.GetString("btn" + ResAdminTrade + "ConfirmToolTip") + Cst.GetAccessKey(imgButton.AccessKey);
            imgButton = FindControl("btnExport") as WCToolTipImageButton;
            if (null != imgButton)
                imgButton.Pty.TooltipContent = Ressource.GetString("btn" + ResAdminTrade + "XMLEventToolTip") + Cst.GetAccessKey(imgButton.AccessKey);
        }
        #endregion SetToolTipButton

        #region GetMQueue
        protected override MQueueBase GetMQueue()
        {
            MQueueBase mQueue;
            if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
            {
                MQueueAttributes mQueueAttributes = new MQueueAttributes()
                {
                    connectionString = SessionTools.CS,
                    id = TradeCommonInput.IdT,
                    identifier = TradeCommonInput.Identifier,
                    idInfo = GetMQueueIdInfo()
                };

                mQueue = new InvoicingAllocationGenMQueue(mQueueAttributes);
            }
            else
                mQueue = base.GetMQueue();
            //
            return mQueue;
        }
        #endregion


        #region UpdatePlaceHolder
        protected override void UpdatePlaceHolder()
        {
            base.UpdatePlaceHolder();
        }
        #endregion UpdatePlaceHolder

        /// <summary>
        /// Sur les Invoice etc => pas de Report 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarReport(Panel pPnlParent)
        {

        }

        /// <summary>
        /// Retourne un menu vide
        /// <para>Il n'y a pas de menu CashFlow sur les trade ADMIN</para>
        /// <para>La consultation 3btns Trade\CASHFLOWS.xml ne traite que les trades de marché</para>
        /// <para>A faire évoluer si la consultation gère les trades ADMINs</para>
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuCashFlow()
        {
            return new skmMenu.MenuItemParent(0);
        }



        #endregion Methods
    }
}

