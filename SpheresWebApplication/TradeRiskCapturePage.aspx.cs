#region Using Directives
using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.CCI;
using EFS.Import;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using System;
using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
//20071212 FI Ticket 16012 => Migration Asp2.0

// EG 20160404 Migration vs2013
#endregion Using Directives

namespace EFS.Spheres
{
    /// <summary>
    /// Page destinée à afficher les produits GPRODUCT=RISK
    /// </summary>
    public partial class TradeRiskCapturePage : TradeCommonCapturePage
    {
        /// <summary>
        /// Liste des Reports officiels utilisés pour l'évaluation du risque 
        /// </summary>
        /// FI 20140312 [19709] add PRISMA Report 
        private enum MarginRequirementReportEnum
        {
            /// <summary>
            /// Report brut (flux xml d'origine)
            /// </summary>
            // PL 20230613 New Name 
            [System.Xml.Serialization.XmlEnumAttribute(Name = "XML-CalculationElts")]
            XML,
            /// <summary>
            /// Report propre à la méthode TIMS IDEM (CCG)
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-MS22")]
            RP_MS22,
            /// <summary>
            /// Report propre à la méthode TIMS EUREX
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CC045")]
            RP_CC045,
            /// <summary>
            /// Report propre à la méthode TIMS EUREX
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CC050")]
            RP_CC050,

            /// <summary>
            /// Report CP040 propre à la méthode PRISMA EUREX
            /// <para>Market Risk Report</para>
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CP040")]
            RP_CP040,

            /// <summary>
            /// Report CP044 propre à la méthode PRISMA EUREX
            /// <para>Liquidity Risk Adjustment Report</para>
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CP044")]
            RP_CP044,

            /// <summary>
            /// Report CP046 propre à la méthode PRISMA EUREX
            /// <para>Aggregated Prisma Margins Repport</para>
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CP046")]
            RP_CP046,

            /// <summary>
            /// Report spécifique si deposit en brut
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-GROSS-IM")]
            RP_GROSS_IM,

            /// <summary>
            /// Report propre aux méthodes SPAN CME,London SPAN,SPAN C21 
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-PB-REQUIREMENTS")]
            RP_PB_REQUIREMENTS,

            /// <summary>
            /// Report propre à la méthode SPAN2
            /// </summary>
            // PM 20220111 [25617] Ajout RP_PB_REQUIREMENTS_SPAN2
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP_PB_REQUIREMENTS_SPAN2")]
            RP_PB_REQUIREMENTS_SPAN2,

            /// <summary>
            /// Report propre à les méthodes SPAN CME,London SPAN,SPAN C21 
            /// à l'identique du Performance Bond Report disponible dans Eurosys  
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-EUROSYS-PB")]
            RP_EUROSYS_PB,

            /// <summary>
            /// Report propre à les méthodes SPAN CME,London SPAN,SPAN C21 
            /// à l'identique du Performance Bond Summary Report disponible dans Eurosys  
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-EUROSYS-PB-SUMMARY")]
            RP_EUROSYS_PB_SUMMARY,

            /// <summary>
            /// Report propre à la méthode MEFFCOM2
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-MEFFCOM2")]
            RP_MEFFCOM2,
            /// <summary>
            /// Report propre à la méthode CBOE Margin
            /// </summary>
            [System.Xml.Serialization.XmlEnumAttribute(Name = "RP-CBOE-MARGIN")]
            RP_CBOE_MARGIN,
        }


        #region Members
        /// <summary>
        /// Membre chargé d'enregister le trade
        /// </summary>
        private TradeRiskCaptureGen _captureGen;
        /// <summary>
        /// 
        /// </summary>
        private TradeRiskInputGUI _inputGUI;
        /// <summary>
        /// 
        /// </summary>
        private TradeRiskHeaderBanner _tradeHeaderBanner;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Représente le trade Risk
        /// </summary>
        public TradeRiskInput TradeInput
        {
            get { return _captureGen.Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonCaptureGen TradeCommonCaptureGen
        {
            get { return (TradeCommonCaptureGen)_captureGen; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return (TradeCommonHeaderBanner)_tradeHeaderBanner; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInput TradeCommonInput
        {
            get { return _captureGen.TradeCommonInput; }
        }

        /// <summary>
        /// 
        /// </summary>
        public override TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return (TradeCommonInputGUI)_inputGUI; }
        }

        /// <summary>
        /// Retourne l'objet qui est affiché
        /// </summary>
        protected override ICustomCaptureInfos Object
        {
            get { return _captureGen.Input; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160804 [Migration TFS]
        protected override string XML_FilesPath
        {
            //get { return @"XML_Files\CustomTradeRisk"; }
            get { return @"CCIML\CustomTradeRisk"; }
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
        protected override string ObjectContext
        {
            get
            {
                return base.ObjectContext;
            }
        }

        // FI 20140930 [XXXXX] Mise en commentaire
        //protected override Cst.SQLCookieGrpElement ProductGrpElement
        //{
        //    get
        //    {
        //        //return Cst.SQLCookieGrpElement.SelRiskProduct;
        //        return this.TradeCommonInputGUI.GrpElement;
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        protected override CapturePageBase.TradeKeyEnum TradeKey
        {
            get
            {
                return TradeKeyEnum.TradeRisk;
            }
        }
        #endregion Accessors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start TradeRiskCapturePage.OnInit");

            base.OnInit(e);
            _captureGen = new TradeRiskCaptureGen();
            _captureGen.Input.InitDefault(m_DefaultParty, m_DefaultCurrency, m_DefaultBusinessCenter);

            AddAuditTimeStep("End TradeRiskCapturePage.OnInit");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            AddAuditTimeStep("Start TradeRiskCapturePage.OnLoad");

            if (false == IsPostBack)
            {
                //Si le menu n'est pas passé dans l'URL, utilisation du menu par défaut 
                string idMenu = Request.QueryString["IDMenu"];
                if (StrFunc.IsEmpty(idMenu))
                    idMenu = GetDefaultMenu();

                _inputGUI = new TradeRiskInputGUI(idMenu, SessionTools.User, XML_FilesPath);
                _inputGUI.InitializeFromMenu(CSTools.SetCacheOn(SessionTools.CS));
            }
            else
            {
                // FI 20200518 [XXXXX] Utilisation de DataCache
                //_inputGUI = (TradeRiskInputGUI)Session[InputGUISessionID];
                //m_FullCtor = (FullConstructor)Session[FullConstructorSessionID];
                //_captureGen.Input = (TradeRiskInput)Session[InputSessionID];

                _inputGUI = DataCache.GetData<TradeRiskInputGUI>(InputGUISessionID);
                m_FullCtor = DataCache.GetData<FullConstructor>(FullConstructorSessionID);
                _captureGen.Input = DataCache.GetData<TradeRiskInput>(InputSessionID);
            }

            base.OnLoad(e);

            AddAuditTimeStep("End TradeRiskCapturePage.OnLoad");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {

            AddAuditTimeStep("Start TradeRiskCapturePage.OnPrerender");
            //
            string eventTarget = Request.Params["__EVENTTARGET"];
            ((TextBox)CtrlTrade).Attributes["oldvalue"] = TradeIdentifier;
            //
            if (IsPostBack)
            {
                if ((Cst.Capture.TypeEnum.Customised == _inputGUI.CaptureType))
                {
                    if ((null != TradeInput.CustomCaptureInfos) && (Cst.Capture.IsModeInput(_inputGUI.CaptureMode)))
                    {
                        // FI 20200402 [XXXXX] Add test (false == IsDblPostbackFromAutocompleteControl)
                        if (IsLoadCcisFromGUI && (false == IsDblPostbackFromAutocompleteControl))
                        {
                            //
                            AddAuditTimeStep("Start Call Initialize_FromGUI");
                            TradeInput.CustomCaptureInfos.Initialize_FromGUI(this);
                            AddAuditTimeStep("End Call Initialize_FromGUI");
                            //
                            //Mise a jour du dataDocument
                            AddAuditTimeStep("Start Call Dump_ToDocument");
                            TradeInput.CustomCaptureInfos.Dump_ToDocument(0);
                            AddAuditTimeStep("End Call Dump_ToDocument");
                            //
                            //EG 20090107 Laisser entre Dump_ToDocument(0) et IsToSynchronizeWithDocument
                            if ("ExecFunction" == eventTarget)
                                ExecFunction();
                            //
                        }
                        //
                        //Mise à jour de l'IHM
                        //FI 20100427 [16970] alimentation de isLastPostUpdatePlaceHolder afin de détecter un éventuel douple post
                        IsLastPostUpdatePlaceHolder = TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument;
                        if (TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument)
                        {
                            //Refaire toutes les étapes pour pouvoir afficher les nouveautés du Document Fpml
                            TradeInput.CustomCaptureInfos.IsToSynchronizeWithDocument = false;
                            TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                            AddAuditTimeStep("Start Call UpdatePlaceHolder");
                            UpdatePlaceHolder();
                            AddAuditTimeStep("End Call UpdatePlaceHolder");
                        }
                        else
                        {
                            AddAuditTimeStep("Start Call CustomCaptureInfos.Dump_ToGUI");
                            TradeInput.CustomCaptureInfos.Dump_ToGUI(this);
                            AddAuditTimeStep("End Call CustomCaptureInfos.Dump_ToGUI");
                        }
                    }
                    //
                    //Bouton Add Or Delete Item
                    if ("OnAddOrDeleteItem" == eventTarget)
                    {
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnAddOrDeleteItem();
                    }
                    //
                    if (Cst.OTCml_ScreenBox == eventTarget)
                    {
                        TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                        OnZoomScreenBox();
                    }
                }
            }
            
            //Bouton zoom sur un section de l'écran Full (ie: Settlement Instruction)
            if (Cst.FpML_ScreenFullCapture == eventTarget)
            {
                TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                OnZoomFpml();
            }
            
            if (TradeInput.IsTradeFound && (Cst.Capture.IsModeNewCapture(_inputGUI.CaptureMode) ||
                                            Cst.Capture.IsModeUpdateOrUpdatePostEvts(_inputGUI.CaptureMode)))
            {
                #region Initialisation du status en fonction des actors et de leurs rôles
                // Initialisation réalisée ici sur le PreRender à cause de la saisie light
                ActorRoleCollection actorRoleNew = TradeCommonInput.DataDocument.GetActorRole(CSTools.SetCacheOn(SessionTools.CS));
                if ((null == _inputGUI.ActorRole) || _inputGUI.ActorRole.CompareTo(actorRoleNew) != 0)
                {
                    _inputGUI.ActorRole = actorRoleNew;
                    //Reinit des default user status (Check/Match) from ACTORROLE et ACTIONTUNING
                    TradeInput.ResetStUser();
                    TradeInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS), null);
                    TradeInput.InitStUserFromTuning(_inputGUI.ActionTuning);
                }
                #endregion
            }

            // FI 20200518 [XXXXX] Utilisation de DataCache
            //Session[InputGUISessionID] = _inputGUI;
            //Session[InputSessionID] = TradeInput;
            //Session[FullConstructorSessionID] = m_FullCtor;

            DataCache.SetData(InputGUISessionID,_inputGUI);
            DataCache.SetData(InputSessionID, TradeInput);
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
            AddAuditTimeStep("End TradeRiskCapturePage.OnPrerender");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140708 [20179] Modify: gestion de IsModeMatch
        protected override void OnValidate(object sender, CommandEventArgs e)
        {
            try
            {
                AddAuditTimeStep("Start TradeRiskCapturePage.OnValidate");

                // FI 20210622 [XXXXX] Add isValidateOk pour prendre le choix de l'utilsateur lorsqu'une demande de confirmation est demandée (ConfirmInputTrade) 
                //bool isValidateOk = (null == e.CommandArgument) || Convert.ToString(e.CommandArgument).Contains("TRUE");
                //
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
                                if (Cst.Capture.IsModeAction(_inputGUI.CaptureMode))
                                {
                                    OnValidateRecordAction(sender, e);
                                }
                                else if (Cst.Capture.IsModeMatch(_inputGUI.CaptureMode))
                                {
                                    OnValidateMatch(sender, e); // FI 20140708 [20179]
                                }
                                else
                                    OnValidateRecordTrade(sender, e);
                                break;
                            case Cst.Capture.MenuValidateEnum.Annul:
                                #region Annulation de la saisie en cours
                                if (null != ctrl)
                                {
                                    string eArgs;
                                    if (Cst.Capture.IsModeUpdateOrUpdatePostEvts(_inputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeDuplicate(_inputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeAction(_inputGUI.CaptureMode) ||
                                        Cst.Capture.IsModeMatch(_inputGUI.CaptureMode))
                                        eArgs = Cst.Capture.ModeEnum.Consult.ToString();
                                    else
                                        eArgs = Cst.Capture.ModeEnum.New.ToString();
                                    //
                                    OnMode(ctrl, new skmMenu.MenuItemClickEventArgs(eArgs));
                                }
                                #endregion Annulation de la saisie en cours
                                break;
                        }
                    }
                }
                AddAuditTimeStep("End TradeRiskCapturePage.OnValidate");
            }
            catch (Exception ex)
            {
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }
        #endregion Events

        #region Methods
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
        /// 
        /// </summary>
        /// <param name="pCell"></param>
        /// FI 20150601 [XXXX] Modify
        // EG 20160119 Refactoring Footer
        protected override void AddButtonsFooter(WebControl pCtrl)
        {
            bool isActionEnabled = SessionTools.IsActionEnabled;
            if (isActionEnabled)
            {
                pCtrl.Controls.Add(GetButtonFooter_XML());
                pCtrl.Controls.Add(GetButtonFooter_EXPORT());
            }
            pCtrl.Controls.Add(GetButtonFooter_CONFIRM());
            pCtrl.Controls.Add(GetButtonFooter_POST());

            if (isActionEnabled)
            {
                //PL 20230613 Hide LOG button (Double usage avec menu Etats\XML)
                //pCtrl.Controls.Add(GetButtonFooter_LOG());
                if (IsShowAdminTools)
                {
                    pCtrl.Controls.Add(GetButtonFooter_INPUTTRADE());
                }
            }
        }

        /// <summary>
        ///  Header 
        /// </summary>
        protected override void AddHeader()
        {
            _tradeHeaderBanner = new TradeRiskHeaderBanner(this, GUID, CellForm, TradeInput, _inputGUI,
                (false == Cst.Capture.IsModeRemoveOnly(_inputGUI.CaptureMode)));
            _tradeHeaderBanner.AddControls();
        }

        /// <summary>
        /// Retourne  les parametres nécessaires à l'importation d'un trade
        /// </summary>
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
            //
            //Template 
            param = new ImportParameter
            {
                name = TradeImportCst.templateIdentifier,
                value = TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(SessionTools.CS))
            };
            al.Add(param);
            //
            //screen
            param = new ImportParameter
            {
                name = TradeImportCst.screen,
                value = TradeCommonInputGUI.CurrentIdScreen
            };
            al.Add(param);
            //
            //DisplayName 
            param = new ImportParameter
            {
                name = TradeImportCst.displayName,
                value = TxtDisplayName
            };
            al.Add(param);
            //
            //Description 
            param = new ImportParameter
            {
                name = TradeImportCst.description,
                value = TxtDescription
            };
            al.Add(param);
            //
            //ExtlLink
            param = new ImportParameter
            {
                name = TradeImportCst.extlLink,
                value = TxtExtLink
            };
            al.Add(param);
            //
            //calcul de frais
            param = new ImportParameter
            {
                //PL 20130718 FeeCalculation Project
                //param.datatype = TypeData.TypeDataEnum.@bool.ToString();
                //param.name = TradeImportCst.isApplyFeeCalculation;
                //param.value = "false";
                datatype = TypeData.TypeDataEnum.@string.ToString(),
                name = TradeImportCst.feeCalculation,
                value = "Ignore"
            };
            al.Add(param);
            //
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

            //
            //Event Calculation
            param = new ImportParameter
            {
                datatype = TypeData.TypeDataEnum.@bool.ToString(),
                name = TradeImportCst.isPostToEventsGen,
                value = "false"
            };
            al.Add(param);
            //
            ImportParameter[] ret = null;
            if (ArrFunc.Count(al) > 0)
                ret = (ImportParameter[])al.ToArray(typeof(ImportParameter));
            //
            return ret;
        }

        /// <summary>
        /// Retourne le contenu de la toolbar action (Notepad,AttachedDoc,Event,et Tracker)
        /// </summary>
        /// <returns></returns>
        /// FI 20150601 [XXXX] Modify
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuAction()
        {
            // FI 20150601 [XXXX] Utilisation de SessionTools.IsActionEnabled
            bool isActionEnabled = SessionTools.IsActionEnabled;

            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(isActionEnabled ? 6 : 2);

            mnu[0] = GetMenuNotepad();
            mnu[1] = GetMenuAttachedDoc();
            if (isActionEnabled)
            {
                //UserWithLimitedRights 
                mnu[2] = GetMenuEvent();
                mnu[3] = GetMenuTrack();
                mnu[4] = GetMenuEar();
                mnu[5] = GetMenuAccDayBook();
            }
            return mnu;
        }

        /// <summary>
        /// Retourne le contenu de la toolbar Mode (création, consultation, Modification, Action utilisateur,  instrument)
        /// </summary>
        /// <returns></returns>
        /// 20161129 [RATP] Modify 
        /// FI 20170621 [XXXXX] Modify
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuMode()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(6);
            mnu[0] = GetMenuCreation();
            mnu[1] = GetMenuConsult();
            // 20161129 [RATP] Ajout du menu duplication
            mnu[2] = GetMenuDuplicate();
            mnu[3] = GetMenuModify();
            mnu[4] = GetMenuUserAction();
            mnu[5] = GetMenuInstrument();

            // FI 20170621 [XXXXX] Alimentation de MnuModeModify 
            MnuModeModify = mnu[3];

            return mnu;

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
        /// Retourne le menu Report
        /// </summary>
        /// <returns></returns>
        // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
        protected skmMenu.MenuItemParent GetMenuReport()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            //
            if (this.TradeCommonInput.IsTradeFound)
            {
                if (this.TradeCommonInput.Product.IsMarginRequirement)
                {
                    MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)this.TradeCommonInput.Product.Product);

                    //Get marginMethod
                    // PM 20160404 [22116] marginMethod devient un array
                    //Nullable<InitialMarginMethodEnum> marginMethod = null;
                    InitialMarginMethodEnum[] marginMethod = null;
                    if (marginRequirement.InitialMarginMethodSpecified)
                    {
                        marginMethod = marginRequirement.InitialMarginMethod;
                    }
                    else
                    {
                        //Recherche de la méthode de la chambre
                        int idACSS = marginRequirement.GetIdAClearingOrganisation(this.TradeCommonInput.DataDocument);
                        SQL_Css sqlCss = new SQL_Css(CSTools.SetCacheOn(SessionTools.CS), idACSS);
                        if (false == sqlCss.LoadTable(new string[] { "IDIMMETHOD" }))
                        {
                            throw new NotSupportedException(StrFunc.AppendFormat("CSS [id:{0]] is not found", idACSS.ToString()));
                        }

                        // PM 20160404 [22116] marginMethod devient un array et la méthode de calcul est maintenant dans la table IMMETHOD
                        if (sqlCss.IdIMMethod.HasValue)
                        {
                            string sqlQuery = "select im.INITIALMARGINMETH from dbo.IMMETHOD im where im.IDIMMETHOD = @IDIMMETHOD";
                            DataParameters dataParameters = new DataParameters();
                            dataParameters.Add(new DataParameter(SessionTools.CS, "IDIMMETHOD", DbType.Int32), sqlCss.IdIMMethod.Value);
                            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, dataParameters);

                            object initialMarginMethod;
                            initialMarginMethod = DataHelper.ExecuteScalar(CSTools.SetCacheOn(SessionTools.CS), CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                            if (initialMarginMethod != null)
                            {
                                string method = Convert.ToString(initialMarginMethod);
                                if (StrFunc.IsFilled(method))
                                {
                                    if (System.Enum.IsDefined(typeof(InitialMarginMethodEnum), method))
                                    {
                                        InitialMarginMethodEnum? initialMarginMethodEnum = (InitialMarginMethodEnum)Enum.Parse(typeof(InitialMarginMethodEnum), method);
                                        marginMethod = new InitialMarginMethodEnum[] { initialMarginMethodEnum.Value };
                                    }
                                }
                            }
                            // EG 20230505 [XXXXX] [WI617] data optional => controls for Trade template
                            else if (false == TradeCommonInput.TradeStatus.IsStEnvironment_Template)
                            {
                                throw new NotSupportedException(StrFunc.AppendFormat("Initial Margin Method [id:{0]] is not found", sqlCss.IdIMMethod.Value.ToString()));
                            }
                        }
                    }

                    // GetMenu from marginMethod
                    // PM 20160404 [22116] marginMethod devient un array
                    //if (marginMethod.HasValue)
                    //    ret = GetMenuReport(marginMethod.Value);
                    if (marginMethod != null)
                    {
                        ret = GetMenuReport(marginMethod);
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le Menu Report 
        /// <para>Ce menu est dynamique, il est fonction de la méthode de calcul du deposit</para>
        /// </summary>
        /// <param name="pInitialMarginMethod"></param>
        /// <returns></returns>
        /// FI 20140312 [19709] Add Report CP046 for PRISMA (Livraison V1 de prisma, seul le report CP046 est livré)  
        // PM 20160404 [22116] pInitialMarginMethod devient un array
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        private skmMenu.MenuItemParent GetMenuReport(InitialMarginMethodEnum[] pInitialMarginMethod)
        {
            bool isMarginGross = IsMarginCalculationGross();
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(1);
            //
            if (isMarginGross)
            {
                int mnuchildCount = 2;
                ret = new skmMenu.MenuItemParent(mnuchildCount);
                skmMenu.MenuItemParent mnuchild = ret[0];
                mnuchild.Enabled = true;
                mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_GROSS_IM);
                mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                mnuchild.eArgument = MarginRequirementReportEnum.RP_GROSS_IM.ToString();
            }
            else
            {
                foreach (InitialMarginMethodEnum initialMarginMethod in pInitialMarginMethod)
                {
                    skmMenu.MenuItemParent mnuMethod = null;
                    skmMenu.MenuItemParent mnuchild = null;
                    int mnuchildCount = 0;
                    switch (initialMarginMethod)
                    {
                        case InitialMarginMethodEnum.TIMS_IDEM:
                            mnuchildCount = 1;
                            mnuMethod = new skmMenu.MenuItemParent(mnuchildCount);
                            //
                            mnuchild = mnuMethod[0];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_MS22);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_MS22.ToString();
                            break;

                        case InitialMarginMethodEnum.TIMS_EUREX:
                            mnuchildCount = 2;
                            mnuMethod = new skmMenu.MenuItemParent(mnuchildCount);
                            //
                            mnuchild = mnuMethod[0];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_CC045);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_CC045.ToString();
                            //
                            mnuchild = mnuMethod[1];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_CC050);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_CC050.ToString();
                            break;

                        case InitialMarginMethodEnum.EUREX_PRISMA:
                            // PM 20151116 [21561] Ajout reports RBM
                            mnuchildCount = 2;
                            //mnuchildCount = 5;
                            mnuMethod = new skmMenu.MenuItemParent(mnuchildCount);
                            //
                            mnuchild = mnuMethod[0];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_CP046);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_CP046.ToString();
                            //
                            mnuchild = mnuMethod[1];
                            mnuchild.Enabled = false;
                            mnuchild.eText = "────────────────────";
                            //
                            #region RBM for Prisma (PL 20230705 Deprecated)
                            // PM 20151116 [21561] Ajout pour RBM
                            // PL 20230705 [*****] Masquer ces 2 menus dont les Reports affichent maintenant ERROR, car dépréciés.
                            //mnuchild = mnuMethod[2];
                            //mnuchild.Enabled = true;
                            //mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_CC045);
                            //mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            //mnuchild.eArgument = MarginRequirementReportEnum.RP_CC045.ToString();
                            ////
                            //mnuchild = mnuMethod[3];
                            //mnuchild.Enabled = true;
                            //mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_CC050);
                            //mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            //mnuchild.eArgument = MarginRequirementReportEnum.RP_CC050.ToString();
                            ////
                            //mnuchild = mnuMethod[4];
                            //mnuchild.Enabled = false;
                            //mnuchild.eText = "────────────────────";
                            #endregion RBM for Prisma (PL 20230705 Deprecated)
                            break;

                        case InitialMarginMethodEnum.SPAN_C21:
                        case InitialMarginMethodEnum.SPAN_CME:
                        case InitialMarginMethodEnum.London_SPAN:
                            mnuchildCount = 3;
                            mnuMethod = new skmMenu.MenuItemParent(mnuchildCount);
                            //
                            mnuchild = mnuMethod[0];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_PB_REQUIREMENTS);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_PB_REQUIREMENTS.ToString();
                            //
                            mnuchild = mnuMethod[1];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_EUROSYS_PB);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_EUROSYS_PB.ToString();
                            //
                            mnuchild = mnuMethod[2];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_EUROSYS_PB_SUMMARY);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_EUROSYS_PB_SUMMARY.ToString();
                            break;

                        case InitialMarginMethodEnum.SPAN_2_CORE:
                        case InitialMarginMethodEnum.SPAN_2_SOFTWARE:
                            // PM 20220111 [25617] Ajout SPAN_2_CORE & SPAN_2_SOFTWARE
                            mnuchildCount = 1;
                            mnuMethod = new skmMenu.MenuItemParent(mnuchildCount);
                            //
                            mnuchild = mnuMethod[0];
                            mnuchild.Enabled = true;
                            mnuchild.eText = GetReportName(MarginRequirementReportEnum.RP_PB_REQUIREMENTS_SPAN2);
                            mnuchild.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                            mnuchild.eArgument = MarginRequirementReportEnum.RP_PB_REQUIREMENTS_SPAN2.ToString();
                            break;
                        default:
                            break;
                    }
                    if (mnuMethod != null)
                    {
                        skmMenu.MenuItemParent addMenu = new skmMenu.MenuItemParent(ret.subItems.Length + mnuMethod.subItems.Length);
                        Array.Copy(mnuMethod.subItems, addMenu.subItems, mnuMethod.subItems.Length);
                        Array.Copy(ret.subItems, 0, addMenu.subItems, mnuMethod.subItems.Length, ret.subItems.Length);
                        ret = addMenu;
                    }
                }
            }
            //
            skmMenu.MenuItemParent mnuChildXML = ret[ret.subItems.Length - 1];
            mnuChildXML.Enabled = true;
            mnuChildXML.eText = GetReportName(MarginRequirementReportEnum.XML);
            mnuChildXML.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
            mnuChildXML.eArgument = MarginRequirementReportEnum.XML.ToString();

            ret.aID = "btnReport";
            ret.eImageUrl = "fas fa-icon fa-file-pdf";
            ret.eText = Ressource.GetString("Reports");
            //ret.aToolTip = "Editions des états de chambre";
            ret.Enabled = true;
            if (ArrFunc.Count(ret.subItems) == 1)
            {
                ret.eCommandName = Cst.Capture.MenuEnum.Report.ToString();
                ret.eArgument = ret[0].eArgument;
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarResultAction(Panel pPnlParent)
        {
            //ce menu n'existe pas pour les Risks
        }

        /// <summary>
        ///  Retourne true si le contrôle de la page représenté par un CustomObject est non saisissable
        /// </summary>
        /// <param name="pCo"></param>
        /// <returns></returns>
        /// FI 20140127 [19533] Seul le contrôle paymentAmount_amount est modifiable
        /// La devise n'est plus modifiable => Spheres® ne retrouve pas l'évènement associé si l'on change de devise
        protected override bool IsControlModeConsult(CustomObject pCo)
        {

            bool ret = base.IsControlModeConsult(pCo);
            //
            if ((ret == false) && (Cst.Capture.IsModeAction(_inputGUI.CaptureMode)))
            {
                //La correction d'un deposit consiste à mettre uniquement à jour les montants
                if (_inputGUI.CaptureMode == Cst.Capture.ModeEnum.Correction &&
                    this.TradeCommonInput.Product.IsMarginRequirement)
                {
                    ret = true;
                    if (pCo.ClientId.EndsWith(CciSimplePayment.CciEnum.paymentAmount_amount.ToString()))
                    {
                        ret = false;
                    }
                }
            }
            //
            return ret;
        }

        /// <summary>
        /// Retourne les attributs {pIsHidden} et {pIsEnabled} d'un menu ACTION
        /// <para></para>
        /// </summary>
        /// <param name="pMenuInput"></param>
        /// <param name="pIsHidden"></param>
        /// <param name="pIsEnabled"></param>
        /// <param name="pDisabledReason"></param>
        /// FI 20130308[] add parameter pDisabledReason (non géré ici pour l'instant)
        protected override void IsActionAuthorized(string pMenuInput, out bool pIsHidden, out bool pIsEnabled, out string pDisabledReason)
        {
            pDisabledReason = string.Empty;
            bool isHidden = false;
            bool isEnabled = true;

            if ((TradeInput.IsTradeFound) && IdMenu.IsMenuTradeRiskInput(pMenuInput))
            {
                ProductContainer product = TradeCommonInput.Product;
                Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(pMenuInput);

                switch (menu)
                {
                    //PL 20140930 
                    case IdMenu.Menu.InputTradeRisk_InitialMargin_Correction:   //Correction d'un Deposit
                    case IdMenu.Menu.InputTradeRisk_CashBalance_Correction:     //Correction d'un Cash-Balance
                    case IdMenu.Menu.InputTradeRisk_CashPayment_Correction:     //Correction d'un Versement
                        isEnabled = product.IsMarginRequirement;
                        isHidden = (false == product.IsMarginRequirement);
                        break;
                    default:
                        isHidden = true;
                        break;
                }
            }
            pIsHidden = isHidden;
            pIsEnabled = isEnabled;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20150923 Refactoring
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override WCToolTipLinkButton GetButtonFooter_LOG()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnLog", "L", "LOG", "btnLogTradeToolTip");
            btn.Click += new EventHandler(OnLogClick);
            return btn;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void RefreshLogButton()
        {
            ////
            //int idLog = 0;
            //if (null != this.TradeCommonInput.SQLLastTradeLog && (this.TradeCommonInput.SQLLastTradeLog.IdProcessL > 0))
            //    idLog = this.TradeCommonInput.SQLLastTradeLog.IdProcessL;
            ////
            //WCToolTipImageButton imgButton = FindButtonLog();
            //if (null != imgButton)
            //    imgButton.Visible = (idLog > 0);

            base.RefreshLogButton();

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLogClick(object sender, EventArgs e)
        {
            try
            {
                DisplayLogXml();
            }
            catch (Exception ex)
            {
                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                MsgForAlertImmediate = ex.Message;
                WriteLogException(ex);
            }
        }

        /// <summary>
        /// Valide une trade risk (Création, Modification, Duplication, Annulation....)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Modify
        private void OnValidateRecordTrade(object sender, CommandEventArgs e)
        {

            bool isOk = ValidateCapture(sender, e);

            if (isOk)
            {
                TradeCaptureGen.ErrorLevel lRet = RecordCapture();

                // FI 20141021 [20350] call SetRequestTrackBuilderItemProcess 
                if (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                    SetRequestTrackBuilderItemProcess(InputGUI.CaptureMode);

                if (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                {
                    OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                }
                else
                {
                    // En cas d'erreur
                    // En mode creation ou duplication, on efface pas l'écran (= On conserve les données avant enregistrement) 
                    // En mode Modification, on efface pas l'écran sauf si Pb de ROWVERSION
                    if ((false == Cst.Capture.IsModeNewCapture(_inputGUI.CaptureMode)) && (lRet == TradeCaptureGen.ErrorLevel.ROWVERSION_ERROR))
                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                }
            }
        }

        /// <summary>
        /// Valide une action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Modify
        private void OnValidateRecordAction(object sender, CommandEventArgs e)
        {
            if (Cst.Capture.IsModeRemove(_inputGUI.CaptureMode))
            {
                OnValidateRecordTrade(sender, e);
            }
            else
            {
                bool isOk = ValidateCapture(sender, e);
                //
                if (isOk)
                {
                    if (Cst.Capture.IsModeCorrection(InputGUI.CaptureMode))
                    {
                        TradeCaptureGen.ErrorLevel lRet = RecordCorrection();

                        // FI 20141021 [20350] call SetRequestTrackBuilderItemProcess 
                        if (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                            SetRequestTrackBuilderItemProcess(InputGUI.CaptureMode);


                        if (TradeCaptureGen.ErrorLevel.SUCCESS == lRet)
                        {
                            OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                        }
                        else
                        {
                            // En cas d'erreur
                            // On efface pas l'écran sauf si Pb de ROWVERSION
                            if (lRet == TradeCaptureGen.ErrorLevel.ROWVERSION_ERROR)
                                OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                        }
                    }
                }
            }
        }

        /// <summary>
        ///  Sauvegarde d'une correction de trade risk
        /// </summary>
        /// <returns></returns>
        /// FI 20170404 [23039] Modify
        private TradeCommonCaptureGen.ErrorLevel RecordCorrection()
        {

            string msgDet = string.Empty;
            
            TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            TradeCommonCaptureGenException errExc = null;
            
            if (false == TradeCommonInput.Product.IsMarginRequirement)
                throw new NotImplementedException(StrFunc.AppendFormat("Correction is not implemented for product [Identifier:{0}]", TradeCommonInput.SQLProduct.Identifier));

            try
            {
                CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo
                {
                    user = SessionTools.User,
                    session = SessionTools.AppSession,
                    licence = SessionTools.License
                };

                _captureGen.RecordMarginRequirementCorrection(SessionTools.CS, captureSessionInfo, InputGUI.CurrentIdScreen);
            }
            catch (TradeCommonCaptureGenException ex)
            {
                //Erreur reconnue
                errExc = ex;
                lRet = errExc.ErrLevel;
            }

            // FI 20170404 [23039]  null ds le paramètre trader et underlying
            string msg = _captureGen.GetResultMsgAfterCheckAndRecord(SessionTools.CS, errExc, InputGUI.CaptureMode, TxtIdentifier, null, null, true, out string msgDetail);

            if ((null != errExc) && (null != errExc.InnerException))
            {
                string msgException = StrFunc.AppendFormat("RecordCorrection[ErrLevel:{0}] Trade[Identifier:{1}] Detail[{2}]", errExc.ErrLevel.ToString(), TxtIdentifier, msgDetail);
                WriteLogException(new SpheresException2(MethodInfo.GetCurrentMethod().Name, msgException, errExc.InnerException));
            }
            
            DisplayMessageAfterRecord(lRet, msg, msgDet);
            
            //20090429 FI Si mode full affichage systématique du trade non conforme
            if (null != errExc)
            {
                if ((errExc.ErrLevel == TradeCommonCaptureGen.ErrorLevel.XMLDOCUMENT_NOTCONFORM) && IsScreenFullCapture)
                    DisplayTradeXml(true, false);
            }
            
            //Mise à jour de lret à SUCCESS lorsque le trade est correctement sauvegardé en base
            if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            
            return lRet;
        }

        /// <summary>
        /// Génère un script JS qui affiche le Log (XML) du deposit
        /// </summary>
        private void DisplayLogXml()
        {
            string dataXML = string.Empty;
            //
            if (TradeCommonInput.Product.IsMarginRequirement)
            {
                dataXML = LoadMarginTrack();
            }
            else
            {

            }
            //
            if (StrFunc.IsFilled(dataXML))
                DisplayXml("Log", this.TradeCommonInput.Identification.Identifier, dataXML);
        }

        /// <summary>
        ///  Retourne le flux détail à l'origine du trade "Deposit" (product MarginRequirement)     
        /// </summary>
        /// FI 20111201 S'il n'existe pas de Idlog ds TRADETRAIL alors Spheres® lit le MARGINTRACK le plus récent
        private string LoadMarginTrack()
        {
            string ret = string.Empty;

            if (false == TradeCommonInput.Product.IsMarginRequirement)
                throw new Exception("Product is not MarginRequirement");
            //
            if (this.TradeCommonInput.IsTradeFound)
            {
                int idLog = this.TradeCommonInput.SQLLastTradeLog.IdProcessL;
                int idT = this.TradeCommonInput.Identification.OTCmlId;
                //
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), idT);
                if (idLog > 0)
                    dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.ID), idLog);
                //
                StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                sql += "tmt.TRADEXML" + Cst.CrLf;
                sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEMARGINTRACK + " tmt " + Cst.CrLf;
                sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARGINTRACK + " mt on mt.IDMARGINTRACK=tmt.IDMARGINTRACK" + Cst.CrLf;
                sql += SQLCst.WHERE + "tmt.IDT=@IDT" + Cst.CrLf;
                if (idLog > 0)
                    sql += "and mt.IDPROCESS_L=@ID" + Cst.CrLf;
                else
                    sql += SQLCst.ORDERBY + "tmt.IDMARGINTRACK desc" + Cst.CrLf;
                //
                string query = DataHelper.GetSelectTop(SessionTools.CS, sql.ToString(), 1);
                QueryParameters qryParameters = new QueryParameters(SessionTools.CS, query, dp);
                //
                IDataReader dr = null;
                try
                {
                    dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    if (dr.Read())
                        ret = dr[0].ToString();
                }
                catch (Exception) { throw; }
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
            return ret;
        }

        /// <summary>
        /// Contrôle la saisie, demande éventuellement des confirmations
        /// <para>Attention L'enregistrement des trades et des actions passent ici (Exemple Exercice)</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20171115 Upd CaptureSessionInfo
        // EG 20210623 Lecture des arguments de validation du postBack associés aux boutons OK/CANCEL de la fenêtre de validation :
        // - OK = TRUE ou CheckValidationRule;TRUE
        // - CANCEL = FALSE ou CheckValidationRule;FALSE
        private bool ValidateCapture(object sender, CommandEventArgs e)
        {
            //
            string target = "tblMenu$btnRecord";
            string CheckValidationRuleStep = "CheckValidationRule";
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
            bool isModeConfirm_CheckValidationRule_Ok = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("TRUE" == eventArgument);
            bool isModeConfirm_CheckValidationRule_Cancel = (target == eventTarget) && (CheckValidationRuleStep == eventStep) && ("FALSE" == eventArgument);
            bool isModeConfirm_CheckValidationRule = isModeConfirm_CheckValidationRule_Ok || isModeConfirm_CheckValidationRule_Cancel;

            bool isOk = true;
            CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo
            {
                user = SessionTools.User,
                session = SessionTools.AppSession,
                licence = SessionTools.License
            };

            if ((isOk) && (false == _captureGen.IsInputIncompleteAllow(_inputGUI.CaptureMode)))
                isOk = ValidatePage();

            if (isOk)
                // Pas de contrôle pour double postback sur Saisie full 
                isOk = IsScreenFullCapture || (false == PostBackForValidation());

            if (isOk)
            {
                if (Cst.Capture.TypeEnum.Customised == _inputGUI.CaptureType)
                {
                    TradeInput.CustomCaptureInfos.SaveCapture(Page);
                    TradeInput.CustomCaptureInfos.CciTrade.SetPartyInOrder();
                    if (false == _captureGen.IsInputIncompleteAllow(_inputGUI.CaptureMode))
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
            //
            if (isOk)
            {
                if ((false == _captureGen.IsInputIncompleteAllow(_inputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnly(_inputGUI.CaptureMode)))
                {
                    TradeInput.SetDefaultValue(SessionTools.CS, null );
                }
            }
            //
            // Validation Rules : Existe-il des Erreurs ??
            isOk = isOk && (false == isModeConfirm_CheckValidationRule_Cancel);

            if (isOk)
            {
                if ((false == _captureGen.IsInputIncompleteAllow(_inputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnly(_inputGUI.CaptureMode)))
                {
                    //
                    CheckTradeRiskValidationRule check = new CheckTradeRiskValidationRule(_captureGen.Input, _inputGUI.CaptureMode, captureSessionInfo.user);
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
                if ((false == _captureGen.IsInputIncompleteAllow(_inputGUI.CaptureMode)) &&
                    (false == Cst.Capture.IsModeRemoveOnly(_inputGUI.CaptureMode)))
                {
                    if (false == isModeConfirm_CheckValidationRule)
                    {
                        // RD 20110222 Pour charger les Assets créés en Mode Transactionnel.
                        CheckTradeRiskValidationRule check = new CheckTradeRiskValidationRule(_captureGen.Input, _inputGUI.CaptureMode, captureSessionInfo.user);
                        check.ValidationRules(CSTools.SetCacheOn(SessionTools.CS), null, CheckTradeValidationRule.CheckModeEnum.Warning);
                        string msgValidationrules = check.GetConformityMsg();
                        isOk = (false == StrFunc.IsFilled(msgValidationrules));
                        if (false == isOk)
                        {
                            string msgConfirm = string.Empty;
                            msgConfirm += Ressource.GetString("Msg_ValidationRuleWarning");
                            msgConfirm += Cst.CrLf + Cst.CrLf + Cst.CrLf + msgValidationrules + Cst.CrLf;
                            msgConfirm += GetRessourceConfirmTrade();
                            string data = (Cst.Capture.IsModeNewOrDuplicate(_inputGUI.CaptureMode) ? string.Empty : this.TradeIdentifier);
                            msgConfirm = msgConfirm.Replace("{0}", data);
                            JavaScript.ConfirmStartUpImmediate(this, msgConfirm, ProcessStateTools.StatusWarning, target, CheckValidationRuleStep + ";TRUE", CheckValidationRuleStep + ";FALSE");
                        }
                    }
                }
            }
            return isOk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnReport(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            try
            {
                if (e.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                {
                    Nullable<MarginRequirementReportEnum> marginRequirementReport = null;
                    if (Enum.IsDefined(typeof(MarginRequirementReportEnum), e.Argument))
                        marginRequirementReport = (MarginRequirementReportEnum)Enum.Parse(typeof(MarginRequirementReportEnum), e.Argument);
                    //
                    if (marginRequirementReport.HasValue)
                    {
                        if (TradeInput.Product.IsMarginRequirement && TradeInput.IsTradeFound)
                            DisplayMarginRequirementReport(marginRequirementReport.Value);
                    }
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
        /// Affiche le report {pReport} après transformation XSL
        /// </summary>
        /// <param name="pReport"></param>
        /// FI 20140312 [17861] add RP_CP040, RP_CP044 , RP_CP046
        /// FI 20160804 [Migration TFS] Modify
        private void DisplayMarginRequirementReport(MarginRequirementReportEnum pReport)
        {
            //
            string dataXML = LoadMarginTrack();
            //
            if (StrFunc.IsFilled(dataXML))
            {
                // EG 20160404 Migration vs2013
                string key = GetReportName(pReport);
                string WindowID = FileTools.GetUniqueName(key, TradeCommonInput.Identifier);
                string physicalPath = SessionTools.TemporaryDirectory.MapPath("MarginRequirementReport");
#if DEBUG
            FileTools.WriteStringToFile(dataXML, physicalPath + @"\" + WindowID + "_data.xml");
#endif
                // FI 20160804 use of GUIOutput Folder
                string xslFile = string.Empty;
                switch (pReport)
                {
                    case MarginRequirementReportEnum.RP_MS22:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\TIMS_CCG_MS22.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_CC045:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\TIMS_EUREX_CC045.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_CC050:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\TIMS_EUREX_CC050.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_CP040:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\PRISMA_EUREX_CP040.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_CP044:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\PRISMA_EUREX_CP044.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_CP046:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\PRISMA_EUREX_CP046.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_GROSS_IM:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\GROSS_InitialMargins.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_PB_REQUIREMENTS:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\SPAN_PbReq.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_PB_REQUIREMENTS_SPAN2:
                        // PM 20220111 [25617] Ajout RP_PB_REQUIREMENTS_SPAN2
                        xslFile = @"~\GUIOutput\MarginRequirementReport\SPAN2_PbReq.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_EUROSYS_PB:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\SPAN_Eurosys_Pb.xslt";
                        break;
                    case MarginRequirementReportEnum.RP_EUROSYS_PB_SUMMARY:
                        xslFile = @"~\GUIOutput\MarginRequirementReport\SPAN_Eurosys_Pb_Summary.xslt";
                        break;
                    case MarginRequirementReportEnum.XML:
                        break;
                    default:
                        throw new NotImplementedException(StrFunc.AppendFormat("Report [{0}] is not implemented", pReport.ToString()));
                }
                //
                if (StrFunc.IsFilled(xslFile))
                    SessionTools.AppSession.AppInstance.SearchFile2(CSTools.SetCacheOn(SessionTools.CS), xslFile, ref xslFile);
                //
                string result = dataXML;
                string outputType = "xml";
                // EG 20160404 Migration vs2013
                //byte[] binaryResult = null;
                //
                if (StrFunc.IsFilled(xslFile))
                {
                    //Transformation xslt                
                    outputType = XSLTTools.GetOutputStreamType(xslFile);
                    result = XSLTTools.TransformXml(new StringBuilder(dataXML), xslFile, null, null);
#if DEBUG
                    FileTools.WriteStringToFile(result, physicalPath + @"\" + WindowID + "_transform.xml");
#endif

                }
                // EG 20160404 Migration vs2013
                //if (outputType == "pdf")
                //{
                //transformation Fop
                // EG 20160308 Migration vs2013
                //binaryResult = FopEngine_V2.TransformToByte(CSTools.SetCacheOn(SessionTools.CS), result, SessionTools.TemporaryDirectory.Path);
                //}


                //
                //string WindowID = FileTools.GetUniqueName(key, TradeCommonInput.Identifier);
                //string physicalPath = SessionTools.TemporaryDirectory.MapPath("MarginRequirementReport");
                string write_File = physicalPath + @"\" + WindowID + "." + outputType;
                if (outputType == "pdf")
                {
                    // EG 20160404 Migration vs2013
                    //FileTools.WriteBytesToFile(binaryResult, write_File, true);
                    FopEngine.WritePDF(CSTools.SetCacheOn(SessionTools.CS), result, physicalPath, write_File);
                }
                else
                {

                    FileTools.WriteStringToFile(result.ToString(), write_File);
                }
                //
                //ouverture du fichier 
                string relativePath = SessionTools.TemporaryDirectory.Path + "MarginRequirementReport";
                string open_File = relativePath + @"/" + WindowID + "." + outputType;
                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }
        }

        /// <summary>
        /// Retourne le nom du report
        /// <para>L'enum n'autorise pas certains caractère (ex:"-"), Ds ce cas il faut ajouter un XmlEnumAttribute dont la property Name vaut le vrai nom</para>
        /// </summary>
        /// <param name="pReport"></param>
        /// <returns></returns>
        // FI 20171025 [23533] Modify
        // PL 20230613 Use ressource for "XML" menu
        private static string GetReportName(MarginRequirementReportEnum pReport)
        {
            //Ecriture du fichier 
            // FI 20171025 [23533] Appel à ReflectionTools.GetXmlEnumAttributName
            string ret = ReflectionTools.GetXmlEnumAttributName(typeof(MarginRequirementReportEnum), pReport.ToString());

            if (StrFunc.IsEmpty(ret))
                ret = pReport.ToString();

            ret = Ressource.GetString(ret, true);

            return ret;
        }

        /// <summary>
        /// Retourne un menu vide
        /// <para>Il n'y a pas de menu CashFlow sur les trade RISK</para>
        /// <para>La consultation 3btns Trade\CASHFLOWS.xml ne traite que les trades de marché</para>
        /// <para>A faire évoluer si la consultation gère les trades RISK</para>
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuCashFlow()
        {
            return new skmMenu.MenuItemParent(0);
        }


        /// <summary>
        ///  Retourne true si le deposit brut a été calculé  
        ///  <para>Lecture dans le log du traitement, soit TRADEMARGINTRACK</para>
        /// </summary>
        private bool IsMarginCalculationGross()
        {
            bool ret = false;

            if (false == TradeCommonInput.Product.IsMarginRequirement)
                throw new Exception("Product is not MarginRequirement");
            //
            //PM 20140317 [17861] Lecture de l'indicateur de calcul Gross directement dans le trade si celui-ci est présent
            MarginRequirementContainer marginRequirement = new MarginRequirementContainer((IMarginRequirement)this.TradeCommonInput.Product.Product);
            if (marginRequirement.IsGrossMarginSpecified)
            {
                ret = marginRequirement.IsGrossMargin.BoolValue;
            }
            else
            {

                if (this.TradeCommonInput.IsTradeFound)
                {
                    int idLog = this.TradeCommonInput.SQLLastTradeLog.IdProcessL;
                    int idT = this.TradeCommonInput.Identification.OTCmlId;
                    //
                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), idT);
                    if (idLog > 0)
                        dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.ID), idLog);
                    //
                    StrBuilder sql = new StrBuilder(SQLCst.SELECT);
                    sql += "1" + Cst.CrLf;
                    sql += SQLCst.FROM_DBO + Cst.OTCml_TBL.TRADEMARGINTRACK + " tmt " + Cst.CrLf;
                    sql += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.MARGINTRACK + " mt on mt.IDMARGINTRACK=tmt.IDMARGINTRACK" + Cst.CrLf;
                    sql += SQLCst.WHERE + "tmt.IDT=@IDT" + Cst.CrLf;
                    sql += SQLCst.AND + DataHelper.GetSQLXQuery_ExistsNode(SessionTools.CS, "TRADEXML", "tmt", @"(//efs:marginRequirementOffice/efs:marginCalculation/efs:grossMargin)", OTCmlHelper.GetXMLNamespace_3_0(SessionTools.CS));
                    if (idLog > 0)
                        sql += "and mt.IDPROCESS_L=@ID" + Cst.CrLf;
                    else
                        sql += SQLCst.ORDERBY + "tmt.IDMARGINTRACK desc" + Cst.CrLf;
                    //
                    string query = DataHelper.GetSelectTop(SessionTools.CS, sql.ToString(), 1);
                    QueryParameters qryParameters = new QueryParameters(SessionTools.CS, query, dp);
                    //
                    object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
                    ret = (null != obj);
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne le menu par défaut 
        /// </summary>
        /// <returns></returns>
        /// FI 20140930 [XXXXX] Add
        private string GetDefaultMenu()
        {
            string ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_InitialMargin);

            string pkValue = Request.QueryString["PKV"];
            if (StrFunc.IsFilled(pkValue) && IntFunc.IsPositiveInteger(pkValue))
            {
                string query = @"
select p.FAMILY from dbo.PRODUCT p
inner join dbo.INSTRUMENT i on i.IDP = p.IDP
inner join dbo.TRADE t on t.IDI = i.IDI  and t.IDT = @IDT";
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), IntFunc.IntValue(pkValue));

                QueryParameters queryParameters = new QueryParameters(SessionTools.CS, query, dp);

                Object result = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter());
                if (null != result)
                {
                    switch (result.ToString())
                    {
                        case Cst.ProductFamily_CASHBALANCE:
                            ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashBalance);
                            break;
                        case Cst.ProductFamily_MARGIN:
                            ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_InitialMargin);
                            break;
                        case Cst.ProductFamily_CASHPAYMENT:
                            ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashPayment);
                            break;
                        case Cst.ProductFamily_CASHINTEREST:
                            ret = IdMenu.GetIdMenu(IdMenu.Menu.InputTradeRisk_CashInterest);
                            break;
                    }
                }
            }

            return ret;
        }

        #endregion Methods
    }
}

