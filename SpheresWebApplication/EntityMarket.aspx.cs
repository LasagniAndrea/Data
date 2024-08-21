using System;
using System.Data;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;

using EFS.ApplicationBlocks.Data;
using EfsML.DynamicData;
using System.Timers;

public partial class EntityMarket : PageBase
{
    bool isDisplayEntityMarket;

    protected void Page_Load(object sender, EventArgs e)
    {
        PageTools.SetHead(this, "EntityMarket", null, null);
        if (isDisplayEntityMarket)
            LoadEntityMarket();
    }

    /// <summary>
    /// 
    /// </summary>
    protected Boolean IsAllowScroll
    {
        get
        {
            Boolean ret = true;
            if (StrFunc.IsFilled(Request.QueryString["isAllowScroll"]))
                ret = BoolFunc.IsTrue(Request.QueryString["isAllowScroll"]);
            return ret;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    protected Boolean IsAllowTimer
    {
        get
        {
            Boolean ret = true;
            if (StrFunc.IsFilled(Request.QueryString["isAllowTimer"]))
                ret = BoolFunc.IsTrue(Request.QueryString["isAllowTimer"]);
            return ret;
        }
    }


    // EG 20210212 [25661] New Appel Protection CSRF(Cross-Site Request Forgery)
    // EG 20210222 [XXXXX] Suppression inscription function JavascriptCookieFunctions
    override protected void OnInit(EventArgs e)
    {
        this.AbortRessource = true;
        base.OnInit(e);

        Form = idEntityMarket;
        AntiForgeryControl();

        //JavaScript.JavascriptCookieFunctions(this);
        isDisplayEntityMarket = BoolFunc.IsTrue(SystemSettings.GetAppSettings("EntityMarket_Banner"));

        if ((false == IsPostBack) && isDisplayEntityMarket)
        {
            // FI 20200618 RefreshInterval est exprimé en secondes 
            int time = (int)SystemSettings.GetAppSettings("EntityMarket_RefreshInterval", typeof(Int32), 0) * 1000;
            timerEntityMarket.Enabled = IsAllowTimer && (time > 0)  ;
            if (timerEntityMarket.Enabled)
                timerEntityMarket.Interval = time;

        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// FI 20120803 Add test sur SessionTools.User.Entity_IdA>0
    /// Lorsque le user n'est pas rattaché à une ENTITY, cela génèrait des exceptions
    /// FI 20200618 [XXXXX] Gestion ou il existe une seule date business 
    // EG 20200930 [XXXXX] Nouvelle interface GUI v10 (Mode Noir ou blanc) Correction
    // EG 20220112 [XXXXX] Modifications mineures liées aux CSS
    private void LoadEntityMarket()
    {
        if (SessionTools.IsConnected && !Software.IsSoftwarePortal())
        {
            string CS = SessionTools.CS;
            DbSvrType dbSvrType = DataHelper.GetDbSvrType(CS);
            if ((DbSvrType.dbUNKNOWN != dbSvrType) && (SessionTools.IsConnected) && SessionTools.User.Entity_IdA > 0)
            {
                DataParameters dp = new DataParameters();
                dp.Add(DataParameter.GetParameter(CS, DataParameter.ParameterEnum.IDA), SessionTools.User.Entity_IdA);

                //PM 20150422 [20575] Add DTENTITY
                string SQLSelect = @"select em.IDEM, em.IDM, mk.IDENTIFIER, mk.SHORT_ACRONYM, ac.IDA, em.DTMARKET, em.DTENTITY, 
                    ac.IDA as IDA_CSSCUSTODIAN, ac.IDENTIFIER as CSSCUSTODIAN, 
                    case when em.IDA_CUSTODIAN is null then 0 else 1 end as ISCUSTODIAN
                    from dbo.ENTITYMARKET em 
                    inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = em.IDM)
                    inner join dbo.ACTOR ac on (ac.IDA = isnull(em.IDA_CUSTODIAN, mk.IDA))
                    where (em.IDA = @IDA) 
                    order by em.DTENTITY" + Cst.CrLf;

                QueryParameters queryParameters = new QueryParameters(CS, SQLSelect, dp);
                using (DataTable dt = DataHelper.ExecuteDataTable(CS, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
                {
                    List<DataRow> rows = dt.Rows.Cast<DataRow>().ToList();
                    List<DateTime> distinctDtEntity = (from item in rows
                                                       select Convert.ToDateTime(item["DTENTITY"])).Distinct().ToList();

                    HtmlGenericControl ul = new HtmlGenericControl("ul")
                    {
                        ID = "entityMarket"
                    };
                    plhEntityMarket.Controls.Add(ul);

                    if (distinctDtEntity.Count == 1)
                    {
                        DateTime dtEntity = Convert.ToDateTime(rows.First()["DTENTITY"]);
                        List<DataRow> rowsMarketclosed = rows.FindAll(x => Convert.ToDateTime(x["DTMARKET"]) != dtEntity);

                        Boolean isScroll = (rowsMarketclosed.Count > 1);
                        SetScroll(isScroll);
                        if (isScroll)
                            ul.Attributes.Add("class", "newsticker"); // Normalement non nécessaire => permet d'avoir un rendu identique au mode scroll lorsque IsAllowScroll==false (Intéressant en debug)
                        else
                            ul.Attributes.Add("class", "allnewsticker");

                        // Image 
                        HtmlGenericControl li = new HtmlGenericControl("li");
                        Panel pnl = new Panel
                        {
                            CssClass = "fa-icon fas fa-map-marker-alt gray"
                        };
                        li.Controls.Add(pnl);
                        
                        // Date Entity
                        string prefixID = "all";
                        Label lblDate = new Label()
                        {
                            ID = $"{prefixID}{"-date"}",
                            Text = DtFunc.DateTimeToString(dtEntity, DtFunc.FmtShortDate),
                        };
                        li.Controls.Add(lblDate);

                        // AllMarkets Anchor
                        HtmlAnchor anchor = new HtmlAnchor()
                        {
                            ID = $"{prefixID}{"-allMarkets"}",
                            InnerText = Ressource.GetString("AllMarkets").ToUpper(),
                            HRef = SpheresURL.GetURL(IdMenu.Menu.EntityMarket, null, SessionTools.User.EntityIdentification.OtcmlId, SpheresURL.LinkEvent.href,
                               Cst.ConsultationMode.ReadOnly, null, null),
                            Target = "blank"
                        };
                        li.Controls.Add(anchor);

                        // closed Market
                        var lstMarket = (from item in rowsMarketclosed
                                         select new
                                         {
                                             IDM = Convert.ToInt32(item["IDM"]),
                                             SHORT_ACRONYM = item["SHORT_ACRONYM"].ToString()
                                         }).Distinct();

                        foreach (var marketItem in lstMarket)
                        {
                            Label lblMarket = new Label()
                            {
                                ID = $"{prefixID}{"-market-"}{"mk-"}{marketItem.IDM}{"-closed"}",
                                Text = marketItem.SHORT_ACRONYM
                            };
                            
                            string lstCssCustodian =ArrFunc.GetStringList((from item in rowsMarketclosed.Where(x => Convert.ToInt32(x["IDM"]) == marketItem.IDM)
                                                                           select item["CSSCUSTODIAN"].ToString()).ToArray());
                            Label lblCssCustodian = new Label()
                            {
                                ID = $"{prefixID}{"-csscustodian-"}{"mk-"}{marketItem.IDM}{"csscustodian-"}{"lstCsscustodian"}{"-closed"}",
                                Text = $"({lstCssCustodian})"
                            };

                            Label lblClosed = new Label()
                            {
                                ID = $"{prefixID}{"-closed-"}{"mk-"}{marketItem.IDM}{"-closed"}",
                                Text = "[Closed]"
                            };
                            anchor.Controls.Add(lblMarket);
                            anchor.Controls.Add(lblCssCustodian);
                            anchor.Controls.Add(lblClosed);
                        }
                        ul.Controls.Add(li);
                    }
                    else
                    {
                        ul.Attributes.Add("class", "newsticker"); // Normalement non nécessaire => permet d'avoir un rendu identique au mode scroll lorsque IsAllowScroll==false (Intéressant en debug)

                        Boolean isScroll = true;
                        SetScroll(isScroll);

                        foreach (DataRow row in rows)
                        {
                            Boolean isMarketClosed = Convert.ToDateTime(row["DTMARKET"]) != Convert.ToDateTime(row["DTENTITY"]);

                            HtmlGenericControl li = new HtmlGenericControl("li");

                            string prefixID = "css";
                            string cssClass = "gray";
                            if (-1 == Convert.ToInt32(row["IDM"]))
                            {
                                prefixID = "otc";
                                cssClass = "rose";
                            }
                            else if (Convert.ToBoolean(row["ISCUSTODIAN"]))
                            {
                                prefixID = "custodian";
                                cssClass = "blue";
                            }
                            
                            // Image
                            Panel pnl = new Panel()
                            {
                                CssClass = $"{"fa-icon fas fa-map-marker-alt "}{cssClass}"
                            };
                            li.Controls.Add(pnl);

                            // date Entity
                            Label lblDate = new Label()
                            {
                                ID = $"{prefixID}{"-date-"}{"mk-"}{row["IDM"]}{"csscustodian-"}{row["IDA_CSSCUSTODIAN"]}",
                                Text = DtFunc.DateTimeToString(Convert.ToDateTime(row["DTENTITY"]), DtFunc.FmtShortDate)
                            };
                            li.Controls.Add(lblDate);

                            // Market Anchor
                            HtmlAnchor anchor = new HtmlAnchor()
                            {
                                ID = $"{prefixID}{"-market-"}{"mk-"}{row["IDM"]}{"csscustodian-"}{row["IDA_CSSCUSTODIAN"]}",
                                InnerText = row["SHORT_ACRONYM"].ToString(),
                                HRef = SpheresURL.GetURL(IdMenu.Menu.EntityMarket, null, SessionTools.User.EntityIdentification.OtcmlId, SpheresURL.LinkEvent.href,
                                Cst.ConsultationMode.ReadOnly, null, null),
                                Target = "blank"
                            };
                            if (isMarketClosed)
                                anchor.ID = $"{anchor.ID}-closed";
                            li.Controls.Add(anchor);

                            // cssCustodian (child of Market Anchor)
                            Label lblCssCustodian = new Label()
                            {
                                ID = $"{prefixID}{"-csscustodian-"}{"mk-"}{row["IDM"]}{"csscustodian-"}{row["IDA_CSSCUSTODIAN"]}",
                                Text = $"({row["CSSCUSTODIAN"]})"
                            };
                            if (isMarketClosed)
                                lblCssCustodian.ID = $"{lblCssCustodian.ID}-closed";
                            anchor.Controls.Add(lblCssCustodian);
                            
                            // closed
                            if (isMarketClosed)
                            {
                                Label lblClosed = new Label()
                                {
                                    ID = $"{prefixID}{"-closed-"}{"mk-"}{row["IDM"]}{"csscustodian-"}{row["IDA_CSSCUSTODIAN"]}{"-closed"}",
                                    Text = "[Closed]"
                                };
                                anchor.Controls.Add(lblClosed);
                            }
                            ul.Controls.Add(li);
                        }
                    }
                }
            }
        }
    }
    #region protected override CreateChildControls
    // EG 20210224 [XXXXX] suppresion PageBase.js déja appelé dans Render de PageBase
    protected override void CreateChildControls()
    {
        // Override complet de PageBase (pas d'appel)
        JQuery.WriteEngineScript(this, JQuery.Engines.JQuery);
        JQuery.UI.WritePluginScript(this, JQuery.Engines.JQuery);
        // FI 20200618[XXXXX] call ScrollEntityMarket
        JQuery.WriteInitialisationScripts(this, "ScrollEntityMarket", "ScrollEntityMarket();");
        //ScriptManager.Scripts.Add(new ScriptReference("~/Javascript/PageBase.js"));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pIsScroll"></param>
    private void SetScroll(Boolean pIsScroll)
    {
        if (pIsScroll)
            __ISSCROLL.Value = IsAllowScroll ? "true" : "false";
        else
            __ISSCROLL.Value = "false";
    }

    #endregion CreateChildControls (Scripts JavaScript)
}
