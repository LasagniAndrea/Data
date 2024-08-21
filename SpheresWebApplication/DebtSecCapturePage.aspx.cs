#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.Import;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
using System;
using System.Collections;
using System.Data;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives




namespace EFS.Spheres
{
    public partial class DebtSecCapturePage : TradeCommonCapturePage
    {

        #region Members
        private DebtSecCaptureGen m_CaptureGen;
        private DebtSecInputGUI m_InputGUI;
        private DebtSecHeaderBanner m_DebtSecHeaderBanner; // peut-être à supprimer (ou à utiliser uniquement côté evènements)
        #endregion Members

        #region accessor
        /// <summary>
        /// 
        /// </summary>
        protected override ICustomCaptureInfos Object
        {
            get { return m_CaptureGen.Input; }
        }

        // FI 20140930 [XXXXX] Mise en commentaire
        //protected override Cst.SQLCookieGrpElement ProductGrpElement
        //{
        //    get { return Cst.SQLCookieGrpElement.SelDebtSecProduct; }
        //}

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonCaptureGen TradeCommonCaptureGen
        {
            get { return (TradeCommonCaptureGen)m_CaptureGen; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return (TradeCommonHeaderBanner)m_DebtSecHeaderBanner; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get { return DebSectInput; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_InputGUI; }
        }

        /// <summary>
        /// 
        /// </summary>
        public DebtSecInput DebSectInput
        {
            get { return m_CaptureGen.Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override CapturePageBase.TradeKeyEnum TradeKey
        {
            get
            {
                return TradeKeyEnum.DebtSecurity;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS]
        protected override string XML_FilesPath
        {
            //get { return @"XML_Files\CustomTrade"; }
            get { return @"CCIML\CustomTrade"; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override int TextBoxConsultWith
        {
            get
            {
                return 300;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsNewModeForArray
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsDynamicId
        {
            get
            {
                return true;
            }
        }

        #endregion

        #region events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            m_CaptureGen = new DebtSecCaptureGen();
            m_CaptureGen.Input.InitDefault(m_DefaultParty, m_DefaultCurrency, m_DefaultBusinessCenter);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {

            if (false == IsPostBack)
            {
                //si le menu n'est pas passé dans lURL, utilisation du menu par défaut 
                string idMenu = Request.QueryString["IDMenu"];
                if (StrFunc.IsEmpty(idMenu))
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputDebtSec);
                //
                //Request.ServerVariables["APPL_PHYSICAL_PATH"]
                m_InputGUI = new DebtSecInputGUI(idMenu, SessionTools.User, XML_FilesPath);
                m_InputGUI.InitializeFromMenu(CSTools.SetCacheOn(SessionTools.CS));
            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //m_InputGUI = (DebtSecInputGUI)Session[InputGUISessionID];
                //m_FullCtor = (FullConstructor)Session[FullConstructorSessionID];
                //m_CaptureGen.Input = (DebtSecInput)Session[InputSessionID];

                m_InputGUI = DataCache.GetData<DebtSecInputGUI>(InputGUISessionID);
                m_CaptureGen.Input = DataCache.GetData<DebtSecInput>(InputSessionID);
                m_FullCtor = DataCache.GetData<FullConstructor>(FullConstructorSessionID);

            }
            base.OnLoad(e);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {

            string eventTarget = Request.Params["__EVENTTARGET"];
            ((TextBox)CtrlTrade).Attributes["oldvalue"] = TradeIdentifier;
            //
            if (IsPostBack)
            {
                if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                {
                    if ((null != DebSectInput.CustomCaptureInfos) && (Cst.Capture.IsModeInput(m_InputGUI.CaptureMode)))
                    {
                        // Mise a jour From GUI
                        // FI 20100427 [16970] add test sur isLoadCcisFromGUI
                        // FI 20200402 [XXXXX] Add test (false == IsDblPostbackFromAutocompleteControl)
                        if (IsLoadCcisFromGUI && (false == IsDblPostbackFromAutocompleteControl))
                        {
                            DebSectInput.CustomCaptureInfos.Initialize_FromGUI(this);
                            //
                            //Mise a jour Document Fmpl
                            DebSectInput.CustomCaptureInfos.Dump_ToDocument(0);
                            //
                            // 20090107 EG Laisser entre Dump_ToDocument(0) et IsToSynchronizeWithDocument
                            if ("ExecFunction" == eventTarget)
                                ExecFunction();
                        }
                        //Mise à jour de l'IHM
                        //FI 20100427 [16970] alimentation de isLastPostUpdatePlaceHolder afin de détecter un éventuel douple post
                        IsLastPostUpdatePlaceHolder = DebSectInput.CustomCaptureInfos.IsToSynchronizeWithDocument;
                        if (DebSectInput.CustomCaptureInfos.IsToSynchronizeWithDocument)
                        {
                            //Refaire toutes les étapes pour pouvoir afficher les nouveautés du Document Fpml
                            DebSectInput.CustomCaptureInfos.IsToSynchronizeWithDocument = false;
                            DebSectInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                            UpdatePlaceHolder();
                        }
                        else
                        {
                            DebSectInput.CustomCaptureInfos.Dump_ToGUI(this);
                        }
                    }
                    //
                    //Bouton Add Or Delete Item
                    if ("OnAddOrDeleteItem" == eventTarget)
                    {
                        DebSectInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnAddOrDeleteItem();
                    }
                    //
                    if (Cst.OTCml_ScreenBox == eventTarget)
                    {
                        DebSectInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnZoomScreenBox();
                    }
                }
            }
            //Bouton zoom sur un section de l'écran Full (ie: Settlement Instruction)
            if (Cst.FpML_ScreenFullCapture == eventTarget)
            {
                DebSectInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                OnZoomFpml();
            }
            //
            if (DebSectInput.IsTradeFound && (Cst.Capture.IsModeNewOrDuplicateOrRemove(m_InputGUI.CaptureMode) ||
                                              Cst.Capture.IsModeUpdateGen(m_InputGUI.CaptureMode)))
            {
                #region Initialisation du status en fonction des actors et de leurs rôles
                // Initialisation réalisée ici sur le PreRender à cause de la saisie light
                ActorRoleCollection actorRoleNew = TradeCommonInput.DataDocument.GetActorRole(CSTools.SetCacheOn(SessionTools.CS));
                if ((null == m_InputGUI.ActorRole) || m_InputGUI.ActorRole.CompareTo(actorRoleNew) != 0)
                {
                    m_InputGUI.ActorRole = actorRoleNew;
                    //Reinit des default user status (Check/Match) from ACTORROLE et ACTIONTUNING
                    DebSectInput.ResetStUser();
                    DebSectInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS), null);
                    DebSectInput.InitStUserFromTuning(m_InputGUI.ActionTuning);
                }
                #endregion
                // RD 20091228 [16809] Confirmation indicators for each party
            }

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[InputGUISessionID] = m_InputGUI;
            //Session[InputSessionID] = DebSectInput;
            //Session[FullConstructorSessionID] = m_FullCtor;

            DataCache.SetData(InputGUISessionID, m_InputGUI);
            DataCache.SetData(InputSessionID, DebSectInput);
            DataCache.SetData(FullConstructorSessionID, m_FullCtor);

            SetValidatorEnabled(false);

            base.OnPreRender(e);

            // A faire absolument apres base.OnPreRender 
            #region PageSubTitle
            string subTitle = TitleLeft;
            if (StrFunc.IsFilled(SubTitleLeft))
                subTitle += " - " + SubTitleLeft;
            if ("0" != TradeIdentifier)
                subTitle += " - " + TradeIdentifier;
            PageTitle = subTitle;
            #endregion PageSubTitle
            //
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20171115 Upd CaptureSessionInfo
        // EG 20210623 Lecture des arguments de validation du postBack associés aux boutons OK/CANCEL de la fenêtre de validation :
        // - OK = TRUE ou CheckValidationRule;TRUE
        // - CANCEL = FALSE ou CheckValidationRule;FALSE
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            AddAuditTimeStep("Start DebtSecCapturePage.OnValidate");

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
                            #region Recording Trade
                            string target = "tblMenu$btnRecord";
                            string CheckValidationRuleStep = "CheckValidationRule";
                            string eventTarget = Request.Params["__EVENTTARGET"];
                            string eventStep = string.Empty;
                            string eventArgument = string.Empty;
                            //
                            if (null != e.CommandArgument)
                            {
                                //                                
                                string[] commandArguments = ((string)e.CommandArgument.ToString()).Split(';');
                                // EG 20210623 Correction alimentation  de eventStep et eventArgument
                                if (ArrFunc.IsFilled(commandArguments))
                                {
                                    if (commandArguments.Length > 1)
                                        eventStep = commandArguments[0];
                                    eventArgument = commandArguments[commandArguments.Length - 1];
                                }
                            }

                            bool isModeConfirm_CheckValidationRule_Ok = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("TRUE" == eventArgument);
                            bool isModeConfirm_CheckValidationRule_Cancel = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("FALSE" == eventArgument);
                            bool isModeConfirm_CheckValidationRule = isModeConfirm_CheckValidationRule_Ok || isModeConfirm_CheckValidationRule_Cancel;

                            bool isOk = true;

                            CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
                            {
                                user = SessionTools.User,
                                session = SessionTools.AppSession,
                                licence = SessionTools.License
                            };


                            // 20100311 PL-StatusBusiness
                            if (false == TradeCommonCaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode))
                                isOk = ValidatePage();

                            if (isOk)
                                // Pas de contrôle pour double postback sur Saisie full 
                                isOk = IsScreenFullCapture || (false == PostBackForValidation());

                            if (isOk)
                            {
                                if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                                {
                                    TradeCommonInput.CustomCaptureInfos.SaveCapture(Page);
                                    TradeCommonInput.CustomCaptureInfos.CciTradeCommon.SetPartyInOrder();
                                }
                            }

                            if (isOk)
                            {
                                if ((false == m_CaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode)) &&
                                    (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType))
                                {
                                    DebSectInput.CustomCaptureInfos.FinaliseAll();
                                    string errMsg = DebSectInput.CustomCaptureInfos.GetErrorMessage();
                                    isOk = StrFunc.IsEmpty(errMsg);
                                    if (false == isOk)
                                    {
                                        ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                                        MsgForAlertImmediate = Ressource.GetString("Msg_ErrorsInForm") + Cst.CrLf + Cst.CrLf + errMsg;
                                    }
                                }
                            }
                            // Validation Rules : Existe-il des Erreurs ??
                            isOk = isOk && (false == isModeConfirm_CheckValidationRule_Cancel);

                            if (isOk)
                            {
                                if ((false == m_CaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)) &&
                                    (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)))
                                {
                                    CheckDebtSecValidationRule check = new CheckDebtSecValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode, captureSessionInfo.user);
                                    isOk = check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckDebtSecValidationRule.CheckModeEnum.Error);
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
                                if ((false == TradeCommonCaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode))
                                    && (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)))
                                {
                                    if (false == isModeConfirm_CheckValidationRule)
                                    {
                                        CheckDebtSecValidationRule check = new CheckDebtSecValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode, captureSessionInfo.user);
                                        check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckDebtSecValidationRule.CheckModeEnum.Warning);
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
                                TradeCaptureGen.ErrorLevel lRet = RecordCapture();
                                if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))// FI 20140805 [XXXXX] Utilisation de TradeCommonCaptureGen.IsRecordInSuccess
                                {
                                    OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                                }
                                else
                                {
                                    // En cas d'erreur
                                    // En mode creation ou duplication, on efface pas l'écran (= On conserve les données avant enregistrement) 
                                    // En mode Modification, on efface pas l'écran sauf si Pb de ROWVERSION
                                    if ((false == Cst.Capture.IsModeNewOrDuplicate(m_InputGUI.CaptureMode)) && (lRet == TradeCaptureGen.ErrorLevel.ROWVERSION_ERROR))
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
                                //
                                if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(m_InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeDuplicate(m_InputGUI.CaptureMode) ||
                                    Cst.Capture.IsModeRemove(m_InputGUI.CaptureMode))
                                    command = Cst.Capture.ModeEnum.Consult.ToString();
                                else
                                    command = Cst.Capture.ModeEnum.New.ToString();

                                OnMode(ctrl, new skmMenu.MenuItemClickEventArgs(command));
                            }
                            #endregion Cancelling CaptureTrade
                            break;
                    }
                }
            }

            AddAuditTimeStep("End DebtSecCapturePage.OnValidate");
        }
        #endregion

        #region Methodes
        /// <summary>
        /// 
        /// </summary>
        protected override void AddHeader()
        {
            m_DebtSecHeaderBanner = new DebtSecHeaderBanner(this, GUID, CellForm, DebSectInput, m_InputGUI,
                (false == Cst.Capture.IsModeRemoveOnly(m_InputGUI.CaptureMode)));
            m_DebtSecHeaderBanner.AddControls();
        }
        
        /// <summary>
        /// Ajoute dans la cellule les boutons XML,EXPORT,POST,LOG
        /// </summary>
        // EG 20160119 Refactoring Footer
        protected override void AddButtonsFooter(WebControl pCtrl)
        {
            //UserWithLimitedRights
            bool isSessionGuest = SessionTools.User.IsSessionGuest;

            if (!isSessionGuest)
            {
                pCtrl.Controls.Add(GetButtonFooter_XML());
                pCtrl.Controls.Add(GetButtonFooter_EXPORT());
            }
            pCtrl.Controls.Add(GetButtonFooter_POST());
            if (!isSessionGuest)
            {
                pCtrl.Controls.Add(GetButtonFooter_LOG());
                if (IsShowAdminTools)
                {
                    pCtrl.Controls.Add(GetButtonFooter_INPUTTRADE());
                }
            }
        }

        /// <summary>
        /// Retourne le contenu du toolbar action (Notepad,AttachedDoc,Event, Tracker)
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuAction()
        {
            bool isSessionGuest = SessionTools.User.IsSessionGuest;

            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(isSessionGuest ? 2 : 4);

            mnu[0] = GetMenuNotepad();
            mnu[1] = GetMenuAttachedDoc();

            if (!isSessionGuest)
            {
                //UserWithLimitedRights 
                mnu[2] = GetMenuEvent();
                mnu[3] = GetMenuTrack();
            }
            return mnu;
        }
        /// <summary>
        /// Retourne le contenu de la toolbar Mode (création, consultation, Duplication, Modifcation, Instrument)
        /// </summary>
        /// <returns></returns>
        /// FI 20170621 [XXXXX] Modify
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuMode()
        {

            //Pas d'actions diverse
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(5);
            mnu[0] = GetMenuCreation();
            mnu[1] = GetMenuConsult();
            mnu[2] = GetMenuDuplicate();
            mnu[3] = GetMenuModify();
            mnu[4] = GetMenuInstrument();

            // FI 20170621 [XXXXX] Alimentation de MnuModeModify
            MnuModeModify = mnu[3];

            return mnu;
        }
        
        /// <summary>
        /// Retourne  les parametres nécessaires à l'importation d'un titre 
        /// </summary>
        /// <returns></returns>
        public override ImportParameter[] GetImportParameter()
        {
            ArrayList al = new ArrayList
            {
                new ImportParameter() { name = TradeImportCst.instrumentIdentifier, value = TradeCommonInput.SQLInstrument.Identifier }, //Instrument
                new ImportParameter() { name = TradeImportCst.identifier, value = TradeCommonInput.Identifier }, // Identifier
                new ImportParameter() { name = TradeImportCst.displayName, value = TxtDisplayName }, // DisplayName
                new ImportParameter() { name = TradeImportCst.description, value = TxtDescription }, // Description
                new ImportParameter() { name = TradeImportCst.templateIdentifier, value = TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(SessionTools.CS)) }, //Template 
                new ImportParameter() { name = TradeImportCst.screen, value = TradeCommonInputGUI.CurrentIdScreen }, // Screen
                new ImportParameter() { name = TradeImportCst.extlLink, value = TxtExtLink } // Extarnal Link
            };
            return (ImportParameter[])al.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void SetToolBarsStyle()
        {
            base.SetToolBarsStyle();
            if (FindControl("tblMenu") is Control ctrlContainer)
            {
                if (ctrlContainer.FindControl("tbrresultaction") is WebControl ctrl)
                    ctrl.Style[HtmlTextWriterStyle.Display] = "none";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20150907 [21317] New Add Override (test existence de négociation sur le DebtSecurity = No Modif totale si existe Allocation)
        /// FI 20170621 [XXXXX] Modify
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override skmMenu.MenuItemParent GetMenuModify()
        {
            bool isOk = ((InputGUI.IsModifyAuthorised ||
                            InputGUI.IsModifyPostEvtsAuthorised ||
                            InputGUI.IsModifyMatchingAuthorised)) & (false == TradeCommonInput.IsTradeRemoved);

            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            if (isOk)
            {
                ret = new skmMenu.MenuItemParent(3);

                //Modification Totale
                skmMenu.MenuItemParent mnuchild = ret[0];
                mnuchild.eText = Ressource.GetString("ModificationGenExpiries");
                bool isEnabled = (InputGUI.IsModifyAuthorised);
                if (isEnabled)
                    isEnabled = !IsTradeFoundAndAllocationOnDebtSecurity(SessionTools.CS, TradeCommonInput.IdT);
                mnuchild.Enabled = isEnabled;
                if (isEnabled)
                {
                    mnuchild.eCommandName = Cst.Capture.ModeEnum.Update.ToString();
                    mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
                }


                //Modification Partielle
                mnuchild = ret[1];
                mnuchild.eText = Ressource.GetString("ModificationNoGenExpiries");
                mnuchild.Enabled = InputGUI.IsModifyPostEvtsAuthorised;
                if (InputGUI.IsModifyPostEvtsAuthorised)
                {
                    mnuchild.eCommandName = Cst.Capture.ModeEnum.UpdatePostEvts.ToString();
                    mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
                }
            }

            ret.aID = "btnModification";
            ret.aToolTip = Ressource.GetString("Modification");
            ret.eImageUrl = "fas fa-icon fa-edit";
            ret.Enabled = isOk;
            if (isOk)
            {
                // FI 20170621 [XXXXX] Modification equivalent à Modification Totale
                // Enabled si Modification Totale enabled
                //ret.eCommandName = Cst.Capture.ModeEnum.Update.ToString();
                ret.eCommandName = ret[0].eCommandName; //ret[0].eCommandName => Modification Totale
                // FI 20170621 [XXXXX] Parent Enabled si 1er enfant Enabled
                ret.Enabled = ret[0].Enabled;
            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += Ressource.GetString("NotAllowed");
            }
            return ret;
        }


        /// <summary>
        /// Retourne true s'il existe un trade CashBalance portant sur le trade a été généré pour ce Trade en date business &lt;= {pBusinessDate}
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pIdT"></param>
        /// <param name="pBusinessDate"></param>
        /// <returns></returns>
        /// EG 20150407 [POC] New
        // EG 20191115 [25077] RDBMS : New version of Trades tables architecture (TRADESTSYS merge to TRADE, NEW TABLE TRADEXML)
        /// EG 20210329 [25562] Correction sur requêtes utilisant encore à tort TRADEINSTRUMENT
        private bool IsTradeFoundAndAllocationOnDebtSecurity(string pCS, int pIdAsset)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "IDASSET", DbType.Int32), pIdAsset);
            parameters.Add(new DataParameter(pCS, "ASSETCATEGORY", DbType.AnsiString, SQLCst.UT_ENUM_OPTIONAL_LEN), Cst.UnderlyingAsset.Bond.ToString());

            string sqlQuery = @"select 1
            from dbo.TRADE tr
            where (tr.IDSTACTIVATION = 'REGULAR') and (tr.IDSTBUSINESS = 'ALLOC') and (tr.ASSETCATEGORY = @ASSETCATEGORY) and (tr.IDASSET = @IDASSET)" + Cst.CrLf;

            QueryParameters queryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
            return (null != obj);
        }

        /// <summary>
        /// Retourne le menu Report
        /// <remarks>Le menu réport affiche les cotations uniquement, il n'affiche pas des reports</remarks>
        /// </summary>
        /// <returns></returns>
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected skmMenu.MenuItemParent GetMenuReport()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            //
            if (this.TradeCommonInput.IsTradeFound)
            {
                ret = new skmMenu.MenuItemParent(1);
                string menuId = SessionTools.Menus.GetMenu_Url(IdMenu.GetIdMenu(IdMenu.Menu.QUOTE_DEBTSEC_H));
                menuId = AspTools.AddIdMenuOnUrl(menuId, IdMenu.GetIdMenu(IdMenu.Menu.QUOTE_DEBTSEC_H)); 
                menuId += "&FK=" + HttpUtility.UrlEncode(this.TradeCommonInput.IdT.ToString(), Encoding.Default);

                skmMenu.MenuItemParent mnuchild = ret[0];
                mnuchild.Enabled = true;
                mnuchild.eText = Ressource.GetString("Prices");
                mnuchild.eUrl = menuId;
            }

            //Comme Le menu réport affiche les cotations uniquement
            //ds eText On y place Detail de manière à ce que le comportement soit identique à celui du référentiel
            ret.aID = "btnReport";
            ret.eImageUrl = "fas fa-icon fa-project-diagram";
            ret.eText = Ressource.GetString("Detail");
            ret.Enabled = true;

            if (ArrFunc.Count(ret.subItems) == 1)
                ret.eUrl = ret[0].eUrl;
            return ret;
        }


        /// <summary>
        /// Retourne le contenu de la toolbar Report 
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuReport()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            mnu[0] = GetMenuReport();
            return mnu;
        }

        /// <summary>
        /// Retourne un menu vide
        /// <para>Il n'y a pas de menu CashFlow sur les trade DEBTSEC</para>
        /// <para>La consultation 3btns Trade\CASHFLOWS.xml ne traite que les trades de marché</para>
        /// <para>A faire évoluer si la consultation gère les trades DEBTSEC</para>
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuCashFlow()
        {
            return new skmMenu.MenuItemParent(0);
        }

        #endregion
    }
}