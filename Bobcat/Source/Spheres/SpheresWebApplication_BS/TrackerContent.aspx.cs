using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

using EFS.ACommon;
using EFS.ApplicationBlocks.Data;
using EFS.Common;
using EFS.Common.Web;

#region TrackerDataReader
/// <summary>
/// 
/// </summary>
public class TrackerDataReader
{
    #region Members
    public IDataReader dr;
    #endregion
    #region Accessors
    public bool isRowUpdated
    {
        get
        {
            DateTime dtIns = Convert.ToDateTime(dr["DTINS"]);
            DateTime dtUpd = Convert.IsDBNull(dr["DTUPD"]) ? Convert.ToDateTime(null) : Convert.ToDateTime(dr["DTUPD"]);
            return (dtUpd.CompareTo(dtIns) >= 0);
        }
    }
    #endregion  Accessors
    #region Constructor
    public TrackerDataReader()
    {
    }
    public TrackerDataReader(IDataReader pDr)
    {
        dr = pDr;
    }
    #endregion Constructor
    #region Methods
    #region GetCssForeColor
    public string GetCssForeColor()
    {
        return ProcessStateTools.GetStatusCssClass(GetStatus());
    }
    #endregion GetCssForeColor
    #region GetReadyState
    public string GetReadyState()
    {
        return dr["READYSTATE"].ToString();
    }
    #endregion GetReadyState
    #region GetStatus
    public string GetStatus()
    {
        return dr["STATUSTRACKER"].ToString();
    }
    #endregion GetStatus
    #endregion Methods
}
#endregion TrackerDataReader

public partial class TrackerContent : Page // : PageBaseNew
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string mode = Request.QueryString["mode"];
        //string[] temp = null;
        switch (mode)
        {
            case "GRPLOAD":
                string group = Request.QueryString["group"];
                string readyState = Request.QueryString["readystate"];
                string status = Request.QueryString["status"];
                int top = SessionTools.TrackerNbRowPerGroup;
                Cst.GroupTrackerEnum groupTracker = default(Cst.GroupTrackerEnum);
                if (Enum.IsDefined(typeof(Cst.GroupTrackerEnum), group.ToUpper()))
                    groupTracker = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), group, true);
                LoadTrackerData(groupTracker, readyState, status, top);
                break;
            // EG 20170125 [Refactoring URL] Obsolete
            case "LNKOPEN":
                // EG 20170125 [Refactoring URL] Upd
                //temp = Request.QueryString["args"].Split(';');
                //string url = PageTools.PageToCall(temp[0], temp[1], temp[2]);
                //if (StrFunc.IsFilled(url))
                //    Server.Transfer(url);
                break;
        }
    }

    #region Methods
    /// <summary>
    /// 
    /// </summary>
    protected void LoadTrackerData(Cst.GroupTrackerEnum pGroupTracker, string pReadyState, string pStatusTracker, int pTop)
    {
        IDataReader dr = null;
        IDbTransaction dbTransaction = null;
        HtmlGenericControl ul = new HtmlGenericControl("ul");
        ul.Attributes.Add("class", "list-group small");

        string resDetail = Ressource.GetString("Detail");
        HtmlGenericControl li = null;
        try
        {
            dbTransaction = DataHelper.BeginTran(SessionTools.CS, IsolationLevel.ReadUncommitted);
            DataParameters parameters = new DataParameters();
            parameters.Add(new DataParameter(SessionTools.CS, "GROUPTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pGroupTracker.ToString());
            parameters.Add(new DataParameter(SessionTools.CS, "ISMARKED", DbType.Boolean), false);

            string message = 
                DataHelper.SQLReplace(SessionTools.CS, DataHelper.SQLReplace(SessionTools.CS, 
                DataHelper.SQLReplace(SessionTools.CS, DataHelper.SQLReplace(SessionTools.CS,
                DataHelper.SQLReplace(SessionTools.CS, DataHelper.SQLIsNull(SessionTools.CS, "smd.MESSAGE", "smd_gb.MESSAGE"),
                "'{1}'", DataHelper.SQLIsNull(SessionTools.CS, "tk.DATA1", "'{1}'")),
                "'{2}'", DataHelper.SQLIsNull(SessionTools.CS, "tk.DATA2", "'{2}'")),
                "'{3}'", DataHelper.SQLIsNull(SessionTools.CS, "tk.DATA3", "'{3}'")),
                "'{4}'", DataHelper.SQLIsNull(SessionTools.CS, "tk.DATA4", "'{4}'")),
                "'{5}'", DataHelper.SQLIsNull(SessionTools.CS, "tk.DATA5", "'{5}'"));

            string sqlSelect = @"select tk.IDTRK_L, tk.SYSCODE, tk.SYSNUMBER, 
            case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'Request' else isnull(smd.SHORTMESSAGE, smd_gb.SHORTMESSAGE) end as SHORTMESSAGETRACKER, 
            case when smd.SYSCODE is null and smd_gb.SYSCODE is null then 'No Message' else {0} end as MESSAGETRACKER, 
            tk.GROUPTRACKER, tk.READYSTATE, tk.STATUSTRACKER, 
            tk.IDDATA, tk.IDDATAIDENT, tk.IDDATAIDENTIFIER, tk.DATA1, tk.DATA1IDENT, tk.DTINS, tk.DTUPD, 
            case tk.STATUSTRACKER when 'ERROR' then 0 when 'WARNING' then 1 when 'PENDING' then 2 when 'PROGRESS' then 3 when 'NA' then 4
            when 'NONE' then 5 when 'SUCCESS' then 6 end as STATUSORDER
            from dbo.TRACKER_L tk
            left outer join dbo.SYSTEMMSG sm on (sm.SYSCODE = tk.SYSCODE) and (sm.SYSNUMBER = tk.SYSNUMBER)
            left outer join dbo.SYSTEMMSGDET smd on (smd.SYSCODE = sm.SYSCODE) and (smd.SYSNUMBER = sm.SYSNUMBER) and (smd.CULTURE = '{1}')
            left outer join dbo.SYSTEMMSGDET smd_gb on (smd_gb.SYSCODE = sm.SYSCODE) and (smd_gb.SYSNUMBER = sm.SYSNUMBER) and (smd_gb.CULTURE = 'en')
            where (tk.GROUPTRACKER = @GROUPTRACKER) and (tk.ISMARKED = @ISMARKED)" + Cst.CrLf;
            if (false == StrFunc.IsEmptyOrUndefined(pReadyState))
            {
                parameters.Add(new DataParameter(SessionTools.CS, "READYSTATE", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pReadyState);
                sqlSelect += " and (tk.READYSTATE=@READYSTATE)" + Cst.CrLf;
            }
            if (false == StrFunc.IsEmptyOrUndefined(pStatusTracker))
            {
                parameters.Add(new DataParameter(SessionTools.CS, "STATUSTRACKER", DbType.AnsiString, SQLCst.UT_ENUM_MANDATORY_LEN), pStatusTracker);
                sqlSelect += " and (tk.STATUSTRACKER=@STATUSTRACKER)" + Cst.CrLf;
            }
            Nullable<DateTime> dtReference = null;
            string histo = SessionTools.TrackerHistoric;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
            {
                dtReference = new DtFunc().StringToDateTime("-" + histo);
                parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.DTINS), dtReference);
                sqlSelect += " and (isnull(tk.DTUPD, tk.DTINS) >= @DTINS)";
            }

            bool isTrackerApplyRestrict = BoolFunc.IsTrue(SystemSettings.GetAppSettings("Spheres_TrackerApplyRestrict"));
            if ((!SessionTools.User.IsSessionSysAdmin) && isTrackerApplyRestrict)
            {
                parameters.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDAINS), SessionTools.User.idA);
                sqlSelect += " and (tk.IDAINS = @IDAINS)" + Cst.CrLf;
            }

            sqlSelect += " order by isnull(tk.DTUPD,tk.DTINS) desc, STATUSORDER";


            sqlSelect = String.Format(sqlSelect, message, SessionTools.Collaborator_Culture_ISOCHAR2);
            if (0 < pTop)
            {
                sqlSelect = DataHelper.GetSelectTop(SessionTools.CS, sqlSelect, pTop);
            }

            QueryParameters qryParameters = new QueryParameters(SessionTools.CS, sqlSelect.ToString(), parameters);
            dr = DataHelper.ExecuteReader(dbTransaction, CommandType.Text, qryParameters.query, qryParameters.parameters.GetArrayDbParameter());
            while (dr.Read())
            {
                Label lbl = null;
                TrackerDataReader tdr = new TrackerDataReader(dr);
                ul.Attributes.Add("class", "list-group small");

                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "list-group-item " + dr["STATUSTRACKER"].ToString());

                lbl = GetLabelDate(tdr);
                li.Controls.Add(GetLabelDate(tdr));
                li.Controls.Add(GetLinkMessage(tdr));
                lbl = GetLinkIdData(tdr, resDetail);
                if (StrFunc.IsFilled(lbl.Text))
                    li.Controls.Add(lbl);
                ul.Controls.Add(li);
            }
            if (0 == ul.Controls.Count)
            {
                li = new HtmlGenericControl("li");
                li.Attributes.Add("class", "list-group-item");
                li.Controls.Add(new LiteralControl(Ressource.GetString("Msg_TradesNoneSelected")));
                ul.Controls.Add(li);
            }
            this.Page.Controls.Add(ul);
        }
        catch (Exception)
        {
            li = new HtmlGenericControl("li");
            li.Attributes.Add("class", "list-group-item ERROR");
            li.Controls.Add(new LiteralControl("Error"));
            ul.Controls.Add(li);
            this.Page.Controls.Add(ul);
        }
        finally
        {
            if (null != dr) {dr.Dispose();}
            if (null != dbTransaction)
                DataHelper.RollbackTran(dbTransaction);
        }
    }
    /// <summary>
    /// Retourne la date d'une ligne du Tracker (DTUPD ou DTINS)
    /// </summary>
    protected Label GetLabelDate(TrackerDataReader pTrackerDR)
    {
        Label lbl = new Label();
        lbl.CssClass = "time";
        DateTime dtTracker = Convert.ToDateTime(pTrackerDR.dr[pTrackerDR.isRowUpdated ? "DTUPD" : "DTINS"]);
        lbl.Text = new DtFunc().GetDateTimeString(dtTracker.ToString(), DtFunc.FmtISOTime);
        return lbl;
    }
    /// <summary>
    /// Retourne la date d'une ligne du Tracker (DTUPD ou DTINS)
    /// </summary>
    protected Label GetLinkMessage(TrackerDataReader pTrackerDR)
    {
        Label lbl = new Label();
        lbl.CssClass = "msg";
        lbl.Text = pTrackerDR.dr["SHORTMESSAGETRACKER"].ToString();
        string tooltip = JavaScript.HTMLStringCrLf(pTrackerDR.dr["MESSAGETRACKER"].ToString());
        lbl.Attributes.Add("args", "TRACKER_L;" + pTrackerDR.dr["IDTRK_L"].ToString() + ";");
        lbl.Attributes.Add("onclick", "TrackerLink(this);");
        return lbl;
    }

    /// <summary>
    /// 
    /// </summary>
    protected Label GetLinkIdData(TrackerDataReader pTrackerDR, string pResDetail)
    {
        Label lbl = new Label();
        lbl.CssClass = "data";
        string idDataIdentifier = string.Empty;
        if (false == Convert.IsDBNull(pTrackerDR.dr["IDDATAIDENTIFIER"]))
            idDataIdentifier = pTrackerDR.dr["IDDATAIDENTIFIER"].ToString();

        Cst.GroupTrackerEnum group = (Cst.GroupTrackerEnum)Enum.Parse(typeof(Cst.GroupTrackerEnum), pTrackerDR.dr["GROUPTRACKER"].ToString(), true);

        string idData = string.Empty;
        if (false == Convert.IsDBNull(pTrackerDR.dr["IDDATA"]))
        {
            idData = pTrackerDR.dr["IDDATA"].ToString();
            lbl.Text = pResDetail;
        }

        if (StrFunc.IsFilled(idDataIdentifier))
            lbl.Text = StrFunc.ReplaceNoBreakSpaceByWhiteSpace(idDataIdentifier);

        if (StrFunc.IsFilled(idData))
        {
            string idDataIdent = pTrackerDR.dr["IDDATAIDENT"].ToString();
            string data1 = Convert.IsDBNull(pTrackerDR.dr["DATA1"]) ? string.Empty : pTrackerDR.dr["DATA1"].ToString();
            // EG 20151102 [21465] IDTRK_L est passé à la place de IDPR
            // pour palier au cas où le tracker regroupe plusieur IDPR
            if ("POSREQUEST" == idDataIdent)
                idData = pTrackerDR.dr["IDTRK_L"].ToString();
            lbl.Attributes.Add("args", idDataIdent + ";" + idData + ";" + data1);
            lbl.Attributes.Add("onclick", "TrackerLink(this);");
        }
        return lbl;
    }
    #endregion Methods

}