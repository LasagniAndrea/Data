using EFS.ACommon;
using EFS.Actor;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.GUI;
using EFS.GUI.Attributes;
using EFS.GUI.CCI;
using EFS.GUI.SimpleControls;
using EFS.Import;
using EFS.Process;
using EFS.Referential;
using EFS.Status;
using EFS.TradeInformation;
using EFS.TradeInformation.Import;
using EfsML;
using EfsML.Business;
using EfsML.Enum;
using EfsML.Interface;
using EfsML.Notification;
using FixML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
//
//20071212 FI Ticket 16012 => Migration Asp2.0
using SpheresMenu = EFS.Common.Web.Menu;

namespace EFS.Spheres
{
    /// <summary>
    /// Description résumée de TradeCommonCapturePage.
    /// </summary>
    public abstract partial class TradeCommonCapturePage : CapturePageBase
    {

        #region Members
        /// <summary>
        /// 
        /// </summary>
        protected bool isRefreshHeader;

        /// <summary>
        /// 
        /// </summary>
        protected FullConstructor m_FullCtor;

        /// <summary>
        /// 
        /// </summary>
        protected bool m_IsReset_IdentifierDisplaynameDescription;

        /// <summary>
        /// 
        /// </summary>
        protected SelCommonProduct m_SelCommonProduct;

        /// <summary>
        /// valoriser à true lorsque l'enregistrement s'est efectué avec succès (voir methode RecordCapture)
        /// </summary>
        /// <remarks>FI 20091130 [16769] add isRecordOk</remarks>
        private bool isRecordOk;

        /// <summary>
        /// valoriser à true lorsque l'utilisateur change de template
        /// </summary>
        /// <remarks>FI 20091130 [16769] add isTemplateChanged</remarks>
        private bool isTemplateChanged;
        #endregion Members

        #region Accessors
        /// <summary>
        /// Obtient le nom de répertoire qui donne les screen et les objets de design 
        /// </summary>
        protected override string FolderName
        {
            get
            {
                string ret = string.Empty;
                if (null != TradeCommonInput.SQLProduct)
                    ret = TradeCommonInput.SQLProduct.Identifier;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        ///  FI 20081128 [ticket 16435] IsStEnvironment_Template doit être une property public
        public bool IsStEnvironment_Template
        {
            get { return TradeCommonInput.TradeStatus.IsStEnvironment_Template; }
        }

        /// <summary>
        /// Obtient true si le trade existe et que la licence autorise le produit associé
        /// </summary>
        protected bool IsTradeFound
        {
            get
            {
                //20090104 FI Ne pas ecrire en 1 seule ligne car si  TradeCommonInput.IsEventFound = false, TradeCommonInput.SQLProduct peut être null
                bool ret = false;
                ret = TradeCommonInput.IsTradeFound;

                #if RELEASE || RELEASEDEV
                if (ret)
                    ret = SessionTools.License.IsLicProductAuthorised(TradeCommonInput.SQLProduct.Identifier);
                #endif 

                return ret;
            }
        }

        /// <summary>
        /// Obtient true si le trade existe (avec TRADEXML) et que la licence autorise le produit associé
        /// </summary>
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (New Accessor)
        protected bool IsTradeFoundWithXML
        {
            get
            {
                bool ret = TradeCommonInput.IsTradeFoundWithXML;

                #if RELEASE || RELEASEDEV
                if (ret)
                    ret = SessionTools.License.IsLicProductAuthorised(TradeCommonInput.SQLProduct.Identifier);
                #endif

                return ret;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        protected override bool IsExtend
        {
            get { return TradeCommonInput.IsExtend(); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// EG 20110308 HPC Nb ligne de frais sur facture
        public int NumberRowByPage
        {
            get
            {
                int nbRow = SessionTools.NumberRowByPage;
                if (0 >= nbRow)
                    nbRow = 20;
                return nbRow;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override string SubTitleLeft
        {
            get
            {
                string ret = string.Empty;
                if (IsTradeFound)
                    ret = TradeCommonInput.SQLInstrument.DisplayName;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override InputGUI InputGUI
        {
            get { return (InputGUI)TradeCommonInputGUI; }
        }

        // FI 20140930 [XXXXX] Mise en commentaire
        //protected virtual Cst.SQLCookieGrpElement ProductGrpElement
        //{
        //    get { return Cst.SQLCookieGrpElement.SelProduct; }
        //}

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonCaptureGen TradeCommonCaptureGen
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonHeaderBanner TradeCommonHeaderBanner
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonInput TradeCommonInput
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual TradeCommonInputGUI TradeCommonInputGUI
        {
            get { return null; }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string TxtIdentifier
        {
            get
            {
                string ret = "N/A";
                Control ctrl = this.FindControl(TradeCommonHeaderBanner.ctrlTxtIdentifier);
                if (null != ctrl)
                    ret = ((TextBox)ctrl).Text;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string TxtDisplayName
        {
            get
            {
                string ret = "N/A";
                Control ctrl = this.FindControl(TradeCommonHeaderBanner.ctrlTxtDisplayName);
                if (null != ctrl)
                    ret = ((TextBox)ctrl).Text;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string TxtDescription
        {
            get
            {
                string ret = "N/A";
                Control ctrl = this.FindControl(TradeCommonHeaderBanner.ctrlTxtDescription);
                if (null != ctrl)
                    ret = ((TextBox)ctrl).Text;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected string TxtExtLink
        {
            get
            {
                string ret = "N/A";
                Control ctrl = this.FindControl(TradeCommonHeaderBanner.ctrlTxtExtLink);
                if (null != ctrl)
                    ret = ((TextBox)ctrl).Text;
                return ret;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsBtnRecordVisible
        {
            get
            {
                return base.IsBtnRecordVisible && IsTradeFound;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override bool IsBtnCancelVisible
        {
            get
            {
                return (base.IsBtnCancelVisible);
            }
        }

        /// <summary>
        /// Obtient la taille de la zone TexteBox de consultation (taille en pixel)
        /// <para>Taille default = 200px</para>
        /// </summary>
        protected virtual int TextBoxConsultWith
        {
            get
            {
                return 200;
            }
        }

        /// <summary>
        /// Obtient le context [utilisé pour la recherche des objets de design]
        /// </summary>
        protected override string ObjectContext
        {
            get
            {
                string ret = string.Empty;
                //
                if (Cst.Capture.IsModeRemove(InputGUI.CaptureMode))
                    ret += "remove";
                //
                return ret;
            }
        }
        #endregion Accessors

        #region Constructors
        public TradeCommonCapturePage()
            : base()
        {

        }
        #endregion Constructors

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnAction(object sender, skmMenu.MenuItemClickEventArgs e)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        // EG 20171204 [23509] Upd (isClearPlaceHolder = true si CashPayment)
        // EG 20180514 [23812] Report
        protected override void OnConsult(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.OnConsult");
            Cst.Capture.MenuConsultEnum captureMenuConsult =
                (Cst.Capture.MenuConsultEnum)System.Enum.Parse(typeof(Cst.Capture.MenuConsultEnum), e.CommandName);

            switch (captureMenuConsult)
            {
                case Cst.Capture.MenuConsultEnum.SetTemplate:
                case Cst.Capture.MenuConsultEnum.SetProduct:
                case Cst.Capture.MenuConsultEnum.LoadTrade:
                case Cst.Capture.MenuConsultEnum.SetScreen:
                case Cst.Capture.MenuConsultEnum.GoUpdate:
                    bool isSetProduct = (Cst.Capture.MenuConsultEnum.SetProduct == captureMenuConsult);
                    bool isLoadTemplate = (Cst.Capture.MenuConsultEnum.SetTemplate == captureMenuConsult) ||
                                          (Cst.Capture.MenuConsultEnum.SetProduct == captureMenuConsult);
                    bool isLoadTrade = (Cst.Capture.MenuConsultEnum.LoadTrade == captureMenuConsult) ||
                                       (Cst.Capture.MenuConsultEnum.GoUpdate == captureMenuConsult);
                    bool isNewSearch = (isLoadTrade || isLoadTemplate);
                    bool isSetScreen = (Cst.Capture.MenuConsultEnum.SetScreen == captureMenuConsult);

                    //FI 20110415 => Il faudrait isClearPlaceHolder = false lorsque l'utilisateur consulte un trade dont le screen est identique au screen précédent
                    if (isSetProduct)
                        TradeCommonInputGUI.InitFromCookies();

                    //Par défaut Spheres efface le contenu du placeHolder
                    bool isClearPlaceHolder = true;
                    if ((false == this.IsScreenFullCapture))
                    {
                        //FI 20091130 [16769]
                        //Spheres® n'efface pas le contenu du placeholder lorsque l'utilisateur vient de créer un trade en mode création 
                        //=> on reste sur le même écran (gain en perf) 
                        // EG 20171204 [23509] isClearPlaceHolder = true si CashPayment
                        if (isRecordOk && (Cst.Capture.MenuConsultEnum.SetTemplate == captureMenuConsult))
                        {
                            //isClearPlaceHolder = false;
                            isClearPlaceHolder = TradeCommonInput.DataDocument.CurrentProduct.IsCashPayment;
                        }
                            
                        //  
                        //FI 20110415 [17405]
                        //Spheres® n'efface pas le contenu du placeholder lorsque l'utilisateur change de template 
                        //=> on reste sur le même écran (gain en perf)
                        if (isTemplateChanged && (Cst.Capture.MenuConsultEnum.SetTemplate == captureMenuConsult))
                        {
                            //Spheres® efface tout de même l'écran si le statut business du template est différent du statut business de la saisie courante
                            //Ex L'utilisateur peut passer à un template de type alloc alors qu'il se trouve sur un block-Trade(de type Executed nécessairement)
                            //Les écrans ne sont pas les même il faut redessiner la page dans ce cas
                            TradeStatus tradeSt = new TradeStatus();
                            tradeSt.Initialize(SessionTools.CS, TradeCommonInputGUI.TemplateIdT);
                            if (tradeSt.stBusiness.NewSt == TradeCommonInput.TradeStatus.stBusiness.NewSt)
                                isClearPlaceHolder = false;
                        }
                    }
                    //
                    if (isClearPlaceHolder)
                        ClearPlaceHolder();
                    //
                    #region Load
                    if (isNewSearch)
                        LoadCapture(isLoadTemplate);
                    #endregion Load
                    //
                    if (TradeCommonInput.IsTradeFound && isNewSearch)
                        SetDropDownInstrumentProdcuctTemplateScreen();
                    //
                    bool isControlSuccessful = true;
                    string msgControl = null;
                    //						
                    #region Lock Control
                    if (TradeCommonInput.IsTradeFound && (false == isSetScreen) && isControlSuccessful)
                    {
                        if (Cst.Capture.MenuConsultEnum.GoUpdate == captureMenuConsult)
                        {
                            Lock lckExisting = LockTools.SearchLock(SessionTools.CS, GetLockTrade());
                            isControlSuccessful = (null == lckExisting);
                            if (false == isControlSuccessful)
                                msgControl = lckExisting.ToString();
                        }
                    }
                    #endregion Lock Control
                    //
                    #region CheckCaptureModeAllowed
                    if (TradeCommonInput.IsTradeFound && (false == isSetScreen) && isControlSuccessful &&
                        (false == (InputGUI.CaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination)))
                    {
                        // FI 20151203 [21613] Modify Appel InitActionTuning
                        TradeCommonInputGUI.InitActionTuning(SessionTools.CS, TradeCommonInput.Product.IdI);
                        isControlSuccessful = TradeCommonCaptureGen.CheckCaptureModeAllowed(SessionTools.CS, TradeCommonInputGUI, out msgControl);
                    }
                    #endregion CheckCaptureModeAllowed
                    //
                    if (isControlSuccessful)
                    {

                        CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
                        {
                            user = SessionTools.User,
                            session = SessionTools.AppSession,
                            licence = SessionTools.License
                        };

                        TradeCommonCaptureGen.InitBeforeCaptureMode(SessionTools.CS, null, InputGUI, captureSessionInfo);
                        //
                        #region ReversePartyReference (ReverseSide)
                        if (isNewSearch && ("ReverseSide" == e.Argument))
                        {
                            try
                            {
                                TradeCommonInput.ReversePartyReference();
                            }
                            catch (NotImplementedException ex)
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                                this.MsgForAlertImmediate = ex.Message + Cst.CrLf2 + "Duplication without Reverse side";
                            }
                        }
                        #endregion ReversePartyReference (ReverseSide)
                        //
                        #region Initialize des status
                        if (TradeCommonInput.IsTradeFound && (false == isSetScreen))
                        {
                            if (Cst.Capture.IsModeNewOrDuplicateOrReflectOrRemove(InputGUI.CaptureMode) ||
                                Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode))
                            {
                                if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                                {
                                    //En création: on ecrase systématiquement le StatusEnvironment (issu du template) par REGULAR
                                    TradeCommonInput.TradeStatus.stEnvironment.CurrentSt = Cst.StatusEnvironment.REGULAR.ToString();
                                    //
                                    //TradeCommonInput.InitializeFromInstrument();
                                    // RD 20091228 [16809] Confirmation indicators for each party
                                    //Set default user status (Check/Match) from ACTORROLE (Utile au cas où un acteur est présent dans le Template)
                                    TradeCommonInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS), null);
                                }
                                //
                                // RD 20110421 [17416/17417] 
                                // Annulation avec remplaçante
                                bool isModeRemoveReplace = Cst.Capture.IsModeRemoveReplace(InputGUI.CaptureMode);
                                // Duplication ou Reflection d’une opération
                                bool isModeDuplicateOrReflect = Cst.Capture.IsModeDuplicateOrReflect(InputGUI.CaptureMode);
                                // Duplication d’une opération annulée
                                bool isModeDuplicateRemoved = isModeDuplicateOrReflect && TradeCommonInput.TradeStatus.IsStActivation_Deactiv;
                                //
                                if (isModeRemoveReplace || isModeDuplicateOrReflect || isModeDuplicateRemoved)
                                {
                                    int IdT_Template = 0;
                                    string templateIdentifier = TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(SessionTools.CS));
                                    if (StrFunc.IsFilled(templateIdentifier))
                                        IdT_Template = TradeRDBMSTools.GetTradeIdT(SessionTools.CS, templateIdentifier);
                                    //
                                    // Duplication d’une opération annulée
                                    // Le nouveau Trade, ne doit pas hérité du statut d’Activation DEACTIV du Trade annulé
                                    // mais bien de celui issu du template utilisé pour le Trade dupliqué
                                    if (isModeDuplicateRemoved)
                                    {
                                        if (false == TradeCommonInput.InitStActivationFromTrade(SessionTools.CS, null, IdT_Template))
                                            TradeCommonInput.TradeStatus.stActivation.CurrentSt = Cst.StatusActivation.REGULAR.ToString();
                                        //
                                        TradeCommonInput.IsTradeRemoved = false;
                                    }
                                    //
                                    // Duplication d’une opération, Reflection et Annulation avec remplaçante
                                    // Le nouveau Trade, ne doit pas hérité des statuts Check/Match du Trade d'origine, mais, à l'image d'un nouveau Trade (IsModeNew):
                                    // 1- de ceux issus du template utilisé pour le Trade d'origine
                                    // 2- de ceux issus de ACTORROLE, en fonction des acteurs présents sur le Trade
                                    if (isModeDuplicateOrReflect || isModeRemoveReplace)
                                    {
                                        TradeCommonInput.ResetStUser(false);
                                        TradeCommonInput.InitStUserFromTrade(SessionTools.CS, null, IdT_Template);
                                        TradeCommonInput.InitStUserFromPartiesRole(CSTools.SetCacheOn(SessionTools.CS),null);
                                    }
                                    //
                                    // Annulation avec remplaçante
                                    // Le nouveau Trade, ne doit pas hérité du statut Priority de l'opération annulée
                                    // mais bien d'une valeur par défaut issue du fichier web.config
                                    // <add key="Spheres_TradeRemoveReplace_StPriority" value="LOW"/>
                                    if (isModeRemoveReplace)
                                    {
                                        TradeCommonInput.TradeStatus.stPriority.CurrentSt = (string)SystemSettings.GetAppSettings("Spheres_TradeRemoveReplace_StPriority",
                                            typeof(System.String), Cst.StatusPriority.HIGH.ToString());
                                    }
                                }
                                //
                                //Set default user status (Check/Match) from ACTIONTUNING
                                TradeCommonInput.InitStUserFromTuning(TradeCommonInputGUI.ActionTuning);
                            }
                        }
                        #endregion Initialize
                        //
                        #region Display
                        DisplayCapture((false == isSetScreen));
                        if ((false == IsScreenAvailable() && (Cst.Capture.TypeEnum.Customised == InputGUI.CaptureType)))
                            OnScreen(sender, new skmMenu.MenuItemClickEventArgs(Cst.FpML_ScreenFullCapture));
                        #endregion Display
                        //
                        SavePlaceHolder();
                        SetInputSession();
                    }
                    else
                    {

                        if (StrFunc.IsFilled(msgControl))
                        {
                            Style style = new Style
                            {
                                ForeColor = Color.Red
                            };
                            style.Font.Bold = true;
                            JavaScript.DialogStartUpImmediate(this, Cst.Capture.GetLabel(InputGUI.CaptureMode), msgControl, false, ProcessStateTools.StatusErrorEnum, JavaScript.DefautHeight, JavaScript.DefautWidth);
                        }
                        //Contrôle invalide --> Annulation de l'action en cours
                        OnValidate(sender, new CommandEventArgs(Cst.Capture.MenuValidateEnum.Annul.ToString(), null));
                    }
                    break;
                case Cst.Capture.MenuConsultEnum.FirstTrade:
                case Cst.Capture.MenuConsultEnum.PreviousTrade:
                case Cst.Capture.MenuConsultEnum.NextTrade:
                case Cst.Capture.MenuConsultEnum.EndTrade:

                    #region Search Trade (First,Last,Previous and next)
                    int tmpIdt = GetIdTrade(captureMenuConsult, TradeIdT);
                    if (tmpIdt > 0)
                    {
                        TradeIdT = tmpIdt;
                        OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadTrade.ToString()));
                    }
                    #endregion Search Trade (First,Last,Previous and next)
                    break;
            }
            //
            if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                CtrlTrade.Focus();

            AddAuditTimeStep("End TradeCommonCapturePage.OnConsult");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnExportClick(object sender, EventArgs e)
        {
            if (0 < TradeCommonInput.IdT)
            {
                DatasetTradeEvent ds = new DatasetTradeEvent(SessionTools.CS);
                ds.Load(TradeCommonInput.IdT);
                TradeExport tradeExport = ds.GetTradeExport();
                EFS_SerializeInfoBase serializerInfo = new EFS_SerializeInfoBase(tradeExport.GetType(), tradeExport);
                StringBuilder sb = CacheSerializer.Serialize(serializerInfo);
                string WindowID = FileTools.GetUniqueName("Trade", TradeCommonInput.Identifier);
                DisplayXml("TradeExport_XML", WindowID, sb.ToString());
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.OnInit");
            base.OnInit(e);
            //
            m_DefaultCurrency = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_currency");
            m_DefaultBusinessCenter = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");
            m_DefaultParty = new EFS_DefaultParty();
            if (SessionTools.Collaborator_ENTITY_ISENTITY)
            {
                m_DefaultParty.id = SessionTools.GetDefaultBICEntityOfUser(true);
                m_DefaultParty.OTCmlId = SessionTools.Collaborator_ENTITY_IDA;
                m_DefaultParty.partyId = SessionTools.Collaborator_ENTITY_IDENTIFIER;
                m_DefaultParty.partyName = SessionTools.Collaborator_ENTITY_DISPLAYNAME;
            }
            AddAuditTimeStep("End TradeCommonCapturePage.OnInit");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210222 [XXXXX] Suppression inscription function PostBack, OnCtrlChanged (présent dans PageBase.js)
        protected override void OnLoad(EventArgs e)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.OnLoad");
            //
            //FI 20120308 Reouverture de la page en cas de perte des variables sessions
            //Les varaibles sessions étant initialisée dans le OnLoad de la page qui hérite de TradeCommonCapturePage
            //Il faut laisser ce contrôle ici
            CheckSessionState();

            //
            if (StrFunc.IsFilled(Request.QueryString["PKV"]) && (false == IsPostBack))
                InputGUI.CaptureMode = Cst.Capture.ModeEnum.Consult;

            string eventArgument = PARAM_EVENTARGUMENT;
            string eventTarget = PARAM_EVENTTARGET;

            Cst.Capture.ModeEnum eventArgumentModeEnum = Cst.Capture.ModeEnum.New;
            if (null != TradeCommonInputGUI)
                eventArgumentModeEnum = TradeCommonInputGUI.CaptureMode;
            if (StrFunc.IsFilled(eventArgument) && Enum.IsDefined(typeof(Cst.Capture.ModeEnum), eventArgument))
                eventArgumentModeEnum = (Cst.Capture.ModeEnum)Enum.Parse(typeof(Cst.Capture.ModeEnum), eventArgument, true);

            m_SelCommonProduct = new SelCommonProduct(this, TradeCommonInputGUI.GrpElement, null, this.InputGUISessionID, false, eventArgumentModeEnum, (eventArgumentModeEnum != TradeCommonInputGUI.CaptureMode));

            PageConstruction();

            if (StrFunc.IsEmpty(eventTarget) || eventTarget.StartsWith("tblMenu$") || eventTarget.StartsWith("tblMenu_lst") || eventTarget.StartsWith("lst"))
                m_SelCommonProduct.Bind(Cst.EnumElement.Instrument);

            if (IsPostBack)
            {
                #region IsPostBack
                InitFromProductInstrument();
                //

                //Mise à jour du Document Fpml en mode Full
                if (null != InputGUI.Controls)
                {
                    CaptureTools.CompleteCollection(this, InputGUI.Controls);
                }
                RestorePlaceHolder();
                //
                MethodsGUI.SetEventHandler(PlaceHolder.Controls);
                //
                SavePlaceHolder();
                #endregion IsPostBack
            }
            else
            {
                #region !IsPostBack
                if (StrFunc.IsFilled(Request.QueryString["FKV"]))
                    TradeCommonInputGUI.IdI = IntFunc.IntValue2(Request.QueryString["FKV"]);
                //
                //Chargement du template, du screen 
                TradeCommonInputGUI.Initialize();
                //
                if (Request.QueryString["N"] == "1" && InputGUI.Permission.IsCreate)
                {
                    InputGUI.CaptureMode = Cst.Capture.ModeEnum.New;
                }
                else
                {
                    if (StrFunc.IsFilled(Request.QueryString["PKV"]))
                    {
                        InputGUI.CaptureMode = Cst.Capture.ModeEnum.Consult;  // important pour PageConstruction (title de la page) 
                        string pkValue = Request.QueryString["PKV"];
                        TradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(SessionTools.CS, Convert.ToInt32(pkValue));
                        TradeIdT = Convert.ToInt32(pkValue);
                    }
                }
                //
                // RD 20110225 C'est pour charger le dernier Marché utilisé                    
                // EG 20171114 [23509] En commentaire car fait plus loin
                if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                {
                    //string defaultFacility = string.Empty;
                    //AspTools.ReadSQLCookie(Cst.SQLCookieElement.TradeDefaultFacility.ToString(), out defaultFacility);

                    //if (StrFunc.IsFilled(defaultFacility))
                    //    TradeCommonInput.InitDefault(CommonInput.DefaultEnum.facility, defaultFacility);

                    //string defaultMarket = string.Empty;
                    //AspTools.ReadSQLCookie(Cst.SQLCookieElement.TradeDefaultMarket.ToString(), out defaultMarket);

                    //if (StrFunc.IsFilled(defaultMarket))
                    //    TradeCommonInput.InitDefault(CommonInput.DefaultEnum.market, defaultMarket);
                }
                //FI 20110929 Il faut absolument charger les instruments
                //Sinon Spheres® conserve l'instrument issu de la dernière saisi, le screen affiche celui de la dernière saisi
                //RD 20110517
                //Dans le cas où les DDL Instrument et Template ne sont pas saisissables (En mode Consult):
                //Vider la DDL et ne laisser que la valeur séléctionnée
                if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                {
                    //DropDownList ddlInstrument = (DropDownList)this.FindControl("tblMenu$lst" + Cst.EnumElement.Instrument.ToString());
                    DropDownList ddlTemplate = (DropDownList)this.FindControl("tblMenu$lst" + Cst.EnumElement.Template.ToString());
                    if (null== ddlTemplate)
                        ddlTemplate = (DropDownList)this.FindControl("lst" + Cst.EnumElement.Template.ToString());

                    //
                    //ControlsTools.DDLLoad_DisplayValue(ddlInstrument);
                    ControlsTools.DDLLoadSingle_SelectedItem(ddlTemplate);
                }
                //
                if ((Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode)))
                {
                    if (StrFunc.IsFilled(TradeCommonInputGUI.TemplateIdentifier))
                    {
                        OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetTemplate.ToString()));
                    }
                    else
                    {
                        //Ici si TradeCommonInputGUI.Initialize n'a rien remonté
                        //Dans ce cas extrême, l'acteur pas aucune permission sur instruments, ou l'instrument remonté n'a pas de template associé...
                        throw new Exception("No instrument, template, screen found for Input");
                    }
                }
                else if ((Cst.Capture.IsModeConsult(TradeCommonInputGUI.CaptureMode)) && StrFunc.IsFilled(TradeIdentifier) && (TradeIdentifier != "0"))
                {
                    OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadTrade.ToString()));
                }
                #endregion !IsPostBack
            }
            //
            base.OnLoad(e);
            //
            //JavaScript.PostBack(this);
            //JavaScript.OnCtrlChanged((PageBase)this);
            AddAuditTimeStep("End TradeCommonCapturePage.OnLoad");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.OnPreRender");

            string eventTarget = string.Empty + PARAM_EVENTTARGET;

            if ((Cst.Capture.IsModeNewOrDuplicate(InputGUI.CaptureMode)) && (FindControl("tblTradeLink") is Table tblTradeLink))
                tblTradeLink.Parent.Controls.Remove(tblTradeLink);

            if (StrFunc.IsFilled(eventTarget))
            {
                if (eventTarget.StartsWith("tblMenu_lst") || eventTarget.StartsWith("lst"))
                {
                    DropDownList ddl = FindControl(eventTarget.Replace("_", "$")) as DropDownList;
                    OnSelectedElementProductChanged(ddl, null);
                }
                else if ("imgStatus" == eventTarget)
                {
                    //FI 20120124 Se produit, en Mode saisie lorsque que l'utilisateur click sur le bouton status
                    //ceci est nécessaire puisque lorsque l'utilisateur va valider les nouveaux status Spheres® recharge la page (avec chgt du trade) 
                    // EG 20210928 [XXXXX] SetPartyOrder n'est appliqué que sur les écrans lights.
                    if (Cst.FpML_ScreenFullCapture != InputGUI.CurrentIdScreen)
                        this.TradeCommonInput.CustomCaptureInfos.CciTradeCommon.SetPartyInOrder();
                }
            }

            m_SelCommonProduct.RestoreColor(Cst.EnumElement.Template);
            m_SelCommonProduct.RestoreColor(Cst.EnumElement.Screen);

            RefreshLogButton();

            base.OnPreRender(e);

            // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (Suppression du body de la saisie + Affichage message)
            if (TradeCommonInput.IsTradeFoundWithoutXML)
            {
                if (FindControl("updPanel") is UpdatePanel ctrlContainer)
                {
                    ctrlContainer.ContentTemplateContainer.Controls.Clear();
                    Panel pnlErorTradeWithoutXML = new Panel()
                    {
                        ID = "pnlMsgTradeWithoutXML",
                    };
                    Label lblErorTradeWithoutXML = new Label()
                    {
                        Text = Ressource.GetString("Msg_TradeWithoutXML"),
                    };
                    pnlErorTradeWithoutXML.Controls.Add(lblErorTradeWithoutXML);
                    ctrlContainer.ContentTemplateContainer.Controls.Add(pnlErorTradeWithoutXML);
                }
            }
            AddAuditTimeStep("End TradeCommonCapturePage.OnPreRender");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// FI 20141021 [20350] Add Method
        protected override void OnPreRenderComplete(EventArgs e)
        {
            SetRequestTrackBuilderItemLoad();
            base.OnPreRenderComplete(e);
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si consultation d'un trade
        /// </summary>
        /// FI 20141021 [20350] Add Method
        private void SetRequestTrackBuilderItemLoad()
        {
            Boolean isTrack = SessionTools.IsRequestTrackConsultEnabled && (null == RequestTrackBuilder);
            if (isTrack)
            {
                Nullable<RequestTrackActionEnum> action = null;
                if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && (this.TradeCommonInput.IsTradeFound))
                {
                    if (false == IsPostBack)
                    {
                        // On rentre ici par exemple lorsque l'utilisateur affiche un trade depuis un grid
                        action = RequestTrackActionEnum.ItemLoad;
                    }
                    else
                    {
                        Control co = GetPostBackControl();
                        if (null != co)
                        {
                            if (co.ID == "mnuConsult")
                            {
                                // On rentre ici lorsque l'utilsateur agit sur la toolbar de consultation
                                // -  s'il click sur un des boutons (le 1er, le suivant, le précédent, ou le dernier) 
                                // -  s'il appuie sur enter  dans la zone texte de saisie des identifier de trade
                                action = RequestTrackActionEnum.ItemLoad;
                            }
                        }
                    }

                    if (null != action)
                    {
                        RequestTrackTradeBuilder builder = new RequestTrackTradeBuilder
                        {
                            DataDocument = TradeCommonInput.DataDocument,
                            TradeIdentIdentification = TradeCommonInput.Identification,
                            action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(action.Value, RequestTrackActionMode.manual)
                        };
                        this.RequestTrackBuilder = builder;
                    }
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnTemplate(object sender, EventArgs e)
        {

            DropDownList ddl = (DropDownList)sender;
            //
            isTemplateChanged = (TradeCommonInputGUI.TemplateIdentifier != ddl.SelectedItem.Text);
            //
            if (Cst.Capture.IsModeNewOrDuplicateOrReflectOrRemove(InputGUI.CaptureMode))
            {
                if (isTemplateChanged)
                {
                    TradeCommonInputGUI.TemplateIdT = IntFunc.IntValue(ddl.SelectedItem.Value);
                    TradeCommonInputGUI.TemplateIdentifier = ddl.SelectedItem.Text;
                    OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetTemplate.ToString()));
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTradeChanged(object sender, System.EventArgs e)
        {

            ClearPlaceHolder();
            InputGUI.ClearGUI();
            //
            JavaScript.SetInitialFocus((TextBox)sender);
            ((TextBox)CtrlTrade).Attributes["oldvalue"] = TradeIdentifier;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20140708 [20179] Modify : Gestion du mode IsModeMatch
        /// FI 20140708 [20179] TODO => gestion du mode IsModeMatch devrait être effectué sur CapturePageBase ( A faire plus tard)
        protected override void OnMode(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            skmMenu.MenuItemClickEventArgs argOnConsult = null;

            InputGUI.CaptureMode = (Cst.Capture.ModeEnum)System.Enum.Parse(typeof(Cst.Capture.ModeEnum), e.CommandName);


            // EG 20150923 Unused
            // if (null != sender)
            //    InputGUI.CaptureModeImage = GetMenuImageUrl(((skmMenu.Menu)sender).Items, e);

            if (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode) ||
                Cst.Capture.IsModeAction(InputGUI.CaptureMode))
            {
                if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                {
                    // Ce cas se produit après consultation d'un Template (les templates n'ont pas de Template Identifier)
                    // et que l'on passe en mode création				
                    if (StrFunc.IsEmpty(TradeCommonInputGUI.TemplateIdentifier))
                        TradeCommonInputGUI.InitFirstTemplateUsedAsDefault();
                    //
                    argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetTemplate.ToString(), e.Argument);
                }
                else
                {
                    argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadTrade.ToString(), e.Argument);
                }
            }
            else if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
            {
                //20090915 Lecture du PKV uniquement la 1er initialisation de la page lorsque l'on vient du datagrid
                //Parce que ce PKV n'a peut-rien à voir avec la saisie en cours
                //Exemple je consulte un trade depuis le Datagrid PKV est en phase avec l'écran
                //Si je consulte un autre trade alors le PKV ne correspond plus au trade en cours
                if (StrFunc.IsFilled(Request.QueryString["PKV"]) && (false == IsPostBack))
                {
                    string pkValue = Request.QueryString["PKV"];
                    TradeIdentifier = TradeRDBMSTools.GetTradeIdentifier(SessionTools.CS, Convert.ToInt32(pkValue));
                }
                else
                {
                    // EG 20100402
                    TradeIdentifier = string.Empty;
                    TradeIdT = -1;
                }
                argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadTrade.ToString(), e.Argument);
            }
            else if (Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode))
            {
                argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.GoUpdate.ToString(), e.Argument);
            }
            else if (Cst.Capture.IsModeMatch(InputGUI.CaptureMode)) // FI 20140708 [20179] Add 
            {
                argOnConsult = new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.LoadTrade.ToString(), e.Argument);
            }

            if (null != argOnConsult)
                OnConsult(null, argOnConsult);

            SetInitialFocus();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnCustomConfirmClick(object sender, EventArgs e)
        {
            try
            {
                DisplayConfirm("CustomConfirm");
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
        protected override void OnResultAction(object sender, skmMenu.MenuItemClickEventArgs e)
        {
            base.OnResultAction(sender, e);
        }

        /// <summary>
        /// Ajoute un script qui affiche le trade xml à l'ouverture de  la page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnXmlClick(object sender, EventArgs e)
        {
            try
            {
                bool isValidationXsd = true;
                if (null != TradeCommonInput.SQLInstrument)
                    isValidationXsd = (false == TradeCommonInput.SQLInstrument.IsOpen);
                //
                DisplayTradeXml(isValidationXsd, false);
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
        protected void OnTradeInputClick(object sender, EventArgs e)
        {
            try
            {
                DisplayTradeInput();
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
        protected override void OnScreen(object sender, EventArgs e)
        {
            if (Cst.FpML_ScreenFullCapture != InputGUI.CurrentIdScreen)
            {
                //CciTradeCommon est null si le product n'est pas géré par la saisie light
                if (null != TradeCommonInput.CustomCaptureInfos.CciTradeCommon)
                    TradeCommonInput.CustomCaptureInfos.CciTradeCommon.SetPartyInOrder();
            }
            //
            base.OnScreen(sender, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void OnSelectedElementProductChanged(object sender, EventArgs e)
        {

            string id = ((DropDownList)sender).ID.Replace("lst", string.Empty);
            Cst.EnumElement enumElement = (Cst.EnumElement)Enum.Parse(typeof(Cst.EnumElement), id);

            m_SelCommonProduct.OnSelectedElementChanged(sender, e);
            m_SelCommonProduct.OnSelectClick(sender, e);
            switch (enumElement)
            {
                case Cst.EnumElement.Instrument:
                    OnConsult(null, new skmMenu.MenuItemClickEventArgs(Cst.Capture.MenuConsultEnum.SetProduct.ToString()));
                    break;
                case Cst.EnumElement.Template:
                    OnTemplate(sender, e);
                    break;
                case Cst.EnumElement.Screen:
                    OnScreen(sender, e);
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="writer"></param>
        // EG 20160119 Refactoring Footer
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void Render(HtmlTextWriter writer)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.Render");

            if (!(FindControl("tblMenu$txtTrade") is TextBox txtTradeId))
                txtTradeId = FindControl("txtTrade") as TextBox;
            if (null != txtTradeId)
                txtTradeId.Enabled = (false == Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode));

            // EG 20160119 Refactoring Header
            TradeCommonHeaderBanner.DisplayHeader(false, isRefreshHeader);

            // EG 20160119 Refactoring Footer
            if (FindControl("divextllink") is Panel pnlExtlLink)
            {
                if (IsTradeFound)
                    ControlsTools.RemoveStyleDisplay(pnlExtlLink);
                else
                    pnlExtlLink.Style.Add(HtmlTextWriterStyle.Display, "none");
            }

            if ((FindControl("tblMenu$lst" + Cst.EnumElement.Instrument.ToString()) is WCDropDownList2 ctrl) && ctrl.IsSetTextOnTitle)
                ControlsTools.DDLItemsSetTextOnTitle(ctrl);

            ctrl = FindControl("tblMenu$lst" + Cst.EnumElement.Template.ToString()) as WCDropDownList2;
            if ((null != ctrl) && ctrl.IsSetTextOnTitle)
                ControlsTools.DDLItemsSetTextOnTitle(ctrl);

            ctrl = FindControl("tblMenu$lst" + Cst.EnumElement.Screen.ToString()) as WCDropDownList2;
            if ((null != ctrl) && ctrl.IsSetTextOnTitle)
                ControlsTools.DDLItemsSetTextOnTitle(ctrl);

            AddAuditTimeStep("End TradeCommonCapturePage.Render");

            base.Render(writer);
        }
        #endregion Events

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override void AddToolBarConsult(Panel pPnlParent)
        {
            Panel pnl = new Panel() { ID = "divtrade", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };
            FpMLTextBox txtTrade = new FpMLTextBox(null, "0", TextBoxConsultWith, TradeCommonInputGUI.MainRessource, false, "txtTrade", null, new Validator(),
                 new Validator(EFSRegex.TypeRegex.RegexString, "N° " + TradeCommonInputGUI.MainRessource, true));
            txtTrade.Attributes.Add("onblur", "OnTradeChanged('tblMenu_txtTrade');");
            txtTrade.Attributes.Add("onkeydown", "PostBackOnKeyEnter(event,'tblMenu$mnu" + Cst.Capture.MenuEnum.Consult.ToString() + "','"
                + Cst.Capture.MenuConsultEnum.LoadTrade.ToString() + "');");
            txtTrade.AssociatedLabel.Style.Remove("color");
            HtmlInputHidden hihTradeIdT = new HtmlInputHidden
            {
                ID = "hihTradeIdT"
            };
            pnl.Controls.Add(txtTrade);
            pnl.Controls.Add(hihTradeIdT);
            pPnlParent.Controls.Add(pnl);

            base.AddToolBarConsult(pPnlParent);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected override void AddToolBarGroupProductList(Panel pPnlParent)
        {
            AddToolBarDropDownList(pPnlParent, Cst.EnumElement.Instrument, "instrument",250);
            AddToolBarDropDownList(pPnlParent, Cst.EnumElement.Template, "template", 220);
            AddToolBarDropDownList(pPnlParent, Cst.EnumElement.Screen, "screen", 200);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pPnlParent">Panel container</param>
        /// <param name="pListName"></param>
        /// <param name="pImageName"></param>
        /// <param name="pWidth"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected void AddToolBarDropDownList(Panel pPnlParent, Cst.EnumElement pListName, string pImageName, int pWidth)
        {

            m_SelCommonProduct.NumberOfSections++;

            Panel pnlMain = new Panel() 
            {
                ID = "div" + pListName.ToString()
            };
            pnlMain.Style.Add(HtmlTextWriterStyle.Display, "inline-flex");

            WCToolTipPanel pnl = new WCToolTipPanel() 
            {
                CssClass = String.Format("fa-icon user-fa-{0}", pImageName),
                ID = "img" + pListName.ToString()
            };
            pnl.Pty.TooltipTitle = Ressource.GetString(pListName.ToString());

            OptionGroupDropDownList ddlList = new OptionGroupDropDownList() 
            {
                CssClass = EFSCssClass.DropDownListCaptureLight,
                ID = "lst" + pListName.ToString(),
                AccessKey = pListName.ToString().Substring(0, 1),
                AutoPostBack = false,
                Width = Unit.Pixel(pWidth),
                IsSetTextOnTitle = true
            };
            ddlList.Attributes.Add("onblur", "OnCtrlChanged('" + ddlList.ID + "','');return false;");
            ddlList.Attributes.Add("oldvalue", string.Empty);
            pnlMain.Controls.Add(pnl);
            pnlMain.Controls.Add(ddlList);
            pPnlParent.Controls.Add(pnlMain);
        }
        // EG 20160119 Refactoring Footer
        protected override void AddToolBarFooter()
        {
            Panel pnlFooter = new Panel() { ID = "divfooter", CssClass = this.CSSMode + " " + InputGUI.MainMenuClassName };
            AddButtonsFooter(pnlFooter);
            CellForm.Controls.Add(pnlFooter);
        }
        // EG 20160119 Refactoring Footer
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210120 [25556] Complement : New version of JQueryUI.1.12.1 (JS and CSS)
        // EG 20210120 [25556] Add input in cssClass
        protected override void AddExternalLink()
        {
            Label lblExtlLink = new Label
            {
                ID = TradeCommonHeaderBanner.ctrlTxtExtLink.Replace(Cst.TXT, Cst.LBL),
                Text = Ressource.GetString2("EXTLLINK"),
                CssClass = EFSCssClass.LabelCapture
            };
            WCTextBox2 txtExtlLink = new WCTextBox2(TradeCommonHeaderBanner.ctrlTxtExtLink, false, false, false, EFSCssClass.Capture, null);

            WCBodyPanel pnlExternalLink = new WCBodyPanel(this.CSSMode + " input gray") { ID = "divextllink" };
            pnlExternalLink.AddContent(lblExtlLink);
            pnlExternalLink.AddContent(txtExtlLink);
            CellForm.Controls.Add(pnlExternalLink);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRoot"></param>
        protected override void AddExtends(ref XmlNode pRoot)
        {
            try
            {
                DataRow[] drInstrExtension = InstrTools.GetInstrExtension(CSTools.SetCacheOn(SessionTools.CS), TradeCommonInput.Product.IdI, TradeCommonInput.SQLInstrument.IdP);
                //
                if (null != drInstrExtension)
                {
                    // RD 20120919 Gestion de l'imbrication de controls de type "Table"
                    Stack<XmlNode> tableNodeStack = new Stack<XmlNode>();
                    XmlNode parentTableNode = null;

                    foreach (DataRow rowExtension in drInstrExtension)
                    {
                        XmlAttribute labelCaptionAttribute = null;
                        XmlNode extendNode = null;
                        //
                        string width = string.Empty;
                        string rows = string.Empty;
                        string textMode = string.Empty;
                        //
                        int idDefineExtendDet = Convert.ToInt32(rowExtension["IDDEFINEEXTENDDET"]);
                        int sequenceNo = Convert.ToInt32(rowExtension["SEQUENCENO"]);
                        string identifier = Convert.ToString(rowExtension["IDENTIFIERDET"]);
                        string clientid = Convert.ToString(rowExtension["IDDEFINEEXTENDDET"]) + CustomObject.KEY_SEPARATOR + Convert.ToString(rowExtension["IDENTIFIERDET"]);
                        string webControlType = Convert.ToString(rowExtension["WEBCONTROLTYPE"]);
                        string dataType = (Convert.IsDBNull(rowExtension["DATATYPE"]) ? TypeData.TypeDataEnum.unknown.ToString() : Convert.ToString(rowExtension["DATATYPE"]));
                        string regEx = (Convert.IsDBNull(rowExtension["REGULAREXPRESSION"]) ? string.Empty : Convert.ToString(rowExtension["REGULAREXPRESSION"]));
                        //
                        string extensionLogMsg = "[Extension: " + identifier + " [id=" + idDefineExtendDet.ToString() + "]" + ", WebControl type: " + webControlType + " [sequenceNo=" + sequenceNo.ToString() + "]]";
                        //
                        string displayName = Convert.ToString(rowExtension["DISPLAYNAMEDET"]);
                        try
                        {
                            #region Recherche de la culture
                            // RD 20120316 / Utilisation des ressources via SYSTEMMSG
                            displayName = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), displayName, null);
                            #endregion
                        }
                        catch (Exception) { }
                        //
                        WebControlType.WebControlTypeEnum controlType = WebControlType.WebControlTypeEnum.Unknown;
                        bool isObject = false;
                        try
                        {
                            controlType = (WebControlType.WebControlTypeEnum)Enum.Parse(typeof(WebControlType.WebControlTypeEnum), webControlType);
                        }
                        catch { isObject = true; }
                        //
                        bool isTableControl = (controlType == WebControlType.WebControlTypeEnum.TableBegin);
                        bool isCloseTableControl = (controlType == WebControlType.WebControlTypeEnum.TableEnd);
                        bool isChekBoxControl = (controlType == WebControlType.WebControlTypeEnum.CheckBox);
                        bool isTextBoxControl = (controlType == WebControlType.WebControlTypeEnum.TextBox);
                        //
                        bool isDataControl = (controlType == WebControlType.WebControlTypeEnum.DropDown ||
                            controlType == WebControlType.WebControlTypeEnum.TextBox ||
                            controlType == WebControlType.WebControlTypeEnum.CheckBox ||
                            controlType == WebControlType.WebControlTypeEnum.DDLBanner);
                        //
                        bool isListRetrieval = (controlType == WebControlType.WebControlTypeEnum.DropDown ||
                            controlType == WebControlType.WebControlTypeEnum.DDLBanner);
                        //
                        bool isRegEx = (controlType == WebControlType.WebControlTypeEnum.TextBox);
                        //
                        bool isAddLabelControl = isDataControl && (false == isChekBoxControl);
                        //
                        //FI 20120525 garde fou où cas où le datatype n'est pas renseigné
                        //Spheres® considère que c'est du string
                        //Sans datatype la construction de la page plante
                        if (isDataControl)
                        {
                            if (dataType == TypeData.TypeDataEnum.unknown.ToString())
                                dataType = TypeData.TypeDataEnum.@string.ToString();
                        }

                        if (isTextBoxControl)
                        {
                            #region Default regEx and width and ...
                            if (StrFunc.IsEmpty(regEx))
                            {
                                if (TypeData.IsTypeDate(dataType))
                                {
                                    regEx = EFSRegex.TypeRegex.RegexDate.ToString();
                                }
                                else if (TypeData.IsTypeDateTime(dataType))
                                {
                                    regEx = EFSRegex.TypeRegex.RegexDateTime.ToString();
                                }
                                else if (TypeData.IsTypeTime(dataType))
                                {
                                    regEx = EFSRegex.TypeRegex.RegexShortTime.ToString();
                                }
                                else if (TypeData.IsTypeInt(dataType))
                                {
                                    regEx = EFSRegex.TypeRegex.RegexInteger.ToString();
                                }
                                else if (TypeData.IsTypeDec(dataType))
                                {
                                    regEx = EFSRegex.TypeRegex.RegexDecimal.ToString();
                                }
                            }
                            //
                            if (TypeData.IsTypeDate(dataType))
                            {
                                width = Cst.WidthDate.ToString();
                            }
                            else if (TypeData.IsTypeDateTime(dataType))
                            {
                                width = Cst.WidthDateTime.ToString();
                            }
                            else if (TypeData.IsTypeTime(dataType))
                            {
                                width = Cst.WidthTime.ToString();
                            }
                            else if (TypeData.IsTypeNumeric(dataType))
                            {
                                width = Cst.WidthNumeric.ToString();
                            }
                            //PL 20100122 Newness
                            else if (TypeData.IsTypeText(dataType))
                            {
                                rows = "100";
                                textMode = TextBoxMode.MultiLine.ToString();
                                width = Cst.WidthText.ToString();
                            }
                            #endregion
                        }
                        //
                        if (isAddLabelControl)
                        {
                            #region Add Label control ( with DisplayName as Caption)
                            extendNode = InputGUI.DocXML.CreateNode(XmlNodeType.Element, "Label", "");
                            //
                            AppendAttribute(extendNode, "clientid", clientid);
                            labelCaptionAttribute = AppendAttribute(extendNode, "caption", displayName);

                            // RD 20120919 Gestion de l'imbrication de controls de type "Table"
                            // Chercher la dernière "Table" ouverte
                            parentTableNode = GetLastTable(tableNodeStack);
                            AppendChild(pRoot, parentTableNode, extendNode);
                            #endregion
                        }
                        //
                        if (isCloseTableControl)
                        {
                            // RD 20120919 Gestion de l'imbrication des controls de type "Table"
                            // Supprimer (fermer) la dernière "Table" ouverte
                            if (tableNodeStack.Count > 0)
                                tableNodeStack.Pop();
                        }
                        else
                        {
                            // RD 20120917 Création d'un control web "Table", à la rencontre de l'extention de type "TableBegin"
                            if (isTableControl)
                                extendNode = InputGUI.DocXML.CreateNode(XmlNodeType.Element, "Table", "");
                            else
                                extendNode = InputGUI.DocXML.CreateNode(XmlNodeType.Element, webControlType, "");
                            //
                            if (false == isAddLabelControl)
                                AppendAttribute(extendNode, "caption", displayName);
                            //
                            if (isDataControl || isObject)
                            {
                                #region Add control Attributes
                                AppendAttribute(extendNode, "clientid", clientid);
                                AppendAttribute(extendNode, "postback", (Convert.IsDBNull(rowExtension["ISPOSTBACK"]) ? "false" : Convert.ToBoolean(rowExtension["ISPOSTBACK"]).ToString().ToLower()));
                                AppendAttribute(extendNode, "required", (Convert.IsDBNull(rowExtension["ISMANDATORY"]) ? "false" : Convert.ToBoolean(rowExtension["ISMANDATORY"]).ToString().ToLower()));
                                //                        
                                if (isDataControl)
                                {
                                    AppendAttribute(extendNode, "datatype", dataType);
                                    //
                                    if (isRegEx && StrFunc.IsFilled(regEx))
                                        AppendAttribute(extendNode, "regex", regEx);
                                    //
                                    if (StrFunc.IsFilled(width))
                                        AppendAttribute(extendNode, "width", width + "px");
                                    if (StrFunc.IsFilled(rows))
                                        AppendAttribute(extendNode, "rows", rows);
                                    if (StrFunc.IsFilled(textMode))
                                        AppendAttribute(extendNode, "textMode", textMode);
                                    //
                                    if (isListRetrieval && false == Convert.IsDBNull(rowExtension["LISTRETRIEVAL"]))
                                        AppendAttribute(extendNode, "listretrieval", Convert.ToString(rowExtension["LISTRETRIEVAL"]));
                                }
                                #endregion
                            }
                            //
                            if (DBNull.Value != rowExtension["STYLE"])
                            {
                                try
                                {
                                    #region Add style Attributes
                                    string style = (Convert.IsDBNull(rowExtension["STYLE"]) ? string.Empty : Convert.ToString(rowExtension["STYLE"]));
                                    //
                                    if (StrFunc.IsFilled(style))
                                    {
                                        string[] styles = style.Split(';');
                                        //
                                        for (int i = 0; i < styles.Length; i++)
                                        {
                                            string itemStyle = styles[i].Trim();
                                            //
                                            if (StrFunc.IsFilled(itemStyle))
                                            {
                                                string[] styleNameValue = itemStyle.Split('=');
                                                string itemStyleName = styleNameValue[0].Trim();
                                                //
                                                if (StrFunc.IsFilled(itemStyleName) && styleNameValue.Length == 2)
                                                {
                                                    string itemStyleValue = styleNameValue[1].Trim('"');
                                                    //                                                    
                                                    if (itemStyleName == "caption" && labelCaptionAttribute != null)
                                                        labelCaptionAttribute.Value = itemStyleValue;
                                                    else
                                                        AppendAttribute(extendNode, itemStyleName, itemStyleValue);
                                                }
                                                else
                                                    throw new Exception("Defined style is incorrect" + Cst.CrLf + "[Style: " + style + "]");
                                            }
                                        }
                                    }
                                    #endregion
                                }
                                catch (Exception ex) { throw new Exception("Error to apply style to Extension" + Cst.CrLf + extensionLogMsg + Cst.CrLf, ex); }
                            }
                            // RD 20120919 Gestion de l'imbrication de controls de type "Table"
                            // Chercher la drenière "Table" ouverte
                            parentTableNode = GetLastTable(tableNodeStack);
                            AppendChild(pRoot, parentTableNode, extendNode);
                            //
                            if (isTableControl)
                                tableNodeStack.Push(extendNode);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                string logMessage = "Error to display instrument Extensions" + Cst.CrLf;
                logMessage += "[Instrument: " + TradeCommonInput.DisplayNameInstrument + " [id=" + TradeCommonInput.Product.IdI.ToString() + "]]" + Cst.CrLf;
                logMessage += Cst.CrLf + ExceptionTools.GetMessageExtended(ex);
                throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, logMessage);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRowToolTip"></param>
        /// <returns></returns>
        protected string AddRowToolTip(ArrayList pRowToolTip)
        {
            string html_tt = string.Empty;
            if (0 < pRowToolTip.Count)
            {
                for (int i = 0; i < pRowToolTip.Count; i++)
                {
                    TableRow tr = (TableRow)pRowToolTip[i];
                    html_tt += "<tr class='" + tr.CssClass + "'>";
                    for (int j = 0; j < tr.Cells.Count; j++)
                    {
                        html_tt += "<td>" + tr.Cells[j].Text + "</td>";
                    }
                    html_tt += "</tr>";
                }
            }
            return html_tt;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pRoot"></param>
        /// <param name="pParentNode"></param>
        /// <param name="pExtendNode"></param>
        private void AppendChild(XmlNode pRoot, XmlNode pParentNode, XmlNode pExtendNode)
        {
            if (null == pParentNode)
                pRoot.AppendChild(pExtendNode);
            else
                pParentNode.AppendChild(pExtendNode);
        }

        /// <summary>
        /// Rtourne la drenière "Table"
        /// <para>Il faut voir {pTableNodeStack }comme une pile (dans le sens LIFO)</para>
        /// </summary>
        /// <param name="pTableNodeStack"></param>
        /// <returns></returns>
        private XmlNode GetLastTable(Stack<XmlNode> pTableNodeStack)
        {
            if (pTableNodeStack.Count > 0)
                return pTableNodeStack.Peek();
            else
                return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pExtension"></param>
        /// <param name="pAttributeName"></param>
        /// <param name="pAttributeValue"></param>
        /// <returns></returns>
        private XmlAttribute AppendAttribute(XmlNode pExtension, string pAttributeName, string pAttributeValue)
        {
            XmlAttribute attribute = InputGUI.DocXML.CreateAttribute(pAttributeName);
            attribute.Value = pAttributeValue;
            pExtension.Attributes.Append(attribute);
            //
            return attribute;
        }

        /// <summary>
        /// 
        /// </summary>
        // EG 20210222 [XXXXX] Suppression FpMLCopyPaste.js (fonctions déplacées dans PageBase.js)
        // EG 20210224 [XXXXX] Minification Trade.js
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Trade.min.js"));
            
            // FI 20191227 [XXXXX] Add
            JQuery.WriteInitialisationScripts(this, "AutoComplete", "LoadAutoCompleteData();");

            base.CreateChildControls();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIdMenu"></param>
        /// <returns></returns>
        /// EG 20120613 BlockUI New
        /// FI 20130813 alimentation du tooltip d'un menu lorsque l'action est indisponible
        /// EG 20160212 Add HttpUtility.UrlEncode (ClearSpec)
        /// FI 20170116 [21916] Modify
        /// RD 20170315 [22967] Modify
        // EG 20180514 [23812] Report
        protected skmMenu.MenuItemParent CreateChildsUserActionMenu(string pIdMenu)
        {
            #region Counting all childs menus of UserAction
            SpheresMenu.Menus menus = SessionTools.Menus;
            ArrayList aMenuChild = new ArrayList();
            for (int i = 0; i < ArrFunc.Count(menus); i++)
            {
                SpheresMenu.Menu menu = (SpheresMenu.Menu)menus[i];
                if ((menu.IdMenu_Parent.ToString() == pIdMenu) && menu.IsEnabled)
                {
                    aMenuChild.Add(menu);
                }
            }

            #endregion Counting all childs menus of UserAction
            string disabledReason = null;
            ArrayList aMenuItemParent = new ArrayList();
            for (int i = 0; i < aMenuChild.Count; i++)
            {
                #region Affecting childs of current menu
                SpheresMenu.Menu menu = (SpheresMenu.Menu)aMenuChild[i];
                IsActionAuthorized(menu.IdMenu, out bool isHidden, out bool isEnabled, out disabledReason);
                isEnabled &= SessionTools.IsActionEnabled;
                if (false == SessionTools.IsActionEnabled)
                    AddActionDisabledReason(ref disabledReason, "Msg_TradeActionDisabledReason_NoPermission");
                //                
                if (false == isHidden)
                {
                    skmMenu.MenuItemParent mnuchild = new skmMenu.MenuItemParent(0)
                    {
                        Enabled = isEnabled
                    };
                    if ((mnuchild.Enabled == false) && StrFunc.IsFilled(disabledReason))
                        mnuchild.aToolTip = Ressource.GetString("Msg_TradeActionDisabledReason") + Cst.CrLf + disabledReason;

                    if (menus.IsParent(menu.IdMenu))
                    {
                        mnuchild = CreateChildsUserActionMenu(menu.IdMenu);
                        mnuchild.eLayout = skmMenu.MenuLayout.Vertical.ToString();
                    }
                    mnuchild.eText = Ressource.GetString(menu.IdMenu.ToString());
                    if (0 < menu.Url.IndexOf("GUID"))
                    {
                        #region GUID - ex. OpenTradeAction('GUID','ExerciseEvents',...)
                        if (TradeCommonCaptureGen.TradeCommonInput.Product.IsFxOption
                            &&
                            menu.Url == "OpenTradeAction('GUID','OptionalEarlyTerminationProvisionEvents'); return false;")
                        {
                            mnuchild.eCommandName = Cst.Capture.ModeEnum.FxOptionEarlyTermination.ToString();
                            mnuchild.eArgument = menu.IdMenu;
                        }
                        else
                        {
                            mnuchild.eUrl = menu.Url.Replace("GUID", this.GUID);

                            bool existsAlternativeAction =
                                mnuchild.EvaluateURLAlternativeAction(new string[][] { 
                                skmMenu.URLAlternativeActionParser.Parameters ,
                                new string[] { 
                                        this.TradeCommonInput.DataDocument.CurrentProduct.Product.ToString(), 
                                        Convert.ToString(Tools.IsExchangeTradedDerivativeOption(
                                            this.TradeCommonInput.DataDocument.CurrentProduct.Product)),
                                        Convert.ToString(Tools.IsExchangeTradedDerivativeFuture(
                                            this.TradeCommonInput.DataDocument.CurrentProduct.Product)),
                                        menu.IdMenu
                                    } 
                                });

                            if (existsAlternativeAction)
                                mnuchild.eArgument = menu.IdMenu;
                        }
                        #endregion
                    }
                    else if (-1 < menu.Url.IndexOf("OpenTradeClearingSpecific"))
                    {
                        #region OpenTradeClearingSpecific - OTC_INP_TRD_CLEARSPEC - Compensation/Clôture spécifique
                        if (mnuchild.Enabled)
                        {
                            // EG 20111012 Codage en dur (pas beau)
                            string url = menu.Url;

                            DateTime dtBusiness = this.TradeCommonInput.GetDefaultDateAction(SessionTools.CS, Cst.Capture.ModeEnum.ClearingSpecific);
                            // EG 20150920 [21314] Int (int32) to Long (Int64) 
                            // EG 20151102 [21465] 
                            // EG 20170127 Qty Long To Decimal
                            decimal availableQty = PosKeepingTools.GetAvailableQuantity(SessionTools.CS, dtBusiness, TradeCommonInput.IdT);
                            Nullable<int> idA_CssCustodian = null;
                            RptSideProductContainer rptSide = TradeCommonInput.Product.RptSideLoadAll(CSTools.SetCacheOn(SessionTools.CS));
                            if (null != rptSide)
                            {
                                // EG 20150907 [21317] Gestion EquitySecurityTransaction et DebtSecurityTransaction
                                _ = rptSide.GetMarket(CSTools.SetCacheOn(SessionTools.CS), null, out SQL_Market sql_Market);
                                decimal? price;
                                SQL_AssetBase sql_Asset;
                                if (TradeCommonInput.Product.IsExchangeTradedDerivative)
                                {
                                    #region ExchangeTradedDerivative
                                    ExchangeTradedDerivativeContainer etd = rptSide as ExchangeTradedDerivativeContainer;
                                    price = etd.Price;
                                    sql_Asset = etd.AssetETD;
                                    #endregion ExchangeTradedDerivative
                                }
                                else if (TradeCommonInput.Product.IsEquitySecurityTransaction)
                                {
                                    #region EquitySecurityTransaction
                                    EquitySecurityTransactionContainer est = rptSide as EquitySecurityTransactionContainer;
                                    // Cours de négociation
                                    price = est.Price;
                                    idA_CssCustodian = rptSide.IdA_Custodian;
                                    sql_Asset = est.AssetEquity;
                                    #endregion EquitySecurityTransaction
                                }
                                else if (TradeCommonInput.Product.IsDebtSecurityTransaction)
                                {
                                    #region DebtSecurityTransaction
                                    DebtSecurityTransactionContainer dst = rptSide as DebtSecurityTransactionContainer;
                                    // FI 20170116 [21916] SetAssetDebtSecurity déjà effectué via l'appel à RptSideLoadAll 
                                    //dst.SetAssetDebtSecurity(CSTools.SetCacheOn(CS));
                                    price = dst.GetPrice().DecValue;
                                    idA_CssCustodian = rptSide.IdA_Custodian;
                                    sql_Asset = dst.AssetDebtSecurity;
                                    #endregion DebtSecurityTransaction
                                }
                                else if (TradeCommonInput.Product.IsReturnSwap)
                                {
                                    #region ReturnSwap
                                    ReturnSwapContainer rst = rptSide as ReturnSwapContainer;
                                    // FI 20170116 [21916] SetMainReturnLeg déjà effectué via l'appel à RptSideLoadAll 
                                    //rst.SetMainReturnLeg(CSTools.SetCacheOn(CS), null);
                                    price = rst.MainInitialNetPrice;
                                    idA_CssCustodian = rptSide.IdA_Custodian;
                                    sql_Asset = rst.MainReturnLeg.Second.SqlAsset;
                                    #endregion ReturnSwap
                                }
                                else
                                {
                                    // FI 20170116 [21916] add throw
                                    throw new NotImplementedException(StrFunc.AppendFormat("product:{0} is not impemented", TradeCommonInput.Product.Product.ToString()));
                                }


                                if ((null != rptSide) && (null != sql_Asset))
                                {
                                    IFixParty partyDealer = rptSide.GetDealer();
                                    IParty dealerParty = TradeCommonInput.DataDocument.GetParty(partyDealer.PartyId.href);
                                    IBookId dealerBook = TradeCommonInput.DataDocument.GetBookId(partyDealer.PartyId.href);
                                    IFixParty partyClearerCustodian = rptSide.GetClearerCustodian();
                                    IParty clearerCustodianParty = TradeCommonInput.DataDocument.GetParty(partyClearerCustodian.PartyId.href);
                                    IBookId clearerCustodianBook = TradeCommonInput.DataDocument.GetBookId(partyClearerCustodian.PartyId.href);

                                    // Identifiant du trade
                                    url = url.Replace("IDENTIFIER", TradeCommonInput.Identifier.ToString());
                                    url = url.Replace("IDT", this.TradeCommonInput.IdT.ToString());

                                    // Date de bourse
                                    url = url.Replace("DTBUSINESS", DtFuncML.DateTimeToStringDateISO(dtBusiness));

                                    // Quantité dispo
                                    // RD 20170315 [22967] availableQty est un decimal alors utiliser StrFunc.FmtDecimalToInvariantCulture
                                    // FI 20210323 [XXXXX] availableQty doit être en integer
                                    //url = url.Replace("AVAILABLEQTY", availableQty.ToString());
                                    url = url.Replace("AVAILABLEQTY", IntFunc.IntValue2(StrFunc.FmtDecimalToInvariantCulture(availableQty), CultureInfo.InvariantCulture).ToString());
                                    // Sens
                                    url = url.Replace("SIDE", rptSide.IsDealerBuyerOrSeller(BuyerSellerEnum.BUYER) ? BuySellEnum.BUY.ToString() : BuySellEnum.SELL.ToString());

                                    // Cours de négociation
                                    url = url.Replace("PRICE", price.HasValue ? StrFunc.FmtDecimalToInvariantCulture(price.Value) : string.Empty);

                                    // Market
                                    url = url.Replace("MARKET", sql_Market.Id.ToString());

                                    // Clearing House|Custodian
                                    url = url.Replace("CSSCUSTODIAN", idA_CssCustodian.HasValue ? idA_CssCustodian.Value.ToString() : sql_Market.IdA.ToString());

                                    // Asset

                                    url = url.Replace("ASSET", HttpUtility.UrlEncode(sql_Asset.Description, Encoding.UTF8));

                                    // Entity
                                    url = url.Replace("ENTITY", TradeCommonInput.EntityOnLoad.ToString());

                                    // Dealer
                                    url = url.Replace("BOOKDEALER", HttpUtility.UrlEncode(dealerBook.BookName, Encoding.UTF8));
                                    url = url.Replace("DEALER", HttpUtility.UrlEncode(dealerParty.PartyName, Encoding.UTF8));

                                    // Clearer|Custodian
                                    url = url.Replace("BOOKCLEARER", ((null != clearerCustodianBook) && clearerCustodianBook.BookNameSpecified) ? HttpUtility.UrlEncode(clearerCustodianBook.BookName, Encoding.UTF8) : string.Empty);
                                    url = url.Replace("CLEARER", HttpUtility.UrlEncode(clearerCustodianParty.PartyName, Encoding.UTF8));

                                    mnuchild.eUrl = url;
                                }
                            }
                        }
                        #endregion
                    }
                    else if (-1 < menu.Url.IndexOf("OpenTradeSplit"))
                    {
                        #region OpenTradeSplit - OTC_INP_TRD_SPLIT - Split
                        if (mnuchild.Enabled)
                        {
                            string url = menu.Url.Replace("Guid", this.GUID);
                            mnuchild.eUrl = url;
                        }
                        #endregion
                    }
                    else
                    {
                        mnuchild.eCommandName = menu.Url;
                        mnuchild.eArgument = menu.IdMenu;
                    }
                    mnuchild.eImageUrl = menu.Icon;
                    aMenuItemParent.Add(mnuchild);
                }
                #endregion Affecting childs of current menu
            }
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(aMenuItemParent.Count);
            for (int i = 0; i < aMenuItemParent.Count; i++)
            {
                mnu[i] = (skmMenu.MenuItemParent)aMenuItemParent[i];
                //EG 20120613 BlockUI New
                mnu[i].eBlockUIMessage = mnu[i].eText + Cst.CrLf + Ressource.GetString("Msg_WaitingRequest");
            }
            return mnu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pIsResetIdentifierDisplaynameDescriptionExtlLink"></param>
        protected void DisplayCapture(bool pIsResetIdentifierDisplaynameDescriptionExtlLink)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.DisplayCapture");

            if ((null != TradeCommonHeaderBanner) && pIsResetIdentifierDisplaynameDescriptionExtlLink)
                TradeCommonHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();

            PlaceHolder plh = new PlaceHolder();
            bool isAddPlaceHolder = true;
            if (IsTradeFound)
            {
                switch (InputGUI.CaptureType)
                {
                    case Cst.Capture.TypeEnum.Customised:
                        bool isOk = DisplayCustomizeCapture();
                        isAddPlaceHolder = (false == isOk);
                        break;
                    case Cst.Capture.TypeEnum.Full:
                        plh = DisplayFullCapture();
                        break;
                }
            }
            else
            {
                plh = GetContolsTradeNotFound();
            }

            if (isAddPlaceHolder)
                PlaceHolder.Controls.Add(plh);

            AddAuditTimeStep("End TradeCommonCapturePage.DisplayCapture");
        }

        /// <summary>
        /// Affiche les confirmations
        /// </summary>
        /// <param name="pType"></param>
        /// FI 20170913 [23417] Modify
        protected void DisplayConfirm(string pType)
        {
            bool isISDA_HTML = ("ISDA" == pType.ToUpper());
            bool isISDA_PDF = ("PDF" == pType.ToUpper());
            bool isISDA = (isISDA_HTML || isISDA_PDF);
            bool isSWIFT = ("SWIFT" == pType.ToUpper());
            bool isCustomConf = ("CUSTOMCONFIRM" == pType.ToUpper());
            
            Cst.StatusBusiness tradeStatus = (Cst.StatusBusiness)Enum.Parse(typeof(Cst.StatusBusiness), TradeCommonInput.TradeStatus.stBusiness.NewSt);
            //20140710 PL [TRIM 20179]
            string tradeStMatch = TradeCommonInput.TradeStatus.GetTickedTradeStUser(StatusEnum.StatusMatch, "}{");
            string tradeStCheck = TradeCommonInput.TradeStatus.GetTickedTradeStUser(StatusEnum.StatusCheck, "}{");
            
            //Default OutputMode
            DocTypeEnum outputDocType = DocTypeEnum.html;
            if (isISDA)
                outputDocType = DocTypeEnum.html;
            else if (isSWIFT)
                outputDocType = DocTypeEnum.txt;
            
            #region CheckControl
            //check Event exist (mode isConf only)
            //exit function when Event not generated
            if (isCustomConf)
            {
                bool isEventOk = ArrFunc.IsFilled(TradeRDBMSTools.GetListEventProcess(SessionTools.CS,
                    TradeCommonInput.IdT, new Cst.ProcessTypeEnum[] { Cst.ProcessTypeEnum.EVENTSGEN }, null, ProcessStateTools.StatusSuccessEnum));
                if (false == isEventOk)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                        Ressource.GetString2("Msg_NoEvent", TradeCommonInput.Identification.Identifier));
                }
            }
            // EG 20220523 [WI639] Réduction des parties (Buyer/Seller) sur les instruments de facturation

            List<int> partyRestrict = new List<int>();
            if (TradeCommonInput.DataDocument.CurrentProduct.IsADM)
                partyRestrict.AddRange(new int[] { TradeCommonInput.SQLTrade.IdA_Buyer.Value, TradeCommonInput.SQLTrade.IdA_Seller.Value });


            //ConfirmationChain[] cnfChain = ConfirmationTools.GetConfirmationChain(CSTools.SetCacheOn(CS), TradeCommonInput.DataDocument, null);
            ConfirmationChain[] cnfChain = ConfirmationTools.GetConfirmationChain2(CSTools.SetCacheOn(SessionTools.CS), TradeCommonInput.DataDocument, partyRestrict);
            if (false == (ArrFunc.IsFilled(cnfChain)))
            {
                //FI 20120215 Il n'y a pas nécessairement d'émettre de la messagerie sur les Trades Risk
                //Ex un CashBalance entre l'entité et une chambre ne donnent lieu à aucune messagerie 
                //Il n'y a aucune pas de chaîne de confirmation ds ce cas
                //FI 20120315 [17724] Certains trades ne génère pas nécessairement de la messagerie
                bool IsTradeOptionalMessage = ConfirmationTools.IsTradeOptionalMessage(SessionTools.CS, TradeCommonInput.SQLInstrument.Id, tradeStatus);

                if (false == IsTradeOptionalMessage)
                {
                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, Ressource.GetString("Msg_CnfProcess_NoConfirmationChain"));
                }
            }
            #endregion CheckControl
           
            ArrayList alOpenFile = new ArrayList();
            ArrayList alMsg = new ArrayList();
           
            if (ArrFunc.IsEmpty(cnfChain))
            {
                alMsg.Add(Ressource.GetString("Msg_CnfProcess_NoConfirmationChainOk"));
                MsgForAlertImmediate = (String)alMsg[0];
            }
            else
            {

                #region for (int i = 0; i < ArrFunc.Count(cnfChain); i++)
                for (int i = 0; i < ArrFunc.Count(cnfChain); i++)
                {
                    string msgHeaderCnfChain = Ressource.GetString2("Msg_CnfProcess_ConfirmationChainHeader",
                                                        Cst.CrLf + cnfChain[i].GetDisplay(CSTools.SetCacheOn(SessionTools.CS)));
                    string msgHeader = msgHeaderCnfChain;
                    StrBuilder msg = new StrBuilder(Ressource.GetString2("Msg_CnfProcess_ResultHeader"));
                    //
                    bool isGenerateMessage = true;
                    //Vérification de l'existence d'un contact office destinataire
                    if (isGenerateMessage)
                    {
                        isGenerateMessage = cnfChain[i].IsExistSendToContactOffice;
                        if (false == isGenerateMessage)
                        {
                            msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_SendToContactOfficeNotFound"));
                            alMsg.Add(msgHeader + Cst.CrLf2 + msg);
                        }
                    }
                    //Vérification de l'existence que la chaîne de confirmation est valide
                    if (isGenerateMessage)
                    {
                        isGenerateMessage = cnfChain[i].IsValid;
                        if (false == isGenerateMessage)
                        {
                            msg.Append(Cst.Space + Ressource.GetString2("Msg_CnfProcess_ConfirmationChainNotValid", cnfChain[i].CheckConfirmationChain()));
                            alMsg.Add(msgHeader + Cst.CrLf2 + msg);
                        }
                    }
                    //Vérification des  éventuelles contre-indication sous entity et sous le book
                    if (isGenerateMessage)
                    {
                        isGenerateMessage = cnfChain[i].IsGenerateMessage(SessionTools.CS, NotificationClassEnum.MONOTRADE, out string msgIsGenerate);
                        if (false == isGenerateMessage)
                        {
                            msg.Append(Cst.Space + msgIsGenerate);
                            alMsg.Add(msgHeader + Cst.CrLf2 + msg);
                        }
                    }

                    if (isGenerateMessage)
                    {
                        // FI 20170913 [23417] Utilisation de sqlContract
                        /*
                        // FI 20121004 [18172]
                        SQL_DerivativeContract sqlDerivativeContract = null;
                        TradeCommonInput.DataDocument.currentProduct.GetDerivativeContract(CSTools.SetCacheOn(CS), null, out sqlDerivativeContract);
                        */
                        Pair<Cst.ContractCategory, int> sqlContractId = null;
                        TradeCommonInput.DataDocument.CurrentProduct.GetContract(CSTools.SetCacheOn(SessionTools.CS), null, 
                            SQL_Table.ScanDataDtEnabledEnum.No, DateTime.MinValue, out Pair<Cst.ContractCategory, SQL_TableWithID> sqlContract);
                        if (null != sqlContract)
                            sqlContractId = new Pair<Cst.ContractCategory, int>(sqlContract.First, sqlContract.Second.Id);

                        // FI 20140808 [20275] valorisation de sqlMarket
                        TradeCommonInput.Product.GetMarket(CSTools.SetCacheOn(SessionTools.CS), null, out SQL_Market sqlMarket);

                        //
                        // Recherche des messages à envoyer: Ici, seul les messages StepLife==StepLifeEnum.INITIAL sont considérés
                        NotificationStepLifeEnum[] stepLife = new NotificationStepLifeEnum[] { NotificationStepLifeEnum.INITIAL };
                        //20140710 PL [TRIM 20179]
                        // FI 20170913 [23417] Utilisation de sqlContractId
                        LoadMessageSettings msgSettings = new LoadMessageSettings( 
                            new NotificationClassEnum[] { NotificationClassEnum.MONOTRADE }, null,
                            TradeCommonInput.Product.IdI, (sqlMarket!=null)? sqlMarket.Id : 0,
                            sqlContractId,
                            tradeStatus,tradeStMatch,tradeStCheck,
                            stepLife, null);
                        //
                        CnfMessages cnfMessages = cnfChain[i].LoadCnfMessage(CSTools.SetCacheOn(SessionTools.CS), msgSettings);
                        //
                        //supprimser les messages sans NCS compatible
                        cnfMessages.RemoveMessageWithoutNcsMatching();
                        //
                        #region isISDA || isSWIFT
                        if (isISDA || isSWIFT)
                        {
                            // Si ISDA ou SWIFT => 1 seul message
                            // S'il existe ce message en base => prise en considération des attributs spécifiés sur celui-ci
                            //CnfMessage cnfMessageDefault = ConfirmationTools.GetCnfMessageDefault(CS, pType, TradeCommonInput.DataDocument);
                            CnfMessage cnfMessageDefault = TradeCommonInput.DataDocument.GetCnfMessageDefault(CSTools.SetCacheOn(SessionTools.CS), pType);
                            //
                            bool isFind = false;
                            for (int k = 0; k < ArrFunc.Count(cnfMessages.cnfMessage); k++)
                            {
                                isFind = (cnfMessageDefault.identifier == cnfMessages[k].identifier);
                                if (isFind)
                                {
                                    CnfMessage cnfMessageFind = cnfMessages[k];
                                    cnfMessages = new CnfMessages
                                    {
                                        cnfMessage = new CnfMessage[] { cnfMessageFind }
                                    };
                                    break;
                                }
                            }
                            if (false == isFind)
                            {
                                cnfMessages = new CnfMessages
                                {
                                    cnfMessage = new CnfMessage[] { cnfMessageDefault }
                                };
                            }
                        }
                        #endregion
                        //
                        if (null == cnfMessages || ArrFunc.IsEmpty(cnfMessages.cnfMessage))
                        {
                            msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_NoMessageFound"));
                            alMsg.Add(msgHeader + Cst.CrLf2 + msg);
                        }
                        //
                        //Balayage de tous les messages trouvés
                        if (null != cnfMessages)
                        {
                            #region for (int j = 0; j < ArrFunc.Count(cnfMessages.cnfMessage); j++)
                            for (int j = 0; j < ArrFunc.Count(cnfMessages.cnfMessage); j++)
                            {
                                //
                                CnfMessage cnfMessage = cnfMessages.cnfMessage[j];
                                string msgHeaderCnfMessage = Ressource.GetString2("Msg_CnfProcess_MessageHeader", Cst.Space + cnfMessage.identifier);
                                msg = new StrBuilder(Ressource.GetString2("Msg_CnfProcess_ResultHeader"));
                                //
                                AppSession session = SessionTools.AppSession;
                                CnfMessageToSend cnfMessageToSend = new CnfMessageToSend(cnfMessage);
                                int idIUnderlyer = IntFunc.ConvertToInt(TradeCommonInput.Product.GetUnderlyingAssetIdI());
                                string idC = Tools.GetIdC(CSTools.SetCacheOn(SessionTools.CS), TradeCommonInput.Product.GetMainCurrency(CSTools.SetCacheOn(SessionTools.CS)));
                                string tradeStBusiness = TradeCommonInput.TradeStatus.stBusiness.NewSt;

                                #region Recherche des instruction
                                CnfMessageToSend.SetNcsInciChainErrLevel errLevel = CnfMessageToSend.SetNcsInciChainErrLevel.Undefined;
                                if (cnfMessage.IsNcsMatching())
                                {
                                    //20140710 PL [TRIM 20179]
                                    // FI 20170913 [23417] Utilisation de sqlContractId
                                    errLevel = cnfMessageToSend.SetNcsInciChain(CSTools.SetCacheOn(SessionTools.CS), cnfChain[i],
                                        (sqlMarket != null) ? sqlMarket.Id : 0,
                                        TradeCommonInput.Product.IdI, idIUnderlyer,
                                        idC, sqlContractId,
                                        tradeStBusiness, tradeStMatch, tradeStCheck);
                                }
                                //
                                if (isISDA || isSWIFT)
                                {
                                    bool isAddVirtualInciChain = false;
                                    if (false == isAddVirtualInciChain)
                                    {
                                        if (errLevel == CnfMessageToSend.SetNcsInciChainErrLevel.Undefined)
                                        {
                                            //Vrai si aucune recherche d'instruction n'a été effectué 
                                            //Vrai lorsque isda ou swift, que  le message est compatable avec le NCS SCREEN et que ce dernier actor est inexistant en base) 
                                            isAddVirtualInciChain = true;
                                        }
                                    }
                                    //
                                    if (false == isAddVirtualInciChain)
                                        isAddVirtualInciChain = (errLevel != CnfMessageToSend.SetNcsInciChainErrLevel.Succes);
                                    //
                                    // En mode isISDA ou isSWIFT Attribution d'instruction automatique afin d'afficher les messages de confirmation 
                                    if (isAddVirtualInciChain)
                                    {
                                        NotificationConfirmationSystem ncs = ConfirmationTools.LoadNotificationConfirmationSystem(CSTools.SetCacheOn(SessionTools.CS), SQL_TableWithID.IDType.Identifier, ConfirmationTools.NCS_SCREEN);
                                        if (null == ncs)
                                            ncs = new NotificationConfirmationSystem(0, ConfirmationTools.NCS_SCREEN, string.Empty);
                                        //
                                        InciChain inciChain = new InciChain();
                                        cnfMessageToSend.NcsInciChain = new NcsInciChain[] { new NcsInciChain(ncs, inciChain) };
                                        //
                                        errLevel = CnfMessageToSend.SetNcsInciChainErrLevel.Succes;
                                    }
                                }
                                //
                                #region add message si instruction non trouvée
                                if (false == (CnfMessageToSend.SetNcsInciChainErrLevel.Succes == errLevel))
                                {
                                    if (CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendTo == errLevel)
                                    {
                                        msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendTo"));
                                        alMsg.Add(msgHeader + Cst.CrLf2 + msgHeaderCnfMessage + Cst.CrLf2 + msg);
                                    }
                                    else if (CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy == errLevel)
                                    {
                                        msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy"));
                                        alMsg.Add(msgHeader + Cst.CrLf2 + msgHeaderCnfMessage + Cst.CrLf2 + msg);
                                    }
                                    else if (
                                        (CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToClient == errLevel)
                                        ||
                                        (CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToEntity == errLevel)
                                        ||
                                        (CnfMessageToSend.SetNcsInciChainErrLevel.NotFound_CI_on_SendBy_for_DefaultToExternalCtr == errLevel)
                                        )
                                    {
                                        if (cnfChain[i].IsContactOfficesIdentical)
                                        {
                                            if (cnfChain[i].IsSendTo_Client(CSTools.SetCacheOn(SessionTools.CS)))
                                                msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy_for_DefaultToClient"));
                                            else if (cnfChain[i].IsSendTo_Entity(CSTools.SetCacheOn(SessionTools.CS), NotificationClassEnum.MONOTRADE))
                                                msg.Append(Cst.Space + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy_for_DefaultToEntity"));
                                            else
                                                throw new NotImplementedException("SendTo External Partie and Contact office are identical");
                                        }
                                        else
                                        {
                                            //Pas d'instruction côté du Destinataire et pas d'instruction par "défaut" côté Emettteur
                                            msg.Append(Cst.CrLf + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendTo"));
                                            //
                                            if (cnfChain[i].IsSendTo_Client(CSTools.SetCacheOn(SessionTools.CS)))
                                                msg.Append(Cst.CrLf + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy_for_DefaultToClient"));
                                            else if (cnfChain[i].IsSendTo_Entity(CSTools.SetCacheOn(SessionTools.CS), NotificationClassEnum.MONOTRADE))
                                                msg.Append(Cst.CrLf + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy_for_DefaultToEntity"));
                                            else
                                                msg.Append(Cst.CrLf + Ressource.GetString("Msg_CnfProcess_NotFound_CI_on_SendBy_for_DefaultToExternalCtr"));
                                        }
                                        alMsg.Add(msgHeader + Cst.CrLf2 + msgHeaderCnfMessage + Cst.CrLf2 + msg);
                                    }
                                    else
                                    {
                                        throw new NotImplementedException("errLevel[code:" + errLevel.ToString() + "] not implemented on CnfMessageToSend.SetNcsInciChainErrLevel");
                                    }
                                }
                                #endregion
                                #endregion
                                //
                                if (CnfMessageToSend.SetNcsInciChainErrLevel.Succes == errLevel)
                                {
                                    // Calcul date Emission
                                    DateTime dateMsg = DateTime.MinValue;
                                    Pair<DateTime,DateTime> [] eventdates = cnfMessage.GetTriggerEventDate(SessionTools.CS, TradeCommonInput.IdT);
                                    if (ArrFunc.IsFilled(eventdates))
                                    {
                                        // FI 20180616 [24718] dateMsg est fonction du mode 
                                        switch (ConfirmationTools.MCOmode)
                                        {
                                            case ConfirmationTools.MCOModeEnum.DTEVENT:
                                                dateMsg = eventdates[0].First;
                                                break;
                                            case ConfirmationTools.MCOModeEnum.DTEVENTFORCED:
                                                dateMsg = eventdates[0].Second;
                                                break;
                                            default:
                                                throw new InvalidProgramException(StrFunc.AppendFormat("{0} is not supported", ConfirmationTools.MCOmode.ToString()));
                                        }
                                    }
                                    else if (isISDA || isSWIFT)
                                        dateMsg = OTCmlHelper.GetDateBusiness(SessionTools.CS);
                                    //
                                    if (DtFunc.IsDateTimeFilled(dateMsg))
                                    {
                                        DateTime dtToSend = cnfMessage.GetDateToSend(SessionTools.CS, dateMsg, cnfChain[i], TradeCommonInput.DataDocument.CurrentProduct.Product.ProductBase);
                                        DateTime dtToSendForced = OTCmlHelper.GetAnticipatedDate(SessionTools.CS, dtToSend);
                                        cnfMessageToSend.DateInfo = new NotificationSendDateInfo[] { new NotificationSendDateInfo(dateMsg, dtToSend, dtToSendForced) };
                                    }
                                    //
                                    if (ArrFunc.IsEmpty(cnfMessageToSend.DateInfo))
                                    {
                                        for (int l = 0; l < ArrFunc.Count(cnfMessageToSend.eventTrigger); l++)
                                        {
                                            msg.Append(Cst.Space + Ressource.GetString2("Msg_CnfProcess_EventNotFound", cnfMessageToSend.eventTrigger[l].eventCode.ToString(), cnfMessageToSend.eventTrigger[l].eventType.ToString(), cnfMessageToSend.eventTrigger[l].eventClass.ToString()));
                                        }

                                        alMsg.Add(msgHeader + Cst.CrLf2 + msgHeaderCnfMessage + Cst.CrLf2 + msg);
                                    }
                                    //
                                    if (ArrFunc.IsFilled(cnfMessageToSend.DateInfo))
                                    {
                                        for (int k = 0; k < ArrFunc.Count(cnfMessageToSend.NcsInciChain); k++)
                                        {
                                            try
                                            {
                                                //NotificationConfirmationSystem ncs = ConfirmationTools.LoadNotificationConfirmationSystem(CS, SQL_TableWithID.IDType.Id, cnfMessageToSend.ncsInciChain[k].ncs.idNcs.ToString());
                                                NotificationConfirmationSystem ncs = cnfMessageToSend.NcsInciChain[k].ncs;

                                                string msgHeaderNcs = Ressource.GetString2("Msg_CnfProcess_NCSHeader", Cst.Space + ncs.identifier);
                                                msgHeaderCnfMessage += Cst.CrLf2 + msgHeaderNcs;
                                                //
                                                InciItems inciItems = ConfirmationTools.GetInciItems(SessionTools.CS, cnfMessage, TradeCommonInput.DataDocument, cnfChain[i], cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendBy].idInci,
                                                                                                                       cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendTo].idInci);
                                                #region Constitution "physique" du message
                                                string key = string.Empty;
                                                key += cnfMessageToSend.identifier + "_";
                                                key += cnfChain[i].SendByContactOffice + "_" + cnfChain[i].SendToContactOffice + "_";
                                                key += ncs.identifier;
                                                //
                                                string WindowID = FileTools.GetUniqueName("Trade", TradeCommonInput.Identifier);
                                                string physicalPath = SessionTools.TemporaryDirectory.MapPath("Trade" + "_" + key);
                                                string write_File = physicalPath + @"\" + WindowID;
                                                string relativePath = SessionTools.TemporaryDirectory.Path + "Trade" + "_" + key;
                                                string open_File = relativePath + @"/" + WindowID;
                                                //
                                                NotificationBuilder cnfMessageBuilder = null;
                                                #region Build Data Flow
                                                try
                                                {
                                                    cnfMessageBuilder = new NotificationBuilder(session, cnfMessage, outputDocType, new UTF8Encoding());
                                                    //20090602 [16497] FI soit une opération en dès le dans dateToSendForced il y la date système (appal à GetAnticipatedDate)
                                                    cnfMessageBuilder.SetNotificationDocument(SessionTools.CS, cnfMessageToSend.DateInfo[0].dateToSend, null, 0, ncs,
                                                        cnfMessageToSend.NcsInciChain[k].inciChain[SendEnum.SendBy].idInci, cnfChain[i],
                                                        inciItems, new int[] { TradeCommonInput.IdT }, (IProductBase)TradeCommonInput.Product.Product);
                                                    //cnfMessageBuilder.LoadMessageDoc(CS, cnfMessageToSend.dateInfo[0].dateToSendForced, 0, ncs, cnfChain[i], inciItems, TradeCommonInput.IdT, (IProductBase)TradeCommonInput.Product);

                                                    // FI 20161114 [RATP] si ISDA alors ressource en anglais  
                                                    if (isISDA)
                                                        cnfMessageBuilder.NotificationDocument.Culture = "en-GB";
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Error on Build Message", ex);
                                                }
                                                #endregion
                                                //
                                                #region Serialize
                                                try
                                                {
                                                    cnfMessageBuilder.SerializeDoc();
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Error on serialize Message", ex);
                                                }
                                                #endregion
                                                //
                                                #region Write serialize XML flow Result on File
                                                string write_serializeFile = write_File + "CnfMessage" + "." + "xml";
                                                try
                                                {
                                                    FileTools.WriteStringToFile(cnfMessageBuilder.SerializeDocWithoutXmlns.ToString(), write_serializeFile);
                                                    if (IsTrace)
                                                        alOpenFile.Add(key + "CnfMessage" + ";" + open_File + "CnfMessage" + "." + "xml");
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                                        "Error on writing File" + Cst.CrLf + write_serializeFile, ex);
                                                }
                                                #endregion
                                                //
                                                #region Transformation XSL
                                                try
                                                {
                                                    bool isConvertHTMLtoPDF = (isISDA_PDF);
                                                    cnfMessageBuilder.TransForm(SessionTools.CS, isConvertHTMLtoPDF, physicalPath, (IProductBase)TradeCommonInput.Product.Product, TradeCommonInput.IdT, 0);
                                                    if (IsTrace)
                                                    {
                                                        string write_xslFile = write_File + "Transform" + "." + "xsl";
                                                        XmlDocument xmldoc = new XmlDocument();
                                                        xmldoc.Load(cnfMessageBuilder.XslFile);
                                                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_xslFile);
                                                        alOpenFile.Add(key + "Transform" + ";" + open_File + "Transform" + "." + "xsl");
                                                    }
                                                }
                                                catch (FileNotFoundException ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, Ressource.GetString2("Msg_FileNotFound", ex.FileName));
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name, "Error on TransForm xsl", ex);
                                                }
                                                #endregion
                                                //
                                                #region Write Transformation Result on File
                                                String result = cnfMessageBuilder.Result;
                                                byte[] binaryResult = cnfMessageBuilder.BinaryResult;
                                                try
                                                {
                                                    if ((null != result) || (null != binaryResult))
                                                    {
                                                        if (null != binaryResult)
                                                        {
                                                            if (IsTrace)
                                                            {
                                                                if (cnfMessageBuilder.OutputBinDocType == "pdf")
                                                                {
                                                                    string tmpFile = write_File + ".xslfo.xml";
                                                                    string tmpOpenFile = open_File + ".xslfo.xml";
                                                                    FileTools.WriteStringToFile(result, tmpFile);
                                                                    alOpenFile.Add(key + "xslfo" + ";" + tmpOpenFile);
                                                                }
                                                            }
                                                            write_File += "." + cnfMessageBuilder.OutputBinDocType.ToString();
                                                            open_File += "." + cnfMessageBuilder.OutputBinDocType;
                                                            FileTools.WriteBytesToFile(binaryResult, write_File, FileTools.WriteFileOverrideMode.Override);
                                                            alOpenFile.Add(key + ";" + open_File);
                                                        }
                                                        else if (null != result)
                                                        {
                                                            if (DocTypeEnum.html == cnfMessageBuilder.OutputMsgDocType)
                                                                result = XMLTools.ReplaceHtmlTagImage(SessionTools.CS, result, physicalPath, relativePath);
                                                            //
                                                            write_File += "." + cnfMessageBuilder.OutputMsgDocType.ToString();
                                                            open_File += "." + cnfMessageBuilder.OutputMsgDocType.ToString();

                                                            FileTools.WriteStringToFile(result, write_File);
                                                            alOpenFile.Add(key + ";" + open_File);
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new SpheresException2(MethodInfo.GetCurrentMethod().Name,
                                                        "Error on writing File " + write_File, ex);
                                                }
                                                #endregion
                                                //
                                                #endregion Constitution "physique" du message
                                            }
                                            catch (Exception ex)
                                            {
                                                WriteLogException(ex);
                                                //
                                                msg.Append(Cst.Space + ex.Message);
                                                alMsg.Add(msgHeader + Cst.CrLf2 + msgHeaderCnfMessage + Cst.CrLf2 + msg);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion
                        }
                    }
                }
                #endregion
                //
                if (ArrFunc.IsFilled(alOpenFile))
                {
                    string[] keyOpenFile = (string[])alOpenFile.ToArray(typeof(string));
                    for (int i = 0; i < ArrFunc.Count(keyOpenFile); i++)
                    {
                        string[] splitResult = keyOpenFile[i].Split(';');
                        string WindowID = /*splitResult[0];*/ StrFunc.AppendFormat("{0}{1}", splitResult[0], i.ToString());

                        string open_File = splitResult[1];
                        //
                        AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
                    }
                }
                //
                if (ArrFunc.IsFilled(alMsg))
                {
                    string msg = Ressource.GetString("Msg_CnfProcess_ErrorOccurs") + Cst.CrLf2;
                    for (int i = 0; i < alMsg.Count; i++)
                        msg += alMsg[i].ToString() + Cst.CrLf2 + Cst.CrLf;
                    //
                    MsgForAlertImmediate = msg;
                }
            }
        }

        /// <summary>
        /// Génère un script JS qui affiche le trade au format XML 
        /// </summary>
        /// <param name="pAddValidateProcess"></param>
        /// <param name="pDisplayMsgAlertConfirm"></param>
        protected void DisplayTradeXml(bool pAddValidateProcess, bool pDisplayMsgAlertConfirm)
        {

            if (0 < TradeCommonInput.IdT)
            {
                string WindowID = FileTools.GetUniqueName("Trade", TradeCommonInput.Identifier);
                string write_FpMLFile = SessionTools.TemporaryDirectory.MapPath("Trade_xml") + @"\" + WindowID + ".xml";
                string open_FpMLFile = SessionTools.TemporaryDirectory.Path + "Trade_xml" + @"/" + WindowID + ".xml";

                EFS_SerializeInfo serializeInfo = new EFS_SerializeInfo(TradeCommonInput.FpMLDataDocReader, this.Request.MapPath("~/BusinessSchemas/"))
                {
                    Source = TradeCommonInput.ProductSource()
                };

                if (pAddValidateProcess)
                {
                    StringBuilder sb = CacheSerializer.Serialize(serializeInfo, Encoding.UTF8);
                    XSDValidation xsd = new XSDValidation(serializeInfo.Schemas);
                    xsd.WriteFile(sb.ToString(), write_FpMLFile);
                }
                else
                {
                    CacheSerializer.Serialize(serializeInfo, write_FpMLFile);
                }

                string msgConfirm = string.Empty;
                if (pDisplayMsgAlertConfirm)
                    msgConfirm = Ressource.GetString("Msg_DisplayXML");

                AddScriptWinDowOpenFile(WindowID, open_FpMLFile, msgConfirm);
            }
        }

        /// <summary>
        /// Affichage de TradeInput
        /// Permet de générer un model réutilisable directement dans le fichier de mapping pour importer des trade
        /// </summary>
        /// FI 20170116 [21916] Modify
        /// FI 20170214 [XXXXX] Modify
        protected virtual void DisplayTradeInput()
        {

            if (0 < TradeCommonInput.IdT)
            {
                string WindowID = FileTools.GetUniqueName("TradeInput", TradeCommonInput.Identifier);
                string write_FpMLFile = SessionTools.TemporaryDirectory.MapPath(@"TradeInput_xml") + @"\" + WindowID + ".xml";
                string open_FpMLFile = SessionTools.TemporaryDirectory.Path + "TradeInput_xml" + @"/" + WindowID + ".xml";
                //
                TradeImport tradeImport = new TradeImport
                {
                    //Settings
                    settings = new ImportSettings()
                    {
                        importModeSpecified = true,
                        user = SessionTools.Collaborator_IDENTIFIER,
                    },
                    settingsSpecified = true
                };
                tradeImport.settings.importMode = new ImportMode
                {
                    value = Cst.Capture.ModeEnum.New.ToString()
                };
                tradeImport.settings.userSpecified = StrFunc.IsFilled(tradeImport.settings.user);
                //Parameter
                //AddImportParameter
                ImportParameter[] importParam = GetImportParameter();
                tradeImport.settings.parametersSpecified = ArrFunc.IsFilled(importParam);
                if (tradeImport.settings.parametersSpecified)
                    tradeImport.settings.parameter = importParam;

                tradeImport.tradeInputSpecified = true;
                tradeImport.tradeInput = new TradeImportInput
                {
                    CustomCaptureInfos = new TradeCommonCustomCaptureInfos()
                };

                for (int i = 0; i < ArrFunc.Count(TradeCommonInput.CustomCaptureInfos); i++)
                {
                    CustomCaptureInfo cci = TradeCommonInput.CustomCaptureInfos[i];

                    bool isAdd = cci.ClientId_Prefix != Cst.BUT;
                    isAdd = isAdd && (false == TradeCommonInput.CustomCaptureInfos.CciTradeCommon.IsCci_Party(CciTradeParty.CciEnum.side, TradeCommonInput.CustomCaptureInfos[i]));

                    if (isAdd)
                    {
                        IParty party = null;
                        if (TradeCommonInput.CustomCaptureInfos.CciTradeCommon.IsClientId_PayerOrReceiver(TradeCommonInput.CustomCaptureInfos[i]))
                            party = TradeCommonInput.CustomCaptureInfos.CciTradeCommon.DataDocument.GetParty(TradeCommonInput.CustomCaptureInfos[i].NewValue, PartyInfoEnum.id);

                        //Dans TradeInput les payer et receiver sont alimentés avec ACTOR.IDENTIFIER et non ACTOR.XMLID                                                          
                        CustomCaptureInfoDynamicData cciDD = new CustomCaptureInfoDynamicData(TradeCommonInput.CustomCaptureInfos[i]);
                        if (null != party)
                            cciDD.NewValue = party.PartyId;

                        //pour les decimal => RegEx = None .NewValue contient la donnée decimale  
                        //Ex si user a saisie 1.5% on aura dans le cci 0.015
                        if (cciDD.DataType == TypeData.TypeDataEnum.@decimal)
                            cciDD.Regex = EFSRegex.TypeRegex.None;

                        //si une donnée de type string avec RegEx=RegexPercentFraction 
                        //si elle est alimentée avec un decimal alors  dans le cci on aura la valeur decimale
                        if (cciDD.DataType == TypeData.TypeDataEnum.@string && cciDD.Regex == EFSRegex.TypeRegex.RegexPercentFraction)
                        {
                            if (cciDD.IsDataValidForFixedRate(cciDD.NewValue))
                            {
                                cciDD.DataType = TypeData.TypeDataEnum.@decimal;
                                cciDD.Regex = EFSRegex.TypeRegex.None;
                            }
                        }
                        //
                        tradeImport.tradeInput.CustomCaptureInfos.Add(cciDD);
                    }
                }
                // On supprime les prefixes avant serialisation 
                for (int i = 0; i < ArrFunc.Count(tradeImport.tradeInput.CustomCaptureInfos); i++)
                {
                    CustomCaptureInfo cci = tradeImport.tradeInput.CustomCaptureInfos[i];
                    cci.ClientId = cci.ClientId_WithoutPrefix;
                }
                //

                EFS_SerializeInfoBase serializeInfo = new EFS_SerializeInfoBase(tradeImport.GetType(), tradeImport);
                CacheSerializer.Serialize(serializeInfo, write_FpMLFile);
                //
                AddScriptWinDowOpenFile(WindowID, open_FpMLFile, string.Empty);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ExecFunction()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void ExecFunctionAfterSynchronize()
        {
        }

        /// <summary>
        /// Retourne  les parametres nécessaires à l'importation
        /// </summary>
        public virtual ImportParameter[] GetImportParameter()
        {
            throw new NotImplementedException("GetImportParameter is not NotImplemented, please override GetImportParameter Method ");

        }

        /// <summary>
        /// Retourne le bouton qui permet l'affichahe des confirmations
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_CONFIRM()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnCustomConfirm", "C", "CONF", "btnCustomConfirmToolTip");
            btn.Click += new EventHandler(OnCustomConfirmClick);
            return btn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_EXPORT()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnExport", "E", "EVENT", "btnXMLEventToolTip");
            btn.Click += new EventHandler(OnExportClick);
            return btn;
        }

        /// <summary>
        /// Retourne le bouton qui permet l'affichage du trade XML
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_XML()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnXML", "X", "XML", "btnXMLToolTip");
            btn.Click += new EventHandler(OnXmlClick);
            return btn;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_LOG()
        {
            return GetLinkButtonFooter("btnLog", "L", "LOG", "btnLogTradeToolTip");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual WCToolTipLinkButton GetButtonFooter_INPUTTRADE()
        {
            WCToolTipLinkButton btn = GetLinkButtonFooter("btnInputTrade", "L", "MAPPING", "btnInputTradeToolTip");
            btn.Click += new EventHandler(OnTradeInputClick);
            return btn;
        }

        /// <summary>
        /// Retourne les cellules qui provoque la modification du trade (avec régénération des Events)
        /// Ces cellules doivent porter un message de confirmation
        /// 2 cellules => la cellule mère "Modification" et le cellule "Modification avec regénération de l'échéancier"
        /// </summary>
        /// <remarks>20090327 FI [16554]</remarks> 
        /// <returns></returns>
        private TableCell[] GetCellModification()
        {

            ArrayList al = new ArrayList();
            TableCell[] ret = null;
            //
            Control ctrlContainer = FindControl("tblMenu");
            if (null != ctrlContainer)
            {
                Table table = (Table)ctrlContainer.FindControl("btnModification");
                if (null != table)
                {
                    if (ArrFunc.IsFilled(table.Rows) && ArrFunc.IsFilled(table.Rows[0].Cells))
                        al.Add(table.Rows[0].Cells[0]);
                }
                //
                table = (Table)ctrlContainer.FindControl("btnModification-sm");
                if (null != table)
                {
                    if (ArrFunc.IsFilled(table.Rows) && ArrFunc.IsFilled(table.Rows[0].Cells))
                    {
                        for (int j = 0; j < ArrFunc.Count(table.Rows[0].Cells); j++)
                            al.Add(table.Rows[0].Cells[j]);
                    }
                }
                //
                if (ArrFunc.IsFilled(al))
                    ret = (TableCell[])al.ToArray(typeof(TableCell));
            }
            return ret;

        }

        /// <summary>
        /// Retourne le libellé associé au mot clef {pKey}
        /// </summary>
        /// <param name="pKey">représente le mot clef</param>
        /// <returns></returns>
        protected override string GetDisplayKey(DisplayKeyEnum pKey)
        {
            string ret;
            switch (pKey)
            {
                case DisplayKeyEnum.DisplayKey_Instrument:
                    ret = TradeCommonInput.DisplayNameInstrument;
                    break;
                default:
                    ret = string.Empty;
                    break;
            }
            return ret;
        }


        // EG 20210322 [XXXXX] Recherche Trade par fonction d'AGREGAT(Min/Max).
        // EG 20210322 [XXXXX] Plus d'usage de "offset 0 rows fetch next 1 rows only" (car mauvais temps de réponse)
        // FI 20230619 [XXXXX][WI663] Mise en place de la syntaxe select Top sur SQLSERVER uniquement puisque les performances sont nettement meilleures 
        // FI 20230619 [XXXXX][WI663] Appel à la fonction DataHelper.GetSelectTop (performances accrues sur SQLServer et ORacle) => l'usage de "fetch next 1 rows only" sur Oracle est de nouveau appliqué (Tests performance effectués avec succès sur backup récent de la base SELLA).
        /// EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (On ne lit que les trades avec TRADEXML présent)
        protected int GetIdTrade(Cst.Capture.MenuConsultEnum pCaptureMenuConsult, int pIdT)
        {
            int retId = 0;
            string addSQLWhere = string.Empty;
            
            string ordertype = string.Empty;

            switch (pCaptureMenuConsult)
            {
                case Cst.Capture.MenuConsultEnum.FirstTrade:
                    ordertype = "asc";
                    break;
                case Cst.Capture.MenuConsultEnum.EndTrade:
                    ordertype = "desc";
                    break;
                case Cst.Capture.MenuConsultEnum.PreviousTrade:
                    addSQLWhere = " and (tr.IDT < @IDT)";
                    ordertype = "desc";
                    break;
                case Cst.Capture.MenuConsultEnum.NextTrade:
                    addSQLWhere = " and (tr.IDT > @IDT)";
                    ordertype = "asc";
                    break;
            }

            // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (Inner join sur TRADEXML)
            string sqlSelect = $@"select (tr.IDT)
                from dbo.TRADE tr
                inner join dbo.TRADEXML trx on (trx.IDT = tr.IDT)
                inner join dbo.INSTRUMENT ns on (ns.IDI = tr.IDI)
                inner join dbo.PRODUCT pr on (pr.IDP = ns.IDP) and ({TradeCommonInputGUI.GetSQLRestrictProduct("pr")})
                where (tr.IDSTENVIRONMENT != @IDSTENVIRONMENT) {addSQLWhere}
                order by tr.IDT {ordertype}";

            sqlSelect = DataHelper.GetSelectTop(SessionTools.CS, sqlSelect, 1);

            DataParameters dp = new DataParameters();
            dp.Add(new DataParameter(SessionTools.CS, "IDSTENVIRONMENT", DbType.String, SQLCst.UT_STATUS_LEN), Cst.StatusEnvironment.TEMPLATE.ToString());
            if (StrFunc.IsFilled(addSQLWhere))
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), pIdT);

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, dp);

            object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            if (obj != null)
                retId = Convert.ToInt32(obj);
            return retId;
        }
        /// <summary>
        /// Retourne le contenu de la toolbar action (Notepad,AttachedDoc,Event,EAr,AccdayBook,Tracker) 
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuAction()
        {
            bool isSessionGuest = SessionTools.User.IsSessionGuest;

            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(isSessionGuest ? 3 : 8);
            
            mnu[0] = GetMenuNotepad();
            mnu[1] = GetMenuAttachedDoc();
            mnu[2] = GetMenuCashFlow();

            if (!isSessionGuest)
            {
                //UserWithLimitedRights 
                mnu[3] = GetMenuEvent();
                mnu[4] = GetMenuEar();
                mnu[5] = GetMenuAccDayBook();
                mnu[6] = GetMenuTrack();
                mnu[7] = GetMenuAuditAction();
            }
            return mnu;
        }

        /// <summary>
        /// Retourne le contenu de la toolbar Consultation (Load,FirstTrade,PreviousTrade,NextTrade,EndTrade)
        /// </summary>
        /// <returns></returns>
        //EG 20120613 BlockUI New
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuConsult()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(5);
            string blockUIMessage = Ressource.GetString("Msg_WaitingRequest");

            mnu[0] = new skmMenu.MenuItemParent(0)
            {
                aID = "btnLoad",
                eImageUrl = "far fa-icon fa-play-circle",
                aToolTip = Ressource.GetString("btnLoad"),
                eCommandName = Cst.Capture.MenuConsultEnum.LoadTrade.ToString()
            };

            mnu[1] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-left",
                aToolTip = Ressource.GetString("imgFirstItem"),
                eCommandName = Cst.Capture.MenuConsultEnum.FirstTrade.ToString()
            };
            mnu[1].eBlockUIMessage = mnu[1].aToolTip + Cst.CrLf + blockUIMessage;

            mnu[2] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-left",
                aToolTip = Ressource.GetString("imgPreviousItem"),
                eCommandName = Cst.Capture.MenuConsultEnum.PreviousTrade.ToString()
            };
            mnu[2].eBlockUIMessage = mnu[2].aToolTip + Cst.CrLf + blockUIMessage;

            mnu[3] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-right",
                aToolTip = Ressource.GetString("imgNextItem"),
                eCommandName = Cst.Capture.MenuConsultEnum.NextTrade.ToString()
            };
            mnu[3].eBlockUIMessage = mnu[3].aToolTip + Cst.CrLf + blockUIMessage;

            mnu[4] = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-angle-double-right",
                aToolTip = Ressource.GetString("imgLastItem"),
                eCommandName = Cst.Capture.MenuConsultEnum.EndTrade.ToString()
            };
            mnu[4].eBlockUIMessage = mnu[4].aToolTip + Cst.CrLf + blockUIMessage;
          
            return mnu;
        }

        /// <summary>
        /// Retourne le contenu de la toolbar Mode (Création, consultation,Duplication,Modification,Action utilisateur, Instrument)
        /// </summary>
        /// <returns></returns>
        /// FI 20170621 [XXXXX] Refactoring
        protected override skmMenu.MenuItemParent GetMenuItemParentMnuMode()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(6);
            mnu[0] = GetMenuCreation();
            mnu[1] = GetMenuConsult();
            mnu[2] = GetMenuDuplicate();
            mnu[3] = GetMenuModify();
            mnu[4] = GetMenuUserAction();
            mnu[5] = GetMenuInstrument();

            // FI 20170621 [XXXXX] Alimentation de MnuModeModify
            MnuModeModify = mnu[3];

            return mnu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        protected virtual skmMenu.MenuItemParent GetMenuInstrument()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aID = "btnInstrument",
                eImageUrl = "fa-icon user-fa-product",
                eText = null,
                aToolTip = Ressource.GetString("Product"),
                eUrl = @"OpenInstrumentSelection('" + TradeCommonInputGUI.GrpElement.ToString() + "'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected override skmMenu.MenuItemParent GetInfoDuplication()
        {
            skmMenu.MenuItemParent mnu = new skmMenu.MenuItemParent(1);
            if (Cst.Capture.IsModeDuplicate(InputGUI.CaptureMode))
            {
                mnu[0].eText = Ressource.GetString("Duplication", true) + " n° " + TradeCommonInput.Identifier;
                mnu[0].eCommandName = TradeCommonInput.Identifier;
            }
            if (Cst.Capture.IsModeReflect(InputGUI.CaptureMode))
            {
                mnu[0].eText = Ressource.GetString("ReflectionOfTrade", true) + " n° " + TradeCommonInput.Identifier;
                mnu[0].eCommandName = TradeCommonInput.Identifier;
            }
            return mnu;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// EG 20120613 BlockUI New
        /// FI 20140708 [20179] Modify
        /// FI 20170116 [21916] Modify
        /// FI 20170620 [XXXXX] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected override skmMenu.MenuItemParent GetMenuModify()
        {
            bool isOk = ((InputGUI.IsModifyAuthorised ||
                            InputGUI.IsModifyPostEvtsAuthorised ||
                            InputGUI.IsModifyFeesUninvoicedAuthorised ||
                            InputGUI.IsModifyMatchingAuthorised)) & (false == TradeCommonInput.IsTradeRemoved);

            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent()
            {
                aID = "btnModification",
                aToolTip = Ressource.GetString("Modification"),
                eImageUrl = "fas fa-icon fa-edit",
                Enabled = isOk
            }; 

            if (isOk)
            {
                string disabledReason = string.Empty;
                bool isEnabled = InputGUI.IsModifyAuthorised;

                if (false == isEnabled)
                    AddActionDisabledReason(ref disabledReason, Ressource.GetString("NotAllowed"));

                if (TradeCommonInput.IsTradeFoundAndAllocation)
                {
                    if (TradeCommonInput.SQLInstrument.IsFungible)
                    {
                        isEnabled = (false == TradeCommonInput.IsAllocationFromPreviousBusinessDate);
                        // FI 20170620 [XXXXX] Utilisation de la ressource Msg_TradeActionDisabledReason_NoClearingDateBusinessDate et de la méthode Ressource.GetString2
                        if (false == isEnabled)
                            AddActionDisabledReason(ref disabledReason,
                                Ressource.GetString2("Msg_TradeActionDisabledReason_NoClearingDateBusinessDate", DtFunc.DateTimeToString(TradeCommonInput.CurrentBusinessDate, DtFunc.FmtShortDate)));
                    }
                    else // cas  Instrument non fongible (Ex commoditySpot) 
                    {
                        // FI 20170116 [21916] pas d'annulation si trade impliqué dans un solde
                        // RD 20201228 [25527] Enlever la restriction Annulation/Modification si trade Commodity Spot impliqué dans un solde
                        //isEnabled = (false == TradeCommonInput.IsExistCashBalance);
                        //if (false == isEnabled)
                        //    AddActionDisabledReason(ref disabledReason, Ressource.GetString("Msg_TradeActionDisabledReason_TradeIsUsedInCashBalance"));
                    }

                    //PL 20200117 [25099] Interdiction de modifier un Trade qui comporte des Frais relatif à un Barème portant sur un Scope "OrderId" ou "FolderId" avec une MIN/MAX
                    if (isEnabled)
                    {
                        isEnabled = (false == TradeCommonInput.IsExistOPP_OnFeeScopeOrderIdOrFolderId_WithMinMax);
                        if (false == isEnabled)
                            AddActionDisabledReason(ref disabledReason, Ressource.GetString("Msg_TradeActionDisabledReason_OPPOnScopeOrderOrFolderWithMinMax"));
                    }
                }

                ret._Childs = 4;
                ret.subItems = new skmMenu.MenuItemParent[4]
                {
                        new skmMenu.MenuItemParent("ModificationGenExpiries", isEnabled,  Cst.Capture.ModeEnum.Update),
                        new skmMenu.MenuItemParent("ModificationNoGenExpiries", InputGUI.IsModifyPostEvtsAuthorised, Cst.Capture.ModeEnum.UpdatePostEvts),
                        new skmMenu.MenuItemParent("ModificationFeesUninvoiced", InputGUI.IsModifyFeesUninvoicedAuthorised, Cst.Capture.ModeEnum.UpdateFeesUninvoiced),
                        new skmMenu.MenuItemParent("ModificationMatching", InputGUI.IsModifyMatchingAuthorised, Cst.Capture.ModeEnum.Match)
                };
                ret.eCommandName = ret.subItems[0].eCommandName;
                ret.Enabled = ret.subItems[0].Enabled;
                if (StrFunc.IsFilled(disabledReason))
                    ret.subItems[0].aToolTip = Ressource.GetString("Msg_TradeActionDisabledReason") + Cst.CrLf + disabledReason;

            }
            else
            {
                ret.aToolTip += Cst.HTMLBreakLine + Ressource.GetString("NotAllowed");
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML)
        protected virtual skmMenu.MenuItemParent GetMenuUserAction()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            //
            //FI 20111124 Inutile de créer tous les menus actions car ils ne seront pas affichés
            //            Ajout test Cst.Capture.IsModeConsult(InputGUI.CaptureMode)
            if (TradeCommonInput.IsTradeFoundWithXML && Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                ret = CreateChildsUserActionMenu(TradeCommonInputGUI.IdMenu);

            ret.aID = "btnUserAction";
            ret.eImageUrl = "fas fa-icon fa-user-cog";
            ret.aToolTip = Ressource.GetString("btnUserAction");
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20140930 [XXXXX] Modify
        /// EG 20190801 Gestion Type Icon si Notepad (vide ou renseigné)
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240123 [WI816] Refactoring
        protected virtual skmMenu.MenuItemParent GetMenuNotepad()
        {
            // PM 20240604 [XXXXX] Ajout paramètre pParentGUID
            string urlNotepad = JavaScript.GetUrlNotepad(Cst.OTCml_TBL.TRADE.ToString(), TradeCommonInput.IdT.ToString(), InputGUI.IdMenu,
                        TradeCommonInput.Identifier, Cst.ConsultationMode.Normal, "0", "Trade", this.GUID);

            bool isNotepadSpecified = false;
            if (false == Cst.Capture.IsModeNewCapture(InputGUI.CaptureMode))
                isNotepadSpecified = IsExistNotePadAttachedDoc(Cst.OTCml_TBL.NOTEPAD.ToString(), Cst.OTCml_TBL.TRADE.ToString(), TradeCommonInput.IdT);

            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                aToolTip = Ressource.GetString("btnNotePad"),
                eImageUrl = "fas fa-icon fa-file-alt" + (isNotepadSpecified ? " green" : ""),
                eUrl = @"OpenNotepad('" + urlNotepad + "','tblMenu_hihTradeIdT','TXTTradeIdentifier'); return false;"
            };

            return ret;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20140930 [XXXXX] Modify
        /// EG 20190801 Gestion type Icon si AttachedDoc (vide ou renseigné)
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240123 [WI816] Refactoring
        protected virtual skmMenu.MenuItemParent GetMenuAttachedDoc()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0);
            //
            //FI 20100331 On stocke dans ATTACHEDDOC.ID l'IDT du trade 
            if (null != TradeCommonInput.SQLTrade)
            {
                //FI 20140930 [XXXXX] Utilisation  de InputGUI.IdMenu
                // PM 20240604 [XXXXX] Ajout paramètre pParentGUID
                string urlAttachedDoc = JavaScript.GetUrlAttachedDoc("ATTACHEDDOC", TradeCommonInput.IdT.ToString(), TradeCommonInput.Identifier,
                                                TradeCommonInput.SQLTrade.Description, InputGUI.IdMenu, "TRADE", this.GUID);

                bool isAttachedDocSpecified = false;
                if (false == Cst.Capture.IsModeNewCapture(InputGUI.CaptureMode))
                    isAttachedDocSpecified = IsExistNotePadAttachedDoc(Cst.OTCml_TBL.ATTACHEDDOC.ToString(), Cst.OTCml_TBL.TRADE.ToString(), TradeCommonInput.IdT);

                ret = new skmMenu.MenuItemParent(0)
                {
                    aToolTip = Ressource.GetString("btnAttachedDoc"),
                    eImageUrl = "fas fa-icon fa-paperclip" + (isAttachedDocSpecified ? " green" : ""),
                    eUrl = @"OpenAttachedDoc('" + urlAttachedDoc + "','tblMenu_hihTradeIdT','TXTTradeIdentifier'); return false;"
                };

            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuEvent()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt",
                eText = "EVENTS",
                eUrl = @"Open" + TradeKey.ToString() + "Events('" + this.GUID + "','Events'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuTrack()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt",
                eText = "AUDIT",
                aToolTip = Ressource.GetString2("Tracks"),
                eUrl = @"OpenTradeTracks('" + GUID + "','Tracks'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuEar()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt",
                eText = "EAR",
                aToolTip = Ressource.GetString2("Ears", TradeCommonInput.Identifier),
                eUrl = @"OpenEars('" + this.GUID + "','Ears'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuAccDayBook()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt",
                eText = "ACC.",
                aToolTip = Ressource.GetString("AccDayBook"),
                eUrl = @"OpenTradeAccDayBook(" + TradeCommonInput.IdT + ",'" + Ressource.GetString("Trade") + ": " + TradeCommonInput.Identifier + "'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuCashFlow()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt",
                eText = "C.FLOWS",
                aToolTip = Ressource.GetString("CashFlows"),
                eUrl = @"OpenTradeCashFlows(" + TradeCommonInput.IdT + ",'" + Ressource.GetString("Trade") + ": " + TradeCommonInput.Identifier + "'); return false;"
            };
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// FI 20161214 [21916] Modify
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected virtual skmMenu.MenuItemParent GetMenuAuditAction()
        {
            skmMenu.MenuItemParent ret = new skmMenu.MenuItemParent(0)
            {
                eImageUrl = "fas fa-icon fa-external-link-alt"
            };
            // FI 20161214 [21916] pas de POSACTION sur les CommoditySpot
            if (TradeCommonInput.IsTradeFoundAndAllocation && (false == TradeCommonInput.Product.ProductBase.IsCommoditySpot))
            {
                ret.eText = "POS.ACT";
                ret.aToolTip = Ressource.GetString("QTip_TradePosaction");
                if (TradeCommonInput.SQLProduct != null)
                {
                    ret.eUrl = "OpenTradePosAction_" + ((Cst.ProductGProduct_FUT == TradeCommonInput.SQLProduct.GProduct) ? "ETD" : "OTC");
                    ret.eUrl += @"(" + TradeCommonInput.IdT + ",'" + Ressource.GetString("Trade") + ": " + TradeCommonInput.Identifier + "'); return false;";
                }
            }
            else
            {
                ret.eText = "TRD.ACT";
                ret.aToolTip = Ressource.GetString("QTip_TradeAuditAction");
                ret.eUrl = @"OpenTradeAuditAction(" + TradeCommonInput.IdT + ",'" + Ressource.GetString("Trade") + ": " + TradeCommonInput.Identifier + "'); return false;";
            }
            return ret;
        }

        /// <summary>
        /// Retourne Le message de confirmation utilisé lorsque l'utilisateur appuie sur le bouton "Record"
        /// </summary>
        /// <returns></returns>
        // FI 20140708 [20179] Modify : Gestion du mode Match
        // EG 20180514 [23812] Report
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected string GetRessourceConfirmTrade()
        {
            string ret = string.Empty;
            string tmpPrefix;
            if (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode))
                tmpPrefix = "Msg_Record";
            else if (Cst.Capture.IsModeRemoveOnlyAll(InputGUI.CaptureMode))
                tmpPrefix = "Msg_Remove";
            else if (Cst.Capture.IsModeRemoveReplace(InputGUI.CaptureMode))
                tmpPrefix = "Msg_RemoveAndRecord";
            else if (Cst.Capture.IsModeUpdate(InputGUI.CaptureMode))
                tmpPrefix = "Msg_Modify";
            else if (Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                tmpPrefix = "Msg_ModifyPostEvts";
            else if (Cst.Capture.IsModeUpdateFeesUninvoiced(InputGUI.CaptureMode))
                tmpPrefix = "Msg_ModifyFeesUninvoiced";
            else if (Cst.Capture.IsModeUpdateAllocatedInvoice(InputGUI.CaptureMode))
                tmpPrefix = "Msg_ModifyAllocatedInvoice";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionCancelation)
                tmpPrefix = "Msg_RecordCorrectionOfQuantity";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer)
                tmpPrefix = "Msg_RecordPositionTransfer";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment)
                tmpPrefix = "Msg_RecordAssignation";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon)
                tmpPrefix = "Msg_RecordAbandon";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise)
                tmpPrefix = "Msg_RecordExercise";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.Correction)
                tmpPrefix = "Msg_RecordCorrection";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.UnderlyingDelivery)
                tmpPrefix = "Msg_RecordUnderlyingDeliveryStep";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.Match)
                tmpPrefix = "Msg_Matching";
            else if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.FxOptionEarlyTermination) // FI 20180320 [23803]
                tmpPrefix = "Msg_RecordEarlyTermination"; 
            else
                tmpPrefix = "Msg_Record";

            string tmpSuffix = (TradeCommonInput.TradeStatus.IsStEnvironment_Template ? "Template" : "Trade");

            ret += Ressource.GetString(tmpPrefix + tmpSuffix);

            //20090416 PL Astuce "temporaire" ...
            if (this.TradeCommonInput.IsTradeFound && TradeCommonInput.Product.ProductBase.IsASSET)
                ret = Ressource.GetTrade(ret);

            return ret;
        }

        /// <summary>
        /// Initialise TradeCommonInputGUI (IDP,IDI,CSS)
        /// </summary>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        protected void InitFromProductInstrument()
        {
            if (TradeCommonInput.IsTradeFound)
            {
                TradeCommonInputGUI.IdP = TradeCommonInput.SQLProduct.Id;
                TradeCommonInputGUI.IdI = TradeCommonInput.SQLInstrument.Id;
                TradeCommonInputGUI.CssColor = AspTools.GetCssColorDefault(TradeCommonInput.SQLInstrument);
            }
        }

        /// <summary>
        /// Retourne l'élément du Datadocument associé au ClientId, cet élément est nécessairement un array
        /// <para>Exemple sur les swap</para>
        /// <para>si clientid="trade1_swap_swapStream" alors la méthode retourne l'élément swapStream rattaché au swap, lui même étant le product du 1er trade existant dans le dataDocument</para>
        /// </summary>
        protected override Array GetArrayElement(string pClientId, out object pParent)
        {
            return TradeCommonInput.CustomCaptureInfos.GetArrayElement(pClientId, out pParent);
        }

        /// <summary>
        /// Retourne l'élément du Datadocument associé au ClientId, cet élément est nécessairement un array
        /// <para>Exemple sur les swap</para>
        /// <para>si clientid="trade1_swap_swapStream" alors la méthode retourne l'élément swapStream rattaché au swap, lui même étant le product du 1er trade existant dans le dataDocument</para>
        /// </summary>
        protected override Array GetArrayElement(string pClientId)
        {
            return GetArrayElement(pClientId, out _);
        }

        /// <summary>
        /// Supprime le dernier item de l'élément Array du Datadocument
        /// <para>Cette suppression n'est effectuée que si l'élément est vide</para>
        /// <para>Retourne true si la suppression s'est opérée</para>
        /// </summary>
        /// <param name="pArray">L'élément du dataDocument</param>
        /// <param name="Parent">L'élémnt du dataDocuement Parent</param>
        /// <returns>true si la suppression s'est opérée</returns>
        protected override bool RemoveLastItemInArrayElement(Array pArray, Object pParent)
        {
            if ((null == pArray) || (null == pParent))
                throw new ArgumentException("Parameters are null");

            int indexLastItem = ArrFunc.Count(pArray) - 1;
            bool ret = indexLastItem > 0;
            if (ret)
            {
                object lastItem = pArray.GetValue(indexLastItem);
                ret = (false == CaptureTools.IsDocumentElementInCapture(lastItem));
            }

            if (ret)
            {
                string element = TradeCommonInput.CustomCaptureInfos.ShiftClientIdToDocumentElement(m_AddRemovePrefix);
                element = element.Substring(element.LastIndexOf("_") + 1);
                //
                //GLOP RemoveItemInArray ne sait pas chercher dans les attributs de serialisation, alors petit codage en dur
                //GLOP petit codage en dur
                if (Tools.IsInterfaceOf(pParent, InterfaceEnum.IFxDigitalOption) && element == "fxEuropeanTrigger")
                    element = "typeTriggerEuropean";
                else if (Tools.IsInterfaceOf(pParent, InterfaceEnum.IFxDigitalOption) && element == "fxAmericanTrigger")
                    element = "typeTriggerAmerican";
                //
                //GLOP sur les strategy il n'est possible de supprimer que des subProduct
                //GLOP petit codage en dur
                if (Tools.IsInterfaceOf(pParent, InterfaceEnum.IStrategy))
                    element = "Item";
                //
                ReflectionTools.RemoveItemInArray(pParent, element, indexLastItem);
            }
            //
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
        ///  EG 20110308 HPC Nb ligne de frais sur facture
        /// EG 20140826 private to protected
        protected override int InitializeNodeObjectArray(CustomObject pCo, XmlNode pNode, string pParentClientId, int pParentOccurs)
        {

            bool isModeConsult = IsControlModeConsult(pCo);
            //
            int docOccurs = TradeCommonInput.CustomCaptureInfos.CciTradeCommon.GetArrayElementDocumentCount(pCo.ClientId, pParentClientId, pParentOccurs);
            int screenOccurs = int.Parse(XMLTools.GetNodeAttribute(pNode, "occurs"));
            // RD 20091021 / Problème sur les Party ou une seule Party est affichée
            int screenMinOccurs = (pCo.ClientId == CciTradeParty.PartyType.party.ToString() ? screenOccurs : 1);
            string minoccurs = XMLTools.GetNodeAttribute(pNode, "minoccurs");
            if (StrFunc.IsFilled(minoccurs))
                screenMinOccurs = int.Parse(minoccurs);
            //
            // EG 20110308 HPC Nb ligne de frais sur facture
            int screenMaxOccurs = 0;
            string maxoccurs = XMLTools.GetNodeAttribute(pNode, "maxoccurs");
            if ("sessiontools" == maxoccurs)
                screenMaxOccurs = NumberRowByPage;
            else if (StrFunc.IsFilled(maxoccurs))
                screenMaxOccurs = int.Parse(maxoccurs);
            //
            int occurs = screenOccurs;
            if (isModeConsult)
            {
                XMLTools.SetNodeAttribute(pNode, "addsuboperator", "false");
                //
                // 20091007 RD / Problème sur les Sales vides affichés
                // En mode consultation et s'il n'y a rien dans le Document alors afficher 1 seul élément vide
                // Sinon Afficher les éléments du document
                occurs = System.Math.Max(docOccurs, screenMinOccurs);
                // EG 20110308 HPC Nb ligne de frais sur facture
                if (0 < screenMaxOccurs)
                    occurs = System.Math.Min(occurs, screenMaxOccurs);
            }
            else if (-1 != docOccurs)
            {
                occurs = System.Math.Max(docOccurs, occurs);
                // EG 20110308 HPC Nb ligne de frais sur facture
                if (0 < screenMaxOccurs)
                    occurs = System.Math.Min(occurs, screenMaxOccurs);
                //
                if ("OnAddOrDeleteItem" == Request.Params["__EVENTTARGET"])
                {
                    #region OnAddOrDeleteItem
                    if ((null != m_AddRemoveNode) && (m_AddRemoveNode.Name == pNode.Name))
                    {
                        string prefix = pParentClientId;
                        //
                        if (pParentOccurs > 0)
                            prefix += pParentOccurs.ToString();
                        //
                        prefix += (StrFunc.IsFilled(prefix) ? CustomObject.KEY_SEPARATOR.ToString() : string.Empty) + pCo.ClientId;
                        //
                        if (m_AddRemovePrefix.EndsWith(prefix))
                        {
                            if (m_AddRemoveOperatorType == Cst.OperatorType.add)
                                occurs++;
                            else if (m_AddRemoveOperatorType == Cst.OperatorType.substract)
                            {
                                if (occurs > 1 && occurs > screenOccurs)
                                {
                                    if (TradeCommonInput.CustomCaptureInfos.CciTradeCommon.RemoveLastEmptyItemInDocumentArray(pCo.ClientId, occurs, pParentClientId, pParentOccurs))
                                        occurs = System.Math.Max(1, occurs - 1);
                                    else
                                        //JavaScript.AlertImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_ItemNotEmpty"), false);
                                        JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_ItemNotEmpty"), false, ProcessStateTools.StatusWarningEnum);
                                }
                                else
                                    //JavaScript.AlertImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_LastItem"), false);
                                    JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_LastItem"), false, ProcessStateTools.StatusWarningEnum);
                            }
                        }
                    }
                    #endregion
                }
            }
            //
            return occurs;

        }

        /// <summary>
        /// Obtient le nombre d'items 
        /// </summary>
        /// <param name="pCo"></param>
        /// <param name="pNode"></param>
        /// <param name="pPrefix"></param>
        /// <returns></returns>
        protected override int InitializeNodeObjectArray2(CustomObject pCo, XmlNode pNode, string pPrefix)
        {

            //
            bool isModeConsult = IsControlModeConsult(pCo);
            //
            int docOccurs = ArrFunc.Count(GetArrayElement(pPrefix));
            int screenOccurs = int.Parse(XMLTools.GetNodeAttribute(pNode, "occurs"));
            //
            // RD 20091021 / Problème sur les Party ou une seule Party est affichée
            int screenMinOccurs = (pCo.ClientId == CciTradeParty.PartyType.party.ToString() ? screenOccurs : 1);
            string minoccurs = XMLTools.GetNodeAttribute(pNode, "minoccurs");
            if (StrFunc.IsFilled(minoccurs))
                screenMinOccurs = int.Parse(minoccurs);

            // Special case we want addsuboperator attribute keep the enabled state
            // EG 20151102 [21465] Add Prefix_denOption
            bool forceEnabledAddSubOperatorAttribute = (
                (InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionExercise ||
                InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAssignment ||
                InputGUI.CaptureMode == Cst.Capture.ModeEnum.OptionAbandon)
                && pPrefix.Contains(TradeCustomCaptureInfos.CCst.Prefix_denOption));

            int ret;
            if (isModeConsult && !forceEnabledAddSubOperatorAttribute)
            {
                XMLTools.SetNodeAttribute(pNode, "addsuboperator", "false");
                //
                // 20091007 RD / Problème sur les Sales vides affichés
                // En mode consultation et s'il n'y a rien dans le Document alors afficher 1 seul élément vide
                // Sinon Afficher les éléments du document
                ret = System.Math.Max(docOccurs, screenMinOccurs);
            }
            else
            {
                ret = System.Math.Max(docOccurs, screenOccurs);
                if ("OnAddOrDeleteItem" == Request.Params["__EVENTTARGET"])
                {
                    if (m_AddRemovePrefix == pPrefix)
                    {
                        if (m_AddRemoveOperatorType == Cst.OperatorType.add)
                        {
                            ret++;
                        }
                        else if (m_AddRemoveOperatorType == Cst.OperatorType.substract)
                        {
                            //if (ret > 1 && ret > screenOccurs)
                            if (ret > 1)
                            {
                                Array array = GetArrayElement(m_AddRemovePrefix, out object parent);
                                if (ArrFunc.IsFilled(array))
                                {
                                    bool isOk = RemoveLastItemInArrayElement(array, parent);
                                    if (isOk)
                                        ret--;
                                    else
                                        JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_ItemNotEmpty"), false, ProcessStateTools.StatusWarningEnum);
                                }
                            }
                            else
                                JavaScript.DialogStartUpImmediate((PageBase)this, Ressource.GetString("Msg_DelItem_LastItem"), false, ProcessStateTools.StatusWarningEnum);
                        }
                    }
                }
            }
            return ret;

        }

        #region private GetSelect
        /// EG 20190801 Gestion si Notepad vide ou renseigné
        protected virtual bool IsExistNotePadAttachedDoc(string pTableSource, string pTableName, int pId)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "TABLENAME", DbType.AnsiString, SQLCst.UT_TABLENAME_LEN), pTableName);
            parameters.Add(new DataParameter(SessionTools.CS, "ID", DbType.Int32), pId);
            string sqlSelect = String.Format(@"select 1 from dbo.{0} np where np.TABLENAME = @TABLENAME and np.ID = @ID", pTableSource);
            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect, parameters);
            object obj = DataHelper.ExecuteScalar(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());
            return (null != obj) && BoolFunc.IsTrue(obj);
        }
        #endregion
        /// <summary>
        /// <para>Vérifie si l'action a un sens ds ce cas pIsHidden = false</para>
        /// <para>Vérifie si l'action est possible pIsEnabled = true</para>
        /// <para>Si l'action est impossible, retourne la raison  </para>
        /// </summary>
        /// <param name="pMenuInput"></param>
        /// <param name="pIsHidden"></param>
        /// <param name="pIsEnabled"></param>
        /// <param name="pDisabledReason"></param> 
        /// FI 20130308[] add parameter pDisabledReason
        protected virtual void IsActionAuthorized(string pMenuInput, out bool pIsHidden, out bool pIsEnabled, out string pDisabledReason)
        {
            pDisabledReason = null;
            pIsHidden = true;
            pIsEnabled = (false == TradeCommonInput.IsTradeRemoved) && TradeCommonInput.TradeStatus.IsCurrentStUsedBy_Regular;
        }


        /// <summary>
        /// Retourne true si le contrôle n'est modifiable lors de l'action PositionTransfer
        /// </summary>
        /// <param name="pClientId"></param>
        /// <returns></returns>
        /// EG 20150624 [21151] Use CciProductExchangeTradedBase instead CciProductExchangeTradedDerivative (EquitySecurityTransaction)
        /// EG 20150624 [21151] Add Test for DebtSecurityTransaction
        /// EG 20171016 [23509] Upd ClearedDate replace BusinessDate
        /// FI 20180301 [XXXXX] Modify         
        protected bool IsControlModeConsultPositionTransferSpecific(string pClientId)
        {
            bool ret = false;
            CciProductBase _cciProductBase = TradeCommonInput.CustomCaptureInfos.CciTradeCommon.cciProduct;
            /// EG 20150624 [21151]
            if (_cciProductBase is CciProductExchangeTradedBase)
            {
                CciProductExchangeTradedBase _cci = _cciProductBase as CciProductExchangeTradedBase;
                if (_cci.CciFixTradeCaptureReport.CciFixInstrument.IsCciOfContainer(pClientId))
                {
                    ret = true;
                }
                else
                {
                    ret = (pClientId == _cci.CciFixTradeCaptureReport.CciClientId(CciFixTradeCaptureReport.CciEnum.LastQty));
                }
            }
            /// EG 20150624 [21151]
            else if (_cciProductBase is CciProductDebtSecurityTransaction)
            {
                CciProductDebtSecurityTransaction _cci = _cciProductBase as CciProductDebtSecurityTransaction;
                if (_cci.CciQuantity.IsCciOfContainer(pClientId) ||
                    (null != _cci.CciSecurityAsset && _cci.CciSecurityAsset.IsCciOfContainer(pClientId)))
                {
                    ret = true;
                }
                else
                {
                    //ret = (pClientId == _cci.CciTradeCommon.cciTradeHeader.CciClientId(CciTradeHeader.CciEnum.businessDate));
                    ret = (pClientId == _cci.CciTradeCommon.cciTradeHeader.CciClientId(CciTradeHeader.CciEnum.clearedDate));
                }
            }
            else if (_cciProductBase is CciProductReturnSwap)
            {
                CciProductReturnSwap _cci = _cciProductBase as CciProductReturnSwap;
                if (0 < _cci.ReturnLegLength)
                {
                    ret = (_cci.CciReturnSwapReturnLeg[0].IsCciOfContainer(pClientId) || _cci.CciReturnSwapInterestLeg[0].IsCciOfContainer(pClientId));
                }
            }
            //FI 20180301 [XXXXX] la plateforme et la date business sont non modifiables
            if (pClientId == TradeCommonInput.CustomCaptureInfos.CciTradeCommon.CciFacilityParty.CciClientId(CciMarketParty.CciEnum.clearedDate) ||
                pClientId == TradeCommonInput.CustomCaptureInfos.CciTradeCommon.CciFacilityParty.CciClientId(CciMarketParty.CciEnum.identifier))
            {
                ret = true;
            }


            return ret;
        }


        /// <summary>
        /// Chargement du trade
        /// </summary>
        /// <param name="pIsLoadTemplate"></param>
        /// EG 20211217 [XXXXX] Possibilité de rechercher un TRADE par son EXTLLINK à l'aide du Préfixe "El "
        /// EG 20240305 [XXXXX] Possibilité de rechercher un TRADE par son DISPLAYNAME à l'aide du Préfixe "Dn "
        protected void LoadCapture(bool pIsLoadTemplate)
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.LoadCapture");
            SQL_TableWithID.IDType idType = SQL_TableWithID.IDType.Id;
            // EG 20100314 Change isSearchByIdentifier valuation
            //bool isSearchByIdentifier = (TradeIdT == TradeCommonInput.IdT);
            //isSearchByIdentifier |= StrFunc.IsFilled(TradeIdentifier) && (-1 == TradeIdT);
            bool isSearchByIdentifier = Cst.Capture.IsModeNewOrDuplicateOrReflect(TradeCommonInputGUI.CaptureMode) || pIsLoadTemplate;
            if (!isSearchByIdentifier)
            {
                isSearchByIdentifier = (TradeIdT == TradeCommonInput.IdT);
                isSearchByIdentifier |= StrFunc.IsFilled(TradeIdentifier) && (-1 == TradeIdT);
            }
            //PL 20100921 Tip
            if (isSearchByIdentifier && TradeIdentifier.StartsWith("Id ") && IntFunc.IsInt64(TradeIdentifier.Substring(3)))
            {
                isSearchByIdentifier = false;
                TradeIdT = IntFunc.IntValue(TradeIdentifier.Substring(3));
            }

            string identifier;
            if (isSearchByIdentifier)
            {
                idType = SQL_TableWithID.IDType.Identifier;
                if (pIsLoadTemplate)
                {
                    idType = SQL_TableWithID.IDType.Identifier;
                    identifier = TradeCommonInputGUI.TemplateIdentifier;
                }
                else if (TradeIdentifier.StartsWith("El "))
                {
                    idType = SQL_TableWithID.IDType.ExtLink;
                    identifier = TradeIdentifier.Substring(3);
                }
                else if (TradeIdentifier.StartsWith("Dn "))
                {
                    idType = SQL_TableWithID.IDType.Displayname;
                    identifier = TradeIdentifier.Substring(3);
                }
                else
                {
                    identifier = TradeIdentifier;
                    if (StrFunc.IsEmpty(identifier) &&
                        (false == TradeCommonInput.TradeStatus.IsStEnvironment_Template) &&
                        (TradeCommonInputGUI.TemplateIdentifier != TradeCommonInput.Identifier))
                        identifier = TradeCommonInput.Identifier;
                }
            }
            else
            {
                int id;
                if (pIsLoadTemplate)
                {
                    id = TradeCommonInputGUI.TemplateIdT;
                }
                else
                {
                    id = TradeIdT;
                    if ((-1 == id) && (false == TradeCommonInput.TradeStatus.IsStEnvironment_Template) &&
                       (TradeCommonInputGUI.TemplateIdentifier != TradeCommonInput.Identifier))
                        id = TradeCommonInput.IdT;
                }
                identifier = id.ToString();
            }
            isRefreshHeader = true;
            m_FullCtor = null;

            //FI 20091130 [16769] add isSetNewCustumCapturesInfos
            //Lorsque le placeHolder est vide => nouvelle instance des ccis
            //car la collection se charge avec le chargement de l'écran (voir methode LoadCustomizeCapture) 
            bool isSetNewCustomCapturesInfos = (false == IsPlaceHolderLoaded);
            //            
            //FI 20120709 [17892] Mise en cache du Load si chargement d'un template
            string csLoad = SessionTools.CS;
            if (pIsLoadTemplate)
                csLoad = CSTools.SetCacheOn(SessionTools.CS);
            //
            TradeCommonCaptureGen.Load(csLoad,null, identifier, idType, InputGUI.CaptureMode, SessionTools.User, SessionTools.SessionID, isSetNewCustomCapturesInfos);
            TradeCommonCaptureGen.TradeCommonInput.CustomCaptureInfos.FmtETDMaturityInput = SessionTools.ETDMaturityFormat;
            //
            if (TradeCommonInput.IsTradeFound)
            {
                InitFromProductInstrument();
                TradeIdT = TradeCommonInput.IdT;
                TradeIdentifier = TradeCommonInput.Identifier;
                if (false == pIsLoadTemplate)
                {
                    TradeCommonInputGUI.TemplateIdentifier = TradeCommonInput.GetTemplateIdentifier(CSTools.SetCacheOn(SessionTools.CS)); // Recupère le Template du Trade consulter
                    TradeCommonInputGUI.CurrentIdScreen = TradeCommonInput.SQLLastTradeLog.ScreenName;
                }
                TradeCommonInputGUI.ActorRole = TradeCommonInput.DataDocument.GetActorRole(CSTools.SetCacheOn(SessionTools.CS));
            }
            else
            {
                // RD 20110530 [17400]
                // Pour éviter d'afficher un ancien Trade déjà trouvé
                TradeIdT = -1;
            }
            AddAuditTimeStep("End TradeCommonCapturePage.LoadCapture");
        }

        /// <summary>
        /// Chargement de l'écran light depuis le descriptif XML lorsque le placeHolder est vide
        /// Chargement de la collection customCaptureInfos avec les infos du dataDocument, et affichage sur la page (Dump_TOGUI) 
        /// </summary>
        /// <returns></returns>
        ///FI 20091130 [16769] Refactoring 
        private bool DisplayCustomizeCapture()
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.DisplayCustomizeCapture");

            bool isOk = true;

            if (false == IsPlaceHolderLoaded)
                isOk = LoadScreen();

            if (false == isOk)
                InputGUI.CurrentIdScreen = string.Empty;

            if (isOk)
                TradeCommonInput.CustomCaptureInfos.LoadDocument(InputGUI.CaptureMode, (CciPageBase)this);

            AddAuditTimeStep("End TradeCommonCapturePage.DisplayCustomizeCapture");

            return isOk;
        }

        /// <summary>
        ///  Création d'un PlaceHolder dont le contenu est alimenté par appel à la saisie full
        /// </summary>
        /// <returns></returns>
        private PlaceHolder DisplayFullCapture()
        {
            PlaceHolder ret = new PlaceHolder();
            //Le Trade existe. L'écran existe
            try
            {
                m_FullCtor = new FullConstructor(TradeCommonInput.FpMLDataDocReader.Version);
                m_FullCtor.Start(ref ret, TradeCommonInput.FpMLDocReader);
            }
            catch (Exception ex) { ret.Controls.AddAt(0, new LiteralControl(System.Environment.NewLine + ex.Message.ToString())); }
            //
            return ret;
        }

        /// <summary>
        /// Ajoute des messages de confirmation sur les boutons enregistrer (btnRecord) et annuler (btnCancel)
        /// </summary>
        /// FI 20140708 [20179] Gestion IsModeMatch
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20210407 [25556] Message de confirmation (Record/Annul) avec Dialog JQuery (à la place de window.confirm)
        protected override void RefreshButtonsValidate()
        {
            // 20081117 RD Refresh Footer buttons  Ticket 16407              
            for (int i = 0; i <= 1; i++)
            {
                #region Record Button
                Control ctrlContainer = FindControl("tblMenu");
                if (null != ctrlContainer)
                {
                    if (ctrlContainer.FindControl("btnRecord") is WebControl ctrl)
                    {
                        ctrl.Attributes["href"] = "#";
                        if (Cst.Capture.IsModeInput(InputGUI.CaptureMode) || Cst.Capture.IsModeMatch(InputGUI.CaptureMode))
                        {
                            string msgConfirm = GetRessourceConfirmTrade();
                            string data = (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode) ? string.Empty : TradeIdentifier);
                            //alimentation attribute onclick

                            StringBuilder sb = new StringBuilder();
                            if (Cst.Capture.IsModeInput(InputGUI.CaptureMode))
                            {
                                // FI 20200128 [25182] Call SaveInputTrade
                                Boolean enableValidator = (false == TradeCommonCaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode));
                                // FI 20210621 [XXXXX] appel de nouveau à SaveInputTrade puisque cette méthode alimente __idLastNoAutoPostbackCtrlChanged
                                sb.AppendFormat("SaveInputTrade('{0}','{1}',{2},'{3}','{4}');", this.TitleLeft, "tblMenu$btnRecord", JavaScript.HTMLString(msgConfirm), data, enableValidator);
                            }
                            else if (Cst.Capture.IsModeMatch(InputGUI.CaptureMode))//FI 20140708 [20179]
                            {
                                sb.AppendFormat("ConfirmInputTrade('{0}','{1}',{2},'{3}');", this.TitleLeft, "tblMenu$btnRecord", JavaScript.HTMLString(msgConfirm), data);
                            }
                            else
                            {
                                throw new NotImplementedException(StrFunc.AppendFormat("Mode (id:{0}) is not implemented", InputGUI.CaptureMode.ToString()));
                            }

                            ctrl.Attributes.Add("onclick", sb.ToString());
                        }
                        else
                            ctrl.Attributes.Remove("onclick");
                    }
                    #endregion Record Button
                    //
                    #region Cancel Button
                    ctrl = ctrlContainer.FindControl("btnCancel") as WebControl;
                    if (null != ctrl)
                    {
                        ctrl.Attributes["href"] = "#";
                        string data = string.Empty;
                        string msgConfirm = Ressource.GetStringForJS("Msg_AbortData");
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("DisableValidators();ConfirmInputTrade('{0}','{1}',{2},'{3}');", this.TitleLeft, "tblMenu$btnCancel", JavaScript.HTMLString(msgConfirm), data);
                        ctrl.Attributes.Add("onclick", sb.ToString());
                    }
                    #endregion Cancel Button

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// FI 20160816 [22146] Modify
        /// FI 20170621 [XXXXX] Refactoring complet  
        /// EG 20210622 [XXXXX] Appel à nouvelle fonction ConfirmInputTradeWithArgs (pour entrer en modification avec un éventiel message informatif demandant confirmation)
        /// EG 20231006 [XXXXX] Trade input - Statut : Valeur du bouton "Annul" sur le message informatif affiché à l'entrée en mode Modification (FALSE -> Cst.Capture.ModeEnum.Consult)
        protected override void RefreshMnuMode()
        {
            base.RefreshMnuMode();

            if (null == MnuModeModify)
                throw new NullReferenceException("MenuModify is null");

            // FI 20170621 [XXXXX] Mise en place d'un message uniquement si le menu est Enabled
            // => signifie que l'utilisateur a les droits, et que l'action Modification est possible sur le trade  
            if (Cst.Capture.IsModeConsult(this.InputGUI.CaptureMode) && (TradeCommonInput.IdT>0) && MnuModeModify.Enabled)
            {
                TableCell[] cell = GetCellModification();
                if (ArrFunc.IsFilled(cell))
                {
                    // FI 20170620 [22146] Génération du message msgConfirm uniquement si InputGUI.IsModifyAuthorised
                    string msgConfirm = string.Empty;

                    if (TradeCommonInput.IsTradeRemoved)
                    {
                        msgConfirm = Ressource.GetString2("Msg_TradeWithEnvironmentDeactiv", Ressource.GetString("REMOVED"));
                    }
                    else
                    {
                        // FI 20160816 [22146] Message spécifique s'il existe des évènements de clôtures impliqués dans un EAR sur les trades autres que celui qui est modifié
                        Boolean isOFSEventVsIdT_WithEAR = false;
                        DataRow[] row = null;
                        if (TradeCommonInput.IsAllocation)
                        {
                            row = TradeRDBMSTools.GetOFSEventVsIdT_WithEAR(SessionTools.CS, null, TradeCommonInput.IdT);
                            isOFSEventVsIdT_WithEAR = ArrFunc.IsFilled(row);
                        }

                        if (TradeCommonInput.ExistEventProcessInSucces || isOFSEventVsIdT_WithEAR)
                        {

                            string sProcessInSucces = string.Empty;
                            string sTradeVs = string.Empty;

                            if (TradeCommonInput.ExistEventProcessInSucces)
                            {
                                Cst.ProcessTypeEnum[] eventProcessInSucces = TradeCommonInput.GetEventProcessInSucces();
                                int nbProcess = ArrFunc.Count(eventProcessInSucces);
                                string[] res = new string[nbProcess];
                                for (int j = 0; j < nbProcess; j++)
                                    res[j] = Ressource.GetString(eventProcessInSucces[j].ToString());
                                sProcessInSucces = ArrFunc.GetStringList(res);
                            }
                            if (isOFSEventVsIdT_WithEAR)
                            {
                                string[] identier = (from DataRow item in row
                                                     select new
                                                     {
                                                         identier = Convert.ToString(item["IDENTIFIER"])
                                                     }).Select(x => x.identier).Distinct().ToArray();
                                sTradeVs = ArrFunc.GetStringList(identier);
                            }

                            msgConfirm = string.Empty;
                            if (TradeCommonInput.ExistEventProcessInSucces && isOFSEventVsIdT_WithEAR)
                            {
                                msgConfirm = Ressource.GetString2("Msg_TradeWithProcessOkAndWithOFS", sProcessInSucces, sTradeVs);
                            }
                            else if (TradeCommonInput.ExistEventProcessInSucces)
                            {
                                msgConfirm = Ressource.GetString2("Msg_TradeWithProcessOk", sProcessInSucces);
                            }
                            else if (isOFSEventVsIdT_WithEAR)
                            {
                                msgConfirm = Ressource.GetString2("Msg_TradeWithOFS", sTradeVs);
                            }
                        }
                    }
                    // EG 20210623 Appel à nouvelle fonction ConfirmInputTradeWithArgs (Identique à ConfirmInputTrade)
                    // mais les arguments associés aux boutons OK/CANCEL de la fenêtre de validation sont passés en paramètre :
                    // - OK = Cst.Capture.ModeEnum.Update.ToString()
                    // - CANCEL = FALSE
                    // ces paramètres sont passés dans le PostBack du control {pControl} et interprétés par la méthode ValidateCapture
                    if (StrFunc.IsFilled(msgConfirm))
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("ConfirmInputTradeWithArgs('{0}','{1}',{2},'{3}','{4}','{5}'); return false;", this.TitleLeft, "tblMenu$mnuMode", 
                            JavaScript.HTMLString(msgConfirm), TradeIdentifier, Cst.Capture.ModeEnum.Update.ToString(),
                            Cst.Capture.ModeEnum.Consult.ToString());


                        for (int i = 0; i < ArrFunc.Count(cell); i++)
                            cell[i].Attributes.Add("onclick", sb.ToString());
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="paMnu"></param>
        /// <param name="phashEvents"></param>
        protected virtual void SetMnuItemResultAction(ArrayList paMnu, Hashtable phashEvents)
        {
        }

        /// <summary>
        /// Sauvegarde du trade, postage d'un message pour la génération des évènements et affichage d'une msgbox. 
        /// <para>NB: TradeCommonCaptureGen est rechargé dans la foulée.</para>
        /// </summary>
        /// <returns></returns>
        // FI 20170404 [23039] Modify 
        // EG 20171113 Upd SetCookieDefaultFacilityAndMarket
        // EG 20180423 Analyse du code Correction [CA2200]
        // EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        //      => plus de recordSettings.isUpdateOnly_TradeStream
        // EG 20200519 [XXXXX] Add recordMode setting for enhancement performance
        // EG 20200914 [XXXXX] Upd recordSettings.isCheckValidationXSD = (false == TradeCommonInput.SQLProduct.IsAdministrativeProduct);
        // EG 20211029 [25696] Transfert de position :  Ajout préfixe [POT] sur EXTLLINK
        protected TradeCommonCaptureGen.ErrorLevel RecordCapture()
        {
            AddAuditTimeStep("Start TradeCommonCapturePage.RecordCapture");
            
            TradeCommonCaptureGen.ErrorLevel lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            string newIdentifier = string.Empty;
            string displayName = string.Empty;
            string description = string.Empty;
            string extlLink = string.Empty;
            
            int idT = 0;

            // FI 20170404 [23039] Utilisation de underlying et trader
            Pair<int,string>[] underlying = null;
            Pair<int,string>[] trader = null;
            
            
            string msg = string.Empty;
            string msgDetail = string.Empty;
            TradeCommonCaptureGenException errExc = null;
            
            #region Init from controls
            //FI 20101210 Ajout TRIM sur newIdentifier, displayName et description
            //ces informations sont réutilisées lors annulation avec remplaçante
            //Pour être en phase avec la saisie light qui effectue des trim systématiques
            //Cela évite que les frais soient effacés
            //
            //Identifier
            newIdentifier = TxtIdentifier.Trim();
            //FI DisplayName
            displayName = TxtDisplayName.Trim();
            //FI Description
            description = TxtDescription.Trim();
            // ExtlLink
            extlLink = TxtExtLink;
            #endregion

            try
            {
                AddAuditTimeStep("Start CheckAndRecord");

                CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo()
                {
                    user = SessionTools.User,
                    session = SessionTools.AppSession,
                    licence = SessionTools.License
                };

                TradeRecordSettings recordSettings = new TradeRecordSettings
                {
                    displayName = displayName,
                    description = description,
                    idScreen = InputGUI.CurrentIdScreen,
                    isCheckValidationRules = true,
                    isCheckValidationXSD = (false == TradeCommonInput.SQLProduct.IsAdministrativeProduct), // true;
                    isCheckLicense = true,
                    isCheckActionTuning = true,
                    //Calcul automatique de l'identifier lorsque l'instrument est autre que asset (ex Reférenteil Titre)
                    //et si Statut autre que template
                    isGetNewIdForIdentifier = (false == TradeCommonInput.SQLProduct.IsAssetProduct) &&
                    (false == TradeCommonInput.TradeStatus.IsStEnvironment_Template),

                    // EG 20200519 [XXXXX] New
                    recordMode = (int)SystemSettings.GetAppSettings("Trade_recordMode", typeof(int), 0)
                };

                // RD 20220617 [26078] Add test on PositionTransfer
                if (InputGUI.CaptureMode == Cst.Capture.ModeEnum.PositionTransfer && StrFunc.IsFilled(extlLink))
                {
                    // EG [25696] Ajout d'un préfixe [POT] sur EXTLLINK
                    string prefix = "[" + ReflectionTools.ConvertEnumToString<Cst.PosRequestTypeEnum>(Cst.PosRequestTypeEnum.PositionTransfer) + "]";
                    extlLink = prefix + extlLink.Replace(prefix, string.Empty);
                }
                recordSettings.extLink = extlLink;

                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //PL 20191210 [25099] In progress... GLOPXXX
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-
                //if (TradeCommonInput.DataDocument.currentProduct.isExchangeTradedDerivative)
                //{
                //    recordSettings.isExistsFeeScope_OrderId = false; 
                //    recordSettings.OrderId = TradeCommonInput.DataDocument.GetOrderId();
                //    if ((!string.IsNullOrWhiteSpace(recordSettings.OrderId)) && Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode))
                //    {
                //        //Tester s'il existe un "feeSchedScope égale à OrderId
                //        //                et un "feeSchedFormulaMin différent de N/A ou un "feeSchedFormulaMax différent de N/A
                //        if (TradeCommonInput.DataDocument.otherPartyPaymentSpecified)
                //        {
                //            IPayment[] otherPartyPayment = TradeCommonInput.DataDocument.otherPartyPayment;
                //            foreach (IPayment payment in otherPartyPayment)
                //            {
                //                if (StrFunc.IsFilled(payment.payerPartyReference.hRef) && (Tools.IsPaymentSourceScheme(payment, Cst.OTCml_RepositoryFeeScheduleScheme)))
                //                {
                //                    bool isExistsOrderId = payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedScopeScheme).Value == Cst.FeeScopeEnum.OrderId.ToString();
                //                    bool isExistsMinValue = payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaMinScheme).Value != Cst.NotAvailable;
                //                    bool isExistsMaxValue = payment.paymentSource.GetSpheresIdFromScheme(Cst.OTCml_RepositoryFeeSchedFormulaMaxScheme).Value != Cst.NotAvailable;
                //                    if (isExistsOrderId && (isExistsMinValue || isExistsMaxValue))
                //                    {
                //                        recordSettings.isExistsFeeScope_OrderId = true;
                //                        break;
                //                    }
                //                }
                //            }
                //        }
                //    }
                //}
                //+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-

                // FI 20170404 [23039] Utilisation de underlying et trader
                TradeCommonCaptureGen.CheckAndRecord(SessionTools.CS, null, InputGUI.IdMenu, InputGUI.CaptureMode, captureSessionInfo, recordSettings,
                                                    ref newIdentifier, ref idT, out underlying, out trader);
            }
            catch (TradeCommonCaptureGenException ex)
            {
                //Erreur reconnue
                errExc = ex;
                lRet = errExc.ErrLevel;
            }
            catch (Exception) { throw; }//Error non gérée
            finally
            {
                AddAuditTimeStep("End CheckAndRecord");
            }
            
            //constitution et affichage du message récapitulatif
            // FI 20170404 [23039] Utilisation de underlying et trader
            msg = TradeCommonCaptureGen.GetResultMsgAfterCheckAndRecord(SessionTools.CS, errExc, InputGUI.CaptureMode, newIdentifier, underlying, trader, true, out msgDetail);
            
            if ((null != errExc) && (null != errExc.InnerException))
            {
                string msgException = StrFunc.AppendFormat("CheckAndRecord[ErrLevel:{0}] Trade[Identifier:{1}] Detail[{2}]", errExc.ErrLevel.ToString(), newIdentifier, msgDetail);
                WriteLogException(new SpheresException2("TradeCommonCapturePage.RecordTrade", msgException, errExc.InnerException));
            }

            
            //20090429 FI Si mode full affichage systématique du trade non conforme
            //En effet CheckAndRecord initialise certaines données (exemple TradeSide) qui ne seront pas visible sur l'écran 
            if (null != errExc)
            {
                if ((errExc.ErrLevel == TradeCommonCaptureGen.ErrorLevel.XMLDOCUMENT_NOTCONFORM) && IsScreenFullCapture)
                    DisplayTradeXml(true, false);
            }
            
            //Mise à jour de lret à SUCCESS lorsque le trade est correctement sauvegardé en base
            if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                lRet = TradeCommonCaptureGen.ErrorLevel.SUCCESS;
            
            if (TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                SetCookieDefaultFacilityAndMarket();

            if (TradeCommonCaptureGen.ErrorLevel.SUCCESS == lRet)
            {
                #region Reload captureGen  => Reload de l'instance captureGen pour eviter tout déphasage avec la database
                AddAuditTimeStep("Start Load Trade");
                isRefreshHeader = true;
                TradeCommonHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();
                //TradeCommonCaptureGen.Load(newIdentifier, InputGUI.CaptureMode, SessionTools.SessionID, SessionTools.IsSessionSysAdmin,false);
                //FI 20110308 Il faut passer mode consult pour ne pas réinitialiser la classe de travail m_RemoveTrade
                //FI 20110123 Utilisation d'un user admin => Un utilisateur peut sauvegarder un trade qu'il ne pourra pas consulter
                //Ex un utlisateur peut saisir un trade dont l'entité n'est pas celle qui lui est rattachée
                User userAdmin = new User(1, null, RoleActor.SYSADMIN);
                TradeCommonCaptureGen.Load(SessionTools.CS, null, idT, Cst.Capture.ModeEnum.Consult, userAdmin, SessionTools.SessionID, false);
                AddAuditTimeStep("End Load Trade");
                #endregion
                //
                // FI 20131219 [19374] Mise en place d'un try catch
                try
                {
                    // FI 20170404 [23039] Aliemntation de idUnderlying à partir de underlying
                    int[] idUnderlying = null;
                    if (ArrFunc.IsFilled(underlying))
                        idUnderlying = (from item in underlying select item.First).ToArray();  
                    
                    SendRecordCapture(null, idUnderlying);
                }
                catch (Exception ex)
                {
                    lRet = TradeCommonCaptureGen.ErrorLevel.AFTER_RECORD_ERROR;
                    msgDetail += Cst.CrLf2;
                    msgDetail += Ressource.GetString("Msg_TradeSendMQueueNok") + Cst.CrLf + ex.Message;
                }
            }
            //
            AddAuditTimeStep("End TradeCommonCapturePage.RecordCapture");
            //
            DisplayMessageAfterRecord(lRet, msg, msgDetail);

            //FI 20091130 [16769]
            isRecordOk = (lRet == TradeCommonCaptureGen.ErrorLevel.SUCCESS);

            return lRet;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void SetInputSession()
        {
        }

        /// <summary>
        /// Poste un message pour générer les évènements ou Annuler un trade (avec ou sans remplaçante) 
        /// <para>Doit être utilisé lorsque l'enregistrement du trade est ok</para>
        /// </summary>
        /// <param name="pDbTransaction"></param>
        /// <param name="pIdUnderlying"></param>
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected virtual void SendRecordCapture(IDbTransaction pDbTransaction, int[] pIdUnderlying )
        {
            AddAuditTimeStep("Start SendRecordCapture");
            
            int idxMQueue = 0;
            MQueueBase[] mQueue = null;

            if (Cst.Capture.IsModeUpdateFeesUninvoiced(InputGUI.CaptureMode))
            {
                mQueue = new MQueueBase[] { GetMQueue() };
            }
            else
            {
                bool isSendEventMsg = (!TradeCommonInput.TradeStatus.IsStActivation_Missing);
                isSendEventMsg &= (!TradeCommonInput.TradeStatus.IsStEnvironment_Template);
                isSendEventMsg &= (!Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode));
                // RD 20091231 [16814] Modification of Trade included in Invoice / ne pas poster de message
                isSendEventMsg &= (false == (Cst.Capture.IsModeUpdate(InputGUI.CaptureMode) && TradeCommonInput.IsTradeInInvoice()));

                if (isSendEventMsg)
                {
                    //Send Mqueue for Generate Event on Underlying created and Trade
                    //FI 20100331  add restriction sur isDebtSecurityTransaction, sur ce produit les assets sont des trades 
                    if (ArrFunc.IsFilled(pIdUnderlying) && (this.TradeCommonInput.Product.IsDebtSecurityTransaction || this.TradeCommonInput.Product.IsDebtSecurityOption))
                    {
                        MQueueBase[] mQueueUnderlying = (MQueueBase[])CaptureTools.GetMQueueForEventProcess(SessionTools.CS, pIdUnderlying, false, null);
                        mQueue = new MQueueBase[ArrFunc.Count(mQueueUnderlying) + 1];
                        Array.Copy(mQueueUnderlying, mQueue, ArrFunc.Count(mQueueUnderlying));
                        idxMQueue = ArrFunc.Count(mQueue) - 1;
                        mQueue[idxMQueue] = GetMQueue();
                    }
                    else
                    {
                        //Send Mqueue for Trade
                        mQueue = new MQueueBase[] { GetMQueue() };
                    }
                }
            }

            if (ArrFunc.IsFilled(mQueue))
                SendMQueue(mQueue, idxMQueue);

            AddAuditTimeStep("End SendRecordCapture");
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pMnu"></param>
        /// <param name="pRow"></param>
        /// <returns></returns>
        public virtual ArrayList SetMenuTradeAction(skmMenu.MenuItemParent pMnu, DataRow pRow)
        {
            ArrayList rowToolTip = new ArrayList();
            TableRow tr;
            TableCell td;
            if (false == Convert.IsDBNull(pRow["VALORISATION"]))
            {
                Hashtable currencyInfos = new Hashtable();

                decimal val = DecFunc.DecValue(pRow["VALORISATION"].ToString(), Thread.CurrentThread.CurrentCulture);
                string amount = StrFunc.FmtDecimalToCurrentCulture(val.ToString(NumberFormatInfo.InvariantInfo));
                string currency = pRow["UNIT"].ToString();
                if (StrFunc.IsFilled(currency))
                {
                    CurrencyCashInfo currencyInfo;
                    if (false == currencyInfos.ContainsKey(currency))
                    {
                        currencyInfo = new CurrencyCashInfo(SessionTools.CS, currency);
                        if (null != currencyInfo)
                            currencyInfos.Add(currency, currencyInfo);
                    }
                    if (currencyInfos.ContainsKey(currency))
                    {
                        currencyInfo = (CurrencyCashInfo)currencyInfos[currency];
                        amount = StrFunc.FmtDecimalToCurrentCulture(val, currencyInfo.RoundPrec);
                    }
                }
                tr = new TableRow
                {
                    CssClass = "DataGrid_AlternatingItemStyle"
                };
                td = new TableCell
                {
                    Text = pRow["TYPEVALUE"].ToString()
                };
                tr.Cells.Add(td);
                rowToolTip.Add(tr);
                tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                td = new TableCell
                {
                    Text = currency + " " + amount
                };
                tr.Cells.Add(td);
                rowToolTip.Add(tr);
            }
            if (false == Convert.IsDBNull(pRow["NOTE"]))
            {
                tr = new TableRow
                {
                    CssClass = "DataGrid_PagerStyle"
                };
                td = new TableCell
                {
                    Text = "Comments"
                };
                tr.Cells.Add(td);
                rowToolTip.Add(tr);
                tr = new TableRow
                {
                    CssClass = "DataGrid_ItemStyle"
                };
                td = new TableCell
                {
                    Text = pRow["NOTE"].ToString()
                };
                tr.Cells.Add(td);
                rowToolTip.Add(tr);
            }
            return rowToolTip;
        }

        /// <summary>
        /// Envoi des messages multiples 
        /// <para>Alimente le tracker et log si une exception se produit</para>
        /// </summary>
        /// <param name="pQueue"></param>
        /// 20090701 le paramètre pQueue est maintenant un array
        /// => car la création de titre depuis la saisie implique l'envoi de plusieurs messages
        protected void SendMQueue( MQueueBase[] pQueue)
        {
            SendMQueue( pQueue, 0);
        }
        /// <summary>
        /// Envoi des messages multiples 
        /// <para>Alimente le tracker et log si une exception se produit</para>
        /// </summary>
        /// <param name="pQueue"></param>
        /// <param name="pIndexQueueForTracker"></param>
        /// 20090701 le paramètre pQueue est maintenant un array
        /// => car la création de titre depuis la saisie implique l'envoi de plusieurs messages
        /// FI 20131219 [19374] suppression du try catch
        protected void SendMQueue(MQueueBase[] pQueue, int pIndexQueueForTracker)
        {
            if (null != pQueue)
            {
                MQueueTaskInfo taskInfo = new MQueueTaskInfo
                {
                    connectionString = SessionTools.CS,
                    process = pQueue[pIndexQueueForTracker].ProcessType,
                    Session = SessionTools.AppSession,
                    mQueue = pQueue,
                    trackerAttrib = new TrackerAttributes()
                    {
                        process = pQueue[pIndexQueueForTracker].ProcessType,
                        gProduct = TradeCommonInput.SQLProduct.GProduct,
                        caller = InputGUI.CaptureMode.ToString(),
                        info = GetMQueueIdData(pQueue[pIndexQueueForTracker])
                    }
                };
                taskInfo.SetTrackerAckWebSessionSchedule(ArrFunc.Count(taskInfo.mQueue) == 1 ? taskInfo.mQueue[0].idInfo : null);

                // FI 20131219 [19374] isExceptionOnTrackerMode = true;
                taskInfo.isExceptionOnTrackerMode = true;
                int idTRK_L = 0;
                MQueueTaskInfo.SendMultiple(taskInfo, ref idTRK_L);
            }
        }

        /// <summary>
        /// Generation de MQueueBase à envoyer pour le trade courant (fonction du traitement Remove ou création,Modification)
        /// </summary>
        /// <returns></returns>
        /// EG 20201005 [XXXXX] Correction alimentation du paramètre EventsGenMQueue.PARAM_DELEVENTS
        // EG 20240123 [WI816] Trade input: Modification of periodic fees uninvoiced on a trade
        protected virtual MQueueBase GetMQueue()
        {
            MQueueAttributes mQueueAttributes = new MQueueAttributes
            {
                connectionString = SessionTools.CS,
                idInfo = GetMQueueIdInfo()
            };

            MQueueBase mQueue = null;
            if (Cst.Capture.IsModeRemove(InputGUI.CaptureMode))
            {
                if (Cst.Capture.IsModeRemoveReplace(InputGUI.CaptureMode))
                {
                    TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTReplace = TradeCommonInput.IdT;
                    TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTReplaceSpecified = true;
                    TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTReplaceIdentifier = TradeCommonInput.Identifier;
                    TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTReplaceIdentifierSpecified = true;
                    TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.isEventsReplace = TradeCommonInput.IsInstrumentEvents();
                }
                mQueueAttributes.id = TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTCancel;
                mQueueAttributes.identifier = TradeCommonCaptureGen.TradeCommonInput.RemoveTrade.idTCancelIdentifier;
                mQueue = new TradeActionGenMQueue(mQueueAttributes, TradeCommonCaptureGen.TradeCommonInput.RemoveTrade);
            }
            else if (Cst.Capture.IsModeUpdateFeesUninvoiced(InputGUI.CaptureMode))
            {
                mQueueAttributes.id = TradeCommonCaptureGen.TradeCommonInput.IdT;
                mQueueAttributes.identifier = TradeCommonCaptureGen.TradeCommonInput.Identifier;
                mQueue = new TradeActionGenMQueue(mQueueAttributes);

                ((TradeActionGenMQueue)mQueue).item = new TradeActionBaseMQueue[]
                {
                    new TradeActionMQueue{tradeActionCode = TradeActionCode.TradeActionCodeEnum.FeesEventGenUninvoiced}
                };
            }
            else if (TradeCommonInput.IsInstrumentEvents())
            {
                mQueueAttributes.id = TradeCommonCaptureGen.TradeCommonInput.IdT;
                mQueueAttributes.identifier = TradeCommonCaptureGen.TradeCommonInput.Identifier;
                mQueue = new EventsGenMQueue(mQueueAttributes);
                mQueue.parameters[EventsGenMQueue.PARAM_DELEVENTS].SetValue(Cst.Capture.IsModeUpdate(InputGUI.CaptureMode));
            }
            return mQueue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IdInfo GetMQueueIdInfo()
        {
            IdInfo idInfo = new IdInfo()
            {
                id = TradeCommonInput.IdT,
                idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", TradeCommonInput.SQLProduct.GProduct) }
            };
            return idInfo;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected IdInfo[] GetMQueueIdData(MQueueBase[] pQueue)
        {
            ArrayList aIdData = new ArrayList();
            string idDataIdent = (TradeCommonInput.SQLProduct.IsAdministrativeProduct ? "TRADEADMIN" : "TRADE");
            foreach (MQueueBase mQueue in pQueue)
            {
                IdInfo idData = new IdInfo()
                {
                    id = mQueue.id,
                    idInfos = new DictionaryEntry[]
                    {
                        new DictionaryEntry("IDDATA", mQueue.id),
                                                   new DictionaryEntry("IDDATAIDENT", idDataIdent),
                                                   new DictionaryEntry("IDDATAIDENTIFIER", mQueue.identifier)
                    }
                };
                aIdData.Add(idData);
            }
            return (IdInfo[])aIdData.ToArray(typeof(IdInfo)); ;
        }
        protected List<DictionaryEntry> GetMQueueIdData(MQueueBase pQueue)
        {

            List<DictionaryEntry> info = new List<DictionaryEntry>();
            string idDataIdent = Cst.OTCml_TBL.TRADE.ToString();
            if (TradeCommonInput.SQLProduct.IsRiskProduct)
                idDataIdent = Cst.OTCml_TBL.TRADERISK.ToString();
            if (TradeCommonInput.SQLProduct.IsAssetProduct)
                idDataIdent = Cst.OTCml_TBL.TRADEDEBTSEC.ToString();
            if (TradeCommonInput.SQLProduct.IsAdministrativeProduct)
                idDataIdent = Cst.OTCml_TBL.TRADEADMIN.ToString();

            info.Add(new DictionaryEntry("IDDATA", pQueue.id));
            info.Add(new DictionaryEntry("IDDATAIDENT", idDataIdent));
            info.Add(new DictionaryEntry("IDDATAIDENTIFIER", pQueue.identifier));
            info.Add(new DictionaryEntry("DATA1", pQueue.identifier));
            info.Add(new DictionaryEntry("PRODUCTNAME", TradeCommonInput.SQLProduct.Identifier));
            return info;
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20140708 [20179] Modify : Gestion IsModeMatch
        // EG 20160119 Refactoring Footer
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (Affichage en fonction si TRADEXML (IsTradeFoundWithXML) présent sur le trade)
        protected override void SetToolBarsStyle()
        {
            Control ctrlContainer = FindControl("tblMenu");
            bool isExistIdTemplate = StrFunc.IsFilled(TradeCommonInputGUI.TemplateIdentifier);

            WebControl ctrl;
            if (null != ctrlContainer)
            {
                #region Validate
                #region tdBtnRecord && tdBtnCancel
                ctrl = ctrlContainer.FindControl("BtnRecord") as WebControl;
                if (null != ctrl)
                {
                    if (IsBtnRecordVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");

                    if (IsBtnRecordVisible)
                        ctrl.Enabled = (null != SessionTools.License) && SessionTools.License.IsLicProductAuthorised_Add(TradeCommonInput.SQLProduct.Identifier);
                }

                ctrl = ctrlContainer.FindControl("BtnCancel") as WebControl;
                if (null != ctrl)
                {
                    if (IsBtnCancelVisible)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                #endregion tdBtnRecord && tdBtnCancel
                #endregion Validate

                #region Instrument
                ctrl = ctrlContainer.FindControl("div" + Cst.EnumElement.Instrument) as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");

                }
                #endregion Instrument

                #region Template
                ctrl = ctrlContainer.FindControl("div" + Cst.EnumElement.Template) as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeNew(InputGUI.CaptureMode) & isExistIdTemplate)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                #endregion Template

                #region Screen
                ctrl = ctrlContainer.FindControl("divScreen") as WebControl;
                // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeMatch(InputGUI.CaptureMode) || Cst.Capture.IsModeUpdatePostEvts(InputGUI.CaptureMode))
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                    else if (IsTradeFoundWithXML)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                #endregion Screen

                #region Duplication
                ctrl = ctrlContainer.FindControl("tbrduplicate") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeDuplicate(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");

                }
                #endregion Duplication

                #region Consult
                ctrl = ctrlContainer.FindControl("divtrade") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) ||
                        Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                //
                ctrl = CtrlTrade as WebControl;
                if (null != ctrl)
                    ((FpMLTextBox)ctrl).IsLocked = Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode);
                //                
                ctrl = ctrlContainer.FindControl("tbrconsult") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                #endregion Consult

                #region Mode
                ctrl = ctrlContainer.FindControl("btnCreation") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                //
                ctrl = ctrlContainer.FindControl("btnConsult") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeNewOrDuplicateOrReflect(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
                ctrl = ctrlContainer.FindControl("btnModification") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && IsPlaceHolderLoaded && IsTradeFoundWithXML)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                //
                ctrl = ctrlContainer.FindControl("btnInstrument") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeNew(InputGUI.CaptureMode))
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
                ctrl = ctrlContainer.FindControl("btnDuplication") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && IsPlaceHolderLoaded && IsTradeFoundWithXML)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
                ctrl = ctrlContainer.FindControl("btnUserAction") as WebControl;
                if (null != ctrl)
                {
                    if (Cst.Capture.IsModeConsult(InputGUI.CaptureMode) && IsPlaceHolderLoaded && IsTradeFoundWithXML)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                }
                // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
                ctrl = ctrlContainer.FindControl("tbraction") as WebControl;
                if (null != ctrl)
                {
                    if ((Cst.Capture.IsModeConsult(InputGUI.CaptureMode) || Cst.Capture.IsModeUpdateGen(InputGUI.CaptureMode)) && IsTradeFoundWithXML)
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    else
                    {
                        ctrl.Controls.RemoveAt(0);
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                }
                #endregion Action

                #region tbrResultAction
                ctrl = ctrlContainer.FindControl("tbrresultaction") as WebControl;
                if (null != ctrl)
                {
                    if (IsTradeFound && (false == Cst.Capture.IsModeNewCapture(InputGUI.CaptureMode)))
                    {
                        ControlsTools.RemoveStyleDisplay(ctrl);
                    }
                    else
                    {
                        ctrl.Controls.RemoveAt(0);
                        ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
                    }
                }
                #endregion Screen
            }
            // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
            Control ctrlContainerFooter = FindControl("divfooter");
            if (null != ctrlContainerFooter)
            {
                ctrlContainerFooter.Visible = IsTradeFoundWithXML;
            }
            Control ctrlContainerToolBarFooter = FindControl("pnlActionFooter");
            if (null != ctrlContainerToolBarFooter)
            {
                ctrlContainerToolBarFooter.Visible = IsBtnRecordVisible || IsBtnCancelVisible;
            }

            #region tblMainDescriptionAndStatus
            ctrl = FindControl("tblMainDescriptionAndStatus") as WebControl;
            if (null != ctrl)
            {
                if (IsTradeFound)
                    ControlsTools.RemoveStyleDisplay(ctrl);
                else
                    ctrl.Style.Add(HtmlTextWriterStyle.Display, "none");
            }
            #endregion tblMainDescriptionAndStatus

            #region ValidationSummary
            ctrl = FindControl("ValidationSummary") as WebControl;
            if (null != ctrl)
            {
                //FI 20140708 [20179] isShowSummary uniquement si IsModeInput
                bool isShowSummary = Cst.Capture.IsModeInput(InputGUI.CaptureMode) &&
                                (false == TradeCommonCaptureGen.IsInputIncompleteAllow(InputGUI.CaptureMode));
                ((ValidationSummary)ctrl).ShowSummary = isShowSummary;
                ((ValidationSummary)ctrl).ShowMessageBox = (!isShowSummary);
            }
            #endregion ValidationSummary
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetDropDownInstrumentProdcuctTemplateScreen()
        {
            if (FindControl("tblMenu$lst" + Cst.EnumElement.Instrument.ToString()) is DropDownList ddl)
            {
                bool isFound = ControlsTools.DDLSelectByValue(ddl, TradeCommonInput.Product.IdI.ToString());
                if (isFound)
                    m_SelCommonProduct.OnSelectedElementChanged(ddl, null);

                if (isFound)
                {
                    isFound = false;
                    ddl = FindControl("tblMenu$lst" + Cst.EnumElement.Template.ToString()) as DropDownList;
                    if (null != ddl)
                        isFound = ControlsTools.DDLSelectByText(ddl, TradeCommonInputGUI.TemplateIdentifier);
                    // EG 20210928 [XXXXX] Complément de contrôle sur Template (et ainsi afficher l'écran associé)
                    if (isFound || TradeCommonInput.TradeStatus.IsStEnvironment_Template)
                    {
                        ddl = FindControl("tblMenu$lst" + Cst.EnumElement.Screen.ToString()) as DropDownList;
                        if (null != ddl)
                            _ = ControlsTools.DDLSelectByValue(ddl, InputGUI.CurrentIdScreen);
                    }
                }
            }
        }

        /// <summary>
        /// Génère le javascript qui affiche un message aprèx la sauvegarde
        /// </summary>
        /// <param name="lRet"></param>
        /// <param name="msg"></param>
        /// <param name="msgDetail"></param>
        protected void DisplayMessageAfterRecord(TradeCommonCaptureGen.ErrorLevel lRet, string msg, string msgDetail)
        {

            //
            if (StrFunc.IsFilled(msg))
            {
                //MsgForAlertImmediate = msg;
                //20090210 FI [16483] Affichage d'une ModalPopup au lieu d'une alert
                string title;
                if (this.TradeCommonInput.Product.IsADM)
                    title = Ressource.GetString("RecordTradeAdminInput_Title");
                else if (this.TradeCommonInput.Product.IsASSET)
                    title = Ressource.GetString("RecordSecurityInput_Title");
                else if (this.TradeCommonInput.Product.IsMarginRequirement)
                    title = Ressource.GetString("RecordTradeRiskInput_Title");
                else
                    title = Ressource.GetString("RecordTradeInput_Title");
                //
                //20091016 FI [Rebuild identification] une exception peut-être générée même si la sauvegarde est en succès=> dans ce cas affichage en orange
                Color color = Color.Green;
                if (false == TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                    color = Color.Red;

                Color colorDet = color;
                if (TradeCommonCaptureGen.IsErrorAfterRecord(lRet))
                    colorDet = Color.Orange;

                Style style = new Style
                {
                    ForeColor = color
                };
                style.Font.Bold = true;

                Style styleDet = new Style
                {
                    ForeColor = colorDet
                };
                styleDet.Font.Bold = false;

                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
                if (false == TradeCommonCaptureGen.IsRecordInSuccess(lRet))
                    status = ProcessStateTools.StatusErrorEnum;
                if (TradeCommonCaptureGen.IsErrorAfterRecord(lRet))
                    status = ProcessStateTools.StatusWarningEnum;

                JavaScript.DialogStartUpImmediate(this, title, msg + Cst.HTMLBreakLine + msgDetail, false, status, JavaScript.DefautHeight, JavaScript.DefautWidth);
            }

        }

        /// <summary>
        ///  
        /// </summary>
		/// EG 20201002 [XXXXX] Gestion des ouvertures via window.open (nouveau mode : opentab : mode par défaut)
        protected virtual void RefreshLogButton()
        {

            int idLog = 0;
            if (null != this.TradeCommonInput.SQLLastTradeLog && (this.TradeCommonInput.SQLLastTradeLog.IdProcessL > 0))
                idLog = this.TradeCommonInput.SQLLastTradeLog.IdProcessL;
            //
            WCToolTipButton btn = FindButtonLog();
            if (null != btn)
            {
                btn.Visible = (idLog > 0);
                if (idLog > 0)
                {
                    string page = ReferentialTools.GetURLOpenReferential("Log", "PROCESS_L", Cst.ConsultationMode.ReadOnly, false, null, null, null, "IDPROCESS_L", idLog.ToString(), null, null, IdMenu.GetIdMenu(IdMenu.Menu.PROCESS_L), null);
                    string js = JavaScript.GetWindowOpen(page, Cst.WindowOpenStyle.EfsML_FormReferential) + "return false;";
                    ControlsTools.SetOnClientClick(btn, js);
                }
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected WCToolTipButton FindButtonLog()
        {
            WCToolTipButton btn = null;
            if (FindControl("tblNotificationRow1") is TableRow tr)
                btn = tr.FindControl("btnLog") as WCToolTipButton;
            return btn;
        }

        /// <summary>
        /// Retourne un LockObject. Il sera utiliser pour locker le trade. 
        /// </summary>
        private LockObject GetLockTrade()
        {
            LockObject lockTrade = null;
            if (TradeCommonInput.IsTradeFound)
                lockTrade = new LockObject(TypeLockEnum.TRADE, this.TradeIdT, this.TradeIdentifier, LockTools.Exclusive);
            return lockTrade;
        }

        /// <summary>
        ///  Vérification que les variables session sont encore accessibles
        ///  <para>Si elles ne sont plus accessibles, Spheres® ouvre de nouveau l'URL de manière à ne pas être en PostBack,les variables session vont alors être initialiées</para>
        /// </summary>
        private void CheckSessionState()
        {
            if ((IsPostBack) && (InputGUI == null))
            {
                //Ce cas se produit lorsque l'utilisateur ouvre le formulaire des événement et qu'in timeout server se produit au post de la page
                //Ds ce cas Spheres® ouvre le formulaire login.aspx et ouvre le formulaire des évènements.
                //Ensuite l'utilsateur poste la page de saisie. Nous avons alors IsPostBack=true et  InputGUI == null

                Response.Redirect(this.Request.Url.AbsoluteUri);
            }
        }

        /// <summary>
        /// Retourne un placeHolder qui contient un contrôle label qui expliquent pourquoi le trade est non trouvé 
        /// </summary>
        /// <returns></returns>
        private PlaceHolder GetContolsTradeNotFound()
        {
            PlaceHolder plh = new PlaceHolder();

            //Trade inconnu => Recherche de la raison (identifiant inexistant, permissions insuffisantes sur les acteurs ou les instruments, license insuffisante ... etc..)
            string msg = Ressource.GetString2("Msg_TradeConsult", TradeCommonInputGUI.MainRessource);
            string css = EFSCssClass.Msg_Information;
            bool isUnknown = true;
            string identifier;
            string res;
            if (IsModeConsult)
            {
                identifier = TradeIdentifier;
                res = "Msg_TradeNotFound";
            }
            else
            {
                identifier = TradeCommonInputGUI.TemplateIdentifier;
                res = "Msg_TemplateNotFound";
            }
            //
            if (IsModeConsult)
            {
                //Aucun message si "0" ou Empty et load d'un trade UNDEFINED on template (on est ici sur le switch en mode Consult)
                isUnknown = StrFunc.IsFilled(identifier) && ("0" != identifier);
            }
            //	
            if (isUnknown)
            {
                //
                msg = Ressource.GetString2(res, identifier);
                css = EFSCssClass.Msg_Alert;
                //
                if ((null != TradeCommonInput.SQLProduct) && ((false == SessionTools.License.IsLicProductAuthorised(TradeCommonInput.SQLProduct.Identifier))))
                {
                    css = EFSCssClass.Msg_Alert;
                    msg = Ressource.GetString("Msg_LicProduct");
                }
                else
                {
                    Cst.StatusEnvironment stEnv = Cst.StatusEnvironment.UNDEFINED;
                    if (false == IsModeConsult)
                        stEnv = Cst.StatusEnvironment.TEMPLATE;
                    //
                    //Sans Restriction
                    SQL_TradeCommon sqlTrade = new SQL_TradeCommon(SessionTools.CS, SQL_TableWithID.IDType.Identifier, identifier, stEnv,
                        SQL_TradeCommon.RestrictEnum.No, SessionTools.User, SessionTools.SessionID, TradeCommonInputGUI.GetSQLRestrictProduct());
                    if (sqlTrade.IsFound)
                    {
                        css = EFSCssClass.Msg_Warning;
                        msg = Ressource.GetString2("Msg_TradeNotAuthorized", identifier);
                    }
                }
            }
            //Affichage dans un label du message
            Label lbl = new Label
            {
                Text = msg,
                CssClass = css
            };
            plh.Controls.AddAt(0, lbl);

            return plh;
        }


        /// <summary>
        ///  Ajoute à {pDisabledReason} la ressource {pResNewDisabledReason}
        /// </summary>
        /// <param name="pDisabledReason"></param>
        /// <param name="pResNewDisabledReason"></param>
        /// FI 20130308 [] new method
        protected static void AddActionDisabledReason(ref string pDisabledReason, string pResNewDisabledReason)
        {
            AddActionDisabledReason(ref pDisabledReason, pResNewDisabledReason, null);
        }

        /// <summary>
        ///  Ajoute à {pDisabledReason} la ressource {pResNewDisabledReason}
        /// </summary>
        /// <param name="pDisabledReason"></param>
        /// <param name="pResNewDisabledReason"></param>
        /// <param name="pResArg">Argument de la ressource</param>
        /// FI 20130308 [] new method
        protected static void AddActionDisabledReason(ref string pDisabledReason, string pResNewDisabledReason, string pResArg)
        {
            if (StrFunc.IsFilled(pDisabledReason))
                pDisabledReason += Cst.CrLf;
            pDisabledReason += Ressource.GetString2(pResNewDisabledReason, pResArg);
        }


        /// <summary>
        ///  Stocke dans cookie la plateforme et le marché par défaut à partir des données courantes sur le trade (TradeCommonInput)
        /// </summary>
        /// FI 20161214 [21916] Modify
        // EG 20171113 Upd SetCookieDefaultFacilityAndMarket replace SetCookieDefaultMarket
        // EG 20221201 [25639] [WI484] Add Environmental
        private void SetCookieDefaultFacilityAndMarket()
        {

            Nullable<Cst.SQLCookieElement> facilityCookieElement = null;
            Nullable<Cst.SQLCookieElement> marketCookieElement = null;

            ProductContainer _product = TradeCommonInput.Product;

            if (_product.IsExchangeTradedDerivative)
            {
                facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility;
                marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket;
            }
            else if (_product.IsEquitySecurityTransaction)
            {
                facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_ESE;
                marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_ESE;
            }
            else if (_product.IsCommoditySpot)
            {
                ICommoditySpot _commoditySpot = (ICommoditySpot)_product.Product;
                if (_commoditySpot.IsGas)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_gas;
                    marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_gas;
                }
                else if (_commoditySpot.IsEnvironmental)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_env;
                    marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_env;
                }
                else if (_commoditySpot.IsElectricity)
                {
                    facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_COMS_elec;
                    marketCookieElement = Cst.SQLCookieElement.TradeDefaultMarket_COMS_elec;
                }
            }
            else if (_product.IsTradeMarket)
            {
                facilityCookieElement = Cst.SQLCookieElement.TradeDefaultFacility_Other;
            }


            if (facilityCookieElement.HasValue)
            {
                object defaultFacility = TradeCommonInput.GetDefault(TradeInput.DefaultEnum.facility);
                if (null != defaultFacility)
                    AspTools.WriteSQLCookie(facilityCookieElement.Value.ToString(), defaultFacility.ToString());
            }

            if (marketCookieElement.HasValue)
            {
                object defaultMarket = TradeCommonInput.GetDefault(TradeInput.DefaultEnum.market);
                if (null != defaultMarket)
                    AspTools.WriteSQLCookie(marketCookieElement.Value.ToString(), defaultMarket.ToString());
            }
        }

        /// <summary>
        ///  Sauvegarde du résultat du matching des données
        /// </summary>
        /// <param name="pList">Liste des ccis qui nécessite matching</param>
        /// FI 20140708 [20179] Add Method
        /// FI 20140728 [20255] Modify
        protected override Boolean RecordMatch(List<CustomCaptureInfo> pList)
        {
            if (false == (this.TradeCommonInput.IdT > 0))
                throw new InvalidProgramException("the trade is unknown (idT=0)");

            Boolean ret = false;
            string msgDetail = string.Empty;

            try
            {
                CultureInfo GBCulture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");

                // FI 20200820 [25468] Dates systèmes en UTC
                //Date système pour alimentation des colonnes DTUPD et DTINS ou equivalentes
                DateTime dtSys = OTCmlHelper.GetDateSysUTC(SessionTools.CS);

                //Delete TRADESTMATCHCCI
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), TradeCommonInput.IdT);
                QueryParameters qryParameters = new QueryParameters(SessionTools.CS, @"delete from dbo.TRADESTMATCHCCI where IDT = @IDT", dp);
                DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, qryParameters.Query, qryParameters.Parameters.GetArrayDbParameter());

                //Insert TRADESTMATCHCCI
                dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDT), TradeCommonInput.IdT);
                dp.Add(new DataParameter(SessionTools.CS, "CLIENTID", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "MATCHSTATUS", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "DATATYPE", DbType.AnsiString, 16), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "VALUE", DbType.AnsiString, 64), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "LABELCULTURE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "LABELVALUE", DbType.AnsiString, 64), Convert.DBNull);
                dp.Add(new DataParameter(SessionTools.CS, "LABELVALUE_EN", DbType.AnsiString, 64), Convert.DBNull);

                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDAINS), SessionTools.User.IdA);
                dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.DTINS), dtSys);

                StrBuilder select = new StrBuilder();
                foreach (CustomCaptureInfo cci in pList)
                {
                    dp["CLIENTID"].Value = cci.ClientId_WithoutPrefix;

                    string status = string.Empty;
                    if (false == cci.NewValueMatch.HasValue)
                        status = Cst.NotAvailable;
                    else
                        status = cci.NewValueMatch.Value.ToString().ToUpper();
                    dp["MATCHSTATUS"].Value = status;

                    dp["DATATYPE"].Value = cci.DataType.ToString();
                    dp["VALUE"].Value = cci.NewValue;

                    dp["LABELCULTURE"].Value = CultureInfo.CurrentCulture.Name;
                    dp["LABELVALUE"].Value = GetCciCustomObject(cci.ClientId).Resource;
                    dp["LABELVALUE_EN"].Value = GetCciCustomObject(cci.ClientId).GetResource(GBCulture);    

                    QueryParameters qry = new QueryParameters(SessionTools.CS, "select @IDT,@CLIENTID,@MATCHSTATUS,@DATATYPE,@VALUE,@LABELCULTURE,@LABELVALUE,@LABELVALUE_EN,@DTINS,@IDAINS from DUAL", dp);
                    string queryItem = qry.GetQueryReplaceParameters(false);

                    if (select.Length > 0)
                        select += SQLCst.UNIONALL;

                    select += queryItem;
                }

                string queryInsert = "insert into dbo.TRADESTMATCHCCI(IDT,CLIENTID,MATCHSTATUS,DATATYPE,VALUE,LABELCULTURE,LABELVALUE,LABELVALUE_EN,DTINS,IDAINS)";
                queryInsert += select;

                DataHelper.ExecuteNonQuery(SessionTools.CS, CommandType.Text, queryInsert);
                ret = true;

                //UPDATE TRADESTMATCH
                Boolean isMatch = (pList.Where(t => t.NewValueMatch.HasValue && t.NewValueMatch.Value == Cst.MatchEnum.match).Count() == pList.Count);
                Boolean isMisMatch = (pList.Where(t => t.NewValueMatch.HasValue && t.NewValueMatch.Value == Cst.MatchEnum.mismatch).Count() > 0);
                if (isMatch || isMisMatch)
                {
                    StWeightCollection stWeights = new StMatchWeightCollection().LoadWeight(CSTools.SetCacheOn(SessionTools.CS));
                    if (ArrFunc.IsFilled(stWeights.StWeight))
                    {
                        var lstStWeightCustomValue =
                              from item in stWeights.StWeight
                              where StrFunc.IsFilled(item.CustomValue)
                              select item;

                        if (lstStWeightCustomValue.Count() > 0)
                        {
                            StatusCollection tradeSt = this.TradeCommonInput.TradeStatus.GetStUsersCollection(StatusEnum.StatusMatch);
                            Boolean isRecord = false;

                            foreach (StWeight item in lstStWeightCustomValue)
                            {
                                if (isMatch)
                                {
                                    if (item.CustomValue.Contains("TickedWhenMatch"))
                                        tradeSt[item.IdSt].NewValue = 1;
                                    else if (item.CustomValue.Contains("UntickedWhenMatch"))
                                        tradeSt[item.IdSt].NewValue = 0;
                                }
                                else if (isMisMatch)
                                {
                                    if (item.CustomValue.Contains("TickedWhenMismatch"))
                                        tradeSt[item.IdSt].NewValue = 1;
                                    else if (item.CustomValue.Contains("UntickedWhenMismatch"))
                                        tradeSt[item.IdSt].NewValue = 0;
                                }

                                if (tradeSt[item.IdSt].IsModify)
                                {
                                    //FI 20140728 [20255] Alimentation de NewDtEffect
                                    tradeSt[item.IdSt].NewDtEffect = dtSys;
                                    isRecord = true;
                                }
                            }

                            if (isRecord)
                            {
                                ret &= TradeCommonInput.TradeStatus.UpdateStUser(SessionTools.CS, null,Mode.Trade,  TradeCommonInput.IdT, SessionTools.User.IdA, dtSys);
                                msgDetail = Ressource.GetString("Msg_StMatchUpdOk");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLogException(ex);

                msgDetail = ex.Message;
                ret = false;
            }

            DisplayRecordMatchAfterRecord(ret, msgDetail);

            return ret;
        }

        /// <summary>
        /// Génère le javascript qui affiche un message aprèx la sauvegarde
        /// </summary>
        /// <param name="msg"></param>
        protected void DisplayRecordMatchAfterRecord(Boolean bResult, string msgDetail)
        {
            string msg;
            if (bResult)
                msg = Ressource.GetString("Msg_ConfirmTradeSaved");
            else
                msg = Ressource.GetString("Msg_ConfirmTradeSavedError");

            msg = msg.Replace("{0}", this.TradeCommonInput.Identification.Identifier);

            string title = Ressource.GetString("RecordMaching_Title");
            ProcessStateTools.StatusEnum status = ProcessStateTools.StatusNoneEnum;
            if (false == bResult)
                status = ProcessStateTools.StatusErrorEnum;

            JavaScript.DialogStartUpImmediate(this, title, msg + Cst.HTMLBreakLine2 + msgDetail, false, status, JavaScript.DefautHeight, JavaScript.DefautWidth);
        }


        /// <summary>
        /// Alimentation du log des actions utilisateur si consultation d'un trade
        /// </summary>
        /// FI 20141021 [20350] Add Method
        protected void SetRequestTrackBuilderItemProcess(Cst.Capture.ModeEnum pCaptureMode)
        {
            Nullable<RequestTrackProcessEnum> processType = null;
            switch (pCaptureMode)
            {
                case Cst.Capture.ModeEnum.New:
                case Cst.Capture.ModeEnum.Duplicate:
                case Cst.Capture.ModeEnum.Reflect:
                    processType = RequestTrackProcessEnum.New;
                    break;
                case Cst.Capture.ModeEnum.RemoveReplace:
                    processType = RequestTrackProcessEnum.RemoveReplace;// pas de trade annulé dans le log
                    break;
                case Cst.Capture.ModeEnum.Update:
                    processType = RequestTrackProcessEnum.Modify;
                    break;
                case Cst.Capture.ModeEnum.RemoveAllocation:
                case Cst.Capture.ModeEnum.RemoveOnly:
                    processType = RequestTrackProcessEnum.Remove; //Annulation
                    break;
                case Cst.Capture.ModeEnum.Correction:
                    processType = RequestTrackProcessEnum.Correction;
                    break;
                case Cst.Capture.ModeEnum.OptionExercise:
                case Cst.Capture.ModeEnum.OptionAssignment:
                case Cst.Capture.ModeEnum.OptionAbandon:
                    processType = (RequestTrackProcessEnum)Enum.Parse(typeof(RequestTrackProcessEnum), pCaptureMode.ToString());
                    break;
                case Cst.Capture.ModeEnum.PositionTransfer:
                    processType = RequestTrackProcessEnum.PositionTransfer; // pas les books précédents
                    break;
                case Cst.Capture.ModeEnum.PositionCancelation:
                    processType = RequestTrackProcessEnum.PositionCancellation;
                    break;
                case Cst.Capture.ModeEnum.TradeSplitting:
                    processType = RequestTrackProcessEnum.Split;
                    break;
                case Cst.Capture.ModeEnum.UnderlyingDelivery:
                    processType = RequestTrackProcessEnum.UnderlyingDelivery;
                    break;
            }

            if (null != processType)
            {
                RequestTrackTradeBuilder builder = new RequestTrackTradeBuilder
                {
                    DataDocument = TradeCommonInput.DataDocument,
                    TradeIdentIdentification = TradeCommonInput.Identification,
                    ProcessType = processType.Value,
                    action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual)
                };
                this.RequestTrackBuilder = builder;
            }
        }
        /// <summary>
        /// Le menu ACTION n'est affiché que si le trade dispose de TRADEXML
        /// </summary>
        // EG 20240619 [WI969] Trade Input: TRADE without TRADEXML (IsTradeFoundWithXML) 
        protected override void RefreshMnuAction()
        {
            if (TradeCommonInput.IsTradeFoundWithXML)
            {
                base.RefreshMnuAction();
            }
            else
            {
                if (FindControl("tblMenu") is Control ctrlContainer)
                {
                    if (ctrlContainer.FindControl("mnuAction") is skmMenu.Menu ctrl)
                        ctrl.Items.Clear();
                }
            }
        }
        #endregion Methods

    }
}

