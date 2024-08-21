using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Process;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{

    public partial class RunGateBCSPage : RunCommonPage
    {
        #region OnInit
        protected override  void OnInit(EventArgs e)
        {
            _title = "Flow_Title";
            InitializeComponent();
            base.OnInit(e);

            if (IsPostBack)
            {
                string currentFlow = Request.Form["ddlFlow"].ToString();
                if ("ddlFlow" != Page.Request.Params["__EVENTTARGET"])
                    LoadInfoDatas(currentFlow, false, false);
                else
                    ResetInfoDatas(false);
            }
        }
        #endregion OnInit
        #region InitializeComponent
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        #endregion InitializeComponent
        #region Page_Load
        protected void Page_Load(object sender, System.EventArgs e)
        {
            if (this.FindControl("ddlFlow") is DropDownList ddlFlow)
            {
                if (false == IsPostBack)
                {
                    // Load ddlFlow
                    QueryParameters queryParameters = GetFlowsSqlQuery(SessionTools.CS);
                    //PL 20141017 this
                    ControlsTools.DDLLoad(this, ddlFlow, "DISPLAY", "VALUE", CSTools.SetCacheOn(SessionTools.CS), queryParameters.Query, true, true, true, null, queryParameters.Parameters.GetArrayDbParameter());
                    // Charger les Paramètres
                    LoadInfoDatas(ddlFlow.SelectedItem.Value);
                }
                // Set Focus to ddlFlow
                JavaScript.SetInitialFocus(ddlFlow);
            }
        }
        #endregion Page_Load

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnProcessClick(object sender, System.EventArgs e)
        {
            SendGateBCSMQueue();
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void SendGateBCSMQueue()
        {
            if (FindControl("ddlFlow") is DropDownList ddlFlow)
            {
                if (StrFunc.IsFilled(ddlFlow.SelectedItem.Value))
                {
                    string flowName = ddlFlow.SelectedItem.Value;
                    GateBCSMQueue.BCSFlowType flowType = GateBCSMQueue.BCSFlowFromName(flowName);
                    int idFlow = (int)flowType;

                    MQueueAttributes mQueueAttributes = new MQueueAttributes()
                    {
                        connectionString = SessionTools.CS,
                        id = idFlow,
                        identifier = flowName,
                        idInfo = new IdInfo()
                        {
                            id = idFlow,
                            idInfos = new DictionaryEntry[] {
                                                        new DictionaryEntry("ident", "BCSTASK"),
                                                        new DictionaryEntry(GateBCSMQueue.INFO_IDENTIFIER, flowName)}
                        },
                        parameters = GetMQueueParameters(flowType)
                    };

                    MQueueTaskInfo taskInfo = new MQueueTaskInfo
                    {
                        process = Cst.ProcessTypeEnum.GATEBCS,
                        connectionString = SessionTools.CS,
                        trackerAttrib = new TrackerAttributes()
                        {
                            process = Cst.ProcessTypeEnum.GATEBCS
                        }
                    };
                    taskInfo.trackerAttrib.info = new List<DictionaryEntry>();

                    List<DictionaryEntry> infoTracker = new List<DictionaryEntry>
                    {
                        new DictionaryEntry("IDDATA", idFlow),
                        new DictionaryEntry("IDDATAIDENT", "BCSTASK"),
                        new DictionaryEntry("IDDATAIDENTIFIER", flowName)
                    };
                    if (false == taskInfo.trackerAttrib.info.Exists(match => match.Key.ToString() == "DATA1"))
                        infoTracker.Add(new DictionaryEntry("DATA1", flowName));
                    taskInfo.trackerAttrib.info.AddRange(infoTracker);

                    taskInfo.mQueue = new MQueueBase[1] { new GateBCSMQueue(mQueueAttributes) };
                    taskInfo.Session = SessionTools.AppSession;

                    var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo);
                    if (!isOk)
                        throw new SpheresException2("MQueueTaskInfo.SendMultiple", errMsg);

                    JavaScript.DialogImmediate(this, Ressource.GetString2("Msg_PROCESS_GENERATE_GATEBCS", ddlFlow.SelectedItem.Text, ddlFlow.SelectedItem.Value));
                }
            }
        }
        
        #region GetMQueueParameters
        private MQueueparameters GetMQueueParameters(GateBCSMQueue.BCSFlowType pFlowType)
        {
            MQueueparameters parameters = null;
            switch (pFlowType)
            {
                case GateBCSMQueue.BCSFlowType.Assignments:
                case GateBCSMQueue.BCSFlowType.Contracts:
                case GateBCSMQueue.BCSFlowType.ContractTransfers:
                case GateBCSMQueue.BCSFlowType.EarlyExercises:
                case GateBCSMQueue.BCSFlowType.Reports:
                    // RD 20121109 Pas besoin de passer le nombre de paramètres
                    // ils sont directement créés par la méthode MQueueparameters.Add()
                    parameters = new MQueueparameters();
                    parameters.Add(new MQueueparameter(MQueueBase.PARAM_DATE1, TypeData.TypeDataEnum.date));
                    if (FindControl("DATA0") is WCTextBox ctrl)
                    {
                        DateTime dtValue = new DtFunc().StringToDateTime(ctrl.Text);
                        if (DtFunc.IsDateTimeFilled(dtValue))
                            parameters.parameter[0].SetValue(dtValue);
                    }
                    break;
                case GateBCSMQueue.BCSFlowType.ExByEx:
                case GateBCSMQueue.BCSFlowType.ExerciseAtExpiry:
                case GateBCSMQueue.BCSFlowType.Series:
                case GateBCSMQueue.BCSFlowType.None:
                default:
                    break;
            }
            return parameters;
        }
        #endregion GetMQueueParameters
        #region OnSelectFlowChange
        private void OnSelectFlowChange(object sender, EventArgs e)
        {
            if (this.FindControl("ddlFlow") is DropDownList ddlFlow)
            {
                // Nettoyer les Task Datas précédentes
                ResetInfoDatas(true);
                // Charger les Params							
                LoadInfoDatas(ddlFlow.SelectedItem.Value);
            }
        }
        #endregion OnSelectFlowChange

        #region AddDetailControls
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected override void AddDetailControls(Panel pPanelParent)
        {
            Panel pnlDetail = new Panel() { ID = "divflow", CssClass = CSSMode + " " + _mainMenuClassName };

            Panel pnlFlowType = new Panel();
            WCTooltipLabel lblFlowType = new WCTooltipLabel()
            {
                ID = "FlowType",
                Text = Ressource.GetString("FlowType"),
                Width = Unit.Pixel(50)
            };
            pnlFlowType.Controls.Add(lblFlowType);

            WCDropDownList2 ddlFlow = new WCDropDownList2()
            {
                ID = "ddlFlow",
                CssClass = EFSCssClass.DropDownListCapture,
                AutoPostBack = true
            };
            ddlFlow.SelectedIndexChanged += new EventHandler(this.OnSelectFlowChange);
            pnlFlowType.Controls.Add(ddlFlow);

            pnlDetail.Controls.Add(pnlFlowType);

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " " + _mainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString(_title));
            togglePanel.AddContent(pnlDetail);

            pPanelParent.Controls.Add(togglePanel);
        }
        #endregion AddDetailControls

        #region GetFlowsSqlQuery
        private static QueryParameters GetFlowsSqlQuery(string pCS)
        {
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(pCS, "CODE", DbType.AnsiString, 64), "GateBCSFlowEnum");
            parameters.Add(new DataParameter(pCS, "ISENABLED", DbType.Boolean), true);

            string sqlQuery = SQLCst.SELECT + "e.VALUE as VALUE, e.VALUE as DISPLAY" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.ENUM.ToString() + " e" + Cst.CrLf;
            sqlQuery += SQLCst.WHERE + "(e.CODE=@CODE)" + SQLCst.AND + "(e.ISENABLED=@ISENABLED)" + Cst.CrLf;
            sqlQuery += SQLCst.ORDERBY + "e.VALUE";
            QueryParameters qryParameters = new QueryParameters(pCS, sqlQuery, parameters);
            return qryParameters;
        }
        #endregion GetFlowsSqlQuery

        #region LoadInfoDatas
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected override void LoadInfoDatas(string pKey, bool pSetData, bool pEnableViewState)
        {
            base.LoadInfoDatas(pKey, pSetData, pEnableViewState);

            if (StrFunc.IsFilled(pKey))
            {
                HttpSessionStateTools.Set(SessionTools.SessionState, "Parameters", pKey);
                GateBCSMQueue.BCSFlowType flowType = GateBCSMQueue.BCSFlowFromName(pKey);
                WCTogglePanel pnlParameters = this.FindControl("divparameters") as WCTogglePanel;
                if (null != pnlParameters)
                    pnlParameters.SetHeaderTitle(Ressource.GetString("RequestParameters"));

                Table tblParameters = this.FindControl("tblParameters") as Table;
                switch (flowType)
                {
                    case GateBCSMQueue.BCSFlowType.Assignments:
                    case GateBCSMQueue.BCSFlowType.Contracts:
                    case GateBCSMQueue.BCSFlowType.ContractTransfers:
                    case GateBCSMQueue.BCSFlowType.EarlyExercises:
                    case GateBCSMQueue.BCSFlowType.Reports:

                        Table tblParametersFlow = HtmlTools.CreateTable(4);
                        tblParametersFlow.ID = "tblParameters" + pKey;
                        TableRow tr = new TableRow();
                        TableCell td = new TableCell();
                        td.Controls.Add(tblParametersFlow);
                        tr.Controls.Add(td);
                        tblParameters.Controls.Add(tr);

                        // La ligne Parameters
                        Table pTable = HtmlTools.CreateTable();
                        tr = new TableRow();
                        WCTogglePanel pnl = new WCTogglePanel(Color.Transparent, Ressource.GetString("Parameters"), "size5", true)
                        {
                            CssClass = CSSMode + " " + _mainMenuClassName,
                        };
                        pnl.AddContent(pTable);
                        tr.Controls.Add(HtmlTools.NewControlInCell(pnl));
                        tblParametersFlow.Controls.Add(tr);

                        // Les lignes de paramètres Parameter
                        Table paramTable = HtmlTools.CreateTable();
                        paramTable.Style.Add(HtmlTextWriterStyle.Margin, "10px");
                        paramTable.ID = "tbl_PARAM0";

                        ParameterRow pr = new ParameterRow(this)
                        {
                            RowNumber = 0,
                            DataType = TypeData.TypeDataEnum.date,
                            DisplayName = Ressource.GetString("Date", true),
                            InfoValue = Ressource.GetString("BUSINESSDATE"),
                            DataValue = DateTime.Today.AddDays(-1).ToString(DtFunc.FmtDateyyyyMMdd)
                        };
                        tr = pr.ConstructParameterRow(pSetData);
                        paramTable.Controls.Add(tr);

                        tr = new TableRow();
                        td = new TableCell();
                        td.Controls.Add(paramTable);
                        tr.Cells.Add(td);
                        pTable.Controls.Add(tr);
                        pnlParameters.Visible = true;
                        break;
                    case GateBCSMQueue.BCSFlowType.ExByEx:
                    case GateBCSMQueue.BCSFlowType.ExerciseAtExpiry:
                    case GateBCSMQueue.BCSFlowType.Series:
                    case GateBCSMQueue.BCSFlowType.None:
                    default:
                        pnlParameters.Visible = false;
                        break;
                }
            }
        }
        #endregion LoadInfoDatas
    }
}
