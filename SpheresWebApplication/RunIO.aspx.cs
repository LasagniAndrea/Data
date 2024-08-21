using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Log;
using EFS.Common.MQueue;
using EFS.Common.Web;
using EFS.Process;
using EFS.Referential;
using EFS.Restriction;
using EFS.Rights;
using EfsML.Business;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace EFS.Spheres
{
    /// <summary>
    /// Formuliare de lancement d'une tâche IO
    /// </summary>
    public partial class RunIOPage : RunCommonPage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            _title = "Task_Title";
            InitializeComponent();
            base.OnInit(e);

            if (IsPostBack)
            {
                string currentTask = Request.Form["ddlTask"].ToString();
                if (("ddlTask" != Page.Request.Params["__EVENTTARGET"]) &&
                    ("ddlTaskType" != Page.Request.Params["__EVENTTARGET"]))
                    LoadInfoDatas(currentTask, false, false);
                else
                    ResetInfoDatas(false);
            }
        }

        
        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20210222 [XXXXX] Suppression inscription function PostBack et OnCtrlChanged
        protected void Page_Load(object sender, System.EventArgs e)
        {
            if ((this.FindControl("ddlTaskType") is DropDownList ddlTaskType) && (this.FindControl("ddlTask") is DropDownList ddlTask))
            {
                if (false == IsPostBack)
                {
                    //Load ddlTaskType
                    ControlsTools.DDLLoad_In_Out(ddlTaskType, false, false);
                    string idIOTask = Request.QueryString["IDIOTask"];
                    if (StrFunc.IsFilled(idIOTask))
                    {
                        DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(SessionTools.CS, null, Convert.ToInt32(idIOTask));
                        if (null != sqlIOTask)
                        {
                            ControlsTools.DDLSelectByValue(ddlTaskType, sqlIOTask.InOut);
                            LoadTask(ddlTask, ddlTaskType.SelectedItem.Value);
                            ControlsTools.DDLSelectByValue(ddlTask, idIOTask);
                            LoadInfoDatas(idIOTask);
                        }
                    }
                    else
                        LoadTask(ddlTask, ddlTaskType.SelectedItem.Value);
                }
                // Set Focus to ddlTask
                JavaScript.SetInitialFocus(ddlTask);
                
                ddlTaskType.Attributes.Add("onchange", "OnCtrlChanged('" + ddlTaskType.UniqueID + "','');");
                ddlTask.Attributes.Add("onchange", "OnCtrlChanged('" + ddlTask.UniqueID + "','');");
            }
           
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPreRender(EventArgs e)
        {
            if (this.FindControl("ddlTask") is DropDownList ddlTask)
            {
                for (int i = 0; i < ddlTask.Items.Count; i++)
                {
                    if (StrFunc.IsFilled(ddlTask.Items[i].Value))
                    {
                        int idTask = Convert.ToInt32(ddlTask.Items[i].Value);
                        if (idTask > 0)
                        {
                            DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(SessionTools.CS, null, Convert.ToInt32(idTask));
                            if (null != sqlIOTask)
                            {
                                string color = GetColorName(sqlIOTask.CSSFilename);
                                if (StrFunc.IsFilled(color))
                                    ddlTask.Items[i].Attributes.CssStyle[HtmlTextWriterStyle.Color] = color;
                            }
                        }
                    }
                }

                if (ddlTask.SelectedIndex > -1 && StrFunc.IsFilled(ddlTask.Items[ddlTask.SelectedIndex].Value))
                    SetColorPanel(Convert.ToInt32(ddlTask.SelectedItem.Value));
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void OnProcessClick(object sender, System.EventArgs e)
        {
            if (this.FindControl("ddlTask") is DropDownList ddlTask)
            {
                if (StrFunc.IsFilled(ddlTask.SelectedItem.Value))
                {
                    SpheresIdentification taskIdent = new SpheresIdentification
                    {
                        OTCmlId = Convert.ToInt32(ddlTask.SelectedItem.Value)
                    };

                    // PL 20220614 Newness: Manage return value for error
                    var (isOk, errMsg) = SendIOMQueue(SessionTools.CS, taskIdent, GetMQueueParameters(), SessionTools.AppSession);
                    if (isOk)
                        JavaScript.DialogImmediate(this, Ressource.GetString2("Msg_PROCESS_GENERATE_IOTASK", taskIdent.Identifier, taskIdent.Displayname));
                    else
                        JavaScript.DialogImmediate(this, Ressource.GetString("Msg_ProcessUndone") + Cst.CrLf + Cst.HTMLHorizontalLine + errMsg, false, ProcessStateTools.StatusEnum.ERROR);
                }
            }
        }

        /// <summary>
        /// Retourne les paramètres (Spheres® effectue la lecture des contrôles de la page)
        /// </summary>
        /// <returns></returns>
        /// FI 20190116 [21916] Modify
        private MQueueparameters GetMQueueParameters()
        {
            MQueueparameters parameters = null;
            DataRow[] drParams = HttpSessionStateTools.Get(SessionTools.SessionState, "Parameters") as DataRow[];
            if (ArrFunc.IsFilled(drParams))
            {
                // RD 20121109 Pas besoin de passer le nombre de paramètres
                // ils sont directement créés par la méthode MQueueparameters.Add()
                parameters = new MQueueparameters();
                foreach (DataRow drParam in drParams)
                {
                    MQueueparameter parameter = new MQueueparameter(drParam["IDIOPARAMDET"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    drParam["DISPLAYNAME"].ToString(),
                                                    (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum),
                                                    drParam["DATATYPE"].ToString(), true))
                    {
                        direction = drParam["DIRECTION"].ToString()
                    };
                    if (DataHelper.IsParamDirectionOutput(parameter.direction) || DataHelper.IsParamDirectionInputOutput(parameter.direction))
                    {
                        if (StrFunc.IsFilled(drParam["RETURNTYPE"].ToString()))
                            parameter.ReturnType = (Cst.ReturnSPParamTypeEnum)Enum.Parse(typeof(Cst.ReturnSPParamTypeEnum),
                                drParam["RETURNTYPE"].ToString(), true);
                    }

                    #region Set parameter Value
                    string inputValue = string.Empty;
                    //Si le paramètre est caché ou sa direction est 'Output' il faut chercher sa valeur par défaut (depuis la table IOPARAMDET)
                    bool isParamHide = (!Convert.IsDBNull(drParam["ISHIDEONGUI"]) && BoolFunc.IsTrue((drParam["ISHIDEONGUI"]).ToString()));
                    if (isParamHide || DataHelper.IsParamDirectionOutput(parameter.direction))
                    {
                        inputValue = Convert.ToString(drParam["DEFAULTVALUE"]); // drParams[i]["DEFAULTVALUE"] est de type string
                    }
                    //PL 20190319 Newness - TIP: Initialisation d'une VALEUR PAR DEFAUT à partir d'un ordre SQL 
                    //else if (StrFunc.IsFilled(drParam["LISTRETRIEVAL"].ToString()))
                    else if (StrFunc.IsFilled(drParam["LISTRETRIEVAL"].ToString()) && (drParam["DEFAULTVALUE"].ToString() != "LISTRETRIEVAL"))
                    {
                        if (FindControl("DATA" + parameter.id) is DropDownList ddlListRet)
                            inputValue = ddlListRet.SelectedValue;
                    }
                    else if (TypeData.IsTypeBool(parameter.dataType))
                    {
                        if (FindControl("DATA" + parameter.id) is CheckBox ckbParams)
                            inputValue = ckbParams.Checked ? "true" : "false";
                    }
                    else
                    {
                        if (FindControl("DATA" + parameter.id) is WCTextBox txtboxParams)
                            inputValue = txtboxParams.Text;
                    }

                    try
                    {
                        switch (parameter.dataType)
                        {
                            case TypeData.TypeDataEnum.@bool:
                            case TypeData.TypeDataEnum.boolean:
                                parameter.SetValue(BoolFunc.IsTrue(inputValue));
                                break;
                            case TypeData.TypeDataEnum.date:
                            case TypeData.TypeDataEnum.datetime:
                            case TypeData.TypeDataEnum.time:
                                DateTime dtValue = new DtFunc().StringToDateTime(inputValue);
                                if (DtFunc.IsDateTimeFilled(dtValue))
                                    parameter.SetValue(dtValue);
                                break;
                            case TypeData.TypeDataEnum.integer:
                                parameter.SetValue(Convert.ToInt32(inputValue));
                                break;
                            case TypeData.TypeDataEnum.@decimal:
                                parameter.SetValue(Convert.ToDecimal(inputValue));
                                break;
                            default:
                                parameter.SetValue(inputValue);
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        // Fonctions qui seront interprétés dans les process
                        if (StrFunc.ContainsIn(inputValue.ToUpper(), "SPHERESLIB") || StrFunc.ContainsIn(inputValue.ToUpper(), "SQL"))
                            parameter.Value = inputValue;
                    }
                    #endregion Set parameter Value

                    parameters.Add(parameter);
                }
            }
            // FI 20190116 [21916] passage du mode DEBUG en tant que paramètre
            if (IsDebug)
            {
                MQueueparameter parameter = new MQueueparameter("ISDEBUG", TypeData.TypeDataEnum.boolean);
                parameter.SetValue(true);
                parameters.Add(parameter);
            }
            return parameters;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// EG 20201029 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) New
        /// EG 20201029 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Gestion CSS Color sur la tâche sélectionnée 
        private void OnSelectTaskTypeChange(object sender, EventArgs e)
        {
            if (((DropDownList)this.FindControl("ddlTaskType") is DropDownList ddlTaskType) && ((DropDownList)this.FindControl("ddlTask") is DropDownList ddlTask))
            {
                // Nettoyer les Task Datas précédentes
                ResetInfoDatas(true);
                LoadTask(ddlTask, ddlTaskType.SelectedItem.Value);
            }
        }
        /// <summary>
        /// Changement de la couleur des panels en fonction du CSSColor spécifié sur la tâche <paramref name="idIOTask"/>
        /// </summary>
        /// EG 20201029 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) New
        private void SetColorPanel(int idIOTask)
        {
            if (FindControl("divbody") is WebControl ctrl)
            {
                string color = _mainMenuClassName;
                DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(SessionTools.CS, null, idIOTask);
                if (null != sqlIOTask)
                {
                    string colorTask = GetColorName(sqlIOTask.CSSFilename);
                    if (StrFunc.IsFilled(colorTask))
                        color = colorTask;
                }
                SetColor(ctrl, color);
            }
        }

        /// <summary>
        /// Changement de la couleur des contrôles enfants de <paramref name="pCtrlMain"/> 
        /// </summary>
        /// <param name="pCtrlMain">Control parent</param>
        /// <param name="pColor">Nouvelle couleur</param>
        private void SetColor(WebControl pCtrlMain, string pColor)
        {
            foreach (var panel in pCtrlMain.Controls.OfType<WebControl>())
            {
                if (panel.CssClass.Contains(CSSMode))
                    panel.CssClass = CSSMode + " " + pColor.ToLower();
                SetColor(panel, pColor);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectTaskChange(object sender, EventArgs e)
        {
            if (this.FindControl("ddlTask") is DropDownList ddlTask)
            {
                // Charger les Task Datas
                LoadInfoDatas(ddlTask.SelectedItem.Value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="panel"></param>
        /// EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected override void AddDetailControls(Panel panel)
        {
            Panel pnlDetail = new Panel() { ID = "divtask", CssClass = CSSMode + " " + _mainMenuClassName };

            Panel pnlTaskType = new Panel();
            WCTooltipLabel lblTaskType = new WCTooltipLabel() 
            { 
                ID = "TASKTYPE",
                Text = Ressource.GetString("IN_OUT"),
                Width = Unit.Pixel(50)
            };
            pnlTaskType.Controls.Add(lblTaskType);

            WCDropDownList2 ddlTaskType = new WCDropDownList2()
            {
                ID = "ddlTaskType",
                CssClass = EFSCssClass.DropDownListCapture,
                AutoPostBack = false
            };
            ddlTaskType.SelectedIndexChanged += new EventHandler(this.OnSelectTaskTypeChange);
            pnlTaskType.Controls.Add(ddlTaskType);

            Panel pnlTaskName = new Panel();
            WCTooltipLabel lblTaskName = new WCTooltipLabel()
            {
                ID = "IDIOTASK",
                Text = Ressource.GetString("IDIOTASK"),
                Width = Unit.Pixel(50)
            };
            pnlTaskName.Controls.Add(lblTaskName);

            WCDropDownList2 ddlTaskName = new WCDropDownList2()
            {
                ID = "ddlTask",
                CssClass = EFSCssClass.DropDownListCapture,
                AutoPostBack = false
            };
            ddlTaskName.SelectedIndexChanged += new EventHandler(this.OnSelectTaskChange);
            pnlTaskName.Controls.Add(ddlTaskName);

            WCTooltipLabel lblInfoDesc = new WCTooltipLabel()
            {
                ID = "lblInfoDesc",
                CssClass = EFSCssClass.Msg_Information,
            };
            pnlTaskName.Controls.Add(lblInfoDesc);

            pnlDetail.Controls.Add(pnlTaskType);
            pnlDetail.Controls.Add(pnlTaskName);

            WCTogglePanel togglePanel = new WCTogglePanel() { CssClass = CSSMode + " " + _mainMenuClassName };
            togglePanel.SetHeaderTitle(Ressource.GetString(_title));
            togglePanel.AddContent(pnlDetail);

            panel.Controls.Add(togglePanel);
        }

        /// <summary>
        /// Retourne la requête de chargement des tâches IO
        /// </summary>
        /// <param name="taskType">type de tâche</param>
        /// <returns></returns>
        private static QueryParameters GetQueryIoTask(string taskType)
        {
            //FI 20100728 [17103] Les tâches IO accessibles sont disponibles dans SESSIONRESTRICT
            RestrictionIOTask restrictionIoTask = new RestrictionIOTask(SessionTools.User);
            
            DataParameters parameters = new DataParameters();
            if (StrFunc.IsFilled(taskType))
                parameters.Add(new DataParameter(SessionTools.CS, "TASKTYPE", DbType.AnsiString, 64), taskType);

            string sqlQuery = SQLCst.SELECT + "iot.IDIOTASK, iot.IDENTIFIER, iot.DISPLAYNAME, a.DISPLAYNAME as OWNER," + Cst.CrLf;
            sqlQuery += "iot.IDA, iot.RIGHTPUBLIC, iot.RIGHTDESK, iot.RIGHTDEPARTMENT, iot.RIGHTENTITY, iot.CSSFILENAME" + Cst.CrLf;
            sqlQuery += SQLCst.FROM_DBO + Cst.OTCml_TBL.IOTASK.ToString() + " iot" + Cst.CrLf;
            sqlQuery += SQLCst.INNERJOIN_DBO + Cst.OTCml_TBL.ACTOR.ToString() + " a on a.IDA=iot.IDA" + Cst.CrLf;
            if (!restrictionIoTask.IsAllElementEnabled)
            {
                SessionRestrictHelper srh = new SessionRestrictHelper(SessionTools.User, SessionTools.SessionID, true);
                sqlQuery += srh.GetSQLIOTask(string.Empty, "iot.IDIOTASK");
                srh.SetParameter(SessionTools.CS, sqlQuery, parameters);
            }
            sqlQuery += SQLCst.WHERE + OTCmlHelper.GetSQLDataDtEnabled(SessionTools.CS, "iot") + Cst.CrLf;

            if (parameters.Contains("TASKTYPE"))
                sqlQuery += "And iot.IN_OUT= @TASKTYPE";

            sqlQuery += SQLCst.ORDERBY + "iot.IDENTIFIER DESC";
            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlQuery, parameters);
            return qryParameters;
        }

        /// <summary>
        /// Charger la DDL <paramref name="DDLTask"/> avec les Tâches IO de type <paramref name="taskType"/>
        /// </summary>
        /// <param name="DDLTask"></param>
        /// <param name="taskType"></param>
        private void LoadTask(DropDownList DDLTask, string taskType)
        {

            DDLTask.Items.Clear();

            QueryParameters qry = GetQueryIoTask(taskType);

            DataSet ds = DataHelper.ExecuteDataset(CSTools.SetCacheOn(qry.Cs), CommandType.Text, qry.Query, qry.Parameters.GetArrayDbParameter());
            DataTable dtDDL = ds.Tables[0];
            dtDDL.TableName = "IOTASK";

            for (int numRow = dtDDL.Rows.Count - 1; numRow > -1; numRow--)
            {

                //getting rights for Task
                LstConsult.LstTemplate template = new LstConsult.LstTemplate
                {
                    IDA = Convert.ToInt32(dtDDL.Rows[numRow]["IDA"]),
                    RIGHTENTITY = dtDDL.Rows[numRow]["RIGHTENTITY"].ToString(),
                    RIGHTDESK = dtDDL.Rows[numRow]["RIGHTDESK"].ToString(),
                    RIGHTDEPARTMENT = dtDDL.Rows[numRow]["RIGHTDEPARTMENT"].ToString(),
                    RIGHTPUBLIC = dtDDL.Rows[numRow]["RIGHTPUBLIC"].ToString()
                };

                bool isAllowed = template.HasUserRight(SessionTools.CS, SessionTools.User, RightsTypeEnum.VIEW);
                if (isAllowed)
                {
                    // RD 20100924 / Utilisation des ressources dans DISPLAYNAME et DESCRIPTION
                    string taskIdentifier = dtDDL.Rows[numRow]["IDENTIFIER"].ToString();
                    string taskDisplayname = dtDDL.Rows[numRow]["DISPLAYNAME"].ToString();
                    string taskOwner = dtDDL.Rows[numRow]["OWNER"].ToString();

                    
                    // RD 20120316 / Utilisation des ressources via SYSTEMMSG
                    string dataText = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), taskIdentifier, null);
                    if (taskIdentifier != taskDisplayname)
                        dataText += " - " + LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), taskDisplayname, null);
                    dataText += " [" + taskOwner + "]";

                    string dataValue = dtDDL.Rows[numRow]["IDIOTASK"].ToString();


                    ListItem item = new ListItem(dataText, dataValue);
                    DDLTask.Items.Add(item);
                }
            }

            DDLTask.Items.Insert(0, new ListItem(string.Empty, string.Empty));
            DDLTask.SelectedIndex = 0;

        }
        
        /// <summary>
        ///  Retourne la couleur en fonction de la class <paramref name="cssClass"/>
        /// </summary>
        /// <param name="cssClass"></param>
        /// <returns></returns>
        private static string GetColorName(string cssClass)
        {
            string ret = string.Empty;
            
            if (StrFunc.IsEmpty(cssClass))
                AspTools.CheckCssColor(ref cssClass);
            if (StrFunc.IsFilled(cssClass))
                ret = Color.FromName(cssClass).Name;

            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pKey"></param>
        /// <param name="pSetData"></param>
        /// <param name="pEnableViewState"></param>
        // EG 20200720 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc)
        // EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        protected override void LoadInfoDatas(string pKey, bool pSetData, bool pEnableViewState)
        {
            base.LoadInfoDatas(pKey, pSetData, pEnableViewState);

            if (StrFunc.IsFilled(pKey))
            {
                #region CSS et DESCRIPTION de LA TACHE
                DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(SessionTools.CS, null, Convert.ToInt32(pKey));
                if (null != sqlIOTask)
                {
                    // CSS de la tâche
                    if (StrFunc.IsFilled(sqlIOTask.CSSFilename))
                        CSSColor = sqlIOTask.CSSFilename;
                    // Description de la tâche
                    if (pSetData)
                    {
                        if (this.FindControl("lblInfoDesc") is WCTooltipLabel lblInfoDesc)
                        {
                            lblInfoDesc.Text = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), sqlIOTask.Description, null);
                            lblInfoDesc.Pty.TooltipContent = sqlIOTask.Description;
                        }
                    }
                }
                #endregion CSS et DESCRIPTION de LA TACHE

                #region Charger les paramètres de la tâche
                DataRow[] drParams = GetTaskParams(pKey);
                bool isAllParamHidden = true;
                WCTogglePanel pnlParameters = this.FindControl("divparameters") as WCTogglePanel;
                if (null != pnlParameters)
                    pnlParameters.SetHeaderTitle(Ressource.GetString("OTC_ADM_TOOL_IO_TASK_PARAM"));
                Table tblParameters = this.FindControl("tblParameters") as Table;
                string idIOParams = string.Empty;

                if (ArrFunc.IsFilled(drParams))
                {
                    HttpSessionStateTools.Set(SessionTools.SessionState, "Parameters", drParams);

                    Table paramTable = null;
                    Table pTable = null;
                    Table tblParametersTask = HtmlTools.CreateTable(4);
                    tblParametersTask.ID = "tblParameters" + pKey;
                    TableRow tr = new TableRow();
                    TableCell td = new TableCell();
                    td.Controls.Add(tblParametersTask);
                    tr.Controls.Add(td);
                    tblParameters.Controls.Add(tr);

                    foreach (DataRow drParam in drParams)
                    {
                        //On ne dessine que les paramètres définis comme "Non Hide"
                        bool isParamHide = !Convert.IsDBNull(drParam["ISHIDEONGUI"]) && BoolFunc.IsTrue((drParam["ISHIDEONGUI"]).ToString());
                        if (!isParamHide)
                        {
                            isAllParamHidden = false;
                            if (idIOParams != drParam["IDIOPARAM"].ToString())
                            {
                                if (pTable != null)
                                {
                                    tr = new TableRow();
                                    td = new TableCell();
                                    td.Controls.Add(paramTable);
                                    tr.Cells.Add(td);
                                    pTable.Controls.Add(tr);
                                }

                                idIOParams = drParam["IDIOPARAM"].ToString();
                                // La ligne Parameters
                                pTable = HtmlTools.CreateTable();
                                pTable.ID = "tblParameters" + drParam["IDIOPARAMDET"].ToString();
                                tr = CreateParametersRow(pTable, drParam);
                                tblParametersTask.Controls.Add(tr);
                                // Les Lignes Parameter
                                paramTable = HtmlTools.CreateTable();
                                paramTable.Style.Add(HtmlTextWriterStyle.Margin, "10px");
                                paramTable.ID = "tbl_" + drParam["IDIOPARAMDET"].ToString();
                            }
                            //La ligne Parameter Données
                            //tr = CreateParameterRow(drParam, pSetData, isParamEnabled);
                            ParameterRow pr = new ParameterRow(this)
                            {
                                DataId = "DATA" + drParam["IDIOPARAMDET"].ToString(),
                                DataType = (TypeData.TypeDataEnum)Enum.Parse(typeof(TypeData.TypeDataEnum), drParam["DATATYPE"].ToString(), true),
                                InfoValue = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), drParam["DESCRIPTION"].ToString(), null),
                                DisplayName = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), drParam["DISPLAYNAME"].ToString(), null),
                                DataValue = drParam["DEFAULTVALUE"].ToString(),
                                IsDataMandatory = Convert.ToBoolean(drParam["ISMANDATORY"]),
                                DataNote = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), drParam["NOTE"].ToString(), null),
                                DataListRet = drParam["LISTRETRIEVAL"].ToString(),
                                IsEnabled = (false == DataHelper.IsParamDirectionOutput(drParam["DIRECTION"].ToString()))
                            };

                            // FI 20130516 [] Interprétation d'une tâche en fonction de la tâche et du mot clé
                            if (StrFunc.IsFilled(pr.DataValue) && TypeData.IsTypeDate(pr.DataType) && (pr.DataValue!="LISTRETRIEVAL"))
                                pr.DataValue = ConvertDate(CSTools.SetCacheOn(SessionTools.CS), pr.DataValue, sqlIOTask.IdIOTask);     
                            
                            tr = pr.ConstructParameterRow(pSetData);
                            paramTable.Controls.Add(tr);
                        }
                    }
                    //
                    if ((pTable != null) && (paramTable != null))
                    {
                        tr = new TableRow();
                        td = new TableCell();
                        td.Controls.Add(paramTable);
                        tr.Cells.Add(td);
                        pTable.Controls.Add(tr);
                    }
                }
                pnlParameters.Visible = (false == isAllParamHidden);
                #endregion Charger les paramètres de la tâche
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTaskId"></param>
        /// <returns></returns>
        private static DataRow[] GetTaskParams(string pTaskId)
        {
            SQL_IOTaskParams taskParams = new SQL_IOTaskParams(CSTools.SetCacheOn(SessionTools.CS), pTaskId);
            return taskParams.Select();
        }
        
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTable"></param>
        /// <param name="pDrParams"></param>
        /// <returns></returns>
        /// EG 20200819 [XXXXX] Nouvelle interface GUI v10(Mode Noir ou blanc) 
        private TableRow CreateParametersRow(Table pTable, DataRow pDrParams)
        {
            // TABLE : PARAMETERS
            string paramDisplayname = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), pDrParams["PDISPLAYNAME"].ToString(), null);
            string paramDescription = LogTools.GetCurrentCultureString(CSTools.SetCacheOn(SessionTools.CS), pDrParams["PDESCRIPTION"].ToString(), null);
            TableCell td = HtmlTools.NewLabelInCell(paramDescription, EFSCssClass.CssClassEnum.lblCaptureTitleBold.ToString(), HorizontalAlign.Left, Unit.Percentage(100));
            TableRow tr = new TableRow();
            tr.Controls.Add(td);
            pTable.Controls.Add(tr);

            tr = new TableRow();
            WCTogglePanel pnl = new WCTogglePanel(Color.Transparent, paramDisplayname, "size5", true) 
            {
                CssClass = CSSMode + " " + _mainMenuClassName,
            };
            pnl.AddContent(pTable);
            tr.Controls.Add(HtmlTools.NewControlInCell(pnl));
            return tr;
        }

        /// <summary>
        /// Génère un message IO
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pTask">Représente la tâche (OTCmlId ou l'identifier est nécessaire)</param>
        /// <param name="parameters">Représente les paramètres de la tâches (issus de IOTASK_PARAM)</param>
        /// <param name="pData">Représente les données xml lorsque le message contient les informations à importer</param>
        /// <param name="session">Instance envoie le message queue</param>
        /// FI 20130129 add
        /// FI 20130129 Cette méthode pourrait être déplacée dans un methode tools public
        /// Envisagé par FI pour la gateway mais PM ne désire pas (L'accès aux bases de données n'est pas nécessairement possible depuis un serveur gateway)
        private static (bool retIsOk, string retErrMsg) SendIOMQueue(string pCS, SpheresIdentification pTask, MQueueparameters parameters, AppSession session)
        {

            DataIOTASK sqlIOTask = DataIOTaskEnabledHelper.GetDataIoTask(SessionTools.CS, null, pTask.OTCmlId);

            if (null == sqlIOTask)
                throw new ArgumentException("{pTask} parameter is not valid, pTask.OTCmlId or pTask.identifier must be specified");

            MQueueAttributes mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pCS,
                id = sqlIOTask.IdIOTask,
                identifier = sqlIOTask.Identifier
            };


            pTask.OTCmlId = sqlIOTask.IdIOTask;
            pTask.Identifier = sqlIOTask.Identifier;
            pTask.Displayname = sqlIOTask.DisplayName;

            mQueueAttributes = new MQueueAttributes()
            {
                connectionString = pCS,
                id = sqlIOTask.IdIOTask,
                identifier = sqlIOTask.Identifier,
                idInfo = SQL_TableTools.GetIOMQueueIdInfo(sqlIOTask)
            };


            mQueueAttributes.parameters = parameters;

            MQueueTaskInfo taskInfo = new MQueueTaskInfo
            {
                Session = session,
                mQueueParameters = mQueueAttributes.parameters,
                mQueue = new MQueueBase[1] { new IOMQueue(mQueueAttributes) },
                process = Cst.ProcessTypeEnum.IO,
                connectionString = pCS,
                trackerAttrib = new TrackerAttributes()
                {
                    process = Cst.ProcessTypeEnum.IO,
                    caller = sqlIOTask.InOut,
                    info = new List<DictionaryEntry>()
                }
            };

            taskInfo.SetTrackerAckWebSessionSchedule(taskInfo.mQueue[0].idInfo);


            List<DictionaryEntry> info = new List<DictionaryEntry>
            {
                new DictionaryEntry("IDDATA", mQueueAttributes.id),
                new DictionaryEntry("IDDATAIDENT", "IOTASK"),
                new DictionaryEntry("IDDATAIDENTIFIER", mQueueAttributes.identifier)
            };
            if (false == taskInfo.trackerAttrib.info.Exists(match => match.Key.ToString() == "DATA1"))
                info.Add(new DictionaryEntry("DATA1", sqlIOTask.DisplayName));
            if (false == taskInfo.trackerAttrib.info.Exists(match => match.Key.ToString() == "DATA2"))
                info.Add(new DictionaryEntry("DATA2", sqlIOTask.InOut));
            taskInfo.trackerAttrib.info.AddRange(info);

            var (isOk, errMsg) = MQueueTaskInfo.SendMultiple(taskInfo);

            return (isOk, errMsg);
        }


        /// <summary>
        /// Interprétation d'une date (BUSINESS,TODAY, etc....) et retourne la date au format fmtShortDate (spécifique à la culture courante)
        /// <para>Lorsque la tâche est spécifique à une chambre et que la donnée en entrée vaut BUSINESS spheres® recherche la date dans ENTITYMARKET</para>
        /// </summary>
        /// <param name="pCS"></param>
        /// <param name="pData">Représente la donnée en entrée</param>
        /// <param name="idTask"></param>
        /// <returns></returns>
        /// EG 20210419 [XXXXX] Upd Usage du businessCenter de l'entité
        private static string ConvertDate(string pCS, string pData, int idTask)
        {
            IsTaskCSS(pCS, idTask, out int idACSS);
            // FI 20130513 [] usage de la classe DtFuncML, de manière à interpréter BUSINESS 
            DtFuncML dtFunc = new DtFuncML(pCS, SessionTools.User.Entity_BusinessCenter, SessionTools.User.Entity_IdA, 0, idACSS, null)
            {
                FourDigitReading = DtFunc.FourDigitReadingEnum.FourDigitHasYYYY
            };
            return dtFunc.GetDateString(pData);
        }

        /// <summary>
        /// Retourne true si la tâche IO est spécifique à une chambre de compensation
        /// </summary>
        /// <returns></returns>
        /// <param name="pCS"></param>
        /// <param name="idTask">Représente la tâche IO</param>
        /// <param name="idCSS">Retourne la chambre</param>
        private static bool IsTaskCSS(string pCS, int idTask, out int idCSS)
        {
            idCSS = 0;

            StrBuilder sql = new StrBuilder();
            sql += "select IDA from dbo.CSS where (IDIOTASK_RISKDATA = @ID or IDIOTASK_REPOSITORY =@ID)";

            DataParameters parameters = new DataParameters();
            parameters.Add(DataParameter.GetParameter(pCS, DataParameter.ParameterEnum.ID), idTask);

            QueryParameters query = new QueryParameters(pCS, sql.ToString(), parameters);

            Object obj = DataHelper.ExecuteScalar(pCS, CommandType.Text, query.Query, query.Parameters.GetArrayDbParameter());
            if (null != obj)
                idCSS = Convert.ToInt32(obj);

            bool ret = (idCSS > 0);
            return ret;
        }
    }
}
