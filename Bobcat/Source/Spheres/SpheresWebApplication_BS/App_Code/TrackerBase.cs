using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;


using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;


#region TrackerCounter classes
public abstract class TrackerCounter
{
    public Enum key;
    public List<Pair<Enum, int>> child;
    public TrackerCounter(Enum pKey, Enum pChild)
    {
        key = pKey;
        child = new List<Pair<Enum, int>>();
        child.Add(new Pair<Enum, int> { First = pChild, Second = 0 });
    }
}
/// <summary>
/// Si : DisplayByStatus
/// </summary>
[Serializable]
public class StatusTrackerCounter : TrackerCounter
{
    public Cst.GroupTrackerEnum keyGroup { get { return (Cst.GroupTrackerEnum)key; } }
    public StatusTrackerCounter(Cst.GroupTrackerEnum pGroup, ProcessStateTools.ReadyStateEnum pReadyState) : base(pGroup, pReadyState) { }
}
/// <summary>
/// Si : DisplayByReadyState
/// </summary>
[Serializable]
public class ReadyStateTrackerCounter  : TrackerCounter
{
    public Cst.GroupTrackerEnum keyGroup {get {return (Cst.GroupTrackerEnum) key;}}
    public ReadyStateTrackerCounter (Cst.GroupTrackerEnum pGroup, ProcessStateTools.StatusEnum pStatus) : base(pGroup, pStatus) { }
}

/// <summary>
/// Si : DisplayByGroupTracker
/// </summary>
[Serializable]
public class GroupTrackerCounter: TrackerCounter
{
    public ProcessStateTools.ReadyStateEnum keyReadyState { get { return (ProcessStateTools.ReadyStateEnum)key; } }
    public GroupTrackerCounter(ProcessStateTools.ReadyStateEnum pReadyState, ProcessStateTools.StatusEnum pStatus) : base(pReadyState, pStatus) { }
}
#endregion GroupTrackerCounter classes

/// <summary>
/// Description résumée de TrackerBase
/// </summary>
public class TrackerBase : ContentPageBase
{
    #region Members
    protected bool m_IsTrackerAlert;
    protected Int64 m_TrackerAlertProcess;
    protected string m_SessionID;
    protected string m_CS;
    protected bool m_IsLoadSession;
    protected string m_TrackerHisToric;
    protected string m_TrackerDisplayMode;
    protected int m_TrackerNbRowPerGroup;
    protected int m_TrackerRefreshInterval;
    protected Dictionary<TrackerTools.CheckListTypeEnum, List<TrackerTools.TrackerFlags>> m_TrackerDisplayValues;
    protected int m_RefreshTime;

    protected Dictionary<Cst.GroupTrackerEnum, string> m_GroupTrackerName;
    protected Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>> m_ReadyStateName;
    protected Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>> m_StatusName;

    protected Dictionary<Cst.ServiceEnum, List<Pair<Cst.ProcessTypeEnum, string>>> m_ServiceObserver;

    protected Dictionary<Enum, Pair<int, List<ReadyStateTrackerCounter>>> m_LstTrackerByReadyState;
    protected Dictionary<Enum, Pair<int, List<GroupTrackerCounter>>> m_LstTrackerByGroup;
    protected Dictionary<Enum, Pair<int, List<StatusTrackerCounter>>> m_LstTrackerByStatus;



    #endregion Members
    #region Constructors
    public TrackerBase()
	{
	}
    #endregion Constructors

    #region Methods
    #region OnInit
    /// <summary>
    /// Initialisation de la page
    /// . Paramètres de gestion des messages de réponse 
    /// . Timer de raffraichissement
    /// </summary>
    /// <param name="e"></param>
    protected override void OnInit(EventArgs e)
    {
        base.OnInit(e);
        // Chargement des paramètres de lecture des messages de RESPONSE
        m_IsTrackerAlert = SessionTools.IsTrackerAlert;
        m_TrackerAlertProcess = SessionTools.TrackerAlertProcess;
        m_TrackerHisToric = SessionTools.TrackerHistoric;
        m_TrackerDisplayMode = SessionTools.TrackerDisplayMode;
        m_SessionID = SessionTools.SessionID;
        m_TrackerDisplayValues = SessionTools.TrackerDisplayValues;
        m_TrackerNbRowPerGroup = SessionTools.TrackerNbRowPerGroup;
        m_TrackerRefreshInterval = SessionTools.TrackerRefreshInterval;

        m_CS = SessionTools.CS;
    }
    #endregion OnInit

    #region Page_Load
    protected virtual void Page_Load(object sender, EventArgs e)
    {
        bool isLoadParam = StrFunc.IsFilled(Request.Params["__EVENTARGUMENT"]) && ("LoadParam" == Request.Params["__EVENTARGUMENT"]);
        bool isTimerRefresh = StrFunc.IsFilled(Request.Params["__EVENTARGUMENT"]) && ("TIMER" == Request.Params["__EVENTARGUMENT"]);
        bool isRefresh = StrFunc.IsFilled(Request.Params["__EVENTARGUMENT"]) && ("SELFRELOAD_" == Request.Params["__EVENTARGUMENT"]);
        isRefresh |= StrFunc.IsFilled(Request.Params["__EVENTTARGET"]) && (Request.Params["__EVENTTARGET"].EndsWith("timerRefresh"));
        m_IsLoadSession = (false == IsPostBack) || isLoadParam || isTimerRefresh || isRefresh;
        if (false == m_IsLoadSession)
            m_IsLoadSession = StrFunc.IsFilled(Request.Params["imgautorefresh.x"]) || StrFunc.IsFilled(Request.Params["imgrefresh.x"]);

        if (m_IsLoadSession)
        {
            m_RefreshTime = SessionTools.TrackerRefreshInterval;
            m_ServiceObserver = TrackerServiceTools.CreateDicServiceProcess();
            // Initialisation des dictionnaires (ReadyStateName, GroupTrackerName et StatusName)
            Initialize();
            //Construction du tracker par ReadyState/Group/Status
            CreateLstTrackerByReadyState();
            //Construction du tracker par Group/ReadyState/Status
            CreateLstTrackerByGroup();
            //Construction du tracker par Status/Group/ReadyState
            CreateLstTrackerByStatus();
            // Chargement des données
            LoadData();
        }
    }
    #endregion Page_Load

    #region Initialize
    /// <summary>
    ///  Chargement du dictionnaire des ReadyState (m_ReadyStateName) 
    ///  Chargement du dictionnaire des groupes de traitements (m_GroupTrackerName)
    ///  Chargement du dictionnaire des statuts (m_StatusName)
    /// </summary>
    public void Initialize()
    {
        FieldInfo fldAttr = null;
        object[] enumAttrs = null;

        #region ReadyState
        m_ReadyStateName = new Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>>();
        foreach (ProcessStateTools.ReadyStateEnum readyState in Enum.GetValues(typeof(ProcessStateTools.ReadyStateEnum)))
        {
            if (false == m_ReadyStateName.ContainsKey(readyState))
            {
                #region Création du ReadyState
                Pair<string, string> pair = new Pair<string, string>(readyState.ToString(), "Black");
                fldAttr = readyState.GetType().GetField(readyState.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                    pair.First = Ressource.GetString(((XmlEnumAttribute)enumAttrs[0]).Name, readyState.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(ProcessStateTools.HelpAssociateAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                    pair.Second = ((ProcessStateTools.HelpAssociateAttribute)enumAttrs[0]).ColorName;
                m_ReadyStateName[readyState] = pair;
                #endregion Création du ReadyState
            }
        }
        #endregion ReadyState
        #region Group
        m_GroupTrackerName = new Dictionary<Cst.GroupTrackerEnum, string>();
        foreach (Cst.GroupTrackerEnum group in Enum.GetValues(typeof(Cst.GroupTrackerEnum)))
        {
            if (false == m_GroupTrackerName.ContainsKey(group))
            {
                #region Création du Groupe
                m_GroupTrackerName.Add(group, group.ToString());
                enumAttrs = group.GetType().GetField(group.ToString()).GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                    m_GroupTrackerName[group] = Ressource.GetString(((XmlEnumAttribute)enumAttrs[0]).Name, group.ToString());
                else
                    m_GroupTrackerName[group] = Ressource.GetString(group.ToString(), group.ToString());
                #endregion Création du Groupe
            }
        }
        #endregion Group
        #region Status
        m_StatusName = new Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>>();
        foreach (ProcessStateTools.StatusEnum status in Enum.GetValues(typeof(ProcessStateTools.StatusEnum)))
        {
            if (false == m_StatusName.ContainsKey(status))
            {
                #region Création du Status
                Pair<string, string> pair = new Pair<string, string>(status.ToString(), "Black");
                fldAttr = status.GetType().GetField(status.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(XmlEnumAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                    pair.First = Ressource.GetString(((XmlEnumAttribute)enumAttrs[0]).Name, status.ToString());
                enumAttrs = fldAttr.GetCustomAttributes(typeof(ProcessStateTools.HelpAssociateAttribute), true);
                if (ArrFunc.IsFilled(enumAttrs))
                    pair.Second = ((ProcessStateTools.HelpAssociateAttribute)enumAttrs[0]).ColorName;
                m_StatusName[status] = pair;
                #endregion Création du Status
            }
        }
        #endregion Status
    }
    #endregion CreateGroupTrackerName

    #region CreateLstTrackerByReadyState
    /// <summary>
    ///  Création de la liste des groupes de traitements pour chaque readyState
    ///  Création de la liste des status pour chaque groupe de traitements
    /// </summary>
    public void CreateLstTrackerByReadyState()
    {
        List<ReadyStateTrackerCounter> _lstGroup = null;

        m_LstTrackerByReadyState = new Dictionary<Enum, Pair<int, List<ReadyStateTrackerCounter>>>();
        foreach (ProcessStateTools.ReadyStateEnum readyState in m_ReadyStateName.Keys)
        {
            foreach (Cst.GroupTrackerEnum group in  m_GroupTrackerName.Keys)
            {
                if (group != Cst.GroupTrackerEnum.ALL)
                {
                    foreach (ProcessStateTools.StatusEnum status in m_StatusName.Keys)
                    {
                        if (false == m_LstTrackerByReadyState.ContainsKey(readyState))
                        {
                            _lstGroup = new List<ReadyStateTrackerCounter>();
                            _lstGroup.Add(new ReadyStateTrackerCounter(group, status));
                            m_LstTrackerByReadyState.Add((Enum)readyState, new Pair<int, List<ReadyStateTrackerCounter>>(0, _lstGroup));
                        }
                        else if (false == m_LstTrackerByReadyState[readyState].Second.Exists(match => match.keyGroup == group))
                        {
                            m_LstTrackerByReadyState[readyState].Second.Add(new ReadyStateTrackerCounter(group, status));
                        }
                        else
                        {
                            TrackerCounter groupTracker = m_LstTrackerByReadyState[readyState].Second.Find(match => match.keyGroup == group);
                            groupTracker.child.Add(new Pair<Enum, int> { First = status, Second = 0 });
                        }
                    }
                }
            }
        }
    }
    #endregion CreateLstTrackerByReadyState
    #region CreateLstTrackerByGroup
    /// <summary>
    ///  Création de la liste des groupes de traitements pour chaque readyState
    ///  Création de la liste des status pour chaque groupe de traitements
    /// </summary>
    public void CreateLstTrackerByGroup()
    {
        List<GroupTrackerCounter> _lstReadyState = null;

        m_LstTrackerByGroup = new Dictionary<Enum, Pair<int, List<GroupTrackerCounter>>>();
        foreach (Cst.GroupTrackerEnum group in m_GroupTrackerName.Keys)
        {
            if (group != Cst.GroupTrackerEnum.ALL)
            {

                foreach (ProcessStateTools.ReadyStateEnum readyState in m_ReadyStateName.Keys)
                {
                    foreach (ProcessStateTools.StatusEnum status in m_StatusName.Keys)
                    {
                        if (false == m_LstTrackerByGroup.ContainsKey(group))
                        {
                            _lstReadyState = new List<GroupTrackerCounter>();
                            _lstReadyState.Add(new GroupTrackerCounter(readyState, status));
                            m_LstTrackerByGroup.Add(group, new Pair<int, List<GroupTrackerCounter>>(0, _lstReadyState));
                        }
                        else if (false == m_LstTrackerByGroup[group].Second.Exists(match => match.keyReadyState == readyState))
                        {
                            m_LstTrackerByGroup[group].Second.Add(new GroupTrackerCounter(readyState, status));
                        }
                        else
                        {
                            TrackerCounter readyStateTracker = m_LstTrackerByGroup[group].Second.Find(match => match.keyReadyState == readyState);
                            readyStateTracker.child.Add(new Pair<Enum, int> { First = status, Second = 0 });
                        }
                    }
                }
            }
        }
    }
    #endregion CreateLstTrackerByGroup
    #region CreateLstTrackerByStatus
    /// <summary>
    ///  Création de la liste des groupes de traitements pour chaque status
    ///  Création de la liste des status pour chaque groupe de traitements
    /// </summary>
    public void CreateLstTrackerByStatus()
    {
        List<StatusTrackerCounter> _lstGroup = null;

        m_LstTrackerByStatus = new Dictionary<Enum, Pair<int, List<StatusTrackerCounter>>>();
        foreach (ProcessStateTools.StatusEnum statusGen in m_StatusName.Keys)
        {
            foreach (Cst.GroupTrackerEnum group in m_GroupTrackerName.Keys)
            {
                if (group != Cst.GroupTrackerEnum.ALL)
                {

                    foreach (ProcessStateTools.ReadyStateEnum readyState in m_ReadyStateName.Keys)
                    {
                        if (false == m_LstTrackerByStatus.ContainsKey(statusGen))
                        {
                            _lstGroup = new List<StatusTrackerCounter>();
                            _lstGroup.Add(new StatusTrackerCounter(group, readyState));
                            m_LstTrackerByStatus.Add((Enum)statusGen, new Pair<int, List<StatusTrackerCounter>>(0, _lstGroup));
                        }
                        else if (false == m_LstTrackerByStatus[statusGen].Second.Exists(match => match.keyGroup == group))
                        {
                            m_LstTrackerByStatus[statusGen].Second.Add(new StatusTrackerCounter(group, readyState));
                        }
                        else
                        {
                            TrackerCounter groupTracker = m_LstTrackerByStatus[statusGen].Second.Find(match => match.keyGroup == group);
                            groupTracker.child.Add(new Pair<Enum, int> { First = readyState, Second = 0 });
                        }
                    }
                }
            }
        }
    }
    #endregion CreateLstTrackerByStatus

    #region LoadData
    /// <summary>
    /// Chargement du tracker
    /// . Dictionnaires (READYSTATE / GROUP / STATUS / HELP)
    /// . Remplissage des onglets (READYSTATE, HELP) et accordéons (GROUP par READYSTATE)
    /// . Compteurs scrollés
    /// </summary>
    public void LoadData()
    {
        LoadHeaderData();
        //FillHeaderHelp();
    }
    #endregion LoadData
    #region LoadHeaderData
    /// <summary>
    /// Chargement dictionnaires (READYSTATE / GROUP / STATUS / HELP)
    /// </summary>
    protected void LoadHeaderData()
    {
        IDataReader dr = null;
        IDbTransaction dbTransaction = null;
        ResetGroup();
        try
        {
            dbTransaction = DataHelper.BeginTran(SessionTools.CS, IsolationLevel.ReadUncommitted);
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "ISMARKED", DbType.Boolean), false);

            #region Restriction sur date Historique
            string sqlWhereTrackerHistoric = string.Empty;
            Nullable<DateTime> dtReference = null;
            string histo = SessionTools.TrackerHistoric;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
            {
                dtReference = new DtFunc().StringToDateTime("-" + histo);
                parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.DTINS), dtReference);

                sqlWhereTrackerHistoric = " and (isnull(tk.DTUPD, tk.DTINS) >= @DTINS) ";
            }
            #endregion Restriction sur date Historique

            #region Restriction sur user
            string sqlWhereRestrict = string.Empty;
            bool isTrackerApplyRestrict = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Spheres_TrackerApplyRestrict"));

            if ((!SessionTools.User.IsSessionSysAdmin) && isTrackerApplyRestrict)
            {
                parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDAINS), SessionTools.User.idA);
                sqlWhereRestrict = " and (tk.IDAINS=@IDAINS) ";
            }
            #endregion Restriction sur user

            string sqlSelect = @"select count(*) as TOTAL, tk.READYSTATE, tk.GROUPTRACKER, tk.STATUSTRACKER
            from dbo.TRACKER_L tk 
            where (ISMARKED=@ISMARKED)" + sqlWhereTrackerHistoric + sqlWhereRestrict + 
            @"group by tk.READYSTATE, tk.GROUPTRACKER, tk.STATUSTRACKER" + Cst.CrLf;

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect.ToString(), parameters);
            dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());

            while (dr.Read())
            {
                ProcessStateTools.ReadyStateEnum readyState = default(ProcessStateTools.ReadyStateEnum);
                if ((false == Convert.IsDBNull(dr["READYSTATE"]) &&
                    Enum.IsDefined(typeof(ProcessStateTools.ReadyStateEnum), dr["READYSTATE"].ToString())))
                    readyState = (ProcessStateTools.ReadyStateEnum)Enum.Parse(typeof(ProcessStateTools.ReadyStateEnum), dr["READYSTATE"].ToString());

                Cst.GroupTrackerEnum group = default(Cst.GroupTrackerEnum);
                if ((false == Convert.IsDBNull(dr["GROUPTRACKER"]) &&
                    Enum.IsDefined(typeof(Cst.GroupTrackerEnum), dr["GROUPTRACKER"].ToString())))
                    group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), dr["GROUPTRACKER"].ToString());

                ProcessStateTools.StatusEnum status = ProcessStateTools.StatusEnum.NA;
                if ((false == Convert.IsDBNull(dr["STATUSTRACKER"]) &&
                    Enum.IsDefined(typeof(ProcessStateTools.StatusEnum), dr["STATUSTRACKER"].ToString())))
                    status = (ProcessStateTools.StatusEnum)Enum.Parse(typeof(ProcessStateTools.StatusEnum), dr["STATUSTRACKER"].ToString());

                int total = Convert.ToInt32(dr["TOTAL"]);

                // ReadyState
                m_LstTrackerByReadyState[readyState].Second.Find(match => 
                    match.keyGroup == group).child.Find(match => 
                        (ProcessStateTools.StatusEnum)match.First == status).Second = total;
                // Group
                m_LstTrackerByGroup[group].Second.Find(match => 
                    match.keyReadyState == readyState).child.Find(match =>
                       (ProcessStateTools.StatusEnum) match.First == status).Second = total;
                // Status
                m_LstTrackerByStatus[status].Second.Find(match => 
                    match.keyGroup == group).child.Find(match =>
                        (ProcessStateTools.ReadyStateEnum)match.First == readyState).Second = total;
            }
            SetTotalByMode(m_LstTrackerByReadyState);
            SetTotalByMode(m_LstTrackerByGroup);
            SetTotalByMode(m_LstTrackerByStatus);

            //SetTotalByReadyState();
            //SetTotalByGroup();
            //SetTotalByStatus();
        }
        finally
        {
            if (null != dr)
                dr.Dispose();
            if (null != dbTransaction)
                DataHelper.RollbackTran(dbTransaction);
        }
    }
    #endregion LoadHeaderData
    #region LoadViewState
    /// <summary>
    /// Lecture des groupes/status dans le ViewState
    /// </summary>
    /// <param name="savedState"></param>
    protected override void LoadViewState(object savedState)
    {
        base.LoadViewState(savedState);
        object[] viewState = (object[])savedState;
        m_GroupTrackerName = (Dictionary<Cst.GroupTrackerEnum, string>)viewState[2];
        m_ReadyStateName = (Dictionary<ProcessStateTools.ReadyStateEnum, Pair<string, string>>)viewState[3];
        m_StatusName = (Dictionary<ProcessStateTools.StatusEnum, Pair<string, string>>)viewState[4];
        if ((null == m_LstTrackerByReadyState) || (null == m_LstTrackerByGroup) || (null == m_LstTrackerByStatus) || ("SELFRELOAD_" == Request.Params["__EVENTARGUMENT"]))
        {
            CreateLstTrackerByReadyState();
            CreateLstTrackerByGroup();
            CreateLstTrackerByStatus();
            LoadData();
        }
    }
    #endregion LoadViewState
    #region SaveViewState
    /// <summary>
    /// Sauvegarde des groupes/status dans le ViewState
    /// </summary>
    /// <returns></returns>
    protected override object SaveViewState()
    {
        if (HttpContext.Current == null)
            return null;
        //
        object[] viewState = new object[5];
        viewState[0] = ((Array)base.SaveViewState()).GetValue(0);
        viewState[1] = ((Array)base.SaveViewState()).GetValue(1);
        viewState[2] = m_GroupTrackerName;
        viewState[3] = m_ReadyStateName;
        viewState[4] = m_StatusName;
        return viewState;
    }
    #endregion SaveViewState


    #region ResetGroup
    /// <summary>
    /// RAZ des compteurs pour chaque tupple
    /// </summary>
    private void ResetGroup()
    {
        if (null != m_LstTrackerByReadyState)
        {
            foreach (ProcessStateTools.ReadyStateEnum readyState in m_ReadyStateName.Keys)
            {
                if (m_LstTrackerByReadyState.ContainsKey(readyState))
                {
                    m_LstTrackerByReadyState[readyState].First = 0;
                    m_LstTrackerByReadyState[readyState].Second.ForEach(group => { group.child.ForEach(item => { item.Second = 0; }); });
                }
            }
        }
        if (null != m_LstTrackerByGroup)
        {
            foreach (Cst.GroupTrackerEnum group in m_GroupTrackerName.Keys)
            {
                if (m_LstTrackerByGroup.ContainsKey(group))
                {
                    m_LstTrackerByGroup[group].First = 0;
                    m_LstTrackerByGroup[group].Second.ForEach(readystate => { readystate.child.ForEach(item => { item.Second = 0; }); });
                }
            }
        }
        if (null != m_LstTrackerByStatus)
        {
            foreach (ProcessStateTools.ReadyStateEnum status in m_StatusName.Keys)
            {
                if (m_LstTrackerByStatus.ContainsKey(status))
                {
                    m_LstTrackerByStatus[status].First = 0;
                    m_LstTrackerByStatus[status].Second.ForEach(group => { group.child.ForEach(item => { item.Second = 0; }); });
                }
            }
        }
    }
    #endregion ResetGroup

    #region SetCssBadge
    public string SetCssBadge(Nullable<ProcessStateTools.StatusEnum> pStatus)
    {
        if (pStatus.HasValue)
            return "sph-badge " + pStatus.ToString();
        else
            return string.Empty;
    }
    #endregion SetCssBadge
    #region SetCssReverseBadge
    public string SetCssReverseBadge(Nullable<ProcessStateTools.StatusEnum> pStatus)
    {
        if (pStatus.HasValue)
            return "sph-r-badge " + pStatus.ToString();
        else
            return string.Empty;
    }
    #endregion SetCssReverseBadge

    #region SetStatus
    protected Nullable<ProcessStateTools.StatusEnum> SetStatus(Nullable<ProcessStateTools.StatusEnum> pStatusMajor,
        Nullable<ProcessStateTools.StatusEnum> pStatusMinor)
    {
        Nullable<ProcessStateTools.StatusEnum> _status = pStatusMajor;
        if (false == pStatusMajor.HasValue)
        {
            _status = pStatusMinor;
        }
        else if (pStatusMinor.HasValue)
        {
            switch (pStatusMinor.Value)
            {
                case ProcessStateTools.StatusEnum.ERROR:
                    _status = pStatusMinor;
                    break;
                case ProcessStateTools.StatusEnum.WARNING:
                    if (_status.Value != ProcessStateTools.StatusEnum.ERROR)
                        _status = pStatusMinor;
                    break;
                case ProcessStateTools.StatusEnum.PENDING:
                    if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) && (_status.Value != ProcessStateTools.StatusEnum.WARNING))
                        _status = pStatusMinor;
                    break;
                case ProcessStateTools.StatusEnum.NA:
                    if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                        (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                        (_status.Value != ProcessStateTools.StatusEnum.PENDING))
                        _status = pStatusMinor;
                    break;
                case ProcessStateTools.StatusEnum.PROGRESS:
                    if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                        (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                        (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                        (_status.Value != ProcessStateTools.StatusEnum.NA))
                        _status = pStatusMinor;
                    break;
                case ProcessStateTools.StatusEnum.NONE:
                    if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                        (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                        (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                        (_status.Value != ProcessStateTools.StatusEnum.PROGRESS) &&
                        (_status.Value != ProcessStateTools.StatusEnum.NA))
                        _status = pStatusMinor;
                    break;
                default:
                    if ((_status.Value != ProcessStateTools.StatusEnum.ERROR) &&
                      (_status.Value != ProcessStateTools.StatusEnum.WARNING) &&
                      (_status.Value != ProcessStateTools.StatusEnum.PENDING) &&
                      (_status.Value != ProcessStateTools.StatusEnum.NA) &&
                      (_status.Value != ProcessStateTools.StatusEnum.PROGRESS) &&
                      (_status.Value != ProcessStateTools.StatusEnum.NONE))
                        _status = pStatusMinor;
                    break;
            }
        }
        return _status;
    }

    #endregion SetStatusReadyState

    #region SetTotalByMode
    private void SetTotalByMode<T>(Dictionary<Enum, Pair<int, List<T>>> pList) where T : TrackerCounter
    {
        if ((null != pList) && (0 < pList.Count))
        {
            foreach (Enum item in pList.Keys)
            {
                int total = 0;
                pList[item].Second.ForEach(subItem =>
                {
                    subItem.child.ForEach(t => { total += t.Second; });
                });
                pList[item].First = total;
            }
        }
    }
    #endregion SetTotalByMode
    #region SetTotalByGroup
    /// <summary>
    /// Alimentation des compteurs TOTAUX par GROUP
    /// </summary>
    private void SetTotalByGroup()
    {
        if ((null != m_LstTrackerByGroup) && (0 < m_LstTrackerByGroup.Count))
        {
            foreach (Cst.GroupTrackerEnum item in m_LstTrackerByGroup.Keys)
            {
                int total = 0;
                m_LstTrackerByGroup[item].Second.ForEach(subItem =>
                {
                    subItem.child.ForEach(t => { total += t.Second; });
                });
                m_LstTrackerByGroup[item].First = total;
            }
        }
    }
    #endregion SetTotalByReadyState
    #region SetTotalByReadyState
    /// <summary>
    /// Alimentation des compteurs TOTAUX par READYSTATE
    /// </summary>
    private void SetTotalByReadyState()
    {
        if ((null != m_LstTrackerByReadyState) && (0 < m_LstTrackerByReadyState.Count))
        {
            foreach (ProcessStateTools.ReadyStateEnum item in m_LstTrackerByReadyState.Keys)
            {
                int total = 0;
                m_LstTrackerByReadyState[item].Second.ForEach(subItem =>
                {
                    subItem.child.ForEach(t => { total += t.Second; });
                });
                m_LstTrackerByReadyState[item].First = total;
            }
        }
    }
    #endregion SetTotalByReadyState
    #region SetTotalByStatus
    /// <summary>
    /// Alimentation des compteurs TOTAUX par STATUS
    /// </summary>
    private void SetTotalByStatus()
    {
        if ((null != m_LstTrackerByStatus) && (0 < m_LstTrackerByStatus.Count))
        {
            foreach (ProcessStateTools.StatusEnum item in m_LstTrackerByStatus.Keys)
            {
                int total = 0;
                m_LstTrackerByStatus[item].Second.ForEach(subItem =>
                {
                    subItem.child.ForEach(t => { total += t.Second; });
                });
                m_LstTrackerByStatus[item].First = total;
            }
        }
    }
    #endregion SetTotalByStatus
    #endregion Methods
}