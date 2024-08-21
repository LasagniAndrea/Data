using System;
using System.Web;
using System.Data;
using System.Drawing;
using System.Threading;
using EFS.ACommon;
using EFS.Common;
using EFS.Common.Web;
using EFS.ApplicationBlocks.Data;
using EfsML.Business;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;

namespace EFS.Spheres
{
    // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
    public partial class Default : PageBase
    {
        bool isDisplayEntityMarket;
        protected HtmlInputHidden CtrlMaskMenu { get { return hidMaskMenu; } }
        protected HtmlInputHidden CtrlIdA { get { return hidIDA; } }

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

        protected int EntityMarketInterval
        {
            get
            {
                return (int)SystemSettings.GetAppSettings("EntityMarket_RefreshInterval", typeof(Int32), 0) * 1000;
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

        protected override void OnInit(System.EventArgs e)
        {
            string eventArgument = Request.Params["__EVENTARGUMENT"];
            if (StrFunc.IsFilled(eventArgument) && eventArgument.Contains("DISCONNECT"))
                Disconnect.Run(this, SessionTools.IsShibbolethAuthenticationType ? "FedAuthLogout.aspx" : null);

            Form = this.Master.FindControl("frmMainMaster") as System.Web.UI.HtmlControls.HtmlForm;
            Form.Attributes.Add("class", CSSMode + " " + SessionTools.Company_CssColor);
            base.OnInit(e);
            AddInputHiddenAutoPostback();

            isDisplayEntityMarket = BoolFunc.IsTrue(SystemSettings.GetAppSettings("EntityMarket_Banner"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            
            hidMaskGroup.Attributes["title"] = Ressource.GetString("imgBtnMaskGroup");
            hidMaskMenu.Attributes["title"] = Ressource.GetString("imgBtnMaskMenu");
            hidHisto.Value = SessionTools.TrackerHistoric;

            SetInfo();
            SetConnectionString();
            SetUser();
            SetMainMenu();

            if (Software.IsSoftwarePortal())
            {
                hidIsPortal.Value = "true";
            }
            else
            {
                btnDetail.Attributes.Add("onclick", GetURLTrackerDetail());
                btnDetail.Attributes["title"] = Ressource.GetString("Zoom");
                btnRefresh.Attributes["title"] = Ressource.GetString("Refresh");
                btnMonitoring.Attributes["title"] = Ressource.GetString("OnBoardMonitoring");
                btnObserver.Attributes["title"] = Ressource.GetString("ServicesObserver");
                // EG 20240523 [XXXXX] Tracker : Correction Tooltip manquant sur bouton(s) et Gestion icon sur Favorites/Groups
                btnParam.Attributes["title"] = Ressource.GetString("Tools");

                SetEntityMarket();
                MaskGroup();

                btnAutoRefresh.Disabled = (SessionTools.TrackerRefreshInterval == 0);
                btnAutoRefresh.Attributes.Add("class", "fa-icon " + (SessionTools.IsTrackerRefreshActive ? "stop" : "start"));
                btnAutoRefresh.Title = Ressource.GetString(btnAutoRefresh.Disabled ? "ActiveRefresh" : "StartRefresh");
                btnAutoRefresh.Attributes["onclick"] = $"_SetTrackerAutoRefresh(true); return false;";
            }

        }

        protected void OnRefresh(object sender, EventArgs e)
        {
        }

        /// EG 20221029 [XXXXX] Refactoring
        protected override void CreateChildControls()
        {
            base.CreateChildControls();
        }
        private void MaskGroup()
        {
            if (!IsPostBack)
            {
                AspTools.ReadSQLCookie("MaskGroup", out string cookieValue);
                if (StrFunc.IsFilled(cookieValue))
                    hidMaskGroup.Value = cookieValue;
                else
                {
                    hidMaskGroup.Value = "all";
                    AspTools.WriteSQLCookie("MaskGroup", hidMaskGroup.Value);
                }
            }
            else
            {
                AspTools.WriteSQLCookie("MaskGroup", hidMaskGroup.Value);
            }
        }


        private void SetInfo()
        {
            if (StrFunc.IsFilled(SessionTools.Collaborator_DISPLAYNAME))
            {
                hidIsConnected.Value = "yes";
                if (!IsPostBack)
                    hidIDA.Value = SessionTools.Collaborator_IDA.ToString();
            }
            else
            {
                hidIsConnected.Value = "no";
            }

            #region Display Entity
            string parent_Identifier = SessionTools.Collaborator_ENTITY_IDENTIFIER;
            string parent_DisplayName = SessionTools.Collaborator_ENTITY_DISPLAYNAME;
            string parent_Description = SessionTools.Collaborator_ENTITY_DESCRIPTION;
            bool parent_IsEntity = SessionTools.Collaborator_ENTITY_ISENTITY;
            string displayNameCssClass = parent_IsEntity ? "entitydsn" : "supportdsn";
            string descriptionCssClass = parent_IsEntity ? "entitydesc" : "supportdesc";
            string guiCssClass = "labelGUITitle";

            if (SessionTools.IsConnected)
            {
                if (StrFunc.IsEmpty(parent_Identifier))
                {
                    //User non rattaché à une entité (ou à un support)
                    parent_DisplayName = Ressource.GetString2("Msg_Warning", true, string.Empty);
                    parent_Description = Ressource.GetString("Msg_UserWithoutEntity", true);
                    displayNameCssClass = "labelErrorDisplayname";
                    descriptionCssClass = "labelErrorDescription";
                    guiCssClass = "labelErrorGUITitle";
                }
                else
                {
                    if (StrFunc.IsEmpty(parent_DisplayName))
                        parent_DisplayName = parent_Identifier;
                    if (StrFunc.IsEmpty(parent_Description) || (parent_DisplayName == parent_Description))
                        parent_Description = string.Empty;
                    else if (parent_Description.StartsWith(parent_DisplayName))
                        parent_DisplayName = parent_Identifier;
                    else
                    {
                        //parent_Description = " - " + parent_Description;
                        parent_Description = Cst.HTMLSpace4 + parent_Description;
                    }
                }

                bool isUserWithLimitedRights = SessionTools.IsUserWithLimitedRights();
                if (isUserWithLimitedRights && (!SessionTools.IsSessionGuest) && (SessionTools.IsDataArchive))
                    _ = false;
            }

            lblCompanyDisplayname.CssClass = displayNameCssClass;
            lblCompanyDisplayname.Text = parent_DisplayName;
            lblCompanyDisplayname.Font.Bold = true;
            lblCompanyDescription.CssClass = descriptionCssClass;
            lblCompanyDescription.Text = parent_Description;
            lblCompanyDescription.Font.Bold = false;



            lblGUITitle.Text = string.Empty;
            lblGUITitle.Font.Bold = false;
            lblGUITitle.CssClass = guiCssClass;
            //
            string source;
            if (SessionTools.IsConnected)
                source = SessionTools.CS;
            else
                source = SystemSettings.GetLogConnectionString(false, System.Web.HttpContext.Current.Request.ApplicationPath);

            // FI 20140120 [] add test if (StrFunc.IsFilled (source))
            DbSvrType dbSvrType = DbSvrType.dbUNKNOWN;
            if (StrFunc.IsFilled(source))
                dbSvrType = DataHelper.GetDbSvrType(source);

            if (DbSvrType.dbUNKNOWN != dbSvrType)
            {
                string SQLSelect = SQLCst.SELECT + DataHelper.SQLIsNullChar(source, "GUITITLE", string.Empty, "GUITITLE");
                SQLSelect += "," + DataHelper.SQLIsNullChar(source, "GUITITLEFORECOLOR", string.Empty, "GUITITLEFORECOLOR");
                SQLSelect += "," + DataHelper.SQLIsNull(source, "GUITITLEFONTSIZE", "0", "GUITITLEFONTSIZE");
                SQLSelect += "," + DataHelper.SQLIsNullChar(source, "GUIMESSAGE", string.Empty, "GUIMESSAGE") + Cst.CrLf;
                SQLSelect += SQLCst.FROM_DBO + Cst.OTCml_TBL.LICENSEE.ToString();
                try
                {
                    using (IDataReader dr = DataHelper.ExecuteDataTable(source, SQLSelect).CreateDataReader())
                    {
                        if (dr.Read())
                        {
                            try
                            {
                                if (!Software.IsSoftwarePortal())
                                {
                                    lblArchive.Visible = SessionTools.IsDataArchive;
                                    if (lblArchive.Visible)
                                    {
                                        lblArchive.Text = "ARCHIVE " + SessionTools.DtArchive.ToString("d") + Cst.HTMLSpace4;
                                        lblArchive.Font.Bold = true;
                                    }

                                    lblGUITitle.Text = Translate(dr["GUITITLE"].ToString());
                                    lblGUITitle.Font.Bold = false;
                                    if (dr["GUITITLEFORECOLOR"].ToString().Length > 0)
                                        lblGUITitle.ForeColor = Color.FromName(dr[1].ToString());
                                    if (Convert.ToInt32(dr["GUITITLEFONTSIZE"]) > 0)
                                    {
                                        int size = Convert.ToInt32(dr["GUITITLEFONTSIZE"]);
                                        lblGUITitle.Font.Size = size;
                                    }
                                }
                            }
                            catch { }
                            try
                            {
                                if (SessionTools.IsConnected && (dr["GUIMESSAGE"].ToString().Length > 0) || SessionTools.IsDataArchive)
                                {
                                    // EG 20130925 HTMLBreakLine
                                    string msg = SessionTools.IsDataArchive ? "ARCHIVE Environment - Date: " + SessionTools.DtArchive.ToString("d") + Cst.HTMLBreakLine2 : string.Empty;                                    
                                    msg += Translate(dr["GUIMESSAGE"].ToString());
                                    // AL 20240607 [WI955] Impersonate mode
                                    if (StrFunc.IsFilled(SessionTools.Collaborator.ImpersonateIdentifier))
                                        msg += Cst.HTMLBreakLine2 + "Impersonated User: " + SessionTools.Collaborator.ImpersonateIdentifier;
                                    JQuery.OpenDialog openDialog = new JQuery.OpenDialog(lblGUITitle.Text, msg.Replace(Cst.CrLf, Cst.HTMLBreakLine), ProcessStateTools.StatusNoneEnum);
                                    JQuery.UI.WriteInitialisationScripts(this, openDialog);
                                }
                            }
                            catch { }
                        }
                    }
                    lblGUITitle.Visible = SessionTools.IsConnected;

                    this.DataBind();
                }
                catch (Exception)
                {
                    throw;
                }
            }

            #endregion
        }

        private void SetConnectionString()
        {
            if (SessionTools.IsConnected)
            {
                bool isUserWithLimitedRights = SessionTools.IsUserWithLimitedRights();
                if (isUserWithLimitedRights && (!SessionTools.IsSessionGuest) && (SessionTools.IsDataArchive))
                    isUserWithLimitedRights = false;

                if (isUserWithLimitedRights || BoolFunc.IsFalse(SystemSettings.GetAppSettings("ConnectionString_Banner", "true")))
                {
                    lblConnectionString.Visible = false;
                }
                else
                {
                    string servername = SessionTools.Data.Server;

                    // AL 20240705 [WI996] the following code is no more usefull with the changes in CSManager.GetSvrName(). Servername is already correct.
                    //if ((SessionTools.Data.RDBMS.IndexOf("SQLSRV") >= 0) && (servername == "."))
                    //{
                    //    //SQLServer: Serveur local
                    //    //servername = "(Local)";
                    //}
                    //else if ((SessionTools.Data.RDBMS.IndexOf("ORACL") >= 0) && (servername.IndexOf("SERVICE_NAME=") >= 0))
                    //{
                    //    //Oracle: CS au format EZConnect, on considère uniquement SERVICE_NAME
                    //    servername = servername.Substring(servername.IndexOf("SERVICE_NAME=") + 13);
                    //    if (servername.IndexOf(".") >= 0)
                    //        servername = servername.Substring(0, servername.IndexOf("."));
                    //    if (servername.IndexOf(")") >= 0)
                    //        servername = servername.Substring(0, servername.IndexOf(")"));
                    //}
                    lblConnectionString.Text = SessionTools.Data.RDBMS + @": \\" + servername + @"\" + SessionTools.Data.Database;
                    lblConnectionString.Style.Add(HtmlTextWriterStyle.Cursor, "pointer");
                    lblConnectionString.Pty.TooltipContent = Ressource.GetString("lblDatabase") + " " + Cst.HTMLBold + SessionTools.Data.DatabaseNameVersionBuild + Cst.HTMLEndBold;
                    lblConnectionString.Pty.TooltipStyle = "qtip-def";
                }
            }
        }
        // EG 20220920 [XXXX][WIXXX] Ajout d'un bouton de déconnexion 
        private void SetUser()
        {
            btnUserName.Visible = SessionTools.IsConnected;
            if (SessionTools.IsConnected)
            {
                string cssClass = "fa-icon {0}";
                if (SessionTools.IsSessionSysAdmin)
                    cssClass = String.Format(cssClass, "red");
                else if (SessionTools.IsSessionSysOper)
                    cssClass = String.Format(cssClass, "blue");
                else if (SessionTools.IsSessionGuest)
                    cssClass = String.Format(cssClass, "blue");
                else
                    cssClass = String.Format(cssClass, "gray");

                imgstart.Attributes["class"] = cssClass + " fas fa-user";
                imgstart.InnerText = " (" + Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName + ")";

                btnUserName.Text = SessionTools.Collaborator.Identifier;                

                // EG [XXXXX] Affichage de l'identité Shibboleth en tooltip
                if (SessionTools.IsShibbolethAuthenticationType)
                    btnUserName.ToolTip = HttpContext.Current.User.Identity.Name;
                btnUserName.Attributes["href"] = "#";

                string collaborator_ROLE = (string)SessionTools.Collaborator_ROLE;
                lblLastLogin.Text = SessionTools.Collaborator_DISPLAYNAME;
                // AL 20240530 Update for Impersonation mode
                if (StrFunc.IsFilled(SessionTools.Collaborator.ImpersonateIdentifier))
                    lblLastLogin.Text += "=>" + SessionTools.Collaborator.ImpersonateDisplayName;
                
                if (collaborator_ROLE != "USER")
                    lblLastLogin.Text += Cst.HTMLBreakLine + Cst.HTMLBold + Ressource.GetString(collaborator_ROLE) + Cst.HTMLEndBold;

                // LP 20240531 [WI949] Show the PARENT actor and the role / PL 20240724 Refactoring
                // Show only Department and entity with priority to department
                // EG 20240607 [WI949] Refactoring pour gestion CSS de l'acteur PARENT
                string parentInfo = string.Empty;
                if (SessionTools.Collaborator.Department.IsLoaded)
                {
                    parentInfo = SessionTools.Collaborator.Department.DisplayName
                               + " (" + StrFunc.FirstUpperCase(SessionTools.Collaborator.Department.Role.ToString().ToLower()) + ")";
                    lblLastLogin.CssClass = "parent-dept"; 
                }
                else if (SessionTools.Collaborator.Entity.IsLoaded)
                {
                    parentInfo = SessionTools.Collaborator.Entity.DisplayName
                               + " (" + StrFunc.FirstUpperCase(SessionTools.Collaborator.Entity.Role.ToString().ToLower()) + ")";
                    lblLastLogin.CssClass = "parent-entity";
                }
                if (!string.IsNullOrEmpty(parentInfo))
                {
                    lblLastLogin.Text += Cst.HTMLBreakLine + $"<i>{parentInfo}</i>";
                }

                if (DtFunc.IsDateTimeFilled(SessionTools.DTLASTLOGIN))
                {
                    // EG 20210406 [25556] Libellé dernière connexion
                    lblLastLogin.Text += Cst.HTMLBreakLine + Ressource.GetString("DTLASTLOGIN") + Cst.HTMLBreakLine + Cst.HTMLBold + DtFuncExtended.DisplayTimestampUTC(SessionTools.DTLASTLOGIN,
                        new AuditTimestampInfo()
                        {
                            TimestampZone = SessionTools.AuditTimestampZone,
                            Collaborator = SessionTools.Collaborator,
                            Precision = Cst.AuditTimestampPrecision.Second
                        }) + Cst.HTMLEndBold;
                }
                imgend.Attributes["class"] = " fa fa-caret-down";
                imgDisconnect.Attributes["class"] = "poweroff fa fa-power-off";
            }
        }
        // EG 20220920 [XXXX][WIXXX] Ajout d'un bouton de déconnexion 
        // EG 20221010 [XXXXX] Changement de nom de la page principale : mainDefault en default
        private void SetMainMenu()
        {
            lnkHome.Text = String.Format("<i class='fa fa-home'></i> {0}", Ressource.GetString("GUIInfos"));
            lnkHome.Attributes.Add("onclick", "document.getElementById('main').src='Welcome.aspx';return false;");
            lnkHome.Attributes["href"] = "#";

            lnkProfile.Text = String.Format("<i class='fa fa-cog'></i> {0}", Ressource.GetString("imgBtnCulture"));
            lnkProfile.Attributes.Add("onclick", "document.getElementById('main').src='Profile.aspx?default=1';return false;");
            lnkProfile.Attributes["href"] = "#";

            lnkHelp.Text = String.Format("<i class='fa fa-info-circle'></i> {0}", Ressource.GetString("imgBtnHelp"));
            lnkHelp.Attributes.Add("onclick", "window.open('/ApplicationHelp.htm','Help');return false;");
            lnkHelp.Attributes["href"] = "#";

            lnkDisconnect.Text = String.Format("<i class='fa fa-power-off'></i> {0}", Ressource.GetString("btnLogout"));
            lnkDisconnect.Attributes["href"] = "#";

            if (SessionTools.IsShibbolethAuthenticationType)
            {
                lnkDisconnect.Attributes.Add("onclick", "document.getElementById('main').src='FedAuthLogout.aspx';return false;");
                imgDisconnect.Attributes.Add("onclick", "document.getElementById('main').src='FedAuthLogout.aspx';return false;");
            }
            else
            {
                lnkDisconnect.Attributes.Add("onclick", "javascript:" + Page.ClientScript.GetPostBackEventReference(lnkDisconnect, "DISCONNECT"));
                imgDisconnect.Attributes.Add("onclick", "javascript:" + Page.ClientScript.GetPostBackEventReference(imgDisconnect, "DISCONNECT"));
            }
        }
        private void SetEntityMarket()
        {
            if (isDisplayEntityMarket && SessionTools.IsConnected && !Software.IsSoftwarePortal())
            {
                
                DbSvrType dbSvrType = DataHelper.GetDbSvrType(SessionTools.CS);
                if ((DbSvrType.dbUNKNOWN != dbSvrType) && (SessionTools.IsConnected) && SessionTools.User.Entity_IdA > 0)
                {
                    DataParameters dp = new DataParameters();
                    dp.Add(DataParameter.GetParameter(SessionTools.CS, DataParameter.ParameterEnum.IDA), SessionTools.User.Entity_IdA);

                    //PM 20150422 [20575] Add DTENTITY
                    string SQLSelect = @"select em.IDEM, em.IDM, mk.IDENTIFIER, mk.SHORT_ACRONYM, ac.IDA, em.DTMARKET, em.DTENTITY, 
                    ac.IDA as IDA_CSSCUSTODIAN, ac.IDENTIFIER as CSSCUSTODIAN, 
                    case when em.IDA_CUSTODIAN is null then 0 else 1 end as ISCUSTODIAN
                    from dbo.ENTITYMARKET em 
                    inner join dbo.VW_MARKET_IDENTIFIER mk on (mk.IDM = em.IDM)
                    inner join dbo.ACTOR ac on (ac.IDA = isnull(em.IDA_CUSTODIAN, mk.IDA))
                    where (em.IDA = @IDA) 
                    order by em.DTENTITY" + Cst.CrLf;

                    QueryParameters queryParameters = new QueryParameters(SessionTools.CS, SQLSelect, dp);
                    using (DataTable dt = DataHelper.ExecuteDataTable(SessionTools.CS, queryParameters.Query, queryParameters.Parameters.GetArrayDbParameter()))
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

                                string lstCssCustodian = ArrFunc.GetStringList((from item in rowsMarketclosed.Where(x => Convert.ToInt32(x["IDM"]) == marketItem.IDM)
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


        private string Translate(string pData)
        {
            pData = pData.Replace("%%RDBMS%%", SessionTools.Data.RDBMS);
            pData = pData.Replace("%%SERVER%%", SessionTools.Data.Server);
            pData = pData.Replace("%%SID%%", SessionTools.Data.Server);
            pData = pData.Replace("%%DATABASE%%", SessionTools.Data.Database);
            pData = pData.Replace("%%SCHEMA%%", SessionTools.Data.Database);
            //
            pData = pData.Replace("%%USER_IDENTIFIER%%", SessionTools.Collaborator_IDENTIFIER);
            pData = pData.Replace("%%USER_DISPLAYNAME%%", SessionTools.Collaborator_DISPLAYNAME);
            pData = pData.Replace("%%USERPARENT_IDENTIFIER%%", SessionTools.Collaborator_ENTITY_IDENTIFIER);
            pData = pData.Replace("%%USERPARENT_DISPLAYNAME%%", SessionTools.Collaborator_ENTITY_DISPLAYNAME);
            //
            pData = Ressource.GetString(pData, true);
            return pData;
        }

        private void SetScroll(Boolean pIsScroll)
        {
            if (pIsScroll)
                __ISSCROLL.Value = IsAllowScroll ? "true" : "false";
            else
                __ISSCROLL.Value = "false";
        }

        private string GetURLTrackerDetail()
        {
            string dateTracker = "null";
            string title = string.Empty;

            string histo = hidHisto.Value;
            Nullable<DateTime> dtReference = null;
            if (StrFunc.IsFilled(histo) && ("Beyond" != histo))
                dtReference = new DtFunc().StringToDateTime("-" + histo);

            if (dtReference.HasValue)
                dateTracker = "'" + DtFuncML.DateTimeToStringDateISO(dtReference.Value) + "'";


            string url = $"TrackerDetail('false',null,null,null,null,{dateTracker},'{title}');return false;";
            return url;
        }

        protected void BtnLogout_Click(object sender, System.EventArgs e)
        {
            Disconnect.Run(this, SessionTools.IsShibbolethAuthenticationType ? "FedAuthLogout.aspx" : null);
        }

        private void BtnQuit_Click(object sender, System.EventArgs e)
        {
            Disconnect.Run(this, SessionTools.IsShibbolethAuthenticationType ? "FedAuthLogout.aspx" : null);
        }

    }
}