using EFS.ACommon;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.Web;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.Interface;
using FixML.Interface;
using FpML.Interface;
using System;
using System.Collections;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    // EG 20150920 [21314] Int (int32) to Long (Int64) 
    // EG 20170127 Qty Long To Decimal
    // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
    public partial class Split : PageBase
    {
		#region Declaration
        private const int NUMBEROFCOLUMS = 9;
        // EG 20160308 Migration vs2013
        private string __EVENTTARGET;
        private string __EVENTARGUMENT;

        private int iMax;

        private string m_ParentGUID;
        private TradeInputGUI m_InputGUI;
        private TradeInput m_Input;
        private TradeHeaderBanner m_TradeHeaderBanner;
        private string m_StActivation;
        private bool m_IsShowPosEfct; //PL 20150723 Add isShowPosEfct
        private string m_PosEfct;
        private bool m_IsOption;
        // EG 20150920 [21314] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private long m_InitialQty;
        private IParty m_DealerParty;
        private IBookId m_DealerBook;
        private bool m_IsDealerBuyer;
        private ArrayList m_alDspActors = new ArrayList();
        private ArrayList m_alDspBooks = new ArrayList();
        // EG 20150920 [21314] Int (int32) to Long (Int64) 
        // EG 20170127 Qty Long To Decimal
        private long m_assignedQty;
        private int m_assignedCount;
        private string idMenu;
        private string  m_MainMenuClassName;
        #endregion

        #region Members
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputGUISessionID
        {
            get { return m_ParentGUID + "_GUI"; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// FI 20200518 [XXXXX] Rename
        protected string ParentInputSessionID
        {
            get { return m_ParentGUID + "_Input"; }
        }
        #endregion Members

        #region Accessors
        private string FmtInitialQty
        {
            get
            {
                return m_InitialQty.ToString();
            }
        }

        private string FmtAssignedQty
        {
            get
            {
                return m_assignedQty.ToString();
            }
        }
        private string FmtRemainQty
        {
            get
            {
                return (m_InitialQty - m_assignedQty).ToString();
            }
        }
        #endregion Accessors
        #region protected override OnInit
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// EG 20150302 RptSideProductContainer refactoring
        /// FI 20170116 [21916] Modify
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected override void OnInit(EventArgs e)
        {
            idMenu = IdMenu.GetIdMenu(IdMenu.Menu.InputTrade_SPLIT);
            m_MainMenuClassName = ControlsTools.MainMenuName(idMenu);

            __EVENTTARGET = Page.Request.Params["__EVENTTARGET"];
            __EVENTARGUMENT = Page.Request.Params["__EVENTARGUMENT"];
            
            m_ParentGUID = Request.QueryString["GUID"];
            if (StrFunc.IsEmpty(m_ParentGUID))
                throw new ArgumentException("Argument GUID expected");
            
            // FI 20200518 [XXXXX] Utilisation de DataCache
            //m_InputGUI = Session[InputGUISessionID] as TradeInputGUI;
            //m_Input = Session[InputSessionID] as TradeInput;
            m_InputGUI = DataCache.GetData<TradeInputGUI>(ParentInputGUISessionID);
            m_Input = DataCache.GetData<TradeInput>(ParentInputSessionID);


            // FI 20170116 [21916] appel à la méthode ProductContainer.RptSide() pour initialisé rptSide
            RptSideProductContainer rptSide = m_Input.DataDocument.CurrentProduct.RptSide() ;
            ProductContainer product = m_Input.DataDocument.CurrentProduct;

            m_IsShowPosEfct = m_Input.SQLInstrument.IsFungibilityMode_OPENCLOSE;

            if (product.IsExchangeTradedDerivative)
            {
                m_IsOption = ((ExchangeTradedDerivativeContainer) rptSide).IsOption;
            }

            if (null != rptSide)
            {
                IFixParty dealerFixParty = rptSide.GetDealer();
                m_DealerParty = m_Input.DataDocument.GetParty(dealerFixParty.PartyId.href);
                m_DealerBook = m_Input.DataDocument.GetBookId(dealerFixParty.PartyId.href);
                m_IsDealerBuyer = (m_DealerParty.Id == rptSide.GetBuyerSeller(FixML.Enum.SideEnum.Buy).PartyId.href);
                m_InitialQty = Convert.ToInt64(rptSide.Qty);
                m_StActivation = m_Input.TradeStatus.stActivation.CurrentSt;
                m_PosEfct = rptSide.RptSide[0].PositionEffect.ToString();
            }
                        
            base.OnInit(e);

            #region Members initialization
            AbortRessource = true;
            #endregion

            #region Checking
            bool bOk = true;
            //Check permission user for this page (GlopPL à faire)
            if (bOk)
            {
                //Check UrlReferrer  (GlopPL à faire)
                _ = Convert.ToString(Request.UrlReferrer);
                //bOk = (urlReferrer.ToString()=="ListReferentiel.aspx");
            }
            if (!bOk)
            {
                Response.Redirect("Sommaire.aspx");
            }
            #endregion
            
            PageConstruction();
        }
        #endregion
        #region protected override CreateChildControls
        // EG 20210224 [XXXXX] Regroupement (PageReferential.js et Referential.js en ReferentialCommon.js et minification
        // EG 20210224 [XXXXX] Minification Trade.js
        protected override void CreateChildControls()
        {
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/ReferentialCommon.min.js"));
            // FI 20201021 [XXXXX] Add Trade.js pour accéder à la méthode LoadAutoCompleteData
            ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/Trade.min.js"));
            // FI 20201021 [XXXXX] 
            JQuery.WriteInitialisationScripts(this, "AutoComplete", "LoadAutoCompleteData();");
            base.CreateChildControls();
        }
        #endregion

        #region protected Page_Load
        protected void Page_Load(object sender, EventArgs e)
        {
            // Gestion du focus suite à postback
            ClientScript.RegisterStartupScript(typeof(Split), "ScriptDoFocus", SCRIPT_DOFOCUS.Replace("REQUEST_LASTFOCUS", Request["__LASTFOCUS"]), true);
            HookOnFocus(this.Page as Control);

            m_TradeHeaderBanner.DisplayHeader(true, true);

            bool isModify_ActorOrBook = false;
            WCTextBox2 txt;
            Label lbl;
            HiddenField hid;
            if (IsPostBack)
            {
                #region Réaffichage des Displaynames Actors et Books
                if (m_alDspActors.Count > 0)
                {
                    for (int i = 0; i <= iMax; i++)
                    {
                        if (m_alDspActors.Count >= i + 1)
                        {
                            lbl = FindControl(Cst.DSP + "Actor" + i.ToString()) as Label;
                            lbl.Text = m_alDspActors[i].ToString();
                            lbl = FindControl(Cst.DSP + "Book" + i.ToString()) as Label;
                            lbl.Text = m_alDspBooks[i].ToString();
                        }
                    }
                }
                #endregion Réaffichage des Displaynames Actors et Books

                if (__EVENTTARGET != null)
                {
                    bool isFound = false;
                    SQL_Actor sql_Actor = null;
                    SQL_Book sql_Book = null;

                    if (__EVENTTARGET.StartsWith(Cst.TXT + "Actor") || __EVENTTARGET.StartsWith(Cst.TXT + "Book"))
                    {
                        #region Actor/Book
                        isModify_ActorOrBook = true;

                        if (__EVENTTARGET.StartsWith(Cst.TXT + "Actor"))
                        {
                            #region Actor
                            txt = FindControl(__EVENTTARGET) as WCTextBox2;
                            lbl = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.DSP)) as Label;
                            hid = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.HID)) as HiddenField;
                            if (StrFunc.IsFilled(txt.Text))
                            {
                                EFS.Actor.RoleActor[] roleActor = new EFS.Actor.RoleActor[] { EFS.Actor.RoleActor.COUNTERPARTY };

                                EFS.GUI.CCI.CciTools.IsActorValid(SessionTools.CS, txt.Text, out sql_Actor, out _, out isFound, false, roleActor, SessionTools.User, SessionTools.SessionID);
                            }
                            if (isFound)
                            {
                                txt.ForeColor = Color.Empty;
                                txt.Text = sql_Actor.Identifier;
                                lbl.Text = sql_Actor.DisplayName;
                                hid.Value = sql_Actor.Id.ToString();
                            }
                            else
                            {
                                txt.ForeColor = Color.Red;
                                lbl.Text = string.Empty;
                                hid.Value = string.Empty;
                            }
                            #endregion Actor
                        }

                        if (__EVENTTARGET.StartsWith(Cst.TXT + "Book") || isFound)
                        {
                            #region Book
                            if (isFound)
                            {
                                //Tip: On est ici dans le cas où l'acteur vient d'être saisi et il est correctement identifié.
                                //     On vérifie la compatibilité d'un éventuel Book existant.
                                //     Pour cela, on modifie __EVENTTARGET afin de considérer que c'est maitenant le book qui vien d'être saisi.
                                __EVENTTARGET = __EVENTTARGET.Replace("Actor", "Book");
                                isFound = false;
                            }
                            int ida = 0;

                            txt = FindControl(__EVENTTARGET) as WCTextBox2;
                            lbl = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.DSP)) as Label;
                            hid = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.HID)) as HiddenField;
                            if (StrFunc.IsFilled(txt.Text))
                            {
                                HiddenField hidActor = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.HID).Replace("Book", "Actor")) as HiddenField;
                                if (IntFunc.IsPositiveInteger(hidActor.Value))
                                {
                                    ida = IntFunc.IntValue(hidActor.Value);
                                }

                                EFS.GUI.CCI.CciTools.IsBookValid(SessionTools.CS, txt.Text, out sql_Book, out _, out isFound, ida, SessionTools.User, SessionTools.SessionID);
                            }
                            if (isFound)
                            {
                                txt.ForeColor = Color.Empty;
                                txt.Text = sql_Book.Identifier;
                                lbl.Text = sql_Book.FullName;
                                hid.Value = sql_Book.Id.ToString();

                                if (ida == 0)
                                {
                                    //Alimentation de l'acteur via l'acteur propriétaire du book
                                    sql_Actor = new SQL_Actor(SessionTools.CS, sql_Book.IdA, SQL_Table.RestrictEnum.Yes, SQL_Table.ScanDataDtEnabledEnum.Yes, SessionTools.User, SessionTools.SessionID);

                                    if (sql_Actor.IsLoaded)
                                    {
                                        WCTextBox2 txtActor = FindControl(__EVENTTARGET.Replace("Book", "Actor")) as WCTextBox2;
                                        Label lblActor = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.DSP).Replace("Book", "Actor")) as Label;
                                        HiddenField hidActor = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.HID).Replace("Book", "Actor")) as HiddenField;

                                        txtActor.ForeColor = Color.Empty;
                                        txtActor.Text = sql_Actor.Identifier;
                                        lblActor.Text = sql_Actor.DisplayName;
                                        hidActor.Value = sql_Actor.Id.ToString();
                                    }
                                }

                                //Alimentation du Position Effect
                                if (m_IsShowPosEfct)
                                {
                                    string posEfct = (m_IsOption ? sql_Book.OptionsPosEffect : sql_Book.FuturesPosEffect);
                                    if (StrFunc.IsFilled(posEfct))
                                    {
                                        if (!ExchangeTradedDerivativeTools.IsPositionEffect_Open(posEfct))
                                        {
                                            //Quelque soit le paramétrage en vigueur, dès lors qu'il diffère de "Open" on considère "Close" (ex. LIFO)
                                            posEfct = ExchangeTradedDerivativeTools.GetPositionEffect_Close();
                                        }
                                        WCDropDownList2 ddlPosEfct = FindControl(__EVENTTARGET.Replace(Cst.TXT, Cst.DDL).Replace("Book", "PosEfct")) as WCDropDownList2;
                                        ControlsTools.DDLSelectByValue(ddlPosEfct, posEfct); //Warning: Ici on utilise DDLSelectByValue()
                                    }
                                }
                            }
                            else
                            {
                                txt.ForeColor = Color.Red;
                                lbl.Text = string.Empty;
                                hid.Value = string.Empty;
                            }
                            #endregion Book
                        }
                        #endregion Actor/Book
                    }
                    else if (__EVENTTARGET.StartsWith(Cst.TXT + "Qty"))
                    {
                        #region Qty
                        //Calcul de la qté totale affectée pour initialisation de la qté restante sur la dernière ligne.
                        CaclAssignedQuantityAndCount();

                        if (m_InitialQty > m_assignedQty)
                        {
                            for (int i = 0; i <= iMax; i++)
                            {
                                txt = FindControl(Cst.TXT + "Qty" + i.ToString()) as WCTextBox2;
                                //if (!IntFunc.IsPositiveInteger(txt.Text))
                                if (false == IntFunc.IsPositiveInteger(txt.Text))
                                {
                                    txt.Text = FmtRemainQty;
                                    break;
                                }
                            }
                        }
                        #endregion Qty
                    }
                }
            }
            else
            {
                txt = FindControl(Cst.TXT + "Qty0") as WCTextBox2;
                txt.Text = FmtInitialQty;

                if (m_InitialQty == 1)
                {
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.WARNING;
                    MsgForAlertImmediate = Ressource.GetString2("Msg_SplitTrade_Impossible", FmtInitialQty);
                    CloseAfterAlertImmediate = true;
                }
            }

            #region Sauvegarde des Displaynames Actors et Books
            if ((!IsPostBack) || isModify_ActorOrBook)
            {
                m_alDspActors.Clear();
                m_alDspBooks.Clear();
                for (int i = 0; i <= iMax; i++)
                {
                    lbl = FindControl(Cst.DSP + "Actor" + i.ToString()) as Label;
                    m_alDspActors.Add(lbl.Text);
                    lbl = FindControl(Cst.DSP + "Book" + i.ToString()) as Label;
                    m_alDspBooks.Add(lbl.Text);
                }
            }
            #endregion Sauvegarde des Displaynames Actors et Books
            
            // FI 20201021 [XXXXX] Si l'acteur est valide => Seul les books de l'acteurs sont disponibles
            for (int i = 0; i <= iMax; i++)
            {
                string suffix = i.ToString();
                HiddenField hidSplit = FindControl(Cst.HID + "Actor" + suffix) as HiddenField;
                if (StrFunc.IsFilled(hidSplit.Value))
                {
                    WCTextBox2 txtSplit = FindControl(Cst.TXT + "Book" + suffix) as WCTextBox2;
                    WebControl btnSplit = FindControl(Cst.BUT + "Book" + suffix) as WebControl;

                    ControlsTools.SetOnClientClick_ButtonReferential(btnSplit, "BOOK", null, Cst.ListType.Repository, true,
                        null, txtSplit.ClientID, null, null, null, hidSplit.Value, null, null);
                }
            }
        }
        #endregion

        //* --------------------------------------------------------------- *//
        // Sauvegarde des données pour maintien après PostBack (VIEWSTATE)
        //* --------------------------------------------------------------- *//
        #region LoadViewState
        /// <summary>
        /// Lecture des groupes/status dans le ViewState
        /// </summary>
        /// <param name="savedState"></param>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override void LoadViewState(object savedState)
        {
            object[] viewState = (object[])savedState;
            base.LoadViewState(viewState[0]);

            m_alDspActors = (ArrayList)viewState[1];
            m_alDspBooks = (ArrayList)viewState[2];
        }
        #endregion LoadViewState
        #region SaveViewState
        /// <summary>
        /// Sauvegarde des groupes/status dans le ViewState
        /// </summary>
        /// <returns></returns>
        /// FI 20200217 [XXXXX] Reafactoring puisque Pagebase viewState ne contient plus ni GUID ni _GUIDReferrer
        protected override object SaveViewState()
        {
            if (HttpContext.Current == null)
                return null;

            object[] ret = new object[3];
            ret[0] = base.SaveViewState();
            ret[1] = m_alDspActors;
            ret[2] = m_alDspBooks;
            return ret;
        }
        #endregion SaveViewState

        #region protected override GenerateHtmlForm
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected override void GenerateHtmlForm()
        {
            base.GenerateHtmlForm();

            AddButton();
            InitHeaderBanner(CellForm);
            m_TradeHeaderBanner.ResetIdentifierDisplaynameDescriptionExtlLink();
            CreateAndLoadPlaceHolder();
        }
        #endregion
        #region protected override PageConstruction
        // EG 20200902 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Corrections et compléments
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected override void PageConstruction()
        {
            string _titleLeft = Ressource.GetString2("SplitOfTrade", ((TradeCommonInput)m_Input).Identifier); ;
            string _titleRight = string.Empty;
            string _footerRight = string.Empty;

            HtmlPageTitle titleLeft = new HtmlPageTitle(_titleLeft);
            HtmlPageTitle titleRight = new HtmlPageTitle(_titleRight, null, _footerRight);
            string title = HtmlTools.HTMLBold_Remove(_titleLeft);
            GenerateHtmlForm();
            this.PageTitle = title;

            FormTools.AddBanniere(this, Form, titleLeft, titleRight, idMenu, m_MainMenuClassName);
            PageTools.BuildPage(this, Form, PageFullTitle, null, false, null);

        }
        #endregion
        #region private CreateAndLoadPlaceHolder
        // EG 20200914 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et compléments
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private void CreateAndLoadPlaceHolder()
		{
            Panel pnlBody = new Panel() { ID = "divbody", CssClass = CSSMode };
            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " " + m_MainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString("OTC_INP_TRD_SPLIT"));
            pnlBody.Controls.Add(togglePanel);
            CellForm.Controls.Add(pnlBody);


            PlaceHolder ph = new PlaceHolder() 
            {
                EnableViewState = false,
                ID = "phSplit"
            };

            #region Split
            Panel pnlDataGrid = new Panel() { ID = "divDtg", CssClass = CSSMode + " " + m_MainMenuClassName };
            pnlDataGrid.Controls.Add(new LiteralControl(Cst.HTMLBreakLine));
            Table tableSplit = new Table() 
            { 
                CssClass="DataGrid inherit",
                CellPadding = 5,
                CellSpacing = 0,
                Width = Unit.Percentage(100),
                BorderStyle = BorderStyle.Solid
            };
            pnlDataGrid.Controls.Add(tableSplit);

            #region HeaderRow
            TableRow tr = new TableRow
            {
                CssClass = "DataGrid_HeaderStyle"
            };
            tr.Cells.Add(TableTools.AddHeaderCell("", false, 5, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("lblStActivation", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("tradeHeader_party_side", true, 0, UnitEnum.Pixel, 0, true));
            tr.Cells.Add(TableTools.AddHeaderCell("IDA", true, 0, UnitEnum.Pixel, 2, true));
            tr.Cells.Add(TableTools.AddHeaderCell("IDB", true, 0, UnitEnum.Pixel, 2, true));
            tr.Cells.Add(TableTools.AddHeaderCell("strategyExchangeTradedDerivative_Header_Quantity", true, 0, UnitEnum.Pixel, 0, true));

            if (m_IsShowPosEfct)
                tr.Cells.Add(TableTools.AddHeaderCell("strategyExchangeTradedDerivative_Header_PosEfct", true, 0, UnitEnum.Pixel, 0, true));

            tableSplit.Controls.Add(tr);
            #endregion HeaderRow

            #region InputRows
            WCDropDownList2 ddlSplit;
            WCTextBox2 txtSplit;
            WCToolTipLinkButton btnSplit;
            Label lblSplit;
            HiddenField hidSplit;

            bool isExcludeForcedValue = false;
            ArrayList alForcedValue = new ArrayList
            {
                Cst.StatusActivation.REGULAR.ToString(),
                Cst.StatusActivation.LOCKED.ToString(),
                Cst.StatusActivation.MISSING.ToString()
            };

            Validator[] validatorsQty = new Validator[3];
            string label = Ressource.GetString("strategyExchangeTradedDerivative_Header_Quantity");
            #region RequireField
            validatorsQty[0] = new Validator(label + Cst.Space + "[" + Ressource.GetString("MissingData") + "]", false, true);
            #endregion RequireField
            #region Regular Expression
            validatorsQty[1] = new Validator(EFSRegex.TypeRegex.RegexPositiveInteger, label + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
            #endregion Regular Expression
            #region ValidationDataType
            validatorsQty[2] = new Validator(ValidationDataType.Integer, label + Cst.Space + "[" + Ressource.GetString("InvalidData") + "]", false, true);
            #endregion ValidationDataType

            iMax = 0;
            if (IsPostBack)
            {
                #region Recherche du nombre de ligne de split et contrôle de la qté totale affectée
                decimal assignedQty = 0;
                foreach (string param in Request.Params)
                {
                    if (param.StartsWith(Cst.TXT + "Qty"))
                    {
                        int suffix = IntFunc.IntValue(param.Substring(6));
                        iMax = Math.Max(iMax, suffix);

                        string data = Request.Form[param];
                        if (IntFunc.IsPositiveInteger(data))
                        {
                            assignedQty += IntFunc.IntValue64(Request.Form[param]);
                        }
                        else
                        {
                            //Error: 
                            assignedQty = m_InitialQty + 1;
                            break;
                        }
                    }
                }

                if (m_InitialQty > assignedQty)
                {
                    string lastRowQty = Request.Form[Cst.TXT + "Qty" + iMax.ToString()];
                    if (IntFunc.IsPositiveInteger(lastRowQty))
                    {
                        //Si la dernière ligne contient une quantité de saisie, on crée un ligne supplémentaire.   
                        iMax++;
                    }
                }
                #endregion 
            }

            for (int i = -1; i <= iMax; i++)
            {
                #region MainRow (Row with Input Data)
                string prefix = string.Empty;
                string suffix = i.ToString();
                bool isEnabled = true;
                if (i == -1)
                {
                    //Source row
                    prefix = "Source";
                    suffix = string.Empty;
                    isEnabled = false;

                    AddRowSeparator(tableSplit, Ressource.GetString("Source", true));
                }

                tr = new TableRow
                {
                    CssClass = "xDataGrid_ItemStyle"
                };

                //Row number
                TableCell td = new TableCell();
                if (prefix != "Source")
                    td.Text = (i + 1).ToString();
                tr.Controls.Add(td);

                //DDL Activation Status
                td = new TableCell();
                ddlSplit = new WCDropDownList2
                {
                    ID = Cst.DDL + prefix + "StActivation" + suffix,
                    Enabled = isEnabled
                };
                ControlsTools.DDLLoad_StatusActivation(SessionTools.CS, ddlSplit, false, alForcedValue, isExcludeForcedValue);
                ControlsTools.DDLSelectByValue(ddlSplit, m_StActivation);
                td.Controls.Add(ddlSplit);
                tr.Controls.Add(td);

                //Side
                td = new TableCell() 
                {
                    Text = Ressource.GetString((m_IsDealerBuyer ? "Buy" : "Sell"), true),
                    //CssClass = (m_IsDealerBuyer ? EFSCssClass.CssClassEnum.DataGrid_Buyer.ToString() : EFSCssClass.CssClassEnum.DataGrid_Seller.ToString())
                };
                td.Font.Bold = true;
                tr.Controls.Add(td);

                //TXT + HID Actor 
                txtSplit = new WCTextBox2(Cst.TXT + prefix + "Actor" + suffix, !isEnabled, true, false, EFSCssClass.Capture, null) 
                { 
                    Enabled = isEnabled,
                    Width = Unit.Percentage(100),
                    AutoPostBack = true,
                };
                // FI 20201021 [XXXXX] Add class or-autocomplete pour implémentation du autocomplete
                txtSplit.CssClass = "or-autocomplete " + txtSplit.CssClass;

                if ((prefix == "Source") || ((!IsPostBack) && (suffix=="0")))
                {
                    txtSplit.Text = m_DealerParty.PartyId;
                }
                hidSplit = new HiddenField() 
                {
                    ID = Cst.HID + prefix + "Actor" + suffix
                };
                
                if ((prefix == "Source") || ((!IsPostBack) && (suffix == "0")))
                    hidSplit.Value = m_DealerParty.OtcmlId;

                td = new TableCell();
                td.Controls.Add(txtSplit);
                td.Controls.Add(hidSplit);
                tr.Controls.Add(td);

                //BTN Actor 
                td = new TableCell();
                if (prefix != "Source")
                {
                    btnSplit = new WCToolTipLinkButton()
                    {
                        CssClass = "fa-icon",
                        ID = Cst.BUT + "Actor" + suffix,
                        Enabled = isEnabled,
                        CausesValidation = false,
                        TabIndex = -1,
                        Text = "<i class='fas fa-ellipsis-h'></i>"
                    };
                    btnSplit.Pty.TooltipContent = Ressource.GetString("SelectAValue");
                    ControlsTools.SetOnClientClick_ButtonReferential(btnSplit, "ACTOR", null, Cst.ListType.Repository, true,
                        null, txtSplit.ClientID, null, null, null, null, null, null);
                    td.Controls.Add(btnSplit);
                }
                tr.Controls.Add(td);

                //TXT + HID Book 
                td = new TableCell();
                txtSplit = new WCTextBox2(Cst.TXT + prefix + "Book" + suffix, !isEnabled, true, false, EFSCssClass.Capture, null) 
                {
                    Enabled = isEnabled,
                    Width = Unit.Percentage(100),
                    AutoPostBack = true
                };
                // FI 20201021 [XXXXX] Add class or-autocomplete pour implémentation du autocomplete
                txtSplit.CssClass = "or-autocomplete " + txtSplit.CssClass;

                if ((prefix == "Source") || ((!IsPostBack) && (suffix == "0")))
                {
                    txtSplit.Text = m_DealerBook.Value;
                }
                td.Controls.Add(txtSplit);
                tr.Controls.Add(td);
                hidSplit = new HiddenField() 
                {
                    ID = Cst.HID + prefix + "Book" + suffix
                };
                if ((prefix == "Source") || ((!IsPostBack) && (suffix == "0")))
                    hidSplit.Value = m_DealerBook.OtcmlId;

                td.Controls.Add(hidSplit);
                tr.Controls.Add(td);

                //BTN Book
                td = new TableCell();
                if (prefix != "Source")
                {
                    btnSplit = new WCToolTipLinkButton()
                    {
                        CssClass = "fa-icon",
                        ID = Cst.BUT + "Book" + suffix,
                        Enabled = isEnabled,
                        CausesValidation = false,
                        TabIndex = -1,
                        Text = "<i class='fas fa-ellipsis-h'></i>"
                    };
                    btnSplit.Pty.TooltipContent = Ressource.GetString("SelectAValue");
                    ControlsTools.SetOnClientClick_ButtonReferential(btnSplit, "BOOK", null, Cst.ListType.Repository, true,
                        null, txtSplit.ClientID, null, null, null, null, null, null);
                    td.Controls.Add(btnSplit);
                }
                tr.Controls.Add(td);

                //TXT Qty
                td = new TableCell() 
                {
                    Width = Unit.Pixel(100)
                };
                
                txtSplit = new WCTextBox2(Cst.TXT + prefix + "Qty" + suffix, !isEnabled, true, true, EFSCssClass.Capture, validatorsQty) 
                {
                    Enabled = isEnabled,
                    Width = Unit.Pixel(100),
                    AutoPostBack = true
                };
                if (prefix == "Source")
                    txtSplit.Text = FmtInitialQty;

                td.Controls.Add(txtSplit);
                tr.Controls.Add(td);

                //DDL PosEfct
                if (m_IsShowPosEfct)
                {
                    td = new TableCell();
                    ddlSplit = new WCDropDownList2() 
                    {
                        ID = Cst.DDL + prefix + "PosEfct" + suffix,
                        Enabled = isEnabled
                    };
                    ControlsTools.DDLLoad_PosEfct_OpenCloseOnly(SessionTools.CS, ddlSplit);
                    ControlsTools.DDLSelectByText(ddlSplit, m_PosEfct); //Warning: Ici on utilise DDLSelectByText()
                    td.Controls.Add(ddlSplit);
                    tr.Controls.Add(td);
                }
                
                tableSplit.Controls.Add(tr);
                #endregion MainRow (Row with Input Data)

                #region SecondRow (Row with Label/Display)
                tr = new TableRow();

                td = new TableCell();
                tr.Controls.Add(td);

                td = new TableCell() { ColumnSpan=2};
                td.Controls.Add(new LiteralControl(Cst.HTMLSpace));
                tr.Controls.Add(td);

                td = new TableCell() { ColumnSpan = 2 };
                lblSplit = new Label() 
                {
                    ID = Cst.DSP + prefix + "Actor" + suffix,
                    Enabled = isEnabled,
                    CssClass = "smaller"
                };
                if ((prefix == "Source") || ((!IsPostBack) && (suffix == "0")))
                    lblSplit.Text = m_DealerParty.PartyName;

                td.Controls.Add(lblSplit);
                tr.Controls.Add(td);

                td = new TableCell() { ColumnSpan = 4 };
                lblSplit = new Label() 
                {
                    ID = Cst.DSP + prefix + "Book" + suffix,
                    Enabled = isEnabled,
                    CssClass = "smaller"
                };
                if ((prefix == "Source") || ((!IsPostBack) && (suffix == "0")))
                    lblSplit.Text = m_DealerBook.BookName;
                td.Controls.Add(lblSplit);
                tr.Controls.Add(td);

                tableSplit.Controls.Add(tr);

                if (prefix == "Source")
                {
                    AddRowSeparator(tableSplit, Ressource.GetString("Split", true));
                }
                #endregion SecondRow (Row with Label/Display)
            }
            #endregion InputRows

            ph.Controls.Add(pnlDataGrid);
            #endregion

            togglePanel.AddContent(ph);
        }
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private void AddRowSeparator(Table pTable, string pTitle)
        {
            pTable.Controls.Add(ControlsTools.GetHRHtmlPage(CstCSSColor.orange, NUMBEROFCOLUMS, "3", pTitle, null));
        }
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        protected void InitHeaderBanner(WebControl pControlContainer)
        {
            m_TradeHeaderBanner = new TradeHeaderBanner(this, GUID, pControlContainer, m_Input, m_InputGUI, false);
            m_TradeHeaderBanner.AddControls();
        }
        #endregion

        #region private AddButton
        // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction et suppression de codes inutiles
        private void AddButton()
        {
            Panel pnl = new Panel() { ID = "divalltoolbar", CssClass = CSSMode + " " + m_MainMenuClassName };
            //Valider
            WCToolTipLinkButton btnOk = ControlsTools.GetAwesomeButtonAction("btnOk", "fa fa-caret-square-right", true);
            btnOk.Click += new EventHandler(this.OnProcess_Click);
            pnl.Controls.Add(btnOk);
            //Annuler
            WCToolTipLinkButton btnCancel = ControlsTools.GetAwesomeButtonCancel(false);
            pnl.Controls.Add(btnCancel);
            CellForm.Controls.Add(pnl);
        }
        #endregion
        #region private OnProcess_Click
        private void OnProcess_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                string errMsg = null;

                #region Contrôle Qté et Actors/Books
                //Contrôle de la somme des quantités affectées
                CaclAssignedQuantityAndCount();

                if (m_assignedCount == 1)
                {
                    errMsg = Ressource.GetString2("Msg_SplitTrade_OnlyOneRow");
                }
                else if (m_assignedQty > m_InitialQty)
                {
                    errMsg = Ressource.GetString2("Msg_SplitTrade_QtyHigherInitialQty", FmtAssignedQty);
                }
                else if (m_assignedQty < m_InitialQty)
                {
                    errMsg = Ressource.GetString2("Msg_SplitTrade_QtyLessInitialQty", FmtAssignedQty);
                }
                else
                {
                    //Contrôle des Actors/Books
                    if (!CheckActorsAndBooks(out string numberOfRow))
                    {
                        errMsg = Ressource.GetString2("Msg_SplitTrade_IncorrectActorBook", numberOfRow);
                    }
                }
                #endregion 

                if (StrFunc.IsEmpty(errMsg))
                {
                    //------------------------------------------------------------------
                    //Constitution et postage du message au service
                    //------------------------------------------------------------------
                    m_Input.InitializeForAction(SessionTools.CS, Cst.Capture.ModeEnum.TradeSplitting);

                    #region Add Split information
                    string posEfct = FixML.v50SP1.Enum.PositionEffectEnum.Close.ToString();
                    WCDropDownList2 ddlStActivation, ddlPosEfct;
                    HiddenField hidActor, hidBook;
                    WCTextBox2 txtActor, txtBook, txtQty;
                    for (int i = 0; i <= iMax; i++)
                    {
                        ddlStActivation = FindControl(Cst.DDL + "StActivation" + i.ToString()) as WCDropDownList2;
                        txtActor = FindControl(Cst.TXT + "Actor" + i.ToString()) as WCTextBox2;
                        hidActor = FindControl(Cst.HID + "Actor" + i.ToString()) as HiddenField;
                        txtBook = FindControl(Cst.TXT + "Book" + i.ToString()) as WCTextBox2;
                        hidBook = FindControl(Cst.HID + "Book" + i.ToString()) as HiddenField;
                        txtQty = FindControl(Cst.TXT + "Qty" + i.ToString()) as WCTextBox2;
                        if (m_IsShowPosEfct)
                        {
                            ddlPosEfct = FindControl(Cst.DDL + "PosEfct" + i.ToString()) as WCDropDownList2;
                            posEfct = ddlPosEfct.SelectedItem.Text;
                        }

                        if (IntFunc.IntValue(txtQty.Text) > 0)
                        {
                            // EG 20150920 [21314] Int (int32) to Long (Int64) 
                            // EG 20170127 Qty Long To Decimal
                            m_Input.split.AddNewTrade(ddlStActivation.SelectedValue,
                                                      txtActor.Text, Convert.ToInt32(hidActor.Value),
                                                      txtBook.Text, Convert.ToInt32(hidBook.Value),
                                                      DecFunc.DecValue(txtQty.Text),
                                                      posEfct);
                        }
                    }

                    //Note, non disponible actuellement sur l'IHM Split.aspx
                    if (FindControl(Cst.TXT + "Note") is WCTextBox2 txtNote)
                    {
                        m_Input.split.note = txtNote.Text;
                    }
                    #endregion Add Split information

                    IPosRequest posRequest = m_Input.NewPostRequest(SessionTools.CS, null, Cst.Capture.ModeEnum.TradeSplitting);
                    Cst.ErrLevelMessage errLevelMessage = PosKeepingTools.AddPosRequest(SessionTools.CS, null, posRequest, SessionTools.AppSession, null, null);

                    if (errLevelMessage.ErrLevel == Cst.ErrLevel.SUCCESS)
                    {
                        ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.SUCCESS; 
                        MsgForAlertImmediate = errLevelMessage.Message;
                        CloseAfterAlertImmediate = true;

                        SetRequestTrackBuilderItemProcess();
                    }
                    else
                    {
                        errMsg = errLevelMessage.Message;
                    }
                }

                if (StrFunc.IsFilled(errMsg))
                {
                    ErrLevelForAlertImmediate = ProcessStateTools.StatusEnum.ERROR;
                    MsgForAlertImmediate = errMsg;
                }
            }
        }
        #endregion

        /// <summary>
        /// Calcul de la qté totale affectée
        /// </summary>
        /// <returns></returns>
        // EG 20170127 Qty Long To Decimal
        private void CaclAssignedQuantityAndCount()
        {
            WCTextBox2 txt;
            m_assignedQty = 0;
            m_assignedCount = 0;

            for (int i = 0; i <= iMax; i++)
            {
                txt = FindControl(Cst.TXT + "Qty" + i.ToString()) as WCTextBox2;
                // EG 20170127 Qty Long To Decimal
                //if (IntFunc.IsPositiveInteger(txt.Text))
                if (0 < DecFunc.DecValue(txt.Text))
                {
                    // EG 20150920 [21314] Int (int32) to Long (Int64) 
                    //m_assignedQty += DecFunc.DecValue(txt.Text);
                    m_assignedQty += Convert.ToInt64(txt.Text);
                    m_assignedCount++;
                }
            }
        }
        /// <summary>
        /// Contrôle la présence de coupel Actor/book valide
        /// </summary>
        /// <param name="pNumberOfRow"></param>
        /// <returns></returns>
        private bool CheckActorsAndBooks(out string pNumberOfRow)
        {
            WCTextBox2 txt;
            HiddenField hidActor;
            HiddenField hidBook;
            string numberOfRow = string.Empty;

            for (int i = 0; i <= iMax; i++)
            {
                txt = FindControl(Cst.TXT + "Qty" + i.ToString()) as WCTextBox2;
                if (IntFunc.IsPositiveInteger(txt.Text))
                {
                    hidActor = FindControl(Cst.HID + "Actor" + i.ToString()) as HiddenField;
                    hidBook = FindControl(Cst.HID + "Book" + i.ToString()) as HiddenField;
                    if ((!IntFunc.IsPositiveInteger(hidActor.Value)) || (!IntFunc.IsPositiveInteger(hidBook.Value)))
                    {
                        if (StrFunc.IsFilled(numberOfRow))
                        {       
                            numberOfRow += ",";
                        }
                        numberOfRow += (i+1).ToString();
                    }
                }
            }

            pNumberOfRow = numberOfRow;
            return StrFunc.IsEmpty(numberOfRow);
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si consultation d'un trade
        /// </summary>
        /// FI 20141021 [20350] Add Method
        private void SetRequestTrackBuilderItemProcess()
        {
            Boolean isTrack = SessionTools.IsRequestTrackProcessEnabled;
            if (isTrack)
            {
                RequestTrackTradeBuilder builder = new RequestTrackTradeBuilder
                {
                    DataDocument = ((TradeInput)m_Input).DataDocument,
                    TradeIdentIdentification = ((TradeInput)m_Input).Identification,
                    ProcessType = RequestTrackProcessEnum.Split,
                    action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual)
                };
                this.RequestTrackBuilder = builder;
            }
        }

    }
}