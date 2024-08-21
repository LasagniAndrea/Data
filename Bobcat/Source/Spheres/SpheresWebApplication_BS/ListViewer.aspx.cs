#region using directives
using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Controls;
using EFS.GridViewProcessor;
using EFS.ListControl;
using EFS.SpheresService;
using EFS.TradeInformation;
using EfsML.Business;
using EfsML.DynamicData;
using EfsML.Enum;
using EfsML.Enum.Tools;
using EfsML.Interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Serialization;
#endregion using directives

namespace EFS.Spheres
{
    public partial class ListViewer : ListBase
    {

        const string Target_OnProcessClick = "OnProcessClick";

        /// <summary>
        ///  Représente les exports Excel
        /// </summary>
        private enum ExportExcelType
        {
            /// <summary>
            /// Exportation Excel du grid
            /// </summary>
            ExportGrid,
            /// <summary>
            /// Exportation Excel du jeu de donné
            /// </summary>
            ExportDataset,
        }

        /// <summary>
        /// Représente les valeurs de retour de la méthode ProcessRemoveAlloc
        /// </summary>
        private enum ProcessRemoveAllocReturn
        {
            /// <summary>
            /// Le trade a été traité correctement
            /// </summary>
            Ok,
            /// <summary>
            /// Le trade n'a pas été traité, il est reservé
            /// </summary>
            Reserved,
            /// <summary>
            /// Le trade n'a pas été traité, il est déjà supprimé
            /// </summary>
            AlreadyRemoved,
            /// <summary>
            /// Le trade n'a pas été traité, il ne possède pas d'évènements
            /// </summary>
            NoEvent,
        }

        #region Members
        /// <summary>
        /// Liste des erreurs inattendues rencontrées (lorsque on est en mode sans Echec)
        /// </summary>
        private List<SpheresException> _alException;
        /// <summary>
        /// Liste des messages d'erreurs affiché sur le render
        /// </summary>
        private ArrayList _alErrorMessages;
        /// <summary>
        ///  liste des dynamicArgument 
        /// </summary>
        private Dictionary<string, StringDynamicData> _dynamicArgs;
        /// <summary>
        ///  Obtient ou définit les _customObjects associés 
        /// </summary>
        private CustomObjects _customObjects;
        /// <summary>
        ///  Obtient ou définit les filtres 
        /// </summary>
        private LstWhereData[] where;
        /// <summary>
        ///  Obtient ou définit les tris et regroupements 
        /// </summary>
        private ArrayList[] orderBy;

        protected string mainTitle;
        protected string subTitle;

        private bool m_IsProcess;
        private bool m_IsSpheresProcess;
        private string m_ProcessBase;
        private string m_ProcessType;
        private string m_ProcessName;
        private int m_IDIOTask;

        private object m_toolbar_Clicked;
        private Button m_btnRunProcess_Clicked;

        #endregion Members

        #region accessors
        #region GetTotalRows
        /// <summary>
        /// Retourne le nombre de lignes de filtres (en comptant le nombre de contrôle de type BS_GroupColumn)
        /// </summary>
        /// <returns>higher ID for rows</returns>
        public override int GetTotalRows
        {
            get
            {
                return uc_lstfilter.GetTotalRows;
            }
        }
        #endregion GetTotalRows

        /// <summary>
        /// Obtient le process courant 
        /// <para>Obtient Cst.ProcessTypeEnum.NA si la page n'est pas en mode process</para>
        /// </summary>
        protected Cst.ProcessTypeEnum SpheresProcessType
        {
            get
            {
                Cst.ProcessTypeEnum ret = Cst.ProcessTypeEnum.NA;
                if (m_IsSpheresProcess)
                    ret = (Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), m_ProcessName.ToUpper(), true);
                return ret;
            }
        }

        /// <summary>
        /// Obtient ou définit le mode sans Echec
        /// </summary>
        public bool isNoFailure
        {
            get;
            set;
        }

        /// <summary>
        /// Obtient les dynamics arguments 
        /// </summary>
        protected Dictionary<string, StringDynamicData> dynamicArgs
        {
            get
            {
                return _dynamicArgs;
            }
        }

        /// <summary>
        /// Obtient les customObjects, les customObjects sont déclarés ds le fichier GUI.xm
        /// </summary>
        protected CustomObjects customObjects
        {
            get
            {
                return _customObjects;
            }
        }

        public HtmlButton BtnConfirmSubmit
        {
            get { return btnConfirmSubmit; }
        }
        #endregion accessors

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        // EG 20120327 Classe Utilisée dans le calcul des frais manquants (Ticket 17706)
        protected override void OnInit(EventArgs e)
        {
            AddAuditTimeStep("Start List.OnInit");
            try
            {
                base.OnInit(e);

                _alException = new List<SpheresException>();
                _alErrorMessages = new ArrayList();

                AbortRessource = true;
                isNoFailure = true;

                InitializeComponent();

                //SetFormClass("form-inline");

                #region IsProcess
                m_IsProcess = false;
                m_ProcessBase = Request.QueryString["ProcessBase"];
                m_ProcessType = Request.QueryString["ProcessType"];
                m_ProcessName = Request.QueryString["ProcessName"];

                if (StrFunc.IsFilled(m_ProcessType) && StrFunc.IsFilled(m_ProcessName))
                {
                    m_IsProcess = Cst.ListProcess.IsListProcess(m_ProcessType);
                    if (m_IsProcess)
                    {
                        m_IsSpheresProcess = Cst.ListProcess.IsService(m_ProcessType);
                        if (m_IsSpheresProcess && (Cst.ProcessTypeEnum.NA == SpheresProcessType))
                        {
                            m_IsProcess = false;
                            m_IsSpheresProcess = false;
                        }
                    }
                }
                #endregion IsProcess

                #region IOTASK
                m_IDIOTask = Convert.ToInt32(Request.QueryString["IOTask"]);
                #endregion IOTASK

                InitializeGridView();
                InitalizeCustomObjectAndDynamicArg();
                gvTemplate.LoadReferential(dynamicArgs);

                if (gvTemplate.consult.template.IsRefreshIntervalSpecified)
                    AutoRefresh = gvTemplate.consult.template.REFRESHINTERVAL;

                SetKeyAndLoadTemplate();

                InitChkValidityAndChkTodayUpdate();

                if (isXMLSource)
                    SaveInSession(gvTemplate.referential);
            }
            catch (Exception) { throw; } //TrapException(ex); }

            AddAuditTimeStep("End List.OnInit");
        }

        private void InitializeGridView()
        {
            gvTemplate.LoadDataError += new GridViewDataErrorEventHandler(OnLoadDataError);
            gvTemplate.isNoFailure = isNoFailure;
            gvTemplate.FooterContainer = lstFooter;
            gvTemplate.IsCheckboxColumn = m_IsProcess;

            if (m_IsProcess)
            {
                //Cas particuliers pour INVOICINGGEN et FEESCALCULATION
                if (m_IsSpheresProcess &&
                    (
                        (Cst.ProcessTypeEnum.INVOICINGGEN == SpheresProcessType) ||
                        (Cst.ProcessTypeEnum.ACTIONGEN == SpheresProcessType) ||
                        (Cst.ProcessTypeEnum.FEESCALCULATION == SpheresProcessType)
                    )
                )
                    gvTemplate.IsCheckboxColumn = false;

                //Si m_IsProcess: Pas de pagination personalisée, mais chargement complet du jeu de résulat.
                //NB: Ceci est nécessaire pour que le post des messages s'applique à toutes les lignes du jeu de résultat.
                gvTemplate.pagingType = PagingTypeEnum.NativePaging;
            }

            if (this.__EVENTTARGET == "btnFilterReadOnlyTitle")
                gvTemplate.isApplyOptionalFilter = false;
            // EG 20160308 Migration vs2013
            if (StrFunc.IsFilled(this.__EVENTTARGET) && this.__EVENTTARGET.StartsWith("imgDisabledFilterPosition"))
                gvTemplate.positionFilterDisabled = StrFunc.GetSuffixNumeric2(this.__EVENTTARGET);

        }
        private void InitalizeCustomObjectAndDynamicArg()
        {
            // Chargement des Custom objects
            LoadCustomObjects();

            // Création du panel des Custom objects
            Panel pnlProcessParameters = null;
            if (null != customObjects)
            {
                pnlProcessParameters = customObjects.CreatePanel(this);
            }
            if (null != pnlProcessParameters)
                plhCustomObject.Controls.Add(pnlProcessParameters);

            // Initialisation des Custom objects
            if ((false == IsPostBack) && (null != customObjects))
            {
                for (int i = 0; i < ArrFunc.Count(customObjects.customObject); i++)
                {
                    InitControlCustomObject(customObjects.customObject[i]);
                }
            }

            // Chargement des Dynamic arguments
            LoadDynamicArg();

            SetCustomControlURLDA();
        }
        private void InitalizeLstInfo()
        {
            bool isLstCustomObjects = (null != customObjects);
            bool isLstFilterReadOnly = ArrFunc.IsFilled(where) || ArrFunc.IsFilled(where);
            lstCustomObjects.Visible = isLstCustomObjects;
            lstFilterReadOnly.Visible = isLstFilterReadOnly;
        }


        /// <summary>
        /// Retourne true si le bouton IO est présent
        /// </summary>
        private bool IsDisplayButtonIO()
        {
            bool ret = (m_IDIOTask > 0);

            if (m_IsSpheresProcess)
            {
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = true;
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Retourne true si le bouton RunProcessAndIO est présent
        /// </summary>
        private bool IsDisplayButtonProcessAndIO()
        {
            bool ret = false;

            if (m_IsSpheresProcess)
            {
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RIMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = true;
                        break;
                }
            }
            return ret;
        }


        /// <summary>
        /// Controle de la validité d'une plage de dates (DATE1 et DATE2)
        /// Si DATE1 est modifié et DATE1 > DATE2 alors DATE2 = DATE1
        /// Si DATE2 est modifié et DATE2 > DATE1 alors DATE1 = DATE2
        /// </summary>
        // EG 20130925 New
        // PL 20150511 Manage DTBUSINESS just like DATE1
        private void ControlBetweenDates()
        {
            bool isExistPERIODREPORTTYPE = dynamicArgs.ContainsKey("PERIODREPORTTYPE");
            DropDownList ddlPeriod = null;
            TextBox txtDate1 = null;
            TextBox txtDate2 = null;

            if (isExistPERIODREPORTTYPE)
            {
                ddlPeriod = plhCustomObject.FindControl(Cst.DDL + "PERIODREPORTTYPE") as WCDropDownList2;

                if (__EVENTTARGET == Cst.DDL + "PERIODREPORTTYPE")
                {
                    DateTime firstDateDefaultValue = DateTime.MinValue;
                    DateTime secondDateValue = DateTime.MinValue;

                    switch (ddlPeriod.SelectedValue)
                    {
                        case "2": //Weekly
                            firstDateDefaultValue = DateTime.Today.AddDays(1 - (int)DateTime.Today.DayOfWeek).AddDays(-7);
                            secondDateValue = firstDateDefaultValue.AddDays(6);
                            break;
                        case "3": //Monthly
                            firstDateDefaultValue = DateTime.Today.AddDays(1 - DateTime.Today.Day).AddMonths(-1);
                            secondDateValue = firstDateDefaultValue.AddMonths(1).AddDays(-1);
                            break;
                        case "4": //Yearly
                            firstDateDefaultValue = DateTime.Today.AddDays(1 - DateTime.Today.DayOfYear).AddYears(-1);
                            secondDateValue = firstDateDefaultValue.AddYears(1).AddDays(-1);
                            break;
                    }

                    if (dynamicArgs.ContainsKey("DATE1"))
                        txtDate1 = plhCustomObject.FindControl(Cst.TXT + "DATE1") as BS_TextBox;
                    else if (dynamicArgs.ContainsKey("DTBUSINESS"))
                        txtDate1 = plhCustomObject.FindControl(Cst.TXT + "DTBUSINESS") as BS_TextBox;

                    if (txtDate1 != null)
                    {
                        if (firstDateDefaultValue != DateTime.MinValue)
                            txtDate1.Text = DtFunc.DateTimeToString(firstDateDefaultValue, DtFunc.FmtShortDate);
                    }

                    txtDate2 = plhCustomObject.FindControl(Cst.TXT + "DATE2") as BS_TextBox;
                    if (txtDate2 != null)
                    {
                        //FI 20150512 Usage de ReadOnly
                        txtDate2.ReadOnly = (ddlPeriod.SelectedValue != "5"); //Other

                        if (ddlPeriod.SelectedValue == "1") //Daily
                            txtDate2.Text = txtDate1.Text;
                        else if (secondDateValue != DateTime.MinValue)
                            txtDate2.Text = DtFunc.DateTimeToString(secondDateValue, DtFunc.FmtShortDate);
                    }
                }
            }

            if (IsPostBack)
            {
                bool isDate1 = false;
                bool isDate2Disabled = false;
                bool isToDo = (null != __EVENTTARGET) && (__EVENTTARGET != Cst.DDL + "PERIODREPORTTYPE");

                if (isToDo)
                {
                    if (dynamicArgs.ContainsKey("DATE1") && dynamicArgs.ContainsKey("DATE2"))
                        isDate1 = true;
                    else if ((!dynamicArgs.ContainsKey("DATE1")) && dynamicArgs.ContainsKey("DTBUSINESS") && dynamicArgs.ContainsKey("DATE2"))
                        isDate1 = false;
                    else
                        isToDo = false;

                    if (isToDo)
                    {
                        TextBox txtDate = plhCustomObject.FindControl(Cst.TXT + "DATE2") as BS_TextBox;
                        isDate2Disabled = txtDate.ReadOnly;
                    }
                }

                if (isToDo)
                {
                    string firstDate = isDate1 ? "DATE1" : "DTBUSINESS";

                    DateTime date1 = Convert.ToDateTime(dynamicArgs[firstDate].value);
                    DateTime date2 = Convert.ToDateTime(dynamicArgs["DATE2"].value);

                    if (isDate2Disabled && isExistPERIODREPORTTYPE)
                    {
                        txtDate2 = plhCustomObject.FindControl(Cst.TXT + "DATE2") as BS_TextBox;

                        switch (ddlPeriod.SelectedValue)
                        {
                            case "1": //Daily
                                txtDate2.Text = DtFunc.DateTimeToString(date1, DtFunc.FmtShortDate);
                                break;
                            case "2": //Weekly
                                txtDate2.Text = DtFunc.DateTimeToString(date1.AddDays(7).AddDays(-1), DtFunc.FmtShortDate);
                                break;
                            case "3": //Monthly
                                txtDate2.Text = DtFunc.DateTimeToString(date1.AddMonths(1).AddDays(-1), DtFunc.FmtShortDate);
                                break;
                            case "4": //Yearly
                                txtDate2.Text = DtFunc.DateTimeToString(date1.AddYears(1).AddDays(-1), DtFunc.FmtShortDate);
                                break;
                        }
                    }
                    else if ((__EVENTTARGET.ToUpper() == "TXT" + firstDate) && (-1 < date1.CompareTo(date2)))
                    {
                        txtDate2 = plhCustomObject.FindControl(Cst.TXT + "DATE2") as BS_TextBox;
                        dynamicArgs["DATE2"].value = dynamicArgs[firstDate].value;
                        txtDate2.Text = new DtFunc().GetDateTimeString(dynamicArgs["DATE2"].value, DtFunc.FmtShortDate);
                    }
                    else if ((__EVENTTARGET.ToUpper() == "TXTDATE2") && (-1 < date1.CompareTo(date2)))
                    {
                        txtDate1 = plhCustomObject.FindControl(Cst.TXT + firstDate) as BS_TextBox;
                        dynamicArgs[firstDate].value = dynamicArgs["DATE2"].value;
                        txtDate1.Text = new DtFunc().GetDateTimeString(dynamicArgs[firstDate].value, DtFunc.FmtShortDate);
                    }
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            AddAuditTimeStep("Start ListViewer.OnPreRender");

            try
            {
                // Chargement des colonnes d'affichages, critères de filtre, tri et regroupement (EDIT)
                //LoadLstCriteria();
                this.DataBind();

                //btnRefresh.Enabled = (false == gvTemplate.isDataModified) && (false == gvTemplate.isLocked);
                btnCancel.Enabled = (true == gvTemplate.isDataModified) && (false == gvTemplate.isLocked);
                btnRecord.Enabled = (true == gvTemplate.isDataModified) && (false == gvTemplate.isLocked);
                btnAddNew.Enabled = (false == gvTemplate.isLocked);

                InitalizeLstInfo();


                //Si des modifications ont été effectués sur le grid; on disable [Critères] et [Rafraîchir] et enable [Enregistrer] et [Annuler]

                if (IsMonoProcessGenerate(SpheresProcessType))
                {
                    TextBox txtSearch = FindControl("txtSearch") as TextBox;
                    if (null != txtSearch)
                    {
                        if (txtSearch.Text.Length > 0)
                            btnRunProcess.Style.Add("opacity", "0.4");
                        else
                            btnRunProcess.Style.Remove("opacity");

                        btnRunProcess.Enabled = (txtSearch.Text.Length == 0);
                    }
                }

                base.OnPreRender(e);
            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End ListViewer.OnPreRender");
        }

        protected override void OnPreRenderComplete(EventArgs e)
        {
            AddAuditTimeStep("Start ListViewer.OnPreRenderComplete");

            try
            {
                // TO DO
                if (isTrace)
                {
                    DisplayPreSelectCommand();
                    DisplayLastQuery();
                }

                if (null != m_toolbar_Clicked)
                {
                    if (m_toolbar_Clicked is Control)
                    {
                        switch ((m_toolbar_Clicked as Control).ID)
                        {
                            case "btnSql":
                                ExportSQL();
                                break;
                            case "btnZip":
                                ExportZIP();
                                break;
                            case "btnMSExcel":
                                ExportExcel(ExportExcelType.ExportGrid);
                                break;
                        }
                    }
                    // EG 20130613 Menu Export Changement de type : skmMenu.MenuItemClickEventArgs
                    else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs)
                    {
                        skmMenu.MenuItemClickEventArgs mnu = (skmMenu.MenuItemClickEventArgs)m_toolbar_Clicked;
                        if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                            ExportPdf(mnu.Argument);
                    }
                }

                if (null != m_btnRunProcess_Clicked)
                {
                    //Poste d'une confirmation en cas de demande de traitement
                    CheckAndConfirmRequest();
                }

                if (Target_OnProcessClick == __EVENTTARGET)
                {
                    if (StrFunc.IsFilled(__EVENTARGUMENT))
                    {
                        string[] commandArguments = __EVENTARGUMENT.Split(';');
                        if (ArrFunc.Count(commandArguments) > 1)
                        {
                            if (Enum.IsDefined(typeof(Cst.ListProcess.ListProcessTypeEnum), commandArguments[0]))
                            {
                                Cst.ListProcess.ListProcessTypeEnum category =
                                    (Cst.ListProcess.ListProcessTypeEnum)Enum.Parse(typeof(Cst.ListProcess.ListProcessTypeEnum), commandArguments[0]);

                                Nullable<Cst.ProcessIOType> processIOType = null;
                                if (Cst.ListProcess.IsService(category) && Enum.IsDefined(typeof(Cst.ProcessIOType), commandArguments[1]))
                                {
                                    processIOType = (Cst.ProcessIOType)Enum.Parse(typeof(Cst.ProcessIOType), commandArguments[1]);
                                    commandArguments[1] = SpheresProcessType.ToString();
                                }

                                string additionalArguments = string.Empty;
                                if ((4 == commandArguments.Length) && ("Args" == commandArguments[2]))
                                    additionalArguments = commandArguments[3];
                                CallProcess(category, commandArguments[1], processIOType, additionalArguments);
                                // FI 20141021 [20350] call SetRequestTrackBuilderListProcess
                                SetRequestTrackBuilderListProcess();
                            }
                        }
                    }
                }
                else if (false == m_IsProcess)
                {
                    // FI 20141021 [20350] call SetRequestTrackBuilderListLoad
                    SetRequestTrackBuilderListLoad();
                }

                base.OnPreRenderComplete(e);
            }
            catch (Exception ex) { TrapException(ex); }

            AddAuditTimeStep("End ListViewer.OnPreRenderComplete");
        }

        public override void VerifyRenderingInServerForm(Control control)
        {
            /* Verifies that the control is rendered */
        }
        protected override void Render(HtmlTextWriter writer)
        {
            try
            {
                AddAuditTimeStep("Start ListViewer.Render");

                try
                {
                    bool isEmptyDtg = ((false == gvTemplate.isDataAvailable) || (gvTemplate.totalRowscount == 0));

                    //lstFooter.Visible = (false == isEmptyDtg);
                    //gvPager.Visible = (false == isEmptyDtg);
                    //pagerContent.Visible = (false == isEmptyDtg);

                    #region bouton zip visible oui/non
                    bool isZipVisible = false;
                    if (gvTemplate.isDataSourceAvailable)
                    {
                        DataTable dt = (gvTemplate.DsData.Tables[0]);
                        isZipVisible = (Cst.OTCml_TBL.VW_MCO.ToString() == dt.TableName);
                        isZipVisible |= (Cst.OTCml_TBL.VW_INVMCO.ToString() == dt.TableName);
                        isZipVisible |= (Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString() == dt.TableName);
                        isZipVisible |= (Cst.OTCml_TBL.ATTACHEDDOC.ToString() == dt.TableName);
                        isZipVisible |= (("MCO" == dt.TableName) && isConsultation);
                    }
                    btnZip.Visible = isZipVisible;
                    #endregion

                    //bool isContainsOptionalFilter = false;
                    //for (int i = 0; i < ArrFunc.Count(where); i++)
                    //{
                    //    isContainsOptionalFilter = (false == where[i].isMandatory);
                    //    if (isContainsOptionalFilter)
                    //        break;
                    //}

                    //btnFilterReadOnlyTitle.Visible = isContainsOptionalFilter;
                    //if (isContainsOptionalFilter)
                    //Page.ClientScript.RegisterForEventValidation(btnFilterReadOnlyTitle.UniqueID, "SELFRELOAD_");

                }
                catch (Exception ex) { TrapException(ex); }

                DisplayInfo();



                AddAuditTimeStep("End ListViewer.Render");

                base.Render(writer);
            }
            catch (Exception ex) { TrapException(ex); }
        }

        protected override void OnLoadComplete(EventArgs e)
        {
            base.OnLoadComplete(e);

            if (IsReloadCriteria)
            {
                gvTemplate.ViewStateMode = System.Web.UI.ViewStateMode.Disabled;
                gvTemplate.Columns.Clear();
                gvTemplate.DsData = null;
                gvTemplate.isBindData = true;
                gvTemplate.DataBind();
                gvTemplate.ResetColumns();
                gvTemplate.LoadReferential(dynamicArgs);
            }
            LoadLstCriteria();
            LoadFilterReadOnly();
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            gvTemplate.Data_CacheName = ((PageBase)this.Page).GUID;
            Loading();
            PageTitle = mainTitle;

            string mainMenu = ControlsTools.MainMenuName(gvTemplate.IDMenu);
            viewertitle.Attributes.Add("data-menu", mainMenu);
            lblMnuTitle.Text = mainTitle;
            lblMnuSubTitle.Text = subTitle;
        }

        private void InitializeComponent()
        {
            btnRunProcess.Command += new CommandEventHandler(this.OnProcess_Click);
            btnRunProcessAndIO.Command += new CommandEventHandler(this.OnProcess_Click);
            btnRunProcessIO.Command += new CommandEventHandler(this.OnProcess_Click);

            btnRefresh.Attributes.Add("onclick", ClientScript.GetPostBackEventReference(this, "SELFRELOAD_"));
            txtGoToPage.TextChanged += new System.EventHandler(OnGoToPageChanged);
            txtRowsPerPage.Text = string.Empty;
            txtRowsPerPage.TextChanged += new System.EventHandler(OnRowsPerPageChanged);
            txtRowsPerPage.Text = SessionTools.NumberRowByPage.ToString();
        }

        private void OnGoToPageChanged(object sender, EventArgs e)
        {
            gvTemplate.SetPageIndex(Math.Max(0, Convert.ToInt32(txtGoToPage.Text) - 1));
        }

        private void OnRowsPerPageChanged(object sender, EventArgs e)
        {
            gvTemplate.PageSize = Convert.ToInt32(txtRowsPerPage.Text);
        }

        /// <summary>
        /// Initialise les control GUI en fonction des CustomObject
        /// <para>Prise en compte des defaultValues par exemple </para>
        /// </summary>
        /// <param name="pCo"></param>
        private void InitControlCustomObject(CustomObject pCustomObject)
        {
            Control ctrl = ControlsTools.FindControlRecursive(plhCustomObject, pCustomObject.CtrlClientId);
            if (null != ctrl)
            {
                if (TypeData.IsTypeDate(pCustomObject.DataType))
                {
                    BS_TextBox txtBox = ctrl as BS_TextBox;
                    if (null != txtBox)
                    {
                        string defaultValue = (pCustomObject.IsMandatory ? DtFunc.TODAY : string.Empty);
                        if (pCustomObject.ContainsDefaultValue)
                            defaultValue = pCustomObject.DefaultValue;

                        string defaultBusinessCenter = SystemSettings.GetAppSettings("Spheres_ReferentialDefault_businesscenter");
                        DtFuncML dtFuncML = new DtFuncML(SessionTools.CS, defaultBusinessCenter, SessionTools.User.entity_IdA, 0, 0, null);
                        defaultValue = dtFuncML.GetDateTimeString(defaultValue, DtFunc.FmtShortDate);
                        txtBox.Text = new DtFunc().GetDateTimeString(defaultValue, DtFunc.FmtShortDate);
                    }
                }
                else if (TypeData.IsTypeInt(pCustomObject.DataType))
                {
                    if (pCustomObject.ContainsDefaultValue)
                    {
                        BS_DropDownList ddl = ctrl as BS_DropDownList;
                        if (null != ddl)
                        {
                            ControlsTools.DDLSelectByValue(ddl, pCustomObject.DefaultValue);
                        }
                        else
                        {
                            BS_TextBox txtBox = ctrl as BS_TextBox;
                            if (null != txtBox)
                                txtBox.Text = pCustomObject.DefaultValue;
                        }
                    }
                    else if (pCustomObject.ClientId == "ENTITY" && SessionTools.User.entity_IdA > 0)
                    {
                        BS_DropDownList ddl = ctrl as BS_DropDownList;
                        if (null != ddl)
                            ControlsTools.DDLSelectByValue(ddl, SessionTools.User.entity_IdA.ToString());
                    }
                }
                else if (TypeData.IsTypeBool(pCustomObject.DataType))
                {
                    if (pCustomObject.ContainsDefaultValue)
                    {
                        BS_ClassicCheckBox classicChk = ctrl as BS_ClassicCheckBox;
                        if (null != classicChk)
                        {
                            classicChk.chk.Checked = BoolFunc.IsTrue(pCustomObject.DefaultValue);
                        }
                        else
                        {
                            BS_ContentCheckBox contentChk = ctrl as BS_ContentCheckBox;
                            if (null != contentChk)
                                (contentChk.Controls[0] as BS_CheckBox).Checked = BoolFunc.IsTrue(pCustomObject.DefaultValue);
                        }
                    }
                }
                else if (TypeData.IsTypeDec(pCustomObject.DataType))
                {
                    if (pCustomObject.ContainsDefaultValue)
                    {
                        BS_TextBox txtBox = ctrl as BS_TextBox;
                        if (null != txtBox)
                            txtBox.Text = StrFunc.FmtDecimalToCurrentCulture(DecFunc.DecValueFromInvariantCulture(pCustomObject.DefaultValue));
                    }
                }
                else
                {
                    if (pCustomObject.ContainsDefaultValue)
                    {
                        //PL 20151208 Manage DefaultValue on String value DDL
                        if (pCustomObject.CtrlClientId.StartsWith(Cst.DDL))
                        {
                            BS_DropDownList ddl = ctrl as BS_DropDownList;
                            if (null != ddl)
                            {

                                if ((!string.IsNullOrEmpty(Request.QueryString["P1"])) && (Request.QueryString["P1"].StartsWith("ABN")))
                                {
                                    //PL 20151208 Temporaire! - Codage en dur pour ABN BULK
                                    pCustomObject.DefaultValue = pCustomObject.DefaultValue.Replace("ITM", "NTM");
                                }
                                ControlsTools.DDLSelectByValue(ddl, pCustomObject.DefaultValue);
                            }
                        }
                        else
                        {
                            BS_TextBox txtBox = ctrl as BS_TextBox;
                            if (null != txtBox)
                                txtBox.Text = pCustomObject.DefaultValue;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Charge _customObjects à partir du fichier desriptif xxxxxxxxxx.GUI.xml
        /// </summary>
        /// <returns></returns>
        private void LoadCustomObjects()
        {
            StreamReader streamReader = null;
            try
            {
                _customObjects = null;
                //
                List<string> objectNameAvailable =
                    RepositoryTools.GetObjectNameForDeserialize(gvTemplate.IDMenu, gvTemplate.ObjectName);
                //
                Nullable<Cst.ListType> objectType = null;
                if (gvTemplate.isConsultation)
                {
                    //FI 20120215 Ds le cas des consultation Le fichier GUI est  présent sous le répertoire  consultation
                    objectType = Cst.ListType.Consultation;
                }
                else
                    objectType = (Cst.ListType)Enum.Parse(typeof(Cst.ListType), gvTemplate.Title);
                //
                if (null == objectType)
                    throw new NullReferenceException("objectType is null");
                //
                bool isFind = false;
                string xmlFile = string.Empty;

                // FI 20121002 [18161] usage de la méthode ReferentialTools.GetObjectXMLFile en priorité
                // Cela permet de récupérer les fichiers éventuellement présents dans FILECONFIG
                for (int i = 0; i < ArrFunc.Count(objectNameAvailable); i++)
                {
                    xmlFile = RepositoryTools.GetObjectXMLFile(objectType.Value, objectNameAvailable[i]);
                    if (StrFunc.IsFilled(xmlFile))
                    {
                        xmlFile = Path.ChangeExtension(xmlFile, null) + ".GUI.xml";
                        //isFind = File.Exists(xmlFile); //PL 20170131 Set comment and use below SearchFile()
                    }
                    else //PL 20170131 New - (utile aux consultations LST pour lesquelles il n'existe pas de fichier XML physique) 
                    {
                        xmlFile = StrFunc.AppendFormat(@"~\PDIML\{0}\{1}.GUI.xml", objectType.Value.ToString(), objectNameAvailable[i]);
                    }
                    isFind = SessionTools.NewAppInstance().SearchFile2(SessionTools.CS, xmlFile, ref xmlFile);

                    if (isFind)
                        break;
                }

                // EG 20151019 [21465] New le fichier GUI associé au fichier XML est spécifié dans l'URL (GUIName)
                if ((false == isFind) && (1 == objectNameAvailable.Count) && StrFunc.IsFilled(gvTemplate.GUIName))
                {
                    xmlFile = RepositoryTools.GetObjectXMLFile(objectType.Value, gvTemplate.GUIName + ".GUI");
                    isFind = StrFunc.IsFilled(xmlFile) && File.Exists(xmlFile);
                }

                if (isFind)
                {
                    streamReader = new StreamReader(xmlFile);
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(CustomObjects));
                    _customObjects = (CustomObjects)xmlSerializer.Deserialize(streamReader);
                }
            }
            finally
            {
                if (null != streamReader)
                    streamReader.Close();
            }
        }

        /// <summary>
        /// Alimente _dynamicArgs à partir des customObjects et des DA présents dans l'URL
        /// </summary>
        private void LoadDynamicArg()
        {
            _dynamicArgs = new Dictionary<string, StringDynamicData>();
            if (null != customObjects)
            {
                MQueueparameter[] mqp = GetCustomObjectParameter2();

                if (ArrFunc.IsFilled(mqp))
                {
                    for (int i = 0; i < mqp.Length; i++)
                    {
                        if (false == _dynamicArgs.ContainsKey(mqp[i].id))
                        {
                            StringDynamicData sDD = new StringDynamicData();
                            sDD.datatype = mqp[i].dataType.ToString();
                            sDD.name = mqp[i].id;
                            sDD.value = mqp[i].Value;
                            _dynamicArgs.Add(mqp[i].id, sDD);
                        }
                        else
                        {
                            _dynamicArgs[mqp[i].id].value = mqp[i].Value;
                        }
                    }
                }
            }
            if (StrFunc.IsFilled(Request.QueryString["DA"]))
            {
                Dictionary<string, StringDynamicData> dynamicArgs2 = RepositoryTools.CalcDynamicArgumentFromHttpParameter2(Ressource.DecodeDA(Request.QueryString["DA"]));
                if (ArrFunc.IsFilled(dynamicArgs2))
                {
                    foreach (string key in dynamicArgs2.Keys)
                    {
                        if (false == _dynamicArgs.ContainsKey(key))
                            _dynamicArgs.Add(key, dynamicArgs2[key]);
                        else
                            _dynamicArgs[key] = dynamicArgs2[key];
                    }
                }
            }
        }

        /// <summary>
        /// Retourne un array de MQueueparameter à partir des customObjects de la page.
        /// <para>Les valeurs des paramètres sont renseignés à partir des valeurs présentes sur les contrôles</para>
        /// </summary>
        /// <returns></returns>
        private MQueueparameter[] GetCustomObjectParameter()
        {
            MQueueparameter[] ret = null;
            ArrayList al = new ArrayList();
            if (_customObjects != null)
            {
                for (int i = 0; i < ArrFunc.Count(_customObjects.customObject); i++)
                {
                    if (Enum.IsDefined(typeof(TypeData.TypeDataEnum), _customObjects.customObject[i].DataType))
                    {
                        MQueueparameter parameter = new MQueueparameter();
                        parameter.id = _customObjects.customObject[i].ClientId;
                        parameter.dataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), _customObjects.customObject[i].DataType, true);

                        BS_TextBox txtbox = null;
                        BS_DropDownList ddl = null;

                        switch (parameter.dataType)
                        {
                            case TypeData.TypeDataEnum.date:
                            case TypeData.TypeDataEnum.datetime:
                            case TypeData.TypeDataEnum.time:
                                #region date,datetime,time
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]))
                                        parameter.SetValue(new DtFunc().StringToDateTime(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    txtbox = plhCustomObject.FindControl(Cst.TXT + parameter.id) as BS_TextBox;
                                    if ((null != txtbox) && StrFunc.IsFilled(txtbox.Text))
                                        parameter.SetValue(new DtFunc().StringToDateTime(txtbox.Text));
                                }
                                #endregion
                                break;
                            case TypeData.TypeDataEnum.@string:
                            case TypeData.TypeDataEnum.text:
                            case TypeData.TypeDataEnum.integer:
                            //PL 20151208 Add Decimal 
                            case TypeData.TypeDataEnum.@decimal:
                            case TypeData.TypeDataEnum.dec:
                                #region string,text,integer,decimal
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]))
                                        parameter.SetValue(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id]))
                                    {
                                        string value = Request.Params[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id];
                                        ddl = plhCustomObject.FindControl(Cst.DDL + parameter.id) as BS_DropDownList;
                                        if (null != ddl)
                                        {
                                            ControlsTools.DDLSelectByValue(ddl, value);
                                            parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                        }
                                        else
                                            parameter.SetValue(Request.Params[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id]);     // Params pour recupérer la selectedValue   
                                    }
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.HSL + parameter.id]))
                                        parameter.SetValue(Request.Params[ContentPlaceHolder_UniqueID + Cst.HSL + parameter.id]);      // Params pour recupérer la selectedValue    
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    txtbox = plhCustomObject.FindControl(Cst.TXT + parameter.id) as BS_TextBox;
                                    if ((null != txtbox) && StrFunc.IsFilled(txtbox.Text))
                                        parameter.SetValue(txtbox.Text);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    ddl = plhCustomObject.FindControl(Cst.DDL + parameter.id) as BS_DropDownList;
                                    if ((null != ddl) && StrFunc.IsFilled(ddl.SelectedValue))
                                    {
                                        parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                    }
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    HtmlSelect hsl = plhCustomObject.FindControl(Cst.HSL + parameter.id) as HtmlSelect;
                                    //
                                    if ((null != hsl) && StrFunc.IsFilled(hsl.Value))
                                        parameter.SetValue(hsl.Value);
                                }
                                #endregion
                                break;
                            case TypeData.TypeDataEnum.@bool:
                                #region bool
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.CHK + parameter.id]))
                                        parameter.SetValue(BoolFunc.IsTrue(Request.Form[ContentPlaceHolder_UniqueID + Cst.CHK + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.HCK + parameter.id]))
                                        parameter.SetValue(BoolFunc.IsTrue(Request.Form[ContentPlaceHolder_UniqueID + Cst.HCK + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_ClassicCheckBox checkbox = plhCustomObject.FindControl(Cst.CHK + parameter.id) as BS_ClassicCheckBox;
                                    if (null != checkbox)
                                        parameter.SetValue(checkbox.chk.Checked);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_CheckBox checkbox = plhCustomObject.FindControl(Cst.CHK + parameter.id) as BS_CheckBox;
                                    if (null != checkbox)
                                        parameter.SetValue(checkbox.Checked);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_HtmlCheckBox htmlcheckbox = plhCustomObject.FindControl(Cst.HCK + parameter.id) as BS_HtmlCheckBox;
                                    if (null != htmlcheckbox)
                                        parameter.SetValue(htmlcheckbox.Checked);
                                }
                                #endregion
                                break;
                        }
                        al.Add(parameter);
                    }
                }
                ret = (MQueueparameter[])al.ToArray(typeof(MQueueparameter));
            }
            return ret;
        }
        private MQueueparameter[] GetCustomObjectParameter2()
        {
            MQueueparameter[] ret = null;
            ArrayList al = new ArrayList();
            if (_customObjects != null)
            {
                for (int i = 0; i < ArrFunc.Count(_customObjects.customObject); i++)
                {
                    if (Enum.IsDefined(typeof(TypeData.TypeDataEnum), _customObjects.customObject[i].DataType))
                    {
                        MQueueparameter parameter = new MQueueparameter();
                        parameter.id = _customObjects.customObject[i].ClientId;
                        parameter.dataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), _customObjects.customObject[i].DataType, true);

                        BS_TextBox txtbox = null;
                        BS_DropDownList ddl = null;

                        switch (parameter.dataType)
                        {
                            case TypeData.TypeDataEnum.date:
                            case TypeData.TypeDataEnum.datetime:
                            case TypeData.TypeDataEnum.time:
                                #region date,datetime,time
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]))
                                        parameter.SetValue(new DtFunc().StringToDateTime(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    txtbox = ControlsTools.FindControlRecursive(plhCustomObject, Cst.TXT + parameter.id) as BS_TextBox;
                                    if ((null != txtbox) && StrFunc.IsFilled(txtbox.Text))
                                        parameter.SetValue(new DtFunc().StringToDateTime(txtbox.Text));
                                }
                                #endregion
                                break;
                            case TypeData.TypeDataEnum.@string:
                            case TypeData.TypeDataEnum.text:
                            case TypeData.TypeDataEnum.integer:
                            //PL 20151208 Add Decimal 
                            case TypeData.TypeDataEnum.@decimal:
                            case TypeData.TypeDataEnum.dec:
                                #region string,text,integer,decimal
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]))
                                        parameter.SetValue(Request.Form[ContentPlaceHolder_UniqueID + Cst.TXT + parameter.id]);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id]))
                                    {
                                        string value = Request.Params[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id];
                                        ddl = ControlsTools.FindControlRecursive(plhCustomObject, Cst.DDL + parameter.id) as BS_DropDownList;
                                        if (null != ddl)
                                        {
                                            ControlsTools.DDLSelectByValue(ddl, value);
                                            parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                        }
                                        else
                                            parameter.SetValue(Request.Params[ContentPlaceHolder_UniqueID + Cst.DDL + parameter.id]);     // Params pour recupérer la selectedValue   
                                    }
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.HSL + parameter.id]))
                                        parameter.SetValue(Request.Params[ContentPlaceHolder_UniqueID + Cst.HSL + parameter.id]);      // Params pour recupérer la selectedValue    
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    txtbox = ControlsTools.FindControlRecursive(plhCustomObject, Cst.TXT + parameter.id) as BS_TextBox;
                                    if ((null != txtbox) && StrFunc.IsFilled(txtbox.Text))
                                        parameter.SetValue(txtbox.Text);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    ddl = ControlsTools.FindControlRecursive(plhCustomObject, Cst.DDL + parameter.id) as BS_DropDownList;
                                    if ((null != ddl) && StrFunc.IsFilled(ddl.SelectedValue))
                                    {
                                        parameter.SetValue(ddl.SelectedValue, ddl.SelectedItem.Text);
                                    }
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    HtmlSelect hsl = ControlsTools.FindControlRecursive(plhCustomObject, Cst.HSL + parameter.id) as HtmlSelect;
                                    if ((null != hsl) && StrFunc.IsFilled(hsl.Value))
                                        parameter.SetValue(hsl.Value);
                                }
                                #endregion
                                break;
                            case TypeData.TypeDataEnum.@bool:
                                #region bool
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.CHK + parameter.id]))
                                        parameter.SetValue(BoolFunc.IsTrue(Request.Form[ContentPlaceHolder_UniqueID + Cst.CHK + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    if (StrFunc.IsFilled(Request.Form[ContentPlaceHolder_UniqueID + Cst.HCK + parameter.id]))
                                        parameter.SetValue(BoolFunc.IsTrue(Request.Form[ContentPlaceHolder_UniqueID + Cst.HCK + parameter.id]));
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_ClassicCheckBox checkbox = ControlsTools.FindControlRecursive(plhCustomObject, Cst.CHK + parameter.id) as BS_ClassicCheckBox;
                                    if (null != checkbox)
                                        parameter.SetValue(checkbox.chk.Checked);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_CheckBox checkbox = plhCustomObject.FindControl(Cst.CHK + parameter.id) as BS_CheckBox;
                                    if (null != checkbox)
                                        parameter.SetValue(checkbox.Checked);
                                }
                                if (StrFunc.IsEmpty(parameter.Value))
                                {
                                    BS_HtmlCheckBox htmlcheckbox = ControlsTools.FindControlRecursive(plhCustomObject, Cst.HCK + parameter.id) as BS_HtmlCheckBox;
                                    if (null != htmlcheckbox)
                                        parameter.SetValue(htmlcheckbox.Checked);
                                }
                                #endregion
                                break;
                        }
                        al.Add(parameter);
                    }
                }
                ret = (MQueueparameter[])al.ToArray(typeof(MQueueparameter));
            }
            return ret;
        }


        /// <summary>
        /// Alimente les customObjects avec les valeurs des dynamicArguments passés via l'URL
        /// </summary>
        private void SetCustomControlURLDA()
        {
            if ((null != customObjects) && (null != dynamicArgs) && (StrFunc.IsFilled(Request.QueryString["DA"])))
            {
                IEnumerator listDA = dynamicArgs.Values.GetEnumerator();
                while (listDA.MoveNext())
                {
                    StringDynamicData sdd = (StringDynamicData)listDA.Current;
                    if (sdd.isSettingByRequestHTTP)
                    {
                        for (int i = 0; i < ArrFunc.Count(customObjects.customObject); i++)
                        {
                            CustomObject co = customObjects.customObject[i];
                            if (co.ClientId == sdd.name)
                            {
                                if (TypeData.IsTypeDate(co.DataType))
                                {
                                    WCTextBox2 txtBox = plhCustomObject.FindControl(co.CtrlClientId) as WCTextBox2;
                                    if (null != txtBox)
                                    {
                                        txtBox.Text = new DtFunc().GetDateTimeString(sdd.value, DtFunc.FmtShortDate);
                                        txtBox.Enabled = false;
                                        break;
                                    }
                                }
                                else if (TypeData.IsTypeBool(co.DataType))
                                {
                                    WCCheckBox2 chktBox = plhCustomObject.FindControl(co.CtrlClientId) as WCCheckBox2;
                                    if (null != chktBox)
                                    {
                                        chktBox.Checked = BoolFunc.IsTrue(sdd.value);
                                        chktBox.Enabled = false;
                                        break;
                                    }
                                }
                                else if (TypeData.IsTypeString(co.DataType) || TypeData.IsTypeInt(co.DataType))
                                {
                                    BS_DropDownList ddlBox = plhCustomObject.FindControl(co.CtrlClientId) as BS_DropDownList;
                                    if (null == ddlBox)
                                    {
                                        BS_TextBox txtBox = plhCustomObject.FindControl(co.CtrlClientId) as BS_TextBox;
                                        if (null == txtBox)
                                        {
                                            BS_Label lbl = plhCustomObject.FindControl(co.CtrlClientId) as BS_Label;
                                            if (null != lbl)
                                            {
                                                lbl.Text = "(id:" + sdd.value + ")";
                                                lbl.Font.Size = FontUnit.Smaller;
                                            }
                                        }
                                        else
                                        {
                                            txtBox.Text = sdd.value;
                                            txtBox.Enabled = false;
                                        }
                                    }
                                    else
                                    {
                                        ddlBox.Enabled = !ControlsTools.DDLSelectByValue(ddlBox, sdd.value);
                                        break;
                                    }
                                }
                                else if (TypeData.IsTypeDec(co.DataType))
                                {
                                    BS_TextBox txtBox = plhCustomObject.FindControl(co.CtrlClientId) as BS_TextBox;
                                    if (null != txtBox)
                                    {
                                        txtBox.Text = StrFunc.FmtDecimalToCurrentCulture(sdd.value);
                                        txtBox.Enabled = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void SetDropDownCustomObjects()
        {
            if (null != this.customObjects)
            {
                if (IsPostBack)
                {
                    customObjects.customObject.ToList().FindAll(item => item.Control == CustomObject.ControlEnum.dropdown).ForEach(item =>
                    {
                        DropDownList ddl = (DropDownList)plhCustomObject.FindControl(Cst.DDL + item.ClientId);
                        string ddlValue = Request.Params[ContentPlaceHolder_UniqueID + Cst.DDL + item.ClientId];
                        ControlsTools.DDLSelectByValue(ddl, ddlValue);
                    });
                }
                ControlBetweenDates();
            }
        }
        /// <summary>
        /// Construction du titre et sous-titre
        /// Affectation de l'URL des bouton OpenModel & SaveModel
        /// </summary>
        private void SetTitleAndUrlModel()
        {
            string titleType = string.Empty;
            string titleMenu = string.Empty;

            if (StrFunc.IsFilled(Request.QueryString["TitleRes"]))
                titleType = Ressource.GetString(Request.QueryString["TitleRes"], true);
            else
                titleType = Ressource.GetString(gvTemplate.Title, string.Empty, true);

            if (isConsultation)
            {
                if (StrFunc.IsEmpty(titleType))
                    titleType = Ressource.GetString("Consultation", "Consultation", true);
                titleMenu = gvTemplate.consult.Title;
                subTitle = "[" + gvTemplate.consult.template.titleDisplayName + "]";
            }
            else
            {
                #region Mode REFERENTIAL / PROCESS / ...
                titleMenu = Ressource.GetMenu_Fullname(gvTemplate.IDMenu.ToString(), gvTemplate.TitleMenu);
                if (SessionTools.Menus.HasSeveralParent(gvTemplate.IDMenu))
                {
                    EFS.Common.Web.Menu.Menu mnuParent = SessionTools.Menus.GetMenuParentById(gvTemplate.IDMenuSys);
                    if (null != mnuParent)
                        titleMenu += " - " + Ressource.GetMenu_Fullname(mnuParent.IdMenu);
                }

                if (StrFunc.IsEmpty(titleMenu) || "null" == titleMenu)
                {
                    if (StrFunc.IsFilled(gvTemplate.referential.Ressource))
                        titleMenu = Ressource.GetString(gvTemplate.referential.Ressource, true);
                    else
                        titleMenu = Ressource.GetString(gvTemplate.ObjectName, true);
                }
                subTitle = HttpUtility.UrlDecode(Request.QueryString["SubTitle"], System.Text.Encoding.UTF8) + string.Empty + "[" +
                    ((null != gvTemplate.consult) ? gvTemplate.consult.template.titleDisplayName : string.Empty) + "]";
                #endregion Mode REFERENTIAL / PROCESS
            }
            mainTitle = titleType + " : " + titleMenu;

            // Set Url pour ouverture form d'ouverture ou sauvegarde d'un modèle
            string urlModel = "&C=" + HttpUtility.UrlEncode(idLstConsult) + "&T=" + HttpUtility.UrlEncode(idLstTemplate) + "&A=" + idA.ToString();
            urlModel += "&T1=" + HttpUtility.UrlEncode(titleType) + "&T2=" + HttpUtility.UrlEncode(titleMenu);
            urlModel += "&IDMenu=" + idMenu;
            urlModel += "&ParentGUID=" + _hiddenFieldGUID.Value;

            btnOpenSaveModel.Attributes.Add("data-id", "O");
            btnOpenSaveModel.Attributes.Add("title", Ressource.GetString("btnOpenSaveModel", true));
        }

        /// <summary>
        /// Ininitalisation des clés idLstConsult|idLstTemplate|idA
        /// Chargement du template
        /// </summary>
        public override void SetKeyAndLoadTemplate()
        {
            isConsultation = StrFunc.IsFilled(Request.QueryString[Cst.ListType.Consultation.ToString()]);
            if (isConsultation)
            {
                idLstConsult = Request.QueryString["Consultation"];
                idLstTemplate = gvTemplate.consult.template.IDLSTTEMPLATE;
                idA = gvTemplate.consult.template.IDA;
            }
            else
            {
                string consultName = Request.QueryString["P1"] + gvTemplate.ObjectName;
                idLstConsult = RepositoryWeb.PrefixForReferential + gvTemplate.Title + consultName;
                idLstTemplate = gvTemplate.IdLstTemplate;
                idA = gvTemplate.IdA;
            }
            idMenu = gvTemplate.IDMenu;
            parentGUID = _hiddenFieldGUID.Value;
            consult = new LstConsultData(this.CS, idLstConsult, string.Empty);
            consult.LoadTemplate(CS, idLstTemplate, idA);
            isColumnByGroup = consult.IsMultiTable(CS);
            isXMLSource = RepositoryWeb.IsReferential(idLstConsult);
            btnDisplay.Visible = isSQLSource;
            sessionName_LstColumn = idLstConsult + "-" + idLstTemplate + "-" + idA.ToString() + "-" + parentGUID + "-" + "ListOfColumns";
        }

        protected void Loading()
        {
            try
            {
                string uploadTag = string.Empty;
                if (StrFunc.IsFilled(Request.QueryString["UploadTag"]))
                    uploadTag = Request.QueryString["UploadTag"];
                string[] uploadFileMIMETypes = new string[1] { Cst.TypeMIME.Text.Xml };

                // Chargement du Titre
                SetTitleAndUrlModel();

                // Mise à jour des contôles DDL avec les valeurs présentes dans REQUEST
                // FI 20110812 [17537] Asp ne maintient pas l'état, la combo perd sa valeur ??? 
                SetDropDownCustomObjects();

                #region Additional CheckBox
                // Affichage des données valides et des données mises à jour aujourd’hui 
                CheckBox chkValidity = ControlsTools.FindControlRecursive(plhCheckSelected, "chkValidity") as CheckBox;
                if (null != chkValidity)
                    gvTemplate.referential.isValidDataOnly = (chkValidity.Visible && chkValidity.Checked);

                CheckBox chkTodayUpdate = ControlsTools.FindControlRecursive(plhCheckSelected, "chkTodayUpdate") as CheckBox;
                if (null != chkTodayUpdate)
                    gvTemplate.referential.isDailyUpdDataOnly = (chkTodayUpdate.Visible && chkTodayUpdate.Checked);

                #endregion

                #region Button properties

                #region btnRunProcess
                string resProcess = GetResourceBtnProcess();
                btnRunProcess.Visible = m_IsProcess;
                btnRunProcess.Enabled = SessionTools.IsActionEnabled;
                btnRunProcess.Text = Ressource.GetMulti(resProcess);
                btnRunProcess.ToolTip = Ressource.GetMulti(resProcess, 1) + Cst.GetAccessKey(btnRunProcess.AccessKey);
                btnRunProcess.CommandName = m_ProcessType;
                btnRunProcess.CommandArgument = m_ProcessName;
                #endregion btnRunProcess

                #region btnRunProcessIO
                string resIO = GetResourceBtnIO();
                btnRunProcessIO.Visible = IsDisplayButtonIO();
                btnRunProcessIO.Enabled = SessionTools.IsActionEnabled;
                btnRunProcessIO.Attributes.Add("data-cmd", Cst.ListProcess.ListProcessTypeEnum.Service.ToString());
                btnRunProcessIO.Attributes.Add("data-arg", Cst.ProcessIOType.IO.ToString());
                btnRunProcessIO.Text = Ressource.GetMulti(resIO);
                btnRunProcessIO.ToolTip = Ressource.GetMulti(resIO, 1) + Cst.GetAccessKey(btnRunProcessIO.AccessKey);
                btnRunProcessIO.CommandName = Cst.ListProcess.ListProcessTypeEnum.Service.ToString();
                btnRunProcessIO.CommandArgument = Cst.ProcessIOType.IO.ToString();
                #endregion btnRunProcessIO

                #region btnRunProcessAndIO
                string resRunProcessAndIO = GetResourceBtnProcessAndIO();
                btnRunProcessAndIO.Visible = IsDisplayButtonProcessAndIO();
                btnRunProcessAndIO.Enabled = SessionTools.IsActionEnabled;
                btnRunProcessAndIO.Text = Ressource.GetMulti(resRunProcessAndIO);
                btnRunProcessAndIO.ToolTip = Ressource.GetMulti(resRunProcessAndIO, 1) + Cst.GetAccessKey(btnRunProcessAndIO.AccessKey);
                btnRunProcessAndIO.CommandName = Cst.ListProcess.ListProcessTypeEnum.Service.ToString();
                btnRunProcessAndIO.CommandArgument = Cst.ProcessIOType.ProcessAndIO.ToString();
                #endregion btnRunProcessAndIO

                #region btnRecord
                btnRecord.Visible = gvTemplate.isGridInputMode;
                btnRecord.Text = Ressource.GetString(btnRecord.ID);
                if ((false == gvTemplate.referential.Create) &&
                    (false == gvTemplate.referential.Modify) &&
                    (false == gvTemplate.referential.Remove))
                {
                    btnRecord.Enabled = false;
                }
                #endregion btnRecord

                #region btnCancel
                btnCancel.Visible = gvTemplate.isGridInputMode;
                btnCancel.Text = Ressource.GetString(btnCancel.ID);
                #endregion btnCancel

                #region btnAddNew
                btnAddNew.Visible = false;
                btnAddNew.Text = Ressource.GetString(btnAddNew.ID);
                if ((gvTemplate.isFormInputMode || gvTemplate.isGridInputMode) && (gvTemplate.referential.Create))
                {
                    btnAddNew.Visible = (false == m_IsSpheresProcess);

                    if (gvTemplate.isGridInputMode)
                    {
                        btnAddNew.Click += new EventHandler(gvTemplate.AddRow);
                    }
                    else
                    {
                        //Cas particulier pour le ref ATTACHEDDOC(S) -> Add ouvre Upload.aspx
                        if (gvTemplate.ObjectName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() || gvTemplate.ObjectName == Cst.OTCml_TBL.ATTACHEDDOCS.ToString())
                        {
                            #region ATTACHEDDOC/ATTACHEDDOCS
                            string[] uploadMIMETypes = new string[1];
                            uploadMIMETypes[0] = Cst.TypeMIME.ALL;
                            string[] keyColumns = new string[2];
                            keyColumns[0] = "TABLENAME";
                            keyColumns[1] = "ID";
                            string[] keyValues = new string[2];
                            keyValues[0] = Ressource.DecodeDA(Request.QueryString["DA"]);
                            keyValues[1] = Request.QueryString["FK"];
                            string[] keyDatatypes = new string[2];
                            keyDatatypes[0] = TypeData.TypeDataEnum.@string.ToString();
                            keyDatatypes[1] = (gvTemplate.ObjectName == Cst.OTCml_TBL.ATTACHEDDOC.ToString() ? TypeData.TypeDataEnum.@integer.ToString() : TypeData.TypeDataEnum.@string.ToString());
                            btnAddNew.Attributes.Add("onclick", JavaScript.GetWindowOpenUpload(gvTemplate.ObjectName, uploadMIMETypes, gvTemplate.ObjectName, "LODOC", keyColumns, keyValues, keyDatatypes, string.Empty) + @"return false;");
                            #endregion
                        }
                        else if (gvTemplate.ObjectName == Cst.OTCml_TBL.ACTOR.ToString())
                        {
                            #region ACTOR
                            ////ADDCLIENTGLOP Test in progress...
                            //Control head = (null != this.Header) ? (Control)this.Header : (Control)PageTools.SearchHeadControl(this);

                            //string retSelectedValue = this.Request.Params["__EVENTARGUMENT"];

                            //string url_ = gvTemplate.GetURLForInsert(Request.QueryString["FK"]);
                            ////TBD ... (Récupération d'une URL, donc sans window.open())
                            //url_ = url_.Substring(@"window.open(""".Length);
                            //url_ = url_.Substring(0, url_.IndexOf(@""",""_blank"","));

                            ////TBD ressource
                            //string message = "";
                            //message += @"<table id=""tblAddActor""><tr><td colspan=""3"">";
                            //message += @"<span class='formula'>" + Ressource.GetString("SelectActorTemplate") + "</span>";
                            //message += @"</td></tr><tr><td>";
                            //message += @"<span>" + Cst.HTMLSpace2 + Ressource.GetString("lblTemplate") + "</span>";
                            //message += @"</td><td>&nbsp;</td><td>";
                            //message += @"<select name=""ddlSelect"" id=""ddlSelect"" class=""ddlCapture"" onchange=""ddlSelect_Change();"">" + Cst.CrLf;
                            //string option = @"<option selected=""selected"" value=""{0}"">{1}</option>" + Cst.CrLf;
                            //message += String.Format(option, "0", "");
                            //#region Load Actor Templates
                            //IDataReader dr = null;
                            //try
                            //{
                            //    option = @"<option value=""{0}"">{1}</option>" + Cst.CrLf;
                            //    string SQLSelect = SQLCst.SELECT + @"IDA,IDENTIFIER,DISPLAYNAME" + Cst.CrLf;
                            //    SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.ACTOR.ToString() + Cst.CrLf;
                            //    SQLSelect += SQLCst.WHERE + "ISTEMPLATE=" + DataHelper.SQL_Bit_True.ToString();
                            //    SQLSelect += SQLCst.AND + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, Cst.OTCml_TBL.ACTOR);
                            //    SQLSelect += SQLCst.ORDERBY + "IDENTIFIER";
                            //    dr = DataHelper.ExecuteReader(SessionTools.CS, CommandType.Text, SQLSelect);
                            //    while (dr.Read())
                            //    {
                            //        string text = dr["IDENTIFIER"].ToString();
                            //        if ((text != dr["DISPLAYNAME"].ToString()) && (!text.StartsWith(dr["DISPLAYNAME"].ToString())))
                            //        {
                            //            if (dr["DISPLAYNAME"].ToString().StartsWith(text))
                            //            {
                            //                text = dr["DISPLAYNAME"].ToString();
                            //            }
                            //            else
                            //            {
                            //                text += " - " + dr["DISPLAYNAME"].ToString();
                            //            }
                            //            if (text.Length > 100)
                            //            {
                            //                text = text.Substring(0, 97) + "...";
                            //            }
                            //        }
                            //        message += JavaScript.JSString(String.Format(option, dr["IDA"].ToString(), text));
                            //    }
                            //}
                            //// EG 20160308 Migration vs2013
                            //catch (Exception) { }
                            //finally
                            //{
                            //    if (null != dr)
                            //    {
                            //        // EG 20160308 Migration vs2013
                            //        //dr.Close();
                            //        dr.Dispose();
                            //    }
                            //}
                            //#endregion
                            //message += @"</select>";
                            //message += @"</td></tr><tr><td colspan=""3"">";
                            //message += @"<span class='title'>" + Ressource.GetString("NewActor") + "</span>";
                            //message += @"</td></tr><tr><td>";
                            //message += @"<span name=""lblActor"" id=""lblActor"" disabled>" + Cst.HTMLSpace2 + Ressource.GetString("ActorIdentifier") + "</span>";
                            //message += @"</td><td>&nbsp;</td><td>";
                            //message += @"<input type=""text"" name=""txtActor"" id=""txtActor"" class=""txtCaptureOptional"" width=""100%"" disabled />" + Cst.CrLf;
                            //message += @"</td></tr><tr><td>";
                            //message += @"<span name=""lblBook"" id=""lblBook"" disabled>" + Cst.HTMLSpace2 + Ressource.GetString("BookIdentifier") + "</span>";
                            //message += @"</td><td>&nbsp;</td><td>";
                            //message += @"<input type=""text"" name=""txtBook"" id=""txtBook"" class=""txtCaptureOptional"" width=""100%"" disabled=""true"" disabled />";
                            //message += @"</td></tr></table>";

                            //string title = Ressource.GetString("CreateNewActor");
                            //string js = "OpenAddActorConfirm(" + JavaScript.JSString(title) + "," + JavaScript.HTMLString(message) + "," +
                            //    JavaScript.JSString("addactor") + ",0,0,0,0," + JavaScript.JSString(url_) + "," + JavaScript.JSString("TBD") + ");";
                            //btnAddNew.Attributes.Add("onclick", js + @"return false;");

                            #endregion
                        }
                        else
                        {
                            btnAddNew.Attributes.Add("onclick", gvTemplate.GetURLForInsert(Request.QueryString["FK"]) + @";return false;");
                        }
                    }
                }
                #endregion btnAddNew

                #region btnMSExcel
                btnMSExcel.Attributes.Add("title", Ressource.GetString("ExportToMSExcel"));
                btnMSExcel.ServerClick += new EventHandler(OnToolbar_Click);

                #endregion btnMSExcel

                #region btnSql
                btnSql.Visible = isDisplaySQLInsertCommand();
                btnSql.Attributes.Add("title", Ressource.GetString("ExportToSQL"));
                btnSql.ServerClick += new EventHandler(OnToolbar_Click);
                #endregion btnSql

                #region btnZip
                btnZip.Attributes.Add("title", Ressource.GetString("ExportToZIP"));
                btnZip.ServerClick += new EventHandler(OnToolbar_Click);
                #endregion btnZip

                #endregion Button properties

                // Chargement des critères de filtres, tri et regroupement (READONLY)
                //LoadFilterReadOnly();

                InitializeListForControls();


                //InitializeListModel();

                if (IsReloadCriteria)
                    gvTemplate.isBindData = false;
                else
                    gvTemplate.isBindData = true;

                if (this.__EVENTTARGET == "PostAfterLoadDataSourceError")
                {
                    string[] stringSeparators = new string[] { "{-}" };
                    String[] aEventArg = this.__EVENTARGUMENT.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (ArrFunc.IsFilled(aEventArg))
                    {
                        if (ArrFunc.Count(aEventArg) >= 2 && aEventArg[1] == "ForceReload")
                        {
                            //On recharche la source de donnée sans les critères
                            gvTemplate.isLoadData = true;
                            _alErrorMessages.Add(Ressource.GetString("Msg_DisabledLstWhere"));
                            //Disable LstWhere
                            gvTemplate.consult.template.SetIsEnabledLstWhere(SessionTools.CS, SessionTools.Collaborator_IDA, gvTemplate.consult.template.IDLSTTEMPLATE, gvTemplate.consult.IdLstConsult, false);
                        }
                        _alErrorMessages.Add(aEventArg[0]);
                    }
                }

                if ((null != gvTemplate.consult) &&
                    (null != gvTemplate.consult.template.CSSFILENAME) &&
                    (0 < gvTemplate.consult.template.CSSFILENAME.Length))
                {
                    CSSFileName = gvTemplate.consult.template.CSSFILENAME;
                }
                //FI 20100512 SetPaging écrase la property AllowPaging
                //Spheres conserve celle spécifiée dans le view State
                //Sauf dans 2 cas (voir les cas plus bas)
                if (StrFunc.IsFilled(this.__EVENTTARGET) && __EVENTTARGET.Contains("lnkDisabledPager"))
                {
                    //1er cas l'utilisateur veut afficher le jeu de résultat sur 1 page
                    gvTemplate.SetPaging(-1);
                    //FI 20140630 [20101] CurrentPageIndex = 0 => il n'y a une unique page et son index est 0
                    //=> Cela permet d'avoir les n° de lignes corrects
                    gvTemplate.PageIndex = 0;
                }
                else if (StrFunc.IsFilled(this.__EVENTARGUMENT) && (__EVENTARGUMENT.ToUpper() == "SELFRELOAD_"))
                {
                    if (gvTemplate.consult.template != null)
                        //2ème cas l'utilisateur recharge les caractéristiques de son template (Bouton recharger ou modification du filtre etc...)
                        gvTemplate.SetPaging(gvTemplate.consult.template.ROWBYPAGE);
                }
                if ((gvTemplate.isFormInputMode || gvTemplate.isGridInputMode) && (gvTemplate.referential.Create))
                {
                    /* TO DO */
                }
            }
            catch (Exception) { throw; } // TrapException(ex); }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void OnLoadDataError(object sender, GridViewDataErrorEventArgs e)
        {
            string error = Ressource.GetString(e.message) + Cst.CrLf + Cst.CrLf + "(" + e.errorCode + ")";
            //En cas d'erreur lors du chargement il ne faut evidemment pas recharger la source de donnée
            gvTemplate.isLoadData = false;
            // pas de bind du datagrid puisque la page est repostée dans la foulée par un ConfirmStartUpImmediate ou un DialogStartUpImmediate
            gvTemplate.isBindData = false;

            string message = Ressource.GetString("Msg_ErrorOccurs");
            message += Cst.CrLf2 + "Error:" + Cst.CrLf;
            message += error;
            if (gvTemplate.referential.HasLstWhereClause && gvTemplate.consult.template.ISENABLEDLSTWHERE)
            {
                string postback_target = "PostAfterLoadDataSourceError";
                string postback_argument = error + "{-}";
                message += Cst.CrLf2 + Ressource.GetString("Msg_ContinueRefreshDataGridAsk");
                message += Cst.CrLf + Ressource.GetString("Msg_DisabledLstWhere");
                JavaScript.ConfirmStartUpImmediate((PageBase)this.Page, message, postback_target, postback_argument + "ForceReload", postback_argument + "Undo");
            }
            else
            {
                JavaScript.DialogStartUpImmediate((PageBase)this.Page, message);
            }
            _alErrorMessages.Add(error);

            //Ecriture du message d'erreur dans le log pour du diag
            string msgError = StrFunc.AppendFormat("Error on LoadDataSource:{0}", e.errorCode); ;
            WriteLogException(new Exception(msgError));
        }


        /// <summary>
        /// Alimente le panel lstFilterReadOnly (plhFilterReadOnly) avec les critères et les tri appliqués par la consultation
        /// </summary>
        private void LoadFilterReadOnly()
        {
            //FI 20150120 [XXXXX] FreeFilter est activé
            Boolean isAddFreeFilter = (SessionTools.User.identification.identifier == "EFS");
#if DEBUG
            isAddFreeFilter = true;
#endif

            if (isAddFreeFilter)
            {
                //WCTextBox2 txt = new WCTextBox2("txtSearch", false, false, false, "txtCapture", null);
                //txt.CssClass = "txtSearch";
                //txt.AutoPostBack = false;
                //txt.EnableViewState = true;
                //txt.AutoCompleteType = AutoCompleteType.Search;
                //txt.Attributes.Add("onkeydown", "PostBackOnKeyEnter(event,'txtSearch','SearchData');false");
                //txt.Attributes.Add("onblur", "OnCtrlChanged('txtSearch','SearchData');false");
                //lstFilterReadOnlyTitle.Controls.Add(txt);
                //if (ArrFunc.Count(infosWhereForPage) > 0)
                //    pnlData.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("And", true), HtmlTools.cssInfosAnd));
                //pnlData.Controls.Add(txt);
            }

            // Chargement des filtres
            if (isConsultation)
                where = gvTemplate.consult.GetInfoWhere(CS, SessionTools.User.entity_IdA);
            else
                where = gvTemplate.consult.GetInfoWhereFromReferential(CS, gvTemplate.referential, SessionTools.User.entity_IdA);
            bool isFilter = ArrFunc.IsFilled(where);
            string filterTitle = Ressource.GetString("lblFilters");

            Panel pnlData = null;
            if (isFilter || isAddFreeFilter)
            {
                #region Infos Where
                pnlData = new Panel();
                pnlData.ID = plhFilterReadOnly.ID + "WhereDataRow";

                for (int i = 0; i < ArrFunc.Count(where); i++)
                {
                    if (i != 0)
                        pnlData.Controls.Add(HtmlTools.NewLabel(Ressource.GetString("And", true), HtmlTools.cssInfosAnd));

                    Panel pnlDataFilter = new Panel();
                    pnlDataFilter.CssClass = "alert";
                    // RD 20170227 [xxxxx] Remplacet <brnobold/> par espace
                    pnlDataFilter.Controls.Add(HtmlTools.NewLabel(where[i].columnIdentifier.Replace(Cst.HTMLBreakLine, Cst.Space).Replace("<brnobold/>", Cst.Space), "filterInfos"));

                    HtmlGenericControl code = new HtmlGenericControl("code");
                    code.InnerText = where[i].GetDisplayOperator();
                    pnlDataFilter.Controls.Add(code);

                    // Ne pas afficher la "valeur" pour les opérateurs: Checked et Unchecked
                    if (("Checked" != where[i].@operator) && ("Unchecked" != where[i].@operator))
                        pnlDataFilter.Controls.Add(HtmlTools.NewLabel(where[i].lstValue, "filterInfos"));

                    if (false == where[i].isMandatory)
                    {
                        pnlDataFilter.CssClass = "alert alert-dismissible";
                        HtmlButton btn = new HtmlButton();
                        btn.ID = "imgDisabledFilterPosition" + where[i].position.ToString(); ;
                        btn.Attributes.Add("class", "close");
                        btn.Attributes.Add("data-dismiss", "alert");
                        Label lbl = new Label();
                        lbl.Text = "&times;";
                        lbl.ToolTip = Ressource.GetString("disablethisFilter");
                        btn.Attributes.Add("onclick", ClientScript.GetPostBackEventReference(btn, "SELFCLEAR_") + ";return false;");
                        btn.Controls.Add(lbl);
                        pnlDataFilter.Controls.Add(btn);
                    }
                    pnlData.Controls.Add(pnlDataFilter);
                }

                bool isContainsOptionalFilter = false;
                for (int i = 0; i < ArrFunc.Count(where); i++)
                {
                    isContainsOptionalFilter = (false == where[i].isMandatory);
                    if (isContainsOptionalFilter)
                        break;
                }

                if (isContainsOptionalFilter)
                {
                    HtmlButton btnFilterReadOnlyTitle = new HtmlButton();
                    btnFilterReadOnlyTitle.ID = "btnFilterReadOnlyTitle";
                    btnFilterReadOnlyTitle.Attributes.Add("class", "btn btn-xs btn-remove");
                    btnFilterReadOnlyTitle.Attributes.Add("title", Ressource.GetString("disableFiltersInformation"));
                    btnFilterReadOnlyTitle.Attributes.Add("onclick", ClientScript.GetPostBackEventReference(btnFilterReadOnlyTitle, "SELFRELOAD_") + ";return false;");
                    HtmlGenericControl span = new HtmlGenericControl("span");
                    span.Attributes.Add("class", "glyphicon glyphicon-check");
                    btnFilterReadOnlyTitle.Controls.Add(span);
                    lstFilterReadOnlyTitle.Controls.Add(btnFilterReadOnlyTitle);
                }


                //btnFilterReadOnlyTitle.Visible = isContainsOptionalFilter;
                //btnFilterReadOnlyTitle.Attributes.Add("onclick", ClientScript.GetPostBackEventReference(btnFilterReadOnlyTitle, "SELFRELOAD_") + ";return false;");
                
                //Add Filters Infos
                plhFilterReadOnly.Controls.Add(pnlData);
                #endregion
            }


            // Chargement des tris
            orderBy = gvTemplate.consult.GetInfoOrderBy(CS);
            bool isSortBy = ArrFunc.IsFilled(orderBy) && ArrFunc.IsFilled(orderBy[0]);
            string sortTitle = Ressource.GetString("SortedBy");


            // Affichage du titre
            string title = (isFilter && isSortBy) ? filterTitle + " / " + sortTitle : isFilter ? filterTitle : sortTitle;
            lstFilterReadOnlyTitle.Controls.Add(HtmlTools.NewLabel(title, HtmlTools.cssInfosColumnName));

            if (isSortBy)
            {
                #region Infos SortBy
                pnlData = new Panel();
                pnlData.ID = plhFilterReadOnly.ID + "OrderByDataRow";
                if (isFilter && isSortBy)
                    pnlData.CssClass = "separator";
                //pnlData.CssClass = "alert alert-warning";
                for (int i = 0; i < orderBy[0].Count; i++)
                {
                    if (i != 0)
                        pnlData.Controls.Add(HtmlTools.NewLabel(", ", HtmlTools.cssInfosAnd));

                    Panel pnlDataSortBy = new Panel();
                    pnlDataSortBy.CssClass = "alert alert-warning";
                    pnlDataSortBy.Controls.Add(HtmlTools.NewLabel(orderBy[0][i].ToString().Replace(Cst.HTMLBreakLine, Cst.Space), "filterInfos"));

                    HtmlGenericControl code = new HtmlGenericControl("code");
                    code.InnerText = orderBy[1][i].ToString().ToUpper();

                    pnlDataSortBy.Controls.Add(code);
                    pnlData.Controls.Add(pnlDataSortBy);
                }
                plhFilterReadOnly.Controls.Add(pnlData);
                #endregion
            }
        }

        /// <summary>
        /// Alimente le panel lstFilterReadOnly (plhFilterReadOnly) avec les critères et les tri appliqués par la consultation
        /// </summary>
        private void LoadLstCriteria()
        {
            LoadLstDisplay();
            LoadLstFilter();
            LoadLstOpen();
        }
        private void LoadLstDisplay()
        {
            uc_lstdisplay.LoadDataControls();
        }
        private void LoadLstFilter()
        {
            uc_lstfilter.LoadDataControls();
        }
        private void LoadLstOpen()
        {
            uc_lstmodel.LoadDataControls();
        }

        /// <summary>
        /// Initialisation des checks Validity et TodayUpdate
        /// </summary>
        private void InitChkValidityAndChkTodayUpdate()
        {
            string javascript = string.Empty;

            BS_ClassicCheckBox chkValidity = new BS_ClassicCheckBox("chkValidity", true,
                Ressource.GetString("Repository" + AdditionalCheckBoxEnum.Validity), false);
            chkValidity.Visible = gvTemplate.referential.ExistsColumnsDateValidity;
            if (IsPostBack == false)
                chkValidity.chk.Checked = true;

            javascript = ClientScript.GetPostBackEventReference(chkValidity.chk, "SELFCLEAR_");
            chkValidity.chk.Attributes.Add("onclick", javascript);
            plhCheckSelected.Controls.Add(chkValidity);

            BS_ClassicCheckBox chkTodayUpdate = new BS_ClassicCheckBox("chkTodayUpdate", true,
                Ressource.GetString("Repository" + AdditionalCheckBoxEnum.TodayUpdate), false);
            chkTodayUpdate.Visible = gvTemplate.referential.ExistsColumnsINS && gvTemplate.referential.ExistsColumnsUPD;
            if (IsPostBack == false)
                chkTodayUpdate.chk.Checked = false;

            if (chkValidity.Visible && chkTodayUpdate.Visible)
            {
                chkValidity.CssClass = "col-sm-6 " + chkValidity.CssClass;
                chkTodayUpdate.CssClass = "col-sm-6 " + chkTodayUpdate.CssClass;
            }
            else if (chkValidity.Visible)
            {
                chkValidity.CssClass = "col-sm-6 col-sm-offset-6 " + chkValidity.CssClass;
            }
            else if (chkTodayUpdate.Visible)
            {
                chkTodayUpdate.CssClass = "col-sm-6 col-sm-offset-6 " + chkTodayUpdate.CssClass;
            }

            javascript = ClientScript.GetPostBackEventReference(chkTodayUpdate.chk, "SELFCLEAR_");
            chkTodayUpdate.chk.Attributes.Add("onclick", javascript);
            plhCheckSelected.Controls.Add(chkTodayUpdate);
        }

        private void DisplayInfo()
        {
            try
            {
                if (ArrFunc.IsFilled(_alException))
                {
                    _alErrorMessages.Add(Ressource.GetString("Msg_Unexpected_ErrorOccurs"));
                    foreach (SpheresException ex in _alException)
                        _alErrorMessages.Add(ex.ToString());
                }

                if (ArrFunc.IsFilled(gvTemplate.alException))
                {
                    _alErrorMessages.Add(Ressource.GetString("Msg_Unexpected_ErrorOccurs"));
                    foreach (SpheresException ex in gvTemplate.alException)
                        _alErrorMessages.Add(ex.ToString());
                }

                string msg = string.Empty;
                if (ArrFunc.IsFilled(_alErrorMessages))
                {
                    #region Error
                    for (int i = 0; i < ArrFunc.Count(_alErrorMessages); i++)
                        msg += _alErrorMessages[i].ToString() + Cst.CrLf2;
                    msg = Ressource.GetString2("Msg_Error", msg);
                    litMsg.Text = "<pre class='alert alert-danger'>" + msg + "</pre>";
                    #endregion Error
                }
                else
                {
                    bool isEmptyDtg = ((false == gvTemplate.isDataAvailable) || (gvTemplate.totalRowscount == 0));

                    if (gvTemplate.isLoadData)
                    {
                        #region Warning
                        if (isEmptyDtg)
                        {
                            msg = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NoInformation"));

                            if (isConsultation)
                                MsgForAlertImmediate += Ressource.GetString2("Msg_NoInformationDet");
                            litMsg.Text = "<pre class='alert alert-warning'>" + msg + "</pre>";

                            btnRunProcess.Enabled = false;
                            btnRunProcessIO.Enabled = false;
                            btnRunProcessAndIO.Enabled = false;
                        }
                        else
                        {
                            if (gvTemplate.isVirtualItemCountError)
                            {
                                msg = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NbRowCountError"));
                                msg += Cst.CrLf + "[" + gvTemplate.lastErrorQueryCount + "]";

                                MsgForAlertImmediate += msg;
                            }
                            else
                            {
                                int top = (int)(SystemSettings.GetAppSettings("Spheres_DataViewMaxRows", typeof(System.Int32), -1));
                                if ((gvTemplate.isDataAvailable) && (top > -1) && (gvTemplate.totalRowscount == top))
                                {
                                    msg = Ressource.GetString2("Msg_Warning", Ressource.GetString2("Msg_NbRowMax"));
                                    MsgForAlertImmediate += Ressource.GetString2("Msg_NbRowMaxDet", top.ToString());
                                }
                            }
                            litMsg.Text = "<pre class='alert alert-warning'>" + msg + "</pre>";
                        }
                        #endregion Warning
                    }
                    else if (((false == IsPostBack) && isEmptyDtg) || (StrFunc.IsFilled(__EVENTARGUMENT) && (__EVENTARGUMENT.ToUpper() == "SELFCLEAR_")))
                    {
                        #region Info
                        msg = Ressource.GetString2("Msg_NotLoad");
                        litMsg.Text = "<pre class='alert alert-info'>" + msg + "</pre>";
                        #endregion Info

                        btnRunProcess.Enabled = false;
                        btnRunProcessIO.Enabled = false;
                        btnRunProcessAndIO.Enabled = false;

                    }
                }
                pnlMsgSummary.ToolTip = msg;
                pnlMsgSummary.Visible = StrFunc.IsFilled(msg);
            }
            catch (Exception) { throw; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pEx"></param>
        private void TrapException(Exception pEx)
        {
            if (isNoFailure)
            {
                SpheresException ex = SpheresExceptionParser.GetSpheresException(null, pEx);
                _alException.Add(ex);
                ((PageBase)Page).WriteLogException(ex);
            }
            else
            {
                throw pEx;
            }
        }

        /// <summary>
        /// Retourne le nom de la Ressource du bounton btnProcess 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnProcess()
        {
            string ret = Request.QueryString["ProcessRes"];
            if (m_IsSpheresProcess)
            {
                if (StrFunc.IsEmpty(ret))
                {
                    ret = "btnGenerate";
                    switch (SpheresProcessType)
                    {
                        case Cst.ProcessTypeEnum.CIGEN:
                        case Cst.ProcessTypeEnum.CMGEN:
                        case Cst.ProcessTypeEnum.RIMGEN:
                        case Cst.ProcessTypeEnum.RMGEN:
                            ret += "_" + SpheresProcessType.ToString();
                            break;
                    }
                }
            }
            else
                ret = "btnProcess";
            return ret;
        }
        /// <summary>
        /// Retourne la Ressource du bounton btnIO 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnIO()
        {
            string ret = Request.QueryString["IORes"];
            if (StrFunc.IsEmpty(ret))
            {
                ret = "btnIO";
                switch (SpheresProcessType)
                {
                    case Cst.ProcessTypeEnum.CMGEN:
                    case Cst.ProcessTypeEnum.RMGEN:
                        ret = "btnIO_SendMsg";
                        break;
                }
            }
            return ret;
        }
        /// <summary>
        /// Retourne le nom de la Ressource du bounton btnProcessAndIO 
        /// </summary>
        /// <returns></returns>
        private string GetResourceBtnProcessAndIO()
        {
            string ret = string.Empty;
            if (m_IsSpheresProcess)
                ret = "btnProcessAndIO_" + SpheresProcessType;
            return ret;
        }
        private void OnProcess_Click(object sender, CommandEventArgs e)
        {
            m_btnRunProcess_Clicked = (Button)sender;
            gvTemplate.isLoadData = false;
        }


        /// <summary>
        /// Affichage de la command exécutée dans preselect
        /// </summary>
        private void DisplayPreSelectCommand()
        {

            if (ArrFunc.IsFilled(gvTemplate.lastPreSelectCommand))
            {
                Pair<string, TimeSpan>[] lastPreSelectCommand = gvTemplate.lastPreSelectCommand;

                string WindowID = FileTools.GetUniqueName("LoadDataSource", "preSelectCommand");
                string write_File = SessionTools.TemporaryDirectory.MapPath("GridView") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "GridView" + @"/" + WindowID + ".xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("PreSelect Command use, File: {0}", write_File);
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("preSelectCommand");
                xmlDoc.AppendChild(xmlRoot);

                if (ArrFunc.Count(lastPreSelectCommand) == 1)
                {
                    xmlRoot.AppendChild(xmlDoc.CreateCDataSection(lastPreSelectCommand[0].First));
                    xmlRoot.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                    xmlRoot.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", lastPreSelectCommand[0].Second);

                }
                else
                {
                    for (int i = 0; i < ArrFunc.Count(lastPreSelectCommand); i++)
                    {
                        XmlElement xmlElement = xmlDoc.CreateElement("preSelectCommand" + i.ToString());
                        xmlElement.AppendChild(xmlDoc.CreateCDataSection(lastPreSelectCommand[i].First));
                        xmlElement.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                        xmlElement.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", lastPreSelectCommand[i].Second);
                        xmlRoot.AppendChild(xmlElement);
                    }
                }

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                XmlWriter xmlWritter = XmlTextWriter.Create(write_File, xmlWriterSettings);

                xmlDoc.Save(xmlWritter);

                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }
        }

        /// <summary>
        /// Affichage de la dernière query exécutée pour alimenter le Datagrid
        /// </summary>
        private void DisplayLastQuery()
        {

            if (null != gvTemplate.lastQuery)
            {
                string WindowID = FileTools.GetUniqueName("LoadDataSource", "lastQuery");
                string write_File = SessionTools.TemporaryDirectory.MapPath("GridView") + @"\" + WindowID + ".xml";
                string open_File = SessionTools.TemporaryDirectory.Path + "GridView" + @"/" + WindowID + ".xml";

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = true;
                //Declaration
                xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));
                //Comment
                string comment = StrFunc.AppendFormat("Last query use, File: {0}", write_File);
                xmlDoc.AppendChild(xmlDoc.CreateComment(comment));
                //Root
                XmlElement xmlRoot = xmlDoc.CreateElement("lastQuery");
                xmlDoc.AppendChild(xmlRoot);
                xmlRoot.Attributes.Append(xmlDoc.CreateAttribute("duration"));
                xmlRoot.Attributes["duration"].Value = String.Format(@"{0:hh\:mm\:ss\.fff}", gvTemplate.lastQuery.Second);

                xmlRoot.AppendChild(xmlDoc.CreateCDataSection(gvTemplate.lastQuery.First));

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.Indent = true;
                XmlWriter xmlWritter = XmlTextWriter.Create(write_File, xmlWriterSettings);
                xmlDoc.Save(xmlWritter);
                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }
        }

        private void ExportSQL()
        {
            Type tClassProcess = typeof(Export);
            Object obj = tClassProcess.InvokeMember(null, BindingFlags.CreateInstance, null, null, null);
            MethodInfo method = tClassProcess.GetMethod("SQLExport");
            if (null != method)
            {
                string key = "SQL_" + gvTemplate.Title + "_" + gvTemplate.ObjectName;
                string physicalPath = SessionTools.TemporaryDirectory.MapPath(key);
                string relativePath = SessionTools.TemporaryDirectory.Path + key;
                string WindowID = FileTools.GetUniqueName(gvTemplate.Title, gvTemplate.ObjectName);
                string write_File = physicalPath + @"\" + WindowID + ".html";
                string open_File = relativePath + @"/" + WindowID + ".html";

                Object[] args = new Object[] { this, gvTemplate.referential, gvTemplate.DsData.Tables[0], write_File };
                object codeReturn = tClassProcess.InvokeMember(method.Name, BindingFlags.InvokeMethod, null, obj, args, null, null, null);

                AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
            }

        }

        private void ExportZIP()
        {
            bool isOk = false;
            string path = string.Empty;
            string targetFileName = string.Empty;

            #region Ecriture des fichiers dans un répertoire temporaire
            if (gvTemplate.isDataSourceAvailable)
            {
                DataTable tblMain = ((DataView)gvTemplate.DataSource).Table;

                path = FileTools.GetUniqueName("EXPORTZIP", tblMain.TableName);
                path = SessionTools.TemporaryDirectory.MapPath(path);
                path += @"\";

                targetFileName = tblMain.TableName;

                for (int i = 0; i < ArrFunc.Count(tblMain.Rows); i++)
                {
                    DataRow row = tblMain.Rows[i];
                    bool isAddItem = true;
                    if (row.Table.Columns.Contains("ISSELECTED"))
                        isAddItem = Convert.ToBoolean(row["ISSELECTED"]);

                    if (isAddItem)
                    {
                        isAddItem = false;

                        string objectName = null;
                        string columnName_Data = null;
                        string columnName_FileName = null;
                        string columnName_Type = null;
                        string[] keyColumns = null;
                        string[] keyValues = null;
                        string[] keyDatatypes = null;

                        if ((tblMain.TableName == Cst.OTCml_TBL.VW_MCO.ToString()) ||
                            (tblMain.TableName == Cst.OTCml_TBL.VW_INVMCO.ToString()) ||
                            (tblMain.TableName == Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString() ||
                            (tblMain.TableName == "MCO" && isConsultation))
                            )
                        {
                            objectName = tblMain.TableName;
                            if ((gvTemplate.referential.AliasTableName == "mco_rpt") ||
                                (gvTemplate.referential.AliasTableName == "mco_rpt_finper"))
                            {
                                objectName = Cst.OTCml_TBL.VW_MCO_MULTITRADES.ToString();
                            }

                            columnName_FileName = "IDMCO";
                            if (row.Table.Columns.Contains("DOCNAME") && !(row["DOCNAME"] is DBNull))
                                columnName_FileName = "DOCNAME";

                            keyColumns = new string[] { "IDMCO" };
                            keyValues = new string[] { row["IDMCO"].ToString() };
                            keyDatatypes = new string[] { "int" };

                            if (row.Table.Columns.Contains("LOCNFMSGBIN") && !(row["LOCNFMSGBIN"] is DBNull))
                            {
                                isAddItem = true;
                                columnName_Data = "LOCNFMSGBIN";
                                columnName_Type = "DOCTYPEMSGBIN";
                            }
                            else if (row.Table.Columns.Contains("LOCNFMSGTXT") && !(row["LOCNFMSGTXT"] is DBNull))
                            {
                                isAddItem = true;
                                columnName_Data = "LOCNFMSGTXT";
                                columnName_Type = "DOCTYPEMSGTXT";
                            }
                        }
                        else if (tblMain.TableName == Cst.OTCml_TBL.ATTACHEDDOC.ToString())
                        {
                            isAddItem = true;

                            objectName = "ATTACHEDDOC";
                            columnName_FileName = "DOCNAME";

                            columnName_Data = "LODOC";
                            columnName_Type = "DOCTYPE";

                            keyColumns = new string[] { "TABLENAME", "ID", "DOCNAME" };
                            keyValues = new string[] { row["TABLENAME"].ToString(), row["ID"].ToString(), row["DOCNAME"].ToString() };
                            keyDatatypes = new string[] { "string", "int", "string" };
                        }
                        else
                        {
                            throw new Exception("Zip on this referential is not managed");
                        }

                        if (isAddItem)
                        {

                            try
                            {
                                LOColumn dbfc = new LOColumn(CS, objectName,
                                columnName_Data, columnName_FileName, columnName_Type,
                                keyColumns, keyValues, keyDatatypes);

                                dbfc.Load();
                                AspTools.WriteLoColumnFile(dbfc, path);

                                if ((keyColumns[0] == "IDMCO"))
                                {
                                    //FI 20160411 [XXXXX] Ajout du flux XML de messagerie 
                                    //- Facilite la mise à dispo d'exemple dans I:\INST_SPHERES\Reports (exemples-maquettes)
                                    //- Permet au support de récupérer le flux XML et son rendu 
                                    Boolean isAddXmlFlow = (isTrace);
#if DEBUG
                                    isAddXmlFlow = true;
#endif
                                    if (isAddXmlFlow)
                                    {
                                        dbfc = new LOColumn(CS, objectName,
                                            "CNFMSGXML", columnName_FileName, "DOCTYPEMSGTXT",
                                            keyColumns, keyValues, keyDatatypes);
                                        //
                                        dbfc.Load();
                                        AspTools.WriteLoColumnFile(dbfc, path);
                                    }
                                }

                                isOk = true; //dès que 1 fichier est ok => on génère le ZIP
                            }
                            catch (Exception ex)
                            {
                                ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                                MsgForAlertImmediate = "Error on writting File :" + ex.Message;
                            }
                        }
                    }
                }
            }
            #endregion
            //
            #region Génération du fichier ZIP
            string zipFile = string.Empty;
            if (isOk)
            {
                try
                {
                    zipFile = ZipCompress.CompressFolder(path, path, targetFileName);
                }
                catch (Exception ex)
                {
                    isOk = false;
                    MsgForAlertImmediate = "Error on generate Zip File :" + ex.Message;
                }
            }
            #endregion
            //
            #region Ouverture du fichier ZIP
            if (isOk)
            {
                try
                {
                    AspTools.OpenBinaryFile(this, zipFile, Cst.TypeMIME.Application.XZipCompressed, true);
                }
                catch (Exception ex)
                {
                    isOk = false;
                    MsgForAlertImmediate = "Error on open Zip File :" + ex.Message;
                }
            }
            #endregion
            //

        }

        /// <summary>
        /// Exportation PDF avec transformation XSLT
        /// </summary>
        /// <param name="pFileName">Fichier XSLT associé pour la transformation</param>
        /// EG 20130613 Nouveau paramètre pFileName
        /// FI 20160804 [Migration TFS] Modify
        private void ExportPdf(string pFileName)
        {
            if (gvTemplate.isDataAvailable)
            {
                string xsltFile = @"~\GUIOutput\DataGrid\default-landscape-pdf.xsl";
                if (StrFunc.IsFilled(pFileName))
                    xsltFile = pFileName;

                bool isFound = File.Exists(xsltFile);
                if (!isFound)
                {
                    string tmp_xslMapping = string.Empty;
                    try
                    {
                        isFound = SessionTools.NewAppInstance().SearchFile2(SessionTools.CS, xsltFile, ref tmp_xslMapping);
                    }
                    catch { isFound = false; }

                    if (isFound)
                    {
                        xsltFile = tmp_xslMapping;
                    }
                    else
                    {
                        xsltFile = SessionTools.NewAppInstance().GetFilepath(xsltFile);
                        isFound = File.Exists(xsltFile);
                    }
                }

                if (isFound)
                {
                    string key = "PDF_" + gvTemplate.Title + "_" + gvTemplate.ObjectName;
                    string physicalPath = SessionTools.TemporaryDirectory.MapPath(key);
                    string relativePath = SessionTools.TemporaryDirectory.Path + key;
                    string WindowID = FileTools.GetUniqueName(gvTemplate.Title, gvTemplate.ObjectName);

                    DataSet dsReferential = gvTemplate.DsData;
                    gvTemplate.SetTableColumnMapping();

                    StringBuilder sbReferential = new StringBuilder();

                    #region Referential XML Construction
                    TextWriter writer = new StringWriter(sbReferential);
                    XmlTextWriter xw = new XmlTextWriter(writer);
                    dsReferential.WriteXml(xw);
                    writer.Close();

                    XmlDocument xmlReferential = new XmlDocument();
                    XmlNode root = xmlReferential.CreateNode(XmlNodeType.Element, "Referential", "");
                    xmlReferential.AppendChild(root);

                    //Add attribute TableName
                    XmlAttribute attName = xmlReferential.CreateAttribute("TableName");
                    attName.Value = dsReferential.Tables[0].TableName;
                    root.Attributes.Append(attName);

                    //Add attribute timestamp
                    XmlAttribute attTimestamp = xmlReferential.CreateAttribute("timestamp");
                    attTimestamp.Value = OTCmlHelper.GetDateSysToString(CS, DtFunc.FmtISODateTime);
                    root.Attributes.Append(attTimestamp);

                    // Titre
                    XmlNode newRow = xmlReferential.CreateNode(XmlNodeType.Element, "Title", "");
                    XmlNode attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                    attColumnValue.Value = mainTitle.Replace(Cst.HTMLBold, string.Empty).Replace(Cst.HTMLEndBold, String.Empty);
                    newRow.AppendChild(attColumnValue);
                    root.AppendChild(newRow);

                    // SubTitle
                    newRow = xmlReferential.CreateNode(XmlNodeType.Element, "SubTitle", "");
                    attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                    attColumnValue.Value = subTitle;
                    newRow.AppendChild(attColumnValue);
                    root.AppendChild(newRow);

                    //Ajout d'un element data qui contient les dynamicArg
                    if (null != dynamicArgs)
                    {
                        StringDynamicDatas sdds = ConvertDynamicArgs();

                        XmlDocument docDA = new XmlDocument();
                        docDA.LoadXml(sdds.Serialize());

                        XmlNode xmlNode = xmlReferential.ImportNode(docDA.DocumentElement, true);
                        root.AppendChild(xmlNode);
                    }

                    //Ajout des rows
                    XmlDocument xmlDataSet = new XmlDocument();
                    xmlDataSet.LoadXml(sbReferential.ToString());

                    XmlNodeList tableRows = xmlDataSet.SelectNodes(dsReferential.DataSetName + "/" + dsReferential.Tables[0].TableName);
                    foreach (XmlNode tableRow in tableRows)
                    {
                        newRow = xmlReferential.CreateNode(XmlNodeType.Element, "Row", "");
                        root.AppendChild(newRow);

                        XmlNodeList tableRowColumns = tableRow.ChildNodes;

                        foreach (XmlNode column in tableRowColumns)
                        {
                            XmlNode newColumn = xmlReferential.CreateNode(XmlNodeType.Element, "Column", "");
                            // Nom de la colonne
                            XmlAttribute attColumnName = xmlReferential.CreateAttribute("ColumnName");
                            attColumnName.Value = column.Name;
                            newColumn.Attributes.Append(attColumnName);

                            int idxCol = gvTemplate.referential.GetIndexDataGrid(column.Name);
                            if (0 <= idxCol)
                            {
                                ReferentialColumn rrc = gvTemplate.referential[idxCol];
                                if (false == rrc.IsHideInDataGrid)
                                {
                                    // Ressource
                                    if (rrc.RessourceSpecified)
                                    {
                                        attColumnName = xmlReferential.CreateAttribute("RessourceName");
                                        attColumnName.Value = Ressource.GetMulti(rrc.Ressource, 0);
                                        newColumn.Attributes.Append(attColumnName);
                                    }
                                    // DataType
                                    string dataType = "UT_ENUM_MANDATORY";
                                    switch (rrc.DataTypeEnum)
                                    {
                                        case TypeData.TypeDataEnum.@bool:
                                        case TypeData.TypeDataEnum.@bool2h:
                                        case TypeData.TypeDataEnum.@bool2v:
                                        case TypeData.TypeDataEnum.@boolean:
                                            dataType = "UT_BOOLEAN";
                                            break;
                                        case TypeData.TypeDataEnum.@date:
                                            dataType = "UT_DATE";
                                            break;
                                        case TypeData.TypeDataEnum.@datetime:
                                            dataType = "UT_DATETIME";
                                            break;
                                        case TypeData.TypeDataEnum.@dec:
                                        case TypeData.TypeDataEnum.@decimal:
                                            dataType = "UT_VALUE";
                                            break;
                                        case TypeData.TypeDataEnum.@int:
                                        case TypeData.TypeDataEnum.@integer:
                                            dataType = "UT_NUMBER";
                                            break;
                                        case TypeData.TypeDataEnum.@string:
                                            dataType = "UT_IDENTIFIER";
                                            break;
                                    }
                                    attColumnName = xmlReferential.CreateAttribute("DataType");
                                    attColumnName.Value = dataType;
                                    newColumn.Attributes.Append(attColumnName);

                                    XmlNodeList columnValues = column.ChildNodes;
                                    foreach (XmlNode columnValue in columnValues)
                                    {
                                        if (columnValue.NodeType == XmlNodeType.Text)
                                        {
                                            attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                                            attColumnValue.Value = columnValue.Value;
                                            newColumn.AppendChild(attColumnValue);
                                        }
                                    }
                                    newRow.AppendChild(newColumn);
                                }
                            }
                            else
                            {
                                XmlNodeList columnValues = column.ChildNodes;
                                foreach (XmlNode columnValue in columnValues)
                                {
                                    if (columnValue.NodeType == XmlNodeType.Text)
                                    {
                                        attColumnValue = xmlReferential.CreateNode(XmlNodeType.Text, "", "");
                                        attColumnValue.Value = columnValue.Value;
                                        newColumn.AppendChild(attColumnValue);
                                    }
                                }
                                newRow.AppendChild(newColumn);
                            }
                        }
                    }

                    sbReferential = new StringBuilder(xmlReferential.InnerXml);

                    if (isTrace)
                    {
                        string write_xmlFile = physicalPath + @"\" + WindowID + "_DataSet.xml";
                        string open_xmlFile = relativePath + @"/" + WindowID + "_DataSet.xml";
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.LoadXml(sbReferential.ToString());
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_xmlFile);
                        AddScriptWinDowOpenFile(WindowID + "_DataSet", open_xmlFile, string.Empty);
                    }
                    #endregion

                    #region Referential XSLT Transformation
                    Hashtable param = new Hashtable();
                    param.Add("pCurrentCulture", Thread.CurrentThread.CurrentCulture.Name);

                    string transformResult = XSLTTools.TransformXml(sbReferential, xsltFile, param, null);

                    if (isTrace)
                    {
                        string write_xslFile = physicalPath + @"\" + WindowID + "_Transform.xsl";
                        string open_xslFile = relativePath + @"/" + WindowID + "_Transform.xsl";
                        XmlDocument xmldoc = new XmlDocument();
                        xmldoc.Load(xsltFile);
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_xslFile);
                        AddScriptWinDowOpenFile(WindowID + "_Transform", open_xslFile, string.Empty);

                        string write_txtFile = physicalPath + @"\" + WindowID + "_Transform_Result.txt";
                        string open_txtFile = relativePath + @"/" + WindowID + "_Transform_Result.txt";
                        xmldoc = new XmlDocument();
                        xmldoc.LoadXml(transformResult);
                        FileTools.WriteStringToFile(xmldoc.InnerXml, write_txtFile);
                        AddScriptWinDowOpenFile(WindowID + "_Transform_Result", open_txtFile, string.Empty);
                    }
                    #endregion

                    string write_File = physicalPath + @"\" + WindowID + ".pdf";
                    string open_File = relativePath + @"/" + WindowID + ".pdf";
                    if (Cst.ErrLevel.SUCCESS == FopEngine.WritePDF(CS, transformResult, physicalPath, write_File))
                        AddScriptWinDowOpenFile(WindowID, open_File, string.Empty);
                }
            }
        }

        /// <summary>
        /// Export Excel du datagrid ou du jeu de  donnée
        /// </summary>
        private void ExportExcel(ExportExcelType pExporType)
        {
            if (pExporType == ExportExcelType.ExportGrid)
            {
                ExportControlToExcel(gvTemplate, gvTemplate.ObjectName);
            }
            else if (pExporType == ExportExcelType.ExportDataset)
            {
                //FI 20120212 [] add filename in URL 
                string url = StrFunc.AppendFormat("~/MSExcel.aspx?dvMSExcel={0}&filename={1}",
                        gvTemplate.Data_CacheName, gvTemplate.ObjectName);
                Page.Response.Redirect(url, false);
            }
        }

        /// <summary>
        /// Contrôle suite à une demande de traitement
        /// <para>Si la demande est validée, poste une confirmation pour ce traitement </para>
        /// </summary>
        private void CheckAndConfirmRequest()
        {

            DataRow[] rows = (gvTemplate.DataSource as DataView).Table.Select("ISSELECTED=true");
            string msg = string.Empty;
            if (ArrFunc.IsEmpty(rows))
                msg = Ressource.GetString("Msg_TradesNoneSelected"); //Aucune ligne sélectionnée!
            else
                msg = IsNoLockProcess(rows);

            confirmTitle.InnerText = this.mainTitle;

            if (StrFunc.IsEmpty(msg))
            {
                string processMsg = string.Empty;
                if (1 == ArrFunc.Count(rows))
                    processMsg += Ressource.GetString("Msg_Process_ConfirmOne") + Cst.CrLf + Cst.CrLf;
                else
                    processMsg += Ressource.GetString2("Msg_Process_Confirm", ArrFunc.Count(rows).ToString()) + Cst.CrLf + Cst.CrLf;

                
                divConfirmHeader.Attributes.Add("class", "modal-header btn-primary");
                confirmMsg.InnerText = processMsg;
                btnConfirmSubmit.Attributes.Add("onclick", "__doPostBack('" + Target_OnProcessClick + "','" + m_btnRunProcess_Clicked.CommandName + ";" + m_btnRunProcess_Clicked.CommandArgument + "');");
                btnConfirmSubmit.Visible = true;
                btnConfirmCancel.Attributes.Add("class", "btn btn-xs btn-cancel");
                ScriptManager.RegisterStartupScript(this, this.GetType(), "RunProcess", "OpenConfirmation();", true);
            }
            else
            {
                divConfirmHeader.Attributes.Add("class", "modal-header btn-danger");
                confirmMsg.InnerText = msg;
                btnConfirmSubmit.Attributes.Remove("onclick");
                btnConfirmSubmit.Visible = false;
                btnConfirmCancel.Attributes.Add("class", "btn btn-xs btn-apply");
                ScriptManager.RegisterStartupScript(this, this.GetType(), "RunProcess", "OpenAlert();", true);
            }

        }

        private string IsNoLockProcess(DataRow[] pRowsSelected)
        {
            string ret = string.Empty;
            TypeLockEnum typeLock = TypeLockEnum.TRADE;

            Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(gvTemplate.IDMenu);
            if (null != menu)
            {
                switch (menu)
                {
                    case IdMenu.Menu.InvoicingValidation:
                    case IdMenu.Menu.InvoicingCancellation:
                    case IdMenu.Menu.InvoicingGeneration:
                        typeLock = (TypeLockEnum.OTC_INV_BROFEE_PROCESS_GENERATION |
                                    TypeLockEnum.OTC_INV_BROFEE_PROCESS_CANCELLATION |
                                    TypeLockEnum.OTC_INV_BROFEE_PROCESS_VALIDATION);
                        ret = LockTools.SearchProcessLocks(CS, typeLock, LockTools.Exclusive, SessionTools.NewAppInstance());
                        break;
                    case IdMenu.Menu.POSKEEPING_UNCLEARING:
                        ret = LockTools.SearchProcessLocks(CS, TypeLockEnum.ENTITYMARKET, LockTools.Shared, SessionTools.NewAppInstance());
                        break;
                    case IdMenu.Menu.InputTrade_CLEARSPEC:
                        // Sur une clôture spécifique les lignes demandées sont de fait sur la même série donc : LECTURE de la row[0]
                        DataRow row = pRowsSelected[0];
                        Nullable<int> idEM = null;
                        if (row.Table.Columns.Contains("IDEM") && (false == Convert.IsDBNull(row["IDEM"])))
                            idEM = Convert.ToInt32(row["IDEM"]);
                        string identifier = string.Empty;
                        if (row.Table.Columns.Contains("ENTITY_IDENTIFIER") && (false == Convert.IsDBNull(row["ENTITY_IDENTIFIER"])))
                            identifier = row["ENTITY_IDENTIFIER"].ToString();
                        if (row.Table.Columns.Contains("MARKET") && (false == Convert.IsDBNull(row["MARKET"])))
                            identifier += " - " + row["MARKET"].ToString();
                        LockObject lockEntityMarket = new LockObject(TypeLockEnum.ENTITYMARKET, idEM.Value, identifier, LockTools.Exclusive);
                        ret = LockTools.SearchProcessLocks(CS, lockEntityMarket, SessionTools.NewAppInstance());
                        break;
                    case IdMenu.Menu.POSKEEPING_CLEARINGBULK:
                        ret = LockTools.SearchProcessLocks(CS, TypeLockEnum.ENTITYMARKET, LockTools.Exclusive, SessionTools.NewAppInstance());
                        break;
                    default:
                        break;
                }
            }
            return ret;
        }

        /// <summary>
        /// Convertie la property dynamicArgs en StringDynamicDatas
        /// </summary>
        /// <returns></returns>
        private StringDynamicDatas ConvertDynamicArgs()
        {
            StringDynamicDatas ret = null;
            if (null != dynamicArgs)
            {
                List<StringDynamicData> lst = new List<StringDynamicData>();
                IEnumerator listDA = dynamicArgs.Values.GetEnumerator();
                while (listDA.MoveNext())
                {
                    StringDynamicData sdd = (StringDynamicData)listDA.Current;
                    lst.Add(sdd);
                }
                ret = new StringDynamicDatas();
                ret.data = lst.ToArray();
            }
            return ret;
        }


        /// <summary>
        /// Alimentation du log des actions utilisateur si Chargement du grid
        /// </summary>
        /// FI 20141021 [20350] Add
        private void SetRequestTrackBuilderListLoad()
        {
            Boolean isTrack = SessionTools.IsRequestTrackConsultEnabled && gvTemplate.referential.RequestTrackSpecified;

            if (isTrack)
            {
                Boolean isAuto = false;
                if (false == IsPostBack)
                {
                    //Alimentation du log si chgt du grid à l'ouverture ou si auto rafraichissement 
                    isTrack = (gvTemplate.isLoadDataOnStart || AutoRefresh > 0);

                    // Mode auto
                    isAuto = true; //default
                    if (null != Request.UrlReferrer)
                    {
                        // L'ouverture d'un référentiel enfant est considérée manuelle
                        // (exemple consultation des positions détaillées à partir des positions synthéthique)
                        //  test sur Request.UrlReferrer de manière à considérer le cas ou l'utilisateur saisie une URL avec FK
                        if ((Request.UrlReferrer.LocalPath == Request.Url.LocalPath) && StrFunc.IsFilled(Request.QueryString["FK"]))
                            isAuto = false;
                    }
                }
                else
                {
                    // RequestTrackBuilder peut-être déjà être renseigné (ex en cas de suppression d'un enregistrement)
                    isTrack &= (null == RequestTrackBuilder);

                    //si purge du grid => Spheres n'alimente pas le log
                    isTrack &= (false == gvTemplate.isSelfClear);
                }

                if (isTrack)
                {
                    RequestTrackActionEnum action = RequestTrackActionEnum.ListLoad;
                    if (false == IsPostBack)
                    {
                        action = RequestTrackActionEnum.ListLoad;
                    }
                    else
                    {
                        if (null != m_toolbar_Clicked)
                        {
                            if (m_toolbar_Clicked is Control)
                            {
                                switch (((Control)m_toolbar_Clicked).ID)
                                {
                                    case "btnSql":
                                    case "btnZip":
                                    case "btnMSExcel":
                                        action = RequestTrackActionEnum.ListExport;
                                        break;
                                }
                            }
                            else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs)
                            {
                                skmMenu.MenuItemClickEventArgs mnu = (skmMenu.MenuItemClickEventArgs)m_toolbar_Clicked;
                                if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                                    action = RequestTrackActionEnum.ListExport;
                            }
                        }
                        else if (gvTemplate.isSelfReload)
                        {
                            action = RequestTrackActionEnum.ListLoad;
                        }
                        else
                        {
                            Control co = GetPostBackControl();
                            if (null != co)
                            {
                                // Control serveur du grid (Changement de page, zone text qui permet de saisir une page)
                                if (co.ClientID.StartsWith("gvTemplate"))
                                {
                                    action = RequestTrackActionEnum.ListLoad;
                                }
                            }
                            else if (this.__EVENTTARGET.StartsWith("gvTemplate"))
                            {
                                // Control client qui s'appuie sur la publication ASP:javascript:__do_postback
                                // "Afficher sur 1 page" passe ici 
                                action = RequestTrackActionEnum.ListLoad;
                            }
                        }
                    }


                    DataRow[] row = gvTemplate.GetRowPage(true, false);

                    RequestTrackListViewerBuilder builder = new RequestTrackListViewerBuilder();
                    builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(action, isAuto ? RequestTrackActionMode.auto : RequestTrackActionMode.manual);
                    if (builder.action.First == RequestTrackActionEnum.ListExport)
                    {
                        string reportName;
                        builder.exportType = GetRequestTrackExportType(out reportName);
                        builder.reportName = reportName;
                    }

                    builder.isConsultation = gvTemplate.isConsultation;
                    builder.consult = gvTemplate.consult;
                    builder.referential = gvTemplate.referential;
                    builder.parameter = GetCustomObjectParameter();
                    builder.row = row;

                    RequestTrackBuilder = builder;
                }

            }
        }


        /// <summary>
        /// Alimentation du log des actions utilisateur si lancement d'un process
        /// </summary>
        private void SetRequestTrackBuilderListProcess()
        {
            bool isTrack = SessionTools.IsRequestTrackProcessEnabled && gvTemplate.referential.RequestTrackSpecified;
            isTrack &= m_IsProcess;
            isTrack &= IsPostBack;
            isTrack &= (Target_OnProcessClick == __EVENTTARGET);
            isTrack &= (null == RequestTrackBuilder);

            if (isTrack)
            {
                DataRow[] row = gvTemplate.GetRowPage(true, true);
                Nullable<RequestTrackProcessEnum> processType = null;

                Nullable<IdMenu.Menu> menu = IdMenu.ConvertToMenu(gvTemplate.IDMenu);
                if (menu.HasValue)
                {
                    switch (menu)
                    {

                        case IdMenu.Menu.POSKEEPING_UPDATENTRY:
                            processType = RequestTrackProcessEnum.UpdateEntry;
                            break;
                        case IdMenu.Menu.POSKEEPING_CLEARINGBULK:
                            processType = RequestTrackProcessEnum.ClearingBulk;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING:
                            processType = RequestTrackProcessEnum.UnClearing;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING_MOF:
                            processType = RequestTrackProcessEnum.UnClearing_MOF;
                            break;
                        case IdMenu.Menu.POSKEEPING_UNCLEARING_MOO:
                            processType = RequestTrackProcessEnum.UnClearing_MOO;
                            break;
                        // EG 20150722 
                        case IdMenu.Menu.PROCESS_FO_REMOVEALLOC:
                        case IdMenu.Menu.PROCESS_OTC_REMOVEALLOC:
                            processType = RequestTrackProcessEnum.Remove;
                            break;
                    }
                }

                if (null != processType)
                {
                    RequestTrackListViewerBuilder builder = new RequestTrackListViewerBuilder();
                    builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ListProcess, RequestTrackActionMode.manual);
                    builder.processType = processType;
                    builder.isConsultation = gvTemplate.isConsultation;
                    builder.consult = gvTemplate.consult;
                    builder.referential = gvTemplate.referential;
                    builder.parameter = GetCustomObjectParameter();
                    builder.row = row;
                    RequestTrackBuilder = builder;
                }
            }
        }

        /// <summary>
        /// Alimentation du log des actions utilisateur si lancement d'un process sur un trade
        /// </summary>
        private void SetRequestTrackBuilderTradeProcess(int pIdT, RequestTrackProcessEnum pProcessType)
        {
            if (SessionTools.IsRequestTrackProcessEnabled)
            {
                EFS_TradeLibrary trade = new EFS_TradeLibrary(CS, null, pIdT);
                SpheresIdentification tradeIdentificaton = TradeRDBMSTools.GetTradeIdentification(CS, null, pIdT);

                //RequestTrackTradeBuilder builder = new RequestTrackTradeBuilder();
                //builder.dataDocument = trade.dataDocument;
                //builder.tradeIdentIdentification = tradeIdentificaton;
                //builder.processType = pProcessType;
                //builder.action = new Pair<RequestTrackActionEnum, RequestTrackActionMode>(RequestTrackActionEnum.ItemProcess, RequestTrackActionMode.manual);
                //this.RequestTrackBuilder = builder;
            }
        }


        /// <summary>
        /// Retourne le type d'export demandé par l'utilisateur 
        /// <para>Fonction appelée lors du log des actions utilisateur (RequestTrack)</para>
        /// </summary>
        /// <param name="pReportName">Retourne le nom du report lorsque l'export est de type PDF</param>
        /// <returns></returns>
        /// FI 20140519 [19923] Add method
        /// FI 20160804 [Migration TFS] Modify
        private RequestTrackExportType GetRequestTrackExportType(out string pReportName)
        {
            RequestTrackExportType ret = default(RequestTrackExportType);
            pReportName = string.Empty;


            if (null != m_toolbar_Clicked)
            {
                if (m_toolbar_Clicked is Control)
                {
                    switch ((m_toolbar_Clicked as Control).ID)
                    {
                        case "btnSql":
                            ret = RequestTrackExportType.SQL;
                            break;
                        case "btnZip":
                            ret = RequestTrackExportType.ZIP;
                            break;
                        case "btnMSExcel":
                            ret = RequestTrackExportType.XLS;
                            break;
                    }
                }
                else if (m_toolbar_Clicked is skmMenu.MenuItemClickEventArgs)
                {
                    skmMenu.MenuItemClickEventArgs mnu = (skmMenu.MenuItemClickEventArgs)m_toolbar_Clicked;
                    if (mnu.CommandName == Cst.Capture.MenuEnum.Report.ToString())
                    {
                        // FI 20160804 [Migration TFS] New folder
                        switch (mnu.Argument)
                        {
                            case @"~\GUIOutput\DataGrid\FinFlows-pdf.xsl":
                                pReportName = "CashFlows";
                                break;
                            case @"~\GUIOutput\DataGrid\FinFlowsByActor-pdf.xsl":
                                pReportName = "CashFlowsByActor";
                                break;
                            case @"~\GUIOutput\DataGrid\FinStatMember-pdf.xsl":
                                pReportName = "FinStatMember";
                                break;
                            default:
                                pReportName = null;
                                break;
                        }
                    }
                }

            }
            return ret;

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="pProcessType"></param>
        /// <param name="pProcessName"></param>
        /// <param name="pIsProcessAndIO"></param>
        private void CallProcess(Cst.ListProcess.ListProcessTypeEnum pProcessType, string pProcessName, Nullable<Cst.ProcessIOType> pProcessIoType, string pAdditionalArguments)
        {
            DataTable dt = ((DataView)gvTemplate.DataSource).Table;
            #region Only Rows with ISSELECTED=true
            DataRow[] arrRowsToProcess = dt.Select("ISSELECTED=true");
            if ((ArrFunc.Count(arrRowsToProcess) < ArrFunc.Count(dt.Rows)) ||
                gvTemplate.referential.HasEditableColumns)
            {
                DataTable dtToProcess = dt.Clone();
                for (int i = 0; i < ArrFunc.Count(arrRowsToProcess); i++)
                    dtToProcess.LoadDataRow(arrRowsToProcess[i].ItemArray, LoadOption.OverwriteChanges);
                dt = dtToProcess;
            }
            #endregion Only Rows with ISSELECTED=true;

            if (0 < dt.Rows.Count)
            {
                int entityId = 0;
                string entityName = string.Empty;
                DropDownList ddl = this.FindControl(Cst.DDL + "ENTITY") as DropDownList;
                if (null != ddl)
                {
                    entityId = IntFunc.IntValue(ddl.SelectedItem.Value);
                    entityName = ddl.SelectedItem.Text;
                }
                switch (pProcessType)
                {
                    case Cst.ListProcess.ListProcessTypeEnum.Service:
                        if (Enum.IsDefined(typeof(Cst.ProcessTypeEnum), pProcessName.ToUpper()))
                        {
                            CallProcessService((Cst.ProcessTypeEnum)Enum.Parse(typeof(Cst.ProcessTypeEnum), pProcessName, true),
                                entityId, entityName, dt, pProcessIoType);
                        }
                        break;
                    case Cst.ListProcess.ListProcessTypeEnum.ProcessCSharp:
                        CallProcessCsharp(pProcessName, entityName, dt, pAdditionalArguments);
                        break;
                    case Cst.ListProcess.ListProcessTypeEnum.StoredProcedure:
                        CallProcessStoredProcedure(pProcessName);
                        break;
                    default:
                        break;
                }
            }
        }


        /// <summary>
        /// Génération des messages pour les services Spheres®
        /// <para>Poste des messages pour les enregistrements sélectionnés</para>
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <param name="pEntityId"></param>
        /// <param name="pEntityName"></param>
        /// <param name="dt"></param>
        /// FI 20170306 [22225] Modify
        private void CallProcessService(Cst.ProcessTypeEnum pProcessName, int pEntityId, string pEntityName, DataTable pDt,
            Nullable<Cst.ProcessIOType> pProcessIoType)
        {
            int rowsCount = pDt.Rows.Count;

            bool isMonoProcessGenerate = IsMonoProcessGenerate(pProcessName);
            Cst.ProcessTypeEnum[] process = new Cst.ProcessTypeEnum[1] { pProcessName };
            switch (pProcessName)
            {
                case Cst.ProcessTypeEnum.CLOSINGGEN:
                    process = new Cst.ProcessTypeEnum[2] { Cst.ProcessTypeEnum.ACCRUALSGEN, Cst.ProcessTypeEnum.LINEARDEPGEN };
                    break;
                default:
                    break;
            }

            #region Boucle principale
            MQueueTaskInfo taskInfo = null;
            string msgDisplay = string.Empty;
            List<Pair<int, string>> lstInfoForFinalMessage = new List<Pair<int, string>>();

            foreach (Cst.ProcessTypeEnum currentProcessType in process)
            {
                CheckMOM(currentProcessType);
                taskInfo = new MQueueTaskInfo();
                taskInfo.process = currentProcessType;
                taskInfo.tracker = new MQueueTaskInfoTracker(currentProcessType);
                taskInfo.connectionString = CS;
                taskInfo.appInstance = SessionTools.NewAppInstance();

                // RD 20121109 / Utilisation d'une seule liste de paramètres commune
                MQueueparameters listCommonCustomParameter = new MQueueparameters(GetCustomObjectParameter());
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                List<MQueueIdInfo> listIdInfo = new List<MQueueIdInfo>();
                MQueueAttributes mQueueAttributes = null;

                if (isMonoProcessGenerate)
                {
                    #region UN SEUL MESSAGE POSTE
                    taskInfo.tracker.process = currentProcessType;
                    MQueueBase mQueue = null;
                    SQL_Criteria criteria = SQLReferentialData.GetSQLCriteria(CS, gvTemplate.referential);
                    mQueueAttributes = new MQueueAttributes(CS, pEntityId, pEntityName);
                    mQueueAttributes.criteria = criteria;

                    // RD 20121109
                    // Pour eviter de référencer les même objets, Il faut clonner la liste de paramètres commune, car on est dans une boucle,
                    mQueueAttributes.parameters = listCommonCustomParameter.Clone();
                    // EG 20121012 SetDataTracker dans MQueueTools
                    taskInfo.tracker.info = MQueueTools.SetDataTracker(currentProcessType, mQueueAttributes.parameters);
                    if (Cst.ProcessTypeEnum.INVOICINGGEN == SpheresProcessType)
                    {
                        mQueue = new InvoicingGenMQueue(mQueueAttributes);
                        msgDisplay = Ressource.GetString2("Msg_InvoicingPostedMessage",
                            Convert.ToString(taskInfo.tracker.info.Find(match => match.Key.ToString() == "DATA2").Value),
                            Convert.ToString(taskInfo.tracker.info.Find(match => match.Key.ToString() == "DATA1").Value));
                    }
                    /* FI 20170306 [22225] Mise en commentaire
                    else if (Cst.ProcessTypeEnum.FEESCALCULATION == SpheresProcessType)
                    {
                        mQueue = new ActionGenMQueue(mQueueAttributes, SpheresProcessType);
                        msgDisplay = Ressource.GetString2("Msg_FeesCalculationPostedMessage",
                            Convert.ToString(taskInfo.tracker.info.Find(match => match.Key.ToString() == "DATA1").Value),
                            Convert.ToString(taskInfo.tracker.info.Find(match => match.Key.ToString() == "DATA2").Value));
                    }
                     */
                    listMQueue.Add(mQueue);
                    #endregion UN SEUL MESSAGE POSTE
                }
                else
                {
                    #region PLUSIEURS MESSAGES POSTES
                    //Ajout des données DATA1..DATA5 pour TRACKER (Tooltip entre autre)
                    // RD 20121109 / Utilisation d'une seule liste de paramètres commune
                    // EG 20121012 SetDataTracker dans MQueueTools
                    taskInfo.tracker.info = MQueueTools.SetDataTracker(currentProcessType, listCommonCustomParameter);
                    #region Boucle par ligne sélectionnée du Datagrid
                    foreach (DataRow row in pDt.Rows)
                    {
                        mQueueAttributes = new MQueueAttributes(CS);
                        //Ajout des paramètres des customObjects
                        // RD 20121109 / 
                        // Pour eviter de référencer les même objets, Il faut clonner la liste de paramètres commune, car on est dans une boucle,
                        mQueueAttributes.parameters = listCommonCustomParameter.Clone();
                        //Ajout des paramètres additionels (CMGEN, RMGEN, RIMGEN...)
                        mQueueAttributes.AdditionalParameters(currentProcessType, pProcessIoType);

                        MQueueIdInfo idInfo = null;
                        MQueueBase mQueue = null;
                        string identifier = string.Empty;
                        string gProduct = string.Empty;
                        // RD 20121119 / Bug génération des EARs / GPRODUCT manquant dans le message MQueue
                        if (pDt.Columns.Contains("GPRODUCT") && (false == Convert.IsDBNull(row["GPRODUCT"])))
                            gProduct = row["GPRODUCT"].ToString();
                        //if (0 == i)
                        if (pDt.Rows.IndexOf(row) == 0)
                        {
                            taskInfo.tracker.gProduct = gProduct;
                            taskInfo.tracker.caller = gvTemplate.IDMenu;
                        }
                        //
                        #region Set idInfo
                        switch (currentProcessType)
                        {
                            case Cst.ProcessTypeEnum.RIMGEN:
                                #region RIMGEN
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["IDA"]));
                                identifier = row["ACSENDTO_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                string cnfType = row["CNFTYPE"].ToString();
                                if (Enum.IsDefined(typeof(NotificationTypeEnum), cnfType))
                                {
                                    if (null == mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFTYPE])
                                        mQueueAttributes.AddParameter(ReportInstrMsgGenMQueue.PARAM_CNFTYPE, cnfType);
                                    else
                                        mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFTYPE].SetValue(cnfType);
                                }
                                else
                                    throw new Exception(StrFunc.AppendFormat("Notification Type  {0} is not defined", cnfType));

                                string cnfClass = row["CNFCLASS"].ToString();
                                Nullable<NotificationClassEnum> cnfClassEnum = ReflectionTools.ConvertStringToEnumOrNullable<NotificationClassEnum>(cnfClass);
                                if (cnfClassEnum.HasValue)
                                {
                                    if (null == mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFCLASS])
                                        mQueueAttributes.AddParameter(ReportInstrMsgGenMQueue.PARAM_CNFCLASS, cnfClass);
                                    else
                                        mQueueAttributes.parameters[ReportInstrMsgGenMQueue.PARAM_CNFCLASS].SetValue(cnfClass);

                                    if (NotificationClassEnum.MULTITRADES == cnfClassEnum)
                                    {
                                        MQueueparameter parameter = new MQueueparameter(ReportInstrMsgGenMQueue.PARAM_BOOK, TypeData.TypeDataEnum.integer);
                                        parameter.SetValue(Convert.ToInt32(row["IDB"]), row["BOOK_IDENTIFIER"].ToString());
                                        mQueueAttributes.AddParameter(parameter);
                                    }
                                }
                                else
                                    throw new Exception(StrFunc.AppendFormat("Notification Class {0} is not defined", cnfClassEnum.ToString()));

                                mQueue = new ReportInstrMsgGenMQueue(mQueueAttributes);
                                #endregion RIMGEN
                                break;
                            case Cst.ProcessTypeEnum.ESRNETGEN:
                                #region ESRNETGEN
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["NETID"]));
                                identifier = row["NETMETHOD"].ToString();
                                idInfo.idInfos = new DictionaryEntry[] { new DictionaryEntry("nettingmethod", identifier) };
                                #endregion ESRNETGEN
                                break;
                            case Cst.ProcessTypeEnum.MSOGEN:
                                #region MSOGEN
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["IDSTLMESSAGE"]));
                                identifier = row["IDENTIFIER"].ToString();
                                #endregion MSOGEN
                                break;
                            case Cst.ProcessTypeEnum.CMGEN:
                            case Cst.ProcessTypeEnum.RMGEN:
                                #region CMGEN / RMGEN
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["IDMCO"]));
                                #endregion CMGEN / RMGEN
                                break;
                            case Cst.ProcessTypeEnum.RISKPERFORMANCE:
                            case Cst.ProcessTypeEnum.CASHBALANCE:
                                #region RISKPERFORMANCE / CASHBALANCE
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["ENTITY_IDA"]));
                                identifier = row["ENTITY_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                //PL 20120521 New feature ----------------------------------------------------
                                //RD 20120627 Code déplacé ici, pour avoir en premier le paramètre PARAM_DATE1
                                //ainsi dans la table PROCESS_L on aura le paramètre PARAM_DATE1 dans la colonne DATA1
                                //car dans la vue VW_MARGINTRACK, on s'attend à PARAM_DATE1 dans DATA1
                                //cette vue est utilisée par la consultation "Logs: Déposit" 
                                // PM 20150520 [20575] Remplacement de DTMARKET par DTENTITY
                                ////DateTime dtBusiness = Convert.ToDateTime(row["DTMARKET"]);
                                DateTime dtBusiness = Convert.ToDateTime(row["DTENTITY"]);
                                mQueueAttributes.AddParameter(RiskPerformanceMQueue.PARAM_DTBUSINESS, dtBusiness);
                                //----------------------------------------------------------------------------
                                if (Cst.ProcessTypeEnum.RISKPERFORMANCE == currentProcessType)
                                {
                                    MQueueparameter parameter = new MQueueparameter(RiskPerformanceMQueue.PARAM_CSSCUSTODIAN, TypeData.TypeDataEnum.integer);
                                    parameter.SetValue(Convert.ToInt32(row["CSS_IDA"]), row["CSS_IDENTIFIER"].ToString());
                                    mQueueAttributes.AddParameter(parameter);
                                }
                                mQueue = new RiskPerformanceMQueue(currentProcessType, mQueueAttributes);
                                #endregion RISKPERFORMANCE / CASHBALANCE
                                break;

                            case Cst.ProcessTypeEnum.CASHINTEREST:
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["CBO_IDA"]));
                                identifier = row["CBO_IDENTIFIER"].ToString();

                                mQueueAttributes.id = idInfo.id;
                                mQueueAttributes.identifier = identifier;
                                mQueueAttributes.idInfo = idInfo;

                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_AMOUNTTYPE, row["AMOUNTTYPE"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_IDC, row["IDC"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_PERIOD, row["PERIOD"].ToString());
                                mQueueAttributes.AddParameter(CashBalanceInterestMQueue.PARAM_PERIODMLTP, Convert.ToInt32(row["PERIODMLTP"]));

                                mQueue = new CashBalanceInterestMQueue(mQueueAttributes);
                                break;

                            default:
                                #region Autres services
                                idInfo = new MQueueIdInfo(Convert.ToInt32(row["IDT"]));
                                identifier = row["IDENTIFIER"].ToString();
                                string idDataIdent = "TRADE";
                                if (Cst.ProductGProduct_ADM == gProduct)
                                    idDataIdent = "TRADEADMIN";
                                else if (Cst.ProductGProduct_ASSET == gProduct)
                                    idDataIdent = "DEBTSECURITY";
                                else if (Cst.ProductGProduct_RISK == gProduct)
                                    idDataIdent = "TRADERISK";

                                idInfo.idInfos = new DictionaryEntry[]{new DictionaryEntry("IDDATA", idInfo.id),
                                                                   new DictionaryEntry("IDDATAIDENT", idDataIdent),
                                                                   new DictionaryEntry("IDDATAIDENTIFIER", identifier),
                                                                   new DictionaryEntry("GPRODUCT", gProduct)};

                                if (1 == rowsCount)
                                {
                                    mQueueAttributes.id = idInfo.id;
                                    mQueueAttributes.identifier = identifier;
                                    // RD 20121119 / Bug génération des EARs / GPRODUCT manquant dans le message MQueue
                                    mQueueAttributes.idInfo = idInfo;
                                    List<DictionaryEntry> infoTracker = new List<DictionaryEntry>();
                                    infoTracker.Add(new DictionaryEntry("IDDATA", idInfo.id));
                                    infoTracker.Add(new DictionaryEntry("IDDATAIDENT", idDataIdent));
                                    infoTracker.Add(new DictionaryEntry("IDDATAIDENTIFIER", identifier));
                                    if (false == taskInfo.tracker.info.Exists(match => match.Key.ToString() == "DATA1"))
                                        infoTracker.Add(new DictionaryEntry("DATA1", identifier));
                                    taskInfo.tracker.info.AddRange(infoTracker);

                                    switch (currentProcessType)
                                    {
                                        case Cst.ProcessTypeEnum.ACCOUNTGEN:
                                            mQueue = new AccountGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EARGEN:
                                            mQueue = new EarGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EVENTSGEN:
                                            mQueue = new EventsGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.EVENTSVAL:
                                            mQueue = new EventsValMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.MTMGEN:
                                            mQueue = new MarkToMarketGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.LINEARDEPGEN:
                                            mQueue = new LinearDepGenMQueue(mQueueAttributes);
                                            break;
                                        case Cst.ProcessTypeEnum.ACCRUALSGEN:
                                            mQueue = new AccrualsGenMQueue(mQueueAttributes);
                                            break;
                                    }
                                }
                                #endregion Autres services
                                break;
                        }
                        if (lstInfoForFinalMessage.Count < 5)
                        {
                            Pair<int, string> display = null;
                            if (null != mQueue)
                            {
                                if (mQueue.identifierSpecified && mQueue.idSpecified)
                                {
                                    if (false == lstInfoForFinalMessage.Exists(match => match.First == mQueue.id))
                                        display = new Pair<int, string>(mQueue.id, mQueue.identifier);
                                }
                                else if ((mQueue.idInfoSpecified) && StrFunc.IsFilled(identifier))
                                {
                                    if ((false == lstInfoForFinalMessage.Exists(match => match.First == mQueue.idInfo.id)))
                                        display = new Pair<int, string>(mQueue.idInfo.id, identifier);
                                }
                            }
                            else if ((null != idInfo) && StrFunc.IsFilled(identifier))
                            {
                                if ((false == lstInfoForFinalMessage.Exists(match => match.First == idInfo.id)))
                                    display = new Pair<int, string>(idInfo.id, identifier);
                            }
                            if (null != display)
                            {
                                if (4 == lstInfoForFinalMessage.Count)
                                    display.Second += "...";
                                lstInfoForFinalMessage.Add(display);
                            }
                        }
                        #endregion Set idInfo
                        //
                        if (null != mQueue)
                            listMQueue.Add(mQueue);
                        else if (null != idInfo)
                            listIdInfo.Add(idInfo);
                    }
                    #endregion Boucle par ligne sélectionnée du Datagrid
                    #endregion PLUSIEURS MESSAGES POSTES
                }

                #region Postage du(des) message(s)
                if (ArrFunc.IsFilled(listMQueue))
                {
                    taskInfo.mQueue = listMQueue.ToArray();
                }
                else if (ArrFunc.IsFilled(listIdInfo))
                {
                    taskInfo.idInfo = listIdInfo.ToArray();
                    if (ArrFunc.IsFilled(mQueueAttributes.parameters.parameter))
                        taskInfo.mQueueParameters = new MQueueparameters(mQueueAttributes.parameters.parameter);
                }
                SendTaskInfoMqueue(taskInfo);
                #endregion Postage du(des) message(s)
            }
            #endregion Boucle principale

            #region Message de réponse
            string msgReturn = "Msg_PROCESS_GENERATE_" + ((1 == rowsCount) || isMonoProcessGenerate ? "MONO" : "MULTI");
            int nbSend = 0;
            if (null != taskInfo.idInfo)
                nbSend = ArrFunc.Count(taskInfo.idInfo);
            else if (null != taskInfo.mQueue)
                nbSend = ArrFunc.Count(taskInfo.mQueue);

            if (0 < lstInfoForFinalMessage.Count)
                lstInfoForFinalMessage.ForEach(display => { msgDisplay += display.Second + ", "; });

            MsgForAlertImmediate = Ressource.GetString2(msgReturn, nbSend.ToString(), msgDisplay);
            #endregion Message de réponse
        }


        /// <summary>
        /// Appel à un traitement en c# qui s'applique sur le jeu de résultat {dt} 
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <param name="pEntityName"></param>
        /// <param name="dt"></param>
        /// FI 20130423 [18601] Refactor
        /// EG 20151019 [21465] Refactoring (Par defaut si non spécifiés dans URL : className = "EfsML.Business.PosKeepingTools" et assemblyName = "EFS.EfsML";
        /// FI 20170616 [XXXXX] Modify
        private void CallProcessCsharp(string pProcessName, string pEntityName, DataTable dt, string pAdditionalArguments)
        {
            string msgReturn = string.Empty;
            object codeReturn = null;

            try
            {
                string methodName = pProcessName;
                switch (methodName)
                {
                    case "AddPosRequestRemoveAlloc":
                        codeReturn = AddPosRequestRemoveAlloc(dt);
                        break;
                    // FI 20170616 [XXXXX]  add UpdateTrade 
                    // => Doit être utilisé par EFS uniquement (fonctionnalité cachée) 
                    case "UpdateTrade":
                        codeReturn = ProcessUpdateTrade(dt);
                        break;
                    case "CalcUTI":
                    case "CalcTradeUTI":
                        codeReturn = CalcUTI(CSTools.SetCacheOn(CS), dt);
                        break;
                    case "CalcPositionUTI":
                        codeReturn = CalcPositionUTI(CSTools.SetCacheOn(CS), dt);
                        break;
                    default:
                        Object[] args = null;
                        Type tClassProcess = null;
                        bool isInvoke = IsInvokeMember(pProcessName, pEntityName, pAdditionalArguments, dt, out tClassProcess, out  methodName, out args);
                        if (isInvoke)
                        {
                            codeReturn = ReflectionTools.InvokeMethod(tClassProcess, methodName.Trim(), args);
                            SizeWidthForAlertImmediate = new Pair<int?, int?>(null, null);
                            if ((tClassProcess.Equals(typeof(PosKeepingTools)) && (methodName.Trim() == "AddPosRequestClearingSPEC")))
                            {
                                int idTSource = Convert.ToInt32(((MQueueparameters)args[2])["IDT"].Value);
                                SetRequestTrackBuilderTradeProcess(idTSource, RequestTrackProcessEnum.ClearingSpecific);
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                msgReturn = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
                msgReturn += ex.Message + Cst.CrLf;
                if (null != ex.InnerException)
                    msgReturn += ex.InnerException.Message;
            }
            finally
            {
                if (StrFunc.IsEmpty(msgReturn))
                {
                    if (codeReturn == null)
                    {
                        msgReturn = Ressource.GetString("Msg_ProcessUndone");
                        CloseAfterAlertImmediate = true;
                    }
                    else if (codeReturn.GetType().Equals(typeof(Cst.ErrLevelMessage)))
                    {
                        msgReturn = ((Cst.ErrLevelMessage)codeReturn).Message;
                        if (Cst.ErrLevel.SUCCESS != ((Cst.ErrLevelMessage)codeReturn).ErrLevel)
                            ErrLevelForAlertImmediate = ProcessStateTools.StatusErrorEnum;
                        else
                            CloseAfterAlertImmediate = true;
                    }
                    else if (codeReturn.GetType().Equals(typeof(Cst.ErrLevel)))
                    {
                        if (Cst.ErrLevel.SUCCESS == (Cst.ErrLevel)codeReturn)
                        {
                            msgReturn = Ressource.GetString("Msg_ProcessSuccessfull");
                            CloseAfterAlertImmediate = true;
                        }
                        else
                            msgReturn = Ressource.GetString("Msg_ProcessIncomplete");
                    }
                }
                MsgForAlertImmediate = msgReturn;
            }
        }


        private void CallProcessStoredProcedure(string pProcessName)
        {

            Cst.ErrLevel ret = Cst.ErrLevel.UNDEFINED;
            int returnValue = 0;
            string msgReturn = string.Empty;
            try
            {
                ret = SQLUP.RunUP(out returnValue, SessionTools.CS, pProcessName,
                    gvTemplate.consult.template.IDLSTCONSULT,
                    gvTemplate.consult.template.IDLSTTEMPLATE,
                    gvTemplate.consult.template.IDA);
                if (Cst.ErrLevel.SUCCESS == ret)
                    msgReturn = Ressource.GetString("Msg_ProcessSuccessfull");
                else
                    msgReturn = Ressource.GetString("Msg_ProcessIncomplete");
            }
            catch { msgReturn = Ressource.GetString("Msg_ProcessUndone"); }
            finally
            {
                MsgForAlertImmediate = msgReturn;
            }
        }


        /// <summary>
        /// Retourne true lorsque le grid affiche les enregistrements dans le grid sans offire la possibilité de choisir certains enregistrement via les checks
        /// </summary>
        /// <param name="pProcessName"></param>
        /// <returns></returns>
        /// FI 20170306 [22225] Modify
        private static Boolean IsMonoProcessGenerate(Cst.ProcessTypeEnum pProcessName)
        {
            Boolean ret = false;

            switch (pProcessName)
            {
                case Cst.ProcessTypeEnum.INVOICINGGEN:
                    ret = true;
                    break;
            }
            return ret;
        }


        /// <summary>
        ///  Génération des messages Mqueue
        /// </summary>
        /// <param name="pTaskInfo"></param>
        /// FI 20130423 [18601] Add Method
        private static void SendTaskInfoMqueue(MQueueTaskInfo pTaskInfo)
        {
            bool isThreadPool = (bool)SystemSettings.GetAppSettings("ThreadPool", typeof(System.Boolean), true);
            if (isThreadPool)
            {
                AutoResetEvent autoResetEvent = new AutoResetEvent(false);
                pTaskInfo.handle = ThreadPool.RegisterWaitForSingleObject(autoResetEvent,
                    new WaitOrTimerCallback(MQueueTools.SendMultiple), pTaskInfo, 1000, false);
                Thread.Sleep(1000);
                autoResetEvent.Set();
            }
            else
            {
                MQueueTools.SendMultiple(pTaskInfo, false);
            }
        }

        /// <summary>
        /// Calcul des UTI sur des trades qui en sont dépourvus.
        /// <para>(avec calcul du PUTI dans la foulée)</para>
        /// </summary>
        /// <param name="pDataTable">Contient les enregistrements candidats</param>
        /// <returns></returns>
        private static Cst.ErrLevelMessage CalcUTI(string pCS, DataTable pDataTable)
        {
            return UTITools.CalcAndRecordUTI(pCS, UTIType.UTI, pDataTable, SessionTools.NewAppInstance().IdA);
        }

        /// <summary>
        /// Calcul des PUTI sur des trades qui en sont dépourvus.
        /// </summary>
        /// <param name="pDataTable">Contient les enregistrements candidats</param>
        /// <returns></returns>
        private static Cst.ErrLevelMessage CalcPositionUTI(string pCS, DataTable pDataTable)
        {
            return UTITools.CalcAndRecordUTI(pCS, UTIType.PUTI, pDataTable, SessionTools.NewAppInstance().IdA);
        }

        /// <summary>
        /// Retourne la classe, la méthode et les arguments qu'il est nécessaire d'invoquer pour traiter le process {processName}
        /// </summary>
        /// <param name="processName">Nom du process</param>
        /// <param name="entityName"></param>
        /// <param name="dt">Jeu de résultat (données cochées sur le grid)</param>
        /// <param name="tClassProcess">Représente la class</param>
        /// <param name="methodName">Représente la méthode</param>
        /// <param name="args">Représente les arguments à passer à la méthode</param>
        /// <returns></returns>
        /// FI 20130423 [18601] add Method
        /// EG 20151019 [21465] Passage du nom de la méthode C# seule (pour alleger URL) => (Par defaut : EfsML.Business.PosKeepingTools est utilisée comme classe)
        private bool IsInvokeMember(string processName, string entityName, string pAdditionalArguments, DataTable dt,
                            out Type tClassProcess, out string methodName, out Object[] args)
        {
            bool isInvoke = false;

            tClassProcess = null;
            methodName = processName;
            args = null;

            if (Software.IsSoftwareVision())
            {
                isInvoke = true;
                //tClassProcess = typeof(Vision);
                args = new Object[] {this, 
										 gvTemplate.referential.TableName,
										 gvTemplate.consult.template.IDA,
										 gvTemplate.consult.template.IDLSTTEMPLATE,
                                         gvTemplate.consult.template.IDLSTCONSULT.Trim()};
            }
            else if (Software.IsSoftwarePortal())
            {
                isInvoke = true;
                //tClassProcess = typeof(Portal);
            }
            else if (Software.IsSoftwareSpheres())
            {
                // EG 20151019 [21465] New
                string[] methodInfo = processName.Split('|');
                string className = string.Empty;
                string assemblyName = string.Empty;

                if (1 < methodInfo.Length)
                {
                    className = methodInfo[0];
                    assemblyName = string.Empty;
                    if (2 == methodInfo.Length)
                    {
                        assemblyName = methodInfo[0].Substring(0, methodInfo[0].LastIndexOf("."));
                        methodName = methodInfo[1];
                    }
                    else
                    {
                        assemblyName = methodInfo[1];
                        methodName = methodInfo[2];
                    }
                    isInvoke = true;
                    tClassProcess = Type.GetType(className + "," + assemblyName, true, false);
                }
                else if (1 == methodInfo.Length)
                {
                    // EG 20151019 [21465] Simplification d'appel de méthode via URL Menu
                    className = "EfsML.Business.PosKeepingTools"; // en dur
                    assemblyName = "EFS.EfsML"; // en dur
                    methodName = methodInfo[0];
                    isInvoke = true;
                    tClassProcess = typeof(EfsML.Business.PosKeepingTools); // Type.GetType(className + "," + assemblyName, true, false);
                }

                if (isInvoke)
                {
                    //tClassProcess = Type.GetType(className + "," + assemblyName, true, false);
                    MQueueparameters parameters = new MQueueparameters(GetCustomObjectParameter());
                    // EG 20141211 Ajout des arguments additionnels
                    if (StrFunc.IsFilled(pAdditionalArguments))
                        args = new Object[] { CS, dt, parameters, entityName, pAdditionalArguments };
                    else
                        args = new Object[] { CS, dt, parameters, entityName };
                }
            }

            return isInvoke;
        }


        /// <summary>
        /// Demande d'annulation d'une allocation Spheres® 
        ///  <para>- annule le trade (le trade est reserved en sortie)</para>
        ///  <para>- génère une entrée dans POSREQUEST</para>
        /// </summary>
        /// <param name="pRow">Repésente le trade</param>
        /// <param name="posRequest">Repésente la demande d'annulation générée</param>
        /// <param name="mQueue">Repésente le message queue pour traiter la demande d'annulation par les services</param>
        /// <param name="pTradeIdentifier">Représente l'identifier du trade traité</param>
        /// FI 20130419 [18601] 
        /// EG 20200226 [25077] RDBMS : New version of Trades tables architecture (TRADEINSTRUMENT (INSTRUMENTNO=1) to TRADE)
        private ProcessRemoveAllocReturn ProcessRemoveTradeAlloc(DataRow pRow, out IPosRequest posRequest, out PosKeepingMQueue mQueue, out string pTradeIdentifier)
        {
            IDbTransaction dbTransaction = null;
            try
            {
                pTradeIdentifier = string.Empty;
                posRequest = null;
                mQueue = null;

                //if (false == efsdtgRefeferentiel.localLstConsult.IsConsultation(LstConsult.ConsultEnum.TRADEFnO_ALLOC))
                //    throw new Exception(StrFunc.AppendFormat("{0} is not valid , please use Consultation TRADEFnO_ALLOC", efsdtgRefeferentiel.localLstConsult.IdLstConsult));

                int indexDataKeyField = gvTemplate.referential.IndexColSQL_DataKeyField;
                if (false == (indexDataKeyField > -1))
                    throw new Exception("Consultation without DataKeyField");

                string columnDataKeyField = gvTemplate.referential.Column[indexDataKeyField].DataField;
                if (false == pRow.Table.Columns.Contains(columnDataKeyField))
                    throw new Exception(StrFunc.AppendFormat("Consultation without column {0}", columnDataKeyField));

                int idT = Convert.ToInt32(pRow[columnDataKeyField]);
                if (false == idT > 0)
                    throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not valid", idT));

                TradeCaptureGen tradeCaptureGen = new TradeCaptureGen();
                bool isLoad = tradeCaptureGen.Load(SessionTools.CS, null, idT, Cst.Capture.ModeEnum.RemoveAllocation, SessionTools.User, SessionTools.SessionID, true);
                if (false == isLoad)
                    throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not loaded", idT));

                TradeInput tradeInput = tradeCaptureGen.Input;
                pTradeIdentifier = tradeInput.identification.identifier;

                ProcessRemoveAllocReturn ret = ProcessRemoveAllocReturn.Ok;
                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    if (false == tradeInput.IsEventsGenerated)
                        ret = ProcessRemoveAllocReturn.NoEvent;
                }
                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    if (tradeInput.IsTradeRemoved)
                        ret = ProcessRemoveAllocReturn.AlreadyRemoved;
                }
                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    if (false == tradeInput.TradeStatus.IsCurrentStUsedBy_Regular)
                        ret = ProcessRemoveAllocReturn.Reserved;
                }
                if (ret == ProcessRemoveAllocReturn.Ok)
                {
                    CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo(SessionTools.User, SessionTools.NewAppInstance(), SessionTools.License, null);

                    TradeRecordSettings recordSettings = new TradeRecordSettings();
                    recordSettings.displayName = tradeInput.identification.displayname;
                    recordSettings.description = tradeInput.identification.description;
                    recordSettings.extLink = tradeInput.identification.extllink;

                    recordSettings.idScreen = tradeInput.SQLLastTradeLog.ScreenName;
                    recordSettings.isLogProcess = isTrace;
                    recordSettings.isCheckValidationRules = false;
                    recordSettings.isCheckValidationXSD = false;
                    recordSettings.isCheckLicense = false;
                    recordSettings.isCheckActionTuning = true;

                    dbTransaction = DataHelper.BeginTran(SessionTools.CS);

                    string identifier = tradeInput.Identifier;
                    // FI 20170404 [23039] Utilisation de underlying et trader
                    Pair<int, string>[] underlying = null;
                    Pair<int, string>[] trader = null;

                    ProcessLog processLog = null;
                    tradeCaptureGen.CheckAndRecord(SessionTools.CS, dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), Cst.Capture.ModeEnum.RemoveAllocation, captureSessionInfo, recordSettings,
                        ref identifier, ref idT, out underlying, out  trader, 0, out processLog);

                    posRequest = tradeInput.NewPostRequest(SessionTools.CS, dbTransaction, Cst.Capture.ModeEnum.RemoveAllocation);
                    PosKeepingTools.FillPosRequest(SessionTools.CS, dbTransaction, posRequest, SessionTools.NewAppInstance());

                    //PL 20151229 Use DataHelper.CommitTran()
                    //dbTransaction. Commit();
                    DataHelper.CommitTran(dbTransaction);

                    mQueue = PosKeepingTools.BuildPosKeepingRequestMQueue(SessionTools.CS, posRequest, null);
                }
                return ret;
            }
            catch
            {
                if (null != dbTransaction)
                {
                    //PL 20151229 Use DataHelper.RollbackTran()
                    //dbTransaction. Rollback();
                    DataHelper.RollbackTran(dbTransaction);
                }
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }

        /// <summary>
        ///  Passe en modification sur l'ensemble des trades 
        ///  <para>Méthode qui peut être utilisé pour réinitailiser tous les évènements des trades d'une journée</para>
        ///  <para>=> Doit être utilisé par EFS uniquement (fonctionnalité cachée) </para>
        /// </summary>
        /// <param name="pDataTable"></param>
        /// <returns></returns>
        private Cst.ErrLevelMessage ProcessUpdateTrade(DataTable pDataTable)
        {

            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            if (pDataTable.Rows.Count > 0)
            {
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                string succesMessage = string.Empty;
                string errorMessage = string.Empty;

                foreach (DataRow dr in pDataTable.Rows)
                {
                    MQueueBase queue = null;
                    UpdateTrade(dr, out queue);
                    if (null != queue)
                        listMQueue.Add(queue);
                }

                //Envoi des messages Mqueue générés
                if (ArrFunc.IsFilled(listMQueue))
                {
                    MQueueTaskInfo taskInfo = new MQueueTaskInfo();
                    taskInfo.connectionString = CS;
                    taskInfo.process = Cst.ProcessTypeEnum.TRADECAPTURE;
                    taskInfo.appInstance = SessionTools.NewAppInstance();
                    taskInfo.mQueue = listMQueue.ToArray();
                    taskInfo.tracker = new MQueueTaskInfoTracker(Cst.ProcessTypeEnum.TRADECAPTURE);
                    taskInfo.tracker.caller = "UpdateTrade";

                    int idTRK_L = 0;
                    MQueueTools.SendMultiple(taskInfo, false, ref idTRK_L);
                }
            }
            return ret;
        }

        /// <summary>
        /// Passe en modification sur un trade
        /// </summary>
        /// <param name="pRow"></param>
        /// FI 20170616 [XXXXX] Add Method
        private void UpdateTrade(DataRow pRow, out MQueueBase pMqueue)
        {
            IDbTransaction dbTransaction = null;
            try
            {

                int indexDataKeyField = gvTemplate.referential.IndexColSQL_DataKeyField;
                if (false == (indexDataKeyField > -1))
                    throw new Exception("Consultation without DataKeyField");

                string columnDataKeyField = gvTemplate.referential.Column[indexDataKeyField].DataField;
                if (false == pRow.Table.Columns.Contains(columnDataKeyField))
                    throw new Exception(StrFunc.AppendFormat("Consultation without column {0}", columnDataKeyField));

                int idT = Convert.ToInt32(pRow[columnDataKeyField]);
                if (false == (idT > 0))
                    throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not valid", idT));

                TradeCaptureGen tradeCaptureGen = new TradeCaptureGen();
                bool isLoad = tradeCaptureGen.Load(SessionTools.CS, null, idT, Cst.Capture.ModeEnum.Update, SessionTools.User, SessionTools.SessionID, true);
                if (false == isLoad)
                    throw new Exception(StrFunc.AppendFormat("Trade (id:{0}) is not loaded", idT));

                TradeInput tradeInput = tradeCaptureGen.Input;

                CaptureSessionInfo captureSessionInfo = new CaptureSessionInfo(SessionTools.User, SessionTools.NewAppInstance(), SessionTools.License, null);

                TradeRecordSettings recordSettings = new TradeRecordSettings();
                recordSettings.displayName = tradeInput.identification.displayname;
                recordSettings.description = tradeInput.identification.description;
                recordSettings.extLink = tradeInput.identification.extllink;

                recordSettings.idScreen = tradeInput.SQLLastTradeLog.ScreenName;
                recordSettings.isLogProcess = isTrace;
                recordSettings.isCheckValidationRules = false;
                recordSettings.isCheckValidationXSD = false;
                recordSettings.isCheckLicense = false;
                recordSettings.isCheckActionTuning = true;

                dbTransaction = DataHelper.BeginTran(SessionTools.CS);

                string identifier = tradeInput.Identifier;
                // FI 20170404 [23039] Utilisation de underlying et trader
                Pair<int, string>[] underlying = null;
                Pair<int, string>[] trader = null;

                ProcessLog processLog = null;
                tradeCaptureGen.CheckAndRecord(SessionTools.CS, dbTransaction, IdMenu.GetIdMenu(IdMenu.Menu.InputTrade), Cst.Capture.ModeEnum.Update, captureSessionInfo, recordSettings,
                    ref identifier, ref idT, out underlying, out  trader, 0, out processLog);

                DataHelper.CommitTran(dbTransaction);

                MQueueIdInfo idInfo = new MQueueIdInfo(tradeInput.IdT);
                idInfo.idInfos = new DictionaryEntry[] { new DictionaryEntry("GPRODUCT", tradeInput.SQLProduct.GProduct) };

                MQueueAttributes mQueueAttributes = new MQueueAttributes();
                mQueueAttributes.connectionString = CS;
                mQueueAttributes.idInfo = idInfo;
                mQueueAttributes.id = tradeInput.IdT;
                mQueueAttributes.identifier = tradeInput.Identifier;

                MQueueBase mQueue = null;
                mQueue = new EventsGenMQueue(mQueueAttributes);
                mQueue.parameters[EventsGenMQueue.PARAM_DELEVENTS].SetValue(tradeInput.IsInstrumentEvents());


                pMqueue = mQueue;

            }
            catch
            {
                if (null != dbTransaction)
                {
                    DataHelper.RollbackTran(dbTransaction);
                }
                throw;
            }
            finally
            {
                if (null != dbTransaction)
                    dbTransaction.Dispose();
            }
        }


        /// <summary>
        ///  Pour chaque trade présent dans le jeu de résultat, Spheres® 
        ///  <para>- annule le trade</para>
        ///  <para>- génère une entrée dans POSREQUEST</para>
        ///  <para>- Envoi un message au service pour traiter le POSREQUEST d'annulation</para>
        /// </summary>
        /// <param name="pDt">Représente les enregistrements sélectionnés</param>
        /// FI 20130423 [18601] Add Method 
        private Cst.ErrLevelMessage AddPosRequestRemoveAlloc(DataTable pDataTable)
        {
            Cst.ErrLevelMessage ret = new Cst.ErrLevelMessage(Cst.ErrLevel.SUCCESS, string.Empty);

            if (pDataTable.Rows.Count > 0)
            {
                List<MQueueBase> listMQueue = new List<MQueueBase>();
                string succesMessage = string.Empty;
                string errorMessage = string.Empty;

                foreach (DataRow dr in pDataTable.Rows)
                {
                    string tradeIdentifier = string.Empty;
                    IPosRequest posRequest = null;
                    PosKeepingMQueue mQueue = null;
                    ProcessRemoveAllocReturn result = ProcessRemoveTradeAlloc(dr, out posRequest, out mQueue, out tradeIdentifier);

                    if (result == ProcessRemoveAllocReturn.Ok)
                    {
                        StrFunc.BuildStringListElement(ref succesMessage, tradeIdentifier, 4);
                        listMQueue.Add(mQueue);
                    }
                    else
                    {
                        string err = string.Empty;
                        switch (result)
                        {
                            case ProcessRemoveAllocReturn.AlreadyRemoved:
                                err = StrFunc.AppendFormat("{0} ({1})", tradeIdentifier, Ressource.GetString("lblAlreadyRemoved"));
                                break;
                            case ProcessRemoveAllocReturn.Reserved:
                                err = StrFunc.AppendFormat("{0} ({1})", tradeIdentifier, Ressource.GetString("lblAlreadyReserved"));
                                break;
                            case ProcessRemoveAllocReturn.NoEvent:
                                err = StrFunc.AppendFormat("{0} ({1})", tradeIdentifier, Ressource.GetString("lblNoEvent"));
                                break;
                            default:
                                throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", result.ToString()));
                        }
                        StrFunc.BuildStringListElement(ref errorMessage, err, -1);
                    }
                }

                //Envoi des messages Mqueue générés
                if (ArrFunc.IsFilled(listMQueue))
                {
                    MQueueTaskInfo taskInfo = new MQueueTaskInfo();
                    taskInfo.process = Cst.ProcessTypeEnum.POSKEEPREQUEST;
                    taskInfo.tracker = new MQueueTaskInfoTracker(Cst.ProcessTypeEnum.POSKEEPREQUEST);
                    taskInfo.connectionString = CS;
                    taskInfo.appInstance = SessionTools.NewAppInstance();
                    taskInfo.tracker.gProduct = Cst.ProductGProduct_FUT;
                    taskInfo.tracker.caller = gvTemplate.IDMenu;
                    taskInfo.mQueue = listMQueue.ToArray();

                    SendTaskInfoMqueue(taskInfo);
                }

                int nbMessageSendToService = ArrFunc.Count(listMQueue);
                int nbMessageNotSendToService = pDataTable.Rows.Count - ArrFunc.Count(listMQueue);
                if (nbMessageSendToService > 0 && nbMessageNotSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.SUCCESS;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_GENERATE_NOGENERATE_MULTI", nbMessageSendToService.ToString(), succesMessage, nbMessageNotSendToService.ToString(), errorMessage);
                }
                else if (nbMessageSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.SUCCESS;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_GENERATE_DATA", nbMessageSendToService.ToString(), succesMessage);
                }
                else if (nbMessageNotSendToService > 0)
                {
                    ret.ErrLevel = Cst.ErrLevel.FAILURE;
                    ret.Message = Ressource.GetString2("Msg_PROCESS_NOGENERATE_DATA", nbMessageNotSendToService.ToString(), errorMessage);
                }
            }
            else
            {
                ret.ErrLevel = Cst.ErrLevel.FAILURE;
                ret.Message = Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf;
            }

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// FI 20130417 [18596] Modify(add ExportExcel) 
        /// FI 20160308 [21782] Modify 
        protected void OnToolbar_Click(object sender, System.EventArgs e)
        {
            /// EG 20130613 Changement de cast (suite à Export PDF)
            m_toolbar_Clicked = (Control)sender;

            if (gvTemplate.AllowCustomPaging)
            {
                //Pas de pagination customisée, la source de donnée est chargée en totalité 
                gvTemplate.pagingType = PagingTypeEnum.NativePaging;
                //Il faut reloader la source de données du GridView
                gvTemplate.isLoadData = true;
            }
            else
            {
                //20091029 FI [download File] Add else pour ne pas reloader lorsque que GridView est déjà totalement chargé
                gvTemplate.isLoadData = false;
            }

            // FI 20130417 [18596] SetPaging -1 pour que le grid charge toute les lignes
            // EG 20130613 Changement de cast (suite à Export PDF)
            if (((Control)m_toolbar_Clicked).ID == "btnMSExcel")
            {
                gvTemplate.SetPaging(-1);
                gvTemplate.isModeExportExcel = true;
            }
        }

        /// <summary>
        /// Retourne true si La page affiche le boutton Exportation SQL
        /// </summary>
        /// <returns></returns>
        private bool isDisplaySQLInsertCommand()
        {
            bool ret = false;

            ret = (SessionTools.Collaborator_IDENTIFIER.ToUpper() == "SYSADM" || SessionTools.Collaborator_IDENTIFIER.ToUpper() == "EFS");
            if ((!ret))
            {
                //PL 20170214 Newness
                ret = SessionTools.Collaborator.isActorSysAdmin
                      && (bool)SystemSettings.GetAppSettings("SQLExportAllowed", typeof(System.Boolean), false);
            }
#if DEBUG
            ret = true;
#endif
            ret &= ((string.Empty + Request.QueryString[Cst.ListType.Repository.ToString()]).ToString().Length > 0);

            return ret;
        }



        #region CheckMOM
        /// <summary>
        /// Vérification Accessibilité 
        /// du folder (si Mode = FileWatcher) 
        /// de la queue (si Mode = MSMQ) 
        /// </summary>
        /// <param name="pProcess"></param>
        private void CheckMOM(Cst.ProcessTypeEnum pProcess)
        {
            MOMSettings settings = MOMSettings.LoadMOMSettings(pProcess);
            string folder = ServiceTools.GetQueueSuffix(CS, Cst.Process.GetService(pProcess), SessionTools.Collaborator_PARENT_IDA);

            int attemps = 0;
            switch (settings.MOMType)
            {
                case Cst.MOM.MOMEnum.FileWatcher:
                    folder = StrFunc.AppendFormat(@"{0}\{1}", settings.MOMPath, folder);
                    FileTools.CheckFolder(folder, 1, out attemps);
                    break;
                case Cst.MOM.MOMEnum.MSMQ:
                    folder = StrFunc.AppendFormat(@"{0}{1}", settings.MOMPath, folder);
                    MQueueTools.GetMsMQueue(folder, 1, ref attemps);
                    break;
                default:
                    throw new NotImplementedException(StrFunc.AppendFormat("{0} is not implemented", settings.MOMType.ToString()));
            }
        }
        #endregion CheckMOM
        #endregion Methods
    }
}