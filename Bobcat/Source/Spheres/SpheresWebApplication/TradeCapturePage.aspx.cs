#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.GUI.ComplexControls;
using EFS.GUI.Interface;
using EFS.Import;
using EFS.Process;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using FixML.Enum;
using FpML.Interface;//
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TradeCapturePage.
    /// </summary>
    public partial class TradeCapturePage : TradeCommonCapturePage
    {
        #region Members
        private TradeCaptureGen m_CaptureGen;
        private TradeInputGUI m_InputGUI;
        private TradeHeaderBanner m_TradeHeaderBanner;
        #endregion Members
        //
        #region Accessors
        /// <summary>
        /// 
        /// </summary>
        public TradeInput TradeInput
        {
            get { return m_CaptureGen.Input; }
        }


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
            get { return (TradeCommonHeaderBanner)m_TradeHeaderBanner; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get { return m_CaptureGen.TradeCommonInput; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)m_InputGUI; }
        }

        /// <summary>
        /// Retourne l'objet qui est affiché
        /// </summary>
        protected override ICustomCaptureInfos Object
        {
            get { return m_CaptureGen.Input; }
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

        /// <summary>
        /// Obtient le context [utilisé pour la recherche des objets de design]
        /// </summary>
        ///FI 20121113 [18224] refactoring
        // EG 20180514 [23812] Report
        protected override string ObjectContext
        {
            get
            {
                string ret = string.Empty;
                //
                bool newMode = true;
                if (StrFunc.IsFilled(Request.QueryString["objectContextMode"]))
                {
                    if (Request.QueryString["objectContextMode"] == "OldMode")
                        newMode = false; 
                }
                
                ArrayList al = new ArrayList();
                if (TradeInput.IsAllocation)
                {
                    al.Add("allocation");
                }
                else
                {
                    //FI 20121113 [18224] dans le flux XML descriptif d'un écran il n'existe aucun object avec l'attribut deal
                    //Spheres® effectue des recherche de noeud XML avec cet attribut pour rien
                    //Donc désormais les trades classiques recherche les object sans attribut context
                    if (newMode == false)
                        al.Add("deal");
                }
                //
                if ((InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionCancelation) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.RemoveAllocation) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.UnderlyingDelivery) ||  // PM 20130827 [17949] Livraison Matif
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination))  // FI 20180221 [23803] Add FxOptionEarlyTermination 

                {
                    al.Add(Enum.GetName(typeof(Cst.Capture.ModeEnum), InputGUI.CaptureMode));
                }
                // EG 20150716 Add Product
                if (((InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionCancelation) || (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer)) &&
                    (TradeInput.IsESEandAllocation || TradeInput.IsDSTandAllocation))
                {
                    al.Add("Safekeeping");
                }
                //
                if (StrFunc.IsFilled(base.ObjectContext))
                    al.Add(base.ObjectContext);
                //
                if (ArrFunc.IsFilled(al))
                    ret = StrFunc.StringArrayList.StringArrayToStringList((String[])al.ToArray(typeof(String)), false);
                //   
                return ret;
            }
        }
        #endregion Accessors
        //
        #region Events
        /// <summary>
        /// <para>Au clique sur le bouton "Calculatrice", cette méthode calcul les frais à partir des barèmes.</para>
        /// <para>Le calcul pourrait être interrompu, pour attendre la réponse de l'utilisateur.</para>
        /// </summary>
        private void OnFeesClick()
        {

            if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeNewOrDuplicateOrReflect(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeAction(m_InputGUI.CaptureMode))
            {
                string target = "OnFeesClick";
                string commandArgument = Request.Params["__EVENTARGUMENT"];
                string eventTarget = Request.Params["__EVENTTARGET"];
                //
                // Nom de l'étape à la quelle a eu lieu l'interruption (NB: Au premier passage, cette donnée est vide)
                string eventStep = string.Empty;
                //
                // Info sur l'interruption
                string eventArgument = string.Empty;
                //
                if (StrFunc.IsFilled(commandArgument))
                {
                    //
                    string[] commandArguments = commandArgument.Split(';');
                    //
                    if (commandArguments.Length > 0)
                        eventStep = commandArguments[0];
                    if (commandArguments.Length > 1)
                        eventArgument = commandArguments[1];
                }
                
                //PL 20140722 A finaliser... 
                // Step 0 : FundingAndMargin
                ProcessFundingAndMargin(eventTarget, eventStep, eventArgument, target, string.Empty);
                
                // Step 1 : On calcul les frais. 
                //      Ici, l'utilisateur peut être amené à devoir répondre à des questions, telles que:
                //       - la confirmation d'effacer des éventuels frais saisis manuellement
                //       - ...
                // NB: Le 4ème paramètre indique le "target" à l'origine de l'appel de ProcessFee(). Ici: la méthode OnFeesClick.
                bool isOk = ProcessFee(eventTarget, eventStep, eventArgument, target, string.Empty);
                //
                // Step 2 : Si tout est ok, on recharge l'écran afin d'afficher les frais calculés.
                if (isOk && (null != InputGUI.DocXML))
                    UpdatePlaceHolder();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start TradeCapturePage.OnInit");

            base.OnInit(e);
            m_CaptureGen = new TradeCaptureGen();
            m_CaptureGen.Input.InitDefault(m_DefaultParty, m_DefaultCurrency, m_DefaultBusinessCenter);

            AddAuditTimeStep("End TradeCapturePage.OnInit");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsdaClick(object sender, EventArgs e)
        {
            try
            {
                DisplayConfirm("Isda");
            }
            catch (Exception ex)
            {
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {

            AddAuditTimeStep("Start TradeCapturePage.OnLoad");

            if (false == IsPostBack)
            {
                //si le menu n'est pas passé dans lURL, utilisation du menu par défaut 
                string idMenu = Request.QueryString["IDMenu"];
                if (StrFunc.IsEmpty(idMenu))
                    idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade);
                //Request.ServerVariables["APPL_PHYSICAL_PATH"]
                m_InputGUI = new TradeInputGUI(idMenu, SessionTools.User, XML_FilesPath);
                m_InputGUI.InitializeFromMenu(CSTools.SetCacheOn(SessionTools.CS));
            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //m_InputGUI = (TradeInputGUI)Session[InputGUISessionID];
                //m_FullCtor = (FullConstructor)Session[FullConstructorSessionID];
                //m_CaptureGen.Input = (TradeInput)Session[InputSessionID];

                m_InputGUI = DataCache.GetData<TradeInputGUI>(InputGUISessionID);
                m_FullCtor = DataCache.GetData<FullConstructor>(FullConstructorSessionID);
                m_CaptureGen.Input = DataCache.GetData<TradeInput>(InputSessionID);
            }

            base.OnLoad(e);

            AddAuditTimeStep("End TradeCapturePage.OnLoad");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20180214 [23774] Modify
        /// EG 20201009 Changement de nom pour enum ClearFeeMode
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("Start TradeCapturePage.OnPrerender");

            string eventTarget = string.Empty + PARAM_EVENTTARGET;
            _ = string.Empty + PARAM_EVENTARGUMENT;

            ((TextBox)CtrlTrade).Attributes["oldvalue"] = TradeIdentifier;

            if (IsPostBack)
            {
                if ((Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType))
                {
                    if ((null != TradeInput.CustomCaptureInfos) && (Cst.Capture.IsModeInput(m_InputGUI.CaptureMode)))
                    {
                        //FI 20180502 [23926] appel à ISSubstituteFeeShedule
                        if (ISSubstituteFeeShedule(eventTarget, out IPayment oppPayment, out SQL_FeeSchedule sqlFeeSchedle))
                        {
                            //Pas de lecture des contrôle, pas de mise à jour de la collection ccis => isLoadCcisFromGUI = false; 
                            IsLoadCcisFromGUI = false;
                            // IsToSynchronizeWithDocument s'il y a eu effectivement substitution d'un barème
                            TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument = SubstituteFeeShedule(oppPayment, sqlFeeSchedle);
                        }

                        // FI 20100427 [16970] add test sur isLoadCcisFromGUI 
                        // FI 20200402 [XXXXX] Add test (false == IsDblPostbackFromAutocompleteControl)
                        if (IsLoadCcisFromGUI && (false == IsDblPostbackFromAutocompleteControl)) 
                        {

                            TradeInput.CustomCaptureInfos.Initialize_FromGUI(this);

                            bool isFeeToReset = false;
                            if ("tblMenu_btnRecord" != eventTarget)
                                isFeeToReset = IsFeeToReset();

                            //Mise a jour du dataDocument
                            TradeInput.CustomCaptureInfos.Dump_ToDocument(0);

                            //A la sortie du dump on reset les fees, uniquement s'il existe au moins un cci modifié avec succès
                            //NB: Si tous les ccis modifiés sont en erreur, il n'est utile de recalculer les fees
                            if (isFeeToReset)
                                isFeeToReset = IsChangeOk(GetCciOtherThanFeeChanged());

                            //EG 20090107 Laisser entre Dump_ToDocument(0) et IsToSynchronizeWithDocument
                            if ("ExecFunction" == eventTarget)
                                ExecFunction();

                            if (isFeeToReset)
                            {
                                //Suppression des frais déja calculés uniquement
                                TradeInput.ClearFee(GetFeeTarget(), TradeInput.ClearFeeMode.FromSchedule);
                                TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument = true;
                            }
                        }

                        //Mise à jour de l'IHM
                        //FI 20100427 [16970] alimentation de isLastPostUpdatePlaceHolder afin de détecter un éventuel douple post
                        IsLastPostUpdatePlaceHolder = TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument;
                        if (TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument)
                        {
                            //Refaire toutes les étapes pour pouvoir afficher les nouveautés du Document Fpml
                            TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument = false;
                            TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                            UpdatePlaceHolder();
                        }
                        else
                        {
                            TradeInput.CustomCaptureInfos.Dump_ToGUI(this);
                        }
                    }

                    //Bouton Add Or Delete Item
                    if ("OnAddOrDeleteItem" == eventTarget)
                    {
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnAddOrDeleteItem();
                    }

                    //Bouton Fees 
                    if ("OnFeesClick" == eventTarget)
                    {
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnFeesClick();
                    }

                    //
                    if (Cst.OTCml_ScreenBox == eventTarget)
                    {
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnZoomScreenBox();
                    }

                    //Bouton UTI
                    //FI 20140206 [19564] Appel à CalcUTI 
                    if (eventTarget.StartsWith("OnUTIParty"))
                    {
                        CalcUTI(eventTarget);
                    }
                }

                //Bouton zoom sur un section de l'écran Full (ie: Settlement Instruction)
                if (Cst.FpML_ScreenFullCapture == eventTarget)
                {
                    TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                    OnZoomFpml();
                }
            }
            //
            if (TradeInput.IsTradeFound &&
                (Cst.Capture.IsModeNewCapture(m_InputGUI.CaptureMode) ||
                 Cst.Capture.IsModeUpdateOrUpdatePostEvts(m_InputGUI.CaptureMode)))
            {
                #region Initialisation du status en fonction des actors et de leurs rôles
                // Initialisation réalisée ici sur le PreRender à cause de la saisie light
                ActorRoleCollection actorRoleNew = TradeCommonInput.DataDocument.GetActorRole(CSTools.SetCacheOn(SessionTools.CS));
                if ((null == m_InputGUI.ActorRole) || m_InputGUI.ActorRole.CompareTo(actorRoleNew) != 0)
                {
                    m_InputGUI.ActorRole = actorRoleNew;
                    //Reinit des default user status (Check/Match) from ACTORROLE et ACTIONTUNING
                    TradeInput.ResetStUser();
                    TradeInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS), null);
                    TradeInput.InitStUserFromTuning(m_InputGUI.ActionTuning);
                }
                #endregion
            }

            // FI 20200518 [XXXXX] Utilisation de DataCache           
            //Session[InputGUISessionID] = m_InputGUI;
            //Session[InputSessionID] = TradeInput;
            //Session[FullConstructorSessionID] = m_FullCtor;
            DataCache.SetData(InputGUISessionID,  m_InputGUI);
            DataCache.SetData(InputSessionID, TradeInput);
            DataCache.SetData(FullConstructorSessionID, m_FullCtor);
            
            SetValidatorEnabled(false);
            
            //PL 20150422
            //if (TradeCommonInput.IsTradeFoundAndAllocation)
            bool isVisible = (false == TradeCommonInput.IsETDorESEorRTSAllocation);

            #region Cacher les Boutons "ISDA" et "SWIFT" 

            LiteralControl space;
            if (FindControl("btnISDA") is ImageButton img)
            {
                img.Visible = isVisible;

                space = FindControl("spaceISDA") as LiteralControl;
                if (null != space)
                    space.Visible = isVisible;
            }
            img = FindControl("btnSWIFT") as ImageButton;
            if (null != img)
            {
                img.Visible = isVisible;

                space = FindControl("spaceSWIFT") as LiteralControl;
                if (null != space)
                    space.Visible = isVisible;
            }
            #endregion
            //
            base.OnPreRender(e);
            //
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
            AddAuditTimeStep("End TradeCapturePage.OnPrerender");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140708 [20179] Modify: Gestion du mode Match
        // EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            try
            {
                // FI 20210621 [XXXXX] ajout de PostBackForValidationArg
                bool isValidateOk = (null == e.CommandArgument) || Convert.ToString(e.CommandArgument).Contains("TRUE") || Convert.ToString(e.CommandArgument) == PostBackForValidationArg;


                AddAuditTimeStep("Start TradeCapturePage.OnValidate");

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
                                if (Cst.Capture.IsModeAction(m_InputGUI.CaptureMode))
                                {
                                    OnValidateRecordAction(sender, e);
                                }
                                else if (Cst.Capture.IsModeMatch(m_InputGUI.CaptureMode))
                                {
                                    OnValidateMatch(sender, e);
                                }
                                else
                                {
                                    OnValidateRecordTrade(sender, e);
                                }
                                break;
                            case Cst.Capture.MenuValidateEnum.Annul:
                                #region Annulation de la saisie en cours
                                if (null != ctrl)
                                {
                                    string eArgs;
                                    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                                    if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(m_InputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeDuplicateOrReflect(m_InputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeUpdateFeesUninvoiced(m_InputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeAction(m_InputGUI.CaptureMode)|| 
                                        Cst.Capture.IsModeMatch(m_InputGUI.CaptureMode))
                                        eArgs = Cst.Capture.ModeEnum.Consult.ToString();
                                    else
                                        eArgs = Cst.Capture.ModeEnum.New.ToString();
                                
                                    OnMode(ctrl, new skmMenu.MenuItemClickEventArgs(eArgs));
                                }
                                #endregion Annulation de la saisie en cours
                                break;
                        }
                    }
                }


                AddAuditTimeStep("End TradeCapturePage.OnValidate");
            }
            catch (Exception ex)
            {
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSwiftClick(object sender, EventArgs e)
        {
            try
            {
                DisplayConfirm("SWIFT");
            }
            catch (Exception ex)
            {
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }
        #endregion Events
        //
        #region Methods
        /// <summary>
        /// Ajoute dans la cellule les boutons XML,EXPORT,SWIFT,CONFIRM
        /// </summary>
        // EG 20160119 Refactoring Footer
        protected override void AddButtonsFooter(WebControl pCtrlContainer)
        {
            //UserWithLimitedRights
            bool isSessionGuest = SessionTools.User.IsSessionGuest;

            if (!isSessionGuest)
            {
                pCtrlContainer.Controls.Add(GetButtonFooter_XML());
                pCtrlContainer.Controls.Add(GetButtonFooter_EXPORT());

                pCtrlContainer.Controls.Add(GetButtonFooter_ISDA());
                pCtrlContainer.Controls.Add(GetButtonFooter_SWIFT());
            }
            pCtrlContainer.Controls.Add(GetButtonFooter_CONFIRM());
            pCtrlContainer.Controls.Add(GetButtonFooter_POST());
            if (!isSessionGuest)
            {
                pCtrlContainer.Controls.Add(GetButtonFooter_LOG());

                if (IsShowAdminTools)
                {
                    pCtrlContainer.Controls.Add(GetButtonFooter_INPUTTRADE());
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void AddHeader()
        {
            m_TradeHeaderBanner = new TradeHeaderBanner(this, GUID, CellForm, TradeInput, m_InputGUI,
                (false == Cst.Capture.IsModeRemoveOnlyAll(m_InputGUI.CaptureMode)));
            m_TradeHeaderBanner.AddControls();
        }


        /// <summary>
        /// Retourne le menu duplication 
        /// </summary>
        ///EG 20120613 BlockUI New
		/// RD 20161117 Add Reflect Menu
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify (DuplicationReverseSide et Reflection désormais disponible sur commoditySpot) 
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override skmMenu.MenuItemParent GetMenuDuplicate()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            
            if (InputGUI.IsCreateAuthorised)
            {
                ret = new skmMenu.MenuItemParent(3)
                {
                    Enabled = true,
                    eCommandName = Cst.Capture.ModeEnum.Duplicate.ToString()
                };

                #region Affecting childs of current menu

                skmMenu.MenuItemParent mnuchild = ret[0];
                mnuchild.Enabled = true;
                mnuchild.eText = Ressource.GetString("Duplication");
                mnuchild.eCommandName = Cst.Capture.ModeEnum.Duplicate.ToString();
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
                
                mnuchild = ret[1];
                mnuchild.Enabled = true;
                mnuchild.eText = Ressource.GetString("DuplicationReverseSide");
                mnuchild.eCommandName = Cst.Capture.ModeEnum.Duplicate.ToString();
                mnuchild.eArgument = "ReverseSide";
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
                
                mnuchild = ret[2];
                mnuchild.Enabled = true;
                mnuchild.eText = Ressource.GetString("Reflection");
                mnuchild.eCommandName = Cst.Capture.ModeEnum.Reflect.ToString();
                mnuchild.eArgument = "ReverseSide";
                mnuchild.eBlockUIMessage = mnuchild.eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
                
                #endregion Affecting childs of current menu

            }
            
            ret.aID = "btnDuplication";
            ret.aToolTip = Ressource.GetString("Duplication");
            if (false == InputGUI.IsCreateAuthorised)
            {
                ret.aToolTip += Cst.HTMLBreakLine;
                ret.aToolTip += Ressource.GetString("NotAllowed");
            }
            ret.eImageUrl = "fas fa-icon fa-copy";
            return ret;

        }

        /// <summary>
        /// Retourne  les parametres nécessaires à l'importation d'un trade
        /// </summary>
        /// FI 20170206 [XXXXX] Modify
        public override ImportParameter[] GetImportParameter()
        {

            ArrayList al = new ArrayList();

            //Instrument
            ImportParameter param = new ImportParameter
            {
                name = TradeImportCst.instrumentIdentifier,
                value = TradeCommonInput.SQLInstrument.Identifier
            };
            al.Add(param);

            //Template 
            param = new ImportParameter
            {
                name = TradeImportCst.templateIdentifier,
                value = TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(SessionTools.CS))
            };
            al.Add(param);

            //screen
            param = new ImportParameter
            {
                name = TradeImportCst.screen,
                value = TradeCommonInputGUI.CurrentIdScreen
            };
            al.Add(param);

            //DisplayName 
            param = new ImportParameter
            {
                name = TradeImportCst.displayName,
                value = TxtDisplayName
            };
            al.Add(param);

            //Description 
            param = new ImportParameter
            {
                name = TradeImportCst.description,
                value = TxtDescription
            };
            al.Add(param);

            //ExtlLink
            param = new ImportParameter
            {
                name = TradeImportCst.extlLink,
                value = TxtExtLink
            };
            al.Add(param);

            //calcul de frais
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@string.ToString(),
                name = TradeImportCst.feeCalculation,
                value = "Ignore"
            };
            al.Add(param);

            //Initialisation depuis PartyTemplate
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isApplyPartyTemplate,
                value = "false"
            };
            al.Add(param);

            //Initialisation depuis ClearingTemplate
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isApplyClearingTemplate,
                value = "false"
            };
            al.Add(param);

            //FI 20170206 [XXXXX] Mod
            //Initialisation depuis ClearingTemplate (mode Reverse)
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isApplyReverseClearingTemplate,
                value = "false"
            };
            al.Add(param);

            //FI 20170206 [XXXXX] add isApplyValidationRules
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isApplyValidationRules,
                value = "false"
            };
            al.Add(param);

            //FI 20170206 [XXXXX] add isApplyValidationXSD
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isApplyValidationXSD,
                value = "false"
            };
            al.Add(param);


            //Event Calculation
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isPostToEventsGen,
                value = "false"
            };
            al.Add(param);
            
            ImportParameter[] ret = null;
            if (ArrFunc.Count(al) > 0)
                ret = (ImportParameter[])al.ToArray(typeof(ImportParameter));
            //
            return ret;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected WCToolTipLinkButton GetButtonFooter_ISDA()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnISDA", null, "ISDA", "btnISDAToolTip");
            btn.Click += new EventHandler(OnIsdaClick);
            return btn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected WCToolTipLinkButton GetButtonFooter_SWIFT()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnSWIFT", null, "SWIFT", "btnSWIFTToolTip");
            btn.Click += new EventHandler(OnSwiftClick);
            return btn;

        }

        /// <summary>
        /// Retourne les attributs {pIsHidden} et {pIsEnabled} d'un menu ACTION
        /// <para></para>
        /// </summary>
        /// <param name="pMenuInput"></param>
        /// <param name="pIsHidden"></param>
        /// <param name="pIsEnabled"></param>
        /// <param name="pDisabledReason"></param>
        /// EG 20110401 Add Test sur existence des EVTS pour Annulation
        /// RD 20110531 [17473]
        /// Add Test sur existence des EventsPosAction pour les ETDAllocation et pour les menus:
        /// - Annulation
        /// - Abandon
        /// - Exercice
        /// - Assignation
        /// - Correction quantité
        /// - Transfert    
        /// EG 20111122 Modification des critères d'accès aux différentes actions sur ETDAlloc 
        /// - en fonction de la présence du trade comme clôturées/clôturante dans POSACTIONDET antérieur ou égal à DTBUSINESS.
        /// - en fonction de la quantité en position disponible pour le trade en date DTBUSINESS.
        /// FI 20130308 [] add pDisabledReason
        /// FI 20161214 [21916] Modify
        /// FI 20170116 [21916] Modify (l'action annulation est déormais posible sur commoditySpot)
        // EG 20180514 [23812] Report
        protected override void IsActionAuthorized(string pMenuInput, out bool pIsHidden, out bool pIsEnabled, out string pDisabledReason)
        {
            pDisabledReason = string.Empty;

            bool isHidden = false;
            bool isEnabled = true;
            if ((TradeInput.IsTradeFound) && IdMenu.IsMenuTradeInput(pMenuInput))
            {
                ProductContainer product = TradeCommonInput.Product;

                Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(pMenuInput);
                bool isEarlyTerminationExe;
                switch (menu)
                {
                    case IdMenu.Menu.InputTrade_RMV:  //Annulation d'un Trade
                    case IdMenu.Menu.InputTrade_REP:  //Annulation d'un Trade avec Remplacement
                        // FI 20170116 [21916] ajout clause instrument fongible    
                        isHidden = TradeCommonInput.IsTradeFoundAndAllocation && TradeCommonInput.SQLInstrument.IsFungible;
                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            if (TradeCommonInput.IsTradeFoundAndAllocation) //Ici les allocations sont des allocations non fongibles  (Ex commoditySpot)
                            {
                                // FI 20170116 [21916] règle générique et qui s'applique pour l'instant uniquement sur les commoditySpot
                                // L'annulation peut se faire uniquement sur les trades du jour (A voir si l'on va plus loin plus tard car il faudrait une gestion des les éditions et cela n'existe pas aujourd'hui)
                                isEnabled &= (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate);
                                if (false == (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate))
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoClearingDateBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));

                                // FI 20170116 [21916] pas d'annulation si trade impliqué dans un solde
                                // RD 20201228 [25527] Enlever la restriction Annulation/Modification si trade Commodity Spot impliqué dans un solde 
                                //isEnabled &= (false == TradeCommonInput.IsExistCashBalance);
                                //if (TradeCommonInput.IsExistCashBalance)
                                //    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_TradeIsUsedInCashBalance");
                            }
                        }
                        break;
                    case IdMenu.Menu.InputTrade_RMVALLOC:  //Annulation d'un Trade Allocation sur produit fongible
                        // FI 20170116 [21916] Remove Alloc uniquement sur instrument Fongible
                        isHidden = !(TradeCommonInput.IsTradeFoundAndAllocation && TradeCommonInput.SQLInstrument.IsFungible);

                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            // RD 20130524 [18657]
                            // L’annulation d’un trade en position n’est pas appropriée, il faudrait passer la fonctionnalité de la « Correction de la position »
                            // Annuler uniquement les trades du jour
                            isEnabled &= (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate);
                            if (false == (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoClearingDateBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));
                        }


                        break;
                    case IdMenu.Menu.InputTrade_CAN:  //Resiliation (CancelableProvision)
                        isHidden = (false == product.IsProduct_CANProvision);
                        isEnabled = TradeInput.IsTradeCancelable;

                        break;
                    case IdMenu.Menu.InputTrade_MET:  //Resiliation (MandatoryEarlyTerminationProvision)
                        isHidden = (false == product.IsProduct_METProvision);
                        isEnabled = TradeInput.IsTradeMandatoryEarlyTermination;

                        break;
                    case IdMenu.Menu.InputTrade_OET:  //Resiliation (OptionalEarlyTerminationProvision)
                        isHidden = (false == product.IsProduct_OETProvision);
                        isEnabled = TradeInput.IsTradeOptionalEarlyTermination;

                        if (product.IsFxOption)
                        {
                            // FI 20180221 [23803] interdiction de resiliée une opération déjà résiliée
                            isEarlyTerminationExe = (TradeRDBMSTools.IsEventExist_Parent(SessionTools.CS, TradeIdT, EventCodeEnum.EXO.ToString(), EventCodeEnum.PRO.ToString()));
                            isEnabled &= (false == isEarlyTerminationExe);
                            if (isEarlyTerminationExe)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_EarlyTerminationAlreadyExist");

                            bool isExeTot = TradeRDBMSTools.IsEventExist(SessionTools.CS, TradeIdT, EventCodeEnum.EXE.ToString(), EventTypeEnum.TOT.ToString());
                            isEnabled &= (false == isExeTot);
                            if (isExeTot)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEarlyTerminationOnExercisedOption");

                            // EG 20180315 [23812] Step2
                            bool isAbnTot = TradeRDBMSTools.IsEventExist(SessionTools.CS, TradeIdT, EventCodeEnum.ABN.ToString());
                            isEnabled &= (false == isAbnTot);
                            if (isAbnTot)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEarlyTerminationOnAbandonedOption");
                        }

                        break;
                    case IdMenu.Menu.InputTrade_RES:  //Resiliation
                        isHidden = (false == product.IsProduct_TerminationProvision);
                        isEnabled = TradeInput.IsTradeTerminationAuthorized;

                        break;
                    case IdMenu.Menu.InputTrade_SUP:  //Augmentation de capital (StepUpProvision)
                        isHidden = (false == product.IsProduct_SUPProvision);
                        isEnabled = TradeInput.IsTradeStepUp;

                        break;
                    case IdMenu.Menu.InputTrade_PRO:  //Prorogation
                        isHidden = (false == product.IsProduct_EXTProvision);
                        isEnabled = TradeInput.IsTradeExtendible;

                        break;
                    case IdMenu.Menu.InputTrade_CAL:  //Dénonciation
                    case IdMenu.Menu.InputTrade_REN:  //Renouvellement
                        isHidden = true;
                        break;

                    case IdMenu.Menu.InputTrade_ABN:  //Abandon
                        isHidden = (false == TradeInput.IsAbandonAuthorized);
                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            if (product.IsFxOption)
                            {
                                // FI 20180221 [23803] interdiction de resiliée une opération déjà abandonnée
                                isEarlyTerminationExe = (TradeRDBMSTools.IsEventExist_Parent(SessionTools.CS, TradeIdT, EventCodeEnum.EXO.ToString(), EventCodeEnum.PRO.ToString()));
                                isEnabled &= (false == isEarlyTerminationExe);
                                if (isEarlyTerminationExe)
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_OnExercisedEarlyTerminationOption");
                            }

                            if (TradeCommonInput.IsETDandAllocation) // EG 20140731 : ICI on garde IsETDAllocation
                            {
                                isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null);
                                if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");
                            }
                        }
                        break;
                    case IdMenu.Menu.InputTrade_ASS:  //Assignation
                        isHidden = (false == TradeInput.IsAssignmentAuthorized);

                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            if (product.IsFxOption)
                            {
                                // FI 20180221 [23803] interdiction de resiliée une opération déjà abandonnée
                                isEarlyTerminationExe = (TradeRDBMSTools.IsEventExist_Parent(SessionTools.CS, TradeIdT, EventCodeEnum.EXO.ToString(), EventCodeEnum.PRO.ToString()));
                                isEnabled &= (false == isEarlyTerminationExe);
                                if (isEarlyTerminationExe)
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_OnExercisedEarlyTerminationOption");
                            }

                            if (TradeCommonInput.IsETDandAllocation) // EG 20140731 : ICI on garde IsETDAllocation
                            {
                                isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null);
                                if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");

                                // EG 20120131 Pas de test sur la quantité available (pour pouvoir retraiter un dénouement du jour)
                                //isEnabled &= (0 < availableQty);
                            }
                        }
                        break;
                    case IdMenu.Menu.InputTrade_POC:  //Correction quantité
                        isHidden = (false == TradeInput.IsPositionCancelationEvents);

                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null);
                            if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");


                            isEnabled &= (0 < TradeCommonInput.AvailableQuantity);
                            if (false == (0 < TradeCommonInput.AvailableQuantity))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoAvailableQty");

                            isEnabled &= (TradeCommonInput.ClearingBusinessDate < TradeCommonInput.CurrentBusinessDate);
                            if (false == (TradeCommonInput.ClearingBusinessDate < TradeCommonInput.CurrentBusinessDate))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoClearingDatePreviousBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));
                        }
                        break;
                    case IdMenu.Menu.InputTrade_POT:  //Transfert
                        isHidden = (false == TradeInput.IsPositionTransferEvents);
                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            //PL 20180305 Enable TRANSFER on Give-Up trade
                            //isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer;
                            //if (false == TradeCommonInput.IsPosKeepingOnBookDealer)
                            //    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");

                            isEnabled &= (0 < TradeCommonInput.AvailableQuantity);
                            if (false == (0 < TradeCommonInput.AvailableQuantity))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoAvailableQty");

                            // EG 20120130 Strictement < DtBusiness
                            isEnabled &= (TradeCommonInput.ClearingBusinessDate < TradeCommonInput.CurrentBusinessDate);
                            if (false == (TradeCommonInput.ClearingBusinessDate < TradeCommonInput.CurrentBusinessDate))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoClearingDatePreviousBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));
                        }

                        break;
                    case IdMenu.Menu.InputTrade_SPLIT:  //Split
                        isHidden = (false == TradeInput.IsTradeSplitting);
                        if (false == isHidden)
                        {
                            isEnabled = (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate);
                            if (false == (TradeCommonInput.ClearingBusinessDate >= TradeCommonInput.CurrentBusinessDate))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoClearingDateBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));
                        }
                        break;
                    case IdMenu.Menu.InputTrade_CLEARSPEC:  //Clôture spécifique
                        // EG 20150907 [21317] Add EquitySecurityTRrans&action|DebtSecurityTransaction
                        //isHidden = (false == product.isExchangeTradedDerivative);
                        // FI 20161214 [21916] Modify
                        //isHidden = (false == TradeCommonInput.IsTradeFoundAndAllocation);
                        isHidden = (false == TradeInput.IsClearingSpecificEvents); // FI 20161214 [21916] call IsClearingSpecificEvents
                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null);
                            if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");

                            isEnabled &= (0 < TradeCommonInput.AvailableQuantity);
                            if (false == (0 < TradeCommonInput.AvailableQuantity))
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoAvailableQty");
                        }
                        break;
                    case IdMenu.Menu.InputTrade_BAR_TRG: //Barrier & trigger
                        isHidden = (false == TradeInput.IsBarrierTriggerAuthorized);

                        isEarlyTerminationExe = (TradeRDBMSTools.IsEventExist_Parent(SessionTools.CS, TradeIdT, EventCodeEnum.EXO.ToString(), EventCodeEnum.PRO.ToString()));
                        isEnabled &= (false == isEarlyTerminationExe);
                        if (isEarlyTerminationExe)
                            AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_OnExercisedEarlyTerminationOption");

                        break;
                    case IdMenu.Menu.InputTrade_CSR:     //Fixing determined by a Calculation Agent
                        isHidden = (false == TradeInput.IsFixingCustomerAuthorized);
                        break;
                    case IdMenu.Menu.InputTrade_EXE:     //Exercise
                        isHidden = (false == TradeInput.IsExerciseAuthorized);
                        if (false == isHidden)
                        {
                            isEnabled = TradeCommonInput.IsEventsGenerated;
                            if (false == TradeCommonInput.IsEventsGenerated)
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");

                            if (product.IsFxOption)
                            {
                                // FI 20180221 [23803] interdiction de resiliée une opération déjà abandonnée
                                isEarlyTerminationExe = (TradeRDBMSTools.IsEventExist_Parent(SessionTools.CS, TradeIdT, EventCodeEnum.EXO.ToString(), EventCodeEnum.PRO.ToString()));
                                isEnabled &= (false == isEarlyTerminationExe);
                                if (isEarlyTerminationExe)
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_OnExercisedEarlyTerminationOption");
                            }

                            if (TradeCommonInput.IsETDandAllocation) // EG 20140731 : ICI on garde IsETDAllocation
                            {
                                isEnabled &= TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null);
                                if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");
                            }
                        }
                        break;
                    case IdMenu.Menu.InputTrade_EEX:    //Levée anticipée
                        _ = (false == product.IsFxOption);
                        isHidden = true;
                        break;
                    // PM 20130822 [17949] Livraison Matif
                    case IdMenu.Menu.InputTrade_DLV:
                        isHidden = (false == TradeInput.IsUnderlyingDeliveryAuthorized);
                        //
                        if (false == isHidden)
                        {
                            isEnabled = true;
                            if (false == TradeCommonInput.IsEventsGenerated)
                            {
                                isEnabled = false;
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoEvents");
                            }
                            if (false == TradeCommonInput.IsPosKeepingOnBookDealer(SessionTools.CS, null))
                            {
                                isEnabled = false;
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPosKeepingDealer");
                            }
                            if (0 != TradeInput.AvailableQuantity)
                            {
                                isEnabled = false;
                                AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoAvailableQty");
                            }

                            if (isEnabled)
                            {
                                ExchangeTradedDerivativeContainer etd = new ExchangeTradedDerivativeContainer(SessionTools.CS,
                                    (IExchangeTradedDerivative)TradeInput.DataDocument.CurrentProduct.Product, TradeInput.DataDocument);
                                if (etd.SettlementMethod != FixML.Enum.SettlMethodEnum.PhysicalSettlement)
                                {
                                    isEnabled = false;
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoPhysicalSettlement");
                                }
                                if (etd.MaturityDate >= TradeCommonInput.CurrentBusinessDate)
                                {
                                    isEnabled = false;
                                    AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoMaturityDatePreviousBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate));
                                }
                            }

                        }
                        break;
                    default:
                        isHidden = true;
                        break;
                }

                if (false == isHidden)
                {
                    isEnabled &= (false == TradeInput.IsTradeRemoved);
                    if (TradeInput.IsTradeRemoved)
                        AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_TradeIsRemoved");

                    isEnabled &= TradeInput.TradeStatus.IsCurrentStUsedBy_Regular;
                    if (false == TradeInput.TradeStatus.IsCurrentStUsedBy_Regular)
                        AddActionDisabledReason(ref pDisabledReason, "Msg_TradeActionDisabledReason_NoRegularTrade");
                }
            }

            pIsHidden = isHidden;
            pIsEnabled = isEnabled;
        }


        /// <summary>
        ///  Retourne true si le contrôle de la page représenté par un CustomObject est non saisissable
        /// </summary>
        /// <param name="pCo"></param>
        /// <returns></returns>
        /// EG 20140826 Déplacé (en provenance de TradeCommonCapturePage)
        // EG 20151102 [21465] Add Prefix_denOption
        // EG 20180514 [23812] Report
        protected override bool IsControlModeConsult(CustomObject pCo)
        {

            bool ret = base.IsControlModeConsult(pCo);
            //
            if (false == ret)
            {
                if (Cst.Capture.IsModeRemoveOnly(InputGUI.CaptureMode))
                    ret = (false == pCo.ClientId.StartsWith(TradeCommonCustomCaptureInfos.CCst.Prefix_remove));
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionCancelation)
                    ret = (false == pCo.ClientId.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_correctionOfQuantity));
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment)
                    ret = (false == pCo.ClientId.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_denOption));
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise)
                    ret = (false == pCo.ClientId.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_denOption));
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon)
                    ret = (false == pCo.ClientId.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_denOption));

                // PM 20130822 [17949] Livraison Matif
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.UnderlyingDelivery)
                {
                    ret = (false == pCo.ClientId.StartsWith(TradeCustomCaptureInfos.CCst.Prefix_underlyingDelivery));
                }
                //
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer)
                {
                    ret = IsControlModeConsultPositionTransferSpecific(pCo.ClientId);
                }
                if (Cst.Capture.IsModeRemoveAllocation(InputGUI.CaptureMode))
                    ret = (false == pCo.ClientId.StartsWith(TradeCommonCustomCaptureInfos.CCst.Prefix_removeAllocation));

                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination)
                    ret = (false == pCo.ClientId.StartsWith("FxOptionEarlyTermination"));

                if (false == ret)
                {
                    // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
                    if (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode))
                        ret = pCo.IsLockedCreate;
                    else if (Cst.Capture.IsModeUpdate(InputGUI.CaptureMode))
                        ret = pCo.IsLockedModify;
                    else if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                        ret = pCo.IsLockedModifyPostEvts;
                    else if (Cst.Capture.IsModeUpdateFeesUninvoiced(InputGUI.CaptureMode))
                        ret = pCo.IsLockedModifyFeesUninvoiced;
                    else if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
                        ret = pCo.IsLockedAllocatedInvoice;
                }
            }
            return ret;
        }

        /// <summary>
        /// Poste un message, une requête,.. 
        /// <para>Doit être utilisé lorsque l'enregistrement du trade est ok</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdUnderlying"></param>
        /// EG 20120103 RMVALLOC
        protected override void SendRecordCapture(IDbTransaction pDbTransaction, int[] pIdUnderlying)
        {
            if (Cst.Capture.IsModePositionTransfer(InputGUI.CaptureMode) ||
                Cst.Capture.IsModeRemoveAllocation(InputGUI.CaptureMode))
            {
                SendPosRequest(pDbTransaction);
            }
            else
            {
                base.SendRecordCapture(pDbTransaction, pIdUnderlying);
            }
        }

        /// <summary>
        /// Valide et Enregistre  une négo (Création, Modification, Duplication, Annulation....)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Modify
        /// EG 20150624 [21151] Gestion PositionTransfer for EquitySecurityTransaction|DebtSecurityTransaction
        private void OnValidateRecordTrade(object sender, CommandEventArgs e)
        {
            bool isOk = ValidateCapture(sender, e);
            
            if (isOk)
            {
                if (Cst.Capture.IsModePositionTransfer(InputGUI.CaptureMode))
                {
                    #region Position Transfer
                    this.TradeInput.positionTransfer.CalcFeeRestitution(CSTools.SetCacheOn(SessionTools.CS));
                    /// EG 20150624 [21151]
                    if (TradeInput.DataDocument.CurrentProduct.IsExchangeTradedDerivative ||
                        TradeInput.DataDocument.CurrentProduct.IsEquitySecurityTransaction)
                    {
                        /// EG 20150624 [21151] Use IExchangeTradedBase
                        IExchangeTradedBase etd = (IExchangeTradedBase)TradeInput.DataDocument.CurrentProduct.Product;
                        if (TradeInput.positionTransfer.noteSpecified)
                        {
                            etd.TradeCaptureReport.TransferReasonSpecified = true;
                            etd.TradeCaptureReport.TransferReason.Value = TradeInput.positionTransfer.note;
                        }

                        etd.TradeCaptureReport.LastQtySpecified = true;
                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        etd.TradeCaptureReport.LastQty.DecValue = TradeInput.positionTransfer.quantity.DecValue;
                    }
                    else if (TradeInput.DataDocument.CurrentProduct.IsDebtSecurityTransaction)
                    {
                        /// EG 20150624 [21151] New
                        IDebtSecurityTransaction dst = (IDebtSecurityTransaction)TradeInput.DataDocument.CurrentProduct.Product;
                        DebtSecurityTransactionContainer _debtSecurityTransactionContainer = new DebtSecurityTransactionContainer(dst, TradeInput.DataDocument);
                        dst.Quantity.NumberOfUnitsSpecified = true;
                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        dst.Quantity.NumberOfUnits.DecValue = TradeInput.positionTransfer.quantity.DecValue;
                        ISecurityAsset securityAsset = _debtSecurityTransactionContainer.GetSecurityAssetInDataDocument();
                        if (null != securityAsset)
                        {
                            IMoney nominal = new SecurityAssetContainer(securityAsset).GetNominal(TradeInput.DataDocument.CurrentProduct.ProductBase);
                            dst.Quantity.NotionalAmount.Amount.DecValue = dst.Quantity.NumberOfUnits.DecValue * nominal.Amount.DecValue;
                        }
                    }
                    else if (TradeInput.DataDocument.CurrentProduct.IsReturnSwap)
                    {
                        IReturnSwap _returnSwap = (IReturnSwap)TradeInput.DataDocument.CurrentProduct.Product;
                        //ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(CS, _returnSwap, TradeInput.DataDocument);
                        ReturnSwapContainer _returnSwapContainer = new ReturnSwapContainer(_returnSwap, TradeInput.DataDocument);
                        // EG 20150920 [21314] Int (int32) to Long (Int64) 
                        // EG 20170127 Qty Long To Decimal
                        _returnSwapContainer.SetMainOpenUnits(TradeInput.positionTransfer.quantity.DecValue);
                    }
                    #endregion Position Transfer
                }

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                // Enregistrement du Trade (Appel à CheckAndRecord...) 
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                TradeCaptureGen.ErrorLevel lRet = RecordCapture();
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                if (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                    SetRequestTrackBuilderItemProcess(InputGUI.CaptureMode);

                // FI 20140805 [XXXXX] Utilisation de TradeCommonCaptureGen.IsRecordInSuccess car Spheres génère une exception lors de la saisie d'un swap et que la queue est mal paramétrée
                // If (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                {
                    OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                }
                else
                {
                    // En cas d'erreur
                    // En mode creation ou duplication, on efface pas l'écran (= On conserve les données avant enregistrement) 
                    // En mode Modification, on efface pas l'écran sauf si Pb de ROWVERSION
                    if ((false == Cst.Capture.IsModeNewOrDuplicateOrReflect(m_InputGUI.CaptureMode)) && (lRet == TradeCaptureGen.ErrorLevel.ROWVERSION_ERROR))
                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                }
            }
        }

        /// <summary>
        /// Contrôle la saisie et demande éventuelle de confirmation avant enregistrement (ex. demande de reset des frais présents).
        /// <para>Attention: l'enregistrement des trades mais également des actions sur trades passent ici (ex. Exercice d'une option)</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20171115 Upd CaptureSessionInfo
        /// EG 20201009 Changement de nom pour enum ClearFeeMode
        // EG 20210623 Lecture des arguments de validation du postBack associés aux boutons OK/CANCEL de la fenêtre de validation :
        // - OK = TRUE ou CheckValidationRule;TRUE ou CheckTradeInvoiced:TRUE ou SaveFee;TRUE
        // - CANCEL = FALSE ou CheckValidationRule;FALSE ou CheckTradeInvoiced:FALSE ou SaveFee;FALSE
        private bool ValidateCapture(object sender, CommandEventArgs e)
        {
            string target = "tblMenu$btnRecord";
            string CheckValidationRuleStep = "CheckValidationRule";
            // RD 20091231 [16814] Modification of Trade included in Invoice 
            string CheckTradeInvoicedStep = "CheckTradeInvoiced";
            // NOTE: Identification de l'étape "SaveFee", nécessaire à la sauvegarde des frais
            string SaveFeeStep = "SaveFee";
            //                                
            string eventTarget = Request.Params["__EVENTTARGET"];
            //
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

            // RD 20091231 [16814] Modification of Trade included in Invoice 
            bool isModeConfirm_CheckTradeInvoiced = (target == eventTarget) && (CheckTradeInvoicedStep == eventStep) && ("TRUE" == eventArgument);

            // On identifie s'il s'agit d'une réponse de l'utilisateur et le type de réponse de l'utilisateur (TRUE/FALSE),
            bool isModeConfirm_CheckValidationRule_Ok = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("TRUE" == eventArgument);
            bool isModeConfirm_CheckValidationRule_Cancel = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("FALSE" == eventArgument);
            bool isModeConfirm_CheckValidationRule = isModeConfirm_CheckValidationRule_Ok || isModeConfirm_CheckValidationRule_Cancel;

            // On identifie s'il s'agit d'une réponse de l'utilisateur et le type de réponse de l'utilisateur (TRUE/FALSE),
            // à la question de sauvegarder les frais calculés ou pas (SaveFeeStep).
            bool isModeConfirm_SaveFee_Ok = (target == eventTarget) && (SaveFeeStep == eventStep) && ("TRUE" == eventArgument);
            bool isModeConfirm_SaveFee_Cancel = (target == eventTarget) && (SaveFeeStep == eventStep) && ("FALSE" == eventArgument);
            bool isModeConfirm_SaveFee = isModeConfirm_SaveFee_Ok || isModeConfirm_SaveFee_Cancel;

            
            CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
            {
                user = SessionTools.User,
                session = SessionTools.AppSession,
                licence = SessionTools.License
            };
            
            bool isOk = true;
            if (isOk && (false == m_CaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode)))
                isOk = ValidatePage();

            if (isOk)
                // Pas de contrôle pour double postback sur Saisie full 
                isOk = IsScreenFullCapture || (false == PostBackForValidation());

            if (isOk)
            {
                if (Cst.Capture.TypeEnum.Customised == m_InputGUI.CaptureType)
                {
                    TradeInput.CustomCaptureInfos.SaveCapture(Page);
                    TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                    //
                    if (false == m_CaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode))
                    {
                        TradeInput.CustomCaptureInfos.FinaliseAll();
                        string errMsg = TradeInput.CustomCaptureInfos.GetErrorMessage();
                        isOk = StrFunc.IsEmpty(errMsg);
                        if (false == isOk)
                        {
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                            MsgForAlertImmediate = Ressource.GetString("Msg_ErrorsInForm") + Cst.CrLf + Cst.CrLf + errMsg;
                        }
                    }
                }
            }

            if (isOk)
            {
                // RD 20100720 Ne pas calculer les frais pour le mode Template
                if (false == m_CaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode))
                {
                    // RD 20120810 [18069] Ne pas calculer les frais pour le mode Modification partielle (sans régénération des événements)
                    if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                    {
                        if ((false == isModeConfirm_CheckTradeInvoiced) && (false == isModeConfirm_CheckValidationRule) &&
                            (false == isModeConfirm_SaveFee))
                        {
                            // S'il ne s'agit pas d'une confirmation quelconque, à l'enregistrement du Trade, 
                            // il faut re-calculer les frais et les comparer aux frais existants sur le Trade.
                            if (TradeInput.IsApplyFeeCalculation(GetFeeTarget()))
                                isOk = CheckFee(target, SaveFeeStep);
                        }
                        // Aucune modification de frais sur le trade 
                        else
                            isOk = true;
                    }
                    else
                    {
                        // RD 20091231 [16814] Modification of Trade included in Invoice 
                        if ((false == isModeConfirm_CheckTradeInvoiced) && (false == isModeConfirm_CheckValidationRule) && (false == isModeConfirm_SaveFee))
                        {
                            // S'il ne s'agit pas d'une confirmation quelconque, à l'enregistrement du Trade, il faut calculer les frais.
                            // On indique donc ici, via la variable SaveFeeStep, que l'on est en sauvegarde du Trade, et ce au sein de l'étape "SaveFee".
                            if (TradeInput.IsApplyFeeCalculation(GetFeeTarget()))
                            {
                                isOk = ProcessFee(eventTarget, eventStep, eventArgument, target, SaveFeeStep);
                            }
                        }
                        else if (isModeConfirm_SaveFee_Cancel)
                        {
                            // Nettoyage des frais affectés et arrêt de la sauvegarde du trade
                            TradeInput.ClearFee(GetFeeTarget(), TradeInput.ClearFeeMode.FromSchedule);
                            isOk = false;
                        }
                        //FI 20110928 [17583] Si validation des frais ok => Spheres® calcule les taxes
                        if (isOk)
                        {
                            if (TradeInput.IsApplyFeeCalculation(GetFeeTarget()))
                            {
                                // RD 20130205 [18389] Utiliser la bonne date de référence, selon s'il s'agit d'un ETD ou pas, d'une action sur trade ou pas.
                                // Ici le mieux est de voir comment on pourrait partager le même objet que le calcul des frais ci-dessus (voir méthode SetFee())
                                // Dans l'urgence, et en sachant que les taxes peuvent être calcuées sur des frais saisis manuellmenet, donc j'ai préféré dupliquer l'objet.
                                FeeRequest feeRequest = GetFeeRequest();
                                isOk = TradeInput.ProcessFeeTax(SessionTools.CS, null, GetFeeTarget(), feeRequest.DtReference);
                            }
                        }
                    }
                }
            }
            // Rq: En cliquant sur annuler l'enregistrement des frais, on annule aussi l'enregistrement du Trade
            //isOk = isOk && (false == isModeConfirm_SaveFee_Cancel);
            isOk = isOk && (false == isModeConfirm_SaveFee_Cancel) && (false == isModeConfirm_CheckValidationRule_Cancel);

            //
            // RD 20091231 [16814] Modification of Trade included in Invoice 
            // Le Trade est-il inclus dans une Facture?
            if (isOk)
            {
                if (Cst.Capture.IsModeUpdate(m_InputGUI.CaptureMode) &&
                    TradeInput.IsTradeInInvoice() &&
                    (false == isModeConfirm_CheckTradeInvoiced) &&
                    (false == isModeConfirm_CheckValidationRule))
                {
                    isOk = false;
                    string msgConfirm = Ressource.GetString("Msg_ValidationRule_TradeAlreadyInvoiced");
                    JavaScript.ConfirmStartUpImmediate(this, msgConfirm, ProcessStateTools.StatusWarning, target, CheckTradeInvoicedStep + ";TRUE", CheckTradeInvoicedStep + ";FALSE");
                }
            }

            if (isOk)
            {
                if ((false == TradeCommonCaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnlyAll(m_InputGUI.CaptureMode)))
                {
                    //20100750 FI [17064] Appel à SetDefaultValue avant de faire un checkValidationRule
                    TradeInput.SetDefaultValue(SessionTools.CS, null);
                }
            }

            // Validation Rules : Existe-il des Erreurs ??
            if (isOk)
            {
                if ((false == TradeCommonCaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnlyAll(m_InputGUI.CaptureMode)))
                {
                    CheckTradeValidationRule check = new CheckTradeValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode, captureSessionInfo.user);
                    isOk = check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckTradeValidationRule.CheckModeEnum.Error);
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
                if ((false == m_CaptureGen.IsInputIncompleteAllow(m_InputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnlyAll(m_InputGUI.CaptureMode)))
                {
                    if (false == isModeConfirm_CheckValidationRule)
                    {
                        CheckTradeValidationRule check = new CheckTradeValidationRule(m_CaptureGen.Input, m_InputGUI.CaptureMode, captureSessionInfo.user);
                        check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckTradeValidationRule.CheckModeEnum.Warning);
                        string msgValidationrules = check.GetConformityMsg();
                        isOk = (false == StrFunc.IsFilled(msgValidationrules));
                        if (false == isOk)
                        {
                            string msgConfirm = string.Empty;
                            msgConfirm += Ressource.GetString("Msg_ValidationRuleWarning");
                            msgConfirm += Cst.CrLf + Cst.CrLf + Cst.CrLf + msgValidationrules + Cst.CrLf;
                            msgConfirm += GetRessourceConfirmTrade();
                            string data = (Cst.Capture.IsModeNewOrDuplicateOrReflect(m_InputGUI.CaptureMode) ? string.Empty : this.TradeIdentifier);
                            msgConfirm = msgConfirm.Replace("{0}", data);
                            JavaScript.ConfirmStartUpImmediate(this, msgConfirm, ProcessStateTools.StatusWarning, target, CheckValidationRuleStep + ";TRUE", CheckValidationRuleStep + ";FALSE");
                        }
                    }
                }
            }
            return isOk;
        }

        /// <summary>
        /// Valide une action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Modify
        /// EG 20151102 [21465] Add
        // EG 20180514 [23812] Report
        private void OnValidateRecordAction(object sender, CommandEventArgs e)
        {

            if (Cst.Capture.IsModeRemove(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModePositionTransfer(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeRemoveAllocation(m_InputGUI.CaptureMode))
            {
                OnValidateRecordTrade(sender, e);
            }
            else
            {
                bool isOk = ValidateCapture(sender, e);
                if (isOk)
                {
                    // PM 20130830 [17949] Livraison
                    if (Cst.Capture.IsModeUnderlyingDelivery(m_InputGUI.CaptureMode))
                    {
                        if (null != TradeInput.underlyingDelivery)
                        {
                            Cst.ErrLevel errLevel = TradeInput.underlyingDelivery.RecordDeliveryStep(SessionTools.CS);
                            isOk = (Cst.ErrLevel.SUCCESS == errLevel);
                        }
                    }
                    else if (m_InputGUI.CaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination)
                    {
                        if (null == TradeInput.fxOptionEarlyTermination)
                            throw new NullReferenceException("fxOptionEarlyTermination is null");
                        isOk = TradeCommonCaptureGen.RecordFxOptionEarlyTermination(SessionTools.CS, InputGUI.CurrentIdScreen);
                    }
                    else
                    {
                        if (Cst.Capture.IsModePositionCancelation(m_InputGUI.CaptureMode))
                            TradeInput.positionCancel.CalcFeeRestitution(CSTools.SetCacheOn(SessionTools.CS));

                        // EG 20151102 [21465] New
                        if (Cst.Capture.IsModeDenActionOption(m_InputGUI.CaptureMode))
                            isOk = SendPosRequestDenouement(null);
                        else 
                            isOk = SendPosRequest(null);
                    }

                    // FI 20141021 [20350] call SetRequestTrackBuilderItemProcess
                    if (isOk)
                        SetRequestTrackBuilderItemProcess(InputGUI.CaptureMode);

                    if (isOk)
                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                }
            }
        }

        /// <summary>
        /// Envoi une demande vers POSREQUEST
        /// <para>Cette demande peut d'accompagner de l'envoi de message pour que le traitement s'applique dans la foulée</para>
        /// </summary>
        /// <returns></returns>
        /// FI 20130318 use of SessionTools.NewAppInstance()
        private bool SendPosRequest(IDbTransaction pDbTransaction)
        {
            //En mode Transfer de position, cette méthode est appelée avant que le nouveau Trade ne soit chargé
            //TradeInput ne contient plus le trade transféré mais le trade résultat du transfert
            IPosRequest posRequest = TradeInput.NewPostRequest(SessionTools.CS, pDbTransaction, m_InputGUI.CaptureMode);
            
            Cst.ErrLevelMessage errLevelMessage = PosKeepingTools.AddPosRequest(SessionTools.CS, pDbTransaction, posRequest, SessionTools.AppSession , null, null);
            
            bool isOk = (errLevelMessage.ErrLevel == Cst.ErrLevel.SUCCESS);
            
            // Abandon de la quantité restante
            if (isOk && (null != TradeInput.tradeDenOption) &&
                TradeInput.tradeDenOption.abandonRemaining.BoolValue)
            {
                // EG 20150920 [21314] Int (int32) to Long (Int64) 
                // EG 20170127 Qty Long To Decimal
                decimal remainQty = TradeInput.tradeDenOption.availableQuantity.DecValue - TradeInput.tradeDenOption.quantity.DecValue;
                if (0 < remainQty)
                {
                    TradeInput.tradeDenOption.posRequestType = Cst.PosRequestTypeEnum.OptionAbandon;
                    // EG 20170127 Qty Long To Decimal
                    TradeInput.tradeDenOption.quantity = new EFS_Decimal(remainQty);
                    TradeInput.tradeDenOption.availableQuantity = TradeInput.tradeDenOption.quantity;
                    TradeInput.tradeDenOption.abandonRemaining.BoolValue = false;
                    posRequest = TradeInput.NewPostRequest(SessionTools.CS, pDbTransaction, Cst.Capture.ModeEnum.OptionAbandon);
                    errLevelMessage = PosKeepingTools.AddPosRequest(SessionTools.CS, pDbTransaction, posRequest, SessionTools.AppSession, null, null);
                }
            }

            MsgForAlertImmediate = errLevelMessage.Message;
            ErrLevelForAlertImmediate = isOk ? ProcessStateTools.StatusNoneEnum : ProcessStateTools.StatusErrorEnum;
            //            
            return isOk;
        }
        // EG 20151102 [21465] New
        private bool SendPosRequestDenouement(IDbTransaction pDbTransaction)
        {
            Cst.ErrLevelMessage finalMessage = null;
            Cst.ErrLevel errMasterLevel = Cst.ErrLevel.SUCCESS;
            List<MQueueBase> listMQueue = new List<MQueueBase>();
            int nbMessageSendToService = 0;
            int nbMessageNotSendToService = 0;

            string headerMessage = string.Empty;
            string returnMessage = "Msg_PROCESS_GENERATE_DATA";
            string succesMessage = string.Empty;
            string errorMessage = string.Empty;
            try
            {
                List<IPosRequest> posRequest = TradeInput.NewPostRequest(SessionTools.CS, null, TradeInput.tradeDenOption.posRequestType,false);
                if ((null != posRequest) && (0 < posRequest.Count))
                {
                    AppSession session = SessionTools.AppSession;
                    string requestMessage = string.Empty;
                    posRequest.ForEach(item =>
                    {
                        Cst.ErrLevel errLevel = Cst.ErrLevel.SUCCESS;
                        if (0 < item.IdPR)
                        {
                            item.SetSource(session.AppInstance);
                            PosKeepingTools.UpdatePosRequest(SessionTools.CS, pDbTransaction, item.IdPR, item, session.IdA, null, null);
                        }
                        else
                        {
                            errLevel = PosKeepingTools.AddNewPosRequest(SessionTools.CS, pDbTransaction, out int newIdPR, item, session, null, null);
                            item.IdPR = newIdPR;
                        }

                        if (Cst.ErrLevel.SUCCESS != errLevel)
                        {
                            errMasterLevel = errLevel;
                            returnMessage = Ressource.GetString("Msg_ProcessIncomplete") + Cst.CrLf;
                            errorMessage += item.RequestMessage + Cst.HTMLHorizontalLine;
                        }
                        else
                        {
                            succesMessage += item.RequestMessage + Cst.HTMLHorizontalLine;
                            nbMessageSendToService++;
                            if (item.RequestMode == SettlSessIDEnum.Intraday)
                                listMQueue.Add(PosKeepingTools.BuildPosKeepingRequestMQueue(SessionTools.CS, item, null));
                            else
                                succesMessage += Ressource.GetString2("Msg_PROCESS_REQUESTMODE", item.RequestMode.ToString());
                        }
                    });

                    // Abandon de la quantité restante (ne peut se produire que si DenActionType = @new)
                    #region Abandon de la quantité restante
                    if ((Cst.ErrLevel.SUCCESS == errMasterLevel) && (null != TradeInput.tradeDenOption) && TradeInput.tradeDenOption.abandonRemaining.BoolValue)
                    {
                        IPosRequest posRequestAbandon = GetPosRequestAbandonWithRemainingQty(pDbTransaction, session);
                        if (null != posRequestAbandon) 
                        {
                            posRequest.Add(posRequestAbandon);
                            succesMessage += posRequestAbandon.RequestMessage + Cst.HTMLHorizontalLine;
                            nbMessageSendToService++;
                            if (posRequestAbandon.RequestMode == SettlSessIDEnum.Intraday)
                                listMQueue.Add(PosKeepingTools.BuildPosKeepingRequestMQueue(SessionTools.CS, posRequestAbandon, null));
                            else
                                succesMessage += Ressource.GetString2("Msg_PROCESS_REQUESTMODE", posRequestAbandon.RequestMode.ToString());

                        }
                    }
                    #endregion Abandon de la quantité restante

                    nbMessageNotSendToService = posRequest.Count - nbMessageSendToService;

                    // Postage des messages (ITD)
                    if (ArrFunc.IsFilled(listMQueue))
                    {
                        MQueueTaskInfo.SetAndSendMultipleThreadPool(SessionTools.CS, Cst.ProcessTypeEnum.POSKEEPREQUEST, Cst.ProductGProduct_FUT, m_InputGUI.CaptureMode.ToString(), session, listMQueue);
                    }

                }
            }
            catch (Exception ex)
            {
                returnMessage = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + ex.Message + Cst.CrLf;
                if (null != ex.InnerException)
                    returnMessage += ex.InnerException.Message;
                errMasterLevel = Cst.ErrLevel.FAILURE;
            }
            finally
            {
                //Envoi des messages Mqueue générés
                if (nbMessageSendToService > 0 && nbMessageNotSendToService > 0)
                {
                    errMasterLevel = Cst.ErrLevel.FAILUREWARNING;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_NOGENERATE_MULTI", nbMessageSendToService.ToString(),
                        returnMessage, nbMessageNotSendToService.ToString(), errorMessage + succesMessage);
                }
                else if (nbMessageSendToService > 0)
                {
                    errMasterLevel = Cst.ErrLevel.SUCCESS;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_GENERATE_DATA", nbMessageSendToService.ToString(), succesMessage);
                }
                else if (nbMessageNotSendToService > 0)
                {
                    errMasterLevel = Cst.ErrLevel.FAILURE;
                    returnMessage = Ressource.GetString2("Msg_PROCESS_NOGENERATE_DATA", nbMessageNotSendToService.ToString(), errorMessage);
                }
                finalMessage = new Cst.ErrLevelMessage(errMasterLevel, returnMessage);
            }
            MsgForAlertImmediate = finalMessage.Message;

            switch (errMasterLevel)
            {
                case Cst.ErrLevel.FAILURE:
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                    break;
                case Cst.ErrLevel.FAILUREWARNING:
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusWarningEnum;
                    break;
                default:
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusNoneEnum;
                    break;
            }
            return (errMasterLevel != Cst.ErrLevel.FAILURE);
        }


        #region GetPosRequestAbandonWithRemainingQty
        /// <summary>
        /// Création du POSREQUEST matérialisant l'abandon de la quantité restante en postion
        /// </summary>
        /// <returns></returns>
        private IPosRequest GetPosRequestAbandonWithRemainingQty(IDbTransaction pDbTransaction, AppSession pAppSession)
        {
            IPosRequest posRequestAbandon = null;
            // Quantité restante disponible pour abandon
            // EG 20170127 Qty Long To Decimal
            decimal remainQty = TradeInput.tradeDenOption.availableQuantity.DecValue - TradeInput.tradeDenOption.quantity.DecValue;
            if (0 < remainQty)
            {
                List<IPosRequest> lstPosRequest = TradeInput.NewPostRequest(SessionTools.CS, null, Cst.PosRequestTypeEnum.OptionAbandon, false);
                if (1 == lstPosRequest.Count)
                {
                    posRequestAbandon = lstPosRequest.First();
                    IPosRequestDetOption detail = posRequestAbandon.DetailBase as IPosRequestDetOption;
                    posRequestAbandon.RequestType = Cst.PosRequestTypeEnum.OptionAbandon;
                    posRequestAbandon.Qty = remainQty;
                    detail.AvailableQty = remainQty;
                    detail.PaymentFeesSpecified = false;
                    detail.PaymentFees = null;
                    detail.FeeCalculationSpecified = true;
                    detail.FeeCalculation = true;

                    if (0 < posRequestAbandon.IdPR)
                    {
                        posRequestAbandon.Qty += TradeInput.tradeDenOption.GetPreviousDenQty(Cst.PosRequestTypeEnum.OptionAbandon);
                        detail.AvailableQty = posRequestAbandon.Qty;
                        posRequestAbandon.SetSource(pAppSession.AppInstance);
                        PosKeepingTools.UpdatePosRequest(SessionTools.CS, pDbTransaction, posRequestAbandon.IdPR, posRequestAbandon, pAppSession.IdA, null, null);
                    }
                    else
                    {
                        PosKeepingTools.AddNewPosRequest(SessionTools.CS, pDbTransaction, out int newIdPR, posRequestAbandon, pAppSession, null, null);
                        posRequestAbandon.IdPR = newIdPR;
                    }
                }
            }
            return posRequestAbandon;
        }
        #endregion GetPosRequestAbandonWithRemainingQty

        /// <summary>
        /// Initialisation du Taux de financement (Funding) et Ratio de risque (Margin) à partir des barèmes.
        /// </summary>
        /// <param name="pEventTarget"></param>
        /// <param name="pEventStep"></param>
        /// <param name="pEventArgument"></param>
        /// <param name="pTarget"></param>
        /// <param name="pSaveFeeStep"></param>
        /// <returns></returns>
        private bool ProcessFundingAndMargin(string pEventTarget, string pEventStep, string pEventArgument, string pTarget, string pSaveFeeStep)
        {
            //Pl 20140722 A finaliser...
           return SetFundingAndMargin(pTarget, pSaveFeeStep); 
        }

        /// <summary>
        /// Calcul des frais à partir des barèmes.
        /// <para>
        /// Le calcul peut être interrompu, pour attendre la réponse de l'utilisateur.
        /// </para>
        /// </summary>
        /// <param name="pEventTarget"></param>
        /// <param name="pEventStep"></param>
        /// <param name="pEventArgument"></param>
        /// <param name="pTarget"></param>
        /// <returns>True si le calcul est effectué sans interruption</returns>
        /// FI 20180607 [XXXXX] Correction Bug suivant remontée par la recette 7.2 
        /// Le message "Recalcul des frais ?" revient systématiquement lorsque l'utilisateur répond à la question "Recalcul des frais ?"
        /// EG 20201009 Changement de nom pour enum ClearFeeMode
        private bool ProcessFee(string pEventTarget, string pEventStep, string pEventArgument, string pTarget, string pSaveFeeStep)
        {
            bool isContinue = true;
            
            // Identification de l'étape en cours, afin de savoir plus tard à quelle étape l'interruption a eu lieu
            string processFeeStep = "ProcessFee";
            
            // Est-ce qu'il s'agit de l'étape de sauvegarde
            bool isStepSave = StrFunc.IsFilled(pSaveFeeStep);
            bool isPostbackControl = (pTarget == pEventTarget);     //Check si le control à l'origine du Postback est le control en cours
            bool isStepResponse = (processFeeStep == pEventStep);   //Check s'il s'agit d'une réponse utilisateur à l'étape en cours 
            bool isConfirmResponse = ("TRUE" == pEventArgument);    //Check si la réponse utilisateur est bien un OUI
            //
            if (!(isPostbackControl && isStepResponse))
            {
                //On entre dans ce if() pour affichage d'un message à l'utilisateur (ex.: Click sur calculette, Record du Trade).
                //On y entre donc lorsque:
                // - il s'agit d'un 1er passage concernant les frais
                // - il s'agit d'une réponse, mais à un autre process 
                
                Cst.OriginOfFeeEnum originOfFee = TradeInput.GetOriginOfFee(GetFeeTarget());
                if (originOfFee != Cst.OriginOfFeeEnum.NoFee)
                {
                    if (isStepSave &&
                        ((originOfFee == Cst.OriginOfFeeEnum.FeeFromCalculateBySchedule)
                        ||
                        (originOfFee == Cst.OriginOfFeeEnum.FeeFromManualInputAndCalculateBySchedule))
                        )
                        return true;
                    
                    if (isContinue)
                    {
                        // Des frais existent, la question est alors posée à l'utilisateur concernant leur effacement
                        string feeMsg = string.Empty;
                        if ((originOfFee == Cst.OriginOfFeeEnum.FeeFromCalculateBySchedule)
                            ||
                            (originOfFee == Cst.OriginOfFeeEnum.FeeFromManualInputAndCalculateBySchedule))
                        {
                            //Existence de frais issus d'un barème (et éventuellement aussi de frais saisis manuellement)
                            feeMsg = Ressource.GetString("Msg_Fees_DeleteProcessed");
                        }
                        //
                        isContinue = true;
                        if (StrFunc.IsFilled(feeMsg))
                        {
                            // FI 20180502 [23935] Passage des paramètres à ConfirmStartUpImmediate 
                            //JavaScript.ConfirmStartUpImmediate(this, feeMsg, ProcessStateTools.StatusWarning, pTarget, processFeeStep + ";TRUE");
                            JavaScript.ConfirmStartUpImmediate(this, feeMsg, ProcessStateTools.StatusWarning, pTarget, processFeeStep + ";TRUE", processFeeStep + ";FALSE");

                            //On interrompt la suite des traitements en attendant la réponse de l'utilisateur à la Popup JS
                            isContinue = false;
                        }
                    }
                }
            }
            else if (isPostbackControl && isStepResponse)
            {
                isContinue =  isConfirmResponse;
            }

            if (isContinue)
            {
                // S'il ne s'agit pas de l'étape de sauvegarde, on reset les frais pour les recalculer.
                if (!isStepSave)
                {
                    //TradeInput.ResetFee(GetFeeTarget(), true, true);
                    TradeInput.ClearFee(GetFeeTarget(), TradeInput.ClearFeeMode.FromSchedule | TradeInput.ClearFeeMode.MissingData);
                }

                isContinue = SetFee(pTarget, pSaveFeeStep);
            }
            return isContinue;
        }

        /// <summary>
        /// Calcul des frais à partir des barèmes et comparaison avec ceux existants sur le trade.
        /// <para>
        /// Un warning est affiché en cas de différence.
        /// </para>
        /// <para>
        /// NB:Uniquement à l'étape de sauvegarde du trade.
        /// </para>
        /// </summary>
        /// <param name="pTarget"></param>
        /// <param name="pSaveFeeStep"></param>
        /// <returns>True s'il n'existe pas de différence</returns>        
        private bool CheckFee(string pTarget, string pSaveFeeStep)
        {
            bool isContinue = true;
            // Uniquement à l'étape de sauvegarde du trade
            if (StrFunc.IsFilled(pSaveFeeStep) && GetFeeTarget() == TradeInput.FeeTarget.trade)
            {
                // Re-calcul des frais
                FeeRequest feeRequest = GetFeeRequest();
                FeeProcessing fees = new FeeProcessing(feeRequest);
                fees.Calc(CSTools.SetCacheOn(SessionTools.CS), null);

                // Comparer les frais re-calculés avec ceux existants sur le Trade
                isContinue = TradeInput.CompareFee(fees.FeeResponse, out string infoMsg);
                infoMsg = infoMsg.TrimEnd(Cst.CrLf.ToCharArray());

                if (false == isContinue)
                {
                    // S'il existe des différences, alors on prépare un message de Warning, pour l'utilisateur                
                    if (StrFunc.IsFilled(infoMsg))
                    {
                        string headMsg = Ressource.GetString("Msg_Fees_RevuePaymentCheck");//Res: "Les frais recalculés sont différents de ceux actuellement en place sur le trade";
                        headMsg += Cst.CrLf + Cst.CrLf;
                        //
                        string footMsg = Cst.CrLf + Cst.CrLf + Ressource.GetString("Msg_Fees_SaveFee");//Res: "Continuer l'enregistrement du trade ?"
                        JavaScript.ConfirmStartUpImmediate(this, headMsg + infoMsg + footMsg, ProcessStateTools.StatusWarning, pTarget, pSaveFeeStep + ";TRUE", pSaveFeeStep + ";FALSE");
                    }
                }
            }

            return isContinue;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void SetInputSession()
        {
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[InputSessionID] = TradeInput;
            DataCache.SetData(InputSessionID, TradeInput);
        }

        /// <summary>
        /// Recherche du Taux de financement (Funding) et Ratio de risque (Margin) et alimentation de tradeInput
        /// <para>Aucune gestion d'exception pour cette méthode. Elle alimente MsgForAlertImmediate si une erreur inattendue se produit !</para>
        /// </summary>
        /// <param name="pTarget">not used yet</param>
        /// <param name="pSaveStep">not used yet</param>
        /// <returns></returns>
        private bool SetFundingAndMargin(string pTarget, string pSaveStep)
        {
            string errMsg = null;
            ProcessStateTools.StatusEnum errStatus = ProcessStateTools.StatusEnum.NA;
            Exception exception = null;

            bool ret = TradeCaptureGen.SetFundingAndMargin(GetFeeRequest(), ref errMsg, ref errStatus, ref exception);
            
            if (!ret)
            {
                MsgForAlertImmediate = errMsg;
                ErrLevelForAlertImmediate = errStatus;
                WriteLogException(exception);
            }
            return ret;
        }

        /// <summary>
        /// Calcul des frais et alimentation de tradeInput
        /// <para>Aucune gestion d'exception pour cette méthode. Elle alimente MsgForAlertImmediate si une erreur inattendue se produit !</para>
        /// </summary>
        /// <param name="pTarget"></param>
        /// <param name="pSaveFeeStep"></param>
        /// <returns></returns>
        private bool SetFee(string pTarget, string pSaveFeeStep)
        {
            bool ret = true;
            try
            {
                //isStepSave=true, s'il s'agit de l'étape de sauvegarde
                bool isStepSave = StrFunc.IsFilled(pSaveFeeStep);
                
                #region Get action
                // RD 20110208 / Obsolete
                //string action = GetPermissionAction(m_IdMenu, (m_InputGUI.Permission.isCreate ? PermissionEnum.Create.ToString() :
                //    (m_InputGUI.Permission.isModify ? PermissionEnum.Modify.ToString() :
                //    (m_InputGUI.Permission.isModifyPostEvts ? PermissionEnum.ModifyPostEvts.ToString() : string.Empty))));
                //
                #endregion
                
                FeeProcessing fees = null;
                bool isExistFeeCalculated = (TradeInput.IsApplyFeeCalculation(GetFeeTarget()));
                if (isExistFeeCalculated)
                {
                    if ((!isStepSave) && (false == Cst.Capture.IsModeAction(InputGUI.CaptureMode)))
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                    
                    #region Calcul des frais et alimentation dans le trade
                    FeeRequest feeRequest = GetFeeRequest();
                    
                    //FI 20110325 => Mise en cache pour accélerer les perfs
                    fees = new FeeProcessing(feeRequest);
                    fees.Calc(CSTools.SetCacheOn(SessionTools.CS), null);
                    
                    isExistFeeCalculated = ArrFunc.IsFilled(fees.FeeResponse);
                    #endregion
                }
                
                string infoMsg = string.Empty;
                if (isExistFeeCalculated)
                {
                    //------------------------------------------------
                    // Des frais issus de barème ont été calculés
                    //------------------------------------------------
                    //NB: Si on est à l'étape de sauvegarde, on prépare un message d'info ou d'erreur, pour confirmation par l'utilisateur
                    //    Si on n'est pas à l'étape de sauvegarde, on prépare un message d'info ou d'erreur pour l'utilisateur
                    infoMsg = TradeInput.SetFee(GetFeeTarget(), fees.FeeResponse, false, this.IsTrace).TrimEnd(Cst.CrLf.ToCharArray());
                    
                    if (isStepSave)
                    {
                        //On interrompt la suite des traitements en attendant la réponse de l'utilisateur à la Popup JS
                        ret = false;
                        //
                        // EG 20101020 Ticket:16954 Mise à jour des controls associés aux OPPs dans la saisie Full
                        if (Cst.FpML_ScreenFullCapture == ScreenName)
                            SetFullScreenWithOPP();
                    }
                }
                else
                {
                    //-----------------------------------------------
                    // Aucun frais issus de barème n'ont été calculés
                    //-----------------------------------------------
                    //NB: Si on n'est pas à l'étape de sauvegarde, on prépare un message d'info pour l'utilisateur
                    if (!isStepSave)
                        infoMsg = Ressource.GetString("Msg_Fees_NoFee"); //Res: "Aucun frais !"
                }
                
                #region Message pour l'utilisateur
                if (StrFunc.IsFilled(infoMsg))
                {
                    string headMsg = Ressource.GetString("Msg_Fees_RevuePayment");//Res: "Récapitulatif des frais issus des barèmes";
                    headMsg += Cst.CrLf + Cst.CrLf;
                    //
                    if (isStepSave)
                    {
                        //Message d'info ou d'erreur, pour confirmation par l'utilisateur
                        string footMsg = Cst.CrLf + Cst.CrLf + Ressource.GetString("Msg_Fees_SaveFee");//Res: "Continuer l'enregistrement du trade ?"
                        JavaScript.ConfirmStartUpImmediate(this, headMsg + infoMsg + footMsg, pTarget, pSaveFeeStep + ";TRUE", pSaveFeeStep + ";FALSE");
                    }
                    else
                    {
                        //Message d'info ou d'erreur
                        //JavaScript.AlertStartUpImmediate((PageBase)this, headMsg + infoMsg, false);
                        JavaScript.DialogStartUpImmediate(this, headMsg + infoMsg, false);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                MsgForAlertImmediate = Ressource.GetString("Msg_Fees_ProcessError") + Cst.CrLf + Cst.CrLf + ex.Message;
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                WriteLogException(ex);
                ret = false;
            }
            return ret;
        }

        /// <summary>
        /// Mise à jour des controls associés aux OPPs dans la saisie Full
        /// </summary>
        /// <returns></returns>
        /// EG 20101020 Ticket:16954 
        private bool SetFullScreenWithOPP()
        {
            Control plh = PlaceHolder;
            return SetFullScreenWithOPP(plh);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pControl"></param>
        /// <returns></returns>
        private bool SetFullScreenWithOPP(Control pControl)
        {
            bool isFound = false;

            if ((null != pControl) && (0 < pControl.Controls.Count))
            {
                foreach (Control ctrl in pControl.Controls)
                {
                    if (ctrl.GetType().Equals(typeof(OptionalItem)))
                    {
                        OptionalItem optItem = (OptionalItem)ctrl;
                        if (optItem.FldCapture.Name.StartsWith("otherPartyPayment"))
                        {
                            optItem.IsSpecified = TradeInput.CurrentTrade.OtherPartyPaymentSpecified;
                            if (ArrFunc.IsFilled(TradeInput.CurrentTrade.OtherPartyPayment))
                            {
                                if (TradeInput.CurrentTrade.OtherPartyPaymentSpecified)
                                {
                                    int tot = TradeInput.CurrentTrade.OtherPartyPayment.Length;
                                    for (int i = tot - 1; 0 <= i; i--)
                                    {
                                        IPayment payment = (IPayment)TradeInput.CurrentTrade.OtherPartyPayment[i];
                                        if (StrFunc.IsEmpty(payment.PayerPartyReference.HRef))
                                            ReflectionTools.AddOrRemoveItemInArray(TradeInput.CurrentTrade, "otherPartyPayment", true, 0);
                                    }
                                }
                            }
                            optItem.SaveClass();
                            isFound = true;
                            break;
                        }
                    }
                    isFound = SetFullScreenWithOPP(ctrl);
                    if (isFound)
                        break;
                }
            }
            return isFound;
        }


        /// <summary>
        /// Retourne le type de requête pour le calcul des frais en fonction du mode de saisie
        /// </summary>
        /// <returns></returns>
        /// FI 20121031 [18208] Gestion des exercices sur l'action abandon
        // EG 20151102 [21465] Upd 
        private FeeRequest GetFeeRequest()
        {
            FeeRequest feeRequest = null;

            switch (m_InputGUI.CaptureMode)
            {
                case Cst.Capture.ModeEnum.OptionExercise:
                case Cst.Capture.ModeEnum.OptionAssignment:
                case Cst.Capture.ModeEnum.OptionAbandon:

                    string idMenu;
                    if (m_InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise)
                        idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_EXE);
                    else if (m_InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment)
                        idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ASS);
                    else if (m_InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon)
                        idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_ABN);
                    else
                        throw new NotImplementedException(StrFunc.AppendFormat("Capture Mode [value:{0}] is not implemented", m_InputGUI.CaptureMode.ToString()));

                    // EG 20151102 [21465] 
                    if (m_CaptureGen.Input.tradeDenOption != null)
                    {
                        // RD 20170208 [22815]
                        //feeRequest = new FeeRequest(CSTools.SetCacheOn(SessionTools.CS), TradeInput, idMenu, m_InputGUI.CaptureMode,
                        //    Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity),
                        //    TradeInput.tradeDenOption.quantity.DecValue, "");
                        feeRequest = new FeeRequest(CSTools.SetCacheOn(SessionTools.CS), null, TradeInput, idMenu, m_InputGUI.CaptureMode,
                            new string[] { Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.Quantity), Enum.GetName(typeof(Cst.AssessmentBasisEnum), Cst.AssessmentBasisEnum.QuantityContractMultiplier) },
                            TradeInput.tradeDenOption.quantity.DecValue, "");
                    }

                    break;

                default:
                    break;
            }

            if (feeRequest == null)
            {
                feeRequest = new FeeRequest(CSTools.SetCacheOn(SessionTools.CS), null, TradeInput, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), m_InputGUI.CaptureMode);
            }

            //PL 20131008 Newness
            feeRequest.IsWithAuditMsg = this.IsTrace;

            return feeRequest;
        }

        /// <summary>
        /// Retourne le type d'élement impacté par des frais en fonction du mode de saisie
        /// </summary>
        /// <returns></returns>
        /// FI 20121031 [18208] Gestion des exercices sur l'action abandon
        private TradeInput.FeeTarget GetFeeTarget()
        {
            TradeInput.FeeTarget ret = TradeInput.FeeTarget.none;
            //
            if ((Cst.Capture.IsModeUpdateOrUpdatePostEvts(m_InputGUI.CaptureMode) ||
                Cst.Capture.IsModeNewOrDuplicateOrReflect(m_InputGUI.CaptureMode)))
            {
                ret = TradeInput.FeeTarget.trade;
            }
            else if (Cst.Capture.IsModeAction(m_InputGUI.CaptureMode))
            {
                // EG 20151102 [21465] Upd denOption instead of exeAssAbnOption
                if ((InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment) ||
                    (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon))
                    ret = TradeInput.FeeTarget.denOption;
                else if ((InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionCancelation) ||
                        (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon))
                    ret = TradeInput.FeeTarget.none;
                else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.RemoveReplace ||
                        (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer))
                    ret = TradeInput.FeeTarget.trade;
            }
            return ret;
        }


        /// <summary>
        /// Sur les trades => pas de Report 
        /// </summary>
        /// <param name="pTr"></param>
        protected override void AddToolBarReport(Panel pPnlParent)
        {

        }

        /// <summary>
        /// Retourne true, si au minimum une modification de cci est valide
        /// </summary>
        /// <param name="cciChangedArray">Liste des ccis modifiés</param>
        /// <returns></returns>
        private static bool IsChangeOk(CustomCaptureInfo[] cciChangedArray)
        {
            bool ret = true;
            /// FI 20180502 [23926] Utilisation de LINQ
            if (ArrFunc.IsFilled(cciChangedArray))
                ret = (null != cciChangedArray.Where(x => !x.HasError).FirstOrDefault());
            return ret;
        }


        /// <summary>
        /// Calcul d'un UTI Dealer (Party1) ou Clearer (Party2).
        /// <para>Warning: Calcul opéré uniquement sur un trade ALLOC.</para>
        /// </summary>
        /// <param name="eventTarget"></param>
        /// FI 20140206 [19564] CalcUTI 
        private void CalcUTI(string eventTarget)
        {
            try
            {
                SpheresIdentification tradeIdentification = null;
                if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(InputGUI.CaptureMode))
                {
                    tradeIdentification = TradeInput.Identification;
                }

                if (TradeCommonInput.IsTradeFoundAndAllocation)
                {
                    if ("OnUTIParty1Click" == eventTarget)      //Bouton UTI Party1 (Dealer)
                        TradeInput.CalcAndSetTradeUTI(SessionTools.CS, null, TypeSideAllocation.Dealer, tradeIdentification);
                    else if ("OnUTIParty2Click" == eventTarget) //Bouton UTI Party2 (Clearer)
                        TradeInput.CalcAndSetTradeUTI(SessionTools.CS, null, TypeSideAllocation.Clearer, tradeIdentification);

                    TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                    UpdatePlaceHolder();
                }
            }
            catch (Exception ex)
            {
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }
        
        
        /// <summary>
        ///  Retourne True s'il faut effacer les frais
        /// </summary>
        /// <returns></returns>
        /// FI 20180502 [23926] Add
        private Boolean IsFeeToReset()
        {
            Boolean ret = false;

            // 20110228 MF dynamic feetargeet related to the current mode
            TradeInput.FeeTarget feeTarget = GetFeeTarget();

            if ((feeTarget != TradeInput.FeeTarget.none))
            {
                // RD 20120810 [18069] Il n'y a plus de reset des frais en cas de modification partielle (sans régénération des événements)
                ret = (false == Cst.Capture.IsModeUpdatePostEvts(m_InputGUI.CaptureMode));

                if (ret)
                {
                    // 20110228 MF dynamic feetargeet related to the current mode
                    Cst.OriginOfFeeEnum originOfFee = TradeInput.GetOriginOfFee(feeTarget);
                    ret = (originOfFee == Cst.OriginOfFeeEnum.FeeFromCalculateBySchedule
                        || originOfFee == Cst.OriginOfFeeEnum.FeeFromManualInputAndCalculateBySchedule);
                }

                if (ret)
                {
                    //Purge des fees précédemment calculés lorsqu'une donnée du trade, n'appartenant pas à otherPartyPayment ou ADP , a changé
                    CustomCaptureInfo[] cciOtherThanFeeChanged = GetCciOtherThanFeeChanged();
                    ret = ArrFunc.IsFilled(cciOtherThanFeeChanged);
                }
            }
            return ret;
        }

        /// <summary>
        ///  Retourne un array constitué des ccis autre que liés au Frais ayant changé
        /// </summary>
        /// <param name="feeTarget"></param>
        /// <returns></returns>
        /// FI 20180502 [23926] Add
        private CustomCaptureInfo[] GetCciOtherThanFeeChanged()
        {
            

            TradeInput.FeeTarget feeTarget = GetFeeTarget();
            CustomCaptureInfo[] ret = TradeInput.CustomCaptureInfos.GetCciOtherThanFeeChanged(feeTarget);

            if (ArrFunc.IsFilled(ret))
            {
                /* FI 20240307 [XXXXX] supppression de la rustine du fait du nouveau comportement de GetCciHasChanged
                // [23670] Rustine pour ne pas prendre en compte les Cci TMZHeader_market1_orderEntered et TMZHeader_market1_executionDateTime
                List<CustomCaptureInfo> cciOtherThanFeeAndTMZChanged = ret.ToList();
                cciOtherThanFeeAndTMZChanged.RemoveAll(cci =>
                    TradeInput.CustomCaptureInfos.CciTrade.cciProduct.IsCciOrderEntered(cci) ||
                    TradeInput.CustomCaptureInfos.CciTrade.cciProduct.IsCciExecutionDateTime(cci));
                */

                ret = (CustomCaptureInfo[])ret.ToArray();
            }

            return ret;
        }

        /// <summary>
        ///  Retourne true s'il y a substitution d'un barème
        ///  <para>L'utilisateur vient de choisir un nouveau barème sur un frais présent</para>
        /// </summary>
        /// <param name="oNewFeeShedule">Retournele barème choisi</param>
        /// <param name="oPayment">retourne le payment OPP sur lequel une modification de barème est demandée</param>
        /// <param name="pEventTarget">Contrôle à l'origine du Postback de la page</param>
        /// <returns></returns>
        /// FI 20180502 [23926] Add
        private Boolean ISSubstituteFeeShedule(string pEventTarget, out IPayment oPayment, out SQL_FeeSchedule oNewFeeShedule)
        {
            Boolean ret = false;
            oPayment = null;
            oNewFeeShedule = null;

            if (StrFunc.IsFilled(pEventTarget) && pEventTarget.StartsWith(Cst.TXT))
            {
                string clientId_WithoutPrefix = pEventTarget.Replace(Cst.TXT, string.Empty);
                CciPayment[] cciOpp = ((CciTrade)TradeInput.CustomCaptureInfos.CciTrade).cciOtherPartyPayment;
                if (ArrFunc.IsFilled(cciOpp))
                {
                    CciPayment cciPayment = cciOpp.Where(x =>
                        x.IsCciClientId(CciPayment.CciEnumPayment.paymentSource_feeSchedule, clientId_WithoutPrefix)).FirstOrDefault();

                    ret = (cciPayment != null);
                    if (ret)
                    {
                        oPayment = (IPayment)cciPayment.Payment;
                        if (!(GetCciControl(pEventTarget) is TextBox txt))
                            throw new NullReferenceException(StrFunc.AppendFormat("{0} is not a textBox", pEventTarget));

                        if (StrFunc.IsFilled(txt.Text))
                        {
                            FeeRequest feeRequest = new FeeRequest
                            {
                                TradeInput = TradeInput,
                                Action = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade)
                            };

                            SQL_FeeSchedule sqNewFeeShedule = new SQL_FeeSchedule(CSTools.SetCacheOn(SessionTools.CS),
                                SQL_TableWithID.IDType.Identifier, txt.Text, SQL_Table.ScanDataDtEnabledEnum.Yes)
                            {
                                IdFeeIn = oPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheme).OTCmlId,
                                DtRefForDtEnabled = feeRequest.DtReference
                            };
                            if (sqNewFeeShedule.LoadTable(new string[] { "FEESCHEDULE.IDFEESCHEDULE, FEESCHEDULE.IDENTIFIER, FEESCHEDULE.DISPLAYNAME" }))
                                oNewFeeShedule = sqNewFeeShedule;
                        }

                    }
                }
            }
            
            return ret;
        }

        /// <summary>
        /// Modification de la ligne de frais {oppPayment}, remplacement du barème présents dans  {oppPayment} par le barème {sqlFeeSchedle} 
        /// <para>Retourne True si la subsitution s'est opérée avec succès</para>
        /// </summary>
        /// <param name="oppPayment">Représente la ligne de frais concernée par la substitution</param>
        /// <param name="sqlFeeSchedule">Représente le nouveau barême</param>
        /// <returns></returns>
        /// FI 20180502 [23926] Add
        private Boolean SubstituteFeeShedule(IPayment oppPayment, SQL_FeeSchedule sqlFeeSchedule)
        {
            Boolean ret = false;

            // Application de la substitution si l'utilisateur a effectivement changer de barème
            // Au cas ou (bizarrement) il conserve le même barème => Spheres® ne fait rien
            // Au cas ou (bizarrement) le barème est non reconnu (sqlFeeSchedule == null)  => Spheres ne fait rien
            ret = ((null != sqlFeeSchedule) &&
                (oppPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId != sqlFeeSchedule.Id));

            if (ret)
            {
                ISpheresIdSchemeId feeMatrixCurrent = oppPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme);
                ISpheresIdSchemeId feeScheduleCurrent = oppPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme);
                ISpheresIdSchemeId feeScheduleSys = oppPayment.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleSys);
                if (null == feeScheduleSys)
                {
                    feeScheduleSys = TradeInput.DataDocument.CurrentProduct.ProductBase.CreateSpheresId(1)[0];
                    feeScheduleSys.Scheme = Cst.OTCml_RepositoryFeeScheduleSys;
                    feeScheduleSys.OTCmlId = feeScheduleCurrent.OTCmlId;
                    feeScheduleSys.Value = feeScheduleCurrent.Value;
                }

                IPayment[] otherPartyPaymentSav = (IPayment[])TradeInput.DataDocument.CurrentTrade.OtherPartyPayment.Clone();

                TradeInput.ClearFee(GetFeeTarget(), TradeInput.ClearFeeMode.All);

                FeeRequest feeRequest = GetFeeRequest();
                feeRequest.SubstituteInfo = new FeeRequestSubstitute()
                {
                    source = new Pair<int, int>(feeMatrixCurrent.OTCmlId, feeScheduleSys.OTCmlId),
                    targetIdFeeSchedule = sqlFeeSchedule.Id
                };

                FeeProcessing fees = new FeeProcessing(feeRequest);
                fees.Calc(CSTools.SetCacheOn(SessionTools.CS), null);
                if (ArrFunc.IsFilled(fees.FeeResponse))
                    TradeInput.SetFee(GetFeeTarget(), fees.FeeResponse, false, this.IsTrace);

                ret = false;
                ArrayList lstNewOpp = new ArrayList();
                foreach (IPayment item in otherPartyPaymentSav)
                {
                    Boolean isToReplace = ((false == IsManualOpp(item)) &&
                        (item.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeMatrixScheme).OTCmlId ==
                        feeMatrixCurrent.OTCmlId) &&
                        (item.PaymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeScheduleScheme).OTCmlId ==
                        feeScheduleCurrent.OTCmlId) &&
                        TradeInput.DataDocument.OtherPartyPaymentSpecified);

                    if (isToReplace)
                    {
                        ret = true;

                        // Pour info sur les Strategie/Execution, il peut exister n enregistrements  avec le même couple {IdFeeMatrix, IdFeeShedule}
                        // (n enregistrements = autant d'enregistrements qu'il existe de jambes options )
                        // => ce cas se produit lorsque le paramétrage du barème ISAPPLYONALLLEGS=1
                        // Pour ces enregistrements le montant sur chaque ligne de frais est identique Car Spheres® considère le MainProduct
                        // => Donc ici on utilise un "raccourci" puisque l'on ne considère uniquement que otherPartyPayment[0]
                        IPayment payment = TradeInput.DataDocument.OtherPartyPayment[0];
                        payment.Id = oppPayment.Id;

                        payment.PaymentSource.Status = SpheresSourceStatusEnum.ScheduleForced;

                        ISpheresIdSchemeId spheresId = payment.PaymentSource.SpheresId.Where(x => StrFunc.IsEmpty(x.Scheme)).FirstOrDefault();
                        if (null == spheresId)
                        {
                            ReflectionTools.AddItemInArray(payment.PaymentSource, "spheresId", 0);
                            spheresId = payment.PaymentSource.SpheresId.Last();
                        }
                        spheresId.Scheme = feeScheduleSys.Scheme;
                        spheresId.OTCmlId = feeScheduleSys.OTCmlId;
                        spheresId.Value = feeScheduleSys.Value;

                        lstNewOpp.Add(payment);
                    }
                    else
                        lstNewOpp.Add(item);
                }

                TradeInput.DataDocument.CurrentTrade.OtherPartyPaymentSpecified = (lstNewOpp.Count > 0);
                if (TradeInput.DataDocument.CurrentTrade.OtherPartyPaymentSpecified)
                    TradeInput.DataDocument.CurrentTrade.OtherPartyPayment = (IPayment[])lstNewOpp.ToArray(typeof(IPayment));
            }

            if (false == ret)
            {
                // On rentre ici lorsqu'il n'y a pas de substitution de frais (Les frais sur le trades restent restent donc inchangés)
                // Ce cas de figure se produit lorsque le barème qui est substitué n'est plus en vigueur (Le barème substituant étant nécessairement en vigueur puisque selectionné via le grid) 
                // Dans ce contexte Spheres devrait affichier le message suivant
                /*
                « Attention, la substitution de barème est impossible, le barème substitué n’est plus n’est plus en vigueur.
                Il est recommander d’effectuer un recalcul des frais pour mettre en place les frais à partir des barèmes  et procéder ensuite, le cas échéant, à une substitution » 
                */
                // Du fait de la technomogie Ajax et que la publication de la page est effectuée via un input de type text, il est techniquement compliqué d'afficher ce message 
                //=> Donc pas de mmessage...l'utilisateur verra que sa substitution n'est pas prise en compte  
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payment"></param>
        /// <returns></returns>
        /// FI 20180502 [23926] Add
        private static Boolean IsManualOpp(IPayment payment)
        {
            if (null == payment)
                throw new ArgumentNullException("payment is null");

            return (false == payment.PaymentSourceSpecified) ||
                    (payment.PaymentSourceSpecified && (false == payment.PaymentSource.StatusSpecified));
        }

        #endregion Methods
    }
}

